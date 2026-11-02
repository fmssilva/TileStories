/* ===================== v8c: elements (Unity's per-row options as style options, merged), motion "By itself", the effects pool, the text patterns ===================== */
/* ----- element options: the merged lists ----- */
Object.assign(OPTDEF.tone,{v:["neutral","family","accent","inverse"],vl:["Neutral","Section colour","Accent","Inverse"],h:"Section colour = the colour of the section the block sits in. Inverse: light words on a dark plate. Every tone keeps 7:1 (a fixed rule)."});
Object.assign(OPTDEF.marker,{v:["bullets","numbers","steps"],vl:["Bullets","Numbers","Steps"],h:"Numbers when the order is information. Steps: circles joined by a line, each item a title and an optional text (from Process Steps)."});
Object.assign(OPTDEF.pictap,{v:["viewer","none","hold","tilt"],vl:["Open the viewer","None","Hold to Reveal","Tilt to Look"],h:"Open the viewer: a tap opens the picture full screen (Card Controls › Picture viewer). Hold to Reveal hides it until a press; Tilt to Look moves it with the phone."});
Object.assign(OPTDEF.layout,{v:["peek","one","grid","film","stack"],vl:["Peek strip","One at a time","Grid","Filmstrip","Stack"],h:"Peek strip: a swipe strip (Card › Look › Swipe strips sets the edge). One at a time: a full frame that turns with the Transition. Grid: 2 x 2 and \"+N\". Filmstrip: one large picture over a row of thumbnails. Stack: pictures one under another, the count below."});
Object.assign(OPTDEF.gyro,{l:"Look around by",v:["off","on"],vl:["Dragging","Turning the phone"],h:"Turning the phone falls back to dragging when the phone has no gyroscope. In a preview, only a horizontal drag pans."});
Object.assign(OPTDEF.space,{h:"16 / 32 / 48 pt, steps of the theme's spacing scale."});
Object.assign(OPTDEF,{
 cshape:{l:"Shape",v:["box","postcard"],vl:["Box","Postcard"],h:"Postcard: a slightly tilted card with a white border (from Fun Fact's Postcard look); a named asymmetry (D7)."},
 noimg:{l:"With no picture",v:["hide","initial"],vl:["Hide","Show the next heading's initial"],h:"When a row has no picture yet: leave it out, or show the next heading's first letter in an arch (from Person)."},
 ratio:{l:"Proportion",v:["auto","3:2","4:3","1:1"],vl:["As the picture","3:2","4:3","1:1"],h:"As the picture: the frame takes the picture's own proportion, from 4:5 to 2:1.",p:1},
 pptap:{l:"Interaction",v:["viewer","none"],vl:["Open the viewer","None"],h:"A tap opens the viewer at the picture tapped."},
 marks:{l:"Position marks",v:["dots","count","none"],vl:["Dots","Counter","None"],h:"Under the strip or frame: dots, \"2 of 4\", or nothing."},
 vtap:{l:"On tap",v:["here","player"],vl:["Play here","Open the player"],h:"Play here: it plays in place, with a full-screen button on the same playback. Open the player: the Video player page."},
 speed:{l:"Speed button",v:["narration","wide","off"],vl:["Narration","Wide","Off"],h:"The speeds the visitor's button steps through: Narration 0.75-1.5x, Wide 0.5-2x. Off hides it."},
 acap:{l:"Captions",v:["on","off"],vl:["On at start","Off at start"],h:"The captions line under the player; the visitor can switch it."},
 fit:{l:"Fit",v:["yaw","sphere","rest","visible"],vl:["Yaw safe","Sphere","At rest","Visible"],h:"How large the model is drawn. Yaw safe: it stays whole as it turns. Visible: a scan is sized to what is seen."},
 code:{l:"Show colour code",v:["off","on"],vl:["Off","On"],h:"The #RRGGBB under each name. A swatch's colour is content, so it is not bound by the no-free-colour rule; its words keep 7:1."},
 swlayout:{l:"Layout",v:["row","grid"],vl:["Row","Grid"],h:"Grid: two columns, with each swatch's note."}});
