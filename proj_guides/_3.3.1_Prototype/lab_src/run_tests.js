// Runs every test in tests/ (not the *_live ones, which spend a provider's quota) one after another and sums them up.
// node run_tests.js [out.json] [--only=test8i,test9b]  -> one line per test, then the failed checks; exit 1 on any FAIL.
const { spawnSync } = require('child_process'), fs = require('fs'), path = require('path');
const D = __dirname, T = path.join(D, 'tests'), out = process.argv.find(a => a.endsWith('.json')) || path.join(D, 'tests_result.json');
const only = (process.argv.find(a => a.startsWith('--only=')) || '').slice(7).split(',').filter(Boolean);
const tests = fs.readdirSync(T).filter(f => /^test[0-9a-z]+\.js$/.test(f) && !/_live/.test(f)).filter(f => !only.length || only.includes(f.replace('.js', ''))).sort();
const res = {}; let bad = 0;
for (const t of tests) {
  const t0 = Date.now(), r = spawnSync(process.execPath, [path.join(T, t)], { cwd: D, encoding: 'utf8', timeout: 600000 });
  const lines = (r.stdout || '').split(/\r?\n/), pass = lines.filter(l => l.startsWith('PASS')).length, fails = lines.filter(l => l.startsWith('FAIL'));
  const crashed = r.status !== 0 && !fails.length;
  res[t] = { pass, fail: fails.length, failed: fails.map(l => l.slice(5, 140)), crashed: crashed ? ((r.stderr || '') + (r.error ? r.error.message : '')).slice(-400) : '', s: Math.round((Date.now() - t0) / 1000) };
  if (fails.length || crashed) bad++;
  console.log(t.padEnd(12), 'pass', String(pass).padStart(3), 'fail', String(fails.length).padStart(2), crashed ? 'CRASHED' : '', res[t].s + 's');
}
fs.writeFileSync(out, JSON.stringify(res, null, 1));
for (const t in res) { res[t].failed.forEach(f => console.log('  ' + t + ': ' + f)); if (res[t].crashed) console.log('  ' + t + ' crashed: ' + res[t].crashed.replace(/\s+/g, ' ')) }
console.log(bad ? bad + ' test file(s) with failures' : 'all ' + tests.length + ' test files green');
process.exit(bad ? 1 : 0);
