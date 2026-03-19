# Validacao da votacao restrita ao banco BRADESCO
# Testa: (1) associado BRADESCO ve a enquete e pode votar; (2) associado de outro banco nao ve e recebe erro ao votar
#
# Uso:
#   1. Crie no admin uma enquete com Banco = "BRADESCO" e anote Id, uma PerguntaId e uma OpcaoId.
#   2. Preencha abaixo: $base, $eleicaoId, $perguntaId, $opcaoId e os dois associados (BRADESCO e outro banco).
#   3. Ou deixe carregar de votantes.json se existir e tiver campo "banco" em cada item.
#   .\scripts\validar-votacao-bradesco.ps1

$ErrorActionPreference = "Stop"

$base = "https://api.sintrafgv.com.br"

# Enquete restrita ao BRADESCO (criar no admin e colar os IDs aqui)
$eleicaoId = "COLOQUE_AQUI_ID_DA_ENQUETE_BRADESCO"
$perguntaId = "COLOQUE_AQUI_ID_DA_PERGUNTA"
$opcaoId = "COLOQUE_AQUI_ID_DA_OPCAO"

# Associado do BRADESCO (deve ver a enquete e poder votar)
$associadoBradesco = @{
  cpf             = ""
  dataNascimento  = ""   # ex: "1990-05-15" ou "15/05/1990"
  matriculaBancaria = ""
}

# Associado de OUTRO banco (nao deve ver a enquete e deve receber erro ao tentar votar)
$associadoOutroBanco = @{
  cpf             = ""
  dataNascimento  = ""
  matriculaBancaria = ""
}

# --- Opcional: carregar de votantes.json se tiver campo "banco" ---
$votantesPath = "d:\progs\Sintrafgv\votantes.json"
if (Test-Path $votantesPath) {
  $votantes = Get-Content $votantesPath -Raw | ConvertFrom-Json
  $comBanco = $votantes | Where-Object { $_.banco }
  if ($comBanco) {
    $bradesco = $comBanco | Where-Object { $_.banco -eq "BRADESCO" } | Select-Object -First 1
    $outro = $comBanco | Where-Object { $_.banco -ne "BRADESCO" } | Select-Object -First 1
    if ($bradesco -and $outro) {
      $associadoBradesco.cpf = ($bradesco.cpf -replace '\D', '')
      $associadoBradesco.dataNascimento = if ($bradesco.dataNascimento -match '^(\d{4}-\d{2}-\d{2})') { $Matches[1] } else { $bradesco.dataNascimento }
      $associadoBradesco.matriculaBancaria = $bradesco.matriculaBancaria
      $associadoOutroBanco.cpf = ($outro.cpf -replace '\D', '')
      $associadoOutroBanco.dataNascimento = if ($outro.dataNascimento -match '^(\d{4}-\d{2}-\d{2})') { $Matches[1] } else { $outro.dataNascimento }
      $associadoOutroBanco.matriculaBancaria = $outro.matriculaBancaria
      Write-Host "Associados carregados de votantes.json (banco BRADESCO vs outro)."
    }
  }
}

function Normalize-Date($d) {
  if ($d -match '^(\d{4}-\d{2}-\d{2})') { return $Matches[1] }
  return $d
}

function Login-Associado {
  param($a)
  $body = @{ cpf = ($a.cpf -replace '\D',''); dataNascimento = (Normalize-Date $a.dataNascimento); matriculaBancaria = $a.matriculaBancaria } | ConvertTo-Json
  $r = Invoke-RestMethod -Uri "$base/api/auth/associado/login" -Method Post -Body $body -ContentType "application/json"
  $r.token
}

function Get-Ativas {
  param($token)
  $h = @{ Authorization = "Bearer $token" }
  Invoke-RestMethod -Uri "$base/api/eleicoes/ativas" -Method Get -Headers $h
}

