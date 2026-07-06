param(
	[string]$ClusterName = 'eduonline'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$clusterExists = kind get clusters | Select-String -SimpleMatch $ClusterName
if (-not $clusterExists) {
	kind create cluster --name $ClusterName --config (Join-Path $PSScriptRoot '..\kind-config.yaml')
}

kubectl config use-context "kind-$ClusterName" | Out-Null
kubectl apply -k (Join-Path $PSScriptRoot '..')
