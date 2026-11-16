// v8t live check, by hand only (it spends the key's quota): node test8t_live.js [shots dir]. Needs keys.local.js (python keys_local.py)
const { chromium, labUrl } = require('./pw');
const url = labUrl('#probe-live');
const shots = process.argv[2] || '';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB && window.LABKEYS, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('D.card.hints.on=false;LAB.previewBar=false;P.guideMute=true;P.introSeen=true;D.agent.voiceOut=false;gdGroupById("inview").on=false;P.lang="pt";ui.view="card";ui.stop="full";renderAll(false);applyProvider();renderEditor()');
  ok('The key from keys.local.js: Gemini answers here', await R('agProvider()==="gemini"&&sampleFn===AG.fn'), await R('agProvider()'));
  // the card
  await R('startTour();pauseTour();G.askOpen=true;ui.dockH="full";renderGuide();ask("O palácio tinha uma ópera? Mostra-me onde se fala disso.")');
  await p.waitForFunction(() => !LAB.run('AG.con.busy'), null, { timeout: 30000 }).catch(() => {});
  const c = await R('[AG.con.err||"",G.agTurn&&G.agTurn.text,G.agTurn&&G.agTurn.cites,AG.con.calls.map(function(k){return k.name+(k.ok?"":"!")}),AG.last&&[AG.last.model,AG.last.first,AG.last.ms,AG.last.tin,AG.last.tout]]');
  ok('Card: an answer in Portuguese, from the library', !c[0] && c[1] && c[1].length > 10, c);
  if (shots) await p.locator('#ph').screenshot({ path: shots + '/live_card.png' });
  // the wall
  await R('stopTour();LAB.wall="belem";ui.view="card";ui.stop="closed";ui.camX=.3;ui.camY=.5;ui.camZoom=1;renderAll(false);G.cam.open=true;camLayer();camAsk("Qual é o mais antigo que estou a ver? Mostra-mo.")');
  await p.waitForFunction(() => !LAB.run('AG.con.busy'), null, { timeout: 30000 }).catch(() => {});
  await p.waitForTimeout(400);
  const w = await R('[AG.con.err||"",G.cam.answer,G.cam.lit,AG.con.calls.map(function(k){return k.name+(k.ok?"":"!")}),AG.last&&[AG.last.model,AG.last.first,AG.last.ms,AG.last.tin,AG.last.tout]]');
  ok('Wall: an answer about the points in view, acting on the wall', !w[0] && w[1] && w[3].length > 0, w);
  if (shots) { await p.locator('#ph').screenshot({ path: shots + '/live_wall.png' });
    await R('ui.tab="guide";renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');f.open=true;f.scrollIntoView()'); await p.waitForTimeout(300);
    await p.locator('[data-fo="s7agent"]').screenshot({ path: shots + '/live_editor.png' }); }
  // the photo of the view goes with the question
  await R('D.agent.photo=true;G.cam.turn=null;window.__img=0;var of=window.fetch;window.fetch=function(u,o){try{if(/inlineData/.test(o.body))window.__img++}catch(e){}return of(u,o)};camAsk("Descreva o que vê nesta parte do painel, numa frase.")');
  await p.waitForFunction(() => !LAB.run('AG.con.busy'), null, { timeout: 30000 }).catch(() => {});
  const f = await R('[AG.con.err||"",G.cam.answer,window.__img,AG.last&&[AG.last.tin,AG.last.ms]]');
  ok('Wall, with the photo: the view went as a JPEG with the question, and an answer came', !f[0] && f[1] && f[2] > 0, f);
  await R('D.agent.photo=false');
  // link cards: where to see something
  await R('LAB.wall="ribeira";ui.stop="full";renderAll(false);startTour();pauseTour();G.askOpen=true;ui.dockH="full";renderGuide();ask("Onde posso ver como era o palácio antes do terramoto?")');
  await p.waitForFunction(() => !LAB.run('AG.con.busy'), null, { timeout: 30000 }).catch(() => {});
  await p.waitForTimeout(300);
  const l = await R('[AG.con.err||"",G.agTurn&&G.agTurn.text,G.agTurn&&G.agTurn.links.map(function(x){return x.title}),G.agTurn&&G.agTurn.chips.map(function(x){return x.label}),AG.con.calls.map(function(k){return k.name+(k.ok?"":"!")})]');
  ok('Card: where to see it: link cards or an offered action', !l[0] && l[1] && (l[2].length + l[3].length > 0 || l[4].length > 0), l);
  if (shots) await p.locator('#ph').screenshot({ path: shots + '/live_links.png' });
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
