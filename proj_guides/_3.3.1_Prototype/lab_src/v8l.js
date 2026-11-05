/* ===================== v8l: Audiences: Family / Explorer / Expert, the length, per-field overrides with fallback, the first-card screen, the looks per audience ===================== */
var AUDS=["family","explorer","expert"],LENS=["short","standard","deep"];
S.vis.aud="explorer";S.vis.len="";
S.aud={ask:true,seen:true,looks:{family:{life:"lively",len:"short",type:"up",orn:"stickers",corners:"round"},explorer:{life:"lively",len:"standard",type:"base",orn:"base",corners:"base"},expert:{life:"calm",len:"deep",type:"base",orn:"off",corners:"tight"}},hide:{family:{},explorer:{},expert:{"b:b_quiz":1}}};
COMP.s8onb=["Audiences","Who's exploring"];COMP.s8set=["Audiences","Audience in Settings"];COMP.s8look=["Audiences","Looks per audience"];
FOCUS.s8onb=["card",".hero","half"];FOCUS.s8set=["card",".hero","half"];FOCUS.s8look=["story",'[data-ref="r:r2"]',"full"];
function aud(){return AUDS.indexOf(S.vis.aud)>=0?S.vis.aud:"explorer"}
function lenNow(){return LENS.indexOf(S.vis.len)>=0?S.vis.len:S.aud.looks[aud()].len}
/* ----- overrides: stored apart from the texts (the texts keep only their languages), one entry per field and audience ----- */
S.ov={};
function hashOf(o){var s=JSON.stringify([o&&o.en||"",o&&o.pt||""]),h=0;for(var i=0;i<s.length;i++)h=(h*31+s.charCodeAt(i))|0;return String(h)}
/* the fields an audience can change: the story's texts and the guide's words, each with its default object */
function ovFields(){var out=[];S.story.forEach(function(r){if(r.c&&r.c.text&&typeof r.c.text==="object")out.push({id:"r:"+r.id,get:r.c.text,label:"Story · "+(KIND[r.kind]?KIND[r.kind].n:r.kind)+" · "+String(Lbase(r.c.text)).slice(0,38)+"…",ref:"r:"+r.id})});
  (S.fullSteps||S.guideSteps).forEach(function(st,j){if(st.text)out.push({id:"g:"+st.id,get:st.text,label:"Guide · step "+(j+1)+" · "+String(Lbase(st.text)).slice(0,34)+"…",ref:st.ref})});return out}
var OVMAP=null;function ovMap(){OVMAP=new Map();ovFields().forEach(function(f){OVMAP.set(f.get,f.id)})}
function ovEntry(id,a){return S.ov[id]&&S.ov[id][a]||null}
function ovState(id,a){var e=ovEntry(id,a);if(a==="explorer"||!e)return"inherited";if(e.source==="ai"&&!e.reviewed)return"draft";var f=ovFields().filter(function(x){return x.id===id})[0];return f&&e.basedOn!==hashOf(f.get)?"stale":"over"}
function setOv(id,a,text,source,reviewed){var f=ovFields().filter(function(x){return x.id===id})[0];S.ov[id]=S.ov[id]||{};S.ov[id][a]={text:text,source:source||"human",reviewed:reviewed!==false,basedOn:f?hashOf(f.get):""}}
/* the content in the visitor's language: the audience's own words for this field when it has them (reviewed), else the default's */
on("L.answer",function(o,f){if(o&&typeof o==="object"&&aud()!=="explorer"){if(!OVMAP)ovMap();var id=OVMAP.get(o);if(id){var e=ovEntry(id,aud());if(e&&!(e.source==="ai"&&!e.reviewed)&&e.text[S.lang])return e.text[S.lang]}}});
/* the demo point: Family and Expert words for the story's first paragraph and the guide's welcome; an AI draft to review; a stale one */
(function(){ovMap();
  setOv("r:r2","family",T("King Manuel thought his castle on the hill was too small. So around 1500 he built a new palace right by the river, and filled it with the riches the ships brought back from India!","O rei D. Manuel achava o castelo no monte pequeno demais. Por isso, por volta de 1500, construiu um palácio novo mesmo junto ao rio, e encheu-o com as riquezas que os navios traziam da Índia!"));
  setOv("r:r2","expert",T("Around 1500 Manuel I moved the court from the castle on the hill down to the river front, beside the docks, and paid for the new palace with the wealth of the India voyages: the crown now lived where its trade arrived.","Por volta de 1500, D. Manuel I mudou a corte do castelo no monte para a frente ribeirinha, junto às docas, e pagou o novo paço com a riqueza das viagens à Índia: a coroa passava a viver onde o seu comércio chegava."));
  setOv("g:g0","family",T("Hi! I'm Azu. Kings and queens lived right here, by the river, for a very long time. Want to see their palace?","Olá! Sou o Azu. Reis e rainhas viveram aqui, junto ao rio, durante muito tempo. Queres ver o palácio deles?"));
  setOv("g:g0","expert",T("The Ribeira Palace: the royal residence for about two and a half centuries, until the earthquake of 1755. Let us go through what is known of it.","O Paço da Ribeira: residência real durante cerca de dois séculos e meio, até ao terramoto de 1755. Vejamos o que se sabe dele."));
  setOv("r:r9","family",T("Then one morning in 1755 a huge earthquake shook Lisbon, and the palace was lost.","Até que, numa manhã de 1755, um grande terramoto abanou Lisboa, e o palácio perdeu-se."),"ai",false);
  setOv("r:r4","expert",T("Later kings kept adding to it; under Philip I the river front gained a new wing and its corner tower.","Os reis seguintes foram-lhe acrescentando partes; com Filipe I, a frente do rio ganhou uma ala nova e o seu torreão."));S.ov["r:r4"].expert.basedOn="an older default"})();
