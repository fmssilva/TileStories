// test10f (Playwright + the installed Edge): node test10f.js -> PASS/FAIL per check. Merge round P1a.2: the first publish
// (tools/publish.py). The publish list is staged in a temp folder (--stage) and the page opened from it at a host that is not this
// computer (http://publish.test/, every request answered from the folder), as the published artifact will be: no ../, no local
// server, no labLocal (the voice sets come packed).
// [list] the list: the page, assets/, the public voice sets (never out-private: a packed set by its packs only) and the music, in
//   batches within the limits (16 MB a file, 64 MB and 255 files a publish); [page] the page from the layout, every file it asks
//   for answered; [served] every file has an extension the artifact host serves (no .glb: the models go as their .glb.js
//   scripts); [glb] the chosen 3D model loads from its .glb.js (no .glb in the layout), and only that one; [voice] a recorded line
//   from its pack (a data URL) plays its audio; [music] a track of music/tracks.js loads.
// MUTATE=1: the packs and the models' scripts left out of the staged folder: the checks named [voice] and [glb] turn red.
const { chromium } = require('./pw');
const { execFileSync } = require('child_process'), fs = require('fs'), os = require('os'), path = require('path');
// the page from python build.py; the local server is not used here
const LAB = path.join(__dirname, '..'), TMP = fs.mkdtempSync(path.join(os.tmpdir(), 'tslab-publish-'));
(async () => {
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const T0 = Date.now();
  // ---------- [list] ----------
  const out = execFileSync(process.env.PYTHON || 'python', [path.join(LAB, 'tools', 'publish.py'), '--stage', TMP, '--json', path.join(TMP, '..', path.basename(TMP) + '.json')], { encoding: 'utf8' });
  const maps = JSON.parse(fs.readFileSync(path.join(TMP, '..', path.basename(TMP) + '.json'), 'utf8'));
  const all = [].concat(...maps.map(m => Object.keys(m))), sizes = maps.map(m => Object.values(m).reduce((a, f) => a + fs.statSync(f).size, 0));
  const big = [].concat(...maps.map(m => Object.entries(m))).filter(([, f]) => fs.statSync(f).size > 16e6);
  ok('[list] the page, assets/ (16 files), voicesets.js and packs.js, the music and its tracks.js', all.includes('tilestories-lab.html') && all.filter(f => f.startsWith('assets/')).length === 16 && all.includes('voicebench/out/voicesets.js') && all.includes('voicebench/out/packs.js') && all.includes('music/tracks.js') && all.filter(f => /^music\/.*\.(mp3|ogg)$/.test(f)).length >= 5, all.length);
  ok('[list] never out-private; a packed set by its packs only (Ryan · Duarte: its pack-N.js, no MP3)', !all.some(f => /out-private/.test(f)) && all.filter(f => /azure-ryan-duarte\/pack-\d+\.js$/.test(f)).length >= 1 && !all.some(f => /azure-ryan-duarte\/.*\.mp3$/.test(f)), all.filter(f => /ryan/.test(f)));
  ok('[list] within the limits: no file over 16 MB, each publish at most 64 MB and 255 files', !big.length && sizes.every(s => s <= 64e6) && maps.every(m => Object.keys(m).length <= 255), { publishes: maps.length, MB: sizes.map(s => +(s / 1e6).toFixed(1)), big });
  const SERVED = /\.(html|css|js|mjs|wasm|json|map|txt|md|csv|xml|png|jpe?g|gif|webp|svg|avif|ico|woff2?|ttf|otf|mp3|wav|ogg|mp4|webm|pdf)$/i;
  ok('[served] every published file has an extension the host serves; the five models as assets/mascot/<id>.glb.js, no .glb', all.every(f => SERVED.test(f)) && !all.some(f => /\.glb$/.test(f)) && all.filter(f => /^assets\/mascot\/[a-z]+\.glb\.js$/.test(f)).length === 5, all.filter(f => !SERVED.test(f) || /glb/.test(f)));
  console.log(out.trim().split('\n').slice(-2).join(' | '));
  if (process.env.MUTATE) { const d = path.join(TMP, 'voicebench', 'out', 'azure-ryan-duarte'); fs.readdirSync(d).filter(f => /^pack-/.test(f)).forEach(f => fs.unlinkSync(path.join(d, f))); const md = path.join(TMP, 'assets', 'mascot'); fs.readdirSync(md).filter(f => /\.glb\.js$/.test(f)).forEach(f => fs.unlinkSync(path.join(md, f))); console.log('MUTATION: the packs and the models\' scripts left out'); }

  // ---------- the page from the published layout ----------
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const ctx = await b.newContext({ viewport: { width: 1400, height: 900 } });
  const asked = [], missing = [];
  const TYPES = { '.html': 'text/html; charset=utf-8', '.js': 'text/javascript', '.json': 'application/json', '.jpg': 'image/jpeg', '.png': 'image/png', '.glb': 'model/gltf-binary', '.mp3': 'audio/mpeg', '.ogg': 'audio/ogg', '.webp': 'image/webp' };
  await ctx.route('http://publish.test/**', async r => {
    const u = decodeURIComponent(new URL(r.request().url()).pathname.slice(1)), f = path.join(TMP, ...u.split('/'));
    asked.push(u);
    if (fs.existsSync(f) && fs.statSync(f).isFile()) {
      const body = fs.readFileSync(f), range = r.request().headers()['range'];
      const ct = TYPES[path.extname(f).toLowerCase()] || 'application/octet-stream';
      if (range) { const m = /bytes=(\d+)-(\d*)/.exec(range), s = +m[1], e = m[2] ? +m[2] : body.length - 1;
        return r.fulfill({ status: 206, body: body.subarray(s, e + 1), headers: { 'content-type': ct, 'content-range': `bytes ${s}-${e}/${body.length}`, 'accept-ranges': 'bytes' } }) }
      return r.fulfill({ status: 200, body, headers: { 'content-type': ct, 'accept-ranges': 'bytes' } });
    }
    missing.push(u); return r.fulfill({ status: 404, body: 'not published' });
  });
  await ctx.route(/^https?:\/\/(?!publish\.test)/, r => /cdnjs|jsdelivr|unpkg/.test(r.request().url()) ? r.continue() : r.abort());
  const p = await ctx.newPage();
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto('http://publish.test/tilestories-lab.html#probe', { timeout: 90000 }); await p.waitForFunction(() => window.LAB && window.LAB.run, null, { timeout: 60000 }); await p.waitForTimeout(1500);
  const R = src => p.evaluate(s => LAB.run(s), src);
  const pg = JSON.parse(await R('JSON.stringify({local:labLocal(),hero:!!scr.querySelector(".hero")||!!document.getElementById("wall"),title:document.title,sets:voiceSets().map(function(s){return s.id}),tracks:musicTracks().length})'));
  ok('[page] the page from the published layout (not this computer: no labLocal), its title "TileStories Lab", the voice sets and the music listed', !pg.local && pg.title === 'TileStories Lab' && pg.sets.indexOf('azure-ryan-duarte') >= 0 && pg.tracks >= 5, pg);
  // [glb]
  await R('P.guideMute=true;P.introSeen=true;D.guidance.on=false;D.guide.azuStyle="glb";goScreen("card",{stop:"half"});renderAll(false);Azu3D.load();""');
  await p.waitForFunction(() => LAB.run('Azu3D.state().ready||Azu3D.state().failed'), null, { timeout: 30000 }).catch(() => {});
  const g = JSON.parse(await R('var s=Azu3D.state();JSON.stringify({ready:s.ready,failed:s.failed,err:s.err,model:s.model})'));
  const glbAsked = asked.filter(u => /^assets\/mascot\/[^/]+\.glb/.test(u));
  ok('[glb] the chosen 3D model (the explorer) loads from assets/mascot/explorer.glb.js in the layout (no .glb there): decoded, parsed, drawn; no other model asked for', g.ready && !g.failed && g.model === 'explorer' && JSON.stringify(glbAsked) === '["assets/mascot/explorer.glb.js"]' && !fs.readdirSync(path.join(TMP, 'assets', 'mascot')).some(f => /\.glb$/.test(f)), [g, glbAsked]);
  // [voice]: the packs load (packs.js names them), a recorded line's URL is its data URL, and the audio plays it
  await p.waitForFunction(() => LAB.run('Object.keys(window.VOICEPACK||{}).length>400'), null, { timeout: 30000 }).catch(() => {});
  const v = JSON.parse(await p.evaluate(() => LAB.run('var s=voiceSets().filter(function(x){return x.id==="azure-ryan-duarte"})[0],k=Object.keys(s.files)[0],f=s.files[k];P.lang=f.lang;var r=recFile(f.text,s);' +
    'new Promise(function(done){var a=new Audio(r.url),t=setTimeout(function(){done(JSON.stringify({packed:Object.keys(window.VOICEPACK||{}).length,data:/^data:audio\\/mpeg;base64,/.test(r.url),dur:0,err:"timeout"}))},8000);' +
    'a.onloadedmetadata=function(){clearTimeout(t);done(JSON.stringify({packed:Object.keys(window.VOICEPACK||{}).length,data:/^data:audio\\/mpeg;base64,/.test(r.url),dur:a.duration,text:f.text.slice(0,40)}))};a.onerror=function(){clearTimeout(t);done(JSON.stringify({packed:Object.keys(window.VOICEPACK||{}).length,data:/^data:audio\\/mpeg;base64,/.test(r.url),dur:0,err:"error"}))}})')));
  ok('[voice] the packs loaded (packs.js and its pack-N.js from the layout), a recorded line\'s audio is its data URL and loads (a duration)', v.packed > 400 && v.data && v.dur > 0.5, v);
  // [music]
  const m = JSON.parse(await p.evaluate(() => LAB.run('var t=musicTrackById("exploration");new Promise(function(done){var a=new Audio(t.url||t.file),k=setTimeout(function(){done(JSON.stringify({url:a.src,dur:0}))},10000);a.preload="metadata";a.onloadedmetadata=function(){clearTimeout(k);done(JSON.stringify({url:a.src,dur:a.duration}))};a.onerror=function(){clearTimeout(k);done(JSON.stringify({url:a.src,dur:0,err:1}))}})')));
  ok('[music] Exploration (music/tracks.js) loads from music/ in the layout', /^http:\/\/publish\.test\/music\/exploration\.mp3$/.test(m.url) && m.dur > 60, m);
  ok('[page] every file the page asked for is in the layout (or one of its two places: "voicebench/..." first, "../voicebench/..." the server\'s)', missing.every(u => /^\.\.\/|^(voicebench|music)\//.test(u) && !all.includes(u)) && !missing.some(u => /^assets\//.test(u)), missing.slice(0, 8));
  ok('No page errors', errs.length === 0, errs.slice(0, 5));
  await b.close();
  fs.rmSync(TMP, { recursive: true, force: true }); fs.rmSync(path.join(TMP, '..', path.basename(TMP) + '.json'), { force: true });
  console.log('TIME ' + Math.round((Date.now() - T0) / 1000) + ' s');
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); process.exit(fails ? 1 : 0);
})();
