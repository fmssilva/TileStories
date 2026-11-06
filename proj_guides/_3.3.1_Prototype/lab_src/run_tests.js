// Runs the test files in tests/ (not the *_live ones, which spend a provider's quota) and sums them up (README.md, "Prove it").
// node run_tests.js [out.json] [--only=test8i,test9b] [--changed[=<git rev>]]  -> one line per test, then the failed
// checks; exit 1 on any FAIL.
//   --changed  the block's tests: the test files PROTOTYPE_STRUCTURE.md names for every module changed since <rev> (default
//              HEAD: the work not yet committed) and every changed or new test file. A changed module the map names no test
//              for is shared ground: the line "SHARED GROUND" says the block closes on level 2.
// One file at a time: each drives its own Edge with WebGL in software, so two at once slow each other past their timings
// (measured 2026-11-06: 2 at once 25 min with crashes, 3 at once 17 min with 11 red files; one at a time 28 min, green).
// A file that crashed at its start (the page not ready in time) runs once more at the end.
const { spawn, execSync } = require('child_process'), fs = require('fs'), path = require('path');
const D = __dirname, T = path.join(D, 'tests'), arg = k => (process.argv.find(a => a === '--' + k || a.startsWith('--' + k + '=')) || null);
const out = process.argv.find(a => a.endsWith('.json')) || path.join(D, 'tests_result.json');
const val = k => { const a = arg(k); return a && a.includes('=') ? a.slice(a.indexOf('=') + 1) : null };
const all = fs.readdirSync(T).filter(f => /^test[0-9a-z]+\.js$/.test(f) && !/_live/.test(f)).sort();
let pick = (val('only') || '').split(',').filter(Boolean).map(t => t.replace(/\.js$/, '') + '.js');
const SHARED_FILES = /^(build\.py|check_structure\.js|run_tests\.js|src\/page\/)/;
if (arg('changed')) {
  const rev = val('changed') || 'HEAD', rel = 'proj_guides/_3.3.1_Prototype/lab_src/';
  const git = c => execSync('git ' + c, { cwd: D, encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore'] }).split(/\r?\n/).filter(Boolean);
  const top = git('rev-parse --show-toplevel')[0];
  const files = [...new Set(git('diff --name-only ' + rev).concat(git('ls-files --others --exclude-standard').map(f => path.relative(top, path.join(D, f)).replace(/\\/g, '/'))))]
    .filter(f => f.startsWith(rel)).map(f => f.slice(rel.length));
  // the structure map: | `area/module.js` | ... | tests | (the last cell names the module's test files)
  const map = {};
  fs.readFileSync(path.join(D, 'PROTOTYPE_STRUCTURE.md'), 'utf8').split(/\r?\n/).forEach(l => {
    const m = l.match(/^\| `([a-z-]+\/[a-z0-9-]+)\.js` \|/); if (!m) return;
    const cells = l.split('|').map(c => c.trim()).filter((c, i, a) => i > 0 && i < a.length - 1);
    map[m[1]] = (cells[cells.length - 1].match(/test[0-9][0-9a-z]*/g) || []).map(t => t + '.js');
  });
  const tests = new Set(), shared = [], unmapped = [];
  files.forEach(f => {
    const t = f.match(/^tests\/(test[0-9a-z]+\.js)$/); if (t) { if (all.includes(t[1])) tests.add(t[1]); return }
    const m = f.match(/^src\/([a-z-]+\/[a-z0-9-]+)\.(js|css)$/);
    if (m && map[m[1]]) { if (map[m[1]].length) map[m[1]].forEach(x => all.includes(x) && tests.add(x)); else shared.push(f); return }
    if (m || SHARED_FILES.test(f)) (m ? unmapped : shared).push(f);
  });
  console.log('changed since ' + rev + ': ' + (files.length ? files.join(', ') : 'nothing'));
  if (unmapped.length) console.log('NOT IN THE MAP (add its row to PROTOTYPE_STRUCTURE.md): ' + unmapped.join(', '));
  if (shared.length || unmapped.length) console.log('SHARED GROUND: ' + shared.concat(unmapped).join(', ') + ' -> this block closes on level 2 (README.md, "Levels")');
  pick = pick.concat([...tests]);
  if (!pick.length) { console.log('no test file for this change'); process.exit(shared.length || unmapped.length ? 1 : 0) }
}
const tests = all.filter(f => !pick.length || pick.includes(f));
const res = {};
const run = t => new Promise(done => {
  const t0 = Date.now(), p = spawn(process.execPath, [path.join(T, t)], { cwd: D }); let so = '', se = '';
  const kill = setTimeout(() => p.kill(), 600000);
  p.stdout.on('data', d => so += d); p.stderr.on('data', d => se += d);
  p.on('close', code => {
    clearTimeout(kill);
    const lines = so.split(/\r?\n/), pass = lines.filter(l => l.startsWith('PASS')).length, fails = lines.filter(l => l.startsWith('FAIL'));
    const crashed = code !== 0 && !fails.length;
    res[t] = { pass, fail: fails.length, failed: fails.map(l => l.slice(5, 140)), crashed: crashed ? se.slice(-400) : '', s: Math.round((Date.now() - t0) / 1000) };
    console.log(t.padEnd(12), 'pass', String(pass).padStart(3), 'fail', String(fails.length).padStart(2), crashed ? 'CRASHED' : '', res[t].s + 's');
    done();
  });
});
(async () => {
  const t0 = Date.now();
  for (const t of tests) await run(t);
  // a start crash (the page not ready in time) runs once more
  for (const t of tests.filter(t => res[t].crashed && /(page\.goto|waitForFunction): Timeout/.test(res[t].crashed))) { console.log(t + ': crashed at its start, run again'); await run(t); res[t].retried = true }
  fs.writeFileSync(out, JSON.stringify(res, null, 1));
  let bad = 0;
  for (const t of tests) { if (res[t].fail || res[t].crashed) bad++; res[t].failed.forEach(f => console.log('  ' + t + ': ' + f)); if (res[t].crashed) console.log('  ' + t + ' crashed: ' + res[t].crashed.replace(/\s+/g, ' ')) }
  console.log((bad ? bad + ' test file(s) with failures' : 'all ' + tests.length + ' test files green') + ' (' + Math.round((Date.now() - t0) / 60000) + ' min)');
  process.exit(bad ? 1 : 0);
})();
