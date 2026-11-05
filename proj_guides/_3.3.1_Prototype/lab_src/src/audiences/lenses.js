/* Lenses: the See it as chips, the tags per block, story row and guide step. */
S.lensCfg={on:true,story:true,min:2};S.lens="";
COMP.s8lens=["Audiences","Lenses"];FOCUS.s8lens=["card",".lensbar","half"];
/* the point's topics, and which blocks, story rows and guide steps belong to them (none = core, always shown) */
S.lenses=[{id:"politics",name:T("Politics","Política")},{id:"architecture",name:T("Architecture","Arquitetura")},{id:"court",name:T("Court life","Vida na corte")}];
S.tags={"r:r2":["politics"],"r:r4":["architecture","politics"],"r:r5":["court"],"r:r1":["architecture"],"r:r6":["architecture"],"b:b_fun":["court"],"b:b_ba":["architecture"]};
(function(){var t={g5:["court"],g6:["architecture"]};Object.keys(t).forEach(function(id){var s=byId(S.guideSteps,id);if(s)s.topics=t[id]})})();
function lensOn(){return S.lensCfg.on&&taggedCount()>=S.lensCfg.min}
function taggedCount(){return Object.keys(S.tags).filter(function(k){return(S.tags[k]||[]).length}).length}
function lensName(id){var l=S.lenses.filter(function(x){return x.id===id})[0];return l?L(l.name):id}
function fitsLens(tags){return!S.lens||!tags||!tags.length||tags.indexOf(S.lens)>=0}
/* the chip row: under the hero on the card, and at the top of the story page */
function lensBar(){return'<div class="lensbar" role="toolbar" aria-label="'+TXH("lens_label")+'"><span class="lbl">'+TXH("lens_label")+'</span><div class="chips">'+
  [["",TXT("lens_all")]].concat(S.lenses.map(function(l){return[l.id,L(l.name)]})).map(function(c){return'<button type="button" data-lens="'+c[0]+'" aria-pressed="'+(S.lens===c[0])+'">'+esc(c[1])+'</button>'}).join("")+'</div></div>'}
on("renderPhone.after",function(keep){lensApply()});
function lensApply(){scr.querySelectorAll(".lensbar").forEach(function(b){b.remove()});scr.querySelectorAll(".lenshidden,.lensmark").forEach(function(e){e.classList.remove("lenshidden","lensmark")});scr.querySelectorAll(".lensbadge").forEach(function(e){e.remove()});
  if(!lensOn()){S.lens="";return}
  if(S.view==="card"){var h=scr.querySelector(".hero");if(h)h.insertAdjacentHTML("afterend",lensBar())}
  else if(S.view==="story"&&S.lensCfg.story){var f=scr.querySelector('[data-ref^="r:"]');if(f)f.insertAdjacentHTML("beforebegin",lensBar())}
  /* the rows and blocks of another lens step aside; a heading goes with its rows; the lens's own get its badge */
  var els=[].slice.call(scr.querySelectorAll("[data-ref]")).filter(function(e){return/^[rb]:/.test(e.dataset.ref)}),lastHead=null,headShown=false;
  els.forEach(function(e){var r=e.dataset.ref,isHead=r.indexOf("r:")===0&&(byId(S.story,r.slice(2))||{}).kind==="heading";
    if(isHead){if(lastHead&&!headShown)lastHead.classList.add("lenshidden");lastHead=e;headShown=false;return}
    var tg=S.tags[r],show=fitsLens(tg);if(!show)e.classList.add("lenshidden");else{headShown=true;if(S.lens&&tg&&tg.indexOf(S.lens)>=0){var host=e.querySelector(":scope > .mv")||e;host.classList.add("lensmark");host.insertAdjacentHTML("afterbegin",'<span class="lensbadge">'+esc(lensName(S.lens))+'</span>')}}});
  if(lastHead&&!headShown&&S.lens)lastHead.classList.add("lenshidden");
  /* the chosen chip is always in view in its row */
  scr.querySelectorAll(".lensbar .chips").forEach(function(c){var pr=c.querySelector("[aria-pressed=true]");if(pr)c.scrollLeft=Math.max(0,pr.offsetLeft-c.offsetLeft-12)})}
/* a tap on a chip: the lens changes in place; the first core block in view stays where it was on the screen */
ph.addEventListener("click",function(e){var c=e.target.closest("[data-lens]");if(!c)return;e.stopPropagation();
  var top=scr.getBoundingClientRect().top,anchor=null,y=0;[].slice.call(scr.querySelectorAll("[data-ref]")).some(function(el){var r=el.getBoundingClientRect();if(r.bottom>top+60&&!(S.tags[el.dataset.ref]||[]).length&&!el.classList.contains("lenshidden")){anchor=el;y=r.top;return true}});
  S.lens=c.dataset.lens;lensApply();if(anchor&&anchor.isConnected)scr.scrollTop+=anchor.getBoundingClientRect().top-y;
  var bar=scr.querySelector(".lensbar [aria-pressed=true]");if(bar)bar.focus({preventScroll:true})},true);
/* the guide plays the core steps and the lens's own */
on("stepFits.ok",function(ok,st){return ok&&fitsLens(st.topics)});

