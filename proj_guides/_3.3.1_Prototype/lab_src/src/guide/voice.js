/* The voice: one order of sources for every line (sayOrder): a recorded line (recSay), the live voice (say.live), then the device voice (sayDevice); the device voices per language; what speaks now and why (G.sayNow, Speaking now). */
/* ----- the order of sources, written once, for every line (the tour's, Azu's own words, the answers) -----
   Voice = Recorded, then live, then this device: the line's recorded file when the set has it; else the live voice (Ask Azu >
   Voice of the answers, when its key is on this computer); else this device's voice. An answer (G.answer): the live voice first,
   then a recorded line, then the device. Voice = Device voice: no recorded files (the answers still try the live voice first). */
function sayOrder(text){var rec=S.guideCfg.voice==="recorded";return!!text&&text===G.answer?(rec?["live","recorded"]:["live"]):(rec?["recorded","live"]:[])}
/* say asks each source in that order (the point say.<source>): it answers {speaks:who} when it takes the line, or {not:why,short:why}
   when it cannot; the device voice speaks the rest. sayFrom(src,…,why) goes on after src: a source that fails hands its line on, with why */
function say(text,onWord,onEnd){emitLatestFirst("say.before",text);G.sayLine={text:text,why:[]};sayFrom(null,text,onWord,onEnd);emit("say.after",text)}
function sayFrom(src,text,onWord,onEnd,why){var line=G.sayLine&&G.sayLine.text===text?G.sayLine:(G.sayLine={text:text,why:[]});if(why)line.why.push(why);
  var o=sayOrder(text),i=src?o.indexOf(src)+1:0;
  for(;i<o.length;i++){var r=firstAnswer("say."+o[i],text,onWord,onEnd);if(r&&r.speaks){sayNow(o[i],r.speaks,line);return}if(r&&r.not)line.why.push(r)}
  sayNow("device","",line);sayDevice(text,onWord,onEnd)}
/* the device voice: before it (latest first), a wait for the voices (sayDevice.wait gets go), started once it speaks, ended when done, after it */
function sayDevice(text,onWord,onEnd){emitLatestFirst("sayDevice.before",text,onWord,onEnd);var end=function(){emit("sayDevice.ended");onEnd()};
  var go=function(){stopSpeech();var words=[],re=/\S+/g,m;while((m=re.exec(text)))words.push({i:m.index,w:m[0]});G.words=words;
    var v=voiceFor(S.lang);musicDuck(true);
    if(v&&!S.guideMute){var u=new SpeechSynthesisUtterance(text);u.voice=v;u.lang=v.lang;u.rate=S.guideCfg.speed;
      u.onboundary=function(e){if(e.name&&e.name!=="word")return;var k=0;for(var j=0;j<words.length;j++)if(words[j].i<=e.charIndex)k=j;onWord(k)};
      u.onend=function(){musicDuck(false);if(G.utter===u)end()};G.utter=u;speechSynthesis.speak(u)}
    else{var k=0,per=60000/(170*S.guideCfg.speed);onWord(0);G.timer=setInterval(function(){if(G.paused)return;k++;if(k>=words.length){clearInterval(G.timer);G.timer=null;musicDuck(false);end();return}onWord(k)},per)}
    emit("sayDevice.started",text)};
  if(firstAnswer("sayDevice.wait",go)===undefined)go();emit("sayDevice.after",text)}
function stopSpeech(){emitLatestFirst("stopSpeech.before");if(window.speechSynthesis)speechSynthesis.cancel();G.utter=null;if(G.timer){clearInterval(G.timer);G.timer=null}}
S.guideCfg.voice_en="";S.guideCfg.voice_pt="";
/* the browser loads its voices lazily: the first getVoices() is empty, so the tour's first step had no voice (captions only) */
var VOICES=[];function loadVoices(){if(window.speechSynthesis)VOICES=speechSynthesis.getVoices()||[];return VOICES.length}
if(window.speechSynthesis){loadVoices();var voicesSeen=false;speechSynthesis.addEventListener("voiceschanged",function(){loadVoices();if(!voicesSeen&&VOICES.length){voicesSeen=true;try{renderEditor()}catch(e){}}})}
/* the voices for a language, best first: the exact accent (pt-PT, en-GB), then natural / neural voices, then the rest */
function voicesFor(lang){var want=lang==="pt"?["pt-PT","pt"]:["en-GB","en"],out=[];
  want.forEach(function(w){VOICES.forEach(function(v){if((v.lang||"").replace("_","-").indexOf(w)===0&&out.indexOf(v)<0)out.push(v)})});
  var rank=function(v){return((v.lang||"").replace("_","-").indexOf(want[0])===0?0:2)+(/natural|neural|online|premium|enhanced/i.test(v.name)?0:1)};
  return out.map(function(v,i){return[v,i]}).sort(function(a,b){return rank(a[0])-rank(b[0])||a[1]-b[1]}).map(function(x){return x[0]})}
