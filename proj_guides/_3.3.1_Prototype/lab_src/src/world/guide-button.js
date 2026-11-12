/* The guide's button (✦) and Azu beside it (fabLayer): the ✦ is the visitor's own way in, always in one place, its menu (the tour, Ask, Ask about the view, the guide's settings; opening it is menu.opened), its label folding once seen, during a tour the progress ring and the dock shown or hidden; Azu beside it takes the initiative through the guidance (Guide › Guidance: its remarks on what the camera sees, Content › What's in view; the hint that points at the ✦, App › The wall › The ✦). */
S.guideCfg.gbtn="both";S.guideCfg.gbIcon="star";S.guideCfg.gbSide="left";S.guideCfg.gbFold=true;
G.fab={menu:false};G.gb={shown:0,folded:false};
COMP.s7gbtn=["Guide","Guide button"];
var GBSTAR='<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M11 3.2c.65 4.9 3.55 7.8 8.4 8.4-4.85.65-7.75 3.55-8.4 8.4-.65-4.85-3.55-7.75-8.4-8.4 4.85-.6 7.75-3.5 8.4-8.4Z"/><path d="M19 2.2c.25 1.45.95 2.15 2.4 2.4-1.45.25-2.15.95-2.4 2.4-.25-1.45-.95-2.15-2.4-2.4 1.45-.25 2.15-.95 2.4-2.4Z"/></svg>',
  GBFACE='<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3.2" y="3.2" width="17.6" height="17.6" rx="4.6" fill="#F4EEE3" stroke="#1F4E9C" stroke-width="1.8"/><circle cx="9.2" cy="11" r="1.7" fill="#1A1814"/><circle cx="14.8" cy="11" r="1.7" fill="#1A1814"/><path d="M9.3 15q2.7 2.1 5.4 0" stroke="#1A1814" stroke-width="1.6" fill="none" stroke-linecap="round"/></svg>',
  GBICO={again:'<path d="M4.5 12a7.5 7.5 0 1 0 2.2-5.3"/><path d="M4.5 4.5v4h4"/>',quiz:'<path d="M12 3.5l2.6 5.3 5.8.8-4.2 4.1 1 5.8L12 16.8l-5.2 2.7 1-5.8-4.2-4.1 5.8-.8z"/>',tour:'<path d="M8 5l11 7-11 7z"/>',ask:'<path d="M5 6h14v9H9l-4 3.5z"/>',view:'<path d="M3 12s3.5-6 9-6 9 6 9 6-3.5 6-9 6-9-6-9-6z"/><circle cx="12" cy="12" r="2.6"/>',settings:'<path d="M4 7h9M17 7h3M4 17h3M11 17h9"/><circle cx="15" cy="7" r="2"/><circle cx="9" cy="17" r="2"/>',
  how:'<circle cx="12" cy="12" r="8.5"/><path d="M9.6 9.6a2.5 2.5 0 1 1 3.4 2.3c-.7.3-1 .8-1 1.5v.4"/><path d="M12 16.6v.4"/>',
  /* round 9: Things to try, a checklist */
  try:'<path d="M4 7.5l1.8 1.8L9.2 6"/><path d="M12.5 8h7.5"/><path d="M4 15.5l1.8 1.8 3.4-3.3"/><path d="M12.5 16h7.5"/>',
  /* round 17c: What next? (a compass), after a tour ended on this point */
  next:'<circle cx="12" cy="12" r="8.5"/><path d="M15.4 8.6l-2 5.2-4.8 1.6 2-5.2z"/>'};
/* guidance round 15b, the developer (2026-11-11): Mascot Off (Guide › 1 · Azu › Mascot) is the ✦ alone: no Azu picture beside it, on the
   card and on the wall (Guide button = Azu only shows the ✦ then); a guide bubble with no Azu beside it carries a small ✦ (guidance/present.js) */
