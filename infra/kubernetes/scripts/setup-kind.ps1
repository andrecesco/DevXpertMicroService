param(
	[string]$ClusterName = 'eduonline',
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

# ---------------------------------------------------------------------------
# 1. Criar o cluster Kind (se ainda não existir)
# ---------------------------------------------------------------------------
$clusterExists = kind get clusters | Select-String -SimpleMatch $ClusterName
if (-not $clusterExists) {
	Write-Host "Criando cluster Kind '$ClusterName'..." -ForegroundColor Cyan
	kind create cluster --name $ClusterName --config (Join-Path $PSScriptRoot '..\kind-config.yaml')
}

kubectl config use-context "kind-$ClusterName" | Out-Null

# ---------------------------------------------------------------------------
# 2. Build das imagens dos microsserviços via docker compose
# ---------------------------------------------------------------------------
$env:IMAGE_TAG = $ImageTag
Write-Host "Executando 'docker compose build' para os microsserviços em '$repoRoot' com tag '$ImageTag'..." -ForegroundColor Cyan
docker compose --project-directory $repoRoot build @composeServices

# ---------------------------------------------------------------------------
# 3. Carregar no Kind as mesmas imagens referenciadas nos Deployments K8s
# ---------------------------------------------------------------------------
foreach ($image in $images) {
	Write-Host "Carregando '$image' no cluster Kind '$ClusterName'..." -ForegroundColor Cyan
	kind load docker-image $image --name $ClusterName
}

# ---------------------------------------------------------------------------
# 4. Aplicar os manifestos Kubernetes
# ---------------------------------------------------------------------------
Write-Host "Aplicando manifestos Kubernetes com tag '$ImageTag'..." -ForegroundColor Cyan
& (Join-Path $PSScriptRoot 'apply.ps1') -Namespace 'eduonline' -ImageTag $ImageTag -SkipWait

Write-Host "Setup concluído." -ForegroundColor Green
