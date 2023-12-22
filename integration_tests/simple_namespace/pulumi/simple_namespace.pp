resource kafka "kubernetes:core/v1:Namespace" {
    apiVersion = "v1"
    kind = "Namespace"
    metadata = {
        "labels" = {
            "name" = "kafka"
        }
        "name" = "kafka"
    }
}
