/* Guidance in the Editor: Guide tab > 4 · Guidance. 1 · Overall: Guidance On / Off, Breathing room, At most a minute, At most a visit,
   Freshness recovers over, the learning (Ignored three in a row, After a dismissal, Fewer like this?), Remember across visits, the long
   press's words. 2 · App and 3 · Content: the kind's rows (On, At most a visit, Gap, its weight along the visit read back in words), its
   groups (their weight) and each item a row (On, its label, its importance dots, how often, ▸ Show now) that opens its fields: Label, Says
   per language, Essential, Shows, At most, Again after, Done when, During a tour, Along the visit, the fixed ones in words and the
   read-back. 0 · Preview & Test: Show next now and why; Why not now? (an item's reason, in the scheduler's own words); the visitor
   (first or returning, the visits, the active minutes, the stage, the features used, each item's memory; New visit, Leave for 31 min,
   Reset the visitor); the Log of the last 30 decisions. Each row's demo (rowDemos): an item's row puts the phone on the wall and shows
   the item now (a demo is pulled: it never writes the visitor's memory); a limit or a learning row shows its effect on a ×10 clock with
   the countdown chip. Global Scene > Onboarding hints links here (the wall hint's pill is gone). Unity: the POI Editor's Guide tab, its
   Guidance section drawn from GuidanceCatalog, each row's ▸ a Play-mode preview (PlayMode Live Config); Why not now? and the Log read
   GuidanceScheduler.WhyNot and the service's decision log. */
COMP["gd:overall"]=["Guide","Guidance › Overall"];
COMP["gd:i:opening"]=["Guide","Guidance › The start › Opening message"];COMP["gd:i:tapmarker"]=["Guide","Guidance › The start › Tap a marker"];
COMP["gd:i:askview"]=["Guide","Guidance › The wall › Ask about the view"];COMP["gd:i:tourof"]=["Guide","Guidance › Tours › A tour of {point}"];
FOCUS["gd:overall"]=["card","#wall","closed"];FOCUS["gd:app"]=["card","[data-marker]","closed"];FOCUS["gd:content"]=["card","[data-marker]","closed"];
FOCUS["gd:g:start"]=["card","[data-marker]","closed"];FOCUS["gd:g:wall"]=["card",".gbtn-wrap","closed"];FOCUS["gd:g:tours"]=["card","[data-marker]","closed"];
FOCUS["gd:i:opening"]=["card",".gbtn-wrap","closed"];FOCUS["gd:i:tapmarker"]=["card","[data-marker]","closed"];FOCUS["gd:i:askview"]=["card",".gbtn-wrap","closed"];FOCUS["gd:i:tourof"]=["card","[data-marker]","closed"];
/* a guidance word's place in the tab (Languages & Terms' links; Global Scene's link) */
function gdKindOfGroup(gid){var k=null;S.guidance.kinds.forEach(function(x){x.groups.forEach(function(g){if(g.id===gid)k=x.id})});return k}
on("guidePath.answer",function(o){if(o==="gd:overall"||/^gd:(app|content)$/.test(o))return["gt4",o];if(o.indexOf("gd:g:")===0)return["gt4","gd:"+(gdKindOfGroup(o.slice(5))||"app"),o];
  var m=o.match(/^gd:i:(.+)$/),it=m&&S.guidance.items[m[1]];if(it)return["gt4","gd:"+it.kind,"gd:g:"+it.group,o]});
