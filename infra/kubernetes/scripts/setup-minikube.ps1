Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..'))
$composeServices = @('auth-api', 'conteudos-api', 'alunos-api', 'pagamentos-api', 'bff-api')
$images = @(
    'andrecesco/eduonline-auth-api:latest',
    'andrecesco/eduonline-conteudos-api:latest',
    'andrecesco/eduonline-alunos-api:latest',
    'andrecesco/eduonline-pagamentos-api:latest',
    'andrecesco/eduonline-bff:latest'
)

minikube start --cpus=4 --memory=8192 --disk-size=40g
minikube addons enable ingress

Write-Host "Executando 'docker compose build' para os microsserviços em '$repoRoot'..." -ForegroundColor Cyan
docker compose --project-directory $repoRoot build @composeServices

foreach ($image in $images) {
    Write-Host "Carregando '$image' no Minikube..." -ForegroundColor Cyan
    minikube image load $image
}

kubectl apply -k (Join-Path $PSScriptRoot '..')
