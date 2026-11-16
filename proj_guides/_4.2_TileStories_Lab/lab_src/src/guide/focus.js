/* Focus: the left bar (round 22, the default), the spotlight, the zoom to a spot named in a picture, Azu pointing; the script's spots. */
/* round 22 (22.5; the developer, 2026-11-13: "Remove the focus that highlights the block and darkens the rest of the app: it damages the
   sense of navigation. Have only a little bar that appears smoothly on the left side, pulsing a bit, to show we are talking about that
   block. Nothing else gets darker: everything stays the same."): Focus = The left bar (the default): a slim bar (4 px, the block's
   seen height, the card's accent --accent2: 8.6:1 on the paper, 7.8:1 on the dark card) in the card's left margin, its right edge 6 px
   from the block's left edge (it never touches the block or moves its layout); it fades in (0.35 s), breathes very softly (opacity 1 to
   0.78, 2.6 s) while the block is told and fades out when the tour moves on; nothing else changes colour. Dim the rest (today's
   spotlight), Soft ring and Off (nothing marks the block) stay as options. Reduce Motion: no breath. Unity: an Image anchored to the
   block's left edge in the ScrollView's content (its height the block's), its alpha tweened (DOTween, a yoyo loop) */
S.guideCfg.cue="bar";S.guideCfg.scrim=0.4;S.guideCfg.zoom="zoom";S.guideCfg.point=true;
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
on("runStep.before",function(){unzoomAll();G.fc={cue:-2,i:G.i}});on("runStep.after",function(){fcLoop()});
function fcLoop(){if(!G.fcRaf)G.fcRaf=requestAnimationFrame(fcTick)}
/* the spotlight's cut-out, one for the guide (the tour's Focus, and Guidance's Spotlight on an Essential tip): a hole the size of the box
   g = [left, top, width, height] in the phone's px, in a scrim at scrim (0.2-0.6), with the warm ring; mode "spotlight" or "ring"; round
   for a marker. Unity: Painter2D in generateVisualContent, FillRule.OddEven: the screen's rect and the hole's rounded rect in one path,
   filled once (the ring a Stroke on the hole) */
function spotPaint(sp,g,mode,scrim,round){sp.classList.add("on");sp.classList.toggle("spotlight",mode==="spotlight");sp.classList.toggle("ring",mode==="ring");sp.classList.toggle("round",!!round);
  sp.style.setProperty("--scrim",scrim);sp.style.left=g[0]+"px";sp.style.top=g[1]+"px";sp.style.width=g[2]+"px";sp.style.height=g[3]+"px"}
function fcTick(){G.fcRaf=0;var sp=ph.querySelector(".gspot:not(.gdspot)");if(!G.on){if(sp)sp.remove();return}
  var st=S.guideSteps[G.i],el=stepEl(st),cfg=Object.assign({},S.guideCfg,st&&st.cue?{cue:st.cue}:{}),k=G.hl?G.hl.k:-1,cues=st&&st.cues||[],now=-1;
  for(var c=0;c<cues.length;c++){var ix=cueIndex(cues[c]);if(ix>=0&&k>=ix)now=c}
  if(!G.fc||G.fc.i!==G.i)G.fc={cue:-2,i:G.i};
  var img=stepImg(el);if(now!==G.fc.cue){G.fc.cue=now;if(img)zoomImg(img,now>=0?cues[now].region:null)}
  var p=ph.getBoundingClientRect(),q=p.width/ph.offsetWidth,box=scr.getBoundingClientRect(),r=null;
  if(el){r=now>=0&&img?regionRect(img,cues[now].region):visRect(el)}
  /* a new target: glide from where the light was, towards where the target is now (so a scroll during the glide is followed) */
  var key=G.i+":"+now,tn=performance.now();if(G.fc.key!==key){G.fc.key=key;G.fc.at=tn;G.fc.from=G.fc.last||null}
  fcBar(cfg.cue==="bar"&&r&&!G.paused&&!G.askOpen&&!G.moment?(st.ref==="hero"&&scr.querySelector(".hero .w")||el):null,p,q,box);
  if(cfg.cue==="none"||cfg.cue==="bar"||!r||G.paused||G.askOpen||G.moment){if(sp)sp.classList.remove("on")}
  else{if(!sp){sp=document.createElement("div");sp.className="gspot";sp.setAttribute("aria-hidden","true");ph.appendChild(sp)}
    var t=Math.max(r.top,box.top+4),b=Math.min(r.bottom,box.bottom-4),pad=now>=0?4:6;
    var g=[(r.left-p.left)/q-pad,(t-p.top)/q-pad,r.width/q+2*pad,Math.max(0,b-t)/q+2*pad],f=G.fc.from,e=S.reduce||!f?1:Math.min(1,(tn-G.fc.at)/550);
    if(e<1){e=1-Math.pow(1-e,3);g=g.map(function(v,i){return f[i]+(v-f[i])*e})}G.fc.last=g;
    spotPaint(sp,g,cfg.cue,cfg.scrim)}
  ph.classList.toggle("gspotting",true);
  pointAt(r&&cfg.point&&!G.paused?{x:(r.left+r.width/2-p.left)/q,y:((Math.max(r.top,box.top)+Math.min(r.bottom,box.bottom))/2-p.top)/q,el:el}:null);
  G.fcRaf=requestAnimationFrame(fcTick)}