/* Editor: 7 · Audiences > Lenses (the chips); Specific Marker > 5 · Lenses (the point's topics, and what belongs to each) */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var lk=d.querySelector('[data-fo="s8look"]');if(!lk)return d.innerHTML;
  lk.insertAdjacentHTML("afterend",comp("s8lens","“See it as” chips: a topic of the point, chosen during the visit",
    row("Lenses",pseg("lensCfg.on",["true","false"],["On","Off"]),"Chips under the hero (Everything, then the point's topics). A chip keeps the core blocks and the chosen topic's; the other topics' blocks step aside. Nothing moves: the order stays, the first block in view stays where it was, and the topic's blocks get its badge. Never asked at the first card.")+
    row("On the story page too",pseg("lensCfg.story",["true","false"],["Yes","No"]),"The same chips at the top of the story; a heading goes with its rows.")+
    row("Shown from",pseg("lensCfg.min",["2","3","4"],["2 tagged","3 tagged","4 tagged"]),"The chips show only when the point has at least this many blocks tagged with a topic.")));
  var f=innerOf(d,"s8lens");if(f)f.insertAdjacentHTML("beforeend",ownbar("s8lens")+ownPanels("s8lens"));return d.innerHTML});
function tagItems(){var out=[];(S.card||[]).forEach(function(b){out.push({ref:"b:"+b.id,label:"Card · "+(b.kind||"")+" · "+b.id.replace(/^b_/,"")})});
  S.story.forEach(function(r){if(r.kind!=="heading")out.push({ref:"r:"+r.id,label:"Story · "+(r.pattern?((patternById(r.pattern)||{}).name||r.pattern):(KIND[r.kind]?KIND[r.kind].n:r.kind))+" · "+r.id})});return out}
on("edMarker.html",function(h){var t='<div class="note">The point\'s topics, shown as “See it as” chips. Tag a block, a story row or a guide step with the topics it belongs to; untagged ones are core and always shown.</div>'+
  '<table class="tbl"><thead><tr><th>Topic</th><th>EN</th><th>PT</th><th style="width:40px"></th></tr></thead><tbody>'+S.lenses.map(function(l,i){return'<tr><td class="uses">'+esc(l.id)+'</td><td><input class="u" data-lensn="'+i+'|en" value="'+esc(l.name.en||"")+'"></td><td><input class="u" data-lensn="'+i+'|pt" value="'+esc(l.name.pt||"")+'"></td><td><button class="del" type="button" data-lensdel="'+i+'">Remove</button></td></tr>'}).join("")+
  '</tbody></table><div class="btnrow"><button class="ubtn sm" type="button" data-lensadd="1">+ Add a topic</button></div>'+
  '<table class="tbl tagm"><thead><tr><th>Belongs to</th>'+S.lenses.map(function(l){return'<th>'+esc(L(l.name))+'</th>'}).join("")+'</tr></thead><tbody>'+
  tagItems().concat((S.fullSteps||S.guideSteps).map(function(st,j){return{ref:"g:"+st.id,label:"Guide · step "+(j+1)}})).map(function(it){var tg=it.ref.indexOf("g:")===0?(byId(S.fullSteps||S.guideSteps,it.ref.slice(2)).topics||[]):(S.tags[it.ref]||[]);
    return'<tr><td>'+esc(it.label)+'</td>'+S.lenses.map(function(l){return'<td><input type="checkbox" data-tag="'+it.ref+'|'+l.id+'"'+(tg.indexOf(l.id)>=0?" checked":"")+' aria-label="'+esc(it.label+" · "+L(l.name))+'"></td>'}).join("")+'</tr>'}).join("")+'</tbody></table>'+
  '<div class="note">'+taggedCount()+' tagged on the card and the story'+(taggedCount()<S.lensCfg.min?": the chips need "+S.lensCfg.min+".":".")+'</div>';
  var sec=fo("m5","5 · Lenses",["new","NEW"],t,1);var i=h.lastIndexOf('<div class="json">');return i<0?h+sec:h.slice(0,i)+sec+h.slice(i)});
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.tag)return;e.stopImmediatePropagation();var q=t.dataset.tag.split("|"),list;
  if(q[0].indexOf("g:")===0){var st=byId(S.fullSteps||S.guideSteps,q[0].slice(2));list=st.topics=st.topics||[]}else list=S.tags[q[0]]=S.tags[q[0]]||[];
  var i=list.indexOf(q[1]);if(t.checked&&i<0)list.push(q[1]);if(!t.checked&&i>=0)list.splice(i,1);renderAll(true)},true);
ed.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.lensn)return;e.stopImmediatePropagation();var q=t.dataset.lensn.split("|");S.lenses[+q[0]].name[q[1]]=t.value;clearTimeout(S.lensT);S.lensT=setTimeout(function(){renderPhone(true)},250)},true);
ed.addEventListener("click",function(e){var t=e.target.closest("[data-lensadd],[data-lensdel]");if(!t)return;e.stopPropagation();
  if(t.dataset.lensadd){var n=S.lenses.length+1;S.lenses.push({id:"topic"+n,name:T("Topic "+n,"Tema "+n)})}
  else{var l=S.lenses.splice(+t.dataset.lensdel,1)[0];Object.keys(S.tags).forEach(function(k){S.tags[k]=S.tags[k].filter(function(x){return x!==l.id})});(S.fullSteps||S.guideSteps).forEach(function(s){if(s.topics)s.topics=s.topics.filter(function(x){return x!==l.id})});if(S.lens===l.id)S.lens=""}
  renderAll(true)},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("ctl\\.jump\\.","ctl\\.jump\\.|lensCfg\\.story|lensCfg\\.min"));
/* the self-test's view of the phone includes the chips and what a lens hides */
on("modelSig.sig",function(h){return h+"#lens:"+(lensOn()?(S.lensCfg.story?"s":"")+S.lens+"|"+S.lenses.map(function(l){return l.id+":"+L(l.name)}).join(",")+"|"+JSON.stringify(S.tags):"off")});
/* each row's demo: the See it as chips under the hero; on the story page */
rowDemos({"lensCfg.on":{stop:"full",ring:".lensbar|.hero"},"lensCfg.min":{stop:"full",ring:".lensbar|.hero"},"lensCfg.story":{view:"story",ring:".lensbar|.topbar"}});
