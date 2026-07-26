# EduOnline Kubernetes Manifests

This folder contains the Kubernetes base manifests for the EduOnline platform.

## Included resources
- Namespace and shared configuration
- SQL Server persistent volume and claim
- Runtime dependencies for the current services
- API deployments and services
- Observability templates and OpenTelemetry Collector resources
- Ingress, network policies, RBAC, and HPAs
- Local setup scripts for Kind and Minikube

## Local setup
### Kind
```powershell
.\infra\kubernetes\scripts\setup-kind.ps1
```

### Minikube
```powershell
.\infra\kubernetes\scripts\setup-minikube.ps1
```

### Apply to an existing cluster
```powershell
.\infra\kubernetes\scripts\apply.ps1
```

## Notes
- `kubectl apply -k infra/kubernetes` applies the full manifest set, including the OpenTelemetry Collector, Jaeger, Alertmanager, Elasticsearch, Fluentd, and security workloads under `infra/kubernetes/observability/` and `infra/kubernetes/security/`.
- The SQL Server data volume uses a local hostPath so it works on Kind and Minikube.
- The manifests assume the current services keep their existing HTTP health endpoints under `/health`.