/* the self-test: a tip's words show on the wall, at its moment (behaviour); every guidance option is timing (behaviour) */
TX_DOM.wall_tip="shown in the guide's tip beside the nearest marker";TX_DOM.gd_opening="shown in the guide's opening tip on the wall";TX_DOM.gd_opening_back="shown in the opening tip to a returning visitor";
TX_DOM.gd_askview="shown in the guide's tip beside the ✦";TX_DOM.gd_tourof="shown in the tour invitation on the wall";
TX_DOM.gd_fewer="shown after a long press on a tip";TX_DOM.gd_fewer_yes="shown after a long press on a tip";TX_DOM.gd_fewer_no="shown after a long press on a tip";
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(guidance\\.|"));
/* the visit's time as the Log and the visitor read it: m:ss */
function gdClock(ms){var s=Math.max(0,Math.round(ms/1000));return Math.floor(s/60)+":"+("0"+s%60).slice(-2)}
/* ----- the rows ----- */
function gdDots(it){var p="@guidance.items."+it.id+".importance";return'<span class="gddots" role="group" aria-label="Importance">'+[1,2,3,4,5].map(function(n){
  return'<button type="button" data-set="'+p+'" data-v="'+n+'" aria-pressed="'+(+it.importance===n)+'"'+(n<=+it.importance?' class="on"':'')+' title="Importance '+n+' of 5" aria-label="Importance '+n+' of 5"></button>'}).join("")+'</span>'}
var GD_ABOUT_WORDS={nothing:"nothing (the wall)",marker:"the nearest marker (it must be on the screen)",gbtn:"the ✦, the way into Ask about the view (it must be on the screen)",middle:"the point in the middle, one with a tour"};
var GD_TAP_WORDS={close:"it closes (acted)",view:"Ask about the view opens (acted)",tour:"the tour of that point starts (acted)"};
function gdItemRow(it){var p="guidance.items."+it.id+".",k=gdKindOf(it),g=gdGroupOf(it),i=g?g.items.indexOf(it.id):0,pv=i>0&&g.mode==="order"?S.guidance.items[g.items[i-1]]:null;
  var sum='<span class="gdi"><span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b>'+(it.essential?'<span class="badge new">ESSENTIAL</span>':"")+gdDots(it)+
    '<span class="gdhow">'+esc(gdHowOften(it))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Show it on the phone now">&#9656; Show now</button></span>';
  var body=row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the item's name in these lists, in the read-back, the Log and the scenes. The visitor never sees it.")+
    '<div class="sub2">Says · one line per language (Languages &amp; Terms, owned here)</div>'+txPanel("gd:i:"+it.id)+
    row("Essential",pseg(p+"essential",["true","false"],["Yes","No"]),"Yes: it skips the breathing room and the limits (never the gates: one at a time, never over a sentence, never while the phone moves or a panel is open), and goes before any other. For the visit's first words.")+
    row("Shows",pseg(p+"shows",GD_SHOWS.map(function(x){return x[0]}),GD_SHOWS.map(function(x){return x[1]})),"How often it may show: once ever; once a visit; up to n a visit; until done (its Done when), at most n a visit; or every time (only Again after and the breathing room limit it).")+
    (it.shows==="upto"||it.shows==="until"?row("At most",pslider(p+"max",1,10,1),"Times a visit.",true):"")+
    (it.shows==="once"||it.shows==="visit"?"":row("Again after",pslider(p+"again",0,120,5),"Seconds after it ends before it may show again (its cooldown; 0: only the breathing room). The read-back says it as the visitor lives it: “at most about every 25 s”. A dismissed App tip waits ×3 (1 · Overall > After a dismissal). Not for Once ever or Once a visit.",true))+
    row("Done when",pseg(p+"doneWhen",GD_DONE.map(function(x){return x[0]}),GD_DONE.map(function(x){return x[1]})),"The visitor's action that retires it for good (this visit and the next, until Reset the visitor): Apple TipKit's rule, a tip gone once its feature is used. A tip showing when its feature is used counts as acted.")+
    row("During a tour",pseg(p+"duringTour",GD_TOUR.map(function(x){return x[0]}),GD_TOUR.map(function(x){return x[1]})),"Never: quiet for the whole tour. Between steps: it may show at a pause between two steps of a tour (never over a step's words, never over the block being told).")+
    row("Along the visit",pseg(p+"along",GD_ALONG.map(function(x){return x[0]}),GD_ALONG.map(function(x){return x[1]})),"How its weight follows the stage of the visit (s: 0 on arrival, 1 after 15 active minutes or every milestone done). As the kind: the kind's weight alone. Steady: the kind's middle weight all visit. Early: full for the first third, then fading to nothing. Late: nothing for the first third, then growing to full. A weight in the score, never a quota.")+
    '<div class="note gdfix">'+esc("About: "+(GD_ABOUT_WORDS[it.about]||it.about)+" · Mascot goes: "+(it.goes==="beside"?"beside it, and points":it.points?"stays beside the ✦ and points at it":"stays beside the ✦")+
      " · Focus: "+(it.focus==="ring"?"Ring":"None")+" · When: "+(it.when==="visit"?"the visit started":"at a pause")+(+it.stillFor>0?", the phone still "+it.stillFor+" s":"")+(+it.notBefore>0?", not before "+it.notBefore+" s of the visit":"")+(pv&&+it.afterPrev>0?", at least "+it.afterPrev+" s after the "+pv.label:"")+
      " · A tap: "+(GD_TAP_WORDS[it.tap]||"it closes")+" · Spoken: "+(k&&k.spoken==="bubble"?"no, a bubble only (App)":"once the visitor has heard the guide this visit, else a bubble ("+(k?k.name:"")+")")+". Fixed for this item.")+'</div>'+
    '<div class="note gdrb">'+esc(gdReadBack(it))+'</div>';
  return fo("gd:i:"+it.id,sum,null,body,4)}
/* a kind's weight along the visit, read back in words (the mix: a fixed table) */
function gdKindWords(k){var w=GD_MIX[k.id]||[0,0,0];return"Its weight along the visit: "+w[0]+" % on arrival, "+w[1]+" % a third of the way in, "+w[2]+" % at the end (s = 0 / 0.3 / 1, straight lines between): a weight in each item's score, never a quota; an item's Along the visit shapes it further. "+
  (k.spoken==="bubble"?"Spoken: never, a bubble only.":"Spoken: once the visitor has heard the guide this visit (a tour step, an answer), else a bubble.")}
function gdKindSub(k){return k.id==="app"?"how to use the app: a bubble, never spoken":"what is here and what next: spoken once the guide has been heard"}
/* ----- 0 · Preview & Test: the read-outs ----- */
function gdVisitorLine(){var v=S.gdVisitor;return"Visit "+v.visits+(v.visits>1?" (a returning visitor)":" (a first visit)")+" · "+gdItems().map(function(it){var vm=gdVisitOf(v,it.id),em=gdEverOf(v,it.id);
  return it.label+": "+vm.shown+" this visit, "+em.shown+" ever"+(em.done?", done ("+GD_EVENTS[em.by]+")":"")}).join(" · ")}
function gdVisitorMore(){var v=S.gdVisitor,vi=v.visit,now=gdNow(),st=gdStage(v,now),used=GD_FEATURES.filter(function(f){return v.used[f[0]]!=null}).map(function(f){return f[1]+" ("+gdClock(v.used[f[0]]-vi.at)+")"}),
    rest=Object.keys(vi.rest||{}).filter(function(g){return vi.rest[g]>now}).map(function(g){var gg=gdGroupById(g);return(gg?gg.name:g)+(vi.fewer&&vi.fewer[g]?" (Fewer like this: the visit)":" ("+gdClock(vi.rest[g]-now)+")")});
  return(v.visits>1?"A returning visitor":"A first visit")+" · visits "+v.visits+" · active "+gdN(st.active,1)+" min"+(st.returning?" (the stage counts from 5 min)":"")+" · the stage s "+gdN(st.s)+
    " · met the guide: "+(v.met?"yes":"no")+" · heard the guide this visit: "+(vi.heard?"yes":"no")+" · features used: "+(used.join(", ")||"none")+
    " · breathing room ×"+(vi.breathMul||1)+" ("+(vi.ignoredRun||0)+" ignored in a row) · resting: "+(rest.join(", ")||"none")+" · Remember across visits: "+(S.guidance.remember!==false?"On":"Off")}
function gdMemTable(){var v=S.gdVisitor;return'<table class="tbl gdmem"><thead><tr><th>Item</th><th>Visit</th><th>Ever</th><th>Last shown</th><th>The line used</th><th>Acted</th><th>Dismissed</th><th>Done</th></tr></thead><tbody>'+
  gdItems().map(function(it){var vm=gdVisitOf(v,it.id),em=gdEverOf(v,it.id);return'<tr><td>'+esc(it.label)+'</td><td>'+vm.shown+'</td><td>'+em.shown+'</td><td>'+(em.last!=null?gdClock(em.last-v.visit.at):"")+'</td><td>'+esc(em.line||"")+'</td><td>'+(em.acted||0)+'</td><td>'+(em.dismissed||0)+'</td><td>'+(em.done?esc(GD_EVENTS[em.by]||em.by):"")+'</td></tr>'}).join("")+'</tbody></table>'}
/* the Log: the last 30 decisions (shown, skipped, ended, and what the learning changed), the newest first, each with the visit's time */
function gdLogLines(){return G.gd.log.filter(function(l){return/^(shown|skipped|ended|learned)$/.test(l.what)}).slice(-30).reverse()}
function gdLogHtml(){var l=gdLogLines();return l.length?l.map(function(x){return'<div class="gdl gdl-'+x.what+'"><span title="visit '+x.v+'">'+gdClock(x.vt)+'</span><b>'+x.what+'</b><span>'+(x.what==="skipped"?x.why.split(" · ").map(esc).join("<br>"):esc(x.why))+'</span></div>'}).join(""):'<div class="uses">Nothing yet.</div>'}
function gdNowLine(){var c=G.gd.cur;return c?"Now: showing the "+c.item.label+(c.pull?" (a demo)":"")+".":G.gd.last?"Now: "+(G.gd.last.item?G.gd.last.why:"nothing, "+G.gd.last.why):"Now: nothing yet."}
on("edGuide.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"gt0",'<div class="sub2">Guidance</div>'+row("Show next now",'<button class="ubtn sm" type="button" data-gdnext="1">&#9656; Show next now</button>',"Asks the scheduler what it would show next, now, with the phone on the wall and still: the item and why, or why nothing (each item's reason). It is shown as the visitor would get it, and counted in the visitor's memory.")+
    '<div class="note gdwhy" id="gdwhy">'+esc(G.gd.next?(G.gd.next.item?"Next: "+G.gd.next.why:"Next: nothing. "+G.gd.next.why):"Next: press Show next now.")+'</div><div class="note gdwhy" id="gdnow">'+esc(gdNowLine())+'</div>'+
    row("Why not now?",'<div class="btnrow" style="margin:0"><select class="u gdwhysel" data-gdwhysel="1" aria-label="The item">'+gdItems().map(function(it){return'<option value="'+it.id+'"'+(G.gd.whyId===it.id?" selected":"")+'>'+esc(it.label)+'</option>'}).join("")+'</select><button class="ubtn sm" type="button" data-gdwhynot="1">Why not now?</button></div>',
      "Pick an item: why it does not show at this moment, in the scheduler's own words (a gate, its own rule, a limit, the learning, its score), or which item goes first. The phone stays as it is.")+
    '<div class="note gdwhy" id="gdwhynot">'+esc(G.gd.whyText||"Pick an item and press Why not now?")+'</div>'+
    row("The visitor",'<div class="btnrow" style="margin:0"><button class="ubtn sm" type="button" data-gdvisit="new">New visit</button><button class="ubtn sm" type="button" data-gdvisit="leave">Leave for 31 min</button><button class="ubtn sm" type="button" data-gdvisit="reset">Reset the visitor</button></div>',"The visitor's memory, kept on the phone only. New visit: the same visitor comes back now (the opening says Welcome back; what is done stays done). Leave for 31 min: the visit's clock moves 31 minutes with the app away: a visit ends after 30 min away, so the visitor comes back to a new visit (the stage counts from 5 min). Reset the visitor: a first visit again, nothing remembered. With Remember across visits Off, every new visit is a first visit.")+
    '<div class="note" id="gdvisitor">'+esc(gdVisitorLine())+'</div><div class="note" id="gdvisitor2">'+esc(gdVisitorMore())+'</div><div id="gdmem">'+gdMemTable()+'</div>'+
    row("Log",'<div class="gdlog" id="gdlog" role="log" aria-label="The last 30 decisions">'+gdLogHtml()+'</div>',"The last 30 decisions, the newest first: each with the visit's time, shown (and why: its rules and its score), skipped (why nothing shows: a gate, or each item's reason; written when the reason changes), ended (and how: acted, dismissed, ignored, or the moment), and what the learning changed."));
  var g=S.guidance,ov=row("Guidance",pseg("guidance.on",["true","false"],["On","Off"]),"On: the guide takes the initiative at a pause, one tip at a time, never over a sentence, never while the phone moves. Off: no tips; what the visitor asks for (the ✦ menu, Ask) still answers.")+
    row("Breathing room",pslider("guidance.breathing",5,60,1),"Seconds between any two tips, from the end of one to the start of the next (an Essential item skips it). Doubled after three ignored in a row (below).")+
    row("At most a minute",pslider("guidance.perMinute",1,6,1),"Tips that may start in any 60 s (an Essential item skips it). Talkative scripts meet it; most visits never do.")+
    row("At most a visit",pslider("guidance.perVisit",5,60,1),"Tips in one visit, all kinds together (an Essential item skips it). A kind has its own cap below.")+
    row("Freshness recovers over",pslider("guidance.freshTau",30,600,10),"Seconds (τ). An item just shown scores ×0, then recovers: 1 − e^(−t/τ) (after τ: ×0.63, after 2τ: ×0.86). So a tip that was just seen yields to another when both are due.")+
    row("Ignored three in a row",pseg("guidance.learnIgnore",["true","false"],["Double the breathing room","Off"]),"Three tips in a row whose time ran out (never tapped, never swiped): the breathing room doubles for the visit (six: ×4, no more). A tip acted on brings it back. The opening's time running out is not ignoring.")+
    row("After a dismissal",pseg("guidance.learnDismiss",["true","false"],["Wait longer","Off"]),"A tip swiped away: an App tip waits ×3 its Again after; a Content item rests its whole group 5 min.")+
    row("Fewer like this?",pseg("guidance.fewer",["true","false"],["Offered on a long press","Off"]),"A long press on a tip asks Fewer like this? Fewer: its group rests for the rest of the visit. Keep: the tip stays. Its words are below.")+
    row("Remember across visits",pseg("guidance.remember",["true","false"],["On","Off"]),"On: the visitor's memory stays on the phone from visit to visit (what is done stays done; the opening says Welcome back; the stage counts from 5 min). Off, for kiosks and demos: every visit starts as a first visit. A visit ends after 30 min away.")+
    '<div class="sub2">The long press · its words (Languages &amp; Terms, owned here)</div>'+txPanel("gd:overall");
  var gs="";S.guidance.kinds.forEach(function(k,ki){var kp="guidance.kinds."+ki+".",inner=row("On",pseg(kp+"on",["true","false"],["On","Off"]),"Off: none of the "+k.name+" items show (each keeps its own On).")+
      row("At most a visit",pslider(kp+"cap",1,40,1),"The "+k.name+" items in one visit, all together (an Essential item skips it).")+
      row("Gap",pslider(kp+"gap",0,180,5),"Seconds from the end of one "+k.name+" item to the start of the next (0: only the breathing room). An Essential item skips it.")+
      '<div class="note gdmix">'+esc(gdKindWords(k))+'</div>';
    k.groups.forEach(function(g,gi){var items=g.items.map(function(id){return S.guidance.items[id]}).filter(Boolean);
      inner+=fo("gd:g:"+g.id,esc(g.name)+' <span class="sub">· '+(g.mode==="order"?"In order: the visit's first words, then the first gesture":"each on its own")+'</span>',null,
        '<div class="note">'+(g.mode==="order"?"In order: an item waits until the one before it has shown this visit.":"Each item on its own.")+' A row: On, its name, importance (●), how often, ▸ Show now; open it for its fields.</div>'+
        row("Weight",pslider(kp+"groups."+gi+".weight",0,2,0.1),"×: this group's items' scores against the others' (1: as they are; 0: they never win).")+items.map(gdItemRow).join(""),3)});
    gs+=fo("gd:"+k.id,esc(k.name)+' <span class="sub">· '+gdKindSub(k)+'</span>',null,inner,2)});
  d.insertAdjacentHTML("beforeend",fo("gt4",sectionTitle("4","Guidance","when the guide speaks up: a tip at a pause, one at a time"),["new","NEW"],
    fo("gd:overall",'Overall <span class="sub">· on, the breathing room, the limits, the learning and the memory</span>',null,ov,2)+gs,1));
  return d.innerHTML});
