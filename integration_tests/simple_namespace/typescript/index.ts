import * as pulumi from "@pulumi/pulumi";
import * as kubernetes from "@pulumi/kubernetes";

const kafka = new kubernetes.core.v1.Namespace("kafka", {metadata: {
    labels: {
        name: "kafka",
    },
    name: "kafka",
}});
