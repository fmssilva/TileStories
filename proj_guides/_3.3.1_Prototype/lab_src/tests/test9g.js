// g3 checks (Playwright + the installed Edge): node test9g.js -> PASS/FAIL per check
// Guidance, round 3 (_3.8 section 8): Azu's remarks and the ✦ hint become items of the one scheduler (their old timers gone).
// 1. Each rule of the two items alone on gdPick (pure, a fake clock): Content › What's in view (one item a point of the wall: settles in
// the middle 1.2 s, never while panning, once a visit for each point, the group's gap 7 s standing in for Content's, at most 12 a visit,
// the 3 lines in turn and a point with a card's own, the group's On) and App › The wall › The ✦ (after 4 pokes or 25 s idle, Lively
// only, never on a Solemn point, once a visit, done when the ✦ menu opens, wherever the ✦ is; never both it and Ask about the view in
// the visit's first minute, the Editor's order); 2. a remark never within the breathing room of a tip, either way, and the yield;
// 3. on the page: the remark's look (2 lines, 7:1, 0 px over words, light, dark, Portuguese), spoken once the guide has been heard,
// its taps; the pokes (real clicks on Azu) and the idle (the visit's clock moved by hand); Tap on Azu shows The ✦ at once; the ✦ menu
// ends it for the visit; the old timers gone; 4. the Editor: the link row in Ask about the view, every new row's demo (the ×10 chip),
// Why not now? and the Log naming them, the words and their owners; 5. the two new scenes in Ask Azu's console.
// MUTATE=1 node test9g.js runs it with the breathing room ignored for the remarks: the checks named [breathing] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(Math.min(ms, 500));
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('var __gc=gdCheck;gdCheck=function(it,items,v,ctx,now){if(it.base==="inview"){var v2=JSON.parse(JSON.stringify(v));v2.visit.lastEnd=null;return __gc(it,items,v2,ctx,now)}return __gc(it,items,v,ctx,now)}'); console.log('MUTATION: the breathing room ignored for the remarks (What\'s in view)'); }
  // the set-up: the Ribeira wall, a first visit whose opening was said 30 s ago, the clock moved by hand; the round's items On
  const base = 'S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.guideCfg.gbtn="both";S.guideCfg.mascot=true;S.guideCfg.life="lively";S.guideCfg.azuTap="play";S.m.solemn=false;' +
    'S.guidance.on=true;S.guidance.breathing=15;S.guidance.perMinute=2;["opening","tapmarker","askview","gbhint","inview","tourof"].forEach(function(id){S.guidance.items[id].on=true});gdGroupById("inview").on=true;G.gd.demo=false;gdChip(null);';
  const opened = 'var o=gdVM("opening"),oe=gdEM("opening");o.shown=1;o.last=gdNow()-30000;o.end=gdNow()-21000;oe.shown=1;S.gdVisitor.visit.lastEnd=gdNow()-21000;';
  const fresh = async (extra, manual = true) => { await R(base + 'if(G.on)stopTour();G.cam.open=false;S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);' + `G.gd.manual=${manual};G.gd.rate=1;` + 'gdStop("test");S.camX=.4;S.camY=.5;S.camZoom=1;renderPhone(false);gdResetVisitor();G.gd.movedAt=null;G.gd.touchAt=null;G.gd.actAt=null;G.gd.onWall=false;' + opened + (extra || '') + (manual ? 'gdAdvance(250)' : '')); await W(150); };
  const CR = 'var px=function(c){var m=c.match(/[\\d.]+/g).map(Number);return{r:m[0],g:m[1],b:m[2],a:m.length>3?m[3]:1}},lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},cr=function(a,b){var x=lum(a),y=lum(b);return Math.round((Math.max(x,y)+.05)/(Math.min(x,y)+.05)*100)/100};';
  const look = CR + 'var b=ph.querySelector(".gdbub:not(.gd-out)"),bt=b.querySelector("button"),cs=getComputedStyle(bt),lh=parseFloat(cs.lineHeight),lines=Math.round((bt.offsetHeight-parseFloat(cs.paddingTop)-parseFloat(cs.paddingBottom))/lh),B=gdPR(b),g=ph.querySelector(".gbtn"),G1=g?gdPR(g):null,nb=document.getElementById("navbar"),N=nb&&gdSeen(nb)?gdPR(nb):null,tg=G.gd.cur&&gdTarget(G.gd.cur.item);' +
    '[lines,cr(px(cs.color),px(getComputedStyle(b).backgroundColor)),gdOverWords(),G1?gdOver(B,G1):0,N?gdOver(B,N):0,Math.round(bt.offsetHeight),bt.textContent,tg?gdOver(B,tg.rect||tg.box):-1]';
  // a poke on Azu as a finger does: a real click, then 0.65 s (three taps within 1.2 s are a tickle, not pokes)
  const pokeAzu = async n => { for (let i = 0; i < n; i++) { await p.locator('#ph .azfab').click(); await R('window.__tap=performance.now()'); await until('performance.now()-window.__tap>650', 1500); } };
  const runTo = (test, ms, step) => R(`var t0=gdNow(),r=null;while(gdNow()-t0<${ms}){gdAdvance(${step || 250});if(G.gd.cur&&!G.gd.cur.pull&&(${test})){r=Math.round(G.gd.cur.at);break}}r`);

  // ---------- 1. the two items alone on gdPick (pure, a fake clock) ----------
  // the moment: a first visit 5 min in, on the wall, the opening said at 0-9 s; Ribeira Palace the nearest marker and the point in the middle; the ✦ on the screen
  await fresh();
  const pk = (mod, now, id) => R(`var items=gdItems(),V={visits:1,visit:{n:1,at:0,lastEnd:9000,items:{opening:{shown:1,last:0,end:9000,dismissed:0,acted:0,how:"ignored"}},starts:[0],away:0,ignoredRun:0,breathMul:1,rest:{},fewer:{},heard:false,pokes:0},ever:{items:{opening:{shown:1,done:false,by:"",last:0}}},used:{},met:false},
    C={on:true,breathing:15,limits:{perMinute:2,perVisit:30},tau:120,learn:{ignore:true,dismiss:true},demo:false,showing:null,remark:null,speaking:false,state:null,panel:null,dockFull:false,tour:null,movedAt:null,touchAt:null,cardAt:null,onWall:true,where:"",
      marker:{id:"painting",name:"Ribeira Palace",x:200,y:400,middle:true},gbtn:{name:"the ✦",x:60,y:770,middle:false},central:{id:"painting",name:"Ribeira Palace",x:200,y:400,middle:true,tour:true,min:3},covers:{},stillSince:0,returning:false,pokes:0,idleSince:0,lively:true,solemn:false},
    it=function(id){return items.filter(function(x){return x.id===id})[0]},off=function(){[].forEach.call(arguments,function(id){items.forEach(function(x){if(x.id===id||x.base===id)x.on=false})})},
    vm=function(id,o){V.visit.items[id]=Object.assign({shown:0,last:null,end:null,dismissed:0,acted:0,how:""},o)},em=function(id,o){V.ever.items[id]=Object.assign({shown:0,done:false,by:""},o)};${mod || ''};
    var r=gdPick(items,V,C,${now});[r.item?r.item.id:null,r.why${id ? `,gdWhyNot(items,V,C,${now},"${id}")` : ''}]`);
  /* (round 7) the checks of one item alone rest Fun's items too: the catalog gained a kind */
  const N = 300000, RP = 'inview:painting', FUN7 = 'off("water","cookie","joke","ptjoke","hunt","season");', ONLY_R = 'off("tapmarker","askview","gbhint","tourof");' + FUN7, ONLY_S = 'off("tapmarker","askview","inview","tourof");' + FUN7 + 'C.idleSince=' + (N - 5000) + ';';
  const P = {};
  P.alone = await pk(ONLY_R, N, RP);
  P.still1 = await pk(ONLY_R + 'C.stillSince=' + (N - 1000), N, RP); P.still2 = await pk(ONLY_R + 'C.stillSince=' + (N - 1300), N, RP);
  P.pan = await pk(ONLY_R + 'C.movedAt=' + (N - 500) + ';C.stillSince=C.movedAt', N, RP); P.pan2 = await pk(ONLY_R + 'C.movedAt=' + (N - 1300) + ';C.stillSince=C.movedAt', N, RP);
  ok('What\'s in view, alone: the point in the middle settled, its remark ("Ribeira Palace in the middle", the scheduler\'s own name)', P.alone[0] === RP && /^Ribeira Palace in the middle: Ribeira Palace in the middle, the phone still/.test(P.alone[1]), P.alone);
  ok('...it settles in the middle 1.2 s (still 1.0 s: it waits; 1.3 s: it shows)', P.still1[0] === null && /Ribeira Palace in the middle: the phone still 1\.0 s \(it waits for 1\.2 s\)/.test(P.still1[2]) && P.still2[0] === RP, [P.still1[2], P.still2[0]]);
  ok('...never while panning (moved 0.5 s ago: quiet, the 0.7 s gate; 1.3 s ago and still: it shows)', P.pan[0] === null && /quiet while the phone moves \(and 0\.7 s after\)/.test(P.pan[2]) && P.pan2[0] === RP, [P.pan[2], P.pan2[0]]);
  P.once = await pk(ONLY_R + 'vm("' + RP + '",{shown:1,last:' + (N - 70000) + ',end:' + (N - 66000) + '})', N, RP);
  P.other = await pk(ONLY_R + 'vm("' + RP + '",{shown:1,last:' + (N - 70000) + ',end:' + (N - 66000) + '});C.central={id:"torreao",name:"The Torreão",x:180,y:420,middle:true}', N, 'inview:torreao');
  P.notMid = await pk(ONLY_R + 'C.central={id:"torreao",name:"The Torreão",x:180,y:420,middle:true}', N, RP);
  P.edge = await pk(ONLY_R + 'C.central.middle=false', N, RP);
  ok('...one item a point, once a visit for each point (Ribeira Palace said: not again; The Torreão in the middle: its own remark)', P.once[0] === null && /Ribeira Palace in the middle: shown once this visit/.test(P.once[2]) && P.other[0] === 'inview:torreao', [P.once[2], P.other[0]]);
  ok('...only its own point, and only in the middle (another point in the middle; the nearest point off the middle cell)', /not in the middle \(The Torreão is nearest\)/.test(P.notMid[2]) && /Ribeira Palace is the nearest, not in the middle yet/.test(P.edge[2]), [P.notMid[2], P.edge[2]]);
  P.gap1 = await pk(ONLY_R + 'C.breathing=5;vm("inview:torreao",{shown:1,last:' + (N - 11000) + ',end:' + (N - 6000) + '});V.visit.lastEnd=' + (N - 6000) + ';V.visit.starts=[]', N, RP);
  P.gap2 = await pk(ONLY_R + 'C.breathing=5;vm("inview:torreao",{shown:1,last:' + (N - 13000) + ',end:' + (N - 8000) + '});V.visit.lastEnd=' + (N - 8000) + ';V.visit.starts=[]', N, RP);
  ok('...the group\'s gap 7 s, from the end of the last remark (breathing room set to 5 s: 6 s after, 1 s left; 8 s after, it shows: the group\'s gap stands in for Content\'s 60 s)', P.gap1[0] === null && /What's in view's gap 7 s: 1 s left/.test(P.gap1[2]) && P.gap2[0] === RP, [P.gap1[2], P.gap2[0]]);
  const capMod = n => ONLY_R + 'items.forEach(function(x){x.kindCap=40});["torreao","river","square"].forEach(function(id,i){vm("inview:"+id,{shown:i<2?4:' + (n - 8) + ',last:100000,end:104000})})';
  P.cap12 = await pk(capMod(12), N, RP); P.cap11 = await pk(capMod(11), N, RP);
  ok('...at most 12 a visit, the group together (12 shown: quiet; 11: it shows)', P.cap12[0] === null && /at most 12 What's in view a visit: shown 12/.test(P.cap12[2]) && P.cap11[0] === RP, [P.cap12[2], P.cap11[0]]);
  P.gOff = await pk(ONLY_R + 'items.forEach(function(x){if(x.base==="inview")x.groupOn=false})', N, RP);
  ok('...the group\'s On: Off, none of its remarks', P.gOff[0] === null && /Ribeira Palace in the middle: What's in view is Off/.test(P.gOff[2]), P.gOff[2]);
  const lines = await R('var d=S.guidance.items.inview,f=function(n,card){return gdLine(d,false,{point:"X",n:n,card:card})},k=d.cardLine;var a=[f(0),f(1),f(2),f(3),f(0,true)];d.cardLine=false;a.push(f(4,true));d.cardLine=k;a.push(TXT("fab_comment_1").replace("{point}","X"),TXT("fab_comment_2").replace("{point}","X"),TXT("fab_comment_3").replace("{point}","X"),TXT("fab_comment_card").replace("{point}","X"));a');
  ok('...its lines: the 3 in turn (never the same twice in a row), a point with a card its own; Lines per point = the 3 for every point', lines[0] === lines[6] && lines[1] === lines[7] && lines[2] === lines[8] && lines[3] === lines[6] && lines[4] === lines[9] && lines[5] === lines[7], lines);
  const spk = await R('var it=gdItems().filter(function(x){return x.id==="inview:painting"})[0],h=S.gdVisitor.visit.heard,a=gdSpoken(it);S.gdVisitor.visit.heard=true;var b2=gdSpoken(it);S.gdVisitor.visit.heard=h;[a,b2]');
  ok('...spoken as the kind (Content): a bubble until the guide has been heard this visit, then spoken', spk[0] === false && spk[1] === true, spk);

  // The ✦
  P.p3 = await pk(ONLY_S + 'C.pokes=3', N, 'gbhint'); P.p4 = await pk(ONLY_S + 'C.pokes=4', N, 'gbhint');
  P.i24 = await pk(ONLY_S + 'C.idleSince=' + (N - 24000), N, 'gbhint'); P.i25 = await pk(ONLY_S + 'C.idleSince=' + (N - 25000), N, 'gbhint');
  ok('The ✦, alone: after 4 pokes on Azu (3: it waits, and says so; 4: it shows)', P.p3[0] === null && /The ✦: after 4 pokes on Azu \(3 so far\) or 25 s idle \(20 s left\)/.test(P.p3[2]) && P.p4[0] === 'gbhint' && /4 pokes on Azu/.test(P.p4[1]), [P.p3[2], P.p4[1]]);
  ok('...or after 25 s idle (24 s: 1 s left; 25 s: it shows)', P.i24[0] === null && /25 s idle \(1 s left\)/.test(P.i24[2]) && P.i25[0] === 'gbhint' && /idle 25 s/.test(P.i25[1]), [P.i24[2], P.i25[1]]);
  P.calm = await pk(ONLY_S + 'C.pokes=4;C.lively=false', N, 'gbhint'); P.sol = await pk(ONLY_S + 'C.pokes=4;C.solemn=true', N, 'gbhint');
  ok('...Lively only, never on a Solemn point', P.calm[0] === null && /The ✦: Lively only: Azu is Calm/.test(P.calm[2]) && P.sol[0] === null && /The ✦: never on a Solemn point/.test(P.sol[2]), [P.calm[2], P.sol[2]]);
  P.once2 = await pk(ONLY_S + 'C.pokes=8;vm("gbhint",{shown:1,last:100000,end:104000})', N, 'gbhint');
  P.done = await pk(ONLY_S + 'C.pokes=4;em("gbhint",{shown:1,done:true,by:"menu"})', N, 'gbhint');
  ok('...once a visit; done for good once the ✦ menu has been opened', P.once2[0] === null && /The ✦: shown once this visit/.test(P.once2[2]) && P.done[0] === null && /The ✦: done for good \(the visitor opened the ✦ menu\)/.test(P.done[2]), [P.once2[2], P.done[2]]);
  P.card = await pk(ONLY_S + 'C.pokes=4;C.onWall=false;C.where="a card is open";it("askview").on=true', N, 'askview');
  P.noStar = await pk(ONLY_S + 'C.pokes=4;C.gbtn=null', N, 'gbhint');
  ok('...wherever the ✦ shows (on the card past the hero too, where Ask about the view does not), never without it', P.card[0] === 'gbhint' && /not on the wall/.test(P.card[2]) && P.noStar[0] === null && /the ✦ is not on the screen/.test(P.noStar[2]), [P.card[0], P.card[2], P.noStar[2]]);
  const pair = FUN7 + 'off("tapmarker","inview","tourof");it("askview").notBefore=0;it("gbhint").importance=5;C.pokes=4;C.idleSince=0;';
  P.pairA = await pk(pair, 30000, 'gbhint');
  P.pairB = await pk(pair + 'vm("askview",{shown:1,last:20000,end:25000});V.visit.lastEnd=25000;V.visit.starts=[0,20000]', 45000, 'gbhint');
  P.pairC = await pk(pair + 'vm("askview",{shown:1,last:20000,end:25000});V.visit.lastEnd=25000;V.visit.starts=[0,20000]', 65000, 'gbhint');
  ok('...never both it and Ask about the view in the visit\'s first minute: both due, the Editor\'s order (Ask about the view first, though The ✦ scores more); after it, The ✦ waits for the minute; then it may show', P.pairA[0] === 'askview' && /The ✦: one about the ✦ in the visit's first minute: the Ask about the view goes first \(the Editor's order\)/.test(P.pairA[2]) && P.pairB[0] === null && /The ✦: one about the ✦ in the visit's first minute: the Ask about the view showed/.test(P.pairB[2]) && P.pairC[0] === 'gbhint', [P.pairA, P.pairB[2], P.pairC[0]]);

  // ---------- 2. a remark never within the breathing room of a tip, either way; the yield ----------
  P.tipRem = await pk('off("askview","gbhint","tourof");vm("tapmarker",{shown:1,last:' + (N - 14000) + ',end:' + (N - 10000) + '});V.visit.lastEnd=' + (N - 10000) + ';V.visit.starts=[]', N, RP);
  P.tipRem2 = await pk('off("askview","gbhint","tourof");vm("tapmarker",{shown:1,last:' + (N - 20000) + ',end:' + (N - 16000) + '});V.visit.lastEnd=' + (N - 16000) + ';V.visit.starts=[]', N, RP);
  ok('[breathing] a remark never within the breathing room after a tip (10 s after Tap a marker: 5 s left; 16 s: the remark)', P.tipRem[0] === null && /Ribeira Palace in the middle: breathing room 15 s: 5 s left/.test(P.tipRem[2]) && P.tipRem2[0] === RP, [P.tipRem[2], P.tipRem2[0]]);
  P.remTip = await pk('off("askview","gbhint","tourof");vm("' + RP + '",{shown:1,last:' + (N - 14000) + ',end:' + (N - 10000) + '});V.visit.lastEnd=' + (N - 10000) + ';V.visit.starts=[]', N, 'tapmarker');
  ok('...and a tip never within it after a remark (10 s after the remark: Tap a marker waits, 5 s left)', P.remTip[0] === null && /Tap a marker: breathing room 15 s: 5 s left/.test(P.remTip[2]), P.remTip[2]);
  P.yield = await pk('off("askview","gbhint","tourof")', N, RP);
  ok('The yield: Tap a marker and the remark both due as the point settles: the tip shows (it scores more), the remark says why it waits', P.yield[0] === 'tapmarker' && /Ribeira Palace in the middle: it could show, but the Tap a marker goes first \(score \d+\.\d+ against \d+\.\d+\)/.test(P.yield[2]), P.yield);
  // on the page, the visit's clock moved by hand: the tip first, the remark only after the breathing room from the tip's end, and the next tip after it
  await fresh('S.guidance.items.askview.on=false;S.guidance.items.gbhint.on=false;S.guidance.items.tourof.on=false;');
  const yl = await R('var r={},t0=gdNow();while(gdNow()-t0<120000){gdAdvance(250);var c=G.gd.cur;if(!c)continue;if(r.tip==null&&c.item.id==="tapmarker"){r.tip=c.at;r.during=gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"inview:painting")}' +
    'if(r.tip!=null&&r.rem==null&&c.item.base==="inview"){r.rem=c.at;r.tipEnd=gdVisitOf(S.gdVisitor,"tapmarker").end;r.id=c.item.id}if(r.rem!=null&&c.item.id==="tapmarker"&&c.at>r.rem){r.next=c.at;r.remEnd=gdVisitOf(S.gdVisitor,r.id).end;break}}' +
    '[r.tip!=null,r.during,r.rem!=null&&r.tipEnd!=null?Math.round((r.rem-r.tipEnd)/1000):null,r.next!=null&&r.remEnd!=null?Math.round((r.next-r.remEnd)/1000):null,r.id]');
  ok('[breathing] On the page: the tip first, the remark waits while it shows (one at a time) and comes 15 s or more after the tip ended; the next tip 15 s or more after the remark', yl[0] && /one at a time: the Tap a marker is showing/.test(yl[1]) && yl[2] >= 15 && yl[3] >= 15 && yl[4] === RP, yl);

  // ---------- 3. on the page: the remark's look and taps; the pokes and the idle; Tap on Azu; the ✦ menu; the old timers gone ----------
  const toRem = 'var e=gdEM("tapmarker");e.done=true;e.by="card";S.guidance.items.askview.on=false;S.guidance.items.gbhint.on=false;';
  for (const dark of [false, true]) {
    await fresh(`S.dark=${dark};renderAll(true);` + toRem); await runTo('G.gd.cur.item.base==="inview"', 40000); await W(400);
    const l = await R(look);
    ok(`The remark (${dark ? 'dark' : 'light'}): beside Ribeira Palace, its card's own line, 2 lines at most, 7:1, 0 px over words, not over its point, the ✦ or the bar, 44 px, a › to say a tap does more`, l[0] <= 2 && l[1] >= 7 && l[2] === 0 && l[3] === 0 && l[4] === 0 && l[5] >= 44 && l[7] === 0 && l[6] === "In the middle: Ribeira Palace. Tap and I'll show you around." && await R('ph.querySelector(".gdbub").classList.contains("gd-more")&&/›/.test(getComputedStyle(ph.querySelector(".gdbub button"),"::after").content)'), l);
  }
  await R('S.dark=false;S.lang="pt";renderAll(true)'); await W(250); const lpt = await R(look);
  ok('...in Portuguese: its words from Languages & Terms, 2 lines at most', lpt[6] === await R('S.tx.fab_comment_card.pt.replace("{point}","Paço da Ribeira")') && lpt[0] <= 2 && lpt[2] === 0, lpt);
  await R('S.lang="en";renderAll(true)'); await W(150);
  await p.locator('#ph .gdbub button[data-gdbub]').click(); await W(400);
  const tapC = await R('[G.on,S.stop,gdEverOf(S.gdVisitor,"inview:painting").acted,!G.gd.cur]');
  ok('...a tap on it: a point with a card starts its tour (at Full), acted', tapC[0] === true && tapC[1] === 'full' && tapC[2] === 1 && tapC[3], tapC);
  await R('stopTour()');
  // a point without a card on the Belém wall: the 3 lines in turn; a tap opens Ask about the view about it
  await fresh(toRem + 'setWall("belem");S.camX=.35;S.camY=.45;S.camZoom=1.3;S.stop="closed";renderPhone(false);G.gd.movedAt=gdNow();');
  await runTo('G.gd.cur.item.base==="inview"', 40000); await W(300);
  const pl = await R('var c=G.gd.cur;[c.item.id,c.text,gdCentral().name,nameIn(TXT("fab_comment_1"),"{point}",gdCentral().name)]');
  await p.locator('#ph .gdbub button[data-gdbub]').click(); await W(400);
  const tapP2 = await R(`[G.cam.open,!!ph.querySelector(".camask.open"),ph.querySelectorAll(".cpin.lit").length,gdEverOf(S.gdVisitor,"${pl[0]}").acted,S.gdVisitor.used.view!=null]`);
  ok('...on the Belém wall, a point with no card: the first of the 3 lines; a tap opens Ask about the view about it, the point lit, acted', pl[1] === pl[3] && tapP2[0] && tapP2[1] && tapP2[2] >= 1 && tapP2[3] === 1 && tapP2[4], [pl, tapP2]);
  await R('stopSpeech();G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};setWall("ribeira");renderPhone(false)');
  // spoken once the guide has been heard this visit, the word being said lit
  await fresh(toRem + 'S.gdVisitor.visit.heard=true;'); await runTo('G.gd.cur.item.base==="inview"', 40000);
  const sp = await R('var c=G.gd.cur;[c.spoken,!!(G.timer||G.utter),ph.querySelectorAll(".gdbub .gdw").length>5,G.gd.log.filter(function(l){return l.what==="shown"}).slice(-1)[0].why]');
  const lit = await until('!!ph.querySelector(".gdbub .gdw.on")', 3000);
  ok('...once the guide has been heard this visit: spoken (the one order of voices), the word being said lit', sp[0] === true && sp[1] && sp[2] && /· spoken$/.test(sp[3]) && lit, [sp, lit]);
  await R('gdStop("test");stopSpeech()');

  // the pokes: real clicks on Azu beside the ✦ (the clock at ×1); The ✦ waits for the finger and for Azu's joke, then shows
  await fresh(toRem.replace('S.guidance.items.gbhint.on=false;', '') + 'gdGroupById("inview").on=false;G.pokes=0;', false);
  await R('G.taps=[]'); await pokeAzu(3);
  const p3 = await R('[S.gdVisitor.visit.pokes,gdVisitOf(S.gdVisitor,"gbhint").shown]');
  await pokeAzu(1);
  const got = await until('gdIs("gbhint")', 8000); await W(400);
  const st = await R(look); const arm = await R('[ph.querySelector(".azfab").classList.contains("point-gb"),G.gd.cur&&G.gd.cur.why,S.gdVisitor.visit.pokes]');
  ok('The ✦ after 4 pokes (real clicks on Azu): not after 3; after the 4th, Azu lifts its arm to the ✦ and says it beside it: 7:1, 0 px over the ✦, words or bar, 44 px', p3[0] === 3 && p3[1] === 0 && got && arm[0] && /4 pokes on Azu/.test(arm[1]) && st[6] === 'Need anything? Tap ✦.' && st[1] >= 7 && st[2] === 0 && st[3] === 0 && st[4] === 0 && st[5] >= 44, [p3, got, arm, st]);
  await until('!G.gd.cur', 9000);
  await pokeAzu(4);
  const again = await until('gdIs("gbhint")', 2000);
  ok('...once a visit: four more pokes, not again', !again && await R('/The ✦: shown once this visit/.test(gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{touchAt:null,remark:null,showing:null}),gdNow(),"gbhint"))'), again);
  // the idle: the visit's clock moved by hand, no touch and the phone still
  await fresh(toRem.replace('S.guidance.items.gbhint.on=false;', '') + 'gdGroupById("inview").on=false;G.gd.actAt=gdNow();');
  const id1 = await R('gdAdvance(23500);[!!(G.gd.cur&&G.gd.cur.item.id==="gbhint")]');
  const id2 = await R('gdAdvance(1750);[!!(G.gd.cur&&G.gd.cur.item.id==="gbhint"),G.gd.cur&&G.gd.cur.why]');
  ok('...or after 25 s idle: nothing at 23.5 s, The ✦ at 25 s (the visit\'s clock)', !id1[0] && id2[0] && /idle 25 s/.test(id2[1]), [id1, id2]);
  await fresh(toRem.replace('S.guidance.items.gbhint.on=false;', '') + 'gdGroupById("inview").on=false;G.gd.actAt=gdNow();');
  const id3 = await R('gdAdvance(20000);ph.dispatchEvent(new PointerEvent("pointerdown",{bubbles:true}));ph.dispatchEvent(new PointerEvent("pointerup",{bubbles:true}));gdAdvance(15000);var a=!!(G.gd.cur&&G.gd.cur.item.id==="gbhint");emit("phone.moved");gdAdvance(15000);var b2=!!(G.gd.cur&&G.gd.cur.item.id==="gbhint");gdAdvance(11000);[a,b2,!!(G.gd.cur&&G.gd.cur.item.id==="gbhint")]');
  ok('...a touch or the phone moving starts the idle again (20 s, a touch, 15 s: no; moved, 15 s: no; 26 s still: yes)', !id3[0] && !id3[1] && id3[2], id3);
  // Tap on Azu = Points to the ✦: at once, a pull (no memory), even over a tip showing (one at a time: the tip gives way)
  await fresh(toRem.replace('S.guidance.items.gbhint.on=false;', '') + 'gdGroupById("inview").on=false;S.guideCfg.azuTap="point";', false);
  await R('gdStart(gdItems().filter(function(x){return x.id==="askview"})[0],"test")'); await W(200);
  await p.locator('#ph .azfab').click(); await W(300);
  const pull = await R('var c=G.gd.cur;[c&&c.item.id,c&&c.pull,(ph.querySelector(".gdbub:not(.gd-out) button")||{}).textContent,ph.querySelector(".azfab").classList.contains("point-gb"),gdVisitOf(S.gdVisitor,"gbhint").shown,G.gd.log.filter(function(l){return l.what==="ended"}).slice(-1)[0].why,S.gdVisitor.visit.pokes]');
  ok('Tap on Azu = Points to the ✦: The ✦ at once (a pull: no memory, no poke), Azu pointing; the tip that was showing gives way (one at a time)', pull[0] === 'gbhint' && pull[1] === true && pull[2] === 'Need anything? Tap ✦.' && pull[3] && pull[4] === 0 && /Ask about the view: the visitor asked for The ✦/.test(pull[5]) && pull[6] === 0, pull);
  await R('gdStop("test");S.guideCfg.azuTap="play"');
  // the ✦ menu ends it for the visit (and for good): a click on the ✦ while it shows; a tap on its bubble opens the menu
  await fresh(toRem.replace('S.guidance.items.gbhint.on=false;', '') + 'gdGroupById("inview").on=false;S.gdVisitor.visit.pokes=4;');
  await runTo('G.gd.cur.item.id==="gbhint"', 5000); await W(200);
  await p.locator('#ph .gbtn').click(); await W(250);
  const mn = await R('[!!ph.querySelector(".gbmenu"),!G.gd.cur,gdVM("gbhint").how,gdEverOf(S.gdVisitor,"gbhint").done,gdEverOf(S.gdVisitor,"gbhint").by]');
  await R('G.fab.menu=false;fabLayer();gdNewVisit();S.gdVisitor.visit.pokes=4;gdAdvance(30000)');
  const mn2 = await R('[!!(G.gd.cur&&G.gd.cur.item.id==="gbhint"),gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{showing:null}),gdNow(),"gbhint")]');
  ok('The ✦ menu opened ends it: the tip ends acted, done for good (not in the next visit either)', mn[0] && mn[1] && mn[2] === 'acted' && mn[3] && mn[4] === 'menu' && !mn2[0] && /done for good \(the visitor opened the ✦ menu\)/.test(mn2[1]), [mn, mn2]);
  await fresh(toRem.replace('S.guidance.items.gbhint.on=false;', '') + 'gdGroupById("inview").on=false;S.gdVisitor.visit.pokes=4;');
  await runTo('G.gd.cur.item.id==="gbhint"', 5000); await W(200);
  await p.locator('#ph .gdbub button[data-gdbub]').click(); await W(250);
  const tb = await R('[!!ph.querySelector(".gbmenu"),gdEverOf(S.gdVisitor,"gbhint").acted,gdEverOf(S.gdVisitor,"gbhint").done]');
  ok('...a tap on its bubble opens the ✦ menu: acted, and done', tb[0] && tb[1] === 1 && tb[2], tb);
  await R('G.fab.menu=false;fabLayer()');
  const gone = await R('[typeof gbHint,typeof showFabBub,G.gb.hinted===undefined&&G.gb.lastAct===undefined,Object.keys(G.fab).join(),S.camCfg.comments===undefined&&S.camCfg.every===undefined,typeof firstAnswer==="function"?extPoint("fabRemark.quiet").length:0,document.querySelectorAll(".azfabbub").length]');
  ok('The old timers are gone: no gbHint, no remark bubble or its 400 ms timer and G.fab counters, no G.gb.hinted, no camCfg.comments / every, no fabRemark.quiet', gone[0] === 'undefined' && gone[1] === 'undefined' && gone[2] && gone[3] === 'menu' && gone[4] && gone[5] === 0 && gone[6] === 0, gone);

  // ---------- 4. the Editor: the link row, the new rows' demos, Why not now? and the Log, the words ----------
  await fresh(); await R('S.tab="guide";S.ui.level=1;S.op={gt3:true,s7ask:true,s7cam:true};renderEditor()'); await W(200);
  const gp = await R('gdGroupPath("inview")');
  const lr = await R(`var r=rowEl(ed,"s7cam","Azu comments");[!!r,!!r&&!!r.querySelector('[data-set="@${gp}on"]'),!!r&&/Said by the guide: Guide › Azu speaks up › Content › What's in view/.test((r.querySelector("[data-goto]")||{}).textContent||""),!rowEl(ed,"s7cam","Between comments")]`);
  await p.locator(`[data-fo="s7cam"] [data-set="@${gp}on"][data-v="false"]`).click(); await W(150);
  const off1 = await R('gdGroupById("inview").on');
  await p.locator(`[data-fo="s7cam"] [data-set="@${gp}on"][data-v="true"]`).click(); await W(150);
  const on1 = await R('gdGroupById("inview").on');
  await p.locator('[data-fo="s7cam"] [data-goto="gd:g:inview"]').click(); await W(400);
  const went = await R('var g=ed.querySelector(\'[data-fo="gd:g:inview"]\');[S.tab,!!g&&g.open,!!g&&g.getBoundingClientRect().top<window.innerHeight&&g.getBoundingClientRect().bottom>0]');
  ok('Ask Azu › Ask about the view › Azu comments: one link row; its On / Off is the group\'s On; its button opens Guide › 4 · Azu speaks up › Content › What\'s in view (Between comments is the group\'s Gap now)', lr.every(Boolean) && off1 === false && on1 === true && went[0] === 'guide' && went[1] && went[2], [lr, off1, on1, went]);
  await R(base + 'G.gd.manual=false;G.gd.rate=1;gdResetVisitor();S.tab="guide";S.ui.level=9;S.op={};renderEditor()');
  const defs = await R('var g=gdGroupById("inview"),d=S.guidance.items.inview,h=S.guidance.items.gbhint;[[g.on,g.cap,g.gap,g.weight,g.items.join()],[d.kind,d.group,d.shows,d.stillFor,d.cardLine,d.tap,d.importance,d.says.join(),d.saysCard],[h.kind,h.group,h.shows,h.pokes,h.idleFor,h.lively,h.notSolemn,h.doneWhen,h.says,h.screen],gdGroupById("wall").items.join(),S.guidance.kinds.map(function(k){return k.groups.filter(function(x){return x.id!=="card"&&x.id!=="howto"&&x.id!=="pages"&&x.id!=="guide"}).map(function(x){return x.id+":"+x.on+":"+x.cap+":"+x.gap}).join()}).join("|")]');
  ok('The new rows\' defaults: What\'s in view On, at most 12 a visit, gap 7 s; {point} in the middle once a visit for each point, still 1.2 s, a card its own line; The ✦ once a visit, 4 pokes or 25 s idle, Lively only, never Solemn, done when the ✦ menu opens; every other group On with no cap or gap of its own (round 7: Fun; round 12: On the tour where Moments were, its own rhythm and no cap or gap)',
    JSON.stringify(defs) === JSON.stringify([[true, 12, 7, 1, 'inview'], ['content', 'inview', 'visit', 1.2, true, 'remark', 2, 'fab_comment_1,fab_comment_2,fab_comment_3', 'fab_comment_card'], ['app', 'wall', 'visit', 4, 25, true, true, 'menu', 'gb_hint', 'any'], 'askview,gbhint', 'start:true:0:0,wall:true:0:0|inview:true:12:7,tours:true:0:0|ontour:true:0:0,play:true:0:0']), defs);
  const newRows = await R('var o=[];ed.querySelectorAll(\'[data-fo="gt4"] .row,[data-fo="gd:g:ontour"] .row\').forEach(function(r){var k=rowKey(r);if(k&&o.indexOf(k)<0&&k.indexOf(gdGroupPath("card"))!==0&&k.indexOf(gdGroupPath("howto"))!==0&&k.indexOf(gdGroupPath("pages"))!==0&&/^guidance\\.(kinds\\.\\d+\\.groups\\.\\d+\\.(on|cap|gap)|items\\.(inview|gbhint)\\.)/.test(k))o.push(k)});o');
  ok(`every new row has its demo (${newRows.length} rows: the 4 groups' On, At most a visit and Gap; {point} in the middle's and The ✦'s, Lines per point, After pokes and After idle among them; round 6's full item form gave each item 32 rows; round 7 adds Fun's 2 groups' 6 and The ✦'s Or a pause late; round 9 The ✦'s In Things to try; round 12: On the tour has no At most a visit or Gap rows of a group, its own rhythm instead)`, newRows.length === 82 && newRows.indexOf('guidance.items.inview.cardLine') >= 0 && newRows.indexOf('guidance.items.gbhint.pokes') >= 0 && newRows.indexOf('guidance.items.gbhint.idleFor') >= 0 && (await R(`${JSON.stringify(newRows)}.filter(function(k){return!rowDemoOf(k)})`)).length === 0, newRows.length);
  const seen = {}, missing = [];
  const CHIP_RE = { [gp + 'gap']: /×10 · What's in view's gap: \d+ s/, 'guidance.items.gbhint.pokes': /A poke on Azu: 4 of 4/, 'guidance.items.gbhint.idleFor': /×10 · Idle: no touch, the phone still: \d+ s/, 'guidance.items.inview.cardLine': /Lines per point: a point with a card says its own line/, [gp + 'cap']: /What's in view: at most 12 a visit/ };
  for (const k of newRows) {
    await R('document.body.dataset.rowdemo="";window.__chips=[];clearInterval(window.__chI);window.__chI=setInterval(function(){var c=ph.querySelector(".gdchip");if(c&&window.__chips.indexOf(c.textContent)<0)window.__chips.push(c.textContent)},50)');
    const lb = await R(`var c=ed.querySelector('[data-fo="gt4"] .row [data-set="@${k}"],[data-fo="gt4"] .row [data-num="@${k}"],[data-fo="gd:g:ontour"] .row [data-set="@${k}"]'),l=c&&c.closest(".row").querySelector("label.rd");if(l){l.scrollIntoView({block:"center"});var r=l.getBoundingClientRect();[r.left+12,r.top+r.height/2]}else null`);
    if (!lb) await R(`showRowDemo("${k}")`); else await p.mouse.click(lb[0], lb[1]);
    const okd = await until('/\\|found\\|/.test(document.body.dataset.rowdemo||"")', 5000);
    if (!okd) missing.push(k + ' ' + await R('document.body.dataset.rowdemo||""'));
    if (CHIP_RE[k]) await until(`window.__chips.some(function(c){return ${CHIP_RE[k]}.test(c)})`, 9000);
    seen[k] = await R('window.__chips.slice()');
  }
  await R('clearInterval(window.__chI)');
  ok('...a press on each new row shows its demo on the phone: the tip (or its chip) ringed', missing.length === 0, { missing, shown: newRows.length - missing.length });
  ok('...the group\'s gap on the ×10 clock with its countdown chip; the cap; Lines per point; The ✦\'s pokes (Azu poked, then the tip) and idle (the countdown)', Object.keys(CHIP_RE).every(k => (seen[k] || []).some(c => CHIP_RE[k].test(c))), Object.keys(CHIP_RE).map(k => [k, seen[k]]));
  const rb = await R('[gdReadBack(S.guidance.items.inview),gdReadBack(S.guidance.items.gbhint),ed.querySelector(\'[data-fo="gd:i:inview"] .gdpts\').textContent]');
  ok('...the read-backs in words: once a visit for each point, the group together at most 12 a visit and 7 s apart; after 4 pokes or 25 s idle, Lively only, never Solemn, until the ✦ menu; the points on the wall now', /when its point settles in the middle, the phone still 1\.2 s · once a visit for each point · What's in view together: at most 12 a visit, at least 7 s apart/.test(rb[0]) && /after 4 pokes on Azu or after 25 s idle \(no touch, the phone still\), Lively only, never on a Solemn point · once a visit, until the visitor opens the ✦ menu/.test(rb[1]) && /6 points\): Ribeira Palace \(a card: its own line\), The Torreão/.test(rb[2]) && !/later round/i.test(await R('ed.querySelector(\'[data-fo="gt4"]\').textContent')), rb);
  // Why not now? and the Log name them
  await fresh(toRem); await R('S.tab="guide";S.ui.level=1;S.op={gt0:true};renderEditor()'); await W(150);
  const opts = await R('[].map.call(ed.querySelectorAll("[data-gdwhysel] option"),function(o){return o.textContent})');
  await R('var s=ed.querySelector("[data-gdwhysel]");s.value="inview:torreao";s.dispatchEvent(new Event("change",{bubbles:true}))'); await W(100);
  await p.locator('[data-gdwhynot]').click(); await W(100);
  const wn = await R('[ed.querySelector("#gdwhynot").textContent,gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"inview:torreao")]');
  await runTo('G.gd.cur.item.base==="inview"', 40000); await R('gdAdvance(10000);gdReadouts()'); await W(150);
  const lg = await R('[].map.call(ed.querySelectorAll("#gdlog .gdl"),function(x){return x.textContent}).slice(0,8)');
  ok('Why not now? and the Log name them: each point\'s remark by its name ("The Torreão in the middle: not in the middle"), its pick and end in the Log', opts.indexOf('Ribeira Palace in the middle') >= 0 && opts.indexOf('The Torreão in the middle') >= 0 && opts.indexOf('The ✦') >= 0 && wn[0] === wn[1] && /^The Torreão in the middle: not in the middle \(Ribeira Palace is nearest\)/.test(wn[0]) && lg.some(x => /shownRibeira Palace in the middle: Ribeira Palace in the middle/.test(x)) && lg.some(x => /endedRibeira Palace in the middle: its time is up/.test(x)), [opts, wn[0], lg.slice(0, 4)]);
  const words = await R('[TXOWN.fab_comment_1,TXOWN.fab_comment_2,TXOWN.fab_comment_3,TXOWN.fab_comment_card,TXOWN.gb_hint,["fab_comment_1","fab_comment_2","fab_comment_3","fab_comment_card","gb_hint"].every(function(k){return S.tx[k].en.length>8&&S.tx[k].pt.length>8&&/\\{(point|button)\\}/.test(S.tx[k].pt)}),ownerName("gd:i:inview"),ownerName("gd:i:gbhint")]');
  ok('The words in Languages & Terms, owned by their item\'s foldout, en and pt, with their slots ({point}, {button})', words.slice(0, 4).every(w => w === 'gd:i:inview') && words[4] === 'gd:i:gbhint' && words[5] && /What's in view › \{point\} in the middle/.test(words[6]) && /The wall › The ✦/.test(words[7]), words);

  // ---------- 5. the two new scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^gd3_/.test(x[1])})');
  ok('The console lists round 3\'s two guidance scenes (a remark yields, The ✦ after four pokes)', scenes.length === 2 && JSON.stringify(scenes.map(s => s[1])) === JSON.stringify(['gd3_yield', 'gd3_pokes']), scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    const does = await R('ed.querySelector(".agscenes").textContent');
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 60000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`${id === 'gd3_yield' ? '[breathing] ' : ''}Scene "${label}": Set up and ask runs it, ✓`, /Does: nothing:/.test(does) && r && r.ok === true, r && [r.note, r.answer]);
    await R('gdStop("test");G.gd.rate=1;G.gd.manual=false');
  }
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
