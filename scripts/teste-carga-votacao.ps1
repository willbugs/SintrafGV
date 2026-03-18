# Teste de carga votacao - login associado + votar em producao
# Uso: .\scripts\teste-carga-votacao.ps1
$ErrorActionPreference = "Stop"
$base = "https://api.sintrafgv.com.br"
$eleicaoId = "2c369c97-ae0a-4084-94fa-9c434084181f"
$perguntaId = "d76b21ed-3a74-4a13-92c3-14379397f691"
$opcaoSim = "a1afca33-eb15-4131-b4b6-5785205d7ae4"
$opcaoNao = "c9f71d8b-cdff-4fbe-84ff-f0aa52fe2763"

# Carregar votantes
$votantesPath = "d:\progs\Sintrafgv\votantes.json"
if (-not (Test-Path $votantesPath)) {
  Write-Host "Execute primeiro a listagem de associados (votantes.json nao encontrado)."
  exit 1
}
$votantes = Get-Content $votantesPath -Raw | ConvertFrom-Json
$total = $votantes.Count
Write-Host "Votantes carregados: $total"
if ($total -eq 0) { exit 1 }

# Offset (pular os que ja votaram) e limite
$offset = 80
$limite = [Math]::Min(70, [Math]::Max(0, $total - $offset))
Write-Host "Disparando $limite votos (offset $offset)..."

$votoBody = @{ respostas = @(@{ perguntaId = $perguntaId; opcaoId = $opcaoSim; votoBranco = $false }) } | ConvertTo-Json
$ok = 0
$err = 0
$jatem = 0
$outros = 0
$tempos = @()

for ($i = 0; $i -lt $limite; $i++) {
  $v = $votantes[$offset + $i]
  $cpf = $v.cpf -replace '\D',''
  $dataNasc = $v.dataNascimento
  if ($dataNasc -match '^(\d{4}-\d{2}-\d{2})') { $dataNasc = $Matches[1] }
  $mat = $v.matriculaBancaria
  $sw = [System.Diagnostics.Stopwatch]::StartNew()
  try {
    $loginBody = @{ cpf = $cpf; dataNascimento = $dataNasc; matriculaBancaria = $mat } | ConvertTo-Json
    $loginResp = Invoke-RestMethod -Uri "$base/api/auth/associado/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResp.token
    $h = @{ Authorization = "Bearer $token" }
    $r = Invoke-RestMethod -Uri "$base/api/eleicoes/$eleicaoId/votar" -Method Post -Body $votoBody -ContentType "application/json" -Headers $h
    $sw.Stop()
    $tempos += $sw.ElapsedMilliseconds
    $ok++
    if ($ok % 10 -eq 0) { Write-Host "  OK $ok" }
  } catch {
    $sw.Stop()
    $code = $_.Exception.Response.StatusCode.value__
    if ($code -eq 409 -or ($_.Exception.Response.Body -and ($_.ErrorDetails.Message -match 'já votou'))) { $jatem++ }
    elseif ($code -eq 400 -or $code -eq 401) { $outros++ }
    else { $err++ }
    if (($jatem + $outros + $err) % 10 -eq 0 -and ($jatem + $outros + $err) -gt 0) {
      Write-Host "  Erros: ja votou=$jatem outros=$outros ex=$err"
    }
  }
}

Write-Host ""
Write-Host "=== Resultado ==="
Write-Host "Votos registrados: $ok"
Write-Host "Ja tinha votado: $jatem"
Write-Host "Outros erros (4xx): $outros"
Write-Host "Excecao/5xx: $err"
if ($tempos.Count -gt 0) {
  $tempos | Measure-Object -Average -Minimum -Maximum | ForEach-Object {
    Write-Host "Tempo (ms) - Min: $($_.Minimum) Media: $([math]::Round($_.Average,0)) Max: $($_.Maximum)"
  }
}
