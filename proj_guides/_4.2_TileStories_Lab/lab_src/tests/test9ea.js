// test9ea (Playwright + the installed Edge): node test9ea.js -> PASS/FAIL per check. Guidance round 17b: the second half of test9e.js
// (it ran 415 s; each file now stays under 300 s): its sections 10 and 11, the Guide tab's sections and every former 6 · Guide row, 4 ·
// Guidance's rows, defaults and each row's demo (test9e keeps Preview & Test and the scenes, with its [Done when] scene). The same code,
// PART=2; MUTATE=1 as test9e.
process.env.PART = '2';
require('./test9e.js');
