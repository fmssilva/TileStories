"""Lab v8: build6 + the merged options of the Unity audit (v8*.js, v8.css), the words on their new owners."""
import io, json, os
D = os.path.dirname(os.path.abspath(__file__))
def rd(n): return io.open(os.path.join(D, n), encoding='utf-8').read()

V8 = ['v8a.js', 'v8b.js', 'v8c.js', 'v8d.js', 'v8e.js', 'v8f.js', 'v8g.js', 'v8h.js', 'v8i.js', 'v8j.js', 'v8k.js', 'v8l.js', 'v8m.js', 'v8n.js', 'v8o.js']

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
    ['hint_navbar', 's5hints', 'The tip over the Navigation Bar the first time it shows (a hint).', 'Jump to any part', 'Salte para qualquer parte', '', 'Navigation Bar tip'],
    ['settings_how', 's5settings', "The settings panel's last button: it starts the short tour of how the card works.", 'How to use the card', 'Como usar o cartão', '', 'How to use button'],
    ['how_grab', 's5hints', 'How to use the card, step 1: the grabber.', 'Tap the bar at the top, or drag it, to open the card wider or smaller.', 'Toque na barra no topo, ou arraste-a, para abrir o cartão mais ou menos.', '', 'Step 1, grabber'],
    ['how_settings', 's5hints', 'Step 2: the settings mark.', 'The sliders hold your language, light or dark, motion and text size.', 'Os ajustes têm o idioma, claro ou escuro, movimento e tamanho do texto.', '', 'Step 2, settings'],
    ['how_bar', 's5hints', 'Step 3: the Navigation Bar.', 'The bar at the bottom jumps to each part of the card.', 'A barra em baixo salta para cada parte do cartão.', '', 'Step 3, bar'],
    ['how_locate', 's5hints', 'Step 4: the Locate Button.', 'The pin lowers the card and shows where this point is on the wall.', 'O pino baixa o cartão e mostra onde fica este ponto na parede.', '', 'Step 4, locate'],
    ['how_block', 's5hints', 'Step 5: a block and its page.', 'Tap a block to open its page; Back brings you here.', 'Toque num bloco para abrir a sua página; Voltar traz de volta aqui.', '', 'Step 5, block'],
    ['state_loading', 's5states', 'Under a loader while the card or a page loads.', 'Loading…', 'A carregar…', '', 'Loading'],
    ['state_slow', 's5states', 'Added under the loader after 4 s.', 'Still loading. The connection is slow.', 'Ainda a carregar. A ligação está lenta.', '', 'Slow line'],
    ['state_lost_title', 's5states', 'The camera cannot see the wall (or lost it): the title.', "Can't see the wall", 'Não vejo a parede', '', 'Wall not found, title'],
    ['state_lost_line', 's5states', 'Its line: what to do.', 'Point your phone at the tiles and hold still.', 'Aponte o telemóvel para os azulejos e fique quieto.', '', 'Wall not found, line'],
    ['state_empty_title', 's5states', 'A list with nothing to show (no results for the filters).', 'Nothing here yet', 'Ainda nada aqui', '', 'Nothing here, title'],
    ['state_empty_line', 's5states', 'Its line.', 'Try fewer filters.', 'Experimente menos filtros.', '', 'Nothing here, line'],
    ['state_empty_action', 's5states', 'Its button.', 'Clear filters', 'Limpar filtros', '', 'Nothing here, button'],
    ['state_offline_title', 's5states', 'No connection: the title.', "You're offline", 'Está sem ligação', '', 'Offline, title'],
    ['state_offline_line', 's5states', 'Its line.', "What you've opened still works.", 'O que já abriu continua a funcionar.', '', 'Offline, line'],
    ['state_retry', 's5states', 'The button that tries again (offline, a failed picture or page).', 'Try again', 'Tentar de novo', '', 'Try again'],
    ['state_done', 's5states', 'After an action that saved something (an answer, a vote, a collected item).', 'Saved', 'Guardado', '', 'Done'],
    ['guide_novoice', 's7cap', 'Over the captions when this device has no voice for the language (the words still light up).', 'No voice for this language on this device: captions only.', 'Sem voz para esta língua neste dispositivo: só legendas.', '', 'No voice'],
    ['dock_label', 's7dock', 'The guide panel, for a screen reader.', 'The guide', 'O guia', '', 'Name'],
    ['dock_stop_of', 's7dock', 'The step: {0} of {1}.', 'Stop {0} of {1}', 'Paragem {0} de {1}', '', 'Stop'],
    ['dock_ask', 's7dock', 'The talk button, for a screen reader.', 'Ask Azu', 'Perguntar ao Azu', '', 'Talk button'],
    ['dock_tap_talk', 's7dock', 'Under the talk button, at rest.', 'Tap to talk · hold to talk', 'Toque para falar · mantenha para falar', '', 'At rest'],
    ['dock_listening', 's7dock', 'Listening.', 'Listening…', 'A ouvir…', '', 'Listening'],
    ['dock_thinking', 's7dock', 'Thinking.', 'Thinking…', 'A pensar…', '', 'Thinking'],
    ['dock_tap_stop', 's7dock', 'While Azu speaks.', 'Tap to interrupt', 'Toque para interromper', '', 'Speaking'],
    ['dock_go_ahead', 's7dock', 'Just interrupted.', 'Go ahead…', 'Diga…', '', 'Interrupted'],
    ['dock_not_heard', 's7dock', 'Nothing heard.', "Didn't catch that. Hold the button, or type.", 'Não percebi. Mantenha o botão, ou escreva.', '', 'Not heard'],
    ['dock_hold_hint', 's7dock', 'With Hold only, after a tap.', 'Hold the button while you speak.', 'Mantenha o botão enquanto fala.', '', 'Hold'],
    ['dock_type', 's7dock', 'The keyboard button and the text field.', 'Type a question', 'Escreva uma pergunta', '', 'Type'],
    ['dock_send', 's7dock', 'Send.', 'Send', 'Enviar', '', 'Send'],
    ['dock_expand', 's7dock', 'The grabber, to raise the panel.', 'Show more', 'Mostrar mais', '', 'Expand'],
    ['dock_collapse', 's7dock', 'The grabber, to lower the panel.', 'Show less', 'Mostrar menos', '', 'Collapse'],
    ['dock_mic_framed', 's7dock', 'In the conversation when the microphone is blocked inside a page (a preview).', 'The microphone is not available in this preview. Type your question instead.', 'O microfone não está disponível nesta pré-visualização. Escreva a sua pergunta.', '', 'Mic blocked'],
    ['dock_mic_none', 's7dock', 'When the device has no speech recognition.', 'Speaking is not available on this device. Type your question instead.', 'Falar não está disponível neste dispositivo. Escreva a sua pergunta.', '', 'No mic'],
    ['guide_intro', 's7mascot', "Azu's introduction, before the first step. {0} = the point's name.", "Hi! I'm Azu, and I'll be your guide. Ask me anything, at any time: just tap Ask. Now, let's meet {0}!", 'Olá! Sou o Azu e vou ser o seu guia. Pergunte-me o que quiser, a qualquer momento: basta tocar em Perguntar. Agora, vamos conhecer {0}!', '', 'Introduction'],
    ['guide_moving_on', 's7resume', 'When the block being told left the screen: the guide tells the block now in view.', "OK, moving on. Let's look at this.", 'Muito bem, seguimos. Vejamos isto.', '', 'Moving on'],
    ['guide_nudge_flip_1', 's7coach', 'Asking again (1st time) to flip a card.', 'Go on, tap the card to flip it.', 'Vá, toque no cartão para o virar.', '', 'Flip, again'],
    ['guide_nudge_flip_2', 's7coach', 'Asking again (2nd time) to flip a card.', "Just one tap on the card, and you'll see what's on the other side.", 'Basta um toque no cartão e vê o que está do outro lado.', '', 'Flip, again (2)'],
    ['guide_nudge_drag_1', 's7coach', 'Asking again (1st time) to drag across a picture.', 'Drag the line across the picture.', 'Arraste a linha sobre a imagem.', '', 'Drag, again'],
    ['guide_nudge_drag_2', 's7coach', 'Asking again (2nd time) to drag.', 'Slide it all the way across, and see what changed.', 'Deslize até ao fim e veja o que mudou.', '', 'Drag, again (2)'],
    ['guide_nudge_answer_1', 's7coach', 'Asking again (1st time) to answer a quiz.', 'Take a guess: tap one of the answers.', 'Arrisque: toque numa das respostas.', '', 'Answer, again'],
    ['guide_nudge_answer_2', 's7coach', 'Asking again (2nd time) to answer.', 'No pressure, any answer will do.', 'Sem pressão, qualquer resposta serve.', '', 'Answer, again (2)'],
    ['guide_try_moveon', 's7coach', 'After the last ask, when the visitor has not tried.', "No problem. Let's keep going.", 'Não faz mal. Vamos continuar.', '', 'Moving on'],
    ['guide_after_flip', 's7coach', 'Once a card is flipped, before its back is read.', 'There it is.', 'Aí está.', '', 'After a flip'],
    ['guide_after_answer', 's7coach', 'Once a quiz is answered, when the step has no words of its own.', 'Nicely done.', 'Muito bem.', '', 'After an answer'],
    ['lens_label', 's8lens', 'Before the chips (also their name for a screen reader).', 'See it as', 'Ver como', '', 'Label'],
    ['lens_all', 's8lens', 'The first chip: no lens.', 'Everything', 'Tudo', '', 'Everything'],
    ['aud_title', 's8onb', 'The first card: the question.', "Who's exploring today?", 'Quem está a explorar hoje?', '', 'Question'],
    ['aud_family', 's8onb', 'The Family choice (also in Settings and the preview bar).', 'Family', 'Família', '', 'Family'],
    ['aud_family_line', 's8onb', 'What Family gives.', 'Short stories, games and a playful guide', 'Histórias curtas, jogos e um guia brincalhão', '', 'Family, line'],
    ['aud_explorer', 's8onb', 'The Explorer choice, the default.', 'Explorer', 'Explorador', '', 'Explorer'],
    ['aud_explorer_line', 's8onb', 'What Explorer gives.', 'The stories, told well', 'As histórias, bem contadas', '', 'Explorer, line'],
    ['aud_expert', 's8onb', 'The Expert choice.', 'Expert', 'Especialista', '', 'Expert'],
    ['aud_expert_line', 's8onb', 'What Expert gives.', 'Dates, sources and the details', 'Datas, fontes e os pormenores', '', 'Expert, line'],
    ['aud_skip', 's8onb', 'Skip: Explorer.', 'Skip', 'Saltar', '', 'Skip'],
    ['aud_note', 's8onb', 'Under the choices.', 'You can change this in Settings.', 'Pode mudar isto nas Definições.', '', 'Note'],
    ['settings_aud', 's8set', 'Settings: the audience row.', "Who's exploring", 'Quem explora', '', 'Audience'],
    ['settings_len', 's8set', 'Settings: the length row.', 'Length', 'Extensão', '', 'Length'],
    ['len_short', 's8set', 'Length: short.', 'Short', 'Curta', '', 'Short'],
    ['len_standard', 's8set', 'Length: standard.', 'Standard', 'Normal', '', 'Standard'],
    ['len_deep', 's8set', 'Length: deep.', 'Deep', 'Aprofundada', '', 'Deep'],
    ['pbar_tip_aud', 's0bar', 'The preview bar: Preview as.', 'Preview as: Family, Explorer or Expert', 'Pré-visualizar como: Família, Explorador ou Especialista', '', 'Preview as, tip'],
    ['mascot_name', 's7mascot', 'The mascot, for a screen reader.', 'Azu, the guide', 'Azu, o guia', '', 'Name'],
    ['mascot_hey', 's7mascot', 'After a poke.', 'Hey!', 'Ei!', '', 'Poke'],
    ['mascot_tickle', 's7mascot', 'After three quick taps.', 'Hehe, that tickles!', 'Hehe, faz cócegas!', '', 'Tickle'],
    ['mascot_oops', 's7mascot', 'After a long press knocks Azu over.', "Oops! I'm fine.", 'Ups! Estou bem.', '', 'Knocked over'],
    ['mascot_joke_1', 's7mascot', 'A joke, every third poke (Lively, not on a Solemn point).', "I've been on walls for centuries. Well, my great-grandmother has.", 'Estou em paredes há séculos. Bem, a minha bisavó está.', '', 'Joke 1'],
    ['mascot_joke_2', 's7mascot', 'A joke.', 'Careful! I chip easily.', 'Cuidado! Lasco-me com facilidade.', '', 'Joke 2'],
    ['mascot_joke_3', 's7mascot', 'A joke.', 'Blue is my favourite colour. Obviously.', 'O azul é a minha cor preferida. Claro.', '', 'Joke 3'],
    ['mascot_water', 's7mascot', 'The water break: Azu asks for a tap on the glass.', 'A sip of water? Tap the glass.', 'Um gole de água? Toque no copo.', '', 'Water break'],
    ['mascot_ahh', 's7mascot', 'After the sip.', 'Ahh! Where were we?', 'Ahh! Onde é que íamos?', '', 'After the sip'],
    ['mascot_hungry', 's7mascot', 'Before the end: Azu asks for the cookie.', 'A little hungry... a cookie? Tap it.', 'Uma fominha... uma bolacha? Toque nela.', '', 'Cookie'],
    ['mascot_thanks', 's7mascot', 'After the cookie.', 'Yum! Thank you!', 'Que bom! Obrigado!', '', 'After the cookie'],
    ['card_starter_custom', 'retired', '', 'Custom', 'Personalizado', '', ''],
]

