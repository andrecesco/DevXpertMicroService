# EduOnline Kubernetes Manifests

This folder contains the Kubernetes base manifests for the EduOnline platform.

## Included resources
- Namespace and shared configuration
- SQL Server persistent volume and claim
- Runtime dependencies for the current services
- API deployments and services
- Core ingress, network policies, RBAC, and HPAs
- Complementary observability templates and OpenTelemetry Collector resources
- Complementary security manifests and bootstrap artifacts
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
- `kubectl apply -k infra/kubernetes` applies the base manifest set, including the observability bundle required for the current CI validation.
- Security complements that are not part of the base flow stay isolated under `infra/kubernetes/security`.
- Local Docker Compose observability files now live under `infra/compose/observability`.
- For canonical documentation navigation, use `docs/README.md`.
- Runtime security status (operante/parcial/não aplicado) is tracked in `docs/SECURITY-COMPLIANCE-MATRIX.md`.
- The SQL Server data volume uses a local hostPath so it works on Kind and Minikube.
- The manifests assume the current services keep their existing HTTP health endpoints under `/health`.
