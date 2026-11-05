/* Loading & states: every state in every look (S.states, stateShow). */
S.states={loading:"skeleton",lost:"frame",empty:"mascot",offline:"banner",done:"button",after:"0.3"};
var STATES=[["loading","Loading",["skeleton","spinner","mascot","tiles","scan"],["Skeleton","Spinner","Mascot","Azulejo tiles","AR scan"],"While the card or a page loads: nothing for the first moment (Show after), then the look; after 4 s it adds “Still loading”. Skeleton: the card's own shapes, shimmering. Azulejo tiles: nine tiles turn over one by one. AR scan: a scan line and points over the camera while the wall is read. Reduce Motion: still shapes and a static line."],
 ["lost","Wall not found",["frame","mascot","simple"],["AR frame","Mascot","Simple"],"The camera cannot see the wall, or lost it: what to do, in one line. AR frame: four corners breathe where the tiles should be, and the phone tilts gently. It goes as soon as the wall is found."],
 ["empty","Nothing here",["mascot","simple"],["Mascot","Simple"],"A list with nothing to show (the Contents sheet's filters, a search): why, and one button that undoes it."],
 ["offline","Offline",["banner","mascot"],["Banner","Mascot"],"No connection: a slim banner under the top bar (what was opened still works), or the mascot with Try again where a page cannot load."],
 ["done","Done",["button","toast","mascot"],["On the button","Toast","Mascot bubble"],"After an action that saves (an answer, a vote, a collected item). On the button: the button itself turns into ✓ Saved for 1.6 s, where the eye already is. Toast: a pill over the bar. Mascot bubble: the mascot says it."]];
function tiles9(){var h="";for(var i=0;i<9;i++)h+='<i style="--d:'+(i%3+Math.floor(i/3))*0.12+'s"></i>';return'<div class="st-tiles" aria-hidden="true">'+h+'</div>'}
function stMascot(cls){return'<span class="st-mascot '+(cls||"")+'" aria-hidden="true">'+MASCOT+'</span>'}
function stSheetBox(el){var top=parseFloat(scr.style.top)||0;el.style.top=top+"px";el.style.left=scr.offsetLeft+"px";el.style.width=scr.offsetWidth+"px";el.style.height=(ph.offsetHeight-top)+"px"}
function stateShow(kind,ms){stateHide();var look=S.states[kind],el=document.createElement("div");el.className="stv st-"+kind+" look-"+look;el.setAttribute("role",kind==="done"?"status":"alert");var h="";
  if(kind==="loading"){
    if(look==="skeleton")h='<div class="sk sk-hero"></div><div class="sk-pad"><div class="sk sk-ey"></div><div class="sk sk-h"></div><div class="sk sk-l"></div><div class="sk sk-l s"></div><div class="sk sk-card"></div><div class="sk sk-card"></div></div>';
    else if(look==="spinner")h='<div class="st-mid"><span class="st-spin" aria-hidden="true"></span><b>'+TXH("state_loading")+'</b></div>';
    else if(look==="mascot")h='<div class="st-mid">'+stMascot("hop")+'<b>'+TXH("state_loading")+'</b></div>';
    else if(look==="tiles")h='<div class="st-mid">'+tiles9()+'<b>'+TXH("state_loading")+'</b></div>';
    else h='<div class="st-scanline" aria-hidden="true"></div><div class="st-dots" aria-hidden="true">'+[[22,30],[64,22],[40,48],[78,56],[30,70],[58,78]].map(function(p,i){return'<i style="left:'+p[0]+'%;top:'+p[1]+'%;--d:'+i*0.25+'s"></i>'}).join("")+'</div><div class="st-pill">'+TXH("state_loading")+'</div>';
    var slow='<div class="st-slow">'+TXH("state_slow")+'</div>';h=h.indexOf('<div class="st-mid">')===0?h.replace(/<\/div>$/,slow+'</div>'):h+slow}/* in the centred column, under Loading; else at the foot */
  else if(kind==="lost"){var txt='<b>'+TXH("state_lost_title")+'</b><span>'+TXH("state_lost_line")+'</span>';
    h=look==="frame"?'<div class="st-frame" aria-hidden="true"><i></i><i></i><i></i><i></i></div><div class="st-phone" aria-hidden="true"></div><div class="st-card">'+txt+'</div>':
      look==="mascot"?'<div class="st-card">'+stMascot("look")+txt+'</div>':'<div class="st-card">'+sicon("visit",28)+txt+'</div>'}
  else if(kind==="empty")h='<div class="st-mid">'+(look==="mascot"?stMascot("tilt"):sicon("visit",30))+'<b>'+TXH("state_empty_title")+'</b><span>'+TXH("state_empty_line")+'</span><button type="button" class="st-btn" data-stclose="1">'+TXH("state_empty_action")+'</button></div>';
  else if(kind==="offline")h=look==="banner"?'<div class="st-banner"><span><b>'+TXH("state_offline_title")+'</b> · '+TXH("state_offline_line")+'</span><button type="button" data-stclose="1">'+TXH("state_retry")+'</button></div>':
      '<div class="st-mid">'+stMascot("tilt")+'<b>'+TXH("state_offline_title")+'</b><span>'+TXH("state_offline_line")+'</span><button type="button" class="st-btn" data-stclose="1">'+TXH("state_retry")+'</button></div>';
  else if(kind==="done")h=look==="button"?'<button type="button" class="st-morph"><span class="a">'+(S.lang==="pt"?"Guardar resposta":"Save my answer")+'</span><span class="b">✓ '+TXH("state_done")+'</span></button>':
      look==="toast"?'<div class="st-toast">✓ '+TXH("state_done")+'</div>':'<div class="st-bubble">'+stMascot("hop")+'<span>'+TXH("state_done")+'!</span></div>';
  el.innerHTML=h;ph.appendChild(el);
  if(kind==="loading"&&look!=="scan"||kind==="empty"||kind==="offline"&&look==="mascot")stSheetBox(el);
  if(kind==="offline"&&look==="banner")el.style.top=(parseFloat(scr.style.top)||0)+(S.stop==="full"?64:0)+"px";
  if(kind==="done"){var nb=document.getElementById("navbar"),p=ph.getBoundingClientRect(),k=p.height/ph.offsetHeight,b=24;if(nb&&getComputedStyle(nb).display!=="none")b=Math.round((p.bottom-nb.getBoundingClientRect().top)/k)+14;el.style.bottom=b+"px";
    if(look==="button")setTimeout(function(){var m=el.querySelector(".st-morph");if(m)m.classList.add("done")},700)}
  if(kind==="loading")S.stSlow=setTimeout(function(){el.classList.add("slow")},4000);
  S.stTimer=setTimeout(stateHide,ms||(kind==="loading"?5600:kind==="done"?2600:4800))}
