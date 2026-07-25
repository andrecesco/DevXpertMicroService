# Runbook de Validação de Resiliência e Observabilidade

Este guia consolida o passo a passo para validar, de forma objetiva e reprodutível, os itens de resiliência e observabilidade do projeto.

## 1. Objetivo

Garantir evidência executável para:

- Health checks (`/health`, `/health/ready`, `/health/live`)
- Exposição de métricas (`/metrics`)
- Presença dos recursos de observabilidade no Kubernetes (`OTEL Collector`, `Jaeger`, `Alertmanager`, `Elasticsearch`, `Fluentd`)

## 2. Validação no CI (GitHub Actions)

No workflow principal (`.github/workflows/standard.yml`), a validação ocorre com dois blocos:

1. **Smoke de Observabilidade (HealthChecks)**
   - Executa testes de integração filtrados por `HealthChecksIntegrationTest`.
2. **Validar manifestos de observabilidade do Kubernetes**
   - Executa `./scripts/validate-observability.ps1`.

### Critério de sucesso no CI

- Job `build` concluído com sucesso.
- Etapas acima em status verde.

## 3. Validação local rápida

Na raiz do repositório:

```powershell
# 1) Verifica se os recursos obrigatórios de observabilidade
# estão referenciados no kustomization e existem em disco
pwsh ./scripts/validate-observability.ps1

# 2) Smoke dos endpoints de health/metrics via integração
# (Auth e Pagamentos)
dotnet test EduOnline.slnx --filter "FullyQualifiedName~HealthChecksIntegrationTest"
```

## 4. Validação operacional no Kubernetes

Após aplicar os manifests (Kind/Minikube):

```powershell
kubectl get deploy -n eduonline
kubectl get pods -n eduonline
kubectl get svc -n eduonline
```

Checklist mínimo:

- APIs com pods `Ready`
- `readinessProbe` apontando para `/health/ready`
- `livenessProbe` apontando para `/health/live`
- Serviços de observabilidade existentes no namespace (`otel-collector`, `jaeger`, `alertmanager` etc.)

## 5. Troubleshooting objetivo

Se falhar em `/health/ready` ou `/health/live`:

```powershell
kubectl describe pod <pod> -n eduonline
kubectl logs <pod> -n eduonline
```

Se falhar na validação de manifestos:

- Conferir `infra/kubernetes/kustomization.yaml`
- Conferir existência dos arquivos em `infra/kubernetes/observability/*`

## 6. Evidências recomendadas para PR

- Link do run do GitHub Actions com etapas verdes
- Saída do `scripts/validate-observability.ps1`
- Resultado do `dotnet test ... HealthChecksIntegrationTest`
