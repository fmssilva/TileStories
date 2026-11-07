// g2 checks (Playwright + the installed Edge): node test9f.js -> PASS/FAIL per check
// Guidance, round 2 (_3.8 section 8): the full scheduler. 1. Each new gate, limit and learning rule alone, on a fake clock (a pure check
// of gdPick); 2. the stage of the visit and the score (the kinds' weights at s = 0 / 0.3 / 1, Along the visit, freshness, fit, ties,
// deterministic); 3. the learning on the page (three ignored, an acted tip, a dismissal, Fewer like this); 4. the memory (Remember
// across visits Off, Leave for 31 min, Reset the visitor, each item's memory); 5. the two new items on the page (their look: 2 lines,
// 7:1, 0 px over words, light, dark and Portuguese; their target, Azu, the ring; the tour invitation spoken once the guide has been
// heard, with the word lit; their taps); 6. the bubble's tap, swipe and long press, by the mouse as a finger; 7. the 15-minute script
// (App first, then Content; at most 2 a minute; none over a sentence; each pick logged with why); 8. Why not now? in the scheduler's
// own words; 9. every new Editor row's demo (the ×10 chip), the Log and the visitor; 10. the five new scenes in Ask Azu's console.
// MUTATE=1 node test9f.js runs it with a score that ignores the stage of the visit (the kinds' weights and Along the visit held at ×1):
// the checks named [the stage] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('var __gs=gdScore;gdScore=function(it,items,v,ctx,now,st){var f=__gs(it,items,v,ctx,now,st);f.kind=1;f.along=1;f.score=f.imp*f.group*f.fresh*f.fit;return f}'); console.log('MUTATION: the score ignores the stage of the visit (the kinds\' weights and Along the visit held at ×1)'); }
  // the set-up: the Ribeira wall, a first visit, the clock moved by hand (deterministic), the remarks and the ✦ hint resting
  const base = 'S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.guidance.items.gbhint.on=false;gdGroupById("inview").on=false;S.guideCfg.gbtn="both";S.guideCfg.mascot=true;' +
    'S.guidance.on=true;S.guidance.breathing=15;S.guidance.perMinute=2;S.guidance.perVisit=30;S.guidance.freshTau=120;S.guidance.learnIgnore=true;S.guidance.learnDismiss=true;S.guidance.fewer=true;S.guidance.remember=true;G.gd.demo=false;gdChip(null);';
  const fresh = async (extra) => { await R(base + 'if(G.on)stopTour();G.cam.open=false;S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);G.gd.manual=true;gdStop("test");gdResetVisitor();G.gd.movedAt=null;G.gd.touchAt=null;G.gd.onWall=false;' + (extra || '') + 'gdAdvance(250)'); await W(150); };
  const cur = () => R('G.gd.cur?[G.gd.cur.item.id,Math.round(G.gd.cur.at),G.gd.cur.text,G.gd.cur.pull]:null');
  const runTill = async (id, ms, step) => R(`var t0=gdNow(),r=null;while(gdNow()-t0<${ms}){gdAdvance(${step || 250});if(G.gd.cur&&G.gd.cur.item.id==="${id}"&&!G.gd.cur.pull){r=Math.round(G.gd.cur.at);break}}r`);
  const CR = 'var px=function(c){var m=c.match(/[\\d.]+/g).map(Number);return{r:m[0],g:m[1],b:m[2],a:m.length>3?m[3]:1}},lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},cr=function(a,b){var x=lum(a),y=lum(b);return Math.round((Math.max(x,y)+.05)/(Math.min(x,y)+.05)*100)/100};';

  // ---------- 1. the scheduler alone: each new gate, limit and learning rule by itself, on a fake clock ----------
  // the moment: a first visit 5 min in (s 0.33), on the wall, the opening said at 0-9 s; Ribeira Palace the nearest marker and the point in the middle; the ✦ on the screen
  const pk = (mod, now, id) => R(`var items=gdItems(),V={visits:1,visit:{n:1,at:0,lastEnd:9000,items:{opening:{shown:1,last:0,end:9000,dismissed:0,acted:0,how:"ignored"}},starts:[0],away:0,ignoredRun:0,breathMul:1,rest:{},fewer:{},heard:false},ever:{items:{opening:{shown:1,done:false,by:"",last:0}}},used:{},met:false},
    C={on:true,breathing:15,limits:{perMinute:2,perVisit:30},tau:120,learn:{ignore:true,dismiss:true},demo:false,showing:null,remark:null,speaking:false,state:null,panel:null,dockFull:false,tour:null,movedAt:null,touchAt:null,cardAt:null,onWall:true,where:"",
      marker:{id:"painting",name:"Ribeira Palace",x:200,y:400,middle:true},gbtn:{name:"the ✦",x:60,y:770,middle:false},central:{id:"painting",name:"Ribeira Palace",x:200,y:400,middle:true,tour:true,min:3},covers:{},stillSince:0,returning:false},
    it=function(id){return items.filter(function(x){return x.id===id})[0]},vm=function(id,o){V.visit.items[id]=Object.assign({shown:0,last:null,end:null,dismissed:0,acted:0,how:""},o)};${mod || ''};
    var r=gdPick(items,V,C,${now});[r.item?r.item.id:null,r.why${id ? `,gdWhyNot(items,V,C,${now},"${id}")` : ''}]`);
  const N = 300000, P = {};
  P.base = await pk('', N);
  P.touch = await pk('C.touchAt=' + (N - 500), N); P.touchOk = await pk('C.touchAt=' + (N - 900), N);
  P.card = await pk('C.cardAt=' + (N - 1000), N); P.cardOk = await pk('C.cardAt=' + (N - 1600), N);
  P.dock = await pk('C.dockFull=true', N); P.state = await pk('C.state="offline"', N);
  P.tourNever = await pk('C.tour={between:true};C.onWall=false;C.where="the tour"', N, 'tapmarker');
  P.tourBetween = await pk('C.tour={between:true};C.onWall=false;C.where="the tour";it("askview").duringTour="between"', N, 'askview');
  P.tourLine = await pk('C.tour={between:false};C.onWall=false;C.where="the tour";it("askview").duringTour="between"', N, 'askview');
  P.covers = await pk('C.covers={tapmarker:"no room beside Ribeira Palace without covering it"}', N, 'tapmarker');
  ok('pick: a touch in the last 0.8 s (0.5 s ago: quiet; 0.9 s: the gate is open)', P.touch[0] === null && /quiet while the visitor acts: a touch 0.5 s ago/.test(P.touch[1]) && P.touchOk[0] !== null, [P.touch, P.touchOk[0]]);
  ok('pick: the card\'s first 1.5 s (1.0 s: quiet; 1.6 s: open)', P.card[0] === null && /card's first 1.5 s/.test(P.card[1]) && P.cardOk[0] !== null, [P.card, P.cardOk[0]]);
  ok('pick: the dock at Full; a system state showing (Loading & states)', P.dock[0] === null && /dock is at Full/.test(P.dock[1]) && P.state[0] === null && /system state shows \(offline\)/.test(P.state[1]), [P.dock, P.state]);
  ok('pick: quiet in a tour: a Never item between steps; a Between steps item at a line waits; between steps it gets past the tour', /quiet in a tour \(During a tour: Never\)/.test(P.tourNever[2]) && /waits for a pause between two steps/.test(P.tourLine[2]) && !/quiet in a tour/.test(P.tourBetween[2]) && /not on the wall \(the tour\)/.test(P.tourBetween[2]), [P.tourNever[2], P.tourLine[2], P.tourBetween[2]]);
  ok('pick: never over its own target (the bubble would cover the marker: Tap a marker yields)', P.covers[0] !== 'tapmarker' && /Tap a marker: never over its own target: no room beside Ribeira Palace/.test(P.covers[2]), P.covers);
  P.min2 = await pk('V.visit.starts=[' + (N - 50000) + ',' + (N - 20000) + '];V.visit.lastEnd=' + (N - 16000), N);
  P.min1 = await pk('V.visit.starts=[' + (N - 70000) + ',' + (N - 20000) + '];V.visit.lastEnd=' + (N - 16000), N);
  P.visit = await pk('C.limits.perVisit=3;vm("tapmarker",{shown:2,last:100000,end:104000})', N);
  P.cap = await pk('it("tapmarker").kindCap=2;it("askview").kindCap=2;vm("tapmarker",{shown:1,last:100000,end:104000})', N, 'askview');
  P.gap = await pk('it("tourof").kindGap=60;vm("tourof",{shown:0,end:' + (N - 30000) + '})', N, 'tourof');
  ok('limit: at most 2 a minute (two starts in the last 60 s: quiet, 10 s left; one: a tip)', P.min2[0] === null && /at most 2 a minute: 10 s left/.test(P.min2[1]) && P.min1[0] !== null, [P.min2, P.min1[0]]);
  ok('limit: at most a visit (3 shown of 3)', P.visit[0] === null && /at most 3 a visit: shown 3 this visit/.test(P.visit[1]), P.visit);
  ok('limit: a kind\'s cap (App: at most 2 a visit) and its gap (Content: 60 s, 30 s left)', /at most 2 App items a visit: shown 2/.test(P.cap[2]) && /Content's gap 60 s: 30 s left/.test(P.gap[2]), [P.cap[2], P.gap[2]]);
  P.mul = await pk('V.visit.breathMul=2;V.visit.ignoredRun=3;V.visit.lastEnd=' + (N - 20000), N, 'tapmarker');
  P.mulOk = await pk('V.visit.breathMul=2;V.visit.ignoredRun=3;V.visit.lastEnd=' + (N - 31000), N);
  P.dis = await pk('vm("tapmarker",{shown:1,last:' + (N - 34000) + ',end:' + (N - 30000) + ',how:"dismissed",dismissed:1});V.visit.lastEnd=' + (N - 30000), N, 'tapmarker');
  P.disOff = await pk('C.learn.dismiss=false;vm("tapmarker",{shown:1,last:' + (N - 34000) + ',end:' + (N - 30000) + ',how:"dismissed",dismissed:1});V.visit.lastEnd=' + (N - 30000), N, 'tapmarker');
  P.rest = await pk('V.visit.rest.tours=' + (N + 120000), N, 'tourof');
  P.fewer = await pk('V.visit.rest.start=1e15;V.visit.fewer.start=true', N, 'tapmarker');
  ok('learning: three ignored in a row: the breathing room 30 s (×2); after 31 s a tip again', /breathing room 30 s \(×2: 3 ignored in a row\): 10 s left/.test(P.mul[2]) && P.mulOk[0] !== null, [P.mul[2], P.mulOk[0]]);
  ok('learning: a dismissed App tip waits ×3 its Again after (75 s, 45 s left); After a dismissal Off: 25 s', /again after 75 s \(×3: it was dismissed\): 45 s left/.test(P.dis[2]) && !/again after/.test(P.disOff[2]), [P.dis[2], P.disOff[2]]);
  ok('learning: a dismissed Content item rests its group (120 s left); Fewer like this rests the group for the visit', /Tours rests 5 min after a dismissal: 120 s left/.test(P.rest[2]) && /Fewer like this: The start rests for the visit/.test(P.fewer[2]), [P.rest[2], P.fewer[2]]);
  P.det = [await pk('', 301234), await pk('', 301234)];
  ok('pick: deterministic (the same inputs, the same answer and why)', JSON.stringify(P.det[0]) === JSON.stringify(P.det[1]) && !!P.det[0][0], P.det[0]);

  // ---------- 2. the stage of the visit and the score ----------
  const st = await R('var V=function(o){return Object.assign({visits:1,visit:{at:0,away:0},used:{}},o)};[gdStage(V(),0).s,gdStage(V(),270000).s,gdStage(V(),900000).s,gdStage(V(),1800000).s,gdStage(V({used:{card:1,view:2}}),60000).s,gdStage(V({visits:2}),0).s,gdStage(V({visit:{at:0,away:120000}}),390000).s]');
  ok('the stage: s = active minutes / 15 (0, 4.5 min: 0.3, 15 min: 1, capped), or the share of the milestones (2 of 4: 0.5), a returning visitor from 5 min (0.33), time away not counted', st[0] === 0 && Math.abs(st[1] - 0.3) < 1e-9 && st[2] === 1 && st[3] === 1 && st[4] === 0.5 && Math.abs(st[5] - 1 / 3) < 1e-9 && Math.abs(st[6] - 0.3) < 1e-9, st);
  const sc = await R('var items=gdItems(),it=function(id){return Object.assign({},items.filter(function(x){return x.id===id})[0],{importance:1,along:"kind"})},V={visits:1,visit:{at:0,items:{}},ever:{items:{}},used:{}},C={tau:120},f=function(i,s){return gdScore(i,items,V,C,0,{s:s})};' +
    'var a=it("askview"),c=it("tourof");[[0,.3,1].map(function(s){return Math.round(f(a,s).score*1000)/1000}),[0,.3,1].map(function(s){return Math.round(f(c,s).score*1000)/1000}),Math.round(f(a,.65).score*1000)/1000]');
  ok('[the stage] the kinds\' weights at s = 0 / 0.3 / 1: App 70 / 40 / 15, Content 25 / 50 / 60 (straight lines between: App at 0.65 is 27.5)', JSON.stringify(sc[0]) === '[0.7,0.4,0.15]' && JSON.stringify(sc[1]) === '[0.25,0.5,0.6]' && sc[2] === 0.275, sc);
  const al = await R('var items=gdItems(),V={visits:1,visit:{at:0,items:{}},ever:{items:{}},used:{}},C={tau:120},g=function(along,s){var i=Object.assign({},items.filter(function(x){return x.id==="tourof"})[0],{importance:1,along:along});var f=gdScore(i,items,V,C,0,{s:s});return Math.round(f.along*f.kind*1000)/1000};' +
    '[["late","early","steady","kind"].map(function(a){return[0,.3,1].map(function(s){return g(a,s)})})]');
  ok('[the stage] Along the visit: Late 0 / 0 / ×1, Early ×1 / ×1 / 0, Steady the kind\'s middle (Content 50 %) all visit, As the kind the kind\'s weight', JSON.stringify(al[0]) === JSON.stringify([[0, 0, 0.6], [0.25, 0.5, 0], [0.5, 0.5, 0.5], [0.25, 0.5, 0.6]]), al);
  const fr = await R('var items=gdItems(),i=items.filter(function(x){return x.id==="askview"})[0],C={tau:120},f=function(last,now,tau){var V={visits:1,visit:{at:0,items:{askview:{shown:1,last:last}}},ever:{items:{}},used:{}};return Math.round(gdScore(i,items,V,{tau:tau},now,{s:0}).fresh*1000)/1000},Vn={visits:1,visit:{at:0,items:{}},ever:{items:{}},used:{}};' +
    '[f(0,0,120),f(0,120000,120),f(0,240000,120),f(0,60000,60),Math.round(gdScore(i,items,Vn,C,0,{s:0}).fresh*1000)/1000]');
  ok('freshness 1 − e^(−t/τ): just shown 0, after τ (2 min) 0.632, after 2τ 0.865, τ = 60 s after 60 s 0.632; never shown 1', JSON.stringify(fr) === '[0,0.632,0.865,0.632,1]', fr);
  const fit = await R('var items=gdItems(),V={visits:1,visit:{at:0,items:{}},ever:{items:{}},used:{}},t=Object.assign({},items.filter(function(x){return x.id==="tourof"})[0],{teaches:"card"}),f=function(c,v){return gdScore(t,items,v||V,{tau:120,central:c},0,{s:1}).fit};' +
    'var Vd={visits:1,visit:{at:0,items:{}},ever:{items:{tapmarker:{shown:1,done:true,by:"card"}}},used:{}};[f({middle:true}),f({middle:false}),Math.round(f({middle:true},Vd)*100)/100,Math.round(f({middle:false},Vd)*100)/100]');
  ok('fit: ×1.5 with the target in the middle, ×1.3 when it teaches an open App tip (Tap a marker teaches a card): ×1.95 / ×1.3; once that tip is done ×1.5 / ×1', Math.abs(fit[0] - 1.95) < 1e-9 && Math.abs(fit[1] - 1.3) < 1e-9 && fit[2] === 1.5 && fit[3] === 1, fit);
  const tie = await R('var items=gdItems(),a=Object.assign({},items.filter(function(x){return x.id==="askview"})[0]),b2=Object.assign({},a,{id:"askview2",label:"Ask (copy)"}),V={visits:1,visit:{at:0,lastEnd:null,items:{},starts:[]},ever:{items:{}},used:{}},C={on:true,breathing:15,limits:{},tau:120,onWall:true,gbtn:{x:1,y:1},stillSince:0,covers:{}};' +
    '[gdPick([a,b2],V,C,120000).item.id,gdPick([b2,a],V,C,120000).item.id,gdPick([a,b2],V,C,120000).score===gdPick([b2,a],V,C,120000).score]');
  ok('ties: two items with the same score: the Editor\'s order decides (either way round)', tie[0] === 'askview' && tie[1] === 'askview2' && tie[2], tie);
  const rank = await R('var items=gdItems(),a=Object.assign({},items.filter(function(x){return x.id==="askview"})[0],{importance:3}),c=Object.assign({},items.filter(function(x){return x.id==="tourof"})[0],{importance:3,along:"kind"}),V=function(min){return{visits:1,visit:{at:0,lastEnd:null,items:{},starts:[]},ever:{items:{}},used:{}}},' +
    'C={on:true,breathing:15,limits:{},tau:120,onWall:true,gbtn:{x:1,y:1,middle:false},central:{id:"painting",name:"Ribeira Palace",x:1,y:1,middle:false,tour:true,min:3},stillSince:-Infinity,covers:{}};[gdPick([a,c],V(),C,60000).item.id,gdPick([a,c],V(),C,900000).item.id]');
  ok('[the stage] the mix: both due, App wins on arrival (70 against 25), Content at the end (60 against 15)', rank[0] === 'askview' && rank[1] === 'tourof', rank);

  // ---------- 3. the learning on the page (the service's gdLearn, by how each tip ends) ----------
  await fresh();
  const ln = await R('var it=function(id){return gdItems().filter(function(x){return x.id===id})[0]},out=[],end=function(id,how){gdStart(it(id),"test");gdStop("test",how)};' +
    'end("tapmarker","ignored");end("askview","ignored");out.push(S.gdVisitor.visit.breathMul);end("tapmarker","ignored");out.push(S.gdVisitor.visit.breathMul);end("askview","ignored");end("tapmarker","ignored");end("askview","ignored");out.push(S.gdVisitor.visit.breathMul);end("tapmarker","ignored");end("askview","ignored");end("tapmarker","ignored");out.push(S.gdVisitor.visit.breathMul);' +
    'end("opening","acted");out.push(S.gdVisitor.visit.breathMul,S.gdVisitor.visit.ignoredRun);end("tapmarker","dismissed");out.push(gdVM("tapmarker").how,gdVM("tapmarker").dismissed);end("tourof","dismissed");out.push(Math.round((S.gdVisitor.visit.rest.tours-gdNow())/1000));end("askview","fewer");out.push(S.gdVisitor.visit.rest.wall>1e14,!!S.gdVisitor.visit.fewer.wall,gdEM("askview").dismissed);out');
  ok('the learning: 3 ignored in a row ×2, 6 ×4 (no more at 9), an acted tip resets it; a dismissal is remembered; a dismissed Content item rests Tours 5 min; Fewer like this rests The wall for the visit', JSON.stringify(ln) === JSON.stringify([1, 2, 4, 4, 1, 0, 'dismissed', 1, 300, true, true, 1]), ln);
  const ess = await R('gdResetVisitor();var o=gdItems().filter(function(x){return x.id==="opening"})[0];gdStart(o,"t");gdStop("t","ignored");gdStart(o,"t");gdStop("t","ignored");gdStart(o,"t");gdStop("t","ignored");[S.gdVisitor.visit.ignoredRun,S.gdVisitor.visit.breathMul]');
  ok('...the opening\'s time running out is not ignoring (Essential: a greeting)', ess[0] === 0 && ess[1] === 1, ess);
  const lOff = await R('gdResetVisitor();S.guidance.learnIgnore=false;var t=gdItems().filter(function(x){return x.id==="tapmarker"})[0];for(var k=0;k<3;k++){gdStart(t,"t");gdStop("t","ignored")}var m=S.gdVisitor.visit.breathMul;S.guidance.learnIgnore=true;S.guidance.learnDismiss=false;var c=gdItems().filter(function(x){return x.id==="tourof"})[0];gdStart(c,"t");gdStop("t","dismissed");S.guidance.learnDismiss=true;[m,S.gdVisitor.visit.rest.tours==null]');
  ok('...Ignored three in a row Off: no doubling; After a dismissal Off: no rest', lOff[0] === 1 && lOff[1], lOff);

  // ---------- 4. the memory: Remember across visits, Leave for 31 min, Reset the visitor; each item's memory ----------
  await fresh(); await R('gdAdvance(1000)');
  const m1 = await R('var o=G.gd.cur&&G.gd.cur.text;gdAdvance(10000);var e=gdEM("opening"),v=gdVM("opening");[o,e.shown,e.line,e.last!=null,v.line,v.how]');
  ok('each item\'s memory: shown this visit and ever, last shown, the line used, how it ended', m1[0] === await R('TXT("gd_opening")') && m1[1] === 1 && m1[2] === m1[0] && m1[3] && m1[4] === m1[0] && m1[5] === 'ignored', m1);
  await R('S.tab="guide";S.ui.level=1;S.op={gt0:true};renderEditor()');
  await p.locator('[data-gdvisit="leave"]').click(); await R('gdAdvance(1500)'); await W(200);
  const lv = await R('[S.gdVisitor.visits,G.gd.cur&&G.gd.cur.text,Math.round(gdStage(S.gdVisitor,gdNow()).s*1000)/1000,gdEM("opening").shown,G.gd.log.filter(function(l){return l.what==="away"}).slice(-1)[0].why,ed.querySelector("#gdvisitor2").textContent]');
  ok('Leave for 31 min (the Editor\'s button): the visit ends; back, a new visit: Welcome back, the stage from 5 min (s 0.333), the ever memory kept', lv[0] === 2 && lv[1] === await R('TXT("gd_opening_back")') && lv[2] >= 0.333 && lv[3] === 2 && /away 31 min: the visit ended/.test(lv[4]) && /A returning visitor · visits 2/.test(lv[5]), lv);
  const sm = await R('gdLeave(5*60000);gdAdvance(1000);[S.gdVisitor.visits,S.gdVisitor.visit.away,Math.round(gdStage(S.gdVisitor,gdNow()).active*10)/10]');
  ok('...away 5 min: the same visit (the time away is not active time)', sm[0] === 2 && sm[1] === 300000 && sm[2] < 1, sm);
  await R('S.guidance.remember=false;renderEditor()'); await p.locator('[data-gdvisit="leave"]').click(); await R('gdAdvance(1500)');
  const ro = await R('[S.gdVisitor.visits,G.gd.cur&&G.gd.cur.text,gdEM("opening").shown,gdStage(S.gdVisitor,gdNow()).s<0.01]');
  await R('gdNewVisit();gdAdvance(1500)'); const ro2 = await R('[S.gdVisitor.visits,G.gd.cur&&G.gd.cur.text]');
  await R('S.guidance.remember=true');
  ok('Remember across visits Off: every visit starts as a first visit (after Leave for 31 min and after New visit)', ro[0] === 1 && ro[1] === await R('TXT("gd_opening")') && ro[2] === 1 && ro[3] && ro2[0] === 1 && ro2[1] === ro[1], [ro, ro2]);
  await R('gdNewVisit();gdUsed("card","test");gdAdvance(1000);renderEditor()'); await p.locator('[data-gdvisit="reset"]').click(); await W(150);
  const rs = await R('[S.gdVisitor.visits,Object.keys(S.gdVisitor.used).length,gdEverOf(S.gdVisitor,"tapmarker").done,ed.querySelector("#gdvisitor").textContent]');
  ok('Reset the visitor: a first visit, nothing remembered (the features used and done cleared)', rs[0] === 1 && rs[1] === 0 && !rs[2] && /^Visit 1 \(a first visit\)/.test(rs[3]), rs);
  const feat = await R('gdResetVisitor();emit("card.opened","test");emit("view.opened","test");emit("ask.after","An answer.");emit("tourStep.ended",{id:"intro",act:"intro"});[Object.keys(S.gdVisitor.used).sort().join(),S.gdVisitor.met,S.gdVisitor.visit.heard,gdStage(S.gdVisitor,gdNow()).s,gdEverOf(S.gdVisitor,"askview").done,gdEverOf(S.gdVisitor,"tourof").done]');
  ok('the features used (card opened, a tour step heard, a question asked, Ask about the view opened): the stage s 1; met the guide (the introduction heard); Ask about the view and the tour invitation done', feat[0] === 'ask,card,tour,view' && feat[1] && feat[2] && feat[3] === 1 && feat[4] && feat[5], feat);

  // ---------- 5. the two new items on the page ----------
  const look = (sel) => CR + `var b=ph.querySelector(".gdbub:not(.gd-out)"),bt=b.querySelector("button"),cs=getComputedStyle(bt),lh=parseFloat(cs.lineHeight),lines=Math.round((bt.offsetHeight-parseFloat(cs.paddingTop)-parseFloat(cs.paddingBottom))/lh),B=gdPR(b),g=ph.querySelector(".gbtn"),G1=g?gdPR(g):null,nb=document.getElementById("navbar"),N=nb&&gdSeen(nb)?gdPR(nb):null;` +
    '[lines,cr(px(cs.color),px(getComputedStyle(b).backgroundColor)),gdOverWords(),G1?gdOver(B,G1):0,N?gdOver(B,N):0,Math.round(bt.offsetHeight),b.dataset.tail,bt.textContent]';
  // Ask about the view: 60 s into the visit, Tap a marker resting
  const toAsk = 'gdResetVisitor();G.gd.movedAt=null;var vm=gdVM("tapmarker");vm.shown=3;vm.last=gdNow();vm.end=gdNow();gdAdvance(1000);var t0=gdNow();while(gdNow()-t0<90000){gdAdvance(250);if(G.gd.cur&&G.gd.cur.item.id==="askview")break}';
  for (const dark of [false, true]) {
    await fresh(`S.dark=${dark};renderAll(true);`); await R(toAsk); await W(400);
    const l = await R(look());
    const tg = await R('var g=ph.querySelector(".gbtn"),G1=gdPR(g),rg=ph.querySelector(".gdring.gd-rect"),RG=rg&&gdPR(rg),az=ph.querySelector(".azfab");[G.gd.cur&&G.gd.cur.item.id,!!RG&&RG.l<G1.l&&RG.r>G1.r&&RG.t<G1.t&&RG.b>G1.b,az.classList.contains("point-gb"),gdOver(gdPR(ph.querySelector(".gdbub")),G1),Math.round(gdNow()-S.gdVisitor.visit.at)]');
    ok(`Ask about the view (${dark ? 'dark' : 'light'}): at 60 s or later, beside the ✦ (the way into Ask about the view): the ring round it, Azu pointing at it; 2 lines at most, 7:1, 0 px over words, not over the ✦ or the bar, 44 px`, tg[0] === 'askview' && tg[1] && tg[2] && tg[3] === 0 && tg[4] >= 60000 && l[0] <= 2 && l[1] >= 7 && l[2] === 0 && l[3] === 0 && l[4] === 0 && l[5] >= 44 && l[7] === 'Ask me about anything you see.', [tg, l]);
  }
  await R('S.dark=false;S.lang="pt";renderAll(true)'); await W(200); const lpt = await R(look());
  ok('...in Portuguese: its words from Languages & Terms, 2 lines at most', lpt[7] === await R('S.tx.gd_askview.pt') && lpt[0] <= 2 && lpt[2] === 0, lpt);
  await R('S.lang="en";renderAll(true)');
  await R('var b=ph.querySelector(".gdbub button[data-gdbub]");gsTouch(b)'); await W(300);
  const tapA = await R('[G.cam.open,gdEverOf(S.gdVisitor,"askview").done,gdEverOf(S.gdVisitor,"askview").acted,S.gdVisitor.used.view!=null,!G.gd.cur]');
  ok('...a tap on it opens Ask about the view: acted, and it is done (Done when Ask about the view opened)', tapA.every(Boolean), tapA);
  await R('G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};renderPhone(false)');
  // A tour of {point}: late in the visit (two milestones: s 0.5), the App tips done; first a bubble only, then spoken once the guide has been heard
  const toTour = 'gdResetVisitor();G.gd.movedAt=null;S.gdVisitor.used.card=0;S.gdVisitor.used.view=0;["tapmarker","askview"].forEach(function(id){var e=gdEM(id);e.done=true;e.by="card"});gdAdvance(1000);var t0=gdNow();while(gdNow()-t0<60000){gdAdvance(250);if(G.gd.cur&&G.gd.cur.item.id==="tourof")break}';
  await fresh(); await R(toTour); await W(500);
  const tv = await R(look());
  const tp = await R('var c=G.gd.cur,az=ph.querySelector(".azfab"),A=gdPR(az),M=gdCentral();[c&&c.item.id,c&&c.spoken,!!(G.timer||G.utter),Math.round(Math.hypot((A.l+A.r)/2-M.x,(A.t+A.b)/2-M.y)),!!ph.querySelector(".gdpoint"),!ph.querySelector(".gdring")]');
  ok('A tour of {point}: about the point in the middle, its slots filled ("Ribeira Palace has a story. A tour of 3 minutes?"), Azu beside it and pointing, no ring; a bubble only (the guide not heard yet); 2 lines, 7:1, 0 px over words', tp[0] === 'tourof' && tp[1] === false && !tp[2] && tp[3] < 120 && tp[4] && tp[5] && tv[7] === 'Ribeira Palace has a story. A tour of 3 minutes?' && tv[0] <= 2 && tv[1] >= 7 && tv[2] === 0, [tp, tv]);
  await fresh(); await R('S.gdVisitor.visit.heard=true;' + toTour.replace('gdResetVisitor();', '')); await W(250);
  const sp = await R('var c=G.gd.cur,lit=0;[c&&c.item.id,c&&c.spoken,!!(G.timer||G.utter),ph.querySelectorAll(".gdbub .gdw").length,G.gd.log.filter(function(l){return l.what==="shown"}).slice(-1)[0].why]');
  const litSeen = await until('!!ph.querySelector(".gdbub .gdw.on")', 3000);
  const litCr = await R(CR + 'var w=ph.querySelector(".gdbub .gdw.on")||ph.querySelector(".gdbub .gdw");cr(px(getComputedStyle(w).color),px(getComputedStyle(w).backgroundColor))');
  ok('...once the visitor has heard the guide this visit: spoken (the one order of voices) and in the bubble, the word being said lit, 7:1 on its light', sp[0] === 'tourof' && sp[1] === true && sp[2] && sp[3] === 10 && /spoken/.test(sp[4]) && litSeen && litCr >= 7, [sp, litSeen, litCr]);
  await R('var b=ph.querySelector(".gdbub button[data-gdbub]");gsTouch(b)'); await W(400);
  const tapT = await R('[G.on,S.stop,gdEverOf(S.gdVisitor,"tourof").acted,!G.gd.cur,!G.timer||G.on]');
  const stepEnd = await until('gdEverOf(S.gdVisitor,"tourof").done', 30000); // ends as soon as the step is heard; the cap only matters on a busy machine
  ok('...a tap on it starts the tour of that point (startTour): acted; once a step is heard, it is done', tapT[0] === true && tapT[2] === 1 && tapT[3] && stepEnd, [tapT, stepEnd]);
  await R('stopTour();S.view="card";S.stop="closed";renderPhone(false)');

  // ---------- 6. the bubble's controls, by the mouse as a finger: a tap, a swipe away, a long press ----------
  const tipBox = async () => { await W(800); return p.locator('#ph .gdbub:not(.gd-out) button[data-gdbub]').boundingBox(); };
  await fresh('S.guidance.breathing=5;S.guidance.items.askview.on=false;S.guidance.items.tourof.on=false;'); await R('gdAdvance(10000)'); await runTill('tapmarker', 30000);
  let bb = await tipBox(); await p.mouse.click(bb.x + bb.width / 2, bb.y + bb.height / 2); await W(200);
  const t1 = await R('[!G.gd.cur,gdVM("tapmarker").how,gdVM("tapmarker").acted,G.gd.log.filter(function(l){return l.what==="ended"}).slice(-1)[0].why]');
  ok('A tap on the bubble: it does its action (Tap a marker: it closes) and counts as acted', t1[0] && t1[1] === 'acted' && t1[2] === 1 && /tapped \(acted\)/.test(t1[3]), t1);
  await R('var e=gdVM("tapmarker");e.end=gdNow()-30000;S.gdVisitor.visit.lastEnd=gdNow()-30000;S.gdVisitor.visit.starts=[]'); await runTill('tapmarker', 30000);
  bb = await tipBox(); await p.mouse.move(bb.x + bb.width - 20, bb.y + bb.height / 2); await p.mouse.down();
  for (let i = 1; i <= 8; i++) await p.mouse.move(bb.x + bb.width - 20 - i * 12, bb.y + bb.height / 2, { steps: 2 });
  await p.mouse.up(); await W(250);
  await R('gdAdvance(6000)'); const sw = await R('[!G.gd.cur||G.gd.cur.item.id!=="tapmarker",gdVM("tapmarker").how,gdVM("tapmarker").dismissed,G.gd.log.filter(function(l){return l.what==="ended"}).slice(-1)[0].why,gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{touchAt:null,movedAt:null,showing:null}),gdNow(),"tapmarker")]');
  ok('A swipe away (a real drag on the bubble): dismissed; Tap a marker then waits ×3 its Again after (75 s)', sw[0] && sw[1] === 'dismissed' && sw[2] === 1 && /swiped away \(dismissed\)/.test(sw[3]) && /again after 75 s \(×3: it was dismissed\)/.test(sw[4]), sw);
  await R('var e=gdVM("tapmarker");e.how="";e.end=gdNow()-30000;S.gdVisitor.visit.lastEnd=gdNow()-30000;S.gdVisitor.visit.starts=[]'); await runTill('tapmarker', 30000);
  bb = await tipBox(); await p.mouse.move(bb.x + bb.width / 2, bb.y + bb.height / 2); await p.mouse.down(); await W(800); await p.mouse.up(); await W(200);
  const lp = await R(CR + 'var q=ph.querySelector(".gdfewer"),bs=[].map.call(ph.querySelectorAll(".gdbub [data-gdfewer]"),function(x){return[x.textContent,Math.round(x.getBoundingClientRect().height*ph.offsetHeight/ph.getBoundingClientRect().height)]});[!!q&&q.textContent.indexOf("Fewer like this?")===0,bs,!!G.gd.cur,gdOverWords(),q?cr(px(getComputedStyle(q.querySelector(".gdfq")).color),px(getComputedStyle(ph.querySelector(".gdbub")).backgroundColor)):0]');
  ok('A long press (0.8 s) on the bubble: Fewer like this? with Fewer and Keep (44 px each), the tip kept; 0 px over words, 7:1', lp[0] && lp[1].length === 2 && lp[1][0][0] === 'Fewer' && lp[1][1][0] === 'Keep' && lp[1].every(x => x[1] >= 44) && lp[2] && lp[3] === 0 && lp[4] >= 7, lp);
  await p.locator('#ph [data-gdfewer="no"]').click(); await W(150);
  const kp = await R('[!!G.gd.cur,(ph.querySelector(".gdbub button[data-gdbub]")||{}).textContent]');
  await R('var b=ph.querySelector(".gdbub button[data-gdbub]");gdAskFewer()'); await W(100); await p.locator('#ph [data-gdfewer="yes"]').click(); await W(150);
  const fw = await R('[!G.gd.cur,gdVM("tapmarker").how,S.gdVisitor.visit.fewer.start,G.gd.log.filter(function(l){return l.what==="learned"}).slice(-1)[0].why]');
  const fw2 = await runTill('tapmarker', 300000, 1000);
  ok('...Keep: the tip again; Fewer: it ends and its group (The start) rests for the visit: no Tap a marker in the next 5 min', kp[0] && kp[1] === 'Tap here.' && fw[0] && fw[1] === 'fewer' && fw[2] === true && /Fewer like this: The start rests for the visit/.test(fw[3]) && fw2 === null, [kp, fw, fw2]);
  await R('S.guidance.fewer=false'); await R('gdNewVisit();gdAdvance(30000)'); await runTill('tapmarker', 60000);
  bb = await tipBox(); await p.mouse.move(bb.x + bb.width / 2, bb.y + bb.height / 2); await p.mouse.down(); await W(800); await p.mouse.up(); await W(200);
  const nf = await R('[!!ph.querySelector(".gdfewer"),!!G.gd.cur]'); await R('S.guidance.fewer=true');
  ok('...Fewer like this? Off: a long press asks nothing (and does not tap)', !nf[0] && nf[1], nf);

  await R('S.guidance.items.askview.on=true;S.guidance.items.tourof.on=true');
  // ---------- 7. the 15-minute script: a first visit at ×30, App first, then Content ----------
  await fresh();
  const script = await R(`var v0=S.gdVisitor.visit.at,stages={},over=0;for(var t=1000;t<=900000;t+=1000){gdAdvance(1000);var c=G.gd.cur;if(c&&!c.pull&&stages[c.at]==null){stages[c.at]=gdStage(S.gdVisitor,c.at).s;if(G.utter||G.timer||G.recAudio)over++}
      if(t===130000){gsTouch(ph.querySelector("[data-marker]"))}if(t===160000){S.stop="closed";renderPhone(false)}if(t===200000){if(G.gd.cur&&G.gd.cur.item.id==="askview")gdTap();else gbGo("view")}if(t===220000){G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};renderPhone(false)}}
    var sh=gdScShows(v0);[sh.map(function(s){return[s.t-v0,s.id,s.kind,Math.round((stages[s.t]||0)*100)/100,/score \\d/.test(s.why)]}),over]`);
  const sh = script[0], app = sh.filter(s => s[2] === 'app'), con = sh.filter(s => s[2] === 'content'), firstC = con[0];
  const perMin = sh.reduce((m, s) => Math.max(m, sh.filter(x => x[0] > s[0] - 60000 && x[0] <= s[0]).length), 0);
  ok('[the stage] the 15-minute script: App first (the opening, Tap a marker, Ask about the view), then the tour invitation once the stage has grown (s 0.3 or more)', sh.length >= 5 && sh[0][1] === 'opening' && !!firstC && app.filter(s => s[0] < firstC[0]).length >= 3 && con.every(s => s[3] >= 0.3) && ['tapmarker', 'askview'].every(id => app.some(s => s[1] === id && s[0] < firstC[0])), sh);
  ok('...at most 2 a minute; none over a sentence; each pick in the Log with its score', perMin <= 2 && script[1] === 0 && sh.every(s => s[4]), { perMin, over: script[1] });
  ok('...about the study\'s pace: 5 to 12 tips in 15 minutes', sh.length >= 5 && sh.length <= 12, sh.length);

  // ---------- 8. Why not now? in the scheduler's own words ----------
  await fresh(); await R('gdAdvance(12000)');
  await R('S.tab="guide";S.ui.level=1;S.op={gt0:true};renderEditor()');
  const wn = [];
  for (const id of ['opening', 'tapmarker', 'askview', 'tourof']) {
    await R(`var s=ed.querySelector("[data-gdwhysel]");s.value="${id}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(60);
    await p.locator('[data-gdwhynot]').click(); await W(60);
    wn.push(await R(`var shown=ed.querySelector("#gdwhynot").textContent,r=gdPick(gdItems(),S.gdVisitor,gdContext(),gdNow());[shown,r.why,r.why.split(" · ").indexOf(shown)>=0]`));
  }
  ok('Why not now? (the Editor): each item\'s reason, word for word one of the scheduler\'s', wn.every(w => w[2]) && /Opening message: shown once this visit/.test(wn[0][0]) && /Tap a marker: 20 s after the Opening message: \d+ s left/.test(wn[1][0]) && /Ask about the view: not before 60 s of the visit: \d+ s left/.test(wn[2][0]) && /A tour of \{point\}: (breathing room 15 s|along the visit Late: nothing before a third)/.test(wn[3][0]), wn.map(w => w[0]));
  const wg = await R('S.settingsOpen=true;var a=gdWhyNow("tapmarker"),r=gdPick(gdItems(),S.gdVisitor,gdContext(),gdNow());S.settingsOpen=false;[a,r.why]');
  ok('...a gate: its words for every item (Settings open)', wg[0] === 'Tap a marker: ' + wg[1] && /quiet while Settings is open/.test(wg[1]), wg);
  const wf = await R('G.gd.manual=true;var r=null;for(var k=0;k<200&&!(r&&r.item);k++){G.gd.t+=250;r=gdPick(gdItems(),S.gdVisitor,gdContext(),gdNow())}var n=gdWhyNow(r.item.id),o=gdItems().filter(function(x){return x.id!==r.item.id}).map(function(x){return gdWhyNow(x.id)});[r.item.id,r.why,n,o]');
  ok('...the one that shows next: "it shows next" and its why, the scheduler\'s own', wf[2] === wf[1].replace(/^([^:]+): /, '$1: it shows next: '), wf);

  // ---------- 9. every new Editor row's demo; the Log; the rows' defaults ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;gdResetVisitor();S.tab="guide";S.ui.level=9;S.op={};renderEditor()');
  const defs = await R('var g=S.guidance,i=g.items;[g.perMinute,g.perVisit,g.freshTau,g.learnIgnore,g.learnDismiss,g.fewer,g.remember,g.kinds.map(function(k){return[k.id,k.on,k.cap,k.gap,k.groups.filter(function(x){return x.id!=="card"&&x.id!=="howto"}).map(function(x){return x.id+":"+x.weight}).join()]}),' +
    '[i.askview.shows,i.askview.max,i.askview.again,i.askview.notBefore,i.askview.doneWhen,i.askview.about],[i.tourof.kind,i.tourof.group,i.tourof.shows,i.tourof.along,i.tourof.doneWhen,i.tourof.tap],Object.keys(i).filter(function(k){return!i[k].form||(i[k].form!=="cue"&&i[k].form!=="walk")}).map(function(k){return i[k].duringTour}).join()]');
  ok('the new rows\' defaults: at most 2 a minute and 30 a visit, freshness over 120 s, the learning On, Fewer like this? offered, Remember On; App On cap 20 gap 0, Content On cap 12 gap 60, every group ×1; Ask about the view up to 2, again 90 s, not before 60 s; A tour of {point} Content › Tours, once a visit, Late; During a tour Never',
    JSON.stringify(defs) === JSON.stringify([2, 30, 120, true, true, true, true, [['app', true, 20, 0, 'start:1,wall:1'], ['content', true, 12, 60, 'inview:1,tours:1']], ['upto', 2, 90, 60, 'view', 'gbtn'], ['content', 'tours', 'visit', 'late', 'tour', 'tour'], 'never,never,never,never,never,never']), defs);
  const newRows = await R('var o=[];ed.querySelectorAll(\'[data-fo="gt4"] .row\').forEach(function(r){var k=rowKey(r);if(k&&o.indexOf(k)<0&&/^guidance\\.(perMinute|perVisit|freshTau|learnIgnore|learnDismiss|fewer|remember|kinds\\.|items\\.[a-z]+\\.(along|duringTour)|items\\.(askview|tourof)\\.)/.test(k))o.push(k)});o');
  ok(`every new row has its demo (${newRows.length} rows: Overall's 7, the kinds' and groups' 10, each item's During a tour and Along the visit, the two new items' own)`, newRows.length >= 30 && (await R(`${JSON.stringify(newRows)}.filter(function(k){return!rowDemoOf(k)})`)).length === 0, newRows.length);
  const seen = {}, missing = [];
  const CHIP_RE = { 'guidance.perMinute': /×10 · At most 2 a minute, the next tip: \d+ s/, 'guidance.learnIgnore': /Breathing room ×2 after 3 ignored: \d+ s/, 'guidance.learnDismiss': /×30 · Dismissed: Again after ×3: \d+ s/,
    'guidance.kinds.1.gap': /×10 · Content's gap: \d+ s/, 'guidance.freshTau': /after τ = 120 s: ×0\.63/, 'guidance.fewer': /Fewer like this\?/, 'guidance.remember': /On: Welcome back/ };
  for (const k of newRows) {
    await R('document.body.dataset.rowdemo="";window.__chips=[];clearInterval(window.__chI);window.__chI=setInterval(function(){var c=ph.querySelector(".gdchip");if(c&&window.__chips.indexOf(c.textContent)<0)window.__chips.push(c.textContent)},50)');
    const lb = await R(`var c=ed.querySelector('[data-fo="gt4"] .row [data-set="@${k}"],[data-fo="gt4"] .row [data-num="@${k}"]'),l=c&&c.closest(".row").querySelector("label.rd");if(l){l.scrollIntoView({block:"center"});var r=l.getBoundingClientRect();[r.left+12,r.top+r.height/2]}else null`);
    if (!lb) await R(`showRowDemo("${k}")`); else await p.mouse.click(lb[0], lb[1]);
    let v = ''; const t0 = Date.now(); while (Date.now() - t0 < 5000) { v = await R('document.body.dataset.rowdemo||""'); if (v) break; await W(80); }
    if (!/\|found\|/.test(v)) missing.push(k + ' ' + v);
    // a limit or a learning row: until its countdown chip shows (at most 9 s / 4 s, as before; no fixed wait)
    const chipRe = CHIP_RE[k];
    if (chipRe) { const t1 = Date.now(), cap = /perMinute|learnIgnore|learnDismiss|kinds\.1\.gap/.test(k) ? 9000 : 4000; while (Date.now() - t1 < cap) { if ((await R('window.__chips.slice()')).some(c => chipRe.test(c))) break; await W(100); } }
    seen[k] = await R('window.__chips.slice()');
  }
  await R('clearInterval(window.__chI)');
  ok('...a press on each new row shows its demo on the phone: the tip (or its chip) ringed', missing.length === 0, { missing, shown: newRows.length - missing.length });
  const has = (k, re) => (seen[k] || []).some(c => re.test(c));
  ok('...the limits and the learning on the ×10 clock with the countdown chip: at most a minute, three ignored (×2), a dismissal (×3), a kind\'s gap; freshness; Fewer like this?; Remember',
    Object.keys(CHIP_RE).every(k => has(k, CHIP_RE[k])), seen);
  const along = await R('[gdAlongWords(S.guidance.items.tourof),gdReadBack(S.guidance.items.askview)]');
  ok('...the read-back in words: Along the visit and During a tour, not before 60 s, until the visitor opens Ask about the view', /late: nothing for the first third, then growing to full/.test(along[0]) && /not before 60 s of the visit · at most about every 90 s, up to 2 times a visit, until the visitor opens Ask about the view · along the visit as the kind \(App: 70 % → 40 % → 15 %\) · during a tour never\./.test(along[1]), along);
  const kw = await R('[].map.call(ed.querySelectorAll(".note.gdmix"),function(n){return n.textContent})');
  ok('...each kind\'s weight read back in words (App 70 / 40 / 15 %, Content 25 / 50 / 60 %), and no row says "a later round"', kw.length === 2 && /70 % on arrival, 40 % a third of the way in, 15 % at the end/.test(kw[0]) && /25 % on arrival, 50 % a third of the way in, 60 % at the end/.test(kw[1]) && !(await R('/later round/i.test(ed.querySelector(\'[data-fo="gt4"]\').textContent+ed.querySelector(\'[data-fo="gt0"]\').textContent)')), kw);
  await R(base + 'G.gd.manual=true;gdStop("t");setWall("ribeira");gdResetVisitor();gdAdvance(60000);S.tab="guide";S.ui.level=1;S.op={gt0:true};renderEditor()'); await W(200);
  const lg = await R('var l=[].map.call(ed.querySelectorAll("#gdlog .gdl"),function(x){return x.textContent});[l.length,l.slice(0,6),ed.querySelectorAll("#gdmem tbody tr").length,gdDefs().length]');
  ok('the Log: the last decisions, the newest first, each with the visit\'s time, shown / skipped / ended and why (at most 30); the memory table, a row an item', lg[0] >= 4 && lg[0] <= 30 && lg[1].some(x => /^[0-2]:\d\dshown/.test(x)) && lg[1].some(x => /^[0-2]:\d\dended/.test(x)) && lg[1].some(x => /^[0-2]:\d\dskipped/.test(x)) && lg[2] === lg[3], lg);
  const lw = await R('gdAdvance(600000);ed.querySelectorAll("#gdlog .gdl").length');
  ok('...never more than 30', lw <= 30 && lw >= 4, lw);

  // ---------- 10. the five new scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^gd2_/.test(x[1])})');
  ok('The console lists round 2\'s five guidance scenes (15 minutes in, ignore three, Fewer like this, leave and come back, during a tour)', scenes.length === 5 && JSON.stringify(scenes.map(s => s[1])) === JSON.stringify(['gd2_15min', 'gd2_ignore3', 'gd2_fewer', 'gd2_back', 'gd2_tour']), scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    const does = await R('ed.querySelector(".agscenes").textContent');
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 60000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`${id === 'gd2_15min' ? '[the stage] ' : ''}Scene "${label}": Set up and ask runs it, ✓`, /Does: nothing:/.test(does) && r && r.ok === true, r && [r.note, r.answer]);
    await R('gdStop("test");G.gd.rate=1;G.gd.manual=false');
  }
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
