// test10g (Node, no browser): node tests/test10g.js -> PASS/FAIL per check. Merge round P1b.1: the token table (core/theme.js's
// D.theme) against Unity's theme, read from the Unity project (Theme.default.uss and the asset's knobs: block).
// [unity] every token not flagged proposed equals the .uss, light and dark (the 163 of each mode rule), the four font tokens name
//   the asset's faces, the theme's own Default Mode is the asset's; [count] no Unity token missing, none extra without the flag;
// [accents] the four accent options equal the asset's knobs (x 255, rounded as ThemeUssGenerator does);
// [proposed] every proposed token is absent from the .uss and every proposed knob option from the asset, so this test says when
//   Unity adds one (the flag then comes off);
// [contrast] composed in linear light (ThemeContrastTests' LinearOver): Premium and Playful x light and dark x every accent option:
//   body text (ink, ink-2) 7:1 on every ground, other text 4.5:1, the accent's own words on it 4.5:1, the hero's words over its
//   scrim over pure white 7:1, the app glass's words over its glass over white and black 4.5:1;
// [resolve] the look's picks (Playful: cobalt, round, solid) win over the wall's; the wall's pick shows under Premium.
// MUTATE=1: heritage's dark accent back to the Lab's old blue, a Unity token flagged proposed, a token dropped, ink-2 light greyed:
//   [unity], [count], [accents], [proposed] and [contrast] turn red.
const fs = require('fs'), path = require('path'), vm = require('vm');
const LAB = path.join(__dirname, '..'), FW = path.join(LAB, '..', '..', '..', 'TileStories', 'Assets', 'Framework');
const USS = fs.readFileSync(path.join(FW, 'Runtime', 'UI', 'Theme', 'Generated', 'Theme.default.uss'), 'utf8');
const ASSET = fs.readFileSync(path.join(FW, 'Runtime', 'Resources', 'TileStories', 'Themes', 'Theme.default.asset'), 'utf8');
let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
const T0 = Date.now();

// ---------- the Lab's table: core/theme.js run as the page runs it (its D, P and on stand in) ----------
const cx = { D: { wall: {} }, P: { vis: {} }, on() {}, console };
vm.createContext(cx); vm.runInContext(fs.readFileSync(path.join(LAB, 'src', 'core', 'theme.js'), 'utf8'), cx);
const D = cx.D;
if (process.env.MUTATE) {
  const set = (list, name, f) => { const t = list.find(x => x.name === name); f(t, list) };
  set(D.theme.knobs.find(o => o.knob === 'accent' && o.option === 'heritage').modes[1].tokens.colours, '--ts-accent', t => t.value = '#8FB0EC');
  set(D.theme.modes[0].tokens.colours, '--ts-ink-2', t => t.value = '#999999');
  set(D.theme.modes[0].tokens.colours, '--ts-line', t => t.proposed = true);
  set(D.theme.modes[1].tokens.colours, '--ts-metal-hi', (t, l) => l.splice(l.indexOf(t), 1));
  console.log('MUTATION: heritage dark = the old blue, ink-2 light grey, --ts-line flagged proposed, --ts-metal-hi dropped from dark');
}

