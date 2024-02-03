resource "nginx.pod" "kubernetes:core/v1:Pod" {
    metadata = {
        name = "nginx.pod"
        namespace = "frontend"
    }
    spec = {
        containers = [
            {
                image = "nginx:1.14-alpine"
                name = "nginx"
                resources = {
                    limits = {
                        cpu = 0.2
                        memory = "20Mi"
                    }
                }
            }
        ]

    }
}
