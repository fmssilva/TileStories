// Exports every line the guide speaks (each step per language and audience, and the guide's fixed words) to script.json.
// node export_script.js   (uses the local Lab build; Playwright + the installed Edge)
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const lab = 'file:///' + path.resolve(__dirname, '../lab_src/block-components-lab.html').replace(/\\/g, '/') + '#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage();
  await p.goto(lab); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 });
  const lines = await p.evaluate(() => LAB.run(`(function(){var out=[],seen={},add=function(key,lang,text){text=String(text||"").trim();if(!text)return;var k=key+"|"+lang;if(seen[k])return;seen[k]=1;out.push({key:key,lang:lang,text:text})};
    var keepL=S.lang,keepA=S.vis.aud,steps=S.fullSteps||S.guideSteps;
    ["en","pt"].forEach(function(lg){S.lang=lg;S.view="card";renderPhone(false);AUDS.forEach(function(a){S.vis.aud=a;ovMap();steps.forEach(function(st){add(st.id+"_"+a,lg,stepText(st));if(st.after)add(st.id+"_after_"+a,lg,L(st.after))})});
      S.vis.aud=keepA;["guide_intro","guide_moving_on","guide_nudge_flip_1","guide_nudge_flip_2","guide_nudge_drag_1","guide_nudge_drag_2","guide_nudge_answer_1","guide_nudge_answer_2","guide_try_moveon","guide_after_flip","guide_after_answer"].forEach(function(k){add(k,lg,k==="guide_intro"?fmt(TXT(k),pointName()||"Ribeira Palace"):TXT(k))})});
    S.lang=keepL;S.vis.aud=keepA;ovMap();renderPhone(false);return out})()`));
  fs.writeFileSync(path.join(__dirname, 'script.json'), JSON.stringify(lines, null, 1));
  console.log('lines', lines.length, 'characters', lines.reduce((a, l) => a + l.text.length, 0));
  await b.close();
})();
