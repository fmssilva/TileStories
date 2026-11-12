/* The Guide's tour: its settings and steps (S.guideCfg, S.guideSteps), startTour, runStep (a step's line heard: tourStep.ended), next, the captions (renderGuide), the coach and the teaser; its length (tourMinutes). */
S.guideCfg={heroButton:true,block:true,voice:"recorded",music:false,captions:true,highlight:true,coach:true,mascot:true,ask:"ai",speed:1};
S.guideSteps=[
 {id:"g0",view:"card",ref:"hero",act:"teaser",text:T("Welcome to the Ribeira Palace. For two and a half centuries the kings of Portugal lived here, right on the river. Let me show you what was here.","Bem-vindo ao Paço da Ribeira. Durante dois séculos e meio, os reis de Portugal viveram aqui, mesmo à beira do rio. Deixe-me mostrar-lhe o que aqui existia.")},
 {id:"g1",view:"card",ref:"b:b_facts",act:"show",text:T("It was the royal home for about two hundred and fifty years, until it was lost on the first of November, 1755.","Foi casa real durante cerca de duzentos e cinquenta anos, até se perder a um de novembro de 1755.")},
 {id:"g2",view:"card",ref:"b:b_fun",act:"try",wait:"flip",text:T("Here is something surprising. Tap the card to flip it.","Aqui está algo surpreendente. Toque no cartão para o virar.")},
 {id:"g3",view:"story",ref:"r:r1",act:"open",text:T("Now the story. Two hundred and fifty years by the river.","Agora a história. Duzentos e cinquenta anos à beira-rio.")},
 {id:"g4",view:"story",ref:"r:r2",act:"read",text:null},
 {id:"g5",view:"story",ref:"r:r5",act:"show",text:T("Inside: a library of some seventy thousand books, the royal archive, a chapel, and next door a brand new opera house.","Lá dentro: uma biblioteca com cerca de setenta mil livros, o arquivo real, uma capela e, ao lado, uma ópera acabada de construir.")},
 {id:"g6",view:"story",ref:"r:r6",act:"try",wait:"drag",text:T("Drag across the picture: the palace before 1755, and the square that took its place.","Arraste sobre a imagem: o palácio antes de 1755 e a praça que ficou no seu lugar.")},
 {id:"g7",view:"story",ref:"r:r9",act:"read",text:null},
 {id:"g8",view:"card",ref:"b:b_gal",act:"show",text:T("Back on the card, the gallery shows the square through three centuries.","De volta ao cartão, a galeria mostra a praça ao longo de três séculos.")},
 {id:"g9",view:"card",ref:"b:b_quiz",act:"try",wait:"answer",text:T("Ready for a question? Which king moved the court to the river? Tap your answer.","Pronto para uma pergunta? Que rei mudou a corte para o rio? Toque na sua resposta.")},
 {id:"g10",view:"card",ref:"hero",act:"end",text:T("That was the Ribeira Palace. Ask me anything, or explore on your own.","Este foi o Paço da Ribeira. Pergunte-me o que quiser, ou explore sozinho.")}];
var G={on:false,i:0,paused:false,waiting:false,timer:null,utter:null,words:[],music:null,askOpen:false,answer:"",busy:false,ctl:null};
/* the tour's length in minutes, as the hero's button and the guidance's invitation (A tour of {point}) say it: a point's (id; the open
   one when none) as a module answers (guidance round 16: a tour made from the card, its words at the voice's speed), else the written
   tour's 3 */
function tourMinutes(id){var m=firstAnswer("tourMinutes.answer",id);return m!==undefined?m:3}
function stepText(st){if(st.text)return L(st.text);var r=byId(S.story,st.ref.slice(2));return r&&r.c?L(r.c.text):""}
/* ----- the music bed (round 11): a track that loops under the voice and ducks while Azu speaks. Music volume 0-100 % (15), Under the
   voice: how low it ducks, a share of its volume (30 %); Track: the manifest's approved tracks (../music/tracks.js: id, name, file,
   licence, author, source; the first the default), the Soft drone (built in: three sine tones), or Add your own… (a file, this session).
   The visitor's Settings › The guide › Music picks a feeling and a volume of their own (Visitor can choose the music). The Lab: a
   looping <audio> (a file from this computer: its volume is the gain, the page may not route it through Web Audio) or the drone's
   GainNode. Unity: an AudioSource with loop, an AudioMixer group and two snapshots (normal / ducked) blended while the voice speaks;
   Add your own is an AudioClip field in the POI Editor ----- */
