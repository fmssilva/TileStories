/* Guidance, the look (_3.8 section 4): a tip is one bubble (the dock's paper, 7:1 words, at most 2 lines, a tail to the mascot). Its
   target: the nearest marker (Tap a marker), the point in the middle (A tour of {point}): the mascot goes beside it (a 0.45 s arc hop)
   and points, Tap a marker's ring circles the marker (one soft pulse, then still); the ✦ (Ask about the view): Azu stays beside it and
   points at it, a ring round the ✦. Placed by a small solver: the side with room, never over its target, the words on the wall, the ✦,
   the bar or the block being told; the service asks it first (gdCovers) so a tip that would cover its target never starts. A spoken tip
   (Content, once the guide has been heard this visit) lights the word being said. Reduce Motion: no hop (Azu is there), a still ring.
   Mascot Off (Guide button = ✦ only, or Mascot > Character = Off): a plain bubble at the target, no character. The bubble's controls
   (_3.8 section 3.4): a tap does its action (Ask about the view opens, the tour starts, or it closes: acted); a swipe away dismisses
   it; a long press asks Fewer like this? (Fewer: its group rests for the visit; Keep: the tip again). The ×10 chip of the Editor's
   demos lives here too (gdChip). Unity: the bubble a 9-slice VisualElement (USS transitions on scale and opacity, its tail a rotated
   child), placed by the same solver on worldBound; the mascot's hop a translate tween (DOTween) on its element, the point the 3D
   model's Animator point clip (the Lab's still picture leans and shows a small pointer, or lifts its arm to the ✦); the rings
   VisualElements with a border radius and two borders (USS), the pulse a USS transition on scale; the marker's screen point from
   RuntimePanelUtils.CameraTransformWorldToPanel; the word lit by TextElement.PostProcessTextVertices (the read-along's tint); the
   swipe and the long press a PointerManipulator (PointerDown / Move / Up, a 0.6 s schedule.Execute for the hold), the swipe's exit a
   translate tween. */
var GD_RING=36,GD_GUT=12,GD_SWIPE=40,GD_HOLD=600;
G.gd.az=null;
function gdPR(el){var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,r=el.getBoundingClientRect();return{l:(r.left-pr.left)/q,t:(r.top-pr.top)/q,r:(r.right-pr.left)/q,b:(r.bottom-pr.top)/q}}
function gdBox(l,t,w,h){return{l:l,t:t,r:l+w,b:t+h}}
function gdOver(a,b){var x=Math.min(a.r,b.r)-Math.max(a.l,b.l),y=Math.min(a.b,b.b)-Math.max(a.t,b.t);return x>0&&y>0?x*y:0}
function gdSeen(e){return e&&e.getClientRects().length>0&&getComputedStyle(e).visibility!=="hidden"}
/* the mascot beside the ✦, when there is one to show (Mascot Off: none) */
function gdMascot(){return S.guideCfg.mascot!==false&&gbAzu()?ph.querySelector(".azfab"):null}
/* what a tip must keep clear of: the words on the wall (marker and point names, Azu's remark), the markers, the ✦, the bar, the preview
   bar, and during a tour the block being told */
function gdObstacles(){var out=[];
  ph.querySelectorAll("#wall .mk b,.campins .cpin b,.azfabbub,.azbub").forEach(function(e){if(gdSeen(e))out.push({b:gdPR(e),w:6,text:1})});
  ph.querySelectorAll("#wall .mk span,.campins .cpin i").forEach(function(e){if(gdSeen(e))out.push({b:gdPR(e),w:4})});
  ["#navbar","#ptool",".gbtn",".gb-pause"].forEach(function(s){var e=ph.querySelector(s);if(gdSeen(e))out.push({b:gdPR(e),w:4})});
  var gf=scr.querySelector(".gfocus");if(G.on&&gdSeen(gf))out.push({b:gdPR(gf),w:8,told:1});return out}
