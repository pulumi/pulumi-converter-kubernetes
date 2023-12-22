open System
open System.IO
open System.Linq
open System.Threading.Tasks
open System.Xml
open Fake.IO
open Fake.Core
open CliWrap
open CliWrap.Buffered
open System.Text
open Fastenshtein
open Octokit
open ICSharpCode.SharpZipLib.Tar
open ICSharpCode.SharpZipLib.GZip
open PulumiSchema.Types

/// Recursively tries to find the parent of a file starting from a directory
let rec findParent (directory: string) (fileToFind: string) =
    let path = if Directory.Exists(directory) then directory else Directory.GetParent(directory).FullName
    let files = Directory.GetFiles(path)
    if files.Any(fun file -> Path.GetFileName(file).ToLower() = fileToFind.ToLower())
    then path
    else findParent (DirectoryInfo(path).Parent.FullName) fileToFind

let repositoryRoot = findParent __SOURCE_DIRECTORY__ "README.md";

let pulumiCliBinary() : Task<string> = task {
    try
        // try to get the version of pulumi installed on the system
        let! version =
            Cli.Wrap("pulumi")
                .WithArguments("version")
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .ExecuteAsync()

        return "pulumi"
    with
    | _ ->
        // when pulumi is not installed, try to get the version of of the dev build
        // installed on the system using `make install` in the pulumi repo
        let homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        let pulumiPath = System.IO.Path.Combine(homeDir, ".pulumi-dev", "bin", "pulumi")
        if System.IO.File.Exists pulumiPath then
            return pulumiPath
        elif System.IO.File.Exists $"{pulumiPath}.exe" then
            return $"{pulumiPath}.exe"
        else
            return "pulumi"
}

