/* Guidance, the look (_3.8 section 4): a tip is one bubble (the dock's paper, 7:1 words, at most 2 lines, a tail to the mascot). Its
   target: the nearest marker (Tap a marker), the point in the middle (A tour of {point}, and Azu's remark on a point, {point} in the
   middle, whose bubble ends with a › to say a tap does more): the mascot goes beside it (a 0.45 s arc hop) and points, Tap a marker's
   ring circles the marker (one soft pulse, then still); the ✦ (Ask about the view, The ✦): Azu stays beside it and lifts its arm to it
   (point-gb; the guide's Azu in a tour, the 3D model, G.pointTo), a ring round the ✦ for Ask about the view; under Reduce Motion the
   arm stays down (the words only). Placed by a small solver: the side with room, never over its target, the words on the wall, the ✦,
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
   translate tween.
   Round 7: a Moment (Fun › Moments) is Azu's own look, unchanged: Azu walks to the middle with its prop and asks for one tap
   (mascot/azu2d.js's azMoment, its words the item's Says); no bubble of the guidance. Unity: the prop and Azu's reactions are the Rive
   file's states (drink, eat), the bubble as every tip's. A tap on the treasure hunt's invitation (or its chip, Start the hunt) opens Ask
   about the view and starts its hunt (camAsk's own, no copy). */
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
  ph.querySelectorAll("#wall .mk b,.campins .cpin b,.azbub").forEach(function(e){if(gdSeen(e))out.push({b:gdPR(e),w:6,text:1})});
  ph.querySelectorAll("#wall .mk span,.campins .cpin i").forEach(function(e){if(gdSeen(e))out.push({b:gdPR(e),w:4})});
  ["#navbar","#ptool",".gbtn",".gb-pause"].forEach(function(s){var e=ph.querySelector(s);if(gdSeen(e))out.push({b:gdPR(e),w:4})});
  var gf=scr.querySelector(".gfocus");if(G.on&&gdSeen(gf))out.push({b:gdPR(gf),w:8,told:1});return out}
/* an item's target on the screen: its point, the box the bubble and Azu keep clear of, and whether Azu goes beside it */
/* the ✦'s wrap: outside a tour, or (a pull in a tour) the one above the dock */
/* round 6: Mascot goes Beside it or Walks to it: Azu leaves the ✦ for its place beside the target */
function gdGoes(it){return it.goes==="beside"||it.goes==="walk"}
function gdGbWrap(){return ph.querySelector(".gbtn-wrap:not(.intour)")||(G.on?ph.querySelector(".gbtn-wrap.intour"):null)}
/* a point of the wall on the screen (its item's own point, else the one in the middle) */
function gdPointNow(it){var c=gdCentral();if(c&&(!it.point||c.id===it.point))return c;if(typeof camView!=="function")return c;var p=camView().pois.filter(function(x){return x.id===it.point})[0];
  return p?{id:p.id,name:p.name,x:p.x,y:p.y,middle:p.where==="middle"}:c}
function gdTarget(it){var P=it.about==="marker"?gdMarker():it.about==="middle"?gdCentral():it.about==="point"?gdPointNow(it):null;
  if(P)return{x:P.x,y:P.y,name:P.name,box:gdBox(P.x-GD_RING,P.y-GD_RING,2*GD_RING,2*GD_RING),beside:true};
  if(it.about==="gbtn"){var w=gdGbWrap(),g=w&&w.querySelector(".gbtn");if(!gdSeen(g))return null;var B=gdPR(g);return{x:(B.l+B.r)/2,y:(B.t+B.b)/2,name:"the ✦",rect:B,box:{l:B.l-6,t:B.t-6,r:B.r+6,b:B.b+6},beside:false}}
  return null}
/* the bubble's words: plain, or one span a word when it is spoken (the word being said is lit) */
function gdWords(t){return String(t).split(/(\s+)/).map(function(w){return/^\s*$/.test(w)?w:'<span class="gdw">'+esc(w)+'</span>'}).join("")}
/* the bubble's chip: How does this work?'s Open a card (cta), or the item's Button (round 6: Show me / Start the tour / Open the card / Ask
   about the view); with Tips stay until I tap them (or On screen for: Until tapped) a dismiss button, so the visitor can close it without acting */
function gdBtnOf(it){return!it?null:it.cta?{word:it.cta,act:"card"}:it.button&&it.button!=="none"&&GD_BTN_WORD[it.button]?{word:GD_BTN_WORD[it.button],act:it.button}:null}
function gdStays(c){return!!c&&!c.pull&&!c.cue&&(c.item.onScreen==="tap"||!!gdPrefs().stay)}
function gdBubHtml(c){var bt=gdBtnOf(c.item),cta=bt?'<button type="button" class="gdcta" data-gdcta="'+bt.act+'">'+TXH(bt.word)+'<span aria-hidden="true">&#8250;</span></button>':"";
  var x=gdStays(c)?'<button type="button" class="gdx" data-gdx="1" aria-label="'+TXH("gd_dismiss")+'"><span aria-hidden="true">&#10005;</span></button>':"";
  return'<button type="button" data-gdbub="1">'+(c.spoken?gdWords(c.text):esc(c.text))+'</button>'+cta+x+'<i class="gdtail"></i>'}
/* the tip on the phone: its elements made once, then placed (again at each paint of the wall: the marker moves with the picture) */
function gdPresent(c){gdClearEls();var it=c.item,az=gdMascot();
  /* a Moment: Azu's prop and bubble (as before); when it ends, the tour's next step it held goes on */
  if(it.form==="moment"){var pull=c.pull;G.gd.mom=true;azMoment(it.prop==="cookie"?"cookie":"water",function(){gdResume(pull)},c.text);return}
  var b=document.createElement("div");b.className="gdbub"+(az?"":" plain")+(S.reduce?"":" gd-in")+(it.form==="remark"?" gd-more":"")+(gdBtnOf(it)?" gd-cta":"")+(gdStays(c)?" gd-stay":"");b.setAttribute("role","status");b.setAttribute("aria-live","polite");
  b.innerHTML=gdBubHtml(c);ph.appendChild(b);
  if(gdFocusNow(it)==="ring")gdRing(it,!S.reduce);
  if(it.points&&az&&gdGoes(it)){var p=document.createElement("i");p.className="gdpoint";p.setAttribute("aria-hidden","true");p.innerHTML='<svg viewBox="0 0 16 16"><path d="M2.5 2.5 14 8 2.5 13.5Z"/></svg>';ph.appendChild(p)}
  gdPlace(c,true)}
/* the rings: on a marker it sits in the wall's layer under the markers, so the marker and its name are drawn over it (never covered; a
   redrawn wall gets it back); round the ✦ it sits just under the button's own layer */
function gdRing(it,pulse){if(it&&it.about==="gbtn"){var q=ph.querySelector(".gdring.gd-rect");if(!q){q=document.createElement("div");q.className="gdring gd-rect"+(pulse?" gd-pulse":"");q.setAttribute("aria-hidden","true");var w0=ph.querySelector(".gbtn-wrap");ph.insertBefore(q,w0||null)}return q}
  var w=document.getElementById("wall");if(!w)return null;var r=w.querySelector(".gdring");if(!r){r=document.createElement("div");r.className="gdring"+(pulse?" gd-pulse":"");r.setAttribute("aria-hidden","true");w.insertBefore(r,w.firstChild)}return r}
