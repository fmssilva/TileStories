/* Guidance, the service: the visit's clock, the visitor's memory, the events, and the loop that asks gdPick 4 times a second and hands
   its choice to guidance/present.js. The clock (G.gd.t, ms of the visit) runs at G.gd.rate (×1; the Editor's ×10 demos and the scenes
   run it faster); with G.gd.manual a test moves it by hand (gdAdvance), so every check is deterministic. The memory (S.gdVisitor,
   _3.8 section 3.4), kept on the phone only: per item, this visit's and the ever counts, the last shown, the line used, done and by
   what, dismissed, acted; per visit, its start, the time away, the tips started (at most n a minute), the ignored in a row and the
   breathing room's factor, the groups resting, whether the guide was heard; per visitor, the visits, met the guide (the tour's
   introduction heard), the features used. A visit ends after 30 min away (gdLeave: Preview & Test > The visitor > Leave for 31 min);
   New visit keeps the ever part, Reset the visitor clears both; with Remember across visits Off every visit starts as a first visit.
   A tip ends acted (a tap, or the feature it teaches used), dismissed (swiped away, or Fewer like this?), ignored (its time ran out)
   or by the moment (the wall left, its target gone): the learning (gdLearn) reads how. The events (_3.8 section 3.2), each from the
   module that knows it: visit started (gdVisitStart), the phone moving (phone.moved), a touch (the phone's pointer), card opened
   (card.opened), a tour step heard (tourStep.ended), a question asked (ask.after), Ask about the view opened (view.opened), the ✦
   menu opened (menu.opened), Azu poked (mascot.poked: counted this visit), idle (no touch, wheel or key and the phone still since,
   the visit's start at the earliest). A pull (gdPull: Tap on Azu = Points to the ✦) shows its item at once, past every gate but one at a
   time (the tip showing gives way). Unity: GuidanceService (a MonoBehaviour ticking at 4 Hz, fed by the event buses: the AR camera's
   motion, the touches, the card's open, the tour's steps, the mascot's poke, the ✦ menu), the memory as JSON in persistentDataPath, the
   visit clock Time.unscaledTime since the visit started, the time away from OnApplicationPause.
   Round 7 (Fun): the tour moving on to its next step asks the scheduler at that moment (next.answer, context.moment = "between"): a
   Moment it picks shows (Azu's prop, mascot/azu2d.js) and holds the next step until it ends (a tap on the prop: acted; 8 s: ignored;
   moment.ended); the context gives the tour's narration so far and whether its last step is next, today (Preview & Test › Today is) and a
   Solemn day, and whether the phone is on a Solemn point (Specific Marker › Solemn point: on its card, or its point in the middle);
   a treasure found (hunt.found, world/camera-ask.js) retires the hunt's invitation. The memory keeps the variants seen (A joke: the
   least seen shows) and the pokes at an item's last start (a joke every third poke). The simulator gains the Moments between a tour's
   steps, a scripted visitor's audience and Solemn points, and a sixth visitor, Family on a Solemn wall. */
var GD_VISIT_GAP=30*60000;
function gdVisitMem(n,now){return{n:n,at:now,lastEnd:null,cueEnd:null,cards:0,items:{},starts:[],away:0,ignoredRun:0,breathMul:1,rest:{},fewer:{},heard:false,pokes:0,ev:{},tips:[]}}
/* round 6: the visitor's own Tips (All / Essentials only / Off) and Tips stay until I tap them (WCAG 2.2.1), kept with the memory */
function gdNewMemory(now){return{visits:1,visit:gdVisitMem(1,now),ever:{items:{}},used:{},met:false,prefs:{tips:"all",stay:false,games:"on"}}}
S.gdVisitor=gdNewMemory(0);
G.gd={t:0,rate:1,manual:false,cur:null,movedAt:null,touchAt:null,actAt:null,cardAt:null,wallAt:null,onWall:false,demo:false,last:null,next:null,log:[],between:false,skipKey:"",plan:null,heard:{}};
function gdNow(){return G.gd.t}
/* the log: every decision (shown, skipped, ended) and event, with the visit's time; the Editor shows the last 30 */
function gdLog(what,why){var v=S.gdVisitor.visit,t=Math.round(gdNow());G.gd.log.push({t:t,vt:t-Math.round(v.at||0),v:v.n,what:what,why:why||""});if(G.gd.log.length>400)G.gd.log.shift();emit("gdLog.after")}
function gdVM(id,V){var v=(V||S.gdVisitor).visit.items;return v[id]||(v[id]={shown:0,last:null,end:null,dismissed:0,acted:0,how:"",line:""})}
function gdEM(id,V){var v=(V||S.gdVisitor).ever.items;return v[id]||(v[id]={shown:0,done:false,by:"",last:null,line:"",dismissed:0,acted:0})}
/* a start and an end in a visitor's memory (the service's, or the simulator's own: one bookkeeping for both). A cue: its last and the
   card it showed on (the Locate label counts once it has been seen); a tip: shown, last, the line used, and its start (at most n a minute) */
function gdMemStart(V,it,now,text,key,answer){var vm=gdVM(it.id,V),em=gdEM(it.id,V);vm.last=now;vm.end=null;em.last=now;vm.card=V.visit.cards||0;
  if(it.form==="cue"){if(!it.untilSeen){vm.shown++;em.shown++}return}
  /* round 7: the pokes at its start (pokes are counted since it last showed) and the variant said (the least seen shows next) */
  vm.pk=V.visit.pokes||0;if(key){var sn=em.seen||(em.seen={});sn[key]=(sn[key]||0)+1}
  vm.shown++;vm.line=text;em.shown++;em.line=text;if(!answer&&!it.onTour)V.visit.starts.push(now)}
/* a cue's end writes the cue room's clock (never a tip's end or learning); a tip's end, how it ended and the learning */
function gdMemEnd(V,c,how,now,log){var vm=gdVM(c.item.id,V);vm.end=now;vm.how=how;
  if(c.cue){V.visit.cueEnd=now;if(c.item.untilSeen&&how==="seen"){vm.shown++;gdEM(c.item.id,V).shown++}return}
  /* round 7: a joke that answered pokes neither starts the breathing room nor counts as ignored */
  if(c.answer){if(how==="acted"||how==="dismissed"||how==="fewer")gdLearn(c,how,now,V,log);return}
  /* round 12: a beat of the tour ends inside the tour's rhythm: no breathing room, no learning of the visit's tips */
  if(c.item.onTour){if(how==="acted"){vm.acted++;gdEM(c.item.id,V).acted++}return}
  V.visit.lastEnd=now;gdLearn(c,how,now,V,log)}
/* a feature used: the items it retires are done for good (the items given, in the Editor's order) */
function gdMemUsed(V,f,now,items){V.used[f]=now;items.forEach(function(it){if(it.doneWhen===f){var em=gdEM(it.id,V);if(!em.done){em.done=true;em.by=f}}})}
/* a visit starts: a new visitor (Reset the visitor) or the same one back (New visit; a return after 30 min away); the clock goes on.
   With Remember across visits Off, the one back is a first visit again */
function gdVisitStart(returning,why){if(G.gd.pages)gdPagesEnd("a new visit",false);if(G.gd.cur)gdStop("a new visit");var now=gdNow(),keep=returning&&S.guidance.remember!==false;
  if(keep){S.gdVisitor.visits++;S.gdVisitor.visit=gdVisitMem(S.gdVisitor.visits,now)}else S.gdVisitor=gdNewMemory(now);
  G.gd.wallAt=G.gd.onWall?now:null;G.gd.skipKey="";
  gdLog("visit",keep?"the visitor came back (visit "+S.gdVisitor.visits+")"+(why?", "+why:""):returning?"a first visit again (Remember across visits is Off)"+(why?", "+why:""):"a first visit")}
function gdNewVisit(){gdVisitStart(true)}
function gdResetVisitor(){gdVisitStart(false)}
/* the visitor leaves the app for ms (the visit's clock moves on): 30 min or more away ends the visit, and coming back starts the next */
function gdLeave(ms){if(G.gd.cur)gdStop("the visitor left the app");var mins=Math.round(ms/60000);G.gd.t+=ms;
  if(ms>=GD_VISIT_GAP){gdLog("away","away "+mins+" min: the visit ended (a visit ends after 30 min away)");gdVisitStart(true,"after "+mins+" min away")}
  else{S.gdVisitor.visit.away=(S.gdVisitor.visit.away||0)+ms;G.gd.actAt=G.gd.t;gdLog("away","away "+mins+" min: the same visit goes on")}}
/* the nearest marker with a card on the screen (its circle's centre, in the phone's pixels), clear of the screen's edges and foot */
function gdMarker(){if(S.view!=="card"||typeof wallMarkers!=="function")return null;var W=ph.offsetWidth,H=ph.offsetHeight,best=null;
  wallMarkers().forEach(function(m){if(!m.card)return;var s=poiScreen(m);if(s.x<40||s.x>W-40||s.y<90||s.y>H-170)return;var d=Math.hypot(s.x-W/2,s.y-H*.45);
    if(!best||d<best.d)best={id:m.id,name:L(m.name),x:s.x,y:s.y,d:d,middle:gdInMiddle(s.x,s.y)}});return best}
/* the middle of the screen: the centre cell of a 3 × 3 grid (Ask about the view's own grid) */
function gdInMiddle(x,y){var W=ph.offsetWidth,H=ph.offsetHeight;return x>=W/3&&x<=2*W/3&&y>=H/3&&y<=2*H/3}
/* the ✦ wherever it shows outside a tour (on the wall the way into Ask about the view; on the card past the hero), and the point in the
   middle with its tour (a point with a card has one) */
function gdGbtn(){if(S.view!=="card"||G.on)return null;var g=ph.querySelector(".gbtn-wrap:not(.intour) .gbtn");if(!g||!g.getClientRects().length)return null;var B=gdPR(g);return{name:"the ✦",x:(B.l+B.r)/2,y:(B.t+B.b)/2,middle:false}}
function gdCentral(){if(S.view!=="card"||S.stop!=="closed"||typeof camView!=="function")return null;var c=camView().central,W=ph.offsetWidth,H=ph.offsetHeight;
  if(!c||c.x<40||c.x>W-40||c.y<90||c.y>H-170)return null;return{id:c.id,name:c.name,x:c.x,y:c.y,middle:c.where==="middle",tour:!!c.card,min:c.card?tourMinutes(c.id):null}}
/* the moment, as the scheduler reads it */
function gdOnWall(){return S.view==="card"&&S.stop==="closed"&&!G.on}
/* on a card: the sheet at Half or Full, or one of its pages (never the wall, Peek or the Demo Components page): where the cues live */
function gdOnCard(){return S.view==="card"?S.stop==="half"||S.stop==="full":S.view!=="demo"}
/* the context's numbers (the page's moment and the simulator's both start from them): the breathing room and the limits in force
   (Talkativeness: the row's value × its factor), the cue room, freshness, the learning, Lively, Solemn, Reduce Motion */
function gdCtxBase(){var g=S.guidance,md=gdTodayMD();return{on:!!g.on,breathing:gdRoomNow(),cueRoom:+g.cueRoom||0,reduce:!!S.reduce,limits:{perMinute:gdLimitNow(g.perMinute),perVisit:gdLimitNow(g.perVisit)},tau:+g.freshTau,
  learn:{ignore:g.learnIgnore!==false,dismiss:g.learnDismiss!==false},lively:S.guideCfg.life==="lively",solemn:!!(S.m&&S.m.solemn),aud:typeof aud==="function"?aud():"",
  today:md,solemnDay:gdSolemnDay(md)?gdDayWords(md):null}}
/* round 7: on a Solemn point now: Specific Marker › Solemn point marks the marker's point (the Ribeira Palace): on its card, in a tour, or on
   the wall with that point in the middle (or no point in the middle); another point in the middle of the wall is not it */
