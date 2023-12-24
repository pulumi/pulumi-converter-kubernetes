# pulumi-converter-k8s

A Pulumi converter plugin to convert kubernetes manifests to Pulumi languages. The idea for this converter is to replace [kube2pulummi](https://github.com/pulumi/kube2pulumi). Currently work in progress.

### Installation
Install the plugin from Github releases using the following command
```
pulumi plugin install converter k8s --server github://api.github.com/Zaid-Ajaj
```

### Usage
Run the following command in the directory where your kubernetes file is located
```
pulumi convert --from k8s --language <language> --out pulumi
```
Will convert kubernetes code into your language of choice: `typescript`, `csharp`, `python`, `go`, `java` or `yaml`

## Component Mode

You can output every kubernetes resource as a Pulumi component resource using the `--component-mode` flag as follows:
```
pulumi convert --from k8s --language <language> --out pulumi -- --component-mode
```

### Development

The following commands are available which you can run inside the root directory of the repo.

### Build the solution

```bash
dotnet run build 
```

### Run unit tests
```bash
dotnet run unit tests
```

### Run integration tests
```bash
dotnet run integration tests
```