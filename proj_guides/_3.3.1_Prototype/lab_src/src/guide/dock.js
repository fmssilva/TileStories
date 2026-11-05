/* The Guide Dock: Compact, Mini, Peek, Full; Pause and Stop at Mini, the hide control; the bubble, the talk button and its states, the conversation, the suggested questions. */
S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.miniCtl="pausestop";S.guideCfg.swipeCompact=true;S.guideCfg.bubbles="own";S.guideCfg.talk="both";S.guideCfg.earcons=true;S.guideCfg.chips=true;
S.dockH="mini";G.talk="idle";G.thread=[];
COMP.s7dock=["Guide","Guide panel"];FOCUS.s7dock=["card",".gblock","full"];
/* the demo point's suggested questions (the point's content; Specific Marker > Guide script) */
S.guideAsks=[T("What happened in 1755?","O que aconteceu em 1755?"),T("Who lived here?","Quem viveu aqui?"),T("Show me the palace","Mostra-me o palácio"),T("What was inside?","O que havia lá dentro?")];
function dockOn(){return S.guideCfg.panel==="dock"}
/* a step's name: the point (the hero), the story's heading above the row being told, else the block's title */
function stepTitle(st){if(!st)return"";if(st.ref==="hero")return pointName();
  if(st.ref.indexOf("r:")===0){var j=S.story.findIndex(function(r){return"r:"+r.id===st.ref});for(var i=j;i>=0;i--){var r=S.story[i];if(r.kind==="heading"&&r.c&&r.c.text)return L(r.c.text)}
    var rr=S.story[j];if(rr&&rr.c&&rr.c.text)return(L(rr.c.text).match(/\S+/g)||[]).slice(0,6).join(" ")+"…"}
  var c=corpus().filter(function(x){return x.ref===st.ref})[0];return c?c.title:""}
/* own words or a block's words: a block's words are read where they are; Azu's own words go in its bubble */
function blockWords(t){var st=S.guideSteps[G.i],el=st&&stepEl(st),n=function(x){return String(x||"").replace(/\s+/g," ").trim()};return!!(el&&t&&n(el.textContent).indexOf(n(t).slice(0,60))>=0)}
var MIC='<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="9" y="3" width="6" height="11" rx="3"/><path d="M5.5 11a6.5 6.5 0 0 0 13 0M12 17.5V21M8.5 21h7"/></svg>',
  KBD='<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="6" width="18" height="12" rx="2.5"/><path d="M7 10h.01M10 10h.01M13 10h.01M16 10h.01M7 14h10"/></svg>',
  ICO={prev:'<path d="M7 6v12M18 6l-8 6 8 6z"/>',next:'<path d="M17 6v12M6 6l8 6-8 6z"/>',pause:'<path d="M8 5v14M16 5v14"/>',play:'<path d="M8 5l11 7-11 7z"/>',mute:'<path d="M4 9h4l5-4v14l-5-4H4zM17 9l4 6M21 9l-4 6"/>',sound:'<path d="M4 9h4l5-4v14l-5-4H4zM17 8a5 5 0 0 1 0 8"/>',stop:'<path d="M6 6l12 12M18 6 6 18"/>',halt:'<rect x="6.5" y="6.5" width="11" height="11" rx="2"/>'};
function ico(k){return'<svg viewBox="0 0 24 24" aria-hidden="true">'+ICO[k]+'</svg>'}
var TALKW={idle:"dock_tap_talk",listening:"dock_listening",thinking:"dock_thinking",speaking:"dock_tap_stop",interrupted:"dock_go_ahead",notheard:"dock_not_heard"};
function talkState(){if(G.talk==="listening"||G.talk==="notheard"||G.talk==="interrupted")return G.talk;if(G.busy)return"thinking";if(G.on&&!G.paused&&(G.utter||G.timer))return"speaking";return"idle"}
/* the dock replaces the captions box and the control bar; the old ones stay as "Classic" */
/* the dock's markup at a height: Mini (the step, its words, Pause and Stop, the talk button), Peek (+ the controls, the keyboard and the
   suggested questions), Full (+ the conversation). Controls at Mini = Tap on the line keeps today's Mini (the keyboard beside the talk button) */
