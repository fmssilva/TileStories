/* Detail Card's organisation: the card's and the sections' settings (S.cardCfg, S.secCfg, S.sections, S.ctl), the card screen, the Navigation Bar, the section icons. */
var SICON={info:'<circle cx="12" cy="12" r="9"/><path d="M12 11v6M12 7.5v.5"/>',book:'<path d="M4 5h6a2 2 0 0 1 2 2v12a2 2 0 0 0-2-2H4zM20 5h-6a2 2 0 0 0-2 2v12a2 2 0 0 1 2-2h6z"/>',
 image:'<rect x="3.5" y="4.5" width="17" height="15" rx="2"/><path d="M5 17l5-5 4 4 2-2 3 3"/><circle cx="15.5" cy="9" r="1.5"/>',headphones:'<path d="M4 15v-3a8 8 0 0 1 16 0v3"/><rect x="3.5" y="14" width="4" height="6" rx="1.5"/><rect x="16.5" y="14" width="4" height="6" rx="1.5"/>',
 play:'<circle cx="12" cy="12" r="9"/><path d="M10 8.5l6 3.5-6 3.5z"/>',pin:'<path d="M12 21s-6.5-6.4-6.5-11.2A6.5 6.5 0 0 1 18.5 9.8C18.5 14.6 12 21 12 21z"/><circle cx="12" cy="10" r="2.3"/>',
 star:'<path d="M12 3.5l2.6 5.4 5.9.8-4.3 4.1 1 5.8-5.2-2.8-5.2 2.8 1-5.8-4.3-4.1 5.9-.8z"/>',chat:'<path d="M4 5h16v10H9l-5 4z"/>',ar:'<path d="M12 3l8 4.5v9L12 21l-8-4.5v-9z"/><path d="M12 12l8-4.5M12 12v9M12 12L4 7.5"/>'};
function sicon(k,sz){return '<svg viewBox="0 0 24 24" width="'+(sz||18)+'" height="'+(sz||18)+'" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'+(SICON[k]||SICON.info)+'</svg>'}
var SCOLOURS=[["about","Terracotta"],["stories","Wine"],["media","Azulejo Blue"],["listen","River Green"],["play","Brick Red"],["visit","Indigo"],["accent2","Accent"]];
S.cardCfg={enabled:true,openAt:"half",halfMax:0.40,peekNext:false,tapOutside:true,mode:"light",layout:"scroll",topBand:"fade",audioOverlap:"fade",keepAudio:true};
S.secCfg={look:"open",start:"tiles",divider:"none",colour:"section",intensity:0.12,tint:120,order:"sections"};
S.sections=[
 {id:"about",name:T("About","Sobre"),icon:"info",colour:"about",shown:true,start:"inherit",kinds:["Header","Quick Facts","Status","Inline","Actions"]},
 {id:"stories",name:T("Stories","Histórias"),icon:"book",colour:"stories",shown:true,start:"inherit",kinds:["Story","Timeline","Compare Points","Dialogue"]},
 {id:"media",name:T("Media","Multimédia"),icon:"image",colour:"media",shown:true,start:"inherit",kinds:["Gallery","Before and After","Hotspot Image","Video","3D Model","360 Panorama"]},
 {id:"listen",name:T("Listen","Ouvir"),icon:"headphones",colour:"listen",shown:true,start:"inherit",kinds:["Audio Guide"]},
 {id:"play",name:T("Play","Jogar"),icon:"play",colour:"play",shown:true,start:"inherit",kinds:["Knowledge Check","Poll","Collect","Feedback","Place In AR"]},
 {id:"visit",name:T("Visit","Visitar"),icon:"pin",colour:"visit",shown:true,start:"inherit",kinds:["Wall Locator","Today Map","Related","Practical Info","Sources","Contents"]}];
S.ctl={bar:{on:true,pos:"bottom",indicator:"raised",depth:1,iconColour:"section",labels:"both",hide:false},jump:{motion:"smooth",opens:"full"},
 top:{title:true,settings:true},crumb:"pages",prevnext:true,openrow:"nudge",locate:{on:true,style:"pin_label",colour:"teal",size:1,float:true},sticky:"auto"};
