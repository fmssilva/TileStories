/* The Editor's Blocks & Components part: element styles, motions, interactions, effects, patterns and the library, with their thumbnails, demos and fields (edCardV3). */
function noBtn(h){return h.replace(/<button/g,"<span").replace(/<\/button>/g,"</span>")}
function thumb(kind,style,cls){return noBtn('<div class="thumb'+(cls?" "+cls:"")+' cv'+(S.dark?" dark":"")+'"><div class="tin">'+renderEl(kind,style,{},0,true)+'</div></div>')}
function patThumb(p,cls){return noBtn('<div class="thumb tall'+(cls?" "+cls:"")+' cv'+(S.dark?" dark":"")+'"><div class="tin">'+p.rows.map(function(r){return renderEl(r.kind,styleById(r.style)||defStyle(r.kind),{},0,true)}).join("")+'</div></div>')}

/* ---------- motion, interaction demos ---------- */
function pathBadge(id){var p=PATH[id]||"bld";return '<span class="mpath">'+({nat:"NATIVE",bld:"BUILD",imp:"IMPORT"}[p])+'</span>'}
function motionSummary(m){var ans=firstAnswer("motionSummary.answer",m);if(ans!==undefined)return ans;if(m.type==="entrance")return "Entrance · "+(m.from==="scale"?"scales up":m.from==="none"?"fades":m.from==="wipe"?"wipes in":"from "+m.from)+(m.rot?" · tilts "+m.rot+"°":"")+" · "+m.dur+" ms"+(m.pieces!=="whole"?" · "+m.pieces+" one by one":"");
  if(m.type==="transition")return "Transition · "+({slide:"Swipe",cube:"Cube",deck:"Card Deck",flip:"Flip",fade:"Cross-fade",zoom:"Zoom"})[m.style]+" · "+m.dur+" ms";if(m.type==="scroll")return "With the scroll · "+m.factor;if(m.type==="imported")return ({lottie:"Lottie",rive:"Rive",video:"Video + alpha",unity:"Unity Animation"})[m.source]+" · "+m.file+" · "+({enter:"on view",loop:"loops",tap:"on tap",scrub:"with the scroll"})[m.trigger];return "Emphasis loop · "+m.dur+" ms";}
