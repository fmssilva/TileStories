// v8y checks (Playwright + the installed Edge, WebGL by SwiftShader): node test8y.js -> PASS/FAIL per check
// The tile of light's styles, pressed in the Editor: each choice reaches the shader, the preview and the guide's own Azu.
// Colours are counted in the pictures: blue ink (b > r + 60), aubergine (green under 80 % of red and 85 % of blue), ochre or gold (r > 170, b < r - 90).
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  // a picture's colour counts, in the page
  await R('window.azCount=function(src){return new Promise(function(res){var im=new Image();im.onload=function(){var c=document.createElement("canvas");c.width=im.width;c.height=im.height;var x=c.getContext("2d");x.drawImage(im,0,0);var d=x.getImageData(0,0,c.width,c.height).data,n={cover:0,blue:0,purple:0,ochre:0,r:0,b:0};for(var i=0;i<d.length;i+=4){if(d[i+3]<200)continue;var r=d[i],g=d[i+1],bl=d[i+2];n.cover++;n.r+=r;n.b+=bl;if(bl>r+60)n.blue++;if(r-g>18&&g<.8*r&&g<.85*bl)n.purple++;if(r>170&&bl<r-90)n.ochre++}n.r=Math.round(n.r/Math.max(1,n.cover));n.b=Math.round(n.b/Math.max(1,n.cover));res(n)};im.src=src})}');
  const prev = async (i) => R('azCount(ed.querySelectorAll(".azprev img")[' + i + '].src)');
  const press = async (key, v) => { await p.locator(`[data-set="@guideCfg.${key}"][data-v="${v}"]`).click(); await W(250); await R('ed.querySelector(\'[data-fo="s7mascot"]\').open=true'); };
  await R('S.guideCfg.azuStyle="light";S.guideCfg.voice="device";S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;sampleFn=null;S.view="card";S.stop="half";renderAll(false)');
  await R('S.tab="guide";renderEditor();ed.querySelector(\'[data-fo="s7mascot"]\').open=true');
  // 1. the rows, by name, with the defaults pressed; the defaults are the shader's look before the styles
  const rows = await R('["Glaze","Pattern","Light","Corners","Preview"].map(function(l){return !!rowEl(ed,"s7mascot",l)})');
  const pressed = await R('["azuGlaze","azuMotif","azuLight","azuShape"].map(function(k){var b=ed.querySelector(\'[data-set="@guideCfg.\'+k+\'"][aria-pressed="true"]\');return b&&b.textContent})');
  ok('Tile of light: the Editor shows Glaze, Pattern, Light, Corners and Preview', rows.every(Boolean), rows);
  ok('...the defaults: Cobalt, Rosette, Pastel clouds, Soft', pressed.join('|') === 'Cobalt|Rosette|Pastel clouds|Soft', pressed);
  ok('...which are the look the tile had (the same colours, pattern, corners and light)', await R('JSON.stringify([AZU_UNI.uCream,AZU_UNI.uInk,AZU_UNI.uMotif,AZU_UNI.uRound,AZU_UNI.uP0])') === JSON.stringify([[.96, .93, .87], [.12, .31, .61], 0, .12, [.48, .86, 1]]));
  const pv = await R('Array.from(ed.querySelectorAll(".azprev img")).map(function(i){return [i.alt,i.complete&&i.naturalWidth]})');
  ok('Preview: Azu at rest and as light, drawn', pv.length === 2 && pv.every(x => x[1] >= 192), pv);
  // 2. the glaze
  const cob = await prev(0);
  await press('azuGlaze', 'manganese'); const man = await prev(0);
  await press('azuGlaze', 'polychrome'); const pol = await prev(0);
  ok('Glaze Cobalt: blue ink, no aubergine', cob.blue > 1500 && cob.purple < 100, [cob.blue, cob.purple]);
  ok('Glaze Manganese: aubergine ink, no blue', man.purple > 1500 && man.blue < 100, [man.purple, man.blue]);
  ok('Glaze Polychrome: blue with an ochre accent', pol.blue > 1500 && pol.ochre > 400, [pol.blue, pol.ochre]);
  await press('azuGlaze', 'cobalt');
  // 3. the pattern
  const pics = {};
  for (const m of ['rosette', 'diamond', 'sprigs', 'plain']) { await press('azuMotif', m); pics[m] = [await R('ed.querySelectorAll(".azprev img")[0].src'), await prev(0)]; }
  const distinct = new Set(Object.values(pics).map(x => x[0])).size;
  ok('Pattern: Rosette, Diamond point, Corner sprigs and Plain draw four different faces', distinct === 4, distinct);
  ok('...Plain leaves the face with the least ink', ['rosette', 'diamond', 'sprigs'].every(m => pics.plain[1].blue < pics[m][1].blue * .6), Object.fromEntries(Object.entries(pics).map(([k, v]) => [k, v[1].blue])));
  await press('azuMotif', 'rosette');
  // 4. the light (the preview as light)
  await press('azuLight', 'gold'); const gold = await prev(1);
  await press('azuLight', 'cobalt'); const cl = await prev(1);
  await press('azuLight', 'pastel'); const pas = await prev(1);
  ok('Light Gold: warm clouds (red over blue)', gold.r - gold.b > 25, [gold.r, gold.b]);
  ok('Light Cobalt: blue clouds (blue over red)', cl.b - cl.r > 25, [cl.r, cl.b]);
  ok('...Pastel clouds sit between them', pas.b - pas.r < cl.b - cl.r && pas.r - pas.b < gold.r - gold.b, [pas.r, pas.b]);
  // 5. the corners
  const cor = {};
  for (const s of ['crisp', 'soft', 'pillow']) { await press('azuShape', s); cor[s] = await R('ed.querySelectorAll(".azprev img")[0].src'); }
  ok('Corners: Crisp, Soft and Pillow draw three different tiles', new Set(Object.values(cor)).size === 3);
  await press('azuShape', 'soft');
  // 6. the guide's own Azu takes the style (its live canvas, not the preview)
  await R('startTour();pauseTour()'); await W(800);
  const live = async () => { await W(400); return R('azCount(ph.querySelector(".mascot .azgl").toDataURL())'); };
  const l0 = await live();
  await press('azuGlaze', 'manganese'); const l1 = await live();
  ok('The guide\'s Azu wears the glaze: blue before, aubergine after Manganese', l0.blue > 200 && l1.purple > 100 && l1.blue < l0.blue / 4, [l0.blue, l1.purple, l1.blue]);
  // 7. Reduce Motion: still, in the same pose as the preview
  await R('S.reduce=true'); await W(600); const s1 = await R('ph.querySelector(".mascot .azgl").toDataURL()'); await W(500); const s2 = await R('ph.querySelector(".mascot .azgl").toDataURL()');
  await R('S.reduce=false');
  ok('Reduce Motion: Azu stands still', s1 === s2);
  await press('azuGlaze', 'cobalt');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
