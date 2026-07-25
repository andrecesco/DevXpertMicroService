param(
	[string]$Namespace = 'eduonline',
	[int]$TimeoutSeconds = 600
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$deployments = @(
	'auth-api',
	'conteudos-api',
	'alunos-api',
	'pagamentos-api',
	'bff-api',
	'status-api'
)

$services = @(
	@{ Name = 'auth-api'; Port = 5000 },
	@{ Name = 'conteudos-api'; Port = 5001 },
	@{ Name = 'alunos-api'; Port = 5002 },
	@{ Name = 'pagamentos-api'; Port = 5003 },
	@{ Name = 'bff-api'; Port = 5004 },
	@{ Name = 'status-api'; Port = 5005 }
)

# ---------------------------------------------------------------------------
# 1. Verifica que todos os Deployments existem e possuem pods agendados.
#    Em ambiente Kind sem infraestrutura real (DB, EventStore), os pods podem
#    nao ficar Ready; o objetivo do smoke-test e validar os manifests.
# ---------------------------------------------------------------------------
Write-Host "Verificando que os Deployments existem no namespace '$Namespace'..." -ForegroundColor Cyan
foreach ($deployment in $deployments) {
	# Aguarda ate o Deployment ser observado pelo controller (max 60s)
	$deadline = (Get-Date).AddSeconds(60)
	$found = $false
	while ((Get-Date) -lt $deadline) {
		$result = kubectl get deployment/$deployment -n $Namespace --ignore-not-found 2>$null
		if (-not [string]::IsNullOrWhiteSpace($result)) { $found = $true; break }
		Start-Sleep -Seconds 5
	}
	if (-not $found) {
		throw "Deployment '$deployment' nao encontrado no namespace '$Namespace'."
	}
	Write-Host "Deployment '$deployment' existe." -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# 2. Aguarda pods de cada Deployment atingirem ao menos 'Pending' ou 'Running'
#    (agendados pelo scheduler, nao necessariamente Ready).
# ---------------------------------------------------------------------------
Write-Host "Aguardando pods serem agendados..." -ForegroundColor Cyan
foreach ($deployment in $deployments) {
	$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
	$scheduled = $false
	while ((Get-Date) -lt $deadline) {
		$podPhases = kubectl get pods -n $Namespace -l "app=$deployment" `
			-o jsonpath='{.items[*].status.phase}' 2>$null
		if ($podPhases -match 'Running|Pending') { $scheduled = $true; break }
		Start-Sleep -Seconds 10
	}
	if (-not $scheduled) {
		throw "Nenhum pod do Deployment '$deployment' foi agendado em ${TimeoutSeconds}s."
	}
	Write-Host "Deployment '$deployment' possui pods agendados (fase: $podPhases)." -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# 3. Verifica que os Services existem (endpoints podem estar vazios no Kind)
# ---------------------------------------------------------------------------
Write-Host "Verificando existencia dos Services..." -ForegroundColor Cyan
foreach ($service in $services) {
	$svcExists = kubectl get svc $service.Name -n $Namespace --ignore-not-found 2>$null
	if ([string]::IsNullOrWhiteSpace($svcExists)) {
		throw "Service '$($service.Name)' nao encontrado no namespace '$Namespace'."
	}
	Write-Host "Service '$($service.Name)' existe." -ForegroundColor Green
}

Write-Host "Smoke test Kubernetes concluido com sucesso." -ForegroundColor Green