S.guideCfg.musicVol=15;S.guideCfg.musicDuck=30;S.guideCfg.musicTrack="exploration";S.guideCfg.musicVis=true;
var MUSIC_FULL=1/3;
window.MUSICTRACKS=window.MUSICTRACKS||[];
(function(){["music/tracks.js","../music/tracks.js"].forEach(function(s){var e=document.createElement("script");e.src=s;e.onload=function(){try{renderEditor()}catch(x){}};e.onerror=function(){};document.head.appendChild(e)})})();
function musicTracks(){var seen={};return(window.MUSICTRACKS||[]).filter(function(t){if(!t||!t.id||seen[t.id])return false;seen[t.id]=1;return true})}
/* a track whose licence asks for it (CC BY-SA) carries its credit line {en, pt}: shown under its choice and in Settings' Music credits */
function musicCredit(id){var t=musicTrackById(id);return t&&t.credit?L(t.credit):""}
function musicCredits(){return musicTracks().filter(function(t){return t.credit}).map(function(t){return L(t.credit)})}
function musicTrackById(id){return musicTracks().filter(function(t){return t.id===id})[0]||null}
/* the track that plays now: the visitor's choice, else the Editor's (Music bed On: its Track; Off: none) */
function musicPick(){var v=S.vis&&S.vis.music;if(v!=null&&(S.guideCfg.musicVis!==false))return v;return S.guideCfg.music?S.guideCfg.musicTrack:"off"}
function musicVol(){var v=S.vis&&S.vis.musicVol!=null&&S.guideCfg.musicVis!==false?S.vis.musicVol:S.guideCfg.musicVol;return Math.max(0,Math.min(100,+v||0))/100}
/* what a pick plays, and why when it stands in for another (no silent fallback) */
function musicSource(id){if(id==="drone")return{kind:"drone",name:"Soft drone (built in)"};
  if(id==="own")return G.musicOwn?{kind:"file",url:G.musicOwn.url,name:G.musicOwn.name}:{kind:"drone",name:"Soft drone (built in)",why:"no file chosen yet for Add your own…"};
  var t=musicTrackById(id);return t?{kind:"file",url:t.url||t.file,name:L(t.name),t:t}:{kind:"drone",name:"Soft drone (built in)",why:"“"+id+"” is not in music/tracks.js on this page"}}
/* the level: the volume, ducked under the voice */
function musicTarget(){var g=G.music;if(!g)return 0;var v=musicVol()*(g.kind==="drone"?MUSIC_FULL:1);return g.ducked?v*(Math.max(0,Math.min(100,+S.guideCfg.musicDuck))/100):v}
function musicLevel(){var g=G.music;if(!g)return 0;return g.kind==="drone"?g.gain.gain.value:g.audio.volume}
function musicSet(fast){var g=G.music;if(!g)return;var v=musicTarget();if(g.kind==="drone"){g.gain.gain.setTargetAtTime(v,actx.currentTime,fast?0.02:0.3);return}
  clearInterval(g.tw);if(fast){g.audio.volume=Math.min(1,v);return}var a=g.audio;g.tw=setInterval(function(){var d=v-a.volume;if(Math.abs(d)<.004){a.volume=Math.min(1,v);clearInterval(g.tw);return}a.volume=Math.max(0,Math.min(1,a.volume+d*.25))},40)}
function musicOn(on,id){try{var want=on?(id||musicPick()):"off";if(G.music&&(!on||G.music.id!==want)){var g=G.music;G.music=null;clearInterval(g.tw);if(g.kind==="drone"){g.oscs.forEach(function(o){try{o.stop()}catch(e){}});g.gain.disconnect()}else{g.audio.pause();g.audio.src=""}}
  if(!on||want==="off"){G.musicNow=null;musicNowShow();return}if(G.music)return;var src=musicSource(want);
  if(src.kind==="drone"){actx=actx||new (window.AudioContext||window.webkitAudioContext)();if(actx.state==="suspended")actx.resume();var gn=actx.createGain();gn.gain.value=0.0001;gn.connect(actx.destination);var oscs=[];
    [196,246.9,293.7].forEach(function(f,i){var o=actx.createOscillator();o.type="sine";o.frequency.value=f;var l=actx.createOscillator(),lg=actx.createGain();l.frequency.value=.07+i*.03;lg.gain.value=1.2;l.connect(lg);lg.connect(o.frequency);o.connect(gn);o.start();l.start();oscs.push(o,l)});
    G.music={id:want,kind:"drone",gain:gn,oscs:oscs,ducked:false}}
  else{var a=new Audio(src.url);a.loop=true;a.preload="auto";a.volume=0;G.music={id:want,kind:"file",audio:a,ducked:false};var p=a.play();if(p&&p.catch)p.catch(function(){})}
  G.musicNow=src;musicNowShow();musicSet(false)}catch(e){}}
