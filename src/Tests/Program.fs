open System
open System.Text
open System.Collections.Generic
open Converter.PulumiTypes
open Expecto
open Converter
open YamlDocument

let parsingYaml = testList "Parsing basic yaml works" [
    test "Parsing yaml works" {
        let yaml = "name: test"
        let documents = parseYamlDocuments yaml
        Expect.equal documents.Length 1 "There is one document"
        let name =
            documents.[0].content
            |> Map.tryFind "name"
            |> Option.map (function
                | Node.Scalar scalar -> scalar.Value
                | _ -> failwith "Expected scalar")

        Expect.equal name (Some "test") "Name is test"
    }
    
    test "Parsing yaml with multiple documents works" {
        let yaml = "name: test\n---\nname: test2"
        let documents = parseYamlDocuments yaml
        Expect.equal documents.Length 2 "There are two documents"
        let firstName =
            documents.[0].content
            |> Map.tryFind "name"
            |> Option.map (function
                | Node.Scalar scalar -> scalar.Value
                | _ -> failwith "Expected scalar for first document")

        Expect.equal firstName (Some "test") "Name is test"

        let secondName =
            documents.[1].content
            |> Map.tryFind "name"
            |> Option.map (function
                | Node.Scalar scalar -> scalar.Value
                | _ -> failwith "Expected scalar for second document")

        Expect.equal secondName (Some "test2") "Name is test"
    }
    
    test "Parsing empty nodes should be removed" {
        let yaml = "key: hello\nname: "
        let documents = parseYamlDocuments yaml
        Expect.equal documents.Length 1 "There is one document"
        let name =
            documents.[0].content
            |> Map.tryFind "name"
    
        Expect.equal name None "name key should be removed during parsing"
    }
    
    test "Parsing explicitly quoted empty nodes should not be removed" {
        let yaml = "key: hello\nname: \"\"\nvalue: ''"
        let documents = parseYamlDocuments yaml
        Expect.equal documents.Length 1 "There is one document"
        let name =
            documents.[0].content
            |> Map.tryFind "name"
            |> Option.map (function
                | Node.Scalar scalar -> scalar.Value
                | _ -> failwith "Expected scalar for second document")

        let value = 
            documents.[0].content
            |> Map.tryFind "value"
            |> Option.map (function
                | Node.Scalar scalar -> scalar.Value
                | _ -> failwith "Expected scalar for second document")
    
        Expect.equal name (Some "") "name should still be parsed an empty string"
        Expect.equal value (Some "") "value should still be parsed an empty string"
    }

    test "Parsing multiline strings works" {
        let yaml = """
data: "global:\n  evaluation_interval: 1m\n  external_labels:\n    cluster_id:\
  \ 'A'\n    port: 9003\n- job_name: kubecost-networking\n  kubernetes_sd_configs:\n\
  \    - role: pod\n  relabel_configs:\n  # Scrape only the the targets matching\
  \ the following metadata\n    - source_labels: [__meta_kubernetes_pod_label_app]\n\
  \      action: keep\n      regex:  kubecost-network-costs\n"
        """
        let documents = parseYamlDocuments yaml
        Expect.equal documents.Length 1 "There is one document"
        let data =
            documents.[0].content
            |> Map.tryFind "data"
            |> Option.map (function
                | Node.Scalar scalar -> scalar.Value
                | _ -> failwith "Expected scalar")

        Expect.equal data (Some """global:
  evaluation_interval: 1m
  external_labels:
    cluster_id: 'A'
    port: 9003
- job_name: kubecost-networking
  kubernetes_sd_configs:
    - role: pod
  relabel_configs:
  # Scrape only the the targets matching the following metadata
    - source_labels: [__meta_kubernetes_pod_label_app]
      action: keep
      regex:  kubecost-network-costs
""")
         "Multiline string in quotes should be parsed correctly"
    }

    test "Parsing complex keys works" {
        let yaml = @"
            kind: Deployment
            apiVersion: apps/v1
            metadata:
              name: repro
              managedFields:
                k:{""name"":""controller""}: foo
        "
        let names = Dictionary<string, int>()
        let resources : list<Resource> = [
            for document in parseYamlDocuments yaml do
                match kubeDocument document with
                | Ok kubeDocument ->
                    let resource = Transform.fromKubeDocument kubeDocument names
                    resource
                | Error err -> failwith err
        ]

        let pulumiProgram : PulumiTypes.PulumiProgram = {
            nodes = [ for res in resources -> PulumiNode.Resource res ]
        }
        let expected = @"resource ""repro"" ""kubernetes:apps/v1:Deployment"" {
    metadata = {
        managedFields = {
            ""k:{\u0022name\u0022:\u0022controller\u0022}"" = ""foo""
        }
        name = ""repro""
    }
}
"
        let output = Printer.printProgram pulumiProgram
        Expect.equal expected output $"Output was '{output}'"
    }
]

let allTests = testList "All tests" [
    parsingYaml
]

[<EntryPoint>]
let main (args:string[]) = runTestsWithCLIArgs [  ] args allTests

