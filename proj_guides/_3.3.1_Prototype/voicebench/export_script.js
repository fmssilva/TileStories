// Exports every line the guide can say to script.json: the Lab's own guideScript() (guide/voice-script.js), the same functions that say
// each line, for the demo points (the Ribeira wall's four points with a card), both languages, every audience: the tours (written and
// from the card) with their after-words, Tell me more and the recaps; the first introduction for each control it may name; the nudges;
// the games on the tour; the remarks and invitations; the tour's end; asking during a game. Live lines (the answers, what is built from the
// visitor's play) are left out: they can never be recorded. Round 19; until then it took the steps and eleven fixed words (106 lines).
// node export_script.js   (uses the local Lab build; Playwright + the installed Edge)
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const fs = require('fs'), path = require('path');
const lab = 'file:///' + path.resolve(__dirname, '../lab_src/block-components-lab.html').replace(/\\/g, '/') + '#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage();
  await p.goto(lab); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 });
  const lines = await p.evaluate(() => LAB.run(`(function(){S.view="card";renderPhone(false);return guideScript().filter(function(x){return!x.live}).map(function(x,i){return{key:"l"+i,lang:x.lang,text:x.text,where:x.where}})})()`));
  const old = fs.existsSync(path.join(__dirname, 'script.json')) ? JSON.parse(fs.readFileSync(path.join(__dirname, 'script.json'), 'utf8')) : [];
  fs.writeFileSync(path.join(__dirname, 'script.json'), JSON.stringify(lines, null, 1));
  console.log('lines', lines.length, '(before', old.length + ')', 'characters', lines.reduce((a, l) => a + l.text.length, 0));
  await b.close();
})();
