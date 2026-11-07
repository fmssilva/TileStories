// v8t checks (Playwright + the installed Edge): node test8t.js -> PASS/FAIL per check. The model is a test double, or fetch is faked: no network
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(500);
  const R = src => p.evaluate(s => LAB.run(s), src);
  const W = ms => p.waitForTimeout(ms);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.guideMute=true;S.introSeen=true;S.agentCfg.voiceOut=false;gdGroupById("inview").on=false;S.view="card";S.stop="full";renderAll(false)');
  await W(300);
  // 1. what answers here: nothing live in the headless Lab (no Claude, no key), so offline
  ok('Nothing live here: no model, the Editor says offline answers', await R('S.tab="guide";renderEditor();agProvider()===null&&!AG.fn&&/nothing live here/.test(ed.textContent)'));
  // 2. the test double becomes the page's model through the same adapter
  await R('AG.mock={text:"ok"};S.agentCfg.provider="mock";applyProvider();renderEditor()');
  ok('A model this page can reach becomes the page\'s model, behind the adapter', await R('sampleFn===AG.fn&&agLive()&&/Test double/.test(ed.textContent)'));
  // 3. a question on the card: the agent acts (Auto), cites and offers link cards
  const card = 'startTour();pauseTour();G.askOpen=true;S.dockH="full";renderGuide();';
  await R(card + 'AG.mock={calls:[{name:"navigate_to",args:{target_id:"b:b_fun"}},{name:"show_links",args:{items:[{target_id:"b:b_ba",reason:"Before 1755"},{target_id:"b:b_gal",reason:"Three centuries"}]}}],text:"The palace had an opera house, opened in 1755 [src:b:b_fun]. A fake one [src:b:nope]. See the [poi:painting|Ribeira Palace]."};ask("Was there an opera?")');
  await W(400);
  const c1 = await R('var t=G.agTurn;[!!(t&&t.links.length===2),t&&t.cites.join(),!!(scr.querySelector(\'[data-ref="b:b_fun"]\')||{classList:{contains:function(){}}}).classList.contains("gfocus"),ph.querySelectorAll(".gd-thread .aglink").length,(ph.querySelector(".gd-thread .agsrc")||{}).textContent,t&&t.text]');
  ok('Card: Go to (Auto) brings the Fun fact block into focus', c1[2], c1);
  ok('...two link cards under the answer, in the conversation', c1[0] && c1[3] === 2, c1);
  ok('...the citations become a source line (a point too, as models write it: [poi:id|name]); one that does not exist is dropped; the tags leave the text', c1[1] === 'b:b_fun,poi:painting' && /Fun fact/.test(c1[4] || '') && !/\[/.test(c1[5]) && /See the Ribeira Palace\.$/.test(c1[5]), c1);
  await p.locator('#ph .gd-thread [data-aglink="b:b_ba"]').click(); await W(300);
  ok('...a tap on a link card goes there (the before and after)', await R('scr.querySelector(\'[data-ref="b:b_ba"]\').classList.contains("gfocus")'));
  ok('...the conversation ends on the cards (scrolled to the bottom)', await R('var th=ph.querySelector(".gd-thread");th.scrollHeight-th.clientHeight-th.scrollTop<2'));
  await R('S.lang="pt";AG.mock={calls:[{name:"show_links",args:{items:[{target_id:"b:b_ba"}]}}],text:"Veja [src:b:b_ba]."};ask("Antes?")'); await W(400);
  ok('...in Portuguese, the cards and the source line name the blocks in Portuguese', await R('var a=ph.querySelectorAll(".gd-thread .aglink"),b=ph.querySelectorAll(".gd-thread .agsrc");/Antes e depois/.test(a[a.length-1].textContent)&&/Fonte: Antes e depois/.test(b[b.length-1].textContent)'));
  await R('S.lang="en"');
  // 4. Suggest: offered as a button, done only on a tap
  await R('AG.mock={calls:[{name:"open_feature",args:{kind:"gallery"}}],text:"I can open the gallery."};ask("Pictures?")'); await W(400);
  const s1 = await R('[scr.querySelector(\'[data-ref="b:b_gal"]\').classList.contains("gfocus"),(ph.querySelector(".gd-thread .agchip")||{}).textContent||"",fmt(TXT("ag_chip_open"),L(T("the gallery","a galeria")))]');
  ok('Suggest: Open a feature is a button, not done by itself', !s1[0] && s1[1] === s1[2], s1);
  await p.locator('#ph .gd-thread .agchip').click(); await W(300);
  ok('...a tap on it opens the gallery, and the button goes', await R('scr.querySelector(\'[data-ref="b:b_gal"]\').classList.contains("gfocus")&&!ph.querySelector(".gd-thread .agchip")'));
  // 5. a wrong id is refused, shown as invalid, and nothing moves
  await R('AG.mock={calls:[{name:"navigate_to",args:{target_id:"b:nope"}}],text:"Here."};ask("Somewhere?")'); await W(400);
  ok('A wrong id is refused (an error back to the model, invalid in the console)', await R('var k=AG.con.calls[0];!!k&&!k.ok&&/unknown id b:nope/.test(k.err)'));
  // 6. general knowledge: a badge when allowed; library only changes the rule in the prompt
  await R('AG.mock={text:"Lisbon was shaken in 1755."};ask("When?")'); await W(400);
  ok('No citation, general knowledge allowed: the General knowledge badge', await R('G.agTurn.source==="general"&&!!ph.querySelector(".gd-thread .agsrc.gen")'));
  await R('S.agentCfg.general=false;AG.mock={text:"I do not know."};ask("Who?")'); await W(400);
  ok('...Library only: no badge, and the prompt says to answer only from the library', await R('G.agTurn.source===""&&/Answer only from the LIBRARY/.test(AG.mockSeen.prompt)'));
  await R('S.agentCfg.general=true');
  // 7. the app's context and the library go up with the question
  ok('The prompt carries the app\'s context (screen, audience, language) and the library with ids', await R('var q=AG.mockSeen.prompt;/WHERE THE VISITOR IS/.test(q)&&/"audience"/.test(q)&&/<doc id="b:b_fun"/.test(q)'));
  // 8. failures fall back to the offline answer with a note; the session's cap too
  await R('AG.mock={fail:"rate_limited"};ask("Opera?")'); await W(400);
  ok('A failed call: the offline answer, with a note', await R('/^\\(Offline: rate_limited\\)/.test(G.answer)'), await R('G.answer'));
  await R('AG.mock={fail:"hang"};S.agentCfg.timeout=.3;ask("Opera?")'); await W(900);
  ok('...no answer in time: the offline answer (timeout)', await R('/^\\(Offline: timeout\\)/.test(G.answer)'), await R('G.answer'));
  await R('S.agentCfg.timeout=12;S.agentCfg.cap=AG.count;AG.mock={text:"x"};ask("Opera?")'); await W(400);
  ok('...the session\'s questions used up: the offline answer with the limit note', await R('G.answer.indexOf(TXT("ag_note_cap"))===0'));
  await R('S.agentCfg.cap=40;AG.count=0');
  // 9. Scripted + offline search never calls the model
  await R('S.guideCfg.ask="search";AG.mockSeen=null;ask("opera")'); await W(300);
  ok('Scripted + offline search: no model call', await R('AG.mockSeen===null'));
  await R('S.guideCfg.ask="ai";stopTour()');
  // 10. the wall: points lit, the camera turns to a point; Off leaves a tool out; Confirm asks first
  await R('S.camCfg.wall="belem";S.view="card";S.stop="closed";S.camX=.2;S.camY=.5;S.camZoom=1.4;S.agentCfg.pol.make_route="off";renderAll(false);G.cam.open=true;camLayer();' +
    'AG.mock={calls:[{name:"highlight_points",args:{poi_ids:["poi:belem","poi:jeronimos"]}},{name:"navigate_to",args:{target_id:"poi:jeronimos"}},{name:"start_tour",args:{}}],text:"The oldest is the Jerónimos Monastery; Belém Tower is in front of you."};camAsk("What is oldest?")');
  await W(500);
  const w1 = await R('[G.cam.lit.join(),camView().central&&camView().central.id,AG.mockSeen.tools.indexOf("make_route"),AG.mockSeen.tools.indexOf("highlight_points")>=0,(ph.querySelector(".camask .agchip.confirm")||{}).textContent||"",/"in_view"/.test(AG.mockSeen.prompt)]');
  ok('Wall: Light points lights the two it names (and Go to keeps them lit)', /belem/.test(w1[0]) && /jeronimos/.test(w1[0]), w1);
  ok('...Go to a point turns the camera to it (it is now the middle one)', w1[1] === 'jeronimos', w1);
  ok('...a tool set to Off is not offered to the model; the points in view are in the prompt', w1[2] === -1 && w1[3] && w1[5], w1);
  ok('...Start the tour (Confirm) is a button that asks first', /Start the tour\?|Começar a visita\?/.test(w1[4]) && /Yes|Sim/.test(w1[4]), w1);
  ok('...a point in view the answer names is its source (data from the app); one not in view is not', await R('G.cam.turn.source==="library"&&G.cam.turn.cites.indexOf("poi:belem")>=0&&G.cam.turn.cites.indexOf("poi:jeronimos")<0'), await R('[G.cam.turn.cites,G.cam.turn.ctx.in_view.map(function(v){return v.id})]'));
  await p.locator('#ph .camask .agchip.confirm').click(); await W(400);
  ok('...a tap on Yes starts the tour', await R('G.on&&S.view==="card"'));
  await R('stopTour();S.agentCfg.pol.make_route="suggest";S.camCfg.wall="ribeira";S.stop="full";renderAll(false)');
  // 11. Gemini, with fetch faked: the tool's result goes back with its id, the key only in the header
  const gem = await R(`window.LABKEYS={gemini:"TESTKEY-123"};S.agentCfg.provider="gemini";applyProvider();window.__calls=[];var of=window.fetch;window.__of=of;
    window.fetch=function(u,o){__calls.push({u:u,h:o.headers,b:JSON.parse(o.body)});var n=__calls.length;
      var sse=n===1?'data: {"candidates":[{"content":{"role":"model","parts":[{"functionCall":{"name":"navigate_to","args":{"target_id":"b:b_info"},"id":"call_1"},"thoughtSignature":"sig"}]}}],"usageMetadata":{"promptTokenCount":900,"candidatesTokenCount":20}}\\n\\n'
        :'data: {"candidates":[{"content":{"role":"model","parts":[{"text":"It is on Praça do "}]}}]}\\n\\ndata: {"candidates":[{"content":{"role":"model","parts":[{"text":"Comércio [src:b:b_info]."}]}}],"usageMetadata":{"promptTokenCount":950,"candidatesTokenCount":15}}\\n\\n';
      return Promise.resolve(new Response(sse,{status:200}))};
    startTour();pauseTour();G.askOpen=true;S.dockH="full";renderGuide();ask("Where is it?");1`);
  await W(600);
  const g1 = await R('var c=window.__calls,two=c[1]&&c[1].b.contents;[c.length,/streamGenerateContent\\?alt=sse/.test(c[0].u)&&/gemini-flash-lite-latest/.test(c[0].u),c[0].h["x-goog-api-key"],JSON.stringify(c[0].b).indexOf("TESTKEY")<0,two&&two[1].parts[0].functionCall.id,two&&JSON.stringify(two[2].parts[0].functionResponse),G.agTurn&&G.agTurn.text,AG.last&&AG.last.tin,AG.last&&AG.last.tout,scr.querySelector(\'[data-ref="b:b_info"]\').classList.contains("gfocus")]');
  ok('Gemini: two calls to the streamed endpoint of the chosen model; the key in the header only', g1[0] === 2 && g1[1] && g1[2] === 'TESTKEY-123' && g1[3], g1);
  ok('...the tool ran (Practical info in focus) and its result went back with the call\'s id', g1[9] && g1[4] === 'call_1' && /"id":"call_1"/.test(g1[5] || '') && /"name":"navigate_to"/.test(g1[5] || ''), g1);
  ok('...the streamed text is the answer (citation kept as a source); tokens counted from the provider', g1[6] === 'It is on Praça do Comércio.' && g1[7] === 1850 && g1[8] === 35, g1);
  // 12. the Claude API, with fetch faked
  const an = await R(`window.LABKEYS={gemini:"TESTKEY-123",anthropic:"TESTANT-9"};S.agentCfg.provider="anthropic";applyProvider();window.__calls=[];
    window.fetch=function(u,o){__calls.push({u:u,h:o.headers,b:JSON.parse(o.body)});var n=__calls.length;
      var j=n===1?{content:[{type:"tool_use",id:"tu_1",name:"navigate_to",input:{target_id:"b:b_fun"}}],stop_reason:"tool_use",usage:{input_tokens:800,output_tokens:30}}:{content:[{type:"text",text:"Yes, an opera [src:b:b_fun]."}],stop_reason:"end_turn",usage:{input_tokens:850,output_tokens:12}};
      return Promise.resolve(new Response(JSON.stringify(j),{status:200}))};ask("Opera?");1`);
  await W(600);
  const a1 = await R('var c=window.__calls;[c.length,c[0].u,c[0].h["x-api-key"],c[0].h["anthropic-dangerous-direct-browser-access"],c[0].b.model,c[0].b.tools.length,c[1]&&JSON.stringify(c[1].b.messages[2].content[0]),G.agTurn&&G.agTurn.text,AG.last&&AG.last.tin]');
  ok('Claude API: Messages with the tools, the key in the header, Haiku 5.5', a1[0] === 2 && a1[1] === 'https://api.anthropic.com/v1/messages' && a1[2] === 'TESTANT-9' && a1[3] === 'true' && a1[4] === 'claude-haiku-5-5' && a1[5] >= 5, a1);
  ok('...the tool\'s result goes back as a tool_result for its id; the answer and the tokens', /"tool_result"/.test(a1[6] || '') && /"tu_1"/.test(a1[6] || '') && a1[7] === 'Yes, an opera.' && a1[8] === 1650, a1);
  await R('window.fetch=window.__of;stopTour()');
  // 13. the Editor: the panel, its nine parts, the tools with their policies, the words; keys never shown
  await R('renderEditor()');
  const e1 = await R('var f=ed.querySelector(\'[data-fo="s7agent"]\');[!!f&&ed.querySelector(\'[data-fo="s7cam"]\').nextElementSibling===f,f&&["1 · Guide mode","2 · Model","3 · Inputs","4 · Tools","5 · Knowledge","6 · Outputs","7 · Services and keys","8 · Limits","9 · Test console"].filter(function(t){return f.textContent.indexOf(t)<0}),f&&f.querySelectorAll(".agtools tbody tr").length,(TXO.s7agent||[]).length,ed.innerHTML.indexOf("TESTKEY")<0&&ed.innerHTML.indexOf("TESTANT")<0,/from keys\\.local\\.js/.test(ed.textContent)]');
  ok('Editor: Ask Azu follows Ask about the view, with its nine parts', e1[0] && e1[1] && e1[1].length === 0, e1);
  ok('...the eight tools, each with its policy; its eleven words', e1[2] === 8 && e1[3] === 11, e1);
  ok('...the keys: where they came from, never the key itself', e1[4] && e1[5], e1);
  await R('var s=ed.querySelector(\'select[data-sel="@agentCfg.pol.navigate_to"]\');s.value="confirm";s.dispatchEvent(new Event("change",{bubbles:true}))');
  ok('...a policy set in the Editor is the agent\'s', await R('S.agentCfg.pol.navigate_to==="confirm"'));
  await R('S.agentCfg.pol.navigate_to="auto";delete window.LABKEYS;S.agentCfg.provider="mock";applyProvider();renderEditor();var f=ed.querySelector(\'[data-fo="s7agent"]\');f.open=true');
  // 14. the test console: runs on the phone as it is, shows the calls, the time, the tokens; saves an eval case
  await R('S.view="card";S.stop="full";renderPhone(false);AG.mock={calls:[{name:"navigate_to",args:{target_id:"b:b_facts"}},{name:"navigate_to",args:{target_id:"b:zzz"}}],text:"Quick facts [src:b:b_facts]."};ed.querySelector("#agq").value="Facts?"');
  await R('ed.querySelector("[data-agrun]").click()'); await W(500);
  const k1 = await R('var c=ed.querySelector(".agcon");[c.querySelectorAll(".agcall.ok").length,c.querySelectorAll(".agcall.bad").length,(c.querySelector(".agmet")||{}).textContent||"",/Quick facts/.test(c.querySelector(".agout").textContent)]');
  ok('Test console: the answer, each call valid or not, the model and its time and tokens', k1[0] === 1 && k1[1] === 1 && /Test double/.test(k1[2]) && / ms /.test(k1[2]) && / in \//.test(k1[2]) && k1[3], k1);
  await R('ed.querySelector("[data-agsave]").click()');
  ok('...Save as eval case keeps the question, the context, the answer and the calls', await R('AG.evals.length===1&&AG.evals[0].question==="Facts?"&&AG.evals[0].calls.length===2&&/1 saved/.test(ed.querySelector(".agcon").textContent)'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
