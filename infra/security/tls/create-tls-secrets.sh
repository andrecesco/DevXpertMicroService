#!/bin/bash

# Script para criar Kubernetes Secrets com certificados TLS
# Usage: ./create-tls-secrets.sh

set -e

NAMESPACE="eduonline"
CERT_DIR="./certs"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Criando Kubernetes TLS Secrets${NC}"
echo ""

# Verificar se certificados existem
if [ ! -d ${CERT_DIR} ] || [ -z "$(ls -A ${CERT_DIR})" ]; then
	echo -e "${RED}✗ Certificados não encontrados em ${CERT_DIR}${NC}"
	echo "Execute ./generate-certs.sh primeiro"
	exit 1
fi

echo -e "${GREEN}✓ Certificados encontrados${NC}"
echo ""

# Criar secrets para cada serviço
create_tls_secret() {
	local SERVICE=$1
	local SECRET_NAME="${SERVICE}-tls"

	echo -e "${YELLOW}Criando secret para ${SERVICE}...${NC}"

	if [ ! -f ${CERT_DIR}/${SERVICE}.pem ] || [ ! -f ${CERT_DIR}/${SERVICE}-key.pem ]; then
		echo -e "${RED}  ✗ Certificados não encontrados para ${SERVICE}${NC}"
		return 1
	fi

	# Delete if exists
	kubectl delete secret ${SECRET_NAME} -n ${NAMESPACE} 2>/dev/null || true

	# Create secret
	kubectl create secret tls ${SECRET_NAME} \
		-n ${NAMESPACE} \
		--cert=${CERT_DIR}/${SERVICE}.pem \
		--key=${CERT_DIR}/${SERVICE}-key.pem

	echo -e "${GREEN}  ✓ Secret criado${NC}"
}

# Create CA ConfigMap
echo -e "${YELLOW}Criando CA ConfigMap...${NC}"
kubectl delete configmap ca-cert -n ${NAMESPACE} 2>/dev/null || true
kubectl create configmap ca-cert \
	-n ${NAMESPACE} \
	--from-file=${CERT_DIR}/ca.pem
echo -e "${GREEN}✓ CA ConfigMap criado${NC}"
echo ""

# Create secrets
echo -e "${YELLOW}Criando TLS Secrets...${NC}"
for service in auth-api alunos-api conteudos-api pagamentos-api bff-api elasticsearch prometheus ingress; do
	create_tls_secret $service || true
done

echo ""
echo -e "${GREEN}✓ TLS Secrets criados${NC}"
echo ""
echo -e "${YELLOW}Verificando secrets:${NC}"
kubectl get secrets -n ${NAMESPACE} -l type=tls
echo ""
