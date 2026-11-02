/* ===================== v8o: the Guide Dock: one panel (mini / peek / full), Azu's own words in a bubble and a block's words in the block, a talk button with states, the conversation ===================== */
S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.bubbles="own";S.guideCfg.talk="both";S.guideCfg.earcons=true;S.guideCfg.chips=true;
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
  ICO={prev:'<path d="M7 6v12M18 6l-8 6 8 6z"/>',next:'<path d="M17 6v12M6 6l8 6-8 6z"/>',pause:'<path d="M8 5v14M16 5v14"/>',play:'<path d="M8 5l11 7-11 7z"/>',mute:'<path d="M4 9h4l5-4v14l-5-4H4zM17 9l4 6M21 9l-4 6"/>',sound:'<path d="M4 9h4l5-4v14l-5-4H4zM17 8a5 5 0 0 1 0 8"/>',stop:'<path d="M6 6l12 12M18 6 6 18"/>'};
function ico(k){return'<svg viewBox="0 0 24 24" aria-hidden="true">'+ICO[k]+'</svg>'}
var TALKW={idle:"dock_tap_talk",listening:"dock_listening",thinking:"dock_thinking",speaking:"dock_tap_stop",interrupted:"dock_go_ahead",notheard:"dock_not_heard"};
function talkState(){if(G.talk==="listening"||G.talk==="notheard"||G.talk==="interrupted")return G.talk;if(G.busy)return"thinking";if(G.on&&!G.paused&&(G.utter||G.timer))return"speaking";return"idle"}
/* the dock replaces the captions box and the control bar; the old ones stay as "Classic" */
var renderGuideV8o=renderGuide;
renderGuide=function(text){renderGuideV8o(text);["gdock","gbubble"].forEach(function(c){var o=ph.querySelector("."+c);if(o)o.remove()});if(!G.on||!dockOn())return;
  var cap=ph.querySelector(".gcap"),bar=ph.querySelector(".gbar"),ask=ph.querySelector(".gask");if(bar)bar.remove();if(ask)ask.remove();
  var st=S.guideSteps[G.i],own=cap&&!blockWords(text!=null?text:stepText(st));if(G.askOpen&&S.dockH!=="full")S.dockH="full";
  var n=S.guideSteps.length,ts=talkState(),seg='<div class="gd-seg">'+S.guideSteps.map(function(s,j){return'<button type="button" data-gseg="'+j+'" class="'+(j<G.i?"done":j===G.i?"now":"")+'" aria-label="'+esc(fmt(TXT("dock_stop_of"),j+1,n))+'"></button>'}).join("")+'</div>';
  var d=document.createElement("div");d.className="gdock h-"+S.dockH;d.setAttribute("role","region");d.setAttribute("aria-label",TXT("dock_label"));
  d.innerHTML='<button type="button" class="gd-grab" data-dock="cycle" aria-label="'+TXH(S.dockH==="full"?"dock_collapse":"dock_expand")+'"><i></i></button>'+seg+
    '<div class="gd-main"><div class="gd-line" data-dock="playpause"><small class="gd-stop">'+esc(fmt(TXT("dock_stop_of"),G.i+1,n))+(stepTitle(st)?' · '+esc(stepTitle(st)):"")+'</small><div class="gd-cap"></div></div>'+
    '<button type="button" class="gd-kb" data-dock="type" aria-label="'+TXH("dock_type")+'">'+KBD+'</button>'+
    '<button type="button" class="gd-talk s-'+ts+'" data-talk="1" aria-label="'+TXH("dock_ask")+'"><i class="ring"></i><i class="ring r2"></i>'+MIC+'</button></div>'+
    '<div class="gd-hint" aria-live="polite">'+TXH(TALKW[ts])+'</div>'+
    '<div class="gd-peek"><div class="gd-ctl"><button type="button" data-g="prev" aria-label="Previous">'+ico("prev")+'</button><button type="button" data-g="'+(G.paused?"play":"pause")+'" aria-label="'+(G.paused?"Play":"Pause")+'">'+ico(G.paused?"play":"pause")+'</button><button type="button" data-g="next" aria-label="Next">'+ico("next")+'</button>'+
      '<button type="button" data-g="mute" aria-pressed="'+!!S.guideMute+'" aria-label="Sound">'+ico(S.guideMute?"mute":"sound")+'</button><button type="button" data-g="cc" aria-pressed="'+S.guideCfg.captions+'" class="cc">CC</button><button type="button" data-g="stop" aria-label="Stop the guide">'+ico("stop")+'</button></div>'+
      (S.guideCfg.chips?'<div class="gd-chips">'+S.guideAsks.map(function(q,i){return'<button type="button" data-chip="'+i+'">'+esc(L(q))+'</button>'}).join("")+'</div>':"")+'</div>'+
    '<div class="gd-full"><div class="gd-thread" aria-live="polite">'+G.thread.map(function(m){return'<div class="gm-'+m.who+'">'+esc(m.text)+'</div>'}).join("")+(G.busy?'<div class="gm-azu typing"><i></i><i></i><i></i></div>':"")+(G.micNote?'<div class="gm-note">'+esc(G.micNote)+'</div>':"")+'</div>'+
      '<form class="gd-form"><input id="gq" class="gin" type="text" autocomplete="off" placeholder="'+TXH("dock_type")+'"><button type="submit" aria-label="'+TXH("dock_send")+'">&#10148;</button></form></div>';
  ph.appendChild(d);
  /* the words: Azu's own in its bubble (above the dock), or in the dock's line; a block's words are lit in the block, so the line shows the step */
  if(cap){if(own&&S.guideCfg.bubbles==="own"&&S.dockH!=="full"){var b=document.createElement("div");b.className="gbubble";b.setAttribute("role","status");cap.classList.add("gb-text");b.appendChild(cap);ph.appendChild(b);placeBubble()}
    else if(own||S.dockH==="full"){d.querySelector(".gd-cap").appendChild(cap)}else cap.remove()}
  var th=d.querySelector(".gd-thread");if(th)th.scrollTop=th.scrollHeight;var m=ph.querySelector(".mascot");if(m)m.dataset.state=ts};
