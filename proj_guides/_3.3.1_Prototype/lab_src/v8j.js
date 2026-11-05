/* ===================== v8j: Focus: a spotlight (or a soft ring) on what the guide tells, a slow zoom to the spot named in a picture, and Azu points at it ===================== */
S.guideCfg.cue="spotlight";S.guideCfg.scrim=0.4;S.guideCfg.zoom="zoom";S.guideCfg.point=true;
COMP.s7focus=["Guide","Focus"];FOCUS.s7focus=["story",'[data-ref="r:r1"]',"full"];
/* the demo point: step 4 tells the 1662 drawing, and names three spots in it */
(function(){var g=byId(S.guideSteps,"g3");if(!g)return;
  g.text=T("Now the story. This drawing shows the square in 1662: the long palace along the back, its great corner tower on the left, and a fountain built for a night of fireworks.","Agora a história. Este desenho mostra a praça em 1662: o longo palácio ao fundo, o grande torreão à esquerda e uma fonte erguida para uma noite de fogo de artifício.");
  g.cues=[{at:T("palace","palácio"),region:[.2,.42,.8,.2]},{at:T("tower","torreão"),region:[0,.2,.25,.42]},{at:T("fountain","fonte"),region:[.34,.5,.34,.34]}]})();
var VIEWBOX=window.CSS&&CSS.supports&&CSS.supports("object-view-box","inset(0px)");
function cueWord(w){return String(w||"").toLowerCase().replace(/^[^\p{L}\p{N}]+|[^\p{L}\p{N}]+$/gu,"")}
/* a cue's word, as the index of the first word of the step that starts with it */
function cueIndex(c){var at=cueWord(L(c.at));if(!at)return -1;for(var j=0;j<G.words.length;j++)if(cueWord(G.words[j].w).indexOf(at)===0)return j;return -1}
function stepEl(st){if(!st)return null;if(st.sel){var s=ph.querySelector(st.sel);if(s)return s}return st.ref==="hero"?scr.querySelector(".hero"):scr.querySelector('[data-ref="'+st.ref+'"]')}
function stepImg(el){return el?el.querySelector("img"):null}
/* the part of the natural picture to show: the region grown to the box's shape, at most 2.6x the picture at rest */
function viewOf(img,reg){var W=img.clientWidth,H=img.clientHeight,nw=img.naturalWidth||W,nh=img.naturalHeight||H,A=W/H,c=Math.max(W/nw,H/nh);
  var cx=(reg[0]+reg[2]/2)*nw,cy=(reg[1]+reg[3]/2)*nh,vw=Math.max(reg[2]*nw,reg[3]*nh*A,W/c/2.6),vh=vw/A;
  if(vw>nw){vw=nw;vh=vw/A}if(vh>nh){vh=nh;vw=vh*A}
  var x0=Math.max(0,Math.min(nw-vw,cx-vw/2)),y0=Math.max(0,Math.min(nh-vh,cy-vh/2));return[x0,y0,vw,vh,nw,nh]}
function zoomImg(img,reg){if(!img)return;var fade=S.reduce,mode=S.guideCfg.zoom;
  var set=function(){if(reg&&mode==="zoom"&&VIEWBOX){var v=viewOf(img,reg);img.style.objectViewBox="inset("+v[1].toFixed(1)+"px "+(v[4]-v[0]-v[2]).toFixed(1)+"px "+(v[5]-v[1]-v[3]).toFixed(1)+"px "+v[0].toFixed(1)+"px)";img.dataset.gz="1"}
    else{img.style.objectViewBox=VIEWBOX?"inset(0px 0px 0px 0px)":"";if(!reg)delete img.dataset.gz}
    img.classList.toggle("gzimg",!!reg)};
  if(fade){img.style.transition="opacity .45s";img.style.opacity="0";clearTimeout(img._gzt);img._gzt=setTimeout(function(){set();img.style.opacity="1"},230)}
  else{img.style.transition="object-view-box 1.5s cubic-bezier(.45,0,.25,1)";set()}}
