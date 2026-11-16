// test9s (Playwright + the installed Edge): node test9s.js -> PASS/FAIL per check. Guidance round 15, the design pass of the AI guide:
// every surface the visitor sees from the guide, opened by the app's own functions or real clicks on a 390 px phone (844 and the shorter
// 712), light and dark, Ink glass and Paper glass (the panel's four Looks where the panel shows), at each dock height where it shows, and
// the rules measured on it (tests/g15lib.js): 44 px hit areas, 4.5:1 text against what is really behind it, 0 px over the words being
// told, inside the phone (edges, the notch, the dock), one focal point. Here: the ✦ and its menu with Azu beside it, Azu on the wall,
// the panel's Looks at Mini, Compact and Open, the captions and the focus, every bubble form, Ask Azu, the first pages and Welcome back.
// The beat cards are test9sb.js. CAP=<dir> saves a capture of each state (<nn>_<surface>_<look>_<height>_<theme>.png).
// MUTATE=1 node test9s.js makes the panel's question chips 36 px tall and draws Azu 170 px to the left, over the told words: the checks
// named [44 px] and [0 px] turn red. One table, three files (each under 300 s): this file 01-09, test9sc.js 10-22 and 29-33, test9sd.js 23-28.
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
  const box = async sel => p.locator('#ph ' + sel).first().boundingBox();
  const tap = async sel => { const r = await box(sel); if (!r) return false; await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return true; };
  /* the phone at rest: Azu's hop, the block's scroll, the bubble's and the beat card's place unchanged for three samples (at most 5 s) */
  const settle = async () => { let last = null, same = 0; const t0 = Date.now(); while (Date.now() - t0 < 5000 && same < 3) { const y = await R('JSON.stringify([".mascot",".gdbub",".gdbeat",".gdock",".azbub",".gbubble",".gspot",".gdpages",".gdock .gd-thread"].map(function(s){var e=ph.querySelector(s);if(!e)return 0;var r=e.getBoundingClientRect();return Math.round(r.top)+","+Math.round(r.left)+","+Math.round(r.height)}).concat([Math.round(scr.scrollTop),document.getAnimations().filter(function(a){var t=a.effect&&a.effect.getTiming&&a.effect.getTiming();return a.playState==="running"&&t&&t.iterations!==Infinity&&a.effect.target&&ph.contains(a.effect.target)}).length]))'); same = y === last ? same + 1 : 0; last = y; await W(150); } };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  await R(require('./g15lib.js'));
  await R('window.__g0=JSON.stringify(S.guidance)');
  if (process.env.MUTATE) { await R('var __m=document.createElement("style");__m.textContent="#ph .gdock .gd-chips [data-chip]{height:36px!important;min-height:0!important;max-height:36px!important}#ph .mascot{translate:-170px 24px!important}";document.head.appendChild(__m)'); console.log('MUTATION: the panel question chips 36 px tall; Azu drawn 170 px to the left, over the told block words'); }

  /* the state every surface starts from: a 390 px phone, the device voice muted (its words go on by time), the guidance quiet */
  const base = (o) => `SCREENS.phone=[390,${o.H || 844}];if(G.gd.pages)gdPagesEnd("test",false);if(G.on)stopTour();if(G.moment)azMomentEnd(false);ph.querySelectorAll(".azbub").forEach(function(x){x.remove()});gdBeatClose();gdStop("test");S.guidance=JSON.parse(window.__g0);
    S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=${!!o.reduce};S.dark=${o.theme === 'dark'};S.cardCfg.mode="${o.theme === 'dark' ? 'dark' : 'light'}";
    S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.miniCtl="pausestop";S.guideCfg.captions=true;S.guideCfg.pWords="prompter";S.guideCfg.pBars=true;S.guideCfg.pSpeed=true;S.guideCfg.chips=true;
    S.guideCfg.speed=1;S.vis.speed=null;S.guideCfg.music=false;S.vis.music=null;S.ui.rowDemo=false;S.ui.gdSR=false;S.m.solemn=false;S.vis.aud="explorer";S.vis.len="";S.guideCfg.life="lively";
    S.guideCfg.gbtn="both";S.guideCfg.mascot=true;S.guideCfg.look="${o.look || 'ink'}";S.guideCfg.cue="spotlight";S.guideCfg.zoom="zoom";S.agentCfg.trace=false;S.agentCfg.voiceOut=false;
    G.gd.demo=false;gdChip(null);G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};S.settingsOpen=false;G.fab.menu=false;G.askOpen=false;G.talk="idle";G.busy=false;
    S.guidance.on=true;S.guidance.kinds.forEach(function(k){k.on=false});G.gd.manual=true;gdResetVisitor();G.gd.movedAt=null;G.gd.touchAt=null;`;
  const wall = 'setWall("ribeira");S.camZoom=1;S.view="card";S.stop="closed";renderAll(false);';
  const card = 'S.view="card";S.stop="full";renderAll(false);';
  /* the tour started at a step (as Resume does), never jumped to it after the start: the start's own placing would race the step's */
  const tourAt = (id, h) => `${card}startTour({id:"${id}"});S.dockH="${h}";renderGuide();`;
  const show = (id, o) => `gdStart(gdItemNow("${id}"),"test",${o || '{pull:true,ms:60000}'});`;

  /* the surfaces: each opened by the app's own functions (and real clicks where a visitor would tap), then measured */
  const S_ = [];
  const add = (nn, name, variants, open) => S_.push({ nn, name, variants, open });
  /* the states of a surface: with CAP every Look, height and theme at 844 and light at 712; as a test (under 300 s a file) light for every
     Look, dark for the first Look, and 712 for the first two Looks */
  const V = (looks, heights, themes, extra) => { const out = []; for (const H of [844, 712]) looks.forEach((look, li) => { for (const h of heights) for (const theme of (H === 712 ? ['light'] : themes)) { if (!CAP && ((theme === 'dark' && li > 0) || (H === 712 && li > 1))) continue; out.push(Object.assign({ look, h, theme, H }, extra || {})); } }); return out; };
  const LT = ['light', 'dark'];
  // 1. the ✦ and its menu, Azu beside it, on the card (past the hero) and on the wall
  add('01', 'gbtn-card', V(['-'], ['-'], LT), async o => { await R(base(o) + card + 'scr.scrollTop=900;scr.dispatchEvent(new Event("scroll"));'); await W(500); return R('!!ph.querySelector(".gbtn")'); });
  add('02', 'gbtn-menu-card', V(['-'], ['-'], LT), async o => { await R(base(o) + card + 'scr.scrollTop=900;scr.dispatchEvent(new Event("scroll"));'); await W(500); await tap('.gbtn'); return until('!!ph.querySelector(".gbmenu")', 1500); });
  add('03', 'gbtn-wall', V(['-'], ['-'], LT), async o => { await R(base(o) + wall); await W(400); return R('!!ph.querySelector(".gbtn")'); });
  add('04', 'gbtn-menu-wall', V(['-'], ['-'], LT), async o => { await R(base(o) + wall); await W(400); await tap('.gbtn'); return until('!!ph.querySelector(".gbmenu")', 1500); });
  // 1b. Azu on the wall beside a marker (Tap a marker: a Tip with its ring) and Azu on the card (pointing at the block told, the panel at Compact)
  add('05', 'azu-wall-marker', V(['-'], ['-'], LT), async o => { await R(base(o) + wall + 'S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});' + show('tapmarker')); await W(1300); return R('!!ph.querySelector(".gdbub")'); });
  add('06', 'azu-card', V(['ink', 'paper'], ['mini', 'compact'], LT), async o => { await R(base(o) + tourAt('g2', o.h)); await W(1300); return R('!!ph.querySelector(".mascot")'); });
  // 2. the panel's four Looks at Mini, Compact and Open (Peek), a tour playing
  add('07', 'panel', V(['ink', 'paper', 'player', 'prompter'], ['mini', 'compact', 'open'], LT), async o => { await R(base(o) + tourAt(o.h === 'mini' ? 'g4' : 'g5', o.h === 'open' ? 'peek' : o.h)); await W(900); return R(o.h === 'compact' ? '!!ph.querySelector(".gbtn")' : '!!ph.querySelector(".gdock")'); });
  // 3. the captions (the block's read-along and the panel's line) and the focus on a block (Spotlight, Ring) and on a spot in a picture
  add('08', 'focus-block-spotlight', V(['ink', 'paper'], ['mini'], LT), async o => { await R(base(o) + 'S.guideCfg.cue="spotlight";' + tourAt('g2', 'mini')); await W(1200); return R('!!ph.querySelector(".gspot.on")'); });
  add('09', 'focus-block-ring', V(['ink', 'paper'], ['mini'], LT), async o => { await R(base(o) + 'S.guideCfg.cue="ring";' + tourAt('g2', 'mini')); await W(1200); return R('!!ph.querySelector(".gspot.on")'); });
  add('10', 'focus-spot-zoom', V(['ink'], ['mini'], LT), async o => { await R(base(o) + 'S.guideCfg.zoom="zoom";' + tourAt('g3', 'mini')); return until('!!G.fc&&G.fc.cue>=0', 12000); });
  add('11', 'focus-spot-light', V(['ink'], ['mini'], LT), async o => { await R(base(o) + 'S.guideCfg.zoom="spot";' + tourAt('g3', 'mini')); return until('!!G.fc&&G.fc.cue>=0', 12000); });
  // 3b. the guidance's Focus on a marker: Glint, Ring, Spotlight (Tap a marker made Essential for the Spotlight)
  for (const [nn, f] of [['12', 'glint'], ['13', 'ring'], ['14', 'spot']]) add(nn, 'focus-marker-' + f, V(['-'], ['-'], LT), async o => { await R(base(o) + wall + `S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});var t=S.guidance.items.tapmarker;t.focus="${f}";if("${f}"==="spot")t.essential=true;` + show('tapmarker')); await W(1300); return R('!!ph.querySelector(".gdbub")'); });
  // 4. every bubble form: Tip (the opening beside Azu at the ✦; Ask about the view at the ✦; The ✦ on the card), Invitation (a tour of the
  // point; the treasure hunt), Remark (the point in the middle), Moment (Azu's water break on the card), Mascot Off's plain bubble
  add('15', 'tip-opening-wall', V(['-'], ['-'], LT), async o => { await R(base(o) + wall + 'S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});' + show('opening')); await W(1100); return R('!!ph.querySelector(".gdbub")'); });
  add('16', 'tip-askview-wall', V(['-'], ['-'], LT), async o => { await R(base(o) + wall + 'S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});' + show('askview')); await W(1100); return R('!!ph.querySelector(".gdbub")'); });
  add('17', 'tip-gbhint-card', V(['-'], ['-'], LT), async o => { await R(base(o) + card + 'scr.scrollTop=900;scr.dispatchEvent(new Event("scroll"));'); await W(400); await R('S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});' + show('gbhint')); await W(1100); return R('!!ph.querySelector(".gdbub")'); });
  add('18', 'invitation-tourof-wall', V(['-'], ['-'], LT), async o => { await R(base(o) + wall + 'S.guidance.kinds.forEach(function(k){k.on=k.id==="content"});' + show('tourof')); await W(1100); return R('!!ph.querySelector(".gdbub")'); });
  add('19', 'invitation-hunt-wall', V(['-'], ['-'], LT), async o => { await R(base(o) + 'S.vis.aud="family";' + wall + 'S.guidance.kinds.forEach(function(k){k.on=k.id==="fun"});' + show('hunt')); await W(1100); return R('!!ph.querySelector(".gdbub")'); });
  add('20', 'remark-wall', V(['-'], ['-'], LT), async o => { await R(base(o) + wall + 'S.guidance.kinds.forEach(function(k){k.on=k.id==="content"});' + show('inview')); await W(1100); return R('!!ph.querySelector(".gdbub")'); });
  add('21', 'moment-card', V(['-'], ['-'], LT), async o => { await R(base(o) + card + 'scr.scrollTop=900;scr.dispatchEvent(new Event("scroll"));'); await until('!!ph.querySelector(".azfab")', 3000); await W(300); await R('azMoment("water",null)'); return until('!!ph.querySelector(".azbub")', 10000); });
  add('22', 'mascotoff-wall', V(['-'], ['-'], LT), async o => { await R(base(o) + 'S.guideCfg.mascot=false;' + wall + 'S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});' + show('tapmarker')); await W(1100); return R('!!ph.querySelector(".gdbub")'); });
  // 6. Ask Azu: typed (the keyboard at Open), by voice (the talk button's states), an answer with its link cards and the trace, Show me
  add('23', 'ask-typed', V(['ink', 'paper'], ['open'], LT), async o => { await R(base(o) + tourAt('g5', 'peek')); await W(500); await p.locator('#ph #gq').fill('Was there an opera?'); return R('!!ph.querySelector("#gq")'); });
  for (const [nn, st] of [['24', 'listening'], ['25', 'thinking'], ['26', 'notheard']]) add(nn, 'ask-mic-' + st, V(['ink', 'paper'], ['open'], LT), async o => { await R(base(o) + tourAt('g5', o.h === 'open' ? 'peek' : 'mini') + 'pauseTour();' + (st === 'thinking' ? 'G.busy=true;' : `G.talk="${st}";`) + 'renderGuide();'); await W(400); return R('!!ph.querySelector(".gd-talk")'); });
  add('27', 'ask-answer-links', V(['ink', 'paper'], ['open'], LT), async o => { await R(base(o) + tourAt('g5', 'peek') + 'S.agentCfg.trace=true;AG.mock={calls:[{name:"show_links",args:{items:[{target_id:"b:b_ba",reason:"Before 1755"},{target_id:"b:b_gal",reason:"Three centuries"}]}}],text:"The palace had an opera house, opened in 1755 [src:b:b_fun]."};S.agentCfg.provider="mock";applyProvider();'); await W(300); await p.locator('#ph #gq').fill('Was there an opera?'); await p.locator('#ph #gq').press('Enter'); return until('!!ph.querySelector(".aglink")', 4000); });
  add('28', 'ask-showme', V(['ink', 'paper'], ['open'], LT), async o => { await R(base(o) + tourAt('g5', 'peek') + 'S.agentCfg.provider="offline";applyProvider();'); await W(300); await p.locator('#ph #gq').fill('How do I pause the guide?'); await p.locator('#ph #gq').press('Enter'); return until('!!ph.querySelector(".gdhowc,[data-gdhow]")', 4000); });
  // 7. the first pages (Start the app › A first run) and Welcome back
  const launch = async kind => { await R(`gdLaunch("${kind}")`); await W(150); };
  const pgTap = async sel => { await until(`!!ph.querySelector('.gdpages:not(.gd-out) ${sel.replace(/'/g, "\\'")}')`, 2500); await tap('.gdpages:not(.gd-out) ' + sel); await W(150); };
  const pgAt = (pg, ms) => until(`(function(){var L=ph.querySelector(".gdpages:not(.gd-out)");return!!L&&L.dataset.page+(L.dataset.cam?":"+L.dataset.cam:"")==="${pg}"})()`, ms || 2500);
  const pagesBase = o => base(o) + 'S.aud.where="start";S.aud.seen=false;G.gd.camPerm="ask";S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});gdGroupById("card").on=false;S.guidance.items.gbhint.on=false;S.guidance.items.askview.on=false;' + wall;
  add('29', 'pages-hello', V(['-'], ['-'], LT), async o => { await R(pagesBase(o)); await launch('first'); return pgAt('hello'); });
  add('30', 'pages-who', V(['-'], ['-'], LT), async o => { await R(pagesBase(o)); await launch('first'); await pgAt('hello'); await pgTap('[data-gdpg="next"]'); return pgAt('aud'); });
  add('31', 'pages-camera', V(['-'], ['-'], LT), async o => { await R(pagesBase(o) + 'S.aud.where="card";'); await launch('first'); await pgAt('hello'); await pgTap('[data-gdpg="next"]'); return pgAt('camera'); });
  add('32', 'pages-camera-refused', V(['-'], ['-'], LT), async o => { await R(pagesBase(o) + 'S.aud.where="card";'); await launch('first'); await pgAt('hello'); await pgTap('[data-gdpg="next"]'); await pgAt('camera'); await pgTap('[data-gdpg="camera"]'); await pgAt('camera:prompt'); await pgTap('[data-gdpg="sys:deny"]'); return pgAt('camera:refused'); });
  add('33', 'welcome-back', V(['-'], ['-'], LT), async o => { await R(pagesBase(o) + 'G.gd.camPerm="allowed";S.aud.seen=true;S.aud.where="card";'); await launch('first'); await pgAt('hello'); await pgTap('[data-gdpg="skip"]'); if (await pgAt('camera:prompt', 1500)) await pgTap('[data-gdpg="sys:allow"]'); await until('!ph.querySelector(".gdpages:not(.gd-out)")', 2500); await R('gdAdvance(1000);gdStop("test");'); await launch('later'); await R('gdAdvance(1000)'); return until('!!G.gd.cur&&G.gd.cur.item.id==="opening"&&!!ph.querySelector(".gdbub")', 4000); });

  /* run each surface in each variant; measure; capture */
  /* three files of one table: test9s the ✦, Azu, the panel, the captions and the focus on a block (01-09); test9sc the focus on a spot and on a
     marker, the bubbles and the first pages (10-22, 29-33); test9sd Ask Azu (23-28); CAP or ALL=1 runs every surface */
  // round 21 (21.10: MUTATE=1 on PART=c caught nothing, its surfaces have no panel chips nor told words): PART=c's own mutation: the
  // bubbles' and the pages' words faint (2:1) and their buttons 30 px tall
  if (process.env.MUTATE && process.env.PART === 'c') { await R('var __mc=document.createElement("style");__mc.textContent="#ph .gdbub,#ph .gdbub *,#ph .gdpages,#ph .gdpages *{color:#C9C4BC!important}#ph .gdbub .gdcta,#ph .gdpages button{height:30px!important;min-height:0!important;max-height:30px!important}";document.head.appendChild(__mc)'); console.log('MUTATION (PART=c): the bubbles\' and the pages\' words faint, their buttons 30 px'); }
  const PART = process.env.PART || '1', inPart = nn => process.env.ALL || CAP ? true : PART === '1' ? +nn <= 9 : PART === 'c' ? (+nn >= 10 && +nn <= 22) || +nn >= 29 : +nn >= 23 && +nn <= 28;
  const only = (process.env.ONLY || '').split(',').filter(Boolean);
  if (CAP) fs.mkdirSync(CAP, { recursive: true });
  const results = [];
  for (const s of S_) {
    if (only.length ? !only.some(x => s.nn === x || s.name.startsWith(x)) : !inPart(s.nn)) continue;
    const rows = [];
    for (const v of s.variants) {
      if (process.env.QUICK && v.H === 712) continue;
      let ready = false; try { ready = await s.open(v); } catch (e) { ready = false; v.err = e.message; }
      await settle();
      const a = await R('G15.audit()');
      const tag = `${s.nn}_${s.name}_${v.look}_${v.h}_${v.theme}${v.H === 712 ? '-712' : ''}`;
      if (CAP) { const r = await p.locator('#ph').boundingBox(); await p.screenshot({ path: path.join(CAP, tag + '.png'), clip: { x: r.x, y: r.y, width: r.width, height: r.height } }); }
      rows.push({ tag, v, ready, a });
    }
    results.push({ s, rows });
    const bad = (f) => rows.filter(f).map(r => r.tag.replace(/^\d+_[^_]+_/, ''));
    const small = rows.flatMap(r => r.a.targets.filter(t => !t.ok).map(t => r.tag.slice(3) + ' ' + t.el + ' ' + t.w + '×' + t.h + ' hit ' + t.hit));
    const low = rows.flatMap(r => r.a.text.filter(t => t.cr < 4.5).map(t => r.tag.slice(3) + ' "' + t.t + '" ' + t.cr + (t.glass ? ' (glass)' : '')));
    const over = rows.flatMap(r => r.a.words.over.map(x => r.tag.slice(3) + ' ' + x.el + ' ' + x.px + ' px²'));
    const out = rows.flatMap(r => r.a.inside.map(x => r.tag.slice(3) + ' ' + x.el + ' ' + x.why));
    const foc = rows.filter(r => r.a.focal.length > 1).map(r => r.tag.slice(3) + ' ' + r.a.focal.join(' + '));
    const notReady = bad(r => !r.ready);
    const nm = `${s.nn} ${s.name} (${rows.length} states)`;
    ok(`[open] ${nm}: the surface opened by the app's own functions in every state`, !notReady.length, notReady);
    ok(`[44 px] ${nm}: every guide target's hit area ≥ 44 × 44 (${rows.reduce((n, r) => n + r.a.targets.length, 0)} targets)`, !small.length, small.slice(0, 12));
    ok(`[4.5:1] ${nm}: every guide text ≥ 4.5:1 against what is behind it (${rows.reduce((n, r) => n + r.a.text.length, 0)} texts)`, !low.length, low.slice(0, 12));
    ok(`[0 px] ${nm}: nothing over the words being told (${rows.reduce((n, r) => n + r.a.words.n, 0)} word boxes)`, !over.length, over.slice(0, 12));
    ok(`[inside] ${nm}: inside the phone at 390 × 844 and 390 × 712 (edges, notch, dock)`, !out.length, out.slice(0, 12));
    ok(`[one focus] ${nm}: at most one focal point`, !foc.length, foc.slice(0, 8));
    tsec(s.nn);
  }
  if (CAP) fs.writeFileSync(path.join(CAP, 'g15_9s.json'), JSON.stringify(results.map(x => ({ nn: x.s.nn, name: x.s.name, rows: x.rows.map(r => ({ tag: r.tag, ready: r.ready, a: r.a })) })), null, 1));
  // Reduce Motion: the same surfaces still read (their words there, no motion needed): a sample of each kind under the preview bar's Motion
  const rm = [];
  for (const nn of ['05', '07', '15', '20', '21', '27']) { const s = S_.find(x => x.nn === nn); if (!s || (only.length ? !only.includes(nn) : !inPart(nn))) continue; const v = Object.assign({}, s.variants[0], { reduce: true, h: s.variants[0].h === 'compact' ? 'mini' : s.variants[0].h }); await s.open(v); await W(200);
    rm.push([s.name, await R('var t=G15.text();[ph.classList.contains("reduce"),t.length,t.filter(function(x){return x.cr<4.5}).length,[].filter.call(ph.querySelectorAll(".gdbub,.gdock,.gbtn,.azbub,.mascot"),function(e){var a=getComputedStyle(e);return a.animationName!=="none"&&parseFloat(a.animationDuration)>0&&a.animationIterationCount==="infinite"}).length]')]);
    if (CAP) { const r = await p.locator('#ph').boundingBox(); await p.screenshot({ path: path.join(CAP, `${nn}_${s.name}_${v.look}_${v.h}_light-reduce.png`), clip: { x: r.x, y: r.y, width: r.width, height: r.height } }); } }
  if (rm.length) ok(`[reduce] Reduce Motion: ${rm.map(x => x[0]).join(', ')} still read (their words there at 4.5:1, nothing looping)`, rm.every(x => x[1][0] && x[1][1] > 0 && x[1][2] === 0 && x[1][3] === 0), rm);
  await R('if(G.on)stopTour();gdStop("test");SCREENS.phone=[360,820];S.dark=false;S.cardCfg.mode="light";S.guidance=JSON.parse(window.__g0);renderAll(false)');
  ok('No page error', errs.length === 0, errs.slice(0, 5));
  tsec('end');
  console.log(fails ? `FAILED ${fails}` : 'ALL PASS');
  await b.close(); process.exit(fails ? 1 : 0);
})().catch(e => { console.error(e); process.exit(2); });
