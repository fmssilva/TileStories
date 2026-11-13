/* Every text on the part that draws it: the parts' names (COMP), the Texts and Demo panels of each part, the owner bar, the Editor's sections 0 to 5 (Preview & Test, Card Controls). */
/* ----- the Editor is organised by UI element: each element the phone shows has ONE component foldout (its options, its Aa words, its Demo content) ----- */
var COMP={s5viewer:["Card Controls","Picture viewer"],s5player:["Card Controls","Video player"],s5chip:["Card Controls","Placed model chip"],s5hints:["Card Controls","Hints"],s5states:["Card Controls","Loading & states"],s8onb:["Audiences","Who’s exploring"],s8set:["Audiences","Audience in Settings"],s8look:["Audiences","Looks per audience"],s8lens:["Audiences","Lenses"],s7dock:["Guide","Guide panel"],s7cam:["Guide","Ask about the view"],s0card:["Preview & Test","Demo card"],s0tile:["Preview & Test","Demo card tile"],s0bar:["Preview & Test","Preview bar"],
 s1a:["Card","Sheet"],s1peek:["Card","Next card peek"],s1mini:["Card","Mini player"],s1b:["Card","Look"],s1c:["Card","Sound"],s1m:["Card","Media"],
 s2cfg:["Sections","All sections"],s2start:["Sections","Section start"],s2div:["Sections","Divider"],s2tint:["Sections","Section tint"],s2next:["Sections","Next Up"],
 s5top:["Card Controls","Top Bar"],s5settings:["Card Controls","Settings panel"],s5bar:["Card Controls","Navigation Bar"],s5contents:["Card Controls","Contents sheet"],
 s5pager:["Card Controls","Section pager"],s5openrow:["Card Controls","Open Row"],s5expand:["Card Controls","Expand Row"],
 s5back:["Card Controls","Back button"],s5crumb:["Card Controls","Breadcrumb"],s5prevnext:["Card Controls","Previous / Next buttons"],s5loc:["Card Controls","Locate Button"],
 s5sticky:["Card Controls","Sticky Action"],s5term:["Card Controls","Glossary popup"],s5termpage:["Card Controls","Term page"],
 s7voice:["Guide","Narration"],s7hero:["Guide","Hero button"],s7block:["Guide","Guide block"],s7cap:["Guide","Captions"],s7coach:["Guide","Invite to try"],
 s7tour:["Guide","The tour › Guide panel › Tour controls"],s7resume:["Guide","Continue the tour"],s7mascot:["Guide","The mascot"],s7ask:["Guide","Ask the mascot › What the visitor sees"],
 s7agent:["Guide","Ask the mascot › What the visitor sees › the answers"],"g:10":["Global Scene","Select, Filter & Search"],"g:10s":["Global Scene","Search box"],"g:10f":["Global Scene","Filters button"],"g:11":["Global Scene","Onboarding hints"]};
function ownerName(o){var p=o.split(":");if(p[0]==="el")return "Blocks & Components › Elements › "+(KIND[p[1]]?KIND[p[1]].n:p[1]);if(p[0]==="eg")return "Blocks & Components › Elements › "+p[1]+" (every one)";
  if(p[0]==="pt"){var pt=patternById(p[1]);return "Blocks & Components › Patterns › "+(pt?pt.name:"(missing)")}if(p[0]==="bd")return "Blocks & Components › Blocks › "+p[1];
  if(o==="retired")return "(retired)";var c=COMP[o];return c?c[0]+" › "+c[1]:o}
function ownerPath(o){if(guideOwns(o))return guidePath(o);var p=o.split(":");if(p[0]==="el")return["s3","s3e","eg:"+KIND[p[1]].c,o];if(p[0]==="eg")return["s3","s3e",o];if(p[0]==="pt")return["s3","s3p",o];
  if(p[0]==="bd")return["s3","s3b","bs:"+secOfKind(p[1]),o];if(o==="g:10s"||o==="g:10f")return["g:10",o];if(p[0]==="g")return[o];var c=o.match(/^s(\d)/);return c?["s"+c[1],o]:[o]}
