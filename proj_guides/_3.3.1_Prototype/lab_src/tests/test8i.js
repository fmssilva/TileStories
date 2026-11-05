// v8i checks (Playwright + the installed Edge): node test8i.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 20000 }); await p.waitForTimeout(400);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.lang="en";S.guideCfg.panel="classic";renderAll(false)');
  // the voice: the browser has not loaded its voices yet (the first getVoices() is empty); the first step still speaks
  // the browser's voices arrive 700 ms after the tap (as on a fresh page, where the first getVoices() is empty)
  await R('window.__vAt=Infinity;var g0=speechSynthesis.getVoices.bind(speechSynthesis);speechSynthesis.getVoices=function(){return performance.now()<window.__vAt?[]:g0()};VOICES=[]');
  await R('S.view="card";S.stop="full";renderPhone(false)'); await p.waitForTimeout(400);
  await R('window.__vAt=performance.now()+700');
  await p.locator('#ph .gstart').first().click(); await p.waitForTimeout(1800);
  const first = await R('[!!G.utter,!!G.timer,G.noVoice,VOICES.length]');
  ok('voices that arrive after the tap: the first step still speaks (it waited), not captions only', first[0] && !first[1] && first[2] === false, first);
  await R('stopTour()');
  // no voice for the language: captions only, said in the captions, and the words still light up
  await R('S.lang="pt";VOICES=VOICES.filter(function(v){return !/^pt/.test(v.lang)});startTour()'); await p.waitForTimeout(2200);
  ok('no voice for the language: the captions say so', await R('var n=ph.querySelector(".gnovoice");!!n&&!n.hidden&&n.textContent.indexOf("legendas")>0'));
  ok('...and the words still light up', await R('[].some.call(ph.querySelectorAll(".gcap .gw"),function(w){return +w.style.getPropertyValue("--a")===1})'));
  await R('stopTour();S.lang="en"');
  // the highlight: a read step, muted (a tick per word, ~353 ms), so the timing is known
  const ri = await R('S.guideSteps.findIndex(function(s){return s.act==="read"})');
  const run = async (mode, extra, wait) => { await R(`S.guideMute=true;S.guideCfg.hl="${mode}";${extra || ''};startTour();G.i=${ri};runStep()`); await p.waitForTimeout(wait || 2600); };
  const snap = 'var t=G.hl.target,ws=t?[].slice.call(t.querySelectorAll(".gw")):[];ws.map(function(w){return[+w.style.getPropertyValue("--f")||0,+w.style.getPropertyValue("--h")||0,+w.style.getPropertyValue("--t")||0,+w.style.getPropertyValue("--a")||0]})';
  await run('comet', 'S.guideCfg.trail=4;S.guideCfg.glide=true');
  let s = await R(snap), k = s.findIndex(x => x[1] > 0 && x[3] === 1);
  const tails = s.slice(Math.max(0, k - 6), k).map(x => x[2]);
  ok('Comet: the word said is the head; the trail fades behind it, and is gone past Trail words', k > 4 && tails.every((v, i) => i === 0 || v >= tails[i - 1]) && tails[tails.length - 1] > 0.5 && s[k - 6][2] === 0 && s[k + 1][0] === 0, { k, tails });
  await R('stopTour()');
  await run('fill'); s = await R(snap); k = s.findIndex(x => x[1] > 0);
  ok('Fill: every word said stays lit', k > 3 && s.slice(0, k).every(x => x[2] === 1) && s.slice(k + 1).every(x => x[2] === 0), { k });
  await R('stopTour()');
  await run('word'); s = await R(snap); k = s.findIndex(x => x[1] > 0);
  ok('One word: only the word said', k > 3 && s.filter(x => x[2] > 0).length === 0 && s.filter(x => x[1] > 0).length === 1, { k });
  await R('stopTour()');
  await run('sentence'); s = await R(snap); k = s.findIndex(x => x[1] > 0);
  const band = await R('var t=G.hl.target,ws=[].slice.call(t.querySelectorAll(".gw")),k=ws.findIndex(function(w){return +w.style.getPropertyValue("--h")>0}),a=k,b=k;while(a>0&&!/[.!?]$/.test(ws[a-1].textContent))a--;while(b<ws.length-1&&!/[.!?]$/.test(ws[b].textContent))b++;[a,b,k,ws.every(function(w,j){var t=+w.style.getPropertyValue("--t")||0;return j===k?t===0:(j>=a&&j<=b)?t>0:t===0})]');
  ok('Sentence + word: a band over the sentence being said, the word in it the head', band[3], band);
  await R('stopTour()');
  // the glide: the head's fill moves through the word in small steps (not 0 -> 1)
  await run('comet', 'S.guideCfg.glide=true', 1200);
  const fills = []; for (let i = 0; i < 14; i++) { fills.push(await R('var w=[].find.call(G.hl.target.querySelectorAll(".gw"),function(w){return +w.style.getPropertyValue("--h")>0});w?+w.style.getPropertyValue("--h"):0')); await p.waitForTimeout(30); }
  ok('Glide: the light moves through the word (in-between fills)', fills.filter(f => f > 0.1 && f < 0.9).length >= 3, fills);
  await R('stopTour()');
  await run('comet', 'S.reduce=true', 1500);
  ok('Reduce Motion: word by word (the head is whole)', await R('[].every.call(G.hl.target.querySelectorAll(".gw"),function(w){var h=+w.style.getPropertyValue("--h")||0;return h===0||h===1})'));
  await R('stopTour();S.reduce=false');
  // contrast: the words over their light, light and dark, block text and captions (composited over the real backgrounds)
  const C = `(function(){var px=function(c){var m=c.match(/[\\d.]+/g).map(Number),k=c.indexOf("color(")===0?255:1;return{r:m[0]*k,g:m[1]*k,b:m[2]*k,a:m.length>3?m[3]:1}};
    var lum=function(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)};
    var over=function(f,b){return{r:f.r*f.a+b.r*(1-f.a),g:f.g*f.a+b.g*(1-f.a),b:f.b*f.a+b.b*(1-f.a),a:1}};
    var cr=function(a,b){var x=lum(a),y=lum(b);return Math.round((Math.max(x,y)+.05)/(Math.min(x,y)+.05)*100)/100};
    var bgOf=function(el){var st=[];while(el){var c=px(getComputedStyle(el).backgroundColor);if(c.a>0)st.push(c);if(c.a>=1)break;el=el.parentElement}var b={r:255,g:255,b:255,a:1};for(var i=st.length-1;i>=0;i--)b=over(st[i],b);return b};
    var t=G.hl.target,ws=[].slice.call(t.querySelectorAll(".gw")),head=ws.find(function(w){return +w.style.getPropertyValue("--h")>0}),tail=ws[ws.indexOf(head)-1],un=ws[ws.indexOf(head)+2];
    var cv=ph.querySelector(".cv")||document.querySelector(".cv"),hl=getComputedStyle(cv).getPropertyValue("--hlw").trim(),hla=S.dark?.36:1;
    var paper=bgOf(t),hb=over({r:255,g:217,b:138,a:hla},paper),tb=over(px(getComputedStyle(tail).backgroundColor),paper);
    var cap=ph.querySelector(".gcap"),cb=bgOf(cap);
    return{head:cr(px(getComputedStyle(head).color),hb),tail:cr(px(getComputedStyle(tail).color),tb),unread:cr(px(getComputedStyle(un).color),paper),capDim:cr({r:244,g:241,b:234,a:1}.r?over({r:244,g:241,b:234,a:.62},cb):cb,cb),capHead:cr({r:255,g:217,b:138,a:1},cb)}})()`;
  for (const dark of [false, true]) {
    await run('comet', `S.dark=${dark};renderAll(false);S.guideCfg.glide=false`, 2000);
    const c = await R(C);
    ok(`${dark ? 'dark' : 'light'}: the head, the trail, the unread words and the captions at 4.5:1 or more`, Object.values(c).every(v => v >= 4.5), c);
    await R('stopTour()');
  }
  await R('S.dark=false;S.guideCfg.glide=true;S.guideMute=false;renderAll(false)');
  // the text goes back to normal when the guide moves on or stops
  await run('comet', '', 1500); await R('stopTour()');
  ok('after the tour, the block text is plain again', await R('!document.querySelector(".gread")&&![].some.call(document.querySelectorAll("#ph .gw"),function(w){return w.getAttribute("style")})'));
  // the Editor: Captions has the style, Trail, Glide and a Try; Narration has the voice in this browser
  ok('Editor: Highlight style (4), Trail, Glide, Try, Voice in this browser', await R('var c=ed.querySelector(\'[data-fo="s7cap"]\'),v=ed.querySelector(\'[data-fo="s7voice"]\');!!c&&c.querySelectorAll(\'[data-k="@guideCfg.hl"] button, .seg [data-v]\').length>=0&&/Highlight style/.test(c.textContent)&&/Comet/.test(c.textContent)&&/Sentence \\+ word/.test(c.textContent)&&/Trail/.test(c.textContent)&&/Glide/.test(c.textContent)&&!!c.querySelector("[data-hltry]")&&!!v&&!!v.querySelector(\'select[data-sel="@guideCfg.voice_en"]\')'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
