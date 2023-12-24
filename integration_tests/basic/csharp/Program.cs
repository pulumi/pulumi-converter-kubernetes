using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Kubernetes = Pulumi.Kubernetes;

return await Deployment.RunAsync(() => 
{
    var myNginxSvc = new Kubernetes.Core.V1.Service("myNginxSvc", new()
    {
        ApiVersion = "v1",
        Kind = "Service",
        Metadata = new Kubernetes.Types.Inputs.Meta.V1.ObjectMetaArgs
        {
            Labels = 
            {
                { "app", "nginx" },
            },
            Name = "my-nginx-svc",
        },
        Spec = new Kubernetes.Types.Inputs.Core.V1.ServiceSpecArgs
        {
            Ports = new[]
            {
                new Kubernetes.Types.Inputs.Core.V1.ServicePortArgs
                {
                    Port = 80,
                },
            },
            Selector = 
            {
                { "app", "nginx" },
            },
            Type = "LoadBalancer",
        },
    });

});

