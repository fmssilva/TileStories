/* ===================== v4: the Detail Card tab's ontology ===================== */
var SICON={info:'<circle cx="12" cy="12" r="9"/><path d="M12 11v6M12 7.5v.5"/>',book:'<path d="M4 5h6a2 2 0 0 1 2 2v12a2 2 0 0 0-2-2H4zM20 5h-6a2 2 0 0 0-2 2v12a2 2 0 0 1 2-2h6z"/>',
 image:'<rect x="3.5" y="4.5" width="17" height="15" rx="2"/><path d="M5 17l5-5 4 4 2-2 3 3"/><circle cx="15.5" cy="9" r="1.5"/>',headphones:'<path d="M4 15v-3a8 8 0 0 1 16 0v3"/><rect x="3.5" y="14" width="4" height="6" rx="1.5"/><rect x="16.5" y="14" width="4" height="6" rx="1.5"/>',
 play:'<circle cx="12" cy="12" r="9"/><path d="M10 8.5l6 3.5-6 3.5z"/>',pin:'<path d="M12 21s-6.5-6.4-6.5-11.2A6.5 6.5 0 0 1 18.5 9.8C18.5 14.6 12 21 12 21z"/><circle cx="12" cy="10" r="2.3"/>',
 star:'<path d="M12 3.5l2.6 5.4 5.9.8-4.3 4.1 1 5.8-5.2-2.8-5.2 2.8 1-5.8-4.3-4.1 5.9-.8z"/>',chat:'<path d="M4 5h16v10H9l-5 4z"/>',ar:'<path d="M12 3l8 4.5v9L12 21l-8-4.5v-9z"/><path d="M12 12l8-4.5M12 12v9M12 12L4 7.5"/>'};
function sicon(k,sz){return '<svg viewBox="0 0 24 24" width="'+(sz||18)+'" height="'+(sz||18)+'" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'+(SICON[k]||SICON.info)+'</svg>'}
var SCOLOURS=[["about","Terracotta"],["stories","Wine"],["media","Azulejo Blue"],["listen","River Green"],["play","Brick Red"],["visit","Indigo"],["accent2","Accent"]];
S.cardCfg={enabled:true,openAt:"half",halfMax:0.62,peekNext:true,tapOutside:true,mode:"light",layout:"scroll",topBand:"fade",audioOverlap:"duck",keepAudio:true};
S.secCfg={look:"open",start:"tiles",divider:"none",colour:"section",intensity:0.12,tint:120,order:"sections"};
S.sections=[
 {id:"about",name:T("About","Sobre"),icon:"info",colour:"about",shown:true,start:"inherit",kinds:["Header","Quick Facts","Status","Inline","Actions"]},
 {id:"stories",name:T("Stories","Histórias"),icon:"book",colour:"stories",shown:true,start:"inherit",kinds:["Story","Timeline","Compare Points","Dialogue"]},
 {id:"media",name:T("Media","Multimédia"),icon:"image",colour:"media",shown:true,start:"inherit",kinds:["Gallery","Before and After","Hotspot Image","Video","3D Model","360 Panorama"]},
 {id:"listen",name:T("Listen","Ouvir"),icon:"headphones",colour:"listen",shown:true,start:"inherit",kinds:["Audio Guide"]},
 {id:"play",name:T("Play","Jogar"),icon:"play",colour:"play",shown:true,start:"inherit",kinds:["Knowledge Check","Poll","Collect","Feedback","Place In AR"]},
 {id:"visit",name:T("Visit","Visitar"),icon:"pin",colour:"visit",shown:true,start:"inherit",kinds:["Wall Locator","Today Map","Related","Practical Info","Sources","Contents"]}];
S.ctl={bar:{on:true,pos:"bottom",indicator:"raised",depth:1,iconColour:"section",labels:"both",hide:false},jump:{motion:"smooth",opens:"half"},
 top:{title:true,settings:true},crumb:"pages",prevnext:true,openrow:"nudge",locate:{on:true,style:"pin_label",colour:"teal",size:1,float:true},sticky:"auto"};
