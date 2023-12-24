module Converter.YamlDocument

open System
open System.IO
open YamlDotNet.RepresentationModel
open FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
type Node =
    | Scalar of YamlScalarNode
    | Sequence of Node list
    | Mapping of Map<string, Node>
    | None

/// Document describes a single YAML document within a YAML file that is a mapping
/// For example "name: foo" is a document with a single entry "name" that is mapped to a scalar "foo"
/// A YAML file can contain multiple documents, for example the following
///
/// 
/// name: foo
/// ---
/// name: bar
type Document = { content: Map<string, Node> }

/// KubeDocument describes a single Kubernetes resource from a YAML document
/// It has a valid non-empty apiVersion, kind, name, optional namespace and a content that is a mapping
/// A YAML file can have multiple KubeDocuments each of which describes a Kubernetes resource
type KubeDocument = {
    content: Map<string, Node>
    apiVersion: string
    kind: string
    name: string
    documentNamespace: string option
}

let rec readYamlNode (node: YamlNode) =
    match node with
    | :? YamlScalarNode as scalar -> Node.Scalar scalar
    | :? YamlSequenceNode as sequence -> Node.Sequence [ for item in sequence.Children -> readYamlNode item ]
    | :? YamlMappingNode as mapping ->
        let pairs =  [
            for pair in mapping.Children do
            match pair.Key with
            | :? YamlScalarNode as scalar -> scalar.Value, readYamlNode pair.Value
            | _ -> () ]

        pairs
        |> Map.ofList
        |> Node.Mapping
    | _ ->
        Node.None

let parseYamlDocuments (yamlText: string) =
    let yamlStream = new YamlStream()
    yamlStream.Load(new StringReader(yamlText))
    List.choose id [
        for document in yamlStream.Documents do
        match readYamlNode document.RootNode with
        | Node.Mapping content -> Some { content = content }
        | _ -> None
    ]

let unquote (value: string) =
    if value.StartsWith("\"") && value.EndsWith("\"") then
        value.Substring(1, value.Length - 2)
    elif value.StartsWith("'") && value.EndsWith("'") then
        value.Substring(1, value.Length - 2)
    else
        value

let parseApiVersion (content: Map<string, Node>) =
    match content.TryFind "apiVersion" with
    | Some (Node.Scalar scalar) ->
        let apiVersion = unquote scalar.Value
        if not (apiVersion.Contains "/") then
            Ok $"core/{apiVersion}"
        else
            Ok scalar.Value
    | Some _ -> Error "apiVersion field for the resource is not a scalar"
    | None -> Error "apiVersion field for the resource is not specified and is required"

let parseKind (content: Map<string, Node>) =
    match content.TryFind "kind" with
    | Some (Node.Scalar scalar) ->
        let kind = unquote scalar.Value
        if String.IsNullOrWhiteSpace kind then
            Error "kind field for the resource is empty"
        elif kind = "CustomResourceDefinition" then
            Error "kind field for the resource is CustomResourceDefinition which cannot be converted"
        else
            Ok kind
    | Some _ -> Error "kind field for the resource is not a scalar"
    | None -> Error "kind field for the resource is not specified and is required"

let parseMetadata (content: Map<string, Node>) =
    match content.TryFind "metadata" with
    | Some (Node.Mapping metadata) -> Ok metadata
    | Some _ -> Error "metadata field for the resource is not a mapping"
    | None -> Error "metadata field for the resource is not specified and is required"

let parseName (metadata: Map<string, Node>) =
    match metadata.TryFind "name" with
    | Some (Node.Scalar scalar) ->
        let name = unquote scalar.Value
        if String.IsNullOrWhiteSpace name then
            Error "name field for the resource is empty"
        else
            Ok name
    | Some _ -> Error "name field for the resource is not a scalar"
    | None -> Error "name field for the resource is not specified and is required"

let parseNamespace (metadata: Map<string, Node>) =
    match metadata.TryFind "namespace" with
    | Some (Node.Scalar scalar) -> Some (unquote scalar.Value)
    | _ -> None

/// removes redundant top-level fields from the document content.
/// Specifically "status" field because it is read-only, "apiVersion" and "kind" because they are already parsed.
let removeRedundantFields (content: Map<string, Node>) =
    content
    |> Map.remove "status"
    |> Map.remove "apiVersion"
    |> Map.remove "kind"

let kubeDocument (document: Document) = result {
    let! apiVersion = parseApiVersion document.content
    let! kind = parseKind document.content
    let! metadata = parseMetadata document.content
    let! name = parseName metadata
    return {
        apiVersion = apiVersion
        kind = kind
        name = name
        content = removeRedundantFields document.content
        documentNamespace = parseNamespace metadata
    }
}