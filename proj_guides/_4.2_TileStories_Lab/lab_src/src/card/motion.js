/* Motion: entrances, transitions and motion with the scroll; the scroll-driven zoom and pan of a picture. */
function entranceOf(style){var m=style.enter==="family"?S.family:style.enter;return m==="none"?null:motionById(m)}
function easeCss(e){return e==="overshoot"?"cubic-bezier(.3,1.45,.5,1)":e==="spring"?"cubic-bezier(.2,1.3,.4,1)":e==="inout"?"cubic-bezier(.65,0,.35,1)":"cubic-bezier(.2,.8,.2,1)"}
function motionVars(m,i){if(!m||m.type!=="entrance")return "";var dx=0,dy=0,sc=1;
  if(m.from==="below")dy=m.dist;else if(m.from==="left")dx=-m.dist;else if(m.from==="right")dx=m.dist;else if(m.from==="scale")sc=.84;
  return "--dx:"+dx+"px;--dy:"+dy+"px;--sc:"+sc+";--rt:"+(m.rot||0)+"deg;--dur:"+m.dur+"ms;--ease:"+easeCss(m.ease)+";--delay:"+((i||0)*m.stagger)+"ms";}
function wrapMv(html,m,i){if(!m||m.type!=="entrance")return html;
  if(m.pieces!=="whole")return '<div class="mvp" data-pieces="'+m.id+'">'+html+'</div>';
  return '<div class="mv pre'+(m.from==="wipe"?" wipe":"")+'" style="'+motionVars(m,i)+'">'+html+'</div>';}
function splitPieces(){scr.querySelectorAll(".mvp").forEach(function(w){var m=motionById(w.dataset.pieces);var n=0;
  w.querySelectorAll("h3,h4,p,q,div.el-eyebrow").forEach(function(el){if(el.children.length)return;
    var parts=m.pieces==="letters"?el.textContent.split(""):el.textContent.split(/(\s+)/);
    el.innerHTML=parts.map(function(x){if(/^\s+$/.test(x))return x;return'<span class="piece mv pre" style="'+motionVars(m,n++)+'">'+esc(x)+'</span>'}).join("")})})}
var io=null;
function observe(){if(io)io.disconnect();io=new IntersectionObserver(function(es){es.forEach(function(e){var t=e.target;
   if(e.isIntersecting){t.classList.add("in");if(t.dataset&&t.dataset.anim==="enter")t.classList.add("play")}
   else if(e.boundingClientRect.top>0&&S.replay==="every"){t.classList.remove("in");if(t.dataset&&t.dataset.anim==="enter")t.classList.remove("play")}})},{root:scr,rootMargin:"0px 0px -10% 0px",threshold:0.05});
  scr.querySelectorAll(".mv,[data-anim=enter]").forEach(function(el){io.observe(el)});}
function onScroll(box){var r=box.getBoundingClientRect();
  box.querySelectorAll("img[data-scroll]").forEach(function(img){if(box===scr&&img.closest(".mbox"))return;
   if(S.reduce){img.style.transform="";return}var b=img.parentNode.getBoundingClientRect();var c=(b.top+b.height/2-(r.top+r.height/2))/r.height;
   var m=motionById(img.dataset.scroll);if(!m)return;
   if(m.style==="zoom")img.style.transform="scale("+(1+m.factor*Math.max(0,Math.min(1,1-Math.abs(c))))+")";
   else if(m.style==="scalein"){var k=Math.max(0,Math.min(1,1-c));img.parentNode.style.transform="scale("+(1-m.factor+m.factor*k)+")";}
   else img.style.transform="translateY("+(c*-m.factor*b.height)+"px) scale(1.2)";});
  if(box===scr)scr.querySelectorAll('[data-anim="scrub"]').forEach(function(a){var b=a.getBoundingClientRect();var k=Math.max(0,Math.min(1,(r.bottom-b.top)/(r.height*.8)));
    a.querySelectorAll(".draw").forEach(function(d){d.style.strokeDashoffset=160*(1-k)});var fi=a.querySelector(".fillin");if(fi)fi.style.opacity=Math.max(0,(k-.6)*2)*.85});}
scr.addEventListener("scroll",function(){onScroll(scr)},{passive:true});
function initTransitions(){scr.querySelectorAll(".trv").forEach(function(t){var imgs=t.dataset.imgs.split(",");t.dataset.i="0";
  t.innerHTML=imgs.map(function(x,i){return'<div class="sl" style="opacity:'+(i?0:1)+'"><img alt="" src="'+IMG[x]+'"><span>'+(i+1)+' / '+imgs.length+'</span></div>'}).join("")})}
