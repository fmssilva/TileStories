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
function gdMascot(){return gbAzu()?ph.querySelector(".azfab"):null}
/* round 15b, the developer (2026-11-11, 2026-11-12): any guide bubble with no Azu beside it is signed by a small ✦ in its top-left corner
   (Mascot Off; Guide button = ✦ only with the mascot on), so a bubble with no voice and no Azu still says who speaks; not a tap target, it
   covers no word (the words start after it). With Azu shown (beside the ✦, or the guide's Azu in a tour), nothing changes. Unity: a child
   VisualElement of the bubble (position absolute, the ✦ sprite, picking-mode Ignore), the label's padding-left past it */
function gdSign(){return gdMascot()||G.on&&ph.querySelector(".mascot")?"":'<i class="gdsign" aria-hidden="true">'+GBSTAR+'</i>'}
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
  /* round 9: the guide's Pause in a tour (the dock's ⏸ at Mini or Peek, the ✦'s at Compact) */
  if(it.about==="pause"){var pz=gdPauseEl();if(!pz)return null;var Bp=gdPR(pz);return{x:(Bp.l+Bp.r)/2,y:(Bp.t+Bp.b)/2,name:"the guide's Pause",rect:Bp,box:{l:Bp.l-6,t:Bp.t-6,r:Bp.r+6,b:Bp.b+6},beside:false}}
  return null}
function gdPauseEl(){var l=['.gdock .gd-mini [data-g="pause"]','.gdock .gd-ctl [data-g="pause"]','.gb-pause[data-g="pause"]'];for(var i=0;i<l.length;i++){var e=ph.querySelector(l[i]);if(gdSeen(e)&&e.getBoundingClientRect().height>0)return e}return null}
/* the bubble's words: plain, or one span a word when it is spoken (the word being said is lit) */
function gdWords(t){return String(t).split(/(\s+)/).map(function(w){return/^\s*$/.test(w)?w:'<span class="gdw">'+esc(w)+'</span>'}).join("")}
/* the bubble's chip: How does this work?'s Open a card (cta), or the item's Button (round 6: Show me / Start the tour / Open the card / Ask
   about the view); with Tips stay until I tap them (or On screen for: Until tapped) a dismiss button, so the visitor can close it without acting */
function gdBtnOf(it){return!it?null:it.cta?{word:it.cta,act:"card"}:it.button&&it.button!=="none"&&GD_BTN_WORD[it.button]?{word:GD_BTN_WORD[it.button],act:it.button}:null}
function gdStays(c){return!!c&&!c.pull&&!c.cue&&(c.item.onScreen==="tap"||!!gdPrefs().stay)}
function gdBubHtml(c){var bt=gdBtnOf(c.item),cta=bt?'<button type="button" class="gdcta" data-gdcta="'+bt.act+'">'+TXH(bt.word)+'<span aria-hidden="true">&#8250;</span></button>':"";
  var x=gdStays(c)?'<button type="button" class="gdx" data-gdx="1" aria-label="'+TXH("gd_dismiss")+'"><span aria-hidden="true">&#10005;</span></button>':"";
  return gdSign()+'<button type="button" data-gdbub="1">'+(c.spoken?gdWords(c.text):esc(c.text))+'</button>'+cta+x+'<i class="gdtail"></i>'}
