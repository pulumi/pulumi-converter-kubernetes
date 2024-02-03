import * as pulumi from "@pulumi/pulumi";
import * as kubernetes from "@pulumi/kubernetes";

const nginx_pod = new kubernetes.core.v1.Pod("nginx.pod", {
    metadata: {
        name: "nginx.pod",
        namespace: "frontend",
    },
    spec: {
        containers: [{
            image: "nginx:1.14-alpine",
            name: "nginx",
            resources: {
                limits: {
                    cpu: "0.2",
                    memory: "20Mi",
                },
            },
        }],
    },
});