function gbOn(){return S.guideCfg.gbtn!=="azu"||S.guideCfg.mascot===false}
function gbAzu(){return S.guideCfg.gbtn!=="star"&&S.guideCfg.mascot!==false}
/* ----- the ✦: bottom left (or right), above the Navigation Bar on the card once the hero's Start the guide is out of sight (the hero is the
   way in until then), in the corner on the wall; during a tour above the dock, or at Compact where the dock was ----- */
function heroStartSeen(){var g=scr.querySelector(".hero .gstart");if(!g)return false;var r=g.getBoundingClientRect(),b=scr.getBoundingClientRect();return r.bottom>b.top&&r.top<b.bottom}
/* during a tour: how far it has come, the step done and the share of the line said */
function gbProgress(){var n=S.guideSteps.length||1,w=(G.words||[]).length,k=G.hl&&G.hl.k>=0?G.hl.k+1:0;return Math.min(1,(G.i+(w?Math.min(1,k/w):0))/n)}
var GBC=2*Math.PI*22.5;
function gbHtml(tour){var c=S.guideCfg,ic=c.gbIcon,word=ic==="word",p=tour?gbProgress():0,fold=!word&&(tour||c.gbFold&&G.gb.folded);
  return'<button type="button" class="gbtn ic-'+ic+(tour?" tour":"")+'" data-gbtn="1" aria-label="'+TXH(tour?"gb_toggle":"gb_name")+'"'+(tour?' aria-pressed="'+(S.dockH!=="compact")+'"':' aria-haspopup="menu" aria-expanded="'+!!G.fab.menu+'"')+'>'+
    (tour&&!word?'<svg class="gb-ring" viewBox="0 0 52 52" aria-hidden="true"><circle cx="26" cy="26" r="22.5" class="tr"/><circle cx="26" cy="26" r="22.5" class="pg" style="stroke-dasharray:'+(GBC*p).toFixed(1)+' '+GBC.toFixed(1)+'"/></svg>':"")+
    (word?"":'<span class="gb-ic">'+(ic==="face"?GBFACE:GBSTAR)+'</span>')+'<span class="gb-lab'+(fold?" fold":"")+'">'+TXH("gb_label")+'</span>'+(tour&&word?'<i class="gb-bar" style="width:'+(p*100).toFixed(1)+'%"></i>':"")+'</button>'}
function gbWrapHtml(tour){var wall=S.stop==="closed"&&!tour,az=gbAzu()&&!tour?'<button type="button" class="azfab'+(wall?" onwall":"")+'" aria-label="'+TXH("fab_label")+'">'+mascotStill()+(mascotIsModel()?"":'<i class="marm"></i>')+'</button>':"",
    pz=tour&&S.dockH==="compact"?'<button type="button" class="gb-pause" data-g="'+(G.paused?"play":"pause")+'" aria-label="'+(G.paused?TXH("guide_ctrl_play"):TXH("guide_ctrl_pause"))+'">'+ico(G.paused?"play":"pause")+'</button>':"";
  return'<div class="gbtn-wrap side-'+S.guideCfg.gbSide+(wall?" onwall":"")+(tour?" intour":"")+'">'+(gbOn()?gbHtml(tour):"")+pz+az+'</div>'}
function fabLayer(){emitLatestFirst("fabLayer.before");ph.querySelectorAll(".gbtn-wrap:not(.intour),.gbmenu").forEach(function(x){x.remove()});
  if(S.view!=="card"||G.on||G.gb.tour||S.stop==="peek"||(G.cam.open&&S.stop==="closed")||(S.stop!=="closed"&&heroStartSeen())){G.fab.menu=false;gbBand();return}
  ph.insertAdjacentHTML("beforeend",gbWrapHtml(false));
  if(G.fab.menu){ph.insertAdjacentHTML("beforeend",gbMenuHtml());placeNear(ph.querySelector(".gbmenu"))}
  gbClear();gbBand();emit("fabLayer.after")}
