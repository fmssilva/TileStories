/* ===================== v8i: the guide's voice from the first step; the words read light up as a comet, a fill or one word, gliding ===================== */
S.guideCfg.hl="comet";S.guideCfg.trail=6;S.guideCfg.glide=true;S.guideCfg.voice_en="";S.guideCfg.voice_pt="";
/* the browser loads its voices lazily: the first getVoices() is empty, so the tour's first step had no voice (captions only) */
var VOICES=[];function loadVoices(){if(window.speechSynthesis)VOICES=speechSynthesis.getVoices()||[];return VOICES.length}
if(window.speechSynthesis){loadVoices();var voicesSeen=false;speechSynthesis.addEventListener("voiceschanged",function(){loadVoices();if(!voicesSeen&&VOICES.length){voicesSeen=true;try{renderEditor()}catch(e){}}})}
/* the voices for a language, best first: the exact accent (pt-PT, en-GB), then natural / neural voices, then the rest */
function voicesFor(lang){var want=lang==="pt"?["pt-PT","pt"]:["en-GB","en"],out=[];
  want.forEach(function(w){VOICES.forEach(function(v){if((v.lang||"").replace("_","-").indexOf(w)===0&&out.indexOf(v)<0)out.push(v)})});
  var rank=function(v){return((v.lang||"").replace("_","-").indexOf(want[0])===0?0:2)+(/natural|neural|online|premium|enhanced/i.test(v.name)?0:1)};
  return out.map(function(v,i){return[v,i]}).sort(function(a,b){return rank(a[0])-rank(b[0])||a[1]-b[1]}).map(function(x){return x[0]})}
voiceFor=function(lang){var vs=voicesFor(lang),pick=S.guideCfg["voice_"+lang];for(var i=0;i<vs.length;i++)if(vs[i].name===pick)return vs[i];return vs[0]||null};
/* say waits up to 1.5 s for the voices, inside the tap's activation, before it falls back to captions only */
var sayV8i=say;
say=function(text,onWord,onEnd){if(G.voiceWait){clearInterval(G.voiceWait);G.voiceWait=null}
  G.hl={k:-1,at:0,p:0,cpm:0,ended:false,last:0,seen:false,start:performance.now(),target:G.hl&&G.hl.target};G.noVoice=false;
  var go=function(){sayV8i(text,onWord,function(){G.hl.ended=true;onEnd()});G.noVoice=!S.guideMute&&!G.utter&&!!G.timer;var n=ph.querySelector(".gnovoice");if(n)n.hidden=!G.noVoice;hlLoop()};
  if(window.speechSynthesis&&!S.guideMute&&!loadVoices()){var t0=Date.now();G.voiceWait=setInterval(function(){if(loadVoices()||Date.now()-t0>1500){clearInterval(G.voiceWait);G.voiceWait=null;go()}},50);return}
  go()};
var stopSpeechV8i=stopSpeech;stopSpeech=function(){if(G.voiceWait){clearInterval(G.voiceWait);G.voiceWait=null}stopSpeechV8i()};
/* the reading head: a word boundary (the voice) or a tick (no voice) sets the word; between them the head glides through it */
markWord=function(k,target){var h=G.hl,now=performance.now();if(G.utter&&!h.est)h.seen=true;if(target!==h.target){hlClear(h.target);h.target=target||null}
  if(k!==h.k){if(h.k>=0&&k>h.k&&!G.timer){var ch=0;for(var j=h.k;j<k;j++)ch+=G.words[j].w.length+1;var per=(now-h.at)/Math.max(1,ch);if(per>15&&per<400)h.cpm=h.cpm?h.cpm*.6+per*.4:per}h.k=k;h.at=now}hlLoop()};
function wordMs(k){var base=60000/(170*S.guideCfg.speed);if(G.timer||!G.words[k])return base;return(G.words[k].w.length+1)*(G.hl.cpm||base/6)}
function hlLoop(){if(!G.hlRaf)G.hlRaf=requestAnimationFrame(hlTick)}
function hlTick(now){G.hlRaf=0;var h=G.hl;if(!G.on||!h){return}var dt=h.last?Math.min(100,now-h.last):16;h.last=now;
  if(G.paused){h.at+=dt;h.start+=dt}
  /* some voices send no word events (Chrome's network voices, Android): after 1.2 s without one, the head follows the time, by characters */
  if(G.utter&&!h.seen&&!h.ended&&now-h.start>1200){h.est=true;var el=now-h.start,per=60000/(170*S.guideCfg.speed)/6,c=0,k=0;for(;k<G.words.length-1;k++){c+=G.words[k].w.length+1;if(c*per>el)break}if(k!==h.k)markWord(k,h.target)}
  var n=G.words.length,tgt=h.ended?n:h.k<0?0:h.k+Math.min(.97,(now-h.at)/wordMs(h.k)),glide=S.guideCfg.glide&&!S.reduce;
  h.p=glide?h.p+(tgt-h.p)*Math.min(1,dt/90):(h.ended?n:Math.max(0,h.k)+1-1e-6);
  if(!glide&&h.k<0)h.p=0;
  hlPaint(ph.querySelector(".gcap"),h.p,glide);hlPaint(h.target,h.p,glide);
  G.hlRaf=requestAnimationFrame(hlTick)}
