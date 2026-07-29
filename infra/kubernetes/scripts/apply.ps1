param(
	[string]$Namespace = 'eduonline',
	[string]$ImageTag = 'latest'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$deploymentImages = @{
	'auth-api' = 'andrecesco/eduonline-auth-api'
	'conteudos-api' = 'andrecesco/eduonline-conteudos-api'
	'alunos-api' = 'andrecesco/eduonline-alunos-api'
	'pagamentos-api' = 'andrecesco/eduonline-pagamentos-api'
	'bff-api' = 'andrecesco/eduonline-bff'
	'status-api' = 'andrecesco/eduonline-status'
}

kubectl apply -k (Join-Path $PSScriptRoot '..')

foreach ($deployment in $deploymentImages.Keys) {
	$image = "$($deploymentImages[$deployment]):$ImageTag"
	kubectl set image "deployment/$deployment" "$deployment=$image" -n $Namespace
}

kubectl wait --for=condition=available deployment --all -n $Namespace --timeout=600s