S.blockDefs={};LIB.forEach(function(r){S.blockDefs[r[1]]={enter:"family",tap:"nothing",orow:"plain",page:false,chrome:"inherit"}});
["Story","Gallery","Timeline","Dialogue","Compare Points","Video"].forEach(function(k){S.blockDefs[k].tap="page";S.blockDefs[k].page=true});
Object.keys(S.blockDefs).forEach(function(k){S.blockDefs[k].orow="inherit";S.blockDefs[k].crumb="pages";S.blockDefs[k].prevnext=true});S.blockDefs["Practical Info"].tap="expand";S.blockDefs.Inline.enter="pop";S.blockDefs["Before and After"].enter="fade";S.blockDefs.Gallery.enter="slide";S.blockDefs["Knowledge Check"].enter="tilt";
S.card.forEach(function(b){b.grp="inherit";b.enter="inherit";b.tap="inherit";b.orow="inherit"});
S.poiSec={};S.texts=[["Open the story","Read the story","Ler a história"],["Close","Close","Fechar"],["Show on the wall","Find it on the wall","Encontre na parede"],["Minutes","min","min"]];
S.glossary=[[T("Torreão","Torreão"),T("The corner tower added to the river front in the 1580s.","A torre de canto acrescentada à frente ribeirinha na década de 1580.")],[T("Pombaline","Pombalino"),T("The rebuilding of Lisbon after 1755, under the Marquis of Pombal.","A reconstrução de Lisboa depois de 1755, com o Marquês de Pombal.")]];
S.sel.section="about";S.sel.blockDef="Story";
function secById(id){return byId(S.sections,id)}
function secOfKind(kind){for(var i=0;i<S.sections.length;i++)if(S.sections[i].kinds.indexOf(kind)>=0)return S.sections[i].id;return S.sections[0].id}
function grpOf(b){return b.grp&&b.grp!=="inherit"?b.grp:secOfKind(b.kind)}
function eff(b,k){var v=b[k];if(v&&v!=="inherit")return v;var d=(S.blockDefs[b.kind]||{})[k];if(k==="orow"&&(!d||d==="inherit"))return S.ctl.openrow;return d}
function colVar(c){return "var(--"+c+")"}
function setPath(path,v){var p=path.split("."),o=S;for(var i=0;i<p.length-1;i++)o=o[isNaN(p[i])?p[i]:+p[i]];var last=p[p.length-1];o[isNaN(last)?last:+last]=v}
function getPath(path){var p=path.split("."),o=S;for(var i=0;i<p.length;i++){if(o==null)return undefined;o=o[isNaN(p[i])?p[i]:+p[i]]}return o}
function pval(v){return v==="true"?true:v==="false"?false:v}
function pseg(path,vals,labels){return seg("@"+path,vals,labels,String(getPath(path)))}
function psel(path,opts){return sel("@"+path,opts,String(getPath(path)))}
function pslider(path,min,max,step){return slider("@"+path,min,max,step,getPath(path))}

/* ----- the phone: sections drive the card, the bar and the colours ----- */
function blockPreview(b,i){return blockPreviewV3(Object.assign({},b,{enter:eff(b,"enter"),tap:eff(b,"tap"),orow:eff(b,"orow")}),i)}
function sectionStart(sec){var st=sec.start==="inherit"?S.secCfg.start:sec.start;if(S.secCfg.look!=="open"||st==="plain")return "";
  var f=st==="tiles"?effectById("e_tiles"):st==="wave"?effectById("e_wave"):st==="arches"?effectById("e_arches"):st==="skyline"?effectById("e_skyline"):null;
  if(!f)return "";return '<div style="margin:0 -20px 10px">'+edgeSvg(Object.assign({},f,{o:Object.assign({},f.o,{colour:"family"})}))+'</div>'}
function cardRuns(){var vis=function(id){var s=secById(id);return s&&s.shown&&S.poiSec[id]!=="hide"};
  if(S.secCfg.order==="placed"){var runs=[];S.card.forEach(function(b){var g=grpOf(b);if(!vis(g))return;if(runs.length&&runs[runs.length-1].sec.id===g)runs[runs.length-1].blocks.push(b);else runs.push({sec:secById(g),blocks:[b]})});return runs}
  return S.sections.filter(function(s){return vis(s.id)}).map(function(s){return{sec:s,blocks:S.card.filter(function(b){return grpOf(b)===s.id})}})}
function cardScreen(){var t=S.test.on&&S.test.poi==="painting",h='<div class="chrome"><i title="'+TXH("close")+'">&#10005;</i><i title="'+TXH("visitor_settings")+'">&#9776;</i></div><div class="hero" style="background-image:url('+IMG.hero+')"><div class="w"><small>'+esc(L(UI.where))+'</small><h2>Ribeira Palace</h2><p>'+esc(L(UI.line))+'</p></div></div>';
  cardRuns().forEach(function(run,si){var sec=run.sec,bl=run.blocks;if(!bl.length&&!(si===0&&t))return;
    var col=colVar(sec.colour),tint=S.secCfg.look==="open"&&S.secCfg.colour!=="neutral"?"linear-gradient(to bottom, color-mix(in srgb,"+(S.secCfg.colour==="accent"?"var(--accent2)":col)+" "+Math.round(S.secCfg.intensity*100)+"%, transparent) 0, transparent "+S.secCfg.tint+"px)":"none";
    h+='<div class="gsec item" data-ref="sec:'+sec.id+'" style="--fam:'+col+';--famtint:color-mix(in srgb,'+col+' 8%,transparent);background:'+tint+(S.secCfg.look==="cards"?";margin:12px;border-radius:18px;background:var(--box)":"")+'">'+sectionStart(sec)+'<div class="ey" style="color:'+(S.secCfg.colour==="neutral"?"var(--ink2)":S.secCfg.colour==="accent"?"var(--accent2)":col)+'">'+esc(L(sec.name))+'</div>';
    if(si===0&&t)h+=testTile();
    bl.forEach(function(b,i){if(i>0&&S.secCfg.divider!=="none"){var dv=effectById(S.secCfg.divider);if(dv)h+=lineSvg(dv)}h+=blockPreview(b,i)});h+='</div>'});
  return h+'<div style="height:60px"></div>';}