function motionDemo(m){var ans=firstAnswer("motionDemo.answer",m);if(ans!==undefined)return ans;
  var head='<div class="mhead"><div><b>'+esc(m.name)+'</b>'+pathBadge(m.id)+'<small>'+esc(motionSummary(m))+'</small></div><button type="button" data-play="'+m.id+'">&#9654; Play</button></div>',body="";
  if(m.type==="entrance"){
    if(m.pieces==="words")body=wrapMv('<h3 class="el-title">'+esc(L(DEMO.title))+'</h3>',m,0);
    else if(m.pieces==="letters")body=wrapMv('<h3 class="el-title a-centre">Ribeira Palace</h3>',m,0);
    else body=wrapMv('<div class="el-eyebrow t-family">'+esc(L(DEMO.eyebrow))+'</div>',m,0)+wrapMv('<h4 class="el-heading">'+esc(L(DEMO.heading))+'</h4>',m,1)+wrapMv('<p class="el-par" style="font-size:15px">'+esc(L(T("Later kings kept adding to it: a new wing and its corner tower, the Torreão.","Os reis seguintes foram acrescentando: uma nova ala e o seu torreão de canto.")))+'</p>',m,2)+wrapMv('<figure class="el-pic" style="margin:6px 0 0"><div class="fr" style="aspect-ratio:16/8"><img alt="" src="'+IMG.g2+'"></div></figure>',m,3);}
  else if(m.type==="transition")body='<div class="trv'+(m.style==="deck"?" deck":"")+'" data-trv="'+m.id+'" data-imgs="g1,g2,g3"></div><div class="ctrl"><button type="button" data-turn="-1">&#8249; Back</button><button type="button" data-turn="1">Next &#8250;</button></div>';
  else if(m.type==="scroll")body='<div class="mbox"><div class="pad">Scroll inside this box (or press Play)</div><figure class="el-pic"><div class="fr"><img alt="" src="'+IMG.g3+'" data-scroll="'+m.id+'"></div></figure><div class="pad">&#8595;</div><div class="pad"></div></div>';
  else if(m.type==="imported"){
    if(m.source==="video")body='<div class="anim" style="background:#1A1712;height:140px;position:relative;overflow:hidden" data-dust>'+Array(18).join("x").split("x").map(function(_,i){return'<i style="position:absolute;left:'+((i*53)%100)+'%;top:'+((i*37)%100)+'%;width:4px;height:4px;border-radius:50%;background:#F2C14E;box-shadow:0 0 8px #F2C14E;animation:pulse '+(1.2+(i%5)*.3)+'s ease-in-out infinite;opacity:.8"></i>'}).join("")+'<small style="position:absolute;bottom:8px;color:#EADBB0">Video with alpha over the card</small></div>';
    else if(m.source==="unity")body='<div class="anim" data-door style="perspective:600px;height:150px;display:flex;gap:2px;justify-content:center;align-items:center;background:#2A2016"><div class="door l" style="width:60px;height:110px;background:#7A4A22;transform-origin:left;transition:transform 1.2s cubic-bezier(.2,.8,.2,1);border:2px solid #C99A3B"></div><div class="door r" style="width:60px;height:110px;background:#7A4A22;transform-origin:right;transition:transform 1.2s cubic-bezier(.2,.8,.2,1);border:2px solid #C99A3B"></div></div>';
    else body='<div class="anim'+(m.trigger==="loop"?" loop":"")+'" data-anim="'+m.trigger+'">'+ANIMSVG+'<small>'+esc(motionSummary(m))+'</small></div>';}
  else body='<div class="emrow">'+(m.style==="shine"?'<span class="shine" data-em>'+esc(L(UI.read))+' &#8250;</span>':m.style==="pulse"?'<span class="pulse" data-em>'+(TXH("locate_label"))+'</span>':'<span class="floatpin" data-em>'+PIN+'</span>')+'</div>';
  return '<div class="item" data-ref="m:'+m.id+'"><div class="mdemo">'+head+body+'</div></div>';}
function interDemo(x){var b="",o=x.o;
  if(x.id==="flip")b=renderEl("callout",{o:{tone:"family",calltap:"flip"}},{},0,false);
  if(x.id==="hold")b=renderEl("picture",{o:{pictap:"hold"}},{img:"before"},0,false);
  if(x.id==="compare")b=renderEl("before_after",{o:{start:String(o.start)}},{},0,false);
  if(x.id==="deck")b='<div class="deck" data-deck>'+["g3","g2","g1"].map(function(g,i){return'<div class="cd" style="transform:translateY('+(i*-6)+'px) scale('+(1-(2-i)*.04)+');z-index:'+i+'"><img alt="" src="'+IMG[g]+'"></div>'}).join("")+'</div>';
  if(x.id==="scrub")b=renderEl("scrolly",{o:{}},{},0,false);
  if(x.id==="tiltlook")b=renderEl("picture",{o:{pictap:"tilt"}},{img:"hero"},0,false);
  if(x.id==="choice")b='<div class="choice" data-choice><div class="opts"><button type="button" data-c="0" aria-pressed="true">'+(S.lang==="pt"?"Seguir o rei":"Follow the king")+'</button><button type="button" data-c="1" aria-pressed="false">'+(S.lang==="pt"?"Seguir a biblioteca":"Follow the library")+'</button></div><p>'+esc(L(T("Manuel I moved the court to the river around 1500.","D. Manuel I mudou a corte para o rio por volta de 1500.")))+'</p></div>';
  if(x.id==="hotspots")b=renderEl("hotspot",{o:{pulse:"pulse"}},{},0,false);
  return '<div class="item" data-ref="i:'+x.id+'"><div class="mdemo"><div class="mhead"><div><b>'+esc(x.name)+'</b><span class="mpath">'+(x.id==="scrub"?"BUILD":"NATIVE")+'</span><small>'+esc(x.h)+'</small></div></div>'+b+'</div></div>';}
