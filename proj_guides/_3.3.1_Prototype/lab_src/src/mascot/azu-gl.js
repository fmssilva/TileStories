/* Azu, the tile of light: the WebGL shader, its forms by state, the peek from behind a block, the walk to the wall. */
/* Unity: the same shader as a Shader Graph Custom Function (HLSL) on a quad rendered by an "Azu camera" into a render texture,
   shown as an element inside the card (overflow: hidden makes the peek); the walk swaps it for a world-space Azu on the wall. */
S.guideCfg.azuStyle="glb";
/* the style: glaze colours, pattern, corner radius, the light's four colours (shader properties; mascot/tile-styles.js sets them from the Editor) */
var AZU_UNI={uCream:[.96,.93,.87],uInk:[.12,.31,.61],uInk2:[.12,.31,.61],uMotif:0,uRound:.12,
  uP0:[.48,.86,1],uP1:[.66,.52,1],uP2:[1,.55,.84],uP3:[1,.8,.66]};
var AZU_FS="precision highp float;uniform vec2 uRes;uniform float uT,uMorph,uBlink,uMood,uTalk,uMotif,uRound;uniform vec3 uCream,uInk,uInk2,uP0,uP1,uP2,uP3;\n"+
"float h3(vec3 p){return fract(sin(dot(p,vec3(127.1,311.7,74.7)))*43758.5453);}\n"+
"float nz(vec3 p){vec3 i=floor(p),f=fract(p);f=f*f*(3.-2.*f);return mix(mix(mix(h3(i),h3(i+vec3(1,0,0)),f.x),mix(h3(i+vec3(0,1,0)),h3(i+vec3(1,1,0)),f.x),f.y),mix(mix(h3(i+vec3(0,0,1)),h3(i+vec3(1,0,1)),f.x),mix(h3(i+vec3(0,1,1)),h3(i+vec3(1,1,1)),f.x),f.y),f.z);}\n"+
"mat3 rY(float a){float c=cos(a),s=sin(a);return mat3(c,0.,-s,0.,1.,0.,s,0.,c);}mat3 rX(float a){float c=cos(a),s=sin(a);return mat3(1.,0.,0.,0.,c,s,0.,-s,c);}\n"+
"float box(vec3 p,vec3 b,float r){vec3 q=abs(p)-b;return length(max(q,0.))+min(max(q.x,max(q.y,q.z)),0.)-r;}\n"+
"mat3 R;vec3 B;float map(vec3 p){return box(R*p,B,uRound);}\n"+
"vec3 nrm(vec3 p){vec2 e=vec2(.002,0.);return normalize(vec3(map(p+e.xyy)-map(p-e.xyy),map(p+e.yxy)-map(p-e.yxy),map(p+e.yyx)-map(p-e.yyx)));}\n"+
"vec2 motif(vec2 u){u=abs(u);\n"+
"  if(uMotif<.5){float c=step(length(u-vec2(1.))-.52,0.);float d=step(u.x+u.y,.34);float ring=step(abs(length(u)-.5),.035);return vec2(max(d,ring),c);}\n"+
"  if(uMotif<1.5){float s=u.x+u.y;float l=step(abs(s-.66),.045)+step(abs(s-1.3),.045);float b=step(.93,max(u.x,u.y));return vec2(min(1.,l+b),step(abs(s-.98),.03)*(1.-b));}\n"+
"  if(uMotif<2.5){float q=length(u-vec2(1.));float c=step(q,.44)-step(q,.33)+step(q,.2);float a=(u.x+u.y)*.7071,b=(u.x-u.y)*.7071;float lf=step(abs(b),.075*sin(clamp((a-.58)/.4,0.,1.)*3.1416))*step(.58,a)*step(a,.98);return vec2(min(1.,c),lf);}\n"+
"  return vec2(0.);}\n"+
"vec3 pal(float t){vec3 a=uP0,b=uP1,c=uP2,d=uP3;float f=fract(t)*4.;return f<1.?mix(a,b,f):f<2.?mix(b,c,f-1.):f<3.?mix(c,d,f-2.):mix(d,a,f-3.);}\n"+
"void main(){vec2 uv=(gl_FragCoord.xy*2.-uRes)/uRes.y;float t=uT;\n"+
" R=rX(.22*sin(t*.5)+.1)*rY(.45*sin(t*.37)+uMorph*t*.6);float th=mix(.16,.58,uMorph);float kr=uRound-.12;B=vec3(.6-kr,.6-kr,max(.03,th-kr))*(1.+.02*sin(t*1.7));\n"+
" vec3 ro=vec3(0.,0.,2.7),rd=normalize(vec3(uv*.62,-1.5));float d=0.,dist=0.;bool hit=false;\n"+
" for(int i=0;i<56;i++){dist=map(ro+rd*d);if(dist<.001){hit=true;break;}d+=dist;if(d>5.)break;}\n"+
" vec3 col=vec3(0.);float a=0.;\n"+
" if(hit){vec3 p=ro+rd*d,n=nrm(p),lp=R*p,ln=R*n,l=normalize(vec3(-.5,.7,.9)),v=-rd;\n"+
"  float front=step(.7,abs(ln.z));vec2 fu=lp.xy/.6;\n"+
"  vec3 cream=uCream,cob=mix(uInk,vec3(.36,.2,.5),clamp(-uMood,0.,1.));vec2 mo=motif(fu);\n"+
"  vec3 solid=mix(cob*.75,mix(mix(cream,cob,mo.x),uInk2,mo.y),front);\n"+
"  float ex=.27,ey=.14,er=.13*mix(1.,.12,uBlink);float eye=min(length((fu-vec2(-ex,ey))*vec2(1.,.13/er)),length((fu-vec2(ex,ey))*vec2(1.,.13/er)));\n"+
"  float mouth=length((fu-vec2(0.,-.3))*vec2(1.-.2*uTalk,1./(.03+.15*uTalk)))-.16;\n"+
"  if(front>.5&&lp.z>0.){solid=mix(solid,vec3(.1),step(eye,.13));solid=mix(solid,vec3(1.),step(length(fu-vec2(-ex+.045,ey+.05)),.04)+step(length(fu-vec2(ex+.045,ey+.05)),.04));solid=mix(solid,vec3(.45,.1,.15),step(mouth,0.)*step(.02,uTalk+.05*uMood));}\n"+
"  float df=.55+.45*max(dot(n,l),0.),sp=pow(max(dot(reflect(-l,n),v),0.),42.),fr=pow(1.-max(dot(n,v),0.),3.);\n"+
"  vec3 sc=solid*df+sp*.85+fr*.35*vec3(.9,.95,1.);\n"+
"  vec3 g=fract(lp*9.)-.5;float dots=smoothstep(.2,.05,length(front>.5?g.xy:(abs(ln.x)>.7?g.yz:g.xz)));\n"+
"  vec3 inner=vec3(0.);for(int k=0;k<10;k++){vec3 q=lp+(R*rd)*float(k)*.12;inner+=pal(nz(q*1.8+t*.3)+t*.04)*nz(q*2.6-t*.35)*.1;}\n"+
"  vec3 lc=mix(vec3(.86,.84,1.),inner*2.4,.6)+dots*vec3(1.)*.55+fr*vec3(.8,.75,1.)*.6;float edge=smoothstep(.0,.35,nz(lp*7.+t)+.4-fr);\n"+
"  col=mix(sc,lc,uMorph);a=mix(1.,.45+.4*edge,uMorph);}\n"+
" else{float glow=exp(-max(dist,0.)*14.)*uMorph*.6;col=pal(t*.1+uv.x*.3)*glow;a=glow*.8;}\n"+
" gl_FragColor=vec4(col*a,a);}";
var AZU_VS="attribute vec2 p;void main(){gl_Position=vec4(p,0.,1.);}";
/* one renderer, one canvas, moved wherever Azu is drawn */
var AzuGL=(function(){var cv=null,gl=null,U={},raf=0,st={morph:0,to:0,blink:0,mood:0,talk:0,t0:performance.now()},nextBlink=0,pv=null;
  /* one canvas with the shader compiled on it: the guide's live Azu has one, the Editor's preview its own (so a preview never draws over the guide) */
  function make(c){var g=null;try{g=c.getContext("webgl",{premultipliedAlpha:true,alpha:true,antialias:true,preserveDrawingBuffer:true})}catch(e){g=null}if(!g)return null;
    var sh=function(t,src){var o=g.createShader(t);g.shaderSource(o,src);g.compileShader(o);if(!g.getShaderParameter(o,g.COMPILE_STATUS))throw new Error(g.getShaderInfoLog(o));return o};var pr,u={};
    try{pr=g.createProgram();g.attachShader(pr,sh(g.VERTEX_SHADER,AZU_VS));g.attachShader(pr,sh(g.FRAGMENT_SHADER,AZU_FS));g.linkProgram(pr);g.useProgram(pr)}catch(e){console.warn(e);return null}
    var b=g.createBuffer();g.bindBuffer(g.ARRAY_BUFFER,b);g.bufferData(g.ARRAY_BUFFER,new Float32Array([-1,-1,1,-1,-1,1,1,1]),g.STATIC_DRAW);var lp=g.getAttribLocation(pr,"p");g.enableVertexAttribArray(lp);g.vertexAttribPointer(lp,2,g.FLOAT,false,0,0);
    ["uRes","uT","uMorph","uBlink","uMood","uTalk"].concat(Object.keys(AZU_UNI)).forEach(function(n){u[n]=g.getUniformLocation(pr,n)});g.clearColor(0,0,0,0);return{gl:g,U:u,cv:c}}
  /* one picture: the moment, the form, the blink, the mood, the mouth, and the style (AZU_UNI) */
  function paint(r,t,morph,blink,mood,talk){var g=r.gl,u=r.U;g.viewport(0,0,r.cv.width,r.cv.height);g.clear(g.COLOR_BUFFER_BIT);g.uniform2f(u.uRes,r.cv.width,r.cv.height);g.uniform1f(u.uT,t);g.uniform1f(u.uMorph,morph);g.uniform1f(u.uBlink,blink);g.uniform1f(u.uMood,mood);g.uniform1f(u.uTalk,talk);
    Object.keys(AZU_UNI).forEach(function(n){var v=AZU_UNI[n];if(v.length===3)g.uniform3fv(u[n],v);else g.uniform1f(u[n],v)});g.drawArrays(g.TRIANGLE_STRIP,0,4)}
  function init(){if(gl!==null)return!!gl;cv=document.createElement("canvas");cv.className="azgl";cv.width=192;cv.height=192;var r=make(cv);if(!r){gl=false;return false}gl=r.gl;U=r;return true}
  function frame(now){raf=0;if(!cv||!cv.isConnected)return;var dt=Math.min(.05,(now-(st.last||now))/1000);st.last=now;var still=S.reduce;
    st.morph+=(st.to-st.morph)*Math.min(1,dt*(still?30:3.2));if(now>nextBlink){st.blinkT=now;nextBlink=now+2500+Math.random()*2500}st.blink=st.blinkT&&now-st.blinkT<140&&!still?1:0;
    /* a still Azu (Reduce Motion) stands at a moment where the face is not under the highlight (t = 1.5 blows the front out) */
    paint(U,still?4.5:(now-st.t0)/1000,st.morph,st.blink,st.mood,st.talk);raf=requestAnimationFrame(frame)}
  return{ok:function(){return init()},canvas:function(){return init()?cv:null},set:function(k,v){st[k]=v},get:function(k){return st[k]},
    start:function(){if(init()&&!raf)raf=requestAnimationFrame(frame)},
    /* one still frame (at rest or as light) on the preview's own canvas, as a picture: the Editor's preview */
    still:function(morph,t){if(pv===null){var c=document.createElement("canvas");c.width=c.height=192;pv=make(c)||false}if(!pv)return null;paint(pv,t===undefined?4.5:t,morph,0,0,0);return pv.cv.toDataURL("image/png")},
    pixel:function(){if(!gl)return null;var px=new Uint8Array(4);gl.readPixels(96,96,1,1,gl.RGBA,gl.UNSIGNED_BYTE,px);return Array.from(px)}}})();