/* an item's target on the screen: its point, the box the bubble and Azu keep clear of, and whether Azu goes beside it */
function gdTarget(it){var P=it.about==="marker"?gdMarker():it.about==="middle"?gdCentral():null;
  if(P)return{x:P.x,y:P.y,name:P.name,box:gdBox(P.x-GD_RING,P.y-GD_RING,2*GD_RING,2*GD_RING),beside:true};
  if(it.about==="gbtn"){var g=ph.querySelector(".gbtn-wrap:not(.intour) .gbtn");if(!gdSeen(g))return null;var B=gdPR(g);return{x:(B.l+B.r)/2,y:(B.t+B.b)/2,name:"the ✦",rect:B,box:{l:B.l-6,t:B.t-6,r:B.r+6,b:B.b+6},beside:false}}
  return null}
/* the bubble's words: plain, or one span a word when it is spoken (the word being said is lit) */
function gdWords(t){return String(t).split(/(\s+)/).map(function(w){return/^\s*$/.test(w)?w:'<span class="gdw">'+esc(w)+'</span>'}).join("")}
function gdBubHtml(c){return'<button type="button" data-gdbub="1">'+(c.spoken?gdWords(c.text):esc(c.text))+'</button><i class="gdtail"></i>'}
/* the tip on the phone: its elements made once, then placed (again at each paint of the wall: the marker moves with the picture) */
function gdPresent(c){gdClearEls();var it=c.item,az=gdMascot();
  var b=document.createElement("div");b.className="gdbub"+(az?"":" plain")+(S.reduce?"":" gd-in");b.setAttribute("role","status");b.setAttribute("aria-live","polite");
  b.innerHTML=gdBubHtml(c);ph.appendChild(b);
  if(it.focus==="ring")gdRing(it,!S.reduce);
  if(it.points&&az&&it.goes==="beside"){var p=document.createElement("i");p.className="gdpoint";p.setAttribute("aria-hidden","true");p.innerHTML='<svg viewBox="0 0 16 16"><path d="M2.5 2.5 14 8 2.5 13.5Z"/></svg>';ph.appendChild(p)}
  gdPlace(c,true)}
/* the rings: on a marker it sits in the wall's layer under the markers, so the marker and its name are drawn over it (never covered; a
   redrawn wall gets it back); round the ✦ it sits just under the button's own layer */
function gdRing(it,pulse){if(it&&it.about==="gbtn"){var q=ph.querySelector(".gdring.gd-rect");if(!q){q=document.createElement("div");q.className="gdring gd-rect"+(pulse?" gd-pulse":"");q.setAttribute("aria-hidden","true");var w0=ph.querySelector(".gbtn-wrap");ph.insertBefore(q,w0||null)}return q}
  var w=document.getElementById("wall");if(!w)return null;var r=w.querySelector(".gdring");if(!r){r=document.createElement("div");r.className="gdring"+(pulse?" gd-pulse":"");r.setAttribute("aria-hidden","true");w.insertBefore(r,w.firstChild)}return r}
function gdClearEls(){ph.querySelectorAll(".gdbub,.gdring,.gdpoint").forEach(function(x){x.remove()})}
/* the solver: Azu's place beside the target (right, left, above, below) and the bubble's (above Azu, below, beside), the one covering least;
   for the ✦ (or no target) the bubble above the ✦ and Azu; cover says when even the best place is over its target or the block being told */
