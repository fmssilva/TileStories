/* ===================== v8f: the card's close and settings stay put; the hero keeps one action; the grabber taps; the demo card closes; a change redraws in place ===================== */
/* ----- the hero keeps one action: the guide button (the Listen pill left; the Audio Guide block plays the audio) ----- */
KOPT.Header=KOPT.Header.filter(function(o){return o[0]!=="listen"});delete S.blockDefs.Header.o.listen;

/* ----- close and settings: pinned to the sheet's top (sticky in the sheet, v8f.css); the top band carries them once it shows ----- */
on("topBand.after",function(){var tb=ph.querySelector(".topband"),c=scr.querySelector(".chrome");if(c)c.style.visibility=tb&&tb.style.display!=="none"?"hidden":""});

/* ----- the sheet: a tap on the grabber moves it one stop (peek > half > full > half); a short drag goes one stop its way ----- */
function stepStop(){S.stop=S.stop==="full"?"half":S.stop==="half"?"full":"half";feedback("tap");renderPhone(true)}
var DOWN={full:"half",half:"peek",peek:"closed"},UP={peek:"half",half:"full",full:"full"};
function initGrab(gb){gb.setAttribute("role","button");gb.tabIndex=0;
  gb.addEventListener("pointerdown",function(e){gb.setPointerCapture(e.pointerId);var y0=e.clientY,t0=parseFloat(scr.style.top)||0,k=fit.getBoundingClientRect().height/SCREENS[S.screen][1],moved=false;
    function mv(ev){if(!moved&&Math.abs(ev.clientY-y0)<6)return;if(!moved){moved=true;scr.style.transition="none"}var t=Math.max(0,t0+(ev.clientY-y0)/k);scr.style.top=t+"px";gb.style.top=(t+2)+"px"}
    function up(ev){gb.removeEventListener("pointermove",mv);gb.removeEventListener("pointerup",up);gb.blur();if(!moved){stepStop();return}
      var dy=(ev.clientY-y0)/k;S.stop=dy<-40?UP[S.stop]:dy>40?DOWN[S.stop]:S.stop;renderPhone(true)}
    gb.addEventListener("pointermove",mv);gb.addEventListener("pointerup",up)});
  gb.addEventListener("keydown",function(e){if(e.key==="Enter"||e.key===" "){e.preventDefault();stepStop()}})};
/* a hint (Card Controls > Hints): the card opened at Half rises 8 px and settles, so the visitor sees it moves (never with Reduce Motion) */
function nudgeOnce(){var m=S.hints.on?S.hints.nudge:"off";if(m==="off"||S.reduce||(m==="visit"&&S.nudged))return;S.nudged=true;var gb=document.getElementById("grab"),k=[{transform:"none"},{transform:"translateY(-8px)",offset:.4},{transform:"none"}],o={duration:700,delay:450,easing:"ease-in-out"};
  scr.animate(k,o);if(gb)gb.animate(k,o)}
/* the half card's picture and the peek row open the card one stop too */
ph.addEventListener("click",function(e){if(S.view!=="card")return;var t=e.target;
  if(S.stop==="peek"&&t.closest(".peekrow")){e.stopPropagation();stepStop();return}
  if(S.stop==="half"&&t.closest(".hero")&&!t.closest("button,a,[data-go],[data-locate],[role=button]")){e.stopPropagation();stepStop()}},true);

/* ----- the demo card closes back to where it was opened from (a card, the wall, Demo Components); its settings open like the card's ----- */
ph.addEventListener("click",function(e){var t=e.target,ch=t.closest(".chrome i, .topband i");if(!ch)return;var x=ch.textContent.charCodeAt(0)===10005;
  if(S.view==="democard"){e.stopPropagation();if(x){var b=S.beforeDemo||{view:"card",stop:"closed"};S.view=b.view;S.stop=b.stop;S.settingsOpen=false;renderPhone(false)}else{S.settingsOpen=!S.settingsOpen;renderPhone(true)}return}
  if(S.view==="card"&&x&&ch.closest(".topband")){e.stopPropagation();S.stop="closed";renderPhone(false)}},true);

/* ----- a change redraws in place: the hero keeps its height, what was on screen stays shown, the scroll stays ----- */
function fitHero(){var hero=scr.querySelector(".hero");if(!hero||(S.view!=="card"&&S.view!=="democard"))return;
  if(S.heroH){hero.style.transition="none";hero.style.height=S.heroH+"px";void hero.offsetHeight;hero.style.transition=""}
  S.heroH=heroH();hero.style.height=S.heroH+"px"}
function shownInPlace(){var r=scr.getBoundingClientRect();scr.classList.add("still");
  scr.querySelectorAll(".mv:not(.in)").forEach(function(el){var b=el.getBoundingClientRect();if(b.bottom>r.top&&b.top<r.bottom)el.classList.add("in")});
  void scr.offsetHeight;scr.classList.remove("still")}
