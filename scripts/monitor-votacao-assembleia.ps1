# Monitor da assembleia 452221F5-6C7B-4F59-8A68-34296A50BD64
$ErrorActionPreference = 'Continue'
$EnqueteId = '452221F5-6C7B-4F59-8A68-34296A50BD64'
$BrTzId = 'E. South America Standard Time'
$SqlServer = '127.0.0.1'
$SqlUser = 'Durval'
$SqlPass = 'Lspxmw01oz'
$SqlDb = 'Sintraf_GV'

function Get-HoraBr {
    [TimeZoneInfo]::ConvertTimeBySystemTimeZoneId([DateTime]::UtcNow, $BrTzId)
}

function Get-VotosSql {
    $q = "SELECT COUNT(*) FROM Votos WHERE EleicaoId='$EnqueteId'"
    $out = sqlcmd -S $SqlServer -U $SqlUser -P $SqlPass -d $SqlDb -Q $q -W -h -1 2>$null
    return [int]($out | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1)
}

function Get-EnqueteId($item) {
    if ($item.id) { return $item.id.ToString() }
    if ($item.Id) { return $item.Id.ToString() }
    return ''
}

function Get-StatusApi {
    $body = @{ cpf = '66984076668'; dataNascimento = '02/10/1965'; matriculaBancaria = '5172446' } | ConvertTo-Json
    $login = Invoke-RestMethod -Uri 'https://api.sintrafgv.com.br/api/auth/associado/login' -Method POST -Body $body -ContentType 'application/json' -TimeoutSec 20
    $ativas = Invoke-RestMethod -Uri 'https://api.sintrafgv.com.br/api/enquetes/ativas' -Headers @{ Authorization = "Bearer $($login.token)" } -TimeoutSec 20
    $e = $null
    foreach ($item in $ativas) {
        if ((Get-EnqueteId $item).ToUpper() -eq $EnqueteId.ToUpper()) {
            $e = $item
            break
        }
    }
    if (-not $e) {
        return @{ ok = $false; msg = 'Enquete nao listada'; totalVotos = 0; podeVotar = $false }
    }
    $pv = $e.podeVotar
    if ($null -eq $pv) { $pv = $e.PodeVotar }
    $tv = $e.totalVotos
    if ($null -eq $tv) { $tv = $e.TotalVotos }
    return @{
        ok = $true
        podeVotar = [bool]$pv
        totalVotos = [int]$tv
    }
}

$agora = Get-HoraBr
Write-Host "MONITOR: aguardando 09:00 BRT (agora $($agora.ToString('HH:mm:ss')))..."
while ((Get-HoraBr).TimeOfDay -lt (New-TimeSpan -Hours 9)) {
    Start-Sleep -Seconds 15
}

Write-Host "MONITOR: INICIO $(Get-HoraBr -Format 'yyyy-MM-dd HH:mm:ss') BRT"
$ultimoVotos = -1
$fimMonitor = (Get-HoraBr).Date.AddHours(18).AddMinutes(5)
$limiteAlerta = (New-TimeSpan -Hours 9).Add((New-TimeSpan -Minutes 5))

while ((Get-HoraBr) -lt $fimMonitor) {
    $hora = Get-HoraBr -Format 'HH:mm:ss'
    $votosSql = Get-VotosSql
    try {
        $api = Get-StatusApi
        if ($api.ok) {
            Write-Host "MONITOR [$hora] SQL=$votosSql API_votos=$($api.totalVotos) podeVotar=$($api.podeVotar) API_OK"
            $brNow = Get-HoraBr
            if (-not $api.podeVotar -and $brNow.TimeOfDay -ge (New-TimeSpan -Hours 9) -and $brNow.TimeOfDay -lt $limiteAlerta) {
                Write-Host "ALERTA [$hora] podeVotar=false apos 09:00 - verificar periodo"
            }
        } else {
            Write-Host "ALERTA [$hora] $($api.msg) SQL_votos=$votosSql"
        }
    } catch {
        Write-Host "ALERTA [$hora] Erro API: $($_.Exception.Message) SQL_votos=$votosSql"
    }

    if ($votosSql -gt $ultimoVotos -and $ultimoVotos -ge 0) {
        $delta = $votosSql - $ultimoVotos
        Write-Host "MONITOR [$hora] +$delta voto(s) novo(s)! Total=$votosSql"
    }
    $ultimoVotos = $votosSql

    Start-Sleep -Seconds 120
}

Write-Host "MONITOR: FIM $(Get-HoraBr -Format 'yyyy-MM-dd HH:mm:ss') BRT"
