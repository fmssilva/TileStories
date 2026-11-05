/* ===================== v8e: the hero fills what the card shows; editor tools look like editor tools and move; the page's top is clean ===================== */
function heroH(){var H=SCREENS[S.screen][1];if(S.screen==="land")return 300;return Math.max(220,H-stopTop(S.stop==="peek"||S.stop==="closed"?"half":S.stop))}
function barAfterHero(){var nb=document.getElementById("navbar"),c=S.ctl.bar;if(!nb||S.view!=="card"||!c.on||c.showFrom!=="hero"||S.stop==="peek"||S.stop==="closed")return;
  var hero=scr.querySelector(".hero"),past=hero?scr.scrollTop>hero.offsetHeight-140:true;if(!past)nb.style.display="none";else if(S.card.length>=c.minBlocks&&S.sections.filter(shownOnBar).length>=2)nb.style.display=""}
on("renderPhone.after",function(keep){fitHero();barAfterHero();editorTools()});
scr.addEventListener("scroll",barAfterHero,{passive:true});
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
/* ----- the setup flow, where the developer works: at the top of the Detail Card tab ----- */
on("edCard.html",function(h){var i=h.indexOf('<details');var flow='<div class="note flow"><b>Setting up a card, in order.</b> 3 · Blocks &amp; Components: elements and their named styles, patterns made of them, then each block\'s own options, preview and page. 1 · Card and 2 · Sections: how the card opens and looks, and its chapters. 4 · Motion &amp; Effects and 5 · Card Controls: movement and the parts around the content. Then Specific Marker: 1 · The card (this point\'s blocks), 2 · Pages (their content). Opening any part shows it on the phone.</div>';
  return i>=0?h.slice(0,i)+flow+h.slice(i):flow+h});
/* captures: #pshot=<stop>|<scroll px> */
if(location.hash.indexOf("#pshot=")===0)setTimeout(function(){var p=decodeURIComponent(location.hash.slice(7)).split("|");S.reduce=true;S.view="card";S.stop=p[0]||"half";renderAll(false);setTimeout(function(){scr.scrollTop=+(p[1]||0);barAfterHero();topBand()},250)},50);

/* the top band after the hero carries the sliders mark too */
on("topBand.after",function(){var tb=ph.querySelector(".topband");if(tb)tb.querySelectorAll("i").forEach(function(i){if(i.textContent==="\u2630")i.innerHTML=SLIDERS})});
/* Peek: the title row starts at the sheet's own top */
function peekFix(){var pr=ph.querySelector(".peekrow");if(pr)pr.style.top=stopTop("peek")+"px"}
/* the locate label shows only where the hero's words leave it room (Unity: LabelFits) */
on("renderPhone.after",function(keep){peekFix();locFit()});scr.addEventListener("scroll",locFit,{passive:true});
on("renderPhone.after",function(keep){var lp=ph.querySelector(".locpin");if(lp)lp.style.display=S.view==="card"&&S.stop==="peek"?"none":"";setTimeout(locFit,420)});
