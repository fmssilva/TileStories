$d = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$base = "file:///" + ($d.Replace('\', '/')) + "/block-components-lab.html#dcshot="
$kinds = @("Quick Facts", "Status", "Actions", "Timeline", "Compare Points", "Dialogue", "Hotspot Image", "Video", "Knowledge Check", "Poll", "Collect", "Feedback", "Place In AR", "Wall Locator", "Today Map", "Related", "Sources", "Contents")
New-Item -ItemType Directory -Force "$d\caps8b" | Out-Null
$i = 0
foreach ($k in $kinds) {
  $i++
  $url = $base + [uri]::EscapeDataString($k)
  $p = Start-Process -FilePath $edge -ArgumentList @('--headless=new', '--disable-gpu', '--window-size=1400,1250', '--virtual-time-budget=3000', "--user-data-dir=$d\edgeprof13", "--screenshot=$d\caps8b\k$i.png", $url) -PassThru -WindowStyle Hidden
  $p.WaitForExit(90000) | Out-Null
}
Get-ChildItem "$d\caps8b\*.png" | Measure-Object | Select-Object Count
