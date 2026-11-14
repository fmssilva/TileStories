// test9z (Playwright + the installed Edge): node test9z.js -> PASS/FAIL per check. Guidance round 21: speech on screen and the panels
// (guide/highlight.js, guide/teleprompter.js, guide/panel-look.js, guide/dock.js's Open and ±10 s, guide/controls.js's seekBy,
// guidance/present.js's bubble that follows, guide/tour-end.js's mascot, guide/game-ask.js's after-the-quiz line and chip).
// [highlighter] one painter (hlGlide) lights the bubble, the teleprompter, the open panel and the block (a hook count per place, every
//   lit place drawn by it); the glide eased (the light sampled mid-glide lies between the two words, on the in-out curve, not linear); the
//   tail fades with distance and is gone past Trail; reduced motion: the light on the word at once, the tail kept.
// [panel] Ink glass's Open at most half of an 844 phone, its words at most 20 px, every target 44 px (elementFromPoint at its centre),
//   nothing cut (each control and label inside the panel, no row overflowing).
// [seek] forward 10 s and back 10 s (real taps) land where the tour's timeline says, across a step, the time shown there; the Mini's back
//   10 s too.
// [bubble] Words during a tour = The bubble: its tail on the mascot's head within 4 px on every frame while the mascot walks; a tip's
//   bubble on the wall goes with the mascot's hop (their offset kept within 4 px).
// [teleprompter] during a tour at Mini the words in the strip and in no bubble; the told block's words above its top (the arithmetic);
//   the line before fading up and out on a new line; Full script (a real tap) opens the whole text.
// [looks] Light glass, Aurora and Dark: every word of the Mini, the open panel and the teleprompter 4.5:1 or more over the glass composed on
//   the worst backdrop and its brightest light shape; Follows the theme: Light glass in light, Aurora in dark.
// [what next] the mascot at What next? covers nothing of the card (inside the panel).
// [quiz ask] during the quiz, a question about something else is answered (the stubbed answer's safe sentences) and, only when the answer
//   would give the quiz away, the warm after-the-quiz line; a question about the quiz keeps its hint; EN and PT; a real typed question.
// [chip] the card's quiz chip 8 px or more from its eyebrow's words, and clear of the mascot.
// [editor] each new row of Speech on screen changes what the visitor sees, and its ▸ (a real click on its label) plays it.
// MUTATE=1: the glide linear and instant, ±10 s does nothing, the bubble never follows, the teleprompter never shows, Follows the theme
//   always Dark, the panel's words 30 px, the mascot back on the panel's edge, the quiz's detection always "about", the chip never moved:
//   [highlighter] (glide), [seek], [bubble], [teleprompter], [looks] (theme), [panel], [what next], [quiz ask] and [chip] turn red (and with
//   them [highlighter]'s one painter, [looks]' contrast and [editor], which need the strip): 18 red of 22 (round 21).
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 1000 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url, { timeout: 150000 }); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 700) : '')); };
  const box = async sel => p.locator('#ph ' + sel).first().boundingBox({ timeout: 2500 }).catch(() => null);
  const tap = async sel => { const r = await box(sel); if (!r) return false; await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return true; };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  await R(require('./g15lib.js'));
  /* the checks' own arithmetic: alpha over, the overlap of two boxes, shown */
  await R('window.Z9={over:function(f,b){var a=f.a+b.a*(1-f.a);if(!a)return{r:0,g:0,b:0,a:0};return{r:(f.r*f.a+b.r*b.a*(1-f.a))/a,g:(f.g*f.a+b.g*b.a*(1-f.a))/a,b:(f.b*f.a+b.b*b.a*(1-f.a))/a,a:a}},inter:function(a,b){return Math.max(0,Math.min(a.r,b.r)-Math.max(a.l,b.l))*Math.max(0,Math.min(a.b,b.b)-Math.max(a.t,b.t))},' +
    'shown:function(e){if(!e||!e.isConnected)return false;var r=e.getBoundingClientRect();if(r.width<1||r.height<1)return false;for(var x=e;x&&x!==document.body;x=x.parentElement){var c=getComputedStyle(x);if(c.display==="none"||c.visibility==="hidden"||+c.opacity<.05)return false}return true}};""');
  if (process.env.MUTATE) {
    await R('hlEase=function(){return 1};hlMs=function(){return 0};seekBy=function(){return null};gbFollow=function(){G.gbRaf=0};gdBubFollow=function(){};teleWant=function(){return false};pSkin=function(){return"dark"};gaAbout=function(){return{about:true,shared:[],opt:0,n:0}};gaChipClear=function(){};' +
      'var __m=document.createElement("style");__m.textContent="#ph .lk-ink .gd-tp .tp-now{font-size:30px!important;line-height:38px!important}#ph .teazu{translate:0 -64px!important}";document.head.appendChild(__m);""');
    console.log('MUTATION: the glide instant, ±10 s dead, the bubble and the tip never follow, no teleprompter, the theme always Dark, the words 30 px, the mascot on the panel\'s edge, every question "about" the quiz, the chip never moved');
  }
  /* the state every check starts from: a 390 x 844 phone, the device voice muted (its words go on by time, 353 ms a word), quiet guidance */
  const base = (o) => `SCREENS.phone=[390,844];if(G.on)stopTour();if(G.gd.pages)gdPagesEnd("test",false);gdBeatClose();gdStop("test");if(G.ga.on)gaEnd("test");if(G.ga.card)gaCardLeave();G.te.open=false;
    S.hints.on=false;S.previewBar=false;S.lang="${(o && o.lang) || 'en'}";S.guideMute=true;S.introSeen=true;S.reduce=${!!(o && o.reduce)};S.dark=${!!(o && o.dark)};S.cardCfg.mode="${o && o.dark ? 'dark' : 'light'}";
    S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.miniCtl="pause";S.guideCfg.captions=true;S.guideCfg.pWords="prompter";S.guideCfg.look="ink";S.guideCfg.speed=1;S.vis.speed=null;
    S.guideCfg.music=false;S.vis.music=null;S.ui.rowDemo=false;S.ui.gdSR=false;S.guideCfg.mascot=true;S.guideCfg.hl="comet";S.guideCfg.trail=6;S.guideCfg.glide=true;S.guideCfg.highlight=true;
    S.guideCfg.tourWords="${(o && o.words) || 'tele'}";S.guideCfg.teleH=184;S.guideCfg.pSkin="${(o && o.skin) || 'theme'}";S.guideCfg.bubbleOut="beside";S.guideCfg.ask="search";sampleFn=null;S.agentCfg.voiceOut=false;
    S.guidance.on=true;S.guidance.kinds.forEach(function(k){k.on=false});G.gd.manual=true;G.gd.demo=false;gdChip(null);S.view="card";S.stop="full";renderAll(false);""`;
  const tourAt = (id, h) => R(`startTour({id:"${id}"});S.dockH="${h}";renderGuide();""`);
  const HLP = '(function(){var o={};ph.querySelectorAll("[data-hlp]").forEach(function(e){o[e.dataset.hlp]=(o[e.dataset.hlp]||0)+1});return JSON.stringify(o)})()';

  // ---------- [highlighter] ----------
  await R(base()); await R('HL.hooks={};""'); await tourAt('g4', 'mini'); await until('!!(G.hl&&G.hl.target&&G.hl.k>=2)', 6000); await W(300);
  const h1 = JSON.parse(await R('JSON.stringify({hooks:HL.hooks,tele:!!ph.querySelector(".gtele .tl-now.hl-box"),block:!!(G.hl.target&&G.hl.target.classList.contains("hl-box")&&G.hl.target.querySelector(":scope > .hl-quad"))})'));
  await R('S.dockH="peek";renderGuide();""'); await W(900);
  const h2 = JSON.parse(await R('JSON.stringify({hooks:HL.hooks,panel:!!ph.querySelector(".gdock .tp-now.hl-box > .hl-quad")})'));
  await R(base({ words: 'bubble' })); await R('HL.hooks.bubble=0;""'); await tourAt('g1', 'mini'); await until('!!ph.querySelector(".gbubble .gcap.hl-box > .hl-quad")', 6000);
  const h3 = JSON.parse(await R('JSON.stringify({hooks:HL.hooks,places:' + HLP + ',old:typeof hlPaint})'));
  ok('[highlighter] one painter: the block, the teleprompter, the open panel and the bubble each lit by hlGlide (its hook counted per place, its light shape in each); no other painter left (hlPaint gone)',
    h1.hooks.block > 0 && h1.hooks.prompter > 0 && h1.block && h1.tele && h2.hooks.panel > 0 && h2.panel && h3.hooks.bubble > 0 && h3.old === 'undefined', { h1, h2, h3 });

  /* the glide: frames sampled in the page while the voice goes from word to word (the block being read) */
  await R(base()); await tourAt('g4', 'mini'); await until('!!(G.hl&&G.hl.target&&G.hl.k>=1)', 6000);
  const gl = JSON.parse(await R(`new Promise(function(res){var out=[],t0=performance.now();(function f(){var b=G.hl&&G.hl.target,st=b&&b._hl;if(st&&st.cur&&st.from&&st.to&&st.dur>0)out.push({k:st.k,fr:st.fr,e:st.e,x:st.cur.x,fx:st.from.x,tx:st.to.x,y:st.cur.y,fy:st.from.y,ty:st.to.y});if(performance.now()-t0<2600)requestAnimationFrame(f);else res(JSON.stringify(out))})()})`));
  /* mid-glide on one line: the light between the two words, its share of the way the in-out curve of the time's share (not linear) */
  const mid = gl.filter(s => s.fr > 0.12 && s.fr < 0.88 && Math.abs(s.fy - s.ty) < 2 && Math.abs(s.tx - s.fx) > 8);
  const between = mid.filter(s => (s.x - s.fx) * (s.tx - s.x) > 0);
  const curve = mid.filter(s => { const r = (s.x - s.fx) / (s.tx - s.fx), e = s.fr < .5 ? 4 * s.fr ** 3 : 1 - Math.pow(-2 * s.fr + 2, 3) / 2; return Math.abs(r - e) < 0.02; });
  const notLinear = mid.some(s => Math.abs((s.x - s.fx) / (s.tx - s.fx) - s.fr) > 0.05);
  ok('[highlighter] the glide: sampled mid-way between two words of a line the light lies between them, on the in-out ease (not linear, no jump)',
    mid.length >= 2 && between.length === mid.length && curve.length === mid.length && notLinear, { n: gl.length, mid: mid.slice(0, 3), between: between.length, curve: curve.length });
  const tail = JSON.parse(await R('var b=G.hl.target,ws=b.querySelectorAll(".gw"),k=b._hl.k,t=[];for(var j=k;j>=Math.max(0,k-9);j--)t.push(+(ws[j].style.getPropertyValue("--t")||0));JSON.stringify({k:k,t:t,trail:S.guideCfg.trail})'));
  const tt = tail.t.slice(1);
  ok('[highlighter] the comet\'s tail: the words just said tinted, fading with distance, gone past Trail (6) words',
    tt.length >= 3 && tt[0] > tt[2] && tt.every((v, i) => i === 0 || v <= tt[i - 1] + 1e-6) && (tt.length < 8 || tt[7] === 0), tail);
  await R(base({ reduce: true })); await tourAt('g4', 'mini'); await until('!!(G.hl&&G.hl.target&&G.hl.k>=2)', 6000);
  const rm = JSON.parse(await R(`new Promise(function(res){var b=G.hl.target,k0=b._hl.k,t0=performance.now();(function f(){var st=b._hl;if(st&&st.k!==k0&&st.k>0){var ws=b.querySelectorAll(".gw"),to=hlRectOf(b,ws[st.k]);res(JSON.stringify({dur:st.dur,dx:Math.abs(st.cur.x-to.x),dy:Math.abs(st.cur.y-to.y),tail:+(ws[st.k-1].style.getPropertyValue("--t")||0)}));return}if(performance.now()-t0<3000)requestAnimationFrame(f);else res("{}")})()})`));
  ok('[highlighter] reduced motion: no glide (the light on the new word in the frame it is said), the tail kept', rm.dur === 0 && rm.dx < 0.5 && rm.dy < 0.5 && rm.tail > 0, rm);
  tsec('highlighter');

  // ---------- [panel] ----------
  const panelOf = async (o) => { await R(base(o)); await tourAt('g4', 'peek'); await W(900); return JSON.parse(await R(`(function(){var d=ph.querySelector(".gdock"),P=ph.getBoundingClientRect(),q=P.height/ph.offsetHeight,R0=function(e){var r=e.getBoundingClientRect();return{l:(r.left-P.left)/q,t:(r.top-P.top)/q,r:(r.right-P.left)/q,b:(r.bottom-P.top)/q,w:r.width/q,h:r.height/q}},D=R0(d);
    var fonts=[].map.call(d.querySelectorAll(".gd-tp p,.gm-now"),function(e){return parseFloat(getComputedStyle(e).fontSize)}),small=[],cut=[];
    d.querySelectorAll("button,[role=slider],.gd-form").forEach(function(e){if(e.closest(".gd-chips"))return;var r=R0(e);if(!r.w||!r.h)return;var hit=e.classList.contains("gd-seek")?r.h:Math.min(r.w,r.h);var cx=e.getBoundingClientRect(),el=document.elementFromPoint(cx.left+cx.width/2,cx.top+cx.height/2);if(hit<44-0.5||!(el===e||e.contains(el)||(el&&el.closest&&el.closest("button")===e)))small.push([e.className.baseVal!==undefined?"svg":e.className,Math.round(r.w),Math.round(r.h),el&&el.className]);if(e.classList.contains("gd-handle")||e.classList.contains("gd-small"))return;if(r.l<D.l-0.5||r.r>D.r+0.5||r.b>D.b+0.5||r.t<D.t-0.5)cut.push([e.className,r])});
    [".gd-head",".gd-ctl",".gd-hrow"].forEach(function(s){var e=d.querySelector(s);if(e&&e.scrollWidth>e.clientWidth+1)cut.push([s,"overflows",e.scrollWidth,e.clientWidth])});
    var ch=d.querySelector(".gd-chips");return JSON.stringify({h:Math.round(D.h),top:Math.round(D.t),H:ph.offsetHeight,fonts:fonts,small:small,cut:cut,chipsScroll:!ch||getComputedStyle(ch).overflowX==="auto",skip:d.querySelectorAll(".gd-ctl .gd-skip").length})})()`)); };
  const pl = await panelOf(), pd = await panelOf({ dark: true });
  ok('[panel] Ink glass Open at most half of the 844 phone (with room), its spoken words at most 20 px (they were 22), back and forward 10 s in its one row of controls',
    [pl, pd].every(x => x.h <= 422 && x.fonts.length && Math.max(...x.fonts) <= 20 && x.skip === 2), { light: [pl.h, pl.fonts, pl.skip], dark: [pd.h, pd.fonts] });
  ok('[panel] every target 44 px or more and the one under its centre; nothing cut (each control inside the panel, no row overflowing; the questions scroll sideways)',
    [pl, pd].every(x => !x.small.length && !x.cut.length && x.chipsScroll), { small: pl.small.concat(pd.small).slice(0, 6), cut: pl.cut.concat(pd.cut).slice(0, 6) });
  tsec('panel');

  // ---------- [seek] ----------
  await R(base()); await tourAt('g2', 'peek'); await W(500);
  /* put the voice 3 s before the end of its step (a seek from outside the controls), then forward 10 s crosses into the next step */
  const prep = JSON.parse(await R('var T=dkTimes(),c=T.stops[G.i],L=seekLand(c.at+c.len-3.2);stopSpeech();G.startWord=L.k;runStep();JSON.stringify({i:G.i,at:c.at,len:c.len,k:L.k})')); await W(700);
  const s0 = JSON.parse(await R('JSON.stringify({i:G.i,el:dkElapsed(dkTimes())})'));
  await tap('.gdock .gd-ctl [data-g="fwd10"]'); await W(700);
  const s1 = JSON.parse(await R('JSON.stringify({i:G.i,to:G.seekedTo,el:dkElapsed(dkTimes()),shown:(ph.querySelector(".gdock .gd-time[data-t=el]")||{}).textContent})'));
  await tap('.gdock .gd-ctl [data-g="back10"]'); await W(700);
  const s2 = JSON.parse(await R('JSON.stringify({i:G.i,to:G.seekedTo,el:dkElapsed(dkTimes()),shown:(ph.querySelector(".gdock .gd-time[data-t=el]")||{}).textContent})'));
  const mm = s => { const a = String(s || '').split(':'); return +a[0] * 60 + +a[1]; };
  ok('[seek] forward 10 s (a real tap) lands 10 s on along the tour\'s timeline, in the next step, the time shown there; back 10 s lands back in the step before',
    s1.to && Math.abs(s1.to.t - (s0.el + 10)) < 0.8 && s1.i === s0.i + 1 && Math.abs(s1.el - s1.to.t) < 1.2 && Math.abs(mm(s1.shown) - s1.to.t) <= 1.5 &&
    s2.to && Math.abs(s2.to.t - (s1.el - 10)) < 0.8 && s2.i === s0.i && Math.abs(s2.el - s2.to.t) < 1.2, { prep, s0, s1, s2 });
  await R('S.dockH="mini";renderGuide();""'); await W(600); const m0 = await R('dkElapsed(dkTimes())');
  const mb = await tap('.gdock .gd-mini [data-g="back10"]'); await W(250); const m1 = JSON.parse(await R('JSON.stringify({to:G.seekedTo,el:dkElapsed(dkTimes()),h:S.dockH,line:ph.querySelectorAll(".gdock.h-mini .gd-mrow").length})'));
  ok('[seek] the Mini\'s back 10 s (a real tap): 10 s back on the timeline, the Mini still one row', mb && m1.to && Math.abs(m1.to.t - Math.max(0, m0 - 10)) < 0.8 && Math.abs(m1.el - m1.to.t) < 1.2 && m1.h === 'mini' && m1.line === 1, { m0, m1 });
  tsec('seek');

  // ---------- [bubble] ----------
  /* the mascot walks from the hero to the next step's block while it says its own words (the bubble by its head) */
  await R(base({ words: 'bubble' })); await tourAt('g1', 'mini'); await until('!!ph.querySelector(".gbubble.gb-head")', 6000); await W(600);
  const bw = JSON.parse(await R(`new Promise(function(res){var out=[],t0=performance.now(),j=S.guideSteps.findIndex(function(s){return s.id==="g2"});stopSpeech();G.paused=false;G.i=j;runStep();
    (function f(){var m=ph.querySelector(".mascot"),tip=gbTailTip(),b=ph.querySelector(".gbubble.gb-head");if(m&&tip&&b&&m.getBoundingClientRect().width>10){var h=gbHead(m),dx=Math.max(h.l-tip.x,0,tip.x-h.r),dy=Math.max(h.t-tip.y,0,tip.y-h.b);out.push({d:Math.round(Math.hypot(dx,dy)*10)/10,mt:Math.round(phRect(m).t)})}if(performance.now()-t0<1800)requestAnimationFrame(f);else res(JSON.stringify(out))})()})`));
  const bmax = Math.max(...bw.map(x => x.d)), moved = bw.length ? Math.max(...bw.map(x => x.mt)) - Math.min(...bw.map(x => x.mt)) : 0;
  ok('[bubble] Words during a tour = The bubble: on every frame while the mascot walks to the next step\'s block, the bubble\'s tail is on its head (within 4 px)', bw.length >= 20 && moved > 40 && bmax <= 4, { frames: bw.length, moved, worst: bmax, over4: bw.filter(x => x.d > 4).slice(0, 5) });
  await R(base()); await R('setWall("ribeira");S.camZoom=1;S.view="card";S.stop="closed";renderAll(false);S.guidance.kinds.forEach(function(k){k.on=k.id==="app"});""'); await W(500);
  const hop = JSON.parse(await R(`new Promise(function(res){gdStart(gdItemNow("tapmarker"),"test",{pull:true,ms:60000});var out=[],t0=performance.now();
    (function f(){var az=ph.querySelector(".azfab"),b=ph.querySelector(".gdbub:not(.gd-out)"),tl=b&&b.querySelector(".gdtail");if(b&&az&&tl&&az.getBoundingClientRect().width>10){var A=gdPR(az),B=gdPR(b),T=gdPR(tl),side=b.dataset.tail,
        tip=side==="left"?{x:T.l,y:(T.t+T.b)/2}:side==="right"?{x:T.r,y:(T.t+T.b)/2}:side==="down"?{x:(T.l+T.r)/2,y:T.b}:{x:(T.l+T.r)/2,y:T.t},h=gdHeadOf(gdFigure(az)||A),dx=Math.max(h.l-tip.x,0,tip.x-h.r),dy=Math.max(h.t-tip.y,0,tip.y-h.b);
        out.push({d:Math.round(Math.hypot(dx,dy)*10)/10,ax:A.l,ay:A.t,bl:B.l,bt:B.t,bw:B.r-B.l,bh:B.b-B.t,side:side})}if(performance.now()-t0<900)requestAnimationFrame(f);else res(JSON.stringify(out))})()})`));
  const hl = hop[hop.length - 1] || {}, inside = s => s.bl > 12.5 && s.bt > 44.5 && s.bl + s.bw < 390 - 12.5 && s.bt + s.bh < 844 - 12.5;
  const hopMoved = hop.length ? Math.hypot(hop[0].ax - hl.ax, hop[0].ay - hl.ay) : 0, ins = hop.filter(inside), dmax = ins.length ? Math.max(...ins.map(s => s.d)) : null;
  ok('[bubble] a tip\'s bubble on the wall goes with the mascot\'s hop: its tail on the mascot\'s head within 4 px on every frame (inside the phone)', hop.length >= 10 && hopMoved > 20 && ins.length >= 5 && dmax <= 4, { frames: hop.length, inside: ins.length, hopMoved: Math.round(hopMoved), worst: dmax, rest: hl.d, over4: ins.filter(s => s.d > 4).slice(0, 4) });
  await R('gdStop("test");S.guidance.kinds.forEach(function(k){k.on=false});""');
  tsec('bubble');

  // ---------- [teleprompter] ----------
  await R(base()); await tourAt('g4', 'mini'); await until('!!ph.querySelector(".gtele .tl-now .hl-on")', 6000); await W(1600);
  const tp = JSON.parse(await R(`(function(){var t=ph.querySelector(".gtele");if(!t)return JSON.stringify({tele:false,bubble:!!ph.querySelector(".gbubble")});var st=S.guideSteps[G.i],el=stepEl(st),P=ph.getBoundingClientRect(),q=P.height/ph.offsetHeight,tt=(t.getBoundingClientRect().top-P.top)/q,
    bx=wordBoxes(el).filter(function(b){var r=el.getBoundingClientRect();return b.t>=(r.top-P.top)/q-1&&b.b<=(r.bottom-P.top)/q+1}),low=bx.length?Math.max.apply(null,bx.map(function(b){return b.b})):null,
    now=t.querySelector(".tl-now").textContent.replace(/\\s+/g," ").trim(),k=G.hl.k,w=G.words[k]&&G.words[k].w;return JSON.stringify({tele:!!t,bubble:!!ph.querySelector(".gbubble"),top:Math.round(tt),low:low,now:now,word:w,has:now.indexOf(w)>=0,h:Math.round(t.getBoundingClientRect().height/q)})})()`));
  ok('[teleprompter] during a tour at Mini the words are in the strip (the word being said in its line), in no bubble; the strip 184 px from the screen\'s foot',
    tp.tele && !tp.bubble && tp.has && Math.abs(tp.h - 184) <= 1, tp);
  ok('[teleprompter] the told block\'s words all above the strip\'s top (the arithmetic: its lowest word line vs the strip)', tp.low != null && tp.low <= tp.top, { low: tp.low, top: tp.top });
  const fade = JSON.parse(await R(`new Promise(function(res){var t=ph.querySelector(".gtele");if(!t){res(JSON.stringify({o:[],i:[],prev:""}));return}var i0=t._i,out=[],t0=performance.now();(function f(){var pv=t.querySelector(".tl-lines.tl-up .tl-prev");if(t._i!==i0&&pv)out.push(+getComputedStyle(pv).opacity);if(performance.now()-t0<9000&&!(out.length&&performance.now()-t0>0&&out.length>40))requestAnimationFrame(f);else res(JSON.stringify({o:out,i:[i0,t._i],prev:(t.querySelector(".tl-prev")||{}).textContent}))})()})`));
  const fo = fade.o;
  ok('[teleprompter] a new line slides up: the line before fades up and out (its opacity falls from about 1 to 0)', fo.length >= 5 && fo[0] > 0.5 && fo[fo.length - 1] < 0.1 && fade.prev && fade.prev.trim().length > 0, { first: fo[0], last: fo[fo.length - 1], n: fo.length, prev: fade.prev });
  const full = await tap('.gtele .tl-full'); await W(500);
  const fs = JSON.parse(await R('JSON.stringify({h:S.dockH,thread:!!ph.querySelector(".gdock .gd-thread"),tele:!!ph.querySelector(".gtele:not(.tl-out)")})'));
  ok('[teleprompter] Full script › (a real tap) opens the open panel\'s whole text; the strip steps aside', full && fs.h === 'full' && fs.thread && !fs.tele, fs);
  tsec('teleprompter');

  // ---------- [looks] ----------
  const looks = [];
  for (const skin of ['light', 'aurora', 'dark']) for (const dark of [false, true]) for (const h of ['mini', 'peek']) {
    await R(base({ skin, dark })); await tourAt('g4', h); await W(700);
    looks.push(Object.assign({ skin, dark, h }, JSON.parse(await R(`(function(){var C=G15.col,ov=Z9.over,cr=G15.cr,res=[];
      [[".gdock",".gdock .tp-now .tw,.gdock .tp-prev .tw,.gdock .gd-eye,.gdock .gd-time,.gdock .gd-chips [data-chip],.gdock .gd-aa,.gdock .gd-speed,.gdock .gin"],[".gtele",".gtele .tl-now .tw,.gtele .tl-who,.gtele .tl-full"]].forEach(function(g){var host=ph.querySelector(g[0]);if(!host)return;var cs=getComputedStyle(host),sf=C(cs.getPropertyValue("--sksurf"))||C(cs.backgroundColor),gl=C(cs.getPropertyValue("--skglow"))||{r:0,g:0,b:0,a:0};
        ph.querySelectorAll(g[1]).forEach(function(e){if(!Z9.shown(e))return;var fc=C(getComputedStyle(e).color);if(!fc)return;var bg0=C(getComputedStyle(e).backgroundColor);
          [{r:0,g:0,b:0,a:1},{r:255,g:255,b:255,a:1}].forEach(function(back){var bg=ov(gl,ov(sf,back));if(bg0&&bg0.a>0)bg=ov(bg0,bg);res.push({s:g[0]+" "+(e.className||e.tagName),c:+cr(ov(fc,bg),bg).toFixed(2)})})})});
      var low=res.slice().sort(function(a,b){return a.c-b.c})[0];return JSON.stringify({n:res.length,low:low,cls:(ph.querySelector(".gdock")||{}).className})})()`)))); }
  const bad = looks.filter(x => !x.n || !x.low || x.low.c < 4.5);
  ok('[looks] Light glass, Aurora and Dark, light and dark, Mini and Open with the teleprompter: every word 4.5:1 or more over the glass on the worst backdrop and its brightest light shape',
    looks.length === 12 && !bad.length, { worst: looks.map(x => [x.skin, x.dark ? 'd' : 'l', x.h, x.low && x.low.c, x.low && x.low.s]).sort((a, b) => a[3] - b[3]).slice(0, 4), bad: bad.slice(0, 3) });
  await R(base({ skin: 'theme' })); await tourAt('g4', 'mini'); await W(400); const th1 = await R('(ph.querySelector(".gdock")||{}).className+" | "+(ph.querySelector(".gtele")||{}).className');
  await R(base({ skin: 'theme', dark: true })); await tourAt('g4', 'mini'); await W(400); const th2 = await R('(ph.querySelector(".gdock")||{}).className+" | "+(ph.querySelector(".gtele")||{}).className');
  ok('[looks] Follows the theme (the default): Light glass in light, Aurora in dark, the Mini and the teleprompter together', /sk-light/.test(th1) && (th1.match(/sk-light/g) || []).length === 2 && (th2.match(/sk-aurora/g) || []).length === 2 && await R('S.guideCfg.pSkin==="theme"||true'), { th1, th2 });
  tsec('looks');

  // ---------- [what next] ----------
  await R(base()); await R('startTour();stopSpeech();G.paused=false;G.i=S.guideSteps.length-1;runStep();""'); await until('!!ph.querySelector(".tepanel")&&!!ph.querySelector(".teazu")', 25000); await W(1800);
  const wn = JSON.parse(await R(`(function(){var a=ph.querySelector(".teazu"),pn=ph.querySelector(".tepanel"),f=gdFigure(a)||gdPR(a),P=gdPR(pn),over=0;
    scr.querySelectorAll(".item,[data-ref]").forEach(function(e){var r=gdPR(e);if(!r||r.b<=r.t)return;over=Math.max(over,Z9.inter({l:f.l,t:f.t,r:f.r,b:f.b},{l:r.l,t:r.t,r:r.r,b:Math.min(r.b,P.t)}))});
    return JSON.stringify({fig:[Math.round(f.t),Math.round(f.b)],panel:Math.round(P.t),over:Math.round(over),inside:f.t>=P.t-0.5})})()`));
  ok('[what next] the mascot sits inside the panel\'s top corner: its drawing covers nothing of the card (0 px² over the card\'s blocks above the panel)', wn.inside && wn.over === 0, wn);
  tsec('what next');

  // ---------- [quiz ask] ----------
  const QZ = async (lang) => { await R(base({ lang })); await R('S.guidance.kinds.forEach(function(k){k.on=true});gdKindById("app").on=false;gdKindById("content").on=false;gdGroupById("play").on=false;["quizpromise","quizfinale","guessyear","dontsay","findwall","water","cookie"].forEach(function(id){S.guidance.items[id].on=id==="quizpromise"||id==="quizfinale"});G.gd.plan=null;gdResetVisitor();startTour();G.gd.plan.quiz="on";gdPlanBeat("quizpromise").state="played";gd12Seam("g8");""'); return until('!!ph.querySelector(".gdbeat .bt-choice")&&G.gd.bt.lineSeq>0', 15000); };
  const qa = {};
  for (const lang of ['en', 'pt']) {
    const up = await QZ(lang);
    qa[lang] = JSON.parse(await R(`(function(){var g=gaGuarded(),k=gaKeys(g),right=g.opts[g.ok],en=S.lang==="en",other=en?"Who lived here?":"Quem viveu aqui?",about=g.q;
      var safe=en?"Kings and queens held court here, by the Tagus.":"Reis e rainhas tiveram aqui a sua corte, junto ao Tejo.",bad=(en?"It was ":"Foi ")+right+".";
      var a1=pipe("answer.text",bad+" "+safe,other),a2=pipe("answer.text",bad,other),a3=pipe("answer.text",bad,about),a4=pipe("answer.text",safe,other);
      return JSON.stringify({q:g.q,right:right,a1:a1,a2:a2,a3:a3,a4:a4,spoil:TXT("ga_spoil"),more:TXT("ga_spoil_more"),hint:gaWhereLine(g)})})()`));
    qa[lang].up = up;
  }
  const qok = l => { const x = qa[l], safe = l === 'en' ? 'Kings and queens held court here, by the Tagus.' : 'Reis e rainhas tiveram aqui a sua corte, junto ao Tejo.'; return x.up && x.a1 === safe + ' ' + x.more && x.a2 === x.spoil && x.a3 === x.hint && x.a3 !== x.spoil && x.a4 === safe; };
  ok('[quiz ask] during the quiz, a question about something else (stubbed answers): its safe sentences said and the warm "after the quiz" line only when the answer would give it away; nothing to say: "I\'d love to, but that would give our quiz away…"; a safe answer untouched; a question about the quiz keeps its hint; EN and PT',
    qok('en') && qok('pt') && qa.en.spoil === "I'd love to, but that would give our quiz away. Ask me again once you've answered!", qa);
  await QZ('en'); await R('S.dockH="peek";renderGuide();""'); await until('!!ph.querySelector("#gq")&&ph.querySelector("#gq").getClientRects().length>0', 3000);
  await p.click('#ph #gq'); await p.keyboard.type('Who lived here?'); await p.keyboard.press('Enter'); await until('!G.busy&&G.answer&&G.answer!==TXT("guide_thinking")', 6000);
  const typed = JSON.parse(await R('var g=gaGuarded()||G.ga.g;JSON.stringify({a:G.answer,hint:g?gaWhereLine(g):"",right:g?g.opts[g.ok]:"",on:G.ga.on})'));
  ok('[quiz ask] typed during the quiz (a real typed question, the offline answer): "Who lived here?" is answered, never with the hint\'s "Have a look at …", never with the quiz\'s answer',
    typed.a && typed.a !== typed.hint && !/Have a look at/.test(typed.a) && typed.a.indexOf(typed.right) < 0, typed);
  await R('if(G.ga.on)gaEnd("test");if(G.on)stopTour();""');
  tsec('quiz ask');

  // ---------- [chip] ----------
  await R(base()); await R('S.view="card";S.stop="full";renderPhone(false);gaStart(svPoi(),"b_quiz");var q=scr.querySelector(\'[data-ref="b:b_quiz"]\');scr.scrollTop+=q.getBoundingClientRect().top-scr.getBoundingClientRect().top-170;""');
  await until('!!ph.querySelector(".gbtn-wrap .gbtn")', 4000); await W(300); await tap('.gbtn-wrap .gbtn'); await until('!!ph.querySelector(".gbmenu")', 2000); await tap('.gbmenu [data-gbgo="ask"]'); await until('!!ph.querySelector(".kq > .ga-chip")', 3000); await W(700);
  const chip = JSON.parse(await R(`(function(){var c=ph.querySelector(".kq > .ga-chip");if(!c)return"{}";var host=c.parentNode,ey=gaEyebrow(host),A=gdPR(c),m=ph.querySelector(".mascot")||ph.querySelector(".azfab"),M=m?(gdFigure(m)||gdPR(m)):null;
    return JSON.stringify({gap:Math.round(gaChipGap(c,host)*10)/10,ey:ey?ey.textContent:"",cls:c.className,h:Math.round(A.b-A.t),w:Math.round(A.r-A.l),mascot:M?Math.round(Z9.inter(A,M)):0})})()`));
  ok('[chip] the card\'s quiz chip 8 px or more from its eyebrow\'s words ("… OF 3"), 44 px, and clear of the mascot', chip.gap >= 8 && chip.h >= 44 && chip.w >= 44 && chip.mascot === 0 && /OF 3/i.test(chip.ey), chip);
  await R('if(G.ga.on)gaEnd("test");if(G.ga.card)gaCardLeave();""');
  tsec('chip');

  // ---------- [editor] ----------
  await R(base()); await R('S.tab="guide";S.op={};S.op.s7mascot=true;S.op["gt:speech"]=true;S.ui.level=9;renderEditor();""'); await W(400);
  const rowEl = lab => p.locator('#ed [data-fo="gt:speech"] .row', { has: p.locator(`label:text-is("${lab}")`) }).first();
  const pick = async (lab, txt) => { const r = rowEl(lab); await r.scrollIntoViewIfNeeded(); await r.locator('button', { hasText: txt }).first().click(); await W(250); };
  const demo = async lab => { const r = rowEl(lab); await r.scrollIntoViewIfNeeded(); await r.locator('label.rd').first().click(); await W(2200); return R('document.body.dataset.rowdemo||""'); };
  const ed = {};
  ed.rows = await R('JSON.stringify(["Highlight style","Trail","Glide","Words during a tour","Its height","A tip\'s bubble","The panels\' look"].map(function(l){var f=ed.querySelector(\'[data-fo="gt:speech"]\');return!!f&&[].some.call(f.querySelectorAll(".row > label"),function(x){return x.firstChild&&x.firstChild.textContent===l})}))').then(JSON.parse);
  await pick('Words during a tour', 'The bubble'); ed.bub = await R('S.guideCfg.tourWords'); ed.bubDemo = await demo('Words during a tour');
  await until('!!ph.querySelector(".gbubble")||!!ph.querySelector(".mascot")', 4000); ed.bubSeen = await R('[!!ph.querySelector(".gbubble"),!!ph.querySelector(".gtele:not(.tl-out)")]');
  await pick('Words during a tour', 'Teleprompter'); ed.teleDemo = await demo('Words during a tour'); ed.teleSeen = await R('!!ph.querySelector(".gtele:not(.tl-out)")');
  await R('S.guideCfg.teleH=150;renderEditor();""'); ed.hDemo = await demo('Its height'); ed.hSeen = await R('var t=ph.querySelector(".gtele:not(.tl-out)");t?Math.round(t.getBoundingClientRect().height/(ph.getBoundingClientRect().height/ph.offsetHeight)):0');
  await pick('The panels\' look', 'Aurora'); ed.skDemo = await demo('The panels\' look'); ed.skSeen = await R('(ph.querySelector(".gdock")||{}).className||""');
  await pick('A tip\'s bubble', 'Above the mascot'); ed.boDemo = await demo('A tip\'s bubble'); ed.boSeen = await R('var b=ph.querySelector(".gdbub");b?b.dataset.tail:""');
  ok('[editor] Speech on screen holds the highlighter\'s rows and the new ones; each changes what the visitor sees (the bubble instead of the strip, the strip again, its height 150 px, the Aurora panel, the tip\'s bubble above the mascot) and its ▸ plays it',
    ed.rows.every(Boolean) && ed.bub === 'bubble' && /found/.test(ed.bubDemo) && ed.bubSeen[1] === false && /found/.test(ed.teleDemo) && ed.teleSeen && /found/.test(ed.hDemo) && Math.abs(ed.hSeen - 150) <= 1 && /found/.test(ed.skDemo) && /sk-aurora/.test(ed.skSeen) && /found/.test(ed.boDemo) && ed.boSeen === 'down', ed);
  await R('S.guideCfg.teleH=184;S.guideCfg.pSkin="theme";S.guideCfg.bubbleOut="beside";S.guideCfg.tourWords="tele";if(G.on)stopTour();gdStop("test");""');
  tsec('editor');

  ok('No page errors', errs.length === 0, errs.slice(0, 5));
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); await b.close(); process.exit(fails ? 1 : 0);
})();
