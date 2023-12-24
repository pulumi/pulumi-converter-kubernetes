resource argocdServer "kubernetes:apps/v1:Deployment" {
    apiVersion = "apps/v1"
    kind = "Deployment"
    metadata = {
        "creationTimestamp" = "2020-08-04T18:50:43Z"
        "generation" = 1
        "name" = "argocd-server"
        "namespace" = "default"
    }
}