/* the tip on the phone: its elements made once, then placed (again at each paint of the wall: the marker moves with the picture) */
function gdPresent(c){gdClearEls();var it=c.item,az=gdMascot();
  /* a Moment: Azu's prop and bubble (as before); when it ends, the tour's next step it held goes on */
  if(it.form==="moment"){var pull=c.pull;G.gd.mom=true;azMoment(it.prop==="cookie"?"cookie":"water",function(){gdResume(pull)},c.text);return}
  /* round 12: a beat of the tour: its card in the panel (gdBeatPresent, below) */
  if(it.form==="beat"){gdBeatPresent(c);return}
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
function gdUnpresent(){if(G.gd.bt)gdBeatClose();if(G.gd.mom){G.gd.mom=false;if(G.moment)azMomentEnd(false)}gdAzHome();ph.querySelectorAll(".gdbub,.gdring,.gdpoint,.gdglint,.gspot.gdspot,.gdfinger,.gdripple").forEach(function(x){x.classList.add("gd-out");x.removeAttribute("aria-live");setTimeout(function(){x.remove()},S.reduce?0:170)})}
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
function gdFewer(){var c=G.gd.cur;if(!c)return;var g=gdGroupOf(c.item);if(c.pull){gdStop("Fewer like this (a demo)");return}gdStop("Fewer like this: "+(g?g.name:"its group")+" rests for the visit","fewer");gdFewerToast(c.item)}
/* round 9: the toast after Fewer like this; an App group's adds the way back to them (Things to try), never a Content or Fun group's */
function gdFewerToast(it){var app=it.kind==="app"&&gdTryDefs().length>0;return gdToast(TXT("gd_fewer_done"),false,app?TXT("gd_fewer_try"):"")}
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
  if(bub)avoid.push({b:gdPR(bub),w:30});/* round 9: words a target brings of its own (the dock's line beside the guide's Pause) */if(M.avoid)M.avoid.forEach(function(b){avoid.push({b:b,w:50})});if(az){var A=gdPR(az),a=G.gd.az,cur=az.style.translate.split(" ").map(parseFloat);avoid.push({b:{l:A.l-(cur[0]||0)+a.dx,t:A.t-(cur[1]||0)+a.dy,r:A.r-(cur[0]||0)+a.dx,b:A.b-(cur[1]||0)+a.dy},w:30})}
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
function gdToast(text,undo,more){ph.querySelectorAll(".gdtoast").forEach(function(x){x.remove()});clearTimeout(G.gd.toastT);var d=document.createElement("div");d.className="gtoast gdtoast"+(more?" gdtoast-2":"");d.setAttribute("role","status");
  d.innerHTML='<span>'+esc(text)+(more?'<small class="gdtoast-try">'+esc(more)+'</small>':"")+'</span>'+(undo?'<button type="button" data-gdundo="1">'+TXH("gd_undo")+'</button>':"");ph.appendChild(d);G.gd.toastT=setTimeout(function(){d.remove()},6000);return d}
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
function gdAzuOn(){return gbAzu()}
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
/* ----- round 9 (_3.8 section 3.4), the pull's look. Things to try: a short sheet in the card's own look (the paper, the hairlines and the
   radius of Settings and the Contents sheet, no new style), one focal point (the count), then one row a feature in the Editor's order: a
   tick when done, the feature in a few words, and Show me (44 px). The line at the top: "3 of 8 tried" and a thin bar; once every row is
   done, one line says so (the rows stay, ticked; the ✦ menu still opens it). Show me (the sheet's, and Ask Azu's Show me card under a
   "how do I…" answer) plays the item's gesture on its target (round 6's Show me: its ghost finger and ring, gdFinger and gdRing; its own end, so an older Show me never cuts it short), taking the visitor there first:
   the wall for a marker, the card at its element for a cue (cuePlace), a tour at Mini for the guide's Pause; it writes no memory.
   Unity: the sheet a UI Toolkit panel like the card's sheets (a VisualElement with the card's USS: background, border, radius; a ListView
   of rows, each a Label, a tick and a Button; the bar a VisualElement whose width is the share done); Show me the ghost finger's sprite
   and its tween (as round 6); the Show me card a link card of Ask Azu's answer (a Button styled as its other cards) ----- */
function gdTryHtml(){var V=S.gdVisitor,rows=gdTryDefs(),c=gdTryCount(V,rows),all=c.n>0&&c.done===c.n;
  return'<div class="gdtry" role="dialog" aria-label="'+TXH("gd_try")+'"><div class="gdtry-h"><b>'+TXH("gd_try")+'</b><button type="button" class="gdtry-x" data-gdtryx="1" aria-label="'+TXH("close")+'">&#10005;</button></div>'+
    '<p class="gdtry-n'+(all?" all":"")+'" role="status">'+(all?TXH("gd_try_all"):esc(fmt(TXT("gd_try_count"),c.done,c.n)))+'</p>'+
    '<div class="gdtry-bar" aria-hidden="true"><i style="width:'+(c.n?Math.round(c.done/c.n*1000)/10:0)+'%"></i></div>'+
    '<ul class="gdtry-list">'+rows.map(function(it){var d=gdTryDone(it,V);return'<li class="gdtry-row'+(d?" done":"")+'" data-gdtryrow="'+it.id+'"><span class="gdtry-tick" role="img" aria-label="'+(d?TXH("gd_try_done"):TXH("gd_try_todo"))+'">'+(d?"&#10003;":"")+'</span>'+
      '<span class="gdtry-w">'+esc(gdTryWord(it))+'</span><button type="button" class="gdtry-show" data-gdtryshow="'+it.id+'">'+TXH("gd_btn_show")+'<span aria-hidden="true">&#8250;</span></button></li>'}).join("")+'</ul></div>'}
/* drawn with the phone (a redraw keeps it); a newer Editor demo closes the one an older demo opened */
on("renderPhone.after",function(){var o=ph.querySelector(".gdtry");if(o)o.remove();if(G.gd.tryOpen&&G.gd.tryOwner!=null&&G.gd.tryOwner!==G.demoSeq){G.gd.tryOpen=false;G.gd.tryOwner=null}
  if(!G.gd.tryOpen||S.view!=="card"||noDraw)return;ph.insertAdjacentHTML("beforeend",gdTryHtml())});
/* Settings › The guide: Things to try › (the same sheet), after Show the tips again */
on("settingsPanel.html",function(h){var d=document.createElement("div");d.innerHTML=h;var g=d.querySelector(".vtips");if(!g||!gdTryDefs().length)return h;
  g.insertAdjacentHTML("beforeend",'<button type="button" class="vtry" data-gdtry="settings"><span>'+TXH("gd_try")+'</span><span aria-hidden="true">&#8250;</span></button>');return d.innerHTML});
/* Show me: the visitor taken to the target (a marker: the wall; a cue: the card at its element; the guide's Pause: a tour at Mini; the ✦:
   where it shows, else the wall), then the gesture once; while it plays the scheduler is quiet (context.showing) */
function gdTryShow(id,from){var it=gdTryItem(id);if(!it)return false;var why=gdPullGate();if(why){gdLog("skipped","Show me ("+it.label+"): "+why);return false}
  G.gd.tryOpen=false;G.gd.tryOwner=null;S.settingsOpen=false;S.contentsOpen=false;G.fab.menu=false;if(G.gd.cur)gdStop("the visitor asked: Show me");gdFingerStop();
  var n=G.gd.tryN=(G.gd.tryN||0)+1,w=gdTryPlace(it);G.gd.tryShow={id:it.id,from:from,n:n,where:w.where,played:false,realUntil:performance.now()+w.ms+3400};
  gdLog("shown","(pulled) Show me: "+gdTryWord(it)+" ("+from+"), on "+w.where);clearTimeout(G.gd.tryT);G.gd.tryT=setTimeout(function(){gdTryPlay(it,n)},w.ms);return true}
function gdTryPlace(it){
  if(it.form==="cue"){cuePlace(it.id);return{where:"the card, at its element",ms:820}}
  if(it.about==="pause"){S.view="card";if(!G.on){S.stop="full";renderPhone(false);S.introSeen=true;startTour()}G.askOpen=false;S.dockH="mini";if(G.paused)resumeTour();renderGuide();return{where:"a tour, at its Pause",ms:900}}
  var wall=/^(marker|middle|point)$/.test(it.about);
  if(!wall&&gdTarget(it)){if(S.view==="card")renderPhone(true);return{where:"the screen as it is",ms:160}}
  if(G.on)stopTour();G.cam.open=false;S.view="card";S.stop="closed";renderPhone(false);
  /* no marker in view: the camera turns to the point with a card (the phone moves in the app; the Lab pans the picture) */
  if(wall&&!gdMarker()){var pc=(S.wallPois||[]).filter(function(p){return p.card})[0];if(pc&&typeof agCenter==="function")agCenter(pc.id)}return{where:"the wall",ms:520}}
function gdTryPlay(it,n){var s=G.gd.tryShow;if(!s||s.n!==n)return;G.gd.hopMs=0;var f=null,M=null,el=null;
  if(it.form==="cue"){el=cueEl(it.about);if(!el){gdLog("skipped","Show me ("+it.label+"): "+(GD_CUE_NO[it.about]||"its part is not on the screen"));return}
    var g=GD_GESTURE[it.about];if(g==="drag"||g==="swipe")f=gdFinger(it,null,el,g);
    else{var te=it.about==="navbar"&&el.querySelector("[data-navsec]")||el,B=gdPR(te);M={x:(B.l+B.r)/2,y:(B.t+B.b)/2,rect:B,box:B};f=gdFinger(it,M,null,"tap")}}
  else{M=gdTarget(it);if(!M){gdLog("skipped","Show me ("+it.label+"): its target is not on the screen");return}if(it.about==="pause")M.avoid=gdDockWords();f=gdFinger(it,M,null,GD_GESTURE[it.about]||"tap");if(!M.rect){var rg=gdRing({about:it.about},!S.reduce);if(rg){rg.style.left=Math.round(M.x)+"px";rg.style.top=Math.round(M.y)+"px"}}}
  s.played=true;s.gesture=f?f.dataset.gesture:"";s.view=S.view;s.stop=S.stop;s.tour=!!G.on;s.box=M?(M.rect||M.box||{l:M.x-GD_RING,t:M.y-GD_RING,r:M.x+GD_RING,b:M.y+GD_RING}):gdPR(el);s.end=f?[+f.dataset.endx,+f.dataset.endy]:null;
  clearTimeout(G.gd.tryEndT);G.gd.tryEndT=setTimeout(function(){if(G.gd.tryN!==n||G.gd.cur)return;gdFingerStop();ph.querySelectorAll(".gdring").forEach(function(x){x.remove()})},S.reduce?2600:2500)}
/* the words beside the guide's Pause (the dock's line and hint, Azu's bubble above it): the finger keeps off them */
function gdDockWords(){var o=[],pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight;ph.querySelectorAll(".gdock .gd-stop,.gdock .gd-cap,.gdock .gd-hint,.gbubble").forEach(function(e){if(!gdSeen(e))return;
  /* the words' own extent (a Range over them), not their box: the line's box runs on under the buttons */
  var rg=document.createRange();rg.selectNodeContents(e);var r=rg.getBoundingClientRect();if(r.width>0&&r.height>0)o.push({l:(r.left-pr.left)/q,t:(r.top-pr.top)/q,r:(r.right-pr.left)/q,b:(r.bottom-pr.top)/q})});return o}
/* Ask Azu's Show me card, under a "how do I…" answer (the live agent's show_how, or the offline match): the agent's other cards' look */
on("agTurnHtml.html",function(h,turn){if(!turn||!turn.shows||!turn.shows.length)return h;
  return h+'<div class="gdhows">'+turn.shows.map(function(id){var it=gdTryItem(id);if(!it)return"";return'<button type="button" class="gdhowc" data-gdhow="'+esc(id)+'"><span class="gdhowc-ic" aria-hidden="true">'+GD_FINGER+'</span><b>'+TXH("gd_btn_show")+'</b><small>'+esc(gdTryWord(it))+'</small><i aria-hidden="true">&#8250;</i></button>'}).join("")+'</div>'});
/* the taps: the ✕, a row's Show me, Settings' link, Ask's Show me card */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gdtry],[data-gdtryx],[data-gdtryshow],[data-gdhow]");if(!t)return;e.stopPropagation();e.preventDefault();
  if(t.dataset.gdtryx){gdTryClose();return}if(t.dataset.gdtry){gdTryOpen(t.dataset.gdtry==="settings"?"Settings › The guide":t.dataset.gdtry);return}
  if(t.dataset.gdtryshow){gdTryShow(t.dataset.gdtryshow,"Things to try");return}if(t.dataset.gdhow){gdTryShow(t.dataset.gdhow,"Ask Azu")}},true);

/* ----- round 12, Fun › On the tour: the beats' look (research_tour_entertainment.md 5.1, 5.3, 4.5). A beat shows in the panel: the beat card
   (`.gdbeat`), one opaque surface in the Look's own colours, set on the dock (at Mini: the dock's controls, its progress line and the quiz's
   star stay below it) or at the foot of the phone at Compact; Azu stands on its top edge. Its line is spoken (the tour's voice is on), the
   word being said lit; its chips are 44 px or more; it never covers words being told (a beat plays at a seam: nothing is told then).
   - The quiz promise: its line, the star at the end of the progress line (every Look), Deal / No quiz, thanks for 4 s (waiting: Deal);
     Expert: Want three tough questions? Yes, please / No, thanks (waiting: no, an opt-in).
   - The breather: its line, Azu drinks (the prop beside Azu, 0 px over words: mascot/azu2d.js's azMoment; a fan in summer, a scarf in
     winter), then Go on / A question first (8 s: Go on). A question first: the panel opens at Peek, the talk button listens; once
     answered, "Back to our story." and the next step. The cookie, the same before the last step.
   - The quiz (the finale): Time for my questions!, then each question (2-3 choices, Not sure; tap always, or say it), right / wrong with
     its why, Show me (the block it came from), no timer; the score (never mocking) and the best here; Skip the quiz.
   - Guess the year: its line and the step's question, a slider in the author's range (Tell me; Not now for 4 s); the step says the year;
     after it, the two pins (the guess, the answer) and Azu's reaction by distance (3 bands).
   - A game's Not now (its 4 s chip): two this visit end the games for the visit, said once.
   Unity: the beat card a UXML template in the dock's sheet (its Look's USS); the chips UI Toolkit Buttons; the 4 s bar a scale
   transition; the quiz the knowledge-check views of 8A (Choice.Mark, VerdictMark, CardIcons.VectorGlyph); the slider a SliderInt with
   the Before / After handle and two pin elements placed by value; the speech the platform recogniser's text through gdQuizMatch's rules
   (plain C#) ----- */
