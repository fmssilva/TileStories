/* ===================== v9b: the real mouth. Azure's facial curves (its 55 blend shapes, 60 frames a second, as three channels: open, round, wide) move Azu's mouth whatever its source: the 3D models' morphs, the tile of light's opening, the 2D tile's nearest drawn shape. They come with the live answers in Azure, and with the recorded Azure lines (render.py keeps them). Voices without curves (ElevenLabs, the recorded sets of other providers): the opening follows the sound's loudness, the shape the letters. The device voice keeps the letters ===================== */
S.agentCfg.liveVoice="azure";   /* the developer's choice (2026-11-04); without Azure's key, the device voice reads the answers */
var MOUTH_ALPHA="0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";
/* a recorded line's curves: three characters a frame, 30 frames a second (voicebench/render.py) */
function mouthDecode(s){var f=[];for(var i=0;i+2<s.length;i+=3)f.push([MOUTH_ALPHA.indexOf(s[i])/63,MOUTH_ALPHA.indexOf(s[i+1])/63,MOUTH_ALPHA.indexOf(s[i+2])/63]);return{fps:30,f:f}}
/* one Azure frame (ARKit's order): the jaw less the lips' closing opens; pucker and funnel round; smile and stretch widen; closed lips shut both */
function mouthOfBlend(b){var cl=function(v){return Math.max(0,Math.min(1,v))},close=b[18],gate=cl(1-close*1.3);
  return[cl((b[17]-close*.9)*2.2+((b[37]+b[38])/2-.3)*.4),cl((Math.max(b[20],b[19]*1.2)*1.4-.1)*gate),cl(((b[23]+b[24])/2+(b[29]+b[30])/2-.15)*2*gate)]}
/* every source's mouth from the three channels */
function azMouthCurve(o,r,w){AzuGL.set("talk",Math.min(1,o*1.1));Azu3D.mouthTo(o,w,r);var m=ph.querySelector(".mascot");if(!m)return;
  var s=o<.1&&r<.2?"M":r>.4&&o>.06?"O":w>.4&&o<.5?"E":o>.62?"big":o>.28?"A":"chew";if(s!==G.m2d){G.m2d=s;azMouthV8q(m,s)}}

/* ----- the player: the curves follow the audio's own clock; the letters' mouth stands down meanwhile ----- */
var MT={track:null,audio:null,mark:[],raf:0,kind:""};
function mtStart(audio,track,kind){MT.track=track;MT.audio=audio;MT.kind=kind;G.visTrack=MT.mark;if(!MT.raf)MT.raf=requestAnimationFrame(mtTick)}
function mtTick(){MT.raf=0;var a=MT.audio;if(!a||G.recAudio!==a){mtStop();return}var tr=MT.track,i=Math.min(tr.f.length-1,Math.floor(a.currentTime*tr.fps)),v=i>=0?tr.f[i]:[0,0,0];
  if(S.guideCfg.lips)azMouthCurve(v[0],v[1],v[2]);MT.raf=requestAnimationFrame(mtTick)}
function mtStop(){if(MT.raf)cancelAnimationFrame(MT.raf);MT.raf=0;if(G.visTrack===MT.mark)G.visTrack=null;MT.track=null;MT.audio=null;MT.kind="";Azu3D.mouthTo(0,0,0);AzuGL.set("talk",0);
  var m=ph.querySelector(".mascot");if(m){G.m2d=null;azMouthV8q(m,restShape(azMood()))}}

/* ----- the loudness: the sound itself opens the mouth (an analyser on the audio); the letters give the shape ----- */
var LD={ctx:null,audio:null,an:null,buf:null,lvl:0,raf:0,zero:0,t0:0,shape:null};
function ldStart(a,shapeAt){try{LD.ctx=LD.ctx||new(window.AudioContext||window.webkitAudioContext)();if(LD.ctx.state==="suspended")LD.ctx.resume();
    var src=LD.ctx.createMediaElementSource(a),an=LD.ctx.createAnalyser();an.fftSize=1024;src.connect(an);an.connect(LD.ctx.destination);
    LD.audio=a;LD.an=an;LD.buf=new Float32Array(an.fftSize);LD.lvl=0;LD.zero=0;LD.t0=performance.now();LD.shape=shapeAt;LD.prev=G.visTrack;G.visTrack=MT.mark;MT.kind="loudness";
    if(!LD.raf)LD.raf=requestAnimationFrame(ldTick)}catch(e){LD.audio=null}}
function ldTick(){LD.raf=0;var a=LD.audio;if(!a||G.recAudio!==a){ldStop();return}LD.an.getFloatTimeDomainData(LD.buf);var s=0;for(var i=0;i<LD.buf.length;i++)s+=LD.buf[i]*LD.buf[i];var rms=Math.sqrt(s/LD.buf.length);
  /* an audio the page may not read (a file opened from disk) gives only zeros: then the letters move the mouth again */
  if(rms===0&&!a.paused&&a.currentTime>.4){if(++LD.zero>20){var p=LD.prev;ldStop(true);G.visTrack=p;return}}else if(rms>0)LD.zero=0;
  LD.lvl+=(Math.min(1,Math.max(0,(rms-.012)*7))-LD.lvl)*.45;var sh=LD.shape?LD.shape(a.currentTime):"A",o=LD.lvl*(sh==="M"?.15:1);
  if(S.guideCfg.lips)azMouthCurve(o,sh==="O"?.75*Math.min(1,LD.lvl*2):0,sh==="E"?.6*Math.min(1,LD.lvl*2):0);LD.raf=requestAnimationFrame(ldTick)}
