/* ===================== v8n: a guide that behaves like a person: it introduces itself, waits and gently insists, carries on when the visitor does it, keeps talking while they scroll, and its voice survives a pause ===================== */
S.guideCfg.intro="first";S.guideCfg.insist=2;S.guideCfg.insistAfter=5;S.guideCfg.onScroll="follow";
/* the demo point's words after the gesture: what the visitor has just revealed */
(function(){var a={g6:T("That's it: the palace before 1755, and the open square that took its place.","É isso: o palácio antes de 1755 e a praça aberta que ficou no seu lugar."),g9:T("It was Manuel the First, around 1500.","Foi D. Manuel I, por volta de 1500.")};
  S.guideSteps.forEach(function(s){if(a[s.id])s.after=a[s.id]})})();
function pointName(){var h=document.querySelector("#scr .hero h2");return h?h.textContent:""}

/* ----- the page: nothing above or below the phone; the phone takes the height there is ----- */
function fitPhoneHeight(){var h=Math.round(Math.max(640,Math.min(820,window.innerHeight-48)));if(SCREENS.phone[1]!==h){SCREENS.phone[1]=h;return true}return false}
fitPhoneHeight();
var resizeT=0;window.addEventListener("resize",function(){clearTimeout(resizeT);resizeT=setTimeout(function(){if(fitPhoneHeight()&&S.screen==="phone")renderPhone(true)},120)});

/* ----- the voice survives a pause: pausing stops it and keeps the word; going on speaks again from that word (as the app seeks
   its recorded audio). A voice that fails, or goes quiet without a pause, starts again from its word once, then the words go on by time ----- */
on("sayDevice.before",function(text,onWord,onEnd){if(!G.sayInner)G.sayArgs={text:text,onWord:onWord,onEnd:onEnd,from:0,retries:0}});
on("sayDevice.after",function(){
  if(G.utter){var u=G.utter;u.addEventListener("error",function(e){if(G.utter!==u||e.error==="interrupted"||e.error==="canceled")return;speakFrom(G.hl&&G.hl.k>=0?G.hl.k:0,true)});
    u.addEventListener("boundary",function(){G.lastWordAt=performance.now()});G.lastWordAt=performance.now()}});
function speakFrom(k,retry){var a=G.sayArgs;if(!a)return;var full=G.words.slice(),w=full[k];if(!w){a.onEnd();return}
  if(retry){a.retries++;if(a.retries>1){S.guideMuteOnce=true}}
  var off=k,sub=a.text.slice(w.i);G.sayInner=true;var mute=S.guideMute;if(S.guideMuteOnce)S.guideMute=true;
  say(sub,function(j){a.onWord(j+off)},a.onEnd);S.guideMute=mute;S.guideMuteOnce=false;G.sayInner=false;
  G.words=full;if(G.hl){G.hl.k=k;G.hl.p=k;G.hl.at=performance.now()}}
/* pauseTour and resumeTour: a feature may answer for them (the latest first); pauseNow pauses whatever plays */
function pauseTour(){if(firstAnswer("pauseTour.answer")===undefined)pauseNow()}
function pauseNow(){if(!G.on)return;G.paused=true;if(G.utter){G.pausedAt=G.hl&&G.hl.k>=0?G.hl.k:0;var u=G.utter;G.utter=null;try{speechSynthesis.cancel()}catch(e){}G.resumeVoice=true}renderGuide()}
function resumeTour(){if(firstAnswer("resumeTour.answer")!==undefined||!G.on)return;G.paused=false;G.askOpen=false;
  if(G.resumeVoice){G.resumeVoice=false;renderGuide();speakFrom(G.pausedAt||0,false);return}
  if(!G.utter&&!G.timer&&!G.waiting&&!G.tryWait)runStep();renderGuide()}
/* a voice that goes quiet for 4 s while speaking, not paused: start again from its word */
setInterval(function(){if(!G.on||G.paused||!G.utter||!G.lastWordAt||!(G.hl&&G.hl.seen))return;if(performance.now()-G.lastWordAt>4000&&window.speechSynthesis&&!speechSynthesis.paused){G.lastWordAt=performance.now();speakFrom(G.hl&&G.hl.k>=0?G.hl.k:0,true)}},1000);

/* ----- the introduction: on the first tour (or every tour), Azu walks in and says who it is and how to ask ----- */
on("runStep.before",function(){if(G.introPending&&G.i===0){G.introPending=false;S.guideSteps.splice(0,0,{id:"intro",view:"card",ref:"hero",act:"intro",text:T(fmt(S.tx.guide_intro.en,pointName()),fmt((S.tx.guide_intro.pt||S.tx.guide_intro.en),pointName()))});G.introNow=true}
  G.tryKind=null;G.tryDone=false;G.tryWait=false;clearTimeout(G.insistT)});