/* the read-outs follow the visit (a tip shown, a card opened, a new visit) */
function gdReadouts(){var v=ed.querySelector("#gdvisitor");if(v)v.textContent=gdVisitorLine();var n=ed.querySelector("#gdnow");if(n)n.textContent=gdNowLine();
  var v2=ed.querySelector("#gdvisitor2");if(v2)v2.textContent=gdVisitorMore();var m=ed.querySelector("#gdmem");if(m)m.innerHTML=gdMemTable();var lg=ed.querySelector("#gdlog");if(lg)lg.innerHTML=gdLogHtml()}
on("gdLog.after",function(){if(!G.gd.roT)G.gd.roT=setTimeout(function(){G.gd.roT=0;gdReadouts()},60)});
setInterval(function(){var n=ed.querySelector("#gdnow");if(n)n.textContent=gdNowLine();var v2=ed.querySelector("#gdvisitor2");if(v2)v2.textContent=gdVisitorMore()},1000);
/* ----- the Editor's actions: Show next now, Why not now?, the visitor, ▸ Show now; a changed label redraws the rows ----- */
function gdOnWallNow(){if(G.on)stopTour();if(typeof hideStopUi==="function")hideStopUi();S.settingsOpen=false;S.contentsOpen=false;G.fab.menu=false;G.cam.open=false;G.fab.bub=null;S.view="card";S.stop="closed";renderPhone(false)}
function gdShowNext(){gdOnWallNow();gdStop("Show next now");var ctx=gdContext();ctx.onWall=true;ctx.where="";ctx.stillSince=-Infinity;ctx.movedAt=null;ctx.touchAt=null;ctx.cardAt=null;ctx.panel=null;ctx.dockFull=false;ctx.tour=null;ctx.state=null;ctx.remark=null;ctx.showing=null;ctx.speaking=false;ctx.demo=false;
  var r=gdPick(gdItems(),S.gdVisitor,ctx,gdNow());G.gd.next=r;if(r.item)gdStart(r.item,r.why);var w=ed.querySelector("#gdwhy");if(w)w.textContent=r.item?"Next: "+r.why:"Next: nothing. "+r.why;return r}
