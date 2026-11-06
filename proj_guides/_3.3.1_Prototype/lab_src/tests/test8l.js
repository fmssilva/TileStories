// v8l checks (Playwright + the installed Edge): node test8l.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 20000 }); await p.waitForTimeout(400);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=true;S.lang="en";S.guideMute=true;S.view="story";S.stop="full";renderAll(false)');
  const r2 = () => R('scr.querySelector(\'[data-ref="r:r2"]\').textContent');
  // 1. Explorer reads the default; Family and Expert their own words; a field without its own words falls back
  const def = await r2();
  await R('S.vis.aud="family";renderAll(false)'); const fam = await r2();
  await R('S.vis.aud="expert";renderAll(false)'); const exp = await r2();
  ok('The same paragraph: the default (Explorer), Family\'s words, Expert\'s words', /cramped/.test(def) && /too small/.test(fam) && /trade arrived/.test(exp), [def.slice(0, 30), fam.slice(0, 30), exp.slice(0, 30)]);
  ok('A field with no Expert words falls back to the default', await R('scr.querySelector(\'[data-ref="r:r3"]\').textContent') === await R('S.vis.aud="explorer";renderAll(false);scr.querySelector(\'[data-ref="r:r3"]\').textContent'));
  await R('S.lang="pt";S.vis.aud="family";renderAll(false)');
  ok('...in Portuguese too', /pequeno demais/.test(await r2()));
  await R('S.ov["r:r2"].family.text.pt="";renderAll(false)');
  ok('A missing Portuguese override falls back to the default\'s Portuguese (never to the English override)', /Quando|D\\. Manuel|castelo/.test(await r2()) && !/too small/.test(await r2()), (await r2()).slice(0, 40));
  await R('S.ov["r:r2"].family.text.pt="O rei D. Manuel achava o castelo no monte pequeno demais.";S.lang="en";renderAll(false)');
  // 2. an AI draft not reviewed never shows; reviewed, it does
  await R('S.view="story";renderAll(false)');
  const r9 = () => R('scr.querySelector(\'[data-ref="r:r9"]\').textContent');
  ok('Family: an AI draft not yet reviewed is not shown (the default is)', !/huge earthquake shook/.test(await r9()));
  await R('S.ov["r:r9"].family.reviewed=true;renderAll(false)');
  ok('...once Reviewed, it is', /huge earthquake shook/.test(await r9()));
  await R('S.ov["r:r9"].family.reviewed=false');
  // 3. the looks per audience and a hidden block
  await R('S.view="card";renderAll(false)');
  const looks = await R('[ph.dataset.corners,ph.dataset.type,ph.dataset.orn,getComputedStyle(scr.querySelector(".ey"),"::before").content]');
  ok('Family: round corners, type a step up, tile stickers on the eyebrows', looks[0] === 'round' && looks[1] === 'up' && looks[2] === 'stickers' && looks[3] !== 'none', looks);
  await R('S.vis.aud="expert";renderAll(false)');
  ok('Expert: the quiz is hidden; tight corners; no ornaments', await R('getComputedStyle(scr.querySelector(\'[data-ref="b:b_quiz"]\')).display==="none"&&ph.dataset.corners==="tight"&&ph.dataset.orn==="off"'));
  // 4. the guide: length per audience, Azu's liveliness per audience
  await R('S.vis.aud="family";S.vis.len="";renderAll(false);startTour()');
  const fs = await R('[S.guideSteps.map(function(s){return s.id}).join(","),S.guideCfg.life,L(S.guideSteps[0].text).slice(0,20)]');
  ok('Family: Short (no g5, g8, no deep step), Azu Lively, the guide\'s own welcome', !/g5|g8|g7x/.test(fs[0]) && fs[1] === 'lively' && /Hi! I'm Azu/.test(fs[2]), fs);
  await R('stopTour();S.vis.aud="expert";renderAll(false);startTour()');
  const es = await R('[S.guideSteps.map(function(s){return s.id}).join(","),S.guideCfg.life]');
  ok('Expert: Deep (the extra step on what was lost), no quiz step, Azu Calm', /g7x/.test(es[0]) && !/g9/.test(es[0]) && es[1] === 'calm', es);
  await R('stopTour()');
  ok('After the tour the script is whole again (for the Editor)', await R('S.guideSteps.length') === 12);
  // 5. the visitor's Settings: Audience and Length
  await R('S.vis.aud="explorer";S.vis.len="";S.view="card";S.settingsOpen=true;renderAll(false)'); await p.waitForTimeout(300);
  ok('Settings: Who\'s exploring and Length, with their words', await R('var v=ph.querySelector(".vset");!!v&&v.textContent.indexOf(TXT("settings_aud"))>=0&&v.textContent.indexOf(TXT("settings_len"))>=0&&!!v.querySelector(\'[data-vis="aud|family"]\')'));
  await p.locator('#ph [data-vis="aud|family"]').first().click(); await p.waitForTimeout(300);
  ok('...a tap on Family changes the card', await R('S.vis.aud==="family"&&ph.dataset.corners==="round"'));
  await p.locator('#ph [data-vis="len|deep"]').first().click(); await p.waitForTimeout(300);
  ok('...and Length can differ from the audience\'s own', await R('lenNow()') === 'deep');
  await R('S.settingsOpen=false;S.vis.aud="explorer";S.vis.len="";renderAll(false)');
  // 6. the first card: one screen, three choices, Skip = Explorer
  await R('S.aud.seen=false;S.view="card";S.stop="half";renderPhone(false)'); await p.waitForTimeout(500);
  ok('The first card asks: one screen, three choices and Skip', await R('var o=ph.querySelector(".onb");!!o&&o.querySelectorAll("[data-onb]").length===4&&o.textContent.indexOf(TXT("aud_title"))>=0'));
  await p.locator('#ph [data-onb="expert"]').click(); await p.waitForTimeout(300);
  ok('...a tap on Expert: the card as Expert, and never asked again', await R('S.vis.aud==="expert"&&S.aud.seen&&!ph.querySelector(".onb")'));
  await R('renderPhone(false)'); ok('...(not after a redraw either)', await R('!ph.querySelector(".onb")'));
  await R('S.aud.seen=false;renderPhone(false)'); await p.waitForTimeout(400); await p.locator('#ph [data-onb="skip"]').click(); await p.waitForTimeout(200);
  ok('Skip: Explorer', await R('S.vis.aud==="explorer"&&S.aud.seen'));
  // the first card's words: 4.5:1 or more, light and dark
  for (const dark of [false, true]) {
    await R(`S.dark=${dark};S.aud.seen=false;renderAll(false)`); await p.waitForTimeout(500);
    const c = await R('var px=function(c){var m=c.match(/[\\d.]+/g).map(Number),k=c.indexOf("color(")===0?255:1;return{r:m[0]*k,g:m[1]*k,b:m[2]*k,a:m.length>3?m[3]:1}},lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},over=function(f,b){return{r:f.r*f.a+b.r*(1-f.a),g:f.g*f.a+b.g*(1-f.a),b:f.b*f.a+b.b*(1-f.a),a:1}},bg=function(el){var st=[];while(el){var c=px(getComputedStyle(el).backgroundColor);if(c.a>0)st.push(c);if(c.a>=1)break;el=el.parentElement}var b={r:255,g:255,b:255,a:1};for(var i=st.length-1;i>=0;i--)b=over(st[i],b);return b},min=99;ph.querySelectorAll(".onb b,.onb strong,.onb small,.onb-skip").forEach(function(e){var a=lum(px(getComputedStyle(e).color)),z=lum(bg(e)),r=(Math.max(a,z)+.05)/(Math.min(a,z)+.05);if(r<min)min=r});Math.round(min*100)/100');
    ok(`${dark ? 'dark' : 'light'}: the first card's words at 4.5:1 or more`, c >= 4.5, c);
  }
  await R('S.dark=false;S.aud.seen=true;renderAll(false)');
  // 7. the preview bar: Preview as
  await p.locator('#ptool [data-pt="aud"]').click(); await p.waitForTimeout(300);
  ok('Preview bar: one tap previews as the next audience', await R('S.vis.aud') === 'family');
  await R('S.vis.aud="explorer";renderAll(false)');
  // 8. the Editor: 6 · Audiences; Specific Marker > 4 · Audiences with its coverage, an override's editor, Draft with AI and its flags
  ok('Editor: 6 · Audiences after Card Controls (the Guide has its own tab), with Preview as, Who\'s exploring (its words), Audience in Settings, Looks per audience', await R('S.tab="card";renderEditor();var s=ed.querySelector(\'[data-fo="s8"]\');!!s&&!ed.querySelector(\'[data-fo="s7"]\')&&ed.querySelector(\'[data-fo="s5"]\').nextElementSibling===s&&!!s.querySelector(\'[data-fo="s8onb"]\')&&!!s.querySelector(\'[data-fo="s8look"]\')&&/Preview as/.test(s.textContent)&&(TXO.s8onb||[]).length===9&&(TXO.s8set||[]).length===5'));
  await R('S.tab="marker";renderEditor()');
  const cov = await R('var m=ed.querySelector(\'[data-fo="m4"]\');m?[m.querySelectorAll(".covc.over").length,m.querySelectorAll(".covc.draft").length,m.querySelectorAll(".covc.stale").length,m.querySelectorAll(".covc.hidden").length]:null');
  ok('Coverage: own words, an AI draft to review, a stale one, a hidden block', cov && cov[0] >= 4 && cov[1] === 1 && cov[2] === 1 && cov[3] === 1, cov);
  await p.locator('[data-ovc="r:r3|family"]').scrollIntoViewIfNeeded(); await p.locator('[data-ovc="r:r3|family"]').click(); await p.waitForTimeout(200);
  await p.locator('textarea[data-ovt="r:r3|family|en"]').fill('Kings kept building!'); await p.waitForTimeout(500);
  ok('Typing Family words for a field: its own words, on the phone as Family', await R('ovState("r:r3","family")==="over"') && await R('S.vis.aud="family";S.view="story";renderAll(false);scr.querySelector(\'[data-ref="r:r3"]\').textContent') === 'Kings kept building!');
  await R('S.vis.aud="explorer";S.tab="marker";renderAll(false)');
  await p.locator('[data-ovback="r:r3|family"]').click(); await p.waitForTimeout(200);
  ok('Back to the default', await R('ovState("r:r3","family")') === 'inherited');
  await R('sampleFn=function(){return Promise.resolve({text:\'{"en":"In 1580 Vasco built a tower.","pt":"Em 1580 o Vasco construiu uma torre."}\'})};S.sel.ov="r:r3|family";renderEditor()');
  await p.locator('[data-ovai="r:r3|family"]').click(); await p.waitForTimeout(400);
  const fl = await R('var f=ed.querySelector(".ovflag");[ovState("r:r3","family"),f?f.textContent:""]');
  ok('Draft with AI: a draft to review, its new names and numbers flagged', fl[0] === 'draft' && /1580/.test(fl[1]) && /Vasco/.test(fl[1]) && !/In|Em/.test(fl[1]), fl);
  await p.locator('[data-ovrev="r:r3|family"]').check(); await p.waitForTimeout(200);
  ok('...Reviewed: it can ship', await R('ovState("r:r3","family")') === 'over');
  await p.locator('[data-ovok="r:r4|expert"]').scrollIntoViewIfNeeded().catch(() => {}); await R('S.sel.ov="r:r4|expert";renderEditor()'); await p.locator('[data-ovok="r:r4|expert"]').click(); await p.waitForTimeout(200);
  ok('Stale: "Still right" marks it checked against the new default', await R('ovState("r:r4","expert")') === 'over');
  await p.locator('[data-ovh="b:b_quiz|expert"]').click(); await p.waitForTimeout(200);
  ok('A tap on Hidden shows the block again for that audience', await R('!S.aud.hide.expert["b:b_quiz"]'));
  await R('S.sel.gstep="g5";renderEditor()'); await p.locator('select[data-glen="g5"]').selectOption('deep');
  ok('Guide script: a step\'s Length', await R('byId(S.guideSteps,"g5").len') === 'deep');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