function musicDuck(d){if(!G.music||G.music.ducked===!!d)return;G.music.ducked=!!d;musicSet(false)}
/* a change in the Editor or Settings: the track swapped, the level set */
function musicApply(){var want=musicPick();if(G.musicDemo&&want==="off")want=S.guideCfg.musicTrack;if((!G.on&&!G.musicDemo)||want==="off"){musicOn(false);return}
  if(G.music&&G.music.id!==want){var d=G.music.ducked;musicOn(false);musicOn(true,want);if(G.music){G.music.ducked=d;musicSet(true)}}else if(!G.music)musicOn(true,want);else musicSet(false)}
/* a music row's demo: the music plays a few seconds at that setting, ducking under a line of Azu's (the tour's), then stops */
function musicDemo(){clearTimeout(G.musicDemoT);G.musicDemo=true;musicApply();G.musicDemoT=setTimeout(function(){G.musicDemo=false;if(!(G.on&&musicPick()!=="off"))musicOn(false)},7000)}
/* Playing now: the Editor says what plays, and why when it stands in */
function musicNowText(){var n=G.musicNow;if(!n)return"Nothing at the moment.";return n.name+(n.t?" · "+n.t.licence+", "+n.t.author:"")+(n.why?" (the Soft drone plays: "+n.why+")":"")}
function musicNowShow(){var e=ed.querySelector(".musicnow");if(e)e.textContent=musicNowText()}
/* Add your own…: a file from this computer, for this session */
function musicPickFile(){var i=document.createElement("input");i.type="file";i.accept="audio/*";i.onchange=function(){var f=i.files&&i.files[0];if(!f)return;if(G.musicOwn)URL.revokeObjectURL(G.musicOwn.url);G.musicOwn={name:f.name,url:URL.createObjectURL(f)};S.guideCfg.musicTrack="own";
    if(G.music&&G.music.id==="own")musicOn(false);musicApply();renderEditor()};i.click()}
ed.addEventListener("change",function(e){var t=e.target;if(t.dataset&&t.dataset.sel==="@guideCfg.musicTrack"&&t.value==="own"&&!noDraw&&!S._stKey)setTimeout(musicPickFile,0)},false);

/* ----- the tour ----- */
/* startTour(from): from the start, or from a step kept by a stop (from = {id: the step}; the features before it know which) */
function startTour(from){emitLatestFirst("startTour.before",from);G.on=true;G.i=0;G.paused=false;S.stop="full";if(from){var j=S.guideSteps.findIndex(function(s){return s.id===from.id});G.i=Math.max(0,j)}if(musicPick()!=="off")musicOn(true);feedback("open");runStep();emit("startTour.after",from)}
function stopTour(){emitLatestFirst("stopTour.before");G.on=false;G.waiting=false;stopSpeech();musicOn(false);clearGuideMarks();renderGuide();emit("stopTour.after")}
function runStep(){emitLatestFirst("runStep.before");var st=G.on&&S.guideSteps[G.i];if(G.on&&!st)stopTour();else if(st){G.waiting=false;
  if(S.view!==st.view){S.view=st.view;renderPhone(false)}
  clearGuideMarks();var el=st.ref==="hero"?scr.querySelector(".hero"):scr.querySelector('[data-ref="'+st.ref+'"]');
  if(el){el.classList.add("gfocus");var y=el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-80;scr.scrollTo({top:Math.max(0,st.ref==="hero"?0:y),behavior:S.reduce?"auto":"smooth"})}
  if(st.act==="teaser")teaser(true);
  var text=stepText(st),target=st.act==="read"&&S.guideCfg.highlight&&el?el.querySelector(".el-par,.el-lede"):null;if(target)wrapWords(target);
  renderGuide(text);moveMascot(el);
  if(st.act==="try"&&S.guideCfg.coach&&el)coach(el,st.wait);
  say(text,function(k){markWord(k,target)},function(){if(!G.on||G.paused)return;emit("tourStep.ended",st);
    if(st.act==="try"&&S.guideCfg.coach){G.waiting=true;renderGuide(text);G.timer=setTimeout(function(){if(G.waiting){G.waiting=false;next()}},9000);return}
    if(st.act==="end"){G.askOpen=S.guideCfg.ask!=="off";renderGuide(text);return}
    setTimeout(function(){if(G.on&&!G.paused)next()},700)})}emit("runStep.after")}
