# Deploy do backend - para api.sintrafgv.com.br
# Execute como Administrador no PowerShell.
# Ajuste $AppPoolName: no IIS, Sites > api.sintrafgv.com.br > Application Pools

Import-Module WebAdministration -ErrorAction SilentlyContinue
$AppPoolName = "DefaultAppPool"  # Ajuste conforme o App Pool do site da API
$SourceDir = "$PSScriptRoot\publish-temp"
$TargetDir = "$PSScriptRoot\publish-new"

if (-not (Test-Path $SourceDir)) {
    Write-Host "ERRO: publish-temp nao encontrado. Execute: dotnet publish -c Release -o publish-temp" -ForegroundColor Red
    exit 1
}

Write-Host "Parando App Pool: $AppPoolName" -ForegroundColor Yellow
Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue

Start-Sleep -Seconds 3

Write-Host "Copiando arquivos de publish-temp para publish-new..." -ForegroundColor Yellow
Remove-Item -Path "$TargetDir\*" -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item -Path "$SourceDir\*" -Destination $TargetDir -Recurse -Force

Write-Host "Iniciando App Pool: $AppPoolName" -ForegroundColor Yellow
Start-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue

Write-Host "Deploy concluido. Aguarde alguns segundos e teste: https://api.sintrafgv.com.br/api/configuracao-email/status" -ForegroundColor Green