on("runStep.after",function(){
  var st=S.guideSteps[G.i];if(st&&st.act==="intro"){var m=ph.querySelector(".mascot");if(m&&!S.reduce){m.classList.add("enter");setTimeout(function(){m.classList.remove("enter")},1400)}}
  if(st&&st.act==="try")G.tryKind=st.wait;placeStep(st)});
on("startTour.before",function(){G.introPending=!S.howSaved&&(S.guideCfg.intro==="every"||(S.guideCfg.intro==="first"&&!S.introSeen));if(G.introPending)S.introSeen=true});

/* ----- the block being told, placed where the guide panel never covers it: a short block at a third of the free height, a tall one near the top ----- */
function placeStep(st){if(G.keepScroll||!st||st.act==="intro")return;var el=stepEl(st);if(!el)return;setTimeout(function(){if(!el.isConnected||!G.on)return;var f=freeHeight(),r=el.getBoundingClientRect(),avail=f.bottom-f.top,q=ph.getBoundingClientRect().height/ph.offsetHeight;
    var want=r.height<=avail*.45?f.top+(avail-r.height)/3:f.top+16,dy=(r.top-want);if(st.ref==="hero")dy=-scr.scrollTop;
    if(Math.abs(dy)>4){G.autoScrollTill=performance.now()+900;scr.scrollTo({top:scr.scrollTop+dy/q,behavior:S.reduce?"auto":"smooth"})}},60)}

/* ----- a scroll by the visitor: the guide keeps talking; if its block leaves the screen, it moves on to the block now in view ----- */
ph.addEventListener("wheel",function(){G.userScrollAt=performance.now()},{capture:true,passive:true});
ph.addEventListener("touchmove",function(){G.userScrollAt=performance.now()},{capture:true,passive:true});
on("pauseTour.answer",function(){if(S.guideCfg.onScroll==="follow"&&G.userScrollAt&&performance.now()-G.userScrollAt<60&&!G.explicitPause)return true});
setInterval(function(){if(!G.on||G.paused||G.askOpen||G.moment||S.guideCfg.onScroll!=="follow")return;var st=S.guideSteps[G.i];if(!st||st.act==="intro"||st.act==="end")return;
  if(!G.userScrollAt||performance.now()-G.userScrollAt>1500||performance.now()<(G.autoScrollTill||0))return;
  var el=stepEl(st);if(!el)return;var f=freeHeight(),r=el.getBoundingClientRect();if(r.bottom>f.top+8&&r.top<f.bottom-8){G.goneSince=0;return}
  if(!G.goneSince){G.goneSince=performance.now();return}if(performance.now()-G.goneSince<700)return;G.goneSince=0;
  /* the step whose block is now most in view */
  var best=-1,bv=0;S.guideSteps.forEach(function(s,j){if(j===G.i||s.act==="intro"||s.act==="end"||s.view!==S.view)return;var e=stepEl(s);if(!e)return;var q=e.getBoundingClientRect(),v=Math.max(0,Math.min(q.bottom,f.bottom)-Math.max(q.top,f.top));if(v>bv){bv=v;best=j}});
  stopSpeech();G.waiting=false;G.tryWait=false;clearTimeout(G.insistT);
  if(best<0){pauseNow();return}
  G.i=best;var t=TXT("guide_moving_on");renderGuide(t);say(t,function(k){markWord(k,null)},function(){if(G.on&&!G.paused){G.noPlace=true;runStepNoScroll()}})},300);
function runStepNoScroll(){var k=G.keepScroll;G.keepScroll=true;try{runStep()}finally{G.keepScroll=k}}
/* the explicit pause button is always a pause */
ph.addEventListener("click",function(e){var b=e.target.closest('[data-g="pause"]');if(b){G.explicitPause=true;setTimeout(function(){G.explicitPause=false},0)}},true);

/* ----- an invitation to try: never blocking. Done while the guide still speaks: it stops and reacts. Not done: it waits, insists
   gently (Insists times, a new line each time), then moves on. Done: it says what was revealed (a flipped card's back, read in place) ----- */
var nudgeN={};
setInterval(function(){if(!G.on||G.paused||!G.waiting||G.tryWait)return;var st=S.guideSteps[G.i];if(!st||st.act!=="try")return;clearTimeout(G.timer);G.timer=null;G.waiting=false;G.tryWait=true;
  if(G.tryDone){afterTry(st);return}nudgeN[st.id]=0;insistLater(st)},120);
