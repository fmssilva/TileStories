/* ===================== v7: every text on the part that draws it; demo content; the demo card; Languages & Terms; Global Scene ===================== */
/* ----- every UI text: the framework's real ones (CardStrings.asset) + the ones only the Lab has so far: [key, owner, where, en, pt, fate, part] ----- */
var REALSTR=__REALSTR__;
var LABSTR=[
 ["democard_title","s0card","The demo card's title, on its hero.","Demo card","Cartão demo","","Title"],
 ["democard_tag","s0card","The line over the demo card's title.","Every block once · not a marker","Todos os blocos uma vez · não é um marcador","","Line over the title"],
 ["test_demo_title","s0card","The tile at the top of the demo card that opens the Block Components demo, and that page's title.","Block Components demo","Demo dos Block Components","","Components tile title (and its page's title)"],
 ["test_demo_line","s0card","The second line of that tile.","Motion, interaction, effects, elements, patterns.","Movimento, interação, efeitos, elementos, padrões.","","Components tile line"],
 ["test_tile_tag","s0tile","The small tag over the tile's title.","TEST · NOT IN THE APP","TESTE · NÃO ESTÁ NA APP","","Tag"],
 ["test_tile_title","s0tile","The tile's title.","Demo card","Cartão demo","","Title"],
 ["test_tile_line","s0tile","The tile's second line.","Every block once, with its demo content.","Todos os blocos uma vez, com o conteúdo demo.","","Line"],
 ["pbar_label","s0bar","The bar's tag, before its buttons.","PREVIEW","PRÉ-VISUALIZAR","","Tag"],
 ["pbar_light","s0bar","The theme button while the phone is light.","Light","Claro","","Theme button (light)"],
 ["pbar_dark","s0bar","The theme button while the phone is dark.","Dark","Escuro","","Theme button (dark)"],
 ["pbar_motion","s0bar","The motion button while motion is on.","Motion","Movimento","","Motion button (on)"],
 ["pbar_still","s0bar","The motion button under Reduce Motion.","Still","Parado","","Motion button (reduced)"],
 ["pbar_focus","s0bar","The Focus button: the demo jumps to the part being edited.","Focus","Foco","","Focus button"],
 ["pbar_phone","s0bar","The screen button on a phone.","Phone","Telemóvel","","Screen button (phone)"],
 ["pbar_wide","s0bar","The screen button on a wide screen.","Wide","Largo","","Screen button (wide)"],
 ["pbar_tablet","s0bar","The screen button on a tablet.","Tablet","Tablet","","Screen button (tablet)"],
 ["peek_next_label","s1peek","At Peek, the line naming the next point's card a swipe up brings.","Next: ","Seguinte: ","","Next card line"],
 ["open_row_expand","s5expand","The words of a preview that expands in place.","See more","Ver mais","","Row label"],
 ["term_eyebrow","s5termpage","The line over a term's title on its page.","GLOSSARY","GLOSSÁRIO","","Line over the title"],
 ["term_read_more","s5term","The link under the definition that opens the term's page.","Read more","Ler mais","","Read more link"],
 ["wall_tip","g:11","Over the wall while no card is open.","Tap a marker to open its card","Toque num marcador para abrir o cartão","","Wall hint"],
 ["search_box","g:10s","The search box's placeholder.","Search","Pesquisar","","Placeholder"],
 ["search_filters","g:10f","The Filters button (its count is added after it).","Filters","Filtros","","Button label"],
 ["search_filters_name","g:10f","The Filters button's name for screen readers.","Show filters","Mostrar filtros","","Button name (screen readers)"],
 ["guide_start","s7hero","The button on the hero that starts the tour.","Start the guide","Visita com o guia","","Button label"],
 ["guide_block_title","s7block","The Guide block's title.","Meet the palace with the guide","Conheça o palácio com o guia","","Title"],
 ["guide_block_line","s7block","The Guide block's second line.","Narrated tour · 3 min · ask anything, any time","Visita narrada · 3 min · pergunte a qualquer momento","","Line"],
 ["guide_your_turn","s7coach","Under the captions while the guide waits for the visitor's gesture.","Your turn…","Experimente…","","Prompt under the captions"],
 ["guide_coach_flip","s7coach","The hint chip at a card that flips.","Tap to flip","Toque para virar","","Hint chip (flip)"],
 ["guide_coach_drag","s7coach","The hint chip at a before / after slider.","Drag across","Arraste","","Hint chip (drag)"],
 ["guide_coach_answer","s7coach","The hint chip at a quiz.","Tap an answer","Toque numa resposta","","Hint chip (quiz)"],
 ["guide_coach_try","s7coach","The hint chip anywhere else.","Try it","Experimente","","Hint chip (other)"],
 ["guide_ctrl_prev","s7tour","The previous-step button's name.","Previous","Anterior","","Previous button"],
 ["guide_ctrl_pause","s7tour","The pause button's name.","Pause","Pausa","","Pause button"],
 ["guide_ctrl_play","s7tour","The play button's name while paused.","Play","Continuar","","Play button"],
 ["guide_ctrl_next","s7tour","The next-step button's name.","Next","Seguinte","","Next button"],
 ["guide_ctrl_mute","s7tour","The sound button's name.","Sound","Som","","Sound button"],
 ["guide_ctrl_stop","s7tour","The stop button's name.","Stop the guide","Parar o guia","","Stop button"],
 ["guide_ask_button","s7tour","The Ask button of the tour's controls.","Ask","Perguntar","","Ask button"],
 ["guide_resume","s7resume","The button that resumes a paused tour.","Resume the tour","Retomar a visita","","Button label"],
 ["guide_ask_title","s7ask","The title of the Ask panel.","Ask the guide","Pergunte ao guia","","Title"],
 ["guide_ask_mode_ai","s7ask","Beside the title when the AI answers.","AI · answers only from this card","IA · responde só com este cartão","","Mode line (AI)"],
 ["guide_ask_mode_search","s7ask","Beside the title when the search answers.","Search · offline","Pesquisa · offline","","Mode line (search)"],
 ["guide_ask_close","s7ask","The panel's close button's name.","Close","Fechar","","Close button"],
 ["guide_ask_example","s7ask","The example question in an empty panel.","e.g. What happened to the palace after the earthquake?","Ex.: Como ficou o palácio depois do terramoto?","","Example question"],
 ["guide_ask_placeholder","s7ask","The question box's placeholder.","Type your question…","Escreva a sua pergunta…","","Question box"],
 ["guide_ask_mic","s7ask","The microphone button's name.","Speak your question","Diga a sua pergunta","","Microphone button"],
 ["guide_ask_send","s7ask","The send button.","Send","Enviar","","Send button"],
 ["guide_ask_stop","s7ask","The button that stops an answer.","Stop","Parar","","Stop button"],
 ["guide_not_found","s7ask","The answer when nothing on the card answers the question.","I could not find that on this card.","Não encontrei isso neste cartão.","","Answer: nothing found"],
 ["guide_thinking","s7ask","While the answer comes.","Thinking…","A pensar…","","Answer: thinking"],
 ["guide_no_mic","s7ask","When the phone gives no microphone.","The microphone is not available in this browser.","O microfone não está disponível neste browser.","","Microphone: unavailable"],
 ["reading_minutes","s5openrow","The reading time after the row's words ({0} = minutes).","{0} min","{0} min","","Reading time"],
 ["guide_duration","s7hero","The tour's length after the button's words ({0} = minutes).","{0} min","{0} min","","Tour length"],
 ["guide_ctrl_captions","s7tour","The captions button of the tour's controls.","CC","Legendas","","Captions button"],
 ["guide_ai_offline","s7ask","Before an answer when the AI cannot be reached and the search answered.","(AI unavailable here, offline search) ","(IA indisponível aqui, pesquisa offline) ","","Notice: AI unreachable"],
 ["guide_ai_error","s7ask","Before an answer when the AI failed ({0} = the reason).","(AI unavailable: {0}) ","(IA indisponível: {0}) ","","Notice: AI failed"],
 ["pbar_tip_theme","s0bar","The theme button's tooltip.","Light / dark","Claro / escuro","","Theme button tooltip"],
 ["pbar_tip_motion","s0bar","The motion button's tooltip.","Reduce Motion","Reduzir movimento","","Motion button tooltip"],
 ["pbar_tip_lang","s0bar","The language button's tooltip.","Language","Idioma","","Language button tooltip"],
 ["pbar_tip_focus","s0bar","The Focus button's tooltip.","Focus on what I edit","Focar no que edito","","Focus button tooltip"],
 ["pbar_tip_screen","s0bar","The screen button's tooltip.","Screen","Ecrã","","Screen button tooltip"],
 ["term_crumb","s5termpage","The word for the glossary in the term page's breadcrumb.","Glossary","Glossário","","Breadcrumb word"],
 ["carousel_prev","el:carousel","The carousel's previous-picture button (screen readers).","Previous picture","Imagem anterior","","Previous button name"],
 ["carousel_next","el:carousel","The carousel's next-picture button (screen readers).","Next picture","Imagem seguinte","","Next button name"],
 ["guide_listening","s7ask","While the microphone listens.","Listening…","A ouvir…","","Microphone: listening"]];
