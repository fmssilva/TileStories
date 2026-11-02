import io, base64, os, re
D = os.path.dirname(os.path.abspath(__file__))
def rd(n): return io.open(os.path.join(D, n), encoding='utf-8').read()


def rep1(s, a, b, count=1):
    assert a in s, a[:70]
    return s.replace(a, b, count)


js = rd('v3.js')
for a, b in [('function edCard(){', 'function edCardV3(){'), ('function edMarker(){', 'function edMarkerV3(){'), ('function blockPreview(b,i){', 'function blockPreviewV3(b,i){')]:
    js = rep1(js, a, b)
rep = [('function matches(name,cat){', 'function matchQ(name){var q=S.ui.q.trim().toLowerCase();return !q||name.toLowerCase().indexOf(q)>=0}\nfunction matches(name,cat){'),
       ('matches(m.name,"Motion")', 'matchQ(m.name)'), ('matches(x.name,"Interactive")', 'matchQ(x.name)'), ('matches(f.name,"Structure")', 'matchQ(f.name)'),
       ('if(!matches(r[1],"Block"))if(S.ui.cat!=="All"||S.ui.q)return;', 'if(!matchQ(r[1]))return;'),
       ('ui:{single:false,level:2,q:"",cat:"All"},op:{},', 'ui:{single:false,level:1,q:"",cat:"All"},op:{s3:false,s4:false,s5:false,s6:false,s2:true},'),
       ('S.op.mcard=n===4;S.op.mpages=n===5;S.op["pg:story"]=n===5;', 'S.op.mcard=n===4;S.op.mpages=n===5;S.op["pg:story"]=n===5;if(n===1){S.op["mt:entrance"]=true;S.op["mo:"+S.sel.motion]=true}'),
       ('function thumb(kind,style,cls){return \'<div class="thumb\'', 'function noBtn(h){return h.replace(/<button/g,"<span").replace(/<\\/button>/g,"</span>")}\nfunction thumb(kind,style,cls){return noBtn(\'<div class="thumb\''),
       ("+renderEl(kind,style,{},0,true)+'</div></div>'}", "+renderEl(kind,style,{},0,true)+'</div></div>')}"),
       ('function patThumb(p,cls){return \'<div class="thumb tall\'', 'function patThumb(p,cls){return noBtn(\'<div class="thumb tall\''),
       ("return renderEl(r.kind,styleById(r.style)||defStyle(r.kind),{},0,true)}).join(\"\")+'</div></div>'}", "return renderEl(r.kind,styleById(r.style)||defStyle(r.kind),{},0,true)}).join(\"\")+'</div></div>')}"),
       ('function cardThumb(b){var h=blockPreview(b,0)', 'function cardThumb(b){var h=noBtn(blockPreview(b,0))')]
for a, b in rep:
    js = rep1(js, a, b)

# ---- v7: the phone's words come from the texts of their part; demo content from the element ----
v3fix = [
    ('<span class="lb l">\'+(S.lang==="pt"?"Antes de 1755":"Before 1755")+\'</span><span class="lb r">\'+(S.lang==="pt"?"Hoje":"Today")+\'</span>',
     '<span class="lb l">\'+esc(L(c.blabel)||TXT("before_label"))+\'</span><span class="lb r">\'+esc(L(c.alabel)||TXT("after_label"))+\'</span>'),
    ('<button type="button" aria-label="Play">&#9654;', '<button type="button" aria-label="\'+TXH("audio_play")+\'">&#9654;'),
    ('\'<div class="topbar"><button type="button" data-go="card" aria-label="Back">', '\'<div class="topbar"><button type="button" data-go="card" aria-label="\'+TXH("page_back")+\'">'),
    ('" &#8250; <b>Gallery</b>"', '" &#8250; <b>"+TXH("gallery_name")+"</b>"'),
    ('esc(C(c,"text",DEMO.paragraph))', 'gloss(C(c,"text",DEMO.paragraph))'),
    ('["1755",T("Earthquake, fire and tsunami","Terramoto, incêndio e maremoto")]];',
     '["1755",T("Earthquake, fire and tsunami","Terramoto, incêndio e maremoto")]];if(c.items&&(c.items[S.lang]||c.items.en))ev=(c.items[S.lang]||c.items.en).map(function(s){var p=s.split(" · ");return[p[0],p.slice(1).join(" · ")]});'),
    ('].map(function(s){return\'<div class="step">\'+esc(L(s))+\'</div>\'})', '].map(function(s,j){var it=c.items&&(c.items[S.lang]||c.items.en);return\'<div class="step">\'+esc(it&&it[j]!=null?it[j]:L(s))+\'</div>\'})'),
    ('ANIMSVG+\'<small>\'+esc(({lottie:', 'ANIMSVG+\'<small>\'+(c.file?esc(c.file)+\' · \':\'\')+esc(({lottie:'),
]
for a, b in v3fix:
    js = rep1(js, a, b)
