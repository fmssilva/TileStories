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
function gdNewMemory(now){return{visits:1,visit:gdVisitMem(1,now),ever:{items:{}},used:{},met:false,prefs:{tips:"all",stay:false}}}
S.gdVisitor=gdNewMemory(0);
G.gd={t:0,rate:1,manual:false,cur:null,movedAt:null,touchAt:null,actAt:null,cardAt:null,wallAt:null,onWall:false,demo:false,last:null,next:null,log:[],between:false,skipKey:""};
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
  vm.shown++;vm.line=text;em.shown++;em.line=text;if(!answer)V.visit.starts.push(now)}
/* a cue's end writes the cue room's clock (never a tip's end or learning); a tip's end, how it ended and the learning */
function gdMemEnd(V,c,how,now,log){var vm=gdVM(c.item.id,V);vm.end=now;vm.how=how;
  if(c.cue){V.visit.cueEnd=now;if(c.item.untilSeen&&how==="seen"){vm.shown++;gdEM(c.item.id,V).shown++}return}
  /* round 7: a joke that answered pokes neither starts the breathing room nor counts as ignored */
  if(c.answer){if(how==="acted"||how==="dismissed"||how==="fewer")gdLearn(c,how,now,V,log);return}
  V.visit.lastEnd=now;gdLearn(c,how,now,V,log)}
/* a feature used: the items it retires are done for good (the items given, in the Editor's order) */
function gdMemUsed(V,f,now,items){V.used[f]=now;items.forEach(function(it){if(it.doneWhen===f){var em=gdEM(it.id,V);if(!em.done){em.done=true;em.by=f}}})}
/* a visit starts: a new visitor (Reset the visitor) or the same one back (New visit; a return after 30 min away); the clock goes on.
   With Remember across visits Off, the one back is a first visit again */
function gdVisitStart(returning,why){if(G.gd.cur)gdStop("a new visit");var now=gdNow(),keep=returning&&S.guidance.remember!==false;
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
  if(!c||c.x<40||c.x>W-40||c.y<90||c.y>H-170)return null;return{id:c.id,name:c.name,x:c.x,y:c.y,middle:c.where==="middle",tour:!!c.card,min:c.card?tourMinutes():null}}
/* the moment, as the scheduler reads it */
function gdOnWall(){return S.view==="card"&&S.stop==="closed"&&!G.on}
/* on a card: the sheet at Half or Full, or one of its pages (never the wall, Peek or the Demo Components page): where the cues live */
function gdOnCard(){return S.view==="card"?S.stop==="half"||S.stop==="full":S.view!=="demo"}
/* the context's numbers (the page's moment and the simulator's both start from them): the breathing room and the limits in force
   (Talkativeness: the row's value × its factor), the cue room, freshness, the learning, Lively, Solemn, Reduce Motion */
function gdCtxBase(){var g=S.guidance,md=gdTodayMD();return{on:!!g.on,breathing:gdRoomNow(),cueRoom:+g.cueRoom||0,reduce:!!S.reduce,limits:{perMinute:gdLimitNow(g.perMinute),perVisit:gdLimitNow(g.perVisit)},tau:+g.freshTau,
  learn:{ignore:g.learnIgnore!==false,dismiss:g.learnDismiss!==false},lively:S.guideCfg.life==="lively",solemn:!!(S.m&&S.m.solemn),aud:typeof aud==="function"?aud():"",
  today:md,solemnDay:gdSolemnDay(md)?gdDayWords(md):null}}
/* round 7: on a Solemn point now: Specific Marker › Solemn point marks the marker's point (Ribeira Palace): on its card, in a tour, or on
   the wall with that point in the middle (or no point in the middle); another point in the middle of the wall is not it */
