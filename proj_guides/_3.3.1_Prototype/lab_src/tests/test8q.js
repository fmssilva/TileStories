// v8q checks (Playwright + the installed Edge, WebGL by SwiftShader): node test8q.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.introSeen=true;sampleFn=null;S.guideCfg.azuStyle="light";S.view="card";S.stop="full";renderAll(false)');
  // 1. the tile of light draws in the guide's Azu
  await R('startTour()'); await p.waitForTimeout(900);
  const g1 = await R('var m=ph.querySelector(".mascot");[!!m.querySelector("canvas.azgl"),getComputedStyle(m.querySelector("svg")).display,AzuGL.pixel()]');
  ok('Tile of light: the guide\'s Azu is the shader\'s canvas (the drawing hidden), and it draws (a solid tile at its centre)', g1[0] && g1[1] === 'none' && g1[2] && g1[2][3] > 200, g1);
  // 2. its form follows its state: at rest a tile, listening light
  await p.waitForTimeout(600); const rest = await R('AzuGL.get("morph")');
  await R('G.talk="listening"'); await p.waitForTimeout(1500); const lis = await R('[AzuGL.get("morph"),AzuGL.pixel()]');
  ok('At rest a tile; listening, it becomes the holographic cube (see-through)', rest < 0.1 && lis[0] > 0.7 && lis[1][3] < 230, [rest, lis]);
  await R('G.talk="idle"'); await p.waitForTimeout(1500);
  ok('...and forms again', await R('AzuGL.get("morph")') < 0.15);
  await R('azMouth(ph.querySelector(".mascot"),"A")');
  ok('The mouth opens with the words (the same shapes as the 2D Azu)', await R('AzuGL.get("talk")') === 1);
  // 3. peek: from behind the block's top edge
  await R('stopTour();S.view="card";S.stop="full";renderPhone(false);var el=scr.querySelector(\'[data-ref="b:b_facts"]\');scr.scrollTop+=el.getBoundingClientRect().top-scr.getBoundingClientRect().top-200;azPeek(el)'); await p.waitForTimeout(900);
  const pk = await R('var x=ph.querySelector(".azpeek"),e=scr.querySelector(\'[data-ref="b:b_facts"]\');x?[Math.round(x.getBoundingClientRect().bottom-e.getBoundingClientRect().top),x.classList.contains("up"),getComputedStyle(x).overflow,!!x.querySelector("canvas")]:null');
  ok('Peek: Azu rises in a box whose bottom is the block\'s top edge (it clips), so it comes from behind the block', pk && Math.abs(pk[0]) <= 2 && pk[1] && pk[2] === 'hidden' && pk[3], pk);
  await p.waitForTimeout(3600);
  ok('...and sinks back after a moment', await R('!ph.querySelector(".azpeek")'));
  // 4. the walk: Azu leaves as light, travels to the point on the wall, becomes a tile there (smaller)
  await R('S.stop="closed";S.camX=.4;renderPhone(false);G.cam.open=true;camLayer();azWalk("torreao")'); await p.waitForTimeout(500);
  const mid = await R('[G.azWalk,AzuGL.get("to")]');
  await p.waitForTimeout(1600);
  const end = await R('var w=ph.querySelector(".azwalker"),m=w.style.transform.match(/translate\\(([-\\d.]+)px, ?([-\\d.]+)px\\) scale\\(([\\d.]+)\\)/),t=poiScreen(S.wallPois.filter(function(p){return p.id==="torreao"})[0]);[+m[1]+32-t.x,+m[2]+70-t.y,+m[3],w.classList.contains("arrived"),G.azWalk,AzuGL.get("to")]');
  ok('The walk: light while it travels', mid[0] === true && mid[1] === 1, mid);
  ok('...it arrives on the point (within 2 px), smaller, and becomes a tile', Math.abs(end[0]) < 2 && Math.abs(end[1]) < 2 && end[2] < 0.7 && end[3] && end[4] === false && end[5] === 0, end);
  // 5. in Ask about the view, an answer that lights a point sends Azu there
  await R('ph.querySelector(".azwalker")&&ph.querySelector(".azwalker").remove();camAsk("",\"oldest\")'); await p.waitForTimeout(2600);
  const lit = await R('[G.cam.lit[0],ph.querySelector(".azwalker")&&ph.querySelector(".azwalker").dataset.at]');
  ok('Ask about the view: Azu walks to the point the answer lights', lit[0] && lit[0] === lit[1], lit);
  await R('G.cam.open=false;camLayer()');
  ok('Closing the panel sends Azu home (the walker goes)', await R('!ph.querySelector(".azwalker")'));
  // 6. Tile (2.5D): the drawing, no canvas
  await R('S.guideCfg.azuStyle="tile";S.stop="full";renderPhone(false);startTour()'); await p.waitForTimeout(500);
  ok('Style Tile: the 2.5D drawing, no canvas', await R('var m=ph.querySelector(".mascot");!m.querySelector("canvas")&&getComputedStyle(m.querySelector("svg")).display!=="none"'));
  await R('stopTour();S.guideCfg.azuStyle="light"');
  // 7. Reduce Motion: the walk is a cut, the shader still
  await R('S.reduce=true;S.stop="closed";renderPhone(false);G.cam.open=true;camLayer();azWalk("painting")'); await p.waitForTimeout(150);
  const rm = await R('var w=ph.querySelector(".azwalker");[w?w.className:null,S.stop,G.cam.open,S.guideCfg.azuStyle,G.on]'); ok('Reduce Motion: the walk is a cut (it is already there)', rm[0] && rm[0].indexOf('arrived') >= 0, rm);
  await R('S.reduce=false;G.cam.open=false;camLayer()');
  // 8. the Editor
  ok('Editor: Mascot has Style, Walks to the wall and three Try', await R('var f=ed.querySelector(\'[data-fo="s7mascot"]\');!!f&&/Tile of light/.test(f.textContent)&&/Walks to the wall/.test(f.textContent)&&f.querySelectorAll("[data-azqtry]").length===3'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
