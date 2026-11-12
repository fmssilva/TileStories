// v8v checks (Playwright + the installed Edge): node test8v.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const gaps = () => R('var g=wallGaps();[g.pic.x,g.pic.y,g.items.map(function(i){return[i.id,Math.round(i.x),Math.round(i.y),+i.gap.toFixed(2)]})]');
  await R('S.hints.on=false;S.guideMute=true;S.introSeen=true;sampleFn=null;gdGroupById("inview").on=false;S.camCfg.wall="ribeira";S.view="card";S.stop="closed";S.camX=.4;S.camY=.5;S.camZoom=1.3;renderAll(false)');
  await W(400);
  // 1. at rest: each marker on its point in the picture; the wall's points shown, the palace's own point is its marker
  const g0 = await gaps(); const mk0 = g0[2].filter(i => i[0].indexOf('pin:') < 0), pins0 = g0[2].filter(i => i[0].indexOf('pin:') === 0);
  ok('Ribeira wall: the palace marker sits on the palace in the picture', mk0.some(i => i[0] === 'painting' && i[3] <= 1.5), mk0);
  ok('...the wall\'s points show as named pins, and the palace\'s point gives way to its marker', pins0.length >= 1 && !pins0.some(i => i[0] === 'pin:painting') && await R('!ph.querySelector(".campins").hidden&&ph.classList.contains("pinnames")'), pins0.map(i => i[0]));
  // 2. a real drag (the mouse, as a finger): the picture and the markers move together
  const box = await p.locator('#ph').boundingBox();
  await p.mouse.move(box.x + box.width * .55, box.y + box.height * .35); await p.mouse.down(); await p.mouse.move(box.x + box.width * .25, box.y + box.height * .25, { steps: 10 }); await p.mouse.up(); await W(200);
  const g1 = await gaps(); const dPic = [g1[0] - g0[0], g1[1] - g0[1]];
  const pal0 = mk0.find(i => i[0] === 'painting'), pal1 = g1[2].find(i => i[0] === 'painting');
  ok('A drag moves the picture both ways', Math.abs(dPic[0]) > 30 && Math.abs(dPic[1]) > 10, dPic);
  ok('...and the palace marker as far as the picture', pal1 && Math.abs(pal1[1] - pal0[1] - dPic[0]) <= 1.5 && Math.abs(pal1[2] - pal0[2] - dPic[1]) <= 1.5, [pal0, pal1]);
  ok('...every marker and pin still on its place in the picture', g1[2].every(i => i[3] <= 1.5), g1[2].filter(i => i[3] > 1.5));
  // 3. the wheel (nearer the wall)
  const cx = box.x + box.width / 2, cy = box.y + box.height * .4; await p.mouse.move(cx, cy); await p.mouse.wheel(0, -200); await W(200);
  const g2 = await gaps();
  ok('A zoom keeps every marker and pin on the picture', await R('S.camZoom') > 1.31 && g2[2].every(i => i[3] <= 1.5), g2[2].filter(i => i[3] > 1.5));
  // 4. the cathedral: off the screen at first, a drag to the right side brings it in, on its place
  await R('S.camX=.9;S.camZoom=1;camPaint();camLayer()'); await W(150);
  const cat = (await gaps())[2].find(i => i[0] === 'cathedral');
  ok('Lisbon Cathedral rides on the picture too', cat && cat[3] <= 1.5 && cat[1] > 0 && cat[1] < 360, cat);
  ok('The markers\' names follow the language', await R('S.lang="pt";renderAll(true);var t=ph.querySelector("[data-mk=painting] b").textContent;S.lang="en";renderAll(true);t') === 'O Paço da Ribeira');
  // 5. the Wall switch on the preview bar
  await R('S.previewBar=true;S.view="demo";S.stop="closed";renderAll(false)'); await W(200);
  const w0 = await R('ph.querySelector(\'[data-pt="wall"]\').textContent');
  await p.locator('#ptool [data-pt="wall"]').click(); await W(300);
  const w1 = await R('[S.camCfg.wall,S.view,S.stop,ph.querySelector(\'[data-pt="wall"]\').textContent,ph.querySelectorAll(".cpin").length,ph.querySelectorAll("#wall .mk").length]');
  ok('The preview bar\'s Wall button: from any screen, one tap to the Belém test wall (its points, no card markers)', w0 === 'Ribeira wall' && w1[0] === 'belem' && w1[1] === 'card' && w1[2] === 'closed' && w1[3] === 'Belém wall' && w1[4] >= 2 && w1[5] === 0, [w0, w1]);
  await p.locator('#ptool [data-pt="wall"]').click(); await W(300);
  ok('...and a tap back to the card\'s wall with its markers', await R('S.camCfg.wall==="ribeira"&&ph.querySelectorAll("#wall .mk").length===2'));
  // 6. the test scene, pressed in Ask Azu's console as the developer does
  await R('S.previewBar=false;S.tab="guide";renderEditor();ed.querySelector(\'[data-fo="s7agent"]\').open=true');
  const idx = await R('AGSCENES.findIndex(function(s){return s.id==="markers"})'); // round 7: the guide's scenes follow it in the console
  await R(`var s=ed.querySelector(".agscsel");s.value="${idx}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(200);
  const lbl = await R('ed.querySelector(".agscenes").textContent');
  ok('The console lists the scene "Ribeira wall · a drag moves the markers with the picture", which says what it does', /a drag moves the markers with the picture/.test(lbl) && /Does: nothing: it drags the wall/.test(lbl), lbl.slice(0, 200));
  await p.locator('[data-agscrun]').click(); await W(1000);
  const sr = await R('AG.sceneRes.markers');
  ok('...Set up and ask runs it with no model: ✓', sr && sr.ok === true, sr);
  ok('...the result row shows in the console', await R('/✓ every marker and point stays on the picture/.test(ed.querySelector(".agres").textContent)'));
  // 7. mutation proof: markers fixed on the screen (the bug) -> the scene goes red
  await R('window.__pm=placeMarkers;placeMarkers=function(){}');
  await R(`agRunScene(${idx})`); await W(1000);
  const bad = await R('AG.sceneRes.markers');
  await R('placeMarkers=window.__pm');
  ok('Mutation: with the markers left where they were, the scene fails', bad && bad.ok === false, bad && bad.note);
  await R(`agRunScene(${idx})`); await W(1000);
  ok('...restored, it passes again', await R('AG.sceneRes.markers.ok'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
