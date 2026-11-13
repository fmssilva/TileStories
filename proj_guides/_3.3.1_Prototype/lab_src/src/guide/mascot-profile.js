/* The mascot's global settings (Guide › 1 · The mascot, round 19): who it is (its name, EN and PT; its first introduction, Draft with AI and Reviewed), what speaks for it (a line with no recording; the rows of its voice are made by guide/voice.js, guide/voice-test.js, guide/voice-script.js and agent/live-voice.js, and placed by editor/guide-tab.js's GUIDE_PLAN), and speech on screen (round 21). Every guide part reads them: the name through mascotName() ({name} in the words: core/words.js's nameFill), the first introduction through introLine(), the voice through say() only. */
/* the developer (2026-11-13): "We need some global AI Guide voice and mascot and other settings in Guide › 1 · Azu… and then all the AI
   guide sub-components and features should use those global settings, not local hard-coded ones"; "use the clear name 'mascot'. Add to the
   mascot's global section the definition of its name and the presentation it gives the first time it talks". Unity: a ScriptableObject
   MascotProfile (name per language, the first introduction, the look, the voice set, its speed, a line with no recording, the speech
   style) that every guide component reads; its words are CardStrings (mascot_called, guide_intro, intro_ask_*) */
/* ----- the name: a word in each language (mascot_called, Languages & Terms too); the guide's words say {name} ----- */
function mascotName(lang){var o=S.tx.mascot_called||{};lang=lang||S.lang;return String(o[lang]||o.en||"").trim()||"Azu"}
/* ----- the first introduction (guide_intro): what the mascot says the very first time it talks, before the tour's first step. {name} is
   its name, {ask} the control the visitor sees at that moment to ask (introAsk: the panel as it looks then), {0} the point (optional).
   A draft from AI plays only once Reviewed is ticked for its language; until then the visitor hears the last reviewed words ----- */
S.guideCfg.introRev={en:true,pt:true};S.guideCfg.introOk={};
/* the control to name, as the screen looks when the introduction plays: the dock at Mini shows its arrow (Player sheet: the mascot's
   face opens it), Open shows the microphone, Compact the ✦; the classic panel (no dock) its Ask button */
function introAsk(){if(typeof dockOn==="function"&&!dockOn())return"intro_ask_button";var h=S.dockH&&G.on?S.dockH:S.guideCfg.dockStart||"mini";
  if(h==="compact")return"intro_ask_star";if(h==="peek"||h==="full"||h==="open")return"intro_ask_mic";return S.guideCfg.look==="player"?"intro_ask_face":"intro_ask_arrow"}
var INTRO_ASKS=["intro_ask_arrow","intro_ask_face","intro_ask_mic","intro_ask_star","intro_ask_button"];
function introWords(lang){var o=S.tx.guide_intro||{},rev=S.guideCfg.introRev||{},t=String(o[lang]||o.en||"");
  if(rev[lang]===false){var ok=S.guideCfg.introOk[lang];t=ok!=null?ok:introDefault(lang)}return t}