function hasDemo(o){var p=o.split(":");if(p[0]==="el")return(FIELDS[p[1]]||[]).length>0;return p[0]==="bd"||p[0]==="pt"}
function missingOf(obj){return S.langs.list.filter(function(l){return !(obj&&obj[l.code])}).length}
function langHead(){return S.langs.list.map(function(l){return'<th>'+l.code.toUpperCase()+'</th>'}).join("")}
function txInputs(k){var v=S.tx[k],long=Object.keys(v).some(function(c){return String(v[c]||"").length>34});return S.langs.list.map(function(l){var a='class="u'+(v[l.code]?'':' miss')+'" data-tx="'+k+'|'+l.code+'" placeholder="'+esc(L(v))+' ('+S.langs.fallback.toUpperCase()+')" aria-label="'+esc(TXP[k]+" · "+l.code.toUpperCase())+'"';
  return'<td>'+(long?'<textarea rows="2" '+a+'>'+esc(v[l.code]||"")+'</textarea>':'<input '+a+' value="'+esc(v[l.code]||"")+'">')+'</td>'}).join("")}
/* Aa: the component's words, one row per word it shows, named by what it is on the phone; the (?) says where it shows (only: some of
   them, the ones a part does not draw itself: a guidance item's other words, round 14) */
function txPanel(o,only){var ks=only||TXO[o]||[];return pipe("txPanel.html",'<div class="ownpanel"><table class="tbl txt"><thead><tr><th style="width:30%">On the phone</th>'+langHead()+'<th style="width:22px"></th></tr></thead><tbody>'+ks.map(function(k){
  return '<tr><td>'+(TXNEW[k]?'<span class="badge new">NEW</span> ':'')+esc(TXP[k]||"Its words")+(TXF[k]?'<span class="fate">'+esc(TXF[k])+'</span>':'')+'</td>'+txInputs(k)+'<td>'+help(TXW[k])+'</td></tr>'}).join("")+'</tbody></table></div>')}
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
/* move rows between foldouts by their label (the rows the Editor already draws are reused, never redrawn) */
function takeRow(inner,label,relabel){if(!inner)return "";var rows=inner.querySelectorAll(":scope > .row");for(var i=0;i<rows.length;i++){var lb=rows[i].querySelector("label");if(lb&&lb.textContent===label){rows[i].remove();if(relabel)lb.textContent=relabel;return rows[i].outerHTML}}return ""}
function innerOf(d,id){return d.querySelector('[data-fo="'+cssq(id)+'"] > .inner')}
function comp(id,sub,body,badge){return fo(id,esc(COMP[id][1])+' <span class="sub">· '+sub+'</span>',badge||null,body||' ',2)}
var PLANNED=["ext","PLANNED"];
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
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
    var vo=g("Sources")+g("Speed")+g("Music bed")+g("Music volume")+g("Under the voice")+g("Track")+g("Credit")+g("Playing now")+g("Visitor can choose the music"),he=g("Start from the hero","Shown"),bl=g("Guide block","Shown"),ca=g("Captions","Shown")+g("Highlight what is read"),co2=g("Invite to try","Shown"),ma=g("Mascot","Character"),as=g("Ask the guide","Answers from");
    s7.innerHTML=rules+comp("s7voice","the voice, its speed and the music under it",vo)+comp("s7hero","the button on the hero that starts the tour",he)+comp("s7block","the tile that starts the tour, first in About",bl)+
     comp("s7cap","the words being read, at the bottom",ca)+comp("s7coach","at a block to try, the guide waits for the gesture",co2)+
     comp("s7tour","progress, previous, pause, next, sound, captions, Ask and stop","")+comp("s7resume","one way back into a stopped tour: this visit at its sentence, a later visit after a recap","")+comp("s7mascot","a small tile that follows the block being told",ma)+comp("s7ask","questions by text or voice",as)}
  /* every owner gets its Aa and its Demo content at the top of its own foldout */
  var owners={};Object.keys(TXO).forEach(function(o){owners[o]=1});KINDS.forEach(function(K){owners["el:"+K.k]=1});LIB.forEach(function(r){owners["bd:"+r[1]]=1});S.patterns.forEach(function(p){owners["pt:"+p.id]=1});
  Object.keys(owners).forEach(function(o){var f=innerOf(d,o);if(!f)return;f.insertAdjacentHTML("beforeend",ownbar(o)+ownPanels(o))});
  [["s4m","entrances, transitions, scroll and emphasis, picked by elements and blocks"],["s4i","flip, hold, compare, swipe, tilt, hotspots"],["s4f","edges, dividers, light and moving effects"]].forEach(function(x){var sm=d.querySelector('[data-fo="'+x[0]+'"] > summary');if(sm&&sm.innerHTML.indexOf('class="sub"')<0)sm.innerHTML=sm.innerHTML.replace(/^(Motion|Interaction|Effects)/,'$1 <span class="sub">· '+x[1]+'</span>')});
  var en=innerOf(d,"s3e");if(en){var n1=en.querySelector(":scope > .note");if(n1)n1.remove()}
  d.querySelectorAll(".badge").forEach(function(b){if(/^WAS |^ONE PLACE/.test(b.textContent))b.textContent="EXTENDED"});
  d.querySelectorAll(".row").forEach(function(rw){var lb=rw.querySelector("label");if(!lb||rw.querySelector(".help"))return;var t=ROWHELP[lb.textContent];if(!t)return;var sp=rw.lastElementChild;if(sp&&sp.tagName==="SPAN")sp.outerHTML=help(t)});
  return d.innerHTML.replace(/ 1 block kinds</g," 1 block<").replace(/ block kinds</g," blocks<")});
