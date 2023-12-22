import * as pulumi from "@pulumi/pulumi";
import * as kubernetes from "@pulumi/kubernetes";

const argocdServer = new kubernetes.apps.v1.Deployment("argocdServer", {
    apiVersion: "apps/v1",
    kind: "Deployment",
    metadata: {
        creationTimestamp: "2020-08-04T18:50:43Z",
        generation: 1,
        name: "argocd-server",
        namespace: "default",
    },
});
