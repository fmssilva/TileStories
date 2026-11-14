/* The Guide tab (edGuide): one place for everything the guide does, on the card, on the wall and at the app's start (_3.8, decision 1).
   Guidance round 14 (the developer's tab, 2026-11-11): six sections, each naming a part of the guide an author looks for:
   1 · Azu (the character), 2 · The tour (the story told), 3 · Games on the tour, 4 · Azu speaks up (the guidance, guidance/editor.js),
   5 · Ask Azu (what the visitor sees, what Azu knows, the engine), 6 · Preview & Test (Editor tools: last, the developer, round 14b). The guide's modules still make
   and fill their foldouts (s7voice, s7dock, s7mascot…) in the Detail Card's document; this tab asks for that document's guide part only
   (edCardDoc("guide"), so its draw is fast) and arranges it by GUIDE_PLAN: a foldout by its id (it keeps its id, only its parent
   changes), a row by its label, a node by its class, the words of an owner (its Aa bar). Two are taken apart, their rows and words
   placed where an author looks for them: Tour controls (s7tour, its
   words in Guide panel: it had no row) and the old 2 · Mascot section around its one foldout (gt2): Mascot itself (s7mascot) is
   1 · Azu now, and Fun › On the tour (gd:g:ontour) is 3 · Games on the tour, each with its id; a saved open state of a foldout taken
   apart opens its new place (GUIDE_WAS, once). A foldout or a row with no place is never lost: it goes to the end of 2 · The tour (or of
   its section) with a warning in the console. The Detail Card's tab leaves the Guide out (edCard.tab). Unity: the POI Editor's Guide
   tab (an IMGUI / UI Toolkit tab beside Detail Card), its foldouts Foldout elements with the same ids, a link a button that opens
   another tab at a foldout. */
var GUIDE_SECS=[
  {id:"s7mascot",n:"1",title:"The mascot",sub:"who it is, its look, voice, speech on screen, manner and moments: read by every part of the guide"},
  {id:"gt1",n:"2",title:"The tour",sub:"the story told: what it says, its music, captions, focus, the panel, the ways in and its end"},
  {id:"gd:g:ontour",n:"3",title:"Games on the tour",sub:"the quiz, Guess the year, Don't say it!, Find it on the wall, the breather, and asking during a game"},
  {id:"gt4",n:"4",title:"The mascot speaks up",sub:"tips, remarks, play, walkthroughs and the first pages: the mascot on its own, one at a time"},
  {id:"gt3",n:"5",title:"Ask the mascot",sub:"the visitor's questions: what the visitor sees, what the mascot knows, the engine"},
  {id:"gt0",n:"6",title:"Preview &amp; Test",sub:"Editor tools, never in the app: start the app, the visitor, a simulated visit, the Lab's wall, the voice test",badge:["test","TEST"]}];
/* each section's parts, in order. A part: "Label" (a row of the current source, by its label), {from:id} (the source: that foldout's
   rows and nodes), {fo:id} (a foldout, whole), {fo:id,t,s,parts,deep} (a foldout made here, or one re-titled, with these parts; deep: it
   opens one level later, an Editor or developer part), {add:parts} on {fo:id} (an existing foldout with more at its end), {words:o} (o's
   Aa bar and its panels), {cls:c} (a node by its class), {drop:c} (nodes not shown here), {h,s} (a heading), {rules:1} (the tour's fixed
   rules). 3 · Games on the tour and 4 · Azu speaks up are drawn by guidance/editor.js (edGuide.html); 6 · Preview & Test is filled there too */
