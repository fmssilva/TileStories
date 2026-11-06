# The self-test, headless: powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1 [-tabs guide,words] [-single] [-minutes 90]
# No -tabs: every tab, as three walks side by side (card / guide / marker,words,global), summed (each tab starts from the
# same state, so the sum is the one walk's result; one walk of every tab, -single, is many times slower: each control costs
# more the longer a walk runs). -tabs walks only those (the texts come with "words"), each tab list in its own Edge profile
# and files (dom_<tabs>.txt, selftest_<tabs>.json). -hash opens another route instead.
param([string]$hash = "selftest", [int]$minutes = 90, [string]$tabs = "", [switch]$single)
$edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$d = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if (-not $tabs -and -not $single -and $hash -eq "selftest") {
  $me = $MyInvocation.MyCommand.Path; $t0 = Get-Date
  $js = @("card", "guide", "marker,words,global") | ForEach-Object { Start-Job -ScriptBlock { param($me, $t, $m) & powershell -ExecutionPolicy Bypass -File $me -tabs $t -minutes $m } -ArgumentList $me, $_, $minutes }
  $lines = $js | Wait-Job | Receive-Job; $js | Remove-Job
  $sum = $lines | Where-Object { $_ -match '^ok=(\d+) behaviour=(\d+) bad=(\d+)' } | ForEach-Object { , [int[]]@($Matches[1], $Matches[2], $Matches[3]) }
  if (@($sum).Count -ne 3 -or ($lines -match '^TIMEOUT|^no json')) { $lines; "INCOMPLETE: a walk did not finish"; exit 1 }
  $lines | Where-Object { $_ -notmatch 'ProtocolLaunch' }
  "ok={0} behaviour={1} bad={2} (three walks, {3:N1} min)" -f ($sum | ForEach-Object { $_[0] } | Measure-Object -Sum).Sum, ($sum | ForEach-Object { $_[1] } | Measure-Object -Sum).Sum, ($sum | ForEach-Object { $_[2] } | Measure-Object -Sum).Sum, ((Get-Date) - $t0).TotalMinutes
  exit
}
$sfx = ""
if ($tabs -and $hash -eq "selftest") { $hash = "selftest=$tabs"; $sfx = "_" + ($tabs -replace "[^a-z]", "") }
$prof = "edgeprof11$sfx"; $dom = "$d\dom$sfx.txt"; $err = "$d\err$sfx.txt"
$url = "file:///$($d -replace '\\','/')/block-components-lab.html#$hash"
Remove-Item $dom, $err -ErrorAction SilentlyContinue
$p = Start-Process -FilePath $edge -ArgumentList @('--headless=new', '--disable-gpu', '--window-size=1400,1500', '--virtual-time-budget=15000', '--enable-logging=stderr', '--v=0', "--user-data-dir=$d\$prof", '--dump-dom', $url) -PassThru -WindowStyle Hidden -RedirectStandardOutput $dom -RedirectStandardError $err
if (-not $p.WaitForExit($minutes * 60000)) {
  Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" | Where-Object { $_.CommandLine -like "*$prof[ `"]*" } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
  "TIMEOUT"; exit
}
Get-Content $err | Where-Object { $_ -match 'CONSOLE|Uncaught' } | Select-Object -First 15
$out = Get-Content $dom -Raw
$m = [regex]::Match($out, 'data-json="([^"]*)"')
if ($m.Success) {
  $j = [System.Net.WebUtility]::HtmlDecode($m.Groups[1].Value); $j | Set-Content "$d\selftest$sfx.json" -Encoding utf8
  $o = $j | ConvertFrom-Json; "ok={0} behaviour={1} bad={2}{3}" -f $o.ok, $o.behaviour, $o.bad.Count, $(if ($tabs) { " (tabs: $tabs)" } else { "" })
  $o.bad | ForEach-Object { "{0} | {1} | {2} = {3} | {4}" -f $_.tab, $_.label, $_.key, $_.v, $_.result } | Select-Object -First 80
} elseif ($hash -like "selftest*") { "no json; length " + $out.Length }
