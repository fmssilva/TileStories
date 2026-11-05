/* The live voice: Azure or ElevenLabs live, for the answers and the lines the recorded set lacks; the source say.live and a source of camSay; the mouth on the voice's own timing. */
S.agentCfg.liveVoice="azure";S.agentCfg.elevenModel="eleven_flash_v2_5";   /* without Azure's key, the device voice reads the answers */
var AZ_SDK="https://cdn.jsdelivr.net/npm/microsoft-cognitiveservices-speech-sdk@1.43.0/distrib/browser/microsoft.cognitiveservices.speech.sdk.bundle-min.js";
var LV={last:null,sdk:null};
/* the keys: only this computer's (keys.local.js, from .secrets/keys.env); the published Lab has none and speaks with the device voice */
function lvKeys(){var k=window.LABKEYS||{};return{azure:!!(k.azure&&k.azureRegion),eleven:!!(k.eleven&&(k.elevenPT||k.elevenEN))}}
function lvOn(){var v=S.agentCfg.liveVoice;return v!=="device"&&!!lvKeys()[v]}
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

/* ----- the live line plays as a recorded line (the recorded voice's controls act on G.recAudio), the mouth on the voice's own shapes;
   paused while its audio was being made, it waits for Resume ----- */
function lvPlay(text,onWord,onEnd,r){var held=LV.held;stopSpeech();G.words=lvWords(text);G.recWords=r.words;G.visTrack=r.vis;
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
  if(!held){var p=a.play();if(p&&p.catch)p.catch(function(){if(G.recAudio===a){a.muted=true;a.play().catch(function(){})}})}requestAnimationFrame(tick);emit("lvPlay.after",text,r)}
/* while the voice's own shapes move the mouth, the letters-per-word mouth stands down (the word light goes on) */
on("markWord.before",function(cx){if(G.visTrack){cx.lips=S.guideCfg.lips;S.guideCfg.lips=false}});on("markWord.done",function(cx){if("lips" in cx)S.guideCfg.lips=cx.lips});
on("stopSpeech.before",function(){G.lvSeq=(G.lvSeq||0)+1;G.visTrack=null;LV.wait=false;LV.held=false});
/* the sources after the live one (guide/voice.js's sayOrder): a live line that fails hands its line on, and says why */
var LVN={device:"the device voice",azure:"Azure",eleven:"ElevenLabs"};
function lvDevice(t,w,e){var l=LV.last||{},n=LVN[l.voice]||"The live voice";sayFrom("live",t,w,e,{not:n+" failed ("+(l.err||"no audio")+")",short:n+" failed"})}
/* the source say.live: any line its turn comes to (sayOrder: an answer first, a line not recorded after the recorded set) */
function lvSay(text,onWord,onEnd){if(!text)return;var v=S.agentCfg.liveVoice;
  if(v==="device")return{not:"Ask Azu's Voice of the answers is the device voice",short:"no live voice chosen"};
  if(!lvKeys()[v])return{not:"no "+LVN[v]+" key on this page",short:"no live key on this page"};
  var t0=performance.now(),my=G.lvSeq=(G.lvSeq||0)+1,ans=text===G.answer;LV.wait=true;LV.held=false;
  (v==="azure"?lvAzure(text):lvEleven(text)).then(function(r){if(G.lvSeq!==my)return;LV.wait=false;LV.last={voice:v,answer:ans,ms:Math.round(performance.now()-t0),shapes:r.shapes||r.vis.length,words:r.words.length,from:r.from};lvPlay(text,onWord,onEnd,r);lvStatus()})
    .catch(function(e){if(G.lvSeq!==my)return;LV.wait=false;LV.last={voice:v,answer:ans,err:String(e&&e.message||e)};console.warn("live voice",e);lvStatus();lvDevice(text,onWord,onEnd)});return{speaks:LVN[v]}}