function voiceFor(lang){var vs=voicesFor(lang),pick=S.guideCfg["voice_"+lang];for(var i=0;i<vs.length;i++)if(vs[i].name===pick)return vs[i];return vs[0]||null}
on("sayDevice.wait",function(go){if(window.speechSynthesis&&!S.guideMute&&!loadVoices()){var t0=Date.now();G.voiceWait=setInterval(function(){if(loadVoices()||Date.now()-t0>1500){clearInterval(G.voiceWait);G.voiceWait=null;go()}},50);return true}});
on("stopSpeech.before",function(){if(G.voiceWait){clearInterval(G.voiceWait);G.voiceWait=null}});
S.guideCfg.voiceSet="";S.vt={line:"",order:[],ratings:{},saved:false,results:null,note:""};
COMP.s7vtest=["Guide","Voice test"];FOCUS.s7vtest=["card",".gblock","full"];
/* the voice sets (voicebench/out: rendered with a provider's key; out-private: Edge, on this computer only, never published) */
window.VOICESETS=window.VOICESETS||[];
(function(){var srcs=["voicebench/out/voicesets.js","../voicebench/out/voicesets.js"];if(location.protocol==="file:")srcs.push("voicebench/out-private/voicesets.js","../voicebench/out-private/voicesets.js");
  srcs.forEach(function(s){var e=document.createElement("script");e.src=s;e.onload=function(){try{renderEditor()}catch(x){}};e.onerror=function(){};document.head.appendChild(e)})})();
/* a line is found by what it says: FNV-1a of "lang|text", the same key voicebench writes */
function fnvKey(s){var b=new TextEncoder().encode(s),h=0x811c9dc5;for(var i=0;i<b.length;i++){h^=b[i];h=Math.imul(h,0x01000193)>>>0}return("0000000"+h.toString(16)).slice(-8)}
function voiceSets(){return window.VOICESETS||[]}
/* Best available: a published set first (this computer's private sets only when no published one exists), then the most lines, so one voice tells the tour */
function currentSet(){var s=voiceSets(),id=S.guideCfg.voiceSet,pick=s.filter(function(x){return x.id===id})[0];if(pick)return pick;
  var n=function(x){return Object.keys(x.files||{}).length};return s.slice().sort(function(a,b){return(a.private?1:0)-(b.private?1:0)||n(b)-n(a)})[0]||null}
function recFile(text,set){set=set||currentSet();if(!set||!text)return null;var f=set.files[fnvKey(S.lang+"|"+String(text).trim())];return f?{set:set,file:f,url:set.url+f.src}:null}
/* a set's short name: its provider and voices ("Azure Neural · en-GB-SoniaNeural / pt-PT-RaquelNeural" is Azure Sonia + Raquel) */
function setName(set){var p=String(set.label||set.id).split(" · "),who=(p[0]||"").replace(/ Neural$/,""),vs=(p[1]||"").split(" / ").map(function(v){return v.replace(/\s*\(.*?\)\s*/g,"").replace(/^[a-z]{2}-[A-Z]{2}-/,"").replace(/Neural$/,"").trim()}).filter(Boolean);
  return vs.length?who+" "+vs.join(" + "):who}