function insistLater(st){clearTimeout(G.insistT);G.insistT=setTimeout(function(){if(!G.on||G.paused||!G.tryWait||S.guideSteps[G.i]!==st)return;var n=nudgeN[st.id]=(nudgeN[st.id]||0)+1;
  var t=n>S.guideCfg.insist?TXT("guide_try_moveon"):TXT("guide_nudge_"+st.wait+"_"+Math.min(2,n));renderGuide(t);var m=ph.querySelector(".mascot");if(m&&n<=S.guideCfg.insist){m.classList.remove("r-jump");void m.offsetWidth;m.classList.add("r-jump");setTimeout(function(){m.classList.remove("r-jump")},700)}
  say(t,function(k){markWord(k,null)},function(){if(!G.on||!G.tryWait||S.guideSteps[G.i]!==st)return;if(n>S.guideCfg.insist){G.tryWait=false;setTimeout(function(){if(G.on&&!G.paused)next()},500)}else insistLater(st)})},S.guideCfg.insistAfter*1000)}
function gesture(kind){if(!G.on)return;var st=S.guideSteps[G.i];if(!st||st.act!=="try"||st.wait!==kind||G.tryDone)return;G.tryDone=true;feedback("right");
  if(G.tryWait||G.waiting){clearTimeout(G.timer);G.waiting=false;G.tryWait=true;clearTimeout(G.insistT);stopSpeech();setTimeout(function(){afterTry(st)},kind==="flip"?650:350);return}
  /* done while the guide is still asking: it stops and reacts */
  if(G.utter||G.timer){stopSpeech();G.tryWait=true;setTimeout(function(){afterTry(st)},kind==="flip"?650:350)}}
function afterTry(st){if(!G.on||S.guideSteps[G.i]!==st)return;clearTimeout(G.insistT);G.tryWait=true;var el=stepEl(st),target=null,t="";
  var done=function(){G.tryWait=false;if(G.on&&!G.paused)setTimeout(next,700)};
  /* a flip: "There it is", then the back is read in place, its words lighting up on the card */
  if(st.wait==="flip"&&!(st.after&&L(st.after))){var back=el&&el.querySelector(".bc p")||el&&el.querySelector(".bc"),lead=TXT("guide_after_flip");
    renderGuide(lead);say(lead,function(k){markWord(k,null)},function(){if(!G.on||S.guideSteps[G.i]!==st)return;if(!back){done();return}var bt=back.textContent.trim();wrapWords(back);renderGuide(bt);say(bt,function(k){markWord(k,back)},done)});return}
  t=st.after&&L(st.after)?L(st.after):TXT(st.wait==="answer"?"guide_after_answer":"guide_after_flip");renderGuide(t);say(t,function(k){markWord(k,null)},done)}

/* Editor: Invite to try gets Insists and Waits; the tour gets What a scroll does; the mascot gets the Introduction; the Guide script gets each step's After words */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7coach",row("Insists",pseg("guideCfg.insist",["0","1","2"],["Never","Once","Twice"]),"When the visitor has not tried yet, the guide asks again, each time with new words, then says it is fine and moves on. It never blocks the tour. Done at any moment, even while the guide is still asking, it carries on at once and says what was revealed (a flipped card's back, read in place).")+
    row("Waits",pslider("guideCfg.insistAfter",3,12,1),"Seconds before each new ask.",true));
  addRows(d,"s7resume",row("When the visitor scrolls",pseg("guideCfg.onScroll",["follow","pause"],["Keeps going","Pauses"]),"Keeps going: the guide talks on while its block is on the screen; when the block leaves it, the guide says “Moving on” and tells the block now in view (or pauses when none is). Pauses: any scroll pauses and offers Resume."));
  addRows(d,"s7mascot",row("Introduction",pseg("guideCfg.intro",["first","every","off"],["First tour","Every tour","Off"]),"Azu walks in and says who it is and how to ask, before the first step."));
  return d.innerHTML});
on("edMarker.html",function(h){var d=document.createElement("div");d.innerHTML=h;var st=S.sel.gstep&&byId(S.fullSteps||S.guideSteps,S.sel.gstep);if(!st||st.act!=="try")return d.innerHTML;
  var slot=d.querySelector('[data-pick="gstep"][data-id="'+cssq(st.id)+'"] + tr.slot td:last-child');if(!slot)return d.innerHTML;var a=st.after||{en:"",pt:""};
  slot.insertAdjacentHTML("beforeend",row("After the gesture · EN",'<textarea class="u" rows="2" data-gafter="'+st.id+'|en" placeholder="'+(st.wait==="flip"?"(reads the card’s back)":"")+'">'+esc(a.en||"")+'</textarea>',"What the guide says once the visitor has done it: what was revealed.")+
    row("After the gesture · PT",'<textarea class="u" rows="2" data-gafter="'+st.id+'|pt">'+esc(a.pt||"")+'</textarea>',""));return d.innerHTML});
ed.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.gafter)return;e.stopImmediatePropagation();var q=t.dataset.gafter.split("|"),st=byId(S.fullSteps||S.guideSteps,q[0]);st.after=st.after||{en:"",pt:""};st.after[q[1]]=t.value},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(intro|insist|insistAfter|onScroll|"));
