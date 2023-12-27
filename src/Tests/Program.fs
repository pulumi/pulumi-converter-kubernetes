open System
open System.Text
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
]

let allTests = testList "All tests" [
    parsingYaml
]

[<EntryPoint>]
let main (args:string[]) = runTestsWithCLIArgs [  ] args allTests

