module Program

open System.Collections.Generic
open Foundatio.Storage
open Converter
open System
open System.IO
open Pulumi.Experimental.Converter
open Pulumi.Codegen

let errorResponse (message: string) = 
    let diagnostics = ResizeArray [
        Diagnostic(Summary=message, Severity=DiagnosticSeverity.Error)
    ]
    
    ConvertProgramResponse(Diagnostics=diagnostics)

let logsFile() = Environment.GetEnvironmentVariable "K8S_CONVERTER_LOGS"

let convertProgram (request: ConvertProgramRequest): ConvertProgramResponse =
    let componentMode = request.Args |> Array.exists (fun arg -> arg = "--component-mode")
    
    let inputYamlFiles =
        Directory.EnumerateFiles(request.SourceDirectory, "*.yaml")
        |> Seq.append (Directory.EnumerateFiles(request.SourceDirectory, "*.yml"))
        |> Seq.toList

    if inputYamlFiles.Length = 0 then
        errorResponse "No YAML files found in source directory."
    else
        
        // default mode is to convert the everything to a single Pulumi program
        let resources = ResizeArray<PulumiTypes.Resource>()
        let diagnostics = ResizeArray<Diagnostic>()
        let usedNames = Dictionary<string, int>()
        for yamlFile in inputYamlFiles do
            let  fileName = Path.GetFileName yamlFile
            let content = File.ReadAllText(yamlFile)
            for (i, document) in List.indexed (YamlDocument.parseYamlDocuments content) do
                match YamlDocument.kubeDocument document with
                | Error errorMessage ->
                    let fullError = $"Error parsing YAML document[{i}] from {fileName}: {errorMessage}"
                    diagnostics.Add(Diagnostic(Summary=fullError, Severity=DiagnosticSeverity.Warning))
                | Ok kubeDoc ->
                    let transformedResource = Transform.fromKubeDocument kubeDoc usedNames
                    resources.Add transformedResource

        if not componentMode then
            let targetPulumiFile = Path.Combine(request.TargetDirectory, "main.pp")
            File.WriteAllText(targetPulumiFile, Printer.printProgram {
                nodes = [ for resource in resources -> PulumiTypes.PulumiNode.Resource resource ]
            })
            
            ConvertProgramResponse(Diagnostics=diagnostics)
        else
            // component mode will turn every kubernetes resource into a Pulumi component
            // Emit each resource as a separate component in a separate directory   
            for resource in resources do
                let targetComponentDirectory = Path.Combine(request.TargetDirectory, resource.name)
                ignore (Directory.CreateDirectory(targetComponentDirectory))
                let targetPulumiFile = Path.Combine(targetComponentDirectory, "main.pp")
                File.WriteAllText(targetPulumiFile, Printer.printProgram {
                    nodes = [ PulumiTypes.PulumiNode.Resource resource ]
                })

            // create a main.pp file that imports all the components
            let entryPulumiProgram = Printer.printProgram {
                nodes = [
                    for resource in resources -> PulumiTypes.PulumiNode.Component {
                        name = resource.name
                        path = $"./{resource.name}"
                        options = None
                        logicalName = None
                        inputs = Map.empty 
                    }
                ]
            }
            
            let targetEntryPulumiFile = Path.Combine(request.TargetDirectory, "main.pp")
            File.WriteAllText(targetEntryPulumiFile, entryPulumiProgram)
            ConvertProgramResponse(Diagnostics=diagnostics)
    
let convertProgramWithErrorHandling (request: ConvertProgramRequest): ConvertProgramResponse =
    try
        convertProgram request
    with
    | ex ->
        let logsFile = logsFile()
        if not (String.IsNullOrWhiteSpace logsFile) then
            File.AppendAllLines(Path.Combine(request.SourceDirectory, logsFile), [
                "Exception: " + ex.ToString()
            ])

        let diagnostics = ResizeArray [
            Diagnostic(Summary=ex.Message, Detail=ex.StackTrace, Severity=DiagnosticSeverity.Error)
        ]

        ConvertProgramResponse(Diagnostics=diagnostics)

convertProgramWithErrorHandling
|> Converter.CreateSimple
|> Converter.Serve
|> Async.AwaitTask
|> Async.RunSynchronously