// ---------- Unity's side ----------
// the .uss: one rule per mode, the mode-free font rule
const rule = sel => { const m = USS.match(new RegExp(sel.replace(/\./g, '\\.') + '\\s*\\{([\\s\\S]*?)\\n\\}')); const o = {};
  m[1].replace(/\/\*[\s\S]*?\*\//g, '').replace(/(--ts-[a-z0-9-]+):\s*([^;]+);/g, (_, n, v) => { o[n] = v.trim() }); return o };
const UNITY = { light: rule('.ts-app.ts-theme-default.ts-mode-light'), dark: rule('.ts-app.ts-theme-default.ts-mode-dark') };
const FONTS = rule('.ts-app.ts-theme-default');
// the asset's knobs (YAML floats x 255, rounded) and its defaultMode / type keys
const knobsTxt = ASSET.slice(ASSET.indexOf('  knobs:'), ASSET.indexOf('  type:'));
const UKNOBS = knobsTxt.split(/\n  - knob: /).slice(1).map(b => {
  const o = { knob: b.split('\n')[0].trim(), option: (b.match(/option: (\w+)/) || [])[1], modes: {} };
  ['light', 'dark'].forEach(mk => { const part = b.slice(b.indexOf('- key: ' + mk)); const end = part.indexOf('lengths:'); o.modes[mk] = {};
    part.slice(0, end).replace(/name: (--ts-[a-z-]+)\s+value: \{r: ([0-9.]+), g: ([0-9.]+), b: ([0-9.]+), a: ([0-9.]+)\}/g, (_, n, r, g, bb, a) => {
      o.modes[mk][n] = 'rgb' + (+a < 1 ? 'a' : '') + '(' + [r, g, bb].map(x => Math.round(+x * 255)).join(', ') + (+a < 1 ? ', ' + a : '') + ')' }) });
  return o });
const yaml = k => (ASSET.match(new RegExp('^\\s+' + k + ': (\\S+)', 'm')) || [])[1];

// ---------- the Lab's values as Unity writes them ----------
const css = (v, kind) => kind === 'colours' ? cx.themeCss(v, kind) : kind === 'numbers' ? String(v) : String(v);
const labMode = mk => { const o = {}, lay = (set, flag) => cx.THEME_KINDS.forEach(k => (set && set[k] || []).forEach(t => { o[t.name] = { v: css(t.value, k), proposed: !!t.proposed } }));
  lay(D.theme.shared); lay(D.theme.modes.find(m => m.key === mk).tokens); return o };
const LABT = { light: labMode('light'), dark: labMode('dark') };
const norm = v => String(v).replace(/\s+/g, '').replace(/^rgba\((\d+),(\d+),(\d+),1\)$/, 'rgb($1,$2,$3)');

// ---------- [unity] [count] ----------
['light', 'dark'].forEach(mk => {
  const u = UNITY[mk], l = LABT[mk], uk = Object.keys(u);
  const off = uk.filter(n => l[n] && !l[n].proposed && norm(l[n].v) !== norm(u[n])).map(n => n + ' lab ' + l[n].v + ' unity ' + u[n]);
  ok(`[unity] ${mk}: every token equals Theme.default.uss (${uk.length} in its rule)`, uk.length === 163 && off.length === 0, off.slice(0, 6));
  const missing = uk.filter(n => !l[n]), flaggedUnity = uk.filter(n => l[n] && l[n].proposed), extra = Object.keys(l).filter(n => !u[n] && !l[n].proposed);
  ok(`[count] ${mk}: no Unity token missing from the table`, missing.length === 0, missing);
  ok(`[count] ${mk}: no Unity token flagged proposed, no Lab token unflagged that Unity lacks`, flaggedUnity.length === 0 && extra.length === 0, { flaggedUnity, extra });
});
const fontFile = n => (FONTS[n] || '').replace(/.*\/([^/]+?)( UITK)?( Halo)?\.asset.*/, '$1');
const ty = D.theme.type;
ok('[unity] the four font tokens: body and display name the asset\'s faces (and its type keys)',
  fontFile('--ts-font-body') === cx.THEME_FONTS[ty.bodyFont].file && fontFile('--ts-font-display') === cx.THEME_FONTS[ty.displayFont].file &&
  fontFile('--ts-font-body-halo') === cx.THEME_FONTS[ty.bodyFont].file && fontFile('--ts-font-display-halo') === cx.THEME_FONTS[ty.displayFont].file &&
  ['displayFont', 'bodyFont', 'bodyMediumFont', 'bodyBoldFont'].every(k => yaml(k) === ty[k]) && +yaml('displayWeight') === ty.displayWeight,
  [fontFile('--ts-font-body'), fontFile('--ts-font-display'), ty]);
ok('[unity] the theme\'s own Default Mode is the asset\'s', yaml('defaultMode') === D.theme.defaultMode, [yaml('defaultMode'), D.theme.defaultMode]);

// ---------- [accents] ----------
const uAcc = UKNOBS.filter(o => o.knob === 'accent'), lAcc = D.theme.knobs.filter(o => o.knob === 'accent' && !o.proposed);
ok('[accents] the asset holds four accent options and the Lab the same four, unflagged, in its order', uAcc.length === 4 && JSON.stringify(uAcc.map(o => o.option)) === JSON.stringify(lAcc.map(o => o.option)), [uAcc.map(o => o.option), lAcc.map(o => o.option)]);
uAcc.forEach(u => {
  const l = lAcc.find(o => o.option === u.option), off = [];
  ['light', 'dark'].forEach(mk => Object.keys(u.modes[mk]).forEach(n => { const t = l && l.modes.find(m => m.key === mk).tokens.colours.find(x => x.name === n);
    if (!t || norm(cx.themeCss(t.value, 'colours')) !== norm(u.modes[mk][n])) off.push(mk + ' ' + n + ' lab ' + (t && t.value) + ' unity ' + u.modes[mk][n]) }));
  ok(`[accents] ${u.option}: accent, accent-text and on-accent equal the asset's, light and dark`, !!l && off.length === 0, off);
});
ok('[accents] the theme\'s own accent is heritage\'s (the modes and the list cannot drift)', ['light', 'dark'].every((mk, i) =>
  D.theme.modes[i].tokens.colours.find(t => t.name === '--ts-accent').value === D.theme.knobs.find(o => o.knob === 'accent' && o.option === 'heritage').modes[i].tokens.colours.find(t => t.name === '--ts-accent').value));

// ---------- [proposed] ----------
const proposedTokens = new Set();
const walk = set => cx.THEME_KINDS.forEach(k => (set && set[k] || []).forEach(t => { if (t.proposed) proposedTokens.add(t.name) }));
walk(D.theme.shared); D.theme.modes.forEach(m => walk(m.tokens)); D.theme.knobs.forEach(o => { walk(o.shared); o.modes.forEach(m => walk(m.tokens)) });
const inUss = [...proposedTokens].filter(n => USS.indexOf(n + ':') >= 0);
ok(`[proposed] every proposed token (${proposedTokens.size}) is absent from Theme.default.uss`, proposedTokens.size >= 29 && inUss.length === 0, inUss);
const pOpts = D.theme.knobs.filter(o => o.proposed), inAsset = pOpts.filter(o => UKNOBS.some(u => u.knob === o.knob && u.option === o.option));
ok(`[proposed] every proposed knob option (${pOpts.length}: ${[...new Set(pOpts.map(o => o.knob))].join(', ')}) is absent from the asset's knobs`, pOpts.length >= 11 && inAsset.length === 0, inAsset.map(o => o.knob + ':' + o.option));
ok('[proposed] every unflagged knob option is in the asset (no Lab-only option without the flag)', D.theme.knobs.filter(o => !o.proposed).every(o => UKNOBS.some(u => u.knob === o.knob && u.option === o.option)));
ok('[proposed] Playful\'s rounded face is a proposed font key, not in the asset', cx.THEME_FONTS.fredoka_semibold.proposed && ASSET.indexOf('fredoka') < 0);

// ---------- [contrast] in linear light ----------
const BODY = ['--ts-ink', '--ts-ink-2'], OTHER = ['--ts-ink-3', '--ts-accent', '--ts-accent-text', '--ts-correct', '--ts-wrong'];
const GROUNDS = ['--ts-surface', '--ts-surface-2', '--ts-bg', '--ts-bar', '--ts-sheet-open', '--ts-panel', '--ts-tonal'];
const accents = D.theme.knobs.filter(o => o.knob === 'accent').map(o => o.option);
const bad = [], WHITE = [255, 255, 255, 1], BLACK = [0, 0, 0, 1];
let pairs = 0;
const need = (tag, fg, bg, bar, base) => { pairs++; const r = cx.themeContrast(cx.themeRgba(fg), cx.themeRgba(bg), base); if (!(r >= bar)) bad.push(tag + ' ' + r.toFixed(2) + ' (needs ' + bar + ')') };
['premium', 'playful'].forEach(look => ['light', 'dark'].forEach(mk => accents.forEach(acc => {
  const v = cx.themeResolve(mk, { look, accent: acc }), tag = look + '/' + mk + '/' + acc + ': ';
  GROUNDS.forEach(g => { BODY.forEach(f => need(tag + f + ' on ' + g, v[f], v[g], 7)); OTHER.forEach(f => need(tag + f + ' on ' + g, v[f], v[g], 4.5)) });
  need(tag + '--ts-on-accent on --ts-accent', v['--ts-on-accent'], v['--ts-accent'], 4.5);
  ['--ts-on-scrim', '--ts-on-scrim-2'].forEach(f => need(tag + f + ' on --ts-scrim-hero over white', v[f], v['--ts-scrim-hero'], 7, WHITE));
  need(tag + '--ts-wall-label-ink on --ts-wall-label over white', v['--ts-wall-label-ink'], v['--ts-wall-label'], 4.5, WHITE);
  need(tag + '--ts-wall-label-ink on --ts-wall-label over black', v['--ts-wall-label-ink'], v['--ts-wall-label'], 4.5, BLACK);
  ['--ts-ink', '--ts-ink-2'].forEach(f => { need(tag + f + ' on --ts-app-glass over white', v[f], v['--ts-app-glass'], 4.5, WHITE); need(tag + f + ' on --ts-app-glass over black', v[f], v['--ts-app-glass'], 4.5, BLACK) });
})));
ok(`[contrast] linear light: Premium and Playful x light and dark x ${accents.length} accents, ${pairs} pairs: body 7:1, other text 4.5:1`, bad.length === 0 && pairs > 700, [...new Set(bad.map(b => b.replace(/\/(heritage|azulejo|terracotta|garden|cobalt):/, '/*:').replace(/ [0-9.]+ \(needs.*/, '')))]);
// the arithmetic itself (the linear blend, the ratio): black on white is 21:1; 50 % black over white in linear light is not sRGB's grey
const mid = cx.themeOver([0, 0, 0, 0.5], WHITE);
ok('[contrast] the helpers: black on white 21:1, a colour on itself 1:1, 50 % black over white blends in linear light (188, not sRGB\'s 128)',
  Math.abs(cx.themeContrast(BLACK, WHITE) - 21) < 0.01 && Math.abs(cx.themeContrast(WHITE, WHITE) - 1) < 1e-9 && mid[0] === 188, mid);

// ---------- [resolve] ----------
D.wall.theme.accent = 'garden'; D.wall.theme.roundness = 'sharp'; D.wall.theme.look = 'premium';
let p = cx.themePicks();
ok('[resolve] Premium: the wall\'s picks show (garden, sharp)', p.look === 'premium' && p.accent === 'garden' && p.roundness === 'sharp' && cx.themeResolve('light', p)['--ts-accent'] === '#174F33' && cx.themeResolve('light', p)['--ts-radius-m'] === '8px', p);
D.wall.theme.look = 'playful'; p = cx.themePicks();
const pv = cx.themeResolve('dark', p);
ok('[resolve] Playful picks its own (cobalt, round, solid) over the wall\'s; its dark accent, radii, glass, face and text sizes', p.accent === 'cobalt' && p.roundness === 'round' && p.glass === 'solid' &&
  pv['--ts-accent'] === '#FFC83D' && pv['--ts-radius-m'] === '16px' && pv['--ts-app-glass'] === '#1A1B1E@0.98' && pv['--ts-size-body'] === '17px' && pv['--ts-ink'] === '#FFF8EC' &&
  cx.themeOption('look', 'playful').type.displayFont === 'fredoka_semibold', [p, pv['--ts-accent'], pv['--ts-radius-m'], pv['--ts-app-glass'], pv['--ts-size-body']]);
console.log('TIME ' + Math.round((Date.now() - T0) / 1000) + ' s');
console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); process.exit(fails ? 1 : 0);