S.poiSec={};S.texts=[["Open the story","Read the story","Ler a história"],["Close","Close","Fechar"],["Show on the wall","Find it on the wall","Encontre na parede"],["Minutes","min","min"]];
S.glossary=[[T("Torreão","Torreão"),T("The corner tower added to the river front in the 1580s.","A torre de canto acrescentada à frente ribeirinha na década de 1580.")],[T("Pombaline","Pombalino"),T("The rebuilding of Lisbon after 1755, under the Marquis of Pombal.","A reconstrução de Lisboa depois de 1755, com o Marquês de Pombal.")]];
S.sel.section="about";
function secById(id){return byId(S.sections,id)}
function secOfKind(kind){for(var i=0;i<S.sections.length;i++)if(S.sections[i].kinds.indexOf(kind)>=0)return S.sections[i].id;return S.sections[0].id}
function grpOf(b){return b.grp&&b.grp!=="inherit"?b.grp:secOfKind(b.kind)}
function eff(b,k){var v=b[k];if(v&&v!=="inherit")return v;var d=(S.blockDefs[b.kind]||{})[k];if(k==="orow"&&(!d||d==="inherit"))return S.ctl.openrow;return d}
function colVar(c){return "var(--"+c+")"}
function sectionStart(sec){var st=startOf(sec);if(S.secCfg.look!=="open"||st==="plain"||st==="fade")return "";
  var id=st==="tiles"?"e_tiles":st==="wave"?"e_wave":st==="arches"?"e_arches":st==="skyline"?"e_skyline":st,f=effectById(id),rv=S.secCfg.motion==="reveal"&&!S.reduce?" rv":"";
  if(st==="own")return '<div class="sstart'+rv+'" style="margin:0 -20px 10px">'+ownStrip()+'</div>';
  if(!f)return "";return '<div class="sstart'+rv+'" style="margin:0 -20px 10px">'+edgeSvg(Object.assign({},f,{o:Object.assign({},f.o,{colour:"family"})}))+'</div>'}
function cardRuns(){var vis=function(id){return shownOnCard(secById(id))};
  if(S.secCfg.order!=="sections"){var runs=[],list=S.card.slice();if(S.secCfg.order==="placed_sources")list=list.filter(function(b){return b.kind!=="Sources"}).concat(list.filter(function(b){return b.kind==="Sources"}));
    list.forEach(function(b){var g=grpOf(b);if(!vis(g))return;if(runs.length&&runs[runs.length-1].sec.id===g)runs[runs.length-1].blocks.push(b);else runs.push({sec:secById(g),blocks:[b]})});return pipe("cardRuns.runs",runs)}
  return pipe("cardRuns.runs",S.sections.filter(function(s){return vis(s.id)}).map(function(s){return{sec:s,blocks:S.card.filter(function(b){return grpOf(b)===s.id})}}))}
function cardScreen(){emitLatestFirst("cardScreen.before");var t=S.test.on&&S.test.poi==="painting",h='<div class="chrome"><i title="'+TXH("close")+'">&#10005;</i><i title="'+TXH("visitor_settings")+'">&#9776;</i></div><div class="hero" style="background-image:url('+IMG.hero+')"><div class="w"><small>'+esc(L(UI.where))+'</small><h2>Ribeira Palace</h2><p>'+esc(L(UI.line))+'</p></div></div>';
  cardRuns().forEach(function(run,si){var sec=run.sec,bl=run.blocks;if(!bl.length&&!(si===0&&t))return;
    var col=colVar(sec.colour),tint=S.secCfg.look==="open"&&S.secCfg.colour!=="neutral"?"linear-gradient(to bottom, color-mix(in srgb,"+(S.secCfg.colour==="accent"?"var(--accent2)":col)+" "+Math.round(S.secCfg.intensity*100)+"%, transparent) 0, transparent "+S.secCfg.tint+"px)":"none";
    h+='<div class="gsec item" data-ref="sec:'+sec.id+'" style="--fam:'+col+';--famtint:color-mix(in srgb,'+col+' 8%,transparent);background:'+tint+(S.secCfg.look==="cards"?";margin:12px;border-radius:18px;background:var(--box)":"")+'">'+sectionStart(sec)+'<div class="ey" style="color:'+(S.secCfg.colour==="neutral"?"var(--ink2)":S.secCfg.colour==="accent"?"var(--accent2)":col)+'">'+esc(L(sec.name))+'</div>';
    if(si===0&&t)h+=testTile();
    bl.forEach(function(b,i){if(i>0&&S.secCfg.divider!=="none"){var dv=effectById(S.secCfg.divider);if(dv)h+=lineSvg(dv)}h+=blockPreview(b,i)});h+='</div>'});
  return pipe("cardScreen.html",h+'<div style="height:60px"></div>');}
