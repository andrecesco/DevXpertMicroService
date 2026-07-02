# Vault Policies para as APIs

## Path structure
```
secret/data/eduonline/
├── auth-api/
│   ├── database
│   ├── jwt-secret
│   └── oauth-providers
├── alunos-api/
│   ├── database
│   └── aws-credentials
├── conteudos-api/
│   ├── database
│   └── storage
├── pagamentos-api/
│   ├── database
│   ├── payment-gateway
│   └── stripe-key
└── bff-api/
	├── api-keys
	└── external-services
```

## Policies

### auth-api-policy.hcl
```hcl
path "secret/data/eduonline/auth-api/*" {
  capabilities = ["read", "list"]
}

path "secret/metadata/eduonline/auth-api/*" {
  capabilities = ["read", "list"]
}
```

### alunos-api-policy.hcl
```hcl
path "secret/data/eduonline/alunos-api/*" {
  capabilities = ["read", "list"]
}

path "secret/metadata/eduonline/alunos-api/*" {
  capabilities = ["read", "list"]
}
```

### conteudos-api-policy.hcl
```hcl
path "secret/data/eduonline/conteudos-api/*" {
  capabilities = ["read", "list"]
}

path "secret/metadata/eduonline/conteudos-api/*" {
  capabilities = ["read", "list"]
}
```

### pagamentos-api-policy.hcl
```hcl
path "secret/data/eduonline/pagamentos-api/*" {
  capabilities = ["read", "list"]
}

path "secret/metadata/eduonline/pagamentos-api/*" {
  capabilities = ["read", "list"]
}
```

### bff-api-policy.hcl
```hcl
path "secret/data/eduonline/bff-api/*" {
  capabilities = ["read", "list"]
}

path "secret/metadata/eduonline/bff-api/*" {
  capabilities = ["read", "list"]
}
```

### admin-policy.hcl
```hcl
# Full access to all secrets
path "secret/*" {
  capabilities = ["create", "read", "update", "delete", "list"]
}

# Access to auth methods
path "auth/token/*" {
  capabilities = ["create", "read", "update", "delete", "list"]
}

# Access to policies
path "sys/policies/*" {
  capabilities = ["create", "read", "update", "delete", "list"]
}
```

## Kubernetes Auth Method Configuration

```bash
# Enable Kubernetes auth method
vault auth enable kubernetes

# Configure Kubernetes auth
vault write auth/kubernetes/config \
  kubernetes_host="https://$KUBERNETES_HOST:$KUBERNETES_PORT" \
  kubernetes_ca_cert=@/var/run/secrets/kubernetes.io/serviceaccount/ca.crt \
  token_reviewer_jwt=@/var/run/secrets/kubernetes.io/serviceaccount/token

# Create roles for each API
vault write auth/kubernetes/role/auth-api \
  bound_service_account_names=auth-api \
  bound_service_account_namespaces=eduonline \
  policies=auth-api \
  ttl=1h

vault write auth/kubernetes/role/alunos-api \
  bound_service_account_names=alunos-api \
  bound_service_account_namespaces=eduonline \
  policies=alunos-api \
  ttl=1h

vault write auth/kubernetes/role/conteudos-api \
  bound_service_account_names=conteudos-api \
  bound_service_account_namespaces=eduonline \
  policies=conteudos-api \
  ttl=1h

vault write auth/kubernetes/role/pagamentos-api \
  bound_service_account_names=pagamentos-api \
  bound_service_account_namespaces=eduonline \
  policies=pagamentos-api \
  ttl=1h

vault write auth/kubernetes/role/bff-api \
  bound_service_account_names=bff-api \
  bound_service_account_namespaces=eduonline \
  policies=bff-api \
  ttl=1h
```
