# Render one WAV per cue line with the Windows built-in voices (System.Speech, fully local).
# Usage:  powershell -File narrate.ps1 -Script ..\audio\guide_paco_en.txt -OutDir <empty dir> -Culture en-US [-Rate -2]
# The voice is the first installed voice whose culture matches -Culture (pt-PT wins over pt-BR for 'pt-PT');
# if none matches, -Fallback (a voice name) is used and the run says so. Then run build_narration.py on the OutDir.
param(
    [Parameter(Mandatory = $true)][string]$Script,
    [Parameter(Mandatory = $true)][string]$OutDir,
    [string]$Culture = 'en-US',
    [string]$Fallback = 'Microsoft Zira Desktop',
    [int]$Rate = -2
)
Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$voices = $synth.GetInstalledVoices() | Where-Object { $_.Enabled } | ForEach-Object { $_.VoiceInfo }
$pick = $voices | Where-Object { $_.Culture.Name -eq $Culture } | Select-Object -First 1
$standIn = $false
if (-not $pick) {
    $pick = $voices | Where-Object { $_.Name -eq $Fallback } | Select-Object -First 1
    $standIn = $true
}
$synth.SelectVoice($pick.Name)
$synth.Rate = $Rate
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$lines = Get-Content -Encoding UTF8 -Path $Script | Where-Object { $_.Trim() -ne '' }
$i = 0
foreach ($line in $lines) {
    $i++
    $synth.SetOutputToWaveFile((Join-Path $OutDir ('cue_{0:D3}.wav' -f $i)))
    $synth.Speak($line)
    $synth.SetOutputToNull()
}
$synth.Dispose()
$info = [pscustomobject]@{ voice = $pick.Name; voice_culture = $pick.Culture.Name; wanted_culture = $Culture; stand_in = $standIn; rate = $Rate; cues = $i }
$info | ConvertTo-Json | Set-Content -Encoding UTF8 -Path (Join-Path $OutDir 'voice.json')
$info | Format-List