/* Azu's state picks its form: listening, thinking and travelling turn it into light; moods tint it */
function azuForm(){if(S.guideCfg.azuStyle!=="light")return;var m=ph.querySelector(".mascot"),ts=m&&m.dataset.state,md=m&&m.dataset.mood;
  AzuGL.set("to",G.azWalk?1:ts==="listening"?.85:(ts==="thinking"||(G.azThinkTill||0)>performance.now())?.6:md==="surprised"?.25:0);AzuGL.set("mood",md==="serious"?-1:md==="big"?.6:0)}
setInterval(azuForm,120);
/* the mouth: the same shapes the 2D Azu uses, as an opening */
on("azMouth.after",function(m,shape){AzuGL.set("talk",{A:1,E:.55,O:.8,M:0,big:.7,chew:.4,surprised:.6}[shape]||0)});
/* the guide's Azu: with Tile of light, the canvas replaces the drawing (the reactions move the canvas as they moved the drawing) */
on("renderGuide.after",function(){if(S.guideCfg.azuStyle!=="light")return;var m=ph.querySelector(".mascot"),c=AzuGL.canvas();if(!m||!c)return;
  var sv=m.querySelector("svg");if(sv)sv.style.display="none";if(c.parentNode!==m)m.insertBefore(c,m.firstChild);m.classList.add("light");AzuGL.start()});