function gdSolemnNow(){if(!(S.m&&S.m.solemn))return false;if(S.view==="card"&&S.stop==="closed"&&!G.on){var c=gdCentral();return!c||c.id===(S.m.poi||"painting")}return true}
function gdContext(){var t=-Infinity,mv=G.gd.movedAt,wa=G.gd.wallAt,st=ph.querySelector(".stv"),sk=st&&(st.className.match(/\bst-(\w+)/)||[])[1];
  var speaking=!!(G.utter||G.timer||G.recAudio);
  var onCard=gdOnCard(),mo=G.gd.moment||null;
  return Object.assign(gdCtxBase(),{scrollAt:G.gd.scrollAt,onCard:onCard,cues:onCard?cueTargets(mo==="open"):{},moment:mo,
    demo:!!G.gd.demo,showing:G.gd.cur?G.gd.cur.item.label:gdTryPlaying()?"Show me ("+gdTryWord(gdTryItem(G.gd.tryShow.id)||{teaches:""})+")":null,
    remark:ph.querySelector(".azbub")?"The mascot's bubble":null,
    pokes:S.gdVisitor.visit.pokes||0,idleSince:Math.max(G.gd.actAt==null?t:G.gd.actAt,mv==null?t:mv,S.gdVisitor.visit.at||0),
    speaking:speaking,state:sk?GD_STATES[sk]||sk:null,
    panel:S.settingsOpen?"Settings":G.cam&&G.cam.open?"Ask about the view":G.fab&&G.fab.menu?"the ✦ menu":S.contentsOpen?"Contents":G.gd.tryOpen?"Things to try":null,
    dockFull:!!(G.on&&typeof dockOn==="function"&&dockOn()&&S.dockH==="full"),solemn:gdSolemnNow(),
    /* the tour: between two steps (as it moves on, nothing is said), its narration so far (since it started or the last Moment) and its last step next */
    plan:G.gd.plan,games:gdPrefs().games||"on",notNow:+S.gdVisitor.visit.notNow||0,sr:!!(S.ui&&S.ui.gdSR),lang:S.lang||"en",findOff:S.tourFind&&!gdFindOnWall(S.tourFind)?"the wall only: the card's point is not on the wall in view":null,
    tour:G.on?{paused:!!G.paused,between:mo==="between"||!!G.gd.between&&!speaking,step:G.i,stepId:S.guideSteps[G.i]&&S.guideSteps[G.i].id,talked:Math.max(0,(performance.now()-(G.talkFrom||performance.now()))/1000),toEnd:!!(S.guideSteps[G.i+1]&&S.guideSteps[G.i+1].act==="end")}:null,
    movedAt:mv,touchAt:G.gd.touchAt,cardAt:S.view==="card"&&S.stop!=="closed"?G.gd.cardAt:null,
    onWall:gdOnWall(),where:S.view!=="card"?"the phone shows "+S.view:G.on?"the tour":S.stop!=="closed"?"a card is open":"",
    marker:gdMarker(),gbtn:gdGbtn(),central:gdCentral(),covers:{},stillSince:Math.max(mv==null?t:mv,wa==null?t:wa),returning:S.gdVisitor.visits>1,tips:gdPrefs().tips,
    /* round 8: the first pages showing (the app's start): the scheduler is quiet */
    pages:G.gd.pages?(G.gd.pages.preview?"a page shown by the Editor":"the first pages"):null})}
/* the visitor's Tips settings (an older memory has none: the defaults) */
function gdPrefs(V){V=V||S.gdVisitor;var p=V.prefs||(V.prefs={tips:"all",stay:false});if(p.games==null)p.games="on";return p}
/* how long a tip stays: by its words, 4 to 9 s (the visit's clock); with the clock fast (a scene, a demo) it stays at least 0.6 to 1.5 s of real time, to be seen */
function gdOnScreen(it,text){if(it.onScreen==="secs"&&+it.onScreenS>0)return +it.onScreenS*1000;var n=String(text||"").split(/\s+/).filter(Boolean).length;return Math.max(4000,Math.min(9000,3000+n*350))}
/* the slots of its line: the point it is about and the minutes of that point's tour */
/* round 6: {year} the point's year; a line with variants says the next in turn (n: how many times it has shown, so never the same twice in a row) */
function gdYearOf(id){var p=(S.wallPois||[]).filter(function(x){return x.id===id})[0];return p&&p.year||""}
/* round 7: every line's slots also carry today (a seasonal line), the variants this visitor has seen (A joke: the least seen) and {thing}
   (the point the treasure hunt asks for: Ask about the view's own pick, camHuntPick) */
function gdSlots(it,V,c,items){V=V||S.gdVisitor;var o=gdSlotsOf(it,V,c,items);o.today=gdTodayMD();
  /* round 8: the visit's first words right after the first pages: Straight to the wall's short line (when that page is On) */
  if(it.when==="visit"&&V.visit&&V.visit.pages){var pw=gdPageOf("wall");if(pw&&pw.on&&gdHasWords(pw.says))o.pagesLine=pw.says}if(it.pick==="least")o.seen=gdEverOf(V,it.id).seen||{};if(it.tap==="hunt"||it.button==="hunt")o.thing=gdHuntThing();
  /* round 17: a place kept from an earlier visit of the point (guide/saved-place.js): the invitation offers to continue, at its step */
  if(it.tap==="tour"&&o.id&&typeof svOffer==="function"){var sv=svOffer(o.id);if(sv){o.cont=sv.n;o.of=sv.of}}return o}
/* the points in view as Ask about the view sees them once it is open (its panel hides the foot of the wall) */
function gdViewOpen(){var k=G.cam.open;G.cam.open=true;try{return camView().pois}finally{G.cam.open=k}}
/* the hunt asks for a point in view (Ask about the view's own list), so the invitation names the same one; with no camera (the simulator) the wall's points */
function gdHuntThing(){var P=S.view==="card"&&typeof camView==="function"?gdViewOpen():(S.wallPois||[]).map(function(p){return{id:p.id,name:L(p.name)}}),h=typeof camHuntPick==="function"&&P.length?camHuntPick(P):null;return h?h.name:""}
function gdSlotsOf(it,V,c,items){if(it.ptJokes)return{id:it.point,point:it.pointName||"",n:gdEverOf(V,it.id).shown||0,jokes:it.jokes,year:gdYearOf(it.point)};
  if(it.about==="point"){var n=(items||gdItems()).filter(function(o){return o.group===it.group}).reduce(function(a,o){return a+gdVisitOf(V,o.id).shown},0);
    return{id:it.point,point:it.pointName||"",card:!!it.card,n:n,year:gdYearOf(it.point)}}
  var nn=gdEverOf(V,it.id).shown||0;
  if(it.about==="marker"){var mk=c===undefined&&S.view==="card"?gdMarker():null;return mk?{id:mk.id,point:mk.name,year:gdYearOf(mk.id),n:nn}:{n:nn}}
  if(it.about!=="middle")return{n:nn};if(c===undefined)c=gdCentral();if(c&&c.tour)return{id:c.id,point:c.name,min:c.min,year:gdYearOf(c.id),n:nn};
  var p=(S.wallPois||[]).filter(function(x){return x.card})[0];return{id:p&&p.id,point:p?L(p.name):"",min:tourMinutes(p&&p.id),year:p?gdYearOf(p.id):"",n:nn}}
/* spoken (_3.8 decision 2): App as a bubble only; Content spoken once the visitor has heard the guide this visit, else a bubble */
/* round 6: Spoken per item (As the kind / A bubble only / Spoken once the guide is heard); never while a screen reader is on (Preview & Test >
   The visitor > Screen reader on, the Lab's stand-in for VoiceOver / TalkBack): the bubble is announced politely instead */
function gdSpoken(it,o){if(S.ui.gdSR)return false;if(o&&o.spoken!=null)return!!o.spoken;var h=!!S.gdVisitor.visit.heard;return it.spoken==="heard"?h:it.spoken==="kind"&&it.kindSpoken==="heard"&&h}
/* a tip starts (the scheduler's choice, or pulled: an Editor demo, which never writes the memory) and ends */
function gdStart(it,why,o){o=o||{};if(G.gd.cur)gdStop("another tip");if(it.form==="cue")return gdCueStart(it,why,o);var now=gdNow(),ret=o.ret!=null?!!o.ret:S.gdVisitor.visits>1&&!o.first,slots=gdSlots(it),key=o.text!=null?null:gdLineKey(it,ret,slots),text=o.text!=null?o.text:gdLine(it,ret,slots),spoken=gdSpoken(it,o);
  G.gd.cur={item:it,answer:!!o.answer,beat:o.beat||null,react:!!o.react,owed:o.owed||null,at:now,until:now+(o.ms||gdOnScreen(it,text)),realMin:G.gd.manual?0:performance.now()+(G.gd.rate>1?Math.max(600,15000/G.gd.rate):0),pull:!!o.pull,why:why||"",text:text,slots:slots,ret:ret,spoken:spoken};
  if(!o.pull){gdMemStart(S.gdVisitor,it,now,text,key,!!o.answer);gdSaid(it,text,now)}
  G.gd.skipKey="";gdLog("shown",(o.pull?"(pulled) ":"")+(why||it.label)+(spoken?" · spoken":""));gdPresent(G.gd.cur);if(spoken)gdVoice(G.gd.cur);return G.gd.cur}
/* how: "acted", "dismissed", "fewer", "ignored", or "" (the moment ended it); round 1's true is a dismissal */
/* a cue starts: its look on its element (card/hints.js's cueStart gives how long it plays; the Locate label: until seen); its memory
   (this visit, ever, the card it showed on) at its start, the label's once it has been seen; no bubble, no mascot, not a tip's start */
function gdCueStart(it,why,o){var now=gdNow(),c={item:it,at:now,until:now,realMin:0,pull:!!o.pull,why:why||"",text:it.says?TXT(it.says):"",slots:{},cue:true},d=cueStart(c);
  if(d==null){gdLog("skipped",it.label+": its part is not on the screen");return null}
  G.gd.cur=c;if(!o.pull){gdMemStart(S.gdVisitor,it,now,"");if(it.says)gdSaid(it,c.text,now)}if(gdGestureOf(it))gdFinger(it,null,cueEl(it.about));
  G.gd.skipKey="";gdLog("shown",(o.pull?"(pulled) ":"")+(why||it.label)+" · a cue");if(!isFinite(d)&&o.ms)d=o.ms;
  c.until=now+d;c.realMin=G.gd.manual||!isFinite(d)?0:performance.now()+d;return c}
function gdStop(why,how){var c=G.gd.cur;if(!c)return;if(how===true)how="dismissed";how=how||"";G.gd.cur=null;var now=gdNow();
  /* a cue ends: seen (it played, or the label was seen 4 s) or by the moment; it writes the cue's end (the cue room), never a tip's */
  if(c.cue){if(!c.pull)gdMemEnd(S.gdVisitor,c,how,now);
    gdLog("ended",c.item.label+": "+(why||"it played")+(how?" ("+how+")":""));cueStop(c,how);gdFingerStop();return}
  if(c.spoken&&G.sayLine&&G.sayLine.text===c.text)stopSpeech();
  if(!c.pull)gdMemEnd(S.gdVisitor,c,how,now);
  gdLog("ended",c.item.label+": "+(why||"its time is up")+(how?" ("+how+")":""));gdUnpresent(why)}
/* learning from the visitor (_3.8 section 3.3, 5): three ignored in a row double the breathing room for the visit (up to ×4), an acted
   item resets it; a dismissed App item waits ×3 its Again after (the scheduler reads its how); a dismissed Content item rests its group
   5 min; Fewer like this? rests the group for the visit. Essential items (the opening) are greetings: their time running out is not ignoring */
