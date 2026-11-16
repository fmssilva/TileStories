/* The elements: renderEl, their options merged with Unity's, the steps list and the carousel. */
function colourVar(c){return c==="accent"?"var(--accent2)":c==="neutral"?"var(--ink2)":"var(--fam)"}
/* ---------- render one element ---------- */
function C(c,k,f){return L(c&&c[k],f)}
/* an element: an own answer, or the plain one (its content first) through renderEl.html; every result through renderEl.final */
function renderEl(kind,style,c,i,still){var a={kind:kind,style:style,c:c,i:i,still:still},ans=firstAnswer("renderEl.answer",a);if(ans!==undefined)return pipe("renderEl.final",ans,a);
  c=pipe("renderEl.content",c,kind);c=c||{};var o=style.o||{},h="";
  var tone=o.tone?" t-"+o.tone:"",size=o.size&&o.size!=="m"?" s-"+o.size:"",al=o.align==="centre"?" a-centre":"";
  var scroll=!still&&style.scroll&&style.scroll!=="none"?' data-scroll="'+style.scroll+'"':"";
  switch(kind){
   case"title":h='<h3 class="el-title'+tone+size+al+'">'+esc(C(c,"text",DEMO.title))+'</h3>';break;
   case"eyebrow":h='<div class="el-eyebrow'+tone+al+'">'+esc(C(c,"text",DEMO.eyebrow))+'</div>';break;
   case"heading":h='<h4 class="el-heading'+tone+size+(o.rule==="on"?" rule":"")+'">'+esc(C(c,"text",DEMO.heading))+'</h4>';break;
   case"lede":h='<p class="el-lede'+size+al+'">'+esc(C(c,"text",DEMO.lede))+'</p>';break;
   case"paragraph":h='<p class="el-par'+(o.dropcap==="on"?" dropcap":"")+'">'+gloss(C(c,"text",DEMO.paragraph))+'</p>';break;
   case"callout":var ti=C(c,"title",DEMO.ctitle),tx=C(c,"text",DEMO.ctext);
     if(o.calltap==="flip"&&!still)h='<div class="flip" data-flip><div class="in2"><div class="fc"><b>'+esc(TXT("fun_fact_label"))+'</b><p>'+esc(tx)+'</p></div><div class="bc"><b>'+esc(ti)+'</b><p>'+esc(C(c,"back",DEMO.back))+'</p></div></div></div>';
     else h='<div class="el-callout '+(o.tone||"neutral")+size+'"><b class="'+tone+'">'+esc(ti)+'</b><p>'+esc(tx)+'</p></div>';break;
   case"pull_quote":h='<blockquote class="el-quote '+(o.variant||"serif")+'"><q>'+esc(C(c,"text",DEMO.quote))+'</q><cite>'+esc(c.cite||DEMO.cite)+'</cite></blockquote>';break;
   case"list":var tag=o.marker==="numbers"?"ol":"ul",items=(c.items&&c.items[S.lang])||DEMO.list[S.lang];h='<'+tag+' class="el-list">'+items.map(function(x){return"<li>"+esc(x)+"</li>"}).join("")+'</'+tag+'>';break;
   case"picture":var im=c.img||"g3",cap=C(c,"cap",DEMO.piccap);
     if(o.pictap==="hold"&&!still)h='<div class="hold cover" data-hold><img alt="" src="'+IMG[im]+'"><div class="ring"></div></div>';
     else if(o.pictap==="tilt"&&!still)h='<div class="tilt" data-tilt><img alt="" src="'+IMG[im]+'"></div>';
     else h='<figure class="el-pic'+(o.width==="bleed"?" bleed":"")+(o.frame==="arch"?" arch":"")+'"><div class="fr"><img alt="" src="'+IMG[im]+'"'+scroll+'></div><figcaption>'+esc(cap)+(c.credit?' · '+esc(c.credit):'')+'</figcaption></figure>';break;
   case"picture_pair":var pi=c.imgs||["before","g2"];h='<figure class="el-pair'+(o.width==="bleed"?" bleed":"")+'">'+pi.slice(0,2).map(function(x){return'<div class="fr"><img alt="" src="'+IMG[x]+'"></div>'}).join("")+'</figure><div class="el-pic" style="margin-top:8px"><figcaption>'+esc(C(c,"cap",DEMO.paircap))+'</figcaption></div>';break;
   case"carousel":var ci=c.imgs||["g1","g2","g3"];
     if(o.layout!=="one"||still)h='<div class="el-car">'+ci.map(function(x){return'<div class="fr"><img alt="" src="'+IMG[x]+'"></div>'}).join("")+'</div>';
     else{var tm=motionById(style.turn||"t_slide");h='<div class="trv'+(tm&&tm.style==="deck"?" deck":"")+'" data-trv="'+(tm?tm.id:"t_slide")+'" data-imgs="'+ci.join(",")+'"></div><div class="ctrl"><button type="button" data-turn="-1" aria-label="'+TXH("carousel_prev")+'">&#8249;</button><button type="button" data-turn="1" aria-label="'+TXH("carousel_next")+'">&#8250;</button></div>'}
     if(c.cap)h+='<div class="el-pic" style="margin-top:6px"><figcaption>'+esc(C(c,"cap"))+'</figcaption></div>';break;
   case"video":h='<div class="el-vid'+(o.width==="bleed"?" bleed":"")+'"><img alt="" src="'+IMG[c.img||"hero"]+'"'+scroll+'><i>&#9654;</i><span>'+esc(C(c,"cap",DEMO.vidcap))+'</span></div>';break;
   case"audio":h='<div class="audio" data-audio><button type="button" aria-label="'+TXH("audio_play")+'">&#9654;</button><div class="bar"><i></i></div><small>'+esc(C(c,"cap",T("Guided tour · 3:11","Visita guiada · 3:11")))+'</small></div>';break;
   case"animation":h='<div class="anim'+(o.trigger==="loop"&&!still?" loop":"")+(still?" play":"")+'" data-anim="'+(o.trigger||"enter")+'">'+ANIMSVG+'<small>'+(c.file?esc(c.file)+' · ':'')+esc(({lottie:"Lottie",rive:"Rive",video:"Video + alpha",unity:"Unity Animation"})[o.source||"lottie"])+' · '+esc(({enter:"plays on view",loop:"loops",tap:"tap to play",scrub:"scrubs with the scroll"})[o.trigger||"enter"])+'</small></div>';break;
   case"before_after":h='<div class="ba" data-ba style="--x:'+(o.start||50)+'%"><img alt="" src="'+IMG[c.after||"g2"]+'"><img class="top" alt="" src="'+IMG[c.before||"before"]+'"><span class="lb l">'+esc(L(c.blabel)||TXT("before_label"))+'</span><span class="lb r">'+esc(L(c.alabel)||TXT("after_label"))+'</span><div class="knob"></div></div>';break;
   case"hotspot":h='<div class="hs"><img alt="" src="'+IMG[c.img||"g3"]+'"><button class="pin" type="button" data-hspot="0" style="left:38%;top:42%;'+(o.pulse==="still"?"animation:none":"")+'">1</button><button class="pin" type="button" data-hspot="1" style="left:72%;top:58%;'+(o.pulse==="still"?"animation:none":"")+'">2</button><div class="pop"></div></div>';break;
   case"model_3d":h='<div class="turn" data-turnt style="background-image:url('+IMG[c.img||"g1"]+')"><span class="hint2">'+(TXH("model_3d_hint"))+'</span></div>';break;
   case"panorama":h='<div class="pano" data-pano style="background-image:url('+IMG[c.img||"hero"]+')"><span class="hint2">'+(TXH("panorama_360_drag_hint"))+'</span></div>';break;
   case"scrolly":h=still?'<figure class="el-pic"><div class="fr"><img alt="" src="'+IMG.g3+'"></div></figure>':'<div class="scrub" data-scrub><div class="stick"><img alt="" src="'+IMG.g3+'"><img alt="" src="'+IMG.before+'" style="opacity:0"><img alt="" src="'+IMG.g2+'" style="opacity:0"></div>'+
     [T("1662: the palace along the river.","1662: o palácio ao longo do rio."),T("Before 1755: the Torreão and the opera.","Antes de 1755: o Torreão e a ópera."),T("Today: the square that took its place.","Hoje: a praça que ficou no seu lugar.")].map(function(s,j){var it=c.items&&(c.items[S.lang]||c.items.en);return'<div class="step">'+esc(it&&it[j]!=null?it[j]:L(s))+'</div>'}).join("")+'<div style="height:60px"></div></div>';break;
   case"map":h='<div class="map"><svg viewBox="0 0 320 200"><path d="M0 150C60 130 120 165 180 150S290 120 320 135V200H0Z" fill="#9FC1D6" opacity=".7"/><text x="230" y="185" font-size="11" fill="#2E5870" font-family="sans-serif">Tagus</text><rect x="120" y="92" width="70" height="42" fill="none" stroke="var(--fam)" stroke-width="2"/><path d="M60 40L120 92M190 92L260 50M155 92V20" stroke="var(--ink2)" stroke-width="1.5" opacity=".5"/><g transform="translate(145 70)">'+PIN.replace("<svg",'<svg width="22" height="30"')+'</g></svg></div>';break;
   case"timeline":var ev=[["c. 1500",T("The court moves to the river","A corte muda-se para o rio")],["1580s",T("The Torreão rises","Ergue-se o Torreão")],["1755",T("Earthquake, fire and tsunami","Terramoto, incêndio e maremoto")]];if(c.items&&(c.items[S.lang]||c.items.en))ev=(c.items[S.lang]||c.items.en).map(function(s){var p=s.split(" · ");return[p[0],p.slice(1).join(" · ")]});
     h='<div class="tl">'+ev.map(function(e){return'<div><b>'+e[0]+'</b><span>'+esc(L(e[1]))+'</span></div>'}).join("")+'</div>';break;
   case"swatches":h='<div class="sw">'+[["#1F4E9C","Cobalt"],["#C99A3B","Ochre"],["#B4572E","Terracotta"],["#EFE9DC","Lime white"],["#4E7A6B","River green"]].map(function(s){return'<div><i style="background:'+s[0]+';'+(o.swsize==="s"?"height:28px":"")+'"></i><small>'+s[1]+'</small></div>'}).join("")+'</div>';break;
   case"divider":var f=effectById(o.uses||"l_hair");h=f?(f.grp==="edge"?'<div style="margin:16px -20px">'+edgeSvg(f)+'</div>':lineSvg(f)):'<div class="el-div missing"></div>';break;
   case"spacer":h='<div class="el-sp" style="--h:'+({s:16,m:24,l:40}[o.space||"m"])+'px"></div>';break;}
  return pipe("renderEl.final",pipe("renderEl.html",still?h:wrapMv(h,entranceOf(style),i),a),a);}
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
on("renderEl.answer",function(a){var kind=a.kind,style=a.style,c=a.c,i=a.i,still=a.still,o=(style&&style.o)||{};c=c||{};
  if(kind==="carousel"&&(still||o.layout!=="one")){if(!Object.keys(c).length)c=S.demoC.carousel||{};var hc=carouselStill(style,c)+(c.cap?'<div class="el-pic" style="margin-top:6px"><figcaption>'+esc(L(c.cap))+'</figcaption></div>':"");return still?hc:wrapMv(hc,entranceOf(style),i)}
  if(kind==="list"&&o.marker==="steps"){if(!Object.keys(c).length)c=S.demoC.list||{};var it=(c.items&&(c.items[S.lang]||c.items.en))||DEMO.list[S.lang];var hl=stepsList(it);return still?hl:wrapMv(hl,entranceOf(style),i)}
  if(kind==="picture"&&c.img==="__none"){var hn=o.noimg==="initial"?'<figure class="el-pic arch noimg"><div class="fr"><span>'+esc(L(DEMO.heading).charAt(0))+'</span></div></figure>':"";return hn}});
