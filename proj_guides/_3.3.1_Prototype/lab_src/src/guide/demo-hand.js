/* The visitor's hand (guidance round 22, 22.2-22.4; the developer, 2026-11-13: "I want the guide to replicate all the movements and
   motion the visitor would need to do by hand to take that tour ... a little see-through hand moving like a comet from some 2 cm away
   towards the place to be clicked, then a click effect (a grey see-through circle), and then the page opening with the normal effects
   ... so every tour is a demo of how to use the app. The same with the interactive elements").
   One module for every action a visitor would do by hand during a tour: dhTap(el), dhDrag(el, points), the turn of a 3D model, the
   swipe of a strip (and of the page, at the start of a long scroll). The hand: the guidance's ghost finger drawn smaller (one drawing
   for the app's hand, guidance/present.js's GD_FINGER: 30 x 39 px, white at 72 % with a soft shadow and a dark edge, so it reads on
   paper and on pictures). It glides in on an eased curve (a slight arc, ease-out: quick, then slowing like a comet; 620 ms) from
   DH_FROM = 88 px away with a short comet trail behind it, presses (a scale to 0.86, 120 ms), and the press leaves a soft grey ripple (a
   ring growing to 44 px and fading in 420 ms); then, as a finger lifts (DH_UP, 140 ms later, so the press is seen on the preview before its
   page opens), the real action: the same event a visitor's tap fires on the same element (a click, or pointer events for a drag), never a shortcut past the app's own animation; then it lifts and fades. 88 px and why: 2 cm on a
   phone 390 pt wide (163 pt to the inch) is about 128 pt, a third of the screen: a glide that long crosses other blocks and words; 88 px
   (about 1.4 cm) still reads as a hand coming from a little way off, and the trail carries the eye further. It comes from the side that
   covers none of the words being told (the teleprompter, the panel, the captions, a bubble, the block's lit words), below and to the
   right first (a right thumb), turned to point where it goes. A visitor's touch on the card takes the hand away and pauses the tour
   there (the visitor's hand wins); a touch on the guide's panel only hides it (the action still happens).
   Reduce Motion: no glide, no trail: the ripple and the action. The visitor's hand Off: neither, the action alone.
   The tour's moves (22.3, runStep.travel): a step in another view than the one shown is reached as a visitor would: the card scrolled
   (the natural scroll) to the story's preview, the hand taps it, the app's own opening plays (card/page-open.js), then the step runs;
   back to the card, the hand taps the page's back (‹), the closing plays. Only between two steps that follow each other (Next, the
   tour going on, Previous); a jump to a step (a row's demo, Continue) is instant as before.
   Blocks in use (22.4, Show blocks in use): when a step tells an interactive block, once its scroll has landed, the hand shows it in use,
   once per block per tour, never while the visitor touches the screen: drags the Before / After across and back, turns the 3D model (and
   a panorama) a little and back, flips the flip card and back, swipes the gallery's strip once and back; each through the block's own
   handlers (the hand's events never count as the visitor trying it: G.handAct). Not the quiz (its answer is the visitor's).
   Unity: the hand a UI Toolkit Image (or a RectTransform) tweened along a curve by DOTween (Ease.OutCubic, a quadratic path), its trail
   a TrailRenderer or a stretched gradient Image, the ripple a ring Image scaled and faded; the action sent through the same UI event
   the visitor fires (ExecuteEvents.pointerClickHandler / a ClickEvent on the same element, drag events for the slider and the orbit). */
