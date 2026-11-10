/* The Guide Dock (the guide panel): Compact (hidden), Mini, Peek, Full, drawn in one of four Looks (round 11: Paper glass, Ink glass,
   Player sheet, Teleprompter); Pause and Stop at Mini, the hide control; the words like a teleprompter, the progress bar, the voice moving,
   the speed chip; the bubble, the talk button and its states, the conversation, the suggested questions; the ✦ growing into it. */
S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.miniCtl="pausestop";S.guideCfg.swipeCompact=true;S.guideCfg.bubbles="own";S.guideCfg.talk="both";S.guideCfg.earcons=true;S.guideCfg.chips=true;
/* round 11: the panel's Look and its parts */
S.guideCfg.look="ink";S.guideCfg.pWords="prompter";S.guideCfg.pBars=true;S.guideCfg.pSpeed=true;
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
var HIDE='<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 9l6 6 6-6"/></svg>';

/* ----- round 11: the Looks. Each has two states on the dock's heights: Mini (its pill, capsule, mini player or one line) at Mini, and
   Open (its panel) at Peek and Full. Peek = Open (asking by typing focuses its field: the keyboard, ✦ › Ask); Full = Open with the whole
   text shown (Read all: the tour's lines so far and the conversation, the line said now marked); Compact = hidden, as before.
   Unity: one UXML template per Look (a VisualTreeAsset), the same controller and the same element names (gd-play, gd-seek…), so a Look
   is a swap of template and USS; the glass is a translucent fill with a light edge (no backdrop blur in 6.3), a gradient a ramp texture */
var DLOOKS=[["paper","Paper glass"],["ink","Ink glass"],["player","Player sheet"],["prompter","Teleprompter"]];
function dkLook(){var l=S.guideCfg.look;return DLOOKS.some(function(x){return x[0]===l})?l:"ink"}
/* the icons of the drawn Looks: filled transport glyphs, stroked chevrons */
var FI={prev:'<path d="M18 5.5v13a1 1 0 0 1-1.55.83L7 13v5H5V6h2v5l9.45-6.33A1 1 0 0 1 18 5.5z"/>',next:'<path d="M6 5.5v13a1 1 0 0 0 1.55.83L17 13v5h2V6h-2v5L7.55 4.67A1 1 0 0 0 6 5.5z"/>',
  pause:'<rect x="6" y="5" width="4" height="14" rx="1.5"/><rect x="14" y="5" width="4" height="14" rx="1.5"/>',play:'<path d="M8 5.6v12.8a1 1 0 0 0 1.53.85l10.2-6.4a1 1 0 0 0 0-1.7L9.53 4.75A1 1 0 0 0 8 5.6z"/>',
  stop:'<rect x="5" y="5" width="14" height="14" rx="3"/>',up:'<polyline points="6 15 12 9 18 15"/>',down:'<polyline points="6 9 12 15 18 9"/>',
  all:'<polyline points="7 10 12 5 17 10"/><polyline points="7 14 12 19 17 14"/>',lines:'<line x1="4" y1="7" x2="20" y2="7"/><line x1="4" y1="12" x2="20" y2="12"/><line x1="4" y1="17" x2="14" y2="17"/>',
  send:'<line x1="5" y1="12" x2="19" y2="12"/><polyline points="13 6 19 12 13 18"/>',wave:'<path d="M4 10v4M8 6v12M12 3v18M16 7v10M20 10v4"/>'};
function fi(k){return'<svg viewBox="0 0 24 24" aria-hidden="true" class="fi">'+FI[k]+'</svg>'}
function si(k){return'<svg viewBox="0 0 24 24" aria-hidden="true" class="si">'+FI[k]+'</svg>'}
/* ----- the time: each stop's length from its words' timing (a recorded line's own word times, else the words at the voice's speed) ----- */
function stopSecs(j){var st=S.guideSteps[j];if(!st)return 0;var t=stepText(st),sp=guideSpeed(),r=typeof recFile==="function"?recFile(t):null,w=r&&r.file&&r.file.words;
  if(w&&w.length)return w[w.length-1][1]/sp+.7;return Math.max(1,(t.match(/\S+/g)||[]).length)*60/(170*sp)+.7}
function dkTimes(){var a=[],tot=0;for(var j=0;j<S.guideSteps.length;j++){var s=stopSecs(j);a.push({at:tot,len:s});tot+=s}return{stops:a,tot:tot}}
/* the share of the line said: the stop's own line (a recorded line by its clock, else its words) */
function dkShare(){var st=S.guideSteps[G.i];if(!st||!G.sayLine||G.sayLine.text!==stepText(st))return 0;if(G.recAudio&&G.recAudio.duration>0)return Math.min(1,G.recAudio.currentTime/G.recAudio.duration);
  var w=(G.words||[]).length,k=G.hl&&G.hl.k>=0?G.hl.k+1:0;return w?Math.min(1,k/w):0}
function dkElapsed(T){var c=T.stops[G.i];return c?c.at+dkShare()*(c.len-.7):0}
function mmss(s){s=Math.max(0,Math.round(s));return Math.floor(s/60)+":"+("0"+s%60).slice(-2)}
/* ----- the progress bar: one segment a stop (its length the stop's), the knob where the voice is; a drag seeks (guide/controls.js) ----- */
function dkSeek(kind){var T=dkTimes(),el=dkElapsed(T),n=S.guideSteps.length,sh=dkShare();
  return'<div class="gd-seg gd-seek sk-'+kind+'" role="slider" tabindex="0" aria-label="'+TXH("dock_seek")+'" aria-valuemin="0" aria-valuemax="'+Math.round(T.tot)+'" aria-valuenow="'+Math.round(el)+'" aria-valuetext="'+esc(mmss(el)+" / "+mmss(T.tot)+" · "+fmt(TXT("dock_stop_of"),G.i+1,n))+'">'+
    '<span class="sk-track">'+T.stops.map(function(s,j){return'<i data-gseg="'+j+'" class="'+(j<G.i?"done":j===G.i?"now":"")+'" style="flex:'+s.len.toFixed(2)+' 1 0"><b style="width:'+(j<G.i?100:j===G.i?sh*100:0).toFixed(1)+'%"></b></i>'}).join("")+'</span>'+
    '<span class="sk-knob" aria-hidden="true"></span></div>'}
