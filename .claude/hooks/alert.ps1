# Looping attention alert for Claude Code hooks.
#   alert.ps1 start  -> plays a sound every few seconds until this session stops it (or Space is pressed)
#   alert.ps1 stop   -> stops it
#   alert.ps1 stopall -> silences every chat's alarm (bound to Ctrl+Alt+S via a Desktop shortcut)
# One PID file per Claude session (session_id from the hook's stdin JSON), so one
# chat's PostToolUse never silences another chat's alert.
# The PID file also records who started the alarm ("main" or a subagent's agent_id): a background
# subagent's own tool calls share the session_id, so its PostToolUse must never silence an alarm
# that the main conversation started (a question waiting for the developer). Fixed 2026-11-11.
param([string]$Action = "start")

$sessionId = "default"
$hookInput = $null
try {
    if (-not [Console]::IsInputRedirected) { throw "no hook input" }
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    if ($hookInput.session_id) { $sessionId = $hookInput.session_id }
} catch {}
$pidFile = Join-Path $env:TEMP "claude-alert-$sessionId.pid"
$caller = if ($hookInput -and $hookInput.agent_id) { [string]$hookInput.agent_id } else { "main" }

function Write-Log([string]$what) {
    try {
        $ev = if ($hookInput) { "$($hookInput.hook_event_name) tool=$($hookInput.tool_name)" } else { "no input" }
        $line = "{0:HH:mm:ss.fff} {1} {2} caller={3} {4}" -f (Get-Date), $what, $sessionId, $caller, $ev
        $log = Join-Path $env:TEMP "claude-alert.log"
        if ((Test-Path $log) -and (Get-Item $log).Length -gt 200KB) { Remove-Item $log -Force }
        Add-Content -Path $log -Value $line
    } catch {}
}

function Stop-Alert {
    if (Test-Path $pidFile) {
        $lines = @(Get-Content $pidFile -ErrorAction SilentlyContinue)
        if ($lines.Count -gt 0 -and $lines[0]) { Stop-Process -Id ([int]$lines[0]) -Force -ErrorAction SilentlyContinue }
        Remove-Item $pidFile -Force -ErrorAction SilentlyContinue
    }
}

if ($Action -eq "stop") {
    if (Test-Path $pidFile) {
        $lines = @(Get-Content $pidFile -ErrorAction SilentlyContinue)
        $owner = if ($lines.Count -gt 1 -and $lines[1]) { $lines[1] } else { "main" }
        # a subagent's tool call never silences an alarm someone else started
        if ($caller -ne "main" -and $caller -ne $owner) { Write-Log "stop-ignored(owner=$owner)"; exit 0 }
        Write-Log "stop"
    }
    Stop-Alert; exit 0
}

# Manual silence (Desktop shortcut / hotkey): every chat's alarm at once.
if ($Action -eq "stopall") {
    Get-ChildItem (Join-Path $env:TEMP "claude-alert-*.pid") -ErrorAction SilentlyContinue | ForEach-Object {
        $lines = @(Get-Content $_.FullName -ErrorAction SilentlyContinue)
        if ($lines.Count -gt 0 -and $lines[0]) { Stop-Process -Id ([int]$lines[0]) -Force -ErrorAction SilentlyContinue }
        Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
    }
    exit 0
}

Stop-Alert
Write-Log "start"
$loop = Join-Path $PSScriptRoot "alarm-loop.ps1"
$p = Start-Process powershell.exe -WindowStyle Hidden -PassThru `
     -ArgumentList "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "`"$loop`""
Set-Content -Path $pidFile -Value @($p.Id, $caller)
exit 0
