using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Kubernetes = Pulumi.Kubernetes;

return await Deployment.RunAsync(() => 
{
    var coredns = new Kubernetes.Core.V1.ConfigMap("coredns", new()
    {
        Data = 
        {
            { "Corefile", @".:53 {
    errors
    health {
      lameduck 5s
    }
    ready
    kubernetes CLUSTER_DOMAIN REVERSE_CIDRS {
      fallthrough in-addr.arpa ip6.arpa
    }
    prometheus :9153
    forward . UPSTREAMNAMESERVER {
      max_concurrent 1000
    }
    cache 30
    loop
    reload
    loadbalance
}STUBDOMAINS
" },
        },
        Metadata = new Kubernetes.Types.Inputs.Meta.V1.ObjectMetaArgs
        {
            Name = "coredns",
            Namespace = "kube-system",
        },
    });

});

