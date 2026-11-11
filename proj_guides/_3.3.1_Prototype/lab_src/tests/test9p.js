// test9p (Playwright + the installed Edge): node test9p.js -> PASS/FAIL per check. Guidance round 12, Fun › On the tour
// (research_tour_entertainment.md 3, 4.1, 4.3, 4.5, 5.1, 5.3): the tour's plan alone on gdTourPlan (deterministic; its rhythm and caps
// by length; never two seams in a row, nor in the first 20 s; pairs whole; Why not now? from the plan); the gates (Solemn, Calm, a screen
// reader, never over a sentence); Not now and the two-Not-now rule; the breather (its chips, 8 s, A question first, the prop beside Azu
// 0 px over words, the prop by date); the quiz (the promise and its star in every Look, Deal / No, questions only from steps heard, one per
// third, a right and a wrong answer by tap, one by the recogniser's text, Not sure, Show me, the score lines, Skip, unreviewed questions
// never playing, Draft with AI with sampleFn stubbed); guess the year (a real drag on the slider, the pins, the bands); every new row's
// demo; the eight scenes in Ask Azu's console (a stop then the ✦, a skipped step, too few questions, Expert's opt-in among them).
// MUTATE=1 node test9p.js lets the plan ignore the tour's cap: the checks named [cap] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const box = async sel => p.locator('#ph ' + sel).first().boundingBox();
  const tap = async sel => { const r = await box(sel); if (!r) return false; await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return true; };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  if (process.env.MUTATE) { await R('var gdTourPlan0=gdTourPlan;gdTourPlan=function(i,v,c,T){return gdTourPlan0(i,v,c,Object.assign({},T,{perTour:99,perShort:99,perDeep:99,every:1}))}'); console.log('MUTATION: the plan ignores the tour\'s cap and rhythm'); }
  // the device voice, muted (its words go on by time); only Fun › On the tour speaks; no introduction; Ink glass at Mini
  const base = 'S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.cardCfg.mode="light";S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.look="ink";S.guideCfg.life="lively";S.vis.aud="explorer";S.vis.len="";S.m.solemn=false;S.ui.gdSR=false;S.guidance.on=true;gdKindById("app").on=false;gdKindById("content").on=false;gdGroupById("play").on=false;S.ui.rowDemo=false;';
  await R('window.QK=JSON.stringify(S.tourQ);window.GK=JSON.stringify(S.tourGuess)');
  const fresh = async (x) => { await R(base + 'S.tourQ=JSON.parse(window.QK);S.tourGuess=JSON.parse(window.GK);S.guidance.items.guessyear.on=true;S.guidance.items.water.answers="chips";if(G.on)stopTour();gdBeatClose();G.gd.owed=null;G.gd.plan=null;G.gd.askFirst=null;gdResetVisitor();S.view="card";S.stop="full";renderAll(false);' + (x || '')); };
  // the tour moved to a step's seam (the steps up to it heard), then on: the beat planned there plays
  const seam = id => R(`gd12Seam(${JSON.stringify(id)})`);
  await R('window.QK=JSON.stringify(S.tourQ);window.GK=JSON.stringify(S.tourGuess)');
  const restoreQ = 'S.tourQ=JSON.parse(window.QK);S.tourGuess=JSON.parse(window.GK);';

  // ---------- 1. the plan, pure ----------
  const plan = await R(`var it=gdItems(),V=gdNewMemory(0),c={solemn:false,lively:true,sr:false,games:"on",notNow:0},mk=function(n,secs,o){var st=[{id:"t0",act:"teaser",secs:10}];for(var j=1;j<n;j++)st.push({id:"s"+j,act:"show",secs:secs,q:j%2===1,guess:j===4});st.push({id:"e",act:"end",secs:5});return Object.assign({steps:st,len:"standard",aud:"explorer",lang:"en",every:75,perTour:3,perShort:2,perDeep:5,auds:null},o||{})},
    sum=function(P){return P.beats.map(function(b){return b.id+"@"+b.seam})},gaps=function(P){var s=P.beats.map(function(b){return b.seam});return s.every(function(x,i){return!i||x-s[i-1]>=2})},o={};
    o.det=JSON.stringify(gdTourPlan(it,V,c,mk(10,30)))===JSON.stringify(gdTourPlan(it,V,c,mk(10,30)));
    var Ls=gdTourPlan(it,V,c,mk(12,25)),Sh=gdTourPlan(it,V,c,mk(12,25,{len:"short"})),Dp=gdTourPlan(it,V,c,mk(12,25,{len:"deep"})),Dl=gdTourPlan(it,V,c,mk(20,40,{len:"deep"})),Tn=gdTourPlan(it,V,c,mk(6,10));
    o.std=[Ls.budget,Ls.beats.length,sum(Ls)];o.short=[Sh.budget,Sh.beats.length];o.deep=[Dp.budget,Dp.beats.length];o.deepLong=[Dl.budget,Dl.beats.length];o.tiny=[Tn.budget,Tn.beats.length,Tn.why.water];
    o.gaps=[Ls,Sh,Dp,Dl].every(gaps);o.first=[Ls,Dp,Dl].every(function(P){return P.beats.every(function(b){return b.place==="opening"||P.at[b.seam]>=20})});
    o.pair=[Ls,Sh,Dp,Dl,Tn].every(function(P){var a=P.beats.some(function(b){return b.id==="quizpromise"}),f=P.beats.some(function(b){return b.id==="quizfinale"});return a===f});
    o.full=Sh.why.water||Sh.why.guessyear;o.sol=gdTourPlan(it,V,Object.assign({},c,{solemn:true}),mk(12,25));o.sol={n:o.sol.beats.length,w:o.sol.why.quizpromise};
    var Ca=gdTourPlan(it,V,Object.assign({},c,{lively:false}),mk(20,40,{len:"deep"}));o.calm=[sum(Ca),Ca.why.water,Ca.why.cookie];
    var srIt=it.map(function(x){return x.id==="water"?Object.assign({},x,{srPath:false}):x}),Sr=gdTourPlan(srIt,V,Object.assign({},c,{sr:true}),mk(20,40,{len:"deep"}));o.sr=Sr.why.water;
    var Off=gdTourPlan(it,V,Object.assign({},c,{games:"off"}),mk(20,40,{len:"deep"}));o.games=[sum(Off),Off.why.quizpromise];
    var Nn=gdTourPlan(it,V,Object.assign({},c,{notNow:2}),mk(20,40,{len:"deep"}));o.notnow=Nn.why.guessyear;o.words=gdPlanWords(Ls);o;`);
  ok('The plan is deterministic: the same tour and memory give the same plan', plan.det);
  // round 13: the quiz's promise and the quiz count as one beat, so each plan holds one beat more than its room counts
  ok('[cap] Its rhythm by length: a 4:50 Standard tour has room for 3 beats (one per 75 s, capped at 3; the quiz pair as one: 4 in the plan), Short 2 (3), Deep 4 (its narration), a 13:25 Deep tour room for 5 (4 items fit: the cookie is a variant of the breather); a 1:05 tour 1 (the pair)', JSON.stringify([plan.std[0], plan.std[1], plan.short, plan.deep, plan.deepLong, plan.tiny[0]]) === JSON.stringify([3, 4, [2, 3], [4, 4], [5, 4], 1]) && plan.tiny[1] === 2, plan);
  ok('[cap] Never two seams in a row, never in the tour\'s first 20 s but the opening; a pair planned whole or not at all', plan.gaps && plan.first && plan.pair, plan.std);
  ok('[cap] An item left out says why in words ("the tour already has 2 beats…" on Short); a short tour leaves the breather out by its narration', /the tour already has 2 beats, the quiz's promise and the quiz as one/.test(plan.full) && /needs 90 s/.test(plan.tiny[2]), [plan.full, plan.tiny[2]]);
  ok('The plan in words: its narration, its room, each beat at its seam', /4:50 of narration/.test(plan.words) && /room for 3 beats/.test(plan.words) && /at the seam after step \d+/.test(plan.words), plan.words);
  ok('Gates in the plan: a Solemn point gets no beats; Calm only the items Calm too (no breather, no cookie); a screen reader skips an item with no button path; Games on the tour Off and two Not nows: no games (the breather still)',
    plan.sol.n === 0 && /Solemn/.test(plan.sol.w) && plan.calm[0].every(x => !/water|cookie/.test(x)) && /Calm/.test(plan.calm[1]) && /screen reader/.test(plan.sr) &&
    plan.games[0].every(x => /water|cookie/.test(x)) && /Games on the tour Off/.test(plan.games[1]) && /two Not nows/.test(plan.notnow), plan);
  const pq = await R(`var st=[{id:"a",at:10,q:true,heard:true,from:5},{id:"b",at:20,q:true,heard:true,from:5},{id:"c",at:40,q:true,heard:false,from:5},{id:"d",at:50,q:true,heard:true,from:5},{id:"e",at:80,q:true,heard:true,from:5},{id:"f",at:85,q:false,heard:true,from:5}];
    [gdQuizPick(st,3,95),gdQuizPick(st.map(function(x){return x.id==="d"||x.id==="e"?Object.assign({},x,{heard:false}):x}),3,95),gdQuizMatch("the second one",["About 7,000","About 70,000","About 700,000"],"en"),gdQuizMatch("I think 1755",["1700","1755"],"en"),gdQuizMatch("an earthquake",["A war","An earthquake, a fire and a tsunami","A great flood"],"en"),gdQuizMatch("a terceira",["x","y","z"],"pt"),gdQuizMatch("no idea",["A war","A flood"],"en"),
     [1500,1510,1550,1700,1410].map(function(v){return gdGuessBand({n:1500,from:1400,to:1700},v)}),gdQOk(S.tourQ.g6,"en"),gdQOk(S.tourQ.g1,"en")]`);
  ok('The questions: only from steps heard, one from each third, never two from a step; too few heard: fewer (the quiz saves itself)', JSON.stringify(pq[0]) === '["a","d","e"]' && JSON.stringify(pq[1]) === '["a","b"]', pq.slice(0, 2));
  ok('The speech path: the recogniser\'s text matched to a choice ("the second one", "1755", "an earthquake", "a terceira"); nothing matched: -1 (tap still)', JSON.stringify(pq.slice(2, 7)) === '[1,1,1,2,-1]', pq.slice(2, 7));
  ok('Guess the year\'s bands (3, by distance in the author\'s range): spot on, close, further', JSON.stringify(pq[7]) === '[1,1,2,3,3]', pq[7]);
  ok('An unreviewed question never plays (the AI draft of "Drag across"); a reviewed one does', pq[8] === false && pq[9] === true);
  const props = await R('var w=S.guidance.items.water;[gdPropOf(w,"07-15",[]),gdPropOf(w,"01-10",[]),gdPropOf(w,"04-01",[]),gdPropOf(w,"07-15",["fan"]),gdPropOf(S.guidance.items.cookie,"07-15",[])]');
  ok('The breather\'s prop by date: a fan in summer, a scarf in winter, the glass else; never the same twice in a visit; the cookie its own', JSON.stringify(props) === '["fan","scarf","water","water","cookie"]', props);
  tsec('pure');

  // ---------- 2. the promise and its star, in every Look ----------
  await fresh(); await R('startTour()');
  const p0 = await R('[gdPlanWords(G.gd.plan),G.gd.plan.beats.map(function(b){return b.id+":"+b.after})]');
  // round 13: the promise and its quiz count as one beat, so the demo tour gets the pair and one more (Guess the year); the quiz replaces the card's own quiz step (g9), so the finale is after g8
  ok('[cap] A tour plans once as it starts: the demo tour (no introduction) has room for 2 beats, the quiz\'s pair as one (after the welcome, before the end) and Guess the year', JSON.stringify(p0[1]) === '["quizpromise:g0","guessyear:g3","quizfinale:g8"]', p0);
  await until('gd12Kind("promise")', 30000);
  const pr = await R('[G.i,S.guideSteps[G.i].id,TXT("tour_promise")===ph.querySelector(".gdbeat .bt-line").textContent,!!ph.querySelector(".gdock .gd-qstar.pend"),G.sayLine&&G.sayLine.text===TXT("tour_promise"),gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"findwall")]');
  ok('The promise after the welcome (the tour held at its seam): its line spoken, the star pending at the end of the progress line; Why not now? for another beat answers from the plan (round 13: Find it on the wall, beside Guess the year now planned with the pair)', pr[1] === 'g0' && pr[2] && pr[3] && pr[4] && /never two seams in a row: “Guess the year”/.test(pr[5]), pr);
  const looks = await R('var o={};DLOOKS.forEach(function(L){S.guideCfg.look=L[0];["mini","peek"].forEach(function(h){S.dockH=h;renderGuide();var s=ph.querySelector(".gdock .gd-qstar"),pr=ph.getBoundingClientRect(),r=s&&s.getBoundingClientRect(),c=ph.querySelector(".gdbeat");o[L[0]+"|"+h]=!!s&&r.left>=pr.left&&r.right<=pr.right&&r.top>=pr.top&&r.bottom<=pr.bottom&&!!c&&c.classList.contains("bt-"+L[0])})});S.guideCfg.look="ink";S.dockH="mini";renderGuide();o');
  ok('The star and the card in every round-11 Look, at Mini and Open (inside the phone; the card in the Look\'s colours)', Object.values(looks).length === 8 && Object.values(looks).every(Boolean), looks);
  // round 13: the promise's chips stay up on the strip above the panel while step 1 starts
  await until('!!ph.querySelector(".gdstrip .bt-chip")', 15000);
  const chips = await R('[].map.call(ph.querySelectorAll(".gdstrip .bt-chip"),function(x){var r=x.getBoundingClientRect();return[x.textContent,Math.round(r.height)]})');
  ok('Deal / No quiz, thanks: 44 px or more', chips.length === 2 && chips[0][0] === 'Deal' && chips.every(c => c[1] >= 44), chips);
  await tap('.gdstrip [data-gdtl="deal"]');
  await until('!G.gd.bt&&G.i>=1&&G.gd.plan.quiz==="on"', 4000);
  ok('Deal (a real tap): the quiz is on, the star full, the tour goes on to its next step', await R('G.gd.plan.quiz==="on"&&!!ph.querySelector(".gdock .gd-qstar:not(.pend)")&&G.i>=1'));
  // waiting is a Deal; No quiz, thanks drops the quiz and is a Not now
  await fresh(); await R('startTour()'); await until('gd12Kind("promise")', 30000);
  const wt = await until('G.gd.plan.quiz==="on"', 20000);
  ok('4 s with no answer after the promise: a Deal (a game starts)', wt && await R('G.gd.plan.quiz==="on"'));
  await fresh(); await R('startTour()'); await until('!!ph.querySelector(\'.gdstrip [data-gdtl="noquiz"]\')', 30000); await tap('.gdstrip [data-gdtl="noquiz"]'); await until('G.gd.plan.quiz==="off"', 4000);
  const nq = await R('var f=gdPlanBeat("quizfinale");[G.gd.plan.quiz,f.state,f.why,S.gdVisitor.visit.notNow,gdQuizStar(),gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"quizfinale")]');
  ok('No quiz, thanks (a real tap): the quiz dropped from the plan, the star gone, one Not now; Why not now? says so', nq[0] === 'off' && nq[1] === 'dropped' && /No quiz/.test(nq[2]) && nq[3] === 1 && nq[4] === '' && /dropped/.test(nq[5]), nq);
  tsec('promise');

  // ---------- 3. the quiz ----------
  await fresh(); await R('startTour();G.gd.plan.quiz="on";gdPlanBeat("quizpromise").state="played";G.gd.heard={}'); await seam('g8');
  await until('gd12Kind("finale")&&!!ph.querySelector(".gdbeat .bt-choice")', 15000);
  const q1 = await R('var B=G.gd.bt;[B.qs,B.k,ph.querySelector(".gdbeat .bt-eye").textContent,[].map.call(ph.querySelectorAll(".gdbeat .bt-choice"),function(x){return Math.round(x.getBoundingClientRect().height)})]');
  ok('The finale: three questions from the steps heard (one from each third), the count, choices 44 px or more, no timer', JSON.stringify(q1[0]) === '["g1","g4","g7"]' && /QUESTION 1 OF 3/.test(q1[2]) && q1[3].every(h => h >= 44), q1);
  await tap('.gdbeat .bt-choice[data-i="0"]'); await until('!!ph.querySelector(".gdbeat .bt-verdict")', 3000);
  const r1 = await R('[ph.querySelector(".gdbeat .bt-verdict").textContent,!!ph.querySelector(".gdbeat .bt-choice.ok[data-i=\\"0\\"]"),G.gd.bt.right,G.gd.bt.log[0].how]');
  ok('A right answer by tap: the tick on it, a "Yes!" line, counted', r1[1] && r1[2] === 1 && /Yes|That|Spot|Right/.test(r1[0]) && r1[3] === 'tap', r1);
  await tap('.gdbeat [data-gdbt="next"]'); await until('G.gd.bt&&G.gd.bt.k===1&&G.gd.bt.answered==null&&!!ph.querySelector(".gdbeat .bt-choice")', 4000);
  await tap('.gdbeat .bt-choice[data-i="2"]'); await until('!!ph.querySelector(".gdbeat .bt-verdict")', 3000);
  const r2 = await R('[ph.querySelector(".gdbeat .bt-verdict").textContent,!!ph.querySelector(".gdbeat .bt-choice.no[data-i=\\"2\\"]"),!!ph.querySelector(".gdbeat .bt-choice.ok[data-i=\\"0\\"]"),!!ph.querySelector(\'.gdbeat [data-gdbt="showme"]\')]');
  ok('A wrong answer by tap: the cross on it, the tick on the right one, "Not quite. The answer: … " with its why (never mocking), Show me', /^Not quite. The answer: The wealth of the India voyages\. Manuel I filled it/.test(r2[0]) && r2[1] && r2[2] && r2[3], r2);
  await tap('.gdbeat [data-gdbt="showme"]'); await until('!!ph.querySelector(".gdshowme")', 3000);
  const sm = await R('var e=ph.querySelector(".gdshowme"),r=e&&e.getBoundingClientRect(),pr=ph.getBoundingClientRect();[!!e,e&&e.dataset.ref,r&&r.top>=pr.top-2&&r.top<pr.bottom,!!ph.querySelector(".gdbeat.bt-away"),!!ph.querySelector(".gdshowback")]');
  ok('Show me (a real tap): the block the question came from, in view and ringed, the card waiting, a way back', sm[0] && sm[1] === 'r:r2' && sm[2] && sm[3] && sm[4], sm);
  await tap('.gdshowback'); await until('G.gd.bt&&G.gd.bt.k===2&&G.gd.bt.answered==null&&!!ph.querySelector(".gdbeat .bt-choice")', 4000);
  await R('heard("it was an earthquake")'); await until('!!ph.querySelector(".gdbeat .bt-verdict")', 3000);
  const r3 = await R('[G.gd.bt.log[2],G.gd.bt.right]');
  ok('An answer by the recogniser\'s text (the dock\'s heard: "it was an earthquake"): matched to its choice, right', r3[0] && r3[0].ok && /said/.test(r3[0].how) && r3[1] === 2, r3);
  await tap('.gdbeat [data-gdbt="next"]'); await until('!!ph.querySelector(".gdbeat .bt-score")', 6000);
  const sc = await R('[ph.querySelector(".gdbeat .bt-line").textContent,ph.querySelector(".gdbeat .bt-score b").textContent,JSON.stringify(gdQuizBest(G.gd.plan.point))]');
  ok('The score: "2 out of 3. Very good." and the best here kept on the phone', sc[0] === '2 out of 3. Very good.' && sc[1] === '2/3' && sc[2] === '{"right":2,"n":3}', sc);
  await until('!G.gd.bt', 8000);
  ok('…then the tour goes on to its last step', await R('S.guideSteps[G.i].act==="end"'));
  const lines = await R('[[3,3],[2,3],[1,3],[0,3]].map(function(x){var r=x[0],n=x[1];return fmt(TXT(r===n?"tour_score_all":r===0?"tour_score_none":r*2>n?"tour_score_most":"tour_score_some"),r,n)})');
  ok('The score lines, never mocking (a zero still ends on what was learnt)', JSON.stringify(lines) === JSON.stringify(["3 out of 3! You'd make a fine guide.", '2 out of 3. Very good.', '1 out of 3. The tiles are tricky!', 'Zero… but now you know 3 new things!']), lines);
  // Not sure and Skip the quiz
  await fresh(); await R('startTour();G.gd.plan.quiz="on";gdPlanBeat("quizpromise").state="played"'); await seam('g8'); await until('!!ph.querySelector(".gdbeat .bt-choice")', 15000);
  await tap('.gdbeat [data-gdbt="unsure"]'); await until('!!ph.querySelector(".gdbeat .bt-verdict")', 3000);
  const ns = await R('[ph.querySelector(".gdbeat .bt-verdict").textContent,!!ph.querySelector(".gdbeat .bt-choice.ok"),!ph.querySelector(".gdbeat .bt-choice.no")]');
  ok('Not sure (a real tap): the right one shows, "The answer: …" with its why', /^The answer: About 250 years\./.test(ns[0]) && ns[1] && ns[2], ns);
  await tap('.gdbeat [data-gdbt="skip"]'); await until('!G.gd.bt', 3000);
  ok('Skip the quiz (a real tap): the quiz ends, the tour goes on to its plain end', await R('!ph.querySelector(".gdbeat")&&G.gd.plan.quizDone&&S.guideSteps[G.i].act==="end"'));
  // too few from the steps heard: saved for next time, the promise kept honestly
  await fresh(); await R('startTour();G.gd.plan.quiz="on";gdPlanBeat("quizpromise").state="played"'); await R('var j=S.guideSteps.findIndex(function(s){return s.id==="g8"});stopSpeech();G.i=j;G.gd.heard={g1:1};next()');
  await until('gd12Kind("finale")', 6000);
  ok('Fewer than 2 questions from the steps heard: "We went fast! I\'ll save the quiz for next time.", the star goes', await R('ph.querySelector(".gdbeat .bt-line").textContent===TXT("tour_quiz_few")&&G.gd.plan.quiz==="off"&&!gdQuizStar()'));
  tsec('quiz');

  // ---------- 4. a skipped step drops its beat; never over a sentence ----------
  await fresh(); await R('startTour()'); await W(400); await R('stopSpeech();G.i=0;G.gd.heard={};next()');
  const sk = await R('var b=gdPlanBeat("quizpromise");[b.state,b.why,gdPlanBeat("quizfinale").state,!!G.gd.bt]');
  ok('A seam reached by Next on a step not heard: its beat drops (the promise, and its quiz with it)', sk[0] === 'dropped' && /skipped/.test(sk[1]) && sk[2] === 'dropped' && !sk[3], sk);
  const sp = await R('var it=gdItems().filter(function(x){return x.id==="quizpromise"})[0],P=gdTourPlan(gdItems(),S.gdVisitor,gdPlanCtx(),gdTourIn(S.guideSteps)),c=Object.assign(gdContext(),{plan:P,moment:"between",speaking:false,tour:{step:0,stepId:"g0"}});[gdBeatCheck(it,Object.assign({},c,{speaking:true})).no,gdBeatCheck(it,c).ok,gdBeatCheck(it,Object.assign({},c,{moment:null})).no]');
  ok('Never over a sentence: at its seam a beat waits while the guide speaks; it plays only at its own seam', /never over a sentence/.test(sp[0]) && sp[1] === true && /planned for the seam after step 1/.test(sp[2]), sp);
  // Fun's hold does not hold a beat; Fun's visit cap does not count it
  const hold = await R('gdGroupById("play").on=true;var items=gdItems(),c=Object.assign(gdContext(),{tour:{paused:false,between:true,step:3}});[gdCheck(items.filter(function(x){return x.id==="joke"})[0],items,S.gdVisitor,c,gdNow()).no,/held while a tour plays/.test(gdCheck(items.filter(function(x){return x.id==="water"})[0],items,S.gdVisitor,c,gdNow()).no||"")]');
  ok('Fun\'s hold in a tour (round 11) still holds the rest of Fun, never a beat of the tour', /held while a tour plays/.test(hold[0]) && hold[1] === false, hold);
  tsec('gates');

  // ---------- 5. the breather ----------
  await fresh(restoreQ + '["g1","g4","g5"].forEach(function(id){S.tourQ[id].rev.en=false});S.guidance.items.guessyear.on=false;S.introSeen=false');
  await R('startTour()'); const wb = await R('var b=gdPlanBeat("water");b&&b.after'); await seam(wb);
  await until('!!ph.querySelector(".azmoment")', 12000);
  const pp = await R('[G.moment,gd12Over(),!!ph.querySelector(".gdbeat .bt-line"),(function(){var m=ph.querySelector(".mascot").getBoundingClientRect(),a=ph.querySelector(".azmoment").getBoundingClientRect();return Math.round(Math.hypot(m.left-a.left,m.top-a.top))})()]');
  ok('The breather: its line in the card, then Azu drinks, the glass beside Azu, 0 px over any words', pp[0] === 'water' && pp[1] === 0 && pp[2] && pp[3] < 160, pp);
  await until('!!ph.querySelector(\'.gdbeat [data-gdbt="goon"]\')', 8000);
  const t8 = Date.now(); const went = await until('!G.gd.bt', 14000); const secs = (Date.now() - t8) / 1000;
  ok('Go on / A question first; 8 s with nothing: "Let\'s continue." and the next step', went && secs > 7.5 && await R('S.guideSteps[G.i-1]&&G.gd.plan.beats.some(function(b){return b.id==="water"&&b.state==="played"})'), secs);
  await fresh(restoreQ + '["g1","g4","g5"].forEach(function(id){S.tourQ[id].rev.en=false});S.guidance.items.guessyear.on=false;S.introSeen=false;S.guidance.items.water.answers="chips"');
  await R('startTour()'); const wb2 = await R('gdPlanBeat("water").after'); await seam(wb2); await until('!!ph.querySelector(\'.gdbeat [data-gdbt="qfirst"]\')', 15000);
  const at0 = await R('G.i'); await tap('.gdbeat [data-gdbt="qfirst"]'); await W(300);
  const qf = await R('[S.dockH,!!G.gd.askFirst,!!ph.querySelector(".gdock #gq"),G.i]');
  ok('A question first (a real tap): the panel opens (Peek) with the talk button listening, the tour waits', /peek|full/.test(qf[0]) && qf[1] && qf[2] && qf[3] === at0, qf);
  await p.locator('#ph .gdock #gq').fill('What happened in 1755?'); await p.keyboard.press('Enter');
  const back = await until('G.gd.bt&&G.gd.bt.kind==="back"', 25000); await until('!G.gd.bt&&G.i>' + at0, 8000);
  ok('…the question answered, "Back to our story." and the next step', back && await R('G.i>' + at0));
  tsec('breather');

  // ---------- 6. guess the year: a real drag, the pins ----------
  await fresh(restoreQ + '["g1","g4","g5"].forEach(function(id){S.tourQ[id].rev.en=false})'); await R('startTour()');
  const gb = await R('gdPlanBeat("guessyear").after'); await seam(gb); await until('!!ph.querySelector(".gdbeat .bt-range")', 8000);
  const rb = await box('.gdbeat .bt-range'); const v0 = await R('+ph.querySelector(".gdbeat .bt-range").value');
  await p.mouse.move(rb.x + rb.width / 2, rb.y + rb.height / 2); await p.mouse.down(); await p.mouse.move(rb.x + rb.width * 0.72, rb.y + rb.height / 2, { steps: 8 }); await p.mouse.up();
  const v1 = await R('[+ph.querySelector(".gdbeat .bt-range").value,ph.querySelector(".gdbeat .bt-val").textContent,G.gd.bt.touched]');
  ok('Guess the year: the slider in the author\'s range (1400-1700, at the middle); a real drag moves it and its number', v0 === 1550 && v1[0] > 1600 && String(v1[0]) === v1[1] && v1[2], [v0, v1]);
  await tap('.gdbeat [data-gdbt="tell"]'); await until('!G.gd.bt', 5000);
  const g4 = await R('[S.guideSteps[G.i].id,G.gd.guess&&G.gd.guess.v]');
  ok('Tell me (a real tap): the step that holds the year plays (it says it)', g4[0] === 'g4' && g4[1] === v1[0], g4);
  await R('G.gd.heard.g4=1;stopSpeech();next()'); await until('!!ph.querySelector(".gdbeat .bt-pin.ans")', 5000);
  const pins = await R('var y=ph.querySelector(".bt-pin.you"),a=ph.querySelector(".bt-pin.ans");[y.textContent,a.textContent,parseFloat(y.style.left)>parseFloat(a.style.left),ph.querySelector(".gdbeat .bt-line").textContent,G.gd.bt.band]');
  ok('After it: the two pins (the guess, the answer) and Azu\'s reaction by its band, never mocking', /Your guess · \d+/.test(pins[0]) && /The answer · 1500/.test(pins[1]) && pins[2] && /older than you'd think/.test(pins[3]) && pins[4] === 3, pins);
  await until('!G.gd.bt', 9000);
  // a game's Not now (4 s): two this visit end the games for the visit, said once
  await fresh(restoreQ + '["g1","g4","g5"].forEach(function(id){S.tourQ[id].rev.en=false})'); await R('startTour()');
  await seam(await R('gdPlanBeat("guessyear").after')); await until('!!ph.querySelector(\'.gdbeat [data-gdbt="notnow"]\')', 8000);
  const nn = await R('var c=ph.querySelector(\'.gdbeat [data-gdbt="notnow"] .bt-bar\');c?getComputedStyle(c).animationDuration:""');
  await tap('.gdbeat [data-gdbt="notnow"]'); await until('!G.gd.bt', 4000);
  const n1 = await R('S.gdVisitor.visit.notNow'), gone = true;
  await R('if(G.on)stopTour();G.gd.plan=null;S.tourQ=JSON.parse(window.QK);startTour()'); await until('!!ph.querySelector(\'.gdstrip [data-gdtl="noquiz"]\')', 30000); await tap('.gdstrip [data-gdtl="noquiz"]');
  await until('!!ph.querySelector(".gdstrip .tl-msg")', 4000);
  const n2 = await R('[S.gdVisitor.visit.notNow,(ph.querySelector(".gdstrip .tl-msg")||{}).textContent,S.gdVisitor.visit.nomore]');
  ok('A game\'s Not now chip (4 s) by a real tap; a second Not now this visit: "Got it: no more games today, just the story." once (on the strip: the step goes on), and no more games', nn === '4s' && n1 === 1 && n2[0] === 2 && n2[1] === "Got it: no more games today, just the story." && n2[2] && gone, [nn, n1, n2]);
  await until('!G.gd.bt', 9000);
  ok('…the next tour plans no game (two Not nows this visit), the breather still could', await R('if(G.on)stopTour();var P=gdPlanNow();!P.beats.some(function(b){return b.game})&&/two Not nows/.test(P.why.quizpromise)'));
  tsec('guess');

  // ---------- 7. Settings and the Editor ----------
  await fresh('S.settingsOpen=true;renderPhone(true)');
  await p.locator('#ph [data-gdpref="games|off"]').scrollIntoViewIfNeeded(); await p.locator('#ph [data-gdpref="games|off"]').click(); await W(150);
  ok('Settings › The guide › Games on the tour (a real tap Off): the visitor\'s switch, in its memory; the plan follows', await R('gdPrefs().games==="off"&&/Games on the tour Off/.test(gdPlanNow().why.quizpromise)'));
  await R('gdPrefs().games="on";S.settingsOpen=false;renderPhone(true)');
  // round 14: On the tour is 3 · Games on the tour (gd:g:ontour, its id kept); the quiz's pair drawn as one (gd:pair:quizpromise); each item's groups open
  const ed1 = await R('S.tab="guide";S.op={gt4:true,"gd:fun":true,"gd:g:ontour":true,"gd:pair:quizpromise":true};["quizpromise","quizfinale","guessyear","water","cookie"].forEach(function(id){S.op["gd:i:"+id]=true});Object.keys(S.op).forEach(function(k){var m=/^gd:i:(.+)$/.exec(k),it=m&&S.op[k]&&S.guidance.items[m[1]];if(it)gdFormOf(it).groups.forEach(function(g){if(g)S.op["gd:f:"+m[1]+":"+g.k]=true})});renderEditor();var f=ed.querySelector(\'[data-fo="gd:g:ontour"]\');[!!f,f&&f.querySelector("#gdplan").textContent,[].map.call(f.querySelectorAll(":scope > .inner > .row > label"),function(l){return l.textContent}),gdGroupById("ontour").every,gdGroupById("ontour").perTour,gdGroupById("ontour").perShort,gdGroupById("ontour").perDeep]');
  ok('Guide › 3 · Games on the tour: On, Liveliness, Audiences, One beat per (75), At most a tour (3; Short 2, Deep 5), and "This tour\'s plan: …"', ed1[0] && /This tour's plan/.test(ed1[1]) && ['On', 'Liveliness', 'Family', 'Explorer', 'Expert', 'One beat per (s)', 'At most a tour', 'At most on Short', 'At most on Deep'].every(x => ed1[2].indexOf(x) >= 0) && ed1[3] === 75 && ed1[4] === 3 && ed1[5] === 2 && ed1[6] === 5, ed1);
  const ed2 = await R('var labs=[].map.call(ed.querySelectorAll(\'[data-fo="gd:i:quizfinale"] .row > label,[data-fo="gd:i:water"] .row > label,[data-fo="gd:i:quizpromise"] .row > label\'),function(l){return l.textContent});labs');
  ok('A beat\'s fields: Place in the tour, The visitor answers by, Waits, Questions, Scores, Show me, Calm too, its Audiences, Tour length, After narration, Prop, For Expert', ['Place in the tour', 'The visitor answers by', 'Waits (s)', 'Questions', 'Scores', 'Show me after a wrong answer', 'Calm too', 'For Family', 'Tour length', 'After narration (s)', 'Prop', 'For Expert, it is'].every(x => ed2.indexOf(x) >= 0), ed2);
  // every new row's demo (a real click on its ▸)
  const keys = await R('var o=[];ed.querySelectorAll(\'[data-fo="gd:g:ontour"] .row\').forEach(function(r){var k=rowKey(r);if(k&&o.indexOf(k)<0)o.push(k)});var m=o.filter(function(k){return!rowDemoOf(k)});[o,m]');
  ok(`Every new row has its demo (${keys[0].length} rows of On the tour; none without)`, keys[0].length >= 50 && keys[1].length === 0, keys[1]);
  const sample = ['guidance.kinds.2.groups.0.every', 'guidance.kinds.2.groups.0.auds.expert', 'guidance.items.quizpromise.waits', 'guidance.items.quizfinale.qn', 'guidance.items.guessyear.calm', 'guidance.items.water.props', 'guidance.items.cookie.place'];
  const shown = [];
  for (const k of sample) {
    await R('if(G.on)stopTour();gdBeatClose();document.body.dataset.rowdemo="";S.ui.rowDemo=true;renderEditor()');
    const lb = p.locator(`.row:has([data-set="@${k}"],[data-sel="@${k}"],[data-num="@${k}"]) > label.rd`).first();
    if (await lb.count()) await lb.scrollIntoViewIfNeeded(); const bb = await lb.boundingBox(); if (!bb) { shown.push(k + ' (no ▸)'); continue; }
    await p.mouse.click(bb.x + bb.width - 12, bb.y + bb.height / 2);
    const got = await until('!!ph.querySelector(".gdbeat")', 6000); shown.push(k + (got ? '|beat' : '|none'));
  }
  ok('Each kind of new row, its ▸ pressed (a real click): its beat on the phone, in a tour, now', shown.every(s => /\|beat$/.test(s)), shown);
  await R('if(G.on)stopTour();gdBeatClose();S.ui.rowDemo=false');
  // Specific Marker › 3 · Guide script: a step's question and guess; Draft with AI with sampleFn stubbed; Reviewed
  await R('S.tab="marker";S.op.m3=true;S.sel.gstep="g3";renderEditor()');
  await R('window.SF0=sampleFn;sampleFn=function(p){window.SFP=p;return Promise.resolve({text:\'{"q":"How long did the kings live by the river?","a":["About 250 years","About 25 years","About 2,500 years"],"ok":0,"why":"About 250 years, by the Tagus."}\'})}');
  await p.locator('[data-tqai="g3|en"]').first().click(); await until('!!S.tourQ.g3&&S.tourQ.g3.q.en', 3000);
  const dr = await R('var q=S.tourQ.g3;[q.q.en,q.a.length,q.src,q.rev.en,!!ed.querySelector(".tqlang .badge.gdwb"),/ONLY facts/.test(window.SFP),/Now the story/.test(window.SFP),gdQOk(q,"en")]');
  ok('Draft with AI (sampleFn stubbed, a real click): the draft from the step\'s own text, marked AI draft, not reviewed (it never plays)', dr[0] === 'How long did the kings live by the river?' && dr[1] === 3 && dr[2] === 'ai' && dr[3] === false && dr[4] && dr[5] && dr[6] && dr[7] === false, dr);
  await p.locator('[data-tqrev="g3|en"]').first().check(); await W(150);
  ok('Reviewed (a real tick): the English question can play now', await R('S.tourQ.g3.rev.en===true&&gdQOk(S.tourQ.g3,"en")&&!gdQOk(S.tourQ.g3,"pt")'));
  const mk = await R('sampleFn=window.SF0;[rowDemoOf("tourQ.g4.ok"),rowDemoOf("tourGuess.g4.unit"),BEHAVIOUR.test("@tourQ.g4.ok")].map(function(x){return!!x})');
  ok('The Guide script\'s question and guess rows have their demos (the quiz on that question; the guess before its step)', mk.every(Boolean), mk);
  await R(restoreQ + 'S.tab="guide";renderEditor()');
  ok('The new words and their owners (tour_* on the beats, Games on the tour on the group)', await R('TXOWN.tour_promise==="gd:i:quizpromise"&&TXOWN.tour_score_all==="gd:i:quizfinale"&&TXOWN.tour_tellme==="gd:i:guessyear"&&TXOWN.tour_goon==="gd:i:water"&&TXOWN.tour_cookie==="gd:i:cookie"&&TXOWN.tour_notnow==="gd:g:ontour"&&TXOWN.settings_games==="gd:g:ontour"&&["tour_promise","tour_wrong","tour_nomore"].every(function(k){return S.tx[k].pt&&S.tx[k].en})'));
  tsec('editor');

  // ---------- 8. the scenes in Ask Azu's console ----------
  const ids = await R('AGSCENES.filter(function(s){return/^gd12_/.test(s.id)}).map(function(s){return s.id})');
  ok('The console lists the eight new scenes', JSON.stringify(ids) === JSON.stringify(['gd12_quiz', 'gd12_wrong', 'gd12_stop', 'gd12_skip', 'gd12_few', 'gd12_expert', 'gd12_breather', 'gd12_guess']), ids);
  for (const id of ids) {
    await R(base + `window.R12=null;var s=AGSCENES.filter(function(x){return x.id==="${id}"})[0];s.setup();Promise.resolve(s.act()).then(function(r){window.R12=r},function(e){window.R12=[false,"ERROR "+e.message,""]});1`);
    await until('window.R12', 90000); const r = await R('window.R12');
    ok(`Scene ${id}: ${r ? r[1] : 'no answer'}`, !!(r && r[0]), r && r[2]);
  }
  tsec('scenes');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  tsec('end');
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
