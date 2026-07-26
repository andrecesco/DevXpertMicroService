# Arquitetura EduOnline - Kubernetes Observabilidade e Segurança

## 📐 Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                      Kubernetes Cluster                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌────────────────────────── Ingress ──────────────────────┐   │
│  │  nginx-ingress (TLS)                                    │   │
│  │  - api.eduonline.local → BFF API                        │   │
│  │  - auth.eduonline.local → Auth API                      │   │
│  │  - alunos.eduonline.local → Alunos API                  │   │
│  │  - conteudos.eduonline.local → Conteudos API            │   │
│  │  - pagamentos.eduonline.local → Pagamentos API          │   │
│  └─────────────────────────────────────────────────────────┘   │
│          ↓                                                       │
│  ┌───────────────────── Aplicações ────────────────────────┐   │
│  │  BFF API (5004) → Alunos API (5002)                     │   │
│  │              ↓    ↓      ↓      ↓                        │   │
│  │     Auth API (5000) ← Conteudos API (5001)              │   │
│  │              ↓                  ↓                        │   │
│  │         Pagamentos API (5003)   ↓                        │   │
│  │              ↓        ┌─────────┘                        │   │
│  │              └────→ SQL Server (1433)                    │   │
│  └─────────────────────────────────────────────────────────┘   │
│          ↓         ↓            ↓              ↓                │
│  ┌──────────────────────── Observabilidade ───────────────┐   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌────────────┐   │   │
│  │  │ Prometheus   │→ │  Grafana     │  │ AlertMgr   │   │   │
│  │  │ (9090)       │  │  (3000)      │  │  (9093)    │   │   │
│  │  └──────────────┘  └──────────────┘  └────────────┘   │   │
│  │       ↑                   ↑                ↑              │   │
│  │  ┌────────────────────────────────────────────────────┐ │   │
│  │  │  Fluentd/Promtail (DaemonSet)                      │ │   │
│  │  │  - Coleta logs de todos os pods                   │ │   │
│  │  │  - Forward para Elasticsearch                     │ │   │
│  │  └────────────────────────────────────────────────────┘ │   │
│  │       ↓                                                   │   │
│  │  ┌────────────────┐  ┌────────────┐  ┌──────────────┐  │   │
│  │  │ Elasticsearch  │→ │   Kibana   │  │    Jaeger    │  │   │
│  │  │   (9200)       │  │  (5601)    │  │   (16686)    │  │   │
│  │  └────────────────┘  └────────────┘  └──────────────┘  │   │
│  └──────────────────────────────────────────────────────────┘  │
│          ↓                                                       │
│  ┌──────────────────────── Segurança ────────────────────────┐  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐  │  │
│  │  │   Vault      │  │  RBAC/PSP    │  │  Network      │  │  │
│  │  │   (8200)     │  │   Policies   │  │  Policies     │  │  │
│  │  └──────────────┘  └──────────────┘  └───────────────┘  │  │
│  │       ↓                   ↓                  ↓             │  │
│  │  ┌─────────────────────────────────────────────────────┐ │  │
│  │  │  TLS/mTLS  Audit Logging  Security Context         │ │  │
│  │  └─────────────────────────────────────────────────────┘ │  │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## 🏗️ Componentes Principais

### 1. Aplicações (Namespace: eduonline)

**5 Microserviços**:
- **Auth API** (5000): Autenticação e autorização
- **Alunos API** (5002): Gerenciamento de alunos
- **Conteudos API** (5001): Gerenciamento de conteúdo
- **Pagamentos API** (5003): Processamento de pagamentos
- **BFF API** (5004): Backend for Frontend

**Database**:
- **SQL Server** (1433): Banco de dados relacional

### 2. Observabilidade

#### Métricas (Prometheus)
- **Prometheus** (9090)
  - Scraping de 5 APIs + 1 Database
  - 30 dias de retention
  - 20Gi storage
  - 10+ alertas definidos

#### Visualização (Grafana)
- **Grafana** (3000)
  - 4 Dashboards principais
  - Data source: Prometheus
  - Admin: admin/GrafanaAdminPassword2026!

#### Alertas (Alertmanager)
- **Alertmanager** (9093)
  - Roteamento por severidade (critical, warning, info)
  - Integração com Slack e PagerDuty
  - Inhibit rules para evitar cascata

#### Logs (ELK Stack)
- **Elasticsearch** (9200)
  - Armazenamento de logs
  - 30Gi storage
  - Index rotation
