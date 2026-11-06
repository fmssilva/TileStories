/* Guidance, the look (_3.8 section 4): a tip is one bubble (the dock's paper, 7:1 words, at most 2 lines, a tail to the mascot), and
   for an item about the nearest marker the mascot goes beside it (a 0.45 s arc hop), points at it, and a ring circles the marker (one
   soft pulse, then still). Placed by a small solver: the side with room, never over the marker, the words on the wall, the ✦ or the
   bar. Reduce Motion: no hop (Azu is there), a still ring. Mascot Off (Guide button = ✦ only, or Mascot > Character = Off): a plain
   bubble at the target, no character. App items are a bubble only, never spoken (_3.8 decision 2). The ×10 chip of the Editor's
   demos lives here too (gdChip). Unity: the bubble a 9-slice VisualElement (USS transitions on scale and opacity, its tail a rotated
   child), placed by the same solver on worldBound; the mascot's hop a translate tween (DOTween) on its element, the point the 3D
   model's Animator point clip (the Lab's still picture leans and shows a small pointer instead); the ring a VisualElement with a
   border radius and two borders (USS), its pulse a USS transition on scale; the marker's screen point from
   RuntimePanelUtils.CameraTransformWorldToPanel. */
var GD_RING=36,GD_GUT=12;
G.gd.az=null;
function gdPR(el){var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,r=el.getBoundingClientRect();return{l:(r.left-pr.left)/q,t:(r.top-pr.top)/q,r:(r.right-pr.left)/q,b:(r.bottom-pr.top)/q}}
function gdBox(l,t,w,h){return{l:l,t:t,r:l+w,b:t+h}}
function gdOver(a,b){var x=Math.min(a.r,b.r)-Math.max(a.l,b.l),y=Math.min(a.b,b.b)-Math.max(a.t,b.t);return x>0&&y>0?x*y:0}
function gdSeen(e){return e&&e.getClientRects().length>0&&getComputedStyle(e).visibility!=="hidden"}
/* the mascot beside the ✦, when there is one to show (Mascot Off: none) */
function gdMascot(){return S.guideCfg.mascot!==false&&gbAzu()?ph.querySelector(".azfab"):null}
/* what a tip must keep clear of: the words on the wall (marker and point names, Azu's remark), the markers, the ✦, the bar, the preview bar */
function gdObstacles(){var out=[];
  ph.querySelectorAll("#wall .mk b,.campins .cpin b,.azfabbub,.azbub").forEach(function(e){if(gdSeen(e))out.push({b:gdPR(e),w:6,text:1})});
  ph.querySelectorAll("#wall .mk span,.campins .cpin i").forEach(function(e){if(gdSeen(e))out.push({b:gdPR(e),w:4})});
  ["#navbar","#ptool",".gbtn",".gb-pause"].forEach(function(s){var e=ph.querySelector(s);if(gdSeen(e))out.push({b:gdPR(e),w:4})});return out}
/* the tip on the phone: its elements made once, then placed (again at each paint of the wall: the marker moves with the picture) */
function gdPresent(c){gdClearEls();var it=c.item,az=gdMascot();
  var b=document.createElement("div");b.className="gdbub"+(az?"":" plain")+(S.reduce?"":" gd-in");b.setAttribute("role","status");b.setAttribute("aria-live","polite");
  b.innerHTML='<button type="button" data-gdbub="1">'+esc(c.text)+'</button><i class="gdtail"></i>';ph.appendChild(b);
  if(it.focus==="ring"&&it.about==="marker")gdRing(!S.reduce);
  if(it.points&&az){var p=document.createElement("i");p.className="gdpoint";p.setAttribute("aria-hidden","true");p.innerHTML='<svg viewBox="0 0 16 16"><path d="M2.5 2.5 14 8 2.5 13.5Z"/></svg>';ph.appendChild(p)}
  gdPlace(c,true)}
