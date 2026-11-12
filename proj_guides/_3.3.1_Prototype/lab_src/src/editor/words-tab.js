/* The Languages & Terms tab (edWords): the words of each part, where each word is used, the term pages. */
/* ----- the fourth tab: Languages & Terms ----- */
/* every localized value of the wall, interface and content, as references to the ONE place it is stored */
function locRefs(){var out=[];
  Object.keys(TXO).forEach(function(o){TXO[o].forEach(function(k){out.push({ref:"tx:"+k,kind:"Interface",label:TXP[k],where:ownerName(o),go:o})})});
  S.sections.forEach(function(s,i){out.push({ref:"sec:"+i,kind:"Content",label:"Section name",where:"Sections › "+s.name.en,go:"sec:"+s.id})});
  S.story.forEach(function(r,i){var add=function(kind,c,slot){(FIELDS[kind]||[]).forEach(function(f){if(f[1]==="ltext"&&c&&c[f[0]])out.push({ref:"story:"+r.id+":"+slot+":"+f[0],kind:"Content",label:(KIND[kind]?KIND[kind].n:kind)+" · "+f[2],where:"The Ribeira Palace › Story page › row "+(i+1),go:"story:"+r.id})})};
    if(r.pattern){var p=patternById(r.pattern);if(p)p.rows.forEach(function(pr,j){add(pr.kind,r.slots[j],"s"+j)})}else add(r.kind,r.c,"c")});
  S.terms.forEach(function(t){out.push({ref:"term:"+t.id,kind:"Content",label:"Definition",where:"Terms › "+(termWords(t)[0]||""),go:"term:"+t.id})});
  S.guideSteps.forEach(function(st,j){if(st.text)out.push({ref:"gstep:"+st.id,kind:"Content",label:"Guide step "+(j+1),where:"The Ribeira Palace › Guide script",go:"gstep:"+st.id})});
  return out}
function refObj(ref){var p=ref.split(":");if(p[0]==="tx")return S.tx[p[1]];if(p[0]==="sec")return S.sections[+p[1]].name;if(p[0]==="term"){var t=byId(S.terms,p[1]);return t&&t.def}
  if(p[0]==="gstep"){var g=byId(S.guideSteps,p[1]);return g&&g.text}if(p[0]==="story"){var r=byId(S.story,p[1]);if(!r)return null;var c=p[2]==="c"?r.c:r.slots[+p[2].slice(1)];return c&&c[p[3]]}return null}
function goLoc(go){var p=go.split(":");if(p[0]==="sec"){goOwner("s2");S.op["sec:"+p[1]]=true;renderEditor();return}
  if(p[0]==="story"){S.tab="marker";S.op.mpages=true;S.op["pg:story"]=true;S.sel.page="story";S.sel.row=p[1];renderEditor();show("story","r:"+p[1]);return}
  if(p[0]==="term"){S.tab="words";S.sel.term=p[1];renderEditor();return}if(p[0]==="gstep"){S.tab="marker";S.op.m3=true;S.sel.gstep=p[1];renderEditor();return}goOwner(go)}
function tryWord(w){w=w.trim();if(!w)return "";var t=termFor(w);
  if(!t){var near=S.terms.filter(function(x){return termWords(x).some(function(y){return y.toLowerCase().indexOf(w.toLowerCase())>=0})});
    return near.length?"Not a whole word of any row (close: "+near.map(function(x){return termWords(x)[0]}).join(", ")+").":"No row has this word: the search matches it only as written."}
  var ws=termWords(t),u=termUses(t),def=L(t.def);
  return "Search: a point holding any of "+ws.map(function(x){return "“"+x+"”"}).join(", ")+" is found by all of them. "+
   (def?(u.n?"Card: underlined in "+u.rows.length+" text"+(u.rows.length>1?"s":"")+" of the Ribeira Palace's story; a tap shows the definition"+(t.page.length?" and Read more opens its page. ":". "):"Card: no card text marks it yet (write [["+ws[0]+"]]). "):"No definition: search only. ")+"Guide: Ask (search) reads the same row."}
