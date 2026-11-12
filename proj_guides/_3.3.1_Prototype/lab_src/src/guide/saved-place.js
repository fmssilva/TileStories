/* The saved place (guidance round 17; the developer's plan, section 5, 2026-11-11): a tour resumes on the next visit where the visitor
   stopped. While a tour plays, each step it reaches is kept for its point (the step, the tour's source, the time): a tour left before its
   end (Stop, the card closed, another point, the app left) leaves its place; its end clears it, and so does a start from the start. On the
   next open of that point (a reload is the next visit) the hero's button, the ✦ menu and A tour of {point}? offer "Continue the tour (step 5
   of 13)" beside "Start again"; Continue says one line first, "Where we were: …" (the step before's own words: no new fact), then the step.
   A saved place expires (Forget after: 7 days, Never, 1 day, 30 days; the clock is Preview & Test › Today is, else the phone's); a tour
   changed since (its source, written or from the card, or the step gone from it) starts again and Azu says so once. A place kept in the
   storage is offered on the next visit only (G.svVisit, the page's load). The Lab keeps it in this browser (localStorage, every access
   wrapped: with storage blocked nothing is kept for the next visit, the app works on, and the Editor says so).
   Round 17b (the developer, 2026-11-12): ONE way back into a stopped tour. Round 11's within-visit Resume (a Stop's place: the step and the
   sentence, guide/controls.js's G.resumeAt) and the next visit's place are one offer with the same words everywhere, "Continue the tour
   (step 5 of 13)" and "Start again": the Stop toast, the ✦ menu, the hero's action row and A tour of {point}?. Within the visit Continue
   goes back to the sentence (no recap: the visitor just heard it); on a later visit it says "Where we were: …" first.
   Unity: PlayerPrefs (or the save file) per marker, "TourPlace.<markerId>" = {step, source, time} as JSON; GuideTourPlace.Offer(marker)
   read by the hero's action row, the ✦ menu and GuidanceService's invitation; GuideTourPlayer.ContinueAt(step) with the recap line */
S.guideCfg.svOn=true;S.guideCfg.svForget="7";
var SV_KEY="tilestories.tourPlace.",SV_DAYS={"1":1,"7":7,"30":30,"never":0};G.svMem={};G.svBlocked=false;
/* this visit (the page's load): a place kept in the storage during it is offered on the next visit only; within the visit the offer is the
   Stop's own place (G.resumeAt: the step and the sentence), the same words (round 17b) */
G.svVisit=Date.now().toString(36)+Math.random().toString(36).slice(2,8);
function svPoi(){return S.m&&S.m.poi||"painting"}
/* the browser's storage, or none (blocked: private mode, a policy, a full disk): the place then lives in memory for the page's life */
function svStore(){if(G.svS!==undefined)return G.svS;try{var s=window.localStorage,k=SV_KEY+"probe";s.setItem(k,"1");s.removeItem(k);G.svBlocked=false;G.svS=s}catch(e){G.svBlocked=true;G.svS=null}return G.svS}
function svRead(poi){var s=svStore(),v=null;if(s){try{v=s.getItem(SV_KEY+poi)}catch(e){v=null}}else v=G.svMem[poi]||null;try{return v?JSON.parse(v):null}catch(e){return null}}
function svWrite(poi,o){var v=JSON.stringify(o),s=svStore();if(!s){G.svMem[poi]=v;return}try{s.setItem(SV_KEY+poi,v)}catch(e){G.svBlocked=true;G.svMem[poi]=v}}
function svDrop(poi){delete G.svMem[poi];var s=svStore();if(!s)return;try{s.removeItem(SV_KEY+poi)}catch(e){}}
/* now, by the app's clock: Preview & Test › Today is (that day, at the phone's time of day), else the phone's */
function svNow(){var n=Date.now(),d=S.ui&&S.ui.gdToday;if(!d)return n;var p=String(d).split("-"),t=new Date(n);return new Date(+p[0],+p[1]-1,+p[2],t.getHours(),t.getMinutes(),t.getSeconds()).getTime()}
/* a point's tour as it is at rest (the open one's, or the one it would play), without its skipped steps */
function svTour(poi){poi=poi||svPoi();var st;if(poi===svPoi())st=S.fullSteps||S.guideSteps;else{var t=typeof atTour==="function"&&POINTS[poi]?atTour(poi):null;st=t&&t.src==="written"&&t.written?t.written:POINTS[poi]&&typeof atSteps==="function"?atSteps(poi):[]}
  return(st||[]).filter(function(s){return!s.skip&&!/^(intro|recap)$/.test(s.act)})}