S.guideCfg.hand="shown";S.guideCfg.inUse=true;
var DH={job:null,used:{},log:[],touching:false,useLog:[]};
var DH_FROM=88,DH_GLIDE=620,DH_W=30,DH_H=39,DH_TX=10.5,DH_TY=3,DH_PID=41;
function dhShown(){return S.guideCfg.hand!=="off"}
function dhQ(){var p=ph.getBoundingClientRect();return{p:p,q:p.width/ph.offsetWidth||1}}
function dhIn(x,y){var o=dhQ();return{x:(x-o.p.left)/o.q,y:(y-o.p.top)/o.q}}
function dhOut(x,y){var o=dhQ();return{x:o.p.left+x*o.q,y:o.p.top+y*o.q}}
function dhRectIn(el){var o=dhQ(),r=el.getBoundingClientRect();return{l:(r.left-o.p.left)/o.q,t:(r.top-o.p.top)/o.q,r:(r.right-o.p.left)/o.q,b:(r.bottom-o.p.top)/o.q}}
/* the hand's box with its tip at (x, y), turned deg about the tip */
function dhBox(x,y,deg){var a=deg*Math.PI/180,c=Math.cos(a),s=Math.sin(a),xs=[],ys=[];[[-DH_TX,-DH_TY],[DH_W-DH_TX,-DH_TY],[DH_W-DH_TX,DH_H-DH_TY],[-DH_TX,DH_H-DH_TY]].forEach(function(k){xs.push(x+k[0]*c-k[1]*s);ys.push(y+k[0]*s+k[1]*c)});
  return{l:Math.min.apply(null,xs),t:Math.min.apply(null,ys),r:Math.max.apply(null,xs),b:Math.max.apply(null,ys)}}
function dhOver(a,b){return Math.max(0,Math.min(a.r,b.r)-Math.max(a.l,b.l))*Math.max(0,Math.min(a.b,b.b)-Math.max(a.t,b.t))}
/* the words being told: the teleprompter, the panel, the captions, a bubble, a game's card, and the block's own lit words */
function dhWords(){var out=[];[".gtele:not(.tl-out)",".gdock",".gcap",".gbubble",".gdbub:not(.gd-out)",".gdbeat:not(.bt-away)",".gbar",".tepanel"].forEach(function(s){ph.querySelectorAll(s).forEach(function(e){var r=e.getBoundingClientRect();if(r.width>1&&r.height>1)out.push(dhRectIn(e))})});
  var t=G.hl&&G.hl.target;if(t&&t.isConnected){var r=dhRectIn(t);if(r.r-r.l>1)out.push(r)}return out}
/* where the glide starts: DH_FROM away from the tip, from the side that covers no words (below right first), turned towards the target */
var DH_ANGLES=[55,125,25,155,90,-35,-145,0,180,-90];
function dhApproach(x,y){var W=ph.offsetWidth,H=ph.offsetHeight,avoid=dhWords(),best=null;
  DH_ANGLES.forEach(function(an,k){var a=an*Math.PI/180,sx=x+Math.cos(a)*DH_FROM,sy=y+Math.sin(a)*DH_FROM,raw=Math.atan2(y-sy,x-sx)*180/Math.PI+90;if(raw>180)raw-=360;
    var deg=Math.max(-45,Math.min(45,raw*.6)),c=0;[0,.5,1].forEach(function(u){var b=dhBox(sx+(x-sx)*u,sy+(y-sy)*u,deg);avoid.forEach(function(w){c+=dhOver(b,w)});c+=(Math.max(0,4-b.l)+Math.max(0,b.r-(W-4))+Math.max(0,4-b.t)+Math.max(0,b.b-(H-4)))*40});
    c+=k*.5;if(!best||c<best.c)best={c:c,x:sx,y:sy,deg:deg,an:an}});return best}
/* ----- the sprite, its trail and the ripple ----- */
function dhSprite(){var h=ph.querySelector(".dhand");if(!h){h=document.createElement("div");h.className="dhand";h.setAttribute("aria-hidden","true");h.innerHTML=typeof GD_FINGER==="string"?GD_FINGER:"";ph.appendChild(h)}return h}
function dhPut(j,x,y,deg,sc,op){j.at={x:x,y:y};if(j.hidden||!j.shown)return;var h=dhSprite();h.style.translate=(x-DH_TX).toFixed(1)+"px "+(y-DH_TY).toFixed(1)+"px";h.style.rotate=deg.toFixed(1)+"deg";h.style.scale=String(sc);h.style.opacity=String(op);
  h.dataset.tip=Math.round(x)+","+Math.round(y)}
