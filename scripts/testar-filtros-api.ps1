# Testa se os filtros de Associados e Usuarios funcionam na API.
# Uso: .\scripts\testar-filtros-api.ps1 -BaseUrl "http://localhost:5066"
# Requer: email e senha de um usuario admin (ou usa admin@sintrafgv.com.br / Admin@123)

param(
    [string]$BaseUrl = "http://localhost:5066",
    [string]$Email = "admin@sintrafgv.com.br",
    [string]$Senha = "Admin@123"
)

$ErrorActionPreference = "Stop"

Write-Host "BaseUrl: $BaseUrl" -ForegroundColor Cyan

# Login (API retorna { data: { token, user } })
$loginBody = @{ email = $Email; password = $Senha } | ConvertTo-Json
try {
    $loginResp = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json; charset=utf-8"
} catch {
    Write-Host "Falha no login (API pode estar parada ou credenciais incorretas): $_" -ForegroundColor Red
    exit 1
}
$token = $loginResp.data.token
if (-not $token) { Write-Host "Falha no login: resposta sem data.token." -ForegroundColor Red; exit 1 }
Write-Host "Login OK." -ForegroundColor Green

$headers = @{ Authorization = "Bearer $token" }

# Associados com status=Ativo
$urlAssocAtivo = "$BaseUrl/api/associados?pagina=1&porPagina=50&status=Ativo"
Write-Host "GET $urlAssocAtivo" -ForegroundColor Cyan
$rAssoc = Invoke-RestMethod -Uri $urlAssocAtivo -Headers $headers -Method Get
$itensAssoc = $rAssoc.itens
$totalAssoc = $rAssoc.total
$inativos = @($itensAssoc | Where-Object { -not $_.ativo })
if ($inativos.Count -gt 0) {
    Write-Host "FALHA Associados(status=Ativo): retornou $($inativos.Count) inativos. Total=$totalAssoc" -ForegroundColor Red
} else {
    Write-Host "OK Associados(status=Ativo): total=$totalAssoc, todos ativo=true" -ForegroundColor Green
}

# Associados com status=Inativo
$urlAssocInativo = "$BaseUrl/api/associados?pagina=1&porPagina=50&status=Inativo"
$rAssocInativo = Invoke-RestMethod -Uri $urlAssocInativo -Headers $headers -Method Get
$ativosInList = @($rAssocInativo.itens | Where-Object { $_.ativo })
if ($ativosInList.Count -gt 0) {
    Write-Host "FALHA Associados(status=Inativo): retornou $($ativosInList.Count) ativos. Total=$($rAssocInativo.total)" -ForegroundColor Red
} else {
    Write-Host "OK Associados(status=Inativo): total=$($rAssocInativo.total), todos ativo=false" -ForegroundColor Green
}

# Usuarios com ativo=true
$urlUsuAtivo = "$BaseUrl/api/usuarios?pagina=1&porPagina=50&ativo=true"
Write-Host "GET $urlUsuAtivo" -ForegroundColor Cyan
$rUsu = Invoke-RestMethod -Uri $urlUsuAtivo -Headers $headers -Method Get
$inativosUsu = @($rUsu.itens | Where-Object { -not $_.ativo })
if ($inativosUsu.Count -gt 0) {
    Write-Host "FALHA Usuarios(ativo=true): retornou $($inativosUsu.Count) inativos. Total=$($rUsu.total)" -ForegroundColor Red
} else {
    Write-Host "OK Usuarios(ativo=true): total=$($rUsu.total), todos ativo=true" -ForegroundColor Green
}

Write-Host "Fim dos testes." -ForegroundColor Cyan
