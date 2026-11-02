/* ===================== v5: the whole flow, feedback, card settings live, self-test ===================== */
(function(){var i=S.card.findIndex(function(b){return b.id==="b_audio"});if(i>0){var a=S.card.splice(i,1)[0];S.card.splice(1,0,a)}})();
S.stop="closed";S.activeSec="about";S.openSecs={about:true};
S.fb={haptics:true,hStrength:"medium",sound:false,volume:0.5,events:[
 {id:"open",ev:"The card opens",hap:"light",snd:"whoosh"},{id:"tap",ev:"A tap on a block",hap:"light",snd:"none"},{id:"jump",ev:"A jump to a section",hap:"selection",snd:"tick"},
 {id:"slide",ev:"A slider or scrub moves",hap:"selection",snd:"none"},{id:"right",ev:"A right answer",hap:"success",snd:"chime"},{id:"wrong",ev:"A wrong answer",hap:"warning",snd:"buzz"},
 {id:"collect",ev:"A stamp collected",hap:"heavy",snd:"stamp"},{id:"locate",ev:"Show me where it is",hap:"medium",snd:"ping"}]};
var HAPS=[["none","None"],["selection","Selection tick"],["light","Light"],["medium","Medium"],["heavy","Heavy"],["success","Success"],["warning","Warning"]];
var SNDS=[["none","None"],["tick","Tick"],["whoosh","Whoosh"],["chime","Chime"],["buzz","Soft buzz"],["stamp","Stamp"],["ping","Ping"]];
var actx=null;
function tone(f,t0,d,type,g){var o=actx.createOscillator(),v=actx.createGain();o.type=type||"sine";o.frequency.setValueAtTime(f,actx.currentTime+t0);v.gain.setValueAtTime(0,actx.currentTime+t0);v.gain.linearRampToValueAtTime(g*S.fb.volume,actx.currentTime+t0+0.01);v.gain.exponentialRampToValueAtTime(0.0001,actx.currentTime+t0+d);o.connect(v);v.connect(actx.destination);o.start(actx.currentTime+t0);o.stop(actx.currentTime+t0+d+0.02)}
function playSound(k){if(!S.fb.sound||k==="none")return;try{actx=actx||new (window.AudioContext||window.webkitAudioContext)();
  if(k==="tick")tone(1800,0,.04,"square",.08);if(k==="chime"){tone(880,0,.5,"sine",.25);tone(1318,.09,.6,"sine",.2)}if(k==="ping")tone(1568,0,.7,"sine",.25);
  if(k==="buzz")tone(140,0,.18,"sawtooth",.12);if(k==="stamp"){tone(90,0,.18,"sine",.45);tone(180,0,.08,"square",.08)}
  if(k==="whoosh"){var n=actx.createBufferSource(),b=actx.createBuffer(1,actx.sampleRate*.35,actx.sampleRate),d=b.getChannelData(0);for(var i=0;i<d.length;i++)d[i]=(Math.random()*2-1)*Math.sin(Math.PI*i/d.length);n.buffer=b;var fl=actx.createBiquadFilter();fl.type="bandpass";fl.frequency.value=900;var g=actx.createGain();g.gain.value=.25*S.fb.volume;n.connect(fl);fl.connect(g);g.connect(actx.destination);n.start()}}catch(e){}}
var HAPMS={selection:[8],light:[12],medium:[22],heavy:[40],success:[14,60,22],warning:[30,50,30]};
function buzz(k){if(!S.fb.haptics||k==="none")return;var p=HAPMS[k]||[10];if(S.fb.hStrength==="soft")p=p.map(function(x){return Math.round(x*.6)});if(S.fb.hStrength==="strong")p=p.map(function(x){return Math.round(x*1.5)});
  try{if(navigator.vibrate)navigator.vibrate(p)}catch(e){}var r=document.createElement("div");r.className="hapring hap-"+k;r.textContent=k==="selection"?"tick":k;ph.appendChild(r);setTimeout(function(){r.remove()},700)}
function feedback(id){var e=byId(S.fb.events,id);if(!e)return;buzz(e.hap);playSound(e.snd)}