var HIDE='<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 9l6 6 6-6"/></svg>';
function dockHtml(h){var n=S.guideSteps.length,st=S.guideSteps[G.i],ts=talkState(),mc=S.guideCfg.miniCtl,kb='<button type="button" class="gd-kb" data-dock="type" aria-label="'+TXH("dock_type")+'">'+KBD+'</button>',
    seg='<div class="gd-seg">'+S.guideSteps.map(function(s,j){return'<button type="button" data-gseg="'+j+'" class="'+(j<G.i?"done":j===G.i?"now":"")+'" aria-label="'+esc(fmt(TXT("dock_stop_of"),j+1,n))+'"></button>'}).join("")+'</div>',
    pz='<button type="button" class="gd-mc" data-g="'+(G.paused?"play":"pause")+'" aria-label="'+(G.paused?TXH("guide_ctrl_play"):TXH("guide_ctrl_pause"))+'">'+ico(G.paused?"play":"pause")+'</button>',
    sp='<button type="button" class="gd-mc" data-g="stop" aria-label="'+TXH("guide_ctrl_stop")+'">'+ico("halt")+'</button>';
  return'<button type="button" class="gd-grab" data-dock="cycle" aria-label="'+TXH(h==="full"?"dock_collapse":"dock_expand")+'"><i></i></button>'+(h!=="full"?'<button type="button" class="gd-hide" data-dock="hide" aria-label="'+TXH("dock_hide")+'">'+HIDE+'</button>':"")+seg+
    '<div class="gd-main"><div class="gd-line" data-dock="playpause"><small class="gd-stop">'+esc(fmt(TXT("dock_stop_of"),G.i+1,n))+(stepTitle(st)?' · '+esc(stepTitle(st)):"")+'</small><div class="gd-cap"></div></div>'+
    (mc==="line"?kb:'<div class="gd-mini">'+pz+(mc==="pausestop"?sp:"")+'</div>')+
    '<button type="button" class="gd-talk s-'+ts+'" data-talk="1" aria-label="'+TXH("dock_ask")+'"><i class="ring"></i><i class="ring r2"></i>'+MIC+'</button></div>'+
    '<div class="gd-hint" aria-live="polite">'+TXH(TALKW[ts])+'</div>'+
    '<div class="gd-peek"><div class="gd-ctl"><button type="button" data-g="prev" aria-label="'+TXH("guide_ctrl_prev")+'">'+ico("prev")+'</button><button type="button" data-g="'+(G.paused?"play":"pause")+'" aria-label="'+(G.paused?TXH("guide_ctrl_play"):TXH("guide_ctrl_pause"))+'">'+ico(G.paused?"play":"pause")+'</button><button type="button" data-g="next" aria-label="'+TXH("guide_ctrl_next")+'">'+ico("next")+'</button>'+
      '<button type="button" data-g="mute" aria-pressed="'+!!S.guideMute+'" aria-label="'+TXH("guide_ctrl_mute")+'">'+ico(S.guideMute?"mute":"sound")+'</button><button type="button" data-g="cc" aria-pressed="'+S.guideCfg.captions+'" class="cc">CC</button><button type="button" data-g="stop" aria-label="'+TXH("guide_ctrl_stop")+'">'+ico("stop")+'</button></div>'+
      '<div class="gd-chips">'+(mc!=="line"?kb:"")+(S.guideCfg.chips?S.guideAsks.map(function(q,i){return'<button type="button" data-chip="'+i+'">'+esc(L(q))+'</button>'}).join(""):"")+'</div></div>'+
    '<div class="gd-full"><div class="gd-thread" aria-live="polite">'+G.thread.map(function(m){return'<div class="gm-'+m.who+'">'+esc(m.text)+'</div>'}).join("")+(G.busy?'<div class="gm-azu typing"><i></i><i></i><i></i></div>':"")+(G.micNote?'<div class="gm-note">'+esc(G.micNote)+'</div>':"")+'</div>'+
      '<form class="gd-form"><input id="gq" class="gin" type="text" autocomplete="off" placeholder="'+TXH("dock_type")+'"><button type="submit" aria-label="'+TXH("dock_send")+'">&#10148;</button></form></div>'}