/* the guide's length: a step may be left out on Short, or told only on Deep */
(function(){var by=function(id){return byId(S.guideSteps,id)};if(by("g5"))by("g5").len="notshort";if(by("g8"))by("g8").len="notshort";var j=S.guideSteps.findIndex(function(s){return s.id==="g7"});
  if(j>=0&&!by("g7x"))S.guideSteps.splice(j+1,0,{id:"g7x",view:"story",ref:"r:r9",act:"show",len:"deep",text:T("What was lost with it: the library of some seventy thousand books and the royal archive, burnt in the fire that followed the earthquake.","O que se perdeu com ele: a biblioteca de cerca de setenta mil livros e o arquivo real, queimados no incêndio que se seguiu ao terramoto.")})})();
function stepFits(st){var l=lenNow();if(S.aud.hide[aud()][st.ref])return false;return st.len==="deep"?l==="deep":st.len==="notshort"?l!=="short":true}
var startTourV8l=startTour;startTour=function(){if(!S.howSaved&&!S.fullSteps){S.fullSteps=S.guideSteps;S.guideSteps=S.fullSteps.filter(stepFits)}ovMap();startTourV8l()};
var stopTourV8l=stopTour;stopTour=function(){stopTourV8l();if(S.fullSteps){S.guideSteps=S.fullSteps;S.fullSteps=null}};
/* ----- the card for an audience: its looks (tokens), its hidden blocks, Azu's liveliness ----- */
on("renderPhone.before",function(keep,cx){var a=cx.aud=aud();if(S.audApplied!==a){S.audApplied=a;S.guideCfg.life=S.aud.looks[a].life}ovMap()});on("renderPhone.after",function(keep,cx){var a=cx.aud;
  var lk=S.aud.looks[a];AUDS.forEach(function(x){ph.classList.toggle("aud-"+x,x===a&&x!=="explorer")});
  ph.dataset.type=lk.type;ph.dataset.orn=lk.orn;ph.dataset.corners=lk.corners;
  var hide=S.aud.hide[a]||{};scr.querySelectorAll("[data-ref]").forEach(function(el){var h=!!hide[el.dataset.ref];el.classList.toggle("audhidden",h)});
  var ask=S.aud.ask&&!S.aud.seen&&S.view==="card"&&S.stop!=="closed"&&S.stop!=="peek",ob=ph.querySelector(".onb");if(!ask&&ob)ob.remove();else if(ask&&!ob)onbShow()});