/* next: a feature may answer for it (next.answer) and go on later with nextStep */
function next(){if(firstAnswer("next.answer")===undefined)nextStep()}
function nextStep(){teaser(false);if(G.i<S.guideSteps.length-1){G.i++;runStep()}else stopTour()}
function prev(){teaser(false);if(G.i>0){G.i--;runStep()}}
function wrapWords(el){if(el.dataset.wrapped)return;var t=el.textContent,re=/\S+/g,m,h="",last=0;while((m=re.exec(t))){h+=esc(t.slice(last,m.index))+'<span class="gw">'+esc(m[0])+'</span>';last=m.index+m[0].length}el.innerHTML=h+esc(t.slice(last));el.dataset.wrapped="1"}
function clearGuideMarks(){scr.querySelectorAll(".gfocus").forEach(function(x){x.classList.remove("gfocus")});var c=ph.querySelector(".gcoach");if(c)c.remove();emit("clearGuideMarks.after")}
function coach(el,kind){var c=document.createElement("div");c.className="gcoach";c.innerHTML=({flip:"&#9758; "+TXH("guide_coach_flip"),drag:"&#8596; "+TXH("guide_coach_drag"),answer:"&#9758; "+TXH("guide_coach_answer")}[kind]||TXH("guide_coach_try"));
  var r=el.getBoundingClientRect(),p=ph.getBoundingClientRect(),k=p.width/ph.offsetWidth;c.style.top=((r.top-p.top)/k+30)+"px";ph.appendChild(c)}
function teaser(on){var h=scr.querySelector(".hero");if(!h)return;h.classList.toggle("teasing",on&&!S.reduce);
  if(on&&!S.reduce){var imgs=[IMG.hero,IMG.g1,IMG.g3,IMG.g2],i=0;clearInterval(G.tz);G.tz=setInterval(function(){if(!G.on||G.paused)return;i=(i+1)%imgs.length;h.style.backgroundImage="url("+imgs[i]+")"},2600)}else{clearInterval(G.tz);h.style.backgroundImage="url("+IMG.hero+")"}}