/* the ring sits in the wall's layer under the markers, so the marker and its name are drawn over it (never covered); a redrawn wall gets it back */
function gdRing(pulse){var w=document.getElementById("wall");if(!w)return null;var r=w.querySelector(".gdring");if(!r){r=document.createElement("div");r.className="gdring"+(pulse?" gd-pulse":"");r.setAttribute("aria-hidden","true");w.insertBefore(r,w.firstChild)}return r}
function gdClearEls(){ph.querySelectorAll(".gdbub,.gdring,.gdpoint").forEach(function(x){x.remove()})}
/* the solver: Azu's place beside the marker (right, left, above, below) and the bubble's (above Azu, below, beside), the one covering least */
function gdPlace(c,first){var b=ph.querySelector(".gdbub");if(!b||!c)return;var it=c.item,W=ph.offsetWidth,H=ph.offsetHeight,az=gdMascot(),obs=gdObstacles(),bw=b.offsetWidth,bh=b.offsetHeight;
  var bounds=function(x){return Math.max(0,GD_GUT-x.l)*(x.b-x.t)+Math.max(0,x.r-(W-GD_GUT))*(x.b-x.t)+Math.max(0,44-x.t)*(x.r-x.l)+Math.max(0,x.b-(H-GD_GUT))*(x.r-x.l)};
  var cost=function(x,extra){var s=bounds(x)*20;obs.forEach(function(o){s+=gdOver(x,o.b)*o.w});(extra||[]).forEach(function(e){s+=gdOver(x,e)*8});return s};
  var clampL=function(l){return Math.max(GD_GUT,Math.min(W-GD_GUT-bw,l))};
  var M=it.about==="marker"?gdMarker():null,home=null,A=null,best=null;
  if(az){var cur=az.style.translate;az.style.translate="";home=gdPR(az);az.style.translate=cur}
  if(M&&it.goes==="beside"&&az){var aw=home.r-home.l,ah=home.b-home.t,ring=gdBox(M.x-GD_RING,M.y-GD_RING,2*GD_RING,2*GD_RING);
    [["right",M.x+GD_RING+6,M.y-ah/2],["left",M.x-GD_RING-6-aw,M.y-ah/2],["above",M.x-aw/2,M.y-GD_RING-6-ah],["below",M.x-aw/2,M.y+GD_RING+30]].forEach(function(a,ai){
      var A1=gdBox(a[1],a[2],aw,ah),cx=(A1.l+A1.r)/2;
      [["down",gdBox(clampL(cx-bw/2),A1.t-10-bh,bw,bh)],["up",gdBox(clampL(cx-bw/2),A1.b+10,bw,bh)],
       ["left",gdBox(A1.r+10,(A1.t+A1.b)/2-bh/2,bw,bh)],["right",gdBox(A1.l-10-bw,(A1.t+A1.b)/2-bh/2,bw,bh)]].forEach(function(q,qi){
        var s=cost(A1,[ring])+cost(q[1],[ring,A1])+ai*30+qi*20;if(!best||s<best.s)best={s:s,side:a[0],A:A1,B:q[1],tail:q[0]}})});
    A=best.A}
  else if(M){var ring2=gdBox(M.x-GD_RING,M.y-GD_RING,2*GD_RING,2*GD_RING);
    [["down",gdBox(clampL(M.x-bw/2),M.y-GD_RING-10-bh,bw,bh)],["left",gdBox(M.x+GD_RING+10,M.y-bh/2,bw,bh)],["right",gdBox(M.x-GD_RING-10-bw,M.y-bh/2,bw,bh)],["up",gdBox(clampL(M.x-bw/2),M.y+GD_RING+34,bw,bh)]].forEach(function(q,qi){
      var s=cost(q[1],[ring2])+qi*20;if(!best||s<best.s)best={s:s,B:q[1],tail:q[0],at:M}})}
  else{var wrap=ph.querySelector(".gbtn-wrap:not(.intour)"),wt=wrap?gdPR(wrap).t:H-80,right=S.guideCfg.gbSide==="right";
    for(var up=0;up<=160;up+=8){var l=right?W-GD_GUT-bw:GD_GUT,B=gdBox(l,wt-12-bh-up,bw,bh),s=cost(B)+up*4;if(!best||s<best.s)best={s:s,B:B,tail:"down"}}
    A=home}
  b.style.left=Math.round(best.B.l)+"px";b.style.top=Math.round(best.B.t)+"px";b.dataset.tail=best.tail;b.dataset.side=best.side||"";
  /* the tail: towards Azu, or (Mascot Off) towards the marker; none for a plain tip with no target */
  var to=A?{x:(A.l+A.r)/2,y:(A.t+A.b)/2}:best.at||null,tl=b.querySelector(".gdtail");
  if(!to){b.dataset.tail="none"}else if(best.tail==="down"||best.tail==="up")tl.style.left=Math.round(Math.max(14,Math.min(bw-28,to.x-best.B.l-7)))+"px";else tl.style.top=Math.round(Math.max(10,Math.min(bh-24,to.y-best.B.t-7)))+"px";
  var rg=it.focus==="ring"&&M?gdRing(false):null;if(rg&&M){rg.style.left=Math.round(M.x)+"px";rg.style.top=Math.round(M.y)+"px"}
  /* Azu: hops to its place beside the marker (Reduce Motion: it is there), points at it */
  if(az&&best.A&&it.goes==="beside"){var dx=Math.round(best.A.l-home.l),dy=Math.round(best.A.t-home.t),left=M.x<(best.A.l+best.A.r)/2;
    G.gd.az={dx:dx,dy:dy,left:left};gdApplyAz(az,first&&!S.reduce);
    var pt=ph.querySelector(".gdpoint");if(pt){var ax=(best.A.l+best.A.r)/2,ay=(best.A.t+best.A.b)/2,ang=Math.atan2(M.y-ay,M.x-ax),px=M.x-Math.cos(ang)*(GD_RING+10),py=M.y-Math.sin(ang)*(GD_RING+10);
      pt.style.left=Math.round(px)+"px";pt.style.top=Math.round(py)+"px";pt.style.rotate=(ang*180/Math.PI).toFixed(1)+"deg";if(first&&!S.reduce){pt.classList.remove("gd-in");void pt.offsetWidth;pt.classList.add("gd-in")}}}}
