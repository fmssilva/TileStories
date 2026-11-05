/* The sheet: its stops (S.stop, stopTop), the camera behind it, the markers that open it, a tap outside that closes it. */
S.stop="closed";
/* ----- the wall, the marker and the sheet ----- */
function stopTop(st){var ans=firstAnswer("stopTop.answer",st);if(ans!==undefined)return ans;var H=SCREENS[S.screen][1];if(S.screen==="land")return 0;if(st==="peek")return H-200;if(st==="half")return Math.round(H*(1-S.cardCfg.halfMax));if(st==="full")return 0;return H+10}
on("renderPhone.after",function(keep){var inWall=S.view==="card";
  document.getElementById("camera").style.backgroundImage=inWall||S.screen==="land"?"url("+IMG.g1+")":"none";
  var w=wallLayer();w.style.display=inWall?"":"none";
  var top=inWall?stopTop(S.stop):0;scr.style.top=top+"px";scr.style.borderRadius=inWall&&S.stop!=="full"?"22px 22px 0 0":"";scr.style.transition="top .35s cubic-bezier(.2,.8,.2,1)";
  scr.style.overflowY=inWall&&S.stop==="peek"?"hidden":"auto";
  var gb=document.getElementById("grab");if(!gb){gb=document.createElement("div");gb.id="grab";gb.className="grab";ph.appendChild(gb);initGrab(gb)}
  gb.style.display=inWall&&S.stop!=="closed"&&S.screen!=="land"?"":"none";gb.style.top=(top+2)+"px";
  var nb=document.getElementById("navbar");if(inWall&&(S.stop==="closed"||S.stop==="peek"))nb.style.display="none";
  var lp=ph.querySelector(".locpin");if(lp&&inWall&&S.stop==="closed")lp.remove();
  var pk=ph.querySelector(".peeknext");if(pk)pk.remove();
  if(inWall&&S.stop==="peek"&&S.cardCfg.peekNext){var p=document.createElement("div");p.className="peeknext";p.textContent=(TXT("peek_next_label"))+"Lisbon Cathedral ↑";ph.appendChild(p)}
  topBand();stickyAction()});
/* ----- phone events ----- */
ph.addEventListener("click",function(e){var t=e.target;
  var mk=t.closest("[data-marker]");if(mk){S.stop=S.cardCfg.openAt;S.view="card";feedback("open");renderPhone(false);return}
  if(S.view==="card"&&t===ph.querySelector("#camera")||t.closest("#wall")&&!t.closest(".mk")){if(S.cardCfg.tapOutside&&S.stop!=="closed"){S.stop="closed";renderPhone(false)}return}
  var ch=t.closest(".chrome i");if(ch&&S.view==="card"&&ch.textContent.charCodeAt(0)===10005){S.stop="closed";renderPhone(false);return}
  var lt=t.closest("[data-tabsec],[data-pagesec]");if(lt){S.activeSec=lt.dataset.tabsec||lt.dataset.pagesec;feedback("jump");renderPhone(true);return}
  var ac=t.closest("[data-accsec]");if(ac){var id=ac.dataset.accsec;S.openSecs[id]=!S.openSecs[id];feedback("tap");renderPhone(true);return}
  var nb=t.closest("#navbar span");if(nb&&S.view==="card"){var idx=Array.prototype.indexOf.call(nb.parentNode.children,nb),vis=S.sections.filter(function(s){return s.shown&&S.poiSec[s.id]!=="hide"}),sec=vis[idx];if(!sec)return;
    feedback("jump");S.activeSec=sec.id;S.openSecs[sec.id]=true;if(S.stop!=="full"&&S.ctl.jump.opens==="full")S.stop="full";else if(S.stop==="peek")S.stop="half";renderPhone(true);
    var el=scr.querySelector('[data-ref="sec:'+sec.id+'"]');if(el)scr.scrollTo({top:el.offsetTop-60,behavior:S.ctl.jump.motion==="smooth"&&!S.reduce?"smooth":"auto"});return}
  if(t.closest(".locpin")||t.closest("[data-locate]")){feedback("locate");S.stop="peek";renderPhone(false);var m=ph.querySelector('[data-marker="painting"]');if(m){m.classList.remove("found");void m.offsetWidth;m.classList.add("found")}return}
  var kq=t.closest(".kq button");if(kq)feedback(kq.dataset.ok?"right":"wrong");
  if(t.closest(".bprev"))feedback("tap")},true);
/* captures: #pshot=<stop>|<scroll px> */
if(location.hash.indexOf("#pshot=")===0)setTimeout(function(){var p=decodeURIComponent(location.hash.slice(7)).split("|");S.reduce=true;S.view="card";S.stop=p[0]||"half";renderAll(false);setTimeout(function(){scr.scrollTop=+(p[1]||0);barAfterHero();topBand()},250)},50);
