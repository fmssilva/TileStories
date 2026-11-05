// v8u live check, by hand only (it spends the key's quota): node test8u_live.js [shots dir]. Needs keys.local.js (python keys_local.py).
// It does what the developer does in the prototype: Editor > 6 · Guide > Ask Azu > 9 · Test console > Run all scenes; then a drag on the wall and a typed question.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe-live';
const shots = process.argv[2] || '';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB && window.LABKEYS, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.introSeen=true;S.guideMute=true;S.agentCfg.voiceOut=false;S.lang="pt";renderAll(false);renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');f.open=true;f.querySelector(".agcon").scrollIntoView()');
  ok('Gemini answers here (keys.local.js)', await R('agProvider()==="gemini"'));
  await p.locator('[data-agscall]').click();
  await p.waitForFunction(() => !LAB.run('AG.runAll'), null, { timeout: 240000 }).catch(() => {});
  const res = await R('AGSCENES.map(function(s){var r=AG.sceneRes[s.id]||{};return[s.id,!!r.ok,r.note,r.answer,r.view]})');
  res.forEach(r => ok('Scene ' + r[0] + ': ' + r[2], r[1], [r[3], r[4]]));
  if (shots) { await R('ed.querySelector(".agres").scrollIntoView()'); await p.waitForTimeout(300); await p.locator('.agscenes').screenshot({ path: shots + '/u_scenes.png' }); }
  // by hand: the Belém wall, a drag with the mouse, then a question typed into the phone's Ask box
  await R('agWall(.15,.5,1.3)'); await p.waitForTimeout(500);
  const bx = await p.locator('#ph').boundingBox();
  await p.mouse.move(bx.x + bx.width * .75, bx.y + bx.height * .3); await p.mouse.down(); await p.mouse.move(bx.x + bx.width * .15, bx.y + bx.height * .3, { steps: 14 }); await p.mouse.up(); await p.waitForTimeout(600);
  const view = await R('camView().pois.map(function(p){return p.name})');
  await p.locator('#caq').fill('O que estou a ver agora?'); await p.locator('#caq').press('Enter');
  await p.waitForFunction(() => !LAB.run('AG.con.busy') && LAB.run('!!G.cam.turn'), null, { timeout: 30000 }).catch(() => {});
  await p.waitForTimeout(400);
  const d = await R('var t=G.cam.turn;[t&&t.text,agNamed(t?t.text:"").map(function(id){return agTitle("poi:"+id)})]');
  const out = d[1].filter(n => view.indexOf(n) < 0);
  ok('After a drag: the typed question is answered with the points now in view, none off the screen', d[0] && d[1].length > 0 && out.length === 0, { view, answer: d[0], named: d[1] });
  if (shots) await p.locator('#ph').screenshot({ path: shots + '/u_drag.png' });
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
