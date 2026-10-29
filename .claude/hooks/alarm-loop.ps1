# The sound itself: replays an alarm every few seconds, starting quiet and getting
# louder each time until it reaches full volume. Runs until alert.ps1 kills it, or
# until Space is pressed: while ringing, a low-level keyboard hook swallows the first
# Space (so it never reaches the question dialog) and silences the alarm.
# Volume is relative to the Windows master volume (1.0 = whatever the PC is set to).
param(
    [string]$Sound = "$env:WINDIR\Media\Alarm01.wav",
    [double]$StartVolume = 0.05,
    [int]$RampSeconds = 60,          # time from StartVolume to full volume
    [int]$IntervalSeconds = 4
)

Add-Type -AssemblyName PresentationCore
Add-Type @"
using System;
using System.Runtime.InteropServices;
using System.Threading;

public static class SpaceSilencer {
    public static volatile bool Pressed;
    delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    static LowLevelKeyboardProc proc = Callback;   // kept alive so the GC never collects it
    static IntPtr hook;

    [StructLayout(LayoutKind.Sequential)]
    struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public int x; public int y; }

    [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc fn, IntPtr hMod, uint threadId);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] static extern int GetMessage(out MSG msg, IntPtr hWnd, uint min, uint max);
    [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string name);

    const int WH_KEYBOARD_LL = 13, WM_KEYDOWN = 0x100, WM_KEYUP = 0x101, VK_SPACE = 0x20;

    public static void Start() {
        var t = new Thread(() => {
            hook = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(null), 0);
            MSG m;
            while (GetMessage(out m, IntPtr.Zero, 0, 0) > 0) { }
        });
        t.IsBackground = true;
        t.Start();
    }

    static IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam) {
        if (nCode >= 0 && Marshal.ReadInt32(lParam) == VK_SPACE) {
            int msg = wParam.ToInt32();
            if (msg == WM_KEYDOWN) { Pressed = true; return (IntPtr)1; }
            if (msg == WM_KEYUP && Pressed) return (IntPtr)1;
        }
        return CallNextHookEx(hook, nCode, wParam, lParam);
    }
}
"@

[SpaceSilencer]::Start()
$player = New-Object System.Windows.Media.MediaPlayer
$player.Open([Uri]$Sound)
$started = Get-Date

while (-not [SpaceSilencer]::Pressed) {
    $elapsed = ((Get-Date) - $started).TotalSeconds
    $player.Volume = [Math]::Min(1.0, $StartVolume + (1.0 - $StartVolume) * $elapsed / $RampSeconds)
    $player.Position = [TimeSpan]::Zero
    $player.Play()
    for ($i = 0; $i -lt $IntervalSeconds * 10 -and -not [SpaceSilencer]::Pressed; $i++) {
        Start-Sleep -Milliseconds 100
    }
}
$player.Stop()
Start-Sleep -Milliseconds 300   # let the swallowed Space's key-up arrive before the hook goes away