function svSrc(poi){return typeof atSrc==="function"?atSrc(poi):"written"}
/* what is kept for a point: its step's place in the tour now, or why it is no longer offered (expired; changed: another source, the step gone) */
function svState(poi){poi=poi||svPoi();var o=svRead(poi);if(!o)return null;if(o.visit===G.svVisit)return{o:o,thisVisit:true,age:svNow()-(+o.t||0)};var d=SV_DAYS[S.guideCfg.svForget],age=svNow()-(+o.t||0);if(d&&age>d*864e5)return{o:o,expired:true,age:age};
  var st=svTour(poi),j=st.findIndex(function(s){return s.id===o.id});return{o:o,j:j,of:st.length,age:age,changed:o.src!==svSrc(poi)||j<1}}
/* the one offer (round 17b): a Stop's place in this visit first (its step and sentence; within: true), else a place kept from an earlier visit;
   null when nothing is offered. Its step's title for the ✦ menu's second line */
function svOffer(poi){poi=poi||svPoi();var r=G.resumeAt,st;
  if(r&&(!r.poi||r.poi===poi)){st=svTour(poi);var j=st.findIndex(function(s){return s.id===r.id});return{poi:poi,id:r.id,n:Math.max(1,j+1),of:Math.max(1,st.length),within:true,title:r.title||""}}
  if(!S.guideCfg.svOn)return null;var s=svState(poi);if(!s||s.thisVisit||s.expired||s.changed)return null;st=svTour(poi);return{poi:poi,id:s.o.id,n:s.j+1,of:s.of,title:stepTitle(st[s.j])||""}}
function svLabel(of){return fmt(TXT("sv_continue"),of.n,of.of)}
/* the recap: "Where we were: …", the first sentence of five words or more of the step before (its intro line left out; the teaser's hook
   without the name): words the visitor heard, no new fact */
function svWordsOf(s,l){var tx=s.text?wordIn(s.text,l):(function(){var r=byId(S.story,String(s.ref||"").slice(2));return r&&r.c?wordIn(r.c.text,l):""})(),lk=s.at&&s.at.links&&s.at.links[l];
  if(lk&&tx.indexOf(lk)===0)tx=tx.slice(lk.length).trim();if(s.act==="teaser"){tx=atSentences(tx)[0]||"";var c=tx.indexOf(": ");if(c>0&&c<60)tx=tx.slice(c+2)}return tx}
function svRecapOf(poi,id){var st=svTour(poi),j=st.findIndex(function(s){return s.id===id}),out={};
  langCodes().forEach(function(l){var t="";for(var i=j-1;i>=0&&!t;i--)t=atSentences(svWordsOf(st[i],l)).filter(function(x){return atWords(x)>=5})[0]||"";
    out[l]=t?fmt(wordIn(S.tx.sv_recap,l),atLowerFirst(t.replace(/[.!?…]+$/,""),l)):""});return out}
/* Continue: within the visit back to the step and the sentence (guide/controls.js's resumeTourAt); on a later visit the recap, then the step.
   Start again: the tour from its start (startTour(): the place cleared, this visit's and the kept one) */
function svContinue(poi){poi=poi||svPoi();var of=svOffer(poi);if(!of){startTour();return}if(of.within){resumeTourAt();return}G.svRecap={id:of.id,text:svRecapOf(poi,of.id)};if(G.on)stopTour();if(typeof hideStopUi==="function")hideStopUi();G.resumeAt=null;
  startTour({id:of.id})}