function gdClearEls(){ph.querySelectorAll(".gdbub,.gdring,.gdpoint,.gdglint,.gspot.gdspot,.gdfinger,.gdripple").forEach(function(x){x.remove()})}
/* the solver: Azu's place beside the target (right, left, above, below) and the bubble's (above Azu, below, beside), the one covering least;
   for the ✦ (or no target) the bubble above the ✦ and Azu; cover says when even the best place is over its target or the block being told */
function gdSolve(c,bw,bh){var it=c.item,W=ph.offsetWidth,H=ph.offsetHeight,az=gdMascot(),obs=gdObstacles();
  var bounds=function(x){return Math.max(0,GD_GUT-x.l)*(x.b-x.t)+Math.max(0,x.r-(W-GD_GUT))*(x.b-x.t)+Math.max(0,44-x.t)*(x.r-x.l)+Math.max(0,x.b-(H-GD_GUT))*(x.r-x.l)};
  /* round 7: any overlap with words costs a fixed 500 on top of its area (a sliver of a name was cheaper than the next side: 0 px over words) */
  var cost=function(x,extra){var s=bounds(x)*20;obs.forEach(function(o){var a=gdOver(x,o.b);s+=a*o.w+(a>0&&o.text?500:0)});(extra||[]).forEach(function(e){s+=gdOver(x,e)*8});return s};
  var clampL=function(l){return Math.max(GD_GUT,Math.min(W-GD_GUT-bw,l))};
  var M=gdTarget(it),home=null,best=null;
  if(az){var cur=az.style.translate;az.style.translate="";home=gdPR(az);az.style.translate=cur}
  if(M&&M.beside&&gdGoes(it)&&az){var aw=home.r-home.l,ah=home.b-home.t,ring=M.box;
    [["right",M.x+GD_RING+6,M.y-ah/2],["left",M.x-GD_RING-6-aw,M.y-ah/2],["above",M.x-aw/2,M.y-GD_RING-6-ah],["below",M.x-aw/2,M.y+GD_RING+30]].forEach(function(a,ai){
      var A1=gdBox(a[1],a[2],aw,ah),cx=(A1.l+A1.r)/2;
      [["down",gdBox(clampL(cx-bw/2),A1.t-10-bh,bw,bh)],["up",gdBox(clampL(cx-bw/2),A1.b+10,bw,bh)],
       ["left",gdBox(A1.r+10,(A1.t+A1.b)/2-bh/2,bw,bh)],["right",gdBox(A1.l-10-bw,(A1.t+A1.b)/2-bh/2,bw,bh)]].forEach(function(q,qi){
        var s=cost(A1,[ring])+cost(q[1],[ring,A1])+ai*30+qi*20;if(!best||s<best.s)best={s:s,side:a[0],A:A1,B:q[1],tail:q[0]}})})}
  else if(M&&M.beside){var ring2=M.box;
    [["down",gdBox(clampL(M.x-bw/2),M.y-GD_RING-10-bh,bw,bh)],["left",gdBox(M.x+GD_RING+10,M.y-bh/2,bw,bh)],["right",gdBox(M.x-GD_RING-10-bw,M.y-bh/2,bw,bh)],["up",gdBox(clampL(M.x-bw/2),M.y+GD_RING+34,bw,bh)]].forEach(function(q,qi){
      var s=cost(q[1],[ring2])+qi*20;if(!best||s<best.s)best={s:s,B:q[1],tail:q[0],at:M}})}
  else{var wrap=gdGbWrap(),wt=wrap?gdPR(wrap).t:H-80,right=S.guideCfg.gbSide==="right",ex=M?[M.box]:[];
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
  var fc=gdFocusNow(it),rg=fc==="ring"&&M?gdRing(it,false):null;
  if(rg&&M&&M.rect){rg.style.left=Math.round(M.rect.l-5)+"px";rg.style.top=Math.round(M.rect.t-5)+"px";rg.style.width=Math.round(M.rect.r-M.rect.l+10)+"px";rg.style.height=Math.round(M.rect.b-M.rect.t+10)+"px"}
  else if(rg&&M){rg.style.left=Math.round(M.x)+"px";rg.style.top=Math.round(M.y)+"px"}
  /* Azu: hops to its place beside the target (Reduce Motion: it is there), points at it; at the ✦ it stays and lifts its arm to it */
  G.gd.hopMs=0;
  if(az&&best.A&&gdGoes(it)&&M&&M.beside){var dx=Math.round(best.A.l-best.home.l),dy=Math.round(best.A.t-best.home.t),left=M.x<(best.A.l+best.A.r)/2;
    G.gd.az={dx:dx,dy:dy,left:left,walk:it.goes==="walk"};gdApplyAz(az,first&&!S.reduce);
    var pt=ph.querySelector(".gdpoint");if(pt){var ax=(best.A.l+best.A.r)/2,ay=(best.A.t+best.A.b)/2,ang=Math.atan2(M.y-ay,M.x-ax),px=M.x-Math.cos(ang)*(GD_RING-5),py=M.y-Math.sin(ang)*(GD_RING-5);
      /* in the gap between the marker and Azu; below the marker its name sits there, so Azu's arm points alone */
      pt.style.display=best.side==="below"?"none":"";pt.style.left=Math.round(px)+"px";pt.style.top=Math.round(py)+"px";pt.style.rotate=(ang*180/Math.PI).toFixed(1)+"deg";if(first&&!S.reduce){pt.classList.remove("gd-in");void pt.offsetWidth;pt.classList.add("gd-in")}}}
  else if(it.points&&it.about==="gbtn"&&!S.reduce){if(az){az.classList.add("point-gb");G.gd.ptGb=true}
    /* no Azu beside the ✦ (a pull in a tour): the guide's own Azu turns to it (the 3D model reads G.pointTo) */
    else if(!G.gd.ptTo&&typeof azuEl==="function"&&azuEl()){G.gd.ptTo=true;G.pointTo={till:performance.now()+Math.max(1500,c.until-c.at),at:function(){var w=gdGbWrap(),g=w&&w.querySelector(".gbtn");if(!g)return null;var B=gdPR(g);return{x:(B.l+B.r)/2,y:(B.t+B.b)/2}}}}}
  /* round 6: the Glint and the Spotlight on the target; the ghost finger once, after Azu's hop */
  if(fc==="glint"&&M)gdGlint(it,M,first);var sp=ph.querySelector(".gspot.gdspot");if(fc==="spotlight"&&M)gdSpotTip(it,M);else if(sp)sp.remove();
  if(first&&M&&gdGestureOf(it))gdFinger(it,M,null)}
/* Azu away from the ✦ (kept across a redraw of the button: fabLayer.after) */
/* round 6: the hop is a short arc (0.45 s, a parabola sampled along an eased time); Walks to it (on the wall) is a slower walk along the
   bottom, a small step bob, then up to its place. Unity: a translate tween on an arc (DOTween DOJump on the mascot's VisualElement) / along
   a two-leg path (DOPath) with the Animator's walk clip; Reduce Motion: no tween, the mascot is there */
function gdArcFrames(x0,y0,x1,y1){var h=Math.min(70,40+Math.hypot(x1-x0,y1-y0)*.08),out=[];for(var i=0;i<=8;i++){var t=i/8,e=1-Math.pow(1-t,2.2);out.push({translate:(x0+(x1-x0)*e).toFixed(1)+"px "+(y0+(y1-y0)*e-h*4*e*(1-e)).toFixed(1)+"px",offset:t})}return out}
function gdWalkMs(x0,y0,x1,y1){return Math.round(Math.max(1100,Math.min(2400,700+(Math.abs(x1-x0)+Math.abs(y1-y0))*3.2)))}
function gdWalkFrames(x0,y0,x1,y1){var a=Math.abs(x1-x0),b=Math.abs(y1-y0),L=a+b||1,out=[{translate:x0+"px "+y0+"px",offset:0}],n=Math.max(2,Math.round(a/26));
  for(var i=1;i<=n;i++){var t=i/n,x=x0+(x1-x0)*t;out.push({translate:x.toFixed(1)+"px "+(y0-(i%2?4:0))+"px",offset:+(a*t/L).toFixed(4)})}
  out[out.length-1].translate=x1+"px "+y0+"px";out.push({translate:x1+"px "+y1+"px",offset:1});return out}
function gdApplyAz(az,hop){var a=G.gd.az;if(!az||!a)return;az.classList.add("gd-away","gd-point");az.classList.toggle("gd-left",a.left);az.dataset.gdlean=a.left?"left":"right";
  var to=a.dx+"px "+a.dy+"px";if(hop&&az.animate){var f=(az.style.translate||"0px 0px").split(" ").map(parseFloat),x0=f[0]||0,y0=f[1]||0,ms=a.walk?gdWalkMs(x0,y0,a.dx,a.dy):450;G.gd.hopMs=ms;
    az.animate(a.walk?gdWalkFrames(x0,y0,a.dx,a.dy):gdArcFrames(x0,y0,a.dx,a.dy),{duration:ms,easing:a.walk?"ease-in-out":"linear"})}
  az.style.translate=to;var arm=az.querySelector(".marm");if(arm){arm.classList.add("on");arm.style.transform=a.left?"rotate(-8deg)":"rotate(8deg)"}}
function gdAzHome(){var az=ph.querySelector(".azfab"),pg=G.gd.ptGb;G.gd.az=null;G.gd.ptGb=false;if(G.gd.ptTo){G.gd.ptTo=false;G.pointTo=null}if(!az)return;if(pg)az.classList.remove("point-gb");var from=az.style.translate;
  if(from&&!S.reduce&&az.animate){var f=from.split(" ").map(parseFloat);az.animate([{translate:from},{translate:(f[0]/2)+"px "+(Math.min(0,f[1]||0)-30)+"px"},{translate:"0px 0px"}],{duration:420,easing:"cubic-bezier(.3,.7,.4,1)"})}
  az.style.translate="";az.classList.remove("gd-away","gd-point","gd-left");delete az.dataset.gdlean;var arm=az.querySelector(".marm");if(arm){arm.classList.remove("on");arm.style.transform=""}}
/* the tour's next step, held by a Moment, goes on (a demo's Moment resumes the tour it paused) */
function gdResume(pull){if(G.gd.hold){G.gd.hold=false;if(G.on&&!G.paused)nextStep()}else if(pull&&G.on&&G.paused)resumeTour()}
function gdUnpresent(){if(G.gd.mom){G.gd.mom=false;if(G.moment)azMomentEnd(false)}gdAzHome();ph.querySelectorAll(".gdbub,.gdring,.gdpoint,.gdglint,.gspot.gdspot,.gdfinger,.gdripple").forEach(function(x){x.classList.add("gd-out");x.removeAttribute("aria-live");setTimeout(function(){x.remove()},S.reduce?0:170)})}
/* would the item's bubble, at its best place, cover its target or the block being told? (measured with its words, never shown) */
function gdCovers(it){if(!gdTarget(it))return null;var m=document.createElement("div");m.className="gdbub";m.style.visibility="hidden";
  m.innerHTML=gdBubHtml({item:it,text:gdLine(it,S.gdVisitor.visits>1,gdSlots(it))});ph.appendChild(m);var best=gdSolve({item:it},m.offsetWidth,m.offsetHeight);m.remove();return best.cover||null}
/* a spoken tip: its line through the one order of voices (guide/voice.js's say), the word being said lit */
/* (the line voiced last, G.gd.voiced: what the screen-reader scene reads) */
function gdVoice(c){G.gd.voiced={text:c.text,at:c.at};say(c.text,function(k){ph.querySelectorAll(".gdbub:not(.gd-out) .gdw").forEach(function(w,i){w.classList.toggle("on",i===k)})},function(){ph.querySelectorAll(".gdbub .gdw.on").forEach(function(w){w.classList.remove("on")})})}
on("fabLayer.after",function(){if(G.gd.az)gdApplyAz(ph.querySelector(".azfab"),false);if(G.gd.ptGb&&G.gd.cur){var az=ph.querySelector(".azfab");if(az)az.classList.add("point-gb")}});
on("camPaint.after",function(){if(G.gd.cur)gdPlace(G.gd.cur,false)});
/* and 4 times a second: what it keeps clear of may move (the ✦'s label folds, a remark comes), never during Azu's hop */
setInterval(function(){var c=G.gd.cur,az=ph.querySelector(".azfab");if(!c||noDraw||az&&az.getAnimations&&az.getAnimations().length)return;gdPlace(c,false)},250);
/* the words may change in the Editor while the tip shows */
on("renderPhone.after",function(){var c=G.gd.cur;if(!c)return;
  /* (and the language: the point's name in the visitor's language, its line kept) */
  if(c.slots&&c.slots.id){var pp=(S.wallPois||[]).filter(function(x){return x.id===c.slots.id})[0];if(pp)c.slots.point=L(pp.name)}var t=gdLine(c.item,c.ret,c.slots),b=ph.querySelector(".gdbub:not(.gd-out) button[data-gdbub]");if(b&&t!==c.text){c.text=t;if(c.spoken)b.innerHTML=gdWords(t);else b.textContent=t}
  var bo=gdBtnOf(c.item),ct=bo&&ph.querySelector(".gdbub:not(.gd-out) [data-gdcta]");if(ct)ct.innerHTML=TXH(bo.word)+'<span aria-hidden="true">&#8250;</span>';gdPlace(c,false)});
/* ----- the bubble's controls: a tap does its action; a swipe away dismisses; a long press asks Fewer like this? ----- */
function gdTap(){var c=G.gd.cur;if(!c)return;var it=c.item;if(c.pull&&G.gd.demo){gdStop("tapped (a demo)");return}
  if(it.tap==="view"){gdStop("tapped: Ask about the view opens","acted");G.fab.menu=false;G.cam.open=true;renderPhone(true);emit("view.opened","the tip");return}
  /* Azu's remark: a point with a card opens it and starts its tour; any other opens Ask about the view about it, the point lit */
  if(it.tap==="remark"){var pid=c.slots.id,p=(S.wallPois||[]).filter(function(x){return x.id===pid})[0];gdStop("tapped: "+(p&&p.card?"the tour of "+(c.slots.point||"the point")+" starts":"Ask about the view opens about "+(c.slots.point||"the point")),"acted");
    if(p&&p.card){S.stop="half";renderPhone(false);emit("card.opened","Azu's remark");startTour();return}
    G.fab.menu=false;G.cam.open=true;renderPhone(true);emit("view.opened","Azu's remark");camAsk("",p&&p.year?"oldest":"what");camLight([pid]);return}
  if(it.tap==="menu"){gdStop("tapped: the ✦ menu opens","acted");gbMenu(true);return}
  if(it.tap==="hunt"){gdStop("tapped: the treasure hunt starts","acted");gdHuntGo("the hunt invitation");return}
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
/* an invitation's chip (How does this work? on the wall: Open a card): the card of the marker Azu points at opens, at its stop */
function gdCta(act){var c=G.gd.cur;if(!c)return;var it=c.item,bt=gdBtnOf(it),w=bt?TXT(bt.word):"",M=gdTarget(it);act=act||bt&&bt.act||"card";
  if(act==="tour"){gdStop("tapped its button: "+w,"acted");S.stop="half";renderPhone(false);emit("card.opened","the tip's button");startTour();return}
  if(act==="view"){gdStop("tapped its button: "+w,"acted");G.fab.menu=false;G.cam.open=true;renderPhone(true);emit("view.opened","the tip's button");return}
  if(act==="hunt"){gdStop("tapped its button: "+w,"acted");gdHuntGo("the tip's button");return}
  /* Show me: the gesture on its target (the ghost finger, the ring), or How does this work? when it has no target */
  if(act==="show"){gdStop("tapped its button: "+w,"acted");if(M)setTimeout(function(){gdShowMe(it,M)},S.reduce?0:180);else gdWalk("Show me");return}
  gdStop("tapped: "+w,"acted");S.view="card";S.stop=S.cardCfg.openAt;feedback("open");renderPhone(false);emit("card.opened",it.cta?"How does this work?":"the tip's button")}
ph.addEventListener("click",function(e){var f=e.target.closest("[data-gdfewer]");if(f){e.stopPropagation();if(f.dataset.gdfewer==="yes")gdFewer();else gdKeep();return}
  var ct=e.target.closest("[data-gdcta]");if(ct){e.stopPropagation();if(performance.now()<(G.gd.noClickUntil||0))return;gdCta(ct.dataset.gdcta);return}
  if(e.target.closest("[data-gdx]")){e.stopPropagation();gdStop("dismissed (its close button)","dismissed");return}
  var t=e.target.closest("[data-gdbub]");if(!t)return;e.stopPropagation();if(performance.now()<(G.gd.noClickUntil||0))return;gdTap()},true);
/* round 7: the treasure hunt from its invitation: Ask about the view opens and its own hunt starts (the Kids' treasure hunt chip's call) */
function gdHuntGo(how){G.fab.menu=false;G.cam.open=true;renderPhone(true);emit("view.opened",how);camAsk(TXT("cam_chip_kids"),"kids")}
/* the Editor's demos: a chip on the phone with the ×10 clock (Editor only, never in the app; Unity: a Play-mode preview label) */
function gdChip(text){var c=ph.querySelector(".gdchip");if(!text){if(c)c.remove();return}if(!c){c=document.createElement("div");c.className="gdchip";c.setAttribute("aria-hidden","true");ph.appendChild(c)}c.textContent=text}

/* ----- How to use the card, the walkthrough's look: its On steps, in order, run through the tour's steps (each scrolls to its part and
   marks its control; its line is the step's words); the card's own guide steps come back when it ends. It opens from Settings (its
   entry at the panel's foot, In Settings) and from the ✦ menu's How does this work? (In the ✦ menu): guidance/service.js's gdWalk.
   Unity: the same GuideTour runner with the walkthrough's steps (GuidanceCatalog), each step's control marked by the guide's focus ----- */
function gdWalkCard(fromId){if(G.on)stopTour();S.settingsOpen=false;G.fab.menu=false;var g=gdGroupById("howto"),steps=g.items.map(function(id){return S.guidance.items[id]}).filter(function(x){return x&&x.on});
  if(!steps.length)return false;if(!S.howSaved)S.howSaved=S.guideSteps;
  S.guideSteps=steps.map(function(it,i){return{id:"how"+i,gd:it.id,view:"card",ref:it.ref,sel:it.sel,act:"show",text:S.tx[it.says],cue:it.focus&&it.focus!=="tour"?it.focus:undefined}});S.view="card";S.stop="full";renderPhone(false);
  var j=fromId?steps.map(function(x){return x.id}).indexOf(fromId):-1;startTour();if(j>0){stopSpeech();G.i=j;runStep()}return true}
on("stopTour.after",function(){if(S.howSaved){S.guideSteps=S.howSaved;S.howSaved=null}});
on("runStep.after",function(){var st=S.guideSteps[G.i];if(G.on&&st&&st.sel){var el=ph.querySelector(st.sel);if(el){el.classList.add("gfocus");moveMascot(el)}}});
on("clearGuideMarks.after",function(){ph.querySelectorAll(".gfocus").forEach(function(x){x.classList.remove("gfocus")})});
on("settingsPanel.html",function(h){var g=gdGroupById("howto");return g&&g.on!==false&&g.inSettings?h.replace(/<\/div>$/,'<button type="button" class="vhow" data-howto="1">'+TXH("settings_how")+' &#8250;</button></div>'):h});
ph.addEventListener("click",function(e){if(e.target.closest("[data-howto]")){e.stopPropagation();gdWalk("Settings")}},true);

/* ----- round 6, the look (_3.8 section 4). Focus per item: None / Glint / Ring / Spotlight.
   - Glint: the element shines once, a light band sweeping across it (0.9 s); under Reduce Motion a still highlight. Unity: a sweep texture
     (a ramp PNG) on a VisualElement clipped to the target (overflow hidden, its radius), its translate tweened once; the Lab draws the band
     with a gradient as that texture's stand-in.
   - Spotlight (Essentials and walkthrough steps only): the rest dims at the item's Dim (0.2-0.6), the target cut out, its name inside the
     hole: the guide's own cut-out (guide/focus.js's spotPaint), never a copy. Unity: Painter2D FillRule.OddEven.
   - Ring: today's (one soft pulse, then still).
   The ghost finger (Shows the gesture): a translucent finger beside the target does its gesture once (a tap on a marker, a point or the ✦;
   a drag across a Before / After; a swipe on a strip), after Azu's hop; under Reduce Motion it is shown still at its end. Unity: a sprite
   (an Image in a VisualElement) with a translate tween (and a scale for the press), the tap's ripple a ring VisualElement scaled and faded.
   The Button chip in the bubble (Show me / Start the tour / Open the card / Ask about the view): a second Button, as How does this work?'s
   chip. Accessibility: the bubble is role=status, aria-live polite (announced, never spoken over a screen reader); Tips stay until I tap
   them keeps it until the visitor taps or closes it (WCAG 2.2.1); every tip also lands in the dock's conversation under Tips. ----- */
function gdGlint(it,M,first){var g=ph.querySelector(".gdglint"),still=!!S.reduce;
  if(!g){g=document.createElement("div");g.className="gdglint"+(still?" still":"")+(M.rect?" gd-rect":"");g.setAttribute("aria-hidden","true");g.innerHTML="<i></i>";ph.appendChild(g);
    if(!still&&g.animate){var i=g.querySelector("i"),dl=Math.max(250,(G.gd.hopMs||0)-100);i.animate([{translate:"0 0"},{translate:"300% 0"}],{duration:1100,delay:dl,easing:"cubic-bezier(.45,0,.3,1)",fill:"both"});
      g.animate([{boxShadow:"0 0 0 0 rgba(255,217,138,0)"},{boxShadow:"0 0 0 3px rgba(255,236,190,.95),0 0 22px 8px rgba(255,217,138,.55)",offset:.45},{boxShadow:"0 0 0 0 rgba(255,217,138,0)"}],{duration:1300,delay:dl,easing:"ease-in-out",fill:"both"})}}
  var B=M.rect?{l:M.rect.l-4,t:M.rect.t-4,r:M.rect.r+4,b:M.rect.b+4}:{l:M.x-26,t:M.y-26,r:M.x+26,b:M.y+26};
  g.style.left=Math.round(B.l)+"px";g.style.top=Math.round(B.t)+"px";g.style.width=Math.round(B.r-B.l)+"px";g.style.height=Math.round(B.b-B.t)+"px"}
/* the spotlight's hole: the target (a marker or a point: its circle; the ✦: its pill) and its name beside it, so the words stay readable in it */
function gdSpotBox(M){var B=M.rect?{l:M.rect.l-6,t:M.rect.t-6,r:M.rect.r+6,b:M.rect.b+6}:{l:M.x-30,t:M.y-30,r:M.x+30,b:M.y+30},grow=false;
  if(!M.rect)gdObstacles().forEach(function(o){if(!o.text)return;var cx=(o.b.l+o.b.r)/2,cy=(o.b.t+o.b.b)/2;if(Math.hypot(cx-M.x,cy-M.y)<64){grow=true;B={l:Math.min(B.l,o.b.l-8),t:Math.min(B.t,o.b.t-6),r:Math.max(B.r,o.b.r+8),b:Math.max(B.b,o.b.b+6)}}});
  B.round=!M.rect&&!grow;return B}
function gdSpotTip(it,M){var sp=ph.querySelector(".gspot.gdspot");if(!sp){sp=document.createElement("div");sp.className="gspot gdspot";sp.setAttribute("aria-hidden","true");ph.appendChild(sp)}
  var B=gdSpotBox(M);spotPaint(sp,[Math.round(B.l),Math.round(B.t),Math.round(B.r-B.l),Math.round(B.b-B.t)],"spotlight",Math.max(.2,Math.min(.6,+it.dim||.4)),B.round)}
/* the ghost finger: its tip at (14, 4) of a 40 x 52 sprite */
var GD_FINGER='<svg viewBox="0 0 40 52" aria-hidden="true"><path d="M14 4c2.8 0 4.6 2 4.6 4.8v14.6l1.6-.2c1.4-2.2 4.8-2 5.8.6 1.6-1.6 4.8-.9 5.4 1.6 1.8-1 4.6.2 4.6 3V38c0 7.6-5.8 12.6-13 12.6-5.4 0-8.6-2.6-11.4-7.2L5 33.6c-1.4-2.4.6-5 3.2-4.2l2 1.6V8.8C10.2 6 11.6 4 14 4z"/></svg>',GD_TIP_X=14,GD_TIP_Y=4;
function gdFingerStop(){ph.querySelectorAll(".gdfinger,.gdripple").forEach(function(x){x.remove()})}
/* where the finger rests beside a point target: its tip at the ring's edge, pointing in, from the side that covers no words, no part of the
   target, the bubble or Azu (12 directions tried; the sprite turned about its tip). Returns the tip, the direction out and the angle */
function gdFingerBox(tx,ty,deg){var r=deg*Math.PI/180,c=Math.cos(r),s=Math.sin(r),xs=[],ys=[];[[-14,-4],[26,-4],[26,48],[-14,48]].forEach(function(q){xs.push(tx+q[0]*c-q[1]*s);ys.push(ty+q[0]*s+q[1]*c)});
  return{l:Math.min.apply(null,xs),t:Math.min.apply(null,ys),r:Math.max.apply(null,xs),b:Math.max.apply(null,ys)}}
function gdFingerRest(M){var bub=ph.querySelector(".gdbub:not(.gd-out)"),az=G.gd.az&&ph.querySelector(".azfab"),W=ph.offsetWidth,H=ph.offsetHeight,avoid=gdObstacles().map(function(o){return{b:o.b,w:o.text?50:4}});
  if(bub)avoid.push({b:gdPR(bub),w:30});if(az){var A=gdPR(az),a=G.gd.az,cur=az.style.translate.split(" ").map(parseFloat);avoid.push({b:{l:A.l-(cur[0]||0)+a.dx,t:A.t-(cur[1]||0)+a.dy,r:A.r-(cur[0]||0)+a.dx,b:A.b-(cur[1]||0)+a.dy},w:30})}
  var core=M.rect?{l:M.rect.l,t:M.rect.t,r:M.rect.r,b:M.rect.b}:{l:M.x-22,t:M.y-22,r:M.x+22,b:M.y+22};avoid.push({b:core,w:40});
  var best=null;for(var k=0;k<12;k++){var an=(k*30+120)*Math.PI/180,ux=Math.cos(an),uy=Math.sin(an),R=M.rect?Math.max(M.rect.r-M.rect.l,M.rect.b-M.rect.t)/2+4:GD_RING-4,tx=M.x+ux*R,ty=M.y+uy*R,deg=Math.atan2(-uy,-ux)*180/Math.PI+90,box=gdFingerBox(tx,ty,deg);
    var c=avoid.reduce(function(s2,o){return s2+gdOver(box,o.b)*o.w},0)+Math.max(0,8-box.l)*99+Math.max(0,box.r-(W-8))*99+Math.max(0,50-box.t)*99+Math.max(0,box.b-(H-8))*99+Math.abs(deg)*.5;
    if(!best||c<best.c)best={c:c,x:tx,y:ty,d:[ux,uy],deg:deg}}return best}
/* the gesture once (M: a point target; el: a cue's element, a Before / After or a strip); hopMs: after Azu's hop */
function gdFinger(it,M,el,force){var g=force||gdGestureOf(it);if(!g)return null;gdFingerStop();var f=document.createElement("div");f.className="gdfinger";f.innerHTML=GD_FINGER;f.dataset.gesture=g;ph.appendChild(f);
  var still=!!S.reduce,delay=still?0:(G.gd.hopMs||0)+150,place=function(x,y){f.style.left=Math.round(x-GD_TIP_X)+"px";f.style.top=Math.round(y-GD_TIP_Y)+"px"},at=function(x,y){return{translate:Math.round(x-GD_TIP_X)+"px "+Math.round(y-GD_TIP_Y)+"px"}};
  f.style.left="0px";f.style.top="0px";
  if(g==="tap"){var r=gdFingerRest(M),dx=r.d[0]*16,dy=r.d[1]*16;f.style.translate=at(r.x,r.y).translate;f.style.rotate=r.deg.toFixed(1)+"deg";f.dataset.endx=Math.round(r.x);f.dataset.endy=Math.round(r.y);
    if(still){f.classList.add("still");return f}
    f.animate([Object.assign({opacity:0,scale:"1"},at(r.x+dx,r.y+dy)),Object.assign({opacity:.95,scale:"1",offset:.35},at(r.x+dx*.3,r.y+dy*.3)),Object.assign({opacity:.95,scale:".9",offset:.5},at(r.x,r.y)),Object.assign({opacity:.95,scale:"1",offset:.7},at(r.x+dx*.4,r.y+dy*.4)),Object.assign({opacity:0,scale:"1"},at(r.x+dx*.6,r.y+dy*.6))],{duration:1400,delay:delay,fill:"both",easing:"ease-out"});
    var rp=document.createElement("i");rp.className="gdripple";rp.setAttribute("aria-hidden","true");rp.style.left=Math.round(M.x)+"px";rp.style.top=Math.round(M.y)+"px";ph.appendChild(rp);
    rp.animate([{opacity:0,scale:".6"},{opacity:.8,scale:".8",offset:.01},{opacity:0,scale:"1.35"}],{duration:650,delay:delay+700,fill:"both",easing:"ease-out"});return f}
  var B=el?gdPR(el):M?M.box:null;if(!B)return null;var w=B.r-B.l,h=B.b-B.t,y=B.t+h*(g==="drag"?.74:.58),xs=g==="drag"?[.5,.8,.2,.5]:[.84,.3];
  var end=xs[xs.length-1];f.style.translate=at(B.l+w*end,y).translate;f.dataset.endx=Math.round(B.l+w*end);f.dataset.endy=Math.round(y);
  if(still){f.classList.add("still");return f}
  var fr=[Object.assign({opacity:0},at(B.l+w*xs[0],y+10))];xs.forEach(function(x,i){fr.push(Object.assign({opacity:.95,offset:+(.12+.76*i/(xs.length-1)).toFixed(3)},at(B.l+w*x,y)))});fr.push(Object.assign({opacity:0},at(B.l+w*end,y+8)));
  f.animate(fr,{duration:g==="drag"?1700:1000,delay:el?0:delay,fill:"both",easing:"ease-in-out"});return f}
/* Show me (the Button chip): the gesture on the target, and its ring, as a demonstration (no memory) */
function gdShowMe(it,M){gdFinger(it,M,null,GD_GESTURE[it.about]||"tap");if(!M.rect){var r=gdRing({about:it.about},!S.reduce);if(r){r.style.left=Math.round(M.x)+"px";r.style.top=Math.round(M.y)+"px"}}
  setTimeout(function(){if(!G.gd.cur){gdFingerStop();ph.querySelectorAll(".gdring").forEach(function(x){x.remove()})}},S.reduce?2600:2000)}
/* a line's height in the bubble (the Editor's warnings): its lines at the bubble's width on this phone, in a hidden bubble */
G.gd.lineCache={};
function gdLineCount(text,form,btn){var key=ph.offsetWidth+"|"+form+"|"+(btn?1:0)+"|"+text;if(G.gd.lineCache[key]!=null)return G.gd.lineCache[key];
  var m=document.createElement("div");m.className="gdbub"+(form==="remark"?" gd-more":"")+(btn?" gd-cta":"");m.style.visibility="hidden";m.innerHTML='<button type="button" data-gdbub="1">'+esc(text)+'</button>';ph.appendChild(m);
  var bt=m.querySelector("button"),cs=getComputedStyle(bt),n=Math.round((bt.offsetHeight-parseFloat(cs.paddingTop)-parseFloat(cs.paddingBottom))/parseFloat(cs.lineHeight));m.remove();return G.gd.lineCache[key]=n}
/* ----- Settings › The guide: Tips (All / Essentials only / Off: "You can always tap ✦ for help"), Tips stay until I tap them (Off by
   default), Show the tips again (the memory reset, an Undo toast, no popup). Unity: the Settings panel's toggles (PlayerPrefs-free: the
   guidance memory JSON), the toast a VisualElement with a scheduled hide ----- */
on("settingsPanel.html",function(h){var d=document.createElement("div");d.innerHTML=h;var g=d.querySelector(".vguide");if(!g)return h;var P=gdPrefs(),
    o=function(k,v,lab,cur){return'<button type="button" data-gdpref="'+k+'|'+v+'" aria-pressed="'+(String(cur)===v)+'">'+esc(lab)+'</button>'};
  g.insertAdjacentHTML("beforeend",'<div class="vtips"><small>'+TXH("settings_tips")+'</small><div class="vo">'+o("tips","all",TXT("settings_tips_all"),P.tips)+o("tips","essentials",TXT("settings_tips_ess"),P.tips)+o("tips","off",TXT("settings_tips_off"),P.tips)+'</div>'+
    (P.tips==="off"?'<p class="vnote">'+TXH("settings_tips_off_note")+'</p>':"")+'<small>'+TXH("settings_tips_stay")+'</small><div class="vo">'+o("stay","true",TXT("settings_on"),String(!!P.stay))+o("stay","false",TXT("settings_off"),String(!!P.stay))+'</div>'+
    '<button type="button" class="vagain" data-gdagain="1">'+TXH("settings_tips_again")+'</button></div>');return d.innerHTML});
function gdToast(text,undo){ph.querySelectorAll(".gdtoast").forEach(function(x){x.remove()});clearTimeout(G.gd.toastT);var d=document.createElement("div");d.className="gtoast gdtoast";d.setAttribute("role","status");
  d.innerHTML='<span>'+esc(text)+'</span>'+(undo?'<button type="button" data-gdundo="1">'+TXH("gd_undo")+'</button>':"");ph.appendChild(d);G.gd.toastT=setTimeout(function(){d.remove()},6000);return d}
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gdpref],[data-gdagain],[data-gdundo]");if(!t)return;e.stopPropagation();
  if(t.dataset.gdpref){var q=t.dataset.gdpref.split("|"),P=gdPrefs();P[q[0]]=q[0]==="stay"?q[1]==="true":q[1];gdLog("event","the visitor set "+(q[0]==="stay"?"Tips stay until I tap them "+(P.stay?"On":"Off"):"Tips: "+q[1]));if(q[0]==="tips"&&q[1]!=="all"&&G.gd.cur&&!G.gd.cur.pull&&!(q[1]==="essentials"&&G.gd.cur.item.essential))gdStop("the visitor changed Tips");renderPhone(true);return}
  if(t.dataset.gdagain){gdTipsAgain();renderPhone(true);gdToast(TXT("gd_tips_again_done"),true);return}
  if(t.dataset.gdundo){gdTipsUndo();t.closest(".gdtoast").remove();renderPhone(true)}},true);