/* the dock replaces the captions box and the control bar; the old ones stay as "Classic". Compact: no dock (the voice goes on; the
   guide's button shows the progress and a pause); Azu's own words in its bubble, a block's words lit in the block */
on("renderGuide.after",function(text){["gdock","gbubble"].forEach(function(c){var o=ph.querySelector("."+c);if(o)o.remove()});if(!G.on||!dockOn())return;
  var cap=ph.querySelector(".gcap"),bar=ph.querySelector(".gbar"),ask=ph.querySelector(".gask");if(bar)bar.remove();if(ask)ask.remove();
  var st=S.guideSteps[G.i],own=cap&&!blockWords(text!=null?text:stepText(st));if(G.askOpen&&S.dockH!=="full")S.dockH="full";var ts=talkState(),compact=S.dockH==="compact",d=null;
  if(!compact){d=document.createElement("div");d.className="gdock h-"+S.dockH;d.setAttribute("role","region");d.setAttribute("aria-label",TXT("dock_label"));d.innerHTML=dockHtml(S.dockH);ph.appendChild(d)}
  /* the words: Azu's own in its bubble (above the dock), or in the dock's line; a block's words are lit in the block, so the line shows the step */
  if(cap){if(own&&(compact||S.guideCfg.bubbles==="own"&&S.dockH!=="full")){var b=document.createElement("div");b.className="gbubble";b.setAttribute("role","status");cap.classList.add("gb-text");b.appendChild(cap);ph.appendChild(b);placeBubble()}
    else if(d&&(own||S.dockH==="full")){d.querySelector(".gd-cap").appendChild(cap)}else cap.remove()}
  var th=d&&d.querySelector(".gd-thread");if(th)th.scrollTop=th.scrollHeight;var m=ph.querySelector(".mascot");if(m)m.dataset.state=ts});
/* the dock's markup reaches the phone's signature: its height at the start, its controls at Mini, its chips */
on("modelSig.sig",function(h){if(!sigWant(/guideCfg\.(dockStart|miniCtl|chips|panel)/))return h;var keep=S.dockH;S.dockH=S.guideCfg.dockStart;var x="#dock:"+S.guideCfg.panel+"|"+S.dockH+"|"+(S.dockH==="compact"?"":dockHtml(S.dockH));S.dockH=keep;return h+x});
/* the bubble sits just above the dock, its tail towards Azu */
function placeBubble(){var b=ph.querySelector(".gbubble"),d=ph.querySelector(".gdock")||ph.querySelector(".gbtn-wrap.intour"),m=ph.querySelector(".mascot");if(!b)return;var top=d?d.offsetTop:ph.offsetHeight-16;
  b.style.bottom=(ph.offsetHeight-top+10)+"px";var up=m&&m.offsetTop+30<top-b.offsetHeight-10;b.classList.toggle("tail-up",!!up)}
