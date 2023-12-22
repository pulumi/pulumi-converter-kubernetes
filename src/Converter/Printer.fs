module Converter.Printer

open System.Text
open Converter.PulumiTypes

let rec print (expression: PulumiSyntax) (indentSize: int) (builder: StringBuilder) =
    let invariantCulture = System.Globalization.CultureInfo.InvariantCulture
    let append (input: string) = builder.Append input |> ignore
    let indent() = append(String.replicate indentSize " ")
    let indentBy(size: int) = append(String.replicate size " ")
    match expression with
    | PulumiSyntax.String value -> append(sprintf $"\"{value}\"")
    | PulumiSyntax.MultilineString value ->
        append $"<<EOF\n{value}\nEOF"
    | PulumiSyntax.InterpolatedString (expressions, values) ->
        let mutable expressionIndex = 0
        let printExpr() =
            if expressionIndex <= expressions.Length - 1 then
                append "${"
                print expressions[expressionIndex] indentSize builder
                expressionIndex <- expressionIndex + 1
                append "}"
    
        append "\""
        for value in values do
            append value
            printExpr()
        append "\""
    | PulumiSyntax.Integer value -> append(value.ToString(invariantCulture))
    | PulumiSyntax.Number value -> append(value.ToString(invariantCulture))
    | PulumiSyntax.Identifier value -> append value
    | PulumiSyntax.Null -> append "null"
    | PulumiSyntax.VariableAccess variableName -> append variableName
    | PulumiSyntax.IndexExpression (target, index) ->
        print target indentSize builder
        append "["
        print index indentSize builder
        append "]"
    | PulumiSyntax.PropertyAccess (target, propertyName) ->
        print target indentSize builder
        append "."
        append propertyName
    | PulumiSyntax.Boolean value -> append(if value then "true" else "false")
    | PulumiSyntax.Array values ->
        append "[\n"
        for (i, value) in List.indexed values do
            indentBy (indentSize + 4)
            print value (indentSize + 4) builder
            if i <> values.Length - 1 then
                append ",\n"
            else
                append "\n"
        indent()
        append "]\n"

    | PulumiSyntax.Object properties ->
        append "{\n"
        for (key, value) in Map.toList properties do
            indentBy (indentSize + 4)
            print key indentSize builder
            append " = "
            print value (indentSize + 4) builder
            append "\n"
        
        indent()
        append "}"

    | PulumiSyntax.FunctionCall(name, args) ->
        append name
        append "("
        for (i, arg) in List.indexed args do
            print arg indentSize builder
            if i <> args.Length - 1 then
                append ", "
                
        append ")"

    | PulumiSyntax.TernaryExpression (condition, trueResult, falseResult) ->
        print condition indentSize builder
        append " ? "
        print trueResult indentSize builder
        append " : "
        print falseResult indentSize builder
        
    | PulumiSyntax.UnaryExpression (operator, operand) ->
        append operator
        print operand indentSize builder

    | PulumiSyntax.BinaryExpression (operator, left, right) ->
        print left indentSize builder
        append $" {operator} "
        print right indentSize builder
        
    | PulumiSyntax.For forSyntax ->
        append $"[for {forSyntax.variable} in "
        print forSyntax.expression indentSize builder
        append " : "
        print forSyntax.body indentSize builder
        append "]"

    | PulumiSyntax.Empty ->
        ()

let printProgram(program: PulumiProgram) =
    let builder = StringBuilder()
    let append (input: string) = builder.Append input |> ignore
    let indentSize = 0
    for node in program.nodes do
        match node with
        | PulumiNode.LocalVariable variable ->
            append variable.name
            append " = "
            print variable.value indentSize builder
            append "\n"

        | PulumiNode.OutputVariable output ->
            append $"output {output.name} {{\n"
            append "    value = "
            print output.value indentSize builder
            append "\n"
            append "}\n"
 
        | PulumiNode.ConfigVariable config ->
            append $"config {config.name} \"{config.configType}\" {{\n"
            match config.description with
            | None -> ()
            | Some description ->
                append "    "
                append $"description = \"{description}\"\n"

            match config.defaultValue with
            | None  -> ()
            | Some defaultValue ->
                append "    default = "
                print defaultValue indentSize builder
                append "\n"
            append "}\n"

        | PulumiNode.Resource resource ->
            append $"resource {resource.name} \"{resource.token}\" {{\n"
            match resource.options with
            | None -> ()
            | Some options ->
                append "    options {\n"
                match options.range with
                | None -> ()
                | Some range ->
                    append "        range = "
                    print range (indentSize + 8) builder
                    append "\n"
                    
                match options.parent with
                | None -> ()
                | Some parent ->
                    append "        parent = "
                    print parent (indentSize + 8) builder
                    append "\n"
                 
                match options.dependsOn with
                | None -> ()
                | Some dependsOn ->
                    append "        dependsOn = "
                    print dependsOn (indentSize + 8) builder
                    append "\n"
                
                append "    }\n"
                
            match resource.logicalName with
            | None -> ()
            | Some logicalName ->
                append $"    __logicalName = \"{logicalName}\"\n"
                
            for key, value in Map.toList resource.inputs do
                append $"    {key} = "
                print value (indentSize + 4) builder
                append "\n"
            append "}\n"
            
        | PulumiNode.Component componentDecl ->
            append $"component {componentDecl.name} \"{componentDecl.path}\" {{\n"
            match componentDecl.options with
            | None -> ()
            | Some options ->
                append "    options {\n"
                match options.range with
                | None -> ()
                | Some range ->
                    append "        range = "
                    print range (indentSize + 8) builder
                    append "\n"
                    
                match options.parent with
                | None -> ()
                | Some parent ->
                    append "        parent = "
                    print parent (indentSize + 8) builder
                    append "\n"
                 
                match options.dependsOn with
                | None -> ()
                | Some dependsOn ->
                    append "        dependsOn = "
                    print dependsOn (indentSize + 8) builder
                    append "\n"
                
                append "    }\n"

            match componentDecl.logicalName with
            | None -> ()
            | Some logicalName ->
                append $"    __logicalName = \"{logicalName}\"\n"

            for key, value in Map.toList componentDecl.inputs do
                append $"    "
                print key indentSize builder
                append " = "
                print value (indentSize + 4) builder
                append "\n"
            append "}\n"

    builder.ToString()