let schemaFromPulumi(pluginName: string) = task {
    let packageName = $"{pluginName}@2.0.0"
    let! binary = pulumiCliBinary()
    let! output =
         Cli.Wrap(binary)
            .WithArguments($"package get-schema {packageName}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync()

    if output.ExitCode <> 0 then
        return Error output.StandardError
    else
        return Ok output.StandardOutput
}

let parseSchemaFromPulumi(pluginName: string) =
    task {
        let! schema = schemaFromPulumi(pluginName)
        return
            match schema with
            | Ok schema -> Ok (PulumiSchema.Parser.parseSchema schema)
            | Error error -> Error error
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously


let runTests() =
    if Shell.Exec("dotnet", "run", Path.Combine(repositoryRoot, "src", "Tests")) <> 0
    then failwithf "Unit tests failed"

let integrationTests() =
    if Shell.Exec("dotnet", "run", Path.Combine(repositoryRoot, "src", "LocalTester")) <> 0
    then failwithf "Integration tests failed"

let buildSolution() =
    if Shell.Exec("dotnet", "build -c Release", Path.Combine(repositoryRoot, "src")) <> 0
    then failwithf "Build failed"

let converterVersion() =
    let projectFilePath = Path.Combine(repositoryRoot, "src", "PulumiKubernetesConverter", "PulumiKubernetesConverter.fsproj")
    let content = File.ReadAllText projectFilePath
    let doc = XmlDocument()
    use content = new MemoryStream(Encoding.UTF8.GetBytes content)
    doc.Load(content)
    doc.GetElementsByTagName("Version").[0].InnerText

let artifacts = Path.Combine(repositoryRoot, "artifacts")

let createTarGz (source: string) (target: string)  =
    let outStream = File.Create target
    let gzipOutput = new GZipOutputStream(outStream)
    let tarArchive = TarArchive.CreateOutputTarArchive(gzipOutput);
    for file in Directory.GetFiles source do
        let tarEntry = TarEntry.CreateEntryFromFile file
        tarEntry.Name <- Path.GetFileName file
        tarArchive.WriteEntry(tarEntry, false)
    tarArchive.Close()

let createArtifacts() =
    let version = converterVersion()
    let cwd = Path.Combine(repositoryRoot, "src", "PulumiKubernetesConverter")
    let runtimes = [
        "linux-x64"
        "linux-arm64"
        "osx-x64"
        "osx-arm64"
        "win-x64"
        "win-arm64"
    ]

    Shell.deleteDirs [
        Path.Combine(cwd, "bin")
        Path.Combine(cwd, "obj")
        artifacts
    ]
    
    let binary = "pulumi-converter-k8s"
    for runtime in runtimes do
        printfn $"Building binary {binary} for {runtime}"
        let args = [
            "publish"
            "--configuration Release"
            $"--runtime {runtime}"
            "--self-contained true"
            "-p:PublishSingleFile=true"
            "/p:DebugType=None"
            "/p:DebugSymbols=false"
        ]
        let exitCode = Shell.Exec("dotnet", String.concat " " args, cwd)
        if exitCode <> 0 then
            failwith $"failed to build for runtime {runtime}"
            
    Directory.create artifacts
    for runtime in runtimes do
        let publishPath = Path.Combine(cwd, "bin", "Release", "net7.0", runtime, "publish")
        let destinationRuntime =
            match runtime with
            | "osx-x64" -> "darwin-amd64"
            | "osx-arm64" -> "darwin-arm64"
            | "linux-x64" -> "linux-amd64"
            | "linux-arm64" -> "linux-arm64"
            | "win-x64" -> "windows-amd64"
            | "win-arm64" -> "windows-arm64"
            | _ -> runtime
       
        let destination = Path.Combine(artifacts, $"{binary}-v{version}-{destinationRuntime}.tar.gz")
        createTarGz publishPath destination

let inline await (task: Task<'t>) = 
    task
    |> Async.AwaitTask
    |> Async.RunSynchronously

let releaseVersion (release: Release) = 
    if not (String.IsNullOrWhiteSpace(release.Name)) then
        release.Name.Substring(1, release.Name.Length - 1) 
    elif not (String.IsNullOrWhiteSpace(release.TagName)) then 
        release.TagName.Substring(1, release.TagName.Length - 1)
    else 
        ""

let createAndPublishArtifacts() =
    let version = converterVersion()
    let github = new GitHubClient(ProductHeaderValue "PulumiKubernetesConverter")
    let githubToken = Environment.GetEnvironmentVariable "GITHUB_TOKEN"
    // only assign github token to the client when it is available (usually in Github CI)
    if not (isNull githubToken) then
        printfn "GITHUB_TOKEN is available"
        github.Credentials <- Credentials(githubToken)
    else
        printfn "GITHUB_TOKEN is not set"

    let githubUsername = "Zaid-Ajaj"
    let githubRepo = "pulumi-converter-k8s"
    let releases = await (github.Repository.Release.GetAll(githubUsername, githubRepo))
    let alreadyReleased = releases |> Seq.exists (fun release -> releaseVersion release = version)
        
    if alreadyReleased then
        printfn $"Release v{version} already exists, skipping publish"
    else
        printfn $"Preparing artifacts to release v{version}"
        createArtifacts()
        let releaseInfo = NewRelease($"v{version}")
        let release = await (github.Repository.Release.Create(githubUsername, githubRepo, releaseInfo))
        for file in Directory.EnumerateFiles artifacts do
            let asset = ReleaseAssetUpload()
            asset.FileName <- Path.GetFileName file
            asset.ContentType <- "application/tar"
            asset.RawData <- File.OpenRead(file)
            let uploadedAsset = await (github.Repository.Release.UploadAsset(release, asset))
            printfn $"Uploaded {uploadedAsset.Name} into assets of v{version}"

[<EntryPoint>]
let main(args: string[]) : int =
    match args with
    | [| "tests" |] ->
        runTests()

    | [| "integration-tests" |] ->
        integrationTests()

    | [| "build" |] ->
        buildSolution()

    | [| "version" |] ->
        printfn $"{converterVersion()}"

    | [| "publish" |] ->
        createAndPublishArtifacts()
        
    | [| "create-artifacts" |] ->
        createArtifacts()

    | otherwise -> printfn $"Unknown build arguments provided %A{otherwise}"
    0
