/* The preview bar's grip (drag it anywhere) and its replay. */
/* the grabber's drag ends in renderPhone: the hero follows the new stop */
/* ----- Peek: the title row on the paper, like Unity's compact header ----- */
/* ----- editor tools: one look (red dashes, EDITOR tag), a grip to move them; the phone's view label is gone ----- */
S.ptoolPos=null;
function editorTools(){var pt=document.getElementById("ptool");if(!pt)return;
  if(!pt.querySelector(".egrip"))pt.insertAdjacentHTML("afterbegin",'<span class="egrip" title="Drag to move this editor tool" aria-label="Move">&#10303;</span>');
  if(!pt.querySelector("[data-ereplay]"))pt.insertAdjacentHTML("beforeend",'<button type="button" data-ereplay="1" title="Replay the motion from the top">&#8635;</button>');
  if(S.ptoolPos){pt.style.left=S.ptoolPos[0]+"px";pt.style.top=S.ptoolPos[1]+"px";pt.style.right="auto"}}
ph.addEventListener("pointerdown",function(e){var g=e.target.closest("#ptool .egrip");if(!g)return;e.preventDefault();e.stopPropagation();var pt=document.getElementById("ptool"),k=fit.getBoundingClientRect().width/SCREENS[S.screen][0];
  var r=pt.getBoundingClientRect(),pr=ph.getBoundingClientRect(),x0=e.clientX,y0=e.clientY,l0=(r.left-pr.left)/k,t0=(r.top-pr.top)/k;g.setPointerCapture(e.pointerId);
  function mv(ev){var W=SCREENS[S.screen][0],H=SCREENS[S.screen][1],l=Math.min(Math.max(0,l0+(ev.clientX-x0)/k),W-pt.offsetWidth),t=Math.min(Math.max(0,t0+(ev.clientY-y0)/k),H-pt.offsetHeight);S.ptoolPos=[Math.round(l),Math.round(t)];pt.style.left=S.ptoolPos[0]+"px";pt.style.top=S.ptoolPos[1]+"px";pt.style.right="auto"}
  function up(){g.removeEventListener("pointermove",mv);g.removeEventListener("pointerup",up)}
  g.addEventListener("pointermove",mv);g.addEventListener("pointerup",up)},true);
ph.addEventListener("click",function(e){if(e.target.closest("#ptool .egrip")){e.stopPropagation();return}var r=e.target.closest("[data-ereplay]");if(r){e.stopPropagation();var b=document.getElementById("replay");if(b)b.click()}},true);
