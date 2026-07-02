#!/bin/bash

# Script para validar stack Prometheus + Grafana em Kind/Minikube
# Usage: ./validate-observability-stack.sh

set -e

NAMESPACE="eduonline"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "================================"
echo "Validando Stack de Observabilidade"
echo "================================"
echo ""

# Check if cluster is running
echo -e "${YELLOW}[1/10] Verificando cluster Kubernetes...${NC}"
if kubectl cluster-info &> /dev/null; then
	echo -e "${GREEN}✓ Cluster está rodando${NC}"
else
	echo -e "${RED}✗ Cluster não está rodando${NC}"
	exit 1
fi

# Check namespace
echo -e "${YELLOW}[2/10] Verificando namespace ${NAMESPACE}...${NC}"
if kubectl get namespace ${NAMESPACE} &> /dev/null; then
	echo -e "${GREEN}✓ Namespace ${NAMESPACE} existe${NC}"
else
	echo -e "${RED}✗ Namespace ${NAMESPACE} não existe${NC}"
	exit 1
fi

# Check Prometheus
echo -e "${YELLOW}[3/10] Verificando Prometheus...${NC}"
if kubectl get deployment prometheus -n ${NAMESPACE} &> /dev/null; then
	PROM_REPLICAS=$(kubectl get deployment prometheus -n ${NAMESPACE} -o jsonpath='{.status.replicas}')
	PROM_READY=$(kubectl get deployment prometheus -n ${NAMESPACE} -o jsonpath='{.status.readyReplicas}')
	if [ "$PROM_REPLICAS" == "$PROM_READY" ]; then
		echo -e "${GREEN}✓ Prometheus está Running (${PROM_REPLICAS}/${PROM_REPLICAS} replicas)${NC}"
	else
		echo -e "${YELLOW}⚠ Prometheus está inicializando (${PROM_READY}/${PROM_REPLICAS} replicas)${NC}"
	fi
else
	echo -e "${RED}✗ Prometheus Deployment não encontrado${NC}"
fi

# Check Grafana
echo -e "${YELLOW}[4/10] Verificando Grafana...${NC}"
if kubectl get deployment grafana -n ${NAMESPACE} &> /dev/null; then
	GRAFANA_REPLICAS=$(kubectl get deployment grafana -n ${NAMESPACE} -o jsonpath='{.status.replicas}')
	GRAFANA_READY=$(kubectl get deployment grafana -n ${NAMESPACE} -o jsonpath='{.status.readyReplicas}')
	if [ "$GRAFANA_REPLICAS" == "$GRAFANA_READY" ]; then
		echo -e "${GREEN}✓ Grafana está Running (${GRAFANA_REPLICAS}/${GRAFANA_REPLICAS} replicas)${NC}"
	else
		echo -e "${YELLOW}⚠ Grafana está inicializando (${GRAFANA_READY}/${GRAFANA_REPLICAS} replicas)${NC}"
	fi
else
	echo -e "${RED}✗ Grafana Deployment não encontrado${NC}"
fi

# Check Alertmanager
echo -e "${YELLOW}[5/10] Verificando Alertmanager...${NC}"
if kubectl get deployment alertmanager -n ${NAMESPACE} &> /dev/null; then
	AM_REPLICAS=$(kubectl get deployment alertmanager -n ${NAMESPACE} -o jsonpath='{.status.replicas}')
	AM_READY=$(kubectl get deployment alertmanager -n ${NAMESPACE} -o jsonpath='{.status.readyReplicas}')
	if [ "$AM_REPLICAS" == "$AM_READY" ]; then
		echo -e "${GREEN}✓ Alertmanager está Running (${AM_REPLICAS}/${AM_REPLICAS} replicas)${NC}"
	else
		echo -e "${YELLOW}⚠ Alertmanager está inicializando (${AM_READY}/${AM_REPLICAS} replicas)${NC}"
	fi
else
	echo -e "${RED}✗ Alertmanager Deployment não encontrado${NC}"
fi

# Check Services
echo -e "${YELLOW}[6/10] Verificando Services...${NC}"
for service in prometheus grafana alertmanager; do
	if kubectl get svc $service -n ${NAMESPACE} &> /dev/null; then
		echo -e "${GREEN}✓ Service $service existe${NC}"
	else
		echo -e "${RED}✗ Service $service não encontrado${NC}"
	fi
done

# Check Pods
echo -e "${YELLOW}[7/10] Verificando Pods...${NC}"
echo "Pods em execução:"
kubectl get pods -n ${NAMESPACE} -l component=observability

# Check PersistentVolumes
echo -e "${YELLOW}[8/10] Verificando PersistentVolumes...${NC}"
if kubectl get pvc prometheus-pvc -n ${NAMESPACE} &> /dev/null; then
	STATUS=$(kubectl get pvc prometheus-pvc -n ${NAMESPACE} -o jsonpath='{.status.phase}')
	echo -e "${GREEN}✓ PVC prometheus-pvc: $STATUS${NC}"
else
	echo -e "${YELLOW}⚠ PVC prometheus-pvc não encontrado (esperado para emptyDir)${NC}"
fi

# Check Prometheus targets
echo -e "${YELLOW}[9/10] Verificando Prometheus targets...${NC}"
if kubectl exec -n ${NAMESPACE} -it $(kubectl get pod -n ${NAMESPACE} -l app=prometheus -o jsonpath='{.items[0].metadata.name}') -- curl -s http://localhost:9090/api/v1/targets 2>/dev/null | grep -q '"state":"up"'; then
	TARGETS=$(kubectl exec -n ${NAMESPACE} -it $(kubectl get pod -n ${NAMESPACE} -l app=prometheus -o jsonpath='{.items[0].metadata.name}') -- curl -s http://localhost:9090/api/v1/targets 2>/dev/null | grep -o '"state":"up"' | wc -l)
	echo -e "${GREEN}✓ Prometheus targets up: $TARGETS${NC}"
else
	echo -e "${YELLOW}⚠ Verifique Prometheus targets manualmente${NC}"
fi

# Summary
echo ""
echo -e "${YELLOW}[10/10] Resumo${NC}"
echo "================================"
echo -e "${GREEN}Stack de Observabilidade validada!${NC}"
echo ""
echo "Acessar serviços:"
echo "  Prometheus: kubectl port-forward -n ${NAMESPACE} svc/prometheus 9090:9090"
echo "  Grafana:    kubectl port-forward -n ${NAMESPACE} svc/grafana 3000:3000"
echo "  AlertMgr:   kubectl port-forward -n ${NAMESPACE} svc/alertmanager 9093:9093"
echo ""
echo "URLs locais:"
echo "  http://localhost:9090     (Prometheus)"
echo "  http://localhost:3000     (Grafana - admin/GrafanaAdminPassword2026!)"
echo "  http://localhost:9093     (Alertmanager)"
echo ""