function dhTrail(j,pts){if(j.hidden||!j.shown||S.reduce)return;var t=ph.querySelector(".dhtrail");if(!t){t=document.createElement("div");t.className="dhtrail";t.setAttribute("aria-hidden","true");ph.appendChild(t)}
  if(pts.length<2){t.style.opacity="0";return}var a=pts[0],b=pts[pts.length-1],L=Math.hypot(b.x-a.x,b.y-a.y);if(L<3){t.style.opacity="0";return}
  t.style.width=L.toFixed(1)+"px";t.style.translate=a.x.toFixed(1)+"px "+(a.y-3).toFixed(1)+"px";t.style.rotate=(Math.atan2(b.y-a.y,b.x-a.x)*180/Math.PI).toFixed(1)+"deg";t.style.opacity=String(Math.min(1,L/30))}
function dhSpriteOff(now){ph.querySelectorAll(".dhand,.dhtrail").forEach(function(e){if(now)e.remove();else{e.classList.add("dh-out");setTimeout(function(){e.remove()},260)}})}
function dhRipple(x,y){var r=document.createElement("i");r.className="dhripple";r.setAttribute("aria-hidden","true");r.style.left=Math.round(x)+"px";r.style.top=Math.round(y)+"px";ph.appendChild(r);
  DH.log.push({ripple:[Math.round(x),Math.round(y)],at:performance.now()});setTimeout(function(){r.remove()},460);return r}
/* ----- a job: one action of the hand at a time; a visitor's touch ends it ----- */
function dhKill(){var j=DH.job;DH.job=null;if(j){j.dead=true;j.t.forEach(clearTimeout);if(j.raf)cancelAnimationFrame(j.raf);if(j.down)dhFire("pointerup",j.down.el,j.down.x,j.down.y)}G.handAct=false;dhSpriteOff(true)}
function dhJob(kind){dhKill();var j={kind:kind,dead:false,t:[],shown:dhShown()&&!S.reduce,at:null};DH.job=j;return j}
function dhWait(j,ms,f){j.t.push(setTimeout(function(){if(!j.dead)f()},ms))}
function dhAnim(j,ms,frame,end){var t0=performance.now();(function tick(){if(j.dead)return;var u=ms>0?Math.min(1,(performance.now()-t0)/ms):1;frame(u);if(u<1)j.raf=requestAnimationFrame(tick);else if(end)end()})()}
function dhEnd(j){if(DH.job===j)DH.job=null;G.handAct=false}
function dhOutCubic(u){return 1-Math.pow(1-u,3)}
/* the visitor's event, as a finger fires it (pointer events for a drag; a click for a tap), on the same element */
function dhFire(type,el,x,y){if(!el||!el.isConnected)return;var c=dhOut(x,y),o={bubbles:true,cancelable:true,composed:true,clientX:c.x,clientY:c.y,view:window,button:0,buttons:type==="pointerup"||type==="click"?0:1};
  var ev=type==="click"?new MouseEvent("click",o):new PointerEvent(type,Object.assign(o,{pointerId:DH_PID,pointerType:"touch",isPrimary:true}));var was=G.handAct;G.handAct=true;try{el.dispatchEvent(ev)}finally{G.handAct=was}}
/* the glide to the tip (x, y) in the phone's px: from DH_FROM away on a slight arc, ease-out, the trail behind it */
function dhGlide(j,x,y,done){if(!j.shown){j.at={x:x,y:y};done();return}var s=dhApproach(x,y),mx=(s.x+x)/2,my=(s.y+y)/2,nx=-(y-s.y)/DH_FROM,ny=(x-s.x)/DH_FROM,cx=mx+nx*14,cy=my+ny*14,hist=[];
  j.from={x:s.x,y:s.y,deg:s.deg};DH.log.push({glide:[Math.round(s.x),Math.round(s.y)],to:[Math.round(x),Math.round(y)],at:performance.now(),dist:Math.round(Math.hypot(x-s.x,y-s.y))});
  dhAnim(j,DH_GLIDE,function(u){var e=dhOutCubic(u),k=1-e,px=k*k*s.x+2*k*e*cx+e*e*x,py=k*k*s.y+2*k*e*cy+e*e*y,now=performance.now();dhPut(j,px,py,s.deg,1,Math.min(.72,u*3.6));
    hist.push({x:px,y:py,t:now});while(hist.length&&now-hist[0].t>90)hist.shift();dhTrail(j,hist)},function(){dhTrail(j,[]);done()})}