function gdSolve(c,bw,bh){var it=c.item,W=ph.offsetWidth,H=ph.offsetHeight,az=gdMascot(),obs=gdObstacles();
  var bounds=function(x){return Math.max(0,GD_GUT-x.l)*(x.b-x.t)+Math.max(0,x.r-(W-GD_GUT))*(x.b-x.t)+Math.max(0,44-x.t)*(x.r-x.l)+Math.max(0,x.b-(H-GD_GUT))*(x.r-x.l)};
  var cost=function(x,extra){var s=bounds(x)*20;obs.forEach(function(o){s+=gdOver(x,o.b)*o.w});(extra||[]).forEach(function(e){s+=gdOver(x,e)*8});return s};
  var clampL=function(l){return Math.max(GD_GUT,Math.min(W-GD_GUT-bw,l))};
  var M=gdTarget(it),home=null,best=null;
  if(az){var cur=az.style.translate;az.style.translate="";home=gdPR(az);az.style.translate=cur}
  if(M&&M.beside&&it.goes==="beside"&&az){var aw=home.r-home.l,ah=home.b-home.t,ring=M.box;
    [["right",M.x+GD_RING+6,M.y-ah/2],["left",M.x-GD_RING-6-aw,M.y-ah/2],["above",M.x-aw/2,M.y-GD_RING-6-ah],["below",M.x-aw/2,M.y+GD_RING+30]].forEach(function(a,ai){
      var A1=gdBox(a[1],a[2],aw,ah),cx=(A1.l+A1.r)/2;
      [["down",gdBox(clampL(cx-bw/2),A1.t-10-bh,bw,bh)],["up",gdBox(clampL(cx-bw/2),A1.b+10,bw,bh)],
       ["left",gdBox(A1.r+10,(A1.t+A1.b)/2-bh/2,bw,bh)],["right",gdBox(A1.l-10-bw,(A1.t+A1.b)/2-bh/2,bw,bh)]].forEach(function(q,qi){
        var s=cost(A1,[ring])+cost(q[1],[ring,A1])+ai*30+qi*20;if(!best||s<best.s)best={s:s,side:a[0],A:A1,B:q[1],tail:q[0]}})})}
  else if(M&&M.beside){var ring2=M.box;
    [["down",gdBox(clampL(M.x-bw/2),M.y-GD_RING-10-bh,bw,bh)],["left",gdBox(M.x+GD_RING+10,M.y-bh/2,bw,bh)],["right",gdBox(M.x-GD_RING-10-bw,M.y-bh/2,bw,bh)],["up",gdBox(clampL(M.x-bw/2),M.y+GD_RING+34,bw,bh)]].forEach(function(q,qi){
      var s=cost(q[1],[ring2])+qi*20;if(!best||s<best.s)best={s:s,B:q[1],tail:q[0],at:M}})}
  else{var wrap=ph.querySelector(".gbtn-wrap:not(.intour)"),wt=wrap?gdPR(wrap).t:H-80,right=S.guideCfg.gbSide==="right",ex=M?[M.box]:[];
    for(var up=0;up<=160;up+=8){var l=right?W-GD_GUT-bw:GD_GUT,B=gdBox(l,wt-12-bh-up,bw,bh),s=cost(B,ex)+up*4;if(!best||s<best.s)best={s:s,B:B,tail:"down"}}
    best.A=home;if(!home&&M)best.at=M}
  best.M=M;best.home=home;
  if(M&&(gdOver(best.B,M.box)>0||best.A&&best.A!==home&&gdOver(best.A,M.box)>0))best.cover="no room beside "+M.name+" without covering it";
  var told=obs.filter(function(o){return o.told})[0];if(told&&gdOver(best.B,told.b)>0)best.cover="it would cover the block being told";
  return best}
