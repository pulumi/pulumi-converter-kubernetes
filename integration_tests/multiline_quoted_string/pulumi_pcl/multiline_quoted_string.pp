resource "kubecostPrometheusServer" "kubernetes:core/v1:ConfigMap" {
    data = {
        "prometheus.yml" = <<EOF
global:
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

EOF
    }
    metadata = {
        labels = {
            app = "prometheus"
            chart = "prometheus-11.0.2"
            component = "server"
            heritage = "Helm"
            release = "kubecost"
        }
        name = "kubecost-prometheus-server"
        namespace = "kubecost"
    }
}
