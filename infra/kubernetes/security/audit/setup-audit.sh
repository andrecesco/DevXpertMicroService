#!/bin/bash

# Script para configurar audit logging no Kubernetes
# Este script assume um cluster Kind/Minikube

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

CLUSTER_NAME="${1:-eduonline}"
AUDIT_LOG_PATH="/var/log/kubernetes/audit.log"
AUDIT_LOG_MAX_AGE=30
AUDIT_LOG_MAX_BACKUP=10

echo -e "${YELLOW}Configurando Audit Logging${NC}"
echo ""

# Para Kind
if command -v kind &> /dev/null; then
	echo -e "${YELLOW}Detectado Kind cluster${NC}"
	echo ""
	echo -e "${YELLOW}Instruções para Kind:${NC}"
	echo ""
	echo "1. Atualizar kind-config.yaml:"
	echo ""
	echo "apiVersion: kind.x-k8s.io/v1alpha4"
	echo "kind: Cluster"
	echo "metadata:"
	echo "  name: ${CLUSTER_NAME}"
	echo "nodes:"
	echo "- role: control-plane"
	echo "  extraMounts:"
	echo "  - hostPath: $(pwd)/infra/security/audit/audit-policy.yaml"
	echo "    containerPath: /etc/kubernetes/audit-policy.yaml"
	echo "  - hostPath: /tmp/audit-logs"
	echo "    containerPath: /var/log/kubernetes"
	echo "  kubeadmConfigPatches:"
	echo "  - |"
	echo "    kind: ClusterConfiguration"
	echo "    apiServer:"
	echo "      extraArgs:"
	echo "        audit-policy-file: /etc/kubernetes/audit-policy.yaml"
	echo "        audit-log-path: /var/log/kubernetes/audit.log"
	echo "        audit-log-maxage: ${AUDIT_LOG_MAX_AGE}"
	echo "        audit-log-maxbackup: ${AUDIT_LOG_MAX_BACKUP}"
	echo ""

	echo "2. Criar cluster:"
	echo "   kind create cluster -f kind-config.yaml"
	echo ""
	echo "3. Verificar logs:"
	echo "   kubectl exec -n kube-system <control-plane-pod> -- tail -f /var/log/kubernetes/audit.log"
fi

# Para Minikube
if command -v minikube &> /dev/null; then
	echo -e "${YELLOW}Detectado Minikube${NC}"
	echo ""
	echo -e "${YELLOW}Instruções para Minikube:${NC}"
	echo ""
	echo "1. Criar pasta para logs:"
	echo "   minikube ssh \"mkdir -p /var/log/kubernetes\""
	echo ""
	echo "2. Copiar audit policy:"
	echo "   minikube cp infra/security/audit/audit-policy.yaml /etc/kubernetes/audit-policy.yaml"
	echo ""
	echo "3. Editar API server config:"
	echo "   minikube ssh \"sudo vi /etc/kubernetes/manifests/kube-apiserver.yaml\""
	echo ""
	echo "4. Adicionar argumentos:"
	echo "   - --audit-policy-file=/etc/kubernetes/audit-policy.yaml"
	echo "   - --audit-log-path=/var/log/kubernetes/audit.log"
	echo "   - --audit-log-maxage=${AUDIT_LOG_MAX_AGE}"
	echo "   - --audit-log-maxbackup=${AUDIT_LOG_MAX_BACKUP}"
	echo ""
	echo "5. Verificar logs:"
	echo "   minikube ssh \"tail -f /var/log/kubernetes/audit.log\""
fi

echo ""
echo -e "${YELLOW}Para verificar audit logs:${NC}"
echo ""
echo "# Buscar ações de admin"
echo "kubectl logs -n kube-system -l component=kube-apiserver | grep audit"
echo ""
echo "# Buscar acessos a secrets"
echo "grep 'secrets' /var/log/kubernetes/audit.log | jq '.'"
echo ""
echo "# Buscar mudanças em RBAC"
echo "grep 'rolebindings' /var/log/kubernetes/audit.log | jq '.'"
echo ""
echo "# Buscar tentativas de autenticação falhadas"
echo "grep 'authentication failure' /var/log/kubernetes/audit.log | jq '.'"
echo ""
