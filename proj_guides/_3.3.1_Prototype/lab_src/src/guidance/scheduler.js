/* Guidance, the scheduler (_3.8 section 3.3): one pure function, gdPick(items, visitor, context, now) → {item, why} | {none, why}.
   It reads only its arguments (no page, no clock of its own), so a test gives it a fake clock and fake memory; gdWhyNot answers for one
   item in the same words. In order:
   1. The gates (fixed, no option breaks them; a pulled item passes the limits, never the gates): Guidance On; one at a time (Azu's
      remarks and any bubble counted); never over a sentence; quiet while a system state shows (Loading & states); a panel open
      (Settings, Ask about the view, the ✦ menu, Contents) or the dock at Full; the phone moving (and 0.7 s after); a touch in the last
      0.8 s; the card's first 1.5 s; quiet in a tour except an item allowed Between steps, at a pause between two steps; never over its
      own target or the block being told (the service measures it: context.covers).
   2. The limits (an Essential item skips them): the breathing room between any two (×2 after three ignored in a row, up to ×4); at most
      n a minute and a visit; each kind's cap a visit and its gap; each group's own cap and gap (a group's gap stands in for its kind's);
      each item's Shows and Again after (×3 after a dismissed App item); a group resting (5 min after a dismissed Content item; for the
      visit after Fewer like this?). Its own When and Only if: after n pokes on Azu or n s idle; Lively only; never on a Solemn point;
      a point of the wall settled in the middle; two items about the ✦ never both in the visit's first minute (the Editor's order).
   3. The stage of the visit: s = max(active minutes / 15, the share of the milestones done); a returning visitor starts at 5 min.
   4. The score of each eligible item: importance × along the visit(s) × kind weight(s) (the Mix, data) × group weight × freshness × fit; the highest
      shows (Essential first); ties: the Editor's order. Deterministic, so Why not now? can always answer.
   Round 7, Fun: never on a Solemn point (a fixed gate of the kind: it.kindSolemnOff, "a Solemn point: no fun here"; no option breaks it);
   no joke on a Solemn day (a seasonal line marked Solemn holds today: context.solemnDay); a Moment only as the tour moves on to its next
   step (context.moment = "between": then only a Moment may show), its own When (after n s of the tour's narration, before its last step);
   pokes on Azu counted since the item last showed, or rarely a pause late in the visit (latePause, once a visit; then Along the visit
   counts, after pokes it is the visitor's own answer: ×1); a seasonal line only in its window of dates (context.today); a point's joke only
   for a point with a joke of its own.
   Unity: GuidanceScheduler, plain C# (Pick(state, now), WhyNot(state, now, id)), EditMode tests on the same cases. */
var GD_MOVE_MS=700,GD_TOUCH_MS=800,GD_CARD_MS=1500,GD_REST_MS=300000,GD_BACK_MIN=5,GD_STAGE_MIN=15,GD_FOREVER=1e15,GD_FIRST_MS=60000;
var GD_STATES={loading:"Loading",lost:"wall not found",empty:"nothing here",offline:"offline",done:"done"};
function gdVisitOf(visitor,id){return visitor.visit.items[id]||{shown:0,last:null,end:null,dismissed:0,acted:0,how:""}}
function gdEverOf(visitor,id){return visitor.ever.items[id]||{shown:0,done:false,by:""}}
function gdLeft(ms){return Math.max(1,Math.ceil(ms/1000))+" s left"}
function gdN(x,d){return(+x).toFixed(d==null?2:d)}
/* the stage of the visit: s from 0 (just arrived) to 1 (15 active minutes, or every milestone done) */
function gdStage(visitor,now){var v=visitor.visit||{},ret=visitor.visits>1,act=Math.max(0,now-(v.at||0)-(v.away||0))/60000,mins=act+(ret?GD_BACK_MIN:0),used=visitor.used||{};
  var done=GD_FEATURES.filter(function(f){return used[f[0]]!=null}).length,share=done/GD_FEATURES.length;
  return{s:Math.min(1,Math.max(mins/GD_STAGE_MIN,share)),mins:mins,active:act,returning:ret,done:done,share:share}}
