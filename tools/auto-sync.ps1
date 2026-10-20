param(
    [int]$IntervalSeconds = 10,
    [string]$Branch = "main"
)

$projectRoot = Split-Path $PSScriptRoot -Parent
Set-Location $projectRoot

Write-Host "GHETTO BEASTS Auto-Sync" -ForegroundColor Magenta
Write-Host "Ordner: $projectRoot"
Write-Host "Pruefe alle $IntervalSeconds Sekunden auf neue Commits (Branch '$Branch'). Strg+C beendet." -ForegroundColor DarkGray

while ($true) {
    git fetch origin $Branch --quiet 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "$(Get-Date -Format HH:mm:ss) fetch fehlgeschlagen - Netzwerk/Login pruefen" -ForegroundColor Yellow
    }
    else {
        $local = git rev-parse HEAD
        $remote = git rev-parse "origin/$Branch"
        if ($local -ne $remote) {
            git pull --ff-only origin $Branch --quiet
            if ($LASTEXITCODE -eq 0) {
                $subject = git log -1 --pretty=%s
                Write-Host "$(Get-Date -Format HH:mm:ss) Update geladen: $subject" -ForegroundColor Green
                [console]::beep(880, 120)
            }
            else {
                Write-Host "$(Get-Date -Format HH:mm:ss) Pull fehlgeschlagen - lokale Aenderungen? 'git status' pruefen" -ForegroundColor Red
            }
        }
    }
    Start-Sleep -Seconds $IntervalSeconds
}