# every language of the wall gets its field; the phone draws the term page and the demo card
js = rep1(js, ';["en","pt"].forEach(function(lg){h+=row(lab', ';langCodes().forEach(function(lg){h+=row(lab', -1)
js = rep1(js, 'S.view==="story"?storyScreen():galleryScreen();', 'S.view==="story"?storyScreen():S.view==="term"?termScreen():S.view==="democard"?demoCardScreen():galleryScreen();')
CHROME = '\'<div class="chrome"><i>&#10005;</i><i>&#9776;</i></div>'
CHROME2 = '\'<div class="chrome"><i title="\'+TXH("close")+\'">&#10005;</i><i title="\'+TXH("visitor_settings")+\'">&#9776;</i></div>'
js = rep1(js, CHROME, CHROME2, -1)

# the literal "PT":"EN" pairs: TXH where the words go into HTML, TXT where they go into textContent or an answer
PAIRS = {'Did you know?': ('fun_fact_label', 'TXT'), 'Drag to turn': ('model_3d_hint', 'TXH'), 'Drag or turn the phone': ('panorama_360_drag_hint', 'TXH'),
         'Find it on the wall': ('locate_label', 'TXH'), 'See more': ('open_row_expand', 'TXH'), 'Next: ': ('peek_next_label', 'TXT'),
         'Show me where it is': ('show_on_wall_button', 'TXH'), 'Previous': ('page_previous', 'TXH'), 'Next': ('page_next', 'TXH'),
         'Your turn…': ('guide_your_turn', 'TXH'), 'Ask': ('guide_ask_button', 'TXH'), 'Resume the tour': ('guide_resume', 'TXT'),
         'Ask the guide': ('guide_ask_title', 'TXH'), 'e.g. What happened to the palace after the earthquake?': ('guide_ask_example', 'TXH'),
         'Type your question…': ('guide_ask_placeholder', 'TXH'), 'Send': ('guide_ask_send', 'TXH'), 'I could not find that on this card.': ('guide_not_found', 'TXT'),
         'Thinking…': ('guide_thinking', 'TXT'), 'Start the guide': ('guide_start', 'TXH'), 'Meet the palace with the guide': ('guide_block_title', 'TXH'),
         'Narrated tour · 3 min · ask anything, any time': ('guide_block_line', 'TXH'), 'The microphone is not available in this browser.': ('guide_no_mic', 'TXT'),
         'Listening…': ('guide_listening', 'TXT')}
used = set()


def pairs(src):
    def sub(m):
        en = m.group(2)
        if en not in PAIRS:
            return m.group(0)
        used.add(en)
        k, f = PAIRS[en]
        return f + '("' + k + '")'
    return re.sub(r'S\.lang==="pt"\?"([^"]*)":"([^"]*)"', sub, src)


js = pairs(js)
v4 = rd('v4.js')
a = 'ed.addEventListener("click",function(e){var t=e.target;\n  var sk='
v4 = rep1(v4, a, 'ed.addEventListener("click",function(e){var t=e.target;\n  if(t.closest("[data-seckind],[data-secicon],[data-seccol],[data-secmove],[data-secdel],[data-secadd],[data-blockdef],[data-poisec]"))e.stopPropagation();\n  var sk=')
v4 = rep1(v4, 'renderAll(true);if(path.indexOf("test.")!==0)show("card");return}', 'renderAll(true);show("card");return}')
v4 = rep1(pairs(v4), CHROME, CHROME2, -1)
v4 = rep1(v4, 'var body=row("Name · EN",\'<input class="u" data-path="sections.\'+i+\'.name.en" value="\'+esc(sec.name.en)+\'">\',"The word on the Navigation Bar and the section\'s eyebrow.")+row("Name · PT",\'<input class="u" data-path="sections.\'+i+\'.name.pt" value="\'+esc(sec.name.pt)+\'">\',"")+',
          'var body=langCodes().map(function(lg,j){return row("Name · "+lg.toUpperCase(),\'<input class="u" data-path="sections.\'+i+\'.name.\'+lg+\'" value="\'+esc(sec.name[lg]||"")+\'">\',j?"":"The word on the Navigation Bar and the section\'s eyebrow.")}).join("")+')