function gdPlace(c,first){var b=ph.querySelector(".gdbub:not(.gd-out)");if(!b||!c)return;var it=c.item,az=gdMascot(),bw=b.offsetWidth,best=gdSolve(c,bw,b.offsetHeight),M=best.M,A=best.A;
  b.style.left=Math.round(best.B.l)+"px";b.style.top=Math.round(best.B.t)+"px";b.dataset.tail=best.tail;b.dataset.side=best.side||"";
  /* the tail: towards Azu, or (Mascot Off) towards the target; none for a plain tip with no target */
  var to=A?{x:(A.l+A.r)/2,y:(A.t+A.b)/2}:best.at||null,tl=b.querySelector(".gdtail"),bh=b.offsetHeight;
  if(!to){b.dataset.tail="none"}else if(tl){if(best.tail==="down"||best.tail==="up")tl.style.left=Math.round(Math.max(14,Math.min(bw-28,to.x-best.B.l-7)))+"px";else tl.style.top=Math.round(Math.max(10,Math.min(bh-24,to.y-best.B.t-7)))+"px"}
  var rg=it.focus==="ring"&&M?gdRing(it,false):null;
  if(rg&&M&&M.rect){rg.style.left=Math.round(M.rect.l-5)+"px";rg.style.top=Math.round(M.rect.t-5)+"px";rg.style.width=Math.round(M.rect.r-M.rect.l+10)+"px";rg.style.height=Math.round(M.rect.b-M.rect.t+10)+"px"}
  else if(rg&&M){rg.style.left=Math.round(M.x)+"px";rg.style.top=Math.round(M.y)+"px"}
  /* Azu: hops to its place beside the target (Reduce Motion: it is there), points at it; at the ✦ it stays and lifts its arm to it */
  if(az&&best.A&&it.goes==="beside"&&M&&M.beside){var dx=Math.round(best.A.l-best.home.l),dy=Math.round(best.A.t-best.home.t),left=M.x<(best.A.l+best.A.r)/2;
    G.gd.az={dx:dx,dy:dy,left:left};gdApplyAz(az,first&&!S.reduce);
    var pt=ph.querySelector(".gdpoint");if(pt){var ax=(best.A.l+best.A.r)/2,ay=(best.A.t+best.A.b)/2,ang=Math.atan2(M.y-ay,M.x-ax),px=M.x-Math.cos(ang)*(GD_RING-5),py=M.y-Math.sin(ang)*(GD_RING-5);
      /* in the gap between the marker and Azu; below the marker its name sits there, so Azu's arm points alone */
      pt.style.display=best.side==="below"?"none":"";pt.style.left=Math.round(px)+"px";pt.style.top=Math.round(py)+"px";pt.style.rotate=(ang*180/Math.PI).toFixed(1)+"deg";if(first&&!S.reduce){pt.classList.remove("gd-in");void pt.offsetWidth;pt.classList.add("gd-in")}}}
  else if(az&&it.points&&it.about==="gbtn"){az.classList.add("point-gb");G.gd.ptGb=true}}
/* Azu away from the ✦ (kept across a redraw of the button: fabLayer.after) */
function gdApplyAz(az,hop){var a=G.gd.az;if(!az||!a)return;az.classList.add("gd-away","gd-point");az.classList.toggle("gd-left",a.left);az.dataset.gdlean=a.left?"left":"right";
  var to=a.dx+"px "+a.dy+"px";if(hop&&az.animate){var from=az.style.translate||"0px 0px",f=from.split(" ").map(parseFloat);az.animate([{translate:from},{translate:((f[0]+a.dx)/2)+"px "+(Math.min(f[1]||0,a.dy)-40)+"px"},{translate:to}],{duration:450,easing:"cubic-bezier(.3,.7,.4,1)"})}
  az.style.translate=to;var arm=az.querySelector(".marm");if(arm){arm.classList.add("on");arm.style.transform=a.left?"rotate(-8deg)":"rotate(8deg)"}}
function gdAzHome(){var az=ph.querySelector(".azfab"),pg=G.gd.ptGb;G.gd.az=null;G.gd.ptGb=false;if(!az)return;if(pg)az.classList.remove("point-gb");var from=az.style.translate;
  if(from&&!S.reduce&&az.animate){var f=from.split(" ").map(parseFloat);az.animate([{translate:from},{translate:(f[0]/2)+"px "+(Math.min(0,f[1]||0)-30)+"px"},{translate:"0px 0px"}],{duration:420,easing:"cubic-bezier(.3,.7,.4,1)"})}
  az.style.translate="";az.classList.remove("gd-away","gd-point","gd-left");delete az.dataset.gdlean;var arm=az.querySelector(".marm");if(arm){arm.classList.remove("on");arm.style.transform=""}}