/* the ✦ and Azu keep clear of what shares the foot of the screen: on the card, on the Locate Button's side, they rise above its pin */
function gbClear(){var w=ph.querySelector(".gbtn-wrap:not(.intour)");
  var lp=ph.querySelector(".locpin");if(!w||!lp||S.stop==="closed"||getComputedStyle(lp).display==="none")return;var a=w.getBoundingClientRect(),b=lp.getBoundingClientRect(),q=ph.getBoundingClientRect().height/ph.offsetHeight;
  if(a.right>b.left&&a.left<b.right&&a.bottom>b.top&&a.top<b.bottom)w.style.bottom=Math.round((ph.getBoundingClientRect().bottom-b.top)/q+8)+"px"}
/* (guidance round 7, the look fix) on the card the ✦ and Azu never cover words: a band of the card's paper sits behind them, from just
   above them down to the foot (under the Navigation Bar, the ✦ and the Locate Button), so the card's words scroll away under it as under
   the bar; the card's foot gains the band's height, so its last words still rise above it. Unity: a VisualElement band with the card's
   background colour and a ramp texture along its top edge (no USS gradient), placed before the bar in the hierarchy (draw order); the
   ScrollView's bottom padding grows by its height */
/* in a tour the same rule for the tour's ✦ and its caption above the dock: a band of the dock's own colour behind that row (from just above
   the ✦ and the caption down under the dock), so the card's words (the hero's eyebrow at the first step) never sit under them */
function gbTourBand(){var w=ph.querySelector(".gbtn-wrap.intour"),d=ph.querySelector(".gdock"),old=ph.querySelector(".gbband.tour");
  if(!G.on||!w||!d||!w.getClientRects().length){if(old)old.remove();return}var top=w.offsetTop,bb=ph.querySelector(".gbubble");if(bb&&bb.getClientRects().length)top=Math.min(top,bb.offsetTop);
  var b=old||document.createElement("div"),t=Math.round(top-24);b.className="gbband tour";b.setAttribute("aria-hidden","true");b.style.top=t+"px";b.style.setProperty("--gbband",getComputedStyle(d).backgroundColor);if(!old)ph.insertBefore(b,d)}
function gbBand(){var w=ph.querySelector(".gbtn-wrap:not(.intour)"),old=ph.querySelector(".gbband:not(.tour)");
  if(!w||S.view!=="card"||S.stop==="closed"||G.on||G.gb.tour){if(old)old.remove();scr.style.paddingBottom="";return}
  var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,top=Infinity;[].forEach.call(w.children,function(x){var r=x.getBoundingClientRect();if(r.height)top=Math.min(top,(r.top-pr.top)/q)});if(!isFinite(top))return;
  var b=old||document.createElement("div"),t=Math.round(top-24),h=ph.offsetHeight-t;b.className="gbband";b.setAttribute("aria-hidden","true");b.style.top=t+"px";if(!old)ph.insertBefore(b,ph.querySelector("#navbar")||w);
  var pb=Math.max(110,h+12)+"px";if(scr.style.paddingBottom!==pb)scr.style.paddingBottom=pb}
/* the label says "Guide" for 4 s of being seen, then folds into the ✦: the Locate Button's rule (card/locate.js's seenLabel) */
seenLabel({label:function(){return ph.querySelector(".gbtn:not(.tour) .gb-lab")},live:function(){return S.guideCfg.gbFold&&S.guideCfg.gbIcon!=="word"&&S.view==="card"&&S.stop!=="peek"},
  done:function(){return G.gb.folded},add:function(ms){return G.gb.shown+=ms},fold:function(){G.gb.folded=true;var l=ph.querySelector(".gbtn:not(.tour) .gb-lab");if(l)l.classList.add("fold")}});
/* the menu just above the ✦ and Azu, on their side */
function placeNear(d){var w=ph.querySelector(".gbtn-wrap");if(!w)return;var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,r=w.getBoundingClientRect();
  d.style.bottom=Math.round((pr.bottom-r.top)/q+8)+"px";if(S.guideCfg.gbSide==="right"){d.style.right=Math.round((pr.right-r.right)/q)+"px";d.style.left="auto"}else{d.style.left=Math.round((r.left-pr.left)/q)+"px";d.style.right="auto"}}