/* ----- the dock's conversation: the tips shown this visit, under Tips (guide/dock.js's dockThread.html) ----- */
on("dockThread.html",function(h){var l=(S.gdVisitor.visit.tips||[]).slice(-8);if(!l.length)return h;
  return'<div class="gm-tips" role="group" aria-label="'+TXH("dock_tips")+'"><small>'+TXH("dock_tips")+'</small>'+l.map(function(x){return'<div class="gm-tip" data-gdtip="'+esc(x.id)+'">'+esc(x.text)+'</div>'}).join("")+'</div>'+h});
/* ----- round 8 (_3.8 section 6), the first pages' look: one screen at a time over the wall, the app's paper (opaque, no blur), one focal
   point: Hello's Azu (it forms from a tile and waves: the tile turns in, dissolves, Azu rises from it and waves; Reduce Motion: Azu is
   there, no tile, no wave; Mascot Off: no Azu), the camera's mark, the refused camera's mark. At the top the pages' progress (a short bar
   a page) and, on Hello, the language as a chip (EN / PT: the app's language). At the foot, in the thumb's reach: the page's buttons
   (the first filled, the second outlined, 54 px) and Skip (the rest of the intro, 44 px). Who's exploring is the audiences' own screen in
   the layer (onbShow, no copy). The camera's request is the phone's own (a stand-in: a dim and the system's alert, Allow / Don't allow);
   refused: Open Settings shows the phone's Settings (a stand-in: the Camera switch, Back to the app) and Try again asks again; the
   refused screen has no Skip (the app never goes on to the wall without the camera). A page comes in with a short rise and fade (0.32 s).
   Unity: each page a UI Toolkit screen (UXML) in the onboarding's UIDocument above the AR view (sort order), the progress and the chip
   VisualElements, the buttons Buttons (USS: 54 px, the accent, a 1.5 px border); the entrance a USS transition on translate and opacity;
   Azu's forming and wave the Rive file's states ("form", "wave"; the 3D model: its Animator's wave clip), Reduce Motion its still state;
   the request the platform's own dialog (no Unity UI); Open Settings a native call (iOS UIApplication.openSettingsURLString, Android the
   app's settings intent) ----- */
