/* ===================== v9a: the voice of the live answers: the device voice, Azure (live) or ElevenLabs (live). The two live voices bring their own timing: Azure's visemes move Azu's mouth; ElevenLabs gives each letter's time, and the letters at their times move it. The audio plays as a recorded line does (captions, the word light, pause, mute). In the app, the server streams the same, a sentence at a time ===================== */
S.agentCfg.liveVoice="device";S.agentCfg.elevenModel="eleven_flash_v2_5";
var AZ_SDK="https://cdn.jsdelivr.net/npm/microsoft-cognitiveservices-speech-sdk@1.43.0/distrib/browser/microsoft.cognitiveservices.speech.sdk.bundle-min.js";
var LV={last:null,sdk:null};
/* the keys: only this computer's (keys.local.js, from .secrets/keys.env); the published Lab has none and speaks with the device voice */
function lvKeys(){var k=window.LABKEYS||{};return{azure:!!(k.azure&&k.azureRegion),eleven:!!(k.eleven&&(k.elevenPT||k.elevenEN))}}
function lvOn(){var v=S.agentCfg.liveVoice;return v!=="device"&&!!lvKeys()[v]}
/* Azure's 22 visemes (its viseme ids 0-21) as Azu's mouth shapes */
var AZ_VIS=["M","A","A","O","E","E","E","O","O","A","O","A","chew","O","chew","E","O","chew","chew","chew","chew","M"];
/* a letter's shape (ElevenLabs gives letters, not visemes) */
var LV_CH={a:"A",á:"A",à:"A",ã:"A",â:"A",e:"E",é:"E",ê:"E",i:"E",í:"E",y:"E",o:"O",ó:"O",ô:"O",õ:"O",u:"O",ú:"O",w:"O",m:"M",b:"M",p:"M",f:"chew",v:"chew"};
function lvWords(text){var out=[],re=/\S+/g,m;while((m=re.exec(text)))out.push({i:m.index,w:m[0]});return out}
function lvBuf(b64){var s=atob(b64),u=new Uint8Array(s.length);for(var i=0;i<s.length;i++)u[i]=s.charCodeAt(i);return u.buffer}
/* words with no time of their own take their neighbours' (a number the voice read out, a sign) */
function lvFill(ws,n){var out=[];for(var j=0;j<n;j++)out.push(ws[j]||null);for(j=0;j<n;j++)if(!out[j]){var p=j-1;while(p>=0&&!out[p])p--;var q=j+1;while(q<n&&!out[q])q++;var a=p>=0?out[p][1]:0,b=q<n&&out[q]?out[q][0]:a+.3;out[j]=[a,Math.max(a+.05,b)]}return out}

/* ----- ElevenLabs: /with-timestamps gives the audio and each character's start and end ----- */
function lvEleven(text){var k=window.LABKEYS,voice=S.lang==="pt"?(k.elevenPT||k.elevenEN):(k.elevenEN||k.elevenPT);
  return fetch("https://api.elevenlabs.io/v1/text-to-speech/"+voice+"/with-timestamps?output_format=mp3_44100_64",{method:"POST",headers:{"xi-api-key":k.eleven,"Content-Type":"application/json"},
      body:JSON.stringify({text:text,model_id:S.agentCfg.elevenModel,voice_settings:{stability:.45,similarity_boost:.8}})})
    .then(function(r){if(!r.ok)throw new Error("ElevenLabs answered "+r.status);return r.json()})
    .then(function(j){var a=j.alignment||j.normalized_alignment||{},ch=a.characters||[],st=a.character_start_times_seconds||[],en=a.character_end_times_seconds||[],ws=lvWords(text),times=[],vis=[],last="";
      if(ch.length===text.length){ws.forEach(function(w,n){times[n]=[st[w.i],en[w.i+w.w.length-1]]})}
      else{var n=-1,inW=false;ch.forEach(function(c,i){if(/\s/.test(c)){inW=false;return}if(!inW){n++;inW=true;times[n]=[st[i],en[i]]}else if(times[n])times[n][1]=en[i]})}
      ch.forEach(function(c,i){var s=LV_CH[String(c).toLowerCase()];if(/[.,!?;:]/.test(c)){vis.push({t:en[i],s:"M"});last="M";return}if(s&&s!==last){vis.push({t:st[i],s:s});last=s}});
      if(en.length)vis.push({t:en[en.length-1],s:"M"});
      return{audio:lvBuf(j.audio_base64),words:lvFill(times,ws.length),vis:vis,from:"the letters at their times"}})}

