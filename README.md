# pulumi-converter-kubernetes

The Pulumi Converter Kubernetes plugin facilitates the conversion of Kubernetes manifests into Pulumi languages, enhancing your Kubernetes development workflow. By leveraging strong types, compilation errors, and comprehensive IDE support, such as autocomplete, you can effectively declare and manage infrastructure across various clouds within the same program that handles your Kubernetes resources.

This converter supersedes [kube2pulumi](https://github.com/pulumi/kube2pulumi).

### Installation
To utilize the pulumi-converter-kubernetes plugin for converting Kubernetes YAML to Pulumi and subsequently deploying it, you must first install the Pulumi CLI.

Install the plugin from Github releases using the following command:

```shell
pulumi plugin install converter kubernetes
```

### Usage
Execute the following command within the directory containing your Kubernetes manifests:

```shell
pulumi convert --from kubernetes --language <language> --out <output_dir>
```
This command converts Kubernetes code into your preferred language: `typescript`, `csharp`, `python`, `go`, `java` or `yaml`

##### Component Mode

You can output every kubernetes resource as a Pulumi component resource specifying the `--component-mode` flag:

```shell
pulumi convert --from kubernetes --language <language> --out <output_dir> -- --component-mode
```

### Example
Let's convert a simple YAML file describing a pod with a single container running nginx:
```yaml
apiVersion: v1
kind: Pod
metadata:
  namespace: foo
  name: bar
spec:
  containers:
    - name: nginx
      image: nginx:1.14-alpine
      resources:
        limits:
          memory: 20Mi
          cpu: 0.2
```
#### Go
```shell
pulumi convert --from kubernetes --language go --out program
```

```go
// ./pulumi/main.go
package main

import (
	corev1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func main() {
	pulumi.Run(func(ctx *pulumi.Context) error {
		_, err := corev1.NewPod(ctx, "bar", &corev1.PodArgs{
			Metadata: &metav1.ObjectMetaArgs{
				Name:      pulumi.String("bar"),
				Namespace: pulumi.String("foo"),
			},
			Spec: &corev1.PodSpecArgs{
				Containers: corev1.ContainerArray{
					&corev1.ContainerArgs{
						Image: pulumi.String("nginx:1.14-alpine"),
						Name:  pulumi.String("nginx"),
						Resources: &corev1.ResourceRequirementsArgs{
							Limits: pulumi.StringMap{
								"cpu":    pulumi.String("0.2"),
								"memory": pulumi.String("20Mi"),
							},
						},
					},
				},
			},
		})
		if err != nil {
			return err
		}
		return nil
	})
}
```

#### Typescript
```shell
pulumi convert --from kubernetes --language go --out program
```
```typescript
// ./program/index.ts
import * as pulumi from "@pulumi/pulumi";
import * as kubernetes from "@pulumi/kubernetes";

const bar = new kubernetes.core.v1.Pod("bar", {
    metadata: {
        name: "bar",
        namespace: "foo",
    },
    spec: {
        containers: [{
            image: "nginx:1.14-alpine",
            name: "nginx",
            resources: {
                limits: {
                    cpu: "0.2",
                    memory: "20Mi",
                },
            },
        }],
    },
});
```

#### Python
```shell
pulumi convert --from kubernetes --language python --out program
```
```python
# ./program/__main__.py
import pulumi
import pulumi_kubernetes as kubernetes

bar = kubernetes.core.v1.Pod("bar",
    metadata=kubernetes.meta.v1.ObjectMetaArgs(
        name="bar",
        namespace="foo",
    ),
    spec=kubernetes.core.v1.PodSpecArgs(
        containers=[kubernetes.core.v1.ContainerArgs(
            image="nginx:1.14-alpine",
            name="nginx",
            resources=kubernetes.core.v1.ResourceRequirementsArgs(
                limits={
                    "cpu": "0.2",
                    "memory": "20Mi",
                },
            ),
        )],
    ))
```

#### C#
```shell
pulumi convert --from kubernetes --language csharp --out program
```
```c#
// ./program/Program.cs
using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Kubernetes = Pulumi.Kubernetes;

return await Deployment.RunAsync(() =>
{
    var bar = new Kubernetes.Core.V1.Pod("bar", new()
    {
        Metadata = new Kubernetes.Types.Inputs.Meta.V1.ObjectMetaArgs
        {
            Name = "bar",
            Namespace = "foo",
        },
        Spec = new Kubernetes.Types.Inputs.Core.V1.PodSpecArgs
        {
            Containers = new[]
            {
                new Kubernetes.Types.Inputs.Core.V1.ContainerArgs
                {
                    Image = "nginx:1.14-alpine",
                    Name = "nginx",
                    Resources = new Kubernetes.Types.Inputs.Core.V1.ResourceRequirementsArgs
                    {
                        Limits =
                        {
                            { "cpu", "0.2" },
                            { "memory", "20Mi" },
                        },
                    },
                },
            },
        },
    });

});
```

#### Java
```shell
pulumi convert --from kubernetes --language java --out program
```
```java
// ./program/src/main/java/generated_program/App.java
package generated_program;

import com.pulumi.Context;
import com.pulumi.Pulumi;
import com.pulumi.core.Output;
import com.pulumi.kubernetes.core_v1.Pod;
import com.pulumi.kubernetes.core_v1.PodArgs;
import com.pulumi.kubernetes.meta_v1.inputs.ObjectMetaArgs;
import com.pulumi.kubernetes.core_v1.inputs.PodSpecArgs;
import java.util.List;
import java.util.ArrayList;
import java.util.Map;
import java.io.File;
import java.nio.file.Files;
import java.nio.file.Paths;

public class App {
    public static void main(String[] args) {
        Pulumi.run(App::stack);
    }

    public static void stack(Context ctx) {
        var bar = new Pod("bar", PodArgs.builder()
            .metadata(ObjectMetaArgs.builder()
                .name("bar")
                .namespace("foo")
                .build())
            .spec(PodSpecArgs.builder()
                .containers(ContainerArgs.builder()
                    .image("nginx:1.14-alpine")
                    .name("nginx")
                    .resources(ResourceRequirementsArgs.builder()
                        .limits(Map.ofEntries(
                            Map.entry("cpu", 0.2),
                            Map.entry("memory", "20Mi")
                        ))
                        .build())
                    .build())
                .build())
            .build());

    }
}
```

### Plugin Development

The following commands are available for execution within the root directory of the repository:

###### Build the solution
```bash
dotnet run build 
```

###### Run unit tests
```bash
dotnet run unit tests
```

###### Run integration tests
```bash
dotnet run integration tests
```

### Limitations

`pulumi-converter-kubernetes` does not handle the conversion of CustomResourceDefinitions or CustomResources. However, our supplementary tool `crd2pulumi` generates strongly-typed arguments for a Resource based on your CRD. If using CRD/CRs, ensure to explore the following tool:

1. [crd2pulumi README](https://github.com/pulumi/crd2pulumi)