var GD_CAM_SVG='<svg viewBox="0 0 48 48" aria-hidden="true"><path d="M17.5 10.5h13l3 5h6a3 3 0 0 1 3 3v17.5a3 3 0 0 1-3 3h-31a3 3 0 0 1-3-3V18.5a3 3 0 0 1 3-3h6z"/><circle cx="24" cy="27" r="7.5"/></svg>';
var GD_CAMOFF_SVG='<svg viewBox="0 0 48 48" aria-hidden="true"><path d="M17.5 10.5h13l3 5h6a3 3 0 0 1 3 3v17.5a3 3 0 0 1-3 3h-31a3 3 0 0 1-3-3V18.5a3 3 0 0 1 3-3h6z"/><circle cx="24" cy="27" r="7.5"/><path class="cut" d="M7 7l34 34"/></svg>';
/* the phone's own words (its request, its Settings): not the app's, so not in Languages & Terms; the Lab's stand-in says them */
var GD_SYS={title:T("Allow the camera?","Permitir a câmara?"),allow:T("Allow","Permitir"),deny:T("Don't Allow","Não permitir"),standin:T("The phone's own request · a stand-in in the Lab","O pedido do próprio telemóvel · uma simulação no Lab"),
  back:T("Back to the app","Voltar à app"),settings:T("Settings","Definições"),access:T("Allow access to","Permitir acesso a"),camera:T("Camera","Câmara"),setin:T("The phone's Settings · a stand-in in the Lab","As Definições do telemóvel · uma simulação no Lab")};