/* ----- the wall, the marker and the sheet ----- */
function stopTop(st){var H=SCREENS[S.screen][1];if(S.screen==="land")return 0;if(st==="peek")return H-200;if(st==="half")return Math.round(H*(1-S.cardCfg.halfMax));if(st==="full")return 0;return H+10}
function wallLayer(){var w=document.getElementById("wall");if(!w){w=document.createElement("div");w.id="wall";w.className="wall";ph.insertBefore(w,ph.firstChild.nextSibling)}
  w.innerHTML='<button class="mk" type="button" data-marker="painting" style="left:52%;top:30%" aria-label="Ribeira Palace"><span>'+sicon("info",20)+'</span><b>Ribeira Palace</b></button><button class="mk dim" type="button" style="left:22%;top:58%" aria-label="Lisbon Cathedral"><span>'+sicon("star",18)+'</span><b>Lisbon Cathedral</b></button>'+
   (S.stop==="closed"?'<div class="walltip">Tap a marker to open its card</div>':"");
  return w}
var renderPhoneV4=renderPhone;
renderPhone=function(keep){renderPhoneV4(keep);var inWall=S.view==="card";
  document.getElementById("camera").style.backgroundImage=inWall||S.screen==="land"?"url("+IMG.g1+")":"none";
  var w=wallLayer();w.style.display=inWall?"":"none";
  var top=inWall?stopTop(S.stop):0;scr.style.top=top+"px";scr.style.borderRadius=inWall&&S.stop!=="full"?"22px 22px 0 0":"";scr.style.transition="top .35s cubic-bezier(.2,.8,.2,1)";
  scr.style.overflowY=inWall&&S.stop==="peek"?"hidden":"auto";
  var gb=document.getElementById("grab");if(!gb){gb=document.createElement("div");gb.id="grab";gb.className="grab";ph.appendChild(gb);initGrab(gb)}
  gb.style.display=inWall&&S.stop!=="closed"&&S.screen!=="land"?"":"none";gb.style.top=(top+2)+"px";
  var nb=document.getElementById("navbar");if(inWall&&(S.stop==="closed"||S.stop==="peek"))nb.style.display="none";
  var lp=ph.querySelector(".locpin");if(lp&&inWall&&S.stop==="closed")lp.remove();
  var pk=ph.querySelector(".peeknext");if(pk)pk.remove();
  if(inWall&&S.stop==="peek"&&S.cardCfg.peekNext){var p=document.createElement("div");p.className="peeknext";p.textContent=(S.lang==="pt"?"Seguinte: ":"Next: ")+"Lisbon Cathedral ↑";ph.appendChild(p)}
  topBand();stickyAction()};
function initGrab(gb){gb.addEventListener("pointerdown",function(e){gb.setPointerCapture(e.pointerId);var y0=e.clientY,t0=parseFloat(scr.style.top)||0,k=fit.getBoundingClientRect().height/SCREENS[S.screen][1];scr.style.transition="none";
  function mv(ev){var t=Math.max(0,t0+(ev.clientY-y0)/k);scr.style.top=t+"px";gb.style.top=(t+2)+"px"}
  function up(ev){gb.removeEventListener("pointermove",mv);gb.removeEventListener("pointerup",up);var t=parseFloat(scr.style.top),best="half",bd=1e9;["peek","half","full"].forEach(function(s){var d=Math.abs(stopTop(s)-t);if(d<bd){bd=d;best=s}});
    if(t>stopTop("peek")+80)best="closed";S.stop=best;renderPhone(true)}
  gb.addEventListener("pointermove",mv);gb.addEventListener("pointerup",up)})}
function topBand(){var b=ph.querySelector(".topband");if(!b){b=document.createElement("div");b.className="topband";ph.appendChild(b)}
  var show=S.view==="card"&&S.stop==="full"&&scr.scrollTop>230;b.style.display=show?"":"none";b.className="topband tb-"+S.cardCfg.topBand;
  b.innerHTML='<i>&#10005;</i>'+(S.ctl.top.title?'<b>Ribeira Palace</b>':'<b></b>')+(S.ctl.top.settings?'<i>&#9776;</i>':'<i style="visibility:hidden"></i>')}
