// g8 checks (Playwright + the installed Edge): node test9l.js -> PASS/FAIL per check
// Guidance, round 8 (_3.8 section 8): the first pages (the Page form), Start the app, Who's exploring's Where to ask. The scheduler's gate
// and the pages' data alone (pure); the run in order by real taps (Start the app on the preview bar, Hello, the language chip, Let's go,
// Who's exploring with Family setting the Family preset, the camera and the phone's request, the wall with the Opening message's short line,
// then Tap a marker as round 1); each Skip and skipping all; Show me how it works; the camera allowed, refused (never the wall), Try again,
// Open Settings; 44 px, 4.5:1 and 0 px over the words, light and dark; Where to ask's three values with the first card asking or not; a
// later launch (no pages, Welcome back) and Remember across visits Off (the pages again); Reduce Motion and Mascot Off; Start the app in
// Preview & Test and the visitor's line; + Add guidance's Page template and the drag within the rules; every new row's demo; the four
// scenes in Ask Azu's console. MUTATE=1 node test9l.js lets a refused camera go on to the wall: the checks named [camera] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const t00 = Date.now();
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  const lap = n => { if (process.env.TIMING) console.log('TIME ' + n + ' ' + Math.round((Date.now() - t00) / 1000) + ' s'); };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('var __ca=gdCamAnswer;gdCamAnswer=function(ok){if(!ok&&G.gd.pages){G.gd.camPerm="denied";gdPagesNext();return}return __ca(ok)}'); console.log('MUTATION: a refused camera goes on to the wall'); }
  await R('window.__g0=JSON.stringify(S.guidance)');
  const base = 'if(G.gd.pages)gdPagesEnd("test",false);S.guidance=JSON.parse(window.__g0);S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.ui.gdSR=false;S.ui.gdToday="2026-03-10";S.m.solemn=false;S.vis.aud="explorer";S.vis.len="";S.aud.where="card";S.aud.seen=true;S.guideCfg.life="lively";S.guideCfg.gbtn="both";S.guideCfg.mascot=true;G.gd.demo=false;G.gd.camPerm="allowed";gdChip(null);';
  /* the guidance's other kinds rest (only The start speaks), a first visit on the wall, the clock by hand */
  const fresh = async (extra) => { await R(base + 'gdGroupById("inview").on=false;S.guidance.items.gbhint.on=false;gdGroupById("card").on=false;gdKindById("fun").on=false;if(G.on)stopTour();stopSpeech();G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.camZoom=1;S.view="card";S.stop="closed";renderPhone(false);G.gd.manual=true;gdStop("test");gdResetVisitor();G.gd.movedAt=null;G.gd.touchAt=null;' + (extra || '')); await W(100); };
  const editor = async (tab, ops) => { await R(`S.tab="${tab}";S.ui.level=1;S.op=${JSON.stringify(ops)};renderEditor()`); await W(120); };
  const page = () => R('var L=ph.querySelector(".gdpages:not(.gd-out)");L?L.dataset.page+(L.dataset.cam?":"+L.dataset.cam:""):""');
  /* a real click on a page's control (a missing control: false, never a crash, so a broken run fails its checks) */
  const tap = async sel => { const ok1 = await p.locator('#ph .gdpages:not(.gd-out) ' + sel).first().click({ timeout: 2500 }).then(() => true, () => false); await W(80); return ok1; };
  const at = (pg, ms) => until(`(function(){var L=ph.querySelector(".gdpages:not(.gd-out)");return!!L&&L.dataset.page+(L.dataset.cam?":"+L.dataset.cam:"")==="${pg}"})()`, ms || 2500);
  const launch = async kind => { await R('S.previewBar=true;renderPhone(false)'); await p.locator(`#ptool [data-gdlaunch="${kind}"]`).click(); await W(120); };
  const opening = 'G.gd.cur&&!G.gd.cur.pull&&G.gd.cur.item.id==="opening"';

  lap('before 1');
  // ---------- 1. alone: the gate, the order's rules, the run's pages, the short line ----------
  const pure = await R(`var C={on:true,pages:"the first pages"},C2={on:true,pages:"a page shown by the Editor"},o=[gdGate(C,0),gdGate(C2,0),gdGate({on:true,pages:null,demo:false,tips:"all"},0)];
    o.push(gdPagesRule(["pg_hello","pg_aud","pg_camera","pg_wall"]),gdPagesRule(["pg_aud","pg_hello","pg_camera","pg_wall"]),gdPagesRule(["pg_hello","pg_wall","pg_camera"]));
    var run=function(w,cam,seen){S.aud.where=w;S.aud.seen=seen;G.gd.camPerm=cam;return gdPagesRun().join(",")};o.push(run("start","ask",false),run("card","ask",false),run("never","ask",false),run("start","allowed",false),run("start","ask",true));
    S.guidance.items.pg_camera.on=false;o.push(run("card","ask",false));S.guidance.items.pg_camera.on=true;gdGroupById("pages").on=false;o.push(run("card","ask",false),run("card","allowed",false));gdGroupById("pages").on=true;
    var op=S.guidance.items.opening;o.push(gdLineKey(op,false,{pagesLine:"gd_pg_wall"}),gdLineKey(op,true,{pagesLine:"gd_pg_wall"}),gdLineKey(op,false,{}));
    S.aud.where="start";o.push(gdPagesSecs(gdPagesPlan()),gdPagesPlan().map(function(x){return x.id}).join(","));S.aud.where="card";G.gd.camPerm="allowed";S.aud.seen=true;o`);
  ok('The scheduler is quiet while the first pages run (a gate, in words; a page the Editor shows too); none otherwise', /quiet while the first pages run \(the app's start\)/.test(pure[0]) && /quiet while a page shown by the Editor shows/.test(pure[1]) && pure[2] === null, pure.slice(0, 3));
  ok('The order\'s ends: Hello stays first, Straight to the wall last (in words); the default order keeps them', pure[3] === '' && /^Hello stays first/.test(pure[4]) && /^Straight to the wall stays last/.test(pure[5]), pure.slice(3, 6));
  ok('A first run\'s pages: Who\'s exploring only when asked at the app\'s start (not on the first card, not Never, not once answered); the camera only while not allowed; its page Off: the phone\'s request alone; the group Off: the request only when needed',
    pure[6] === 'pg_hello,pg_aud,pg_camera' && pure[7] === 'pg_hello,pg_camera' && pure[8] === 'pg_hello,pg_camera' && pure[9] === 'pg_hello,pg_aud' && pure[10] === 'pg_hello,pg_camera' && pure[11] === 'pg_hello,pg_camera:direct' && pure[12] === 'pg_camera:direct' && pure[13] === '', pure.slice(6, 14));
  ok('The Opening message right after the pages says Straight to the wall\'s short line (a returning visitor: Welcome back; otherwise its own line)', pure[14] === 'gd_pg_wall' && pure[15] === 'gd_opening_back' && pure[16] === 'gd_opening', pure.slice(14, 17));
  ok('The whole run takes under 30 s (Hello, Who\'s exploring, the camera: read at 4 words a second, 1.5 s to act)', pure[17] > 5 && pure[17] < 30 && pure[18] === 'pg_hello,pg_aud,pg_camera', pure.slice(17));

  lap('before 2');
  // ---------- 2. the run in order, by real taps: Start the app, Hello, the chip, Let's go, Family, the camera, the wall ----------
  await fresh('S.aud.where="start";');
  await launch('first');
  const r1 = await R('[!!ph.querySelector(".gdpages"),S.gdVisitor.visits,G.gd.camPerm,S.aud.seen,(ph.querySelector(".gdpages .gdpg-t")||{}).textContent,(ph.querySelector(".gdpages .gdpg-l")||{}).textContent,ph.querySelectorAll(".gdpages .gdpg-dots i").length,[].map.call(ph.querySelectorAll(".gdpages .gdpg-foot button"),function(b){return b.textContent}).join("|")]');
  ok('Start the app (the preview bar, a real click): a first visit, the camera not yet allowed, the audience not yet asked; Hello first: “Olá!”, its line, 3 steps, Let\'s go, Show me how it works, Skip',
    r1[0] && r1[1] === 1 && r1[2] === 'ask' && r1[3] === false && r1[4] === 'Olá!' && r1[5] === "I'm your guide to Lisbon's tiles." && r1[6] === 3 && r1[7] === "Let's go|Show me how it works|Skip", r1);
  await R('gdAdvance(3000)');
  ok('...the scheduler quiet meanwhile (3 s of the visit: no tip; Why: the first pages)', await R('!G.gd.cur&&!!G.gd.last&&/quiet while the first pages run/.test(G.gd.last.why)'), await R('G.gd.last&&G.gd.last.why'));
  await tap('[data-gdpg="lang:pt"]');
  const pt = await R('[S.lang,(ph.querySelector(".gdpages .gdpg-l")||{}).textContent,(ph.querySelector(".gdpages .gdpg-pri")||{}).textContent,(ph.querySelector(\'.gdpages [data-gdpg="lang:pt"]\')||{}).getAttribute("aria-pressed"),(document.getElementById("ptool")||{}).textContent]');
  await tap('[data-gdpg="lang:en"]');
  ok('The language chip (a real click on PT): the app\'s language is Portuguese at once (the page, its buttons, the app around it); EN back', pt[0] === 'pt' && pt[1] === 'Sou o seu guia dos azulejos de Lisboa.' && pt[2] === 'Vamos lá' && pt[3] === 'true' && /Iniciar a app/.test(pt[4]) && await R('S.lang') === 'en', pt);
  await tap('[data-gdpg="next"]'); const a1 = await at('aud');
  const r2 = await R('[!!ph.querySelector(".gdpages .gdpg-stage .onb.onb-start"),ph.querySelectorAll(".gdpages .onb [data-onb]").length,(ph.querySelector(".gdpages .onb-t")||{}).textContent,ph.querySelectorAll(".onb").length,(ph.querySelector(".gdpages .gdpg-dots i.on")?[].indexOf.call(ph.querySelectorAll(".gdpages .gdpg-dots i"),ph.querySelector(".gdpages .gdpg-dots i.on")):-1)]');
  ok('Let\'s go (a real click): Who\'s exploring, the first card\'s own screen in the pages (one home: three choices and Skip), step 2 of 3', a1 && r2[0] && r2[1] === 4 && r2[2] === "Who's exploring today?" && r2[3] === 1 && r2[4] === 1, r2);
  await p.locator('#ph .gdpages .onb [data-onb="family"]').click(); const a2 = await at('camera');
  const r3 = await R('[S.vis.aud,S.aud.seen,gdPresetNow(),S.guidance.breathing,(ph.querySelector(".gdpages .gdpg-t")||{}).textContent,(ph.querySelector(".gdpages .gdpg-l")||{}).textContent,(ph.querySelector(".gdpages .gdpg-pri")||{}).textContent]');
  ok('Family (a real click) sets the audience, and with it the guidance preset (Family: 12 s between two tips); then the camera: its title, “I need the camera to see the tiles. Nothing is recorded.”, Allow the camera',
    a2 && r3[0] === 'family' && r3[1] === true && r3[2] === 'family' && r3[3] === 12 && r3[4] === 'Let me see the tiles' && r3[5] === 'I need the camera to see the tiles. Nothing is recorded.' && r3[6] === 'Allow the camera', r3);
  await tap('[data-gdpg="camera"]'); const a3 = await at('camera:prompt');
  const r4 = await R('[!!ph.querySelector(".gdpages .gdpg-alert"),(ph.querySelector(".gdpages .gdpg-alert p")||{}).textContent,(ph.querySelector(".gdpages .gdpg-standin")||{}).textContent,G.gd.camPerm]');
  ok('Allow the camera (a real click): the phone\'s own request (a stand-in, said so), with the app\'s line', a3 && r4[0] && r4[1] === 'To see the tiles you point at. Nothing is recorded.' && /stand-in/.test(r4[2]) && r4[3] === 'ask', r4);
  await tap('[data-gdpg="sys:allow"]'); const gone = await until('!ph.querySelector(".gdpages:not(.gd-out)")', 1500);
  await R('gdAdvance(1000)');
  const r5 = await R(`[G.gd.camPerm,${opening},G.gd.cur&&G.gd.cur.text,!!S.gdVisitor.pages,S.gdVisitor.visit.pages,S.view,S.stop]`);
  ok('Allow: the pages end on the wall; the Opening message says the short line “Point at the tiles and tap a glowing marker.”; the visitor\'s memory says the pages were seen',
    gone && r5[0] === 'allowed' && r5[1] && r5[2] === 'Point at the tiles and tap a glowing marker.' && r5[3] && r5[4] === true && r5[5] === 'card' && r5[6] === 'closed', r5);
  const log = await R('G.gd.log.filter(function(l){return l.what==="event"&&/first pages/.test(l.why)}).map(function(l){return l.why})');
  ok('The run in order (the Log): Hello → Who\'s exploring → The camera → the wall', /Hello → Who's exploring → The camera → the wall/.test(log[0] || '') && log.some(l => /Hello done/.test(l)) && log.some(l => /The camera done/.test(l)), log);
  await R('gdAdvance(9000);gdStop("test");G.gd.movedAt=null;for(var i=0;i<100&&!(G.gd.cur&&G.gd.cur.item.id==="tapmarker");i++)gdAdvance(250)');
  const tm = await R('[G.gd.cur&&G.gd.cur.item.id,Math.round(((G.gd.cur&&G.gd.cur.at)-gdVisitOf(S.gdVisitor,"opening").last)/1000)]');
  ok('Then Tap a marker, as round 1 does (at least 20 s after the Opening message, beside the nearest marker)', tm[0] === 'tapmarker' && tm[1] >= 20, tm);
  await R('S.view="card";S.stop="half";renderPhone(false)');
  ok('Asked at the app\'s start: the first card does not ask again', await R('!ph.querySelector(".onb")'));

  lap('before 3');
  // ---------- 3. each Skip, and skipping all ----------
  await fresh('S.aud.where="start";'); await launch('first');
  await tap('[data-gdpg="skip"]'); const s1 = await at('camera:prompt');
  const sk = await R('[S.aud.seen,S.vis.aud,!ph.querySelector(".gdpages .onb"),!ph.querySelector(\'.gdpages [data-gdpg="skip"]\')]');
  await tap('[data-gdpg="sys:allow"]'); await until('!ph.querySelector(".gdpages:not(.gd-out)")', 1500); await R('gdAdvance(1000)');
  const sk2 = await R(`[${opening},!!(S.gdVisitor.pages&&S.gdVisitor.pages.skipped)]`); await R('S.view="card";S.stop="half";renderPhone(false)'); const sk3 = await R('!ph.querySelector(".onb")');
  ok('Skip on Hello (a real click) skips all the rest: no Who\'s exploring (Explorer, answered), straight to the phone\'s camera request (never the wall without it); Allow: the wall and the Opening message; the first card asks nothing', s1 && sk[0] && sk[1] === 'explorer' && sk[2] && sk[3] && sk2[0] && sk2[1] && sk3, [s1, sk, sk2, sk3]);
  await fresh('S.aud.where="start";'); await launch('first'); await tap('[data-gdpg="next"]'); await at('aud');
  await p.locator('#ph .gdpages .onb [data-onb="skip"]').click(); const s2 = await at('camera'); const s2b = await R('[S.vis.aud,S.aud.seen]');
  await tap('[data-gdpg="skip"]'); const s3 = await at('camera:prompt');
  ok('Who\'s exploring\'s own Skip (a real click): Explorer, the next page; Skip on the camera\'s page: straight to the phone\'s request', s2 && s2b[0] === 'explorer' && s2b[1] && s3, [s2, s2b, s3]);
  await R('gdCamAnswer(true)');
  await fresh('S.guidance.remember=false;'); await launch('later'); await at('hello'); await tap('[data-gdpg="skip"]');
  const s4 = await until('!ph.querySelector(".gdpages:not(.gd-out)")', 1500);
  ok('Skip with the camera already allowed (a later launch, Remember across visits Off): straight to the wall', s4 && await R('G.gd.camPerm==="allowed"&&!!S.gdVisitor.pages'));

  lap('before 4');
  // ---------- 4. Show me how it works ----------
  await fresh(); await launch('first'); await tap('[data-gdpg="how"]'); await at('camera'); await tap('[data-gdpg="camera"]'); await at('camera:prompt'); await tap('[data-gdpg="sys:allow"]');
  await until('!ph.querySelector(".gdpages:not(.gd-out)")', 1500); await R('gdAdvance(500)');
  const hw = await R('[G.gd.cur&&G.gd.cur.item.id,G.gd.cur&&G.gd.cur.pull,!!ph.querySelector(".gdbub:not(.gd-out) [data-gdcta]"),gdVisitOf(S.gdVisitor,"opening").shown,!!(S.gdVisitor.pages&&S.gdVisitor.pages.how)]');
  ok('Show me how it works (a real click): after the pages, How to use the card\'s bubble on the wall (with Open a card); it says the visit\'s first words (the Opening message counted, not said twice)', hw[0] === 'howto:wall' && hw[1] && hw[2] && hw[3] === 1 && hw[4], hw);

  lap('before 5');
  // ---------- 5. the camera: refused, never the wall; Try again; Open Settings; the look ----------
  await fresh(); await launch('first'); await tap('[data-gdpg="next"]'); await at('camera'); await tap('[data-gdpg="camera"]'); await at('camera:prompt');
  await tap('[data-gdpg="sys:deny"]'); const c1 = await at('camera:refused');
  const cr = await R('[G.gd.camPerm,(ph.querySelector(".gdpages .gdpg-t")||{}).textContent,(ph.querySelector(".gdpages .gdpg-l")||{}).textContent,[].map.call(ph.querySelectorAll(".gdpages .gdpg-foot button"),function(b){return b.dataset.gdpg}).join(",")]');
  ok('[camera] Don\'t Allow (a real click): “The camera is off”, how to allow it, Open Settings and Try again, no Skip', c1 && cr[0] === 'denied' && cr[1] === 'The camera is off' && /Settings › Camera/.test(cr[2]) && cr[3] === 'settings,retry', cr);
  await R('gdAdvance(30000)');
  ok('[camera] Never the wall without the camera: 30 s later still the camera\'s page, no tip, no Opening message', await R('!!ph.querySelector(".gdpages:not(.gd-out)")&&!G.gd.cur&&!S.gdVisitor.visit.pages&&gdVisitOf(S.gdVisitor,"opening").shown===0'));
  await tap('[data-gdpg="retry"]'); const c2 = await at('camera:prompt'); await tap('[data-gdpg="sys:deny"]'); const c3 = await at('camera:refused');
  ok('[camera] Try again (a real click): the phone\'s request again; refused again, the same page', c2 && c3);
  await tap('[data-gdpg="settings"]'); const c4 = await at('camera:settings');
  const sw0 = await R('var w=ph.querySelector(".gdpages .gdpg-sw");w?w.getAttribute("aria-checked"):null');
  await tap('[data-gdpg="back"]'); const c5 = await at('camera:refused');
  await tap('[data-gdpg="settings"]'); await at('camera:settings'); await tap('[data-gdpg="switch"]');
  const sw1 = await R('var w=ph.querySelector(".gdpages .gdpg-sw");[w?w.getAttribute("aria-checked"):null,G.gd.camPerm]');
  await tap('[data-gdpg="back"]'); const c6 = await until('!ph.querySelector(".gdpages:not(.gd-out)")', 1500); await R('gdAdvance(1000)');
  ok('[camera] Open Settings (a real click): the phone\'s Settings (a stand-in), Camera Off; Back with it Off: refused still; the switch On and Back: the wall and the Opening message', c4 && sw0 === 'false' && c5 && sw1[0] === 'true' && sw1[1] === 'allowed' && c6 && await R(opening), [c4, sw0, c5, sw1, c6]);
  // the look, light and dark: 44 px targets, 4.5:1, 0 px over the words
  const LOOK = `(function(){var px=function(c){var m=c.match(/[\\d.]+/g).map(Number),k=c.indexOf("color(")===0?255:1;return{r:m[0]*k,g:m[1]*k,b:m[2]*k,a:m.length>3?m[3]:1}},lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},over=function(f,b){return{r:f.r*f.a+b.r*(1-f.a),g:f.g*f.a+b.g*(1-f.a),b:f.b*f.a+b.b*(1-f.a),a:1}},
    bg=function(el){var st=[];while(el){var c=px(getComputedStyle(el).backgroundColor);if(c.a>0)st.push(c);if(c.a>=1)break;el=el.parentElement}var b={r:255,g:255,b:255,a:1};for(var i=st.length-1;i>=0;i--)b=over(st[i],b);return b},L=ph.querySelector(".gdpages:not(.gd-out)"),min=99,small=[],ov=0;if(!L)return[0,["no page"],-1];
    L.querySelectorAll(".gdpg-t,.gdpg-l,.gdpg-b,.gdpg-skip,.gdpg-lang button,.gdpg-alert b,.gdpg-alert p,.gdpg-abtn button,.gdpg-standin,.gdpg-setback,.gdpg-setrow span").forEach(function(e){var a=lum(px(getComputedStyle(e).color)),z=lum(bg(e)),r=(Math.max(a,z)+.05)/(Math.min(a,z)+.05);if(r<min)min=r});
    L.querySelectorAll("button").forEach(function(e){var r=e.getBoundingClientRect(),q=ph.getBoundingClientRect().height/ph.offsetHeight;if(r.height/q<44-0.5||r.width/q<44-0.5)small.push((e.dataset.gdpg||e.className)+" "+Math.round(r.width/q)+"x"+Math.round(r.height/q))});
    var els=[].slice.call(L.querySelectorAll(".gdpg-t,.gdpg-l,.gdpg-b,.gdpg-skip,.gdpg-lang,.gdpg-dots,.gdpg-art,.gdpg-cam"));for(var i=0;i<els.length;i++)for(var j=i+1;j<els.length;j++){var a=els[i].getBoundingClientRect(),c=els[j].getBoundingClientRect(),x=Math.min(a.right,c.right)-Math.max(a.left,c.left),y=Math.min(a.bottom,c.bottom)-Math.max(a.top,c.top);if(x>0.5&&y>0.5&&!els[i].contains(els[j])&&!els[j].contains(els[i]))ov+=x*y}
    return[Math.round(min*100)/100,small,Math.round(ov)]})()`;
  const looks = [];
  for (const dark of [false, true]) {
    for (const st of ['hello', 'camera', 'prompt', 'refused', 'settings']) {
      await fresh(`S.dark=${dark};renderAll(true);`); await R('gdLaunch("first")'); await at('hello');
      if (st !== 'hello') { await R('gdPagesNext()'); await at('camera'); }
      if (st === 'prompt' || st === 'refused' || st === 'settings') { await R('gdCamAsk()'); }
      if (st === 'refused' || st === 'settings') { await R('gdCamAnswer(false)'); }
      if (st === 'settings') { await R('gdCamSettings()'); }
      await W(450); looks.push([dark ? 'dark' : 'light', st].concat(await R(LOOK)));
    }
  }
  ok('The look, light and dark (Hello, the camera, its request, refused, Settings): words 4.5:1 or more, every button 44 px or more, 0 px over the words', looks.every(l => l[2] >= 4.5 && l[3].length === 0 && l[4] === 0), looks);

  lap('before 6');
  // ---------- 6. Where to ask: each of the three, the first card asking or not (real clicks on the row) ----------
  await fresh(); await R('S.ui.rowDemo=true'); await editor('card', { s8: true, s8onb: true });
  const where = {};
  for (const w of ['start', 'card', 'never']) {
    await R('document.body.dataset.rowdemo=""'); await p.locator(`[data-set="@aud.where"][data-v="${w}"]`).click();
    await until('/\\|(found|missing)\\|/.test(document.body.dataset.rowdemo||"")', 3000); await W(200);
    where[w] = await R('[S.aud.where,document.body.dataset.rowdemo,!!ph.querySelector(".gdpages .onb-start"),!!ph.querySelector(".onb:not(.onb-start)"),(ph.querySelector(".gdchip")||{}).textContent||""]');
    await R('if(G.gd.pages)gdPagesEnd("test",false)');
    await R(`gdStop("test");S.ui.rowDemo=false;G.gd.camPerm="allowed";gdLaunch("first");`); if (await page()) { await R('while(G.gd.pages){var n=gdPageNow();if(n.it&&n.it.page==="aud"){window.__askedStart=1;S.vis.aud="expert";S.aud.seen=true;emit("audience.asked","expert",false)}else gdPagesNext()}'); }
    await R('S.view="card";S.stop="half";renderPhone(false)'); where[w].push(await R('!!window.__askedStart'), await R('!!ph.querySelector(".onb:not(.onb-start)")'), await R('S.vis.aud'), await R('gdPresetNow()'));
    await R('window.__askedStart=0;S.stop="closed";S.ui.rowDemo=true;renderPhone(false)');
  }
  ok('Where to ask = At the app\'s start (a real click): its demo shows Who\'s exploring in the first pages; a first run asks it there (Expert: the Expert preset) and the first card does not', where.start[0] === 'start' && /\|found\|/.test(where.start[1]) && where.start[2] && where.start[5] && !where.start[6] && where.start[7] === 'expert' && where.start[8] === 'expert', where.start);
  ok('Where to ask = On the first card (today\'s, the default): its demo shows the card\'s screen; a first run does not ask, the first card does', where.card[0] === 'card' && where.card[3] && !where.card[5] && where.card[6], where.card);
  ok('Where to ask = Never: its demo says so; neither asks (Explorer)', where.never[0] === 'never' && /Never/.test(where.never[4]) && !where.never[5] && !where.never[6] && where.never[7] === 'explorer', where.never);
  await R('S.ui.rowDemo=false;S.aud.where="card";S.aud.seen=true');

  lap('before 7');
  // ---------- 7. a later launch; Remember across visits Off ----------
  await fresh(); await R('gdLaunch("first");gdPagesSkip();gdCamAnswer(true);gdAdvance(9000);gdStop("test")'); await W(250);
  await launch('later'); await R('gdAdvance(1000)');
  const lt = await R(`[!ph.querySelector(".gdpages:not(.gd-out)"),S.gdVisitor.visits,${opening},G.gd.cur&&G.gd.cur.text]`);
  ok('Later launch (the preview bar, a real click): no pages; the Opening message says “Welcome back! Tap a marker to begin.”', lt[0] && lt[1] === 2 && lt[2] && lt[3] === 'Welcome back! Tap a marker to begin.', lt);
  await R('gdStop("test");S.guidance.remember=false'); await launch('later'); const lt2 = await at('hello');
  ok('Remember across visits Off: every launch is a first: the pages again, from Hello (a first visit)', lt2 && await R('S.gdVisitor.visits===1&&!S.gdVisitor.pages'));

  lap('before 8');
  // ---------- 8. Reduce Motion and Mascot Off ----------
  await fresh(); await R('gdLaunch("first")'); await at('hello');
  const mo = await R('var a=ph.querySelector(".gdpages .gdpg-art");[a&&a.dataset.azu,a?a.querySelector(".gdpg-tile").getAnimations().length:0,a?a.querySelector(".gdpg-azu").getAnimations().length:0,!!ph.querySelector(".gdpages .gdpg-azu img,.gdpages .gdpg-azu svg")]');
  await fresh('S.reduce=true;renderPhone(false);'); await R('gdLaunch("first")'); await at('hello');
  const rd = await R('var a=ph.querySelector(".gdpages .gdpg-art");[a&&a.dataset.azu,a?getComputedStyle(a.querySelector(".gdpg-tile")).opacity:"",a?a.querySelector(".gdpg-azu").getAnimations().length:-1,ph.querySelector(".gdpages .gdpg-body").getAnimations().length,(ph.querySelector(".gdpages .gdpg-t")||{}).textContent]');
  await fresh('S.guideCfg.mascot=false;'); await R('gdLaunch("first")'); await at('hello');
  const mf = await R('[!ph.querySelector(".gdpages .gdpg-art"),(ph.querySelector(".gdpages .gdpg-t")||{}).textContent,ph.querySelectorAll(".gdpages .gdpg-foot button").length]');
  ok('Hello: Azu forms from a tile (the tile turns in and dissolves) and waves (its animations run)', mo[0] === 'form' && mo[1] >= 1 && mo[2] >= 2 && mo[3], mo);
  ok('Reduce Motion: Azu appears (no tile, no wave, no entrance); the words stay', rd[0] === 'still' && rd[1] === '0' && rd[2] === 0 && rd[3] === 0 && rd[4] === 'Olá!', rd);
  ok('Mascot Off: Hello without Azu, its words and buttons stay', mf[0] && mf[1] === 'Olá!' && mf[2] === 3, mf);

  lap('before 9');
  // ---------- 9. Start the app in Preview & Test, and the visitor's line ----------
  await fresh(); await R('S.ui.rowDemo=true'); await editor('guide', { gt0: true });
  const v0 = await R('ed.querySelector("#gdpgseen").textContent');
  await p.locator('[data-set="@ui.gdLaunch"][data-v="first"]').click(); const pv1 = await at('hello', 3000);
  const v1 = await R('ed.querySelector("#gdpgseen").textContent');
  await R('gdPagesSkip();gdCamAnswer(true)'); await W(150); const v2 = await R('ed.querySelector("#gdpgseen").textContent');
  await p.locator('[data-set="@ui.gdLaunch"][data-v="later"]').click(); await until('/\\|(found|missing)\\|/.test(document.body.dataset.rowdemo||"")', 3000);
  const pv2 = await R('[!ph.querySelector(".gdpages:not(.gd-out)"),S.gdVisitor.visits,/Welcome back/.test((ph.querySelector(".gdchip")||{}).textContent||"")]');
  ok('Preview & Test › Start the app (real clicks): a first run (the pages from Hello) and a later launch (no pages, Welcome back); The visitor says whether the first pages were seen',
    /not seen yet/.test(v0) && pv1 && /running now, Hello/.test(v1) && /seen at visit 1, skipped/.test(v2) && pv2[0] && pv2[1] === 2 && pv2[2], [v0, v1, v2, pv2]);
  await R('S.ui.rowDemo=false;gdChip(null)');

  lap('before 10');
  // ---------- 10. the Editor: Sequences, + Add guidance's Page template, the drag within the rules ----------
  await fresh(); await editor('guide', { gt4: true, 'gd:seq': true, 'gd:g:pages': true });
  const sq = await R('var s=ed.querySelector(\'[data-fo="gd:seq"]\');[!!s,!!s&&!!s.querySelector(\'[data-fo="gd:g:howto"]\'),!!s&&!!s.querySelector(\'[data-fo="gd:g:pages"]\'),!ed.querySelector(\'[data-fo="gd:app"] [data-fo="gd:g:howto"]\'),[].map.call(ed.querySelectorAll(\'[data-fo="gd:g:pages"] details[data-fo^="gd:i:"]\'),function(d){return d.dataset.fo.slice(5)}).join(","),(ed.querySelector("#gdpgrun")||{}).textContent]');
  ok('Guide › 4 · Guidance › Sequences: How to use the card (moved from App) and First pages, its four pages in order, the run now and its time', sq[0] && sq[1] && sq[2] && sq[3] && sq[4] === 'pg_hello,pg_aud,pg_camera,pg_wall' && /under 30 s/.test(sq[5]), sq);
  await p.locator('[data-gdadd="pages"]').click(); await W(150);
  const tpl = await R('[].map.call(ed.querySelectorAll(".gdtpl [data-gdtpl]"),function(b){return b.textContent}).join("|")');
  await p.locator('[data-gdtpl="pages|page"]').click(); await W(300);
  const np = await R('var g=gdGroupById("pages"),id=g.items[g.items.length-2],it=S.guidance.items[id];[g.items.join(","),it&&it.form,it&&TXT(it.title),it&&TXT(it.says),it&&it.buttons.length,it&&TXT(it.buttons[0].word),TXOWN[it.title],gdPagesPlan().map(function(x){return x.id}).join(","),id]');
  ok('+ Add guidance in First pages offers the Page template only (real clicks); a new page (a title, a line, one button) goes before Straight to the wall, its words its own, in the run',
    tpl === 'Page' && np[1] === 'page' && np[2] === 'A new page' && np[3] === 'A short line under the title.' && np[4] === 1 && np[5] === 'Next' && np[6] === 'gd:i:' + np[8] && /,gdn\d+,pg_wall$/.test(np[0]) && np[7].indexOf(np[8]) > 0, [tpl, np]);
  const nid = np[8];
  const pv = await R(`gdPagesPreview("${nid}");var L=ph.querySelector(".gdpages");[L&&L.dataset.page,(L.querySelector(".gdpg-t")||{}).textContent,L.querySelectorAll(".gdpg-foot button").length]`);
  ok('...the new page on the phone: its title, line, its button and Skip', pv[0] === 'custom' && pv[1] === 'A new page' && pv[2] === 2, pv);
  await R('if(G.gd.pages)gdPagesEnd("test",false)');
  // a drag that would break the order's ends (the new page above Hello), then one within the rules (above the camera)
  await editor('guide', { gt4: true, 'gd:seq': true, 'gd:g:pages': true });
  const drag = async (id, toId, before) => { const h = p.locator(`[data-gddrag="${id}"]`); await h.scrollIntoViewIfNeeded(); const a = await h.boundingBox(), t = await p.locator(`[data-fo="gd:i:${toId}"] > summary`).boundingBox();
    await p.mouse.move(a.x + a.width / 2, a.y + a.height / 2); await p.mouse.down(); await p.mouse.move(a.x + a.width / 2, t.y + (before ? 4 : t.height - 4), { steps: 8 }); await W(80);
    const ln = await R('var l=ed.querySelector(".gddropline");l?[l.classList.contains("no"),l.dataset.why||""]:null'); await p.mouse.up(); await W(250); return ln; };
  const dl1 = await drag(nid, 'pg_hello', true);
  const o1 = await R('[gdGroupById("pages").items.join(","),(ed.querySelector(".gdpgrule")||{}).textContent||""]');
  ok('A drag above Hello (a real drag): the drop line says why while it would break the order; the drop is undone, the field says “Hello stays first”', dl1 && dl1[0] && /Hello stays first/.test(dl1[1]) && o1[0] === np[0] && /Hello stays first.*undone/.test(o1[1]), [dl1, o1]);
  const dl2 = await drag(nid, 'pg_camera', true);
  const o2 = await R('[gdGroupById("pages").items.join(","),!ed.querySelector(".gdpgrule")]');
  ok('A drag within the rules (the new page above the camera, a real drag): the order changes, no warning', dl2 && !dl2[0] && new RegExp('^pg_hello,pg_aud,' + nid + ',pg_camera,pg_wall$').test(o2[0]) && o2[1], [dl2, o2]);
  const dl3 = await drag('pg_wall', 'pg_hello', false);
  ok('Straight to the wall dragged up (a real drag): undone, “Straight to the wall stays last”', dl3 && dl3[0] && /stays last/.test(dl3[1]) && await R('gdGroupById("pages").items[gdGroupById("pages").items.length-1]==="pg_wall"'), dl3);

  lap('before 11');
  // ---------- 11. every new row's demo ----------
  await R('if(G.gd.pages)gdPagesEnd("test",false);S.ui.rowDemo=false;gdChip(null);gdStop("test")');
  const keys = await R(`[gdGroupPath("pages")+"on","guidance.items.pg_hello.on","guidance.items.pg_hello.azu","guidance.items.pg_hello.langChip","guidance.items.pg_aud.on","guidance.items.pg_camera.on","guidance.items.pg_wall.on","guidance.items.${nid}.on","guidance.items.${nid}.azu","guidance.items.${nid}.buttons.0.act","aud.where","ui.gdLaunch"]`);
  const shown = [], miss = [];
  for (const k of keys) {
    await R('document.body.dataset.rowdemo=""'); await R(`showRowDemo("${k}")`);
    let v = ''; const t0 = Date.now(); while (Date.now() - t0 < 3000) { v = await R('document.body.dataset.rowdemo||""'); if (v) break; await W(60); }
    (/\|found\|/.test(v) ? shown : miss).push(k + ' ' + v.split('|').slice(1).join(' '));
  }
  ok('Every new row\'s demo shows its part on the phone (the group, each page with its value, Where to ask, Start the app)', miss.length === 0 && shown.length === keys.length, { miss, shown });
  await R('if(G.gd.pages)gdPagesEnd("test",false);gdStop("test")');
  const real = [];
  await R(base + 'S.ui.rowDemo=true'); await editor('guide', { gt4: true, 'gd:seq': true, 'gd:g:pages': true, 'gd:i:pg_hello': true });
  await p.locator('[data-fo="gd:i:pg_hello"] [data-set="@guidance.items.pg_hello.azu"][data-v="appear"]').click(); real.push(await until('!!ph.querySelector(\'.gdpages .gdpg-art[data-azu="appear"]\')', 3000));
  await p.locator('[data-fo="gd:i:pg_hello"] [data-set="@guidance.items.pg_hello.langChip"][data-v="false"]').click(); real.push(await until('!!ph.querySelector(".gdpages")&&!ph.querySelector(".gdpages .gdpg-lang")', 3000));
  await p.locator('[data-fo="gd:i:pg_hello"] [data-set="@guidance.items.pg_hello.on"][data-v="false"]').click(); real.push(await until('!ph.querySelector(".gdpages:not(.gd-out)")&&/Off: the first run skips Hello/.test((ph.querySelector(".gdchip")||{}).textContent||"")', 3000));
  await p.locator('[data-gdshow="pg_camera"]').click(); real.push(await until('!!ph.querySelector(\'.gdpages[data-page="camera"]\')', 3000));
  ok('...pressed by real clicks: Azu Appears, the chip Hidden, Hello Off, ▸ Show now on the camera: each on the phone at once', real.every(Boolean), real);
  await R('S.ui.rowDemo=false;if(G.gd.pages)gdPagesEnd("test",false);gdChip(null);S.guidance=JSON.parse(window.__g0)');
  ok('The words and their owners: each page\'s own (Languages & Terms), Skip and the progress the group\'s, the bar\'s Start the app', await R('TXOWN.gd_pg_hello==="gd:i:pg_hello"&&TXOWN.gd_pg_cam==="gd:i:pg_camera"&&TXOWN.gd_pg_wall==="gd:i:pg_wall"&&TXOWN.gd_pg_skip==="gd:g:pages"&&TXOWN.pbar_start==="s0bar"&&S.tx.gd_pg_off.pt.length>0&&!!TX_DOM.gd_pg_hello&&guidePath("gd:i:pg_hello").join(">")==="gt4>gd:seq>gd:g:pages>gd:i:pg_hello"'));

  lap('before 12');
  // ---------- 12. the four scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'G.gd.manual=false;G.gd.rate=1;S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^gd8_/.test(x[1])})');
  ok('The console lists the four new scenes', scenes.length === 4 && JSON.stringify(scenes.map(s => s[1])) === JSON.stringify(['gd8_first', 'gd8_skip', 'gd8_camera', 'gd8_later']), scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 30000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`${id === 'gd8_camera' ? '[camera] ' : ''}Scene "${label}": Set up and ask runs it, ✓`, r && r.ok === true, r && [r.note, r.answer]);
    await R('gdStop("test");G.gd.rate=1;if(G.gd.pages)gdPagesEnd("test",false)');
  }
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log((fails ? `${fails} FAILED` : 'ALL PASS') + ` (${Math.round((Date.now() - t00) / 1000)} s)`);
  await b.close();
})();
