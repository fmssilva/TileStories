// v8p checks (Playwright + the installed Edge): node test8p.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;sampleFn=null;S.camCfg.fab=false;S.camCfg.names="middle";S.view="card";S.stop="closed";S.camX=.4;renderAll(false)');
  // 1. the pill names the point nearest the middle
  const pill = await R('var b=ph.querySelector(".ca-pill");[!!b,b&&b.textContent,camView().central&&camView().central.name]');
  ok('On the wall: the Ask pill names the point nearest the middle', pill[0] && pill[1] === 'Ask about ' + pill[2], pill);
  await p.locator('#ph .ca-pill').click(); await p.waitForTimeout(200);
  const open = await R('[ph.classList.contains("camopen"),ph.querySelectorAll(".cpin").length,!!ph.querySelector(".cpin.central b"),getComputedStyle(ph.querySelector(".cpin.central b")).opacity,ph.querySelectorAll("[data-cchip]").length]');
  ok('A tap opens the panel: the points in view as pins, the middle one ringed with its name, five suggestions', open[0] && open[1] >= 2 && open[2] && open[3] === '1' && open[4] === 5, open);
  // 2. what the model is given: the points in view and where each sits (no photo)
  const ctx = await R('camContext()');
  ok('The context: each point in view with its place on the screen, and the middle one', ctx.in_view.length >= 2 && ctx.in_view.every(x => /^(top|middle|bottom)( (left|centre|right))?$/.test(x.place)) && ctx.middle === ctx.in_view[0].id, ctx);
  // 3. the suggestions, answered from the view (no model here)
  await p.locator('#ph [data-cchip="what"]').click(); await p.waitForTimeout(600);
  const w = await R('[ph.querySelector(".ca-ans").textContent,ph.querySelectorAll(".cpin.lit").length,ph.querySelectorAll(".cpin").length]');
  ok('What am I looking at?: the middle point, then each other with its place; all lit', w[0].indexOf('In the middle: ') === 0 && w[1] === w[2], w);
  await p.locator('#ph [data-cchip="route"]').click(); await p.waitForTimeout(600);
  const rt = await R('[ph.querySelector(".ca-ans").textContent,[].map.call(ph.querySelectorAll(".cpin i"),function(i){return i.textContent}).filter(Boolean)]');
  ok('A route: numbered pins (1, 2, 3…) left to right, and the list', /1\. /.test(rt[0]) && rt[1].length >= 2 && rt[1].indexOf('1') >= 0, rt);
  await p.locator('#ph [data-cchip="oldest"]').click(); await p.waitForTimeout(600);
  ok('The oldest here: a point with its year, lit, the rest dimmed', await R('/from about \\d{4}/.test(ph.querySelector(".ca-ans").textContent)&&ph.querySelectorAll(".cpin.lit").length===1&&ph.querySelectorAll(".cpin.dim").length>=1'));
  await p.locator('#ph [data-cchip="kids"]').click(); await p.waitForTimeout(600);
  const hunt = await R('G.cam.hunt');
  const other = await R(`[].filter.call(ph.querySelectorAll(".cpin"),function(x){return x.dataset.cpin!=="${hunt}"})[0].dataset.cpin`);
  await p.locator(`#ph [data-cpin="${other}"]`).click({ force: true }); await p.waitForTimeout(200);
  ok('Treasure hunt: a wrong point says look again', await R('ph.querySelector(".ca-ans").textContent') === await R('TXT("cam_not_it")'));
  await p.locator(`#ph [data-cpin="${hunt}"]`).click({ force: true }); await p.waitForTimeout(300);
  ok('...the right one: found, and it lights', await R(`ph.querySelector(".ca-ans").textContent===TXT("cam_found")&&ph.querySelector('[data-cpin="${hunt}"]').classList.contains("lit")`));
  // 4. moving the phone (a drag on the wall) changes what is in view and the middle point
  const before = await R('camView().central.id');
  const box = await p.locator('#ph').boundingBox(); await p.mouse.move(box.x + 40, box.y + 260); await p.mouse.down(); await p.mouse.move(box.x + 330, box.y + 260, { steps: 8 }); await p.mouse.up(); await p.waitForTimeout(200);
  const after = await R('[S.camX,camView().central&&camView().central.id,ph.querySelector(".ca-pill").textContent]');
  ok('A drag on the wall (moving the phone): the view and the middle point change, and the pill follows', after[0] < 0.5 && after[1] !== before && after[2].indexOf(after[1] ? (await R(`L(S.wallPois.filter(function(p){return p.id==="${after[1]}"})[0].name)`)) : '') > 0, [before, after]);
  // 5. with the model: its tools light the points and draw the route (a stand-in model)
  await R('S.camX=.4;camLayer()');
  await R('window.SAMPLE=[];sampleFn=function(prompt,opts){SAMPLE.push(prompt);var t=opts.tools;t[0].execute({ids:[camView().pois[0].id]});t[1].execute({ids:camView().pois.map(function(p){return p.id}).slice(0,2)});return Promise.resolve({text:"Here you go."})}');
  await p.locator('#ph #caq').fill('Where should I start?'); await p.locator('#ph .ca-form button').click(); await p.waitForTimeout(500);
  const ai = await R('[ph.querySelector(".ca-ans").textContent,ph.querySelectorAll(".cpin.lit").length,[].map.call(ph.querySelectorAll(".cpin i"),function(i){return i.textContent}).filter(Boolean).length,/VIEW: \\{"in_view"/.test(SAMPLE[0])&&!/base64|image/.test(SAMPLE[0])]');
  ok('With the model: it gets the view as text (no photo), lights points and draws the route with its tools', ai[0] === 'Here you go.' && ai[1] >= 1 && ai[2] === 2 && ai[3], ai);
  await R('sampleFn=null');
  // 6. close; a point with a card opens it
  await p.locator('#ph [data-camclose]').click(); await p.waitForTimeout(200);
  ok('Close: the panel and the pins go, the wall\'s markers are back', await R('!ph.classList.contains("camopen")&&!ph.querySelector(".camask.open")'));
  await R('S.camX=.4;renderAll(false);G.cam.open=true;camLayer()'); await p.locator('#ph [data-cpin="painting"]').click({ force: true }); await p.waitForTimeout(400);
  ok('A tap on a point with a card opens its card', await R('S.stop') === 'half');
  // 7. the words' contrast (the panel, the pins' names)
  await R('S.stop="closed";renderAll(false);G.cam.open=true;camSay("Test.")');
  const c = await R('var px=function(c){var m=c.match(/[\\d.]+/g).map(Number);return[m[0],m[1],m[2]]},lum=function(a){return a.map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(s,v,i){return s+v*[.2126,.7152,.0722][i]},0)},cr=function(f,b){var x=lum(px(f)),y=lum(px(b));return Math.round((Math.max(x,y)+.05)/(Math.min(x,y)+.05)*100)/100};[cr(getComputedStyle(ph.querySelector(".ca-ans")).color,getComputedStyle(ph.querySelector(".camask")).backgroundColor),cr("rgb(255,255,255)","rgb(16,18,22)")]');
  ok('The panel\'s words and the pins\' names at 4.5:1 or more', c.every(v => v >= 4.5), c);
  // 8. the Editor
  ok('Editor: Ask about the view after Ask the guide, its rows, the wall\'s points and its words', await R('var f=ed.querySelector(\'[data-fo="s7cam"]\');!!f&&ed.querySelector(\'[data-fo="s7ask"]\').nextElementSibling===f&&/Suggestions/.test(f.textContent)&&f.querySelectorAll("tbody tr").length===S.wallPois.length&&(TXO.s7cam||[]).length===26'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
