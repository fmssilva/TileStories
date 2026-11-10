// g7 checks (Playwright + the installed Edge): node test9k.js -> PASS/FAIL per check
// Guidance, round 7 (_3.8 section 8): Fun. The Moments through the scheduler (between steps only, Lively only, never on a Solemn point,
// once a card, 5 min apart; the old timers gone); a joke after pokes (real clicks on Azu), its variants rotating (the least seen); a
// point's joke when it settles, none for a point without one; the treasure hunt invitation (Family only, after a card, once a visit; its
// chip starts the real hunt; done when found); each seasonal window with Today is, and the 1st of November never a joke; the Solemn gate
// for every Fun item, with Why not now?'s words; Calm and Lively only; the Fun lane and the sixth visitor's timeline (Instant, in the
// page); Guide › Mascot's link rows; the ✦ fix (0 px over words on the card past the hero); every new row's demo; the five scenes in Ask
// Azu's console. MUTATE=1 node test9k.js removes the Fun kind's Solemn gate: the checks named [solemn] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const t00 = Date.now();
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  const lap = n => { if (process.env.TIMING) console.log('TIME ' + n + ' ' + Math.round((Date.now() - t00) / 1000) + ' s'); };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('var __c=gdCheck;gdCheck=function(it,items,v,ctx,now){if(it.kindSolemnOff&&ctx&&ctx.solemn)ctx=Object.assign({},ctx,{solemn:false});return __c(it,items,v,ctx,now)}'); console.log('MUTATION: the Fun kind\'s Solemn gate removed'); }
  await R('window.__g0=JSON.stringify(S.guidance);window.__jk0=JSON.stringify(S.ptJokes)');
  const base = 'S.guidance=JSON.parse(window.__g0);S.ptJokes=JSON.parse(window.__jk0);S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.ui.gdSR=false;S.ui.gdToday="2026-03-10";S.m.solemn=false;S.vis.aud="explorer";S.guideCfg.life="lively";S.guideCfg.azuTap="play";S.guideCfg.gbtn="both";S.guideCfg.mascot=true;G.gd.demo=false;gdChip(null);';
  /* only Fun On (the other kinds rest), a first visit on the wall, the clock by hand */
  const FUN = 'S.guidance.kinds.forEach(function(k){k.on=k.id==="fun"});';
  const fresh = async (extra) => { await R(base + 'if(G.on)stopTour();stopSpeech();G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.camZoom=1;S.view="card";S.stop="closed";renderPhone(false);G.gd.manual=true;gdStop("test");gdResetVisitor();G.gd.movedAt=null;G.gd.touchAt=null;G.gd.onWall=false;' + (extra || '') + 'gdAdvance(250);'); await W(120); };
  const editor = async (tab, ops) => { await R(`S.tab="${tab}";S.ui.level=1;S.op=${JSON.stringify(ops)};renderEditor()`); await W(120); };

  lap('before 1');
  // ---------- 1. the scheduler alone (gdPick / gdWhyNot on a fake clock and a fake memory) ----------
  await R(base);
  const pk = (mod, now, id) => R(`S.guidance=JSON.parse(window.__g0);var V={visits:1,visit:{n:1,at:0,lastEnd:null,cards:0,items:{opening:{shown:1,last:0,end:9000,dismissed:0}},starts:[0],ev:{},pokes:0,rest:{},fewer:{}},ever:{items:{opening:{shown:1,done:false,by:""}}},used:{}},
    C={on:true,breathing:15,limits:{perMinute:2,perVisit:30},tau:120,demo:false,showing:null,remark:null,speaking:false,panel:null,movedAt:null,touchAt:null,onWall:true,where:"",marker:null,gbtn:{name:"the ✦",x:40,y:700},central:null,stillSince:-Infinity,returning:false,aud:"explorer",tips:"all",lively:true,solemn:false,today:"03-10",solemnDay:null,pokes:0,idleSince:null,covers:{}},
    vm=function(id,o){V.visit.items[id]=Object.assign({shown:0,last:null,end:null,dismissed:0,acted:0,how:""},o)},TOUR=function(t,e,play){C.tour={between:true,step:2,talked:t,toEnd:!!e,paused:!play};C.onWall=false;C.where="the tour";C.moment="between"};
    ${mod || ''};var items=gdItems(),only=${id ? `"${id}"` : 'null'};${id ? `gdWhyNot(items,V,C,${now},"${id}")` : `var r=gdPick(items,V,C,${now});[r.item?r.item.id:null,r.why,!!r.answer]`}`);
  const P = {};
  // the Moments
  P.water = await pk('TOUR(70)', 600000); P.tick = await pk('TOUR(70);C.moment=null', 600000, 'water'); P.noTour = await pk('C.moment="between"', 600000, 'water');
  P.talk = await pk('TOUR(30)', 600000, 'water'); P.calmW = await pk('TOUR(70);C.lively=false', 600000, 'water'); P.solW = await pk('TOUR(70);C.solemn=true', 600000, 'water');
  P.card = await pk('TOUR(70);vm("water",{shown:1,last:500000,end:508000,card:0})', 600000, 'water'); P.card2 = await pk('TOUR(70);vm("water",{shown:1,last:200000,end:208000,card:0});V.visit.cards=1', 600000);
  P.apart = await pk('TOUR(70,true);vm("water",{shown:1,last:400000,end:408000,card:0});V.visit.lastEnd=408000', 500000, 'cookie'); P.cookie = await pk('TOUR(10,true)', 600000);
  P.onlyM = await pk('TOUR(70);V.visit.pokes=3', 600000, 'joke');
  // round 11: a playing tour holds every Fun item (the Moments' own rules above are read with the tour not playing)
  P.holdW = await pk('TOUR(70,false,true)', 600000, 'water'); P.holdC = await pk('TOUR(10,true,true)', 600000, 'cookie'); P.holdJ = await pk('TOUR(70,false,true);C.moment=null;C.pokes=3', 600000, 'joke'); P.holdPick = await pk('TOUR(70,false,true)', 600000);
  ok('[fun hold] Round 11: while a tour plays every Fun item is held (the water break, the cookie, a joke): Why not now? says "held while a tour plays", and nothing is picked as it moves on', [P.holdW, P.holdC, P.holdJ].every(x => /held while a tour plays/.test(x)) && P.holdPick[0] === null, [P.holdW, P.holdC, P.holdJ, P.holdPick]);
  ok('Moments through the scheduler (their own rules, the tour not playing): as the tour moves on (context.moment "between"), after 60 s of narration, the Water break is the pick (its kind Fun, a Moment)', P.water[0] === 'water' && /between two steps, 70 s of narration/.test(P.water[1]), P.water);
  ok('...between steps only: never at a tick of the clock, never outside a tour, not before its 60 s of narration (and it says so)', /a Moment comes only as the tour moves on/.test(P.tick) && /only between two steps of a tour/.test(P.noTour) && /after 60 s of the tour's narration \(30 s so far\)/.test(P.talk), [P.tick, P.noTour, P.talk]);
  ok('...Lively only (Calm: none); [solemn] never on a Solemn point', /Lively only/.test(P.calmW) && /a Solemn point: no fun here/.test(P.solW), [P.calmW, P.solW]);
  ok('...once a card (again on the next card); 5 min apart (the Moments\' gap 300 s, the cookie after a water break); the cookie before the last step', /shown on this card/.test(P.card) && P.card2[0] === 'water' && /Moments's gap 300 s/.test(P.apart) && P.cookie[0] === 'cookie' && /the tour's last step next/.test(P.cookie[1]), [P.card, P.card2[0], P.apart, P.cookie]);
  ok('...and as the tour moves on only a Moment may show (the joke waits)', /only a Moment holds it/.test(P.onlyM), P.onlyM);
  // a joke
  P.j3 = await pk('C.pokes=3', 30000); P.j2 = await pk('C.pokes=2', 30000, 'joke'); P.jSince = await pk('C.pokes=5;vm("joke",{shown:1,last:20000,end:24000,pk:3})', 60000, 'joke'); P.j6 = await pk('C.pokes=6;vm("joke",{shown:1,last:20000,end:24000,pk:3});V.visit.lastEnd=59000', 60000);
  const NOAC = 'S.guidance.kinds[0].on=false;S.guidance.kinds[1].on=false;';
  P.jLate = await pk(NOAC, 660000); P.jEarly = await pk('', 300000, 'joke'); P.jDay = await pk('C.pokes=3;C.solemnDay="1 Nov"', 30000, 'joke'); P.jTour = await pk('C.pokes=3;C.tour={between:true,step:1,talked:5};C.onWall=false', 30000, 'joke');
  ok('A joke after 3 pokes on Azu (not after 2), and again 3 pokes after it last showed; an answer to the pokes, so the breathing room does not hold it', P.j3[0] === 'joke' && P.j3[2] === true && /3 pokes on Azu/.test(P.j3[1]) && /after 3 pokes on Azu \(2 so far\)/.test(P.j2) && /\(2 so far since it last showed\)/.test(P.jSince) && P.j6[0] === 'joke' && P.j6[2] === true, [P.j3, P.j2, P.jSince, P.j6[0]]);
  ok('...and rarely at a pause late in the visit (from 10:00, once a visit), never in a tour, never on a Solemn day', P.jLate[0] === 'joke' && /a pause late in the visit/.test(P.jLate[1]) && P.jLate[2] === false && /from 10:00/.test(P.jEarly) && /a Solemn day \(1 Nov\): no jokes today/.test(P.jDay) && /quiet in a tour|held while a tour plays/.test(P.jTour), [P.jLate[0], P.jEarly, P.jDay, P.jTour]);  // round 11: the Fun hold comes first
  const rot = await R('var it=gdItems().filter(function(x){return x.id==="joke"})[0];[gdLine(it,false,{seen:{}}),gdLine(it,false,{seen:{mascot_joke_2:1}}),gdLine(it,false,{seen:{mascot_joke_2:1,mascot_joke_3:1}}),gdLine(it,false,{seen:{mascot_joke_2:2,mascot_joke_3:1,mascot_joke_1:1}}),TXT("mascot_joke_2"),TXT("mascot_joke_3"),TXT("mascot_joke_1")]');
  ok('...its variants rotate: the least seen shows (ties: the first, today\'s first joke)', rot[0] === rot[4] && rot[1] === rot[5] && rot[2] === rot[6] && rot[3] === rot[5], rot.slice(0, 4));
  // a point's joke
  const PT = 'V.used.card=1000;V.used.view=2000;C.central={id:"torreao",name:"The Torreão",x:180,y:360,middle:true,tour:false};';
  P.pt = await pk(NOAC + PT, 120000); P.ptNone = await pk(PT.replace(/torreao/g, 'arcade').replace('The Torreão', 'The arcades'), 120000, 'ptjoke:arcade');
  P.ptLine = await R('var it=gdItems().filter(function(x){return x.id==="ptjoke:torreao"})[0];[gdLine(it,false,gdSlots(it)),TXT("ptj_torreao_1"),gdItems().filter(function(x){return x.base==="ptjoke"&&x.jokes.length}).map(function(x){return x.point}).join()]');
  ok('{point}\'s joke when its point settles in the middle: the Torreão\'s own line; a point with none (the arcades) says none, never a generic one', P.pt[0] === 'ptjoke:torreao' && P.ptLine[0] === P.ptLine[1] && /The arcades has no joke of its own/.test(P.ptNone) && P.ptLine[2] === 'painting,torreao,river', [P.pt[0], P.ptNone, P.ptLine]);
  // the treasure hunt
  const HU = 'C.aud="family";V.used.card=1000;';
  P.hunt = await pk(NOAC + HU, 120000); P.hAud = await pk('V.used.card=1000', 120000, 'hunt'); P.hCard = await pk('C.aud="family"', 120000, 'hunt');
  P.hOnce = await pk(HU + 'vm("hunt",{shown:1,last:60000,end:66000})', 120000, 'hunt'); P.hDone = await pk(HU + 'V.ever.items.hunt={shown:1,done:true,by:"hunt"}', 120000, 'hunt');
  ok('The treasure hunt invitation: Family only, once the visitor has opened a card, once a visit, done when a treasure is found', P.hunt[0] === 'hunt' && /only for Family/.test(P.hAud) && /only once a card opened/.test(P.hCard) && /shown once this visit/.test(P.hOnce) && /done for good \(the visitor found the treasure\)/.test(P.hDone), [P.hunt[0], P.hAud, P.hCard, P.hOnce, P.hDone]);
  // seasonal
  const sea = await R('var it=gdItems().filter(function(x){return x.id==="season"})[0];["06-13","12-24","01-03","11-01","03-10","07-01"].map(function(md){var sv=gdSeasonOf(it,md);return sv?sv.says+(sv.solemn?"!":""):""})');
  P.sXmas = await pk(NOAC + 'C.today="12-24"', 120000); P.sNone = await pk('C.today="03-10"', 120000, 'season');
  P.nov1 = await R('S.ui.gdToday="2026-11-01";var c=gdCtxBase();S.ui.gdToday="2026-03-10";[c.today,c.solemnDay]');
  P.sNovJoke = await pk(`C.today="11-01";C.solemnDay="1 Nov";C.pokes=3`, 120000, 'joke');
  ok('Seasonal: each line only in its window (13 Jun Santo António, 24 Dec and 3 Jan Christmas, 1 Nov the 1755 line, 10 Mar and 1 Jul none)', JSON.stringify(sea) === '["gd_season_antonio","gd_season_xmas","gd_season_xmas","gd_season_1755!","",""]' && P.sXmas[0] === 'season' && /no season today \(10 Mar\)/.test(P.sNone), [sea, P.sXmas[0], P.sNone]);
  ok('...the 1st of November is a Solemn day: its line a remembrance, and never a joke that day', P.nov1[0] === '11-01' && P.nov1[1] === '1 Nov' && /a Solemn day \(1 Nov\): no jokes today/.test(P.sNovJoke) && !/joke/i.test(await R('TXT("gd_season_1755")')), [P.nov1, P.sNovJoke]);
  // the Solemn gate for every Fun item, in Why not now?'s words
  const solAll = await R(`S.guidance=JSON.parse(window.__g0);var V={visits:1,visit:{n:1,at:0,items:{},starts:[],ev:{},pokes:0,rest:{},fewer:{},cards:1},ever:{items:{}},used:{card:1,view:2}},C={on:true,breathing:15,limits:{},tau:120,onWall:true,where:"",gbtn:{x:1,y:1},central:{id:"torreao",name:"The Torreão",x:1,y:1,middle:true},stillSince:-Infinity,aud:"family",lively:true,solemn:true,today:"12-24",pokes:3,covers:{},tour:null};
    var items=gdItems(),o={};["water","cookie","joke","ptjoke:torreao","hunt","season"].forEach(function(id){var c=Object.assign({},C);if(/water|cookie/.test(id)){c.tour={between:true,talked:99,toEnd:true};c.moment="between";c.onWall=false}o[id]=gdWhyNot(items,V,c,900000,id)});o`);
  ok('[solemn] the Solemn gate holds for every Fun item (water, cookie, joke, a point\'s joke, the hunt invitation, the seasonal line), in Why not now?\'s words: "a Solemn point: no fun here"', Object.keys(solAll).length === 6 && Object.keys(solAll).every(k => /a Solemn point: no fun here/.test(solAll[k])), solAll);
  const calm = await R(`S.guidance=JSON.parse(window.__g0);var V={visits:1,visit:{n:1,at:0,items:{},starts:[],ev:{},pokes:0,rest:{},fewer:{},cards:1},ever:{items:{}},used:{card:1,view:2}},C={on:true,breathing:15,limits:{},tau:120,onWall:true,where:"",gbtn:{x:1,y:1},central:{id:"torreao",name:"The Torreão",x:1,y:1,middle:true},stillSince:-Infinity,aud:"family",lively:false,solemn:false,today:"12-24",pokes:3,covers:{}};
    var items=gdItems(),o={};["joke","ptjoke:torreao","hunt","season"].forEach(function(id){var r=gdCheck(items.filter(function(x){return x.id===id})[0],items,V,C,900000);o[id]=r.ok?"ok":r.no});o`);
  ok('Calm: only the Fun items without Lively only (the hunt invitation, the seasonal line); the joke and a point\'s joke wait for Lively', /Lively only/.test(calm.joke) && /Lively only/.test(calm['ptjoke:torreao']) && calm.hunt === 'ok' && calm.season === 'ok', calm);
  ok('The Fun kind: On, at most 4 a visit, 5 min apart, a bubble only, "Solemn points: off" fixed (no option); its items the study\'s', await R('var k=gdKindById("fun"),i=S.guidance.items;k.on&&k.cap===4&&k.gap===300&&k.spoken==="bubble"&&k.solemnOff===true&&gdGroupById("moments").items.join()==="water,cookie"&&gdGroupById("play").items.join()==="joke,ptjoke,hunt,season"&&!("notSolemn" in {})&&i.water.form==="moment"&&i.hunt.aud==="family"&&i.hunt.hasUsed==="card"&&i.hunt.doneWhen==="hunt"'));

  lap('before 2');
  // ---------- 2. on the page: the Moments in a real tour; the old timers gone ----------
  await fresh(FUN + 'S.guidance.items.water.talkFor=1;G.gd.manual=false;G.gd.rate=1;');
  const gone = await R('[typeof momentAllowed,G.cardMoments,G.lastMoment,S.guideCfg.moments,S.guideCfg.waterAfter,String(azPoke).indexOf("mascot_joke")<0]');
  ok('The old Moments timers and the poke joke\'s are gone (momentAllowed, G.cardMoments, G.lastMoment, Mascot\'s moments and waterAfter, the joke in azPoke)', gone[0] === 'undefined' && gone[1] === undefined && gone[2] === undefined && gone[3] === undefined && gone[4] === undefined && gone[5], gone);
  await R('S.guidance.kinds.forEach(function(k){k.on=false});G.talkFrom=performance.now()-90000;startTour()');
  const noSched = await until('G.i>=1', 25000) && await R('!G.moment&&!ph.querySelector(".azmoment")');
  ok('...no Moment without the scheduler: with Fun Off the tour moves on, no water break', noSched, await R('[G.i,G.moment]'));
  // round 11: in a real tour the water break is held (Fun never during a tour); it was a Moment between two steps
  await R('stopTour();gdResetVisitor();' + FUN + 'G.talkFrom=performance.now()-90000;startTour()');
  let sawW = false; { const t0 = Date.now(); while (Date.now() - t0 < 25000) { if (await R('!!G.moment||!!ph.querySelector(".azmoment")')) { sawW = true; break; } if (await R('G.i>=2')) break; await W(150); } }
  const wa1 = await R('[G.i,gdVisitOf(S.gdVisitor,"water").shown,(function(id){var a=gdAll(gdItems(),S.gdVisitor,gdContext(),gdNow()).filter(function(x){return x.it.id===id})[0];return a?String(a.r.no||""):""})("water")]');
  ok('[fun hold] A real tour: it moves on, step after step, and no water break shows between them (Fun held while it plays; Why not now? says so)', !sawW && wa1[0] >= 1 && wa1[1] === 0 && /held while a tour plays/.test(wa1[2]), [sawW, wa1]);
  await R('stopTour();gdResetVisitor();' + FUN + 'S.guideCfg.life="calm";G.talkFrom=performance.now()-90000;startTour()'); await until('G.i>=1', 25000);
  const calmT = await R('!G.moment&&!ph.querySelector(".azmoment")');
  await R('stopTour();gdResetVisitor();' + FUN + 'S.guideCfg.life="lively";S.m.solemn=true;G.talkFrom=performance.now()-90000;startTour()'); await until('G.i>=1', 25000);
  const solT = await R('!G.moment&&!ph.querySelector(".azmoment")&&gdVisitOf(S.gdVisitor,"water").shown===0');
  ok('...Calm: none; [solemn] on a Solemn point: none', calmT && solT, [calmT, solT]);
  await R('stopTour();S.m.solemn=false');

  lap('before 3');
  // ---------- 3. a joke after real clicks on Azu; a point's joke when it settles ----------
  await fresh(FUN + 'G.gd.manual=false;G.gd.rate=1;');
  const pokeAzu = async n => { for (let i = 0; i < n; i++) { await p.locator('#ph .azfab').click(); await R('window.__tap=performance.now()'); await until('performance.now()-window.__tap>650', 1500); } };
  await R('G.taps=[]'); await pokeAzu(2); await W(900);
  const j2 = await R('!(G.gd.cur&&G.gd.cur.item.id==="joke")');
  await pokeAzu(1);
  const j1 = await until('!!(G.gd.cur&&G.gd.cur.item.id==="joke"&&!G.gd.cur.pull)', 5000); await W(300);
  const jl = await R('var b=ph.querySelector(".gdbub:not(.gd-out) button[data-gdbub]");[G.gd.cur&&G.gd.cur.text,b?gdLineCount(b.textContent,"tip",false):0,gdOverWords(),G.gd.cur&&G.gd.cur.spoken,TXT("mascot_joke_2")]');
  ok('A joke after three real clicks on Azu (not after two): one of Azu\'s jokes, the least seen first, a bubble only (never spoken), 1-2 lines, 0 px over words', j2 && j1 && jl[0] === jl[4] && jl[1] >= 1 && jl[1] <= 2 && jl[2] === 0 && !jl[3], [j2, j1, jl]);
  await R('gdStop("test","ignored");G.gd.touchAt=null'); await R('G.taps=[]'); await pokeAzu(3);
  const j3 = await until('!!(G.gd.cur&&G.gd.cur.item.id==="joke"&&!G.gd.cur.pull)', 5000);
  ok('...three more: another joke, the next least seen (they rotate)', j3 && await R('G.gd.cur.text===TXT("mascot_joke_3")'), await R('G.gd.cur&&G.gd.cur.text'));
  await fresh(FUN + 'var t=gdNow()-60000;S.gdVisitor.used.card=t;S.gdVisitor.used.view=t;gdSimAim("torreao");G.gd.movedAt=gdNow()-3000;');
  await R('gdAdvance(3000)');
  const pj = await R('var c=G.gd.cur;[c&&c.item.id,c&&c.text,TXT("ptj_torreao_1"),gdOverWords(),!!G.gd.az]');
  ok('{point}\'s joke when the Torreão settles in the middle: its own line, Azu beside it, 0 px over words', pj[0] === 'ptjoke:torreao' && pj[1] === pj[2] && pj[3] === 0 && pj[4], pj);
  await R('gdStop("test");gdSimAim("arcade");G.gd.movedAt=gdNow()-3000;gdAdvance(3000)');
  const pa = await R('[G.gd.cur&&G.gd.cur.item.id,gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{showing:null,touchAt:null}),gdNow(),"ptjoke:arcade")]');
  ok('...the arcades (no joke of its own): none, and Why not now? says so', !(pa[0] || '').startsWith('ptjoke') && /The arcades has no joke of its own/.test(pa[1]), pa);

  lap('before 4');
  // ---------- 4. the treasure hunt: the invitation, its chip (a real click), the hunt, found (a real click on the pin) ----------
  await fresh(FUN + 'S.vis.aud="explorer";S.gdVisitor.used.card=gdNow();');
  await R('gdAdvance(3000)');
  const hEx = await R('[G.gd.cur&&G.gd.cur.item.id,gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{showing:null,touchAt:null}),gdNow(),"hunt")]');
  await fresh(FUN + 'S.vis.aud="family";');
  await R('gdAdvance(3000)');
  const hNo = await R('[G.gd.cur&&G.gd.cur.item.id,gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{showing:null,touchAt:null}),gdNow(),"hunt")]');
  await R('S.gdVisitor.used.card=gdNow();gdAdvance(3000)');
  const hi = await R('var c=G.gd.cur,ch=ph.querySelector(".gdbub:not(.gd-out) [data-gdcta]");[c&&c.item.id,c&&c.text,ch?[ch.offsetWidth,ch.offsetHeight]:null,gdOverWords(),gdHuntThing()]');
  ok('The treasure hunt invitation: not for an Explorer, not before a card, then for a Family: "Want a treasure hunt? Find {thing} on the wall." with its 44 px chip, 0 px over words', /only for Family/.test(hEx[1]) && /only once a card opened/.test(hNo[1]) && hi[0] === 'hunt' && hi[1] === 'Want a treasure hunt? Find ' + hi[4] + ' on the wall.' && hi[2] && hi[2][1] >= 44 && hi[3] === 0, [hEx[1], hNo[1], hi]);
  await p.locator('#ph .gdbub:not(.gd-out) [data-gdcta]').click();
  const hs = await until('G.cam.open&&!!G.cam.hunt&&!!ph.querySelector(\'[data-cpin="\'+G.cam.hunt+\'"]\')', 4000);
  const hid = await R('[G.cam.hunt,(ph.querySelector(".ca-ans")||{}).textContent,gdVisitOf(S.gdVisitor,"hunt").how]');
  ok('...its chip (a real click) opens Ask about the view and starts its own hunt (G.cam.hunt), for the point the invitation named', hs && hid[1] && hid[1].indexOf(hi[4]) >= 0 && hid[2] === 'acted', hid);
  await p.locator(`#ph [data-cpin="${hid[0]}"]`).click();
  const hf = await until('!G.cam.hunt&&gdEverOf(S.gdVisitor,"hunt").done', 3000);
  ok('...the treasure found (a real click on its pin): the invitation is done for good; once a visit', hf && await R('/done for good \\(the visitor found the treasure\\)/.test(gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{showing:null,touchAt:null,panel:null,speaking:false}),gdNow(),"hunt"))'), await R('gdEverOf(S.gdVisitor,"hunt")'));

  lap('before 5');
  // ---------- 5. Today is (the Editor's date, a real input) and each season on the phone; the Solemn gate on the page ----------
  await fresh(FUN + 'S.guidance.items.season.notBefore=0;');
  await editor('guide', { gt0: true });
  await p.locator('input[data-sel="@ui.gdToday"]').fill('2026-12-20'); await W(400);
  const td = await R('[S.ui.gdToday,(ed.querySelector("#gdtoday")||{}).textContent]');
  await R('gdStop("test");G.gd.demo=false;gdChip(null);' + FUN + 'gdResetVisitor();S.view="card";S.stop="closed";renderPhone(false);G.gd.movedAt=null;gdAdvance(2000)');
  const sx = await R('[G.gd.cur&&G.gd.cur.item.id,G.gd.cur&&G.gd.cur.text,TXT("gd_season_xmas"),G.gd.cur&&G.gd.cur.spoken]');
  ok('Preview & Test › Today is (a real date input): 20 Dec reads back with the Christmas line, and the phone says it (a bubble only)', td[0] === '2026-12-20' && /Today is 20 Dec/.test(td[1]) && /nearly Christmas/.test(td[1]) && sx[0] === 'season' && sx[1] === sx[2] && !sx[3], [td, sx]);
  const seasons = [];
  for (const [d, k] of [['2026-06-13', 'gd_season_antonio'], ['2027-01-03', 'gd_season_xmas'], ['2026-11-01', 'gd_season_1755'], ['2026-03-10', '']]) {
    await R(`gdStop("test");S.ui.gdToday="${d}";gdResetVisitor();G.gd.movedAt=null;gdAdvance(2000)`);
    seasons.push(await R(`[S.ui.gdToday,G.gd.cur&&G.gd.cur.item.id==="season"?G.gd.cur.text:"",${k ? `TXT("${k}")` : '""'}]`));
  }
  ok('...each window on the phone: 13 Jun Santo António, 3 Jan Christmas (over the new year), 1 Nov the 1755 remembrance, 10 Mar nothing', seasons.every(s => s[1] === s[2]), seasons);
  await R(`gdStop("test");S.ui.gdToday="2026-11-01";gdResetVisitor();S.gdVisitor.visit.pokes=3;`);
  const nj = await R('gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{showing:null,touchAt:null,remark:null}),gdNow(),"joke")');
  ok('...on the 1st of November no joke (Why not now?: a Solemn day)', /a Solemn day \(1 Nov\): no jokes today/.test(nj), nj);
  await fresh(FUN + 'S.m.solemn=true;S.vis.aud="family";S.gdVisitor.used.card=gdNow();S.ui.gdToday="2026-12-20";gdSimAim("painting");G.gd.movedAt=gdNow()-3000;S.gdVisitor.visit.pokes=3;');
  await R('gdAdvance(4000)');
  const sg = await R('[G.gd.cur&&G.gd.cur.item.kind,gdSolemnNow()]');
  await editor('guide', { gt0: true });
  await R('var s=ed.querySelector("[data-gdwhysel]");s.value="joke"'); await p.locator('[data-gdwhynot]').click(); await W(200);
  const sw = await R('(ed.querySelector("#gdwhynot")||{}).textContent');
  ok('[solemn] on the page: Ribeira Palace marked Solemn in the middle: no Fun shows (with a joke, a hunt and Christmas all due), and Why not now? (a real click) says "a Solemn point: no fun here"', sg[0] !== 'fun' && sg[1] === true && /^A joke: a Solemn point: no fun here/.test(sw), [sg, sw]);
  await R('gdSimAim("torreao");G.gd.movedAt=gdNow()-3000');
  ok('...another point in the middle is not the Solemn one', await R('gdSolemnNow()===false'));

  lap('before 6');
  // ---------- 6. the simulator: the Fun lane, Tour lover's Moments, the sixth visitor ----------
  const sim = await R(base + 'gdPresetWrite("first");S.ui.gdToday="2026-12-20";var o={};["tour","solemn","first"].forEach(function(id){var r=gdSimulate(id);o[id]={fun:r.dots.filter(function(d){return d.kind==="fun"}).map(function(d){return[d.t,d.base,d.how]}),sum:gdSimSum(r,15).words,same:JSON.stringify(gdSimulate(id))===JSON.stringify(r)}});o.names=GD_SIM_SCRIPTS.map(function(s){return s.name});S.ui.gdToday="2026-03-10";o');
  const sol = sim.solemn.fun;
  ok('Simulate a visit: a sixth visitor, Family on a Solemn wall', sim.names.length === 6 && sim.names[5] === 'Family on a Solemn wall', sim.names);
  ok('[solemn] ...no Fun dot while the Solemn point is in the middle (its first 5 minutes, Today is 20 Dec: the Christmas line due from 1:00); Fun after, once a Lively point is there (deterministic)', sol.length >= 1 && sol.every(d => d[0] >= 300000) && sim.solemn.same, sol);
  ok('[fun hold] ...Tour lover gets no Moment in its tours (round 11: Fun is held while a tour plays; it had two water breaks)', sim.tour.fun.filter(d => d[1] === 'water' || d[1] === 'cookie').length === 0 && sim.tour.same, sim.tour.fun);
  await R(base + 'gdPresetWrite("first");'); await editor('guide', { gt0: true, 'gd:sim': true }); await R('setOpt("ui.gdSim","tour")'); await W(300);
  await p.locator('[data-set="ui.gdSim"][data-v="solemn"]').click(); await W(400);
  const lane = await R('var s=ed.querySelector("[data-gdsimsvg]"),labs=[].map.call(s.querySelectorAll(".gdlab"),function(x){return[x.textContent,+x.getAttribute("y")]}),fy=labs.filter(function(l){return l[0]==="Fun"})[0][1]-4;[S.ui.gdSim,[].filter.call(s.querySelectorAll(".gddot .gdd"),function(c){return Math.abs(+c.getAttribute("cy")-fy)<2}).length,!!s.querySelector(".gdnone"),(ed.querySelector("#gdsimsum")||{}).textContent,(ed.querySelector(".gdleg")||{}).textContent]');
  ok('...the Editor\'s timeline (the sixth visitor chosen by a real click): the Fun lane shows its dots, no "no Fun items yet", Fun in the summary and the legend', lane[0] === 'solemn' && lane[1] >= 1 && !lane[2] && /Fun \d/.test(lane[3]) && /Fun/.test(lane[4]), lane);

  lap('before 7');
  // ---------- 7. Guide › Mascot's link rows; the Mix's Fun row ----------
  await R(base); await editor('guide', { gt2: true, s7mascot: true });
  const ml = await R('var f=ed.querySelector(\'[data-fo="s7mascot"]\'),rows=[].filter.call(f.querySelectorAll(".row"),function(r){var l=r.querySelector("label");return l&&/^(Moments|Water break after)$/.test(l.textContent)});rows.map(function(r){return[r.querySelector("label").textContent,rowKey(r),(r.querySelector("[data-goto]")||{}).textContent,(r.querySelector("[data-goto]")||{dataset:{}}).dataset.goto,(r.querySelector(".uses")||{}).textContent]})');
  ok('Guide › Mascot: Moments and Water break after are link rows ("Edited in Guide › Guidance › Fun › Moments", its On the group\'s; the water break\'s 60 s of narration)', ml.length === 2 && ml[0][1] === 'guidance.kinds.2.groups.0.on' && /^Edited in Guide › Guidance › Fun › Moments/.test(ml[0][2]) && ml[0][3] === 'gd:g:moments' && ml[1][1] === null && ml[1][3] === 'gd:i:water' && /60 s of narration/.test(ml[1][4]), ml);
  await p.locator('[data-fo="s7mascot"] [data-goto="gd:g:moments"]').click(); await W(400);
  ok('...its link (a real click) opens Guide › Guidance › Fun › Moments', await R('var f=ed.querySelector(\'[data-fo="gd:g:moments"]\');S.tab==="guide"&&!!f&&f.open&&!!f.closest(\'[data-fo="gd:fun"]\')'));
  await editor('guide', { gt4: true, 'gd:overall': true, 'gd:mix': true });
  ok('The Mix\'s Fun row loses "no items yet"; the Fun kind reads its weight back', await R('var t=(ed.querySelector(".gdmixt")||{}).textContent||"";/Fun/.test(t)&&!/no items yet/.test(t)'));

  lap('before 8');
  // ---------- 8. the ✦ fix: on the card past the hero, the ✦ and Azu cover 0 px of words, at every scroll ----------
  await fresh('');
  await R('S.view="card";S.stop="full";renderPhone(false)');
  const star = await R(`var out=[],H=scr.scrollHeight;for(var k=0;k<14;k++){scr.scrollTop=Math.round(700+k*(H-700)/13);fabLayer();var w=ph.querySelector(".gbtn-wrap:not(.intour)");if(!w){out.push(null);continue}var band=ph.querySelector(".gbband"),bt=band?gdPR(band).t:1e9,a=0;
      var mine=[].map.call(w.children,function(x){return gdPR(x)}).filter(function(r){return r.b>r.t});
      scr.querySelectorAll("b,span,small,p,label,button,h1,h2,h3,h4,div,i,em,strong,a,li,td").forEach(function(x){if(x.children.length||!String(x.textContent).trim()||!gdSeen(x))return;var r=gdPR(x);r={l:r.l,t:r.t,r:r.r,b:Math.min(r.b,bt+20)};if(r.b<=r.t)return;mine.forEach(function(m){a+=gdOver(m,r)})});
      out.push([Math.round(a),!!band,Math.round(bt),Math.round(Math.min.apply(null,mine.map(function(m){return m.t})))])}out`);
  const seen = star.filter(Boolean);
  ok('The ✦ fix: on the card past the hero, at 14 scroll positions, the ✦ and Azu cover 0 px of visible words (a band of the card\'s paper behind them, from above them to the foot)', seen.length >= 10 && seen.every(s => s[0] === 0 && s[1] && s[2] + 20 <= s[3]), star);
  await R('cuePlace("ba")'); await W(900); await R('fabLayer()');
  const capState = await R('var w=ph.querySelector(".gbtn-wrap:not(.intour)"),band=ph.querySelector(".gbband"),go=[].filter.call(scr.querySelectorAll("*"),function(x){return/^Go to this part/.test(x.textContent.trim())}).pop();if(!w||!go)null;else{var g=gdPR(go),bt=band?gdPR(band).t:1e9;[g.t>=bt||gdOver(g,gdPR(w.querySelector(".gbtn")))===0,Math.round(bt),Math.round(g.t),scr.style.paddingBottom]}');
  ok('...round 6\'s capture (Media, a Before / After in view): "Go to this part ›" is never under the ✦ (under the band, or clear); the card\'s foot grows by the band', !!capState && capState[0] && parseInt(capState[3]) > 110, capState);
  await R('S.view="card";S.stop="closed";renderPhone(false)');
  ok('...on the wall: no band', await R('!ph.querySelector(".gbband")&&!scr.style.paddingBottom'));

  lap('before 9');
  // ---------- 9. every new row's demo; the Guide script's Jokes ----------
  await R(base); await editor('guide', { gt0: true, gt4: true, 'gd:fun': true, 'gd:g:moments': true, 'gd:g:play': true, 'gd:i:water': true, 'gd:i:cookie': true, 'gd:i:joke': true, 'gd:i:ptjoke': true, 'gd:i:hunt': true, 'gd:i:season': true });
  const keys = await R('var o=[];ed.querySelectorAll(\'[data-fo="gd:fun"] .row,[data-fo="gt0"] .row\').forEach(function(r){var k=rowKey(r);if(k&&o.indexOf(k)<0&&/^(guidance\\.(kinds\\.2|items\\.(water|cookie|joke|ptjoke|hunt|season))|ui\\.gdToday)/.test(k))o.push(k)});o');
  const miss = await R(`${JSON.stringify(keys)}.filter(function(k){return!rowDemoOf(k)})`);
  ok(`Every new row has its demo (${keys.length} rows: Fun's kind and groups, its six items' forms, the seasons' From / To / Solemn day, Today is)`, keys.length >= 150 && miss.length === 0 && ['guidance.items.water.prop', 'guidance.items.water.talkFor', 'guidance.items.cookie.beforeEnd', 'guidance.items.joke.latePause', 'guidance.items.season.seasons.0.from', 'ui.gdToday'].every(k => keys.indexOf(k) >= 0) && (await R('[0,1,2].every(function(i){return["from","to","solemn"].every(function(f){return!!rowDemoOf("guidance.items.season.seasons."+i+"."+f)})})&&!!rowDemoOf("ui.gdJkPoint")')), { n: keys.length, miss });
  await R('S.ui.rowDemo=true;G.gd.manual=false;G.gd.rate=1');
  const demo = async (sel, check, ms) => { await p.locator(sel).first().scrollIntoViewIfNeeded(); await p.locator(sel).first().click(); return until(check, ms || 3500); };
  const d1 = await demo('[data-fo="gd:i:water"] [data-set="@guidance.items.water.prop"][data-v="cookie"]', '!!ph.querySelector(".azmoment [data-azprop=cookie]")', 5000);
  await R('if(G.on)stopTour();S.guidance.items.water.prop="water"');
  const d2 = await demo('[data-fo="gd:i:season"] [data-set="@guidance.items.season.seasons.2.solemn"][data-v="false"]', '!!ph.querySelector(".gdbub:not(.gd-out)")&&/1 Nov/.test((ph.querySelector(".gdchip")||{}).textContent)');
  await R('S.guidance.items.season.seasons[2].solemn=true');
  const d3 = await demo('[data-fo="gd:i:joke"] [data-gdsay]', '!!ph.querySelector(".gdbub:not(.gd-out)")');
  const d4 = await demo('[data-fo="gd:i:ptjoke"] [data-gdjkshow]', '!!(G.gd.cur&&G.gd.cur.item.base==="ptjoke")&&!!ph.querySelector(".gdbub:not(.gd-out)")');
  ok('...pressed (one real click per kind of control): Prop (the cookie in a tour), a season\'s Solemn day (its line and dates), a joke\'s ▸, a point\'s joke ▸', d1 && d2 && d3 && d4, [d1, d2, d3, d4]);
  await R('if(G.on)stopTour();gdStop("test");S.ui.rowDemo=false');
  await R(base); await editor('marker', { m3: true });
  await p.locator('select[data-sel="@ui.gdJkPoint"]').selectOption('torreao'); await W(300);
  const jk = await R('var b=ed.querySelector(".gdjk");[S.ui.gdJkPoint,b?b.querySelectorAll("[data-tx]").length:0,b?b.textContent:"",!!b&&!!b.previousElementSibling&&/Solemn point/.test(b.previousElementSibling.textContent)]');
  await p.locator('.gdjk [data-gdjkadd]').click(); await W(200);
  const jk2 = await R('[S.ptJokes.torreao.length,TXOWN[S.ptJokes.torreao[1]],ed.querySelectorAll(".gdjk [data-tx]").length]');
  await R(`var k=S.ptJokes.torreao[1];S.tx[k]={en:"Zz new joke",pt:"Zz piada"};renderEditor()`);
  const jk3 = await R('var it=gdItems().filter(function(x){return x.id==="ptjoke:torreao"})[0];[it.jokes.length,gdLine(it,false,{n:1,jokes:it.jokes})]');
  ok('Specific Marker › Guide script › Jokes (after the Solemn point row): the point (a real select), its line in each language, + Add a joke (a real click: a new line owned by {point}\'s joke), the new line said in turn', jk[0] === 'torreao' && jk[1] === 2 && /The Torreão \(1 joke\)/.test(jk[2]) && jk[3] && jk2[0] === 2 && jk2[1] === 'gd:i:ptjoke' && jk2[2] === 4 && jk3[0] === 2 && jk3[1] === 'Zz new joke', [jk, jk2, jk3]);
  ok('The new words and their owners: gd_hunt, gd_season_* (the items), ptj_* ({point}\'s joke), the jokes (A joke), the Moments\' asks (their items)', await R('TXOWN.gd_hunt==="gd:i:hunt"&&TXOWN.gd_season_xmas==="gd:i:season"&&TXOWN.ptj_river_1==="gd:i:ptjoke"&&TXOWN.mascot_joke_3==="gd:i:joke"&&TXOWN.mascot_hungry==="gd:i:cookie"&&TXOWN.gd_btn_hunt==="gd:overall"&&!!TX_DOM.gd_hunt&&!!TX_DOM.ptj_river_1'));

  lap('before 10');
  // ---------- 10. the five scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^gd7_/.test(x[1])})');
  ok('The console lists the five new scenes', scenes.length === 5 && JSON.stringify(scenes.map(s => s[1])) === JSON.stringify(['gd7_water', 'gd7_joke', 'gd7_hunt', 'gd7_solemn', 'gd7_xmas']), scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 50000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`${id === 'gd7_solemn' ? '[solemn] ' : ''}Scene "${label}": Set up and ask runs it, ✓`, r && r.ok === true, r && [r.note, r.answer]);
    await R('gdStop("test");G.gd.rate=1;if(G.on)stopTour()');
  }
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log((fails ? `${fails} FAILED` : 'ALL PASS') + ` (${Math.round((Date.now() - t00) / 1000)} s)`);
  await b.close();
})();