function effectDemo(f){return '<div class="item" data-ref="f:'+f.id+'"><div class="fx"><div class="cap">'+esc(f.name)+' <small>· '+({edge:"edge",line:"line divider",light:"light",moving:"moving"})[f.grp]+'</small></div>'+effectArt(f)+'</div></div>'}
function motionOpts(type,withFamily){var a=[];if(withFamily)a.push(["family","Family's ("+((motionById(S.family)||{}).name||"(missing)")+")"]);S.motions.forEach(function(m){if(m.type===type)a.push([m.id,m.name])});a.push(["none","None"]);return a}
function matchQ(name){var q=S.ui.q.trim().toLowerCase();return !q||name.toLowerCase().indexOf(q)>=0}
function matches(name,cat){var q=S.ui.q.trim().toLowerCase();return (S.ui.cat==="All"||cat===S.ui.cat)&&(!q||name.toLowerCase().indexOf(q)>=0)}
function advice(m){if(m.type==="entrance"&&m.dur>600)return "Long for an entrance: the visitor waits "+m.dur+" ms to read. Fine for one hero moment.";if(m.type==="transition"&&m.dur>900)return "A slow turn: good for a story mode, tiring for a quick swipe.";if(m.type==="entrance"&&m.dist>80)return "A long travel: a dramatic entrance.";return ""}
function motionBody(m){var ans=firstAnswer("motionBody.answer",m);if(ans!==undefined)return ans;var b=row("Name",'<input class="u" data-text="motion" value="'+esc(m.name)+'">',"The name styles and blocks pick it by.");
  if(m.type==="entrance"){b+=row("Comes in",seg("motion.from",["below","left","right","scale","wipe","none"],["Below","Left","Right","Scale","Wipe","In place"],m.from),"Where it enters from.",true)+
   row("Distance (pt)",slider("motion.dist",0,200,1,m.dist),"How far it travels.",true)+row("Tilt (°)",slider("motion.rot",-20,20,1,m.rot||0),"A rotation that settles to straight.",true)+
   row("Duration (ms)",slider("motion.dur",100,2000,10,m.dur,advice(m)),"Advice, not a cap: 250-500 ms reads best.",true)+row("Easing",seg("motion.ease",["out","overshoot","spring","inout"],["Ease Out","Overshoot","Spring","In-Out"],m.ease),"Overshoot and Spring give the little jump.",true)+
   row("Pieces",seg("motion.pieces",["whole","words","letters"],["Whole","Word by word","Letter by letter"],m.pieces),"Words and letters build a line of text.",true)+row("Stagger (ms)",slider("motion.stagger",0,300,1,m.stagger),"The delay between pieces or rows.",true)}
  else if(m.type==="transition")b+=row("Turn",seg("motion.style",["slide","cube","deck","flip","fade","zoom"],["Swipe","Cube","Card Deck","Flip","Cross-fade","Zoom"],m.style),"How one item gives way to the next.",true)+row("Duration (ms)",slider("motion.dur",150,2000,10,m.dur,advice(m)),"Advice: 400-700 ms.",true)+row("Easing",seg("motion.ease",["out","inout","spring"],["Ease Out","In-Out","Spring"],m.ease),"",true);
  else if(m.type==="scroll")b+=row("Strength",slider("motion.factor",0,0.5,0.01,m.factor),"How much the picture moves or grows.",true);
  else if(m.type==="imported")b+=row("Source",seg("motion.source",["lottie","rive","video","unity"],["Lottie","Rive","Video + alpha","Unity Animation"],m.source),"Made in another tool (or a Unity prefab), imported as an asset.",true)+row("File",'<input class="u" data-text="motionfile" value="'+esc(m.file)+'">',"The asset in the wall's folder.",true)+row("Plays",seg("motion.trigger",["enter","loop","tap","scrub"],["On view","Loop","On tap","With scroll"],m.trigger),"With scroll scrubs it with the visitor's scroll.",true);
  else b+=row("Period (ms)",slider("motion.dur",600,6000,50,m.dur),"One loop; loops stop under Reduce Motion.",true);
  return b;}
