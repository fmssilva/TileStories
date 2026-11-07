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
   visit clock Time.unscaledTime since the visit started, the time away from OnApplicationPause. */
var GD_VISIT_GAP=30*60000;
function gdVisitMem(n,now){return{n:n,at:now,lastEnd:null,cueEnd:null,cards:0,items:{},starts:[],away:0,ignoredRun:0,breathMul:1,rest:{},fewer:{},heard:false,pokes:0}}
function gdNewMemory(now){return{visits:1,visit:gdVisitMem(1,now),ever:{items:{}},used:{},met:false}}
S.gdVisitor=gdNewMemory(0);
G.gd={t:0,rate:1,manual:false,cur:null,movedAt:null,touchAt:null,actAt:null,cardAt:null,wallAt:null,onWall:false,demo:false,last:null,next:null,log:[],between:false,skipKey:""};
function gdNow(){return G.gd.t}
/* the log: every decision (shown, skipped, ended) and event, with the visit's time; the Editor shows the last 30 */
function gdLog(what,why){var v=S.gdVisitor.visit,t=Math.round(gdNow());G.gd.log.push({t:t,vt:t-Math.round(v.at||0),v:v.n,what:what,why:why||""});if(G.gd.log.length>400)G.gd.log.shift();emit("gdLog.after")}
function gdVM(id){var v=S.gdVisitor.visit.items;return v[id]||(v[id]={shown:0,last:null,end:null,dismissed:0,acted:0,how:"",line:""})}
function gdEM(id){var v=S.gdVisitor.ever.items;return v[id]||(v[id]={shown:0,done:false,by:"",last:null,line:"",dismissed:0,acted:0})}
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
function gdContext(){var t=-Infinity,mv=G.gd.movedAt,wa=G.gd.wallAt,g=S.guidance,st=ph.querySelector(".stv"),sk=st&&(st.className.match(/\bst-(\w+)/)||[])[1];
  var speaking=!!(G.utter||G.timer||G.recAudio);
  var onCard=gdOnCard(),mo=G.gd.moment||null;
  return{on:!!g.on,breathing:+g.breathing,cueRoom:+g.cueRoom||0,reduce:!!S.reduce,scrollAt:G.gd.scrollAt,onCard:onCard,cues:onCard?cueTargets(mo==="open"):{},moment:mo,limits:{perMinute:+g.perMinute,perVisit:+g.perVisit},tau:+g.freshTau,learn:{ignore:g.learnIgnore!==false,dismiss:g.learnDismiss!==false},
    demo:!!G.gd.demo,showing:G.gd.cur?G.gd.cur.item.label:null,
    remark:ph.querySelector(".azbub")?"Azu's bubble":null,
    pokes:S.gdVisitor.visit.pokes||0,idleSince:Math.max(G.gd.actAt==null?t:G.gd.actAt,mv==null?t:mv,S.gdVisitor.visit.at||0),
    lively:S.guideCfg.life==="lively",solemn:!!(S.m&&S.m.solemn),
    speaking:speaking,state:sk?GD_STATES[sk]||sk:null,
    panel:S.settingsOpen?"Settings":G.cam&&G.cam.open?"Ask about the view":G.fab&&G.fab.menu?"the ✦ menu":S.contentsOpen?"Contents":null,
    dockFull:!!(G.on&&typeof dockOn==="function"&&dockOn()&&S.dockH==="full"),tour:G.on?{between:!!G.gd.between&&!speaking,step:G.i}:null,
    movedAt:mv,touchAt:G.gd.touchAt,cardAt:S.view==="card"&&S.stop!=="closed"?G.gd.cardAt:null,
    onWall:gdOnWall(),where:S.view!=="card"?"the phone shows "+S.view:G.on?"the tour":S.stop!=="closed"?"a card is open":"",
    marker:gdMarker(),gbtn:gdGbtn(),central:gdCentral(),covers:{},stillSince:Math.max(mv==null?t:mv,wa==null?t:wa),returning:S.gdVisitor.visits>1}}
