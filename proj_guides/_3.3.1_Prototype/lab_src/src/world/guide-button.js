/* The guide's button (✦) and Azu beside it (fabLayer): the ✦ is the visitor's own way in, always in one place, its menu (the tour, Ask, Ask about the view, the guide's settings), its label folding once seen, during a tour the progress ring and the dock shown or hidden; Azu beside it takes the initiative (its remarks on what the camera sees, the hint that points at the ✦). */
S.guideCfg.gbtn="both";S.guideCfg.gbIcon="star";S.guideCfg.gbSide="left";S.guideCfg.gbFold=true;
G.fab={menu:false,bub:null,last:0,cen:null,since:0,count:0};G.gb={shown:0,folded:false,hinted:false,lastAct:0};
COMP.s7gbtn=["Guide","Guide button"];
var GBSTAR='<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M11 3.2c.65 4.9 3.55 7.8 8.4 8.4-4.85.65-7.75 3.55-8.4 8.4-.65-4.85-3.55-7.75-8.4-8.4 4.85-.6 7.75-3.5 8.4-8.4Z"/><path d="M19 2.2c.25 1.45.95 2.15 2.4 2.4-1.45.25-2.15.95-2.4 2.4-.25-1.45-.95-2.15-2.4-2.4 1.45-.25 2.15-.95 2.4-2.4Z"/></svg>',
  GBFACE='<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3.2" y="3.2" width="17.6" height="17.6" rx="4.6" fill="#F4EEE3" stroke="#1F4E9C" stroke-width="1.8"/><circle cx="9.2" cy="11" r="1.7" fill="#1A1814"/><circle cx="14.8" cy="11" r="1.7" fill="#1A1814"/><path d="M9.3 15q2.7 2.1 5.4 0" stroke="#1A1814" stroke-width="1.6" fill="none" stroke-linecap="round"/></svg>',
  GBICO={tour:'<path d="M8 5l11 7-11 7z"/>',ask:'<path d="M5 6h14v9H9l-4 3.5z"/>',view:'<path d="M3 12s3.5-6 9-6 9 6 9 6-3.5 6-9 6-9-6-9-6z"/><circle cx="12" cy="12" r="2.6"/>',settings:'<path d="M4 7h9M17 7h3M4 17h3M11 17h9"/><circle cx="15" cy="7" r="2"/><circle cx="9" cy="17" r="2"/>'};
function gbOn(){return S.guideCfg.gbtn!=="azu"}
function gbAzu(){return S.guideCfg.gbtn!=="star"}
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
function fabLayer(){emitLatestFirst("fabLayer.before");ph.querySelectorAll(".gbtn-wrap:not(.intour),.azfabbub,.gbmenu").forEach(function(x){x.remove()});if(S.stop!=="closed")G.fab.bub=null;
  if(S.view!=="card"||G.on||S.stop==="peek"||(G.cam.open&&S.stop==="closed")||(S.stop!=="closed"&&heroStartSeen())){G.fab.menu=false;return}
  ph.insertAdjacentHTML("beforeend",gbWrapHtml(false));
  if(G.fab.menu){ph.insertAdjacentHTML("beforeend",gbMenuHtml());placeNear(ph.querySelector(".gbmenu"))}
  if(G.fab.bub&&S.stop==="closed"&&gbAzu())showFabBub(G.fab.bub);gbClear()}
/* the ✦ and Azu keep clear of what shares the foot of the screen: on the wall the hint (Tap a marker…) rises above them, and gives
   way to Azu's remark while it shows (one invitation at a time); on the card,
   on the Locate Button's side, they rise above its pin */
function gbClear(){var w=ph.querySelector(".gbtn-wrap:not(.intour)"),tip=ph.querySelector(".walltip");if(tip){tip.style.bottom=w&&S.stop==="closed"?(ph.offsetHeight-w.offsetTop+10)+"px":"";tip.style.visibility=ph.querySelector(".azfabbub")?"hidden":""}
  var lp=ph.querySelector(".locpin");if(!w||!lp||S.stop==="closed"||getComputedStyle(lp).display==="none")return;var a=w.getBoundingClientRect(),b=lp.getBoundingClientRect(),q=ph.getBoundingClientRect().height/ph.offsetHeight;
  if(a.right>b.left&&a.left<b.right&&a.bottom>b.top&&a.top<b.bottom)w.style.bottom=Math.round((ph.getBoundingClientRect().bottom-b.top)/q+8)+"px"}