function gdLearn(c,how,now,V,log){V=V||S.gdVisitor;log=log||gdLog;var it=c.item,v=V.visit,vm=gdVM(it.id,V),em=gdEM(it.id,V),g=gdGroupOf(it),gn=g?g.name:it.group,br=gdRoomNow();
  if(how==="acted"){vm.acted++;em.acted++;if(v.breathMul>1)log("learned","an acted tip: the breathing room back to "+br+" s");v.ignoredRun=0;v.breathMul=1;return}
  if(how==="ignored"){if(it.essential)return;v.ignoredRun++;var m=S.guidance.learnIgnore!==false?Math.min(4,Math.pow(2,Math.floor(v.ignoredRun/3))):1;
    if(m!==v.breathMul){v.breathMul=m;log("learned",v.ignoredRun+" ignored in a row: the breathing room ×"+m+" ("+br*m+" s) for the visit")}return}
  if(how==="dismissed"||how==="fewer"){vm.dismissed++;em.dismissed++;
    if(how==="fewer"){v.rest[it.group]=GD_FOREVER;v.fewer[it.group]=true;log("learned","Fewer like this: "+gn+" rests for the visit");return}
    if(S.guidance.learnDismiss===false)return;
    if(it.afterDismiss==="visit"||it.afterDismiss==="never"){log("learned",it.label+" dismissed: "+(it.afterDismiss==="never"?"never again":"not again this visit"));return}
    if(it.kind==="app"){if(+it.again>0)log("learned",it.label+" dismissed: it waits ×3 its Again after ("+3*it.again+" s)");return}
    v.rest[it.group]=now+GD_REST_MS;log("learned",it.label+" dismissed: "+gn+" rests 5 min")}}
/* the events */
on("phone.moved",function(){G.gd.movedAt=gdNow()});
["pointerdown","pointerup"].forEach(function(t){ph.addEventListener(t,function(){G.gd.touchAt=G.gd.actAt=gdNow()},{capture:true,passive:true})});
/* idle: no touch, no wheel, no key (and the phone still: movedAt) */
["wheel","keydown"].forEach(function(t){ph.addEventListener(t,function(){G.gd.actAt=gdNow()},{capture:true,passive:true})});
/* Azu poked (mascot/azu2d.js), counted this visit; the ✦ menu opened (world/guide-button.js): The ✦ is done */
on("mascot.poked",function(){var v=S.gdVisitor.visit;v.pokes=(v.pokes||0)+1;(v.ev=v.ev||{}).poked=gdNow();gdLog("event",GD_EVENTS.poked+" ("+v.pokes+" this visit)")});
on("menu.opened",function(how){gdUsed("menu",how)});
/* an audience set (Who's exploring, the visitor's Settings, Preview as: audiences/audiences.js) writes its Guidance preset (round 5) */
on("audience.set",function(a,p){if(!p||!GD_PRESETS[p])return;gdPresetWrite(p);gdLog("event","the audience set: "+(AUDN[a]||a)+" (the "+gdPresetName(p)+" preset)")});
/* a feature used (a milestone of the stage): the items it retires are done for good; a tip that teaches it, showing now, was acted on */
function gdUsed(f,how){var now=gdNow(),c=G.gd.cur,vv=S.gdVisitor.visit;(vv.ev=vv.ev||{})[f]=now;gdMemUsed(S.gdVisitor,f,now,gdItems());
  gdLog("event",GD_EVENTS[f]+(how?" ("+how+")":""));if(c&&!c.pull&&c.item.teaches===f)gdStop(GD_EVENTS[f],"acted")}
/* a card opened: a tip it ends (the visitor did what it asked, or opened a card over it) is answered: the cue room does not count from it */
on("card.opened",function(how){var v=S.gdVisitor.visit,e0=v.lastEnd;G.gd.cardAt=gdNow();gdUsed("card",how);if(G.gd.cur&&!G.gd.cur.cue)gdStop("the visitor opened a card");if(v.lastEnd!==e0)v.openEnd=v.lastEnd});
on("view.opened",function(how){gdUsed("view",how)});
on("ask.after",function(){S.gdVisitor.visit.heard=true;gdUsed("ask")});
/* the tour: a step heard (the guide heard this visit; the introduction: met the guide); between two steps until the next one starts.
   A step of How to use the card (st.gd: its item) is not the tour's: its line heard is that step seen, which retires its cue */
on("tourStep.ended",function(st){S.gdVisitor.visit.heard=true;if(st&&st.id)G.gd.heard[st.id]=1;if(st&&st.act==="intro"&&!S.gdVisitor.met){S.gdVisitor.met=true;gdLog("event",GD_EVENTS.intro)}G.gd.between=true;
  if(st&&st.gd){gdStepSeen(st.gd);return}gdUsed("tour",st&&st.id)});
function gdStepSeen(id){var now=gdNow(),vm=gdVM(id),em=gdEM(id);vm.shown++;vm.last=now;em.shown++;em.last=now;gdUsed(id,"How to use the card")}
/* the card scrolled: the visitor acts (quiet 0.8 s after, as after a touch) */
scr.addEventListener("scroll",function(){G.gd.scrollAt=gdNow()},{passive:true});
/* a card opens (the sheet up from the wall, by any way): a new card for Every card, and the moment of the opening: once the opening's own
   events have run (card.opened ends a tip), the scheduler is asked at once with context.moment = "open" (only the cues that answer the
   opening, past the touch that opened it and the card's first 1.5 s) */
on("renderPhone.after",function(){if(!S.justOpened||noDraw)return;S.gdVisitor.visit.cards=(S.gdVisitor.visit.cards||0)+1;clearTimeout(G.gd.openT);G.gd.openT=setTimeout(gdOpening,0)});
function gdOpening(){if(noDraw||G.gd.demo||!gdOnCard())return;var c=G.gd.cur;if(c&&c.cue&&!c.pull)gdStop("a new card");if(G.gd.cur)return;
  G.gd.moment="open";var r;try{var ctx=gdContext();r=gdPick(gdItems(),S.gdVisitor,ctx,gdNow())}finally{G.gd.moment=null}
  G.gd.last=r;if(r.item)gdStart(r.item,r.why);else gdLog("skipped","the card opened: "+r.why)}
/* a cue on the screen now (the Locate Button asks for its label), and the label seen 4 s or the pin tapped (card/locate.js) */
function gdCueOn(id){var c=G.gd.cur;return!!(c&&c.cue&&c.item.id===id)}
function gdCueSeen(id){if(gdCueOn(id))gdStop("seen (4 s, or the pin tapped)","seen")}
/* How to use the card, pulled (Settings, the ✦ menu's How does this work?): past every gate and never capped, the tip showing gives way.
   On a card it walks the card (guidance/present.js's gdWalkCard, through the tour's steps); on the wall, one bubble says how the wall
   works and offers to open a card (gdHowWall). Its group Off: nothing (the entries are hidden too) */
function gdWalk(from){var g=gdGroupById("howto");if(!g||g.on===false)return false;if(G.gd.cur)gdStop("the visitor asked for How to use the card");
  if(S.view==="card"&&S.stop==="closed"&&!G.on){gdStart(gdHowWall(),"asked by the visitor ("+from+")",{pull:true,ms:9000});return true}
  gdLog("shown","(pulled) How to use the card ("+from+"): "+g.items.filter(function(id){return S.guidance.items[id].on}).length+" steps on the card");gdWalkCard();return true}
on("startTour.before",function(){G.gd.hold=false;if(G.gd.cur&&!G.gd.cur.pull)gdStop("the tour started")});on("runStep.before",function(){G.gd.between=false});on("stopTour.after",function(){G.gd.between=false});
/* ----- round 7: the tour moves on to its next step (guide/tour.js's next asks next.answer): the scheduler is asked at that moment
   (context.moment = "between": only a Moment); a Moment it picks shows and holds the next step until it ends (G.gd.hold: present.js's
   gdResume goes on); a Moment showing holds it too. The tour stopping ends a Moment (never "ignored": the moment ended it) ----- */
on("next.answer",function(){if(!G.on||G.paused||noDraw||G.gd.demo)return;var c=G.gd.cur;if(c&&(c.item.form==="moment"||c.item.onTour)){G.gd.hold=true;return true}if(c)return;
  /* round 12: a seam of the tour's plan (Fun › On the tour): its beat, or the guess's answer after the step that held it */
  if(gdSeam())return true;
  /* (the tour moving on ends the line it was saying: a Next pressed mid-line is not a sentence still being said) */
  G.gd.moment="between";var r;try{var ctx=gdContext();ctx.speaking=false;r=gdPick(gdItems(),S.gdVisitor,ctx,gdNow())}finally{G.gd.moment=null}
  G.gd.last=r;if(r.item&&r.item.form==="moment"){G.gd.hold=true;gdStart(r.item,r.why);return true}
  var key="between|"+r.why.replace(/\d+(\.\d+)?/g,"#");if(key!==G.gd.skipKey){G.gd.skipKey=key;gdLog("skipped","between two steps: "+r.why)}});
on("stopTour.before",function(){G.gd.hold=false;gdWordOff();var c=G.gd.cur;if(c&&(c.item.form==="moment"||c.item.onTour&&!c.pull))gdStop("the tour stopped")});
/* a Moment ends (mascot/azu2d.js): a tap on its prop is acted, 8 s with no tap ignored */
on("moment.ended",function(k,done){var c=G.gd.cur;if(c&&c.item.form==="moment")gdStop(done?"tapped: the mascot "+(k==="cookie"?"eats the cookie":"drinks")+", and the tour goes on":"8 s with no tap: the tour goes on",done?"acted":"ignored")});
/* a treasure found in Ask about the view's hunt (world/camera-ask.js): the hunt's invitation is done */
on("hunt.found",function(id){gdUsed("hunt","found")});
/* the loop: the wall seen since when, the tip on screen ends, or the scheduler is asked (a choice that would cover its target is asked again without it) */
function gdTick(){if(noDraw)return;var now=gdNow(),w=gdOnWall();if(w&&!G.gd.onWall)G.gd.wallAt=now;G.gd.onWall=w;
  var c=G.gd.cur;if(c){var e=gdEnds(c,now,{moment:!!G.moment,onWall:w,get onCard(){return gdOnCard()},get locate(){return!!cueTargets(false).locate},get marker(){return gdMarker()},get central(){return gdCentral()},get gbtn(){return gdGbtn()},realOk:performance.now()>=c.realMin,stay:!!gdPrefs().stay});
    if(e&&e!=="wait")gdStop(e[0],e[1]);return}
  if(G.gd.demo&&G.gd.demoOwner!==G.demoSeq){G.gd.demo=false;gdChip(null)}if(G.gd.demo)return;
  var ctx=gdContext(),items=gdItems(),r=gdPick(items,S.gdVisitor,ctx,now);
  for(var k=0;k<4&&r.item;k++){var cov=gdCovers(r.item);if(!cov)break;ctx.covers[r.item.id]=cov;r=gdPick(items,S.gdVisitor,ctx,now)}
  G.gd.last=r;G.gd.ctx=ctx;if(r.item){gdStart(r.item,r.why,{answer:r.answer});return}
  /* a skip is logged when its reason changes (the numbers aside) */
  var key=r.why.replace(/\d+(\.\d+)?/g,"#");if(key!==G.gd.skipKey){G.gd.skipKey=key;gdLog("skipped",r.why)}}
/* why the tip on the screen ends now: [why, how], "wait" (Fewer like this? asked), or null (it stays). Its screen left (a wall tip off the
   wall; a cue off its card, the Locate label also when its pin goes), its target gone (its marker, its point out of the middle, the ✦),
   or its time up (a cue: seen; a tip: ignored). W is the moment (the page's, or the simulator's) */