/* the gates for every item: fixed, no option breaks them */
function gdGate(ctx,now){if(!ctx.on)return"Guidance is Off";if(ctx.demo)return"an Editor demo is showing";
  /* round 8: the app's start: quiet while the first pages run (they are an In order group of their own, never the scheduler's) */
  if(ctx.pages)return"quiet while "+ctx.pages+" "+(/^the first/.test(ctx.pages)?"run (the app's start)":"shows");
  /* round 6: the visitor's own Tips (Settings › The guide › Tips): Off silences every push item (the ✦ and Ask still answer) */
  if(ctx.tips==="off")return"the visitor turned Tips Off (Settings › The guide › Tips)";
  if(ctx.showing)return"one at a time: "+(/^the /i.test(ctx.showing)?"":"the ")+ctx.showing+" is showing";if(ctx.remark)return"one at a time: "+ctx.remark+" is showing";
  if(ctx.speaking)return"never over a sentence: the guide is speaking";
  if(ctx.state)return"quiet while a system state shows ("+ctx.state+")";
  if(ctx.panel)return"quiet while "+ctx.panel+" is open";if(ctx.dockFull)return"quiet while the dock is at Full";
  if(ctx.movedAt!=null&&now-ctx.movedAt<GD_MOVE_MS)return"quiet while the phone moves (and 0.7 s after)";
  /* the moment a card opens (context.moment = "open") is the visitor's own act answered: the touch that opened it and the card's first
     1.5 s do not hold back a cue that answers the opening (gdCheck lets no other item through at that moment) */
  if(ctx.moment==="open")return null;
  if(ctx.touchAt!=null&&now-ctx.touchAt<GD_TOUCH_MS)return"quiet while the visitor acts: a touch "+gdN((now-ctx.touchAt)/1000,1)+" s ago (it waits 0.8 s)";
  if(ctx.scrollAt!=null&&now-ctx.scrollAt<GD_TOUCH_MS)return"quiet while the visitor scrolls (and 0.8 s after)";
  if(ctx.cardAt!=null&&now-ctx.cardAt<GD_CARD_MS)return"quiet in the card's first 1.5 s";return null}
/* the tour's gate, per item: quiet in a tour, except an item allowed Between steps, at a pause between two steps */
function gdTourGate(it,ctx){if(!ctx.tour)return null;if(it.duringTour!=="between")return"quiet in a tour (During a tour: Never)";
  if(!ctx.tour.between)return"quiet in a tour: it waits for a pause between two steps";return null}
/* the item before this one in its In order group (the On ones only): items come in the Editor's order, each with its group and the group's mode */
function gdPrevOf(it,items){if(it.groupMode!=="order")return null;var i=items.indexOf(it);
  for(var j=i-1;j>=0;j--){var p=items[j];if(p.group===it.group&&p.on)return p}return null}
/* where its target is on the screen (the context's own: the nearest marker, the ✦, the point in the middle) */
function gdTargetCtx(it,ctx){return it.about==="marker"?ctx.marker:it.about==="gbtn"?ctx.gbtn:it.about==="middle"?ctx.central:it.about==="point"&&ctx.central&&ctx.central.id===it.point?ctx.central:null}
/* the visit's time (its time away not counted) */
function gdVisitT(v,now){return now-(v.at||0)-(v.away||0)}
/* the items of its group, or of its kind, shown this visit and the latest end among them */
function gdShownOf(items,visitor,f){return items.filter(f).reduce(function(a,o){return a+gdVisitOf(visitor,o.id).shown},0)}
function gdLastEndOf(items,visitor,f){var e=null;items.forEach(function(o){if(!f(o))return;var x=gdVisitOf(visitor,o.id).end;if(x!=null&&(e==null||x>e))e=x});return e}
/* the score: importance × along the visit × kind weight × group weight × freshness × fit, each factor kept for the words */
/* the kind's weight: its row of the Mix along the visit (S.guidance.mix, handed in with the item as kindMix: the scheduler reads only its arguments) */
function gdScore(it,items,visitor,ctx,now,st){var kw=it.kindMix||[100,100,100],f={imp:+it.importance||0,s:st.s};
  if(it.along==="steady"){f.kind=kw[1]/100;f.along=1}else{f.kind=gdCurve(kw,st.s)/100;f.along=gdCurve(GD_CURVE[it.along]||GD_CURVE.kind,st.s)}
  f.group=it.groupWeight==null?1:+it.groupWeight;
  var vm=gdVisitOf(visitor,it.id),em=gdEverOf(visitor,it.id),last=em.last!=null?em.last:vm.last,tau=(+ctx.tau||120)*1000;f.fresh=last==null?1:1-Math.exp(-Math.max(0,now-last)/tau);
  f.fit=1;f.fitWhy=[];var T=gdTargetCtx(it,ctx);if(T&&T.middle){f.fit*=1.5;f.fitWhy.push("in the middle")}
  if(it.teaches&&items.some(function(o){return o.id!==it.id&&o.kind==="app"&&o.on&&(o.teaches===it.teaches||o.doneWhen===it.teaches)&&!gdEverOf(visitor,o.id).done})){f.fit*=1.3;f.fitWhy.push("it teaches an open app tip")}
  f.score=f.imp*f.along*f.kind*f.group*f.fresh*f.fit;return f}
