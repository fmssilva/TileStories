// test9se (Playwright + the installed Edge): node test9se.js -> PASS/FAIL per check. Guidance round 15b: the developer's answers on round
// 15's design pass (2026-11-11) and one old defect, measured with round 15's rules (tests/g15lib.js, installed as window.G15).
// [mascot off] Mascot Off is the ✦ alone: no Azu picture beside the ✦ on the card (past the hero) and on the wall, Guide button = Azu only
//   showing the ✦ then; the plain bubble at its target signed by a small ✦ (present, inside the bubble, 0 px over its words, not a tap
//   target), a tip and a remark, light and dark, the round 15 rules on it (44 px, 4.5:1, inside the phone, one focal point); any bubble with
//   no Azu beside it is signed (Guide button = ✦ only with the mascot on, 2026-11-12); the mascot on with Azu beside the ✦: nothing changes.
// [article] a point's name mid-sentence takes a lower-case article (nameIn, core/words.js): EN and PT, a slot mid-sentence lower-cased, a
//   slot at the start kept, after a colon lower-cased, a name with no article as written; on the page a remark about the Torreão on the
//   wall, the marker's label unchanged; Azu's lines (the introduction, Ask about the view's What am I looking at? and the hunt).
// [azu] Azu 0 px over the words after a quick step jump (the test's own way: stopSpeech();G.i=j;runStep()), 6 repeats at g1, g5, g7,
//   light and dark, Mini: measured once Azu stands still and again 1.5 s later (the card's words under Azu's whole box, deeper than 1 px).
// MUTATE=1: Azu beside the ✦ whatever Mascot says and the bubble unsigned; every name as written in its slot; Azu never placed again after
//   its step's start: the checks named [mascot off], [article] and [azu] turn red. CAP=<dir> saves the three changes at 390 px, light and dark.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
const CAP = process.env.CAP || '', REPS = +process.env.REPS || 6;
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 1000 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 900) : '')); };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  const shot = async name => { if (!CAP) return; fs.mkdirSync(CAP, { recursive: true }); const r = await p.locator('#ph').boundingBox(); await p.screenshot({ path: path.join(CAP, name + '.png'), clip: { x: r.x, y: r.y, width: r.width, height: r.height } }); };
  await R(require('./g15lib.js'));
  await R('window.__g0=JSON.stringify(S.guidance)');
  if (process.env.MUTATE) {
    await R('gbAzu=function(){return S.guideCfg.gbtn!=="star"};gbOn=function(){return S.guideCfg.gbtn!=="azu"};gdSign=function(){return""};nameMid=function(n){return String(n==null?"":n)};azuPlace=function(m){return parseFloat(m.style.top)||0}');
    console.log('MUTATION: Azu beside the ✦ whatever Mascot says, the bubble unsigned; every name as written in its slot; Azu never placed again after its step\'s start');
  }
  /* the state every check starts from (test9s.js's): a 390 px phone, the device voice muted, the guidance quiet */
  const base = (o) => `SCREENS.phone=[390,844];if(G.on)stopTour();if(G.moment)azMomentEnd(false);ph.querySelectorAll(".azbub").forEach(function(x){x.remove()});gdBeatClose();gdStop("test");S.guidance=JSON.parse(window.__g0);
    S.hints.on=false;S.previewBar=false;S.lang="${o.lang || 'en'}";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=${o.theme === 'dark'};S.cardCfg.mode="${o.theme === 'dark' ? 'dark' : 'light'}";
    S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.captions=true;S.guideCfg.music=false;S.vis.music=null;S.ui.rowDemo=false;S.ui.gdSR=false;S.m.solemn=false;S.vis.aud="explorer";S.guideCfg.life="lively";
    S.guideCfg.gbtn="${o.gbtn || 'both'}";S.guideCfg.mascot=${o.mascot !== false};S.guideCfg.look="ink";S.agentCfg.voiceOut=false;
    G.gd.demo=false;gdChip(null);G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};S.settingsOpen=false;G.fab.menu=false;G.askOpen=false;
    S.guidance.on=true;S.guidance.kinds.forEach(function(k){k.on=false});G.gd.manual=true;gdResetVisitor();G.gd.movedAt=null;G.gd.touchAt=null;`;
  const wall = 'setWall("ribeira");S.camZoom=1;S.view="card";S.stop="closed";renderAll(false);';
  const card = 'S.view="card";S.stop="full";renderAll(false);scr.scrollTop=900;scr.dispatchEvent(new Event("scroll"));';
  const show = (id) => `gdStart(gdItemNow("${id}"),"test",{pull:true,ms:60000});`;
  const LT = ['light', 'dark'];

  // ---------- 1. Mascot Off: the ✦ alone, the bubble signed ----------
  const btn = 'var w=ph.querySelector(".gbtn-wrap:not(.intour)");JSON.stringify({star:!!(w&&w.querySelector(".gbtn")&&G15.targets().some(function(t){return/gbtn/.test(t.el)})),azu:!!ph.querySelector(".azfab"),kids:w?[].map.call(w.children,function(c){return c.className.split(" ")[0]}):null})';
  const off = {}, on = {};
  for (const theme of LT) {
    await R(base({ theme, mascot: false }) + card); await until('!!ph.querySelector(".gbtn-wrap .gbtn")', 3000); await W(300);
    off[theme + ' card'] = JSON.parse(await R(btn)); await shot(`mascotoff_card_${theme}`);
    await R(base({ theme, mascot: false }) + wall); await until('!!ph.querySelector(".gbtn-wrap .gbtn")', 3000); await W(300);
    off[theme + ' wall'] = JSON.parse(await R(btn));
    await R(base({ theme, mascot: false, gbtn: 'azu' }) + wall); await until('!!ph.querySelector(".gbtn-wrap .gbtn")', 3000); await W(200);
    off[theme + ' wall, Azu only'] = JSON.parse(await R(btn));
    await R(base({ theme }) + card); await until('!!ph.querySelector(".azfab")', 3000); await W(200); on[theme + ' card'] = JSON.parse(await R(btn));
    await R(base({ theme }) + wall); await until('!!ph.querySelector(".azfab")', 3000); await W(200); on[theme + ' wall'] = JSON.parse(await R(btn));
  }
  ok('[mascot off] Mascot Off: the ✦ alone on the card (past the hero) and on the wall, light and dark: no Azu picture beside it (Guide button = Azu only: the ✦ then)', Object.values(off).every(x => x.star && !x.azu && x.kids.join() === 'gbtn'), off);
  ok('...the mascot on: nothing changes (Azu beside the ✦ on the card and the wall)', Object.values(on).every(x => x.star && x.azu), on);
  const bub = {};
  for (const theme of LT) for (const [id, kind] of [['tapmarker', 'app'], ['inview', 'content']]) {
    await R(base({ theme, mascot: false }) + wall + `S.guidance.kinds.forEach(function(k){k.on=k.id==="${kind}"});` + show(id)); await until('!!ph.querySelector(".gdbub:not(.gd-out)")', 3000); await W(700);
    const a = await R('var a=G15.audit();JSON.stringify({sign:G15.sign(),small:a.targets.filter(function(t){return!t.ok}).length,low:a.text.filter(function(t){return t.cr<4.5}).length,out:a.inside.length,focal:a.focal.length,lines:Math.round(ph.querySelector(".gdbub:not(.gd-out) [data-gdbub]").getBoundingClientRect().height)})');
    bub[theme + ' ' + id] = JSON.parse(a); await shot(`mascotoff_${id}_${theme}`);
  }
  const sg = Object.values(bub);
  ok('[mascot off] the plain bubble is signed: a ✦ mark in each bubble (a tip and a remark, light and dark)', sg.every(x => x.sign && x.sign.has && x.sign.plain), bub);
  ok('[mascot off] ...the mark inside the bubble, 0 px over its words, not a tap target (a tap at its centre reaches the bubble, never the mark; aria-hidden)', sg.every(x => x.sign && x.sign.inside && x.sign.over === 0 && !x.sign.tap && x.sign.pe === 'none' && x.sign.aria === 'true'), bub);
  ok('...the signed bubble keeps round 15\'s rules: 44 px, 4.5:1, inside the phone, one focal point', sg.every(x => x.small === 0 && x.low === 0 && x.out === 0 && x.focal <= 1), bub);
  await R(base({}) + wall + 'S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});' + show('tapmarker')); await until('!!ph.querySelector(".gdbub:not(.gd-out)")', 3000); await W(400);
  const onB = JSON.parse(await R('JSON.stringify(G15.sign())'));
  ok('...the mascot on: the bubble beside Azu carries no mark (nothing changes)', onB && !onB.has && !onB.plain, onB);
  /* the developer (2026-11-12): any guide bubble with no Azu beside it is signed: Guide button = ✦ only, the mascot on, light and dark */
  const star = {};
  for (const theme of LT) { await R(base({ theme, gbtn: 'star' }) + wall + 'S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});' + show('tapmarker')); await until('!!ph.querySelector(".gdbub:not(.gd-out)")', 3000); await W(500);
    star[theme] = JSON.parse(await R('JSON.stringify({sign:G15.sign(),azu:!!ph.querySelector(".azfab")})')); await shot(`staronly_tapmarker_${theme}`); }
  ok('[mascot off] Guide button = ✦ only with the mascot on: no Azu beside the ✦, so the bubble is signed too (inside, 0 px over its words, not a tap target), light and dark', Object.values(star).every(x => !x.azu && x.sign && x.sign.has && x.sign.inside && x.sign.over === 0 && !x.sign.tap), star);
  tsec('mascot off');

  // ---------- 2. a point's name mid-sentence: a lower-case article ----------
  const L1 = (lang, key, slots) => `(function(){S.lang="${lang}";var it=Object.assign({},S.guidance.items.inview,{says:["${key}"],saysAud:{},saysBack:""});return gdLine(it,false,${slots})})()`;
  const en = await R(`[${L1('en', 'fab_comment_2', '{point:"The Torreão",n:0}')},${L1('en', 'gd_tourof', '{point:"The Torreão",min:3,n:0}')},${L1('en', 'fab_comment_1', '{point:"The Torreão",n:0}')},${L1('en', 'fab_comment_2', '{point:"Ribeira Palace",n:0}')},${L1('en', 'gd_hunt', '{thing:"The Tagus and its ships",n:0}')}]`);
  const pt = await R(`[${L1('pt', 'fab_comment_2', '{point:"O Torreão",n:0}')},${L1('pt', 'gd_tourof', '{point:"O Torreão",min:3,n:0}')},${L1('pt', 'fab_comment_1', '{point:"A ermida no alto",n:0}')},${L1('pt', 'fab_comment_2', '{point:"Os Jerónimos",n:0}')},${L1('pt', 'gd_tourof', '{point:"As arcadas",min:2,n:0}')}]`);
  await R('S.lang="en"');
  ok('[article] EN: a name mid-sentence takes "the" ("That\'s the Torreão right there"; after a colon too; the hunt\'s "Find the Tagus…"), at the start kept ("The Torreão has a story"), a name with no article as written',
    en[0] === "That's the Torreão right there. Want the story? Tap me." && en[1] === 'The Torreão has a story. A tour of 3 minutes?' && en[2] === 'In the middle: the Torreão. Tap to hear more.' && en[3] === "That's Ribeira Palace right there. Want the story? Tap me." && en[4] === 'Want a treasure hunt? Find the Tagus and its ships on the wall.', en);
  ok('[article] PT: O / A / Os / As become o / a / os / as mid-sentence ("Ali está o Torreão"), at the start kept ("O Torreão tem uma história", "As arcadas…")',
    pt[0] === 'Ali está o Torreão. Quer a história? Toque-me.' && pt[1] === 'O Torreão tem uma história. Uma visita de 3 minutos?' && pt[2] === 'Ao centro: a ermida no alto. Toque para saber mais.' && pt[3] === 'Ali está os Jerónimos. Quer a história? Toque-me.' && /^As arcadas tem uma história/.test(pt[4]), pt);
  // on the page: Azu's remark about the Torreão on the wall (the Torreão in the middle), the marker's label as written
  const pg = {};
  for (const lang of ['en', 'pt']) {
    await R(base({ lang }) + 'S.camCfg.names="all";' + wall + 'gdSimAim("torreao");renderPhone(false);S.guidance.kinds.forEach(function(k){k.on=k.id==="content"});var it=gdItemNow("inview:torreao");gdStart(it,"test",{pull:true,ms:60000})');
    await until('!!ph.querySelector(".gdbub:not(.gd-out)")', 3000); await W(500);
    pg[lang] = await R('[(ph.querySelector(".gdbub:not(.gd-out) [data-gdbub]")||{}).textContent,(ph.querySelector(\'[data-cpin="torreao"] b\')||ph.querySelector(\'#wall .mk[data-mk="torreao"] b\')||{}).textContent,L(S.wallPois.filter(function(p){return p.id==="torreao"})[0].name)]');
    if (lang === 'en') await shot('article_remark_light');
  }
  await R('S.lang="en"');
  ok('[article] on the page: Azu\'s remark about the Torreão on the wall says "the Torreão" / "o Torreão" mid-sentence; the point\'s label still "The Torreão" / "O Torreão" (the stored name unchanged)',
    /(: |está )(the|o) Torreão/.test(pg.en[0]) && !/The Torreão/.test(pg.en[0]) && /(: |está )o Torreão/.test(pg.pt[0]) && pg.en[1] === 'The Torreão' && pg.en[2] === 'The Torreão' && pg.pt[1] === 'O Torreão' && pg.pt[2] === 'O Torreão', pg);
  // Azu's lines: the introduction (EN, PT), What am I looking at? and the hunt on the Ribeira wall
  await R(base({}) + 'S.camCfg.names="all";' + wall + 'gdSimAim("torreao");renderPhone(false)');
  const az = await R('var w=camOffline("what");gdSimAim("river");renderPhone(false);var k=camOffline("kids");G.cam.hunt=null;G.cam.lit=[];[nameIn(S.tx.guide_intro.en,"{0}","The Torreão","en"),nameIn(S.tx.guide_intro.pt,"{0}","O Torreão","pt"),w,k,nameIn(TXT("cam_story_none"),"{0}","The Torreão")]');
  ok('[article] Azu\'s lines: "let\'s meet the Torreão!", "vamos conhecer o Torreão!", What am I looking at? names "the Torreão" after its place, the hunt "Find the Tagus…", a line that starts with the name keeps "The"',
    /let's meet the Torreão!$/.test(az[0]) && /vamos conhecer o Torreão!$/.test(az[1]) && /: the Torreão\./.test(az[2]) && !/[^.] The /.test(az[2]) && /Find the Tagus and its ships/.test(az[3]) && /^The Torreão: /.test(az[4]), az);
  tsec('article');

  // ---------- 3. Azu 0 px over the words after a quick step jump ----------
  const rows = [];
  for (let rep = 0; rep < REPS; rep++) for (const dark of [false, true]) for (const id of ['g1', 'g5', 'g7']) {
    await R(base({}) + `S.view="card";S.stop="full";renderAll(false);startTour();var j=S.guideSteps.findIndex(function(s){return s.id==="${id}"});if(j>0){stopSpeech();G.i=j;runStep()}S.dockH="mini";renderGuide()`); await W(500);
    await R(`S.dark=${dark};S.cardCfg.mode="${dark ? 'dark' : 'light'}";renderAll(true)`);
    /* Azu standing still: its place and the scroll unchanged for three samples (at most 10 s) */
    let last = null, same = 0; const t0 = Date.now(); while (Date.now() - t0 < 10000 && same < 3) { const y = await R('JSON.stringify([Math.round(ph.querySelector(".mascot").getBoundingClientRect().top),Math.round(scr.scrollTop)])'); same = y === last ? same + 1 : 0; last = y; await W(150); }
    const o1 = await R('G15.azuInk()'); await W(1500); const o2 = await R('G15.azuInk()');
    const row = { rep, theme: dark ? 'dark' : 'light', id, still: o1, later: o2 };
    /* a red case says where Azu, the block and the scroll were (the phone's px) */
    if (o1 || o2) row.at = await R('var pr=ph.getBoundingClientRect(),m=ph.querySelector(".mascot").getBoundingClientRect(),el=stepEl(S.guideSteps[G.i]).getBoundingClientRect(),f=freeHeight();var M=ph.querySelector(".mascot"),cur=parseFloat(M.style.top),k0=G.wbKey,o0=azuOver(M,cur,G.wb||[]).length;G.wbKey=null;var o1=azuOver(M,cur,wordBoxes(stepEl(S.guideSteps[G.i]))).length;JSON.stringify({azu:+(m.top-pr.top).toFixed(1),styleTop:cur,block:[Math.round(el.top-pr.top),Math.round(el.bottom-pr.top)],free:Math.round(f.bottom-pr.top),scroll:Math.round(scr.scrollTop),tries:G.placeTries,cls:M.className,key:k0,keyNow:G.wbKey,overCached:o0,overFresh:o1,beside:besideWords(M,cur,70,Math.round(f.bottom-pr.top)-72,stepEl(S.guideSteps[G.i]))})');
    rows.push(row);
    if (CAP && rep === 0 && id === 'g5') await shot(`azu_jump_g5_${dark ? 'dark' : 'light'}`);
  }
  const bad = rows.filter(r => r.still > 0 || r.later > 0);
  ok(`[azu] Azu 0 px over the words after a quick step jump: ${REPS} repeats at g1, g5, g7, light and dark (${rows.length} cases), once it stands still and 1.5 s later`, rows.length === REPS * 6 && !bad.length, { bad, sample: rows.slice(0, 6).map(r => r.id + ' ' + r.theme + ' ' + r.still + '/' + r.later) });
  tsec('azu');

  await R('if(G.on)stopTour();gdStop("test");SCREENS.phone=[360,820];S.dark=false;S.cardCfg.mode="light";S.lang="en";S.guideCfg.mascot=true;S.guideCfg.gbtn="both";S.guidance=JSON.parse(window.__g0);renderAll(false)');
  ok('No page error', errs.length === 0, errs.slice(0, 5));
  tsec('end');
  console.log(fails ? `FAILED ${fails}` : 'ALL PASS');
  await b.close(); process.exit(fails ? 1 : 0);
})().catch(e => { console.error(e); process.exit(2); });
