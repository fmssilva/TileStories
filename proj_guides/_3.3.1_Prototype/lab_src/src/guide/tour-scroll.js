/* The tour's natural scroll (guidance round 22, 22.1; the developer, 2026-11-13: "the scroll should be slower and more natural, with
   some scrolling effect to show we are moving down or up the page (a grey see-through element pulsing while we scroll?)").
   Every scroll the tour makes (guide/tour.js's runStep, guide/person.js's placing) goes through tourScroll(top): a visitor's flick,
   eased in and out (a cubic in-out: slow start, the speed at its height half way, a soft landing), its time by the distance
   (450 ms + 0.75 ms a px, between 500 and 1200 ms: 100 px 525 ms, a screen 1080 ms, more 1200 ms), never a jump. A second call while a
   scroll is young (its first 40 %), in the same direction, only moves its end (the placing 60 ms after the step's own scroll): one
   motion, not two. While it moves, a quiet cue at the screen's edge says which way: a soft grey see-through pill with a chevron,
   pulsing near the bottom of the free screen moving down, near the top moving up, gone when the scroll lands (.gscue). The visitor's
   own wheel, touch or press stops it where it is (theirs wins). The Scroll row (2 · The tour › The tour's movement): Natural (this) or
   Quick (the browser's own smooth scroll, round 21 and before, no cue). Reduce Motion: a short fade-scroll (the card fades out 140 ms,
   jumps, fades back) and the cue shown still. Long distances start with the visitor's hand's swipe (guide/demo-hand.js joins
   tourScroll.start). Unity: ScrollView.scrollOffset tweened by DOTween (Ease.InOutCubic, the same time by distance), the cue a small
   Image faded in and out (its pulse a looping alpha and translate tween); the visitor's PointerDownEvent kills the tween. */
S.guideCfg.scroll="natural";
var TS={tw:null,raf:0,log:null};
function tsMs(d){return Math.round(Math.max(500,Math.min(1200,450+Math.abs(d)*0.75)))}
function tsEase(u){return u<.5?4*u*u*u:1-Math.pow(-2*u+2,3)/2}
function tourScroll(top,done){var max=Math.max(0,scr.scrollHeight-scr.clientHeight);top=Math.max(0,Math.min(max,top));var from=scr.scrollTop,d=top-from,now=performance.now();
  if(S.guideCfg.scroll==="quick"){tsStop();scr.scrollTo({top:top,behavior:S.reduce?"auto":"smooth"});if(done)setTimeout(done,S.reduce?0:450);return}
  if(Math.abs(d)<2){if(!TS.tw){if(done)done();return}}
  var tw=TS.tw;
  /* the same scroll, young, the same way: only its end moves */
  if(tw&&Math.sign(tw.to-tw.from)===Math.sign(top-tw.from)&&(now-tw.t0)<tw.ms*.4&&!S.reduce){tw.to=top;tw.ms=tsMs(top-tw.from);if(done)tw.done.push(done);G.autoScrollTill=tw.t0+tw.ms+120;tsCue(top>tw.from?"down":"up");return}
  tsStop();if(Math.abs(d)<2){if(done)done();return}
  var dir=d>0?"down":"up";emit("tourScroll.start",{dir:dir,dist:Math.abs(d),from:from,to:top});
  if(S.reduce){G.autoScrollTill=now+420;tsCue(dir);scr.style.transition="opacity .14s";scr.style.opacity="0";
    TS.tw={from:from,to:top,t0:now,ms:300,done:done?[done]:[],fade:true};
    TS.tw.t=setTimeout(function(){scr.scrollTop=top;scr.style.opacity="";setTimeout(function(){if(TS.tw&&TS.tw.fade){var w=TS.tw;TS.tw=null;scr.style.transition="";tsCueOff();w.done.forEach(function(f){f()})}},160)},140);return}
  TS.tw={from:from,to:top,t0:now,ms:tsMs(d),done:done?[done]:[]};G.autoScrollTill=now+TS.tw.ms+120;tsCue(dir);TS.raf=requestAnimationFrame(tsTick)}
function tsTick(){TS.raf=0;var tw=TS.tw;if(!tw||tw.fade)return;var u=Math.min(1,(performance.now()-tw.t0)/tw.ms),y=tw.from+(tw.to-tw.from)*tsEase(u);scr.scrollTop=y;
  if(TS.log)TS.log.push([Math.round(performance.now()),Math.round(scr.scrollTop*10)/10]);
  if(u<1){TS.raf=requestAnimationFrame(tsTick);return}TS.tw=null;tsCueOff();tw.done.forEach(function(f){f()})}
/* stops where it is (no callback: a visitor's touch, a new page) */
function tsStop(){var tw=TS.tw;TS.tw=null;if(TS.raf)cancelAnimationFrame(TS.raf);TS.raf=0;if(tw&&tw.fade){clearTimeout(tw.t);scr.style.opacity="";scr.style.transition=""}tsCueOff()}
function tsBusy(){return!!TS.tw}
/* the cue: a pill near the free screen's bottom (down) or top (up), its chevron pointing the way */
function tsCue(dir){var c=ph.querySelector(".gscue");if(!c){c=document.createElement("div");c.className="gscue";c.setAttribute("aria-hidden","true");
    c.innerHTML='<svg viewBox="0 0 24 14" width="20" height="12"><path d="M3 3l9 8 9-8" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"/></svg>';ph.appendChild(c)}
  var p=ph.getBoundingClientRect(),q=p.width/ph.offsetWidth||1,f=typeof freeHeight==="function"&&G.on?freeHeight():{top:scr.getBoundingClientRect().top,bottom:scr.getBoundingClientRect().bottom},
    top=Math.max(56,(f.top-p.top)/q+60),bot=Math.min(ph.offsetHeight-8,(f.bottom-p.top)/q)-12;
  c.dataset.dir=dir;c.classList.toggle("up",dir==="up");c.classList.toggle("still",!!S.reduce);c.style.top=Math.round(dir==="down"?bot-26:top)+"px";clearTimeout(c._out);c.classList.remove("out");void c.offsetWidth;c.classList.add("on")}
function tsCueOff(){var c=ph.querySelector(".gscue");if(!c)return;c.classList.remove("on");c.classList.add("out");clearTimeout(c._out);c._out=setTimeout(function(){if(c.classList.contains("out"))c.remove()},320)}
/* the visitor's own wheel, touch or press on the card stops it where it is */
["wheel","touchstart","pointerdown"].forEach(function(ev){scr.addEventListener(ev,function(e){if(e.isTrusted&&TS.tw)tsStop()},{capture:true,passive:true})});
on("renderPhone.before",function(keep){if(!keep)tsStop()});
on("stopTour.before",function(){tsStop()});

/* Editor: 2 · The tour › The tour's movement › The scroll */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;if(!d.querySelector('[data-fo="s7move"]'))return h;
  addRows(d,"s7move",row("The scroll",pseg("guideCfg.scroll",["natural","quick"],["Natural","Quick"]),"How the tour scrolls to the block it tells. Natural: like a visitor's flick, eased in and out, 0.5-1.2 s by the distance, with a soft grey pill pulsing at the screen's edge for the way it moves (down: near the bottom; up: near the top), gone when it lands. Quick: the browser's own smooth scroll, no pill. Reduce Motion: a short fade instead of the glide, the pill still."));
  return d.innerHTML});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(scroll|"));
/* its demo: the tour goes on from the facts to the next block, the scroll and its pill seen */
rowDemos({"guideCfg.scroll":{tour:"g1",act:function(){stopSpeech();G.paused=false;next()},ringAfter:350,ring:".gscue|.gfocus"}});
