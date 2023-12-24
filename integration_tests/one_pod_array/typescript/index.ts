import * as pulumi from "@pulumi/pulumi";
import * as kubernetes from "@pulumi/kubernetes";

const bar = new kubernetes.core.v1.Pod("bar", {
    metadata: {
        name: "bar",
        namespace: "foo",
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
