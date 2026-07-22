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

Write-Host "Aguardando disponibilidade dos deployments no namespace '$Namespace'..." -ForegroundColor Cyan
foreach ($deployment in $deployments) {
	kubectl rollout status deployment/$deployment -n $Namespace --timeout="${TimeoutSeconds}s"
}

Write-Host "Verificando endpoints dos serviços..." -ForegroundColor Cyan
foreach ($service in $services) {
	$readyAddresses = kubectl get endpoints $service.Name -n $Namespace -o jsonpath='{.subsets[*].addresses[*].ip}'
	if ([string]::IsNullOrWhiteSpace($readyAddresses)) {
		throw "Serviço '$($service.Name)' sem endpoints prontos no namespace '$Namespace'."
	}

	Write-Host "Serviço '$($service.Name)' possui endpoints: $readyAddresses" -ForegroundColor Green
}

Write-Host "Smoke test Kubernetes concluído com sucesso." -ForegroundColor Green
