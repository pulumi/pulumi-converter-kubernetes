import * as pulumi from "@pulumi/pulumi";
import * as kubernetes from "@pulumi/kubernetes";

const coredns = new kubernetes.core.v1.ConfigMap("coredns", {
    data: {
        Corefile: `.:53 {
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
`,
    },
    metadata: {
        name: "coredns",
        namespace: "kube-system",
    },
});