/* ----- the first card: "Who's exploring today?" One screen, three choices, Skip = Explorer ----- */
var AUDICON={family:'<svg viewBox="0 0 40 40" aria-hidden="true"><circle cx="14" cy="11" r="5"/><circle cx="28" cy="15" r="4"/><path d="M5 34c0-7 4-12 9-12s9 5 9 12M21 34c0-6 3-10 7-10s7 4 7 10"/></svg>',
  explorer:'<svg viewBox="0 0 40 40" aria-hidden="true"><circle cx="20" cy="20" r="14"/><path d="M25 15l-3 8-7 2 3-8z"/></svg>',
  expert:'<svg viewBox="0 0 40 40" aria-hidden="true"><circle cx="17" cy="17" r="10"/><path d="M24.5 24.5L34 34M12 17h10M17 12v10"/></svg>'};
function onbShow(){var o=ph.querySelector(".onb");if(o)o.remove();var d=document.createElement("div");d.className="onb";d.setAttribute("role","dialog");d.setAttribute("aria-modal","true");d.setAttribute("aria-label",TXT("aud_title"));
  d.innerHTML='<div class="onb-in"><b class="onb-t">'+TXH("aud_title")+'</b>'+AUDS.map(function(a){return'<button type="button" class="onb-c onb-'+a+'" data-onb="'+a+'" aria-pressed="'+(aud()===a)+'"><span class="onb-ic">'+AUDICON[a]+'</span><span class="onb-tx"><strong>'+TXH("aud_"+a)+'</strong><small>'+TXH("aud_"+a+"_line")+'</small></span></button>'}).join("")+
    '<button type="button" class="onb-skip" data-onb="skip">'+TXH("aud_skip")+'</button><small class="onb-note">'+TXH("aud_note")+'</small></div>';d.tabIndex=-1;ph.appendChild(d);d.focus({preventScroll:true})}
ph.addEventListener("click",function(e){var b=e.target.closest("[data-onb]");if(!b)return;e.stopPropagation();var a=b.dataset.onb;S.vis.aud=a==="skip"?"explorer":a;S.vis.len="";S.aud.seen=true;var o=ph.querySelector(".onb");if(o)o.remove();renderAll(true)},true);
/* ----- the visitor's Settings: who's exploring, and the length ----- */
on("settingsPanel.html",function(h){var opt=function(k,v,lab,cur){return'<button type="button" data-vis="'+k+'|'+v+'" aria-pressed="'+(cur===v)+'">'+esc(lab)+'</button>'};
  var add='<small>'+TXH("settings_aud")+'</small><div class="vo">'+AUDS.map(function(a){return opt("aud",a,TXT("aud_"+a),aud())}).join("")+'</div><small>'+TXH("settings_len")+'</small><div class="vo">'+LENS.map(function(l){return opt("len",l,TXT("len_"+l),lenNow())}).join("")+'</div>';
  var i=h.indexOf("<small>");return i<0?h:h.slice(0,i)+add+h.slice(i)});
/* a new audience starts from its own length */
document.addEventListener("click",function(e){var v=e.target.closest&&e.target.closest('[data-vis^="aud|"]');if(v)S.vis.len=""},true);
/* the preview bar: Preview as (the Lab only) */
var ptoolV8l=ptool;ptool=function(){var a=aud(),nx={explorer:"family",family:"expert",expert:"explorer"};return ptoolV8l()+'<button type="button" data-pt="aud" data-v="'+nx[a]+'" aria-pressed="'+(a!=="explorer")+'" title="'+TXH("pbar_tip_aud")+'">'+TXH("aud_"+a)+'</button>'};
document.addEventListener("click",function(e){var b=e.target.closest&&e.target.closest('[data-pt="aud"]');if(!b)return;e.stopPropagation();e.preventDefault();S.vis.aud=b.dataset.v;S.vis.len="";renderAll(true)},true);