function renderGuide(text){["gbar","gcap","mascot","gask","gresume"].forEach(function(c){var o=ph.querySelector("."+c);if(o)o.remove()});var pt=document.getElementById("ptool");if(pt)pt.style.display=G.on?"none":"";
  if(!G.on){var nb0=document.getElementById("navbar");if(nb0&&S.view==="card")nb0.style.display=S.ctl.bar.on&&S.stop!=="peek"&&S.stop!=="closed"?"":"none"}else{
  var nb=document.getElementById("navbar");if(nb)nb.style.display="none";var lp=ph.querySelector(".locpin");if(lp)lp.remove();
  var st=S.guideSteps[G.i],t=text!=null?text:stepText(st);
  if(S.guideCfg.captions){var cap=document.createElement("div");cap.className="gcap";cap.innerHTML=t.split(/(\s+)/).map(function(w){return/^\s+$/.test(w)?w:'<span class="gw">'+esc(w)+'</span>'}).join("")+(G.waiting?'<em>'+(TXH("guide_your_turn"))+'</em>':"");ph.appendChild(cap)}
  var bar=document.createElement("div");bar.className="gbar";
  bar.innerHTML='<div class="gseg">'+S.guideSteps.map(function(s,j){return'<i class="'+(j<G.i?"done":j===G.i?"now":"")+'"></i>'}).join("")+'</div><div class="gbtns">'+
   '<button type="button" data-g="prev" aria-label="'+TXH("guide_ctrl_prev")+'">&#9198;</button><button type="button" data-g="'+(G.paused?"play":"pause")+'" aria-label="'+(G.paused?TXH("guide_ctrl_play"):TXH("guide_ctrl_pause"))+'">'+(G.paused?"&#9654;":"&#10074;&#10074;")+'</button><button type="button" data-g="next" aria-label="'+TXH("guide_ctrl_next")+'">&#9197;</button>'+
   '<button type="button" data-g="mute" aria-pressed="'+!!S.guideMute+'" aria-label="'+TXH("guide_ctrl_mute")+'">'+(S.guideMute?"&#128263;":"&#128266;")+'</button><button type="button" data-g="cc" aria-pressed="'+S.guideCfg.captions+'">'+TXH("guide_ctrl_captions")+'</button>'+
   (S.guideCfg.ask!=="off"?'<button type="button" data-g="ask" class="askb">'+(TXH("guide_ask_button"))+'</button>':"")+'<button type="button" data-g="stop" aria-label="'+TXH("guide_ctrl_stop")+'">&#10005;</button></div>';ph.appendChild(bar);
  if(S.guideCfg.mascot){var m=document.createElement("div");m.className="mascot"+(S.reduce?" still":"")+(G.utter||G.timer?" talk":"");m.innerHTML=MASCOT;m.style.top="120px";ph.appendChild(m);var el=st.ref==="hero"?scr.querySelector(".hero"):scr.querySelector('[data-ref="'+st.ref+'"]');moveMascot(el)}
  /* paused: a Resume pill (with the dock, its own Play at Mini and the guide button's at Compact offer it) */
  if(G.paused&&!G.askOpen&&!dockOn()){var rs=document.createElement("button");rs.type="button";rs.className="gresume";rs.dataset.g="play";rs.textContent=TXT("guide_resume");ph.appendChild(rs)}
  if(G.askOpen){var a=document.createElement("div");a.className="gask";var ai=S.guideCfg.ask==="ai";
    a.innerHTML='<div class="gaskh"><b>'+(TXH("guide_ask_title"))+'</b><span>'+(ai?TXH("guide_ask_mode_ai"):TXH("guide_ask_mode_search"))+'</span><button type="button" data-g="askclose" aria-label="'+TXH("guide_ask_close")+'">&#10005;</button></div>'+
     '<div class="gans" aria-live="polite">'+(G.answer?esc(G.answer):'<span class="hintq">'+(TXH("guide_ask_example"))+'</span>')+'</div>'+
     '<form class="gform"><input id="gq" class="gin" type="text" autocomplete="off" placeholder="'+(TXH("guide_ask_placeholder"))+'"><button type="button" data-g="mic" aria-label="'+TXH("guide_ask_mic")+'">&#127908;</button><button type="submit" '+(G.busy?"disabled":"")+'>'+(G.busy?"…":(TXH("guide_ask_send")))+'</button>'+(G.busy?'<button type="button" data-g="stopask">'+TXH("guide_ask_stop")+'</button>':"")+'</form>';ph.appendChild(a)}}emit("renderGuide.after",text)}
/* ----- hooks into the card: hero button, Guide block, gestures, take-over ----- */
on("cardScreen.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  if(S.guideCfg.heroButton){var w=d.querySelector(".hero .w");if(w)w.insertAdjacentHTML("beforeend",'<button type="button" class="gstart" data-g="start">&#9654; '+TXH("guide_start")+' · '+esc(fmt(TXT("guide_duration"),tourMinutes()))+'</button>')}
  if(S.guideCfg.block){var ab=d.querySelector('[data-ref="sec:about"] .ey');if(ab)ab.insertAdjacentHTML("afterend",'<div class="item" data-ref="b:b_guide"><button type="button" class="gblock" data-g="start"><span class="gm">'+(S.guideCfg.mascot?mascotStill():sicon("play",48))+'</span><span><b>'+(TXH("guide_block_title"))+'</b><small>'+(TXH("guide_block_line"))+'</small></span></button></div>')}
  return d.innerHTML});
