/* The self-test: every Editor control is worked and the phone's signature compared (selfTest, modelSig, BEHAVIOUR, showSelfTest); the #probe handle for the headless checks. */
/* the part a control edits = the foldout it sits in (fixes editing a second open motion / effect / interaction) */
function setCtx(el){var d=el.closest("details[data-fo]");while(d){var f=d.dataset.fo,k=f.split(":");
  if(k[0]==="mo"){S.sel.motion=k[1];return}if(k[0]==="fx"){S.sel.effect=k[1];return}if(k[0]==="in"){S.sel.inter=k[1];return}if(k[0]==="bd"){S.sel.blockDef=k[1];return}
  d=d.parentElement&&d.parentElement.closest("details[data-fo]")}}
["click","input","change"].forEach(function(t){ed.addEventListener(t,function(e){if(e.target.closest("[data-set],[data-num],[data-sel],[data-text]"))setCtx(e.target)},true)});
function curOf(key){var p=key.split("."),st=styleById(S.sel.style);
  if(key[0]==="@")return getPath(key.slice(1));
  if(p[0]==="motion")return(motionById(S.sel.motion)||{})[p[1]];if(p[0]==="o")return st&&st.o[p[1]];if(p[0]==="fx")return(effectById(S.sel.effect)||{o:{}}).o[p[1]];
  if(p[0]==="inter")return(interById(S.sel.inter)||{o:{}}).o[p[1]];if(p[0]==="cb")return(byId(S.card,S.sel.cblock)||{})[p[1]];
  if(key==="style.uses")return st&&st.o.uses;if(p[0]==="style")return st&&st[p[1]];if(p[0]==="prow")return(patternById(p[1])||{rows:[]}).rows[+p[2]]&&patternById(p[1]).rows[+p[2]].style;
  return{family:S.family,replay:S.replay,focusOn:S.focusOn,previewLang:S.lang,"test.poi":S.test.poi,"m.poi":S.m&&S.m.poi||"painting"}[key]}
/* ----- self-test: every Editor control must change what the phone shows ----- */
var BEHAVIOUR=/^@?(inter\.|m\.page|blockDefs\.[^.]+\.chrome|cardCfg\.(tapOutside|keepAudio|audioOverlap|peekNext|enabled)|fb\.|ctl\.jump\.|ctl\.bar\.hide|ui\.|test\.poi|focusOn|previewLang)/;
/* the phone's signature for the self-test: each feature's piece is made before the older ones (latest first) and appended after them */
function modelSig(){var cx={};emitLatestFirst("modelSig.before",cx);var keep=S.view,out="";["card","demo","story","gallery"].forEach(function(v){S.view=v;out+=(v==="card"?cardScreen()+navbar():v==="demo"?demoScreen():v==="story"?storyScreen():galleryScreen())+"#"});S.view=keep;
  return pipe("modelSig.sig",out+JSON.stringify({loc:S.ctl.locate,sticky:S.ctl.sticky,bar:[S.ctl.bar.on,S.ctl.bar.pos],openAt:stopTop(S.cardCfg.openAt),half:S.cardCfg.halfMax,band:S.cardCfg.topBand,top:S.ctl.top,dark:S.dark,lang:S.lang,peek:S.cardCfg.peekNext,on:S.cardCfg.enabled}),cx)}
