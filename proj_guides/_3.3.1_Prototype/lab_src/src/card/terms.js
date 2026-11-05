/* Terms: the glossary words in the text, their popups and pages (S.terms, gloss, termScreen). */
/* ----- Terms: the glossary and the search's synonym groups, one list; a term may have its own small page ----- */
S.terms=[{id:"t1",words:"Torreão, corner tower, torre de canto",def:T("The corner tower added to the river front in the 1580s.","A torre de canto acrescentada à frente ribeirinha na década de 1580."),
  page:[{kind:"picture",c:{img:"g3",cap:T("The Torreão at the corner of the river front, 1662.","O Torreão no canto da frente ribeirinha, 1662.")}},
   {kind:"paragraph",c:{text:T("Built for Philip I at the end of the 16th century, the tower stood at the corner of the palace, over the river. It fell with the palace in 1755.","Construído para Filipe I no fim do século XVI, o torreão erguia-se no canto do palácio, sobre o rio. Caiu com o palácio em 1755.")}}]},
 {id:"t2",words:"Pombaline, Pombalino, Pombal",def:T("The rebuilding of Lisbon after 1755, under the Marquis of Pombal.","A reconstrução de Lisboa depois de 1755, com o Marquês de Pombal."),page:[]},
 {id:"t3",words:"chapel, church, capela, igreja",def:T("",""),page:[]}];
S.sel.term=null;S.termOpen="t1";S.ui.txf="all";S.ui.txq="";
(function(){var r4=byId(S.story,"r4");if(r4){r4.c.text.en=r4.c.text.en.replace("the Torreão.","the [[Torreão]].");r4.c.text.pt=r4.c.text.pt.replace(", o Torreão.",", o [[Torreão]].")}})();
function termWords(t){return t.words.split(",").map(function(w){return w.trim()}).filter(Boolean)}
function termFor(word){var w=String(word).trim().toLowerCase();for(var i=0;i<S.terms.length;i++)if(termWords(S.terms[i]).some(function(x){return x.toLowerCase()===w}))return S.terms[i];return null}
var TERM_RE=/\[\[([^\[\]]+?)\]\]/g;
function markupTerm(inner){var bar=inner.indexOf("|");return{shown:(bar>=0?inner.slice(0,bar):inner).trim(),term:(bar>=0?inner.slice(bar+1):inner).trim()}}
function gloss(text){return pipe("gloss.html",esc(text).replace(TERM_RE,function(m,inner){var x=markupTerm(inner);if(!x.shown||!x.term)return m;var t=termFor(x.term),def=t?L(t.def):"";
  return def?'<span class="gterm" tabindex="0" data-gterm="'+t.id+'" data-gdef="'+esc(def)+'">'+x.shown+'</span>':x.shown}))}
function plainTerms(text){return String(text).replace(TERM_RE,function(m,inner){return markupTerm(inner).shown||m})}
function termUses(t){var n=0,rows=[];S.story.forEach(function(r){var txts=[];if(r.c&&r.c.text)txts.push(r.c.text);(r.slots||[]).forEach(function(s){if(s&&s.text)txts.push(s.text)});
  txts.forEach(function(x){Object.keys(x).forEach(function(lg){var s=x[lg]||"",m;TERM_RE.lastIndex=0;while((m=TERM_RE.exec(s))){if(termFor(markupTerm(m[1]).term)===t){n++;if(rows.indexOf(r.id)<0)rows.push(r.id)}}})})});return{n:n,rows:rows}}
function termScreen(){var t=byId(S.terms,S.termOpen)||S.terms[0];if(!t)return "";var ws=termWords(t);
  var h=topbar(TXH("term_crumb")+" &#8250; <b>"+esc(ws[0]||"")+"</b>")+'<div class="pg"><div class="blk"><div class="el-eyebrow">'+TXH("term_eyebrow")+'</div><h3 class="el-title">'+esc(ws[0]||"")+'</h3><p class="el-lede">'+esc(L(t.def))+'</p>';
  (t.page||[]).forEach(function(r,i){h+='<div class="item" data-ref="tp:'+i+'">'+renderEl(r.kind,defStyle(r.kind),r.c,i)+'</div>'});return h+'</div></div>'}
on("corpus.list",function(l){return l.map(function(c){return Object.assign({},c,{text:plainTerms(c.text)})})});
on("searchAnswer.q",function(q){var extra=[];S.terms.forEach(function(t){var ws=termWords(t);if(ws.some(function(w){return q.toLowerCase().indexOf(w.toLowerCase())>=0}))extra=extra.concat(ws)});return q+" "+extra.join(" ")});
ph.addEventListener("click",function(e){var old=ph.querySelector(".gpop"),o=e.target.closest("[data-termopen]");
  if(o){e.stopPropagation();if(old)old.remove();S.termOpen=o.dataset.termopen;S.view="term";renderPhone(false);return}
  var g=e.target.closest(".gterm");if(!g){if(old)old.remove();return}
  e.stopPropagation();if(old)old.remove();var t=byId(S.terms,g.dataset.gterm),p=document.createElement("div");p.className="gpop";
  p.innerHTML='<b>'+esc(g.textContent)+'</b><p>'+esc(g.dataset.gdef)+'</p>'+(t&&t.page&&t.page.length?'<button type="button" data-termopen="'+t.id+'">'+TXH("term_read_more")+' &#8250;</button>':'');
  var r=g.getBoundingClientRect(),pr=ph.getBoundingClientRect(),sc=pr.width/ph.offsetWidth||1;p.style.top=Math.round((r.bottom-pr.top)/sc+6)+"px";ph.appendChild(p)},true);
