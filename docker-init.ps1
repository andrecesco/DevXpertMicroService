#!/usr/bin/env pwsh

<#
.SYNOPSIS
	Script de inicialização do ambiente Docker para a Plataforma EduOnline

.DESCRIPTION
	Automatiza o build e inicialização dos containers Docker

.PARAMETER Action
	Ação a executar: up, down, logs, build, clean, status, restart

.EXAMPLE
	.\docker-init.ps1 -Action up
	.\docker-init.ps1 -Action logs -Follow
	.\docker-init.ps1 -Action down -RemoveVolumes
#>

param(
	[Parameter(Mandatory = $true)]
	[ValidateSet('up', 'down', 'logs', 'build', 'clean', 'status', 'restart', 'help')]
	[string]$Action,

	[switch]$Follow,
	[switch]$RemoveVolumes,
	[switch]$NoBuild
)

$ErrorActionPreference = "Stop"

$RepoRoot = $PSScriptRoot

function Invoke-DockerCompose {
	param(
		[Parameter(ValueFromRemainingArguments = $true)]
		[string[]]$ComposeArgs
	)

	docker compose --project-directory $RepoRoot @ComposeArgs
}

function Show-Header {
	Write-Host ""
	Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
	Write-Host "║        EduOnline Platform - Docker Environment            ║" -ForegroundColor Cyan
	Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
	Write-Host ""
}

function Show-Help {
	Write-Host @"
USAGE:
	.\docker-init.ps1 -Action [action] [options]

ACTIONS:
	up          - Subir todos os containers (com build automático)
	down        - Desligar todos os containers
	logs        - Mostrar logs dos containers
	build       - Compilar as imagens Docker
	clean       - Remover containers, imagens e volumes
	status      - Mostrar status dos containers
	restart     - Reiniciar todos os containers
	help        - Mostrar esta mensagem

OPTIONS:
	-Follow                 - Seguir logs em tempo real (com logs)
	-RemoveVolumes          - Remover volumes ao desligar (com down)
	-NoBuild                - Não rebuildar ao subir (com up)

EXAMPLES:
	.\docker-init.ps1 -Action up
	.\docker-init.ps1 -Action logs -Follow
	.\docker-init.ps1 -Action down -RemoveVolumes
	.\docker-init.ps1 -Action build
	.\docker-init.ps1 -Action clean
"@
}

function Test-Docker {
	Write-Host "🔍 Verificando Docker..." -ForegroundColor Yellow

	try {
		$version = docker --version
		Write-Host "✅ Docker: $version" -ForegroundColor Green
	}
	catch {
		Write-Host "❌ Docker não instalado ou não está no PATH" -ForegroundColor Red
		exit 1
	}

	try {
		$composeVersion = docker compose version
		Write-Host "✅ Docker Compose: $composeVersion" -ForegroundColor Green
	}
	catch {
		Write-Host "❌ Docker Compose não está disponível" -ForegroundColor Red
		exit 1
	}
}

function Invoke-Up {
	Write-Host ""
	Write-Host "🚀 Iniciando containers..." -ForegroundColor Green

	$buildFlag = if ($NoBuild) { @() } else { @('--build') }
	Invoke-DockerCompose up -d @buildFlag

	Start-Sleep -Seconds 5
	Invoke-Status

	Write-Host ""
	Write-Host "✅ Plataforma iniciada com sucesso!" -ForegroundColor Green
	Write-Host ""
	Write-Host "📍 Acessar APIs:" -ForegroundColor Cyan
	Write-Host "   Auth API:       http://localhost:5000/swagger" -ForegroundColor White
	Write-Host "   Conteúdos API:  http://localhost:5001/swagger" -ForegroundColor White
	Write-Host "   Alunos API:     http://localhost:5002/swagger" -ForegroundColor White
	Write-Host "   Pagamentos API: http://localhost:5003/swagger" -ForegroundColor White
	Write-Host "   BFF API:        http://localhost:5004/swagger" -ForegroundColor White
	Write-Host ""
}

function Invoke-Down {
	Write-Host ""
	Write-Host "⏹️  Desligando containers..." -ForegroundColor Yellow

	$volumeFlag = if ($RemoveVolumes) { @('-v') } else { @() }
	Invoke-DockerCompose down @volumeFlag

	Write-Host "✅ Containers desligados" -ForegroundColor Green
	Write-Host ""
}

function Invoke-Logs {
	Write-Host ""
	Write-Host "📋 Exibindo logs..." -ForegroundColor Cyan

	$followFlag = if ($Follow) { @('-f') } else { @() }
	Invoke-DockerCompose logs @followFlag
}

function Invoke-Build {
	Write-Host ""
	Write-Host "🔨 Compilando imagens Docker..." -ForegroundColor Green

	Invoke-DockerCompose build --no-cache

	Write-Host "✅ Imagens compiladas com sucesso" -ForegroundColor Green
	Write-Host ""
}

function Invoke-Clean {
	Write-Host ""
	Write-Host "🧹 Limpando ambiente Docker..." -ForegroundColor Red

	Write-Host "   Removendo containers..." -ForegroundColor Yellow
	Invoke-DockerCompose down -v

	Write-Host "   Removendo imagens..." -ForegroundColor Yellow
	Invoke-DockerCompose down --rmi all

	Write-Host "✅ Limpeza concluída" -ForegroundColor Green
	Write-Host ""
}

function Invoke-Status {
	Write-Host ""
	Write-Host "📊 Status dos containers:" -ForegroundColor Cyan
	Write-Host ""

	Invoke-DockerCompose ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

	Write-Host ""
}

function Invoke-Restart {
	Write-Host ""
	Write-Host "🔄 Reiniciando containers..." -ForegroundColor Yellow

	Invoke-DockerCompose restart

	Start-Sleep -Seconds 3
	Invoke-Status

	Write-Host "✅ Containers reiniciados" -ForegroundColor Green
	Write-Host ""
}

# ============================================================================
# MAIN
# ============================================================================

Show-Header

if ($Action -eq 'help') {
	Show-Help
	exit 0
}

Test-Docker

switch ($Action) {
	'up'      { Invoke-Up }
	'down'    { Invoke-Down }
	'logs'    { Invoke-Logs }
	'build'   { Invoke-Build }
	'clean'   { Invoke-Clean }
	'status'  { Invoke-Status }
	'restart' { Invoke-Restart }
}
