# Guia de RBAC - Kubernetes Role-Based Access Control

## Visão Geral

Este guia documenta a estratégia de RBAC implementada no cluster EduOnline Kubernetes.

## Hierarquia de Roles

```
├── Admin (Full Access)
│   └── Todas as operações em todos os recursos
├── Developer (Read-Only)
│   └── List, Watch, Get de pods, deployments, services, configmaps
└── Service Accounts (API-Specific)
	├── auth-api (Read secrets, configmaps)
	├── alunos-api (Read secrets, configmaps)
	├── conteudos-api (Read secrets, configmaps)
	├── pagamentos-api (Read secrets, configmaps)
	└── bff-api (Read secrets, configmaps)
```

## Aplicação

### 1. Aplicar todos os RBAC
```bash
chmod +x infra/security/rbac/apply-rbac.sh
./infra/security/rbac/apply-rbac.sh
```

### 2. Aplicar manualmente
```bash
# APIs
kubectl apply -f infra/security/rbac/auth-api-rbac.yaml
kubectl apply -f infra/security/rbac/alunos-api-rbac.yaml
kubectl apply -f infra/security/rbac/conteudos-api-rbac.yaml
kubectl apply -f infra/security/rbac/pagamentos-api-rbac.yaml
kubectl apply -f infra/security/rbac/bff-api-rbac.yaml

# Admin e Developer
kubectl apply -f infra/security/rbac/admin-rbac.yaml
kubectl apply -f infra/security/rbac/developer-rbac.yaml

# Pod Security Policies
kubectl apply -f infra/security/rbac/pod-security-policies.yaml
```

## Verificação

### Verificar ServiceAccounts
```bash
kubectl get serviceaccounts -n eduonline
```

### Verificar Roles
```bash
kubectl get roles -n eduonline
```

### Verificar RoleBindings
```bash
kubectl get rolebindings -n eduonline
```

### Verificar Permissões
```bash
# Testar se auth-api pode listar pods
kubectl auth can-i list pods --as=system:serviceaccount:eduonline:auth-api -n eduonline

# Testar se auth-api pode deletar pods (deve retornar "no")
kubectl auth can-i delete pods --as=system:serviceaccount:eduonline:auth-api -n eduonline

# Testar se developer pode listar pods (deve retornar "yes")
kubectl auth can-i list pods --as=system:serviceaccount:eduonline:developer -n eduonline

# Testar se developer pode deletar pods (deve retornar "no")
kubectl auth can-i delete pods --as=system:serviceaccount:eduonline:developer -n eduonline
```

## Permissões por Role

### Service Accounts (APIs)
- `get` configmaps, secrets
- `list` configmaps, secrets
- `watch` configmaps, secrets
- `get`, `list` services

### Developer
- `get`, `list`, `watch` pods, pods/logs, pods/portforward
- `get`, `list`, `watch` deployments, statefulsets, daemonsets
- `get`, `list`, `watch` services, configmaps
- `get`, `list`, `watch` jobs, cronjobs

### Admin
- `*` (todas as operações)

## Pod Security Policies

### Restricted
- Sem privilégios
- Sem escalação de privilégios
- Drop ALL capabilities
- Volumes permitidos: configMap, emptyDir, projected, secret, downwardAPI, PVC
- Sem host network/IPC/PID
- runAsNonRoot obrigatório
- SELinux labels requeridos

### Baseline
- Permissivo
- Compatível com a maioria das aplicações

## Troubleshooting

### "Forbidden" ao executar comandos
```bash
# Verificar qual role o SA tem
kubectl get rolebindings -n eduonline -o wide | grep <serviceaccount>

# Ver permissões específicas
kubectl get role -n eduonline <role-name> -o yaml
```

### Pod não inicia (PSP)
```bash
# Verificar logs
kubectl describe pod <pod-name> -n eduonline

# Ver PSP aplicada
kubectl get psp <psp-name> -o yaml

# Temporariamente usar baseline
kubectl label pod <pod-name> pod-security.kubernetes.io/enforce=baseline
```

## Boas Práticas

1. **Least Privilege**: Cada SA só tem permissões necessárias
2. **Read-Only**: Developer role é read-only
3. **Separation**: APIs têm SAs e roles separadas
4. **Audit**: Todas as ações são auditadas pelo Kubernetes
5. **Pod Security**: PSPs reforçam segurança em nível de pod

## Próximos Passos

1. Implementar Network Policies
2. Configurar TLS/mTLS entre serviços
3. Integrar com Vault para secrets rotation
4. Configurar audit logging
5. Implementar OPA/Gatekeeper para policy enforcement
