#!/usr/bin/env pwsh

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-Contains {
	param(
		[string]$Content,
		[string]$Needle,
		[string]$ErrorMessage
	)

	if ($Content -notmatch [regex]::Escape($Needle)) {
		throw $ErrorMessage
	}
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$kubernetesRoot = Join-Path $repoRoot "infra/kubernetes"
$kustomizationPath = Join-Path $kubernetesRoot "kustomization.yaml"

if (-not (Test-Path $kustomizationPath)) {
	throw "Arquivo não encontrado: $kustomizationPath"
}

$kustomizationContent = Get-Content $kustomizationPath -Raw -Encoding UTF8

$requiredResources = @(
	"observability/otel-collector/configmap.yaml",
	"observability/otel-collector/deployment.yaml",
	"observability/otel-collector/service.yaml",
	"observability/jaeger/deployment.yaml",
	"observability/jaeger/service.yaml",
	"observability/alertmanager/configmap.yaml",
	"observability/alertmanager/deployment.yaml",
	"observability/alertmanager/service.yaml",
	"observability/elasticsearch/deployment.yaml",
	"observability/fluentd/daemonset.yaml"
)

foreach ($resource in $requiredResources) {
	Assert-Contains -Content $kustomizationContent -Needle $resource -ErrorMessage "Recurso obrigatório de observabilidade ausente no kustomization: $resource"

	$resourcePath = Join-Path $kubernetesRoot $resource
	if (-not (Test-Path $resourcePath)) {
		throw "Arquivo de recurso obrigatório não encontrado: $resourcePath"
	}
}

$otelConfigPath = Join-Path $kubernetesRoot "observability/otel-collector/configmap.yaml"
$otelConfigContent = Get-Content $otelConfigPath -Raw -Encoding UTF8
Assert-Contains -Content $otelConfigContent -Needle "pipelines:" -ErrorMessage "OTEL Collector sem seção de pipelines em $otelConfigPath"
Assert-Contains -Content $otelConfigContent -Needle "traces:" -ErrorMessage "OTEL Collector sem pipeline de traces em $otelConfigPath"
Assert-Contains -Content $otelConfigContent -Needle "receivers: [otlp]" -ErrorMessage "OTEL Collector sem receiver OTLP no pipeline de traces em $otelConfigPath"
Assert-Contains -Content $otelConfigContent -Needle "exporters: [otlp/jaeger, debug]" -ErrorMessage "OTEL Collector sem exportação de traces para Jaeger em $otelConfigPath"

$jaegerDeploymentPath = Join-Path $kubernetesRoot "observability/jaeger/deployment.yaml"
$jaegerDeploymentContent = Get-Content $jaegerDeploymentPath -Raw -Encoding UTF8
Assert-Contains -Content $jaegerDeploymentContent -Needle "name: COLLECTOR_OTLP_ENABLED" -ErrorMessage "Jaeger sem configuração COLLECTOR_OTLP_ENABLED em $jaegerDeploymentPath"
Assert-Contains -Content $jaegerDeploymentContent -Needle 'value: "true"' -ErrorMessage "Jaeger com OTLP desabilitado em $jaegerDeploymentPath"

$alertmanagerConfigPath = Join-Path $kubernetesRoot "observability/alertmanager/configmap.yaml"
$alertmanagerConfigContent = Get-Content $alertmanagerConfigPath -Raw -Encoding UTF8
Assert-Contains -Content $alertmanagerConfigContent -Needle "route:" -ErrorMessage "Alertmanager sem rota principal em $alertmanagerConfigPath"
Assert-Contains -Content $alertmanagerConfigContent -Needle "- name: 'critical'" -ErrorMessage "Alertmanager sem receiver 'critical' em $alertmanagerConfigPath"
Assert-Contains -Content $alertmanagerConfigContent -Needle "- name: 'warning'" -ErrorMessage "Alertmanager sem receiver 'warning' em $alertmanagerConfigPath"
Assert-Contains -Content $alertmanagerConfigContent -Needle "- name: 'info'" -ErrorMessage "Alertmanager sem receiver 'info' em $alertmanagerConfigPath"

Write-Host "Validação de observabilidade concluída com sucesso (recursos, traces e alertas)."
