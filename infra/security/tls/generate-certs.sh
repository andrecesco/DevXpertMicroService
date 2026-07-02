#!/bin/bash

# Script para gerar certificados self-signed para TLS/mTLS
# Usage: ./generate-certs.sh

set -e

NAMESPACE="eduonline"
DOMAIN="eduonline.local"
CERT_DIR="./certs"
DAYS=365

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Gerando Certificados Self-Signed${NC}"
echo ""

# Criar diretório
mkdir -p ${CERT_DIR}

# Função para gerar certificado
generate_cert() {
	local SERVICE=$1
	local CN="${SERVICE}.${NAMESPACE}.svc.cluster.local"

	echo -e "${YELLOW}Gerando certificado para ${SERVICE}...${NC}"

	# Generate CA if not exists
	if [ ! -f ${CERT_DIR}/ca-key.pem ]; then
		echo "  Gerando CA..."
		openssl genrsa -out ${CERT_DIR}/ca-key.pem 2048 2>/dev/null
		openssl req -new -x509 -days ${DAYS} -key ${CERT_DIR}/ca-key.pem \
			-out ${CERT_DIR}/ca.pem \
			-subj "/C=BR/ST=SP/L=SP/O=EduOnline/CN=EduOnline-CA" 2>/dev/null
	fi

	# Generate private key
	openssl genrsa -out ${CERT_DIR}/${SERVICE}-key.pem 2048 2>/dev/null

	# Generate CSR
	openssl req -new -key ${CERT_DIR}/${SERVICE}-key.pem \
		-out ${CERT_DIR}/${SERVICE}.csr \
		-subj "/C=BR/ST=SP/L=SP/O=EduOnline/CN=${CN}" 2>/dev/null

	# Generate certificate
	openssl x509 -req -in ${CERT_DIR}/${SERVICE}.csr \
		-CA ${CERT_DIR}/ca.pem \
		-CAkey ${CERT_DIR}/ca-key.pem \
		-CAcreateserial -out ${CERT_DIR}/${SERVICE}.pem \
		-days ${DAYS} \
		-extfile <(printf "subjectAltName=DNS:${CN},DNS:${SERVICE},DNS:localhost") 2>/dev/null

	# Create PKCS12 for Java/.NET
	openssl pkcs12 -export -in ${CERT_DIR}/${SERVICE}.pem \
		-inkey ${CERT_DIR}/${SERVICE}-key.pem \
		-out ${CERT_DIR}/${SERVICE}.p12 \
		-name ${SERVICE} \
		-passout pass:changeit 2>/dev/null

	echo -e "${GREEN}  ✓ Certificado gerado${NC}"
}

# Gerar certificados para cada serviço
echo -e "${YELLOW}Gerando certificados para cada serviço:${NC}"
echo ""

generate_cert "auth-api"
generate_cert "alunos-api"
generate_cert "conteudos-api"
generate_cert "pagamentos-api"
generate_cert "bff-api"
generate_cert "ingress"
generate_cert "elasticsearch"
generate_cert "prometheus"

echo ""
echo -e "${GREEN}✓ Certificados gerados${NC}"
echo ""
echo -e "${YELLOW}Próximos passos:${NC}"
echo ""
echo "1. Criar Kubernetes Secrets com os certificados:"
echo ""
echo "   # Para cada API"
echo "   kubectl create secret tls auth-api-tls -n ${NAMESPACE} \\"
echo "     --cert=${CERT_DIR}/auth-api.pem \\"
echo "     --key=${CERT_DIR}/auth-api-key.pem"
echo ""
echo "2. Atualizar Ingress para usar TLS"
echo "3. Atualizar APIs para usar mTLS"
echo ""
echo -e "${YELLOW}Arquivos gerados em:${NC}"
ls -la ${CERT_DIR}/
echo ""
