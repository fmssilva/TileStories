/* The reading head (markWord, hlTick): which word is being said, and the share of it said, for every place that lights the words (round 21:
   the light itself, its style, trail and glide are guide/highlight.js's, the one highlighter). */
/* the device voice waits up to 1.5 s for the voices, inside the tap's activation, before it falls back to captions only; its read-along starts with each line */
on("sayDevice.before",function(){if(G.voiceWait){clearInterval(G.voiceWait);G.voiceWait=null}
  G.hl={k:-1,at:0,p:0,cpm:0,ended:false,last:0,seen:false,start:performance.now(),target:G.hl&&G.hl.target};G.noVoice=false});
on("sayDevice.started",function(){G.noVoice=!S.guideMute&&!G.utter&&!!G.timer;var n=ph.querySelector(".gnovoice");if(n)n.hidden=!G.noVoice;hlLoop()});
on("sayDevice.ended",function(){G.hl.ended=true;G.hl.endAt=performance.now()});
/* the reading head: a word boundary (the voice) or a tick (no voice) sets the word; between them the head glides through it */
/* markWord: before (latest first) and done (always, in module order) share the notes cx; after, in module order */
function markWord(k,target){var cx={};emitLatestFirst("markWord.before",cx);try{var h=G.hl,now=performance.now();if(G.utter&&!h.est)h.seen=true;if(target!==h.target){hlClear(h.target);h.target=target||null}
  if(k!==h.k){if(h.k>=0&&k>h.k&&!G.timer){var ch=0;for(var j=h.k;j<k;j++)ch+=G.words[j].w.length+1;var per=(now-h.at)/Math.max(1,ch);if(per>15&&per<400)h.cpm=h.cpm?h.cpm*.6+per*.4:per}h.k=k;h.at=now}hlLoop();emit("markWord.after",k,target)}finally{emit("markWord.done",cx)}}
function wordMs(k){var ans=firstAnswer("wordMs.answer",k);if(ans!==undefined)return ans;var base=60000/(170*guideSpeed());if(G.timer||!G.words[k])return base;return(G.words[k].w.length+1)*(G.hl.cpm||base/6)}
function hlLoop(){if(!G.hlRaf)G.hlRaf=requestAnimationFrame(hlTick)}
function hlTick(now){G.hlRaf=0;var h=G.hl;if(!h)return;
  /* round 21: the head runs while a line is said anywhere (a tip's bubble, What next?'s line, an answer: not only in a tour), and 400 ms after
     its end (the light's last glide and the tail's fade) */
  if(!G.on&&!G.utter&&!G.timer&&!G.recAudio&&(!h.ended||now-(h.endAt||0)>400))return;var dt=h.last?Math.min(100,now-h.last):16;h.last=now;
  if(G.paused){h.at+=dt;h.start+=dt}
  /* some voices send no word events (Chrome's network voices, Android): after 1.2 s without one, the head follows the time, by characters */
  if(G.utter&&!h.seen&&!h.ended&&now-h.start>1200){h.est=true;var el=now-h.start,per=60000/(170*guideSpeed())/6,c=0,k=0;for(;k<G.words.length-1;k++){c+=G.words[k].w.length+1;if(c*per>el)break}if(k!==h.k)markWord(k,h.target)}
  var n=G.words.length,tgt=h.ended?n:h.k<0?0:h.k+Math.min(.97,(now-h.at)/wordMs(h.k)),glide=S.guideCfg.glide&&!S.reduce;
  h.p=glide?h.p+(tgt-h.p)*Math.min(1,dt/90):(h.ended?n:Math.max(0,h.k)+1-1e-6);
  if(!glide&&h.k<0)h.p=0;if(glide&&h.k>=0&&h.p<h.k)h.p=h.k;
  hlPaintAll(h);
  G.hlRaf=requestAnimationFrame(hlTick)}
on("clearGuideMarks.after",function(){if(G.hl&&G.hl.target){hlClear(G.hl.target);G.hl.target=null}});
on("stopTour.before",function(){if(G.hl&&G.hl.target){hlClear(G.hl.target);G.hl.target=null}});
/* the captions say when this device has no voice for the language */
on("renderGuide.after",function(text){var cap=ph.querySelector(".gcap");if(!cap)return;
  cap.insertAdjacentHTML("afterbegin",'<small class="gnovoice"'+(G.noVoice?"":" hidden")+'>'+TXH("guide_novoice")+'</small>');if(G.hl)hlGlide(cap,cap.closest(".gbubble")?"bubble":"captions",G.hl.p,G.hl.ended)});