function gdAzuOn(){return S.guideCfg.mascot!==false&&gbAzu()}
function gdPagesLayer(){ph.querySelectorAll(".gdpages.gd-out").forEach(function(x){x.remove()});var L=ph.querySelector(".gdpages");
  if(!L){L=document.createElement("div");L.className="gdpages";L.setAttribute("role","dialog");L.setAttribute("aria-modal","true");ph.appendChild(L)}return L}
function gdPagesUnpaint(){var L=ph.querySelector(".gdpages:not(.gd-out)");if(!L)return;L.classList.add("gd-out");L.removeAttribute("aria-modal");setTimeout(function(){L.remove()},S.reduce?0:220)}
/* Azu on a page: the tile it forms from, and Azu (the guide's own still: the 2D tile or the 3D model's picture) */
function gdPgAzu(it){if(!gdAzuOn()||it.azu==="none")return"";return'<div class="gdpg-art" data-azu="'+(S.reduce?"still":it.azu)+'"><span class="gdpg-tile">'+TILE+'</span><span class="gdpg-azu">'+mascotStill()+'</span></div>'}
function gdPgSys(it){return'<div class="gdpg-sys"><div class="gdpg-scrim"></div><div class="gdpg-alert" role="alertdialog" aria-label="'+esc(L(GD_SYS.title))+'"><b>'+esc(L(GD_SYS.title))+'</b><p>'+TXH(it.usage||"gd_pg_cam_usage")+'</p>'+
  '<div class="gdpg-abtn"><button type="button" data-gdpg="sys:deny">'+esc(L(GD_SYS.deny))+'</button><button type="button" data-gdpg="sys:allow">'+esc(L(GD_SYS.allow))+'</button></div></div><span class="gdpg-standin">'+esc(L(GD_SYS.standin))+'</span></div>'}