function gdBeatLook(){return typeof dkLook==="function"?dkLook():"ink"}
/* where the card sits: on the dock's top edge (Mini), at the foot above the ✦ (Compact), else 16 px from the foot */
function gdBeatPlace(){var b=ph.querySelector(".gdbeat");if(!b)return;var lk=gdBeatLook();if(!b.classList.contains("bt-"+lk)){b.classList.remove("bt-ink","bt-paper","bt-player","bt-prompter");b.classList.add("bt-"+lk)}var H=ph.offsetHeight,d=ph.querySelector(".gdock"),w=ph.querySelector(".gbtn-wrap"),bot=16;
  if(d&&d.getClientRects().length){var r=phRect(d);bot=Math.round(H-r.t+10)}else if(w&&w.getClientRects().length){var q=phRect(w);bot=Math.round(H-q.t+10)}
  b.style.bottom=bot+"px";var m=ph.querySelector(".mascot");if(m&&m.getClientRects().length&&!b.classList.contains("bt-away")){var br=phRect(b),mh=m.offsetHeight||64;ph.style.setProperty("--gd-bt-top",Math.max(64,Math.round(br.t-mh+8))+"px");m.classList.add("gd-onbeat")}}
/* the line's words, each its own span (the word being said lit) */
function gdBeatWords(t){return String(t||"").split(/(\s+)/).map(function(w){return/^\s+$/.test(w)?w:'<span class="btw">'+esc(w)+'</span>'}).join("")}
/* a beat's line: spoken through the one order of voices (say), its word lit; with a screen reader on, announced (aria-live) instead.
   then(): when it has been said (or its time by its words, whichever first: a voice that never ends never holds the tour) */
function gdBeatSay(t,then,sel){var B=G.gd.bt;if(!B)return;var my=B.lineSeq=G.gd.lineN=(G.gd.lineN||0)+1,done=false,fin=function(){if(done||G.gd.bt!==B||B.lineSeq!==my)return;done=true;clearTimeout(B.sayT);ph.querySelectorAll(".gdbeat .btw.on").forEach(function(w){w.classList.remove("on")});if(then)then()};
  ph.querySelectorAll(".gdbeat .btw.on").forEach(function(w){w.classList.remove("on")});var p=ph.querySelector(".gdbeat "+(sel||".bt-line"));if(p){p.innerHTML=gdBeatWords(t);p.setAttribute("aria-label",t)}if(!sel)B.said=t;var n=String(t).split(/\s+/).filter(Boolean).length,ms=n*60000/(170*(typeof guideSpeed==="function"?guideSpeed():1))+900;
  B.sayT=setTimeout(fin,ms+1400);if(S.ui.gdSR){clearTimeout(B.sayT);B.sayT=setTimeout(fin,Math.min(ms,2600));return}
  try{say(t,function(k){if(G.gd.bt!==B||B.lineSeq!==my)return;(p?p.querySelectorAll(".btw"):[]).forEach(function(w,i){w.classList.toggle("on",i===k)})},fin)}catch(e){}}
/* the card: its eyebrow, its line, its body (chips, the quiz, the slider) and foot */
function gdBeatHtml(B){var L=gdBeatLook();return'<div class="gdbeat bt-'+L+(B.kind?" bk-"+B.kind:"")+(S.reduce?"":" bt-in")+'" role="dialog" aria-label="'+esc(B.label||"")+'" aria-live="polite">'+
  '<small class="bt-eye">'+esc(B.eye||"AZU")+'</small><p class="bt-line">'+gdBeatWords(B.said||"")+'</p><div class="bt-body"></div><div class="bt-foot"></div></div>'}
function gdBeatDraw(B){var o=ph.querySelector(".gdbeat");if(o)o.remove();ph.insertAdjacentHTML("beforeend",gdBeatHtml(B));ph.classList.add("gd-beating");gdBeatPlace()}
function gdBeatBody(h,foot){var b=ph.querySelector(".gdbeat .bt-body"),f=ph.querySelector(".gdbeat .bt-foot");if(b)b.innerHTML=h||"";if(f)f.innerHTML=foot||"";gdBeatPlace()}
function gdBeatEye(t){var e=ph.querySelector(".gdbeat .bt-eye");if(e)e.textContent=t}
/* a chip; a timed one carries its bar (it empties over its seconds: the 4 s Not now, the 8 s Go on) */
function gdBeatChip(act,word,cls,secs){return'<button type="button" class="bt-chip'+(cls?" "+cls:"")+'" data-gdbt="'+act+'">'+esc(word)+(secs?'<i class="bt-bar" style="animation-duration:'+secs+'s"></i>':"")+'</button>'}
/* a timed wait: then() after s seconds (cancelled by any answer: a new wait or the card's end) */
function gdBeatWait(s,then){var B=G.gd.bt;if(!B)return;clearTimeout(B.waitT);B.waitT=setTimeout(function(){if(G.gd.bt===B)then()},s*1000)}
/* the card's end: the beat ends (gdStop: its memory, its how), then the tour's next step (the hold released) */
function gdBeatFinish(why,how){var c=G.gd.cur,pull=c&&c.pull;gdStop(why,how);if(!G.on||pull&&G.gd.demo){if(!G.on)gdResume(pull);return}
  if(G.gd.hold&&G.paused){G.gd.hold=false;if(G.i<S.guideSteps.length-1)G.i++;renderGuide();return}gdResume(pull)}
function gdBeatClose(){var B=G.gd.bt;if(B){clearTimeout(B.waitT);clearTimeout(B.sayT);clearTimeout(B.stepT);B.lineSeq=-1}G.gd.bt=null;ph.classList.remove("gd-beating");
  ph.querySelectorAll(".gdbeat,.gdshowback").forEach(function(x){x.remove()});var m=ph.querySelector(".mascot");if(m)m.classList.remove("gd-onbeat");if(G.moment)azMomentEnd(false)}
/* a beat on the phone (guidance/service.js's gdStart): by its kind */
function gdBeatPresent(c){var it=c.item;gdBeatClose();if(G.on){clearGuideMarks();if(S.dockH!=="mini"&&S.dockH!=="compact"){S.dockH="mini";renderGuide()}}
  var B=G.gd.bt={item:it,c:c,kind:it.beat,label:it.label,eye:"AZU",said:""};
  if(c.react){if(it.beat==="word"){B.kind="wordres";gdWordResult(B)}else{B.kind="guess";gdGuessReact(B)}return}
  if(it.beat==="promise")gdPromise(B);else if(it.beat==="finale")gdQuiz(B);else if(it.beat==="guess")gdGuess(B);else if(it.beat==="word")gdWordAnnounce(B);else if(it.beat==="find")gdFind(B);else gdBreather(B)}
/* a game's Not now: counted (two this visit: no more games, said once) */
function gdBeatNotNow(B){var t=gdNotNow(B.item);if(t){gdBeatBody("","");gdBeatSay(t,function(){gdBeatFinish("Not now: no more games this visit","dismissed")});return true}return false}
/* --- the quiz promise --- */
/* round 13 (the developer's answer on round 12): the promise says its line on the card, then the card goes and its two chips stay up for
   4 s above the dock while step 1 starts (the tour no longer waits about 9 s); waiting is a Deal (Expert's opt-in: waiting is no) */
function gdPromise(B){var it=B.item,ex=aud()==="expert"&&it.expert==="optin",P=G.gd.plan;B.eye="AZU · "+TXT("tour_star").toUpperCase();B.optin=ex;
  B.said=ex?TXT("tour_promise_expert"):gdLine(it,false,{aud:""});if(P&&!ex)P.quiz="pending";gdBeatDraw(B);if(G.on&&!ex)renderGuide();
  var w=+it.waits||4,chips=ex?[["yes",TXT("tour_yes_please"),"pri",0],["no",TXT("tour_no_thanks"),"",w]]:[["deal",TXT("tour_deal"),"pri",w],["noquiz",TXT("tour_noquiz"),"",0]];
  gdBeatSay(B.said,function(){if(G.gd.bt!==B)return;var pull=B.c&&B.c.pull&&G.gd.demo;gdTailShow({kind:"promise",item:it,optin:ex,chips:chips,secs:w,wait:function(){gdPromiseAns(ex?"no":"deal",true)}});
    if(!pull)gdBeatFinish("the promise said: its chips stay "+w+" s while the tour goes on","");else{gdBeatClose();gdStop("the promise said (a demo)")}})}
function gdPromiseAns(a,waited){var T=G.gd.tail;if(!T||T.kind!=="promise")return;var P=G.gd.plan,yes=a==="deal"||a==="yes";
  if(P)P.quiz=yes?"on":"off";if(yes){gdTailEnd();azReact("r-wink",900,"big");gdLog("event","the quiz promised: "+(waited?"4 s with no answer: a Deal":a==="yes"?"Yes, please (Expert's opt-in)":"Deal"));if(G.on)renderGuide();return}
  var f=gdPlanBeat("quizfinale");if(f)gdBeatDrop(f,T.optin?"No, thanks (Expert's opt-in)":"No quiz, thanks");gdLog("event","no quiz: "+(T.optin?(waited?"no answer to the opt-in":"No, thanks"):"No quiz, thanks"));
  var t=!T.optin&&a==="noquiz"?gdNotNow(T.item):"";if(t){gdTailMsg(t,3200)}else gdTailEnd();if(G.on)renderGuide()}
