// test9r (Playwright + the installed Edge): node test9r.js -> PASS/FAIL per check. Guidance round 14, the Guide tab's organization (the
// developer's tab, 2026-11-11): the six sections in order with their names; each moved foldout under its new parent (by id); no foldout
// without a row or a word to edit, no section with a single row; every row key of round 13's tab still there (tests/guide_rows_r13.json, but
// the one the developer took out: Water break after) and every foldout id (but the two taken apart: the old Mascot section gt2 and Tour
// controls s7tour, its words now in Guide panel); the levels (1: the six sections, 2: their foldouts, an item's groups never by level); an
// item opened shows its group headers only, each with its changed count (one field changed by a real click: 1; Reset: 0; the default
// clicked: 0); Says as one list (a real edit of the PT text is Languages & Terms' word, and its Go to opens the item's Says; + Add a
// variant; Remove with Undo); a point's own field: its link (a real click) opens Specific Marker › Guide script at that field; an old open
// state (gd:fun > gd:g:ontour; gt2) opens the new place; the tab's names at the top (a real click); a foldout a module adds later lands at
// the end of 2 · The tour with a console warning; the Detail Card has no guide foldout; NO DEMO for none; the Guide tab's draw time, now and
// round 13's (measured on master's build the same way: tests/guide_rows_r13.json's drawMs), not slower.
// MUTATE=1 node test9r.js leaves Ask about the view out of its new place (GUIDE_PLAN): the checks named [moved] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
const R13 = JSON.parse(fs.readFileSync(path.join(__dirname, 'guide_rows_r13.json'), 'utf8'));
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1600, height: 1000 } });
  const errs = [], warns = []; p.on('pageerror', e => errs.push(e.message)); p.on('console', m => { if (m.type() === 'warning' && /^Guide tab/.test(m.text())) warns.push(m.text()) });
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(50); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  if (process.env.MUTATE) { await R('GUIDE_PLAN.gt3[0].parts=GUIDE_PLAN.gt3[0].parts.filter(function(x){return x.fo!=="s7cam"});GUIDE_PATHS=null'); console.log('MUTATION: Ask about the view left out of its new place'); }
  await R('S.ui.rowDemo=false;S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.view="card";S.stop="closed";renderPhone(false)');
  const guide = (level, op) => R(`S.tab="guide";S.ui.level=${level};S.op=${JSON.stringify(op || {})};renderEditor()`);

  // ---------- 1. the six sections, their names, the moved foldouts' parents ----------
  await guide(9);
  const secs = await R('[].map.call(ed.querySelectorAll(":scope > details[data-fo]"),function(d){return d.dataset.fo+"|"+d.querySelector(":scope > summary").textContent.split(" · ").slice(0,2).join(" · ")})');
  ok('The six sections in order: 1 · Azu, 2 · The tour, 3 · Games on the tour, 4 · Azu speaks up, 5 · Ask Azu, 6 · Preview & Test (last: round 14b) (Mascot and Fun › On the tour keep their ids as sections)',
    JSON.stringify(secs) === JSON.stringify(['s7mascot|1 · The mascot', 'gt1|2 · The tour', 'gd:g:ontour|3 · Games on the tour', 'gt4|4 · The mascot speaks up', 'gt3|5 · Ask the mascot', 'gt0|6 · Preview & Test']), secs);
  const PARENT = { s7vtest: 'gt0', 'gd:sim': 'gt0', 'gd:visitor': 'gt0', 'gt:tools': 'gt:look', s7voice: 'gt1', s7cap: 'gt1', s7focus: 'gt1', s7coach: 'gt1', s7dock: 'gt1', 'gt:ways': 'gt1',
    s7gbtn: 'gt:ways', s7hero: 'gt:ways', s7block: 'gt:ways', s7resume: 'gt:ways', 'gd:pair:quizpromise': 'gd:g:ontour', 'gd:i:quizpromise': 'gd:pair:quizpromise', 'gd:i:quizfinale': 'gd:pair:quizpromise',
    'gd:i:guessyear': 'gd:g:ontour', 'gd:i:dontsay': 'gd:g:ontour', 'gd:i:findwall': 'gd:g:ontour', 'gd:var:water': 'gd:g:ontour', 'gd:i:water': 'gd:var:water', 'gd:i:cookie': 'gd:var:water',
    'gd:overall': 'gt4', 'gd:app': 'gt4', 'gd:content': 'gt4', 'gd:fun': 'gt4', 'gd:seq': 'gt4', 'gd:g:play': 'gd:fun', s7ask: 'gt3', s7cam: 's7ask', 'gt:know': 'gt3', s7agent: 'gt3' };
  const par = await R(`var P=${JSON.stringify(PARENT)},o={};Object.keys(P).forEach(function(id){var d=ed.querySelector('[data-fo="'+id+'"]'),q=d&&d.parentElement.closest("details[data-fo]");o[id]=d?(q?q.dataset.fo:null):"missing"});o`);
  const wrong = Object.keys(PARENT).filter(id => par[id] !== PARENT[id]).map(id => id + ' in ' + par[id]);
  ok('[moved] Each moved foldout under its new parent, by id (the Voice test in Preview & Test; the four ways in under Ways in; Ask about the view in What the visitor sees; the games in 3 · Games on the tour, the quiz\'s pair as one, the cookie inside the breather; Fun keeps Play)', wrong.length === 0, wrong);
  const ask = await R('var c=ed.querySelector(\'[data-fo="s7cam"]\');[!!c&&c.previousElementSibling===rowEl(ed,"s7ask","Answers from"),!ed.querySelector(\'[data-fo="gd:fun"] [data-fo^="gd:i:quiz"]\'),!!ed.querySelector(\'[data-fo="s7dock"] [data-own="tx|s7tour"]\')]');
  ok('[moved] ...Ask about the view right after Answers from; no game left in Fun; Tour controls\' words in Guide panel', ask.every(Boolean), ask);
  // ---------- 2. nothing lost; no empty foldout; no section with a single row ----------
  const now = await R('var keys={},fos={};ed.querySelectorAll(".row").forEach(function(r){keys[rowKey(r)||("label:"+((r.querySelector("label")||{}).textContent||"").trim())]=1});ed.querySelectorAll("details[data-fo]").forEach(function(d){fos[d.dataset.fo]=1});[Object.keys(keys),Object.keys(fos),ed.querySelectorAll(".row").length]');
  const keySet = new Set(now[0]), foSet = new Set(now[1]), TAKEN = ['label:Water break after'], APART = { gt2: 's7mascot', s7tour: 's7dock' };
  const lostKeys = R13.keys.filter(k => !keySet.has(k) && TAKEN.indexOf(k) < 0), lostFos = R13.foldouts.map(f => f.id).filter(id => !foSet.has(id) && !APART[id]);
  ok(`Every row key of round 13's tab is still here (${R13.keys.length} keys, ${R13.rows} rows then; ${now[2]} rows now), but Water break after (taken out by the developer: Games on the tour holds it)`, lostKeys.length === 0, lostKeys);
  ok(`[moved] ...and every foldout id (${R13.foldouts.length}), but the two taken apart: the old Mascot section (gt2, around one foldout: Mascot is 1 · Azu now) and Tour controls (s7tour, no row: its words in Guide panel)`, lostFos.length === 0 && !foSet.has('gt2') && !foSet.has('s7tour'), lostFos);
  const empty = await R('var z=[],one=[];ed.querySelectorAll("details[data-fo]").forEach(function(d){if(!d.querySelector(".row,[data-tx]"))z.push(d.dataset.fo)});ed.querySelectorAll(":scope > details[data-fo]").forEach(function(d){if(d.querySelectorAll(".row").length<2)one.push(d.dataset.fo)});[z,one]');
  ok('No foldout without a row or a word to edit (Tour controls, Help and the Mix, the visitor\'s Details: rows now); no section with a single row', empty[0].length === 0 && empty[1].length === 0, empty);
  const nod = await R('rowsWithoutDemo(guideSecIds()).map(function(r){return r.key})');
  ok('...every row of the tab still has its demo (NO DEMO: none; a group\'s header is not a row)', nod.length === 0, nod);
  tsec('nothing lost');

  // ---------- 3. the levels: 1 the six sections, 2 their foldouts, an item's groups never by level ----------
  const lv = await R(`var o={};[1,2,3].forEach(function(l){S.ui.level=l;S.op={};renderEditor();var open=function(sel){return[].filter.call(ed.querySelectorAll(sel),function(d){return d.open}).length},all=function(sel){return ed.querySelectorAll(sel).length};
    o[l]={secs:open(":scope > details[data-fo]"),second:[].filter.call(ed.querySelectorAll(":scope > details > .inner > details[data-fo]"),function(d){return d.open}).map(function(d){return d.dataset.fo}).length,
      deep:["gd:visitor","gt:tools","s7agent"].filter(function(id){var d=ed.querySelector('[data-fo="'+id+'"]');return d&&d.open}),items:open('details[data-fo^="gd:i:"]'),itemsAll:all('details[data-fo^="gd:i:"]'),games:open('[data-fo="gd:g:ontour"] details[data-fo^="gd:i:"]'),gamesAll:all('[data-fo="gd:g:ontour"] details[data-fo^="gd:i:"]'),groups:open('details[data-fo^="gd:f:"]')}});o`);
  ok('Level 1: the six sections open, their foldouts closed; level 2: their foldouts open (an Editor or developer part, Details and The engine, one level later; round 19: Model tools is inside 1 · The mascot › Look, so it opens at level 4; an item closed); level 3: the games open to their group headers (4 · Azu speaks up\'s items one level deeper, in their groups)',
    lv[1].secs === 6 && lv[1].second === 0 && lv[2].second >= 14 && lv[2].deep.length === 0 && lv[2].items === 0 && lv[3].deep.length === 2 && lv[3].deep.indexOf('gt:tools') < 0 && lv[3].games === lv[3].gamesAll && lv[3].gamesAll === 7 && lv[3].items === 7, lv);
  ok('...an item\'s groups never open by level alone (levels 1, 2, 3: none open)', lv[1].groups === 0 && lv[2].groups === 0 && lv[3].groups === 0, lv);
  // the tab's names at the top: a real click opens that section
  await R('S.op={gt0:false,s7mascot:false,gt1:false,"gd:g:ontour":false,gt4:false,gt3:false};renderEditor()');
  const navs = await R('[].map.call(ed.querySelectorAll(".gtnav [data-gtgo]"),function(b){return b.textContent})');
  await p.locator('.gtnav [data-gtgo="gt3"]').click(); await W(250);
  const nv = await R('var f=ed.querySelector(\'[data-fo="gt3"]\'),r=f.getBoundingClientRect();[f.open,r.top>=-2&&r.top<innerHeight/2]');
  ok('The tab names its six parts at its top (the first view names every part of the guide); a real click on one opens it at the top of the view', JSON.stringify(navs) === JSON.stringify(['1 · The mascot', '2 · The tour', '3 · Games on the tour', '4 · The mascot speaks up', '5 · Ask the mascot', '6 · Preview & Test']) && nv[0] && nv[1], [navs, nv]);
  tsec('levels');

  // ---------- 4. an item: its group headers only, each with what it holds and its changed count ----------
  await guide(1, { gt4: true, 'gd:fun': true, 'gd:g:play': true });
  await p.locator('[data-fo="gd:i:joke"] > summary b').click(); await W(200);
  const gh = await R('var it=ed.querySelector(\'[data-fo="gd:i:joke"]\'),gs=[].slice.call(it.querySelectorAll(\':scope > .inner > details[data-fo^="gd:f:"]\'));[it.open,gs.map(function(g){return g.dataset.fo.split(":")[3]+"|"+g.open+"|"+(g.querySelector(".gdchg")||{dataset:{}}).dataset.changed}),[].filter.call(it.querySelectorAll(":scope > .inner .row"),function(r){return r.checkVisibility()}).length,(it.querySelector(\'[data-fo="gd:f:joke:when"] .gdgs\')||{}).textContent]');
  ok('An item opened (a real click on its name) shows its group headers only: Identity, Says, About, Show, When, Only if, How much, each closed, each 0 changed; no row visible; When says "after 3 pokes or a late pause"',
    gh[0] && JSON.stringify(gh[1]) === JSON.stringify(['identity|false|0', 'says|false|0', 'about|false|0', 'show|false|0', 'when|false|0', 'only|false|0', 'much|false|0']) && gh[2] === 0 && /after 3 pokes or a late pause/.test(gh[3]), gh);
  await p.locator('[data-fo="gd:f:joke:much"] > summary').click(); await W(150);
  await p.locator('[data-fo="gd:f:joke:much"] [data-set="@guidance.items.joke.essential"][data-v="true"]').click(); await W(250);
  const c1 = await R('[S.guidance.items.joke.essential,+ed.querySelector(\'[data-fo="gd:f:joke:much"] .gdchg\').dataset.changed,ed.querySelector(\'[data-fo="gd:f:joke:much"]\').open,ed.querySelector(\'[data-fo="gd:f:joke:much"] .gdchg\').textContent,+ed.querySelector(\'[data-fo="gd:f:joke:when"] .gdchg\').dataset.changed]');
  ok('...one field changed by a real click (How much › Essential Yes): its group says 1 changed, the others 0; the group stays open', c1[0] === true && c1[1] === 1 && c1[2] && c1[3] === '1 changed' && c1[4] === 0, c1);
  await p.locator('[data-gdreset="joke|much"]').click(); await W(250);
  const c2 = await R('[S.guidance.items.joke.essential,+ed.querySelector(\'[data-fo="gd:f:joke:much"] .gdchg\').dataset.changed,!ed.querySelector(\'[data-gdreset="joke|much"]\'),(document.getElementById("gdedtoast")||{}).textContent||""]');
  ok('...Reset (a real click): back to the item\'s defaults, 0 changed, said in the Editor\'s toast', c2[0] === false && c2[1] === 0 && c2[2] && /Reset: 1 field of the “A joke” back to its defaults/.test(c2[3]), c2);
  await p.locator('[data-fo="gd:f:joke:much"] [data-set="@guidance.items.joke.essential"][data-v="true"]').click(); await W(200);
  await p.locator('[data-fo="gd:f:joke:much"] [data-set="@guidance.items.joke.essential"][data-v="false"]').click(); await W(200);
  const c3 = await R('[S.guidance.items.joke.essential,+ed.querySelector(\'[data-fo="gd:f:joke:much"] .gdchg\').dataset.changed]');
  ok('...the default clicked again (Essential No): 0 changed', c3[0] === false && c3[1] === 0, c3);
  tsec('groups');

  // ---------- 5. Says, one list ----------
  await p.locator('[data-fo="gd:f:joke:says"] > summary').click(); await W(150);
  const sl = await R('var g=ed.querySelector(\'[data-fo="gd:f:joke:says"]\');[[].map.call(g.querySelectorAll("tr[data-gdvark]"),function(r){return r.dataset.gdvark+"|"+r.querySelectorAll("[data-tx]").length+"|"+!!r.querySelector("[data-gdsay]")+"|"+!!r.querySelector("[data-gdvardel]")}),!!rowEl(ed,"gd:f:joke:says","Per audience"),!!rowEl(ed,"gd:f:joke:says","Returning visitor"),g.querySelectorAll(".ownpanel").length,!ed.querySelector(\'[data-fo="gd:i:joke"] .gdvt\')]');
  ok('Says is one list: each line once, EN | PT side by side, with ▸ and Remove; Per audience and Returning visitor under it; no second Words table of the same lines',
    sl[0].length === 3 && sl[0].every(x => /\|2\|true\|true$/.test(x)) && sl[1] && sl[2] && sl[3] === 0 && sl[4], sl);
  const pt = p.locator('[data-fo="gd:f:joke:says"] [data-tx="mascot_joke_2|pt"]');
  await pt.fill('O azul é a minha cor preferida. Obviamente.'); await pt.press('Tab'); await W(250);
  const w1 = await R('[S.tx.mascot_joke_2.pt,TXOWN.mascot_joke_2,+ed.querySelector(\'[data-fo="gd:f:joke:says"] .gdchg\').dataset.changed]');
  await p.locator('.utab[data-tab="words"]').click(); await W(300);
  const lw = await R('var i=ed.querySelector(\'input[data-loc="tx:mascot_joke_2|pt"]\'),tr=i&&i.closest("tr");[i?i.value:null,tr?tr.textContent:""]');
  ok('A real edit of a line\'s PT text is Languages & Terms\' own word (the same key, owned by the item): the words tab shows it; the group says 1 changed', w1[0] === 'O azul é a minha cor preferida. Obviamente.' && w1[1] === 'gd:i:joke' && w1[2] === 1 && lw[0] === w1[0] && /A joke/.test(lw[1]), [w1, lw]);
  const go = p.locator('tr:has(input[data-loc="tx:mascot_joke_2|pt"]) [data-goloc]'); await go.scrollIntoViewIfNeeded(); await go.click(); await W(400);
  const gw = await R('var s=ed.querySelector(\'[data-fo="gd:f:joke:says"]\'),i=ed.querySelector(\'[data-fo="gd:i:joke"]\'),r=i&&i.getBoundingClientRect();[S.tab,!!s&&s.open,!!i&&i.open,!!r&&r.top<innerHeight&&r.bottom>0]');
  ok('...its Go to (a real click) opens the Guide tab at the item, its Says group open (Languages & Terms\' owner link: Guide › Azu speaks up › Fun › Play › A joke)', gw[0] === 'guide' && gw[1] && gw[2] && gw[3] && await R('ownerName("gd:i:joke")==="Guide › The mascot speaks up › Fun › Play › A joke"'), gw);
  await p.locator('[data-fo="gd:f:joke:says"] [data-gdvaradd="joke"]').click(); await W(200);
  const a1 = await R('var s=[].concat(S.guidance.items.joke.says);[s.length,TXOWN[s[3]],ed.querySelectorAll(\'[data-fo="gd:f:joke:says"] tr[data-gdvark]\').length,/4 variants: the most/.test(ed.querySelector(\'[data-fo="gd:f:joke:says"]\').textContent)]');
  ok('+ Add a variant (a real click): a fourth line, its words owned by the item, the list at its most', a1[0] === 4 && a1[1] === 'gd:i:joke' && a1[2] === 4 && a1[3], a1);
  const k2 = await R('[].concat(S.guidance.items.joke.says)[1]');
  await p.locator('[data-fo="gd:f:joke:says"] [data-gdvardel="joke|1"]').click(); await W(250);
  const r1 = await R(`var s=[].concat(S.guidance.items.joke.says);[s.length,s.indexOf("${k2}"),(document.getElementById("gdedtoast")||{}).textContent||""]`);
  await p.locator('#gdedtoast [data-gdedundo="var"]').click(); await W(250);
  const r2 = await R(`var s=[].concat(S.guidance.items.joke.says);[s.length,s.indexOf("${k2}"),(document.getElementById("gdedtoast")||{}).textContent||"",ed.querySelectorAll('[data-fo="gd:f:joke:says"] tr[data-gdvark]').length]`);
  ok('Remove (a real click) takes line 2 out and says so with Undo (no popup); Undo (a real click) puts it back in its place, its words unchanged', r1[0] === 3 && r1[1] < 0 && /Removed line 2 of the “A joke”/.test(r1[2]) && /Undo/.test(r1[2]) && r2[0] === 4 && r2[1] === 1 && /Undone: line 2/.test(r2[2]) && r2[3] === 4, [r1, r2]);
  await R('var s=[].concat(S.guidance.items.joke.says);s.pop();S.guidance.items.joke.says=s;S.tx.mascot_joke_2=JSON.parse(JSON.stringify(TXDEF.mascot_joke_2));renderEditor()');
  tsec('says');

  // ---------- 6. a point's own field: its link opens Specific Marker › Guide script at that field ----------
  await guide(1, { 'gd:g:ontour': true, 'gd:i:dontsay': true });
  const pl = await R('var l=ed.querySelector(\'[data-fo="gd:i:dontsay"] .gdpp\');l?l.textContent:""');
  ok('Don\'t say it! shows the demo point\'s word: "This point\'s word: EN palace (Reviewed) · PT palácio (Reviewed) · Specific Marker › Guide script ›"', /^This point's word: EN palace \(Reviewed\) · PT palácio \(Reviewed\) · Specific Marker › Guide script ›/.test(pl), pl);
  await p.locator('[data-fo="gd:i:dontsay"] .gdpplink').click(); await W(500);
  const ld = await R('var b=ed.querySelector(".gdwbox"),r=b&&b.getBoundingClientRect(),m=ed.querySelector(\'[data-fo="m3"]\');[S.tab,!!m&&m.open,document.body.dataset.gdland,!!r&&r.top>=0&&r.top<innerHeight,!!b&&b.classList.contains("gdland")]');
  ok('...its link (a real click) opens Specific Marker › Guide script at the word: the tab, Guide script open, the word\'s box in view and marked', ld[0] === 'marker' && ld[1] && ld[2] === 'word|found' && ld[3] && ld[4], ld);
  await guide(1, { 'gd:g:ontour': true, 'gd:pair:quizpromise': true, 'gd:i:quizfinale': true });
  await p.locator('[data-fo="gd:i:quizfinale"] .gdpplink').click(); await W(500);
  const lq = await R('var h=ed.querySelector(\'[data-gdland="q"]\'),r=h&&h.getBoundingClientRect();[S.tab,document.body.dataset.gdland,S.sel.gstep,!!r&&r.top>=0&&r.top<innerHeight]');
  ok('...The quiz\'s link opens it at a step\'s quiz question (the step chosen, its question in view)', lq[0] === 'marker' && lq[1] === 'q|found' && /^g\d+$/.test(lq[2]) && lq[3], lq);
  const others = await R('["guessyear","findwall","ptjoke","inview","tourof"].map(function(id){var it=S.guidance.items[id],h=gdPointLine(it);return id+"|"+(/data-gdscript="([^"]+)"/.exec(h)||[0,""])[1]})');
  ok('...each item that uses a point\'s own field has its line and link (Guess the year\'s guess, Find it on the wall\'s find, a point\'s joke, What\'s in view\'s line, a tour\'s steps)', others.every(x => /\|(guess:g\d+|find|jokes|steps)$/.test(x)), others);
  tsec('links');

  // ---------- 7. an old open state opens the new place; a foldout added later is never lost ----------
  await R('S.tab="guide";S.ui.level=1;S.op={gt4:true,"gd:fun":true,"gd:g:ontour":true,gt2:false};renderEditor()');
  const os = await R('var g=ed.querySelector(\'[data-fo="gd:g:ontour"]\'),m=ed.querySelector(\'[data-fo="s7mascot"]\');[!!g&&g.open,!!g&&!g.parentElement.closest("[data-fo]"),!!m&&!m.open,S.op.hasOwnProperty("gt2")]');
  ok('An old open state opens the new places: gd:fun > gd:g:ontour opens 3 · Games on the tour (its id); the old Mascot section closed (gt2) closes 1 · Azu, once', os[0] && os[1] && os[2] && !os[3], os);
  await R('window.__later=1;on("edCard.html",function(h){if(!window.__later)return h;var d=document.createElement("div");d.innerHTML=h;var s=innerOf(d,"s7");if(s)s.insertAdjacentHTML("beforeend",fo("s7later","A later part",null,row("Later",pseg("guideCfg.later",["a","b"],["A","B"]),""),2));return d.innerHTML});renderEditor()');
  const lt = await R('var f=ed.querySelector(\'[data-fo="s7later"]\'),q=f&&f.parentElement.closest("[data-fo]");[!!f,q&&q.dataset.fo,!!f&&!f.nextElementSibling]');
  await R('window.__later=0;renderEditor()');
  ok('A foldout a module adds later with no place here lands at the end of 2 · The tour, said in the console', lt[0] && lt[1] === 'gt1' && lt[2] && warns.some(w => /s7later has no place in GUIDE_PLAN/.test(w)), [lt, warns.slice(-2)]);
  await R('S.tab="card";S.ui.level=9;S.op={};renderEditor()');
  const dc = await R('[ed.querySelectorAll(\'[data-fo^="s7"],[data-fo^="gd:"],[data-fo^="gt"]\').length,!!ed.querySelector(\'[data-fo="s8"]\')]');
  ok('The Detail Card shows no guide foldout (6 · Audiences stays there)', dc[0] === 0 && dc[1], dc);
  tsec('state');

  // ---------- 8. the draw: now, and round 13's on master's build (the same measure) ----------
  // the same machine, the same moment (a busy computer slows both): round 13's draw built the whole Detail Card document every time
  // (edCardDoc(), most of its cost); this round's whole draw must take less than that document alone. round 13's own numbers, measured on
  // master's build the same way, are printed beside (guide_rows_r13.json's drawMs)
  const tm = await R('S.tab="guide";var o={},med=function(f){var t=[];for(var i=0;i<5;i++){var t0=performance.now();f();t.push(performance.now()-t0)}t.sort(function(a,b){return a-b});return Math.round(t[2])};[1,9].forEach(function(l){S.ui.level=l;S.op={};renderEditor();o["level"+l]=med(renderEditor)});S.ui.level=1;o.cardDoc=med(function(){edCardDoc()});o.guideDoc=med(function(){edCardDoc("guide")});o');
  console.log('DRAW now ' + JSON.stringify(tm) + ' · round 13 ' + JSON.stringify(R13.drawMs));
  ok(`The Guide tab's full redraw is faster than round 13's: ${tm.level1} ms at level 1 and ${tm.level9} ms with every foldout open, against the Detail Card document round 13 built for every draw, alone ${tm.cardDoc} ms (the guide's part ${tm.guideDoc} ms); round 13's draw on master's build: ${R13.drawMs.level1} / ${R13.drawMs.level9} ms`, tm.level1 < tm.cardDoc && tm.level9 < tm.cardDoc * 1.3 && tm.guideDoc * 4 < tm.cardDoc, tm);
  ok('No error on the page', errs.length === 0, errs);
  tsec('end');
  await b.close();
  process.exit(fails ? 1 : 0);
})();
