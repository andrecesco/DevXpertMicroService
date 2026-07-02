# Pod Security Policies - Guia de Configuração

## Visão Geral

Pod Security Policies (PSPs) definem regras de segurança que os pods devem cumprir para serem criados no cluster.

**Nota**: PSPs foram depreciadas no Kubernetes 1.21 e removidas no 1.25. Use Pod Security Standards (PSS) em versões mais recentes.

## PSPs Implementadas

### 1. Restricted PSP
- **Segurança**: Máxima
- **Aplicabilidade**: APIs em produção

**Restrições**:
- Sem privilégios (privileged: false)
- Sem escalação de privilégios (allowPrivilegeEscalation: false)
- Drop ALL capabilities
- Volumes: configMap, emptyDir, projected, secret, downwardAPI, PVC
- Sem host network, IPC, ou PID
- runAsNonRoot obrigatório
- Filesystem read-only (configurável)
- SELinux labels requeridos

### 2. Baseline PSP
- **Segurança**: Média
- **Aplicabilidade**: Componentes do sistema

**Permissões**:
- Sem privilégios (compatível)
- Escalação de privilégios permitida
- Todos os volumes
- runAsAny
- SELinux: RunAsAny

## Aplicação

### Via Script
```bash
chmod +x infra/security/rbac/apply-psp.sh
./infra/security/rbac/apply-psp.sh
```

### Manual
```bash
kubectl apply -f infra/security/rbac/pod-security-policies.yaml
```

## Verificação

### Ver PSPs
```bash
kubectl get psp
```

### Detalhes de uma PSP
```bash
kubectl describe psp restricted
kubectl describe psp baseline
```

### Verificar qual PSP é aplicada a um pod
```bash
kubectl get pod <pod-name> -o jsonpath='{.metadata.annotations.pod-security-policy}'
```

## Pod Security Standards (PSS) - Alternativa Moderna

Para Kubernetes >= 1.25, use Pod Security Standards ao invés de PSPs:

### Aplicar PSS a um namespace
```bash
# Restricted (máxima segurança)
kubectl label namespace eduonline pod-security.kubernetes.io/enforce=restricted

# Baseline (compatibilidade)
kubectl label namespace eduonline pod-security.kubernetes.io/enforce=baseline

# Unrestricted (sem restrições)
kubectl label namespace eduonline pod-security.kubernetes.io/enforce=unrestricted
```

### Verificar PSS
```bash
kubectl get namespace eduonline --show-labels
```

### PSS Levels

1. **Unrestricted**
   - Sem restrições de segurança
   - Padrão se não especificado

2. **Baseline**
   - Minimamente restritivo
   - Impede escalações de privilégio conhecidas
   - Permite uso de capabilities comuns

3. **Restricted**
   - Máxima segurança
   - Segue melhores práticas
   - Pode quebrar algumas aplicações

## Aplicar Restricted PSP aos Pods das APIs

### Opção 1: Label no Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: auth-api
  namespace: eduonline
spec:
  template:
	metadata:
	  labels:
		pod-security.kubernetes.io/enforce: restricted
	spec:
	  securityContext:
		runAsNonRoot: true
		runAsUser: 1000
		fsGroup: 1000
	  containers:
	  - name: api
		# ...
```

### Opção 2: Label no Namespace
```bash
kubectl label namespace eduonline pod-security.kubernetes.io/enforce=restricted
```

## Troubleshooting

### Pod rejeitado por PSP
```
Error: pods "xyz" is forbidden: unable to validate against any pod security policy: [...]
```

**Solução**:
1. Verificar qual PSP é aplicada
2. Verificar restrições da PSP vs. configuração do pod
3. Usar baseline PSP se necessário
4. Atualizar configuração do pod para cumprir PSP

### Verificar logs
```bash
kubectl logs -n kube-system -l component=kubelet
```

## Boas Práticas

1. **Sempre usar Restricted**: Máximo na produção
2. **Baseline para sistema**: Componentes do cluster
3. **Teste antes**: Validar PSP antes de enforcar
4. **Migrar para PSS**: Em versões mais recentes do K8s
5. **Audit primeiro**: Use "audit" antes de "enforce"

## Próximos Passos

1. Aplicar PSPs a todos os namespaces
2. Atualizar aplicações para cumprir Restricted PSP
3. Migrar para Pod Security Standards
4. Implementar OPA/Gatekeeper para policy enforcement avançado
5. Integrar com audit logging
