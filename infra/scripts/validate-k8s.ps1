param(
	[int]$TimeoutMinutes = 15,
	[string]$ClusterName = "eduonline",
	[string]$ManifestsDir = "infra/kubernetes"
)

$scriptRoot = $PSScriptRoot
$repoRoot = Resolve-Path (Join-Path $scriptRoot "..")

if (-not [System.IO.Path]::IsPathRooted($ManifestsDir)) {
	$ManifestsDir = Join-Path $repoRoot $ManifestsDir
}

function Fail([string]$msg){
	Write-Host "[ERROR] $msg" -ForegroundColor Red
	exit 2
}

function Info([string]$msg){
	Write-Host "[INFO] $msg"
}

# Check dependencies
if (-not (Get-Command kind -ErrorAction SilentlyContinue)) {
	Fail "'kind' not found in PATH. Install Kind: https://kind.sigs.k8s.io/"
}
if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
	Fail "'kubectl' not found in PATH. Install kubectl: https://kubernetes.io/docs/tasks/tools/"
}

Info "Using cluster name: $ClusterName"

# Create cluster if missing
$clusters = (& kind get clusters) -split "\r?\n"
if ($clusters -notcontains $ClusterName) {
	Info "Creating kind cluster '$ClusterName'..."
	& kind create cluster --name $ClusterName
	if ($LASTEXITCODE -ne 0) { Fail "kind create cluster failed" }
} else {
	Info "Kind cluster '$ClusterName' already exists"
}

# Ensure namespace exists (manifests may declare it; this is idempotent)
Info "Ensuring namespace 'eduonline' exists (if manifests expect it)"
& kubectl create namespace eduonline --dry-run=client -o yaml | kubectl apply -f - | Out-Null

# Apply manifests
if (-not (Test-Path $ManifestsDir)) { Fail "Manifests directory '$ManifestsDir' not found" }
Info "Applying manifests from $ManifestsDir"

$kustomizationFile = Join-Path $ManifestsDir "kustomization.yaml"
if (Test-Path $kustomizationFile) {
	& kubectl apply -k $ManifestsDir
} else {
	& kubectl apply -f $ManifestsDir
}

if ($LASTEXITCODE -ne 0) { Fail "kubectl apply failed" }

# Wait for pods readiness
$timeoutSec = $TimeoutMinutes * 60
Info "Waiting up to $TimeoutMinutes minute(s) for pods in namespace 'eduonline' to be ready..."
try {
	& kubectl wait --for=condition=ready pod --all -n eduonline --timeout=${TimeoutMinutes}m | Write-Host
} catch {
	Write-Host "[WARN] Timeout waiting pods ready. Will continue to gather diagnostics." -ForegroundColor Yellow
}

# Helper to check deployment replicas
function Check-Deployment([string]$name, [int]$expectedReplicas = 0){
	$d = & kubectl get deploy $name -n eduonline --ignore-not-found -o json | Out-String
	if ([string]::IsNullOrWhiteSpace($d)) { return @{Name=$name; Exists=$false} }
	$json = $d | ConvertFrom-Json
	$ready = if ($json.status.readyReplicas) { [int]$json.status.readyReplicas } else { 0 }
	$desired = if ($json.spec.replicas) { [int]$json.spec.replicas } else { 0 }
	return @{Name=$name; Exists=$true; Ready=$ready; Desired=$desired; MeetsExpected=($expectedReplicas -eq 0 -or $desired -eq $expectedReplicas)}
}

$apps = @("auth","conteudos","alunos","pagamentos","bff")
$failures = @()
foreach ($a in $apps) {
	$res = Check-Deployment $a 3
	if (-not $res.Exists) {
		$failures += "Deployment '$a' not found"
	} else {
		Info "Deployment '$a' desired=$($res.Desired) ready=$($res.Ready)"
		if ($res.Desired -ne 3) { $failures += "Deployment '$a' desired replicas is $($res.Desired) (expected 3)" }
		if ($res.Ready -lt $res.Desired) { $failures += "Deployment '$a' has $($res.Ready)/$($res.Desired) ready replicas" }
	}
}

# Check key observability/security components declared
$components = @("prometheus","grafana","jaeger","elasticsearch","kibana","vault","sql-server")
foreach ($c in $components) {
	$res = Check-Deployment $c
	if (-not $res.Exists) {
		Write-Host "[WARN] Component deployment '$c' not found in namespace 'eduonline'" -ForegroundColor Yellow
	} else {
		Info "Component '$c' found (desired=$($res.Desired), ready=$($res.Ready))"
	}
}

# List services
Info "Listing services in namespace 'eduonline'"
& kubectl get svc -n eduonline | Write-Host

# Summary
if ($failures.Count -eq 0) {
	Write-Host "\n[OK] Kubernetes runtime validation completed with no critical failures (checks passed for app deployments)." -ForegroundColor Green
	exit 0
} else {
	Write-Host "\n[FAIL] Kubernetes runtime validation found issues:" -ForegroundColor Red
	foreach ($f in $failures) { Write-Host " - $f" }
	Write-Host "\nRun 'kubectl describe' and 'kubectl logs' for failing resources to investigate." -ForegroundColor Yellow
	exit 3
}