v4 = rep1(v4, 'seg("previewLang",["en","pt"],["EN","PT"],S.lang)', 'seg("previewLang",langCodes(),langCodes().map(function(c){return c.toUpperCase()}),S.lang)')
v5 = pairs(rd('v5.js'))
v5 = rep1(v5, '["card","marker"].forEach(function(tab){S.tab=tab;real.renderEditor();', '["card","marker","words","global"].forEach(function(tab){S.tab=tab;real.renderEditor();')
v6 = pairs(rd('v6.js'))
v6 = rep1(v6, '(st.text?row("Says · EN",\'<textarea class="u" rows="2" data-gtext="\'+st.id+\'|en">\'+esc(st.text.en)+\'</textarea>\',"")+row("Says · PT",\'<textarea class="u" rows="2" data-gtext="\'+st.id+\'|pt">\'+esc(st.text.pt)+\'</textarea>\',""):',
          '(st.text?langCodes().map(function(lg){return row("Says · "+lg.toUpperCase(),\'<textarea class="u" rows="2" data-gtext="\'+st.id+\'|\'+lg+\'">\'+esc(st.text[lg]||"")+\'</textarea>\',"")}).join(""):')
# the test tile, the wall hint, the top band and the tour's controls take their words from their component
TILE = '\'<button class="testtile" type="button" data-go="demo"><div><b>TEST · NOT IN THE APP</b><strong>\'+esc(L(UI.demo))+\'</strong><span>Motion, interaction, effects, elements, patterns.</span></div><em>&#8250;</em></button>\''
js = rep1(js, TILE, 'testTile()')
v4 = rep1(v4, TILE, 'testTile()')
v5 = rep1(v5, '\'<div class="walltip">Tap a marker to open its card</div>\'', '\'<div class="walltip">\'+TXH("wall_tip")+\'</div>\'')
v5 = rep1(v5, 'b.innerHTML=\'<i>&#10005;</i>\'', 'b.innerHTML=\'<i title="\'+TXH("close")+\'">&#10005;</i>\'')
v5 = rep1(v5, '\'<i>&#9776;</i>\'', '\'<i title="\'+TXH("visitor_settings")+\'">&#9776;</i>\'')
for a, b in [('data-g="prev" aria-label="Previous"', 'data-g="prev" aria-label="\'+TXH("guide_ctrl_prev")+\'"'),
             ('aria-label="\'+(G.paused?"Play":"Pause")+\'"', 'aria-label="\'+(G.paused?TXH("guide_ctrl_play"):TXH("guide_ctrl_pause"))+\'"'),
             ('data-g="next" aria-label="Next"', 'data-g="next" aria-label="\'+TXH("guide_ctrl_next")+\'"'),
             ('aria-label="Mute"', 'aria-label="\'+TXH("guide_ctrl_mute")+\'"'),
             ('data-g="stop" aria-label="Stop the guide"', 'data-g="stop" aria-label="\'+TXH("guide_ctrl_stop")+\'"'),
             ('data-g="askclose" aria-label="Close"', 'data-g="askclose" aria-label="\'+TXH("guide_ask_close")+\'"'),
             ('data-g="mic" aria-label="Speak your question"', 'data-g="mic" aria-label="\'+TXH("guide_ask_mic")+\'"'),
             ('data-g="stopask">Stop<', 'data-g="stopask">\'+TXH("guide_ask_stop")+\'<'),
             ('(ai?"AI · answers only from this card":"Search · offline")', '(ai?TXH("guide_ask_mode_ai"):TXH("guide_ask_mode_search"))'),
             ('{flip:"&#9758; Tap to flip",drag:"&#8596; Drag across",answer:"&#9758; Tap an answer"}[kind]||"Try it"',
              '({flip:"&#9758; "+TXH("guide_coach_flip"),drag:"&#8596; "+TXH("guide_coach_drag"),answer:"&#9758; "+TXH("guide_coach_answer")}[kind]||TXH("guide_coach_try"))')]:
    v6 = rep1(v6, a, b)