function dkTimeHtml(kind){var T=dkTimes(),el=dkElapsed(T);if(kind==="both")return'<span class="gd-time" data-t="both">'+mmss(el)+" / "+mmss(T.tot)+'</span>';
  return'<span class="gd-time" data-t="'+kind+'">'+(kind==="el"?mmss(el):kind==="rem"?"-"+mmss(T.tot-el):mmss(T.tot))+'</span>'}
/* ----- the words like a teleprompter: the line said now, its spoken word marked, the line before faded above it and the next below;
   they move up as the voice goes on. A line is a sentence, or a phrase of a long one (at a comma after 5 words, at most 12 words).
   Unity: three Labels in a VisualElement; on a new line their translate is animated up (experimental.animation), the word marked by
   rich text (<mark>) from the voice's word times ----- */
function tpLines(text){var ws=String(text||"").match(/\S+/g)||[],out=[],a=0;for(var i=0;i<ws.length;i++){var w=ws[i],n=i-a+1;
    if(/[.!?…]["”»)]*$/.test(w)||(/[,:;—]["”»)]*$/.test(w)&&n>=5)||n>=12||i===ws.length-1){out.push({a:a,b:i});a=i+1}}return{ws:ws,ls:out}}
function tpLineOf(ls,k){for(var i=0;i<ls.length;i++)if(k<=ls[i].b)return i;return Math.max(0,ls.length-1)}
/* the word being said in this text (its index), when this text is the line being said */
function dkWordK(text){return G.sayLine&&G.sayLine.text===text&&G.hl&&G.hl.k>=0?G.hl.k:-1}
function tpWords(P,a,b,k){var h="";for(var j=a;j<=b;j++)h+=(j>a?" ":"")+'<span class="tw'+(j<k?" said":j===k?" on":"")+'" data-k="'+j+'">'+esc(P.ws[j])+'</span>';return h}
function tpHtml(text,variant){var P=tpLines(text),k=dkWordK(text),i=tpLineOf(P.ls,Math.max(0,k));
  if(variant==="mini"){var l=P.ls[i];if(!l)return'<div class="gd-tp tp-mini" data-line="0" data-v="mini"><p class="tp-now">&nbsp;</p></div>';var a0=k>l.a+2?k-2:l.a;return'<div class="gd-tp tp-mini" data-line="'+i+'" data-v="mini"><p class="tp-now">'+(a0>l.a?"…":"")+tpWords(P,a0,l.b,k)+'</p></div>'}
  var sp={ink:[1,0],paper:[1,1],player:[0,0],prompter:[2,1]}[variant]||[1,1],out="",line=function(x,c){var l=P.ls[x];return'<p class="'+c+(l?"":" tp-empty")+'">'+(l?tpWords(P,l.a,l.b,c==="tp-now"?k:-1):"&nbsp;")+'</p>'};
  for(var b=sp[0];b>=1;b--)out+=line(i-b,"tp-prev");out+=line(i,"tp-now");for(var a=1;a<=sp[1];a++)out+=line(i+a,"tp-next");
  return'<div class="gd-tp tp-'+variant+'" data-line="'+i+'" data-v="'+variant+'">'+out+'</div>'}
function dkWordsOn(){return S.guideCfg.pWords!=="off"&&!!S.guideCfg.captions}
/* the words move on with the voice: a new line slides up; within a line the word said is marked */
function dkWord(k){if(!dkWordsOn()||G.tpText==null||!G.sayLine||G.sayLine.text!==G.tpText)return;var P=tpLines(G.tpText);
  ph.querySelectorAll(".gdock .gd-tp").forEach(function(tp){var i=tpLineOf(P.ls,Math.max(0,k));if(tp.dataset.v==="mini"){var x0=document.createElement("div");x0.innerHTML=tpHtml(G.tpText,"mini");tp.replaceWith(x0.firstChild);return}if(String(i)!==tp.dataset.line){var v=tp.dataset.v,x=document.createElement("div");x.innerHTML=tpHtml(G.tpText,v);var n=x.firstChild;if(!S.reduce&&+i>+tp.dataset.line)n.classList.add("tp-up");tp.replaceWith(n);return}
    tp.querySelectorAll(".tp-now .tw").forEach(function(w){var j=+w.dataset.k;w.classList.toggle("said",j<k);w.classList.toggle("on",j===k)})});
  ph.querySelectorAll(".gdock .gm-now .tw").forEach(function(w){var j=+w.dataset.k;w.classList.toggle("said",j<k);w.classList.toggle("on",j===k)})}