S.tx={};var TXO={},TXW={},TXF={},TXP={},TXNEW={},TXOWN={},TXRET=[];
REALSTR.concat(LABSTR).forEach(function(r){S.tx[r[0]]={en:r[3],pt:r[4]};TXW[r[0]]=r[2];TXF[r[0]]=r[5]||"";TXP[r[0]]=r[6]||"";TXOWN[r[0]]=r[1];
  if(r[1]==="retired")TXRET.push(r[0]);else(TXO[r[1]]=TXO[r[1]]||[]).push(r[0])});
LABSTR.forEach(function(r){TXNEW[r[0]]=1});
var TX0=JSON.stringify(S.tx);
function TXT(k){var o=S.tx[k];return o?L(o):k}
function TXH(k){return esc(TXT(k))}
/* ----- the wall's languages (Languages & Terms > Languages): every localized field shows one field per language; an empty one shows the fallback's ----- */
S.langs={list:[{code:"en",name:"English"},{code:"pt",name:"Português (Portugal)"}],fallback:"en"};
var LANGPICK=[["fr","Français"],["es","Español"],["de","Deutsch"],["it","Italiano"]];
function langCodes(){return S.langs.list.map(function(l){return l.code})}
L=function(o,f){if(o==null)return f==null?"":L(f);if(typeof o==="string")return o;return o[S.lang]||o[S.langs.fallback]||o.en||o.pt||""};
function fmt(t){var a=arguments;return String(t).replace(/\{(\d)\}/g,function(m,i){return a[+i+1]!=null?a[+i+1]:m})}
Object.defineProperty(UI,"read",{get:function(){var o={};langCodes().forEach(function(c){var w=S.tx.open_row_story[c];o[c]=w?fmt(S.tx.open_row_meta[c]||S.tx.open_row_meta.en,w,fmt(S.tx.reading_minutes[c]||S.tx.reading_minutes.en,4)):""});return o}});
Object.defineProperty(UI,"open",{get:function(){return S.tx.open_block_button}});
Object.defineProperty(UI,"demo",{get:function(){return S.tx.test_demo_title}});

/* ----- demo content of each element: what it shows in the Editor's previews and the demo card (never in the app) ----- */
FIELDS.timeline=[["items","litems","Events (date · words, one per line)"]];FIELDS.scrolly=[["items","litems","Steps (one per line)"]];
FIELDS.before_after=FIELDS.before_after.concat([["blabel","ltext","Before label"],["alabel","ltext","After label"]]);
S.demoC={title:{text:DEMO.title},eyebrow:{text:DEMO.eyebrow},heading:{text:DEMO.heading},lede:{text:DEMO.lede},paragraph:{text:DEMO.paragraph},
 callout:{title:DEMO.ctitle,text:DEMO.ctext,back:DEMO.back},pull_quote:{text:DEMO.quote,cite:DEMO.cite},list:{items:DEMO.list},
 picture:{img:"g3",cap:DEMO.piccap},picture_pair:{imgs:["before","g2"],cap:DEMO.paircap},carousel:{imgs:["g1","g2","g3"]},video:{img:"hero",cap:DEMO.vidcap},
 audio:{cap:T("Guided tour · 3:11","Visita guiada · 3:11")},animation:{file:"tile_bloom.lottie"},
 before_after:{before:"before",after:"g2",blabel:T("Before 1755","Antes de 1755"),alabel:T("Today","Hoje")},hotspot:{img:"g3"},model_3d:{img:"g1"},panorama:{img:"hero"},
 timeline:{items:{en:["c. 1500 · The court moves to the river","1580s · The Torreão rises","1755 · Earthquake, fire and tsunami"],pt:["c. 1500 · A corte muda-se para o rio","1580s · Ergue-se o Torreão","1755 · Terramoto, incêndio e maremoto"]}},
 scrolly:{items:{en:["1662: the palace along the river.","Before 1755: the Torreão and the opera.","Today: the square that took its place."],pt:["1662: o palácio ao longo do rio.","Antes de 1755: o Torreão e a ópera.","Hoje: a praça que ficou no seu lugar."]}}};
var DEMOC0=JSON.parse(JSON.stringify(S.demoC));
var renderElV6=renderEl;
renderEl=function(kind,style,c,i,still){if(!c||!Object.keys(c).length)c=S.demoC[kind]||{};var h=renderElV6(kind,style,c,i,still);return kind==="animation"&&S.view!=="demo"?h.replace(/<small>[\s\S]*?<\/small>/,""):h};

/* ----- the demo card: a card of its own with every block once, seeded once from the Ribeira Palace, then independent of every marker ----- */
S.demoCard=JSON.parse(JSON.stringify(S.card));
function demoCardScreen(){var keep=S.card,kt=S.test;S.card=S.demoCard;S.test={on:true,poi:"painting"};inDemoCard=true;var h;try{h=cardScreen()}finally{S.card=keep;S.test=kt;inDemoCard=false}
  h=h.replace(/<i title="([^"]*)">&#10005;<\/i>/,'<i title="$1" data-go="card" role="button" style="cursor:pointer">&#10005;</i>');
  return h.replace(/<small>[^<]*<\/small><h2>Ribeira Palace<\/h2>/,"<small>"+TXH("democard_tag")+"</small><h2>"+TXH("democard_title")+"</h2>")}

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
function gloss(text){return esc(text).replace(TERM_RE,function(m,inner){var x=markupTerm(inner);if(!x.shown||!x.term)return m;var t=termFor(x.term),def=t?L(t.def):"";
  return def?'<span class="gterm" tabindex="0" data-gterm="'+t.id+'" data-gdef="'+esc(def)+'">'+x.shown+'</span>':x.shown})}
function plainTerms(text){return String(text).replace(TERM_RE,function(m,inner){return markupTerm(inner).shown||m})}
function termUses(t){var n=0,rows=[];S.story.forEach(function(r){var txts=[];if(r.c&&r.c.text)txts.push(r.c.text);(r.slots||[]).forEach(function(s){if(s&&s.text)txts.push(s.text)});
  txts.forEach(function(x){Object.keys(x).forEach(function(lg){var s=x[lg]||"",m;TERM_RE.lastIndex=0;while((m=TERM_RE.exec(s))){if(termFor(markupTerm(m[1]).term)===t){n++;if(rows.indexOf(r.id)<0)rows.push(r.id)}}})})});return{n:n,rows:rows}}
