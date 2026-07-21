param(
	[string]$ClusterName = 'eduonline'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..'))
$composeServices = @('auth-api', 'conteudos-api', 'alunos-api', 'pagamentos-api', 'bff-api')

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
# 3. Mapeamento: imagem gerada pelo compose -> nome usado nos deployments K8s
# ---------------------------------------------------------------------------
$images = @(
	@{ Compose = 'andrecesco/eduonline-auth-api:latest';       K8s = 'eduonline/auth-api:latest' },
	@{ Compose = 'andrecesco/eduonline-conteudos-api:latest';  K8s = 'eduonline/conteudos-api:latest' },
	@{ Compose = 'andrecesco/eduonline-alunos-api:latest';     K8s = 'eduonline/alunos-api:latest' },
	@{ Compose = 'andrecesco/eduonline-pagamentos-api:latest'; K8s = 'eduonline/pagamentos-api:latest' },
	@{ Compose = 'andrecesco/eduonline-bff:latest';            K8s = 'eduonline/bff-api:latest' }
)

# ---------------------------------------------------------------------------
# 4. Re-tagear e carregar cada imagem no cluster Kind
# ---------------------------------------------------------------------------
foreach ($img in $images) {
	Write-Host "Tagueando '$($img.Compose)' -> '$($img.K8s)'..." -ForegroundColor Cyan
	docker tag $img.Compose $img.K8s

	Write-Host "Carregando '$($img.K8s)' no cluster Kind '$ClusterName'..." -ForegroundColor Cyan
	kind load docker-image $img.K8s --name $ClusterName
}

# ---------------------------------------------------------------------------
# 5. Aplicar os manifestos Kubernetes
# ---------------------------------------------------------------------------
Write-Host "Aplicando manifestos Kubernetes..." -ForegroundColor Cyan
kubectl apply -k (Join-Path $PSScriptRoot '..')

Write-Host "Setup concluído." -ForegroundColor Green