on("say.live",lvSay);
/* a pause or a mute while the audio is being made: it plays held (paused) or muted when it comes */
on("pauseNow.voice",function(){if(LV.wait){LV.held=true;return true}});
on("resumeNow.voice",function(){if(LV.wait){LV.held=false;return true}});
on("mute.voice",function(){if(LV.wait)return true});
/* the wall's answers (Ask about the view) too: the audio, no captions */
function lvCamSay(t){if(!lvOn())return;G.cam.busy=false;G.cam.answer=t;camLayer();var v=S.agentCfg.liveVoice,t0=performance.now();
  (v==="azure"?lvAzure(t):lvEleven(t)).then(function(r){LV.last={voice:v,ms:Math.round(performance.now()-t0),shapes:r.shapes||r.vis.length,words:r.words.length,from:r.from};lvStatus();if(S.guideMute)return;
    if(LV.camAudio)try{LV.camAudio.pause()}catch(x){}LV.camAudio=new Audio(URL.createObjectURL(new Blob([r.audio],{type:"audio/mpeg"})));LV.camAudio.play().catch(function(){})})
    .catch(function(e){LV.last={voice:v,err:String(e&&e.message||e)};lvStatus();camSayFrom(lvCamSay,t)});return true}
on("camSay.source",lvCamSay);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(agentCfg\\.(liveVoice|elevenModel)|"));

/* ----- Editor: 6 · Guide > Ask Azu > Outputs: the voice of the answers ----- */
function lvStatusText(){var k=lvKeys(),l=LV.last,names=LVN;
  var s="Keys on this computer: Azure "+(k.azure?"✓":"none")+", ElevenLabs "+(k.eleven?"✓":"none")+".";
  if(S.agentCfg.liveVoice!=="device"&&!k[S.agentCfg.liveVoice])s+=" Without its key the answers use the device voice, and so do the tour's lines the recorded set lacks.";
  if(l){var w=l.answer===false?" Last line: ":" Last answer: ";s+=l.err?w+names[l.voice]+" failed ("+l.err+"); the device voice read it.":w+names[l.voice]+", "+l.ms+" ms to the audio, "+l.shapes+" mouth shapes from "+l.from+"."}
  return s}
function lvStatus(){var e=ed.querySelector(".lvstat");if(e)e.textContent=lvStatusText()}
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  var lab=Array.from(d.querySelectorAll('[data-fo="s7agent"] .row > label')).filter(function(l){return l.firstChild&&l.firstChild.textContent==="Spoken answer"})[0];
  if(lab)lab.parentNode.insertAdjacentHTML("afterend",row("Voice of the answers",pseg("agentCfg.liveVoice",["device","azure","eleven"],["Device voice","Azure (live)","ElevenLabs (live)"])+'<div class="uses lvstat">'+esc(lvStatusText())+'</div>',
      "Device voice: the phone's own, free; the mouth follows each word's letters over the word's time. Azure (live): the narration's voice (Sonia in English, Raquel in Portuguese) made for each answer; the mouth follows Azure's visemes. ElevenLabs (live): the voices picked for the recorded set, with each letter's time; the mouth follows the letters at their times. The live voices need a key (on this computer from .secrets/keys.env, by keys_local.py; in the app, on the server, which streams the audio and its timings a sentence at a time). Without a key, or on an error, the device voice reads the answer. The live voice also speaks the tour's lines the recorded set lacks (Narration > Voice = Recorded, then live, then this device); Narration > Speaking now says which voice speaks and why.",true)+
    (S.agentCfg.liveVoice==="eleven"?row("ElevenLabs model",pseg("agentCfg.elevenModel",["eleven_flash_v2_5","eleven_multilingual_v2"],["Flash v2.5 (fastest)","Multilingual v2 (richest)"]),"Flash: about 75 ms to the first audio and half the credits; Multilingual v2: the richest voice, slower. Both speak European Portuguese with a pt-PT voice.",true):""));
  return d.innerHTML});
