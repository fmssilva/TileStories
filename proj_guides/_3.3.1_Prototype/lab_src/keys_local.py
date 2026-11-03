"""Gives the local Lab (opened from a file) the keys its live agent needs, from the workspace's one key file.

  python keys_local.py          writes keys.local.js beside the Lab pages (lab_src/ and the record copy's folder); git ignores it
  python keys_local.py --check  asks each service whether its key works, and prints ok or the HTTP error, never the key

The published Lab never loads a key: there the agent answers through Claude (the artifact's sample capability).
See proj_guides/__Services_And_Keys.md.
"""
import io, json, os, sys, urllib.error, urllib.request

HERE = os.path.dirname(os.path.abspath(__file__))
KEYS = os.path.normpath(os.path.join(HERE, '..', '..', '..', '.secrets', 'keys.env'))
LAB_KEYS = {'gemini': 'GEMINI_API_KEY', 'anthropic': 'ANTHROPIC_API_KEY'}

def keys():
    k = {}
    if os.path.exists(KEYS):
        for line in io.open(KEYS, encoding='utf-8'):
            line = line.strip()
            if line and not line.startswith('#') and '=' in line:
                a, b = line.split('=', 1)
                if b.strip(): k[a.strip()] = b.strip()
    return k

def write(k):
    js = '/* Written by keys_local.py from .secrets/keys.env. Never commit or publish this file. */\nwindow.LABKEYS=%s;\n' % json.dumps(
        {n: k[v] for n, v in LAB_KEYS.items() if v in k})
    for d in (HERE, os.path.dirname(HERE)):
        io.open(os.path.join(d, 'keys.local.js'), 'w', encoding='utf-8', newline='\n').write(js)
    print('keys.local.js:', ', '.join(n for n, v in LAB_KEYS.items() if v in k) or 'no keys')

def ask(url, headers, data=None):
    try:
        urllib.request.urlopen(urllib.request.Request(url, data=data, headers=headers), timeout=30).read()
        return 'ok'
    except urllib.error.HTTPError as e:
        return 'HTTP %d' % e.code
    except Exception as e:
        return type(e).__name__

def check(k):
    tests = [
        ('Gemini', 'GEMINI_API_KEY', lambda v: ask('https://generativelanguage.googleapis.com/v1beta/models?pageSize=1', {'x-goog-api-key': v})),
        ('Anthropic', 'ANTHROPIC_API_KEY', lambda v: ask('https://api.anthropic.com/v1/models', {'x-api-key': v, 'anthropic-version': '2023-06-01'})),
        ('Azure Speech', 'AZURE_SPEECH_KEY', lambda v: ask('https://%s.api.cognitive.microsoft.com/sts/v1.0/issueToken' % k.get('AZURE_SPEECH_REGION', 'westeurope'),
                                                           {'Ocp-Apim-Subscription-Key': v, 'Content-Length': '0'}, b'')),
        ('ElevenLabs', 'ELEVENLABS_API_KEY', lambda v: ask('https://api.elevenlabs.io/v1/user', {'xi-api-key': v})),
        ('Meshy', 'MESHY_API_KEY', None),
        ('Tripo', 'TRIPO_API_KEY', None),
    ]
    for name, var, test in tests:
        print('%-13s %s' % (name, 'not set' if var not in k else (test(k[var]) if test else 'set (not checked)')))

if __name__ == '__main__':
    k = keys()
    if not os.path.exists(KEYS): sys.exit('No %s: copy keys.env.example to keys.env and fill it in.' % KEYS)
    check(k) if '--check' in sys.argv else write(k)