function dhPress(j,done){var a=j.at;if(!j.shown){done();return}dhAnim(j,120,function(u){dhPut(j,a.x,a.y,j.from?j.from.deg:0,1-.14*u,.72)},done)}
function dhLift(j,stay,done){var a=j.at,f=j.from||{x:a.x+30,y:a.y+40,deg:0},d=Math.hypot(f.x-a.x,f.y-a.y)||1,ux=(f.x-a.x)/d,uy=(f.y-a.y)/d;if(!j.shown){if(done)done();return}
  dhAnim(j,stay?160:260,function(u){dhPut(j,a.x+ux*(stay?10:12)*u,a.y+uy*(stay?10:12)*u,f.deg,.86+.14*u,stay?.72:.72*(1-u))},function(){j.at=a;if(!stay)dhSpriteOff(true);if(done)done()})}
/* a tap on el at the phone point pt: glide, press, the ripple and the visitor's click, lift. acted(): right after the click */
/* as a finger: the ripple on the press, the click on the release DH_UP ms later (so the press is seen on the preview before its page opens) */
var DH_UP=140;
function dhTapIn(j,el,pt,acted,stay){var shownR=dhShown(),act=function(then){var c=dhOut(pt.x,pt.y),u=document.elementFromPoint(c.x,c.y),t=u&&el.contains(u)?u:el;
    if(shownR)dhRipple(pt.x,pt.y);dhWait(j,shownR?DH_UP:0,function(){DH.log.push({tap:t.className||t.tagName,at:performance.now(),pt:[Math.round(pt.x),Math.round(pt.y)]});dhFire("click",t,pt.x,pt.y);if(acted)acted();if(then)then()})};
  if(!dhShown()){dhWait(j,200,function(){act()});return}
  if(S.reduce){act();return}
  dhGlide(j,pt.x,pt.y,function(){dhPress(j,function(){act(function(){dhWait(j,60,function(){dhLift(j,stay)})})})})}
/* a drag along pts (phone px) in ms: glide to the first, press, the pointer events along the way, release */
function dhDragIn(j,el,pts,ms,done){var go=function(){j.down={el:el,x:pts[0].x,y:pts[0].y};G.handAct=true;dhFire("pointerdown",el,pts[0].x,pts[0].y);var seg=[],tot=0;for(var i=1;i<pts.length;i++){var d=Math.hypot(pts[i].x-pts[i-1].x,pts[i].y-pts[i-1].y);seg.push(d);tot+=d}
    var at=function(u){var w=u*tot;for(var i=0;i<seg.length;i++){if(w<=seg[i]||i===seg.length-1){var f=seg[i]?Math.min(1,w/seg[i]):1;return{x:pts[i].x+(pts[i+1].x-pts[i].x)*f,y:pts[i].y+(pts[i+1].y-pts[i].y)*f}}w-=seg[i]}return pts[pts.length-1]};
    var fin=function(){var e=pts[pts.length-1];dhFire("pointermove",el,e.x,e.y);dhFire("pointerup",el,e.x,e.y);j.down=null;G.handAct=false;dhLift(j,false,done)};
    if(S.reduce){var k=1;(function stepTo(){if(j.dead)return;if(k>=pts.length){fin();return}j.down.x=pts[k].x;j.down.y=pts[k].y;dhFire("pointermove",el,pts[k].x,pts[k].y);k++;dhWait(j,450,stepTo)})();return}
    dhAnim(j,ms,function(u){var e=u<.5?2*u*u:1-Math.pow(-2*u+2,2)/2,p=at(e);j.down.x=p.x;j.down.y=p.y;dhPut(j,p.x,p.y,j.from?j.from.deg:0,.86,.72);dhFire("pointermove",el,p.x,p.y)},fin)};
  if(S.reduce||!dhShown()){if(dhShown())dhRipple(pts[0].x,pts[0].y);go();return}
  dhGlide(j,pts[0].x,pts[0].y,function(){dhPress(j,go)})}