function gdSolemnNow(){if(!(S.m&&S.m.solemn))return false;if(S.view==="card"&&S.stop==="closed"&&!G.on){var c=gdCentral();return!c||c.id===(S.m.poi||"painting")}return true}
function gdContext(){var t=-Infinity,mv=G.gd.movedAt,wa=G.gd.wallAt,st=ph.querySelector(".stv"),sk=st&&(st.className.match(/\bst-(\w+)/)||[])[1];
  var speaking=!!(G.utter||G.timer||G.recAudio);
  var onCard=gdOnCard(),mo=G.gd.moment||null;
  return Object.assign(gdCtxBase(),{scrollAt:G.gd.scrollAt,onCard:onCard,cues:onCard?cueTargets(mo==="open"):{},moment:mo,
    demo:!!G.gd.demo,showing:G.gd.cur?G.gd.cur.item.label:null,
    remark:ph.querySelector(".azbub")?"Azu's bubble":null,
    pokes:S.gdVisitor.visit.pokes||0,idleSince:Math.max(G.gd.actAt==null?t:G.gd.actAt,mv==null?t:mv,S.gdVisitor.visit.at||0),
    speaking:speaking,state:sk?GD_STATES[sk]||sk:null,
    panel:S.settingsOpen?"Settings":G.cam&&G.cam.open?"Ask about the view":G.fab&&G.fab.menu?"the ✦ menu":S.contentsOpen?"Contents":null,
    dockFull:!!(G.on&&typeof dockOn==="function"&&dockOn()&&S.dockH==="full"),solemn:gdSolemnNow(),
    /* the tour: between two steps (as it moves on, nothing is said), its narration so far (since it started or the last Moment) and its last step next */
    tour:G.on?{between:mo==="between"||!!G.gd.between&&!speaking,step:G.i,talked:Math.max(0,(performance.now()-(G.talkFrom||performance.now()))/1000),toEnd:!!(S.guideSteps[G.i+1]&&S.guideSteps[G.i+1].act==="end")}:null,
    movedAt:mv,touchAt:G.gd.touchAt,cardAt:S.view==="card"&&S.stop!=="closed"?G.gd.cardAt:null,
    onWall:gdOnWall(),where:S.view!=="card"?"the phone shows "+S.view:G.on?"the tour":S.stop!=="closed"?"a card is open":"",
    marker:gdMarker(),gbtn:gdGbtn(),central:gdCentral(),covers:{},stillSince:Math.max(mv==null?t:mv,wa==null?t:wa),returning:S.gdVisitor.visits>1,tips:gdPrefs().tips})}
/* the visitor's Tips settings (an older memory has none: the defaults) */
function gdPrefs(V){V=V||S.gdVisitor;return V.prefs||(V.prefs={tips:"all",stay:false})}
/* how long a tip stays: by its words, 4 to 9 s (the visit's clock); with the clock fast (a scene, a demo) it stays at least 0.6 to 1.5 s of real time, to be seen */
function gdOnScreen(it,text){if(it.onScreen==="secs"&&+it.onScreenS>0)return +it.onScreenS*1000;var n=String(text||"").split(/\s+/).filter(Boolean).length;return Math.max(4000,Math.min(9000,3000+n*350))}
/* the slots of its line: the point it is about and the minutes of that point's tour */
/* round 6: {year} the point's year; a line with variants says the next in turn (n: how many times it has shown, so never the same twice in a row) */
function gdYearOf(id){var p=(S.wallPois||[]).filter(function(x){return x.id===id})[0];return p&&p.year||""}
/* round 7: every line's slots also carry today (a seasonal line), the variants this visitor has seen (A joke: the least seen) and {thing}
   (the point the treasure hunt asks for: Ask about the view's own pick, camHuntPick) */
function gdSlots(it,V,c,items){V=V||S.gdVisitor;var o=gdSlotsOf(it,V,c,items);o.today=gdTodayMD();if(it.pick==="least")o.seen=gdEverOf(V,it.id).seen||{};if(it.tap==="hunt"||it.button==="hunt")o.thing=gdHuntThing();return o}
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
  var p=(S.wallPois||[]).filter(function(x){return x.card})[0];return{id:p&&p.id,point:p?L(p.name):"",min:tourMinutes(),year:p?gdYearOf(p.id):"",n:nn}}
