/* Ask about the view: the points in view as text, the middle one, the model's tools to light points and draw routes, a treasure hunt (camLayer, camSay, camAsk). */
S.camCfg={on:true,chips:true};S.camX=.4;G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};
COMP.s7cam=["Guide","Ask about the view"];FOCUS.s7cam=["card",".camask","closed"];
/* the wall's points: where each sits on the wall's picture (u, v: 0-1), its year when known, and whether it has a card */
S.wallPois=[{id:"painting",name:T("Ribeira Palace","Paço da Ribeira"),u:.46,v:.52,year:1500,card:true},{id:"torreao",name:T("The Torreão","O Torreão"),u:.333,v:.44,year:1580},
  {id:"river",name:T("The Tagus and its ships","O Tejo e os seus navios"),u:.06,v:.55},{id:"square",name:T("Terreiro do Paço","Terreiro do Paço"),u:.40,v:.78,year:1500},
  {id:"castle",name:T("The city on the hill","A cidade na colina"),u:.62,v:.45},{id:"arcade",name:T("The arcades","As arcadas"),u:.80,v:.70}];
/* a point on the screen, through the camera (camGeom: the wall's picture as the phone sees it); a drag on the wall moves it, as moving the phone does */
function poiScreen(p){var g=camGeom();return{x:g.ox+p.u*g.dw,y:g.oy+p.v*g.dh}}
/* what the camera sees: the points on screen, each in a 3 x 3 grid cell, and the one nearest the middle */
function camView(){var g=camGeom(),rows=["top","middle","bottom"],cols=["left","centre","right"],out=[];
  var foot=g.H-(G.cam.open?330:140);S.wallPois.forEach(function(p){var s=poiScreen(p);if(s.x<8||s.x>g.W-8||s.y<60||s.y>foot)return;var c=Math.min(2,Math.floor(s.x/g.W*3)),r=Math.min(2,Math.floor(s.y/g.H*3));
    out.push({id:p.id,name:L(p.name),x:s.x,y:s.y,where:r===1&&c===1?"middle":rows[r]+" "+cols[c],d:Math.hypot(s.x-g.W/2,s.y-g.H*.45),year:p.year||null,card:!!p.card})});
  out.sort(function(a,b){return a.d-b.d});return{pois:out,central:out[0]||null}}
on("renderPhone.after",function(keep){var c=document.getElementById("camera");if(c)c.style.backgroundPosition=(S.camX*100).toFixed(1)+"% 50%";camLayer()});
/* the pins and the Ask pill, on the wall when no card is open */
function camLayer(){var o=ph.querySelector(".campins");if(o)o.remove();var b=ph.querySelector(".camask");if(b)b.remove();
  ph.classList.toggle("camopen",!!G.cam.open&&S.stop==="closed");if(S.camCfg.on&&S.view==="card"&&S.stop==="closed"&&!G.on){var v=camView(),d=document.createElement("div");d.className="campins";if(!G.cam.open)d.hidden=true;
  d.innerHTML=v.pois.map(function(p,i){var lit=G.cam.lit.indexOf(p.id)>=0,r=G.cam.route.indexOf(p.id);return'<button type="button" class="cpin'+(i===0?" central":"")+(lit?" lit":"")+(G.cam.lit.length&&!lit?" dim":"")+'" data-cpin="'+p.id+'" style="left:'+p.x.toFixed(0)+'px;top:'+p.y.toFixed(0)+'px"><i>'+(r>=0?r+1:"")+'</i><b>'+esc(p.name)+'</b></button>'}).join("");ph.appendChild(d);
  var a=document.createElement("div");a.className="camask"+(G.cam.open?" open":"");var cen=v.central;
  a.innerHTML=(G.cam.open?'<div class="ca-ans" aria-live="polite">'+(G.cam.busy?'<span class="ca-dots"><i></i><i></i><i></i></span>':esc(G.cam.answer||TXT("cam_hint")))+'</div>'+
    (S.camCfg.chips?'<div class="gd-chips ca-chips">'+["what","story","route","oldest","kids"].map(function(k){return'<button type="button" data-cchip="'+k+'">'+TXH("cam_chip_"+k)+'</button>'}).join("")+'</div>':"")+
    '<form class="gd-form ca-form"><input class="gin" id="caq" type="text" autocomplete="off" placeholder="'+TXH("dock_type")+'"><button type="submit" aria-label="'+TXH("dock_send")+'">&#10148;</button></form>':"")+
    '<div class="ca-bar"><button type="button" class="ca-pill" data-camask="1">'+MIC+'<span>'+esc(cen?fmt(TXT("cam_ask_about"),cen.name):TXT("cam_ask"))+'</span></button>'+(G.cam.open?'<button type="button" class="ca-x" data-camclose="1" aria-label="'+TXH("close")+'">&#10005;</button>':"")+'</div>';
  ph.appendChild(a)}emit("camLayer.after")}