function txRows(){var ans=firstAnswer("txRows.answer");if(ans!==undefined)return ans;var q=S.ui.txq.trim().toLowerCase(),out="",f=S.ui.txf;
  if(f==="retired")return TXRET.map(function(k){return'<tr><td>'+esc(S.tx[k].en)+'</td><td colspan="'+S.langs.list.length+'" class="uses">'+esc(TXF[k])+'</td><td></td></tr>'}).join("");
  locRefs().forEach(function(r){var v=refObj(r.ref);if(!v)return;var m=missingOf(v),k=r.ref.slice(3);
    if(f==="missing"&&!m)return;if(f==="new"&&!(r.kind==="Interface"&&TXNEW[k]))return;if(f==="interface"&&r.kind!=="Interface")return;if(f==="content"&&r.kind!=="Content")return;
    if(q&&(JSON.stringify(v)+" "+r.label+" "+r.where).toLowerCase().indexOf(q)<0)return;
    out+='<tr><td title="'+esc(r.where)+'"><span class="kt '+(r.kind==="Interface"?"i":"c")+'">'+(r.kind==="Interface"?"UI":"Content")+'</span> '+esc(r.label)+'<span class="fate2">'+esc(r.where)+'</span></td>'+
     S.langs.list.map(function(l){return'<td><input class="u'+(v[l.code]?'':' miss')+'" data-loc="'+r.ref+'|'+l.code+'" value="'+esc(v[l.code]||"")+'"></td>'}).join("")+'<td><button class="del" type="button" data-goloc="'+esc(r.go)+'">Go to</button></td></tr>'});
  return out||'<tr><td colspan="'+(S.langs.list.length+2)+'" class="uses">No text matches.</td></tr>'}
function wordsJson(){return{languages:langCodes(),fallback_language:S.langs.fallback,
  terms:S.terms.map(function(t){var o={words:termWords(t)};if(t.def.en||t.def.pt)o.definition=t.def;if(t.page.length)o.page=t.page.map(function(r){return{element:r.kind,content:r.c}});return o}),strings_changed:changedTexts()}}
function termPageEditor(t){var h='<div class="sub2">Its page (optional) · opened by Read more under the definition</div><table class="tbl"><thead><tr><th style="width:22px">#</th><th style="width:28%">Element</th><th>Content</th><th style="width:30px"></th></tr></thead><tbody>';
  t.page.forEach(function(r,i){h+='<tr><td class="uses">'+(i+1)+'</td><td><span class="kicon">'+KIND[r.kind].ab+'</span>'+KIND[r.kind].n+'</td><td style="white-space:normal">'+fieldsFor(r.kind,r.c,"termpage:"+t.id+":"+i)+'</td><td><button class="del" type="button" data-tprm="'+t.id+'|'+i+'">&#128465;</button></td></tr>'});
  return h+'</tbody></table><div class="btnrow">'+["paragraph","picture","carousel","video","pull_quote","before_after"].map(function(k){return'<button class="ubtn sm" type="button" data-tpadd="'+t.id+'|'+k+'">+ '+KIND[k].n+'</button>'}).join("")+'</div>'}