function gdUnpresent(){gdAzHome();ph.querySelectorAll(".gdbub,.gdring,.gdpoint").forEach(function(x){x.classList.add("gd-out");x.removeAttribute("aria-live");setTimeout(function(){x.remove()},S.reduce?0:170)})}
/* would the item's bubble, at its best place, cover its target or the block being told? (measured with its words, never shown) */
function gdCovers(it){if(!gdTarget(it))return null;var m=document.createElement("div");m.className="gdbub";m.style.visibility="hidden";
  m.innerHTML=gdBubHtml({text:gdLine(it,S.gdVisitor.visits>1,gdSlots(it))});ph.appendChild(m);var best=gdSolve({item:it},m.offsetWidth,m.offsetHeight);m.remove();return best.cover||null}
/* a spoken tip: its line through the one order of voices (guide/voice.js's say), the word being said lit */
function gdVoice(c){say(c.text,function(k){ph.querySelectorAll(".gdbub:not(.gd-out) .gdw").forEach(function(w,i){w.classList.toggle("on",i===k)})},function(){ph.querySelectorAll(".gdbub .gdw.on").forEach(function(w){w.classList.remove("on")})})}
on("fabLayer.after",function(){if(G.gd.az)gdApplyAz(ph.querySelector(".azfab"),false);if(G.gd.ptGb&&G.gd.cur){var az=ph.querySelector(".azfab");if(az)az.classList.add("point-gb")}});
on("camPaint.after",function(){if(G.gd.cur)gdPlace(G.gd.cur,false)});
/* and 4 times a second: what it keeps clear of may move (the ✦'s label folds, a remark comes), never during Azu's hop */
setInterval(function(){var c=G.gd.cur,az=ph.querySelector(".azfab");if(!c||noDraw||az&&az.getAnimations&&az.getAnimations().length)return;gdPlace(c,false)},250);
/* the words may change in the Editor while the tip shows */
on("renderPhone.after",function(){var c=G.gd.cur;if(!c)return;var t=gdLine(c.item,c.ret,c.slots),b=ph.querySelector(".gdbub:not(.gd-out) button[data-gdbub]");if(b&&t!==c.text){c.text=t;if(c.spoken)b.innerHTML=gdWords(t);else b.textContent=t}gdPlace(c,false)});
/* one at a time: Azu's remark and the hint wait while a tip shows */
on("fabRemark.quiet",function(){if(G.gd.cur)return true});
/* ----- the bubble's controls: a tap does its action; a swipe away dismisses; a long press asks Fewer like this? ----- */
function gdTap(){var c=G.gd.cur;if(!c)return;var it=c.item;if(c.pull){gdStop("tapped (a demo)");return}
  if(it.tap==="view"){gdStop("tapped: Ask about the view opens","acted");G.fab.bub=null;G.fab.menu=false;G.cam.open=true;renderPhone(true);emit("view.opened","the tip");return}
  if(it.tap==="tour"){gdStop("tapped: the tour of "+(c.slots.point||"the point")+" starts","acted");S.stop="half";renderPhone(false);emit("card.opened","the tour invitation");startTour();return}
  gdStop("tapped","acted")}
function gdSwipe(dx){var c=G.gd.cur;if(!c)return;var b=ph.querySelector(".gdbub:not(.gd-out)");if(b){b.style.opacity="";if(!S.reduce)b.style.translate=(dx>0?1:-1)*Math.round(ph.offsetWidth*.7)+"px 0px"}
  if(c.pull){gdStop("swiped away (a demo)");return}gdStop("swiped away","dismissed")}