function stickyAction(){var s=ph.querySelector(".sticky");if(s)s.remove();var on=S.ctl.sticky==="shown"||(S.ctl.sticky==="auto"&&!(S.ctl.bar.on&&S.ctl.bar.pos==="bottom"));
  if(S.view==="card"&&on&&(S.stop==="half"||S.stop==="full")){s=document.createElement("div");s.className="sticky";s.innerHTML='<button type="button" data-locate="1">'+(S.lang==="pt"?"Mostrar onde fica":"Show me where it is")+'</button>';ph.appendChild(s)}}
scr.addEventListener("scroll",function(){if(S.view==="card")topBand()},{passive:true});

/* ----- card layouts: one scroll, tabs, pages, accordion ----- */
var cardScreenV4=cardScreen;
cardScreen=function(){var lay=S.cardCfg.layout;if(lay==="scroll")return cardScreenV4();
  var h=cardScreenV4(),d=document.createElement("div");d.innerHTML=h;var secs=d.querySelectorAll(".gsec");if(!secs.length)return h;
  var ids=Array.prototype.map.call(secs,function(x){return x.dataset.ref.slice(4)});if(ids.indexOf(S.activeSec)<0)S.activeSec=ids[0];
  if(lay==="tabs"||lay==="pages"){var strip=lay==="tabs"?'<div class="lt-tabs">'+ids.map(function(id){var s=secById(id);return'<button type="button" data-tabsec="'+id+'" aria-pressed="'+(S.activeSec===id)+'" style="color:'+colVar(s.colour)+'">'+esc(L(s.name))+'</button>'}).join("")+'</div>':"";
    secs.forEach(function(x){if(x.dataset.ref!=="sec:"+S.activeSec)x.remove()});
    var keep=d.querySelector(".gsec");if(keep){keep.insertAdjacentHTML("beforebegin",strip);
      if(lay==="pages"){var i=ids.indexOf(S.activeSec);keep.insertAdjacentHTML("afterend",'<div class="lt-pages"><button type="button" data-pagesec="'+ids[(i-1+ids.length)%ids.length]+'">&#8249;</button><span>'+ids.map(function(id){return'<i class="'+(id===S.activeSec?"on":"")+'"></i>'}).join("")+'</span><button type="button" data-pagesec="'+ids[(i+1)%ids.length]+'">&#8250;</button></div>')}}}
  if(lay==="accordion")secs.forEach(function(x){var id=x.dataset.ref.slice(4),open=!!S.openSecs[id],ey=x.querySelector(".ey");
    var head=document.createElement("button");head.type="button";head.className="lt-acc";head.dataset.accsec=id;head.setAttribute("aria-expanded",String(open));head.innerHTML=ey?ey.outerHTML+'<em>'+(open?"&#8722;":"+")+'</em>':id;if(ey)ey.remove();
    x.insertBefore(head,x.firstChild);if(!open)Array.prototype.slice.call(x.children,1).forEach(function(c){c.style.display="none"})});
  return d.innerHTML};

/* ----- page chrome on pages: breadcrumb, previous / next ----- */
var topbarV3=topbar;
function chromeOf(k){var bd=S.blockDefs[k]||{};return bd.chrome==="override"?{crumb:bd.crumb,prevnext:bd.prevnext}:{crumb:S.ctl.crumb,prevnext:S.ctl.prevnext}}
topbar=function(crumb){if((S._pk?chromeOf(S._pk).crumb:S.ctl.crumb)==="off")return '<div class="topbar"><button type="button" data-go="card" aria-label="Back">&#8249;</button><span></span></div>';return topbarV3(crumb)};
var storyScreenV3=storyScreen,galleryScreenV3=galleryScreen;
function pageFoot(prev,next){if(!(S._pk?chromeOf(S._pk).prevnext:S.ctl.prevnext))return "";return '<div class="pfoot"><button type="button" data-go="'+prev[0]+'"><small>'+(S.lang==="pt"?"Anterior":"Previous")+'</small>'+esc(prev[1])+'</button><button type="button" data-go="'+next[0]+'"><small>'+(S.lang==="pt"?"Seguinte":"Next")+'</small>'+esc(next[1])+'</button></div>'}
storyScreen=function(){S._pk="Story";var h=storyScreenV3()+pageFoot(["card",L(UI.about)],["gallery","Gallery"]);S._pk=null;return h};
galleryScreen=function(){S._pk="Gallery";var h=galleryScreenV3()+pageFoot(["story",L(DEMO.title)],["card",L(UI.visit)]);S._pk=null;return h};

