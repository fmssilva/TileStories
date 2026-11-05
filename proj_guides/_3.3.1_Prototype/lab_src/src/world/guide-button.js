/* Azu as the guide's button on the wall (fabLayer): its remarks on what the camera sees; a tour raises the card to Full. */
G.fab={menu:false,bub:null,last:0,cen:null,since:0,count:0};
/* ----- Azu as the guide's button: bottom left; on the card, above the bar; on the wall, in the corner ----- */
/* on the card it shows once the hero's Start the guide is off the screen (the hero is the guide's way in until then) */
function heroStartSeen(){var g=scr.querySelector(".hero .gstart");if(!g)return false;var r=g.getBoundingClientRect(),b=scr.getBoundingClientRect();return r.bottom>b.top&&r.top<b.bottom}
function fabLayer(){emitLatestFirst("fabLayer.before");var o=ph.querySelector(".azfab");if(o)o.remove();ph.querySelectorAll(".azfabbub").forEach(function(x){x.remove()});if(S.stop!=="closed")G.fab.bub=null;
  if(!S.camCfg.fab||S.view!=="card"||G.on||S.stop==="peek"||(G.cam.open&&S.stop==="closed")||(S.stop!=="closed"&&heroStartSeen()))return;
  var b=document.createElement("button");b.type="button";b.className="azfab"+(S.stop==="closed"?" onwall":"");b.setAttribute("aria-label",TXT("fab_label"));/* the button draws the 2D Azu: the shader's one canvas belongs to the guide, the peek and the walk */b.innerHTML=MASCOT;ph.appendChild(b);
  if(G.fab.menu&&S.stop!=="closed"){var m=document.createElement("div");m.className="azfabbub menu";m.innerHTML='<button type="button" data-fabgo="tour">▶ '+TXH("fab_tour")+'</button><button type="button" data-fabgo="ask">'+TXH("fab_ask")+'</button>';ph.appendChild(m)}
  if(G.fab.bub&&S.stop==="closed")showFabBub(G.fab.bub)}
function showFabBub(bb){var o=ph.querySelector(".azfabbub");if(o)o.remove();var d=document.createElement("button");d.type="button";d.className="azfabbub"+(bb.act?" act":"");d.dataset.fabbub="1";d.textContent=bb.text;ph.appendChild(d)}
/* a tour from a card at Half: the card rises to Full first (lowered again by the visitor, the tour goes on there) */
on("startTour.before",function(){G.fab.menu=false;var f=ph.querySelector(".azfab");if(f)f.remove();if(S.view==="card"&&S.stop!=="full"){S.stop="full";renderPhone(true)}});
on("stopTour.after",function(){fabLayer()});
ph.addEventListener("click",function(e){var t=e.target.closest(".azfab,[data-fabgo],[data-fabbub]");if(!t)return;e.stopPropagation();
  if(t.classList.contains("azfab")){if(S.stop==="closed"){G.fab.bub=null;G.cam.open=true;renderPhone(true);return}G.fab.menu=!G.fab.menu;fabLayer();return}
  if(t.dataset.fabgo==="tour"){startTour();return}
  if(t.dataset.fabgo==="ask"){startTour();pauseTour();G.askOpen=true;S.dockH="full";renderGuide();var i=ph.querySelector("#gq");if(i)i.focus({preventScroll:true});return}
  var bb=G.fab.bub;G.fab.bub=null;if(!bb){fabLayer();return}var p=S.wallPois.filter(function(x){return x.id===bb.id})[0];
  if(p&&p.card){S.stop="half";renderPhone(false);startTour();return}G.cam.open=true;renderPhone(true);camAsk("",p&&p.year?"oldest":"what");camLight([bb.id])},true);
/* ----- Azu's comments: watching where the camera points; when a point settles in the middle, a short, tappable remark ----- */
setInterval(function(){if(!S.camCfg.comments||!S.camCfg.fab||S.view!=="card"||S.stop!=="closed"||G.on||G.cam.open)return;var v=camView(),c=v.central,now=performance.now();
  if(!c){G.fab.cen=null;return}if(G.fab.panAt&&now-G.fab.panAt<700)return;if(c.id!==G.fab.cen){G.fab.cen=c.id;G.fab.since=now;return}
  if(now-G.fab.since<1200||now-G.fab.last<S.camCfg.every*1000||G.fab.count>=12||G.fab.said===c.id)return;
  var p=S.wallPois.filter(function(x){return x.id===c.id})[0],k=["fab_comment_1","fab_comment_2","fab_comment_3"][G.fab.count%3];
  G.fab.bub={id:c.id,text:fmt(TXT(p&&p.card?"fab_comment_card":k),c.name),act:true};G.fab.last=now;G.fab.count++;G.fab.said=c.id;fabLayer();var f=ph.querySelector(".azfab");if(f){f.classList.remove("hop");void f.offsetWidth;f.classList.add("hop")}
  clearTimeout(G.fab.t);G.fab.t=setTimeout(function(){G.fab.bub=null;fabLayer()},6000)},400);
/* the card's scroll decides whether the hero's Start is still in sight */
var fabT=0;scr.addEventListener("scroll",function(){if(G.on||S.stop==="closed")return;clearTimeout(fabT);fabT=setTimeout(function(){var had=!!ph.querySelector(".azfab"),want=S.camCfg.fab&&!heroStartSeen();if(had!==want)fabLayer()},120)},{passive:true});
/* and every 400 ms (a card still rising, a stop changed by a drag): the button follows what is on the screen */
setInterval(function(){if(G.on||S.view!=="card"||S.stop==="closed"||S.stop==="peek")return;var had=!!ph.querySelector(".azfab"),want=!!S.camCfg.fab&&!heroStartSeen();if(had!==want)fabLayer()},400);
