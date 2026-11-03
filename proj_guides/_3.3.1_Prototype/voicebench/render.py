"""Renders the guide's lines (script.json, from export_script.js) with a voice provider into audio files plus each word's time,
and writes the manifest the Lab reads (voicesets.js). Keys come from the workspace's .secrets/keys.env (never committed).

  python render.py azure  --en en-GB-SoniaNeural --pt pt-PT-RaquelNeural
  python render.py azure  --en en-GB-RyanNeural  --pt pt-PT-DuarteNeural
  python render.py eleven --en <voice_id> --pt <voice_id> [--model eleven_multilingual_v2]
  python render.py edge   --en en-GB-SoniaNeural --pt pt-PT-RaquelNeural     (private A/B only: out-private/, never published)
  python render.py gemini --en Kore --pt Kore [--model gemini-3.8-flash-tts]  (word times estimated: Gemini gives none)
  python render.py ... --only g0,g1 --lang pt --aud explorer                  (a subset)

Word times: each word of the line (split on spaces, as the Lab splits it) gets [start, end] in seconds, from the provider's own
word or character timings (Azure WordBoundary text offsets, ElevenLabs character alignment, edge WordBoundary). The app reads the
same files: the narration is rendered once, at authoring time.
"""
import argparse, base64, io, json, os, re, sys, time, urllib.request

HERE = os.path.dirname(os.path.abspath(__file__))

KEYS = os.path.normpath(os.path.join(HERE, '..', '..', '..', '.secrets', 'keys.env'))

def env():
    """The environment, then the workspace's one key file (.secrets/keys.env), then a local .env if one is left here."""
    e = dict(os.environ)
    for p in (KEYS, os.path.join(HERE, '.env')):
        if os.path.exists(p):
            for line in io.open(p, encoding='utf-8'):
                line = line.strip()
                if line and not line.startswith('#') and '=' in line:
                    k, v = line.split('=', 1)
                    if v.strip(): e.setdefault(k.strip(), v.strip())
    return e

def fnv(s):
    """The file key: FNV-1a 32-bit of "lang|text", the same hash the Lab computes, so a line is found by what it says."""
    h = 0x811c9dc5
    for ch in s.encode('utf-8'):
        h = ((h ^ ch) * 0x01000193) & 0xffffffff
    return '%08x' % h

def tokens(text):
    """The Lab's words: runs of non-space characters, with their character spans."""
    return [(m.start(), m.end()) for m in re.finditer(r'\S+', text)]

def words_from_spans(text, spans):
    """spans: [(char_start, char_end, t_start, t_end)] from the provider. Each Lab word gets the time of the spans inside it."""
    out = []
    for a, b in tokens(text):
        inside = [s for s in spans if s[0] < b and s[1] > a]
        if inside:
            out.append([round(min(s[2] for s in inside), 3), round(max(s[3] for s in inside), 3)])
        else:
            out.append(None)
    # a word with no timing (punctuation alone): between its neighbours
    for i, w in enumerate(out):
        if w is None:
            prev = next((out[j] for j in range(i - 1, -1, -1) if out[j]), [0, 0])
            nxt = next((out[j] for j in range(i + 1, len(out)) if out[j]), [prev[1], prev[1]])
            out[i] = [prev[1], max(prev[1], nxt[0])]
    return out

# ----- providers -----
def render_azure(text, lang, voice, keys):
    import azure.cognitiveservices.speech as sdk
    cfg = sdk.SpeechConfig(subscription=keys['AZURE_SPEECH_KEY'], region=keys.get('AZURE_SPEECH_REGION', 'westeurope'))
    cfg.speech_synthesis_voice_name = voice
    cfg.set_speech_synthesis_output_format(sdk.SpeechSynthesisOutputFormat.Audio24Khz48KBitRateMonoMp3)
    cfg.set_property(sdk.PropertyId.SpeechServiceResponse_RequestWordBoundary, 'true')
    syn = sdk.SpeechSynthesizer(speech_config=cfg, audio_config=None)
    spans = []
    def wb(evt):
        if str(evt.boundary_type).endswith('Word'):
            t0 = evt.audio_offset / 1e7
            spans.append((evt.text_offset, evt.text_offset + evt.word_length, t0, t0 + evt.duration.total_seconds()))
    syn.synthesis_word_boundary.connect(wb)
    r = syn.speak_text_async(text).get()
    if r.reason != sdk.ResultReason.SynthesizingAudioCompleted:
        raise RuntimeError('azure: %s %s' % (r.reason, getattr(r, 'cancellation_details', '')))
    return r.audio_data, words_from_spans(text, spans), 'mp3'

