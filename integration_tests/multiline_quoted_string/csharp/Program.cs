using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Kubernetes = Pulumi.Kubernetes;

return await Deployment.RunAsync(() => 
{
    var kubecostPrometheusServer = new Kubernetes.Core.V1.ConfigMap("kubecostPrometheusServer", new()
    {
        Data = 
        {
            { "prometheus.yml", @"global:
  evaluation_interval: 1m
  external_labels:
    cluster_id: 'A'
    port: 9003
- job_name: kubecost-networking
  kubernetes_sd_configs:
    - role: pod
  relabel_configs:
  # Scrape only the the targets matching the following metadata
    - source_labels: [__meta_kubernetes_pod_label_app]
      action: keep
      regex:  kubecost-network-costs

" },
        },
        Metadata = new Kubernetes.Types.Inputs.Meta.V1.ObjectMetaArgs
        {
            Labels = 
            {
                { "app", "prometheus" },
                { "chart", "prometheus-11.0.2" },
                { "component", "server" },
                { "heritage", "Helm" },
                { "release", "kubecost" },
            },
            Name = "kubecost-prometheus-server",
            Namespace = "kubecost",
        },
    });

});

