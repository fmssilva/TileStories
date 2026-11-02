// v8j checks (Playwright + the installed Edge): node test8j.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;renderAll(false)');
  // helpers in the page: the spotlight's box, the picture's seen box, the view box's centre (as fractions of the picture)
  await R(`window.T8={rel:function(r){var p=ph.getBoundingClientRect();return[r.left-p.left,r.top-p.top,r.width,r.height].map(Math.round)},
    spot:function(){var s=ph.querySelector(".gspot.on"),d=G.fc&&G.fc.cue>=0?4:6;if(!s)return null;var r=T8.rel(s.getBoundingClientRect());return[r[0]+d,r[1]+d,r[2]-2*d,r[3]-2*d]},
    vb:function(){var i=T8.img(),m=(i.style.objectViewBox.match(/[\\d.]+/g)||[]).map(Number),nw=i.naturalWidth,nh=i.naturalHeight;return[m[3]/nw,m[0]/nh,(nw-m[1]-m[3])/nw,(nh-m[0]-m[2])/nh]},
    img:function(){return stepImg(stepEl(S.guideSteps[G.i]))},
    vbc:function(){var i=T8.img(),m=(i.style.objectViewBox.match(/[\\d.]+/g)||[]).map(Number);if(m.length<4)return null;var nw=i.naturalWidth,nh=i.naturalHeight;return[(m[3]+(nw-m[1]-m[3])/2)/nw,(m[0]+(nh-m[0]-m[2])/2)/nh].map(function(v){return Math.round(v*100)/100})}}`);
  const go = async (extra) => { await R(`if(G.on)stopTour();${extra || ''};S.guideCfg.speed=0.7;startTour();G.i=3;runStep()`); await p.waitForTimeout(900); };
  const reach = async (w) => { await p.waitForFunction(w => { const ix = LAB.run('cueIndex(S.guideSteps[3].cues[' + w + '])'); return LAB.run('G.hl.k') >= ix; }, w, { timeout: 15000 }); await p.waitForTimeout(1700); };
  // 1. before a spot: the spotlight rings the block, the rest dimmed
  await go();
  let s = await R('[T8.spot(),T8.rel(visRect(stepEl(S.guideSteps[G.i]))),getComputedStyle(ph.querySelector(".gspot")).boxShadow]');
  ok('Spotlight: on the block being told, the rest of the card dimmed', s[0] && s[0].every((v, i) => Math.abs(v - s[1][i]) <= 2) && /2400px/.test(s[2]) && /0\.4\)/.test(s[2]), s.slice(0, 2));
  // 2. at "palace", then "tower": the picture zooms to each spot, the spotlight rings the picture, Azu points
  await reach(0);
  s = await R('[T8.vbc(),T8.spot(),T8.rel(visRect(T8.img())),T8.img().classList.contains("gzimg")]');
  ok('"palace": the picture zooms to the palace (the view centred on it)', s[0] && Math.abs(s[0][0] - 0.6) < 0.12 && Math.abs(s[0][1] - 0.52) < 0.12 && s[3], s[0]);
  ok('...and the spotlight rings the picture', s[1] && s[1].every((v, i) => Math.abs(v - s[2][i]) <= 3), s.slice(1, 3));
  const arm = await R('var m=ph.querySelector(".mascot"),a=m.querySelector(".marm.on"),sp=T8.spot();if(!a)null;else{var ar=a.getBoundingClientRect(),mr=m.getBoundingClientRect(),pr=ph.getBoundingClientRect(),tx=pr.left+sp[0]+sp[2]/2,ty=pr.top+sp[1]+sp[3]/2,hx=ar.left,hy=ar.top+ar.height/2,mx=mr.left+mr.width/2,my=mr.top+mr.height/2;[Math.hypot(tx-hx,ty-hy),Math.hypot(tx-mx,ty-my)].map(Math.round)}');
  ok('Azu points: its hand is nearer the spot than Azu is', arm && arm[0] < arm[1], arm);
  await reach(1);
  s = await R('[T8.vbc(),T8.vb(),S.guideSteps[3].cues[1].region]');
  ok('"tower": the view moves left, to the whole corner tower', s[0][0] < 0.5 && s[1][0] <= s[2][0] + 0.01 && s[1][0] + s[1][2] >= s[2][0] + s[2][2] - 0.01 && s[1][1] <= s[2][1] + 0.01 && s[1][1] + s[1][3] >= s[2][1] + s[2][3] - 0.01, s);
  // 3. Light the spot: the picture stays; the spotlight closes on the spot inside it
  await go('S.guideCfg.zoom="spot"'); await reach(2);
  s = await R('[T8.vbc(),T8.spot(),T8.rel(visRect(T8.img()))]');
  ok('Light the spot: no zoom; the spotlight closes on the fountain, inside the picture', (!s[0] || (s[0][0] === 0.5 && s[0][1] === 0.5)) && s[1][0] >= s[2][0] && s[1][0] + s[1][2] <= s[2][0] + s[2][2] + 1 && s[1][2] * s[1][3] < 0.5 * s[2][2] * s[2][3], s);
  // 4. Soft ring and None
  await go('S.guideCfg.zoom="zoom";S.guideCfg.cue="ring"');
  ok('Soft ring: the ring without the dimming', await R('var g=ph.querySelector(".gspot.on");!!g&&!/2400px/.test(getComputedStyle(g).boxShadow)'));
  await go('S.guideCfg.cue="none"');
  ok('None: no spotlight (the step\'s own outline is back)', await R('!ph.querySelector(".gspot.on")&&!ph.classList.contains("gspotting")'));
  // 5. Reduce Motion: a crossfade, not a glide
  await go('S.guideCfg.cue="spotlight";S.reduce=true'); await reach(0);
  ok('Reduce Motion: the picture crossfades to the spot (no glide)', await R('var t=T8.img().style.transition;/opacity/.test(t)&&!/object-view-box/.test(t)'));
  await R('S.reduce=false');
  // 6. the visitor takes over (pause): the light and the arm step back
  await go(); await R('pauseTour()'); await p.waitForTimeout(200);
  ok('Paused: no spotlight, Azu lowers its arm', await R('!ph.querySelector(".gspot.on")&&!ph.querySelector(".marm.on")'));
  await R('resumeTour()');
  // 7. during a scroll, the light stays on its block, frame by frame
  await R('if(G.on)stopTour();startTour();G.i=4;runStep()'); await p.waitForTimeout(1500);
  await R('scr.scrollTop+=90'); await p.waitForTimeout(60);
  s = await R('[T8.spot(),T8.rel(visRect(stepEl(S.guideSteps[G.i])))]');
  ok('A scroll: the spotlight stays on its block (no lag; 4 px inside the scroller edge)', s[0] && s[0].every((v, i) => Math.abs(v - s[1][i]) <= 4), s);
  // 8. the tour stops: the picture is itself again
  await go(); await reach(0); await R('stopTour()'); await p.waitForTimeout(100);
  ok('Stop: no light, the picture back (no zoom, its scroll effect back)', await R('!ph.querySelector(".gspot")&&!scr.querySelector("img.gzimg")&&!scr.querySelector("img[data-gz]")'));
  // 9. the Editor: Focus after Captions; the Guide script's spots, drawn with the mouse
  ok('Editor: Focus after Captions, its four rows and a Try', await R('var c=ed.querySelector(\'[data-fo="s7cap"]\'),f=ed.querySelector(\'[data-fo="s7focus"]\');!!f&&c.nextElementSibling===f&&/Spotlight/.test(f.textContent)&&/Dim the rest/.test(f.textContent)&&/Slow zoom/.test(f.textContent)&&/Azu points/.test(f.textContent)&&!!f.querySelector("[data-fctry]")'));
  await R('S.tab="marker";S.sel.gstep="g3";renderEditor()'); await p.waitForTimeout(300);
  ok('Guide script: the step names three spots, each on its picture', await R('ed.querySelectorAll("[data-cuepic]").length===3&&ed.querySelectorAll("[data-cuepic] img").length===3'));
  const pic = p.locator('[data-cuepic="g3|0"]'); await pic.scrollIntoViewIfNeeded(); const bx = await pic.boundingBox();
  await p.mouse.move(bx.x + bx.width * 0.1, bx.y + bx.height * 0.1); await p.mouse.down(); await p.mouse.move(bx.x + bx.width * 0.3, bx.y + bx.height * 0.25, { steps: 6 }); await p.mouse.move(bx.x + bx.width * 0.5, bx.y + bx.height * 0.4, { steps: 6 }); await p.mouse.up();
  s = await R('S.guideSteps[3].cues[0].region');
  ok('Drawing on the picture sets the spot', Math.abs(s[0] - 0.1) < 0.03 && Math.abs(s[1] - 0.1) < 0.03 && Math.abs(s[2] - 0.4) < 0.03 && Math.abs(s[3] - 0.3) < 0.03, s);
  await p.locator('select[data-cueat="g3|0"]').selectOption('drawing');
  ok('...and its word is picked from what the guide says', await R('S.guideSteps[3].cues[0].at.en') === 'drawing');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
