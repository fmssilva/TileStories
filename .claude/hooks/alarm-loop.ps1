# The sound itself: replays an alarm every few seconds, starting quiet and getting
# louder each time until it reaches full volume. Runs until alert.ps1 kills it.
# Volume is relative to the Windows master volume (1.0 = whatever the PC is set to).
param(
    [string]$Sound = "$env:WINDIR\Media\Alarm01.wav",
    [double]$StartVolume = 0.1,
    [double]$Step = 0.1,
    [int]$IntervalSeconds = 4
)

Add-Type -AssemblyName PresentationCore
$player = New-Object System.Windows.Media.MediaPlayer
$player.Open([Uri]$Sound)
$volume = $StartVolume

while ($true) {
    $player.Volume = $volume
    $player.Position = [TimeSpan]::Zero
    $player.Play()
    Start-Sleep -Seconds $IntervalSeconds
    $volume = [Math]::Min(1.0, $volume + $Step)
}