function gdEnds(c,now,W){var it=c.item;if(it.onTour&&W.moment!==undefined)return null;
  /* round 7: a Moment on the page ends with Azu's prop (moment.ended: a tap, or 8 s); in the simulator by its time */
  if(it.form==="moment"&&W.moment!==undefined)return W.moment?null:["the moment ended",""];
  if(it.screen==="wall"&&!W.onWall)return["the wall left the screen",""];var keep=it.expires==="never";
  if(c.cue&&(!W.onCard||it.about==="locate"&&!(c.pull||W.locate)))return["its card left the screen",""];
  if(!c.pull&&!keep){if(it.about==="marker"&&!W.marker)return["its marker left the screen",""];
    if(it.about==="middle"||it.about==="point"){var cc=W.central;if(!cc||cc.id!==c.slots.id)return["its point left the middle",""]}
    if(it.about==="gbtn"&&!W.gbtn)return["the ✦ left the screen",""]}
  if(c.ask)return now>=c.askUntil?["Fewer like this? not answered","ignored"]:"wait";
  /* round 6: a tip stays until tapped (On screen for: Until tapped, or the visitor's Tips stay until I tap them): its time never runs out */
  if(!c.cue&&!c.pull&&(it.onScreen==="tap"||W.stay))return null;
  if(now>=c.until&&W.realOk!==false)return[c.cue?"it played":"its time is up",c.cue?"seen":"ignored"];return null}
/* a pull (_3.8 section 3.3: past every gate but one at a time): the item at once, the tip showing gives way; it writes no memory.
   Tap on Azu = Points to the ✦ pulls The ✦ (false when there is no ✦ or no Azu beside it to point) */
function gdPull(id,why){var it=gdInstances(id)[0];if(!it)return false;if(it.about==="gbtn"&&!(ph.querySelector(".gbtn")&&typeof azuEl==="function"&&azuEl()&&gbAzu()&&gbOn()))return false;
  if(G.gd.cur)gdStop("the visitor asked for "+it.label);var ab=ph.querySelector(".azbub");if(ab)ab.remove();gdStart(it,why||"asked by the visitor",{pull:true});return true}
function gdAdvance(ms){for(var k=0;k<ms;k+=250){G.gd.t+=Math.min(250,ms-k);gdTick()}}
setInterval(function(){if(G.gd.manual||noDraw)return;G.gd.t+=250*G.gd.rate;gdTick()},250);
gdLog("visit","a first visit");
/* ----- the simulator (round 5, _3.8 section 7: Preview & Test › Simulate a visit). A scripted visitor on a fake visit clock, 0 to 20 min,
   4 ticks a second as the service runs. Each tick asks the real scheduler (gdPick, no copy of it) with the scripted moment: the context's
   numbers from gdCtxBase (the room and the limits in force), the script's wall, card, tour and panels as the page would give them; a tip
   ends by the service's own rule (gdEnds) and the memory moves through the service's own bookkeeping (gdMemStart, gdMemEnd, gdLearn,
   gdMemUsed) on the simulator's own visitor, never S.gdVisitor. Deterministic: the same script and settings give the same timeline.
   A script: where the visitor points (at, pan: the phone moving 3 s, then the point settles in the middle), the card (opened by a tap on
   the marker: at Half, then Full, then its parts scrolled into view), a tour (its steps: a line, then a pause), a panel (Ask about the
   view, the ✦ menu), pokes on Azu; and how it answers a tip (react: on its n-th showing, after s seconds, tap it, swipe it away, or tap
   the marker; else it is ignored). The result: each shown item (a dot: its time, end, line and why), the quiet spans (a tour, panning,
   a card open, a panel open: no item showed), each skip when its reason changed (the scheduler's words), the tour's lines (none over a
   sentence) and what the learning changed. Unity: GuidanceSimulator (plain C#, EditMode), GuidanceScheduler.Pick on a scripted
   GuidanceState, drawn by the POI Editor's window (IMGUI / UI Toolkit Painter2D) ----- */
var GD_SIM_MS=20*60000,GD_SIM_TICK=250;
/* the cues' lengths as their looks play them (card/hints.js; the Locate label is seen after 4 s) */
var GD_SIM_CUE={grab:1150,locate:4000,navbar:3400,ba:1600,strip:1400,term:2200};
/* the card from its opening (s): at Half (the grabber's moment, the Locate Button), dragged to Full (the bar), then scrolled: a Before /
   After, a strip, an underlined word in view; from 12 s past the hero (the ✦ shows) */
var GD_SIM_CARD=[[0,"half",["locate"]],[5,"full",["navbar","locate"]],[12,"full",["navbar","ba"]],[20,"full",["navbar","strip"]],[28,"full",["navbar","term"]]];
/* the scripted visitors (_3.8 section 7): ev [s, what, arg], react {item: [its n-th showing (0: each), after s, tap / dismiss / card (a tap
   on the marker) / tour (a tap on the marker, then Start the tour), for s]} */
var GD_SIM_SCRIPTS=[
  {id:"first",name:"First visit",says:"a first visit: it looks round the wall, a point every 30 to 70 s; it taps the marker when Tap a marker shows the second time (the card for 50 s), opens Ask about the view when it is offered and takes the tour it is invited to; it ignores the rest",
    ev:[[0,"at","painting"],[30,"pan","torreao"],[62,"pan","square"],[95,"pan","river"],[125,"pan","painting"],[170,"pan","castle"],[205,"pan","arcade"],[240,"pan","painting"],[285,"pan","square"],[330,"pan","painting"],
      [560,"pan","torreao"],[600,"pan","river"],[650,"pan","castle"],[700,"pan","arcade"],[760,"pan","painting"],[820,"pan","square"],[880,"pan","torreao"],[950,"pan","river"],[1020,"pan","castle"],[1100,"pan","painting"]],
    react:{tapmarker:[2,3,"card",50],askview:[1,2,"tap",20],tourof:[1,3,"tap"]}},
  {id:"fast",name:"Fast learner",says:"a fast learner: it taps the marker at 0:25 before any tip asks, starts the tour from the card and hears one step, then pans the wall; it opens Ask about the view when offered and taps a remark",
    ev:[[0,"at","painting"],[25,"card",0],[33,"tour",1],[120,"pan","torreao"],[200,"pan","square"],[300,"pan","river"],[420,"pan","castle"],[540,"pan","arcade"],[700,"pan","painting"],[900,"pan","torreao"]],
    react:{askview:[1,1,"tap",15],inview:[3,2,"tap",15]}},
  {id:"idle",name:"Idle",says:"an idle visitor: it holds the phone on the Ribeira Palace, never touches and ignores every tip",
    ev:[[0,"at","painting"]],react:{}},
  {id:"tour",name:"Tour lover",says:"a tour lover: it taps the marker when Tap a marker shows and starts the tour, takes every tour it is offered, taps each remark and starts the tour again at 13:00",
    ev:[[0,"at","painting"],[290,"pan","torreao"],[400,"pan","painting"],[560,"pan","square"],[700,"pan","painting"],[780,"tour",0],[1000,"pan","river"],[1100,"pan","painting"]],
    react:{tapmarker:[1,2,"tour"],tourof:[0,2,"tap"],inview:[0,2,"tap",15],water:[0,2,"tap"],cookie:[0,2,"tap"]}},
  {id:"expert",name:"Expert",says:"an expert: a returning visitor who has used everything (the card, a tour, Ask, Ask about the view, the ✦ menu, How to use the card); it pans across every point, opens the card at 8:00 for 60 s and ignores the tips",
    returning:true,used:["card","tour","ask","view","menu","wk_grab","wk_settings","wk_bar","wk_locate","wk_block"],
    ev:[[0,"at","square"],[60,"pan","torreao"],[140,"pan","river"],[230,"pan","castle"],[330,"pan","arcade"],[420,"pan","painting"],[480,"card",60],[700,"pan","square"],[900,"pan","torreao"],[1080,"pan","painting"]],react:{}},
  /* round 7: a family on a wall with a Solemn point (Terreiro do Paço, marked Solemn for this visit): it in the middle 5 minutes, then a
     Lively point (the Torreão, with a joke of its own); no Fun while the Solemn point is in the middle */
  {id:"solemn",name:"Family on a Solemn wall",says:"a family (Who's exploring: Family) on the Ribeira wall, where Terreiro do Paço is marked Solemn for this visit: it holds the phone on it for 5 minutes, then pans to the Torreão (a point with a joke of its own), opens the card at 6:40 for 40 s, and looks round the wall; it ignores the tips",
    aud:"family",solemn:["square"],ev:[[0,"at","square"],[300,"pan","torreao"],[400,"card",40],[470,"pan","river"],[600,"pan","castle"],[760,"pan","painting"],[900,"pan","torreao"],[1050,"pan","arcade"]],react:{}}];
function gdSimScript(id){return GD_SIM_SCRIPTS.filter(function(x){return x.id===id})[0]||GD_SIM_SCRIPTS[0]}
/* a visit simulated with these settings (set: {g, life}, a copy of S.guidance and Mascot > Liveliness; none: the Editor's now) */
function gdSimulate(id,set){var sc=gdSimScript(id),keepG=S.guidance,keepL=S.guideCfg.life;if(set){S.guidance=set.g;S.guideCfg.life=set.life}
  try{return gdSimRun(sc)}finally{S.guidance=keepG;S.guideCfg.life=keepL}}