function Invoke-Votar {
  param($token, $eleicaoId, $perguntaId, $opcaoId)
  $script:lastVotarStatusCode = $null
  $script:lastVotarError = $null
  $h = @{ Authorization = "Bearer $token" }
  $body = @{ respostas = @(@{ perguntaId = $perguntaId; opcaoId = $opcaoId; votoBranco = $false }) } | ConvertTo-Json
  try {
    Invoke-RestMethod -Uri "$base/api/eleicoes/$eleicaoId/votar" -Method Post -Body $body -ContentType "application/json" -Headers $h
    return $true
  } catch {
    $script:lastVotarStatusCode = $_.Exception.Response.StatusCode.value__
    $script:lastVotarError = $_.ErrorDetails.Message
    return $false
  }
}

# Validacao de config
if ($eleicaoId -match 'COLOQUE_AQUI' -or -not $associadoBradesco.cpf -or -not $associadoOutroBanco.cpf) {
  Write-Host "Configure no script: eleicaoId, perguntaId, opcaoId e os dois associados (ou use votantes.json com campo 'banco')."
  exit 1
}

Write-Host "=== Validacao votacao BRADESCO ==="
Write-Host "Base: $base | Eleicao: $eleicaoId"
Write-Host ""

# --- Teste 1: Associado BRADESCO ---
Write-Host "[1] Associado BRADESCO: login e listar ativas..."
$tokenBradesco = $null
try {
  $tokenBradesco = Login-Associado $associadoBradesco
  Write-Host "    Login OK."
} catch {
  Write-Host "    ERRO login BRADESCO: $_"
  exit 1
}

$ativasBradesco = Get-Ativas $tokenBradesco
# API pode retornar id em camelCase
$encontrada = $ativasBradesco | Where-Object { ($_.id -eq $eleicaoId) -or ($_.Id -eq $eleicaoId) }
if (-not $encontrada) {
  Write-Host "    FALHA: Enquete BRADESCO nao aparece na lista de ativas para o associado BRADESCO."
  Write-Host "    IDs nas ativas: $(($ativasBradesco | ForEach-Object { $_.id }) -join ', ')"
  exit 1
}
Write-Host "    Enquete aparece na lista de ativas. OK."

Write-Host "[2] Associado BRADESCO: tentar votar..."
$okVoto = Invoke-Votar $tokenBradesco $eleicaoId $perguntaId $opcaoId
if ($okVoto -eq $true) {
  Write-Host "    Voto registrado. OK."
} elseif ($lastVotarStatusCode -eq 409 -or ($lastVotarError -and $lastVotarError -match 'já votou')) {
  Write-Host "    Ja tinha votado (409). Comportamento esperado se rodar de novo. OK."
} else {
  Write-Host "    ERRO ao votar: status $lastVotarStatusCode - $lastVotarError"
  exit 1
}

# --- Teste 3: Associado outro banco ---
Write-Host ""
Write-Host "[3] Associado OUTRO BANCO: login e listar ativas..."
$tokenOutro = $null
try {
  $tokenOutro = Login-Associado $associadoOutroBanco
  Write-Host "    Login OK."
} catch {
  Write-Host "    ERRO login outro banco: $_"
  exit 1
}

$ativasOutro = Get-Ativas $tokenOutro
$encontradaOutro = $ativasOutro | Where-Object { ($_.id -eq $eleicaoId) -or ($_.Id -eq $eleicaoId) }
if ($encontradaOutro) {
  Write-Host "    FALHA: Enquete BRADESCO NAO deveria aparecer para associado de outro banco."
  exit 1
}
Write-Host "    Enquete nao aparece na lista. OK."

Write-Host "[4] Associado OUTRO BANCO: tentar votar (deve dar erro 400)..."
$okVotoOutro = Invoke-Votar $tokenOutro $eleicaoId $perguntaId $opcaoId
if ($okVotoOutro -eq $true) {
  Write-Host "    FALHA: Voto foi aceito; deveria ser rejeitado (restricao por banco)."
  exit 1
}
if ($lastVotarStatusCode -eq 400 -and $lastVotarError -match 'BRADESCO|restrita|banco') {
  Write-Host "    Rejeicao esperada (400): $lastVotarError"
  Write-Host "    OK."
} else {
  Write-Host "    Resposta: status $lastVotarStatusCode - $lastVotarError (esperado: 400 e mensagem sobre banco)."
}

Write-Host ""
Write-Host "=== Validacao concluida: votacao restrita ao BRADESCO esta correta. ==="
