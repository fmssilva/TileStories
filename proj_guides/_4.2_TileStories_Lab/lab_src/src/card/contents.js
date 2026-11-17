/* The Contents sheet and each block's reading time (contentsSheet, minsOf). */
setOverlay("contents",false);
/* a tap on the bar: the merged jump rule, and More (registered on the document so it runs before the older handlers) */
document.addEventListener("click",function(e){var t=e.target;if(!ph.contains(t)||ui.view!=="card")return;
  if(t.closest("#navbar [data-navmore]")){e.stopPropagation();setOverlay("contents",true);renderPhone(true);return}
  var it=t.closest("#navbar [data-navsec]");if(!it)return;e.stopPropagation();var id=it.dataset.navsec;feedback("jump");ui.activeSec=id;ui.openSecs[id]=true;
  if(D.card.ctl.jump.opens==="full")sheetTo("full");renderPhone(true);
  var el=scr.querySelector('[data-ref="sec:'+id+'"]');if(el)scr.scrollTo({top:el.offsetTop-60,behavior:D.card.ctl.jump.motion==="smooth"&&!P.reduce?"smooth":"auto"})},true);
function contentsSheet(){var c=D.card.ctl.contents,runs=cardRuns();if(!c.on)return "";
  var chips=c.chips?'<div class="cchips"><button type="button">'+esc(fmt(TXT("nav_facet_up_to"),2))+'</button><button type="button">'+esc(fmt(TXT("nav_facet_up_to"),5))+'</button><button type="button">'+TXH("nav_facet_no_headphones")+'</button><button type="button">'+TXH("nav_facet_no_camera")+'</button><button type="button">'+TXH("nav_facet_no_network")+'</button><button type="button">'+TXH("nav_facet_clear")+'</button></div>':"";
  var tick=function(read){return c.ticks?'<i class="ctick'+(read?" on":"")+'" aria-label="'+(read?TXH("nav_contents_read"):"")+'">'+(read?"&#10003;":"")+'</i>':""};
  var rows=c.rows==="sections"?runs.map(function(r,k){return'<div class="crow sec"><i style="color:'+colVar(r.sec.colour)+'">'+sicon(r.sec.icon,16)+'</i><b>'+esc(L(r.sec.name))+'</b><span>'+r.blocks.length+'</span>'+tick(k===0)+'</div>'+r.blocks.map(function(b,j){return'<div class="crow sub">'+esc(b.kind)+tick(k===0&&j===0)+'</div>'}).join("")}).join(""):
    D.poi.card.map(function(b,j){var s=secById(grpOf(b));return'<div class="crow"><i style="color:'+colVar(s.colour)+'">'+sicon(s.icon,14)+'</i>'+esc(b.kind)+tick(j<2)+'</div>'}).join("");
  return pipe("contentsSheet.html",'<div class="csheet" role="dialog"><div class="vh"><b>'+TXH("nav_menu")+'</b><button type="button" data-csheet-close="1" aria-label="'+TXH("close")+'">&#10005;</button></div>'+chips+rows+'</div>')}
/* reading time: the Contents sheet shows each block's minutes */
var KMIN={"Story":4,"Gallery":2,"Video":1,"Audio Guide":3,"Timeline":2,"Dialogue":2,"Knowledge Check":2};function minsOf(b){return b.minsMode==="hand"?b.mins:(KMIN[b.kind]||1)}
on("contentsSheet.html",function(h){var d=document.createElement("div");d.innerHTML=h;var subs=d.querySelectorAll(".crow.sub, .crow:not(.sec)"),list=D.poi.card.filter(function(b){return(D.card.blockDefs[b.kind]||{}).offered!==false});
  if(D.card.ctl.contents.rows==="sections"){var k=0;cardRuns().forEach(function(r){r.blocks.forEach(function(b){var el=d.querySelectorAll(".crow.sub")[k++];if(el)el.insertAdjacentHTML("beforeend",'<span class="cmin">'+esc(fmt(TXT("nav_contents_minutes"),minsOf(b)))+'</span>')})})}
  else d.querySelectorAll(".crow").forEach(function(el,j){var b=D.poi.card[j];if(b)el.insertAdjacentHTML("beforeend",'<span class="cmin">'+esc(fmt(TXT("nav_contents_minutes"),minsOf(b)))+'</span>')});return d.innerHTML});
