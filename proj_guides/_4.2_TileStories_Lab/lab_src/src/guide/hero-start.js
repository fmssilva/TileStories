/* The hero's Start the guide (round 19): the mascot stands beside it at the hero's bottom left, pointing at it (it no longer vanishes behind the card), and the button gets a calm "tap me" life once the card has settled: a soft glow and a slow pulse, gone once tapped (Reduce Motion: the glow only). */
/* the developer (2026-11-13, print 15): "when we open the Ribeira Palace the mascot disappears… maybe it can move to the left side of the
   Start the guide button, and we move the button a bit to the right, and we make the button a bit more active with some click-me action
   and the guide pointing at it". The ✦ and the mascot beside it stay hidden while the hero's Start is on the screen (world/guide-button.js:
   the hero is the way in until then); this mascot is the same character, in the hero's action row, so the visitor sees it the moment the
   card opens. A tap on it pokes it (Tap on the mascot), never starts the tour. Unity: the hero's action row (a VisualElement row: the
   mascot's still image or its 3D prefab's RenderTexture, then the Start button); the glow a child image behind the button, its pulse a
   USS transition loop (or a DOTween loop on its scale and alpha), stopped by the button's first click; Reduce Motion: the glow alone */
G.hs={glow:false,tapped:false,t:0};
/* round 20 (the developer, 2026-11-13: "make it an Editor option; as default it can stay once per visit"): Start the guide glows Once per
   visit (from the first card the visitor opens until the first tap on Start; after it, no card glows again this visit), On every new card
   (each card opened glows again until its Start is tapped) or Off. Unity: GuideConfig.heroGlow (an enum), read by the hero's action row */
S.guideCfg.heroGlow="visit";
function hsGlowOn(){return S.guideCfg.heroGlow!=="off"}
function hsOn(){return!!S.guideCfg.heroButton&&typeof gbAzu==="function"&&gbAzu()}
on("cardScreen.html",function(h){if(!hsOn())return h;var d=document.createElement("div");d.innerHTML=h;var a=d.querySelector(".hero .hact"),g=a&&a.querySelector(".gstart");if(!g)return h;
  a.classList.add("hs-row");a.insertAdjacentHTML("afterbegin",'<span class="hsazu" aria-hidden="true"><span class="hsfig">'+mascotStill()+'</span><i class="hspoint"><svg viewBox="0 0 16 16"><path d="M2.5 2.5 14 8 2.5 13.5Z"/></svg></i></span>');
  if(G.hs.glow&&!G.hs.tapped&&hsGlowOn())g.classList.add("hsglow");
  /* the room the Locate Button's label asked for (card/locate.js's locRoom), kept across a redraw while the label shows */
  var w=d.querySelector(".hero .w");if(w&&G.locRoom&&!S.locFolded)w.classList.add("hsroom");return d.innerHTML});
/* the glow comes once the card has settled (the sheet's move ended: 900 ms after it moved), and stays until the button is tapped */
function hsSettle(){clearTimeout(G.hs.t);if(G.hs.tapped||G.hs.glow||!hsGlowOn())return;var g=scr.querySelector(".hero .gstart");if(!g||S.view!=="card"||S.stop==="closed"||S.stop==="peek")return;
  var wait=Math.max(0,900-(performance.now()-(S.movedAt||0)));G.hs.t=setTimeout(function(){var b=scr.querySelector(".hero .gstart");if(!b||G.hs.tapped||S.view!=="card"||S.stop==="closed")return;G.hs.glow=true;b.classList.add("hsglow")},wait)}
on("renderPhone.after",function(){hsSettle()});
/* On every new card: a card opened (a marker, a pin, an invitation, What next?) or another point's card made the open one glows again */
function hsNewCard(){if(S.guideCfg.heroGlow!=="card")return;clearTimeout(G.hs.t);G.hs.glow=false;G.hs.tapped=false;var b=scr.querySelector(".hero .gstart");if(b)b.classList.remove("hsglow");hsSettle()}
on("card.opened",hsNewCard);on("point.opened",hsNewCard);
/* the row, in Guide › 2 · The tour › Ways in › Start the guide (the button's own rows: Shown, its words, now its glow) */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7hero",row("Start the guide glows",pseg("guideCfg.heroGlow",["visit","card","off"],["Once per visit","On every new card","Off"]),
    "A soft warm glow and a slow pulse on Start the guide once the card has settled, gone when the visitor taps it. Once per visit (the default): it glows from the first card the visitor opens until the first tap on Start; after that no card glows again this visit. On every new card: each card the visitor opens glows again until its Start is tapped. Off: never. Reduce Motion: the glow without its pulse. Unity: GuideConfig.heroGlow, the glow a child image of the button, its pulse a USS transition loop."));
  return d.innerHTML});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(heroGlow|"));
/* its demo: a fresh visit, the Ribeira Palace's card opened (its Start glows once the card settles, or not when Off), a tap on Start
   (the glow goes; the tour does not start), then a second card, the Torreão's: it glows again On every new card, not Once per visit */
function hsDemo(){var my=G.demoSeq,alive=function(){return G.demoSeq===my},open=function(id){S.movedAt=performance.now();pointOpen(id);S.view="card";S.stop="half";renderPhone(false)};
  clearTimeout(G.hs.t);G.hs.glow=false;G.hs.tapped=false;open("painting");
  setTimeout(function(){if(!alive())return;var b=scr.querySelector(".hero .gstart");G.hs.tapped=true;G.hs.glow=false;clearTimeout(G.hs.t);if(b){b.classList.remove("hsglow");b.classList.remove("hstap");void b.offsetWidth;b.classList.add("hstap")}document.body.dataset.hsdemo="tapped"},2600);
  setTimeout(function(){if(!alive())return;open("torreao");if(S.guideCfg.heroGlow==="card")hsNewCard();document.body.dataset.hsdemo="second"},3800)}
rowDemos({"guideCfg.heroGlow":{view:"card",stop:"half",act:hsDemo,ring:".hero .gstart",ringAfter:1200}});
ph.addEventListener("click",function(e){var g=e.target.closest&&e.target.closest(".hero .gstart");if(g){G.hs.tapped=true;G.hs.glow=false;clearTimeout(G.hs.t);g.classList.remove("hsglow");
    /* round 20: the tour's own tap handler (an earlier module) has already redrawn the phone with the glow on: the new button too */
    ph.querySelectorAll(".hero .gstart.hsglow").forEach(function(x){x.classList.remove("hsglow")});return}
  var m=e.target.closest&&e.target.closest(".hsazu");if(m){e.stopPropagation();if(typeof haptic==="function")haptic();m.classList.remove("hsjump");void m.offsetWidth;m.classList.add("hsjump")}},true);
