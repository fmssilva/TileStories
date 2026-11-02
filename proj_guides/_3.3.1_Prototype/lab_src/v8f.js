/* ===================== v8f: the card's close and settings stay put; the hero keeps one action; the grabber taps; the demo card closes; a change redraws in place ===================== */
/* ----- the hero keeps one action: the guide button (the Listen pill left; the Audio Guide block plays the audio) ----- */
KOPT.Header=KOPT.Header.filter(function(o){return o[0]!=="listen"});delete S.blockDefs.Header.o.listen;

/* ----- close and settings: pinned to the sheet's top (sticky in the sheet, v8f.css); the top band carries them once it shows ----- */
var topBandV8f=topBand;
topBand=function(){topBandV8f();var tb=ph.querySelector(".topband"),c=scr.querySelector(".chrome");if(c)c.style.visibility=tb&&tb.style.display!=="none"?"hidden":""};

/* ----- the sheet: a tap on the grabber moves it one stop (peek > half > full > half); a short drag goes one stop its way ----- */
function stepStop(){S.stop=S.stop==="full"?"half":S.stop==="half"?"full":"half";feedback("tap");renderPhone(true)}
var DOWN={full:"half",half:"peek",peek:"closed"},UP={peek:"half",half:"full",full:"full"};
initGrab=function(gb){gb.setAttribute("role","button");gb.tabIndex=0;
  gb.addEventListener("pointerdown",function(e){gb.setPointerCapture(e.pointerId);var y0=e.clientY,t0=parseFloat(scr.style.top)||0,k=fit.getBoundingClientRect().height/SCREENS[S.screen][1],moved=false;
    function mv(ev){if(!moved&&Math.abs(ev.clientY-y0)<6)return;if(!moved){moved=true;scr.style.transition="none"}var t=Math.max(0,t0+(ev.clientY-y0)/k);scr.style.top=t+"px";gb.style.top=(t+2)+"px"}
    function up(ev){gb.removeEventListener("pointermove",mv);gb.removeEventListener("pointerup",up);if(!moved){stepStop();return}
      var dy=(ev.clientY-y0)/k;S.stop=dy<-40?UP[S.stop]:dy>40?DOWN[S.stop]:S.stop;renderPhone(true)}
    gb.addEventListener("pointermove",mv);gb.addEventListener("pointerup",up)});
  gb.addEventListener("keydown",function(e){if(e.key==="Enter"||e.key===" "){e.preventDefault();stepStop()}})};
/* the first card a visit opens at Half rises 8 px and settles, once, so the visitor sees it moves (never with Reduce Motion) */
function nudgeOnce(){if(S.nudged||S.reduce)return;S.nudged=true;var gb=document.getElementById("grab"),k=[{transform:"none"},{transform:"translateY(-8px)",offset:.4},{transform:"none"}],o={duration:700,delay:450,easing:"ease-in-out"};
  scr.animate(k,o);if(gb)gb.animate(k,o)}
/* the half card's picture and the peek row open the card one stop too */
ph.addEventListener("click",function(e){if(S.view!=="card")return;var t=e.target;
  if(S.stop==="peek"&&t.closest(".peekrow")){e.stopPropagation();stepStop();return}
  if(S.stop==="half"&&t.closest(".hero")&&!t.closest("button,a,[data-go],[data-locate],[role=button]")){e.stopPropagation();stepStop()}},true);

/* ----- the demo card closes back to where it was opened from; its settings open like the card's ----- */
ph.addEventListener("click",function(e){var t=e.target,ch=t.closest(".chrome i, .topband i");if(!ch)return;var x=ch.textContent.charCodeAt(0)===10005;
  if(S.view==="democard"){e.stopPropagation();if(x){var b=S.lastCard||{view:"card",stop:"closed"};S.view=b.view;S.stop=b.stop;S.settingsOpen=false;renderPhone(false)}else{S.settingsOpen=!S.settingsOpen;renderPhone(true)}return}
  if(S.view==="card"&&x&&ch.closest(".topband")){e.stopPropagation();S.stop="closed";renderPhone(false)}},true);

/* ----- a change redraws in place: the hero keeps its height, what was on screen stays shown, the scroll stays ----- */
fitHero=function(){var hero=scr.querySelector(".hero");if(!hero||(S.view!=="card"&&S.view!=="democard"))return;
  if(S.heroH){hero.style.transition="none";hero.style.height=S.heroH+"px";void hero.offsetHeight;hero.style.transition=""}
  S.heroH=heroH();hero.style.height=S.heroH+"px"};
function shownInPlace(){var r=scr.getBoundingClientRect();scr.classList.add("still");
  scr.querySelectorAll(".mv:not(.in)").forEach(function(el){var b=el.getBoundingClientRect();if(b.bottom>r.top&&b.top<r.bottom)el.classList.add("in")});
  void scr.offsetHeight;scr.classList.remove("still")}
var renderPhoneV8f=renderPhone;
renderPhone=function(keep){var top=scr.scrollTop,view=S.view,was=S.lastStop;if(!keep||view!==S.lastView)S.heroH=0;S.lastView=view;
  renderPhoneV8f(keep);if(keep&&S.view===view){scr.scrollTop=top;shownInPlace()}
  if(S.view==="card"&&S.stop==="half"&&was==="closed")nudgeOnce();S.lastStop=S.view==="card"?S.stop:"closed";
  if(S.view==="card"||S.view==="story"||S.view==="gallery")S.lastCard={view:"card",stop:S.view==="card"?S.stop:"full"};
  if(S.view==="democard"&&S.settingsOpen&&!ph.querySelector(".vset"))ph.insertAdjacentHTML("beforeend",settingsPanel());
  var gb=document.getElementById("grab");if(gb)gb.setAttribute("aria-label",TXT(S.stop==="full"?"sheet_collapse":"sheet_expand"))};
/* light / dark only repaints: the editor's previews change colour in place, the editor is not rebuilt */
function darkInPlace(){ed.querySelectorAll(".cv").forEach(function(c){c.classList.toggle("dark",S.dark)});renderPhone(true)}
document.addEventListener("click",function(e){var t=e.target;if(!t.closest)return;
  if(t.closest('#ptool [data-pt="dark"]')){e.stopPropagation();S.dark=!S.dark;darkInPlace();return}
  var vo=t.closest('[data-vis^="mode|"]');if(vo){e.stopPropagation();var m=vo.dataset.vis.split("|")[1];S.vis.mode=m;S.dark=m==="dark"||(m==="phone"&&S.cardCfg.mode==="dark");darkInPlace()}},true);

/* probes (#probe): the page's state for the headless checks */
if(location.hash.indexOf("#probe")===0)window.LAB={S:S,run:function(src){return eval(src)}};
