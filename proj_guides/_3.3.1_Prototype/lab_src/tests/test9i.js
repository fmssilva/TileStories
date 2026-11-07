// g5 checks (Playwright + the installed Edge): node test9i.js -> PASS/FAIL per check
// Guidance, round 5 (_3.8 section 8): the simulator, the presets, Talkativeness, the Mix table, the audiences' preset, the ×10 demos.
// 1. The Mix along the visit as data: a change moves the score (pure, on gdScore), the kinds' read-backs and the curve follow, the Editor's
// table writes it; 2. Talkativeness: each level's numbers in force (the room, at most a minute and a visit, each kind's cap), on the
// scheduler (pure) and in the rows' words; 3. the presets: each one's values, never the items, the Editor's Preset, Custom and its list,
// the help's tables; the audiences' Guidance preset (Who's exploring and Settings, by real clicks); 4. Simulate a visit: each scripted
// visitor's timeline against its expected counts and order (Instant, in the page, no real-time wait), deterministic, the real gdPick, the
// dots 10 px apart or merged; 5. a dot tapped (by the mouse) shows its item on the phone; a drag scrubs; a skip in the scheduler's words;
// 6. the ×10 demos: the old chip demos with the values in force, a change re-running the simulator and naming what moved; every new row's
// demo; 7. the three scenes in Ask Azu's console.
// MUTATE=1 node test9i.js runs it with the Mix table and Talkativeness ignored (always First visit's mix, always Balanced): the checks
// named [mix] and [talk] turn red.
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
  if (process.env.MUTATE) { await R('gdMix=function(kind){return(GD_PRESETS.first.mix[kind]||[0,0,0]).slice()};gdTalk=function(){return GD_TALK.balanced}'); console.log('MUTATION: the Mix table and Talkativeness ignored (always First visit\'s mix, always Balanced)'); }
  const base = 'S.previewBar=false;S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.lang="en";S.ui.rowDemo=false;S.guideCfg.gbtn="both";S.guideCfg.mascot=true;S.guideCfg.life="lively";S.m.solemn=false;S.hints.on=true;' +
    'S.vis.aud="explorer";S.aud.ask=true;S.aud.seen=true;gdPresetWrite("first");G.gd.manual=true;gdStop("test");gdResetVisitor();G.gd.demo=false;gdChip(null);if(G.on)stopTour();G.cam.open=false;S.settingsOpen=false;G.fab.menu=false;setWall("ribeira");S.view="card";S.stop="closed";renderPhone(false);';
  const edOpen = op => R(`S.tab="guide";S.ui.level=1;S.op=${op};renderEditor()`);

  // ---------- 1. the Mix along the visit: data, read by gdScore ----------
  await R(base);
  const mx = await R('var sc=function(kind,s){var it=Object.assign({},gdItems().filter(function(x){return x.kind===kind})[0],{importance:1,along:"kind"});return Math.round(gdScore(it,gdItems(),{visits:1,visit:{at:0,items:{}},ever:{items:{}},used:{}},{tau:120},0,{s:s}).kind*1000)/1000};' +
    'var a=[sc("app",0),sc("app",0.3),sc("app",1),sc("content",0),sc("content",1)];S.guidance.mix.app=[35,20,10];S.guidance.mix.content=[40,60,80];var b=[sc("app",0),sc("app",0.3),sc("app",1),sc("content",0),sc("content",1)];' +
    'var it=gdItems(),rank=function(s){var V={visits:1,visit:{at:0,items:{}},ever:{items:{}},used:{}};return["askview","tourof"].map(function(id){var x=Object.assign({},it.filter(function(o){return o.id===id})[0],{importance:3,along:"kind"});return gdScore(x,it,V,{tau:120},0,{s:s}).score})};var r0=rank(0);' +
    'gdPresetWrite("first");[a,b,r0[0]>r0[1],typeof GD_MIX,JSON.stringify(S.guidance.mix)]');
  ok('[mix] the Mix is data (S.guidance.mix, no GD_MIX constant): App 70 / 40 / 15 and Content 25 / 50 / 60 by default, and a change to the table moves gdScore (App 35 / 20 / 10, Content 40 / 60 / 80)',
    JSON.stringify(mx[0]) === '[0.7,0.4,0.15,0.25,0.6]' && JSON.stringify(mx[1]) === '[0.35,0.2,0.1,0.4,0.8]' && mx[3] === 'undefined' && mx[4] === '{"app":[70,40,15],"content":[25,50,60],"fun":[5,10,25]}', mx);
  ok('[mix] ...with the changed table Content wins on arrival over App (40 against 35: the order across kinds is the table)', mx[2] === false, mx);
  // the Editor's table: a number typed in a cell writes the Mix; the kind's read-back and the curve follow
  await R(base); await edOpen('{gt4:true,"gd:overall":true,"gd:mix":true,"gd:app":true}'); await W(200);
  const cell = p.locator('[data-num="@guidance.mix.app.0"]'); await cell.scrollIntoViewIfNeeded(); await cell.fill('45'); await cell.dispatchEvent('change'); await W(300);
  const mt = await R('[S.guidance.mix.app[0],(ed.querySelector(".note.gdmix")||{}).textContent,ed.querySelectorAll(\'[data-fo="gd:mix"] input[data-num^="@guidance.mix."]\').length,(ed.querySelector(".gdcurve polyline")||{getAttribute:function(){return""}}).getAttribute("points"),(ed.querySelector(".gdmixt")||{}).textContent,gdPresetNow()]');
  ok('[mix] Overall › Mix along the visit: a 3 × 3 table (App, Content, Fun at s = 0 / 0.3 / 1); typing 45 in App on arrival writes it, App\'s read-back says 45 %, its curve starts at 45 %, the preset turns Custom',
    mt[0] === 45 && /^Its weight along the visit: 45 % on arrival, 40 % a third of the way in, 15 % at the end/.test(mt[1] || '') && mt[2] === 9 && /^46\.0,84\.6 /.test(mt[3]) && mt[5] === 'custom', mt);
  ok('...Fun\'s row is there, at 5 / 10 / 25, and says it has no items yet', /Fun.*no items yet/.test(mt[4] || '') && JSON.stringify(await R('S.guidance.mix.fun')) === '[5,10,25]' && !(await R('S.guidance.kinds.some(function(k){return k.id==="fun"})')), mt[4]);

  // ---------- 2. Talkativeness: one knob over the numbers ----------
  const tk = await R(base + 'var o={};GD_TALK_IDS.forEach(function(id){S.guidance.talk=id;var c=gdContext(),it=gdItems();o[id]=[c.breathing,c.limits.perMinute,c.limits.perVisit,it.filter(function(x){return x.kind==="app"})[0].kindCap,it.filter(function(x){return x.kind==="content"})[0].kindCap,gdForceWords(S.guidance.breathing," s",true),gdForceWords(S.guidance.perMinute,"",false)]});S.guidance.talk="balanced";o');
  ok('[talk] Talkativeness\'s numbers in force: Quiet 30 s, 1 a minute, 12 a visit, App 8, Content 5; Balanced 15 s, 2, 30, App 20, Content 12; Chatty 9 s, 3, 45, App 30, Content 18',
    JSON.stringify(tk.quiet.slice(0, 5)) === '[30,1,12,8,5]' && JSON.stringify(tk.balanced.slice(0, 5)) === '[15,2,30,20,12]' && JSON.stringify(tk.chatty.slice(0, 5)) === '[9,3,45,30,18]', tk);
  ok('[talk] ...each row reads its value in force in words: "In force: 15 s × 2 = 30 s (Quiet)", "In force: 2 × 0.4 = 1 (Quiet)", "In force: 15 s (Balanced)"',
    tk.quiet[5] === 'In force: 15 s × 2 = 30 s (Quiet)' && tk.quiet[6] === 'In force: 2 × 0.4 = 1 (Quiet)' && tk.balanced[5] === 'In force: 15 s (Balanced)' && tk.chatty[5] === 'In force: 15 s × 0.6 = 9 s (Chatty)', tk);
  const tp = await R(base + 'var items=gdItems(),V={visits:1,visit:{n:1,at:0,lastEnd:100000,items:{opening:{shown:1,last:0,end:9000}},starts:[0],away:0,ignoredRun:0,breathMul:1,rest:{},fewer:{},cards:0},ever:{items:{}},used:{}},' +
    'ctx=function(){return Object.assign(gdCtxBase(),{onWall:true,marker:{id:"painting",name:"Ribeira Palace",x:200,y:300,middle:true},gbtn:null,central:null,covers:{},stillSince:-Infinity,returning:false,cues:{}})},w=function(){return gdWhyNot(items,V,ctx(),120000,"tapmarker")};' +
    'S.guidance.talk="balanced";var b=w();S.guidance.talk="quiet";var q=w();S.guidance.talk="balanced";[b,q]');
  ok('[talk] ...on the scheduler (pure): 20 s after a tip, Balanced lets Tap a marker show; Quiet holds it ("breathing room 30 s: 10 s left")', /it shows next/.test(tp[0]) && /breathing room 30 s: 10 s left/.test(tp[1]), tp);
  await R(base); await edOpen('{gt4:true,"gd:overall":true,"gd:content":true}'); await W(150);
  await p.locator('[data-set="@guidance.talk"][data-v="quiet"]').click(); await W(300);
  const tr = await R('[S.guidance.talk,[].map.call(ed.querySelectorAll(\'[data-fo="gd:overall"] .gdforce\'),function(x){return x.textContent}),(ed.querySelector(\'[data-fo="gd:content"] .gdforce\')||{}).textContent,(ed.querySelector(".note.gdtalk")||{}).textContent,gdPresetNow(),(ed.querySelector(".gdpreset")||{}).textContent]');
  ok('[talk] ...pressed in the Editor: Quiet; the rows show it ("In force: 15 s × 2 = 30 s (Quiet)", "30 × 0.4 = 12", Content\'s "12 × 0.4 = 5"); the preset reads Quiet (First visit\'s values with Talkativeness Quiet)',
    tr[0] === 'quiet' && tr[1][0] === 'In force: 15 s × 2 = 30 s (Quiet)' && tr[1][2] === 'In force: 30 × 0.4 = 12 (Quiet)' && tr[2] === 'In force: 12 × 0.4 = 5 (Quiet)' && /^Quiet: the breathing room ×2, the limits ×0\.4/.test(tr[3]) && tr[4] === 'quiet', tr);

  // ---------- 3. the presets ----------
  const ps = await R(base + 'var items0=JSON.stringify(S.guidance.items),o={};GD_PRESET_IDS.forEach(function(x){gdPresetWrite(x[0]);o[x[0]]=[gdPresetNow(),JSON.stringify(gdPresetOf())===JSON.stringify(GD_PRESETS[x[0]]),gdRoomNow(),gdLimitNow(S.guidance.perVisit),S.guidance.kinds[0].cap,S.guidance.kinds[1].gap,S.guidance.mix.app.join("/"),S.guidance.remember]});' +
    'o.items=JSON.stringify(S.guidance.items)===items0;gdPresetWrite("first");o');
  ok('Each preset writes its values (Talkativeness, the room, the limits, the kinds\' caps and gaps, the Mix, Remember): First visit 15 s / 30 / App 20 / Content gap 60 / 70-40-15; Returning 20 s / 24 / 8 / 45 / 35-20-10; Family 12 s / 30 / 20 / 45 / 60-35-15; Expert Quiet 30 s / 10 / 8 / 45 / 40-20-10; Quiet 30 s / 12; Chatty 9 s / 45',
    ['first', 'returning', 'family', 'expert', 'quiet', 'chatty'].every(id => ps[id][0] === id && ps[id][1]) && JSON.stringify(ps.first.slice(2)) === '[15,30,20,60,"70/40/15",true]' && JSON.stringify(ps.returning.slice(2)) === '[20,24,8,45,"35/20/10",true]' &&
    JSON.stringify(ps.family.slice(2)) === '[12,30,20,45,"60/35/15",true]' && JSON.stringify(ps.expert.slice(2)) === '[30,10,8,45,"40/20/10",true]' && ps.quiet[2] === 30 && ps.quiet[3] === 12 && ps.chatty[2] === 9 && ps.chatty[3] === 45, ps);
  ok('...a preset never writes the items', ps.items === true, ps.items);
  await R(base); await edOpen('{gt4:true,"gd:overall":true}'); await W(150);
  await p.locator('[data-sel="@guidance.preset"]').selectOption('family'); await W(300);
  const pe = await R('[gdPresetNow(),S.guidance.breathing,(ed.querySelector(\'[data-sel="@guidance.preset"]\')||{}).value,(ed.querySelector(".gdpreset")||{}).textContent]');
  ok('Overall › Preset, chosen in the Editor (a real select): Family is written and read back', pe[0] === 'family' && pe[1] === 12 && pe[2] === 'family' && /^Family: a group that moves on fast/.test(pe[3]), pe);
  const sl = p.locator('[data-fo="gd:overall"] input[type=number][data-num="@guidance.breathing"]'); await sl.fill('20'); await sl.dispatchEvent('change'); await W(300);
  await R('S.guidance.kinds[1].cap=10;renderEditor()'); await W(200);
  const cu = await R('[gdPresetNow(),(ed.querySelector(\'[data-sel="@guidance.preset"]\')||{}).value,(ed.querySelector(".gdpreset.custom")||{}).textContent,gdPresetDiff()]');
  ok('...a value changed by hand makes it Custom, which lists its changes in words ("Breathing room 20 s, was 12 s · Content: At most a visit 10, was 12")', cu[0] === 'custom' && cu[1] === 'custom' && cu[2] === 'Custom, changed from Family: Breathing room 20 s, was 12 s · Content: At most a visit 10, was 12.' && cu[3].length === 2, cu);
  await R('S.op["gd:help"]=true;renderEditor()'); await W(200);
  const hp = await R('var t=[].map.call(ed.querySelectorAll(\'[data-fo="gd:help"] table\'),function(x){return x.textContent});[t.length,t[0],t[1],ed.querySelector(\'[data-fo="gd:help"]\').textContent]');
  ok('The help reads the tables back in words: each preset\'s values (from GD_PRESETS) and why, Talkativeness\'s factors (×2 / ×0.4, ×0.6 / ×1.5) and what they give with the rows now',
    hp[0] === 2 && /First visit.*Returning.*Family.*Expert.*Quiet.*Chatty/.test(hp[1]) && /Mix: App70 \/ 40 \/ 1535 \/ 20 \/ 10/.test(hp[1]) && /The breathing room×2×1×0\.6/.test(hp[2]) && /×0\.4×1×1\.5/.test(hp[2]) && /Returning: the app is known/.test(hp[3]), hp.slice(0, 3));
  // the audiences' Guidance preset
  await R(base + 'S.tab="card";S.ui.level=1;S.op={s8:true,s8look:true};renderEditor()'); await W(200);
  const ar = await R('[[].map.call(ed.querySelectorAll(\'[data-fo="s8look"] tr.audgd select\'),function(x){return x.dataset.sel+"="+x.value}),(ed.querySelector(\'[data-fo="s8look"] tr.audgd td\')||{}).textContent]');
  ok('Audiences › Looks per audience: a row Guidance preset per audience (Family → Family, Explorer → First visit, Expert → Expert)', JSON.stringify(ar[0]) === JSON.stringify(['@aud.looks.family.gdPreset=family', '@aud.looks.explorer.gdPreset=first', '@aud.looks.expert.gdPreset=expert']) && ar[1] === 'Guidance preset', ar);
  // (the Editor off Looks per audience first: opening that foldout turns the phone to its part, the story page)
  await R('S.tab="guide";S.ui.level=1;S.op={};renderEditor()'); await W(500);
  await R(base + 'S.aud.seen=false;S.view="card";S.stop="half";renderPhone(false)'); await until('!!ph.querySelector(\'.onb [data-onb="family"]\')', 3000);
  await p.locator('.onb [data-onb="family"]').click(); await W(300);
  const af = await R('[aud(),gdPresetNow(),S.guidance.breathing,S.guidance.kinds[1].gap]');
  await R('S.settingsOpen=true;S.stop="half";renderPhone(false)'); await until('!!ph.querySelector(\'[data-vis="aud|expert"]\')', 2000);
  await p.locator('[data-vis="aud|expert"]').first().click(); await W(300);
  const ae = await R('[aud(),gdPresetNow(),gdRoomNow(),S.guidance.talk]');
  await R('S.settingsOpen=false;S.vis.aud="explorer";renderAll(true)'); await W(200);
  const ax = await R('[aud(),gdPresetNow(),S.guidance.breathing]');
  ok('[aud] Who\'s exploring sets the preset: Family tapped on the first card → the Family preset (12 s, Content\'s gap 45 s); Expert in Settings → Expert (Quiet: 30 s in force); Explorer → First visit', JSON.stringify(af) === '["family","family",12,45]' && JSON.stringify(ae) === '["expert","expert",30,"quiet"]' && JSON.stringify(ax) === '["explorer","first",15]', { af, ae, ax });

  // ---------- 4. Simulate a visit: each scripted visitor (Instant, in the page) ----------
  const sim = await R(base + 'var o={};GD_SIM_SCRIPTS.forEach(function(sc){var r=gdSimulate(sc.id),s=gdSimSum(r,15),t=r.dots.filter(function(d){return!d.cue&&d.t<900000});o[sc.id]={s:[s.tips,s.app,s.content,s.cues,s.perMin,s.over,s.first,s.appBefore],order:t.map(function(d){return d.base}),tours:r.quiet.filter(function(q){return q.cls==="tour"}).length,learned:r.learned.map(function(l){return gdClock(l.t)+" "+l.why}),first:r.dots[0]&&r.dots[0].text,same:JSON.stringify(gdSimulate(sc.id))===JSON.stringify(r)}});o');
  const S5 = (id, arr) => JSON.stringify(sim[id].s) === JSON.stringify(arr);
  ok('Simulate a visit · First visit: 12 tips in 15 min, App 5, Content 7 (and 6 card cues), at most 2 a minute, none over a sentence; App first (the opening, Tap a marker, Ask about the view), then Content',
    S5('first', [12, 5, 7, 6, 2, 0, 'app', 3]) && JSON.stringify(sim.first.order.slice(0, 4)) === '["opening","tapmarker","askview","inview"]' && sim.first.tours === 1, sim.first);
  ok('...Fast learner (opens the card at 0:25, hears a tour step): 10 tips, App 4, Content 6 (3 cues); Content from the 3rd tip', S5('fast', [10, 4, 6, 3, 2, 0, 'app', 2]) && sim.fast.order[2] === 'inview' && sim.fast.tours === 1, sim.fast);
  ok('...Idle (ignores every tip): 9 tips, App 7, Content 2, no cue; the breathing room ×2 after three ignored, ×4 after six', S5('idle', [9, 7, 2, 0, 2, 0, 'app', 5]) && /3 ignored in a row: the breathing room ×2/.test(sim.idle.learned[0] || '') && /6 ignored in a row: the breathing room ×4/.test(sim.idle.learned[1] || ''), sim.idle);
  ok('...Tour lover (starts tours): 7 tips, App 5, Content 2 (2 cues); three tours on the Quiet lane', S5('tour', [7, 5, 2, 2, 2, 0, 'app', 2]) && sim.tour.tours === 3, sim.tour);
  ok('...Expert (a returning visitor who has used everything): 7 tips, App 1 (Welcome back), Content 6; no App tip after the opening', S5('expert', [7, 1, 6, 3, 2, 0, 'app', 1]) && /^Welcome back/.test(sim.expert.first || '') && sim.expert.order.slice(1).every(x => x === 'inview'), sim.expert);
  ok('...deterministic: the same script gives the same timeline (each run twice)', Object.keys(sim).every(k => sim[k].same), Object.keys(sim).map(k => k + ' ' + sim[k].same));
  const real = await R(base + 'var n=0,__p=gdPick;gdPick=function(){n++;return __p.apply(null,arguments)};var r;try{r=gdSimulate("first")}finally{gdPick=__p}[n,r.dots.length,String(gdSimRun).indexOf("gdPick(")>0&&!/gd(Gate|Check|All|TourGate)\\(/.test(String(gdSimRun))]');
  ok('...the real scheduler: gdPick is asked at the ticks with nothing showing (thousands over 20 min), no copy of it', real[0] > 3000 && real[1] > 10 && real[2] === true, real);
  const qs = await R(base + 'var b=gdSimSum(gdSimulate("first"),15);gdPresetWrite("quiet");var q=gdSimSum(gdSimulate("first"),15);gdPresetWrite("chatty");var c=gdSimSum(gdSimulate("first"),15);gdPresetWrite("first");[b.tips,q.tips,q.perMin,c.perMin]');
  ok('[talk] Quiet makes the simulated first visit quieter (fewer tips in 15 min, at most 1 a minute); Chatty allows 3 a minute', qs[1] < qs[0] && qs[2] === 1 && qs[3] === 3, qs);

  // ---------- 5. the timeline in the Editor: a dot tapped, a drag, a skip ----------
  await R(base); await edOpen('{gt0:true,"gd:sim":true}'); await R('setOpt("ui.gdSim","first")'); await W(300);
  const tl = await R('var s=ed.querySelector("[data-gdsimsvg]"),lanes=[].map.call(s.querySelectorAll(".gdlab"),function(x){return x.textContent}),g=[].map.call(s.querySelectorAll(".gddot .gdd"),function(c){return[+c.getAttribute("cx"),+c.getAttribute("cy"),+c.getAttribute("r")]}),m=1e9;' +
    'g.forEach(function(a,i){g.forEach(function(b2,j){if(j>i&&a[1]===b2[1])m=Math.min(m,Math.abs(a[0]-b2[0])-a[2]-b2[2])})});[lanes,s.querySelectorAll(".gdq").length,m,s.querySelectorAll(".gddot.many").length,(ed.querySelector("#gdsimsum")||{}).textContent,s.getBoundingClientRect().width<=ed.querySelector(\'[data-fo="gd:sim"] > .inner\').getBoundingClientRect().width]');
  ok('The timeline: lanes App / Content / Fun / Quiet, the quiet spans drawn, the dots apart (never touching: ' + Math.round(tl[2]) + ' px between the closest) or merged into a count; it fits the Editor; the summary under it',
    JSON.stringify(tl[0]) === '["App","Content","Fun","Quiet"]' && tl[1] > 10 && tl[2] >= 3 && tl[3] >= 1 && /^12 tips in 15 min: App 5, Content 7 \(and 6 card cues\); at most 2 a minute; none over a sentence/.test(tl[4]) && tl[5], tl);
  const dot = await R('var res=gdSimNow(),i=res.dots.map(function(d){return d.id}).indexOf("inview:square"),g=ed.querySelector(\'[data-gdsimdot="\'+i+\'"]\');g.scrollIntoView({block:"center"});var r=g.querySelector(".gdd").getBoundingClientRect();[r.left+r.width/2,r.top+r.height/2,i,res.dots[i].text]');
  await p.mouse.click(dot[0], dot[1]); await until('!!(G.gd.cur&&ph.querySelector(".gdbub:not(.gd-out)"))', 2500); await W(400);
  const dt = await R('[G.gd.cur&&G.gd.cur.item.id,G.gd.cur&&G.gd.cur.pull,(ph.querySelector(".gdbub:not(.gd-out)")||{}).textContent,(ph.querySelector(".gdchip")||{}).textContent,(ed.querySelector("#gdsimnow")||{}).textContent,gdOverWords(),(camView().central||{}).id,JSON.stringify(S.gdVisitor.visit.items)]');
  ok('A dot tapped (the mouse on Terreiro do Paço\'s remark at 1:24): the phone turns to that point and shows the remark as it was said, a demo (no memory); the read-out says when, the line and why', dt[0] === 'inview:square' && dt[1] === true && dt[2] === dot[3] && /^Simulated · First visit · 1:24/.test(dt[3]) && /^1:24–1:31 · Content · Terreiro do Paço in the middle/.test(dt[4]) && /why: Terreiro do Paço in the middle, the phone still/.test(dt[4]) && dt[5] === 0 && dt[6] === 'square' && dt[7] === '{}', dt);
  const mg = await R('var g=ed.querySelector("[data-gdsimsvg] .gddot.many");g.scrollIntoView({block:"center"});var r=g.querySelector(".gdd").getBoundingClientRect();[r.left+r.width/2,r.top+r.height/2,g.dataset.gdsimn.split(",").length]');
  await p.mouse.click(mg[0], mg[1]); await W(400);
  const pk = await R('[ed.querySelectorAll("#gdsimpick [data-gdsimpick]").length]'); const cueBtn = await R('var b=[].filter.call(ed.querySelectorAll("#gdsimpick [data-gdsimpick]"),function(x){return/\\(cue\\)/.test(x.textContent)})[0];b?b.dataset.gdsimpick:null');
  if (cueBtn) await p.locator(`#gdsimpick [data-gdsimpick="${cueBtn}"]`).click(); await until('!!(G.gd.cur&&G.gd.cur.cue)', 2500);
  const cq = await R('[G.gd.cur&&G.gd.cur.item.id,G.gd.cur&&G.gd.cur.cue,S.stop,gdSimNow().dots[' + (cueBtn || 0) + '].id]');
  ok('...a merged count tapped lists its items, each a button; a cue among them shows on the card at its element', pk[0] === mg[2] && pk[0] > 1 && cq[1] === true && cq[0] === cq[3] && /half|full/.test(cq[2]), { mg, pk, cq });
  // a drag along the timeline: the playhead to the tour (quiet), the read-out in the scheduler's own words
  const tx = await R('var s=ed.querySelector("[data-gdsimsvg]"),r=s.getBoundingClientRect(),W=+s.getAttribute("width"),res=gdSimNow(),q=res.quiet.filter(function(x){return x.cls==="tour"})[0],mid=(q.from+q.to)/2,x=70+(W-82)*mid/GD_SIM_MS;s.scrollIntoView({block:"center"});r=s.getBoundingClientRect();[r.left+x*r.width/W,r.top+r.height-20,r.left+(x-40)*r.width/W,mid]');
  await p.mouse.move(tx[2], tx[1]); await p.mouse.down(); await p.mouse.move((tx[0] + tx[2]) / 2, tx[1]); await p.mouse.move(tx[0], tx[1]); await p.mouse.up(); await W(200);
  const sk = await R('[G.gd.sim.t,(ed.querySelector("#gdsimnow")||{}).textContent,gdSimAt(gdSimNow(),G.gd.sim.t).skip.why]');
  ok('...a drag along the timeline scrubs it: in the tour the read-out says nothing shows, quiet: a tour, in the scheduler\'s own words (a step being said: never over a sentence)', Math.abs(sk[0] - tx[3]) < 15000 && /nothing shows \(quiet: a tour\): /.test(sk[1]) && /never over a sentence: the guide is speaking|quiet in a tour/.test(sk[2]) && sk[1].indexOf(sk[2].split(' · ')[0]) > 0, sk);
  // Play at ×30: the playhead moves and each dot reached shows on the phone
  await R('setOpt("ui.gdSpeed","30");G.gd.sim.t=0;G.gd.sim.shown=null'); await W(150); await p.locator('[data-gdsimplay]').click();
  const played = await until('G.gd.sim.t>20000&&!!(G.gd.sim.shown&&G.gd.sim.shown.id==="tapmarker")&&/Simulated · First visit · 0:24 · Tap a marker/.test((ph.querySelector(".gdchip")||{}).textContent||"")', 6000);
  await p.locator('[data-gdsimplay]').click(); const pl = await R('[G.gd.sim.t,G.gd.sim.playI,G.gd.sim.upto===G.gd.sim.t,(ph.querySelector(".gdchip")||{}).textContent]'); await R('setOpt("ui.gdSpeed","instant")');
  ok('...Play at ×30: the playhead runs on the visit\'s clock, the dots appear as it passes and each shows on the phone (Tap a marker at 0:24); Stop stops it', played && pl[1] === 0 && pl[2], pl);

  // ---------- 6. the ×10 demos and every new row's demo ----------
  await R(base); await edOpen('{gt0:true,"gd:sim":true,gt4:true,"gd:overall":true,"gd:app":true,"gd:mix":true}'); await W(200);
  const sl2 = p.locator('[data-fo="gd:overall"] input[type=number][data-num="@guidance.breathing"]'); await sl2.fill('25'); await sl2.dispatchEvent('change'); await W(400);
  const mvd = await R('[(ed.querySelector("#gdsimmoved")||{}).textContent,ed.querySelectorAll("[data-gdsimsvg] .gdring2").length,ed.querySelectorAll("[data-gdsimsvg] .gdghost").length]');
  ok('[x10] A change to an Overall row re-runs the simulator and names what moved (Breathing room 25 s, was 15 s), the moved dots ringed and their old places ghosted', /^What moved since the last change \(Breathing room 25 s, was 15 s\): Tap a marker 0:24 → /.test(mvd[0] || '') && mvd[1] > 2 && mvd[2] > 2, mvd);
  await R('S.guidance.talk="quiet";gdPresetWrite("quiet");S.guidance.breathing=15;renderEditor()'); await W(200);
  await R('window.__chips=[];clearInterval(window.__chI);window.__chI=setInterval(function(){var c=ph.querySelector(".gdchip");if(c&&window.__chips.indexOf(c.textContent)<0)window.__chips.push(c.textContent)},50);document.body.dataset.rowdemo=""');
  await R('var lb=ed.querySelector(\'[data-num="@guidance.breathing"]\').closest(".row").querySelector("label.rd");lb.click()');
  await until('window.__chips.some(function(c){return/×10 · Breathing room: 30 s/.test(c)})', 6000);
  const old = await R('window.__chips.slice()');
  ok('[x10] ...round 2\'s ×10 chip demos stay, with the value in force: Breathing room under Quiet counts down 30 s', old.some(c => /×10 · Breathing room: 30 s/.test(c)), old);
  await R('gdPresetWrite("first");renderEditor()'); await W(200);
  const newRows = ['guidance.preset', 'guidance.talk'].concat(['app', 'content', 'fun'].reduce((a, k) => a.concat([0, 1, 2].map(i => 'guidance.mix.' + k + '.' + i)), []));
  const miss = [], chips = {};
  for (const k of newRows) {
    await R('window.__chips=[];document.body.dataset.rowdemo=""');
    const lb = await R(`var c=ed.querySelector('[data-fo="gt4"] .row [data-set="@${k}"],[data-fo="gt4"] .row [data-sel="@${k}"]'),l=c&&c.closest(".row").querySelector("label.rd");if(l){l.scrollIntoView({block:"center"});var r=l.getBoundingClientRect();[r.left+12,r.top+r.height/2]}else null`);
    if (lb) await p.mouse.click(lb[0], lb[1]); else await R(`showRowDemo("${k}")`);
    const f = await until('/\\|found\\|/.test(document.body.dataset.rowdemo||"")&&window.__chips.some(function(c){return/^×10 · /.test(c)})', 5000);
    if (!f) miss.push(k + ' ' + (await R('document.body.dataset.rowdemo||""'))); chips[k] = await R('window.__chips.slice(0,2)');
  }
  ok('[x10] Every new Overall row has its demo: Preset, Talkativeness and the nine cells of the Mix (a press on the label, or the cell\'s change) re-run the simulator and play its two visit-minutes ×10 on the phone', miss.length === 0, { miss, sample: chips['guidance.talk'] });
  // a new row's demo plays the moved items at their moments: Talkativeness Chatty clicked (Show each row's demo On)
  await R('S.ui.rowDemo=true;window.__chips=[]'); await p.locator('[data-set="@guidance.talk"][data-v="chatty"]').click();
  const dm = await until('window.__chips.some(function(c){return/ · was \\d|· new$/.test(c)})', 8000); const dmc = await R('window.__chips.slice()');
  await R('S.ui.rowDemo=false;gdPresetWrite("first");renderEditor()');
  ok('[x10] ...Talkativeness Chatty pressed: the demo starts where the timeline moved and names each item\'s move on the phone ("×10 · 0:18 · Ribeira Palace · was 4:18")', dm && dmc.some(c => /^×10 · Chatty: room 9 s, 3\/min · \d+:\d\d–\d+:\d\d$/.test(c)) && dmc.some(c => /^×10 · \d+:\d\d · .+ · (was \d+:\d\d|new)$/.test(c)), dmc.slice(0, 5));
  await R(base + 'S.tab="card";S.ui.level=1;S.op={s8:true,s8look:true};renderEditor()'); await W(200);
  const am = [];
  for (const a of ['family', 'explorer', 'expert']) {
    await R('window.__chips=[];document.body.dataset.rowdemo=""'); await R(`showRowDemo("aud.looks.${a}.gdPreset")`);
    const f = await until(`/\\|found\\|/.test(document.body.dataset.rowdemo||"")&&window.__chips.some(function(c){return c.indexOf("×10 · "+AUDN["${a}"]+" → ")===0})`, 5000); am.push([a, f, await R(`window.__chips.filter(function(c){return c.indexOf("×10 · "+AUDN["${a}"]+" → ")===0})[0]||""`)]);
  }
  await R('clearInterval(window.__chI)');
  ok('...each audience\'s Guidance preset row has its demo: that preset simulated, two visit-minutes ×10 on the wall ("×10 · Family → Family")', am.every(x => x[1]) && /^×10 · Family → Family · /.test(am[0][2]) && /^×10 · Expert → Expert · /.test(am[2][2]), am);
  const sd = await R('S.tab="guide";S.ui.level=9;S.op={};renderEditor();var r=rowsWithoutDemo(guideSecIds());S.ui.level=1;S.tab="card";renderEditor();r.concat(rowsWithoutDemo(["s8"])).map(function(x){return x.key})');
  ok('...and the self-test\'s rule finds no row of the Guide tab or of 6 · Audiences without a demo', sd.length === 0, sd);

  // ---------- 7. the three scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^gd5_/.test(x[1])})');
  ok('The console lists round 5\'s three guidance scenes (the Quiet preset; the Family audience; a first visit simulated)', JSON.stringify(scenes.map(s => s[1])) === JSON.stringify(['gd5_quiet', 'gd5_family', 'gd5_sim']), scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    const does = await R('ed.querySelector(".agscenes").textContent');
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 30000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`${id === 'gd5_quiet' ? '[talk] ' : ''}Scene "${label}": Set up and ask runs it, ✓`, /Does: nothing:/.test(does) && r && r.ok === true, r && [r.note, r.answer]);
    await R('gdStop("test");G.gd.demo=false;gdChip(null)');
  }
  const back = await R('[gdPresetNow(),aud(),S.aud.seen,S.guidance.breathing]');
  ok('...the scenes give back what they found (First visit, Explorer, the first card answered)', JSON.stringify(back) === '["first","explorer",true,15]', back);
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
