resource myNginxSvc "kubernetes:core/v1:Service" {
    metadata = {
        "labels" = {
            "app" = "nginx"
        }
        "name" = "my-nginx-svc"
    }
    spec = {
        "ports" = [
            {
                "port" = 80
            }
        ]

        "selector" = {
            "app" = "nginx"
        }
        "type" = "LoadBalancer"
    }
}