/* ----- the visitor's own touch: theirs wins ----- */
window.addEventListener("pointerdown",function(e){if(!e.isTrusted||!ph.contains(e.target))return;DH.touching=true;var j=DH.job;if(!j)return;
  if(j.kind==="travel"&&!scr.contains(e.target)){j.hidden=true;dhSpriteOff(true);return}
  DH.log.push({visitor:j.kind,at:performance.now()});dhKill();if(j.kind==="travel"&&G.trav){G.trav=null;if(G.on&&!G.paused)pauseNow()}},true);
["pointerup","pointercancel"].forEach(function(ev){window.addEventListener(ev,function(e){if(e.isTrusted)DH.touching=false},true)});
window.addEventListener("wheel",function(e){if(e.isTrusted&&DH.job&&ph.contains(e.target)&&DH.job.kind!=="travel")dhKill()},{capture:true,passive:true});
/* ----- the tour's moves: a step in another view, reached as a visitor would (22.3) ----- */
function dhWay(st){if(S.view==="card"&&st.view==="story"){var g=scr.querySelector('.item[data-ref] [data-go="story"]');if(!g)return null;return{kind:"open story",el:g,item:g.closest(".item[data-ref]")}}
  if(S.view==="story"&&st.view==="card"){var b=scr.querySelector('.topbar [data-go="card"]');return b?{kind:"back to the card",el:b}:null}return null}
on("runStep.travel",function(st){if(!st||!G.on)return;if(G.trav&&G.trav.i===G.i&&G.trav.done)return;if(st.view===S.view)return;
  var sh=G.trShown;if(!sh||Math.abs(sh.i-G.i)!==1||sh.view!==S.view)return;var way=dhWay(st);if(!way)return;
  var tr=G.trav={i:G.i,done:false,way:way.kind,at:performance.now()};DH.log.push({travel:way.kind,i:G.i,at:tr.at});dhTravel(tr,way);return true});
function dhArrive(tr){if(G.trav!==tr)return;tr.done=true;if(DH.job&&DH.job.kind==="travel")dhEnd(DH.job);if(!G.on||G.i!==tr.i){G.trav=null;return}if(G.paused){G.travelHeld=true;return}runStep()}
/* the card's own top bar (its close and settings, the title band) over the scroll: the preview is put under it */
function dhTopClear(){return Math.max(64,Math.min(140,cardTopClear()+10))}
function dhTravel(tr,way){var j=dhJob("travel");clearGuideMarks();
  var tap=function(){if(j.dead||G.trav!==tr)return;if(!way.el.isConnected){tr.done=true;S.view=S.guideSteps[tr.i].view;renderPhone(false);dhArrive(tr);return}
    var r=dhRectIn(way.el),img=way.el.querySelector("img"),ri=img?dhRectIn(img):null,pt=ri&&ri.b-ri.t>40?{x:(ri.l+ri.r)/2,y:(ri.t+ri.b)/2}:{x:(r.l+r.r)/2,y:(r.t+r.b)/2};
    dhTapIn(j,way.el,pt,function(){dhWait(j,(way.kind==="open story"?PO_MS:PO_BACK_MS)+60,function(){dhArrive(tr)})})};
  if(way.item){var o=dhQ(),ir=way.item.getBoundingClientRect(),want=o.p.top+dhTopClear()*o.q,dy=(ir.top-want)/o.q;if(Math.abs(dy)>8){tourScroll(scr.scrollTop+dy,function(){dhWait(j,140,tap)});return}}
  dhWait(j,160,tap)}
