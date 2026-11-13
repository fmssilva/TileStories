/* The wall behind the card (wallLayer) and the test walls: Ribeira and Belém, pan and zoom (WALLS, camGeom, camPaint); each drag or zoom is the phone moving (phone.moved). Ask about the view > Azu comments links to the guidance that says them (Guide › 4 · Azu speaks up › Content › What's in view). */
function wallLayer(){var w=document.getElementById("wall");if(!w){w=document.createElement("div");w.id="wall";w.className="wall";ph.insertBefore(w,ph.firstChild.nextSibling)}
  w.innerHTML='<button class="mk" type="button" data-marker="painting" style="left:52%;top:30%" aria-label="The Ribeira Palace"><span>'+sicon("info",20)+'</span><b>The Ribeira Palace</b></button><button class="mk dim" type="button" style="left:22%;top:58%" aria-label="Lisbon Cathedral"><span>'+sicon("star",18)+'</span><b>Lisbon Cathedral</b></button>';
  emit("wallLayer.after",w);return w}
IMG.belem="__BELEM__";
S.camCfg.wall="ribeira";S.camY=.5;S.camZoom=1;
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
function camGeom(){var wl=curWall(),W=ph.offsetWidth,H=ph.offsetHeight,k=Math.max(W/wl.w,H/wl.h)*S.camZoom,dw=wl.w*k,dh=wl.h*k;return{W:W,H:H,dw:dw,dh:dh,ox:(W-dw)*S.camX,oy:(H-dh)*S.camY}}
function camPaint(){var c=document.getElementById("camera");if(c&&S.view==="card"){var g=camGeom();c.style.backgroundImage="url("+curWall().img()+")";c.style.backgroundSize=g.dw.toFixed(0)+"px "+g.dh.toFixed(0)+"px";c.style.backgroundPosition=g.ox.toFixed(0)+"px "+g.oy.toFixed(0)+"px"}emit("camPaint.after")}
on("renderPhone.before",function(keep){S.wallPois=curWall().pois});on("renderPhone.after",function(keep){camPaint();ph.classList.toggle("testwall",S.camCfg.wall!=="ribeira");camLayer();fabLayer()});
/* the test wall's points are its markers, always shown (the card's wall keeps its own marker) */
on("camLayer.after",function(){var pins=ph.querySelector(".campins");if(pins&&S.camCfg.wall!=="ribeira"&&S.stop==="closed"&&S.view==="card")pins.hidden=false;
  /* the guide's button opens Ask about the view (its menu names the middle point): no pill while the panel is closed */
  var a=ph.querySelector(".camask");if(a&&!G.cam.open)a.remove()});
/* drag (both ways) and zoom (the wheel, or two fingers) stand in for moving the phone; each move emits phone.moved (the guidance waits for the phone to be still) */
var camPan=null,pinch=null,ptrs={};
ph.addEventListener("pointerdown",function(e){if(S.stop!=="closed"||S.view!=="card"||e.target.closest(".camask,.cpin,.mk,#ptool,button,.gbtn-wrap,.gbmenu,.gdbub,.gdock,.gdbeat,.gdstrip"))return;ptrs[e.pointerId]={x:e.clientX,y:e.clientY};
  camDrag=null;var ids=Object.keys(ptrs);if(ids.length===2){var a=ptrs[ids[0]],b=ptrs[ids[1]];pinch={d:Math.hypot(a.x-b.x,a.y-b.y),z:S.camZoom};camPan=null}else camPan={x:e.clientX,y:e.clientY,cx:S.camX,cy:S.camY};e.stopPropagation()},true);
ph.addEventListener("pointermove",function(e){if(!ptrs[e.pointerId])return;ptrs[e.pointerId]={x:e.clientX,y:e.clientY};var g=camGeom(),q=ph.getBoundingClientRect().width/ph.offsetWidth;
  if(pinch){var ids=Object.keys(ptrs),a=ptrs[ids[0]],b=ptrs[ids[1]];if(a&&b){S.camZoom=Math.max(1,Math.min(3.2,pinch.z*Math.hypot(a.x-b.x,a.y-b.y)/pinch.d))}}
  else if(camPan){var sx=g.dw-g.W,sy=g.dh-g.H;if(sx>0)S.camX=Math.max(0,Math.min(1,camPan.cx-(e.clientX-camPan.x)/q/sx));if(sy>0)S.camY=Math.max(0,Math.min(1,camPan.cy-(e.clientY-camPan.y)/q/sy))}
  if(pinch||camPan)emit("phone.moved")
  camPaint();camLayer();e.stopPropagation()},true);
["pointerup","pointercancel"].forEach(function(t){ph.addEventListener(t,function(e){delete ptrs[e.pointerId];if(Object.keys(ptrs).length<2)pinch=null;if(!Object.keys(ptrs).length)camPan=null},true)});
ph.addEventListener("wheel",function(e){if(S.stop!=="closed"||S.view!=="card")return;e.preventDefault();S.camZoom=Math.max(1,Math.min(3.2,S.camZoom*(e.deltaY<0?1.1:1/1.1)));emit("phone.moved");camPaint();camLayer()},{capture:true,passive:false});
/* camera-ask's own drag: the pan above replaces it */
camDrag=null;
/* Editor: Guide tab > Ask about the view gets the wall and the link to Azu's comments */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  /* Azu comments: a link row to the guidance that says them now (Guide › 4 · Azu speaks up › Content › What's in view); its On / Off is that group's On */
  var gp=S.guidance?gdGroupPath("inview"):null,gg=gp?gdGroupById("inview"):null;
  addRows(d,"s7cam",(gp?row("The mascot comments",'<div class="btnrow" style="margin:0">'+pseg(gp+"on",["true","false"],["On","Off"])+'<button class="ubtn sm" type="button" data-goto="gd:g:inview">Said by the guide: Guide › The mascot speaks up › Content › What\'s in view &#8250;</button></div>',"On the wall, when a point settles in the middle, the mascot says a short remark beside it (“In the middle: Belém Tower. Tap to hear more.”); a tap starts the tour of a point with a card, or opens Ask about the view about it. Its On / Off is the group's own (Guide › 4 · The mascot speaks up › Content › What's in view); there its gap"+(gg?" ("+gg.gap+" s)":"")+", its cap"+(gg?" ("+gg.cap+" a visit)":"")+", its lines and the rest. The guide's breathing room, one at a time and its other rules hold for it as for every tip."):"")+
    row("Wall (the Lab)",pseg("camCfg.wall",["ribeira","belem"],["Ribeira painting","Belém panorama (11 points)"]),"Ribeira: the card’s own wall. Belém: a test wall from the Grande Panorama de Lisboa (Museu Nacional do Azulejo; photo CC0, Gerda Arendt) with 11 points. Drag pans, the wheel or two fingers zoom: the Lab’s stand-in for moving the phone nearer or farther."),"Try");
  return d.innerHTML});
on("modelSig.sig",function(h){return h+"#wall:"+S.camCfg.wall});
/* each row's demo: the wall itself (Azu comments' On / Off is the guidance group's row: its demo is guidance/editor.js's) */
rowDemos({"camCfg.wall":{wall:true,ring:"#wall"}});