/* the star at the end of the progress line (guide/dock.js draws it in every Look): from the promise to the quiz, while it is on */
function gdQuizStar(){var P=G.gd.plan;if(!G.on||!P)return"";var f=gdPlanBeat("quizfinale",P);if(!f)return"";if(P.quiz==="on"&&(f.state==="planned"||G.gd.bt&&G.gd.bt.kind==="finale"))return"on";if(P.quiz==="pending")return"pending";return""}
/* --- the breather (and the cookie) --- */
function gdBreather(B){var it=B.item,v=S.gdVisitor.visit,used=v.props||(v.props=[]),prop=gdPropOf(it,gdTodayMD(),used);B.prop=prop;if(used.indexOf(prop)<0)used.push(prop);
  B.said=gdLine(it,false,{aud:""});gdBeatDraw(B);gdBeatBody("","");
  var ask=function(){if(G.gd.bt!==B)return;B.stage="ask";gdBeatBody('<div class="bt-chips">'+gdBeatChip("goon",TXT("tour_goon"),"pri",+it.waits||8)+gdBeatChip("qfirst",TXT("tour_qfirst"),"",0)+'</div>',"");
    if(it.answers==="nothing"){gdBeatBody("","");gdBeatSay(TXT("tour_breather_ask"),function(){gdBeatFinish("the breather: it goes on (no chips)","")});return}
    var bars=ph.querySelectorAll(".gdbeat .bt-bar");bars.forEach(function(x){x.style.animationPlayState="paused"});
    gdBeatSay(TXT("tour_breather_ask"),function(){bars.forEach(function(x){x.style.animationPlayState=""});gdBeatWait(+it.waits||8,function(){gdBreatherAns("goon",true)})})};
  /* the line, then Azu drinks (the prop placed beside it by azMoment: 0 px over words; it waits for room) */
  gdBeatSay(B.said,function(){if(G.gd.bt!==B)return;azMoment(prop,function(){ask()},"",{auto:true,quiet:true})})}
function gdBreatherAns(a,waited){var B=G.gd.bt;if(!B||B.kind!=="breather"||B.stage!=="ask")return;clearTimeout(B.waitT);B.stage="done";
  if(a==="goon"){gdBeatBody("","");gdBeatSay(TXT("tour_continue"),function(){gdBeatFinish(waited?"8 s with no answer: Go on":"Go on",waited?"ignored":"acted")});return}
  /* A question first: the panel opens (Peek) with the talk button listening; the tour waits for the answer */
  gdBeatClose();G.gd.askFirst={at:performance.now()};gdStop("A question first","acted");S.dockH="peek";renderGuide();try{listen(false)}catch(e){}}
/* the answer given (guide/ask.js or the agent: ask.after): once it has been said, "Back to our story." and the next step */
on("ask.after",function(t){var q=G.gd.askFirst;if(!q||q.answered)return;q.answered=true;var t0=performance.now(),est=Math.min(20000,String(t||"").split(/\s+/).length*60000/170+1500);
  (function w(){if(!G.gd.askFirst)return;if((G.utter||G.recAudio||G.timer)&&performance.now()-t0<est){setTimeout(w,300);return}G.gd.askFirst=null;if(!G.on)return;
    S.dockH="mini";G.askOpen=false;G.paused=false;G.resumeVoice=false;renderGuide();var B=G.gd.bt={kind:"back",label:TXT("tour_back"),eye:"AZU",said:TXT("tour_back")};gdBeatDraw(B);gdBeatSay(B.said,function(){gdBeatClose();gdResume(false)})})()});
/* Play or Next on the dock while it waits for the question: back to the story */
ph.addEventListener("click",function(e){if(!G.gd.askFirst||!G.on)return;var t=e.target.closest('[data-g="next"],[data-g="play"]');if(!t)return;e.stopPropagation();e.preventDefault();G.gd.askFirst=null;G.paused=false;G.resumeVoice=false;G.askOpen=false;S.dockH="mini";gdBeatClose();gdResume(false)},true);
/* --- guess the year --- */
function gdGuessOf(B){var P=G.gd.plan,b=B.c&&B.c.beat||gdPlanBeat("guessyear"),j=P&&b?P.ids.indexOf(b.after)+1:-1,id=j>0?P.ids[j]:null;return id?{step:id,g:S.tourGuess[id]}:null}
function gdGuess(B){var x=gdGuessOf(B);if(!x||!x.g){gdBeatFinish("no guess on the next step","");return}B.eye="AZU · "+(S.lang==="pt"?"PALPITE":"GUESS");B.gs=x;var g=x.g,mid=Math.round((+g.from+ +g.to)/2);B.v=mid;B.touched=false;
  B.said=gdLine(B.item,false,{aud:""});gdBeatDraw(B);
  gdBeatBody('<p class="bt-q">'+esc(L(g.q))+'</p><div class="bt-slider"><output class="bt-val">'+mid+'</output><input type="range" class="bt-range" min="'+g.from+'" max="'+g.to+'" step="1" value="'+mid+'" aria-label="'+esc(L(g.q))+'"><div class="bt-ends"><span>'+g.from+'</span><span>'+g.to+'</span></div></div>'+
    '<div class="bt-chips">'+gdBeatChip("tell",TXT("tour_tellme"),"pri",0)+gdBeatChip("notnow",TXT("tour_notnow"),"",4)+'</div>',"");
  gdBeatSay(B.said);B.notNowT=setTimeout(function(){var n=ph.querySelector('.gdbeat [data-gdbt="notnow"]');if(n&&G.gd.bt===B){n.classList.add("bt-gone");setTimeout(function(){if(n.isConnected)n.remove()},300)}},4000);
  gdBeatWait(+B.item.waits||8,function(){if(!B.touched)gdGuessIdle()})}
function gdGuessIdle(){var B=G.gd.bt;if(!B||B.kind!=="guess")return;gdBeatBody("","");gdBeatSay(TXT("tour_keepgoing"),function(){gdBeatFinish(B.item.waits+" s with no guess: the tour goes on","ignored")})}
function gdGuessTell(){var B=G.gd.bt;if(!B||B.kind!=="guess"||B.told)return;B.told=true;clearTimeout(B.waitT);clearTimeout(B.notNowT);var v=+B.v;
  G.gd.guess={step:B.gs.step,v:v,g:B.gs.g,shown:false};gdLog("event","Guess the year: "+v+" (the step "+B.gs.step+" says it)");gdBeatBody(gdGuessTrack(B.gs.g,v,null),"");gdBeatSay(TXT("tour_guess_wait"),function(){gdBeatFinish("Tell me: "+v,"acted")})}
/* the track with its pins (the guess; the answer once the step said it) */
function gdGuessTrack(g,v,ans){var pc=function(x){return Math.max(0,Math.min(100,(x-g.from)/(g.to-g.from)*100)).toFixed(1)};
  return'<div class="bt-track" aria-hidden="true"><i class="bt-rail"></i><b class="bt-pin you" style="left:'+pc(v)+'%"><span>'+esc(TXT("tour_guess_you"))+' · '+v+'</span></b>'+(ans!=null?'<b class="bt-pin ans" style="left:'+pc(ans)+'%"><span>'+esc(TXT("tour_guess_it"))+' · '+ans+'</span></b>':"")+'</div><div class="bt-ends"><span>'+g.from+'</span><span>'+g.to+'</span></div>'}
/* after the step that held the year (guidance/service.js's seam): the two pins and Azu's reaction by distance; then on */
function gdGuessAnswer(gs){var it=gdItems().filter(function(x){return x.id==="guessyear"})[0]||S.guidance.items.guessyear;gdStart(it,"the answer after its step",{pull:true,react:true})}
function gdGuessReact(B){var gs=G.gd.guess;if(!gs){gdBeatFinish("","");return}var g=gs.g,v=gs.v,band=gdGuessBand(g,v),d=Math.abs(v-g.n),ans=String(g.n);B.eye="AZU · "+(S.lang==="pt"?"PALPITE":"GUESS");B.band=band;
  var t=band===1?fmt(TXT("tour_guess_b1")).replace("{answer}",ans):band===2?fmt(TXT("tour_guess_b2"),d):TXT(g.n<v?"tour_guess_b3_older":"tour_guess_b3_newer").replace("{answer}",ans);B.said=t;
  gdBeatDraw(B);gdBeatBody(gdGuessTrack(g,v,g.n),"");azReact(band===1?"r-jump":band===2?"r-wink":"r-blush",900,"big");gdLog("event","Guess the year: "+v+" against "+g.n+", band "+band);
  gdBeatSay(t,function(){B.stepT=setTimeout(function(){if(G.gd.bt===B)gdBeatFinish("the guess's answer said","")},1400)})}
ph.addEventListener("input",function(e){var r=e.target.closest(".gdbeat .bt-range");if(!r)return;var B=G.gd.bt;if(!B)return;B.v=+r.value;B.touched=true;var o=ph.querySelector(".gdbeat .bt-val");if(o)o.textContent=r.value;gdBeatWait(+B.item.waits||8,function(){gdGuessIdle()})},true);
/* --- the quiz (the finale) --- */
function gdQuiz(B){var it=B.item,P=G.gd.plan,owed=B.c.owed;B.qs=owed?owed.qs:gdQuizQs(P);B.k=0;B.right=0;B.eye="AZU · "+TXT("tour_star").toUpperCase();B.point=P&&P.point||"painting";
  if(B.qs.length<2){B.said=TXT("tour_quiz_few");if(P)P.quiz="off";gdBeatDraw(B);if(G.on)renderGuide();gdLog("event","the quiz: "+B.qs.length+" question(s) from the steps heard: saved for next time");
    gdBeatSay(B.said,function(){B.stepT=setTimeout(function(){gdBeatFinish("too few questions from the steps heard","")},1000)});return}
  B.said=gdLine(it,false,{aud:""});gdBeatDraw(B);gdLog("event","the quiz: "+B.qs.length+" questions ("+B.qs.join(", ")+")");gdBeatSay(B.said,function(){gdQuizAsk()})}
