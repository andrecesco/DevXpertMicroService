# Vault-backed secrets

This folder contains the optional Vault integration path for the EduOnline platform.

## Purpose

Use this overlay in secured environments to source the shared `eduonline-secrets` secret from Vault through External Secrets Operator.

## Prerequisites

- HashiCorp Vault available and reachable from the cluster
- External Secrets Operator installed in the cluster
- Kubernetes auth configured in Vault for the `eduonline-api` role

## Apply

```bash
kubectl apply -k infra/kubernetes/vault
```

## Secret mapping

The generated Kubernetes secret keeps the same keys used by the application manifests:

- `SA_PASSWORD`
- `auth-connection-string`
- `alunos-connection-string`
- `conteudos-connection-string`
- `pagamentos-connection-string`
- `AppTokenSettings__Segredo`
- `AppSettings__Segredo`
- `RabbitMq__Password`

## Local development

The repository still keeps `infra/kubernetes/secrets.yaml` for local development and offline demos.
