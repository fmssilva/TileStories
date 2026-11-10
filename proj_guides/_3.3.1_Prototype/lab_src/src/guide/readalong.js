/* The read-along light: Comet, Sentence + word, Fill, One word; the trail and the glide (markWord, hlTick). */
S.guideCfg.hl="comet";S.guideCfg.trail=6;S.guideCfg.glide=true;
/* the device voice waits up to 1.5 s for the voices, inside the tap's activation, before it falls back to captions only; its read-along starts with each line */
on("sayDevice.before",function(){if(G.voiceWait){clearInterval(G.voiceWait);G.voiceWait=null}
  G.hl={k:-1,at:0,p:0,cpm:0,ended:false,last:0,seen:false,start:performance.now(),target:G.hl&&G.hl.target};G.noVoice=false});
on("sayDevice.started",function(){G.noVoice=!S.guideMute&&!G.utter&&!!G.timer;var n=ph.querySelector(".gnovoice");if(n)n.hidden=!G.noVoice;hlLoop()});
on("sayDevice.ended",function(){G.hl.ended=true});
/* the reading head: a word boundary (the voice) or a tick (no voice) sets the word; between them the head glides through it */
/* markWord: before (latest first) and done (always, in module order) share the notes cx; after, in module order */
function markWord(k,target){var cx={};emitLatestFirst("markWord.before",cx);try{var h=G.hl,now=performance.now();if(G.utter&&!h.est)h.seen=true;if(target!==h.target){hlClear(h.target);h.target=target||null}
  if(k!==h.k){if(h.k>=0&&k>h.k&&!G.timer){var ch=0;for(var j=h.k;j<k;j++)ch+=G.words[j].w.length+1;var per=(now-h.at)/Math.max(1,ch);if(per>15&&per<400)h.cpm=h.cpm?h.cpm*.6+per*.4:per}h.k=k;h.at=now}hlLoop();emit("markWord.after",k,target)}finally{emit("markWord.done",cx)}}
function wordMs(k){var ans=firstAnswer("wordMs.answer",k);if(ans!==undefined)return ans;var base=60000/(170*guideSpeed());if(G.timer||!G.words[k])return base;return(G.words[k].w.length+1)*(G.hl.cpm||base/6)}
function hlLoop(){if(!G.hlRaf)G.hlRaf=requestAnimationFrame(hlTick)}
function hlTick(now){G.hlRaf=0;var h=G.hl;if(!G.on||!h){return}var dt=h.last?Math.min(100,now-h.last):16;h.last=now;
  if(G.paused){h.at+=dt;h.start+=dt}
  /* some voices send no word events (Chrome's network voices, Android): after 1.2 s without one, the head follows the time, by characters */
  if(G.utter&&!h.seen&&!h.ended&&now-h.start>1200){h.est=true;var el=now-h.start,per=60000/(170*guideSpeed())/6,c=0,k=0;for(;k<G.words.length-1;k++){c+=G.words[k].w.length+1;if(c*per>el)break}if(k!==h.k)markWord(k,h.target)}
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
on("clearGuideMarks.after",function(){if(G.hl&&G.hl.target){hlClear(G.hl.target);G.hl.target=null}});
on("stopTour.before",function(){if(G.hl&&G.hl.target){hlClear(G.hl.target);G.hl.target=null}});
/* the captions say when this device has no voice for the language */
on("renderGuide.after",function(text){var cap=ph.querySelector(".gcap");if(!cap)return;
  cap.insertAdjacentHTML("afterbegin",'<small class="gnovoice"'+(G.noVoice?"":" hidden")+'>'+TXH("guide_novoice")+'</small>');if(G.hl)hlPaint(cap,G.hl.p,S.guideCfg.glide&&!S.reduce)});

/* Editor: Captions gets the highlight's style, its trail, the glide, the voice in this browser and a Try */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  var vs=voicesFor(S.lang),key="guideCfg.voice_"+S.lang;
  addRows(d,"s7cap",row("Highlight style",pseg("guideCfg.hl",["comet","sentence","fill","word"],["Comet","Sentence + word","Fill","One word"]),"How the words being read light up, in the captions and in the block's own text. Comet: the word being said is bright and the words behind it fade away. Sentence + word: a soft band over the sentence being said, the word in it brighter (a small study found a sentence band easiest to follow for readers with dyslexia). Fill: the words already said stay lit. One word: only the word being said.")+
    row("Trail",pslider("guideCfg.trail",2,14,1),"Comet only: how many words the fading trail covers behind the word being said.",true)+
    row("Glide",pseg("guideCfg.glide",["true","false"],["Smooth","Word by word"]),"Smooth: the light moves through each word as it is spoken, without jumps. Word by word: it jumps from word to word. Reduce Motion: word by word.")+
    row("Try",'<button class="del" type="button" data-hltry="1">Try</button>',"Starts the tour with these settings."));
  addRows(d,"s7voice",row("Voice in this browser",psel(key,[["","Best available"]].concat(vs.map(function(v){return[v.name,v.name+(v.localService?"":" · online")]}))),
    "The prototype speaks with this browser's voices (in Edge, the Natural voices are online and sound best). The app speaks with recorded files or the phone's voice. "+(vs.length?vs.length+" voice(s) for this language here.":"No voice for this language in this browser: captions only.")));
  return d.innerHTML});
ed.addEventListener("click",function(e){var t=e.target.closest("[data-hltry]");if(t){e.stopPropagation();if(G.on)stopTour();startTour()}},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(hl|trail|glide|voice_en|voice_pt|"));
/* each row's demo: a block read in place, its words lit */
rowDemos({"guideCfg.hl":{tour:"g4",ring:".gfocus"},"guideCfg.trail":{tour:"g4",ring:".gfocus"},"guideCfg.glide":{tour:"g4",ring:".gfocus"},
  "guideCfg.voice_en":{tour:"g1",ring:".gbubble|.gdock"},"guideCfg.voice_pt":{tour:"g1",ring:".gbubble|.gdock"}});