/* the label says "Guide" for 4 s of being seen, then folds into the ✦: the Locate Button's rule (card/locate.js's seenLabel) */
seenLabel({label:function(){return ph.querySelector(".gbtn:not(.tour) .gb-lab")},live:function(){return S.guideCfg.gbFold&&S.guideCfg.gbIcon!=="word"&&S.view==="card"&&S.stop!=="peek"},
  done:function(){return G.gb.folded},add:function(ms){return G.gb.shown+=ms},fold:function(){G.gb.folded=true;var l=ph.querySelector(".gbtn:not(.tour) .gb-lab");if(l)l.classList.add("fold")}});
/* Azu's remark on the wall: a small bubble beside Azu, a tap acts on it */
function showFabBub(bb){var o=ph.querySelector(".azfabbub");if(o)o.remove();var d=document.createElement("button");d.type="button";d.className="azfabbub"+(bb.act?" act":"");d.dataset.fabbub="1";d.textContent=bb.text;ph.appendChild(d);placeNear(d)}
/* a bubble or the menu just above the ✦ and Azu, on their side */
function placeNear(d){var w=ph.querySelector(".gbtn-wrap");if(!w)return;var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,r=w.getBoundingClientRect();
  d.style.bottom=Math.round((pr.bottom-r.top)/q+8)+"px";if(S.guideCfg.gbSide==="right"){d.style.right=Math.round((pr.right-r.right)/q)+"px";d.style.left="auto"}else{d.style.left=Math.round((r.left-pr.left)/q)+"px";d.style.right="auto"}}
/* ----- the ✦'s menu: Start (or Resume) the tour, Ask by typing or talking, Ask about the view (on the wall), the guide's settings ----- */
function gbItems(){var wall=S.stop==="closed",out=[];if(!wall||S.camCfg.wall==="ribeira")out.push("tour");out.push("ask");if(wall&&S.camCfg.on)out.push("view");out.push("settings");return out}
function gbMenuHtml(){var c=camView().central;return'<div class="gbmenu" role="menu" aria-label="'+TXH("gb_name")+'">'+gbItems().map(function(k){
    var t=k==="tour"?(G.resumeAt?TXH("gb_resume")+(G.resumeAt.title?'<small>'+esc(G.resumeAt.title)+'</small>':""):TXH("gb_start")):k==="view"?esc(c?fmt(TXT("gb_view_about"),c.name):TXT("gb_view")):TXH("gb_"+k);
    return'<button type="button" role="menuitem" data-gbgo="'+k+'"><svg viewBox="0 0 24 24" aria-hidden="true">'+GBICO[k]+'</svg><span>'+t+'</span></button>'}).join("")+'</div>'}
function gbMenu(open){G.fab.menu=open==null?!G.fab.menu:open;fabLayer()}
function gbGo(k){G.fab.menu=false;
  if(k==="tour"){if(G.resumeAt)resumeTourAt();else startTour();return}
  if(k==="ask"){startTour();pauseTour();G.askOpen=true;S.dockH="full";renderGuide();var i=ph.querySelector("#gq");if(i)i.focus({preventScroll:true});return}
  if(k==="view"){G.fab.bub=null;G.cam.open=true;renderPhone(true);var q=ph.querySelector("#caq");if(q)q.focus({preventScroll:true});return}
  if(k==="settings"){S.settingsOpen=true;renderPhone(true);var v=ph.querySelector(".vset"),g=v&&v.querySelector(".vguide");if(g){v.scrollTop=Math.max(0,g.offsetTop-12);g.classList.remove("pfocus");void g.offsetWidth;g.classList.add("pfocus");setTimeout(function(){g.classList.remove("pfocus")},1800)}}}
/* a tour from a card at Half: the card rises to Full first (lowered again by the visitor, the tour goes on there) */
on("startTour.before",function(){G.fab.menu=false;ph.querySelectorAll(".gbtn-wrap,.gbmenu,.azfabbub").forEach(function(x){x.remove()});if(S.view==="card"&&S.stop!=="full"){S.stop="full";renderPhone(true)}});
on("stopTour.after",function(){ph.querySelectorAll(".gbtn-wrap.intour").forEach(function(x){x.remove()});fabLayer()});
/* ----- during a tour: the ✦ above the dock (Mini, Peek) or where the dock was (Compact), with the progress ring and, at Compact, a pause; a tap shows or hides the dock ----- */
on("renderGuide.after",function(){var o=ph.querySelector(".gbtn-wrap.intour");if(o)o.remove();if(!G.on||!gbOn()||!dockOn()||S.dockH==="full")return;
  ph.insertAdjacentHTML("beforeend",gbWrapHtml(true));var w=ph.querySelector(".gbtn-wrap.intour"),d=ph.querySelector(".gdock");w.style.bottom=(d?ph.offsetHeight-d.offsetTop+8:22)+"px";placeBubble()});
