// test9q (Playwright + the installed Edge): node test9q.js -> PASS/FAIL per check. Guidance round 13, Fun › On the tour's second half
// (research_tour_entertainment.md 4.4, 4.7, 5.4) and the developer's answers on round 12: the plan alone on gdTourPlan (the card's own quiz
// step skipped when the quiz is promised, the pair as one beat, Don't say it! In a step with its two seams, Find it on the wall on the wall
// only; the gates: a screen reader, Calm, Expert, a word said once or never, a span not in the tour); the word suggestions and their counts
// in EN and PT, the slips' matcher; on the page: the quiz step skipped and back with No quiz, the promise's chips up while step 1 is told
// (0 px over its words, Mini, Compact, Open); Don't say it! by real clicks on Azu (the windows from the word times, a catch and its pip, a
// second tap in a window ignored, Previous never counting a slip twice, a tap after a window's end a false alarm, pokes off), Pause freezing
// a window, the result with the slip played back, Heard it! at Compact and with Mascot Off, the screen reader, a language change; Find it on
// the wall (a real drag on the wall to the point, the 1.2 s settle, the hint's direction, I give up at 20 s, the wall lost, the wall only);
// every new row's demo (a real click on a sample), the Guide script's word with Reviewed. Its thirteen scenes in Ask Azu's console: test9qs.js.
// MUTATE=1 node test9q.js makes the tap window ignore its end: the checks named [window] turn red.
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
  // the mutation: every window of Don't say it! stays open (the tap window ignores its end)
  if (process.env.MUTATE) { await R('setInterval(function(){var d=G.gd.ds;if(d)d.wins.forEach(function(w){w.e=1e12})},20)'); console.log('MUTATION: the tap window ignores its end'); }
  // the device voice, muted (its words go on by time); only Fun › On the tour speaks; Ink glass at Mini
  const base = 'S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.cardCfg.mode="light";S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.look="ink";S.guideCfg.life="lively";S.guideCfg.mascot=true;S.vis.aud="explorer";S.vis.len="";S.vis.speed=null;S.m.solemn=false;S.ui.gdSR=false;S.guidance.on=true;gdKindById("app").on=false;gdKindById("content").on=false;gdGroupById("play").on=false;S.ui.rowDemo=false;S.camCfg.wall="ribeira";S.camX=.5;S.camY=.5;S.camZoom=1;';
  await R('window.WK=JSON.stringify(S.tourWord);window.FK=JSON.stringify(S.tourFind);window.QK=JSON.stringify(S.tourQ)');
  const only = ids => `["quizpromise","quizfinale","guessyear","dontsay","findwall","water","cookie"].forEach(function(id){S.guidance.items[id].on=${JSON.stringify(ids)}.indexOf(id)>=0});`;
  const fresh = async (ids, x) => { await R(base + 'S.tourWord=JSON.parse(window.WK);S.tourFind=JSON.parse(window.FK);S.tourQ=JSON.parse(window.QK);if(G.on)stopTour();gdBeatClose();gdTailEnd();gdWordOff();G.gd.owed=null;G.gd.plan=null;gdResetVisitor();var st=ph.querySelector(".stv");if(st)stateHide();' + only(ids) + 'S.view="card";S.stop="full";renderAll(false);' + (x || '')); };
  const seam = id => R(`gd12Seam(${JSON.stringify(id)})`);

  // ---------- 1. the plan, pure ----------
  const pl = await R(`var it=gdItems(),V=gdNewMemory(0),c={solemn:false,lively:true,sr:false,games:"on",notNow:0},
    mk=function(o){var st=[{id:"t0",act:"teaser",secs:10}];for(var j=1;j<=9;j++)st.push({id:"s"+j,act:"show",secs:12,q:j%2===1,quizBlock:j===9,title:j===9?"Test yourself":""});st.push({id:"e",act:"end",secs:5});
      return Object.assign({steps:st,len:"standard",aud:"explorer",lang:"en",every:75,perTour:3,perShort:2,perDeep:5,auds:null,word:{w:"palace",span:["s4","s5"],rev:true,n:2},find:{after:"s7",point:"torreao",onWall:true}},o||{})},
    on=function(ids){return it.map(function(x){return x.onTour?Object.assign({},x,{on:ids.indexOf(x.id)>=0}):x})},sum=function(P){return P.beats.map(function(b){return b.id+"@"+b.seam+(b.end!=null?"-"+b.end:"")+(b.one?"(one)":"")})},o={};
    var A=gdTourPlan(on(["quizpromise","quizfinale","guessyear"]),V,c,mk());o.skip=[A.skip.map(function(x){return x.id}),sum(A),A.ids.indexOf("s9"),gdPlanWords(A)];
    var B=gdTourPlan(on(["quizpromise","quizfinale"]),V,c,mk({steps:mk().steps.map(function(s){return Object.assign({},s,{q:false})})}));o.noskip=[B.skip.length,B.ids.indexOf("s9"),B.why.quizpromise];
    var C=gdTourPlan(on(["quizpromise","quizfinale","guessyear","findwall"]),V,c,mk({steps:mk().steps.map(function(s){return Object.assign({},s,{secs:s.secs*1.2,guess:s.id==="s4"})}),find:{after:"s6",point:"torreao",onWall:true}}));o.one=[C.budget,sum(C),C.why.findwall||"",C.why.dontsay||""];
    var D=gdTourPlan(on(["dontsay"]),V,c,mk());o.word=[sum(D),D.beats[0]&&D.beats[0].after,D.beats[0]&&D.beats[0].endAfter,gdPlanWords(D)];
    var Dg=gdTourPlan(on(["dontsay","guessyear"]),V,c,mk({steps:mk().steps.map(function(s){return Object.assign({},s,{guess:s.id==="s5"})})}));o.inside=[sum(Dg),Dg.why.dontsay];
    o.gates=[gdTourPlan(on(["dontsay"]),V,Object.assign({},c,{sr:true}),mk()).why.dontsay,gdTourPlan(on(["dontsay"]),V,Object.assign({},c,{lively:false}),mk()).why.dontsay,gdTourPlan(on(["dontsay"]),V,c,mk({aud:"expert"})).why.dontsay,
      gdTourPlan(on(["dontsay"]),V,c,mk({word:{w:"palace",span:["s4"],rev:true,n:1}})).why.dontsay,gdTourPlan(on(["dontsay"]),V,c,mk({word:{w:"palace",span:["s4","s5"],rev:false,n:2}})).why.dontsay,
      gdTourPlan(on(["dontsay"]),V,c,mk({word:null})).why.dontsay,gdTourPlan(on(["dontsay"]),V,c,mk({word:{w:"palace",span:["s4","zz"],rev:true,n:2}})).why.dontsay,gdTourPlan(on(["dontsay"]),V,c,mk({lang:"pt",word:null})).why.dontsay];
    var F=gdTourPlan(on(["findwall"]),V,Object.assign({},c,{lively:false}),mk());o.find=[sum(F),gdTourPlan(on(["findwall"]),V,c,mk({find:{after:"s7",point:"torreao",onWall:false}})).why.findwall,gdTourPlan(on(["findwall"]),V,c,mk({find:null})).why.findwall];o`);
  ok('13.0: with the quiz promised, the plan skips the card\'s own quiz step ("Test yourself"); the finale moves to the seam before it, and the plan says so', JSON.stringify(pl.skip[0]) === '["s9"]' && pl.skip[2] < 0 && pl.skip[1].indexOf('quizfinale@8(one)') >= 0 && /Test yourself.*is skipped: Azu's quiz replaces it/.test(pl.skip[3]), pl.skip);
  ok('13.0: with no quiz planned (too few questions), the step stays in the tour', pl.noskip[0] === 0 && pl.noskip[1] > 0 && /too few reviewed questions/.test(pl.noskip[2]), pl.noskip);
  ok('13.0: [cap] the promise and its quiz count as one beat: a tour with room for 2 gets the pair and one more beat; the plan\'s words say why the rest waits', pl.one[0] === 2 && pl.one[1].length === 3 && pl.one[1].some(x => /\(one\)/.test(x)) && /already has 2 beats, the quiz's promise and the quiz as one/.test(pl.one[2] + pl.one[3]), pl.one);
  ok('Don\'t say it! In a step: announced at the seam before its span, its result at the seam after it; the plan names the span', pl.word[0][0] === 'dontsay@3-5' && pl.word[1] === 's3' && pl.word[2] === 's5' && /Don't say it! in steps 5-6 \(announced at the seam after step 4/.test(pl.word[3]), pl.word);
  ok('Its span holds its seams: no beat inside it or beside it (Guess the year before step 6 is first in the Editor\'s order: Don\'t say it! is not planned over it)', JSON.stringify(pl.inside[0]) === '["guessyear@4"]' && /never two seams in a row/.test(pl.inside[1]), pl.inside);
  ok('Its gates: a screen reader (not offered), Calm (Lively only), Expert (Family and Explorer), said once (no game), not reviewed, no word, a span not in the tour, a language without its word', /screen reader.*not offered/.test(pl.gates[0]) && /Calm/.test(pl.gates[1]) && /not for Expert/.test(pl.gates[2]) && /said once/.test(pl.gates[3]) && /not reviewed/.test(pl.gates[4]) && /no word in English/.test(pl.gates[5]) && /not all in this tour/.test(pl.gates[6]) && /no word in Portuguese/.test(pl.gates[7]), pl.gates);
  ok('Find it on the wall: Calm too, at the seam after its step; the wall only (its point not on the wall in view: no find); no point: no find', JSON.stringify(pl.find[0]) === '["findwall@7"]' && /the wall only/.test(pl.find[1]) && /no wall point/.test(pl.find[2]), pl.find);
  const sg = await R('var e=gdWordSuggest("en"),p=gdWordSuggest("pt");[gdWordLine(e[0]),e[0].flags,gdWordLine(p[0]),p[0].flags,e.filter(function(c){return c.w==="palace"}).length,e.some(function(c){return c.w.length<4}),[gdWordHit("palace\'s","palace","en"),gdWordHit("Palaces,","palace","en"),gdWordHit("palácio.","palácio","pt"),gdWordHit("place","palace","en")],gdWordCount(S.tourWord.en,"en"),gdWordCount(S.tourWord.pt,"pt")]');
  ok('The word suggestions from the tour\'s own text, best first, with count and span: EN "palace: 2 times in steps 4-5", PT "palácio: 2 times in steps 4-5"; each rule it bends named (the demo tour\'s short span)', /^palace: 2 times in steps 4-5 \(0:\d\d\)$/.test(sg[0]) && /^palácio: 2 times in steps 4-5 \(0:\d\d\)$/.test(sg[2]) && sg[1].length === 1 && /short span/.test(sg[1][0]) && sg[3].length === 1 && sg[4] >= 2 && !sg[5], sg);
  ok('The slips\' matcher: the word, its possessive and plural; never a near word ("place"); the demo\'s words said 2 times in their spans', JSON.stringify(sg[6]) === '[true,true,true,false]' && sg[7] === 2 && sg[8] === 2, sg.slice(6));
  const wn = await R('var k=JSON.stringify(S.tourWord);S.tourWord.en.span=["g4"];var a=gdBeatNever(S.guidance.items.dontsay);S.tourWord.en={w:"the",span:["g3","g4"],rev:true};var b=gdBeatNever(S.guidance.items.dontsay);S.tourWord.pt.rev=false;var c=gdBeatNever(S.guidance.items.dontsay);S.tourWord=JSON.parse(k);[a,b,c,gdBeatNever(S.guidance.items.dontsay)]');
  ok('The Editor\'s warnings: a count of 1 (no game), over 4 (too easy, too long), not reviewed; none for the demo\'s words', wn[0].some(x => /said once/.test(x)) && wn[1].some(x => /too easy, too long/.test(x)) && wn[2].some(x => /not reviewed/.test(x)) && wn[3].length === 0, wn);
  tsec('pure');

  // ---------- 2. 13.0 on the page: the quiz step skipped, the promise's chips over step 1 ----------
  await fresh(['quizpromise', 'quizfinale']); await R('startTour()');
  const sk = await R('[S.guideSteps.map(function(s){return s.id}).indexOf("g9"),gdPlanBeat("quizfinale").after,/is skipped/.test(gdPlanWords(G.gd.plan)),!!scr.querySelector(\'[data-ref="b:b_quiz"]\')]');
  ok('13.0: the tour skips the card\'s "Test yourself" step (g9) while the quiz is promised; the finale after g8; the block stays on the card', sk[0] < 0 && sk[1] === 'g8' && sk[2] && sk[3], sk);
  await until('gd12Kind("promise")', 20000); const t1 = Date.now(); await until('!!ph.querySelector(".gdstrip .bt-chip")', 15000);
  const tl = await R('[G.i,S.guideSteps[G.i].id,gsSpeaking(),!G.gd.bt,G.gd.plan.quiz]');
  const ov = {}; for (const h of ['mini', 'compact', 'peek']) ov[h] = await R(`S.dockH="${h}";renderGuide();var e=ph.querySelector(".gdstrip"),r=e&&gdPR(e);[!!e,Math.round(gdTailOver(e)),r&&r.l>=0&&r.r<=ph.offsetWidth&&r.t>=0&&r.b<=ph.offsetHeight,[].map.call(ph.querySelectorAll(".gdstrip .bt-chip"),function(x){var q=gdPR(x);return Math.round(q.b-q.t)})]`);
  await R('S.dockH="mini";renderGuide()');
  ok('13.0: the promise\'s Deal / No quiz, thanks stay up while step 1 is told (the tour no longer waits): step 1 speaking, the quiz pending', tl[1] === 'g1' && tl[2] && tl[3] && tl[4] === 'pending', tl);
  ok('13.0: the chips 0 px over the words being told, inside the phone, 44 px, at Mini, Compact and Open', Object.values(ov).every(v => v[0] && v[1] === 0 && v[2] && v[3].length === 2 && v[3].every(h => h >= 44)), ov);
  await tap('.gdstrip [data-gdtl="noquiz"]'); await until('G.gd.plan.quiz==="off"', 3000);
  const nq = await R('[S.guideSteps.map(function(s){return s.id}).indexOf("g9")>0,gdPlanBeat("quizfinale").state,/plays again/.test(gdPlanWords(G.gd.plan))]');
  ok('13.0: No quiz, thanks (a real tap): the quiz dropped, the card\'s "Test yourself" step plays again', nq[0] && nq[1] === 'dropped' && nq[2], nq);
  tsec('13.0');

  // ---------- 3. Don't say it! by real clicks ----------
  const mascot = async () => { const r = await box('.mascot'); if (r) await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return !!r; };
  await fresh(['dontsay']); await R('startTour();window.POKES=0;on("mascot.poked",function(){window.POKES++})'); await W(300); await seam('g2');
  await until('gd12Kind("word")', 8000);
  const an = await R('[ph.querySelector(".gdbeat .bt-line").textContent,ph.querySelector(".gdbeat .bt-word b").textContent,G.i]');
  ok('The announce at the seam before the span: its line with the word, the word on its card', /try not to say “palace”\. If I say it, tap me/.test(an[0]) && an[1] === 'palace' && an[2] === 2, an);
  await until('G.gd.ds&&G.gd.ds.on&&G.i===3', 20000);
  const sp = await R('[!!ph.querySelector(".gdstrip .tl-word"),[].map.call(ph.querySelectorAll(".gdstrip .bt-chip"),function(x){var q=gdPR(x);return x.textContent+"|"+Math.round(q.b-q.t)}),Math.round(gdTailOver(ph.querySelector(".gdstrip")))]');
  ok('The span plays: the word card pinned, Let\'s play / Not now (44 px) for 4 s, 0 px over the words told', sp[0] && sp[1].length === 2 && /^Let's play\|\d+/.test(sp[1][0]) && sp[1].every(x => +x.split('|')[1] >= 44) && sp[2] === 0, sp);
  await until('!G.gd.tail', 6000);
  // a tap before the first slip: a false alarm (Nope!), never a poke
  await mascot(); await until('G.gd.ds&&G.gd.ds.falses===1', 2000);
  const fa = await R('[G.gd.ds.falses,!!ph.querySelector(".azbub")&&ph.querySelector(".azbub").textContent,window.POKES,gsSpeaking()]');
  ok('A tap on Azu before the word (a real click): a false alarm, “Nope!” for 1 s, no voice of its own; pokes are off (no poke), the story goes on', fa[0] === 1 && fa[1] === 'Nope!' && fa[2] === 0 && fa[3], fa);
  await until('G.gd.ds&&G.gd.ds.order.length>=1', 15000);
  const w1 = await R('var d=G.gd.ds,w=d.wins[0],k=d.slips[d.order[0]].k;[w.e-w.s,wordMs(k),G.words[k].w,Math.round(gdWordClock()-w.s)]');
  ok('[window] The slip\'s window from the word time: the word\'s start to its end + 2.5 s', Math.abs(w1[0] - (w1[1] + 2500)) < 5 && /palace/i.test(w1[2]) && w1[3] < 1500, w1);
  await mascot(); await until('G.gd.ds.caught===1', 1500); await W(150); await mascot(); await W(900);
  const c1 = await R('[G.gd.ds.caught,G.gd.ds.falses,(ph.querySelector(".gdstrip .tl-pip")||{}).textContent,gsSpeaking(),window.POKES]');
  ok('A catch (a real click in the window): the pip “Caught 1”, the voice never stopped; a second tap in the same window is ignored (not counted, not a false alarm)', c1[0] === 1 && c1[1] === 1 && c1[2] === 'Caught 1' && c1[3] && c1[4] === 0, c1);
  // Previous: the sentence again; the slip relit, never counted twice
  await R('S.dockH="peek";renderGuide()'); await until('!!ph.querySelector(\'.gdock [data-g="prev"]\')', 2000); await tap('.gdock [data-g="prev"]'); await R('S.dockH="mini";renderGuide()');
  await until('G.gd.ds&&G.gd.ds.wins.length>=2', 8000); await mascot(); await W(900);
  const pv = await R('[G.gd.ds.order.length,G.gd.ds.wins.length,G.gd.ds.caught,G.gd.ds.falses,G.gd.ds.slips[G.gd.ds.order[0]].said]');
  ok('Previous (a real tap): the sentence again lights the same slip; a tap in its new window never counts it twice', pv[0] === 1 && pv[1] >= 2 && pv[2] === 1 && pv[3] === 1 && pv[4] >= 2, pv);
  // the next slip: let its window pass, then tap: a false alarm
  await until('G.gd.ds&&G.gd.ds.order.length>=2', 25000);
  await until('var d=G.gd.ds,w=d.wins[d.wins.length-1];gdWordClock()>w.e+150', 6000); await mascot(); await W(1000);
  const we = await R('[G.gd.ds?G.gd.ds.caught:G.gd.plan.wordScore.caught,G.gd.ds?G.gd.ds.falses:G.gd.plan.wordScore.falses]');
  ok('[window] A tap after the window\'s end (a real click): a false alarm, the slip stays missed', we[0] === 1 && we[1] === 2, we);
  await until('G.gd.bt&&G.gd.bt.kind==="wordres"', 25000); const res = await R('[ph.querySelector(".gdbeat .bt-line").textContent,G.gd.plan.wordScore]');
  await until('!!ph.querySelector(".gdbeat .bt-slip .slipw")', 15000);
  const back = await R('[G.gd.clip&&(G.gd.clip.phrase||G.gd.clip.src),(ph.querySelector(".gdbeat .bt-slip .slipw")||{}).textContent,(ph.querySelector(".gdbeat .bt-slip")||{}).textContent]');
  ok('The result at the seam after the span: “I said it 2 times. You caught 1!”', /^I said it 2 times\. You caught 1!/.test(res[0]) && res[1].caught === 1 && res[1].n === 2, res);
  ok('…the missed slip played back (the device voice: its phrase said again), the word lit in the card', !!back[0] && /palace/.test(back[1]) && /palace/.test(back[2]), back);
  await until('!G.gd.bt', 15000); ok('…then the tour goes on after the span', await R('S.guideSteps[G.i].id==="g5"&&!G.gd.ds'));
  // the developer's answer on round 13: the result waits until the last window has closed (a slip said just before the seam)
  const wt = await R('var b=gdPlanBeat("dontsay",G.gd.plan),d=gdWordStart(b),t=gdWordClock();d.slips.x={key:"x",step:"g4",k:0,caught:false,text:"",phrase:{words:["palace"],at:0,rec:null},said:1};d.order.push("x");d.wins.push({key:"x",s:t,e:t+1500});window.__wt=performance.now();gdWordEnd();[d.on,!!d.waitEnd,!!(G.gd.bt&&G.gd.bt.kind==="wordres")]');
  await W(800); const wm = await R('[!!(G.gd.ds&&G.gd.ds.on),!!(G.gd.bt&&G.gd.bt.kind==="wordres")]');
  await until('G.gd.bt&&G.gd.bt.kind==="wordres"', 5000); const wa = await R('Math.round(performance.now()-window.__wt)');
  ok('[window] The result waits until the last window has closed (a slip 1.5 s before the seam): no result at 0.8 s, the result after the window', wt[0] && wt[1] && !wt[2] && wm[0] && !wm[1] && wa >= 1400 && wa < 2600, [wt, wm, wa]);
  await R('gdBeatClose();gdWordOff()'); await until('!G.gd.bt', 5000);
  const lines = await R('[fmt(TXT("tour_word_all"),3),fmt(TXT("tour_word_none"),3),fmt(TXT("tour_word_false"),4),TXT("tour_word_dream"),fmt(TXT("tour_word_callback"),2,3).replace("{thing}","palace")]');
  ok('The result lines by score, never mocking; the score said again in the quiz\'s callback', lines[0] === '3 times, and you caught me every time. Sharp ears!' && /didn't tap once/.test(lines[1]) && /Nice try/.test(lines[2]) && /pastéis de nata/.test(lines[3]) && lines[4] === 'And you caught me saying “palace” 2 of 3 times.', lines);
  const cb = await R('G.gd.plan.wordScore={word:"palace",caught:2,n:2,falses:0};G.gd.plan.quiz="on";var it=S.guidance.items.quizfinale;gdStart(it,"t",{owed:{qs:["g1","g4"],n:2}});var B=G.gd.bt;B.right=2;B.k=1;B.answered=0;gdQuizScore();var l=G.gd.bt.said||"";gdBeatClose();gdStop("t");l');
  ok('The quiz\'s score says the game once more (“…caught me saying “palace” 2 of 2 times.”)', /caught me saying “palace” 2 of 2 times/.test(cb), cb);
  tsec('word');

  // Pause freezes a window
  await fresh(['dontsay']); await R('startTour()'); await W(300); await seam('g2');
  await until('G.gd.ds&&G.gd.ds.order.length>=1', 35000); await tap('.gdock [data-g="pause"]'); await until('G.paused', 1500);
  const c0 = await R('gdWordClock()'); await W(3200); const c2 = await R('gdWordClock()');
  await tap('.gdock [data-g="play"],.gdock [data-g="pause"]'); await until('!G.paused', 2000); await mascot(); await until('G.gd.ds&&G.gd.ds.caught===1', 1500);
  ok('[window] Pause (a real tap) freezes the windows with the voice: 3 s paused, then a tap after Play still catches the slip', Math.abs(c2 - c0) < 150 && await R('G.gd.ds&&G.gd.ds.caught===1'), [c0, c2]);
  // Heard it! at Compact and with Mascot Off
  await R('S.dockH="compact";renderGuide()'); await until('!!ph.querySelector(".gdheard")', 2000);
  const hd = await R('var h=ph.querySelector(".gdheard"),g=ph.querySelector(".gbtn-wrap.intour"),r=gdPR(h),q=g&&gdStarBox(g);[Math.round(r.b-r.t),Math.round(r.r-r.l),!!q&&r.l-q.r>=0&&r.l-q.r<=24&&Math.abs((r.t+r.b)/2-(q.t+q.b)/2)<12,h.textContent]');
  ok('At Compact a Heard it! button by the ✦ (44 px)', hd[0] >= 44 && hd[1] >= 44 && hd[2] && hd[3] === 'Heard it!', hd);
  await R('S.dockH="mini";S.guideCfg.mascot=false;renderGuide()'); const mo = await until('!!ph.querySelector(".gdstrip .tl-heard")', 2000);
  await until('!!G.gd.ds&&G.gd.ds.wins.some(function(w){var t=gdWordClock();return t>=w.s+150&&t<=w.e&&!G.gd.ds.slips[w.key].caught})', 30000);
  await tap('.gdstrip .tl-heard'); await W(200);
  ok('With Mascot Off, Heard it! too (at Mini); a real tap on it in a window catches the slip', mo && await R('(G.gd.ds?G.gd.ds.caught:G.gd.plan.wordScore.caught)===2'), await R('G.gd.ds?[G.gd.ds.caught,G.gd.ds.order.length]:G.gd.plan.wordScore'));
  // a screen reader: not offered; a language change ends it quietly
  await fresh(['dontsay'], 'S.ui.gdSR=true');
  ok('A screen reader on: Don\'t say it! is not offered (the plan says why)', await R('/screen reader.*not offered/.test(gdPlanNow().why.dontsay)'));
  await fresh(['dontsay']); await R('startTour()'); await W(300); await seam('g2'); await until('G.gd.ds&&G.gd.ds.on&&G.i===3', 20000);
  await R('S.lang="pt";renderAll(true)'); const lc = await until('!G.gd.ds', 2000);
  ok('The language changed mid-span: the game ends quietly (“Let\'s play another time.” on the strip, never said)', lc && await R('/Jogamos noutra altura|Let\'s play another time/.test((ph.querySelector(".gdstrip .tl-msg")||{}).textContent||"")'));
  // Portuguese with its own word
  await fresh(['dontsay'], 'S.lang="pt";renderAll(false)'); await R('startTour()'); await W(300); await seam('g2'); await until('gd12Kind("word")', 8000);
  ok('Portuguese with its own word: the announce says “palácio”, its card shows it', await R('/não dizer “palácio”/.test(ph.querySelector(".gdbeat .bt-line").textContent)&&ph.querySelector(".gdbeat .bt-word b").textContent==="palácio"'));
  tsec('word edges');

  // ---------- 4. Find it on the wall ----------
  await fresh(['findwall']); await R('startTour()'); await W(300);
  const fb = await R('gdPlanBeat("findwall")&&gdPlanBeat("findwall").after'); await seam(fb);
  await until('!!G.gd.find&&!!ph.querySelector(".gdseek")', 8000);
  const f0 = await R('var F=G.gd.find,s=poiScreen(F.p),c=gdFindC();[S.stop,S.dockH,F.p.id,Math.round(s.x),Math.round(s.y),Math.round(c.x),Math.round(c.y),gdFindR(),gdFindDir(F.p),ph.querySelector(".gdbeat .bt-line").textContent]');
  ok('Find it on the wall at the seam after its step: the card lowered (the wall), the panel at Mini, a seek ring in the middle, its line', f0[0] === 'closed' && f0[1] === 'mini' && f0[2] === 'torreao' && /find the great corner tower on the real wall/.test(f0[9]), f0);
  // a real drag on the wall: the point into the ring
  const ph0 = await p.locator('#ph').boundingBox(), q = ph0.width / (await R('ph.offsetWidth'));
  const sx = ph0.x + (f0[5] - (f0[5] - f0[3]) / 2) * q, sy = ph0.y + 300 * q; await p.mouse.move(sx, sy); await p.mouse.down();
  await p.mouse.move(sx + (f0[5] - f0[3]) * q, sy + (f0[6] - f0[4]) * q, { steps: 12 }); await p.mouse.up(); const tUp = Date.now();
  const inR = await R('gdFindIn(G.gd.find.p)'); await until('G.gd.find&&G.gd.find.state==="found"', 4000); const settle = (Date.now() - tUp) / 1000;
  const fd = await R('[ph.querySelector(".gdbeat .bt-line").textContent,!!ph.querySelector(".gdfound"),!!ph.querySelector(".gdseek.got")]');
  ok('A real drag on the wall brings the tower into the ring; held still, it is found after the 1.2 s settle, “That\'s it!” and a glint', inR && settle >= 1.1 && settle < 2.5 && fd[1] && fd[2], [inR, settle, fd]);
  await until('!G.gd.bt&&S.stop==="full"', 8000); ok('…then back to the card, the tour goes on', await R('S.stop==="full"&&S.guideSteps[G.i].id!==' + JSON.stringify(fb)));
  // the hint's direction; I give up at 20 s
  await fresh(['findwall']); await R('startTour()'); await W(300); await seam(fb); await until('!!G.gd.find', 8000);
  await R('G.gd.find.clock=9800'); await until('G.gd.find.hinted', 2000); await until('ph.querySelector(".gdbeat .bt-line").textContent===TXT("tour_find_"+gdFindDir(G.gd.find.p))', 3000);
  const hn = await R('[gdFindDir(G.gd.find.p),ph.querySelector(".gdbeat .bt-line").textContent]');
  ok('Hint after 10 s: from the point\'s direction on the screen (the tower is left of the middle: “A little to the left…”)', hn[0] === 'left' && hn[1] === 'A little to the left…', hn);
  await R('G.gd.find.clock=19800'); await until('G.gd.find&&G.gd.find.state==="shown"', 2000);
  await until('!!ph.querySelector(".mascot.gd-findto")', 1500); await W(1300);
  const gu = await R('var F=G.gd.find,m=ph.querySelector(".mascot"),s=gdFindSpot(F.p),r=phRect(m),g=ph.querySelector(".gdfound");[Math.round(Math.hypot(r.l+r.w/2-30-s.x,r.t+r.h+14-s.y)),!!g,s.edge,g&&g.className]');
  await until('ph.querySelector(".gdbeat .bt-line").textContent==="Just past the left edge: the great corner tower."', 5000);
  ok('Give up after 20 s: Azu walks to the tower and shows it (off the screen: at the edge on its side, an arrow to it), “Just past the left edge: the great corner tower.”', gu[0] <= 70 && gu[1] && gu[2] === 'left' && /edge e-left/.test(gu[3]) && await R('ph.querySelector(".gdbeat .bt-line").textContent==="Just past the left edge: the great corner tower."'), gu);
  await until('!G.gd.bt', 12000);
  // tracking lost; the wall only
  await fresh(['findwall']); await R('startTour()'); await W(300); await seam(fb); await until('!!G.gd.find', 8000);
  await R('stateShow("lost")'); const lost = await until('G.gd.find&&G.gd.find.state==="lost"&&/another time/.test((ph.querySelector(".gdbeat .bt-line")||{}).textContent||"")', 2000); await until('!G.gd.find', 6000);
  ok('The wall lost while seeking (a system state wins): the find ends quietly, “We\'ll find it another time.”', lost && await R('!G.gd.find&&!ph.querySelector(".gdseek")'));
  await R('stateHide()'); await until('!G.gd.bt', 8000);
  const wo = await R('var c=gdContext();c.state="wall not found";var P=G.gd.plan,it=gdItems().filter(function(x){return x.id==="findwall"})[0],b=gdPlanBeat("findwall"),k=b.state;b.state="planned";var r=gdBeatCheck(it,Object.assign(c,{moment:"between",speaking:false,tour:{step:3,stepId:b.after}}));b.state=k;S.camCfg.wall="belem";renderPhone(false);var w=gdPlanNow().why.findwall;S.camCfg.wall="ribeira";renderPhone(false);[r.no,w]');
  ok('Never while tracking is lost (at its seam); the wall only: on another wall, no find', /tracking lost/.test(wo[0]) && /the wall only/.test(wo[1]), wo);
  tsec('find');

  // ---------- 5. the Editor: rows, demos, the Guide script ----------
  await fresh(['quizpromise', 'quizfinale', 'guessyear', 'dontsay', 'findwall', 'water', 'cookie']);
  await R('S.tab="guide";S.op={gt4:true,"gd:fun":true,"gd:g:ontour":true,"gd:i:dontsay":true,"gd:i:findwall":true};Object.keys(S.op).forEach(function(k){var m=/^gd:i:(.+)$/.exec(k),it=m&&S.op[k]&&S.guidance.items[m[1]];if(it)gdFormOf(it).groups.forEach(function(g){if(g)S.op["gd:f:"+m[1]+":"+g.k]=true})});renderEditor()'); // round 14: the items' groups open
  const ek = await R('var o=[];ed.querySelectorAll(\'[data-fo="gd:i:dontsay"] .row,[data-fo="gd:i:findwall"] .row\').forEach(function(r){var k=rowKey(r);if(k&&o.indexOf(k)<0)o.push(k)});var labs=[].map.call(ed.querySelectorAll(\'[data-fo="gd:i:dontsay"] .row > label,[data-fo="gd:i:findwall"] .row > label\'),function(l){return l.textContent});[o,o.filter(function(k){return!rowDemoOf(k)}),labs,ed.querySelector("#gdplan").textContent]');
  ok('The new rows: The word (suggested) · The span · Tap window (2.5) · False taps · Play my slip back (On); The point · Says · Give up after (20) · Hint after (10); each with its demo', ['The word · EN', 'The word · PT', 'The span · EN', 'Tap window (s)', 'False taps', 'Play my slip back', 'The point', 'After step', 'Give up after (s)', 'Hint after (s)'].every(x => ek[2].indexOf(x) >= 0) && ek[1].length === 0 && await R('var d=S.guidance.items.dontsay,f=S.guidance.items.findwall;d.tapWin===2.5&&d.falseTaps==="nope"&&d.slipBack===true&&f.giveUp===20&&f.hintAfter===10'), [ek[1], ek[2].length]);
  ok('The plan line names the pair as one and the skipped step', /the quiz's promise and the quiz count as one/.test(ek[3]) && /is skipped: Azu's quiz replaces it/.test(ek[3]), ek[3]);
  const sample = ['guidance.items.dontsay.tapWin', 'guidance.items.dontsay.falseTaps', 'guidance.items.dontsay.slipBack', 'tourWord.en.pick', 'guidance.items.findwall.giveUp', 'guidance.items.findwall.hintAfter', 'tourFind.point'];
  const shown = [];
  for (const k of sample) {
    await R('if(G.on)stopTour();gdBeatClose();gdTailEnd();document.body.dataset.rowdemo="";S.ui.rowDemo=true;renderEditor()');
    const lb = p.locator(`.row:has([data-set="@${k}"],[data-sel="@${k}"],[data-num="@${k}"]) > label.rd`).first();
    if (await lb.count()) await lb.scrollIntoViewIfNeeded(); const bb = await lb.boundingBox(); if (!bb) { shown.push(k + ' (no ▸)'); continue; }
    await p.mouse.click(bb.x + bb.width - 12, bb.y + bb.height / 2);
    const want = /findwall|tourFind/.test(k) ? '!!ph.querySelector(".gdseek")&&G.on' : k === 'guidance.items.dontsay.slipBack' ? 'G.gd.bt&&G.gd.bt.kind==="wordres"' : /tapWin|falseTaps/.test(k) ? '!!ph.querySelector(".gdstrip .tl-word")&&G.on' : '!!ph.querySelector(".gdbeat")&&G.on';
    const got = await until(want, 9000); shown.push(k + (got ? '|shown' : '|none'));
  }
  ok('Each new row\'s ▸ pressed (a real click): its game on the phone, in a tour, now (the announce, the span with its word card, the result, the seek ring)', shown.every(s => /\|shown$/.test(s)), shown);
  await R('if(G.on)stopTour();gdBeatClose();gdTailEnd();S.ui.rowDemo=false;S.tab="marker";S.op={m3:true};renderEditor()');
  const gs = await R('var b=ed.querySelector(".gdwbox");[!!b,b&&b.textContent.indexOf("palace: 2 times in steps 4-5")>=0,b&&b.textContent.indexOf("palácio: 2 times in steps 4-5")>=0,b&&b.querySelectorAll(\'[data-gdwrev]\').length,b&&/the great corner tower/.test(b.textContent)]');
  ok('Specific Marker › Guide script: the word per language with its suggestions (count and span), a Reviewed tick each, the find\'s line', gs[0] && gs[1] && gs[2] && gs[3] === 2 && gs[4], gs);
  await p.locator('[data-gdwrev="pt"]').first().uncheck(); await W(200);
  ok('Reviewed unticked (a real click): no game in Portuguese (the plan says why), English still plays', await R('S.tourWord.pt.rev===false&&(function(){var k=S.lang;S.lang="pt";var w=gdPlanNow().why.dontsay;S.lang=k;return/not reviewed/.test(w)})()&&S.tourWord.en.rev===true'));
  await p.locator('select[data-sel="@tourWord.en.pick"]').first().selectOption({ index: 1 }); await W(300);
  ok('Picking another suggestion (a real select): the word and its span change, and it needs Reviewed again', await R('var w=S.tourWord.en;w.rev===false&&w.src==="suggested"&&(w.w!=="palace"||w.span.join(",")!=="g3,g4")'));
  await R('S.tourWord=JSON.parse(window.WK);S.tab="guide";renderEditor()');
  ok('The new words and their owners (tour_word* on Don\'t say it!, tour_find* on Find it on the wall)', await R('TXOWN.tour_word==="gd:i:dontsay"&&TXOWN.tour_word_heard==="gd:i:dontsay"&&TXOWN.tour_letsplay==="gd:i:dontsay"&&TXOWN.tour_find==="gd:i:findwall"&&TXOWN.tour_find_giveup==="gd:i:findwall"&&/não dizer/.test(S.tx.tour_word.pt)&&/Baixe o cartão/.test(S.tx.tour_find.pt)'));
  tsec('editor');

  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  tsec('end');
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