var ROWHELP={"Tap outside closes":"A tap outside the card closes it.","Shown":"Shows or hides this part.","Position":"Where the bar sits on the screen.",
 "Hide while scrolling":"The bar slides away while the visitor reads down, and comes back on a scroll up.","Float":"The pin bobs gently; still under Reduce Motion.",
 "A jump opens the card to":"The stop the card rises to when a bar item is tapped at Peek.","Size":"1 = the size of the chosen section's disc on the bar.",
 "Speed":"1 = the voice's normal speed.","Labels":"What each bar item shows.","Icon colour":"The colour of the bar's icons.","Colour":"Its colour.",
 "Raised depth":"+1 a bell, 0 flat, below 0 a cradle."};
function changedTexts(){var o={},z=JSON.parse(TX0);Object.keys(S.tx).forEach(function(k){if(JSON.stringify(S.tx[k])!==JSON.stringify(z[k]))o[k]=S.tx[k]});return o}
/* ----- events ----- */
function pathShow(path){if(path.indexOf("termpage:")===0){S.termOpen=path.split(":")[1];show("term");return}var k=path.split("|")[0].slice(5),st=defStyle(k);show("demo",st?"s:"+st.id:null)}
function goDemo(id){S.tab="marker";if(id==="guide"){S.op.m3=true;renderEditor();show("card");return}}
function goOwner(spec,still){var q=spec.split("|"),o=q[0],pth=ownerPath(o);S.tab=o.indexOf("g:")===0?"global":guideOwns(o)?"guide":"card";S.ui.q="";S.ui.cat="All";pth.forEach(function(id){S.op[id]=true});if(q[1]==="dm")S.op["own:dm|"+o]=true;renderEditor();
  /* a foldout taken apart (the Guide tab, round 14): its words' place, the path's end */
  var f=ed.querySelector('[data-fo="'+cssq(o)+'"]')||ed.querySelector('[data-fo="'+cssq(pth[pth.length-1])+'"]');if(f){if(!still)f.scrollIntoView({block:"start"});f.classList.add("cur")}if(q[1]==="dm"&&o.indexOf("el:")===0)pathShow("demo:"+o.slice(3))}
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
/* the Preview bar's Shown: the bar on the phone (Hidden: the card's top, where it was) */
rowDemos({"previewBar":{stop:"half",wait:0,ring:function(){return S.previewBar?"#ptool":".chrome"}}});
