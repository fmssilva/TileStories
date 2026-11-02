"""Lab v8: build6 + the merged options of the Unity audit (v8*.js, v8.css), the words on their new owners."""
import io, json, os
D = os.path.dirname(os.path.abspath(__file__))
def rd(n): return io.open(os.path.join(D, n), encoding='utf-8').read()

V8 = ['v8a.js', 'v8b.js', 'v8c.js', 'v8d.js', 'v8e.js', 'v8f.js']

# words that moved to another part, or left with their part
REOWN = {'peek_next_label': ('retired', 'the Next card peek left: a swipe up at Peek raises the card'),
         'show_on_wall_nearby': ('retired', "Show On Wall's neighbours left: Related (Nearest) and the Wall Locator show them"),
         'gallery_count': ('s5viewer', ''), 'zoom_hint': ('s5viewer', ''),
         'video_full_screen': ('s5player', ''), 'video_chapters': ('s5player', ''), 'video_unavailable': ('s5player', ''),
         'hero_listen': ('retired', 'the hero keeps one action, the guide button; the Audio Guide block plays the audio'),
         'placed_model_remove': ('s5chip', ''), 'placed_model_other': ('s5chip', ''), 'nav_more': ('s5bar', '')}
NEWSTR = [
    ['mode_system', 's5settings', "Appearance and Motion: follow the phone's own setting.", 'Follow the phone', 'Seguir o telemóvel', '', 'Follow the phone'],
    ['settings_motion', 's5settings', 'The title of the Motion group.', 'Motion', 'Movimento', '', 'Motion group title'],
    ['motion_reduced', 's5settings', 'Motion: everything whole and still.', 'Reduced', 'Reduzido', '', 'Reduced'],
    ['settings_text', 's5settings', 'The title of the Text size group.', 'Text size', 'Tamanho do texto', '', 'Text size group title'],
    ['text_default', 's5settings', 'Text size: as the phone sets it.', 'Default', 'Normal', '', 'Default size'],
    ['text_large', 's5settings', 'Text size: one step larger.', 'Large', 'Grande', '', 'Large'],
    ['text_larger', 's5settings', 'Text size: two steps larger.', 'Larger', 'Maior', '', 'Larger'],
    ['settings_feedback', 's5settings', 'The title of the Haptics and sounds group.', 'Haptics and sounds', 'Vibração e sons', '', 'Haptics group title'],
    ['settings_on', 's5settings', 'A switch: on.', 'On', 'Ligado', '', 'On'],
    ['settings_off', 's5settings', 'A switch: off.', 'Off', 'Desligado', '', 'Off'],
    ['viewer_position', 's5viewer', 'The counter in the picture viewer ({0} = this picture, {1} = how many).', '{0} of {1}', '{0} de {1}', '', 'Counter'],
    ['player_captions', 's5player', "The player's captions switch.", 'Captions', 'Legendas', '', 'Captions switch'],
    ['story_resume', 'bd:Story', 'On a story the visitor left half read: the button that opens it where they stopped.', 'Continue reading', 'Continuar a ler', '', 'Resume button'],
    ['quiz_retry', 'bd:Knowledge Check', 'After the last question, with Retry on.', 'Try again', 'Tentar de novo', '', 'Retry button'],
    ['quiz_score', 'bd:Knowledge Check', 'At the end, with Score = At the end ({0} = right answers, {1} = questions).', '{0} of {1} right', '{0} de {1} certas', '', 'Score line'],
    ['model_in_room', 'bd:3D Model', "On the 3D Model's page: opens Place In AR with the same model.", 'See it in the room', 'Ver na sala', '', 'See it in the room'],
    ['transcript', 's5player', 'The button under a video or audio that shows the words of its captions.', 'Transcript', 'Transcrição', '', 'Transcript button'],
    ['sheet_expand', 's1a', 'The name of the grabber at the top of the card while a tap raises the card one stop (read by screen readers).', 'Expand card', 'Expandir cartão', '', 'Grabber, raise'],
    ['sheet_collapse', 's1a', 'The name of the grabber when the card is full and a tap lowers it to half (read by screen readers).', 'Collapse card', 'Recolher cartão', '', 'Grabber, lower'],
    ['card_starter_custom', 'retired', '', 'Custom', 'Personalizado', '', ''],
]

def fix_real(src):
    rows = json.loads(src)
    for r in rows:
        if r[0] in REOWN:
            o, why = REOWN[r[0]]
            r[1] = o
            if why:
                r[5] = why
    return json.dumps(rows, ensure_ascii=False)

PBAR_A = '["pbar_label","s0bar","The bar\'s tag, before its buttons.","PREVIEW","PRÉ-VISUALIZAR"'
PBAR_B = '["pbar_label","s0bar","The bar\'s tag, before its buttons: it says this is an Editor tool.","EDITOR","EDITOR"'
src = rd('build6.py')
a = "v7 = rd('v7.js').replace('__REALSTR__', rd('realstrings.json'))"
assert a in src
src = src.replace(a, "v7 = rd('v7.js').replace('__REALSTR__', fix_real(rd('realstrings.json')))\n"
                     "v7 = v7.replace('var LABSTR=[', 'var LABSTR=[' + ''.join(json.dumps(r, ensure_ascii=False) + ',' for r in NEWSTR if r[1] != 'retired'), 1)\n"
                     "v7 = v7.replace('var COMP={', 'var COMP={s5viewer:[\"Card Controls\",\"Picture viewer\"],s5player:[\"Card Controls\",\"Video player\"],s5chip:[\"Card Controls\",\"Placed model chip\"],', 1)\n"
                     "v7 = v7.replace(PBAR_A, PBAR_B, 1)\n"
                     "v8 = '\\n'.join(rd(f) for f in V8 if os.path.exists(os.path.join(D, f)))")
b = "+ v7 + '\\nif(location.hash===\"#selftest\")"
assert b in src, 'hook'
src = src.replace(b, "+ v7 + '\\n' + v8 + '\\nif(location.hash===\"#selftest\")", 1)
c = "rd('v7.css')"
assert c in src
src = src.replace(c, "rd('v7.css') + rd('v8.css') + rd('v8b.css') + rd('v8c.css') + rd('v8d.css') + rd('v8e.css') + rd('v8f.css')", 1)
src = src.replace("'Editor Lab · v7'", "'Editor Lab · v8'")
exec(compile(src, 'build6+8', 'exec'), {'__file__': os.path.join(D, 'build6.py'), '__name__': '__main__', 'fix_real': fix_real, 'NEWSTR': NEWSTR, 'V8': V8, 'json': json, 'PBAR_A': PBAR_A, 'PBAR_B': PBAR_B})
