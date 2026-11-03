// v8w checks (Playwright + the installed Edge, WebGL by SwiftShader, three.js from jsdelivr): node test8w.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/block-components-lab.html#probe';
const GLB = path.join(__dirname, '..', '..', 'mascote', '3d', 'azu_tile.glb');
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;sampleFn=null;S.view="card";S.stop="half";renderAll(false)');
  // 1. the Editor: Mascot source with 3D model (GLB); choosing it loads the model
  await R('renderEditor();ed.querySelector(\'[data-fo="s7mascot"]\').open=true');
  const src = await R('Array.from(ed.querySelectorAll(\'[data-set="@guideCfg.azuStyle"]\')).map(function(b){return b.textContent})');
  ok('Editor: Mascot source offers Tile of light, Tile (2.5D) and 3D model (GLB)', src.join('|') === 'Tile of light|Tile (2.5D)|3D model (GLB)', src);
  await p.locator('[data-set="@guideCfg.azuStyle"][data-v="glb"]').click();
  await p.waitForFunction(() => LAB.run('Azu3D.state().ready||Azu3D.state().failed'), null, { timeout: 60000 });
  const st = await R('var s=Azu3D.state();[s.ready,s.err,s.info&&s.info.clips,s.info&&s.info.morphs,s.info&&s.info.front]');
  ok('The built-in GLB loads with its seven clips, four morphs and the front arm as its own mesh', st[0] && st[2].length === 7 && ['Blink', 'Open', 'Wide', 'Round'].every(m => st[3].indexOf(m) >= 0) && st[4], st);
  ok('...the Editor says what it holds', await R('/clips: idle, listen, point, talk, think, walk, wave/.test(ed.querySelector(\'[data-fo="s7mascot"]\').textContent)'));
  // 2. the guide: the 3D Azu in the mascot's place, drawn (a pixel at its centre is not empty)
  const wv = await R('startTour();az3dWant()'); await p.waitForFunction(() => LAB.run('Azu3D.state().frames>3'), null, { timeout: 20000 }).catch(() => {});
  const g = await R('var c=ph.querySelector(".mascot .az3d");[!!c,c&&c.offsetWidth,Azu3D.state().frames>3,Azu3D.pixel()]');
  ok('Start the guide: Azu is the 3D model, in the mascot\'s place, and drawn', g[0] && g[1] >= 40 && g[2] && g[3] && g[3][3] > 0, g);
  const fit = await R('Azu3D.fits()');
  ok('...the whole model in its frame: feet, arms, hat (its box between 85 % and 100 % of the frame)', fit > .85 && fit <= 1, fit);
  ok('...waves as the tour starts', wv === 'wave', wv);
  // 3. each state its clip
  const clipOf = async (setup) => { await R(setup); await W(450); return R('Azu3D.state().clip'); };
  await R('G.azWaveTill=0');
  const sp = await clipOf('G.paused=false;G.talk="idle";G.busy=false;G.timer=G.timer||1;var m=ph.querySelector(".mascot");m.dataset.state="speaking";m.classList.remove("pointing")');
  const li = await clipOf('ph.querySelector(".mascot").dataset.state="listening"');
  const th = await clipOf('ph.querySelector(".mascot").dataset.state="thinking"');
  // pointing: the guide's own pointAt (as a step with a spot calls it); the clip changes on the next frame
  const pt = await R('ph.querySelector(".mascot").dataset.state="speaking";G.moment=null;G.i=S.guideSteps.findIndex(function(x){return x.ref!=="hero"});pointAt({x:150,y:330});new Promise(function(r){requestAnimationFrame(function(){requestAnimationFrame(function(){r(Azu3D.state().clip)})})})');
  ok('Each state its clip: speaking -> talk, listening -> listen, thinking -> think, pointing -> point', sp === 'talk' && li === 'listen' && th === 'think' && pt === 'point', [sp, li, th, pt]);
  // 4. the mouth: visemes move the morphs (the tour paused, so its own words do not drive the mouth); blink
  await R('pauseTour();ph.querySelector(".mascot").dataset.state="idle"'); await W(300);
  await R('azMouth(ph.querySelector(".mascot"),"A")'); await W(300); const mA = await R('[Azu3D.morphNow("Open"),Azu3D.morphNow("Round")]');
  await R('azMouth(ph.querySelector(".mascot"),"O")'); await W(300); const mO = await R('[Azu3D.morphNow("Open"),Azu3D.morphNow("Round")]');
  await R('azMouth(ph.querySelector(".mascot"),"M")'); await W(300); const mM = await R('[Azu3D.morphNow("Open"),Azu3D.morphNow("Round")]');
  ok('The mouth follows the words: A opens it, O rounds it, M closes it', mA[0] > .8 && mO[1] > .8 && mO[0] < .2 && mM[0] < .1 && mM[1] < .1, [mA, mO, mM]);
  const bl = await R('new Promise(function(res){var seen=0,t0=performance.now();(function f(){if(Azu3D.morphNow("Blink")>.5)seen=1;if(seen||performance.now()-t0>6000)res(seen);else requestAnimationFrame(f)})()})');
  ok('...it blinks within six seconds', bl === 1);
  // 5. Play a clip, from the Editor
  await p.locator('[data-az3d="walk"]').click(); await W(300);
  ok('Editor: Play a clip plays it on the guide\'s Azu', await R('Azu3D.state().clip') === 'walk');
  // 6. the peek: the 3D Azu rises from behind the block
  await R('Azu3D.force(null,0);azPeek()'); await W(500);
  ok('Peek: the 3D Azu goes into the peek box', await R('!!ph.querySelector(".azpeek-in .az3d")'));
  await W(4500);
  ok('...and back to the guide after it', await R('!!ph.querySelector(".mascot .az3d")'));
  // 7. Reduce Motion: still (the clip's first frame), no blink
  await R('S.reduce=true'); await W(2000); const rb = await R('Azu3D.morphNow("Blink")'); await R('S.reduce=false');
  ok('Reduce Motion: no blink', rb === 0, rb);
  // 8. Import: the real GLB passes; a GLB without clips and morphs is named for what it lacks
  await p.locator('[data-az3dimport]').setInputFiles(GLB); await W(2500);
  const im = await R('ed.querySelector(".az3dres")&&ed.querySelector(".az3dres").textContent');
  ok('Import… checks a GLB: azu_tile.glb has all seven clips, all four morphs and the front arm', /azu_tile\.glb: all seven clips · all four morphs · the front arm is its own mesh · size 1\.6\d × 1\.6\d × 0\.4\d/.test(im || ''), im);
  // a GLB with one cube and nothing else, written here
  const bare = await R('(function(){var j={asset:{version:"2.0"},scenes:[{nodes:[0]}],nodes:[{mesh:0,name:"Box"}],meshes:[{primitives:[{attributes:{POSITION:0}}]}],buffers:[{byteLength:36}],bufferViews:[{buffer:0,byteLength:36}],accessors:[{bufferView:0,componentType:5126,count:3,type:"VEC3",min:[0,0,0],max:[1,1,0]}]};var js=new TextEncoder().encode(JSON.stringify(j));var pad=(4-js.length%4)%4,jl=js.length+pad,bin=new Float32Array([0,0,0,1,0,0,0,1,0]).buffer,tot=12+8+jl+8+36,o=new ArrayBuffer(tot),v=new DataView(o),u=new Uint8Array(o);v.setUint32(0,0x46546C67,true);v.setUint32(4,2,true);v.setUint32(8,tot,true);v.setUint32(12,jl,true);v.setUint32(16,0x4E4F534A,true);u.set(js,20);for(var i=0;i<pad;i++)u[20+js.length+i]=32;v.setUint32(20+jl,36,true);v.setUint32(24+jl,0x004E4942,true);u.set(new Uint8Array(bin),28+jl);var s="";for(var k=0;k<u.length;k++)s+=String.fromCharCode(u[k]);return btoa(s)})()');
  await p.locator('[data-az3dimport]').setInputFiles({ name: 'box.glb', mimeType: 'model/gltf-binary', buffer: Buffer.from(bare, 'base64') }); await W(1500);
  const im2 = await R('ed.querySelector(".az3dres")&&ed.querySelector(".az3dres").textContent');
  ok('...a GLB with no clips is told what it lacks, by name, and the guide keeps its model', /box\.glb: missing clips: idle, talk, listen, think, point, walk, wave · missing morphs: Blink, Open, Wide, Round · no front-arm mesh/.test(im2 || '') && await R('Azu3D.state().info.clips.length===7'), im2);
  // 9. back to the Tile of light: the 3D canvas leaves, the shader returns
  await p.locator('[data-set="@guideCfg.azuStyle"][data-v="light"]').click(); await W(500);
  ok('Back to Tile of light: the 3D model leaves the guide', await R('!ph.querySelector(".az3d")&&!!ph.querySelector(".mascot .azgl")'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  // 10. with no network: the 3D source says so and the guide shows the Tile of light
  const p2 = await b.newPage({ viewport: { width: 1400, height: 900 } }); const errs2 = []; p2.on('pageerror', e => errs2.push(e.message));
  await p2.route('**/cdn.jsdelivr.net/**', r => r.abort());
  await p.close();
  await p2.goto(url, { waitUntil: 'domcontentloaded', timeout: 120000 }); await p2.waitForFunction(() => window.LAB, null, { timeout: 90000 });
  const R2 = s => p2.evaluate(x => LAB.run(x), s);
  await R2('S.hints.on=false;S.guideMute=true;S.introSeen=true;S.view="card";S.stop="half";S.guideCfg.azuStyle="glb";renderAll(false);ed.querySelector(\'[data-fo="s7mascot"]\')&&(ed.querySelector(\'[data-fo="s7mascot"]\').open=true);startTour()');
  await p2.waitForFunction(() => LAB.run('Azu3D.state().failed'), null, { timeout: 30000 }).catch(() => {});
  const off = await R2('[Azu3D.state().failed,!!ph.querySelector(".mascot"),!ph.querySelector(".az3d"),/Could not load three\\.js/.test(ed.textContent)]');
  ok('No network: the Editor says the model could not load, the guide keeps its Azu', off.every(Boolean), off);
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
