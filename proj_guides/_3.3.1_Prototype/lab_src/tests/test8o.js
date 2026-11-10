// v8o checks (Playwright + the installed Edge): node test8o.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 20000 }); await p.waitForTimeout(400);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.view="card";S.stop="full";renderAll(false)');
  // 1. one panel: the dock, not the captions box over the bar; Azu's own words in a bubble, one sentence at a time
  await R('startTour()'); await p.waitForTimeout(1500);
  const d1 = await R('[!!ph.querySelector(".gdock.h-mini"),!!ph.querySelector(".gbar"),!!ph.querySelector(".gbubble .gcap"),S.guideSteps[G.i].id]');
  ok('The dock (Mini) replaces the captions box and the bar; the welcome is in Azu\'s bubble', d1[0] && !d1[1] && d1[2] && d1[3] === 'intro', d1);
  const sent = await R('var ws=[].filter.call(ph.querySelectorAll(".gbubble .gw"),function(w){return w.getClientRects().length>0}).map(function(w){return w.textContent});ws.join(" ")');
  ok('...one sentence at a time (the one being said)', sent.length > 0 && (sent.match(/[.!?]/g) || []).length <= 1, sent);
  ok('...and Azu sits beside its bubble, above the dock', await R('var m=ph.querySelector(".mascot").getBoundingClientRect(),b=ph.querySelector(".gbubble").getBoundingClientRect(),d=ph.querySelector(".gdock").getBoundingClientRect();m.left>b.right-8&&m.bottom<=d.top+4&&Math.abs(m.bottom-b.bottom)<40'));
  // 2. a block's own words: lit in the block, no bubble; the dock names the part being told
  await R('G.i=S.guideSteps.findIndex(function(s){return s.id==="g4"});runStep()'); await p.waitForTimeout(2200);
  const d2 = await R('[!!ph.querySelector(".gbubble"),!!scr.querySelector(".gread .gw"),ph.querySelector(".gd-stop").textContent]');
  ok('A block read: its words lit in the block, no bubble; the dock says the story\'s part, not "Story · Paragraph"', !d2[0] && d2[1] && !/Paragraph/.test(d2[2]) && /Stop \d+ of \d+/.test(d2[2]), d2);
  ok('...the block sits above the dock (never under it)', await R('var e=stepEl(S.guideSteps[G.i]).getBoundingClientRect(),d=ph.querySelector(".gdock").getBoundingClientRect();e.bottom<=d.top+2'));
  // 3. the dock's heights: the grabber, a swipe; a tap on its line pauses and plays; a segment jumps
  await p.locator('#ph .gd-grab').click(); await p.waitForTimeout(150);
  ok('Grabber: Mini -> Peek (the controls and suggested questions)', await R('S.dockH==="peek"&&!!ph.querySelector(".gd-ctl [data-g=next]")&&ph.querySelectorAll(".gd-chips [data-chip]").length===S.guideAsks.length'));
  const g = await p.locator('#ph .gd-grab').boundingBox(); await p.mouse.move(g.x + g.width / 2, g.y + 5); await p.mouse.down(); await p.mouse.move(g.x + g.width / 2, g.y - 60, { steps: 5 }); await p.mouse.up(); await p.waitForTimeout(150);
  ok('...a swipe up: Full (the conversation)', await R('S.dockH==="full"&&!!ph.querySelector(".gd-thread")&&ph.querySelectorAll(".gd-thread .gm-azu").length>=2'));
  await p.locator('#ph .gd-grab').click(); await p.waitForTimeout(150);
  ok('...the grabber again: back to Mini', await R('S.dockH') === 'mini');
  await p.locator('#ph .gd-line').click(); await p.waitForTimeout(150);
  ok('A tap on the dock\'s line pauses', await R('G.paused'));
  await p.locator('#ph .gd-line').click(); await p.waitForTimeout(200);
  ok('...and plays again', await R('!G.paused'));
  await R('S.dockH="peek";renderGuide()'); await p.locator('#ph .gd-seg [data-gseg="2"]').click(); await p.waitForTimeout(300); // round 11: the bar is Open's
  ok('A tap on a segment goes to that step', await R('G.i') === 2);
  // 4. a suggested question, a typed question
  await R('S.dockH="peek";renderGuide()'); await p.locator('#ph [data-chip="0"]').click(); await p.waitForTimeout(800);
  ok('A suggested question: asked, in the conversation (Full), with its answer', await R('S.dockH==="full"&&[].some.call(ph.querySelectorAll(".gm-me"),function(e){return e.textContent===L(S.guideAsks[0])})&&ph.querySelectorAll(".gm-azu").length>=1'));
  await p.locator('#ph #gq').fill('Who lived here?'); await p.locator('#ph .gd-form button').click(); await p.waitForTimeout(600);
  ok('A typed question: in the conversation', await R('[].some.call(ph.querySelectorAll(".gm-me"),function(e){return e.textContent==="Who lived here?"})'));
  await R('stopTour()');
  // 5. the talk button: a tap listens (a stand-in recogniser), its words are asked; while Azu speaks, a tap interrupts it
  await R(`window.FakeRec=function(){};FakeRec.prototype.start=function(){var s=this;window.RECS=(window.RECS||0)+1;setTimeout(function(){s.onresult&&s.onresult({results:[[{transcript:"What was inside?"}]]});if(!s.continuous)s.onend&&s.onend()},300)};FakeRec.prototype.stop=function(){this.onend&&this.onend()};window.SpeechRecognition=FakeRec`);
  await R('startTour();G.i=1;runStep();S.dockH="peek";renderGuide()'); await p.waitForTimeout(600); // round 11: the talk button is Open's
  ok('While Azu speaks: the button says it can interrupt', await R('ph.querySelector(".gd-talk").classList.contains("s-speaking")&&ph.querySelector(".gd-hint").textContent===TXT("dock_tap_stop")'));
  await p.locator('#ph .gd-talk').click(); await p.waitForTimeout(120);
  const ls = await R('[G.paused,G.talk,ph.querySelector(".gd-talk").className]');
  ok('...a tap interrupts it and listens', ls[0] && /listening|interrupted/.test(ls[1]) && /s-listening|s-interrupted/.test(ls[2]), ls);
  await p.waitForTimeout(700);
  ok('...the words heard are asked', await R('[].some.call(ph.querySelectorAll(".gm-me"),function(e){return e.textContent==="What was inside?"})'));
  await R('stopTour();startTour()'); await p.waitForTimeout(300); await R('pauseTour();S.dockH="peek";renderGuide()');
  const tb = await p.locator('#ph .gd-talk').boundingBox(); await p.mouse.move(tb.x + 28, tb.y + 28); await p.mouse.down(); await p.waitForTimeout(900);
  const hold = await R('[G.talk,!!G.rec&&G.rec.continuous]'); await p.mouse.up(); await p.waitForTimeout(300);
  ok('Hold: listens while held (continuous), sends on release', hold[0] === 'listening' && hold[1] && await R('ph.querySelectorAll(".gm-me").length>=1'), hold);
  // no recogniser allowed: it says why, and opens the text field
  await R(`window.SpeechRecognition=function(){};SpeechRecognition.prototype.start=function(){var s=this;setTimeout(function(){s.onerror&&s.onerror({error:"not-allowed"})},50)};SpeechRecognition.prototype.stop=function(){}`);
  await R('stopTour();startTour()'); await p.waitForTimeout(300); await R('pauseTour();S.dockH="peek";renderGuide()'); await p.locator('#ph .gd-talk').click(); await p.waitForTimeout(400);
  ok('The microphone not allowed: the conversation says why, and offers typing', await R('S.dockH==="full"&&!!ph.querySelector(".gm-note")&&ph.querySelector(".gm-note").textContent.length>20&&!!ph.querySelector("#gq")'));
  await R('stopTour()');
  // 6. the words' contrast: the bubble (light and dark) and the dock
  for (const dark of [false, true]) {
    await R(`S.dark=${dark};renderAll(false);startTour()`); await p.waitForTimeout(1500);
    const c = await R('var px=function(c){var m=c.match(/[\\d.]+/g).map(Number),k=c.indexOf("color(")===0?255:1;return{r:m[0]*k,g:m[1]*k,b:m[2]*k,a:m.length>3?m[3]:1}},lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},over=function(f,b){return{r:f.r*f.a+b.r*(1-f.a),g:f.g*f.a+b.g*(1-f.a),b:f.b*f.a+b.b*(1-f.a),a:1}},cr=function(a,b){var x=lum(a),y=lum(b);return Math.round((Math.max(x,y)+.05)/(Math.min(x,y)+.05)*100)/100};var bb=px(getComputedStyle(ph.querySelector(".gbubble")).backgroundColor),cs=getComputedStyle(document.querySelector(".cv")||ph),ink2=px(getComputedStyle(ph).getPropertyValue("--ink2")?getComputedStyle(ph.querySelector(".gbubble")).color:"rgb(0,0,0)"),db={r:22,g:24,b:28,a:1};[cr(px(getComputedStyle(ph.querySelector(".gbubble")).color),bb),cr(over({r:244,g:241,b:234,a:.66},db),db),cr(over({r:244,g:241,b:234,a:.72},db),db)]');
    ok(`${dark ? 'dark' : 'light'}: the bubble's words and the dock's labels at 4.5:1 or more`, c.every(v => v >= 4.5), c);
    await R('stopTour()');
  }
  await R('S.dark=false;renderAll(false)');
  // 7. Classic, as before
  await R('S.guideCfg.panel="classic";startTour()'); await p.waitForTimeout(600);
  ok('Panel: Classic: the captions box and the bar, no dock', await R('!ph.querySelector(".gdock")&&!!ph.querySelector(".gbar")'));
  await R('stopTour();S.guideCfg.panel="dock"');
  // 8. the Editor
  ok('Editor: Guide panel (then Guide button, round 7) before Captions, its rows and its words', await R('S.tab="guide";renderEditor();var f=ed.querySelector(\'[data-fo="s7dock"]\');!!f&&f.nextElementSibling.nextElementSibling===ed.querySelector(\'[data-fo="s7cap"]\')&&["Panel","Starts at","Azu\'s own words","Talk button","Sounds","Suggested questions"].every(function(t){return f.textContent.indexOf(t)>=0})&&(TXO.s7dock||[]).length===28'));  // round 11: 11 new words of the panel (dock_speed, dock_readall…)
  await R('S.tab="marker";renderEditor()');
  await p.locator('input[data-gask="0|en"]').fill('When was it built?'); await R('startTour();S.dockH="peek";renderGuide()'); await p.waitForTimeout(200);
  ok('Specific Marker: a suggested question, typed, on its chip', await R('ph.querySelector(\'[data-chip="0"]\').textContent') === 'When was it built?');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
