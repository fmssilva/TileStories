"""Local neural TTS (Piper, pt_PT 'tugao' voice, MIT / CC voice) -> one WAV per cue line, same layout as narrate.ps1.
Usage: python -X utf8 narrate_piper.py <script.txt> <out_dir> <model.onnx> [length_scale]
Needs `pip install piper-tts` (here installed with --target in a scratch folder: put it on PYTHONPATH) and the voice files from
https://huggingface.co/rhasspy/piper-voices/tree/main/pt/pt_PT/tug%C3%A3o/medium . Then run build_narration.py on the out_dir."""
import os, sys, wave
from piper import PiperVoice
from piper.config import SynthesisConfig

script, out_dir, model = sys.argv[1:4]
scale = float(sys.argv[4]) if len(sys.argv) > 4 else 1.15   # >1 = slower, a guide's pace
os.makedirs(out_dir, exist_ok=True)
voice = PiperVoice.load(model)
lines = [l.strip() for l in open(script, encoding="utf-8") if l.strip()]
for i, text in enumerate(lines, 1):
    with wave.open(os.path.join(out_dir, "cue_%03d.wav" % i), "wb") as w:
        voice.synthesize_wav(text, w, syn_config=SynthesisConfig(length_scale=scale))
print(len(lines), "cues ->", out_dir)
