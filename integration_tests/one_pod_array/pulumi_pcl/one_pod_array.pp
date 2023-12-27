resource bar "kubernetes:core/v1:Pod" {
    metadata = {
        name = "bar"
        namespace = "foo"
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