function termScreen(){var t=byId(S.terms,S.termOpen)||S.terms[0];if(!t)return "";var ws=termWords(t);
  var h=topbar(TXH("term_crumb")+" &#8250; <b>"+esc(ws[0]||"")+"</b>")+'<div class="pg"><div class="blk"><div class="el-eyebrow">'+TXH("term_eyebrow")+'</div><h3 class="el-title">'+esc(ws[0]||"")+'</h3><p class="el-lede">'+esc(L(t.def))+'</p>';
  (t.page||[]).forEach(function(r,i){h+='<div class="item" data-ref="tp:'+i+'">'+renderEl(r.kind,defStyle(r.kind),r.c,i)+'</div>'});return h+'</div></div>'}
var corpusV6=corpus;corpus=function(){return corpusV6().map(function(c){return Object.assign({},c,{text:plainTerms(c.text)})})};
var searchAnswerV6=searchAnswer;
searchAnswer=function(q){var extra=[];S.terms.forEach(function(t){var ws=termWords(t);if(ws.some(function(w){return q.toLowerCase().indexOf(w.toLowerCase())>=0}))extra=extra.concat(ws)});return searchAnswerV6(q+" "+extra.join(" "))};
ph.addEventListener("click",function(e){var old=ph.querySelector(".gpop"),o=e.target.closest("[data-termopen]");
  if(o){e.stopPropagation();if(old)old.remove();S.termOpen=o.dataset.termopen;S.view="term";renderPhone(false);return}
  var g=e.target.closest(".gterm");if(!g){if(old)old.remove();return}
  e.stopPropagation();if(old)old.remove();var t=byId(S.terms,g.dataset.gterm),p=document.createElement("div");p.className="gpop";
  p.innerHTML='<b>'+esc(g.textContent)+'</b><p>'+esc(g.dataset.gdef)+'</p>'+(t&&t.page&&t.page.length?'<button type="button" data-termopen="'+t.id+'">'+TXH("term_read_more")+' &#8250;</button>':'');
  var r=g.getBoundingClientRect(),pr=ph.getBoundingClientRect(),sc=pr.width/ph.offsetWidth||1;p.style.top=Math.round((r.bottom-pr.top)/sc+6)+"px";ph.appendChild(p)},true);
var contentAtV6=contentAt;
contentAt=function(path){if(path.indexOf("demo:")===0){var k=path.slice(5);return S.demoC[k]=S.demoC[k]||{}}
  if(path.indexOf("termpage:")===0){var q=path.split(":"),t=byId(S.terms,q[1]),r=t&&t.page[+q[2]];return r?(r.c=r.c||{}):null}return contentAtV6(path)};
var modelSigV6=modelSig;modelSig=function(){var keep=S.view;S.view="term";var x=termScreen()+"#"+demoCardScreen()+"#"+(S.previewBar?ptool():"");S.view=keep;return modelSigV6()+x};

/* ----- the Editor is organised by UI element: each element the phone shows has ONE component foldout (its options, its Aa words, its Demo content) ----- */
var COMP={s0card:["Preview & Test","Demo card"],s0tile:["Preview & Test","Demo card tile"],s0bar:["Preview & Test","Preview bar"],
 s1a:["Card","Sheet"],s1peek:["Card","Next card peek"],s1mini:["Card","Mini player"],s1b:["Card","Look"],s1c:["Card","Sound"],s1m:["Card","Media"],
 s2cfg:["Sections","All sections"],s2start:["Sections","Section start"],s2div:["Sections","Divider"],s2tint:["Sections","Section tint"],s2next:["Sections","Next Up"],
 s5top:["Card Controls","Top Bar"],s5settings:["Card Controls","Settings panel"],s5bar:["Card Controls","Navigation Bar"],s5contents:["Card Controls","Contents sheet"],
 s5pager:["Card Controls","Section pager"],s5openrow:["Card Controls","Open Row"],s5expand:["Card Controls","Expand Row"],
 s5back:["Card Controls","Back button"],s5crumb:["Card Controls","Breadcrumb"],s5prevnext:["Card Controls","Previous / Next buttons"],s5loc:["Card Controls","Locate Button"],
 s5sticky:["Card Controls","Sticky Action"],s5term:["Card Controls","Glossary popup"],s5termpage:["Card Controls","Term page"],
 s7voice:["Guide","Narration"],s7hero:["Guide","Hero button"],s7block:["Guide","Guide block"],s7cap:["Guide","Captions"],s7coach:["Guide","Invite to try"],
 s7tour:["Guide","Tour controls"],s7resume:["Guide","Resume button"],s7mascot:["Guide","Mascot"],s7ask:["Guide","Ask panel"],"g:10":["Global Scene","Select, Filter & Search"],"g:10s":["Global Scene","Search box"],"g:10f":["Global Scene","Filters button"],"g:11":["Global Scene","Onboarding hints"]};
function ownerName(o){var p=o.split(":");if(p[0]==="el")return "Blocks & Components › Elements › "+(KIND[p[1]]?KIND[p[1]].n:p[1]);if(p[0]==="eg")return "Blocks & Components › Elements › "+p[1]+" (every one)";
  if(p[0]==="pt"){var pt=patternById(p[1]);return "Blocks & Components › Patterns › "+(pt?pt.name:"(missing)")}if(p[0]==="bd")return "Blocks & Components › Blocks › "+p[1];
  if(o==="retired")return "(retired)";var c=COMP[o];return c?c[0]+" › "+c[1]:o}
function ownerPath(o){var p=o.split(":");if(p[0]==="el")return["s3","s3e","eg:"+KIND[p[1]].c,o];if(p[0]==="eg")return["s3","s3e",o];if(p[0]==="pt")return["s3","s3p",o];
  if(p[0]==="bd")return["s3","s3b","bs:"+secOfKind(p[1]),o];if(o==="g:10s"||o==="g:10f")return["g:10",o];if(p[0]==="g")return[o];var c=o.match(/^s(\d)/);return c?["s"+c[1],o]:[o]}
function hasDemo(o){var p=o.split(":");if(p[0]==="el")return(FIELDS[p[1]]||[]).length>0;return p[0]==="bd"||p[0]==="pt"}
function missingOf(obj){return S.langs.list.filter(function(l){return !(obj&&obj[l.code])}).length}
function langHead(){return S.langs.list.map(function(l){return'<th>'+l.code.toUpperCase()+'</th>'}).join("")}
function txInputs(k){var v=S.tx[k],long=Object.keys(v).some(function(c){return String(v[c]||"").length>34});return S.langs.list.map(function(l){var a='class="u'+(v[l.code]?'':' miss')+'" data-tx="'+k+'|'+l.code+'" placeholder="'+esc(L(v))+' ('+S.langs.fallback.toUpperCase()+')" aria-label="'+esc(TXP[k]+" · "+l.code.toUpperCase())+'"';
  return'<td>'+(long?'<textarea rows="2" '+a+'>'+esc(v[l.code]||"")+'</textarea>':'<input '+a+' value="'+esc(v[l.code]||"")+'">')+'</td>'}).join("")}
/* Aa: the component's words, one row per word it shows, named by what it is on the phone; the (?) says where it shows */
function txPanel(o){var ks=TXO[o]||[];return '<div class="ownpanel"><table class="tbl txt"><thead><tr><th style="width:30%">On the phone</th>'+langHead()+'<th style="width:22px"></th></tr></thead><tbody>'+ks.map(function(k){
  return '<tr><td>'+(TXNEW[k]?'<span class="badge new">NEW</span> ':'')+esc(TXP[k]||"Its words")+(TXF[k]?'<span class="fate">'+esc(TXF[k])+'</span>':'')+'</td>'+txInputs(k)+'<td>'+help(TXW[k])+'</td></tr>'}).join("")+'</tbody></table></div>'}
