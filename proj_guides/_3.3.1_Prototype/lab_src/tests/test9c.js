// v9c checks (Playwright + the installed Edge), with no network for the voices: node test9c.js -> PASS/FAIL per check
// One order of sources for every line: a recorded line, else the live voice (its key here), else the device voice; Speaking now
// says which and why; pause, resume, stop and mute act on every source; an answer reaches the conversation once, whatever speaks it.
// ElevenLabs is answered by a fake server (a real MP3 of the Azure set, the request's letters at even times); Azure by a fake Speech SDK.
// MUTATE=1 node test9c.js runs it with the order broken (the device voice before the recorded one) to show which checks turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
const OUT = path.join(__dirname, '..', '..', 'voicebench', 'out', 'azure-sonia-raquel');
const MP3 = fs.readFileSync(path.join(OUT, fs.readdirSync(OUT).filter(f => f.endsWith('.mp3')).sort((a, b) => fs.statSync(path.join(OUT, b)).size - fs.statSync(path.join(OUT, a)).size)[0]));
const NEW = 'Uma frase nova, ainda sem áudio, que nenhum conjunto gravou.';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  const el = { n: 0, last: null, status: 200, delay: 0 };
  await p.route('**/api.elevenlabs.io/**', async r => { const body = JSON.parse(r.request().postData() || '{}'); el.n++; el.last = body.text;
    if (el.delay) await new Promise(res => setTimeout(res, el.delay));
    if (el.status !== 200) return r.fulfill({ status: el.status, body: '{"detail":"refused"}', headers: { 'access-control-allow-origin': '*' } });
    const t = body.text || '', st = [], en = []; for (let i = 0; i < t.length; i++) { st.push(i * .06); en.push(i * .06 + .05); }
    r.fulfill({ status: 200, contentType: 'application/json', headers: { 'access-control-allow-origin': '*' }, body: JSON.stringify({ audio_base64: MP3.toString('base64'), alignment: { characters: t.split(''), character_start_times_seconds: st, character_end_times_seconds: en } }) }); });
  // a fake Azure Speech SDK: a word boundary every 0.3 s, the same MP3
  const FAKE_SDK = `window.SpeechSDK={ResultReason:{SynthesizingAudioCompleted:10},SpeechSynthesisOutputFormat:{Audio24Khz48KBitRateMonoMp3:1},
    SpeechConfig:{fromSubscription:function(k,r){window.__az=(window.__az||0)+1;return{}}},
    SpeechSynthesizer:function(cfg){var me=this;this.close=function(){};
      this.speakSsmlAsync=function(ssml,ok){var t=ssml.replace(/^.*FacialExpression"\\/>/,"").replace(/<\\/voice><\\/speak>$/,""),at=ssml.indexOf(t),re=/\\S+/g,m,i=0;
        while((m=re.exec(t))){me.wordBoundary&&me.wordBoundary(me,{audioOffset:i*3e6,duration:2.5e6,textOffset:at+m.index,wordLength:m[0].length});i++}
        var b=atob("${MP3.toString('base64')}"),u=new Uint8Array(b.length);for(var j=0;j<b.length;j++)u[j]=b.charCodeAt(j);setTimeout(function(){ok({reason:10,audioData:u.buffer})},50)}}};`;
  await p.route('**/microsoft-cognitiveservices-speech-sdk@*/**', r => r.fulfill({ status: 200, contentType: 'application/javascript', body: FAKE_SDK }));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(1200);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(50); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('sayOrder=function(text){return!!text&&text===G.answer?["live"]:[]}'); console.log('MUTATION: the device voice before the recorded one (tour lines skip the recorded and live voices)'); }
  // 1. the default: Recorded, then live, then this device; Speaking now in Narration, idle
  const def = await R('[S.guideCfg.voice,(ed.querySelector(\'[data-set="@guideCfg.voice"][aria-pressed="true"]\')||{}).textContent,(ed.querySelector(".saynow")||{}).textContent]');
  ok('The default voice is the recorded one: Voice = Recorded, then live, then this device', def[0] === 'recorded' && def[1] === 'Recorded, then live, then this device', def);
  ok('...Narration has Speaking now (nothing at the moment)', def[2] === 'Nothing at the moment.', def[2]);
  await R('S.hints.on=false;S.previewBar=false;S.guideMute=false;S.introSeen=true;S.lang="pt";S.guideCfg.voiceSet="azure-sonia-raquel";S.guideCfg.panel="classic";S.view="card";S.stop="full";renderAll(false);renderEditor();' +
    'window.SPK=0;var sp0=speechSynthesis.speak.bind(speechSynthesis);speechSynthesis.speak=function(u){SPK++;sp0(u)};window.NOW=function(){return(ed.querySelector(".saynow")||{}).textContent}');
  const atStep = id => R(`stopSpeech();G.paused=false;G.i=S.guideSteps.findIndex(function(s){return s.id==="${id}"});runStep()`);
  // 2. a recorded line plays its file
  await R('startTour()'); await until('G.recAudio&&G.recAudio.currentTime>.3', 8000);
  const r1 = await R('var a=G.recAudio,r=recFile(stepText(S.guideSteps[0]));[!!a,a&&r&&a.src===r.url,SPK,NOW()]');
  ok('A recorded line plays its own file (no device voice)', r1[0] && r1[1] && r1[2] === 0, r1);
  ok('...Speaking now: "Recorded · Azure Sonia + Raquel"', r1[3] === 'Recorded · Azure Sonia + Raquel', r1[3]);
  // 3. a line the set lacks, no live key on this page: the device voice, and why
  await R('window.KEEP=byId(S.guideSteps,"g1").text.pt;byId(S.guideSteps,"g1").text.pt=' + JSON.stringify(NEW)); await atStep('g1'); await W(700);
  const d1 = await R('[!G.recAudio,!!G.utter||!!G.timer,NOW()]');
  ok('A line not recorded, no live key here: the device voice says it', d1[0] && d1[1], d1);
  ok('...Speaking now: "This device\'s voice (not recorded; no live key on this page)"', d1[2].indexOf("This device's voice (not recorded; no live key on this page)") === 0, d1[2]);
  // 4. the same line with a live key: the live voice (ElevenLabs, then Azure), with the read-along and the mouth of an answer
  await R('window.LABKEYS={eleven:"k",elevenPT:"voice-pt",elevenEN:"voice-en",azure:"az",azureRegion:"westeurope"};S.agentCfg.liveVoice="eleven";renderEditor()');
  const n0 = el.n; await atStep('g1'); await until('G.recAudio&&G.utter&&G.utter.live&&G.recAudio.currentTime>.6', 8000);
  const l1 = await R('[!!(G.utter&&G.utter.live),G.hl.k,mouthNow().kind,NOW(),SPK]');
  ok('A line not recorded, with a key: the live voice says it (ElevenLabs asked for that line)', el.n === n0 + 1 && el.last === NEW && l1[0], [el.n - n0, el.last, l1]);
  ok('...its words light up and the mouth follows the sound, as for an answer', l1[1] >= 1 && l1[2] === 'loudness', l1);
  ok('...Speaking now: "Live · ElevenLabs (this line is not recorded in Azure Sonia + Raquel)"', l1[3] === 'Live · ElevenLabs (this line is not recorded in Azure Sonia + Raquel)', l1[3]);
  // 5. pause and resume act on a live line; mute silences it while the words go on; stop silences it
  await p.locator('#ph [data-g="pause"]').first().click({ timeout: 4000 }).catch(() => {}); await W(400);
  const lp = await R('[G.paused,G.recAudio&&G.recAudio.paused,G.recAudio&&G.recAudio.currentTime]'); await W(500); const lp2 = await R('G.recAudio&&G.recAudio.currentTime');
  await p.locator('#ph [data-g="play"]').first().click({ timeout: 4000 }).catch(() => {}); await W(600); const lp3 = await R('[G.paused,G.recAudio&&!G.recAudio.paused,G.recAudio&&G.recAudio.currentTime]');
  ok('Pause holds a live line (the audio is paused, where it was); Resume plays it on', lp[0] && lp[1] === true && lp2 === lp[2] && !lp3[0] && lp3[1] && lp3[2] > lp[2], [lp, lp2, lp3]);
  await p.locator('#ph [data-g="mute"]').first().click({ timeout: 4000 }).catch(() => {}); await W(300); const mu = await R('[G.recAudio&&G.recAudio.muted,G.hl.k]'); await W(600); const mu2 = await R('[G.hl.k,!!G.recAudio]');
  ok('Mute: the live line is silent and its words go on', mu[0] === true && mu2[1] && mu2[0] > mu[1], [mu, mu2]);
  await p.locator('#ph [data-g="mute"]').first().click({ timeout: 4000 }).catch(() => {}); await W(100);
  await R('window.A0=G.recAudio'); await p.locator('#ph [data-g="stop"]').first().click({ timeout: 4000 }).catch(() => {}); await W(300);
  ok('Stop silences a live line', await R('!G.on&&!G.recAudio&&!!A0&&A0.paused&&!G.utter'));
  // a pause while the live audio is being made: it comes paused; Resume plays it; a stop meanwhile: it never plays
  el.delay = 900; await R('startTour();pauseTour()'); await atStep('g1'); await W(150); await R('pauseNow()'); await W(1300);
  const hp = await R('[G.paused,!!G.recAudio,G.recAudio&&G.recAudio.paused,G.recAudio&&G.recAudio.currentTime]');
  await R('resumeTour()'); await W(600); const hp2 = await R('G.recAudio&&!G.recAudio.paused&&G.recAudio.currentTime>0');
  ok('Paused while the live audio is being made: it arrives paused, and Resume plays it', hp[0] && hp[1] && hp[2] === true && hp[3] === 0 && hp2, [hp, hp2]);
  await atStep('g1'); await W(150); await p.locator('#ph [data-g="stop"]').first().click({ timeout: 4000 }).catch(() => {}); await W(1300);
  ok('...stopped while it is being made: it never plays', await R('!G.on&&!G.recAudio&&!G.utter'));
  el.delay = 0;
  // Azure live (the fake SDK)
  await R('S.agentCfg.liveVoice="azure";startTour();pauseTour();resumeTour()'); await atStep('g1'); await until('G.recAudio&&G.utter&&G.utter.live', 8000);
  ok('...with Azure chosen: "Live · Azure (this line is not recorded in Azure Sonia + Raquel)"', await R('NOW()') === 'Live · Azure (this line is not recorded in Azure Sonia + Raquel)', await R('NOW()'));
  // 6. the live voice refuses: the device voice, and why
  await R('S.agentCfg.liveVoice="eleven"'); el.status = 401; await atStep('g1'); await until('!!G.utter&&!G.utter.live||!!G.timer', 6000);
  const f1 = await R('[!G.recAudio,!!G.utter||!!G.timer,NOW()]');
  ok('The live voice refuses its key: the device voice says the line, and Speaking now says why', f1[0] && f1[1] && f1[2].indexOf("This device's voice (not recorded; ElevenLabs failed)") === 0, f1);
  el.status = 200;
  await R('stopTour();byId(S.guideSteps,"g1").text.pt=KEEP');
  // 7. a scroll-follow pause (the line's block scrolled away, none in view) pauses a recorded line where it is; Resume plays on; stop
  const follow = async () => { await R('window.STEPS=S.guideSteps;S.guideSteps=S.guideSteps.filter(function(s){return s.id==="g1"||s.id==="g10"});S.guideCfg.onScroll="follow";startTour()');
    await W(900); await R('window.scrollTo(0,0)'); const box = await p.locator('#ph').boundingBox(); await p.mouse.move(box.x + box.width / 2, box.y + 300);
    for (let i = 0; i < 12; i++) { await p.mouse.wheel(0, 500); await W(60); } await until('G.paused', 6000); };
  await follow();
  const sp = await R('[G.paused,!!G.recAudio,G.recAudio&&G.recAudio.paused,G.recAudio&&G.recAudio.currentTime,G.utter&&G.utter.live]'); await W(500); const sp2 = await R('G.recAudio&&G.recAudio.currentTime');
  await p.locator('#ph [data-g="play"]').first().click({ timeout: 4000 }).catch(() => {}); await W(600); const sp3 = await R('[G.paused,G.recAudio&&!G.recAudio.paused,G.recAudio&&G.recAudio.currentTime]');
  ok('A scroll-follow pause pauses a recorded line (its audio paused where it was); Resume plays it on', sp[0] && sp[1] && sp[2] === true && sp[3] > 0 && sp2 === sp[3] && !sp3[0] && sp3[1] && sp3[2] > sp[3], [sp, sp2, sp3]);
  await R('window.A0=G.recAudio'); await p.locator('#ph [data-g="stop"]').first().click({ timeout: 4000 }).catch(() => {}); await W(300);
  ok('Stop silences a recorded line', await R('!G.on&&!G.recAudio&&!!A0&&A0.paused&&!G.utter'));
  await R('S.guideSteps=STEPS;byId(S.guideSteps,"g1").text.pt=' + JSON.stringify(NEW)); await R('S.guideSteps=S.guideSteps.filter(function(s){return s.id==="g1"||s.id==="g10"});startTour()');
  await until('G.recAudio&&G.utter&&G.utter.live&&G.recAudio.currentTime>.3', 8000);
  await R('window.scrollTo(0,0)'); { const box = await p.locator('#ph').boundingBox(); await p.mouse.move(box.x + box.width / 2, box.y + 300); for (let i = 0; i < 12; i++) { await p.mouse.wheel(0, 500); await W(60); } await until('G.paused', 6000); }
  const sl = await R('[G.paused,!!(G.utter&&G.utter.live),G.recAudio&&G.recAudio.paused,G.recAudio&&G.recAudio.currentTime]'); await W(500); const sl2 = await R('G.recAudio&&G.recAudio.currentTime');
  await p.locator('#ph [data-g="play"]').first().click({ timeout: 4000 }).catch(() => {}); await W(600); const sl3 = await R('[G.paused,G.recAudio&&!G.recAudio.paused,G.recAudio&&G.recAudio.currentTime]');
  ok('A scroll-follow pause pauses a live line too; Resume plays it on', sl[0] && sl[1] && sl[2] === true && sl2 === sl[3] && !sl3[0] && sl3[1] && sl3[2] > sl[3], [sl, sl2, sl3]);
  await R('stopTour();S.guideSteps=STEPS;byId(S.guideSteps,"g1").text.pt=KEEP');
  // 8. Voice = Device voice: as before (a recorded line is read by the device voice; no live voice for the tour); stop silences it
  await R('window.LABKEYS={};S.guideCfg.voice="device";renderEditor();startTour()'); await W(900); const nd = el.n;
  const dv = await R('[!G.recAudio,!!G.utter||!!G.timer,NOW()]');
  ok('Voice = Device voice: the device voice reads a recorded line', dv[0] && dv[1], dv);
  ok('...Speaking now: "This device\'s voice (Voice is Device voice)"', dv[2].indexOf("This device's voice (Voice is Device voice)") === 0, dv[2]);
  await R('window.LABKEYS={eleven:"k",elevenPT:"voice-pt",elevenEN:"voice-en"}'); await atStep('g1'); await W(600);
  ok('...and with a live key it still reads the tour (no live voice asked)', el.n === nd && await R('!G.recAudio&&(!!G.utter||!!G.timer)'), el.n - nd);
  await p.locator('#ph [data-g="stop"]').first().click({ timeout: 4000 }).catch(() => {}); await W(200);
  ok('Stop silences the device voice', await R('!G.on&&!G.utter&&!G.timer&&!speechSynthesis.speaking'));
  await R('S.guideCfg.voice="recorded";renderEditor()');
  // 9. an answer reaches the conversation once, whatever speaks it; Speaking now and the trace under it name the voice
  const REC = await R('L(byId(S.guideSteps,"g8").text)');
  const ask = async (text, keys) => { await R('window.LABKEYS=' + JSON.stringify(keys) + ';S.guideCfg.panel="dock";AG.mock={text:' + JSON.stringify(text) + '};S.agentCfg.provider="mock";applyProvider();S.agentCfg.voiceOut=true;agCardAsk();ask("Uma pergunta?")');
    await W(1500); return R('[G.thread.filter(function(m){return m.text===' + JSON.stringify(text) + '}).length,G.thread.filter(function(m){return m.who==="me"}).length,NOW(),(ph.querySelector(".gd-thread .agvoice")||{}).textContent,G.sayNow&&G.sayNow.src]'); };
  const a1 = await ask('O palácio tinha uma ópera, inaugurada em março de 1755.', { eleven: 'k', elevenPT: 'voice-pt', elevenEN: 'voice-en' }); await R('stopTour()');
  ok('An answer spoken live: once in the conversation (and the question once)', a1[0] === 1 && a1[1] === 1 && a1[4] === 'live', a1);
  ok('...Speaking now and the trace under it: "Live · ElevenLabs (an answer: the live voice comes first)"', a1[2] === 'Live · ElevenLabs (an answer: the live voice comes first)' && a1[3] === 'Voice: ' + a1[2], a1);
  const a2 = await ask(REC, {}); await R('stopTour()');
  ok('An answer the set has, no key: recorded, once in the conversation', a2[0] === 1 && a2[1] === 1 && a2[4] === 'recorded' && a2[2] === 'Recorded · Azure Sonia + Raquel (no ElevenLabs key on this page)' && a2[3] === 'Voice: ' + a2[2], a2);
  const a3 = await ask('Uma resposta que ninguém gravou.', {}); await R('stopTour()');
  ok('An answer by the device voice: once in the conversation; "This device\'s voice (no live key on this page; not recorded)"', a3[0] === 1 && a3[1] === 1 && a3[4] === 'device' && a3[2].indexOf("This device's voice (no live key on this page; not recorded)") === 0, a3);
  await R('S.agentCfg.provider="offline";applyProvider();sampleFn=null;agCardAsk();ask("palácio")'); await W(1200);
  ok('The offline search\'s answer: once in the conversation too', await R('G.thread.filter(function(m){return m.who==="azu"&&m.text===G.answer}).length===1&&G.thread.filter(function(m){return m.who==="me"}).length===1'));
  await R('stopTour()');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