function introDefault(lang){try{var z=JSON.parse(TX0).guide_intro||{};return z[lang]||z.en||""}catch(e){return""}}
function introLine(lang,point,ask){var t=nameFill(introWords(lang),lang).replace(/\{ask\}/g,wordIn(S.tx[ask||introAsk()],lang));return nameIn(t,"{0}",point==null?pointName():point,lang)}
/* the step the person (guide/person.js) and the plan (guidance/service.js) put first */
function introStep(){var p=pointName();return{id:"intro",view:"card",ref:"hero",act:"intro",text:T(introLine("en",p),introLine("pt",p))}}
/* Draft with AI: two warm sentences in the mascot's name, naming {ask}, in one language (sampleFn: the published Lab's model; never in tests) */
function introDraft(l){if(typeof sampleFn!=="function"||!sampleFn){S.mpNote="AI is not available here (it runs inside the published Lab).";renderEditor();return}S.mpBusy=l;renderEditor();
  var prompt="Write the first words a museum guide mascot says to a visitor, in "+(l==="pt"?"European Portuguese (Portugal, never Brazil), formal \"você\" forms":"British English")+". Its name is "+mascotName(l)+
    ". Two short sentences, about 20 words in all (8 seconds aloud): warm, human, a little exciting, never robotic, never \"Now\". Say its name. Then invite the visitor to ask anything by tapping {ask} (keep {ask} exactly: the app puts the control's name there). Return JSON: {\"text\":\"…\"}.";
  sampleFn(prompt,{modelTier:"quick"}).then(function(r){var j=null;try{j=JSON.parse(String(r.text).replace(/^[^{]*/,"").replace(/[^}]*$/,""))}catch(x){}S.mpBusy=null;
    if(!j||!j.text||j.text.indexOf("{ask}")<0){S.mpNote="The draft did not come back with {ask}: nothing changed.";renderEditor();return}
    if(S.guideCfg.introRev[l]!==false)S.guideCfg.introOk[l]=S.tx.guide_intro[l];S.tx.guide_intro[l]=String(j.text).trim();S.guideCfg.introRev[l]=false;S.mpNote="AI draft ("+l.toUpperCase()+"): not reviewed, so the visitor still hears the reviewed words.";renderEditor()})
  .catch(function(e){S.mpBusy=null;S.mpNote="The draft failed ("+(e&&e.message||"error")+").";renderEditor()})}
/* ----- the rows: Who (made here), a line with no recording (Voice), speech on screen's note; the source foldout s7who, placed by GUIDE_PLAN ----- */
COMP.s7who=["Guide","Who"];
function mpPlay(k){return'<span class="atin"><button class="atplay" type="button" data-mpdemo="'+k+'" title="Show it on the phone" aria-label="Show it on the phone">&#9656;</button></span>'}
function mpTx(k,l,area){var v=S.tx[k]||{},a='class="u" data-tx="'+k+'|'+l+'" aria-label="'+esc(TXP[k]+" · "+l.toUpperCase())+'"';return'<span class="mplang">'+l.toUpperCase()+'</span>'+(area?'<textarea rows="3" '+a+'>'+esc(v[l]||"")+'</textarea>':'<input '+a+' value="'+esc(v[l]||"")+'">')}
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var s7=innerOf(d,"s7");if(!s7)return d.innerHTML;var rv=S.guideCfg.introRev||{};
  var name=row("Name",'<div class="mpname">'+langCodes().map(function(l){return'<label class="mpl">'+mpTx("mascot_called",l)+'</label>'}).join("")+mpPlay("name")+'</div>',
      "The mascot's name in each language. The guide's words say {name} and the visitor reads this name there: the guide panel and its ask field, the ✦'s name, Ask about the view's pill, the games' cards, the first introduction, What next?. The Editor always says “the mascot”. Unity: MascotProfile.displayName, a CardStrings entry per language.");
  var intro=row("First introduction",'<div class="mpintro">'+langCodes().map(function(l){return'<div class="mpil">'+mpTx("guide_intro",l,true)+
        '<div class="btnrow"><button class="ubtn sm" type="button" data-mpai="'+l+'">'+(S.mpBusy===l?"Drafting…":"Draft with AI")+'</button><label class="ovrev"><input type="checkbox" data-mprev="'+l+'"'+(rv[l]!==false?" checked":"")+'> Reviewed</label>'+(l===langCodes()[0]?mpPlay("intro"):"")+'</div>'+
        '<div class="uses mpnow" data-mpnow="'+l+'">'+(rv[l]===false?"Not reviewed: the visitor hears the reviewed words. ":"")+"Said now: “"+esc(introLine(l,null))+'”</div></div>'}).join("")+'</div>'+(S.mpNote?'<div class="note">'+esc(S.mpNote)+'</div>':""),
      "What the mascot says the very first time it talks, before the tour's first step (Moments › Introduction says when: the first tour or every tour). Two short sentences, about 8 s, warm and in its own name. {name}: its name; {ask}: the control the visitor sees at that moment to ask (the panel's arrow at Mini, the microphone when it is open, the ✦ at Compact, the Ask button of the classic panel; Languages & Terms › intro_ask_*); {0}: the point, if wanted. Draft with AI writes a draft in that language, never played until Reviewed is ticked (the AI runs inside the published Lab only). Recorded in the chosen voice: Voice › Coverage lists it if the words change.");
  var where=row("Where the name shows",'<span class="uses">The guide panel (its name, “Ask '+esc(mascotName())+' anything”), the ✦'+"'"+'s name for screen readers, Ask about the view'+"'"+'s pill, the games'+"'"+' cards (“'+esc(mascotName().toUpperCase())+' · QUIZ”), the first introduction, What next?'+"'"+'s offers, Ask'+"'"+'s answers (the model is told its name).</span>'+mpPlay("where"),
      "Each place the visitor reads or hears the mascot's name, all from Name above ({name} in the words). The ▸ opens the guide panel, where the name shows.");
  s7.insertAdjacentHTML("beforeend",fo("s7who",'Who <span class="sub">· its name and its first words</span>',null,name+intro+where,2));
  addRows(d,"s7voice",row("A line with no recording",pseg("guideCfg.noRec",["device","silent"],["The device voice","Silent, the words on screen"]),
    "A line the chosen voice has no recording of (an edited line, a new one: Coverage lists them). The device voice: the live voice first when its key is on this computer (Voice of the answers), else this device's voice. Silent: no voice, its words on screen at the voice's pace. An answer to a question is always spoken (it can never be recorded). Unity: MascotProfile.noRecording."),"Speaking now");
  /* where these rows were: one line each, a link to their one place */
  addRows(d,"s7voice",row("Voice and speed",mpGo("gt:voice","Guide › 1 · The mascot › Voice"),"The mascot's voice, its speed, a line with no recording, its coverage and what speaks now are global settings: every part of the guide (the tour, the games, the tips, the answers) speaks in them (round 19)."),"Music bed");
  addRows(d,"s7agent",row("The answers' voice",mpGo("gt:voice","Guide › 1 · The mascot › Voice › Voice of the answers"),"The voice that speaks the answers (the device's, or Azure or ElevenLabs live) is one of the mascot's global settings, beside its recorded voice (round 19)."),"Link cards");
  addRows(d,"s7cap",'<div class="note mpspeech">The bubble, the teleprompter and the highlight: how the mascot’s words show on screen while it speaks. Round 21 makes them one smooth highlighter for every place speech is shown (the bubble, the teleprompter, the panel, the block being read); until then these rows light the captions and the block’s own words, and the bubbles and the panel light the word being said.</div>');
  addRows(d,"s7cap",row("Highlight",mpGo("gt:speech","Guide › 1 · The mascot › Speech on screen"),"How the spoken words light up is one setting for the whole guide (round 19; one highlighter in round 21)."));
  return d.innerHTML});