function gdSimRun(sc){var items=gdItems(),base=gdCtxBase(),V=gdNewMemory(0),pois=S.wallPois||[],cardP=pois.filter(function(p){return p.card})[0],cur=null,due=[],occ={},occB={},skipKey="";
  var res={id:sc.id,name:sc.name,dots:[],quiet:[],skips:[],speech:[],learned:[],ms:GD_SIM_MS},lg=function(what,why){res.learned.push({t:now,why:why})},now=0;
  var solemnAt=function(){var id=w.where==="wall"?w.point:cardP&&cardP.id;return(sc.solemn||[]).indexOf(id)>=0};
  var w={where:"wall",point:null,moveTill:-1e9,movedAt:null,touchAt:null,actAt:null,scrollAt:null,cardAt:null,openAt:null,stage:-1,cardTill:-1,wallAt:0,panel:null,panelTill:-1,tour:null};
  if(sc.returning){V.visits=2;V.visit.n=2;V.met=true;(sc.used||[]).forEach(function(f){gdMemUsed(V,f,-60000,items)})}
  var poi=function(id){return pois.filter(function(p){return p.id===id})[0]};
  var onCard=function(){return w.where==="card"||w.where==="tour"},moving=function(){return now<w.moveTill};
  var marker=function(){return w.where==="wall"&&cardP?{id:cardP.id,name:L(cardP.name),x:195,y:380,d:0,middle:w.point===cardP.id}:null};
  var central=function(){var p=w.where==="wall"&&!moving()&&w.point?poi(w.point):null;return p?{id:p.id,name:L(p.name),x:195,y:380,middle:true,tour:!!p.card,min:p.card?tourMinutes(p.id):null}:null};
  var gbtn=function(){return w.where==="wall"||w.where==="card"&&w.stage>=2?{name:"the ✦",x:350,y:760,middle:false}:null};
  var cues=function(moment){var o={},st=w.where==="card"&&w.stage>=0?GD_SIM_CARD[w.stage]:null;if(st){st[2].forEach(function(k){o[k]=CUE_WHY[k]});if(moment==="open"&&st[1]==="half")o.grab=CUE_WHY.grab}return o};
  var ctxAt=function(moment){var t=-Infinity,mv=w.movedAt,tr=w.where==="tour"?{between:!!w.tour.between,step:w.tour.i,talked:(now-w.tour.talkFrom)/1000,toEnd:w.tour.i+1===w.tour.n-1}:null,oc=onCard();
    return Object.assign({},base,{scrollAt:w.scrollAt,onCard:oc,cues:oc?cues(moment):{},moment:moment||null,demo:false,showing:cur?cur.item.label:null,remark:null,pokes:V.visit.pokes||0,
      idleSince:Math.max(w.actAt==null?t:w.actAt,mv==null?t:mv,V.visit.at||0),speaking:!!(w.tour&&now<w.tour.lineTill),state:null,panel:w.panel,dockFull:false,tour:tr,movedAt:mv,touchAt:w.touchAt,
      cardAt:oc?w.cardAt:null,onWall:w.where==="wall",where:w.where==="wall"?"":w.where==="tour"?"the tour":"a card is open",marker:marker(),gbtn:tr?null:gbtn(),central:central(),covers:{},
      stillSince:Math.max(mv==null?t:mv,w.wallAt==null?t:w.wallAt),returning:V.visits>1,solemn:solemnAt(),aud:sc.aud||base.aud})};
  var touch=function(){w.touchAt=w.actAt=now};
  var start=function(it,why){var cue=it.form==="cue",slots=gdSlots(it,V,central(),items),text=cue?(it.says?TXT(it.says):""):gdLine(it,V.visits>1,slots),dur=cue?GD_SIM_CUE[it.about]||2000:it.form==="moment"?8000:gdOnScreen(it,text);
    cur={item:it,at:now,until:now+dur,cue:cue,pull:false,slots:slots,text:text,why:why};gdMemStart(V,it,now,text,cue?null:gdLineKey(it,V.visits>1,slots));var b=it.base||it.id;occ[it.id]=(occ[it.id]||0)+1;occB[b]=(occB[b]||0)+1;
    var d={t:now,end:null,id:it.id,base:b,at:it.point||w.point,label:it.label.replace(/\{point\}/g,slots.point||"the point"),kind:it.kind,cue:cue,text:text,why:why,n:occ[it.id],how:""};res.dots.push(d);cur.dot=d;skipKey="";
    var rc=sc.react&&sc.react[b];if(rc&&(rc[0]===0||rc[0]===occB[b]))due.push({t:now+rc[1]*1000,act:rc[2],dur:rc[3],dot:d})};
  var stop=function(why,how){var c=cur;if(!c)return;cur=null;gdMemEnd(V,c,how,now,lg);c.dot.end=now;c.dot.how=how;c.dot.endWhy=why};
  /* the events, as the service answers them (card.opened, view.opened, menu.opened, tourStep.ended, startTour.before) */
  var used=function(f){gdMemUsed(V,f,now,items);if(cur&&!cur.cue&&cur.item.teaches===f)stop(GD_EVENTS[f],"acted")};
  var panel=function(name,s,f){w.panel=name;w.panelTill=now+(s||20)*1000;touch();if(f)used(f)};
  var openCard=function(s,quiet){var v=V.visit,e0=v.lastEnd;w.where="card";w.cardAt=w.openAt=now;w.stage=0;w.cardTill=s?now+s*1000:Infinity;touch();used("card");
    if(cur&&!cur.cue)stop("the visitor opened a card","");if(v.lastEnd!==e0)v.openEnd=v.lastEnd;v.cards=(v.cards||0)+1;
    /* the card's opening: the scheduler asked at that moment, only the cues that answer it */
    if(quiet)return;if(cur&&cur.cue)stop("a new card","");if(!cur){var r=gdPick(items,V,ctxAt("open"),now);if(r.item)start(r.item,r.why)}};
  var closeCard=function(){w.where="wall";w.stage=-1;w.openAt=null;w.cardTill=-1;w.wallAt=now;touch()};
  var startTour=function(k){if(cur)stop("the tour started","");if(!onCard())openCard(0,true);var n=Math.max(1,(S.guideSteps||[]).length),step=tourMinutes()*60000/n;
    w.where="tour";w.openAt=null;w.tour={i:0,n:n,line:step-2000,lineTill:now+step-2000,pauseTill:null,between:false,stopAfter:k||0,talkFrom:now,asked:false};res.speech.push([now,now+step-2000])};
  var endTour=function(){w.tour=null;w.where="card";w.stage=0;w.cardTill=now+4000};
  var tap=function(){var c=cur,it=c.item;touch();
    if(it.tap==="view"){stop("tapped: Ask about the view opens","acted");panel("Ask about the view",20,"view");return}
    if(it.tap==="remark"){var p=poi(c.slots.id);stop("tapped","acted");if(p&&p.card)startTour();else panel("Ask about the view",15,"view");return}
    if(it.tap==="menu"){stop("tapped: the ✦ menu opens","acted");panel("the ✦ menu",5,"menu");return}
    if(it.tap==="tour"){stop("tapped: the tour starts","acted");startTour();return}
    stop("tapped","acted")};
  var act=function(a,arg,d){if(a==="tap"||a==="dismiss"){if(!cur||cur.dot!==d)return;if(a==="dismiss"){touch();stop("swiped away","dismissed")}else tap();return}
    if(a==="at"){w.point=arg;return}if(a==="pan"){w.moveTill=now+3000;w.point=arg;touch();return}
    if(a==="card"){if(w.where==="wall")openCard(arg);return}if(a==="tour"){if(w.where!=="tour")startTour(arg);return}
    if(a==="view"){panel("Ask about the view",arg,"view");return}if(a==="menu"){panel("the ✦ menu",arg,"menu");return}
    if(a==="poke"){V.visit.pokes=(V.visit.pokes||0)+1;touch();return}if(a==="touch")touch()};
  var evs=sc.ev.map(function(e){return{t:e[0]*1000,act:e[1],dur:e[2]}});
  for(now=0;now<=GD_SIM_MS;now+=GD_SIM_TICK){
    while(evs.length&&evs[0].t<=now){var e=evs.shift();act(e.act,e.dur)}
    due.sort(function(a,b){return a.t-b.t});while(due.length&&due[0].t<=now){var q=due.shift();act(q.act,q.dur||0,q.dot)}
    /* the world moves on: a panel closes, the card is dragged and scrolled, the tour's line ends (a step heard) and its pause, the card closes */
    if(w.panel&&now>=w.panelTill){w.panel=null;touch()}
    if(w.where==="card"&&w.openAt!=null){var el=(now-w.openAt)/1000,s0=w.stage;while(w.stage+1<GD_SIM_CARD.length&&GD_SIM_CARD[w.stage+1][0]<=el)w.stage++;if(w.stage!==s0){if(w.stage===1)touch();else w.scrollAt=now}}
    var T=w.tour;if(T){if(T.pauseTill==null&&now>=T.lineTill){V.visit.heard=true;T.between=true;T.pauseTill=now+2000;used("tour")}
      else if(T.pauseTill!=null&&now>=T.pauseTill&&!(cur&&cur.item.form==="moment")){
        /* the tour moves on: the scheduler asked at that moment (only a Moment); one it picks holds the next step until it ends */
        if(!T.asked){T.asked=true;if(!cur&&T.i+1<T.n&&!(T.stopAfter&&T.i+1>=T.stopAfter)){var rm=gdPick(items,V,ctxAt("between"),now);if(rm.item){start(rm.item,rm.why);T.talkFrom=now}}}
        if(!(cur&&cur.item.form==="moment")){T.asked=false;T.i++;if(T.i>=T.n||T.stopAfter&&T.i>=T.stopAfter)endTour();else{T.between=false;T.pauseTill=null;T.lineTill=now+T.line;res.speech.push([now,T.lineTill])}}}}
    if(w.where==="card"&&now>=w.cardTill)closeCard();
    if(moving())w.movedAt=now;
    if(cur){var en=gdEnds(cur,now,{onWall:w.where==="wall",onCard:onCard(),locate:!!cues().locate,marker:marker(),central:central(),gbtn:w.where==="tour"?null:gbtn(),realOk:true});if(en&&en!=="wait")stop(en[0],en[1]);continue}
    var r=gdPick(items,V,ctxAt(null),now);if(r.item){start(r.item,r.why);continue}
    var key=r.why.replace(/\d+(\.\d+)?/g,"#");if(key!==skipKey){skipKey=key;res.skips.push({t:now,why:r.why})}
    /* the quiet lane: no item, and the gates or the moment held (a tour, the phone panning and 0.7 s after, a panel, a card open) */
    var cls=w.where==="tour"?"tour":now<w.moveTill+700?"pan":w.panel?"panel":w.where==="card"?"card":null,lq=res.quiet[res.quiet.length-1];
    if(cls){if(lq&&lq.cls===cls&&lq.to===now)lq.to=now+GD_SIM_TICK;else res.quiet.push({from:now,to:now+GD_SIM_TICK,cls:cls,why:r.why})}}
  if(cur)stop("the simulation ended","");return res}
/* the summary of a simulated visit over its first n minutes: the tips (a cue is not a tip), by kind, the most in any minute, any over a sentence */
function gdSimSum(res,mins){var lim=(mins||15)*60000,tips=res.dots.filter(function(d){return!d.cue&&d.t<lim}),cues=res.dots.filter(function(d){return d.cue&&d.t<lim}),by=function(k){return tips.filter(function(d){return d.kind===k}).length};
  var perMin=tips.reduce(function(m,d){return Math.max(m,tips.filter(function(x){return x.t>d.t-60000&&x.t<=d.t}).length)},0),over=tips.filter(function(d){return res.speech.some(function(s){return d.t>=s[0]&&d.t<s[1]})}).length;
  var fc=tips.map(function(d){return d.kind}).indexOf("content"),k=[["App",by("app")],["Content",by("content")],["Fun",by("fun")]].filter(function(x){return x[1]>0});
  return{tips:tips.length,app:by("app"),content:by("content"),fun:by("fun"),cues:cues.length,perMin:perMin,over:over,first:tips[0]?tips[0].kind:null,appBefore:fc<0?tips.length:fc,
    words:tips.length+" tips in "+(mins||15)+" min"+(k.length?": "+k.map(function(x){return x[0]+" "+x[1]}).join(", "):"")+(cues.length?" (and "+cues.length+" card cue"+(cues.length>1?"s":"")+")":"")+"; at most "+perMin+" a minute; "+(over?over+" over a sentence":"none over a sentence")}}
/* ----- round 6: every tip that showed also lands in the dock's conversation under Tips (a bubble is never the only channel); Settings >
   The guide > Show the tips again resets the memory (what showed, what is done, the features used; the visitor's Tips settings stay), with
   an Undo toast and no popup. Unity: GuidanceService.Said (the dock's thread reads it), GuidanceMemory.ResetShown with a kept copy for Undo ----- */
function gdSaid(it,text,now){var v=S.gdVisitor.visit,l=v.tips||(v.tips=[]);if(!text)return;l.push({t:now,id:it.id,label:it.label,text:text});if(l.length>30)l.shift()}
function gdTipsAgain(){var V=S.gdVisitor,keep=JSON.stringify(V);if(G.gd.cur)gdStop("Show the tips again");V.ever.items={};V.visit.items={};V.used={};V.visit.rest={};V.visit.fewer={};V.visit.starts=[];V.visit.ignoredRun=0;V.visit.breathMul=1;V.visit.lastEnd=null;V.visit.cueEnd=null;
  G.gd.tipsUndo=keep;gdLog("event","Show the tips again: the memory reset (the visitor's Tips settings kept)");return keep}
function gdTipsUndo(){if(!G.gd.tipsUndo)return false;S.gdVisitor=JSON.parse(G.gd.tipsUndo);G.gd.tipsUndo=null;gdLog("event","Show the tips again undone: the memory back");return true}
/* ----- round 8 (_3.8 section 6): the first pages, at the app's start. A launch (gdLaunch: the preview bar's and Preview & Test's Start the
   app) is the app opening: a first run (the visitor reset to a first visit, as Reset the visitor; the audience not yet asked; the camera
   not yet allowed, as a new install) or a later launch (the same visitor back: a new visit, with Remember across visits Off a first one).
   On a first run the pages run from Hello (gdPagesRun: First pages' On pages in order; Who's exploring only when Where to ask = At the
   app's start and not yet answered; the camera only while it is not allowed, and its own request even when its page is Off), the
   scheduler quiet (a gate); the wall ends the run: the Opening message says Straight to the wall's short line, then Tap a marker as The
   start's. Skip skips the rest of the intro (Who's exploring unanswered is Explorer; the camera's own request still comes). The camera:
   Allow the camera, then the phone's request (G.gd.camPerm: ask / allowed / denied, the phone's permission); refused, how to allow it with
   Open Settings (the phone's Settings, a stand-in) and Try again: the run never goes on to the wall without the camera. Show me how it
   works: How to use the card's bubble on the wall after the pages (it says the visit's first words). Later launches: no pages, the Opening
   message's Welcome back. The memory: S.gdVisitor.pages (seen at which visit, skipped, Show me how) and the visit's pages (the short
   line). A newer Editor demo closes the pages. Unity: an OnboardingController (UI Toolkit screens from GuidanceCatalog's First pages),
   started by the app's first scene when GuidanceMemory has no pages; the camera through Application.RequestUserAuthorization(
   UserAuthorization.WebCam) on iOS (Application.HasUserAuthorization after; refused: Settings by UIApplication.openSettingsURLString, a
   small native call) and Permission.RequestUserPermission(Permission.Camera) with PermissionCallbacks (Denied, DeniedAndDontAskAgain:
   the app settings intent) on Android; AR Foundation's ARCameraManager starts only once allowed ----- */