/* ----- the answers: the model gets the points in view (names, grid places, the middle one), the card, the audience; its tools light points and make routes ----- */
function camContext(){var v=camView();return{in_view:v.pois.map(function(p){return{id:p.id,name:p.name,place:p.where,year:p.year,has_card:p.card}}),middle:v.central&&v.central.id,audience:aud(),lens:S.lens||null,language:S.lang==="pt"?"pt-PT":"en"}}
/* the wall's answers take the one order of every line (guide/voice.js's sayOrder: an answer, so the live voice first, then a recorded
   line, then the device) and Narration > Speaking now says which voice and why; the panel shows the words, so no captions */
function camSay(t){G.cam.busy=false;G.cam.answer=t;camLayer();G.answer=t;say(t,function(){},function(){})}
function camLight(ids){G.cam.lit=ids.filter(function(id){return S.wallPois.some(function(p){return p.id===id})});camLayer();emit("camLight.after",ids)}
/* without the model (offline, or a preview): answers built from the same view */
function camOffline(kind){var v=camView(),P=v.pois;if(!P.length)return TXT("cam_none");var c=v.central;
  if(kind==="what"){camLight(P.map(function(p){return p.id}));return fmt(TXT("cam_what_mid"),c.name)+" "+P.slice(1).map(function(p){return fmt(TXT("cam_what_at"),TXT("cam_place_"+p.where.replace(" ","_")),p.name)}).join(" ")}
  if(kind==="story"){camLight([c.id]);var r=c.card?byId(S.story,"r2"):null;return r?L(r.c.text):fmt(TXT("cam_story_none"),c.name)}
  if(kind==="route"){var rt=P.slice().sort(function(a,b){return a.x-b.x}).map(function(p){return p.id});G.cam.route=rt;camLight(rt);return TXT("cam_route")+" "+rt.map(function(id,i){return(i+1)+". "+P.filter(function(p){return p.id===id})[0].name}).join(" · ")}
  if(kind==="oldest"){var o=P.filter(function(p){return p.year}).sort(function(a,b){return a.year-b.year})[0];if(!o)return TXT("cam_none");camLight([o.id]);return fmt(TXT("cam_oldest"),o.name,o.year)}
  if(kind==="kids"){var h=P.filter(function(p){return p.id==="river"})[0]||P[P.length-1];G.cam.hunt=h.id;G.cam.lit=[];return fmt(TXT("cam_hunt"),h.name)}
  return""}
function camAsk(q,kind){if(firstAnswer("camAsk.answer",q,kind)!==undefined)return;G.cam.open=true;G.cam.busy=true;G.cam.route=[];G.cam.lit=[];G.cam.hunt=null;camLayer();
  if(!sampleFn||kind){setTimeout(function(){camSay(camOffline(kind||"what"))},350);return}
  var ctx=camContext(),prompt="You are Azu, the guide of a heritage app, talking to a visitor who points the phone at a tiled wall in Lisbon. You do not see the picture: you get the points of the wall now in view, where each sits on the screen, and the one in the middle. Answer only from these and from what you know for sure about them; never invent dates or facts. Answer in "+(ctx.language==="pt-PT"?"European Portuguese":"English")+", in at most 3 short spoken sentences, for a "+ctx.audience+" visitor. Use the tools: highlight_points for the points you name; make_route for a route (an ordered list of ids, each once).\n\nVIEW: "+JSON.stringify(ctx)+"\n\nVISITOR: "+q;
  var ids=ctx.in_view.map(function(p){return p.id});if(!ids.length){camSay(TXT("cam_none"));return}
  sampleFn(prompt,{modelTier:"quick",onText:function(u){G.cam.busy=false;G.cam.answer=u.text;var e=ph.querySelector(".ca-ans");if(e)e.textContent=u.text},
    tools:[{name:"highlight_points",description:"Lights the named points on the wall and dims the rest.",inputSchema:{type:"object",properties:{ids:{type:"array",items:{type:"string",enum:ids}}},required:["ids"]},execute:function(i){camLight(i.ids||[]);return"lit"}},
      {name:"make_route",description:"Shows a numbered route over the points, in this order.",inputSchema:{type:"object",properties:{ids:{type:"array",items:{type:"string",enum:ids}}},required:["ids"]},execute:function(i){G.cam.route=(i.ids||[]).slice();camLight(G.cam.route);return"shown"}}]})
  .then(function(r){camSay(r.text)}).catch(function(){camSay(camOffline("what"))})}
