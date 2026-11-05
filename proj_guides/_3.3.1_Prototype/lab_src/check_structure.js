// The Lab's structure check (PROTOTYPE_STRUCTURE.md, "Rules"): node check_structure.js [--write-baseline]
// Counts, over the modules in build.py's order:
//   wraps     - a function saved and replaced by one that calls it (var fOld=f; f=function(){...fOld()...})
//   replaces  - a function assigned again by a later module
//   redeclared - a function declared in more than one module
//   defaults  - an S.* value set at the top level of more than one module
// structure_baseline.json holds the allowed counts: a count may only go down (the restructure drives them all to 0).
const fs = require('fs'), path = require('path');
const acorn = require(process.env.ACORN || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/acorn');
const D = __dirname, BASE = path.join(D, 'structure_baseline.json');
const py = fs.readFileSync(path.join(D, 'build.py'), 'utf8');
const MODULES = JSON.parse(py.match(/MODULES = (\[[\s\S]*?\])/)[1].replace(/'/g, '"'));
const fn = {}, st = {}, errors = [];
const F = n => (fn[n] = fn[n] || { def: [], wrap: [], replace: [] });
const mpath = n => n.type === 'Identifier' ? n.name : n.type === 'MemberExpression' && !n.computed ? ((o => o && o + '.' + n.property.name)(mpath(n.object))) : null;
const isFn = n => n && (n.type === 'FunctionExpression' || n.type === 'ArrowFunctionExpression');
// the modules share one function scope (the first opens it, the last closes it): parse the joined code, then attribute by offset
let joined = '', starts = [];
MODULES.forEach(m => { starts.push([joined.length, m]); joined += fs.readFileSync(path.join(D, m), 'utf8') + '\n' });
const owner = pos => { let o = starts[0][1]; for (const [s, m] of starts) { if (s <= pos) o = m; else break } return o };
let ast; try { ast = acorn.parse(joined, { ecmaVersion: 'latest', sourceType: 'script', allowReturnOutsideFunction: true }) } catch (e) { console.log('PARSE ERROR', e.message, 'in', owner(e.pos)); process.exit(2) }
let body = ast.body; const iife = body.find(s => s.type === 'ExpressionStatement' && s.expression.type === 'CallExpression' && isFn(s.expression.callee));
if (iife) body = iife.expression.callee.body.body;
const saved = {};
const visit = list => list.forEach(s => {
  const m = owner(s.start);
  if (s.type === 'FunctionDeclaration') { F(s.id.name).def.push(m); return }
  if (s.type === 'VariableDeclaration') s.declarations.forEach(d => {
    if (d.init && d.init.type === 'Identifier' && d.id.name !== d.init.name && d.id.name.indexOf(d.init.name) === 0) saved[d.init.name] = m;
    if (isFn(d.init)) F(d.id.name).def.push(m) });
  if (s.type === 'ExpressionStatement') (s.expression.type === 'SequenceExpression' ? s.expression.expressions : [s.expression]).forEach(x => {
    if (x.type === 'CallExpression' && isFn(x.callee) && x.callee.body.type === 'BlockStatement') { visit(x.callee.body.body); return }
    if (x.type !== 'AssignmentExpression' || x.operator !== '=') return; const p = mpath(x.left); if (!p) return;
    if (x.left.type === 'Identifier' && isFn(x.right)) { if (saved[p]) { F(p).wrap.push(m); delete saved[p] } else F(p).replace.push(m); return }
    if (p.indexOf('S.') === 0) (st[p] = st[p] || new Set()).add(m) });
});
visit(body);
const list = k => Object.keys(fn).filter(n => k(fn[n]));
const now = {
  wraps: list(f => f.wrap.length).reduce((a, n) => a + fn[n].wrap.length, 0),
  replaces: list(f => f.replace.length).reduce((a, n) => a + fn[n].replace.length, 0),
  redeclared: list(f => f.def.length > 1).length,
  defaults: Object.keys(st).filter(p => st[p].size > 1).length,
};
if (process.argv.includes('--write-baseline')) { fs.writeFileSync(BASE, JSON.stringify(now, null, 1) + '\n'); console.log('baseline written', JSON.stringify(now)); process.exit(0) }
const base = fs.existsSync(BASE) ? JSON.parse(fs.readFileSync(BASE, 'utf8')) : { wraps: 0, replaces: 0, redeclared: 0, defaults: 0 };
let bad = 0;
for (const k in now) { const up = now[k] > base[k]; if (up) bad++; console.log((up ? 'FAIL ' : 'ok   ') + k.padEnd(11) + now[k] + ' (allowed ' + base[k] + ')') }
if (process.argv.includes('--list')) {
  list(f => f.def.length + f.wrap.length + f.replace.length > 1).forEach(n => console.log('  ' + n + ': def[' + fn[n].def + '] wrap[' + fn[n].wrap + '] replace[' + fn[n].replace + ']'));
  Object.keys(st).filter(p => st[p].size > 1).forEach(p => console.log('  ' + p + ' set in ' + [...st[p]].join(' ')));
}
if (bad) console.log('The structure got worse: a wrapper, a re-assignment, a second declaration or a default set twice was added. See PROTOTYPE_STRUCTURE.md, "Rules".');
process.exit(bad ? 1 : 0);
