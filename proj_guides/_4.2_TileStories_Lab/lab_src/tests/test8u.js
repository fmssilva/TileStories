// v8u checks (Playwright + the installed Edge): node test8u.js -> PASS/FAIL per check. The model is a test double: no network
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src);
  // round 21 (21.10): an Editor row inside closed foldouts (the accordions of rounds 20 and 21) is reached as the developer does: each
  // closed foldout above it opened by a real click on its header, from the outside in, then the row's own control clicked (edClick)
  const openAbove = async sel => { for (let i = 0; i < 8; i++) { const m = await p.evaluate(s => LAB.run(`(function(){var b=document.querySelector(${JSON.stringify(s)});if(!b)return false;var ds=[];for(var d=b.closest("details");d;d=d.parentElement&&d.parentElement.closest("details"))ds.unshift(d);var c=ds.filter(function(d){return!d.open})[0];if(!c)return false;c.querySelector(":scope > summary").setAttribute("data-t21","1");return true})()`), sel); if (!m) return; await p.locator('summary[data-t21="1"]').first().click(); await p.evaluate(() => LAB.run('document.querySelectorAll("[data-t21]").forEach(function(x){x.removeAttribute("data-t21")});""')); await p.waitForTimeout(120); } };
  const edClick = async (sel, o) => { await openAbove(sel); await p.locator(sel).first().click(o); };
  const W = ms => p.waitForTimeout(ms);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.agentCfg.voiceOut=false;gdGroupById("inview").on=false;AG.mock={text:"ok"};S.agentCfg.provider="mock";applyProvider();S.view="card";S.stop="full";renderAll(false)');
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
  // (guidance round 3: Azu's remarks are the guidance's What's in view, one item a point of the wall in view)
  ok('Azu\'s remark about a point not on this wall is dropped', await R('gdItems().filter(function(x){return x.base==="inview"}).every(function(x){return S.wallPois.some(function(p){return p.id===x.point})})&&!gdItems().some(function(x){return x.id==="inview:painting"})&&gdItems().some(function(x){return x.id==="inview:houses"})'));
  await R('G.cam.open=false;camLayer();G.gd.manual=false;gdStop("t");var c=camView().central,it=gdItems().filter(function(x){return x.id==="inview:"+c.id})[0];gdStart(it,"test")'); await W(300);
  const bb = await R('!!ph.querySelector(".gdbub:not(.gd-out)")&&G.gd.cur&&G.gd.cur.item.base==="inview"');
  await R('S.camX=.98;camPaint();camLayer()'); await W(700);
  ok('...one about a point in view stays, and goes when a pan takes the point off the screen', bb && await R('!G.gd.cur&&!ph.querySelector(".gdbub:not(.gd-out)")'), bb);
  // 4. the scenes: each sets the prototype as a visitor would be, and the console says what it asks and expects
  await R('S.tab="guide";renderEditor();ed.querySelector(\'[data-fo="s7agent"]\').open=true');
  const s0 = await R('[AGSCENES.length,ed.querySelectorAll(".agscsel option").length]');
  ok('Scenes: 8 in the console, the guide\'s 7 (round 7: its controls\' scenes) and the guidance\'s 55 (rounds g1 and g2: 5 each; g3: 2; g4: 2; g5: 3; g6: 4; g7: 5; g8: 4; g9: 4; g12: 8; g13: 13)', s0[0] === 70 && s0[1] === 70, s0);
  await R('var s=ed.querySelector(".agscsel");s.value="3";s.dispatchEvent(new Event("change",{bubbles:true}))');
  await edClick('[data-agsetup]'); await W(400);
  const s1 = await R('[S.camCfg.wall,S.stop,G.cam.open,camView().pois.map(function(p){return p.id}).join(),/In view now: .*Jerónimos/.test(ed.querySelector(".agscenes").textContent),ed.querySelector("#agq").value]');
  ok('...Set up (the oldest here): the Belém wall, the card closed, Ask open, the monastery in view; the question filled in', s1[0] === 'belem' && s1[1] === 'closed' && s1[2] && /jeronimos/.test(s1[3]) && s1[4] && /oldest|antigo/.test(s1[5]), s1);
  // the checks: an answer that names a point off the screen fails; the right one passes
  await R('AG.mock={calls:[{name:"highlight_points",args:{poi_ids:["poi:jeronimos"]}}],text:"The oldest here is the Jerónimos Monastery, from 1501."};AG.sceneRes={}');
  await edClick('[data-agscrun]'); await W(1500);
  ok('...Set up and ask: the right answer passes (the oldest in view, by year)', await R('AG.sceneRes.right&&AG.sceneRes.right.ok'), await R('AG.sceneRes.right'));
  await R('AG.mock={text:"The oldest here is Belém Tower."};agRunScene(3)'); await W(1500);
  ok('...naming a point that is not in view fails, and says which', await R('var r=AG.sceneRes.right;r&&!r.ok&&/not in view: Belém Tower/.test(r.note)'), await R('AG.sceneRes.right'));
  ok('...the result table shows the answer and the check', await R('/not in view/.test(ed.querySelector(".agres").textContent)'));
  await R('AG.mock={calls:[{name:"navigate_to",args:{target_id:"poi:jeronimos"}}],text:"Here is the Jerónimos Monastery."};agRunScene(4)'); await W(1500);
  ok('Take me to a point off the screen: the camera turns until it is the middle point', await R('AG.sceneRes.take&&AG.sceneRes.take.ok'), await R('AG.sceneRes.take'));
  await R('AG.mock={calls:[{name:"make_route",args:{poi_ids:["poi:bomsucesso","poi:houses"]}}],text:"A short route."};agRunScene(5)'); await W(1500);
  ok('A route over points in view passes; it is offered as a button (Route is Suggest)', await R('AG.sceneRes.route&&AG.sceneRes.route.ok&&!!ph.querySelector(".camask .agchip")'), await R('[AG.sceneRes.route,camView().pois.map(function(p){return p.id})]'));
  // 5. Run all: the scenes in turn, each with its check; the console's 8, then Stop (the guide's and the guidance's scenes are
  //    pressed one by one in test9da, test9e and test9f, so they are not run a second time here)
  await R('AG.mock={text:"Ok."};AG.sceneRes={}');
  await edClick('[data-agscall]');
  const ran8 = await p.waitForFunction(() => Object.keys(LAB.run('AG.sceneRes')).length >= 8, null, { timeout: 120000 }).then(() => true).catch(() => false);
  const runLabel = await R('ed.querySelector("[data-agscall]").textContent');
  // (round 12 review) the count is taken as Stop lands: Playwright waits for a still button, and the Guide tab's redraws (about 1 s
  //  each, twice in a scene) can hold the click while one or two more scenes finish; the rule is the same: after Stop, only the
  //  scene running may finish
  await R('ed.addEventListener("click",function(e){if(e.target.closest("[data-agscall]"))window.__t8uAt=Object.keys(AG.sceneRes).length},{capture:true,once:true})');
  await edClick('[data-agscall]'); await p.waitForFunction(() => !LAB.run('AG.runAll'), null, { timeout: 120000 }).catch(() => {});
  const at = await R('window.__t8uAt');
  const ra = await R('var k=Object.keys(AG.sceneRes),ids=AGSCENES.map(function(s){return s.id});[k.length,k.slice(0,8).join()===ids.slice(0,8).join(),ed.querySelectorAll(".agres tbody tr").length,ed.querySelector("[data-agscall]").textContent]');
  ok('Run all scenes: the console\'s eight in order, each with its result in the table; while it runs the button is Stop', ran8 && runLabel === 'Stop' && ra[1] && ra[2] === ra[0], [ran8, runLabel, ra]);
  ok('...Stop ends it: the scene running ends, no next one starts, the button reads Run all scenes again', ra[0] >= 8 && at >= 8 && ra[0] <= at + 1 && ra[3] === 'Run all scenes', [at, ra]);
  ok('...the photo scene leaves Photo of the view as it was (Off)', await R('S.agentCfg.photo===false&&!!AG.sceneRes.photo'));
  // 6. the Editor rows
  // round 14: Trace under each answer is a row of 5 · Ask Azu › What the visitor sees (s7ask), found by its label
  ok('Editor: Trace under each answer (What the visitor sees) and Names on the points (Ask about the view)', await R('!!rowEl(ed,"s7ask","Trace under each answer")&&/Names on the points/.test(ed.querySelector(\'[data-fo="s7cam"]\').textContent)'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
