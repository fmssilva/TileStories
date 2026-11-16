/* The self-test of the texts: each text field must reach the phone (selfTestWords; the page's own #selftest walks it in slices, selfTestWordsAsync). */
var TX_EXPECT={reading_minutes:1,guide_duration:1,pbar_tip_theme:1,pbar_tip_motion:1,pbar_tip_lang:1,pbar_tip_focus:1,pbar_tip_screen:1,term_crumb:1,test_tile_tag:1,test_tile_title:1,test_tile_line:1,test_demo_line:1,democard_title:1,democard_tag:1,pbar_label:1,pbar_light:1,pbar_motion:1,pbar_focus:1,pbar_phone:1,term_eyebrow:1,close:1,visitor_settings:1,page_back:1,gallery_name:1,page_previous:1,page_next:1,locate_label:1,open_block_button:1,open_row_story:1,fun_fact_label:1,
 before_label:1,after_label:1,model_3d_hint:1,panorama_360_drag_hint:1,audio_play:1,test_demo_title:1,open_row_expand:1,guide_start:1,guide_block_title:1,guide_block_line:1};
var TX_DOM={peek_next_label:"shown at Peek",show_on_wall_button:"shown on the sticky button",audio_pause:"after a tap on Play",term_read_more:"shown in the glossary popup",pbar_dark:"the bar in dark mode",pbar_still:"the bar under Reduce Motion",pbar_wide:"the bar on a wide screen",pbar_tablet:"the bar on a tablet"};
var STW_TABS=["card","guide","words","global"];
/* the walk's steps: its start, a tab's fields (the tab drawn, the state it starts from), one field worked and judged, its end */
function stwStart(){emitLatestFirst("selfTestWords.before");S.ui.level=9;S.op={};S.ui.single=false;S.ownAll=true;S.ui.txf="content"}
function stwEnd(){noDraw=false;S.ownAll=false;S.ui.txf="all"}
function stwTab(tab){S.tab=tab;renderEditor();var base=JSON.stringify(S);noDraw=true;
  var acts=[];function lg(s){return langCodes().indexOf(s)>=0?s:null}
  ed.querySelectorAll("[data-tx]").forEach(function(x){var p=x.dataset.tx.split("|");acts.push({el:x,key:"tx:"+x.dataset.tx,tx:p[0],lang:p[1],v:"Zz"+p[0],kind:"input"})});
  ed.querySelectorAll("[data-loc]").forEach(function(x){var p=x.dataset.loc.split("|");acts.push({el:x,key:"loc:"+x.dataset.loc,lang:lg(p[1]),v:"Zz",kind:"input"})});
  ed.querySelectorAll('[data-field^="demo:"],[data-field^="termpage:"]').forEach(function(x){var p=x.dataset.field.split("|");acts.push({el:x,key:"demo:"+x.dataset.field,lang:lg(p[2]),v:p[3]==="lines"?"Zz1\nZz2\nZz3":"Zz",kind:"input"})});
  ed.querySelectorAll('[data-asset^="demo:"],[data-asset^="termpage:"]').forEach(function(x){if(x.getAttribute("aria-pressed")!=="true")acts.push({el:x,key:"demo:"+x.dataset.asset,kind:"click"})});
  ed.querySelectorAll('[data-assets^="demo:"],[data-assets^="termpage:"]').forEach(function(x){acts.push({el:x,key:"demo:"+x.dataset.assets,kind:"click"})});
  ed.querySelectorAll("[data-term]").forEach(function(x){acts.push({el:x,key:"term:"+x.dataset.term,lang:lg(x.dataset.term.split("|")[2]),v:"Zz",kind:"input"})});
  return{tab:tab,base:base,acts:acts}}
/* the phone before a field is worked: every field of a tab starts from the same state (its base, in the field's language), so its signature is
   drawn once per language and kept, after two fields in a row drew it the same (a tab whose signature ever differs draws it every time) */
function stwBefore(T){var l=S.lang,c=T.sig||(T.sig={}),x=c[l];if(T.noCache)return modelSig();if(x&&x.n>=2)return x.s;var s=modelSig();
  if(!x)c[l]={s:s,n:1};else if(x.s===s)x.n++;else{T.noCache=true;G.stwNoCache=(G.stwNoCache||0)+1}return s}