/* ----- the tour: a step reached is kept; the end clears; a start from the start clears (and says once that the tour changed) ----- */
on("startTour.before",function(from){G.svNoSave=!!(G.demoTour||G.atForce||S.howSaved);if(from||G.svNoSave)return;var poi=svPoi(),s=svState(poi);
  if(s&&!s.thisVisit&&!s.expired&&s.changed&&S.guideCfg.svOn)G.svNotice=true;if(s)svDrop(poi)});
on("startTour.after",function(){G.svRecap=null;G.svNotice=false});
on("runStep.before",function(){var st=G.on&&S.guideSteps[G.i];if(!st)return;var T2=function(k,f){var o={};langCodes().forEach(function(l){o[l]=f?f(l):wordIn(S.tx[k],l)});return o};
  if(G.svRecap&&st.id===G.svRecap.id){var r=G.svRecap;G.svRecap=null;if(String(L(r.text)||"").trim())S.guideSteps.splice(G.i,0,{id:"recap",view:st.view,ref:st.ref,act:"recap",text:T2(null,function(l){return r.text[l]||""})});return}
  if(G.svNotice&&G.i===0){G.svNotice=false;S.guideSteps.splice(0,0,{id:"svnote",view:"card",ref:"hero",act:"recap",text:T2("sv_changed")})}});
on("runStep.after",function(){if(!G.on||!S.guideCfg.svOn||G.svNoSave)return;var st=S.guideSteps[G.i],poi=svPoi();if(!st)return;
  if(st.act==="end"){svDrop(poi);return}if(/^(intro|recap|teaser)$/.test(st.act))return;svWrite(poi,{id:st.id,src:svSrc(poi),t:svNow(),v:1,visit:G.svVisit})});
/* ----- the ways in: the hero's action row (the ✦ menu: world/guide-button.js; the invitation: guidance/) ----- */
on("cardScreen.html",function(h){if(!S.guideCfg.heroButton||G.on||typeof inDemoCard!=="undefined"&&inDemoCard)return h;var of=svOffer();if(!of)return h;
  var d=document.createElement("div");d.innerHTML=h;var gs=d.querySelector(".hero .gstart");if(!gs)return h;gs.classList.add("gagain");gs.textContent=TXT("sv_again");
  var c=document.createElement("button");c.type="button";c.className="gcont";c.dataset.sv="cont";c.innerHTML='&#9654; '+esc(svLabel(of));gs.parentNode.insertBefore(c,gs);gs.parentNode.classList.add("svoff");return d.innerHTML});
ph.addEventListener("click",function(e){var b=e.target.closest('[data-sv="cont"]');if(!b)return;e.stopPropagation();if(S.view!=="card")S.view="card";svContinue()},true);

/* ----- the Editor: Guide › 2 · The tour › Ways in › Continue the tour (round 17b: one foldout for the one offer; its rows' keys kept) ----- */
function svAgo(ms){var m=Math.round(ms/60000);return m<1?"just now":m<60?m+" min ago":m<1440?Math.round(m/60)+" h ago":Math.round(m/1440)+" days ago"}
/* Kept now: a Stop in this visit first (what is offered now), then what this browser keeps for the next visit */
function svNowText(){var poi=svPoi(),w=G.resumeAt&&(!G.resumeAt.poi||G.resumeAt.poi===poi);
  return w?"This visit: the Stop at “"+(G.resumeAt.title||G.resumeAt.id)+"”, offered now as “"+svLabel(svOffer(poi))+"” (back to its sentence). For the next visit: "+svNextText(poi):svNextText(poi)}