/* the left bar on the block told (its right edge FC_BAR_GAP px left of the block's left edge, the block's seen height); a new step's block:
   the old bar fades out, a new one fades in */
var FC_BAR_W=4,FC_BAR_GAP=6;
/* the card's own top bar over its scroll (the close, the title band, settings): what lies under it is not seen; its bottom in the
   phone's px (at least 0) */
function cardTopClear(){var p=ph.getBoundingClientRect(),q=p.width/ph.offsetWidth||1,y=0;ph.querySelectorAll(".topband,.chrome,.sticky,.topbar").forEach(function(e){var r=e.getBoundingClientRect();if(r.height>1&&r.top-p.top<140&&getComputedStyle(e).display!=="none")y=Math.max(y,(r.bottom-p.top)/q)});return y}
/* the block's drawn height: its seen box, from its first drawn part's top to its last's bottom (not the item's own spacing) */
function fcBarRect(el){var r=visRect(el),t=Infinity,b=-Infinity;[].forEach.call(el.children,function(k){var q=k.getBoundingClientRect();if(q.height>1){t=Math.min(t,q.top);b=Math.max(b,q.bottom)}});
  return t<b?{left:r.left,top:Math.max(r.top,t),bottom:Math.min(r.bottom,b)}:r}
function fcBar(el,p,q,box){var bar=ph.querySelector(".gtold:not(.out)");if(bar&&(!el||bar._el!==el)){bar.classList.remove("on");bar.classList.add("out");(function(b){setTimeout(function(){b.remove()},340)})(bar);bar=null}
  if(!el)return;var r=fcBarRect(el),t=Math.max(r.top,box.top+4,p.top+cardTopClear()*q),b=Math.min(r.bottom,box.bottom-4);if(b-t<8)return;
  if(!bar){bar=document.createElement("div");bar.className="gtold";bar.setAttribute("aria-hidden","true");bar._el=el;ph.appendChild(bar);void bar.offsetWidth;bar.classList.add("on")}
  bar.style.left=Math.max(2,(r.left-p.left)/q-FC_BAR_GAP-FC_BAR_W).toFixed(1)+"px";bar.style.top=((t-p.top)/q).toFixed(1)+"px";bar.style.height=((b-t)/q).toFixed(1)+"px"}
on("stopTour.before",function(){unzoomAll();var sp=ph.querySelector(".gspot");if(sp)sp.remove();ph.querySelectorAll(".gtold").forEach(function(b){b.remove()});ph.classList.remove("gspotting")});

/* Editor: Guide tab gets Focus, after Captions; Specific Marker > Guide script gets each step's spots, drawn on its picture */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var ca=d.querySelector('[data-fo="s7cap"]');if(!ca)return d.innerHTML;
  ca.insertAdjacentHTML("afterend",comp("s7focus","the left bar on what is told, the zoom to a spot in a picture, the mascot pointing, a scroll by the visitor",
    row("Focus",pseg("guideCfg.cue",["bar","spotlight","ring","none"],["The left bar","Dim the rest","Soft ring","Off"]),"How the block being told is marked. The left bar: a slim bar in the card's accent fades in beside the block's left edge and breathes softly while it is told; nothing else changes, so the visitor keeps the sense of the page. Dim the rest: the rest of the card dims and a soft light rings the block (the spotlight). Soft ring: only the ring. Off: nothing marks it. One at a time; it follows the scroll and hides while the visitor takes over. Reduce Motion: the bar does not breathe.")+
    row("How dark",pslider("guideCfg.scrim",0.2,0.6,0.05),"Dim the rest only: how dark the rest of the card goes (0.4 = 40 %).",true)+
    row("Into pictures",pseg("guideCfg.zoom",["zoom","spot"],["Slow zoom","Light the spot"]),"When the guide names a spot in a picture (the step's spots, in the Guide script). Slow zoom: the picture glides in to the spot (1.5 s). Light the spot: the picture stays and the spotlight closes on the spot. Reduce Motion: a crossfade instead of the glide.")+
    row("The mascot points",pseg("guideCfg.point",["true","false"],["On","Off"]),"The mascot stands level with what is being told and points at it with one arm (with the mascot on).")+
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
    "A word of what the guide says, and the spot in the picture it means: when the voice reaches the word, the picture zooms to the spot and the mascot points at it. Drag on the picture to draw the spot."));
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
/* each row's demo: the light on the block being told; the zoom on the step that names three spots */
rowDemos({"guideCfg.cue":{tour:"g1",ringAfter:900,ring:".gtold|.gspot|.gfocus"},"guideCfg.scrim":{tour:"g1",act:function(){if(S.guideCfg.cue!=="spotlight")gdChip("How dark: for Dim the rest (Focus); the left bar dims nothing")},ring:".gspot|.gtold|.gfocus"},"guideCfg.zoom":{tour:"g3",ringAfter:1500,ring:".gspot|.gfocus"},"guideCfg.point":{tour:"g1",ring:".mascot"}});