function unzoomAll(){scr.querySelectorAll("img[data-gz]").forEach(function(i){zoomImg(i,null)})}
/* the spot's box on the phone: the whole picture when zoomed, else the region where the picture shows it (object-fit: cover) */
/* what is really seen of an element: its box cut by the boxes that clip it (a picture's frame, the scroller) */
function visRect(el){var r=el.getBoundingClientRect(),L0=r.left,T0=r.top,R0=r.right,B0=r.bottom,a=el.parentElement;
  while(a&&a!==ph){var o=getComputedStyle(a);if(o.overflow!=="visible"||o.overflowY!=="visible"||(o.clipPath&&o.clipPath!=="none")){var q=a.getBoundingClientRect();L0=Math.max(L0,q.left);T0=Math.max(T0,q.top);R0=Math.min(R0,q.right);B0=Math.min(B0,q.bottom)}a=a.parentElement}
  return{left:L0,top:T0,right:Math.max(L0,R0),bottom:Math.max(T0,B0),width:Math.max(0,R0-L0),height:Math.max(0,B0-T0)}}
/* while a spot is shown the picture drops its scroll effect (its parallax scale), so the spot is where the words say */
function regionRect(img,reg){var r=visRect(img);if(S.guideCfg.zoom==="zoom"&&VIEWBOX)return r;r=img.getBoundingClientRect();
  var W=r.width,H=r.height,nw=img.naturalWidth||W,nh=img.naturalHeight||H,c=Math.max(W/nw,H/nh),dw=nw*c,dh=nh*c,ox=(W-dw)/2,oy=(H-dh)/2;
  var L0=Math.max(r.left,r.left+ox+reg[0]*dw),T0=Math.max(r.top,r.top+oy+reg[1]*dh),R0=Math.min(r.right,r.left+ox+(reg[0]+reg[2])*dw),B0=Math.min(r.bottom,r.top+oy+(reg[1]+reg[3])*dh);
  return{left:L0,top:T0,width:R0-L0,height:B0-T0,right:R0,bottom:B0}}
/* each frame while the tour runs: the cue now (the last one whose word was reached), the spotlight on it, Azu pointing */
var runStepV8j=runStep;runStep=function(){unzoomAll();G.fc={cue:-2,i:G.i};runStepV8j();fcLoop()};
function fcLoop(){if(!G.fcRaf)G.fcRaf=requestAnimationFrame(fcTick)}
function fcTick(){G.fcRaf=0;var sp=ph.querySelector(".gspot");if(!G.on){if(sp)sp.remove();return}
  var st=S.guideSteps[G.i],el=stepEl(st),cfg=S.guideCfg,k=G.hl?G.hl.k:-1,cues=st&&st.cues||[],now=-1;
  for(var c=0;c<cues.length;c++){var ix=cueIndex(cues[c]);if(ix>=0&&k>=ix)now=c}
  if(!G.fc||G.fc.i!==G.i)G.fc={cue:-2,i:G.i};
  var img=stepImg(el);if(now!==G.fc.cue){G.fc.cue=now;if(img)zoomImg(img,now>=0?cues[now].region:null)}
  var p=ph.getBoundingClientRect(),q=p.width/ph.offsetWidth,box=scr.getBoundingClientRect(),r=null;
  if(el){r=now>=0&&img?regionRect(img,cues[now].region):visRect(el)}
  /* a new target: glide from where the light was, towards where the target is now (so a scroll during the glide is followed) */
  var key=G.i+":"+now,tn=performance.now();if(G.fc.key!==key){G.fc.key=key;G.fc.at=tn;G.fc.from=G.fc.last||null}
  if(cfg.cue==="none"||!r||G.paused||G.askOpen||G.moment){if(sp)sp.classList.remove("on")}
  else{if(!sp){sp=document.createElement("div");sp.className="gspot";sp.setAttribute("aria-hidden","true");ph.appendChild(sp)}
    var t=Math.max(r.top,box.top+4),b=Math.min(r.bottom,box.bottom-4),pad=now>=0?4:6;
    sp.className="gspot on "+cfg.cue;sp.style.setProperty("--scrim",cfg.scrim);
    var g=[(r.left-p.left)/q-pad,(t-p.top)/q-pad,r.width/q+2*pad,Math.max(0,b-t)/q+2*pad],f=G.fc.from,e=S.reduce||!f?1:Math.min(1,(tn-G.fc.at)/550);
    if(e<1){e=1-Math.pow(1-e,3);g=g.map(function(v,i){return f[i]+(v-f[i])*e})}G.fc.last=g;
    sp.style.left=g[0]+"px";sp.style.top=g[1]+"px";sp.style.width=g[2]+"px";sp.style.height=g[3]+"px"}
  ph.classList.toggle("gspotting",cfg.cue!=="none");
  pointAt(r&&cfg.point&&!G.paused?{x:(r.left+r.width/2-p.left)/q,y:((Math.max(r.top,box.top)+Math.min(r.bottom,box.bottom))/2-p.top)/q}:null);
  G.fcRaf=requestAnimationFrame(fcTick)}
