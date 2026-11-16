// test9qs (Playwright + the installed Edge): node test9qs.js -> PASS/FAIL per check. Round 13's thirteen scenes in Ask Azu's console, each
// pressed as the developer does (split from test9q, which keeps the rest under 300 s); the rest of the header is test9q's.
// test9q: Guidance round 13, Fun › On the tour's second half
// (research_tour_entertainment.md 4.4, 4.7, 5.4) and the developer's answers on round 12: the plan alone on gdTourPlan (the card's own quiz
// step skipped when the quiz is promised, the pair as one beat, Don't say it! In a step with its two seams, Find it on the wall on the wall
// only; the gates: a screen reader, Calm, Expert, a word said once or never, a span not in the tour); the word suggestions and their counts
// in EN and PT, the slips' matcher; on the page: the quiz step skipped and back with No quiz, the promise's chips up while step 1 is told
// (0 px over its words, Mini, Compact, Open); Don't say it! by real clicks on Azu (the windows from the word times, a catch and its pip, a
// second tap in a window ignored, Previous never counting a slip twice, a tap after a window's end a false alarm, pokes off), Pause freezing
// a window, the result with the slip played back, Heard it! at Compact and with Mascot Off, the screen reader, a language change; Find it on
// the wall (a real drag on the wall to the point, the 1.2 s settle, the hint's direction, I give up at 20 s, the wall lost, the wall only);
// every new row's demo (a real click on a sample), the Guide script's word with Reviewed; the twelve scenes in Ask Azu's console.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(50); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const box = async sel => p.locator('#ph ' + sel).first().boundingBox();
  const tap = async sel => { const r = await box(sel); if (!r) return false; await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return true; };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  // the device voice, muted (its words go on by time); only Fun › On the tour speaks; Ink glass at Mini
  const base = 'S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.cardCfg.mode="light";S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.look="ink";S.guideCfg.life="lively";S.guideCfg.mascot=true;S.vis.aud="explorer";S.vis.len="";S.vis.speed=null;S.m.solemn=false;S.ui.gdSR=false;S.guidance.on=true;gdKindById("app").on=false;gdKindById("content").on=false;gdGroupById("play").on=false;S.ui.rowDemo=false;S.camCfg.wall="ribeira";S.camX=.5;S.camY=.5;S.camZoom=1;';
  await R('window.WK=JSON.stringify(S.tourWord);window.FK=JSON.stringify(S.tourFind);window.QK=JSON.stringify(S.tourQ)');
  const only = ids => `["quizpromise","quizfinale","guessyear","dontsay","findwall","water","cookie"].forEach(function(id){S.guidance.items[id].on=${JSON.stringify(ids)}.indexOf(id)>=0});`;
  const fresh = async (ids, x) => { await R(base + 'S.tourWord=JSON.parse(window.WK);S.tourFind=JSON.parse(window.FK);S.tourQ=JSON.parse(window.QK);if(G.on)stopTour();gdBeatClose();gdTailEnd();gdWordOff();G.gd.owed=null;G.gd.plan=null;gdResetVisitor();var st=ph.querySelector(".stv");if(st)stateHide();' + only(ids) + 'S.view="card";S.stop="full";renderAll(false);' + (x || '')); };
  const seam = id => R(`gd12Seam(${JSON.stringify(id)})`);

  // the scenes in Ask Azu's console; after a tour's plan is made the voice goes at 1.5 (the speed chip's value), so the thirteen fit in 300 s
  await R('on("startTour.after",function(){S.vis.speed=1.5})');
  const ids = await R('AGSCENES.filter(function(s){return/^gd13_/.test(s.id)}).map(function(s){return s.id})');
  ok('The console lists the thirteen new scenes', ids.length === 13, ids);
  for (const id of ids) {
    await R(base + `window.R13=null;var s=AGSCENES.filter(function(x){return x.id==="${id}"})[0];s.setup();Promise.resolve(s.act()).then(function(r){window.R13=r},function(e){window.R13=[false,"ERROR "+e.message,""]});1`);
    await until('window.R13', 90000); const r = await R('window.R13');
    ok(`Scene ${id}: ${r ? r[1] : 'no answer'}`, !!(r && r[0]), r && r[2]);
  }
  tsec('scenes');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  tsec('end');
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
