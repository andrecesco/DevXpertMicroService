# Security Compliance Matrix

| Requisito | Status | Implementação |
| --- | --- | --- |
| Secrets managed no Vault | Parcial | `infra/kubernetes/security/vault/secretstore.yaml` e `external-secret.yaml` conectam `eduonline-secrets` ao Vault; bootstrap/role validation still depends on cluster runtime. |
| RBAC restritivo implementado | Concluído | Workloads use dedicated `ServiceAccount`s and the security base includes the RBAC policies. |
| Network policies bloqueando tráfego não-autorizado | Concluído | `infra/kubernetes/security/network-policies/network-policies.yaml` plus the base deny/allow policies. |
| Segurança de pods aplicada | Concluído | `PodSecurityPolicy` removida (API extinta no K8s 1.25+). Substituída por **Pod Security Admission** nível `restricted` via labels no `infra/kubernetes/namespace.yaml` + `securityContext` restritivo em todos os Deployments (`runAsNonRoot`, `drop: ALL`, `allowPrivilegeEscalation: false`, `seccompProfile`). |
| Audit logging habilitado | Parcial | `infra/kubernetes/security/audit/configmap.yaml` and `kind-config.yaml` provide the policy and bootstrap settings; cluster control-plane enablement is still required. |
| TLS/mTLS certificates | Parcial | `infra/kubernetes/security/tls/clusterissuer.yaml`, `certificates.yaml`, and `ingress-tls.yaml` provide the CA and cert resources; workload rollout/validation remains. |
| Container image scanning | Concluído | `.github/workflows/security.yml` runs Trivy image scanning and uploads SARIF results. |
| Compliance matrix coverage | Concluído | This document now reflects the current security posture and remaining runtime validations. |
