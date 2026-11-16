// test9sg (Playwright + the installed Edge): node test9sg.js -> PASS/FAIL per check. Guidance round 17b: the second half of test9sf.js
// (it ran 370 s; each file now stays under 300 s): [azu] Azu 0 px over the words after a quick step jump, the repeats 4-6 at g1, g5, g7,
// light and dark (test9sf runs 1-3). The same code and mutation, REP0=3.
process.env.REP0 = process.env.REP0 || '3';
require('./test9sf.js');
