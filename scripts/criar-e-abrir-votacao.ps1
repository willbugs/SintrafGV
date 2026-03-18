# Cria uma nova enquete e abre para votacao (uso: admin).
# Defina $env:ADMIN_EMAIL e $env:ADMIN_PASSWORD antes de executar.
# Exemplo: $env:ADMIN_EMAIL="admin@sintrafgv.com.br"; $env:ADMIN_PASSWORD="..."; .\scripts\criar-e-abrir-votacao.ps1
$ErrorActionPreference = "Stop"
$base = "https://api.sintrafgv.com.br"

if (-not $env:ADMIN_EMAIL -or -not $env:ADMIN_PASSWORD) {
  Write-Host "Defina ADMIN_EMAIL e ADMIN_PASSWORD (env)."
  exit 1
}

# Login admin
$loginBody = @{ email = $env:ADMIN_EMAIL; password = $env:ADMIN_PASSWORD } | ConvertTo-Json
$loginResp = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResp.token
if (-not $token) { Write-Host "Login falhou."; exit 1 }
Write-Host "Login admin OK."

$headers = @{ Authorization = "Bearer $token"; "Content-Type" = "application/json" }

# Datas: agora ate amanha (UTC)
$inicio = [DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
$fim = [DateTime]::UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

$createBody = @{
  titulo = "Votacao PWA teste " + (Get-Date -Format "dd/MM HH:mm")
  descricao = "Enquete criada pelo script para teste no PWA"
  inicioVotacao = $inicio
  fimVotacao = $fim
  tipo = 1
  apenasAssociados = $true
  apenasAtivos = $true
  perguntas = @(
    @{
      ordem = 1
      texto = "Voce aprova?"
      tipo = 1
      permiteBranco = $true
      opcoes = @(
        @{ ordem = 1; texto = "Sim" }
        @{ ordem = 2; texto = "Nao" }
      )
    }
  )
} | ConvertTo-Json -Depth 5

$eleicao = Invoke-RestMethod -Uri "$base/api/eleicoes" -Method Post -Body $createBody -Headers $headers
$id = if ($eleicao.id) { $eleicao.id } else { $eleicao.Id }
Write-Host "Enquete criada: $id - $($eleicao.titulo)"

# Abrir votacao (status = 2 = Aberta) - PATCH api/eleicoes/{id}/status
$statusBody = @{ status = 2 } | ConvertTo-Json
Invoke-RestMethod -Uri "$base/api/eleicoes/$id/status" -Method Patch -Body $statusBody -Headers $headers | Out-Null
Write-Host "Status alterado para Aberta."

# IDs para votar (primeira pergunta, primeira opcao)
$pergs = if ($eleicao.perguntas) { $eleicao.perguntas } else { $eleicao.Perguntas }
$p0 = $pergs[0]
$perguntaId = if ($p0.id) { $p0.id } else { $p0.Id }
$opcoes = if ($p0.opcoes) { $p0.opcoes } else { $p0.Opcoes }
$opcaoId = if ($opcoes[0].id) { $opcoes[0].id } else { $opcoes[0].Id }
Write-Host ""
Write-Host "Para votar via script, use:"
Write-Host "  `$eleicaoId = `"$id`""
Write-Host "  `$perguntaId = `"$perguntaId`""
Write-Host "  `$opcaoId = `"$opcaoId`""
Write-Host "Ou acesse no PWA: https://votacao.sintrafgv.com.br/votacao/$id"