/* ----- the ✦'s menu: Start (or Resume) the tour, Ask by typing or talking, Ask about the view (on the wall), How does this work? (Guide ›
   Guidance › How to use the card, In the ✦ menu: on the card it walks the card, on the wall one bubble says how the wall works), Things to
   try (round 9: Guide › Guidance › Overall › Things to try, In the ✦ menu: the checklist of the App items that teach; it opens whatever the
   visitor's Tips are, and while it has a row), the guide's settings ----- */
function gbItems(){var wall=S.stop==="closed",out=[],hw=gdGroupById("howto");if(!wall||S.camCfg.wall==="ribeira"){out.push("tour");/* round 17b: the one offer (a Stop in this visit, or a place kept from an earlier visit): Continue the tour (the tour item) and Start again */if(!G.on&&svOffer())out.push("again")}out.push("ask");if(wall&&S.camCfg.on)out.push("view");if(hw&&hw.on!==false&&hw.inMenu)out.push("how");
  if(S.guidance.tryMenu!==false&&gdTryDefs().length)out.push("try");
  /* guidance round 12: the tour stopped for good before its promised quiz: Your 3 questions (from the steps heard), for the rest of the visit */
  if(typeof gdQuizOwed==="function"&&gdQuizOwed())out.splice(1,0,"quiz");out.push("settings");
  /* round 17c: after a tour ended here, What next? comes first until the visitor leaves the point (guide/tour-end.js) */
  if(!G.on&&typeof teOffered==="function"&&teOffered())out.unshift("next");return out}
function gbMenuHtml(){var c=camView().central;return'<div class="gbmenu" role="menu" aria-label="'+TXH("gb_name")+'">'+gbItems().map(function(k){
    var sv=k==="tour"&&!G.on?svOffer():null,t=k==="again"?TXH("sv_again"):k==="tour"?(sv?esc(svLabel(sv))+(sv.title?'<small>'+esc(sv.title)+'</small>':""):TXH("gb_start")+' · '+esc(tourLength(tourMinutes()))):k==="view"?esc(c?nameIn(TXT("gb_view_about"),"{0}",c.name):TXT("gb_view")):k==="how"?TXH("gd_how"):k==="try"?TXH("gd_try"):k==="quiz"?esc(fmt(TXT("tour_menu_quiz"),gdQuizOwed())):k==="next"?TXH("te_title"):TXH("gb_"+k);
    return'<button type="button" role="menuitem" data-gbgo="'+k+'"><svg viewBox="0 0 24 24" aria-hidden="true">'+GBICO[k]+'</svg><span>'+t+'</span></button>'}).join("")+'</div>'}
function gbMenu(open){var was=G.fab.menu;G.fab.menu=open==null?!G.fab.menu:open;fabLayer();if(G.fab.menu&&!was)emit("menu.opened","the ✦")}
function gbGo(k){G.fab.menu=false;
  if(k==="tour"){if(!G.on&&svOffer())svContinue();else startTour();return}
  if(k==="again"){startTour();return}
  if(k==="next"){teSet("open");return}
  if(k==="ask"){startTour();pauseTour();G.askOpen=true;S.dockH="full";renderGuide();var i=ph.querySelector("#gq");if(i)i.focus({preventScroll:true});return}
  if(k==="how"){fabLayer();gdWalk("the ✦ menu");return}
  if(k==="try"){fabLayer();gdTryOpen("the ✦ menu");return}
  if(k==="quiz"){fabLayer();gdQuizPull();return}
  if(k==="view"){G.cam.open=true;renderPhone(true);emit("view.opened","the ✦ menu");var q=ph.querySelector("#caq");if(q)q.focus({preventScroll:true});return}
  if(k==="settings"){S.settingsOpen=true;renderPhone(true);var v=ph.querySelector(".vset"),g=v&&v.querySelector(".vguide");if(g){v.scrollTop=Math.max(0,g.offsetTop-12);g.classList.remove("pfocus");void g.offsetWidth;g.classList.add("pfocus");setTimeout(function(){g.classList.remove("pfocus")},1800)}}}