function dmPanel(o){var p=o.split(":");
  if(p[0]==="el")return '<div class="ownpanel"><div class="sub2">Demo content</div>'+fieldsFor(p[1],S.demoC[p[1]]||{},"demo:"+p[1])+row("Reset",'<button class="ubtn sm" type="button" data-demoreset="'+p[1]+'">Back to the framework\'s demo</button>',"Demo content shows in the Editor\'s previews and on the demo card, never in the app. A marker\'s own content always wins.")+'</div>';
  if(p[0]==="bd"){var b=S.demoCard.filter(function(x){return x.kind===p[1]})[0];return '<div class="ownpanel">'+
    row("On the demo card",b?'<button class="ubtn sm" type="button" data-democard="'+b.id+'">Show it &#8250;</button>':'<span class="uses">Not on the Lab\'s demo card</span>',"A block\'s demo is its block on the demo card (Preview & Test › Demo card), with its own content.")+'</div>'}
  if(p[0]==="pt"){var pt=patternById(p[1]);return '<div class="ownpanel"><div class="btnrow"><span class="uses">Its elements\' demo content:</span> '+(pt?pt.rows.map(function(r){return'<button class="ubtn sm" type="button" data-goto="el:'+r.kind+'|dm">'+esc(KIND[r.kind].n)+' &#8250;</button>'}).join(""):"")+'</div></div>'}return ""}
function ownbar(o){var ks=TXO[o]||[],dm=hasDemo(o);if(!ks.length&&!dm)return "";
  var miss=ks.filter(function(k){return missingOf(S.tx[k])>0}).length,ot=S.ownAll||S.op["own:tx|"+o],od=S.ownAll||S.op["own:dm|"+o];
  return '<div class="ownbar">'+(ks.length?'<button class="ownb" type="button" data-own="tx|'+o+'" aria-pressed="'+!!ot+'"><b>Aa</b> '+ks.length+' word'+(ks.length>1?"s":"")+(miss?' <em>'+miss+' missing</em>':'')+'</button>':'')+
   (dm?'<button class="ownb" type="button" data-own="dm|'+o+'" aria-pressed="'+!!od+'"><b>&#9635;</b> Demo content</button>':'')+'</div>'}
