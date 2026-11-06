/* Azu as a 3D model: the GLB drawn by three.js; the source, a clip, Import, Generate from a picture. */

/* what a mascot GLB must hold (the contract of _3.7 section 1): the clips, the morphs, the front arm as its own mesh */
var AZ3D_CLIPS=["idle","talk","listen","think","point","walk","wave"],AZ3D_MORPHS=["Blink","Open","Wide","Round"];
var AZ3D_THREE="https://cdn.jsdelivr.net/npm/three@0.160.0/+esm",AZ3D_LOADER="https://cdn.jsdelivr.net/npm/three@0.160.0/examples/jsm/loaders/GLTFLoader.js/+esm";
var Azu3D=(function(){var T=null,GL=null,cv=null,R=null,scene=null,cam=null,model=null,mixer=null,acts={},raf=0,last=0,
    st={ready:false,failed:false,err:"",loading:null,clip:"",force:null,forceTill:0,mouth:{Open:0,Wide:0,Round:0},to:{Open:0,Wide:0,Round:0},blinkT:0,nextBlink:0,frames:0,src:"built-in",info:null};
  function lib(){return T?Promise.resolve():Promise.all([import(AZ3D_THREE),import(AZ3D_LOADER)]).then(function(m){T=m[0];GL=m[1].GLTFLoader})}
  function view(){cv=document.createElement("canvas");cv.className="azgl az3d";cv.width=cv.height=256;
    R=new T.WebGLRenderer({canvas:cv,alpha:true,antialias:true,preserveDrawingBuffer:true});R.setPixelRatio(1);R.setSize(256,256,false);R.outputColorSpace=T.SRGBColorSpace;
    scene=new T.Scene();cam=new T.PerspectiveCamera(28,1,.05,50);scene.add(new T.HemisphereLight(0xffffff,0x8090b0,1.7));var d=new T.DirectionalLight(0xffffff,1.5);d.position.set(-1.5,2.5,3);scene.add(d)}
  /* the box from each mesh's own geometry in its rest (bind) pose: Box3.setFromObject would skin a SkinnedMesh with bones not yet posed */
  function bounds(root){var box=new T.Box3();root.updateMatrixWorld(true);root.traverse(function(o){if(o.isMesh&&o.geometry){o.geometry.computeBoundingBox();box.union(o.geometry.boundingBox.clone().applyMatrix4(o.matrixWorld))}});return box}
  function parse(buf){return new Promise(function(res,rej){new GL().parse(buf,"",res,rej)})}
  /* each model is decoded and parsed once; a switch back reuses it */
  var cache={};
  function get(m){if(cache[m.id])return Promise.resolve(cache[m.id]);var b=atob(m.glb.split(",").pop()),u=new Uint8Array(b.length);for(var i=0;i<b.length;i++)u[i]=b.charCodeAt(i);
    st.parses=(st.parses||0)+1;return parse(u.buffer).then(function(g){cache[m.id]=g;return g})}
  /* what a GLB holds, against the contract: each missing part is named */
  function check(g){var clips=g.animations.map(function(a){return a.name}),morphs=[],meshes=[],front=false;
    g.scene.traverse(function(o){if(o.isMesh){meshes.push(o.name);if(/armfront|front_?arm/i.test(o.name))front=true;Object.keys(o.morphTargetDictionary||{}).forEach(function(k){if(morphs.indexOf(k)<0)morphs.push(k)})}});
    var sz=bounds(g.scene).getSize(new T.Vector3());
    return{clips:clips,morphs:morphs,meshes:meshes.length,front:front,size:[sz.x,sz.y,sz.z],missClips:AZ3D_CLIPS.filter(function(c){return clips.indexOf(c)<0}),missMorphs:AZ3D_MORPHS.filter(function(k){return morphs.indexOf(k)<0})}}
  function use(g,src){if(model)scene.remove(model);model=g.scene;scene.add(model);
    var box=bounds(model),c=box.getCenter(new T.Vector3()),sz=box.getSize(new T.Vector3()),h=Math.max(sz.y,sz.x)*1.15;
    cam.position.set(c.x,c.y,c.z+sz.z/2+h/2/Math.tan(14*Math.PI/180));cam.lookAt(c);
    mixer=new T.AnimationMixer(model);acts={};g.animations.forEach(function(a){var x=mixer.clipAction(a);if(a.name==="point"){x.setLoop(T.LoopOnce);x.clampWhenFinished=true}acts[a.name]=x});
    st.clip="";st.src=src;st.info=check(g);play("idle",0)}
  function morph(name,v){if(!model)return;model.traverse(function(o){var i=o.morphTargetDictionary&&o.morphTargetDictionary[name];if(i!==undefined)o.morphTargetInfluences[i]=v})}
  function play(name,fade){if(!acts[name])name=acts.idle?"idle":Object.keys(acts)[0];if(!name||name===st.clip)return;var nx=acts[name],cu=acts[st.clip];
    nx.reset().play();if(cu&&fade){nx.crossFadeFrom(cu,fade,false)}else if(cu)cu.stop();st.clip=name}
  function frame(now){raf=0;if(!cv||!cv.isConnected||!model)return;var dt=Math.min(.05,(now-(last||now))/1000);last=now;var still=S.reduce;
    var want=st.force&&now<st.forceTill?st.force:az3dWant();play(want,.25);if(still){mixer.setTime(0)}else mixer.update(dt);
    Object.keys(st.mouth).forEach(function(k){st.mouth[k]+=(st.to[k]-st.mouth[k])*Math.min(1,dt*18);morph(k,st.mouth[k])});
    if(now>st.nextBlink){st.blinkT=now;st.nextBlink=now+2500+Math.random()*2500}morph("Blink",!still&&now-st.blinkT<140?1:0);
    R.render(scene,cam);st.frames++;raf=requestAnimationFrame(frame)}
  return{
    load:function(){if(st.loading)return st.loading;if(location.hash==="#selftest"){st.failed=true;st.err="not loaded in the self-test";return st.loading=Promise.resolve()}var m=az3dModel();st.loading=lib().then(function(){view();return get(m)})
      .then(function(g){use(g,m.name);st.model=m.id;st.ready=true;az3dPlace();if(ed.querySelector('[data-fo="s7mascot"]'))renderEditor()}).catch(function(e){st.failed=true;st.err=String(e&&e.message||e);console.warn("Azu3D",e);if(ed.querySelector('[data-fo="s7mascot"]'))renderEditor()});return st.loading},
    /* Import: a GLB from the developer's computer, checked against the contract; it replaces the built-in one when it has clips */
    importFile:function(file){return lib().then(function(){if(!cv)view();return file.arrayBuffer()}).then(parse).then(function(g){var info=check(g);if(g.animations.length){use(g,file.name);st.model=az3dModel().id;st.ready=true;az3dPlace()}st.lastImport={name:file.name,info:info};return info})
      .catch(function(e){st.lastImport={name:file.name,err:String(e&&e.message||e)};return null})},
    /* the Editor picked another model: parse it (once) and draw it; an imported file stays until the next pick */
    sync:function(){var m=az3dModel();if(!st.ready||st.swapping||st.model===m.id)return;st.swapping=true;
      get(m).then(function(g){use(g,m.name);st.model=m.id;st.swapping=false;if(ed.querySelector('[data-fo="s7mascot"]'))renderEditor()}).catch(function(e){st.swapping=false;st.model=m.id;st.err=String(e&&e.message||e);console.warn("Azu3D",e)})},
    canvas:function(){return st.ready?cv:null},start:function(){if(st.ready&&!raf)raf=requestAnimationFrame(frame)},
    /* a continuous mouth (mascot/mouth.js: Azure's facial curves, or the loudness): the morphs ease to these each frame */
    mouthTo:function(o,w,r){st.to={Open:o||0,Wide:w||0,Round:r||0}},
    viseme:function(shape){var m={A:{Open:1},E:{Wide:.8,Open:.3},O:{Round:1},big:{Open:.8,Wide:.3},chew:{Open:.4},surprised:{Round:.8,Open:.3}}[shape]||{};st.to={Open:m.Open||0,Wide:m.Wide||0,Round:m.Round||0}},
    force:function(clip,ms){st.force=clip;st.forceTill=performance.now()+(ms||3000)},
    /* how much of the frame the whole model takes: the largest |x| or |y| of its box's corners on the screen (1 = the edge) */
    fits:function(){if(!model)return null;var b=bounds(model),m=0;[b.min.x,b.max.x].forEach(function(x){[b.min.y,b.max.y].forEach(function(y){[b.min.z,b.max.z].forEach(function(z){var v=new T.Vector3(x,y,z).project(cam);m=Math.max(m,Math.abs(v.x),Math.abs(v.y))})})});return m},
    state:function(){return st},morphNow:function(name){var v=null;if(model)model.traverse(function(o){var i=o.morphTargetDictionary&&o.morphTargetDictionary[name];if(i!==undefined)v=o.morphTargetInfluences[i]});return v},
    pixel:function(){if(!R)return null;var g=R.getContext(),px=new Uint8Array(4);g.readPixels(128,128,1,1,g.RGBA,g.UNSIGNED_BYTE,px);return Array.from(px)}}})();
