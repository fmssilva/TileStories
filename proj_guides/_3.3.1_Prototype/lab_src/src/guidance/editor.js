/* Guidance in the Editor: Guide tab > 4 · Guidance. 1 · Overall: Guidance On / Off, Breathing room, At most a minute, At most a visit,
   Freshness, the learning (Ignored three in a row, After a dismissal, Fewer like this?), Remember across visits, the long
   press's words. 2 · App and 3 · Content: the kind's rows (On, At most a visit, Gap, its weight along the visit read back in words), its
   groups (their On, weight, own cap a visit and own gap) and each item a row (On, its label, its importance dots, how often, ▸ Show now)
   that opens its fields: Label, Says per language, its own rows (What's in view: Lines per point and the points on the wall now; The ✦:
   After pokes on Azu, After idle), Essential, Shows, At most, Again after, Done when, During a tour, Along the visit, the fixed ones in
   words and the read-back. Ask › Ask about the view › Azu comments links to What's in view (world/wall.js). 0 · Preview & Test: Show next now and why; Why not now? (an item's reason, in the scheduler's own words); the visitor
   (first or returning, the visits, the active minutes, the stage, the features used, each item's memory; New visit, Leave for 31 min,
   Reset the visitor); the Log of the last 30 decisions. Each row's demo (rowDemos): an item's row puts the phone on the wall and shows
   the item now (a demo is pulled: it never writes the visitor's memory); a limit or a learning row shows its effect on a ×10 clock with
   the countdown chip. Global Scene > Onboarding hints links here (the wall hint's pill is gone). Unity: the POI Editor's Guide tab, its
   Guidance section drawn from GuidanceCatalog, each row's ▸ a Play-mode preview (PlayMode Live Config); Why not now? and the Log read
   GuidanceScheduler.WhyNot and the service's decision log. */
COMP["gd:overall"]=["Guide","Guidance › Overall"];
COMP["gd:i:opening"]=["Guide","Guidance › The start › Opening message"];COMP["gd:i:tapmarker"]=["Guide","Guidance › The start › Tap a marker"];
COMP["gd:i:askview"]=["Guide","Guidance › The wall › Ask about the view"];COMP["gd:i:tourof"]=["Guide","Guidance › Tours › A tour of {point}"];
COMP["gd:i:gbhint"]=["Guide","Guidance › The wall › The ✦"];COMP["gd:i:inview"]=["Guide","Guidance › What's in view › {point} in the middle"];
FOCUS["gd:overall"]=["card","#wall","closed"];FOCUS["gd:app"]=["card","[data-marker]","closed"];FOCUS["gd:content"]=["card","[data-marker]","closed"];
FOCUS["gd:g:start"]=["card","[data-marker]","closed"];FOCUS["gd:g:wall"]=["card",".gbtn-wrap","closed"];FOCUS["gd:g:tours"]=["card","[data-marker]","closed"];FOCUS["gd:g:inview"]=["card","#wall","closed"];
FOCUS["gd:i:opening"]=["card",".gbtn-wrap","closed"];FOCUS["gd:i:tapmarker"]=["card","[data-marker]","closed"];FOCUS["gd:i:askview"]=["card",".gbtn-wrap","closed"];FOCUS["gd:i:tourof"]=["card","[data-marker]","closed"];
FOCUS["gd:i:gbhint"]=["card",".gbtn-wrap","closed"];FOCUS["gd:i:inview"]=["card","#wall","closed"];
/* round 4: App › The card (cues), each opened on the card at its element; App › How to use the card (its steps' words, the ✦ menu's) */
var GD_CUE_SEL={nudge:"#grab",label:".locpin",navbar:"#navbar",ba:"[data-ba]",strip:".el-car,.hstrip",term:".gterm"};
COMP["gd:g:card"]=["Guide","Guidance › The card (cues)"];FOCUS["gd:g:card"]=["card","#grab","half"];COMP["gd:g:howto"]=["Guide","Guidance › How to use the card"];FOCUS["gd:g:howto"]=["card",".chrome","half"];
Object.keys(GD_CUE_SEL).forEach(function(id){COMP["gd:i:"+id]=["Guide","Guidance › The card › "+S.guidance.items[id].label];FOCUS["gd:i:"+id]=["card",GD_CUE_SEL[id].split(",")[0],id==="nudge"||id==="label"?"half":"full"]});
gdGroupById("howto").items.forEach(function(id){var it=S.guidance.items[id];COMP["gd:i:"+id]=["Guide","Guidance › How to use the card › "+it.label];FOCUS["gd:i:"+id]=["card",it.sel||".hero","full"]});
/* a guidance word's place in the tab (Languages & Terms' links; Global Scene's link) */
function gdKindOfGroup(gid){var k=null;S.guidance.kinds.forEach(function(x){x.groups.forEach(function(g){if(g.id===gid)k=x.id})});return k}
on("guidePath.answer",function(o){if(o==="gd:overall"||/^gd:(app|content)$/.test(o))return["gt4",o];if(o.indexOf("gd:g:")===0)return["gt4","gd:"+(gdKindOfGroup(o.slice(5))||"app"),o];
  var m=o.match(/^gd:i:(.+)$/),it=m&&S.guidance.items[m[1]];if(it)return["gt4","gd:"+it.kind,"gd:g:"+it.group,o]});