function gdPgSet(){var on=G.gd.camPerm==="allowed";return'<div class="gdpg-set" role="dialog" aria-label="'+esc(L(GD_SYS.settings))+'"><button type="button" class="gdpg-setback" data-gdpg="back"><span aria-hidden="true">&#8249;</span> '+esc(L(GD_SYS.back))+'</button>'+
  '<h3>'+esc(L(GD_SYS.settings))+'</h3><small>'+esc(L(GD_SYS.access))+'</small><div class="gdpg-setrow"><span>'+esc(L(GD_SYS.camera))+'</span><button type="button" class="gdpg-sw" role="switch" aria-checked="'+on+'" aria-label="'+esc(L(GD_SYS.camera))+'" data-gdpg="switch"><i></i></button></div>'+
  '<span class="gdpg-standin">'+esc(L(GD_SYS.setin))+'</span></div>'}
/* the page now, drawn (anim: it comes in, and Hello's Azu forms; a redraw for the language or the words keeps it still) */
function gdPagePaint(anim){var P=G.gd.pages,n=gdPageNow();if(!P||!n||noDraw)return;var it=n.it||S.guidance.items.pg_camera,kind=n.direct?"camera":it.page,Lr=gdPagesLayer();
  Lr.dataset.page=kind;Lr.dataset.cam=P.cam||"";Lr.classList.toggle("gd-still",!!S.reduce);Lr.setAttribute("aria-label",it.label);
  var dots='<div class="gdpg-dots" role="img" aria-label="'+esc(fmt(TXT("gd_pg_dots"),P.i+1,P.run.length))+'">'+(P.run.length>1?P.run.map(function(x,i){return'<i'+(i===P.i?' class="on"':i<P.i?' class="done"':"")+'></i>'}).join(""):"")+'</div>';
  var lang=it.langChip&&kind!=="aud"?'<div class="gdpg-lang" role="group" aria-label="'+TXH("gd_pg_lang")+'">'+langCodes().map(function(l){return'<button type="button" data-gdpg="lang:'+l+'" aria-pressed="'+(S.lang===l)+'">'+l.toUpperCase()+'</button>'}).join("")+'</div>':"";
  var top='<div class="gdpg-top">'+dots+lang+'</div>';
  /* Who's exploring: the audiences' own screen, in the layer */
  if(kind==="aud"){var ha=top+"|aud|"+S.lang+"|"+S.vis.aud;if(!anim&&Lr._html===ha&&Lr.querySelector(".onb"))return;Lr._html=ha;Lr.innerHTML=top+'<div class="gdpg-stage"></div>';onbShow(Lr.querySelector(".gdpg-stage"));return}
  var off=kind==="camera"&&(P.cam==="refused"||P.cam==="settings"),t=off?it.offTitle:n.direct?"":it.title,s=off?it.offSays:n.direct?"":it.says;
  var art=kind==="camera"?'<div class="gdpg-cam'+(off?" off":"")+'">'+(off?GD_CAMOFF_SVG:GD_CAM_SVG)+'</div>':gdPgAzu(it);
  var bs=off?[{word:it.offSettings,act:"settings"},{word:it.offRetry,act:"retry"}]:n.direct?[]:(it.buttons||[]).filter(function(b){return b&&b.word&&gdHasWords(b.word)}).slice(0,2);
  var foot='<div class="gdpg-foot">'+bs.map(function(b,i){return'<button type="button" class="gdpg-b '+(i?"gdpg-sec":"gdpg-pri")+'" data-gdpg="'+b.act+'">'+TXH(b.word)+'</button>'}).join("")+
    (off||n.direct?"":'<button type="button" class="gdpg-skip" data-gdpg="skip">'+TXH("gd_pg_skip")+'</button>')+'</div>';
  var mid='<div class="gdpg-mid">'+art+(t&&gdHasWords(t)?'<h2 class="gdpg-t">'+TXH(t)+'</h2>':"")+(s&&gdHasWords(s)?'<p class="gdpg-l">'+TXH(s)+'</p>':"")+'</div>';
  var sys=kind==="camera"&&P.cam==="prompt"?gdPgSys(it):kind==="camera"&&P.cam==="settings"?gdPgSet():"";
  /* a redraw that changes nothing keeps the page as it is (an entrance or Azu's forming goes on) */
  var html=top+'<div class="gdpg-body">'+mid+foot+'</div>'+sys;if(!anim&&Lr._html===html)return;Lr._html=html;Lr.innerHTML=html;
  var body=Lr.querySelector(".gdpg-body"),al=Lr.querySelector(".gdpg-alert,.gdpg-set");
  if(anim&&!S.reduce&&body.animate){body.animate([{opacity:0,translate:"0 18px"},{opacity:1,translate:"0 0"}],{duration:320,easing:"cubic-bezier(.2,.8,.2,1)"})}
  if(al&&!S.reduce&&al.animate)al.animate([{opacity:0,scale:".94"},{opacity:1,scale:"1"}],{duration:200,easing:"ease-out"});
  /* the screen reader starts at the page (the dialog, its label), or at the phone's request when it shows */
  gdPgForm(Lr,anim&&!P.cam);var f=Lr.querySelector(".gdpg-alert,.gdpg-set")||Lr;if(f.tabIndex<0||!f.hasAttribute("tabindex"))f.setAttribute("tabindex","-1");f.focus({preventScroll:true})}
