/* ===================== v8g: the demo in one component (the tile opens Demo Components, whose top button opens the full demo card); first-time hints; How to use the card ===================== */
/* ----- the demo: a tile first on every marker's card opens Demo Components; its top button opens the demo card ----- */
delete FOCUS.s0tile;FOCUS.s0card=["card",".testtile","full"];S.test.poi="painting";
testTile=function(){return inDemoCard?"":'<button class="testtile" type="button" data-go="demo"><div><b>'+TXH("test_tile_tag")+'</b><strong>'+TXH("test_tile_title")+'</strong><span>'+TXH("test_tile_line")+'</span></div><em>&#8250;</em></button>'};
demoScreen=function(){return demoScreenV6().replace('<b>'+esc(L(UI.demo))+'</b>','<b>'+TXH("test_tile_title")+'</b>').replace('<div class="pg">','<div class="pg"><button class="testtile demoopen" type="button" data-go="democard"><div><b>'+TXH("test_tile_tag")+'</b><strong>'+TXH("test_demo_title")+'</strong><span>'+TXH("test_demo_line")+'</span></div><em>&#8250;</em></button>')};

/* ----- hints: small first-time cues, each at the moment its feature is in reach, one at a time, each until seen ----- */
S.hints={on:true,nudge:"visit",navbar:"visit",ba:"visit",strip:"visit",term:"visit",howto:true};S.hintDone={};S.hintCard={};S.hintBusy=0;S.lastMove=0;S.openedAt=0;
var HINTS=[["nudge","Grabber nudge","The card opens at Half: it rises 8 px and settles, so the visitor sees it moves. Skipped with Reduce Motion."],
 ["label","Locate label","“Find it on the wall” beside the pin until seen for 4 s (only while it has room), then it folds into the pin."],
 ["navbar","Navigation Bar","The bar shows for the first time: it rings once and “Jump to any part” sits over it for 3 s."],
 ["ba","Before / After","The first before / after in view sweeps a little each way. Skipped with Reduce Motion."],
 ["strip","Swipe strips","The first strip in view slides a little and back: there is more to the side. Skipped with Reduce Motion."],
 ["term","Underlined words","The first underlined word in view glows once: a tap explains it."]];
function hintPath(id){return id==="label"?"ctl.locate.label":"hints."+id}
function hintMode(id){if(!S.hints.on)return"off";var m=getPath(hintPath(id));return m==="open"?"card":m}
function hintDue(id){var m=hintMode(id);return m!=="off"&&!(m==="visit"&&S.hintDone[id])&&!S.hintCard[id]}
function hintMark(id,ms){S.hintDone[id]=true;S.hintCard[id]=true;S.hintBusy=performance.now()+ms}
function labelShowing(){var lb=locLabel();return !!lb&&!lb.classList.contains("fold")&&lb.style.visibility!=="hidden"}
function hintFree(){var now=performance.now();return now>S.hintBusy&&now-S.lastMove>800&&now-S.openedAt>1500&&!labelShowing()&&!G.on&&!S.settingsOpen}
function inView(el){var r=el.getBoundingClientRect(),s=scr.getBoundingClientRect();return r.height>0&&r.top>=s.top-1&&r.bottom<=s.bottom-80}
function firstIn(sel,f){return Array.prototype.filter.call(scr.querySelectorAll(sel),function(x){return(!f||f(x))&&inView(x)})[0]}
function hintNavbar(nb){hintMark("navbar",3400);var p=ph.getBoundingClientRect(),k=p.height/ph.offsetHeight,r=nb.getBoundingClientRect(),tip=document.createElement("div");
  tip.className="hinttip";tip.setAttribute("role","status");tip.textContent=TXT("hint_navbar");tip.style.bottom=Math.round((p.bottom-r.top)/k+10)+"px";ph.appendChild(tip);
  nb.classList.remove("hintring");void nb.offsetWidth;nb.classList.add("hintring");setTimeout(function(){tip.remove();nb.classList.remove("hintring")},3200)}
