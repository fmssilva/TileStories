/* A change redraws the phone in place: the scroll stays, no entrance plays again; dark mode in place. */
function shownInPlace(){var r=scr.getBoundingClientRect();scr.classList.add("still");
  scr.querySelectorAll(".mv:not(.in)").forEach(function(el){var b=el.getBoundingClientRect();if(b.bottom>r.top&&b.top<r.bottom)el.classList.add("in")});
  void scr.offsetHeight;scr.classList.remove("still")}
on("renderPhone.before",function(keep,cx){cx.top=scr.scrollTop;cx.view=ui.view;cx.was=ui.lastStop;if(!keep||cx.view!==ui.lastView)ui.heroH=0;ui.lastView=cx.view});
on("renderPhone.after",function(keep,cx){var top=cx.top,view=cx.view,was=cx.was;if(keep&&ui.view===view){scr.scrollTop=top;shownInPlace()}
  ui.justOpened=ui.view==="card"&&ui.stop!=="closed"&&was==="closed";ui.stopMoved=ui.view==="card"&&was!==ui.stop;if(ui.stopMoved)ui.movedAt=performance.now();ui.lastStop=ui.view==="card"?ui.stop:"closed";
  if(ui.view!=="democard")ui.beforeDemo={view:ui.view,stop:ui.stop};
  if(ui.view==="democard"&&overlayOn("settings")&&!ph.querySelector(".vset"))ph.insertAdjacentHTML("beforeend",settingsPanel());
  var gb=document.getElementById("grab");if(gb)gb.setAttribute("aria-label",TXT(ui.stop==="full"?"sheet_collapse":"sheet_expand"))});
/* light / dark only repaints: the editor's previews change colour in place, the editor is not rebuilt */
function darkInPlace(){ed.querySelectorAll(".cv").forEach(function(c){c.classList.toggle("dark",P.dark)});renderPhone(true)}
document.addEventListener("click",function(e){var t=e.target;if(!t.closest)return;
  if(t.closest('#ptool [data-pt="dark"]')){e.stopPropagation();P.dark=!P.dark;emit("profileChanged","dark");darkInPlace();return}
  var vo=t.closest('[data-vis^="mode|"]');if(vo){e.stopPropagation();var m=vo.dataset.vis.split("|")[1];P.vis.mode=m;P.dark=m==="dark"||(m==="phone"&&D.card.cardCfg.mode==="dark");emit("profileChanged","mode");darkInPlace()}},true);