/* ----- Editor: Detail Card > 7 · Audiences; Specific Marker > 4 · Audiences (coverage); the Guide script's length per step ----- */
var AUDN={family:"Family",explorer:"Explorer",expert:"Expert"};
on("edCard.html",function(h){var lk=S.aud.looks;
  var looks='<table class="tbl"><thead><tr><th style="width:24%"></th>'+AUDS.map(function(a){return'<th>'+AUDN[a]+(a==="explorer"?" (default)":"")+'</th>'}).join("")+'</tr></thead><tbody>'+
    [["life","Azu",["lively","calm"],["Lively","Calm"]],["len","Length",LENS,["Short","Standard","Deep"]],["type","Type",["base","up"],["Base","A step up"]],["orn","Ornaments",["stickers","base","off"],["Stickers","Base","Off"]],["corners","Corners",["round","base","tight"],["Round","Base","Tight"]]].map(function(r){
      return'<tr><td>'+r[1]+'</td>'+AUDS.map(function(a){return'<td>'+pseg("aud.looks."+a+"."+r[0],r[2],r[3])+'</td>'}).join("")+'</tr>'}).join("")+'</tbody></table>';
  var body='<div class="note">Who is exploring: one preset changes the words (where the point has them), the guide\'s length, Azu and a few looks. One design for all; a preset is a content preference, never an age, kept on the phone only.</div>'+
    row("Preview as",pseg("vis.aud",AUDS,["Family","Explorer","Expert"]),"The phone shows the card as this audience sees it (also on the preview bar).")+
    comp("s8onb","the first card asks once: Family, Explorer or Expert; Skip = Explorer",row("Ask on the first card",pseg("aud.ask",["true","false"],["On","Off"]),"The first time a card opens, one screen: three choices and Skip. Never again after an answer or a Skip.")+
      row("Try",'<button class="del" type="button" data-onbtry="1">Show it now</button>',""))+
    comp("s8set","in the visitor's Settings: who's exploring, and Short / Standard / Deep","")+
    comp("s8look","per audience: Azu, the length, the type, the ornaments, the corners",looks+'<div class="note">Contrast and tap sizes pass in every set (the same tokens, a few values changed).</div>');
  var sec=fo("s8",'7 · Audiences <span class="sub">· Family, Explorer, Expert</span>',["new","NEW"],body,1);
  var d=document.createElement("div");d.innerHTML=h;var g=d.querySelector('[data-fo="s7"]');if(g)g.insertAdjacentHTML("afterend",sec);else{var i=d.innerHTML.lastIndexOf('<div class="json">');d.innerHTML=d.innerHTML.slice(0,i)+sec+d.innerHTML.slice(i)}
  ["s8onb","s8set"].forEach(function(o){var f=innerOf(d,o);if(f)f.insertAdjacentHTML("beforeend",ownbar(o)+ownPanels(o))});
  return d.innerHTML});
