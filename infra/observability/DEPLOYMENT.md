# Guia de Deployment - Stack de Observabilidade

## Pré-requisitos
- Cluster Kubernetes rodando (Kind ou Minikube)
- kubectl configurado
- Namespace `eduonline` criado

## Ordem de Deployment

### 1. Prometheus
```bash
# Criar recursos do Prometheus
kubectl apply -f infra/observability/prometheus/rbac.yaml
kubectl apply -f infra/observability/prometheus/configmap.yaml
kubectl apply -f infra/observability/prometheus/rules.yaml
kubectl apply -f infra/observability/prometheus/pvc.yaml
kubectl apply -f infra/observability/prometheus/deployment.yaml
kubectl apply -f infra/observability/prometheus/service.yaml

# Verificar status
kubectl get deployment prometheus -n eduonline
kubectl get pod -n eduonline -l app=prometheus
```

### 2. Grafana
```bash
# Criar recursos do Grafana
kubectl apply -f infra/observability/grafana/rbac.yaml
kubectl apply -f infra/observability/grafana/datasources.yaml
kubectl apply -f infra/observability/grafana/dashboards-configmap.yaml
kubectl apply -f infra/observability/grafana/deployment.yaml
kubectl apply -f infra/observability/grafana/service.yaml

# Verificar status
kubectl get deployment grafana -n eduonline
kubectl get pod -n eduonline -l app=grafana
```

### 3. Alertmanager
```bash
# Criar recursos do Alertmanager
kubectl apply -f infra/observability/alertmanager/rbac.yaml
kubectl apply -f infra/observability/alertmanager/configmap.yaml
kubectl apply -f infra/observability/alertmanager/deployment.yaml
kubectl apply -f infra/observability/alertmanager/service.yaml

# Verificar status
kubectl get deployment alertmanager -n eduonline
kubectl get pod -n eduonline -l app=alertmanager
```

## Validação

### Script de Validação
```bash
chmod +x infra/observability/validate-stack.sh
./infra/observability/validate-stack.sh
```

### Testes Manuais

#### Prometheus
```bash
# Port forward
kubectl port-forward -n eduonline svc/prometheus 9090:9090

# Testar no browser
# http://localhost:9090
# http://localhost:9090/api/v1/targets
# http://localhost:9090/api/v1/query?query=up
```

#### Grafana
```bash
# Port forward
kubectl port-forward -n eduonline svc/grafana 3000:3000

# Acessar
# http://localhost:3000
# User: admin
# Password: GrafanaAdminPassword2026!

# Verifique:
# - Data source Prometheus conectado
# - Dashboards criados
# - Alertas configurados
```

#### Alertmanager
```bash
# Port forward
kubectl port-forward -n eduonline svc/alertmanager 9093:9093

# Testar no browser
# http://localhost:9093
# Verifique status de alertas
```

## Configuração Pós-Deployment

### 1. Atualizar Secrets do Alertmanager
```bash
# Editar secret com URLs reais
kubectl edit secret alertmanager-secrets -n eduonline

# Ou via patch
kubectl patch secret alertmanager-secrets -n eduonline \
  -p '{"data":{"slack-webhook-url":"aGR0cHM6Ly9ob29rcy5zbGFjay5jb20vc2VydmljZXMvWU9VUi9XRUJIT09LL1VSTAo="}}'
```

### 2. Habilitar Notificações
- Slack: Configure webhook em Alertmanager secret
- PagerDuty: Configure service key em Alertmanager secret
- Email: Adicionar email config ao Alertmanager ConfigMap

### 3. Validar Scraping de Métricas
```bash
# Verificar que Prometheus está scrapando das APIs
kubectl port-forward -n eduonline svc/prometheus 9090:9090
# Acessar http://localhost:9090/targets
# Todos os targets devem estar "UP"
```

### 4. Validar Dashboards
```bash
# Testar conexão com Prometheus
kubectl port-forward -n eduonline svc/grafana 3000:3000
# Acessar http://localhost:3000/dashboards
# Todos os 4 dashboards devem estar disponíveis
```

## Troubleshooting

### Pod não inicia
```bash
kubectl describe pod -n eduonline -l app=prometheus
kubectl logs -n eduonline -l app=prometheus
```

### Sem métricas no Prometheus
```bash
# Verificar target status
kubectl port-forward -n eduonline svc/prometheus 9090:9090
# Acessar http://localhost:9090/targets
# Procure por targets com status DOWN e clique para detalhes
```

### Grafana não conecta ao Prometheus
```bash
# Testar conectividade
kubectl exec -n eduonline -it $(kubectl get pod -n eduonline -l app=grafana -o jsonpath='{.items[0].metadata.name}') -- curl http://prometheus:9090/api/v1/query?query=up
```

## Cleanup
```bash
# Remover stack de observabilidade
kubectl delete -f infra/observability/alertmanager/
kubectl delete -f infra/observability/grafana/
kubectl delete -f infra/observability/prometheus/
```

## Próximos Passos
1. Implementar Fluentd para log aggregation
2. Implementar Elasticsearch e Kibana
3. Implementar Jaeger para distributed tracing
4. Adicionar métricas customizadas nas APIs
5. Configurar alertas específicos do negócio