OPTS.heading=["tone","size","align","rule"];OPTS.pull_quote=["variant","tone"];OPTS.callout=["tone","size","cshape","calltap"];OPTS.picture=["width","frame","ratio","pictap","noimg"];
OPTS.picture_pair=["width","frame","pptap"];OPTS.carousel=["layout","marks","width","frame"];OPTS.video=["width","frame","vtap"];OPTS.audio=["alook","speed","acap"];
OPTS.model_3d=["fit","spin"];OPTS.map=[];OPTS.swatches=["swsize","swlayout","code"];
var ODEF={tone:"neutral",align:"start",cshape:"box",ratio:"auto",pictap:"viewer",noimg:"hide",frame:"rect",pptap:"viewer",marks:"dots",width:"inset",vtap:"here",speed:"narration",acap:"on",fit:"yaw",code:"off",swlayout:"row"};
Object.keys(S.styles).forEach(function(k){(S.styles[k]||[]).forEach(function(st){(OPTS[k]||[]).forEach(function(o){if(st.o[o]==null)st.o[o]=ODEF[o]!=null?ODEF[o]:OPTDEF[o].v[0]});if(st.o.pictap==="none"&&k==="picture"&&st.id==="st16")st.o.pictap="viewer"})});
(S.styles.carousel||[]).forEach(function(st){st.o.width="bleed"});(S.styles.pull_quote||[]).forEach(function(st){st.o.tone="family"});
S.styles.title[0].o.size="l";S.styles.lede[0].o.size="l";(S.styles.panorama||[]).forEach(function(st){st.o.gyro="off"});
S.styles.callout.push({id:"st46",name:"Postcard",o:{tone:"family",size:"m",cshape:"postcard",calltap:"none"},enter:"tilt",proposed:1});
S.styles.list.push({id:"st47",name:"Steps",o:{marker:"steps"},enter:"family"});
S.styles.carousel.push({id:"st48",name:"Grid",o:{layout:"grid",marks:"none",width:"inset",frame:"rect"},enter:"fade"},{id:"st49",name:"Filmstrip",o:{layout:"film",marks:"count",width:"inset",frame:"rect"},enter:"fade"});
var ptSteps=patternById("pt5");if(ptSteps)ptSteps.rows[1].style="st47";
S.patterns.push({id:"pt9",name:"Text",cat:"Text",rows:[{kind:"paragraph",style:"st8"}]},{id:"pt10",name:"Text with headings",cat:"Text",rows:[{kind:"heading",style:"st5"},{kind:"paragraph",style:"st8"}]});
/* ----- drawing the new options ----- */
function stepsList(items){return '<ol class="el-steps">'+items.map(function(x,i){var p=String(x).split(": ");return'<li><i>'+(i+1)+'</i><div><b>'+esc(p[0])+'</b>'+(p[1]?'<span>'+esc(p[1])+'</span>':"")+'</div></li>'}).join("")+'</ol>'}
function carouselStill(st,c){var o=st.o,ci=c.imgs||["g1","g2","g3","before"],arch=o.frame==="arch"?" arch":"",h;
  var marks=o.marks==="dots"?'<div class="cmarks">'+ci.map(function(x,i){return'<i class="'+(i===0?"on":"")+'"></i>'}).join("")+'</div>':o.marks==="count"?'<div class="cmarks n">'+esc(fmt(TXT("viewer_position"),1,ci.length))+'</div>':"";
  if(o.layout==="grid")h='<div class="cgrid'+arch+'">'+ci.slice(0,4).map(function(x,i){return'<div class="fr"><img alt="" src="'+IMG[x]+'">'+(i===3&&ci.length>4?'<em>+'+(ci.length-3)+'</em>':"")+'</div>'}).join("")+'</div>';
  else if(o.layout==="film")h='<div class="cfilm'+arch+'"><div class="fr big"><img alt="" src="'+IMG[ci[0]]+'"></div><div class="thumbsrow">'+ci.map(function(x,i){return'<span class="'+(i===0?"on":"")+'" style="background-image:url('+IMG[x]+')"></span>'}).join("")+'</div></div>';
  else if(o.layout==="stack")h='<div class="cstack'+arch+'">'+ci.slice(0,2).map(function(x){return'<div class="fr"><img alt="" src="'+IMG[x]+'"></div>'}).join("")+'<div class="fmt">'+esc(fmt(TXT("gallery_count"),ci.length))+'</div></div>';
  else if(o.layout==="one")h='<div class="cone'+arch+'"><div class="fr"><img alt="" src="'+IMG[ci[0]]+'"></div><span class="carr l">&#8249;</span><span class="carr r">&#8250;</span></div>';
  else h='<div class="el-car'+(o.width==="inset"?" inset":"")+'">'+ci.map(function(x){return'<div class="fr'+arch+'"><img alt="" src="'+IMG[x]+'"></div>'}).join("")+'</div>';
  return h+marks}
