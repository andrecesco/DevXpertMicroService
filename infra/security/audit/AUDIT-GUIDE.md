# Audit Logging Guide

## Visão Geral

Audit logging registra solicitações do Kubernetes API Server, útil para:
- Conformidade e compliance
- Análise de segurança
- Investigação de incidentes
- Rastreamento de mudanças

## Audit Policy

O arquivo `audit-policy.yaml` define quais eventos são registrados e em qual nível:

### Níveis de Log

1. **None**: Não registra
2. **Metadata**: Log do usuário, timestamp, recurso (sem corpo da requisição)
3. **Request**: Metadata + corpo da requisição
4. **RequestResponse**: Metadata + request body + response body

### Eventos Registrados

- **Pods exec/portforward**: RequestResponse (segurança)
- **Secrets**: RequestResponse (acesso a dados sensíveis)
- **ConfigMaps**: RequestResponse (mudanças de configuração)
- **RBAC changes**: RequestResponse (mudanças de permissões)
- **Autenticação falhada**: Metadata
- **Todos os outros**: Metadata

## Configuração por Cluster

### Kind

Criar arquivo `kind-config.yaml`:

```yaml
apiVersion: kind.x-k8s.io/v1alpha4
kind: Cluster
metadata:
  name: eduonline
nodes:
- role: control-plane
  extraMounts:
  - hostPath: $(pwd)/infra/security/audit/audit-policy.yaml
	containerPath: /etc/kubernetes/audit-policy.yaml
  - hostPath: /tmp/audit-logs
	containerPath: /var/log/kubernetes
  kubeadmConfigPatches:
  - |
	kind: ClusterConfiguration
	apiServer:
	  extraArgs:
		audit-policy-file: /etc/kubernetes/audit-policy.yaml
		audit-log-path: /var/log/kubernetes/audit.log
		audit-log-maxage: 30
		audit-log-maxbackup: 10
```

Criar cluster:
```bash
kind create cluster -f kind-config.yaml
```

### Minikube

```bash
# Setup
minikube ssh "mkdir -p /var/log/kubernetes"
minikube cp infra/security/audit/audit-policy.yaml /etc/kubernetes/audit-policy.yaml

# Editar API server
minikube ssh "sudo vi /etc/kubernetes/manifests/kube-apiserver.yaml"

# Adicionar args:
# - --audit-policy-file=/etc/kubernetes/audit-policy.yaml
# - --audit-log-path=/var/log/kubernetes/audit.log
# - --audit-log-maxage=30
# - --audit-log-maxbackup=10
```

## Verificação

### Ver logs em Kind
```bash
# Obter nome do container control-plane
kubectl get nodes

# SSH no container
kind exec <cluster-name> -it <node> -- tail -f /var/log/kubernetes/audit.log
```

### Ver logs em Minikube
```bash
minikube ssh "tail -f /var/log/kubernetes/audit.log"
```

### Buscar eventos específicos
```bash
# Acessos a secrets
grep 'secrets' /var/log/kubernetes/audit.log | jq '.'

# Mudanças em RBAC
grep 'rolebindings\|clusterrolebindings' /var/log/kubernetes/audit.log | jq '.verb, .user'

# Pods exec
grep 'pods/exec' /var/log/kubernetes/audit.log | jq '.user, .sourceIPs'
```

## Análise de Logs

### Estrutura de um evento
```json
{
  "kind": "Event",
  "apiVersion": "audit.k8s.io/v1",
  "level": "RequestResponse",
  "auditID": "...",
  "stage": "ResponseComplete",
  "requestReceivedTimestamp": "2024-01-15T10:30:00.000Z",
  "stageTimestamp": "2024-01-15T10:30:00.100Z",
  "user": {
	"username": "...",
	"uid": "...",
	"groups": [...]
  },
  "verb": "get",
  "apiVersion": "v1",
  "objectRef": {
	"resource": "secrets",
	"namespace": "eduonline",
	"name": "..."
  },
  "sourceIPs": ["..."],
  "userAgent": "...",
  "responseStatus": {
	"code": 200,
	"reason": "OK"
  },
  "requestObject": {...},
  "responseObject": {...}
}
```

## Integração com Observabilidade

### Enviar logs para Elasticsearch
```bash
# Instalar Logstash
# Configurar pipeline para ler /var/log/kubernetes/audit.log
# Enviar para Elasticsearch
```

### Query no Kibana
```json
{
  "query": {
	"bool": {
	  "must": [
		{ "match": { "verb": "delete" } },
		{ "match": { "objectRef.resource": "secrets" } }
	  ]
	}
  }
}
```

## Retention Policy

### Configurações
- `audit-log-maxage`: Máximo de dias para manter logs (padrão: 0 = indefinido)
- `audit-log-maxbackup`: Máximo número de backup files (padrão: 0 = indefinido)

### Limpeza manual
```bash
# Remover logs antigos
find /var/log/kubernetes -name "audit.log*" -mtime +30 -delete
```

## Segurança de Logs

1. **Acesso restrito**: Apenas admins devem ter acesso aos logs
2. **Backup seguro**: Copiar logs para local seguro
3. **Imutabilidade**: Considerar integrar com blockchain ou WORM storage
4. **Criptografia**: Considerar criptografar logs em repouso

## Troubleshooting

### Logs vazios
```bash
# Verificar se audit policy existe
kubectl exec -it <control-plane> -- ls -la /etc/kubernetes/audit-policy.yaml

# Verificar se API server está iniciado
kubectl logs -n kube-system -l component=kube-apiserver
```

### Disco cheio
```bash
# Verificar tamanho de logs
du -sh /var/log/kubernetes

# Limpar logs antigos
find /var/log/kubernetes -name "audit.log*" -mtime +7 -delete
```

### Performance degradado
- Aumentar `audit-log-maxbackup`
- Reduzir verbosidade da policy
- Usar log rotation

## Compliance

### GDPR
- Audit logs podem conter dados pessoais
- Implementar retenção apropriada
- Permitir acesso a usuários sobre seus logs

### HIPAA
- Logs de acesso a dados sensíveis
- Manter trail de todas as mudanças
- Auditoria regular

### PCI-DSS
- Logs de autenticação
- Logs de modificações
- Proteger integridade de logs

## Próximos Passos

1. Implementar log aggregation (Fluentd → Elasticsearch)
2. Criar dashboards de audit no Kibana
3. Configurar alertas para atividades suspeitas
4. Implementar policy enforcement (OPA/Gatekeeper)
5. Automatizar análise de logs com SIEM
