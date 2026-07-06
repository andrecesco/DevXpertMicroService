param(
	[string]$Namespace = 'eduonline'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

kubectl apply -k (Join-Path $PSScriptRoot '..')
kubectl wait --for=condition=available deployment --all -n $Namespace --timeout=600s