/* Hello: the tile turns in and dissolves, Azu rises from it, then waves (Reduce Motion, Appears, or a redraw: Azu is simply there) */
function gdPgForm(Lr,anim){var art=Lr.querySelector(".gdpg-art");if(!art)return;var t=art.querySelector(".gdpg-tile"),a=art.querySelector(".gdpg-azu");
  if(!anim||S.reduce||art.dataset.azu!=="form"||!a.animate){art.classList.add("gd-formed");return}
  t.animate([{opacity:0,scale:".45",rotate:"-90deg"},{opacity:1,scale:"1",rotate:"0deg",offset:.42},{opacity:1,scale:"1",rotate:"0deg",offset:.58},{opacity:0,scale:"1.18",rotate:"0deg"}],{duration:1150,easing:"cubic-bezier(.2,.8,.2,1)",fill:"forwards"});
  a.animate([{opacity:0,scale:".5",translate:"0 14px"},{opacity:0,scale:".5",translate:"0 14px",offset:.5},{opacity:1,scale:"1.06",translate:"0 -2px",offset:.82},{opacity:1,scale:"1",translate:"0 0"}],{duration:1150,easing:"ease-out",fill:"backwards"});
  a.animate([{rotate:"0deg"},{rotate:"-11deg",offset:.18},{rotate:"9deg",offset:.42},{rotate:"-7deg",offset:.66},{rotate:"4deg",offset:.84},{rotate:"0deg"}],{duration:1100,delay:1200,easing:"ease-in-out"});
  setTimeout(function(){art.classList.add("gd-formed")},1200)}