/* how long a tip stays: by its words, 4 to 9 s (the visit's clock); with the clock fast (a scene, a demo) it stays at least 0.6 to 1.5 s of real time, to be seen */
function gdOnScreen(it,text){var n=String(text||"").split(/\s+/).filter(Boolean).length;return Math.max(4000,Math.min(9000,3000+n*350))}
/* the slots of its line: the point it is about and the minutes of that point's tour */
function gdSlots(it){if(it.about==="point"){var n=gdItems().filter(function(o){return o.group===it.group}).reduce(function(a,o){return a+gdVisitOf(S.gdVisitor,o.id).shown},0);
    return{id:it.point,point:it.pointName||"",card:!!it.card,n:n}}
  if(it.about!=="middle")return{};var c=gdCentral();if(c&&c.tour)return{id:c.id,point:c.name,min:c.min};
  var p=(S.wallPois||[]).filter(function(x){return x.card})[0];return{id:p&&p.id,point:p?L(p.name):"",min:tourMinutes()}}
/* spoken (_3.8 decision 2): App as a bubble only; Content spoken once the visitor has heard the guide this visit, else a bubble */
function gdSpoken(it,o){if(o&&o.spoken!=null)return!!o.spoken;return it.spoken==="kind"&&it.kindSpoken==="heard"&&!!S.gdVisitor.visit.heard}
/* a tip starts (the scheduler's choice, or pulled: an Editor demo, which never writes the memory) and ends */
function gdStart(it,why,o){o=o||{};if(G.gd.cur)gdStop("another tip");if(it.form==="cue")return gdCueStart(it,why,o);var now=gdNow(),ret=o.ret!=null?!!o.ret:S.gdVisitor.visits>1&&!o.first,slots=gdSlots(it),text=gdLine(it,ret,slots),spoken=gdSpoken(it,o);
  G.gd.cur={item:it,at:now,until:now+(o.ms||gdOnScreen(it,text)),realMin:G.gd.manual?0:performance.now()+(G.gd.rate>1?Math.max(600,15000/G.gd.rate):0),pull:!!o.pull,why:why||"",text:text,slots:slots,ret:ret,spoken:spoken};
  if(!o.pull){var vm=gdVM(it.id),em=gdEM(it.id);vm.shown++;vm.last=now;vm.end=null;vm.line=text;em.shown++;em.last=now;em.line=text;S.gdVisitor.visit.starts.push(now)}
  G.gd.skipKey="";gdLog("shown",(o.pull?"(pulled) ":"")+(why||it.label)+(spoken?" · spoken":""));gdPresent(G.gd.cur);if(spoken)gdVoice(G.gd.cur);return G.gd.cur}
/* how: "acted", "dismissed", "fewer", "ignored", or "" (the moment ended it); round 1's true is a dismissal */
/* a cue starts: its look on its element (card/hints.js's cueStart gives how long it plays; the Locate label: until seen); its memory
   (this visit, ever, the card it showed on) at its start, the label's once it has been seen; no bubble, no mascot, not a tip's start */
function gdCueStart(it,why,o){var now=gdNow(),c={item:it,at:now,until:now,realMin:0,pull:!!o.pull,why:why||"",text:it.says?TXT(it.says):"",slots:{},cue:true},d=cueStart(c);
  if(d==null){gdLog("skipped",it.label+": its part is not on the screen");return null}
  G.gd.cur=c;if(!o.pull){var vm=gdVM(it.id),em=gdEM(it.id);vm.last=now;vm.end=null;em.last=now;vm.card=S.gdVisitor.visit.cards||0;if(!it.untilSeen){vm.shown++;em.shown++}}
  G.gd.skipKey="";gdLog("shown",(o.pull?"(pulled) ":"")+(why||it.label)+" · a cue");if(!isFinite(d)&&o.ms)d=o.ms;
  c.until=now+d;c.realMin=G.gd.manual||!isFinite(d)?0:performance.now()+d;return c}
