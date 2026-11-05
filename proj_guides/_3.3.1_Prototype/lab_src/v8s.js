/* ===================== v8s: Azu always present (the guide's button), its comments on what the camera sees, a test wall to pan and zoom, a tour that opens the card ===================== */
IMG.belem="__BELEM__";
S.camCfg.wall="ribeira";S.camCfg.fab=true;S.camCfg.comments=true;S.camCfg.every=7;S.camY=.5;S.camZoom=1;G.fab={menu:false,bub:null,last:0,cen:null,since:0,count:0};
/* the walls the Lab can stand in front of: the card's own (the Ribeira painting) and a test wall with many points (the Belém
   stretch of the Grande Panorama de Lisboa, Museu Nacional do Azulejo; photo CC0, Gerda Arendt, Wikimedia Commons) */
var WALLS={ribeira:{img:function(){return IMG.g1},w:720,h:414,pois:S.wallPois},
  belem:{img:function(){return IMG.belem},w:2400,h:1837,pois:[
    {id:"belem",name:T("Belém Tower","Torre de Belém"),u:.344,v:.55,year:1514},{id:"jeronimos",name:T("Jerónimos Monastery","Mosteiro dos Jerónimos"),u:.92,v:.37,year:1501},
    {id:"bomsucesso",name:T("The convent with the dome","O convento da cúpula"),u:.61,v:.48},{id:"ermida",name:T("The chapel on the hill","A ermida no alto"),u:.707,v:.2},
    {id:"cadaval",name:T("A noble estate","Uma quinta nobre"),u:.164,v:.36},{id:"bridge",name:T("The stone bridge","A ponte de pedra"),u:.234,v:.566},
    {id:"caravel",name:T("A ship with a lateen sail","Um barco de vela latina"),u:.5,v:.66},{id:"ship2",name:T("A ship on the Tagus","Um barco no Tejo"),u:.828,v:.61},
    {id:"palace",name:T("A palace by the river","Um palácio junto ao rio"),u:.81,v:.5},{id:"houses",name:T("Houses on the hill","Casas na encosta"),u:.39,v:.39},
    {id:"tagus",name:T("The Tagus","O Tejo"),u:.66,v:.72}]}};
function curWall(){return WALLS[S.camCfg.wall]||WALLS.ribeira}
/* the camera: the wall's picture as the phone sees it, panned (camX, camY) and zoomed (camZoom: nearer the wall) */
camGeom=function(){var wl=curWall(),W=ph.offsetWidth,H=ph.offsetHeight,k=Math.max(W/wl.w,H/wl.h)*S.camZoom,dw=wl.w*k,dh=wl.h*k;return{W:W,H:H,dw:dw,dh:dh,ox:(W-dw)*S.camX,oy:(H-dh)*S.camY}};
function camPaint(){var c=document.getElementById("camera");if(!c||S.view!=="card")return;var g=camGeom();c.style.backgroundImage="url("+curWall().img()+")";c.style.backgroundSize=g.dw.toFixed(0)+"px "+g.dh.toFixed(0)+"px";c.style.backgroundPosition=g.ox.toFixed(0)+"px "+g.oy.toFixed(0)+"px"}
on("renderPhone.before",function(keep){S.wallPois=curWall().pois});on("renderPhone.after",function(keep){camPaint();ph.classList.toggle("testwall",S.camCfg.wall!=="ribeira");camLayer();fabLayer()});
/* the test wall's points are its markers, always shown (the card's wall keeps its own marker) */
var camLayerV8s=camLayer;camLayer=function(){camLayerV8s();var pins=ph.querySelector(".campins");if(pins&&S.camCfg.wall!=="ribeira"&&S.stop==="closed"&&S.view==="card")pins.hidden=false;
  var a=ph.querySelector(".camask");if(a&&!G.cam.open&&S.camCfg.fab)a.remove();var bar=a&&a.querySelector(".ca-bar");if(bar&&S.camCfg.fab){var pill=bar.querySelector(".ca-pill span");if(pill)pill.textContent=TXT("cam_ask")}};
/* drag (both ways) and zoom (the wheel, or two fingers) stand in for moving the phone */
var camPan=null,pinch=null,ptrs={};
ph.addEventListener("pointerdown",function(e){if(S.stop!=="closed"||S.view!=="card"||e.target.closest(".camask,.cpin,.mk,#ptool,button,.azfab,.azfabbub"))return;ptrs[e.pointerId]={x:e.clientX,y:e.clientY};
  camDrag=null;var ids=Object.keys(ptrs);if(ids.length===2){var a=ptrs[ids[0]],b=ptrs[ids[1]];pinch={d:Math.hypot(a.x-b.x,a.y-b.y),z:S.camZoom};camPan=null}else camPan={x:e.clientX,y:e.clientY,cx:S.camX,cy:S.camY};e.stopPropagation()},true);
ph.addEventListener("pointermove",function(e){if(!ptrs[e.pointerId])return;ptrs[e.pointerId]={x:e.clientX,y:e.clientY};var g=camGeom(),q=ph.getBoundingClientRect().width/ph.offsetWidth;
  if(pinch){var ids=Object.keys(ptrs),a=ptrs[ids[0]],b=ptrs[ids[1]];if(a&&b){S.camZoom=Math.max(1,Math.min(3.2,pinch.z*Math.hypot(a.x-b.x,a.y-b.y)/pinch.d))}}
  else if(camPan){var sx=g.dw-g.W,sy=g.dh-g.H;if(sx>0)S.camX=Math.max(0,Math.min(1,camPan.cx-(e.clientX-camPan.x)/q/sx));if(sy>0)S.camY=Math.max(0,Math.min(1,camPan.cy-(e.clientY-camPan.y)/q/sy));G.fab.panAt=performance.now()}
  camPaint();camLayer();e.stopPropagation()},true);
