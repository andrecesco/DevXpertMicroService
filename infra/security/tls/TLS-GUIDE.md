# TLS/mTLS Configuration Guide

## Visão Geral

Implementação de TLS (Transport Layer Security) e mTLS (mutual TLS) para comunicação segura entre serviços no cluster Kubernetes.

## Componentes

1. **TLS para Ingress**: HTTPS para clientes externos
2. **mTLS entre Serviços**: Autenticação mútua entre APIs
3. **Certificados Auto-Assinados**: Para desenvolvimento

## Geração de Certificados

### Pré-requisitos
- OpenSSL instalado
- `openssl` no PATH

### Gerar certificados
```bash
chmod +x infra/security/tls/generate-certs.sh
./infra/security/tls/generate-certs.sh
```

Isso irá gerar:
- CA (Certificado de Autoridade)
- Certificados privados e públicos para cada serviço
- Arquivos P12 para aplicações Java/.NET

### Criar Kubernetes Secrets
```bash
chmod +x infra/security/tls/create-tls-secrets.sh
./infra/security/tls/create-tls-secrets.sh
```

Isso irá criar:
- `auth-api-tls` secret
- `alunos-api-tls` secret
- `conteudos-api-tls` secret
- `pagamentos-api-tls` secret
- `bff-api-tls` secret
- `ca-cert` ConfigMap
- Outros secrets para componentes

## TLS para Ingress

### Aplicar Ingress com TLS
```bash
kubectl apply -f infra/security/tls/ingress-tls.yaml
```

### Testar HTTPS
```bash
# Obter endereço do Ingress
kubectl get ingress -n eduonline

# Testar com curl (ignorando certificado auto-assinado)
curl -k https://api.eduonline.local/health
curl -k https://auth.eduonline.local/health
```

## mTLS entre Serviços

### Configuração em .NET

#### Habilitando mTLS no Kestrel
```csharp
services.AddKestrel(options =>
{
	options.ConfigureHttpsDefaults(https =>
	{
		https.ServerCertificate = new X509Certificate2(
			"/var/run/secrets/tls/tls.crt",
			"/var/run/secrets/tls/tls.key"
		);
	});
});
```

#### Configurar cliente com certificado
```csharp
var handler = new HttpClientHandler();
handler.ClientCertificateOptions = ClientCertificateOption.Manual;
handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

var cert = new X509Certificate2("/var/run/secrets/ca/ca.crt");
handler.ClientCertificates.Add(cert);

var client = new HttpClient(handler)
{
	BaseAddress = new Uri("https://auth-api:5000")
};
```

### Atualizar Deployment para usar TLS

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: auth-api
  namespace: eduonline
spec:
  template:
	spec:
	  containers:
	  - name: auth-api
		volumeMounts:
		- name: tls-certs
		  mountPath: /var/run/secrets/tls
		  readOnly: true
		- name: ca-cert
		  mountPath: /var/run/secrets/ca
		  readOnly: true
	  volumes:
	  - name: tls-certs
		secret:
		  secretName: auth-api-tls
	  - name: ca-cert
		configMap:
		  name: ca-cert
```

## Verificação

### Ver secrets TLS
```bash
kubectl get secrets -n eduonline -l type=tls
```

### Ver certificados
```bash
kubectl get secret auth-api-tls -n eduonline -o jsonpath='{.data.tls\.crt}' | base64 -d | openssl x509 -text -noout
```

### Testar conectividade mTLS
```bash
# Enter pod
kubectl exec -it auth-api-pod -n eduonline -- /bin/bash

# Test mTLS to another service
curl --cacert /var/run/secrets/ca/ca.crt \
	 --cert /var/run/secrets/tls/tls.crt \
	 --key /var/run/secrets/tls/tls.key \
	 https://alunos-api:5002/health
```

## Rotação de Certificados

### Manual
```bash
# Gerar novos certificados
./generate-certs.sh

# Criar novos secrets
./create-tls-secrets.sh

# Restart pods
kubectl rollout restart deployment/auth-api -n eduonline
```

### Automática (com cert-manager)
```bash
# Instalar cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Criar ClusterIssuer para self-signed
kubectl apply -f - <<EOF
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: selfsigned-issuer
spec:
  selfSigned: {}
EOF

# Usar no Ingress (annotations)
# cert-manager.io/cluster-issuer: selfsigned-issuer
```

## Troubleshooting

### Certificado expirado
```bash
# Verificar validade
openssl x509 -in certs/auth-api.pem -text -noout | grep -A2 "Not"

# Regenerar
./generate-certs.sh
```

### Erro de certificado inválido
```bash
# Verificar SAN (Subject Alternative Name)
openssl x509 -in certs/auth-api.pem -text -noout | grep -A1 "Subject Alternative"

# Certificado deve ter SAN para DNS
```

### Conexão recusada no mTLS
```bash
# Verificar se pods têm acesso aos secrets
kubectl get secret auth-api-tls -n eduonline

# Verificar volume mounts
kubectl describe pod auth-api-pod -n eduonline | grep -A5 "Mounts"

# Testar conectividade
kubectl run -it --image=curlimages/curl debug -- sh
curl -k https://auth-api:5000/health
```

## Produção

Para produção, considere:

1. **Certificados públicos**: Use cert-manager com Let's Encrypt
2. **Rotação automática**: Configure rotação com cert-manager
3. **Auditoria**: Log de mudanças de certificados
4. **Monitoramento**: Alerte antes de expiração
5. **Backup**: Backup seguro de certificados

## Próximos Passos

1. Implementar mTLS entre todas as APIs
2. Configurar cert-manager para rotação automática
3. Integrar com Vault para key management
4. Configurar audit logging
5. Implementar service mesh (Istio) para gerenciamento avançado de TLS