function edCardV3(){var h='';
  /* Editor view: how foldouts behave + find */
  h+='<div class="fo" style="padding:6px 8px"><div class="row" style="grid-template-columns:150px minmax(0,1fr) 22px"><label>Foldouts</label><div>'+seg("ui.single",["false","true"],["Many open","One at a time"],String(S.ui.single))+'</div>'+help("Editor preference (not saved to the wall): one open at a time closes the others at the same level.")+'</div>'+
   row("Open to level",seg("ui.level",["0","1","2","3"],["Closed","1","2","3"],String(S.ui.level)),"How deep the foldouts open by default.")+
   '<div class="filters"><input class="u" data-q="1" placeholder="Find a part..." value="'+esc(S.ui.q)+'">'+["All"].concat(CATS).map(function(c){return'<button class="chip" type="button" data-cat="'+c+'" aria-pressed="'+(S.ui.cat===c)+'">'+c+'</button>'}).join("")+'</div></div>';
  h+=locked("Card Components")+locked("Sections")+locked("Navigation Bar")+locked("Locate Button");
  var b="";
  b+=fo("test","Test",["test","TEST"],row("Show the components demo",seg("test.on",["true","false"],["On","Off"],String(S.test.on)),"Adds a test block at the top of the chosen marker's About, in Scene and Play only. Never saved to the marker.")+
     row("On marker",sel("test.poi",POIS,S.test.poi),"Which marker's card shows the demo. The Ribeira Palace is the reference card.",true)+
     row("Focus on what I edit",seg("focusOn",["true","false"],["On","Off"],String(S.focusOn)),"The demo jumps to the part you edit, marks it EDITING and dims the rest.",true)+
     '<div class="note">Theme, Reduce Motion, language and screen are on the phone\'s floating Preview bar (Play Mode).</div>'+
     fo("how1","How to Scene Test","",'<div class="tip">LivingRoomScene, the marker above, its card preview: the orange tile is first in About. Edit any part: the demo jumps to it.</div>',9)+
     fo("how2","How to Playmode Test","",'<div class="tip">Play, tap the marker, open the card, tap the orange tile. Use the floating Preview bar: Light / Dark, Reduce, EN / PT, Focus, screen.</div>',9),2);
  /* Motion */
  var mb=row("Motion Family",sel("family",motionOpts("entrance",false),S.family),"The card's default entrance.")+
   row("Replay",seg("replay",["every","once"],["Every Pass","Once"],S.replay),"Every Pass: scrolling down plays an entrance, scrolling back up undoes it, down again replays. Once: it stays. One choice for the whole card (Reveal follows it).")+
   '<div class="note fixed">One hard rule: Reduce Motion shows everything whole at once and stops loops. Everything else is yours; warnings are advice.</div>';
  Object.keys(TYPES).forEach(function(t){var inner="";S.motions.filter(function(m){return m.type===t&&matchQ(m.name)}).forEach(function(m){var u=motionUses(m.id);
     inner+=fo("mo:"+m.id,esc(m.name)+' <span class="sub">· '+esc(motionSummary(m))+' · used '+u+'</span>',[PATH[m.id]==="imp"?"bro":PATH[m.id]==="bld"?"bld":"nat",({nat:"NATIVE",bld:"BUILD",imp:"IMPORT"})[PATH[m.id]||"bld"]],motionBody(m)+'<div class="btnrow"><button class="del" type="button" data-del="motion" data-id="'+m.id+'" '+(u?"disabled title=\"In use\"":"")+'>Delete</button></div>',9,S.sel.layer==="motion"&&S.sel.motion===m.id)});
    inner+='<div class="btnrow"><button class="ubtn sm" type="button" data-add="motion" data-type="'+t+'">+ New '+TYPES[t].toLowerCase()+' preset</button></div>';
    mb+=fo("mt:"+t,TYPES[t]+' <span class="sub">· '+S.motions.filter(function(m){return m.type===t}).length+'</span>',null,inner,3)});
  mb+='<div class="tip"><b>Making a motion.</b> + New makes a named preset of a type: that covers most needs. Imported animation brings motion made elsewhere (Rive, Lottie, a video with alpha, or a Unity prefab\'s Animation) with a trigger. A new kind of movement in code is a motion type registered like an element kind.</div>';
  b+=fo("motion","Motion",["new","NEW"],mb,2);
  /* Interaction */
  var ib='<div class="note">Things the visitor does to a part. Elements and patterns pick them (a Callout\'s Tap to Flip, a Picture\'s Hold to Reveal).</div>';
  S.inter.forEach(function(x){if(!matchQ(x.name))return;var o=x.o,bb='<div class="note">'+esc(x.h)+'</div>';
    if(x.id==="flip")bb+=row("Turn (ms)",slider("inter.dur",200,1500,10,o.dur),"",true);if(x.id==="hold")bb+=row("Hold (ms)",slider("inter.hold",300,3000,50,o.hold),"How long the press must last.",true);
    if(x.id==="compare")bb+=row("Start at (%)",slider("inter.start",0,100,1,o.start),"",true);if(x.id==="deck")bb+=row("Throw at (pt)",slider("inter.throwd",40,240,5,o.throwd),"How far a drag must go to throw a card.",true);
    if(x.id==="tiltlook")bb+=row("Strength (pt)",slider("inter.strength",0,40,1,o.strength),"",true);
    ib+=fo("in:"+x.id,esc(x.name),[x.id==="scrub"?"bld":"nat",x.id==="scrub"?"BUILD":"NATIVE"],bb,9,S.sel.layer==="inter"&&S.sel.inter===x.id)});
  b+=fo("interaction","Interaction",["new","NEW"],ib,2);
  /* Effects */
  var eb='<div class="note">The Look Kit. Edges shape the start of a section or a divider; line dividers sit between parts. A Divider element picks one of these.</div>';
  [["edge","Edges"],["line","Line dividers"],["light","Light"],["moving","Moving"]].forEach(function(gp){var inner="";S.effects.filter(function(f){return f.grp===gp[0]&&matchQ(f.name)}).forEach(function(f){var o=f.o,fb='<div class="thumb cv'+(S.dark?" dark":"")+'" style="height:auto;padding:4px 0">'+(f.grp==="edge"?edgeSvg(f):f.grp==="line"?lineSvg(f):"")+'</div>';
     if(o.h!=null&&f.grp==="edge")fb+=row("Height (pt)",slider("fx.h",12,80,1,o.h),"",true);
     if(o.colour)fb+=row("Colour",seg("fx.colour",["family","accent","neutral"],["Family colour","Accent","Neutral"],o.colour),"",true);
     if(f.id==="x_fade")fb+=row("Height (pt)",slider("fx.h",20,160,1,o.h),"",true);if(f.id==="x_tone")fb+=row("Spread",slider("fx.spread",0.4,1.5,0.05,o.spread),"Its strength stays automatic.",true);if(f.id==="x_drift")fb+=row("Speed (pt/s)",slider("fx.speed",2,40,1,o.speed),"",true);
     if(f.id==="x_reveal")fb+='<div class="note">Replay follows Motion > Replay.</div>';
     inner+=fo("fx:"+f.id,esc(f.name)+' <span class="sub">· used '+effectUses(f.id)+'</span>',["nat","NATIVE"],fb,9,S.sel.layer==="effect"&&S.sel.effect===f.id)});
    eb+=fo("fg:"+gp[0],gp[1],null,inner,3)});
  eb+='<div class="btnrow"><button class="ubtn sm" type="button" disabled title="Lab: an SVG asset of the wall">+ Import an edge (SVG)</button></div>';
  b+=fo("effects","Effects",["ext","EXTENDED"],eb,2);
  /* Elements */
  var elb='<div class="note">Each element opens to its styles, rendered. ★ = the default a new row gets.</div>';
  CATS.forEach(function(cat){var inner="";KINDS.filter(function(K){return K.c===cat&&matches(K.n,cat)}).forEach(function(K){var sts=S.styles[K.k]||[],d=defStyle(K.k);
    var cards='<div class="thumbs">'+sts.map(function(st){return'<button class="sc" type="button" data-pickstyle="'+st.id+'" aria-pressed="'+(S.sel.style===st.id)+'">'+thumb(K.k,st)+'<span class="nm">'+(st.def?"<i>&#9733;</i>":"")+'<span>'+esc(st.name)+'</span><em>'+styleUses(st.id)+'</em></span></button>'}).join("")+'</div>';
    var st=styleById(S.sel.style),sb="";
    if(st&&kindOfStyle(st.id)===K.k){sb+=row("Name",'<input class="u" data-text="style" value="'+esc(st.name)+'">',"The name rows and patterns pick.");
      (OPTS[K.k]||[]).forEach(function(k){var dd=OPTDEF[k];if(k==="uses"){sb+=row(dd.l,sel("style.uses",S.effects.filter(function(f){return f.grp==="edge"||f.grp==="line"}).map(function(f){return[f.id,f.name+" ("+(f.grp==="edge"?"edge":"line")+")"]}),st.o.uses),dd.h,true);return}sb+=row(dd.l,seg("o."+k,dd.v,dd.vl,st.o[k]),dd.h,true)});
      sb+=row("Entrance",sel("style.enter",motionOpts("entrance",true),st.enter),"How it comes into view.",true);
      if(K.k==="carousel"&&st.o.layout==="one")sb+=row("Transition",sel("style.turn",motionOpts("transition",false).filter(function(x){return x[0]!=="none"}),st.turn||"t_slide"),"How one picture gives way to the next.",true);
      if(MEDIA[K.k]&&K.k!=="carousel")sb+=row("With the scroll",sel("style.scroll",motionOpts("scroll",false),st.scroll||"none"),"Parallax, a slow zoom or growing in.",true);
      var u2=styleUses(st.id);sb+='<div class="btnrow"><button class="ubtn sm" type="button" data-def="'+st.id+'"'+(st.def?" disabled":"")+'>&#9733; Make default</button><button class="ubtn sm" type="button" data-add="style">+ Add style (copy)</button><button class="del" type="button" data-del="style" data-id="'+st.id+'" '+(u2||st.def?"disabled":"")+'>Delete</button></div>';}
    inner+=fo("el:"+K.k,'<span class="foth cv'+(S.dark?" dark":"")+'"><span class="tin" style="display:block">'+renderEl(K.k,d,{},0,true)+'</span></span><span class="kicon'+(cat==="Media"?" m":cat==="Structure"?" s":cat==="Interactive"?" p":"")+'">'+K.ab+'</span>'+esc(K.n)+' <span class="sub">· '+sts.length+' style'+(sts.length>1?"s":"")+'</span>',null,cards+sb,9,S.sel.layer==="style"&&S.sel.kind===K.k)});
    if(inner)elb+=fo("eg:"+cat,cat,null,inner,3)});
  b+=fo("elements","Elements",["new","NEW"],elb,2);
  /* Patterns */
  var pb='<div class="note">A pattern owns its rows\' styles; a marker only fills its content.</div>';
  S.patterns.forEach(function(p){if(!matches(p.name,p.cat))return;var u=patternUses(p.id),body=patThumb(p)+row("Name",'<input class="u" data-text="pattern" data-pid="'+p.id+'" value="'+esc(p.name)+'">',"The name a marker picks.");
    body+='<table class="tbl"><thead><tr><th style="width:22px">#</th><th style="width:34%">Element</th><th>Style</th><th style="width:80px"></th></tr></thead><tbody>'+p.rows.map(function(r,i){var opts=(S.styles[r.kind]||[]).map(function(s){return[s.id,s.name+(s.def?" ★":"")]});if(r.style&&!styleById(r.style))opts.unshift([r.style,"(missing)"]);
      return'<tr><td class="uses">'+(i+1)+'</td><td><span class="kicon">'+KIND[r.kind].ab+'</span>'+KIND[r.kind].n+'</td><td>'+sel("prow."+p.id+"."+i,opts,r.style)+'</td><td><button class="del" type="button" data-pmove="'+p.id+'.'+i+'" data-dir="-1">^</button> <button class="del" type="button" data-pmove="'+p.id+'.'+i+'" data-dir="1">v</button> <button class="del" type="button" data-pdel="'+p.id+'.'+i+'">&#128465;</button></td></tr>'}).join("")+'</tbody></table>'+
     '<div class="note">+ Add element</div><div class="btnrow">'+KINDS.map(function(K){return'<button class="ubtn sm" type="button" data-padd="'+p.id+'.'+K.k+'">'+K.n+'</button>'}).join("")+'</div><div class="btnrow"><button class="del" type="button" data-del="pattern" data-id="'+p.id+'" '+(u?"disabled":"")+'>Delete pattern</button></div>';
    pb+=fo("pt:"+p.id,'<span class="kicon p">Pt</span>'+esc(p.name)+' <span class="sub">· '+p.cat+' · '+p.rows.length+' elements · used '+u+'</span>',null,body,9,S.sel.layer==="pattern"&&S.sel.pattern===p.id)});
  pb+='<div class="btnrow"><button class="ubtn sm" type="button" data-add="pattern">+ Add pattern</button></div>';
  b+=fo("patterns","Patterns",["new","NEW"],pb,2);
  h+=fo("bc","Block Components",["new","NEW"],b,1);
  /* Block Library */
  var lb='<div class="note">A block earns its place with behaviour, data or an owner; arrangements of words and pictures are patterns, placed by the Inline block. '+esc(GONE)+'</div><div class="bgrid">';
  LIB.forEach(function(r){if(!matchQ(r[1]))return;lb+='<button class="sc bcard" type="button" data-block="'+esc(r[1])+'" aria-pressed="'+(S.sel.block===r[1])+'">'+libThumb(r[1])+'<span class="nm">'+esc(r[1])+(r[2]==="new"?' <span class="badge new">NEW</span>':'')+'</span><span class="from">'+esc(r[0])+' · built from '+esc(r[4])+'</span></button>'});
  lb+='</div>';var cur=LIB.filter(function(r){return r[1]===S.sel.block})[0];if(cur)lb+='<div class="tip"><b>'+esc(cur[1])+'</b> · '+esc(cur[3])+' Its own options (Block Library defaults) and its preview / page settings live here; a marker places it and fills its content.</div>';
  h+=fo("library","Block Library",["ext","EXTENDED"],lb,1);
  h+=jsonBox("config.json · card_settings.block_components",bcJson());return h;}
