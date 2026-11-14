// test9o (Playwright + the installed Edge): node test9o.js -> PASS/FAIL per check. Guidance round 11: the guide panel's four Looks
// (Paper glass, Ink glass, Player sheet, Teleprompter), each in Mini and Open, light and dark (its parts, inside the phone, 0 px over the
// words, 44 px targets, 4.5:1); the speed chip's seven steps (the voice's rate follows); Previous / Next part; Pause / Play; Stop and its
// Undo; a real drag on the progress bar seeking to a stop and a sentence; the bars moving while the voice speaks and still when paused;
// the words moving up, Aa and Read all; a typed question and a suggestion tapped; the ✦ growing into the panel and back (hidden while
// open), Reduce Motion's crossfade; Fun held during a tour (Why not now? says so) and a Moment's prop never over words (it waits for
// room); the green ▸ (its size and contrast, still pressing the demo); the music rows (the volume and the ducking read from the gain,
// the tracks, the credit); every new row's demo. MUTATE=1 node test9o.js lets Fun through during a tour: the checks named [fun hold]
// turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  // round 21 (21.10): an Editor row inside closed foldouts (the accordions of rounds 20 and 21) is reached as the developer does: each
  // closed foldout above it opened by a real click on its header, from the outside in, then the row's own control clicked (edClick)
  const openAbove = async sel => { for (let i = 0; i < 8; i++) { const m = await p.evaluate(s => LAB.run(`(function(){var b=document.querySelector(${JSON.stringify(s)});if(!b)return false;var ds=[];for(var d=b.closest("details");d;d=d.parentElement&&d.parentElement.closest("details"))ds.unshift(d);var c=ds.filter(function(d){return!d.open})[0];if(!c)return false;c.querySelector(":scope > summary").setAttribute("data-t21","1");return true})()`), sel); if (!m) return; await p.locator('summary[data-t21="1"]').first().click(); await p.evaluate(() => LAB.run('document.querySelectorAll("[data-t21]").forEach(function(x){x.removeAttribute("data-t21")});""')); await p.waitForTimeout(120); } };
  const edClick = async (sel, o) => { await openAbove(sel); await p.locator(sel).first().click(o); };
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const box = async sel => p.locator('#ph ' + sel).first().boundingBox();
  const tap = async sel => { const r = await box(sel); if (!r) return false; await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return true; };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  if (process.env.MUTATE) { await R('var gdCheck0=gdCheck;gdCheck=function(it,items,v,ctx,now){if(it.kind==="fun"&&ctx.tour)ctx=Object.assign({},ctx,{tour:Object.assign({},ctx.tour,{paused:true})});return gdCheck0(it,items,v,ctx,now)}'); console.log('MUTATION: Fun is let through during a tour (the hold removed)'); }
  // the device voice, muted: its words go on by time, the same on every run; the guidance quiet unless a check asks for it
  const base = 'S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.miniCtl="pausestop";S.guideCfg.captions=true;S.guideCfg.pWords="prompter";S.guideCfg.pBars=true;S.guideCfg.pSpeed=true;S.guideCfg.chips=true;S.guideCfg.speed=1;S.vis.speed=null;S.guidance.on=false;S.guideCfg.music=false;S.vis.music=null;S.vis.musicVol=null;';
  const tourAt = async (look, id, h, dark) => { await R(base + `S.dark=${!!dark};S.cardCfg.mode=${dark ? '"dark"' : '"light"'};S.guideCfg.look="${look}";if(G.on)stopTour();S.view="card";S.stop="full";renderAll(false);startTour();var j=S.guideSteps.findIndex(function(s){return s.id==="${id}"});if(j>0){stopSpeech();G.i=j;runStep()}S.dockH="${h || 'mini'}";renderGuide()`); await W(700); };

  const defs = await R('[S.guideCfg.look,S.guideCfg.pWords,S.guideCfg.pBars,S.guideCfg.pSpeed,S.guideCfg.music,S.guideCfg.musicTrack,S.guideCfg.musicVol,S.guideCfg.musicDuck,S.guideCfg.musicVis]');
  ok('The new rows\' defaults: Look Ink glass, Words Teleprompter, Voice moving On, Speed button Shown; Music bed Off, Track Exploration, Music volume 15 %, Under the voice 30 %, Visitor can choose Shown', JSON.stringify(defs) === JSON.stringify(['ink', 'prompter', true, true, false, 'exploration', 15, 30, true]), defs);
  // ---------- 1. each Look, Mini and Open, light and dark ----------
  const CR = 'var px=function(c){var m=c.match(/[\\d.]+/g).map(Number);return{r:m[0],g:m[1],b:m[2],a:m.length>3?m[3]:1}},lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},cr=function(a,b){var x=lum(a),y=lum(b);return(Math.max(x,y)+.05)/(Math.min(x,y)+.05)};';
  const audit = h => R(CR + `var d=ph.querySelector(".gdock"),pr=ph.getBoundingClientRect(),r=d.getBoundingClientRect(),o={};
    o.inside=r.left>=pr.left-1&&r.right<=pr.right+1&&r.bottom<=pr.bottom+1&&r.top>=pr.top-1;
    o.small=[].filter.call(d.querySelectorAll("button,.gd-seek"),function(x){var q=x.getBoundingClientRect();return q.width&&(q.width<43.5||q.height<43.5)}).map(function(x){return x.className});
    var st=stepEl(S.guideSteps[G.i]),over=0;if(st){var w=document.createTreeWalker(st,NodeFilter.SHOW_TEXT),n,rg=document.createRange();while((n=w.nextNode())){if(!/\\S/.test(n.nodeValue))continue;rg.selectNodeContents(n);[].forEach.call(rg.getClientRects(),function(q){var a=Math.max(0,Math.min(q.right,r.right)-Math.max(q.left,r.left))*Math.max(0,Math.min(q.bottom,r.bottom)-Math.max(q.top,r.top));over+=a})}}o.over=Math.round(over);
    var tws=[].map.call(d.querySelectorAll(".gd-tp .tw"),function(x){return x.getBoundingClientRect()}),bs=[].map.call(d.querySelectorAll("button"),function(x){return x.getBoundingClientRect()}),self=0;tws.forEach(function(t){bs.forEach(function(q){self+=Math.max(0,Math.min(q.right,t.right)-Math.max(q.left,t.left))*Math.max(0,Math.min(q.bottom,t.bottom)-Math.max(q.top,t.top))})});o.self=Math.round(self);
    var surf=px(getComputedStyle(d).getPropertyValue("--surfsolid").trim().replace(/^#(..)(..)(..)$/,function(m,a,b,c){return"rgb("+parseInt(a,16)+","+parseInt(b,16)+","+parseInt(c,16)+")"}));
    o.cr=[".gd-stop:not(.vh)",".gd-time",".gd-tp .tp-now .tw.said",".gd-chips [data-chip]",".gd-mt"].map(function(s){var e=d.querySelector(s);if(!e)return null;var c=px(getComputedStyle(e).color),bg=getComputedStyle(e).backgroundColor,bb=px(bg);var under=bb.a>.9?bb:surf;return+cr(c,under).toFixed(2)}).filter(function(x){return x!=null});
    o.parts=${h === 'mini' ? `[".gd-mini [data-g=pause]",".gd-mini [data-g=stop]",".gd-grab",".gd-hide",".gd-line"]` : `[".gd-speed",".gd-ctl [data-g=prev]",".gd-ctl [data-g=pause]",".gd-ctl [data-g=next]",".gd-ctl [data-g=stop]",".gd-seek",".gd-time",".gd-tp",".gd-aa",".gd-all",".gd-chips [data-chip]",".gd-talk",".gd-grab"]`}.filter(function(s){return!d.querySelector(s)});
    o.form=${h === 'mini' ? 'true' : '!!d.querySelector("#gq")||!!d.querySelector(".gd-kb")'};o;`);
  for (const dark of [false, true]) for (const L of ['paper', 'ink', 'player', 'prompter']) {
    await tourAt(L, 'g4', 'mini', dark); const mi = await audit('mini');
    ok(`${L} ${dark ? 'dark' : 'light'} · Mini: its parts (Pause, Stop, open, hide, the line), inside the phone, 0 px over the words told, 44 px targets, its words 4.5:1`, mi.inside && !mi.parts.length && mi.over === 0 && !mi.small.length && mi.cr.every(v => v >= 4.5), mi);
    await tourAt(L, 'g5', 'peek', dark); const op = await audit('open');
    ok(`${L} ${dark ? 'dark' : 'light'} · Open: speed, previous, play, next, stop, the bar, the time, the words, Aa, Read all, the questions, ask (typed and by voice), inside, 0 px over the words told and its own words, 44 px, 4.5:1`, op.inside && !op.parts.length && op.form && op.over === 0 && op.self === 0 && !op.small.length && op.cr.every(v => v >= 4.5), op);
  }
  await R('if(G.on)stopTour();S.dark=false;S.cardCfg.mode="light";renderAll(false)');
  tsec('looks');

  // ---------- 2. the speed chip: seven steps, the voice's rate follows ----------
  await tourAt('ink', 'g5', 'peek'); await until('G.timer||G.utter', 3000);
  const sp = []; for (let i = 0; i < 7; i++) { await tap('.gd-speed'); await W(120); sp.push(await R('[ph.querySelector(".gd-speed").textContent,G.rate,guideSpeed(),!!(G.timer||G.utter)]')); }
  ok('The speed chip: 1× → 1.25× · 1.5× · 1.75× · 2× · 0.5× · 0.75× · 1× (real taps), the voice\'s rate follows at once and it keeps speaking', JSON.stringify(sp.map(x => x[0])) === JSON.stringify(['1.25×', '1.5×', '1.75×', '2×', '0.5×', '0.75×', '1×']) && sp.every(x => Math.abs(x[1] - parseFloat(x[0])) < 1e-6 && x[1] === x[2] && x[3]), sp);
  await R('S.guideCfg.speed=0.75;S.vis.speed=null;renderGuide()');
  ok('...its first value is Narration › Speed (its range 0.5-2), and the time follows the speed', await R('S.tab="guide";renderEditor();var s=ed.querySelector(\'input[type=range][data-num="@guideCfg.speed"]\');var a=stopSecs(1);S.vis.speed=1.5;var c=stopSecs(1);S.vis.speed=null;ph.querySelector(".gd-speed").textContent==="0.75×"&&Math.abs((a-.7)*.75-(c-.7)*1.5)<.01&&!!s&&s.min==="0.5"&&s.max==="2"'));
  await R('S.guideCfg.speed=1;renderGuide()');

  // ---------- 3. Previous / Next part, Pause / Play, Stop and Undo ----------
  await tourAt('ink', 'g5', 'peek'); const i0 = await R('G.i');
  await tap('.gd-ctl [data-g="next"]'); await W(250); const i1 = await R('G.i');
  await tap('.gd-ctl [data-g="prev"]'); await W(120); await tap('.gd-ctl [data-g="prev"]'); await W(300); const i2 = await R('G.i');
  ok('Next part goes to the stop after; Previous (twice: the sentence again, then the stop before) to the stop before (real taps)', i1 === i0 + 1 && i2 === i0, [i0, i1, i2]);
  await tap('.gd-ctl [data-g="pause"]'); await W(200); const pz = await R('[G.paused,!!ph.querySelector(\'.gd-ctl [data-g="play"]\')]');
  await tap('.gd-ctl [data-g="play"]'); await W(200);
  ok('Pause / Play: the large button pauses (it becomes Play) and plays again', pz[0] && pz[1] && await R('!G.paused'), pz);
  await until('G.hl&&G.hl.k>=4', 6000); const at = await R('[G.i,S.guideSteps[G.i].id]');
  await tap('.gd-ctl [data-g="stop"]'); await W(250); const st = await R('[G.on,!!ph.querySelector(".gtoast")]');
  await tap('[data-gstop="resume"]'); await W(500);
  ok('Stop ends the tour with today\'s Undo toast; its Resume goes back to the same stop', !st[0] && st[1] && await R(`G.on&&S.guideSteps[G.i].id===${JSON.stringify(at[1])}`), [st, at, await R('S.guideSteps[G.i].id')]);

  // ---------- 4. a real drag on the progress bar: to a stop and a sentence ----------
  await tourAt('ink', 'g1', 'peek'); await W(300);
  const sk = await box('.gd-seek'), seg = await R('var s=ph.querySelector(\'.gd-seek [data-gseg="9"]\').getBoundingClientRect();[s.left,s.width]');
  const tx = seg[0] + seg[1] * 0.62, ty = sk.y + sk.height / 2, want = await R('var ws=stepText(S.guideSteps[9]).match(/\\S+/g),k=Math.min(ws.length-1,Math.floor(.62*ws.length)),s=0;for(var i=0;i<k;i++)if(/[.!?…]["”»)]*$/.test(ws[i]))s=i+1;s');
  ok('The bar\'s hit area is 44 px high, one segment a stop', sk.height >= 43.5 && await R('ph.querySelectorAll(".gd-seek [data-gseg]").length===S.guideSteps.length'), [sk.height]);
  await p.mouse.move(sk.x + 6, ty); await p.mouse.down(); await p.mouse.move(tx, ty, { steps: 10 });
  const mid = await R('var k=ph.querySelector(".gd-seek .sk-knob").getBoundingClientRect();[k.left+k.width/2,!!G.seekDrag,G.seekDrag&&G.seekDrag.j,ph.querySelector(".gd-seek").classList.contains("dragging"),ph.querySelector(".gd-time").textContent]');
  await p.mouse.up(); await W(400);
  const after = await R('[G.i,G.seekedTo&&G.seekedTo.k,G.hl&&G.hl.k,!!(G.timer||G.utter)]');
  ok('A real drag: the knob follows the finger and the time shows where it will go; on release the tour is at that stop (10) and that sentence (not its first), speaking from it', Math.abs(mid[0] - tx) < 3 && mid[1] && mid[2] === 9 && mid[3] && after[0] === 9 && want > 0 && after[1] === want && after[3], [mid, after, want]);
  await until(`G.hl&&G.hl.k>=${want}`, 3000);
  ok('...the voice goes on from the sentence\'s first word (the words lit from there)', await R(`G.hl.k>=${want}`), await R('G.hl.k'));
  // round 21 (21.1): Ink glass's Open shows the time at the progress line's two ends (where the tour is, and its whole), no longer "m:ss / m:ss"
  const tm = await R('var e=ph.querySelector(".gd-time[data-t=el]"),t=ph.querySelector(".gd-time[data-t=tot]");[e&&e.textContent,!!e&&/^\\d+:\\d\\d$/.test(e.textContent),mmss(dkTimes().tot),t&&t.textContent,mmss(dkElapsed(dkTimes()))]');
  const sec = s => { const a = String(s || '').split(':'); return +a[0] * 60 + +a[1]; };
  ok('The time from the words\' timing at the progress line\'s ends (round 21): where the tour is, and its whole length', tm[1] && tm[3] === tm[2] && Math.abs(sec(tm[0]) - sec(tm[4])) <= 1, tm);

  // ---------- 5. the voice moving: bars move while it speaks, still when paused ----------
  await tourAt('ink', 'g5', 'mini'); await until('G.timer||G.utter', 3000); await W(300);
  const sample = async () => { const out = []; for (let i = 0; i < 6; i++) { out.push(await R('[].map.call(ph.querySelectorAll(".gdock .gd-bars i"),function(i){return i.style.transform}).join("|")')); await W(110); } return out; };
  const moving = await sample(); const lvl = await R('G.barsLvl');
  await R('pauseTour()'); await W(700); const still = await sample();
  ok('The bars move only while the voice speaks (its word events) and are still when paused', new Set(moving).size >= 3 && new Set(still).size === 1 && still[0].split('|').every(t => /scaleY\(0\.18/.test(t)) && await R('voiceLevel()===0'), [moving.slice(0, 2), still[0], lvl]);
  await R('resumeTour()');
  await R('S.guideCfg.pBars=false;renderGuide()'); ok('Voice moving Off: no bars', await R('!ph.querySelector(".gd-bars")')); await R('S.guideCfg.pBars=true;renderGuide()');

  // ---------- 6. the words like a teleprompter; Aa; Read all ----------
  await tourAt('paper', 'g5', 'peek'); await until('G.timer||G.utter', 3000);
  const w0 = await R('var t=ph.querySelector(".gd-tp");[+t.dataset.line,(t.querySelector(".tw.on")||{}).dataset]');
  const moved = await until('var t=ph.querySelector(".gd-tp");+t.dataset.line>' + (w0[0] || 0) + '&&t.classList.contains("tp-up")', 15000);
  const w1 = await R('var t=ph.querySelector(".gd-tp"),n=t.querySelector(".tp-now"),pv=t.querySelector(".tp-prev"),nx=t.querySelector(".tp-next");[+t.dataset.line,!!n.querySelector(".tw.on"),pv.textContent.length>0,nx?getComputedStyle(nx).color:"",getComputedStyle(n).fontSize,getComputedStyle(pv).fontSize,pv.getBoundingClientRect().bottom<=n.getBoundingClientRect().top+1]');
  ok('The words like a teleprompter: the line said now (its word marked), the line before faded above it, the next below; a new line moves up (no fixed bubble)', moved && w1[1] && w1[2] && parseFloat(w1[4]) > parseFloat(w1[5]) && w1[6] && !await R('!!ph.querySelector(".gbubble")'), [w0, w1]);
  await tap('.gd-aa'); await W(200); const aa = await R('[S.guideCfg.captions,!!ph.querySelector(".gd-tp .tw"),ph.querySelector(".gd-aa").getAttribute("aria-pressed")]');
  await tap('.gd-aa'); await W(200);
  ok('Aa hides the words and shows them again', aa[0] === false && !aa[1] && aa[2] === 'false' && await R('S.guideCfg.captions&&!!ph.querySelector(".gd-tp .tw")'), aa);
  await tap('.gd-all'); await W(250); const ra = await R('[S.dockH,!!ph.querySelector(".gd-thread .gm-now"),ph.querySelectorAll(".gd-thread .gm-azu").length,ph.querySelector(".gd-thread .gm-now")&&ph.querySelector(".gd-thread .gm-now").textContent===stepText(S.guideSteps[G.i])]');
  ok('Read all shows the whole text (Full): the tour\'s lines so far, the line said now marked whole', ra[0] === 'full' && ra[1] && ra[2] >= 1 && ra[3], ra);
  await tap('.gd-all'); await W(200); ok('...and back to the line said now (Peek)', await R('S.dockH==="peek"&&!!ph.querySelector(".gd-tp")'));
  await R('S.guideCfg.pWords="off";renderGuide()'); ok('Words Off: no words in the panel, no Aa', await R('!ph.querySelector(".gd-tp")&&!ph.querySelector(".gd-aa")')); await R('S.guideCfg.pWords="prompter";renderGuide()');

  // ---------- 7. asking: typed, a suggestion ----------
  await tourAt('ink', 'g5', 'peek');
  await p.locator('#ph #gq').fill('Who lived here?'); await p.locator('#ph #gq').press('Enter'); await W(700);
  ok('A typed question (real keys): asked, in the whole text (Full) with its answer', await R('S.dockH==="full"&&[].some.call(ph.querySelectorAll(".gm-me"),function(e){return e.textContent==="Who lived here?"})&&ph.querySelectorAll(".gd-thread .gm-azu").length>=1'));
  await R('S.dockH="peek";G.askOpen=false;renderGuide()'); await tap('[data-chip="1"]'); await W(700);
  ok('A suggested question tapped: asked', await R('[].some.call(ph.querySelectorAll(".gm-me"),function(e){return e.textContent===L(S.guideAsks[1])})'));
  await tourAt('prompter', 'g5', 'peek'); await tap('.gd-kb'); await W(250);
  ok('The keyboard (Teleprompter): Open with the typed question focused', await R('S.dockH==="peek"&&document.activeElement&&document.activeElement.id==="gq"'));
  await R('S.guideCfg.chips=false;renderGuide()'); ok('Suggested questions Hidden: no chips', await R('!ph.querySelector("[data-chip]")')); await R('S.guideCfg.chips=true;renderGuide()');
  tsec('controls');

  // ---------- 8. the ✦ grows into the panel and back ----------
  await R(base + 'S.guideCfg.look="ink";if(G.on)stopTour();G.fab.menu=false;S.view="card";S.stop="full";renderAll(false);scr.scrollTop=900;fabLayer()'); await W(500);
  const star = await R('var b=ph.querySelector(".gbtn-wrap .gbtn").getBoundingClientRect();[b.left,b.top,b.width]');
  const g0 = await R('gbGo("tour");var s=ph.querySelector(".gd-morph"),r=s&&s.getBoundingClientRect();[!!s,r&&Math.round(r.left),r&&Math.round(r.width),!!ph.querySelector(".gbtn-wrap")]');
  await W(160); const g1 = await R('var s=ph.querySelector(".gd-morph"),r=s&&s.getBoundingClientRect();[!!s,r&&Math.round(r.width)]');
  await W(400); const g2 = await R('var d=ph.querySelector(".gdock").getBoundingClientRect();[!!ph.querySelector(".gd-morph"),!!ph.querySelector(".gbtn-wrap"),Math.round(d.width),ph.querySelector(".gdock").classList.contains("gd-in")]');
  ok('A tour started from the ✦: its circle widens from where the ✦ was into the panel (320 ms), the ✦ gone', g0[0] && Math.abs(g0[1] - star[0]) < 4 && Math.abs(g0[2] - star[2]) < 6 && !g0[3] && g1[0] && g1[1] > g0[2] && !g2[0] && !g2[3], [star, g0, g1, g2]);
  ok('...and the ✦ stays hidden while the panel shows (Mini, Peek, Full)', !g2[1] && await R('S.dockH="peek";renderGuide();var a=!ph.querySelector(".gbtn-wrap");S.dockH="full";renderGuide();var c=!ph.querySelector(".gbtn-wrap");S.dockH="mini";renderGuide();a&&c&&!ph.querySelector(".gbtn-wrap")'));
  await W(400); await tap('.gd-hide');
  const sh = await R('[S.dockH,!!ph.querySelector(".gd-morph.shrink"),!ph.querySelector(".gdock")]'); await W(450);
  const sh2 = await R('var w=ph.querySelector(".gbtn-wrap.intour");[!!w,w&&w.classList.contains("gd-arriving"),!!ph.querySelector(".gd-morph")]');
  ok('Closing (the hide chevron, a real tap) shrinks the panel back into the ✦, which then shows with its ring and pause', sh[0] === 'compact' && sh[1] && sh[2] && sh2[0] && !sh2[1] && !sh2[2] && await R('!!ph.querySelector(".gbtn.tour .gb-ring")&&!!ph.querySelector(".gb-pause")'), [sh, sh2]);
  await tap('.gbtn.tour'); const back = await R('[S.dockH,!!ph.querySelector(".gd-morph.grow")]'); await W(450);
  ok('...a tap on the ✦ grows it into the panel again (Mini)', back[0] === 'mini' && back[1] && await R('!ph.querySelector(".gbtn-wrap")&&!!ph.querySelector(".gdock.h-mini")'), back);
  await R('stopTour();S.reduce=true;renderAll(false);scr.scrollTop=900;fabLayer()'); await W(300);
  const rm = await R('gbGo("tour");var d=ph.querySelector(".gdock");[!!ph.querySelector(".gd-morph"),d.getAnimations().some(function(a){var k=a.effect&&a.effect.getKeyframes();return k&&k.length&&k[0].opacity==="0"})]');
  ok('Reduce Motion: a crossfade (no growing shape)', !rm[0] && rm[1], rm);
  await R('stopTour();S.reduce=false;renderAll(false)');
  tsec('morph');

  // ---------- 9. Fun never during a tour, never over words ----------
  const fun = await R(base + 'S.guidance.on=true;S.guidance.items.water.talkFor=1;var items=gdItems(),V=S.gdVisitor,c=gdContext(),out={};c.moment="between";c.tour={between:true,step:2,talked:99,toEnd:false,paused:false};c.onWall=false;c.solemn=false;' +
    'out.water=gdWhyNot(items,V,Object.assign({},c,{moment:null}),gdNow()+600000,"hunt");out.joke=gdWhyNot(items,V,Object.assign({},c,{moment:null}),gdNow()+600000,"joke");c.tour.paused=true;out.paused=gdWhyNot(items,V,Object.assign({},c,{moment:null}),gdNow()+600000,"hunt");out.never=gdNever(S.guidance.items.water).join("; ");out');
  // round 12: the water break became a beat of Fun › On the tour (made for the tour: never held), so the hold is checked on the treasure hunt
  ok('[fun hold] Every Fun item is held while a tour plays (the treasure hunt, a joke): Why not now? says "held while a tour plays"; paused, not that reason', /held while a tour plays/.test(fun.water) && /held while a tour plays/.test(fun.joke) && !/held while a tour plays/.test(fun.paused), fun);
  ok('...the breather (round 12, a beat of the tour) is never held, so no "cannot show" warning', !/Fun is held while a tour plays/.test(fun.never), fun.never);
  await R(base + 'S.guidance.on=true;gdGroupById("ontour").on=false;S.guidance.items.water.talkFor=1;S.guideCfg.look="ink";if(G.on)stopTour();S.view="card";S.stop="full";renderAll(false);G.gd.manual=false;startTour()');
  let sawMoment = false; const t9 = Date.now(); while (Date.now() - t9 < 14000) { if (await R('!!G.moment||!!ph.querySelector(".azmoment")')) { sawMoment = true; break; } if (await R('G.i>=2')) break; await W(150); }
  ok('[fun hold] On the page: the tour moves on (its steps end) and no water break shows', !sawMoment && await R('G.i>=1'), [await R('G.i'), sawMoment]);
  await R('stopTour();S.guidance.on=false');
  // a Moment shown (as the Editor's Try does): its prop beside Azu, 0 px over any words; no room: it waits
  await tourAt('ink', 'g4', 'mini'); await R('pauseTour();azMoment("water",function(){})'); await until('!!ph.querySelector(".azmoment")', 2000);
  const pw = await R('var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,a=ph.querySelector(".azmoment .azprop").getBoundingClientRect(),m=ph.querySelector(".mascot").getBoundingClientRect(),boxes=wordBoxes(null).map(function(b){return{l:b.l*q+pr.left,t:b.t*q+pr.top,r:b.r*q+pr.left,b:b.b*q+pr.top}});[".gdock",".gbubble",".azbub"].forEach(function(s){var e=ph.querySelector(s);if(e){var r=e.getBoundingClientRect();boxes.push({l:r.left,t:r.top,r:r.right,b:r.bottom})}});' +
    'var ov=0;boxes.forEach(function(b){ov+=Math.max(0,Math.min(b.r,a.right)-Math.max(b.l,a.left))*Math.max(0,Math.min(b.b,a.bottom)-Math.max(b.t,a.top))});var gap=Math.max(a.left-m.right,m.left-a.right,a.top-m.bottom,m.top-a.bottom);[Math.round(ov),Math.round(gap),a.left>=pr.left&&a.right<=pr.right]');
  ok('A Moment\'s prop sits beside Azu (within one prop of it), inside the phone, 0 px over any words (the card\'s, the panel\'s, the bubbles\')', pw[0] === 0 && pw[1] <= 120 && pw[2], pw);
  await R('azMomentEnd(false);var f=document.createElement("div");f.className="gtoast";f.id="t9block";f.style.cssText="position:absolute;left:0;top:0;width:100%;height:100%;pointer-events:none;opacity:0";ph.appendChild(f);azMoment("water",function(){})'); await W(900);
  const wait1 = await R('[G.moment,!!ph.querySelector(".azmoment")]');
  await R('document.getElementById("t9block").remove()'); const shownP = await until('!!ph.querySelector(".azmoment")', 1500);
  ok('...with no room it waits (no prop, the moment held), and shows once there is room', wait1[0] === 'water' && !wait1[1] && shownP, [wait1, shownP]);
  await R('azMomentEnd(false);stopTour();gdGroupById("ontour").on=true');
  tsec('fun');

  // ---------- 10. the music bed ----------
  await R(base + 'S.guideCfg.look="ink";S.guideCfg.music=true;S.guideCfg.musicTrack="drone";S.guideCfg.musicVol=40;S.guideCfg.musicDuck=25;if(G.on)stopTour();S.view="card";S.stop="full";renderAll(false);startTour()');
  await until('G.music&&G.music.ducked', 4000); await R('pauseTour();G.music.ducked=true;musicSet(true)'); await until('Math.abs(musicLevel()-musicTarget())<0.003', 5000); // held ducked: the line's end would bring it back up; the gain ramps on the audio clock, so wait for it, not a fixed time
  const m1 = await R('[G.music.id,G.music.kind,+musicLevel().toFixed(4),+musicTarget().toFixed(4)]');
  await R('musicDuck(false)'); await until('Math.abs(musicLevel()-musicTarget())<0.005', 6000); const m2 = await R('[+musicLevel().toFixed(4)]');
  ok('Music volume 40 % and Under the voice 25 %, read from the gain: ducked while Azu speaks, back up between lines', m1[0] === 'drone' && Math.abs(m1[2] - 0.4 / 3 * 0.25) < 0.004 && Math.abs(m2[0] - 0.4 / 3) < 0.006, [m1, m2]);
  await R('stopTour();S.guideCfg.musicTrack="exploration";S.guideCfg.musicVol=15;S.guideCfg.musicDuck=30;startTour()'); await until('G.music', 3000);
  const m3 = await R('[G.music.id,G.music.kind,G.music.audio&&G.music.audio.loop,G.music.audio&&/music\\/exploration\\.mp3$/.test(G.music.audio.src),musicTracks().map(function(t){return t.id}).join(),musicNowText()]');
  ok('A manifest track (Exploration, the default) plays from music/tracks.js, looping; Playing now names it and its licence', m3[0] === 'exploration' && m3[1] === 'file' && m3[2] && m3[3] && /^exploration,/.test(m3[4]) && /CC0/.test(m3[5]), m3);
  await R('stopTour();S.guideCfg.music=false;S.vis.music="harvest-season";S.vis.musicVol=60;startTour()'); await until('G.music', 3000);
  ok('The visitor\'s own music (Settings: Calm, 60 %) plays even with the Music bed Off', await R('G.music&&G.music.id==="harvest-season"&&Math.abs(musicVol()-.6)<1e-6'));
  await R('stopTour();S.vis.music=null;S.vis.musicVol=null');
  // Settings › The guide › Music, and the credits
  await R(base + 'S.guideCfg.musicVis=true;S.view="card";S.stop="full";S.settingsOpen=true;renderPhone(true)'); await W(300);
  const s1 = await R('var v=ph.querySelector(".vmusic");[!!v,[].map.call(ph.querySelectorAll("[data-gmus]"),function(b){return b.textContent}).join(","),!!ph.querySelector(".vmusic [data-gmusvol]"),!!ph.querySelector(".vmusic .vcredit"),(ph.querySelector(".vcredits")||{}).textContent||""]');
  ok('Settings › The guide › Music: Off and each track\'s feeling, a volume; Music credits lists every track that asks for one', s1[0] && /^Off,Exploration,Storyteller,Calm,Cinematic/.test(s1[1]) && s1[2] && !s1[3] && /Cabezón/.test(s1[4]) && !/RandomMind/.test(s1[4]), s1);
  await R('var v=ph.querySelector(".vset"),g=v.querySelector(".vmusic");v.scrollTop=g.offsetTop-60'); await W(200);
  await tap('[data-gmus="cabezon-duuiensela"]'); await W(200); const c1 = await R('[S.vis.music,(ph.querySelector(".vmusic .vcredit")||{}).textContent||""]');
  await tap('[data-gmus="exploration"]'); await W(200); const c2 = await R('[S.vis.music,!!ph.querySelector(".vmusic .vcredit")]');
  ok('The credited track chosen (a real tap): its credit line under the choice; a CC0 track: none', c1[0] === 'cabezon-duuiensela' && /Joan Benson/.test(c1[1]) && c2[0] === 'exploration' && !c2[1], [c1, c2]);
  await R('S.guideCfg.musicVis=false;renderPhone(true)'); ok('Visitor can choose the music Hidden: no Music in Settings', await R('!ph.querySelector(".vmusic")'));
  await R('S.guideCfg.musicVis=true;S.vis.music=null;S.settingsOpen=false;renderPhone(true)');
  await R('S.tab="guide";S.guideCfg.musicTrack="cabezon-duuiensela";renderEditor()'); const e1 = await R('[].map.call(ed.querySelectorAll(\'[data-fo="s7voice"] .row\'),function(r){return r.querySelector("label").textContent}).join("|")+"#"+((ed.querySelector(".mcredit")||{}).textContent||"")');
  await R('S.guideCfg.musicTrack="exploration";renderEditor()'); const e2 = await R('!!ed.querySelector(".mcredit")');
  ok('The Editor: Music volume, Under the voice, Track (the manifest\'s, the Soft drone, Add your own…), its Credit for a credited track only, Playing now, Visitor can choose the music', /Music volume\|Under the voice\|Track\|Credit\|Playing now\|Visitor can choose the music/.test(e1) && /Joan Benson/.test(e1) && !e2 && await R('var o=[].map.call(ed.querySelectorAll(\'[data-sel="@guideCfg.musicTrack"] option\'),function(x){return x.value});o[0]==="exploration"&&o.slice(-2).join()==="drone,own"&&o.indexOf("cabezon-duuiensela")>0'), e1);
  tsec('music');

  // ---------- 11. the green ▸ and every new row's demo ----------
  await R('S.tab="guide";renderEditor();ed.querySelectorAll(\'[data-fo="gt1"],[data-fo="s7dock"],[data-fo="s7voice"]\').forEach(function(f){f.open=true})'); await W(200);
  const pb = await R(CR + 'var l=ed.querySelector(\'[data-fo="s7dock"] .row > label.rd\'),a=getComputedStyle(l,"::after"),bg=px(getComputedStyle(ed).backgroundColor);[parseFloat(a.width),parseFloat(a.height),a.borderRadius,a.backgroundColor,+cr(px(a.backgroundColor),bg).toFixed(2),/svg/.test(a.backgroundImage)]');
  ok('The row demo\'s ▸ is a green round play button, 22 px, 3:1 or more on the Editor\'s dark', pb[0] >= 22 && pb[1] >= 22 && pb[2] === '50%' && /rgb\(47, 179, 107\)/.test(pb[3]) && pb[4] >= 3 && pb[5], pb);
  const keys = ['guideCfg.look', 'guideCfg.pWords', 'guideCfg.pBars', 'guideCfg.pSpeed', 'guideCfg.speed', 'guideCfg.musicVol', 'guideCfg.musicDuck', 'guideCfg.musicTrack', 'guideCfg.musicVis', 'guideCfg.music'];
  const shown = [];
  for (const k of keys) {
    await R(base + 'if(G.on)stopTour();document.body.dataset.rowdemo="";S.tab="guide";renderEditor();ed.querySelectorAll(\'[data-fo="gt1"],[data-fo="s7dock"],[data-fo="s7voice"]\').forEach(function(f){f.open=true})');
    // round 21 (21.10): Speed moved to 1 · The mascot › Voice (round 19): the foldouts above each row opened by real clicks, as the developer does
    const rsel = `.row:has([data-set="@${k}"],[data-sel="@${k}"],[data-num="@${k}"])`; await openAbove(rsel + ' > label');
    const lb = p.locator(rsel + ' > label.rd').first();
    if (await lb.count()) await lb.scrollIntoViewIfNeeded(); const bb = await lb.boundingBox(); if (!bb) { shown.push(k + ' (no ▸)'); continue; }
    await p.mouse.click(bb.x + bb.width - 12, bb.y + bb.height / 2); // on the green button itself
    await until('/found/.test(document.body.dataset.rowdemo||"")', 3500); shown.push(await R('document.body.dataset.rowdemo'));
  }
  ok('Every new row\'s ▸ (a real click on the green button) shows its demo, the part ringed (Look, Words, Voice moving, Speed button, Speed, the music rows, Visitor can choose)', shown.length === keys.length && shown.every((s, i) => s && s.indexOf(keys[i] + '|found') === 0), shown);
  await R('if(G.on)stopTour();S.guideCfg.music=false;S.guideCfg.musicTrack="exploration";S.guideCfg.musicVol=10;musicDemo()'); await W(400);
  const md = await R('[!!G.music,G.musicDemo,+musicTarget().toFixed(4)]'); await R('musicOn(false);G.musicDemo=false;S.guideCfg.musicVol=15');
  ok('A music row\'s demo plays the music at that setting for a few seconds, even with the Music bed Off', md[0] && md[1] && Math.abs(md[2] - 0.1) < 1e-3, md);
  await R(base + 'if(G.on)stopTour();S.guideCfg.look="paper";S.ui.rowDemo=true;S.tab="guide";renderEditor();ed.querySelector(\'[data-fo="s7dock"]\').open=true'); await W(200);
  await edClick('[data-set="@guideCfg.look"][data-v="player"]'); await until('/guideCfg\\.look\\|found/.test(document.body.dataset.rowdemo||"")', 3500);
  ok('A change to Look (a real click) shows its demo: the tour at Open in that Look', await R('S.guideCfg.look==="player"&&!!ph.querySelector(".gdock.lk-player.h-peek")'));
  await R('if(G.on)stopTour();S.guideCfg.look="ink"');
  ok('The words and their owners (the panel\'s, the music\'s)', await R('["dock_speed","dock_readall","dock_seek","dock_open","dock_small"].every(function(k){return TXOWN[k]==="s7dock"})&&["settings_music","settings_music_vol","settings_music_credits"].every(function(k){return TXOWN[k]==="s5settings"})&&L(S.tx.dock_readall)==="Read all"'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  tsec('end');
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
