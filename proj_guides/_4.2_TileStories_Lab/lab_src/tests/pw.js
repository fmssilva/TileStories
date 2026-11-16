// Playwright for the tests: package.json's devDependency playwright-core (npm install, once, in lab_src/); PW=<path> names another
// copy. The tests launch the installed Edge (executablePath), so no browser is downloaded.
//   const { chromium, labUrl } = require('./pw');  const url = labUrl('#probe');
// labUrl(hash): the page over the Lab's local http server (tools/serve.py, http://localhost:$LAB_PORT/lab_src/..., LAB_PORT default 8642: one port per copy of the Lab), started first if it
// is not running: over http the page loads its assets/ (the 3D mascot's models) and can read a picture back from a canvas.
const path = require('path'), { execFileSync } = require('child_process');
let pw;
try { pw = require(process.env.PW || 'playwright-core') } catch (e) {
  console.log('The tests need Playwright: run "npm install" in ' + path.dirname(__dirname) + ' (package.json), or set PW to a copy of it. ' + e.message);
  process.exit(2);
}
const SERVE = path.join(__dirname, '..', 'tools', 'serve.py'), PORT = Number(process.env.LAB_PORT) || 8642;
let served = false;
function labUrl(hash) {
  if (!served) {
    try { execFileSync(process.env.PYTHON || 'python', [SERVE, '--ensure'], { stdio: 'pipe' }) } catch (e) {
      console.log('The Lab server did not start (python tools/serve.py --ensure): ' + String(e.stdout || e.message).trim()); process.exit(2);
    }
    served = true;
  }
  return 'http://localhost:' + PORT + '/lab_src/block-components-lab.html' + (hash || '');
}
module.exports = Object.assign(Object.create(pw), { labUrl });
