// test9u (Playwright + the installed Edge): node test9u.js -> PASS/FAIL per check. Guidance round 17, Part B (the developer's plan section 5,
// approved 2026-11-11): a tour resumes on the next visit where the visitor stopped (guide/saved-place.js), and Tell me more, short or deep per
// step at the visitor's own pace (guide/tell-more.js, the deep parts made by guide/autotour.js), through the app's own controls.
// [deep] each step's deep part: the words its rule left out (a text's further sentences, the pictures' own captions), only the card's words
//   and the deep words; none where there is nothing deeper (the teaser, the end, a short story paragraph, the quiz).
// [facts] no fact twice with every deep part heard, the four points, EN and PT (the test's own fact keys); a card made for it: a deep
//   sentence whose date a later step says is left out.
// [minutes] the tour's length is the short version's (the hero and tourMinutes), the Editor's step list shows both.
// [more] Tell me more only where there is more (the chip), 44 px, inside the phone, clear of the panel; the panel's Next is Skip ahead; a
//   real tap: More coming, then the deep part said after the line, then the tour goes on.
// [pace] three Tell me more in a row: the next step's deep part comes by itself (More coming); three Skip ahead: lighter (no deep first);
//   Start deep for: Everyone starts deep. [edit] a written step's Tell me more typed in Specific Marker › 3 · Guide script is said.
// [ask] Ask Azu's "tell me more", typed (real keys) and spoken (the talk button's recogniser stand-in), does the same; the model is stubbed
//   (never live) and never asked for it; any other question still goes to the model.
// [offer] a tour stopped at a step, the page reloaded: the hero, the ✦ menu and A tour of {point}? offer "Continue the tour (step 5 of 7)"
//   and "Start again" (PT too). [continue] each way in, on a new visit (a reload): "Where we were: …" (the step before's own words, no new
//   fact), then that step; within the visit nothing is offered (round 11's Resume carries on).
// [again] Start again: the tour from its start, the place cleared, kept again from its next step. [finished] a tour played to its end
//   clears it. [expiry] Forget after by Preview & Test › Today is (the app's own clock, never a real wait): 7 days, 30, 1, Never.
// [changed] the card changed so the step is gone, or another source (written / from the card): no offer; the next start says so once.
// [blocked] storage blocked: no error, the tour and Tell me more work, the Editor says no place can be kept for the next visit.
// [rows] the Editor's rows: Remember Off, Tell me more Off, each row's ▸ showing its demo.
// Split in round 17 to keep each file under 300 s: [expiry], [changed], [rows] and [blocked] are test9ub.js (the same helpers).
// MUTATE=1: the recap gone and Continue from the start, a saved place never forgotten, the pace never deep, the fact rule off: the checks
//   named [continue], [expiry], [pace] and [facts] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  let ctx = await b.newContext({ viewport: { width: 1500, height: 1000 } }), p = await ctx.newPage();
  const errs = []; const watch = pg => pg.on('pageerror', e => errs.push(e.message)); watch(p);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 900) : '')); };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  const MUT = 'svRecapOf=function(){var o={};langCodes().forEach(function(l){o[l]=""});return o};svContinue=function(){startTour()};SV_DAYS={"1":0,"7":0,"30":0,"never":0};TM_ROW=99;atKnown=function(){return false};""';
  const load = async () => { await p.goto('about:blank'); await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await W(500); if (process.env.MUTATE) await R(MUT); };
  if (process.env.MUTATE) console.log('MUTATION: no recap and Continue from the start, a place never forgotten, the pace never deep, the fact rule off');
  const base = `SCREENS.phone=[390,844];if(G.on)stopTour();S.previewBar=false;S.hints.on=false;S.guidance.on=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.cardCfg.mode="light";
    S.guideCfg.voice="device";S.guideCfg.look="ink";S.ui.rowDemo=false;S.guideCfg.music=false;S.vis.music=null;S.vis.speed=2;S.agentCfg.voiceOut=false;G.gd.manual=true;S.guideCfg.dockStart="mini";gdPrefs().games="off";""`;
  const jump = async id => { await R(`var j=S.guideSteps.findIndex(function(s){return s.id==="${id}"});if(j>=0){stopSpeech();G.paused=false;G.i=j;runStep()}""`); await until(`S.guideSteps[G.i]&&S.guideSteps[G.i].id==="${id}"`, 3000); await W(250); };
  const openPin = async id => { await R('if(G.on)stopTour();setWall("ribeira");renderPhone(false);""'); await W(300); await p.click(`#ph [data-cpin="${id}"]`); await until(`S.m.poi==="${id}"&&S.view==="card"`, 3000); await R('S.stop="full";renderPhone(false);scr.scrollTop=0;""'); await W(300); };
  const heroStart = async () => { await R('scr.scrollTop=0;""'); await p.click('#ph .hero .gstart'); await until('G.on', 3000); await W(200); };
  const KEY = 'tilestories.tourPlace.';
  const visit = async (poi) => { await load(); await R(base); await openPin(poi || 'torreao'); };
  const kept = poi => R(`(function(){try{return localStorage.getItem("${KEY + poi}")||""}catch(e){return"blocked"}})()`);
  const words = t => (String(t).match(/\S+/g) || []).length, sents = t => String(t).trim().split(/(?<=[.!?…])\s+/).filter(Boolean), norm = s => String(s).toLowerCase().replace(/[^\p{L}\p{N}]+/gu, ' ').trim();
  await load(); await R(base);

  // ---------- 1. the deep parts as data: the four tours, EN and PT ----------
  const TOURS = JSON.parse(await R(`(function(){var out={};["painting","torreao","square","river"].forEach(function(poi){var p=pointOf(poi);out[poi]={};langCodes().forEach(function(l){var o=atOpts(poi,l),st=tourFromCard(p.card||[],p.story||[],l,o);
      out[poi][l]={name:o.name,have:" "+atNorm(atCardWords(p.card||[],p.story||[],p.hero,l)+" "+o.name+" "+atLinkWords(l))+" ",min:tourMinutes(poi),shortMin:atMins(atSecs(st.map(function(s){return s.text}),guideSpeed())),
        steps:st.map(function(s){return{id:s.id,act:s.act,kind:s.at&&s.at.kind||null,text:s.text,link:s.at&&s.at.link||"",deep:s.at&&s.at.deepText||"",cut:!!(s.at&&s.at.cut)}})}})});return JSON.stringify(out)})()`));
  const dp = (poi, l) => TOURS[poi][l].steps.filter(s => s.deep);
  const tor = TOURS.torreao.en.steps, torText = tor.filter(s => s.id === 'a:t_text')[0], torGal = tor.filter(s => s.id === 'a:t_gal')[0], ribGal = TOURS.painting.en.steps.filter(s => s.kind === 'Gallery')[0];
  const strays = []; for (const poi in TOURS) for (const l of ['en', 'pt']) dp(poi, l).forEach(s => norm(s.deep).split(' ').forEach(w => { if (w && !/^\d+$/.test(w) && TOURS[poi][l].have.indexOf(' ' + w + ' ') < 0) strays.push(poi + ':' + l + ':' + w); }));
  ok('[deep] each step\'s deep part is the words its rule left out: the Torreão\'s text its further sentence ("In the paintings of the river front…"), each gallery its pictures\' own captions ("In its pictures: …"), EN and PT; only the card\'s words and the deep words',
    !!torText && /^In the paintings of the river front, it is the tower with the dome/.test(torText.deep) && !!torGal && /^In its pictures: /.test(torGal.deep) && !!ribGal && /^In its pictures: a painting of the square/.test(ribGal.deep) && dp('torreao', 'pt').length === 2 && /^Nas suas imagens: /.test(dp('river', 'pt')[0].deep) && !strays.length,
    { torText: torText && torText.deep, torGal: torGal && torGal.deep, strays: strays.slice(0, 6) });
  const noDeep = []; for (const poi in TOURS) TOURS[poi].en.steps.forEach(s => { if (s.deep && (/^(teaser|end|open)$/.test(s.act) || s.kind === 'Knowledge Check' || s.kind === 'Timeline' || s.kind === 'Before and After')) noDeep.push(poi + ':' + s.id); });
  const ribPar = TOURS.painting.en.steps.filter(s => s.act === 'read' && /^a:r:/.test(s.id));
  ok('[deep] no deep part where there is nothing deeper: the teaser, the end, the quiz, a timeline, a before and after, and the Ribeira story\'s paragraphs (each said whole)', !noDeep.length && ribPar.length >= 3 && ribPar.every(s => !s.deep && !s.cut), { noDeep, par: ribPar.map(s => s.id + ':' + !!s.deep) });
  // the test's own fact keys (test9t's rule, written again): numbers, dates, decades, numbers in words, proper names
  const MON = { en: 'january february march april may june july august september october november december'.split(' '), pt: 'janeiro fevereiro março abril maio junho julho agosto setembro outubro novembro dezembro'.split(' ') };
  const TITLE = /^(king|queen|saint|st|rei|rainha|d|dom|dona|são)$/i, ART = /^(the|a|an|o|os|as|um|uma)$/i, JOIN = /^(do|da|dos|das|de|of)$/;
  const keysOf = (t, lang, proper, notName) => {
    const out = new Set(), mo = MON[lang], de = lang === 'pt' ? '\\s+de' : ''; let s = String(t);
    s = s.replace(new RegExp(`\\b(\\d{1,2})${de}\\s+(${mo.join('|')})${de}\\s+(\\d{4})\\b`, 'gi'), (m, d, mn, y) => { out.add(`date ${+d}.${mo.indexOf(mn.toLowerCase()) + 1}.${y}`); return '§'; });
    s = s.replace(new RegExp(`\\b(${mo.join('|')})${de}\\s+(\\d{4})\\b`, 'gi'), (m, mn, y) => { out.add(`month ${mo.indexOf(mn.toLowerCase()) + 1}.${y}`); return '§'; });
    s = s.replace(/\b(\d{3}0)s\b|\bdécada de (\d{3}0)\b/gi, (m, a, b2) => { out.add('decade ' + (a || b2)); return '§'; });
    s.replace(/\b\d{1,3}(?:[ ,\u00a0\u202f]\d{3})+\b|\b\d+\b/g, (m, at) => { const v = +m.replace(/\D/g, ''); if (v >= 1000 && v <= 2100) { if (!/\b(before|after|since|until|antes de|depois de|desde|até)\s+$/i.test(s.slice(0, at))) out.add('year ' + v); } else if (v >= 10) out.add('n ' + v); return m; });
    const toks = [...String(t).matchAll(/[\p{L}\p{N}]+(?:['’-][\p{L}\p{N}]+)*/gu)].map(m => ({ w: m[0], at: m.index }));
    const atStart = k => { const b2 = String(t).slice(0, toks[k].at).replace(/["“'«(\s]+$/, ''); return !b2 || /[.!?…]$/.test(b2) && !/\b(D|St)\.$/.test(b2); };
    const cap = k => /^\p{Lu}/u.test(toks[k].w) && MON[lang].indexOf(toks[k].w.toLowerCase()) < 0 && (!atStart(k) || proper.has(toks[k].w));
    for (let a = 0; a < toks.length; a++) { if (!cap(a)) continue; const run = [toks[a].w]; let e = a + 1;
      for (; e < toks.length; e++) { const y = toks[e].w; if (cap(e) || /^[IVX]+$/.test(y)) { run.push(y); continue; } if (JOIN.test(y) && toks[e + 1] && cap(e + 1)) { run.push(y); continue; } break; }
      a = e - 1; while (run.length && (TITLE.test(run[0]) || ART.test(run[0]))) run.shift(); while (run.length && JOIN.test(run[run.length - 1])) run.pop();
      if (!run.length || run.length === 1 && run[0].length < 2) continue; const k = run.join(' ').toLowerCase();
      if (notName && k.split(' ').every(x => notName.indexOf(x) >= 0)) continue; out.add('name ' + k); }
    return [...out];
  };
  const known = (set, k) => set.has(k) || (/^year /.test(k) && [...set].some(x => /^(date|month) /.test(x) && x.endsWith('.' + k.slice(5))));
  // every deep part heard: each step's line, then its deep part; each sentence (a list's item) with a fact brings one not said before
  const deepJudge = (T, lang) => {
    const proper = new Set(); T.steps.forEach(s => sents(s.text + ' ' + s.deep).forEach(x => (x.match(/[\p{L}][\p{L}'’-]*/gu) || []).forEach((w, i) => { if (i && /^\p{Lu}/u.test(w)) proper.add(w); })));
    const nameW = T.name.toLowerCase().split(/\s+/).filter(w => !ART.test(w)), said = new Set(keysOf(T.name, lang, new Set(T.name.split(/\s+/)), null)), twice = [], seen = {};
    T.steps.forEach((s, i) => { const item = s.kind === 'Quick Facts' || s.kind === 'Timeline', tx = s.link && s.text.indexOf(s.link) === 0 ? s.text.slice(s.link.length).trim() : s.text;
      [[tx, item], [s.deep, s.kind === 'Gallery' || s.kind === 'Quick Facts']].forEach(([t, it]) => sents(t).forEach(x => { const n = norm(x); if (n && seen[n] && i > 0) twice.push('sentence: ' + x); seen[n] = 1;
        (it ? x.split(/[;,]\s+/) : [x]).forEach(u => { const k = keysOf(u, lang, proper, it ? null : nameW); if (i > 0 && k.length && k.every(q => known(said, q))) twice.push(u + ' ' + JSON.stringify(k)); k.forEach(q => said.add(q)); }); })); });
    return twice;
  };
  const dj = {}; for (const poi in TOURS) for (const l of ['en', 'pt']) dj[poi + ':' + l] = deepJudge(TOURS[poi][l], l);
  // a card made for the rule: a text whose third sentence says the date a later timeline says: that sentence is no deep part
  const made = JSON.parse(await R(`(function(){var c=[pointBlock("x_txt","Inline",{title:T("The tower","A torre"),text:T("The tower rose on the river front. It held the royal archive for many years. It burned on 1 November 1755, with the palace.","A torre ergueu-se na frente ribeirinha. Guardou o arquivo real durante muitos anos. Ardeu a 1 de novembro de 1755, com o paço.")}),
    pointBlock("x_tl","Timeline",{events:[["1755",T("The tower burns","A torre arde")]]})],o=Object.assign(atOpts("painting","en"),{cap:8,line:""}),st=tourFromCard(c,[],"en",o),t=st.filter(function(s){return s.at&&s.at.block==="x_txt"})[0],tl=st.filter(function(s){return s.at&&s.at.block==="x_tl"})[0];
    return JSON.stringify({deep:t&&t.at.deepText||"",tl:tl&&tl.text||""})})()`));
  ok('[facts] no fact twice with every deep part heard (each line, then its deep part), the four points, EN and PT, by the test\'s own fact keys; a card made for it: the deep sentence with the date a later step says is left out ("It held the royal archive…" stays)',
    Object.values(dj).every(x => !x.length) && /royal archive/.test(made.deep) && !/1755/.test(made.deep) && /1755/.test(made.tl), { dj, made });
  const listNote = await R('S.tab="guide";S.op={};S.op.gt1=true;S.op.s7auto=true;S.ui.level=3;pointOpen("torreao");S.view="card";S.stop="full";renderAll(false);var n=[].filter.call(ed.querySelectorAll(\'[data-fo="s7auto"] .note\'),function(x){return/^The tour made from/.test(x.textContent)})[0];var m=ed.querySelectorAll(\'[data-fo="s7auto"] .attbl .atmore\');JSON.stringify({note:n?n.textContent:"",more:[].map.call(m,function(x){return x.textContent}),len:[].map.call(ed.querySelectorAll(\'[data-fo="s7auto"] .attbl .atmorelen\'),function(x){return x.textContent}),hero:(scr.querySelector(".hero .gstart")||{}).textContent})');
  const ln = JSON.parse(listNote), tm = TOURS.torreao.en;
  ok('[minutes] the tour\'s length is the short version\'s (tourMinutes = the short lines\' own sum, the hero\'s "· short"); the Editor\'s step list shows both lengths and each deep part under its step',
    tm.min === tm.shortMin && /· short$/.test(ln.hero) && /with every Tell me more \(2 steps\) about \d+ min/.test(ln.note) && ln.more.length === 2 && ln.len.length === 2 && /^\+\d+ w · \d+ s$/.test(ln.len[0]), { min: tm.min, short: tm.shortMin, ln });
  await R('pointOpen("painting");S.tab="card";renderAll(false);""');
  tsec('deep');

  // ---------- 2. Tell me more on the phone: the Torreão, through its pin, its Guide button, the chip and the panel ----------
  await openPin('torreao'); await heroStart();
  await jump('a:t_facts'); await until('G.tpText===L(S.guideSteps[G.i].text)', 3000); await W(300);
  const c0 = await R('!!ph.querySelector(".gtmore")');
  await R('S.dockH="peek";renderGuide();""'); await W(300);
  const nx = await R('(ph.querySelector(\'.gdock [data-g="next"]\')||{getAttribute:function(){return""}}).getAttribute("aria-label")');
  await R('S.dockH="mini";renderGuide();""'); await jump('a:t_text'); await until('!!ph.querySelector(".gtmore")', 3000); await W(400);
  const c1 = JSON.parse(await R('var c=ph.querySelector(".gtmore"),r=c.getBoundingClientRect(),pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,d=ph.querySelector(".gdock").getBoundingClientRect();JSON.stringify({t:c.textContent,h:r.height/q,inside:r.left>=pr.left&&r.right<=pr.right&&r.top>=pr.top&&r.bottom<=pr.bottom,clear:r.bottom<=d.top+0.5,pressed:c.getAttribute("aria-pressed")})'));
  ok('[more] Tell me more only where there is more: none at the facts step, the chip at the text step ("Tell me more", 44 px, inside the phone, above the panel); the panel\'s Next is "Skip ahead"', !c0 && c1.t === 'Tell me more' && c1.h >= 43.5 && c1.inside && c1.clear && nx === 'Skip ahead', { c0, c1, nx });
  await p.click('#ph .gtmore'); await W(150);
  const q1 = await R('var c=ph.querySelector(".gtmore");c?c.textContent+"|"+c.getAttribute("aria-pressed"):"gone:"+G.tm.playing');
  await until('G.tm.playing', 15000);
  const d1 = JSON.parse(await R('JSON.stringify({tp:G.tpText,i:G.i,id:S.guideSteps[G.i].id,chip:!!ph.querySelector(".gtmore")})'));
  await until('G.i>2&&!G.tm.playing', 15000);
  const d2 = await R('S.guideSteps[G.i].id');
  ok('[more] a real tap: "More coming" (pressed), then after the step\'s line its deep part is said (the panel\'s words, no chip while it plays), then the tour goes on to the next step', (/^More coming\|true$/.test(q1) || /^gone:true/.test(q1)) && d1.tp === torText.deep && d1.id === 'a:t_text' && !d1.chip && d2 === 'a:t_time', { q1, d1, d2 });
  await R('stopTour();""'); tsec('more');

  // ---------- 3. the pace, and a written step's Tell me more typed in the Editor (the Ribeira's written tour) ----------
  await R('pointOpen("painting");S.tours.painting.src="written";S.tab="marker";S.op={};S.op.m3=true;S.ui.level=3;S.view="card";S.stop="full";renderAll(false);""'); await W(300);
  const MORE = { g1: 'Royal home for so long, it shaped the whole riverside.', g3: 'The story starts with a king who wanted to live by the water.', g4: 'The docks were full of ships and warehouses then.', g5: 'Visitors came from all over Europe to see it.', g8: 'Each picture shows the square from a different side.', g9: 'Take your time before you answer.' };
  for (const id of Object.keys(MORE)) {
    await p.click(`[data-fo="m3"] tr.pick[data-pick="gstep"][data-id="${id}"] td:nth-child(2)`); await W(150);
    const ta = p.locator(`[data-gmore="${id}|en"]`); if (id === 'g1') { await ta.click(); await p.keyboard.type(MORE[id]); } else await ta.fill(MORE[id]);
  }
  const typed = await R('JSON.stringify(["g1","g4","g9"].map(function(id){var s=byId(S.guideSteps,id);return s&&s.more&&s.more.en}))');
  await R('S.tab="guide";S.view="card";S.stop="full";renderAll(false);scr.scrollTop=0;""'); await W(300);
  await heroStart();
  const tap = async id => { await jump(id); await until('!!ph.querySelector(".gtmore")', 3000); await p.click('#ph .gtmore'); await until('G.tm.playing', 15000); const t = await R('G.tpText'); await until(`!G.tm.playing&&S.guideSteps[G.i].id!=="${id}"`, 15000); return t; };
  const g1 = await tap('g1'); await tap('g3'); await tap('g4');
  const pace1 = await R('G.tm.pace');
  await jump('g5'); await W(200);
  const g5 = JSON.parse(await R('var c=ph.querySelector(".gtmore");JSON.stringify({t:c?c.textContent:"",p:c?c.getAttribute("aria-pressed"):""})'));
  const g5play = await until('G.tm.playing&&S.guideSteps[G.i].id==="g5"', 15000);
  ok('[edit] a written step\'s Tell me more typed in Specific Marker › 3 · Guide script (its own box, real keys) is said after its line', typed.indexOf(MORE.g1) >= 0 && g1 === MORE.g1, { typed, g1 });
  ok('[pace] three Tell me more in a row (real taps): the pace is deep, the next step offers it first ("More coming") and its deep part comes by itself after its line', pace1 === 'deep' && g5.t === 'More coming' && g5.p === 'true' && g5play, { pace1, g5, g5play });
  const skip = async () => { await R('S.dockH="peek";renderGuide();""'); await W(150); await p.click('#ph .gdock [data-g="next"]'); await W(150); await R('S.dockH="mini";renderGuide();""'); };
  await skip();
  await jump('g7'); await until('G.tm.playing', 15000); await skip();
  await until('S.guideSteps[G.i].id==="g8"', 3000); await until('G.tm.playing', 15000); await skip();
  const pace2 = await R('G.tm.pace'); await until('S.guideSteps[G.i].id==="g9"', 3000); await W(300);
  const g9 = JSON.parse(await R('var c=ph.querySelector(".gtmore");JSON.stringify({t:c?c.textContent:"",p:c?c.getAttribute("aria-pressed"):"",log:G.tm.log.slice(-4)})'));
  ok('[pace] three Skip ahead in a row (the panel\'s Next, real taps, during the deep parts): the tour is lighter, the next step offers Tell me more again, not first', pace2 === 'short' && g9.t === 'Tell me more' && g9.p === 'false', { pace2, g9 });
  await R('stopTour();S.guideCfg.tmStart="all";""'); await heroStart(); await jump('g1');
  const deepStart = await until('G.tm.playing&&S.guideSteps[G.i].id==="g1"', 15000);
  await R('stopTour();S.guideCfg.tmStart="none";""');
  ok('[pace] Start deep for: Everyone: a tour starts deep (the first step with more says it by itself)', deepStart);
  tsec('pace');

  // ---------- 4. Ask Azu's "tell me more": typed and spoken; the model stubbed, never asked for it ----------
  await R('window.__calls=0;sampleFn=function(){window.__calls++;return Promise.resolve({text:"A stub answer."})};S.guideCfg.ask="ai";""');
  await openPin('torreao'); await heroStart(); await jump('a:t_text'); await until('!!ph.querySelector(".gtmore")', 3000);
  await R('S.dockH="peek";G.typing=true;renderGuide();""'); await W(250);
  await p.click('#ph #gq'); await p.keyboard.type('tell me more'); await p.keyboard.press('Enter');
  const ak = await until('G.tm.playing', 15000), ak1 = JSON.parse(await R('JSON.stringify({calls:window.__calls,tp:G.tpText})'));
  await until('!G.tm.playing&&S.guideSteps[G.i].id!=="a:t_text"', 15000);
  await jump('a:t_gal'); await until('!!ph.querySelector(".gtmore")', 3000);
  await R('heard("Tell me more");""'); const sp = await until('G.tm.playing', 15000), sp1 = JSON.parse(await R('JSON.stringify({calls:window.__calls,tp:G.tpText})'));
  await until('!G.tm.playing', 15000); await R('stopTour();""');
  await R('S.view="card";S.stop="full";renderPhone(false);startTour();pauseTour();G.askOpen=true;S.dockH="full";renderGuide();""'); await W(250);
  await p.click('#ph #gq'); await p.keyboard.type('Who built the tower?'); await p.keyboard.press('Enter');
  const asked = await until('window.__calls>=1', 5000); await R('stopTour();""');
  ok('[ask] Ask Azu\'s "tell me more" typed (real keys in the panel\'s box) and spoken (the talk button\'s recogniser stand-in) says the step\'s deep part; the model (stubbed) is never asked for it; another question still goes to the model',
    ak && ak1.calls === 0 && ak1.tp === torText.deep && sp && sp1.calls === 0 && sp1.tp === torGal.deep && asked, { ak, ak1, sp, sp1, asked });
  tsec('ask');

  // ---------- 5. the saved place: a tour stopped at step 5, the page reloaded, the three ways in ----------
  await load(); await R(base);
  await openPin('torreao'); await heroStart();
  for (let k = 0; k < 4; k++) { const i = await R('G.i'); await R('S.dockH="peek";renderGuide();""'); await p.click('#ph .gdock [data-g="next"]'); await until(`G.i!==${i}`, 3000); }
  const at5 = await R('S.guideSteps[G.i].id'); await R('S.dockH="peek";renderGuide();""'); await p.click('#ph .gdock [data-g="stop"]'); await until('!G.on', 3000);
  const k5 = JSON.parse((await kept('torreao')) || '{}');
  await load(); await R(base); await openPin('torreao');
  const of = JSON.parse(await R('JSON.stringify({cont:(scr.querySelector(".hero .gcont")||{}).textContent||"",again:(scr.querySelector(".hero .gstart")||{}).textContent||"",n:svTour().length})'));
  await R('S.lang="pt";renderPhone(false);""'); const ofPt = await R('(scr.querySelector(".hero .gcont")||{}).textContent||""'); await R('S.lang="en";renderPhone(false);""');
  await R('scr.scrollTop=900;fabLayer();""'); await W(500); await R('fabLayer();""'); await until('!!ph.querySelector(".gbtn")', 3000);
  await p.click('#ph .gbtn'); await until('!!ph.querySelector(".gbmenu")', 2000);
  const menu = await R('JSON.stringify([].map.call(ph.querySelectorAll(".gbmenu [data-gbgo]"),function(b){return b.dataset.gbgo+":"+b.textContent}))');
  await R('gbMenu(false);""');
  await R('S.guidance.on=true;setWall("ribeira");S.guidance.kinds.forEach(function(k){k.on=k.id==="content"});gdResetVisitor();gdSimAim("torreao");renderPhone(false);gdStop("test");G.gd.cur=null;gdStart(gdItemNow("tourof"),"test",{pull:true,ms:60000});""');
  await until('!!ph.querySelector(".gdbub:not(.gd-out) [data-gdbub]")', 3000); await W(300);
  const inv = JSON.parse(await R('JSON.stringify({t:(ph.querySelector(".gdbub:not(.gd-out) [data-gdbub]")||{}).textContent||"",cta:[].map.call(ph.querySelectorAll(".gdbub:not(.gd-out) [data-gdcta]"),function(b){return b.textContent})})'));
  const nb = '\u00a0';
  ok('[offer] a tour stopped at step 5 by the panel\'s Stop (real taps), the page reloaded: the hero, the ✦ menu (a real tap) and A tour of {point}? offer "Continue the tour (step 5 of 7)" and "Start again", in Portuguese "Continuar a visita (passo 5 de 7)"',
    k5.id === at5 && of.cont === '▶ Continue the tour (step' + nb + '5 of' + nb + of.n + ')' && of.again === 'Start again' && ofPt === '▶ Continuar a visita (passo' + nb + '5 de' + nb + of.n + ')' &&
    menu.indexOf('tour:Continue the tour (step' + nb + '5 of' + nb + of.n + ')') >= 0 && menu.indexOf('again:Start again') >= 0 && inv.t === 'Continue the tour of the Torreão (step' + nb + '5 of' + nb + of.n + ')?' && inv.cta.join() === 'Start again›', { k5, at5, of, ofPt, menu, inv });
  // [continue] each way in: the recap from the step before (its own words, no new fact), then that step
  const prevTxt = (TOURS.torreao.en.steps.filter((s, i, a) => a[i + 1] && a[i + 1].id === k5.id)[0] || {}).text || '';
  const before = TOURS.torreao.en.steps.slice(0, TOURS.torreao.en.steps.findIndex(s => s.id === k5.id)).map(s => s.text).join(' ');
  const recapCheck = async how => { await until('G.on&&!!S.guideSteps[G.i]', 3000); const r = JSON.parse(await R('var s=S.guideSteps[G.i];JSON.stringify({act:s.act,t:L(s.text),next:(S.guideSteps[G.i+1]||{}).id,poi:S.m.poi})'));
    const body = r.t.replace(/^Where we were: /, '').replace(/[.!?…]$/, ''), kb = new Set(); keysOf(before, 'en', new Set(), null).forEach(k => kb.add(k));
    const fresh = keysOf(body, 'en', new Set(), null).filter(k => !known(kb, k));
    const thenOk = await until(`S.guideSteps[G.i]&&S.guideSteps[G.i].id==="${k5.id}"`, 15000);
    await R('S.dockH="peek";renderGuide();""'); await p.click('#ph .gdock [data-g="stop"]'); await until('!G.on', 3000);
    return { how, ok: r.act === 'recap' && /^Where we were: /.test(r.t) && norm(prevTxt).indexOf(norm(body)) >= 0 && !fresh.length && r.next === k5.id && thenOk && r.poi === 'torreao', r, fresh };
  };
  // within a visit Stop keeps round 11's Resume (the sentence); the place is offered on the next visit: a reload before each way in
  const sameVisit = await R(`gdStop("test");G.gd.cur=null;S.guidance.on=false;S.view="card";S.stop="full";startTour({id:"${k5.id}"});stopTour();renderPhone(false);!!scr.querySelector(".hero .gcont")`);
  await visit();
  await R('S.view="card";S.stop="full";renderPhone(false);scr.scrollTop=900;fabLayer();""'); await W(500); await R('fabLayer();""'); await until('!!ph.querySelector(".gbtn")', 3000);
  await p.click('#ph .gbtn'); await until('!!ph.querySelector(".gbmenu")', 2000); await p.click('#ph .gbmenu [data-gbgo="tour"]'); const cv3 = await recapCheck('the ✦ menu');
  await visit(); await W(300);
  await p.click('#ph .hero .gcont'); const cv2 = await recapCheck('the hero');
  await visit(); await R('S.guidance.on=true;setWall("ribeira");gdResetVisitor();gdSimAim("torreao");renderPhone(false);gdStop("test");G.gd.cur=null;gdStart(gdItemNow("tourof"),"test",{pull:true,ms:60000});""');
  await until('!!ph.querySelector(".gdbub:not(.gd-out) [data-gdbub]")', 3000); await W(300);
  await p.click('#ph .gdbub:not(.gd-out) [data-gdbub]'); const cv1 = await recapCheck('the invitation'); await R('S.guidance.on=false;""');
  ok('[continue] Continue from each way in, each on a new visit (real taps: the ✦ menu, the hero, the invitation): "Where we were: …" first (one sentence of the step before, no new fact), then the step it stopped at; within the visit nothing is offered (Stop\'s Resume the tour carries on)', cv1.ok && cv2.ok && cv3.ok && !sameVisit, [cv1, cv2, cv3, sameVisit]);
  tsec('offer');
  // [again] Start again; [finished] a tour to its end
  await visit(); await W(300);
  await p.click('#ph .hero .gstart'); await until('G.on', 3000); await W(200);
  const ag = JSON.parse(await R('JSON.stringify({i:G.i,act:S.guideSteps[G.i].act})')), agK = await kept('torreao');
  await R('S.dockH="peek";renderGuide();""'); await p.click('#ph .gdock [data-g="next"]'); await until('G.i===1', 3000); await W(200);
  const agK2 = JSON.parse((await kept('torreao')) || '{}'), id2 = await R('S.guideSteps[1].id');
  ok('[again] Start again (a real tap on the hero): the tour from its teaser, the old place cleared at once, a new one kept from its next step', ag.i === 0 && ag.act === 'teaser' && agK === '' && agK2.id === id2, { ag, agK, agK2, id2 });
  for (let k = 0; k < 12; k++) { if (!(await R('G.on&&S.guideSteps[G.i].act!=="end"'))) break; const i = await R('G.i'); await R('S.dockH="peek";renderGuide();""'); await p.click('#ph .gdock [data-g="next"]'); await until(`G.i!==${i}||!G.on`, 3000); }
  const endK = await kept('torreao'), endAct = await R('G.on?S.guideSteps[G.i].act:"off"'); await R('stopTour();""');
  ok('[finished] a tour played to its end (the panel\'s Skip ahead, real taps) clears its place: no offer on the hero', endAct === 'end' && endK === '' && !(await R('!!(renderPhone(false),scr.querySelector(".hero .gcont"))')), { endAct, endK });

  ok('no page errors', errs.length === 0, errs.slice(0, 5));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS'); console.log('TIME ' + Math.round((Date.now() - T0) / 1000) + ' s');
  await b.close(); process.exit(fails ? 1 : 0);
})();
