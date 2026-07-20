$logPath = "C:\Users\franc\AppData\Local\Unity\Editor\Editor.log"

if (-not (Test-Path $logPath)) {
    Write-Host "Log file not found at: $logPath" -ForegroundColor Yellow
    exit 1
}

$lines = Get-Content $logPath -Tail 300
$errors   = $lines | Where-Object { $_ -match "^Assets.*error CS" }
$warnings = $lines | Where-Object { $_ -match "^Assets.*warning CS" }

if ($errors.Count -eq 0) {
    Write-Host "NO COMPILE ERRORS" -ForegroundColor Green
} else {
    Write-Host "COMPILE ERRORS ($($errors.Count)):" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
}

if ($warnings.Count -gt 0) {
    Write-Host "Warnings ($($warnings.Count)):" -ForegroundColor Yellow
    $warnings | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}