/* ----- playing a recorded line, the source say.recorded: the audio's clock lights each word (its real start and end: the glide is exact) ----- */
function recSay(text,onWord,onEnd){var set=currentSet();if(!set)return{not:"no recorded voice set on this page",short:"no recorded set"};
  var r=recFile(text,set);if(!r)return{not:"this line is not recorded in "+setName(set),short:"not recorded"};
  stopSpeech();var words=[],re=/\S+/g,m;while((m=re.exec(text)))words.push({i:m.index,w:m[0]});G.words=words;G.recWords=r.file.words;
  G.hl={k:-1,at:0,p:0,cpm:0,ended:false,last:0,seen:true,start:performance.now(),target:G.hl&&G.hl.target};G.noVoice=false;
  var a=new Audio(r.url);a.preload="auto";a.muted=!!S.guideMute;a.playbackRate=S.guideCfg.speed||1;G.recAudio=a;G.utter={recorded:true};G.lastWordAt=performance.now();musicDuck(true);
  var fin=function(){if(G.recAudio!==a)return;G.recAudio=null;G.utter=null;G.recWords=null;musicDuck(false);if(G.hl)G.hl.ended=true;onEnd()};
  a.addEventListener("ended",fin);a.addEventListener("error",function(){if(G.recAudio!==a)return;G.recAudio=null;G.utter=null;G.recWords=null;sayFrom("recorded",text,onWord,onEnd,{not:"the recording did not play",short:"the recording did not play"})});
  var tick=function(){if(G.recAudio!==a)return;var t=a.currentTime,w=G.recWords,k=-1;for(var j=0;j<w.length;j++){if(w[j][0]<=t+.02)k=j;else break}
    if(k>=0&&k!==G.hl.k){onWord(k);G.lastWordAt=performance.now()}if(k>=0&&G.hl){G.hl.at=performance.now()-(t-w[k][0])*1000/(a.playbackRate||1)}if(!a.paused)G.lastWordAt=performance.now();requestAnimationFrame(tick)};
  var p=a.play();if(p&&p.catch)p.catch(function(){if(G.recAudio===a){a.muted=true;a.play().catch(function(){})}});requestAnimationFrame(tick);return{speaks:setName(set)}}
on("say.recorded",recSay);
/* the glide uses the word's real length */
on("wordMs.answer",function(k){if(G.recWords&&G.recWords[k])return Math.max(60,(G.recWords[k][1]-G.recWords[k][0])*1000/((G.recAudio&&G.recAudio.playbackRate)||1))});
on("stopSpeech.before",function(){if(G.recAudio){try{G.recAudio.pause()}catch(e){}G.recAudio=null;G.utter=null;G.recWords=null}});
/* pause, resume and mute act on whatever speaks: an audio (a recorded line, a live one) pauses and plays on, as the app's AudioSource
   does; muted, its words go on with its clock */
on("pauseNow.voice",function(){if(G.recAudio){G.recAudio.pause();return true}});
on("resumeNow.voice",function(){if(G.recAudio){G.recAudio.muted=!!S.guideMute;G.recAudio.play().catch(function(){});return true}});
on("mute.voice",function(){if(G.recAudio){G.recAudio.muted=!!S.guideMute;return true}});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(voiceSet|"));

/* ----- what speaks now, and why when one source stands in for another (no silent fallback) ----- */
function sayNow(src,who,line){G.sayNow={src:src,by:who,why:line.why.slice(),text:line.text,answer:!!line.text&&line.text===G.answer};sayNowShow()}
function sayNowText(){var n=G.sayNow;if(!n)return"Nothing at the moment.";var w=n.why,why=w.length>1?w.map(function(x){return x.short}).join("; "):w.length?w[0].not:"";
  if(n.src==="recorded")return"Recorded · "+n.by+(why?" ("+why+")":"");
  if(n.src==="live")return"Live · "+n.by+(why?" ("+why+")":n.answer?" (an answer: the live voice comes first)":"");
  return"This device's voice"+(why?" ("+why+")":" (Voice is Device voice)")+(G.noVoice?" · it has no voice for "+S.lang.toUpperCase()+" here: captions only":"")}
function sayNowShow(){var e=ed.querySelector(".saynow");if(e)e.textContent=sayNowText();emit("sayNow.after",G.sayNow)}
on("sayDevice.started",function(){sayNowShow()});
on("stopTour.after",function(){G.sayLine=null;G.sayNow=null;sayNowShow()});
/* Editor: Narration > Speaking now, under Voice */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7voice",row("Speaking now",'<span class="uses saynow">'+esc(sayNowText())+'</span>',"The voice of the line being said, as it starts: Recorded (the set's file), Live (Ask Azu > Voice of the answers, with its key on this computer) or this device's voice, and why when one stands in for another. The tour's lines and Azu's own words try them in that order; an answer tries the live voice first."),"Speed");
  return d.innerHTML});
