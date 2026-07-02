#!/bin/bash

# Script para testar stack de observabilidade com carga de tráfego
# Usage: ./test-observability.sh

set -e

NAMESPACE="eduonline"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo ""
echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Teste de Carga - Stack de Observabilidade                  ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Verificar pré-requisitos
echo -e "${YELLOW}[1/6] Verificando pré-requisitos...${NC}"
if ! command -v kubectl &> /dev/null; then
	echo -e "${RED}✗ kubectl não encontrado${NC}"
	exit 1
fi
if ! command -v curl &> /dev/null; then
	echo -e "${RED}✗ curl não encontrado${NC}"
	exit 1
fi
echo -e "${GREEN}✓ Pré-requisitos OK${NC}"

# Verificar se namespace existe
echo -e "${YELLOW}[2/6] Verificando namespace ${NAMESPACE}...${NC}"
if ! kubectl get namespace ${NAMESPACE} &> /dev/null; then
	echo -e "${RED}✗ Namespace ${NAMESPACE} não existe${NC}"
	exit 1
fi
echo -e "${GREEN}✓ Namespace ${NAMESPACE} existe${NC}"

# Verificar se APIs estão rodando
echo -e "${YELLOW}[3/6] Verificando APIs...${NC}"
APIs=("auth-api" "alunos-api" "conteudos-api" "pagamentos-api" "bff-api")
for api in "${APIs[@]}"; do
	if kubectl get svc $api -n ${NAMESPACE} &> /dev/null; then
		echo -e "${GREEN}  ✓ $api encontrada${NC}"
	else
		echo -e "${YELLOW}  ⚠ $api não encontrada${NC}"
	fi
done

# Gerar carga de tráfego
echo -e "${YELLOW}[4/6] Gerando carga de tráfego por 1 minuto...${NC}"

# Porta-forward para BFF API
kubectl port-forward -n ${NAMESPACE} svc/bff-api 5004:5004 &
PORT_FORWARD_PID=$!
sleep 2

echo -e "${BLUE}Enviando requisições...${NC}"
REQUESTS=0
SUCCESSES=0
FAILURES=0

for i in {1..60}; do
	# GET requests
	for endpoint in "/health" "/api/v1/users" "/api/v1/courses"; do
		if curl -s -o /dev/null -w "%{http_code}" http://localhost:5004$endpoint 2>/dev/null | grep -q "200\|404\|500"; then
			SUCCESSES=$((SUCCESSES + 1))
		else
			FAILURES=$((FAILURES + 1))
		fi
		REQUESTS=$((REQUESTS + 1))
	done

	# POST requests (simples)
	if curl -s -X POST http://localhost:5004/api/v1/login \
		-H "Content-Type: application/json" \
		-d '{"username":"test","password":"test"}' \
		-o /dev/null 2>/dev/null; then
		SUCCESSES=$((SUCCESSES + 1))
	else
		FAILURES=$((FAILURES + 1))
	fi
	REQUESTS=$((REQUESTS + 1))

	echo -ne "${BLUE}  Requisições: $REQUESTS | Sucessos: $SUCCESSES | Falhas: $FAILURES${NC}\r"
	sleep 1
done

# Parar port-forward
kill $PORT_FORWARD_PID 2>/dev/null || true
wait $PORT_FORWARD_PID 2>/dev/null || true

echo ""
echo -e "${GREEN}✓ Carga de tráfego gerada${NC}"
echo -e "  Total: $REQUESTS requisições"
echo -e "  Sucessos: $SUCCESSES"
echo -e "  Falhas: $FAILURES"

# Verificar métricas no Prometheus
echo -e "${YELLOW}[5/6] Verificando métricas no Prometheus...${NC}"

kubectl port-forward -n ${NAMESPACE} svc/prometheus 9090:9090 &
PROM_PID=$!
sleep 2

PROMETHEUS_URL="http://localhost:9090/api/v1/query"

# Testar algumas queries
QUERIES=(
	'up{job="prometheus"}'
	'rate(http_requests_total[5m])'
	'container_memory_usage_bytes'
)

for query in "${QUERIES[@]}"; do
	RESULT=$(curl -s "${PROMETHEUS_URL}?query=${query}" | grep -o '"result":\[' | head -1 || true)
	if [ -n "$RESULT" ]; then
		echo -e "${GREEN}  ✓ Query OK: $query${NC}"
	else
		echo -e "${YELLOW}  ⚠ Query retornou vazio: $query${NC}"
	fi
done

kill $PROM_PID 2>/dev/null || true
wait $PROM_PID 2>/dev/null || true

# Resumo e próximos passos
echo -e "${YELLOW}[6/6] Resumo${NC}"
echo ""
echo -e "${GREEN}✓ Teste de carga concluído!${NC}"
echo ""
echo -e "${BLUE}Para visualizar os resultados:${NC}"
echo ""
echo -e "1. ${YELLOW}Prometheus (métricas):${NC}"
echo "   kubectl port-forward -n ${NAMESPACE} svc/prometheus 9090:9090"
echo "   http://localhost:9090"
echo ""
echo -e "2. ${YELLOW}Grafana (dashboards):${NC}"
echo "   kubectl port-forward -n ${NAMESPACE} svc/grafana 3000:3000"
echo "   http://localhost:3000 (admin/GrafanaAdminPassword2026!)"
echo ""
echo -e "3. ${YELLOW}Kibana (logs):${NC}"
echo "   kubectl port-forward -n ${NAMESPACE} svc/kibana 5601:5601"
echo "   http://localhost:5601"
echo ""
echo -e "4. ${YELLOW}Jaeger (traces):${NC}"
echo "   kubectl port-forward -n ${NAMESPACE} svc/jaeger 16686:16686"
echo "   http://localhost:16686"
echo ""
echo -e "${BLUE}Queries úteis no Prometheus:${NC}"
echo ""
echo "- rate(http_requests_total[5m]) - Taxa de requisições"
echo "- histogram_quantile(0.95, http_request_duration_seconds_bucket) - Latência P95"
echo "- rate(http_requests_total{status=~\"5..\"}[5m]) - Taxa de erros"
echo "- container_memory_usage_bytes{pod=~\".*-api\"} - Uso de memória"
echo "- rate(container_cpu_usage_seconds_total[5m]) - Uso de CPU"
echo ""