function navbar(){var c=S.ctl.bar,vis=S.sections.filter(shownOnBar),n=+c.more,act=vis.some(function(s){return s.id===S.activeSec})?S.activeSec:vis[0]&&vis[0].id,items=vis,more=false;
  if(vis.length>n){items=vis.slice(0,n-1);more=true}
  var h=items.map(function(s){var chosen=s.id===act,sc=colVar(s.colour),cc=c.chosenColour==="accent"?"var(--accent2)":c.chosenColour==="ink"?"var(--ink1)":sc,col=c.iconColour==="section"?sc:c.iconColour==="accent"?"var(--accent2)":"var(--ink2)";
    var icon=sicon(s.icon,18);if(chosen)icon=icon.replace('stroke-width="1.8"','stroke-width="2.5"');
    var disc=chosen&&c.indicator==="raised"&&c.chosenIcon==="filled"?'<em style="display:grid;place-items:center;width:'+(30+10*c.depth)+'px;height:'+(30+10*c.depth)+'px;border-radius:50%;background:'+cc+';color:#fff;margin-top:-'+(14*c.depth)+'px;box-shadow:0 4px 10px rgba(0,0,0,.2)">'+icon+'</em>':
      chosen&&c.indicator==="raised"?'<em style="display:grid;place-items:center;margin-top:-'+(14*c.depth)+'px;color:'+cc+'">'+icon+'</em>':chosen&&c.chosenIcon==="filled"&&c.indicator!=="none"?'<em style="display:grid;place-items:center;width:30px;height:30px;border-radius:50%;background:'+cc+';color:#fff">'+icon+'</em>':icon;
    var lab=c.labels==="icon"||(c.labels==="chosen"&&!chosen)?"":'<b style="font-weight:'+(chosen?800:600)+'">'+esc(L(s.name))+'</b>';
    return '<span data-navsec="'+s.id+'" aria-label="'+esc(L(s.name))+'" style="color:'+(chosen?cc:col)+';'+(chosen&&c.indicator==="underline"?"border-bottom:2px solid "+cc+";padding-bottom:3px":"")+(chosen&&c.indicator==="pill"?"background:color-mix(in srgb,"+cc+" 14%,transparent);border-radius:999px;padding:4px 8px":"")+'">'+(c.labels!=="text"?disc:"")+lab+'</span>'}).join("");
  if(more)h+='<span data-navmore="1" aria-label="More" style="color:var(--ink2)">'+(c.labels!=="text"?DOTS:"")+(c.labels==="both"||c.labels==="text"?"<b>"+TXH("nav_more")+"</b>":"")+'</span>';
  return h}