js = rep1(js, " · stand-in", "")
js = rep1(js, 'au.innerHTML=a.classList.contains("playing")?"&#10073;&#10073;":"&#9654;";return}', 'au.innerHTML=a.classList.contains("playing")?"&#10073;&#10073;":"&#9654;";au.setAttribute("aria-label",a.classList.contains("playing")?TXT("audio_pause"):TXT("audio_play"));return}')
js = rep1(js, 'data-turn="-1">&#8249;</button><button type="button" data-turn="1">&#8250;', 'data-turn="-1" aria-label="\'+TXH("carousel_prev")+\'">&#8249;</button><button type="button" data-turn="1" aria-label="\'+TXH("carousel_next")+\'">&#8250;')
v5 = rep1(v5, '<button type="button" data-go="card" aria-label="Back">&#8249;</button><span></span>', '<button type="button" data-go="card" aria-label="\'+TXH("page_back")+\'">&#8249;</button><span></span>')
v5 = rep1(v5, 'pageFoot(["card",L(UI.about)],["gallery","Gallery"])', 'pageFoot(["card",L(secById("about").name)],["gallery",TXT("gallery_name")])')
v5 = rep1(v5, 'pageFoot(["story",L(DEMO.title)],["card",L(UI.visit)])', 'pageFoot(["story",L(DEMO.title)],["card",L(secById("visit").name)])')
v5 = rep1(v5, 'data-pagesec="\'+ids[(i-1+ids.length)%ids.length]+\'">&#8249;', 'data-pagesec="\'+ids[(i-1+ids.length)%ids.length]+\'" aria-label="\'+TXH("nav_pager_previous")+\'">&#8249;')
v5 = rep1(v5, 'data-pagesec="\'+ids[(i+1)%ids.length]+\'">&#8250;', 'data-pagesec="\'+ids[(i+1)%ids.length]+\'" aria-label="\'+TXH("nav_pager_next")+\'">&#8250;')
v6 = rep1(v6, 'aria-pressed="\'+S.guideCfg.captions+\'">CC</button>', 'aria-pressed="\'+S.guideCfg.captions+\'">\'+TXH("guide_ctrl_captions")+\'</button>')
v6 = rep1(v6, '(S.lang==="pt"?"(IA indisponível aqui, pesquisa offline) ":"(AI unavailable here, offline search) ")', 'TXT("guide_ai_offline")')
v6 = rep1(v6, '(S.lang==="pt"?"(IA indisponível: ":"(AI unavailable: ")+(e&&e.code||"error")+") "', 'TXT("guide_ai_error").replace("{0}",(e&&e.code||"error"))')
v6 = rep1(v6, '(TXH("guide_start"))+\' · 3 min</button>\'', 'TXH("guide_start")+\' · \'+esc(fmt(TXT("guide_duration"),3))+\'</button>\'')
js = rep1(js, 'topbar(esc(L(UI.stories))', 'topbar(esc(L(secById("stories").name))')
js = rep1(js, 'topbar(esc(L(UI.media))', 'topbar(esc(L(secById("media").name))')
v5 = rep1(v5, '(S.stop==="closed"?', '(S.stop==="closed"&&S.wallTip!==false?')
missing = set(PAIRS) - used
assert not missing, missing
v7 = rd('v7.js').replace('__REALSTR__', rd('realstrings.json'))
init = 'S.focusRef="m:rise";renderAll(false);setTimeout(function(){flashRef("m:rise")},300);'
js = rep1(js, init, v4 + '\n' + v5 + '\n' + v6 + '\n' + v7 + '\nif(location.hash==="#selftest"){var r0=selfTest();showSelfTest(r0)}\nS.view="card";S.tab="card";if(location.hash!=="#selftest")renderAll(false);\nif(location.hash==="#guide"){S.stop="full";renderAll(false);startTour();G.i=4;runStep()}\nif(location.hash==="#words"){S.tab="words";S.sel.term="t1";renderEditor();S.view="story";renderPhone(false)}\nif(location.hash==="#texts"){["s0","s2","s3","s4","s7"].forEach(function(id){S.op[id]=false});S.op["tp:s1b:Pages layout › chevrons"]=true;S.op["tp:s5page:Previous / Next buttons"]=true;S.op["tp:s5page:Back button"]=true;goOwner("s1b",true);goOwner("s5page",true);show("card")}\nif(location.hash.indexOf("#open=")===0){var ids=decodeURIComponent(location.hash.slice(6)).split(",");S.op={};["s0","s1","s2","s3","s4","s5","s7"].forEach(function(x){S.op[x]=false});var vw=null;ids.forEach(function(i){if(i.indexOf("tab:")===0)S.tab=i.slice(4);else if(i.indexOf("view:")===0)vw=i.slice(5);else S.op[i]=true});S.ui.level=0;renderEditor();if(vw){S.view=vw;renderPhone(false)}}\nif(location.hash.indexOf("#toggle=")===0){var tg=decodeURIComponent(location.hash.slice(8)).split(",");S.op={};tg.slice(0,-1).forEach(function(i){S.op[i]=true});S.ui.level=0;renderEditor();setTimeout(function(){var dd=ed.querySelector(\'[data-fo="\'+tg[tg.length-1]+\'"]\');if(dd){dd.open=true;window.scrollTo(0,0)}},400)}\nif(location.hash==="#fr"){S.langs.list.push({code:"fr",name:"Français"});S.tab="words";S.ui.txf="content";S.sel.term="t1";renderEditor();S.termOpen="t1";S.view="term";renderPhone(false)}\n')
for k, f in [('__HERO__', 'paco_hero'), ('__G1__', 'paco_gallery_1'), ('__G2__', 'paco_gallery_2'), ('__G3__', 'paco_gallery_3'), ('__BEFORE__', 'paco_before_1755')]:
    js = js.replace(k, 'data:image/jpeg;base64,' + base64.b64encode(open(os.path.join(D, f + '.jpg'), 'rb').read()).decode())
