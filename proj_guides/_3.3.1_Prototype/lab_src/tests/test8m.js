// v8m checks (Playwright + the installed Edge): node test8m.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 20000 }); await p.waitForTimeout(400);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.view="card";S.stop="full";S.lens="";renderAll(false)');
  const shown = () => R('[].filter.call(scr.querySelectorAll("[data-ref]"),function(e){return/^[rb]:/.test(e.dataset.ref)&&e.getClientRects().length>0}).map(function(e){return e.dataset.ref})');
  // 1. the chips: under the hero, Everything first, then the point's topics
  ok('The chips under the hero: See it as · Everything, Politics, Architecture, Court life', await R('var b=scr.querySelector(".lensbar");!!b&&b.previousElementSibling===scr.querySelector(".hero")&&[].map.call(b.querySelectorAll("[data-lens]"),function(x){return x.textContent}).join(",")==="Everything,Politics,Architecture,Court life"&&b.getAttribute("aria-label")===TXT("lens_label")'));
  const all = await shown();
  // 2. a chip: the other topics' blocks step aside, the core stays, the lens's own get a badge; the order stays
  await p.locator('#ph [data-lens="court"]').click(); await p.waitForTimeout(200);
  const court = await shown();
  ok('Court life: the Before/After (Architecture) steps aside; the fun fact (Court life) and the core stay', court.indexOf('b:b_ba') < 0 && court.indexOf('b:b_fun') >= 0 && court.indexOf('b:b_facts') >= 0 && court.indexOf('b:b_quiz') >= 0, court);
  ok('...the order of what stays is unchanged', JSON.stringify(court) === JSON.stringify(all.filter(r => court.indexOf(r) >= 0)));
  ok('...the fun fact carries the "Court life" badge', await R('var e=scr.querySelector(\'[data-ref="b:b_fun"] .lensbadge\');!!e&&e.textContent==="Court life"'));
  ok('...and the chip is pressed', await R('scr.querySelector(\'[data-lens="court"]\').getAttribute("aria-pressed")') === 'true');
  // 3. the first block in view stays where it was on the screen
  await p.locator('#ph [data-lens=""]').click(); await R('var q=scr.querySelector(\'[data-ref="b:b_quiz"]\');scr.style.overflowAnchor="none";scr.scrollTop+=q.getBoundingClientRect().top-scr.getBoundingClientRect().top-160'); await p.waitForTimeout(200);
  const before = await R('var top=scr.getBoundingClientRect().top,a=[].filter.call(scr.querySelectorAll("[data-ref]"),function(e){var r=e.getBoundingClientRect();return/^[rb]:/.test(e.dataset.ref)&&r.bottom>top+60&&!(S.tags[e.dataset.ref]||[]).length&&e.getClientRects().length})[0];[a.dataset.ref,Math.round(a.getBoundingClientRect().top)]');
  await R('var c=scr.querySelector(\'[data-lens="politics"]\');c.click()'); await p.waitForTimeout(200);
  const after = await R(`Math.round(scr.querySelector('[data-ref="${before[0]}"]').getBoundingClientRect().top)`);
  ok('A lens change with hidden blocks above: the first core block in view stays where it was (no jump)', before[0] !== 'b:b_guide' && Math.abs(after - before[1]) <= 1 && await R('scr.querySelector(\'[data-ref="b:b_ba"]\').classList.contains("lenshidden")'), [before, after]);
  // 4. the story page: the same chips; a heading goes with its rows
  await R('S.lens="architecture";S.view="story";renderAll(false);scr.scrollTop=0'); await p.waitForTimeout(300);
  const st = await shown();
  ok('Story, Architecture: the drawing, the additions and Then and now stay; What was inside (Court life) and the move (Politics) step aside', st.indexOf('r:r1') >= 0 && st.indexOf('r:r4') >= 0 && st.indexOf('r:r6') >= 0 && st.indexOf('r:r5') < 0 && st.indexOf('r:r2') < 0 && !!(await R('scr.querySelector(".lensbar")')), st);
  await R('window.KT=JSON.stringify(S.tags);["r:r9","r:r10","r:r11"].forEach(function(k){S.tags[k]=["court"]});S.lens="politics";renderAll(false)'); const st2 = await shown();
  ok('...a heading whose rows all step aside goes with them ("One morning in November")', st2.indexOf('r:r8') < 0 && st2.indexOf('r:r9') < 0 && st2.indexOf('r:r3') >= 0, st2);
  await R('S.tags=JSON.parse(KT);renderAll(false)');
  // 5. the guide plays the core steps and the lens's own
  await R('S.lens="politics";S.view="card";renderAll(false);startTour()');
  const ids = await R('S.guideSteps.map(function(s){return s.id}).join(",")');
  ok('The guide with Politics: no Court life step (g5) nor Architecture step (g6)', !/g5|g6/.test(ids) && /g4/.test(ids), ids);
  await R('stopTour();S.lens=""');
  // 6. below the minimum, no chips
  await R('S.lensCfg.min=4;var keep=JSON.stringify(S.tags);window.KEEPT=keep;S.tags={"b:b_fun":["court"],"b:b_ba":["architecture"],"r:r5":["court"]};S.view="card";renderAll(false)');
  ok('Fewer tagged blocks than "Shown from": no chips', await R('!scr.querySelector(".lensbar")'));
  await R('S.tags=JSON.parse(KEEPT);S.lensCfg.min=2;renderAll(false)');
  // 7. the chips' words: 4.5:1 or more, light and dark (pressed and not)
  for (const dark of [false, true]) {
    await R(`S.dark=${dark};S.lens="politics";renderAll(false)`);
    const c = await R('var px=function(c){var m=c.match(/[\\d.]+/g).map(Number),k=c.indexOf("color(")===0?255:1;return[m[0]*k,m[1]*k,m[2]*k]},lum=function(a){return a.map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(s,v,i){return s+v*[.2126,.7152,.0722][i]},0)},min=99;scr.querySelectorAll(".lensbar button,.lensbadge").forEach(function(e){var a=lum(px(getComputedStyle(e).color)),b=lum(px(getComputedStyle(e).backgroundColor)),r=(Math.max(a,b)+.05)/(Math.min(a,b)+.05);if(r<min)min=r});Math.round(min*100)/100');
    ok(`${dark ? 'dark' : 'light'}: the chips and badges at 4.5:1 or more`, c >= 4.5, c);
  }
  await R('S.dark=false;S.lens="";renderAll(false)');
  // 8. the Editor: Lenses (with its words); Specific Marker > 5 · Lenses: names, tags, a new topic
  ok('Editor: Lenses after Looks per audience, three rows, its words', await R('var l=ed.querySelector(\'[data-fo="s8lens"]\');!!l&&ed.querySelector(\'[data-fo="s8look"]\').nextElementSibling===l&&/On the story page too/.test(l.textContent)&&/Shown from/.test(l.textContent)&&(TXO.s8lens||[]).length===2'));
  await R('S.tab="marker";renderEditor()');
  ok('Specific Marker > 5 · Lenses: the topics, and a tag table with cards, story rows and guide steps', await R('var m=ed.querySelector(\'[data-fo="m5"]\');!!m&&m.querySelectorAll("[data-lensn]").length===6&&m.querySelectorAll(\'[data-tag^="b:"]\').length>0&&m.querySelectorAll(\'[data-tag^="r:"]\').length>0&&m.querySelectorAll(\'[data-tag^="g:"]\').length>0'));
  await p.locator('[data-tag="b:b_quiz|politics"]').scrollIntoViewIfNeeded(); await p.locator('[data-tag="b:b_quiz|politics"]').check(); await p.waitForTimeout(200);
  ok('Ticking a tag: the quiz now belongs to Politics', await R('(S.tags["b:b_quiz"]||[]).indexOf("politics")>=0'));
  await p.locator('[data-lensadd]').click(); await p.waitForTimeout(200);
  ok('+ Add a topic: a fourth chip on the card', await R('S.lenses.length===4&&scr.querySelectorAll(".lensbar [data-lens]").length===5'));
  await p.locator('input[data-lensn="3|en"]').fill('Military'); await p.waitForTimeout(500);
  ok('...its name, typed, on the chip', await R('[].some.call(scr.querySelectorAll(".lensbar [data-lens]"),function(b){return b.textContent==="Military"})'));
  await p.locator('[data-lensdel="3"]').click(); await p.waitForTimeout(200);
  ok('...Remove: gone', await R('S.lenses.length===3'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
