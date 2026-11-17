/* The screen router (Unity: the UI Toolkit screen stack, one UIDocument per screen and a stack of panels over it): every change
   of what the phone shows goes through here. It is the only writer of ui.view (the screen), ui.stop (the card sheet's stop),
   ui.history (the screens to go back to) and ui.overlays (the overlays open, bottom first); check_structure.js counts the writes
   anywhere else (0). Each change emits screenChanged once, through core/ext.js: {kind: "screen" | "stop" | "open" | "close",
   from, to, stop, was, id}.
   - Screens, registered by their module (screenDef): card (the wall, with the card sheet over it), story, gallery, kpage (a
     block's own page), viewer (the picture viewer), term (a term's page), explore (the points without the camera), and the Lab's
     demo and democard. `over: "card"` draws the page in the card screen's frame (the page's own layers hidden, card/pages.js).
   - The card sheet is the wall screen's base overlay: open while ui.stop is not "closed" (sheetTo moves it). Without the
     camera (cameraOn(), screen/explore.js) the card has no closed stop: closing it goes back to the screen it came from.
   - Overlays over a screen, in the order they opened (overlayDef): settings (the visitor's Settings), contents (the Contents
     sheet), menu (the guide's ✦ menu), ask (Ask about the view), try (Things to try).
   - back(): the top overlay first, then the sheet, then the screen before; goScreen(id) to a screen already in the history returns
     to it (the screens above it leave the history), so the history never loops.
   - Action views by key (actionDef / doAction): what a tap on a point can open, by On tap's keys (card, tour, ask; P4 and P5
     add model_screen, pano, then_now...). */
var SCREEN={},OVERLAY={},ACTION={};
ui.stop="closed";ui.history=[];ui.overlays=[];
function screenDef(id,spec){SCREEN[id]=spec}
function overlayDef(id,spec){OVERLAY[id]=spec}
function actionDef(key,spec){ACTION[key]=spec}
function screenHtml(){return SCREEN[ui.view].draw()}
function screenChanged(d){emit("screenChanged",d)}
/* go to a screen; o.stop: the card sheet's stop with it (one change); o.reset: a new history (the app's home); o.draw: redraw now */
function goScreen(id,o){o=o||{};if(!SCREEN[id])throw new Error("No screen "+id+" (screenDef)");var from=ui.view,was=ui.stop;
  if(from!==id){var i=ui.history.indexOf(id);if(i>=0)ui.history.length=i;else if(from&&SCREEN[from])ui.history.push(from);ui.view=id}
  if(o.reset)ui.history=[];
  if(o.stop!==undefined&&o.stop!==null)ui.stop=o.stop;
  if(from!==id||was!==ui.stop)screenChanged({kind:from!==id?"screen":"stop",from:from,to:id,stop:ui.stop,was:was});
  if(o.draw)renderPhone(false)}
/* the card sheet to a stop (closed, peek, half, full); without the camera, closing the card goes back to where it came from */
function sheetTo(st){if(st===ui.stop)return;
  if(st==="closed"&&ui.view==="card"&&!cameraOn()){goScreen(ui.history.length?ui.history[ui.history.length-1]:"explore",{stop:"closed"});return}
  var was=ui.stop;ui.stop=st;screenChanged({kind:"stop",from:ui.view,to:ui.view,stop:st,was:was})}
function overlayOn(id){return ui.overlays.indexOf(id)>=0}
function topOverlay(){return ui.overlays.length?ui.overlays[ui.overlays.length-1]:ui.view==="card"&&ui.stop!=="closed"?"sheet":null}
/* overlayOn(id): the overlay is open (the Editor's own isOpen is its foldouts'); an overlay opened or closed; setOverlay returns its state, so a chained assignment keeps its value */
function setOverlay(id,on){on=!!on;if(!OVERLAY[id])throw new Error("No overlay "+id+" (overlayDef)");var i=ui.overlays.indexOf(id);if(on===(i>=0))return on;
  if(on)ui.overlays.push(id);else ui.overlays.splice(i,1);screenChanged({kind:on?"open":"close",id:id,from:ui.view,to:ui.view,stop:ui.stop});return on}
function openOverlay(id){return setOverlay(id,true)}
function closeOverlay(id){return setOverlay(id,false)}
function closeOverlays(){ui.overlays.slice().reverse().forEach(closeOverlay)}
/* the drawing of another screen's frame without a change (a page over the card's frame, a signature drawn off screen): ui.view is
   that screen until the returned function puts it back; no event, no history */
function viewAs(id){var keep=ui.view;ui.view=id;return function(){ui.view=keep}}
/* back: the top overlay, else the sheet, else the screen before; returns what it closed or left (null: nothing to go back to) */
function back(){var t=ui.overlays[ui.overlays.length-1];
  if(t){closeOverlay(t);renderPhone(true);return t}
  if(ui.view==="card"&&ui.stop!=="closed"){sheetTo("closed");renderPhone(false);return "sheet"}
  if(!ui.history.length)return null;var from=ui.view,to=ui.history.pop();ui.view=to;screenChanged({kind:"screen",from:from,to:to,stop:ui.stop,was:ui.stop,back:true});renderPhone(false);return from}
function doAction(key,id,why){var a=ACTION[key];if(!a)throw new Error("No action view "+key+" (actionDef)");return a.run(id,why)}

overlayDef("settings",{name:"Settings"});
overlayDef("contents",{name:"Contents"});
overlayDef("menu",{name:"The guide's ✦ menu"});
overlayDef("ask",{name:"Ask about the view"});
overlayDef("try",{name:"Things to try"});
/* On tap's actions this Lab has (W's keys, data/ACTIONS): the card at its opening stop (from Explore, without the camera: Full) */
actionDef("card",{name:"Open the card",run:function(id,why){if(id&&!pointOpen(id))return false;
  goScreen("card",{stop:cameraOn()?D.card.cardCfg.openAt:"full"});feedback("open");renderPhone(false);emit("card.opened",why||"marker");return true}});
actionDef("tour",{name:"Start a tour here",run:function(id){if(id&&!pointOpen(id))return false;if(ui.view!=="card"||ui.stop==="closed")goScreen("card",{stop:"half"});renderPhone(false);startTour();return true}});
actionDef("ask",{name:"Ask the guide",run:function(id,why){if(ui.view!=="card")goScreen("card",{stop:"closed"});openOverlay("ask");renderPhone(true);emit("view.opened",why||"an action");return true}});