function gdQuizQ(B,k){return S.tourQ[B.qs[k]]}
function gdQuizFoot(){return gdBeatChip("skip",TXT("tour_skipquiz"),"bt-skip",0)}
function gdQuizMic(){return!!(window.SpeechRecognition||window.webkitSpeechRecognition)&&S.guidance.items.quizfinale.answers==="tapsay"}
function gdQuizAsk(){var B=G.gd.bt;if(!B||B.kind!=="finale")return;var q=gdQuizQ(B,B.k),n=B.qs.length;B.answered=null;gdBeatEye(fmt(TXT("tour_q_of"),B.k+1,n).toUpperCase());
  var say=S.guidance.items.quizfinale.answers==="tapsay";
  gdBeatBody('<div class="bt-choices" role="group" aria-label="'+esc(L(q.q))+'">'+q.a.map(function(a,i){return'<button type="button" class="bt-choice" data-gdbt="ans" data-i="'+i+'"><span class="bt-mark" aria-hidden="true"></span><span>'+esc(L(a))+'</span></button>'}).join("")+'</div>'+
    '<div class="bt-row"><span class="bt-hint">'+TXH(say?"tour_tapsay":"tour_tap")+'</span>'+(gdQuizMic()?'<button type="button" class="bt-mic" data-talk="1" aria-label="'+TXH("dock_ask")+'">'+MIC+'</button>':"")+gdBeatChip("unsure",TXT("tour_notsure"),"",0)+'</div>',gdQuizFoot());
  gdBeatSay(L(q.q))}
/* an answer: a tap (i), Not sure (-1), or the recogniser's text matched (guidance/scheduler.js's gdQuizMatch) */
function gdQuizAns(i,how){var B=G.gd.bt;if(!B||B.kind!=="finale"||B.answered!=null)return;var q=gdQuizQ(B,B.k),ok=+q.ok,right=i===ok;B.answered=i;if(right)B.right++;
  (B.log=B.log||[]).push({id:B.qs[B.k],i:i,ok:right,how:how||"tap"});
  ph.querySelectorAll(".gdbeat .bt-choice").forEach(function(b){var j=+b.dataset.i;b.disabled=true;b.classList.toggle("ok",j===ok);b.classList.toggle("no",j===i&&!right);b.setAttribute("aria-pressed",String(j===i))});
  if(right){azReact("r-jump",800,"big");haptic()}var ans=L(q.a[ok]),why=L(q.why),t=right?TXT("tour_right_"+(1+(B.k+B.right)%4)):(i<0?TXT("tour_unsure"):TXT("tour_wrong")).replace("{answer}",ans).replace("{why}",why);
  var last=B.k>=B.qs.length-1,nx=gdBeatChip("next",TXT(last?"tour_to_score":"tour_next_q"),"pri",0),sm=!right&&q&&S.guidance.items.quizfinale.showMe!==false?gdBeatChip("showme",TXT("tour_showme"),"",0):"";
  var row=ph.querySelector(".gdbeat .bt-row");if(row)row.outerHTML='<p class="bt-verdict '+(right?"ok":"no")+'" aria-live="polite">'+esc(t)+'</p><div class="bt-chips">'+sm+nx+'</div>';gdBeatPlace();
  gdBeatSay(t,function(){gdBeatWait(right?1.2:3.5,function(){if(!B.away)gdQuizNext()})},".bt-verdict")}
function gdQuizNext(){var B=G.gd.bt;if(!B||B.kind!=="finale"||B.answered==null)return;clearTimeout(B.waitT);B.k++;if(B.k<B.qs.length){gdQuizAsk();return}gdQuizScore()}
function gdQuizScore(){var B=G.gd.bt,n=B.qs.length,r=B.right,demo=B.c&&B.c.pull&&G.gd.demo,best=demo?gdQuizBest(B.point)||{right:r,n:n}:gdQuizKeep(B.point,r,n),P=G.gd.plan;if(!demo){if(P)P.quizDone=true;G.gd.owed=null}
  var t=fmt(TXT(r===n?"tour_score_all":r===0?"tour_score_none":r*2>n?"tour_score_most":"tour_score_some"),r,n);gdBeatEye(TXT("tour_star").toUpperCase());
  /* round 13: Don't say it!'s score said once more (research 5.4, step 8) */
  var ws=P&&P.wordScore;if(ws&&ws.n>0&&!demo)t+=" "+nameIn(fmt(TXT("tour_word_callback"),ws.caught,ws.n),"{thing}",ws.word);
  gdBeatBody('<div class="bt-score"><b>'+r+'<small>/'+n+'</small></b><span class="bt-best">'+esc(fmt(TXT("tour_best"),best.right,best.n))+'</span></div>',"");var p=ph.querySelector(".gdbeat .bt-line");if(p)p.textContent="";
  gdLog("event","the quiz: "+r+" of "+n+" (the best here "+best.right+" of "+best.n+")");azReact(r===n?"r-jump":"r-wink",900,"big");
  gdBeatSay(t,function(){B.stepT=setTimeout(function(){if(G.gd.bt===B)gdBeatFinish("the quiz: "+r+" of "+n,"acted")},1600)})}
/* Show me: the block the question came from, in view and ringed (the card waits below the fold), then back to the quiz */
function gdQuizShow(){var B=G.gd.bt;if(!B||B.kind!=="finale")return;clearTimeout(B.waitT);var st=byId(S.fullSteps||S.guideSteps,B.qs[B.k])||byId(S.guideSteps,B.qs[B.k]),el=st&&stepEl(st);B.away=true;
  var c=ph.querySelector(".gdbeat");if(c)c.classList.add("bt-away");var m=ph.querySelector(".mascot");if(m)m.classList.remove("gd-onbeat");
  if(st){if(st.view&&S.view!==st.view){S.view=st.view;renderPhone(false)}
    /* the block found once the screen is drawn (a view just changed redraws), scrolled into view and ringed */
    var tries=0;(function ring(){var e=stepEl(st);if(!e&&tries++<8){setTimeout(ring,60);return}if(!e)return;var y=e.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-90;scr.scrollTo({top:Math.max(0,y),behavior:S.reduce?"auto":"smooth"});e.classList.add("gdshowme");setTimeout(function(){e.classList.remove("gdshowme")},4200)})()}
  gdLog("event","Show me: the block of "+B.qs[B.k]);ph.insertAdjacentHTML("beforeend",'<button type="button" class="gdshowback bt-chip pri" data-gdbt="back">'+TXH("tour_back_quiz")+'</button>');
  B.stepT=setTimeout(function(){gdQuizBack()},4500)}
function gdQuizBack(){var B=G.gd.bt;if(!B||!B.away)return;clearTimeout(B.stepT);B.away=false;ph.querySelectorAll(".gdshowback").forEach(function(x){x.remove()});var c=ph.querySelector(".gdbeat");if(c)c.classList.remove("bt-away");gdBeatPlace();gdQuizNext()}
function gdQuizSkip(){var B=G.gd.bt;if(!B||B.kind!=="finale")return;var P=G.gd.plan;if(P)P.quizDone=true;G.gd.owed=null;gdLog("event","Skip the quiz");gdBeatFinish("Skip the quiz","dismissed")}
/* the speech path: what the recogniser heard, while a question waits, is matched to a choice (none: the hint says so; a tap still works) */
on("heard.answer",function(q,err){var B=G.gd.bt;if(!B||B.kind!=="finale"||B.answered!=null||!B.qs||B.k>=B.qs.length)return;var h=ph.querySelector(".gdbeat .bt-hint");
  if(!q){if(h)h.textContent=TXT("tour_tap");return true}var qq=gdQuizQ(B,B.k),i=gdQuizMatch(q,qq.a.map(function(a){return L(a)}),S.lang||"en");
  if(i<0){if(h)h.textContent="“"+q+"”: "+TXT("tour_tap");return true}gdQuizAns(i,"said: “"+q+"”");return true});
/* the ✦ menu's Your 3 questions (the tour stopped for good before its quiz): the quiz on the card, a pull */
function gdQuizPull(){var o=G.gd.owed;if(!o)return false;var it=gdItems().filter(function(x){return x.id==="quizfinale"})[0]||S.guidance.items.quizfinale;gdStart(it,"asked from the ✦ menu (the tour stopped before its quiz)",{pull:true,owed:o});return true}
/* the card's taps */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gdbt]");if(!t)return;e.stopPropagation();var a=t.dataset.gdbt,B=G.gd.bt;if(a==="back"){gdQuizBack();return}if(!B)return;
  if(B.kind==="promise"){gdPromiseAns(a,false);return}
  if(B.kind==="breather"){gdBreatherAns(a,false);return}
  if(B.kind==="guess"){if(a==="tell")gdGuessTell();else if(a==="notnow"){clearTimeout(B.waitT);clearTimeout(B.notNowT);if(!gdBeatNotNow(B))gdBeatFinish("Not now","dismissed")}return}
  if(B.kind==="finale"){if(a==="ans")gdQuizAns(+t.dataset.i,"tap");else if(a==="unsure")gdQuizAns(-1,"Not sure");else if(a==="next")gdQuizNext();else if(a==="showme")gdQuizShow();else if(a==="skip")gdQuizSkip()}},true);