function az3dOn(){return S.guideCfg.azuStyle==="glb"}
/* the guide's state picks the clip: the same states the other sources show */
function az3dWant(){var m=ph.querySelector(".mascot"),ts=m&&m.dataset.state,now=performance.now();
  if(G.azWalk||ph.querySelector(".azwalker:not(.arrived)"))return"walk";if(ts==="listening")return"listen";if(ts==="thinking"||(G.azThinkTill||0)>now)return"think";
  if(m&&m.classList.contains("pointing"))return"point";if((G.azWaveTill||0)>now)return"wave";if(ts==="speaking")return"talk";return"idle"}
/* the canvas goes where Azu is: the guide's mascot, the peek from behind a block, the walk to the wall */
function az3dPlace(){if(!az3dOn()){var o=ph.querySelector(".az3d");if(o){var h=o.parentNode;o.remove();h.querySelectorAll("svg,.azgl").forEach(function(x){x.style.display=""})}return}var c=Azu3D.canvas();if(!c){Azu3D.load();return}Azu3D.sync();
  var host=ph.querySelector(".azwalker")||ph.querySelector(".azpeek-in")||ph.querySelector(".mascot");if(!host)return;
  if(c.parentNode!==host){host.querySelectorAll("svg,.azgl:not(.az3d)").forEach(function(x){x.style.display="none"});host.insertBefore(c,host.firstChild)}
  if(host.classList.contains("mascot"))host.classList.add("light");Azu3D.start()}