function mpGo(id,t){return'<button class="ubtn sm" type="button" data-goto="'+id+'">'+esc(t)+' &#8250;</button>'}
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(noRec|"));
ed.addEventListener("click",function(e){var t=e.target.closest("[data-mpai],[data-mpdemo]");if(!t)return;e.stopPropagation();if(t.dataset.mpai){introDraft(t.dataset.mpai);return}showRowDemo("mp:"+t.dataset.mpdemo)},true);
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.mprev)return;e.stopImmediatePropagation();var l=t.dataset.mprev;S.guideCfg.introRev[l]=t.checked;if(t.checked){delete S.guideCfg.introOk[l]}S.mpNote="";renderEditor()},true);
/* each row's demo: the name on the open panel; the introduction played; a line with no recording said (Speaking now names its voice) */
rowDemos({"mp:name":{tour:"g1",dock:"peek",ring:".gd-who|.gd-eye|.gdock"},"mp:where":{tour:"g1",dock:"peek",ring:".gd-who|.gd-eye|.gdock"},"mp:intro":{tour:true,intro:true,ring:".mascot|.gdock"},
  "guideCfg.noRec":{tour:true,act:function(){if(!G.on)return;stopSpeech();G.sayLine=null;var t=L(T("This line has no recording yet, so you hear what A line with no recording says.","Esta frase ainda não tem gravação, por isso ouve o que diz Uma frase sem gravação."));renderGuide(t);say(t,function(k){markWord(k,null)},function(){})},ring:".gdock|.gcap"}});
