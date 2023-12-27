resource kafka "kubernetes:core/v1:Namespace" {
    metadata = {
        labels = {
            name = "kafka"
        }
        name = "kafka"
    }
}
