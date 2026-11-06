// v8u checks (Playwright + the installed Edge): node test8u.js -> PASS/FAIL per check. The model is a test double: no network
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src);
  const W = ms => p.waitForTimeout(ms);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.agentCfg.voiceOut=false;S.camCfg.comments=false;AG.mock={text:"ok"};S.agentCfg.provider="mock";applyProvider();S.view="card";S.stop="full";renderAll(false)');
  await W(300);
  // 1. the trace under an answer: the model, the times, the tokens, the calls; the text and the link cards untouched
  await R('agCardAsk();AG.mock={calls:[{name:"navigate_to",args:{target_id:"b:b_fun"}},{name:"show_links",args:{items:[{target_id:"b:b_ba"}]}},{name:"navigate_to",args:{target_id:"b:zz"}}],text:"An opera [src:b:b_fun]."};ask("Opera?")');
  await W(400);
  const t1 = await R('var m=ph.querySelectorAll(".gd-thread > div"),last=m[m.length-1],tr=last.querySelector(".agtrace");[!!tr,tr&&tr.textContent,last.querySelectorAll(".aglink").length,G.agTurn.text,!!last.querySelector(".aglinks")&&last.querySelector(".aglinks").compareDocumentPosition(tr)&4]');
  ok('Trace: under the answer, the model, the times and the tokens', t1[0] && /Test double · mock/.test(t1[1]) && / ms · /.test(t1[1]) && / in \//.test(t1[1]), t1);
  ok('...each call, valid or refused', /✓ navigate_to\(b:b_fun\)/.test(t1[1]) && /✓ show_links\(b:b_ba\)/.test(t1[1]) && /✗ navigate_to\(b:zz\)/.test(t1[1]), t1);
  ok('...after the link cards, which still work; the answer\'s text has no trace in it', t1[2] === 1 && t1[3] === 'An opera.' && t1[4], t1);
  await p.locator('#ph .gd-thread [data-aglink="b:b_ba"]').last().click(); await W(300);
  ok('...a link card under a trace still goes there', await R('scr.querySelector(\'[data-ref="b:b_ba"]\').classList.contains("gfocus")'));
  await R('AG.mock={fail:"rate_limited"};ask("Opera?")'); await W(400);
  ok('...a failed call keeps a trace too (it says why)', await R('var m=ph.querySelectorAll(".gd-thread > div"),tr=m[m.length-1].querySelector(".agtrace");!!tr&&/rate_limited/.test(tr.textContent)'));
  await R('AG.mock={text:"Use the *slider* or the **gallery** [src:b:b_ba]."};ask("How?")'); await W(400);
  ok('Markdown a model writes is taken out of the answer; the prompt asks for plain text', await R('G.agTurn.text==="Use the slider or the gallery."&&/Plain spoken text: no markdown/.test(AG.mockSeen.prompt)'), await R('G.agTurn.text'));
  await R('S.agentCfg.trace=false;renderGuide()');
  ok('...Off: no trace', await R('!ph.querySelector(".agtrace")'));
  await R('S.agentCfg.trace=true;stopTour()');
  // 2. the wall: every point in view named; the middle only, as in the app
  await R('agWall(.4,.5,1)'); await W(300);
  const n1 = await R('[].map.call(ph.querySelectorAll(".cpin b"),function(b){return getComputedStyle(b).opacity}).every(function(o){return +o>.5})');
  ok('Wall: every point in view shows its name (testing)', n1, n1);
  await R('S.camCfg.names="middle";camLayer()'); await W(400);
  ok('...The middle one: the others are dots', await R('[].filter.call(ph.querySelectorAll(".cpin:not(.central):not(.lit) b"),function(b){return +getComputedStyle(b).opacity>.5}).length===0'));
  await R('S.camCfg.names="all";camLayer()');
  // 3. a remark about a point that has left the screen goes
  await R('G.fab.bub={id:"painting",text:"In the middle: Ribeira Palace.",act:true};fabLayer()'); await W(200);
  ok('Azu\'s remark about a point not on this wall is dropped', await R('!ph.querySelector(".azfabbub")&&!G.fab.bub'));
  await R('G.cam.open=false;camLayer();var c=camView().central;G.fab.bub={id:c.id,text:"x",act:true};fabLayer()');
  const bb = await R('!!ph.querySelector(".azfabbub")');
  await R('S.camX=.98;camPaint();camLayer()'); await W(700);
  ok('...one about a point in view stays, and goes when a pan takes the point off the screen', bb && await R('!ph.querySelector(".azfabbub")'), bb);
  // 4. the scenes: each sets the prototype as a visitor would be, and the console says what it asks and expects
  await R('S.tab="guide";renderEditor();ed.querySelector(\'[data-fo="s7agent"]\').open=true');
  const s0 = await R('[AGSCENES.length,ed.querySelectorAll(".agscsel option").length]');
  ok('Scenes: 8 in the console, the guide\'s 7 (round 7: its controls\' scenes) and the guidance\'s 10 (rounds g1 and g2: 5 each)', s0[0] === 25 && s0[1] === 25, s0);
  await R('var s=ed.querySelector(".agscsel");s.value="3";s.dispatchEvent(new Event("change",{bubbles:true}))');
  await p.locator('[data-agsetup]').click(); await W(400);
  const s1 = await R('[S.camCfg.wall,S.stop,G.cam.open,camView().pois.map(function(p){return p.id}).join(),/In view now: .*Jerónimos/.test(ed.querySelector(".agscenes").textContent),ed.querySelector("#agq").value]');
  ok('...Set up (the oldest here): the Belém wall, the card closed, Ask open, the monastery in view; the question filled in', s1[0] === 'belem' && s1[1] === 'closed' && s1[2] && /jeronimos/.test(s1[3]) && s1[4] && /oldest|antigo/.test(s1[5]), s1);
  // the checks: an answer that names a point off the screen fails; the right one passes
  await R('AG.mock={calls:[{name:"highlight_points",args:{poi_ids:["poi:jeronimos"]}}],text:"The oldest here is the Jerónimos Monastery, from 1501."};AG.sceneRes={}');
  await p.locator('[data-agscrun]').click(); await W(1500);
  ok('...Set up and ask: the right answer passes (the oldest in view, by year)', await R('AG.sceneRes.right&&AG.sceneRes.right.ok'), await R('AG.sceneRes.right'));
  await R('AG.mock={text:"The oldest here is Belém Tower."};agRunScene(3)'); await W(1500);
  ok('...naming a point that is not in view fails, and says which', await R('var r=AG.sceneRes.right;r&&!r.ok&&/not in view: Belém Tower/.test(r.note)'), await R('AG.sceneRes.right'));
  ok('...the result table shows the answer and the check', await R('/not in view/.test(ed.querySelector(".agres").textContent)'));
  await R('AG.mock={calls:[{name:"navigate_to",args:{target_id:"poi:jeronimos"}}],text:"Here is the Jerónimos Monastery."};agRunScene(4)'); await W(1500);
  ok('Take me to a point off the screen: the camera turns until it is the middle point', await R('AG.sceneRes.take&&AG.sceneRes.take.ok'), await R('AG.sceneRes.take'));
  await R('AG.mock={calls:[{name:"make_route",args:{poi_ids:["poi:bomsucesso","poi:houses"]}}],text:"A short route."};agRunScene(5)'); await W(1500);
  ok('A route over points in view passes; it is offered as a button (Route is Suggest)', await R('AG.sceneRes.route&&AG.sceneRes.route.ok&&!!ph.querySelector(".camask .agchip")'), await R('[AG.sceneRes.route,camView().pois.map(function(p){return p.id})]'));
  // 5. Run all: every scene in turn, each with its check
  await R('AG.mock={text:"Ok."};AG.sceneRes={}');
  await p.locator('[data-agscall]').click(); await p.waitForFunction(() => !LAB.run('AG.runAll'), null, { timeout: 240000 }).catch(() => {});
  ok('Run all scenes: twenty-five results in the table (the markers scene, the guide\'s and the guidance\'s need no model)', await R('Object.keys(AG.sceneRes).length===25&&ed.querySelectorAll(".agres tbody tr").length===25'), await R('Object.keys(AG.sceneRes)'));
  ok('...the photo scene leaves Photo of the view as it was (Off)', await R('S.agentCfg.photo===false'));
  // 6. the Editor rows
  ok('Editor: Trace under each answer (Outputs) and Names on the points (Ask about the view)', await R('/Trace under each answer/.test(ed.querySelector(\'[data-fo="s7agent"]\').textContent)&&/Names on the points/.test(ed.querySelector(\'[data-fo="s7cam"]\').textContent)'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
