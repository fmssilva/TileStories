// g9 checks (Playwright + the installed Edge): node test9m.js -> PASS/FAIL per check
// Guidance, round 9 (_3.8 section 8): the visitor's pull. Things to try in the ✦ menu (Shown / Hidden), its rows from the App items that
// teach (In Things to try per item), the count, the ticks from the same memory as Done when, the all-done line; each Show me from the
// sheet (the gesture on its target, the visitor taken there first: the card at its element, the wall's marker, the ✦, the guide's Pause)
// with no memory written; Tips Off and Essentials only: no pushed tip, Things to try still there and its Show me still plays; Ask Azu's
// "how do I…" offline in English and Portuguese (the feature's words, a Show me card that plays, the item seen and not done; a history
// question gets no card; on the wall too); the live agent's tool show_how (its schema and its result, called in the page with no
// network; a test double calling it puts the card under the answer; Off: no tool, no rule in the prompt); Settings › The guide's link;
// Fewer like this's toast line on an App group, not on a Content group (the rest as before); the visitor's line in Preview & Test; every
// new row's demo; the four scenes in Ask Azu's console. MUTATE=1 node test9m.js makes Tips Off hide Things to try too: the checks named
// [tips off] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const t00 = Date.now();
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  const lap = n => { if (process.env.TIMING) console.log('TIME ' + n + ' ' + Math.round((Date.now() - t00) / 1000) + ' s'); };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('var __gi=gbItems;gbItems=function(){var o=__gi();return gdPrefs().tips==="off"?o.filter(function(k){return k!=="try"}):o};var __to=gdTryOpen;gdTryOpen=function(f,d){if(gdPrefs().tips==="off")return false;return __to(f,d)}'); console.log('MUTATION: Tips Off hides Things to try too'); }
  await R('window.__g0=JSON.stringify(S.guidance)');
  const base = 'if(G.gd.pages)gdPagesEnd("test",false);S.guidance=JSON.parse(window.__g0);S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.ui.gdSR=false;S.ui.gdToday="2026-03-10";S.m.solemn=false;S.vis.aud="explorer";S.aud.where="card";S.aud.seen=true;S.guideCfg.life="lively";S.guideCfg.gbtn="both";S.guideCfg.mascot=true;S.guideCfg.ask="search";S.agentCfg.voiceOut=false;S.agentCfg.pol.show_how="auto";G.gd.demo=false;G.gd.camPerm="allowed";gdChip(null);G.gd.tryOpen=false;G.gd.tryShow=null;';
  /* the push rests (only The start speaks), a first visit on the wall, the clock by hand */
  const fresh = async (extra) => { await R(base + 'gdGroupById("inview").on=false;S.guidance.items.gbhint.on=false;gdGroupById("card").on=false;gdKindById("fun").on=false;if(G.on)stopTour();stopSpeech();G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null,turn:null};G.agTurn=null;S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.camZoom=1;S.view="card";S.stop="closed";renderPhone(false);G.gd.manual=true;gdStop("test");gdResetVisitor();G.gd.movedAt=null;G.gd.touchAt=null;ph.querySelectorAll(".gdtoast").forEach(function(x){x.remove()});' + (extra || '')); await W(100); };
  const past = 'S.view="card";S.stop="full";renderPhone(false);scr.scrollTop=900;fabLayer();';
  const menuItems = 'var m=ph.querySelectorAll(".gbmenu [data-gbgo]");[[].map.call(m,function(b){return b.dataset.gbgo}).join(),[].map.call(m,function(b){return Math.round(b.getBoundingClientRect().height*ph.offsetHeight/ph.getBoundingClientRect().height)})]';
  const rowsNow = '[].map.call(ph.querySelectorAll(".gdtry .gdtry-row"),function(r){return r.dataset.gdtryrow+(r.classList.contains("done")?"+":"")})';
  /* where Show me played: its gesture, the screen, and the finger's end inside the target's box (its tip rests at the edge) */
  const played = id => until(`!!(G.gd.tryShow&&G.gd.tryShow.id==="${id}"&&G.gd.tryShow.played)`, 3500);
  const onTarget = 'var s=G.gd.tryShow,b=s&&s.box,m=24;[s&&s.gesture,s&&s.view,s&&s.stop,!!(s&&s.end&&b&&s.end[0]>=b.l-m&&s.end[0]<=b.r+m&&s.end[1]>=b.t-m&&s.end[1]<=b.b+m),!!ph.querySelector(".gdfinger"),!ph.querySelector(".gdtry"),s&&s.where]';
  const editor = async (ops) => { await R(`S.tab="guide";S.ui.level=1;S.op=${JSON.stringify(ops)};renderEditor()`); await W(120); };
  const px = 'function px(c){var m=String(c).match(/[\\d.]+/g).map(Number);return m}function lum(c){var a=px(c).slice(0,3).map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.2)});return .2126*a[0]+.7152*a[1]+.0722*a[2]}function cr(a,b){var x=lum(a),y=lum(b);return(Math.max(x,y)+.05)/(Math.min(x,y)+.05)}';

  lap('before 1');
  // ---------- 1. alone: the rows, In Things to try's default, the count and the ticks, the match in both languages ----------
  await fresh();
  const d1 = await R('[gdTryDefs().map(function(x){return x.id+":"+gdTryWord(x)}),["tapmarker","askview","gbhint","label","navbar","ba","strip","pause","nudge","term","opening","tourof"].map(function(id){var it=S.guidance.items[id];return id+"="+it.inTry})]');
  ok('Things to try\'s rows: the App items that teach a feature, one a feature, in the Editor\'s order (8: Open a card … Pause the guide); In Things to try Yes where an App item Teaches, unset elsewhere',
    d1[0].join() === 'tapmarker:Open a card,askview:Ask about the view,gbhint:Open the ✦ menu,label:Find it on the wall,navbar:Jump to a part,ba:Drag Before / After,strip:Swipe the strips,pause:Pause the guide' &&
    d1[1].join() === 'tapmarker=true,askview=true,gbhint=true,label=true,navbar=true,ba=true,strip=true,pause=true,nudge=false,term=false,opening=false,tourof=undefined', d1);
  const d2 = await R('var V=JSON.parse(JSON.stringify(S.gdVisitor)),c0=gdTryCount(V);V.used.card=1;V.used.ba=2;var c1=gdTryCount(V);V.ever.items.navbar={shown:0,done:true,by:"wk_bar"};var c2=gdTryCount(V);' +
    '["view","menu","locate","strip","pause"].forEach(function(f){V.used[f]=3});var c3=gdTryCount(V);[c0,c1,c2,c3,gdTryDone(S.guidance.items.navbar,V),fmt(TXT("gd_try_count"),c1.done,c1.n)]');
  ok('[Done when] the count and the ticks from the same memory: the feature used (Open a card, Before / After) or the item done for good (the Navigation Bar\'s step seen): 0 → 2 → 3 → 8 of 8; "2 of 8 tried"',
    JSON.stringify(d2.slice(0, 4).map(c => c.done + '/' + c.n)) === '["0/8","2/8","3/8","8/8"]' && d2[4] === true && d2[5] === '2 of 8 tried', d2);
  const QS = ['How do I pause?', 'how do I open a card?', 'How can I see the before and after?', 'How do I find it on the wall?', 'How do I jump to a part?', 'How do I open the menu?', 'How do I ask about the view?', 'How do I swipe the strips?',
    'como vejo o antes e depois?', 'Como pauso o guia?', 'Como abro um cartão?', 'Como deslizo as faixas?', 'How old is the palace?', 'Who built it?', 'Como era o palácio antes do terramoto?', 'How did the earthquake change the square?', 'What happened in 1755?'];
  const d3 = await R(`${JSON.stringify(QS)}.map(function(q){var it=gdHowFind(q);return it?it.id:"-"})`);
  ok('"How do I…" matched on the question\'s words, in English and Portuguese; a question about the history matches nothing',
    d3.join() === 'pause,tapmarker,ba,label,navbar,gbhint,askview,strip,ba,pause,tapmarker,strip,-,-,-,-,-', QS.map((q, i) => q + ' → ' + d3[i]));

  lap('before 2');
  // ---------- 2. the ✦ menu: Things to try (Shown / Hidden), on the card and on the wall ----------
  await fresh(past); await p.locator('#ph .gbtn').click(); await W(200);
  const m1 = await R(menuItems);
  await fresh(); await p.locator('#ph .gbtn').click(); await W(200);
  const m2 = await R(menuItems);
  ok('The ✦ menu has Things to try (real clicks): on the card Start the tour, Ask, How does this work?, Things to try, Guide settings; on the wall Ask about the view too; 44 px items',
    m1[0] === 'tour,ask,how,try,settings' && m2[0] === 'tour,ask,view,how,try,settings' && m1[1].concat(m2[1]).every(h => h >= 44), [m1, m2]);
  await R('S.guidance.tryMenu=false;G.fab.menu=false;fabLayer();G.fab.menu=true;fabLayer()');
  const m3 = await R(menuItems);
  await R('G.fab.menu=false;fabLayer();S.settingsOpen=true;renderPhone(false)'); await p.locator('#ph .vset .vtry').click(); await W(200);
  const m4 = await R(`[!!ph.querySelector(".gdtry"),!S.settingsOpen,${rowsNow}.length]`);
  ok('Things to try Hidden: not in the ✦ menu; Settings › The guide › Things to try › still opens it (a real click), Settings closed', m3[0] === 'tour,ask,view,how,settings' && m4[0] && m4[1] && m4[2] === 8, [m3, m4]);

  lap('before 3');
  // ---------- 3. the sheet: the rows, the count, the ticks, the all-done line; 44 px, contrast, nothing over its words ----------
  await fresh(); await p.locator('#ph .gbtn').click(); await W(150); await p.locator('#ph [data-gbgo="try"]').click(); await W(250);
  const s1 = await R(`[!!ph.querySelector(".gdtry"),!ph.querySelector(".gbmenu"),(ph.querySelector(".gdtry-n")||{}).textContent,${rowsNow},[].map.call(ph.querySelectorAll(".gdtry .gdtry-w"),function(x){return x.textContent}).join("|"),gdContext().panel]`);
  ok('Things to try (a real click on the ✦ menu): the sheet, the menu closed; "1 of 8 tried" (the ✦ menu just opened ticks Open the ✦ menu); its rows in order with their words; a panel (the scheduler quiet)',
    s1[0] && s1[1] && s1[2] === '1 of 8 tried' && s1[3].join() === 'tapmarker,askview,gbhint+,label,navbar,ba,strip,pause' && s1[4] === 'Open a card|Ask about the view|Open the ✦ menu|Find it on the wall|Jump to a part|Drag Before / After|Swipe the strips|Pause the guide' && s1[5] === 'Things to try', s1);
  const look = async (dark, lang) => { await R(`S.dark=${dark};S.lang="${lang}";renderAll(true)`); await W(300); return R(px + `var s=ph.querySelector(".gdtry"),q=ph.getBoundingClientRect().height/ph.offsetHeight,h=function(e){return Math.round(e.getBoundingClientRect().height/q)},bg=getComputedStyle(s).backgroundColor;
    var over=0;[].forEach.call(s.querySelectorAll(".gdtry-w,.gdtry-n,.gdtry-h b"),function(w){var r=w.getBoundingClientRect(),e=document.elementFromPoint(r.left+Math.min(8,r.width/2),r.top+r.height/2);if(!e||!s.contains(e))over++});
    [Math.min.apply(null,[].map.call(s.querySelectorAll(".gdtry-show,.gdtry-x"),h)),Math.min(cr(getComputedStyle(s.querySelector(".gdtry-w")).color,bg),cr(getComputedStyle(s.querySelector(".gdtry-show")).color,bg),cr(getComputedStyle(s.querySelector(".gdtry-n")).color,bg),cr(getComputedStyle(s.querySelector(".gdtry-row.done .gdtry-show")||s.querySelector(".gdtry-show")).color,bg)),over,s.querySelector(".gdtry-h b").textContent,s.querySelector(".gdtry-n").textContent,s.getBoundingClientRect().bottom<=ph.getBoundingClientRect().bottom+1]`); };
  const lk = [await look(false, 'en'), await look(true, 'en'), await look(false, 'pt'), await look(true, 'pt')];
  await R('S.dark=false;S.lang="en";renderAll(true)');
  ok('...its look, light and dark, English and Portuguese: Show me and ✕ 44 px, words 4.5:1 or more, nothing over its words, inside the phone', lk.every(l => l[0] >= 44 && l[1] >= 4.5 && l[2] === 0 && l[5]) && lk[2][3] === 'Coisas a experimentar' && lk[2][4] === '1 de 8 experimentadas', lk);
  await R('S.gdVisitor.used.card=gdNow();S.gdVisitor.used.ba=gdNow();S.gdVisitor.used.pause=gdNow();renderPhone(true)');
  const s2 = await R(`[(ph.querySelector(".gdtry-n")||{}).textContent,${rowsNow}.filter(function(x){return/\\+$/.test(x)}).length,(ph.querySelector('[data-gdtryrow="ba"] .gdtry-tick')||{getAttribute:function(){return""}}).getAttribute("aria-label")]`);
  await R('["view","menu","locate","navbar","strip"].forEach(function(f){S.gdVisitor.used[f]=gdNow()});renderPhone(true)');
  const s3 = await R(`[(ph.querySelector(".gdtry-n")||{}).textContent,${rowsNow}.length,${rowsNow}.every(function(x){return/\\+$/.test(x)}),!!ph.querySelector(".gdtry-n.all")]`);
  await R('gdTryClose();G.fab.menu=true;fabLayer()'); const s4 = await R(menuItems); await p.locator('#ph [data-gbgo="try"]').click(); await W(200);
  const s5 = await R('[!!ph.querySelector(".gdtry"),(ph.querySelector(".gdtry-n")||{}).textContent]');
  ok('...the ticks follow the memory: 4 of 8 tried, a done row says Done; every row done: "You\'ve tried them all. Well done!", the rows stay ticked, the ✦ menu still opens it',
    s2[0] === '4 of 8 tried' && s2[1] === 4 && s2[2] === 'Done' && s3[0] === "You've tried them all. Well done!" && s3[1] === 8 && s3[2] && s3[3] && s4[0].indexOf('try') >= 0 && s5[0] && s5[1] === s3[0], [s2, s3, s4[0], s5]);
  await R('gdTryClose()');

  lap('before 4');
  // ---------- 4. each Show me from the sheet: the visitor taken there, the gesture on its target, no memory ----------
  const SM = [['ba', past, 'drag', 'card', 'full'], ['strip', past, 'swipe', 'card', 'full'], ['navbar', past, 'tap', 'card', 'full'], ['label', past, 'tap', 'card', 'half'], ['tapmarker', past, 'tap', 'card', 'closed'],
    ['askview', '', 'tap', 'card', 'closed'], ['gbhint', past, 'tap', 'card', 'full'], ['pause', past, 'tap', 'card', 'full']];
  const sm = {};
  for (const [id, where, g, v, st] of SM) {
    await fresh(where + 'gdTryOpen("test")'); await W(150);
    const mem0 = await R('JSON.stringify([S.gdVisitor.visit.items,S.gdVisitor.ever.items,S.gdVisitor.used])');
    if (id === 'ba') await p.locator('#ph .gdtry [data-gdtryshow="ba"]').click(); else await R(`ph.querySelector('.gdtry [data-gdtryshow="${id}"]').click()`);
    const pl = await played(id), o = await R(onTarget), gate = await R('gdContext().showing||""'), mem1 = await R('JSON.stringify([S.gdVisitor.visit.items,S.gdVisitor.ever.items,S.gdVisitor.used])');
    sm[id] = [pl, o, gate, mem0 === mem1, id === 'pause' ? await R('[G.on,S.dockH]') : null];
    await R('if(G.on)stopTour();gdFingerStop()');
  }
  const smOk = id => { const [pl, o, gate, same] = sm[id], want = SM.filter(x => x[0] === id)[0]; return pl && o[0] === want[2] && o[1] === want[3] && o[2] === want[4] && o[3] && o[4] && o[5] && /^Show me/.test(gate) && same; };
  ok('Show me on the card (Before / After by a real click): the card at its element, the finger drags across it; strips: a swipe; the Navigation Bar and the Locate Button: a tap on them', ['ba', 'strip', 'navbar', 'label'].every(smOk), ['ba', 'strip', 'navbar', 'label'].map(id => [id, sm[id]]));
  ok('...Open a card from the card: the visitor taken to the wall, the finger taps the nearest marker; Ask about the view and the ✦ menu: a tap on the ✦ where it shows', ['tapmarker', 'askview', 'gbhint'].every(smOk), ['tapmarker', 'askview', 'gbhint'].map(id => [id, sm[id]]));
  ok('...Pause the guide: a tour starts at Mini and the finger taps ⏸; while each plays the scheduler is quiet ("Show me …"), and none writes the visitor\'s memory', smOk('pause') && sm.pause[4][0] && sm.pause[4][1] === 'mini', sm.pause);
  await fresh(); const fw = await R('gdTryShow("tapmarker","test");0'); await played('tapmarker');
  const fo = await R('var f=ph.querySelector(".gdfinger"),r=f&&gdFingerBox(+f.dataset.endx,+f.dataset.endy,parseFloat(f.style.rotate)||0),a=0;gdObstacles().filter(function(o){return o.text}).forEach(function(o){if(r)a+=gdOver(r,o.b)});[!!r,Math.round(a)]');
  ok('...the finger at its end is 0 px over the wall\'s words', fo[0] && fo[1] === 0, fo);

  lap('before 5');
  // ---------- 5. Tips Off and Essentials only: no pushed tip, Things to try still there, its Show me still plays ----------
  for (const tips of ['off', 'essentials']) {
    await fresh(`gdPrefs().tips="${tips}";`); await R('gdAdvance(60000)');
    const t1 = await R('[!G.gd.cur||G.gd.cur.item.essential,S.gdVisitor.visit.starts.length,G.gd.last&&G.gd.last.why]'); await R('gdStop("test")');
    await p.locator('#ph .gbtn').click(); await W(150); const mi = await R(menuItems);
    const op = await R('var b=ph.querySelector(\'[data-gbgo="try"]\');if(b)b.click();[!!ph.querySelector(".gdtry"),ph.querySelectorAll(".gdtry .gdtry-row").length]');
    await R('var b=ph.querySelector(\'.gdtry [data-gdtryshow="tapmarker"]\');if(b)b.click()'); const pl = await played('tapmarker'); const o = await R(onTarget);
    ok(`[tips off] Tips ${tips === 'off' ? 'Off' : 'Essentials only'}: ${tips === 'off' ? 'no tip in 60 s (not even the opening)' : 'only the Essential opening'}; the ✦ menu still has Things to try, its 8 rows, and its Show me plays on the marker`,
      t1[0] && (tips === 'off' ? t1[1] === 0 && /Tips Off/.test(t1[2]) : t1[1] <= 1) && mi[0].indexOf('try') >= 0 && op[0] && op[1] === 8 && pl && o[3], [t1, mi[0], op, o]);
  }

  lap('before 6');
  // ---------- 6. In Things to try, per item (the Editor's own button) ----------
  await fresh(); await editor({ gt4: true, 'gd:app': true, 'gd:g:card': true, 'gd:i:ba': true });
  await R('var b=ed.querySelector(\'[data-set="@guidance.items.ba.inTry"][data-v="false"]\');b.click()'); await W(150);
  const it1 = await R(`[S.guidance.items.ba.inTry,gdTryDefs().map(function(x){return x.id}).join(),(gdTryOpen("test"),${rowsNow}.join()),!!ed.querySelector('[data-fo="gd:i:ba"] .gdtryrb')]`);
  await R('gdTryClose();var b=ed.querySelector(\'[data-set="@guidance.items.ba.inTry"][data-v="true"]\');b.click()'); await W(150);
  const it2 = await R(`[S.guidance.items.ba.inTry,(gdTryOpen("test"),${rowsNow}.join())]`);
  await R('gdTryClose();S.guidance.items.tapmarker.inTry=false;var n=gdNewItem("step","start");S.guidance.items[n.id].on=false;window.__n=n.id');
  const it3 = await R('[gdTryDefs().map(function(x){return x.id})[0],S.guidance.items[window.__n].inTry,gdTryDefs().length]');
  ok('In Things to try, per item: No takes the row out (7 rows), Yes puts it back; one row a feature: Open a card out of Tap a marker, a new step that teaches it (Off as a tip) is its row', it1[0] === false && it1[1].split(',').indexOf('ba') < 0 && it1[2].split(',').length === 7 && it1[3] && it2[0] === true && it2[1].split(',').length === 8 && /^gdn/.test(it3[0]) && it3[1] === true && it3[2] === 8, [it1, it2, it3]);
  await R('gdDelItem(window.__n);S.guidance=JSON.parse(window.__g0)');

  lap('before 7');
  // ---------- 7. Ask Azu's "how do I…" offline: English, Portuguese, a history question, the wall ----------
  await fresh(past + 'gbGo("ask");');
  await p.locator('#ph #gq').fill('How do I see the before and after?'); await p.locator('#ph #gq').press('Enter');
  await until('!!ph.querySelector(".gdhowc")', 2500);
  const a1 = await R('[G.answer,!!ph.querySelector(".gd-thread .gdhowc"),(ph.querySelector(".gdhowc small")||{}).textContent,gdVM("ba").told,gdVM("ba").shown,gdEverOf(S.gdVisitor,"ba").done,gdTryDone(S.guidance.items.ba),Math.round(ph.querySelector(".gdhowc").getBoundingClientRect().height*ph.offsetHeight/ph.getBoundingClientRect().height)]');
  await p.locator('#ph .gdhowc').last().click(); const ap = await played('ba'); const ao = await R(onTarget);
  ok('"How do I see the before and after?" typed in the dock (real keys), no key: the feature\'s words, a Show me card (44 px) under the answer; the item seen (not done); the card\'s tap plays the drag on the Before / After',
    a1[0] === 'Drag the line across the picture to compare then and now.' && a1[1] && a1[2] === 'Drag Before / After' && a1[3] === 1 && a1[4] === 1 && a1[5] === false && a1[6] === false && a1[7] >= 44 && ap && ao[0] === 'drag' && ao[3], [a1, ao]);
  await fresh(past + 'S.lang="pt";renderAll(true);gbGo("ask");ask("Como pauso o guia?")'); await until('!!ph.querySelector(".gdhowc")', 2500);
  const a2 = await R('[G.answer,(ph.querySelector(".gdhowc b")||{}).textContent,(ph.querySelector(".gdhowc small")||{}).textContent]');
  await R('ph.querySelector(".gdhowc").click()'); const pp = await played('pause'); const po = await R(onTarget + '.concat([G.on,S.dockH])');
  ok('...in Portuguese: "Como pauso o guia?": the Portuguese words and Mostre-me; its tap starts the tour at Mini and taps ⏸', a2[0] === 'Durante a visita, toque em ⏸ ao lado do botão de falar.' && a2[1] === 'Mostre-me' && a2[2] === 'Pausar o guia' && pp && po[3] && po[7] && po[8] === 'mini', [a2, po]);
  await fresh(past + 'gbGo("ask");ask("How old is the palace?")'); await W(300);
  const a3 = await R('[!ph.querySelector(".gdhowc"),!(G.agTurn&&G.agTurn.how),G.answer]');
  await fresh('G.cam.open=true;renderPhone(true);camAsk("How do I open a card?")'); await until('!!ph.querySelector(".camask .gdhowc")', 2500);
  const a4 = await R('[(ph.querySelector(".ca-ans")||{}).textContent||"",!!ph.querySelector(".camask .gdhowc")]');
  await R('camAsk("What am I looking at?")'); await W(500); const a5 = await R('!ph.querySelector(".camask .gdhowc")');
  ok('...a history question ("How old is the palace?") gets the scripted search, no Show me card; on the wall Ask about the view answers "How do I open a card?" with its card, and the next question drops it', a3[0] && a3[1] && a4[0].indexOf('Point at the tiles and tap a glowing marker') === 0 && a4[1] && a5, [a3, a4, a5]);

  lap('before 8');
  // ---------- 8. the live agent's tool show_how (in the page, no network) ----------
  await fresh();
  const t1 = await R('var t={links:[],chips:[],calls:[],cites:[]},ts=agTools("card",t),sh=ts.filter(function(x){return x.name==="show_how"})[0],r=sh?sh.execute({feature:"strip"}):null,bad=sh?sh.execute({feature:"history"}):null;' +
    '[!!sh,sh&&JSON.stringify(sh.inputSchema.properties.feature.enum),sh&&JSON.stringify(sh.inputSchema.required),r&&r.says,r&&r.does,JSON.stringify(t.shows),bad,AGTOOLS.show_how&&AGTOOLS.show_how.ro,S.agentCfg.pol.show_how,/how to use the app/.test(sh&&sh.description),/history/.test(sh&&sh.description),gdVM("strip").told]');
  ok('show_how: in the manifest (Auto, read only), its feature an enum of the 8 features taught, required; its result the feature\'s words; a Show me card added to the answer; the item seen; a wrong feature refused',
    t1[0] && t1[1] === '["card","view","menu","locate","navbar","ba","strip","pause"]' && t1[2] === '["feature"]' && t1[3] === 'Swipe a strip sideways to see more.' && t1[4] === 'Swipe the strips' && t1[5] === '["strip"]' && /^Error: unknown feature/.test(t1[6]) && t1[7] === true && t1[8] === 'auto' && t1[9] && t1[10] && t1[11] === 1, t1);
  await R('AG.mock={calls:[{name:"show_how",args:{feature:"pause"}}],text:"During the tour, tap pause."};S.agentCfg.provider="mock";S.guideCfg.ask="ai";applyProvider();' + past + 'gbGo("ask");ask("How do I pause?")'); await until('!!ph.querySelector(".gdhowc")', 3000);
  const t2 = await R('[agLive(),AG.mockSeen&&AG.mockSeen.tools.indexOf("show_how")>=0,/call show_how/.test(AG.mockSeen&&AG.mockSeen.prompt),!!ph.querySelector(".gdhowc"),G.agTurn&&JSON.stringify(G.agTurn.shows),G.agTurn&&G.agTurn.calls.map(function(c){return c.name+":"+c.ok}).join()]');
  await R('S.agentCfg.pol.show_how="off";if(G.on)stopTour();' + past + 'gbGo("ask");AG.mock={text:"ok"};ask("How do I pause?")'); await W(400);
  const t3 = await R('[AG.mockSeen.tools.indexOf("show_how")<0,!/call show_how/.test(AG.mockSeen.prompt)]');
  await R('AG.mock=null;S.agentCfg.provider="auto";S.agentCfg.pol.show_how="auto";S.guideCfg.ask="search";applyProvider()');
  ok('...with a test double as the live agent: the tool offered, the prompt says when to use it, its call puts the Show me card under the answer; policy Off: no tool, no rule', t2[0] && t2[1] && t2[2] && t2[3] && t2[4] === '["pause"]' && t2[5] === 'show_how:true' && t3.every(Boolean), [t2, t3]);

  lap('before 9');
  // ---------- 9. Settings › The guide's link; Fewer like this's toast; the rest as before ----------
  await fresh('S.view="card";S.stop="half";renderPhone(false);S.settingsOpen=true;renderPhone(false)');
  const st = await R('var v=ph.querySelector(".vset"),t=v&&v.querySelector(".vtips .vtry"),a=v&&v.querySelector(".vtips .vagain");[!!t,t&&t.textContent.indexOf("Things to try")===0,!!(a&&t&&a.compareDocumentPosition(t)&4),Math.round(t.getBoundingClientRect().height*ph.offsetHeight/ph.getBoundingClientRect().height),v.querySelectorAll("[data-gdpref]").length]');
  await p.locator('#ph .vset [data-gdpref="tips|off"]').click(); await W(150); const st2 = await R('[gdPrefs().tips,!!ph.querySelector(".vset .vnote")]'); await R('gdPrefs().tips="all";renderPhone(true)');
  ok('Settings › The guide: Things to try › after Show the tips again (44 px); Tips and its other rows as before (Off by a real click, its note)', st[0] && st[1] && st[2] && st[3] >= 44 && st[4] === 5 && st2[0] === 'off' && st2[1], [st, st2]);
  await fresh('G.gd.manual=true;S.gdVisitor.visit.lastEnd=null;var it=gdItems().filter(function(x){return x.id==="tapmarker"})[0];gdStart(it,"test")');
  await R('gdAskFewer()'); await p.locator('#ph [data-gdfewer="yes"]').click(); await W(150);
  const fa = await R('var t=ph.querySelector(".gdtoast");[!!t,t&&t.querySelector("span").firstChild.textContent,t&&(t.querySelector(".gdtoast-try")||{}).textContent,!G.gd.cur,!!S.gdVisitor.visit.fewer.start,gdVM("tapmarker").how]');
  await fresh('G.gd.manual=true;gdKindById("content").on=true;var it=gdItems().filter(function(x){return x.id==="tourof"})[0];gdStart(it,"test")');
  await R('gdAskFewer()'); await p.locator('#ph [data-gdfewer="yes"]').click(); await W(150);
  const fc = await R('var t=ph.querySelector(".gdtoast");[!!t,!(t&&t.querySelector(".gdtoast-try")),!!S.gdVisitor.visit.fewer.tours]');
  ok('Fewer like this on an App tip (Tap a marker): the toast and its line "You can always find these in Things to try."; on a Content item (A tour of …): the toast without it; each group rests for the visit as before',
    fa[0] && fa[1] === 'OK, fewer like this for now.' && fa[2] === 'You can always find these in Things to try.' && fa[3] && fa[4] && fa[5] === 'fewer' && fc.every(Boolean), [fa, fc]);

  lap('before 10');
  // ---------- 10. the Editor: Overall's row, the visitor's line, every new row's demo ----------
  await fresh('S.gdVisitor.used.card=gdNow();S.gdVisitor.used.ba=gdNow();S.gdVisitor.used.menu=gdNow();'); await editor({ gt0: true, gt4: true, 'gd:overall': true });
  const e1 = await R('[(ed.querySelector("#gdtryseen")||{}).textContent,!!ed.querySelector(\'[data-fo="gd:overall"] [data-set="@guidance.tryMenu"]\'),(ed.querySelector("#gdtrynow")||{}).textContent]');
  ok('Preview & Test › The visitor: "Things to try: 3 of 8 tried"; 1 · Overall › The visitor\'s controls › Things to try (Shown / Hidden) and its rows in words', /^Things to try: 3 of 8 tried/.test(e1[0]) && e1[1] && /^In the ✦ menu: Shown/.test(e1[2]) && /its 8 rows/.test(e1[2]), e1);
  await R('S.ui.rowDemo=false;gdChip(null)');
  const keys = ['guidance.tryMenu', 'guidance.items.tapmarker.inTry', 'guidance.items.askview.inTry', 'guidance.items.gbhint.inTry', 'guidance.items.label.inTry', 'guidance.items.navbar.inTry', 'guidance.items.ba.inTry', 'guidance.items.strip.inTry', 'guidance.items.pause.inTry', 'guidance.items.pause.on', 'agentCfg.pol.show_how'];
  const shown = [], miss = [];
  for (const k of keys) {
    await R('document.body.dataset.rowdemo=""'); await R(`showRowDemo("${k}")`);
    let v = ''; const t0 = Date.now(); while (Date.now() - t0 < 4000) { v = await R('document.body.dataset.rowdemo||""'); if (v) break; await W(60); }
    (/\|found\|/.test(v) ? shown : miss).push(k + ' ' + v.split('|').slice(1).join(' '));
  }
  ok('Every new row\'s demo shows its part on the phone (Things to try in the ✦ menu and the sheet; each row in or out; Pause the guide\'s Show me; show_how\'s scripted answer and card)', miss.length === 0 && shown.length === keys.length, { miss, shown });
  await R('if(G.on)stopTour();gdTryClose();gdChip(null)');
  await R(base + 'S.ui.rowDemo=true;S.guidance.tryMenu=true'); await editor({ gt4: true, 'gd:overall': true });
  await p.locator('[data-fo="gd:overall"] [data-set="@guidance.tryMenu"][data-v="false"]').click(); const r1 = await until('!!ph.querySelector(".gbmenu")&&!ph.querySelector(\'[data-gbgo="try"]\')', 3000);
  await p.locator('[data-fo="gd:overall"] [data-set="@guidance.tryMenu"][data-v="true"]').click(); const r2 = await until('!!ph.querySelector(".gdtry")', 4000);
  ok('...pressed by real clicks: Hidden, the ✦ menu without it; Shown, the menu with it, then the sheet', r1 && r2, [r1, r2]);
  await R('S.ui.rowDemo=false;gdTryClose();gdChip(null);S.guidance=JSON.parse(window.__g0)');
  ok('The words and their owners: Things to try\'s own (Overall), each feature\'s row and answer the item\'s that teaches it, in both languages; the Pause the guide row in App › The guide (pulled only)',
    await R('TXOWN.gd_try==="gd:overall"&&TXOWN.gd_fewer_try==="gd:overall"&&TXOWN.gd_try_ba==="gd:i:ba"&&TXOWN.gd_tryhow_pause==="gd:i:pause"&&TXOWN.gd_try_card==="gd:i:tapmarker"&&["card","view","menu","locate","navbar","ba","strip","pause"].every(function(f){return S.tx["gd_try_"+f].pt&&S.tx["gd_tryhow_"+f].pt})&&gdGroupById("guide").pulled===true&&gdItems().every(function(x){return x.id!=="pause"})&&gdDefs().some(function(x){return x.id==="pause"})'));

  lap('before 11');
  // ---------- 11. the four scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^gd9_/.test(x[1])})');
  ok('The console lists the four new scenes', scenes.length === 4 && JSON.stringify(scenes.map(s => s[1])) === JSON.stringify(['gd9_tryba', 'gd9_tipsoff', 'gd9_howpause', 'gd9_fewer']), scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 30000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`${id === 'gd9_tipsoff' ? '[tips off] ' : ''}Scene "${label}": Set up and ask runs it, ✓`, r && r.ok === true, r && [r.note, r.answer]);
    await R('gdStop("test");G.gd.rate=1;if(G.on)stopTour();gdTryClose()');
  }
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log((fails ? `${fails} FAILED` : 'ALL PASS') + ` (${Math.round((Date.now() - t00) / 1000)} s)`);
  await b.close();
})();