/* Azu stands level with the spot and points at it with one arm */
function pointAt(tg){var m=ph.querySelector(".mascot");if(!m)return;if(G.moment)tg=null;var a=m.querySelector(".marm");
  if(!tg){if(a)a.classList.remove("on");m.classList.remove("pointing");return}
  if(!a){a=document.createElement("i");a.className="marm";m.appendChild(a)}
  /* never under the captions: Azu stays above their box */
  var cap=ph.querySelector(".gcap"),lo=cap?cap.offsetTop-72:ph.offsetHeight-230,y=Math.max(70,Math.min(lo,tg.y-27));if(Math.abs((parseFloat(m.style.top)||0)-y)>2)m.style.top=y+"px";
  var mx=m.offsetLeft+8,my=y+34,ang=Math.atan2(tg.y-my,tg.x-mx)*180/Math.PI;ang=Math.max(120,Math.min(240,ang<0?ang+360:ang));
  a.style.transform="rotate("+(ang-180).toFixed(1)+"deg)";a.classList.add("on");m.classList.add("pointing")}
var stopTourV8j=stopTour;stopTour=function(){unzoomAll();var sp=ph.querySelector(".gspot");if(sp)sp.remove();ph.classList.remove("gspotting");stopTourV8j()};

/* Editor: 6 · Guide gets Focus, after Captions; Specific Marker > Guide script gets each step's spots, drawn on its picture */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var ca=d.querySelector('[data-fo="s7cap"]');if(!ca)return d.innerHTML;
  ca.insertAdjacentHTML("afterend",comp("s7focus","the spotlight on what is told, the zoom to a spot in a picture, Azu pointing",
    row("Focus",pseg("guideCfg.cue",["spotlight","ring","none"],["Spotlight","Soft ring","None"]),"Spotlight: the rest of the card dims and a soft light rings what the guide is telling. Soft ring: only the ring. One at a time; it follows the scroll and hides while the visitor takes over.")+
    row("Dim the rest",pslider("guideCfg.scrim",0.2,0.6,0.05),"Spotlight only: how dark the rest of the card goes (0.4 = 40 %).",true)+
    row("Into pictures",pseg("guideCfg.zoom",["zoom","spot"],["Slow zoom","Light the spot"]),"When the guide names a spot in a picture (the step's spots, in the Guide script). Slow zoom: the picture glides in to the spot (1.5 s). Light the spot: the picture stays and the spotlight closes on the spot. Reduce Motion: a crossfade instead of the glide.")+
    row("Azu points",pseg("guideCfg.point",["true","false"],["On","Off"]),"Azu stands level with what is being told and points at it with one arm (with the mascot on).")+
    row("Try",'<button class="del" type="button" data-fctry="g3">Try</button>',"Starts the tour at the step that names three spots in the 1662 drawing.")));
  return d.innerHTML});
function stepPic(st){var el=stepEl(st),i=stepImg(el);if(i)return i.currentSrc||i.src;var r=st.ref&&st.ref.indexOf("r:")===0?byId(S.story,st.ref.slice(2)):null,k=null;
  (function walk(o){if(k||!o||typeof o!=="object")return;if(typeof o.img==="string"){k=o.img;return}for(var x in o)walk(o[x])})(r);return k&&IMG[k]?IMG[k]:null}
