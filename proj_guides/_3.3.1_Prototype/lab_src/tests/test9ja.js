// g6 checks, part A (Playwright + the installed Edge): node test9ja.js -> PASS/FAIL per check
// Guidance, round 6 (_3.8 section 8): the look and the item form's tools. Each Focus (None, Glint, Ring, Spotlight) and the Spotlight
// offered only for Essentials and walkthrough steps; the hop (a 0.45 s arc) and Walks to it (their end beside the marker, 0 px over its
// words); the ghost finger (a tap, a drag across a Before / After, a swipe on a strip); each Button chip's action by a real click; Reduce
// Motion's still versions; Tips stay until tapped; the screen-reader stand-in; the dock's Tips; Settings › The guide › Tips (All /
// Essentials only / Off) and Show the tips again with its Undo; each named warning made to appear, then fixed; each template; Duplicate,
// Delete and Undo; the drag reorder changing the tie-break; every new row's demo; the four scenes in Ask Azu's console; the scheduler's
// new rules alone on a fake clock. Part B (test9jb.js) presses every button of the item form. Split from test9j.js on 2026-11-09 (level 3
// after guidance round 10: the one file reached its 600 s kill on a busy machine). MUTATE=1 node test9ja.js runs it with the Spotlight
// offered on any item and a Delete with no Undo: the checks named [spot] and [undo] turn red.
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
  if (process.env.MUTATE) { await R('gdSpotOk=function(){return true};gdUndoDel=function(){return false}'); console.log('MUTATION: the Spotlight offered on any item, and Delete with no Undo'); }
  await R('window.__g0=JSON.stringify(S.guidance);window.__tx0=JSON.stringify(S.tx)');
  const base = 'S.guidance=JSON.parse(window.__g0);S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.ui.gdSR=false;S.guidance.items.gbhint.on=false;gdGroupById("inview").on=false;S.guideCfg.gbtn="both";S.guideCfg.mascot=true;G.gd.demo=false;gdChip(null);';
  const fresh = async (extra) => { await R(base + 'if(G.on)stopTour();G.cam.open=false;S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);G.gd.manual=true;gdStop("test");gdResetVisitor();G.gd.movedAt=null;G.gd.onWall=false;gdAdvance(250);' + (extra || '')); await W(120); };
  const show = (id, o) => `gdStart(gdItemNow("${id}"),"test",${o || '{pull:true,ms:60000}'});`;
  const editor = async (ops) => { await R(`S.tab="guide";S.ui.level=1;S.op=${JSON.stringify(ops)};renderEditor()`); await W(120); };

  lap('before 1');
  // ---------- 1. the scheduler's new rules alone (gdPick on a fake clock) ----------
  const pk = (mod, now, id) => R(`S.guidance=JSON.parse(window.__g0);var V={visits:1,visit:{n:1,at:0,lastEnd:null,items:{opening:{shown:1,last:0,end:9000,dismissed:0}},ev:{}},ever:{items:{opening:{shown:1,done:false,by:""}}},used:{}},
    C={on:true,breathing:15,demo:false,showing:null,remark:null,speaking:false,panel:null,movedAt:null,onWall:true,where:"",marker:{id:"painting",name:"Ribeira Palace",x:200,y:400},gbtn:{name:"the ✦",x:40,y:700},stillSince:0,returning:false,aud:"explorer",tips:"all"};
    V.visit.lastEnd=9000;var T=S.guidance.items.tapmarker;${mod || ''};var items=gdItems();${id ? `gdWhyNot(items,V,C,${now},"${id}")` : `var r=gdPick(items,V,C,${now});[r.item?r.item.id:null,r.why]`}`);
  await R(base);
  const P = {};
  P.all = await pk('', 30000); P.ess = await pk('C.tips="essentials"', 30000); P.off = await pk('C.tips="off"', 30000);
  P.essOpen = await R('var V={visits:1,visit:{n:1,at:0,items:{},ev:{}},ever:{items:{}},used:{}},C={on:true,breathing:15,onWall:true,where:"",stillSince:0,tips:"essentials"};var r=gdPick(gdItems(),V,C,0);[r.item&&r.item.id,r.why]');
  P.aud = await pk('T.aud="family"', 30000, 'tapmarker'); P.audOk = await pk('T.aud="family";C.aud="family"', 30000);
  P.visit = await pk('T.visitIs="returning"', 30000, 'tapmarker'); P.used = await pk('T.hasUsed="view"', 30000, 'tapmarker'); P.notUsed = await pk('T.notUsed="view";V.used.view=1', 30000, 'tapmarker');
  P.latest = await pk('T.notAfter=25', 30000, 'tapmarker'); P.event = await pk('T.when="event";T.event="menu"', 30000, 'tapmarker'); P.eventOk = await pk('T.when="event";T.event="menu";V.visit.ev.menu=25000', 30000);
  P.never = await pk('T.afterDismiss="never";V.ever.items.tapmarker={shown:1,dismissed:1,done:false}', 300000, 'tapmarker'); P.visitD = await pk('T.afterDismiss="visit";V.visit.items.tapmarker={shown:1,last:0,end:1000,dismissed:1,how:"dismissed"}', 300000, 'tapmarker');
  P.must = await pk('C.marker=null', 30000, 'tapmarker'); P.mustNo = await pk('C.marker=null;T.mustSee=false', 30000);
  ok('pick: Settings › Tips: All shows Tap a marker; Essentials only holds it (the opening, Essential, still shows); Off holds everything', P.all[0] === 'tapmarker' && P.ess[0] === null && /Essentials only/.test(P.ess[1]) && P.essOpen[0] === 'opening' && P.off[0] === null && /turned Tips Off/.test(P.off[1]), [P.all, P.ess, P.off, P.essOpen]);
  ok('pick: Only if: the audience, a returning visit, has used, has not used, the latest second', /only for Family/.test(P.aud) && P.audOk[0] === 'tapmarker' && /only on a returning visit/.test(P.visit) && /only once Ask about the view opened/.test(P.used) && /only until/.test(P.notUsed) && /not after 25 s/.test(P.latest), [P.aud, P.audOk, P.visit, P.used, P.notUsed, P.latest]);
  ok('pick: When on an event (the ✦ menu opened 5 s ago: yes; never: it waits)', /waits for the visitor opened the ✦ menu/.test(P.event) && P.eventOk[0] === 'tapmarker', [P.event, P.eventOk]);
  ok('pick: After a dismissal: Never again, Not again this visit', /never again/.test(P.never) && /not again this visit/.test(P.visitD), [P.never, P.visitD]);
  ok('pick: Must be on screen: Yes waits for its marker; No shows without it', /no marker in view/.test(P.must) && P.mustNo[0] === 'tapmarker', [P.must, P.mustNo]);
  const words = await R('var it=Object.assign({},S.guidance.items.tapmarker,{says:["wall_tip","gd_askview"],saysAud:{family:"gd_how_open"}});S.tx.__y={en:"{point} in {year}",pt:"{point} em {year}"};' +
    '[[0,1,2,3].map(function(n){return gdLine(it,false,{n:n,aud:"explorer"})}),gdLine(it,false,{aud:"family"}),gdLine(Object.assign({},it,{saysBack:"gd_opening_back"}),true,{}),gdLine(Object.assign({},it,{says:"__y"}),false,{point:"Ribeira Palace",year:1500})]');
  ok('Says: variants in turn, never the same twice in a row; an audience\'s own line; the returning visitor\'s; the {point} and {year} slots', words[0].every((w, i) => i === 0 || w !== words[0][i - 1]) && words[1] === await R('TXT("gd_how_open")') && words[2] === await R('TXT("gd_opening_back")') && words[3] === 'Ribeira Palace in 1500', words);

  lap('before 2');
  // ---------- 2. each Focus on the phone; the Spotlight offered only where it is allowed ----------
  const fr = {};
  for (const f of ['none', 'glint', 'ring', 'spotlight']) {
    await fresh(`var it=S.guidance.items.tapmarker;it.focus="${f}";it.essential=${f === 'spotlight'};it.gesture=false;it.dim=0.35;` + show('tapmarker')); await W(350);
    fr[f] = await R('var sp=ph.querySelector(".gspot.gdspot.on"),g=ph.querySelector(".gdglint i");[!!ph.querySelector(".gdglint"),!!ph.querySelector(".gdring"),!!sp,sp?sp.style.getPropertyValue("--scrim"):"",g?g.getAnimations().length:0,gdOverWords()]');
  }
  ok('Focus None: no glint, no ring, no spotlight', !fr.none[0] && !fr.none[1] && !fr.none[2], fr.none);
  ok('Focus Glint: the target shines once (a light band sweeping across it), no ring', fr.glint[0] && fr.glint[4] > 0 && !fr.glint[1] && !fr.glint[2], fr.glint);
  ok('Focus Ring: the ring on the marker, no glint', fr.ring[1] && !fr.ring[0] && !fr.ring[2], fr.ring);
  ok('Focus Spotlight (an Essential): the rest dims at the item\'s Dim (0.35), the marker cut out; 0 px over words', fr.spotlight[2] && fr.spotlight[3] === '0.35' && !fr.spotlight[1] && fr.spotlight[5] === 0, fr.spotlight);
  const spot = await R('var sc=AGSCENES.filter(function(s){return s.id==="gd6_spot"})[0];sc.setup();Promise.resolve(sc.act())');
  ok('...the cut-out holds the marker and its name (readable), the bubble not over the marker', spot && spot[0] === true, spot);
  await fresh('var it=S.guidance.items.tapmarker;it.focus="spotlight";it.essential=false;' + show('tapmarker')); await W(300);
  const fb = await R('[!!ph.querySelector(".gspot.gdspot"),!!ph.querySelector(".gdring"),gdFocusNow(S.guidance.items.tapmarker),/a Spotlight shows as a Ring/.test(gdReadBack(S.guidance.items.tapmarker))]');
  ok('[spot] a Spotlight on an item that is not Essential shows as a Ring, and the read-back says so (no silent fallback)', !fb[0] && fb[1] && fb[2] === 'ring' && fb[3], fb);
  await R(base); await editor({ gt4: true, 'gd:app': true, 'gd:g:start': true, 'gd:i:tapmarker': true, 'gd:i:opening': true, 'gd:g:howto': true, 'gd:i:wk_bar': true });
  const sp1 = await R('var q=function(id){var b=[].filter.call(ed.querySelectorAll(\'[data-fo="gd:i:\'+id+\'"] .seg button\'),function(x){return x.textContent==="Spotlight"})[0];var w=ed.querySelector(\'[data-fo="gd:i:\'+id+\'"] .gdspotwhy\');return b?[b.disabled,!!b.dataset.set,w?w.textContent:""]:null};[q("tapmarker"),q("opening"),q("wk_bar")]');
  ok('[spot] the Editor: Spotlight disabled for Tap a marker (not Essential) and for the Opening message (no target), each saying why; offered for a walkthrough step', sp1[0] && sp1[0][0] && !sp1[0][1] && /not Essential/.test(sp1[0][2]) && sp1[1] && sp1[1][0] && /needs a target/.test(sp1[1][2]) && sp1[2] && !sp1[2][0] && sp1[2][1], sp1);
  await p.locator('[data-fo="gd:i:tapmarker"] [data-set="@guidance.items.tapmarker.essential"][data-v="true"]').click(); await W(150);
  await p.locator('[data-fo="gd:i:tapmarker"] [data-set="@guidance.items.tapmarker.focus"][data-v="spotlight"]').click(); await W(150);
  const sp2 = await R('[S.guidance.items.tapmarker.essential,S.guidance.items.tapmarker.focus,!!ed.querySelector(\'[data-num="@guidance.items.tapmarker.dim"]\')]');
  ok('...made Essential (a real click), Spotlight is offered; chosen, its Dim row appears', sp2[0] === true && sp2[1] === 'spotlight' && sp2[2], sp2);

  lap('before 3');
  // ---------- 3. the hop and the walk: their end beside the marker, 0 px over its words ----------
  const mv = {}; await W(900); // the Editor's last change shows the card a moment later: let it settle first
  for (const g of ['beside', 'walk']) {
    await fresh(`var it=S.guidance.items.tapmarker;it.goes="${g}";it.gesture=false;fabLayer();`); await until('!!ph.querySelector(".azfab")', 2000); await R(show('tapmarker'));
    mv[g] = await R('var az=ph.querySelector(".azfab"),a=az.getAnimations().filter(function(x){return x.effect&&x.effect.getKeyframes().some(function(f){return f.translate})})[0],k=a?a.effect.getKeyframes():[],y=function(f){return parseFloat(String(f.translate).split(" ")[1]||0)};[a?Math.round(a.effect.getTiming().duration):0,k.length,k.length?k.some(function(f){var y0=y(k[0]),y1=y(k[k.length-1]);return y(f)<y0+(y1-y0)*(1-Math.pow(1-f.computedOffset,2.2))-10}):false,k.length>3?y(k[k.length-2])===y(k[0])&&y(k[k.length-1])!==y(k[0]):false]');
    await until('!ph.querySelector(".azfab").getAnimations().some(function(x){return x.effect&&x.effect.getKeyframes().some(function(f){return f.translate})})', 3500); await W(200);
    mv[g] = mv[g].concat(await R('var az=ph.querySelector(".azfab"),A=gdPR(az),M=gdMarker();[Math.round(Math.hypot((A.l+A.r)/2-M.x,(A.t+A.b)/2-M.y)),gdOver(A,gdBox(M.x-22,M.y-22,44,44)),gdOverWords()]'));
  }
  ok('Mascot goes Beside it: a short arc hop (0.45 s, rising above the straight path); it ends beside the marker, 0 px over it or its words', mv.beside[0] === 450 && mv.beside[2] && mv.beside[4] < 110 && mv.beside[5] === 0 && mv.beside[6] === 0, mv.beside);
  ok('Mascot goes Walks to it: a slower walk (along the bottom first, then up); it ends beside the marker, 0 px over it or its words', mv.walk[0] > 900 && mv.walk[3] && mv.walk[4] < 110 && mv.walk[5] === 0 && mv.walk[6] === 0, mv.walk);

  lap('before 4');
  // ---------- 4. the ghost finger ----------
  await fresh('S.guidance.items.tapmarker.gesture=true;' + show('tapmarker'));
  await until('var f=ph.querySelector(".gdfinger"),a=f&&f.getAnimations()[0];!!a&&a.currentTime>a.effect.getTiming().delay+500', 3000);
  const fg = await R('var f=ph.querySelector(".gdfinger"),M=gdMarker();[f.dataset.gesture,gdOverText(f),gdOver(gdPR(f),gdBox(M.x-22,M.y-22,44,44)),gdOver(gdPR(f),gdPR(ph.querySelector(".gdbub")))]');
  ok('The ghost finger on Tap a marker: a tap, mid-gesture beside the marker, 0 px over words, the marker or the bubble', fg[0] === 'tap' && fg[1] === 0 && fg[2] === 0 && fg[3] === 0, fg);
  await fresh('S.guidance.items.tapmarker.gesture=false;' + show('tapmarker')); await W(1500);
  ok('...Shows the gesture Off: no finger', !(await R('!!ph.querySelector(".gdfinger")')));
  const fba = await R('var sc=AGSCENES.filter(function(s){return s.id==="gd6_finger"})[0];sc.setup();Promise.resolve(sc.act())');
  ok('...on Before / After: a drag across, 0 px over its words (the scene)', fba && fba[0] === true, fba);
  await R(base + 'G.gd.manual=true;cuePlace("strip")'); await W(900); await R(show('strip', '{pull:true,ms:4000}'));
  await until('var f=ph.querySelector(".gdfinger"),a=f&&f.getAnimations()[0];!!a&&a.currentTime>300', 2000);
  const fst = await R('var f=ph.querySelector(".gdfinger");f?[f.dataset.gesture,gdOverText(f)]:null');
  ok('...on Swipe strips: a swipe, 0 px over words', fst && fst[0] === 'swipe' && fst[1] === 0, fst);

  lap('before 5');
  // ---------- 5. each Button chip's action (a real click) ----------
  const btn = {};
  for (const a of ['show', 'tour', 'card', 'view']) {
    await fresh(`var it=S.guidance.items.askview;it.button="${a}";it.notBefore=0;G.gd.cur=null;` + show('askview', '{}')); await W(350);
    const word = await R('(ph.querySelector("#ph [data-gdcta]")||{}).textContent||""');
    await p.locator('#ph [data-gdcta]').click(); await W(450);
    btn[a] = [word, await R('[!!G.gd.cur,gdVisitOf(S.gdVisitor,"askview").how,!!ph.querySelector(".gdfinger"),G.on,S.stop,!!G.cam.open]')];
    await R('if(G.on)stopTour();G.cam.open=false');
  }
  ok('Button Show me: the chip reads “Show me›”; a tap ends the tip (acted) and the ghost finger shows the gesture on the ✦', /^Show me/.test(btn.show[0]) && !btn.show[1][0] && btn.show[1][1] === 'acted' && btn.show[1][2], btn.show);
  ok('Button Start the tour: the tour starts (acted)', /^Start the tour/.test(btn.tour[0]) && btn.tour[1][1] === 'acted' && btn.tour[1][3], btn.tour);
  ok('Button Open the card: the card opens (acted)', /^Open the card/.test(btn.card[0]) && btn.card[1][1] === 'acted' && btn.card[1][4] !== 'closed', btn.card);
  ok('Button Ask about the view: it opens (acted)', /^Ask about the view/.test(btn.view[0]) && btn.view[1][1] === 'acted' && btn.view[1][5], btn.view);
  await fresh('S.guidance.items.askview.button="show";' + show('askview')); await W(300);
  const chip = await R('var c=ph.querySelector("[data-gdcta]"),b=ph.querySelector(".gdbub");[c.offsetHeight,gdOver(gdPR(b),gdPR(ph.querySelector(".gbtn")))]');
  ok('...the chip is a 44 px target; the bubble is not over the ✦', chip[0] >= 44 && chip[1] === 0, chip);

  lap('before 6');
  // ---------- 6. Reduce Motion: no hop, a still glint, a still ring, the finger still at its end; the words stay ----------
  await fresh('S.reduce=true;renderPhone(false);var it=S.guidance.items.tapmarker;it.focus="glint";it.gesture=true;it.goes="walk";' + show('tapmarker')); await W(300);
  const rm = await R('var az=ph.querySelector(".azfab"),g=ph.querySelector(".gdglint"),f=ph.querySelector(".gdfinger"),b=ph.querySelector(".gdbub");[az.getAnimations().length,/px/.test(az.style.translate),g.classList.contains("still"),g.getAnimations().length+g.querySelector("i").getAnimations().length,f.classList.contains("still"),f.getAnimations().length,f.dataset.endx!=null,b.classList.contains("gd-in"),b.textContent]');
  await fresh('S.reduce=true;renderPhone(false);' + show('tapmarker')); await W(200);
  const rr = await R('var r=ph.querySelector(".gdring");[!!r,r&&r.classList.contains("gd-pulse")]');
  ok('Reduce Motion: no walk or hop (Azu is there), the glint a still highlight, the ghost finger still at its end, the ring still, the words stay', rm[0] === 0 && rm[1] && rm[2] && rm[3] === 0 && rm[4] && rm[5] === 0 && rm[6] && !rm[7] && rm[8] === 'Tap here.' && rr[0] && !rr[1], [rm, rr]);
  await R('S.reduce=false;renderPhone(false)');

  lap('before 7');
  // ---------- 7. Tips stay until tapped ----------
  await fresh('gdPrefs().stay=true;S.guidance.items.tapmarker.gesture=false;G.gd.cur=null;' + show('tapmarker', '{}'));
  await R('gdAdvance(30000)');
  const st1 = await R('var x=ph.querySelector(".gdbub [data-gdx]");[!!G.gd.cur&&G.gd.cur.item.id,x?[x.offsetWidth,x.offsetHeight]:null,x?x.getAttribute("aria-label"):""]');
  await p.locator('#ph .gdbub [data-gdx]').click(); await W(200);
  const st2 = await R('[!!G.gd.cur,gdVisitOf(S.gdVisitor,"tapmarker").how]');
  await fresh('S.guidance.items.tapmarker.gesture=false;G.gd.cur=null;' + show('tapmarker', '{}')); await R('gdAdvance(30000)');
  const st3 = await R('[!!G.gd.cur,gdVisitOf(S.gdVisitor,"tapmarker").how]');
  ok('Tips stay until I tap them On: 30 s later the tip is still there, with a 44 px close button (Dismiss); a click closes it (dismissed); Off: its time runs out (ignored)', st1[0] === 'tapmarker' && st1[1] && st1[1][0] >= 44 && st1[1][1] >= 44 && st1[2] === 'Dismiss' && !st2[0] && st2[1] === 'dismissed' && !st3[0] && st3[1] === 'ignored', [st1, st2, st3]);
  const stay = await R('var sc=AGSCENES.filter(function(s){return s.id==="gd6_stay"})[0];sc.setup();Promise.resolve(sc.act())');
  ok('...the scene Tips stay until tapped', stay && stay[0] === true, stay);

  lap('before 8');
  // ---------- 8. the screen-reader stand-in; the dock's Tips ----------
  await R(base); await editor({ gt0: true });
  await p.locator('[data-fo="gt0"] [data-set="ui.gdSR"][data-v="true"]').click(); await W(300);
  const srOn = await R('S.ui.gdSR');
  await fresh('S.ui.gdSR=true;gdGroupById("inview").on=true;S.gdVisitor.visit.heard=true;G.gd.cur=null;' + show('inview', '{}')); await W(150);
  const sr1 = await R('var c=G.gd.cur,b=ph.querySelector(".gdbub");[c&&c.item.base,c&&c.spoken,b.getAttribute("aria-live"),b.getAttribute("role"),c&&c.text]');
  await R('gdStop("test","ignored");G.gd.cur=null;S.ui.gdSR=false;S.gdVisitor.visit.heard=true;gdGroupById("inview").on=true;S.gdVisitor.visit.items={};' + show('inview', '{}')); await W(150);
  const sr2 = await R('[G.gd.cur&&G.gd.cur.spoken]'); await R('gdStop("test","ignored")');
  ok('Screen reader on (a real click in Preview & Test › The visitor): Azu\'s remark is announced politely (role status, aria-live polite), never spoken; off, it is spoken', srOn === true && sr1[0] === 'inview' && sr1[1] === false && sr1[2] === 'polite' && sr1[3] === 'status' && sr2[0] === true, [srOn, sr1, sr2]);
  await p.locator('#ph .gbtn-wrap .gbtn').click(); await W(250); await p.locator('#ph [data-gbgo="ask"]').click(); await W(400);
  const dock = await R('var t=ph.querySelector(".gdock .gm-tips");[!!t,t?t.textContent:"",[].map.call(ph.querySelectorAll(".gdock .gm-tip"),function(x){return x.textContent})]');
  ok('The dock\'s conversation (✦ › Ask, real clicks) holds every tip shown this visit under Tips', dock[0] && /^Tips/.test(dock[1]) && dock[2].indexOf(sr1[4]) >= 0 && dock[2].indexOf(await R('TXT("gd_opening")')) >= 0, dock);
  await R('stopTour()');
  const srs = await R('var sc=AGSCENES.filter(function(s){return s.id==="gd6_sr"})[0];sc.setup();Promise.resolve(sc.act())');
  ok('...the scene Screen reader on: nothing spoken, the tip in the dock', srs && srs[0] === true, srs);

  lap('before 9');
  // ---------- 9. Settings › The guide › Tips; Show the tips again and its Undo (real clicks on the phone) ----------
  await fresh('S.stop="half";S.settingsOpen=true;renderPhone(false);');
  const opt = async (k) => { const l = p.locator(`#ph [data-gdpref="${k}"]`); await l.scrollIntoViewIfNeeded(); await l.click(); await W(150); };
  await opt('tips|essentials'); const s1 = await R('[gdPrefs().tips,gdContext().tips]');
  await opt('tips|off'); const s2 = await R('[gdPrefs().tips,(ph.querySelector(".vtips .vnote")||{}).textContent,gdGate(gdContext(),gdNow())]');
  await opt('tips|all'); await opt('stay|true'); const s3 = await R('[gdPrefs().tips,gdPrefs().stay]'); await opt('stay|false');
  ok('Settings › The guide › Tips: Essentials only, Off (“You can always tap ✦ for help.”, the scheduler\'s gate), All; Tips stay until I tap them On', s1[0] === 'essentials' && s1[1] === 'essentials' && s2[0] === 'off' && /You can always tap ✦ for help/.test(s2[1]) && /turned Tips Off/.test(s2[2]) && s3[0] === 'all' && s3[1] === true, [s1, s2, s3]);
  await R('gdUsed("card","test");renderPhone(true)');
  const a0 = await R('gdEverOf(S.gdVisitor,"tapmarker").done');
  const ag = p.locator('#ph [data-gdagain]'); await ag.scrollIntoViewIfNeeded(); await ag.click(); await W(200);
  const a1 = await R('[gdEverOf(S.gdVisitor,"tapmarker").done,(ph.querySelector(".gdtoast")||{}).textContent||"",!!ph.querySelector(".gstopask,[role=alertdialog]")]');
  await p.locator('#ph .gdtoast [data-gdundo]').click(); await W(200);
  const a2 = await R('gdEverOf(S.gdVisitor,"tapmarker").done');
  ok('Show the tips again: the memory reset (Tap a marker no longer done), a toast “The tips will show again.” with Undo, no popup; Undo brings the memory back', a0 === true && a1[0] === false && /The tips will show again/.test(a1[1]) && !a1[2] && a2 === true, [a0, a1, a2]);

  lap('before 10');
  lap('before 11');
  // ---------- 11. each named warning, made to appear, then fixed ----------
  const warns = async () => { await R('S.tab="guide";S.ui.level=1;S.op={gt4:true,"gd:app":true,"gd:g:wall":true};renderEditor()'); return R('[].map.call(ed.querySelectorAll("#gdwarns .gdwl"),function(x){return x.dataset.gdwarn+": "+x.textContent})'); };
  await R(base + 'gdGroupById("inview").on=true;S.guidance.items.gbhint.on=true;'); const w0 = await warns(); await R(base);
  const wcase = [['its kind is Off', 'S.guidance.kinds[0].on=false', 'S.guidance.kinds[0].on=true', /^never: The “Tap a marker” can never show: its kind, App, is Off/],
    ['Only if contradicts itself', 'S.guidance.items.tapmarker.hasUsed="card";S.guidance.items.tapmarker.notUsed="card"', 'S.guidance.items.tapmarker.hasUsed="";S.guidance.items.tapmarker.notUsed=""', /Only if contradicts itself: has used and has not used/],
    ['Done when already true', 'S.guidance.items.tapmarker.hasUsed="card"', 'S.guidance.items.tapmarker.hasUsed=""', /Done when is already true/],
    ['a line too long for 2 lines (Portuguese)', 'S.tx.wall_tip.pt="Toque aqui no marcador que brilha na parede para abrir a sua história completa com fotografias e muito mais"', 'S.tx.wall_tip.pt="Toque aqui."', /^long: The “Tap a marker” has a line too long for 2 lines in Portuguese/]];
  const wres = [];
  for (const [nm, make, fix, re] of wcase) { await R(make); const a = await warns(); await R(fix); const c = await warns(); wres.push([nm, a.some(x => re.test(x)), !c.some(x => re.test(x))]); }
  // a line said twice: Duplicate (a real click), then the copy's words changed in its table (typed)
  await R(base); await editor({ gt4: true, 'gd:app': true, 'gd:g:wall': true, 'gd:i:askview': true });
  await p.locator('[data-gddup="askview"]').click(); await W(300);
  const dupId = await R('gdGroupById("wall").items[1]'); const d1 = await warns();
  await R(`S.op["gd:i:${dupId}"]=true;renderEditor()`);
  const ins = await R(`[].map.call(ed.querySelectorAll('[data-fo="gd:i:${dupId}"] [data-tx]'),function(x){return x.dataset.tx})`);
  for (const k of ins) { const l = p.locator(`[data-fo="gd:i:${dupId}"] [data-tx="${k}"]`); await l.scrollIntoViewIfNeeded(); await l.fill(k.endsWith('|pt') ? 'Pergunte-me o que quiser.' : 'Ask me anything.'); await l.press('Tab'); await W(250); }
  await W(400); const d2 = await R('[].map.call(ed.querySelectorAll("#gdwarns .gdwl"),function(x){return x.textContent})');
  wres.push(['a line said by two items', d1.some(x => /^dup: The “Ask about the view” says the same line as the “Ask about the view \(copy\)”/.test(x)), !d2.some(x => /same line/.test(x))]);
  // no words: the Blank template, then words typed
  await R(base + 'S.ui.gdAddIn="wall";'); await editor({ gt4: true, 'gd:app': true, 'gd:g:wall': true });
  await p.locator('[data-gdtpl="wall|blank"]').click(); await W(300);
  const bid = await R('var g=gdGroupById("wall");g.items[g.items.length-1]'); const n1 = await R('[].map.call(ed.querySelectorAll("#gdwarns .gdwl"),function(x){return x.textContent})');
  for (const k of await R(`[].map.call(ed.querySelectorAll('[data-fo="gd:i:${bid}"] [data-tx]'),function(x){return x.dataset.tx})`)) { const l = p.locator(`[data-fo="gd:i:${bid}"] [data-tx="${k}"]`); await l.scrollIntoViewIfNeeded(); await l.fill(k.endsWith('|pt') ? 'Olá.' : 'Hello there.'); await l.press('Tab'); await W(250); }
  await W(400); const n2 = await R('[].map.call(ed.querySelectorAll("#gdwarns .gdwl"),function(x){return x.textContent})');
  wres.push(['no words (Blank)', n1.some(x => /“New item” can never show: it has no words in English/.test(x)), !n2.some(x => /no words/.test(x))]);
  // round 11: Fun is held while a tour plays, so the two Moments (they come only between tour steps) are named as unable to show; nothing else
  ok('No warning with the catalog as it is, but the two Moments of round 11 (Fun held in a tour: they cannot show)', w0.length === 2 && w0.every(x => /Fun is held while a tour plays/.test(x)) && /Water break/.test(w0.join()) && /Cookie/.test(w0.join()), w0);
  ok('Each named warning appears, naming the item, and goes once fixed: kind Off, Only if contradicts itself, Done when already true, a line too long (Portuguese), a line said twice, no words', wres.every(x => x[1] && x[2]), wres);
  const badge = await R(`S.guidance.kinds[0].on=false;renderEditor();var s=ed.querySelector('[data-fo="gd:i:tapmarker"] > summary .gdwb'),n=ed.querySelector('[data-fo="gd:i:tapmarker"] .gdwarn');S.guidance.kinds[0].on=true;[!!s,n?n.textContent:""]`);
  ok('...and on the item itself: a ⚠ badge on its row, the warning in its form', badge[0] && /its kind, App, is Off/.test(badge[1]), badge);

  lap('before 12');
  // ---------- 12. + Add guidance: each template (real clicks) ----------
  const tpl = [];
  /* round 8: the Page template belongs to First pages only (test9l), never to The wall */
  for (const t of await R('GD_TEMPLATES.filter(function(x){return!x.page}).map(function(x){return[x.id,x.name]})')) {
    await R(base + 'S.ui.gdAddIn=null;S.ui.rowDemo=true;G.gd.manual=false;'); await editor({ gt4: true, 'gd:app': true, 'gd:g:wall': true });
    const ad = p.locator('[data-gdadd="wall"]'); await ad.scrollIntoViewIfNeeded(); await ad.click(); await W(150);
    const tb = p.locator(`[data-gdtpl="wall|${t[0]}"]`); const label = await tb.textContent({ timeout: 2500 }).catch(() => null); if (label == null) { tpl.push([t[1], null, []]); continue; } await tb.click(); await W(200);
    const nid = await R('var g=gdGroupById("wall");g.items[g.items.length-1]');
    await until(`!!G.gd.cur&&(G.gd.cur.item.base||G.gd.cur.item.id)==="${nid}"||/no words/.test(((ph.querySelector(".gdchip")||{}).textContent||""))||document.body.dataset.rowdemo.indexOf("${nid}")>=0`, 2500);
    const r = await R(`var it=S.guidance.items["${nid}"],T=GD_TEMPLATES.filter(function(x){return x.id==="${t[0]}"})[0];[it.group,it.kind,Object.keys(T.f).every(function(k){return JSON.stringify(it[k])===JSON.stringify(T.f[k])}),TXOWN[[].concat(it.says)[0]],S.tx[[].concat(it.says)[0]].en===T.en,!!ed.querySelector('[data-fo="gd:i:${nid}"]')&&ed.querySelector('[data-fo="gd:i:${nid}"]').open,(document.getElementById("gdedtoast")||{}).textContent||""]`);
    tpl.push([t[1], label, r]);
  }
  const offered = await R(`S.ui.gdAddIn="wall";renderEditor();var o=[].map.call(ed.querySelectorAll('[data-gdtpl^="wall|"]'),function(b){return b.dataset.gdtpl.slice(5)}).join(",");S.ui.gdAddIn=null;renderEditor();o`);
  ok('+ Add guidance offers the six templates (never the Page of First pages); each adds its item to the group (its kind), with the template\'s fields and words (owned by it), opens its form and says so', tpl.length === 6 && tpl.every(x => x[1] === x[0] && x[2][0] === 'wall' && x[2][1] === 'app' && x[2][2] && /^gd:i:gdn\d+$/.test(x[2][3]) && x[2][4] && x[2][5] && /^Added the/.test(x[2][6])) && offered === 'control,remark,tour,moment,step,blank', [offered, tpl]);
  await R('G.gd.manual=true;S.ui.rowDemo=false');

  lap('before 13');
  // ---------- 13. Duplicate, Delete and Undo (real clicks; no popup) ----------
  await R(base); await editor({ gt4: true, 'gd:app': true, 'gd:g:wall': true, 'gd:i:gbhint': true });
  await p.locator('[data-gddup="gbhint"]').click(); await W(250);
  const dp = await R('var g=gdGroupById("wall");[g.items.slice(),S.guidance.items[g.items[2]]&&S.guidance.items[g.items[2]].label]');
  const cid = dp[0][2];
  await R(`S.op["gd:i:${cid}"]=true;renderEditor()`); const dl = p.locator(`[data-gddel="${cid}"]`); await dl.scrollIntoViewIfNeeded(); await dl.click(); await W(250);
  const de = await R(`[gdGroupById("wall").items.indexOf("${cid}"),(document.getElementById("gdedtoast")||{}).textContent||"",!!document.querySelector("[role=alertdialog],dialog[open]")]`);
  await p.locator('#gdedtoast [data-gdedundo]').click(); await W(250);
  const un = await R(`[gdGroupById("wall").items.indexOf("${cid}"),!S.guidance.items["${cid}"].deleted]`);
  ok('Duplicate: “The ✦ (copy)” just below it', dp[0][1] === 'gbhint' && dp[1] === 'The ✦ (copy)', dp);
  ok('[undo] Delete: gone from its group at once, a toast “Deleted the …” with Undo, no popup; Undo puts it back in its place', de[0] === -1 && /^Deleted the “The ✦ \(copy\)”/.test(de[1]) && !de[2] && un[0] === 2 && un[1], [de, un]);

  lap('before 14');
  // ---------- 14. drag ≡ to reorder: the tie-break changes ----------
  await R(base + 'S.guidance.items.askview.notBefore=0;var c=gdDupItem("askview");S.tx[[].concat(c.says)[0]].en="Ask me anything.";window.__cp=c.id;');
  const tie = () => R('var V={visits:1,visit:{n:1,at:0,items:{opening:{shown:1,last:0,end:1000}},ev:{}},ever:{items:{opening:{shown:1}}},used:{}},C={on:true,breathing:15,onWall:true,where:"",gbtn:{name:"the ✦",x:40,y:700},marker:null,stillSince:0,tips:"all",aud:"explorer"};V.visit.lastEnd=1000;var r=gdPick(gdItems(),V,C,120000);[r.item&&r.item.id,gdGroupById("wall").items.join(",")]');
  const tb0 = await tie();
  await editor({ gt4: true, 'gd:app': true, 'gd:g:wall': true });
  const h = p.locator(`[data-gddrag="${await R('window.__cp')}"]`); await h.scrollIntoViewIfNeeded(); const hb = await h.boundingBox(); const ab = await p.locator('[data-gddrag="askview"]').boundingBox();
  await p.mouse.move(hb.x + hb.width / 2, hb.y + hb.height / 2); await p.mouse.down(); await p.mouse.move(hb.x + hb.width / 2, ab.y - 6, { steps: 8 });
  const mid = await R('[!!ed.querySelector(".fo.gddragging"),!!ed.querySelector(".gddropline")]'); await p.mouse.up(); await W(250);
  const tb1 = await tie();
  ok('Drag ≡ (a real drag, the row lifted and a line where it lands) puts the copy first in The wall: the tie between two equal tips now goes to it', tb0[0] === 'askview' && mid[0] && mid[1] && tb1[0] === await R('window.__cp') && tb1[1].indexOf(await R('window.__cp')) === 0, [tb0, mid, tb1]);
  await R(base + 'S.tab="guide";S.ui.level=1;S.op={gt4:true,"gd:app":true,"gd:g:start":true};renderEditor()');
  const sh = p.locator('[data-gddrag="tapmarker"]'); const sb = await sh.boundingBox(); const ob = await p.locator('[data-gddrag="opening"]').boundingBox();
  await p.mouse.move(sb.x + 8, sb.y + 10); await p.mouse.down(); await p.mouse.move(sb.x + 8, ob.y - 6, { steps: 8 }); await p.mouse.up(); await W(250);
  const seq = await R('var V={visits:1,visit:{n:1,at:0,items:{},ev:{}},ever:{items:{}},used:{}},C={on:true,breathing:15,onWall:true,where:"",stillSince:0,tips:"all",marker:{id:"painting",name:"Ribeira Palace",x:200,y:400}};[gdGroupById("start").items.join(","),gdWhyNot(gdItems(),V,C,0,"opening")]');
  ok('...in an In order group the drag is the sequence: Tap a marker first, the Opening message now waits for it', seq[0] === 'tapmarker,opening' && /after the Tap a marker \(not shown yet/.test(seq[1]), seq);

  lap('before 15');
  // ---------- 15. every new row's demo ----------
  await R(base); await editor({ gt4: true, 'gd:app': true, 'gd:g:start': true, 'gd:i:tapmarker': true, 'gd:g:card': true, 'gd:i:ba': true, 'gd:g:howto': true, 'gd:i:wk_bar': true });
  const NEW = /\.(group|spoken|about|mustSee|form|goes|points|focus|dim|gesture|button|when|event|stillFor|afterPrev|screen|aud|visitIs|hasUsed|notUsed|notSolemn|lively|notBefore|notAfter|teaches|afterDismiss|expires|onScreen|onScreenS)$/;
  const keys = await R(`var o=[];ed.querySelectorAll('[data-fo="gd:i:tapmarker"] .row,[data-fo="gd:i:ba"] .row,[data-fo="gd:i:wk_bar"] .row').forEach(function(r){var k=rowKey(r);if(k&&${NEW}.test(k)&&o.indexOf(k)<0)o.push(k)});o`);
  const demoMiss = [];
  for (const k of keys.concat(['gdVisitor.prefs.tips', 'gdVisitor.prefs.stay', 'ui.gdSR'])) {
    await R('document.body.dataset.rowdemo=""'); await R(`showRowDemo("${k}")`);
    let v = ''; const t0 = Date.now(); while (Date.now() - t0 < 4000) { v = await R('document.body.dataset.rowdemo||""'); if (v) break; await W(70); }
    if (!/\|found\|/.test(v)) demoMiss.push(k + ' ' + v);
  }
  ok(`Every new row's demo shows on the phone (${keys.length + 3} rows: the form's fields, Before / After's gesture, a step's Focus, Tips, Tips stay, Screen reader)`, demoMiss.length === 0 && keys.length >= 25, { demoMiss, keys: keys.length });
  await R('showRowDemo("guidance.items.tapmarker.focus")'); await W(900);
  const fd = await R('[G.gd.cur&&G.gd.cur.item.id,G.gd.cur&&G.gd.cur.pull,(ph.querySelector(".gdchip")||{}).textContent]');
  ok('...a field\'s demo shows the item on the phone with its value (Focus: Ring)', fd[0] === 'tapmarker' && fd[1] && /^Focus: Ring/.test(fd[2]), fd);
  const missDemo = await R(`var o=[];ed.querySelectorAll('[data-fo="gt4"] .row,[data-fo="gt0"] .row').forEach(function(r){var k=rowKey(r);if(k&&!rowDemoOf(k))o.push(k)});o`);
  ok('...and no row of 4 · Guidance or 0 · Preview & Test is without one', missDemo.length === 0, missDemo);

  lap('before 16');
  // ---------- 16. the four scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^gd6_/.test(x[1])})');
  ok('The console lists the four new scenes', scenes.length === 4, scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 30000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`Scene "${label}": Set up and ask runs it, ✓`, r && r.ok === true, r && [r.note, r.answer]);
    await R('gdStop("test");G.gd.rate=1');
  }
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log((fails ? `${fails} FAILED` : 'ALL PASS') + ` (${Math.round((Date.now() - t00) / 1000)} s)`);
  await b.close();
})();