var GUIDE_PLAN={
  /* round 20: the Lab's wall (Ask about the view's Wall (the Lab)) is a stand-in only the Lab has: an Editor tool, so it is here */
  gt0:[{h:"The Lab's wall",s:"its stand-in for the camera, never in the app"},{from:"s7cam",borrow:true},"Wall (the Lab)",{fo:"s7vtest"}],
  /* round 19 (the developer): the mascot's global settings, one foldout each, in this order; the rows of its voice and of speech on screen
     are borrowed from the foldouts that make them (Narration, Captions, Ask's engine), which keep a one-line link to here */
  s7mascot:[{from:"s7mascot"},{fo:"s7who"},
    {fo:"gt:look",t:"Look",s:"what the mascot is: its character, its source, its tile or its model",parts:[{from:"s7mascot"},"Character","Mascot source","Glaze","Pattern","Light","Corners","Preview","Model",
      {fo:"gt:tools",t:"Model tools",s:"the 3D model's clips, Import… and a model from a picture (Mascot source = 3D model)",deep:true,parts:["3D model","Play a clip","Import…","Generate from a picture"]}]},
    {fo:"gt:voice",t:"Voice",s:"its recorded voice, its speed, a line with no recording, the answers' voice, the coverage",parts:[{from:"s7voice",borrow:true},"The voice","Sources","Speed","A line with no recording","Voice in this browser",
      {from:"s7agent",borrow:true},"Voice of the answers","ElevenLabs model",{from:"s7voice",borrow:true},"Coverage","Speaking now"]},
    {fo:"gt:speech",t:"Speech on screen",s:"how its words show while it speaks: one highlighter, the teleprompter, the bubble, the panels' look",parts:[{from:"s7cap",borrow:true},{cls:"mpspeech"},"Highlight what is read","Highlight style","Trail","Glide",
      {h:"Where the words show",s:"during a tour and outside it"},"Words during a tour","Its height","A tip's bubble",{h:"The panels",s:"the Mini, the open panel and the teleprompter"},"The panels' look","Try"]},
    {fo:"gt:manner",t:"Manner",s:"how big, how lively, how it speaks and moves",parts:[{from:"s7mascot"},"Size","Liveliness","Mouth","How it moves","Moods","Tap on the mascot","Try"]},
    {fo:"gt:moments",t:"Moments",s:"when it introduces itself, and its walk to the wall",parts:[{from:"s7mascot"},"Introduction","Walks to the wall","Try"]},
    {words:"s7mascot"}],
  gt1:[{rules:1},{fo:"s7auto"},{fo:"s7more"},{fo:"s7voice"},{fo:"s7cap"},{fo:"s7focus"},{fo:"s7coach"},
    {fo:"s7dock",add:[{h:"Tour controls",s:"previous, pause, next, sound, captions, Ask and stop: their words"},{words:"s7tour"}]},
    {fo:"gt:ways",t:"Ways in",s:"how a visitor starts the tour, or picks it up again",parts:[{fo:"s7gbtn"},{fo:"s7hero"},{fo:"s7block"},{fo:"s7resume"}]},{fo:"s7end"}],
  gt3:[{fo:"s7ask",t:"What the visitor sees",s:"the question box, Ask about the view, the answers and how they sound",parts:[{from:"s7ask"},"Answers from",{fo:"s7cam"},{from:"s7agent"},
      "Spoken answer","The answers' voice","Link cards","Trace under each answer",{h:"The panel's words",s:"the question box, the mic, Ask's title"},{words:"s7ask"},{h:"The answers' words",s:"under an answer, its chips, its notes"},{words:"s7agent"}]},
    {fo:"gt:know",t:"Knowledge",s:"what the mascot answers from, and what it may do in the app",parts:[{from:"s7agent"},"App context","What is sent",{cls:"agpre"},"Photo of the view","Library","General knowledge",
      {h:"Its tools",s:"what it can do in the app"},{cls:"agtools"},"Targets","Actions in the app"]},
    {fo:"s7agent",t:"The engine",s:"the model, its key, its limits and the test console (for the developer)",deep:true,parts:[{from:"s7agent"},{drop:"agh"},"Guide mode",
      {h:"The model"},"Model","Gemini model","Claude API model","Claude tier (published)","Longest answer","Cost",{h:"Inputs"},"Text and voice",
      {h:"Services and keys"},{cls:"agstat"},"Paste a key","Test the key",{h:"Limits"},"Model steps","Timeout","Questions a session",{h:"Test console",s:"the scenes, by hand"},{cls:"agcon"}]}]};
/* the foldouts taken apart (Mascot's section around its one foldout; Tour controls, words only), and the place a saved open state of each
   opens now (once). Two foldouts became sections and keep their ids: Mascot (s7mascot, now 1 · Azu: its rows, no "Mascot › Mascot") and
   Fun › On the tour (gd:g:ontour, now 3 · Games on the tour), so a saved state that opened them opens them by id */
var GUIDE_WAS={gt2:"s7mascot",s7tour:"s7dock"};
/* the order the sections are drawn in, when it differs from the order shown: a section that takes rows from another's parts goes first */
var GUIDE_DRAW=["gt0"];
/* the tab is an accordion (round 20): its sections collapsed on one line each, sticky headers (editor/accordion.js) */
ACC_TABS.guide=true;
function guideOpMap(){Object.keys(GUIDE_WAS).forEach(function(o){if(!S.op.hasOwnProperty(o))return;var n=GUIDE_WAS[o];if(!S.op.hasOwnProperty(n))S.op[n]=S.op[o];delete S.op[o]})}
/* the Guide tab's button, after Detail Card */
(function(){var tabs=document.querySelector(".utabs"),c=tabs&&tabs.querySelector('[data-tab="card"]');if(!c||tabs.querySelector('[data-tab="guide"]'))return;
  var b=document.createElement("button");b.className="utab gd";b.setAttribute("role","tab");b.type="button";b.dataset.tab="guide";b.setAttribute("aria-selected","false");b.textContent="Guide";c.after(b);
  b.addEventListener("click",function(){S.tab="guide";renderEditor()})})();
