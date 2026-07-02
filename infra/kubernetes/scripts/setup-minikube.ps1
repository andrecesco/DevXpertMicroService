Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

minikube start --cpus=4 --memory=8192 --disk-size=40g
minikube addons enable ingress
kubectl apply -k (Join-Path $PSScriptRoot '..')