function gdStop(why,how){var c=G.gd.cur;if(!c)return;if(how===true)how="dismissed";how=how||"";G.gd.cur=null;var now=gdNow();
  /* a cue ends: seen (it played, or the label was seen 4 s) or by the moment; it writes the cue's end (the cue room), never a tip's */
  if(c.cue){if(!c.pull){var cv=gdVM(c.item.id);cv.end=now;cv.how=how;S.gdVisitor.visit.cueEnd=now;if(c.item.untilSeen&&how==="seen"){cv.shown++;gdEM(c.item.id).shown++}}
    gdLog("ended",c.item.label+": "+(why||"it played")+(how?" ("+how+")":""));cueStop(c,how);return}
  if(c.spoken&&G.sayLine&&G.sayLine.text===c.text)stopSpeech();
  if(!c.pull){var vm=gdVM(c.item.id);vm.end=now;vm.how=how;S.gdVisitor.visit.lastEnd=now;gdLearn(c,how,now)}
  gdLog("ended",c.item.label+": "+(why||"its time is up")+(how?" ("+how+")":""));gdUnpresent(why)}
/* learning from the visitor (_3.8 section 3.3, 5): three ignored in a row double the breathing room for the visit (up to ×4), an acted
   item resets it; a dismissed App item waits ×3 its Again after (the scheduler reads its how); a dismissed Content item rests its group
   5 min; Fewer like this? rests the group for the visit. Essential items (the opening) are greetings: their time running out is not ignoring */
function gdLearn(c,how,now){var it=c.item,v=S.gdVisitor.visit,vm=gdVM(it.id),em=gdEM(it.id),g=gdGroupOf(it),gn=g?g.name:it.group;
  if(how==="acted"){vm.acted++;em.acted++;if(v.breathMul>1)gdLog("learned","an acted tip: the breathing room back to "+S.guidance.breathing+" s");v.ignoredRun=0;v.breathMul=1;return}
  if(how==="ignored"){if(it.essential)return;v.ignoredRun++;var m=S.guidance.learnIgnore!==false?Math.min(4,Math.pow(2,Math.floor(v.ignoredRun/3))):1;
    if(m!==v.breathMul){v.breathMul=m;gdLog("learned",v.ignoredRun+" ignored in a row: the breathing room ×"+m+" ("+S.guidance.breathing*m+" s) for the visit")}return}
  if(how==="dismissed"||how==="fewer"){vm.dismissed++;em.dismissed++;
    if(how==="fewer"){v.rest[it.group]=GD_FOREVER;v.fewer[it.group]=true;gdLog("learned","Fewer like this: "+gn+" rests for the visit");return}
    if(S.guidance.learnDismiss===false)return;
    if(it.kind==="app"){if(+it.again>0)gdLog("learned",it.label+" dismissed: it waits ×3 its Again after ("+3*it.again+" s)");return}
    v.rest[it.group]=now+GD_REST_MS;gdLog("learned",it.label+" dismissed: "+gn+" rests 5 min")}}