function hintSweep(ba){hintMark("ba",1600);var x0=parseFloat(ba.style.getPropertyValue("--x"))||50,t0=performance.now(),stop=false;ba.addEventListener("pointerdown",function(){stop=true},{once:true});
  (function f(){var t=Math.min(1,(performance.now()-t0)/1400);if(stop)return;ba.style.setProperty("--x",(x0-12*Math.sin(2*Math.PI*t)).toFixed(1)+"%");if(t<1)requestAnimationFrame(f);else ba.style.setProperty("--x",x0+"%")})()}
function hintStrip(st){hintMark("strip",1400);st.animate([{transform:"none"},{transform:"translateX(-40px)",offset:.45},{transform:"none"}],{duration:1100,easing:"ease-in-out"})}
function hintTerm(tm){hintMark("term",2200);tm.classList.remove("hintglow");void tm.offsetWidth;tm.classList.add("hintglow");setTimeout(function(){tm.classList.remove("hintglow")},2100)}
setInterval(function(){if(S.view==="demo"||(S.view==="card"&&(S.stop==="closed"||S.stop==="peek"))||!hintFree())return;var nb=document.getElementById("navbar"),el;
  if(S.view==="card"&&hintDue("navbar")&&nb&&getComputedStyle(nb).display!=="none"){hintNavbar(nb);return}
  if(hintDue("ba")&&!S.reduce&&(el=firstIn("[data-ba]"))){hintSweep(el);return}
  if(hintDue("strip")&&!S.reduce&&(el=firstIn(".el-car,.hstrip",function(x){return x.scrollWidth>x.clientWidth+20}))){hintStrip(el);return}
  if(hintDue("term")&&(el=firstIn(".gterm"))){hintTerm(el)}},300);
scr.addEventListener("scroll",function(){S.lastMove=performance.now()},{passive:true});
var renderPhoneV8g=renderPhone;
renderPhone=function(keep){renderPhoneV8g(keep);if(S.justOpened){S.hintCard={};S.openedAt=performance.now()}
  if(hintMode("label")==="off"){var lb=locLabel();if(lb)lb.classList.add("fold")}};
/* Try: forget that hint, fold the Locate label (one at a time), and bring the phone where the hint shows */
function hintTry(id){S.hintDone[id]=false;S.hintCard[id]=false;S.hintBusy=0;S.settingsOpen=false;
  if(id==="nudge"||id==="label"){S.nudged=false;S.locSeen=false;S.locFolded=false;S.locShown=0;S.view="card";S.stop="closed";renderPhone(false);S.stop="half";renderPhone(false);if(id==="label")scr.scrollTop=260;return}
  S.locFolded=true;S.view="card";S.stop="full";renderPhone(false);var sel={navbar:null,ba:"[data-ba]",strip:".el-car,.hstrip",term:".gterm"}[id],el=sel&&scr.querySelector(sel);
  if(sel&&!el){S.view="story";renderPhone(false);el=scr.querySelector(sel)}
  scr.scrollTop=el?el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-140:scr.querySelector(".hero").offsetHeight+60;S.lastMove=performance.now()}

/* ----- How to use the card: the Guide's tour with five steps on the card's own controls, from the settings panel only ----- */
var HOW=[["hero","#grab","how_grab"],["hero",".chrome i:last-child","how_settings"],["sec:stories","#navbar","how_bar"],["sec:stories",".locpin","how_locate"],["b:b_story",null,"how_block"]];
function howTour(){if(G.on)stopTour();S.settingsOpen=false;S.howSaved=S.guideSteps;
  S.guideSteps=HOW.map(function(h,i){return{id:"how"+i,view:"card",ref:h[0],sel:h[1],act:"show",text:S.tx[h[2]]}});S.view="card";S.stop="full";renderPhone(false);startTour()}
