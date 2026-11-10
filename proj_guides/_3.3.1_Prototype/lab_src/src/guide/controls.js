/* The tour's Stop and Previous: Stop with its Undo toast (or Ask first) and the resume point (the step and the sentence); Previous replays the sentence being said. */
S.guideCfg.stopUndo="toast";G.resumeAt=null;
/* ----- the resume point: where the visitor's Stop left the tour, the step and the first word of the sentence being said. It is kept
   after the toast is gone (the guide's button offers Resume the tour) until the tour is resumed or reaches its end ----- */
function keepResume(){var st=S.guideSteps[G.i];if(!G.on||!st||st.act==="end"){G.resumeAt=null;return null}var intro=st.act==="intro",nx=intro?S.guideSteps[G.i+1]:st;
  return G.resumeAt={id:(nx||st).id,k:intro||!(G.hl&&G.hl.k>=0)?0:sentenceStart(G.hl.k),title:stepTitle(nx||st)}}
function resumeTourAt(pt){pt=pt||G.resumeAt;if(!pt)return;G.resumeAt=null;hideStopUi();if(G.on)stopTour();G.startWord=pt.k||0;startTour({id:pt.id})}
/* the step's line, said again from the kept sentence (guide/voice.js's seekWord: an audio seeks, the device voice speaks from the word) */
on("say.after",function(){if(!G.startWord)return;var k=G.startWord;G.startWord=0;seekWord(k)});
on("runStep.after",function(){var st=S.guideSteps[G.i];if(st&&st.act==="end")G.resumeAt=null});
/* ----- Stop: no question by default; the tour stops and a toast offers Resume for 5 s. Ask first: the tour pauses and asks ----- */
function stopUiHtml(kind){return kind==="ask"?'<b>'+TXH("guide_stop_ask")+'</b><div><button type="button" data-gstop="yes">'+TXH("guide_stop_yes")+'</button><button type="button" data-gstop="no">'+TXH("guide_stop_no")+'</button></div>'
  :'<span>'+TXH("guide_stopped")+'</span><button type="button" data-gstop="resume">'+TXH("guide_stop_resume")+'</button>'}
function hideStopUi(){clearTimeout(G.toastT);ph.querySelectorAll(".gtoast,.gstopask").forEach(function(x){x.remove()})}
/* above whatever is at the foot of the phone: the dock, the guide's button, the Navigation Bar */
function placeStopUi(d){var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight,top=ph.offsetHeight-12;
  [ph.querySelector(".gdock"),ph.querySelector(".gbtn-wrap"),ph.querySelector(".gbubble"),document.getElementById("navbar")].forEach(function(e){if(!e||getComputedStyle(e).display==="none")return;var r=e.getBoundingClientRect();if(r.height&&r.top>pr.top+pr.height/2)top=Math.min(top,(r.top-pr.top)/q)});
  d.style.bottom=Math.round(ph.offsetHeight-top+10)+"px"}
function stopUi(kind){hideStopUi();var d=document.createElement("div");d.className=kind==="ask"?"gstopask":"gtoast";d.setAttribute("role",kind==="ask"?"alertdialog":"status");d.setAttribute("aria-label",TXT(kind==="ask"?"guide_stop_ask":"guide_stopped"));
  d.innerHTML=stopUiHtml(kind);ph.appendChild(d);placeStopUi(d);if(kind!=="ask")G.toastT=setTimeout(hideStopUi,5000);return d}
on("stopTap.answer",function(){if(!G.on)return;if(S.guideCfg.stopUndo==="ask"){G.explicitPause=true;pauseTour();G.explicitPause=false;stopUi("ask");return true}
  keepResume();stopTour();stopUi("toast");return true});
ph.addEventListener("click",function(e){var t=e.target.closest("[data-gstop]");if(!t)return;e.stopPropagation();var k=t.dataset.gstop;
  if(k==="resume"){resumeTourAt();return}hideStopUi();if(k==="yes"){keepResume();stopTour()}else if(k==="no")resumeTour()},true);
on("startTour.before",function(){hideStopUi()});
/* ----- Previous: mid-sentence it says the sentence again from its first word; a second tap within 2 s goes to the step before ----- */
function replaySentence(k){var s=sentenceStart(k);G.paused=false;G.askOpen=false;var held=G.resumeVoice;G.resumeVoice=false;if(!seekWord(s)){G.resumeVoice=held;return false}
  if(G.recAudio){G.recAudio.muted=!!S.guideMute;G.recAudio.play().catch(function(){})}renderGuide();return true}
on("prevTap.answer",function(){if(!G.on)return;var now=performance.now(),k=G.hl&&G.hl.k,speaking=G.utter||G.timer||G.recAudio||(G.paused&&G.resumeVoice);
  if(!speaking||!(k>=0)||!(G.words||[]).length||now-(G.prevAt||0)<2000){G.prevAt=0;return}
  G.prevAt=now;if(replaySentence(k))return true;G.prevAt=0});
/* the stop's look reaches the phone's signature (the toast or the question) */
on("modelSig.sig",function(h){return sigWant(/guideCfg\.stopUndo/)?h+"#stop:"+stopUiHtml(S.guideCfg.stopUndo):h});

/* Editor: Guide tab > Guide panel gets Stop */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7dock",row("Stop",pseg("guideCfg.stopUndo",["toast","ask"],["Undo toast","Ask first"]),"Undo toast: Stop ends the tour at once, with no question, and a toast says “Tour stopped · Resume” for 5 s; Resume goes back to the step and the sentence it stopped at. The point is kept after the toast is gone: the guide's button offers Resume the tour. Ask first: the tour pauses and asks “Stop the tour?” (Stop / Keep listening). Previous, mid-sentence, says the sentence again; a second tap within 2 s goes to the step before."),"Azu's own words");
  return d.innerHTML});
