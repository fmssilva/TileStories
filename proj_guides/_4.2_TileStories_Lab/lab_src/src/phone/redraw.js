/* A change redraws the phone in place: the scroll stays, no entrance plays again; dark mode in place. */
function shownInPlace(){var r=scr.getBoundingClientRect();scr.classList.add("still");
  scr.querySelectorAll(".mv:not(.in)").forEach(function(el){var b=el.getBoundingClientRect();if(b.bottom>r.top&&b.top<r.bottom)el.classList.add("in")});
  void scr.offsetHeight;scr.classList.remove("still")}
on("renderPhone.before",function(keep,cx){cx.top=scr.scrollTop;cx.view=S.view;cx.was=S.lastStop;if(!keep||cx.view!==S.lastView)S.heroH=0;S.lastView=cx.view});
on("renderPhone.after",function(keep,cx){var top=cx.top,view=cx.view,was=cx.was;if(keep&&S.view===view){scr.scrollTop=top;shownInPlace()}
  S.justOpened=S.view==="card"&&S.stop!=="closed"&&was==="closed";S.stopMoved=S.view==="card"&&was!==S.stop;if(S.stopMoved)S.movedAt=performance.now();S.lastStop=S.view==="card"?S.stop:"closed";
  if(S.view!=="democard")S.beforeDemo={view:S.view,stop:S.stop};
  if(S.view==="democard"&&S.settingsOpen&&!ph.querySelector(".vset"))ph.insertAdjacentHTML("beforeend",settingsPanel());
  var gb=document.getElementById("grab");if(gb)gb.setAttribute("aria-label",TXT(S.stop==="full"?"sheet_collapse":"sheet_expand"))});
/* light / dark only repaints: the editor's previews change colour in place, the editor is not rebuilt */
function darkInPlace(){ed.querySelectorAll(".cv").forEach(function(c){c.classList.toggle("dark",S.dark)});renderPhone(true)}
document.addEventListener("click",function(e){var t=e.target;if(!t.closest)return;
  if(t.closest('#ptool [data-pt="dark"]')){e.stopPropagation();S.dark=!S.dark;darkInPlace();return}
  var vo=t.closest('[data-vis^="mode|"]');if(vo){e.stopPropagation();var m=vo.dataset.vis.split("|")[1];S.vis.mode=m;S.dark=m==="dark"||(m==="phone"&&S.cardCfg.mode==="dark");darkInPlace()}},true);