var OVLAB={inherited:"Inherited",over:"Own words",stale:"Check: the default changed",draft:"AI draft · to review",hidden:"Hidden"};
function blockRefs(){var out=[];(S.card||[]).forEach(function(b){out.push({ref:"b:"+b.id,label:(b.kind||"Block")+" · "+b.id.replace(/^b_/,"")})});return out}
on("edMarker.html",function(h0){var d=document.createElement("div");d.innerHTML=h0;
  var fs=ovFields(),tot={family:0,expert:0},sel=S.sel.ov||"";
  var t='<div class="note">What each audience reads on this point. Explorer is the default; Family and Expert inherit it field by field until a field gets its own words. A tap on a cell opens it.</div><table class="tbl cov"><thead><tr><th>Field</th>'+AUDS.map(function(a){return'<th>'+AUDN[a]+'</th>'}).join("")+'</tr></thead><tbody>';
  fs.forEach(function(f){t+='<tr><td>'+esc(f.label)+'</td>'+AUDS.map(function(a){var st=a==="explorer"?"default":ovState(f.id,a);if(st!=="inherited"&&st!=="default")tot[a]++;
      return'<td><button type="button" class="covc '+st+(sel===f.id+"|"+a?" sel":"")+'" data-ovc="'+f.id+'|'+a+'">'+(st==="default"?"Default":OVLAB[st])+'</button></td>'}).join("")+'</tr>';
    if(sel.indexOf(f.id+"|")===0){var a=sel.split("|")[1];if(a!=="explorer")t+='<tr class="slot"><td colspan="4">'+ovEditor(f,a)+'</td></tr>'}});
  t+='<tr><td><b>Own words</b></td><td></td><td>'+tot.family+' of '+fs.length+'</td><td>'+tot.expert+' of '+fs.length+'</td></tr></tbody></table>';
  t+='<div class="note">Blocks shown to each audience:</div><table class="tbl cov"><thead><tr><th>Block</th>'+AUDS.map(function(a){return'<th>'+AUDN[a]+'</th>'}).join("")+'</tr></thead><tbody>'+
    blockRefs().map(function(b){return'<tr><td>'+esc(b.label)+'</td>'+AUDS.map(function(a){var h=!!S.aud.hide[a][b.ref];return'<td><button type="button" class="covc '+(h?"hidden":"default")+'" data-ovh="'+b.ref+'|'+a+'">'+(h?"Hidden":"Shown")+'</button></td>'}).join("")+'</tr>'}).join("")+'</tbody></table>';
  var sec=fo("m4","4 · Audiences",["new","NEW"],t,1);var i=d.innerHTML.lastIndexOf('<div class="json">');var html=i<0?d.innerHTML+sec:d.innerHTML.slice(0,i)+sec+d.innerHTML.slice(i);
  d.innerHTML=html;var st=S.sel.gstep&&byId(S.fullSteps||S.guideSteps,S.sel.gstep),slot=st&&d.querySelector('[data-pick="gstep"][data-id="'+cssq(st.id)+'"] + tr.slot td:last-child');
  if(slot)slot.insertAdjacentHTML("afterbegin",row("Length",'<select class="u" data-glen="'+st.id+'">'+[["","Every length"],["notshort","Not on Short"],["deep","Deep only"]].map(function(o){return'<option value="'+o[0]+'"'+((st.len||"")===o[0]?" selected":"")+'>'+o[1]+'</option>'}).join("")+'</select>',"Which lengths tell this step."));
  return d.innerHTML});