G.gd.pages=null;G.gd.camPerm="allowed";
function gdPagesRun(){var g=gdGroupById("pages"),on=!g||g.on!==false,need=G.gd.camPerm!=="allowed",out=[];
  if(on)gdPageDefs().forEach(function(it){if(!it.on||it.page==="wall")return;if(it.page==="aud"&&!(S.aud.where==="start"&&!S.aud.seen))return;if(it.page==="camera"&&!need)return;out.push(it.id)});
  if(need&&out.indexOf("pg_camera")<0)out.push("pg_camera:direct");return out}
function gdPageName(id){var it=S.guidance.items[String(id).split(":")[0]];return it?it.label+(/:direct$/.test(id)?" (the phone's request only: its page is Off)":""):id}
/* the page showing now: its id in the run, its item, and whether it is the camera's request alone */
function gdPageNow(){var P=G.gd.pages;if(!P)return null;var id=P.run[P.i]||"";return{id:id,it:S.guidance.items[id.split(":")[0]]||null,direct:/:direct$/.test(id)}}
function gdLaunch(kind){if(noDraw)return false;var later=kind==="later";
  if(G.gd.pages)gdPagesEnd("the app started again",false);if(G.gd.cur)gdStop("the app started");gdChip(null);G.gd.demo=false;
  if(G.on)stopTour();if(typeof hideStopUi==="function")hideStopUi();S.settingsOpen=false;S.contentsOpen=false;G.fab.menu=false;G.cam.open=false;ph.querySelectorAll(".azbub,.gdtoast,.onb").forEach(function(x){x.remove()});
  /* a later launch of a visitor the Lab opened mid-visit: its first pages were seen at that earlier launch */
  if(later&&!S.gdVisitor.pages){S.gdVisitor.pages={at:gdNow(),visit:S.gdVisitor.visits,how:false,skipped:false,earlier:true}}
  gdVisitStart(later,later?"a later launch":"Start the app: a first run");
  var first=!S.gdVisitor.pages;if(!later)G.gd.camPerm="ask";
  /* a first run: the audience not yet asked (Who's exploring asks again where it is asked) */
  if(first){S.aud.seen=false;S.vis.aud="explorer";S.vis.len=""}
  S.view="card";S.stop="closed";renderPhone(false);
  if(!first){gdLog("event","a later launch: no pages (seen at visit "+S.gdVisitor.pages.visit+"); the Opening message says Welcome back");return true}
  gdPagesStart(gdPagesRun());return true}
function gdPagesStart(run,o){o=o||{};if(!run.length){gdPagesDone({how:false,skipped:false},"no page to show");return null}
  G.gd.pages={run:run,i:o.i||0,how:false,skipped:false,cam:o.cam||null,seq:G.demoSeq,at:gdNow(),preview:!!o.preview};
  if(/:direct$/.test(run[G.gd.pages.i]))G.gd.pages.cam="prompt";
  if(!o.preview)gdLog("event","the first pages: "+run.map(gdPageName).join(" → ")+" → the wall (the scheduler is quiet while they run)");gdPagePaint(true);return G.gd.pages}
/* an Editor's demo: one page shown, the run going on from it as the visitor would (no memory) */
function gdPagesPreview(id,o){if(noDraw)return null;o=o||{};if(G.gd.pages)gdPagesEnd("another page shown",false);if(G.gd.cur)gdStop("a page shown");
  if(G.on)stopTour();S.settingsOpen=false;G.fab.menu=false;G.cam.open=false;S.view="card";S.stop="closed";renderPhone(false);
  var run=gdPageDefs().filter(function(x){return x.page!=="wall"&&(x.on||x.id===id)}).map(function(x){return x.id}),i=run.indexOf(id);if(i<0){run=[id];i=0}
  return gdPagesStart(run,{i:i,cam:o.cam,preview:true})}
function gdPagesNext(how){var P=G.gd.pages;if(!P)return;if(how)P.how=true;var n=gdPageNow();
  if(n&&n.it&&!P.preview)gdLog("event","the first pages: "+n.it.label+" done"+(how?" (Show me how it works)":""));P.i++;P.cam=null;
  if(P.i>=P.run.length){gdPagesEnd("the run ended on the wall",true);return}if(/:direct$/.test(P.run[P.i]))P.cam="prompt";gdPagePaint(true)}
/* Skip: the rest of the intro (an unanswered Who's exploring is Explorer); the camera's own request still comes when it is not allowed */
function gdPagesSkip(){var P=G.gd.pages;if(!P)return;var n=gdPageNow();P.skipped=true;
  if(S.aud.where==="start"&&!S.aud.seen&&P.run.slice(P.i).some(function(id){return/^pg_aud/.test(id)})){S.aud.seen=true;if(!P.preview)gdLog("event","Who's exploring: skipped with the intro (Explorer)")}
  if(!P.preview)gdLog("event","Skip on "+(n&&n.it?n.it.label:"a page")+": the rest of the intro skipped");
  if(G.gd.camPerm!=="allowed"){var cid=n&&/^pg_camera/.test(n.id)?n.id:"pg_camera:direct";P.run=P.run.slice(0,P.i).concat([cid]);P.i=P.run.length-1;P.cam="prompt";gdPagePaint(true);return}
  gdPagesEnd("Skip: the intro skipped",true)}
/* the camera: its page's button asks; the phone's own request answers; refused, Open Settings (the switch) and Try again */
function gdCamAsk(){var P=G.gd.pages;if(!P)return;P.cam="prompt";if(!P.preview)gdLog("event","the camera: the phone's own request");gdPagePaint(false)}
function gdCamAnswer(ok){var P=G.gd.pages;if(!P)return;G.gd.camPerm=ok?"allowed":"denied";
  if(!P.preview)gdLog("event",ok?"the camera allowed":"the camera refused: how to allow it, Open Settings and Try again (never the wall without it)");
  if(ok){gdPagesNext();return}P.cam="refused";gdPagePaint(true)}
function gdCamSettings(){var P=G.gd.pages;if(!P)return;P.cam="settings";gdPagePaint(true)}
function gdCamSwitch(){var P=G.gd.pages;if(!P)return;G.gd.camPerm=G.gd.camPerm==="allowed"?"denied":"allowed";gdPagePaint(false)}
function gdCamBack(){var P=G.gd.pages;if(!P)return;if(G.gd.camPerm==="allowed"){if(!P.preview)gdLog("event","the camera allowed in Settings: back to the app");gdPagesNext();return}P.cam="refused";gdPagePaint(true)}
/* Try again: the phone's request once more (Android asks again; iOS answers at once with what Settings says: the stand-in asks again) */
function gdCamRetry(){var P=G.gd.pages;if(!P)return;P.cam="prompt";gdPagePaint(false)}
/* the run ends: on the wall (done), or closed by a new launch, a new visit or a newer Editor demo (nothing remembered) */
function gdPagesEnd(why,done){var P=G.gd.pages;if(!P)return;G.gd.pages=null;gdPagesUnpaint();
  if(!done||P.preview){if(!P.preview)gdLog("event","the first pages closed: "+why);return}gdPagesDone(P,why)}
function gdPagesDone(P,why){var V=S.gdVisitor,now=gdNow();V.pages={at:now,visit:V.visits,how:!!P.how,skipped:!!P.skipped};V.visit.pages=true;
  /* asked at the app's start and its page Off, or never reached: no question on the first card either */
  if(S.aud.where==="start"&&!S.aud.seen)S.aud.seen=true;
  gdLog("event","the first pages ended ("+why+"): the wall, "+(P.how?"How to use the card's bubble":"the Opening message's short line"));
  if(P.how)gdPagesHow()}
/* Show me how it works: How to use the card's bubble on the wall says the visit's first words (the Opening message counts it as said) */
function gdPagesHow(){if(!gdWalk("Show me how it works (the first pages)"))return false;var op=gdItems().filter(function(x){return x.id==="opening"})[0],c=G.gd.cur;
  if(op&&c){gdMemStart(S.gdVisitor,op,gdNow(),c.text,null);gdLog("event","the Opening message: said by How to use the card's bubble (Show me how it works)")}return true}
/* Who's exploring answered on its page (audiences/audiences.js's audience.asked): the next page */
on("audience.asked",function(a,skip){var n=gdPageNow();if(!n||!n.it||n.it.page!=="aud")return;if(!G.gd.pages.preview)gdLog("event","Who's exploring at the app's start: "+(skip?"Skip (Explorer)":AUDN[a]||a));gdPagesNext()});
/* a newer Editor demo takes the phone: the pages close */
setInterval(function(){var P=G.gd.pages;if(P&&P.seq!==G.demoSeq)gdPagesEnd("a newer demo took the phone",false)},250);
/* ----- round 9 (_3.8 section 3.4), the visitor's pull: Things to try and Ask Azu's "how do I…?". Things to try opens from the ✦ menu
   (Overall › Things to try: Shown) and from Settings › The guide; while it is open it is a panel (the scheduler is quiet). Its Show me
   (and Ask Azu's Show me card) is a pull: past the limits, never past the gates that still hold for a pull (a system state showing, the
   first pages running); one at a time (the tip showing gives way, and while the gesture plays the scheduler is quiet: context.showing);
   it writes no memory. A "how do I…" answer marks its item as seen (shown this visit and ever, never done: done is the feature used).
   The features the card's cues and The guide teach come from the visitor's own touch, here (the page's own events, so no other module
   changes): a Before / After dragged, a strip swiped, the Navigation Bar or the Locate Button tapped, the guide paused. Unity: the same
   GuidanceService, fed by the card's controls' events (BeforeAfter.Dragged, Strip.Swiped, NavBar.Jumped, Locate.Tapped, GuideTour.Paused) ----- */
G.gd.tryOpen=false;G.gd.tryShow=null;G.gd.tryOwner=null;
function gdTryPlaying(){var s=G.gd.tryShow;return!!(s&&performance.now()<s.realUntil)}
/* the gates a pull still meets (round 2's rule: the limits pass, the gates hold): a system state showing, the first pages running */
function gdPullGate(){var st=ph.querySelector(".stv"),sk=st&&(st.className.match(/\bst-(\w+)/)||[])[1];if(sk)return"quiet while a system state shows ("+(GD_STATES[sk]||sk)+")";
  if(G.gd.pages)return"quiet while the first pages run";return null}
/* Things to try opens (the ✦ menu, Settings › The guide, an Editor demo) and closes */
function gdTryOpen(from,demo){G.fab.menu=false;S.settingsOpen=false;S.contentsOpen=false;if(G.gd.cur)gdStop("the visitor opened Things to try");G.gd.tryOpen=true;G.gd.tryOwner=demo?G.demoSeq:null;
  var c=gdTryCount();if(!demo)gdLog("event","Things to try opened ("+from+"): "+c.done+" of "+c.n+" tried");renderPhone(true);return true}
