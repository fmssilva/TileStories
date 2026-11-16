/* The card's chrome: the top band, the sticky action, the close and settings buttons, the page foot. */
function topBand(){var b=ph.querySelector(".topband");if(!b){b=document.createElement("div");b.className="topband";ph.appendChild(b)}
  var show=ui.view==="card"&&ui.stop==="full"&&scr.scrollTop>230;b.style.display=show?"":"none";b.className="topband tb-"+D.card.cardCfg.topBand;
  b.innerHTML='<i title="'+TXH("close")+'">&#10005;</i>'+(D.card.ctl.top.title?'<b>'+esc(L(D.poi.hero.title))+'</b>':'<b></b>')+(D.card.ctl.top.settings?'<i title="'+TXH("visitor_settings")+'">&#9776;</i>':'<i style="visibility:hidden"></i>');emit("topBand.after")}
function stickyAction(){var s=ph.querySelector(".sticky");if(s)s.remove();var on=D.card.ctl.sticky==="shown"||(D.card.ctl.sticky==="auto"&&!(D.card.ctl.bar.on&&D.card.ctl.bar.pos==="bottom"));
  if(ui.view==="card"&&on&&(ui.stop==="half"||ui.stop==="full")){s=document.createElement("div");s.className="sticky";s.innerHTML='<button type="button" data-locate="1">'+(TXH("show_on_wall_button"))+'</button>';ph.appendChild(s)}}
scr.addEventListener("scroll",function(){if(ui.view==="card")topBand()},{passive:true});
/* ----- page chrome on pages: breadcrumb, previous / next ----- */
function chromeOf(k){var bd=D.card.blockDefs[k]||{};return bd.chrome==="override"?{crumb:bd.crumb,prevnext:bd.prevnext}:{crumb:D.card.ctl.crumb,prevnext:D.card.ctl.prevnext}}
on("topbar.answer",function(crumb){if((ui._pk?chromeOf(ui._pk).crumb:D.card.ctl.crumb)==="off")return '<div class="topbar"><button type="button" data-go="card" aria-label="'+TXH("page_back")+'">&#8249;</button><span></span></div>'});
function pageFoot(prev,next){if(!(ui._pk?chromeOf(ui._pk).prevnext:D.card.ctl.prevnext))return "";return '<div class="pfoot"><button type="button" data-go="'+prev[0]+'"><small>'+(TXH("page_previous"))+'</small>'+esc(prev[1])+'</button><button type="button" data-go="'+next[0]+'"><small>'+(TXH("page_next"))+'</small>'+esc(next[1])+'</button></div>'}
on("storyScreen.before",function(){ui._pk="Story"});on("storyScreen.html",function(h){h=h+pageFoot(["card",L(secById("about").name)],["gallery",TXT("gallery_name")]);ui._pk=null;return h});
on("galleryScreen.before",function(){ui._pk="Gallery"});on("galleryScreen.html",function(h){h=h+pageFoot(["story",L(DEMO.title)],["card",L(secById("visit").name)]);ui._pk=null;return h});
/* the top band after the hero carries the sliders mark too */
on("topBand.after",function(){var tb=ph.querySelector(".topband");if(tb)tb.querySelectorAll("i").forEach(function(i){if(i.textContent==="\u2630")i.innerHTML=SLIDERS})});
