module Converter.Transform

open System
open System.Collections.Generic
open Converter.PulumiTypes
open Converter.YamlDocument
open Humanizer
open YamlDotNet.Core
open YamlDotNet.RepresentationModel

let resourceName (document: KubeDocument) (usedNames: Dictionary<string, int>) =
    match document.documentNamespace with
    | None ->
        if not (usedNames.ContainsKey document.name) then
            usedNames.Add(document.name, 0)
            document.name
        else
            let lastUsedName = usedNames.[document.name]
            usedNames.[document.name] <- lastUsedName + 1
            $"%s{document.name}%d{lastUsedName + 1}"
    | Some ns ->
        let combined = $"{document.name}{ns}"
        if not (usedNames.ContainsKey document.name) then
            usedNames.Add(document.name, 0)
            document.name
        else if not (usedNames.ContainsKey combined) then
            usedNames.Add(combined, 0)
            combined
        else
            let lastUsedName = usedNames.[combined]
            usedNames.[combined] <- lastUsedName + 1
            $"%s{combined}%d{lastUsedName + 1}"

let fromYamlScalar (node: YamlScalarNode) =
    if node.Style = ScalarStyle.DoubleQuoted || node.Style = ScalarStyle.SingleQuoted then
        if node.Value.Contains "\"" || node.Value.Contains "\n" then
            PulumiSyntax.MultilineString (unquote node.Value)
        else
            PulumiSyntax.String (unquote node.Value)
    else
        let invariantCulture = System.Globalization.CultureInfo.InvariantCulture
        match Int32.TryParse(node.Value, invariantCulture) with
        | true, value -> PulumiSyntax.Integer value
        | _ ->
             match Double.TryParse(node.Value, invariantCulture) with
             | true, value -> PulumiSyntax.Number value
             | _ ->
                 if node.Value.Contains "\n" || node.Value.Contains "\"" then
                     PulumiSyntax.MultilineString node.Value
                 else
                     PulumiSyntax.String node.Value

let rec fromYamlNode = function
    | Node.Scalar node -> fromYamlScalar node
    | Node.Sequence sequence ->
        let transformed = [ for item in sequence -> fromYamlNode item ]
        PulumiSyntax.Array transformed
    | Node.Mapping mapping ->
        let transformed = Map.ofList [ for pair in mapping -> PulumiSyntax.String pair.Key, fromYamlNode pair.Value ]
        PulumiSyntax.Object transformed
    | Node.None ->
        PulumiSyntax.Empty

let fromKubeDocument (document: KubeDocument) (usedNames: Dictionary<string, int>) : Resource =
    let name = resourceName document usedNames
    let resource: Resource = {
        name = name.Underscore().Camelize()
        token = $"kubernetes:{document.apiVersion}:{document.kind}"
        inputs = Map.ofList [ for (key, value) in Map.toList document.content -> key, fromYamlNode value ]
        options = None
        logicalName = None
    }

    resource