on("markWord.after",function(k){if(G.on&&dockOn())dkWord(k)});
/* ----- the parts every Look shares ----- */
function dkAv(c){return'<span class="gd-av'+(c?" "+c:"")+'" aria-hidden="true">'+mascotStill()+'</span>'}
function dkBars(n){if(!S.guideCfg.pBars)return"";var h="";for(var i=0;i<n;i++)h+="<i></i>";return'<span class="gd-bars" aria-hidden="true">'+h+'</span>'}
function dkPP(c){var p=G.paused;return'<button type="button" class="'+c+'" data-g="'+(p?"play":"pause")+'" aria-label="'+(p?TXH("guide_ctrl_play"):TXH("guide_ctrl_pause"))+'">'+fi(p?"play":"pause")+'</button>'}
function dkStopB(c){return'<button type="button" class="'+c+'" data-g="stop" aria-label="'+TXH("guide_ctrl_stop")+'">'+fi("stop")+'</button>'}
function dkStep(k,c){return'<button type="button" class="'+c+'" data-g="'+k+'" aria-label="'+TXH("guide_ctrl_"+k)+'">'+fi(k)+'</button>'}
function spd(v){return String(+(+v).toFixed(2))}
function dkSpeed(){if(S.guideCfg.pSpeed===false)return"";var v=guideSpeed();return'<button type="button" class="gd-speed" data-g="speed" aria-label="'+esc(fmt(TXT("dock_speed"),spd(v)))+'">'+spd(v)+'×</button>'}
function dkCtl(c){return'<div class="gd-ctl'+(c?" "+c:"")+'">'+dkSpeed()+dkStep("prev","gd-pn")+dkPP("gd-play")+dkStep("next","gd-pn")+dkStopB("gd-stopb")+'</div>'}
function dkKb(){return'<button type="button" class="gd-kb" data-dock="type" aria-label="'+TXH("dock_type")+'">'+KBD+'</button>'}
/* Controls at Mini: Pause + Stop, Pause only, or Tap on the line (no buttons; the keyboard to ask by typing) */
function dkMiniCtl(){var mc=S.guideCfg.miniCtl;if(mc==="line")return dkKb();return'<div class="gd-mini">'+dkPP("gd-mc gd-mpp")+(mc==="pausestop"?dkStopB("gd-mc gd-mstop"):"")+'</div>'}
function dkOpenB(c){return'<button type="button" class="gd-grab '+(c||"gd-openb")+'" data-dock="cycle" aria-label="'+TXH("dock_open")+'">'+(c?dkAv():si("up"))+'</button>'}
function dkHide(){return'<button type="button" class="gd-hide" data-dock="hide" aria-label="'+TXH("dock_hide")+'">'+si("down")+'</button>'}
function dkHandle(h){return'<button type="button" class="gd-grab gd-handle" data-dock="cycle" aria-label="'+TXH(h==="full"?"dock_collapse":"dock_expand")+'"><i></i></button>'}
function dkSmall(){return'<button type="button" class="gd-small" data-dock="mini" aria-label="'+TXH("dock_small")+'">'+si("down")+'</button>'}
function dkAa(){return dkWordsOff()?"":'<button type="button" class="gd-aa" data-g="cc" aria-pressed="'+!!S.guideCfg.captions+'" aria-label="'+TXH("dock_words")+'">Aa</button>'}
function dkWordsOff(){return S.guideCfg.pWords==="off"}
function dkAll(h,icon){var f=h==="full";return'<button type="button" class="gd-all'+(icon?" ic":"")+'" data-dock="readall" aria-pressed="'+f+'" aria-label="'+TXH(f?"dock_readless":"dock_readall")+'">'+(icon?si(icon):TXH(f?"dock_readless":"dock_readall"))+'</button>'}
function dkChips(kb){var c=S.guideCfg.chips?S.guideAsks.map(function(q,i){return'<button type="button" data-chip="'+i+'">'+esc(L(q))+'</button>'}).join(""):"";return c||kb?'<div class="gd-chips">'+(kb?dkKb():"")+c+'</div>':""}
function dkForm(){return'<form class="gd-form"><input id="gq" class="gin" type="text" autocomplete="off" placeholder="'+TXH("dock_ask_ph")+'" aria-label="'+TXH("dock_type")+'"><button type="submit" class="gd-send" aria-label="'+TXH("dock_send")+'">'+si("send")+'</button></form>'}
function dkTalk(ts,label){return'<button type="button" class="gd-talk s-'+ts+'" data-talk="1" aria-label="'+TXH("dock_ask")+'"><i class="ring"></i><i class="ring r2"></i>'+(label?si("wave")+'<span>'+TXH("dock_voice")+'</span>':MIC)+'</button>'}
function dkHint(ts){return'<div class="gd-hint'+(/listening|thinking|interrupted|notheard/.test(ts)?" on":"")+'" aria-live="polite">'+TXH(TALKW[ts])+'</div>'}
function dkStatus(ts){return G.paused?TXH("dock_paused"):ts!=="idle"&&ts!=="speaking"?TXH(TALKW[ts]):TXH("dock_telling")}
/* the whole text (Full): the tour's lines so far and the questions and answers, the line said now marked */
function dkThread(){var now=-1;G.thread.forEach(function(m,i){if(m.who==="azu"&&m.text===G.tpText)now=i});
  return'<div class="gd-thread" aria-live="polite">'+pipe("dockThread.html",G.thread.map(function(m,i){if(i!==now)return'<div class="gm-'+m.who+'">'+esc(m.text)+'</div>';var P=tpLines(m.text);return'<div class="gm-'+m.who+' gm-now">'+tpWords(P,0,P.ws.length-1,dkWordK(m.text))+'</div>'}).join(""))+
    (G.busy?'<div class="gm-azu typing"><i></i><i></i><i></i></div>':"")+(G.micNote?'<div class="gm-note">'+esc(G.micNote)+'</div>':"")+'</div>'}
