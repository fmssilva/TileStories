/* The card's page openings (guidance round 22, 22.3; the developer, 2026-11-13: "then the page opening with the normal effects and
   animations (a zoom from the whole screen to the block's preview position, or others)"). Before this round a tap on a block's preview
   (the Story, the Gallery, a page row) swapped the screen at once: there was no opening. Built once, for the visitor's tap and the
   tour's alike (the tour taps the same preview, guide/demo-hand.js): the page zooms out of the preview's rectangle to the whole screen
   (its top left on the preview's, one scale from the preview's width to the screen's, clipped to the preview's height and corners, a
   quick fade in over its first quarter; 460 ms, cubic-bezier(.2,.8,.2,1)), the card staying still under it. Back (the page's ‹, its
   Previous / Next to the card) plays the closing: the card where the visitor left it (its scroll kept, the old behaviour went back to
   the top) and the page shrinking into the same preview, fading as it lands (380 ms); a preview no longer on the screen: the page
   shrinks a little and fades. Reduce Motion: a crossfade (200 ms), no zoom. PO.log keeps each one's rectangles for the checks.
   Unity: the page's root VisualElement (or a RectTransform) tweened from the preview's worldBound (translate and scale, DOTween,
   the same curve), the card's ScrollView kept under it; the same tap's handler opens it, the back button's closes it. */
var PO={rec:null,last:null,log:[]};
var PO_MS=460,PO_BACK_MS=380;
function poRect(el){var p=ph.getBoundingClientRect(),q=p.width/ph.offsetWidth||1,r=el.getBoundingClientRect(),s=scr.getBoundingClientRect(),t=Math.max(r.top,s.top),b=Math.min(r.bottom,s.bottom);
  return{l:(r.left-p.left)/q,t:(t-p.top)/q,w:r.width/q,h:Math.max(0,b-t)/q,seen:b-t>24}}
/* a tap: noted before any handler (the window's capture), used by the redraw it causes */
window.addEventListener("click",function(e){var t=e.target;if(!t||!t.closest||!ph.contains(t)||t.closest(".kq"))return;var g=t.closest("[data-go]");if(!g||!scr.contains(g))return;var to=g.dataset.go;
  if(S.view==="card"&&/^(story|gallery|kpage)$/.test(to)){var it=g.closest('.item[data-ref]')||g;PO.rec={dir:"open",to:to,from:poRect(it),ref:it.dataset.ref||null,top:scr.scrollTop,rel:it.getBoundingClientRect().top-scr.getBoundingClientRect().top}}
  else if(S.view!=="card"&&to==="card"&&PO.last&&PO.last.view===S.view)PO.rec={dir:"close",to:"card",ref:PO.last.ref,top:PO.last.top,rel:PO.last.rel};
  /* a page reached another way (the tour's jump to a step): back lands on the card at this page's preview */
  else if(/^(story|gallery)$/.test(S.view)&&to==="card"){var pv=poPreview(S.view);PO.rec={dir:"close",to:"card",ref:pv,top:null,rel:pv?90:null}}
  else return;setTimeout(function(){PO.rec=null},0)},true);
function poPreview(view){var b=S.card.filter(function(x){return x.kind===(view==="story"?"Story":"Gallery")})[0];return b?"b:"+b.id:null}
function poClone(){var c=scr.cloneNode(true);c.removeAttribute("id");c.classList.add("po-ghost");c.querySelectorAll("[id]").forEach(function(x){x.removeAttribute("id")});c.setAttribute("aria-hidden","true");c.style.pointerEvents="none";return c}
on("renderPhone.before",function(keep,cx){var r=PO.rec;if(!r||keep)return;var want=r.dir==="open"?r.to:"card";if(S.view!==want&&!(want==="kpage"&&S.view==="kpage"))return;
  PO.rec=null;cx.po={rec:r,ghost:poClone(),top:scr.scrollTop}});