/* a row of a source by its label (the first not yet taken) */
function gtRow(box,label){if(!box)return null;var rs=box.querySelectorAll(":scope > .row");for(var i=0;i<rs.length;i++){var lb=rs[i].querySelector("label");if(lb&&lb.firstChild?lb.firstChild.textContent===label:label===""&&lb&&!lb.textContent)return rs[i]}return null}
/* an owner's words: its Aa bar and the panels after it */
function gtWords(cx,o){var box=cx.box[o];if(!box)return"";var b=box.querySelector(":scope > .ownbar");if(!b)return"";var h=b.outerHTML,n=b.nextElementSibling;b.remove();
  while(n&&n.classList.contains("ownpanel")){var x=n.nextElementSibling;h+=n.outerHTML;n.remove();n=x}return h}
function gtBody(cx,parts){var h="";parts.forEach(function(p){
  if(typeof p==="string"){var r=gtRow(cx.from,p);if(r){r.remove();h+=r.outerHTML}return}
  if(p.from){cx.from=cx.box[p.from]||null;if(p.borrow)return;if(cx.el[p.from])cx.el[p.from].remove();if(cx.from&&cx.srcs.indexOf(p.from)<0)cx.srcs.push(p.from);return}
  if(p.h){h+='<div class="sub2 gth">'+p.h+(p.s?' <span class="sub">· '+p.s+'</span>':"")+'</div>';return}
  if(p.cls){var n=cx.from&&cx.from.querySelector(":scope > ."+p.cls);if(n){n.remove();h+=n.outerHTML}return}
  if(p.drop){if(cx.from)cx.from.querySelectorAll(":scope > ."+p.drop).forEach(function(x){x.remove()});return}
  if(p.words){h+=gtWords(cx,p.words);return}
  if(p.rules){h+=cx.rules;return}
  if(p.fo){var el=cx.el[p.fo];if(el)el.remove();
    if(!p.parts){if(!el)return;if(p.add){var a=gtBody(cx,p.add),inn=el.querySelector(":scope > .inner");if(inn&&a)inn.insertAdjacentHTML("beforeend",a)}h+=el.outerHTML;return}
    var keep=cx.from,inner=gtBody(cx,p.parts);cx.from=keep;if(!inner.replace(/<div class="sub2 gth">[\s\S]*?<\/div>/g,"").trim())return;
    h+=fo(p.fo,esc(p.t)+' <span class="sub">· '+esc(p.s)+'</span>',null,inner,p.deep?3:2)}});return h}
/* whatever a source still holds once its section is drawn: never lost, said in the console */
function gtLeft(cx,where){var h="";cx.srcs.forEach(function(s){var b=cx.box[s];if(!b)return;[].slice.call(b.children).forEach(function(x){if(!x.classList.contains("row")&&!x.matches("details,.note,table,div"))return;
  console.warn("Guide tab: a part of "+s+" ("+(x.dataset.fo||(x.querySelector("label")||{}).textContent||x.className)+") has no place in GUIDE_PLAN; shown at the end of "+where);h+=x.outerHTML;x.remove()})});cx.srcs=[];return h}
function edGuide(){guideOpMap();var d=document.createElement("div");d.innerHTML=edCardDoc("guide");var g=d.querySelector('[data-fo="s7"] > .inner'),h="";
  var rules=g&&g.querySelector(":scope > .row");rules=rules&&/Fixed rules/.test(rules.textContent)?rules:null;if(rules)rules.remove();
  var cx={from:null,srcs:[],rules:rules?rules.outerHTML:"",box:{},el:{}};
  if(g)g.querySelectorAll("details[data-fo]").forEach(function(x){cx.el[x.dataset.fo]=x;cx.box[x.dataset.fo]=x.querySelector(":scope > .inner")});
  /* drawn in GUIDE_DRAW's order (6 · Preview & Test first: it borrows the Editor tools from the parts that make them), shown in GUIDE_SECS' */
  var body={};GUIDE_SECS.slice().sort(function(a,b){return GUIDE_DRAW.indexOf(b.id)-GUIDE_DRAW.indexOf(a.id)}).forEach(function(s){cx.from=null;body[s.id]=gtBody(cx,GUIDE_PLAN[s.id]||[]);body[s.id]+=gtLeft(cx,s.n+" · "+s.title.replace(/&amp;/g,"&"))});
  /* a foldout a module adds later, with no place in GUIDE_PLAN: the end of 2 · The tour, said in the console */
  var rest=g?[].slice.call(g.children).filter(function(x){var i=x.matches("details")&&x.querySelector(":scope > .inner");if(i&&!i.children.length){x.remove();return false}return true}):[];if(rest.length){body.gt1+='<div class="sub2 gth">Not placed yet <span class="sub">· a part with no place in the Guide tab (GUIDE_PLAN)</span></div>';
    rest.forEach(function(x){console.warn("Guide tab: "+(x.dataset.fo||(x.querySelector("label")||{}).textContent||x.className)+" has no place in GUIDE_PLAN; shown at the end of 2 · The tour");body.gt1+=x.outerHTML})}
  /* round 20 (the developer): an accordion, its six sections collapsed on one line each (editor/accordion.js); the chip row is gone */
  h=accBar();GUIDE_SECS.forEach(function(s){h+=fo(s.id,'<span class="acct">'+s.n+' · '+s.title+'</span> <span class="sub">· '+s.sub+'</span>',s.badge,body[s.id]||" ",1)});
  h=pipe("edGuide.html",h);var t=document.createElement("div");t.innerHTML=h;guideDepths(t);accCount(t);
  return t.innerHTML+jsonBox("config.json · guide",guideJson())}
