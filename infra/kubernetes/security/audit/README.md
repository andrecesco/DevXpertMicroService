# Audit Logging Bootstrap

This folder contains the audit policy and a Kind cluster configuration file that mounts the policy and enables Kubernetes API server audit logging.

## Kind
Use `kind-config.yaml` when creating a local cluster:

```powershell
kind create cluster --name eduonline --config .\infra\kubernetes\security\audit\kind-config.yaml
```

## Minikube
Use the instructions in `infra/security/audit/AUDIT-GUIDE.md` or `infra/security/audit/setup-audit.sh` to add the audit policy file and API server flags.

## Notes
- The audit policy itself is stored in `audit-policy.yaml` and also wrapped as a ConfigMap in the Kubernetes base.
- Control-plane audit logging is a cluster bootstrap concern; this folder provides the repeatable bootstrap artifacts.
