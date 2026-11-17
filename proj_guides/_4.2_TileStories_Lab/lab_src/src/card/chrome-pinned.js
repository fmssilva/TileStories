/* The close and settings buttons pinned to the sheet's top; the demo card's close. */
/* ----- close and settings: pinned to the sheet's top (sticky in the sheet, phone/grabber.css); the top band carries them once it shows ----- */
on("topBand.after",function(){var tb=ph.querySelector(".topband"),c=scr.querySelector(".chrome");if(c)c.style.visibility=tb&&tb.style.display!=="none"?"hidden":""});
/* ----- the demo card closes back to where it was opened from (a card, the wall, Demo Components); its settings open like the card's ----- */
ph.addEventListener("click",function(e){var t=e.target,ch=t.closest(".chrome i, .topband i");if(!ch)return;var x=ch.textContent.charCodeAt(0)===10005;
  if(ui.view==="democard"){e.stopPropagation();if(x){var b=ui.beforeDemo||{view:"card",stop:"closed"};goScreen(b.view,{stop:b.stop});setOverlay("settings",false);renderPhone(false)}else{setOverlay("settings",!overlayOn("settings"));renderPhone(true)}return}
  if(ui.view==="card"&&x&&ch.closest(".topband")){e.stopPropagation();sheetTo("closed");renderPhone(false)}},true);