/* ----- phone events ----- */
ph.addEventListener("click",function(e){var t=e.target;
  var mk=t.closest("[data-marker]");if(mk){S.stop=S.cardCfg.openAt;S.view="card";feedback("open");renderPhone(false);return}
  if(S.view==="card"&&t===ph.querySelector("#camera")||t.closest("#wall")&&!t.closest(".mk")){if(S.cardCfg.tapOutside&&S.stop!=="closed"){S.stop="closed";renderPhone(false)}return}
  var ch=t.closest(".chrome i");if(ch&&S.view==="card"&&ch.textContent.charCodeAt(0)===10005){S.stop="closed";renderPhone(false);return}
  var lt=t.closest("[data-tabsec],[data-pagesec]");if(lt){S.activeSec=lt.dataset.tabsec||lt.dataset.pagesec;feedback("jump");renderPhone(true);return}
  var ac=t.closest("[data-accsec]");if(ac){var id=ac.dataset.accsec;S.openSecs[id]=!S.openSecs[id];feedback("tap");renderPhone(true);return}
  var nb=t.closest("#navbar span");if(nb&&S.view==="card"){var idx=Array.prototype.indexOf.call(nb.parentNode.children,nb),vis=S.sections.filter(function(s){return s.shown&&S.poiSec[s.id]!=="hide"}),sec=vis[idx];if(!sec)return;
    feedback("jump");S.activeSec=sec.id;S.openSecs[sec.id]=true;if(S.stop!=="full"&&S.ctl.jump.opens==="full")S.stop="full";else if(S.stop==="peek")S.stop="half";renderPhone(true);
    var el=scr.querySelector('[data-ref="sec:'+sec.id+'"]');if(el)scr.scrollTo({top:el.offsetTop-60,behavior:S.ctl.jump.motion==="smooth"&&!S.reduce?"smooth":"auto"});return}
  if(t.closest(".locpin")||t.closest("[data-locate]")){feedback("locate");S.stop="peek";renderPhone(false);var m=ph.querySelector('[data-marker="painting"]');if(m){m.classList.remove("found");void m.offsetWidth;m.classList.add("found")}return}
  var kq=t.closest(".kq button");if(kq)feedback(kq.dataset.ok?"right":"wrong");
  if(t.closest(".bprev"))feedback("tap")},true);

/* ----- Detail Card tab: blocks as accordions, feedback, dividers pool, Card Controls ----- */
var edCardV4=edCard;
edCard=function(){var h=edCardV4(),d=document.createElement("div");d.innerHTML=h;
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
  return d.innerHTML};
ed.addEventListener("click",function(e){var tr=e.target.closest("[data-try]");if(tr){e.stopPropagation();feedback(tr.dataset.try)}},true);


/* the part a control edits = the foldout it sits in (fixes editing a second open motion / effect / interaction) */
function setCtx(el){var d=el.closest("details[data-fo]");while(d){var f=d.dataset.fo,k=f.split(":");
  if(k[0]==="mo"){S.sel.motion=k[1];return}if(k[0]==="fx"){S.sel.effect=k[1];return}if(k[0]==="in"){S.sel.inter=k[1];return}if(k[0]==="bd"){S.sel.blockDef=k[1];return}
  d=d.parentElement&&d.parentElement.closest("details[data-fo]")}}