/* the self-test: a tip's words show on the wall, at its moment (behaviour); every guidance option is timing (behaviour) */
TX_DOM.wall_tip="shown in the guide's tip beside the nearest marker";TX_DOM.gd_opening="shown in the guide's opening tip on the wall";TX_DOM.gd_opening_back="shown in the opening tip to a returning visitor";
TX_DOM.gd_askview="shown in the guide's tip beside the ✦";TX_DOM.gd_tourof="shown in the tour invitation on the wall";
TX_DOM.fab_comment_1="shown in Azu's remark on the point in the middle, in turn";TX_DOM.fab_comment_2=TX_DOM.fab_comment_1;TX_DOM.fab_comment_3=TX_DOM.fab_comment_1;
TX_DOM.fab_comment_card="shown in Azu's remark on a point with a card";TX_DOM.gb_hint="shown in the guide's tip beside the ✦ (after pokes on Azu, or idle)";
TX_DOM.hint_navbar="shown over the Navigation Bar in its cue";TX_DOM.gd_how="shown in the ✦ menu";TX_DOM.gd_how_wall="shown in How does this work? on the wall";TX_DOM.gd_how_open="shown in How does this work? on the wall";
["how_grab","how_settings","how_bar","how_locate","how_block"].forEach(function(k){TX_DOM[k]="said in How to use the card's walkthrough"});
TX_DOM.gd_fewer="shown after a long press on a tip";TX_DOM.gd_fewer_yes="shown after a long press on a tip";TX_DOM.gd_fewer_no="shown after a long press on a tip";
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(guidance\\.|"));
/* the visit's time as the Log and the visitor read it: m:ss */
function gdClock(ms){var s=Math.max(0,Math.round(ms/1000));return Math.floor(s/60)+":"+("0"+s%60).slice(-2)}
/* ----- the rows ----- */
function gdDots(it){var p="@guidance.items."+it.id+".importance";return'<span class="gddots" role="group" aria-label="Importance">'+[1,2,3,4,5].map(function(n){
  return'<button type="button" data-set="'+p+'" data-v="'+n+'" aria-pressed="'+(+it.importance===n)+'"'+(n<=+it.importance?' class="on"':'')+' title="Importance '+n+' of 5" aria-label="Importance '+n+' of 5"></button>'}).join("")+'</span>'}
var GD_ABOUT_WORDS={nothing:"nothing (the wall)",marker:"the nearest marker (it must be on the screen)",gbtn:"the ✦ (it must be on the screen: on the wall, the way into Ask about the view; on the card, once the hero's Start is out of sight)",middle:"the point in the middle, one with a tour",
  point:"its point of the wall, when it is the one in the middle (one item a point of the wall, each with its own memory)"};
var GD_TAP_WORDS={close:"it closes (acted)",view:"Ask about the view opens (acted)",tour:"the tour of that point starts (acted)",remark:"a point with a card starts its tour; any other opens Ask about the view about it, the point lit (acted)",menu:"the ✦'s menu opens (acted; then it is done)"};
/* a per-point item's points on the wall now, and the line each says (Lines per point) */
function gdPointsWords(it){var ins=gdInstances(it.id),wn=S.camCfg.wall==="belem"?"the Belém panorama":"the Ribeira painting";
  return"One item a point of the wall in the Lab ("+wn+", "+ins.length+" points): "+ins.map(function(x){return x.pointName+(x.card&&it.cardLine!==false?" (a card: its own line)":"")}).join(", ")+
    ". Each once a visit, with its own memory; the others say the 3 remarks in turn, so never the same twice in a row."}
/* a cue's row (App › The card): On, its name, importance, how often, ▸ Show now; opened: Label, its words (where it has any), Shows (Once a
   visit / Every card), Done when (a step of How to use the card), the fixed ones in words (its element, its look, Reduce Motion, the cue
   room) and its read-back */
function gdCueRow(it){var p="guidance.items."+it.id+".",r=+S.guidance.cueRoom||0;
  var sum='<span class="gdi"><span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b><span class="badge">CUE</span>'+gdDots(it)+
    '<span class="gdhow">'+esc(gdHowOften(it))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Show it on the card now">&#9656; Show now</button></span>';
  var words=it.id==="label"?'<div class="note">Says: the Locate Button\'s own label, “'+esc(TXT("locate_label"))+'” (Languages &amp; Terms, owned by Card Controls › Locate Button).</div>':
    it.says?'<div class="sub2">Says · its words over the bar (Languages &amp; Terms, owned here)</div>'+txPanel("gd:i:"+it.id):'<div class="note">No words: motion on the element only.</div>';
  var body=row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the cue's name in these lists, in Card Controls > Hints, the Log and the scenes.")+words+
    row("Shows",pseg(p+"shows",GD_CUE_SHOWS.map(function(x){return x[0]}),GD_CUE_SHOWS.map(function(x){return x[1]})),"Once a visit: once it has shown, not again this visit. Every card: once on each card the visitor opens. Off (the row's On): never. A cue is not a tip: the tips' limits (at most a minute, a visit, App's cap) do not count it.")+
    row("Done when",pseg(p+"doneWhen",GD_DONE_CUE.map(function(x){return x[0]}),GD_DONE_CUE.map(function(x){return x[1]})),"Retired for good once the visitor has seen that step of How to use the card (its line heard): the walkthrough taught it, so the cue is not needed.")+
    '<div class="note gdfix">'+esc("Its element: "+GD_CUE_WHEN[it.about]+" · Its look: "+it.look+" · Under Reduce Motion: "+(it.reduce==="skip"?"skipped (it is a movement)":"shown, with no travel")+
      " · One at a time: never two cues at once, never with a tip; the cue room ("+r+" s, 1 · Overall) between a tip and a cue, either way; none between two cues · Never in a tour, with a panel open, while the visitor scrolls or touches (0.8 s), or in the card's first 1.5 s"+(it.when==="open"?" (it answers the opening itself, so the touch that opened the card and the first 1.5 s do not hold it)":"")+". Fixed for this cue.")+'</div>'+
    '<div class="note gdrb">'+esc(gdReadBack(it))+'</div>';
  return fo("gd:i:"+it.id,sum,null,body,4)}
/* a step of How to use the card: On, its name, ▸ Show now (the walkthrough from this step); opened: Label, its words, what it marks and
   what seeing it retires, its read-back */
function gdStepRow(it){var p="guidance.items."+it.id+".";
  var sum='<span class="gdi"><span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b><span class="gdhow">'+esc(gdHowOften(it))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Walk the card from this step">&#9656; Show now</button></span>';
  var body=row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the step's name in these lists and the read-back.")+
    '<div class="sub2">Says · its line in the walkthrough (Languages &amp; Terms, owned here)</div>'+txPanel("gd:i:"+it.id)+'<div class="note gdrb">'+esc(gdReadBack(it))+'</div>';
  return fo("gd:i:"+it.id,sum,null,body,4)}
function gdItemRow(it){if(it.form==="cue")return gdCueRow(it);if(it.form==="walk")return gdStepRow(it);var p="guidance.items."+it.id+".",k=gdKindOf(it),g=gdGroupOf(it),i=g?g.items.indexOf(it.id):0,pv=i>0&&g.mode==="order"?S.guidance.items[g.items[i-1]]:null;
  var sum='<span class="gdi"><span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b>'+(it.essential?'<span class="badge new">ESSENTIAL</span>':"")+gdDots(it)+
    '<span class="gdhow">'+esc(gdHowOften(it))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Show it on the phone now">&#9656; Show now</button></span>';
  var body=row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the item's name in these lists, in the read-back, the Log and the scenes. The visitor never sees it.")+
    '<div class="sub2">'+(Array.isArray(it.says)?"Says · its lines per language: "+it.says.length+" in turn"+(it.saysCard?", and a point with a card's own":"")+" (Languages &amp; Terms, owned here)":"Says · one line per language (Languages &amp; Terms, owned here)")+'</div>'+txPanel("gd:i:"+it.id)+
    (it.perPoint?row("Lines per point",pseg(p+"cardLine",["true","false"],["A point with a card: its own line","The 3 in turn for every point"]),"Which line each point says. The 3 remarks come in turn (never the same twice in a row). A point with a card can say its own line instead (“Tap and I'll show you around”), since a tap there starts its tour.")+'<div class="note gdpts">'+esc(gdPointsWords(it))+'</div>':"")+
    (it.pokes!=null?row("After pokes on Azu",pslider(p+"pokes",0,10,1),"Pokes on Azu in this visit that bring it (0: not by pokes). The tip waits for the finger to lift (0.8 s) and for Azu's own bubble (a joke after a poke) to end."):"")+
    (it.idleFor!=null?row("After idle (s)",pslider(p+"idleFor",0,90,5),"Seconds with no touch and the phone still that bring it (0: not by idle), counted from the visit's start at the earliest. Whichever comes first, the pokes or the idle."):"")+
    row("Essential",pseg(p+"essential",["true","false"],["Yes","No"]),"Yes: it skips the breathing room and the limits (never the gates: one at a time, never over a sentence, never while the phone moves or a panel is open), and goes before any other. For the visit's first words.")+
    row("Shows",pseg(p+"shows",GD_SHOWS.map(function(x){return x[0]}),GD_SHOWS.map(function(x){return x[1]})),"How often it may show: once ever; once a visit; up to n a visit; until done (its Done when), at most n a visit; or every time (only Again after and the breathing room limit it).")+
    (it.shows==="upto"||it.shows==="until"?row("At most",pslider(p+"max",1,10,1),"Times a visit.",true):"")+
    (it.shows==="once"||it.shows==="visit"?"":row("Again after (s)",pslider(p+"again",0,120,5),"Seconds after it ends before it may show again (its cooldown; 0: only the breathing room). The read-back says it as the visitor lives it: “at most about every 25 s”. A dismissed App tip waits ×3 (1 · Overall > After a dismissal). Not for Once ever or Once a visit.",true))+
    row("Done when",pseg(p+"doneWhen",GD_DONE.map(function(x){return x[0]}),GD_DONE.map(function(x){return x[1]})),"The visitor's action that retires it for good (this visit and the next, until Reset the visitor): Apple TipKit's rule, a tip gone once its feature is used. A tip showing when its feature is used counts as acted.")+
    row("During a tour",pseg(p+"duringTour",GD_TOUR.map(function(x){return x[0]}),GD_TOUR.map(function(x){return x[1]})),"Never: quiet for the whole tour. Between steps: it may show at a pause between two steps of a tour (never over a step's words, never over the block being told).")+
    row("Along the visit",pseg(p+"along",GD_ALONG.map(function(x){return x[0]}),GD_ALONG.map(function(x){return x[1]})),"How its weight follows the stage of the visit (s: 0 on arrival, 1 after 15 active minutes or every milestone done). As the kind: the kind's weight alone. Steady: the kind's middle weight all visit. Early: full for the first third, then fading to nothing. Late: nothing for the first third, then growing to full. A weight in the score, never a quota.")+
    '<div class="note gdfix">'+esc("About: "+(GD_ABOUT_WORDS[it.about]||it.about)+" · Mascot goes: "+(it.goes==="beside"?"beside it, and points":it.points?"stays beside the ✦ and points at it":"stays beside the ✦")+
      " · Focus: "+(it.focus==="ring"?"Ring":"None")+" · When: "+(it.when==="visit"?"the visit started":it.when==="settle"?"its point settles in the middle":"at a pause")+(+it.stillFor>0?", the phone still "+it.stillFor+" s (never while panning, nor 0.7 s after)":"")+(+it.notBefore>0?", not before "+it.notBefore+" s of the visit":"")+(pv&&+it.afterPrev>0?", at least "+it.afterPrev+" s after the "+pv.label:"")+
      (it.lively||it.notSolemn?", "+gdWhenWords(Object.assign({},it,{pokes:0,idleFor:0})).join(", "):"")+
      " · A tap: "+(GD_TAP_WORDS[it.tap]||"it closes")+" · Spoken: "+(k&&k.spoken==="bubble"?"no, a bubble only (App)":"once the visitor has heard the guide this visit, else a bubble ("+(k?k.name:"")+")")+
      (it.id==="gbhint"?" · Mascot > Tap on Azu = Points to the ✦ button shows it at once, as asked: a pull, past every rule but one at a time":"")+
      (it.about==="gbtn"?" · Two items about the ✦ never both in the visit's first minute (the first in this list goes first)":"")+". Fixed for this item.")+'</div>'+
    '<div class="note gdrb">'+esc(gdReadBack(it))+'</div>';
  return fo("gd:i:"+it.id,sum,null,body,4)}
/* a kind's weight along the visit, read back in words (the mix: a fixed table) */
function gdKindWords(k){var w=GD_MIX[k.id]||[0,0,0];return"Its weight along the visit: "+w[0]+" % on arrival, "+w[1]+" % a third of the way in, "+w[2]+" % at the end (s = 0 / 0.3 / 1, straight lines between): a weight in each item's score, never a quota; an item's Along the visit shapes it further. "+
  (k.spoken==="bubble"?"Spoken: never, a bubble only.":"Spoken: once the visitor has heard the guide this visit (a tour step, an answer), else a bubble.")}
function gdKindSub(k){return k.id==="app"?"how to use the app: a bubble, never spoken":"what is here and what next: spoken once the guide has been heard"}
/* ----- 0 · Preview & Test: the read-outs ----- */
/* an item's memory as the Editor reads it: a per-point item's, summed over its points on this wall (the latest line used) */
function gdMemOf(def){var v=S.gdVisitor,r={shown:0,ever:0,last:null,line:"",acted:0,dismissed:0,done:false,by:""};
  gdInstances(def.id).forEach(function(it){var vm=gdVisitOf(v,it.id),em=gdEverOf(v,it.id);r.shown+=vm.shown;r.ever+=em.shown;r.acted+=em.acted||0;r.dismissed+=em.dismissed||0;
    if(em.last!=null&&(r.last==null||em.last>r.last)){r.last=em.last;r.line=em.line||""}if(em.done){r.done=true;r.by=em.by}});return r}
function gdVisitorLine(){var v=S.gdVisitor;return"Visit "+v.visits+(v.visits>1?" (a returning visitor)":" (a first visit)")+" · "+gdDefs().map(function(it){var m=gdMemOf(it);
  return it.label+": "+m.shown+" this visit, "+m.ever+" ever"+(m.done?", done ("+GD_EVENTS[m.by]+")":"")}).join(" · ")}
function gdVisitorMore(){var v=S.gdVisitor,vi=v.visit,now=gdNow(),st=gdStage(v,now),used=GD_FEATURES.filter(function(f){return v.used[f[0]]!=null}).map(function(f){return f[1]+" ("+gdClock(v.used[f[0]]-vi.at)+")"}),
    rest=Object.keys(vi.rest||{}).filter(function(g){return vi.rest[g]>now}).map(function(g){var gg=gdGroupById(g);return(gg?gg.name:g)+(vi.fewer&&vi.fewer[g]?" (Fewer like this: the visit)":" ("+gdClock(vi.rest[g]-now)+")")});
  return(v.visits>1?"A returning visitor":"A first visit")+" · visits "+v.visits+" · active "+gdN(st.active,1)+" min"+(st.returning?" (the stage counts from 5 min)":"")+" · the stage s "+gdN(st.s)+
    " · met the guide: "+(v.met?"yes":"no")+" · heard the guide this visit: "+(vi.heard?"yes":"no")+" · pokes on Azu: "+(vi.pokes||0)+" · features used: "+(used.join(", ")||"none")+
    " · breathing room ×"+(vi.breathMul||1)+" ("+(vi.ignoredRun||0)+" ignored in a row) · resting: "+(rest.join(", ")||"none")+" · Remember across visits: "+(S.guidance.remember!==false?"On":"Off")}
function gdMemTable(){var v=S.gdVisitor;return'<table class="tbl gdmem"><thead><tr><th>Item</th><th>Visit</th><th>Ever</th><th>Last shown</th><th>The line used</th><th>Acted</th><th>Dismissed</th><th>Done</th></tr></thead><tbody>'+
  gdDefs().map(function(it){var m=gdMemOf(it);return'<tr><td>'+esc(it.label)+(it.perPoint?" <small>(its points summed)</small>":"")+'</td><td>'+m.shown+'</td><td>'+m.ever+'</td><td>'+(m.last!=null?gdClock(m.last-v.visit.at):"")+'</td><td>'+esc(m.line)+'</td><td>'+m.acted+'</td><td>'+m.dismissed+'</td><td>'+(m.done?esc(GD_EVENTS[m.by]||m.by):"")+'</td></tr>'}).join("")+'</tbody></table>'}
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
    row("Breathing room (s)",pslider("guidance.breathing",5,60,1),"Seconds between any two tips, from the end of one to the start of the next (an Essential item skips it). Doubled after three ignored in a row (below).")+
    row("Cue room (s)",pslider("guidance.cueRoom",0,30,1),"Seconds between a tip and a card cue (a hint on the card: the grabber's nudge, the bar's ring…), either way: from the end of a tip to a cue, and from the end of a cue to a tip. Shorter than the breathing room: a cue is a small movement on the element, no bubble, no mascot. Two cues follow each other with no room, one at a time. 0: none (still never at once).")+
    row("At most a minute",pslider("guidance.perMinute",1,6,1),"Tips that may start in any 60 s (an Essential item skips it). Talkative scripts meet it; most visits never do.")+
    row("At most a visit",pslider("guidance.perVisit",5,60,1),"Tips in one visit, all kinds together (an Essential item skips it). A kind has its own cap below.")+
    row("Freshness (s)",pslider("guidance.freshTau",30,600,10),"Seconds (τ) to recover. An item just shown scores ×0, then recovers: 1 − e^(−t/τ) (after τ: ×0.63, after 2τ: ×0.86). So a tip that was just seen yields to another when both are due.")+
    row("Ignored three in a row",pseg("guidance.learnIgnore",["true","false"],["Double the breathing room","Off"]),"Three tips in a row whose time ran out (never tapped, never swiped): the breathing room doubles for the visit (six: ×4, no more). A tip acted on brings it back. The opening's time running out is not ignoring.")+
    row("After a dismissal",pseg("guidance.learnDismiss",["true","false"],["Wait longer","Off"]),"A tip swiped away: an App tip waits ×3 its Again after; a Content item rests its whole group 5 min.")+
    row("Fewer like this?",pseg("guidance.fewer",["true","false"],["Offered on a long press","Off"]),"A long press on a tip asks Fewer like this? Fewer: its group rests for the rest of the visit. Keep: the tip stays. Its words are below.")+
    row("Remember across visits",pseg("guidance.remember",["true","false"],["On","Off"]),"On: the visitor's memory stays on the phone from visit to visit (what is done stays done; the opening says Welcome back; the stage counts from 5 min). Off, for kiosks and demos: every visit starts as a first visit. A visit ends after 30 min away.")+
    '<div class="sub2">The long press · its words (Languages &amp; Terms, owned here)</div>'+txPanel("gd:overall");
  var gs="";S.guidance.kinds.forEach(function(k,ki){var kp="guidance.kinds."+ki+".",inner=row("On",pseg(kp+"on",["true","false"],["On","Off"]),"Off: none of the "+k.name+" items show (each keeps its own On).")+
      row("At most a visit",pslider(kp+"cap",1,40,1),"The "+k.name+" items in one visit, all together (an Essential item skips it).")+
      row("Gap (s)",pslider(kp+"gap",0,180,5),"Seconds from the end of one "+k.name+" item to the start of the next (0: only the breathing room). An Essential item skips it.")+
      '<div class="note gdmix">'+esc(gdKindWords(k))+'</div>';
    k.groups.forEach(function(g,gi){var items=g.items.map(function(id){return S.guidance.items[id]}).filter(Boolean),gp=kp+"groups."+gi+".",per=items.some(function(x){return x.perPoint});
      /* a pulled group (How to use the card): no weight, cap or gap (never scheduled, never capped); where it opens, its words, its steps */
      if(g.pulled){inner+=fo("gd:g:"+g.id,esc(g.name)+' <span class="sub">· a walkthrough, in order, pulled only</span>',null,
        '<div class="note">Pulled only: never scheduled, never capped, never by itself. It opens from the visitor\'s Settings and from the ✦ menu (How does this work?). On a card it walks the card, step by step, through the guide\'s tour; on the wall one bubble says how the wall works and offers to open a card. A step seen (its line heard) retires its cue in The card (cues).</div>'+
        row("On",pseg(gp+"on",["true","false"],["On","Off"]),"Off: the walkthrough is gone, and with it its entries in Settings and the ✦ menu.")+
        row("In Settings",pseg(gp+"inSettings",["true","false"],["Shown","Hidden"]),"A button at the foot of the visitor's Settings, “"+TXT("settings_how")+"” (its word in Card Controls › Settings panel). It closes the panel and walks the card.")+
        row("In the ✦ menu",pseg(gp+"inMenu",["true","false"],["Shown","Hidden"]),"An item of the ✦ menu, “"+TXT("gd_how")+"”, on the card and on the wall: on the card it walks the card; on the wall one bubble says how the wall works, with a chip that opens a card.")+
        '<div class="sub2">Its words · the ✦ menu\'s item and the wall\'s bubble (Languages &amp; Terms, owned here)</div>'+txPanel("gd:g:"+g.id)+
        '<div class="note">Its steps, in this order (a row: On, the step, ▸ Show now walks the card from it; open it for its words):</div>'+items.map(gdItemRow).join(""),3);return}
      inner+=fo("gd:g:"+g.id,esc(g.name)+' <span class="sub">· '+(g.mode==="order"?"In order: the visit's first words, then the first gesture":per?"one item a point of the wall: Azu's remarks":g.id==="card"?"the card's hints, one cue each: motion on the element, no mascot":"each on its own")+'</span>',null,
        '<div class="note">'+(g.mode==="order"?"In order: an item waits until the one before it has shown this visit.":g.id==="card"?"Each cue on its own, at the moment its feature is in reach, taking turns with the tips (one at a time, the cue room between them). Card Controls › Hints shows them as a view.":"Each item on its own.")+' A row: On, its name, importance (●), how often, ▸ Show now; open it for its fields.</div>'+
        row("On",pseg(gp+"on",["true","false"],["On","Off"]),"Off: none of "+g.name+"'s items show (each keeps its own On)."+(per?" Ask › Ask about the view › Azu comments is this same switch.":""))+
        row("Weight",pslider(gp+"weight",0,2,0.1),"×: this group's items' scores against the others' (1: as they are; 0: they never win).")+
        row("At most a visit",pslider(gp+"cap",0,40,1),"The "+g.name+" items in one visit, all together (0: no cap of its own; "+(g.id==="card"?"a cue is not a tip, so "+k.name+"'s cap does not count it":k.name+"'s "+k.cap+" still holds")+"). An Essential item skips it.")+
        row("Gap (s)",pslider(gp+"gap",0,120,1),"Seconds from the end of one "+g.name+" item to the start of the next. 0: "+(g.id==="card"?"none: two cues follow each other with no room":"as the kind ("+k.name+"'s "+k.gap+" s)")+"; more: the group's own, in place of the kind's. The breathing room between any two tips ("+S.guidance.breathing+" s) still holds.")+
        items.map(gdItemRow).join(""),3)});
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
function gdOnWallNow(){if(G.on)stopTour();if(typeof hideStopUi==="function")hideStopUi();S.settingsOpen=false;S.contentsOpen=false;G.fab.menu=false;G.cam.open=false;S.view="card";S.stop="closed";renderPhone(false)}
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
/* an item by its id; a per-point item by its definition's id: the one whose point is in the middle now (else its first point) */
function gdItemNow(id){var all=gdItems(),x=all.filter(function(o){return o.id===id})[0];if(x)return x;var ins=all.filter(function(o){return o.base===id}),c=gdCentral();
  return ins.filter(function(o){return c&&o.point===c.id})[0]||ins[0]}
/* a demo begins: the guidance waits (gate "an Editor demo is showing") while it runs; a newer demo of any row ends it */
function gdDemoBegin(){G.gd.demo=true;G.gd.demoOwner=G.demoSeq;clearTimeout(G.gd.demoT);clearInterval(G.gd.demoI);gdStop("an Editor demo");gdChip(null);return G.demoSeq}
function gdDemoLive(my){if(G.demoSeq===my)return true;if(G.gd.demoOwner===my){G.gd.demo=false;gdChip(null);clearInterval(G.gd.demoI)}return false}
function gdDemoEnd(my,ms){G.gd.demoT=setTimeout(function(){if(!gdDemoLive(my))return;gdChip(null);gdStop("the demo ended");G.gd.demo=false},ms)}
function gdDemoShow(it,ms,o){if(it.form==="cue"){cuePlace(it.id);return gdStart(it,"the Editor's demo",Object.assign({pull:true,ms:ms||4000},o||{}))}var need=it.about==="marker"&&!gdMarker()||it.about==="middle"&&!(gdCentral()||{}).tour||it.about==="gbtn"&&!gdGbtn()||it.about==="point"&&!gdCentral();
  if(need&&typeof setWall==="function"){setWall("ribeira");S.stop="closed";renderPhone(false)}
  /* a per-point item: the one of the point in the middle now */
  if(it.base){var m=gdItemNow(it.base);if(m)it=m}
  return gdStart(it,"the Editor's demo",Object.assign({pull:true,ms:ms},o||{}))}
var GD_FIELD_CHIP={essential:function(it){return it.essential?"Essential: it skips the breathing room":"Not Essential: it waits for the breathing room ("+S.guidance.breathing+" s)"},
  doneWhen:function(it){return it.doneWhen?"Done when "+GD_EVENTS[it.doneWhen]+": then never again":"Never done: only Shows and Again after limit it"},
  importance:function(it){return"Importance "+it.importance+" of 5: when two are due, the higher scores more"}};
function gdDemoItem(id,field){var my=gdDemoBegin(),it=gdItemNow(id);if(!it)return;
  if(!it.on){gdChip("Off: the "+it.label+" never shows");gdDemoEnd(my,3500);return}
  var c=gdDemoShow(it);if(GD_FIELD_CHIP[field])gdChip(GD_FIELD_CHIP[field](it));gdDemoEnd(my,Math.max(3500,c?c.until-c.at+300:0))}
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
function gdOnItems(kind){return gdItems().filter(function(x){return x.on&&x.groupOn&&(!kind||x.kind===kind)})}
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
  cueRoom:function(){var r=+S.guidance.cueRoom;return[{show:"gbhint",ms:2400,chip:"×10 · a tip (the ✦)",wait:2600},{stop:true,count:r,label:"The cue room after a tip"},{show:"navbar",chip:r>0?"×10 · then a cue: the bar's ring and its words":"Cue room 0: the cue right after the tip (never at once)",wait:3600}]},
  remember:function(){var on=S.guidance.remember!==false,op=gdItemNow("opening");
    return[{chip:"×10 · Leave for 31 min: the visit ends",wait:1500},{show:op&&op.on?"opening":null,o:{ret:on},chip:on?"On: Welcome back (a returning visitor, the stage from 5 min)":"Off: a first visit again (nothing remembered)",wait:3400}]}};
function gdDemoKind(ki,f){var k=S.guidance.kinds[ki];if(!k)return;var its=gdOnItems(k.id);
  if(f==="on"){if(!k.on||!its.length){gdDemo([{chip:k.name+" Off: none of its items show",wait:3200}]);return}gdDemo([{show:its[0].id,chip:k.name+" On: its items take part in the score",wait:3200}]);return}
  if(!its.length){gdDemo([{chip:k.name+": no item is On",wait:3000}]);return}
  if(f==="cap"){var n=its.reduce(function(a,o){return a+gdVisitOf(S.gdVisitor,o.id).shown},0);gdDemo([{show:its[0].id,chip:k.name+": at most "+k.cap+" a visit ("+n+" so far)",wait:3200}]);return}
  if(!(+k.gap>0)){gdDemo([{show:its[0].id,chip:k.name+": no gap of its own, only the breathing room",wait:3200}]);return}
  /* the kind's gap holds for the items whose group has no gap of its own */
  var kg=its.filter(function(x){return!(x.groupGap>0)});if(kg.length)its=kg;
  gdDemo([{show:its[0].id,chip:"×10 · a "+k.name+" item",wait:2200},{stop:true,count:+k.gap,label:k.name+"'s gap"},{show:(its[1]||its[0]).id,chip:"×10 · then the next "+k.name+" item"}])}
function gdDemoGroup(ki,gi){var g=S.guidance.kinds[ki]&&S.guidance.kinds[ki].groups[gi];if(!g)return;var it=g.items.map(gdItemNow).filter(function(x){return x&&x.on})[0];
  if(!it){gdDemo([{chip:g.name+": no item is On",wait:3000}]);return}
  var r=gdCheck(it,gdItems(),S.gdVisitor,Object.assign(gdContext(),{onWall:true,stillSince:-Infinity,movedAt:null,touchAt:null,showing:null,speaking:false,demo:false,panel:null}),gdNow());
  gdDemo([{show:it.id,chip:"Weight ×"+gdN(g.weight,1)+": "+g.name+"'s items' scores ×"+gdN(g.weight,1)+(r.ok?" (the "+it.label+": "+gdN(r.score)+")":""),wait:3400}])}
/* a group's On, its own cap and its own gap (the gap on the ×10 clock with the countdown chip, between two of its items) */
function gdDemoGroupField(ki,gi,f){var k=S.guidance.kinds[ki],g=k&&k.groups[gi];if(!g)return;var its=gdItems().filter(function(x){return x.group===g.id&&x.on&&x.kindOn});
  if(f==="on"&&g.on===false){gdDemo([{chip:g.name+" Off: none of its items show",wait:3200}]);return}
  if(!its.length){gdDemo([{chip:g.name+": no item is On",wait:3000}]);return}var a=gdItemNow(its[0].base||its[0].id);
  if(f==="on"){gdDemo([{show:a.id,chip:g.name+" On: its items take part in the score",wait:3200}]);return}
  if(f==="cap"){var n=its.reduce(function(s,o){return s+gdVisitOf(S.gdVisitor,o.id).shown},0);
    gdDemo([{show:a.id,chip:+g.cap>0?g.name+": at most "+g.cap+" a visit ("+n+" so far)":g.name+": no cap of its own ("+(g.id==="card"?"a cue is not a tip: "+k.name+"'s cap does not count it":k.name+"'s "+k.cap+" a visit holds")+")",wait:3200}]);return}
  if(!(+g.gap>0)){gdDemo([{show:a.id,chip:g.id==="card"?g.name+": no gap: two cues follow each other, one at a time":g.name+": no gap of its own, as the kind ("+k.name+"'s "+k.gap+" s)",wait:3200}]);return}
  var b=its.filter(function(x){return x.id!==a.id})[0]||a;
  gdDemo([{show:a.id,chip:"×10 · a "+g.name+" item",wait:2200},{stop:true,count:+g.gap,label:g.name+"'s gap"},{show:b.id,chip:"×10 · then the next "+g.name+" item"}])}
/* Lines per point: the remark of the point in the middle, with the line it says */
function gdDemoLines(id){var it=gdItemNow(id);if(!it)return;var on=S.guidance.items[id].cardLine!==false;
  gdDemo([{show:it.id,chip:on?"Lines per point: a point with a card says its own line, the others the 3 in turn":"Lines per point: the 3 in turn for every point, a card or not",wait:3600}])}
/* The ✦'s pokes: Azu reacts to n pokes (the demo's own: no memory), then the tip; its idle: the ×10 countdown, then the tip */
function gdDemoPokes(id){var it=gdItemNow(id),d=S.guidance.items[id];if(!it)return;var n=+d.pokes,st=[];
  if(!(n>0)){gdDemo([{chip:"Not by pokes (0)"+(+d.idleFor>0?": only after "+d.idleFor+" s idle":""),wait:3000}]);return}
  for(var k=1;k<=n;k++)st.push({fn:function(){azReact("r-giggle",450,"A")},chip:"A poke on Azu: "+k+" of "+n,wait:550});
  st.push({show:it.id,chip:"After "+n+" pokes on Azu: the ✦ tip"+(S.guideCfg.life==="lively"?"":" (Lively only: Azu is Calm now, so not by itself)"),wait:3400});gdDemo(st)}
function gdDemoIdle(id){var it=gdItemNow(id),d=S.guidance.items[id];if(!it)return;
  if(!(+d.idleFor>0)){gdDemo([{chip:"Not by idle (0)"+(+d.pokes>0?": only after "+d.pokes+" pokes on Azu":""),wait:3000}]);return}
  gdDemo([{count:+d.idleFor,label:"Idle: no touch, the phone still"},{show:it.id,chip:"After "+d.idleFor+" s idle: the ✦ tip",wait:3400}])}
function gdDemoAlong(id){var it=gdItemNow(id);if(!it)return;var kw=GD_MIX[it.kind]||[100,100,100],w=function(s){return it.along==="steady"?kw[1]/100:gdCurve(kw,s)/100*gdCurve(GD_CURVE[it.along]||GD_CURVE.kind,s)},lab=(GD_ALONG.filter(function(a){return a[0]===it.along})[0]||[0,""])[1];
  gdDemo([{chip:"×10 · "+lab+": on arrival (s 0) ×"+gdN(w(0)),wait:1100},{chip:"×10 · "+lab+": a third in (s 0.3) ×"+gdN(w(0.3)),wait:1100},{chip:"×10 · "+lab+": at the end (s 1) ×"+gdN(w(1)),wait:1100},
    {show:it.on?id:null,chip:"Along the visit, "+lab.toLowerCase()+": "+gdAlongWords(it),wait:3200}])}
function gdDemoTour(id){var it=gdItemNow(id);if(!it)return;var my=gdDemoBegin();gdChip(it.duringTour==="between"?"During a tour · Between steps: the "+it.label+" may show at a pause between two steps":"During a tour · Never: the "+it.label+" waits for the tour to end");gdDemoEnd(my,4200)}
var GD_DEMOS={"guidance.on":{wall:true,act:function(){gdDemoOverall("on")},ringAfter:600,ring:".gdbub|.gdchip"},
  "guidance.breathing":{wall:true,act:function(){gdDemoOverall("breathing")},ringAfter:600,ring:".gdbub|.gdchip"}};
Object.keys(GD_DEMO_ROWS).forEach(function(f){GD_DEMOS["guidance."+f]={wall:true,act:function(){gdDemo(GD_DEMO_ROWS[f]())},ringAfter:600,ring:".gdbub|.gdchip"}});
S.guidance.kinds.forEach(function(k,ki){var kp="guidance.kinds."+ki+".";["on","cap","gap"].forEach(function(f){GD_DEMOS[kp+f]={wall:true,act:function(){gdDemoKind(ki,f)},ringAfter:600,ring:".gdbub|.gdchip"}});
  k.groups.forEach(function(g,gi){var gp=kp+"groups."+gi+".";if(g.pulled){GD_DEMOS[gp+"on"]={act:function(){gdDemoHow("on")},ringAfter:1100,ring:".gfocus|.gdchip"};
      GD_DEMOS[gp+"inSettings"]={stop:"half",act:function(){gdDemoHow("settings")},ringAfter:500,ring:".vhow|.vset"};GD_DEMOS[gp+"inMenu"]={wall:true,act:function(){gdDemoHow("menu")},ringAfter:500,ring:'[data-gbgo="how"]|.gbmenu'};return}
    GD_DEMOS[kp+"groups."+gi+".weight"]={wall:true,act:function(){gdDemoGroup(ki,gi)},ringAfter:600,ring:".gdbub|.gdchip"};
    ["on","cap","gap"].forEach(function(f){GD_DEMOS[kp+"groups."+gi+"."+f]={wall:true,act:function(){gdDemoGroupField(ki,gi,f)},ringAfter:600,ring:".gdbub|.gdchip"}})})});
Object.keys(S.guidance.items).forEach(function(id){var p="guidance.items."+id+".",d=S.guidance.items[id];
  /* a cue: on the card at its element (never the wall); a step of How to use the card: the walkthrough from that step */
  if(d.form==="cue"){var rg=".gdchip|"+GD_CUE_SEL[id].split(",").join("|");["on","importance","doneWhen"].forEach(function(f){GD_DEMOS[p+f]={stop:id==="nudge"||id==="label"?"half":"full",act:function(){gdDemoCue(id,f)},ringAfter:700,ring:rg}});
    GD_DEMOS[p+"shows"]={stop:"full",act:function(){gdDemoCueShows(id)},ringAfter:700,ring:rg};return}
  if(d.form==="walk"){GD_DEMOS[p+"on"]={act:function(){gdDemoWalk(id)},ringAfter:1100,ring:".gfocus|.gdchip"};return}
  if(d.perPoint)GD_DEMOS[p+"cardLine"]={wall:true,act:function(){gdDemoLines(id)},ringAfter:600,ring:".gdbub|.gdchip"};
  if(d.pokes!=null)GD_DEMOS[p+"pokes"]={wall:true,act:function(){gdDemoPokes(id)},ringAfter:600,ring:".gdchip|.gdbub"};
  if(d.idleFor!=null)GD_DEMOS[p+"idleFor"]={wall:true,act:function(){gdDemoIdle(id)},ringAfter:600,ring:".gdchip|.gdbub"};
  ["on","importance","essential","doneWhen"].forEach(function(f){GD_DEMOS[p+f]={wall:true,act:function(){gdDemoItem(id,f)},ringAfter:600,ring:".gdbub|.gdchip"}});
  ["shows","max","again"].forEach(function(f){GD_DEMOS[p+f]={wall:true,act:function(){gdDemoTwice(id)},ringAfter:600,ring:".gdbub|.gdchip"}});
  GD_DEMOS[p+"along"]={wall:true,act:function(){gdDemoAlong(id)},ringAfter:600,ring:".gdbub|.gdchip"};
  GD_DEMOS[p+"duringTour"]={tour:"g1",act:function(){gdDemoTour(id)},ringAfter:600,ring:".gdchip"}});
/* round 4: a cue shown now on the card (a pull: no memory), its chip for the row; Shows twice for Every card (the next card); the
   walkthrough from a step; How to use the card's group: the walkthrough, its Settings entry, its ✦ menu item */
function gdDemoCue(id,f){var my=gdDemoBegin(),it=gdItemNow(id),d=S.guidance.items[id];if(!it)return;
  if(!d.on){cuePlace(id);gdChip("Off: the "+it.label+" never shows");gdDemoEnd(my,3500);return}
  var c=gdDemoShow(it,4000),chip=f==="doneWhen"?(d.doneWhen?"Done when "+GD_EVENTS[d.doneWhen]+": then never again":"Never done for good: once a visit or a card"):f==="importance"?"Importance "+d.importance+" of 5: a cue and a tip both due, the higher goes first":null;
  if(d.reduce==="skip"&&S.reduce)chip="Reduce Motion: a moving cue is skipped";else if(!gdGroupById("card").on)chip="The card (cues) is Off: shown here as a demo only";if(chip)gdChip(chip);
  gdDemoEnd(my,Math.max(3500,c&&isFinite(c.until)?c.until-c.at+400:4000))}
function gdDemoCueShows(id){var d=S.guidance.items[id];if(d.shows!=="card"){gdDemo([{show:id,ms:3000,chip:"Once a visit: once it has shown, not again this visit",wait:3200}]);return}
  gdDemo([{show:id,ms:2400,chip:"Every card: on this card…",wait:2600},{stop:true,show:id,ms:2400,chip:"×10 · …and again on the next card",wait:2800}])}
function gdDemoWalk(id){var my=gdDemoBegin(),d=S.guidance.items[id];if(!d.on){gdChip("Off: the walkthrough skips "+d.label);gdDemoEnd(my,3000);return}
  if(gdGroupById("howto").on===false){gdChip("How to use the card is Off");gdDemoEnd(my,3000);return}gdWalkCard(id);gdChip("How to use the card · from "+d.label);gdDemoEnd(my,3400)}
function gdDemoHow(f){var my=gdDemoBegin(),g=gdGroupById("howto");
  if(g.on===false){gdChip("How to use the card Off: no walkthrough, no entry in Settings or the ✦ menu");gdDemoEnd(my,3500);return}
  if(f==="on"){gdWalkCard();gdChip("How to use the card: the walkthrough, pulled");gdDemoEnd(my,3400);return}
  if(f==="settings"){S.settingsOpen=true;renderPhone(false);var v=ph.querySelector(".vset");if(v)v.scrollTop=v.scrollHeight;gdChip(g.inSettings?"In Settings: its button at the panel's foot":"In Settings: Hidden");gdDemoEnd(my,3500);return}
  gbMenu(true);gdChip(g.inMenu?"In the ✦ menu: How does this work?":"In the ✦ menu: Hidden");gdDemoEnd(my,3500)}
rowDemos(GD_DEMOS);
