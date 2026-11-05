/* The voice: say asks its sources, the latest first (say.source), then the device voice (sayDevice) speaks; the device voices per language; the recorded lines (recSay). */
/* ----- speech: the device voice with word boundaries, or a timed fallback ----- */
/* say asks its sources (say.source, the latest first), each one true when it speaks the line; the device voice speaks the rest.
   sayFrom(src,…) asks only the sources older than src: a source that fails hands the line on */
function say(text,onWord,onEnd){sayFrom(null,text,onWord,onEnd);emit("say.after",text)}
function sayFrom(src,text,onWord,onEnd){if(answerBelow("say.source",src,text,onWord,onEnd)===undefined)sayDevice(text,onWord,onEnd)}
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
/* Best available: the set with the most lines rendered (a published one first when as full), so one voice tells the tour */
function currentSet(){var s=voiceSets(),id=S.guideCfg.voiceSet,pick=s.filter(function(x){return x.id===id})[0];if(pick)return pick;
  var n=function(x){return Object.keys(x.files||{}).length};return s.slice().sort(function(a,b){return n(b)-n(a)||(a.private?1:0)-(b.private?1:0)})[0]||null}
function recFile(text,set){set=set||currentSet();if(!set||!text)return null;var f=set.files[fnvKey(S.lang+"|"+String(text).trim())];return f?{set:set,file:f,url:set.url+f.src}:null}
/* ----- playing a recorded line, a source of say: the audio's clock lights each word (its real start and end: the glide is exact) ----- */
function recSay(text,onWord,onEnd){var r=S.guideCfg.voice==="recorded"?recFile(text):null;if(!r)return;
  stopSpeech();var words=[],re=/\S+/g,m;while((m=re.exec(text)))words.push({i:m.index,w:m[0]});G.words=words;G.recWords=r.file.words;
  G.hl={k:-1,at:0,p:0,cpm:0,ended:false,last:0,seen:true,start:performance.now(),target:G.hl&&G.hl.target};G.noVoice=false;
  var a=new Audio(r.url);a.preload="auto";a.muted=!!S.guideMute;a.playbackRate=S.guideCfg.speed||1;G.recAudio=a;G.utter={recorded:true};G.lastWordAt=performance.now();musicDuck(true);
  var fin=function(){if(G.recAudio!==a)return;G.recAudio=null;G.utter=null;G.recWords=null;musicDuck(false);if(G.hl)G.hl.ended=true;onEnd()};
  a.addEventListener("ended",fin);a.addEventListener("error",function(){if(G.recAudio!==a)return;G.recAudio=null;G.utter=null;G.recWords=null;sayFrom(recSay,text,onWord,onEnd)});
  var tick=function(){if(G.recAudio!==a)return;var t=a.currentTime,w=G.recWords,k=-1;for(var j=0;j<w.length;j++){if(w[j][0]<=t+.02)k=j;else break}
    if(k>=0&&k!==G.hl.k){onWord(k);G.lastWordAt=performance.now()}if(k>=0&&G.hl){G.hl.at=performance.now()-(t-w[k][0])*1000/(a.playbackRate||1)}if(!a.paused)G.lastWordAt=performance.now();requestAnimationFrame(tick)};
  var p=a.play();if(p&&p.catch)p.catch(function(){if(G.recAudio===a){a.muted=true;a.play().catch(function(){})}});requestAnimationFrame(tick);return true}
on("say.source",recSay);
/* the glide uses the word's real length */
on("wordMs.answer",function(k){if(G.recWords&&G.recWords[k])return Math.max(60,(G.recWords[k][1]-G.recWords[k][0])*1000/((G.recAudio&&G.recAudio.playbackRate)||1))});
on("stopSpeech.before",function(){if(G.recAudio){try{G.recAudio.pause()}catch(e){}G.recAudio=null;G.utter=null;G.recWords=null}});
/* pause and resume: the audio just pauses and plays on (the app's AudioSource does the same) */
on("pauseTour.answer",function(){if(!G.on)return true;if(G.recAudio){if(S.guideCfg.onScroll==="follow"&&G.userScrollAt&&performance.now()-G.userScrollAt<60&&!G.explicitPause)return true;G.paused=true;G.recAudio.pause();renderGuide();return true}});
on("resumeTour.answer",function(){if(!G.on)return true;if(G.recAudio&&G.paused){G.paused=false;G.askOpen=false;G.recAudio.muted=!!S.guideMute;G.recAudio.play().catch(function(){});renderGuide();return true}});
/* mute keeps the words going (the captions follow the audio's clock) */
document.addEventListener("click",function(e){if(!G.recAudio||!e.target.closest||!e.target.closest('#ph [data-g="mute"]'))return;e.stopImmediatePropagation();e.stopPropagation();S.guideMute=!S.guideMute;G.recAudio.muted=S.guideMute;renderGuide()},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(voiceSet|"));