css = rd('v2.css') + rd('v3extra.css') + '.navbar span em{flex:none}.navbar span{gap:4px}\n' + rd('v5.css') + rd('v6.css') + rd('v7.css')
body = rd('v3body.html').replace('Editor Lab · v3', 'Editor Lab · v7')
body = rep1(body, '<p class="lede">From the smallest part', '<p class="lede">The whole flow: tap the marker on the wall, the card opens at its stop, drag it between Peek, Half and Full, jump with the bar, open a page. The Detail Card tab: 0 Preview &amp; Test, 1 Card, 2 Sections, 3 Blocks &amp; Components, 4 Motion &amp; Effects, 5 Card Controls, 6 Guide. Every text lives on the part that draws it (a button, a hint, a heading), one field per wall language. Every element has its <b>Demo content</b>; every block\'s demo is its block on the independent <b>demo card</b>. No Shared Content section. A fourth tab, <b>Languages &amp; Terms</b>: the wall\'s languages, one list of terms for the glossary and the search\'s synonyms (a term may have its own small page), and a check of every text, interface and content, editing the same values. From the smallest part')
st = '<div class="card2"><h2>Does every option work?</h2><p>The self-test tries every value of every Editor control on every tab, every text field, every demo field and every term, and checks that the phone shows something different (or marks it as behaviour: sound, taps, timing; or as a part the Lab does not draw).</p><button id="runtest" type="button">Run the self-test</button><div id="selftest"></div></div>'
body = rep1(body, '<div class="notes">', '<div class="notes">' + st)
head = '<title>Block Components Lab</title>\n<meta name="claude:viewport-fit-sides" content="cover">\n<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Fraunces:opsz,wght@9..144,500;9..144,600&family=Atkinson+Hyperlegible+Next:wght@400;600;700&family=IBM+Plex+Mono:wght@400;500&display=swap">\n'
io.open(os.path.join(D, 'block-components-lab.html'), 'w', encoding='utf-8').write(head + '<style>' + css + '</style>\n' + body + '\n<script>\n' + js + '\n</script>\n')
print('built', len(js))