/* a tour from a card at Half: the card rises to Full first (lowered again by the visitor, the tour goes on there) */
/* a tour starting: the card's ✦, its Azu and their band go (the tour has its own ✦ above the dock); the redraw that puts the card at Full
   runs before G.on is set, so it is marked as the tour's (G.gb.tour): fabLayer draws no second ✦ and Azu behind the tour's (guidance round 7's look fix) */
on("startTour.before",function(){var b=ph.querySelector(".gbtn-wrap .gbtn");G.morphFrom=b&&b.getClientRects().length?phRect(b):null;G.fab.menu=false;ph.querySelectorAll(".gbtn-wrap,.gbmenu,.gbband").forEach(function(x){x.remove()});scr.style.paddingBottom="";if(S.view==="card"&&S.stop!=="full"){S.stop="full";G.gb.tour=true;try{renderPhone(true)}finally{G.gb.tour=false}}});
on("stopTour.after",function(){ph.querySelectorAll(".gbtn-wrap.intour,.gbband.tour").forEach(function(x){x.remove()});fabLayer()});
/* ----- during a tour: the ✦ where the dock was (Compact), with the progress ring and a pause; a tap shows the dock again, the ✦ growing
   into it. Round 11: while the panel shows (Mini, Peek, Full) the ✦ is hidden: it became the panel (guide/dock.js's dkMorph) ----- */
on("renderGuide.after",function(){var o=ph.querySelector(".gbtn-wrap.intour");if(o)o.remove();if(!G.on||!gbOn()||!dockOn()||S.dockH!=="compact")return;
  ph.insertAdjacentHTML("beforeend",gbWrapHtml(true));var w=ph.querySelector(".gbtn-wrap.intour"),d=ph.querySelector(".gdock");w.style.bottom=(d?ph.offsetHeight-d.offsetTop+8:22)+"px";placeBubble();gbTourBand()});
setInterval(function(){if(!G.on)return;var c=ph.querySelector(".gbtn.tour .pg"),b=ph.querySelector(".gbtn.tour .gb-bar"),p=gbProgress();if(c)c.style.strokeDasharray=(GBC*p).toFixed(1)+" "+GBC.toFixed(1);if(b)b.style.width=(p*100).toFixed(1)+"%";
  var w=ph.querySelector(".gbtn-wrap.intour"),d=ph.querySelector(".gdock");if(w)w.style.bottom=(d?ph.offsetHeight-d.offsetTop+8:22)+"px";gbTourBand()},200);
/* the bubble above the dock moves beside the ✦ (Mini, Peek), or above it (Compact) */
setInterval(function(){var b=ph.querySelector(".gbubble"),w=ph.querySelector(".gbtn-wrap.intour");if(!b)return;b.style.left=w&&ph.querySelector(".gdock")&&S.guideCfg.gbSide==="left"?(w.offsetLeft+w.offsetWidth+8)+"px":"";b.style.right=w&&ph.querySelector(".gdock")&&S.guideCfg.gbSide==="right"?(ph.offsetWidth-w.offsetLeft+8)+"px":""},150);
/* ----- the taps: the ✦ (its menu, or during a tour the dock), a menu item; a tap elsewhere closes the menu ----- */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gbtn],[data-gbgo]");
  if(!t){if(G.fab.menu&&!e.target.closest(".gbmenu,.azfab")){G.fab.menu=false;fabLayer()}return}e.stopPropagation();
  if(t.dataset.gbtn){if(G.on){if(S.dockH==="compact")G.morphFrom=phRect(t);S.dockH=S.dockH==="compact"?"mini":"compact";G.askOpen=false;renderGuide()}else gbMenu();return}
  if(t.dataset.gbgo){gbGo(t.dataset.gbgo);return}},true);
/* what a tap on Azu does (mascot/azu2d.js asks): with the ✦ beside it, Mascot > Tap on Azu; as the button itself (Azu only), the ✦'s part.
   Points to the ✦ button pulls the guidance's The ✦ at once (guidance/service.js's gdPull; no ✦ to point at: it plays) */
