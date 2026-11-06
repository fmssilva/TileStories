/* Guidance, the scheduler (_3.8 section 3.3): one pure function, gdPick(items, visitor, context, now) → {item, why} | {none, why}.
   It reads only its arguments (no page, no clock of its own), so a test gives it a fake clock and fake memory. Round 1: the gates (one
   at a time, Azu's remarks and any bubble counted; never over a sentence; quiet while a panel is open or during the tour; quiet while
   the phone moves and 0.7 s after), the breathing room between any two (Essential skips it), and each item's own rules (On, Done when,
   Shows, Again after, its screen, its target on screen, the phone still, an In order group's sequence and its gap). Ties: Essential
   first, then importance, then the Editor's order. Unity: GuidanceScheduler, plain C# (Pick(state, now)), EditMode tests on the same
   cases. */
var GD_MOVE_MS=700;
function gdVisitOf(visitor,id){return visitor.visit.items[id]||{shown:0,last:null,end:null,dismissed:0}}
function gdEverOf(visitor,id){return visitor.ever.items[id]||{shown:0,done:false,by:""}}
function gdLeft(ms){return Math.max(1,Math.ceil(ms/1000))+" s left"}
/* the gates: fixed, no option breaks them (a pulled item passes the limits, never the gates) */
function gdGate(ctx,now){if(!ctx.on)return"Guidance is Off";if(ctx.demo)return"an Editor demo is showing";
  if(ctx.showing)return"one at a time: the "+ctx.showing+" is showing";if(ctx.remark)return"one at a time: "+ctx.remark+" is showing";
  if(ctx.speaking)return"never over a sentence: the guide is speaking";if(ctx.panel)return"quiet while "+ctx.panel+" is open";
  if(ctx.movedAt!=null&&now-ctx.movedAt<GD_MOVE_MS)return"quiet while the phone moves (and 0.7 s after)";return null}
/* the item before this one in its In order group (the On ones only): items come in the Editor's order, each with its group and the group's mode */
function gdPrevOf(it,items){if(it.groupMode!=="order")return null;var i=items.indexOf(it);
  for(var j=i-1;j>=0;j--){var p=items[j];if(p.group===it.group&&p.on)return p}return null}
/* one item: {ok, why} or {no: why} */
function gdCheck(it,items,visitor,ctx,now){var vm=gdVisitOf(visitor,it.id),em=gdEverOf(visitor,it.id),why=[];
  if(!it.on)return{no:"Off"};
  if(it.doneWhen&&em.done)return{no:"done for good ("+GD_EVENTS[em.by||it.doneWhen]+")"};
  if(it.shows==="once"&&em.shown>=1)return{no:"shown once ever"};
  if(it.shows==="visit"&&vm.shown>=1)return{no:"shown once this visit"};
  if((it.shows==="upto"||it.shows==="until")&&vm.shown>=+it.max)return{no:"shown "+vm.shown+" of "+it.max+" this visit"};
  if(it.screen==="wall"&&!ctx.onWall)return{no:"not on the wall ("+(ctx.where||"a card is open")+")"};
  if(it.about==="marker"){if(!ctx.marker)return{no:"no marker in view"};why.push(ctx.marker.name+" in view")}
  var p=gdPrevOf(it,items);if(p){var pm=gdVisitOf(visitor,p.id);if(!pm.shown)return{no:"after the "+p.label+" (not shown yet this visit)"};
    var since=now-pm.last;if(since<+it.afterPrev*1000)return{no:it.afterPrev+" s after the "+p.label+": "+gdLeft(+it.afterPrev*1000-since)};if(+it.afterPrev>0)why.push(Math.floor(since/1000)+" s after the "+p.label)}
  if(+it.stillFor>0){var st=now-ctx.stillSince;if(!(st>=+it.stillFor*1000))return{no:"the phone still "+(isFinite(st)?(st/1000).toFixed(1):"0")+" s (it waits for "+it.stillFor+" s)"};why.push("the phone still "+(isFinite(st)?(st/1000).toFixed(1)+" s":"")) }
  if(!it.essential&&visitor.visit.lastEnd!=null&&now-visitor.visit.lastEnd<ctx.breathing*1000)return{no:"breathing room "+ctx.breathing+" s: "+gdLeft(ctx.breathing*1000-(now-visitor.visit.lastEnd))};
  if(+it.again>0&&vm.end!=null&&it.shows!=="once"&&it.shows!=="visit"&&now-vm.end<+it.again*1000)return{no:"again after "+it.again+" s: "+gdLeft(+it.again*1000-(now-vm.end))};
  if(it.when==="visit")why.unshift("the visit started ("+(ctx.returning?"a returning visitor":"a first visit")+")");
  if(it.shows==="upto"||it.shows==="until")why.push("shown "+vm.shown+" of "+it.max+" this visit");
  if(it.essential)why.push("Essential: it skips the breathing room");
  return{ok:true,why:why.join(", ")}}
function gdPick(items,visitor,ctx,now){var g=gdGate(ctx,now);if(g)return{none:true,why:g};
  var list=items.map(function(it,i){return{it:it,i:i}}).sort(function(a,b){return(b.it.essential?1:0)-(a.it.essential?1:0)||(+b.it.importance)-(+a.it.importance)||a.i-b.i});
  var skips=[];for(var k=0;k<list.length;k++){var it=list[k].it,r=gdCheck(it,items,visitor,ctx,now);if(r.ok)return{item:it,why:it.label+": "+r.why};skips.push(it.label+": "+r.no)}
  return{none:true,why:skips.length?skips.join(" · "):"no guidance items"}}
