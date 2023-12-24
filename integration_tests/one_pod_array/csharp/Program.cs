using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Kubernetes = Pulumi.Kubernetes;

return await Deployment.RunAsync(() => 
{
    var bar = new Kubernetes.Core.V1.Pod("bar", new()
    {
        ApiVersion = "v1",
        Kind = "Pod",
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

