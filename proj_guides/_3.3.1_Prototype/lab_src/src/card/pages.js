/* Each block's full page (kpage) and the picture viewer. */
/* ----- pages: every kind with a page; page chips; the picture viewer; the video player ----- */
S.pageKind="Timeline";S.viewerAt=0;
function pageChips(kind){if(!S.ctl.pageChips)return "";var sid=secOfKind(kind),sib=S.card.concat(S.demoCard).filter(function(b,i,a){return secOfKind(b.kind)===sid&&a.findIndex(function(x){return x.kind===b.kind})===i});
  return '<div class="pchips">'+sib.map(function(b){return'<button type="button" data-go="'+(b.kind==="Story"?"story":b.kind==="Gallery"?"gallery":"kpage")+'" data-kind="'+esc(b.kind)+'" aria-pressed="'+(b.kind===kind)+'">'+sicon(secById(sid).icon,13)+esc(b.kind)+'</button>'}).join("")+'</div>'}
function kpageBody(k){var o=function(key){return kopt(k,key)};
  if(k==="Timeline")return '<div class="tl">'+[["c. 1500","The court moves to the river"],["1580s","The Torreão rises"],["1755","Earthquake, fire and tsunami"],["1758","The Pombaline plan"]].map(function(e){return'<div><b>'+e[0]+'</b><span>'+e[1]+'</span></div>'}).join("")+(o("now")==="true"?'<div><b>'+TXH("timeline_now")+'</b><span>The Praça do Comércio</span></div>':"")+'</div>';
  if(k==="Dialogue")return KPREV.Dialogue()+'<div class="dlg"><p class="them">'+esc(L(T("It rises at the corner, over the water.","Ergue-se no canto, sobre a água.")))+'</p></div><div class="dopts"><button type="button">'+TXH("dialogue_again")+'</button></div>';
  if(k==="Video"){var cap=o("captions")==="inherit"?S.ctl.player.captions:o("captions")==="true";return '<div class="vplayer"><img alt="" src="'+IMG.hero+'"><i>&#9654;</i>'+(cap?'<span class="vcap">'+esc(L(T("From the square, the road climbs to the castle.","Da praça, a rua sobe até ao castelo.")))+'</span>':"")+'<div class="vbar"><span>0:12 / 0:55</span><button type="button" aria-pressed="'+cap+'">'+TXH("player_captions")+'</button><button type="button">'+TXH("video_full_screen")+'</button></div></div><div class="achap"><small>'+TXH("video_chapters")+'</small><button type="button">0:00 The square</button><button type="button">0:30 The castle</button></div><button type="button" class="ubtn2">'+TXH("transcript")+'</button>'}
  if(k==="3D Model")return renderEl("model_3d",kstyle(k,"model_3d"),{},0,false)+(o("room")==="true"?'<button type="button" class="arfocal">'+TXH("model_in_room")+'</button>':"");
  if(k==="360 Panorama")return renderEl("panorama",kstyle(k,"panorama"),{},0,false);
  if(k==="Knowledge Check")return KPREV["Knowledge Check"]()+'<div class="kq"><b>'+esc(L(T("When did the opera house open?","Quando abriu a ópera?")))+'</b><button type="button" data-ok="1">1755</button><button type="button">1700</button></div>'+(o("score")==="end"?'<div class="kscore">'+esc(fmt(TXT("quiz_score"),2,3))+'</div>':"")+(o("retry")==="true"?'<button type="button" class="ubtn2">'+TXH("quiz_retry")+'</button>':"");
  if(k==="Hotspot Image")return renderEl("hotspot",kstyle(k,"hotspot"),{},0,false)+'<ol class="hslist"><li>'+esc(L(T("The Torreão, the corner tower of the 1580s.","O Torreão, a torre de canto dos anos 1580.")))+'</li><li>'+esc(L(T("The royal chapel.","A capela real.")))+'</li></ol>';
  if(k==="Today Map")return KPREV["Today Map"]()+'<button type="button" class="arfocal">'+TXH("today_map_directions")+'</button>';
  return ""}