function gdTryClose(){if(!G.gd.tryOpen)return;G.gd.tryOpen=false;G.gd.tryOwner=null;renderPhone(true)}
/* a card opened under it (a marker tapped above the sheet): the sheet closes */
on("card.opened",function(){G.gd.tryOpen=false;G.gd.tryOwner=null});
/* an answer to "how do I…" seen: its item shown this visit and ever (as a step of How to use the card is seen), never done */
function gdTold(id,how){var now=gdNow(),vm=gdVM(id),em=gdEM(id);vm.shown++;vm.last=now;em.shown++;em.last=now;vm.told=(vm.told||0)+1;em.told=(em.told||0)+1;
  var it=S.guidance.items[id];gdLog("event","Ask the mascot answered how to "+(it?gdTryWord(it).toLowerCase():id)+" ("+how+"): the "+(it?it.label:id)+" seen, not done")}
/* the offline answer (no key: the scripted answers): the feature's words and a Show me card, in the dock's conversation or on the wall */
function gdHowTurn(q,it,t){return{q:q,links:[],chips:[],calls:[],cites:[],source:"",text:t,ctx:null,shows:[it.id],how:true}}
function gdHowAsk(q,it){it=it||gdHowFind(q);if(!it)return false;emitLatestFirst("ask.before",q);pauseTour();G.askOpen=true;G.busy=false;var t=gdTryHowLine(it),turn=gdHowTurn(q,it,t);
  G.answer=t;G.agTurn=turn;gdTold(it.id,"Ask the mascot, offline");emit("ask.after",t,turn);renderGuide();say(t,function(){},function(){});return true}
/* joined latest first, so before the live agent: with the live agent on, the agent answers (its tool show_how); without, the match */
on("ask.answer",function(q){if(G.agTurn&&G.agTurn.how)G.agTurn=null;if(!q||!String(q).trim()||agLive())return;var it=gdHowFind(q);if(!it)return;return gdHowAsk(q,it)});
on("camAsk.answer",function(q,kind){if(G.cam.turn&&G.cam.turn.how)G.cam.turn=null;if(kind||!q||!String(q).trim()||agLive())return;var it=gdHowFind(q);if(!it)return;
  var t=gdTryHowLine(it);G.cam.open=true;G.cam.route=[];G.cam.lit=[];G.cam.hunt=null;G.cam.turn=gdHowTurn(q,it,t);gdTold(it.id,"Ask about the view, offline");camSay(t);return true});
/* the features used, from the visitor's own touch (registered on the document, so a module that stops a tap from going on never hides it) */
document.addEventListener("pointerdown",function(e){var b=!G.handAct&&ph.contains(e.target)&&e.target.closest("[data-ba]");G.gd.baDrag=b?{x:e.clientX,id:e.pointerId,used:false}:null},{capture:true,passive:true});
document.addEventListener("pointermove",function(e){var d=G.gd.baDrag;if(!d||d.used||e.pointerId!==d.id)return;if(Math.abs(e.clientX-d.x)>12){d.used=true;gdUsed("ba","dragged")}},{capture:true,passive:true});
document.addEventListener("pointerup",function(){G.gd.baDrag=null},{capture:true,passive:true});
scr.addEventListener("scroll",function(e){var t=e.target;if(G.handAct||t===scr||!t.matches||!t.matches(".el-car,.hstrip")||t._gdSwiped||t.scrollLeft<24)return;t._gdSwiped=true;gdUsed("strip","swiped")},{capture:true,passive:true});
document.addEventListener("click",function(e){var t=e.target;if(!t.closest||!ph.contains(t)||G.gd.demo||G.handAct)return;
  if(t.closest("#navbar [data-navsec],#navbar [data-navmore]")){gdUsed("navbar","tapped");return}
  if(t.closest(".locpin")){gdUsed("locate","tapped");return}
  if(t.closest('[data-g="pause"]'))gdUsed("pause","tapped")},true);

/* ----- round 12, Fun › On the tour: the tour's plan (research_tour_entertainment.md 3.1). As a tour starts (not a walkthrough of How to
   use the card), the scheduler is asked once for the plan (gdTourPlan, pure) with the tour's steps as they will play (the length, the lens,
   the introduction; each step's seconds of narration at the visitor's speed and whether it holds a reviewed question or guess in this
   language). The plan is fixed and readable (Guide › Guidance › Fun › On the tour: "This tour's plan"; the Log); only what the visitor does
   changes it: a Not now, No quiz, a skip (a seam reached by Next on a step not heard drops its beat), a stop (the plan is kept: Resume
   keeps the promise; stopped for good, the quiz is offered from the ✦ menu, or said away in one line). At each seam the service asks the
   scheduler for its beat (gdCheck: the live gates) and holds the next step until it ends. Unity: GuideTour asks
   GuidanceService.PlanTour at Start; the seams are its StepEnded events ----- */
/* a step's seconds of narration (guide/dock.js's stepSecs: its recorded words' times, else its words at the visitor's speed) */
function gdTourIn(steps){var l=S.lang||"en",g=gdGroupById("ontour")||{},w=gdWordOf(l),fd=S.tourFind;
  return{steps:steps.map(function(st){return{id:st.id,act:st.act,secs:stepSecs(st),title:stepTitle(st)||(gdQuizStep(st)?TXT("knowledge_check_heading"):""),q:gdQOk(S.tourQ[st.id],l),guess:gdGuessOk(S.tourGuess[st.id],l),quizBlock:gdQuizStep(st)}}),
    len:typeof lenNow==="function"?lenNow():"standard",aud:typeof aud==="function"?aud():"explorer",lang:l,every:+g.every||75,perTour:+g.perTour||3,perShort:+g.perShort||2,perDeep:+g.perDeep||5,auds:g.auds||null,
    /* round 13: the point's word in this language (Don't say it!) and its find (Find it on the wall) */
    word:w?{w:w.w,span:(w.span||[]).slice(),rev:!!w.rev,n:gdWordCount(w,l)}:null,find:fd&&fd.point?{after:fd.after,point:fd.point,onWall:gdFindOnWall(fd)}:null}}
/* round 13: a step of the card's own quiz (a Knowledge Check block: "Test yourself"), which Azu's promised quiz replaces */
function gdQuizStep(st){var m=/^b:(.+)$/.exec(st&&st.ref||"");if(!m)return false;var b=byId(S.card,m[1]);return!!(b&&b.kind==="Knowledge Check")}
/* the wall only: the card's point and the find's point are both on the wall in view */
function gdFindOnWall(fd){var W=(typeof curWall==="function"?curWall().pois:S.wallPois)||[],me=S.m&&S.m.poi||"painting";return!!fd&&W.some(function(p){return p.id===me})&&W.some(function(p){return p.id===fd.point})}
function gdPlanCtx(){return{solemn:gdSolemnNow()||!!(S.m&&S.m.solemn&&(G.on||S.view==="card")),lively:S.guideCfg.life==="lively",sr:!!S.ui.gdSR,games:gdPrefs().games||"on",notNow:+S.gdVisitor.visit.notNow||0}}
/* the steps a tour would play now (not started): the length and the lens (audiences/stepFits), the introduction when it is due */
function gdStepsNow(){if(G.on&&!S.howSaved)return S.guideSteps;var st=(S.fullSteps||S.guideSteps).filter(function(x){return typeof stepFits!=="function"||stepFits(x)});
  if(S.guideCfg.intro==="every"||(S.guideCfg.intro==="first"&&!S.introSeen))st=[introStep()].concat(st);return st}
/* the plan a tour would get now (the Editor's line and Why not now? when no tour plays) */
function gdPlanNow(){return gdTourPlan(gdItems(),S.gdVisitor,gdPlanCtx(),gdTourIn(gdStepsNow()))}
function gdPlanMake(){var P=gdTourPlan(gdItems(),S.gdVisitor,gdPlanCtx(),gdTourIn(S.guideSteps));P.point=S.m&&S.m.poi||"painting";P.quiz=null;P.at0=gdNow();G.gd.plan=P;G.gd.heard={};G.gd.guess=null;G.gd.owed=null;G.gd.skipped=[];gdWordOff();
  gdLog("event","the tour's plan: "+gdPlanWords(P));gdSkipApply(P);return P}
/* round 13 (the developer's answer on round 12): with the quiz promised, the tour skips the card's own quiz step (the plan says so; the
   block stays on the card to play freely); the quiz dropped (No quiz, thanks, two Not nows, a skip), the step plays again */
function gdSkipApply(P){(P.skip||[]).forEach(function(x){var j=S.guideSteps.findIndex(function(s){return s.id===x.id});if(j<0||j<=G.i)return;var st=S.guideSteps.splice(j,1)[0];
  G.gd.skipped.push({st:st,before:S.guideSteps[j]?S.guideSteps[j].id:null});gdLog("event","the card's “"+(x.title||x.id)+"” step skipped: the mascot's quiz replaces it")});
  /* the panel's progress line redrawn once the ✦ has grown into it (a redraw now would cut its growth or its crossfade) */
  if(G.on&&(P.skip||[]).length)setTimeout(function(){if(G.on)renderGuide()},(typeof MORPH_MS!=="undefined"?MORPH_MS:320)+120)}
function gdSkipUndo(why){var k=G.gd.skipped||[];if(!k.length)return;G.gd.skipped=[];var P=G.gd.plan;
  k.forEach(function(x){if(S.guideSteps.some(function(s){return s.id===x.st.id}))return;var j=x.before?S.guideSteps.findIndex(function(s){return s.id===x.before}):S.guideSteps.length;if(j<0)j=S.guideSteps.length;if(j<=G.i)return;
    S.guideSteps.splice(j,0,x.st);gdLog("event","the card's “"+(stepTitle(x.st)||x.st.id)+"” step plays again ("+why+")")});if(P)P.skipBack=why;if(G.on)renderGuide()}
on("startTour.after",function(from){if(S.howSaved)return;var P=G.gd.plan;
  /* Resume (from a kept step): the same plan, its promise kept; a new tour: a new plan */
  if(from&&P&&P.stopped&&P.point===(S.m&&S.m.poi||"painting")){P.stopped=false;G.gd.owed=null;gdLog("event","the tour resumed: its plan kept");var f=gdPlanBeat("quizfinale",P);G.gd.skipped=[];if(f&&f.state==="planned"&&P.quiz!=="off")gdSkipApply(P);return}gdPlanMake()});
on("stopTour.after",function(){var P=G.gd.plan;if(!P||S.howSaved)return;P.stopped=true;var f=gdPlanBeat("quizfinale");
  /* stopped before its finale, a quiz promised and accepted: offered from the ✦ menu for the rest of the visit, or said away in one line */
  if(f&&f.state==="planned"&&P.quiz==="on"){var q=gdQuizQs(P,true);if(q.length>=2){G.gd.owed={n:q.length,qs:q};gdLog("event","the tour stopped before its quiz: Your "+q.length+" questions in the ✦ menu")}
    else{G.gd.owed=null;f.state="dropped";f.why="the tour stopped: too few questions from the steps heard";setTimeout(function(){if(!G.on&&typeof azBubble==="function")azBubble(TXT("tour_quiz_saved"),3200)},400);gdLog("event","the tour stopped before its quiz: said away (“"+TXT("tour_quiz_saved")+"”)")}}});
/* a beat of the plan by its item */
function gdPlanBeat(id,P){P=P||G.gd.plan;return P?P.beats.filter(function(b){return b.id===id})[0]||null:null}
/* a beat dropped (a skip, a gate at its seam, the visitor's Not now): with its reason; a promise dropped takes its payoff with it */
function gdBeatDrop(b,why){if(!b||b.state!=="planned")return;b.state="dropped";b.why=why;gdLog("skipped","“"+b.label+"”: dropped from the tour's plan ("+why+")");
  if(b.place==="finale"&&b.pairs)gdSkipUndo("no quiz: "+why);
  if(b.pairs&&b.place==="opening"){var f=gdPlanBeat(b.pairs);if(f&&f.state==="planned")gdBeatDrop(f,"its promise was not made")}}
