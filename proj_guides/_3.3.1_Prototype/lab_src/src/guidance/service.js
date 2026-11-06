/* Guidance, the service: the visit's clock, the visitor's memory, the events, and the loop that asks gdPick 4 times a second and hands
   its choice to guidance/present.js. The clock (G.gd.t, ms of the visit) runs at G.gd.rate (×1; the Editor's ×10 demos and the scenes
   run it faster); with G.gd.manual a test moves it by hand (gdAdvance), so every check is deterministic. The memory (S.gdVisitor) holds,
   per item, this visit's and the ever counts, the times shown and ended, and done (retired for good); New visit keeps the ever part,
   Reset the visitor clears both. The events (_3.8 section 3.2): visit started (gdVisitStart), a marker in view (gdMarker), the phone
   moving / still (the wall's phone.moved), card opened (card.opened: a marker, a point with a card, a remark). Unity: GuidanceService
   (a MonoBehaviour ticking at 4 Hz, fed by the event buses: the AR camera's motion, the card's open), the memory as JSON in
   persistentDataPath, the visit clock Time.unscaledTime since the visit started. */
S.gdVisitor={visits:1,visit:{n:1,at:0,lastEnd:null,items:{}},ever:{items:{}},used:{}};
G.gd={t:0,rate:1,manual:false,cur:null,movedAt:null,wallAt:null,onWall:false,demo:false,last:null,next:null,log:[]};
function gdNow(){return G.gd.t}
function gdLog(what,why){G.gd.log.push({t:Math.round(gdNow()),what:what,why:why||""});if(G.gd.log.length>30)G.gd.log.shift();emit("gdLog.after")}
function gdVM(id){var v=S.gdVisitor.visit.items;return v[id]||(v[id]={shown:0,last:null,end:null,dismissed:0})}
function gdEM(id){var v=S.gdVisitor.ever.items;return v[id]||(v[id]={shown:0,done:false,by:""})}
/* a visit starts: a new visitor (Reset the visitor) or the same one back (New visit); the clock goes on */
function gdVisitStart(returning){if(G.gd.cur)gdStop("a new visit");var now=gdNow();
  if(returning){S.gdVisitor.visits++;S.gdVisitor.visit={n:S.gdVisitor.visits,at:now,lastEnd:null,items:{}}}
  else S.gdVisitor={visits:1,visit:{n:1,at:now,lastEnd:null,items:{}},ever:{items:{}},used:{}};
  G.gd.wallAt=G.gd.onWall?now:null;gdLog("visit",returning?"the visitor came back (visit "+S.gdVisitor.visits+")":"a first visit")}
function gdNewVisit(){gdVisitStart(true)}
function gdResetVisitor(){gdVisitStart(false)}
/* the nearest marker with a card on the screen (its circle's centre, in the phone's pixels), clear of the screen's edges and foot */
function gdMarker(){if(S.view!=="card"||typeof wallMarkers!=="function")return null;var W=ph.offsetWidth,H=ph.offsetHeight,best=null;
  wallMarkers().forEach(function(m){if(!m.card)return;var s=poiScreen(m);if(s.x<40||s.x>W-40||s.y<90||s.y>H-170)return;var d=Math.hypot(s.x-W/2,s.y-H*.45);
    if(!best||d<best.d)best={id:m.id,name:L(m.name),x:s.x,y:s.y,d:d}});return best}