/* the words area of Open: the teleprompter (Peek), the whole text (Full); Words Off: none (the words still light up in the block) */
function dkWords(h,v){if(h==="full")return dkThread();if(dkWordsOff())return"";return dkWordsOn()?tpHtml(G.tpText||"",v):'<div class="gd-tp gd-tpoff" data-v="'+v+'"><p class="tp-now">&nbsp;</p></div>'}
/* ----- the four Looks, drawn as the Architect's layouts (scratchpad gp/project: Main = Paper glass, InkGlass, Player, Prompter) ----- */
function dockMini(L){var n=S.guideSteps.length,st=S.guideSteps[G.i],lab=fmt(TXT("dock_stop_of"),G.i+1,n),ctl=dkMiniCtl(),line=dkWordsOn()?tpHtml(G.tpText||"","mini"):"";
  if(L==="paper")return'<div class="gd-mrow gd-main">'+dkAv()+'<div class="gd-line" data-dock="playpause"><small class="gd-stop">Azu · '+esc(lab)+dkBars(7)+'</small>'+(line||'<span class="gd-mt">'+esc(stepTitle(st))+'</span>')+'</div>'+ctl+dkOpenB()+'</div><span class="gd-mprog" aria-hidden="true"><b></b></span>'+dkHide();
  if(L==="player")return'<span class="gd-mprog top" aria-hidden="true"><b></b></span><div class="gd-mrow gd-main">'+dkOpenB("gd-avb")+'<div class="gd-line" data-dock="playpause"><b class="gd-mt">'+esc(stepTitle(st))+'</b><small class="gd-stop">Azu · '+esc(lab)+' · '+dkTimeHtml("el")+'</small></div>'+ctl+dkStep("next","gd-mnext")+'</div>'+dkHide();
  if(L==="prompter")return(line?'<div class="gd-pline">'+line+'</div>':"")+'<div class="gd-mrow gd-main gd-capsule"><span class="gd-line" data-dock="playpause"><small class="gd-stop vh">'+esc(lab+" · "+stepTitle(st))+'</small>'+dkBars(7)+'</span>'+ctl+dkOpenB()+'</div>'+dkHide();
  /* Ink glass: Azu inside its progress ring, the voice's bars, Pause, open */
  return'<div class="gd-mrow gd-main"><span class="gd-ring" aria-hidden="true"><svg viewBox="0 0 52 52"><circle cx="26" cy="26" r="23" class="tr"/><circle cx="26" cy="26" r="23" class="pg"/></svg>'+dkAv()+'</span><span class="gd-line" data-dock="playpause"><small class="gd-stop vh">'+esc(lab+" · "+stepTitle(st))+'</small>'+dkBars(7)+'</span>'+ctl+dkOpenB()+'</div>'+dkHide()}
function dockOpen(L,h,ts){var n=S.guideSteps.length,st=S.guideSteps[G.i],lab=fmt(TXT("dock_stop_of"),G.i+1,n),nm=stepTitle(st),stop='<span class="gd-stop">'+esc(lab)+(nm?" · "+esc(nm):"")+'</span>',typing=h==="peek"&&G.typing;
  if(L==="paper")return dkHandle(h)+'<div class="gd-head">'+dkAv()+'<div class="gd-who"><b>Azu'+dkBars(9)+'</b><small class="gd-status">'+dkStatus(ts)+'</small></div><span class="gd-sp"></span>'+dkAa()+dkAll(h,"lines")+dkSmall()+'</div>'+
    dkWords(h,"paper")+'<div class="gd-where"><div class="gd-lab">'+stop+dkTimeHtml("both")+'</div>'+dkSeek("segs")+'</div>'+dkCtl()+
    '<div class="gd-askrow"><span class="gd-askf">'+KBD+dkForm()+'</span>'+dkTalk(ts)+'</div>'+dkChips(false)+dkHint(ts);
  if(L==="player")return dkHandle(h)+dkSmall()+'<div class="gd-card"><div class="gd-cardw"><small class="gd-eye">AZU · '+esc(lab.toUpperCase())+'</small>'+dkWords(h,"player")+'</div><div class="gd-side">'+dkAll(h)+dkAa()+'</div></div>'+
    '<div class="gd-where">'+dkSeek("line")+'<div class="gd-lab3">'+dkTimeHtml("el")+stop+dkTimeHtml("rem")+'</div></div>'+dkCtl()+dkChips(false)+
    '<div class="gd-askbar">'+dkForm()+dkTalk(ts,true)+'</div>'+dkHint(ts);
  if(L==="prompter")return dkHandle(h)+'<div class="gd-head">'+dkAv()+'<small class="gd-eye">AZU · '+esc(lab.toUpperCase())+'</small><span class="gd-sp"></span>'+dkAll(h)+dkAa()+dkSmall()+'</div>'+dkWords(h,"prompter")+
    '<div class="gd-where gd-hrow">'+dkTimeHtml("el")+dkSeek("hair")+dkTimeHtml("tot")+'</div><div class="gd-lab gd-lab1">'+stop+'</div>'+(typing||h==="full"?'<div class="gd-askrow">'+dkForm()+'</div>'+dkChips(false):dkChips(true))+
    '<div class="gd-botrow">'+dkCtl("gd-capsule")+dkTalk(ts)+'</div>'+dkHint(ts);
  /* Ink glass */
  return dkHandle(h)+dkSmall()+'<div class="gd-wrow">'+dkAv()+dkWords(h,"ink")+'<div class="gd-side">'+dkAll(h,"all")+dkAa()+'</div></div>'+
    '<div class="gd-where">'+dkSeek("dots")+'<div class="gd-lab">'+stop+dkTimeHtml("both")+'</div></div>'+dkCtl()+dkChips(false)+'<div class="gd-askrow">'+dkForm()+dkTalk(ts)+'</div>'+dkHint(ts)}
function dockHtml(h){var L=dkLook(),ts=talkState();return h==="mini"?dockMini(L):dockOpen(L,h,ts)}
/* ----- the progress, the time and the ring follow the voice (4 times a second); a drag on the bar holds them (guide/controls.js) ----- */
function dkTick(){var d=ph.querySelector(".gdock");if(!d||!G.on)return;var T=dkTimes(),el=dkElapsed(T),sh=dkShare(),f=T.tot?el/T.tot:0;
  if(!G.seekDrag){var sk=d.querySelector(".gd-seek");if(sk){sk.querySelectorAll("[data-gseg]").forEach(function(s){var j=+s.dataset.gseg;s.className=j<G.i?"done":j===G.i?"now":"";var b=s.firstChild;if(b)b.style.width=(j<G.i?100:j===G.i?sh*100:0).toFixed(1)+"%"});
      sk.setAttribute("aria-valuenow",Math.round(el));sk.setAttribute("aria-valuetext",mmss(el)+" / "+mmss(T.tot)+" · "+fmt(TXT("dock_stop_of"),G.i+1,S.guideSteps.length));dkKnob(sk,G.i,sh)}
    d.querySelectorAll(".gd-time").forEach(function(t){var k=t.dataset.t;t.textContent=k==="both"?mmss(el)+" / "+mmss(T.tot):k==="el"?mmss(el):k==="rem"?"-"+mmss(T.tot-el):mmss(T.tot)})}
  d.querySelectorAll(".gd-mprog b").forEach(function(b){b.style.width=(f*100).toFixed(1)+"%"});var pg=d.querySelector(".gd-ring .pg");if(pg)pg.style.strokeDasharray=(2*Math.PI*23*f).toFixed(1)+" 200"}
