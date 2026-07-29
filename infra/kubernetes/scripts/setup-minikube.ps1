param(
    [string]$ImageTag = 'latest'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..'))
$composeServices = @('auth-api', 'conteudos-api', 'alunos-api', 'pagamentos-api', 'bff-api', 'status-api')
$images = @(
    "andrecesco/eduonline-auth-api:$ImageTag",
    "andrecesco/eduonline-conteudos-api:$ImageTag",
    "andrecesco/eduonline-alunos-api:$ImageTag",
    "andrecesco/eduonline-pagamentos-api:$ImageTag",
    "andrecesco/eduonline-bff:$ImageTag",
    "andrecesco/eduonline-status:$ImageTag"
)

minikube start --cpus=4 --memory=8192 --disk-size=40g
minikube addons enable ingress

$env:IMAGE_TAG = $ImageTag
Write-Host "Executando 'docker compose build' para os microsserviços em '$repoRoot' com tag '$ImageTag'..." -ForegroundColor Cyan
docker compose --project-directory $repoRoot build @composeServices

foreach ($image in $images) {
    Write-Host "Carregando '$image' no Minikube..." -ForegroundColor Cyan
    minikube image load $image
}

& (Join-Path $PSScriptRoot 'apply.ps1') -Namespace 'eduonline' -ImageTag $ImageTag
