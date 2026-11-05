/* The grabber: a tap or a drag moves the sheet between its stops (initGrab, stepStop); the peek row and the hero's tap. */
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