/* the knob on a stop's segment, at the share given */
function dkKnob(sk,j,sh){var s=sk.querySelector('[data-gseg="'+j+'"]'),k=sk.querySelector(".sk-knob");if(!s||!k)return;var a=sk.getBoundingClientRect(),r=s.getBoundingClientRect(),q=a.width/(sk.offsetWidth||1);if(!a.width)return;k.style.left=((r.left-a.left+sh*r.width)/q).toFixed(1)+"px"}
setInterval(dkTick,250);
/* ----- the voice moving: the bars (and Ink glass's ring of light) move only while the voice speaks, from its level (guide/voice.js's
   voiceLevel); still when paused. Unity: the bars are VisualElements whose style.scale follows the level each frame ----- */
var BARS={raf:0,cur:[],t0:0};
function barsLoop(){BARS.raf=0;var bs=ph.querySelectorAll(".gdock .gd-bars i");if(!bs.length||!G.on)return;var lv=voiceLevel(),t=performance.now()/1000;G.barsLvl=lv;
  for(var i=0;i<bs.length;i++){var shape=.45+.55*Math.abs(Math.sin(t*(5.3+i*1.7)+i*1.9)*Math.cos(t*(2.1+i*.6)+i)),want=.18+.82*Math.min(1,lv*shape*1.3),c=BARS.cur[i]==null?.18:BARS.cur[i];c+=(want-c)*(lv>0?.5:.35);if(lv<=0&&c<.19)c=.18;BARS.cur[i]=c;bs[i].style.transform="scaleY("+c.toFixed(3)+")"}
  BARS.raf=requestAnimationFrame(barsLoop)}
function barsStart(){if(!BARS.raf)BARS.raf=requestAnimationFrame(barsLoop)}

/* the dock replaces the captions box and the control bar; the old ones stay as "Classic". Compact: no dock (the voice goes on; the
   guide's button shows the progress and a pause); Azu's own words in its bubble (Compact, and at Mini in the Looks without a line of
   words: Ink glass, Player sheet), a block's words lit in the block; at Open the words are the panel's teleprompter */
on("renderGuide.after",function(text){var old=ph.querySelector(".gdock"),oldR=old&&old.getClientRects().length?phRect(old):null,oldH=old&&old.dataset.h,oldL=old&&old.dataset.look;
  ["gdock","gbubble"].forEach(function(c){var o=ph.querySelector("."+c);if(o)o.remove()});if(!G.on||!dockOn()){if(oldR)dkMorph(oldR,null,oldL);return}
  var cap=ph.querySelector(".gcap"),bar=ph.querySelector(".gbar"),ask=ph.querySelector(".gask");if(bar)bar.remove();if(ask)ask.remove();
  var st=S.guideSteps[G.i],t=text!=null?text:stepText(st),own=cap&&!blockWords(t);G.tpText=t;if(G.askOpen&&S.dockH!=="full")S.dockH="full";if(S.dockH!=="peek")G.typing=false;
  var ts=talkState(),compact=S.dockH==="compact",L=dkLook(),line=L==="paper"||L==="prompter",d=null;
  if(!compact){d=document.createElement("div");d.className="gdock h-"+S.dockH+" lk-"+L;d.dataset.h=S.dockH;d.dataset.look=L;d.setAttribute("role","region");d.setAttribute("aria-label",TXT("dock_label"));d.innerHTML=dockHtml(S.dockH);ph.appendChild(d)}
  /* the words: Azu's own in its bubble (Compact; Mini of a Look with no line), else in the panel; a block's words are lit in the block */
  if(cap){if(own&&(compact||S.guideCfg.bubbles==="own"&&(S.dockH==="mini"&&(!line||!dkWordsOn())||S.dockH==="peek"&&dkWordsOff()))){var b=document.createElement("div");b.className="gbubble";b.setAttribute("role","status");cap.classList.add("gb-text");b.appendChild(cap);ph.appendChild(b);placeBubble()}
    else cap.remove()}
  var th=d&&d.querySelector(".gd-thread");if(th){var nw=th.querySelector(".gm-now");th.scrollTop=nw&&!G.askOpen?Math.max(0,nw.offsetTop-th.offsetTop-24):th.scrollHeight}var m=ph.querySelector(".mascot");if(m)m.dataset.state=ts;
  if(d){dkTick();barsStart();if(!oldR)dkMorph(G.morphFrom||dkStarAt(),d,L);else if(oldH!==S.dockH&&oldL===L)dkMorph(oldR,d,L);else if(G.morph&&performance.now()<G.morph.end)dkHold(d)}else if(oldR)dkMorph(oldR,null,oldL);G.morphFrom=null;
  if(d&&G.typing){var gi=d.querySelector("#gq");if(gi)gi.focus({preventScroll:true})}});
/* ----- the ✦ grows into the panel (Morph.dc.html): a tap on the ✦, or a tour starting, widens the ✦'s circle into the pill, then the
   panel (320 ms); the ✦ fades out and stays hidden while the panel shows (world/guide-button.js); closing shrinks the panel back into the
   ✦. A change of height glides the same way. Reduce Motion: a crossfade. Unity: the panel's width, height, border-radius and position
   animated (USS transitions, or experimental.animation with the named ease-out curve: no cubic-bezier in 6.3), its children's opacity
   from Azu down, the ✦'s opacity ----- */
