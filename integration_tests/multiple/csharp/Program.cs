using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Kubernetes = Pulumi.Kubernetes;

return await Deployment.RunAsync(() => 
{
    var myNginxSvc = new Kubernetes.Core.V1.Service("myNginxSvc", new()
    {
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

    var myNginx = new Kubernetes.Apps.V1.Deployment("myNginx", new()
    {
        Metadata = new Kubernetes.Types.Inputs.Meta.V1.ObjectMetaArgs
        {
            Labels = 
            {
                { "app", "nginx" },
            },
            Name = "my-nginx",
        },
        Spec = new Kubernetes.Types.Inputs.Apps.V1.DeploymentSpecArgs
        {
            Replicas = 3,
            Selector = new Kubernetes.Types.Inputs.Meta.V1.LabelSelectorArgs
            {
                MatchLabels = 
                {
                    { "app", "nginx" },
                },
            },
            Template = new Kubernetes.Types.Inputs.Core.V1.PodTemplateSpecArgs
            {
                Metadata = new Kubernetes.Types.Inputs.Meta.V1.ObjectMetaArgs
                {
                    Labels = 
                    {
                        { "app", "nginx" },
                    },
                },
                Spec = new Kubernetes.Types.Inputs.Core.V1.PodSpecArgs
                {
                    Containers = new[]
                    {
                        new Kubernetes.Types.Inputs.Core.V1.ContainerArgs
                        {
                            Image = "nginx:1.14.2",
                            Name = "nginx",
                            Ports = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.ContainerPortArgs
                                {
                                    ContainerPort = 80,
                                },
                            },
                        },
                    },
                },
            },
        },
    });

});