def render_eleven(text, lang, voice, keys, model):
    body = json.dumps({'text': text, 'model_id': model, 'language_code': 'pt' if lang == 'pt' else 'en',
                       'voice_settings': {'stability': 0.45, 'similarity_boost': 0.8, 'style': 0.25}}).encode()
    req = urllib.request.Request('https://api.elevenlabs.io/v1/text-to-speech/%s/with-timestamps?output_format=mp3_44100_64' % voice,
                                 data=body, headers={'xi-api-key': keys['ELEVENLABS_API_KEY'], 'Content-Type': 'application/json'})
    j = json.loads(urllib.request.urlopen(req, timeout=120).read())
    al = j.get('alignment') or j.get('normalized_alignment') or {}
    chars, st, en = al.get('characters', []), al.get('character_start_times_seconds', []), al.get('character_end_times_seconds', [])
    # the alignment is per character of the text sent: rebuild their offsets
    spans, pos = [], 0
    for c, a, b in zip(chars, st, en):
        k = text.find(c, pos) if c.strip() else -1
        if k >= 0:
            spans.append((k, k + 1, a, b)); pos = k + 1
    return base64.b64decode(j['audio_base64']), words_from_spans(text, spans), 'mp3'

# a direction in square brackets is followed, not read aloud (measured: "...: " is spoken, "[...] " is not)
GEMINI_STYLE = {'pt': '[Português europeu de Lisboa, nunca do Brasil; tom caloroso e sem pressa de guia de museu] ',
                'en': '[Warm, unhurried British English museum guide] '}

def estimate_words(text, pcm, rate):
    """No word times from the provider: spread the words over the voiced part of the audio, by their characters (a pause after
    punctuation). Marked 'estimated' in the set."""
    import array
    a = array.array('h'); a.frombytes(pcm)
    th = 600; n = len(a)
    first = next((i for i in range(0, n, 120) if abs(a[i]) > th), 0)
    last = next((i for i in range(n - 1, 0, -120) if abs(a[i]) > th), n - 1)
    t0, t1 = first / rate, last / rate
    toks = tokens(text)
    weights = [(b - a2) + (3 if text[b - 1] in '.,;:!?' else 1) for a2, b in toks]
    tot = float(sum(weights)) or 1.0
    out, t = [], t0
    for w in weights:
        d = (t1 - t0) * w / tot
        out.append([round(t, 3), round(t + d * .92, 3)]); t += d
    return out

def render_gemini(text, lang, voice, keys, model):
    body = json.dumps({'contents': [{'parts': [{'text': GEMINI_STYLE.get(lang, '') + text}]}],
                       'generationConfig': {'responseModalities': ['AUDIO'], 'speechConfig': {'voiceConfig': {'prebuiltVoiceConfig': {'voiceName': voice}}}}}).encode()
    req = urllib.request.Request('https://generativelanguage.googleapis.com/v1beta/models/%s:generateContent' % model, data=body,
                                 headers={'x-goog-api-key': keys['GEMINI_API_KEY'], 'Content-Type': 'application/json'})
    j = json.loads(urllib.request.urlopen(req, timeout=180).read())
    part = j['candidates'][0]['content']['parts'][0]['inlineData']
    pcm = base64.b64decode(part['data'])
    rate = int(re.search(r'rate=(\d+)', part.get('mimeType', 'rate=24000')).group(1)) if 'rate=' in part.get('mimeType', '') else 24000
    import lameenc
    enc = lameenc.Encoder(); enc.set_bit_rate(48); enc.set_in_sample_rate(rate); enc.set_channels(1); enc.set_quality(2)
    mp3 = enc.encode(pcm) + enc.flush()
    return bytes(mp3), estimate_words(text, pcm, rate), 'mp3'

