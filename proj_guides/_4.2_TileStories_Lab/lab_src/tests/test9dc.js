// test9dc (Playwright + the installed Edge): node test9dc.js -> PASS/FAIL per check. Guidance round 17b: the second half of test9db.js
// (it ran 530 s; each file now stays under 300 s): a press on each row's label of the Guide tab and 6 · Audiences, the second half of the
// rows (test9db presses the first half and the controls' rows), its demo on the phone, its part ringed. The same code, HALF=2.
process.env.HALF = '2';
require('./test9db.js');
