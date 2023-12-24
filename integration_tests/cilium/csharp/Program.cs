using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Kubernetes = Pulumi.Kubernetes;

return await Deployment.RunAsync(() => 
{
    var cilium = new Kubernetes.Apps.V1.DaemonSet("cilium", new()
    {
        Metadata = new Kubernetes.Types.Inputs.Meta.V1.ObjectMetaArgs
        {
            Labels = 
            {
                { "app.kubernetes.io/name", "cilium-agent" },
                { "app.kubernetes.io/part-of", "cilium" },
                { "k8s-app", "cilium" },
            },
            Name = "cilium",
            Namespace = "default",
        },
        Spec = new Kubernetes.Types.Inputs.Apps.V1.DaemonSetSpecArgs
        {
            Selector = new Kubernetes.Types.Inputs.Meta.V1.LabelSelectorArgs
            {
                MatchLabels = 
                {
                    { "k8s-app", "cilium" },
                },
            },
            Template = new Kubernetes.Types.Inputs.Core.V1.PodTemplateSpecArgs
            {
                Metadata = new Kubernetes.Types.Inputs.Meta.V1.ObjectMetaArgs
                {
                    Annotations = 
                    {
                        { "container.apparmor.security.beta.kubernetes.io/apply-sysctl-overwrites", "unconfined" },
                        { "container.apparmor.security.beta.kubernetes.io/cilium-agent", "unconfined" },
                        { "container.apparmor.security.beta.kubernetes.io/clean-cilium-state", "unconfined" },
                        { "container.apparmor.security.beta.kubernetes.io/mount-cgroup", "unconfined" },
                    },
                    Labels = 
                    {
                        { "app.kubernetes.io/name", "cilium-agent" },
                        { "app.kubernetes.io/part-of", "cilium" },
                        { "k8s-app", "cilium" },
                    },
                },
                Spec = new Kubernetes.Types.Inputs.Core.V1.PodSpecArgs
                {
                    Affinity = new Kubernetes.Types.Inputs.Core.V1.AffinityArgs
                    {
                        PodAntiAffinity = new Kubernetes.Types.Inputs.Core.V1.PodAntiAffinityArgs
                        {
                            RequiredDuringSchedulingIgnoredDuringExecution = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.PodAffinityTermArgs
                                {
                                    LabelSelector = new Kubernetes.Types.Inputs.Meta.V1.LabelSelectorArgs
                                    {
                                        MatchLabels = 
                                        {
                                            { "k8s-app", "cilium" },
                                        },
                                    },
                                    TopologyKey = "kubernetes.io/hostname",
                                },
                            },
                        },
                    },
                    AutomountServiceAccountToken = true,
                    Containers = new[]
                    {
                        new Kubernetes.Types.Inputs.Core.V1.ContainerArgs
                        {
                            Args = new[]
                            {
                                "--config-dir=/tmp/cilium/config-map",
                            },
                            Command = new[]
                            {
                                "cilium-agent",
                            },
                            Env = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "K8S_NODE_NAME",
                                    ValueFrom = new Kubernetes.Types.Inputs.Core.V1.EnvVarSourceArgs
                                    {
                                        FieldRef = new Kubernetes.Types.Inputs.Core.V1.ObjectFieldSelectorArgs
                                        {
                                            ApiVersion = "v1",
                                            FieldPath = "spec.nodeName",
                                        },
                                    },
                                },
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "CILIUM_K8S_NAMESPACE",
                                    ValueFrom = new Kubernetes.Types.Inputs.Core.V1.EnvVarSourceArgs
                                    {
                                        FieldRef = new Kubernetes.Types.Inputs.Core.V1.ObjectFieldSelectorArgs
                                        {
                                            ApiVersion = "v1",
                                            FieldPath = "metadata.namespace",
                                        },
                                    },
                                },
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "CILIUM_CLUSTERMESH_CONFIG",
                                    Value = "/var/lib/cilium/clustermesh/",
                                },
                            },
                            Image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35",
                            ImagePullPolicy = "IfNotPresent",
                            Lifecycle = new Kubernetes.Types.Inputs.Core.V1.LifecycleArgs
                            {
                                PreStop = new Kubernetes.Types.Inputs.Core.V1.LifecycleHandlerArgs
                                {
                                    Exec = new Kubernetes.Types.Inputs.Core.V1.ExecActionArgs
                                    {
                                        Command = new[]
                                        {
                                            "/cni-uninstall.sh",
                                        },
                                    },
                                },
                            },
                            LivenessProbe = new Kubernetes.Types.Inputs.Core.V1.ProbeArgs
                            {
                                FailureThreshold = 10,
                                HttpGet = new Kubernetes.Types.Inputs.Core.V1.HTTPGetActionArgs
                                {
                                    Host = "127.0.0.1",
                                    HttpHeaders = new[]
                                    {
                                        new Kubernetes.Types.Inputs.Core.V1.HTTPHeaderArgs
                                        {
                                            Name = "brief",
                                            Value = "true",
                                        },
                                    },
                                    Path = "/healthz",
                                    Port = 9879,
                                    Scheme = "HTTP",
                                },
                                PeriodSeconds = 30,
                                SuccessThreshold = 1,
                                TimeoutSeconds = 5,
                            },
                            Name = "cilium-agent",
                            ReadinessProbe = new Kubernetes.Types.Inputs.Core.V1.ProbeArgs
                            {
                                FailureThreshold = 3,
                                HttpGet = new Kubernetes.Types.Inputs.Core.V1.HTTPGetActionArgs
                                {
                                    Host = "127.0.0.1",
                                    HttpHeaders = new[]
                                    {
                                        new Kubernetes.Types.Inputs.Core.V1.HTTPHeaderArgs
                                        {
                                            Name = "brief",
                                            Value = "true",
                                        },
                                    },
                                    Path = "/healthz",
                                    Port = 9879,
                                    Scheme = "HTTP",
                                },
                                PeriodSeconds = 30,
                                SuccessThreshold = 1,
                                TimeoutSeconds = 5,
                            },
                            SecurityContext = new Kubernetes.Types.Inputs.Core.V1.SecurityContextArgs
                            {
                                Capabilities = new Kubernetes.Types.Inputs.Core.V1.CapabilitiesArgs
                                {
                                    Add = new[]
                                    {
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
                                        "SETUID",
                                    },
                                    Drop = new[]
                                    {
                                        "ALL",
                                    },
                                },
                                SeLinuxOptions = new Kubernetes.Types.Inputs.Core.V1.SELinuxOptionsArgs
                                {
                                    Level = "s0",
                                    Type = "spc_t",
                                },
                            },
                            StartupProbe = new Kubernetes.Types.Inputs.Core.V1.ProbeArgs
                            {
                                FailureThreshold = 105,
                                HttpGet = new Kubernetes.Types.Inputs.Core.V1.HTTPGetActionArgs
                                {
                                    Host = "127.0.0.1",
                                    HttpHeaders = new[]
                                    {
                                        new Kubernetes.Types.Inputs.Core.V1.HTTPHeaderArgs
                                        {
                                            Name = "brief",
                                            Value = "true",
                                        },
                                    },
                                    Path = "/healthz",
                                    Port = 9879,
                                    Scheme = "HTTP",
                                },
                                PeriodSeconds = 2,
                                SuccessThreshold = 1,
                            },
                            TerminationMessagePolicy = "FallbackToLogsOnError",
                            VolumeMounts = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/host/proc/sys/net",
                                    Name = "host-proc-sys-net",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/host/proc/sys/kernel",
                                    Name = "host-proc-sys-kernel",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/sys/fs/bpf",
                                    MountPropagation = "HostToContainer",
                                    Name = "bpf-maps",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/var/run/cilium",
                                    Name = "cilium-run",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/host/etc/cni/net.d",
                                    Name = "etc-cni-netd",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/var/lib/cilium/clustermesh",
                                    Name = "clustermesh-secrets",
                                    ReadOnly = true,
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/lib/modules",
                                    Name = "lib-modules",
                                    ReadOnly = true,
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/run/xtables.lock",
                                    Name = "xtables-lock",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/var/lib/cilium/tls/hubble",
                                    Name = "hubble-tls",
                                    ReadOnly = true,
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/tmp",
                                    Name = "tmp",
                                },
                            },
                        },
                    },
                    HostNetwork = true,
                    InitContainers = new[]
                    {
                        new Kubernetes.Types.Inputs.Core.V1.ContainerArgs
                        {
                            Command = new[]
                            {
                                "cilium",
                                "build-config",
                            },
                            Env = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "K8S_NODE_NAME",
                                    ValueFrom = new Kubernetes.Types.Inputs.Core.V1.EnvVarSourceArgs
                                    {
                                        FieldRef = new Kubernetes.Types.Inputs.Core.V1.ObjectFieldSelectorArgs
                                        {
                                            ApiVersion = "v1",
                                            FieldPath = "spec.nodeName",
                                        },
                                    },
                                },
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "CILIUM_K8S_NAMESPACE",
                                    ValueFrom = new Kubernetes.Types.Inputs.Core.V1.EnvVarSourceArgs
                                    {
                                        FieldRef = new Kubernetes.Types.Inputs.Core.V1.ObjectFieldSelectorArgs
                                        {
                                            ApiVersion = "v1",
                                            FieldPath = "metadata.namespace",
                                        },
                                    },
                                },
                            },
                            Image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35",
                            ImagePullPolicy = "IfNotPresent",
                            Name = "config",
                            TerminationMessagePolicy = "FallbackToLogsOnError",
                            VolumeMounts = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/tmp",
                                    Name = "tmp",
                                },
                            },
                        },
                        new Kubernetes.Types.Inputs.Core.V1.ContainerArgs
                        {
                            Command = new[]
                            {
                                "sh",
                                "-ec",
                                @"cp /usr/bin/cilium-mount /hostbin/cilium-mount;
nsenter --cgroup=/hostproc/1/ns/cgroup --mount=/hostproc/1/ns/mnt ""${BIN_PATH}/cilium-mount"" $CGROUP_ROOT;
rm /hostbin/cilium-mount

",
                            },
                            Env = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "CGROUP_ROOT",
                                    Value = "/run/cilium/cgroupv2",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "BIN_PATH",
                                    Value = "/opt/cni/bin",
                                },
                            },
                            Image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35",
                            ImagePullPolicy = "IfNotPresent",
                            Name = "mount-cgroup",
                            SecurityContext = new Kubernetes.Types.Inputs.Core.V1.SecurityContextArgs
                            {
                                Capabilities = new Kubernetes.Types.Inputs.Core.V1.CapabilitiesArgs
                                {
                                    Add = new[]
                                    {
                                        "SYS_ADMIN",
                                        "SYS_CHROOT",
                                        "SYS_PTRACE",
                                    },
                                    Drop = new[]
                                    {
                                        "ALL",
                                    },
                                },
                                SeLinuxOptions = new Kubernetes.Types.Inputs.Core.V1.SELinuxOptionsArgs
                                {
                                    Level = "s0",
                                    Type = "spc_t",
                                },
                            },
                            TerminationMessagePolicy = "FallbackToLogsOnError",
                            VolumeMounts = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/hostproc",
                                    Name = "hostproc",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/hostbin",
                                    Name = "cni-path",
                                },
                            },
                        },
                        new Kubernetes.Types.Inputs.Core.V1.ContainerArgs
                        {
                            Command = new[]
                            {
                                "sh",
                                "-ec",
                                @"cp /usr/bin/cilium-sysctlfix /hostbin/cilium-sysctlfix;
nsenter --mount=/hostproc/1/ns/mnt ""${BIN_PATH}/cilium-sysctlfix"";
rm /hostbin/cilium-sysctlfix

",
                            },
                            Env = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "BIN_PATH",
                                    Value = "/opt/cni/bin",
                                },
                            },
                            Image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35",
                            ImagePullPolicy = "IfNotPresent",
                            Name = "apply-sysctl-overwrites",
                            SecurityContext = new Kubernetes.Types.Inputs.Core.V1.SecurityContextArgs
                            {
                                Capabilities = new Kubernetes.Types.Inputs.Core.V1.CapabilitiesArgs
                                {
                                    Add = new[]
                                    {
                                        "SYS_ADMIN",
                                        "SYS_CHROOT",
                                        "SYS_PTRACE",
                                    },
                                    Drop = new[]
                                    {
                                        "ALL",
                                    },
                                },
                                SeLinuxOptions = new Kubernetes.Types.Inputs.Core.V1.SELinuxOptionsArgs
                                {
                                    Level = "s0",
                                    Type = "spc_t",
                                },
                            },
                            TerminationMessagePolicy = "FallbackToLogsOnError",
                            VolumeMounts = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/hostproc",
                                    Name = "hostproc",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/hostbin",
                                    Name = "cni-path",
                                },
                            },
                        },
                        new Kubernetes.Types.Inputs.Core.V1.ContainerArgs
                        {
                            Args = new[]
                            {
                                @"mount | grep ""/sys/fs/bpf type bpf"" || mount -t bpf bpf /sys/fs/bpf
",
                            },
                            Command = new[]
                            {
                                "/bin/bash",
                                "-c",
                                "--",
                            },
                            Image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35",
                            ImagePullPolicy = "IfNotPresent",
                            Name = "mount-bpf-fs",
                            SecurityContext = new Kubernetes.Types.Inputs.Core.V1.SecurityContextArgs
                            {
                                Privileged = true,
                            },
                            TerminationMessagePolicy = "FallbackToLogsOnError",
                            VolumeMounts = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/sys/fs/bpf",
                                    MountPropagation = "Bidirectional",
                                    Name = "bpf-maps",
                                },
                            },
                        },
                        new Kubernetes.Types.Inputs.Core.V1.ContainerArgs
                        {
                            Command = new[]
                            {
                                "/init-container.sh",
                            },
                            Env = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "CILIUM_ALL_STATE",
                                    ValueFrom = new Kubernetes.Types.Inputs.Core.V1.EnvVarSourceArgs
                                    {
                                        ConfigMapKeyRef = new Kubernetes.Types.Inputs.Core.V1.ConfigMapKeySelectorArgs
                                        {
                                            Key = "clean-cilium-state",
                                            Name = "cilium-config",
                                            Optional = true,
                                        },
                                    },
                                },
                                new Kubernetes.Types.Inputs.Core.V1.EnvVarArgs
                                {
                                    Name = "CILIUM_BPF_STATE",
                                    ValueFrom = new Kubernetes.Types.Inputs.Core.V1.EnvVarSourceArgs
                                    {
                                        ConfigMapKeyRef = new Kubernetes.Types.Inputs.Core.V1.ConfigMapKeySelectorArgs
                                        {
                                            Key = "clean-cilium-bpf-state",
                                            Name = "cilium-config",
                                            Optional = true,
                                        },
                                    },
                                },
                            },
                            Image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35",
                            ImagePullPolicy = "IfNotPresent",
                            Name = "clean-cilium-state",
                            Resources = new Kubernetes.Types.Inputs.Core.V1.ResourceRequirementsArgs
                            {
                                Requests = 
                                {
                                    { "cpu", "100m" },
                                    { "memory", "100Mi" },
                                },
                            },
                            SecurityContext = new Kubernetes.Types.Inputs.Core.V1.SecurityContextArgs
                            {
                                Capabilities = new Kubernetes.Types.Inputs.Core.V1.CapabilitiesArgs
                                {
                                    Add = new[]
                                    {
                                        "NET_ADMIN",
                                        "SYS_MODULE",
                                        "SYS_ADMIN",
                                        "SYS_RESOURCE",
                                    },
                                    Drop = new[]
                                    {
                                        "ALL",
                                    },
                                },
                                SeLinuxOptions = new Kubernetes.Types.Inputs.Core.V1.SELinuxOptionsArgs
                                {
                                    Level = "s0",
                                    Type = "spc_t",
                                },
                            },
                            TerminationMessagePolicy = "FallbackToLogsOnError",
                            VolumeMounts = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/sys/fs/bpf",
                                    Name = "bpf-maps",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/run/cilium/cgroupv2",
                                    MountPropagation = "HostToContainer",
                                    Name = "cilium-cgroup",
                                },
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/var/run/cilium",
                                    Name = "cilium-run",
                                },
                            },
                        },
                        new Kubernetes.Types.Inputs.Core.V1.ContainerArgs
                        {
                            Command = new[]
                            {
                                "/install-plugin.sh",
                            },
                            Image = "quay.io/cilium/cilium:v1.14.2@sha256:6263f3a3d5d63b267b538298dbeb5ae87da3efacf09a2c620446c873ba807d35",
                            ImagePullPolicy = "IfNotPresent",
                            Name = "install-cni-binaries",
                            Resources = new Kubernetes.Types.Inputs.Core.V1.ResourceRequirementsArgs
                            {
                                Requests = 
                                {
                                    { "cpu", "100m" },
                                    { "memory", "10Mi" },
                                },
                            },
                            SecurityContext = new Kubernetes.Types.Inputs.Core.V1.SecurityContextArgs
                            {
                                Capabilities = new Kubernetes.Types.Inputs.Core.V1.CapabilitiesArgs
                                {
                                    Drop = new[]
                                    {
                                        "ALL",
                                    },
                                },
                                SeLinuxOptions = new Kubernetes.Types.Inputs.Core.V1.SELinuxOptionsArgs
                                {
                                    Level = "s0",
                                    Type = "spc_t",
                                },
                            },
                            TerminationMessagePolicy = "FallbackToLogsOnError",
                            VolumeMounts = new[]
                            {
                                new Kubernetes.Types.Inputs.Core.V1.VolumeMountArgs
                                {
                                    MountPath = "/host/opt/cni/bin",
                                    Name = "cni-path",
                                },
                            },
                        },
                    },
                    NodeSelector = 
                    {
                        { "kubernetes.io/os", "linux" },
                    },
                    PriorityClassName = "system-node-critical",
                    RestartPolicy = "Always",
                    ServiceAccount = "cilium",
                    ServiceAccountName = "cilium",
                    TerminationGracePeriodSeconds = 1,
                    Tolerations = new[]
                    {
                        new Kubernetes.Types.Inputs.Core.V1.TolerationArgs
                        {
                            Operator = "Exists",
                        },
                    },
                    Volumes = new[]
                    {
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            EmptyDir = null,
                            Name = "tmp",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/var/run/cilium",
                                Type = "DirectoryOrCreate",
                            },
                            Name = "cilium-run",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/sys/fs/bpf",
                                Type = "DirectoryOrCreate",
                            },
                            Name = "bpf-maps",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/proc",
                                Type = "Directory",
                            },
                            Name = "hostproc",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/run/cilium/cgroupv2",
                                Type = "DirectoryOrCreate",
                            },
                            Name = "cilium-cgroup",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/opt/cni/bin",
                                Type = "DirectoryOrCreate",
                            },
                            Name = "cni-path",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/etc/cni/net.d",
                                Type = "DirectoryOrCreate",
                            },
                            Name = "etc-cni-netd",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/lib/modules",
                            },
                            Name = "lib-modules",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/run/xtables.lock",
                                Type = "FileOrCreate",
                            },
                            Name = "xtables-lock",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            Name = "clustermesh-secrets",
                            Projected = new Kubernetes.Types.Inputs.Core.V1.ProjectedVolumeSourceArgs
                            {
                                DefaultMode = 400,
                                Sources = new[]
                                {
                                    new Kubernetes.Types.Inputs.Core.V1.VolumeProjectionArgs
                                    {
                                        Secret = new Kubernetes.Types.Inputs.Core.V1.SecretProjectionArgs
                                        {
                                            Name = "cilium-clustermesh",
                                            Optional = true,
                                        },
                                    },
                                    new Kubernetes.Types.Inputs.Core.V1.VolumeProjectionArgs
                                    {
                                        Secret = new Kubernetes.Types.Inputs.Core.V1.SecretProjectionArgs
                                        {
                                            Items = new[]
                                            {
                                                new Kubernetes.Types.Inputs.Core.V1.KeyToPathArgs
                                                {
                                                    Key = "tls.key",
                                                    Path = "common-etcd-client.key",
                                                },
                                                new Kubernetes.Types.Inputs.Core.V1.KeyToPathArgs
                                                {
                                                    Key = "tls.crt",
                                                    Path = "common-etcd-client.crt",
                                                },
                                                new Kubernetes.Types.Inputs.Core.V1.KeyToPathArgs
                                                {
                                                    Key = "ca.crt",
                                                    Path = "common-etcd-client-ca.crt",
                                                },
                                            },
                                            Name = "clustermesh-apiserver-remote-cert",
                                            Optional = true,
                                        },
                                    },
                                },
                            },
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/proc/sys/net",
                                Type = "Directory",
                            },
                            Name = "host-proc-sys-net",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            HostPath = new Kubernetes.Types.Inputs.Core.V1.HostPathVolumeSourceArgs
                            {
                                Path = "/proc/sys/kernel",
                                Type = "Directory",
                            },
                            Name = "host-proc-sys-kernel",
                        },
                        new Kubernetes.Types.Inputs.Core.V1.VolumeArgs
                        {
                            Name = "hubble-tls",
                            Projected = new Kubernetes.Types.Inputs.Core.V1.ProjectedVolumeSourceArgs
                            {
                                DefaultMode = 400,
                                Sources = new[]
                                {
                                    new Kubernetes.Types.Inputs.Core.V1.VolumeProjectionArgs
                                    {
                                        Secret = new Kubernetes.Types.Inputs.Core.V1.SecretProjectionArgs
                                        {
                                            Items = new[]
                                            {
                                                new Kubernetes.Types.Inputs.Core.V1.KeyToPathArgs
                                                {
                                                    Key = "tls.crt",
                                                    Path = "server.crt",
                                                },
                                                new Kubernetes.Types.Inputs.Core.V1.KeyToPathArgs
                                                {
                                                    Key = "tls.key",
                                                    Path = "server.key",
                                                },
                                                new Kubernetes.Types.Inputs.Core.V1.KeyToPathArgs
                                                {
                                                    Key = "ca.crt",
                                                    Path = "client-ca.crt",
                                                },
                                            },
                                            Name = "hubble-server-certs",
                                            Optional = true,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            },
            UpdateStrategy = new Kubernetes.Types.Inputs.Apps.V1.DaemonSetUpdateStrategyArgs
            {
                RollingUpdate = new Kubernetes.Types.Inputs.Apps.V1.RollingUpdateDaemonSetArgs
                {
                    MaxUnavailable = 2,
                },
                Type = "RollingUpdate",
            },
        },
    });

});

