/* The hero's Start the guide (round 19): the mascot stands beside it at the hero's bottom left, pointing at it (it no longer vanishes behind the card), and the button gets a calm "tap me" life once the card has settled: a soft glow and a slow pulse, gone once tapped (Reduce Motion: the glow only). */
/* the developer (2026-11-13, print 15): "when we open the Ribeira Palace the mascot disappears… maybe it can move to the left side of the
   Start the guide button, and we move the button a bit to the right, and we make the button a bit more active with some click-me action
   and the guide pointing at it". The ✦ and the mascot beside it stay hidden while the hero's Start is on the screen (world/guide-button.js:
   the hero is the way in until then); this mascot is the same character, in the hero's action row, so the visitor sees it the moment the
   card opens. A tap on it pokes it (Tap on the mascot), never starts the tour. Unity: the hero's action row (a VisualElement row: the
   mascot's still image or its 3D prefab's RenderTexture, then the Start button); the glow a child image behind the button, its pulse a
   USS transition loop (or a DOTween loop on its scale and alpha), stopped by the button's first click; Reduce Motion: the glow alone */
G.hs={glow:false,tapped:false,t:0};
function hsOn(){return!!S.guideCfg.heroButton&&typeof gbAzu==="function"&&gbAzu()}
on("cardScreen.html",function(h){if(!hsOn())return h;var d=document.createElement("div");d.innerHTML=h;var a=d.querySelector(".hero .hact"),g=a&&a.querySelector(".gstart");if(!g)return h;
  a.classList.add("hs-row");a.insertAdjacentHTML("afterbegin",'<span class="hsazu" aria-hidden="true"><span class="hsfig">'+mascotStill()+'</span><i class="hspoint"><svg viewBox="0 0 16 16"><path d="M2.5 2.5 14 8 2.5 13.5Z"/></svg></i></span>');
  if(G.hs.glow&&!G.hs.tapped)g.classList.add("hsglow");
  /* the room the Locate Button's label asked for (card/locate.js's locRoom), kept across a redraw while the label shows */
  var w=d.querySelector(".hero .w");if(w&&G.locRoom&&!S.locFolded)w.classList.add("hsroom");return d.innerHTML});
/* the glow comes once the card has settled (the sheet's move ended: 900 ms after it moved), and stays until the button is tapped */
function hsSettle(){clearTimeout(G.hs.t);if(G.hs.tapped||G.hs.glow)return;var g=scr.querySelector(".hero .gstart");if(!g||S.view!=="card"||S.stop==="closed"||S.stop==="peek")return;
  var wait=Math.max(0,900-(performance.now()-(S.movedAt||0)));G.hs.t=setTimeout(function(){var b=scr.querySelector(".hero .gstart");if(!b||G.hs.tapped||S.view!=="card"||S.stop==="closed")return;G.hs.glow=true;b.classList.add("hsglow")},wait)}
on("renderPhone.after",function(){hsSettle()});
ph.addEventListener("click",function(e){var g=e.target.closest&&e.target.closest(".hero .gstart");if(g){G.hs.tapped=true;G.hs.glow=false;clearTimeout(G.hs.t);g.classList.remove("hsglow");return}
  var m=e.target.closest&&e.target.closest(".hsazu");if(m){e.stopPropagation();if(typeof haptic==="function")haptic();m.classList.remove("hsjump");void m.offsetWidth;m.classList.add("hsjump")}},true);
