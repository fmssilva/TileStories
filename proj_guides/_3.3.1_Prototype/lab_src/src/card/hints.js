/* The card's Hints as Guidance's cues (Guide › 4 · Guidance › App › The card (cues)): each cue's element on the card (cueTargets, what the
   scheduler reads), its look (cueStart: the grabber's nudge, the Locate label, the Navigation Bar's ring and tip, the Before / After sweep,
   the strip's slide, the underlined word's glow), where it shows (cuePlace: the phone brought to it); Card Controls > Hints as a view of the
   same data (each row reads Edited in Guidance and links there). When a cue may come, how often, Reduce Motion, one at a time and its
   memory are the scheduler's (guidance/): the old 300 ms timer, S.hintDone / S.hintCard / S.hintBusy and their moments are gone. */
/* the names kept for the code and the tests: S.hints.on is the group The card (cues)'s On, S.ctl.locate.label the Locate label's Shows
   (Once a visit / Every card / Off); each reads and writes Guidance's own value (no copy) */
S.hints={};
Object.defineProperty(S.hints,"on",{enumerable:true,get:function(){var g=S.guidance&&gdGroupById("card");return!!(g&&g.on)},set:function(v){var g=S.guidance&&gdGroupById("card");if(g)g.on=!!v}});
/* (S.guidance is made later in the load, by guidance/catalog.js: read before it, as at load, the label has no value yet) */
Object.defineProperty(S.ctl.locate,"label",{enumerable:true,get:function(){var it=S.guidance&&S.guidance.items.label;return!it?undefined:!it.on?"off":it.shows==="card"?"open":"visit"},
  set:function(v){var it=S.guidance&&S.guidance.items.label;if(!it)return;it.on=v!=="off";if(v!=="off")it.shows=v==="open"?"card":"visit"}});
/* ----- where each cue's element is: on the card, in view (clear of the foot), the strip with more to the side ----- */
function inView(el){var r=el.getBoundingClientRect(),s=scr.getBoundingClientRect();return r.height>0&&r.top>=s.top-1&&r.bottom<=s.bottom-80}
function firstIn(sel,f){return Array.prototype.filter.call(scr.querySelectorAll(sel),function(x){return(!f||f(x))&&inView(x)})[0]}
function cueEl(about){if(about==="grab")return document.getElementById("grab");
  if(about==="locate"){var lp=ph.querySelector(".locpin");return lp&&getComputedStyle(lp).display!=="none"&&!(S.view==="card"&&S.stop==="peek")?lp:null}
  if(about==="navbar"){var nb=document.getElementById("navbar");return S.view==="card"&&nb&&getComputedStyle(nb).display!=="none"?nb:null}
  if(about==="ba")return firstIn("[data-ba]");if(about==="strip")return firstIn(".el-car,.hstrip",function(x){return x.scrollWidth>x.clientWidth+20});if(about==="term")return firstIn(".gterm");return null}
var CUE_WHY={grab:"the card opened at Half",locate:"the Locate Button on the screen",navbar:"the Navigation Bar shows",ba:"a Before / After in view",strip:"a strip with more to the side in view",term:"an underlined word in view"};
/* the cues' elements on the screen now, in words (the scheduler's context: plain data); the grabber only as the card opens at Half */
function cueTargets(opening){var o={};Object.keys(CUE_WHY).forEach(function(k){if(k==="grab"&&!(opening&&S.view==="card"&&S.stop==="half"))return;if(cueEl(k))o[k]=CUE_WHY[k]});return o}
/* ----- the looks (unchanged): each gives how long it plays (ms); the Locate label stays until it has been seen 4 s ----- */
function hintNavbar(nb){var p=ph.getBoundingClientRect(),k=p.height/ph.offsetHeight,r=nb.getBoundingClientRect(),tip=document.createElement("div");
  tip.className="hinttip";tip.setAttribute("role","status");tip.textContent=TXT("hint_navbar");tip.style.bottom=Math.round((p.bottom-r.top)/k+10)+"px";ph.appendChild(tip);
  nb.classList.remove("hintring");void nb.offsetWidth;nb.classList.add("hintring");setTimeout(function(){tip.remove();nb.classList.remove("hintring")},3200);return 3400}
function hintSweep(ba){var x0=parseFloat(ba.style.getPropertyValue("--x"))||50,t0=performance.now(),stop=false;ba.addEventListener("pointerdown",function(){stop=true},{once:true});
  (function f(){var t=Math.min(1,(performance.now()-t0)/1400);if(stop)return;ba.style.setProperty("--x",(x0-12*Math.sin(2*Math.PI*t)).toFixed(1)+"%");if(t<1)requestAnimationFrame(f);else ba.style.setProperty("--x",x0+"%")})();return 1600}
function hintStrip(st){st.animate([{transform:"none"},{transform:"translateX(-40px)",offset:.45},{transform:"none"}],{duration:1100,easing:"ease-in-out"});return 1400}
function hintTerm(tm){tm.classList.remove("hintglow");void tm.offsetWidth;tm.classList.add("hintglow");setTimeout(function(){tm.classList.remove("hintglow")},2100);return 2200}
function cueLabel(){S.locFolded=false;S.locShown=0;var lb=locLabel();if(lb){lb.classList.remove("fold");locFit()}return Infinity}
/* a cue starts (guidance/service.js): its look on its element; null when the element is not there; a moving cue under Reduce Motion plays nothing */
function cueStart(c){var it=c.item,el=cueEl(it.about);if(!el)return null;if(it.reduce==="skip"&&S.reduce)return 0;
  return it.about==="grab"?cueNudge():it.about==="locate"?cueLabel():it.about==="navbar"?hintNavbar(el):it.about==="ba"?hintSweep(el):it.about==="strip"?hintStrip(el):it.about==="term"?hintTerm(el):null}
