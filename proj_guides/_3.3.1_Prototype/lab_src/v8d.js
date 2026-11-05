/* ===================== v8d: Specific Marker (card starter, per block: look, frame, reading time, Open Row words, warnings; the page's tools; the Header page) and Languages & Terms ===================== */
S.card.concat(S.demoCard).forEach(function(b){Object.assign(b,{k_look:"inherit",frame:"inherit",minsMode:"auto",mins:5,orw:{en:"",pt:""}},b)});
S.hero={title:T("Ribeira Palace","Palácio da Ribeira"),eyebrow:T("Lisbon · c. 1500 to 1755","Lisboa · c. 1500 a 1755"),line:null,fx:50,fy:50,pic:"hero",pic2:"g2"};
S.starter="rich";S.pendingStarter=null;S.story.forEach(function(r){if(r.checked==null)r.checked=r.id!=="r10"&&r.id!=="r11"});
var STARTERS={simple:["Simple point",["Header","Quick Facts","Story","Gallery","Practical Info"]],rich:["Rich point",null],ar:["AR point",["Header","Quick Facts","Story","3D Model","Place In AR","Practical Info"]],scratch:["Start from scratch",[]]};
var CARD0=JSON.stringify(S.card);
/* a marker's own look and frame win over the block's */
var koptV8b=kopt;kopt=function(kind,key){var b=S._cb;if(b&&b.kind===kind&&key==="look"&&b.k_look&&b.k_look!=="inherit")return b.k_look;return koptV8b(kind,key)};
on("blockPreviewHtml.before",function(b){S._cb=b});on("blockPreviewHtml.done",function(){S._cb=null});
on("blockPreviewHtml.html",function(h,b){if(b.frame&&b.frame!=="inherit"&&S.secCfg.look==="cards")h=h.replace(/class="bprev( fr-[a-z]+)?/,'class="bprev fr-'+b.frame);
  if(b.minsMode==="hand")h=h.replace(/(<div class="open[^"]*"[^>]*>[^<]*?)\d+ min/,"$1"+b.mins+" min");
  var ow=b.orw&&L(b.orw);if(ow)h=h.replace(/(<div class="open[^"]*"[^>]*>)[^<]*/,"$1"+esc(ow)+" ");
  return h});
/* the hero's content (the Header page) */
on("cardScreen.html",function(h){if(typeof inDemoCard!=="undefined"&&inDemoCard)return h;return h.replace(/<h2>Ribeira Palace<\/h2>/,"<h2>"+esc(L(S.hero.title))+"</h2>").replace(/(<div class="hero[^"]*" style="background-image:url\()[^)]*\)/,"$1"+IMG[S.hero.pic]+");background-position:"+S.hero.fx+"% "+S.hero.fy+"%")});
function storyMarks(){var out=[],seen=[];function walk(o,d){if(o==null||d>8)return;if(typeof o==="string"){var i=0,j;while((i=o.indexOf("[[",i))>=0){j=o.indexOf("]]",i+2);if(j<0)break;var m=o.slice(i+2,j),k=m.indexOf("|");out.push(k>=0?m.slice(k+1):m);i=j+2}return}if(typeof o!=="object"||seen.indexOf(o)>=0)return;seen.push(o);Object.keys(o).forEach(function(x){walk(o[x],d+1)})}walk(S.story,0);return out}
function cardWarnings(){var w=[],terms=S.terms.map(function(t){return t.words.toLowerCase()}).join(",");
  S.card.forEach(function(b){var bd=S.blockDefs[b.kind]||{},sec=secById(grpOf(b));if(bd.offered===false)w.push(b.kind+" is not shown: its kind is Off for this wall (Detail Card › Blocks).");else if(sec&&sec.shown===false)w.push(b.kind+" is not shown: its section "+L(sec.name)+" is Hidden.");else if(S.poiSec[grpOf(b)]==="hide")w.push(b.kind+" is not shown: its section is hidden on this card.")});
  storyMarks().forEach(function(word){if(terms.indexOf(word.toLowerCase())<0)w.push("The story marks [["+word+"]], which Languages & Terms › Terms does not have.")});
  if(!S.hero.pic)w.push("The Header needs its picture for the Image parallax look.");
  return w}
on("edMarker.html",function(h){var d=document.createElement("div");d.innerHTML=h;var mc=innerOf(d,"mcard");
  if(mc){var edited=JSON.stringify(S.card)!==CARD0,cur=edited?"custom":S.starter,opts=Object.keys(STARTERS).map(function(k){return[k,STARTERS[k][0]]});if(edited)opts.unshift(["custom","Custom"]);
    var top=row("Card starter",sel("m.starter",opts,cur),"Fills this card with a set of blocks to start from. It reads Custom once you edit the blocks.");
    if(S.pendingStarter){var n=S.card.length,ps=STARTERS[S.pendingStarter];top+='<div class="decide"><b>Replace the '+n+' block'+(n===1?"":"s")+' on Ribeira Palace with '+esc(ps[0])+'?</b><span>'+(ps[1]?ps[1].length+" blocks: "+esc(ps[1].join(", ")):"the Rich point's blocks")+'. Your content in the blocks you keep stays; Ctrl+Z undoes it.</span><div class="btnrow"><button class="ubtn sm" type="button" data-starter-go="1">Replace the blocks</button><button class="del" type="button" data-starter-no="1">Keep my blocks</button></div></div>'}
    mc.insertAdjacentHTML("afterbegin",top);
    var ws=cardWarnings();if(ws.length)mc.insertAdjacentHTML("beforeend",'<div class="note warn"><b>On this card:</b><br>'+ws.map(esc).join("<br>")+'</div>');
    var i=S.card.findIndex(function(b){return b.id===S.sel.cblock}),b=S.card[i],fl=mc.querySelector("tr.slot .fields");
    if(b&&fl){var bd=S.blockDefs[b.kind]||{},k=KOPT[b.kind]||[],lk=k.filter(function(o){return o[0]==="look"})[0],x="",p="card."+i+".";
      var tr=Array.prototype.filter.call(fl.querySelectorAll(":scope > .row"),function(r){return(r.querySelector("label")||{}).textContent==="Tap"})[0];
      if(tr)tr.outerHTML=row("Tap",bd.page?pseg(p+"tap",["inherit","page","orow","expand","nothing"],["As the block ("+bd.tap+")","Open its page","Only the Open Row","Expand here","Nothing"]):pseg(p+"tap",["inherit","expand","nothing"],["As the block ("+bd.tap+")","Expand here","Nothing"]),"");
      if(lk){var ch=typeof lk[2]==="string"?(S.styles[lk[2].slice(3)]||[]).map(function(st){return[st.id,st.name]}):lk[2].map(function(v,j){return[v,lk[3][j]]}),dv=kopt(b.kind,"look"),dn=(ch.filter(function(c){return c[0]===dv})[0]||[0,dv])[1];
        x+=row("Look",psel(p+"k_look",[["inherit","As the block ("+dn+")"]].concat(ch)),"This block's look on this card only.")}
      if(b.kind==="Story")x+=row("Preview",pseg(p+"preview",["inherit","auto","custom"],["As the block ("+kopt("Story","preview")+")","Automatic","Custom"]),"Custom: pick the rows in 2 · Pages › Story › Preview rows.");
      x+=row("Frame",pseg(p+"frame",["inherit","normal","featured","quiet"],["As the block ("+bd.frame+")","Normal","Featured","Quiet"]),"Cards style only.")+
        row("Reading time",pseg(p+"minsMode",["auto","hand"],["Automatic","Set by hand"]),"Automatic: 200 words a minute, a picture 10 s (6 s in a carousel), a video 30 s.")+(b.minsMode==="hand"?row("Minutes",pslider(p+"mins",1,120,1),"",true):"");
      if(bd.page)x+=langCodes().map(function(c,j){return row(j?"Open Row words · "+c.toUpperCase():"Open Row words · "+c.toUpperCase(),'<input class="u" data-path="'+p+'orw.'+c+'" value="'+esc((b.orw||{})[c]||"")+'" placeholder="'+esc(L(UI.read))+' (the block\'s)">',j?"":"Empty: the block's own words.")}).join("");
      fl.insertAdjacentHTML("beforeend",x)}
    mc.querySelectorAll('tr.pick[data-pick="cblock"]').forEach(function(r){var b2=byId(S.card,r.dataset.id);if(b2&&b2.kind==="Story"){var n2=S.story.filter(function(x){return!x.checked}).length;if(n2){var bc=r.querySelector("td:nth-child(3)");if(bc)bc.insertAdjacentHTML("beforeend",' <span class="tocheck">'+n2+' to check</span>')}}})}
  var pg=innerOf(d,"pg:story");if(pg){var custom=(S.card.filter(function(b){return b.kind==="Story"})[0]||{}).preview==="custom"||(kopt("Story","preview")==="custom"&&(S.card.filter(function(b){return b.kind==="Story"})[0]||{}).preview==="inherit");
    var tl='<div class="tmplline">Started from <b>'+esc(STARTS_NAME[kopt("Story","starts")]||"Story -- magazine")+'</b> (its rows were copied) <button class="del" type="button" data-tmpl="reapply">Re-apply</button> <button class="del" type="button" data-tmpl="save">Save as template…</button> <button class="del" type="button" data-tmpl="pattern">Save these rows as a pattern…</button></div>';
    var pv=custom?'<div class="sub2">Preview rows · what the card shows</div><table class="tbl"><tbody>'+S.previewRows.map(function(pr,j){var src=byId(S.story,pr.link);return'<tr><td class="uses">'+(j+1)+'</td><td>'+(pr.link?(src?'Linked to row '+(S.story.indexOf(src)+1)+' · its own look':'<span class="warnrow">Broken link: the page row was removed. It shows nothing.</span>'):'Own row · '+esc(pr.kind))+'</td><td>'+(pr.link&&src?'<button class="del" type="button" data-prv="unlink|'+j+'">Unlink (make a copy)</button> ':'')+'<button class="del" type="button" data-prv="del|'+j+'">Remove</button></td></tr>'}).join("")+'</tbody></table><div class="btnrow"><button class="ubtn sm" type="button" data-prv="link">+ Link a page row</button><button class="ubtn sm" type="button" data-prv="own">+ Add own row</button></div>':
      '<div class="shows">Shows on the card (Automatic): Picture #1, Eyebrow #1, Title #1, Lede #1 <span class="uses">· marked P below</span></div>';
    var nt=pg.querySelector(":scope > .note");if(nt)nt.insertAdjacentHTML("afterend",tl+pv);
    pg.querySelectorAll('tr.pick[data-pick="row"]').forEach(function(r,j){var row0=byId(S.story,r.dataset.id);if(!row0)return;var last=r.lastElementChild,first=r.firstElementChild;
      if(!custom&&j===0&&first)first.insertAdjacentHTML("beforeend",'<span class="pmark" title="Used by the preview">P</span>');
      if(custom&&S.previewRows.some(function(pr){return pr.link===row0.id})&&first)first.insertAdjacentHTML("beforeend",'<span class="pmark" title="Linked in the preview">P</span>');
      if(last)last.insertAdjacentHTML("afterbegin",'<span class="grip" title="Drag to reorder">&#8942;&#8942;</span>');
      if(last)last.insertAdjacentHTML("beforeend",' <button class="del" type="button" data-sdup="'+row0.id+'" title="Duplicate">&#10697;</button> <label class="chk" title="Checked by the developer"><input type="checkbox" data-scheck="'+row0.id+'"'+(row0.checked?" checked":"")+'></label>')});
    var th=pg.querySelector("thead tr");if(th)th.lastElementChild.style.width="132px"}
  if(S.sel.page==="header"){var mp=innerOf(d,"mpages");if(mp)mp.innerHTML=row("Page of",sel("m.page",[["story","Story · "+L(DEMO.title)],["gallery","Gallery"],["header","Header · the hero"]],"header"),"")+fo("pg:header","Header · the hero's content",["ext","EXTENDED"],
    langCodes().map(function(c){return row("Title · "+c.toUpperCase(),'<input class="u" data-path="hero.title.'+c+'" value="'+esc(S.hero.title[c]||"")+'">',c==="en"?"The display title. The Editor warns when it takes 3+ lines on a phone in a language.":"")}).join("")+
    row("Picture",'<div class="assets">'+ASSETS.map(function(a){return'<button type="button" data-heropic="pic|'+a+'" aria-pressed="'+(S.hero.pic===a)+'" style="background-image:url('+IMG[a]+')" aria-label="'+a+'"></button>'}).join("")+'</div>',"The hero's picture (Image parallax, Spotlight crop).")+
    row("Second picture",'<div class="assets">'+ASSETS.map(function(a){return'<button type="button" data-heropic="pic2|'+a+'" aria-pressed="'+(S.hero.pic2===a)+'" style="background-image:url('+IMG[a]+')" aria-label="'+a+'"></button>'}).join("")+'</div>',"Split then and now: the Now side.")+
    row("Focus across",pslider("hero.fx",0,100,1),"The point of the picture that stays in view when the frame crops it (and the Spotlight's centre).")+row("Focus down",pslider("hero.fy",0,100,1),"")+
    row("Loop clip",'<input class="u" value="(none)" readonly>',"Video loop: a short silent clip from the media folder.")+row("Model",'<input class="u" value="(none)" readonly>',"Model turntable: a model from the media folder."),2)}
  else{var ps=d.querySelector('select[data-sel="m.page"]');if(ps&&!ps.querySelector('option[value="header"]'))ps.insertAdjacentHTML("beforeend",'<option value="header">Header · the hero</option>')}
  return d.innerHTML});
var STARTS_NAME={magazine:"Story -- magazine",short:"Story -- short",empty:"Empty"};
S.previewRows=[{link:"r1"},{link:"r7"},{kind:"paragraph"},{link:"r99"}];
document.addEventListener("change",function(e){var t=e.target;if(!ed.contains(t))return;
  if(t.dataset.sel==="m.page"&&t.value==="header"){e.stopPropagation();S.sel.page="header";renderEditor();S.view="card";S.stop="full";renderPhone(false);return}
  if(t.dataset.sel==="m.starter"){e.stopPropagation();if(t.value!=="custom"){S.pendingStarter=t.value;renderEditor()}return}
  if(t.dataset.scheck){e.stopPropagation();var r=byId(S.story,t.dataset.scheck);if(r){r.checked=t.checked;renderEditor()}}},true);
ed.addEventListener("click",function(e){var t=e.target;
  if(t.closest("[data-starter-no]")){e.stopPropagation();S.pendingStarter=null;renderEditor();return}
  if(t.closest("[data-starter-go]")){e.stopPropagation();var ks=STARTERS[S.pendingStarter][1];S.starter=S.pendingStarter;S.pendingStarter=null;
    if(ks===null)S.card=JSON.parse(CARD0);else S.card=ks.filter(function(k){return k!=="Header"}).map(function(k,j){var old=S.card.filter(function(b){return b.kind===k})[0];return old||{id:"b_s"+j,kind:k,grp:"inherit",enter:"inherit",tap:"inherit",orow:"inherit",k_look:"inherit",frame:"inherit",minsMode:"auto",mins:5,orw:{en:"",pt:""}}});
    CARD0=JSON.stringify(S.card);renderAll(false);show("card");return}
  var dp=t.closest("[data-sdup]");if(dp){e.stopPropagation();var i=S.story.findIndex(function(r){return r.id===dp.dataset.sdup}),cp=JSON.parse(JSON.stringify(S.story[i]));cp.id=nid("r");cp.checked=false;S.story.splice(i+1,0,cp);renderAll(true);show("story");return}
  var tp=t.closest("[data-tmpl]");if(tp){e.stopPropagation();tp.textContent=tp.dataset.tmpl==="reapply"?"Nothing missing":"Named · saved";return}
  var pv=t.closest("[data-prv]");if(pv){e.stopPropagation();var q=pv.dataset.prv.split("|");if(q[0]==="del")S.previewRows.splice(+q[1],1);else if(q[0]==="unlink")S.previewRows[+q[1]]={kind:"pull_quote"};else if(q[0]==="link")S.previewRows.push({link:"r9"});else S.previewRows.push({kind:"paragraph"});renderAll(true);show("card");return}
  var hp=t.closest("[data-heropic]");if(hp){e.stopPropagation();var q2=hp.dataset.heropic.split("|");S.hero[q2[0]]=q2[1];renderAll(true);S.view="card";S.stop="full";renderPhone(false);return}},true);

/* ----- Languages & Terms ----- */
LANGPICK.push(["pt-BR","Português (Brasil)"],["nl","Nederlands"],["sv","Svenska"],["da","Dansk"],["nb","Norsk bokmål"],["fi","Suomi"],["pl","Polski"],["cs","Čeština"],["hu","Magyar"],["ro","Română"],["el","Ελληνικά"],["tr","Türkçe"],["ru","Русский"],["uk","Українська"],["ar","العربية"],["he","עברית"],["hi","हिन्दी"],["zh-Hans","中文（简体）"],["zh-Hant","中文（繁體）"],["ja","日本語"],["ko","한국어"],["ca","Català"],["gl","Galego"],["eu","Euskara"]);
S.langs.opensIn="phone";
var edWordsV7=edWords;
edWords=function(){var d=document.createElement("div");d.innerHTML=edWordsV7();
  addRows(d,"w1",prow("Opens in",pseg("langs.opensIn",["phone","fallback"],["The phone's language when the wall has it","The fallback"]),"The card's language at the first open; the visitor's own pick wins after that."),"Fallback language");
  var lt=innerOf(d,"w1");if(lt)lt.querySelectorAll("tbody tr").forEach(function(r,i){var c=r.lastElementChild;if(c)c.insertAdjacentHTML("afterbegin",'<button class="del" type="button" data-lmove="'+i+'|-1" aria-label="Up">^</button><button class="del" type="button" data-lmove="'+i+'|1" aria-label="Down">v</button>')});
  var fb=d.querySelector("[data-txf]");if(fb&&!d.querySelector('[data-txf="app"]'))fb.parentNode.insertAdjacentHTML("beforeend",'<button class="chip" type="button" data-txf="app" aria-pressed="'+(S.ui.txf==="app")+'">App texts</button>');
  return d.innerHTML};
var txRowsV7=txRows;txRows=function(){if(S.ui.txf==="app")return '<tr><td colspan="9" class="uses">This wall\'s app adds no blocks of its own. An app\'s own block kinds bring their words here, each on its block.</td></tr>';return txRowsV7()};
var TXDEF=JSON.parse(TX0);
var txPanelV7=txPanel;txPanel=function(o){var h=txPanelV7(o),d=document.createElement("div");d.innerHTML=h;
  d.querySelectorAll("tbody tr").forEach(function(r){var inp=r.querySelector("[data-tx]");if(!inp)return;var k=inp.dataset.tx.split("|")[0];
    if(JSON.stringify(S.tx[k])!==JSON.stringify(TXDEF[k])){var c=r.lastElementChild;if(c)c.insertAdjacentHTML("afterbegin",'<button class="del" type="button" data-txreset="'+k+'" title="Back to the framework\'s wording">&#8634;</button>')}});return d.innerHTML};
ed.addEventListener("click",function(e){var t=e.target;
  var lm=t.closest("[data-lmove]");if(lm){e.stopPropagation();var q=lm.dataset.lmove.split("|"),i=+q[0],j=i+(+q[1]),a=S.langs.list;if(j<0||j>=a.length)return;var x=a[i];a[i]=a[j];a[j]=x;renderAll(true);return}
  var tr=t.closest("[data-txreset]");if(tr){e.stopPropagation();var k=tr.dataset.txreset;S.tx[k]=JSON.parse(JSON.stringify(TXDEF[k]));renderAll(true);return}},true);
var FOCUS9={w1:["card",".chrome i:last-child","full"]};Object.keys(FOCUS9).forEach(function(k){if(!FOCUS[k])FOCUS[k]=FOCUS9[k]});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(m\.starter|"));
on("modelSig.before",function(cx){var x="";if(sigWant(/frame/)){var look=S.secCfg.look,keep=S.view;S.secCfg.look="cards";S.view="card";x="#cardsR#"+cardScreen();S.secCfg.look=look;S.view=keep}cx.frame=x});on("modelSig.sig",function(s,cx){return s+cx.frame});

/* reading time: the Contents sheet shows each block's minutes */
var KMIN={"Story":4,"Gallery":2,"Video":1,"Audio Guide":3,"Timeline":2,"Dialogue":2,"Knowledge Check":2};function minsOf(b){return b.minsMode==="hand"?b.mins:(KMIN[b.kind]||1)}
var contentsSheetV8b=contentsSheet;contentsSheet=function(){var h=contentsSheetV8b(),d=document.createElement("div");d.innerHTML=h;var subs=d.querySelectorAll(".crow.sub, .crow:not(.sec)"),list=S.card.filter(function(b){return(S.blockDefs[b.kind]||{}).offered!==false});
  if(S.ctl.contents.rows==="sections"){var k=0;cardRuns().forEach(function(r){r.blocks.forEach(function(b){var el=d.querySelectorAll(".crow.sub")[k++];if(el)el.insertAdjacentHTML("beforeend",'<span class="cmin">'+esc(fmt(TXT("nav_contents_minutes"),minsOf(b)))+'</span>')})})}
  else d.querySelectorAll(".crow").forEach(function(el,j){var b=S.card[j];if(b)el.insertAdjacentHTML("beforeend",'<span class="cmin">'+esc(fmt(TXT("nav_contents_minutes"),minsOf(b)))+'</span>')});return d.innerHTML};

/* captures: #dcshot=<kind> opens the demo card at that block, still */
if(location.hash.indexOf("#dcshot=")===0)setTimeout(function(){var dk=decodeURIComponent(location.hash.slice(8)),pv=dk.split("|");S.reduce=true;S.previewBar=false;if(pv[1])S.lang=pv[1];S.view="democard";S.stop="full";renderAll(false);setTimeout(function(){var b=S.demoCard.filter(function(x){return x.kind===pv[0]})[0],el=b&&scr.querySelector('[data-ref="b:'+b.id+'"]');if(el)scr.scrollTop=Math.max(0,el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-70)},200)},50);
/* block thumbnails: the kind's own preview, where it has one */
on("libThumb.answer",function(name){var f=KPREV[name];if(!f||name==="Quick Facts")return;var h;try{h=f({kind:name,id:"thumb"})}catch(e){return}return noBtn('<div class="thumb cv'+(S.dark?" dark":"")+'"><div class="tin"><div class="bprev">'+h+'</div></div></div>')});
delete S.cardCfg.peekNext;