/* Why not now?: the scheduler's words for one item at this moment (the phone as it is; its bubble measured, so a covered target says so) */
function gdWhyNow(id){var items=gdItems(),ctx=gdContext(),it=items.filter(function(x){return x.id===id})[0];if(it&&!gdGate(ctx,gdNow())){var cov=gdCovers(it);if(cov)ctx.covers[id]=cov}
  var t=gdWhyNot(items,S.gdVisitor,ctx,gdNow(),id);G.gd.whyId=id;G.gd.whyText=t;var w=ed.querySelector("#gdwhynot");if(w)w.textContent=t;return t}
ed.addEventListener("click",function(e){var t=e.target;
  var nx=t.closest("[data-gdnext]");if(nx){e.stopPropagation();gdShowNext();return}
  var wn=t.closest("[data-gdwhynot]");if(wn){e.stopPropagation();var s=ed.querySelector("[data-gdwhysel]");gdWhyNow(s?s.value:"opening");return}
  var vs=t.closest("[data-gdvisit]");if(vs){e.stopPropagation();var k=vs.dataset.gdvisit;if(k==="leave"){gdLeave(31*60000);return}gdOnWallNow();if(k==="new")gdNewVisit();else gdResetVisitor();return}
  var sh=t.closest("[data-gdshow]");if(sh){e.stopPropagation();e.preventDefault();showRowDemo("guidance.items."+sh.dataset.gdshow+".on");return}},true);
