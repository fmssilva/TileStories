// test9da (Playwright + the installed Edge): node test9da.js -> PASS/FAIL per check. The first half of round 9's test9d (split in guidance
// round 10, by its own sections; every check kept as it was): the guide's controls on the phone. Pause and Stop at Mini, Compact and
// back, Stop's toast and Resume (the step and the sentence), Ask first, Previous replays the sentence; the ✦ and its menu (every item,
// on the card and on the wall), Azu's tap plays and never opens the menu, the hint at most once, the label that folds; the contrast,
// light and dark;
// the seven scenes in Ask Azu's console; and the fixes: Azu beside the block (0 px over words), the mouth of a paused recorded line,
// the wall's answers in the one order. MUTATE=1 node test9da.js runs it with the toast's Resume starting the tour again from the
// start (the named checks turn red). The other half: test9db.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
const OUT = path.join(__dirname, '..', '..', 'voicebench', 'out', 'azure-sonia-raquel');
const MP3 = fs.readFileSync(path.join(OUT, fs.readdirSync(OUT).filter(f => f.endsWith('.mp3')).sort()[0]));
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  const el = { n: 0 };
  await p.route('**/api.elevenlabs.io/**', r => { const body = JSON.parse(r.request().postData() || '{}'), t = body.text || '', st = [], en = []; el.n++; for (let i = 0; i < t.length; i++) { st.push(i * .06); en.push(i * .06 + .05); }
    r.fulfill({ status: 200, contentType: 'application/json', headers: { 'access-control-allow-origin': '*' }, body: JSON.stringify({ audio_base64: MP3.toString('base64'), alignment: { characters: t.split(''), character_start_times_seconds: st, character_end_times_seconds: en } }) }); });
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  // a finger on the phone: a real click at the middle of an element, a real drag on it
  const box = async sel => p.locator('#ph ' + sel).first().boundingBox();
  const tap = async sel => { const r = await box(sel); if (!r) return false; await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return true; };
  const drag = async (sel, dy) => { const r = await box(sel); await p.mouse.move(r.x + r.width / 2, r.y + r.height / 2); await p.mouse.down(); await p.mouse.move(r.x + r.width / 2, r.y + r.height / 2 + dy, { steps: 6 }); await p.mouse.up(); };
  const size = async sel => { const r = await box(sel); return r ? [Math.round(r.width), Math.round(r.height)] : [0, 0]; };
  if (process.env.MUTATE) { await R('resumeTourAt=function(pt){pt=pt||G.resumeAt;if(!pt)return;G.resumeAt=null;hideStopUi();if(G.on)stopTour();startTour()}'); console.log('MUTATION: the toast\'s Resume starts the tour again from the start (no step, no sentence)'); }
  // the device voice, muted: its words go on by time (one every ~350 ms), the same on every run
  const base = 'S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.miniCtl="pausestop";S.guideCfg.swipeCompact=true;S.guideCfg.stopUndo="toast";S.guideCfg.gbtn="both";S.guideCfg.gbIcon="star";S.guideCfg.gbSide="left";S.guideCfg.gbFold=true;S.guideCfg.azuTap="play";S.guideCfg.life="lively";S.m.solemn=false;S.guideCfg.captions=true;G.resumeAt=null;hideStopUi();S.camCfg.wall="ribeira";G.cam.open=false;S.settingsOpen=false;';
  const tourAt = async (id, h) => { await R(base + `if(G.on)stopTour();S.view="card";S.stop="full";renderAll(false);startTour();var j=S.guideSteps.findIndex(function(s){return s.id==="${id}"});if(j>0){stopSpeech();G.i=j;runStep()}S.dockH="${h || 'mini'}";renderGuide()`); await W(500); };
  const past = async () => { await R(base + 'if(G.on)stopTour();G.fab.menu=false;S.view="card";S.stop="full";renderAll(false);scr.scrollTop=900;fabLayer()'); await W(400); };

  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  // ---------- the fixes ----------
  // a. Azu points from beside the block, never over words (Quick facts, the story's inside, the 1662 drawing), light and dark
  const inkOver = 'var m=ph.querySelector(".mascot").getBoundingClientRect(),a=0,w=document.createTreeWalker(scr,NodeFilter.SHOW_TEXT),n,rg=document.createRange();while((n=w.nextNode())){if(!/\\S/.test(n.nodeValue))continue;var cs=getComputedStyle(n.parentElement);if(cs.visibility==="hidden"||cs.display==="none"||cs.opacity==="0")continue;rg.selectNodeContents(n);[].forEach.call(rg.getClientRects(),function(r){var x=Math.min(r.right,m.right)-Math.max(r.left,m.left),y=Math.min(r.bottom,m.bottom)-Math.max(r.top,m.top);if(x>1&&y>1)a+=x*y})}Math.round(a)';
  for (const dark of [false, true]) for (const id of ['g1', 'g5', 'g7']) {
    await tourAt(id); await R(`S.dark=${dark};renderAll(true)`);
    let last = null, same = 0; const t0 = Date.now(); while (Date.now() - t0 < 10000 && same < 3) { const y = await R('JSON.stringify([Math.round(ph.querySelector(".mascot").getBoundingClientRect().top),Math.round(scr.scrollTop)])'); same = y === last ? same + 1 : 0; last = y; await W(150); }
    const o = await R(inkOver);
    ok(`a. ${dark ? 'dark' : 'light'}, step ${id}: Azu points from beside the block, 0 px over any words`, o === 0 && await R('ph.querySelector(".mascot").classList.contains("pointing")'), o);
  }
  await R('stopTour();S.dark=false;renderAll(false)');
  // b. the mouth on a recorded line: at a pause it rests (the curves do not hold it open); under Reduce Motion it does not move
  await R(base + 'S.guideMute=false;S.lang="pt";S.guideCfg.azuStyle="tile";S.guideCfg.voice="recorded";S.guideCfg.voiceSet="azure-sonia-raquel";S.view="card";S.stop="full";renderAll(false);startTour();G.i=S.guideSteps.findIndex(function(s){return s.id==="g1"});stopSpeech();runStep()');
  const curves = await until('mouthNow().kind==="curves"&&G.recAudio&&G.recAudio.currentTime>.6', 8000);
  await R('G.explicitPause=true;pauseTour();G.explicitPause=false'); await W(200);
  const held = new Set(); for (let i = 0; i < 10; i++) { held.add(await R('ph.querySelector(".mascot .mouth").getAttribute("d")')); await W(60); }
  const rest = await R('MOUTH[restShape(azMood())]');
  ok('b. A recorded line (its curves) paused: the mouth rests in its mood\'s shape and stays still', curves && held.size === 1 && [...held][0] === rest && await R('AzuGL.get("talk")===0&&G.recAudio.paused'), [curves, [...held], rest]);
  await R('S.reduce=true;resumeTour()'); await W(300);
  const still = new Set(); for (let i = 0; i < 15; i++) { still.add(await R('ph.querySelector(".mascot .mouth").getAttribute("d")')); await W(70); }
  ok('b. ...under Reduce Motion, while it plays, the mouth does not move', still.size === 1 && await R('!!G.recAudio&&!G.recAudio.paused'), [...still]);
  await R('S.reduce=false;stopTour();S.guideMute=true;S.lang="en";S.guideCfg.voice="device"');
  // c. the wall's answers: the one order (sayOrder), Speaking now says which; no camSay.source any more
  await R(base + 'window.LABKEYS={};S.view="card";S.stop="closed";G.cam.open=true;renderPhone(false);S.tab="guide";renderEditor()'); await R('camSay("Test answer on the wall.")'); await W(300);
  const c1 = await R('[extPoint("camSay.source").length,G.sayNow&&G.sayNow.src,G.sayNow&&G.sayNow.answer,(ed.querySelector(".saynow")||{}).textContent,ph.querySelector(".ca-ans").textContent]');
  ok('c. The wall\'s answer takes the one order: no live key here, so this device\'s voice, and Speaking now says why', c1[0] === 0 && c1[1] === 'device' && c1[2] === true && /This device.s voice \(no .* key/.test(c1[3]) && c1[4] === 'Test answer on the wall.', c1);
  await R('window.LABKEYS={eleven:"k",elevenPT:"v",elevenEN:"v"};S.agentCfg.liveVoice="eleven";camSay("Another answer on the wall.")'); await until('!!G.recAudio', 4000);
  const c2 = await R('[G.sayNow&&G.sayNow.src,(ed.querySelector(".saynow")||{}).textContent,!!G.recAudio]');
  ok('c. ...with the live voice\'s key: the live voice first (an answer), as on the card; Speaking now says Live', c2[0] === 'live' && /^Live · ElevenLabs/.test(c2[1]) && c2[2] && el.n >= 1, c2);
  await R('stopSpeech();window.LABKEYS={};S.agentCfg.liveVoice="azure";G.cam.open=false;S.stop="full";renderPhone(false)');

  // ---------- Mini: Pause and Stop ----------
  await tourAt('g1', 'mini');
  const ms = [await size('.gd-mini [data-g="pause"]'), await size('.gd-mini [data-g="stop"]')];
  ok('Mini: Pause and Stop in the Mini, 44 px or more; no keyboard at Mini (round 11: typing is Open\'s ask field)', ms.every(s => s[0] >= 44 && s[1] >= 44) && await R('!ph.querySelector(".gd-main .gd-kb")'), ms);
  await tap('.gd-mini [data-g="pause"]'); await W(200); const k0 = await R('G.hl.k'); await W(800);
  ok('...a tap on Pause pauses the voice (its word holds)', await R('G.paused') && await R('G.hl.k') === k0, [k0]);
  await tap('.gd-mini [data-g="play"]'); await W(900);
  ok('...a tap on Play goes on from there', await R('!G.paused') && await R('G.hl.k') > k0);

  // ---------- Compact ----------
  await drag('.gd-grab', 90); await W(300);
  const cp = await R('[S.dockH,!!ph.querySelector(".gdock"),!!ph.querySelector(".gbtn.tour .gb-ring"),!!ph.querySelector(".gb-pause"),!!G.timer&&!G.paused]');
  ok('Compact: a swipe down on the grabber at Mini hides the dock; the voice goes on; the ✦ shows the ring and a pause', cp[0] === 'compact' && !cp[1] && cp[2] && cp[3] && cp[4], cp);
  const ring = []; for (let i = 0; i < 2; i++) { ring.push(await R('parseFloat(ph.querySelector(".gbtn.tour .pg").style.strokeDasharray)')); await W(1200); }
  ok('...the ring grows as the tour goes on', ring[1] > ring[0], ring);
  await tap('.gb-pause'); await W(200); ok('...its pause pauses', await R('G.paused&&!!ph.querySelector(\'.gb-pause[data-g="play"]\')'));
  await tap('.gb-pause'); await W(200);
  await tap('.gbtn.tour'); await W(250); ok('...a tap on the ✦ brings the dock back at Mini', await R('S.dockH==="mini"&&!!ph.querySelector(".gdock.h-mini")&&!G.paused'));
  await tap('.gd-hide'); await W(250); ok('The hide chevron hides the dock too (Compact)', await R('S.dockH==="compact"&&!ph.querySelector(".gdock")'));
  await tap('.gbtn.tour'); await until('!!ph.querySelector(".gd-grab")', 5000); await R('S.guideCfg.swipeCompact=false'); await drag('.gd-grab', 90); await W(250);
  ok('Swipe down to Compact Off: a swipe down at Mini stays at Mini', await R('S.dockH') === 'mini');
  // round 21 (21.5): at Compact a tour's words go to the teleprompter (Words during a tour, the default), never a bubble over the ✦; with
  // captions off, nowhere
  await tourAt('g0', 'compact'); await W(400);
  const bw = await R('var t=ph.querySelector(".gtele:not(.tl-out)"),w=ph.querySelector(".gbtn-wrap.intour");[!!t&&!!t.querySelector(".tl-now .tw"),!ph.querySelector(".gbubble"),!!w]');
  ok('Compact: the mascot\'s words in the teleprompter (round 21), no bubble, the ✦ there', bw[0] && bw[1] && bw[2], bw);
  await R('S.guideCfg.captions=false;renderGuide()'); ok('...with captions off, nowhere', await R('!ph.querySelector(".gbubble")&&!ph.querySelector(".gcap")&&!ph.querySelector(".gtele:not(.tl-out)")'));
  await R('S.guideCfg.captions=true');

  // ---------- Stop: the toast and Resume; Ask first ----------
  await tourAt('g3', 'mini'); await until('G.hl&&G.hl.k>=13', 9000);
  const at = await R('[S.guideSteps[G.i].id,G.hl.k,sentenceStart(G.hl.k)]');
  await tap('.gd-mini [data-g="stop"]'); await W(200);
  const ts = await R('var t=ph.querySelector(".gtoast");[G.on,!!t&&t.textContent,!!ph.querySelector(".gstopask"),!!G.resumeAt]');
  // round 17b: the toast offers the one way back: "Continue the tour (step N of M)" and "Start again" (the ✦ menu, the hero and the invitation say the same)
  ok('Stop: no question; the tour stops; a toast "Tour stopped" with "Continue the tour (step N of M)", "Start again" and What next? (round 17c)', !ts[0] && /^Tour stoppedContinue the tour \(step\s\d+\sof\s\d+\)Start again(What next\?)?$/.test(ts[1]) && !ts[2] && ts[3], ts);
  await tap('[data-gstop="resume"]'); await W(300);
  const rs = await R('[G.on,S.guideSteps[G.i].id,G.hl.k,!!ph.querySelector(".gtoast")]');
  ok('...Continue goes back to the step and the sentence it stopped at', rs[0] && rs[1] === at[0] && rs[2] >= at[2] && rs[2] < at[1] && !rs[3], [at, rs]);
  await tourAt('g3', 'mini'); await until('G.hl&&G.hl.k>=13', 9000); await tap('.gd-mini [data-g="stop"]'); await W(5300);
  ok('...the toast goes after 5 s; the point is kept: the ✦ offers Continue the tour', await R('!ph.querySelector(".gtoast")&&!!G.resumeAt'));
  await R('S.view="card";S.stop="full";renderPhone(true);scr.scrollTop=900;fabLayer()'); await W(300); await tap('.gbtn'); await W(250);
  const mi = await R('ph.querySelector(\'[data-gbgo="tour"]\').textContent');
  await tap('[data-gbgo="tour"]'); await W(300);
  ok('...its Continue the tour (step N of M) (the step\'s name under it) goes back to that step and sentence', /^Continue the tour \(step\s\d+\sof\s\d+\)./.test(mi) && await R(`G.on&&S.guideSteps[G.i].id==="${at[0]}"&&G.hl.k>=${at[2]}&&G.hl.k<${at[1]}`), mi);
  await tourAt('g5', 'mini'); await R('S.guideCfg.stopUndo="ask"'); await W(600); await tap('.gd-mini [data-g="stop"]'); await W(200);
  ok('Stop = Ask first: the tour pauses and asks "Stop the tour?"', await R('G.on&&G.paused&&!!ph.querySelector(".gstopask")&&/Stop the tour\\?/.test(ph.querySelector(".gstopask").textContent)'));
  await tap('[data-gstop="no"]'); await W(250); ok('...Keep listening: it goes on', await R('G.on&&!G.paused&&!ph.querySelector(".gstopask")'));
  await tap('.gd-mini [data-g="stop"]'); await W(200); await tap('[data-gstop="yes"]'); await W(250);
  ok('...Stop: it stops, with no toast', await R('!G.on&&!ph.querySelector(".gtoast")&&!ph.querySelector(".gstopask")'));

  // ---------- Previous replays the sentence ----------
  await tourAt('g3', 'peek'); await until('G.hl&&G.hl.k>=13', 9000);
  const pv = await R('[G.i,G.hl.k,sentenceStart(G.hl.k)]'); await tap('.gd-ctl [data-g="prev"]'); await W(150);
  const pv1 = await R('[G.i,G.hl.k]');
  ok('Previous mid-sentence: the same step, the sentence again from its first word', pv[2] > 0 && pv1[0] === pv[0] && pv1[1] >= pv[2] && pv1[1] < pv[1], [pv, pv1]);
  await tap('.gd-ctl [data-g="prev"]'); await W(250);
  ok('...a second tap within 2 s goes to the step before', await R('G.i') === pv[0] - 1);
  await until('G.hl&&G.hl.k>=3', 5000); await W(2200); const pv2 = await R('[G.i,G.hl.k]'); await tap('.gd-ctl [data-g="prev"]'); await W(150);
  ok('...after 2 s, a tap replays again (it does not go back a step)', await R('G.i') === pv2[0] && await R('G.hl.k') <= pv2[1], pv2);
  await R('stopTour()');

  tsec('fixes, Mini, Compact, Stop, Previous');
  // ---------- the ✦ and its menu ----------
  await past();
  const gb = await R('var w=ph.querySelector(".gbtn-wrap"),b=ph.querySelector(".gbtn"),a=ph.querySelector(".azfab"),n=document.getElementById("navbar");[!!b,!!a,b&&a&&b.getBoundingClientRect().right<=a.getBoundingClientRect().left,Math.round(n.getBoundingClientRect().top-w.getBoundingClientRect().bottom),ph.querySelector(".gbtn .gb-lab").textContent]');
  ok('Past the hero: the ✦ (labelled Guide) and Azu beside it, not on each other, above the Navigation Bar', gb[0] && gb[1] && gb[2] && gb[3] >= 0 && gb[4] === 'Guide', gb);
  ok('...44 px targets', (await size('.gbtn'))[1] >= 44 && (await size('.azfab'))[1] >= 44, [await size('.gbtn'), await size('.azfab')]);
  await R('scr.scrollTop=0'); await W(600); ok('...with the hero\'s Start the guide in sight: no ✦ (the hero is the way in)', await R('!ph.querySelector(".gbtn-wrap")'));
  await R('scr.scrollTop=900'); await W(600);
  await tap('.gbtn'); await W(250);
  const items = await R('[].map.call(ph.querySelectorAll(".gbmenu [data-gbgo]"),function(b){return b.dataset.gbgo})');
  const ih = await R('[].map.call(ph.querySelectorAll(".gbmenu [data-gbgo]"),function(b){return Math.round(b.getBoundingClientRect().height)})');
  ok('A tap on the ✦: its menu on the card: Start the tour, Ask (type or talk), How does this work?, Things to try (guidance round 9), Guide settings (44 px items)', items.join() === 'tour,ask,how,try,settings' && ih.every(h => h >= 44), [items, ih]);
  await R('scr.dispatchEvent(new MouseEvent("click",{bubbles:true}))'); await W(200); ok('...a tap elsewhere closes it', await R('!ph.querySelector(".gbmenu")&&!G.fab.menu'));
  await past(); await tap('.gbtn'); await W(200); await tap('[data-gbgo="tour"]'); await W(400);
  ok('...Start the tour: the tour runs; the ✦ grew into the panel (hidden while it shows, round 11), Azu becomes the guide', await R('G.on&&!ph.querySelector(".gbtn-wrap")&&!!ph.querySelector(".gdock")&&!ph.querySelector(".azfab")&&!!ph.querySelector(".mascot")'));
  await W(400); await tap('.gd-hide'); await W(450); ok('...during the tour the hide chevron hides the dock, back into the ✦', await R('S.dockH==="compact"&&!!ph.querySelector(".gbtn.tour")'));
  await tap('.gbtn.tour'); await W(450); ok('...and a tap on the ✦ shows it again', await R('S.dockH==="mini"'));
  await past(); await tap('.gbtn'); await W(200); await tap('[data-gbgo="ask"]'); await W(300);
  ok('...Ask (type or talk): the conversation opens, its text field ready', await R('G.on&&S.dockH==="full"&&document.activeElement&&document.activeElement.id==="gq"'));
  await past(); await tap('.gbtn'); await W(200); await tap('[data-gbgo="settings"]'); await W(300);
  ok('...Guide settings: Settings opens at the guide\'s group (voice, speed, captions)', await R('var v=ph.querySelector(".vset"),g=v&&v.querySelector(".vguide");!!g&&g.querySelectorAll("[data-gset]").length===7&&g.getBoundingClientRect().top>=v.getBoundingClientRect().top-1'));
  await tap('[data-gset="speed|1.2"]'); await W(200); await tap('[data-gset="voice|recorded"]'); await W(200);
  ok('...its choices set the guide (Faster: speed 1.2; Recorded voice)', await R('S.guideCfg.speed===1.2&&S.guideCfg.voice==="recorded"&&ph.querySelector(\'[data-gset="speed|1.2"]\').getAttribute("aria-pressed")==="true"'));
  await R('S.guideCfg.speed=1;S.guideCfg.voice="device";S.settingsOpen=false;renderAll(true)');
  // on the wall: in the corner; Ask about the view, named after the point in the middle
  await R(base + 'setWall("ribeira")'); await W(500);
  const wl = await R('var w=ph.querySelector(".gbtn-wrap");[!!w&&w.classList.contains("onwall"),Math.round(ph.getBoundingClientRect().bottom-w.getBoundingClientRect().bottom)]');
  await tap('.gbtn'); await W(250);
  const wi = await R('[[].map.call(ph.querySelectorAll(".gbmenu [data-gbgo]"),function(b){return b.dataset.gbgo}).join(),(ph.querySelector(\'[data-gbgo="view"]\')||{}).textContent,camView().central&&camView().central.name]');
  ok('On the wall: the ✦ in the corner; its menu adds Ask about the view, named after the point in the middle', wl[0] && wl[1] <= 30 && wi[0] === 'tour,ask,view,how,try,settings' && wi[1] === 'Ask about ' + String(wi[2]).replace(/^(The|An?)(?=\s)/, a => a.toLowerCase()), [wl, wi]);
  await tap('[data-gbgo="view"]'); await W(300);
  ok('...Ask about the view: its panel opens on the wall', await R('G.cam.open&&!!ph.querySelector(".camask.open")'));
  await R('G.cam.open=false;renderPhone(true)'); await tap('.gbtn'); await W(200); await tap('[data-gbgo="tour"]'); await W(500);
  ok('...Start the tour from the wall: the card opens at Full and the tour runs', await R('G.on&&S.stop==="full"'));
  await R('stopTour()');

  // ---------- Azu: a tap plays; the menu only when asked; the hint ----------
  await past(); const pk0 = await R('G.pokes||0'); await tap('.azfab'); await W(150);
  ok('A tap on Azu (Tap on Azu = Play): it plays (a poke), never the menu', await R('G.pokes||0') === pk0 + 1 && await R('!G.fab.menu&&!ph.querySelector(".gbmenu")'));
  await R('S.guideCfg.azuTap="menu";G.taps=[]'); await W(1300); await tap('.azfab'); await W(200);
  ok('...Tap on Azu = Opens the menu: the ✦\'s menu opens', await R('!!ph.querySelector(".gbmenu")'));
  await past(); await R('S.guideCfg.azuTap="point";G.taps=[]'); await tap('.azfab'); await W(300);
  ok('...Tap on Azu = Points to the ✦ button: Azu points at the ✦ and says "Need anything? Tap ✦."', await R('ph.querySelector(".azfab").classList.contains("point-gb")&&(ph.querySelector(".gdbub:not(.gd-out) button[data-gdbub]")||{}).textContent==="Need anything? Tap ✦."'));
  await R('S.guideCfg.azuTap="play"'); await tourAt('g1', 'mini'); const pk1 = await R('G.pokes||0'); await tap('.mascot'); await W(150);
  ok('...during the tour, a tap on the guide\'s Azu plays too and leaves the dock as it is', await R('G.pokes||0') === pk1 + 1 && await R('S.dockH==="mini"&&!G.paused'));
  await past(); await R('S.guideCfg.gbtn="azu";fabLayer()'); await W(150); await tap('.azfab'); await W(200);
  ok('Guide button = Azu only: no ✦; Azu is the button, its tap opens the menu', await R('!ph.querySelector(".gbtn")&&!!ph.querySelector(".gbmenu")'));
  await past(); await R('S.guideCfg.gbtn="star";fabLayer()'); await W(150);
  ok('Guide button = ✦ only: the ✦, no Azu beside it', await R('!!ph.querySelector(".gbtn")&&!ph.querySelector(".azfab")'));
  // the hint: after a few pokes, at most once a visit; Reduce Motion: words only; Calm: never by itself
  const pokeN = async n => { for (let i = 0; i < n; i++) { await tap('.azfab'); await W(650); } };
  // (guidance round 3: the hint is Guide › Guidance › The wall › The ✦: its bubble, once a visit in the visitor's memory; it waits for Azu's own joke)
  const hintN = 'gdVisitOf(S.gdVisitor,"gbhint").shown';
  await past(); await R('gdStop("t");gdResetVisitor();S.guidance.items.gbhint.on=true;G.pokes=0'); await pokeN(4); await until(hintN + '===1', 8000); await W(300);
  const h1 = await R('[(ph.querySelector(".gdbub:not(.gd-out) button[data-gdbub]")||{}).textContent==="Need anything? Tap ✦.",ph.querySelector(".azfab").classList.contains("point-gb"),' + hintN + '===1]');
  await pokeN(4); await W(1300);
  ok('The hint: after a few pokes Azu points at the ✦ and says it; not again in the visit', h1.every(Boolean) && await R(hintN + '===1'), h1);
  await past(); await R('gdStop("t");gdResetVisitor();G.pokes=0;S.reduce=true;renderPhone(true);scr.scrollTop=900;fabLayer()'); await W(200); await pokeN(4); await until(hintN + '===1', 8000); await W(300);
  ok('...under Reduce Motion: the words only, nothing moves (no arm)', await R(hintN + '===1&&!!ph.querySelector(".gdbub")&&!ph.querySelector(".azfab.point-gb")&&!G.pointTo'));
  await past(); await R('gdStop("t");gdResetVisitor();G.pokes=0;S.guideCfg.life="calm"'); await pokeN(4); await W(1300);
  ok('...Calm: never by itself', await R(hintN + '===0'));
  await R('S.guideCfg.life="lively";gdStop("t");gdVM("gbhint").shown=1');

  // ---------- the label folds after 4 s of being seen (the Locate Button's rule and code) ----------
  await past(); await R('G.gb.folded=false;G.gb.shown=0;fabLayer()'); await W(300);
  const lb0 = await R('!ph.querySelector(".gb-lab").classList.contains("fold")'); await W(4500);
  ok('The label "Guide" folds into the ✦ after 4 s of being seen: one rule with the Locate Button (seenLabel)', lb0 && await R('ph.querySelector(".gb-lab").classList.contains("fold")&&G.gb.folded&&SEEN.length===2'));
  await R('G.gb.folded=false;G.gb.shown=0;S.guideCfg.gbFold=false;fabLayer()'); await W(4500);
  ok('...Label folds after 4 s = Off: the word stays', await R('!ph.querySelector(".gb-lab").classList.contains("fold")'));
  await R('S.guideCfg.gbFold=true;G.gb.folded=true');

  tsec('the ✦, Azu, the hint, the label');
  // ---------- contrast: words 4.5:1, controls 3:1, light and dark ----------
  const CR = 'var px=function(c){var m=c.match(/[\\d.]+/g).map(Number),k=c.indexOf("color(")===0?255:1;return{r:m[0]*k,g:m[1]*k,b:m[2]*k,a:m.length>3?m[3]:1}},lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)},over=function(f,b){return{r:f.r*f.a+b.r*(1-f.a),g:f.g*f.a+b.g*(1-f.a),b:f.b*f.a+b.b*(1-f.a),a:1}},cr=function(a,b){var x=lum(a),y=lum(b);return Math.round((Math.max(x,y)+.05)/(Math.min(x,y)+.05)*100)/100},cs=function(s){return getComputedStyle(ph.querySelector(s))},bgOf=function(s){var e=ph.querySelector(s);while(e){var c=px(getComputedStyle(e).backgroundColor);if(c.a>.5)return c;e=e.parentElement}return{r:255,g:255,b:255,a:1}};';
  for (const dark of [false, true]) {
    await past(); await R(`G.resumeAt={id:"g3",k:3,title:"Story"};S.dark=${dark};renderAll(true);scr.scrollTop=900;fabLayer();G.fab.menu=true;fabLayer()`); await W(300);
    for (let i = 0; i < 8 && !(await R('!!ph.querySelector(".gbtn .gb-ic svg")&&!!ph.querySelector(".gbmenu small")')); i++) { await R('S.view="card";S.stop="full";G.on&&stopTour();scr.scrollTop=900;G.fab.menu=true;fabLayer()'); await W(300); }
    const a = await R(CR + 'var disc=px(cs(".gbtn").backgroundColor),card=bgOf("#scr .item")||px(getComputedStyle(scr).backgroundColor),rim=px(cs(".gbtn").borderTopColor),pg=bgOf("#scr");[cr(px(cs(".gb-ic svg").fill),disc),cr(px(cs(".gbtn").color),disc),Math.max(cr(disc,pg),cr(rim,pg)),cr(px(cs(".gbmenu button").color),px(cs(".gbmenu").backgroundColor)),cr(px(getComputedStyle(ph.querySelector(".gbmenu small")||ph.querySelector(".gbmenu button")).color),px(cs(".gbmenu").backgroundColor))]');
    await tourAt('g5', 'mini'); await R(`S.dark=${dark};renderAll(true)`); await W(300);
    const dk = { r: 22, g: 24, b: 28, a: 1 };
    // round 21 (21.6): the Mini is no longer always dark (The panels' look follows the theme): its icons are measured on its own glass over
    // the worst backdrop (black and white), the hide chevron on its own tab
    const t = await R(CR + 'var sf=px(cs(".gdock").backgroundColor),bk=[{r:0,g:0,b:0,a:1},{r:255,g:255,b:255,a:1}].map(function(b){return over(sf,b)}),mc=function(b){return cr(px(cs(".gd-mc").color),over(px(cs(".gd-mc").backgroundColor),b))},tab=px(getComputedStyle(ph.querySelector(".gd-hide"),"::before").backgroundColor);[Math.min.apply(null,bk.map(mc)),cr(over(px(cs(".gd-hide").color),tab),tab)]');
    t.push(await R(CR + 'S.dockH="compact";renderGuide();var r=cr(px(cs(".gb-ring .pg").stroke),px(cs(".gbtn.tour").backgroundColor));S.dockH="mini";renderGuide();r')); // round 11: the ✦ and its ring show at Compact
    await R('ph.querySelector(\'.gd-mini [data-g="stop"]\').click()'); await W(200);
    const tt = await R(CR + 'var t=ph.querySelector(".gtoast");[cr(px(cs(".gtoast").color),px(cs(".gtoast").backgroundColor)),cr(px(cs(".gtoast button").color),px(cs(".gtoast button").backgroundColor))]');
    await R('hideStopUi()');
    const all = { '✦ on its disc': a[0], 'label': a[1], 'button vs the page': a[2], 'menu words': a[3], 'menu small words': a[4], 'Mini icons': t[0], 'hide chevron': t[1], 'ring vs disc': t[2], 'toast words': tt[0], 'toast button': tt[1] };
    const words = ['label', 'menu words', 'menu small words', 'toast words', 'toast button'];
    ok(`Contrast, ${dark ? 'dark' : 'light'}: the words at 4.5:1 or more, the controls (✦, icons, ring, the button's edge) at 3:1 or more`, Object.keys(all).every(k => all[k] >= (words.indexOf(k) >= 0 ? 4.5 : 3)), all);
  }
  await R('S.dark=false;G.fab.menu=false;renderAll(false)');

  tsec('contrast');
  // ---------- the scenes in Ask Azu's console, pressed as the developer does ----------
  await R(base + 'S.ui.level=1;S.tab="guide";S.op={gt3:true,s7agent:true};renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');if(f)f.open=true');
  const scenes = await R('AGSCENES.map(function(s,i){return[i,s.id,s.label]}).filter(function(x){return/^g_/.test(x[1])})');
  ok('The console lists the guide\'s scenes (Tour at Mini, Compact, Stop and its toast, Previous, the ✦ menu, Azu\'s tap, the hint)', scenes.length === 7 && await R('var o=[].map.call(ed.querySelectorAll(".agscsel option"),function(o){return o.textContent}).join("|");/Guide · the tour at Mini/.test(o)&&/hidden \\(Compact\\)/.test(o)&&/Stop, its toast/.test(o)'), scenes.map(s => s[2]));
  for (const [i, id, label] of scenes) {
    await R(`var s=ed.querySelector(".agscsel");s.value="${i}";s.dispatchEvent(new Event("change",{bubbles:true}))`); await W(150);
    const does = await R('ed.querySelector(".agscenes").textContent');
    await p.locator('[data-agscrun]').click();
    await until(`!!AG.sceneRes["${id}"]`, 30000); const r = await R(`AG.sceneRes["${id}"]`);
    ok(`Scene "${label}": Set up and ask runs it, ✓`, /Does: nothing:/.test(does) && r && r.ok === true, r && [r.note, r.answer]);
    await R('if(G.on)stopTour();hideStopUi();S.settingsOpen=false;G.cam.open=false');
  }
  tsec('scenes');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