/* the pages' controls: real taps (the language chip, the buttons, Skip, the camera's request, Settings' switch and Back) */
ph.addEventListener("click",function(e){var b=e.target.closest("[data-gdpg]");if(!b||!G.gd.pages)return;e.stopPropagation();var a=b.dataset.gdpg;
  if(a.indexOf("lang:")===0){var l=a.slice(5);if(S.lang!==l){S.lang=l;gdLog("event","the language: "+l.toUpperCase()+" (the app's, on Hello's chip)");renderAll(true)}return}
  if(a==="next")gdPagesNext();else if(a==="how")gdPagesNext(true);else if(a==="skip")gdPagesSkip();else if(a==="camera")gdCamAsk();
  else if(a==="sys:allow")gdCamAnswer(true);else if(a==="sys:deny")gdCamAnswer(false);else if(a==="settings")gdCamSettings();else if(a==="retry")gdCamRetry();
  else if(a==="switch")gdCamSwitch();else if(a==="back")gdCamBack()},true);
/* the wall under the pages never takes a touch, a drag or the wheel meant for a page */
["pointerdown","pointermove","wheel"].forEach(function(t){window.addEventListener(t,function(e){if(G.gd.pages&&e.target&&e.target.closest&&e.target.closest(".gdpages"))e.stopPropagation()},{capture:true,passive:true})});
/* a redraw (the language, dark, the words in the Editor) draws the page again, still */
on("renderPhone.after",function(){if(G.gd.pages&&!noDraw)gdPagePaint(false)});
