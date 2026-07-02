#!/bin/bash

# Script para aplicar RBAC roles e bindings
# Usage: ./apply-rbac.sh

set -e

NAMESPACE="eduonline"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo ""
echo -e "${YELLOW}Aplicando RBAC roles e bindings...${NC}"
echo ""

# Verificar namespace
if ! kubectl get namespace ${NAMESPACE} &> /dev/null; then
	echo -e "${RED}✗ Namespace ${NAMESPACE} não existe${NC}"
	exit 1
fi

echo -e "${GREEN}✓ Namespace ${NAMESPACE} existe${NC}"
echo ""

# Apply API-specific RBAC
echo -e "${YELLOW}Aplicando RBAC para APIs...${NC}"
for api in auth-api alunos-api conteudos-api pagamentos-api bff-api; do
	echo "  Aplicando $api..."
	kubectl apply -f infra/security/rbac/${api}-rbac.yaml -n ${NAMESPACE}
done

echo -e "${GREEN}✓ RBAC para APIs aplicado${NC}"
echo ""

# Apply Admin RBAC
echo -e "${YELLOW}Aplicando RBAC para Admin...${NC}"
kubectl apply -f infra/security/rbac/admin-rbac.yaml -n ${NAMESPACE}
echo -e "${GREEN}✓ Admin RBAC aplicado${NC}"
echo ""

# Apply Developer RBAC
echo -e "${YELLOW}Aplicando RBAC para Developer...${NC}"
kubectl apply -f infra/security/rbac/developer-rbac.yaml -n ${NAMESPACE}
echo -e "${GREEN}✓ Developer RBAC aplicado${NC}"
echo ""

# Apply Pod Security Policies
echo -e "${YELLOW}Aplicando Pod Security Policies...${NC}"
kubectl apply -f infra/security/rbac/pod-security-policies.yaml -n ${NAMESPACE}
echo -e "${GREEN}✓ Pod Security Policies aplicadas${NC}"
echo ""

# Verify ServiceAccounts
echo -e "${YELLOW}Verificando ServiceAccounts...${NC}"
kubectl get serviceaccounts -n ${NAMESPACE} | grep -E "(auth-api|alunos-api|conteudos-api|pagamentos-api|bff-api|admin|developer)"
echo ""

# Verify Roles
echo -e "${YELLOW}Verificando Roles...${NC}"
kubectl get roles -n ${NAMESPACE}
echo ""

# Verify RoleBindings
echo -e "${YELLOW}Verificando RoleBindings...${NC}"
kubectl get rolebindings -n ${NAMESPACE}
echo ""

# Verify PSPs
echo -e "${YELLOW}Verificando Pod Security Policies...${NC}"
kubectl get psp -n ${NAMESPACE} 2>/dev/null || echo "  (PSP pode não ser disponível em K8s >= 1.25)"
echo ""

echo -e "${GREEN}✓ RBAC roles e bindings aplicados com sucesso!${NC}"
echo ""
echo -e "${YELLOW}Para testar RBAC:${NC}"
echo ""
echo "# Verificar permissões de um usuário/SA"
echo "kubectl auth can-i list pods --as=system:serviceaccount:${NAMESPACE}:auth-api -n ${NAMESPACE}"
echo "kubectl auth can-i delete pods --as=system:serviceaccount:${NAMESPACE}:auth-api -n ${NAMESPACE}"
echo ""
echo "# Verificar permissões de developer"
echo "kubectl auth can-i get pods --as=system:serviceaccount:${NAMESPACE}:developer -n ${NAMESPACE}"
echo "kubectl auth can-i delete pods --as=system:serviceaccount:${NAMESPACE}:developer -n ${NAMESPACE}"
echo ""