/* the card follows the dock (a height change, a new step drawn) */
on("renderGuide.after",function(){if(ph.querySelector(".gdbeat"))gdBeatPlace()});
/* Settings › The guide › Games on the tour (On / Off: the quiz and Guess the year; the breather is not a game) */
on("settingsPanel.html",function(h){var d=document.createElement("div");d.innerHTML=h;var g=d.querySelector(".vtips");if(!g)return h;var P=gdPrefs(),cur=P.games||"on",o=function(v,lab){return'<button type="button" data-gdpref="games|'+v+'" aria-pressed="'+(cur===v)+'">'+esc(lab)+'</button>'};
  g.insertAdjacentHTML("beforeend",'<div class="vgames"><small>'+TXH("settings_games")+'</small><div class="vo">'+o("on",TXT("settings_on"))+o("off",TXT("settings_off"))+'</div></div>');return d.innerHTML});

/* ----- round 13, the tail (`.gdstrip`): a beat's chips that stay up while the tour goes on (the quiz promise's Deal / No quiz, thanks;
   Don't say it!'s Let's play / Not now), and Don't say it!'s word card with its pip and, at Compact or with Mascot Off, its Heard it!
   button. It sits above the dock like the Mini's controls (at Compact by the ✦), its own pieces opaque in the Look's colours, and moves
   above the block being told whenever it would cover one of its words (0 px over the words being told, measured). Unity: a row of the
   dock's sheet (UXML) above the Mini, the chips UI Toolkit Buttons with the 4 s bar a scale transition ----- */
function gdTailShow(o){gdTailEnd();var T=G.gd.tail={kind:o.kind,item:o.item,optin:!!o.optin,chips:o.chips||[],secs:+o.secs||0,at:performance.now(),wait:o.wait};
  if(T.secs)T.t=setTimeout(function(){if(G.gd.tail===T){T.waited=true;T.wait()}},T.secs*1000);gdTailDraw();return T}
function gdTailEnd(){var T=G.gd.tail;if(T){clearTimeout(T.t);clearTimeout(T.msgT)}G.gd.tail=null;gdTailDraw()}
/* a line on the tail (shown, never spoken: the tour's voice goes on): two Not nows, Let's play another time, Nope! with no Azu */
function gdTailMsg(t,ms){var T=G.gd.tail;if(T){clearTimeout(T.t);T.chips=[]}else T=G.gd.tail={kind:"msg",chips:[],secs:0,at:performance.now()};T.msg=t;gdTailDraw();clearTimeout(T.msgT);T.msgT=setTimeout(function(){if(G.gd.tail===T){T.msg="";if(T.kind==="msg"||T.kind==="promise")gdTailEnd();else gdTailDraw()}},ms||2600)}
/* the ✦ and its pause at Compact, as drawn (the wrap's own box may be empty: its parts are placed) */
function gdStarBox(gb){var o=null;[gb].concat([].slice.call(gb.querySelectorAll("*"))).forEach(function(e){if(!e.getClientRects().length)return;var b=gdPR(e);if(b.r-b.l<8||b.b-b.t<8)return;o=o?{l:Math.min(o.l,b.l),t:Math.min(o.t,b.t),r:Math.max(o.r,b.r),b:Math.max(o.b,b.b)}:b});return o||{l:0,t:0,r:0,b:0}}
function gdHeardOn(){var d=G.gd.ds;return!!(d&&d.on&&G.on&&(S.dockH==="compact"||S.guideCfg.mascot===false))}
function gdTailDraw(){var T=G.gd.tail,d=G.gd.ds,el=ph.querySelector(".gdstrip"),word=d&&d.on&&G.on;
  if(!T&&!word){if(el)el.remove();var hb0=ph.querySelector(".gdheard");if(hb0)hb0.remove();return}if(!el){el=document.createElement("div");el.setAttribute("role","group");ph.appendChild(el)}
  el.className="gdstrip bt-"+gdBeatLook()+(S.dockH==="compact"?" tl-compact":"");el.setAttribute("aria-label",T&&T.item?T.item.label:TXT("tour_word_card"));
  var now=performance.now(),h="";
  if(word)h+='<div class="tl-word" aria-label="'+esc(TXT("tour_word_card")+": "+d.word)+'"><span class="tl-ic" aria-hidden="true">'+GD_NOSAY+'</span><span class="tl-w"><small>'+esc(TXT("tour_word_card"))+'</small><b>'+esc(d.word)+'</b></span>'+
    (d.caught?'<span class="tl-pip'+(G.gd.pipNew?" tl-new":"")+'" role="status">'+esc(fmt(TXT("tour_word_caught"),d.caught))+'</span>':"")+'</div>';
  if(T&&T.msg)h+='<div class="tl-msg" role="status">'+esc(T.msg)+'</div>';
  else if(T&&T.chips.length)h+='<div class="tl-chips">'+T.chips.map(function(c){var left=c[3]?Math.max(.1,c[3]-(now-T.at)/1000):0;return'<button type="button" class="bt-chip'+(c[2]?" "+c[2]:"")+'" data-gdtl="'+c[0]+'">'+esc(c[1])+(c[3]?'<i class="bt-bar" style="animation-duration:'+c[3]+'s;animation-delay:-'+(c[3]-left).toFixed(2)+'s"></i>':"")+'</button>'}).join("")+'</div>';
  /* Heard it!: by the ✦ at Compact (its own place, beside it), else at the strip's end (Mascot Off at Mini) */
  var gb=ph.querySelector(".gbtn-wrap.intour"),byStar=gdHeardOn()&&S.dockH==="compact"&&gb&&gb.getClientRects().length,hb=ph.querySelector(".gdheard");
  if(gdHeardOn()&&!byStar)h+='<button type="button" class="tl-heard" data-gdtl="heard">'+esc(TXT("tour_word_heard"))+'</button>';
  if(byStar){if(!hb){hb=document.createElement("button");hb.type="button";hb.className="tl-heard gdheard";hb.dataset.gdtl="heard";hb.textContent=TXT("tour_word_heard");ph.appendChild(hb)}var g=gdStarBox(gb);hb.style.left=Math.round(g.r+10)+"px";hb.style.top=Math.round((g.t+g.b)/2-24)+"px"}else if(hb)hb.remove();
  if(el.dataset.h!==h){el.innerHTML=h;el.dataset.h=h}G.gd.pipNew=false;gdTailPlace()}
/* the words being told: the block in focus and the panel's own line (its words' boxes, in the phone's pixels) */
function gdToldBoxes(){var out=[],f=scr.querySelector(".gfocus"),pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight;
  if(f&&f.getClientRects().length){var w=document.createTreeWalker(f,NodeFilter.SHOW_TEXT),n,rg=document.createRange();while((n=w.nextNode())){if(!/\S/.test(n.nodeValue))continue;rg.selectNodeContents(n);
    Array.prototype.forEach.call(rg.getClientRects(),function(r){if(r.width&&r.height)out.push({l:(r.left-pr.left)/q,t:(r.top-pr.top)/q,r:(r.right-pr.left)/q,b:(r.bottom-pr.top)/q})})}}
  return out.concat(gdDockWords())}
function gdTailOver(el){var r=gdPR(el);return gdToldBoxes().reduce(function(s,b){return s+gdOver(r,b)},0)}
function gdTailPlace(){var el=ph.querySelector(".gdstrip");if(!el)return;var H=ph.offsetHeight,top=H-16;
  /* above the stack at the foot: the panel (or the ✦ at Compact), its captions' bubble, a beat's card */
  ph.querySelectorAll(".gdock,.gbtn-wrap.intour,.gbubble,.gdbeat:not(.bt-away)").forEach(function(e){if(!e.getClientRects().length||getComputedStyle(e).visibility==="hidden")return;var r=gdPR(e);if(r.t>H*.35)top=Math.min(top,r.t)});
  var bot=Math.round(H-top+10);
  /* above that stack (like the Mini's controls); over a word being told, under the top bar instead */
  el.style.top="";el.style.bottom=bot+"px";var c1=gdTailOver(el);if(c1>0){el.style.bottom="";el.style.top="68px";if(gdTailOver(el)>=c1){el.style.top="";el.style.bottom=bot+"px"}}}
on("renderGuide.after",function(){if(G.gd.tail||G.gd.ds)gdTailDraw()});
on("stopTour.after",function(){gdTailEnd();ph.querySelectorAll(".gdstrip,.gdheard").forEach(function(e){e.remove()})});
setInterval(function(){if(ph.querySelector(".gdstrip"))gdTailPlace()},400);
/* the tail's taps */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gdtl]");if(!t)return;e.stopPropagation();e.preventDefault();var a=t.dataset.gdtl,T=G.gd.tail;
  if(a==="heard"){gdWordTap("Heard it!");return}if(!T)return;
  if(T.kind==="promise"){gdPromiseAns(a,false);return}
  if(T.kind==="word"){if(a==="play"){var d=G.gd.ds;if(d)d.play="yes";gdLog("event","Don't say it!: Let's play");gdTailEnd()}else if(a==="notnow"){gdWordNotNow()}}},true);