/* per word: --f its fill (read = 1), --h the head's fill, --t the trail behind (comet: fades over Trail words; fill: stays; sentence: the band), --a head or trail */
function hlPaint(box,p,glide){if(!box)return;var ws=box.querySelectorAll(".gw");if(!ws.length)return;box.classList.add("gread");
  var k=Math.floor(p),f=glide?p-k:1,mode=S.guideCfg.hl,tr=Math.max(1,+S.guideCfg.trail||6);
  var sid=[],n=0;for(var i=0;i<ws.length;i++){sid.push(n);if(/[.!?…][”»")\]]*$/.test(ws[i].textContent))n++}
  for(var j=0;j<ws.length;j++){var w=ws[j],fi,hd,t;
    if(j>k){fi=0;hd=0;t=0}else if(j===k){fi=glide?f:1;hd=fi;t=0}else{fi=1;hd=0;t=mode==="comet"?Math.max(0,1-(p-j-1)/tr):mode==="fill"?1:0}
    if(mode==="sentence")t=k<ws.length&&sid[j]===sid[Math.min(k,ws.length-1)]&&j!==k?.22:0;
    w.style.setProperty("--f",fi.toFixed(3));w.style.setProperty("--h",hd.toFixed(3));w.style.setProperty("--t",t.toFixed(3));w.style.setProperty("--a",(j===k?1:t).toFixed(3));
    w.classList.toggle("un",j>k)}}
function hlClear(box){if(!box)return;box.classList.remove("gread");box.querySelectorAll(".gw").forEach(function(w){w.removeAttribute("style");w.classList.remove("un")})}
var clearGuideMarksV8i=clearGuideMarks;clearGuideMarks=function(){clearGuideMarksV8i();if(G.hl&&G.hl.target){hlClear(G.hl.target);G.hl.target=null}};
var stopTourV8i=stopTour;stopTour=function(){if(G.hl&&G.hl.target){hlClear(G.hl.target);G.hl.target=null}stopTourV8i()};
/* the captions say when this device has no voice for the language */
var renderGuideV8i=renderGuide;renderGuide=function(text){renderGuideV8i(text);var cap=ph.querySelector(".gcap");if(!cap)return;
  cap.insertAdjacentHTML("afterbegin",'<small class="gnovoice"'+(G.noVoice?"":" hidden")+'>'+TXH("guide_novoice")+'</small>');if(G.hl)hlPaint(cap,G.hl.p,S.guideCfg.glide&&!S.reduce)};

/* Editor: Captions gets the highlight's style, its trail, the glide, the voice in this browser and a Try */
var edCardV8i=edCard;
edCard=function(){var d=document.createElement("div");d.innerHTML=edCardV8i();
  var vs=voicesFor(S.lang),key="guideCfg.voice_"+S.lang;
  addRows(d,"s7cap",row("Highlight style",pseg("guideCfg.hl",["comet","sentence","fill","word"],["Comet","Sentence + word","Fill","One word"]),"How the words being read light up, in the captions and in the block's own text. Comet: the word being said is bright and the words behind it fade away. Sentence + word: a soft band over the sentence being said, the word in it brighter (a small study found a sentence band easiest to follow for readers with dyslexia). Fill: the words already said stay lit. One word: only the word being said.")+
    row("Trail",pslider("guideCfg.trail",2,14,1),"Comet only: how many words the fading trail covers behind the word being said.",true)+
    row("Glide",pseg("guideCfg.glide",["true","false"],["Smooth","Word by word"]),"Smooth: the light moves through each word as it is spoken, without jumps. Word by word: it jumps from word to word. Reduce Motion: word by word.")+
    row("Try",'<button class="del" type="button" data-hltry="1">Try</button>',"Starts the tour with these settings."));
  addRows(d,"s7voice",row("Voice in this browser",psel(key,[["","Best available"]].concat(vs.map(function(v){return[v.name,v.name+(v.localService?"":" · online")]}))),
    "The prototype speaks with this browser's voices (in Edge, the Natural voices are online and sound best). The app speaks with recorded files or the phone's voice. "+(vs.length?vs.length+" voice(s) for this language here.":"No voice for this language in this browser: captions only.")));
  return d.innerHTML};
ed.addEventListener("click",function(e){var t=e.target.closest("[data-hltry]");if(t){e.stopPropagation();if(G.on)stopTour();startTour()}},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(hl|trail|glide|voice_en|voice_pt|"));