on("renderPhone.after",function(keep){var nb=document.getElementById("navbar"),c=S.ctl.bar;
  nb.style.display=S.view==="card"&&c.on?"":"none";nb.style.top=c.pos==="top"?"58px":"";nb.style.bottom=c.pos==="top"?"auto":"";nb.style.alignItems="center";nb.style.paddingBottom="0";
  var old=ph.querySelector(".locpin");if(old)old.remove();
  if(S.view==="card"&&S.ctl.locate.on){var L2=S.ctl.locate,sz=Math.round(40*L2.size),pin=document.createElement("div");pin.className="locpin";
    pin.style.cssText="position:absolute;right:18px;bottom:"+(c.on&&c.pos==="bottom"?92:22)+"px;z-index:7;display:flex;align-items:center;gap:8px";
    var hue={teal:["#7fe3c4","#0e9a7c"],green:["#b8ef7a","#3f9e2c"],cyan:["#8be4ff","#0a84b8"],accent:["#8FB0EC","#1F4E9C"]}[L2.colour]||["#7fe3c4","#0e9a7c"];
    pin.innerHTML=(L2.style==="pin_label"?'<span style="background:rgba(255,255,255,.95);color:#123b32;font:700 12px/1 var(--body);padding:8px 11px;border-radius:999px;box-shadow:0 3px 10px rgba(0,0,0,.18)">'+(TXH("locate_label"))+'</span>':"")+
      '<span class="'+(L2.float&&!S.reduce?"floatpin":"")+'" style="'+(L2.style==="glass"?"display:grid;place-items:center;width:"+(sz+14)+"px;height:"+(sz+14)+"px;border-radius:50%;background:rgba(255,255,255,.85);box-shadow:0 6px 16px rgba(0,0,0,.18)":"")+'">'+PIN.replace('<svg','<svg width="'+sz+'" height="'+Math.round(sz*1.33)+'"').replace(/#7fe3c4/g,hue[0]).replace(/#0e9a7c/g,hue[1])+'</span>';
    ph.appendChild(pin)}});

/* ----- the Detail Card tab, reorganised ----- */
function sectionTitle(n,t,sub){return n+" · "+t+(sub?' <span class="sub">· '+sub+'</span>':"")}
/* ----- the sections' events ----- */
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
S.activeSec="about";S.openSecs={about:true};
/* ----- card layouts: one scroll, tabs, pages, accordion ----- */
on("cardScreen.html",function(h){var lay=S.cardCfg.layout;if(lay==="scroll")return h;
  var d=document.createElement("div");d.innerHTML=h;var secs=d.querySelectorAll(".gsec");if(!secs.length)return h;
  var ids=Array.prototype.map.call(secs,function(x){return x.dataset.ref.slice(4)});if(ids.indexOf(S.activeSec)<0)S.activeSec=ids[0];
  if(lay==="tabs"||lay==="pages"){var strip=lay==="tabs"?'<div class="lt-tabs">'+ids.map(function(id){var s=secById(id);return'<button type="button" data-tabsec="'+id+'" aria-pressed="'+(S.activeSec===id)+'" style="color:'+colVar(s.colour)+'">'+esc(L(s.name))+'</button>'}).join("")+'</div>':"";
    secs.forEach(function(x){if(x.dataset.ref!=="sec:"+S.activeSec)x.remove()});
    var keep=d.querySelector(".gsec");if(keep){keep.insertAdjacentHTML("beforebegin",strip);
      if(lay==="pages"){var i=ids.indexOf(S.activeSec);keep.insertAdjacentHTML("afterend",'<div class="lt-pages"><button type="button" data-pagesec="'+ids[(i-1+ids.length)%ids.length]+'" aria-label="'+TXH("nav_pager_previous")+'">&#8249;</button><span>'+ids.map(function(id){return'<i class="'+(id===S.activeSec?"on":"")+'"></i>'}).join("")+'</span><button type="button" data-pagesec="'+ids[(i+1)%ids.length]+'" aria-label="'+TXH("nav_pager_next")+'">&#8250;</button></div>')}}}
  if(lay==="accordion")secs.forEach(function(x){var id=x.dataset.ref.slice(4),open=!!S.openSecs[id],ey=x.querySelector(".ey");
    var head=document.createElement("button");head.type="button";head.className="lt-acc";head.dataset.accsec=id;head.setAttribute("aria-expanded",String(open));head.innerHTML=ey?ey.outerHTML+'<em>'+(open?"&#8722;":"+")+'</em>':id;if(ey)ey.remove();
    x.insertBefore(head,x.firstChild);if(!open)Array.prototype.slice.call(x.children,1).forEach(function(c){c.style.display="none"})});
  return d.innerHTML});
