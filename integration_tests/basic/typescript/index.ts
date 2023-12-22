import * as pulumi from "@pulumi/pulumi";
import * as kubernetes from "@pulumi/kubernetes";

const myNginxSvc = new kubernetes.core.v1.Service("myNginxSvc", {
    apiVersion: "v1",
    kind: "Service",
    metadata: {
        labels: {
            app: "nginx",
        },
        name: "my-nginx-svc",
    },
    spec: {
        ports: [{
            port: 80,
        }],
        selector: {
            app: "nginx",
        },
        type: "LoadBalancer",
    },
});
