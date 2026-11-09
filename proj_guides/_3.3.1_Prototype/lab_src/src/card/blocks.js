/* Every block kind: its options (KOPT), its preview on the card (KPREV, blockPreviewHtml), its defaults (S.blockDefs) and its Editor rows. */
/* ---------- block previews on the card ---------- */
/* a block on the card, from its effective settings: an own answer or the plain one, its frame, then the last touches */
function blockPreviewHtml(b,i){emitLatestFirst("blockPreviewHtml.before",b,i);var out;
  try{out=firstAnswer("blockPreviewHtml.answer",b,i);if(out===undefined){var m=b.enter==="family"?motionById(S.family):motionById(b.enter),h="",tap=b.tap==="page"?" tap":"";
  var or=b.tap==="page"?'<div class="open'+(b.orow==="nudge"?" nudge":"")+(b.orow==="shine"?" shine":"")+'">'+(b.kind==="Story"?esc(L(UI.read)):esc(L(UI.open))+(b.orow==="nudge"?"":" &#8250;"))+'</div>':"";
  if(b.kind==="Quick Facts")h='<div class="qf"><div><small>'+esc(L(UI.home))+'</small><b>'+esc(L(UI.homev))+'</b></div><div><small>'+esc(L(UI.lost))+'</small><b>'+esc(L(UI.lostv))+'</b></div></div>';
  if(b.kind==="Inline"){var p=patternById(b.pattern);h=p?p.rows.map(function(r,j){return renderEl(r.kind,styleById(r.style)||defStyle(r.kind),{},j,true).replace('class="el-callout','class="el-callout')}).join(""):"";
    if(p&&p.rows[0].kind==="callout"&&(styleById(p.rows[0].style)||{o:{}}).o.calltap==="flip")h=renderEl("callout",styleById(p.rows[0].style),{text:T("The palace had its own opera house, opened in March 1755.","O palácio tinha a sua própria ópera, inaugurada em março de 1755."),back:T("It stood for only seven months.","Durou apenas sete meses.")},0,false).replace(/^<div class="mv[^>]*>|<\/div>$/g,"")}
  if(b.kind==="Story")h='<img alt="" src="'+IMG.g3+'" style="width:100%;height:170px;object-fit:cover;border-radius:12px;display:block"><div class="bt" style="color:var(--stories);margin-top:12px">'+esc(L(UI.where))+'</div><h3>'+esc(L(DEMO.title))+'</h3><p>'+esc(L(DEMO.lede))+'</p>';
  if(b.kind==="Before and After")h=renderEl("before_after",defStyle("before_after"),{},0,true);
  if(b.kind==="Gallery")h=renderEl("carousel",defStyle("carousel"),{},0,true);
  if(b.kind==="Audio Guide")h=renderEl("audio",defStyle("audio"),{},0,true);
  if(b.kind==="Knowledge Check")h='<div class="kq" data-kq><b>'+esc(L(T("Which king moved the court to the river?","Que rei mudou a corte para o rio?")))+'</b><button type="button" data-ok="1">Manuel I</button><button type="button">Philip I</button><button type="button">José I</button></div>';
  if(b.kind==="Practical Info")h='<dl class="info"><dt>'+(S.lang==="pt"?"Onde":"Where")+'</dt><dd>Praça do Comércio</dd></dl><div class="more"><dl class="info" style="margin-top:6px"><dt>'+(S.lang==="pt"?"Hoje":"Today")+'</dt><dd>'+(S.lang==="pt"?"Praça aberta, sempre":"An open square, always")+'</dd><dt>Metro</dt><dd>Terreiro do Paço</dd></dl></div>'+'<div class="open">'+(TXH("open_row_expand"))+' &#8964;</div>';
  var inner='<div class="bprev'+tap+(b.tap==="expand"?" expand":"")+'" '+(b.tap==="page"?'data-go="'+(b.kind==="Story"?"story":"gallery")+'"':b.tap==="expand"?"data-expand":"")+'>'+h+or+'</div>';
  out='<div class="item" data-ref="b:'+b.id+'">'+wrapMv(inner,m,0)+'</div>'}out=pipe("blockPreviewHtml.frame",out,b)}finally{emit("blockPreviewHtml.done",b,i)}
  return pipe("blockPreviewHtml.html",out,b)}
S.blockDefs={};LIB.forEach(function(r){S.blockDefs[r[1]]={enter:"family",tap:"nothing",orow:"plain",page:false,chrome:"inherit"}});
["Story","Gallery","Timeline","Dialogue","Compare Points","Video"].forEach(function(k){S.blockDefs[k].tap="page";S.blockDefs[k].page=true});
Object.keys(S.blockDefs).forEach(function(k){S.blockDefs[k].orow="inherit";S.blockDefs[k].crumb="pages";S.blockDefs[k].prevnext=true});S.blockDefs["Practical Info"].tap="expand";S.blockDefs.Inline.enter="pop";S.blockDefs["Before and After"].enter="fade";S.blockDefs.Gallery.enter="slide";S.blockDefs["Knowledge Check"].enter="tilt";
S.sel.blockDef="Story";
/* ----- the phone: sections drive the card, the bar and the colours ----- */
function blockPreview(b,i){return blockPreviewHtml(Object.assign({},b,{enter:eff(b,"enter"),tap:eff(b,"tap"),orow:eff(b,"orow")}),i)}
/* ----- the options of each kind: [key, label, values, labels, help, default, proposed] ; "el:<kind>" = the look is that element's named style ----- */
var KOPT={
 "Header":[["look","Hero look",["compact","text","parallax","split","spotlight","video","model"],["Compact","Text only","Image parallax","Split then and now","Spotlight crop","Video loop","Model turntable"],"The hero's look. A look without its media (no picture, clip or model) falls back to the plain plate.","parallax"],
   ["motion","Hero motion",["panzoom","pan","zoom","none"],["Pan and zoom","Pan","Zoom","None"],"A wide picture drifts at most 12 pt/s over at least 26 s, and zooms to 1.06. Reduce Motion: centred and still.","panzoom"],
   ["words","Words on the picture",["halo","scrim","halo_only"],["Tone + Halo","Edge scrim","Halo only"],"How the hero's words stay at 4.5:1 over any picture. Tone + Halo: a shade only as dark as each word needs, and a soft halo. The shade's reach is Effects › Tone.","halo"],
   ["zoom","Spotlight zoom",["1.5","2","3"],["1.5x","2x","3x"],"Spotlight crop only: how close the crop is; the marker sets the focus point.","2"],
   ["listen","Listen button",["true","false"],["Shown","Hidden"],"The compact Listen pill in the hero's actions row: it plays the card's first audio guide (Unity's Audio Guide › Hero Chip).","true"]],
 "Status":[["look","Look",["ring","scale"],["Ring","Scale"],"Ring: the condition as a ring and its words (never colour alone). Scale: every outline type in order of damage, this one marked.","ring"]],
 "Quick Facts":[["look","Look",["grid","chips","big"],["Grid","Chips","Big numbers"],"Grid: label / value pairs on hairlines. Chips: one pill per fact. Big numbers: the numbers large (they count up once).","grid"],
   ["cat","Show category",["no","cat","catlevel"],["No","Category","Category and level"],"Adds the point's category (and its level on the wall) as the first fact.","no"]],
 "Actions":[["look","Look",["pills","circles","sticky"],["Pill row","Circles","Sticky"],"Sticky: the first button is pinned at the card's foot (Card Controls › Sticky Action decides where it shows).","pills"]],
 "Story":[["preview","Preview",["auto","custom"],["Automatic","Custom"],"Automatic: the page's first picture, an eyebrow right above the title, the title and the first text, cut at half the card with a fade (the title is never cut). Custom: the rows the marker picks.","auto"],
   ["layout","Page layout",["scroll","chapters"],["One scroll","Chapters"],"Chapters: one chapter at a time, a chapter starting at each Heading in the Chapter style, with Previous / Next and \"Chapter 1 of 3\". Nothing turns by itself.","scroll"],
   ["starts","Starts with",["magazine","short","empty"],["Story -- magazine","Story -- short","Empty"],"A template: its rows are COPIED into a new story, which then owns them. Patterns, unlike templates, stay linked.","magazine"],
   ["resume","Continue reading",["true","false"],["On","Off"],"A story the visitor left half read shows Continue reading, kept on the device.","true",1]],
 "Timeline":[["look","Preview look",["strip","vertical"],["Strip","Vertical"],"The card shows a horizontal strip of event cards (or the vertical list); the page is always vertical.","strip"],
   ["now","Now point",["false","true"],["Off","On"],"A last point, Now, at today.","false"],
   ["pics","A picture per event",["true","false"],["On","Off"],"Each event card shows its own thumbnail.","true",1]],
 "Gallery":[["look","Look","el:carousel"],["from","Pictures from",["own","all"],["Its own","Every picture on the card"],"Every picture on the card: every visual medium the card holds, in block order; its page filters All / Pictures / Video / 360 / 3D.","own"]],
 "Before and After":[["look","Look","el:before_after"]],
 "Hotspot Image":[["look","Look","el:hotspot"]],
 "Video":[["look","Look","el:video"],["captions","Captions",["inherit","true","false"],["As the Video player","On at start","Off at start"],"Overrides Card Controls › Video player for this block.","inherit"]],
 "3D Model":[["look","Look","el:model_3d"],["room","See it in the room",["true","false"],["Shown","Hidden"],"On the 3D Model's page: a button to Place In AR with the same model.","true",1]],
 "360 Panorama":[["look","Look","el:panorama"]],
 "Audio Guide":[["look","Look","el:audio"],["chapters","Chapters",["true","false"],["Shown","Hidden"],"A time-coded list to jump inside a long narration, under the player.","true",1]],
 "Knowledge Check":[["qtype","Question type",["multi","tf","pic"],["Multiple choice","True or false (swipe)","Picture choice"],"How every question of the block asks.","multi"],
   ["appears","Appears",["card","read"],["With the card","After the card is read"],"After the card is read: it shows once the visitor has seen most of the card.","card"],
   ["score","Score",["none","end"],["None","At the end"],"At the end: \"3 of 4 right\" after the last question. Never a penalty.","none"],
   ["retry","Retry",["false","true"],["Off","On"],"Try again after the last question; children and groups want a second go.","false",1]],
 "Feedback":[["answer","Answer with",["thumbs","stars"],["Thumbs","Stars"],"Thumbs: Helpful / Not helpful. Stars: 1 to 5. One answer, kept on the device.","thumbs"]],
 "Place In AR":[["dist","Distance from the wall (cm)",[0,200,1],null,"How far in front of the wall the model stands.",10],
   ["size","Size",["real","height","times"],["Real size","A set height","Times the marker"],"How large the model stands in the room.","real"],
   ["height","Height (cm)",[1,500,1],null,"With A set height.",30],["times","Times the marker",[0.5,20,0.5],null,"With Times the marker.",3],
   ["keep","When the visitor picks another point",["take","keep"],["Take it away","Keep it standing"],"Keep it standing: the model stays while the visitor reads another card; the placed model chip takes it away.","take"]],
 "Wall Locator":[["look","Look",["strip","neighbours"],["Strip","Neighbours"],"Strip: the wall as a strip, with this point and You are here. Neighbours: the points beside it, with arrows.","strip"]],
 "Today Map":[["look","Look",["map","thennow"],["Map","Then and now"],"Then and now: the wall's place and today's place side by side. Directions are built from the coordinates when the marker gives no link.","map"]],
 "Related":[["look","Look",["strip","next"],["Strip","Next along the wall"],"Strip: neighbours on a swipe strip. Next along the wall: one large card for the next point.","strip"],
   ["from","Points from",["hand","category","nearest"],["Picked by hand","Same category","Nearest"],"Up to 6 points. Picked by hand: the marker lists them.","hand"]],
 "Sources":[["look","Look",["list","confidence"],["List","With confidence"],"With confidence: each row says Verified or Draft.","list"]],
 "Contents":[["rows","Rows",["sections","every"],["Sections first","Every block"],"The same choice as the Contents sheet, for this block.","sections"],["ticks","Read ticks",["true","false"],["Shown","Hidden"],"","true"]]};
var PAGE_KINDS=["Story","Gallery","Timeline","Dialogue","Video","3D Model","360 Panorama","Knowledge Check","Hotspot Image","Today Map"];
Object.keys(S.blockDefs).forEach(function(k){var bd=S.blockDefs[k];bd.offered=true;bd.frame=k==="Sources"?"quiet":"normal";bd.o={};
  (KOPT[k]||[]).forEach(function(o){bd.o[o[0]]=typeof o[2]==="string"?(defStyle(o[2].slice(3))||{}).id:o[5]});
  bd.page=PAGE_KINDS.indexOf(k)>=0;if(bd.page&&bd.tap==="nothing")bd.tap="page";if(!bd.page&&bd.tap==="page")bd.tap="nothing"});
S.blockDefs.Sources.tap="expand";S.blockDefs["Compare Points"].tap="nothing";
function kopt(kind,key){var ans=firstAnswer("kopt.answer",kind,key);if(ans!==undefined)return ans;var bd=S.blockDefs[kind],v=bd&&bd.o?bd.o[key]:undefined;return typeof v==="boolean"?String(v):v}
function kstyle(kind,el){var id=kopt(kind,"look");return styleById(id)||defStyle(el)}

/* ----- the Editor: Blocks ----- */
function koptRows(k){var q="blockDefs."+k+".",bd=S.blockDefs[k],h="";
  if(k!=="Header")h+=row("Offered on this wall",pseg(q+"offered",["true","false"],["On","Off"]),"Off: no marker of this wall can place it; blocks already placed are not shown.");
  (KOPT[k]||[]).forEach(function(o){var p=q+"o."+o[0],r;
    if(typeof o[2]==="string"){var el=o[2].slice(3);r=row(o[1],psel(p,(S.styles[el]||[]).map(function(st){return[st.id,(st.def?"★ ":"")+st.name]})),"A named style of the "+KIND[el].n+" element (Blocks & Components › Elements): its options live there.")}
    else if(o[3]===null)r=row(o[1],pslider(p,o[2][0],o[2][1],o[2][2]),o[4],o[0]==="height"||o[0]==="times");
    else r=(o[6]?prow:row)(o[1],pseg(p,o[2],o[3]),o[4]);h+=r});
  if(k!=="Header")h+=row("Frame",pseg(q+"frame",["normal","featured","quiet"],["Normal","Featured","Quiet"]),"Cards style only. Featured: a deeper shadow and a larger title, for the one or two blocks to push. Quiet: a hairline edge, for credits. Every choice keeps the 3:1 edge (D2).");
  return h}
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  Object.keys(S.blockDefs).forEach(function(k){var f=innerOf(d,"bd:"+k);if(!f)return;var bd=S.blockDefs[k],q="blockDefs."+k+".";
    var first=f.querySelector(":scope > .sub2");if(first)first.insertAdjacentHTML("beforebegin",'<div class="sub2">Its own options</div>'+koptRows(k));
    if(k==="Header"){var tr=rowEl(d,"bd:"+k,"Tap");if(tr)tr.remove()}
    else setRow(d,"bd:"+k,"Tap",row("Tap",pseg(q+"tap",bd.page?["page","orow","expand","nothing"]:["expand","nothing"],bd.page?["Open its page","Only the Open Row","Expand here","Nothing"]:["Expand here","Nothing"]),bd.page?"Open its page: the whole preview opens it. Only the Open Row: the rest of the preview keeps its own gestures. Nothing: no page at all.":"This kind has no page: everything shows on the card."));
    if(bd.tap==="orow"){var orw=rowEl(d,"bd:"+k,"Tap");if(orw&&!rowEl(d,"bd:"+k,"Open Row"))orw.insertAdjacentHTML("afterend",row("Open Row",pseg(q+"orow",["inherit","plain","nudge","shine"],["As in Card Controls ("+S.ctl.openrow+")","Plain","Nudge","Shine"]),"The words inviting the tap, and their motion."))}
    var sw=rowEl(d,"bd:"+k,"Starts with");if(sw)sw.remove()});
  var nt=innerOf(d,"s3b");if(nt){var n0=nt.querySelector(":scope > .note");if(n0)n0.innerHTML="A block = a part with behaviour, data or an owner. Each one opens to its own options (its look and settings, Unity's Block Library), then its Preview on the card and its Full Page. A marker places it, picks its look \"As the block\" or its own, and may override the rest. Kinds that became patterns or merged: "+esc(GONE)}
  return d.innerHTML});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(blockDefs\\.Place In AR\\.o\\.|blockDefs\\.Knowledge Check\\.o\\.appears|"));
/* ----- previews: one per kind, drawn from its options ----- */
function hstrip(items){return '<div class="hstrip">'+items.join("")+'</div>'}
function ring(p,col,label){var r=26,c=2*Math.PI*r;return '<svg viewBox="0 0 64 64" width="64" height="64" aria-hidden="true"><circle cx="32" cy="32" r="'+r+'" fill="none" stroke="var(--hair)" stroke-width="7"/><circle cx="32" cy="32" r="'+r+'" fill="none" stroke="'+col+'" stroke-width="7" stroke-dasharray="'+(c*p/100).toFixed(1)+' '+c.toFixed(1)+'" transform="rotate(-90 32 32)" stroke-linecap="round"/><text x="32" y="37" text-anchor="middle" font-size="14" font-weight="700" fill="var(--ink1)">'+p+'%</text></svg>'}
var KPREV={
 "Status":function(){var lv=[T("Intact","Intacto"),T("Altered","Alterado"),T("Damaged","Danificado"),T("Ruin","Ruína"),T("Lost","Perdido")];
   if(kopt("Status","look")==="scale")return '<div class="bt">'+TXH("status_heading")+'</div><div class="scale5">'+lv.map(function(l,i){return'<span class="'+(i===4?"on":"")+'">'+esc(L(l))+'</span>'}).join("")+'</div>';
   return '<div class="bt">'+TXH("status_heading")+'</div><div class="ringrow">'+ring(100,"var(--play)")+'<div><b>'+esc(L(lv[4]))+'</b><span>'+esc(fmt(TXT("status_percent"),100))+' · 1755</span></div></div>'},
 "Quick Facts":function(){var lk=kopt("Quick Facts","look"),cat=kopt("Quick Facts","cat"),f=[[L(UI.home),L(UI.homev)],[L(UI.lost),L(UI.lostv)]];
   if(cat!=="no")f.unshift([TXT("facts_category"),L(T("Royal palace","Palácio real"))+(cat==="catlevel"?" · "+L(T("Level 1","Nível 1")):"")]);
   if(lk==="chips")return '<div class="qchips">'+f.map(function(x){return'<span><small>'+esc(x[0])+'</small>'+esc(x[1])+'</span>'}).join("")+'</div>';
   if(lk==="big")return '<div class="qbig"><div><b>c. 1500</b><small>'+esc(f[f.length-2][0])+'</small></div><div><b>1755</b><small>'+esc(f[f.length-1][0])+'</small></div>'+(cat!=="no"?'<div class="wide"><small>'+esc(f[0][0])+'</small>'+esc(f[0][1])+'</div>':"")+'</div>';
   return '<div class="qf">'+f.map(function(x){return'<div><small>'+esc(x[0])+'</small><b>'+esc(x[1])+'</b></div>'}).join("")+'</div>'},
 "Actions":function(){var lk=kopt("Actions","look"),a=[["pin",TXT("locate_label")],["book",L(T("Read the story","Ler a história"))],["chat",L(T("Share","Partilhar"))]];
   if(lk==="circles")return '<div class="acirc">'+a.map(function(x){return'<button type="button"><i>'+sicon(x[0],20)+'</i><span>'+esc(x[1])+'</span></button>'}).join("")+'</div>';
   if(lk==="sticky")return '<div class="asticky"><button type="button">'+TXH("show_on_wall_button")+'</button><small>'+esc(L(T("Pinned at the card's foot (Card Controls › Sticky Action).","Fixo no fundo do cartão (Card Controls › Sticky Action).")))+'</small></div>';
   return '<div class="apills">'+a.map(function(x,i){return'<button type="button" class="'+(i===0?"focal":"")+'">'+sicon(x[0],16)+esc(x[1])+'</button>'}).join("")+'</div>'},
 "Story":function(b){var custom=(b.preview&&b.preview!=="inherit"?b.preview:kopt("Story","preview"))==="custom",res=kopt("Story","resume")==="true";
   var h=custom?'<div class="bt" style="color:var(--stories)">'+esc(L(UI.where))+'</div><h3>'+esc(L(DEMO.title))+'</h3>'+renderEl("pull_quote",defStyle("pull_quote"),{},0,true):
     '<img alt="" src="'+IMG.g3+'" style="width:100%;height:170px;object-fit:cover;border-radius:12px;display:block"><div class="bt" style="color:var(--stories);margin-top:12px">'+esc(L(UI.where))+'</div><h3>'+esc(L(DEMO.title))+'</h3><p class="capfade">'+esc(L(DEMO.lede))+'</p>';
   return h+(res?'<div class="resume"><button type="button" data-go="story">'+TXH("story_resume")+' &#8250;</button><small>'+esc(L(T("You stopped at Two centuries of additions","Parou em Dois séculos de acrescentos")))+'</small></div>':"")},
 "Timeline":function(){var ev=[["c. 1500",T("The court moves to the river","A corte muda-se para o rio"),"g3"],["1580s",T("The Torreão rises","Ergue-se o Torreão"),"before"],["1755",T("Earthquake, fire and tsunami","Terramoto, incêndio e maremoto"),"g1"]],now=kopt("Timeline","now")==="true",pics=kopt("Timeline","pics")==="true";
   if(now)ev.push([TXT("timeline_now"),T("The Praça do Comércio","A Praça do Comércio"),"g2"]);
   if(kopt("Timeline","look")==="vertical")return '<div class="tl">'+ev.map(function(e){return'<div><b>'+esc(e[0])+'</b><span>'+esc(L(e[1]))+'</span></div>'}).join("")+'</div>';
   return hstrip(ev.map(function(e){return'<div class="tcard">'+(pics?'<img alt="" src="'+IMG[e[2]]+'">':"")+'<b>'+esc(e[0])+'</b><span>'+esc(L(e[1]))+'</span></div>'}))},
 "Compare Points":function(){return '<div class="bt">'+TXH("compare_heading")+'</div><div class="cmp"><div>'+ring(100,"var(--play)")+'<b>Ribeira Palace</b></div><div>'+ring(20,"var(--media)")+'<b>Lisbon Cathedral</b></div></div>'},
 "Dialogue":function(){return '<div class="dlg"><p class="them">'+esc(L(T("Your Majesty, the river front is finished.","Majestade, a frente do rio está concluída.")))+'</p><p class="you"><small>'+TXH("dialogue_you")+'</small>'+esc(L(T("And the tower?","E o torreão?")))+'</p><div class="dopts"><button type="button">'+esc(L(T("Ask about the opera","Perguntar pela ópera")))+'</button><button type="button">'+TXH("dialogue_continue")+'</button></div></div>'},
 "Gallery":function(){var all=kopt("Gallery","from")==="all";return renderEl("carousel",kstyle("Gallery","carousel"),{},0,true)+'<div class="fmt">'+esc(fmt(TXT("gallery_count"),all?9:4))+(all?" · "+esc(L(T("every picture on the card","todas as imagens do cartão"))):"")+'</div>'},
 "Before and After":function(){return renderEl("before_after",kstyle("Before and After","before_after"),{},0,true)},
 "Hotspot Image":function(){return renderEl("hotspot",kstyle("Hotspot Image","hotspot"),{},0,true)},
 "Video":function(){return renderEl("video",kstyle("Video","video"),{},0,true)},
 "3D Model":function(){return renderEl("model_3d",kstyle("3D Model","model_3d"),{},0,true)},
 "360 Panorama":function(){return renderEl("panorama",kstyle("360 Panorama","panorama"),{},0,true)},
 "Audio Guide":function(){return renderEl("audio",kstyle("Audio Guide","audio"),{},0,true)+(kopt("Audio Guide","chapters")==="true"?'<div class="achap"><small>'+TXH("video_chapters")+'</small><button type="button">0:00 '+esc(L(T("The river palace","O palácio do rio")))+'</button><button type="button">1:20 '+esc(L(T("The opera","A ópera")))+'</button><button type="button">2:30 1755</button></div>':"")},
 "Knowledge Check":function(){var t=kopt("Knowledge Check","qtype"),late=kopt("Knowledge Check","appears")==="read",q=L(T("Which king moved the court to the river?","Que rei mudou a corte para o rio?")),h;
   if(t==="tf")h='<div class="kq tf"><div class="tfcard">'+esc(L(T("The opera house stood for only seven months.","A ópera durou apenas sete meses.")))+'</div><div class="tfb"><button type="button" data-ok="1">'+TXH("knowledge_true")+'</button><button type="button">'+TXH("knowledge_false")+'</button></div><small>'+TXH("knowledge_swipe_hint")+'</small></div>';
   else if(t==="pic")h='<div class="kq"><b>'+esc(L(T("Which picture shows the palace before 1755?","Que imagem mostra o palácio antes de 1755?")))+'</b><div class="kpic"><button type="button" data-ok="1"><img alt="" src="'+IMG.before+'"></button><button type="button"><img alt="" src="'+IMG.g2+'"></button><button type="button"><img alt="" src="'+IMG.g1+'"></button><button type="button"><img alt="" src="'+IMG.g3+'"></button></div></div>';
   else h='<div class="kq" data-kq><b>'+esc(q)+'</b><button type="button" data-ok="1">Manuel I</button><button type="button">Philip I</button><button type="button">José I</button></div>';
   return '<div class="bt">'+TXH("knowledge_check_heading")+' · '+esc(fmt(TXT("knowledge_question_of"),1,3))+'</div>'+h},
 "Poll":function(){return '<div class="poll"><b>'+esc(L(T("Should the opera house be rebuilt?","Devia a ópera ser reconstruída?")))+'</b><button type="button">'+esc(L(T("Yes","Sim")))+'</button><button type="button">'+esc(L(T("No","Não")))+'</button><button type="button">'+esc(L(T("Only as a museum","Só como museu")))+'</button></div>'},
 "Collect":function(){return '<div class="collect"><span class="stamp">'+TILE.replace("<svg",'<svg width="40" height="40"')+'</span><div><b>'+TXH("collect_heading")+'</b><small>'+esc(fmt(TXT("collect_progress"),3,12))+'</small></div><button type="button">'+TXH("collect_add")+'</button></div>'},
 "Feedback":function(){if(kopt("Feedback","answer")==="stars")return '<div class="fbk"><b>'+TXH("feedback_stars_question")+'</b><div class="stars">'+[1,2,3,4,5].map(function(n){return'<button type="button" aria-label="'+esc(fmt(TXT("feedback_star"),n,5))+'">&#9734;</button>'}).join("")+'</div></div>';
   return '<div class="fbk"><b>'+TXH("feedback_thumbs_question")+'</b><div class="thumbs2"><button type="button">&#128077; '+TXH("feedback_thumb_up")+'</button><button type="button">&#128078; '+TXH("feedback_thumb_down")+'</button></div></div>'},
 "Place In AR":function(){return '<div class="placear"><div class="arstill" style="background-image:url('+IMG.g1+')"></div><button type="button" class="arfocal">'+TXH("place_in_ar_button")+'</button><small>'+TXH("place_in_ar_not_localised")+'</small></div>'},
 "Wall Locator":function(){if(kopt("Wall Locator","look")==="neighbours")return '<div class="bt">'+TXH("wall_locator_heading")+'</div><div class="nbrs"><span>&#8592; Lisbon Cathedral</span><b>Ribeira Palace</b><span>St George\'s Castle &#8594;</span></div>';
   return '<div class="bt">'+TXH("wall_locator_heading")+'</div><div class="wstrip"><i style="left:12%"></i><i style="left:34%"></i><i class="me" style="left:58%"></i><i style="left:81%"></i><em style="left:46%">'+TXH("wall_locator_you")+'</em></div>'},
 "Today Map":function(){var h=kopt("Today Map","look")==="thennow"?'<div class="tnmap"><div><small>'+TXH("today_map_then")+'</small><img alt="" src="'+IMG.g3+'"></div><div><small>'+TXH("today_map_now")+'</small>'+renderEl("map",defStyle("map"),{},0,true)+'</div></div>':renderEl("map",defStyle("map"),{},0,true);
   return '<div class="bt">'+TXH("today_map_heading")+'</div>'+h+'<div class="fmt">'+esc(fmt(TXT("today_map_coordinates"),"38.7075","-9.1364"))+'</div>'},
 "Related":function(){var from=kopt("Related","from"),pts=from==="category"?["Ajuda Palace","Necessidades Palace","Queluz Palace"]:from==="nearest"?["Lisbon Cathedral","The Camera","St George's Castle"]:["Lisbon Cathedral","St George's Castle"];
   if(kopt("Related","look")==="next")return '<div class="bt">'+TXH("related_heading")+'</div><div class="nextalong"><img alt="" src="'+IMG.g2+'"><div><small>'+esc(L(T("Next along the wall","A seguir na parede")))+'</small><b>'+esc(pts[0])+'</b></div><em>&#8250;</em></div>';
   return '<div class="bt">'+TXH("related_heading")+'</div>'+hstrip(pts.map(function(p,i){return'<div class="rcard"><img alt="" src="'+IMG[["g1","g2","before"][i%3]]+'"><b>'+esc(p)+'</b></div>'}))},
 "Sources":function(){var conf=kopt("Sources","look")==="confidence",r=[["Ayres de Carvalho, D. João V e a arte do seu tempo, 1962","v"],["Kubler, Portuguese Plain Architecture, 1972","d"]];
   return '<div class="bt">'+TXH("sources_heading")+'</div><ul class="srcs">'+r.map(function(x){return'<li>'+esc(x[0])+(conf?' <em class="'+x[1]+'">'+TXH(x[1]==="v"?"sources_verified":"sources_draft")+'</em>':"")+'</li>'}).join("")+'</ul><div class="fmt">'+esc(L(T("6 sources","6 fontes")))+'</div>'},
 "Contents":function(){var runs=cardRuns(),ev=kopt("Contents","rows")==="every",tk=kopt("Contents","ticks")==="true";
   return '<div class="bt">'+TXH("nav_menu")+'</div><div class="cblock">'+(ev?S.card.slice(0,6).map(function(b,j){return'<div>'+esc(b.kind)+(tk&&j<2?' <em>&#10003;</em>':"")+'</div>'}).join(""):runs.map(function(r,k){return'<div><b>'+esc(L(r.sec.name))+'</b> <span>'+esc(fmt(TXT("nav_contents_section_read"),tk&&k===0?1:0,r.blocks.length))+'</span></div>'}).join(""))+'</div>'}};
on("blockPreviewHtml.answer",function(b,i){var bd=S.blockDefs[b.kind]||{},f=KPREV[b.kind];
  if(b.kind==="Practical Info"||b.kind==="Inline"||!f)return;
  var m=b.enter==="family"?motionById(S.family):motionById(b.enter),page=bd.page&&(b.tap==="page"||b.tap==="orow"),h=f(b);
  var or=page?'<div class="open'+(b.orow==="nudge"?" nudge":"")+(b.orow==="shine"?" shine":"")+'"'+(b.tap==="orow"?' data-go="kpage" data-kind="'+esc(b.kind)+'"':"")+'>'+(b.kind==="Story"?esc(L(UI.read)):esc(L(UI.open))+(b.orow==="nudge"?"":" &#8250;"))+'</div>':b.tap==="expand"?'<div class="open">'+TXH("open_row_expand")+' &#8964;</div>':"";
  var go=b.tap==="page"?' data-go="'+(b.kind==="Story"?"story":b.kind==="Gallery"?"gallery":"kpage")+'" data-kind="'+esc(b.kind)+'"':b.tap==="expand"?" data-expand":"";
  var inner='<div class="bprev'+(b.tap==="page"?" tap":"")+(b.tap==="expand"?" expand":"")+'"'+go+'>'+h+or+'</div>';
  return '<div class="item" data-ref="b:'+b.id+'">'+wrapMv(inner,m,0)+'</div>'});
on("blockPreviewHtml.frame",function(h,b){return frameOf(b,h)});
function frameOf(b,h){var fr=(S.blockDefs[b.kind]||{}).frame;return fr&&fr!=="normal"&&S.secCfg.look==="cards"?h.replace('class="bprev','class="bprev fr-'+fr):h}
/* a block whose kind is not offered leaves every card */
on("cardRuns.runs",function(runs){return runs.map(function(r){return{sec:r.sec,blocks:r.blocks.filter(function(b){return(S.blockDefs[b.kind]||{}).offered!==false})}})});
/* the hero: its look, words on the picture, motion, the Listen pill */
on("cardScreen.html",function(h){var d=document.createElement("div");d.innerHTML=h;var hero=d.querySelector(".hero");
  if(hero){var lk=kopt("Header","look"),w=hero.querySelector(".w");hero.classList.add("hl-"+lk,"hw-"+kopt("Header","words"),"hm-"+(S.reduce?"none":kopt("Header","motion")));
    if(lk==="text")hero.style.backgroundImage="none";
    if(lk==="split")hero.insertAdjacentHTML("afterbegin",'<div class="hsplit"><span style="background-image:url('+IMG.before+')"><em>'+TXH("header_then")+'</em></span><span style="background-image:url('+IMG.g2+')"><em>'+TXH("header_now")+'</em></span></div>');
    if(lk==="spotlight")hero.style.backgroundSize=(+kopt("Header","zoom")*100)+"%";
    if(lk==="video")hero.insertAdjacentHTML("afterbegin",'<span class="hloop">&#9654; loop</span>');
    if(lk==="model")hero.insertAdjacentHTML("afterbegin",'<span class="hloop">3D</span>');
    if(w){var gs=w.querySelector(".gstart"),listen=kopt("Header","listen")==="true"&&lk!=="compact";if(gs||listen){var row1=document.createElement("div");row1.className="hact";if(gs)row1.appendChild(gs);
      if(listen)row1.insertAdjacentHTML("beforeend",'<button type="button" class="hlisten">'+sicon("headphones",15)+esc(fmt(TXT("hero_listen"),"3:11"))+'</button>');w.appendChild(row1)}}}
  return d.innerHTML});
on("modelSig.before",function(cx){var keep=S.view,pk=S.pageKind,look=S.secCfg.look,x="";
  if(sigWant(/blockDefs|ctl\.(player|pageChips)|^o\.|^style\.|card\.\d+\./)){S.view="kpage";PAGE_KINDS.forEach(function(k){if(k!=="Story"&&k!=="Gallery"){S.pageKind=k;x+=kpageScreen()}})}
  if(sigWant(/ctl\.viewer/)){S.view="viewer";x+=viewerScreen()}
  if(sigWant(/secCfg\.(glass|rim)|frame/)){S.secCfg.look="cards";S.view="card";x+="#cards#"+demoCardScreen();S.secCfg.look=look}
  if(sigWant(/secCfg\.divSec/)){var st=S.secCfg.start;S.view="card";S.secCfg.start="fade";x+="#fade#"+cardScreen();S.secCfg.start=st}
  if(sigWant(/ctl\.term/)){x+="#gloss#"+gloss("[[Torreão]] [[Torreão]]");S._gseen={}}S.view=keep;S.pageKind=pk;cx.pages=x});on("modelSig.sig",function(s,cx){return s+cx.pages});
var FOCUS8={s5viewer:["viewer",".viewer .vtop"],s5player:["kpage",".vplayer"],s5chip:["card",".hero"]};Object.keys(FOCUS8).forEach(function(k){FOCUS[k]=FOCUS8[k]});
ed.addEventListener("toggle",function(e){var d=e.target;if(!d.dataset||d.dataset.fo!=="s5player"||!d.open)return;S.pageKind="Video"},true);
/* block thumbnails: the kind's own preview, where it has one */
on("libThumb.answer",function(name){var f=KPREV[name];if(!f||name==="Quick Facts")return;var h;try{h=f({kind:name,id:"thumb"})}catch(e){return}return noBtn('<div class="thumb cv'+(S.dark?" dark":"")+'"><div class="tin"><div class="bprev">'+h+'</div></div></div>')});
/* ----- the hero keeps one action: the guide button (the Listen pill left; the Audio Guide block plays the audio) ----- */
KOPT.Header=KOPT.Header.filter(function(o){return o[0]!=="listen"});delete S.blockDefs.Header.o.listen;
/* ----- each block row's demo (Blocks & Components › Blocks › a block): that block in view, the marker's card when it holds it, else
   the demo card, ringed (Offered Off: its section, where it was); an option of its full page opens the page; Entrance plays it once;
   Tap and Open Row ring its Open Row; Page chrome, Breadcrumb and Previous / Next open its page at that part. One builder for every
   kind (blockRowDemo); the rows made in editor/card-tab.js (Entrance, Open Row, Page chrome) use it too. Unity: the row's preview
   action opens the reference card at that block (CardTestInput.IntoReach) ----- */
var KOPT_PAGE={Story:["layout"],Video:["captions"],"Knowledge Check":["score","retry"],"3D Model":["room"]};
function blockAt(kind){if(kind==="Header")return{view:"card",sel:".hero"};var b=S.card.filter(function(x){return x.kind===kind})[0],v="card";
  if(!b){b=S.demoCard.filter(function(x){return x.kind===kind})[0];v="democard"}return{view:v,sel:(b?'[data-ref="b:'+b.id+'"]|':"")+'[data-ref="sec:'+secOfKind(kind)+'"]'}}
function blockRowDemo(kind,field){var o=field.indexOf("o.")===0?field.slice(2):null,first=function(){return blockAt(kind).sel.split("|")[0]};
  if(o&&(KOPT_PAGE[kind]||[]).indexOf(o)>=0)return{page:kind,wait:0,ring:{layout:".chapbar|.pg .blk",captions:".vcap|.vplayer",room:".arfocal|.pg .blk",score:".kscore|.pg .blk",retry:".ubtn2|.pg .blk"}[o]};
  if(field==="chrome"||field==="crumb")return{page:kind,wait:0,ring:field==="crumb"?".topbar span|.topbar":".topbar"};
  if(field==="prevnext")return{page:kind,wait:0,scroll:".pfoot|.pg",ring:".pfoot|.pg"};
  var sp={block:kind,wait:0};
  if(field==="enter"){sp.act=function(){var e=phFind(blockAt(kind).sel);if(e)replayIn(e)};sp.ringAfter=60}
  if(field==="tap"||field==="orow")sp.ring=sp.scroll=function(){return first()+" .open|"+blockAt(kind).sel};
  return sp}
rowDemos(function(key){var m=/^blockDefs\.([^.]+)\.(offered|frame|tap|o\.\w+)$/.exec(key);if(m&&S.blockDefs[m[1]])return blockRowDemo(m[1],m[2])});
