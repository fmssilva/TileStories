// test10c (Playwright + the installed Edge): node test10c.js -> PASS/FAIL per check. Merge round P1a.1: the Lab's local http server
// (tools/serve.py) and the assets out of the page (build.py: the page, its pictures in assets/, the 3D mascot's models and thumbnails in
// assets/mascot/).
// [http] the page comes over http://localhost:8642/lab_src/ and is under 5 MB; every assets/ file it names answers.
// [glb] the 3D mascot's model loads over http (fetched, parsed, drawn); [photo] Photo of the view reads its canvas back (a JPEG);
// [figure] guidance/present.js measures the mascot's picture (its thumbnail's pixels, not the whole box); [local] over the local
//   server the page is on this computer (labLocal: the keys, the private voice sets and the MP3s as from disk).
// [file] opened from disk (file://) the page runs (the card, the Editor); the 3D mascot (the default mascot) cannot load there, so the
//   Editor says so on one line: "Open with tools/serve.py for the 3D mascot and photos"; a photo asked for is not sent, the line stays.
// MUTATE=1: the notice never shows (fileOnlyNotice does nothing): the [file] check turns red.
const { chromium, labUrl } = require('./pw');
const path = require('path');
const url = labUrl('#probe');
const fileUrl = 'file:///' + path.join(__dirname, '..', 'block-components-lab.html').replace(/\\/g, '/') + '#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  let pageBytes = 0; p.on('response', async r => { if (/block-components-lab\.html$/.test(r.url().split('#')[0])) { try { pageBytes = (await r.body()).length } catch (e) {} } });
  const go = async (pg, u) => { await pg.goto(u); await pg.waitForFunction(() => window.LAB && window.LAB.run, null, { timeout: 30000 }); await pg.waitForTimeout(600) };
  await go(p, url);
  const R = src => p.evaluate(s => LAB.run(s), src), W = ms => p.waitForTimeout(ms), T0 = Date.now();
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };

  // ---------- [http] ----------
  const loc = await p.evaluate(() => location.href.split('#')[0]);
  ok('[http] the page comes from the local server and is under 5 MB', loc === 'http://localhost:8642/lab_src/block-components-lab.html' && pageBytes > 1e6 && pageBytes < 5e6, { loc, MB: +(pageBytes / 1e6).toFixed(2) });
  const named = await p.evaluate(async () => { const html = document.documentElement.outerHTML, list = [...new Set(html.match(/assets\/[\w./-]+\.(?:jpg|png|glb)/g) || [])];
    const st = await Promise.all(list.map(a => fetch(a, { method: 'HEAD' }).then(r => r.status).catch(() => 0))); return list.map((a, i) => [a, st[i]]) });
  ok(`[http] every assets/ file the page names answers (${named.length}: the pictures, the 5 models, their thumbnails)`, named.length >= 16 && named.every(x => x[1] === 200) && named.filter(x => /\.glb$/.test(x[0])).length === 5, named.filter(x => x[1] !== 200));
  ok('[local] over the local server the page is on this computer (labLocal)', await R('labLocal()'));

  // ---------- [glb] [photo] [figure] ----------
  await R('P.guideMute=true;P.introSeen=true;D.guidance.on=false;D.guide.azuStyle="glb";ui.view="card";ui.stop="half";renderAll(false);Azu3D.load();""');
  await p.waitForFunction(() => LAB.run('Azu3D.state().ready||Azu3D.state().failed'), null, { timeout: 30000 }).catch(() => {});
  const g = JSON.parse(await R('var s=Azu3D.state();JSON.stringify({ready:s.ready,failed:s.failed,err:s.err,parses:s.parses,model:s.model})'));
  ok('[glb] the 3D mascot\'s model loads over http (fetched from assets/mascot/, parsed, drawn)', g.ready && !g.failed && g.parses >= 1 && g.model === 'explorer', g);
  const photo = await p.evaluate(() => LAB.run('D.agent.photo=true;agPhoto()').then(x => x ? x.slice(0, 23) + ':' + x.length : null));
  ok('[photo] Photo of the view reads the camera\'s canvas back (a JPEG data URL)', !!photo && /^data:image\/jpeg;base64,/.test(photo) && +photo.split(':').pop() > 5000, photo);
  const fig = await p.evaluate(() => LAB.run('var d=document.createElement("div");d.style.cssText="position:fixed;left:0;top:0;width:120px;height:120px";d.innerHTML=mascotStill();document.body.appendChild(d);var i=d.querySelector("img");window.__fig=d;i.decode().then(function(){var f=gdFigure(d),k=i.currentSrc||i.src,m=GD_FIG[k];d.remove();return JSON.stringify({src:k.split("/").slice(-3).join("/"),m:m})})'));
  const fm = JSON.parse(fig).m;
  ok('[figure] the mascot\'s picture is measured from its pixels (guidance/present.js: the figure inside its box, not the whole box)', !!fm && (fm.l > 0 || fm.t > 0 || fm.r < 1 || fm.b < 1) && fm.r > fm.l && fm.b > fm.t, JSON.parse(fig));
  ok('[http] no line about opening with tools/serve.py over http', await R('!LAB.fileNote&&!document.querySelector("[data-filenote]")'));

  // ---------- [file] ----------
  const q = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const qerrs = []; q.on('pageerror', e => qerrs.push(e.message));
  await go(q, fileUrl);
  const Q = src => q.evaluate(s => LAB.run(s), src);
  await Q('P.guideMute=true;D.guidance.on=false;ui.view="card";ui.stop="half";renderAll(false);Azu3D.load();""');
  await q.waitForFunction(() => LAB.run('Azu3D.state().ready||Azu3D.state().failed'), null, { timeout: 30000 }).catch(() => {});
  await q.waitForTimeout(400);
  const f1 = JSON.parse(await Q('var n=document.querySelector("[data-filenote]");JSON.stringify([location.protocol,!!scr.querySelector(".hero"),ed.querySelectorAll(".row").length,labLocal(),D.guide.azuStyle,Azu3D.state().failed,n?n.textContent:null,ed.querySelectorAll("[data-filenote]").length])'));
  ok('[file] opened from disk the page runs (the card, the Editor); its mascot is the 3D model (the default), which cannot load there, so the Editor says so on one line: "Open with tools/serve.py for the 3D mascot and photos"',
    f1[0] === 'file:' && f1[1] && f1[2] > 50 && f1[3] && f1[4] === 'glb' && f1[5] === true && f1[6] === 'Open with tools/serve.py for the 3D mascot and photos' && f1[7] === 1, f1);
  // the line taken away, then a photo asked for: the line comes back with it (MUTATE=1: fileOnlyNotice does nothing from here)
  if (process.env.MUTATE) { await Q('fileOnlyNotice=function(){}'); console.log('MUTATION: the notice never shows'); }
  const f2 = JSON.parse(await q.evaluate(() => LAB.run('LAB.fileNote=false;ui.tab="guide";renderEditor();var gone=!ed.querySelector("[data-filenote]");D.agent.photo=true;agPhoto().then(function(x){return JSON.stringify([gone,x,ed.querySelectorAll("[data-filenote]").length])})')));
  ok('[file] a photo asked for from disk: none is sent (null), and the line says so (on the Guide tab too)', f2[0] === true && f2[1] === null && f2[2] === 1, f2);
  await q.close();

  ok('No page errors', errs.length === 0 && qerrs.length === 0, errs.concat(qerrs).slice(0, 5));
  console.log('TIME ' + Math.round((Date.now() - T0) / 1000) + ' s');
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); await b.close(); process.exit(fails ? 1 : 0);
})();