function gdAskFewer(){var c=G.gd.cur,b=ph.querySelector(".gdbub:not(.gd-out)");if(!c||!b||!S.guidance.fewer||c.ask)return false;
  c.ask=true;c.askUntil=gdNow()+8000;c.askReal=performance.now()+8000;b.classList.add("asking");b.style.translate="";b.style.opacity="";
  b.innerHTML='<div class="gdfewer" role="group" aria-label="'+TXH("gd_fewer")+'"><span class="gdfq">'+TXH("gd_fewer")+'</span><span class="gdfb"><button type="button" data-gdfewer="yes">'+TXH("gd_fewer_yes")+'</button><button type="button" data-gdfewer="no">'+TXH("gd_fewer_no")+'</button></span></div><i class="gdtail"></i>';
  gdPlace(c,false);if(!c.pull)gdLog("event","a long press on the "+c.item.label+": Fewer like this?");return true}
function gdFewer(){var c=G.gd.cur;if(!c)return;var g=gdGroupOf(c.item);if(c.pull){gdStop("Fewer like this (a demo)");return}gdStop("Fewer like this: "+(g?g.name:"its group")+" rests for the visit","fewer")}
function gdKeep(){var c=G.gd.cur,b=ph.querySelector(".gdbub:not(.gd-out)");if(!c||!b)return;c.ask=false;b.classList.remove("asking");b.innerHTML=gdBubHtml(c);c.until=Math.max(c.until,gdNow()+3000);gdPlace(c,false)}
ph.addEventListener("pointerdown",function(e){var b=e.target.closest(".gdbub:not(.gd-out)");if(!b||!G.gd.cur||e.target.closest("[data-gdfewer]"))return;
  var p={x:e.clientX,y:e.clientY,id:e.pointerId,moved:false,held:false,dx:0};G.gd.press=p;clearTimeout(G.gd.holdT);try{e.target.setPointerCapture(e.pointerId)}catch(x){}
  G.gd.holdT=setTimeout(function(){if(G.gd.press===p&&!p.moved){p.held=true;gdAskFewer()}},GD_HOLD)},true);
ph.addEventListener("pointermove",function(e){var p=G.gd.press;if(!p||e.pointerId!==p.id||p.held)return;var q=ph.getBoundingClientRect().width/ph.offsetWidth,dx=(e.clientX-p.x)/q,dy=(e.clientY-p.y)/q;
  if(Math.hypot(dx,dy)>10){p.moved=true;clearTimeout(G.gd.holdT)}if(!p.moved)return;p.dx=dx;var b=ph.querySelector(".gdbub:not(.gd-out)");if(!b)return;if(!S.reduce)b.style.translate=Math.round(dx)+"px 0px";b.style.opacity=String(Math.max(.4,1-Math.abs(dx)/240))},true);
["pointerup","pointercancel"].forEach(function(t){ph.addEventListener(t,function(e){var p=G.gd.press;if(!p||e.pointerId!==p.id)return;G.gd.press=null;clearTimeout(G.gd.holdT);
  if(p.held||p.moved)G.gd.noClickUntil=performance.now()+400;if(!p.moved)return;
  if(t==="pointerup"&&Math.abs(p.dx)>=GD_SWIPE){gdSwipe(p.dx);return}var b=ph.querySelector(".gdbub:not(.gd-out)");if(b){b.style.translate="";b.style.opacity=""}},true)});
ph.addEventListener("click",function(e){var f=e.target.closest("[data-gdfewer]");if(f){e.stopPropagation();if(f.dataset.gdfewer==="yes")gdFewer();else gdKeep();return}
  var t=e.target.closest("[data-gdbub]");if(!t)return;e.stopPropagation();if(performance.now()<(G.gd.noClickUntil||0))return;gdTap()},true);
/* the Editor's demos: a chip on the phone with the ×10 clock (Editor only, never in the app; Unity: a Play-mode preview label) */
function gdChip(text){var c=ph.querySelector(".gdchip");if(!text){if(c)c.remove();return}if(!c){c=document.createElement("div");c.className="gdchip";c.setAttribute("aria-hidden","true");ph.appendChild(c)}c.textContent=text}