["pointerup","pointercancel"].forEach(function(t){ph.addEventListener(t,function(e){delete ptrs[e.pointerId];if(Object.keys(ptrs).length<2)pinch=null;if(!Object.keys(ptrs).length)camPan=null},true)});
ph.addEventListener("wheel",function(e){if(S.stop!=="closed"||S.view!=="card")return;e.preventDefault();S.camZoom=Math.max(1,Math.min(3.2,S.camZoom*(e.deltaY<0?1.1:1/1.1)));G.fab.panAt=performance.now();camPaint();camLayer()},{capture:true,passive:false});
/* v8p's own drag: the pan above replaces it */
camDrag=null;
/* ----- Azu as the guide's button: bottom left; on the card, above the bar; on the wall, in the corner ----- */
/* on the card it shows once the hero's Start the guide is off the screen (the hero is the guide's way in until then) */
function heroStartSeen(){var g=scr.querySelector(".hero .gstart");if(!g)return false;var r=g.getBoundingClientRect(),b=scr.getBoundingClientRect();return r.bottom>b.top&&r.top<b.bottom}
function fabLayer(){var o=ph.querySelector(".azfab");if(o)o.remove();ph.querySelectorAll(".azfabbub").forEach(function(x){x.remove()});if(S.stop!=="closed")G.fab.bub=null;
  if(!S.camCfg.fab||S.view!=="card"||G.on||S.stop==="peek"||(G.cam.open&&S.stop==="closed")||(S.stop!=="closed"&&heroStartSeen()))return;
  var b=document.createElement("button");b.type="button";b.className="azfab"+(S.stop==="closed"?" onwall":"");b.setAttribute("aria-label",TXT("fab_label"));/* the button draws the 2D Azu: the shader's one canvas belongs to the guide, the peek and the walk */b.innerHTML=MASCOT;ph.appendChild(b);
  if(G.fab.menu&&S.stop!=="closed"){var m=document.createElement("div");m.className="azfabbub menu";m.innerHTML='<button type="button" data-fabgo="tour">▶ '+TXH("fab_tour")+'</button><button type="button" data-fabgo="ask">'+TXH("fab_ask")+'</button>';ph.appendChild(m)}
  if(G.fab.bub&&S.stop==="closed")showFabBub(G.fab.bub)}
function showFabBub(bb){var o=ph.querySelector(".azfabbub");if(o)o.remove();var d=document.createElement("button");d.type="button";d.className="azfabbub"+(bb.act?" act":"");d.dataset.fabbub="1";d.textContent=bb.text;ph.appendChild(d)}
/* a tour from a card at Half: the card rises to Full first (lowered again by the visitor, the tour goes on there) */
var startTourV8s=startTour;startTour=function(){G.fab.menu=false;var f=ph.querySelector(".azfab");if(f)f.remove();if(S.view==="card"&&S.stop!=="full"){S.stop="full";renderPhone(true)}startTourV8s()};
var stopTourV8s=stopTour;stopTour=function(){stopTourV8s();fabLayer()};
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

/* Editor: 6 · Guide > Ask about the view gets the wall, Azu's button and its comments */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7cam",row("Azu as the guide's button",pseg("camCfg.fab",["true","false"],["On","Off"]),"Azu stays on the screen as the guide's button, bottom left: above the bar on the card (a tap offers Start the guide or Ask), in the corner on the wall (a tap opens Ask about the view). It replaces the Ask pill. During a tour it is the guide itself.")+
    row("Azu comments",pseg("camCfg.comments",["true","false"],["On","Off"]),"On the wall, Azu watches where the camera points. When a point settles in the middle (1.2 s), a short remark in a small bubble (“In the middle: Belém Tower. Tap to hear more.”). A tap on the bubble opens the point’s card and starts its tour, or tells its story. Never while the phone moves, at most 12 a visit.")+
    row("Between comments",pslider("camCfg.every",4,30,1),"Seconds, at least.",true)+
    row("Wall (the Lab)",pseg("camCfg.wall",["ribeira","belem"],["Ribeira painting","Belém panorama (11 points)"]),"Ribeira: the card’s own wall. Belém: a test wall from the Grande Panorama de Lisboa (Museu Nacional do Azulejo; photo CC0, Gerda Arendt) with 11 points. Drag pans, the wheel or two fingers zoom: the Lab’s stand-in for moving the phone nearer or farther."),"Try");
  return d.innerHTML});
on("modelSig.sig",function(h){return h+"#fab:"+S.camCfg.fab+S.camCfg.comments+S.camCfg.wall});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("ctl\\.jump\\.","ctl\\.jump\\.|camCfg\\.every"));

/* the card's scroll decides whether the hero's Start is still in sight */
var fabT=0;scr.addEventListener("scroll",function(){if(G.on||S.stop==="closed")return;clearTimeout(fabT);fabT=setTimeout(function(){var had=!!ph.querySelector(".azfab"),want=S.camCfg.fab&&!heroStartSeen();if(had!==want)fabLayer()},120)},{passive:true});
/* and every 400 ms (a card still rising, a stop changed by a drag): the button follows what is on the screen */
setInterval(function(){if(G.on||S.view!=="card"||S.stop==="closed"||S.stop==="peek")return;var had=!!ph.querySelector(".azfab"),want=!!S.camCfg.fab&&!heroStartSeen();if(had!==want)fabLayer()},400);