on("runStep.after",function(){G.trShown=G.on?{i:G.i,view:S.view}:null;G.trav=null;G.travelHeld=false});
on("startTour.before",function(){G.trShown=null;G.trav=null;DH.used={};dhKill()});
on("stopTour.before",function(){G.trav=null;G.travelHeld=false;dhKill()});
/* a travel held by a pause goes on with Resume */
on("resumeTour.answer",function(){if(!G.travelHeld||!G.on)return;G.travelHeld=false;G.paused=false;G.askOpen=false;runStep();renderGuide();return true});
/* a long scroll starts with the hand's swipe up (or down) the screen, as a visitor's thumb would (no event: the scroll is the tour's) */
on("tourScroll.start",function(o){if(!G.on||!dhShown()||S.reduce||DH.job||o.dist<scr.clientHeight*.9)return;var j=dhJob("swipe the page"),W=ph.offsetWidth,f=freeHeight(),q=dhQ(),bot=(f.bottom-q.p.top)/q.q,top=(f.top-q.p.top)/q.q;
  var y0=o.dir==="down"?Math.max(top+180,bot-90):top+120,y1=o.dir==="down"?y0-150:y0+150,x=W*.66,hist=[];j.from={x:x,y:y0,deg:-12};DH.log.push({swipe:o.dir,at:performance.now()});
  dhAnim(j,520,function(u){var e=u<.5?2*u*u:1-Math.pow(-2*u+2,2)/2,y=y0+(y1-y0)*e,now=performance.now();dhPut(j,x,y,-12,u<.15?1-u:.86,Math.min(.62,u*4)*(u>.8?(1-u)*5:1));hist.push({x:x,y:y,t:now});while(hist.length&&now-hist[0].t>90)hist.shift();dhTrail(j,hist)},function(){dhSpriteOff(true);dhEnd(j)})});
/* ----- blocks in use (22.4) ----- */
function dhUseOf(el){if(!el)return null;var f=function(s){return el.matches(s)?el:el.querySelector(s)},e;
  if((e=f("[data-ba]")))return{kind:"ba",el:e};if((e=f("[data-flip]")))return{kind:"flip",el:e};if((e=f("[data-turnt]")))return{kind:"turn",el:e};if((e=f("[data-pano]")))return{kind:"turn",el:e};
  e=f(".el-car");if(e&&e.scrollWidth>e.clientWidth+20)return{kind:"swipe",el:e};return null}
function dhUse(w,ref){var j=dhJob("use"),el=w.el,r=dhRectIn(el),cy=(r.t+r.b)/2,cx=(r.l+r.r)/2,wd=r.r-r.l,rec={kind:w.kind,ref:ref,at:performance.now()};DH.useLog.push(rec);
  var fin=function(){rec.end=performance.now();dhEnd(j)};
  if(w.kind==="ba"){var x0=(parseFloat(el.style.getPropertyValue("--x"))||50)/100,y=r.t+(r.b-r.t)*.74;rec.x0=x0;
    dhDragIn(j,el,[{x:r.l+wd*x0,y:y},{x:r.l+wd*.84,y:y},{x:r.l+wd*.16,y:y},{x:r.l+wd*x0,y:y}],2100,fin);return}
  if(w.kind==="turn"){var y2=r.t+(r.b-r.t)*.62;dhDragIn(j,el,[{x:cx,y:y2},{x:cx+64,y:y2},{x:cx-48,y:y2},{x:cx,y:y2}],1700,fin);return}
  if(w.kind==="flip"){var pt={x:cx,y:r.t+(r.b-r.t)*.62};dhTapIn(j,el,pt,function(){dhWait(j,1500,function(){var again=function(){var c=dhOut(pt.x,pt.y);DH.log.push({tap:"flip back",at:performance.now()});if(dhShown())dhRipple(pt.x,pt.y);dhWait(j,dhShown()?DH_UP:0,function(){dhFire("click",el,pt.x,pt.y);dhWait(j,700,fin)})};
      if(j.shown&&j.at){dhPress(j,function(){again();dhWait(j,120,function(){dhLift(j,false)})})}else again()})},true);return}
  if(w.kind==="swipe"){var kids=el.children,step=kids.length>1?kids[1].offsetLeft-kids[0].offsetLeft:el.clientWidth*.8,l0=el.scrollLeft,snap=el.style.scrollSnapType,yy=cy+10;rec.l0=l0;
    var slide=function(to,x1,x2,done){el.style.scrollSnapType="none";var from=el.scrollLeft;G.handAct=true;
      var move=function(u){var e=u<.5?2*u*u:1-Math.pow(-2*u+2,2)/2;el.scrollLeft=from+(to-from)*e;if(j.shown)dhPut(j,x1+(x2-x1)*e,yy,j.from?j.from.deg:0,.86,.72)};
      if(S.reduce){el.scrollLeft=to;dhWait(j,60,function(){G.handAct=false;el.style.scrollSnapType=snap;done()});return}
      dhAnim(j,520,move,function(){dhWait(j,80,function(){G.handAct=false;el.style.scrollSnapType=snap;done()})})};
    var x1=r.l+wd*.78,x2=r.l+wd*.36,start=function(f){if(!j.shown){if(dhShown())dhRipple(x1,yy);f();return}dhGlide(j,x1,yy,function(){dhPress(j,f)})};
    start(function(){slide(l0+step,x1,x2,function(){dhLift(j,true,function(){dhWait(j,900,function(){if(j.shown){dhPut(j,x2,yy,j.from?j.from.deg:0,.86,.72)}else if(dhShown())dhRipple(x2,yy);slide(l0,x2,x1,function(){dhLift(j,false,fin)})})})})});return}}
