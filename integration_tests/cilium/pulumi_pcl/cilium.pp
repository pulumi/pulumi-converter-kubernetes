resource "cilium" "kubernetes:apps/v1:DaemonSet" {
    metadata = {
        labels = {
            "app.kubernetes.io/name" = "cilium-agent"
            "app.kubernetes.io/part-of" = "cilium"
            "k8s-app" = "cilium"
        }
        name = "cilium"
        namespace = "default"
    }
    spec = {
        selector = {
            matchLabels = {
                "k8s-app" = "cilium"
            }
        }
        template = {
            metadata = {
                annotations = {
                    "container.apparmor.security.beta.kubernetes.io/apply-sysctl-overwrites" = "unconfined"
                    "container.apparmor.security.beta.kubernetes.io/cilium-agent" = "unconfined"
                    "container.apparmor.security.beta.kubernetes.io/clean-cilium-state" = "unconfined"
                    "container.apparmor.security.beta.kubernetes.io/mount-cgroup" = "unconfined"
                }
                labels = {
                    "app.kubernetes.io/name" = "cilium-agent"
                    "app.kubernetes.io/part-of" = "cilium"
                    "k8s-app" = "cilium"
                }
            }
            spec = {
                affinity = {
                    podAntiAffinity = {
                        requiredDuringSchedulingIgnoredDuringExecution = [
                            {
                                labelSelector = {
                                    matchLabels = {
                                        "k8s-app" = "cilium"
                                    }
                                }
                                topologyKey = "kubernetes.io/hostname"
                            }
                        ]

                    }
                }
                automountServiceAccountToken = "true"
                containers = [
                    {
                        args = [
                            "--config-dir=/tmp/cilium/config-map"
                        ]

                        command = [
                            "cilium-agent"
                        ]

                        env = [
                            {
                                name = "K8S_NODE_NAME"
                                valueFrom = {
                                    fieldRef = {
                                        apiVersion = "v1"
                                        fieldPath = "spec.nodeName"
                                    }
                                }
                            },
                            {
                                name = "CILIUM_K8S_NAMESPACE"
                                valueFrom = {
                                    fieldRef = {
                                        apiVersion = "v1"
                                        fieldPath = "metadata.namespace"
                                    }
                                }
                            },
                            {
                                name = "CILIUM_CLUSTERMESH_CONFIG"
                                value = "/var/lib/cilium/clustermesh/"
                            }
                        ]

                        image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35"
                        imagePullPolicy = "IfNotPresent"
                        lifecycle = {
                            preStop = {
                                exec = {
                                    command = [
                                        "/cni-uninstall.sh"
                                    ]

                                }
                            }
                        }
                        livenessProbe = {
                            failureThreshold = 10
                            httpGet = {
                                host = "127.0.0.1"
                                httpHeaders = [
                                    {
                                        name = "brief"
                                        value = "true"
                                    }
                                ]

                                path = "/healthz"
                                port = 9879
                                scheme = "HTTP"
                            }
                            periodSeconds = 30
                            successThreshold = 1
                            timeoutSeconds = 5
                        }
                        name = "cilium-agent"
                        readinessProbe = {
                            failureThreshold = 3
                            httpGet = {
                                host = "127.0.0.1"
                                httpHeaders = [
                                    {
                                        name = "brief"
                                        value = "true"
                                    }
                                ]

                                path = "/healthz"
                                port = 9879
                                scheme = "HTTP"
                            }
                            periodSeconds = 30
                            successThreshold = 1
                            timeoutSeconds = 5
                        }
                        securityContext = {
                            capabilities = {
                                add = [
                                    "CHOWN",
                                    "KILL",
                                    "NET_ADMIN",
                                    "NET_RAW",
                                    "IPC_LOCK",
                                    "SYS_MODULE",
                                    "SYS_ADMIN",
                                    "SYS_RESOURCE",
                                    "DAC_OVERRIDE",
                                    "FOWNER",
                                    "SETGID",
                                    "SETUID"
                                ]

                                drop = [
                                    "ALL"
                                ]

                            }
                            seLinuxOptions = {
                                level = "s0"
                                type = "spc_t"
                            }
                        }
                        startupProbe = {
                            failureThreshold = 105
                            httpGet = {
                                host = "127.0.0.1"
                                httpHeaders = [
                                    {
                                        name = "brief"
                                        value = "true"
                                    }
                                ]

                                path = "/healthz"
                                port = 9879
                                scheme = "HTTP"
                            }
                            periodSeconds = 2
                            successThreshold = 1
                        }
                        terminationMessagePolicy = "FallbackToLogsOnError"
                        volumeMounts = [
                            {
                                mountPath = "/host/proc/sys/net"
                                name = "host-proc-sys-net"
                            },
                            {
                                mountPath = "/host/proc/sys/kernel"
                                name = "host-proc-sys-kernel"
                            },
                            {
                                mountPath = "/sys/fs/bpf"
                                mountPropagation = "HostToContainer"
                                name = "bpf-maps"
                            },
                            {
                                mountPath = "/var/run/cilium"
                                name = "cilium-run"
                            },
                            {
                                mountPath = "/host/etc/cni/net.d"
                                name = "etc-cni-netd"
                            },
                            {
                                mountPath = "/var/lib/cilium/clustermesh"
                                name = "clustermesh-secrets"
                                readOnly = "true"
                            },
                            {
                                mountPath = "/lib/modules"
                                name = "lib-modules"
                                readOnly = "true"
                            },
                            {
                                mountPath = "/run/xtables.lock"
                                name = "xtables-lock"
                            },
                            {
                                mountPath = "/var/lib/cilium/tls/hubble"
                                name = "hubble-tls"
                                readOnly = "true"
                            },
                            {
                                mountPath = "/tmp"
                                name = "tmp"
                            }
                        ]

                    }
                ]

                hostNetwork = "true"
                initContainers = [
                    {
                        command = [
                            "cilium",
                            "build-config"
                        ]

                        env = [
                            {
                                name = "K8S_NODE_NAME"
                                valueFrom = {
                                    fieldRef = {
                                        apiVersion = "v1"
                                        fieldPath = "spec.nodeName"
                                    }
                                }
                            },
                            {
                                name = "CILIUM_K8S_NAMESPACE"
                                valueFrom = {
                                    fieldRef = {
                                        apiVersion = "v1"
                                        fieldPath = "metadata.namespace"
                                    }
                                }
                            }
                        ]

                        image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35"
                        imagePullPolicy = "IfNotPresent"
                        name = "config"
                        terminationMessagePolicy = "FallbackToLogsOnError"
                        volumeMounts = [
                            {
                                mountPath = "/tmp"
                                name = "tmp"
                            }
                        ]

                    },
                    {
                        command = [
                            "sh",
                            "-ec",
                            <<EOF
cp /usr/bin/cilium-mount /hostbin/cilium-mount;
nsenter --cgroup=/hostproc/1/ns/cgroup --mount=/hostproc/1/ns/mnt "$${BIN_PATH}/cilium-mount" $CGROUP_ROOT;
rm /hostbin/cilium-mount

EOF
                        ]

                        env = [
                            {
                                name = "CGROUP_ROOT"
                                value = "/run/cilium/cgroupv2"
                            },
                            {
                                name = "BIN_PATH"
                                value = "/opt/cni/bin"
                            }
                        ]

                        image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35"
                        imagePullPolicy = "IfNotPresent"
                        name = "mount-cgroup"
                        securityContext = {
                            capabilities = {
                                add = [
                                    "SYS_ADMIN",
                                    "SYS_CHROOT",
                                    "SYS_PTRACE"
                                ]

                                drop = [
                                    "ALL"
                                ]

                            }
                            seLinuxOptions = {
                                level = "s0"
                                type = "spc_t"
                            }
                        }
                        terminationMessagePolicy = "FallbackToLogsOnError"
                        volumeMounts = [
                            {
                                mountPath = "/hostproc"
                                name = "hostproc"
                            },
                            {
                                mountPath = "/hostbin"
                                name = "cni-path"
                            }
                        ]

                    },
                    {
                        command = [
                            "sh",
                            "-ec",
                            <<EOF
cp /usr/bin/cilium-sysctlfix /hostbin/cilium-sysctlfix;
nsenter --mount=/hostproc/1/ns/mnt "$${BIN_PATH}/cilium-sysctlfix";
rm /hostbin/cilium-sysctlfix

EOF
                        ]

                        env = [
                            {
                                name = "BIN_PATH"
                                value = "/opt/cni/bin"
                            }
                        ]

                        image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35"
                        imagePullPolicy = "IfNotPresent"
                        name = "apply-sysctl-overwrites"
                        securityContext = {
                            capabilities = {
                                add = [
                                    "SYS_ADMIN",
                                    "SYS_CHROOT",
                                    "SYS_PTRACE"
                                ]

                                drop = [
                                    "ALL"
                                ]

                            }
                            seLinuxOptions = {
                                level = "s0"
                                type = "spc_t"
                            }
                        }
                        terminationMessagePolicy = "FallbackToLogsOnError"
                        volumeMounts = [
                            {
                                mountPath = "/hostproc"
                                name = "hostproc"
                            },
                            {
                                mountPath = "/hostbin"
                                name = "cni-path"
                            }
                        ]

                    },
                    {
                        args = [
                            <<EOF
mount | grep "/sys/fs/bpf type bpf" || mount -t bpf bpf /sys/fs/bpf
EOF
                        ]

                        command = [
                            "/bin/bash",
                            "-c",
                            "--"
                        ]

                        image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35"
                        imagePullPolicy = "IfNotPresent"
                        name = "mount-bpf-fs"
                        securityContext = {
                            privileged = "true"
                        }
                        terminationMessagePolicy = "FallbackToLogsOnError"
                        volumeMounts = [
                            {
                                mountPath = "/sys/fs/bpf"
                                mountPropagation = "Bidirectional"
                                name = "bpf-maps"
                            }
                        ]

                    },
                    {
                        command = [
                            "/init-container.sh"
                        ]

                        env = [
                            {
                                name = "CILIUM_ALL_STATE"
                                valueFrom = {
                                    configMapKeyRef = {
                                        key = "clean-cilium-state"
                                        name = "cilium-config"
                                        optional = "true"
                                    }
                                }
                            },
                            {
                                name = "CILIUM_BPF_STATE"
                                valueFrom = {
                                    configMapKeyRef = {
                                        key = "clean-cilium-bpf-state"
                                        name = "cilium-config"
                                        optional = "true"
                                    }
                                }
                            }
                        ]

                        image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35"
                        imagePullPolicy = "IfNotPresent"
                        name = "clean-cilium-state"
                        resources = {
                            requests = {
                                cpu = "100m"
                                memory = "100Mi"
                            }
                        }
                        securityContext = {
                            capabilities = {
                                add = [
                                    "NET_ADMIN",
                                    "SYS_MODULE",
                                    "SYS_ADMIN",
                                    "SYS_RESOURCE"
                                ]

                                drop = [
                                    "ALL"
                                ]

                            }
                            seLinuxOptions = {
                                level = "s0"
                                type = "spc_t"
                            }
                        }
                        terminationMessagePolicy = "FallbackToLogsOnError"
                        volumeMounts = [
                            {
                                mountPath = "/sys/fs/bpf"
                                name = "bpf-maps"
                            },
                            {
                                mountPath = "/run/cilium/cgroupv2"
                                mountPropagation = "HostToContainer"
                                name = "cilium-cgroup"
                            },
                            {
                                mountPath = "/var/run/cilium"
                                name = "cilium-run"
                            }
                        ]

                    },
                    {
                        command = [
                            "/install-plugin.sh"
                        ]

                        image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35"
                        imagePullPolicy = "IfNotPresent"
                        name = "install-cni-binaries"
                        resources = {
                            requests = {
                                cpu = "100m"
                                memory = "10Mi"
                            }
                        }
                        securityContext = {
                            capabilities = {
                                drop = [
                                    "ALL"
                                ]

                            }
                            seLinuxOptions = {
                                level = "s0"
                                type = "spc_t"
                            }
                        }
                        terminationMessagePolicy = "FallbackToLogsOnError"
                        volumeMounts = [
                            {
                                mountPath = "/host/opt/cni/bin"
                                name = "cni-path"
                            }
                        ]

                    }
                ]

                nodeSelector = {
                    "kubernetes.io/os" = "linux"
                }
                priorityClassName = "system-node-critical"
                restartPolicy = "Always"
                serviceAccount = "cilium"
                serviceAccountName = "cilium"
                terminationGracePeriodSeconds = 1
                tolerations = [
                    {
                        operator = "Exists"
                    }
                ]

                volumes = [
                    {
                        emptyDir = {
                        }
                        name = "tmp"
                    },
                    {
                        hostPath = {
                            path = "/var/run/cilium"
                            type = "DirectoryOrCreate"
                        }
                        name = "cilium-run"
                    },
                    {
                        hostPath = {
                            path = "/sys/fs/bpf"
                            type = "DirectoryOrCreate"
                        }
                        name = "bpf-maps"
                    },
                    {
                        hostPath = {
                            path = "/proc"
                            type = "Directory"
                        }
                        name = "hostproc"
                    },
                    {
                        hostPath = {
                            path = "/run/cilium/cgroupv2"
                            type = "DirectoryOrCreate"
                        }
                        name = "cilium-cgroup"
                    },
                    {
                        hostPath = {
                            path = "/opt/cni/bin"
                            type = "DirectoryOrCreate"
                        }
                        name = "cni-path"
                    },
                    {
                        hostPath = {
                            path = "/etc/cni/net.d"
                            type = "DirectoryOrCreate"
                        }
                        name = "etc-cni-netd"
                    },
                    {
                        hostPath = {
                            path = "/lib/modules"
                        }
                        name = "lib-modules"
                    },
                    {
                        hostPath = {
                            path = "/run/xtables.lock"
                            type = "FileOrCreate"
                        }
                        name = "xtables-lock"
                    },
                    {
                        name = "clustermesh-secrets"
                        projected = {
                            defaultMode = 400
                            sources = [
                                {
                                    secret = {
                                        name = "cilium-clustermesh"
                                        optional = "true"
                                    }
                                },
                                {
                                    secret = {
                                        items = [
                                            {
                                                key = "tls.key"
                                                path = "common-etcd-client.key"
                                            },
                                            {
                                                key = "tls.crt"
                                                path = "common-etcd-client.crt"
                                            },
                                            {
                                                key = "ca.crt"
                                                path = "common-etcd-client-ca.crt"
                                            }
                                        ]

                                        name = "clustermesh-apiserver-remote-cert"
                                        optional = "true"
                                    }
                                }
                            ]

                        }
                    },
                    {
                        hostPath = {
                            path = "/proc/sys/net"
                            type = "Directory"
                        }
                        name = "host-proc-sys-net"
                    },
                    {
                        hostPath = {
                            path = "/proc/sys/kernel"
                            type = "Directory"
                        }
                        name = "host-proc-sys-kernel"
                    },
                    {
                        name = "hubble-tls"
                        projected = {
                            defaultMode = 400
                            sources = [
                                {
                                    secret = {
                                        items = [
                                            {
                                                key = "tls.crt"
                                                path = "server.crt"
                                            },
                                            {
                                                key = "tls.key"
                                                path = "server.key"
                                            },
                                            {
                                                key = "ca.crt"
                                                path = "client-ca.crt"
                                            }
                                        ]

                                        name = "hubble-server-certs"
                                        optional = "true"
                                    }
                                }
                            ]

                        }
                    }
                ]

            }
        }
        updateStrategy = {
            rollingUpdate = {
                maxUnavailable = 2
            }
            type = "RollingUpdate"
        }
    }
}