function turn(t,dir){var m=motionById(t.dataset.trv),sl=t.querySelectorAll(".sl"),n=sl.length,i=+t.dataset.i,j=(i+dir+n)%n;var A=sl[i],B=sl[j];t.dataset.i=j;
  sl.forEach(function(s,k){if(k!==i&&k!==j)s.style.opacity=0});
  if(S.reduce||!m){A.style.opacity=0;B.style.opacity=1;return}
  var s=m.style,KA,KB,oa="50% 50%",ob="50% 50%";
  if(s==="slide"){KA=[{transform:"translateX(0)"},{transform:"translateX("+(-100*dir)+"%)"}];KB=[{transform:"translateX("+(100*dir)+"%)"},{transform:"translateX(0)"}]}
  else if(s==="cube"){oa=dir>0?"100% 50%":"0% 50%";ob=dir>0?"0% 50%":"100% 50%";KA=[{transform:"translateX(0) rotateY(0)"},{transform:"translateX("+(-100*dir)+"%) rotateY("+(-90*dir)+"deg)"}];KB=[{transform:"translateX("+(100*dir)+"%) rotateY("+(90*dir)+"deg)"},{transform:"translateX(0) rotateY(0)"}]}
  else if(s==="deck"){KA=[{transform:"translate(0,0) rotate(0)"},{transform:"translate("+(-115*dir)+"%,6%) rotate("+(-14*dir)+"deg)"}];KB=[{transform:"translateY(14px) scale(.9)",opacity:.6},{transform:"translateY(0) scale(1)",opacity:1}]}
  else if(s==="flip"){KA=[{transform:"rotateY(0)"},{transform:"rotateY("+(180*dir)+"deg)"}];KB=[{transform:"rotateY("+(-180*dir)+"deg)"},{transform:"rotateY(0)"}]}
  else if(s==="zoom"){KA=[{transform:"scale(1)",opacity:1},{transform:"scale(1.25)",opacity:0}];KB=[{transform:"scale(.85)",opacity:0},{transform:"scale(1)",opacity:1}]}
  else{KA=[{opacity:1},{opacity:0}];KB=[{opacity:0},{opacity:1}]}
  A.style.transformOrigin=oa;B.style.transformOrigin=ob;A.style.opacity=1;B.style.opacity=1;A.style.zIndex=2;B.style.zIndex=1;
  var o={duration:m.dur,easing:easeCss(m.ease),fill:"forwards"};A.animate(KA,o).onfinish=function(){A.style.opacity=0;A.getAnimations().forEach(function(x){x.cancel()});A.style.zIndex=""};B.animate(KB,o).onfinish=function(){B.getAnimations().forEach(function(x){x.cancel()});B.style.opacity=1}}
/* ----- motion: a new type, By itself (Unity's time-driven Slow Zoom and Pan) ----- */
TYPES.self="By itself";PATH.self_zoom="nat";PATH.self_pan="nat";
S.motions.splice(S.motions.findIndex(function(m){return m.type==="emphasis"}),0,{id:"self_zoom",name:"Slow Zoom",type:"self",style:"zoom",dur:28000},{id:"self_pan",name:"Pan",type:"self",style:"pan",dur:26000});
var mz=motionById("zoom");if(mz)mz.name="Zoom with the scroll";
(S.styles.picture||[]).forEach(function(st){if(st.self==null)st.self="none"});(S.styles.video||[]).forEach(function(st){if(st.self==null)st.self="none"});
on("motionSummary.answer",function(m){if(m.type==="self")return "By itself · "+({zoom:"1 to 1.06 and back",pan:"across its room and back"})[m.style]+" · "+Math.round(m.dur/1000)+" s"});
on("motionDemo.answer",function(m){if(m.type!=="self")return;
  return '<div class="item" data-ref="m:'+m.id+'"><div class="mdemo"><div class="mhead"><div><b>'+esc(m.name)+'</b>'+pathBadge(m.id)+'<small>'+esc(motionSummary(m))+'</small></div></div><figure class="el-pic"><div class="fr"><img alt="" src="'+IMG.hero+'" class="'+(S.reduce?"":"self-"+m.style)+'" style="animation-duration:'+(m.dur/1000)+'s"></div></figure></div></div>'});
on("motionBody.answer",function(m){if(m.type!=="self")return;return row("Name",'<input class="u" data-text="motion" value="'+esc(m.name)+'">',"")+row("Loop (s)",slider("motion.dur",8000,60000,1000,m.dur),"One way, then back. Slower than 26 s per drift, at most 12 pt/s; stopped under Reduce Motion.")});
on("motionUses.count",function(n,id){Object.keys(S.styles).forEach(function(k){S.styles[k].forEach(function(st){if(st.self===id)n++})});return n});
on("renderEl.final",function(h,a){var style=a.style,still=a.still;if(!still&&style&&style.self&&style.self!=="none"&&!S.reduce){var m=motionById(style.self);if(m)h=h.replace(/<img alt="" src="([^"]*)"/,'<img alt="" src="$1" class="self-'+m.style+'" style="animation-duration:'+(m.dur/1000)+'s"')}return h});
