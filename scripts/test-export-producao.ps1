# Testa exportacao em producao via curl
# Uso: .\test-export-producao.ps1

$api = "https://api.sintrafgv.com.br"
$email = "admin@sintrafgv.com.br"
$senha = "Admin@123"

Write-Host "1. Login..." -ForegroundColor Cyan
$loginBody = @{ email = $email; password = $senha } | ConvertTo-Json
$loginResp = Invoke-RestMethod -Uri "$api/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResp.data.token
Write-Host "   Token obtido." -ForegroundColor Green

Write-Host "2. Exportar relatorio (CSV)..." -ForegroundColor Cyan
$exportBody = '{"tipoRelatorio":"associados-geral","filtros":{},"formatoExportacao":"csv"}'
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
}
try {
    $bytes = Invoke-WebRequest -Uri "$api/api/relatorios/exportar" -Method Post -Body $exportBody -Headers $headers -UseBasicParsing
    Write-Host "   OK! Tamanho: $($bytes.Content.Length) bytes" -ForegroundColor Green
    $bytes.Content | Out-File -FilePath "relatorio_teste.csv" -Encoding utf8
    Write-Host "   Salvo em relatorio_teste.csv" -ForegroundColor Green
} catch {
    Write-Host "   ERRO: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $body = $reader.ReadToEnd()
        Write-Host "   Response: $body" -ForegroundColor Red
    }
}
