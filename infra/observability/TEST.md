# 🧪 Teste da Stack de Observabilidade

## Pré-requisitos
- Cluster Kubernetes rodando
- Todos os componentes deployados:
  - Prometheus
  - Grafana
  - Alertmanager
  - Elasticsearch
  - Kibana
  - Fluentd/Promtail
  - Jaeger
  - APIs (auth-api, alunos-api, conteudos-api, pagamentos-api, bff-api)

## Plano de Teste

### 1. Teste de Conectividade

```bash
# Verificar se todos os pods estão rodando
kubectl get pods -n eduonline -l component=observability

# Verificar services
kubectl get svc -n eduonline -l component=observability
```

### 2. Teste de Prometheus

#### Port Forward
```bash
kubectl port-forward -n eduonline svc/prometheus 9090:9090
```

#### Verificações
- Acessar http://localhost:9090
- Ir para "Targets" e verificar que todos os targets estão "UP"
- Testar queries:
  ```
  up{job="prometheus"}  # Prometheus está up
  rate(http_requests_total[5m])  # Taxa de requisições
  container_memory_usage_bytes{pod=~".*-api"}  # Memória das APIs
  ```

### 3. Teste de Grafana

#### Port Forward
```bash
kubectl port-forward -n eduonline svc/grafana 3000:3000
```

#### Login
- URL: http://localhost:3000
- User: admin
- Password: GrafanaAdminPassword2026!

#### Verificações
1. **Data Source**
   - Settings → Data Sources
   - Verificar que Prometheus está conectado
   - Clique em "Test" para validar

2. **Dashboards**
   - Verifique se os 4 dashboards estão presentes:
	 - API Metrics Dashboard
	 - Database Dashboard
	 - Kubernetes Dashboard
	 - Business Metrics Dashboard
   - Cada dashboard deve mostrar gráficos

### 4. Teste de Kibana/Elasticsearch

#### Port Forwards
```bash
# Elasticsearch
kubectl port-forward -n eduonline svc/elasticsearch 9200:9200

# Kibana
kubectl port-forward -n eduonline svc/kibana 5601:5601
```

#### Verificações
1. **Elasticsearch**
   - Acessar http://localhost:9200/_cluster/health
   - Verificar status "green"

2. **Kibana**
   - Acessar http://localhost:5601
   - Ir para "Stack Management" → "Index Patterns"
   - Criar padrão "kubernetes-logs-*"
   - Ir para "Discover" e buscar logs das APIs

### 5. Teste de Jaeger

#### Port Forward
```bash
kubectl port-forward -n eduonline svc/jaeger 16686:16686
```

#### Verificações
- Acessar http://localhost:16686
- Verificar serviços disponíveis no dropdown
- Procurar por traces recentes
- Clicar em um trace para ver detalhes

### 6. Teste de Alertmanager

#### Port Forward
```bash
kubectl port-forward -n eduonline svc/alertmanager 9093:9093
```

#### Verificações
- Acessar http://localhost:9093
- Verificar que não há alertas não resolvidos (a menos que você tenha disparado algum)

## Teste de Carga

### Script Automatizado
```bash
chmod +x infra/observability/test-load.sh
./infra/observability/test-load.sh
```

### Teste Manual
```bash
# Port forward para BFF API
kubectl port-forward -n eduonline svc/bff-api 5004:5004

# Gerar tráfego com Apache Bench (ab)
ab -n 1000 -c 10 http://localhost:5004/health

# Ou com wrk
wrk -t4 -c100 -d1m http://localhost:5004/health

# Ou com curl em loop
for i in {1..100}; do curl http://localhost:5004/health; done
```

## Verificação de Métricas

### Prometheus Queries para Validar

```promql
# 1. APIs estão respondendo?
up{job=~".*-api"}

# 2. Taxa de requisições
sum(rate(http_requests_total[5m])) by (job)

# 3. Latência P95
histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket[5m])) by (job, le))

# 4. Taxa de erros
sum(rate(http_requests_total{status=~"5.."}[5m])) by (job)

# 5. Uso de memória
sum(container_memory_usage_bytes) by (pod_name)

# 6. Uso de CPU
sum(rate(container_cpu_usage_seconds_total[5m])) by (pod_name)

# 7. Status dos nós
kube_node_status_condition{condition="Ready"}

# 8. Pods restarted
rate(kube_pod_container_status_restarts_total[15m])
```

## Checklist de Validação

- [ ] Todos os pods estão em estado "Running"
- [ ] Prometheus está scrapeando métricas das 5 APIs
- [ ] Grafana conecta ao Prometheus
- [ ] 4 Dashboards estão criados e mostrando dados
- [ ] Elasticsearch está "green"
- [ ] Kibana consegue acessar Elasticsearch
- [ ] Logs estão sendo coletados por Fluentd
- [ ] Jaeger está recebendo traces
- [ ] Alertmanager está funcionando
- [ ] Teste de carga gera tráfego sem erros críticos
- [ ] Métricas aparecem no Prometheus durante teste de carga
- [ ] Dashboards atualizam com dados de teste
- [ ] Logs aparecem no Kibana
- [ ] Traces aparecem no Jaeger

## Troubleshooting

### Prometheus sem dados
```bash
# Verificar logs
kubectl logs -n eduonline -l app=prometheus

# Verificar ConfigMap
kubectl get configmap -n eduonline prometheus-config -o yaml

# Verificar targets
kubectl port-forward -n eduonline svc/prometheus 9090:9090
# Acessar http://localhost:9090/targets e procure por "DOWN"
```

### Grafana sem data source
```bash
# Resetar datasources
kubectl delete pod -n eduonline -l app=grafana

# Recriar ConfigMap se necessário
kubectl apply -f infra/observability/grafana/datasources.yaml
```

### Elasticsearch sem dados
```bash
# Verificar conectividade
kubectl exec -n eduonline -it $(kubectl get pod -n eduonline -l app=elasticsearch -o jsonpath='{.items[0].metadata.name}') -- curl http://localhost:9200/_cat/indices

# Verificar Fluentd logs
kubectl logs -n eduonline -l app=fluentd
```

### Jaeger sem traces
```bash
# Verificar se APIs têm OpenTelemetry SDK configurado
# Verificar logs do Jaeger
kubectl logs -n eduonline -l app=jaeger

# Verificar conexão da API
kubectl exec -n eduonline -it $(kubectl get pod -n eduonline -l app=jaeger -o jsonpath='{.items[0].metadata.name}') -- netstat -tlnp
```

## Próximos Passos

1. **Adicionar instrumentação nas APIs**
   - Application Insights ou OpenTelemetry SDK
   - Adicionar custom metrics

2. **Configurar alertas reais**
   - Atualizar Alertmanager com Slack webhook real
   - Definir alertas específicos do negócio

3. **Configurar retention de dados**
   - Prometheus: ajustar `--storage.tsdb.retention.time`
   - Elasticsearch: ILM (Index Lifecycle Management)

4. **Performance tuning**
   - Aumentar replicas em produção
   - Usar StatefulSet para Elasticsearch
   - Configurar persistent storage real

5. **Security hardening**
   - Habilitar autenticação no Kibana
   - Configurar RBAC para Elasticsearch
   - Usar secrets para credenciais
