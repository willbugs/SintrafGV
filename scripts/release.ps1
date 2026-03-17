# Release: para pool IIS, publica backend e build dos frontends.
# NAO alterar pasta de publish nem criar subpastas. Caminho unico usado pelo IIS:
#   D:\progs\Sintrafgv\src\backend\publish

$ErrorActionPreference = "Stop"
$PublishPath = "D:\progs\Sintrafgv\src\backend\publish"
$ApiProject = "D:\progs\Sintrafgv\src\backend\src\SintrafGv.Api\SintrafGv.Api.csproj"
$PoolName = "SintrafGv Api"

Write-Host "Pasta de publish (fixa): $PublishPath" -ForegroundColor Cyan
Write-Host "Parando pool: $PoolName" -ForegroundColor Yellow
Import-Module WebAdministration -ErrorAction Stop
Stop-WebAppPool -Name $PoolName
Start-Sleep -Seconds 2

Write-Host "Publicando backend em Release -> $PublishPath" -ForegroundColor Yellow
dotnet publish $ApiProject -c Release -o $PublishPath
if ($LASTEXITCODE -ne 0) { throw "dotnet publish falhou" }

Write-Host "Build frontend admin" -ForegroundColor Yellow
Set-Location "D:\progs\Sintrafgv\src\frontend\admin"
npm run build
if ($LASTEXITCODE -ne 0) { throw "npm run build admin falhou" }

Write-Host "Build frontend voting" -ForegroundColor Yellow
Set-Location "D:\progs\Sintrafgv\src\frontend\voting"
npm run build
if ($LASTEXITCODE -ne 0) { throw "npm run build voting falhou" }

Write-Host "Iniciando pool: $PoolName" -ForegroundColor Yellow
Start-WebAppPool -Name $PoolName
Start-Sleep -Seconds 2
$state = (Get-Item "IIS:\AppPools\$PoolName").State
Write-Host "Pool estado: $state" -ForegroundColor Green
Write-Host "Release concluido. Publish em: $PublishPath" -ForegroundColor Green
