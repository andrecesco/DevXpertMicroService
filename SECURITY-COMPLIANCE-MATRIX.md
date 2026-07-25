# Security Compliance Matrix

| Requisito | Status | Implementação |
| --- | --- | --- |
| Secrets managed no Vault | Não aplicado em runtime | Manifestos do Vault existem em `infra/kubernetes/security/vault/` e `infra/kubernetes/vault/`, porém **nenhum Deployment consome segredos via Vault atualmente** — as variáveis de ambiente vêm de `ConfigMap` e `Secret` padrão do Kubernetes. O Vault foi provisionado como infraestrutura futura; o ExternalSecretsOperator não está habilitado nos manifestos ativos. |
| RBAC restritivo implementado | Concluído | Workloads utilizam `ServiceAccount`s dedicadas e as políticas RBAC estão definidas em `infra/kubernetes/security/rbac/`. |
| Network policies bloqueando tráfego não-autorizado | Concluído | `infra/kubernetes/networkpolicies/` aplica abordagem `deny-all` com liberações explícitas por serviço. |
| Segurança de pods aplicada | Concluído | `PodSecurityPolicy` removida (API extinta no K8s 1.25+). Substituída por **Pod Security Admission** nível `restricted` via labels no `infra/kubernetes/namespace.yaml` + `securityContext` restritivo em todos os Deployments (`runAsNonRoot`, `drop: ALL`, `allowPrivilegeEscalation: false`, `seccompProfile`). |
| Audit logging habilitado | Parcial | `infra/kubernetes/security/audit/configmap.yaml` e `kind-config.yaml` fornecem a política e as configurações de bootstrap; a habilitação no control-plane do cluster ainda requer configuração em runtime. |
| TLS/mTLS certificates | Parcial | `infra/kubernetes/security/tls/clusterissuer.yaml`, `certificates.yaml` e `ingress-tls.yaml` fornecem os recursos de CA e certificados; a validação em ambiente real ainda depende de um cluster com cert-manager instalado. |
| Container image scanning | Concluído | `.github/workflows/security.yml` executa Trivy image scanning e publica resultados SARIF no GitHub Security. |
| Compliance matrix coverage | Concluído | Este documento reflete a postura real de segurança implementada, distinguindo o que está operante do que é infraestrutura planejada. |
