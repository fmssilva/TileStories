// v9b checks (Playwright + the installed Edge, WebGL by SwiftShader), no network for the voices: node test9b.js -> PASS/FAIL per check
// The real mouth: a recorded Azure line moves the mouth by its facial curves on the audio's clock; a live ElevenLabs answer
// (faked: a real MP3) opens it by the sound's loudness; a recorded set without curves, opened from a file, falls back to the letters.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/block-components-lab.html#probe';
const OUT = path.join(__dirname, '..', 'voicebench', 'out');
const MP3 = fs.readFileSync(path.join(OUT, 'azure-sonia-raquel', fs.readdirSync(path.join(OUT, 'azure-sonia-raquel')).filter(f => f.endsWith('.mp3')).sort((a, b) => fs.statSync(path.join(OUT, 'azure-sonia-raquel', b)).size - fs.statSync(path.join(OUT, 'azure-sonia-raquel', a)).size)[0]));
const TEXT = 'O palácio tinha uma ópera, inaugurada em março de 1755.';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.route('**/api.elevenlabs.io/**', r => { const n = TEXT.length, st = [], en = []; for (let i = 0; i < n; i++) { st.push(i * .06); en.push(i * .06 + .05); }
    r.fulfill({ status: 200, contentType: 'application/json', headers: { 'access-control-allow-origin': '*' }, body: JSON.stringify({ audio_base64: MP3.toString('base64'), alignment: { characters: TEXT.split(''), character_start_times_seconds: st, character_end_times_seconds: en } }) }); });
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const setup = async (src, set) => R('stopSpeech();S.hints.on=false;S.previewBar=false;S.guideMute=false;S.introSeen=true;S.lang="pt";S.guideCfg.azuStyle="' + src + '";S.guideCfg.voice="recorded";S.guideCfg.voiceSet="' + set + '";S.view="card";S.stop="half";renderAll(false);if(!G.on)startTour();pauseTour()');
  // watch a spoken line: the mouth's kind, the tile of light's opening, the 2D drawn mouth, and how close the opening is to the curve at the audio's time
  const watch = async (start, ms) => { await R(start); const t0 = Date.now(), kinds = new Set(), talks = new Set(), d2 = new Set(); let near = 0, n = 0, ended = false;
    while (Date.now() - t0 < ms) { const s = await R('var m=ph.querySelector(".mascot .mouth"),a=G.recAudio,tr=MT.track,e=null;if(a&&tr){var i=Math.min(tr.f.length-1,Math.floor(a.currentTime*tr.fps));e=Math.min(1,tr.f[i][0]*1.1)}[mouthNow().kind,AzuGL.get("talk"),m&&m.getAttribute("d"),e,window.__end]');
      kinds.add(s[0]); talks.add(Math.round(s[1] * 100)); if (s[2]) d2.add(s[2]); if (s[3] !== null) { n++; if (Math.abs(s[3] - s[1]) < .2) near++; } if (s[4]) { ended = true; break; } await W(40); }
    return { kinds: [...kinds], talks: talks.size, shapes2d: d2.size, near, n, ended }; };
  await R('renderEditor();ed.querySelector(\'[data-fo="s7mascot"]\').open=true');
  ok('Editor: Mascot says how the mouth moves', /Azure's facial curves: 60 frames a second/.test(await R('(rowEl(ed,"s7mascot","How it moves")||{}).textContent||""')));
  // 1. a recorded Azure line: its curves, on the audio's clock, on the tile of light
  await setup('light', 'azure-sonia-raquel');
  ok('The recorded Azure set carries the mouth\'s curves for every line', await R('(function(){var s=voiceSets().filter(function(x){return x.id==="azure-sonia-raquel"})[0];var f=Object.keys(s.files).map(function(k){return s.files[k]});return f.length>50&&f.every(function(x){return x.mouth&&x.mouth.length>=90})})()'));
  const r1 = await watch('window.__end=0;G.answer="";say(guideLines("pt")[4].text,function(k){G.hl.k=k},function(){window.__end=1})', 20000);
  ok('A recorded Azure line: the mouth follows its curves (the tile of light opens to many heights)', r1.kinds.indexOf('curves') >= 0 && r1.talks >= 10 && r1.ended, r1);
  ok('...on the audio\'s own clock (the opening matches the curve at the audio\'s time, most frames)', r1.n > 10 && r1.near / r1.n > .8, [r1.near, r1.n]);
  ok('...and at the end it rests (the curves stop, the opening closes)', await R('[mouthNow().kind,AzuGL.get("talk")]').then(v => v[0] !== 'curves' && v[1] === 0));
  // 2. the same line on the 2D tile: its drawn mouths follow (several shapes)
  await setup('tile', 'azure-sonia-raquel');
  const r2 = await watch('window.__end=0;G.answer="";say(guideLines("pt")[4].text,function(k){G.hl.k=k},function(){window.__end=1})', 20000);
  ok('...on the 2D tile: the nearest drawn mouth, several shapes', r2.kinds.indexOf('curves') >= 0 && r2.shapes2d >= 4, r2);
  // 3. a live ElevenLabs answer (faked, a real MP3 as a blob): the loudness opens the mouth, the letters shape it
  await setup('light', 'azure-sonia-raquel');
  await R('window.LABKEYS={eleven:"k",elevenPT:"v",elevenEN:"v"};S.agentCfg.liveVoice="eleven"');
  const r3 = await watch('window.__end=0;G.answer=' + JSON.stringify(TEXT) + ';say(G.answer,function(k){G.hl.k=k},function(){window.__end=1})', 20000);
  ok('A live ElevenLabs answer: the sound\'s loudness opens the mouth (many heights)', r3.kinds.indexOf('loudness') >= 0 && r3.talks >= 6 && r3.ended, r3);
  // 4. a recorded set without curves, from a file: the page cannot read its sound, so the letters move the mouth again
  const eleven = await R('(voiceSets().filter(function(x){return /^eleven/.test(x.id)})[0]||{}).id');
  await setup('tile', eleven);
  const r4 = await watch('window.__end=0;G.answer="";say(guideLines("pt")[4].text,function(k){G.hl.k=k;markWord(k)},function(){window.__end=1})', 20000);
  ok('A recorded ElevenLabs line opened from a file: the loudness gives way to the letters, and the mouth still moves', r4.kinds.indexOf('letters') >= 0 && r4.shapes2d >= 3 && r4.ended, r4);
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