/* the moment, as the scheduler reads it */
function gdOnWall(){return S.view==="card"&&S.stop==="closed"&&!G.on}
function gdContext(){var t=-Infinity,mv=G.gd.movedAt,wa=G.gd.wallAt;
  return{on:!!S.guidance.on,breathing:+S.guidance.breathing,demo:!!G.gd.demo,showing:G.gd.cur?G.gd.cur.item.label:null,
    remark:G.fab&&G.fab.bub||ph.querySelector(".azfabbub")?"Azu's remark":ph.querySelector(".azbub")?"Azu's bubble":null,
    speaking:!!(G.utter||G.timer||G.recAudio),
    panel:S.settingsOpen?"Settings":G.cam&&G.cam.open?"Ask about the view":G.fab&&G.fab.menu?"the ✦ menu":G.on?"the tour":S.contentsOpen?"Contents":null,
    movedAt:mv,onWall:gdOnWall(),where:S.view!=="card"?"the phone shows "+S.view:S.stop!=="closed"?"a card is open":G.on?"the tour":"",
    marker:gdMarker(),stillSince:Math.max(mv==null?t:mv,wa==null?t:wa),returning:S.gdVisitor.visits>1}}
/* how long a tip stays: by its words, 4 to 9 s (the visit's clock); with the clock fast (a scene, a demo) it stays at least 0.6 to 1.5 s of real time, to be seen */
function gdOnScreen(it,text){var n=String(text||"").split(/\s+/).filter(Boolean).length;return Math.max(4000,Math.min(9000,3000+n*350))}
/* a tip starts (the scheduler's choice, or pulled: an Editor demo, which never writes the memory) and ends */
function gdStart(it,why,o){o=o||{};if(G.gd.cur)gdStop("another tip");var now=gdNow(),ret=S.gdVisitor.visits>1&&!o.first,text=gdLine(it,ret);
  G.gd.cur={item:it,at:now,until:now+(o.ms||gdOnScreen(it,text)),realMin:G.gd.manual?0:performance.now()+(G.gd.rate>1?Math.max(600,15000/G.gd.rate):0),pull:!!o.pull,why:why||"",text:text};
  if(!o.pull){var vm=gdVM(it.id),em=gdEM(it.id);vm.shown++;vm.last=now;vm.end=null;em.shown++}
  gdLog("shown",(o.pull?"(pulled) ":"")+(why||it.label));gdPresent(G.gd.cur);return G.gd.cur}
function gdStop(why,dismissed){var c=G.gd.cur;if(!c)return;G.gd.cur=null;var now=gdNow();
  if(!c.pull){var vm=gdVM(c.item.id);vm.end=now;if(dismissed)vm.dismissed++;S.gdVisitor.visit.lastEnd=now}
  gdLog("ended",c.item.label+": "+(why||"its time is up"));gdUnpresent(why)}
/* the events */
on("phone.moved",function(){G.gd.movedAt=gdNow()});
on("card.opened",function(how){var now=gdNow();S.gdVisitor.used.card=now;gdItems().forEach(function(it){if(it.doneWhen==="card"){var em=gdEM(it.id);if(!em.done){em.done=true;em.by="card"}}});
  gdLog("event",GD_EVENTS.card+" ("+how+")");if(G.gd.cur)gdStop("the visitor opened a card")});
/* the loop: the wall seen since when, the tip on screen ends, or the scheduler is asked */
function gdTick(){if(noDraw)return;var now=gdNow(),w=gdOnWall();if(w&&!G.gd.onWall)G.gd.wallAt=now;G.gd.onWall=w;
  var c=G.gd.cur;if(c){if(c.item.screen==="wall"&&!w){gdStop("the wall left the screen");return}
    if(c.item.about==="marker"&&!c.pull&&!gdMarker()){gdStop("its marker left the screen");return}
    if(now>=c.until&&performance.now()>=c.realMin)gdStop("its time is up");return}
  if(G.gd.demo&&G.gd.demoOwner!==G.demoSeq){G.gd.demo=false;gdChip(null)}if(G.gd.demo)return;var r=gdPick(gdItems(),S.gdVisitor,gdContext(),now);G.gd.last=r;if(r.item)gdStart(r.item,r.why)}
function gdAdvance(ms){for(var k=0;k<ms;k+=250){G.gd.t+=Math.min(250,ms-k);gdTick()}}
setInterval(function(){if(G.gd.manual||noDraw)return;G.gd.t+=250*G.gd.rate;gdTick()},250);
gdLog("visit","a first visit");