setInterval(az3dPlace,150);
on("renderGuide.after",function(){az3dPlace()});
on("azMouth.after",function(m,shape){Azu3D.viseme(shape)});
on("startTour.before",function(){G.azWaveTill=performance.now()+2600});

/* ----- Editor: Guide tab > Azu: Mascot source, the 3D model's clips, Import, Generate from a picture ----- */
var AZ3D_GEN=[["Hyper3D Rodin","the add-on's shared trial key: no credit left on 2026-11-03","a Rodin key (Business plan) in the add-on"],
  ["Hunyuan 3D (Tencent)","about 20 a day on the web; 200 credits once on the API","a Tencent Cloud SecretId and SecretKey in the add-on; its licence may exclude the EU (check before use)"],
  ["Tripo","about 300 credits a month on the web (CC BY, not for commercial use)","a Tripo API key in the add-on"],
  ["Meshy","100 credits a month on the web (outputs CC BY 4.0)","the API starts at Pro (about $20 a month)"]];
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  var r=rowEl(d,"s7mascot","Style");if(r)r.outerHTML=row("Mascot source",pseg("guideCfg.azuStyle",["light","tile","glb"],["Tile of light","Tile (2.5D)","3D model (GLB)"]),"Tile of light: a glazed tile drawn by a shader (it turns to light to listen, think and travel). Tile (2.5D): the drawing. 3D model: a GLB with a skeleton, clips and morphs (built in Blender from the developer's picture), drawn by three.js here and by glTFast in Unity. Rive files and sprite sheets are other sources of the same contract (Unity import; the Lab does not draw them).");
  if(az3dOn()){var s=Azu3D.state(),i=s.info;
    addRows(d,"s7mascot",
      row("3D model",'<span class="uses">'+esc(s.failed?"Could not load three.js or the model ("+s.err+"): the guide shows the Tile of light.":!s.ready?"Loading…":s.src+" · clips: "+i.clips.join(", ")+" · morphs: "+i.morphs.join(", ")+(i.front?" · the front arm is its own mesh":""))+'</span>',"The guide's state picks the clip: idle; talk while it speaks; listen while the microphone is open; think while the answer comes; point when it points at a block; walk on the way to the wall; wave as the tour starts. The mouth opens with the words (the Open, Wide and Round morphs); it blinks every few seconds. Reduce Motion: still, no blink.",true)+
      row("Play a clip",'<div class="btnrow">'+AZ3D_CLIPS.map(function(c){return'<button class="del" type="button" data-az3d="'+c+'">'+c+'</button>'}).join("")+'</div>',"Plays the clip on the guide's Azu for three seconds (the guide opens if it is closed).",true)+
      row("Import…",'<label class="ubtn sm az3dimp">Choose a GLB…<input type="file" accept=".glb,model/gltf-binary" data-az3dimport="1" hidden></label>'+(s.lastImport?'<div class="uses az3dres">'+az3dReport(s.lastImport)+'</div>':""),"Checks a GLB against what Azu needs: the clips (idle, talk, listen, think, point, walk, wave), the morphs (Blink, Open, Wide, Round), the front arm as its own mesh (drawn in front of the card), and its size. A file with clips replaces the chosen model in this Lab until another model is picked.",true)+
      row("Generate from a picture",'<table class="tbl az3dgen"><thead><tr><th>Generator</th><th>Free</th><th>To use it from Blender</th></tr></thead><tbody>'+AZ3D_GEN.map(function(g){return'<tr><td>'+esc(g[0])+'</td><td>'+esc(g[1])+'</td><td>'+esc(g[2])+'</td></tr>'}).join("")+'</tbody></table>',"A generator makes one textured model from the picture, in Blender (the blender-mcp add-on's AI Model Generation). There it gets its skeleton, clips and morphs, and is exported as a GLB to import here. The tile is modelled by script instead: a generator's tile is a lumpy slab with the face baked in.",true),"Walks to the wall")}
  return d.innerHTML});