on("renderPhone.after",function(keep,cx){var o=cx.po;if(!o)return;var r=o.rec,g=o.ghost,an=!!scr.animate,still=!!S.reduce;
  PO.anims&&PO.anims.forEach(function(a){try{a.finish()}catch(x){}});PO.anims=[];ph.querySelectorAll(".po-ghost").forEach(function(x){x.remove()});
  var F={l:scr.offsetLeft,t:scr.offsetTop,w:scr.offsetWidth,h:scr.offsetHeight},ent={dir:r.dir,at:performance.now(),ref:r.ref,full:[F.l,F.t,F.w,F.h].map(Math.round)};PO.log.push(ent);
  if(r.dir==="open"){PO.last={view:S.view,ref:r.ref,top:r.top,rel:r.rel};
    scr.parentNode.insertBefore(g,scr);g.scrollTop=o.top;var R=r.from;ent.from=[R.l,R.t,R.w,R.h].map(Math.round);
    if(!an){g.remove();return}
    var done=function(){g.remove()};
    if(still||!R||R.w<40){var a0=scr.animate([{opacity:0},{opacity:1}],{duration:200,easing:"ease-out"});a0.onfinish=done;PO.anims.push(a0);ent.fade=true;return}
    var s=R.w/F.w,x=R.l-F.l,y=R.t-F.t,clipB=Math.max(0,F.h-R.h/s),rad=(14/s).toFixed(1);
    scr.style.transformOrigin="0 0";
    var a=scr.animate([{transform:"translate("+x+"px,"+y+"px) scale("+s+")",clipPath:"inset(0px 0px "+clipB.toFixed(1)+"px 0px round "+rad+"px)",opacity:0},
      {transform:"translate("+(x*.7)+"px,"+(y*.7)+"px) scale("+(s+(1-s)*.3)+")",opacity:1,offset:.25},
      {transform:"translate(0px,0px) scale(1)",clipPath:"inset(0px 0px 0px 0px round 0px)",opacity:1}],{duration:PO_MS,easing:"cubic-bezier(.2,.8,.2,1)"});
    a.onfinish=function(){scr.style.transformOrigin="";done()};PO.anims.push(a);return}
  /* the closing: the card where the visitor left it, the page shrinking into its preview */
  PO.last=null;if(r.top!=null)scr.scrollTop=r.top;else{var pi=r.ref&&scr.querySelector('[data-ref="'+r.ref+'"]');if(pi)scr.scrollTop=Math.max(0,pi.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-r.rel)}poKeep(r);
  scr.parentNode.insertBefore(g,scr.nextSibling);g.scrollTop=o.top;g.style.zIndex="5";
  if(!an){g.remove();return}
  var it=r.ref&&scr.querySelector('[data-ref="'+r.ref+'"]'),T=it?poRect(it):null;ent.to=T?[T.l,T.t,T.w,T.h].map(Math.round):null;
  var end=function(){g.remove()};
  if(still){var b0=g.animate([{opacity:1},{opacity:0}],{duration:200,easing:"ease-in",fill:"forwards"});b0.onfinish=end;PO.anims.push(b0);ent.fade=true;return}
  var k,fr;g.style.transformOrigin="0 0";
  if(T&&T.seen&&T.w>40){k=T.w/F.w;fr=[{transform:"translate(0px,0px) scale(1)",clipPath:"inset(0px 0px 0px 0px round 0px)",opacity:1},
      {opacity:1,offset:.6},{transform:"translate("+(T.l-F.l)+"px,"+(T.t-F.t)+"px) scale("+k+")",clipPath:"inset(0px 0px "+Math.max(0,F.h-T.h/k).toFixed(1)+"px 0px round "+(14/k).toFixed(1)+"px)",opacity:0}]}
  else{g.style.transformOrigin="50% 40%";fr=[{transform:"scale(1)",opacity:1},{transform:"scale(.92)",opacity:0}]}
  var b=g.animate(fr,{duration:PO_BACK_MS,easing:"cubic-bezier(.3,0,.2,1)",fill:"forwards"});b.onfinish=end;PO.anims.push(b)});
/* the card where the visitor left it: its preview back at the same height on the screen, held while the card settles (its pictures,
   its parts' heights) for 0.6 s, until the visitor scrolls */
function poKeep(r){var it=r.ref&&scr.querySelector('[data-ref="'+r.ref+'"]');if(!it||r.rel==null)return;var t0=performance.now(),moved=false,
    stop=function(e){if(e.isTrusted)moved=true},put=function(){if(moved||!it.isConnected||S.view!=="card")return;var d=it.getBoundingClientRect().top-scr.getBoundingClientRect().top-r.rel;if(Math.abs(d)>.5)scr.scrollTop+=d;if(performance.now()-t0<600)requestAnimationFrame(put);else{scr.removeEventListener("wheel",stop,true);scr.removeEventListener("touchstart",stop,true)}};
  scr.addEventListener("wheel",stop,true);scr.addEventListener("touchstart",stop,true);put()}
/* the page's own way back keeps its preview: any other way to the card (a redraw, the tour's jump) forgets it */
on("renderPhone.after",function(keep){if(S.view==="card"&&PO.last&&!keep)PO.last=null});