function azuTapAct(){var c=S.guideCfg;if(c.gbtn==="azu")return G.on?(S.dockH==="compact"?"dock":"play"):"menu";if(c.azuTap==="menu")return G.on?"dock":"menu";return c.azuTap}
function azuTapDo(act){if(act==="menu"){gbMenu();return}if(act==="dock"){S.dockH=S.dockH==="compact"?"mini":"compact";renderGuide();return}if(act==="point"){if(!gdPull("gbhint","Tap on Azu = Points to the ✦"))azPoke();return}azPoke()}
/* the card's scroll decides whether the hero's Start is still in sight */
var fabT=0;scr.addEventListener("scroll",function(){if(G.on||S.stop==="closed")return;clearTimeout(fabT);fabT=setTimeout(function(){var had=!!ph.querySelector(".gbtn-wrap"),want=!heroStartSeen();if(had!==want)fabLayer()},120)},{passive:true});
/* and every 400 ms (a card still rising, a stop changed by a drag): the button follows what is on the screen */
setInterval(function(){if(G.on||S.view!=="card"||S.stop==="closed"||S.stop==="peek")return;var had=!!ph.querySelector(".gbtn-wrap"),want=!heroStartSeen();if(had!==want)fabLayer();else if(had){gbClear();gbBand()}},400);
/* ----- the guide's settings, where the visitor's Settings live: the voice, its speed and the captions (Who's exploring is there already) ----- */
on("settingsPanel.html",function(h){var c=S.guideCfg,o=function(k,v,lab){return'<button type="button" data-gset="'+k+'|'+v+'" aria-pressed="'+(String(c[k])===v)+'">'+esc(lab)+'</button>'};
  var mu=musicPick(),mo=function(id,lab){return'<button type="button" data-gmus="'+id+'" aria-pressed="'+(mu===id)+'">'+esc(lab)+'</button>'},
    music=c.musicVis===false?"":'<div class="vmusic"><small>'+TXH("settings_music")+'</small><div class="vo vmus">'+mo("off",TXT("settings_music_off"))+musicTracks().map(function(t){return mo(t.id,L(t.name))}).join("")+'</div>'+(musicCredit(mu)?'<small class="vcredit">'+esc(musicCredit(mu))+'</small>':"")+
      '<label class="vvol"><span>'+TXH("settings_music_vol")+'</span><input type="range" min="0" max="100" step="5" data-gmusvol="1" value="'+Math.round(musicVol()*100)+'" aria-label="'+TXH("settings_music_vol")+'"></label>'+(musicCredits().length?'<div class="vcredits"><small>'+TXH("settings_music_credits")+'</small>'+musicCredits().map(function(c){return'<p>'+esc(c)+'</p>'}).join("")+'</div>':"")+'</div>';
  var g='<div class="vguide"><small>'+TXH("settings_guide")+'</small><div class="vo">'+o("voice","recorded",TXT("settings_voice_rec"))+o("voice","device",TXT("settings_voice_dev"))+'</div>'+
    '<div class="vo">'+o("speed","0.85",TXT("settings_slower"))+o("speed","1",TXT("settings_normal"))+o("speed","1.2",TXT("settings_faster"))+'</div><div class="vo">'+o("captions","true",TXT("settings_cc_on"))+o("captions","false",TXT("settings_cc_off"))+'</div>'+music+'</div>';
  var i=h.indexOf("<small>"+TXH("settings_appearance"));if(i<0)i=h.indexOf("<small>");return i<0?h:h.slice(0,i)+g+h.slice(i)});
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gset]");if(!t)return;e.stopPropagation();var q=t.dataset.gset.split("|");S.guideCfg[q[0]]=q[0]==="speed"?parseFloat(q[1]):pval(q[1]);if(q[0]==="speed")S.vis.speed=null;renderAll(true)},true);
/* round 11: the visitor's music (Settings › The guide › Music): Off or a track's feeling, and its volume; it plays at once in a tour */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gmus]");if(!t)return;e.stopPropagation();S.vis.music=t.dataset.gmus;musicApply();ph.querySelectorAll("[data-gmus]").forEach(function(b){b.setAttribute("aria-pressed",String(b.dataset.gmus===S.vis.music))});
  var c=ph.querySelector(".vmusic .vcredit"),w=musicCredit(S.vis.music);if(c&&!w)c.remove();else if(w){if(!c){c=document.createElement("small");c.className="vcredit";ph.querySelector(".vmus").after(c)}c.textContent=w}},true);
