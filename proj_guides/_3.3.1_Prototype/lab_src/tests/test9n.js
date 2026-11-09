// test9n (Playwright + the installed Edge): node test9n.js -> PASS/FAIL per check. Guidance round 10: every Editor row shows its own
// demo on the phone (_3.8 section 7, the developer's rule). The self-test's NO DEMO check walks every tab (Detail Card, Guide, Specific
// Marker, Languages & Terms, Global Scene) and finds no row without a demo but its named exemptions (each with its reason); a row's
// demo taken away in the page, or a row added with none, in any tab, is reported NO DEMO. A sample of each kind of row, at least two a
// section, by real clicks in the Editor: a changed value and a press on the row's label (▸) put the phone in that row's view and state
// with its part ringed (a type row: the text it sets; a colour row: the part it colours; a motion row: the motion played once; a
// block's option: that block in view; a page option: the page; a section row: its section; a Card Controls row: its control; a marker
// row: the marker's block; a Guide row: its simulation). The demo starts within one frame of the change and rings after 60 ms.
// MUTATE=1 node test9n.js drops the Specific Marker tab from the NO DEMO check: the checks named [every tab] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const T0 = Date.now();
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { const v = await R(src); if (v) return v; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=true;S.lang="en";S.guideMute=true;S.reduce=false;S.dark=false;S.ui.rowDemo=true;S.ui.level=9;S.op={};G.gd&&(G.gd.manual=true)');
  if (process.env.MUTATE) {
    await R('rowsWithoutDemoAll=function(){var keep=[S.tab,S.ui.level,JSON.stringify(S.op)],out=[];S.ui.level=9;S.op={};ST_TABS.filter(stWalks).filter(function(t){return t!=="marker"}).forEach(function(tab){S.tab=tab;renderEditor();rowsWithoutDemo().forEach(function(r){r.tab=tab;out.push(r)})});S.tab=keep[0];S.ui.level=keep[1];S.op=JSON.parse(keep[2]);return out}');
    console.log('MUTATION: the Specific Marker tab is dropped from the NO DEMO check');
  }

  // ---------- 10.2: NO DEMO walks every tab ----------
  const nd = await R('rowsWithoutDemoAll().map(function(r){return r.tab+" "+r.key})');
  const per = await R('var o={},keep=S.tab;ST_TABS.forEach(function(t){S.tab=t;renderEditor();var k={};ed.querySelectorAll(".row").forEach(function(r){var x=rowKey(r);if(x)k[x]=1});o[t]=Object.keys(k).length});S.tab=keep;renderEditor();o');
  ok(`[every tab] The self-test's NO DEMO check walks every tab and finds no row without a demo (rows with a key: ${JSON.stringify(per)})`, nd.length === 0 && per.card >= 250 && per.guide >= 500 && per.marker >= 10, nd.slice(0, 10));
  const ex = await R('Object.keys(ROW_DEMO_EXEMPT).map(function(k){return[k,ROW_DEMO_EXEMPT[k]]})');
  ok(`The exemptions are named, each with its reason (${ex.length}): rows the Lab's phone cannot show`, ex.length > 0 && ex.length <= 12 && ex.every(e => e[1].length >= 40) && await R('Object.keys(ROW_DEMO_EXEMPT).every(function(k){return!rowDemoOf(k)})'), ex.map(e => e[0]));
  // a row's demo taken away in the page (one per tab), and a row added with none (the Detail Card, the Specific Marker): reported NO DEMO
  await R('window.__drop=["ctl.bar.indicator","ui.gdSim","card.3.tap","blockDefs.Gallery.o.look"];on("rowDemo.spec",function(k){if(window.__drop&&window.__drop.indexOf(k)>=0)return null});' +
    'on("edCard.html",function(h){if(!window.__fake)return h;var d=document.createElement("div");d.innerHTML=h;addRows(d,"s5bar",row("A row with no demo",pseg("ctl.noDemoRow",["a","b"],["A","B"]),""));return d.innerHTML});' +
    'on("edMarker.html",function(h){if(!window.__fake)return h;var d=document.createElement("div");d.innerHTML=h;var f=d.querySelector("[data-fo] > .inner");if(f)f.insertAdjacentHTML("beforeend",row("A marker row with no demo",pseg("m.noDemoRow",["a","b"],["A","B"]),""));return d.innerHTML});' +
    'window.__fake=1;S.ctl.noDemoRow="a";S.m.noDemoRow="a"');
  const st = await R('var j=extPoint("selfTest.res").filter(function(f){return String(f).indexOf("rowsWithoutDemoAll")>=0})[0],r=j([]);showSelfTest(r);[r.map(function(x){return x.tab+" "+x.key}).sort(),JSON.parse(document.getElementById("selftest").dataset.json).bad.length]');
  await R('window.__fake=0;window.__drop=null;delete S.ctl.noDemoRow;delete S.m.noDemoRow;S.tab="card";renderEditor();document.getElementById("selftest").innerHTML=""');
  const want = ['card @blockDefs.Gallery.o.look', 'card @ctl.bar.indicator', 'card @ctl.noDemoRow', 'guide @ui.gdSim', 'marker @card.3.tap', 'marker @m.noDemoRow'];
  ok('[every tab] A row\'s demo taken away (Card Controls, a block\'s option, Guide › Preview & Test, a marker\'s block) or a row added with none (Detail Card, Specific Marker): each reported NO DEMO with its tab, counted as bad', JSON.stringify(st[0]) === JSON.stringify(want) && st[1] === want.length, st);
  ok('...the Global Scene tab has no option rows (its sections are Unity\'s existing ones, shown locked)', per.global === 0, per.global);
  console.log('TIME NO DEMO ' + Math.round((Date.now() - T0) / 1000) + ' s');

  // ---------- 10.1: a sample of each kind of row, by real clicks: a change and a press on the label ----------
  const tabNow = async (tab, extra) => R(`${extra || ''};if(S.tab!=="${tab}"||window.__ext!==${JSON.stringify(extra || '')}){S.tab="${tab}";window.__ext=${JSON.stringify(extra || '')};renderEditor()}`);
  const ctrlSel = (k, scope) => (scope ? scope + ' ' : '') + `[data-set="@${k}"],` + (scope ? scope + ' ' : '') + `[data-sel="@${k}"],` + (scope ? scope + ' ' : '') + `[data-num="@${k}"],` + (scope ? scope + ' ' : '') + `[data-set="${k}"],` + (scope ? scope + ' ' : '') + `[data-sel="${k}"]`;
  const read = async k => { const v = await until(`(document.body.dataset.rowdemo||"").indexOf(${JSON.stringify(k + '|')})===0&&document.body.dataset.rowdemo`, 6000); return [v || '', await R('S.view+"/"+S.stop')]; };
  const label = async (tab, k, scope, extra) => {
    await tabNow(tab, extra); await R('document.body.dataset.rowdemo="";G.demoSeq++;clearTimeout(G.demoT)');
    const at = await R(`var c=ed.querySelector('${ctrlSel(k, scope)}'),l=c&&c.closest(".row").querySelector("label.rd");if(l){l.scrollIntoView({block:"center"});var r=l.getBoundingClientRect();[r.left+12,r.top+r.height/2]}else null`);
    if (!at) return ['(no ▸ on the row)', '']; await p.mouse.click(at[0], at[1]); return read(k);
  };
  const change = async (tab, k, v, scope, extra) => {
    await tabNow(tab, extra); await R('document.body.dataset.rowdemo="";G.demoSeq++;clearTimeout(G.demoT)');
    const sel = (scope ? scope + ' ' : '') + `select[data-sel="@${k}"],` + (scope ? scope + ' ' : '') + `select[data-sel="${k}"]`;
    if (await R(`!!ed.querySelector('${sel}')`)) await p.locator(sel).first().selectOption(v);
    else { const s = (scope ? scope + ' ' : '') + `[data-set="@${k}"][data-v="${v}"],` + (scope ? scope + ' ' : '') + `[data-set="${k}"][data-v="${v}"]`; await p.locator(s).first().scrollIntoViewIfNeeded(); await p.locator(s).first().click(); }
    return read(k);
  };
  const isRing = (r, re) => /\|found\|/.test(r[0]) && re.test(r[0].split('|')[2]);
  const inView = () => R('var e=ph.querySelector(".pfocus");if(!e)false;else{var r=e.getBoundingClientRect(),s=scr.contains(e)?scr.getBoundingClientRect():ph.getBoundingClientRect();r.top>=s.top-4&&r.bottom<=s.bottom+4}');
  const S_ = [];   // [name, ok, info]
  const t = (name, r, viewRe, ringRe) => S_.push([name, isRing(r, ringRe) && viewRe.test(r[1]), r]);
  // Detail Card › 0 · Preview & Test
  t('Preview & Test › Tile on every card ▸: the card, the tile ringed', await label('card', 'test.on'), /^card\/full$/, /testtile/);
  t('Preview & Test › Preview bar Shown = Hidden (change): the card, its top ringed where the bar was', await change('card', 'previewBar', 'false'), /^card\//, /chrome/); await R('S.previewBar=true;renderPhone(true)');
  // 1 · Card
  t('Card › Sheet › Opens at = Peek (change): the card at Peek, the sheet\'s top ringed', await change('card', 'cardCfg.openAt', 'peek'), /^card\/peek$/, /#grab/); await R('S.cardCfg.openAt="half"');
  t('Card › Look › Accent ▸ (a colour row): the Open Row it colours, ringed', await label('card', 'cardCfg.accent'), /^card\/full$/, /b_story"\] \.open/);
  await W(450); const acc = await inView(); S_.push(['...the ringed Open Row is in view on the sheet', acc, acc]);
  t('Card › Sheet › Tap outside closes ▸: a tap on the camera closes the card, the wall ringed', await label('card', 'cardCfg.tapOutside'), /^card\/closed$/, /#wall/);
  // 2 · Sections
  t('Sections › Section start › Style = Wave (change, a select): the Stories section\'s start ringed', await change('card', 'secCfg.start', 'wave'), /^card\/full$/, /sec:stories"\] \.sstart/); await R('S.secCfg.start="tiles"');
  t('Sections › Stories › Shown ▸ (a section row): that section ringed', await label('card', 'sections.1.shown'), /^card\/full$/, /sec:stories/);
  // 3 · Blocks & Components
  t('Blocks › Quick Facts › Look = Chips (change, a block\'s option): that block in view, ringed', await change('card', 'blockDefs.Quick Facts.o.look', 'chips'), /^card\/full$/, /b_facts/);
  await W(450); const qf = await inView(); S_.push(['...the block is in view on the sheet', qf, qf]); await R('S.blockDefs["Quick Facts"].o.look="grid"');
  t('Blocks › Status › Frame ▸ (a block not on this card): the demo card at that block', await label('card', 'blockDefs.Status.frame'), /^democard\/full$/, /b:d\d+/);
  t('Blocks › Story › Page layout = Chapters (change, a page option): its page open, the chapter bar ringed', await change('card', 'blockDefs.Story.o.layout', 'chapters'), /^story\/full$/, /chapbar/); await R('S.blockDefs.Story.o.layout="scroll"');
  t('Blocks › Story › Entrance ▸: the block in view, ringed', await label('card', 'blockDefs.Story.enter'), /^card\/full$/, /b_story/);
  t('Elements › a style\'s Size ▸ (a type row): the demo screen at the text it sets, ringed', await label('card', 'o.size', '[data-fo^="el:"]', 'S.sel.kind="heading";S.sel.style="st5";S.sel.layer="style"'), /^demo\//, /s:st5/);
  // 4 · Motion & Effects
  await R('window.__plays=0;if(!window.__pl){window.__pl=1;scr.addEventListener("click",function(e){if(e.target.closest("[data-play]"))window.__plays++},true)}');
  const mo = await change('card', 'motion.ease', 'spring', '[data-fo="mo:rise"]', 'S.sel.motion="rise";S.op["mo:rise"]=true;S.op["mt:entrance"]=true');
  await W(900); const plays = await R('window.__plays');
  t('Motion › Rise › Easing = Spring (change, a motion row): the demo screen at Rise, ringed', mo, /^demo\//, /m:rise/);
  S_.push(['...the motion plays once (the change and its demo ask one play)', plays === 1, plays]); await R('motionById("rise").ease="out"');
  t('Motion › Rise › Comes in ▸: Rise ringed and played', await label('card', 'motion.from', '[data-fo="mo:rise"]', 'S.sel.motion="rise";S.op["mo:rise"]=true;S.op["mt:entrance"]=true'), /^demo\//, /m:rise/);
  t('Feedback › Haptics ▸: a tap buzzes, the ring where the phone vibrates', await label('card', 'fb.haptics'), /^card\/full$/, /hapring/);
  t('Motion › Motion level = Reduced (change): the section start ringed', await change('card', 'motionLevel', 'reduced'), /^card\/full$/, /sec:stories/); await R('S.motionLevel="full";S.reduce=false');
  // 5 · Card Controls
  t('Card Controls › Navigation Bar › Indicator = Pill (change): the bar ringed', await change('card', 'ctl.bar.indicator', 'pill'), /^card\/full$/, /#navbar/); await R('S.ctl.bar.indicator="raised"');
  t('Card Controls › Contents sheet › Rows ▸: the sheet open, ringed', await label('card', 'ctl.contents.rows'), /^card\/full$/, /csheet/);
  t('Card Controls › Glossary popup › Opens as ▸: the story, a term tapped, its popup ringed', await label('card', 'ctl.term.opens'), /^story\/full$/, /gpop/);
  t('Card Controls › Loading & states › Loader shows after ▸: the loader after that wait, ringed', await label('card', 'states.after'), /^card\/half$/, /stv/);
  // 6 · Audiences (round 9's demos, through the same runner)
  t('Audiences › Preview as ▸: the card, ringed', await label('card', 'vis.aud'), /^card\//, /hero/);
  // Guide › 0 · Preview & Test (rows with no @ path: their first control's key)
  t('Guide › Simulate a visit › The visitor (change): the phone plays its first two minutes ×10', await change('guide', 'ui.gdSim', 'expert'), /^card\/closed$/, /gdchip|gdbub/); await R('S.ui.gdSim="first"');
  t('Guide › The visitor › Screen reader on ▸: a tip announced, not spoken', await label('guide', 'ui.gdSR'), /^card\/closed$/, /gdbub|gdchip/);
  t('Guide › Guide panel › Panel ▸ (round 9\'s row, the same runner)', await label('guide', 'guideCfg.panel'), /^card\/full$/, /gdock|gbar/);
  // Specific Marker
  await R('S.sel.cblock="b_story"');
  t('Specific Marker › The card › Story › Tap = Only the Open Row (change, a marker row): that block\'s Open Row ringed', await change('marker', 'card.3.tap', 'orow'), /^card\/full$/, /b_story"\] \.open/); await R('S.card[3].tap="inherit"');
  t('Specific Marker › Guide script › Solemn point ▸: the tour, Azu\'s face ringed', await label('marker', 'm.solemn'), /^card\/full$/, /mascot/);
  t('Specific Marker › Pages › Page of = Gallery (change): the gallery page, its top bar ringed', await change('marker', 'm.page', 'gallery'), /^gallery\//, /topbar/); await R('S.sel.page="story"');
  S_.forEach(([n, c, i]) => ok(n, c, c ? undefined : i));
  console.log('TIME samples ' + Math.round((Date.now() - T0) / 1000) + ' s');

  // ---------- 10.4: the demo starts within one frame of the change; the ring after 60 ms ----------
  await tabNow('card');
  const fr = await R('var c=ed.querySelector(\'[data-set="@ctl.bar.indicator"][data-v="underline"]\');c.scrollIntoView({block:"center"});G.demoStart=0;G.demoRingAt=0;c.click();window.__t1=performance.now();1');
  const tm = await until('G.demoRingAt&&[Math.round(G.demoStart-window.__t1),Math.round(G.demoRingAt-G.demoStart),G.demoStart>G.demoAsk]', 3000);
  ok('A change\'s demo starts within one frame of the change (as soon as the change\'s own handlers end: under 17 ms after) and rings 60 ms later', fr && tm && tm[2] && tm[0] < 17 && tm[1] >= 55 && tm[1] < 200, tm);
  await R('S.ctl.bar.indicator="raised";renderAll(true)');
  // Show each row's demo = Off: a change leaves the phone as it is; the ▸ still shows it
  await R('S.ui.rowDemo=false;G.demoSeq++;clearTimeout(G.demoT);document.querySelectorAll(".pfocus").forEach(function(x){x.classList.remove("pfocus")});document.body.dataset.rowdemo=""');
  await p.locator('[data-set="@ctl.bar.indicator"][data-v="pill"]').first().click(); await W(500);
  const off = await R('[document.body.dataset.rowdemo||"",!ph.querySelector(".pfocus")]');
  const offLb = await label('card', 'ctl.bar.indicator');
  ok('Show each row\'s demo = Off: a change runs no demo (no ring); a press on the label still shows it', off[0] === '' && off[1] && isRing(offLb, /#navbar/), [off, offLb]);
  await R('S.ui.rowDemo=true;S.ctl.bar.indicator="raised";renderAll(true)');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log('TIME end ' + Math.round((Date.now() - T0) / 1000) + ' s');
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
