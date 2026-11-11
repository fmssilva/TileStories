/* The Detail Card tab (edCard, its point edCard.html), the @path options and the steps bar. */
/* the Detail Card's document (edCardDoc): every section its modules make or fill through edCard.html, the guide's too; the tab shows it
   through edCard.tab, where a section shown by another tab is taken out (editor/guide-tab.js takes the Guide's to the Guide tab) */
function edCard(){return pipe("edCard.tab",edCardDoc())}
/* edCardDoc("guide"): the guide's part only (the Guide tab's draw): the card's own sections are not drawn, so each joiner works on the
   guide's foldouts alone (guidance round 14: the Guide tab's draw went from about 1.3 s to a fraction of it) */
function edCardDoc(part){var h="";if(part==="guide")return pipe("edCard.html",h);
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
  return pipe("edCard.html",h);}
/* reuse the Blocks & Components bodies (motion, interaction, effects, elements, patterns) by rendering edCardV3 and lifting them out */
function extractInner(html,id){var d=document.createElement("div");d.innerHTML=html;var f=d.querySelector('[data-fo="'+id+'"] > .inner');return f?f.innerHTML:""}
on("setOpt.answer",function(key,v){if(key==="previewLang"){S.lang=v;renderAll(true);return true}
  if(key[0]==="@"){var path=key.slice(1);setPath(path,pval(v));if(path==="cardCfg.mode")S.dark=v==="dark";
    if(path.indexOf("sections.")===0){var sid=S.sections[+path.split(".")[1]].id;S.sel.layer="section";S.sel.section=sid;renderAll(true);show("card","sec:"+sid);return true}
    if(path.indexOf("blockDefs.")===0){renderAll(true);var b=S.card.filter(function(x){return x.kind===S.sel.blockDef})[0];show("card",b?"b:"+b.id:null);return true}
    renderAll(true);show("card");return true}});
ed.addEventListener("change",function(e){var t=e.target,k=t.dataset.sel;if(!k)return;
  if(k[0]==="@"){e.stopImmediatePropagation();setOpt(k,t.value);return}},true);
ed.addEventListener("input",function(e){var t=e.target;if(t.dataset.path){e.stopImmediatePropagation();setPath(t.dataset.path,t.value);renderPhone(true);refreshJsonV4();return}
  if(t.dataset.num&&t.dataset.num[0]==="@"){e.stopImmediatePropagation();var v=parseFloat(t.value);if(isNaN(v))return;setPath(t.dataset.num.slice(1),v);ed.querySelectorAll('[data-num="'+t.dataset.num+'"]').forEach(function(o){if(o!==t)o.value=v});renderPhone(true);refreshJsonV4()}},true);
ed.addEventListener("change",function(e){var t=e.target;if(t.dataset.num&&t.dataset.num[0]==="@"){e.stopImmediatePropagation();renderEditor();show("card")}},true);
/* the steps follow the new tab */
document.querySelectorAll("[data-step]").forEach(function(b){b.addEventListener("click",function(){var n=+b.dataset.step;
  S.op={s0:false,s1:n===0,s2:n===2,s3:n===1||n===2||n===3,s4:n===1,s5:false,s6:false,s3e:n===1,s3p:n===2,s3b:n===3,s4m:n===1,"mt:entrance":n===1,mcard:n===4,mpages:n===5,"pg:story":n===5,m0:n===4};
  if(n===1)S.op["mo:"+S.sel.motion]=true;if(n===3)S.op["bd:"+S.sel.blockDef]=true;
  renderEditor()},false)});
