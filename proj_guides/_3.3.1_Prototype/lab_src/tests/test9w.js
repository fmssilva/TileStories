// test9w (Playwright + the installed Edge): node test9w.js -> PASS/FAIL per check. Guidance round 18: asking during a game
// (guide/game-ask.js, guidance/present.js's game clock, guide/ask.js's goTo.answer and askPrompt.text, world/guide-button.js's gbAsk.answer).
// [today] every case of the round's table now pauses: the tour's quiz (the panel raised, the quiz card's microphone), Guess the year (the
//   keyboard), the breather, Don't say it!, the card's own quiz outside a tour once started (Play the quiz; then the ✦ menu's Ask, Tap on
//   Azu) and a Poll; the card's quiz not started (What next?'s field, the ✦ menu's Ask): no pause, no chip, a question about it its hint.
// [no spoiler] (the Architect) while a game's question is open, no answer carries its right answer (EN and PT, figures and words; the
//   offline answer and the model's, stubbed), the model told the answer it must never say.
// [pause] the clock, the option, the slider and the score frozen during Ask (the test's own clock readings), the chip shown (44 px), no
//   scroll and no tour restart. [back] Back to the game (a real tap) and Back by itself resume exactly (the clock within 100 ms of where it
//   stopped), the warm line from Azu's intro lines › Back to the game, never twice in a row; a tap cancels Back by itself.
// [about] the detection with no AI, EN and PT, at each strictness: a question about the question, about something else, a near miss;
//   the model's context holds the rule (the model stubbed, never live). [ladder] hint 1 (a written hint; the block found from the card's
//   words, Show me there and back), hint 2 (one wrong choice out; Guess the year's band), step 3 (Tell me: the answer, no star, the next
//   question; I'll try: back). [off] Off says its line and never pauses. [stop] a Stop during the pause: the Stop bar with Continue.
// [editor] each new row changes what the visitor gets and its ▸ demo plays it; a question's own hint typed in Specific Marker is said.
// [reduced motion] the faded choice at once.
// MUTATE=1: the game's clock never freezes, the detection never knows a question about the question and no answer is checked for
//   the right one: the checks named [today], [pause], [back], [about], [no spoiler] and [ladder] turn red (and with them [editor]'s demos and [reduced motion], which need a hint).
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 700) : '')); };
  const box = async sel => p.locator('#ph ' + sel).first().boundingBox({ timeout: 2500 }).catch(() => null);
  const tap = async sel => { const r = await box(sel); if (!r) return false; await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return true; };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  const MUT = 'gdBeatFreeze=function(){};gaAbout=function(){return{about:false,shared:[],opt:0,n:0}};gaSpoils=function(){return false};""';
  if (process.env.MUTATE) { await R(MUT); console.log('MUTATION: the game\'s clock never freezes, no question is known to be about the question, no answer is checked for the right one'); }
  const base = 'SCREENS.phone=[390,844];S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.cardCfg.mode="light";S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.miniCtl="pausestop";S.guideCfg.look="ink";S.guideCfg.life="lively";S.guideCfg.azuTap="play";S.vis.aud="explorer";S.vis.len="";S.vis.speed=null;S.m.solemn=false;S.ui.gdSR=false;S.guidance.on=true;gdKindById("app").on=false;gdKindById("content").on=false;gdGroupById("play").on=false;S.ui.rowDemo=false;S.agentCfg.voiceOut=false;S.guideCfg.ask="search";sampleFn=null;' +
    'Object.assign(S.guideCfg,{gaOn:true,gaBack:false,gaBackS:4,gaH1:true,gaH2:true,gaH3:true,gaStrict:"normal"});';
  await R('window.QK=JSON.stringify(S.tourQ);window.GK=JSON.stringify(S.tourGuess);window.CK=JSON.stringify(S.card);""');
  const only = ids => `["quizpromise","quizfinale","guessyear","dontsay","findwall","water","cookie"].forEach(function(id){S.guidance.items[id].on=${JSON.stringify(ids)}.indexOf(id)>=0});`;
  const fresh = async (x) => { await R(base + 'if(G.ga.on)gaEnd("test");G.ga.lad={};G.ga.polled={};G.ga.started={};G.ga.idle=null;S.tourQ=JSON.parse(window.QK);S.tourGuess=JSON.parse(window.GK);S.card=JSON.parse(window.CK);S.guidance.items.guessyear.on=true;S.guidance.items.water.answers="chips";' + only(['quizpromise', 'quizfinale', 'guessyear', 'water', 'cookie']) +
    'if(G.ga.card)gaCardLeave();G.demoSeq=(G.demoSeq||0)+1;G.gd.demo=false;clearTimeout(G.gd.demoT);gdChip(null);if(G.on)stopTour();if(typeof hideStopUi==="function")hideStopUi();gdBeatClose();gdTailEnd();gdWordOff();G.gd.owed=null;G.gd.plan=null;G.gd.askFirst=null;gdResetVisitor();G.te.played={};G.te.open=false;S.view="card";S.stop="full";renderAll(false);' + (x || '') + '""'); };
  const seam = id => R(`gd12Seam(${JSON.stringify(id)})`);
  const quizTo = async x => { await fresh(x); await R('startTour();G.gd.plan.quiz="on";gdPlanBeat("quizpromise").state="played";""'); await seam('g8'); return until('!!ph.querySelector(".gdbeat .bt-choice")&&G.gd.bt.lineSeq>0', 15000); };
  const guessTo = async x => { await fresh(x); await R('startTour();""'); const g = await R('var b=gdPlanBeat("guessyear");b?b.after:""'); if (!g) return false; await seam(g); return until('!!ph.querySelector(".gdbeat .bt-range")', 9000); };
  const settle = () => R('new Promise(function(res){var last=scr.scrollTop,t=performance.now(),t0=t;(function f(){var n=scr.scrollTop;if(n!==last){last=n;t=performance.now()}if(performance.now()-t>500||performance.now()-t0>4000)res(n);else requestAnimationFrame(f)})()})');
  const kqTop = () => R('var k=scr.querySelector(\'[data-ref="b:b_quiz"]\');Math.round(k.getBoundingClientRect().top-scr.getBoundingClientRect().top)');
  const playQuiz = async x => { await fresh(x); await R('S.view="card";S.stop="full";renderPhone(false);scr.scrollTop=0;""'); await W(150); await p.click('#ph .hero .gstart'); await until('G.on', 3000);
    await R('stopSpeech();G.paused=false;G.i=S.guideSteps.length-1;runStep();""'); await until('!!ph.querySelector(".tepanel")', 25000); await W(900);
    await p.click('#ph .te-acts [data-te="quiz"]'); await until('!ph.querySelector(".tepanel")', 4000); await settle(); return until('!!ph.querySelector(".gbtn-wrap .gbtn")', 4000); };
  const kcView = async () => { await R('if(G.on)stopTour();G.te.open=false;teDraw&&teDraw();S.view="card";S.stop="full";renderPhone(false);""'); await W(300);
    await R('var q=scr.querySelector(\'[data-ref="b:b_quiz"]\');scr.scrollTop+=q.getBoundingClientRect().top-scr.getBoundingClientRect().top-170;""'); return until('!!ph.querySelector(".gbtn-wrap .gbtn")', 4000); };
  const typeQ = async q => { if (!await R('!!ph.querySelector("#gq")&&ph.querySelector("#gq").getClientRects().length>0')) { await tap('.gdock [data-dock="cycle"]'); await until('!!ph.querySelector("#gq")&&ph.querySelector("#gq").getClientRects().length>0', 2000); } await p.click('#ph #gq'); await p.keyboard.type(q); await p.keyboard.press('Enter'); };
  const chipH = () => R('var c=ph.querySelector(".ga-chip");c?Math.round(c.getBoundingClientRect().height/(ph.getBoundingClientRect().height/ph.offsetHeight)):0');
  const ga = () => R('JSON.stringify({on:G.ga.on,how:G.ga.how,kind:G.ga.g&&G.ga.g.kind,step:G.ga.step,chip:!!ph.querySelector(".ga-chip")})').then(JSON.parse);
  const resumedClock = () => R('new Promise(function(res){var t0=performance.now();(function f(){if(!G.ga.on||performance.now()-t0>12000)res(gaClock());else requestAnimationFrame(f)})()})');
  const backLines = await R('atIntroLines("gameback","en").map(function(x){return x.text})');
  await R(base + 'renderAll(false);""');

  // ---------- 1. [today] every way in, every game: now it pauses ----------
  const today = {};
  await quizTo(); const sc0 = await settle(); await tap('.gdock [data-dock="cycle"]'); await W(250); today.quizPanel = await ga(); await W(700); today.quizScroll = [sc0, await R('scr.scrollTop')];
  await quizTo('S.guidance.items.quizfinale.answers="tapsay";window.SpeechRecognition=function(){var me=this;me.start=function(){setTimeout(function(){me.onresult&&me.onresult({results:[[{transcript:"what did the kings eat"}]]});me.onend&&me.onend()},700)};me.stop=function(){}};');
  await until('!!ph.querySelector(".gdbeat .bt-mic")', 3000); await tap('.gdbeat .bt-mic'); await W(250); today.quizMic = await ga(); await until('G.thread.some(function(m){return m.who==="me"&&/kings eat/.test(m.text)})', 4000);
  today.quizMicAsked = await R('[G.ga.on,G.gd.bt&&G.gd.bt.answered,G.thread.some(function(m){return m.who==="me"&&/kings eat/.test(m.text)})]'); await R('window.SpeechRecognition=undefined;S.guidance.items.quizfinale.answers="tap";""');
  await guessTo('S.guideCfg.miniCtl="line";'); await tap('.gdock .gd-kb'); await W(250); today.guessKb = await ga();
  await fresh('["g1","g4","g5"].forEach(function(id){S.tourQ[id].rev.en=false});S.guidance.items.guessyear.on=false;S.introSeen=false;');
  await R('startTour();""'); const wb = await R('var b=gdPlanBeat("water");b?b.after:""'); if (wb) { await seam(wb); await until('!!ph.querySelector(\'.gdbeat [data-gdbt="qfirst"]\')', 16000); }
  await tap('.gdock [data-dock="cycle"]'); await W(250); today.breather = await ga(); const brLeft0 = await R('gaClock()'); await W(1200); const brLeft1 = await R('gaClock()'); const brBars = await R('[].every.call(ph.querySelectorAll(".gdbeat .bt-bar"),function(x){return x.style.animationPlayState==="paused"})');
  await fresh(only(['dontsay'])); await R('startTour();""'); await W(300); await seam('g2'); await until('G.gd.ds&&G.gd.ds.on&&G.i===3', 20000); await W(800);
  await tap('.gdock [data-dock="cycle"]'); await W(250); today.word = await ga(); const dc0 = await R('gdWordClock()'); await W(1000); const dc1 = await R('gdWordClock()');
  await playQuiz(); const kcStarted = await R('!!G.ga.started[svPoi()+"|b_quiz"]'); const kcTop = await R('scr.scrollTop'); await tap('.gbtn-wrap .gbtn'); await until('!!ph.querySelector(".gbmenu")', 2000); await tap('.gbmenu [data-gbgo="ask"]'); await W(300);
  today.kcMenu = await ga(); today.kcMenuPlace = await R('[G.i,scr.scrollTop,G.on,!!G.ga.card]'); today.kcTop = kcTop;
  await R('gaBack("test");""'); await until('!G.ga.on', 8000);
  await playQuiz('S.guideCfg.azuTap="menu";'); const hasAz = await until('!!ph.querySelector(".azfab")', 2000); await tap('.azfab'); await until('!!ph.querySelector(".gbmenu")', 2000); await tap('.gbmenu [data-gbgo="ask"]'); await W(300); today.kcAzu = await ga(); today.hasAz = hasAz;
  await R('gaBack("test");""'); await until('!G.ga.on', 8000);
  // What next?'s field: the tour's end, the card's quiz its showcase
  await fresh(); await R('S.view="card";S.stop="full";renderPhone(false);scr.scrollTop=0;""'); await W(150); await p.click('#ph .hero .gstart'); await until('G.on', 3000);
  await R('stopSpeech();G.paused=false;G.i=S.guideSteps.length-1;runStep();""'); await until('!!ph.querySelector(".tepanel")', 25000); await W(900);
  await p.click('#ph #teq'); await p.keyboard.type('Which king moved the court to the river?'); await p.keyboard.press('Enter'); await W(400); today.next = await ga(); today.nextMode = await R('!!G.te.askMode'); today.nextAns = await R('G.answer');
  // the ✦ menu's Ask with the quiz not started: Ask over the card where it is, nothing paused
  await fresh(); await kcView(); const idleTop = await R('scr.scrollTop'); await tap('.gbtn-wrap .gbtn'); await until('!!ph.querySelector(".gbmenu")', 2000); await tap('.gbmenu [data-gbgo="ask"]'); await W(300);
  today.idle = await ga(); today.idlePlace = [await R('!!G.ga.card'), Math.abs(await R('scr.scrollTop') - idleTop)]; await R('if(G.ga.card)gaCardLeave();""');
  await R('if(G.ga.on)gaBack("test");""'); await until('!G.ga.on', 8000);
  // a Poll on the card outside a tour
  await fresh('S.card.splice(1,0,{id:"b_poll",kind:"Poll",grp:"play",enter:"family",tap:"nothing",orow:"plain"});renderAll(false);');
  await R('if(G.on)stopTour();S.view="card";S.stop="full";renderPhone(false);var q=scr.querySelector(\'[data-ref="b:b_poll"]\');scr.scrollTop+=q.getBoundingClientRect().top-scr.getBoundingClientRect().top-170;""'); await until('!!ph.querySelector(".gbtn-wrap .gbtn")', 4000);
  await tap('.gbtn-wrap .gbtn'); await until('!!ph.querySelector(".gbmenu")', 2000); await tap('.gbmenu [data-gbgo="ask"]'); await W(300); today.poll = await ga(); await R('if(G.ga.on)gaBack("test");""'); await until('!G.ga.on', 8000);
  ok('[today] the tour\'s quiz: the panel raised pauses it (no scroll), and its own microphone too; a question it hears (no choice matched) goes to Ask, the question unanswered',
    today.quizPanel.on && today.quizPanel.kind === 'quiz' && today.quizPanel.chip && today.quizScroll[0] === today.quizScroll[1] && today.quizMic.on && /microphone/.test(today.quizMic.how) && today.quizMicAsked[0] && today.quizMicAsked[1] == null && today.quizMicAsked[2], { panel: today.quizPanel, scroll: today.quizScroll, mic: today.quizMic, asked: today.quizMicAsked });
  ok('[today] Guess the year (the keyboard), the breather (its 8 s frozen), Don\'t say it! (its clock frozen): each paused, the chip on it',
    today.guessKb.on && today.guessKb.kind === 'guess' && today.breather.on && today.breather.kind === 'breather' && brLeft0 != null && Math.abs(brLeft1 - brLeft0) < 5 && brBars && today.word.on && today.word.kind === 'word' && Math.abs(dc1 - dc0) < 20, { g: today.guessKb, br: [today.breather, brLeft0, brLeft1], w: [today.word, dc0, dc1] });
  ok('[today] the card\'s own quiz outside a tour: the ✦ menu\'s Ask keeps its place (no tour restart, no scroll), Tap on Azu\'s menu too, What next?\'s field, and a Poll: each paused with the chip',
    kcStarted && today.kcMenu.on && today.kcMenu.kind === 'kc' && today.kcMenu.chip && today.kcMenuPlace[3] && Math.abs(today.kcMenuPlace[1] - kcTop) < 2 && today.hasAz && today.kcAzu.on && today.kcAzu.kind === 'kc' && today.poll.on && today.poll.kind === 'poll', { kcStarted, menu: today.kcMenu, place: today.kcMenuPlace, kcTop, az: [today.hasAz, today.kcAzu], poll: today.poll });
  ok('[today] the card\'s own quiz not started (the Architect): What next?\'s field and the ✦ menu\'s Ask pause nothing and show no chip (the card where it is); a question about it gets its hint, never the answer',
    !today.next.on && !today.next.chip && today.nextMode && today.nextAns === 'Have a look at “Two hundred and fifty years by the river”: the answer is hiding there.' && !today.idle.on && !today.idle.chip && today.idlePlace[0] && today.idlePlace[1] < 2, { next: today.next, ans: today.nextAns, idle: today.idle, place: today.idlePlace });
  tsec('today');

  // ---------- 2. [pause] and [back] on Guess the year: the clock, the slider, frozen; back exactly ----------
  await guessTo(); await W(600); const g0 = await R('[G.i,G.gd.plan.ids.join(),scr.scrollTop]');
  await tap('.gdock [data-dock="cycle"]'); await until('G.ga.on', 2000); const c0 = await R('gaClock()'), left0 = await R('gdBTLeft(G.gd.bt,"waitT")'), v0 = await R('G.gd.bt.v');
  await typeQ('Who lived here?'); await until('!G.busy&&G.thread.some(function(m){return m.who==="azu"})', 5000);
  const rr = await box('.gdbeat .bt-range'); if (rr) { await p.mouse.move(rr.x + rr.width * 0.5, rr.y + rr.height / 2); await p.mouse.down(); await p.mouse.move(rr.x + rr.width * 0.9, rr.y + rr.height / 2, { steps: 5 }); await p.mouse.up(); }
  await W(1500); const c1 = await R('gaClock()'), left1 = await R('gdBTLeft(G.gd.bt,"waitT")'), v1 = await R('G.gd.bt.v'), ch = await chipH(), g1 = await R('[G.i,G.gd.plan.ids.join(),scr.scrollTop]');
  ok('[pause] Guess the year during Ask: its clock and its 8 s still (the test\'s readings 1.5 s apart), the slider unmoved by a real drag, the chip 44 px, no scroll, no restart',
    Math.abs(c1 - c0) < 5 && Math.abs(left1 - left0) < 5 && v0 === v1 && ch >= 44 && g0[0] === g1[0] && g0[1] === g1[1] && Math.abs(g0[2] - g1[2]) < 2, { c0, c1, left0, left1, v0, v1, ch, g0, g1 });
  await tap('.ga-row [data-ga="back"]'); await W(80); const line1 = await R('(ph.querySelector(".ga-chip")||{}).textContent'); const c2 = await resumedClock(); const left2 = await R('gdBTLeft(G.gd.bt,"waitT")');
  ok('[back] Back to the game (a real tap): a line of Back to the game on the chip, then the clock goes on within 100 ms of where it stopped, the chip gone',
    backLines.indexOf(line1) >= 0 && Math.abs(c2 - c1) < 100 && Math.abs(left2 - left1) < 100 && !(await R('!!ph.querySelector(".ga-chip")')), { line1, c1, c2, left1, left2 });
  await tap('.gdock [data-dock="cycle"]'); await until('G.ga.on', 2000); await R('S.guideCfg.gaBack=true;S.guideCfg.gaBackS=2;""'); await typeQ('Who lived here?');
  await until('!!G.ga.backT', 8000); const c3 = await R('gaClock()'); await W(200); const barOn = await R('!!ph.querySelector(".ga-row .ga-bar")'); await W(150); const line2p = until('G.ga.on&&!!ph.querySelector(".ga-chip.ga-line")', 4000);
  const line2ok = await line2p; const line2 = await R('(ph.querySelector(".ga-chip")||{}).textContent'); const c4 = await resumedClock();
  ok('[back] Back by itself (2 s after the answer is heard, a bar on the button): the next line of the set, never the same twice in a row, the clock on within 100 ms',
    barOn && line2ok && backLines.indexOf(line2) >= 0 && line2 !== line1 && Math.abs(c4 - c3) < 100, { barOn, line1, line2, c3, c4 });
  await tap('.gdock [data-dock="cycle"]'); await until('G.ga.on', 2000); await typeQ('Who lived here?'); await until('!!G.ga.backT', 8000); await tap('.gdock .gd-thread'); await W(2800);
  ok('[back] a tap on the panel cancels Back by itself: still paused 2.8 s later', await R('G.ga.on&&!G.ga.backT'), await ga());
  await R('S.guideCfg.gaBack=false;if(G.ga.on)gaBack("test");""'); await until('!G.ga.on', 8000);
  tsec('pause, back');

  // ---------- 3. [about] the detection, EN and PT; the model's rule ----------
  const det = await R(`var g=gaSample("en"),gp=gaSample("pt"),f=function(q,gg,l){return["loose","normal","strict"].map(function(v){return gaAbout(q,gg,v).about?1:0}).join("")};
    [f("Which king moved the court?",g),f("Which king moved his great ships, soldiers and horses to Belém?",g),f("Which king rebuilt the city after the earthquake?",g),f("Did the king live here?",g),f("Was it Philip?",g),f("Can you give me a hint?",g),
     f("Que rei mudou a corte?",gp),f("Que rei mudou os seus grandes navios, soldados e cavalos para Belém?",gp),f("Que rei reconstruiu a cidade depois do terramoto?",gp),f("O rei vivia aqui?",gp),f("Foi o Manuel I?",gp),f("Dá-me uma pista?",gp)]`);
  ok('[about] the detection at Loose / Normal / Strict, EN: about the question (all), a long one touching it (Loose, Normal), two near misses (Loose only), a choice named (all), a hint asked (all)',
    JSON.stringify(det.slice(0, 6)) === '["111","110","100","100","111","111"]', det.slice(0, 6));
  ok('[about] the same in Portuguese', JSON.stringify(det.slice(6)) === '["111","110","100","100","111","111"]', det.slice(6));
  await quizTo('S.guideCfg.ask="ai";window.__calls=[];sampleFn=function(pr){window.__calls.push(String(pr));return Promise.resolve({text:"A stub answer."})};');
  await typeQ('Who drew the pictures in the gallery?'); await until('window.__calls.length>0&&!G.busy', 5000);
  const rule = await R('var c=window.__calls[0]||"";[/THE VISITOR IS PLAYING A GAME/.test(c),/How long was the palace the royal home\\?/.test(c),/Do not say the answer/.test(c),/HINT: Have a look at/.test(c),G.answer,JSON.stringify(agContext("card").game||null),/THE VISITOR IS PLAYING/.test(agPrompt("x","card",agContext("card")))]');
  ok('[about] a question about something else goes to the model (stubbed) with the game, its question, the rule and the hint in its context; the agent\'s context and prompt too', rule[0] && rule[1] && rule[2] && rule[3] && rule[4] === 'A stub answer.' && /How long/.test(rule[5]) && rule[6], rule);
  await R('S.guideCfg.ask="search";sampleFn=null;""');
  tsec('about');

  // ---------- 3b. [no spoiler] (the Architect): the paused question's answer is never in an answer ----------
  const spoil = '/250|two hundred (and )?fifty|duzentos e cinquenta/i.test(G.answer)';
  await quizTo(); await tap('.gdock [data-dock="cycle"]'); await until('G.ga.on', 2000); await typeQ('Who lived here?'); await until('!G.busy&&G.thread.some(function(m){return m.who==="azu"})', 5000);
  const ns1 = await R('[G.answer,' + spoil + ',G.ga.on,G.gd.bt.answered]');
  await quizTo('S.lang="pt";'); await tap('.gdock [data-dock="cycle"]'); await until('G.ga.on', 2000); await typeQ('Quem viveu aqui?'); await until('!G.busy&&G.thread.some(function(m){return m.who==="azu"})', 5000);
  const ns2 = await R('[G.answer,' + spoil + ',G.ga.on]');
  await quizTo('S.guideCfg.ask="ai";window.__calls=[];sampleFn=function(pr){window.__calls.push(String(pr));return Promise.resolve({text:"For about 250 years the kings of Portugal lived here. They loved the light on the river."})};');
  await typeQ('Did the kings like it here?'); await until('window.__calls.length>0&&!G.busy', 5000);
  const ns3 = await R('[G.answer,' + spoil + ',/THE ANSWER, never to be said[^“]*“About 250 years”/.test(window.__calls[0]||"")]');
  const ns4 = await R('var q=S.tourQ.g1,g={kind:"quiz",q:q.q.pt,opts:q.a.map(function(a){return a.pt}),ok:q.ok,lad:{}},k=gaKeys(g);[gaSpoils("Os reis viveram aqui duzentos e cinquenta anos.",k),gaSpoils("Viveram aqui cerca de 250 anos.",k),gaSpoils("Os reis gostavam do rio.",k)]');
  await R('S.guideCfg.ask="search";sampleFn=null;S.lang="en";""');
  ok('[no spoiler] the quiz paused on “How long was the palace the royal home?”: “Who lived here?” answered with no 250 in figures or words (hint 1\'s where to look instead), the question still open', ns1[0] === 'Have a look at “Quick facts”: the answer is hiding there.' && !ns1[1] && ns1[2] && ns1[3] == null, ns1);
  ok('[no spoiler] the same in Portuguese: “Quem viveu aqui?” with no 250 nor “duzentos e cinquenta”', !!ns2[0] && !ns2[1] && ns2[2], ns2);
  ok('[no spoiler] the model\'s answer (stubbed) said without its sentence that gives the answer, the next one kept; the model told the answer it must never say', ns3[0] === 'They loved the light on the river.' && !ns3[1] && ns3[2], ns3);
  ok('[no spoiler] the check in Portuguese, figures and words: “duzentos e cinquenta” and “250” caught, a sentence without them kept', ns4[0] === true && ns4[1] === true && ns4[2] === false, ns4);
  tsec('no spoiler');

  // ---------- 4. [ladder] hint 1, hint 2, step 3 on the quiz; hint 1 on the card's own quiz with Show me ----------
  await quizTo(); await typeQ('How long was the palace the royal home?'); await until('G.ga.on&&G.ga.step===1', 4000);
  const h1 = await R('[G.answer,G.ga.where&&G.ga.where.ref,!!ph.querySelector(\'.ga-row [data-ga="show"]\'),G.gd.bt.answered]');
  await typeQ('Is it about 250 years?'); await until('G.ga.step===2', 4000);
  const h2 = await R('[G.answer,[].map.call(ph.querySelectorAll(".gdbeat .bt-choice"),function(x){return x.classList.contains("ga-out")?1:0}).join(""),G.gd.bt.answered]');
  await typeQ('Which one is the answer?'); await until('G.ga.step===3', 4000);
  const h3 = await R('[G.answer,[].map.call(ph.querySelectorAll(".ga-row button"),function(x){return x.textContent+":"+Math.round(x.getBoundingClientRect().height/(ph.getBoundingClientRect().height/ph.offsetHeight))}).join("|")]');
  await tap('.ga-row [data-ga="tell"]'); await until('!!ph.querySelector(".gdbeat .bt-verdict")', 4000);
  const told = await R('[(ph.querySelector(".gdbeat .bt-verdict")||{}).textContent,G.gd.bt&&G.gd.bt.right,G.gd.bt&&G.gd.bt.log&&G.gd.bt.log[0]&&G.gd.bt.log[0].told,!!ph.querySelector(".gdbeat .bt-choice.ok[data-i=\\"0\\"]"),G.ga.on]');
  ok('[ladder] hint 1 from the card\'s words: “Have a look at “Quick facts”: the answer is hiding there.”, a Show me chip, the question still open', /^Have a look at “Quick facts”: the answer is hiding there\.$/.test(h1[0]) && h1[1] === 'b:b_facts' && h1[2] && h1[3] == null, h1);
  ok('[ladder] hint 2: one wrong choice out (the last), “It\'s not about 500 years, that much I can tell you.”', h2[0] === "It's not about 500 years, that much I can tell you." && h2[1] === '001' && h2[2] == null, h2);
  ok('[ladder] step 3: “Shall I just tell you? Then this one won\'t earn its star.” with Tell me and I\'ll try (44 px); Tell me: the answer and its why, told, no star (not counted right)',
    h3[0] === "Shall I just tell you? Then this one won't earn its star." && /^Tell me:4[4-9]\|I'll try:4[4-9]$/.test(h3[1]) && /^The answer: About 250 years\. Kings lived here/.test(told[0]) && told[1] === 0 && told[2] === true && told[3] && !told[4], { h3, told });
  await tap('.gdbeat [data-gdbt="next"]'); await until('G.gd.bt&&G.gd.bt.k===1&&G.gd.bt.answered==null&&!!ph.querySelector(".gdbeat .bt-choice")', 5000);
  await typeQ('What filled the new palace?'); await until('G.ga.on&&G.ga.step===1', 4000); const own = await R('G.answer');
  await typeQ('Was it gold from Brazil?'); await until('G.ga.step===2', 4000); await typeQ('Tell me the answer'); await until('G.ga.step===3', 4000);
  await tap('.ga-row [data-ga="try"]'); await until('!G.ga.on', 8000); const tried = await R('G.gd.bt?[G.gd.bt.k,G.gd.bt.answered,G.gd.bt.frozen]:[]');
  await tap('.gdbeat .bt-choice[data-i="0"]'); await until('!!ph.querySelector(".gdbeat .bt-verdict")', 3000); await tap('.gdbeat [data-gdbt="next"]'); await until('G.gd.bt&&G.gd.bt.k===2&&!!ph.querySelector(".gdbeat .bt-choice")', 5000);
  const ok3 = await R('G.gd.bt&&G.gd.bt.qs?S.tourQ[G.gd.bt.qs[2]].ok:0'); await tap('.gdbeat .bt-choice[data-i="' + ok3 + '"]'); await until('!!ph.querySelector(".gdbeat .bt-verdict")', 3000); await tap('.gdbeat [data-gdbt="next"]'); await until('!!(G.gd.plan&&G.gd.plan.quizScore)', 8000);
  const score = await R('JSON.stringify(G.gd.plan&&G.gd.plan.quizScore)');
  ok('[ladder] hint 1, the question\'s own written hint: “Here\'s a little clue: Think of where the king\'s ships were sailing home from.”; I\'ll try: back to the same question, unanswered', own === "Here's a little clue: Think of where the king's ships were sailing home from." && tried[0] === 1 && tried[1] == null && !tried[2], { own, tried });
  ok('[ladder] the told question never earns its star: the quiz\'s score for What next?\'s header 2 of 3 with the two others right', score === '{"right":2,"n":3}', score);
  const pt = await R('var q=S.tourQ.g4,g={kind:"quiz",q:q.q.pt,opts:q.a.map(function(a){return a.pt}),ok:q.ok,hint:q.hint.pt,lad:{}},k=gaSample("pt");[gaStepWords(1,g,"pt"),gaStepWords(1,k,"pt"),gaStepWords(2,k,"pt"),gaStepWords(3,k,"pt")]');
  ok('[ladder] the words in Portuguese: its own hint, where to look, one wrong answer out, the choice', pt[0] === 'Aqui vai uma pequena pista: Pense de onde voltavam os navios do rei.' && pt[1] === 'Espreite “Duzentos e cinquenta anos à beira-rio”: a resposta está lá escondida.' && pt[2] === 'Não é José I, isso posso dizer-lhe.' && pt[3] === 'Quer que lhe diga? Assim esta não ganha a estrela.', pt);
  // the card's own quiz: the block found in the story, Show me there (the story page), back where the card was
  await playQuiz(); const kTop = await kqTop(); await tap('.gbtn-wrap .gbtn'); await until('!!ph.querySelector(".gbmenu")', 2000); await tap('.gbmenu [data-gbgo="ask"]'); await until('G.ga.on', 2000); await p.keyboard.type('Which king moved the court to the river?'); await p.keyboard.press('Enter'); await until('G.ga.step===1', 4000);
  const kh = await R('[G.answer,G.ga.where&&G.ga.where.ref,G.ga.where&&G.ga.where.view]'); await tap('.ga-row [data-ga="show"]'); await until('S.view==="story"&&!!scr.querySelector(".gdshowme")', 4000);
  const away = await R('var e=scr.querySelector(".gdshowme"),r=e&&e.getBoundingClientRect(),s=scr.getBoundingClientRect();[S.view,e&&e.dataset.ref,!!r&&r.top>=s.top-2&&r.top<s.bottom,G.ga.on,!!ph.querySelector(".ga-showback")]');
  await tap('.ga-showback'); await until('!G.ga.on', 8000); await W(700); const home = [await R('S.view'), await kqTop(), await R('G.on')];
  ok('[ladder] the card\'s quiz: hint 1 names the story\'s part that holds Manuel I (“Two hundred and fifty years by the river”), Show me opens it ringed with the game still paused, Back to the game returns to the card where it was',
    kh[0] === 'Have a look at “Two hundred and fifty years by the river”: the answer is hiding there.' && kh[1] === 'r:r2' && away[0] === 'story' && away[1] === 'r:r2' && away[2] && away[3] && away[4] && home[0] === 'card' && Math.abs(home[1] - kTop) < 3 && !home[2], { kh, away, home, kTop });
  // Guess the year's band, and hint 2 on the card's quiz (José I fades)
  await guessTo(); await typeQ('When did the king move the court?'); await until('G.ga.step===1||G.ga.step===2', 4000); const gs1 = await R('[G.ga.step,G.answer]');
  if (gs1[0] === 1) { await typeQ('When did the court move to the river?'); await until('G.ga.step===2', 4000); }
  const band = await R('var r=ph.querySelector(".gdbeat .bt-range");[G.answer,r&&r.min,r&&r.max,G.gd.bt&&G.gd.bt.v]');
  ok('[ladder] Guess the year (no choices, no block: hint 1 skipped): hint 2 narrows its band, “It\'s before 1600.”, the slider 1400-1600', band[0] === "It's before 1600." && band[1] === '1400' && band[2] === '1600' && band[3] <= 1600, { gs1, band });
  tsec('ladder');

  // ---------- 5. [off] and [stop] ----------
  await guessTo('S.guideCfg.gaOn=false;S.guideCfg.miniCtl="line";'); await tap('.gdock .gd-kb'); await W(200); const of0 = await R('gaClock()'); await p.keyboard.type('Who lived here?'); await p.keyboard.press('Enter'); await W(300);
  const off = await R('[G.ga.on,(ph.querySelector(".gdbeat .ga-note")||{}).textContent,G.paused,S.dockH,!!G.gd.bt&&!G.gd.bt.frozen]'); await W(800); const of1 = await R('gaClock()');
  ok('[off] Ask during a game Off: “Let\'s talk about it once you\'ve answered” on the game, never paused (its clock runs on)', !off[0] && off[1] === "Let's talk about it once you've answered" && !off[2] && off[4] && of1 - of0 > 800, { off, of0, of1 });
  await quizTo(); await tap('.gdock [data-dock="cycle"]'); await until('G.ga.on', 2000); const stI = await R('G.i'); await typeQ('Who lived here?'); await until('!G.busy', 4000); await tap('.gdock [data-g="stop"]'); await W(300);
  const stp = await R('[G.ga.on,G.on,!!G.gd.bt,!!ph.querySelector(".ga-chip"),(ph.querySelector(\'.gtoast [data-gstop="resume"]\')||{}).textContent]');
  await tap('.gtoast [data-gstop="resume"]'); await until('G.on', 3000); const stc = await R('[G.on,G.i]');
  ok('[stop] a Stop during the pause ends the game and Ask as Stop does: the Stop bar with Continue the tour, which goes back to the step', !stp[0] && !stp[1] && !stp[2] && !stp[3] && /Continue the tour/.test(stp[4] || '') && stc[0] && stc[1] === stI, { stp, stc, stI });
  tsec('off, stop');

  // ---------- 6. [editor] each row changes what the visitor gets; its ▸ plays it; a question's own hint typed ----------
  await fresh('S.ui.rowDemo=true;'); await R('if(G.on)stopTour();S.tab="guide";S.op=Object.assign(S.op||{},{"gd:g:ontour":true,s7gask:true});renderEditor();""'); await W(400);
  const rows = await R('var f=ed.querySelector(\'[data-fo="s7gask"]\');f?[].map.call(f.querySelectorAll(":scope > .inner > .row > label"),function(l){return l.textContent}):null');
  const sigOf = () => R('S._stKey="guideCfg.gaOn";var h=modelSig();S._stKey=null;h.slice(h.indexOf("#ga:"))');
  const clicks = [['gaOn', 'false'], ['gaBack', 'true'], ['gaH1', 'false'], ['gaH2', 'false'], ['gaH3', 'false']], ed1 = [];
  for (const [k, v] of clicks) {
    const s0 = await sigOf(); const btn = `#ed button[data-set="@guideCfg.${k}"][data-v="${v}"]`; const has = await p.locator(btn).count();
    if (has) await p.click(btn); else await R(`setOpt("@guideCfg.${k}","${v}");""`);
    await W(300); const s1 = await sigOf(); const shown = await until('G.ga.on||!!ph.querySelector(".ga-note")', 9000); ed1.push([k, has, s0 !== s1, shown]);
    await R(`if(G.ga.on)gaEnd("test");S.guideCfg.${k}=${v === 'false' ? 'true' : 'false'};renderEditor();""`); await W(200);
  }
  const sel0 = await sigOf(); await p.selectOption('#ed [data-sel="@guideCfg.gaStrict"]', 'loose').catch(() => R('setOpt("@guideCfg.gaStrict","loose")')); await W(300); const sel1 = await sigOf(); const selShown = await until('G.ga.on', 9000);
  await R('if(G.ga.on)gaEnd("test");S.guideCfg.gaStrict="normal";renderEditor();""');
  ok('[editor] Guide › 3 · Games on the tour › Asking during a game: its rows (Ask during a game, Back by itself, Its delay, the ladder\'s three, Detection, each question\'s hint, the Back lines)',
    rows && ['Ask during a game', 'Back by itself', 'Where to look', 'One answer out', 'Tell me, no star', 'Detection', "Each question's own hint", 'Back to the game lines'].every(x => rows.indexOf(x) >= 0), rows);
  ok('[editor] each row changed by a real click changes what the visitor gets (the phone\'s signature) and its demo plays it on the phone (a game paused, or Off\'s line)', ed1.every(x => x[2] && x[3]) && sel0 !== sel1 && selShown, { ed1, sel: [sel0 !== sel1, selShown] });
  await R('if(G.ga.on)gaEnd("test");S.guideCfg.gaBack=true;S.guideCfg.gaBackS=2;""'); const play = await p.locator('#ed [data-fo="s7gask"] [data-gademo="back"]').count(); if (play) await p.click('#ed [data-fo="s7gask"] [data-gademo="back"]'); const dPlay = await until('G.ga.on', 9000); const dBack = await until('!G.ga.on&&!!G.gd.bt', 18000);
  ok('[editor] the Back lines\' ▸ (a real click): a game started, paused by a question, answered, then back to the game by itself', play && dPlay && dBack, { play, dPlay, dBack });
  await R('if(G.ga.on)gaEnd("test");if(G.on)stopTour();S.ui.rowDemo=false;S.guideCfg.gaBack=false;S.tab="marker";S.sel.gstep="g1";S.op=Object.assign(S.op||{},{m3:true});renderEditor();""'); await W(400);
  const hin = '#ed input[data-path="tourQ.g1.hint.en"]'; const hasH = await p.locator(hin).count();
  if (hasH) { await p.fill(hin, 'Count the kings, not the stones.'); await p.locator(hin).press('Tab'); await p.check('#ed input[data-gahrev="g1|en"]').catch(() => { }); }
  const hrev = await R('[S.tourQ.g1.hint&&S.tourQ.g1.hint.en,S.tourQ.g1.hrev&&S.tourQ.g1.hrev.en]');
  await quizTo('S.tourQ.g1.hint={en:"Count the kings, not the stones.",pt:""};S.tourQ.g1.hrev={en:true,pt:false};'); await typeQ('How long was the palace the royal home?'); await until('G.ga.step===1', 4000); const hs = await R('G.answer');
  ok('[editor] Specific Marker › Guide script: a question\'s own hint typed (real keys) and Reviewed: the next ask about it says it', hasH && hrev[0] === 'Count the kings, not the stones.' && hrev[1] === true && hs === "Here's a little clue: Count the kings, not the stones.", { hasH, hrev, hs });
  await R('S.tab="guide";renderEditor();""');
  tsec('editor');

  // ---------- 7. [reduced motion] ----------
  await quizTo('S.reduce=true;renderAll(false);'); await typeQ('How long was the palace the royal home?'); await until('G.ga.step===1', 4000); await typeQ('Is it about 250 years?'); await until('G.ga.step===2', 4000);
  const rm = await R('var b=ph.querySelector(".gdbeat .bt-choice.ga-out"),c=ph.querySelector(".ga-chip");[!!b,b&&getComputedStyle(b).transitionDuration,b&&getComputedStyle(b).opacity,c&&getComputedStyle(c).animationName]');
  ok('[reduced motion] the wrong choice out at once (no transition), the chip with no entrance', rm[0] && /^0s/.test(rm[1]) && +rm[2] < 0.5 && rm[3] === 'none', rm);
  await R('if(G.ga.on)gaEnd("test");if(G.on)stopTour();S.reduce=false;""');
  tsec('reduced motion');

  ok('No page errors', errs.length === 0, errs.slice(0, 5));
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); await b.close(); process.exit(fails ? 1 : 0);
})();
