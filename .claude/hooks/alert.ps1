# Looping attention alert for Claude Code hooks.
#   alert.ps1 start  -> plays a sound every few seconds until this session stops it (or Space is pressed)
#   alert.ps1 stop   -> stops it
#   alert.ps1 stopall -> silences every chat's alarm (bound to Ctrl+Alt+S via a Desktop shortcut)
# One PID file per Claude session (session_id from the hook's stdin JSON), so one
# chat's PostToolUse never silences another chat's alert.
param([string]$Action = "start")

$sessionId = "default"
try {
    if (-not [Console]::IsInputRedirected) { throw "no hook input" }
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    if ($hookInput.session_id) { $sessionId = $hookInput.session_id }
} catch {}
$pidFile = Join-Path $env:TEMP "claude-alert-$sessionId.pid"

function Stop-Alert {
    if (Test-Path $pidFile) {
        $old = Get-Content $pidFile -ErrorAction SilentlyContinue
        if ($old) { Stop-Process -Id ([int]$old) -Force -ErrorAction SilentlyContinue }
        Remove-Item $pidFile -Force -ErrorAction SilentlyContinue
    }
}

if ($Action -eq "stop") { Stop-Alert; exit 0 }

# Manual silence (Desktop shortcut / hotkey): every chat's alarm at once.
if ($Action -eq "stopall") {
    Get-ChildItem (Join-Path $env:TEMP "claude-alert-*.pid") -ErrorAction SilentlyContinue | ForEach-Object {
        $old = Get-Content $_.FullName -ErrorAction SilentlyContinue
        if ($old) { Stop-Process -Id ([int]$old) -Force -ErrorAction SilentlyContinue }
        Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
    }
    exit 0
}

Stop-Alert
$loop = Join-Path $PSScriptRoot "alarm-loop.ps1"
$p = Start-Process powershell.exe -WindowStyle Hidden -PassThru `
     -ArgumentList "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "`"$loop`""
Set-Content -Path $pidFile -Value $p.Id
exit 0
