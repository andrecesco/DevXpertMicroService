#!/bin/bash

# Script para inicializar Vault
# Usage: ./init-vault.sh

set -e

NAMESPACE="eduonline"
VAULT_POD=$(kubectl get pod -n ${NAMESPACE} -l app=vault -o jsonpath='{.items[0].metadata.name}')
VAULT_PORT=8200

echo "Inicializando Vault..."
echo ""

# Port forward
echo "Configurando port forward..."
kubectl port-forward -n ${NAMESPACE} ${VAULT_POD} ${VAULT_PORT}:${VAULT_PORT} &
PF_PID=$!
sleep 2

# Initialize Vault
echo "Inicializando Vault..."
INIT_OUTPUT=$(curl -s -X POST http://localhost:${VAULT_PORT}/v1/sys/init \
  -H "Content-Type: application/json" \
  -d '{"secret_shares": 5, "secret_threshold": 3}')

echo "$INIT_OUTPUT" > vault-init.json

# Extract keys and token
KEYS=$(echo "$INIT_OUTPUT" | jq -r '.keys[]')
UNSEAL_KEY1=$(echo "$INIT_OUTPUT" | jq -r '.keys[0]')
ROOT_TOKEN=$(echo "$INIT_OUTPUT" | jq -r '.root_token')

echo ""
echo "=== IMPORTANTE ==="
echo "Guarde estas informações em um local seguro!"
echo ""
echo "Root Token:"
echo "$ROOT_TOKEN"
echo ""
echo "Unseal Keys:"
echo "$KEYS"
echo ""
echo "Arquivo salvo em: vault-init.json"
echo ""

# Unseal Vault (usando 3 das 5 chaves)
echo "Desselando Vault..."
for key in $(echo "$INIT_OUTPUT" | jq -r '.keys[0:3][]'); do
	curl -s -X POST http://localhost:${VAULT_PORT}/v1/sys/unseal \
	  -H "Content-Type: application/json" \
	  -d "{\"key\": \"$key\"}" > /dev/null
	echo "  ✓ Unseal com chave aplicada"
done

sleep 2

# Check Vault status
echo ""
echo "Status do Vault:"
curl -s http://localhost:${VAULT_PORT}/v1/sys/health | jq .

# Login
echo ""
echo "Login com Root Token..."
curl -s -X POST http://localhost:${VAULT_PORT}/v1/auth/token/lookup-self \
  -H "X-Vault-Token: ${ROOT_TOKEN}" | jq .

# Clean up
kill $PF_PID 2>/dev/null || true
wait $PF_PID 2>/dev/null || true

echo ""
echo "✓ Vault inicializado com sucesso!"
echo ""
echo "Próximos passos:"
echo "1. Configure as policies (ver VAULT-SETUP.md)"
echo "2. Configure Kubernetes auth method"
echo "3. Crie os roles para cada API"
echo "4. Adicione os secrets"
