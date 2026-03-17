# Verifica processos IIS e top consumidores de memoria
$ErrorActionPreference = 'SilentlyContinue'

Write-Host "=== Processos IIS (w3wp, dotnet) ===" -ForegroundColor Cyan
Get-Process -Name w3wp*, dotnet* -ErrorAction SilentlyContinue | ForEach-Object {
    [PSCustomObject]@{
        PID      = $_.Id
        Processo = $_.ProcessName
        MemoriaMB = [math]::Round($_.WorkingSet64 / 1MB, 2)
        VirtualMB = [math]::Round($_.VirtualMemorySize64 / 1MB, 2)
    }
} | Format-Table -AutoSize

Write-Host "`n=== Top 30 processos por memoria (Working Set) ===" -ForegroundColor Cyan
Get-Process | Sort-Object WorkingSet64 -Descending | Select-Object -First 30 | ForEach-Object {
    [PSCustomObject]@{
        PID      = $_.Id
        Processo = $_.ProcessName
        MemoriaMB = [math]::Round($_.WorkingSet64 / 1MB, 2)
    }
} | Format-Table -AutoSize

Write-Host "`n=== Application Pools IIS (se modulo disponivel) ===" -ForegroundColor Cyan
try {
    Import-Module WebAdministration -ErrorAction Stop
    Get-ChildItem IIS:\AppPools | ForEach-Object {
        $name = $_.Name
        $state = $_.State
        $wp = Get-ChildItem "IIS:\AppPools\$name\WorkerProcesses" -ErrorAction SilentlyContinue
        $pids = ($wp | ForEach-Object { $_.processId }) -join ', '
        [PSCustomObject]@{ AppPool = $name; State = $state; PIDs = $pids }
    } | Format-Table -AutoSize
} catch {
    Write-Host "Modulo WebAdministration nao disponivel: $_"
}

Write-Host "`n=== Total memoria por nome de processo (agrupado) ===" -ForegroundColor Cyan
Get-Process | Group-Object ProcessName | ForEach-Object {
    $totalMB = ($_.Group | Measure-Object -Property WorkingSet64 -Sum).Sum / 1MB
    [PSCustomObject]@{
        Processo = $_.Name
        Count = $_.Count
        TotalMB = [math]::Round($totalMB, 2)
    }
} | Sort-Object TotalMB -Descending | Select-Object -First 20 | Format-Table -AutoSize
