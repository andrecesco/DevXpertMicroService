#!/usr/bin/env pwsh

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$kustomizationPath = Join-Path $repoRoot "infra/kubernetes/kustomization.yaml"

if (-not (Test-Path $kustomizationPath)) {
	throw "Arquivo não encontrado: $kustomizationPath"
}

$content = Get-Content $kustomizationPath -Raw -Encoding UTF8

$requiredResources = @(
	"observability/otel-collector/configmap.yaml",
	"observability/otel-collector/deployment.yaml",
	"observability/otel-collector/service.yaml",
	"observability/jaeger/deployment.yaml",
	"observability/alertmanager/deployment.yaml",
	"observability/elasticsearch/deployment.yaml",
	"observability/fluentd/daemonset.yaml"
)

foreach ($resource in $requiredResources) {
	if ($content -notmatch [regex]::Escape($resource)) {
		throw "Recurso obrigatório de observabilidade ausente no kustomization: $resource"
	}

	$resourcePath = Join-Path (Join-Path $repoRoot "infra/kubernetes") $resource
	if (-not (Test-Path $resourcePath)) {
		throw "Arquivo de recurso obrigatório não encontrado: $resourcePath"
	}
}

Write-Host "Validação de manifestos de observabilidade concluída com sucesso."