ph.addEventListener("click",function(e){var t=e.target,g=t.closest("[data-g]");
  if(t.closest("[data-flip]"))gesture("flip");if(t.closest(".kq button"))gesture("answer");
  if(!g)return;e.stopPropagation();var k=g.dataset.g;
  if(k==="start"){if(S.view!=="card"){S.view="card"}startTour()}else if(k==="pause")pauseTour();else if(k==="play"){G.askOpen=false;resumeTour()}else if(k==="next"){stopSpeech();G.paused=false;next()}else if(k==="prev"){if(firstAnswer("prevTap.answer")===undefined){stopSpeech();G.paused=false;prev()}}
  else if(k==="stop"){if(firstAnswer("stopTap.answer")===undefined)stopTour()}else if(k==="mute"){S.guideMute=!S.guideMute;if(firstAnswer("mute.voice")===undefined&&S.guideMute)stopSpeech();renderGuide()}else if(k==="cc"){S.guideCfg.captions=!S.guideCfg.captions;renderGuide()}
  else if(k==="ask"){pauseTour();G.askOpen=true;renderGuide();var i=ph.querySelector("#gq");if(i)i.focus()}else if(k==="askclose"){G.askOpen=false;renderGuide()}
  else if(k==="stopask"&&G.ctl)G.ctl.abort();
  else if(k==="mic"){var R=window.SpeechRecognition||window.webkitSpeechRecognition;if(!R){G.answer=TXT("guide_no_mic");renderGuide();return}
    var rec=new R();rec.lang=S.lang==="pt"?"pt-PT":"en-GB";rec.onresult=function(ev){var q=ev.results[0][0].transcript;var i2=ph.querySelector("#gq");if(i2)i2.value=q;ask(q)};try{rec.start();G.answer=TXT("guide_listening");var a=ph.querySelector(".gans");if(a)a.textContent=G.answer}catch(err){}}},true);
ph.addEventListener("pointerdown",function(e){if(e.target.closest("[data-ba]"))gesture("drag")},true);
["wheel","touchmove"].forEach(function(ev){scr.addEventListener(ev,function(){if(G.on&&!G.paused&&!G.waiting)pauseTour()},{passive:true})});
on("renderPhone.after",function(keep){if(G.on)renderGuide()});

/* ----- the Editor: Detail Card > 7 · Guide, Specific Marker > 3 · Guide script ----- */
on("edCard.html",function(h){var c=S.guideCfg;
  var gb=row("Start from the hero",pseg("guideCfg.heroButton",["true","false"],["Shown","Hidden"]),"A Start the guide button on the hero (replaces the Listen pill).")+
   row("Guide block",pseg("guideCfg.block",["true","false"],["Shown","Hidden"]),"A Guide block first in the first section.")+
   row("Voice",pseg("guideCfg.voice",["recorded","device"],["Recorded, then live, then this device","Device voice"]),"Recorded, then live, then this device: each line plays its audio rendered when authoring (or a human voice), with word timings; a line the recorded set lacks is spoken by the live voice (Ask Azu > Voice of the answers, when its key is on this computer), else by this device's voice. Speaking now says which, and why. Device voice: the phone reads the script (free, offline, plainer).")+
   row("Speed",pslider("guideCfg.speed",0.5,2,0.05),"The voice's speed, 0.5-2: the first value of the visitor's speed chip (the guide panel) and of Settings › The guide.")+row("Music bed",pseg("guideCfg.music",["true","false"],["On","Off"]),"Soft music that loops under the voice and ducks while Azu speaks. Off: none, unless the visitor picks one in Settings › The guide › Music.")+
   row("Music volume",pslider("guideCfg.musicVol",0,100,5),"0-100 % (15): the music's level against the voice (the visitor's own in Settings when they set it).",true)+
   row("Under the voice",pslider("guideCfg.musicDuck",0,100,5),"How low the music goes while Azu speaks, as a share of its volume (30 %); it comes back up between the lines. Unity: an AudioMixer snapshot.",true)+
   row("Track",psel("guideCfg.musicTrack",musicTracks().map(function(t){return[t.id,L(t.name)+" · "+t.licence+", "+t.author]}).concat([["drone","Soft drone (built in)"],["own",G.musicOwn?"Your file · "+G.musicOwn.name:"Add your own…"]])),
     (musicTracks().length?musicTracks().length+" approved tracks from music/tracks.js (each with its licence and source; the Architect adds them), then the Soft drone, then Add your own… (a file from this computer, for this session; Unity: an AudioClip in the POI Editor). The first track is the default.":"music/tracks.js is not on this page: only the Soft drone and Add your own… (the Exploration default plays the Soft drone, and Playing now says so)."),true)+
   (musicCredit(S.guideCfg.musicTrack)?row("Credit",'<span class="uses mcredit">'+esc(musicCredit(S.guideCfg.musicTrack))+'</span>',"This track's licence asks for this credit: the visitor sees it under the choice and in Settings › The guide › Music credits.",true):"")+
   row("Playing now",'<span class="uses musicnow">'+esc(musicNowText())+'</span>',"The music playing now and its licence, and why when the Soft drone stands in for a track this page does not have (no silent fallback).",true)+
   row("Visitor can choose the music",pseg("guideCfg.musicVis",["true","false"],["Shown","Hidden"]),"Shown: Settings › The guide › Music offers Off and each track's feeling (Exploration, Storyteller, Calm, Cinematic) and a volume; the Track above is the default. Hidden: the Editor's choice only.",true)+
   row("Captions",pseg("guideCfg.captions",["true","false"],["On","Off"]),"On by default (accessibility, noisy places).")+row("Highlight what is read",pseg("guideCfg.highlight",["true","false"],["On","Off"]),"The words light up in the text as they are spoken.")+
   row("Invite to try",pseg("guideCfg.coach",["true","false"],["On","Off"]),"At an interactive block the guide waits for the gesture (9 s at most), with one hint.")+
   row("Mascot",pseg("guideCfg.mascot",["true","false"],["Azu","Off"]),"A small tile character that follows the block being told. Still under Reduce Motion. Off: no Azu anywhere: the ✦ alone on the card and on the wall, and the guide's bubbles signed with a small ✦ in their corner.")+
   row("Ask the guide",pseg("guideCfg.ask",["off","search","ai"],["Off","Search (offline)","AI (cloud)"]),"Search: offline keyword search that jumps to a block. AI: answers only from this card's content and scrolls to the block that helps.")+
   '<div class="note fixed">Fixed rules: one visible pause that stops voice, scroll and mascot (WCAG 2.2.2); any drag by the visitor pauses the tour and offers Resume; captions available; Reduce Motion = no auto-scroll animation, a still mascot.</div>';
  var sec=fo("s7",'7 · Guide <span class="sub">· the narrated tour and Ask the guide</span>',["new","NEW"],gb,1);
  var i=h.lastIndexOf('<div class="json">');return h.slice(0,i)+sec+h.slice(i)});
