/* The sheet: its stops (ui.stop, stopTop), the camera behind it, the markers that open it, a tap outside that closes it. */
/* ----- the wall, the marker and the sheet ----- */
function stopTop(st){var ans=firstAnswer("stopTop.answer",st);if(ans!==undefined)return ans;var H=devH();if(st==="closed")return H+10;if(devLand())return 0;if(st==="peek")return H-200;if(st==="half")return Math.round(H*(1-D.card.cardCfg.halfMax));if(st==="full")return 0;return H+10}
on("renderPhone.after",function(keep){var inWall=ui.view==="card";
  document.getElementById("camera").style.backgroundImage=inWall&&cameraOn()?"url("+IMG.g1+")":"none";
  var w=wallLayer();w.style.display=inWall&&cameraOn()?"":"none";
  var top=inWall?stopTop(ui.stop):0;scr.style.top=top+"px";scr.style.borderRadius=inWall&&ui.stop!=="full"?"22px 22px 0 0":"";scr.style.transition="top .35s cubic-bezier(.2,.8,.2,1)";
  scr.style.overflowY=inWall&&ui.stop==="peek"?"hidden":"auto";
  var gb=document.getElementById("grab");if(!gb){gb=document.createElement("div");gb.id="grab";gb.className="grab";ph.appendChild(gb);initGrab(gb)}
  gb.style.display=inWall&&ui.stop!=="closed"&&!devLand()?"":"none";gb.style.top=(top+2)+"px";
  var nb=document.getElementById("navbar");if(inWall&&(ui.stop==="closed"||ui.stop==="peek"))nb.style.display="none";
  var lp=ph.querySelector(".locpin");if(lp&&inWall&&ui.stop==="closed")lp.remove();
  var pk=ph.querySelector(".peeknext");if(pk)pk.remove();
  if(inWall&&ui.stop==="peek"&&D.card.cardCfg.peekNext){var p=document.createElement("div");p.className="peeknext";p.textContent=(TXT("peek_next_label"))+"Lisbon Cathedral ↑";ph.appendChild(p)}
  topBand();stickyAction()});
/* ----- phone events ----- */
ph.addEventListener("click",function(e){var t=e.target;
  var mk=t.closest("[data-marker]");if(mk){doAction("card",mk.dataset.marker,"marker");return}
  if(ui.view==="card"&&t===ph.querySelector("#camera")||t.closest("#wall")&&!t.closest(".mk")){if(D.card.cardCfg.tapOutside&&ui.stop!=="closed"){sheetTo("closed");renderPhone(false)}return}
  var ch=t.closest(".chrome i");if(ch&&ui.view==="card"&&ch.textContent.charCodeAt(0)===10005){sheetTo("closed");renderPhone(false);return}
  var lt=t.closest("[data-tabsec],[data-pagesec]");if(lt){ui.activeSec=lt.dataset.tabsec||lt.dataset.pagesec;feedback("jump");renderPhone(true);return}
  var ac=t.closest("[data-accsec]");if(ac){var id=ac.dataset.accsec;ui.openSecs[id]=!ui.openSecs[id];feedback("tap");renderPhone(true);return}
  var nb=t.closest("#navbar span");if(nb&&ui.view==="card"){var idx=Array.prototype.indexOf.call(nb.parentNode.children,nb),vis=D.card.sections.filter(function(s){return s.shown&&D.poi.poiSec[s.id]!=="hide"}),sec=vis[idx];if(!sec)return;
    feedback("jump");ui.activeSec=sec.id;ui.openSecs[sec.id]=true;if(ui.stop!=="full"&&D.card.ctl.jump.opens==="full")sheetTo("full");else if(ui.stop==="peek")sheetTo("half");renderPhone(true);
    var el=scr.querySelector('[data-ref="sec:'+sec.id+'"]');if(el)scr.scrollTo({top:el.offsetTop-60,behavior:D.card.ctl.jump.motion==="smooth"&&!P.reduce?"smooth":"auto"});return}
  if(t.closest(".locpin")||t.closest("[data-locate]")){feedback("locate");sheetTo("peek");renderPhone(false);var m=ph.querySelector('[data-marker="painting"]');if(m){m.classList.remove("found");void m.offsetWidth;m.classList.add("found")}return}
  var kq=t.closest(".kq button");if(kq)feedback(kq.dataset.ok?"right":"wrong");
  if(t.closest(".bprev"))feedback("tap")},true);
/* captures: #pshot=<stop>|<scroll px> */
if(location.hash.indexOf("#pshot=")===0)setTimeout(function(){var p=decodeURIComponent(location.hash.slice(7)).split("|");P.reduce=true;goScreen("card",{stop:p[0]||"half"});renderAll(false);setTimeout(function(){scr.scrollTop=+(p[1]||0);barAfterHero();topBand()},250)},50);
