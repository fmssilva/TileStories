// g6 checks, part B (Playwright + the installed Edge): node test9jb.js -> PASS/FAIL per check
// Guidance, round 6 (_3.8 section 8): every button of the item form pressed once by a real click (each field, select and number), every
// other value pressed in the page as the self-test does, and the form's own buttons (a variant, an audience's line, a returning line, a
// chip's ×, ▸ on a line). Part A (test9ja.js) holds the rest of round 6's checks. Split from test9j.js on 2026-11-09 (level 3 after
// guidance round 10: the one file reached its 600 s kill on a busy machine). It names no mutation: its checks are counts of values set,
// and part A's mutation covers round 6's rules.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const t00 = Date.now();
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  const lap = n => { if (process.env.TIMING) console.log('TIME ' + n + ' ' + Math.round((Date.now() - t00) / 1000) + ' s'); };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('window.__g0=JSON.stringify(S.guidance);window.__tx0=JSON.stringify(S.tx)');
  const base = 'S.guidance=JSON.parse(window.__g0);S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.ui.gdSR=false;S.guidance.items.gbhint.on=false;gdGroupById("inview").on=false;S.guideCfg.gbtn="both";S.guideCfg.mascot=true;G.gd.demo=false;gdChip(null);';
  const fresh = async (extra) => { await R(base + 'if(G.on)stopTour();G.cam.open=false;S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);G.gd.manual=true;gdStop("test");gdResetVisitor();G.gd.movedAt=null;G.gd.onWall=false;gdAdvance(250);' + (extra || '')); await W(120); };
  const show = (id, o) => `gdStart(gdItemNow("${id}"),"test",${o || '{pull:true,ms:60000}'});`;
  // round 14: an item's groups are folded: an item opened here shows its groups open too (each by its id, as the author opens them)
  const editor = async (ops) => { await R(`S.tab="guide";S.ui.level=1;S.op=${JSON.stringify(ops)};Object.keys(S.op).forEach(function(k){var m=/^gd:i:(.+)$/.exec(k),it=m&&S.op[k]&&S.guidance.items[m[1]];if(it)gdFormOf(it).groups.forEach(function(g){if(g)S.op["gd:f:"+m[1]+":"+g.k]=true})});renderEditor()`); await W(120); };

  lap('before 1');
  // ---------- 10. every button of the item form, pressed once by a real click ----------
  await R(base); const op = { gt4: true, 'gd:app': true, 'gd:g:start': true, 'gd:i:tapmarker': true };
  await editor(op);
  const ctl = await R('var o=[];ed.querySelectorAll(\'[data-fo="gd:i:tapmarker"] > .inner [data-set]\').forEach(function(b){o.push([b.dataset.set,b.dataset.v])});o');
  // every field once by a real mouse click (its first value not in force), the field put back without a redraw (the click itself redraws);
  // then every other value of every button, select and number pressed in the page as the self-test does (noDraw: no redraw), each checked
  const missing = [], T0 = 'S.guidance.items.tapmarker', redraw = `S.tab="guide";S.ui.level=1;S.op=${JSON.stringify(op)};Object.keys(S.op).forEach(function(k){var m=/^gd:i:(.+)$/.exec(k),it=m&&S.op[k]&&S.guidance.items[m[1]];if(it)gdFormOf(it).groups.forEach(function(g){if(g)S.op["gd:f:"+m[1]+":"+g.k]=true})});renderEditor()`; // round 14: the item's groups open
  const fields = {}; ctl.forEach(([k, v]) => { (fields[k] = fields[k] || []).push(v); });
  let real = 0;
  for (const k of Object.keys(fields)) {
    const f = k.split('.').pop(), cur = await R(`String(${T0}.${f})`), v = fields[k].filter(x => x !== cur)[0]; if (v === undefined) continue;
    if (f === 'focus' && v === 'spotlight') continue;
    let loc = p.locator(`[data-fo="gd:i:tapmarker"] > .inner [data-set="${k}"][data-v="${v}"]`).first(); if (!(await loc.count())) { await R(redraw); loc = p.locator(`[data-fo="gd:i:tapmarker"] > .inner [data-set="${k}"][data-v="${v}"]`).first(); }
    if (!(await loc.count())) { missing.push(k + '=' + v + ' (not drawn)'); continue; }
    await loc.scrollIntoViewIfNeeded(); await loc.click(); real++;
    const got = await R(`String(getPath("${k.slice(1)}"))`); if (got !== String(v)) missing.push(k + '=' + v + ' got ' + got);
    await R(`${T0}.${f}=${JSON.stringify(cur === 'true' ? true : cur === 'false' ? false : isNaN(+cur) || cur === '' ? cur : +cur)}`);
  }
  await R(base + redraw);
  const sels = await R('[].map.call(ed.querySelectorAll(\'[data-fo="gd:i:tapmarker"] > .inner select[data-sel]\'),function(s){return[s.dataset.sel,[].map.call(s.options,function(o){return o.value})]})');
  for (const [k, vals] of sels) {
    const f = k.split('.').pop(), cur = await R(`String(${T0}.${f})`), v = vals.filter(x => x !== cur)[0]; if (v === undefined) continue;
    const loc = p.locator(`[data-fo="gd:i:tapmarker"] > .inner select[data-sel="${k}"]`); if (!(await loc.count())) { missing.push(k + ' (not drawn)'); continue; }
    await loc.scrollIntoViewIfNeeded(); await loc.selectOption(v); real++;
    const got = await R(f === 'group' ? `String((gdGroupOf(S.guidance.items.tapmarker)||{}).id)` : `String(getPath("${k.slice(1)}"))`); if (got !== v) missing.push(k + '=' + v + ' got ' + got);
    await R(base + redraw);
  }
  const nums = await R('[].map.call(ed.querySelectorAll(\'[data-fo="gd:i:tapmarker"] > .inner input.u[type=number][data-num]\'),function(s){return[s.dataset.num,s.max]})');
  for (const [k, mx] of nums) {
    const f = k.split('.').pop(), old = await R(`JSON.stringify(${T0}.${f})`), loc = p.locator(`[data-fo="gd:i:tapmarker"] > .inner input.u[type=number][data-num="${k}"]`);
    if (!(await loc.count())) { missing.push(k + ' (not drawn)'); continue; }
    await loc.scrollIntoViewIfNeeded(); await loc.fill(String(mx)); await loc.press('Tab'); real++;
    const got = await R(`String(getPath("${k.slice(1)}"))`); if (+got !== +mx) missing.push(k + '=' + mx + ' got ' + got);
    await R(`${T0}.${f}=${old}`);
  }
  await R(base + redraw);
  // the rest of the values, in the page: each button clicked, each select changed, as the self-test works them (no redraw between)
  const rest = await R(`var bad=[],n=0,keep=JSON.stringify(S.guidance),put=function(){S.guidance=JSON.parse(keep)};noDraw=true;try{
    ed.querySelectorAll('[data-fo="gd:i:tapmarker"] > .inner [data-set]').forEach(function(b){put();b.click();n++;var g=String(getPath(b.dataset.set.slice(1)));if(g!==b.dataset.v)bad.push(b.dataset.set+"="+b.dataset.v+" got "+g)});
    ed.querySelectorAll('[data-fo="gd:i:tapmarker"] > .inner select[data-sel]').forEach(function(x){[].forEach.call(x.options,function(o){put();x.value=o.value;x.dispatchEvent(new Event("change",{bubbles:true}));n++;var g=/group$/.test(x.dataset.sel)?(gdGroupOf(S.guidance.items.tapmarker)||{}).id:String(getPath(x.dataset.sel.slice(1)));if(g!==o.value)bad.push(x.dataset.sel+"="+o.value+" got "+g)})})}
    finally{noDraw=false;put()}[n,bad]`);
  await R(base + redraw);
  rest[1].forEach(x => missing.push(x));
  ok(`Every field of the item form pressed once by a real click (${real}), and every other value of its ${ctl.length} buttons and selects in the page (${rest[0]}): each sets its field`, missing.length === 0 && real >= 30 && ctl.length > 60 && rest[0] > 80, missing);
  // the form's own buttons: a variant, an audience's line, a returning line, a chip's ×, ▸ on a line
  await R(base + 'S.guidance.items.tapmarker.aud="family";S.tab="guide";S.ui.level=1;S.op=' + JSON.stringify(op) + ';Object.keys(S.op).forEach(function(k){var m=/^gd:i:(.+)$/.exec(k),it=m&&S.op[k]&&S.guidance.items[m[1]];if(it)gdFormOf(it).groups.forEach(function(g){if(g)S.op["gd:f:"+m[1]+":"+g.k]=true})});renderEditor()');
  const clickIn = async sel => { const l = p.locator('[data-fo="gd:i:tapmarker"] ' + sel).first(); await l.scrollIntoViewIfNeeded(); await l.click(); await W(120); };
  await clickIn('[data-gdvaradd]'); const fv1 = await R('[].concat(S.guidance.items.tapmarker.says).length');
  { const l = p.locator('[data-fo="gd:i:tapmarker"] [data-gdvardel]').last(); await l.scrollIntoViewIfNeeded(); await l.click(); await W(120); } const fv2 = await R('[].concat(S.guidance.items.tapmarker.says).length');
  await clickIn('[data-gdaudadd="tapmarker|expert"]'); const fa1 = await R('!!S.guidance.items.tapmarker.saysAud.expert&&TXOWN[S.guidance.items.tapmarker.saysAud.expert]');
  await clickIn('[data-gdauddel="tapmarker|expert"]'); const fa2 = await R('S.guidance.items.tapmarker.saysAud.expert===undefined');
  await clickIn('[data-gdbackadd]'); const fb1 = await R('!!S.tx[S.guidance.items.tapmarker.saysBack]');
  await clickIn('[data-gdbackdel]'); const fb2 = await R('S.guidance.items.tapmarker.saysBack');
  await clickIn('[data-gdoff="tapmarker|aud"]'); const fc = await R('S.guidance.items.tapmarker.aud');
  await R('S.ui.rowDemo=true'); await clickIn('[data-gdsay]'); await until('!!G.gd.cur&&G.gd.cur.pull', 2000); const fs = await R('[G.gd.cur&&G.gd.cur.item.id,G.gd.cur&&G.gd.cur.text]');
  ok('...+ Add a variant and Remove; an audience\'s line (owned by the item) and Remove; a returning visitor\'s line and Remove; a chip\'s × clears its condition; ▸ shows the line on the phone', fv1 === 2 && fv2 === 1 && fa1 === 'gd:i:tapmarker' && fa2 && fb1 && fb2 === '' && fc === '' && fs[0] === 'tapmarker' && fs[1] === 'Tap here.', [fv1, fv2, fa1, fa2, fb1, fb2, fc, fs]);

  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log((fails ? `${fails} FAILED` : 'ALL PASS') + ` (${Math.round((Date.now() - t00) / 1000)} s)`);
  await b.close();
})();