ed.addEventListener("change",function(e){var t=e.target;if(t.dataset.gdwhysel){e.stopPropagation();gdWhyNow(t.value);return}if(t.dataset.path&&t.dataset.path.indexOf("guidance.")===0)renderEditor()},true);
/* ----- each row's demo ----- */
function gdItemNow(id){return gdItems().filter(function(x){return x.id===id})[0]}
/* a demo begins: the guidance waits (gate "an Editor demo is showing") while it runs; a newer demo of any row ends it */
function gdDemoBegin(){G.gd.demo=true;G.gd.demoOwner=G.demoSeq;clearTimeout(G.gd.demoT);clearInterval(G.gd.demoI);gdStop("an Editor demo");gdChip(null);var r=ph.querySelector(".azfabbub");if(r)r.remove();return G.demoSeq}
function gdDemoLive(my){if(G.demoSeq===my)return true;if(G.gd.demoOwner===my){G.gd.demo=false;gdChip(null);clearInterval(G.gd.demoI)}return false}
function gdDemoEnd(my,ms){G.gd.demoT=setTimeout(function(){if(!gdDemoLive(my))return;gdChip(null);gdStop("the demo ended");G.gd.demo=false},ms)}
function gdDemoShow(it,ms,o){var need=it.about==="marker"&&!gdMarker()||it.about==="middle"&&!(gdCentral()||{}).tour||it.about==="gbtn"&&!gdGbtn();
  if(need&&typeof setWall==="function"){setWall("ribeira");S.stop="closed";renderPhone(false)}return gdStart(it,"the Editor's demo",Object.assign({pull:true,ms:ms},o||{}))}