["click","input","change"].forEach(function(t){ed.addEventListener(t,function(e){if(e.target.closest("[data-set],[data-num],[data-sel],[data-text]"))setCtx(e.target)},true)});
function curOf(key){var p=key.split("."),st=styleById(S.sel.style);
  if(key[0]==="@")return getPath(key.slice(1));
  if(p[0]==="motion")return(motionById(S.sel.motion)||{})[p[1]];if(p[0]==="o")return st&&st.o[p[1]];if(p[0]==="fx")return(effectById(S.sel.effect)||{o:{}}).o[p[1]];
  if(p[0]==="inter")return(interById(S.sel.inter)||{o:{}}).o[p[1]];if(p[0]==="cb")return(byId(S.card,S.sel.cblock)||{})[p[1]];
  if(key==="style.uses")return st&&st.o.uses;if(p[0]==="style")return st&&st[p[1]];if(p[0]==="prow")return(patternById(p[1])||{rows:[]}).rows[+p[2]]&&patternById(p[1]).rows[+p[2]].style;
  return{family:S.family,replay:S.replay,focusOn:S.focusOn,previewLang:S.lang,"test.poi":S.test.poi,"m.poi":"painting"}[key]}

var edMarkerV4=edMarker;
edMarker=function(){var h=edMarkerV4();if(S.sel.page!=="gallery")return h;var d=document.createElement("div");d.innerHTML=h;var pg=d.querySelector('[data-fo="mpages"] > .inner');
  if(pg){var imgs=S.galleryImgs||["g1","g2","g3","before"];pg.innerHTML=row("Page of",sel("m.page",[["story","Story · "+L(DEMO.title)],["gallery","Gallery"]],"gallery"),"Each block with a page has its content here.")+
    fo("pg:gallery","Gallery · its pictures",["ext","EXTENDED"],row("Pictures",'<div class="assets">'+ASSETS.map(function(a){return'<button type="button" data-gal="'+a+'" aria-pressed="'+(imgs.indexOf(a)>=0)+'" style="background-image:url('+IMG[a]+')" aria-label="'+a+'"></button>'}).join("")+'</div>',"Tap to add or remove; the order is the tap order."),2)}
  return d.innerHTML};
ed.addEventListener("change",function(e){var t=e.target;if(t.dataset.sel==="m.page"){e.stopImmediatePropagation();S.sel.page=t.value;renderEditor();show(t.value==="gallery"?"gallery":"story")}},true);
ed.addEventListener("click",function(e){var g=e.target.closest("[data-gal]");if(!g)return;e.stopPropagation();var a=S.galleryImgs||["g1","g2","g3","before"],i=a.indexOf(g.dataset.gal);if(i>=0)a.splice(i,1);else a.push(g.dataset.gal);S.galleryImgs=a;renderEditor();S.view="gallery";renderPhone(false)},true);

/* ----- self-test: every Editor control must change what the phone shows ----- */
var BEHAVIOUR=/^@?(inter\.|m\.page|blockDefs\.[^.]+\.chrome|cardCfg\.(tapOutside|keepAudio|audioOverlap|peekNext|enabled)|fb\.|ctl\.jump\.|ctl\.bar\.hide|ui\.|test\.poi|focusOn|previewLang)/;
function modelSig(){var keep=S.view,out="";["card","demo","story","gallery"].forEach(function(v){S.view=v;out+=(v==="card"?cardScreen()+navbar():v==="demo"?demoScreen():v==="story"?storyScreen():galleryScreen())+"#"});S.view=keep;
  return out+JSON.stringify({loc:S.ctl.locate,sticky:S.ctl.sticky,bar:[S.ctl.bar.on,S.ctl.bar.pos],openAt:stopTop(S.cardCfg.openAt),half:S.cardCfg.halfMax,band:S.cardCfg.topBand,top:S.ctl.top,dark:S.dark,lang:S.lang,peek:S.cardCfg.peekNext,on:S.cardCfg.enabled})}