function phRect(el){var pr=ph.getBoundingClientRect(),q=pr.height/(ph.offsetHeight||1),r=el.getBoundingClientRect();return{l:(r.left-pr.left)/q,t:(r.top-pr.top)/q,w:r.width/q,h:r.height/q}}
/* where the ✦ is (or would be): the tour's ✦ at Compact, the card's, else its corner */
function dkStarAt(){var b=ph.querySelector(".gbtn-wrap .gbtn");if(b&&b.getClientRects().length)return phRect(b);return{l:S.guideCfg.gbSide==="right"?ph.offsetWidth-68:12,t:ph.offsetHeight-78,w:56,h:56}}
var MORPH_MS=320;
function dkHold(d){d.classList.add("gd-in");if(G.morph.end-performance.now()>200){d.classList.add("gd-in0");setTimeout(function(){d.classList.remove("gd-in0")},Math.max(0,G.morph.end-performance.now()-200))}setTimeout(function(){d.classList.remove("gd-in","gd-in0")},Math.max(0,G.morph.end-performance.now())+30)}
/* a morph's end: the panel on the screen now shows its own surface (a redraw during the morph made a new one) */
function dkMorphEnd(my){if(G.morph===my)G.morph=null;var c=ph.querySelector(".gdock");if(c)c.classList.remove("gd-in","gd-in0")}
function dkMorph(from,d,L){var old=ph.querySelector(".gd-morph");if(old)old.remove();var to=d?phRect(d):null;
  if(S.reduce){if(d&&d.animate)d.animate([{opacity:0},{opacity:1}],{duration:200,easing:"ease-out"});if(!d)dkStarIn(200);return}
  var s=document.createElement("div");s.className="gd-morph lk-"+L+(d?" grow":" shrink");s.setAttribute("aria-hidden","true");s.innerHTML='<span class="gd-mstar">'+GBSTAR+'</span>';
  var cs=d&&getComputedStyle(d),surf=d?(L==="prompter"?getComputedStyle(ph).getPropertyValue("--paper"):cs.backgroundColor):null,rad=d?(L==="prompter"?"28px":cs.borderRadius):null,star=function(r){return{left:r.l+"px",top:r.t+"px",width:r.w+"px",height:r.h+"px",borderRadius:Math.min(r.w,r.h)/2+"px"}};
  var go=function(a,b,ab,bb,ar,br){Object.assign(s.style,a,{background:ab,borderRadius:ar||a.borderRadius});ph.appendChild(s);s.style.transitionDuration=MORPH_MS+"ms";void s.offsetWidth;s.classList.add("go");Object.assign(s.style,b,{background:bb,borderRadius:br||b.borderRadius})};
  var end=performance.now()+MORPH_MS,my=G.morph={end:end};
  if(d){d.classList.add("gd-in","gd-in0");setTimeout(function(){d.classList.remove("gd-in0")},from.w>80?60:120);var A=star(from),B={left:to.l+"px",top:to.t+"px",width:to.w+"px",height:to.h+"px"};if(from.w>80){A.borderRadius=rad}
    go(A,B,from.w>80?surf:"#16181C",surf,null,rad);setTimeout(function(){dkMorphEnd(my);s.remove()},MORPH_MS+20);return}
  /* closing: into the ✦ (once it is drawn: the tour's at Compact, the card's after the tour) */
  Object.assign(s.style,{left:from.l+"px",top:from.t+"px",width:from.w+"px",height:from.h+"px",borderRadius:"28px",background:"rgba(22,24,28,.9)"});ph.appendChild(s);
  requestAnimationFrame(function(){var tgt=dkStarAt();go({left:from.l+"px",top:from.t+"px",width:from.w+"px",height:from.h+"px",borderRadius:"28px"},star(tgt),"rgba(22,24,28,.9)","#16181C");s.classList.add("into");
    var w=ph.querySelector(".gbtn-wrap");if(w)w.classList.add("gd-arriving");setTimeout(function(){s.remove();var w2=ph.querySelector(".gbtn-wrap.gd-arriving");if(w2)w2.classList.remove("gd-arriving");dkMorphEnd(my)},MORPH_MS+20)})}
function dkStarIn(ms){var w=ph.querySelector(".gbtn-wrap");if(w&&w.animate)w.animate([{opacity:0},{opacity:1}],{duration:ms,easing:"ease-out"})}
/* the dock's markup reaches the phone's signature: its Look, its height at the start and its Open, its controls at Mini, its parts */
on("modelSig.sig",function(h){if(!sigWant(/guideCfg\.(dockStart|miniCtl|chips|panel|look|pWords|pBars|pSpeed)/))return h;var keep=S.dockH;S.dockH=S.guideCfg.dockStart;var x="#dock:"+S.guideCfg.panel+"|"+S.dockH+"|"+(S.dockH==="compact"?"":dockHtml(S.dockH))+"|"+dockHtml("peek");S.dockH=keep;return h+x});
/* the bubble sits just above the dock, its tail towards Azu */
function placeBubble(){var b=ph.querySelector(".gbubble"),d=ph.querySelector(".gdock")||ph.querySelector(".gbtn-wrap.intour"),m=ph.querySelector(".mascot");if(!b)return;var hd=d&&d.querySelector(".gd-hide"),top=d?Math.min(phRect(d.querySelector(".gd-mrow")||d).t,hd?phRect(hd).t:1e9):ph.offsetHeight-16;
  b.style.bottom=(ph.offsetHeight-top+10)+"px";var up=m&&m.offsetTop+30<top-b.offsetHeight-10;b.classList.toggle("tail-up",!!up)}