function stepWords(st){var t=stepText(st),out=[],seen={};(t.match(/\S+/g)||[]).forEach(function(w){var c=cueWord(w);if(c.length>2&&!seen[c]){seen[c]=1;out.push(c)}});return out}
on("edMarker.html",function(html){var d=document.createElement("div");d.innerHTML=html;var st=S.sel.gstep&&byId(S.guideSteps,S.sel.gstep);if(!st)return d.innerHTML;
  var slot=d.querySelector('[data-pick="gstep"][data-id="'+cssq(st.id)+'"] + tr.slot td:last-child');if(!slot)return d.innerHTML;
  var pic=stepPic(st),words=stepWords(st),cues=st.cues||[];
  var h=cues.map(function(c,i){var r=c.region||[.3,.3,.4,.4],cur=cueWord(L(c.at));
    return'<div class="cue"><div class="cuehd">When it says <select class="u" data-cueat="'+st.id+'|'+i+'">'+(words.indexOf(cur)<0?'<option value="">(pick a word)</option>':"")+words.map(function(w){return'<option'+(w===cur?" selected":"")+'>'+esc(w)+'</option>'}).join("")+'</select> it shows this spot <button class="del" type="button" data-cuedel="'+st.id+'|'+i+'">Remove</button></div>'+
      (pic?'<div class="cuepic" data-cuepic="'+st.id+'|'+i+'"><img src="'+pic+'" alt="" draggable="false"><i style="left:'+r[0]*100+'%;top:'+r[1]*100+'%;width:'+r[2]*100+'%;height:'+r[3]*100+'%"></i></div>':'<div class="note">This step\'s block has no picture: the spotlight shows the whole block.</div>')+'</div>'}).join("");
  slot.insertAdjacentHTML("beforeend",row("Spots it names",'<div class="cues">'+(h||'<span class="uses">None: the spotlight shows the whole block.</span>')+'<div class="btnrow"><button class="ubtn sm" type="button" data-cueadd="'+st.id+'">+ Add a spot</button>'+(cues.length?'<button class="ubtn sm" type="button" data-fctry="'+st.id+'">Try this step</button>':"")+'</div></div>',
    "A word of what the guide says, and the spot in the picture it means: when the voice reaches the word, the picture zooms to the spot and Azu points at it. Drag on the picture to draw the spot."));
  return d.innerHTML});
ed.addEventListener("click",function(e){var t=e.target.closest("[data-fctry],[data-cueadd],[data-cuedel]");if(!t)return;e.stopPropagation();
  if(t.dataset.fctry){var j=S.guideSteps.findIndex(function(s){return s.id===t.dataset.fctry});if(G.on)stopTour();startTour();if(j>0){G.i=j;runStep()}return}
  if(t.dataset.cueadd){var st=byId(S.guideSteps,t.dataset.cueadd);(st.cues=st.cues||[]).push({at:T("",""),region:[.3,.3,.4,.4]});renderEditor();return}
  var q=t.dataset.cuedel.split("|"),s2=byId(S.guideSteps,q[0]);s2.cues.splice(+q[1],1);renderEditor()},true);
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.cueat)return;e.stopImmediatePropagation();var q=t.dataset.cueat.split("|"),c=byId(S.guideSteps,q[0]).cues[+q[1]];c.at=c.at||T("","");c.at[S.lang]=t.value},true);
/* drawing a spot: press on the picture, drag, let go */
var cueDraw=null;
ed.addEventListener("pointerdown",function(e){var b=e.target.closest("[data-cuepic]");if(!b)return;e.preventDefault();e.stopPropagation();var r=b.getBoundingClientRect();
  cueDraw={b:b,r:r,x:(e.clientX-r.left)/r.width,y:(e.clientY-r.top)/r.height};try{b.setPointerCapture(e.pointerId)}catch(x){}},true);
function cueAt(e){var r=cueDraw.r,x=Math.max(0,Math.min(1,(e.clientX-r.left)/r.width)),y=Math.max(0,Math.min(1,(e.clientY-r.top)/r.height));
  return[Math.min(x,cueDraw.x),Math.min(y,cueDraw.y),Math.max(.04,Math.abs(x-cueDraw.x)),Math.max(.04,Math.abs(y-cueDraw.y))]}
ed.addEventListener("pointermove",function(e){if(!cueDraw)return;var g=cueAt(e),i=cueDraw.b.querySelector("i");i.style.left=g[0]*100+"%";i.style.top=g[1]*100+"%";i.style.width=g[2]*100+"%";i.style.height=g[3]*100+"%"},true);
ed.addEventListener("pointerup",function(e){if(!cueDraw)return;var g=cueAt(e),q=cueDraw.b.dataset.cuepic.split("|");byId(S.guideSteps,q[0]).cues[+q[1]].region=g.map(function(v){return Math.round(v*1000)/1000});cueDraw=null;
  if(G.on&&G.fc){G.fc.cue=-2}},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(cue|scrim|zoom|point|"));