- **Kibana** (5601)
  - Visualização de logs
  - Discover, Dashboards, Alerting
- **Fluentd/Promtail** (DaemonSet)
  - Coleta de logs de todos os pods
  - Forward para Elasticsearch

#### Tracing (Jaeger)
- **Jaeger** (16686)
  - Distributed tracing
  - 50% sampling para APIs
  - All-in-one deployment

### 3. Segurança

#### Gerenciamento de Secrets
- **Vault** (8200)
  - Centralização de secrets
  - Kubernetes auth method
  - 6 Policies (5 APIs + admin)
  - 10Gi storage

#### Controle de Acesso
- **RBAC**
  - 5 ServiceAccounts (1 por API)
  - 3 Roles (API, Admin, Developer)
  - Least privilege principle
- **Pod Security Policies**
  - Restricted: máxima segurança
  - Baseline: compatibilidade

#### Políticas de Rede
- **Network Policies** (5 policies)
  - deny-all: padrão
  - allow-dns: resolução de nomes
  - allow-inter-service: comunicação entre APIs
  - allow-ingress: entrada do BFF
  - allow-observability: acesso a monitoramento

#### Criptografia
- **TLS/mTLS**
  - Certificados auto-assinados para dev
  - Ingress com HTTPS
  - mTLS entre serviços suportado

#### Auditoria
- **Audit Logging**
  - RequestResponse para secrets/RBAC
  - Metadata para outras requisições
  - 30 dias de retention

## 📊 Fluxo de Dados

### 1. Requisição de Usuário
```
Cliente → Ingress (HTTPS) → BFF API (5004)
								↓
						Auth API (5000) ✓
								↓
			  Alunos/Conteudos/Pagamentos APIs
								↓
						   SQL Server
```

### 2. Observabilidade
```
APIs (Prometheus client lib)
  ↓
Prometheus Scraper (15s interval)
  ↓
Prometheus TSDB (20Gi storage)
  ↓
Grafana (visualization)
  ↓
Alertmanager (if threshold exceeded)
  ↓
Slack/PagerDuty (notifications)
```

### 3. Logging
```
Pods (stdout/stderr)
  ↓
kubelet (log collection)
  ↓
Fluentd DaemonSet (tail plugin)
  ↓
Elasticsearch (indexing)
  ↓
Kibana (visualization)
  ↓
Elasticsearch index retention (30d)
```

### 4. Tracing
```
API requests (OpenTelemetry instrumentation)
  ↓
Jaeger Agent (UDP 6831-6833)
  ↓
Jaeger Collector (aggregation)
  ↓
Jaeger Query (16686)
  ↓
Frontend (trace visualization)
```

### 5. Security
```
Credentials (in app)
  ↓
Vault Auth (Kubernetes auth method)
  ↓
Vault Policy Enforcement
  ↓
Secrets rotation (if configured)
  ↓
RBAC/Network Policies (enforcement)
  ↓
Audit logs (Elasticsearch)
```

## 💾 Storage

### PersistentVolumeClaims

| Component | Size | Access Mode | Type |
|-----------|------|-------------|------|
| Prometheus | 20Gi | ReadWriteOnce | standard |
| Elasticsearch | 30Gi | ReadWriteOnce | standard |
| Vault | 10Gi | ReadWriteOnce | standard |
| **Total** | **60Gi** | | |

### ConfigMaps/Secrets

| Component | Type | Content |
|-----------|------|---------|
| Prometheus | ConfigMap | prometheus.yml, rules.yaml |
| Grafana | ConfigMap | datasources, dashboards |
| Alertmanager | ConfigMap + Secret | config, webhooks |
| Fluentd | ConfigMap | fluent.conf |
| Vault | ConfigMap | vault.hcl |
| TLS | Secret | certificates |

## 🔐 Matriz de Permissões RBAC

| Role | Resources | Verbs | Scope |
|------|-----------|-------|-------|
| auth-api | secrets, configmaps | get, list, watch | Namespace |
| alunos-api | secrets, configmaps | get, list, watch | Namespace |
| conteudos-api | secrets, configmaps | get, list, watch | Namespace |
| pagamentos-api | secrets, configmaps | get, list, watch | Namespace |
| bff-api | secrets, configmaps | get, list, watch | Namespace |
| developer | pods, deployments, services | get, list, watch | Namespace |
| admin | * | * | Cluster |

## 📈 Métricas e Alertas