on("renderPhone.before",function(keep,cx){cx.top=scr.scrollTop;cx.view=S.view;cx.was=S.lastStop;if(!keep||cx.view!==S.lastView)S.heroH=0;S.lastView=cx.view});
on("renderPhone.after",function(keep,cx){var top=cx.top,view=cx.view,was=cx.was;if(keep&&S.view===view){scr.scrollTop=top;shownInPlace()}
  S.justOpened=S.view==="card"&&S.stop!=="closed"&&was==="closed";S.stopMoved=S.view==="card"&&was!==S.stop;if(S.stopMoved)S.movedAt=performance.now();if(S.justOpened&&S.stop==="half")nudgeOnce();S.lastStop=S.view==="card"?S.stop:"closed";
  if(S.view!=="democard")S.beforeDemo={view:S.view,stop:S.stop};
  if(S.view==="democard"&&S.settingsOpen&&!ph.querySelector(".vset"))ph.insertAdjacentHTML("beforeend",settingsPanel());
  var gb=document.getElementById("grab");if(gb)gb.setAttribute("aria-label",TXT(S.stop==="full"?"sheet_collapse":"sheet_expand"))});
/* light / dark only repaints: the editor's previews change colour in place, the editor is not rebuilt */
function darkInPlace(){ed.querySelectorAll(".cv").forEach(function(c){c.classList.toggle("dark",S.dark)});renderPhone(true)}
document.addEventListener("click",function(e){var t=e.target;if(!t.closest)return;
  if(t.closest('#ptool [data-pt="dark"]')){e.stopPropagation();S.dark=!S.dark;darkInPlace();return}
  var vo=t.closest('[data-vis^="mode|"]');if(vo){e.stopPropagation();var m=vo.dataset.vis.split("|")[1];S.vis.mode=m;S.dark=m==="dark"||(m==="phone"&&S.cardCfg.mode==="dark");darkInPlace()}},true);

/* ----- the Locate Button rests at the foot of the screen; the Navigation Bar (or the Sticky Action) lifts it when it comes ----- */
function pinLift(anim){var lp=ph.querySelector(".locpin");if(!lp)return;var k=fit.getBoundingClientRect().height/SCREENS[S.screen][1],pr=ph.getBoundingClientRect(),b=22;
  [document.getElementById("navbar"),ph.querySelector(".sticky")].forEach(function(el){if(!el||getComputedStyle(el).display==="none"||el.classList.contains("away"))return;
    var r=el.getBoundingClientRect();if(r.height&&r.top>pr.top+pr.height/2)b=Math.max(b,Math.round((pr.bottom-r.top)/k)+12)});
  if(anim&&!S.reduce){lp.style.bottom=b+"px";return}lp.style.transition="none";lp.style.bottom=b+"px";void lp.offsetHeight;lp.style.transition=""}
/* its label says what the pin does for 4 s of being SEEN (time counts only while it has room beside the pin), then folds into it:
   Once a visit = on the first card(s) until those 4 s are spent; Every card = 4 s on each card. A tap on the pin counts as read. */
S.ctl.locate.label="visit";S.locShown=0;
function locLabel(){return ph.querySelector(".locpin > span:first-child:not(.floatpin)")}
function foldLabel(){S.locFolded=true;if(S.ctl.locate.label==="visit")S.locSeen=true;var lb=locLabel();if(lb)lb.classList.add("fold")}
/* room = no overlap with the INK of the hero's words (their text and the guide button), not the box that holds them */
function locFit(){var lb=locLabel();if(!lb)return;var wait=650-(performance.now()-(S.movedAt||0));if(wait>0){lb.style.visibility="hidden";clearTimeout(S.fitT);S.fitT=setTimeout(locFit,wait+20);return}lb.style.visibility="";var a=lb.getBoundingClientRect();if(!a.width)return;
  var hit=Array.prototype.some.call(scr.querySelectorAll(".hero .w small,.hero .w h2,.hero .w p,.hero .w button"),function(el){var c;if(el.tagName==="BUTTON")c=el.getBoundingClientRect();else{var rg=document.createRange();rg.selectNodeContents(el);c=rg.getBoundingClientRect()}
    return c.width&&!(a.right<c.left||a.left>c.right||a.bottom<c.top||a.top>c.bottom)});
  if(hit)lb.style.visibility="hidden"}
setInterval(function(){var lb=locLabel();if(!lb||S.locFolded||S.view!=="card"||S.stop==="closed"||S.stop==="peek"||document.hidden)return;
  if(lb.style.visibility!=="hidden"&&!lb.classList.contains("fold")){S.locShown+=250;if(S.locShown>=4000)foldLabel()}},250);
on("renderPhone.after",function(keep){pinLift(false);
  if(S.justOpened&&S.ctl.locate.label==="open"){S.locFolded=false;S.locShown=0}
  var lb=locLabel();if(!lb)return;lb.classList.add("loclab");
  if(S.locFolded||(S.ctl.locate.label==="visit"&&S.locSeen))lb.classList.add("fold");
  locFit()});/* while the sheet and the hero settle (650 ms after a stop change) the words are not in place: it waits */
scr.addEventListener("scroll",function(){if(S.view!=="card")return;pinLift(true);locFit()},{passive:true});
ph.addEventListener("click",function(e){if(e.target.closest(".locpin"))foldLabel()},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("ctl\\.jump\\.","ctl\\.jump\\.|ctl\\.locate\\.label"));

/* probes (#probe): the page's state for the headless checks */
if(location.hash.indexOf("#probe")===0)window.LAB={S:S,run:function(src){return eval(src)}};
