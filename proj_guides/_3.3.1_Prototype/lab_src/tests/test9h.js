// h4 checks (Playwright + the installed Edge): node test9h.js -> PASS/FAIL per check
// Guidance, round 4 (_3.8 section 8): the card's Hints become App › The card (cues) and How to use the card becomes App › How to use the
// card (a walkthrough, pulled only); the ✦ menu gets How does this work?.
// 1. Each cue's rules alone on gdPick (pure, a fake clock): the Grabber nudge at the card's opening (past the touch and the card's first
// 1.5 s, at Half only), the Locate label (once a visit / every card), the Navigation Bar, Before / After, Swipe strips, Underlined words
// (at a pause, their element in view; the gates: a touch, a scroll, the card's first 1.5 s, a panel, a tour; Reduce Motion skips the
// moving ones), not on the wall, the group's On, Done when (a step of How to use the card seen), a cue is not a tip (the tips' limits);
// the walkthrough's steps never the scheduler's. 2. A cue and a tip never at once [at once], and the cue room between them, either way
// [room] (pure and on the page). 3. How to use the card pulled from Settings and from the ✦ (real clicks), no gate stopping it; a step seen
// retires its cue. 4. How does this work? on the card and on the wall (its look, its chip). 5. Card Controls > Hints as a view: it reads
// Edited in Guidance and its links open the group and the cue; the old names (S.hints.on, S.ctl.locate.label) are Guidance's values.
// 6. Every new row's demo, the words and their owners, the old timers gone. 7. The two new scenes in Ask Azu's console.
// MUTATE=1 node test9h.js runs it with the cues past the one-at-a-time gate: the checks named [at once] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url, { timeout: 90000 }); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(Math.min(ms, 500));
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('var __gp=gdPick;gdPick=function(items,v,ctx,now){if(ctx.showing){var r=__gp(items.filter(function(x){return x.form==="cue"}),v,Object.assign({},ctx,{showing:null}),now);if(r.item)return r}return __gp(items,v,ctx,now)}'); console.log('MUTATION: the cues skip the one-at-a-time gate'); }
  // the set-up: the cues On (and The ✦), the remarks resting, a first visit whose opening was said 30 s ago
  const base = 'S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.guideCfg.gbtn="both";S.guideCfg.mascot=true;S.guideCfg.life="lively";S.guideCfg.azuTap="play";S.m.solemn=false;' +
    'S.guidance.on=true;S.guidance.breathing=15;S.guidance.cueRoom=5;S.hints.on=true;gdGroupById("card").items.forEach(function(id){var it=S.guidance.items[id];it.on=true;it.shows="visit"});var hw=gdGroupById("howto");hw.on=true;hw.inSettings=true;hw.inMenu=true;' +
    '["opening","tapmarker","askview","gbhint","tourof"].forEach(function(id){S.guidance.items[id].on=true});gdGroupById("inview").on=false;G.gd.demo=false;gdChip(null);';
  const opened = 'var o=gdVM("opening"),oe=gdEM("opening");o.shown=1;o.last=gdNow()-30000;o.end=gdNow()-21000;oe.shown=1;S.gdVisitor.visit.lastEnd=gdNow()-21000;var e=gdEM("tapmarker");e.done=true;e.by="card";';
  const fresh = async (extra, manual) => { await R(base + 'if(G.on)stopTour();G.cam.open=false;S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);' + `G.gd.manual=${!!manual};G.gd.rate=1;` + 'gdStop("test");gdResetVisitor();G.gd.movedAt=null;G.gd.touchAt=null;G.gd.actAt=null;G.gd.scrollAt=null;' + opened + (extra || '')); await W(150);
    /* (round 11 review) the sheet may still be growing to its stop (184 px of 820 measured): wait until its height holds */
    await until('var h=Math.round(scr.getBoundingClientRect().height),o=window.__t9sh;window.__t9sh=h;o===h', 3000); };
  const CR = 'var px=function(c){var m=c.match(/[\\d.]+/g).map(Number);return{r:m[0],g:m[1],b:m[2]}},lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},cr=function(a,b){var x=lum(a),y=lum(b);return Math.round((Math.max(x,y)+.05)/(Math.min(x,y)+.05)*100)/100};';
  const look = CR + 'var q=ph.getBoundingClientRect().height/ph.offsetHeight,b=ph.querySelector(".gdbub:not(.gd-out)"),bt=b.querySelector("button[data-gdbub]"),cs=getComputedStyle(bt),lh=parseFloat(cs.lineHeight),lines=Math.round((bt.offsetHeight-parseFloat(cs.paddingTop)-parseFloat(cs.paddingBottom))/lh),B=gdPR(b),g=ph.querySelector(".gbtn"),ct=b.querySelector("[data-gdcta]");' +
    '[lines,cr(px(cs.color),px(getComputedStyle(b).backgroundColor)),gdOverWords(),g?gdOver(B,gdPR(g)):0,bt.textContent,ct?Math.round(ct.getBoundingClientRect().height/q):0,ct?cr(px(getComputedStyle(ct).color),px(getComputedStyle(ct).backgroundColor)):0,ct?ct.textContent:""]';

  // ---------- 1. each cue's rules alone on gdPick (pure, a fake clock) ----------
  await fresh();
  const pk = (mod, now, id) => R(`var items=gdItems(),V={visits:1,visit:{n:1,at:0,lastEnd:null,cueEnd:null,cards:1,items:{},starts:[],away:0,ignoredRun:0,breathMul:1,rest:{},fewer:{},heard:false,pokes:0},ever:{items:{}},used:{card:1000},met:false},
    C={on:true,breathing:15,cueRoom:5,limits:{perMinute:2,perVisit:30},tau:120,learn:{ignore:true,dismiss:true},demo:false,showing:null,remark:null,speaking:false,state:null,panel:null,dockFull:false,tour:null,movedAt:null,touchAt:null,scrollAt:null,cardAt:null,
      onWall:false,where:"a card is open",onCard:true,reduce:false,moment:null,cues:{},marker:null,gbtn:null,central:null,covers:{},stillSince:0,returning:false,pokes:0,idleSince:null,lively:true,solemn:false},
    it=function(id){return items.filter(function(x){return x.id===id})[0]},only=function(){var k=[].slice.call(arguments);items.forEach(function(x){x.on=k.indexOf(x.base||x.id)>=0})},
    vm=function(id,o){V.visit.items[id]=Object.assign({shown:0,last:null,end:null,dismissed:0,acted:0,how:""},o)},em=function(id,o){V.ever.items[id]=Object.assign({shown:0,done:false,by:""},o)},
    star=function(){C.gbtn={name:"the ✦",x:60,y:770,middle:false};C.pokes=4};${mod || ''};
    var r=gdPick(items,V,C,${now});[r.item?r.item.id:null,r.why${id ? `,gdWhyNot(items,V,C,${now},"${id}")` : ''}]`);
  const N = 300000, GRAB = 'C.cues={grab:"the card opened at Half",locate:"the Locate Button on the screen"};', OPEN = 'C.moment="open";' + GRAB;
  const P = {};
  P.n1 = await pk('only("nudge");' + OPEN + 'C.touchAt=' + (N - 100) + ';C.cardAt=' + (N - 200), N, 'nudge');
  P.n2 = await pk('only("nudge");C.cues={locate:"x"}', N, 'nudge');
  P.n3 = await pk('only("nudge");' + OPEN + 'C.reduce=true', N, 'nudge');
  P.n4 = await pk('only("nudge");' + OPEN + 'vm("nudge",{shown:1,last:100000,end:101000})', N, 'nudge');
  P.n5 = await pk('only("nudge");' + OPEN + 'em("nudge",{shown:1,done:true,by:"wk_grab"})', N, 'nudge');
  ok('Grabber nudge: at the card\'s opening at Half, past the touch that opened it (0.1 s) and the card\'s first 1.5 s', P.n1[0] === 'nudge' && /^Grabber nudge: the card opened at Half/.test(P.n1[1]), P.n1);
  ok('...only as the card opens (later: it waits for nothing, it says so); skipped under Reduce Motion; once a visit; done for good once its step of How to use the card is seen',
    P.n2[0] === null && /Grabber nudge: the grabber nudges only as the card opens at Half/.test(P.n2[2]) && /Grabber nudge: skipped under Reduce Motion \(a moving cue\)/.test(P.n3[2]) && /Grabber nudge: shown once this visit/.test(P.n4[2]) &&
    /Grabber nudge: done for good \(the visitor saw How to use the card's step “The grabber”\)/.test(P.n5[2]), [P.n2[2], P.n3[2], P.n4[2], P.n5[2]]);
  P.o1 = await pk('only("nudge","gbhint");' + OPEN + 'star()', N, 'gbhint');
  ok('...the opening is the cues\' moment: a tip waits (only a cue that answers the opening shows then)', P.o1[0] === 'nudge' && /The ✦: the card is opening: only a cue that answers the opening shows now/.test(P.o1[2]), P.o1[2]);
  P.l1 = await pk('only("label");' + OPEN, N, 'label'); P.l2 = await pk('only("label");C.cues={locate:"the Locate Button on the screen"}', N, 'label');
  P.l3 = await pk('only("label");C.cues={locate:"x"};it("label").shows="card";vm("label",{shown:1,card:1,last:100000,end:104000})', N, 'label');
  P.l4 = await pk('only("label");C.cues={locate:"x"};it("label").shows="card";vm("label",{shown:1,card:1,last:100000,end:104000});V.visit.cards=2', N, 'label');
  P.l5 = await pk('only("label");C.cues={}', N, 'label');
  ok('Locate label: as the card opens, or at the next pause; Every card: not again on this card, again on the next; never without the Locate Button',
    P.l1[0] === 'label' && P.l2[0] === 'label' && /Locate label: shown on this card \(again on the next\)/.test(P.l3[2]) && P.l4[0] === 'label' && /Locate label: no Locate Button on the screen/.test(P.l5[2]), [P.l1[0], P.l2[0], P.l3[2], P.l4[0], P.l5[2]]);
  const NB = 'only("navbar");C.cues={navbar:"the Navigation Bar shows"};';
  P.b1 = await pk(NB, N, 'navbar'); P.b2 = await pk(NB + 'C.touchAt=' + (N - 500), N, 'navbar'); P.b3 = await pk(NB + 'C.scrollAt=' + (N - 500), N, 'navbar'); P.b3b = await pk(NB + 'C.scrollAt=' + (N - 900), N, 'navbar');
  P.b4 = await pk(NB + 'C.cardAt=' + (N - 1000), N, 'navbar'); P.b5 = await pk(NB + 'C.panel="Settings"', N, 'navbar'); P.b6 = await pk(NB + 'C.tour={between:true}', N, 'navbar'); P.b7 = await pk('only("navbar");C.cues={}', N, 'navbar');
  ok('Navigation Bar: at a pause when the bar shows; quiet after a touch (0.8 s), while the visitor scrolls (0.8 s), in the card\'s first 1.5 s, with Settings open, in a tour; never without the bar',
    P.b1[0] === 'navbar' && /the Navigation Bar shows/.test(P.b1[1]) && /a touch 0\.5 s ago/.test(P.b2[2]) && /quiet while the visitor scrolls \(and 0\.8 s after\)/.test(P.b3[2]) && P.b3b[0] === 'navbar' && /card's first 1\.5 s/.test(P.b4[2]) &&
    /quiet while Settings is open/.test(P.b5[2]) && /Navigation Bar: quiet in a tour \(During a tour: Never\)/.test(P.b6[2]) && /the Navigation Bar is not showing/.test(P.b7[2]), [P.b1[1], P.b2[2], P.b3[2], P.b4[2], P.b5[2], P.b6[2], P.b7[2]]);
  const ALL = 'C.cues={ba:"a Before / After in view",strip:"a strip with more to the side in view",term:"an underlined word in view"};';
  P.r1 = await pk('only("ba");' + ALL + 'C.reduce=true', N, 'ba'); P.r2 = await pk('only("strip");' + ALL + 'C.reduce=true', N, 'strip'); P.r3 = await pk('only("term");' + ALL + 'C.reduce=true', N, 'term');
  P.r4 = await pk('only("ba");' + ALL, N, 'ba'); P.r5 = await pk('only("strip");' + ALL, N, 'strip'); P.r6 = await pk('only("ba");C.cues={}', N, 'ba');
  ok('Before / After and Swipe strips: at a pause with their element in view, skipped under Reduce Motion; Underlined words still shows (it glows, no travel)',
    P.r4[0] === 'ba' && P.r5[0] === 'strip' && /Before \/ After: skipped under Reduce Motion/.test(P.r1[2]) && /Swipe strips: skipped under Reduce Motion/.test(P.r2[2]) && P.r3[0] === 'term' && /no Before \/ After in view/.test(P.r6[2]), [P.r1[2], P.r2[2], P.r3[0], P.r6[2]]);
  P.w1 = await pk(NB + 'C.onCard=false;C.onWall=true;C.where=""', N, 'navbar');
  P.g1 = await pk(NB + 'items.forEach(function(x){if(x.group==="card")x.groupOn=false})', N, 'navbar');
  ok('...never on the wall; the group\'s On (S.hints.on): Off, no cue', /Navigation Bar: not on a card \(the wall\)/.test(P.w1[2]) && /Navigation Bar: The card \(cues\) is Off/.test(P.g1[2]), [P.w1[2], P.g1[2]]);
  P.t1 = await pk(NB + 'C.limits.perVisit=1;vm("askview",{shown:1,last:100000,end:104000})', N, 'navbar');
  P.t2 = await pk('only("gbhint");star();C.limits.perVisit=1;vm("navbar",{shown:1,last:100000,end:101000});vm("ba",{shown:1,last:100000,end:101000})', N, 'gbhint');
  ok('A cue is not a tip: the tips\' limits do not hold it (at most 1 a visit, a tip shown: the cue still shows), and the cues shown do not count in them', P.t1[0] === 'navbar' && P.t2[0] === 'gbhint', [P.t1, P.t2[0]]);
  const steps = await R('var d=gdDefs().filter(function(x){return x.form==="walk"}),V=JSON.parse(JSON.stringify(S.gdVisitor)),c=gdContext();[gdItems().filter(function(x){return x.form==="walk"}).length,d.map(function(x){return x.id}).join(),gdCheck(d[0],gdItems(),V,c,gdNow()).no]');
  ok('The walkthrough\'s steps are never the scheduler\'s: not in its items, and pulled only if asked', steps[0] === 0 && steps[1] === 'wk_grab,wk_settings,wk_bar,wk_locate,wk_block' && /pulled only/.test(steps[2]), steps);

  // ---------- 2. a cue and a tip never at once; the cue room, either way ----------
  const BOTH = 'only("navbar","gbhint");C.cues={navbar:"the Navigation Bar shows"};star();';
  P.a1 = await pk(BOTH + 'C.showing="The ✦"', N, 'navbar'); P.a2 = await pk(BOTH + 'C.showing="Navigation Bar"', N, 'gbhint'); P.a3 = await pk(BOTH, N, 'navbar');
  ok('[at once] A tip showing: the cue waits (one at a time: The ✦ is showing); a cue showing: the tip waits', P.a1[0] === null && /Navigation Bar: one at a time: The ✦ is showing/.test(P.a1[2]) && P.a2[0] === null && /The ✦: one at a time: the Navigation Bar is showing/.test(P.a2[2]), [P.a1, P.a2[2]]);
  ok('...both due and nothing showing: one of them (the score, then the Editor\'s order), the other says why it waits', P.a3[0] === 'gbhint' && /Navigation Bar: it could show, but the The ✦ goes first/.test(P.a3[2]), P.a3);
  P.m1 = await pk(NB + 'V.visit.lastEnd=' + (N - 3000), N, 'navbar'); P.m2 = await pk(NB + 'V.visit.lastEnd=' + (N - 5100), N, 'navbar');
  P.m3 = await pk('only("gbhint");star();V.visit.cueEnd=' + (N - 3000), N, 'gbhint'); P.m4 = await pk('only("gbhint");star();V.visit.cueEnd=' + (N - 5100), N, 'gbhint');
  P.m5 = await pk(NB + 'V.visit.cueEnd=' + (N - 100), N, 'navbar'); P.m6 = await pk(NB + 'C.cueRoom=0;V.visit.lastEnd=' + (N - 1000), N, 'navbar');
  P.m7 = await pk('only("nudge");' + OPEN + 'V.visit.lastEnd=' + (N - 50) + ';V.visit.openEnd=V.visit.lastEnd', N, 'nudge');
  ok('[room] The cue room (5 s) after a tip: 3 s after, the cue waits (2 s left); 5.1 s after, it shows', P.m1[0] === null && /Navigation Bar: the cue room 5 s after a tip: 2 s left/.test(P.m1[2]) && P.m2[0] === 'navbar', [P.m1[2], P.m2[0]]);
  ok('[room] ...and after a cue, a tip waits it too (not the 15 s breathing room: a cue is small motion); 5.1 s after, the tip', P.m3[0] === null && /The ✦: the cue room 5 s after a cue: 2 s left/.test(P.m3[2]) && P.m4[0] === 'gbhint', [P.m3[2], P.m4[0]]);
  ok('[room] ...none between two cues (one at a time only); Cue room 0: none; a tip the card\'s opening ended (the visitor did what it asked) does not hold the opening\'s cue', P.m5[0] === 'navbar' && P.m6[0] === 'navbar' && P.m7[0] === 'nudge', [P.m5[0], P.m6[0], P.m7]);
  // on the page, the visit's clock moved by hand: The ✦'s tip on the card; a Before / After scrolls into view while it shows; the sweep waits, then its room
  await fresh('gdGroupById("card").items.forEach(function(id){S.guidance.items[id].on=id==="ba"});S.view="card";S.stop="full";renderPhone(false);var el=scr.querySelector("[data-ba]"),top=el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop,h=scr.querySelector(".hero").offsetHeight;scr.scrollTop=Math.max(h+40,top-scr.clientHeight-60);fabLayer();S.gdVisitor.visit.pokes=4;', true);
  const pg = await R('var r={both:0,x:[]},t0=gdNow(),ba=null,x0=null;while(gdNow()-t0<60000){gdAdvance(250);var c=G.gd.cur;' +
    'if(c&&c.item.id==="gbhint"&&r.tip==null){r.tip=c.at;var el=scr.querySelector("[data-ba]");scr.scrollTop=el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-140;ba=el;x0=el.style.getPropertyValue("--x")}' +
    'if(c&&c.item.id==="gbhint"&&r.tip!=null&&gdNow()-r.tip>=2000&&!r.during)r.during=gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"ba");' +
    'if(r.tip!=null&&r.end==null&&!(c&&c.item.id==="gbhint")){r.end=gdVisitOf(S.gdVisitor,"gbhint").end;r.after=gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"ba")}' +
    'if(c&&c.cue&&ph.querySelector(".gdbub:not(.gd-out)"))r.both++;if(c&&c.item.id==="ba"){r.cue=c.at;r.moved=ba&&ba.style.getPropertyValue("--x")!==x0;break}}' +
    '[r.tip!=null,r.during,r.after,r.cue!=null&&r.end!=null?Math.round((r.cue-r.end)/100)/10:null,r.both]');
  const moved = await until('var e=scr.querySelector("[data-ba]");e&&e.style.getPropertyValue("--x")!==""&&Math.abs(parseFloat(e.style.getPropertyValue("--x"))-50)>2', 1500);
  ok('[at once] On the page: The ✦ on the card; a Before / After scrolls into view while it shows: the sweep waits (one at a time), never with a bubble on the screen', pg[0] && /one at a time: The ✦ is showing/.test(pg[1] || '') && pg[4] === 0, pg);
  ok('[room] ...then the cue room after the tip, and only then the sweep plays (5 s or more after the tip ended)', /the cue room 5 s after a tip/.test(pg[2] || '') && pg[3] >= 5 && moved, [pg[2], pg[3], moved]);
  await R('G.gd.manual=false');

  // ---------- 3. How to use the card, pulled: from Settings and from the ✦, no gate stopping it; a step seen retires its cue ----------
  await fresh('S.guidance.on=false;S.view="card";S.stop="half";S.settingsOpen=true;renderPhone(false);');
  await R('var v=ph.querySelector(".vset");v.scrollTop=v.scrollHeight'); await W(200);
  await p.locator('#ph .vset .vhow').click(); await W(400);
  const s1 = await R('[G.on,S.guideSteps[G.i].id,S.guideSteps[G.i].gd,!!ph.querySelector("#grab.gfocus"),S.stop,!ph.querySelector(".vset"),S.guidance.on,G.gd.touchAt!=null&&gdNow()-G.gd.touchAt<800]');
  ok('From Settings (a real click), with Guidance Off and a touch just now: the walkthrough starts at once at its first step, the grabber marked, the card at Full', s1[0] && s1[1] === 'how0' && s1[2] === 'wk_grab' && s1[3] && s1[4] === 'full' && s1[5] && s1[6] === false && s1[7], s1);
  const ret = await until('gdEverOf(S.gdVisitor,"nudge").done', 12000);
  const rt = await R('[gdEverOf(S.gdVisitor,"nudge").by,gdEverOf(S.gdVisitor,"wk_grab").shown,gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{on:true,showing:null,tour:null}),gdNow(),"nudge"),G.gd.log.filter(function(l){return l.what==="event"&&/The grabber/.test(l.why)}).map(function(l){return l.why}).join()]');
  ok('A step seen retires its cue: the grabber\'s line heard, the Grabber nudge is done for good (by its step), the step\'s memory "seen"', ret && rt[0] === 'wk_grab' && rt[1] === 1 && /Grabber nudge: done for good \(the visitor saw How to use the card's step “The grabber”\)/.test(rt[2]) && /How to use the card's step “The grabber”/.test(rt[3]), rt);
  await R('stopTour()'); await W(200);
  ok('...stopping it brings back the card\'s own guide steps', await R('!G.on&&S.guideSteps[0].id==="g0"'));
  await fresh('S.guidance.on=false;S.view="card";S.stop="full";renderPhone(false);scr.scrollTop=scr.querySelector(".hero").offsetHeight+60;fabLayer();');
  await until('!!ph.querySelector("#ph .gbtn")||!!ph.querySelector(".gbtn")', 2000);
  await p.locator('#ph .gbtn').click(); await W(250);
  const mi = await R('[].map.call(ph.querySelectorAll(".gbmenu [data-gbgo]"),function(b){return b.dataset.gbgo}).join()');
  await p.locator('#ph [data-gbgo="how"]').click(); await W(400);
  const s2 = await R('[G.on,S.guideSteps[G.i].id,!!ph.querySelector("#grab.gfocus"),!ph.querySelector(".gbmenu")]');
  ok('From the ✦ on the card (real clicks): its menu has How does this work?, which walks the card (Guidance Off: no gate stops a pull)', mi === 'tour,ask,how,try,settings' && s2.every(Boolean), [mi, s2]);
  await R('stopTour()');

  // ---------- 4. How does this work? on the wall: one bubble, beside the nearest marker, a chip that opens a card ----------
  for (const dark of [false, true]) {
    await fresh(`S.dark=${dark};renderAll(true);`); await W(200);
    await p.locator('#ph .gbtn').click(); await W(250);
    const wi = await R('[].map.call(ph.querySelectorAll(".gbmenu [data-gbgo]"),function(b){return b.dataset.gbgo}).join()');
    await p.locator('#ph [data-gbgo="how"]').click(); await until('!!(G.gd.cur&&G.gd.cur.item.id==="howto:wall")', 2000); await W(450);
    /* (round 9) Azu's hop ended before its place is measured: the check read Azu mid-hop under load */
    await until('var a=ph.querySelector(".azfab");!a||!a.getAnimations||!a.getAnimations().length', 2500); const l = await R(look); const az = await R('[!ph.querySelector(".gbmenu"),!!ph.querySelector(".gdring"),G.gd.cur&&G.gd.cur.pull,G.gd.az!=null]');
    ok(`On the wall (${dark ? 'dark' : 'light'}): the ✦ menu has it; one bubble (the menu closed), beside the nearest marker, Azu pointing, its ring; 2 lines, 7:1, 0 px over words or the ✦; the chip Open a card, 44 px, 7:1`,
      wi === 'tour,ask,view,how,try,settings' && az.every(Boolean) && l[0] <= 2 && l[1] >= 7 && l[2] === 0 && l[3] === 0 && l[4] === await R('TXT("gd_how_wall")') && l[5] >= 44 && l[6] >= 7 && /^Open a card/.test(l[7]), [wi, az, l]);
  }
  await R('S.dark=false;S.lang="pt";renderAll(true)'); await W(250); await until('var a=ph.querySelector(".azfab");!a||!a.getAnimations||!a.getAnimations().length', 2500); const lpt = await R(look);
  ok('...in Portuguese: its words from Languages & Terms, 2 lines at most', lpt[4] === await R('S.tx.gd_how_wall.pt') && /^Abrir um cartão/.test(lpt[7]) && lpt[0] <= 2 && lpt[2] === 0, lpt);
  await R('S.lang="en";renderAll(true)'); await W(150);
  await p.locator('#ph .gdbub [data-gdcta]').click(); await W(400);
  const op = await R('[S.view,S.stop,!G.gd.cur||G.gd.cur.item.id!=="howto:wall",S.gdVisitor.used.card!=null,JSON.stringify(gdVisitOf(S.gdVisitor,"howto:wall"))]');
  ok('...its chip opens the card of the marker Azu points at (a pull: no memory of its own)', op[0] === 'card' && op[1] !== 'closed' && op[2] && op[3] && /"shown":0/.test(op[4]), op);
  await fresh('var g=gdGroupById("howto");g.inMenu=false;'); const h1 = await R('gbItems().join()');
  await R('var g=gdGroupById("howto");g.inMenu=true;g.inSettings=false;S.view="card";S.stop="half";S.settingsOpen=true;renderPhone(false)'); const h2 = await R('!ph.querySelector(".vhow")');
  await R('var g=gdGroupById("howto");g.inSettings=true;g.on=false;S.settingsOpen=true;renderPhone(false)'); const h3 = await R('[!ph.querySelector(".vhow"),gbItems().indexOf("how")<0,gdWalk("test")===false]');
  await R('gdGroupById("howto").on=true;S.settingsOpen=false;renderPhone(false)');
  ok('Its entries follow the group: In the ✦ menu Hidden: no item; In Settings Hidden: no button; the group Off: neither, and nothing opens', h1 === 'tour,ask,view,try,settings' && h2 && h3.every(Boolean), [h1, h2, h3]);

  // ---------- 5. Card Controls > Hints: a view of the same data ----------
  await fresh(); await R('S.tab="card";S.ui.level=1;S.op={s5:true,s5hints:true};renderEditor()'); await W(200);
  const vw = await R('var f=ed.querySelector(\'[data-fo="s5hints"]\');[[].map.call(f.querySelectorAll("[data-goto]"),function(b){return b.dataset.goto+"|"+b.textContent.trim()}),f.querySelectorAll("[data-set],[data-sel],[data-num],[data-path]").length,[].map.call(f.querySelectorAll("tbody tr"),function(r){return r.children[0].textContent+"="+r.children[1].textContent}).join()]');
  ok('Card Controls > Hints: each row reads Edited in Azu speaks up and links to its cue (6), the group and How to use the card; no control of its own (it only reads)',
    vw[0].length === 8 && vw[0].filter(v => /^gd:i:/.test(v)).every(v => /\|Edited in The mascot speaks up ›$/.test(v)) && vw[0].indexOf('gd:g:card|Edited in Guide › The mascot speaks up › App › The card (cues) ›') >= 0 && vw[0].some(v => /^gd:g:howto\|Edited in Guide › The mascot speaks up › Sequences/.test(v)) && vw[1] === 0 &&
    /^Grabber nudge=Once a visit,Locate label=Once a visit,Navigation Bar=Once a visit/.test(vw[2]), vw);
  await R('S.guidance.items.ba.shows="card";S.guidance.items.term.on=false;renderEditor()'); await W(150);
  const vw2 = await R('[].map.call(ed.querySelectorAll(\'[data-fo="s5hints"] tbody tr\'),function(r){return r.children[1].textContent}).join()');
  await R('S.guidance.items.ba.shows="visit";S.guidance.items.term.on=true;renderEditor()');
  ok('...it reads Guidance\'s values (Before / After set to Every card, Underlined words Off, in Guidance: the view says so)', vw2 === 'Once a visit,Once a visit,Once a visit,Every card,Once a visit,Off', vw2);
  await R('ed.querySelector(\'[data-fo="s5hints"] [data-goto="gd:g:card"]\').scrollIntoView({block:"center"})'); await W(150);
  await p.locator('[data-fo="s5hints"] [data-goto="gd:g:card"]').click(); await W(400);
  const gone = await R('var g=ed.querySelector(\'[data-fo="gd:g:card"]\');[S.tab,!!g&&g.open,!!g&&g.getBoundingClientRect().top<window.innerHeight&&g.getBoundingClientRect().bottom>0]');
  await R('S.tab="card";S.ui.level=1;S.op={s5:true,s5hints:true};renderEditor();ed.querySelector(\'[data-fo="s5hints"] [data-goto="gd:i:navbar"]\').scrollIntoView({block:"center"})'); await W(150);
  await p.locator('[data-fo="s5hints"] [data-goto="gd:i:navbar"]').click(); await W(400);
  const gone2 = await R('var g=ed.querySelector(\'[data-fo="gd:i:navbar"]\');[S.tab,!!g&&g.open,!!g&&g.getBoundingClientRect().top<window.innerHeight&&g.getBoundingClientRect().bottom>0]');
  ok('...its links open Guide › 4 · Azu speaks up › App › The card, and a cue\'s own row (Navigation Bar)', gone.every(x => x === true || x === 'guide') && gone2.every(x => x === true || x === 'guide'), [gone, gone2]);
  const al = await R('var g=gdGroupById("card"),l=S.guidance.items.label,a=[];S.hints.on=false;a.push(g.on);S.hints.on=true;a.push(g.on);S.ctl.locate.label="open";a.push(l.shows);S.ctl.locate.label="off";a.push(l.on);S.ctl.locate.label="visit";a.push(l.on,l.shows,S.ctl.locate.label);g.on=false;a.push(S.hints.on);g.on=true;a');
  ok('The old names are Guidance\'s values (no copy): S.hints.on is The card (cues)\'s On; S.ctl.locate.label the Locate label\'s Shows and On', JSON.stringify(al) === JSON.stringify([false, true, 'card', false, true, 'visit', 'visit', false]), al);

  // ---------- 6. every new row's demo; the words and their owners; the old timers gone ----------
  await fresh(); await R('S.tab="guide";S.ui.level=9;S.op={};renderEditor()'); await W(200);
  const rows = await R('var o=[];ed.querySelectorAll(\'[data-fo="gd:g:card"] .row,[data-fo="gd:g:howto"] .row,[data-fo="gd:overall"] .row,[data-fo="gd:g:card"] summary [data-set],[data-fo="gd:g:howto"] summary [data-set]\').forEach(function(r){var k=r.matches(".row")?rowKey(r):r.dataset.set.slice(1);if(k&&o.indexOf(k)<0&&(/^guidance\\.(cueRoom|items\\.(nudge|label|navbar|ba|strip|term|wk_[a-z]+)\\.)/.test(k)||k.indexOf(gdGroupPath("card"))===0||k.indexOf(gdGroupPath("howto"))===0))o.push(k)});o');
  ok(`every new row has its demo (${rows.length} rows: Overall's Cue room; The card's 4 group rows and its 6 cues' On, importance, Shows and Done when, round 6's Shows the gesture on Before / After and Swipe strips; How to use the card's On, In Settings, In the ✦ menu and its 5 steps' On and, round 6, Focus; round 9: In Things to try on the 4 cues that teach)`,
    rows.length === 1 + 4 + 24 + 2 + 3 + 5 + 5 + 4 && (await R(`${JSON.stringify(rows)}.filter(function(k){return!rowDemoOf(k)})`)).length === 0, rows.length);
  const CHIP_RE = { 'guidance.cueRoom': /×10 · The cue room after a tip: \d+ s/, 'guidance.items.nudge.doneWhen': /Done when the visitor saw How to use the card's step “The grabber”/, 'guidance.items.navbar.importance': /Importance 3 of 5/,
    'guidance.items.label.shows': /Once a visit: once it has shown/, [await R('gdGroupPath("howto")') + 'inMenu']: /In the ✦ menu: How does this work\?/, 'guidance.items.wk_bar.on': /How to use the card · from The Navigation Bar/ };
  const RING_RE = { 'guidance.items.nudge.on': /\|(#grab|\.gdchip)$/, 'guidance.items.wk_grab.on': /\|\.gfocus$/, [await R('gdGroupPath("howto")') + 'inSettings']: /\|\.vhow$/, [await R('gdGroupPath("howto")') + 'inMenu']: /\|\[data-gbgo="how"\]$/ };
  const missing = [], seen = {}, rings = {};
  for (const k of rows) {
    await R('document.body.dataset.rowdemo="";window.__chips=[];clearInterval(window.__chI);window.__chI=setInterval(function(){var c=ph.querySelector(".gdchip");if(c&&window.__chips.indexOf(c.textContent)<0)window.__chips.push(c.textContent)},50)');
    const lb = await R(`var c=ed.querySelector('.row [data-set="@${k}"],.row [data-num="@${k}"]'),l=c&&c.closest(".row").querySelector("label.rd");if(l){l.scrollIntoView({block:"center"});var r=l.getBoundingClientRect();[r.left+12,r.top+r.height/2]}else null`);
    if (!lb) await R(`showRowDemo("${k}")`); else await p.mouse.click(lb[0], lb[1]);
    const okd = await until('/\\|found\\|/.test(document.body.dataset.rowdemo||"")', 5000);
    if (!okd) missing.push(k + ' ' + await R('document.body.dataset.rowdemo||""'));
    rings[k] = await R('document.body.dataset.rowdemo||""');
    if (CHIP_RE[k]) await until(`window.__chips.some(function(c){return ${CHIP_RE[k]}.test(c)})`, 9000);
    seen[k] = await R('window.__chips.slice()');
  }
  await R('clearInterval(window.__chI);if(G.on)stopTour()');
  ok('...a press on each new row shows its demo on the phone, its part ringed (the cue\'s element or its chip, the step\'s mark, the Settings button, the menu item)', missing.length === 0 && Object.keys(RING_RE).every(k => RING_RE[k].test(rings[k] || '')), { missing, rings: Object.keys(RING_RE).map(k => [k, rings[k]]) });
  ok('...the Cue room on the ×10 clock with its countdown chip; Done when, importance and Shows in words; the walkthrough from a step; the ✦ menu\'s item', Object.keys(CHIP_RE).every(k => (seen[k] || []).some(c => CHIP_RE[k].test(c))), Object.keys(CHIP_RE).map(k => [k, seen[k]]));
  const rb = await R('[gdReadBack(S.guidance.items.navbar),gdReadBack(S.guidance.items.nudge),gdReadBack(S.guidance.items.wk_grab),gdHowOften(S.guidance.items.label)]');
  ok('...the read-backs in words: a cue, its moment, how often, Reduce Motion, the cue room; a step, pulled only, what seeing it retires',
    /Navigation Bar · App › The card \(cues\) · a cue \(motion on its element, no mascot, its own words over it\) · at a pause, when the Navigation Bar shows · once a visit, until the visitor has seen it in How to use the card · under Reduce Motion shown, with no travel · one at a time with every tip and cue; never within 5 s of a tip, either way \(the cue room\)/.test(rb[0]) &&
    /when the card opens at Half/.test(rb[1]) && /skipped \(a moving cue\)/.test(rb[1]) && /The grabber · App › How to use the card · step 1 of 5 · pulled only .* seen \(its line heard\): the Grabber nudge is done for good/.test(rb[2]) && rb[3] === 'once a visit, until the visitor has seen it in How to use the card', rb);
  const words = await R('[TXOWN.gd_how,TXOWN.gd_how_wall,TXOWN.gd_how_open,TXOWN.hint_navbar,TXOWN.how_grab,TXOWN.how_block,["gd_how","gd_how_wall","gd_how_open","hint_navbar","how_grab","how_bar"].every(function(k){return S.tx[k].en.length>5&&S.tx[k].pt.length>5}),ownerName("gd:g:howto"),ownerName("gd:i:navbar")]');
  ok('The words in Languages & Terms, en and pt: How does this work?\'s owned by the group\'s foldout; the bar\'s tip and each step\'s line by their item', words[0] === 'gd:g:howto' && words[1] === 'gd:g:howto' && words[2] === 'gd:g:howto' && words[3] === 'gd:i:navbar' && words[4] === 'gd:i:wk_grab' && words[5] === 'gd:i:wk_block' && words[6] && /How to use the card/.test(words[7]) && /Navigation Bar/.test(words[8]), words);
  const old = await R('[typeof hintDue,typeof hintMark,typeof hintFree,typeof hintTry,typeof howTour,typeof nudgeOnce,typeof labelShowing,typeof HINTS,typeof HOW,S.hintDone===undefined&&S.hintCard===undefined&&S.hintBusy===undefined&&S.lastMove===undefined&&S.openedAt===undefined]');
  ok('The old Hints code is gone: no 300 ms timer, no hintDue / hintMark / hintFree / hintTry, no howTour or nudgeOnce, no HINTS / HOW lists, no S.hintDone / hintCard / hintBusy', old.slice(0, 9).every(t => t === 'undefined') && old[9], old);

  // ---------- 7. the two new scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^gd4_/.test(x[1])})');
  ok('The console lists round 4\'s two guidance scenes (a hint and a tip never at once; How does this work?)', scenes.length === 2 && JSON.stringify(scenes.map(s => s[1])) === JSON.stringify(['gd4_once', 'gd4_how']), scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    const does = await R('ed.querySelector(".agscenes").textContent');
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 60000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`${id === 'gd4_once' ? '[room] ' : ''}Scene "${label}": Set up and ask runs it, ✓`, /Does: nothing:/.test(does) && r && r.ok === true, r && [r.note, r.answer]);
    await R('gdStop("test");G.gd.rate=1;G.gd.manual=false;if(G.on)stopTour()');
  }
  const cs = await R('[S.hints.on,gdGroupById("card").items.every(function(id){return S.guidance.items[id].on}),S.guidance.on,S.guideMute]');
  ok('...the scenes give back what they rested (the cues On, Guidance On)', cs[0] === true && cs[1] && cs[2] === true, cs);
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
