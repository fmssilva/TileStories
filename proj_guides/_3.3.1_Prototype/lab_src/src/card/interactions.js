/* The interactive parts on the phone: scroll boxes, the before/after slider, the deck, hold, the panorama, scrub, tilt, page turns (initBoxes, dragX). */
function initBoxes(){scr.querySelectorAll(".mbox").forEach(function(bx){bx.addEventListener("scroll",function(){onScroll(bx)},{passive:true});onScroll(bx)});
  scr.querySelectorAll("[data-scrub]").forEach(function(sb){sb.addEventListener("scroll",function(){var st=sb.querySelectorAll(".step"),imgs=sb.querySelectorAll(".stick img"),top=sb.getBoundingClientRect().top+150,idx=0;
    st.forEach(function(s,i){if(s.getBoundingClientRect().top<top+40)idx=i});imgs.forEach(function(im,i){im.style.opacity=i===idx?1:0})},{passive:true})});
  scr.querySelectorAll("[data-ba]").forEach(dragX(function(el,f){el.style.setProperty("--x",(f*100).toFixed(1)+"%")}));
  scr.querySelectorAll("[data-turnt]").forEach(dragX(function(el,f,dx){el.style.backgroundPositionX=(parseFloat(el.dataset.bx||50)+dx*.3)+"%"},true));
  scr.querySelectorAll("[data-pano]").forEach(dragX(function(el,f,dx){var v=(parseFloat(el.dataset.px||0)+dx);el.dataset.px=v;el.style.backgroundPositionX=v+"px"},true));
  scr.querySelectorAll("[data-tilt]").forEach(function(t){t.addEventListener("pointermove",function(e){if(S.reduce)return;var b=t.getBoundingClientRect(),x=(e.clientX-b.left)/b.width-.5,y=(e.clientY-b.top)/b.height-.5;var s=(interById("tiltlook")||{o:{strength:12}}).o.strength;t.querySelector("img").style.transform="translate("+(-x*s)+"px,"+(-y*s)+"px)"})});
  scr.querySelectorAll("[data-hold]").forEach(function(hd){var timer=null,start=0;function stop(){cancelAnimationFrame(timer);timer=null;if(hd.classList.contains("cover"))hd.querySelector(".ring").style.setProperty("--p",0)}
    hd.addEventListener("pointerdown",function(){if(!hd.classList.contains("cover")){hd.classList.add("cover");return}start=performance.now();var need=(interById("hold")||{o:{hold:900}}).o.hold;(function step(){var p=Math.min(1,(performance.now()-start)/need);hd.querySelector(".ring").style.setProperty("--p",p*100);if(p>=1){hd.classList.remove("cover");timer=null;return}timer=requestAnimationFrame(step)})()});
    hd.addEventListener("pointerup",stop);hd.addEventListener("pointerleave",stop)});
  scr.querySelectorAll("[data-deck]").forEach(function(dk){dk.addEventListener("pointerdown",function(e){var cards=dk.querySelectorAll(".cd"),top=cards[cards.length-1];if(!top)return;var x0=e.clientX;top.setPointerCapture(e.pointerId);
    function mv(ev){top.style.transition="none";top.style.transform="translateX("+(ev.clientX-x0)+"px) rotate("+((ev.clientX-x0)/12)+"deg)"}
    function up(ev){top.removeEventListener("pointermove",mv);top.removeEventListener("pointerup",up);top.style.transition="";var dx=ev.clientX-x0,need=(interById("deck")||{o:{throwd:120}}).o.throwd;
      if(Math.abs(dx)>need/2){top.style.transform="translateX("+(dx>0?400:-400)+"px) rotate("+(dx>0?25:-25)+"deg)";top.style.opacity=0;setTimeout(function(){dk.insertBefore(top,dk.firstChild);top.style.transform="translateY(6px) scale(.92)";top.style.opacity=1;restack(dk)},350)}else top.style.transform=""}
    top.addEventListener("pointermove",mv);top.addEventListener("pointerup",up)})});}
function restack(dk){var cards=dk.querySelectorAll(".cd");cards.forEach(function(c,i){c.style.zIndex=i;c.style.transform="translateY("+((cards.length-1-i)*6)+"px) scale("+(1-(cards.length-1-i)*.04)+")"})}
function dragX(fn,rel){return function(el){el.addEventListener("pointerdown",function(e){try{el.setPointerCapture(e.pointerId)}catch(x){}var x0=e.clientX,last=e.clientX;
  function mv(ev){var b=el.getBoundingClientRect();fn(el,Math.max(0,Math.min(1,(ev.clientX-b.left)/b.width)),ev.clientX-last);last=ev.clientX}
  function up(){el.removeEventListener("pointermove",mv);el.removeEventListener("pointerup",up)}
  if(!rel)mv(e);el.addEventListener("pointermove",mv);el.addEventListener("pointerup",up)})}}