/* the page opened as the self-test (#selftest, #selftest=<tab>,<tab>), and the tabs it walks (null: all) */
function isSelfTest(){return location.hash==="#selftest"||location.hash.indexOf("#selftest=")===0}
function selfTestTabs(){var m=location.hash.match(/^#selftest=(.+)$/);return m?decodeURIComponent(m[1]).split(","):null}
var ST_TABS=["card","guide","marker","words","global"],stWalk=null;
/* a tab the self-test walks this run (every tab when none was named) */
function stWalks(tab){return!stWalk||stWalk.indexOf(tab)>=0}
function selfTest(tabs){var res=[],seen={};stWalk=tabs||null;
  S.ui.level=9;S.op={};S.ui.single=false;
  ST_TABS.filter(stWalks).forEach(function(tab){S.tab=tab;renderEditor();var base=JSON.stringify(S);noDraw=true;
    var ctrls=[];ed.querySelectorAll("[data-set]").forEach(function(b){ctrls.push({el:b,kind:"set",key:b.dataset.set,v:b.dataset.v})});
    ed.querySelectorAll("select[data-sel]").forEach(function(x){Array.prototype.forEach.call(x.options,function(o){ctrls.push({el:x,kind:"sel",key:x.dataset.sel,v:o.value})})});
    ed.querySelectorAll('input[type=range][data-num]').forEach(function(r){ctrls.push({el:r,kind:"num",key:r.dataset.num,v:r.min});ctrls.push({el:r,kind:"num",key:r.dataset.num,v:r.max})});
    ctrls.forEach(function(c){var id=tab+"|"+c.key+"|"+c.v;if(seen[id])return;seen[id]=1;if(/^(ui\.|test\.on|@test\.on)/.test(c.key))return;
      var B=JSON.parse(base);Object.keys(B).forEach(function(k){S[k]=B[k]});setCtx(c.el);if(c.kind!=="num"&&String(curOf(c.key))===String(c.v))return;var lab=labelOf(c.el),before=modelSig(),state=JSON.stringify(S);
      try{if(c.kind==="set")c.el.click();else if(c.kind==="sel"){c.el.value=c.v;c.el.dispatchEvent(new Event("change",{bubbles:true}))}else{c.el.value=c.v;c.el.dispatchEvent(new Event("input",{bubbles:true}));c.el.dispatchEvent(new Event("change",{bubbles:true}))}}
      catch(err){res.push({tab:tab,key:c.key,v:c.v,label:lab,result:"ERROR "+err.message});return}
      var stateChanged=JSON.stringify(S)!==state,after=modelSig();
      var offCard=/^@blockDefs\./.test(c.key)&&!S.card.some(function(b){return b.kind===c.key.split(".")[1]});res.push({tab:tab,key:c.key,v:c.v,label:lab,result:after!==before?"changes the phone":!stateChanged?"NOT WIRED (state unchanged)":BEHAVIOUR.test(c.key)?"behaviour (no picture)":offCard?"behaviour (block not on this card)":"NO EFFECT (state changes, phone does not)"})});
    noDraw=false});
  return pipe("selfTest.res",res)}
function cssq(s){return String(s).replace(/"/g,'\\"')}
function labelOf(el){emit("labelOf.before",el);var r=el.closest(".row");return r?(r.querySelector("label")||{}).textContent||"":""}
function showSelfTest(res){var works={};res.forEach(function(r){if(r.result==="changes the phone")works[r.tab+"|"+r.key]=1});res.forEach(function(r){if(/NO EFFECT/.test(r.result)&&works[r.tab+"|"+r.key])r.result="same look as now (other values of this row change the phone)"});var bad=res.filter(function(r){return /NO EFFECT|NOT WIRED|ERROR|NO DEMO/.test(r.result)}),ok=res.filter(function(r){return r.result==="changes the phone"}).length,beh=res.filter(function(r){return/behaviour/.test(r.result)}).length;
  var box=document.getElementById("selftest");box.innerHTML='<h2>Self-test</h2><p><b>'+ok+'</b> option values change the phone · <b>'+beh+'</b> are behaviour (sound, taps, timing) · <b style="color:#C0392B">'+bad.length+'</b> do nothing or were not found.</p>'+
   (bad.length?'<table><tr><th>Tab</th><th>Row</th><th>Key</th><th>Value</th><th>Result</th></tr>'+bad.map(function(r){return'<tr><td>'+r.tab+'</td><td>'+esc(r.label)+'</td><td><code>'+esc(r.key)+'</code></td><td>'+esc(r.v)+'</td><td>'+r.result+'</td></tr>'}).join("")+'</table>':"<p>Every option shows on the phone.</p>");
  box.dataset.json=JSON.stringify({ok:ok,behaviour:beh,bad:bad});emit("showSelfTest.after",res)}
document.getElementById("runtest").addEventListener("click",function(){var b=this;b.disabled=true;b.textContent="Running…";setTimeout(function(){var snap=JSON.stringify(S);var r=selfTest();showSelfTest(r);b.disabled=false;b.textContent="Run the self-test again";
  var o=JSON.parse(snap);Object.keys(o).forEach(function(k){S[k]=o[k]});S.tab="card";S.view="card";renderAll(false)},30)});
on("modelSig.before",function(cx){var keep=S.view;S.view="term";var x=termScreen()+"#"+demoCardScreen()+"#"+(S.previewBar?ptool():"");S.view=keep;cx.term=x});on("modelSig.sig",function(s,cx){return s+cx.term});
/* ----- the self-test: what draws outside the card's HTML, and what is behaviour ----- */
/* the option the self-test is trying: extra views are drawn only for the options that can change them */
S._stKey=null;function sigWant(re){return S._stKey==null||re.test(S._stKey)}
on("labelOf.before",function(el){S._stKey=el&&el.dataset?(el.dataset.set||el.dataset.sel||el.dataset.num||null):null});
on("selfTestWords.before",function(){S._stKey=null});
/* probes (#probe): the page's state for the headless checks */
if(location.hash.indexOf("#probe")===0)window.LAB={S:S,run:function(src){return eval(src)}};