function svNextText(poi){var s=svState(poi),nm=nameMid(L(pointOf(poi).hero.title)||poi,"en"),bl=G.svBlocked||!svStore()?" This browser blocks storage: no place can be kept for the next visit.":"";
  if(!S.guideCfg.svOn)return"Off: nothing is kept for the next visit (within the visit a Stop is still offered)."+bl;if(!s)return"Nothing kept for "+nm+"."+bl;
  if(s.thisVisit)return nm.charAt(0).toUpperCase()+nm.slice(1)+": a place kept in this visit, "+svAgo(s.age)+": offered on the next visit (a reload)."+bl;
  if(s.expired)return"A place at "+nm+", "+svAgo(s.age)+": forgotten (older than "+S.guideCfg.svForget+" day"+(S.guideCfg.svForget==="1"?"":"s")+"); the next tour starts from its start."+bl;
  if(s.changed)return"A place at "+nm+", "+svAgo(s.age)+", in a tour that has changed since ("+(s.o.src!==svSrc(poi)?"another source":"its step is gone")+"): the next tour starts again and says so once."+bl;
  return nm.charAt(0).toUpperCase()+nm.slice(1)+": step "+(s.j+1)+" of "+s.of+", "+svAgo(s.age)+"; offered as “"+svLabel({n:s.j+1,of:s.of})+"”."+bl}
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7resume",'<div class="note svone"><b>One way back into a stopped tour.</b> The same offer everywhere: “'+esc(fmt(TXT("sv_continue"),5,13))+'” and “'+esc(TXT("sv_again"))+'”, on the Stop toast, the ✦ menu, the hero\'s action row and A tour of {point}?. Within the visit, Continue goes back to the step and the sentence the Stop left. On a later visit, Azu first says “Where we were: …” (the step before\'s own words), then the step. Start again, the tour\'s end or any start from the start clears it. Unity: GuideTourPlace.Offer, one source for the four places.</div>'+
    row("Remember where the visitor stopped",pseg("guideCfg.svOn",["true","false"],["On","Off"]),"On: the place is also kept for the next visit. Each step a tour reaches is kept for its point (the step, the tour's source, the time), so a tour left before its end (Stop, the card closed, another point, the app left) is offered on the next open of that point, with the recap first. A tour changed since (another written tour or the one from the card, or its step gone) starts again, and Azu says so once. Off: nothing kept for the next visit; within the visit a Stop is still offered (back to the sentence). Unity: PlayerPrefs (or the save file) per marker.")+
    row("Forget after",psel("guideCfg.svForget",[["7","7 days"],["never","Never"],["1","1 day"],["30","30 days"]]),"How long a saved place is offered (7 days); older, the tour starts from its start. The clock: Preview & Test › Today is, else the phone's.",true)+
    row("Kept now",'<span class="uses svnow">'+esc(svNowText())+'</span>'+(svRead(svPoi())||G.resumeAt?' <button class="del" type="button" data-svforget="1">Forget it</button>':""),"What is offered for the open point: a Stop in this visit, and what this browser keeps for the next visit, as a visitor's phone would. Forget it clears both.",true),"When the visitor scrolls");return d.innerHTML});
ed.addEventListener("click",function(e){var b=e.target.closest("[data-svforget]");if(!b)return;e.stopPropagation();svDrop(svPoi());G.resumeAt=null;renderAll(true)},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(svOn|svForget|"));
/* each row's demo: a place kept at step 5 of the open point's tour, the card at its hero with the offer (Off: the plain Start, a chip says why) */
function svDemo(){var poi=svPoi(),st=svTour(poi);if(G.on)stopTour();var s=st[Math.min(4,st.length-2)];if(S.guideCfg.svOn&&s)svWrite(poi,{id:s.id,src:svSrc(poi),t:svNow(),v:1,visit:"an earlier visit"});
  S.view="card";S.stop="full";renderPhone(false);scr.scrollTop=0;if(typeof gdChip==="function"){var of=svOffer(poi);gdChip(of?svLabel(of)+" · "+(S.guideCfg.svForget==="never"?"kept until the tour ends":"offered for "+S.guideCfg.svForget+" day"+(S.guideCfg.svForget==="1"?"":"s")):"Remember where the visitor stopped: Off");clearTimeout(G.svChipT);G.svChipT=setTimeout(function(){gdChip(null)},3200)}}
rowDemos({"guideCfg.svOn":{act:function(){svDemo()},ring:".hero .gcont|.hero .gstart|.hero"},"guideCfg.svForget":{act:function(){svDemo()},ring:".hero .gcont|.hero"}});