/* how deep each foldout opens (S.ui.level): its place in this tab, not where its module drew it. Level 1: the six sections; level 2 their
   foldouts (an Editor or developer part one level later: The visitor's Details, Model tools, The engine); an item (gd:i:) from level 3;
   an item's groups (gd:f:) never by level alone (only the self-test's walk, level 9, opens them) */
function guideDepths(t){t.querySelectorAll("details[data-fo]").forEach(function(x){var id=x.dataset.fo,n=1,p=x.parentElement&&x.parentElement.closest("details[data-fo]");
  while(p){n++;p=p.parentElement&&p.parentElement.closest("details[data-fo]")}
  var dp=/^gd:f:/.test(id)?9:/^gd:i:/.test(id)?Math.max(n,3):GUIDE_DEEP[id]?n+1:n;x.dataset.depth=dp;if(isOpen(id,dp))x.setAttribute("open","");else x.removeAttribute("open")})}
var GUIDE_DEEP={"gd:visitor":1,"gt:tools":1,s7agent:1};
function guideJson(){var a=JSON.parse(JSON.stringify(S.agentCfg||{}));Object.keys(a).forEach(function(k){if(/key/i.test(k))delete a[k]});return{guide:S.guideCfg,view:S.camCfg,agent:a,guidance:S.guidance}}
on("renderEditor.tab",function(){if(S.tab==="guide")return edGuide()});
on("refreshJsonV4.answer",function(){if(S.tab==="guide"){var pre=ed.querySelector(".json pre");if(pre)pre.textContent=JSON.stringify(guideJson(),null,2);return true}});
/* the Detail Card's tab: the Guide is in its own tab now */
on("edCard.tab",function(h){var d=document.createElement("div");d.innerHTML=h;var g=d.querySelector('[data-fo="s7"]');if(g)g.remove();return d.innerHTML});
/* the tab's sections, top level (the self-test's NO DEMO check walks them) */
function guideSecIds(){var d=document.createElement("div");d.innerHTML=edGuide();return[].map.call(d.querySelectorAll(":scope > details[data-fo]"),function(x){return x.dataset.fo})}
/* a word's owner in this tab: its path of foldouts (Languages & Terms' links, goOwner): where its words are drawn (GUIDE_PLAN), a
   foldout taken apart by its new place, and the guidance's own (guidePath.answer) */
function guideOwns(o){return /^s7/.test(o)||/^gd:/.test(o)||/^gt/.test(o)}
var GUIDE_PATHS=null;
function guidePaths(){if(GUIDE_PATHS)return GUIDE_PATHS;var P={},fos={},walk=function(parts,path){parts.forEach(function(p){if(!p||typeof p!=="object")return;
    if(p.fo){var q=path.concat([p.fo]);fos[p.fo]=q;if(p.parts)walk(p.parts,q);if(p.add)walk(p.add,q)}else if(p.words)P[p.words]=path.slice()})};
  GUIDE_SECS.forEach(function(s){fos[s.id]=[s.id];walk(GUIDE_PLAN[s.id]||[],[s.id])});
  Object.keys(fos).forEach(function(o){if(!P[o])P[o]=fos[o]});Object.keys(GUIDE_WAS).forEach(function(o){if(!P[o]&&P[GUIDE_WAS[o]])P[o]=P[GUIDE_WAS[o]]});return GUIDE_PATHS=P}
function guidePath(o){var P=guidePaths();if(P[o])return P[o].slice();var p=firstAnswer("guidePath.answer",o);return p||[o]}
/* the tab's sections have no FOCUS of their own (as the Detail Card's): they render open, and their toggle must never move the phone;
   each foldout inside keeps its own */