var renderElV7=renderEl;
renderEl=function(kind,style,c,i,still){var o=(style&&style.o)||{};c=c||{};
  if(kind==="carousel"&&(still||o.layout!=="one")){if(!Object.keys(c).length)c=S.demoC.carousel||{};var hc=carouselStill(style,c)+(c.cap?'<div class="el-pic" style="margin-top:6px"><figcaption>'+esc(L(c.cap))+'</figcaption></div>':"");return still?hc:wrapMv(hc,entranceOf(style),i)}
  if(kind==="list"&&o.marker==="steps"){if(!Object.keys(c).length)c=S.demoC.list||{};var it=(c.items&&(c.items[S.lang]||c.items.en))||DEMO.list[S.lang];var hl=stepsList(it);return still?hl:wrapMv(hl,entranceOf(style),i)}
  if(kind==="picture"&&c.img==="__none"){var hn=o.noimg==="initial"?'<figure class="el-pic arch noimg"><div class="fr"><span>'+esc(L(DEMO.heading).charAt(0))+'</span></div></figure>':"";return hn}
  var h=renderElV7(kind,style,c,i,still);
  if(kind==="heading"&&o.align==="centre")h=h.replace('class="el-heading','class="el-heading a-centre');
  if(kind==="pull_quote")h=h.replace('class="el-quote ','class="el-quote tq-'+(o.tone||"family")+' ');
  if(kind==="callout"&&o.cshape==="postcard")h=h.replace(/class="(el-callout|flip)/,'class="$1 postcard');
  if(kind==="callout"&&o.tone==="inverse")h=h.replace('class="el-callout inverse','class="el-callout inverse');
  if(kind==="picture"){if(o.ratio&&o.ratio!=="auto")h=h.replace('<div class="fr">','<div class="fr" style="aspect-ratio:'+o.ratio.replace(":","/")+'">');if(o.pictap==="viewer")h=h.replace('<div class="fr">','<div class="fr"><span class="vbadge" aria-hidden="true">&#10530;</span>')}
  if(kind==="picture_pair"){if(o.frame==="arch")h=h.replace('class="el-pair','class="el-pair arch');if(o.pptap==="viewer")h=h.replace('<div class="fr">','<div class="fr"><span class="vbadge" aria-hidden="true">&#10530;</span>')}
  if(kind==="video"){if(o.frame==="arch")h=h.replace('class="el-vid','class="el-vid arch');if(o.vtap==="player")h=h.replace("<i>&#9654;</i>",'<i>&#9654;</i><b class="vbadge" aria-hidden="true">&#10530;</b>')}
  if(kind==="audio"){if(o.speed!=="off")h=h.replace('<div class="bar">','<button type="button" class="aspeed" aria-label="'+TXH("audio_speed")+'">1x</button><div class="bar">');if(o.acap==="on")h=h.replace(/(<\/small>)(<\/div>)/,'$1<p class="acapline">'+esc(L(T("For about 250 years the kings lived by the river…","Durante cerca de 250 anos os reis viveram junto ao rio…")))+'</p>$2')}
  if(kind==="model_3d")h=h.replace('class="turn"','class="turn fit-'+(o.fit||"yaw")+'"');
  if(kind==="panorama"&&o.gyro==="on")h=h.replace(TXH("panorama_360_drag_hint"),TXH("panorama_360_gyro_hint"));
  if(kind==="swatches"){if(o.swlayout==="grid")h=h.replace('class="sw"','class="sw grid"');if(o.code==="on")h=h.replace(/<i style="background:(#[0-9A-Fa-f]{6});([^"]*)"><\/i><small>([^<]*)<\/small>/g,'<i style="background:$1;$2"></i><small>$3<code>$1</code></small>')}
  if(kind==="spacer")h=h.replace(/--h:\d+px/,"--h:"+({s:16,m:32,l:48}[o.space||"m"])+"px");
  return h};
/* the Lab's map look leaves: the map is an authored picture */
(S.styles.map||[]).forEach(function(st){delete st.o.mlook});
/* ----- motion: a new type, By itself (Unity's time-driven Slow Zoom and Pan) ----- */
TYPES.self="By itself";PATH.self_zoom="nat";PATH.self_pan="nat";
S.motions.splice(S.motions.findIndex(function(m){return m.type==="emphasis"}),0,{id:"self_zoom",name:"Slow Zoom",type:"self",style:"zoom",dur:28000},{id:"self_pan",name:"Pan",type:"self",style:"pan",dur:26000});
var mz=motionById("zoom");if(mz)mz.name="Zoom with the scroll";
(S.styles.picture||[]).forEach(function(st){if(st.self==null)st.self="none"});(S.styles.video||[]).forEach(function(st){if(st.self==null)st.self="none"});
var motionSummaryV7=motionSummary;motionSummary=function(m){if(m.type==="self")return "By itself · "+({zoom:"1 to 1.06 and back",pan:"across its room and back"})[m.style]+" · "+Math.round(m.dur/1000)+" s";return motionSummaryV7(m)};
var motionDemoV7=motionDemo;motionDemo=function(m){if(m.type!=="self")return motionDemoV7(m);
  return '<div class="item" data-ref="m:'+m.id+'"><div class="mdemo"><div class="mhead"><div><b>'+esc(m.name)+'</b>'+pathBadge(m.id)+'<small>'+esc(motionSummary(m))+'</small></div></div><figure class="el-pic"><div class="fr"><img alt="" src="'+IMG.hero+'" class="'+(S.reduce?"":"self-"+m.style)+'" style="animation-duration:'+(m.dur/1000)+'s"></div></figure></div></div>'};
var motionBodyV7=motionBody;motionBody=function(m){if(m.type!=="self")return motionBodyV7(m);return row("Name",'<input class="u" data-text="motion" value="'+esc(m.name)+'">',"")+row("Loop (s)",slider("motion.dur",8000,60000,1000,m.dur),"One way, then back. Slower than 26 s per drift, at most 12 pt/s; stopped under Reduce Motion.")};
var motionUsesV7=motionUses;motionUses=function(id){var n=motionUsesV7(id);Object.keys(S.styles).forEach(function(k){S.styles[k].forEach(function(st){if(st.self===id)n++})});return n};
ed.addEventListener("change",function(e){var t=e.target;if(t.dataset.sel!=="style.self")return;e.stopImmediatePropagation();var st=styleById(S.sel.style);if(!st)return;st.self=t.value;renderAll(true);show("demo","s:"+st.id)},true);
var renderElV8c=renderEl;renderEl=function(kind,style,c,i,still){var h=renderElV8c(kind,style,c,i,still);if(!still&&style&&style.self&&style.self!=="none"&&!S.reduce){var m=motionById(style.self);if(m)h=h.replace(/<img alt="" src="([^"]*)"/,'<img alt="" src="$1" class="self-'+m.style+'" style="animation-duration:'+(m.dur/1000)+'s"')}return h};
/* ----- effects: light and the own strip ----- */
S.effects.push({id:"e_own",grp:"edge",name:"Own strip picture",o:{h:24,colour:"family"}},{id:"x_glass",grp:"light",name:"Living glass",o:{}},{id:"x_rim",grp:"light",name:"Rim light",o:{}});
var edgeSvgV7=edgeSvg;edgeSvg=function(f){if(f.id!=="e_own")return edgeSvgV7(f);var h=f.o.h||24,p="";for(var x=0;x<390;x+=h)p+='<rect x="'+x+'" y="0" width="'+(h-2)+'" height="'+h+'" rx="3" fill="currentColor" opacity="'+((x/h)%2?.3:.75)+'"/>';return '<svg class="edge" viewBox="0 0 390 '+h+'" preserveAspectRatio="none" style="height:'+h+'px;color:'+colourVar(f.o.colour)+'">'+p+'</svg>'};
var effectArtV7=effectArt;effectArt=function(f){if(f.id==="x_glass")return '<div class="gsec glasslit" style="--fam:var(--media);height:90px;border-radius:14px;margin:8px 14px"></div>';if(f.id==="x_rim")return '<div style="padding:10px 14px"><div class="bprev rimlit" style="--fam:var(--stories);height:70px;margin:0"></div></div>';return effectArtV7(f)};
/* ----- the Editor: notes and rows ----- */
var edCardV8b=edCard;
edCard=function(){var d=document.createElement("div");d.innerHTML=edCardV8b();
  var en=innerOf(d,"s3e");if(en)en.insertAdjacentHTML("afterbegin",'<div class="note">An app may register its own element kinds; they appear here with their styles.</div>');
  d.querySelectorAll('[data-fo^="el:"] .inner').forEach(function(f){f.querySelectorAll(":scope > .row").forEach(function(r){var lb=r.querySelector("label");if(lb&&lb.textContent==="Proportion")lb.insertAdjacentHTML("beforeend",' <span class="prop">PROPOSED</span>')})});
  var st=styleById(S.sel.style);if(st&&(kindOfStyle(st.id)==="picture"||kindOfStyle(st.id)==="video")){var f=innerOf(d,"el:"+kindOfStyle(st.id)),ws=f&&Array.prototype.filter.call(f.querySelectorAll(":scope > .row"),function(r){var l=r.querySelector("label");return l&&l.textContent==="With the scroll"})[0];
    if(ws)ws.insertAdjacentHTML("afterend",row("By itself",sel("style.self",[["none","None"]].concat(S.motions.filter(function(m){return m.type==="self"}).map(function(m){return[m.id,m.name]})),st.self||"none"),"A slow move with no scroll (Unity's Slow Zoom and Pan). Only the picture nearest the middle moves by itself.",true))}
  addRows(d,"s4m",'<div class="note fixed">Fixed rules: the stagger stops growing after 5 rows; only the picture nearest the middle moves by itself; Reduce Motion makes every part whole and still.</div>');
  var bt=d.querySelector('button[disabled][title^="Lab: an SVG"]');if(bt)bt.textContent="+ Import an edge (SVG or a tileable PNG strip)";
  if(S.ctl.openrow==="shine"&&S.secCfg.look==="cards"&&S.secCfg.rim)addRows(d,"s5openrow",'<div class="note warn">Rim light is on (Sections › All sections): the card already has its one glow (D5). Shine adds a second.</div>');
  return d.innerHTML};
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(o\\.speed|o\\.pptap|"));
var modelSigV8c=modelSig;modelSig=function(){var x="";if(sigWant(/^o\.noimg/))(S.styles.picture||[]).forEach(function(st){x+=renderEl("picture",st,{img:"__none"},0,true)});return modelSigV8c()+x};