on("edMarker.html",function(h){var t='<div class="note">The tour of this point: one row per step, in order. Each step shows a block, says its words (EN / PT), and may wait for the visitor to try it.</div><table class="tbl"><thead><tr><th style="width:22px">#</th><th style="width:28%">Shows</th><th style="width:70px">Does</th><th>Says</th></tr></thead><tbody>';
  S.guideSteps.forEach(function(st,j){var c=corpus().filter(function(x){return x.ref===st.ref})[0];t+='<tr class="pick'+(S.sel.gstep===st.id?" sel":"")+(st.skip?" gskipped":"")+'" data-pick="gstep" data-id="'+st.id+'"><td class="uses">'+(j+1)+'</td><td>'+esc(st.ref==="hero"?"The hero (teaser)":c?c.title:st.at&&st.at.kind?"The "+st.at.kind+" (no longer on the card)":"A block no longer on the card")+'</td><td class="uses">'+st.act+(st.wait?" · "+st.wait:"")+(st.skip?" · skipped":"")+'</td><td class="uses">'+esc(stepText(st))+'</td></tr>';
    if(S.sel.gstep===st.id)t+='<tr class="slot"><td></td><td colspan="3">'+gstepCtl(st,j)+(st.text?langCodes().map(function(lg){return row("Says · "+lg.toUpperCase(),'<textarea class="u" rows="2" data-gtext="'+st.id+'|'+lg+'">'+esc(st.text[lg]||"")+'</textarea>',"")}).join(""):'<div class="note">Reads the block\'s own text.</div>')+'</td></tr>'});
  t+='</tbody></table>';var sec=fo("m3","3 · Guide script",["new","NEW"],t,1);var i=h.lastIndexOf('<div class="json">');return h.slice(0,i)+sec+h.slice(i)});