function edWords(){var h='<div class="note">Words every part of the wall reads: the languages, the terms both the search and the card know, and a check of every text. Each text is stored once, where its part is; this tab and the parts edit the same value.</div>';
  var lg='<table class="tbl"><thead><tr><th style="width:60px">Code</th><th>Name</th><th style="width:150px">Role</th><th style="width:30px"></th></tr></thead><tbody>'+S.langs.list.map(function(l,i){return'<tr><td><b>'+l.code.toUpperCase()+'</b></td><td>'+esc(l.name)+'</td><td class="uses">'+(l.code===S.langs.fallback?"fallback":"")+'</td><td>'+(i>1?'<button class="del" type="button" data-langdel="'+l.code+'">&#128465;</button>':'')+'</td></tr>'}).join("")+'</tbody></table>'+
   row("Fallback language",pseg("langs.fallback",langCodes(),langCodes().map(function(c){return c.toUpperCase()})),"An empty text in the visitor's language shows this language's words.")+
   '<div class="btnrow"><select class="u" id="langpick" style="width:auto">'+LANGPICK.filter(function(x){return langCodes().indexOf(x[0])<0}).map(function(x){return'<option value="'+x[0]+'">'+x[1]+'</option>'}).join("")+'</select><button class="ubtn sm" type="button" data-langadd="1">+ Add language</button></div>'+
   '<div class="note">A new language adds a field to every text of every part, every content field and every term, all empty (they show the fallback until translated), and a column to the Texts check.</div>';
  h+=fo("w1",'1 · Languages <span class="sub">· the wall\'s languages, for every text</span>',["ext","EXTENDED"],lg,1);
  var tb='<div class="note">One row = one idea. Its words are equal in the search: a point holding any of them is found by all of them (Synonym Groups). A row with a definition is also a glossary term: a card text that writes <code>[[any of its words]]</code> shows it underlined, and a tap shows the definition. A term may have its own small page, built from the same elements as a block\'s page (Read more). The Guide\'s Ask reads the same rows.</div>'+
   '<table class="tbl"><thead><tr><th style="width:34%">Words</th><th>Definition ('+S.lang.toUpperCase()+')</th><th style="width:170px">Used by</th></tr></thead><tbody>';
  S.terms.forEach(function(t){var u=termUses(t),def=L(t.def),ws=termWords(t);
    tb+='<tr class="pick'+(S.sel.term===t.id?" sel":"")+'" data-termpick="'+t.id+'"><td><b>'+esc(ws[0]||"(empty)")+'</b>'+(ws.length>1?' <span class="uses">· '+esc(ws.slice(1).join(", "))+'</span>':'')+'</td><td class="uses">'+(def?esc(def):"— (search only)")+'</td><td class="used"><span class="chip2">Search</span>'+(def?'<span class="chip2'+(u.n?'':' off')+'">Card · '+u.rows.length+'</span>':'')+(t.page.length?'<span class="chip2">Page</span>':'')+'<span class="chip2">Guide</span></td></tr>';
    if(S.sel.term===t.id||S.ownAll)tb+='<tr class="slot"><td colspan="3"><div class="fields">'+row("Words",'<input class="u" data-term="'+t.id+'|words" value="'+esc(t.words)+'">',"Comma-separated, in any language. The first is the row's name; every word links and searches alike.")+
      S.langs.list.map(function(l){return row("Definition · "+l.code.toUpperCase(),'<textarea class="u" rows="2" data-term="'+t.id+'|def|'+l.code+'">'+esc(t.def[l.code]||"")+'</textarea>',l.code==="en"?"Empty in every language: the row is for the search only.":"")}).join("")+
      (L(t.def)?termPageEditor(t):'')+
      '<div class="btnrow"><button class="del" type="button" data-termdel="'+t.id+'" '+(u.n?'disabled title="A card text still links it"':'')+'>Delete</button></div></div></td></tr>'});
  tb+='</tbody></table><div class="btnrow"><button class="ubtn sm" type="button" data-termadd="1">+ Add term</button></div>'+
   row("Try a word",'<input class="u" data-termtry="1" placeholder="e.g. corner tower">',"Shows what the search, the card and the Guide do with it.")+'<div class="note" id="termtry" aria-live="polite"></div>';
  h+=fo("w2",'2 · Terms <span class="sub">· the glossary and the search\'s synonyms, one list</span>',["new","NEW"],tb,1);
  var refs=locRefs(),miss=refs.filter(function(r){return missingOf(refObj(r.ref))>0}).length,ui=refs.filter(function(r){return r.kind==="Interface"}).length;
  var ck='<div class="note"><b>'+refs.length+'</b> texts: '+ui+' interface words and '+(refs.length-ui)+' content fields · <b'+(miss?' class="missing"':'')+'>'+miss+'</b> missing a language. Every row is the same value its part edits: change it here or there, both show it. Go to opens it in its part.</div>'+
   '<div class="filters"><input class="u" data-txq="1" placeholder="Find a text..." value="'+esc(S.ui.txq)+'">'+[["all","All"],["interface","Interface"],["content","Content"],["missing","Missing a language"],["new","New"],["retired","Retired ("+TXRET.length+")"]].map(function(f){return'<button class="chip" type="button" data-txf="'+f[0]+'" aria-pressed="'+(S.ui.txf===f[0])+'">'+f[1]+'</button>'}).join("")+'</div>'+
   '<div class="txscroll"><table class="tbl txt"><thead><tr><th style="width:32%">Text · lives in</th>'+langHead()+'<th style="width:52px"></th></tr></thead><tbody id="txrows">'+txRows()+'</tbody></table></div>'+
   '<div class="note">Outside the Editor: export / import the same rows for a translator (CSV or XLIFF), with a status per text (machine draft, reviewed).</div>';
  h+=fo("w3",'3 · Texts check <span class="sub">· every text of the wall in one list, for translators</span>',["new","NEW"],ck,1);
  h+=jsonBox("config.json · wall level (languages, terms)",wordsJson());
  return pipe("edWords.html",h)}
on("renderEditor.tab",function(){if(S.tab==="words")return edWords()});
on("refreshJsonV4.answer",function(){if(S.tab==="words"){var pre=ed.querySelector(".json pre");if(pre)pre.textContent=JSON.stringify(wordsJson(),null,2);return true}});
(function(){var tabs=document.querySelector(".utabs");if(tabs&&!tabs.querySelector('[data-tab="words"]')){var b=document.createElement("button");b.className="utab w";b.setAttribute("role","tab");b.type="button";b.dataset.tab="words";b.setAttribute("aria-selected","false");b.textContent="Languages & Terms";tabs.appendChild(b);
  b.addEventListener("click",function(){S.tab="words";renderEditor();show("story","r:r4")})}})();
/* ----- self-test: every text, demo field, term and term page must reach the phone (or say why not) ----- */
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(langs\\.|"));
