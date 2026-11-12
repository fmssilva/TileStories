// test9t (Playwright + the installed Edge): node test9t.js -> PASS/FAIL per check. Guidance round 16: the automatic tour (guide/autotour.js),
// a tour made from a Detail Card's blocks, the default of a point with no written tour, through the app's and the Editor's own controls.
// [teaser] step 0 is the teaser at the hero: two sentences at most, the point's name, the minutes it says = tourMinutes().
// [blocks] every block with something to tell has its step, in the card's order; [skip] the kinds with nothing to tell (Audio Guide,
//   Practical Info on the Ribeira card) have none. [cap] each line under its cap. [twice] no sentence said twice. [links] no two links in a
//   row the same. [card words] only the card's words and the link words. [minutes] the minutes computed from the words at the voice's speed
//   (170 words a minute at speed 1, 0.7 s between lines), on the hero's button too; the written tour keeps its 3. Each in EN and PT.
// [follows] a card changed by Specific Marker's own block controls (the Gallery's 🗑, + Timeline): the tour follows.
// [play] the tour played through the real Guide button and the panel's Next, every step: its words in the captions, the focus on its block,
//   the try steps answered by real taps and a real drag; the games on the tour planned on its steps.
// [mine] Make it mine copies the steps into the written tour; an edited step stays after Regenerate of another; the changed-blocks notice
//   names the new block, Add them puts its step in the card's order, Dismiss hides it; on a point with a written tour it asks first.
// [ai] Make it more fluid… with AI, the model stubbed (never the live one): nothing changes and nothing is asked until the box is ticked;
//   then the stored lines play, each step shows its template line, a line with a new number keeps the template, Use the template line
//   brings one back; no model: the row says so and the template stays.
// [point] another point (the Torreão): its pin opens its card and its own tour; "A tour of {point}?" names it with its minutes and a tap
//   starts it; the scheduler: a point with a card has a tour now, a point with none (the city on the hill) still "has no tour".
// MUTATE=1: the teaser dropped, a block left out and a link said twice in a row: the checks named [teaser], [blocks] and [links] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1500, height: 1000 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 900) : '')); };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  const MUT = 'var _tf=tourFromCard;tourFromCard=function(c,s,l,o){var st=_tf(c,s,l,o),sk=st.skipped,sd=st.said;st.shift();var g=st.findIndex(function(x){return x.at&&x.at.kind==="Gallery"});if(g>=0)st.splice(g,1);' +
    'var ls=st.map(function(x,i){return x.at&&x.at.link?i:-1}).filter(function(i){return i>=0});if(ls.length>1){var a=st[ls[0]],z=st[ls[1]];z.text=z.text.replace(z.at.link,a.at.link);z.at.link=a.at.link}st.skipped=sk;st.said=sd;return st};""';
  const load = async () => { await p.goto('about:blank'); await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await W(600); if (process.env.MUTATE) await R(MUT); };
  await load();
  if (process.env.MUTATE) console.log('MUTATION: the teaser dropped, the Gallery\'s step left out, the second link said as the first');
  const base = `SCREENS.phone=[390,844];if(G.on)stopTour();S.previewBar=false;S.hints.on=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.cardCfg.mode="light";
    S.guideCfg.voice="device";S.guideCfg.look="ink";S.ui.rowDemo=false;S.guideCfg.music=false;S.vis.music=null;S.agentCfg.voiceOut=false;G.gd.manual=true;""`;
  await R(base);

  // ---------- 1. the Ribeira card's tour, switched to From the card by the Guide tab's own row ----------
  await R('S.tab="guide";S.op={};S.op.gt1=true;S.op.s7auto=true;S.ui.level=2;pointOpen("painting");S.view="card";S.stop="full";renderAll(false);""');
  const before = JSON.parse(await R('JSON.stringify({src:atSrc(),n:S.guideSteps.length,first:S.guideSteps[0].id,min:tourMinutes()})'));
  await p.click('[data-fo="s7auto"] [data-set="@tours.painting.src"][data-v="card"]'); await W(400);
  const DATA = `(function(){var st=S.guideSteps,lang=S.lang;return JSON.stringify({src:atSrc(),min:tourMinutes(),name:wordIn(S.hero.title,"en"),namePt:wordIn(S.hero.title,"pt"),hero:(scr.querySelector(".hero .gstart")||{}).textContent,
    card:S.card.map(function(b){return{id:b.id,kind:b.kind}}),rules:S.guideCfg.atRule,cap:S.guideCfg.atCap,tcap:S.guideCfg.atTeaserCap,
    steps:st.map(function(s){return{id:s.id,act:s.act,ref:s.ref,view:s.view,wait:s.wait||"",en:s.text.en,pt:s.text.pt,link:s.at&&s.at.link||"",block:s.at&&s.at.block||null,kind:s.at&&s.at.kind||null}}),
    page:langCodes().map(function(l){var g=atSteps(null,true),x=g.map(function(s){return Object.assign({},s,{text:s.text[l]})});x.said=g.said;return atCheck(x,l,Object.assign(atOpts(null,l),{rules:S.guideCfg.atRule})).filter(function(c){return!c.ok}).map(function(c){return l+":"+c.rule})})})})()`;
  const d = JSON.parse(await R(DATA));
  const words = t => (String(t).match(/\S+/g) || []).length, sents = t => String(t).trim().split(/(?<=[.!?…])\s+/).filter(Boolean), norm = s => s.toLowerCase().replace(/[^\p{L}\p{N}]+/gu, ' ').trim();
  const judge = (d, tag) => {
    const out = {};
    for (const l of ['en', 'pt']) {
      const st = d.steps, t0 = st[0] || {}, tx = t0[l] || '';
      const secs = st.reduce((a, s) => a + Math.max(1, words(s[l])) * 60 / 170 + .7, 0), mins = Math.max(1, Math.ceil(secs / 60 - .15));
      const name = l === 'en' ? d.name : d.namePt || '';
      out[l] = {
        teaser: t0.act === 'teaser' && t0.ref === 'hero' && sents(tx).length <= 2 && tx.indexOf(name) >= 0 && (mins === 1 ? /a minute|um minuto/.test(tx) : tx.indexOf(String(mins)) >= 0),
        mins, secs: Math.round(secs),
        cap: st.every((s, i) => words(s[l]) <= (i === 0 ? d.tcap : d.cap + 12)),
        twice: (() => { const seen = {}; let dup = []; st.forEach(s => sents(s[l]).forEach(x => { const k = norm(x); if (seen[k]) dup.push(x); seen[k] = 1 })); return dup })(),
        links: st.filter(s => s.link).map(s => s.link).filter((x, i, a) => i && a[i - 1] === x)
      };
    }
    const tell = d.card.filter(c => (d.rules[c.kind] || 'skip') !== 'skip').map(c => c.id), got = [];
    d.steps.forEach(s => { if (s.block && got.indexOf(s.block) < 0) got.push(s.block) });
    out.blocks = { tell, got, inOrder: JSON.stringify(got) === JSON.stringify(tell) };
    out.skipped = d.card.filter(c => (d.rules[c.kind] || 'skip') === 'skip').map(c => c.kind + ':' + d.steps.filter(s => s.block === c.id).length);
    return out;
  };
  const j = judge(d, 'ribeira');
  ok('[teaser] Ribeira, From the card (the Guide tab\'s own row): step 0 is the teaser at the hero, two sentences at most, the point\'s name, the minutes it says, EN and PT', before.src === 'written' && d.src === 'card' && j.en.teaser && j.pt.teaser, { before, src: d.src, en: d.steps[0] && d.steps[0].en, pt: d.steps[0] && d.steps[0].pt, mins: j.en.mins });
  ok('[blocks] every block with something to tell has its step, in the card\'s order (the story with its rows)', j.blocks.inOrder && j.blocks.tell.length >= 5, j.blocks);
  ok('[skip] the kinds with nothing to tell have no step (Audio Guide, Practical Info)', j.skipped.length >= 2 && j.skipped.every(x => /:0$/.test(x)) && j.skipped.some(x => /^Audio Guide/.test(x)) && j.skipped.some(x => /^Practical Info/.test(x)), j.skipped);
  ok('[cap] each line under its cap (the teaser 45 words, a step 40 and its link), EN and PT', j.en.cap && j.pt.cap, d.steps.map(s => words(s.en) + '/' + words(s.pt)));
  ok('[twice] no sentence said twice, EN and PT', !j.en.twice.length && !j.pt.twice.length, [j.en.twice, j.pt.twice]);
  ok('[links] no two links in a row the same, EN and PT; the links varied (at least 4 different)', !j.en.links.length && !j.pt.links.length && new Set(d.steps.map(s => s.link).filter(Boolean)).size >= 4, d.steps.map(s => s.link).filter(Boolean));
  ok('[minutes] the minutes computed from the words at the voice\'s speed: tourMinutes() = the test\'s own sum, said by the teaser and the hero\'s button (the written tour said 3)', d.min === j.en.mins && before.min === 3 && /· \d+ min/.test(d.hero) && d.hero.indexOf('· ' + d.min + ' min') >= 0, { min: d.min, test: j.en.mins, secs: j.en.secs, hero: d.hero });
  ok('[card words] the page\'s own checks (atCheck): only the card\'s words and the link words, every rule, EN and PT', d.page.every(x => !x.length), d.page);
  tsec('generate');

  // ---------- 2. the card changed through Specific Marker's own block controls: the tour follows ----------
  await R('S.tab="marker";S.op={};S.op.mcard=true;renderAll(false);""'); await W(300);
  const gi = await R('S.card.findIndex(function(b){return b.kind==="Gallery"})');
  await p.locator(`[data-fo="mcard"] [data-cmove="${gi}"][data-dir="0"]`).dispatchEvent('click'); await W(300);
  await p.click('[data-fo="mcard"] [data-cadd="Timeline"]'); await W(400);
  const f = JSON.parse(await R(DATA)), jf = judge(f);
  const tl = f.card.filter(c => c.kind === 'Timeline')[0], tlStep = tl && f.steps.filter(s => s.block === tl.id)[0], endI = f.steps.findIndex(s => s.act === 'end');
  ok('[follows] the Gallery removed (its 🗑) and a Timeline added (+ Timeline) by Specific Marker\'s controls: no Gallery step, a Timeline step before the end in the card\'s order, its first and last dates', !f.steps.some(s => s.kind === 'Gallery') && !!tlStep && f.steps.indexOf(tlStep) === endI - 1 && /Around 1500: the court moves to the river\. 1755: earthquake/.test(tlStep.en) && jf.blocks.inOrder, { tl: tlStep && tlStep.en, order: jf.blocks });
  tsec('follows');

  // ---------- 3. the tour played through the real Guide button and Next ----------
  await load(); await R(base); await R('S.tours.painting.src="card";S.tab="guide";S.view="card";S.stop="full";S.guidance.on=true;renderAll(false);""'); await W(400);
  await p.click('#ph .hero .gstart'); await until('G.on&&!!G.gd.plan', 4000);
  const plan = JSON.parse(await R('var P=G.gd.plan;JSON.stringify({n:P?P.beats.length:0,beats:P?P.beats.map(function(b){return{id:b.id,after:b.after||null,seam:b.seam}}):[],ids:S.guideSteps.map(function(s){return s.id}),gen:atIsGen(S.guideSteps)})'));
  ok('[play] the games on the tour are planned on the tour made from the card (its beats at its own steps)', plan.gen && plan.n >= 1 && plan.beats.every(x => !x.after || plan.ids.indexOf(x.after) >= 0), plan);
  await R('stopTour();gdPrefs().games="off";S.guideCfg.dockStart="peek";renderAll(false);""'); await W(300);
  await p.click('#ph .hero .gstart'); await until('G.on', 3000);
  const seen = [];
  for (let k = 0; k < 30; k++) {
    if (!(await until('G.on&&!!S.guideSteps[G.i]', 4000))) break;
    await until('var st=S.guideSteps[G.i];G.tpText===L(st.text)&&!!scr.querySelector(".gfocus")', 3000);
    const s = JSON.parse(await R('S.dockH="peek";renderGuide();var st=S.guideSteps[G.i],el=stepEl(st),fc=scr.querySelector(".gfocus");JSON.stringify({i:G.i,n:S.guideSteps.length,id:st.id,act:st.act,wait:st.wait||"",cap:G.tpText===L(st.text),focus:!!fc&&fc===el,line:(ph.querySelector(".gdock .gd-stop")||{}).textContent||""})'));
    seen.push(s);
    if (s.act === 'try') {
      // the panel at Mini, the block where the guide placed it: a finger's tap or drag at what the visitor sees (no scrolling by the test)
      await R('S.dockH="mini";renderGuide();""'); await W(700);
      const sel = s.wait === 'flip' ? '#ph .gfocus [data-flip]' : s.wait === 'answer' ? '#ph .gfocus .kq button[data-ok]' : '#ph .gfocus [data-ba]';
      const r = await p.locator(sel).first().boundingBox(); s.box = r && [Math.round(r.y), Math.round(r.height)];
      // a point of the target the visitor can touch (not under the coach's hint or the panel): the first one elementFromPoint gives back
      const pt = await p.evaluate(q => { const e = document.querySelector(q); if (!e) return null; const b = e.getBoundingClientRect(); for (const fy of [.5, .3, .7, .15, .85]) for (const fx of [.5, .2, .8, .1, .9]) { const x = b.left + b.width * fx, y = b.top + b.height * fy, h = document.elementFromPoint(x, y); if (h && (h === e || e.contains(h))) return [x, y]; } return null; }, sel);
      if (pt && s.wait !== 'drag') await p.mouse.click(pt[0], pt[1]);
      else if (r) { await p.mouse.move(r.x + r.width * .5, r.y + r.height * .5); await p.mouse.down(); await p.mouse.move(r.x + r.width * .2, r.y + r.height * .5, { steps: 6 }); await p.mouse.up(); }
      s.tried = await until(`G.tryDone||G.i!==${s.i}||!G.on`, 3000);
      await until(`G.i!==${s.i}||!G.on`, 9000);
      if (await R(`G.on&&G.i===${s.i}`)) { await R('S.dockH="peek";renderGuide();""'); await p.click('#ph .gdock [data-g="next"]'); }
    } else if (s.act === 'end') { break }
    else { await p.click('#ph .gdock [data-g="next"]'); await until(`G.i!==${s.i}||!G.on`, 3000); }
  }
  const tries = seen.filter(s => s.act === 'try');
  ok('[play] the tour from the card plays from start to end through the Guide button and Next: every step once, in order, to the end', seen.length === seen[0].n && seen.every((s, i) => s.i === i) && seen[seen.length - 1].act === 'end', seen.map(s => s.i + ':' + s.id));
  ok('[play] each step\'s words in the captions and the focus on its block (the hero for the teaser and the end)', seen.every(s => s.cap && s.focus), seen.filter(s => !s.cap || !s.focus));
  ok('[play] the try steps answered by real taps and a real drag (the flip card, the before and after, the quiz)', tries.length >= 3 && tries.every(s => s.tried) && ['flip', 'drag', 'answer'].every(w => tries.some(s => s.wait === w)), tries.map(s => s.id + ':' + s.wait + ':' + s.tried + ':' + JSON.stringify(s.box)));
  await R('if(G.on)stopTour();gdPrefs().games="on";""');
  tsec('play');

  // ---------- 4. Make it mine, an edit kept through Regenerate, the changed blocks ----------
  await load(); await R(base);
  await R('pointOpen("torreao");S.tab="marker";S.op={};S.op.m3=true;S.view="card";S.stop="full";renderAll(false);""'); await W(300);
  const t0 = JSON.parse(await R('JSON.stringify({src:atSrc(),gen:atSteps().map(function(s){return s.text.en})})'));
  await p.click('[data-fo="m3"] [data-at="mine"]'); await W(400);
  const t1 = JSON.parse(await R('JSON.stringify({src:atSrc(),made:!!atTour().made,mine:S.guideSteps.map(function(s){return s.text.en}),gen:atIsGen(S.guideSteps),pickable:ed.querySelectorAll(\'[data-fo="m3"] [data-pick="gstep"]\').length})'));
  ok('[mine] Make it mine (a real click, Specific Marker › 3 · Guide script): the steps copied into the written tour, now the Torreão\'s, editable step by step', t0.src === 'card' && t1.src === 'written' && t1.made && !t1.gen && JSON.stringify(t1.mine) === JSON.stringify(t0.gen) && t1.pickable === t1.mine.length, { t0: t0.src, t1 });
  // edit step 2's words (its own textarea), then Regenerate step 4
  await p.click('[data-fo="m3"] [data-pick="gstep"][data-id="a:t_facts"]'); await W(300);
  await p.fill('[data-fo="m3"] textarea[data-gtext="a:t_facts|en"]', 'My own words about the Torreão.'); await W(200);
  await R('renderEditor();""');
  await p.click('[data-fo="m3"] [data-pick="gstep"][data-id="a:t_time"]'); await W(300);
  await R('byId(S.guideSteps,"a:t_time").text.en="Something else.";renderEditor();""');
  await p.click('[data-fo="m3"] [data-at="regen|a:t_time"]'); await W(300);
  const t2 = JSON.parse(await R('JSON.stringify({facts:byId(S.guideSteps,"a:t_facts").text.en,time:byId(S.guideSteps,"a:t_time").text.en,gen:byId(atSteps(),"a:t_time").text.en,edited:atEdited(byId(S.guideSteps,"a:t_facts"))})'));
  ok('[mine] an edited step stays after Regenerate of another (the edit typed in its EN box; the Timeline\'s step regenerated from the card)', t2.facts === 'My own words about the Torreão.' && t2.edited && t2.time === t2.gen, t2);
  // the card changes: + Before and After (Specific Marker's own button): the notice names it; Add them; then + Dialogue and Dismiss
  await R('S.op.mcard=true;renderEditor();""'); await p.click('[data-fo="mcard"] [data-cadd="Before and After"]'); await W(300);
  await R('S.op.m3=true;renderEditor();""'); await W(200);
  const nt = await R('(ed.querySelector(\'[data-fo="m3"] .atnew b\')||{}).textContent||""');
  await p.click('[data-fo="m3"] [data-at="addnew"]'); await W(300);
  const t3 = JSON.parse(await R('var b=S.card.filter(function(x){return x.kind==="Before and After"})[0];JSON.stringify({ids:S.guideSteps.map(function(s){return s.at&&s.at.block||s.id}),ba:b.id,card:S.card.map(function(x){return x.id}),notice:!!ed.querySelector(\'[data-fo="m3"] .atnew\'),facts:byId(S.guideSteps,"a:t_facts").text.en})'));
  const baAt = t3.ids.indexOf(t3.ba), endAt = t3.ids.indexOf('aend');
  ok('[mine] the changed-blocks notice names the new block in words ("1 new block not in the tour: the Before and After."); Add them puts its step in the card\'s order (last block: before the end); the edit kept', /^1 new block not in the tour: the Before and After\.$/.test(nt) && baAt === endAt - 1 && !t3.notice && t3.facts === 'My own words about the Torreão.', { nt, t3 });
  await R('S.op.mcard=true;renderEditor();""'); await p.click('[data-fo="mcard"] [data-cadd="Dialogue"]'); await W(300); await R('S.op.m3=true;renderEditor();""');
  const nt2 = await R('(ed.querySelector(\'[data-fo="m3"] .atnew b\')||{}).textContent||""');
  await p.click('[data-fo="m3"] [data-at="dismiss"]'); await W(300);
  const t4 = JSON.parse(await R('JSON.stringify({notice:!!ed.querySelector(\'[data-fo="m3"] .atnew\'),poll:S.guideSteps.some(function(s){return s.at&&s.at.kind==="Dialogue"})})'));
  ok('[mine] Dismiss: the notice about the Dialogue goes and the tour stays as it is', /the Dialogue\./.test(nt2) && !t4.notice && !t4.poll, { nt2, t4 });
  // the palace has a written tour: Make it mine asks first
  await R('pointOpen("painting");S.tours.painting.src="card";S.tab="guide";S.op={};S.op.gt1=true;S.op.s7auto=true;renderAll(false);""'); await W(300);
  await p.click('[data-fo="s7auto"] [data-at="mine"]'); await W(300);
  const dq = await R('(ed.querySelector(\'[data-fo="s7auto"] .decide b\')||{}).textContent||""');
  await p.click('[data-fo="s7auto"] [data-at="mine-no"]'); await W(200);
  const kept = JSON.parse(await R('JSON.stringify({src:atSrc(),w:atTour().written.length,first:atTour().written[0].id})'));
  ok('[mine] on a point with a written tour (Ribeira Palace) Make it mine asks first, in words; Keep the written tour keeps it', /^Replace the 12 written steps of Ribeira Palace with the \d+ made from its card\?$/.test(dq) && kept.src === 'card' && kept.w === 12 && kept.first === 'g0', { dq, kept });
  tsec('mine');

  // ---------- 5. Make it more fluid… with AI, the model stubbed ----------
  await load(); await R(base);
  await R('pointOpen("painting");S.tours.painting.src="card";S.tab="guide";S.op={};S.op.gt1=true;S.op.s7auto=true;S.view="card";S.stop="full";renderAll(false);""'); await W(200);
  await R(`window.__calls=0;sampleFn=function(pr){window.__calls++;var m=JSON.parse(pr.slice(pr.indexOf("LINES:")+7));return Promise.resolve({text:"Here you are: "+JSON.stringify({steps:m.map(function(x){var en=x.id==="a:b_gal"?"In 1999 the square had three centuries.":x.id==="a0"?x.en:"Oh! "+x.en;return{id:x.id,en:en,pt:x.id==="a0"?x.pt:"Olhe! "+x.pt}})})})};
    pointOpen("painting");S.tours.painting.src="card";S.tab="guide";S.op={};S.op.gt1=true;S.op.s7auto=true;S.view="card";S.stop="full";renderAll(false);""`); await W(300);
  const a0 = JSON.parse(await R('JSON.stringify({calls:window.__calls,steps:S.guideSteps.map(function(s){return s.text.en}),ai:S.guideCfg.autoAI})'));
  await p.click('[data-fo="s7auto"] input[data-set="@guideCfg.autoAI"]');
  await until('/Rewritten by the model/.test((ed.querySelector(".atainote")||{}).textContent||"")', 4000); await W(300);
  const a1 = JSON.parse(await R('JSON.stringify({calls:window.__calls,note:(ed.querySelector(".atainote")||{}).textContent,steps:S.guideSteps.map(function(s){return{id:s.id,en:s.text.en,ai:!!(s.at&&s.at.ai),tpl:s.at&&s.at.tpl&&s.at.tpl.en}}),was:ed.querySelectorAll(\'[data-fo="s7auto"] .atai-was\').length})'));
  const aiN = a1.steps.filter(s => s.ai).length, gal = a1.steps.filter(s => s.id === 'a:b_gal')[0], teaser = a1.steps[0];
  ok('[ai] nothing changes and the model is not asked until the box is ticked (Off is the template tour)', a0.calls === 0 && !a0.ai && a0.steps.every(t => !/^Oh! /.test(t)), a0);
  ok('[ai] ticked (a real click): the stored lines play (a step\'s link and words, Azu\'s voice), each with its template line beside it in the list; the teaser the model left as it was stays the template', a1.calls === 4 && aiN >= 6 && a1.steps.filter(s => s.ai).every(s => /^Oh! /.test(s.en) && s.tpl && s.en === 'Oh! ' + s.tpl) && a1.was === aiN && !teaser.ai, { calls: a1.calls, aiN, was: a1.was, note: a1.note });
  ok('[ai] a line with a new number (1999) keeps the template, and the row says so', gal && !gal.ai && !/1999/.test(gal.en) && /kept as the template \(new words: 1999/.test(a1.note), { gal, note: a1.note });
  const one = a1.steps.filter(s => s.ai)[0];
  await p.click(`[data-fo="s7auto"] [data-at="tpl|${one.id}"]`); await W(300);
  const a2 = JSON.parse(await R(`var s=byId(S.guideSteps,"${one.id}");JSON.stringify({en:s.text.en,ai:!!s.at.ai,others:S.guideSteps.filter(function(x){return x.at&&x.at.ai}).length})`));
  ok('[ai] Use the template line (a real click) brings that step\'s template back; the others keep theirs', !a2.ai && a2.en === one.tpl && a2.others === aiN - 1, a2);
  await R('sampleFn=null;S.guideCfg.autoAI=false;S.atAINote=null;S.tours.painting.ai={};renderAll(false);""'); await W(200);
  await p.click('[data-fo="s7auto"] input[data-set="@guideCfg.autoAI"]'); await W(300);
  const a3 = JSON.parse(await R('JSON.stringify({note:(ed.querySelector(".atainote")||{}).textContent,ai:S.guideSteps.some(function(s){return s.at&&s.at.ai}),on:S.guideCfg.autoAI})'));
  ok('[ai] no model on the page: ticking says so in the row and the template tour stays', a3.on && /No model on this page/.test(a3.note) && !a3.ai, a3);
  tsec('ai');

  // ---------- 6. another point: the Torreão's pin, A tour of {point}?, the scheduler ----------
  await load(); await R(base);
  await R('setWall("ribeira");S.guidance.on=true;S.guidance.kinds.forEach(function(k){k.on=k.id==="content"});gdResetVisitor();gdSimAim("torreao");renderPhone(false);""'); await W(400);
  await p.click('#ph [data-cpin="torreao"]'); await W(500);
  const pc = JSON.parse(await R('JSON.stringify({poi:S.m.poi,title:(scr.querySelector(".hero h2")||{}).textContent,stop:S.stop,hero:(scr.querySelector(".hero .gstart")||{}).textContent,min:tourMinutes(),blocks:scr.querySelectorAll(\'[data-ref^="b:t_"]\').length})'));
  ok('[point] the Torreão\'s pin on the wall (a real tap) opens its own card: its hero, its blocks, the Guide button with its tour\'s minutes', pc.poi === 'torreao' && pc.title === 'The Torreão' && pc.blocks >= 5 && pc.hero.indexOf('· ' + pc.min + ' min') >= 0, pc);
  await R('S.stop="closed";pointOpen("painting");setWall("ribeira");gdResetVisitor();gdSimAim("torreao");renderPhone(false);""'); await W(500);
  const QUIET = 'gdStop("test");G.gd.cur=null;G.gd.touchAt=null;G.gd.actAt=null;G.gd.movedAt=null;gdAdvance(3000);if(G.gd.cur){gdStop("test");G.gd.cur=null}';
  const sc = JSON.parse(await R(QUIET + 'var c=gdCentral(),it=gdItemNow("tourof");JSON.stringify({c:c,why:gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"tourof"),min:tourMinutes("torreao")})'));
  await R('gdStart(gdItemNow("tourof"),"test",{pull:true,ms:60000});""');
  await until('!!ph.querySelector(".gdbub:not(.gd-out) [data-gdbub]")', 3000); await W(400);
  const bub = await R('(ph.querySelector(".gdbub:not(.gd-out) [data-gdbub]")||{}).textContent||""');
  await p.click('#ph .gdbub:not(.gd-out) [data-gdbub]'); await until('G.on', 3000); await W(400);
  const tr = JSON.parse(await R('JSON.stringify({on:G.on,poi:S.m.poi,first:S.guideSteps[0]&&S.guideSteps[0].id,gen:atIsGen(S.guideSteps),teaser:S.guideSteps[0]&&L(S.guideSteps[0].text),title:(scr.querySelector(".hero h2")||{}).textContent})'));
  ok('[point] "A tour of {point}?" about the Torreão in the middle: its name and its tour\'s minutes', sc.c && sc.c.id === 'torreao' && sc.c.tour && sc.c.min === sc.min && bub === 'The Torreão has a story. A tour of ' + sc.min + (sc.min === 1 ? ' minute?' : ' minutes?'), { sc, bub });
  ok('[point] a tap on it opens the Torreão\'s card and starts its tour made from the card (the teaser names it)', tr.on && tr.poi === 'torreao' && tr.gen && tr.first === 'a0' && /^The Torreão: /.test(tr.teaser) && tr.title === 'The Torreão', tr);
  await R('if(G.on)stopTour();gdStop("test");S.stop="closed";pointOpen("painting");setWall("ribeira");gdResetVisitor();gdSimAim("castle");renderPhone(false);""'); await W(300);
  const cs = JSON.parse(await R(QUIET + 'var c=gdCentral();JSON.stringify({c:c,why:gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"tourof")})'));
  ok('[point] the scheduler: a point with no card (the city on the hill) still "has no tour"; the points with a card have one now', cs.c && !cs.c.tour && /has no tour/.test(cs.why) && !/has no tour/.test(sc.why), { castle: cs.why, torreao: sc.why });
  tsec('point');

  ok('no page errors', !errs.length, errs.slice(0, 5));
  console.log(fails ? `FAILED ${fails}` : 'ALL PASS');
  await b.close(); process.exit(fails ? 1 : 0);
})().catch(e => { console.error(e); process.exit(2); });