/* a written step's own controls (guidance round 16, the written tour edited step by step): what it does, kept or skipped, its place */
var GSTEP_ACTS=[["teaser","The teaser (on the hero)"],["show","Show the block and say its line"],["read","Read the block's own words"],["try","Wait for the visitor to try it"],["open","Open the block's page"],["end","The end (Ask me anything)"]];
function gstepCtl(st,j){return row("Does",'<select class="u" data-gact="'+st.id+'">'+GSTEP_ACTS.map(function(a){return'<option value="'+a[0]+'"'+(st.act===a[0]?" selected":"")+'>'+esc(a[1])+'</option>'}).join("")+'</select>',"What the guide does at this step. Read lights the block's own words as they are said, so its line must be the block's words.")+
  row("Keep",'<div class="seg"><button type="button" data-gkeep="'+st.id+'|keep" aria-pressed="'+!st.skip+'">Keep</button><button type="button" data-gkeep="'+st.id+'|skip" aria-pressed="'+!!st.skip+'">Skip</button></div>',"Skip: the tour leaves this step out (it stays here, to bring back).")+
  row("Order",'<div class="btnrow"><button class="del" type="button" data-gmv="'+st.id+'|-1"'+(j===0?" disabled":"")+'>^ Earlier</button><button class="del" type="button" data-gmv="'+st.id+'|1"'+(j===S.guideSteps.length-1?" disabled":"")+'>v Later</button></div>',"Moves the step one place in the tour.")}
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.gact)return;e.stopImmediatePropagation();var st=byId(S.guideSteps,t.dataset.gact);if(!st)return;st.act=t.value;if(st.act!=="try")delete st.wait;else st.wait=st.wait||"answer";renderEditor()},true);
ed.addEventListener("click",function(e){var k=e.target.closest("[data-gkeep],[data-gmv]");if(!k)return;e.stopPropagation();
  if(k.dataset.gkeep){var q=k.dataset.gkeep.split("|"),st=byId(S.guideSteps,q[0]);if(st){if(q[1]==="skip")st.skip=true;else delete st.skip}renderEditor();return}
  var m=k.dataset.gmv.split("|"),i=S.guideSteps.findIndex(function(s){return s.id===m[0]}),j=i+(+m[1]);if(i<0||j<0||j>=S.guideSteps.length)return;swap(S.guideSteps,i,j);renderEditor()},true);
/* a skipped step is left out when the tour starts (the same filter as an audience's length) */
on("stepFits.ok",function(ok,st){return ok&&!(st&&st.skip)});
ed.addEventListener("click",function(e){var p=e.target.closest('[data-pick="gstep"]');if(!p||e.target.closest("textarea"))return;e.stopPropagation();S.sel.gstep=p.dataset.id;renderEditor()},true);
ed.addEventListener("input",function(e){var t=e.target;if(!t.dataset.gtext)return;e.stopImmediatePropagation();var q=t.dataset.gtext.split("|"),st=byId(S.guideSteps,q[0]);st.text[q[1]]=t.value},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(guideCfg\\.(voice|speed|music|captions|highlight|coach|ask)|"));
/* each row's demo (editor/demo.js): the tour at a step, the part the row changes ringed */
rowDemos({"guideCfg.voice":{tour:"g1",ring:".gbubble|.gdock|.gcap"},"guideCfg.speed":{tour:"g1",act:function(){S.vis.speed=null;dkTick()},ring:".gd-speed|.gbubble|.gdock|.gcap"},"guideCfg.music":{tour:"g1",act:function(){musicDemo()},ring:".gdock|.gbar"},"guideCfg.musicVol":{tour:"g1",act:function(){musicDemo()},ring:".gdock|.gbar"},"guideCfg.musicDuck":{tour:"g1",act:function(){musicDemo()},ring:".gdock|.gbar"},
  "guideCfg.musicTrack":{tour:"g1",act:function(){musicDemo()},ring:".gdock|.gbar"},"guideCfg.musicVis":{act:function(){S.settingsOpen=true;renderPhone(true);var v=ph.querySelector(".vset"),g=v&&v.querySelector(".vmusic");if(v&&g)v.scrollTop=Math.max(0,g.offsetTop-60)},ring:".vmusic|.vguide"},
  "guideCfg.heroButton":{stop:"full",ring:".hero .gstart|.hero"},"guideCfg.block":{stop:"full",scroll:'[data-ref="b:b_guide"]',ring:".gblock|[data-ref=\"sec:about\"]"},
  "guideCfg.captions":{tour:"g1",ring:".gbubble|.gdock|.gbar"},"guideCfg.highlight":{tour:"g4",ring:".gfocus"},"guideCfg.coach":{tour:"g2",ring:".gcoach|.gfocus"},
  "guideCfg.mascot":{tour:"g1",ring:".mascot|.gfocus"},"guideCfg.ask":{tour:true,dock:"full",act:function(){ask(L(S.guideAsks[0]))},ringAfter:1400,ring:".gd-thread|.gask"}});