/* the free height for the block being told: above the dock and the bubble */
function freeHeight(){var box=scr.getBoundingClientRect(),lo=box.bottom;[".gcap",".gbar",".gdock",".gbubble",".gbtn-wrap.intour"].forEach(function(s){var e=ph.querySelector(s);if(e){var r=e.getBoundingClientRect();if(r.height)lo=Math.min(lo,r.top)}});return{top:box.top,bottom:lo}}
/* keep the talk state and the bubble's place in step with the voice */
/* a bubble shows one sentence at a time (2 lines at most), the sentence being said */
function bubbleSentence(){var b=ph.querySelector(".gbubble .gcap");if(!b)return;var ws=b.querySelectorAll(".gw"),k=Math.max(0,G.hl&&G.hl.k>=0?G.hl.k:0),sid=[],n=0;
  for(var i=0;i<ws.length;i++){sid.push(n);if(/[.!?…]["”»)]*$/.test(ws[i].textContent))n++}var cur=sid[Math.min(k,ws.length-1)];for(var j=0;j<ws.length;j++)ws[j].classList.toggle("gb-off",sid[j]!==cur)}
setInterval(function(){if(!G.on||!dockOn())return;bubbleSentence();var t=ph.querySelector(".gd-talk"),ts=talkState();if(t&&!t.classList.contains("s-"+ts)){t.className="gd-talk s-"+ts;var h=ph.querySelector(".gd-hint");if(h)h.textContent=TXT(TALKW[ts]);var m=ph.querySelector(".mascot");if(m)m.dataset.state=ts}placeBubble()},150);
/* the conversation: the tour's lines and the questions and answers */
on("runStep.after",function(){var st=S.guideSteps[G.i];if(st){var t=stepText(st);if(t&&(!G.thread.length||G.thread[G.thread.length-1].text!==t))G.thread.push({who:"azu",text:t});if(G.thread.length>60)G.thread.shift()}});
on("ask.before",function(q){G.thread.push({who:"me",text:q.trim()});G.micNote="";G.talk="idle"});
/* the answer goes to the conversation once, when it is given (spoken by any voice, or not spoken); its words show as it is said */
on("ask.after",function(t,turn){if(!t)return;G.thread.push(turn?{who:"azu",text:t,turn:turn}:{who:"azu",text:t});if(G.thread.length>60)G.thread.shift()});
on("say.before",function(text){if(G.askOpen&&G.answer&&text===G.answer)renderGuide(text)});
on("startTour.before",function(){S.dockH=S.guideCfg.dockStart;G.thread=[];G.talk="idle";G.askOpen=false;G.answer="";G.micNote=""});
/* ----- the talk button: a tap listens until the visitor stops; a long press is hold-to-talk; speaking, a tap interrupts and listens ----- */
function earcon(up){if(!S.guideCfg.earcons)return;try{actx=actx||new (window.AudioContext||window.webkitAudioContext)();var o=actx.createOscillator(),g=actx.createGain(),t=actx.currentTime;o.type="sine";o.frequency.setValueAtTime(up?620:880,t);o.frequency.exponentialRampToValueAtTime(up?880:560,t+.12);g.gain.setValueAtTime(.0001,t);g.gain.exponentialRampToValueAtTime(.08,t+.02);g.gain.exponentialRampToValueAtTime(.0001,t+.16);o.connect(g);g.connect(actx.destination);o.start(t);o.stop(t+.18)}catch(e){}}
function micBlockedNote(){return TXT(window.top!==window?"dock_mic_framed":"dock_mic_none")}
function listen(hold){var R=window.SpeechRecognition||window.webkitSpeechRecognition;if(G.on&&!G.paused&&(G.utter||G.timer)){pauseTour();G.talk="interrupted";earcon(false);haptic()}
  if(!R){heard(null,"none");return}var rec;try{rec=new R()}catch(e){heard(null,"none");return}
  rec.lang=S.lang==="pt"?"pt-PT":"en-GB";rec.interimResults=true;rec.continuous=!!hold;G.rec=rec;var got="";
  rec.onresult=function(ev){got="";for(var i=0;i<ev.results.length;i++)got+=ev.results[i][0].transcript;var h=ph.querySelector(".gd-hint");if(h)h.textContent=got||TXT("dock_listening")};
  rec.onerror=function(e){G.rec=null;heard(null,e.error)};rec.onend=function(){if(G.rec!==rec)return;G.rec=null;heard(got,null)};
  try{rec.start();G.talk="listening";earcon(true);haptic();renderGuide()}catch(e){heard(null,"start")}}
function heard(q,err){if(firstAnswer("heard.answer",q,err)!==undefined)return;if(q&&q.trim()){G.talk="idle";earcon(false);ask(q);return}
  G.talk="notheard";if(err&&err!=="no-speech"&&err!=="aborted"){G.micNote=micBlockedNote();S.dockH="full";G.askOpen=true}renderGuide();try{if(navigator.vibrate)navigator.vibrate([10,60,10])}catch(e){}
  setTimeout(function(){if(G.talk==="notheard"){G.talk="idle";renderGuide();var i=ph.querySelector("#gq");if(i&&S.dockH==="full")i.focus({preventScroll:true})}},2200)}
var talkDown=null;
ph.addEventListener("pointerdown",function(e){var t=e.target.closest("[data-talk]");if(!t)return;e.preventDefault();e.stopPropagation();var mode=S.guideCfg.talk;
  talkDown={at:performance.now(),hold:false,timer:mode!=="tap"?setTimeout(function(){if(!talkDown)return;talkDown.hold=true;listen(true)},380):0}},true);
ph.addEventListener("pointerup",function(e){if(!talkDown)return;var td=talkDown;talkDown=null;clearTimeout(td.timer);e.stopPropagation();
  if(td.hold){if(G.rec)try{G.rec.stop()}catch(x){}return}
  if(S.guideCfg.talk==="hold"){G.talk="notheard";G.micNote="";renderGuide();var h=ph.querySelector(".gd-hint");if(h)h.textContent=TXT("dock_hold_hint");setTimeout(function(){G.talk="idle";renderGuide()},1600);return}
  if(G.rec){try{G.rec.stop()}catch(x){}return}listen(false)},true);
/* the dock's own taps: its height, play / pause on the line, typing, a step's segment, a suggested question */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-dock],[data-gseg],[data-chip]");if(!t||!ph.querySelector(".gdock"))return;e.stopPropagation();
  if(t.dataset.chip!=null){ask(L(S.guideAsks[+t.dataset.chip]));return}
  if(t.dataset.gseg!=null){stopSpeech();G.paused=false;G.askOpen=false;S.dockH=S.dockH==="full"?"peek":S.dockH;G.i=+t.dataset.gseg;runStep();return}
  var k=t.dataset.dock;if(k==="hide"){S.dockH="compact";G.askOpen=false;renderGuide();return}if(k==="cycle"){S.dockH={mini:"peek",peek:"full",full:"mini"}[S.dockH];G.askOpen=S.dockH==="full";renderGuide();return}
  if(k==="playpause"){if(S.dockH==="full")return;if(G.paused)resumeTour();else{G.explicitPause=true;pauseTour();G.explicitPause=false}return}
  if(k==="type"){S.dockH="full";G.askOpen=true;renderGuide();var i=ph.querySelector("#gq");if(i)i.focus({preventScroll:true})}},true);
/* a swipe on the grabber: up raises the dock one height, down lowers it; down at Mini hides it (Compact) when Swipe down to Compact is on */
var grabY=null;ph.addEventListener("pointerdown",function(e){if(e.target.closest(".gd-grab"))grabY=e.clientY},true);
ph.addEventListener("pointerup",function(e){if(grabY==null)return;var dy=e.clientY-grabY;grabY=null;if(Math.abs(dy)<24)return;var o=["compact","mini","peek","full"],i=o.indexOf(S.dockH),lo=S.guideCfg.swipeCompact?0:1;
  S.dockH=o[Math.max(lo,Math.min(3,i+(dy<0?1:-1)))];G.askOpen=S.dockH==="full";e.stopPropagation();renderGuide()},true);
ph.addEventListener("submit",function(e){var f=e.target.closest(".gd-form");if(!f)return;e.preventDefault();e.stopPropagation();var i=f.querySelector("#gq");if(i&&i.value.trim()){var q=i.value;i.value="";ask(q)}},true);

/* Editor: 6 · Guide > Guide panel (after Captions); Specific Marker > Guide script > Suggested questions */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var ca=d.querySelector('[data-fo="s7cap"]');if(!ca)return d.innerHTML;
  ca.insertAdjacentHTML("beforebegin",comp("s7dock","one panel for the tour's words, its controls and the conversation",
    row("Panel",pseg("guideCfg.panel",["dock","classic"],["Dock","Classic"]),"Dock: one panel at the bottom with its heights: Mini (the step, the words, Pause, Stop and the talk button), Peek (+ previous, next, sound, captions, the keyboard and the suggested questions), Full (the conversation and a text field), and Compact (hidden: the voice goes on, the guide's button shows the progress and a pause). The grabber or a swipe changes its height; the chevron hides it. Classic: the captions box over the control bar.")+
    row("Starts at",pseg("guideCfg.dockStart",["mini","peek","compact"],["Mini","Peek","Compact"]),"Its height when the tour starts. Compact: the tour starts hidden; a tap on the guide's button brings the dock at Mini.",true)+
    row("Controls at Mini",pseg("guideCfg.miniCtl",["pausestop","pause","line"],["Pause + Stop","Pause only","Tap on the line"]),"Pause + Stop: two buttons (44 px) beside the talk button, always in sight; the keyboard moves to Peek. Pause only: the same without Stop (Stop is at Peek). Tap on the line: no buttons; a tap on the dock's line pauses or plays, and the keyboard stays beside the talk button.",true)+
    row("Swipe down to Compact",pseg("guideCfg.swipeCompact",["true","false"],["On","Off"]),"On: a swipe down on the grabber at Mini hides the dock (Compact); the voice goes on. Off: Mini is the lowest a swipe goes; the chevron and the guide's button still hide it.",true)+
    row("Azu's own words",pseg("guideCfg.bubbles",["own","panel"],["In a bubble","In the panel"]),"In a bubble: what Azu says in its own words (the welcome, an ask, an answer) appears in a bubble above the panel, its tail towards Azu; a block's words light up in the block itself, with no bubble. Every line also goes to the conversation (Full).")+
    row("Talk button",pseg("guideCfg.talk",["both","tap","hold"],["Tap or hold","Tap","Hold"]),"Tap: listens until the visitor stops speaking; a second tap sends. Hold: listens while held, sends on release. While Azu speaks, the button interrupts it and listens. The keyboard is always beside it. Never always-listening.")+
    row("Sounds",pseg("guideCfg.earcons",["true","false"],["On","Off"]),"A short rising tone when it starts listening and a falling one when it stops (under 300 ms). With a light haptic.")+
    row("Suggested questions",pseg("guideCfg.chips",["true","false"],["Shown","Hidden"]),"Chips in the Peek height (the point's own, in the Guide script). A tap asks it.")+
    row("Try",'<button class="del" type="button" data-docktry="1">Try</button>',"Starts the tour with the dock.")));
  var f=innerOf(d,"s7dock");if(f)f.insertAdjacentHTML("beforeend",ownbar("s7dock")+ownPanels("s7dock"));return d.innerHTML});