### Métricas Coletadas

**Application**:
- http_requests_total - Total de requisições
- http_request_duration_seconds - Duração de requisições
- http_requests_in_flight - Requisições em progresso

**Container**:
- container_cpu_usage_seconds_total - CPU usage
- container_memory_usage_bytes - Memory usage
- container_network_transmit_bytes_total - Network I/O

**Kubernetes**:
- kube_pod_container_status_restarts_total - Pod restarts
- kube_node_status_condition - Node status
- kubelet_volume_stats_used_bytes - Volume usage

### Alertas Configurados

| Alert | Condition | Duration | Severity |
|-------|-----------|----------|----------|
| APIDown | up == 0 | 2m | critical |
| HighErrorRate | error_rate > 5% | 5m | warning |
| HighResponseTime | p95_latency > 1s | 5m | warning |
| HighCPUUsage | cpu > 80% | 5m | warning |
| HighMemoryUsage | memory > 80% | 5m | warning |
| DBConnectionPoolNearExhaustion | connections > 80% | 5m | warning |
| PodRestart | restart_rate > 0 | 5m | warning |
| NodeNotReady | node_status != ready | 5m | critical |
| PVSpaceLow | pv_usage > 80% | 5m | warning |

## 📊 Dashboards Grafana

### 1. API Metrics Dashboard
- Request rate (req/s)
- Request duration (P95)
- Error rate (%)
- Active requests

### 2. Database Dashboard
- Active connections
- Query execution time
- Database size
- Transactions per minute

### 3. Kubernetes Dashboard
- Node status
- Pod restarts
- CPU usage by pod
- Memory usage by pod
- Volume usage

### 4. Business Metrics Dashboard
- Total users
- Active users (24h)
- Course enrollments
- Total revenue (BRL)
- Payment success rate
- Revenue trend (7d)

## 🚀 Escalabilidade

### Horizontal Scaling
- APIs: 3 replicas (configurable)
- Prometheus: 1 (pode ser escalado com remote storage)
- Grafana: 1 (stateless, pode ser escalado)
- Elasticsearch: 1 node (cluster possível)
- Jaeger: 1 (pode ser distribuído)

### Resource Allocation
```
Requestos: ~4Gi memory, ~1.1 CPU cores
Limits: ~8.5Gi memory, ~2.1 CPU cores
Storage: 60Gi (PVCs)
```

### Bottlenecks e Soluções
| Bottleneck | Causa | Solução |
|-----------|-------|---------|
| Prometheus storage | 30d retention | Aumentar storage ou usar remote storage |
| Elasticsearch disk | Log volume | Índice rotation automática |
| Network I/O | Pod-to-pod chatter | Service mesh (Istio) |
| API latency | Database queries | Query optimization, caching |

## 🔄 Fluxo de Deployment

### 1. Criar namespace
```bash
kubectl create namespace eduonline
```

### 2. Aplicar RBAC
```bash
kubectl apply -f infra/security/rbac/
```

### 3. Aplicar Network Policies
```bash
kubectl apply -f infra/security/network-policies/
```

### 4. Aplicar Observabilidade
```bash
kubectl apply -f infra/observability/prometheus/
kubectl apply -f infra/observability/grafana/
kubectl apply -f infra/observability/alertmanager/
kubectl apply -f infra/observability/elasticsearch/
kubectl apply -f infra/observability/fluentd/
kubectl apply -f infra/observability/jaeger/
```

### 5. Aplicar Segurança
```bash
kubectl apply -f infra/security/vault/
kubectl apply -f infra/security/tls/
kubectl apply -f infra/security/audit/
```

### 6. Validar
```bash
./infra/observability/validate-stack.sh
./infra/security/security-test.sh
```

## 📚 Referências

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Prometheus Docs](https://prometheus.io/docs/)
- [Grafana Docs](https://grafana.com/docs/)
- [Elasticsearch Docs](https://www.elastic.co/guide/index.html)
- [Jaeger Docs](https://www.jaegertracing.io/docs/)
- [Vault Docs](https://www.vaultproject.io/docs)

## 🔄 Próximos Passos

1. Implementar instrumentação nas APIs (OpenTelemetry)
2. Configurar cert-manager para rotação de certificados
3. Implementar OPA/Gatekeeper para policy enforcement
4. Configurar backup strategy
5. Implementar service mesh (Istio/Linkerd)
6. Migrar para Kubernetes 1.27+
7. Implementar cost optimization
