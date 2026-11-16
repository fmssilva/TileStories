// test9x (Playwright + the installed Edge): node test9x.js -> PASS/FAIL per check. Guidance round 19: one mascot, one voice
// (guide/mascot-profile.js, guide/voice-script.js, guide/hero-start.js, guide/voice.js's default set, editor/guide-tab.js's 1 · The mascot,
// guidance/present.js's beat card at the hero, the tail on the head and Guess the year's time, card/locate.js's room for its label).
// [settings] 1 · The mascot's foldouts in order (Who, Look, Voice, Speech on screen, Manner, Moments), each row's ▸ demo by a real click.
// [name] a new name (typed in Who › Name, real keys) shows everywhere the visitor reads it, EN and PT (the panel, its ask field, a game's
//   card, the ✦'s name, the first introduction, Ask about the view's pill), and nowhere in the Editor does "Azu" remain (the Guide tab's
//   text and its help, every foldout open).
// [voice] the default set is Ryan · Duarte by its id, not by its count; every line of the script for the Ribeira Palace has a recording,
//   the Coverage list empty but for the live lines; the first introduction and the quiz promise are said by the recorded source
//   (G.sayNow.src; the audio stubbed: play() resolves, "ended" follows).
// [audit] no module but guide/voice.js touches the device voice or picks a voice (speechSynthesis, getVoices, a voice's name or id).
// [intro] the new words, warm (no "Now", no "tap Ask"), EN and PT, about 8 s (its recording), naming a control on the screen at that moment.
// [promise] the quiz promise's card covers neither the hero's title nor its line (the card's top below the line's bottom), EN and PT.
// [tap here] the wall's Tap here.: its tail's tip meets the mascot's head (within 4 px, the test's own measure of the figure), the mascot
//   pointing at the marker. [card] the Ribeira Palace opened by a real tap on its marker at 390 px: the mascot in sight beside Start the
//   guide, left of it, nothing over anything (the hero's words, the mascot, Start, Find it on the wall, the pin), each text whole, each
//   target 44 px; Start's glow once the card has settled, gone after a real tap. [guess] Guess the year waits 14 s, 18 s the first time in a
//   visit; its two Editor rows. [reduced motion] the glow without its pulse; the mascot beside the marker with no hop.
// MUTATE=1: the set picked by its count, the old introduction, no room for the hero's words under a beat card, no mascot at the hero, the
//   bubble's tail off the head, Guess the year at 8 s: the checks named [voice], [intro], [promise], [card], [tap here] and [guess] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 700) : '')); };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  await until('voiceSets().length>0', 8000);
  const MUT = 'currentSet=function(){var s=voiceSets(),n=function(x){return Object.keys(x.files||{}).length};return s.slice().sort(function(a,b){return n(b)-n(a)})[0]||null};' +
    'S.tx.guide_intro={en:"Hi! I\'m {name}, and I\'ll be your guide. Ask me anything, at any time: just tap Ask. Now, let\'s meet {0}!",pt:"Olá! Sou o {name} e vou ser o seu guia. Basta tocar em Perguntar. Agora, vamos conhecer {0}!"};' +
    'freeHeight=function(){var box=scr.getBoundingClientRect(),lo=box.bottom;[".gcap",".gbar",".gdock",".gbubble",".gbtn-wrap.intour",".gtmore"].forEach(function(s){var e=ph.querySelector(s);if(e){var r=e.getBoundingClientRect();if(r.height)lo=Math.min(lo,r.top)}});return{top:box.top,bottom:lo}};heroWords=function(){return null};hsOn=function(){return false};GD_TIP=-40;gdGuessWait=function(){return 8};""';
  if (process.env.MUTATE) { await R(MUT); console.log('MUTATION: the set by its count, the old introduction, no room under a beat card, no mascot at the hero, the tail off the head, the guess at 8 s'); }
  /* the audio stubbed: a recorded line "plays" at once and ends 300 ms later (no sound, no file read) */
  await R('HTMLMediaElement.prototype.play=function(){var a=this;clearTimeout(a._stubT);a._stubT=setTimeout(function(){try{a.dispatchEvent(new Event("ended"))}catch(e){}},window.__stubMs||300);return Promise.resolve()};""');
  const base = (o) => `SCREENS.phone=[390,844];if(G.on)stopTour();gdBeatClose();if(G.gd.sc12)gd12Done([true,"",""]);gdStop("test");S.hints.on=false;S.previewBar=false;S.lang="${o && o.lang || 'en'}";S.guideMute=true;S.introSeen=true;S.reduce=${!!(o && o.reduce)};S.dark=false;S.cardCfg.mode="light";S.ui.rowDemo=false;S.ui.gdSR=false;S.vis.speed=null;S.vis.aud="explorer";S.guideCfg.voice="recorded";S.guideCfg.panel="dock";S.guideCfg.look="ink";S.guideCfg.dockStart="mini";S.guideCfg.mascot=true;S.guideCfg.gbtn="both";S.guidance.on=false;`;
  const box = async sel => p.locator('#ph ' + sel).first().boundingBox({ timeout: 2500 }).catch(() => null);
  const tapEl = async sel => { const r = await box(sel); if (!r) return false; await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return true; };

  // ---------- [settings] 1 · The mascot: the foldouts in order, each row's ▸ ----------
  await R('S.tab="guide";S.ui.level=2;S.op={};S.op.s7mascot=true;renderEditor();""');
  const fos = await R('[].map.call(ed.querySelectorAll(\'[data-fo="s7mascot"] > .inner > details[data-fo]\'),function(d){return d.dataset.fo+"|"+d.querySelector("summary").textContent.split(" · ")[0]})');
  ok('[settings] 1 · The mascot holds its global settings, one foldout each, in this order: Who, Look, Voice, Speech on screen, Manner, Moments',
    JSON.stringify(fos) === JSON.stringify(['s7who|Who', 'gt:look|Look', 'gt:voice|Voice', 'gt:speech|Speech on screen', 'gt:manner|Manner', 'gt:moments|Moments']), fos);
  const want = { s7who: ['Name', 'First introduction', 'Where the name shows'], 'gt:voice': ['The voice', 'Sources', 'Speed', 'A line with no recording', 'Voice in this browser', 'Voice of the answers', 'Coverage', 'Speaking now'],
    'gt:speech': ['Highlight what is read', 'Highlight style', 'Trail', 'Glide', 'Words during a tour', 'Its height', "A tip's bubble", "The panels' look", 'Try'], 'gt:manner': ['Size', 'Liveliness', 'Mouth', 'How it moves', 'Moods', 'Tap on the mascot', 'Try'], 'gt:moments': ['Introduction', 'Walks to the wall', 'Try'] };
  const rows = await R(`var W=${JSON.stringify(want)},o={};Object.keys(W).forEach(function(id){var f=ed.querySelector('[data-fo="'+id+'"] > .inner');o[id]=f?[].map.call(f.querySelectorAll(":scope > .row > label"),function(l){return l.firstChild?l.firstChild.textContent:""}):null});o`);
  ok('[settings] ...each foldout holds its rows (the voice and the highlight borrowed from Narration, Ask and Captions)', Object.keys(want).every(k => JSON.stringify(rows[k]) === JSON.stringify(want[k])), rows);
  const links = await R('[!!rowEl(ed,"s7voice","Voice and speed"),!!rowEl(ed,"s7ask","The answers\' voice"),!!rowEl(ed,"s7cap","Highlight"),!rowEl(ed,"s7voice","Sources"),!rowEl(ed,"s7cap","Highlight style")]');
  ok('[settings] ...where the rows were: one link row each (Narration, Ask\'s answers, Captions), the rows themselves gone from there', links.every(Boolean), links);
  /* every row of the section: its ▸ (the label's, or the row's own) shows its demo on the phone */
  await R('S.ui.level=9;S.op={};renderEditor();""');
  const demos = await R('[].map.call(ed.querySelectorAll(\'[data-fo="s7mascot"] .row\'),function(r,i){var l=r.querySelector("label"),k=rowKey(r),pl=r.querySelector("[data-mpdemo],[data-vsdemo],.atplay");return{i:i,label:l?l.textContent:"",key:k,rd:!!(l&&l.classList.contains("rd")),play:!!pl}})');
  const noPlay = demos.filter(d => !d.rd && !d.play && !/^(Try|Import…|Generate from a picture|Play a clip|3D model)$/.test(d.label));
  ok('[settings] ...every row has its ▸ demo (a pressable label, or a ▸ of its own) and its ? help', noPlay.length === 0 && await R('[].every.call(ed.querySelectorAll(\'[data-fo="s7mascot"] .row\'),function(r){return!!r.querySelector(".help")||/Try|Import|Generate|Play a clip|3D model/.test(r.textContent)})'), { noPlay, n: demos.length });
  const pressed = [];
  for (const d of demos.filter(x => x.rd || x.play).slice(0, 40)) {
    await R('document.body.dataset.rowdemo="";if(G.on)stopTour();""');
    const sel = `[data-fo="s7mascot"] .row >> nth=${d.i}`;
    const row = p.locator('#ed [data-fo="s7mascot"] .row').nth(d.i);
    await row.scrollIntoViewIfNeeded().catch(() => {});
    if (d.rd) await row.locator('label').first().click({ timeout: 2000 }).catch(() => {}); else await row.locator('[data-mpdemo],[data-vsdemo],.atplay').first().click({ timeout: 2000 }).catch(() => {});
    const got = await until('!!document.body.dataset.rowdemo', 3500);
    pressed.push([d.label, got ? await R('document.body.dataset.rowdemo.split("|").slice(0,2).join("|")') : 'none']);
  }
  ok('[settings] ...each ▸ pressed by a real click shows its demo on the phone (the part ringed)', pressed.length >= 15 && pressed.every(x => /\|found$/.test(x[1])), pressed.filter(x => !/\|found$/.test(x[1])).concat([pressed.length]));
  await R('if(G.on)stopTour();""');
  tsec('settings');

  // ---------- [name] a new name everywhere the visitor reads it, EN and PT; "Azu" nowhere in the Editor ----------
  await R('S.tab="guide";S.ui.level=2;S.op={s7mascot:true,s7who:true};renderEditor();""');
  for (const l of ['en', 'pt']) { const i = p.locator(`#ed [data-tx="mascot_called|${l}"]`).first(); await i.scrollIntoViewIfNeeded(); await i.click({ clickCount: 3 }); await p.keyboard.type('Lumi'); await i.dispatchEvent('change'); }
  const nm = {};
  for (const l of ['en', 'pt']) {
    await R(base({ lang: l }) + 'S.guideCfg.look="paper";S.view="card";S.stop="full";renderPhone(false);startTour();""'); await W(400);
    const mini = await R('(ph.querySelector(".gd-stop")||{}).textContent||""');
    await R('S.dockH="peek";renderGuide();""'); await W(200);
    const open = await R('[(ph.querySelector(".gd-who b")||{}).textContent||"",(ph.querySelector("#gq")||{}).placeholder||""]');
    await R('stopTour();S.guideCfg.look="player";S.view="card";renderPhone(false);startTour();S.dockH="peek";renderGuide();""'); await W(200);
    const eye = await R('(ph.querySelector(".gd-eye")||{}).textContent||""');
    await R(base({ lang: l }) + 'S.guidance.on=true;gd12Keep();gd12Only(["quizpromise","quizfinale"]);S.introSeen=false;S.view="card";S.stop="full";renderPhone(false);startTour();""');
    await until('gd12Kind("promise")', 30000);
    const beat = await R('(ph.querySelector(".gdbeat .bt-eye")||{}).textContent||""');
    await R(base({ lang: l }) + 'gd12Keep();setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);""'); await W(200);
    const fab = await R('[(ph.querySelector(".azfab")||{getAttribute:function(){return""}}).getAttribute("aria-label"),TXT("cam_ask"),introLine(S.lang,"x","intro_ask_arrow"),TXT("te_len_view")]');
    nm[l] = { mini, open, eye, beat, fab };
  }
  const isL = s => /\bLumi\b/.test(s) && !/\bAzu\b/.test(s);
  ok('[name] a new name, typed in Who › Name (real keys): the panel (Mini, Open), its ask field, the player\'s eyebrow, a game\'s card, the ✦\'s name, Ask about the view\'s pill, the first introduction, What next?\'s offer, EN and PT',
    ['en', 'pt'].every(l => { const x = nm[l]; return isL(x.mini) && isL(x.open[0]) && isL(x.open[1]) && /^LUMI · /.test(x.eye) && /^LUMI · /.test(x.beat) && x.fab.every(isL); }) && /Ask Lumi anything/.test(nm.en.open[1]) && /Pergunte ao Lumi/.test(nm.pt.open[1]), nm);
  const azu = await R('S.tab="guide";S.ui.level=9;S.op={};S.ownAll=true;renderEditor();var out=[],w=document.createTreeWalker(ed,NodeFilter.SHOW_TEXT);while(w.nextNode())if(/\\bAzu\\b/.test(w.currentNode.nodeValue))out.push(w.currentNode.nodeValue.trim().slice(0,90));' +
    'ed.querySelectorAll("*").forEach(function(e){["title","data-help","aria-label","placeholder","alt"].forEach(function(a){var v=e.getAttribute(a);if(v&&/\\bAzu\\b/.test(v))out.push(a+": "+v.slice(0,90))})});S.ownAll=false;out');
  const sz = await R('ed.textContent.length');
  ok('[name] ...and nowhere in the Editor does "Azu" remain: the Guide tab\'s text, help, labels and placeholders, every foldout open (the name is Lumi now)', azu.length === 0 && sz > 50000, { azu: azu.slice(0, 8), sz });
  await R('S.tx.mascot_called={en:"Azu",pt:"Azu"};S.ui.level=2;renderEditor();""');
  tsec('name');

  // ---------- [voice] the default set by its id; the coverage; the recorded source ----------
  const v1 = await R('var s=voiceSets(),fake={id:"zz-more",provider:"azure",label:"Azure Neural · en-GB-Zed / pt-PT-Zed",voices:{en:"en-GB-ZedNeural",pt:"pt-PT-ZedNeural"},files:{},url:""};for(var i=0;i<2000;i++)fake.files["f"+i]={src:"x.mp3",text:"x"+i,lang:"en",words:[]};window.VOICESETS.push(fake);' +
    'var a=[S.guideCfg.voiceSet,VOICE_DEFAULT,currentSet().id,voiceSetName(currentSet())];S.guideCfg.voiceSet="";a.push(currentSet().id);S.guideCfg.voiceSet=VOICE_DEFAULT;window.VOICESETS=window.VOICESETS.filter(function(x){return x!==fake});a');
  ok('[voice] the default is Ryan · Duarte (Azure) by its id: a set with more lines does not take its place (Best available would)', v1[0] === 'azure-ryan-duarte' && v1[1] === 'azure-ryan-duarte' && v1[2] === 'azure-ryan-duarte' && v1[3] === 'Ryan · Duarte (Azure)' && v1[4] === 'zz-more', v1);
  const cov = await R('var c=vsCoverage(),rib=guideScript().filter(function(x){return!x.live&&/Ribeira/.test(x.where)});[c.all,c.have,c.miss.map(function(x){return x.where+" · "+x.lang+" · "+x.text.slice(0,50)}).slice(0,6),rib.length,rib.filter(function(x){return!vsCovered(x,currentSet())}).length,c.live.length]');
  ok(`[voice] every line of the script for the Ribeira Palace has its recording (${cov[3]} lines), and the Coverage list is empty but for the live lines (${cov[1]} of ${cov[0]} lines recorded, ${cov[5]} live)`, cov[3] > 100 && cov[4] === 0 && cov[0] > 400 && cov[1] === cov[0] && cov[2].length === 0 && cov[5] >= 2, cov);
  await R('S.tab="guide";S.ui.level=9;S.op={};renderEditor();""');
  const covRow = await R('var r=ed.querySelector(\'[data-fo="gt:voice"] .vscov\'),l=ed.querySelector(\'[data-fo="gt:voice"] .vslive\');[r&&r.textContent,l&&l.textContent,!!ed.querySelector(\'[data-fo="gt:voice"] .vsmiss\')]');
  ok('[voice] ...the Editor says it: "N of N lines recorded in Ryan · Duarte (Azure)", the live lines named, no list of lines for the device voice', /^(\d+) of \1 lines recorded in Ryan · Duarte \(Azure\)/.test(covRow[0] || '') && /Answers to the visitor/.test(covRow[1] || '') && !covRow[2], covRow);
  await R(base() + 'S.introSeen=false;S.view="card";S.stop="full";renderPhone(false);startTour();""');
  const in1 = await until('G.on&&S.guideSteps[G.i]&&S.guideSteps[G.i].act==="intro"&&G.sayNow&&G.sayNow.src', 4000) && await R('[G.sayNow.src,G.sayNow.by,G.sayNow.text]');
  await R(base() + 'S.guidance.on=true;gd12Keep();gd12Only(["quizpromise","quizfinale"]);S.introSeen=false;S.view="card";S.stop="full";renderPhone(false);startTour();""');
  const pr0 = await until('gd12Kind("promise")&&G.sayNow&&G.sayNow.text===TXT("tour_promise")', 30000) && await R('[G.sayNow.src,G.sayNow.by,G.sayNow.text]');
  ok('[voice] the first introduction and the quiz promise are said by the recorded source, Ryan · Duarte (G.sayNow.src)', in1 && in1[0] === 'recorded' && in1[1] === 'Ryan · Duarte (Azure)' && pr0 && pr0[0] === 'recorded', { in1, pr0 });
  tsec('voice');

  // ---------- [audit] the voice in one place ----------
  const srcDir = path.join(__dirname, '..', 'src'), hits = [];
  for (const a of fs.readdirSync(srcDir)) for (const f of fs.readdirSync(path.join(srcDir, a)).filter(x => /\.js$/.test(x))) {
    const name = a + '/' + f; if (name === 'guide/voice.js') continue;
    const s = fs.readFileSync(path.join(srcDir, a, f), 'utf8').replace(/\/\*[\s\S]*?\*\//g, '');
    const re = /(^|[^.\w$])speechSynthesis\b|getVoices|SpeechSynthesisUtterance|[A-Za-z]+Neural["']|elevenPT|elevenEN|\bvoicesFor\(|\bvoiceFor\(/g; let m;
    while ((m = re.exec(s))) hits.push(name + ': ' + s.slice(Math.max(0, m.index - 30), m.index + 40).replace(/\s+/g, ' '));
  }
  ok('[audit] no module but guide/voice.js touches the device voice or picks a voice (speechSynthesis, getVoices, an utterance, a voice\'s name or id): every line goes through say()', hits.length === 0, hits.slice(0, 8));
  const live = await R('[azureVoice("en"),azureVoice("pt")]');
  ok('[audit] ...the live voice speaks in the chosen voice (Azure: Ryan and Duarte, no longer Sonia and Raquel)', live[0] === 'en-GB-RyanNeural' && live[1] === 'pt-PT-DuarteNeural', live);

  // ---------- [intro] the new words, warm, naming a control on the screen ----------
  const iw = await R('var o=S.tx.guide_intro,set=currentSet(),dur=function(l){var t=introLine(l,"x","intro_ask_arrow"),f=set&&set.files[fnvKey(l+"|"+t)];return f&&f.words&&f.words.length?f.words[f.words.length-1][1]:null};[o.en,o.pt,dur("en"),dur("pt")]');
  const warm = t => !/\bNow\b|\bAgora\b|tap Ask|tocar em Perguntar|toque em Perguntar/.test(t) && /\{name\}/.test(t) && /\{ask\}/.test(t) && (t.match(/[.!?]/g) || []).length >= 2 && t.split(/\s+/).length <= 22;
  ok('[intro] the first introduction: warm, in the mascot\'s name, two short sentences, never "Now" nor "tap Ask", EN and PT; about 8 s aloud (its recording)', warm(iw[0]) && warm(iw[1]) && iw[2] > 4 && iw[2] <= 9.5 && iw[3] > 4 && iw[3] <= 9.5, iw);
  const ctl = [];await R('window.__stubMs=6000;""');
  for (const [nmc, set, wantK, sel] of [['ink, Mini', 'S.guideCfg.look="ink";S.guideCfg.dockStart="mini";', 'intro_ask_arrow', '.gdock .gd-openb'], ['player, Mini', 'S.guideCfg.look="player";S.guideCfg.dockStart="mini";', 'intro_ask_face', '.gdock .gd-avb'], ['ink, Compact', 'S.guideCfg.look="ink";S.guideCfg.dockStart="compact";', 'intro_ask_star', '.gbtn.tour']]) {
    await R(base() + set + 'S.introSeen=false;S.view="card";S.stop="full";renderPhone(false);startTour();""');
    await until('G.on&&S.guideSteps[G.i]&&S.guideSteps[G.i].act==="intro"', 3000); await W(500);
    ctl.push([nmc, await R(`var t=stepText(S.guideSteps[G.i]),e=ph.querySelector('${sel}'),r=e&&e.getBoundingClientRect(),P=ph.getBoundingClientRect();[t.indexOf(TXT("${wantK}"))>=0,!!r&&r.width>20&&r.top>=P.top&&r.bottom<=P.bottom+1&&getComputedStyle(e).visibility!=="hidden",t.slice(-48)]`)]);
  }
  ok('[intro] ...it names the control the visitor sees at that moment: the panel\'s arrow at Mini, the mascot\'s face (Player sheet), the ✦ at Compact', ctl.every(c => c[1][0] && c[1][1]), ctl);
  await R('S.guideCfg.dockStart="mini";S.guideCfg.look="ink";window.__stubMs=300;""');
  tsec('intro');

  // ---------- [promise] the card never covers the hero's title or line ----------
  const prm = {};await R('window.__stubMs=6000;""');
  for (const l of ['en', 'pt']) {
    await R(base({ lang: l }) + 'S.guidance.on=true;gd12Keep();gd12Only(["quizpromise","quizfinale"]);S.introSeen=false;S.view="card";S.stop="full";renderPhone(false);startTour();""');
    await until('gd12Kind("promise")&&!!ph.querySelector(".gdbeat")', 30000);
    /* the card is placed and the card's scroll settles (the guide's smooth scroll): until the hero's line is clear, at most 4 s */
    await until('var b=ph.querySelector(".gdbeat"),q=ph.querySelector(".hero .w p");!!b&&!!q&&b.getBoundingClientRect().top>=q.getBoundingClientRect().bottom', 8000); await W(400);
    prm[l] = await R('var bb=ph.querySelector(".gdbeat");if(!bb){["no card"]}else{var b=bb.getBoundingClientRect(),t=ph.querySelector(".hero .w h2").getBoundingClientRect(),q=ph.querySelector(".hero .w p").getBoundingClientRect(),e=ph.querySelector(".hero .w small").getBoundingClientRect();[Math.round(b.top),Math.round(t.bottom),Math.round(q.bottom),Math.round(e.top),S.guideSteps[G.i].ref,S.guideSteps[G.i].act,Math.round(scr.scrollTop),Math.round((G.autoScrollTill||0)-performance.now()),Math.round(freeHeight().bottom),scr.scrollHeight-scr.clientHeight,S.stop,S.dockH,(ph.querySelectorAll(".gdbeat")).length]}');
  }
  await R('window.__stubMs=300;""');
  ok('[promise] the quiz promise\'s card leaves the hero\'s title and line clear: its top below the line\'s bottom (the test\'s arithmetic), EN and PT', ['en', 'pt'].every(l => prm[l][4] === 'hero' && prm[l][0] >= prm[l][2] && prm[l][0] >= prm[l][1] && prm[l][3] > 40), prm);
  tsec('promise');

  // ---------- [tap here] the tail on the head ----------
  const tapHere = async (reduce) => {
    await R(base({ reduce }) + 'S.guidance.on=true;S.guidance.items.gbhint.on=false;gdGroupById("inview").on=false;S.guidance.breathing=5;G.gd.demo=false;gdChip(null);G.cam.open=false;G.fab.menu=false;setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);G.gd.manual=true;gdStop("test");gdResetVisitor();G.gd.movedAt=null;G.gd.onWall=false;gdAdvance(250);""');
    await W(300); await R('var t0=gdNow();while(gdNow()-t0<40000){gdAdvance(250);if(G.gd.cur&&G.gd.cur.item.id==="tapmarker")break}""'); await W(1300);
    /* the test's own figure: the still picture's opaque pixels, as drawn (object-fit: contain); the head its top 40 %, middle 60 % */
    return R('var az=ph.querySelector(".azfab"),i=az.querySelector(".azstill"),r=i.getBoundingClientRect(),c=document.createElement("canvas");c.width=i.naturalWidth;c.height=i.naturalHeight;var x=c.getContext("2d");x.drawImage(i,0,0);var d=x.getImageData(0,0,c.width,c.height).data,L=1e9,T=1e9,Rr=-1,B=-1;' +
      'for(var y=0;y<c.height;y++)for(var q=0;q<c.width;q++)if(d[(y*c.width+q)*4+3]>40){L=Math.min(L,q);Rr=Math.max(Rr,q);T=Math.min(T,y);B=Math.max(B,y)}var s=Math.min(r.width/c.width,r.height/c.height),ox=r.left+(r.width-c.width*s)/2,oy=r.top+(r.height-c.height*s)/2,F={l:ox+L*s,r:ox+(Rr+1)*s,t:oy+T*s,b:oy+(B+1)*s},w=F.r-F.l,h=F.b-F.t,H={l:F.l+w*.2,r:F.r-w*.2,t:F.t,b:F.t+h*.4};' +
      'var bub=ph.querySelector(".gdbub"),tl=bub.querySelector(".gdtail").getBoundingClientRect(),k=bub.dataset.tail,mid=(tl.top+tl.bottom)/2,cx=(tl.left+tl.right)/2,gap=k==="left"?Math.abs(tl.left-H.r):k==="right"?Math.abs(tl.right-H.l):k==="down"?Math.abs(tl.bottom-H.t):Math.abs(tl.top-F.b),along=k==="left"||k==="right"?(mid>=H.t-1&&mid<=H.b+1):(cx>=H.l-1&&cx<=H.r+1);' +
      'var m=gdMarker(),A=az.getBoundingClientRect(),P=ph.getBoundingClientRect(),qq=P.height/ph.offsetHeight,ax=(A.left+A.right)/2,mx=P.left+m.x*qq,pt=ph.querySelector(".gdpoint");[k,Math.round(gap*10)/10,along,az.classList.contains("gd-point"),az.dataset.gdlean===(mx<ax?"left":"right"),!!pt&&getComputedStyle(pt).display!=="none",bub.textContent.trim(),az.getAnimations().length]');
  };
  const th = await tapHere(false);
  ok('[tap here] the wall\'s "Tap here.": its tail\'s tip meets the mascot\'s head (within 4 px, the test\'s own measure of the figure), the bubble beside the head, the mascot pointing at the marker (leaning to it, its pointer shown)',
    th[1] <= 4 && th[2] && (th[0] === 'left' || th[0] === 'right') && th[3] && th[4] && th[5] && th[6] === 'Tap here.', th);
  const thr = await tapHere(true);
  ok('[reduced motion] ...under Reduce Motion: the same place, no hop (no animation on the mascot), the tail on the head', thr[1] <= 4 && thr[2] && thr[7] === 0, thr);
  await R('gdStop("test");S.guidance.on=false;S.reduce=false;""');
  tsec('tap here');

  // ---------- [card] the mascot beside Start, nothing over anything, the glow ----------
  const openCard = async (o) => {
    await R(base(o) + 'S.hints.on=true;S.guidance.on=true;G.gd.manual=false;G.hs={glow:false,tapped:false,t:0};S.locFolded=false;S.locShown=0;G.locRoom=false;setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);gdResetVisitor();""'); await W(500);
    await tapEl('[data-marker="painting"]');
    /* the card settles, the Locate label's cue opens it (the guidance's own moment), the hero's words make room: then measured */
    await until('!!ph.querySelector(".locpin .loclab:not(.fold)")&&!!ph.querySelector(".hero .gstart.hsglow")', 8000); await W(900);
    return R('var P=ph.getBoundingClientRect(),q=P.height/ph.offsetHeight,g=function(s){var e=ph.querySelector(s);if(!e||!e.getClientRects().length||getComputedStyle(e).visibility==="hidden")return null;var r=e.getBoundingClientRect();return{l:(r.left-P.left)/q,t:(r.top-P.top)/q,r:(r.right-P.left)/q,b:(r.bottom-P.top)/q,el:e}},' +
      'B={eyebrow:g(".hero .w small"),title:g(".hero .w h2"),line:g(".hero .w p"),mascot:g(".hero .hsfig"),start:g(".hero .gstart"),label:g(".locpin .loclab:not(.fold)"),pin:g(".locpin")},inkOf=function(e){var rg=document.createRange();rg.selectNodeContents(e);var r=rg.getBoundingClientRect();return{l:(r.left-P.left)/q,t:(r.top-P.top)/q,r:(r.right-P.left)/q,b:(r.bottom-P.top)/q}};' +
      '["eyebrow","title","line"].forEach(function(k){if(B[k])B[k]=Object.assign(inkOf(B[k].el),{el:B[k].el})});var ov=[],ks=Object.keys(B);for(var i=0;i<ks.length;i++)for(var j=i+1;j<ks.length;j++){var a=B[ks[i]],b=B[ks[j]];if(!a||!b||ks[i]==="label"&&ks[j]==="pin")continue;var x=Math.min(a.r,b.r)-Math.max(a.l,b.l),y=Math.min(a.b,b.b)-Math.max(a.t,b.t);if(x>0&&y>0)ov.push(ks[i]+"/"+ks[j]+" "+Math.round(x*y))}' +
      'var whole=function(e){return!!e&&e.el.scrollWidth<=e.el.clientWidth+1&&e.l>=0&&e.r<=ph.offsetWidth&&e.b<=ph.offsetHeight},st=B.start;' +
      '[!!B.mascot,!!B.mascot&&!!st&&B.mascot.r<=st.l+2&&B.mascot.b>st.t&&B.mascot.t<st.b,ov,whole(st),!!B.label&&whole(B.label),st&&Math.round(st.b-st.t),B.pin&&Math.round(B.pin.b-B.pin.t),st&&st.el.classList.contains("hsglow"),st&&getComputedStyle(st.el).animationName,!!B.label]');
  };
  const c1 = await openCard({});
  ok('[card] the Ribeira Palace opened by a real tap on its marker (390 px): the mascot in sight at the hero, standing left of Start the guide, pointing at it', c1[0] && c1[1], c1);
  ok('[card] ...nothing over anything: the hero\'s eyebrow, title and line, the mascot, Start, Find it on the wall and its pin (0 px², the test\'s arithmetic); each text whole; each target 44 px', c1[9] && c1[2].length === 0 && c1[3] && c1[4] && c1[5] >= 44 && c1[6] >= 44, c1);
  ok('[card] ...Start the guide\'s calm "tap me" life once the card has settled: a soft glow and a slow pulse', c1[7] && c1[8] === 'hspulse', c1);
  await tapEl('.hero .gstart'); await W(400);
  const c2 = await R('var a=G.on;stopTour();S.view="card";S.stop="half";renderPhone(false);[a,G.hs.tapped,!!ph.querySelector(".hero .gstart.hsglow")]'); await W(1200);
  const c3 = await R('!!ph.querySelector(".hero .gstart.hsglow")');
  ok('[card] ...a real tap on it starts the tour, and the glow is gone (not back once the card is drawn again)', c2[0] && c2[1] && !c2[2] && !c3, [c2, c3]);
  const c4 = await openCard({ lang: 'pt' });
  ok('[card] ...in Portuguese too ("Visita com o guia", "Encontra-o na parede"): nothing over anything, each text whole', c4[0] && c4[1] && c4[2].length === 0 && c4[3] && c4[4], c4);
  const c5 = await openCard({ reduce: true });
  ok('[reduced motion] ...the glow alone under Reduce Motion, no pulse', c5[7] && c5[8] === 'none', c5);
  await R('S.reduce=false;S.guidance.on=false;S.hints.on=false;""');
  tsec('card');

  // ---------- [guess] longer, longest the first time ----------
  /* the scene's start is a first visit (gd12Only); the second guess is the same visit's tour started again */
  const guess = async (again) => { await R(again ? 'if(G.on)stopTour();gdBeatClose();S.view="card";S.stop="full";renderPhone(false);startTour();""' : 'if(G.on)stopTour();gdBeatClose();S.guidance.on=true;gd12Keep();gd12Only(["guessyear"]);S.view="card";S.stop="full";renderPhone(false);startTour();""'); await until('G.gd.plan', 8000);
    await R('var b=G.gd.plan&&gdPlanBeat("guessyear");b&&gd12Seam(b.after);""'); await until('!!ph.querySelector(".gdbeat .bt-range")', 12000);
    return R('var B=G.gd.bt;[B&&B.kind,B&&B.waitS,B&&B.tm&&B.tm.waitT&&Math.round(B.tm.waitT.left/1000),!!(S.gdVisitor.visit&&S.gdVisitor.visit.guessed)]'); };
  await R(base() + 'gdResetVisitor();""');
  const g1 = await guess(false), g2 = await guess(true);
  ok('[guess] "Before I tell you, a guess": 14 s to answer (it was 8), 18 s the first time in a visit (+4 s)', g1[0] === 'guess' && g1[1] === 18 && g1[2] === 18 && g2[1] === 14 && g2[2] === 14, [g1, g2]);
  await R('if(G.on)stopTour();gdBeatClose();S.tab="guide";S.ui.level=9;S.op={};renderEditor();""');
  const gr = await R('var f=ed.querySelector(\'[data-fo="gd:i:guessyear"]\'),w=f&&[].filter.call(f.querySelectorAll(".row"),function(r){return/^Waits \\(s\\)|^First guess of a visit/.test(r.querySelector("label").textContent)}).map(function(r){var i=r.querySelector("input[type=range]");return r.querySelector("label").textContent+"|"+(i&&i.max)+"|"+(i&&i.value)});w');
  ok('[guess] ...two Editor rows: Waits (s), up to 30, 14; First guess of a visit (+ s), 4', JSON.stringify(gr) === JSON.stringify(['Waits (s)|30|14', 'First guess of a visit (+ s)|10|4']), gr);
  tsec('guess');

  ok('no page errors', errs.length === 0, errs.slice(0, 5));
  console.log('fails', fails);
  await b.close();
})();
