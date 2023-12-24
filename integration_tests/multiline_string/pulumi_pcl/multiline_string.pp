resource coredns "kubernetes:core/v1:ConfigMap" {
    data = {
        "Corefile" = <<EOF
.:53 {
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
EOF
    }
    metadata = {
        "name" = "coredns"
        "namespace" = "kube-system"
    }
}