on("runStep.after",function(){if(!G.on||!S.guideCfg.inUse)return;var st=S.guideSteps[G.i];if(!st||/^(teaser|end|intro)$/.test(st.act)||DH.used[st.ref])return;var my=G.i,n=0;
  var wait=function(){if(!G.on||G.i!==my||S.guideSteps[G.i]!==st)return;if(G.paused||G.askOpen||tsBusy()||performance.now()<(G.autoScrollTill||0)||DH.touching||DH.job){if(++n<40)setTimeout(wait,120);return}
    var w=dhUseOf(stepEl(st));if(!w)return;DH.used[st.ref]=true;dhUse(w,st.ref)};setTimeout(wait,380)});
on("pauseTour.answer",function(){if(DH.job&&DH.job.kind!=="travel")dhKill()});

/* ----- Editor: 2 · The tour › The tour's movement (after Focus) ----- */
COMP.s7move=["Guide","The tour's movement"];FOCUS.s7move=["card",'[data-ref="b:b_story"]',"full"];
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var fc=d.querySelector('[data-fo="s7focus"]');if(!fc)return h;
  fc.insertAdjacentHTML("afterend",comp("s7move","the tour moves as a visitor would: the hand, the blocks in use, the scroll",
    row("The visitor's hand",pseg("guideCfg.hand",["shown","off"],["Shown","Off"]),"Shown: when the tour taps something (a story's preview to open it, the page's ‹ to go back), a small see-through hand glides in from a little way off (88 px) with a short trail, presses, and a soft grey ripple marks the tap; then the app does what the visitor's own tap does (the page opening from the preview). It never covers the words being told, and goes away when the visitor touches the screen. Off: the same tap and opening, with no hand and no ripple. Reduce Motion: the ripple only.")+
    row("Show blocks in use",pseg("guideCfg.inUse",["true","false"],["On","Off"]),"On: when a step tells an interactive block, the hand shows it in use once, gently, after the scroll lands: it drags the before and after across and back, turns a 3D model a little, flips the flip card and back, swipes the gallery once and back. Once per block per tour, never while the visitor touches the screen; the quiz stays the visitor's. Off: the blocks wait for the visitor.")));
  return d.innerHTML});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(hand|inUse|"));
/* each row's demo: from the flip card to the story (the hand taps the story's preview, the page opens); the flip card in use */
rowDemos({"guideCfg.hand":{tour:"g2",act:function(){stopSpeech();G.paused=false;next()},ringAfter:900,ring:'.dhand|.dhripple|.topbar|[data-ref="b:b_story"]'},
  "guideCfg.inUse":{tour:"g2",ringAfter:1600,ring:'[data-ref="b:b_fun"]'}});