function gdScoreWords(f,it){var w=["importance "+f.imp,(it.kindName||it.kind)+" "+Math.round(f.kind*100)+" %"];
  if(f.along!==1)w.push((it.along==="late"?"late ":"early ")+gdN(f.along,f.along<0.1?3:2));if(f.group!==1)w.push("group "+gdN(f.group,1));
  if(f.fresh<0.995)w.push("fresh "+gdN(f.fresh));if(f.fit!==1)w.push("fit "+gdN(f.fit,2).replace(/0$/,"")+" ("+f.fitWhy.join(", ")+")");
  return"score "+gdN(f.score,f.score<0.1?3:2)+" ("+w.join(" × ")+"; stage s "+gdN(f.s)+")"}
function gdZeroWhy(it,f){if(f.along===0)return"along the visit "+(it.along==="late"?"Late: nothing before a third of the visit (stage s "+gdN(f.s)+"; it grows after s 0.30)":"Early: nothing left at this stage (s "+gdN(f.s)+")");
  if(f.group===0)return"its group's weight is 0";return"score 0"}
/* one item: {ok, why, score} or {no: why} */
function gdCheck(it,items,visitor,ctx,now){var vm=gdVisitOf(visitor,it.id),em=gdEverOf(visitor,it.id),v=visitor.visit,why=[];
  if(!it.on)return{no:"Off"};
  if(it.kindOn===false)return{no:(it.kindName||it.kind)+" is Off"};
  if(it.groupOn===false)return{no:(it.groupName||it.group)+" is Off"};
  if(it.groupPulled)return{no:"pulled only: it opens from Settings or the ✦ menu, never on its own"};
  /* round 7: Fun never on a Solemn point (fixed for the kind) and no joke on a Solemn day */
  if(it.kindSolemnOff&&ctx.solemn)return{no:"a Solemn point: no fun here"};
  if(it.joke&&ctx.solemnDay)return{no:"a Solemn day ("+ctx.solemnDay+"): no jokes today"};
  /* round 11: Fun never during a tour: every Fun item is held while a tour plays (not paused); fixed, no option */
  if(it.kind==="fun"&&ctx.tour&&!ctx.tour.paused)return{no:"held while a tour plays: Fun never shows during a tour"};
  if(ctx.moment==="between"&&it.form!=="moment")return{no:"the tour moves on to its next step: only a Moment holds it"};
  if(it.form==="moment"&&ctx.moment!=="between")return{no:"a Moment comes only as the tour moves on to its next step"};
  if(ctx.tips==="essentials"&&!it.essential)return{no:"the visitor chose Essentials only (Settings › The guide › Tips)"};
  if(ctx.moment==="open"&&it.when!=="open")return{no:"the card is opening: only a cue that answers the opening shows now"};
  if(it.doneWhen&&em.done)return{no:"done for good ("+GD_EVENTS[em.by||it.doneWhen]+")"};
  if(it.shows==="once"&&em.shown>=1)return{no:"shown once ever"};
  if(it.shows==="visit"&&vm.shown>=1)return{no:"shown once this visit"};
  if(it.shows==="card"&&vm.shown>=1&&vm.card===(v.cards||0))return{no:"shown on this card (again on the next)"};
  if((it.shows==="upto"||it.shows==="until")&&vm.shown>=+it.max)return{no:"shown "+vm.shown+" of "+it.max+" this visit"};
  /* round 6: After a dismissal (Wait ×3 is the learning below; Not again this visit; Never again) */
  if(it.afterDismiss==="never"&&(em.dismissed||0)>0)return{no:"dismissed before: never again (After a dismissal: Never again)"};
  if(it.afterDismiss==="visit"&&(vm.dismissed||0)>0)return{no:"dismissed this visit: not again this visit (After a dismissal)"};
  var rest=v.rest&&v.rest[it.group];if(rest!=null&&now<rest)return{no:v.fewer&&v.fewer[it.group]?"Fewer like this: "+(it.groupName||it.group)+" rests for the visit":(it.groupName||it.group)+" rests 5 min after a dismissal: "+gdLeft(rest-now)};
  var tg=gdTourGate(it,ctx);if(tg)return{no:tg};
  /* round 7: between two tour steps (a Moment): only in a tour; the water break after n s of narration; the cookie before the last step */
  if(it.when==="between"){var Tb=ctx.tour;if(!Tb)return{no:"only between two steps of a tour"};
    if(+it.talkFor>0&&!((+Tb.talked||0)>=+it.talkFor))return{no:"after "+it.talkFor+" s of the tour's narration ("+Math.floor(+Tb.talked||0)+" s so far)"};
    if(it.beforeEnd&&!Tb.toEnd)return{no:"before the tour's last step (not yet)"};why.push(it.beforeEnd?"the tour's last step next":"between two steps, "+Math.floor(+Tb.talked||0)+" s of narration")}
  /* a seasonal line: only in its window of dates; a point's joke: only for a point with one of its own (never a generic one) */
  if(it.seasons){var sv=gdSeasonOf(it,ctx.today);if(!sv)return{no:"no season today ("+gdDayWords(ctx.today)+")"};why.push("today ("+gdDayWords(ctx.today)+") is in its window "+gdDayWords(sv.from)+(sv.to!==sv.from?"–"+gdDayWords(sv.to):""))}
  if(it.ptJokes&&!(it.jokes&&it.jokes.length))return{no:(it.pointName||"this point")+" has no joke of its own (never a generic one in its place)"};
  if(it.lively&&!ctx.lively)return{no:"Lively only: Azu is Calm (Mascot > Liveliness)"};
  if(it.notSolemn&&ctx.solemn)return{no:"never on a Solemn point"};
  if(it.screen==="wall"&&!ctx.onWall)return{no:"not on the wall ("+(ctx.where||"a card is open")+")"};
  if(it.screen==="card"&&!ctx.onCard&&it.form!=="cue")return{no:"only on a card ("+(ctx.where||"the wall")+")"};
  /* round 6: Only if (the audience, a first or returning visit, a feature used or not yet), and the latest second of the visit */
  if(it.aud&&ctx.aud!==it.aud)return{no:"only for "+gdNameOf(GD_AUD,it.aud)+" (the audience is "+gdNameOf(GD_AUD,ctx.aud||"explorer")+")"};
  if(it.visitIs==="first"&&ctx.returning)return{no:"only on a first visit"};if(it.visitIs==="returning"&&!ctx.returning)return{no:"only on a returning visit"};
  var used=visitor.used||{};if(it.hasUsed&&used[it.hasUsed]==null)return{no:"only once "+gdNameOf(GD_USED,it.hasUsed)};if(it.notUsed&&used[it.notUsed]!=null)return{no:"only until "+gdNameOf(GD_USED,it.notUsed)+" (it was)"};
  if(+it.notAfter>0&&gdVisitT(v,now)>+it.notAfter*1000)return{no:"not after "+it.notAfter+" s of the visit"};
  if(it.when==="event"){var et=v.ev&&v.ev[it.event];if(et==null||now-et>GD_EVENT_MS)return{no:"on an event: it waits for "+(GD_EVENTS[it.event]||it.event)+" (then "+GD_EVENT_MS/1000+" s)"};why.push(GD_EVENTS[it.event]+" "+Math.floor((now-et)/1000)+" s ago")}
  /* a cue: on a card, its element on the screen (the context's cues: card/hints.js finds them), a moving cue never under Reduce Motion */
  if(it.form==="cue"){if(!ctx.onCard)return{no:"not on a card ("+(ctx.where||"the wall")+")"};if(it.reduce==="skip"&&ctx.reduce)return{no:"skipped under Reduce Motion (a moving cue)"};
    var ce=ctx.cues&&ctx.cues[it.about];if(!ce)return{no:GD_CUE_NO[it.about]||"its part is not on the screen"};why.push(ce)}
  var must=it.mustSee!==false;
  if(it.about==="marker"){if(!ctx.marker){if(must)return{no:"no marker in view"}}else why.push(ctx.marker.name+" in view")}
  if(it.about==="gbtn"){if(!ctx.gbtn){if(must)return{no:"the ✦ is not on the screen"}}else why.push("the ✦ on the screen")}
  if(it.about==="middle"){var c=ctx.central;if(!c||!c.tour){if(must)return{no:!c?"no point in the middle":c.name+" in the middle has no tour"}}else why.push(c.name+" in the middle")}
  /* round 6: When its target settles (a marker or the point in the middle): the target in the middle */
  if(it.when==="settle"&&it.about!=="point"){var Tg=gdTargetCtx(it,ctx);if(!Tg||!Tg.middle)return{no:"its target has not settled in the middle"}}
  if(it.about==="point"){var cp=ctx.central;if(!cp)return{no:"no point in the middle"};if(cp.id!==it.point)return{no:"not in the middle ("+cp.name+" is nearest)"};
    if(!cp.middle)return{no:cp.name+" is the nearest, not in the middle yet"};why.push(cp.name+" in the middle")}
  if(ctx.covers&&ctx.covers[it.id])return{no:"never over its own target: "+ctx.covers[it.id]};
  /* its own When: after n pokes on Azu (this visit) or n s idle (no touch, the phone still), whichever comes first */
  var iN=gdIdleOf(it),lp=+it.latePause||0,viaPokes=false;
  /* round 7: a Fun item counts the pokes since it last showed (a joke every third poke, as before); latePause: or a pause late in the visit, once a visit */
  if(+it.pokes>0||iN>0){var pc=(+ctx.pokes||0)-(it.kind==="fun"?+vm.pk||0:0),pk=+it.pokes>0&&pc>=+it.pokes,iS=ctx.idleSince,idl=iN>0&&iS!=null&&now-iS>=iN*1000;
    var late=!pk&&!idl&&lp>0&&!vm.shown&&gdVisitT(v,now)>=lp*1000;
    if(!pk&&!idl&&!late){var ow=[];if(+it.pokes>0)ow.push(it.pokes+" pokes on Azu ("+pc+" so far"+(it.kind==="fun"&&vm.shown?" since it last showed":"")+")");if(iN>0)ow.push(iN+" s idle ("+(iS==null?"not idle":gdLeft(iN*1000-(now-iS)))+")");
      if(lp>0)ow.push(vm.shown?"a pause late in the visit (once a visit: it has shown)":"a pause late in the visit (from "+Math.floor(lp/60)+":"+("0"+lp%60).slice(-2)+": "+gdLeft(lp*1000-gdVisitT(v,now))+")");return{no:"after "+ow.join(" or ")}}
    viaPokes=pk;why.push(pk?pc+" pokes on Azu":idl?"idle "+Math.floor((now-iS)/1000)+" s":"a pause late in the visit")}
  if(+it.notBefore>0){var at=now-(v.at||0)-(v.away||0);if(at<+it.notBefore*1000)return{no:"not before "+it.notBefore+" s of the visit: "+gdLeft(+it.notBefore*1000-at)}}
  var p=gdPrevOf(it,items);if(p){var pm=gdVisitOf(visitor,p.id);if(!pm.shown)return{no:"after the "+p.label+" (not shown yet this visit)"};
    var since=now-pm.last;if(since<+it.afterPrev*1000)return{no:it.afterPrev+" s after the "+p.label+": "+gdLeft(+it.afterPrev*1000-since)};if(+it.afterPrev>0)why.push(Math.floor(since/1000)+" s after the "+p.label)}
  if(+it.stillFor>0){var st=now-ctx.stillSince;if(!(st>=+it.stillFor*1000))return{no:"the phone still "+(isFinite(st)?(st/1000).toFixed(1):"0")+" s (it waits for "+it.stillFor+" s)"};why.push("the phone still "+(isFinite(st)?(st/1000).toFixed(1)+" s":"")) }
  /* two items about the ✦ never both in the visit's first minute */
  if(it.about==="gbtn"&&gdVisitT(v,now)<GD_FIRST_MS){var o2=items.filter(function(o){return o.id!==it.id&&o.about==="gbtn"&&gdVisitOf(visitor,o.id).shown>0})[0];
    if(o2)return{no:"one about the ✦ in the visit's first minute: the "+o2.label+" showed"}}
  /* the cue room (Overall): between a tip and a cue, either way (a cue is small motion: a shorter room than the breathing room between
     two tips); two cues follow each other with no room (one at a time). A tip that a card's opening ended (visitor.visit.openEnd: the visitor did
     what it asked) is answered, so the room does not count from it */
  var room=+ctx.cueRoom||0,tip=function(o){return o.form!=="cue"};
  /* round 7: a Fun item after pokes answers the visitor (as Azu's own bubble did): the breathing room, the cue room, at most a minute and the
     kind's and group's gaps neither hold it nor start from it (one at a time, the gates and the caps still hold) */
  var answer=viaPokes&&it.kind==="fun";
  if(answer&&it.kindCap>0){var kn0=items.filter(function(o){return o.kind===it.kind&&tip(o)}).reduce(function(a,o){return a+gdVisitOf(visitor,o.id).shown},0);if(kn0>=it.kindCap)return{no:"at most "+it.kindCap+" "+(it.kindName||it.kind)+" items a visit: shown "+kn0}}
  if(it.form==="cue"){var te=v.lastEnd;if(room>0&&te!=null&&now-te<room*1000&&te!==v.openEnd)return{no:"the cue room "+room+" s after a tip: "+gdLeft(room*1000-(now-te))}}
  else if(!it.essential&&!answer){var mul=v.breathMul||1,br=ctx.breathing*mul,L=ctx.limits||{},starts=v.starts||[];
    if(v.lastEnd!=null&&now-v.lastEnd<br*1000)return{no:"breathing room "+br+" s"+(mul>1?" (×"+mul+": "+(v.ignoredRun||0)+" ignored in a row)":"")+": "+gdLeft(br*1000-(now-v.lastEnd))};
    if(room>0&&v.cueEnd!=null&&now-v.cueEnd<room*1000)return{no:"the cue room "+room+" s after a cue: "+gdLeft(room*1000-(now-v.cueEnd))};
    if(+L.perMinute>0){var inMin=starts.filter(function(t){return now-t<60000});if(inMin.length>=+L.perMinute)return{no:"at most "+L.perMinute+" a minute: "+gdLeft(60000-(now-inMin[inMin.length-L.perMinute]))}}
    /* the tips' limits count the tips (a cue is not a tip: it has its own Shows and the cue room) */
    var all=items.filter(tip).reduce(function(a,o){return a+gdVisitOf(visitor,o.id).shown},0);if(+L.perVisit>0&&all>=+L.perVisit)return{no:"at most "+L.perVisit+" a visit: shown "+all+" this visit"};
    if(it.kindCap>0){var kn=items.filter(function(o){return o.kind===it.kind&&tip(o)}).reduce(function(a,o){return a+gdVisitOf(visitor,o.id).shown},0);if(kn>=it.kindCap)return{no:"at most "+it.kindCap+" "+(it.kindName||it.kind)+" items a visit: shown "+kn}}}
  if(!it.essential&&!answer){
    if(it.groupCap>0){var gc=gdShownOf(items,visitor,function(o){return o.group===it.group});if(gc>=it.groupCap)return{no:"at most "+it.groupCap+" "+(it.groupName||it.group)+" a visit: shown "+gc}}
    /* a group's own gap stands in for its kind's (the kind's gap is the tips') */
    if(it.groupGap>0){var ge=gdLastEndOf(items,visitor,function(o){return o.group===it.group});
      if(ge!=null&&now-ge<it.groupGap*1000)return{no:(it.groupName||it.group)+"'s gap "+it.groupGap+" s: "+gdLeft(it.groupGap*1000-(now-ge))}}
    else if(it.kindGap>0&&tip(it)){var ke=gdLastEndOf(items,visitor,function(o){return o.kind===it.kind&&tip(o)});
      if(ke!=null&&now-ke<it.kindGap*1000)return{no:(it.kindName||it.kind)+"'s gap "+it.kindGap+" s: "+gdLeft(it.kindGap*1000-(now-ke))}}}
  var tri=vm.how==="dismissed"&&it.kind==="app"&&!(ctx.learn&&ctx.learn.dismiss===false),ag=+it.again*(tri?3:1);
  if(ag>0&&vm.end!=null&&it.shows!=="once"&&it.shows!=="visit"&&now-vm.end<ag*1000)return{no:"again after "+ag+" s"+(tri?" (×3: it was dismissed)":"")+": "+gdLeft(ag*1000-(now-vm.end))};
  if(it.when==="visit")why.unshift("the visit started ("+(ctx.returning?"a returning visitor":"a first visit")+")");
  if(it.shows==="upto"||it.shows==="until")why.push("shown "+vm.shown+" of "+it.max+" this visit");
  if(it.essential)why.push("Essential: it skips the breathing room");
  var f=gdScore(it,items,visitor,ctx,now,gdStage(visitor,now));
  /* after pokes the item answers the visitor: Along the visit does not hold it back */
  if(viaPokes&&f.along!==1){f.along=1;f.score=f.imp*f.along*f.kind*f.group*f.fresh*f.fit}if(answer)why.push("an answer to the pokes: the breathing room does not hold it");
  if(!(f.score>0))return{no:gdZeroWhy(it,f)};
  why.push(gdScoreWords(f,it));return{ok:true,why:why.join(", "),score:f.score,f:f,answer:answer}}
