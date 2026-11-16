// v8z checks (Playwright + the installed Edge): node test8z.js -> PASS/FAIL per check
// The mascot's size, and a character per audience, pressed in the Editor.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  // round 21 (21.10): an Editor row inside closed foldouts (the accordions of rounds 20 and 21) is reached as the developer does: each
  // closed foldout above it opened by a real click on its header, from the outside in, then the row's own control clicked (edClick)
  const openAbove = async sel => { for (let i = 0; i < 8; i++) { const m = await p.evaluate(s => LAB.run(`(function(){var b=document.querySelector(${JSON.stringify(s)});if(!b)return false;var ds=[];for(var d=b.closest("details");d;d=d.parentElement&&d.parentElement.closest("details"))ds.unshift(d);var c=ds.filter(function(d){return!d.open})[0];if(!c)return false;c.querySelector(":scope > summary").setAttribute("data-t21","1");return true})()`), sel); if (!m) return; await p.locator('summary[data-t21="1"]').first().click(); await p.evaluate(() => LAB.run('document.querySelectorAll("[data-t21]").forEach(function(x){x.removeAttribute("data-t21")});""')); await p.waitForTimeout(120); } };
  const edClick = async (sel, o) => { await openAbove(sel); await p.locator(sel).first().click(o); };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  // round 7: a change to an Editor row shows its demo on the phone; this test changes rows mid-scene, so it keeps the phone as it set it
  await R('S.ui.rowDemo=false;S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;sampleFn=null;S.view="card";S.stop="half";renderAll(false);S.tab="guide";renderEditor();ed.querySelector(\'[data-fo="s7mascot"]\').open=true;startTour();pauseTour()'); await W(800);
  // 1. Size: the row, Medium by default; each choice resizes the guide's Azu (the drawing and the shader's canvas)
  const opts = await R('Array.from(ed.querySelectorAll(\'[data-set="@guideCfg.azuSize"]\')).map(function(b){return b.textContent+(b.getAttribute("aria-pressed")==="true"?"*":"")})');
  ok('Mascot: Size offers Small, Medium (chosen) and Large', opts.join('|') === 'Small|Medium*|Large', opts);
  const dims = {};
  for (const s of ['s', 'm', 'l']) { await edClick(`[data-set="@guideCfg.azuSize"][data-v="${s}"]`); await W(400); dims[s] = await R('var m=ph.querySelector(".mascot"),c=m&&m.querySelector(".azgl");[m.offsetWidth,m.offsetHeight,c?c.offsetWidth:0]'); }
  ok('...Small 80 %, Medium 56 px wide, Large 135 % (the drawing and the canvas alike)', dims.m[0] === 56 && Math.abs(dims.s[0] - 56 * .8) <= 1 && Math.abs(dims.l[0] - 56 * 1.35) <= 1 && Math.abs(dims.l[2] - 64 * 1.35) <= 1, dims);
  await edClick('[data-set="@guideCfg.azuSize"][data-v="m"]'); await W(300);
  // 2. Character per audience: a row in Looks per audience, "Same as the guide" for each by default
  await R('S.tab="card";renderEditor();ed.querySelector(\'[data-fo="s8look"]\')&&(ed.querySelector(\'[data-fo="s8look"]\').open=true)');
  const sels = await R('Array.from(ed.querySelectorAll(\'[data-sel^="@aud.looks."][data-sel$=".char"]\')).map(function(s){return s.dataset.sel+"="+s.options[s.selectedIndex].text+"/"+s.options.length})');
  ok('Looks per audience: a Character for Family, Explorer and Expert, each "Same as the guide", with every source and model', sels.length === 3 && sels.every(s => /=Same as the guide\/8$/.test(s)), sels);
  // Family = the mouse; previewing as Family turns the guide's Azu into the 3D mouse; Explorer keeps the guide's own
  await openAbove('[data-sel="@aud.looks.family.char"]'); await p.locator('[data-sel="@aud.looks.family.char"]').selectOption('glb:mouse'); await W(300);
  await p.locator('[data-set="@vis.aud"][data-v="family"]').first().click(); await W(500);
  await p.waitForFunction(() => LAB.run('Azu3D.state().ready&&Azu3D.state().model==="mouse"'), null, { timeout: 60000 }).catch(() => {});
  const fam = await R('[S.guideCfg.azuStyle,S.guideCfg.azuModel,Azu3D.state().model,!!ph.querySelector(".mascot .az3d")]');
  ok('Preview as Family: the guide\'s Azu is the 3D mouse', fam[0] === 'glb' && fam[1] === 'mouse' && fam[2] === 'mouse' && fam[3], fam);
  await openAbove('[data-sel="@aud.looks.expert.char"]'); await p.locator('[data-sel="@aud.looks.expert.char"]').selectOption('light'); await W(300);
  await p.locator('[data-set="@vis.aud"][data-v="expert"]').first().click(); await W(600);
  ok('Preview as Expert (Tile of light): the shader\'s tile again', await R('S.guideCfg.azuStyle==="light"&&!!ph.querySelector(".mascot .azgl:not(.az3d)")&&!ph.querySelector(".az3d")'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
