// test9y (Playwright + the installed Edge): node test9y.js -> PASS/FAIL per check. Guidance round 20, the Editor as an accordion
// (editor/accordion.js, editor/guide-tab.js, guide/hero-start.js's glow option, the review's moved rows).
// [collapsed] a fresh page, the Guide tab opened by a real click: its six sections collapsed, one line each (the number and name, a
//   sub-line, a quiet count), all six in the first screen, no foldout open, no chip row; Collapse all and One section open at a time.
// [open] a real click opens one section, the others as they were, its foldouts collapsed; another opens beside it; the open state kept
//   after a re-render and a trip to another tab; One section open at a time On: a third closes the others; Collapse all closes all.
// [links] a link to a deep place (Ask about the view's "Said by the guide: … What's in view", a real click) opens its path and lands it
//   under the pinned headers; Captions' Highlight link (round 19) the same at level 2.
// [sticky] 2 · The tour and its Guide panel open (real clicks), the page scrolled by the wheel: the section's header pinned at the top,
//   the panel's just under it, by the test's own arithmetic (each top = the one above's bottom, no overlap, one line each, the same at
//   every wheel step); a row scrolled to lands under them (scroll-margin); a third level does not pin; a real tap on a pinned header
//   scrolls back to its start and leaves it open.
// [review] the rows the review moved are where the report says (When the visitor scrolls in Focus; Wall (the Lab) in 6 · Preview & Test;
//   What is sent named in Knowledge; Music and Start the guide renamed), each with its ▸ (pressed: its demo on the phone) and its ?.
// [glow] Start the guide glows: Once per visit, On every new card, Off by real clicks in the Editor; each by real taps on the wall's
//   marker, the Torreão's pin and Start: what the visitor sees on the first card, after a tap, on a second card; the row's ▸ plays its demo.
// [card & global] (round 21, 21.9) the Detail Card and Global tabs the same: a fresh open by real clicks, every section collapsed on one
//   line with its sub-line and count, Collapse all and One section; a section opened alone; two headers pinned by the wheel's arithmetic;
//   Languages & Terms' Go to into each tab lands under the pinned header.
// MUTATE=1: the sections open by level as before, the headers not sticky, the moved Lab wall row back in Ask about the view and the glow
//   option ignored (Off glows, a new card never glows again): the checks named [collapsed], [sticky], [review], [glow] and [card & global] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(50); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 700) : '')); };
  const CT = { timeout: 8000 }, NC = e => console.log('NOTE a click could not be made: ' + String(e.message).split(/\r?\n/)[0]);
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  if (process.env.MUTATE) {
    await R('extPoint("isOpen.answer").length=0;document.head.insertAdjacentHTML("beforeend","<style>#ed.acc summary{position:static!important}</style>");' +
      'GUIDE_PLAN.gt0=[{fo:"s7vtest"}];hsGlowOn=function(){return true};["card.opened","point.opened"].forEach(function(n){var l=extPoint(n),i=l.indexOf(hsNewCard);if(i>=0)l.splice(i,1)});""');
    console.log('MUTATION: the sections open by level, the headers not sticky, the Lab wall row back in Ask about the view, the glow option ignored');
  }
  await R('S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.ui.rowDemo=false;G.gd&&(G.gd.manual=true);""');
  const SECS = ['s7mascot', 'gt1', 'gd:g:ontour', 'gt4', 'gt3', 'gt0'];
  const sum = id => `ed.querySelector('[data-fo="${id}"] > summary')`;
  const click = async id => { await p.locator(`[data-fo="${id}"] > summary`).first().click(CT).catch(NC); await W(200); };
  const openState = () => R(`[${SECS.map(id => `!!ed.querySelector('[data-fo="${id}"]').open`).join(',')}]`);

  // ---------- [collapsed] a fresh open ----------
  await p.locator('.utab[data-tab="guide"]').click(CT).catch(NC); await W(400);
  const c1 = await R(`var s=[].map.call(ed.querySelectorAll(":scope > details[data-fo]"),function(d){var m=d.querySelector(":scope > summary"),r=m.getBoundingClientRect(),t=m.querySelector(".acct"),u=m.querySelector(":scope > .sub"),n=m.querySelector(".accn");
      return{id:d.dataset.fo,open:d.open,top:r.top,bottom:r.bottom,h:r.height,t:t&&t.textContent,sub:!!u&&u.textContent.length>10&&getComputedStyle(u).whiteSpace==="nowrap",n:n&&n.textContent}});
    [s,ed.querySelectorAll("details[open]").length,!!ed.querySelector(".gtnav,[data-gtgo]"),!!ed.querySelector('.accbar [data-acc="collapse"]'),!!ed.querySelector('.accbar [data-set="ui.accOne"][data-v="false"][aria-pressed="true"]'),innerHeight]`);
  const secs = c1[0];
  ok('[collapsed] A fresh open (a real click on the Guide tab): the six sections in order, every one collapsed, no foldout open anywhere in the tab', JSON.stringify(secs.map(s => s.id)) === JSON.stringify(SECS) && secs.every(s => !s.open) && c1[1] === 0, secs.map(s => s.id + '|' + s.open));
  ok('[collapsed] ...each on one line (34 px): its number and name, a short sub-line on one line, a quiet count of its parts', secs.every(s => Math.abs(s.h - 34) < 0.6 && /^\d · /.test(s.t || '') && s.sub && /^\d+ parts?$/.test(s.n || '')),
    secs.map(s => [s.t, Math.round(s.h), s.n]));
  ok(`[collapsed] ...all six in the first screen (the sixth's bottom at ${Math.round(secs[5].bottom)} px of ${c1[5]})`, secs[5].bottom <= c1[5] && secs[0].top >= 0, secs.map(s => Math.round(s.top)));
  ok('[collapsed] ...the chip row "The guide: 1 · …" is gone; at the top: Collapse all and One section open at a time (Off)', !c1[2] && c1[3] && c1[4], c1.slice(2, 5));
  tsec('collapsed');

  // ---------- [open] ----------
  await click('gt1');
  const o1 = await openState(), in1 = await R(`[].filter.call(ed.querySelectorAll('[data-fo="gt1"] details[data-fo]'),function(d){return d.open}).length`);
  ok('[open] A real click on 2 · The tour opens it alone; its foldouts start collapsed', JSON.stringify(o1) === JSON.stringify([false, true, false, false, false, false]) && in1 === 0, [o1, in1]);
  await click('gt4');
  const o2 = await openState();
  ok('[open] ...4 · The mascot speaks up opens beside it (One section open at a time is Off)', JSON.stringify(o2) === JSON.stringify([false, true, false, true, false, false]), o2);
  await R('renderEditor();""'); const o3 = await openState();
  await p.locator('.utab[data-tab="card"]').click(CT).catch(NC); await W(300); await p.locator('.utab[data-tab="guide"]').click(CT).catch(NC); await W(400); const o4 = await openState();
  ok('[open] ...remembered after a re-render and after a trip to Detail Card and back (S.op)', JSON.stringify(o3) === JSON.stringify(o2) && JSON.stringify(o4) === JSON.stringify(o2), [o3, o4]);
  await p.locator('.accbar [data-set="ui.accOne"][data-v="true"]').click(CT).catch(NC); await W(300);
  await click('gt3'); const o5 = await openState();
  ok('[open] One section open at a time On (a real click): opening 5 · Ask the mascot closes the two that were open', JSON.stringify(o5) === JSON.stringify([false, false, false, false, true, false]) && await R('S.op.gt1===false&&S.op.gt4===false&&S.op.gt3===true'), o5);
  await p.locator('.accbar [data-set="ui.accOne"][data-v="false"]').click(CT).catch(NC); await W(300);
  await click('s7mascot'); await click('gt:voice'); const o6 = await openState();
  ok('[open] ...Off again: 1 · The mascot opens beside it', JSON.stringify(o6) === JSON.stringify([true, false, false, false, true, false]), o6);
  await p.evaluate(() => window.scrollTo(0, 600)); await W(100);
  await p.locator('.accbar [data-acc="collapse"]').click(CT).catch(NC); await W(400);
  const o7 = await R('[ed.querySelectorAll("details[open]").length,[].filter.call(ed.querySelectorAll("details[data-fo]"),function(d){return S.op[d.dataset.fo]!==false}).length,document.querySelector(".ed").getBoundingClientRect().top>=-1]');
  ok('[open] Collapse all (a real click, scrolled down): every section and foldout of the tab closed, each kept closed in S.op, the Editor\'s top in view', o7[0] === 0 && o7[1] === 0 && o7[2], o7);
  tsec('open');

  // ---------- [links] a link to a deep place lands under the pinned headers ----------
  await click('gt3'); await click('s7ask'); await click('s7cam');
  await p.locator('[data-fo="s7cam"] [data-goto="gd:g:inview"]').click(CT).catch(NC); await W(400);
  const l1 = await R(`var t=ed.querySelector('[data-fo="gd:g:inview"]'),a=${sum('gt4')}.getBoundingClientRect(),c=${sum('gd:content')}.getBoundingClientRect(),r=t.getBoundingClientRect();
    [["gt4","gd:content","gd:g:inview"].every(function(id){return ed.querySelector('[data-fo="'+id+'"]').open}),a.top,a.bottom,c.top,c.bottom,r.top,innerHeight]`);
  ok(`[links] A real click on Ask about the view's link "Said by the guide: … What's in view" opens its path (4 · The mascot speaks up › Content › What's in view) and lands it under the two pinned headers (theirs at ${Math.round(l1[1])}-${Math.round(l1[2])} and ${Math.round(l1[3])}-${Math.round(l1[4])} px, it at ${Math.round(l1[5])} px)`,
    l1[0] && Math.abs(l1[1]) < 0.6 && Math.abs(l1[3] - l1[2]) < 0.6 && l1[5] >= l1[4] - 0.5 && l1[5] < l1[6] / 2, l1);
  await R('S.op={};S.op.gt1=true;S.op.s7cap=true;renderEditor();""'); await W(100);
  await p.locator('[data-fo="s7cap"] [data-goto="gt:speech"]').click(CT).catch(NC); await W(400);
  const l2 = await R(`var a=${sum('s7mascot')}.getBoundingClientRect(),c=${sum('gt:speech')}.getBoundingClientRect(),d=ed.querySelector('[data-fo="gt:speech"]');[d.open,a.top,a.bottom,c.top,d.getBoundingClientRect().top]`);
  ok('[links] ...Captions\' Highlight link (round 19, a real click): 1 · The mascot › Speech on screen open, its header just under its section\'s, its start not under either', l2[0] && Math.abs(l2[1]) < 0.6 && l2[3] >= l2[2] - 0.5 && l2[3] - l2[2] < 2 && l2[4] >= l2[2] - 0.5, l2);
  tsec('links');

  // ---------- [sticky] two pinned headers, stacked exactly ----------
  await p.locator('.accbar [data-acc="collapse"]').click(CT).catch(NC); await W(300);
  await click('gt1'); await click('s7dock');
  const pin = () => R(`var a=${sum('gt1')}.getBoundingClientRect(),c=${sum('s7dock')}.getBoundingClientRect(),d=ed.querySelector('[data-fo="s7dock"]').getBoundingClientRect(),u=${sum('s7dock')}.querySelector(".sub");
    [a.top,a.bottom,c.top,c.bottom,d.top,a.height,c.height,!!u&&getComputedStyle(u).textOverflow==="ellipsis"&&getComputedStyle(${sum('gt1')}).whiteSpace==="nowrap",getComputedStyle(${sum('gt1')}).backgroundColor,getComputedStyle(${sum('s7dock')}).backgroundColor,getComputedStyle(${sum('s7dock')}).boxShadow]`);
  const d0 = await R(`ed.querySelector('[data-fo="s7dock"]').getBoundingClientRect().top+scrollY`);
  await p.evaluate(y => window.scrollTo(0, y - 60), d0); await W(150);
  const box = await p.locator('#ed').boundingBox(); await p.mouse.move(box.x + box.width / 2, 450);
  const steps = [];
  for (let i = 0; i < 5; i++) { await p.mouse.wheel(0, 90); await W(140); steps.push(await pin()); }
  const s3 = steps[steps.length - 1];
  ok('[sticky] Scrolled by the wheel inside 2 · The tour › Guide panel: the section\'s header pinned at the top (0 px) and the panel\'s just under it, its top = the section header\'s bottom, at every one of 5 wheel steps',
    steps.slice(1).every(s => Math.abs(s[0]) < 0.6 && Math.abs(s[2] - s[1]) < 0.6 && s[4] < s[2] - 1), steps.map(s => s.slice(0, 5).map(x => Math.round(x * 10) / 10)));
  ok('[sticky] ...nothing overlaps (the panel\'s header starts where the section\'s ends), each one line (34 and 32 px, the sub-line cut with an ellipsis), on the foldouts\' own opaque colours with a hairline under',
    s3[2] >= s3[1] - 0.5 && Math.abs(s3[5] - 34) < 0.6 && Math.abs(s3[6] - 32) < 0.6 && s3[7] && /rgb\(60, 60, 60\)/.test(s3[8]) && /rgb\(65, 65, 65\)/.test(s3[9]) && /1px/.test(s3[10]), s3.slice(5));
  const rw = await R(`var r=rowEl(ed,"s7dock","Talk button");r.scrollIntoView({block:"start"});var c=${sum('s7dock')}.getBoundingClientRect(),x=r.getBoundingClientRect();[x.top,c.bottom,document.elementFromPoint(x.left+40,x.top+x.height/2)===r||r.contains(document.elementFromPoint(x.left+40,x.top+x.height/2))]`);
  ok(`[sticky] ...a row scrolled to (Talk button) lands under the pinned headers, not under them (its top ${Math.round(rw[0])} px, the headers' bottom ${Math.round(rw[1])} px), and is the one under a finger there`, rw[0] >= rw[1] - 0.5 && rw[2], rw);
  await p.locator(`[data-fo="s7dock"] > summary`).click(CT).catch(NC); await until(`Math.abs(ed.querySelector('[data-fo="s7dock"]').getBoundingClientRect().top-34)<1.5`, 2500);
  const bk = await R(`[ed.querySelector('[data-fo="s7dock"]').open,ed.querySelector('[data-fo="s7dock"]').getBoundingClientRect().top]`);
  ok('[sticky] A real tap on the pinned Guide panel header scrolls back to its start (its top just under the section\'s header, 34 px) and leaves it open', bk[0] && Math.abs(bk[1] - 34) < 1.5, bk);
  await p.mouse.wheel(0, 300); await W(200);
  await p.locator(`[data-fo="gt1"] > summary`).click(CT).catch(NC); await until(`Math.abs(ed.querySelector('[data-fo="gt1"]').getBoundingClientRect().top)<1.5`, 2500);
  const bk2 = await R(`[ed.querySelector('[data-fo="gt1"]').open,ed.querySelector('[data-fo="gt1"]').getBoundingClientRect().top]`);
  ok('[sticky] ...and on the pinned 2 · The tour: back to its start (0 px), still open', bk2[0] && Math.abs(bk2[1]) < 1.5, bk2);
  await click('gt:ways'); await click('s7gbtn');
  await R(`var d=ed.querySelector('[data-fo="s7gbtn"]');scrollBy(0,d.getBoundingClientRect().top-66+40);""`); await W(150);
  const l3 = await R(`var d=ed.querySelector('[data-fo="s7gbtn"]'),s=d.querySelector(":scope > summary");[getComputedStyle(s).position,s.getBoundingClientRect().top-d.getBoundingClientRect().top<1.5&&s.getBoundingClientRect().top<66,${sum('gt:ways')}.getBoundingClientRect().top]`);
  ok('[sticky] ...a third level (Ways in › Guide button) does not pin: it scrolls away under Ways in, which is pinned at 34 px', l3[0] === 'static' && l3[1] && Math.abs(l3[2] - 34) < 0.6, l3);
  tsec('sticky');

  // ---------- [review] the moved and renamed rows, each with its ▸ and ? ----------
  await R('S.ui.level=9;S.op={};renderEditor();""');
  const rv = await R(`var lab=function(r){return r?r.querySelector("label").textContent:null},has=function(r){return!!r&&!!r.querySelector(".help")&&(r.querySelector("label").classList.contains("rd")||!!r.querySelector("[data-agctx]"))},
      w=rowEl(ed,"gt0","Wall (the Lab)"),h=w&&w.previousElementSibling;
    [!!rowEl(ed,"s7focus","When the visitor scrolls"),!rowEl(ed,"s7resume","When the visitor scrolls"),!!w,!rowEl(ed,"s7cam","Wall (the Lab)"),!!h&&/The Lab's wall/.test(h.textContent),!!rowEl(ed,"gt:know","What is sent"),!rowEl(ed,"gt:know",""),
     ed.querySelector('[data-fo="s7voice"] > summary').textContent.indexOf("Music ·")===0,ed.querySelector('[data-fo="s7hero"] > summary').textContent.indexOf("Start the guide ·")===0,!!rowEl(ed,"s7hero","Start the guide glows"),
     [rowEl(ed,"s7focus","When the visitor scrolls"),w,rowEl(ed,"gt:know","What is sent"),rowEl(ed,"s7hero","Start the guide glows")].map(has)]`);
  ok('[review] When the visitor scrolls is in 2 · The tour › Focus (no longer in Continue the tour); Wall (the Lab) in 6 · Preview & Test under "The Lab\'s wall" (no longer in Ask about the view)', rv[0] && rv[1] && rv[2] && rv[3] && rv[4], rv.slice(0, 5));
  ok('[review] ...Knowledge\'s unnamed row is What is sent; Narration is Music, Hero button is Start the guide, with Start the guide glows', rv[5] && rv[6] && rv[7] && rv[8] && rv[9], rv.slice(5, 10));
  ok('[review] ...each with its ? help and its ▸ (a pressable label; What is sent: its own Show button)', rv[10].every(Boolean), rv[10]);
  const demo = async (fo, label, key) => { if (!await R(`S.ui.level=9;S.op={};renderEditor();var r=rowEl(ed,"${fo}","${label}");if(r)r.scrollIntoView({block:"center"});document.body.dataset.rowdemo="";!!r`)) return false; await W(100);
    await p.locator(`[data-fo="${fo}"] > .inner > .row`).filter({ has: p.locator(`label:text-is("${label}")`) }).locator('label').click(CT).catch(NC);
    return until(`(document.body.dataset.rowdemo||"").indexOf("${key}|found")===0`, 4000); };
  const dm = [await demo('s7focus', 'When the visitor scrolls', 'guideCfg.onScroll'), await demo('gt0', 'Wall (the Lab)', 'camCfg.wall')];
  ok('[review] ...a real press on each moved row\'s ▸ shows its demo on the phone, the part found and ringed (When the visitor scrolls, Wall (the Lab))', dm.every(Boolean), dm);
  await R('if(G.on)stopTour();S.camCfg.wall="ribeira";""');
  tsec('review');

  // ---------- [glow] the three choices, by real taps ----------
  const glowOn = () => R('!!ph.querySelector(".hero .gstart.hsglow")');
  const wallTap = async id => { await R('if(G.on)stopTour();S.view="card";S.stop="closed";G.cam.open=false;renderPhone(false);""'); await W(300);
    /* the Ribeira Palace by its marker; the Torreão (no marker on this wall) by its pin in Ask about the view */
    if (id === 'torreao') { await R('G.cam.open=true;camLayer();""'); await p.locator('#ph [data-cpin="torreao"]').first().click(CT).catch(NC); }
    else await p.locator(`#ph [data-marker="${id}"]`).first().click(CT).catch(NC);
    await W(200); };
  const res = {};
  for (const [v, lab] of [['visit', 'Once per visit'], ['card', 'On every new card'], ['off', 'Off']]) {
    await R('S.tab="guide";S.ui.level=1;S.op={gt1:true,"gt:ways":true,s7hero:true};renderEditor();rowEl(ed,"s7hero","Start the guide glows").scrollIntoView({block:"center"});""'); await W(150);
    await p.locator(`[data-fo="s7hero"] [data-set="@guideCfg.heroGlow"][data-v="${v}"]`).click(CT).catch(NC); await W(200);
    await R('if(G.on)stopTour();clearTimeout(G.hs.t);G.hs.glow=false;G.hs.tapped=false;S.reduce=false;setWall("ribeira");pointOpen("painting");""');
    const r = { set: await R('S.guideCfg.heroGlow') };
    await wallTap('painting');
    r.first = await until('!!ph.querySelector(".hero .gstart.hsglow")', 2600);
    await p.locator('#ph .hero .gstart').click(CT).catch(NC); await W(250);
    r.afterTap = await glowOn(); r.tour = await R('!!G.on');
    await wallTap('torreao'); await W(1800);
    r.second = await glowOn(); r.secondPoint = await R('S.m.poi');
    await wallTap('painting'); await W(1800); r.third = await glowOn();
    res[v] = r;
  }
  ok('[glow] Once per visit (the default): the first card glows once settled; a real tap on Start takes it away (the tour starts); a second card (the Torreão, a real tap on its pin in Ask about the view) and the first again do not glow',
    res.visit.set === 'visit' && res.visit.first && !res.visit.afterTap && res.visit.tour && res.visit.secondPoint === 'torreao' && !res.visit.second && !res.visit.third, res.visit);
  ok('[glow] On every new card: the first card glows, a tap takes it away, the Torreão\'s card glows again, and the Ribeira Palace\'s too when opened again', res.card.first && !res.card.afterTap && res.card.second && res.card.third, res.card);
  ok('[glow] Off: no card glows, ever (2.6 s and 1.8 s after each opens)', !res.off.first && !res.off.second && !res.off.third, res.off);
  const dmo = {};
  for (const v of ['card', 'visit']) {
    await R(`if(G.on)stopTour();S.guideCfg.heroGlow="${v}";S.tab="guide";S.ui.level=1;S.op={gt1:true,"gt:ways":true,s7hero:true};renderEditor();rowEl(ed,"s7hero","Start the guide glows").scrollIntoView({block:"center"});document.body.dataset.rowdemo="";document.body.dataset.hsdemo="";""`); await W(150);
    await p.locator('[data-fo="s7hero"] .row label:text-is("Start the guide glows")').click(CT).catch(NC);
    const g1 = await until('!!ph.querySelector(".hero .gstart.hsglow")&&S.m.poi==="painting"', 2400), rg = await until('(document.body.dataset.rowdemo||"").indexOf("guideCfg.heroGlow|found")===0', 2400);
    const tp = await until('document.body.dataset.hsdemo==="tapped"&&!ph.querySelector(".hero .gstart.hsglow")', 3500);
    const sc = await until('document.body.dataset.hsdemo==="second"&&S.m.poi==="torreao"', 3000); await W(1500);
    dmo[v] = { glow: g1, ring: rg, tapped: tp, second: sc, glowAgain: await glowOn(), tour: await R('!!G.on') };
  }
  ok('[glow] Its ▸ (a real press) plays its demo: the Ribeira Palace\'s card glowing and ringed, a tap that takes the glow away (no tour), then the Torreão\'s card: glowing again On every new card, not Once per visit',
    dmo.card.glow && dmo.card.ring && dmo.card.tapped && dmo.card.second && dmo.card.glowAgain && !dmo.card.tour && dmo.visit.glow && dmo.visit.tapped && dmo.visit.second && !dmo.visit.glowAgain, dmo);
  await R('S.guideCfg.heroGlow="visit";if(G.on)stopTour();""');
  tsec('glow');

  // ---------- [card & global] round 21 (21.9, the developer): the Detail Card and Global tabs are accordions too ----------
  const tabSecs = () => R(`[].map.call(ed.querySelectorAll(":scope > details[data-fo]"),function(d){var m=d.querySelector(":scope > summary"),r=m.getBoundingClientRect(),t=m.querySelector(".acct"),u=m.querySelector(":scope > .sub"),n=m.querySelector(".accn"),
      body=!!d.querySelector(":scope > .inner > details[data-fo]")||!!d.querySelector(":scope > .inner > .row");return{id:d.dataset.fo,open:d.open,h:r.height,t:t&&t.textContent,sub:!!u&&u.textContent.length>10&&getComputedStyle(u).whiteSpace==="nowrap",n:n&&n.textContent,body:body}})`);
  const ct = {};
  for (const tab of ['card', 'global']) {
    await R('S.op={};S.ui.level=1;S.tab="guide";renderEditor();scrollTo(0,0);""');
    await p.locator(`.utab[data-tab="${tab}"]`).click(CT).catch(NC); await W(400);
    const ss = await tabSecs(), bar = await R('[!!ed.querySelector(\':scope > .accbar [data-acc="collapse"]\'),!!ed.querySelector(\':scope > .accbar [data-set="ui.accOne"]\'),ed.querySelectorAll("details[open]").length,ed.classList.contains("acc")]');
    ct[tab] = { ss, bar };
    ok(`[card & global] ${tab === 'card' ? 'Detail Card' : 'Global'} (a real click on its tab, fresh): ${ss.length} sections, every one collapsed on one line (34 px) with its name, a plain sub-line and (when it holds something) a quiet count; no foldout open; Collapse all and One section open at a time at the top`,
      ss.length >= (tab === 'card' ? 7 : 12) && ss.every(s => !s.open && Math.abs(s.h - 34) < 0.6 && s.t && s.sub && (s.body ? /^\d+ (parts?|rows?)$/.test(s.n || '') : true)) && bar[0] && bar[1] && bar[2] === 0 && bar[3],
      { secs: ss.map(s => [s.id, s.open, Math.round(s.h), s.t, s.n]), bar });
  }
  /* Detail Card: a real click opens 5 · Card Controls alone, its foldouts collapsed; its Top bar opened; the wheel: two headers pinned */
  await R('S.op={};S.ui.level=1;renderEditor();scrollTo(0,0);""'); await p.locator('.utab[data-tab="card"]').click(CT).catch(NC); await W(300);
  await click('s5'); const co = await R('[[].filter.call(ed.querySelectorAll(":scope > details[data-fo]"),function(d){return d.open}).map(function(d){return d.dataset.fo}),[].filter.call(ed.querySelectorAll(\'[data-fo="s5"] details[data-fo]\'),function(d){return d.open}).length]');
  ok('[card & global] Detail Card: a real click opens 5 · Card Controls alone, its foldouts collapsed', JSON.stringify(co[0]) === '["s5"]' && co[1] === 0, co);
  /* the Navigation Bar's foldout (about 540 px open: a header stays pinned only while its foldout runs on under it) */
  await click('s5bar');
  const cpin = () => R(`var a=${sum('s5')}.getBoundingClientRect(),c=${sum('s5bar')}.getBoundingClientRect(),d=ed.querySelector('[data-fo="s5bar"]').getBoundingClientRect();[a.top,a.bottom,c.top,c.bottom,d.top,d.height]`);
  const c0 = await R(`ed.querySelector('[data-fo="s5bar"]').getBoundingClientRect().top+scrollY`);
  await p.evaluate(y => window.scrollTo(0, y - 60), c0); await W(150);
  const eb = await p.locator('#ed').boundingBox(); await p.mouse.move(eb.x + eb.width / 2, 450);
  const cst = []; for (let i = 0; i < 4; i++) { await p.mouse.wheel(0, 70); await W(140); cst.push(await cpin()); }
  ok('[card & global] ...its Navigation Bar opened too (a real click), scrolled by the wheel: 5 · Card Controls pinned at the top (0 px) and the Navigation Bar\'s header just under it (its top = the section header\'s bottom), at every wheel step',
    cst.slice(1).every(s => Math.abs(s[0]) < 0.6 && Math.abs(s[2] - s[1]) < 0.6 && s[4] < s[2] - 1), cst.map(s => s.map(x => Math.round(x * 10) / 10)));
  /* the links into both tabs: Languages & Terms' Go to (real clicks) opens the path and lands under the pinned headers */
  const land = async (owner, sec) => { await R('S.op={};S.ui.level=1;S.tab="words";renderEditor();scrollTo(0,0);""'); await W(200);
    await p.locator(`#ed [data-goloc="${owner}"]`).first().click(CT).catch(NC); await W(500);
    return R(`var t=ed.querySelector('[data-fo="${owner}"]'),a=${sum(sec)}.getBoundingClientRect(),r=t?t.getBoundingClientRect():{top:-1};[S.tab,!!t&&t.open&&ed.querySelector('[data-fo="${sec}"]').open,a.top,a.bottom,r.top,innerHeight]`); };
  const lc = await land('s5top', 's5'), lg = await land('g:10s', 'g:10');
  ok(`[card & global] Languages & Terms' Go to (real clicks) opens the path in Detail Card (5 · Card Controls › Top bar) and in Global (Select, Filter & Search › its search field), each landing under the pinned header (at ${Math.round(lc[4])} and ${Math.round(lg[4])} px, the headers' bottoms ${Math.round(lc[3])} and ${Math.round(lg[3])} px), in the upper half of the screen`,
    lc[0] === 'card' && lc[1] && lc[4] >= lc[3] - 0.5 && lc[4] < lc[5] / 2 && lg[0] === 'global' && lg[1] && lg[4] >= lg[3] - 0.5 && lg[4] < lg[5] / 2, { card: lc, global: lg });
  await R('S.op={};S.ui.level=1;S.tab="guide";renderEditor();scrollTo(0,0);""');
  tsec('card & global');

  ok('no page errors', errs.length === 0, errs.slice(0, 5));
  console.log('fails', fails);
  await b.close();
})();