on("edMarker.html",function(h){var d=document.createElement("div");d.innerHTML=h;var f=innerOf(d,"m3");if(!f)return d.innerHTML;
  f.insertAdjacentHTML("beforeend",row("Suggested questions",'<div class="gasks">'+S.guideAsks.map(function(q,i){return'<div class="btnrow"><input class="u" data-gask="'+i+'|en" value="'+esc(q.en||"")+'"><input class="u" data-gask="'+i+'|pt" value="'+esc(q.pt||"")+'"><button class="del" type="button" data-gaskdel="'+i+'">Remove</button></div>'}).join("")+
    '<button class="ubtn sm" type="button" data-gaskadd="1">+ Add a question</button></div>',"Chips in the guide panel (EN / PT): questions a visitor might ask here. A tap asks Azu."));return d.innerHTML});
ed.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.gask)return;e.stopImmediatePropagation();var q=t.dataset.gask.split("|");S.guideAsks[+q[0]][q[1]]=t.value},true);
ed.addEventListener("click",function(e){var t=e.target.closest("[data-gaskadd],[data-gaskdel],[data-docktry]");if(!t)return;e.stopPropagation();
  if(t.dataset.docktry){if(G.on)stopTour();S.guideCfg.panel="dock";startTour();return}
  if(t.dataset.gaskadd)S.guideAsks.push(T("",""));else S.guideAsks.splice(+t.dataset.gaskdel,1);renderEditor()},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(panel|dockStart|bubbles|talk|earcons|chips|swipeCompact|"));

/* Azu's place: never under the dock or the bubble; with the bubble on the welcome (the hero), it sits beside the bubble, just above the dock */
function pointAt(tg){var m=ph.querySelector(".mascot");if(!m)return;if(G.moment)tg=null;var a=m.querySelector(".marm"),st=S.guideSteps[G.i],b=ph.querySelector(".gbubble");
  if(dockOn()&&b&&st&&st.ref==="hero"){var y0=Math.max(70,b.offsetTop+b.offsetHeight-64),gw=ph.querySelector(".gbtn-wrap.intour.side-right");if(gw)y0=Math.max(70,Math.min(y0,gw.offsetTop-70));if(Math.abs((parseFloat(m.style.top)||0)-y0)>2)m.style.top=y0+"px";if(a)a.classList.remove("on");m.classList.remove("pointing");return}
  if(!tg){if(a)a.classList.remove("on");m.classList.remove("pointing");return}
  if(!a){a=document.createElement("i");a.className="marm";m.appendChild(a)}
  var fb=freeHeight(),pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,lo=(fb.bottom-pr.top)/q-72,y=Math.max(70,Math.min(lo,tg.y-27));y=besideWords(m,y,70,lo,tg.el);if(Math.abs((parseFloat(m.style.top)||0)-y)>2)m.style.top=y+"px";
  var mx=m.offsetLeft+8,my=y+34,ang=Math.atan2(tg.y-my,tg.x-mx)*180/Math.PI;ang=Math.max(120,Math.min(240,ang<0?ang+360:ang));
  a.style.transform="rotate("+(ang-180).toFixed(1)+"deg)";a.classList.add("on");m.classList.add("pointing")}
/* Azu points from beside the block, never over words: the ink of the words on the screen (each line of text, as a Range draws it:
   the block's and its neighbours') is measured, and Azu takes the nearest height where its tile covers none of them (above or below a
   line, or beside the block). Unity: the glyph boxes from TextElement.PostProcessTextVertices, the same search on the Azu element's top */
function wordBoxes(el){var key=G.i+"|"+S.view+"|"+Math.round(scr.scrollTop)+"|"+ph.offsetHeight;if(G.wbKey===key)return G.wb;
  var out=[],pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,sb=scr.getBoundingClientRect(),w=document.createTreeWalker(scr,NodeFilter.SHOW_TEXT),n,rg=document.createRange();
  while((n=w.nextNode())){if(!/\S/.test(n.nodeValue)||!n.parentElement)continue;var pb=n.parentElement.getBoundingClientRect();if(pb.bottom<sb.top||pb.top>sb.bottom||!pb.height)continue;
    var cs=getComputedStyle(n.parentElement);if(cs.visibility==="hidden"||cs.display==="none"||cs.opacity==="0")continue;rg.selectNodeContents(n);
    Array.prototype.forEach.call(rg.getClientRects(),function(r){if(r.width&&r.height&&r.bottom>sb.top&&r.top<sb.bottom)out.push({l:(r.left-pr.left)/q,t:(r.top-pr.top)/q,r:(r.right-pr.left)/q,b:(r.bottom-pr.top)/q})})}
  G.wbKey=key;G.wb=out;return out}
function azuOver(m,y,boxes){var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,r=m.getBoundingClientRect(),l=(r.left-pr.left)/q,rt=(r.right-pr.left)/q,h=r.height/q;
  return boxes.filter(function(b){return b.r>l+1&&b.l<rt-1&&b.b>y+1&&b.t<y+h-1})}
function besideWords(m,y,top,lo,el){if(!el)return y;var boxes=wordBoxes(el);if(!azuOver(m,y,boxes).length)return y;var h=m.getBoundingClientRect().height/(ph.getBoundingClientRect().height/ph.offsetHeight),best=null;
  boxes.forEach(function(b){[b.t-h-3,b.b+3].forEach(function(c){if(c<top||c>lo||azuOver(m,c,boxes).length)return;if(best===null||Math.abs(c-y)<Math.abs(best-y))best=c})});
  return best===null?y:best}
/* each row's demo: the tour at that height, the part ringed; a swipe; the talk button; the chips at Peek */
rowDemos({"guideCfg.panel":{tour:"g1",ring:".gdock|.gbar"},"guideCfg.dockStart":{tour:"g1",ring:".gdock|.gbtn-wrap.intour"},"guideCfg.miniCtl":{tour:"g1",dock:"mini",ring:".gd-mini|.gd-line"},
  "guideCfg.swipeCompact":{tour:"g1",dock:"mini",act:function(){var g=ph.querySelector(".gd-grab");if(g)gsTouch(g,90)},ring:function(){return S.dockH==="compact"?".gbtn-wrap.intour":".gd-grab"}},
  "guideCfg.bubbles":{tour:true,ring:".gbubble|.gd-cap"},"guideCfg.talk":{tour:"g1",ring:".gd-talk"},"guideCfg.earcons":{tour:"g1",act:function(){earcon(true)},ring:".gd-talk"},"guideCfg.chips":{tour:"g1",dock:"peek",ring:".gd-chips"}});