function az3dReport(r){if(r.err)return esc(r.name+": not a GLB the loader can read ("+r.err+").");var i=r.info,p=[];
  p.push(i.missClips.length?"missing clips: "+i.missClips.join(", "):"all seven clips");p.push(i.missMorphs.length?"missing morphs: "+i.missMorphs.join(", "):"all four morphs");
  p.push(i.front?"the front arm is its own mesh":"no front-arm mesh (name one Azu_ArmFront) so the arm cannot be drawn in front of the card");p.push("size "+i.size.map(function(v){return v.toFixed(2)}).join(" × ")+" (1 = one tile; Unity scales it to 0.15 m on the wall)");
  return esc(r.name+": "+p.join(" · ")+".")}
ed.addEventListener("click",function(e){var t=e.target.closest("[data-az3d]");if(!t)return;e.stopPropagation();if(!G.on){S.view="card";if(S.stop==="closed")S.stop="half";renderPhone(false);startTour();pauseTour()}Azu3D.force(t.dataset.az3d,3000)},true);
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.az3dimport||!t.files||!t.files[0])return;e.stopPropagation();Azu3D.importFile(t.files[0]).then(function(){renderEditor()})},true);
/* choosing the source loads the model (once) */
on("renderEditor.after",function(){if(az3dOn())Azu3D.load()});
/* the row's demo: the guide's Azu drawn by that source */
rowDemos({"guideCfg.azuStyle":{tour:"g1",ring:".mascot"}});
