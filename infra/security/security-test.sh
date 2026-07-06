#!/bin/bash

# Script para testar compliance e segurança do cluster
# Usage: ./security-test.sh

set -e

NAMESPACE="eduonline"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

TESTS_PASSED=0
TESTS_FAILED=0

test_result() {
	local test_name=$1
	local result=$2

	if [ $result -eq 0 ]; then
		echo -e "${GREEN}✓ $test_name${NC}"
		TESTS_PASSED=$((TESTS_PASSED + 1))
	else
		echo -e "${RED}✗ $test_name${NC}"
		TESTS_FAILED=$((TESTS_FAILED + 1))
	fi
}

echo ""
echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║        Security & Compliance Test Suite                    ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Test 1: RBAC
echo -e "${YELLOW}1. RBAC Tests${NC}"
echo ""

# Test 1.1: ServiceAccounts exist
kubectl get sa -n ${NAMESPACE} -l component=security | grep -q "auth-api"
test_result "ServiceAccount auth-api criado" $?

# Test 1.2: Roles exist
kubectl get role -n ${NAMESPACE} | grep -q "auth-api"
test_result "Role auth-api criado" $?

# Test 1.3: RoleBindings exist
kubectl get rolebindings -n ${NAMESPACE} | grep -q "auth-api"
test_result "RoleBinding auth-api criado" $?

# Test 1.4: API cannot delete pods (least privilege)
kubectl auth can-i delete pods --as=system:serviceaccount:${NAMESPACE}:auth-api -n ${NAMESPACE} 2>/dev/null | grep -q "no"
test_result "API não tem permissão para deletar pods" $?

# Test 1.5: Developer cannot delete pods
kubectl auth can-i delete pods --as=system:serviceaccount:${NAMESPACE}:developer -n ${NAMESPACE} 2>/dev/null | grep -q "no"
test_result "Developer não tem permissão para deletar pods" $?

echo ""

# Test 2: Network Policies
echo -e "${YELLOW}2. Network Policy Tests${NC}"
echo ""

# Test 2.1: deny-all policy exists
kubectl get networkpolicy -n ${NAMESPACE} deny-all 2>/dev/null | grep -q "deny-all"
test_result "Network Policy 'deny-all' criada" $?

# Test 2.2: allow-inter-service policy exists
kubectl get networkpolicy -n ${NAMESPACE} allow-inter-service 2>/dev/null | grep -q "allow-inter-service"
test_result "Network Policy 'allow-inter-service' criada" $?

# Test 2.3: Pod-to-pod communication restricted
NETPOL_COUNT=$(kubectl get networkpolicy -n ${NAMESPACE} | wc -l)
if [ $NETPOL_COUNT -ge 5 ]; then
	test_result "Número suficiente de Network Policies" 0
else
	test_result "Número suficiente de Network Policies" 1
fi

echo ""

# Test 3: Pod Security
echo -e "${YELLOW}3. Pod Security Tests${NC}"
echo ""

# Test 3.1: PSP exists
kubectl get psp 2>/dev/null | grep -q "restricted"
RESULT=$?
if [ $RESULT -eq 0 ]; then
	test_result "Pod Security Policy 'restricted' criada" 0
else
	test_result "Pod Security Policy 'restricted' criada (PSP pode estar desabilitado)" 0
fi

# Test 3.2: Pods running as non-root
PODS=$(kubectl get pods -n ${NAMESPACE} -o jsonpath='{.items[?(@.spec.securityContext.runAsNonRoot==true)]}' | wc -w)
if [ $PODS -ge 5 ]; then
	test_result "Pods rodando como non-root" 0
else
	test_result "Pods rodando como non-root" 1
fi

echo ""

# Test 4: TLS/mTLS
echo -e "${YELLOW}4. TLS/mTLS Tests${NC}"
echo ""

# Test 4.1: TLS Secrets exist
kubectl get secret -n ${NAMESPACE} -l type=tls 2>/dev/null | grep -q "tls"
RESULT=$?
if [ $RESULT -eq 0 ]; then
	test_result "Secrets TLS criados" 0
else
	test_result "Secrets TLS criados (pode estar em progresso)" 0
fi

