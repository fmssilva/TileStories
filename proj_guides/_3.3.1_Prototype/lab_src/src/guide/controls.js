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

/* Editor: 6 · Guide > Guide panel gets Stop */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7dock",row("Stop",pseg("guideCfg.stopUndo",["toast","ask"],["Undo toast","Ask first"]),"Undo toast: Stop ends the tour at once, with no question, and a toast says “Tour stopped · Resume” for 5 s; Resume goes back to the step and the sentence it stopped at. The point is kept after the toast is gone: the guide's button offers Resume the tour. Ask first: the tour pauses and asks “Stop the tour?” (Stop / Keep listening). Previous, mid-sentence, says the sentence again; a second tap within 2 s goes to the step before."),"Azu's own words");
  return d.innerHTML});
/* the row's demo: Stop pressed mid-sentence at Mini: the toast, or the question */
rowDemos({"guideCfg.stopUndo":{tour:"g5",dock:"mini",act:function(){var b=ph.querySelector('.gd-mini [data-g="stop"]')||ph.querySelector('.gd-ctl [data-g="stop"]');if(b)b.click()},ring:".gtoast|.gstopask"}});
