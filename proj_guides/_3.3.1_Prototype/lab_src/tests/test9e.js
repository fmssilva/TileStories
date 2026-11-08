// g1 checks (Playwright + the installed Edge): node test9e.js -> PASS/FAIL per check
// Guidance, round 1 (_3.8 section 8): the Guide tab (every former 6 · Guide row moved, the same rows); the scheduler's pick, each rule
// alone on a fake clock; the opening once a visit; Tap a marker only after the phone is still 1.2 s and 20 s after the opening, never
// while the wall is dragged, again after 25 s, at most 3 a visit, never after a card opens (this visit and the next, until Reset the
// visitor), never at the same time as Azu's remark; Mascot Off's plain bubble; Reduce Motion; the look (2 lines, 7:1, the ring 3:1 on
// the wall, 0 px over words, 44 px); the wall's pill gone and Global Scene's link; every Guidance row's demo (the ×10 chip); Show next
// now and the visitor; the five scenes in Ask Azu's console. MUTATE=1 node test9e.js runs it with a scheduler that ignores Done when:
// the named checks turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
// the former Detail Card > 6 · Guide, counted before the move (label|key of each row; the foldouts)
const BEFORE = { rows: ["Fixed rules|", "Voice|guideCfg.voice", "Speaking now|", "Speed|guideCfg.speed", "Music bed|guideCfg.music", "Voice in this browser|guideCfg.voice_en", "Recorded voice|guideCfg.voiceSet", "Line|", "Shown|guideCfg.heroButton", "Shown|guideCfg.block", "Panel|guideCfg.panel", "Starts at|guideCfg.dockStart", "Controls at Mini|guideCfg.miniCtl", "Swipe down to Compact|guideCfg.swipeCompact", "Stop|guideCfg.stopUndo", "Azu's own words|guideCfg.bubbles", "Talk button|guideCfg.talk", "Sounds|guideCfg.earcons", "Suggested questions|guideCfg.chips", "Try|", "Guide button|guideCfg.gbtn", "Icon|guideCfg.gbIcon", "Side|guideCfg.gbSide", "Label folds after 4 s|guideCfg.gbFold", "Shown|guideCfg.captions", "Highlight what is read|guideCfg.highlight", "Highlight style|guideCfg.hl", "Trail|guideCfg.trail", "Glide|guideCfg.glide", "Try|", "Focus|guideCfg.cue", "Dim the rest|guideCfg.scrim", "Into pictures|guideCfg.zoom", "Azu points|guideCfg.point", "Try|", "Shown|guideCfg.coach", "Insists|guideCfg.insist", "Waits|guideCfg.insistAfter", "When the visitor scrolls|guideCfg.onScroll", "Character|guideCfg.mascot", "Tap on Azu|guideCfg.azuTap", "Size|guideCfg.azuSize", "Liveliness|guideCfg.life", "Mouth|guideCfg.lips", "How it moves|", "Moods|guideCfg.moods", "Moments|guidance.kinds.2.groups.0.on", "Water break after|", "Try|", "Introduction|guideCfg.intro", "Mascot source|guideCfg.azuStyle", "Model|guideCfg.azuModel", "3D model|", "Play a clip|", "Import…|", "Generate from a picture|", "Walks to the wall|guideCfg.azuWalk", "Try|", "Answers from|guideCfg.ask", "Ask about the view|camCfg.on", "Suggestions|camCfg.chips", "Azu comments|guidance.kinds.1.groups.0.on", "Names on the points|camCfg.names", "Wall (the Lab)|camCfg.wall", "Try|", "Guide mode|guideCfg.ask", "Model|agentCfg.provider", "Gemini model|agentCfg.gemModel", "Claude API model|agentCfg.claudeModel", "Claude tier (published)|agentCfg.tier", "Longest answer|agentCfg.maxOut", "Cost|", "Text and voice|", "App context|agentCfg.ctx", "|", "Photo of the view|agentCfg.photo", "Targets|", "Library|", "General knowledge|agentCfg.general", "Trace under each answer|agentCfg.trace", "Spoken answer|agentCfg.voiceOut", "Voice of the answers|agentCfg.liveVoice", "Link cards|agentCfg.links", "Actions in the app|agentCfg.actions", "Paste a key|", "Test the key|", "Model steps|agentCfg.steps", "Timeout|agentCfg.timeout", "Questions a session|agentCfg.cap"],
  fos: ["s7voice", "s7vtest", "s7hero", "s7block", "s7dock", "s7gbtn", "s7cap", "s7focus", "s7coach", "s7tour", "s7resume", "s7mascot", "s7ask", "s7cam", "s7agent"] };
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('var __gc=gdCheck;gdCheck=function(it,items,v,ctx,now){var e=gdEverOf(v,it.id);if(e.done){var v2=JSON.parse(JSON.stringify(v));v2.ever.items[it.id].done=false;return __gc(it,items,v2,ctx,now)}return __gc(it,items,v,ctx,now)}'); console.log('MUTATION: the scheduler ignores Done when (a card opened no longer retires Tap a marker)'); }
  // the set-up: the Ribeira wall, a first visit, the clock moved by hand (deterministic), the remarks and the ✦ hint resting
  const base = 'S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.guidance.items.gbhint.on=false;gdGroupById("inview").on=false;S.guideCfg.gbtn="both";S.guideCfg.mascot=true;S.guidance.on=true;S.guidance.breathing=15;G.gd.demo=false;gdChip(null);';
  const fresh = async (extra) => { await R(base + 'if(G.on)stopTour();G.cam.open=false;S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);G.gd.manual=true;gdStop("test");gdResetVisitor();G.gd.movedAt=null;G.gd.onWall=false;' + (extra || '') + 'gdAdvance(250)'); await W(150); };
  const cur = () => R('G.gd.cur?[G.gd.cur.item.id,Math.round(G.gd.cur.at),G.gd.cur.text]:null');
  const adv = async ms => { await R('gdAdvance(' + ms + ')'); };
  // run the clock until the item shows (or ms of the visit pass): the visit time it started
  const runTill = async (id, ms, step) => R(`var t0=gdNow(),r=null;while(gdNow()-t0<${ms}){gdAdvance(${step || 250});if(G.gd.cur&&G.gd.cur.item.id==="${id}"){r=Math.round(G.gd.cur.at);break}}r`);

  // ---------- 1. the scheduler alone: gdPick on a fake clock, each rule by itself ----------
  const pk = (mod, now) => R(`var items=gdItems(),V={visits:1,visit:{n:1,at:0,lastEnd:null,items:{}},ever:{items:{}},used:{}},C={on:true,breathing:15,demo:false,showing:null,remark:null,speaking:false,panel:null,movedAt:null,onWall:true,where:"",marker:{id:"painting",name:"Ribeira Palace",x:200,y:400},stillSince:0,returning:false};
    var op=function(last,end){V.visit.items.opening={shown:1,last:last,end:end,dismissed:0};V.ever.items.opening={shown:1,done:false,by:""};V.visit.lastEnd=end};${mod || ''};var r=gdPick(items,V,C,${now});[r.item?r.item.id:null,r.why]`);
  const P = {};
  P.first = await pk('', 0);
  P.off = await pk('C.on=false', 0);
  P.showing = await pk('C.showing="Opening message"', 0);
  P.remark = await pk('C.remark="Azu\'s remark"', 0);
  P.speaking = await pk('C.speaking=true', 0);
  P.panel = await pk('C.panel="Settings"', 0);
  P.moving = await pk('C.movedAt=9700', 10000);
  P.moved = await pk('C.movedAt=9200', 10000);
  P.order = await pk('items[0].shows="once";V.ever.items.opening={shown:1,done:false,by:""}', 30000);
  P.after = await pk('op(0,9000)', 10000);
  P.gap = await pk('op(0,9000)', 30000);
  P.still = await pk('op(0,9000);C.stillSince=29000', 30000);
  P.stillOk = await pk('op(0,9000);C.stillSince=28700', 30000);
  P.breath = await pk('op(0,9000);V.visit.lastEnd=25000', 30000);
  P.essential = await pk('V.visit.lastEnd=-5000', 0);
  P.again = await pk('op(0,9000);V.visit.items.tapmarker={shown:1,last:24000,end:28000,dismissed:0};V.visit.lastEnd=10000', 40000);
  P.againOk = await pk('op(0,9000);V.visit.items.tapmarker={shown:1,last:24000,end:28000,dismissed:0};V.visit.lastEnd=10000', 53000);
  P.max = await pk('op(0,9000);V.visit.items.tapmarker={shown:3,last:24000,end:28000,dismissed:0}', 90000);
  P.done = await pk('op(0,9000);V.ever.items.tapmarker={shown:1,done:true,by:"card"}', 30000);
  P.marker = await pk('op(0,9000);C.marker=null', 30000);
  P.wall = await pk('op(0,9000);C.onWall=false;C.where="a card is open"', 30000);
  P.det = [await pk('op(0,9000)', 31234), await pk('op(0,9000)', 31234)];
  ok('pick: a first visit on the wall: the Opening message, Essential', P.first[0] === 'opening' && /Essential/.test(P.first[1]), P.first);
  ok('pick: the gates: Off, one at a time (a tip, Azu\'s remark), never over a sentence, a panel open', P.off[0] === null && /Off/.test(P.off[1]) && /one at a time/.test(P.showing[1]) && /Azu's remark/.test(P.remark[1]) && /never over a sentence/.test(P.speaking[1]) && /Settings/.test(P.panel[1]), [P.off, P.showing, P.remark, P.speaking, P.panel]);
  ok('pick: quiet while the phone moves and 0.7 s after (0.3 s: quiet; 0.8 s: the gate is open)', P.moving[0] === null && /moves/.test(P.moving[1]) && !/moves/.test(P.moved[1]), [P.moving, P.moved]);
  ok('pick: In order: Tap a marker waits for the Opening message', P.order[0] === null && /after the Opening message \(not shown yet/.test(P.order[1]), P.order);
  ok('pick: Tap a marker at least 20 s after the opening (10 s: no; 30 s: yes)', P.after[0] === null && /20 s after the Opening message: 10 s left/.test(P.after[1]) && P.gap[0] === 'tapmarker', [P.after, P.gap]);
  ok('pick: the phone still 1.2 s (1.0 s: no; 1.3 s: yes)', P.still[0] === null && /still 1.0 s/.test(P.still[1]) && P.stillOk[0] === 'tapmarker', [P.still, P.stillOk]);
  ok('pick: the breathing room 15 s (an Essential item skips it)', P.breath[0] === null && /breathing room 15 s: 10 s left/.test(P.breath[1]) && P.essential[0] === 'opening', [P.breath, P.essential]);
  ok('pick: again after 25 s (12 s after it ended: no; 25 s: yes)', P.again[0] === null && /again after 25 s: 13 s left/.test(P.again[1]) && P.againOk[0] === 'tapmarker', [P.again, P.againOk]);
  ok('pick: at most 3 a visit', P.max[0] === null && /shown 3 of 3 this visit/.test(P.max[1]), P.max);
  ok('pick [Done when]: a card opened retires Tap a marker for good', P.done[0] === null && /done for good/.test(P.done[1]), P.done);
  ok('pick: its target on screen (no marker in view) and its screen (not on the wall)', P.marker[0] === null && /no marker in view/.test(P.marker[1]) && P.wall[0] === null && /not on the wall/.test(P.wall[1]), [P.marker, P.wall]);
  ok('pick: deterministic (the same inputs, the same answer and why)', JSON.stringify(P.det[0]) === JSON.stringify(P.det[1]) && P.det[0][0] === 'tapmarker', P.det[0]);

  // ---------- 2. the opening, once a visit; its look ----------
  await fresh();
  const o1 = await cur();
  ok('The opening message on the wall at the visit\'s start, in the words of Languages & Terms', !!o1 && o1[0] === 'opening' && o1[2] === await R('TXT("gd_opening")'), o1);
  const CR = 'var px=function(c){var m=c.match(/[\\d.]+/g).map(Number);return{r:m[0],g:m[1],b:m[2],a:m.length>3?m[3]:1}},lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},cr=function(a,b){var x=lum(a),y=lum(b);return Math.round((Math.max(x,y)+.05)/(Math.min(x,y)+.05)*100)/100};';
  const look = 'var b=ph.querySelector(".gdbub"),bt=b.querySelector("button"),cs=getComputedStyle(bt),lh=parseFloat(cs.lineHeight),lines=Math.round((bt.offsetHeight-parseFloat(cs.paddingTop)-parseFloat(cs.paddingBottom))/lh),B=gdPR(b),g=ph.querySelector(".gbtn"),G1=g?gdPR(g):null,nb=document.getElementById("navbar"),N=nb&&gdSeen(nb)?gdPR(nb):null;' +
    '[lines,cr(px(cs.color),px(getComputedStyle(b).backgroundColor)),gdOverWords(),G1?gdOver(B,G1):0,N?gdOver(B,N):0,Math.round(bt.offsetHeight),b.dataset.tail]';
  for (const dark of [false, true]) {
    await R(`S.dark=${dark};renderAll(true)`); await W(200);
    const l = await R(CR + look);
    ok(`...its look, ${dark ? 'dark' : 'light'}: at most 2 lines, the words at 7:1 or more, 0 px over words, not under the ✦ or the bar, 44 px to tap, its tail to Azu`, l[0] <= 2 && l[1] >= 7 && l[2] === 0 && l[3] === 0 && l[4] === 0 && l[5] >= 44 && l[6] === 'down', l);
  }
  await R('S.dark=false;S.lang="pt";renderAll(true)'); await W(200);
  const lpt = await R(CR + look);
  ok('...in Portuguese too: at most 2 lines', lpt[0] <= 2 && lpt[2] === 0, lpt);
  await R('S.lang="en";renderAll(true)');
  await adv(10000); const o2 = await cur();
  await adv(60000);
  const oc = await R('[gdVisitOf(S.gdVisitor,"opening").shown,G.gd.log.filter(function(l){return l.what==="shown"&&/^Opening/.test(l.why)}).length]');
  ok('...once a visit: it ends by itself (9 s), and in the next 60 s it does not come again', o2 === null || o2[0] !== 'opening', [o2, oc]);
  ok('...shown once in this visit', oc[0] === 1, oc);

  // ---------- 3. Tap a marker: 20 s after the opening, the phone still 1.2 s, never while the wall is dragged ----------
  await fresh('S.guidance.breathing=5;'); const t0 = await R('gdVisitOf(S.gdVisitor,"opening").last');
  const tA = await runTill('tapmarker', 40000);
  ok('Tap a marker at least 20 s after the opening (breathing room 5 s, so the 20 s rule is the one that waits)', tA !== null && tA - t0 >= 20000 && tA - t0 <= 20500, [t0, tA]);
  await fresh('S.guidance.breathing=5;');
  await adv(19500); await R('emit("phone.moved")'); const mv = await R('Math.round(G.gd.movedAt)');
  const tB = await runTill('tapmarker', 10000);
  ok('...only after the phone has been still 1.2 s (moved at 19.5 s: it waits until 20.7 s)', tB !== null && tB - mv >= 1200 && tB - mv <= 1500, [mv, tB]);
  // a real drag on the wall (Playwright's mouse) while the visit's clock goes on: nothing while it moves
  await fresh('S.guidance.breathing=5;');
  await adv(19000);
  const ph = await p.locator('#ph').boundingBox(); let during = 0;
  await p.mouse.move(ph.x + 80, ph.y + 450); await p.mouse.down();
  for (let i = 0; i < 16; i++) { await p.mouse.move(ph.x + 80 + (i % 2 ? 6 : -6), ph.y + 450 + (i % 2 ? 4 : -4), { steps: 2 }); await adv(300); if (await R('!!G.gd.cur')) during++; }
  await p.mouse.up(); const lastMove = await R('Math.round(G.gd.movedAt)');
  const tC = await runTill('tapmarker', 8000);
  ok('Never while panning: a real drag on the wall for 4.8 s of the visit, no tip; then Tap a marker 1.2 s after the last move', during === 0 && tC !== null && tC - lastMove >= 1200, [during, lastMove, tC]);
  await W(700);
  const placed = await R(CR + 'var az=ph.querySelector(".azfab"),A=gdPR(az),M=gdMarker(),rg=ph.querySelector(".gdring"),RG=gdPR(rg),bub=gdPR(ph.querySelector(".gdbub")),mk=ph.querySelector("[data-marker] span"),MK=gdPR(mk);' +
    '[Math.round(Math.hypot((A.l+A.r)/2-M.x,(A.t+A.b)/2-M.y)),Math.round((RG.l+RG.r)/2-M.x),Math.round((RG.t+RG.b)/2-M.y),gdOver(bub,MK),gdOver(A,MK),gdOverWords(),!!ph.querySelector(".gdpoint"),az.classList.contains("gd-point"),Math.round(A.r-A.l),Math.round(A.b-A.t)]');
  ok('...Azu goes beside the nearest marker and points (the pointer), the ring is on the marker, nothing covers the marker or any words', placed[0] < 110 && Math.abs(placed[1]) <= 1 && Math.abs(placed[2]) <= 1 && placed[3] === 0 && placed[4] === 0 && placed[5] === 0 && placed[6] && placed[7], placed);
  ok('...Azu stays a 44 px target (or more) where it stands', placed[8] >= 44 && placed[9] >= 44, placed.slice(8));
  // the ring holds 3:1 against the wall's picture, outside and inside, at 16 points round it
  const ring = await R('new Promise(function(res){var cam=document.getElementById("camera"),bp=cam.style.backgroundPosition.split(" ").map(parseFloat),bs=cam.style.backgroundSize.split(" ").map(parseFloat),img=new Image();img.onload=function(){var c=document.createElement("canvas");c.width=ph.offsetWidth;c.height=ph.offsetHeight;var x=c.getContext("2d");x.drawImage(img,bp[0],bp[1],bs[0],bs[1]);var M=gdMarker(),worst=99;' +
    'var lum=function(r,g,b){return[r,g,b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},cr=function(a,b){return(Math.max(a,b)+.05)/(Math.min(a,b)+.05)};' +
    'for(var k=0;k<16;k++){var an=k/16*2*Math.PI;[39,30].forEach(function(rr){var d=x.getImageData(Math.round(M.x+Math.cos(an)*rr),Math.round(M.y+Math.sin(an)*rr),1,1).data,L=lum(d[0],d[1],d[2]),dk=lum(22*.88+d[0]*.12,24*.88+d[1]*.12,28*.88+d[2]*.12),w=lum(255,255,255);worst=Math.min(worst,Math.max(cr(w,L),cr(dk,L)))})}res(Math.round(worst*100)/100)};img.src=curWall().img()})');
  ok('...the ring holds 3:1 against the wall\'s picture all round (its white or its dark edge)', ring >= 3, ring);

  // ---------- 4. again after 25 s, at most 3 a visit ----------
  const seq = await R('var t0=gdNow();while(gdNow()-t0<240000){gdAdvance(250)}var lg=G.gd.log,vi=lg.map(function(l){return l.what}).lastIndexOf("visit");lg.slice(vi+1).filter(function(l){return/^Tap a marker/.test(l.why)&&(l.what==="shown"||l.what==="ended")}).map(function(l){return[l.what,l.t,l.why.slice(0,40)]})');
  const starts = seq.filter(s => s[0] === 'shown').map(s => s[1]), ends = seq.filter(s => s[0] === 'ended').map(s => s[1]);
  const gaps = starts.slice(1).map((s, i) => Math.round((s - ends[i]) / 1000));
  ok('Ignored, Tap a marker comes again after 25 s, at most 3 a visit (then not in the next 4 min)', starts.length === 3 && gaps.every(g => g >= 25), [starts, ends, gaps]);

  // ---------- 5. never after a card opens: this visit, and the next, until Reset the visitor ----------
  await fresh('S.guidance.breathing=5;');
  await runTill('tapmarker', 30000); await W(600);
  const mkb = await p.locator('#ph [data-marker] span').boundingBox(); await p.mouse.click(mkb.x + mkb.width / 2, mkb.y + mkb.height / 2); await W(300);
  const op1 = await R('[S.stop,!G.gd.cur,gdEverOf(S.gdVisitor,"tapmarker").done]');
  ok('A tap on the marker opens the card: the tip ends and Tap a marker is done', op1[0] !== 'closed' && op1[1] && op1[2], op1);
  await R('S.stop="closed";renderPhone(false)');
  const n1 = await runTill('tapmarker', 90000);
  ok('[Done when] ...never again in this visit (90 s on the wall)', n1 === null, n1);
  await R('gdNewVisit()'); await adv(500); const back = await cur();
  const n2 = await runTill('tapmarker', 90000);
  ok('...a new visit: the opening says Welcome back', !!back && back[0] === 'opening' && back[2] === await R('TXT("gd_opening_back")'), back);
  ok('[Done when] ...and Tap a marker does not come in the next visit either (90 s)', n2 === null, n2);
  await R('gdResetVisitor()'); const n3 = await runTill('tapmarker', 40000);
  ok('...Reset the visitor: it comes again', n3 !== null, n3);

  // ---------- 6. never at the same time as Azu's remark ----------
  // (guidance round 3: the remark is the guidance's What's in view, so one at a time is the scheduler's own gate; Tap a marker resting here)
  await fresh('gdGroupById("inview").on=true;S.guidance.items.tapmarker.on=false;'); await adv(2500);
  const r1 = await R('[G.gd.cur&&G.gd.cur.item.id,!!ph.querySelector(".gdbub.gd-more"),gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"inview:painting")]');
  ok('While a tip shows, Azu\'s remark waits (the point settled 2.5 s in the middle: no remark)', r1[0] === 'opening' && !r1[1] && /one at a time: the Opening message is showing/.test(r1[2]), r1);
  const r2 = await R('var t0=gdNow();while(gdNow()-t0<40000){gdAdvance(250);if(G.gd.cur&&G.gd.cur.item.base==="inview")break}var c=G.gd.cur,e=gdVisitOf(S.gdVisitor,"opening").end;[c&&c.item.base,!!ph.querySelector(".gdbub.gd-more"),gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"askview"),c&&e!=null?Math.round((c.at-e)/1000):null]');
  ok('...while Azu\'s remark shows, no tip starts (one at a time: the remark), and it came after the breathing room', r2[0] === 'inview' && r2[1] && /one at a time: the Ribeira Palace in the middle is showing/.test(r2[2]) && r2[3] >= 15, r2);
  await R('gdStop("test");gdGroupById("inview").on=false;S.guidance.items.tapmarker.on=true');

  // ---------- 7. Mascot Off: a plain bubble at the marker ----------
  for (const [nm, set] of [['Guide button = ✦ only', 'S.guideCfg.gbtn="star";'], ['Mascot > Character Off', 'S.guideCfg.mascot=false;']]) {
    await fresh('S.guidance.breathing=5;' + set + 'fabLayer();'); const tt = await runTill('tapmarker', 30000); await W(300);
    const m = await R('var b=ph.querySelector(".gdbub"),B=gdPR(b),M=gdMarker();[b.classList.contains("plain"),!G.gd.az,Math.round(Math.max(0,B.l-M.x,M.x-B.r)+Math.max(0,B.t-M.y,M.y-B.b)),b.dataset.tail,!!ph.querySelector(".gdring"),gdOverWords(),!ph.querySelector(".gdpoint")]');
    ok(`Mascot Off (${nm}): a plain bubble at the marker, its tail to it, the ring; no character, nothing over the words`, tt !== null && m[0] && m[1] && m[2] < 90 && m[3] !== 'none' && m[4] && m[5] === 0 && m[6], m);
  }
  await fresh('S.guideCfg.gbtn="star";fabLayer();'); const pl = await R('var b=ph.querySelector(".gdbub");[G.gd.cur&&G.gd.cur.item.id,b.classList.contains("plain"),b.dataset.tail]');
  ok('...the opening with Mascot Off: a plain bubble, no tail', pl[0] === 'opening' && pl[1] && pl[2] === 'none', pl);
  await R('S.guideCfg.gbtn="both";S.guideCfg.mascot=true;fabLayer()');

  // ---------- 8. Reduce Motion: no hop, a still ring, the words stay ----------
  await fresh('S.reduce=true;S.guidance.breathing=5;renderPhone(false);');
  await runTill('tapmarker', 30000);
  const rm = await R('var az=ph.querySelector(".azfab"),rg=ph.querySelector(".gdring"),b=ph.querySelector(".gdbub");[az.getAnimations().length,az.style.translate,rg.classList.contains("gd-pulse"),getComputedStyle(rg).animationName,b.classList.contains("gd-in"),b.textContent]');
  ok('Reduce Motion: Azu is beside the marker at once (no hop), the ring is still, the bubble has no motion, its words stay', rm[0] === 0 && /px/.test(rm[1]) && !rm[2] && rm[3] === 'none' && !rm[4] && rm[5] === 'Tap here.', rm);
  await R('S.reduce=false;renderPhone(false)');

  // ---------- 9. the wall's pill is gone; Global Scene links to the guidance ----------
  await R('gdStop("test");S.view="card";S.stop="closed";renderPhone(false)');
  const pill = await R('[!!ph.querySelector(".walltip"),document.getElementById("wall").innerHTML.indexOf("walltip")<0,S.wallTip===undefined]');
  ok('The wall hint\'s pill is gone (no .walltip on the wall, no wallTip state)', !pill[0] && pill[1] && pill[2], pill);
  await R('S.tab="global";S.ui.level=1;S.op={"g:11":true};renderEditor()');
  const g11 = await R('var f=ed.querySelector(\'[data-fo="g:11"]\'),bt=f&&f.querySelector("[data-goto]");[!!bt&&/Said by the guide: Guide › Guidance › The start/.test(bt.textContent),!f.querySelector(\'[data-set="@wallTip"],[data-set="wallTip"]\'),f.querySelectorAll(".row").length]');
  await p.locator('[data-fo="g:11"] [data-goto]').click(); await W(300);
  const went = await R('var g=ed.querySelector(\'[data-fo="gd:g:start"]\');[S.tab,!!g&&g.open,!!g&&g.getBoundingClientRect().top<window.innerHeight&&g.getBoundingClientRect().bottom>0]');
  ok('Global Scene > Onboarding hints: one link row, "Said by the guide: Guide › Guidance › The start", which opens it', g11[0] && g11[1] && g11[2] === 1 && went[0] === 'guide' && went[1] && went[2], [g11, went]);
  const words = await R('[TXOWN.wall_tip,TXOWN.gd_opening,TXOWN.gd_opening_back,S.tx.gd_opening.pt.length>10,S.tx.wall_tip.pt,ownerName("gd:i:tapmarker")]');
  ok('The words in Languages & Terms, owned by their items (Tap a marker reuses wall_tip), in each language', words[0] === 'gd:i:tapmarker' && words[1] === 'gd:i:opening' && words[2] === 'gd:i:opening' && words[3] && words[4] === 'Toque aqui.' && /Guide › Guidance › The start › Tap a marker/.test(words[5]), words);

  // ---------- 10. the Guide tab: its sections; every former 6 · Guide row, the same rows ----------
  await R('S.tab="guide";S.ui.level=9;S.op={};renderEditor()');
  const gt = await R('[[].map.call(ed.querySelectorAll(":scope > details[data-fo]"),function(d){return d.dataset.fo+"|"+d.querySelector("summary").textContent.split(" · ").slice(0,2).join(" · ")}),[].map.call(document.querySelectorAll(".utab"),function(t){return t.textContent})]');
  ok('The Guide tab, after Detail Card: 0 · Preview & Test, 1 · The tour, 2 · Mascot, 3 · Ask, 4 · Guidance', JSON.stringify(gt[0].map(s => s.split('|')[1])) === JSON.stringify(['0 · Preview & Test', '1 · The tour', '2 · Mascot', '3 · Ask', '4 · Guidance']) && gt[1].indexOf('Guide') === gt[1].indexOf('Detail Card') + 1, gt);
  const after = await R('var o=[];["gt1","gt2","gt3"].forEach(function(id){ed.querySelectorAll(\'[data-fo="\'+id+\'"] .row\').forEach(function(r){var lb=r.querySelector("label");o.push((lb?lb.textContent:"?")+"|"+(rowKey(r)||""))})});o');
  const sortJ = a => JSON.stringify(a.slice().sort());
  const fosIn = await R(`${JSON.stringify(BEFORE.fos)}.filter(function(id){return!ed.querySelector('[data-fo="'+id+'"]')})`);
  ok(`...it holds every former 6 · Guide row: ${BEFORE.rows.length} before, ${after.length} now, the same rows (and the ${BEFORE.fos.length} foldouts)`, sortJ(after) === sortJ(BEFORE.rows) && fosIn.length === 0, { missing: BEFORE.rows.filter(r => after.indexOf(r) < 0), extra: after.filter(r => BEFORE.rows.indexOf(r) < 0), fosIn });
  await R('S.tab="card";renderEditor()');
  const dc = await R('[!ed.querySelector(\'[data-fo^="s7"]\'),(ed.querySelector(\'[data-fo="s8"] > summary\')||{}).textContent.indexOf("6 · Audiences")===0]');
  ok('...the Detail Card has none of them, and 6 · Audiences stays there', dc[0] && dc[1], dc);
  // a Guide word's owner opens in the Guide tab (Languages & Terms' links)
  await R('goOwner("s7dock")'); const ow = await R('[S.tab,!!ed.querySelector(\'[data-fo="s7dock"]\')&&ed.querySelector(\'[data-fo="s7dock"]\').open,ed.querySelector(\'[data-fo="gt1"]\').open]');
  ok('...a word of the Guide panel opens its foldout in the Guide tab', ow[0] === 'guide' && ow[1] && ow[2], ow);
  // opening a section still shows its part (FOCUS): the Guide panel, Ask about the view, The start
  const fx = [];
  for (const [id, sel] of [['s7dock', '.gblock'], ['s7mascot', '.gblock'], ['gd:g:start', '[data-marker]']]) {
    await R(`S.tab="guide";S.ui.level=1;S.op={gt1:true,gt3:true,gt4:true,"gd:app":true};renderEditor();document.body.dataset.lastfocus=""`); await W(200);
    await R(`var d=ed.querySelector('[data-fo="${id}"]');d.open=true`); await W(500); fx.push([id, await R('document.body.dataset.lastfocus||""')]);
  }
  ok('...opening a section shows its part on the phone (FOCUS): Guide panel, Mascot, The start', fx.every(f => /\|found\|/.test(f[1])), fx);

  // ---------- 11. 4 · Guidance: the rows, their defaults, each row's demo ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;S.tab="guide";S.ui.level=9;S.op={};renderEditor()');
  const rows = await R('var o=[];ed.querySelectorAll(\'[data-fo="gt4"] .row, [data-fo="gt4"] summary [data-set]\').forEach(function(r){var k=r.matches(".row")?rowKey(r):r.dataset.set.slice(1);if(k&&o.indexOf(k)<0)o.push(k)});o');
  const defs = await R('var i=S.guidance.items;[S.guidance.on,S.guidance.breathing,i.opening.essential,i.opening.shows,i.tapmarker.shows,i.tapmarker.max,i.tapmarker.again,i.tapmarker.doneWhen,i.tapmarker.stillFor,i.tapmarker.afterPrev,ed.querySelector(\'[data-fo="gd:i:tapmarker"] .gdrb\').textContent]');
  ok('Guidance\'s defaults: On, breathing 15 s; Opening Essential once a visit; Tap a marker until done at most 3, again after 25 s, done when a card opens, still 1.2 s, 20 s after the opening; its read-back in words',
    JSON.stringify(defs.slice(0, 10)) === JSON.stringify([true, 15, true, 'visit', 'until', 3, 25, 'card', 1.2, 20]) && /at most about every 25 s, up to 3 times a visit, until the visitor opens a card/.test(defs[10]), defs);
  ok(`...every Guidance row has its demo (${rows.length} rows: Overall, each item's On, importance, Essential, Shows, At most, Again after, Done when)`, rows.length >= 14 && (await R(`${JSON.stringify(rows)}.filter(function(k){return!rowDemoOf(k)})`)).length === 0, rows);
  const shown = [], missing = [];
  for (const k of rows.filter(k => !/importance$/.test(k))) {
    await R('document.body.dataset.rowdemo=""');
    const lb = await R(`var c=ed.querySelector('[data-fo="gt4"] .row [data-set="@${k}"],[data-fo="gt4"] .row [data-num="@${k}"]'),l=c&&c.closest(".row").querySelector("label.rd");if(l){l.scrollIntoView({block:"center"});var r=l.getBoundingClientRect();[r.left+12,r.top+r.height/2]}else null`);
    if (!lb) { const s2 = await R(`showRowDemo("${k}")`); } else await p.mouse.click(lb[0], lb[1]);
    let v = ''; const t0 = Date.now(); while (Date.now() - t0 < 5000) { v = await R('document.body.dataset.rowdemo||""'); if (v) break; await W(80); }
    (/\|found\|/.test(v) ? shown : missing).push(k + ' ' + v.split('|').slice(1).join(' '));
  }
  ok('...a press on each Guidance row shows its demo on the phone: the tip (or its chip) ringed', missing.length === 0, { missing, shown: shown.length });
  // ▸ Show now on an item's row: the phone on the wall, the item now (pulled: the visitor's memory unchanged)
  await R('S.guidance.on=false;gdResetVisitor();S.stop="full";renderPhone(false)'); const mem0 = await R('JSON.stringify(S.gdVisitor.visit.items)');
  await p.locator('[data-gdshow="tapmarker"]').click(); await until('!!G.gd.cur', 2500); await W(700);
  const sn = await R('[S.stop,G.gd.cur&&G.gd.cur.item.id,G.gd.cur&&G.gd.cur.pull,!!ph.querySelector(".gdring"),JSON.stringify(S.gdVisitor.visit.items)]');
  await R('S.guidance.on=true');
  ok('▸ Show now on Tap a marker: the phone on the wall, the tip at the marker now; a demo writes no memory', sn[0] === 'closed' && sn[1] === 'tapmarker' && sn[2] && sn[3] && sn[4] === mem0, sn);
  // Again after: twice, with the ×10 countdown chip between
  await R('document.body.dataset.rowdemo="";var lb=ed.querySelector(\'[data-num="@guidance.items.tapmarker.again"]\').closest(".row").querySelector("label.rd");lb.click()');
  const chips = new Set(); let pulls = 0, wasOn = false; const t1 = Date.now();
  while (Date.now() - t1 < 12000) { const s = await R('[(ph.querySelector(".gdchip")||{}).textContent||"",!!(G.gd.cur&&G.gd.cur.pull)]'); if (s[0]) chips.add(s[0].replace(/\d+ s$/, 'n s')); if (s[1] && !wasOn) pulls++; wasOn = s[1]; await W(100); }
  ok('...Again after\'s demo: the tip, a ×10 countdown chip ("Again after 25 s"), the tip again, then how often in words', pulls === 2 && chips.has('×10 · Again after 25 s: n s') && [...chips].some(c => /at most about every 25 s/.test(c)), { pulls, chips: [...chips] });
  // the self-test fails a Guide tab row with no demo
  await R('on("edGuide.html",function(h){if(!window.__fake)return h;var d=document.createElement("div");d.innerHTML=h;addRows(d,"gd:overall",row("A row with no demo",pseg("guidance.noDemoRow",["a","b"],["A","B"]),""));return d.innerHTML});window.__fake=1;S.guidance.noDemoRow="a"');
  const st = await R('var j=extPoint("selfTest.res").filter(function(f){return String(f).indexOf("rowsWithoutDemo")>=0})[0],r=j([]);r.map(function(x){return x.tab+" "+x.key+" "+x.result.slice(0,7)})');
  await R('window.__fake=0;delete S.guidance.noDemoRow;renderEditor()');
  ok('The self-test fails a row of the Guide tab with no demo (NO DEMO)', st.length === 1 && /^guide @guidance\.noDemoRow NO DEMO/.test(st[0]), st);

  // ---------- 12. 0 · Preview & Test: Show next now and why; the visitor ----------
  await R(base + 'G.gd.manual=true;gdStop("test");gdResetVisitor();S.tab="guide";S.ui.level=1;S.op={gt0:true};renderEditor()');
  await p.locator('[data-gdnext]').click(); await W(200);
  const nx1 = await R('[G.gd.cur&&G.gd.cur.item.id,ed.querySelector("#gdwhy").textContent]');
  await p.locator('[data-gdnext]').click(); await W(200);
  const nx2 = await R('[G.gd.cur&&G.gd.cur.item.id,ed.querySelector("#gdwhy").textContent]');
  ok('Show next now: the opening and why; pressed again, nothing yet and why (Tap a marker waits 20 s after the opening)', nx1[0] === 'opening' && /^Next: Opening message: the visit started/.test(nx1[1]) && nx2[0] === null && /^Next: nothing\. .*Tap a marker: 20 s after the Opening message: \d+ s left/.test(nx2[1]), [nx1, nx2]);
  await p.locator('[data-gdvisit="new"]').click(); await W(200); const v1 = await R('ed.querySelector("#gdvisitor").textContent');
  await p.locator('[data-gdvisit="reset"]').click(); await W(200); const v2 = await R('ed.querySelector("#gdvisitor").textContent');
  ok('The visitor: New visit (a returning visitor) and Reset the visitor (a first visit), said in words', /^Visit 2 \(a returning visitor\)/.test(v1) && /^Visit 1 \(a first visit\) · Opening message: \d this visit/.test(v2), [v1, v2]);

  // ---------- 13. the scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^gd_/.test(x[1])})');
  ok('The console lists the guidance\'s five scenes (first visit, ignore it, open a card, panning, Mascot Off)', scenes.length === 5, scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    const does = await R('ed.querySelector(".agscenes").textContent');
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 40000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`${id === 'gd_card' ? '[Done when] ' : ''}Scene "${label}": Set up and ask runs it, ✓`, /Does: nothing:/.test(does) && r && r.ok === true, r && [r.note, r.answer]);
    await R('gdStop("test");G.gd.rate=1');
  }
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