/* ----- Azure: the Speech SDK (loaded on demand) gives the audio, each word's boundary and the visemes ----- */
function lvSdk(){if(window.SpeechSDK)return Promise.resolve(window.SpeechSDK);if(LV.sdk)return LV.sdk;
  LV.sdk=new Promise(function(res,rej){var s=document.createElement("script");s.src=AZ_SDK;s.onload=function(){window.SpeechSDK?res(window.SpeechSDK):rej(new Error("the Azure Speech SDK did not load"))};
    s.onerror=function(){LV.sdk=null;rej(new Error("could not load the Azure Speech SDK"))};document.head.appendChild(s)});return LV.sdk}
function lvAzure(text){var k=window.LABKEYS;return lvSdk().then(function(SDK){return new Promise(function(res,rej){
    var cfg=SDK.SpeechConfig.fromSubscription(k.azure,k.azureRegion);cfg.speechSynthesisVoiceName=S.lang==="pt"?"pt-PT-RaquelNeural":"en-GB-SoniaNeural";
    cfg.speechSynthesisOutputFormat=SDK.SpeechSynthesisOutputFormat.Audio24Khz48KBitRateMonoMp3;
    var syn=new SDK.SpeechSynthesizer(cfg,null),vis=[],bounds=[];
    syn.visemeReceived=function(s,e){vis.push({t:e.audioOffset/1e7,s:AZ_VIS[e.visemeId]||"E"})};
    syn.wordBoundary=function(s,e){bounds.push({t:e.audioOffset/1e7,d:(e.duration||0)/1e7,o:e.textOffset,l:e.wordLength})};
    syn.speakTextAsync(text,function(r){try{syn.close()}catch(x){}if(r.reason!==SDK.ResultReason.SynthesizingAudioCompleted){rej(new Error("Azure: "+(r.errorDetails||"no audio")));return}
      var ws=lvWords(text),times=[];bounds.forEach(function(b){for(var n=0;n<ws.length;n++)if(b.o>=ws[n].i&&b.o<ws[n].i+ws[n].w.length){if(!times[n])times[n]=[b.t,b.t+Math.max(.05,b.d)];break}});
      res({audio:r.audioData,words:lvFill(times,ws.length),vis:vis.sort(function(a,b){return a.t-b.t}),from:"Azure's visemes"})},
      function(e){try{syn.close()}catch(x){}rej(new Error("Azure: "+e))})})})}

/* ----- the answer plays as a recorded line (v8r's controls act on G.recAudio), the mouth on the voice's own shapes ----- */
function lvPlay(text,onWord,onEnd,r){stopSpeech();G.words=lvWords(text);G.recWords=r.words;G.visTrack=r.vis;
  G.hl={k:-1,at:0,p:0,cpm:0,ended:false,last:0,seen:true,start:performance.now(),target:G.hl&&G.hl.target};G.noVoice=false;
  var url=URL.createObjectURL(new Blob([r.audio],{type:"audio/mpeg"})),a=new Audio(url),vi=-1;a.muted=!!S.guideMute;a.playbackRate=S.guideCfg.speed||1;
  G.recAudio=a;G.utter={recorded:true,live:true};G.lastWordAt=performance.now();musicDuck(true);
  var rest=function(){var m=ph.querySelector(".mascot");if(m)azMouth(m,restShape(azMood()))};
  var fin=function(){if(G.recAudio!==a)return;G.recAudio=null;G.utter=null;G.recWords=null;G.visTrack=null;musicDuck(false);if(G.hl)G.hl.ended=true;URL.revokeObjectURL(url);rest();onEnd()};
  a.addEventListener("ended",fin);a.addEventListener("error",function(){if(G.recAudio!==a)return;G.recAudio=null;G.utter=null;G.recWords=null;G.visTrack=null;LV.last={voice:S.agentCfg.liveVoice,err:"the audio did not play"};rest();lvDevice(text,onWord,onEnd)});
  var tick=function(){if(G.recAudio!==a)return;var t=a.currentTime,w=G.recWords,k=-1;for(var j=0;j<w.length;j++){if(w[j][0]<=t+.02)k=j;else break}
    if(k>=0&&k!==G.hl.k){onWord(k);G.lastWordAt=performance.now()}if(k>=0&&G.hl)G.hl.at=performance.now()-(t-w[k][0])*1000/(a.playbackRate||1);if(!a.paused)G.lastWordAt=performance.now();
    var v=G.visTrack||[],n=vi;while(n+1<v.length&&v[n+1].t<=t)n++;if(n!==vi){vi=n;var m=ph.querySelector(".mascot");if(m&&S.guideCfg.lips&&n>=0)azMouth(m,v[n].s)}
    requestAnimationFrame(tick)};
  var p=a.play();if(p&&p.catch)p.catch(function(){if(G.recAudio===a){a.muted=true;a.play().catch(function(){})}});requestAnimationFrame(tick)}