setInterval(function(){if(!G.on)return;var c=ph.querySelector(".gbtn.tour .pg"),b=ph.querySelector(".gbtn.tour .gb-bar"),p=gbProgress();if(c)c.style.strokeDasharray=(GBC*p).toFixed(1)+" "+GBC.toFixed(1);if(b)b.style.width=(p*100).toFixed(1)+"%";
  var w=ph.querySelector(".gbtn-wrap.intour"),d=ph.querySelector(".gdock");if(w)w.style.bottom=(d?ph.offsetHeight-d.offsetTop+8:22)+"px"},200);
/* the bubble above the dock moves beside the ✦ (Mini, Peek), or above it (Compact) */
setInterval(function(){var b=ph.querySelector(".gbubble"),w=ph.querySelector(".gbtn-wrap.intour");if(!b)return;b.style.left=w&&ph.querySelector(".gdock")&&S.guideCfg.gbSide==="left"?(w.offsetLeft+w.offsetWidth+8)+"px":"";b.style.right=w&&ph.querySelector(".gdock")&&S.guideCfg.gbSide==="right"?(ph.offsetWidth-w.offsetLeft+8)+"px":""},150);
/* ----- the taps: the ✦ (its menu, or during a tour the dock), a menu item, Azu's remark; a tap elsewhere closes the menu ----- */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gbtn],[data-gbgo],[data-fabbub]");
  if(!t){if(G.fab.menu&&!e.target.closest(".gbmenu,.azfab")){G.fab.menu=false;fabLayer()}return}e.stopPropagation();
  if(t.dataset.gbtn){if(G.on){S.dockH=S.dockH==="compact"?"mini":"compact";G.askOpen=false;renderGuide()}else gbMenu();return}
  if(t.dataset.gbgo){gbGo(t.dataset.gbgo);return}
  var bb=G.fab.bub;G.fab.bub=null;if(!bb){fabLayer();return}var p=S.wallPois.filter(function(x){return x.id===bb.id})[0];
  if(p&&p.card){S.stop="half";renderPhone(false);startTour();return}G.cam.open=true;renderPhone(true);camAsk("",p&&p.year?"oldest":"what");camLight([bb.id])},true);
/* what a tap on Azu does (mascot/azu2d.js asks): with the ✦ beside it, Mascot > Tap on Azu; as the button itself (Azu only), the ✦'s part */
function azuTapAct(){var c=S.guideCfg;if(c.gbtn==="azu")return G.on?(S.dockH==="compact"?"dock":"play"):"menu";if(c.azuTap==="menu")return G.on?"dock":"menu";return c.azuTap}
function azuTapDo(act){if(act==="menu"){gbMenu();return}if(act==="dock"){S.dockH=S.dockH==="compact"?"mini":"compact";renderGuide();return}if(act==="point"){if(!gbHint(true))azPoke();return}azPoke()}
/* ----- Azu's comments: watching where the camera points; when a point settles in the middle, a short, tappable remark ----- */
setInterval(function(){if(!S.camCfg.comments||!gbAzu()||S.view!=="card"||S.stop!=="closed"||G.on||G.cam.open)return;var v=camView(),c=v.central,now=performance.now();
  if(!c){G.fab.cen=null;return}if(G.fab.panAt&&now-G.fab.panAt<700)return;if(c.id!==G.fab.cen){G.fab.cen=c.id;G.fab.since=now;return}
  if(now-G.fab.since<1200||now-G.fab.last<S.camCfg.every*1000||G.fab.count>=12||G.fab.said===c.id)return;
  var p=S.wallPois.filter(function(x){return x.id===c.id})[0],k=["fab_comment_1","fab_comment_2","fab_comment_3"][G.fab.count%3];
  G.fab.bub={id:c.id,text:fmt(TXT(p&&p.card?"fab_comment_card":k),c.name),act:true};G.fab.last=now;G.fab.count++;G.fab.said=c.id;fabLayer();var f=ph.querySelector(".azfab");if(f&&!S.reduce){f.classList.remove("hop");void f.offsetWidth;f.classList.add("hop")}
  clearTimeout(G.fab.t);G.fab.t=setTimeout(function(){G.fab.bub=null;fabLayer()},6000)},400);
/* the card's scroll decides whether the hero's Start is still in sight */
var fabT=0;scr.addEventListener("scroll",function(){if(G.on||S.stop==="closed")return;clearTimeout(fabT);fabT=setTimeout(function(){var had=!!ph.querySelector(".gbtn-wrap"),want=!heroStartSeen();if(had!==want)fabLayer()},120)},{passive:true});
/* and every 400 ms (a card still rising, a stop changed by a drag): the button follows what is on the screen */
setInterval(function(){if(G.on||S.view!=="card"||S.stop==="closed"||S.stop==="peek")return;var had=!!ph.querySelector(".gbtn-wrap"),want=!heroStartSeen();if(had!==want)fabLayer();else if(had)gbClear()},400);
/* ----- the hint: Azu points at the ✦ and says "Need anything? Tap ✦". By a tap (Tap on Azu = Points to the ✦ button) whenever asked;
   by itself at most once a visit, after a few pokes or when the visitor seems idle (25 s), Lively only, never over a sentence; under
   Reduce Motion nothing moves (the words only). Unity: the Rive state machine's point input aimed at the button's world box ----- */
