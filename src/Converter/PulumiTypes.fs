module rec Converter.PulumiTypes

type PulumiForSyntax = {
    variable : string
    expression : PulumiSyntax
    body : PulumiSyntax
}

[<RequireQualifiedAccess>]
type PulumiSyntax =
    | String of string
    | MultilineString of string
    | InterpolatedString of expressions: PulumiSyntax list * segments: string list
    | Integer of int64
    | Number of double
    | Boolean of bool
    | Array of elements:PulumiSyntax list
    | Object of properties:Map<PulumiSyntax, PulumiSyntax>
    | Null
    | FunctionCall of name: string * args:PulumiSyntax list
    | Identifier of name: string
    | PropertyAccess of target: PulumiSyntax * property: string
    | VariableAccess of variableName: string
    | UnaryExpression of operator: string * operand: PulumiSyntax
    | BinaryExpression of operator: string * left: PulumiSyntax * right: PulumiSyntax
    | TernaryExpression of condition: PulumiSyntax * trueValue: PulumiSyntax * falseValue: PulumiSyntax
    | IndexExpression of target: PulumiSyntax * index: PulumiSyntax
    | For of forSyntax: PulumiForSyntax
    | Empty

type ResourceOptions = {
    dependsOn: PulumiSyntax option
    provider: PulumiSyntax option
    parent: PulumiSyntax option
    range: PulumiSyntax option
}
  with static member Empty = { dependsOn = None; provider = None; parent = None; range = None }

type Resource = {
    name: string 
    logicalName: string option
    token: string
    inputs: Map<string, PulumiSyntax>
    options: ResourceOptions option
}

type Component = {
    name: string
    path: string
    logicalName: string option
    inputs: Map<PulumiSyntax, PulumiSyntax>
    options: ResourceOptions option
}

type OutputVariable = {
    name: string
    value: PulumiSyntax
}

type LocalVariable = {
    name: string
    value: PulumiSyntax
}

type ConfigVariable = {
    name: string
    defaultValue: PulumiSyntax option
    description: string option
    configType: string
}

[<RequireQualifiedAccess>]
type PulumiNode = 
    | Resource of Resource
    | OutputVariable of OutputVariable
    | LocalVariable of LocalVariable
    | ConfigVariable of ConfigVariable
    | Component of Component

type PulumiProgram = {
    nodes : PulumiNode list
}