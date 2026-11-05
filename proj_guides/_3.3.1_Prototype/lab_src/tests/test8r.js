// v8r checks (Playwright + the installed Edge): node test8r.js -> PASS/FAIL per check
// Needs two voice sets rendered by voicebench (out/ or, on this computer, out-private/).
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const sets = await R('voiceSets().map(function(s){return s.id})');
  ok('The voice sets rendered by voicebench are found (two or more)', sets.length >= 2, sets);
  await R('S.hints.on=false;S.previewBar=false;S.lang="pt";S.guideMute=false;S.guideCfg.voice="recorded";S.guideCfg.voiceSet="";S.guideCfg.panel="classic";S.view="card";S.stop="full";S.introSeen=true;renderAll(false);window.SPK=0;var sp0=speechSynthesis.speak.bind(speechSynthesis);speechSynthesis.speak=function(u){SPK++;sp0(u)};startTour()');
  await p.waitForTimeout(2200);
  const a = await R('var a=G.recAudio,w=G.recWords,t=a?a.currentTime:0,k=-1;if(w)for(var j=0;j<w.length;j++){if(w[j][0]<=t+.05)k=j}[!!a,t,G.hl.k,k,w?w.length:0,G.words.length,SPK]');
  ok('Recorded: the line plays from its audio file, no device voice', a[0] && a[1] > 0.5 && a[6] === 0, a);
  ok('...the word lit is the word being said at the audio\'s time (within one word)', Math.abs(a[2] - a[3]) <= 1 && a[4] === a[5], a);
  ok('...the glide uses the word\'s real length', await R('var k=G.hl.k,w=G.recWords[k];Math.abs(wordMs(k)-(w[1]-w[0])*1000)<2'));
  await R('pauseTour()'); await p.waitForTimeout(400); const t1 = await R('G.recAudio.currentTime'); await p.waitForTimeout(600); const t2 = await R('G.recAudio.currentTime');
  await R('resumeTour()'); await p.waitForTimeout(700); const t3 = await R('G.recAudio?G.recAudio.currentTime:-1');
  ok('Pause holds the audio where it is; Resume plays on from there', t1 === t2 && t3 > t2 && !await R('G.paused'), [t1, t2, t3]);
  await p.locator('#ph [data-g="mute"]').click(); await p.waitForTimeout(400);
  const mu = await R('[G.recAudio&&G.recAudio.muted,G.hl.k]'); await p.waitForTimeout(700); const k2 = await R('G.hl.k');
  ok('Mute: the audio is silent and the words go on with its clock', mu[0] === true && k2 > mu[1], [mu, k2]);
  await p.locator('#ph [data-g="mute"]').click();
  // a line that is not rendered (an edited text) speaks with the device voice
  await R('stopTour();var st=byId(S.guideSteps,"g1");window.KEEP=st.text.pt;st.text.pt="Uma frase nova, ainda sem áudio.";startTour();G.i=S.guideSteps.findIndex(function(s){return s.id==="g1"});runStep()'); await p.waitForTimeout(600);
  ok('A line not rendered (its text changed): the device voice says it (never a stale recording)', await R('!G.recAudio&&(!!G.utter||!!G.timer)'));
  await R('stopTour();byId(S.guideSteps,"g1").text.pt=KEEP');
  // the Editor: the recorded voice and its coverage; the blind test
  await R('S.tab="card";renderEditor()');
  ok('Narration: Recorded voice lists the sets, with the coverage of this point\'s lines', await R('var v=ed.querySelector(\'[data-fo="s7voice"]\');!!v&&!!v.querySelector(\'select[data-sel="@guideCfg.voiceSet"]\')&&/\\d+ of \\d+ lines rendered/.test(v.innerHTML)'));
  await R('var o=ed.querySelector(\'select[data-vtline] option:nth-child(2)\');S.vt.line=o.value;renderEditor()');
  // the foldouts are closed in the Editor: click through the page (the same handlers)
  const C = sel => R('var e=ed.querySelector(' + JSON.stringify(sel) + ');e&&e.click();!!e');
  await C('[data-vtstart]'); await p.waitForTimeout(200);
  const rows = await R('[ed.querySelectorAll("[data-vtplay]").length,ed.querySelector(".vtt").textContent.indexOf("edge-")<0]');
  ok('Voice test: the voices as A, B, … with no names before scoring', rows[0] >= 2 && rows[1], rows);
  await C('[data-vtplay="0"]'); await p.waitForTimeout(400);
  ok('...Play plays that voice\'s file of the line', await R('!!G.vtA&&!G.vtA.paused'));
  await C('.vtt tbody tr:nth-child(1) [data-vtr$="|nat|4"]'); await C('.vtt tbody tr:nth-child(2) [data-vtr$="|nat|2"]'); await C('.vtt tbody tr:nth-child(1) [data-vtr$="|acc|5"]');
  await R('var r=ed.querySelector(".vtt tbody tr:nth-child(1) [data-vtbest]");r.checked=true;r.dispatchEvent(new Event("change",{bubbles:true}))');
  await C('[data-vtsave]'); await p.waitForTimeout(500);
  const res = await R('[S.vt.saved,!!S.vt.results,ed.querySelector(".vtt").textContent.indexOf("Edge")>=0||ed.querySelector(".vtt").textContent.indexOf("·")>=0,Object.keys(S.vt.results||{}).length]');
  ok('...Save: the names appear, and the results table has each voice\'s means', res[0] && res[1] && res[2] && res[3] >= 2, res);
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
