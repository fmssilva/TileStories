// test9t (Playwright + the installed Edge): node test9t.js -> PASS/FAIL per check. Guidance round 16: the automatic tour (guide/autotour.js),
// a tour made from a Detail Card's blocks, the default of a point with no written tour, through the app's and the Editor's own controls.
// [teaser] step 0 is the teaser at the hero: two sentences at most, the point's name, the minutes it says = tourMinutes().
// [blocks] every block with something to tell has its step, in the card's order; [skip] the kinds with nothing to tell (Audio Guide,
//   Practical Info on the Ribeira card) have none. [cap] each line under its cap. [twice] no sentence said twice. [links] no two links in a
//   row the same. [card words] only the card's words and the link words. [minutes] the minutes computed from the words at the voice's speed
//   (170 words a minute at speed 1, 0.7 s between lines), on the hero's button too; the written tour keeps its 3. Each in EN and PT.
// [follows] a card changed by Specific Marker's own block controls (the Gallery's 🗑, + Timeline, + Dialogue): the tour follows (16b: the
//   Timeline's three dates were all told by the story, so it has no step; the Dialogue has one, before the end).
// [play] the tour played through the real Guide button and the panel's Next, every step: its words in the captions, the focus on its block,
//   the try steps answered by real taps and a real drag; the games on the tour planned on its steps.
// [mine] Make it mine copies the steps into the written tour; an edited step stays after Regenerate of another; the changed-blocks notice
//   names the new block, Add them puts its step in the card's order, Dismiss hides it; on a point with a written tour it asks first.
// [ai] Make it more fluid… with AI, the model stubbed (never the live one): nothing changes and nothing is asked until the box is ticked;
//   then the stored lines play, each step shows its template line, a line with a new number keeps the template, Use the template line
//   brings one back; no model: the row says so and the template stays.
// [point] another point (the Torreão): its pin opens its card and its own tour; "A tour of {point}?" names it with its minutes and a tap
//   starts it; the scheduler: a point with a card has a tour now, a point with none (the city on the hill) still "has no tour".
// Guidance round 16b (the developer's answers, the Architect's two fixes):
// [facts] no fact twice, EN and PT, the four points, by the test's own arithmetic (a fact's keys: its numbers, a year, a decade, a date, a
//   number in words; its proper names, a title left out; a year after before / after / since / until is a reference; a bare year repeats a
//   date of that year; the point's name is no sentence's fact but a list item's): each sentence or list item with keys brings one not said
//   before, and no Quick Facts item's keys come again in a later unit; the page's own atCheck agrees; and the named cases.
// [links] every link from its block's own words (a heading or a label, a caption, a date, the dates, its kind's sentence) or the neutral
//   one, at most 6 words before the content, none of round 16's generic links, never twice in a row, EN and PT; the Editor's list shows each
//   link with its source.
// [short] under 2 minutes "a short tour" (the teaser, the hero's button, the invitation, the Editor), from 2 up the minutes, EN and PT.
// [hero words] the hero's words (its eyebrow, title, line) clear of the panel, the bubble and Azu at the teaser at Mini (G15.hero, round
//   15's family), the four tours and the written tour's g0; the hero's Start button hidden while a tour plays, back after it.
// [crop] each new point's hero, a crop of the wall painting around its own point (the wall's file, its size and offset by the test's own
//   arithmetic, the point inside the hero where the crop puts it), at Full and at Half, three different crops, kept during the teaser.
// MUTATE=1: the teaser dropped, a block left out and a link said twice in a row; and (16b) the fact rule off, the neutral link back to
//   "Look at this.", the short-tour words off, the hero left at the top with its Start button shown, the crop off: the checks named
//   [teaser], [blocks], [links], [facts], [short], [hero words] and [crop] turn red.
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
  const MUT = 'atKnown=function(){return false};S.tx.at_link={en:"Look at this.",pt:"Repare nisto."};S.tx.gd_tourof_short={en:"",pt:""};S.tx.at_cover_short={en:"In about 1 minutes: {0}.",pt:"Em cerca de 1 minutos: {0}."};' +
    'tourLength=function(m){return fmt(TXT("guide_duration"),m)};heroWords=function(){return null};heroCropOf=function(){return null};document.head.insertAdjacentHTML("beforeend","<style>#ph.touring .hero .hact,#ph.touring .hero .gstart{visibility:visible!important}</style>");' +
    'var _tf=tourFromCard;tourFromCard=function(c,s,l,o){var st=_tf(c,s,l,o),sk=st.skipped,sd=st.said;st.shift();var g=st.findIndex(function(x){return x.at&&x.at.kind==="Gallery"});if(g>=0)st.splice(g,1);' +
    'var ls=st.map(function(x,i){return x.at&&x.at.link?i:-1}).filter(function(i){return i>=0});if(ls.length>1){var a=st[ls[0]],z=st[ls[1]];z.text=z.text.replace(z.at.link,a.at.link);z.at.link=a.at.link}st.skipped=sk;st.said=sd;return st};""';
  const load = async () => { await p.goto('about:blank'); await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await W(600); if (process.env.MUTATE) await R(MUT); };
  await load();
  if (process.env.MUTATE) console.log('MUTATION: the teaser dropped, the Gallery\'s step left out, the second link said as the first; the fact rule off, the neutral link "Look at this.", no short-tour words, the hero left at the top with its Start button, no crop');
  const base = `SCREENS.phone=[390,844];if(G.on)stopTour();S.previewBar=false;S.hints.on=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.reduce=false;S.dark=false;S.cardCfg.mode="light";
    S.guideCfg.voice="device";S.guideCfg.look="ink";S.ui.rowDemo=false;S.guideCfg.music=false;S.vis.music=null;S.agentCfg.voiceOut=false;G.gd.manual=true;""`;
  await R(base);

  // ---------- 1. the Ribeira card's tour, switched to From the card by the Guide tab's own row ----------
  await R('S.tab="guide";S.op={};S.op.gt1=true;S.op.s7auto=true;S.ui.level=2;pointOpen("painting");S.view="card";S.stop="full";renderAll(false);""');
  const before = JSON.parse(await R('JSON.stringify({src:atSrc(),n:S.guideSteps.length,first:S.guideSteps[0].id,min:tourMinutes()})'));
  await p.click('[data-fo="s7auto"] [data-set="@tours.painting.src"][data-v="card"]'); await W(400);
  const DATA = `(function(){var st=S.guideSteps,lang=S.lang;return JSON.stringify({src:atSrc(),min:tourMinutes(),name:wordIn(S.hero.title,"en"),namePt:wordIn(S.hero.title,"pt"),hero:(scr.querySelector(".hero .gstart")||{}).textContent,
    card:S.card.map(function(b){return{id:b.id,kind:b.kind}}),said:Object.keys(atSteps(null,true).said||{}),rules:S.guideCfg.atRule,cap:S.guideCfg.atCap,tcap:S.guideCfg.atTeaserCap,
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
        teaser: t0.act === 'teaser' && t0.ref === 'hero' && sents(tx).length <= 2 && tx.indexOf(name) >= 0 && (mins < 2 ? /In a short tour:|Numa visita curta:/.test(tx) && !/\d+ min/.test(tx) : tx.indexOf(String(mins)) >= 0),
        mins, secs: Math.round(secs),
        cap: st.every((s, i) => words(s[l]) <= (i === 0 ? d.tcap : d.cap + 12)),
        twice: (() => { const seen = {}; let dup = []; st.forEach(s => sents(s[l]).forEach(x => { const k = norm(x); if (seen[k]) dup.push(x); seen[k] = 1 })); return dup })(),
        links: st.filter(s => s.link).map(s => s.link).filter((x, i, a) => i && a[i - 1] === x)
      };
    }
    const tell = d.card.filter(c => (d.rules[c.kind] || 'skip') !== 'skip' && (d.said || []).indexOf(c.id) < 0).map(c => c.id), got = [];
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
  ok('[minutes] the minutes computed from the words at the voice\'s speed: tourMinutes() = the test\'s own sum, said by the teaser and the hero\'s button (the written tour said 3)', d.min === j.en.mins && before.min === 3 && (d.min < 2 ? /· short$/.test(d.hero) : d.hero.indexOf('· ' + d.min + ' min') >= 0), { min: d.min, test: j.en.mins, secs: j.en.secs, hero: d.hero });
  ok('[card words] the page\'s own checks (atCheck): only the card\'s words and the link words, every rule, EN and PT', d.page.every(x => !x.length), d.page);
  tsec('generate');

  // ---------- 2. the card changed through Specific Marker's own block controls: the tour follows ----------
  await R('S.tab="marker";S.op={};S.op.mcard=true;renderAll(false);""'); await W(300);
  const gi = await R('S.card.findIndex(function(b){return b.kind==="Gallery"})');
  await p.locator(`[data-fo="mcard"] [data-cmove="${gi}"][data-dir="0"]`).dispatchEvent('click'); await W(300);
  await p.click('[data-fo="mcard"] [data-cadd="Timeline"]'); await until('S.card.some(function(b){return b.kind==="Timeline"})', 3000);
  await p.click('[data-fo="mcard"] [data-cadd="Dialogue"]'); await until('S.card.some(function(b){return b.kind==="Dialogue"})', 3000);
  const f = JSON.parse(await R(DATA)), jf = judge(f), fsaid = JSON.parse(await R('JSON.stringify(Object.keys(atSteps(null,true).said||{}))'));
  const tl = f.card.filter(c => c.kind === 'Timeline')[0], tlStep = tl && f.steps.filter(s => s.block === tl.id)[0], dl = f.card.filter(c => c.kind === 'Dialogue')[0], dlStep = dl && f.steps.filter(s => s.block === dl.id)[0], endI = f.steps.findIndex(s => s.act === 'end');
  ok('[follows] [facts] the Gallery removed (its 🗑), a Timeline and a Dialogue added (+ Timeline, + Dialogue) by Specific Marker\'s controls: no Gallery step; the Timeline none (its dates c. 1500, 1580s and 1755 all told by the story: nothing new); the Dialogue\'s step before the end in the card\'s order, with the neutral link', !f.steps.some(s => s.kind === 'Gallery') && !tlStep && fsaid.indexOf(tl && tl.id) >= 0 && !!dlStep && f.steps.indexOf(dlStep) === endI - 1 && /^And now, your Majesty, the river front is finished\.$/.test(dlStep.en) && jf.blocks.inOrder, { dl: dlStep && dlStep.en, tl: tlStep && tlStep.en, fsaid, order: jf.blocks });
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
  ok('[point] the Torreão\'s pin on the wall (a real tap) opens its own card: its hero, its blocks, the Guide button with its tour\'s length (a short one: "· short")', pc.poi === 'torreao' && pc.title === 'The Torreão' && pc.blocks >= 5 && (pc.min < 2 ? /· short$/.test(pc.hero) : pc.hero.indexOf('· ' + pc.min + ' min') >= 0), pc);
  await R('S.stop="closed";pointOpen("painting");setWall("ribeira");gdResetVisitor();gdSimAim("torreao");renderPhone(false);""'); await W(500);
  const QUIET = 'gdStop("test");G.gd.cur=null;G.gd.touchAt=null;G.gd.actAt=null;G.gd.movedAt=null;gdAdvance(3000);if(G.gd.cur){gdStop("test");G.gd.cur=null}';
  const sc = JSON.parse(await R(QUIET + 'var c=gdCentral(),it=gdItemNow("tourof");JSON.stringify({c:c,why:gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"tourof"),min:tourMinutes("torreao")})'));
  await R('gdStart(gdItemNow("tourof"),"test",{pull:true,ms:60000});""');
  await until('!!ph.querySelector(".gdbub:not(.gd-out) [data-gdbub]")', 3000); await W(400);
  const bub = await R('(ph.querySelector(".gdbub:not(.gd-out) [data-gdbub]")||{}).textContent||""');
  await p.click('#ph .gdbub:not(.gd-out) [data-gdbub]'); await until('G.on', 3000); await W(400);
  const tr = JSON.parse(await R('JSON.stringify({on:G.on,poi:S.m.poi,first:S.guideSteps[0]&&S.guideSteps[0].id,gen:atIsGen(S.guideSteps),teaser:S.guideSteps[0]&&L(S.guideSteps[0].text),title:(scr.querySelector(".hero h2")||{}).textContent})'));
  ok('[point] [short] "A tour of {point}?" about the Torreão in the middle: its name, and its tour under 2 minutes offered as a short one ("A short tour of the Torreão?")', sc.c && sc.c.id === 'torreao' && sc.c.tour && sc.c.min === sc.min && (sc.min < 2 ? bub === 'A short tour of the Torreão?' : bub === 'The Torreão has a story. A tour of ' + sc.min + ' minutes?'), { sc, bub });
  ok('[point] a tap on it opens the Torreão\'s card and starts its tour made from the card (the teaser names it)', tr.on && tr.poi === 'torreao' && tr.gen && tr.first === 'a0' && /^The Torreão: /.test(tr.teaser) && tr.title === 'The Torreão', tr);
  await R('if(G.on)stopTour();gdStop("test");S.stop="closed";pointOpen("painting");setWall("ribeira");gdResetVisitor();gdSimAim("castle");renderPhone(false);""'); await W(300);
  const cs = JSON.parse(await R(QUIET + 'var c=gdCentral();JSON.stringify({c:c,why:gdWhyNot(gdItems(),S.gdVisitor,gdContext(),gdNow(),"tourof")})'));
  ok('[point] the scheduler: a point with no card (the city on the hill) still "has no tour"; the points with a card have one now', cs.c && !cs.c.tour && /has no tour/.test(cs.why) && !/has no tour/.test(sc.why), { castle: cs.why, torreao: sc.why });
  tsec('point');

  // ---------- 7. guidance round 16b: [facts] no fact twice and [links] tied to the content, the four points, EN and PT ----------
  await load(); await R(base);
  const TOURS = JSON.parse(await R(`(function(){var out={};["painting","torreao","square","river"].forEach(function(poi){var p=pointOf(poi);out[poi]={};langCodes().forEach(function(l){var o=atOpts(poi,l),st=tourFromCard(p.card||[],p.story||[],l,o);
    var src=function(s){if(!s.at||!s.at.block)return"";if(s.ref&&s.ref.indexOf("r:")===0){var rows=p.story||[],r=byId(rows,s.ref.slice(2)),i=rows.indexOf(r),rs=r?atRowSays(r,l):{},hd=i>0&&rows[i-1].kind==="heading"?atRowSays(rows[i-1],l).text:"";return[rs.text,rs.lead,rs.cap,rs.title,hd].join(" | ")}
      var b=byId(p.card||[],s.at.block),bits=[];(function walk(v){if(v==null)return;if(Array.isArray(v))v.forEach(walk);else if(typeof v==="object")Object.keys(v).forEach(function(k){walk(v[k])});else bits.push(String(v))})(b?blockSays(b,l):{});return bits.join(" | ")};
    out[poi][l]={name:o.name,said:Object.keys(st.said||{}),checks:atCheck(st,l,Object.assign(o,{rules:S.guideCfg.atRule})).filter(function(c){return!c.ok}).map(function(c){return c.rule+": "+c.why}),
      neutral:wordIn(S.tx.at_link,l),sentence:["at_link_facts","at_link_facts_n","at_link_story","at_link_back","at_link_quiz"].map(function(k){return wordIn(S.tx[k],l)}),circa:wordIn(S.tx.at_circa,l),
      tpl:{head:wordIn(S.tx.at_link_head,l),cap:wordIn(S.tx.at_link_cap,l),pics:wordIn(S.tx.at_link_pics,l),dates:wordIn(S.tx.at_link_dates,l),year:wordIn(S.tx.at_link_year,l),circa:wordIn(S.tx.at_link_circa,l)},
      steps:st.map(function(s){return{id:s.id,act:s.act,text:s.text,kind:s.at&&s.at.kind||null,link:s.at&&s.at.link||"",src:src(s)}})}})});return JSON.stringify(out)})()`));
  // the test's own fact keys (the developer's rule, written again here): numbers, dates, decades, numbers in words, proper names
  const MON = { en: 'january february march april may june july august september october november december'.split(' '), pt: 'janeiro fevereiro março abril maio junho julho agosto setembro outubro novembro dezembro'.split(' ') };
  const NUM = { en: { one: 1, two: 2, three: 3, four: 4, five: 5, six: 6, seven: 7, eight: 8, nine: 9, ten: 10, twenty: 20, thirty: 30, forty: 40, fifty: 50, sixty: 60, seventy: 70, eighty: 80, ninety: 90, hundred: 100, thousand: 1000 },
    pt: { um: 1, uma: 1, dois: 2, duas: 2, 'três': 3, quatro: 4, cinco: 5, seis: 6, sete: 7, oito: 8, nove: 9, dez: 10, vinte: 20, trinta: 30, quarenta: 40, cinquenta: 50, sessenta: 60, setenta: 70, oitenta: 80, noventa: 90, cem: 100, cento: 100, duzentos: 200, trezentos: 300, quatrocentos: 400, quinhentos: 500, mil: 1000 } };
  const TITLE = /^(king|queen|saint|st|rei|rainha|d|dom|dona|são)$/i, ART = /^(the|a|an|o|os|as|um|uma)$/i, JOIN = /^(do|da|dos|das|de|of)$/;
  const keysOf = (t, lang, proper, notName) => {
    const out = new Set(), mo = MON[lang], de = lang === 'pt' ? '\\s+de' : ''; let s = String(t);
    s = s.replace(new RegExp(`\\b(\\d{1,2})${de}\\s+(${mo.join('|')})${de}\\s+(\\d{4})\\b`, 'gi'), (m, d, mn, y) => { out.add(`date ${+d}.${mo.indexOf(mn.toLowerCase()) + 1}.${y}`); return '§'; });
    s = s.replace(new RegExp(`\\b(${mo.join('|')})${de}\\s+(\\d{4})\\b`, 'gi'), (m, mn, y) => { out.add(`month ${mo.indexOf(mn.toLowerCase()) + 1}.${y}`); return '§'; });
    s = s.replace(/\b(\d{3}0)s\b|\bdécada de (\d{3}0)\b/gi, (m, a, b) => { out.add('decade ' + (a || b)); return '§'; });
    s.replace(/\b\d{1,3}(?:[ ,  ]\d{3})+\b|\b\d+\b/g, (m, at) => { const v = +m.replace(/\D/g, ''); if (v >= 1000 && v <= 2100) { if (!/\b(before|after|since|until|antes de|depois de|desde|até)\s+$/i.test(s.slice(0, at))) out.add('year ' + v); } else if (v >= 10) out.add('n ' + v); return m; });
    const toks = [...String(t).matchAll(/[\p{L}\p{N}]+(?:['’-][\p{L}\p{N}]+)*/gu)].map(m => ({ w: m[0], at: m.index }));
    for (let i = 0; i < toks.length; i++) { if (NUM[lang][toks[i].w.toLowerCase()] == null) continue; let tot = 0, cur = 0, j = i;
      for (; j < toks.length; j++) { const x = toks[j].w.toLowerCase(); if ((x === 'and' || x === 'e') && j > i && toks[j + 1] && NUM[lang][toks[j + 1].w.toLowerCase()] != null) continue; const v = NUM[lang][x]; if (v == null) break; if (x === 'hundred') cur = (cur || 1) * 100; else if (v === 1000) { tot += (cur || 1) * 1000; cur = 0; } else cur += v; }
      if (tot + cur >= 20) out.add('n ' + (tot + cur)); i = j - 1; }
    const atStart = k => { const b = String(t).slice(0, toks[k].at).replace(/["“'«(\s]+$/, ''); return !b || /[.!?…]$/.test(b) && !/\b(D|St)\.$/.test(b); };
    const cap = k => /^\p{Lu}/u.test(toks[k].w) && MON[lang].indexOf(toks[k].w.toLowerCase()) < 0 && (!atStart(k) || proper.has(toks[k].w));
    for (let a = 0; a < toks.length; a++) { if (!cap(a)) continue; const run = [toks[a].w]; let b = a + 1;
      for (; b < toks.length; b++) { const y = toks[b].w; if (cap(b) || /^[IVX]+$/.test(y)) { run.push(y); continue; } if (JOIN.test(y) && toks[b + 1] && cap(b + 1)) { run.push(y); continue; } break; }
      a = b - 1; while (run.length && (TITLE.test(run[0]) || ART.test(run[0]))) run.shift(); while (run.length && JOIN.test(run[run.length - 1])) run.pop();
      if (!run.length || run.length === 1 && run[0].length < 2) continue; const k = run.join(' ').toLowerCase();
      if (notName && k.split(' ').every(x => notName.indexOf(x) >= 0)) continue; out.add('name ' + k); }
    return [...out];
  };
  const known = (set, k) => set.has(k) || (/^year /.test(k) && [...set].some(x => /^(date|month) /.test(x) && x.endsWith('.' + k.slice(5))));
  const factJudge = (T, lang) => {
    const proper = new Set(); T.steps.forEach(s => sents(s.text).forEach(x => (x.match(/[\p{L}][\p{L}'’-]*/gu) || []).forEach((w, i) => { if (i && /^\p{Lu}/u.test(w)) proper.add(w); })));
    const nameW = T.name.toLowerCase().split(/\s+/).filter(w => !ART.test(w)), nameK = keysOf(T.name, lang, new Set(T.name.split(/\s+/)), null);
    const units = []; T.steps.forEach((s, i) => { const item = s.kind === 'Quick Facts' || s.kind === 'Timeline'; sents(s.text).forEach(x => (item ? x.split(/[;,]\s+/) : [x]).forEach(u => units.push({ i, t: u, qf: s.kind === 'Quick Facts', k: keysOf(u, lang, proper, item ? null : nameW) }))); });
    const said = new Set(nameK), twice = [], again = [];
    units.forEach(u => { if (u.i > 0 && u.k.length && u.k.every(k => known(said, k))) twice.push(u.t + ' ' + JSON.stringify(u.k)); u.k.forEach(k => said.add(k)); });
    units.forEach((u, j) => { if (u.qf && u.k.length && units.slice(j + 1).some(v => v.i > u.i && u.k.every(k => v.k.indexOf(k) >= 0))) again.push(u.t); });
    return { twice, again, n: units.filter(u => u.k.length).length };
  };
  const fj = {}; for (const poi in TOURS) for (const l of ['en', 'pt']) fj[poi + ':' + l] = factJudge(TOURS[poi][l], l);
  ok('[facts] no fact twice, the four points, EN and PT (the test\'s own keys: each sentence or list item with a fact brings one not said before; no Quick Facts item told again later)', Object.values(fj).every(x => !x.twice.length && !x.again.length) && Object.values(fj).every(x => x.n >= 2), fj);
  ok('[facts] the page\'s own atCheck agrees (its "facts" rule and every other), the four points, EN and PT', Object.keys(TOURS).every(poi => ['en', 'pt'].every(l => !TOURS[poi][l].checks.length)), Object.keys(TOURS).map(poi => poi + ':' + JSON.stringify(TOURS[poi].en.checks.concat(TOURS[poi].pt.checks))));
  const stepOf = (poi, l, f) => TOURS[poi][l].steps.filter(f)[0] || {};
  const cases = { story: stepOf('painting', 'en', s => s.act === 'open').text, storyPt: stepOf('painting', 'pt', s => s.act === 'open').text, facts: stepOf('painting', 'en', s => s.kind === 'Quick Facts').text,
    tFacts: stepOf('torreao', 'en', s => s.kind === 'Quick Facts').text, tTime: stepOf('torreao', 'en', s => s.kind === 'Timeline').text, vFacts: stepOf('river', 'en', s => s.kind === 'Quick Facts').text,
    any250: TOURS.painting.en.steps.slice(1).some(s => /250|two hundred and fifty/i.test(s.text)) };
  ok('[facts] the named cases: Ribeira\'s story opens on its link alone ("Now the story." / "Agora, a história.": its title\'s 250 years told by the teaser) and its facts on "First, the facts." alone (the 250 years by the teaser, 1 November 1755 by the story); the Torreão\'s facts keep only "Lost 1 November 1755" (the 1580s and Philip I told by its text) and its timeline leaves 1755 out; the Tagus\'s facts leave "River: the Tagus" out',
    cases.story === 'Now the story.' && cases.storyPt === 'Agora, a história.' && cases.facts === 'First, the facts.' && !cases.any250 && cases.tFacts === 'First, the facts. Lost 1 November 1755.' && !!cases.tTime && !/1755/.test(cases.tTime) && !!cases.vFacts && !/River: the Tagus/.test(cases.vFacts), cases);
  // the links: from the block's own words or the neutral one, at most 6 words, none of round 16's generic links, never twice in a row
  const OLD = ['Look at this.', 'Here\'s more.', 'Now, something to see.', 'Take a look.', 'And there\'s this.', 'One more thing.', 'Repare nisto.', 'Há mais.', 'Agora, algo para ver.', 'Veja.', 'E há isto.', 'Só mais uma coisa.'];
  const nn = s => norm(String(s || ''));
  const slot = (tpl, link) => { const [a, b] = tpl.split('{0}'); return link.startsWith(a) && link.endsWith(b) && link.length > a.length + b.length ? link.slice(a.length, link.length - b.length) : null; };
  const linkJudge = (T) => {
    const bad = [], seen = [];
    T.steps.forEach((s, i) => {
      if (!s.link) return; const L = s.link, rest = s.text.slice(L.length).trim(), why = [];
      if (OLD.indexOf(L) >= 0) why.push('a generic link');
      if (words(L) > 6) why.push('over 6 words');
      let from = '';
      if (L === T.neutral) from = 'neutral';
      else if (T.sentence.indexOf(L) >= 0) from = 'its kind\'s sentence';
      else if (L === T.tpl.cap || L === T.tpl.pics) from = (' ' + nn(s.src) + ' ').indexOf(' ' + nn(rest) + ' ') >= 0 ? 'its caption' : '';
      else if (L === T.tpl.dates) from = s.kind === 'Timeline' ? 'its dates' : '';
      else if (slot(T.tpl.year, L) != null || slot(T.tpl.circa, L) != null) { const y = slot(T.tpl.year, L) || slot(T.tpl.circa, L); from = s.kind === 'Timeline' && s.src.indexOf(y) >= 0 ? 'its date' : ''; }
      else if (slot(T.tpl.head, L) != null) { const h = slot(T.tpl.head, L); from = (' ' + nn(s.src) + ' ').indexOf(' ' + nn(h) + ' ') >= 0 ? 'its heading' : ''; }
      if (!from) why.push('not from its block\'s words');
      if (seen.length && seen[seen.length - 1] === L) why.push('twice in a row');
      seen.push(L); if (why.length) bad.push(s.id + ': “' + L + '” ' + why.join(', '));
    });
    return { bad, links: seen };
  };
  const lj = {}; for (const poi in TOURS) for (const l of ['en', 'pt']) lj[poi + ':' + l] = linkJudge(TOURS[poi][l]);
  ok('[links] every link from its block\'s own words (a heading or label, a caption, its date or dates, its kind\'s sentence) or the neutral one ("And now," / "E agora,"), at most 6 words, none of round 16\'s generic links, never twice in a row; the four points, EN and PT',
    Object.values(lj).every(x => !x.bad.length) && new Set(lj['painting:en'].links).size >= 6 && lj['painting:en'].links.indexOf('Now, what was inside:') >= 0 && lj['painting:en'].links.indexOf('Here\'s') >= 0 && lj['square:en'].links.indexOf('And in 1758,') >= 0, lj);
  // the Editor's step list shows each link with its source (Guide › 2 · The tour › Tour from the card, Ribeira From the card)
  await R('S.tours.painting.src="card";S.tab="guide";S.op={};S.op.gt1=true;S.op.s7auto=true;S.ui.level=2;renderAll(false);""');
  await until('!!ed.querySelector(\'[data-fo="s7auto"] .attbl\')', 3000);
  const edl = JSON.parse(await R('JSON.stringify({rows:[].map.call(ed.querySelectorAll(\'[data-fo="s7auto"] .attbl tbody tr\'),function(r){var a=r.querySelector(".atlnk");return a?a.textContent:""}),note:(ed.querySelector(\'[data-fo="s7auto"] .atlist,[data-fo="s7auto"] .note\')||{}).textContent})'));
  const withLink = TOURS.painting.en.steps.map(s => s.link);
  ok('[links] the Editor\'s step list (Guide › 2 · The tour › Tour from the card) shows each step\'s link and its source ("Link “Now, what was inside:” · from its words “What was inside”"; the neutral one says so), and none for a step with no link',
    edl.rows.length === withLink.length && edl.rows.every((t, i) => withLink[i] ? t.indexOf('“' + withLink[i] + '”') >= 0 && /· (from |the neutral link)/.test(t) : !t) && edl.rows.some(t => /from its words “What was inside”/.test(t)), edl.rows);
  tsec('facts and links');

  // ---------- 8. [short] under 2 minutes "a short tour", from 2 up the minutes: the teaser, the hero's button, the invitation, the Editor ----------
  const minsOf = st => Math.max(1, Math.ceil(st.reduce((a, s) => a + Math.max(1, words(s.text)) * 60 / 170 + .7, 0) / 60 - .15));
  const sh = {};
  for (const poi of ['painting', 'torreao', 'square', 'river']) {
    for (const l of ['en', 'pt']) {
      const T = TOURS[poi][l], m = minsOf(T.steps), t0 = T.steps[0].text;
      const tease = m < 2 ? (l === 'en' ? /In a short tour: /.test(t0) : /Numa visita curta: /.test(t0)) && !/\d+ (minutes|minutos)/.test(t0) : t0.indexOf(l === 'en' ? 'In about ' + m + ' minutes: ' : 'Em cerca de ' + m + ' minutos: ') >= 0;
      await R(`S.lang="${l}";if(G.on)stopTour();S.tours.painting.src="card";pointOpen("${poi}");S.view="card";S.stop="full";renderPhone(false);""`);
      await until('!!scr.querySelector(".hero .gstart")', 3000);
      const hb = await R('(scr.querySelector(".hero .gstart")||{}).textContent||""'), page = await R(`tourMinutes("${poi}")`);
      const hero = m < 2 ? hb.endsWith(l === 'en' ? '· short' : '· curta') : hb.endsWith('· ' + m + ' min');
      const nm = T.name, mid = nm.replace(/^(The|O|A|Os|As)(?=\s)/, a => a.toLowerCase());
      const inv = await R(`(function(){S.lang="${l}";var it=Object.assign({},S.guidance.items.tourof,{saysAud:{},saysBack:""});return gdLine(it,false,{point:${JSON.stringify(nm)},min:tourMinutes("${poi}"),n:0})})()`);
      const want = m < 2 ? (l === 'en' ? 'A short tour of ' + mid + '?' : 'Uma visita curta ' + (/^o /.test(mid) ? 'ao ' + mid.slice(2) : /^a /.test(mid) ? 'à ' + mid.slice(2) : /^os /.test(mid) ? 'aos ' + mid.slice(3) : 'a ' + mid) + '?') : (l === 'en' ? nm + ' has a story. A tour of ' + m + ' minutes?' : nm + ' tem uma história. Uma visita de ' + m + ' minutos?');
      sh[poi + ':' + l] = { m, page, tease, hero, hb, inv, ok: tease && hero && inv === want && (l !== 'en' || page === m), want };
    }
  }
  await R('S.lang="en";pointOpen("torreao");S.tab="guide";S.op={};S.op.gt1=true;S.op.s7auto=true;renderAll(false);""'); await until('!!ed.querySelector(\'[data-fo="s7auto"] .attbl\')', 3000);
  const edShort = await R('[].map.call(ed.querySelectorAll(\'[data-fo="s7auto"] .note\'),function(n){return n.textContent}).filter(function(t){return/^The tour made from/.test(t)})[0]||""');
  ok('[short] a tour under 2 minutes is a short tour, from 2 up its minutes (the test\'s own sum, EN and PT): the teaser ("In a short tour: …" / "Numa visita curta: …", else "In about N minutes: …"), the hero\'s button ("· short" / "· curta", else "· N min"), the invitation ("A short tour of the Torreão?" / "Uma visita curta ao Torreão?", else "… A tour of N minutes?"), the Editor\'s list ("a short tour")',
    Object.values(sh).every(x => x.ok) && ['torreao', 'square', 'river'].every(p => sh[p + ':en'].m < 2) && sh['painting:en'].m >= 2 && /a short tour \(under 2 minutes\)/.test(edShort), { bad: Object.keys(sh).filter(k => !sh[k].ok).map(k => [k, sh[k]]), edShort, sh });
  await R('S.lang="en";pointOpen("painting");""');
  tsec('short');

  // ---------- 9. [hero words] the hero's words clear at the teaser at Mini (G15.hero), the four tours and the written g0 ----------
  await load(); await R(base); await R(require('./g15lib.js'));
  const hw = {};
  for (const [poi, src] of [['painting', 'card'], ['torreao', 'card'], ['square', 'card'], ['river', 'card'], ['painting', 'written']]) {
    await R(`if(G.on)stopTour();S.guideCfg.dockStart="mini";S.tours.painting.src="${src}";pointOpen("${poi}");S.view="card";S.stop="full";S.tab="guide";renderAll(false);""`);
    await until('!!scr.querySelector(".hero .gstart")', 3000);
    await p.click('#ph .hero .gstart');
    const atT = await until('G.on&&!!S.guideSteps[G.i]&&S.guideSteps[G.i].act==="teaser"', 4000);
    await R('S.dockH="mini";renderGuide();""');
    await until('G.placedStep===G.i&&performance.now()>(G.autoScrollTill||0)+400&&!!ph.querySelector(".gdock")&&!G15.hero().over.length', 5000);
    const m = JSON.parse(await R('JSON.stringify(Object.assign(G15.hero(),{top:Math.round(scr.scrollTop),step:S.guideSteps[G.i].id,told:G15.overWords().over}))'));
    await R('stopTour();""'); await until('!G.on', 2000);
    const after = JSON.parse(await R('JSON.stringify(G15.hero())'));
    hw[poi + ':' + src] = Object.assign(m, { atT, btnAfter: after.btn });
  }
  ok('[hero words] at the teaser at Mini the hero\'s words (its eyebrow, title, line) are clear of the panel, the bubble and Azu (0 px², G15.hero and the told words), the four tours from the card and the written tour\'s g0; the card scrolled to put them above the guide',
    Object.values(hw).every(x => x.atT && x.n >= 3 && !x.over.length && !x.told.length) && Object.values(hw).some(x => x.top > 0), hw);
  ok('[hero words] while the tour plays the hero\'s Start button is hidden (one focal point: the panel), back once the tour stops', Object.values(hw).every(x => !x.btn && x.btnAfter), Object.keys(hw).map(k => k + ':' + hw[k].btn + '/' + hw[k].btnAfter));
  tsec('hero words');

  // ---------- 10. [crop] each new point's hero: a crop of the wall painting around its own point ----------
  const cr = {};
  for (const poi of ['torreao', 'square', 'river']) for (const stop of ['full', 'half']) {
    await R(`if(G.on)stopTour();pointOpen("${poi}");S.view="card";S.stop="${stop}";renderPhone(false);""`);
    await until('var h=scr.querySelector(".hero");!!h&&Math.abs(h.offsetHeight-S.heroH)<1', 3000);
    const g = JSON.parse(await R(`(function(){var h=scr.querySelector(".hero"),c=S.hero.crop||{},pt=(WALLS[c.wall||"ribeira"].pois||[]).filter(function(x){return x.id===c.at})[0]||{};return JSON.stringify({W:h.offsetWidth,H:h.offsetHeight,size:h.style.backgroundSize,pos:h.style.backgroundPosition,img:getComputedStyle(h).backgroundImage,wall:"url(\\""+WALLS.ribeira.img()+"\\")",ribeira:"url(\\""+IMG.hero+"\\")",iw:WALLS.ribeira.w,ih:WALLS.ribeira.h,c:c,u:pt.u,v:pt.v})})()`));
    // the test's own arithmetic: covering the hero, the crop's height h of the picture at most and its width w (0.4) at most; the point at (fx, fy)
    const h = g.c.h || 1, w = g.c.w || .4, fx = g.c.fx != null ? g.c.fx : .5, fy = g.c.fy != null ? g.c.fy : .4;
    const s = Math.max(g.H / (g.ih * h), g.W / (g.iw * w)), bw = g.iw * s, bh = g.ih * s, cl = (v, lo) => Math.min(0, Math.max(lo, v));
    const ex = cl(fx * g.W - g.u * bw, g.W - bw), ey = cl(fy * g.H - g.v * bh, g.H - bh);
    const sz = (g.size.match(/-?[\d.]+/g) || []).map(Number), m = /clamp\(100%,\s*(?:calc\()?([\d.]+)%\s*\+\s*(-?[\d.]+)px\)?,\s*0px\)\s+(-?[\d.]+)px/.exec(g.pos) || [];
    const px = m.length ? cl(+m[1] / 100 * (g.W - bw) + +m[2], g.W - bw) : NaN, py = +m[3];
    const ptx = px + g.u * bw, pty = py + g.v * bh;
    cr[poi + ':' + stop] = { ok: g.img === g.wall && g.img !== g.ribeira && Math.abs(sz[0] - bw) < 1 && Math.abs(sz[1] - bh) < 1 && Math.abs(px - ex) < 1 && Math.abs(py - ey) < 1 && ptx >= 0 && ptx <= g.W && pty >= 0 && pty <= g.H, size: g.size, pos: g.pos, test: [Math.round(bw), Math.round(bh), Math.round(ex), Math.round(ey)], point: [Math.round(ptx), Math.round(pty)], WH: [g.W, g.H], wallImg: g.img === g.wall };
  }
  const fulls = ['torreao', 'square', 'river'].map(p => cr[p + ':full']);
  const distinct = fulls.every((a, i) => fulls.every((b, j) => i === j || Math.abs(parseFloat(a.pos.split('+')[1]) - parseFloat(b.pos.split('+')[1])) > 20 || a.pos.split(' ').pop() !== b.pos.split(' ').pop()));
  // kept during the teaser: the Torreão's tour started by its hero's button, 3 s into the teaser (the teaser's pictures change every 2.6 s)
  await R('if(G.on)stopTour();pointOpen("torreao");S.view="card";S.stop="full";renderPhone(false);""'); await until('!!scr.querySelector(".hero .gstart")', 3000);
  await p.click('#ph .hero .gstart'); await until('G.on&&!!S.guideSteps[G.i]&&S.guideSteps[G.i].act==="teaser"', 4000);
  const tzAt = await R('performance.now()'); await until(`performance.now()-${tzAt}>3000`, 4000);
  const tz = JSON.parse(await R('var h=scr.querySelector(".hero"),cs=getComputedStyle(h);JSON.stringify({teaser:G.on&&S.guideSteps[G.i].act==="teaser",wall:cs.backgroundImage==="url(\\""+WALLS.ribeira.img()+"\\")",crop:h.classList.contains("hcrop"),anim:cs.animationName})'));
  await R('if(G.on)stopTour();pointOpen("painting");""');
  ok('[crop] the Torreão, the Terreiro do Paço and the Tagus each show a crop of the wall painting around its own point (the wall\'s own file, never the Ribeira\'s hero; its size and offset by the test\'s own arithmetic, the point inside the hero where the crop puts it), at Full and at Half; three different crops; kept during the teaser (its drift around the crop)',
    Object.values(cr).every(x => x.ok) && distinct && tz.wall && tz.crop && tz.anim === 'herocrop', { cr, distinct, tz });
  tsec('crop');

  ok('no page errors', !errs.length, errs.slice(0, 5));
  console.log(fails ? `FAILED ${fails}` : 'ALL PASS');
  await b.close(); process.exit(fails ? 1 : 0);
})().catch(e => { console.error(e); process.exit(2); });
