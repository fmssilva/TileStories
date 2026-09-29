"""Generate the LivingRoom card's PLACEHOLDER audio fixtures (Python standard library only, ASCII source).

Writes into Resources/LivingRoom/CardMedia/audio/:
  lamp_tone.wav  -- 20 s, mono, 16-bit, 11.025 kHz: four 5-second sections, each a soft two-note chime on a different pitch,
                    so a seek is audible and the clip length is exactly known. No voice, no music: a placeholder tone.
  lamp_tone.vtt  -- four cues, one per section (placeholder wording: it describes the section, it is not a transcript)
  castelo_s_jorge_guide_pt.vtt -- PLACEHOLDER captions for the real Portuguese narration (200.4 s): twenty cues of 10 s of
                    "test caption n" wording. The real clip was not transcribed: these lines only prove the caption machinery.
Run from the LivingRoom folder:  python generate_card_audio.py
Placeholders are released CC0 (the tone and the captions were made by this script).
"""
import math
import os
import struct
import wave

OUT = os.path.join("Resources", "LivingRoom", "CardMedia", "audio")
RATE = 11025
SECTION_SECONDS = 5
PITCHES = (262.0, 330.0, 392.0, 523.0)


def stamp(seconds):
    whole = int(seconds)
    ms = int(round((seconds - whole) * 1000))
    return "%02d:%02d:%02d.%03d" % (whole // 3600, (whole // 60) % 60, whole % 60, ms)


def write_tone(path):
    frames = bytearray()
    for section, pitch in enumerate(PITCHES):
        for i in range(SECTION_SECONDS * RATE):
            t = i / RATE
            # - a chime: the pitch and its fifth, decaying every second so the section has a heartbeat
            beat = math.exp(-3.0 * (t % 1.0))
            value = 0.22 * beat * (math.sin(2 * math.pi * pitch * t) + 0.5 * math.sin(2 * math.pi * pitch * 1.5 * t))
            frames += struct.pack("<h", int(max(-1.0, min(1.0, value)) * 32767))
    with wave.open(path, "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(RATE)
        wav.writeframes(bytes(frames))


def write_vtt(path, cues):
    lines = ["WEBVTT", ""]
    for n, (start, end, text) in enumerate(cues, 1):
        lines += [str(n), stamp(start) + " --> " + stamp(end), text, ""]
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


def main():
    os.makedirs(OUT, exist_ok=True)
    write_tone(os.path.join(OUT, "lamp_tone.wav"))
    write_vtt(os.path.join(OUT, "lamp_tone.vtt"), [
        (i * SECTION_SECONDS, (i + 1) * SECTION_SECONDS, "Placeholder caption: tone section %d of %d." % (i + 1, len(PITCHES)))
        for i in range(len(PITCHES))
    ])
    cues = []
    total = 20
    for i in range(total):
        # - PT wording, proper letters (the file is UTF-8 data; the source stays ASCII through escapes)
        cues.append((i * 10, (i + 1) * 10, "Legenda de teste %d de %d: texto provisório, não é a transcrição." % (i + 1, total)))
    write_vtt(os.path.join(OUT, "castelo_s_jorge_guide_pt.vtt"), cues)
    print("written to", OUT)


if __name__ == "__main__":
    main()