/* the events */
on("phone.moved",function(){G.gd.movedAt=gdNow()});
["pointerdown","pointerup"].forEach(function(t){ph.addEventListener(t,function(){G.gd.touchAt=G.gd.actAt=gdNow()},{capture:true,passive:true})});
/* idle: no touch, no wheel, no key (and the phone still: movedAt) */
["wheel","keydown"].forEach(function(t){ph.addEventListener(t,function(){G.gd.actAt=gdNow()},{capture:true,passive:true})});
/* Azu poked (mascot/azu2d.js), counted this visit; the ✦ menu opened (world/guide-button.js): The ✦ is done */
on("mascot.poked",function(){var v=S.gdVisitor.visit;v.pokes=(v.pokes||0)+1;gdLog("event",GD_EVENTS.poked+" ("+v.pokes+" this visit)")});
on("menu.opened",function(how){gdUsed("menu",how)});
/* a feature used (a milestone of the stage): the items it retires are done for good; a tip that teaches it, showing now, was acted on */
function gdUsed(f,how){var now=gdNow(),c=G.gd.cur;S.gdVisitor.used[f]=now;
  gdItems().forEach(function(it){if(it.doneWhen===f){var em=gdEM(it.id);if(!em.done){em.done=true;em.by=f}}});
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
on("startTour.before",function(){if(G.gd.cur&&!G.gd.cur.pull)gdStop("the tour started")});on("runStep.before",function(){G.gd.between=false});on("stopTour.after",function(){G.gd.between=false});
/* the loop: the wall seen since when, the tip on screen ends, or the scheduler is asked (a choice that would cover its target is asked again without it) */
function gdTick(){if(noDraw)return;var now=gdNow(),w=gdOnWall();if(w&&!G.gd.onWall)G.gd.wallAt=now;G.gd.onWall=w;
  var c=G.gd.cur;if(c){var it=c.item;if(it.screen==="wall"&&!w){gdStop("the wall left the screen");return}
    /* a cue ends with its card; the Locate label also when its pin goes (Peek) */
    if(c.cue&&(!gdOnCard()||it.about==="locate"&&!(c.pull||cueTargets(false).locate))){gdStop("its card left the screen");return}
    if(!c.pull){if(it.about==="marker"&&!gdMarker()){gdStop("its marker left the screen");return}
      if(it.about==="middle"||it.about==="point"){var cc=gdCentral();if(!cc||cc.id!==c.slots.id){gdStop("its point left the middle");return}}
      if(it.about==="gbtn"&&!gdGbtn()){gdStop("the ✦ left the screen");return}}
    if(c.ask){if(now>=c.askUntil)gdStop("Fewer like this? not answered","ignored");return}
    if(now>=c.until&&performance.now()>=c.realMin)gdStop(c.cue?"it played":"its time is up",c.cue?"seen":"ignored");return}
  if(G.gd.demo&&G.gd.demoOwner!==G.demoSeq){G.gd.demo=false;gdChip(null)}if(G.gd.demo)return;
  var ctx=gdContext(),items=gdItems(),r=gdPick(items,S.gdVisitor,ctx,now);
  for(var k=0;k<4&&r.item;k++){var cov=gdCovers(r.item);if(!cov)break;ctx.covers[r.item.id]=cov;r=gdPick(items,S.gdVisitor,ctx,now)}
  G.gd.last=r;G.gd.ctx=ctx;if(r.item){gdStart(r.item,r.why);return}
  /* a skip is logged when its reason changes (the numbers aside) */
  var key=r.why.replace(/\d+(\.\d+)?/g,"#");if(key!==G.gd.skipKey){G.gd.skipKey=key;gdLog("skipped",r.why)}}
/* a pull (_3.8 section 3.3: past every gate but one at a time): the item at once, the tip showing gives way; it writes no memory.
   Tap on Azu = Points to the ✦ pulls The ✦ (false when there is no ✦ or no Azu beside it to point) */
function gdPull(id,why){var it=gdInstances(id)[0];if(!it)return false;if(it.about==="gbtn"&&!(ph.querySelector(".gbtn")&&typeof azuEl==="function"&&azuEl()&&gbAzu()&&gbOn()))return false;
  if(G.gd.cur)gdStop("the visitor asked for "+it.label);var ab=ph.querySelector(".azbub");if(ab)ab.remove();gdStart(it,why||"asked by the visitor",{pull:true});return true}
function gdAdvance(ms){for(var k=0;k<ms;k+=250){G.gd.t+=Math.min(250,ms-k);gdTick()}}
setInterval(function(){if(G.gd.manual||noDraw)return;G.gd.t+=250*G.gd.rate;gdTick()},250);
gdLog("visit","a first visit");
