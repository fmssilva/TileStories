// v8s checks (Playwright + the installed Edge): node test8s.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;sampleFn=null;S.guideCfg.azuStyle="tile";S.camCfg.wall="belem";S.camCfg.every=4;S.view="card";S.stop="closed";S.camX=.35;S.camY=.45;S.camZoom=1.3;renderAll(false)');
  await p.waitForTimeout(400);
  // 1. the test wall: its points are the markers, on the picture
  const w1 = await R('[S.wallPois.length,ph.querySelectorAll(".cpin").length,!ph.querySelector(".campins").hidden,getComputedStyle(document.getElementById("camera")).backgroundImage.length>1000]');
  ok('The Belém test wall: 11 points, those in view shown as pins on the picture', w1[0] === 11 && w1[1] >= 2 && w1[2] && w1[3], w1);
  const pinAt = await R('var e=ph.querySelector(".cpin.central"),p=S.wallPois.filter(function(x){return x.id===e.dataset.cpin})[0],s=poiScreen(p);[Math.round(parseFloat(e.style.left)-s.x),Math.round(parseFloat(e.style.top)-s.y)]');
  ok('...each pin sits where its point is on the picture', Math.abs(pinAt[0]) <= 1 && Math.abs(pinAt[1]) <= 1, pinAt);
  // 2. drag pans both ways; the wheel zooms (nearer the wall)
  const box = await p.locator('#ph').boundingBox(); const c0 = await R('[S.camX,S.camY,camView().central&&camView().central.id]');
  await p.mouse.move(box.x + 250, box.y + 500); await p.mouse.down(); await p.mouse.move(box.x + 120, box.y + 380, { steps: 8 }); await p.mouse.up(); await p.waitForTimeout(200);
  const c1 = await R('[S.camX,S.camY,camView().central&&camView().central.id]');
  ok('A drag pans the view both ways (the phone moved)', c1[0] > c0[0] && c1[1] > c0[1], [c0, c1]);
  await p.mouse.move(box.x + 180, box.y + 400); await p.mouse.wheel(0, -300); await p.waitForTimeout(250);
  const z = await R('[S.camZoom,ph.querySelectorAll(".cpin").length]');
  ok('The wheel zooms in (nearer the wall): fewer points in view', z[0] > 1.3, z);
  await R('S.camZoom=1.3;S.camX=.35;S.camY=.45;camPaint();camLayer()');
  // 3. Azu in the corner comments on the point that settles in the middle; the bubble is tappable
  await p.waitForFunction(() => LAB.run('!!ph.querySelector(".azfabbub")'), null, { timeout: 15000 }).catch(() => {});
  const cm = await R('var b=ph.querySelector(".azfabbub");[!!ph.querySelector(".azfab.onwall"),b&&b.textContent,camView().central.name]');
  ok('On the wall: Azu in the corner says what is in the middle, by itself', cm[0] && cm[1] && cm[1].indexOf(cm[2]) >= 0, cm);
  await p.locator('#ph .azfabbub').click(); await p.waitForTimeout(700);
  ok('...a tap on its remark opens Ask about the view, the point lit', await R('G.cam.open&&!!ph.querySelector(".camask.open")&&ph.querySelectorAll(".cpin.lit").length>=1&&ph.querySelector(".ca-ans").textContent.length>5'));
  await R('G.cam.open=false;G.cam.lit=[];renderPhone(true)'); await p.waitForTimeout(200);
  // round 7: the ✦ beside Azu is the visitor's way in; its menu has Ask about the view (a tap on Azu plays)
  await p.locator('#ph .gbtn').click(); await p.waitForTimeout(200); await p.locator('#ph [data-gbgo="view"]').click(); await p.waitForTimeout(200);
  ok('On the wall, the ✦\'s Ask about the view opens it (no Ask pill any more)', await R('G.cam.open&&!!ph.querySelector(".camask.open")&&!ph.querySelector(".azfab")'));
  await R('G.cam.open=false;renderPhone(true)');
  // 4. the card's wall: a remark on a point with a card opens it and starts its tour, at Full
  await R('S.camCfg.wall="ribeira";S.camX=.4;S.camY=.5;S.camZoom=1;G.fab.said=null;G.fab.last=0;renderAll(false)');
  await p.waitForFunction(() => LAB.run('!!ph.querySelector(".azfabbub")'), null, { timeout: 15000 }).catch(() => {});
  ok('On the card\'s wall, the remark on Ribeira Palace offers its tour', await R('ph.querySelector(".azfabbub").textContent.indexOf(TXT("fab_comment_card").slice(0,10))===0||/show you around/.test(ph.querySelector(".azfabbub").textContent)'));
  await p.locator('#ph .azfabbub').click(); await p.waitForTimeout(600);
  ok('...a tap: the card opens and the tour starts, at Full', await R('G.on&&S.stop==="full"'));
  await R('stopTour()');
  // 5. a tour from a card at Half raises it to Full; on the card Azu's button waits until the hero's Start is out of sight
  await R('S.stop="half";renderPhone(false)'); await p.waitForTimeout(900);
  ok('At Half, with the hero\'s Start the guide in sight: no Azu button', await R('!ph.querySelector(".azfab")'));
  await R('S.stop="full";renderPhone(true);scr.scrollTop=900'); await p.waitForTimeout(900);
  ok('...scrolled past the hero: Azu appears above the bar', await R('!!ph.querySelector(".azfab")&&!ph.querySelector(".azfab.onwall")'));
  await p.locator('#ph .gbtn').click(); await p.waitForTimeout(150);
  ok('...a tap on the ✦ beside it offers Start the tour, Ask and Guide settings', await R('ph.querySelectorAll(".gbmenu [data-gbgo]").length===3'));
  await p.locator('#ph [data-gbgo="tour"]').click(); await p.waitForTimeout(300);
  ok('...Start the tour: the tour runs, Azu becomes the guide (no Azu beside the ✦)', await R('G.on&&!ph.querySelector(".azfab")'));
  await R('stopTour()'); await p.waitForTimeout(700); await R('scr.scrollTop=900'); await p.waitForFunction(()=>!!document.querySelector("#ph .azfab"),null,{timeout:4000}).catch(()=>{});
  ok('...the tour stopped (and the hero out of sight): Azu is the button again', await R('!!ph.querySelector(".azfab")'));
  await R('S.stop="half";renderPhone(false);scr.scrollTop=0'); await p.waitForTimeout(700); await p.locator('#ph .gstart').click(); await p.waitForTimeout(900);
  ok('Play from a card at Half: the card rises to Full (on the screen, not only in its state)', await R('G.on&&S.stop==="full"&&scr.getBoundingClientRect().top-ph.getBoundingClientRect().top<40'), await R('Math.round(scr.getBoundingClientRect().top-ph.getBoundingClientRect().top)'));
  await R('stopTour()');
  // 6. the Editor
  ok('Editor: Ask about the view has Azu\'s comments, their pace and the wall; the button is in Guide button', await R('S.tab="guide";renderEditor();var f=ed.querySelector(\'[data-fo="s7cam"]\');!!f&&["Azu comments","Between comments","Wall (the Lab)"].every(function(t){return f.textContent.indexOf(t)>=0})&&!!rowEl(ed,"s7gbtn","Guide button")'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
