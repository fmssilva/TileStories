/* The Specific Marker tab (edMarker): the point's card and pages, the starters, the card's warnings, the preview rows. */
function edMarkerV3(){var h=row("Marker",sel("m.poi",[["painting","7. Ribeira Palace"]],"painting"),"The marker whose card you edit.");
  h+=locked("Position & Visual")+locked("Labels");
  /* 1. the card */
  var cb='<div class="note">The card as the visitor meets it: blocks in order, in their bar groups, each with its preview and what a tap does. The Header is set above (existing).</div><table class="tbl"><thead><tr><th style="width:22px">#</th><th style="width:80px">Preview</th><th>Block</th><th style="width:70px">Group</th><th style="width:74px"></th></tr></thead><tbody>';
  S.card.forEach(function(b,i){cb+='<tr class="pick'+(S.sel.cblock===b.id?" sel":"")+'" data-pick="cblock" data-id="'+b.id+'"><td class="uses">'+(i+1)+'</td><td>'+cardThumb(b)+'</td><td><b>'+esc(b.kind)+'</b>'+(b.pattern?' <span class="uses">· '+esc((patternById(b.pattern)||{}).name||"(missing)")+'</span>':'')+'</td><td class="uses">'+esc(L(UI[b.grp]))+'</td><td>'+mv2("cmove",i)+'</td></tr>';
    if(S.sel.cblock===b.id){var x='<div class="fields">'+row("Group",sel("cb.grp",GROUPS.map(function(g){return[g,UI[g].en]}),b.grp),"The bar group (section) it sits in.")+
      row("Entrance",sel("cb.enter",motionOpts("entrance",true),b.enter),"How the preview comes into view.")+
      row("Tap",seg("cb.tap",["page","expand","nothing"],["Open its page","Expand here","Nothing"],b.tap),"What a tap on the preview does. Open its page goes to the block's full page (Pages below).")+
      (b.tap==="page"?row("Open Row",seg("cb.orow",["plain","nudge","shine"],["Plain","Nudge","Shine"],b.orow),"The words that invite the tap, and their small motion."):"")+
      (b.kind==="Story"?row("Preview",seg("cb.preview",["auto","custom"],["Automatic","Custom"],b.preview||"auto"),"Automatic: the page's first picture, eyebrow, title and lede."):"")+
      (b.kind==="Inline"?row("Pattern",sel("cb.pattern",S.patterns.map(function(p){return[p.id,p.name]}),b.pattern),"What the Inline block places on the card."):"")+'</div>';
      cb+='<tr class="slot"><td></td><td colspan="4">'+x+'</td></tr>'}});
  cb+='</tbody></table><div class="note">+ Add block</div><div class="btnrow">'+LIB.slice(0,10).map(function(r){return'<button class="ubtn sm" type="button" data-cadd="'+esc(r[1])+'">'+esc(r[1])+'</button>'}).join("")+'<span class="uses">…</span></div>';
  h+=fo("mcard","1 · The card · blocks and previews",["new","NEW"],cb,1);
  /* 2. pages */
  var t='<div class="note">Story · '+S.story.length+' rows. Pick a style by its picture; open a row for its content fields (EN / PT).</div><table class="tbl"><thead><tr><th style="width:22px">#</th><th style="width:26%">Element</th><th style="width:38%">Style</th><th>Content</th><th style="width:74px"></th></tr></thead><tbody>';
  S.story.forEach(function(r,i){
   if(r.pattern){var p=patternById(r.pattern);
     t+='<tr class="grp pick'+(S.sel.row===r.id?" sel":"")+'" data-pick="row" data-id="'+r.id+'"><td class="uses">'+(i+1)+'</td><td colspan="2"><span class="kicon p">Pt</span><b>'+(p?esc(p.name):'<span class="missing">(missing)</span>')+'</b></td><td class="uses">pattern · '+(p?p.rows.length:0)+' slots</td><td>'+mv2("smove",i)+'</td></tr>';
     if(p&&S.sel.row===r.id)p.rows.forEach(function(pr,j){var st=styleById(pr.style)||defStyle(pr.kind);r.slots[j]=r.slots[j]||{};
       t+='<tr class="slot"><td></td><td colspan="4"><div class="sub2">'+KIND[pr.kind].n+' · '+esc(st?st.name:"(missing)")+' · the pattern\'s style</div>'+fieldsFor(pr.kind,r.slots[j],r.id+".s"+j)+'</td></tr>'});}
   else{var st2=(r.style&&styleById(r.style))||defStyle(r.kind);
     t+='<tr class="pick'+(S.sel.row===r.id?" sel":"")+'" data-pick="row" data-id="'+r.id+'"><td class="uses">'+(i+1)+'</td><td><span class="kicon">'+KIND[r.kind].ab+'</span>'+KIND[r.kind].n+'</td><td><button class="ubtn sm" type="button" data-openpicker="'+r.id+'" style="height:auto;padding:2px 6px;display:flex;align-items:center;max-width:100%">'+thumb(r.kind,st2,"sm")+'<span style="overflow:hidden;text-overflow:ellipsis">'+esc(st2.name)+(r.style?"":" (default)")+' &#9662;</span></button></td><td class="uses">'+esc(C(r.c,"text")||C(r.c,"cap"))+'</td><td>'+mv2("smove",i)+'</td></tr>';
     if(S.sel.picker===r.id)t+='<tr class="slot"><td></td><td colspan="4"><div class="picker"><div class="thumbs">'+[{id:"",name:defStyle(r.kind).name+" (default)"}].concat(S.styles[r.kind]).map(function(s){var real=s.id?s:defStyle(r.kind);return'<button class="sc" type="button" data-setstyle="'+r.id+'|'+s.id+'" aria-pressed="'+((r.style||"")===s.id)+'">'+thumb(r.kind,real)+'<span class="nm"><span>'+esc(s.name)+'</span></span></button>'}).join("")+'</div></div></td></tr>';
     if(S.sel.row===r.id)t+='<tr class="slot"><td></td><td colspan="4">'+fieldsFor(r.kind,r.c,r.id+".c")+'</td></tr>';}});
  t+='</tbody></table><div class="note">+ Add element</div><div class="btnrow">'+KINDS.map(function(K){return'<button class="ubtn sm" type="button" data-sadd="'+K.k+'">'+K.n+'</button>'}).join("")+'</div>';
  t+='<div class="note">+ Add pattern</div><div class="thumbs">'+S.patterns.map(function(p){return'<button class="sc" type="button" data-spadd="'+p.id+'">'+patThumb(p)+'<span class="nm"><span>'+esc(p.name)+'</span></span></button>'}).join("")+'</div>';
  var pages=row("Page of",sel("m.page",[["story","Story · "+L(DEMO.title)],["gallery","Gallery"]],S.sel.page||"story"),"Each block with a page has its content here.")+fo("pg:story","Story · "+esc(L(DEMO.title)),["ext","EXTENDED"],t,9);
  h+=fo("mpages","2 · Pages · each block's content",["new","NEW"],pages,1);
  h+=jsonBox("config.json · the marker's detail card (blocks + the story page)",markerJson());return h;}
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
  return pipe("edMarker.html",h);}
S.starter="rich";S.pendingStarter=null;
var STARTERS={simple:["Simple point",["Header","Quick Facts","Story","Gallery","Practical Info"]],rich:["Rich point",null],ar:["AR point",["Header","Quick Facts","Story","3D Model","Place In AR","Practical Info"]],scratch:["Start from scratch",[]]};
var CARD0=JSON.stringify(S.card);
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
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(m\.starter|"));
