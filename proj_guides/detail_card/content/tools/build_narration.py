"""Join the per-cue WAVs of narrate.ps1 into one narration, write its WebVTT from the MEASURED cue lengths, encode an mp3.
Usage: python -X utf8 build_narration.py <cue_dir> <script.txt> <out_base> <ffmpeg.exe>
Writes <out_base>.mp3 and <out_base>.vtt (and <out_base>.wav when the 4th argument is 'keep-wav' instead of ffmpeg)."""
import os, subprocess, sys, wave

cue_dir, script, out_base, ffmpeg = sys.argv[1:5]
lines = [l.strip() for l in open(script, encoding="utf-8") if l.strip()]
LEAD = 0.30          # silence before the first word
GAP_END = 0.55       # after a sentence end
GAP_CONT = 0.12      # after a line that continues in the next one


def stamp(t):
    ms = int(round(t * 1000))
    return "%02d:%02d:%02d.%03d" % (ms // 3600000, ms // 60000 % 60, ms // 1000 % 60, ms % 1000)


frames, vtt, params, t = [], ["WEBVTT", ""], None, LEAD
for i, text in enumerate(lines, 1):
    with wave.open(os.path.join(cue_dir, "cue_%03d.wav" % i)) as w:
        if params is None:
            params = w.getparams()
        assert (w.getnchannels(), w.getsampwidth(), w.getframerate()) == params[:3], "voices differ in format"
        data = w.readframes(w.getnframes())
        dur = w.getnframes() / w.getframerate()
    rate, width = params.framerate, params.sampwidth * params.nchannels
    if i == 1:
        frames.append(b"\x00" * int(LEAD * rate) * width)
    frames.append(data)
    gap = GAP_END if text[-1] in ".!?:" else GAP_CONT
    frames.append(b"\x00" * int(gap * rate) * width)
    vtt += [str(i), "%s --> %s" % (stamp(t), stamp(t + dur)), text, ""]
    t += dur + gap

wav_path = out_base + ".wav"
with wave.open(wav_path, "wb") as o:
    o.setparams(params)
    o.writeframes(b"".join(frames))
open(out_base + ".vtt", "w", encoding="utf-8", newline="\n").write("\n".join(vtt))
if ffmpeg != "keep-wav":
    subprocess.run([ffmpeg, "-v", "error", "-y", "-i", wav_path, "-ac", "1", "-ar", "22050", "-b:a", "64k", out_base + ".mp3"], check=True)
    os.remove(wav_path)
print("%s: %d cues, %.1f s" % (os.path.basename(out_base), len(lines), t))