/* spoken (_3.8 decision 2): App as a bubble only; Content spoken once the visitor has heard the guide this visit, else a bubble */
/* round 6: Spoken per item (As the kind / A bubble only / Spoken once the guide is heard); never while a screen reader is on (Preview & Test >
   The visitor > Screen reader on, the Lab's stand-in for VoiceOver / TalkBack): the bubble is announced politely instead */
function gdSpoken(it,o){if(S.ui.gdSR)return false;if(o&&o.spoken!=null)return!!o.spoken;var h=!!S.gdVisitor.visit.heard;return it.spoken==="heard"?h:it.spoken==="kind"&&it.kindSpoken==="heard"&&h}
/* a tip starts (the scheduler's choice, or pulled: an Editor demo, which never writes the memory) and ends */
function gdStart(it,why,o){o=o||{};if(G.gd.cur)gdStop("another tip");if(it.form==="cue")return gdCueStart(it,why,o);var now=gdNow(),ret=o.ret!=null?!!o.ret:S.gdVisitor.visits>1&&!o.first,slots=gdSlots(it),key=o.text!=null?null:gdLineKey(it,ret,slots),text=o.text!=null?o.text:gdLine(it,ret,slots),spoken=gdSpoken(it,o);
  G.gd.cur={item:it,answer:!!o.answer,at:now,until:now+(o.ms||gdOnScreen(it,text)),realMin:G.gd.manual?0:performance.now()+(G.gd.rate>1?Math.max(600,15000/G.gd.rate):0),pull:!!o.pull,why:why||"",text:text,slots:slots,ret:ret,spoken:spoken};
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
on("tourStep.ended",function(st){S.gdVisitor.visit.heard=true;if(st&&st.act==="intro"&&!S.gdVisitor.met){S.gdVisitor.met=true;gdLog("event",GD_EVENTS.intro)}G.gd.between=true;
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
on("next.answer",function(){if(!G.on||G.paused||noDraw||G.gd.demo)return;var c=G.gd.cur;if(c&&c.item.form==="moment"){G.gd.hold=true;return true}if(c)return;
  /* (the tour moving on ends the line it was saying: a Next pressed mid-line is not a sentence still being said) */
  G.gd.moment="between";var r;try{var ctx=gdContext();ctx.speaking=false;r=gdPick(gdItems(),S.gdVisitor,ctx,gdNow())}finally{G.gd.moment=null}
  G.gd.last=r;if(r.item&&r.item.form==="moment"){G.gd.hold=true;gdStart(r.item,r.why);return true}
  var key="between|"+r.why.replace(/\d+(\.\d+)?/g,"#");if(key!==G.gd.skipKey){G.gd.skipKey=key;gdLog("skipped","between two steps: "+r.why)}});
on("stopTour.before",function(){G.gd.hold=false;var c=G.gd.cur;if(c&&c.item.form==="moment")gdStop("the tour stopped")});
/* a Moment ends (mascot/azu2d.js): a tap on its prop is acted, 8 s with no tap ignored */
on("moment.ended",function(k,done){var c=G.gd.cur;if(c&&c.item.form==="moment")gdStop(done?"tapped: Azu "+(k==="cookie"?"eats the cookie":"drinks")+", and the tour goes on":"8 s with no tap: the tour goes on",done?"acted":"ignored")});
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
function gdEnds(c,now,W){var it=c.item;
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
  {id:"idle",name:"Idle",says:"an idle visitor: it holds the phone on Ribeira Palace, never touches and ignores every tip",
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
  var central=function(){var p=w.where==="wall"&&!moving()&&w.point?poi(w.point):null;return p?{id:p.id,name:L(p.name),x:195,y:380,middle:true,tour:!!p.card,min:p.card?tourMinutes():null}:null};
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