var GD_FIELD_CHIP={essential:function(it){return it.essential?"Essential: it skips the breathing room":"Not Essential: it waits for the breathing room ("+S.guidance.breathing+" s)"},
  doneWhen:function(it){return it.doneWhen?"Done when "+GD_EVENTS[it.doneWhen]+": then never again":"Never done: only Shows and Again after limit it"},
  importance:function(it){return"Importance "+it.importance+" of 5: when two are due, the higher scores more"}};
function gdDemoItem(id,field){var my=gdDemoBegin(),it=gdItemNow(id);if(!it)return;
  if(!it.on){gdChip("Off: the "+it.label+" never shows");gdDemoEnd(my,3500);return}
  var c=gdDemoShow(it);if(GD_FIELD_CHIP[field])gdChip(GD_FIELD_CHIP[field](it));gdDemoEnd(my,Math.max(3500,c.until-c.at+300))}
/* Shows and Again after: the item, the ×10 countdown to its next time, the item again, then how often in words */
function gdDemoTwice(id){var my=gdDemoBegin(),it=gdItemNow(id);if(!it)return;
  if(!it.on){gdChip("Off: the "+it.label+" never shows");gdDemoEnd(my,3500);return}
  var once=it.shows==="once"||it.shows==="visit",gap=+it.again>0?+it.again:+S.guidance.breathing,why=+it.again>0?"Again after "+it.again+" s":"Breathing room "+S.guidance.breathing+" s";
  gdDemoShow(it,2600);gdChip("×10 · shown 1"+(it.shows==="upto"||it.shows==="until"?" of "+it.max:"")+" this visit");
  G.gd.demoT=setTimeout(function(){if(!gdDemoLive(my))return;gdStop("the demo");
    if(once){gdChip(it.shows==="once"?"Once ever: never again for this visitor":"Once a visit: again at the next visit");gdDemoEnd(my,3500);return}
    var left=gap;gdChip("×10 · "+why+": "+left+" s");G.gd.demoI=setInterval(function(){if(!gdDemoLive(my)){clearInterval(G.gd.demoI);return}left--;
      if(left>0){gdChip("×10 · "+why+": "+left+" s");return}clearInterval(G.gd.demoI);gdDemoShow(it,2600);
      gdChip("×10 · shown 2"+(it.shows==="upto"||it.shows==="until"?" of "+it.max:"")+" this visit");G.gd.demoT=setTimeout(function(){if(!gdDemoLive(my))return;gdStop("the demo");gdChip(gdHowOften(it));gdDemoEnd(my,3200)},2800)},100)},2800)}
/* Overall: Guidance On shows the first tip (Off: a chip says none); Breathing room: one tip, the ×10 countdown, the next */
function gdDemoOverall(f){var my=gdDemoBegin(),its=gdItems().filter(function(x){return x.on});
  if(!S.guidance.on||!its.length){gdChip("Guidance Off: no tips (the ✦ and Ask still answer)");gdDemoEnd(my,3500);return}
  if(f==="on"||its.length<2){var c=gdDemoShow(its[0]);gdChip("Guidance On: a tip at a pause, one at a time");gdDemoEnd(my,Math.max(3500,c.until-c.at+300));return}
  var left=+S.guidance.breathing;gdDemoShow(its[0],2600);G.gd.demoT=setTimeout(function(){if(!gdDemoLive(my))return;gdStop("the demo");gdChip("×10 · Breathing room: "+left+" s");
    G.gd.demoI=setInterval(function(){if(!gdDemoLive(my)){clearInterval(G.gd.demoI);return}left--;if(left>0){gdChip("×10 · Breathing room: "+left+" s");return}
      clearInterval(G.gd.demoI);gdDemoShow(its[1],2600);gdChip("×10 · then the next tip");gdDemoEnd(my,3000)},100)},2800)}
/* the round 2 demos: a sequence of steps on the ×10 clock (show an item, a chip, a countdown, an action), each step ended by a newer demo */
function gdDemoRun(my,steps,i){i=i||0;if(!gdDemoLive(my))return;if(i>=steps.length){gdDemoEnd(my,0);return}var s=steps[i];
  if(s.count!=null){var left=Math.round(s.count),rate=left>60?30:10,lab=s.label;if(left<=0){gdDemoRun(my,steps,i+1);return}gdChip("×"+rate+" · "+lab+": "+left+" s");clearInterval(G.gd.demoI);
    G.gd.demoI=setInterval(function(){if(!gdDemoLive(my)){clearInterval(G.gd.demoI);return}left--;if(left>0){gdChip("×"+rate+" · "+lab+": "+left+" s");return}clearInterval(G.gd.demoI);gdDemoRun(my,steps,i+1)},1000/rate);return}
  if(s.stop)gdStop("the demo");if(s.show){var it=gdItemNow(s.show);if(it)gdDemoShow(it,s.ms||2600,s.o)}if(s.fn)s.fn();if(s.chip!=null)gdChip(s.chip);
  G.gd.demoT=setTimeout(function(){gdDemoRun(my,steps,i+1)},s.wait!=null?s.wait:2600)}
