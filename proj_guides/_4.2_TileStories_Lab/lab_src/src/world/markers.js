/* The card's markers on the wall's picture, the Wall button, the markers' test scene; the scenes that act instead of asking (act: [ok, note, what it did], or a promise of it). */
/* each marker is a place on its wall's picture (u, v: 0-1 across and down), placed with camGeom() at every paint, as the pins are.
   The palace's marker stands for its point ("painting"), so that point draws no pin while the marker shows. */
WALLS.ribeira.markers=[{id:"painting",u:.46,v:.52,name:T("The Ribeira Palace","O Paço da Ribeira"),icon:"info",card:true},
  {id:"cathedral",u:.70,v:.47,name:T("Lisbon Cathedral","Sé de Lisboa"),icon:"star"}];
WALLS.belem.markers=[];
function wallMarkers(){return curWall().markers||[]}
on("wallLayer.after",function(w){w.querySelectorAll(".mk").forEach(function(b){b.remove()});
  w.insertAdjacentHTML("afterbegin",wallMarkers().map(function(m){var n=esc(L(m.name));return'<button class="mk'+(m.card?"":" dim")+'" type="button"'+(m.card?' data-marker="'+m.id+'"':"")+' data-mk="'+m.id+'" aria-label="'+n+'"><span>'+sicon(m.icon,m.card?20:18)+'</span><b>'+n+'</b></button>'}).join(""));
  placeMarkers()});
function placeMarkers(){var w=document.getElementById("wall");if(!w)return;var ms=wallMarkers();
  w.querySelectorAll("[data-mk]").forEach(function(b){var m=ms.filter(function(x){return x.id===b.dataset.mk})[0];if(!m)return;var s=poiScreen(m);b.style.left=s.x.toFixed(1)+"px";b.style.top=s.y.toFixed(1)+"px"})}
on("camPaint.after",function(){placeMarkers()});
/* the Ribeira wall shows its points too (named, as Names on the points says); the marker's own point gives way to the marker */
on("camLayer.after",function(){var pins=ph.querySelector(".campins");if(!pins||S.camCfg.wall!=="ribeira")return;pins.hidden=false;
  if(!G.cam.open)wallMarkers().forEach(function(m){var e=pins.querySelector('[data-cpin="'+m.id+'"]');if(e)e.remove()})});

/* ----- the Wall switch on the preview bar: the card's wall or the Belém test wall, one tap from anywhere ----- */
function setWall(w){if(G.on)stopTour();S.camCfg.wall=w;S.view="card";S.stop="closed";G.cam.open=false;G.cam.lit=[];G.cam.route=[];G.cam.hunt=null;
  S.camX=w==="belem"?.35:.4;S.camY=.5;S.camZoom=1;renderAll(true)}
on("ptool.html",function(h){var w=S.camCfg.wall==="belem"?"belem":"ribeira";return h+'<button type="button" data-pt="wall" aria-pressed="'+(w!=="ribeira")+'" title="'+TXH("pbar_tip_wall")+'">'+TXH("pbar_wall_"+w)+'</button>'});
document.addEventListener("click",function(e){var b=e.target.closest&&e.target.closest('[data-pt="wall"]');if(!b)return;e.stopPropagation();e.preventDefault();setWall(S.camCfg.wall==="belem"?"ribeira":"belem")},true);

/* ----- the test scene: a finger drags the wall, the wheel zooms; each marker and pin must stay on its place in the picture ----- */
function phPoint(e){var r=e.getBoundingClientRect(),p=ph.getBoundingClientRect(),q=p.width/ph.offsetWidth;return{x:(r.left+r.width/2-p.left)/q,y:(r.top+r.height/2-p.top)/q}}
/* where the picture is painted (read from the camera itself, not from camGeom), and each marker's and pin's gap from its point on it */
function wallGaps(){var c=document.getElementById("camera"),bp=c.style.backgroundPosition.split(" ").map(parseFloat),bs=c.style.backgroundSize.split(" ").map(parseFloat),out=[];
  var at=function(u,v){return{x:bp[0]+u*bs[0],y:bp[1]+v*bs[1]}};
  ph.querySelectorAll("#wall [data-mk]").forEach(function(b){var m=wallMarkers().filter(function(x){return x.id===b.dataset.mk})[0],e=at(m.u,m.v),s=phPoint(b.querySelector("span"));out.push({id:m.id,name:L(m.name),x:s.x,y:s.y,gap:Math.hypot(s.x-e.x,s.y-e.y)})});
  ph.querySelectorAll(".campins .cpin").forEach(function(b){var p=S.wallPois.filter(function(x){return x.id===b.dataset.cpin})[0],e=at(p.u,p.v),x=parseFloat(b.style.left),y=parseFloat(b.style.top);out.push({id:"pin:"+p.id,name:L(p.name),x:x,y:y,gap:Math.hypot(x-e.x,y-e.y)})});
  return{pic:{x:bp[0],y:bp[1]},items:out}}