on("renderEl.html",function(h,a){var kind=a.kind,o=(a.style&&a.style.o)||{};
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
  return h});
/* the Lab's map look leaves: the map is an authored picture */
(S.styles.map||[]).forEach(function(st){delete st.o.mlook});
ed.addEventListener("change",function(e){var t=e.target;if(t.dataset.sel!=="style.self")return;e.stopImmediatePropagation();var st=styleById(S.sel.style);if(!st)return;st.self=t.value;renderAll(true);show("demo","s:"+st.id)},true);
/* ----- the Editor: notes and rows ----- */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  var en=innerOf(d,"s3e");if(en)en.insertAdjacentHTML("afterbegin",'<div class="note">An app may register its own element kinds; they appear here with their styles.</div>');
  d.querySelectorAll('[data-fo^="el:"] .inner').forEach(function(f){f.querySelectorAll(":scope > .row").forEach(function(r){var lb=r.querySelector("label");if(lb&&lb.textContent==="Proportion")lb.insertAdjacentHTML("beforeend",' <span class="prop">PROPOSED</span>')})});
  var st=styleById(S.sel.style);if(st&&(kindOfStyle(st.id)==="picture"||kindOfStyle(st.id)==="video")){var f=innerOf(d,"el:"+kindOfStyle(st.id)),ws=f&&Array.prototype.filter.call(f.querySelectorAll(":scope > .row"),function(r){var l=r.querySelector("label");return l&&l.textContent==="With the scroll"})[0];
    if(ws)ws.insertAdjacentHTML("afterend",row("By itself",sel("style.self",[["none","None"]].concat(S.motions.filter(function(m){return m.type==="self"}).map(function(m){return[m.id,m.name]})),st.self||"none"),"A slow move with no scroll (Unity's Slow Zoom and Pan). Only the picture nearest the middle moves by itself.",true))}
  addRows(d,"s4m",'<div class="note fixed">Fixed rules: the stagger stops growing after 5 rows; only the picture nearest the middle moves by itself; Reduce Motion makes every part whole and still.</div>');
  var bt=d.querySelector('button[disabled][title^="Lab: an SVG"]');if(bt)bt.textContent="+ Import an edge (SVG or a tileable PNG strip)";
  if(S.ctl.openrow==="shine"&&S.secCfg.look==="cards"&&S.secCfg.rim)addRows(d,"s5openrow",'<div class="note warn">Rim light is on (Sections › All sections): the card already has its one glow (D5). Shine adds a second.</div>');
  return d.innerHTML});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(o\\.speed|o\\.pptap|"));
on("modelSig.before",function(cx){var x="";if(sigWant(/^o\.noimg/))(S.styles.picture||[]).forEach(function(st){x+=renderEl("picture",st,{img:"__none"},0,true)});cx.noimg=x});on("modelSig.sig",function(s,cx){return s+cx.noimg});
