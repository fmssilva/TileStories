/* Explore: the non-AR path (.clinerules/30-ui-content.md: every core feature works without the camera). A list of the points that
   have a card; a tap opens the card (the action view "card", at Full: no wall behind it), and the card's tour plays as on the wall.
   It is reached from the wall (its Explore button) and, without the camera, it is the app's home. Built from existing parts (What
   next?'s point cards, the card, the tour); W's Explore screen replaces its look in P4.
   The camera: cameraOn() is the phone's camera, on and allowed (Unity: the AR session's availability and the camera permission);
   the preview bar's Camera / No camera switch (LAB.noCamera, Lab-only) stands in for a phone without it. Without the camera the
   wall is never drawn, the card opens at Full and closing it goes back to Explore (screen/router.js's sheetTo). */
LAB.noCamera=false;
function cameraOn(){return!LAB.noCamera}
/* the points with a card, in the wall's order (D.wall.wallPois: card true and a card in POINTS) */
function explorePoints(){return(D.wall.wallPois||[]).filter(function(p){return p.card&&POINTS[p.id]})}
function exploreScreen(){var ps=explorePoints();
  return'<div class="xp"><div class="xp-h"><div><b>'+TXH("explore_title")+'</b><small>'+TXH("explore_line")+'</small></div>'+
   (cameraOn()?'<button type="button" class="xp-wall" data-xpwall="1" aria-label="'+TXH("explore_wall")+'" title="'+TXH("explore_wall")+'">&#10005;</button>':"")+'</div>'+
   '<div class="xp-list" role="list">'+ps.map(function(p,i){var h=pointOf(p.id).hero||{};
    return'<button type="button" role="listitem" class="te-card xp-row" data-xpopen="'+p.id+'" style="--i:'+i+'"><i class="te-th" style="'+teThumb(p.id)+'"></i><span class="te-ct"><b>'+esc(L(h.title||p.name))+'</b><small>'+esc(L(h.eyebrow||""))+'</small></span></button>'}).join("")+'</div></div>'}
screenDef("explore",{name:"Explore (without the camera)",draw:exploreScreen});
/* the wall's way in: a button at the wall's bottom right (the ✦ at its bottom left), while the wall shows alone: no card, no overlay, no tour */
function exploreBtn(){return'<button type="button" class="xp-open" data-xpgo="1"><span aria-hidden="true">&#9776;</span>'+TXH("explore_open")+'</button>'}
on("renderPhone.after",function(){ph.classList.toggle("on-explore",ui.view==="explore");var o=ph.querySelector(".xp-open");if(o)o.remove();if(ui.view==="card"&&ui.stop==="closed"&&cameraOn()&&!ui.overlays.length&&!G.on)ph.insertAdjacentHTML("beforeend",exploreBtn())});
/* the preview bar's Camera / No camera: without the camera Explore is home; with it, the wall */
on("ptool.html",function(h){return h+'<button type="button" data-pt="camera" aria-pressed="'+LAB.noCamera+'" title="'+TXH("pbar_tip_camera")+'">'+TXH(LAB.noCamera?"pbar_nocamera":"pbar_camera")+'</button>'});
function cameraSet(on){if(G.on)stopTour();LAB.noCamera=!on;closeOverlays();goScreen(on?"card":"explore",{stop:"closed",reset:true});renderPhone(false)}
document.addEventListener("click",function(e){var t=e.target;if(!t.closest||!ph.contains(t))return;
  if(t.closest('[data-pt="camera"]')){e.stopPropagation();e.preventDefault();cameraSet(LAB.noCamera);return}
  if(t.closest("[data-xpgo]")){e.stopPropagation();goScreen("explore");renderPhone(false);return}
  if(t.closest("[data-xpwall]")){e.stopPropagation();goScreen("card",{stop:"closed"});renderPhone(false);return}
  var r=t.closest("[data-xpopen]");if(r){e.stopPropagation();doAction("card",r.dataset.xpopen,"explore")}},true);
/* the signature: Explore and the wall's button reach the phone (the self-test's words) */
on("modelSig.before",function(cx){var un=viewAs("explore");cx.explore=exploreScreen()+exploreBtn();un()});on("modelSig.sig",function(s,cx){return s+cx.explore});
COMP.s9explore=["Screens","Explore (without the camera)"];