/* a drag by a finger: the same pointer events a touch sends, on the camera */
function fingerDrag(dx,dy){var cam=document.getElementById("camera"),p=ph.getBoundingClientRect(),q=p.width/ph.offsetWidth,x0=p.left+p.width*.5,y0=p.top+p.height*.4;
  var ev=function(t,x,y){cam.dispatchEvent(new PointerEvent(t,{bubbles:true,cancelable:true,composed:true,pointerId:41,pointerType:"touch",isPrimary:true,clientX:x,clientY:y}))};
  ev("pointerdown",x0,y0);for(var i=1;i<=8;i++)ev("pointermove",x0+dx*q*i/8,y0+dy*q*i/8);ev("pointerup",x0+dx*q,y0+dy*q)}
function markerScene(){var a=wallGaps(),r=function(n){return Math.round(n)};
  fingerDrag(-120,-80);var b=wallGaps();
  document.getElementById("camera").dispatchEvent(new WheelEvent("wheel",{deltaY:-100,bubbles:true,cancelable:true}));var c=wallGaps();
  var pic={x:b.pic.x-a.pic.x,y:b.pic.y-a.pic.y},mk=a.items.filter(function(i){return i.id.indexOf("pin:")<0}),moved=mk.map(function(m){var n=b.items.filter(function(i){return i.id===m.id})[0];return{name:m.name,x:n.x-m.x,y:n.y-m.y}});
  var worst=[a,b,c].reduce(function(w,s){return s.items.reduce(function(w2,i){return i.gap>w2.gap?i:w2},w)},{gap:0,name:"-"}),pins=b.items.length-mk.length;
  var ans="The picture moved "+r(pic.x)+", "+r(pic.y)+" px; "+moved.map(function(m){return m.name+" "+r(m.x)+", "+r(m.y)}).join("; ")+". Then zoomed to "+S.camZoom.toFixed(2)+"×. "+pins+(pins===1?" point":" points")+" shown.";
  if(Math.abs(pic.x)<20||Math.abs(pic.y)<20)return[false,"the drag did not move the picture",ans];
  if(!mk.length)return[false,"no marker on the wall",ans];
  if(!pins)return[false,"the wall's points are not shown",ans];
  if(moved.some(function(m){return Math.abs(m.x-pic.x)>1.5||Math.abs(m.y-pic.y)>1.5}))return[false,"a marker did not move as far as the picture",ans];
  return worst.gap>1.5?[false,worst.name+" is "+worst.gap.toFixed(1)+" px off its place in the picture",ans]:[true,"every marker and point stays on the picture (within "+Math.max(.1,worst.gap).toFixed(1)+" px), after the drag and the zoom",ans]}
AGSCENES.push({id:"markers",label:"Ribeira wall · a drag moves the markers with the picture",where:"wall",act:markerScene,
  q:T("nothing: it drags the wall 120 px left and 80 up as a finger does, then zooms in with the wheel","nada: arrasta a parede 120 px para a esquerda e 80 para cima, como um dedo, e depois aproxima com a roda"),
  expect:T("Each marker and each point stays on its place in the picture (within 1.5 px) and moves as far as the picture.","Cada marcador e cada ponto fica no seu lugar na imagem (a menos de 1,5 px) e anda tanto como a imagem."),
  setup:function(){setWall("ribeira");S.camX=.4;S.camY=.5;S.camZoom=1.3;renderPhone(false)}});
on("agScene.after",function(sc){if(sc.act){AG.con.q="";agConPaint()}});
on("agRunScene.answer",function(i){var sc=AGSCENES[i];if(!sc.act)return;agScene(i);
  return new Promise(function(res){setTimeout(function(){Promise.resolve(sc.act()).then(function(r){AG.sceneRes[sc.id]={ok:r[0],note:r[1],answer:r[2],st:null,view:[]};agConPaint();res(AG.sceneRes[sc.id])})},300)})});
on("agScenesHtml.html",function(h){var sc=AGSCENES[AG.scene];return sc.act?h.replace("Asks: “"+esc(L(sc.q))+"”","Does: "+esc(L(sc.q))):h});