/* ----- peek: Azu rises from behind the block being told (a box that clips at the block's top edge) ----- */
function azPeek(el){var o=ph.querySelector(".azpeek");if(o)o.remove();el=el||stepEl(S.guideSteps[G.i])||scr.querySelector('[data-ref^="b:"]');if(!el)return;var c=AzuGL.canvas();
  var box=document.createElement("div");box.className="azpeek";box.setAttribute("aria-hidden","true");var inner=document.createElement("div");inner.className="azpeek-in";
  if(c&&S.guideCfg.azuStyle==="light"){inner.appendChild(c);AzuGL.start()}else inner.innerHTML=MASCOT;box.appendChild(inner);
  /* in the scroller's content, just above the block's top edge: the block hides Azu until it rises (blocks clip their own overflow) */
  /* over the card, just above the block's top edge, following the block on the screen each frame (a sheet that moves, a scroll,
     a sticky section): the box clips Azu at the block's edge, so Azu rises from behind it */
  var mm=ph.querySelector(".mascot");if(mm)mm.style.visibility="hidden";
  ph.appendChild(box);var follow=function(){if(!box.isConnected)return;var r=el.getBoundingClientRect(),pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight;
    box.style.top=((r.top-pr.top)/q-52)+"px";box.style.left=((r.right-pr.left)/q-92)+"px";requestAnimationFrame(follow)};follow();requestAnimationFrame(function(){box.classList.add("up")});
  setTimeout(function(){box.classList.remove("up");setTimeout(function(){box.remove();var m=ph.querySelector(".mascot");if(m){m.style.visibility="";if(c&&S.guideCfg.azuStyle==="light")m.insertBefore(c,m.firstChild)}},700)},2600)}