function ownPanels(o){var ks=TXO[o]||[],dm=hasDemo(o),ot=S.ownAll||S.op["own:tx|"+o],od=S.ownAll||S.op["own:dm|"+o];return(ot&&ks.length?txPanel(o):"")+(od&&dm?dmPanel(o):"")}
/* move rows between foldouts by their label (the Lab's v4-v6 rows are reused, never redrawn) */
function takeRow(inner,label,relabel){if(!inner)return "";var rows=inner.querySelectorAll(":scope > .row");for(var i=0;i<rows.length;i++){var lb=rows[i].querySelector("label");if(lb&&lb.textContent===label){rows[i].remove();if(relabel)lb.textContent=relabel;return rows[i].outerHTML}}return ""}
function innerOf(d,id){return d.querySelector('[data-fo="'+cssq(id)+'"] > .inner')}
function comp(id,sub,body,badge){return fo(id,esc(COMP[id][1])+' <span class="sub">· '+sub+'</span>',badge||null,body||' ',2)}
var PLANNED=["ext","PLANNED"];
var edCardV6=edCard;
edCard=function(){var d=document.createElement("div");d.innerHTML=edCardV6();
  var s6=d.querySelector('[data-fo="s6"]');if(s6)s6.remove();
  /* 0 · Preview & Test */
  var s0=innerOf(d,"s0");if(s0)s0.innerHTML=
    comp("s0card","every block once with its demo content, and a tile to the Block Components page",row("Demo card",'<button class="ubtn sm" type="button" data-democard="">Open it &#8250;</button>',"Filled once with the Ribeira Palace\'s content, independent since: a block removed from a real point stays here. The tests read it, never a real point.")+
      row("Components page",'<button class="ubtn sm" type="button" data-democard="" data-then="demo">Open it &#8250;</button>',"Opened by the first tile of the demo card: every motion, interaction, effect, element and pattern, on real parts."))+
    comp("s0tile","on a marker's card, it opens the demo card",row("Shown",pseg("test.on",["true","false"],["On","Off"]),"Scene and Play only.")+row("On marker",sel("test.poi",POIS,S.test.poi),"The marker whose card shows the tile.",true))+
    comp("s0bar","floating on the phone: theme, motion, language, focus, screen",row("Shown",pseg("previewBar",["true","false"],["Shown","Hidden"]),"Scene and Play only. Its Focus button makes the phone jump to the part being edited."));
  /* 1 · Card: Media holds the media folders */
  var s1=innerOf(d,"s1");if(s1)s1.insertAdjacentHTML("beforeend",comp("s1m","where the card's media files live",
    row("Media folder",'<input class="u" value="LivingRoom/CardMedia" readonly>',"Where this wall's card pictures, sounds and models live.")+
    row("Own stock media",'<input class="u" value="(none: the framework\'s)" readonly>',"Optional: this wall's own library of stock media. Every picture, sound or model field's Pick default… offers it first, then the framework's (azulejo pictures, a plain panel, a map plan, a chime, an ambient sound, a tiled 360 room).")));
  var sh=innerOf(d,"s1a"),pk=takeRow(sh,"Peek next card","Shown"),snd=innerOf(d,"s1c"),ka=takeRow(snd,"Keep audio when closed","Shown");
  var f1a=d.querySelector('[data-fo="s1a"]');if(f1a)f1a.insertAdjacentHTML("afterend",comp("s1peek","at Peek, the next point's card a swipe up brings",pk));
  var f1c=d.querySelector('[data-fo="s1c"]');if(f1c)f1c.insertAdjacentHTML("afterend",comp("s1mini","keeps the audio playing on the wall after the card closes",ka));
  [["s1a","how the card opens and where it stops"],["s1b","light or dark, the layout of the sections, the band under the hero"],["s1c","when the audio guide and a video meet"]].forEach(function(x){var sm=d.querySelector('[data-fo="'+x[0]+'"] > summary');if(sm)sm.innerHTML=sm.innerHTML.replace(/^(Sheet|Look|Sound)/,'$1 <span class="sub">· '+x[1]+'</span>')});
  var lay=s1&&s1.querySelector('[data-fo="s1b"] > .inner');if(lay){var lr=lay.querySelectorAll(":scope > .row");lr.forEach(function(r){var lb=r.querySelector("label");if(lb&&lb.textContent==="Layout"){var hb=r.querySelector(".help");if(hb)hb.dataset.help="How the sections are laid out: one long scroll (default) or one section at a time. Pages adds the Section pager (Card Controls)."}})}
  /* 2 · Sections: each element of a section's look is its own component */
  var c2=innerOf(d,"s2cfg");if(c2){var st=takeRow(c2,"Start","Style"),dv=takeRow(c2,"Divider","Style"),co=takeRow(c2,"Colour"),it=takeRow(c2,"Intensity"),ti=takeRow(c2,"Tint depth (pt)");
    takeRow(c2,"__none__");var lk=c2.querySelector(":scope > .row label");if(lk&&lk.textContent==="Look")lk.textContent="Style";var f2=d.querySelector('[data-fo="s2cfg"]');f2.querySelector("summary").innerHTML='All sections <span class="sub">· their look on the card and their order</span>';
    f2.insertAdjacentHTML("afterend",comp("s2start","the band where each section begins",st)+comp("s2div","the line between two blocks of a section",dv)+comp("s2tint","the colour at a section's top",co+it+ti)+
      comp("s2next","at a section's end: the next block, and a button to it","",PLANNED))}
  var s2n=innerOf(d,"s2");if(s2n){var nt=s2n.querySelector(":scope > .note");if(nt)nt.remove()}
  /* 5 · Card Controls: one component per UI element, ordered as on the screen */
  var s5=innerOf(d,"s5");if(s5){var pg=innerOf(d,"s5page"),cr=takeRow(pg,"Breadcrumb","Shown"),pn=takeRow(pg,"Previous / Next","Shown"),orw=takeRow(pg,"Open Row","Default look"),tb=innerOf(d,"s5top"),stk=innerOf(d,"s5sticky");
    var sk=takeRow(stk,"Sticky action","Shown"),nbi=innerOf(d,"s5bar"),ji=innerOf(d,"s5jump");if(nbi){var nn=nbi.querySelector(":scope > .note");if(nn)nn.remove()}
    if(ji)ji.querySelectorAll(":scope > .row label").forEach(function(l){if(l.textContent==="Jump motion")l.textContent="A tap moves the card";if(l.textContent==="A jump opens the card to")l.textContent="After a tap, the card is at"});
    var navb=(nbi?nbi.innerHTML:"")+(ji?ji.innerHTML:"");
    s5.innerHTML=comp("s5top","the round buttons and the title at the card's top",tb?tb.innerHTML:"")+
     comp("s5settings","opened by the Top Bar's settings button: language and light / dark","")+
     comp("s5bar","one item per section; a tap moves the card there",navb)+
     comp("s5contents","every section and block in one list, with filter chips, opened from the bar","",PLANNED)+
     comp("s5pager","the counter and chevrons at the top, with Layout = Pages","",PLANNED)+
     comp("s5openrow","the line on a preview that opens its page",orw)+
     comp("s5expand","the line on a preview whose Tap is Expand here","")+
     comp("s5back","the ‹ at a page's top","")+
     comp("s5crumb","point › section › page, at a page's top",cr)+
     comp("s5prevnext","the neighbouring pages, at a page's foot",pn)+
     comp("s5loc","lowers the card and shows the point on the wall",(innerOf(d,"s5loc")||{}).innerHTML||"")+
     comp("s5sticky","the button pinned at the card's foot",sk)+
     comp("s5term","a term's definition, after a tap on an underlined word","")+
     comp("s5termpage","a term's own page, opened by Read more","")}
  /* 7 · Guide: one component per element of the tour */
  var s7=innerOf(d,"s7");if(s7){var g=function(l,r){return takeRow(s7,l,r)},rules=s7.querySelector(":scope > .note.fixed");rules=rules?row("Fixed rules",'<span class="uses">pause, take-over, captions, Reduce Motion</span>',rules.textContent.replace(/^Fixed rules:\s*/,"")):"";
    var vo=g("Voice")+g("Speed")+g("Music bed"),he=g("Start from the hero","Shown"),bl=g("Guide block","Shown"),ca=g("Captions","Shown")+g("Highlight what is read"),co2=g("Invite to try","Shown"),ma=g("Mascot","Character"),as=g("Ask the guide","Answers from");
    s7.innerHTML=rules+comp("s7voice","the voice, its speed and the music under it",vo)+comp("s7hero","the button on the hero that starts the tour",he)+comp("s7block","the tile that starts the tour, first in About",bl)+
     comp("s7cap","the words being read, at the bottom",ca)+comp("s7coach","at a block to try, the guide waits for the gesture",co2)+
     comp("s7tour","progress, previous, pause, next, sound, captions, Ask and stop","")+comp("s7resume","after the visitor takes over, the button that carries on","")+comp("s7mascot","a small tile that follows the block being told",ma)+comp("s7ask","questions by text or voice",as)}
  /* every owner gets its Aa and its Demo content at the top of its own foldout */
  var owners={};Object.keys(TXO).forEach(function(o){owners[o]=1});KINDS.forEach(function(K){owners["el:"+K.k]=1});LIB.forEach(function(r){owners["bd:"+r[1]]=1});S.patterns.forEach(function(p){owners["pt:"+p.id]=1});
  Object.keys(owners).forEach(function(o){var f=innerOf(d,o);if(!f)return;f.insertAdjacentHTML("beforeend",ownbar(o)+ownPanels(o))});
  [["s4m","entrances, transitions, scroll and emphasis, picked by elements and blocks"],["s4i","flip, hold, compare, swipe, tilt, hotspots"],["s4f","edges, dividers, light and moving effects"]].forEach(function(x){var sm=d.querySelector('[data-fo="'+x[0]+'"] > summary');if(sm&&sm.innerHTML.indexOf('class="sub"')<0)sm.innerHTML=sm.innerHTML.replace(/^(Motion|Interaction|Effects)/,'$1 <span class="sub">· '+x[1]+'</span>')});
  var en=innerOf(d,"s3e");if(en){var n1=en.querySelector(":scope > .note");if(n1)n1.remove()}
  var g7=d.querySelector('[data-fo="s7"] > summary');if(g7)g7.innerHTML=g7.innerHTML.replace("7 · Guide","6 · Guide");
  d.querySelectorAll(".badge").forEach(function(b){if(/^WAS |^ONE PLACE/.test(b.textContent))b.textContent="EXTENDED"});
  d.querySelectorAll(".row").forEach(function(rw){var lb=rw.querySelector("label");if(!lb||rw.querySelector(".help"))return;var t=ROWHELP[lb.textContent];if(!t)return;var sp=rw.lastElementChild;if(sp&&sp.tagName==="SPAN")sp.outerHTML=help(t)});
  return d.innerHTML.replace(/ 1 block kinds</g," 1 block<").replace(/ block kinds</g," blocks<")};
var ROWHELP={"Tap outside closes":"A tap outside the card closes it.","Shown":"Shows or hides this part.","Position":"Where the bar sits on the screen.",
 "Hide while scrolling":"The bar slides away while the visitor reads down, and comes back on a scroll up.","Float":"The pin bobs gently; still under Reduce Motion.",
 "A jump opens the card to":"The stop the card rises to when a bar item is tapped at Peek.","Size":"1 = the size of the chosen section's disc on the bar.",
 "Speed":"1 = the voice's normal speed.","Labels":"What each bar item shows.","Icon colour":"The colour of the bar's icons.","Colour":"Its colour.",
 "Raised depth":"+1 a bell, 0 flat, below 0 a cradle."};
function changedTexts(){var o={},z=JSON.parse(TX0);Object.keys(S.tx).forEach(function(k){if(JSON.stringify(S.tx[k])!==JSON.stringify(z[k]))o[k]=S.tx[k]});return o}

/* ----- the phone's own words for the test tile, the preview bar and the demo card ----- */
var inDemoCard=false;
function testTile(){return inDemoCard?'<button class="testtile" type="button" data-go="demo"><div><b>'+TXH("test_tile_tag")+'</b><strong>'+TXH("test_demo_title")+'</strong><span>'+TXH("test_demo_line")+'</span></div><em>&#8250;</em></button>'
  :'<button class="testtile" type="button" data-go="democard"><div><b>'+TXH("test_tile_tag")+'</b><strong>'+TXH("test_tile_title")+'</strong><span>'+TXH("test_tile_line")+'</span></div><em>&#8250;</em></button>'}