/* ----- round 13, Don't say it! (research_tour_entertainment.md 4.4, 5.4): the announce at the seam before the span (its line and the word on
   a small card, a crossed speech bubble), then the tour goes on into the span with Let's play / Not now up 4 s (waiting is Let's play) and
   the word card pinned beside Azu above the dock; a catch: Azu flinches and blushes (Reduce Motion: the pip only), the "Caught 1" pip, a
   light haptic, the voice never stopped, "Caught me!" at the sentence's end; a false alarm: Azu shakes its head and a "Nope!" for 1 s, no
   voice; the result at the next seam with the first missed slip played back (the recorded clip from 1.2 s before the word to 0.3 s after;
   with another voice, the phrase said again), the word lit. Unity: the word card a UXML template on the dock's sheet, the pip a Label
   with a USS scale transition, the clip AudioSource.PlayScheduled from the window's start with a stop after its end, the flinch an
   Animator trigger, the blush a morph or a Rive input ----- */
var GD_NOSAY='<svg viewBox="0 0 24 24"><path d="M4 5.5h16a1.5 1.5 0 0 1 1.5 1.5v8a1.5 1.5 0 0 1-1.5 1.5h-8l-4.5 3.5V16.5H4A1.5 1.5 0 0 1 2.5 15V7A1.5 1.5 0 0 1 4 5.5z" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round"/><path d="M5 3.5L19 18.5" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"/></svg>';
function gdWordAnnounce(B){var it=B.item,w=gdWordOf(S.lang||"en");if(!w){gdBeatFinish("no word in this language","");return}var P=G.gd.plan,b=B.c&&B.c.beat||gdPlanBeat("dontsay",P)||{span:w.span,endAfter:w.span[w.span.length-1]};
  B.eye="AZU · "+TXT("tour_word_eye").toUpperCase();B.said=gdLine(it,false,{aud:"",thing:w.w});gdBeatDraw(B);
  gdBeatBody('<div class="bt-word"><span class="tl-ic" aria-hidden="true">'+GD_NOSAY+'</span><b>'+esc(w.w)+'</b></div>',"");
  gdBeatSay(B.said,function(){if(G.gd.bt!==B)return;var pull=B.c&&B.c.pull&&G.gd.demo;gdWordStart(b);var wt=+it.waits||4;
    gdTailShow({kind:"word",item:it,chips:[["play",TXT("tour_letsplay"),"pri",wt],["notnow",TXT("tour_notnow"),"",0]],secs:wt,wait:function(){var d=G.gd.ds;if(d)d.play="yes";gdLog("event","Don't say it!: 4 s with no answer: Let's play");gdTailEnd()}});
    if(!pull)gdBeatFinish("announced, its span plays","acted");else{gdBeatClose();gdStop("Don't say it! announced (a demo)");var j=S.guideSteps.findIndex(function(s){return s.id===G.gd.ds.span[0]});if(j>=0&&G.on){G.paused=false;G.i=j;runStep()}}})}
function gdWordNotNow(){var T=G.gd.tail,d=G.gd.ds;gdLog("event","Don't say it!: Not now");var P=G.gd.plan,b=P&&gdPlanBeat("dontsay",P);if(b){b.why="Not now"}gdWordOff();
  var t=gdNotNow(S.guidance.items.dontsay);if(t)gdTailMsg(t,3200);else gdTailEnd()}
/* the game's look at each moment: a catch, a false alarm, the aside, the quiet end */
function gdWordLook(kind){var d=G.gd.ds,m=ph.querySelector(".mascot"),az=m&&m.getClientRects().length&&S.guideCfg.mascot!==false;
  if(kind==="caught"){G.gd.pipNew=true;gdTailDraw();haptic();if(az&&!S.reduce){azReact("r-flinch",520,"surprised");setTimeout(function(){if(G.gd.ds===d)azReact("r-blush",900,"big")},520)}return}
  if(kind==="nope"){var it=S.guidance.items.dontsay;if(it.falseTaps!=="nope")return;if(az){if(!S.reduce)azReact("r-shake",600,"serious");azBubble(TXT("tour_word_nope"),1000)}else gdTailMsg(TXT("tour_word_nope"),1000);return}
  if(kind==="aside"){if(az)azBubble(TXT("tour_word_caughtme"),1300);return}
  if(kind==="quiet"){gdTailMsg(TXT("tour_word_later"),2600)}}
/* the result, at the seam after the span: the lines by the score, and the first missed slip played back with its word lit */
function gdWordResult(B){var P=G.gd.plan,ws=P&&P.wordScore||{word:"",caught:0,n:0,falses:0},d=G.gd.ds,it=B.item,n=ws.n,c=ws.caught;gdTailEnd();
  B.eye="AZU · "+TXT("tour_word_eye").toUpperCase();var miss=d?d.order.map(function(k){return d.slips[k]}).filter(function(s){return!s.caught})[0]:null;
  var lines=[],t1=n===0?TXT("tour_word_zero"):c===n?(n===1?TXT("tour_word_all1"):fmt(TXT("tour_word_all"),n)):c===0?fmt(TXT("tour_word_none"),n):fmt(TXT("tour_word_some"),n,c);lines.push(t1);
  if(ws.falses>=3)lines.push(fmt(TXT("tour_word_false"),ws.falses));B.said=lines.join(" ");B.kind="wordres";gdBeatDraw(B);
  gdBeatBody('<div class="bt-wscore"><b>'+c+'<small>/'+n+'</small></b><span>“'+esc(ws.word)+'”'+(ws.falses?" · "+esc(fmt(TXT("tour_word_nopes"),ws.falses)):"")+'</span></div>',"");
  azReact(c===n&&n?"r-jump":"r-wink",900,"big");gdLog("event","Don't say it!: the result: “"+B.said+"”"+(miss?" · the slip played back: "+miss.key:""));
  var end=function(){B.stepT=setTimeout(function(){if(G.gd.bt===B){gdWordOff();gdBeatFinish("Don't say it!: "+c+" of "+n+" caught","acted")}},1400)};
  gdBeatSay(B.said,function(){if(G.gd.bt!==B)return;if(!miss||it.slipBack===false){end();return}
    gdBeatSay(TXT("tour_word_missed"),function(){if(G.gd.bt!==B)return;gdWordPlayBack(B,miss,function(){if(G.gd.bt!==B)return;gdBeatSay(TXT(c===0?"tour_word_dream":"tour_word_gotcha"),end)})})})}
/* the slip played back: the phrase on the card with its word lit; the recorded line's own clip (1.2 s before the word, 0.3 s after) when
   it was recorded, else the phrase said again by the voice the tour uses */
function gdWordPlayBack(B,sl,then){var ph0=sl.phrase,wds=ph0.words;B.slip=sl;
  var b=ph.querySelector(".gdbeat .bt-body");if(b)b.insertAdjacentHTML("beforeend",'<p class="bt-slip" aria-label="'+esc(wds.join(" "))+'">…'+wds.map(function(w,i){return'<span class="btw'+(i===ph0.at?" slipw":"")+'">'+esc(w)+'</span>'}).join(" ")+'…</p>');gdBeatPlace();
  var mark=function(){var sp=ph.querySelectorAll(".gdbeat .bt-slip .btw");if(sp[ph0.at])sp[ph0.at].classList.add("slipw")};
  var r=ph0.rec,rf=typeof recFile==="function"?recFile(sl.text):null;
  if(r&&rf&&rf.file&&rf.file.words&&rf.file.words[sl.k]){var w=rf.file.words[sl.k],a=new Audio(rf.url),from=Math.max(0,w[0]-1.2),to=w[1]+.3,done=false,fin=function(){if(done)return;done=true;try{a.pause()}catch(e){}G.gd.clip=null;then()};
    G.gd.clip={from:from,to:to,src:rf.url,k:sl.k};a.muted=!!S.guideMute;a.currentTime=from;a.addEventListener("timeupdate",function(){if(a.currentTime>=to)fin()});a.addEventListener("error",fin);
    var p=a.play();if(p&&p.catch)p.catch(function(){setTimeout(fin,(to-from)*1000)});setTimeout(fin,(to-from)*1000+1500);gdLog("event","the slip played back: the recorded clip "+from.toFixed(2)+"-"+to.toFixed(2)+" s");return}
  G.gd.clip={phrase:wds.join(" ")};gdLog("event","the slip played back: the phrase said again (“"+wds.join(" ")+"”)");gdBeatSay(wds.join(" "),function(){G.gd.clip=null;then()},".bt-slip");mark()}
/* the word card stays in the panel's view: a height change redraws it */
/* ----- round 13, Find it on the wall (research_tour_entertainment.md 4.7): "Lower the card and find the square on the real wall. Point me at
   it." The card lowers (the sheet closes: the wall's view comes back, the panel stays at Mini), a soft seek ring in the middle of the
   screen; when the point settles in the ring (the phone still 1.2 s: today's settle) "That's it!", the point lights with a short glint;
   Hint after (10 s): "A little to the left…" from the point's direction on the screen; I give up (and after Give up after, 20 s): Azu
   walks to it and shows it; then back to the card and the tour goes on. Never while the wall is lost (a system state wins: it ends
   quietly). In the Lab a drag on the wall stands in for the phone. Unity: the point's anchor through the AR camera's WorldToScreenPoint,
   its distance to the screen's centre under the ring's radius for 1.2 s; the sheet to Mini; the ring a Painter2D arc; the walk the mascot's
   screen position tweened ----- */