function libThumb(name){var ans=firstAnswer("libThumb.answer",name);if(ans!==undefined)return ans;var k={"Header":["picture","st17"],"Inline":["callout","st28"],"Story":["picture","st16"],"Quick Facts":["swatches","st42"],"Timeline":["timeline","st41"],"Gallery":["carousel","st20"],"Before and After":["before_after","st35"],"Hotspot Image":["hotspot","st36"],"Video":["video","st21"],"3D Model":["model_3d","st37"],"360 Panorama":["panorama","st38"],"Audio Guide":["audio","st32"],"Wall Locator":["map","st40"],"Today Map":["map","st40"],"Compare Points":["picture_pair","st19"],"Related":["picture_pair","st19"],"Sources":["list","st14"],"Contents":["list","st15"],"Practical Info":["list","st14"]}[name]||["callout","st11"];
  return thumb(k[0],styleById(k[1]))}
function mv2(kind,i){return'<button class="del" type="button" data-'+kind+'="'+i+'" data-dir="-1">^</button> <button class="del" type="button" data-'+kind+'="'+i+'" data-dir="1">v</button> <button class="del" type="button" data-'+kind+'="'+i+'" data-dir="0">&#128465;</button>'}
function cardThumb(b){var h=noBtn(blockPreview(b,0)).replace(/class="mv pre[^"]*"/g,'class=""').replace(/data-go="[^"]*"/g,"");return '<div class="thumb sm cv'+(S.dark?" dark":"")+'" style="width:72px;height:44px"><div class="tin" style="transform:scale(.21)">'+h+'</div></div>'}
function fieldsFor(kind,c,path){var f=FIELDS[kind]||[],h='<div class="fields">';if(!f.length)return '<span class="uses">No content: the element draws from its style (and the wall\'s data).</span>';
  f.forEach(function(x){var key=x[0],type=x[1],lab=x[2],v=c[key];
   if(type==="ltext"){v=v||{en:"",pt:""};langCodes().forEach(function(lg){h+=row(lab+" · "+lg.toUpperCase(),'<textarea class="u" rows="2" data-field="'+path+'|'+key+'|'+lg+'">'+esc(v[lg]||"")+'</textarea>',lg==="en"?"One per language; empty in PT falls back to EN.":"")})}
   else if(type==="litems"){v=v||{en:[],pt:[]};langCodes().forEach(function(lg){h+=row(lab+" · "+lg.toUpperCase(),'<textarea class="u" rows="3" data-field="'+path+'|'+key+'|'+lg+'|lines">'+esc((v[lg]||[]).join("\n"))+'</textarea>',"")})}
   else if(type==="text")h+=row(lab,'<input class="u" data-field="'+path+'|'+key+'" value="'+esc(v||"")+'">',"");
   else if(type==="img")h+=row(lab,'<div class="assets">'+ASSETS.map(function(a){return'<button type="button" data-asset="'+path+'|'+key+'|'+a+'" aria-pressed="'+(v===a)+'" style="background-image:url('+IMG[a]+')" aria-label="'+a+'"></button>'}).join("")+'</div>',"A picture from the wall's media folder.");
   else if(type==="imgs"){v=v||[];h+=row(lab,'<div class="assets">'+ASSETS.map(function(a){return'<button type="button" data-assets="'+path+'|'+key+'|'+a+'" aria-pressed="'+(v.indexOf(a)>=0)+'" style="background-image:url('+IMG[a]+')" aria-label="'+a+'"></button>'}).join("")+'</div>',"Tap to add or remove; the order is the tap order.")}});
  return h+'</div>';}
/* ----- each row's demo (Blocks & Components, Motion & Effects): the item it edits on the demo screen (the foldout it sits in: the
   motion, the effect, the interaction, the element's style), brought into view and ringed; a motion plays once; an element's Entrance
   plays it in; Motion Family and Replay play the family's entrance ----- */
function compRef(kind){var s=S.sel;return kind==="motion"?"m:"+s.motion:kind==="fx"?"f:"+s.effect:kind==="inter"?"i:"+s.inter:kind==="family"?"m:"+S.family:"s:"+s.style}
function compDemo(kind){var sel=function(){return'[data-ref="'+compRef(kind)+'"]'};
  return{view:"demo",wait:0,scroll:sel,ring:sel,act:function(){S.focusRef=compRef(kind);applyFocus();var r=compRef(kind);
    if(kind==="motion"||kind==="family")playRef(r);else if(kind==="enter")replayRef(r)},ringAfter:60}}
rowDemos(function(key){var k=key.split(".")[0];
  if(k==="motion"||k==="fx"||k==="inter")return compDemo(k);if(k==="o")return compDemo("style");
  if(k==="style")return compDemo(key==="style.enter"?"enter":"style");if(key==="family"||key==="replay")return compDemo("family")});