function gdDemo(steps){var my=gdDemoBegin();gdDemoRun(my,steps.concat([{stop:true,wait:400}]))}
function gdOnItems(kind){return gdItems().filter(function(x){return x.on&&(!kind||x.kind===kind)})}
/* a tip on the bubble, swiped away as a finger does (the demo's own: no memory) */
function gdDemoSwipe(){var b=ph.querySelector(".gdbub:not(.gd-out)");if(!b)return;if(!S.reduce){b.style.transition="translate .3s ease, opacity .3s ease";b.style.translate=Math.round(ph.offsetWidth*.6)+"px 0px";b.style.opacity=".2"}setTimeout(function(){gdSwipe(1)},300)}
var GD_DEMO_ROWS={
  perMinute:function(){var n=+S.guidance.perMinute,its=gdOnItems(),st=[];for(var k=0;k<=n&&its.length;k++){var it=its[k%its.length];
      if(k===n){st.push({count:Math.max(5,60-n*4),label:"At most "+n+" a minute, the next tip"});st.push({show:it.id,chip:"×10 · then the next tip: a new minute"})}
      else st.push({stop:true,show:it.id,ms:1800,chip:"×10 · tip "+(k+1)+" of at most "+n+" in this minute",wait:2000})}return st},
  perVisit:function(){var its=gdOnItems(),all=gdItems().reduce(function(a,o){return a+gdVisitOf(S.gdVisitor,o.id).shown},0);
    return[{show:its[0]&&its[0].id,chip:"At most "+S.guidance.perVisit+" a visit: "+all+" shown so far this visit",wait:3000},{stop:true,chip:"After "+S.guidance.perVisit+": quiet until the next visit (an Essential item still shows)",wait:2800}]},
  freshTau:function(){var tau=+S.guidance.freshTau,its=gdOnItems(),f=function(t){return gdN(1-Math.exp(-t/tau))},id=its[0]&&its[0].id;
    return[{show:id,chip:"Freshness: just shown, it scores ×0.00",wait:2200},{stop:true,chip:"×10 · after 30 s: ×"+f(30),wait:1000},{chip:"×10 · after τ = "+tau+" s: ×"+f(tau),wait:1000},{chip:"×10 · after 2τ: ×"+f(2*tau),wait:1000},{show:id,chip:"Fresh again: it scores in full",wait:2600}]},
  learnIgnore:function(){var its=gdOnItems().filter(function(x){return!x.essential}),br=+S.guidance.breathing;if(!its.length)return[{chip:"No tip to ignore"}];
    if(S.guidance.learnIgnore===false)return[{show:its[0].id,chip:"Off: three ignored in a row change nothing",wait:3000},{stop:true,count:br,label:"Breathing room"}];
    var st=[];[0,1,2].forEach(function(k){st.push({stop:true,show:its[k%its.length].id,ms:1400,chip:"×10 · ignored "+(k+1)+" in a row (its time ran out)",wait:1700})});
    st.push({stop:true,count:br*2,label:"Breathing room ×2 after 3 ignored"});st.push({show:its[0].id,chip:"A tap on a tip brings the breathing room back to "+br+" s"});return st},
  learnDismiss:function(){var it=gdOnItems("app").filter(function(x){return+x.again>0})[0]||gdOnItems()[0];if(!it)return[];var on=S.guidance.learnDismiss!==false;
    return[{show:it.id,ms:4000,chip:"A swipe away: dismissed",wait:1300},{fn:gdDemoSwipe,wait:700},{count:on?3*it.again:+it.again,label:on?"Dismissed: Again after ×3":"Off: Again after as it is"},
      {show:it.id,chip:on?"An App tip waits ×3; a Content item rests its group 5 min":"Off: a dismissal changes nothing"}]},
  fewer:function(){var it=gdOnItems()[0];if(!it)return[];if(!S.guidance.fewer)return[{show:it.id,ms:4000,chip:"Off: a long press does nothing",wait:3000}];
    return[{show:it.id,ms:9000,chip:"A long press on the tip…",wait:900},{fn:function(){gdAskFewer()},chip:"Fewer like this? Fewer: its group rests for the visit",wait:3600}]},
  remember:function(){var on=S.guidance.remember!==false,op=gdItemNow("opening");
    return[{chip:"×10 · Leave for 31 min: the visit ends",wait:1500},{show:op&&op.on?"opening":null,o:{ret:on},chip:on?"On: Welcome back (a returning visitor, the stage from 5 min)":"Off: a first visit again (nothing remembered)",wait:3400}]}};