/* a seam (the tour moving on, guide/tour.js's next): the guess's answer after the step that held it; else the beat planned after this step */
function gdSeam(){var P=G.gd.plan,st=S.guideSteps[G.i];if(!P||!st||S.howSaved)return false;
  var gs=G.gd.guess;if(gs&&gs.step===st.id&&!gs.shown){gs.shown=true;G.gd.hold=true;gdGuessAnswer(gs);return true}
  /* round 13: Don't say it!'s result at the seam after its span */
  var ds=G.gd.ds;if(ds&&ds.on&&ds.endAfter===st.id){G.gd.hold=true;gdWordEnd();return true}
  var b=P.beats.filter(function(x){return x.state==="planned"&&x.after===st.id})[0];if(!b)return false;
  var it=gdItems().filter(function(x){return x.id===b.id})[0];if(!it){gdBeatDrop(b,"its item is gone");return false}
  /* a seam reached by a skip (Next on a step not heard): its beat drops; the finale still keeps the promise */
  if(!G.gd.heard[st.id]&&b.place!=="finale"){gdBeatDrop(b,"the visitor skipped step "+(G.i+1));return false}
  G.gd.moment="between";var r;try{var ctx=gdContext();ctx.speaking=false;r=S.guidance.on?gdCheck(it,gdItems(),S.gdVisitor,ctx,gdNow()):{no:"Guidance is Off"}}finally{G.gd.moment=null}
  if(!r.ok){gdBeatDrop(b,r.no);return false}
  b.state="played";G.gd.hold=true;gdStart(it,r.why,{beat:b});return true}
/* Next pressed: the step being said counts as heard when 80 % of it was played (any speed) */
document.addEventListener("click",function(e){var t=e.target.closest&&e.target.closest('#ph [data-g="next"]');if(!t||!G.on)return;var st=S.guideSteps[G.i];if(st&&typeof dkShare==="function"&&dkShare()>=.8)G.gd.heard[st.id]=1},true);
/* a game's Not now (its 4 s chip; No quiz, thanks): two this visit end the games for the visit, said once, kindly */
function gdNotNow(it){var v=S.gdVisitor.visit;v.notNow=(+v.notNow||0)+1;gdLog("event","Not now on the "+it.label+" ("+v.notNow+" this visit)");
  if(v.notNow>=2){var P=G.gd.plan;if(P)P.beats.forEach(function(b){if(b.game&&b.state==="planned")gdBeatDrop(b,"two Not nows this visit: no more games")});if(!v.nomore){v.nomore=true;return TXT("tour_nomore")}}return""}
/* the quiz's questions at the finale (research 5.3): from the steps heard, one from each third, never two from a step */
function gdQuizQs(P,heardOnly){var l=S.lang||"en",steps=S.guideSteps,at=0,out=[],k0=P?P.k0:0;steps.forEach(function(st,j){var s=stepSecs(st);if(j>k0&&!/^(intro|teaser|end)$/.test(st.act))out.push({id:st.id,from:0,at:at+s/2,q:gdQOk(S.tourQ[st.id],l),heard:!!G.gd.heard[st.id]});at+=s});
  var lo=out.length?out[0].at:0;out.forEach(function(x){x.from=lo});var it=S.guidance.items.quizfinale;return gdQuizPick(out,+(it&&it.qn)||3,at)}
/* the best score per point, on the phone only (the visitor's memory; never sent) */
function gdQuizBest(point){var b=S.gdVisitor.quizBest||(S.gdVisitor.quizBest={});return b[point||"painting"]||null}
function gdQuizKeep(point,right,n){var b=S.gdVisitor.quizBest||(S.gdVisitor.quizBest={}),k=point||"painting",o=b[k];if(!o||right/n>o.right/o.n||right/n===o.right/o.n&&n>o.n)b[k]={right:right,n:n};return b[k]}
/* the ✦ menu's Your 3 questions (world/guide-button.js): while the tour is stopped for good before its quiz */
function gdQuizOwed(){var o=G.gd.owed;return!G.on&&o&&G.gd.plan&&G.gd.plan.stopped?o.n:0}

/* ----- round 13, Don't say it! at run time (research_tour_entertainment.md 5.4). The game's clock runs with the voice: Pause, a drag of
   the progress bar and Ask (it pauses the tour) freeze it, so a window waits where it was. Each slip is the word lit in the narration of
   its span (the tour's word-time events: the recorded line's own word times, the live voice's, the device voice's estimate): its window
   opens at the word's start and closes at its end + the Tap window (2.5 s). A tap on Azu (or Heard it!) in a window catches that slip,
   once; more taps in it are ignored; a slip said again by Previous is the same slip (caught once; a missed one can be caught on the
   replay). A tap outside every window is a false alarm (counted, never punished), unless it is the start of three quick taps (a tickle:
   no count). The game ends quietly when the language changes or a step of its span leaves the tour. Unity: the AudioSource's time
   against the recorded word times, a PointerManipulator on the Azu element routed to the game during the span ----- */
function gdWordOff(){var d=G.gd.ds;if(d){clearInterval(d.iv);clearTimeout(d.pend)}G.gd.ds=null}
function gdWordRunning(){return G.on&&!G.paused&&!G.seekDrag&&!G.typing}
function gdWordClock(){var d=G.gd.ds;if(!d)return 0;var now=performance.now();if(gdWordRunning())d.clock+=now-d.tick;d.tick=now;return d.clock}
/* the game starts as its span's first step starts (the announce's chips still up: waiting is Let's play) */
function gdWordStart(b){gdWordOff();var it=S.guidance.items.dontsay,w=gdWordOf(S.lang||"en");if(!w)return null;
  var d=G.gd.ds={on:true,word:w.w,lang:S.lang||"en",span:(b&&b.span||w.span).slice(),endAfter:b&&b.endAfter||w.span[w.span.length-1],slips:{},order:[],wins:[],caught:0,falses:0,clock:0,tick:performance.now(),play:"pending",tapWin:+it.tapWin||2.5};
  d.iv=setInterval(function(){if(G.gd.ds!==d){clearInterval(d.iv);return}gdWordClock();gdWordWatch()},100);gdLog("event","Don't say it!: “"+d.word+"” in "+gdSpanWords(d.span));return d}
/* a span's step hidden (Length, a lens) or the language changed: the game ends quietly */
function gdWordWatch(){var d=G.gd.ds;if(!d||!d.on||!G.on)return;if((S.lang||"en")!==d.lang){gdWordQuiet("the language changed");return}
  if(d.span.some(function(id){return!S.guideSteps.some(function(s){return s.id===id})})){gdWordQuiet("a step of its span left the tour")}}
function gdWordQuiet(why){var d=G.gd.ds;if(!d)return;gdLog("event","Don't say it! ends quietly ("+why+")");gdWordOff();var P=G.gd.plan,b=P&&gdPlanBeat("dontsay",P);if(b){b.why=why}gdWordLook("quiet")}
/* a word lit in the narration: a slip when it is the word, in a step of the span */
on("markWord.after",function(k){var d=G.gd.ds;if(!d||!d.on||!G.on)return;var st=S.guideSteps[G.i];if(!st||d.span.indexOf(st.id)<0||!G.sayLine||G.sayLine.text!==stepText(st))return;
  if((S.lang||"en")!==d.lang){gdWordQuiet("the language changed");return}var w=G.words&&G.words[k];
  if(d.aside&&w&&/[.!?…]["”»)]*$/.test(w.w)){d.aside=false;setTimeout(function(){if(G.gd.ds===d)gdWordLook("aside")},250)}
  if(!w||!gdWordHit(w.w,d.word,d.lang))return;var key=st.id+":"+k,t=gdWordClock(),dur=Math.max(150,+wordMs(k)||300),sl=d.slips[key];
  if(!sl){sl=d.slips[key]={key:key,step:st.id,k:k,caught:false,text:stepText(st),phrase:gdWordPhrase(k),said:1};d.order.push(key)}else sl.said++;
  d.wins.push({key:key,s:t,e:t+dur+d.tapWin*1000});gdLog("event","Don't say it!: a slip (“"+w.w+"”, step "+st.id+", word "+(k+1)+(sl.said>1?", said again":"")+"): its window "+(dur/1000+d.tapWin).toFixed(1)+" s")});
/* the slip's phrase: a few words around it, in its sentence (said again with the device voice; shown with the word lit) */
function gdWordPhrase(k){var w=G.words||[],a=Math.max(sentenceStart(k),k-4),e=k;for(var j=k;j<Math.min(w.length,k+4);j++){e=j;if(/[.!?…,;:]["”»)]*$/.test(w[j].w)&&j>k)break}
  return{words:w.slice(a,e+1).map(function(x){return x.w}),at:k-a,rec:G.recWords&&G.recWords[k]?{from:Math.max(0,G.recWords[k][0]-1.2),to:G.recWords[k][1]+.3,url:G.recAudio&&G.recAudio.src||""}:null}}
/* a tap on Azu (or Heard it!): a catch in a window, ignored in a window already caught, else a false alarm (after 0.7 s: three quick taps tickle) */
function gdWordTap(how){var d=G.gd.ds;if(!d||!d.on)return false;var t=gdWordClock(),open=d.wins.filter(function(x){return t>=x.s&&t<=x.e});
  if(open.length){clearTimeout(d.pend);d.pend=null;var sl=open.map(function(x){return d.slips[x.key]}).filter(function(s){return!s.caught})[0];
    if(!sl){gdLog("event","Don't say it!: a tap in a window already caught (ignored)");return true}
    sl.caught=true;sl.how=how||"tap";d.caught++;d.aside=true;if(d.play==="pending")d.play="yes";gdLog("event","Don't say it!: caught "+d.caught+" ("+sl.key+", "+(how||"a tap on the mascot")+")");gdWordLook("caught");return true}
  clearTimeout(d.pend);d.pend=setTimeout(function(){if(G.gd.ds!==d)return;d.pend=null;d.falses++;gdLog("event","Don't say it!: a false alarm ("+d.falses+")");gdWordLook("nope")},700);return true}
/* Azu's taps during the game (mascot/azu2d.js's azu.tap, azu.tickled): the game's, not a poke */
on("azu.tap",function(m){var d=G.gd.ds;if(!d||!d.on||!G.on||m&&m.classList.contains("azfab"))return;return gdWordTap("a tap on the mascot")});
on("azu.tickled",function(){var d=G.gd.ds;if(d&&d.pend){clearTimeout(d.pend);d.pend=null;gdLog("event","Don't say it!: three quick taps, a tickle (no count)")}});
/* the result at the seam after the span (gdSeam): the beat's result card (guidance/present.js's gdWordResult) */
function gdWordEnd(){var d=G.gd.ds;if(!d)return;
  /* the last slip's window still open: the result waits for it to close (on the game's clock) */
  var t=gdWordClock(),open=d.wins.filter(function(x){return t<x.e&&!d.slips[x.key].caught});if(open.length&&!d.waitEnd){d.waitEnd=true;var ms=Math.max.apply(null,open.map(function(x){return x.e-t}));(function w(){if(G.gd.ds!==d||!G.on)return;var u=gdWordClock();if(open.some(function(x){return u<x.e&&!d.slips[x.key].caught})){setTimeout(w,120);return}gdWordEnd()})();return}
  d.on=false;clearInterval(d.iv);clearTimeout(d.pend);var P=G.gd.plan;if(P)P.wordScore={word:d.word,caught:d.caught,n:d.order.length,falses:d.falses};
  var it=gdItems().filter(function(x){return x.id==="dontsay"})[0]||S.guidance.items.dontsay;gdLog("event","Don't say it!: the span ended: "+d.caught+" of "+d.order.length+" caught, "+d.falses+" false alarm(s)");
  gdStart(it,"the result after its span",{pull:true,react:true})}
/* ----- round 13, Find it on the wall's clock and the phone settling (research 4.7): the point in the seek ring and the phone still 1.2 s
   (today's settle); a hint after Hint after (10 s), Azu shows it after Give up after (20 s); the clock stops while the tour is paused ----- */
on("phone.moved",function(){if(G.gd.find)G.gd.find.movedAt=performance.now()});