function kpageScreen(){var k=S.pageKind;S._pk=k;var h=topbar(esc(L(secById(secOfKind(k)).name))+" &#8250; <b>"+esc(k)+"</b>")+pageChips(k)+'<div class="pg"><div class="blk">'+kpageBody(k)+'</div></div>'+pageFoot(["card",L(secById(secOfKind(k)).name)],["card",L(UI.visit)]);S._pk=null;return h}
on("storyScreen.html",function(h){var ch=kopt("Story","layout")==="chapters";
  h=h.replace('<div class="pg">',pageChips("Story")+(ch?'<div class="chapbar"><span class="on"></span><span></span><span></span><small>'+esc(fmt(TXT("story_chapter_of"),1,3))+'</small></div>':"")+'<div class="pg">');return h});
on("galleryScreen.html",function(h){return h.replace('<div class="pg">',pageChips("Gallery")+'<div class="pg">')});
function viewerScreen(){var imgs=S.galleryImgs||["g1","g2","g3","before"],i=S.viewerAt%imgs.length,c=S.ctl.viewer.counter;
  return '<div class="viewer"><div class="vtop"><button type="button" data-go="gallery" aria-label="'+TXH("page_back")+'">&#8249;</button>'+(c==="chips"?'<div class="vchips">'+imgs.map(function(x,j){return'<button type="button" data-vat="'+j+'" aria-pressed="'+(j===i)+'">'+(j+1)+'</button>'}).join("")+'</div>':c==="count"?'<span>'+esc(fmt(TXT("viewer_position"),i+1,imgs.length))+'</span>':"<span></span>")+'</div>'+
   '<img alt="" src="'+IMG[imgs[i]]+'"><div class="vcapt">'+esc(L(DEMO.piccap))+' <small>Dirk Stoop</small><em>'+TXH("zoom_hint")+'</em></div></div>'}
on("renderPhone.before",function(keep,cx){if(S.view==="kpage"||S.view==="viewer"){cx.page=S.view;S.view="card"}});
on("renderPhone.after",function(keep,cx){if(cx.page){var v=cx.page;S.view=v;scr.innerHTML=v==="kpage"?kpageScreen():viewerScreen();scr.style.top="0px";
    var nb=document.getElementById("navbar");if(nb)nb.style.display="none";["locpin","sticky","vset","csheet","peekrow","topband"].forEach(function(k){var o=ph.querySelector("."+k);if(o)o.style.display="none"});
    var w=document.getElementById("wall");if(w)w.style.display="none";var gb=document.getElementById("grab");if(gb)gb.style.display="none"}});
document.addEventListener("click",function(e){var t=e.target;if(!ph.contains(t))return;var g=t.closest('[data-go="kpage"]');if(g){e.stopPropagation();S.pageKind=g.dataset.kind;S.view="kpage";renderPhone(false);return}
  var gp=t.closest('[data-go="story"][data-kind],[data-go="gallery"][data-kind]');if(gp&&S.view!=="card"){e.stopPropagation();S.view=gp.dataset.go;renderPhone(false);return}
  var vi=t.closest(".blk .el-pic img, .blk .fr img");if(vi&&S.view==="gallery"){e.stopPropagation();var all=Array.prototype.slice.call(scr.querySelectorAll(".blk img"));S.viewerAt=Math.max(0,all.indexOf(vi));S.view="viewer";renderPhone(false);return}
  var va=t.closest("[data-vat]");if(va){e.stopPropagation();S.viewerAt=+va.dataset.vat;renderPhone(false);return}
  var vp=t.closest(".viewer img");if(vp){e.stopPropagation();vp.classList.toggle("z2");return}},true);
/* a block kind's full page on the phone (a row's demo, the spec's page): the story, the gallery, or its own page */
function pageOpen(kind){if(kind==="Story")S.view="story";else if(kind==="Gallery")S.view="gallery";else{S.pageKind=kind;S.view="kpage"}}
