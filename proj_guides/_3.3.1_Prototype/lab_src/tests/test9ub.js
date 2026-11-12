// test9ub (Playwright + the installed Edge): node test9ub.js -> PASS/FAIL per check. Guidance round 17, Part B, the saved place's second
// half (split from test9u.js in round 17, each file under 300 s): [expiry] Forget after by Preview & Test › Today is (the app's own clock, never
// a real wait): 7 days, 30, 1, Never; [changed] the card changed so the step is gone (the Gallery's 🗑 in Specific Marker), or another source
// (the Ribeira's Written tour by the Guide tab's row): no offer, the next start says so once; [rows] Remember Off, Tell me more Off, each new
// row's ▸ showing its demo; [blocked] storage blocked: no error, Tell me more works, nothing kept for the next visit and the Editor says so.
// Each check keeps a place, then a new visit (the page reloaded) as the visitor would come back. MUTATE=1 (as test9u) never forgets a place:
// the check named [expiry] turns red.
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
  await openPin('torreao');
  // ---------- 6. [expiry] Forget after, by Preview & Test › Today is (the app's own clock) ----------
  const keep5 = async (id) => { await R('S.view="card";S.stop="full";renderPhone(false);scr.scrollTop=0;""'); await heroStart(); await jump(id || 'a:t_gal'); await R('stopTour();""'); };
  await keep5(); await visit();
  const day = n => R(`(function(){var d=new Date(Date.now()+${n}*864e5);return d.getFullYear()+"-"+String(d.getMonth()+1).padStart(2,"0")+"-"+String(d.getDate()).padStart(2,"0")})()`);
  const setDay = async n => { const v = await day(n); await R('S.tab="guide";S.op={};S.op.gt0=true;S.ui.level=3;renderEditor();""'); const inp = p.locator('input.gdtoday'); await inp.scrollIntoViewIfNeeded(); await inp.fill(v); await inp.dispatchEvent('change'); await W(200); };
  const forget = async v => { await R('S.tab="guide";S.op={};S.op.gt1=true;S.op["gt:ways"]=true;S.op.s7resume=true;S.ui.level=3;renderEditor();""'); const s = p.locator('select[data-sel="@guideCfg.svForget"]'); await s.scrollIntoViewIfNeeded(); await s.selectOption(v); await W(200); };
  const offered = () => R('S.view="card";S.stop="full";renderPhone(false);!!scr.querySelector(".hero .gcont")');
  await setDay(6); const e6 = await offered(); await setDay(8); const e8 = await offered();
  await forget('30'); const e8b = await offered(); await setDay(31); const e31 = await offered();
  await forget('1'); await setDay(2); const e2 = await offered(); await forget('never'); await setDay(400); const e400 = await offered();
  const kn = await R('(ed.querySelector(".svnow")||{}).textContent||""');
  await forget('7'); await setDay(0);
  ok('[expiry] Forget after by Preview & Test › Today is (real edits, never a real wait): 7 days offered at +6 and not at +8; 30 days at +8, not at +31; 1 day not at +2; Never at +400', e6 && !e8 && e8b && !e31 && !e2 && e400, { e6, e8, e8b, e31, e2, e400, kn });
  tsec('expiry');

  // ---------- 7. [changed] the step gone from the card; another source ----------
  await keep5(); await visit();
  await R('S.tab="marker";S.op={};S.op.mcard=true;S.ui.level=3;renderAll(false);""'); await W(300);
  const gi = await R('S.card.findIndex(function(b){return b.kind==="Gallery"})');
  await p.locator(`[data-fo="mcard"] [data-cmove="${gi}"][data-dir="0"]`).dispatchEvent('click'); await W(300);
  const ch1 = await offered(), knCh = await R('S.tab="guide";S.op={};S.op.gt1=true;S.op["gt:ways"]=true;S.op.s7resume=true;S.ui.level=3;renderEditor();(ed.querySelector(".svnow")||{}).textContent||""');
  await R('scr.scrollTop=0;""'); await heroStart();
  const ch2 = JSON.parse(await R('JSON.stringify({first:S.guideSteps[0].id,t:L(S.guideSteps[0].text),second:S.guideSteps[1].act})')), chK = await kept('torreao');
  await R('stopTour();""'); await heroStart(); const ch3 = await R('S.guideSteps[0].id'); await R('stopTour();""');
  // another source: the Ribeira from the card, a place kept, then its Written tour (the Guide tab's own row)
  await R('pointOpen("painting");S.tours.painting.src="card";S.view="card";S.stop="full";renderAll(false);""'); await heroStart(); await jump(await R('S.guideSteps[4].id')); await R('stopTour();""');
  await load(); await R(base); await R('pointOpen("painting");S.tours.painting.src="card";S.view="card";S.stop="full";renderAll(false);""');
  const src1 = await offered();
  await R('S.tab="guide";S.op={};S.op.gt1=true;S.op.s7auto=true;S.ui.level=2;renderAll(false);""'); await p.click('[data-fo="s7auto"] [data-set="@tours.painting.src"][data-v="written"]'); await W(300);
  const src2 = await offered(); await heroStart(); const src3 = await R('S.guideSteps[0].id'); await R('stopTour();""');
  ok('[changed] the card changed so the step is gone (the Gallery\'s 🗑 in Specific Marker): no offer, the Editor says why; the next start says once, gently, that the tour changed, then the teaser; the place cleared; the start after it says nothing; another source (the Ribeira\'s Written tour by the Guide tab\'s row): no offer, the notice once',
    !ch1 && /changed since/.test(knCh) && ch2.first === 'svnote' && /^This tour has changed since your last visit/.test(ch2.t) && ch2.second === 'teaser' && chK === '' && ch3 !== 'svnote' && src1 && !src2 && src3 === 'svnote', { ch1, knCh, ch2, chK, ch3, src1, src2, src3 });
  tsec('changed');

  // ---------- 8. [rows] the Editor's rows: Remember Off, Tell me more Off, each row's ▸ ----------
  await R('pointOpen("torreao");S.view="card";S.stop="full";renderAll(false);""'); await keep5('a:t_time'); await visit();
  await R('S.tab="guide";S.op={};S.op.gt1=true;S.op["gt:ways"]=true;S.op.s7resume=true;S.ui.level=3;renderEditor();""');
  await p.locator('[data-set="@guideCfg.svOn"][data-v="false"]').click(); await W(300); const off1 = await offered();
  await R('S.tab="guide";S.op={};S.op.gt1=true;S.op["gt:ways"]=true;S.op.s7resume=true;S.ui.level=3;renderEditor();""');
  await p.locator('[data-set="@guideCfg.svOn"][data-v="true"]').click(); await W(300); const on1 = await offered();
  await R('S.tab="guide";S.op={};S.op.gt1=true;S.op.s7more=true;S.ui.level=3;renderEditor();""');
  await p.locator('[data-set="@guideCfg.tmOn"][data-v="false"]').click(); await W(300);
  await R('S.view="card";S.stop="full";renderPhone(false);""'); await heroStart(); await jump('a:t_text'); await W(500); const tmOff = await R('!!ph.querySelector(".gtmore")'); await R('stopTour();""');
  await R('S.tab="guide";S.op={};S.op.gt1=true;S.op.s7more=true;S.ui.level=3;renderEditor();""'); await p.locator('[data-set="@guideCfg.tmOn"][data-v="true"]').click(); await W(300);
  const demos = [];
  for (const k of ['guideCfg.tmOn', 'guideCfg.tmStart', 'guideCfg.svOn', 'guideCfg.svForget']) {
    await R(`S.tab="guide";S.op={};S.op.gt1=true;S.op.s7more=true;S.op["gt:ways"]=true;S.op.s7resume=true;S.ui.level=3;S.ui.rowDemo=true;renderEditor();document.body.dataset.rowdemo="";""`);
    const lb = await R(`var c=ed.querySelector('[data-set="@${k}"],[data-sel="@${k}"]'),l=c.closest(".row").querySelector("label.rd");l.scrollIntoView({block:"center"});var r=l.getBoundingClientRect();[r.left+12,r.top+r.height/2]`);
    await p.mouse.click(lb[0], lb[1]); await until('!!document.body.dataset.rowdemo', 6000); await W(400);
    demos.push(k + ' ' + (await R('document.body.dataset.rowdemo')) + (k === 'guideCfg.tmStart' ? ' playing:' + (await R('G.tm.playing')) : ''));
    await R('if(G.on)stopTour();""');
  }
  ok('[rows] Remember where the visitor stopped Off (a real click): no offer, On: the offer back; Tell me more Off: no chip; each new row\'s ▸ (a real click) shows its demo, its part ringed (the chip; the deep part playing; the hero\'s offer)',
    !off1 && on1 && !tmOff && demos.every(x => /\|found\|/.test(x)) && /\.gtmore/.test(demos[0]) && /playing:true/.test(demos[1]) && /gcont/.test(demos[2]), { off1, on1, tmOff, demos });
  tsec('rows');

  // ---------- 9. [blocked] storage blocked: the app works, the place lives for the page's life, the Editor says so ----------
  await ctx.close(); ctx = await b.newContext({ viewport: { width: 1500, height: 1000 } });
  await ctx.addInitScript(() => { Object.defineProperty(window, 'localStorage', { configurable: true, get() { throw new Error('storage blocked'); } }); });
  p = await ctx.newPage(); watch(p); const e0 = errs.length;
  await load(); await R(base); await openPin('torreao'); await heroStart(); await jump('a:t_text'); await until('!!ph.querySelector(".gtmore")', 3000);
  await p.click('#ph .gtmore'); const bl1 = await until('G.tm.playing', 15000);
  await jump('a:t_gal'); await R('stopTour();S.view="card";S.stop="full";renderPhone(false);""');
  const bl2 = JSON.parse(await R('S.tab="guide";S.op={};S.op.gt1=true;S.op["gt:ways"]=true;S.op.s7resume=true;S.ui.level=3;renderEditor();JSON.stringify({cont:(scr.querySelector(".hero .gcont")||{}).textContent||"",kn:(ed.querySelector(".svnow")||{}).textContent||""})'));
  await load(); await R(base); await openPin('torreao'); const bl3 = await R('!!scr.querySelector(".hero .gcont")');
  ok('[blocked] storage blocked: no error, Tell me more works, the Editor says the browser blocks storage (no place for the next visit); after a reload nothing is offered, still no error',
    errs.length === e0 && bl1 && !bl2.cont && /blocks storage/.test(bl2.kn) && !bl3, { bl1, bl2, bl3, errs: errs.slice(e0, e0 + 3) });
  tsec('blocked');

  ok('no page errors', errs.length === 0, errs.slice(0, 5));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS'); console.log('TIME ' + Math.round((Date.now() - T0) / 1000) + ' s');
  await b.close(); process.exit(fails ? 1 : 0);
})();
