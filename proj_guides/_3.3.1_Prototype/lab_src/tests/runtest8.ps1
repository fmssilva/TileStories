param([string]$hash = "selftest")
$edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$d = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$url = "file:///$($d -replace '\\','/')/block-components-lab.html#$hash"
Remove-Item "$d\dom.txt", "$d\err.txt" -ErrorAction SilentlyContinue
$p = Start-Process -FilePath $edge -ArgumentList @('--headless=new', '--disable-gpu', '--window-size=1400,1500', '--virtual-time-budget=15000', '--enable-logging=stderr', '--v=0', "--user-data-dir=$d\edgeprof11", '--dump-dom', $url) -PassThru -WindowStyle Hidden -RedirectStandardOutput "$d\dom.txt" -RedirectStandardError "$d\err.txt"
if (-not $p.WaitForExit(1500000)) {
  Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" | Where-Object { $_.CommandLine -like '*edgeprof11*' } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
  "TIMEOUT"; exit
}
Get-Content "$d\err.txt" | Where-Object { $_ -match 'CONSOLE|Uncaught' } | Select-Object -First 15
$out = Get-Content "$d\dom.txt" -Raw
$m = [regex]::Match($out, 'data-json="([^"]*)"')
if ($m.Success) {
  $j = [System.Net.WebUtility]::HtmlDecode($m.Groups[1].Value); $j | Set-Content "$d\selftest.json" -Encoding utf8
  $o = $j | ConvertFrom-Json; "ok={0} behaviour={1} bad={2}" -f $o.ok, $o.behaviour, $o.bad.Count
  $o.bad | ForEach-Object { "{0} | {1} | {2} = {3} | {4}" -f $_.tab, $_.label, $_.key, $_.v, $_.result } | Select-Object -First 80
} elseif ($hash -eq "selftest") { "no json; length " + $out.Length }