function stwAct(T,a,res){var tab=T.tab,B=JSON.parse(T.base);Object.keys(B).forEach(function(k){S[k]=B[k]});if(a.lang)S.lang=a.lang;var before=stwBefore(T),state=JSON.stringify(S),lab=a.key;
  try{if(a.kind==="click")a.el.click();else{a.el.value=a.v;a.el.dispatchEvent(new Event("input",{bubbles:true}))}}catch(err){res.push({tab:tab,key:a.key,v:a.v,label:lab,result:"ERROR "+err.message});return}
  var changed=modelSig()!==before,stateChanged=JSON.stringify(S)!==state,k=a.tx,r;
  if(changed)r="changes the phone";else if(!stateChanged)r="NOT WIRED (state unchanged)";
  else if(k&&/^s7/.test(TXOWN[k])&&!TX_EXPECT[k])r="behaviour (shown during the tour)";else if(k&&TX_DOM[k])r="behaviour ("+TX_DOM[k]+")";
  else if(k&&!TX_EXPECT[k])r="elsewhere (its part is not drawn on the Lab's card)";
  else if(/^loc:gstep:/.test(a.key))r="behaviour (shown during the tour)";
  else if(/^(term:|loc:term:)/.test(a.key)&&!termUses(byId(S.terms,a.key.replace(/^loc:/,"").split(":")[1].split("|")[0])||{words:""}).n)r="elsewhere (no card text links this term)";
  else r="NO EFFECT (state changes, phone does not)";
  res.push({tab:tab,key:a.key,v:a.v||"",label:lab,result:r})}
/* in one go (the Editor's Run the self-test, the checks' probes) */
function selfTestWords(){var res=[];stwStart();STW_TABS.forEach(function(tab){var T=stwTab(tab);T.acts.forEach(function(a){stwAct(T,a,res)});noDraw=false});stwEnd();return res}
/* round 21 (21.8; the words walk ran past 40 min, 14 at round 17b, and slowed as it went). What builds up: each field drew the phone's
   signature twice, and each signature parses about 1.7 MB of HTML into detached elements (the card's screen made again by its
   cardScreen.html handlers for the card, the demo card and the other views: about 60 parses, some 95 % of a field's time); that DOM
   garbage piles up within one long task (the walk ran as one task of about 1,770 fields). Measured (every field walked, bad=0): one task
   with two signatures a field passed 10 min; one task with the signature before a field drawn once per tab and language (stwBefore):
   1.9 min; slices of 20 fields with two signatures: 3.5 min. Both are kept: stwBefore (every field of a tab starts from the same
   state; checked equal on two fields first) and, for the page's own self-test, slices of STW_SLICE fields, each its own task (a
   MessageChannel message: no timer, so the headless run's virtual time does not move and its DOM dump waits for the end), the same
   fields and the same judgement; its JSON says pending until the last slice */
var STW_SLICE=20;
function selfTestWordsAsync(done){var res=[],ti=-1,T=null,i=0,mc=new MessageChannel();stwStart();
  var go=function(){var n=0;while(true){if(!T||i>=T.acts.length){noDraw=false;ti++;if(ti>=STW_TABS.length){stwEnd();mc.port1.onmessage=null;done(res);return}T=stwTab(STW_TABS[ti]);i=0}
      stwAct(T,T.acts[i++],res);if(++n>=STW_SLICE){mc.port2.postMessage(0);return}}};
  mc.port1.onmessage=function(){noDraw=true;go()};go()}
/* the page's own #selftest (core/boot.js) walks the texts after the controls, in slices; the Editor's button keeps the one go */
on("selfTest.res",function(a){return stWalks("words")&&!G.stwAsync?a.concat(selfTestWords()):a});
on("showSelfTest.after",function(res){var box=document.getElementById("selftest"),els=res.filter(function(r){return/^elsewhere/.test(r.result)}).length,
  cnt=function(re){var x=res.filter(function(r){return re.test(r.key)});return[x.length,x.filter(function(r){return r.result==="changes the phone"}).length]},tx=cnt(/^tx:/),lc=cnt(/^loc:/),dm=cnt(/^demo:/);
  box.insertAdjacentHTML("beforeend",'<p>Texts on their parts: <b>'+tx[1]+'</b> of '+tx[0]+' fields change the phone; the rest belong to parts the Lab does not draw (<b>'+els+'</b> elsewhere) or show during the tour. Content in the Texts check: <b>'+lc[1]+'</b> of '+lc[0]+'. Demo content and term pages: <b>'+dm[1]+'</b> of '+dm[0]+'.</p>');
  var j=JSON.parse(box.dataset.json||"{}");j.elsewhere=els;j.texts={fields:tx[0],ok:tx[1]};j.content={fields:lc[0],ok:lc[1]};j.demo={fields:dm[0],ok:dm[1]};if(G.stwPending)j.pending="the words walk";box.dataset.json=JSON.stringify(j)});
