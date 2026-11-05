// v9a checks (Playwright + the installed Edge), with no network for the voices: node test9a.js -> PASS/FAIL per check
// ElevenLabs is answered by a fake server (a real MP3 of the Azure set and even letter times); Azure by a fake Speech SDK
// (the same MP3, word boundaries and visemes). The live run with real keys is by hand (it spends credits): see the README.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/block-components-lab.html#probe';
const MP3 = fs.readFileSync(path.join(__dirname, '..', 'voicebench', 'out', 'azure-sonia-raquel', fs.readdirSync(path.join(__dirname, '..', 'voicebench', 'out', 'azure-sonia-raquel')).filter(f => f.endsWith('.mp3'))[0]));
const TEXT = 'O palácio tinha uma ópera, inaugurada em março de 1755.';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  let elReq = null, elStatus = 200;
  await p.route('**/api.elevenlabs.io/**', async r => { elReq = { url: r.request().url(), key: r.request().headers()['xi-api-key'], body: JSON.parse(r.request().postData() || '{}') };
    if (elStatus !== 200) return r.fulfill({ status: elStatus, body: '{"detail":"refused"}', headers: { 'access-control-allow-origin': '*' } });
    const n = TEXT.length, st = [], en = []; for (let i = 0; i < n; i++) { st.push(i * .06); en.push(i * .06 + .05); }
    r.fulfill({ status: 200, contentType: 'application/json', headers: { 'access-control-allow-origin': '*' }, body: JSON.stringify({ audio_base64: MP3.toString('base64'), alignment: { characters: TEXT.split(''), character_start_times_seconds: st, character_end_times_seconds: en } }) }); });
  // a fake Azure Speech SDK: word boundaries every 0.3 s, visemes 1 (A), 8 (O), 21 (M) in turn, the same MP3
  const FAKE_SDK = `window.SpeechSDK={ResultReason:{SynthesizingAudioCompleted:10},SpeechSynthesisOutputFormat:{Audio24Khz48KBitRateMonoMp3:1},
    SpeechConfig:{fromSubscription:function(k,r){window.__az={key:k,region:r};return{}}},
    SpeechSynthesizer:function(cfg){var me=this;this.close=function(){};
      var run=function(text,at,ok,curves){window.__az.voice=cfg.speechSynthesisVoiceName;window.__az.ssml=curves;var re=/\\S+/g,m,i=0;
        while((m=re.exec(text))){me.wordBoundary&&me.wordBoundary(me,{audioOffset:i*3e6,duration:2.5e6,textOffset:at+m.index,wordLength:m[0].length});i++}
        for(var v=0;v<30;v++){var e={audioOffset:v*1e6,visemeId:[1,8,21][v%3]};if(curves&&v<6){var fr=[];for(var n=0;n<30;n++){var b=new Array(55).fill(0);var ph=(v*30+n)%60;b[17]=ph<20?.6:ph<40?.3:.2;b[18]=ph>=40?.3:0;b[20]=ph>=20&&ph<40?.5:0;fr.push(b)}e.animation=JSON.stringify({FrameIndex:v*30,BlendShapes:fr})}me.visemeReceived&&me.visemeReceived(me,e)}
        var b=atob("${MP3.toString('base64')}"),u=new Uint8Array(b.length);for(var j=0;j<b.length;j++)u[j]=b.charCodeAt(j);setTimeout(function(){ok({reason:10,audioData:u.buffer})},50)};
      this.speakTextAsync=function(text,ok){run(text,0,ok,false)};
      this.speakSsmlAsync=function(ssml,ok){var t=ssml.replace(/^.*FacialExpression"\\/>/,"").replace(/<\\/voice><\\/speak>$/,"");run(t,ssml.indexOf(t),ok,/FacialExpression/.test(ssml))}}};`;
  await p.route('**/microsoft-cognitiveservices-speech-sdk@*/**', r => r.fulfill({ status: 200, contentType: 'application/javascript', body: FAKE_SDK }));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.guideMute=false;S.introSeen=true;S.lang="pt";S.guideCfg.azuStyle="tile";S.view="card";S.stop="half";renderAll(false);renderEditor();ed.querySelector(\'[data-fo="s7agent"]\').open=true');
  // 1. the Editor: the row and its status, with no keys here
  const opts = await R('Array.from(ed.querySelectorAll(\'[data-set="@agentCfg.liveVoice"]\')).map(function(b){return b.textContent+(b.getAttribute("aria-pressed")==="true"?"*":"")})');
  ok('Editor: Voice of the answers offers the device voice, Azure (live, chosen: the developer\'s default) and ElevenLabs (live)', opts.join('|') === 'Device voice|Azure (live)*|ElevenLabs (live)', opts);
  await p.locator('[data-set="@agentCfg.liveVoice"][data-v="eleven"]').click(); await W(300);
  ok('...ElevenLabs with no key here: the Editor says the device voice reads the answers', /ElevenLabs none.*Without its key the answers use the device voice/.test(await R('ed.querySelector(".lvstat").textContent')));
  ok('...and its model row appears (Flash v2.5 chosen)', await R('(ed.querySelector(\'[data-set="@agentCfg.elevenModel"][aria-pressed="true"]\')||{}).textContent') === 'Flash v2.5 (fastest)');
  // 2. ElevenLabs, live (faked): the request, the audio, the words, the mouth from the letters at their times
  await R('window.LABKEYS={eleven:"test-key",elevenPT:"voice-pt",elevenEN:"voice-en",azure:"az-key",azureRegion:"westeurope"};renderEditor();startTour();pauseTour()'); await W(600);
  const speak = async () => { await R('G.answer=' + JSON.stringify(TEXT) + ';window.__w=[];window.__end=0;say(G.answer,function(k){window.__w.push(k);G.hl.k=k},function(){window.__end=1})');
    const seen = new Set(); const t0 = Date.now(); let rec = false;
    while (Date.now() - t0 < 20000) { const s = await R('var m=ph.querySelector(".mascot .mouth");[m&&m.getAttribute("d"),window.__end,!!(G.recAudio&&G.utter&&G.utter.live)]'); if (s[0]) seen.add(s[0]); if (s[2]) rec = true; if (s[1]) break; await W(40); }
    return { shapes: seen.size, rec, end: await R('window.__end'), words: await R('window.__w.length'), last: await R('LV.last'), rest: await R('(ph.querySelector(".mascot .mouth")||{getAttribute:function(){return null}}).getAttribute("d")===MOUTH[restShape(azMood())]') }; };
  const e1 = await speak();
  ok('ElevenLabs: one request, with the key, the Portuguese voice, the answer\'s text and the model', elReq && elReq.key === 'test-key' && /text-to-speech\/voice-pt\/with-timestamps/.test(elReq.url) && elReq.body.text === TEXT && elReq.body.model_id === 'eleven_flash_v2_5', elReq && { url: elReq.url, model: elReq.body.model_id });
  ok('...the answer plays as a live line, the words light one by one, and it ends', e1.rec && e1.end === 1 && e1.words >= 8, e1);
  ok('...the mouth moves with the voice (several shapes) and rests at the end', e1.shapes >= 3 && e1.rest, [e1.shapes, e1.rest]);
  ok('...the Editor names what happened (ElevenLabs, its time, the shapes from the letters)', /Last answer: ElevenLabs, \d+ ms to the audio, \d+ mouth shapes from the letters at their times/.test(await R('lvStatusText()')), await R('lvStatusText()'));
  // 3. Azure, live (a fake SDK): the voice, the visemes as the mouth
  await p.locator('[data-set="@agentCfg.liveVoice"][data-v="azure"]').click(); await W(300);
  const a1 = await speak();
  ok('Azure: the SDK loads on demand, with the key, the region and Raquel for Portuguese, asking for the facial curves (SSML)', JSON.stringify(await R('window.__az')) === JSON.stringify({ key: 'az-key', region: 'westeurope', voice: 'pt-PT-RaquelNeural', ssml: true }), await R('window.__az'));
  ok('...the answer plays and ends; the mouth follows the facial curves (180 frames, several shapes)', a1.rec && a1.end === 1 && a1.shapes >= 3 && /Azure's facial curves/.test(a1.last && a1.last.from || '') && a1.last.shapes === 180, a1);
  // 4. the tour's own lines keep their voice (only the agent's answers go live)
  ok('A tour line (not an answer) does not call a live voice', await R('(function(){var n=0,f=lvAzure;lvAzure=function(t){n++;return f(t)};G.answer="";say("Bem-vindo ao Paço da Ribeira.",function(){},function(){});lvAzure=f;stopSpeech();return n})()') === 0);
  // 5. a refused key: the device voice reads the answer, and the Editor says so
  await p.locator('[data-set="@agentCfg.liveVoice"][data-v="eleven"]').click(); await W(300); elStatus = 401;
  await R('G.answer=' + JSON.stringify(TEXT) + ';window.__dev=0;var f=lvDevice;lvDevice=function(t,w,e){window.__dev++;stopSpeech()};say(G.answer,function(){},function(){})'); await W(1500);
  ok('ElevenLabs refuses the key: the device voice reads the answer instead', await R('window.__dev') === 1);
  ok('...and the Editor says it failed', /ElevenLabs failed \(ElevenLabs answered 401\); the device voice read it/.test(await R('lvStatusText()')), await R('lvStatusText()'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