DEMO_ROWS = {
    'test_tile_tag': ["test_tile_tag", "s0card", "The small tag over the tile's title and over the Demo Components page's demo card button.", "TEST · NOT IN THE APP", "TESTE · NÃO ESTÁ NA APP", "", "Tag"],
    'test_tile_title': ["test_tile_title", "s0card", "The tile's title: first on every marker's card (Scene and Play only), it opens the Demo Components page.", "Demo Components", "Componentes demo", "", "Tile title"],
    'test_tile_line': ["test_tile_line", "s0card", "The tile's second line.", "Motion, interaction, effects, elements, patterns.", "Movimento, interação, efeitos, elementos, padrões.", "", "Tile line"],
    'test_demo_title': ["test_demo_title", "s0card", "On the Demo Components page: the button at its top that opens the full demo card.", "Demo card", "Cartão demo", "", "Demo card button"],
    'test_demo_line': ["test_demo_line", "s0card", "That button's second line.", "Every block once, with its demo content.", "Todos os blocos uma vez, com o conteúdo demo.", "", "Demo card button line"],
}
def fix_demo_rows(v7):
    import re
    for k, row in DEMO_ROWS.items():
        v7, n = re.subn(r'\["' + k + r'",[^\n]*?\],', lambda m: json.dumps(row, ensure_ascii=False) + ',', v7, count=1)
        assert n == 1, k
    return v7

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
                     "v7 = v7.replace('var COMP={', 'var COMP={s5viewer:[\"Card Controls\",\"Picture viewer\"],s5player:[\"Card Controls\",\"Video player\"],s5chip:[\"Card Controls\",\"Placed model chip\"],s5hints:[\"Card Controls\",\"Hints\"],s5states:[\"Card Controls\",\"Loading & states\"],s8onb:[\"Audiences\",\"Who’s exploring\"],s8set:[\"Audiences\",\"Audience in Settings\"],s8look:[\"Audiences\",\"Looks per audience\"],s8lens:[\"Audiences\",\"Lenses\"],s7dock:[\"Guide\",\"Guide panel\"],', 1)\n"
                     "v7 = v7.replace(PBAR_A, PBAR_B, 1)\nv7 = fix_demo_rows(v7)\n"
                     "v8 = '\\n'.join(rd(f) for f in V8 if os.path.exists(os.path.join(D, f)))")
b = "+ v7 + '\\nif(location.hash===\"#selftest\")"
assert b in src, 'hook'
src = src.replace(b, "+ v7 + '\\n' + v8 + '\\nif(location.hash===\"#selftest\")", 1)
c = "rd('v7.css')"
assert c in src
src = src.replace(c, "rd('v7.css') + rd('v8.css') + rd('v8b.css') + rd('v8c.css') + rd('v8d.css') + rd('v8e.css') + rd('v8f.css') + rd('v8g.css') + rd('v8h.css') + rd('v8i.css') + rd('v8j.css') + rd('v8k.css') + rd('v8l.css') + rd('v8m.css') + rd('v8n.css') + rd('v8o.css')", 1)
src = src.replace("'Editor Lab · v7'", "'Editor Lab · v8'")
exec(compile(src, 'build6+8', 'exec'), {'__file__': os.path.join(D, 'build6.py'), '__name__': '__main__', 'fix_real': fix_real, 'fix_demo_rows': fix_demo_rows, 'NEWSTR': NEWSTR, 'V8': V8, 'json': json, 'PBAR_A': PBAR_A, 'PBAR_B': PBAR_B})
