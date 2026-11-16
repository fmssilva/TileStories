// test9sb (Playwright + the installed Edge): node test9sb.js -> PASS/FAIL per check. Guidance round 15, the design pass's second half:
// every beat card of Games on the tour, played by the tour itself (the plan, its seams, the app's own scene helpers and real taps), on a
// 390 px phone (844, and 712), light and dark, Ink glass and Paper glass, at Mini and Compact (where a beat card shows): the quiz's promise
// and its strip, the quiz and its verdict, Guess the year (the slider, the two pins), Don't say it! (the announce, the word card, the
// result), Find it on the wall (the seek ring, the hint, Azu at the edge with its arrow), the breather and the cookie; the rules of
// tests/g15lib.js on each (44 px, 4.5:1, 0 px over the words being told, inside the phone, one focal point). CAP=<dir> saves each state.
// MUTATE=1 node test9sb.js makes a beat card's chips 36 px tall: the checks named [44 px] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
const CAP = process.env.CAP || '';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 1000 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 900) : '')); };
  /* the phone at rest: Azu, the beat card, the strip and the scroll unchanged for three samples (at most 3 s; the beat's own clock runs on) */
  const settle = async () => { let last = null, same = 0; const t0 = Date.now(); while (Date.now() - t0 < 3000 && same < 3) { const y = await R('JSON.stringify([".mascot",".gdbeat",".gdstrip",".gdock",".azmoment"].map(function(s){var e=ph.querySelector(s);if(!e)return 0;var r=e.getBoundingClientRect();return Math.round(r.top)+","+Math.round(r.left)+","+Math.round(r.height)}).concat([Math.round(scr.scrollTop),document.getAnimations().filter(function(a){var t=a.effect&&a.effect.getTiming&&a.effect.getTiming();return a.playState==="running"&&t&&t.iterations!==Infinity&&a.effect.target&&ph.contains(a.effect.target)}).length]))'); same = y === last ? same + 1 : 0; last = y; await W(120); } };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  await R(require('./g15lib.js'));
  if (process.env.MUTATE) { await R('var __m=document.createElement("style");__m.textContent="#ph .gdbeat .bt-chip,#ph .gdstrip .bt-chip{height:36px!important;min-height:0!important;max-height:36px!important}";document.head.appendChild(__m)'); console.log('MUTATION: a beat card\'s chips 36 px tall'); }
  const base = o => `SCREENS.phone=[390,${o.H}];S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.reduce=false;S.dark=${o.theme === 'dark'};S.cardCfg.mode="${o.theme === 'dark' ? 'dark' : 'light'}";
    S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="${o.h}";S.guideCfg.look="${o.look}";S.guideCfg.captions=true;S.guideCfg.pWords="prompter";S.ui.rowDemo=false;S.ui.gdSR=false;S.guideCfg.gbtn="both";S.guideCfg.mascot=true;S.vis.speed=null;`;
  const keep = async o => { await R('if(G.gd.sc12){gd12Done([true,"",""])}'); await R(base(o) + 'gd12Keep();'); };
  const dockTo = h => `if(S.dockH!=="${h}"){S.dockH="${h}";renderGuide();gdBeatPlace()}`;
  /* each game: how it is opened, then its stages (each waited for, measured and captured as the visitor sees it) */
  const G_ = [
    { nn: '40', name: 'quiz', open: o => `gd12Only(["quizpromise","quizfinale"]);startTour();`, stages: [
      ['promise', 'gd12Kind("promise")', 30000],
      ['strip', '!!G.gd.tail&&G.i>=1&&!!ph.querySelector(".gdstrip")', 20000],
      ['quiz', null, 0, `G.gd.tail&&gdTailEnd();gd12Seam(gd12Fin());`, '!!ph.querySelector(".gdbeat .bt-choice")', 25000],
      ['verdict', null, 0, `var B=G.gd.bt,q=gdQuizQ(B,B.k);gd12Tap('[data-gdbt="ans"][data-i="'+((+q.ok+1)%q.a.length)+'"]');`, '!!ph.querySelector(".gdbeat .bt-verdict")', 4000]] },
    { nn: '41', name: 'guess', open: o => `gd12Only(["guessyear"]);startTour();`, stages: [
      ['slider', null, 0, `var b=G.gd.plan&&gdPlanBeat("guessyear");b&&gd12Seam(b.after);`, '!!ph.querySelector(".gdbeat .bt-range")', 12000, 'G.gd.plan'],
      ['pins', null, 0, `var r=ph.querySelector(".gdbeat .bt-range");r.value=1620;r.dispatchEvent(new Event("input",{bubbles:true}));gd12Tap('[data-gdbt="tell"]');`, '!!G.on&&!G.gd.bt', 6000],
      ['pins2', null, 0, `gd12Seam(S.guideSteps[G.i].id);`, '!!ph.querySelector(".gdbeat .bt-pin.ans")', 20000]] },
    { nn: '42', name: 'dontsay', open: o => `gd13Only(["dontsay"]);startTour();`, stages: [
      ['announce', null, 0, `G.gd.plan&&gd13Word();`, 'gd12Kind("word")', 12000, 'G.gd.plan'],
      ['wordcard', '!!G.gd.ds&&G.gd.ds.on&&!G.gd.bt', 30000],
      ['result', null, 0, `G.gd.plan.wordScore={word:G.gd.ds?G.gd.ds.word:"palace",caught:1,n:2,falses:0};gdBeatPresent({item:S.guidance.items.dontsay,react:true});`, 'G.gd.bt&&G.gd.bt.kind==="wordres"&&!!ph.querySelector(".gdbeat")', 4000]] },
    { nn: '43', name: 'find', open: o => `gd13Only(["findwall"]);startTour();`, stages: [
      ['seek', null, 0, `var b=G.gd.plan&&gdPlanBeat("findwall");b&&gd12Seam(b.after);`, '!!G.gd.find&&!!ph.querySelector(".gdseek")&&S.stop==="closed"', 12000, 'G.gd.plan'],
      ['hint', null, 0, `G.gd.find.clock=Math.max(G.gd.find.clock,9950);`, 'G.gd.find&&G.gd.find.hinted', 4000],
      ['edge', null, 0, `var F=G.gd.find;gd13Aim(F.p,-160,ph.offsetHeight*0.4);gd12Tap('.gdbeat [data-gdbt="giveup"]');`, '!!ph.querySelector(".mascot.gd-findto")', 4000]] },
    { nn: '44', name: 'breather', open: o => `gd12Only(["water"]);S.introSeen=false;startTour();`, stages: [
      ['ask', null, 0, `var b=G.gd.plan&&gdPlanBeat("water");b&&gd12Seam(b.after);`, '!!ph.querySelector(\'.gdbeat [data-gdbt="goon"]\')', 15000, 'G.gd.plan']] },
    { nn: '45', name: 'cookie', open: o => `gd12Only(["cookie"]);S.introSeen=false;startTour();`, stages: [
      ['ask', null, 0, `var b=G.gd.plan&&gdPlanBeat("cookie");b&&gd12Seam(b.after);`, '!!ph.querySelector(\'.gdbeat [data-gdbt="goon"]\')', 15000, 'G.gd.plan']] }];
  const VARS = [];
  for (const look of ['ink', 'paper']) for (const theme of ['light', 'dark']) for (const h of ['mini', 'compact']) VARS.push({ look, theme, h, H: 844 });
  VARS.push({ look: 'ink', theme: 'light', h: 'mini', H: 712 }, { look: 'paper', theme: 'light', h: 'compact', H: 712 });
  const only = (process.env.ONLY || '').split(',').filter(Boolean);
  if (CAP) fs.mkdirSync(CAP, { recursive: true });
  const results = [];
  for (const g of G_) {
    if (only.length && !only.some(x => g.nn === x || g.name === x)) continue;
    const rows = [];
    /* as a test (under 300 s): Ink glass light at Mini, Paper glass light at Compact, Ink glass dark at Mini and Ink glass at 712; with CAP all ten */
    const TV = CAP ? VARS : [VARS[0], VARS[5], VARS[2], VARS[8]];
    for (const v of TV) {
      await keep(v); await R(g.open(v));
      for (const st of g.stages) {
        const [sname, waitFor, ms, act, waitAfter, ms2, pre] = st;
        let ready;
        if (act) { if (pre) await until(pre, 4000); await R(act); ready = await until(waitAfter, ms2); }
        else ready = await until(waitFor, ms);
        await R(dockTo(v.h)); await settle();
        const a = await R('G15.audit()');
        const tag = `${g.nn}_beat-${g.name}-${sname}_${v.look}_${v.h}_${v.theme}${v.H === 712 ? '-712' : ''}`;
        if (CAP) { const r = await p.locator('#ph').boundingBox(); await p.screenshot({ path: path.join(CAP, tag + '.png'), clip: { x: r.x, y: r.y, width: r.width, height: r.height } }); }
        rows.push({ tag, ready, a, stage: sname });
      }
    }
    results.push({ g, rows });
    const nm = `${g.nn} ${g.name} (${rows.length} states: ${g.stages.map(s => s[0]).join(', ')})`;
    const lst = (f) => rows.flatMap(f);
    const notReady = rows.filter(r => !r.ready).map(r => r.tag.slice(3));
    ok(`[open] ${nm}: each stage reached by the tour itself`, !notReady.length, notReady);
    const small = lst(r => r.a.targets.filter(t => !t.ok).map(t => r.tag.slice(3) + ' ' + t.el + ' ' + t.w + '×' + t.h + ' hit ' + t.hit));
    ok(`[44 px] ${nm}: every guide target's hit area ≥ 44 × 44`, !small.length, small.slice(0, 12));
    const low = lst(r => r.a.text.filter(t => t.cr < 4.5).map(t => r.tag.slice(3) + ' "' + t.t + '" ' + t.cr + (t.glass ? ' (glass)' : '')));
    ok(`[4.5:1] ${nm}: every guide text ≥ 4.5:1 against what is behind it`, !low.length, low.slice(0, 12));
    const over = lst(r => r.a.words.over.map(x => r.tag.slice(3) + ' ' + x.el + ' ' + x.px + ' px²'));
    ok(`[0 px] ${nm}: no beat card, strip or Azu over the words being told`, !over.length, over.slice(0, 12));
    const out = lst(r => r.a.inside.map(x => r.tag.slice(3) + ' ' + x.el + ' ' + x.why));
    ok(`[inside] ${nm}: inside the phone at 390 × 844 and 390 × 712`, !out.length, out.slice(0, 12));
    const foc = rows.filter(r => r.a.focal.length > 1).map(r => r.tag.slice(3) + ' ' + r.a.focal.join(' + '));
    ok(`[one focus] ${nm}: at most one focal point (the beat card alone)`, !foc.length, foc.slice(0, 8));
    tsec(g.nn);
  }
  await R('if(G.gd.sc12){gd12Done([true,"",""])}');
  if (CAP) fs.writeFileSync(path.join(CAP, 'g15_9sb.json'), JSON.stringify(results.map(x => ({ nn: x.g.nn, name: x.g.name, rows: x.rows })), null, 1));
  await R('if(G.on)stopTour();SCREENS.phone=[360,820];S.dark=false;S.cardCfg.mode="light";S.vis.speed=null;renderAll(false)');
  ok('No page error', errs.length === 0, errs.slice(0, 5));
  tsec('end');
  console.log(fails ? `FAILED ${fails}` : 'ALL PASS');
  await b.close(); process.exit(fails ? 1 : 0);
})().catch(e => { console.error(e); process.exit(2); });
