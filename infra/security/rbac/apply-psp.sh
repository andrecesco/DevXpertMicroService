#!/bin/bash

# Script para aplicar Pod Security Policies
# Usage: ./apply-psp.sh

set -e

NAMESPACE="eduonline"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo ""
echo -e "${YELLOW}Aplicando Pod Security Policies...${NC}"
echo ""

# Verificar namespace
if ! kubectl get namespace ${NAMESPACE} &> /dev/null; then
	echo -e "${RED}✗ Namespace ${NAMESPACE} não existe${NC}"
	exit 1
fi

echo -e "${GREEN}✓ Namespace ${NAMESPACE} existe${NC}"
echo ""

# Apply PSPs
echo -e "${YELLOW}Aplicando Pod Security Policies...${NC}"
kubectl apply -f infra/security/rbac/pod-security-policies.yaml
echo -e "${GREEN}✓ PSPs aplicadas${NC}"
echo ""

# Verificar PSPs
echo -e "${YELLOW}Verificando PSPs instaladas...${NC}"
if kubectl get psp 2>/dev/null; then
	kubectl get psp
	echo ""
	echo -e "${GREEN}✓ PSPs criadas com sucesso${NC}"
else
	echo -e "${YELLOW}⚠ PSPs podem não estar disponíveis em K8s >= 1.25${NC}"
	echo "  Use Pod Security Standards (PSS) em versões mais recentes"
fi

echo ""
echo -e "${YELLOW}Informações sobre PSPs:${NC}"
echo ""
echo "Restricted PSP:"
kubectl describe psp restricted 2>/dev/null || echo "  (não disponível)"
echo ""

echo "Baseline PSP:"
kubectl describe psp baseline 2>/dev/null || echo "  (não disponível)"
echo ""

echo -e "${YELLOW}Para aplicar PSPs em namespaces:${NC}"
echo ""
echo "# Adicionar labels ao namespace"
echo "kubectl label namespace ${NAMESPACE} pod-security.kubernetes.io/enforce=restricted"
echo ""
echo "# Verificar qual Pod Security Standard está em uso"
echo "kubectl get namespace ${NAMESPACE} --show-labels"
echo ""