ph.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.gmusvol)return;e.stopPropagation();S.vis.musicVol=+t.value;musicSet(false)},true);
/* the ✦ and Azu as the card shows them reach the phone's signature */
on("modelSig.sig",function(h){if(!sigWant(/guideCfg\.(gb|azuTap)/))return h;var m=G.fab.menu;G.fab.menu=false;var x="#gb:"+gbWrapHtml(false);G.fab.menu=m;return h+x});

/* Editor: Guide tab > Guide button, after Guide panel */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var dk=d.querySelector('[data-fo="s7dock"]');if(!dk)return d.innerHTML;
  dk.insertAdjacentHTML("afterend",comp("s7gbtn","the visitor's own way in, always in one place, and Azu beside it",
    row("Guide button",pseg("guideCfg.gbtn",["both","azu","star"],["✦ and Azu beside it","Azu only","✦ only"]),"✦ and Azu beside it: two roles. The ✦ is the visitor's: a tap opens its menu (Start or Resume the tour, Ask by typing or talking, Ask about the view on the wall, the guide's settings); during a tour it shows the progress as a ring and a tap shows or hides the dock. Azu beside it takes the initiative (its remarks on the view and the hint to the ✦, said through Guide › 4 · Azu speaks up) and a tap on Azu plays (Guide › 1 · Azu › Tap on Azu). Azu only: Azu is the button, as before: a tap opens the menu, three taps and a long press still play; during a tour Azu is the guide, and a tap at Compact brings the dock back. ✦ only: no Azu outside the tour; the guide's tips and remarks show as plain bubbles, signed with a small ✦. With Mascot Off (1 · Azu) the ✦ shows alone whatever this says, and the plain bubbles carry the same ✦. On the card it shows once the hero's Start the guide is out of sight; on the wall, in the corner.")+
    row("Icon",pseg("guideCfg.gbIcon",["star","face","word"],["✦","Azu's face","The word"]),"What the button shows: the ✦ (the guide's own sign, gold on the dock's dark), Azu's face (a small tile), or the word (Guide, never folded; during a tour the progress is a line under it).",true)+
    row("Side",pseg("guideCfg.gbSide",["left","right"],["Left","Right"]),"Bottom left (the thumb of a right hand reaches the card's middle; the Locate Button keeps its place) or bottom right. Azu stands beside it, on the inside.",true)+
    row("Label folds after 4 s",pseg("guideCfg.gbFold",["true","false"],["On","Off"]),"On: the button says Guide beside the ✦ for 4 s of being seen (time counts only while it shows), then folds into the ✦, once a visit: the Locate Button's rule and code. Off: the word stays.",true)+
    '<div class="note">Unity: a UI Toolkit Button in the card\'s root, absolutely placed above the Navigation Bar (or the dock); the ring is drawn by generateVisualContent with Painter2D (Arc and Stroke, checked), its end angle the tour\'s progress; the label folds by a USS transition on width and opacity.</div>'));
  var f=innerOf(d,"s7gbtn");if(f)f.insertAdjacentHTML("beforeend",ownbar("s7gbtn")+ownPanels("s7gbtn"));return d.innerHTML});
/* a tap on Azu, and when the label folds (its time seen): behaviour */
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(azuTap|gbFold|"));
/* each row's demo: the card scrolled past the hero, the ✦ and Azu; the label shown again, to fold */
rowDemos({"guideCfg.gbtn":{past:true,ring:".gbtn-wrap"},"guideCfg.gbIcon":{past:true,ring:".gbtn|.gbtn-wrap"},"guideCfg.gbSide":{past:true,ring:".gbtn-wrap"},
  "guideCfg.gbFold":{past:true,act:function(){G.gb.folded=false;G.gb.shown=0;fabLayer()},ring:".gbtn|.gbtn-wrap"}});