def render_edge(text, lang, voice, keys):
    import asyncio, edge_tts
    async def go():
        c = edge_tts.Communicate(text, voice, boundary='WordBoundary')
        audio, spans, pos = b'', [], 0
        async for ch in c.stream():
            if ch['type'] == 'audio':
                audio += ch['data']
            elif ch['type'] == 'WordBoundary':
                w = ch['text']; k = text.find(w, pos)
                if k < 0: k = pos
                t0 = ch['offset'] / 1e7
                spans.append((k, k + len(w), t0, t0 + ch['duration'] / 1e7)); pos = k + len(w)
        return audio, spans
    audio, spans = asyncio.run(go())
    return audio, words_from_spans(text, spans), 'mp3'

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('provider', choices=['azure', 'eleven', 'edge', 'gemini'])
    ap.add_argument('--en', required=True); ap.add_argument('--pt', required=True)
    ap.add_argument('--model', default='')
    ap.add_argument('--pause', type=float, default=0.0)
    ap.add_argument('--label', default='')
    ap.add_argument('--only', default=''); ap.add_argument('--lang', default=''); ap.add_argument('--aud', default='')
    a = ap.parse_args()
    keys = env()
    need = {'azure': 'AZURE_SPEECH_KEY', 'eleven': 'ELEVENLABS_API_KEY', 'gemini': 'GEMINI_API_KEY'}.get(a.provider)
    model = a.model or {'eleven': 'eleven_multilingual_v2', 'gemini': 'gemini-3.8-flash-tts'}.get(a.provider, '')
    pause = a.pause or (7.0 if a.provider == 'gemini' else 0.0)
    if need and not keys.get(need):
        sys.exit('Missing %s in .secrets/keys.env (copy keys.env.example to keys.env and fill it in).' % need)
    lines = json.load(io.open(os.path.join(HERE, 'script.json'), encoding='utf-8'))
    only = set(x for x in a.only.split(',') if x)
    lines = [l for l in lines if (not only or l['key'].split('_')[0] in only or l['key'] in only) and (not a.lang or l['lang'] == a.lang)
             and (not a.aud or l['key'].endswith('_' + a.aud) or not re.search(r'_(family|explorer|expert)$', l['key']))]
    set_id = '%s-%s-%s' % (a.provider, a.en.split('-')[-1].replace('Neural', '').lower(), a.pt.split('-')[-1].replace('Neural', '').lower())
    base = os.path.join(HERE, 'out-private' if a.provider == 'edge' else 'out', set_id)
    os.makedirs(base, exist_ok=True)
    man_path = os.path.join(base, 'set.json')
    man = json.load(io.open(man_path, encoding='utf-8')) if os.path.exists(man_path) else {'id': set_id, 'provider': a.provider, 'voices': {'en': a.en, 'pt': a.pt}, 'files': {}}
    man['label'] = a.label or man.get('label') or '%s · %s / %s' % ({'azure': 'Azure Neural', 'eleven': 'ElevenLabs', 'edge': 'Edge (private)', 'gemini': 'Gemini TTS'}[a.provider], a.en, a.pt)
    if a.provider == 'gemini': man['timings'] = 'estimated'
    chars, done = 0, set()
    for l in lines:
        voice = a.en if l['lang'] == 'en' else a.pt
        fk = fnv(l['lang'] + '|' + l['text'])
        if fk in done: continue
        done.add(fk)
        old = man['files'].get(fk)
        if old and old.get('text') == l['text'] and os.path.exists(os.path.join(base, old['src'])):
            continue
        for tries in range(3):
            try:
                if a.provider == 'azure': audio, words, ext = render_azure(l['text'], l['lang'], voice, keys)
                elif a.provider == 'eleven': audio, words, ext = render_eleven(l['text'], l['lang'], voice, keys, model)
                elif a.provider == 'gemini': audio, words, ext = render_gemini(l['text'], l['lang'], voice, keys, model)
                else: audio, words, ext = render_edge(l['text'], l['lang'], voice, keys)
                break
            except Exception as e:
                if tries == 2: raise
                print('retry', fk, e); time.sleep(20 if '429' in str(e) else 2 + tries * 3)
        src = fk + '.' + ext
        open(os.path.join(base, src), 'wb').write(audio)
        man['files'][fk] = {'src': src, 'text': l['text'], 'lang': l['lang'], 'words': words}
        chars += len(l['text'])
        print('ok', fk, len(audio), 'bytes', len(words), 'words')
        if pause: time.sleep(pause)
    io.open(man_path, 'w', encoding='utf-8').write(json.dumps(man, ensure_ascii=False))
    # the manifest the Lab reads: every set in out/ (published) or out-private/ (local only)
    for folder in ('out', 'out-private'):
        root = os.path.join(HERE, folder)
        if not os.path.isdir(root): continue
        sets = []
        for d in sorted(os.listdir(root)):
            p = os.path.join(root, d, 'set.json')
            if os.path.exists(p):
                s = json.load(io.open(p, encoding='utf-8')); s['base'] = '%s/%s/' % (folder, d); sets.append(s)
        # each set's files are found next to this script, wherever the page loads it from
        io.open(os.path.join(root, 'voicesets.js'), 'w', encoding='utf-8').write(
            '(function(){var u=(document.currentScript&&document.currentScript.src||"").replace(/voicesets\\.js.*$/,""),sets=' + json.dumps(sets, ensure_ascii=False) +
            ';sets.forEach(function(s){s.url=u+s.id+"/";s.private=' + ('true' if folder == 'out-private' else 'false') + '});window.VOICESETS=(window.VOICESETS||[]).concat(sets)})();\n')
    print('rendered', chars, 'characters into', base)

if __name__ == '__main__':
    main()
