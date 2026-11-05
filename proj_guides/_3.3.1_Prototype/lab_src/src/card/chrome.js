/* The card's chrome: the top band, the sticky action, the close and settings buttons, the page foot. */
function topBand(){var b=ph.querySelector(".topband");if(!b){b=document.createElement("div");b.className="topband";ph.appendChild(b)}
  var show=S.view==="card"&&S.stop==="full"&&scr.scrollTop>230;b.style.display=show?"":"none";b.className="topband tb-"+S.cardCfg.topBand;
  b.innerHTML='<i title="'+TXH("close")+'">&#10005;</i>'+(S.ctl.top.title?'<b>Ribeira Palace</b>':'<b></b>')+(S.ctl.top.settings?'<i title="'+TXH("visitor_settings")+'">&#9776;</i>':'<i style="visibility:hidden"></i>');emit("topBand.after")}
function stickyAction(){var s=ph.querySelector(".sticky");if(s)s.remove();var on=S.ctl.sticky==="shown"||(S.ctl.sticky==="auto"&&!(S.ctl.bar.on&&S.ctl.bar.pos==="bottom"));
  if(S.view==="card"&&on&&(S.stop==="half"||S.stop==="full")){s=document.createElement("div");s.className="sticky";s.innerHTML='<button type="button" data-locate="1">'+(TXH("show_on_wall_button"))+'</button>';ph.appendChild(s)}}
scr.addEventListener("scroll",function(){if(S.view==="card")topBand()},{passive:true});
/* ----- page chrome on pages: breadcrumb, previous / next ----- */
function chromeOf(k){var bd=S.blockDefs[k]||{};return bd.chrome==="override"?{crumb:bd.crumb,prevnext:bd.prevnext}:{crumb:S.ctl.crumb,prevnext:S.ctl.prevnext}}
on("topbar.answer",function(crumb){if((S._pk?chromeOf(S._pk).crumb:S.ctl.crumb)==="off")return '<div class="topbar"><button type="button" data-go="card" aria-label="'+TXH("page_back")+'">&#8249;</button><span></span></div>'});
function pageFoot(prev,next){if(!(S._pk?chromeOf(S._pk).prevnext:S.ctl.prevnext))return "";return '<div class="pfoot"><button type="button" data-go="'+prev[0]+'"><small>'+(TXH("page_previous"))+'</small>'+esc(prev[1])+'</button><button type="button" data-go="'+next[0]+'"><small>'+(TXH("page_next"))+'</small>'+esc(next[1])+'</button></div>'}
on("storyScreen.before",function(){S._pk="Story"});on("storyScreen.html",function(h){h=h+pageFoot(["card",L(secById("about").name)],["gallery",TXT("gallery_name")]);S._pk=null;return h});
on("galleryScreen.before",function(){S._pk="Gallery"});on("galleryScreen.html",function(h){h=h+pageFoot(["story",L(DEMO.title)],["card",L(secById("visit").name)]);S._pk=null;return h});
/* the top band after the hero carries the sliders mark too */
on("topBand.after",function(){var tb=ph.querySelector(".topband");if(tb)tb.querySelectorAll("i").forEach(function(i){if(i.textContent==="\u2630")i.innerHTML=SLIDERS})});