/* the row's demo: Stop pressed mid-sentence at Mini: the toast, or the question */
rowDemos({"guideCfg.stopUndo":{tour:"g5",dock:"mini",act:function(){var b=ph.querySelector('.gd-mini [data-g="stop"]')||ph.querySelector('.gd-ctl [data-g="stop"]');if(b)b.click()},ring:".gtoast|.gstopask"}});

/* ----- round 11: the speed chip, the visitor's own: 0.5× · 0.75× · 1× · 1.25× · 1.5× · 1.75× · 2× in turn (its first value Narration ›
   Speed); the voice follows at once: an audio's playbackRate, the device voice spoken again from its word at the new rate. Unity:
   AudioSource.pitch with a pitch-shifter (the time stretch), or the TextToSpeech rate from the word ----- */
var SPEEDS=[0.5,0.75,1,1.25,1.5,1.75,2];
function speedNext(v){for(var i=0;i<SPEEDS.length;i++)if(SPEEDS[i]>v+1e-6)return SPEEDS[i];return SPEEDS[0]}
function setGuideSpeed(v){S.vis.speed=v;G.rate=v;if(G.recAudio)G.recAudio.playbackRate=v;else if(G.on&&!G.paused&&(G.utter||G.timer)&&G.sayArgs)speakFrom(G.hl&&G.hl.k>=0?G.hl.k:0,false);
  ph.querySelectorAll(".gd-speed").forEach(function(b){b.textContent=spd(v)+"×";b.setAttribute("aria-label",fmt(TXT("dock_speed"),spd(v)))});dkTick()}
ph.addEventListener("click",function(e){var b=e.target.closest('[data-g="speed"]');if(!b)return;e.stopPropagation();setGuideSpeed(speedNext(guideSpeed()))},true);
/* ----- round 11: the progress bar seeks. A drag (its hit area 44 px high) or a tap: the stop under the finger, and within it the sentence
   at that place (its share of the stop's words); the knob and the time follow the finger, the voice goes there on release. Keys: ← → the
   stop before / after. Unity: a PointerDown / Move / Up manipulator on the bar's element (CapturePointer), the same rule ----- */
function seekAt(sk,x){var best=null;sk.querySelectorAll("[data-gseg]").forEach(function(s){var r=s.getBoundingClientRect();if(!best||x>=r.left)best={j:+s.dataset.gseg,r:r}});if(!best)return{j:G.i,f:0};
  return{j:best.j,f:Math.max(0,Math.min(.999,(x-best.r.left)/Math.max(1,best.r.width)))}}
function seekShow(d){var sk=d.sk.isConnected?d.sk:ph.querySelector(".gd-seek");if(!sk)return;d.sk=sk;dkKnob(sk,d.j,d.f);var T=dkTimes(),c=T.stops[d.j],el=c?c.at+d.f*(c.len-.7):0;
  ph.querySelectorAll(".gdock .gd-time").forEach(function(t){var k=t.dataset.t;t.textContent=k==="both"?mmss(el)+" / "+mmss(T.tot):k==="el"?mmss(el):k==="rem"?"-"+mmss(T.tot-el):mmss(T.tot)})}
/* to stop j, at the first word of the sentence at share f of its words */
function seekTo(j,f){var st=S.guideSteps[j];if(!st||!G.on)return;var ws=stepText(st).match(/\S+/g)||[],k=Math.min(ws.length-1,Math.floor(f*ws.length)),s=0;for(var i=0;i<k;i++)if(/[.!?…]["”»)]*$/.test(ws[i]))s=i+1;
  G.seekedTo={j:j,k:s};hideStopUi();stopSpeech();G.paused=false;G.waiting=false;G.tryWait=false;G.askOpen=false;G.startWord=s;if(S.dockH==="full")S.dockH="peek";G.i=j;runStep()}
ph.addEventListener("pointerdown",function(e){var sk=e.target.closest(".gdock .gd-seek");if(!sk)return;e.preventDefault();e.stopPropagation();try{sk.setPointerCapture(e.pointerId)}catch(x){}
  G.seekDrag=Object.assign({id:e.pointerId,sk:sk},seekAt(sk,e.clientX));sk.classList.add("dragging");seekShow(G.seekDrag)},true);
ph.addEventListener("pointermove",function(e){var d=G.seekDrag;if(!d||e.pointerId!==d.id)return;e.stopPropagation();if(!d.sk.isConnected)d.sk=ph.querySelector(".gd-seek")||d.sk;Object.assign(d,seekAt(d.sk,e.clientX));seekShow(d)},true);
["pointerup","pointercancel"].forEach(function(t){ph.addEventListener(t,function(e){var d=G.seekDrag;if(!d||e.pointerId!==d.id)return;G.seekDrag=null;e.stopPropagation();d.sk.classList.remove("dragging");if(t==="pointerup")seekTo(d.j,d.f)},true)});
ph.addEventListener("keydown",function(e){var sk=e.target.closest&&e.target.closest(".gd-seek");if(!sk||!G.on)return;var j=e.key==="ArrowRight"?G.i+1:e.key==="ArrowLeft"?G.i-1:e.key==="Home"?0:e.key==="End"?S.guideSteps.length-1:null;
  if(j==null||j<0||j>=S.guideSteps.length)return;e.preventDefault();seekTo(j,0);setTimeout(function(){var n=ph.querySelector(".gd-seek");if(n)n.focus({preventScroll:true})},0)},true);