/* ----- Detail Card tab: blocks as accordions, feedback, dividers pool, Card Controls ----- */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  /* 3 > Blocks: one foldout per block, grouped by section, options inside */
  var bl=d.querySelector('[data-fo="s3b"] > .inner');if(bl){var html='<div class="note">A block = a part with behaviour, data or an owner. Each one opens to its defaults: its section, its Preview on the card and its Full Page. A marker places it and may override.</div>';
    S.sections.forEach(function(sec){var inner="";sec.kinds.forEach(function(k){if(!matchQ(k))return;var bd=S.blockDefs[k],q="blockDefs."+k+".";
      var body='<div class="sub2">Preview · on the card</div>'+row("Entrance",sel("@"+q+"enter",motionOpts("entrance",true),bd.enter),"How its preview comes into view.")+row("Tap",seg("@"+q+"tap",["page","expand","nothing"],["Open its page","Expand here","Nothing"],bd.tap),"What a tap on the preview does.")+
        (bd.tap==="page"?row("Open Row",seg("@"+q+"orow",["inherit","plain","nudge","shine"],["As in Card Controls ("+S.ctl.openrow+")","Plain","Nudge","Shine"],bd.orow),"The words inviting the tap, and their motion."):"")+'<div class="sub2">Full page</div>'+
        (bd.page?row("Page chrome",seg("@"+q+"chrome",["inherit","override"],["As in Card Controls","Override"],bd.chrome),"")+(bd.chrome==="override"?row("Breadcrumb",seg("@"+q+"crumb",["pages","off"],["On","Off"],bd.crumb),"For this block's page only.",true)+row("Previous / Next",seg("@"+q+"prevnext",["true","false"],["Shown","Hidden"],String(bd.prevnext)),"",true):"")+row("Starts with",'<span class="uses">'+(k==="Story"?"Chapter Opener (pattern)":"its own layout")+'</span>',"A new page starts with these rows."):'<div class="note">No page: everything shows on the card.</div>');
      inner+=fo("bd:"+k,'<span class="foth cv'+(S.dark?" dark":"")+'"><span class="tin" style="display:block">'+noBtn(libThumb(k)).replace(/class="thumb[^"]*"/,'class=""')+'</span></span>'+esc(k)+' <span class="sub">· '+(bd.tap==="page"?"preview + page":"on the card")+'</span>',null,body,9,S.sel.blockDef===k)});
      if(inner)html+=fo("bs:"+sec.id,'<span style="color:var(--'+sec.colour+')" class="cv">'+sicon(sec.icon,14)+'</span> '+esc(sec.name.en)+' <span class="sub">· '+sec.kinds.length+' blocks</span>',null,inner,3)});
    bl.innerHTML=html}
  /* 4 > Feedback (haptics and sound) */
  var me=d.querySelector('[data-fo="s4"] > .inner');if(me){var fb=row("Haptics",pseg("fb.haptics",["true","false"],["On","Off"]),"Small vibrations on taps and moments. The visitor can turn them off in the card's settings.")+
      row("Strength",pseg("fb.hStrength",["soft","medium","strong"],["Soft","Medium","Strong"]),"",true)+row("Sounds",pseg("fb.sound",["true","false"],["On","Off"]),"Short UI sounds. Off by default: a visitor in a museum may have the sound on. Never with the phone on silent.")+
      row("Volume",pslider("fb.volume",0,1,0.05),"",true)+'<table class="tbl"><thead><tr><th style="width:34%">Moment</th><th>Haptic</th><th>Sound</th><th style="width:44px"></th></tr></thead><tbody>'+
      S.fb.events.map(function(ev,i){return'<tr><td>'+esc(ev.ev)+'</td><td>'+sel("@fb.events."+i+".hap",HAPS,ev.hap)+'</td><td>'+sel("@fb.events."+i+".snd",SNDS,ev.snd)+'</td><td><button class="del" type="button" data-try="'+ev.id+'">Try</button></td></tr>'}).join("")+'</tbody></table>'+
      '<div class="tip">In the browser: sound through Web Audio; a vibration only on Android Chrome (the phone shows a ring where it would buzz). In Unity: <span class="badge bld">BUILD</span> haptics need a native call (iOS UIImpactFeedbackGenerator, Android VibrationEffect) or a package (Nice Vibrations: licence unverified); sounds are AudioSource one-shots <span class="badge nat">NATIVE</span>.</div>';
    me.insertAdjacentHTML("beforeend",fo("s4h","Feedback <span class=\"sub\">· haptics and sound</span>",["new","NEW"],fb,2))}
  /* 3 > Elements > Divider: the one pool lives in Effects */
  var dv=d.querySelector('[data-fo="el:divider"] > .inner');if(dv)dv.insertAdjacentHTML("afterbegin",'<div class="tip"><b>One pool of dividers.</b> Every divider artwork (edges and line dividers) lives in Motion &amp; Effects &gt; Effects. A Divider style here only picks one of them; a section\'s start picks one too. Add or import a new divider there and it is usable everywhere.</div>');
  /* 5: Card Controls */
  var s5=d.querySelector('[data-fo="s5"] > summary');if(s5)s5.innerHTML=s5.innerHTML.replace("Navigation &amp; Controls","Card Controls").replace("what the visitor uses to move around","the parts around the content: bar, top bar, page chrome, locate button, sticky action");
  return d.innerHTML});
/* ----- the demos of the rows this tab makes: Card, Sections, Feedback and Card Controls (the rows card/controls.js remakes have their
   demos there; a block's rows: card/blocks.js's builder) ----- */
function barRing(){var nb=document.getElementById("navbar");return nb&&nb.style.display!=="none"?"#navbar":".gsec"}
var CARDTAB_DEMOS={
  "cardCfg.layout":{stop:"full",wait:0,scroll:".lt-tabs|.lt-acc|.gsec",ring:".lt-tabs|.lt-acc|.gsec"},
  "secCfg.look":{stop:"full",wait:0,scroll:".gsec",ring:".gsec"},
  "secCfg.tint":{stop:"full",wait:0,scroll:'[data-ref="sec:stories"]',ring:'[data-ref="sec:stories"]'},
  "ctl.top.title":{stop:"full",past:true,wait:0,ring:".topband b|.topband"},
  "ctl.bar.on":{stop:"full",past:true,wait:0,ring:barRing},"ctl.bar.pos":{stop:"full",past:true,wait:0,ring:barRing},
  "ctl.bar.depth":{stop:"full",past:true,wait:0,ring:barRing},"ctl.bar.iconColour":{stop:"full",past:true,wait:0,ring:barRing},
  "ctl.jump.motion":{stop:"full",past:true,wait:0,act:function(){var b=ph.querySelector('#navbar [data-navsec="media"]');if(b)b.click()},ringAfter:700,ring:'[data-ref="sec:media"]'},
  "ctl.openrow":{block:"Story",wait:0,scroll:function(){return blockAt("Story").sel.split("|")[0]+" .open"},ring:function(){return blockAt("Story").sel.split("|")[0]+" .open"}},
  "ctl.crumb":{view:"story",wait:0,ring:".topbar span|.topbar"},
  "ctl.prevnext":{view:"story",wait:0,scroll:".pfoot|.pg",ring:".pfoot|.pg"},
  "ctl.locate.on":{stop:"half",wait:0,ring:".locpin|.hero"},"ctl.locate.colour":{stop:"half",wait:0,ring:".locpin|.hero"},
  "ctl.locate.size":{stop:"half",wait:0,ring:".locpin|.hero"},"ctl.locate.float":{stop:"half",wait:0,ring:".locpin|.hero"},
  "ctl.sticky":{stop:"half",wait:0,ring:".sticky|#navbar"}};
/* Feedback: a tap on a block buzzes (the ring where the phone would vibrate) and, with Sounds On, plays its sound */
["fb.haptics","fb.hStrength","fb.sound","fb.volume"].forEach(function(k){CARDTAB_DEMOS[k]={block:"Knowledge Check",wait:0,act:function(){feedback(k==="fb.sound"||k==="fb.volume"?"right":"tap")},ringAfter:60,ring:function(){return".hapring|"+blockAt("Knowledge Check").sel}}});
rowDemos(CARDTAB_DEMOS);
rowDemos(function(key){var m=/^blockDefs\.([^.]+)\.(enter|orow|chrome|crumb|prevnext)$/.exec(key);if(m&&S.blockDefs[m[1]])return blockRowDemo(m[1],m[2])});