var stopTourV8g=stopTour;stopTour=function(){stopTourV8g();if(S.howSaved){S.guideSteps=S.howSaved;S.howSaved=null}};
var runStepV8g=runStep;runStep=function(){runStepV8g();var st=S.guideSteps[G.i];if(G.on&&st&&st.sel){var el=ph.querySelector(st.sel);if(el){el.classList.add("gfocus");moveMascot(el)}}};
var clearGuideMarksV8g=clearGuideMarks;clearGuideMarks=function(){clearGuideMarksV8g();ph.querySelectorAll(".gfocus").forEach(function(x){x.classList.remove("gfocus")})};
var settingsPanelV8g=settingsPanel;settingsPanel=function(){var h=settingsPanelV8g();return S.hints.howto?h.replace(/<\/div>$/,'<button type="button" class="vhow" data-howto="1">'+TXH("settings_how")+' &#8250;</button></div>'):h};
ph.addEventListener("click",function(e){if(e.target.closest("[data-howto]")){e.stopPropagation();howTour()}},true);

/* ----- the Editor: Demo card as one component; Hints after the Locate Button ----- */
var edCardV8g=edCard;
edCard=function(){var d=document.createElement("div");d.innerHTML=edCardV8g();
  var t=d.querySelector('[data-fo="s0tile"]');if(t)t.remove();
  var r1=rowEl(d,"s0card","Demo card"),r2=rowEl(d,"s0card","Components page");
  if(r1)r1.outerHTML=row("Tile on every card",pseg("test.on",["true","false"],["On","Off"]),"First on every marker's card, in Scene and Play only (never in a build). It opens Demo Components.")+
    row("Demo Components",'<button class="ubtn sm" type="button" data-democard="" data-then="demo">Open it &#8250;</button>',"Every motion, interaction, effect, element and pattern, on real parts. Its first button opens the full demo card.")+
    row("Demo card",'<button class="ubtn sm" type="button" data-democard="">Open it &#8250;</button>',"Every block once, with its demo content (filled once from the Ribeira Palace, independent since). The tests read it, never a real point. Its X returns to where it was opened.");
  if(r2)r2.remove();
  var sb=d.querySelector('[data-fo="s0card"] .sub');if(sb)sb.textContent="· the tile on every card, Demo Components, and the full demo card";
  var lc=d.querySelector('[data-fo="s5loc"]');if(lc){var tb='<table class="tbl"><thead><tr><th style="width:24%">Hint</th><th>Shows</th><th style="width:44px"></th><th style="width:30px"></th></tr></thead><tbody>'+
      HINTS.map(function(h){var p=hintPath(h[0]),v=h[0]==="label"?["visit","open","off"]:["visit","card","off"];return'<tr><td>'+esc(h[1])+'</td><td>'+pseg(p,v,["Once a visit","Every card","Off"])+'</td><td><button class="del" type="button" data-hinttry="'+h[0]+'">Try</button></td><td>'+help(h[2])+'</td></tr>'}).join("")+'</tbody></table>';
    var body=row("Hints",pseg("hints.on",["true","false"],["On","Off"]),"Small first-time cues, each at the moment its feature is in reach: never two at once, never while the visitor scrolls, never in the card's first 1.5 s, never over what they touch. Each stays until seen; then Once a visit = not again this visit, Every card = again on each card. Off: none, the Locate label too.")+tb+
      row("How to use the card",pseg("hints.howto",["true","false"],["Shown","Hidden"]),"A button at the foot of the visitor's settings: the Guide shows the grabber, the settings, the bar, the pin and a block's page in five steps. Only when asked, never on its own.")+
      row("Its tour",'<button class="ubtn sm" type="button" data-howtry="1">Try it &#8250;</button>',"",true);
    lc.insertAdjacentHTML("afterend",comp("s5hints","first-time cues, one at a time, and How to use the card",body));var f=innerOf(d,"s5hints");if(f)f.insertAdjacentHTML("beforeend",ownbar("s5hints")+ownPanels("s5hints"))}
  return d.innerHTML};
ed.addEventListener("click",function(e){var h=e.target.closest("[data-hinttry]");if(h){e.stopPropagation();hintTry(h.dataset.hinttry);return}if(e.target.closest("[data-howtry]")){e.stopPropagation();howTour()}},true);
FOCUS.s5hints=["card","#grab","half"];
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("ctl\\.jump\\.","ctl\\.jump\\.|hints\\.|test\\.on"));
