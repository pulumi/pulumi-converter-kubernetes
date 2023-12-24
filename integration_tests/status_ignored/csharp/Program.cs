using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Kubernetes = Pulumi.Kubernetes;

return await Deployment.RunAsync(() => 
{
    var argocdServer = new Kubernetes.Apps.V1.Deployment("argocdServer", new()
    {
        ApiVersion = "apps/v1",
        Kind = "Deployment",
        Metadata = new Kubernetes.Types.Inputs.Meta.V1.ObjectMetaArgs
        {
            CreationTimestamp = "2020-08-04T18:50:43Z",
            Generation = 1,
            Name = "argocd-server",
            Namespace = "default",
        },
    });

});