/* the talk button's words go here while the camera ask is open */
on("heard.answer",function(q,err){if(G.cam.open&&q&&q.trim()){G.talk="idle";camAsk(q);return true}});
ph.addEventListener("click",function(e){var t=e.target.closest("[data-camask],[data-camclose],[data-cchip],[data-cpin]");if(!t)return;e.stopPropagation();
  if(t.dataset.camclose){G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};camLayer();stopSpeech();return}
  if(t.dataset.camask){if(!G.cam.open){G.cam.open=true;camLayer();return}listen(false);return}
  if(t.dataset.cchip){var k=t.dataset.cchip;camAsk(TXT("cam_chip_"+k),sampleFn&&k!=="kids"?null:k);if(sampleFn&&k!=="kids")G.cam.lastChip=k;return}
  var id=t.dataset.cpin;if(G.cam.hunt){if(id===G.cam.hunt){G.cam.hunt=null;haptic();camLight([id]);camSay(TXT("cam_found"))}else{G.cam.answer=TXT("cam_not_it");camLayer()}return}
  var p=S.wallPois.filter(function(x){return x.id===id})[0];if(p&&p.card){G.cam.open=false;S.stop="half";renderPhone(false)}},true);
ph.addEventListener("submit",function(e){var f=e.target.closest(".ca-form");if(!f)return;e.preventDefault();e.stopPropagation();var i=f.querySelector("#caq");if(i&&i.value.trim()){var q=i.value;i.value="";camAsk(q)}},true);
/* a drag on the wall pans the camera (the Lab's stand-in for moving the phone) */
var camDrag=null;ph.addEventListener("pointerdown",function(e){if(S.stop!=="closed"||S.view!=="card"||e.target.closest(".camask,.cpin,.mk,#ptool,button"))return;camDrag={x:e.clientX,cx:S.camX}},true);
ph.addEventListener("pointermove",function(e){if(!camDrag)return;var g=camGeom(),q=ph.getBoundingClientRect().width/ph.offsetWidth,dx=(e.clientX-camDrag.x)/q,span=g.dw-g.W;if(span<=0)return;S.camX=Math.max(0,Math.min(1,camDrag.cx-dx/span));var c=document.getElementById("camera");if(c)c.style.backgroundPosition=(S.camX*100).toFixed(1)+"% 50%";camLayer()},true);
ph.addEventListener("pointerup",function(){camDrag=null},true);

/* Editor: 6 · Guide > Ask about the view */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var at=d.querySelector('[data-fo="s7ask"]');if(!at)return d.innerHTML;
  at.insertAdjacentHTML("afterend",comp("s7cam","on the wall, with no card open: questions about the points in view",
    row("Ask about the view",pseg("camCfg.on",["true","false"],["Shown","Hidden"]),"A pill at the foot of the camera view: “Ask about Ribeira Palace”, the point nearest the middle (it has a ring and its name). No photo is sent: the model gets the points in view, each one’s place on the screen (top right, middle…), the middle one, the audience and the lens. Its tools light the points it names (the rest dimmed) and draw a numbered route. A tap on a point with a card opens it. In the Lab, a drag on the wall stands in for moving the phone.")+
    row("Suggestions",pseg("camCfg.chips",["true","false"],["Shown","Hidden"]),"What am I looking at? · Tell me a story about this wall · Make me a 20-min route · What’s the oldest here? · Kids’ treasure hunt (find the point Azu names, then tap it).")+
    row("Try",'<button class="del" type="button" data-camtry="1">Try</button>',"Closes the card and opens Ask about the view.")+
    '<table class="tbl"><thead><tr><th>The wall’s points</th><th>u</th><th>v</th><th>Year</th></tr></thead><tbody>'+S.wallPois.map(function(p){return'<tr><td>'+esc(L(p.name))+'</td><td>'+p.u+'</td><td>'+p.v+'</td><td>'+(p.year||"")+'</td></tr>'}).join("")+'</tbody></table>'));
  var f=innerOf(d,"s7cam");if(f)f.insertAdjacentHTML("beforeend",ownbar("s7cam")+ownPanels("s7cam"));return d.innerHTML});
ed.addEventListener("click",function(e){var t=e.target.closest("[data-camtry]");if(!t)return;e.stopPropagation();if(G.on)stopTour();S.view="card";S.stop="closed";G.cam.open=true;renderPhone(false)},true);
on("modelSig.sig",function(h){return h+"#cam:"+S.camCfg.on+S.camCfg.chips});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("ctl\\.jump\\.","ctl\\.jump\\.|camCfg\\."));
/* each row's demo: the wall, the ✦'s menu with Ask about the view; the panel open with its suggestions */
rowDemos({"camCfg.on":{wall:true,act:function(){gbMenu(true)},ring:'[data-gbgo="view"]|.gbtn'},"camCfg.chips":{wall:"open",ring:".ca-chips|.camask"}});