function gbHint(byVisitor){var b=ph.querySelector(".gbtn"),az=azuEl();if(!b||!az||!gbAzu()||!gbOn())return false;
  if(!byVisitor){if(G.gb.hinted||S.guideCfg.life!=="lively"||S.m.solemn||G.utter||G.timer||G.recAudio||G.moment)return false;G.gb.hinted=true}
  if(!S.reduce){if(az.classList.contains("azfab")){az.classList.add("point-gb");clearTimeout(G.gb.pt);G.gb.pt=setTimeout(function(){az.classList.remove("point-gb")},2800)}
    else G.pointTo={till:performance.now()+2800,at:function(){var e=ph.querySelector(".gbtn");if(!e)return null;var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,r=e.getBoundingClientRect();return{x:(r.left+r.width/2-pr.left)/q,y:(r.top+r.height/2-pr.top)/q}}}}
  azBubble(fmt(TXT("gb_hint"),S.guideCfg.gbIcon==="star"?"✦":TXT("gb_label")),3200);G.gb.hintAt=performance.now();return true}
["pointerdown","wheel","keydown"].forEach(function(t){ph.addEventListener(t,function(){G.gb.lastAct=performance.now()},{capture:true,passive:true})});
setInterval(function(){if(G.gb.hinted||!gbAzu()||!gbOn()||!ph.querySelector(".gbtn")||!azuEl())return;var idle=!G.on&&G.gb.lastAct&&performance.now()-G.gb.lastAct>25000;if((G.pokes||0)>=4||idle)gbHint(false)},1000);
/* ----- the guide's settings, where the visitor's Settings live: the voice, its speed and the captions (Who's exploring is there already) ----- */
on("settingsPanel.html",function(h){var c=S.guideCfg,o=function(k,v,lab){return'<button type="button" data-gset="'+k+'|'+v+'" aria-pressed="'+(String(c[k])===v)+'">'+esc(lab)+'</button>'};
  var g='<div class="vguide"><small>'+TXH("settings_guide")+'</small><div class="vo">'+o("voice","recorded",TXT("settings_voice_rec"))+o("voice","device",TXT("settings_voice_dev"))+'</div>'+
    '<div class="vo">'+o("speed","0.85",TXT("settings_slower"))+o("speed","1",TXT("settings_normal"))+o("speed","1.2",TXT("settings_faster"))+'</div><div class="vo">'+o("captions","true",TXT("settings_cc_on"))+o("captions","false",TXT("settings_cc_off"))+'</div></div>';
  var i=h.indexOf("<small>"+TXH("settings_appearance"));if(i<0)i=h.indexOf("<small>");return i<0?h:h.slice(0,i)+g+h.slice(i)});
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gset]");if(!t)return;e.stopPropagation();var q=t.dataset.gset.split("|");S.guideCfg[q[0]]=q[0]==="speed"?parseFloat(q[1]):pval(q[1]);renderAll(true)},true);
/* the ✦ and Azu as the card shows them reach the phone's signature */
on("modelSig.sig",function(h){if(!sigWant(/guideCfg\.(gb|azuTap)/))return h;var m=G.fab.menu;G.fab.menu=false;var x="#gb:"+gbWrapHtml(false);G.fab.menu=m;return h+x});

/* Editor: 6 · Guide > Guide button, after Guide panel */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var dk=d.querySelector('[data-fo="s7dock"]');if(!dk)return d.innerHTML;
  dk.insertAdjacentHTML("afterend",comp("s7gbtn","the visitor's own way in, always in one place, and Azu beside it",
    row("Guide button",pseg("guideCfg.gbtn",["both","azu","star"],["✦ and Azu beside it","Azu only","✦ only"]),"✦ and Azu beside it: two roles. The ✦ is the visitor's: a tap opens its menu (Start or Resume the tour, Ask by typing or talking, Ask about the view on the wall, the guide's settings); during a tour it shows the progress as a ring and a tap shows or hides the dock. Azu beside it takes the initiative (its remarks on the view, the hint) and a tap on Azu plays (Mascot > Tap on Azu). Azu only: Azu is the button, as before: a tap opens the menu, three taps and a long press still play; during a tour Azu is the guide, and a tap at Compact brings the dock back. ✦ only: no Azu outside the tour, so no remarks on the wall. On the card it shows once the hero's Start the guide is out of sight; on the wall, in the corner.")+
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