/* Azu away from the ✦ (kept across a redraw of the button: fabLayer.after) */
function gdApplyAz(az,hop){var a=G.gd.az;if(!az||!a)return;az.classList.add("gd-away","gd-point");az.classList.toggle("gd-left",a.left);az.dataset.gdlean=a.left?"left":"right";
  var to=a.dx+"px "+a.dy+"px";if(hop&&az.animate){var from=az.style.translate||"0px 0px",f=from.split(" ").map(parseFloat);az.animate([{translate:from},{translate:((f[0]+a.dx)/2)+"px "+(Math.min(f[1]||0,a.dy)-40)+"px"},{translate:to}],{duration:450,easing:"cubic-bezier(.3,.7,.4,1)"})}
  az.style.translate=to;var arm=az.querySelector(".marm");if(arm){arm.classList.add("on");arm.style.transform=a.left?"rotate(-8deg)":"rotate(8deg)"}}
function gdAzHome(){var az=ph.querySelector(".azfab");G.gd.az=null;if(!az)return;var from=az.style.translate;
  if(from&&!S.reduce&&az.animate){var f=from.split(" ").map(parseFloat);az.animate([{translate:from},{translate:(f[0]/2)+"px "+(Math.min(0,f[1]||0)-30)+"px"},{translate:"0px 0px"}],{duration:420,easing:"cubic-bezier(.3,.7,.4,1)"})}
  az.style.translate="";az.classList.remove("gd-away","gd-point","gd-left");delete az.dataset.gdlean;var arm=az.querySelector(".marm");if(arm){arm.classList.remove("on");arm.style.transform=""}}
function gdUnpresent(){gdAzHome();ph.querySelectorAll(".gdbub,.gdring,.gdpoint").forEach(function(x){x.classList.add("gd-out");x.removeAttribute("aria-live");setTimeout(function(){x.remove()},S.reduce?0:170)})}
on("fabLayer.after",function(){if(G.gd.az)gdApplyAz(ph.querySelector(".azfab"),false)});
on("camPaint.after",function(){if(G.gd.cur)gdPlace(G.gd.cur,false)});
/* and 4 times a second: what it keeps clear of may move (the ✦'s label folds, a remark comes), never during Azu's hop */
setInterval(function(){var c=G.gd.cur,az=ph.querySelector(".azfab");if(!c||noDraw||az&&az.getAnimations&&az.getAnimations().length)return;gdPlace(c,false)},250);
/* the words may change in the Editor while the tip shows */
on("renderPhone.after",function(){var c=G.gd.cur;if(!c)return;var t=gdLine(c.item,S.gdVisitor.visits>1),b=ph.querySelector(".gdbub button");if(b&&t!==c.text){c.text=t;b.textContent=t}gdPlace(c,false)});
/* one at a time: Azu's remark and the hint wait while a tip shows */
on("fabRemark.quiet",function(){if(G.gd.cur)return true});
/* a tap on the bubble dismisses it */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gdbub]");if(!t)return;e.stopPropagation();gdStop("dismissed by a tap",true)},true);
/* the Editor's demos: a chip on the phone with the ×10 clock (Editor only, never in the app; Unity: a Play-mode preview label) */
function gdChip(text){var c=ph.querySelector(".gdchip");if(!text){if(c)c.remove();return}if(!c){c=document.createElement("div");c.className="gdchip";c.setAttribute("aria-hidden","true");ph.appendChild(c)}c.textContent=text}