function stateHide(){clearTimeout(S.stTimer);clearTimeout(S.stSlow);ph.querySelectorAll(".stv").forEach(function(x){x.remove()})}
ph.addEventListener("click",function(e){if(e.target.closest(".stv")){e.stopPropagation();if(e.target.closest("[data-stclose]")||!e.target.closest(".st-morph"))stateHide()}},true);
/* Try: the phone where the state belongs (the wall for the camera's states, the card for the rest) */
function stateTry(kind){stateHide();S.settingsOpen=false;S.view="card";S.stop=kind==="lost"||kind==="loading"&&S.states.loading==="scan"?"closed":kind==="done"?"full":"half";renderPhone(false);
  if(kind==="done")scr.scrollTop=scr.querySelector(".hero").offsetHeight+200;setTimeout(function(){stateShow(kind)},kind==="done"?500:400)}
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var hn=d.querySelector('[data-fo="s5hints"]');if(!hn)return d.innerHTML;
  var body='<table class="tbl"><thead><tr><th style="width:22%">State</th><th>Look</th><th style="width:44px"></th><th style="width:30px"></th></tr></thead><tbody>'+
    STATES.map(function(s){return'<tr><td>'+esc(s[1])+'</td><td>'+pseg("states."+s[0],s[2],s[3])+'</td><td><button class="del" type="button" data-sttry="'+s[0]+'">Try</button></td><td>'+help(s[4])+'</td></tr>'}).join("")+'</tbody></table>'+
    row("Loader shows after",pseg("states.after",["0.2","0.3","0.5"],["0.2 s","0.3 s","0.5 s"]),"A loader never flashes: nothing shows for a load quicker than this, and once shown it stays at least 0.5 s.");
  hn.insertAdjacentHTML("afterend",comp("s5states","loading, the wall not found, nothing here, offline, done",body));var f=innerOf(d,"s5states");if(f)f.insertAdjacentHTML("beforeend",ownbar("s5states")+ownPanels("s5states"));
  return d.innerHTML});
ed.addEventListener("click",function(e){var t=e.target.closest("[data-sttry]");if(t){e.stopPropagation();stateTry(t.dataset.sttry)}},true);
FOCUS.s5states=["card",".hero","half"];
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("ctl\\.jump\\.","ctl\\.jump\\.|states\\."));