function gdDemoKind(ki,f){var k=S.guidance.kinds[ki];if(!k)return;var its=gdOnItems(k.id);
  if(f==="on"){if(!k.on||!its.length){gdDemo([{chip:k.name+" Off: none of its items show",wait:3200}]);return}gdDemo([{show:its[0].id,chip:k.name+" On: its items take part in the score",wait:3200}]);return}
  if(!its.length){gdDemo([{chip:k.name+": no item is On",wait:3000}]);return}
  if(f==="cap"){var n=its.reduce(function(a,o){return a+gdVisitOf(S.gdVisitor,o.id).shown},0);gdDemo([{show:its[0].id,chip:k.name+": at most "+k.cap+" a visit ("+n+" so far)",wait:3200}]);return}
  if(!(+k.gap>0)){gdDemo([{show:its[0].id,chip:k.name+": no gap of its own, only the breathing room",wait:3200}]);return}
  gdDemo([{show:its[0].id,chip:"×10 · a "+k.name+" item",wait:2200},{stop:true,count:+k.gap,label:k.name+"'s gap"},{show:(its[1]||its[0]).id,chip:"×10 · then the next "+k.name+" item"}])}
function gdDemoGroup(ki,gi){var g=S.guidance.kinds[ki]&&S.guidance.kinds[ki].groups[gi];if(!g)return;var it=g.items.map(gdItemNow).filter(function(x){return x&&x.on})[0];
  if(!it){gdDemo([{chip:g.name+": no item is On",wait:3000}]);return}
  var r=gdCheck(it,gdItems(),S.gdVisitor,Object.assign(gdContext(),{onWall:true,stillSince:-Infinity,movedAt:null,touchAt:null,showing:null,speaking:false,demo:false,panel:null}),gdNow());
  gdDemo([{show:it.id,chip:"Weight ×"+gdN(g.weight,1)+": "+g.name+"'s items' scores ×"+gdN(g.weight,1)+(r.ok?" (the "+it.label+": "+gdN(r.score)+")":""),wait:3400}])}
function gdDemoAlong(id){var it=gdItemNow(id);if(!it)return;var kw=GD_MIX[it.kind]||[100,100,100],w=function(s){return it.along==="steady"?kw[1]/100:gdCurve(kw,s)/100*gdCurve(GD_CURVE[it.along]||GD_CURVE.kind,s)},lab=(GD_ALONG.filter(function(a){return a[0]===it.along})[0]||[0,""])[1];
  gdDemo([{chip:"×10 · "+lab+": on arrival (s 0) ×"+gdN(w(0)),wait:1100},{chip:"×10 · "+lab+": a third in (s 0.3) ×"+gdN(w(0.3)),wait:1100},{chip:"×10 · "+lab+": at the end (s 1) ×"+gdN(w(1)),wait:1100},
    {show:it.on?id:null,chip:"Along the visit, "+lab.toLowerCase()+": "+gdAlongWords(it),wait:3200}])}
function gdDemoTour(id){var it=gdItemNow(id);if(!it)return;var my=gdDemoBegin();gdChip(it.duringTour==="between"?"During a tour · Between steps: the "+it.label+" may show at a pause between two steps":"During a tour · Never: the "+it.label+" waits for the tour to end");gdDemoEnd(my,4200)}
var GD_DEMOS={"guidance.on":{wall:true,act:function(){gdDemoOverall("on")},ringAfter:600,ring:".gdbub|.gdchip"},
  "guidance.breathing":{wall:true,act:function(){gdDemoOverall("breathing")},ringAfter:600,ring:".gdbub|.gdchip"}};
Object.keys(GD_DEMO_ROWS).forEach(function(f){GD_DEMOS["guidance."+f]={wall:true,act:function(){gdDemo(GD_DEMO_ROWS[f]())},ringAfter:600,ring:".gdbub|.gdchip"}});
S.guidance.kinds.forEach(function(k,ki){var kp="guidance.kinds."+ki+".";["on","cap","gap"].forEach(function(f){GD_DEMOS[kp+f]={wall:true,act:function(){gdDemoKind(ki,f)},ringAfter:600,ring:".gdbub|.gdchip"}});
  k.groups.forEach(function(g,gi){GD_DEMOS[kp+"groups."+gi+".weight"]={wall:true,act:function(){gdDemoGroup(ki,gi)},ringAfter:600,ring:".gdbub|.gdchip"}})});
Object.keys(S.guidance.items).forEach(function(id){var p="guidance.items."+id+".";
  ["on","importance","essential","doneWhen"].forEach(function(f){GD_DEMOS[p+f]={wall:true,act:function(){gdDemoItem(id,f)},ringAfter:600,ring:".gdbub|.gdchip"}});
  ["shows","max","again"].forEach(function(f){GD_DEMOS[p+f]={wall:true,act:function(){gdDemoTwice(id)},ringAfter:600,ring:".gdbub|.gdchip"}});
  GD_DEMOS[p+"along"]={wall:true,act:function(){gdDemoAlong(id)},ringAfter:600,ring:".gdbub|.gdchip"};
  GD_DEMOS[p+"duringTour"]={tour:"g1",act:function(){gdDemoTour(id)},ringAfter:600,ring:".gdchip"}});
rowDemos(GD_DEMOS);