/* a cue ends: the Locate label folds into its pin (seen, or its card gone); the other looks end by themselves */
function cueStop(c){if(c.item.about==="locate"){var lb=locLabel();if(lb)lb.classList.add("fold")}}
/* the phone brought to where a cue shows (the Editor's demos, the scenes, the tests): the card opened at Half for the nudge and the label,
   at Full scrolled to its element for the others (the story page when the card has none) */
function cuePlace(id){S.settingsOpen=false;if(G.on)stopTour();
  if(id==="nudge"||id==="label"){S.view="card";S.stop="closed";renderPhone(false);S.stop="half";renderPhone(false);if(id==="label")scr.scrollTop=260;return}
  S.view="card";S.stop="full";renderPhone(false);var sel={navbar:null,ba:"[data-ba]",strip:".el-car,.hstrip",term:".gterm"}[id],el=sel&&scr.querySelector(sel);
  if(sel&&!el){S.view="story";renderPhone(false);el=scr.querySelector(sel)}
  var go=function(){var e=sel&&scr.querySelector(sel);scr.scrollTop=e?e.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-140:scr.querySelector(".hero").offsetHeight+60};
  /* and once the sheet and the hero have settled at their stop (650 ms), again if it moved out of view */
  go();clearTimeout(G.cuePlaceT);G.cuePlaceT=setTimeout(function(){var e=sel&&scr.querySelector(sel);if(e&&!inView(e))go()},700)}

/* ----- the Editor: Demo card as one component; Card Controls > Hints after the Locate Button, a view of Guidance's data ----- */
function cueShowsWords(it){var g=gdGroupById("card");return g&&!g.on?"Off (all the hints)":!it.on?"Off":it.shows==="card"?"Every card":"Once a visit"}
on("edCard.html",function(html){var d=document.createElement("div");d.innerHTML=html;
  var t=d.querySelector('[data-fo="s0tile"]');if(t)t.remove();
  var r1=rowEl(d,"s0card","Demo card"),r2=rowEl(d,"s0card","Components page");
  if(r1)r1.outerHTML=row("Tile on every card",pseg("test.on",["true","false"],["On","Off"]),"First on every marker's card, in Scene and Play only (never in a build). It opens Demo Components.")+
    row("Demo Components",'<button class="ubtn sm" type="button" data-democard="" data-then="demo">Open it &#8250;</button>',"Every motion, interaction, effect, element and pattern, on real parts. Its first button opens the full demo card.")+
    row("Demo card",'<button class="ubtn sm" type="button" data-democard="">Open it &#8250;</button>',"Every block once, with its demo content (filled once from the Ribeira Palace, independent since). The tests read it, never a real point. Its X returns to where it was opened.");
  if(r2)r2.remove();
  var sb=d.querySelector('[data-fo="s0card"] .sub');if(sb)sb.textContent="· the tile on every card, Demo Components, and the full demo card";
  var lc=d.querySelector('[data-fo="s5loc"]');if(lc){var g=gdGroupById("card"),hw=gdGroupById("howto"),go=function(o,t){return'<button class="ubtn sm gdedited" type="button" data-goto="'+o+'">'+esc(t||"Edited in Guidance")+' &#8250;</button>'};
    var tb='<table class="tbl cueview"><thead><tr><th style="width:30%">Hint</th><th>Shows</th><th style="width:44%"></th></tr></thead><tbody>'+
      g.items.map(function(id){var it=S.guidance.items[id];return'<tr><td>'+esc(it.label)+'</td><td>'+esc(cueShowsWords(it))+'</td><td>'+go("gd:i:"+id)+'</td></tr>'}).join("")+'</tbody></table>';
    var body='<div class="note">A view: the hints are Guide › 4 · Guidance › App › The card (cues), one cue each, taking turns with the guide\'s tips (one at a time, the cue room between a tip and a cue). Their words, how often, Done when and each one\'s demo are edited there.</div>'+
      row("Hints",'<div class="btnrow" style="margin:0"><span class="uses">'+(g.on?"On":"Off")+'</span>'+go("gd:g:card","Edited in Guidance › The card")+'</div>',"Small first-time cues, each at the moment its feature is in reach, through the guide's scheduler: never two at once and never with a tip, never while the visitor scrolls or touches, never in the card's first 1.5 s (the nudge and the label answer the opening itself). Off: none, the Locate label too.")+tb+
      row("How to use the card",'<div class="btnrow" style="margin:0"><span class="uses">'+(hw.on===false?"Off":[hw.inSettings?"In Settings":"",hw.inMenu?"in the ✦ menu":""].filter(Boolean).join(" and ")||"Hidden")+'</span>'+go("gd:g:howto","Edited in Guidance › How to use the card")+'</div>',"The walkthrough of the card's controls, five steps through the guide's tour; only when asked (Settings, the ✦ menu's How does this work?), never on its own.");
    lc.insertAdjacentHTML("afterend",comp("s5hints","a view of Guide › Guidance › The card (cues) and How to use the card",body));var f=innerOf(d,"s5hints");if(f)f.insertAdjacentHTML("beforeend",ownbar("s5hints")+ownPanels("s5hints"))}
  return d.innerHTML});
FOCUS.s5hints=["card","#grab","half"];
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("ctl\\.jump\\.","ctl\\.jump\\.|test\\.on"));
/* Tile on every card's demo: the card, its first section, the tile ringed (Off: the section without it) */
rowDemos({"test.on":{stop:"full",wait:0,scroll:".testtile|.gsec",ring:".testtile|.gsec"}});