/* one override open: the default beside it, its words per language, Back to the default, Draft with AI (flags new names, dates and numbers), Reviewed */
function newTokens(draft,base){var have=(" "+[base.en,base.pt].join(" ")+" ").toLowerCase(),out=[];
  /* a number anywhere, or a capitalised word that does not start its sentence, which the default does not have */
  String(draft).split(/(?<=[.!?])\s+/).forEach(function(sen){(sen.match(/[\p{L}\p{N}][\p{L}\p{N}'’-]*/gu)||[]).forEach(function(w,i){if((/^\d/.test(w)||(i>0&&/^\p{Lu}/u.test(w)))&&have.indexOf(w.toLowerCase())<0&&out.indexOf(w)<0)out.push(w)})});return out}
function ovEditor(f,a){var e=ovEntry(f.id,a),st=ovState(f.id,a),txt=e?e.text:{en:"",pt:""};
  var flags=e&&e.source==="ai"?newTokens([txt.en,txt.pt].join(" "),f.get):[];
  return'<div class="ovx"><div class="ovcol"><small>Default (Explorer)</small><p>'+esc(f.get.en||"")+'</p><p class="uses">'+esc(f.get.pt||"")+'</p></div><div class="ovcol"><small>'+AUDN[a]+(e&&e.source==="ai"?" · AI draft":"")+'</small>'+
    '<textarea class="u" rows="3" data-ovt="'+f.id+'|'+a+'|en" placeholder="(inherits the default)">'+esc(txt.en||"")+'</textarea><textarea class="u" rows="3" data-ovt="'+f.id+'|'+a+'|pt" placeholder="(herda o default)">'+esc(txt.pt||"")+'</textarea>'+
    (flags.length?'<div class="ovflag" role="note">Not in the default: <b>'+flags.map(esc).join("</b>, <b>")+'</b>. Check each before it ships.</div>':"")+
    (st==="stale"?'<div class="ovflag">The default changed after these words were written.</div>':"")+
    '<div class="btnrow">'+(e?'<button class="ubtn sm" type="button" data-ovback="'+f.id+'|'+a+'">Back to the default</button>':"")+'<button class="ubtn sm" type="button" data-ovai="'+f.id+'|'+a+'">'+(S.ovBusy===f.id+"|"+a?"Drafting…":"Draft with AI")+'</button>'+
    (e&&e.source==="ai"?'<label class="ovrev"><input type="checkbox" data-ovrev="'+f.id+'|'+a+'"'+(e.reviewed?" checked":"")+'> Reviewed: it can ship</label>':"")+(st==="stale"?'<button class="ubtn sm" type="button" data-ovok="'+f.id+'|'+a+'">Still right</button>':"")+'</div></div></div>'}
var AUDBRIEF={family:"a family with children of about 6 to 12: warm and playful, short sentences, simple words",expert:"an expert visitor (a historian): precise and sober, dense, with the period's proper terms"};
function ovDraft(id,a){var f=ovFields().filter(function(x){return x.id===id})[0];if(!f)return;if(!sampleFn){S.ovNote="AI is not available here (it runs inside the published Lab).";renderEditor();return}
  S.ovBusy=id+"|"+a;renderEditor();
  var prompt="Rewrite this heritage text for "+AUDBRIEF[a]+". Use ONLY facts that are in the text: add no names, dates, numbers, places or events. Keep it about the same length or shorter. Return JSON only: {\"en\":\"...\",\"pt\":\"...\"} with pt in European Portuguese.\n\nEN: "+(f.get.en||"")+"\nPT: "+(f.get.pt||"");
  sampleFn(prompt,{modelTier:"quick"}).then(function(r){var j=null;try{j=JSON.parse(String(r.text).replace(/^[^{]*/,"").replace(/[^}]*$/,""))}catch(x){}
    S.ovBusy=null;if(j&&(j.en||j.pt))setOv(id,a,{en:j.en||"",pt:j.pt||""},"ai",false);else S.ovNote="The draft could not be read; try again.";renderEditor()}).catch(function(){S.ovBusy=null;S.ovNote="The AI could not answer; try again.";renderEditor()})}
ed.addEventListener("click",function(e){var t=e.target.closest("[data-ovc],[data-ovh],[data-ovback],[data-ovai],[data-ovok],[data-onbtry]");if(!t)return;e.stopPropagation();var q;
  if(t.dataset.onbtry){S.aud.seen=false;S.view="card";if(S.stop==="closed"||S.stop==="peek")S.stop="half";renderPhone(false);return}
  if(t.dataset.ovc){q=t.dataset.ovc;S.sel.ov=S.sel.ov===q?"":q;renderEditor();return}
  if(t.dataset.ovh){q=t.dataset.ovh.split("|");var h=S.aud.hide[q[1]];if(h[q[0]])delete h[q[0]];else h[q[0]]=1;renderAll(true);return}
  if(t.dataset.ovback){q=t.dataset.ovback.split("|");delete S.ov[q[0]][q[1]];renderAll(true);return}
  if(t.dataset.ovok){q=t.dataset.ovok.split("|");var f=ovFields().filter(function(x){return x.id===q[0]})[0];S.ov[q[0]][q[1]].basedOn=hashOf(f.get);renderEditor();return}
  if(t.dataset.ovai){q=t.dataset.ovai.split("|");ovDraft(q[0],q[1])}},true);
ed.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.ovt)return;e.stopImmediatePropagation();var q=t.dataset.ovt.split("|"),en=ovEntry(q[0],q[1]);
  if(!en){setOv(q[0],q[1],{en:"",pt:""},"human",true);en=ovEntry(q[0],q[1])}en.text[q[2]]=t.value;if(!en.text.en&&!en.text.pt)delete S.ov[q[0]][q[1]];clearTimeout(S.ovT);S.ovT=setTimeout(function(){renderPhone(true)},250)},true);
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset)return;if(t.dataset.ovrev){e.stopImmediatePropagation();var q=t.dataset.ovrev.split("|");ovEntry(q[0],q[1]).reviewed=t.checked;renderAll(true);return}
  if(t.dataset.glen){e.stopImmediatePropagation();var st=byId(S.fullSteps||S.guideSteps,t.dataset.glen);if(t.value)st.len=t.value;else delete st.len}},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("ctl\\.jump\\.","ctl\\.jump\\.|aud\\.ask|aud\\.looks\\.|aud\\.hide\\."));