function navbar(){var c=S.ctl.bar,first=true;return S.sections.filter(function(s){return s.shown&&S.poiSec[s.id]!=="hide"}).map(function(s){var col=c.iconColour==="section"?colVar(s.colour):c.iconColour==="accent"?"var(--accent2)":"var(--ink2)";
  var chosen=first;first=false;var ic=chosen&&c.indicator==="raised"?'<em style="display:grid;place-items:center;width:'+(30+10*c.depth)+'px;height:'+(30+10*c.depth)+'px;border-radius:50%;background:'+colVar(s.colour)+';color:#fff;margin-top:-'+(14*c.depth)+'px;box-shadow:0 4px 10px rgba(0,0,0,.2)">'+sicon(s.icon,18)+'</em>':sicon(s.icon,18);
  return '<span style="color:'+(chosen?colVar(s.colour):col)+';'+(chosen&&c.indicator==="underline"?"border-bottom:2px solid currentColor;padding-bottom:3px":"")+(chosen&&c.indicator==="pill"?"background:color-mix(in srgb,currentColor 14%,transparent);border-radius:999px;padding:4px 8px":"")+'">'+(c.labels!=="text"?ic:"")+(c.labels!=="icon"?esc(L(s.name)):"")+'</span>'}).join("")}
var renderPhoneV3=renderPhone;
renderPhone=function(keep){renderPhoneV3(keep);var nb=document.getElementById("navbar"),c=S.ctl.bar;
  nb.style.display=S.view==="card"&&c.on?"":"none";nb.style.top=c.pos==="top"?"58px":"";nb.style.bottom=c.pos==="top"?"auto":"";nb.style.alignItems="center";nb.style.paddingBottom="0";
  var old=ph.querySelector(".locpin");if(old)old.remove();
  if(S.view==="card"&&S.ctl.locate.on){var L2=S.ctl.locate,sz=Math.round(40*L2.size),pin=document.createElement("div");pin.className="locpin";
    pin.style.cssText="position:absolute;right:18px;bottom:"+(c.on&&c.pos==="bottom"?92:22)+"px;z-index:7;display:flex;align-items:center;gap:8px";
    var hue={teal:["#7fe3c4","#0e9a7c"],green:["#b8ef7a","#3f9e2c"],cyan:["#8be4ff","#0a84b8"],accent:["#8FB0EC","#1F4E9C"]}[L2.colour]||["#7fe3c4","#0e9a7c"];
    pin.innerHTML=(L2.style==="pin_label"?'<span style="background:rgba(255,255,255,.95);color:#123b32;font:700 12px/1 var(--body);padding:8px 11px;border-radius:999px;box-shadow:0 3px 10px rgba(0,0,0,.18)">'+(TXH("locate_label"))+'</span>':"")+
      '<span class="'+(L2.float&&!S.reduce?"floatpin":"")+'" style="'+(L2.style==="glass"?"display:grid;place-items:center;width:"+(sz+14)+"px;height:"+(sz+14)+"px;border-radius:50%;background:rgba(255,255,255,.85);box-shadow:0 6px 16px rgba(0,0,0,.18)":"")+'">'+PIN.replace('<svg','<svg width="'+sz+'" height="'+Math.round(sz*1.33)+'"').replace(/#7fe3c4/g,hue[0]).replace(/#0e9a7c/g,hue[1])+'</span>';
    ph.appendChild(pin)}};
renderAll=function(keep){renderEditor();renderPhone(keep)};

/* ----- the Detail Card tab, reorganised ----- */
function sectionTitle(n,t,sub){return n+" · "+t+(sub?' <span class="sub">· '+sub+'</span>':"")}
function edCard(){var h="";
  h+='<div class="fo" style="padding:6px 8px"><div class="row"><label>Foldouts</label><div>'+seg("ui.single",["false","true"],["Many open","One at a time"],String(S.ui.single))+'</div>'+help("Editor preference, not saved to the wall.")+'</div>'+row("Open to level",seg("ui.level",["0","1","2","3"],["Closed","1","2","3"],String(S.ui.level)),"How deep the foldouts open.")+
   '<div class="filters"><input class="u" data-q="1" placeholder="Find a setting or part..." value="'+esc(S.ui.q)+'">'+["All"].concat(CATS).map(function(c){return'<button class="chip" type="button" data-cat="'+c+'" aria-pressed="'+(S.ui.cat===c)+'">'+c+'</button>'}).join("")+'</div></div>';
  /* 0. Preview & Test */
  h+=fo("s0",sectionTitle("0","Preview &amp; Test","Editor tools, never in the app"),["test","TEST"],
    row("Show the demo card",pseg("test.on",["true","false"],["On","Off"]),"Adds the Block Components demo block at the top of the chosen marker's first section, in Scene and Play only.")+
    row("On marker",sel("test.poi",POIS,S.test.poi),"The reference card: the Ribeira Palace.",true)+row("Focus on what I edit",seg("focusOn",["true","false"],["On","Off"],String(S.focusOn)),"The demo jumps to the part you edit and dims the rest.",true)+
    row("Preview language",seg("previewLang",langCodes(),langCodes().map(function(c){return c.toUpperCase()}),S.lang),"The language the Editor previews show (the phone's bar too).")+
    '<div class="note">On the phone: the floating Preview bar (theme, motion, language, focus, screen).</div>',1);
  /* 1. Card */
  var c1=fo("s1a","Sheet",null,row("Enable Detail Card",pseg("cardCfg.enabled",["true","false"],["On","Off"]),"Off: tapping a marker shows only its label.")+
     row("Opens at",pseg("cardCfg.openAt",["peek","half","full"],["Peek","Half","Full"]),"The stop the card opens at.")+row("Half height max",pslider("cardCfg.halfMax",0.4,0.8,0.01),"The Half stop's share of the screen.")+
     row("Peek next card",pseg("cardCfg.peekNext",["true","false"],["On","Off"]),"A swipe up from Peek shows the next point's card.")+row("Tap outside closes",pseg("cardCfg.tapOutside",["true","false"],["On","Off"]),""),2)+
   fo("s1b","Look",null,row("Default mode",pseg("cardCfg.mode",["light","dark","phone"],["Light","Dark","Follow the phone"]),"The visitor can change it in the card's settings.")+
     row("Layout",pseg("cardCfg.layout",["scroll","tabs","pages","accordion"],["One scroll","Tabs","Pages","Accordion"]),"How the sections are laid out: one long scroll (default) or one section at a time.")+
     row("Top band",pseg("cardCfg.topBand",["fade","glass","clear"],["Fade","Glass","Clear"]),"The band behind the title after the hero."),2)+
   fo("s1c","Sound",null,row("Two sounds at once",pseg("cardCfg.audioOverlap",["duck","pause","mix"],["Lower the other","Pause the other","Mix"]),"When a video plays over the audio guide.")+row("Keep audio when closed",pseg("cardCfg.keepAudio",["true","false"],["On","Off"]),""),2);
  h+=fo("s1",sectionTitle("1","Card","how the sheet opens, looks and sounds"),["ext","WAS CARD CONTAINER"],c1,1);
  /* 2. Sections */
  var cfg=row("Look",pseg("secCfg.look",["open","cards"],["Open","Cards"]),"Open: sections flow on one ground. Cards: each section on its own sheet.")+
   row("Start",pseg("secCfg.start",["tiles","wave","arches","skyline","plain"],["Tiles","Wave","Arches","Skyline","Plain"]),"What marks a section's start (Effects > Edges).")+
   row("Divider",pseg("secCfg.divider",["none","l_hair","l_tile"],["None","Hairline","Tile Ornament"]),"Between blocks inside a section.")+
   row("Colour",pseg("secCfg.colour",["section","accent","neutral"],["Section colour","Accent","Neutral"]),"The tint at a section's top.")+
   row("Intensity",pslider("secCfg.intensity",0,0.2,0.01),"How strong the tint is (capped so text stays 7:1).")+row("Tint depth (pt)",pslider("secCfg.tint",40,340,5),"How far down the tint fades.")+
   row("Block order",pseg("secCfg.order",["sections","placed"],["By section","As placed"]),"By section: blocks gather in their section's order. As placed: the marker's own order.");
  var sx=fo("s2cfg","Sections config <span class=\"sub\">· for every section</span>",null,cfg,2);
  S.sections.forEach(function(sec,i){var col=colVar(sec.colour);
    var body=langCodes().map(function(lg,j){return row("Name · "+lg.toUpperCase(),'<input class="u" data-path="sections.'+i+'.name.'+lg+'" value="'+esc(sec.name[lg]||"")+'">',j?"":"The word on the Navigation Bar and the section's eyebrow.")}).join("")+
     row("Icon",'<div class="assets">'+Object.keys(SICON).map(function(k){return'<button type="button" data-secicon="'+sec.id+'|'+k+'" aria-pressed="'+(sec.icon===k)+'" style="background:#2E2E2E;color:#EEE;display:grid;place-items:center;width:34px;height:30px">'+sicon(k,16)+'</button>'}).join("")+'</div>',"Its icon on the bar.")+
     row("Colour",'<div class="assets">'+SCOLOURS.map(function(c){return'<button type="button" class="cv" data-seccol="'+sec.id+'|'+c[0]+'" aria-pressed="'+(sec.colour===c[0])+'" title="'+c[1]+'" style="background:var(--'+c[0]+');width:30px;height:26px"></button>'}).join("")+'</div>',"A theme colour: the bar icon, the eyebrow, the tint, the family colour of its elements.")+
     row("Start",'<div class="seg">'+["inherit","tiles","wave","arches","plain"].map(function(v){return'<button type="button" data-set="@sections.'+i+'.start" data-v="'+v+'" aria-pressed="'+(sec.start===v)+'">'+(v==="inherit"?"As configured":v[0].toUpperCase()+v.slice(1))+'</button>'}).join("")+'</div>',"Override the Sections config for this section only.")+
     row("On the bar",'<div class="seg">'+["true","false"].map(function(v){return'<button type="button" data-set="@sections.'+i+'.shown" data-v="'+v+'" aria-pressed="'+(String(sec.shown)===v)+'">'+(v==="true"?"Shown":"Hidden")+'</button>'}).join("")+'</div>',"Hidden: the section and its blocks stay off every card.")+
     '<div class="sub2">Blocks this section holds by default</div><div class="filters">'+LIB.map(function(r){var mine=sec.kinds.indexOf(r[1])>=0;return'<button class="chip" type="button" data-seckind="'+sec.id+'|'+esc(r[1])+'" aria-pressed="'+mine+'">'+esc(r[1])+'</button>'}).join("")+'</div>'+
     '<div class="note">Each block kind lives in one section; tapping a block here moves it from its old section. A marker can place one block elsewhere (Specific Marker > The card). The blocks themselves: section 3, Blocks.</div>'+
     '<div class="btnrow"><button class="del" type="button" data-secmove="'+i+'" data-dir="-1">^ Up</button><button class="del" type="button" data-secmove="'+i+'" data-dir="1">v Down</button><button class="del" type="button" data-secdel="'+i+'" '+(sec.kinds.length?"disabled title=\"Move its blocks first\"":"")+'>Delete</button></div>';
    sx+=fo("sec:"+sec.id,'<span style="display:inline-grid;place-items:center;width:22px;height:22px;border-radius:50%;background:'+col+';color:#fff" class="cv">'+sicon(sec.icon,14)+'</span>'+esc(sec.name.en)+' <span class="sub">· '+sec.kinds.length+' block kinds'+(sec.shown?"":" · hidden")+'</span>',null,body,2,S.sel.layer==="section"&&S.sel.section===sec.id)});
  sx+='<div class="btnrow"><button class="ubtn sm" type="button" data-secadd="1">+ Add section</button></div>';
  h+=fo("s2",sectionTitle("2","Sections","the card's chapters, one per Navigation Bar item"),["ext","ONE PLACE FOR GROUPS"],sx,1);
  /* 3. Blocks & Components */
  var bc="";var elv=edCardV3();
  bc+=fo("s3e","Elements <span class=\"sub\">· the smallest parts, with named styles</span>",["new","NEW"],extractInner(elv,"elements"),2);
  bc+=fo("s3p","Patterns <span class=\"sub\">· elements composed</span>",["new","NEW"],extractInner(elv,"patterns"),2);
  var bl='<div class="note">A block = a part with behaviour, data or an owner. Its defaults: the section it lives in, its preview on the card, and its full page. A marker places it and may override.</div><div class="bgrid">';
  LIB.forEach(function(r){if(!matchQ(r[1]))return;bl+='<button class="sc bcard" type="button" data-blockdef="'+esc(r[1])+'" aria-pressed="'+(S.sel.blockDef===r[1])+'">'+libThumb(r[1])+'<span class="nm">'+esc(r[1])+(r[2]==="new"?' <span class="badge new">NEW</span>':'')+'</span><span class="from">'+esc(L(secById(secOfKind(r[1])).name))+' · '+esc(r[4])+'</span></button>'});bl+='</div>';
  var bd=S.blockDefs[S.sel.blockDef];if(bd){var k=S.sel.blockDef,q="blockDefs."+k+".";
    var prev=row("Entrance",sel("@"+q+"enter",motionOpts("entrance",true),bd.enter),"How its preview comes into view.")+row("Tap",seg("@"+q+"tap",["page","expand","nothing"],["Open its page","Expand here","Nothing"],bd.tap),"What a tap on the preview does.")+
      (bd.tap==="page"?row("Open Row",seg("@"+q+"orow",["plain","nudge","shine"],["Plain","Nudge","Shine"],bd.orow),"The words inviting the tap, and their motion."):"");
    var page=bd.page?row("Page chrome",seg("@"+q+"chrome",["inherit","override"],["As in Navigation &amp; Controls","Override"],bd.chrome),"The top bar, breadcrumb and Previous / Next of its page.")+row("Starts with",'<span class="uses">'+(k==="Story"?"Chapter Opener (pattern)":"its own layout")+'</span>',"A new page of this block starts with these rows (a template).")
      :'<div class="note">This block has no page: everything shows on the card.</div>';
    bl+=fo("bd:"+k,'<b>'+esc(k)+'</b> <span class="sub">· in '+esc(L(secById(secOfKind(k)).name))+'</span>',null,'<div class="sub2">Preview · on the card</div>'+prev+'<div class="sub2">Full page</div>'+page,9,true)}
  bc+=fo("s3b","Blocks <span class=\"sub\">· the Block Library: preview + full page defaults</span>",["ext","EXTENDED"],bl,2);
  h+=fo("s3",sectionTitle("3","Blocks &amp; Components","elements → patterns → blocks"),["new","NEW"],bc,1);
  /* 4. Motion & Effects */
  var me=fo("s4m","Motion",["new","NEW"],extractInner(elv,"motion"),2)+fo("s4i","Interaction",["new","NEW"],extractInner(elv,"interaction"),2)+fo("s4f","Effects",["ext","EXTENDED"],extractInner(elv,"effects"),2);
  h+=fo("s4",sectionTitle("4","Motion &amp; Effects","used by elements, blocks, sections and the card"),["new","NEW"],me,1);
  /* 5. Navigation & Controls */
  var b=S.ctl.bar,nc=fo("s5bar","Navigation Bar",null,row("Enabled",pseg("ctl.bar.on",["true","false"],["On","Off"]),"The bar of sections.")+row("Position",pseg("ctl.bar.pos",["bottom","top"],["Bottom","Top"]),"")+
     row("Indicator",pseg("ctl.bar.indicator",["raised","pill","underline"],["Raised","Pill","Underline"]),"How the chosen section is marked.")+(b.indicator==="raised"?row("Raised depth",pslider("ctl.bar.depth",-1,1,0.1),"+1 a bell, 0 flat, below 0 a cradle.",true):"")+
     row("Icon colour",pseg("ctl.bar.iconColour",["section","accent","neutral"],["Section colour","Accent","Neutral"]),"")+row("Labels",pseg("ctl.bar.labels",["both","icon","text"],["Icon and text","Icon only","Text only"]),"")+
     row("Hide while scrolling",pseg("ctl.bar.hide",["true","false"],["On","Off"]),"")+'<div class="note">Its items are the Sections (section 2): no second list of groups here.</div>',2)+
   fo("s5jump","Jumps",null,row("Jump motion",pseg("ctl.jump.motion",["smooth","instant"],["Smooth","Instant"]),"How the card travels to a section.")+row("A jump opens the card to",pseg("ctl.jump.opens",["half","full"],["Half","Full"]),""),2)+
   fo("s5top","Top Bar",null,row("Title on the bar",pseg("ctl.top.title",["true","false"],["Shown","Hidden"]),"The point's name beside the round buttons after the hero.")+row("Settings button",pseg("ctl.top.settings",["true","false"],["Shown","Hidden"]),"Language, theme, motion for the visitor."),2)+
   fo("s5page","Page Chrome",null,row("Breadcrumb",pseg("ctl.crumb",["pages","off"],["On pages","Off"]),"Point › section › page, on a block's page.")+row("Previous / Next",pseg("ctl.prevnext",["true","false"],["Shown","Hidden"]),"At a page's foot: the neighbouring pages.")+row("Open Row",pseg("ctl.openrow",["plain","nudge","shine"],["Plain","Nudge","Shine"]),"The default emphasis of the words that open a page."),2)+
   fo("s5loc","Locate Button",null,row("Shown",pseg("ctl.locate.on",["true","false"],["On","Off"]),"Lowers the card and shows the point on the wall.")+row("Style",pseg("ctl.locate.style",["pin","pin_label","glass"],["Floating Pin","Pin + Label","Glass + Ping"]),"")+
     row("Colour",pseg("ctl.locate.colour",["teal","green","cyan","accent"],["AR Teal","Fresh Green","Cyan","Accent"]),"")+row("Size",pslider("ctl.locate.size",0.6,2,0.05),"1 = the chosen section's disc.")+row("Float",pseg("ctl.locate.float",["true","false"],["On","Off"]),""),2)+
   fo("s5sticky","Sticky Action",null,row("Sticky action",pseg("ctl.sticky",["auto","shown","off"],["Automatic","Shown","Off"]),"Automatic: off while the Navigation Bar sits at the bottom."),2);
  h+=fo("s5",sectionTitle("5","Navigation &amp; Controls","what the visitor uses to move around"),["ext","WAS NAVIGATION + CARD COMPONENTS + LOCATE"],nc,1);
  /* 6. Shared content */
  var tx='<table class="tbl"><thead><tr><th>Text</th><th>EN</th><th>PT</th></tr></thead><tbody>'+S.texts.map(function(r){return'<tr><td class="uses">'+esc(r[0])+'</td><td>'+esc(r[1])+'</td><td>'+esc(r[2])+'</td></tr>'}).join("")+'</tbody></table>';
  var gl='<table class="tbl"><thead><tr><th style="width:30%">Term</th><th>Definition</th></tr></thead><tbody>'+S.glossary.map(function(g){return'<tr><td>'+esc(L(g[0]))+'</td><td style="white-space:normal">'+esc(L(g[1]))+'</td></tr>'}).join("")+'</tbody></table>';
  var dm='<div class="assets">'+ASSETS.map(function(a){return'<button type="button" style="background-image:url('+IMG[a]+')" aria-label="'+a+'" aria-pressed="false"></button>'}).join("")+'</div><div class="note">Pictures a block falls back on when a marker gives none.</div>';
  h+=fo("s6",sectionTitle("6","Shared Content","words, glossary and fallback media for every card"),["ext","EXISTING"],fo("s6t","Card Texts",null,tx,2)+fo("s6g","Glossary",null,gl,2)+fo("s6m","Default Media",null,dm,2),1);
  h+=jsonBox("config.json · card_settings",{card:S.cardCfg,sections_config:S.secCfg,sections:S.sections.map(function(s){return{key:s.id,name:s.name,icon:s.icon,colour:s.colour,shown:s.shown,start:s.start,blocks:s.kinds}}),controls:S.ctl,block_defaults:S.blockDefs});
  return h;}
/* reuse v3's inner bodies (motion, interaction, effects, elements, patterns) by rendering v3 and lifting them out */
function extractInner(html,id){var d=document.createElement("div");d.innerHTML=html;var f=d.querySelector('[data-fo="'+id+'"] > .inner');return f?f.innerHTML:""}

/* ----- Specific Marker: this point's card ----- */
function edMarker(){var h=row("Marker",sel("m.poi",[["painting","7. Ribeira Palace"]],"painting"),"The marker whose card you edit.")+locked("Position & Visual")+locked("Labels");
  var sb='<div class="note">This card follows Detail Card. Hide a section only this point does not need.</div><div class="filters">'+S.sections.filter(function(s){return s.shown}).map(function(s){var off=S.poiSec[s.id]==="hide";return'<button class="chip" type="button" data-poisec="'+s.id+'" aria-pressed="'+!off+'">'+esc(s.name.en)+(off?" · hidden here":"")+'</button>'}).join("")+'</div>';
  h+=fo("m0","0 · Sections on this card",["new","NEW"],sb,1);
  var cb='<div class="note">Blocks in order, each in its section. Everything says "As the block" until you override it for this point.</div><table class="tbl"><thead><tr><th style="width:22px">#</th><th style="width:80px">Preview</th><th>Block</th><th style="width:80px">Section</th><th style="width:74px"></th></tr></thead><tbody>';
  S.card.forEach(function(b,i){var sec=secById(grpOf(b));cb+='<tr class="pick'+(S.sel.cblock===b.id?" sel":"")+'" data-pick="cblock" data-id="'+b.id+'"><td class="uses">'+(i+1)+'</td><td>'+cardThumb(b)+'</td><td><b>'+esc(b.kind)+'</b>'+(b.pattern?' <span class="uses">· '+esc((patternById(b.pattern)||{}).name||"(missing)")+'</span>':'')+'</td><td class="uses">'+esc(sec?L(sec.name):"(missing)")+(b.grp!=="inherit"?" *":"")+'</td><td>'+mv2("cmove",i)+'</td></tr>';
    if(S.sel.cblock===b.id){var d=S.blockDefs[b.kind]||{},asB=function(v,map){return "As the block ("+(map[v]||v)+")"};
      var x='<div class="fields">'+row("Section",sel("cb.grp",[["inherit","As the block ("+L(secById(secOfKind(b.kind)).name)+")"]].concat(S.sections.map(function(s){return[s.id,L(s.name)]})),b.grp),"Place this block in another section for this point only.")+
      row("Entrance",sel("cb.enter",[["inherit",asB(d.enter,{family:"Family's"})]].concat(motionOpts("entrance",true)),b.enter),"")+
      row("Tap",seg("cb.tap",["inherit","page","expand","nothing"],[asB(d.tap,{page:"page",expand:"expand",nothing:"nothing"}),"Open its page","Expand here","Nothing"],b.tap),"")+
      (eff(b,"tap")==="page"?row("Open Row",seg("cb.orow",["inherit","plain","nudge","shine"],[asB(d.orow,{}),"Plain","Nudge","Shine"],b.orow),""):"")+
      (b.kind==="Inline"?row("Pattern",sel("cb.pattern",S.patterns.map(function(p){return[p.id,p.name]}),b.pattern),"What it places on the card."):"")+'</div>';
      cb+='<tr class="slot"><td></td><td colspan="4">'+x+'</td></tr>'}});
  cb+='</tbody></table><div class="note">+ Add block</div><div class="btnrow">'+LIB.slice(0,12).map(function(r){return'<button class="ubtn sm" type="button" data-cadd="'+esc(r[1])+'">'+esc(r[1])+'</button>'}).join("")+'</div>';
  h+=fo("mcard","1 · The card · blocks and previews",["new","NEW"],cb,1);
  h+=fo("mpages","2 · Pages · each block's content",["new","NEW"],extractInner(edMarkerV3(),"mpages"),1);
  h+=jsonBox("config.json · this point's detail card",{hidden_sections:Object.keys(S.poiSec).filter(function(k){return S.poiSec[k]==="hide"}),blocks:S.card.map(function(b){var o={kind:b.kind};["grp","enter","tap","orow"].forEach(function(k){if(b[k]&&b[k]!=="inherit")o[k==="grp"?"section":k]=b[k]});if(b.pattern)o.pattern=b.pattern;return o}),story_page:markerJson().story_page});
  return h;}

/* ----- v4 events ----- */
ed.addEventListener("click",function(e){var t=e.target;
  if(t.closest("[data-seckind],[data-secicon],[data-seccol],[data-secmove],[data-secdel],[data-secadd],[data-blockdef],[data-poisec]"))e.stopPropagation();
  var sk=t.closest("[data-seckind]");if(sk){var q=sk.dataset.seckind.split("|"),sec=secById(q[0]);if(sec.kinds.indexOf(q[1])>=0)return;S.sections.forEach(function(s){s.kinds=s.kinds.filter(function(k){return k!==q[1]})});sec.kinds.push(q[1]);S.sel.layer="section";S.sel.section=sec.id;renderAll(true);show("card","sec:"+sec.id);return}
  var si=t.closest("[data-secicon]");if(si){var q2=si.dataset.secicon.split("|");secById(q2[0]).icon=q2[1];renderAll(true);show("card");return}
  var sc=t.closest("[data-seccol]");if(sc){var q3=sc.dataset.seccol.split("|");secById(q3[0]).colour=q3[1];renderAll(true);show("card","sec:"+q3[0]);return}
  var sm=t.closest("[data-secmove]");if(sm){var i=+sm.dataset.secmove,j=i+(+sm.dataset.dir);if(j<0||j>=S.sections.length)return;swap(S.sections,i,j);renderAll(true);show("card");return}
  var sd=t.closest("[data-secdel]");if(sd&&!sd.disabled){S.sections.splice(+sd.dataset.secdel,1);renderAll(true);return}
  var sa=t.closest("[data-secadd]");if(sa){var ns={id:nid("sec"),name:T("New section","Nova secção"),icon:"star",colour:"accent2",shown:true,start:"inherit",kinds:[]};S.sections.push(ns);S.op["sec:"+ns.id]=true;renderAll(true);return}
  var bd=t.closest("[data-blockdef]");if(bd){S.sel.blockDef=bd.dataset.blockdef;S.op["bd:"+S.sel.blockDef]=true;renderEditor();var b=S.card.filter(function(x){return x.kind===S.sel.blockDef})[0];show("card",b?"b:"+b.id:null);return}
  var ps=t.closest("[data-poisec]");if(ps){var id=ps.dataset.poisec;S.poiSec[id]=S.poiSec[id]==="hide"?"show":"hide";renderAll(true);show("card");return}},true);
var setOptV3=setOpt;
setOpt=function(key,v){if(key==="previewLang"){S.lang=v;renderAll(true);return}
  if(key[0]==="@"){var path=key.slice(1);setPath(path,pval(v));if(path==="cardCfg.mode")S.dark=v==="dark";
    if(path.indexOf("sections.")===0){var sid=S.sections[+path.split(".")[1]].id;S.sel.layer="section";S.sel.section=sid;renderAll(true);show("card","sec:"+sid);return}
    if(path.indexOf("blockDefs.")===0){renderAll(true);var b=S.card.filter(function(x){return x.kind===S.sel.blockDef})[0];show("card",b?"b:"+b.id:null);return}
    renderAll(true);show("card");return}
  if(key.indexOf("cb.")===0){var cb=byId(S.card,S.sel.cblock);cb[key.slice(3)]=v;renderAll(true);show("card","b:"+cb.id);return}
  setOptV3(key,v)};
ed.addEventListener("change",function(e){var t=e.target,k=t.dataset.sel;if(!k)return;
  if(k[0]==="@"){e.stopImmediatePropagation();setOpt(k,t.value);return}},true);
ed.addEventListener("input",function(e){var t=e.target;if(t.dataset.path){e.stopImmediatePropagation();setPath(t.dataset.path,t.value);renderPhone(true);refreshJsonV4();return}
  if(t.dataset.num&&t.dataset.num[0]==="@"){e.stopImmediatePropagation();var v=parseFloat(t.value);if(isNaN(v))return;setPath(t.dataset.num.slice(1),v);ed.querySelectorAll('[data-num="'+t.dataset.num+'"]').forEach(function(o){if(o!==t)o.value=v});renderPhone(true);refreshJsonV4()}},true);
ed.addEventListener("change",function(e){var t=e.target;if(t.dataset.num&&t.dataset.num[0]==="@"){e.stopImmediatePropagation();renderEditor();show("card")}},true);
function refreshJsonV4(){var pre=ed.querySelector(".json pre");if(!pre)return;if(S.tab==="card")pre.textContent=JSON.stringify({card:S.cardCfg,sections_config:S.secCfg,sections:S.sections.map(function(s){return{key:s.id,name:s.name,icon:s.icon,colour:s.colour,shown:s.shown,start:s.start,blocks:s.kinds}}),controls:S.ctl,block_defaults:S.blockDefs},null,2)}
/* the steps follow the new tab */
document.querySelectorAll("[data-step]").forEach(function(b){b.addEventListener("click",function(){var n=+b.dataset.step;
  S.op={s0:false,s1:n===0,s2:n===2,s3:n===1||n===2||n===3,s4:n===1,s5:false,s6:false,s3e:n===1,s3p:n===2,s3b:n===3,s4m:n===1,"mt:entrance":n===1,mcard:n===4,mpages:n===5,"pg:story":n===5,m0:n===4};
  if(n===1)S.op["mo:"+S.sel.motion]=true;if(n===3)S.op["bd:"+S.sel.blockDef]=true;
  renderEditor()},false)});
