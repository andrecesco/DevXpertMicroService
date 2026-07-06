#!/usr/bin/env pwsh

<#
.SYNOPSIS
	Validador de Workflows GitHub Actions

.DESCRIPTION
	Valida sintaxe e estrutura dos workflows
#>

param(
	[switch]$Verbose
)

Write-Host "🔍 Validando GitHub Actions Workflows..." -ForegroundColor Cyan
Write-Host ""

$workflowDir = ".\.github\workflows"
$workflows = @(
	"ci.yml",
	"cd.yml", 
	"security.yml",
	"pr.yml"
)

$allValid = $true
$results = @()

foreach ($workflow in $workflows) {
	$path = Join-Path $workflowDir $workflow

	if (-not (Test-Path $path)) {
		Write-Host "❌ $workflow - Arquivo não encontrado" -ForegroundColor Red
		$allValid = $false
		continue
	}

	try {
		$content = Get-Content $path -Raw -Encoding UTF8

		# Validações básicas
		$checks = @{
			"name" = $content -match "^name:\s*"
			"on" = $content -match "^on:\s*"
			"jobs" = $content -match "^jobs:\s*"
			"runs-on" = $content -match "runs-on:"
		}

		$allChecks = $checks.Values | Measure-Object -Sum | ForEach-Object { $_.Sum -eq $checks.Count }

		if ($allChecks) {
			Write-Host "✅ $workflow - Válido" -ForegroundColor Green

			# Extrair informações
			$nameMatch = $content | Select-String "^name:\s*(.+)" | ForEach-Object { $_.Matches.Groups[1].Value }
			if ($Verbose) {
				Write-Host "   Nome: $nameMatch" -ForegroundColor DarkGray
			}

			$results += @{
				Workflow = $workflow
				Status = "Valid"
				Name = $nameMatch
			}
		} else {
			Write-Host "⚠️  $workflow - Estrutura incompleta" -ForegroundColor Yellow
			$results += @{
				Workflow = $workflow
				Status = "Warning"
				Name = "N/A"
			}
		}
	}
	catch {
		Write-Host "❌ $workflow - Erro: $_" -ForegroundColor Red
		$allValid = $false
	}
}

Write-Host ""
Write-Host "═" * 60
Write-Host ""

if ($allValid) {
	Write-Host "✅ Todos os workflows estão válidos!" -ForegroundColor Green
	Write-Host ""
	Write-Host "Workflows prontos:" -ForegroundColor Cyan
	foreach ($result in $results) {
		Write-Host "  • $($result.Workflow) - $($result.Name)" -ForegroundColor Green
	}
} else {
	Write-Host "❌ Alguns workflows têm problemas. Verifique acima." -ForegroundColor Red
}

Write-Host ""
Write-Host "📋 Checklist de Validação:" -ForegroundColor Cyan
Write-Host ""
Write-Host "Workflows:" -ForegroundColor White
$results | ForEach-Object { 
	$icon = if ($_.Status -eq "Valid") { "✅" } else { "⚠️" }
	Write-Host "  $icon $($_.Workflow)" -ForegroundColor $(if ($_.Status -eq "Valid") { "Green" } else { "Yellow" })
}

Write-Host ""
Write-Host "Documentação:" -ForegroundColor White
@(
	"docs/CI-CD-PIPELINE.md",
	"docs/GITHUB-ACTIONS-SETUP.md",
	"sonar-project.properties"
) | ForEach-Object {
	$icon = if (Test-Path $_) { "✅" } else { "❌" }
	Write-Host "  $icon $_"
}

Write-Host ""
Write-Host "═" * 60
Write-Host ""

if ($allValid) {
	Write-Host "🚀 Pronto para usar! Execute os próximos passos:" -ForegroundColor Green
	Write-Host ""
	Write-Host "1. Configurar secrets no GitHub:" -ForegroundColor Cyan
	Write-Host "   → Leia: docs/GITHUB-ACTIONS-SETUP.md"
	Write-Host ""
	Write-Host "2. Fazer push em develop para testar CI:" -ForegroundColor Cyan
	Write-Host "   git commit --allow-empty -m 'test: trigger CI'"
	Write-Host "   git push origin develop"
	Write-Host ""
	Write-Host "3. Monitorar execução em:" -ForegroundColor Cyan
	Write-Host "   GitHub → Actions → Workflows"
	Write-Host ""
} else {
	Write-Host "❌ Corrija os problemas acima antes de fazer push" -ForegroundColor Red
}