S.previewBar=true;S.wallTip=true;
ptool=function(){var nx={phone:"land",land:"tablet",tablet:"phone"},nm={phone:"pbar_phone",land:"pbar_wide",tablet:"pbar_tablet"};
  return '<span class="lbl">'+TXH("pbar_label")+'</span><button type="button" data-pt="dark" aria-pressed="'+S.dark+'" title="'+TXH("pbar_tip_theme")+'">'+(S.dark?"&#9790; "+TXH("pbar_dark"):"&#9728; "+TXH("pbar_light"))+'</button>'+
   '<button type="button" data-pt="reduce" aria-pressed="'+S.reduce+'" title="'+TXH("pbar_tip_motion")+'">'+(S.reduce?TXH("pbar_still"):TXH("pbar_motion"))+'</button>'+
   '<button type="button" data-pt="lang" aria-pressed="false" title="'+TXH("pbar_tip_lang")+'">'+S.lang.toUpperCase()+'</button>'+
   '<button type="button" data-pt="focusOn" aria-pressed="'+S.focusOn+'" title="'+TXH("pbar_tip_focus")+'">'+TXH("pbar_focus")+'</button>'+
   '<button type="button" data-pt="screen" data-v="'+nx[S.screen]+'" aria-pressed="false" title="'+TXH("pbar_tip_screen")+'">'+TXH(nm[S.screen])+'</button>'};
var renderPhoneV6b=renderPhone;renderPhone=function(keep){renderPhoneV6b(keep);var pt=document.getElementById("ptool");if(pt)pt.style.display=S.previewBar?"":"none"};

/* ----- Global Scene: the top level only ----- */
var GSECS=[["Labels, Text & Fonts","the marker labels' words, sizes and fonts"],["Marker","shape, icon and the category table"],["Orientation","how markers, labels and badges face the camera"],
 ["Badge","the second sign on a marker, and its table"],["Outline","the ring around a marker, and its levels"],["Effects","glow, pulse and the other marker effects, with their Test grid"],
 ["Hierarchy Levels","what shows at each level of the wall"],["LOD","how much detail shows with the distance"],["Zoom","how far the visitor zooms, and its steps"],
 ["Displacement","how labels make room for each other"],["Select, Filter & Search","selection, search, filters, results, minimap, voice, keyword fields"]];
edGlobal=function(){var h='<div class="note">The top level only: the wall\'s markers and how a visitor finds them.</div>';
  var gc=function(id,sub){return fo(id,esc(COMP[id][1])+' <span class="sub">· '+sub+'</span>',null,ownbar(id)+' '+ownPanels(id),2)};
  GSECS.forEach(function(g,i){var srch=i===GSECS.length-1,body=srch?row("Synonym Groups",'<button class="ubtn sm" type="button" data-goloc="term:t3">Languages &amp; Terms › Terms &#8250;</button>',"One list with the card\'s glossary. Keyword Fields stay here: only the search reads them.")+gc("g:10s","where the visitor types a search")+gc("g:10f","opens the filters, with how many are on"):' ';
   h+=fo("g:"+i,esc(g[0])+' <span class="sub">· '+esc(g[1])+'</span>',srch?["ext","EXTENDED"]:["","EXISTING"],body,1)});
  h+=fo("g:11",'Onboarding hints <span class="sub">· the first-use hints over the wall</span>',["new","NEW"],row("Wall hint",pseg("wallTip",["true","false"],["Shown","Hidden"]),"Over the wall until the visitor opens a card.")+ownbar("g:11")+ownPanels("g:11"),1);return h};