function ldStop(keep){if(LD.raf)cancelAnimationFrame(LD.raf);LD.raf=0;LD.audio=null;if(!keep&&G.visTrack===MT.mark)G.visTrack=null;if(MT.kind==="loudness")MT.kind="";Azu3D.mouthTo(0,0,0);AzuGL.set("talk",0)}
/* the shape of the moment: a live answer's letters at their times, or the current word's letters */
function ldShapeFromVis(vis){return function(t){var s="A";for(var i=0;i<vis.length&&vis[i].t<=t;i++)s=vis[i].s;return s}}
function ldShapeFromWords(){return function(){var k=G.hl&&G.hl.k,w=k>=0&&G.words[k];return w?shapesOf(w.w)[0]:"M"}}

/* ----- Azure live, with the facial curves (SSML asks for them; word offsets map back to the text) ----- */
function lvSsml(text,voice){var head='<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xmlns:mstts="http://www.w3.org/2001/mstts" xml:lang="'+voice.slice(0,5)+'"><voice name="'+voice+'"><mstts:viseme type="FacialExpression"/>',body="",back=[];
  for(var i=0;i<text.length;i++){var c=text[i],e=c==="&"?"&amp;":c==="<"?"&lt;":c===">"?"&gt;":c;body+=e;for(var j=0;j<e.length;j++)back.push(i)}return{ssml:head+body+"</voice></speak>",at:head.length,back:back}}
lvAzure=function(text){var k=window.LABKEYS;return lvSdk().then(function(SDK){return new Promise(function(res,rej){
    var voice=S.lang==="pt"?"pt-PT-RaquelNeural":"en-GB-SoniaNeural",cfg=SDK.SpeechConfig.fromSubscription(k.azure,k.azureRegion);cfg.speechSynthesisVoiceName=voice;
    cfg.speechSynthesisOutputFormat=SDK.SpeechSynthesisOutputFormat.Audio24Khz48KBitRateMonoMp3;
    var syn=new SDK.SpeechSynthesizer(cfg,null),chunks=[],bounds=[],sm=lvSsml(text,voice),ids=0;
    syn.visemeReceived=function(s,e){ids++;if(e.animation)try{chunks.push(JSON.parse(e.animation))}catch(x){}};
    syn.wordBoundary=function(s,e){var o=e.textOffset-sm.at;if(o>=0&&o<sm.back.length)bounds.push({t:e.audioOffset/1e7,d:(e.duration||0)/1e7,o:sm.back[o]})};
    syn.speakSsmlAsync(sm.ssml,function(r){try{syn.close()}catch(x){}if(r.reason!==SDK.ResultReason.SynthesizingAudioCompleted){rej(new Error("Azure: "+(r.errorDetails||"no audio")));return}
      var ws=lvWords(text),times=[];bounds.forEach(function(b){for(var n=0;n<ws.length;n++)if(b.o>=ws[n].i&&b.o<ws[n].i+ws[n].w.length){if(!times[n])times[n]=[b.t,b.t+Math.max(.05,b.d)];break}});
      chunks.sort(function(a,b){return a.FrameIndex-b.FrameIndex});var f=[];chunks.forEach(function(c){(c.BlendShapes||[]).forEach(function(b){f.push(mouthOfBlend(b))})});
      res({audio:r.audioData,words:lvFill(times,ws.length),vis:[],mouth:f.length?{fps:60,f:f}:null,shapes:f.length||ids,from:f.length?"Azure's facial curves":"Azure's visemes"})},
      function(e){try{syn.close()}catch(x){}rej(new Error("Azure: "+e))})})})};
/* the live answer: Azure's curves, or the loudness with the letters (ElevenLabs) */
var lvPlayV9b=lvPlay;lvPlay=function(text,onWord,onEnd,r){lvPlayV9b(text,onWord,onEnd,r);var a=G.recAudio;if(!a)return;
  if(r.mouth)mtStart(a,r.mouth,"curves");else if(r.vis&&r.vis.length)ldStart(a,ldShapeFromVis(r.vis))};
/* the recorded lines: their curves when the set has them (Azure), else the loudness with the words' letters */
var sayV9b=say;say=function(text,onWord,onEnd){sayV9b(text,onWord,onEnd);var a=G.recAudio;if(!a||!G.utter||G.utter.live||MT.audio===a||LD.audio===a)return;
  var r=S.guideCfg.voice==="recorded"?recFile(text):null;if(!r)return;if(r.file&&r.file.mouth)mtStart(a,mouthDecode(r.file.mouth),"curves");else ldStart(a,ldShapeFromWords())};
var stopSpeechV9b=stopSpeech;stopSpeech=function(){if(MT.audio)mtStop();if(LD.audio)ldStop();stopSpeechV9b()};
/* the moment's mouth, for the Editor and the checks */
function mouthNow(){return{kind:MT.kind||(G.visTrack?"shapes":"letters"),talk:AzuGL.get("talk"),m3d:Azu3D.morphNow("Open")}}

/* ----- Editor: 6 · Guide > Mascot: how the mouth moves now ----- */
var edCardV9b=edCard;
edCard=function(){var d=document.createElement("div");d.innerHTML=edCardV9b();var r=rowEl(d,"s7mascot","Mouth");
  if(r)r.insertAdjacentHTML("afterend",row("How it moves",'<span class="uses">Azure\'s facial curves: 60 frames a second, the jaw, the lips\' closing, their rounding and widening (the live answers in Azure, the recorded Azure lines). Other recorded voices and ElevenLabs: the sound\'s loudness opens it, the letters shape it. The device voice: the letters at the word times.</span>',"The same three values (open, round, wide) move every source: the 3D models' morphs Open, Round and Wide, the tile of light's opening, the 2D tile's nearest drawn mouth. Unity: the Worker streams Azure's blend shapes with the audio; SetViseme takes the three values; uLipSync gives the same from any audio.",true));
  return d.innerHTML};
