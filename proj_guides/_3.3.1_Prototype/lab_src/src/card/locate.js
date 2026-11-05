/* The Locate Button: it rides with the bar, its label folds after it is seen (pinLift, locFit); the rule of a label seen 4 s (seenLabel). */
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
/* the rule, for every label that folds into its button once seen (the Locate Button's, the guide button's): seenLabel({label: its
   element, live: may its time count now, done: folded already, add(ms): adds seen time and gives the total, fold}). Unity: one
   scheduler (schedule.Execute) adds the visible time, then the USS class "fold" transitions the label's width and opacity */
var SEEN_MS=4000,SEEN=[];
function seenLabel(o){SEEN.push(o)}
setInterval(function(){if(document.hidden)return;SEEN.forEach(function(o){var lb=o.label();if(!lb||o.done()||!o.live())return;
  if(lb.style.visibility!=="hidden"&&!lb.classList.contains("fold")&&o.add(250)>=SEEN_MS)o.fold()})},250);
seenLabel({label:locLabel,live:function(){return S.view==="card"&&S.stop!=="closed"&&S.stop!=="peek"},done:function(){return S.locFolded},add:function(ms){return S.locShown+=ms},fold:foldLabel});
on("renderPhone.after",function(keep){pinLift(false);
  if(S.justOpened&&S.ctl.locate.label==="open"){S.locFolded=false;S.locShown=0}
  var lb=locLabel();if(!lb)return;lb.classList.add("loclab");
  if(S.locFolded||(S.ctl.locate.label==="visit"&&S.locSeen))lb.classList.add("fold");
  locFit()});/* while the sheet and the hero settle (650 ms after a stop change) the words are not in place: it waits */
scr.addEventListener("scroll",function(){if(S.view!=="card")return;pinLift(true);locFit()},{passive:true});
ph.addEventListener("click",function(e){if(e.target.closest(".locpin"))foldLabel()},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("ctl\\.jump\\.","ctl\\.jump\\.|ctl\\.locate\\.label"));