/* ----- walk into the wall: Azu leaves the panel as light, travels to the point on the wall, and becomes a tile there ----- */
function azWalk(id,done){var poi=S.wallPois.filter(function(p){return p.id===id})[0];if(!poi){done&&done();return}if(S.stop!=="closed"){S.stop="closed";renderPhone(false)}
  var o=ph.querySelector(".azwalker");if(o)o.remove();var w=document.createElement("div");w.className="azwalker";w.setAttribute("aria-hidden","true");var c=AzuGL.canvas();if(c){w.appendChild(c);AzuGL.start()}else w.innerHTML=MASCOT;ph.appendChild(w);
  var from={x:ph.offsetWidth-44,y:ph.offsetHeight-120},to=poiScreen(poi),t0=performance.now(),dur=S.reduce?1:1600;G.azWalk=true;AzuGL.set("to",1);
  var step=function(now){var k=Math.min(1,(now-t0)/dur),e=k<.5?2*k*k:1-Math.pow(-2*k+2,2)/2,x=from.x+(to.x-from.x)*e,y=from.y+(to.y-from.y)*e-Math.sin(e*Math.PI)*70,s=1-.42*e;
    w.style.transform="translate("+(x-32).toFixed(1)+"px,"+(y-70).toFixed(1)+"px) scale("+s.toFixed(3)+")";if(k<1){requestAnimationFrame(step);return}
    G.azWalk=false;AzuGL.set("to",0);w.classList.add("arrived");done&&done()};if(S.reduce)step(t0+dur);else requestAnimationFrame(step)}