function gdFindPoint(){var fd=S.tourFind;if(!fd)return null;var W=(typeof curWall==="function"?curWall().pois:S.wallPois)||[];return W.filter(function(p){return p.id===fd.point})[0]||null}
function gdFindR(){return Math.round(ph.offsetWidth/6)}
function gdFindC(){return{x:ph.offsetWidth/2,y:ph.offsetHeight/2}}
function gdFind(B){var it=B.item,fd=S.tourFind,p=gdFindPoint();if(!fd||!p){gdBeatFinish("no point to find on this wall","");return}
  var thing=L(fd.thing)||L(p.name);B.eye="AZU · "+TXT("tour_find_eye").toUpperCase();B.said=gdLine(it,false,{aud:"",thing:thing});B.thing=thing;gdBeatDraw(B);
  gdBeatBody('<div class="bt-chips">'+gdBeatChip("giveup",TXT("tour_find_giveup"),"",0)+gdBeatChip("notnow",TXT("tour_notnow"),"",4)+'</div>',"");
  B.notNowT=setTimeout(function(){var n=ph.querySelector('.gdbeat [data-gdbt="notnow"]');if(n&&G.gd.bt===B){n.classList.add("bt-gone");setTimeout(function(){if(n.isConnected)n.remove();gdBeatPlace()},300)}},4000);
  /* the card lowers: the wall's view comes back, the panel stays at Mini */
  S.stop="closed";S.view="card";renderPhone(false);if(S.dockH!=="mini"&&S.dockH!=="compact"){S.dockH="mini";renderGuide()}
  var F=G.gd.find={B:B,p:p,clock:0,tick:performance.now(),movedAt:performance.now(),hinted:false,state:"seek"};gdFindRing();gdLog("event","Find it on the wall: "+L(p.name));
  gdBeatSay(B.said);F.iv=setInterval(function(){gdFindLoop(F)},120)}
function gdFindRing(){var r=ph.querySelector(".gdseek");if(!r){r=document.createElement("div");r.className="gdseek"+(S.reduce?" still":"");r.setAttribute("aria-hidden","true");r.innerHTML="<i></i>";ph.appendChild(r)}
  var c=gdFindC(),R=gdFindR();r.style.left=(c.x-R)+"px";r.style.top=(c.y-R)+"px";r.style.width=r.style.height=2*R+"px"}
function gdFindXY(p){var s=poiScreen(p);return{x:s.x,y:s.y}}
function gdFindIn(p){var s=gdFindXY(p),c=gdFindC();return Math.hypot(s.x-c.x,s.y-c.y)<=gdFindR()}
function gdFindLoop(F){if(G.gd.find!==F){clearInterval(F.iv);return}var now=performance.now();if(!G.paused&&F.state==="seek")F.clock+=now-F.tick;F.tick=now;
  if(!G.on){gdFindEnd(F);return}if(F.state!=="seek")return;
  if(ph.querySelector(".stv.st-lost")){gdFindLost(F);return}
  var inR=gdFindIn(F.p),ring=ph.querySelector(".gdseek");if(ring)ring.classList.toggle("near",inR);
  if(inR&&now-F.movedAt>=1200&&!G.paused){gdFindFound(F);return}
  var it=F.B.item;if(!F.hinted&&F.clock>=(+it.hintAfter||10)*1000){F.hinted=true;gdFindHint(F)}
  if(F.clock>=(+it.giveUp||20)*1000){gdFindGiveUp(F,"after "+(+it.giveUp||20)+" s")}}
/* the hint: where the point is from the middle of the screen (the larger way first) */
function gdFindDir(p){var s=gdFindXY(p),c=gdFindC(),dx=s.x-c.x,dy=s.y-c.y;return Math.abs(dx)>=Math.abs(dy)?(dx<0?"left":"right"):(dy<0?"up":"down")}
function gdFindHint(F){var dir=gdFindDir(F.p);F.dir=dir;var B=F.B;gdLog("event","Find it on the wall: the hint (“"+TXT("tour_find_"+dir)+"”)");if(G.gd.bt===B)gdBeatSay(TXT("tour_find_"+dir))}
/* the point's glint; a point off the screen: at the screen's edge on its side, an arrow towards it (the phone is not moved for the visitor) */
function gdFindSpot(p){var s=gdFindXY(p),W=ph.offsetWidth,H=ph.offsetHeight,c={x:Math.min(Math.max(s.x,40),W-40),y:Math.min(Math.max(s.y,110),H-300)};c.edge=Math.hypot(c.x-s.x,c.y-s.y)>4?gdFindDir(p):"";return c}
function gdFindGlint(p){var g=ph.querySelector(".gdfound");if(!g){g=document.createElement("div");g.setAttribute("aria-hidden","true");ph.appendChild(g)}var s=gdFindSpot(p);g.className="gdfound"+(S.reduce?" still":"")+(s.edge?" edge e-"+s.edge:"");g.style.left=(s.x-34)+"px";g.style.top=(s.y-34)+"px";return s}
function gdFindFound(F){F.state="found";var B=F.B;haptic();gdFindGlint(F.p);var r=ph.querySelector(".gdseek");if(r)r.classList.add("got");azReact("r-jump",900,"big");gdLog("event","Find it on the wall: found (settled in the ring 1.2 s, after "+(F.clock/1000).toFixed(1)+" s)");
  gdBeatBody("","");gdBeatSay(TXT("tour_find_found"),function(){B.stepT=setTimeout(function(){gdFindBack(F,"found","acted")},900)})}
function gdFindGiveUp(F,why){if(F.state!=="seek")return;F.state="shown";var B=F.B,m=ph.querySelector(".mascot");clearTimeout(B.notNowT);gdBeatBody("","");var s=gdFindGlint(F.p);
  if(m&&S.guideCfg.mascot!==false){m.classList.remove("gd-onbeat");m.classList.add("gd-findto");var mw=m.offsetWidth||56,mh=m.offsetHeight||64,W=ph.offsetWidth,cx=s.x-30;if(cx-mw/2<8)cx=s.x+30;var lx=Math.min(Math.max(cx-mw/2,8),W-8-mw);m.style.right=Math.round(W-lx-mw)+"px";m.style.top=Math.round(s.y-mh-14)+"px"}
  /* round 15 (inside the phone): at the left edge Azu stood 18 px past it (its centre 30 px left of a spot held 40 px in); it takes the spot's other side there, and is kept 8 px inside. Unity: the same clamp on the walk's end */
  gdLog("event","Find it on the wall: I give up ("+why+"): Azu shows it");var t=nameIn(TXT(s.edge?"tour_find_edge_"+s.edge:"tour_find_show"),"{thing}",B.thing||"");
  setTimeout(function(){if(G.gd.find!==F)return;gdBeatSay(t,function(){B.stepT=setTimeout(function(){gdFindBack(F,"shown ("+why+")",why==="I give up"?"dismissed":"ignored")},1100)})},S.reduce?0:900)}
function gdFindLost(F){F.state="lost";var B=F.B;gdLog("event","Find it on the wall: the wall is lost (a system state wins): it ends");gdBeatBody("","");gdBeatSay(TXT("tour_find_lost"),function(){gdFindBack(F,"the wall was lost","")})}
function gdFindEnd(F){clearInterval(F.iv);if(G.gd.find===F)G.gd.find=null;ph.querySelectorAll(".gdseek,.gdfound").forEach(function(x){x.remove()});var m=ph.querySelector(".mascot");if(m){m.classList.remove("gd-findto");m.style.right=""}}
/* back to the card: the sheet up, the tour goes on */
function gdFindBack(F,why,how){gdFindEnd(F);S.stop="full";S.view="card";renderPhone(false);gdBeatFinish("Find it on the wall: "+why,how)}
ph.addEventListener("click",function(e){var t=e.target.closest(".gdbeat [data-gdbt]");if(!t)return;var B=G.gd.bt,F=G.gd.find;if(!B||B.kind!=="find"||!F)return;e.stopPropagation();e.preventDefault();var a=t.dataset.gdbt;
  if(a==="giveup")gdFindGiveUp(F,"I give up");else if(a==="notnow"&&F.state==="seek"){clearTimeout(B.notNowT);F.state="no";gdFindEnd(F);var tx=gdNotNow(B.item);S.stop="full";renderPhone(false);if(tx){gdBeatBody("","");gdBeatSay(tx,function(){gdBeatFinish("Not now: no more games this visit","dismissed")})}else gdBeatFinish("Not now","dismissed")}},true);
on("stopTour.before",function(){var F=G.gd.find;if(F)gdFindEnd(F)});
on("renderPhone.after",function(){if(G.gd.find){gdFindRing();var F=G.gd.find;if(F.state==="found"||F.state==="shown")gdFindGlint(F.p)}});
on("camPaint.after",function(){var F=G.gd.find;if(F&&(F.state==="found"||F.state==="shown"))gdFindGlint(F.p)});
