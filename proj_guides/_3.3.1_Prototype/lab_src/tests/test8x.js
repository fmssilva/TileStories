// v8x checks (Playwright + the installed Edge, WebGL by SwiftShader, three.js from jsdelivr): node test8x.js -> PASS/FAIL per check
// Every check presses the Editor's own controls: Mascot source = 3D model, then each card of Model, then Play a clip.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
const CAP = process.env.CAP || require('os').tmpdir();  // a picture of each model in the guide, to look at
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  // round 7: a change to an Editor row shows its demo on the phone; this test picks models in the Editor mid-scene, so it keeps the phone as it set it
  await R('S.ui.rowDemo=false;S.guideCfg.azuStyle="light";S.guideCfg.voice="device";S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;sampleFn=null;S.view="card";S.stop="half";renderAll(false)');
  await R('renderEditor();ed.querySelector(\'[data-fo="s7mascot"]\').open=true');
  // 1. the Model row is there only for the 3D source
  ok('Tile of light: no Model row', await R('!ed.querySelector(".az3dpick")'));
  await p.locator('[data-set="@guideCfg.azuStyle"][data-v="glb"]').click();
  await p.waitForFunction(() => LAB.run('Azu3D.state().ready||Azu3D.state().failed'), null, { timeout: 60000 });
  await R('ed.querySelector(\'[data-fo="s7mascot"]\').open=true');
  const cards = await R('Array.from(ed.querySelectorAll(".az3dpick .sc")).map(function(c){var i=c.querySelector("img");return[c.textContent,c.getAttribute("aria-pressed"),i&&i.complete&&i.naturalWidth]})');
  ok('3D model: the Model row shows five cards by name, each with its picture', cards.map(c => c[0]).join('|') === 'Azu, detailed|The mouse|The explorer|The poet|The soldier' && cards.every(c => c[2] >= 96), cards);
  ok('...The explorer is chosen by default', cards[2] && cards[2][1] === 'true' && cards.filter(c => c[1] === 'true').length === 1);
  const st0 = await R('var s=Azu3D.state();[s.ready,s.src,s.model]');
  ok('...and it is the model loaded', st0[0] && st0[2] === 'explorer' && /The explorer \(explorer\.glb\)/.test(st0[1]), st0);
  await R('startTour();pauseTour()'); await W(600);
  // 2. each card: the model swaps in, holds the whole contract, is drawn and fits its frame, plays a clip, moves its mouth
  const models = [['azu', 'Azu, detailed'], ['mouse', 'The mouse'], ['explorer', 'The explorer'], ['poet', 'The poet'], ['soldier', 'The soldier']];
  for (const [id, name] of models) {
    await p.locator(`.az3dpick [data-v="${id}"]`).click();
    await p.waitForFunction(i => LAB.run('Azu3D.state().model===' + JSON.stringify(i) + '&&!Azu3D.state().swapping'), id, { timeout: 30000 }).catch(() => {});
    const f0 = await R('Azu3D.state().frames'); await W(700);
    const s = await R('var s=Azu3D.state(),i=s.info;[s.model,s.src,i.clips.length,i.missMorphs,i.front,s.frames,Azu3D.pixel(),Azu3D.fits(),!!ph.querySelector(".mascot .az3d")]');
    ok(`${name}: picking its card loads it, with the seven clips, the four morphs and the front arm`, s[0] === id && s[1].indexOf(name) === 0 && s[2] === 7 && s[3].length === 0 && s[4], s.slice(0, 5));
    ok(`...drawn in the guide (frames run, a pixel at its centre)`, s[8] && s[5] > f0 && s[6] && s[6][3] > 0, [s[5] - f0, s[6]]);
    ok(`...the whole model in its frame (85 % to 100 %)`, s[7] > .85 && s[7] <= 1, s[7]);
    ok(`...the Editor names it`, await R('ed.querySelector(".az3dpick [aria-pressed=true]").textContent===' + JSON.stringify(name) + '&&ed.textContent.indexOf(' + JSON.stringify(name + ' (') + ')>=0'));
    await p.locator('[data-az3d="wave"]').click(); await W(300);
    const wv = await R('Azu3D.state().clip');
    await R('azMouth(ph.querySelector(".mascot"),"A")'); await W(300); const mo = await R('Azu3D.morphNow("Open")');
    await R('azMouth(ph.querySelector(".mascot"),"M")');
    ok(`...Play a clip waves it, and the mouth opens on A`, wv === 'wave' && mo > .8, [wv, mo]);
    await W(900);
    const box = await p.locator('#ph').boundingBox().catch(() => null);
    if (box) await p.screenshot({ path: path.join(CAP, `v8x_${id}.png`), clip: box });
  }
  // 3. back to a model seen before: it comes from the cache (no second parse of its GLB)
  const n0 = await R('Azu3D.state().parses'); await p.locator('.az3dpick [data-v="azu"]').click();
  await p.waitForFunction(() => LAB.run('Azu3D.state().model==="azu"&&!Azu3D.state().swapping'), null, { timeout: 10000 }).catch(() => {});
  const n1 = await R('Azu3D.state().parses');
  ok('Back to Azu, detailed: drawn again from the cache (five models, five parses)', await R('Azu3D.state().model') === 'azu' && n0 === 5 && n1 === 5, [n0, n1]);
  // 4. Import still replaces the chosen model; the next pick brings a built-in back
  await p.locator('[data-az3dimport]').setInputFiles(path.join(__dirname, '..', '..', '..', 'mascote', '3d', 'mouse.glb')); await W(2500);
  const im = await R('[Azu3D.state().src,ed.querySelector(".az3dres")&&ed.querySelector(".az3dres").textContent]');
  ok('Import… mouse.glb: it is drawn, and the check finds all seven clips, four morphs and the front arm', im[0] === 'mouse.glb' && /mouse\.glb: all seven clips · all four morphs · the front arm is its own mesh/.test(im[1] || ''), im);
  await p.locator('.az3dpick [data-v="explorer"]').click();
  await p.waitForFunction(() => LAB.run('Azu3D.state().model==="explorer"&&Azu3D.state().src.indexOf("The explorer")===0'), null, { timeout: 30000 }).catch(() => {});
  ok('...then picking The explorer draws the explorer', /^The explorer/.test(await R('Azu3D.state().src')));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
