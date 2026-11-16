// Playwright for the tests: package.json's devDependency playwright-core (npm install, once, in lab_src/); PW=<path> names another
// copy. The tests launch the installed Edge (executablePath), so no browser is downloaded. const { chromium } = require('./pw');
try { module.exports = require(process.env.PW || 'playwright-core') } catch (e) {
  console.log('The tests need Playwright: run "npm install" in ' + require('path').dirname(__dirname) + ' (package.json), or set PW to a copy of it. ' + e.message);
  process.exit(2);
}