function selfTest(){var res=[],seen={},real={renderEditor:renderEditor,renderPhone:renderPhone,renderAll:renderAll,show:show,playRef:playRef,flashRef:flashRef};
  S.ui.level=9;S.op={};S.ui.single=false;
  ["card","marker"].forEach(function(tab){S.tab=tab;real.renderEditor();var base=JSON.stringify(S);
    renderEditor=function(){};renderPhone=function(){};renderAll=function(){};show=function(v){S.view=v};playRef=function(){};flashRef=function(){};
    var ctrls=[];ed.querySelectorAll("[data-set]").forEach(function(b){ctrls.push({el:b,kind:"set",key:b.dataset.set,v:b.dataset.v})});
    ed.querySelectorAll("select[data-sel]").forEach(function(x){Array.prototype.forEach.call(x.options,function(o){ctrls.push({el:x,kind:"sel",key:x.dataset.sel,v:o.value})})});
    ed.querySelectorAll('input[type=range][data-num]').forEach(function(r){ctrls.push({el:r,kind:"num",key:r.dataset.num,v:r.min});ctrls.push({el:r,kind:"num",key:r.dataset.num,v:r.max})});
    ctrls.forEach(function(c){var id=tab+"|"+c.key+"|"+c.v;if(seen[id])return;seen[id]=1;if(/^(ui\.|test\.on|@test\.on)/.test(c.key))return;
      var B=JSON.parse(base);Object.keys(B).forEach(function(k){S[k]=B[k]});setCtx(c.el);if(c.kind!=="num"&&String(curOf(c.key))===String(c.v))return;var lab=labelOf(c.el),before=modelSig(),state=JSON.stringify(S);
      try{if(c.kind==="set")c.el.click();else if(c.kind==="sel"){c.el.value=c.v;c.el.dispatchEvent(new Event("change",{bubbles:true}))}else{c.el.value=c.v;c.el.dispatchEvent(new Event("input",{bubbles:true}));c.el.dispatchEvent(new Event("change",{bubbles:true}))}}
      catch(err){res.push({tab:tab,key:c.key,v:c.v,label:lab,result:"ERROR "+err.message});return}
      var stateChanged=JSON.stringify(S)!==state,after=modelSig();
      var offCard=/^@blockDefs\./.test(c.key)&&!S.card.some(function(b){return b.kind===c.key.split(".")[1]});res.push({tab:tab,key:c.key,v:c.v,label:lab,result:after!==before?"changes the phone":!stateChanged?"NOT WIRED (state unchanged)":BEHAVIOUR.test(c.key)?"behaviour (no picture)":offCard?"behaviour (block not on this card)":"NO EFFECT (state changes, phone does not)"})});
    renderEditor=real.renderEditor;renderPhone=real.renderPhone;renderAll=real.renderAll;show=real.show;playRef=real.playRef;flashRef=real.flashRef});
  return res}
function cssq(s){return String(s).replace(/"/g,'\\"')}
function labelOf(el){var r=el.closest(".row");return r?(r.querySelector("label")||{}).textContent||"":""}
function showSelfTest(res){var works={};res.forEach(function(r){if(r.result==="changes the phone")works[r.tab+"|"+r.key]=1});res.forEach(function(r){if(/NO EFFECT/.test(r.result)&&works[r.tab+"|"+r.key])r.result="same look as now (other values of this row change the phone)"});var bad=res.filter(function(r){return /NO EFFECT|NOT WIRED|ERROR/.test(r.result)}),ok=res.filter(function(r){return r.result==="changes the phone"}).length,beh=res.filter(function(r){return/behaviour/.test(r.result)}).length;
  var box=document.getElementById("selftest");box.innerHTML='<h2>Self-test</h2><p><b>'+ok+'</b> option values change the phone · <b>'+beh+'</b> are behaviour (sound, taps, timing) · <b style="color:#C0392B">'+bad.length+'</b> do nothing or were not found.</p>'+
   (bad.length?'<table><tr><th>Tab</th><th>Row</th><th>Key</th><th>Value</th><th>Result</th></tr>'+bad.map(function(r){return'<tr><td>'+r.tab+'</td><td>'+esc(r.label)+'</td><td><code>'+esc(r.key)+'</code></td><td>'+esc(r.v)+'</td><td>'+r.result+'</td></tr>'}).join("")+'</table>':"<p>Every option shows on the phone.</p>");
  box.dataset.json=JSON.stringify({ok:ok,behaviour:beh,bad:bad})}
document.getElementById("runtest").addEventListener("click",function(){var b=this;b.disabled=true;b.textContent="Running…";setTimeout(function(){var snap=JSON.stringify(S);var r=selfTest();showSelfTest(r);b.disabled=false;b.textContent="Run the self-test again";
  var o=JSON.parse(snap);Object.keys(o).forEach(function(k){S[k]=o[k]});S.tab="card";S.view="card";renderAll(false)},30)});