/* the free height for the block being told: above the dock and the bubble */
function freeHeight(){var box=scr.getBoundingClientRect(),lo=box.bottom;[".gcap",".gbar",".gdock",".gbubble",".gbtn-wrap.intour"].forEach(function(s){var e=ph.querySelector(s);if(e){var r=e.getBoundingClientRect();if(r.height)lo=Math.min(lo,r.top)}});return{top:box.top,bottom:lo}}
/* keep the talk state and the bubble's place in step with the voice */
/* a bubble shows one sentence at a time (2 lines at most), the sentence being said */
function bubbleSentence(){var b=ph.querySelector(".gbubble .gcap");if(!b)return;var ws=b.querySelectorAll(".gw"),k=Math.max(0,G.hl&&G.hl.k>=0?G.hl.k:0),sid=[],n=0;
  for(var i=0;i<ws.length;i++){sid.push(n);if(/[.!?…]["”»)]*$/.test(ws[i].textContent))n++}var cur=sid[Math.min(k,ws.length-1)];for(var j=0;j<ws.length;j++)ws[j].classList.toggle("gb-off",sid[j]!==cur)}
setInterval(function(){if(!G.on||!dockOn())return;bubbleSentence();var t=ph.querySelector(".gd-talk"),ts=talkState(),m=ph.querySelector(".mascot");if(m&&m.dataset.state!==ts)m.dataset.state=ts;/* Azu's state follows at every height (round 11: the Mini has no talk button) */if(t&&!t.classList.contains("s-"+ts)){t.className="gd-talk s-"+ts;var h=ph.querySelector(".gd-hint");if(h){h.textContent=TXT(TALKW[ts]);h.classList.toggle("on",/listening|thinking|interrupted|notheard/.test(ts))}}
  var stt=ph.querySelector(".gd-status");if(stt){var w=dkStatus(ts);if(stt.innerHTML!==w)stt.innerHTML=w}if(ph.querySelector(".gd-bars"))barsStart();placeBubble()},150);
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
/* the dock's own taps: its height, play / pause on the line, typing, Read all, a suggested question (a step's segment is the progress
   bar's: guide/controls.js seeks) */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-dock],[data-chip]");if(!t||!ph.querySelector(".gdock"))return;e.stopPropagation();
  if(t.dataset.chip!=null){ask(L(S.guideAsks[+t.dataset.chip]));return}
  var k=t.dataset.dock;if(k==="hide"){S.dockH="compact";G.askOpen=false;renderGuide();return}if(k==="cycle"){S.dockH={mini:"peek",peek:"full",full:"mini"}[S.dockH];G.askOpen=S.dockH==="full";renderGuide();return}
  if(k==="mini"){S.dockH="mini";G.askOpen=false;renderGuide();return}
  if(k==="readall"){S.dockH=S.dockH==="full"?"peek":"full";G.askOpen=false;renderGuide();return}
  if(k==="playpause"){if(S.dockH==="full")return;if(G.paused)resumeTour();else{G.explicitPause=true;pauseTour();G.explicitPause=false}return}
  /* the keyboard: Open (Peek) with the typed question focused */
  if(k==="type"){S.dockH="peek";G.typing=true;G.askOpen=false;renderGuide();var i=ph.querySelector("#gq");if(i)i.focus({preventScroll:true})}},true);
/* a swipe on the grabber (the handle, the open chevron) or on the Mini: up raises the dock one height, down lowers it; down at Mini
   hides it (Compact) when Swipe down to Compact is on */
/* the swipe ends wherever the finger lifts (the Mini sits at the foot: a swipe down leaves the phone), so its end is heard on the document */
var grabY=null;ph.addEventListener("pointerdown",function(e){if(e.target.closest(".gd-grab,.gdock.h-mini .gd-mrow"))grabY=e.clientY},true);
document.addEventListener("pointerup",function(e){if(grabY==null)return;var dy=e.clientY-grabY;grabY=null;if(Math.abs(dy)<24)return;var o=["compact","mini","peek","full"],i=o.indexOf(S.dockH),lo=S.guideCfg.swipeCompact?0:1;
  S.dockH=o[Math.max(lo,Math.min(3,i+(dy<0?1:-1)))];G.askOpen=S.dockH==="full";G.swiped=performance.now();e.stopPropagation();renderGuide()},true);
/* a swipe is not a tap: the click that follows it does nothing */
ph.addEventListener("click",function(e){if(G.swiped&&performance.now()-G.swiped<400&&e.target.closest(".gdock")){G.swiped=0;e.stopPropagation();e.preventDefault()}},true);
ph.addEventListener("submit",function(e){var f=e.target.closest(".gd-form");if(!f)return;e.preventDefault();e.stopPropagation();var i=f.querySelector("#gq");if(i&&i.value.trim()){var q=i.value;i.value="";ask(q)}},true);

/* Editor: Guide tab > Guide panel (after Captions); Specific Marker > Guide script > Suggested questions */
var LOOKW={paper:"Paper glass: the card's paper as a glass pill (Mini: Azu, the step, a line of words, Pause) and a glass panel (Open: Azu and its voice's bars, the words, the stops as segments, the controls with a terracotta Play, the ask field and the questions).",
  ink:"Ink glass (the Architect's pick): a dark capsule (Mini: Azu inside its progress ring, the voice's bars, a saffron Pause) and a dark glass panel (Open: Azu beside the words, the line said now large with its word in saffron, a line with a dot for each stop, the controls around a large saffron Play, the questions, the ask field and the microphone in its ring).",
  player:"Player sheet: a music player's mini bar docked at the foot (Mini: Azu, the step's name, the time, Pause, Next) and a white sheet (Open: the words in a caption card with Read all, a scrubber, the time and what is left, a large black Play, the questions, a dark ask bar with Voice).",
  prompter:"Teleprompter: no box; one line of words over the paper's fade and a small capsule (Mini); Open: the words scroll up large over the fade, a hairline with its knob, the questions and the keyboard, and a capsule of controls beside a terracotta microphone."};
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var ca=d.querySelector('[data-fo="s7cap"]');if(!ca)return d.innerHTML;
  ca.insertAdjacentHTML("beforebegin",comp("s7dock","one panel for the tour's words, its controls and the conversation",
    row("Panel",pseg("guideCfg.panel",["dock","classic"],["Dock","Classic"]),"Dock: one panel at the bottom in its Look, with its heights: Mini (the Look's pill, capsule, mini player or one line: the step, the voice, Pause and Stop), Peek (Open: the panel with the words, the progress, the controls, the questions and the ask field), Full (Open with the whole text: Read all) and Compact (hidden: the voice goes on, the guide's button shows the progress and a pause). The ✦ grows into it and it shrinks back into the ✦. The grabber or a swipe changes its height; the chevron hides it. Classic: the captions box over the control bar.")+
    row("Look",pseg("guideCfg.look",DLOOKS.map(function(x){return x[0]}),DLOOKS.map(function(x){return x[1]})),LOOKW[dkLook()]+" Each in light and dark, the same controls, the same heights. Unity: one UXML template per Look with the same element names, swapped with its USS; no backdrop blur (a translucent fill with a light edge), a gradient as a ramp texture.",true)+
    row("Words",pseg("guideCfg.pWords",["prompter","off"],["Teleprompter","Off"]),"Teleprompter: the line said now, its spoken word marked, the line before faded above it and the next below; they move up as the voice goes on (never fixed bubbles). The visitor's Aa shows or hides them, Read all shows the whole text. Off: no words in the panel (a block's words still light up in the block; Azu's own words go to its bubble).",true)+
    row("Voice moving",pseg("guideCfg.pBars",["true","false"],["On","Off"]),"On: bars (Ink glass's Mini: with Azu's ring) move only while the voice speaks: a recorded or live voice by its level (the Lab: an AnalyserNode where the page may read the audio, else the line's own mouth curve or its word times; Unity: AudioSource.GetOutputData), the device voice by its word events. Still when paused.",true)+
    row("Speed button",pseg("guideCfg.pSpeed",["true","false"],["Shown","Hidden"]),"A chip in the controls: 0.5× · 0.75× · 1× · 1.25× · 1.5× · 1.75× · 2×, the visitor's own (the voice follows at once); its first value is Narration › Speed.",true)+
    row("Starts at",pseg("guideCfg.dockStart",["mini","peek","compact"],["Mini","Peek","Compact"]),"Its height when the tour starts (the ✦ grows into it). Compact: the tour starts hidden; a tap on the guide's button brings the dock at Mini.",true)+
    row("Controls at Mini",pseg("guideCfg.miniCtl",["pausestop","pause","line"],["Pause + Stop","Pause only","Tap on the line"]),"Pause + Stop: two buttons (44 px) in the Mini, in the Look's style. Pause only: the same without Stop (Stop is in Open). Tap on the line: no buttons; a tap on the Mini's line (or the voice's bars) pauses or plays, and a keyboard button asks by typing.",true)+
    row("Swipe down to Compact",pseg("guideCfg.swipeCompact",["true","false"],["On","Off"]),"On: a swipe down on the Mini hides the dock (Compact); the voice goes on. Off: Mini is the lowest a swipe goes; the chevron and the guide's button still hide it.",true)+
    row("Azu's own words",pseg("guideCfg.bubbles",["own","panel"],["In a bubble","In the panel"]),"In a bubble: what Azu says in its own words (the welcome, an ask, an answer) appears in a bubble above the Mini, its tail towards Azu, in the Looks whose Mini has no line of words (Ink glass, Player sheet) and at Compact; Paper glass and Teleprompter show them in their Mini's line. In Open the words are the panel's. A block's words light up in the block itself. Every line also goes to the whole text (Full).")+
    row("Talk button",pseg("guideCfg.talk",["both","tap","hold"],["Tap or hold","Tap","Hold"]),"The microphone in Open, beside the ask field. Tap: listens until the visitor stops speaking; a second tap sends. Hold: listens while held, sends on release. While Azu speaks, the button interrupts it and listens. Its states show on it and in words under it (listening, thinking, go ahead, not heard). Never always-listening.")+
    row("Sounds",pseg("guideCfg.earcons",["true","false"],["On","Off"]),"A short rising tone when it starts listening and a falling one when it stops (under 300 ms). With a light haptic.")+
    row("Suggested questions",pseg("guideCfg.chips",["true","false"],["Shown","Hidden"]),"Chips in Open (the point's own, in the Guide script), ready to tap: a tap asks it.")+
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
rowDemos({"guideCfg.look":{tour:"g1",dock:"peek",ring:".gdock"},"guideCfg.pWords":{tour:"g1",dock:"peek",ring:".gd-tp|.gd-wrow|.gdock"},"guideCfg.pBars":{tour:"g1",dock:"mini",ring:".gd-bars|.gdock"},
  "guideCfg.pSpeed":{tour:"g1",dock:"peek",ring:".gd-speed|.gd-ctl"},"guideCfg.panel":{tour:"g1",ring:".gdock|.gbar"},"guideCfg.dockStart":{tour:"g1",ring:".gdock|.gbtn-wrap.intour"},"guideCfg.miniCtl":{tour:"g1",dock:"mini",ring:".gd-mini|.gd-line"},
  "guideCfg.swipeCompact":{tour:"g1",dock:"mini",act:function(){var g=ph.querySelector(".gd-grab");if(g)gsTouch(g,90)},ring:function(){return S.dockH==="compact"?".gbtn-wrap.intour":".gd-grab"}},
  "guideCfg.bubbles":{tour:true,ring:".gbubble|.gd-tp"},"guideCfg.talk":{tour:"g1",dock:"peek",ring:".gd-talk"},"guideCfg.earcons":{tour:"g1",dock:"peek",act:function(){earcon(true)},ring:".gd-talk"},"guideCfg.chips":{tour:"g1",dock:"peek",ring:".gd-chips"}});