/* while the voice's own shapes move the mouth, the letters-per-word mouth stands down (the word light goes on) */
var markWordV9a=markWord;markWord=function(k,target){if(!G.visTrack){markWordV9a(k,target);return}var l=S.guideCfg.lips;S.guideCfg.lips=false;try{markWordV9a(k,target)}finally{S.guideCfg.lips=l}};
var stopSpeechV9a=stopSpeech;stopSpeech=function(){G.lvSeq=(G.lvSeq||0)+1;G.visTrack=null;stopSpeechV9a()};
var sayV9a=say,lvDevice=function(t,w,e){sayV9a(t,w,e)};
/* only the live agent's answers take the live voice; the tour's lines keep theirs (recorded or the device's) */
say=function(text,onWord,onEnd){if(!lvOn()||!text||text!==G.answer){sayV9a(text,onWord,onEnd);return}
  var v=S.agentCfg.liveVoice,t0=performance.now(),my=G.lvSeq=(G.lvSeq||0)+1;
  (v==="azure"?lvAzure(text):lvEleven(text)).then(function(r){if(G.lvSeq!==my)return;LV.last={voice:v,ms:Math.round(performance.now()-t0),shapes:r.shapes||r.vis.length,words:r.words.length,from:r.from};lvPlay(text,onWord,onEnd,r);lvStatus()})
    .catch(function(e){if(G.lvSeq!==my)return;LV.last={voice:v,err:String(e&&e.message||e)};console.warn("live voice",e);lvStatus();sayV9a(text,onWord,onEnd)})};
/* the wall's answers (Ask about the view) too: the audio, no captions */
var camSayV9a=camSay;camSay=function(t){if(!lvOn()){camSayV9a(t);return}G.cam.busy=false;G.cam.answer=t;camLayer();var v=S.agentCfg.liveVoice,t0=performance.now();
  (v==="azure"?lvAzure(t):lvEleven(t)).then(function(r){LV.last={voice:v,ms:Math.round(performance.now()-t0),shapes:r.shapes||r.vis.length,words:r.words.length,from:r.from};lvStatus();if(S.guideMute)return;
    if(LV.camAudio)try{LV.camAudio.pause()}catch(x){}LV.camAudio=new Audio(URL.createObjectURL(new Blob([r.audio],{type:"audio/mpeg"})));LV.camAudio.play().catch(function(){})})
    .catch(function(e){LV.last={voice:v,err:String(e&&e.message||e)};lvStatus();camSayV9a(t)})};
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(agentCfg\\.(liveVoice|elevenModel)|"));

/* ----- Editor: 6 · Guide > Ask Azu > Outputs: the voice of the answers ----- */
function lvStatusText(){var k=lvKeys(),l=LV.last,names={device:"the device voice",azure:"Azure",eleven:"ElevenLabs"};
  var s="Keys on this computer: Azure "+(k.azure?"✓":"none")+", ElevenLabs "+(k.eleven?"✓":"none")+".";
  if(S.agentCfg.liveVoice!=="device"&&!k[S.agentCfg.liveVoice])s+=" Without its key the answers use the device voice.";
  if(l)s+=l.err?" Last answer: "+names[l.voice]+" failed ("+l.err+"); the device voice read it.":" Last answer: "+names[l.voice]+", "+l.ms+" ms to the audio, "+l.shapes+" mouth shapes from "+l.from+".";
  return s}
function lvStatus(){var e=ed.querySelector(".lvstat");if(e)e.textContent=lvStatusText()}
var edCardV9a=edCard;
edCard=function(){var d=document.createElement("div");d.innerHTML=edCardV9a();
  var lab=Array.from(d.querySelectorAll('[data-fo="s7agent"] .row > label')).filter(function(l){return l.firstChild&&l.firstChild.textContent==="Spoken answer"})[0];
  if(lab)lab.parentNode.insertAdjacentHTML("afterend",row("Voice of the answers",pseg("agentCfg.liveVoice",["device","azure","eleven"],["Device voice","Azure (live)","ElevenLabs (live)"])+'<div class="uses lvstat">'+esc(lvStatusText())+'</div>',
      "Device voice: the phone's own, free; the mouth follows each word's letters over the word's time. Azure (live): the narration's voice (Sonia in English, Raquel in Portuguese) made for each answer; the mouth follows Azure's visemes. ElevenLabs (live): the voices picked for the recorded set, with each letter's time; the mouth follows the letters at their times. The live voices need a key (on this computer from .secrets/keys.env, by keys_local.py; in the app, on the server, which streams the audio and its timings a sentence at a time). Without a key, or on an error, the device voice reads the answer.",true)+
    (S.agentCfg.liveVoice==="eleven"?row("ElevenLabs model",pseg("agentCfg.elevenModel",["eleven_flash_v2_5","eleven_multilingual_v2"],["Flash v2.5 (fastest)","Multilingual v2 (richest)"]),"Flash: about 75 ms to the first audio and half the credits; Multilingual v2: the richest voice, slower. Both speak European Portuguese with a pt-PT voice.",true):""));
  return d.innerHTML};
