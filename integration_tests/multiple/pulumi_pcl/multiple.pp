resource myNginxSvc "kubernetes:core/v1:Service" {
    metadata = {
        labels = {
            app = "nginx"
        }
        name = "my-nginx-svc"
    }
    spec = {
        ports = [
            {
                port = 80
            }
        ]

        selector = {
            app = "nginx"
        }
        type = "LoadBalancer"
    }
}
resource myNginx "kubernetes:apps/v1:Deployment" {
    metadata = {
        labels = {
            app = "nginx"
        }
        name = "my-nginx"
    }
    spec = {
        replicas = 3
        selector = {
            matchLabels = {
                app = "nginx"
            }
        }
        template = {
            metadata = {
                labels = {
                    app = "nginx"
                }
            }
            spec = {
                containers = [
                    {
                        image = "nginx:1.14.2"
                        name = "nginx"
                        ports = [
                            {
                                containerPort = 80
                            }
                        ]

                    }
                ]

            }
        }
    }
}