/* the bubble sits just above the dock, its tail towards Azu */
function placeBubble(){var b=ph.querySelector(".gbubble"),d=ph.querySelector(".gdock"),m=ph.querySelector(".mascot");if(!b||!d)return;var q=ph.getBoundingClientRect().height/ph.offsetHeight;
  b.style.bottom=(ph.offsetHeight-d.offsetTop+10)+"px";var up=m&&m.offsetTop+30<d.offsetTop-b.offsetHeight-10;b.classList.toggle("tail-up",!!up)}
/* the free height for the block being told: above the dock and the bubble */
freeHeight=function(){var box=scr.getBoundingClientRect(),lo=box.bottom;[".gcap",".gbar",".gdock",".gbubble"].forEach(function(s){var e=ph.querySelector(s);if(e){var r=e.getBoundingClientRect();if(r.height)lo=Math.min(lo,r.top)}});return{top:box.top,bottom:lo}};
/* keep the talk state and the bubble's place in step with the voice */
/* a bubble shows one sentence at a time (2 lines at most), the sentence being said */
function bubbleSentence(){var b=ph.querySelector(".gbubble .gcap");if(!b)return;var ws=b.querySelectorAll(".gw"),k=Math.max(0,G.hl&&G.hl.k>=0?G.hl.k:0),sid=[],n=0;
  for(var i=0;i<ws.length;i++){sid.push(n);if(/[.!?…]["”»)]*$/.test(ws[i].textContent))n++}var cur=sid[Math.min(k,ws.length-1)];for(var j=0;j<ws.length;j++)ws[j].classList.toggle("gb-off",sid[j]!==cur)}
setInterval(function(){if(!G.on||!dockOn())return;bubbleSentence();var t=ph.querySelector(".gd-talk"),ts=talkState();if(t&&!t.classList.contains("s-"+ts)){t.className="gd-talk s-"+ts;var h=ph.querySelector(".gd-hint");if(h)h.textContent=TXT(TALKW[ts]);var m=ph.querySelector(".mascot");if(m)m.dataset.state=ts}placeBubble()},150);
/* the conversation: the tour's lines and the questions and answers */
var runStepV8o=runStep;runStep=function(){runStepV8o();var st=S.guideSteps[G.i];if(st){var t=stepText(st);if(t&&(!G.thread.length||G.thread[G.thread.length-1].text!==t))G.thread.push({who:"azu",text:t});if(G.thread.length>60)G.thread.shift()}};
var askV8o=ask;ask=function(q){if(!q||!q.trim())return;G.thread.push({who:"me",text:q.trim()});G.micNote="";G.talk="idle";askV8o(q)};
var sayV8o=say;say=function(text,onWord,onEnd){if(G.askOpen&&G.answer&&text===G.answer){if(!G.thread.length||G.thread[G.thread.length-1].text!==text)G.thread.push({who:"azu",text:text});renderGuide(text)}sayV8o(text,onWord,onEnd)};
var startTourV8o=startTour;startTour=function(){S.dockH=S.guideCfg.dockStart;G.thread=[];G.talk="idle";G.askOpen=false;G.answer="";G.micNote="";startTourV8o()};
/* ----- the talk button: a tap listens until the visitor stops; a long press is hold-to-talk; speaking, a tap interrupts and listens ----- */
function earcon(up){if(!S.guideCfg.earcons)return;try{actx=actx||new (window.AudioContext||window.webkitAudioContext)();var o=actx.createOscillator(),g=actx.createGain(),t=actx.currentTime;o.type="sine";o.frequency.setValueAtTime(up?620:880,t);o.frequency.exponentialRampToValueAtTime(up?880:560,t+.12);g.gain.setValueAtTime(.0001,t);g.gain.exponentialRampToValueAtTime(.08,t+.02);g.gain.exponentialRampToValueAtTime(.0001,t+.16);o.connect(g);g.connect(actx.destination);o.start(t);o.stop(t+.18)}catch(e){}}
function micBlockedNote(){return TXT(window.top!==window?"dock_mic_framed":"dock_mic_none")}
function listen(hold){var R=window.SpeechRecognition||window.webkitSpeechRecognition;if(G.on&&!G.paused&&(G.utter||G.timer)){pauseTour();G.talk="interrupted";earcon(false);haptic()}
  if(!R){heard(null,"none");return}var rec;try{rec=new R()}catch(e){heard(null,"none");return}
  rec.lang=S.lang==="pt"?"pt-PT":"en-GB";rec.interimResults=true;rec.continuous=!!hold;G.rec=rec;var got="";
  rec.onresult=function(ev){got="";for(var i=0;i<ev.results.length;i++)got+=ev.results[i][0].transcript;var h=ph.querySelector(".gd-hint");if(h)h.textContent=got||TXT("dock_listening")};
  rec.onerror=function(e){G.rec=null;heard(null,e.error)};rec.onend=function(){if(G.rec!==rec)return;G.rec=null;heard(got,null)};
  try{rec.start();G.talk="listening";earcon(true);haptic();renderGuide()}catch(e){heard(null,"start")}}
function heard(q,err){if(q&&q.trim()){G.talk="idle";earcon(false);ask(q);return}
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
  var k=t.dataset.dock;if(k==="cycle"){S.dockH={mini:"peek",peek:"full",full:"mini"}[S.dockH];G.askOpen=S.dockH==="full";renderGuide();return}
  if(k==="playpause"){if(S.dockH==="full")return;if(G.paused)resumeTour();else{G.explicitPause=true;pauseTour();G.explicitPause=false}return}
  if(k==="type"){S.dockH="full";G.askOpen=true;renderGuide();var i=ph.querySelector("#gq");if(i)i.focus({preventScroll:true})}},true);
/* a swipe on the grabber: up raises the dock one height, down lowers it */
var grabY=null;ph.addEventListener("pointerdown",function(e){if(e.target.closest(".gd-grab"))grabY=e.clientY},true);
ph.addEventListener("pointerup",function(e){if(grabY==null)return;var dy=e.clientY-grabY;grabY=null;if(Math.abs(dy)<24)return;var o=["mini","peek","full"],i=o.indexOf(S.dockH);S.dockH=o[Math.max(0,Math.min(2,i+(dy<0?1:-1)))];G.askOpen=S.dockH==="full";e.stopPropagation();renderGuide()},true);
ph.addEventListener("submit",function(e){var f=e.target.closest(".gd-form");if(!f)return;e.preventDefault();e.stopPropagation();var i=f.querySelector("#gq");if(i&&i.value.trim()){var q=i.value;i.value="";ask(q)}},true);

/* Editor: 6 · Guide > Guide panel (after Captions); Specific Marker > Guide script > Suggested questions */
var edCardV8o=edCard;
edCard=function(){var d=document.createElement("div");d.innerHTML=edCardV8o();var ca=d.querySelector('[data-fo="s7cap"]');if(!ca)return d.innerHTML;
  ca.insertAdjacentHTML("beforebegin",comp("s7dock","one panel for the tour's words, its controls and the conversation",
    row("Panel",pseg("guideCfg.panel",["dock","classic"],["Dock","Classic"]),"Dock: one panel at the bottom with three heights: Mini (the step, the words, the talk button), Peek (the controls and suggested questions), Full (the conversation and a text field). A tap on its line pauses or plays; the grabber or a swipe changes its height. Classic: the captions box over the control bar.")+
    row("Starts at",pseg("guideCfg.dockStart",["mini","peek"],["Mini","Peek"]),"Its height when the tour starts.",true)+
    row("Azu's own words",pseg("guideCfg.bubbles",["own","panel"],["In a bubble","In the panel"]),"In a bubble: what Azu says in its own words (the welcome, an ask, an answer) appears in a bubble above the panel, its tail towards Azu; a block's words light up in the block itself, with no bubble. Every line also goes to the conversation (Full).")+
    row("Talk button",pseg("guideCfg.talk",["both","tap","hold"],["Tap or hold","Tap","Hold"]),"Tap: listens until the visitor stops speaking; a second tap sends. Hold: listens while held, sends on release. While Azu speaks, the button interrupts it and listens. The keyboard is always beside it. Never always-listening.")+
    row("Sounds",pseg("guideCfg.earcons",["true","false"],["On","Off"]),"A short rising tone when it starts listening and a falling one when it stops (under 300 ms). With a light haptic.")+
    row("Suggested questions",pseg("guideCfg.chips",["true","false"],["Shown","Hidden"]),"Chips in the Peek height (the point's own, in the Guide script). A tap asks it.")+
    row("Try",'<button class="del" type="button" data-docktry="1">Try</button>',"Starts the tour with the dock.")));
  var f=innerOf(d,"s7dock");if(f)f.insertAdjacentHTML("beforeend",ownbar("s7dock")+ownPanels("s7dock"));return d.innerHTML};
var edMarkerV8o=edMarker;
edMarker=function(){var d=document.createElement("div");d.innerHTML=edMarkerV8o();var f=innerOf(d,"m3");if(!f)return d.innerHTML;
  f.insertAdjacentHTML("beforeend",row("Suggested questions",'<div class="gasks">'+S.guideAsks.map(function(q,i){return'<div class="btnrow"><input class="u" data-gask="'+i+'|en" value="'+esc(q.en||"")+'"><input class="u" data-gask="'+i+'|pt" value="'+esc(q.pt||"")+'"><button class="del" type="button" data-gaskdel="'+i+'">Remove</button></div>'}).join("")+
    '<button class="ubtn sm" type="button" data-gaskadd="1">+ Add a question</button></div>',"Chips in the guide panel (EN / PT): questions a visitor might ask here. A tap asks Azu."));return d.innerHTML};
ed.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.gask)return;e.stopImmediatePropagation();var q=t.dataset.gask.split("|");S.guideAsks[+q[0]][q[1]]=t.value},true);
ed.addEventListener("click",function(e){var t=e.target.closest("[data-gaskadd],[data-gaskdel],[data-docktry]");if(!t)return;e.stopPropagation();
  if(t.dataset.docktry){if(G.on)stopTour();S.guideCfg.panel="dock";startTour();return}
  if(t.dataset.gaskadd)S.guideAsks.push(T("",""));else S.guideAsks.splice(+t.dataset.gaskdel,1);renderEditor()},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(panel|dockStart|bubbles|talk|earcons|chips|"));

/* Azu's place: never under the dock or the bubble; with the bubble on the welcome (the hero), it sits beside the bubble, just above the dock */
pointAt=function(tg){var m=ph.querySelector(".mascot");if(!m)return;if(G.moment)tg=null;var a=m.querySelector(".marm"),st=S.guideSteps[G.i],b=ph.querySelector(".gbubble");
  if(dockOn()&&b&&st&&st.ref==="hero"){var y0=Math.max(70,b.offsetTop+b.offsetHeight-64);if(Math.abs((parseFloat(m.style.top)||0)-y0)>2)m.style.top=y0+"px";if(a)a.classList.remove("on");m.classList.remove("pointing");return}
  if(!tg){if(a)a.classList.remove("on");m.classList.remove("pointing");return}
  if(!a){a=document.createElement("i");a.className="marm";m.appendChild(a)}
  var fb=freeHeight(),pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,lo=(fb.bottom-pr.top)/q-72,y=Math.max(70,Math.min(lo,tg.y-27));if(Math.abs((parseFloat(m.style.top)||0)-y)>2)m.style.top=y+"px";
  var mx=m.offsetLeft+8,my=y+34,ang=Math.atan2(tg.y-my,tg.x-mx)*180/Math.PI;ang=Math.max(120,Math.min(240,ang<0?ang+360:ang));
  a.style.transform="rotate("+(ang-180).toFixed(1)+"deg)";a.classList.add("on");m.classList.add("pointing")};