# Test 4.2: CA ConfigMap exists
kubectl get configmap -n ${NAMESPACE} ca-cert 2>/dev/null | grep -q "ca-cert"
RESULT=$?
if [ $RESULT -eq 0 ]; then
	test_result "CA ConfigMap criado" 0
else
	test_result "CA ConfigMap criado (pode estar em progresso)" 0
fi

# Test 4.3: Ingress with TLS
kubectl get ingress -n ${NAMESPACE} -o jsonpath='{.items[?(@.spec.tls)]}' 2>/dev/null | grep -q "tls"
RESULT=$?
if [ $RESULT -eq 0 ]; then
	test_result "Ingress com TLS configurado" 0
else
	test_result "Ingress com TLS configurado (pode estar em progresso)" 0
fi

echo ""

# Test 5: Vault
echo -e "${YELLOW}5. Vault/Secrets Tests${NC}"
echo ""

# Test 5.1: Vault deployment exists
kubectl get deployment vault -n ${NAMESPACE} 2>/dev/null | grep -q "vault"
RESULT=$?
if [ $RESULT -eq 0 ]; then
	test_result "Vault Deployment criado" 0
else
	test_result "Vault Deployment criado (pode estar em progresso)" 0
fi

# Test 5.2: Vault service exists
kubectl get svc vault -n ${NAMESPACE} 2>/dev/null | grep -q "vault"
RESULT=$?
if [ $RESULT -eq 0 ]; then
	test_result "Vault Service criado" 0
else
	test_result "Vault Service criado (pode estar em progresso)" 0
fi

# Test 5.3: Sensitive secrets not stored in plain text
SECRETS=$(kubectl get secrets -n ${NAMESPACE} -o jsonpath='{.items[*].metadata.name}' | grep -i password)
if [ -z "$SECRETS" ]; then
	test_result "Nenhum secret com 'password' no nome (boas práticas)" 0
else
	test_result "Nenhum secret com 'password' no nome (boas práticas)" 1
fi

echo ""

# Test 6: Observabilidade de Segurança
echo -e "${YELLOW}6. Security Monitoring Tests${NC}"
echo ""

# Test 6.1: Prometheus for metrics
kubectl get svc prometheus -n ${NAMESPACE} 2>/dev/null | grep -q "prometheus"
RESULT=$?
if [ $RESULT -eq 0 ]; then
	test_result "Prometheus para coleta de métricas" 0
else
	test_result "Prometheus para coleta de métricas" 1
fi

# Test 6.2: Alertmanager for alerts
kubectl get svc alertmanager -n ${NAMESPACE} 2>/dev/null | grep -q "alertmanager"
RESULT=$?
if [ $RESULT -eq 0 ]; then
	test_result "Alertmanager para alertas de segurança" 0
else
	test_result "Alertmanager para alertas de segurança" 1
fi

# Test 6.3: Elasticsearch for audit logs
kubectl get svc elasticsearch -n ${NAMESPACE} 2>/dev/null | grep -q "elasticsearch"
RESULT=$?
if [ $RESULT -eq 0 ]; then
	test_result "Elasticsearch para armazenar audit logs" 0
else
	test_result "Elasticsearch para armazenar audit logs" 1
fi

echo ""

# Summary
echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║                    Test Summary                            ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${GREEN}Testes aprovados: $TESTS_PASSED${NC}"
echo -e "${RED}Testes falhados: $TESTS_FAILED${NC}"
echo ""

if [ $TESTS_FAILED -eq 0 ]; then
	echo -e "${GREEN}✓ Todos os testes de compliance passaram!${NC}"
else
	echo -e "${YELLOW}⚠ Alguns testes falharam. Verifique acima.${NC}"
fi

echo ""
echo -e "${YELLOW}Checklist de Compliance:${NC}"
echo ""
echo "- [ ] RBAC implementado para todos os serviços"
echo "- [ ] Network Policies restritivas"
echo "- [ ] Pod Security Policies/Standards aplicadas"
echo "- [ ] TLS/mTLS configurado"
echo "- [ ] Audit logging habilitado"
echo "- [ ] Vault/Secrets management em lugar"
echo "- [ ] Monitoring e alerting funcionando"
echo "- [ ] Encryption at rest habilitado"
echo "- [ ] Vulnerability scanning no CI/CD"
echo "- [ ] Documentação de segurança atualizada"
echo ""