/* in Ask about the view, Azu walks to the first point the answer lights */
on("camLight.after",function(ids){if(S.guideCfg.azuStyle==="light"&&S.guideCfg.azuWalk&&ids&&ids.length&&G.cam.open){var w=ph.querySelector(".azwalker");if(!w||w.dataset.at!==ids[0])azWalk(ids[0],function(){var x=ph.querySelector(".azwalker");if(x)x.dataset.at=ids[0]})}});
S.guideCfg.azuWalk=true;
on("camLayer.after",function(){if(!G.cam.open){var w=ph.querySelector(".azwalker");if(w)w.remove()}});

/* Editor: 6 · Guide > Mascot gets Style, Walks to the wall, and a Try for the peek and the walk */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7mascot",row("Style",pseg("guideCfg.azuStyle",["light","tile"],["Tile of light","Tile (2.5D)"]),"Tile of light: a glazed tile drawn by a shader. It turns into a holographic dot cube with colour clouds inside to listen, think and travel, then forms again; its moods tint it, its mouth opens with the words. Tile: the 2.5D drawing. Unity: the same shader as a Shader Graph Custom Function, rendered by an Azu camera into a render texture shown inside the card.")+
    row("Walks to the wall",pseg("guideCfg.azuWalk",["true","false"],["On","Off"]),"In Ask about the view, Azu leaves the panel as light, travels to the first point the answer lights, and becomes a tile there (smaller: farther away). Unity: its screen box is handed to a world-space Azu on the wall’s plane, the size matched by depth.")+
    row("Try",'<div class="btnrow"><button class="del" type="button" data-azqtry="peek">Peek from behind a block</button><button class="del" type="button" data-azqtry="walk">Walk to the Torreão</button><button class="del" type="button" data-azqtry="think">Think</button></div>',"Peek: Azu rises from behind the block’s top edge (a box that clips it).",true));
  return d.innerHTML});
ed.addEventListener("click",function(e){var t=e.target.closest("[data-azqtry]");if(!t)return;e.stopPropagation();var k=t.dataset.azqtry;
  if(k==="peek"){if(!G.on){S.view="card";if(S.stop==="closed"||S.stop==="peek")S.stop="half";renderPhone(false)}azPeek()}
  else if(k==="walk"){if(G.on)stopTour();S.view="card";G.cam.open=true;S.stop="closed";renderPhone(false);azWalk("torreao")}
  else{if(!G.on)startTour();G.azThinkTill=performance.now()+3200}},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(azuStyle|azuWalk|"));
/* the row's demo: a question on the wall, Azu walking to the point it lights */
rowDemos({"guideCfg.azuWalk":{wall:"open",act:function(){camAsk("","what")},ringAfter:900,ring:".azwalker|.camask"}});