/* ----- the fourth tab: Languages & Terms ----- */
/* every localized value of the wall, interface and content, as references to the ONE place it is stored */
function locRefs(){var out=[];
  Object.keys(TXO).forEach(function(o){TXO[o].forEach(function(k){out.push({ref:"tx:"+k,kind:"Interface",label:TXP[k],where:ownerName(o),go:o})})});
  S.sections.forEach(function(s,i){out.push({ref:"sec:"+i,kind:"Content",label:"Section name",where:"Sections › "+s.name.en,go:"sec:"+s.id})});
  S.story.forEach(function(r,i){var add=function(kind,c,slot){(FIELDS[kind]||[]).forEach(function(f){if(f[1]==="ltext"&&c&&c[f[0]])out.push({ref:"story:"+r.id+":"+slot+":"+f[0],kind:"Content",label:(KIND[kind]?KIND[kind].n:kind)+" · "+f[2],where:"Ribeira Palace › Story page › row "+(i+1),go:"story:"+r.id})})};
    if(r.pattern){var p=patternById(r.pattern);if(p)p.rows.forEach(function(pr,j){add(pr.kind,r.slots[j],"s"+j)})}else add(r.kind,r.c,"c")});
  S.terms.forEach(function(t){out.push({ref:"term:"+t.id,kind:"Content",label:"Definition",where:"Terms › "+(termWords(t)[0]||""),go:"term:"+t.id})});
  S.guideSteps.forEach(function(st,j){if(st.text)out.push({ref:"gstep:"+st.id,kind:"Content",label:"Guide step "+(j+1),where:"Ribeira Palace › Guide script",go:"gstep:"+st.id})});
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
function txRows(){var q=S.ui.txq.trim().toLowerCase(),out="",f=S.ui.txf;
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
  return h}
var renderEditorV6=renderEditor;
renderEditor=function(){if(S.tab!=="words")return renderEditorV6();lastRender=performance.now();document.querySelectorAll(".utab").forEach(function(t){t.setAttribute("aria-selected",String(t.dataset.tab===S.tab))});ed.innerHTML=edWords()};
var refreshJsonV6=refreshJsonV4;
refreshJsonV4=function(){if(S.tab==="words"){var pre=ed.querySelector(".json pre");if(pre)pre.textContent=JSON.stringify(wordsJson(),null,2);return}refreshJsonV6()};
(function(){var tabs=document.querySelector(".utabs");if(tabs&&!tabs.querySelector('[data-tab="words"]')){var b=document.createElement("button");b.className="utab w";b.setAttribute("role","tab");b.type="button";b.dataset.tab="words";b.setAttribute("aria-selected","false");b.textContent="Languages & Terms";tabs.appendChild(b);
  b.addEventListener("click",function(){S.tab="words";renderEditor();show("story","r:r4")})}})();

/* ----- events ----- */
function pathShow(path){if(path.indexOf("termpage:")===0){S.termOpen=path.split(":")[1];show("term");return}var k=path.split("|")[0].slice(5),st=defStyle(k);show("demo",st?"s:"+st.id:null)}
function goDemo(id){S.tab="marker";if(id==="guide"){S.op.m3=true;renderEditor();show("card");return}}
function goOwner(spec,still){var q=spec.split("|"),o=q[0];S.tab=o.indexOf("g:")===0?"global":"card";S.ui.q="";S.ui.cat="All";ownerPath(o).forEach(function(id){S.op[id]=true});if(q[1]==="dm")S.op["own:dm|"+o]=true;renderEditor();
  var f=ed.querySelector('[data-fo="'+cssq(o)+'"]');if(f){if(!still)f.scrollIntoView({block:"start"});f.classList.add("cur")}if(q[1]==="dm"&&o.indexOf("el:")===0)pathShow("demo:"+o.slice(3))}
ed.addEventListener("click",function(e){var t=e.target;
  var ob=t.closest("[data-own]");if(ob){e.stopPropagation();e.preventDefault();var k="own:"+ob.dataset.own,pp=ob.dataset.own.split("|");S.op[k]=!S.op[k];renderEditor();if(S.op[k]&&pp[1].indexOf("el:")===0)pathShow("demo:"+pp[1].slice(3));return}
  var da=t.closest('[data-asset^="demo:"],[data-asset^="termpage:"]');if(da){e.stopPropagation();var q=da.dataset.asset.split("|"),c=contentAt(q[0]);c[q[1]]=q[2];renderAll(true);pathShow(q[0]);return}
  var ds=t.closest('[data-assets^="demo:"],[data-assets^="termpage:"]');if(ds){e.stopPropagation();var q2=ds.dataset.assets.split("|"),c2=contentAt(q2[0]),arr=(c2[q2[1]]||[]).slice(),ix=arr.indexOf(q2[2]);if(ix>=0){if(arr.length>1)arr.splice(ix,1)}else{arr.push(q2[2]);if(q2[0]==="demo:picture_pair"&&arr.length>2)arr.shift()}c2[q2[1]]=arr;renderAll(true);pathShow(q2[0]);return}
  var dr=t.closest("[data-demoreset]");if(dr){e.stopPropagation();var dk=dr.dataset.demoreset;S.demoC[dk]=JSON.parse(JSON.stringify(DEMOC0[dk]||{}));renderAll(true);pathShow("demo:"+dk);return}
  var dc=t.closest("[data-democard]");if(dc){e.stopPropagation();if(dc.dataset.then){show(dc.dataset.then);return}show("democard",dc.dataset.democard?"b:"+dc.dataset.democard:null);return}
  var dg=t.closest("[data-demogo]");if(dg){e.stopPropagation();goDemo(dg.dataset.demogo);return}
  var gt=t.closest("[data-goto]");if(gt){e.stopPropagation();goOwner(gt.dataset.goto);return}
  var gl=t.closest("[data-goloc]");if(gl){e.stopPropagation();goLoc(gl.dataset.goloc);return}
  var tf=t.closest("[data-txf]");if(tf){e.stopPropagation();S.ui.txf=tf.dataset.txf;renderEditor();return}
  var la=t.closest("[data-langadd]");if(la){e.stopPropagation();var pk=document.getElementById("langpick");if(!pk||!pk.value)return;var nm=LANGPICK.filter(function(x){return x[0]===pk.value})[0];S.langs.list.push({code:nm[0],name:nm[1]});renderAll(true);return}
  var ld=t.closest("[data-langdel]");if(ld){e.stopPropagation();var cd=ld.dataset.langdel;S.langs.list=S.langs.list.filter(function(l){return l.code!==cd});if(S.lang===cd)S.lang="en";if(S.langs.fallback===cd)S.langs.fallback="en";renderAll(true);return}
  var ta=t.closest("[data-termadd]");if(ta){e.stopPropagation();var nt={id:nid("t"),words:"",def:T("",""),page:[]};S.terms.push(nt);S.sel.term=nt.id;renderEditor();return}
  var tdl=t.closest("[data-termdel]");if(tdl){e.stopPropagation();if(tdl.disabled)return;S.terms=S.terms.filter(function(x){return x.id!==tdl.dataset.termdel});S.sel.term=null;renderAll(true);return}
  var tpa=t.closest("[data-tpadd]");if(tpa){e.stopPropagation();var qa=tpa.dataset.tpadd.split("|"),tt=byId(S.terms,qa[0]);tt.page.push({kind:qa[1],c:JSON.parse(JSON.stringify(DEMOC0[qa[1]]||{}))});S.termOpen=tt.id;renderEditor();show("term");return}
  var tpr=t.closest("[data-tprm]");if(tpr){e.stopPropagation();var qr=tpr.dataset.tprm.split("|"),tr=byId(S.terms,qr[0]);tr.page.splice(+qr[1],1);S.termOpen=tr.id;renderEditor();show("term");return}
  var tp=t.closest("[data-termpick]");if(tp&&!t.closest("input,textarea,button")){e.stopPropagation();S.sel.term=S.sel.term===tp.dataset.termpick?null:tp.dataset.termpick;if(S.sel.term){S.termOpen=S.sel.term}renderEditor();return}},true);
ed.addEventListener("input",function(e){var t=e.target;
  if(t.dataset.tx){e.stopImmediatePropagation();var q=t.dataset.tx.split("|");S.tx[q[0]][q[1]]=t.value;t.classList.toggle("miss",!t.value);renderPhone(true);refreshJsonV4();return}
  if(t.dataset.loc){e.stopImmediatePropagation();var ql=t.dataset.loc.split("|"),o=refObj(ql[0]);if(!o)return;o[ql[1]]=t.value;t.classList.toggle("miss",!t.value);renderPhone(true);refreshJsonV4();return}
  if(t.dataset.term){e.stopImmediatePropagation();var q2=t.dataset.term.split("|"),tm=byId(S.terms,q2[0]);if(!tm)return;if(q2[1]==="words")tm.words=t.value;else tm.def[q2[2]]=t.value;renderPhone(true);refreshJsonV4();return}
  if(t.dataset.termtry){e.stopImmediatePropagation();var tw=document.getElementById("termtry");if(tw)tw.textContent=tryWord(t.value);return}
  if(t.dataset.txq){e.stopImmediatePropagation();S.ui.txq=t.value;var tb=document.getElementById("txrows");if(tb)tb.innerHTML=txRows();return}},true);
ed.addEventListener("change",function(e){var t=e.target;if(t.dataset.tx||t.dataset.term){var y=window.scrollY;renderEditor();window.scrollTo(0,y)}},true);
ed.addEventListener("focusin",function(e){var f=e.target.dataset&&e.target.dataset.field;if(f&&(f.indexOf("demo:")===0||f.indexOf("termpage:")===0))pathShow(f)});

/* ----- self-test: every text, demo field, term and term page must reach the phone (or say why not) ----- */
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(langs\\.|wallTip|"));
var TX_EXPECT={reading_minutes:1,guide_duration:1,pbar_tip_theme:1,pbar_tip_motion:1,pbar_tip_lang:1,pbar_tip_focus:1,pbar_tip_screen:1,term_crumb:1,test_tile_tag:1,test_tile_title:1,test_tile_line:1,test_demo_line:1,democard_title:1,democard_tag:1,pbar_label:1,pbar_light:1,pbar_motion:1,pbar_focus:1,pbar_phone:1,term_eyebrow:1,close:1,visitor_settings:1,page_back:1,gallery_name:1,page_previous:1,page_next:1,locate_label:1,open_block_button:1,open_row_story:1,fun_fact_label:1,
 before_label:1,after_label:1,model_3d_hint:1,panorama_360_drag_hint:1,audio_play:1,test_demo_title:1,open_row_expand:1,guide_start:1,guide_block_title:1,guide_block_line:1};
var TX_DOM={peek_next_label:"shown at Peek",show_on_wall_button:"shown on the sticky button",wall_tip:"shown over the wall",audio_pause:"after a tap on Play",term_read_more:"shown in the glossary popup",pbar_dark:"the bar in dark mode",pbar_still:"the bar under Reduce Motion",pbar_wide:"the bar on a wide screen",pbar_tablet:"the bar on a tablet"};
function selfTestWords(){var res=[],real={renderEditor:renderEditor,renderPhone:renderPhone,renderAll:renderAll,show:show,playRef:playRef,flashRef:flashRef};
  S.ui.level=9;S.op={};S.ui.single=false;S.ownAll=true;S.ui.txf="content";
  ["card","words","global"].forEach(function(tab){S.tab=tab;real.renderEditor();var base=JSON.stringify(S);
    renderEditor=function(){};renderPhone=function(){};renderAll=function(){};show=function(v){S.view=v};playRef=function(){};flashRef=function(){};
    var acts=[];function lg(s){return langCodes().indexOf(s)>=0?s:null}
    ed.querySelectorAll("[data-tx]").forEach(function(x){var p=x.dataset.tx.split("|");acts.push({el:x,key:"tx:"+x.dataset.tx,tx:p[0],lang:p[1],v:"Zz"+p[0],kind:"input"})});
    ed.querySelectorAll("[data-loc]").forEach(function(x){var p=x.dataset.loc.split("|");acts.push({el:x,key:"loc:"+x.dataset.loc,lang:lg(p[1]),v:"Zz",kind:"input"})});
    ed.querySelectorAll('[data-field^="demo:"],[data-field^="termpage:"]').forEach(function(x){var p=x.dataset.field.split("|");acts.push({el:x,key:"demo:"+x.dataset.field,lang:lg(p[2]),v:p[3]==="lines"?"Zz1\nZz2\nZz3":"Zz",kind:"input"})});
    ed.querySelectorAll('[data-asset^="demo:"],[data-asset^="termpage:"]').forEach(function(x){if(x.getAttribute("aria-pressed")!=="true")acts.push({el:x,key:"demo:"+x.dataset.asset,kind:"click"})});
    ed.querySelectorAll('[data-assets^="demo:"],[data-assets^="termpage:"]').forEach(function(x){acts.push({el:x,key:"demo:"+x.dataset.assets,kind:"click"})});
    ed.querySelectorAll("[data-term]").forEach(function(x){acts.push({el:x,key:"term:"+x.dataset.term,lang:lg(x.dataset.term.split("|")[2]),v:"Zz",kind:"input"})});
    acts.forEach(function(a){var B=JSON.parse(base);Object.keys(B).forEach(function(k){S[k]=B[k]});if(a.lang)S.lang=a.lang;var before=modelSig(),state=JSON.stringify(S),lab=a.key;
      try{if(a.kind==="click")a.el.click();else{a.el.value=a.v;a.el.dispatchEvent(new Event("input",{bubbles:true}))}}catch(err){res.push({tab:tab,key:a.key,v:a.v,label:lab,result:"ERROR "+err.message});return}
      var changed=modelSig()!==before,stateChanged=JSON.stringify(S)!==state,k=a.tx,r;
      if(changed)r="changes the phone";else if(!stateChanged)r="NOT WIRED (state unchanged)";
      else if(k&&/^s7/.test(TXOWN[k])&&!TX_EXPECT[k])r="behaviour (shown during the tour)";else if(k&&TX_DOM[k])r="behaviour ("+TX_DOM[k]+")";
      else if(k&&!TX_EXPECT[k])r="elsewhere (its part is not drawn on the Lab's card)";
      else if(/^loc:gstep:/.test(a.key))r="behaviour (shown during the tour)";
      else if(/^(term:|loc:term:)/.test(a.key)&&!termUses(byId(S.terms,a.key.replace(/^loc:/,"").split(":")[1].split("|")[0])||{words:""}).n)r="elsewhere (no card text links this term)";
      else r="NO EFFECT (state changes, phone does not)";
      res.push({tab:tab,key:a.key,v:a.v||"",label:lab,result:r})});
    renderEditor=real.renderEditor;renderPhone=real.renderPhone;renderAll=real.renderAll;show=real.show;playRef=real.playRef;flashRef=real.flashRef});
  S.ownAll=false;S.ui.txf="all";return res}
var selfTestV6=selfTest;selfTest=function(){var a=selfTestV6(),b=selfTestWords();return a.concat(b)};
var showSelfTestV6=showSelfTest;
showSelfTest=function(res){showSelfTestV6(res);var box=document.getElementById("selftest"),els=res.filter(function(r){return/^elsewhere/.test(r.result)}).length,
  cnt=function(re){var x=res.filter(function(r){return re.test(r.key)});return[x.length,x.filter(function(r){return r.result==="changes the phone"}).length]},tx=cnt(/^tx:/),lc=cnt(/^loc:/),dm=cnt(/^demo:/);
  box.insertAdjacentHTML("beforeend",'<p>Texts on their parts: <b>'+tx[1]+'</b> of '+tx[0]+' fields change the phone; the rest belong to parts the Lab does not draw (<b>'+els+'</b> elsewhere) or show during the tour. Content in the Texts check: <b>'+lc[1]+'</b> of '+lc[0]+'. Demo content and term pages: <b>'+dm[1]+'</b> of '+dm[0]+'.</p>');
  var j=JSON.parse(box.dataset.json||"{}");j.elsewhere=els;j.texts={fields:tx[0],ok:tx[1]};j.content={fields:lc[0],ok:lc[1]};j.demo={fields:dm[0],ok:dm[1]};box.dataset.json=JSON.stringify(j)};

var demoScreenV6=demoScreen;demoScreen=function(){return demoScreenV6().replace('data-go="card"','data-go="democard"')};

/* ----- opening a component shows that element on the phone, marked for a moment ----- */
var FOCUS={s0card:["democard",".hero"],s0tile:["card",".testtile","full"],s0bar:[null,"#ptool"],s1a:["card",".hero","half"],s1peek:["card",".peeknext","peek"],
 s1b:["card",".hero","full"],s1c:["card",'[data-ref="b:b_audio"]',"full"],s1mini:["card",'[data-ref="b:b_audio"]',"full"],
 s2cfg:["card",".gsec","full"],s2start:["card",'[data-ref="sec:stories"]',"full"],s2div:["card",".gsec","full"],s2tint:["card",".gsec","full"],
 s5top:["card",".chrome","full"],s5settings:["card",".chrome i:last-child","full"],s5bar:["card","#navbar","full"],s5openrow:["card",'[data-ref="b:b_story"] .open',"full"],
 s5expand:["card",'[data-ref="b:b_info"] .open',"full"],s5back:["story",".topbar button"],s5crumb:["story",".topbar span"],s5prevnext:["story",".pfoot"],
 s5loc:["card",".locpin","half"],s5sticky:["card",".sticky","half"],s5term:["story",".gterm"],s5termpage:["term",".el-title"],
 s7voice:["card",".gblock","full"],s7hero:["card",".gstart","full"],s7block:["card",".gblock","full"],s7cap:["card",".gblock","full"],s7coach:["card",".gblock","full"],
 s7tour:["card",".gblock","full"],s7resume:["card",".gblock","full"],s7mascot:["card",".gblock","full"],s7ask:["card",".gblock","full"],
 "g:10":["card",".walltip","closed"],"g:11":["card",".walltip","closed"],w2:["story",".gterm"]};
function focusPhone(view,sel,stop){if(view){var same=S.view===view&&(!stop||S.stop===stop);if(stop)S.stop=stop;S.view=view;if(!same)renderPhone(false)}
  setTimeout(function(){var el=ph.querySelector(sel)||document.querySelector(sel);if(!el)return;
    if(scr.contains(el)){var y=el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-90;scr.scrollTo({top:Math.max(0,y),behavior:S.reduce?"auto":"smooth"})}
    document.body.dataset.lastfocus=sel+"|found|"+(scr.contains(el)?Math.round(el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop):"overlay");el.classList.remove("pfocus");void el.offsetWidth;el.classList.add("pfocus");setTimeout(function(){el.classList.remove("pfocus")},1800)},80)}
ed.addEventListener("toggle",function(e){var d=e.target;if(!d.dataset||!d.dataset.fo||!d.open||performance.now()-lastRender<150)return;var id=d.dataset.fo,f=FOCUS[id],k=id.split(":");
  if(f){focusPhone(f[0],f[1],f[2]);return}
  if(k[0]==="bd"){var b=S.card.filter(function(x){return x.kind===k[1]})[0];if(b){focusPhone("card",'[data-ref="b:'+b.id+'"]',"full");return}var c=S.demoCard.filter(function(x){return x.kind===k[1]})[0];if(c)focusPhone("democard",'[data-ref="b:'+c.id+'"]');return}
  if(k[0]==="sec"){focusPhone("card",'[data-ref="sec:'+k[1]+'"]',"full");return}
  if(k[0]==="eg"){var K=KINDS.filter(function(x){return x.c===k[1]})[0];if(K){var st=defStyle(K.k);if(st)show("demo","s:"+st.id)}}},true);