/* every item checked, in the Editor's order; the eligible ones in the order they would show: Essential first, then the score, then the Editor's order */
function gdAll(items,visitor,ctx,now){var all=items.map(function(it,i){return{it:it,i:i,r:gdCheck(it,items,visitor,ctx,now)}});
  /* two items about the ✦ both due in the visit's first minute: the first in the Editor's order, the other never in that minute */
  if(gdVisitT(visitor.visit||{},now)<GD_FIRST_MS){var first=null;all.forEach(function(x){if(x.it.about!=="gbtn"||!x.r.ok)return;if(!first){first=x;return}
    x.r={no:"one about the ✦ in the visit's first minute: the "+first.it.label+" goes first (the Editor's order)"}})}
  return all}
function gdOrder(a,b){var d=b.r.score-a.r.score;return(b.it.essential?1:0)-(a.it.essential?1:0)||(Math.abs(d)<1e-9?0:d)||a.i-b.i}
function gdPick(items,visitor,ctx,now){var g=gdGate(ctx,now);if(g)return{none:true,why:g};
  var all=gdAll(items,visitor,ctx,now),ok=all.filter(function(x){return x.r.ok}).sort(gdOrder);
  if(ok.length){var b=ok[0];return{item:b.it,why:b.it.label+": "+b.r.why,score:b.r.score,answer:!!b.r.answer}}
  return{none:true,why:all.length?all.map(function(x){return x.it.label+": "+x.r.no}).join(" · "):"no guidance items"}}
/* Why not now? for one item, in the scheduler's own words: a gate, its own reason, or the item that goes first */
function gdWhyNot(items,visitor,ctx,now,id){var it=items.filter(function(x){return x.id===id})[0];if(!it)return"no such item";
  var g=gdGate(ctx,now);if(g)return it.label+": "+g;
  var all=gdAll(items,visitor,ctx,now),me=all.filter(function(x){return x.it===it})[0];if(!me.r.ok)return it.label+": "+me.r.no;
  var ok=all.filter(function(x){return x.r.ok}).sort(gdOrder),b=ok[0];if(b.it===it)return it.label+": it shows next: "+me.r.why;
  return it.label+": it could show, but the "+b.it.label+" goes first ("+(b.it.essential&&!it.essential?"Essential":"score "+gdN(b.r.score)+" against "+gdN(me.r.score))+")"}
