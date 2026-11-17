// test10e (Playwright + the installed Edge): node test10e.js -> PASS/FAIL per check. Merge round P1a.2: W's four devices
// (screen/device.js) by real clicks on the device buttons, each with the card's stops and the guide.
// [buttons] the phone button pressed on the phone; a second click turns it (portrait <-> landscape); the tablet's the same;
// [size] each device's logical size (390 x 844, 844 x 390, 820 x 1180, 1180 x 820), its frame scaled to fit the window (never
//   resized), the class land / tablet; [stops] the card's stops by the test's own arithmetic: portrait Peek H - 64, Half
//   H x (1 - Half max), Full 0, closed below the screen; [land] landscape: a full-height side sheet at the right (360 px) at Peek,
//   Half and Full, hidden when closed, no grabber; [guide] the tour from the hero's Start: its dock inside the phone, the card too.
// MUTATE=1: landscape never recognised (devLand false): the checks named [land] turn red.
const { chromium, labUrl } = require('./pw');
const url = labUrl('#probe');
const DEV = { phone_p: [390, 844], phone_l: [844, 390], tablet_p: [820, 1180], tablet_l: [1180, 820] };
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1600, height: 1000 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url, { timeout: 90000 }); await p.waitForFunction(() => window.LAB && window.LAB.run, null, { timeout: 60000 }); await p.waitForTimeout(600);
  const R = src => p.evaluate(s => LAB.run(s), src), W = ms => p.waitForTimeout(ms), T0 = Date.now();
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('devLand=function(){return false};""'); console.log('MUTATION: landscape never recognised'); }
  const reset = 'if(G.on)stopTour();if(G.gd.pages)gdPagesEnd("test",false);gdStop("test");closeOverlays();LAB.previewBar=true;LAB.ptoolPos=null;LAB.noCamera=false;D.guidance.on=false;D.card.hints.on=false;P.guideMute=true;P.introSeen=true;P.lang="en";P.reduce=true;pointOpen("painting");goScreen("card",{stop:"closed",reset:true});renderAll(false);""';
  await R(reset); await W(400);
  const press = async kind => { await p.click(`#ph #ptool [data-dev="${kind}"]`); await W(450) };
  const devNow = () => R('JSON.stringify({d:ui.device,w:ph.offsetWidth,h:ph.offsetHeight,land:ph.classList.contains("land"),tab:ph.classList.contains("tablet"),k:+((fit.style.transform.match(/scale\\(([\\d.]+)\\)/)||[0,1])[1]),r:(function(){var r=ph.getBoundingClientRect();return[Math.round(r.width),Math.round(r.height),Math.round(r.bottom)]})(),pressed:[].map.call(ph.querySelectorAll("#ptool [data-dev]"),function(x){return x.dataset.dev+":"+x.getAttribute("aria-pressed")+(x.classList.contains("land")?":land":"")}),win:innerHeight})').then(JSON.parse);

  const order = [['phone', 'phone_l'], ['phone', 'phone_p'], ['tablet', 'tablet_p'], ['tablet', 'tablet_l'], ['phone', 'phone_p']];
  let d0 = await devNow();
  ok('[buttons] on the phone (portrait, the default): the phone button pressed, upright', d0.d === 'phone_p' && JSON.stringify(d0.pressed) === '["phone:true","tablet:false"]', d0);
  for (let i = 0; i < order.length; i++) {
    const [kind, want] = order[i];
    await press(kind);
    const d = await devNow(), [w, h] = DEV[want], land = w > h;
    ok(`[buttons] a click on the ${kind} button: ${want}`, d.d === want && d.pressed.indexOf(kind + ':true' + (land ? ':land' : '')) >= 0, d);
    ok(`[size] ${want}: ${w} x ${h}, its frame scaled to fit (never resized), ${land ? 'land' : 'portrait'}${d.tab ? ', tablet' : ''}`, d.w === w && d.h === h && d.land === land && d.tab === want.startsWith('tablet') && d.k > 0 && d.k <= 1 && Math.abs(d.r[0] - w * d.k) <= 1 && Math.abs(d.r[1] - h * d.k) <= 1 && d.r[2] <= d.win, d);
    if (i === order.length - 1) continue;   // the phone again: only the buttons
    // the card's stops (the marker tapped, then each stop)
    await R('goScreen("card",{stop:"closed"});renderPhone(false);""'); await W(300);
    await p.click('#ph [data-marker="painting"]'); await W(500);
    const cfg = JSON.parse(await R('JSON.stringify({half:D.card.cardCfg.halfMax,full:D.card.cardCfg.fullHeight})'));
    const stops = {};
    for (const s of ['peek', 'half', 'full', 'closed']) {
      await R(`sheetTo("${s}");renderPhone(false);""`); await W(500);
      stops[s] = JSON.parse(await R('var g=document.getElementById("grab");JSON.stringify({top:parseFloat(scr.style.top),left:scr.offsetLeft,w:scr.offsetWidth,grab:!!g&&g.style.display!=="none"})'));
    }
    if (!land) {
      const want2 = { peek: h - 64, half: Math.round(h * (1 - cfg.half)), full: cfg.full === 'strip' ? 42 : 0 };
      ok(`[stops] ${want}: Peek ${want2.peek}, Half ${want2.half}, Full ${want2.full} (the test's own arithmetic), closed below the screen, the sheet the screen's width, the grabber shown`,
        stops.peek.top === want2.peek && stops.half.top === want2.half && stops.full.top === want2.full && stops.closed.top >= h && stops.peek.w === w && stops.half.grab, { stops, want2 });
    } else {
      ok(`[land] ${want}: a full-height side sheet at the right (top 0, 360 px wide, left ${w - 360}) at Peek, Half and Full; hidden when closed; no grabber`,
        ['peek', 'half', 'full'].every(s => stops[s].top === 0 && stops[s].w === 360 && stops[s].left === w - 360 && !stops[s].grab) && stops.closed.top >= h, stops);
    }
    // the guide: the tour from the hero's Start, its dock inside the phone, the card too
    await R('sheetTo("full");renderPhone(false);scr.scrollTop=0;""'); await W(400);
    await p.click('#ph .hero .gstart'); await W(1500);
    const g = JSON.parse(await R('var r=ph.getBoundingClientRect(),d=ph.querySelector(".gdock"),c=scr.getBoundingClientRect(),q=d&&d.getBoundingClientRect(),inn=function(a){return!!a&&a.left>=r.left-1&&a.right<=r.right+1&&a.top>=r.top-1&&a.bottom<=r.bottom+1};JSON.stringify({on:!!G.on,dock:!!d,dockIn:inn(q),cardIn:inn(c),dh:q&&Math.round(q.height)})'));
    ok(`[guide] ${want}: the tour plays from the hero's Start; its dock and the card inside the phone`, g.on && g.dock && g.dockIn && g.cardIn, g);
    if (land) ok(`[land] ${want}: the guide's dock inside the phone's height (${h} px)`, g.dockIn && g.dh <= h, g);
    await R('stopTour();""'); await W(300);
  }
  await R('deviceTo("phone_p");""');
  ok('No page errors', errs.length === 0, errs.slice(0, 5));
  console.log('TIME ' + Math.round((Date.now() - T0) / 1000) + ' s');
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); await b.close(); process.exit(fails ? 1 : 0);
})();
