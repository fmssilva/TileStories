// test9sf (Playwright + the installed Edge): node test9sf.js -> PASS/FAIL per check. Guidance round 15b's [azu] check, split from
// test9se.js in guidance round 16 (test9se ran 324-340 s; each file now stays under 300 s), the same check and the same mutation.
// [azu] Azu 0 px over the words after a quick step jump (the test's own way: stopSpeech();G.i=j;runStep()), 6 repeats at g1, g5, g7,
//   light and dark, Mini: measured once Azu stands still and again 1.5 s later (the card's words under Azu's whole box, deeper than 1 px).
// MUTATE=1: (test9se's whole mutation) Azu never placed again after its step's start: the check named [azu] turns red. CAP=<dir> saves g5.
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
