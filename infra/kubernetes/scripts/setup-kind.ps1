param(
	[string]$ClusterName = 'eduonline'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..'))
$composeServices = @('auth-api', 'conteudos-api', 'alunos-api', 'pagamentos-api', 'bff-api', 'status-api')
$images = @(
	'andrecesco/eduonline-auth-api:latest',
	'andrecesco/eduonline-conteudos-api:latest',
	'andrecesco/eduonline-alunos-api:latest',
	'andrecesco/eduonline-pagamentos-api:latest',
	'andrecesco/eduonline-bff:latest',
	'andrecesco/eduonline-status:latest'
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
Write-Host "Executando 'docker compose build' para os microsserviços em '$repoRoot'..." -ForegroundColor Cyan
docker compose --project-directory $repoRoot build @composeServices

# ---------------------------------------------------------------------------
# 3. Carregar no Kind as mesmas imagens referenciadas nos Deployments K8s
# ---------------------------------------------------------------------------
foreach ($image in $images) {
	Write-Host "Carregando '$image' no cluster Kind '$ClusterName'..." -ForegroundColor Cyan
	kind load docker-image $image --name $ClusterName
}

# ---------------------------------------------------------------------------
# 4. Instalar CRDs de cert-manager e external-secrets (necessários para TLS e Vault)
# ---------------------------------------------------------------------------
Write-Host "Instalando CRDs do cert-manager..." -ForegroundColor Cyan
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.3/cert-manager.crds.yaml

Write-Host "Instalando CRDs do external-secrets-operator..." -ForegroundColor Cyan
kubectl apply -f https://raw.githubusercontent.com/external-secrets/external-secrets/v0.9.13/deploy/crds/bundle.yaml

# ---------------------------------------------------------------------------
# 5. Aplicar os manifestos Kubernetes
# ---------------------------------------------------------------------------
Write-Host "Aplicando manifestos Kubernetes..." -ForegroundColor Cyan
kubectl apply -k (Join-Path $PSScriptRoot '..')

# ---------------------------------------------------------------------------
# 6. Ajustar replicas para 1 e aumentar progressDeadlineSeconds no smoke-test
#    (Kind nao tem infraestrutura real; 1 replica e suficiente para validar)
# ---------------------------------------------------------------------------
Write-Host "Ajustando replicas=1 e progressDeadlineSeconds=900 para smoke-test..." -ForegroundColor Cyan
$apiDeployments = @('auth-api', 'conteudos-api', 'alunos-api', 'pagamentos-api', 'bff-api')
foreach ($dep in $apiDeployments) {
	kubectl scale deployment/$dep -n eduonline --replicas=1
	kubectl patch deployment/$dep -n eduonline --type=merge --patch '{"spec":{"progressDeadlineSeconds":900}}'
}

Write-Host "Setup concluído." -ForegroundColor Green
