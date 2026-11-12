// test9v (Playwright + the installed Edge): node test9v.js -> PASS/FAIL per check. Guidance round 17c: the tour's end (guide/tour-end.js),
// where the tour leaves the visitor and the What next? panel, and the Stop bar's third choice (the developer's answer, 2026-11-12).
// [showcase] after the end line the card's showcase sits whole between the card's top (below the notch, 47 px) and the panel, by the test's
//   own arithmetic on the drawn boxes: the quiz not played; the quiz played (a real tap on an answer) -> the gallery; a block picked; the hero.
// [asks] Azu's bubble: a line of Azu's intro lines › After the tour (EN and PT), warm (a question, no "Now", at most 20 words), spoken, never the
//   same line twice in a row, faded a few seconds after it is said.
// [panel] at most 34 % of 844 px, above the bottom bar, inside the phone, no sideways scroll of the page, every target 44 px (its box and
//   elementFromPoint 20 px each side), its words 4.5:1 (G15.text), light and dark.
// [header] "Tour complete · " and the same length as the hero's button; the quiz's star and score once it is played.
// [questions] 2-3 chips, this point's own or the templates (the test's own patterns), none a sentence the tour said; a heading the tour told
//   word for word gives no chip (the written step edited to say it); a real tap asks (the model stubbed, never live).
// [ask] the field (real keys) and the microphone (a recogniser stand-in) reach Ask Azu.
// [nearby] 3-5 cards ordered picks -> category -> distance (the test's own distances from the wall's coordinates); a real tap: minimal at
//   once, the wall flown to the point, its card at Half with its invitation ("A short tour of the Torreão?"), whose tap starts its tour.
// [minimal] the chevron and a swipe down make the 56 px pill; a tap opens it; Done closes; the ✦ menu's What next? (first) brings it back.
// [written] the written tour and a tour from the card end the same way.
// [stop bar] (the developer) the Stop bar's three choices, 44 px: Continue (back to the step), Start again (the teaser), What next? (the panel,
//   "Tour paused · step N of M", Continue first of its actions, which goes back to the step); light and dark.
// [editor] each new row of Guide › 2 · The tour › The tour's end changed by a real click changes what the visitor sees, its demo playing the
//   tour's end; the ▸ on its label plays it; ▲▼, a point's own question typed, a template's words typed. [reduced motion] a fade only.
// [ask] (the Architect) Ask from the panel keeps the card's scroll and runs no step; Ask closed, the panel comes back as it was.
// [nearby] (the developer): the points with a card and a tour always first (picks, category, nearest among them), then the places
//   without a card in the same order.
// MUTATE=1: Ask through the ✦ menu's path (the tour restarted), the showcase forgets the quiz was played, the answered rule off, the wall's
//   distances ignored, the Stop bar's What next? doing nothing: the checks named [ask], [showcase], [header], [questions], [nearby] and [stop
//   bar] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const ctx = await b.newContext({ viewport: { width: 1500, height: 1000 } }), p = await ctx.newPage();
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 900) : '')); };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  const MUT = 'teAskEnter=function(){teClose(false);S.view="card";S.stop="full";startTour();pauseTour();G.askOpen=true;S.dockH="full";renderGuide()};tePlayed=function(){return false};teAnswered=function(){return false};teDist=function(){return 0};teOpenPaused=function(){};""';
  const load = async () => { await p.goto('about:blank'); await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await W(400); if (process.env.MUTATE) await R(MUT); await R(require('./g15lib.js')); };
  if (process.env.MUTATE) console.log('MUTATION: Ask restarts the tour, the quiz never played, the answered rule off, no distances, the Stop bar\'s What next? does nothing');
  const base = `SCREENS.phone=[390,844];if(G.on)stopTour();S.previewBar=false;S.hints.on=false;S.guidance.on=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.cardCfg.mode="light";
    S.guideCfg.voice="device";S.guideCfg.look="ink";S.ui.rowDemo=false;S.guideCfg.music=false;S.vis.music=null;S.vis.speed=2;S.agentCfg.voiceOut=false;G.gd.manual=true;S.guideCfg.dockStart="mini";gdPrefs().games="off";
    window.__calls=[];sampleFn=function(pr){window.__calls.push(String(pr));return Promise.resolve({text:"A stub answer."})};S.guideCfg.ask="ai";renderAll(true);""`;
  const DEF = await (async () => { await load(); await R(base); return R('JSON.stringify({teOn:S.guideCfg.teOn,teWhere:S.guideCfg.teWhere,teBlock:S.guideCfg.teBlock,teStart:S.guideCfg.teStart,teSec:S.guideCfg.teSec,teOrder:S.guideCfg.teOrder,teQsN:S.guideCfg.teQsN,teQsSrc:S.guideCfg.teQsSrc,teQs:S.guideCfg.teQs,teNearN:S.guideCfg.teNearN,teNearRule:S.guideCfg.teNearRule,tePicks:S.guideCfg.tePicks})'); })();
  const reset = () => R(`Object.assign(S.guideCfg,JSON.parse(${JSON.stringify(DEF)}));G.te.played={};""`);
  // the panel at rest: drawn, its entrance over, the scroll where the end put it
  const settled = () => until('!!ph.querySelector(".tepanel,.tepill")&&!document.getAnimations().some(function(a){var t=a.effect&&a.effect.target;return a.playState==="running"&&!!t&&!!t.closest&&!!t.closest(".tepanel,.tepill,.teazu")})&&(!G.te.sc||Math.abs(scr.scrollTop-G.te.scTop)<2||G.te.userScroll)', 6000);
  const heroStart = async () => { await R('if(G.on)stopTour();G.te.open=false;S.view="card";S.stop="full";renderPhone(false);scr.scrollTop=0;""'); await W(150); await p.click('#ph .hero .gstart'); await until('G.on', 3000); };
  const toEnd = async () => { await heroStart(); await R('stopSpeech();G.paused=false;G.i=S.guideSteps.length-1;runStep();""'); const got = await until('!G.on&&(!!ph.querySelector(".tepanel,.tepill")||G.te.open==="min")||G.askOpen', 25000); if (got) { await R('ph.scrollLeft=0;ph.scrollTop=0;var b=ph.querySelector(".tebub");window.__bub=b?b.textContent:"";""'); await settled(); } return got; };
  const box = sel => R(`(function(){var e=${sel};if(!e)return null;var r=e.getBoundingClientRect(),P=ph.getBoundingClientRect(),q=P.height/ph.offsetHeight;return{t:(r.top-P.top)/q,b:(r.bottom-P.top)/q,l:(r.left-P.left)/q,r:(r.right-P.left)/q,h:r.height/q,w:r.width/q}})()`);
  const scEl = 'G.te.sc?(G.te.sc.ref==="hero"?scr.querySelector(".hero .w"):scr.querySelector(\'[data-ref="\'+G.te.sc.ref+\'"]\')):null';
  const whole = async () => { const s = await box(scEl), pn = await box('ph.querySelector(".tepanel,.tepill")'), ref = await R('G.te.sc&&G.te.sc.ref'); const top = 47;
    return { ref, ok: !!s && !!pn && s.t >= top && s.b <= pn.t + 0.5, s: s && [Math.round(s.t), Math.round(s.b)], panelTop: pn && Math.round(pn.t) }; };
  await R(base);

  // ---------- 1. [showcase] each case ----------
  const sc1 = (await toEnd()) && await whole();
  await R('scr.scrollTop=0;""'); const qb = '#ph [data-ref="b:b_quiz"] .kq button[data-ok]'; await R('G.te.open=false;teDraw();""'); await p.click(qb); const played = await R('tePlayed("painting","b_quiz")');
  const sc2 = (await toEnd()) && await whole(); const star = await R('var s=ph.querySelector(".te-star");s?s.textContent:""');
  await R('S.guideCfg.teWhere="pick";S.guideCfg.teBlock={painting:"b_ba"};""'); const sc3 = (await toEnd()) && await whole();
  await R('S.guideCfg.teWhere="hero";""'); const sc4 = (await toEnd()) && await whole();
  ok('[showcase] the quiz not played: the tour ends on it, whole between the top (47 px) and the panel', sc1 && sc1.ref === 'b:b_quiz' && sc1.ok, sc1);
  ok('[showcase] the quiz played (a real tap on its right answer): the gallery, whole above the panel', played && sc2 && sc2.ref === 'b:b_gal' && sc2.ok, { played, sc2 });
  ok('[showcase] a block picked (the Before and After) and the hero: each whole above the panel', sc3 && sc3.ref === 'b:b_ba' && sc3.ok && sc4 && sc4.ref === 'hero' && sc4.ok, { sc3, sc4 });
  ok('[header] the quiz played: its star and score in the header ("1 of 1")', /1 of 1/.test(star), star);
  await reset(); tsec('showcase');

  // ---------- 2. [asks], [panel], [header] light and dark ----------
  const LINES = JSON.parse(await R('var o={};langCodes().forEach(function(l){o[l]=(S.guideCfg.atIntro.after||[]).map(function(k){return wordIn(S.tx[k],l)})});JSON.stringify(o)'));
  const NAME = { en: 'the Ribeira Palace', pt: 'o Paço da Ribeira' };
  const isLine = (t, l) => LINES[l].some(x => x.replace('{point}', NAME[l]).replace(/^./, c => c.toUpperCase()) === t);
  const warm = t => /\?$/.test(t) && !/^Now\b/.test(t) && t.split(/\s+/).length <= 20;
  const PANEL = [];
  for (const dark of [false, true]) {
    await R(`S.dark=${dark};S.cardCfg.mode="${dark ? 'dark' : 'light'}";renderAll(true);""`);
    await toEnd();
    const m = JSON.parse(await R(`(function(){var pn=ph.querySelector(".tepanel"),nb=document.getElementById("navbar"),b=ph.querySelector(".tebub"),P=ph.getBoundingClientRect(),q=P.height/ph.offsetHeight,R0=function(e){var r=e.getBoundingClientRect();return{t:(r.top-P.top)/q,b:(r.bottom-P.top)/q,l:(r.left-P.left)/q,r:(r.right-P.left)/q,w:r.width/q,h:r.height/q}};
      var pr=R0(pn),nr=nb&&getComputedStyle(nb).display!=="none"?R0(nb):null,tg=[];
      [].forEach.call(document.querySelectorAll("#ph .tepanel button,#ph .tepanel input"),function(e){var r=e.getBoundingClientRect(),cx=r.left+r.width/2,cy=r.top+r.height/2;for(var a=e.parentElement;a&&a!==ph;a=a.parentElement){var ac=getComputedStyle(a);if(ac.overflowX!=="visible"){var z=a.getBoundingClientRect();if(r.left<z.left-.5||r.right>z.right+.5)return}}
        var hit=[[0,0],[-20,0],[20,0],[0,-20],[0,20]].map(function(d){var h=document.elementFromPoint(cx+d[0]*q,cy+d[1]*q);return!!h&&(h===e||e.contains(h))});tg.push({el:e.className||e.tagName,w:+(r.width/q).toFixed(1),h:+(r.height/q).toFixed(1),ok:hit.every(Boolean)&&r.width/q>=43.5&&r.height/q>=43.5})});
      var tx=G15.text(".tepanel,.tebub").filter(function(x){return x.cr<4.5});
      return JSON.stringify({h:pr.h,top:pr.t,bot:pr.b,l:pr.l,r:pr.r,nav:nr&&nr.t,phw:ph.offsetWidth,sx:scr.scrollWidth-scr.clientWidth,px:pn.scrollWidth-pn.clientWidth,phx:ph.scrollWidth-ph.clientWidth,tg:tg,low:tx.slice(0,4),nt:G15.text(".tepanel,.tebub").length,bub:window.__bub||"",said:G.sayLine&&G.sayLine.text,head:(ph.querySelector(".te-hd")||{}).textContent||"",hero:""})})()`));
    PANEL.push(m);
    ok(`[panel] ${dark ? 'dark' : 'light'}: at most 34 % of 844 px (${Math.round(m.h)} px), above the bottom bar, inside the phone, no sideways scroll, every target 44 px, its words 4.5:1`,
      m.h <= 844 * 0.34 && (m.nav == null || m.bot <= m.nav + 0.5) && m.l >= -0.5 && m.r <= m.phw + 0.5 && m.sx <= 0 && m.px <= 0.5 && m.tg.length >= 8 && m.tg.every(x => x.ok) && !m.low.length && m.nt > 5,
      { h: m.h, bot: m.bot, nav: m.nav, l: m.l, r: m.r, sx: m.sx, px: m.px, n: m.tg.length, nt: m.nt, bad: m.tg.filter(x => !x.ok), low: m.low });
    if (!dark) {
      ok('[asks] Azu\'s bubble is a line of After the tour (EN), warm (a question, no "Now", 20 words at most), and spoken (the line said)', isLine(m.bub, 'en') && warm(m.bub) && m.said === m.bub, { bub: m.bub, said: m.said });
      const heroLen = await R('S.guideCfg.teOn&&(function(){var h=document.createElement("div");h.innerHTML=cardScreen();var g=h.querySelector(".hero .gstart");return g?g.textContent:""})()');
      ok('[header] "Tour complete · " and the same length as the hero\'s button', /^Tour complete · /.test(m.head) && heroLen.indexOf(m.head.replace('Tour complete · ', '')) >= 0, { head: m.head, heroLen });
      const faded = await until('!ph.querySelector(".tebub")', 20000);
      ok('[asks] the bubble fades a few seconds after the line is said', faded);
    }
  }
  const b1 = PANEL[0].bub, b2 = PANEL[1].bub;
  await R('S.dark=false;S.cardCfg.mode="light";S.lang="pt";renderAll(true);""'); await toEnd(); const bpt = await R('window.__bub||""');
  await R('S.lang="en";renderAll(true);""');
  ok('[asks] never the same line twice in a row (two ends here), and in Portuguese a line of the set', b1 !== b2 && isLine(b2, 'en') && isLine(bpt, 'pt') && warm(bpt), { b1, b2, bpt });
  tsec('panel');

  // ---------- 3. [questions] and [ask] ----------
  await toEnd();
  const Q = JSON.parse(await R('JSON.stringify({qs:[].map.call(ph.querySelectorAll(".te-q"),function(x){return x.textContent}),said:S.guideSteps.map(function(s){return stepText(s)}).join(" ")})'));
  const nrm = s => String(s).toLowerCase().replace(/[^\p{L}\p{N}]+/gu, ' ').trim();
  const TPL = [/^Who lived in the palace\?$/, /^(What|Who|Why|How|When|Where|Which) .+ the Ribeira Palace\?$/, /^What happened in \d{4}\?$/, /^Who was [A-Z]\S* [IVX]+\?$/, /^Tell me about .+$/];
  const saidS = String(Q.said).split(/(?<=[.!?])\s+/).map(nrm);
  ok('[questions] 2-3 chips, each this point\'s own or a template\'s (the test\'s own patterns), none a sentence the tour said',
    Q.qs.length >= 2 && Q.qs.length <= 3 && Q.qs.every(q => TPL.some(re => re.test(q))) && Q.qs.every(q => saidS.indexOf(nrm(q)) < 0), Q.qs);
  // the tour tells the callout "What was inside" word for word: its question is no longer offered
  const inside = await R('var r=byId(S.story,"r5");wordIn(r.slots[1].text,"en")');
  await R(`var s=byId(S.guideSteps,"g5");window.__g5=s.text.en;s.text.en=${JSON.stringify(inside)};""`); await toEnd();
  const Q2 = await R('JSON.stringify([].map.call(ph.querySelectorAll(".te-q"),function(x){return x.textContent}))'); await R('byId(S.guideSteps,"g5").text.en=window.__g5;""');
  ok('[questions] a heading the tour told word for word gives no chip: "What was inside the Ribeira Palace?" offered, then gone once the tour says its words', Q.qs.indexOf('What was inside the Ribeira Palace?') >= 0 && JSON.parse(Q2).indexOf('What was inside the Ribeira Palace?') < 0, { before: Q.qs, after: Q2 });
  await toEnd(); const chip = await R('ph.querySelector(".te-q").textContent'); await R('window.__calls=[];window.__rs=0;if(!window.__rsOn){window.__rsOn=1;on("runStep.before",function(){window.__rs++})}""'); const top0 = await R('scr.scrollTop'); await p.click('#ph .te-q');
  const asked = await until('window.__calls.length>=1', 5000), aq = JSON.parse(await R('JSON.stringify({p:(window.__calls[0]||"").slice(-160),full:S.dockH==="full"&&G.askOpen,panel:!!ph.querySelector(".tepanel"),offer:G.te.offer})'));
  ok('[questions] a real tap on a chip asks it at once through Ask Azu (its full view; the model stubbed); the panel closes, the ✦ menu keeps What next?', asked && aq.p.indexOf(chip) >= 0 && aq.full && !aq.panel && aq.offer, { chip, aq });
  const midA = JSON.parse(await R('JSON.stringify({top:scr.scrollTop,rs:window.__rs})'));
  await p.click('#ph .gdock [data-g="stop"]'); const backA = await until('!G.on&&G.te.open==="open"&&!!ph.querySelector(".tepanel")', 4000);
  const endA = JSON.parse(await R('JSON.stringify({top:scr.scrollTop,rs:window.__rs,toast:!!ph.querySelector(".gtoast")})'));
  ok('[ask] Ask from the panel leaves the card where it is: no tour restarted (no step run), the scroll the same during Ask and after it closes (a real tap on its Stop; within 2 px), the panel back open, no Stop toast',
    Math.abs(midA.top - top0) <= 2 && Math.abs(endA.top - top0) <= 2 && midA.rs === 0 && endA.rs === 0 && backA && !endA.toast, { top0, midA, endA, backA });
  await R('S.guideCfg.teQsN="2";""'); await toEnd(); const n2 = await R('ph.querySelectorAll(".te-q").length'); await reset();
  // [ask] the field by real keys, the microphone by a recogniser stand-in
  await toEnd(); await R('window.__calls=[];""'); await p.click('#ph #teq'); await p.keyboard.type('Who built the tower?'); await p.keyboard.press('Enter');
  const fa = await until('window.__calls.some(function(c){return c.indexOf("Who built the tower?")>=0})', 5000); await R('stopTour();""');
  await toEnd(); await R('window.__calls=[];window.SpeechRecognition=function(){var me=this;me.start=function(){setTimeout(function(){me.onresult&&me.onresult({results:[[{transcript:"Where did the king live?"}]]});me.onend&&me.onend()},80)};me.stop=function(){}};""');
  await p.click('#ph .te-mic'); const ma = await until('window.__calls.some(function(c){return c.indexOf("Where did the king live?")>=0})', 5000); await R('stopTour();window.SpeechRecognition=undefined;""');
  ok('[ask] the field (real keys) and the microphone (a recogniser stand-in) reach Ask Azu; How many 2 shows 2 chips', fa && ma && n2 === 2, { fa, ma, n2 });
  tsec('questions');

  // ---------- 4. [nearby] ----------
  const WALL = JSON.parse(await R('JSON.stringify({w:WALLS.ribeira.w,h:WALLS.ribeira.h,p:WALLS.ribeira.pois.map(function(p){return{id:p.id,u:p.u,v:p.v,cat:p.cat,card:!!POINTS[p.id]}})})'));
  const expect = (poi, picks, n) => { const me = WALL.p.find(x => x.id === poi), d = x => Math.hypot((x.u - me.u) * WALL.w, (x.v - me.v) * WALL.h);
    const rest = WALL.p.filter(x => x.id !== poi && picks.indexOf(x.id) < 0).sort((a, c) => d(a) - d(c)); const all = picks.concat(rest.filter(x => x.cat === me.cat).map(x => x.id), rest.filter(x => x.cat !== me.cat).map(x => x.id)), card = id => WALL.p.find(x => x.id === id).card;
    return all.filter(card).concat(all.filter(id => !card(id))).slice(0, n); };
  const cards = () => R('JSON.stringify([].map.call(ph.querySelectorAll(".te-card"),function(x){return{id:x.dataset.tenear,why:x.querySelector("small").textContent}}))').then(JSON.parse);
  await toEnd(); const N1 = await cards();
  await R('S.guideCfg.tePicks={painting:{p1:"river"}};S.guideCfg.teNearN="5";""'); await toEnd(); const N2 = await cards(); await reset();
  ok('[nearby] 3 cards, most related first: the points with a card and a tour first (the same category, the Torreão: "Part of this palace"; then the nearest by the wall\'s coordinates), the places without one after',
    N1.length === 3 && JSON.stringify(N1.map(x => x.id)) === JSON.stringify(expect('painting', [], 3)) && N1[0].why === 'Part of this palace', { got: N1, want: expect('painting', [], 3) });
  ok('[nearby] a pick first ("Recommended · …"), How many 5: five cards, picks -> category -> distance among the points with a card, then the places without one (the city on the hill, the arcades) in the same order', JSON.stringify(N2.map(x => x.id)) === JSON.stringify(expect('painting', ['river'], 5)) && /^Recommended · /.test(N2[0].why), { got: N2, want: expect('painting', ['river'], 5) });
  await toEnd(); await p.click('#ph .te-card[data-tenear="torreao"]'); const minAt = await R('G.te.open');
  const landed = await until('S.m.poi==="torreao"&&S.stop==="half"&&!!ph.querySelector(".teinv")', 6000);
  const NV = JSON.parse(await R('var c=camView(),p=WALLS.ribeira.pois.filter(function(x){return x.id==="torreao"})[0],s=poiScreen(p),t=teFlyTarget(p);JSON.stringify({inv:ph.querySelector(".teinv").textContent.replace(/\\s*›\\s*$/,""),x:s.x,y:s.y,W:ph.offsetWidth,H:ph.offsetHeight,cam:[S.camX,S.camY],t:t,min:tourMinutes("torreao")})'));
  const wantInv = NV.min < 2 ? 'A short tour of the Torreão?' : `The Torreão has a story. A tour of ${NV.min} minutes?`;
  const flown = Math.abs(NV.cam[0] - NV.t.x) < 0.01 && Math.abs(NV.cam[1] - NV.t.y) < 0.01 && NV.x > 0 && NV.x < NV.W && NV.y > 47 && NV.y < NV.H * 0.45;
  await p.click('#ph .teinv button'); const started = await until('G.on&&S.m.poi==="torreao"', 4000); await R('stopTour();""');
  ok('[nearby] a real tap on the Torreão: the panel minimal at once, the wall flown to the point (in view above the card), its card at Half with its invitation; the invitation\'s tap starts its tour',
    minAt === 'min' && landed && flown && NV.inv === wantInv && started, { minAt, landed, NV, wantInv, started });
  tsec('nearby');

  // ---------- 5. [minimal] and [written] (the Torreão: a tour from its card) ----------
  const back = async () => { await R('if(G.on)stopTour();pointOpen("painting");setWall("ribeira");S.view="card";S.stop="full";renderPhone(false);""'); };
  await back(); await toEnd(); await p.click('#ph .te-chev'); const pill = await box('ph.querySelector(".tepill")'); const noPanel = await R('!ph.querySelector(".tepanel")');
  await p.click('#ph .te-po'); const reopened = await until('!!ph.querySelector(".tepanel")', 2000); await settled();
  const hd = await box('ph.querySelector(".tepanel .te-head")'); const P0 = await R('JSON.stringify((function(){var P=ph.getBoundingClientRect(),q=P.height/ph.offsetHeight;return{x:P.left,y:P.top,q:q}})())').then(JSON.parse);
  await p.mouse.move(P0.x + 200 * P0.q, P0.y + (hd.t + 22) * P0.q); await p.mouse.down(); await p.mouse.move(P0.x + 200 * P0.q, P0.y + (hd.t + 90) * P0.q, { steps: 6 }); await p.mouse.up();
  const swiped = await until('G.te.open==="min"&&!!ph.querySelector(".tepill")', 2000); await until('performance.now()-(G.te.swiped||0)>450', 2000);
  await p.click('#ph .te-po'); await until('!!ph.querySelector(".tepanel")', 2000); await settled(); await p.click('#ph .te-acts [data-te="done"]'); const closed = await until('!ph.querySelector(".tepanel,.tepill")', 2000);
  await until('!!ph.querySelector(".gbtn-wrap .gbtn")', 3000); await p.click('#ph .gbtn-wrap .gbtn'); await until('!!ph.querySelector(".gbmenu")', 2000);
  const first = await R('var b=ph.querySelector(".gbmenu [data-gbgo]");b?b.dataset.gbgo+":"+b.textContent:""'); await p.click('#ph .gbmenu [data-gbgo="next"]'); const viaMenu = await until('!!ph.querySelector(".tepanel")', 2000);
  ok('[minimal] the chevron makes the 56 px pill, a tap opens it, a swipe down makes it the pill again, Done closes it, the ✦ menu\'s first item What next? brings it back',
    !!pill && Math.abs(pill.h - 56) < 0.6 && noPanel && reopened && swiped && closed && first === 'next:What next?' && viaMenu, { pill, noPanel, reopened, swiped, closed, first, viaMenu });
  await R('teClose(false);""'); const xs = await R('G.te.open="min";teDraw();!!ph.querySelector(".tepill .te-x")'); await p.click('#ph .tepill .te-x'); const xClosed = await until('!ph.querySelector(".tepill")&&!G.te.open&&G.te.offer', 2000);
  ok('[minimal] the pill\'s × closes it (the offer kept for the ✦ menu)', xs && xClosed);
  const wr = await R('atSrc("painting")'); await back(); const w1 = (await toEnd()) && await whole();
  await R('if(G.on)stopTour();pointOpen("torreao");S.view="card";S.stop="full";renderPhone(false);""'); const cs = await R('atSrc("torreao")'); const w2 = (await toEnd()) && await whole(); const h2 = await R('(ph.querySelector(".te-hd")||{}).textContent||""');
  ok('[written] the written tour (the Ribeira Palace) and a tour from the card (the Torreão) end the same way: the showcase whole above the What next? panel', wr === 'written' && cs === 'card' && w1 && w1.ok && w2 && w2.ok && w2.ref === 'b:t_quiz' && /our complete/.test(h2), { wr, cs, w1, w2, h2 });
  await back(); tsec('minimal');

  // ---------- 6. [stop bar] the Stop bar's three choices ----------
  const stopAt = async () => { await heroStart(); for (let k = 0; k < 3; k++) { const i = await R('G.i'); await R('S.dockH="peek";renderGuide();""'); await p.click('#ph .gdock [data-g="next"]'); await until(`G.i!==${i}`, 3000); }
    await R('S.dockH="peek";renderGuide();""'); await p.click('#ph .gdock [data-g="stop"]'); await until('!G.on&&!!ph.querySelector(".gtoast")', 3000); return R('JSON.stringify(G.resumeAt?{id:G.resumeAt.id}:{})').then(JSON.parse); };
  const SB = [];
  for (const dark of [false, true]) {
    await R(`S.dark=${dark};S.cardCfg.mode="${dark ? 'dark' : 'light'}";renderAll(true);""`); const at = await stopAt();
    await until('!document.getAnimations().some(function(a){var t=a.effect&&a.effect.target;return a.playState==="running"&&!!t&&!!t.closest&&!!t.closest(".gtoast")})', 2000);
    const t = JSON.parse(await R('JSON.stringify([".gtcont",".gtagain",".gtnext"].map(function(s){var e=ph.querySelector(".gtoast "+s);if(!e)return null;var r=e.getBoundingClientRect(),q=ph.getBoundingClientRect().height/ph.offsetHeight,cx=r.left+r.width/2,cy=r.top+r.height/2,h=document.elementFromPoint(cx,cy);return{t:e.textContent.replace(/\\s+/g," "),h:r.height/q,w:r.width/q,hit:!!h&&(h===e||e.contains(h))}}))'));
    const low = await R('JSON.stringify(G15.text(".gtoast").filter(function(x){return x.cr<4.5}))');
    SB.push({ dark, at, t, low });
  }
  await R('S.dark=false;S.cardCfg.mode="light";renderAll(true);""');
  ok('[stop bar] three choices, each 44 px tall and hit at its centre, its words 4.5:1, light and dark: "Continue the tour (step N of M)", "Start again", "What next?"',
    SB.every(s => s.t.every(x => x && x.h >= 43.5 && x.w >= 43.5 && x.hit) && /^Continue the tour \(step \d+ of \d+\)$/.test(s.t[0].t) && s.t[1].t === 'Start again' && s.t[2].t === 'What next?' && s.low === '[]'), SB);
  let at = await stopAt(); await p.click('#ph .gtoast .gtcont'); const c1 = await until(`G.on&&S.guideSteps[G.i].id==="${at.id}"`, 4000); await R('stopTour();""');
  at = await stopAt(); await p.click('#ph .gtoast .gtagain'); const c2 = await until('G.on&&G.i===0&&S.guideSteps[0].act==="teaser"', 4000); await R('stopTour();""');
  at = await stopAt(); const lab = await R('svLabel(svOffer()).replace(/\\s+/g," ")'); await p.click('#ph .gtoast .gtnext'); const c3 = await until('!!ph.querySelector(".tepanel")', 3000);
  const PZ = JSON.parse(await R('JSON.stringify({hd:((ph.querySelector(".te-hd")||{}).textContent||"").replace(/\\s+/g," "),first:((ph.querySelector(".te-acts button")||{}).textContent||"").replace(/\\s+/g," ")})'));
  const nOf = /\(step (\d+) of (\d+)\)/.exec(lab) || [];
  if (c3) { await settled(); await p.click('#ph .te-acts .te-cont'); }
  const c4 = c3 && await until(`G.on&&S.guideSteps[G.i].id==="${at.id}"`, 4000); await R('if(G.on)stopTour();""');
  ok('[stop bar] each does its job by a real tap: Continue goes back to the step; Start again to the teaser; What next? opens the panel "Tour paused · step N of M" with Continue first of its actions, which goes back to the step',
    c1 && c2 && c3 && PZ.hd === `Tour paused · step ${nOf[1]} of ${nOf[2]}` && PZ.first === lab && c4, { c1, c2, c3, PZ, lab, c4, eq: [PZ.hd === `Tour paused · step ${nOf[1]} of ${nOf[2]}`, PZ.first === lab] });
  tsec('stop bar');

  // ---------- 7. [editor] each new row by a real click: what the visitor sees, and its demo ----------
  await reset(); await R('S.ui.rowDemo=true;S.tab="guide";S.op={gt1:true,s7end:true};S.ui.level=9;renderEditor();""');
  const panelNow = () => R('JSON.stringify({open:G.te.open,panel:!!ph.querySelector(".tepanel"),pill:!!ph.querySelector(".tepill"),ask:!!G.askOpen,sc:G.te.sc&&G.te.sc.ref,qs:ph.querySelectorAll(".te-q").length,own:[].some.call(ph.querySelectorAll(".te-q"),function(x){return x.textContent==="Who lived in the palace?"}),chips:[].map.call(ph.querySelectorAll(".te-q"),function(x){return x.textContent}),secs:[].map.call(ph.querySelectorAll(".tepanel > div,.tepanel > form"),function(x){return x.className.split(" ")[0]}),near:[].map.call(ph.querySelectorAll(".te-card"),function(x){return x.dataset.tenear}),why0:(ph.querySelector(".te-card small")||{}).textContent||""})').then(JSON.parse);
  const demoEnd = () => until('(!G.on&&!!ph.querySelector(".tepanel,.tepill"))||G.askOpen', 25000).then(async g => { if (g) await settled().catch(() => 0); return g; });
  const ED = [];
  const edCase = async (name, act, check) => { await R('if(G.on)stopTour();G.te.open=false;teDraw();""'); await act(); const g = await demoEnd(); const v = await panelNow(); const good = g && check(v); ED.push({ name, good, g, v: good ? undefined : v }); await reset(); await R('renderAll(true);""'); };
  const seg = (k, v) => () => p.click(`#ed [data-set="@guideCfg.${k}"][data-v="${v}"]`);
  const sel = (k, v) => () => p.selectOption(`#ed select[data-sel="@guideCfg.${k}"]`, v);
  await edCase('What next? panel Off: Ask opens', seg('teOn', 'false'), v => v.ask && !v.panel);
  await edCase('Where the tour ends: The hero', sel('teWhere', 'hero'), v => v.sc === 'hero');
  await edCase('Where the tour ends: A block I pick, then The block: the Gallery', async () => { await sel('teWhere', 'pick')(); await demoEnd(); await R('if(G.on)stopTour();G.te.open=false;teDraw();""'); await p.selectOption('#ed select[data-sel="@guideCfg.teBlock.painting"]', 'b_gal'); }, v => v.sc === 'b:b_gal');
  await edCase('Start as Minimal', seg('teStart', 'min'), v => v.pill && !v.panel);
  for (const s of ['qs', 'ask', 'near', 'act']) await edCase(`${s} Off`, seg('teSec.' + s, 'false'), v => v.panel && v.secs.indexOf({ qs: 'te-qs', ask: 'te-ask', near: 'te-near', act: 'te-acts' }[s]) < 0);
  await edCase('How many 2', seg('teQsN', '2'), v => v.qs === 2);
  await edCase('Questions from: the templates only', seg('teQsSrc', 'tpl'), v => v.qs >= 2 && !v.own);
  await edCase('Nearby How many 5', seg('teNearN', '5'), v => v.near.length === 5);
  await edCase('Order: the nearest only', sel('teNearRule', 'd'), v => /^Nearby · /.test(v.why0));
  await edCase('This point\'s picks: the Tagus', sel('tePicks.painting.p1', 'river'), v => v.near[0] === 'river');
  await edCase('▼ on Suggested questions: Ask right away first', () => p.click('#ed [data-teord="qs|1"]'), v => v.secs.indexOf('te-ask') >= 0 && v.secs.indexOf('te-ask') < v.secs.indexOf('te-qs'));
  await edCase('a point\'s own question typed', async () => { await p.click('#ed [data-teqadd]'); const i = await p.$$('#ed [data-teqin^="painting|1|en"]'); await i[0].click(); await p.keyboard.type('What did the king eat?'); await p.click('#ed [data-teqplay="1"]'); },
    v => v.chips.indexOf('What did the king eat?') >= 0);
  await edCase('a template\'s words typed', async () => { await R('S.guideCfg.teQsSrc="tpl";renderEditor();""'); const i = await p.$('#ed [data-tx="te_tq_year|en"]'); await i.click({ clickCount: 3 }); await p.keyboard.type('Tell me what happened in {0}'); await p.keyboard.press('Tab'); await p.click('#ed [data-tetpl="te_tq_year"]'); },
    v => v.chips.indexOf('Tell me what happened in 1755') >= 0);
  await R('S.tx.te_tq_year=JSON.parse(TX0).te_tq_year;""');
  // the ▸ on each new row's label
  const keys = ['teOn', 'teWhere', 'teStart', 'teSec.qs', 'teSec.ask', 'teSec.near', 'teSec.act', 'teQsN', 'teQsSrc', 'teNearN', 'teNearRule', 'tePicks.painting.p1'], PL = [];
  await R('renderEditor();""');
  for (const k of keys) { await R('if(G.on)stopTour();G.te.open=false;teDraw();document.body.dataset.rowdemo="";""'); const lb = await p.$(`#ed .row:has([data-set="@guideCfg.${k}"],[data-sel="@guideCfg.${k}"]) > label.rd`);
    if (!lb) { PL.push({ k, lb: false }); continue; } await lb.click(); const g = await demoEnd(); const want = JSON.stringify('guideCfg.' + k + '|found|'); const rd = await until(`(function(){var d=document.body.dataset.rowdemo||"";return d.indexOf(${want})===0&&!/[|][.](gdock|gcap)$/.test(d)})()`, 3000); PL.push({ k, g, rd }); }
  ok('[editor] each new row changed by a real click changes what the visitor sees at the tour\'s end (its demo plays the end): ' + ED.map(x => x.name).join('; '), ED.every(x => x.good), { bad: ED.filter(x => !x.good) });
  ok('[editor] the ▸ on each new row\'s label plays the tour\'s end and rings the part it changes (' + keys.length + ' rows)', PL.every(x => x.g && x.rd), PL.filter(x => !(x.g && x.rd)));
  const nodemo = await R('JSON.stringify(rowsWithoutDemo(["s7end"]))');
  ok('[editor] no row of The tour\'s end without its demo (NO DEMO), the foldout in Guide › 2 · The tour after Ways in', nodemo === '[]' && await R('var a=ed.querySelector(\'[data-fo="gt:ways"]\'),b=ed.querySelector(\'[data-fo="s7end"]\');!!a&&!!b&&!!(a.compareDocumentPosition(b)&Node.DOCUMENT_POSITION_FOLLOWING)&&!!b.closest(\'[data-fo="gt1"]\')'), nodemo);
  await R('S.ui.rowDemo=false;S.tab="card";renderEditor();""'); tsec('editor');

  // ---------- 8. [reduced motion] ----------
  await R('S.reduce=true;renderAll(true);""'); await toEnd();
  const RM = JSON.parse(await R('JSON.stringify({p:getComputedStyle(ph.querySelector(".tepanel")).animationName,c:getComputedStyle(ph.querySelector(".te-card")).animationName,d:getComputedStyle(ph.querySelector(".te-card")).animationDelay,top:Math.abs(scr.scrollTop-G.te.scTop)<2})'));
  await p.click('#ph .te-card[data-tenear="torreao"]'); const inst = await R('S.m.poi==="torreao"&&S.stop==="half"'); await R('if(G.on)stopTour();S.reduce=false;""');
  ok('[reduced motion] a fade only: the panel and its cards fade (no rise, no slide, no stagger), the showcase reached at once, the wall at the point at once', RM.p === 'teFade' && RM.c === 'teFade' && RM.d === '0s' && RM.top && inst, { RM, inst });

  ok('no page errors', !errs.length, errs.slice(0, 5));
  tsec('end');
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS');
  await b.close();
})();
