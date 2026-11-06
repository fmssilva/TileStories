/* Guidance, the catalog (_3.8 section 3.2): Overall → Kind → Group → Item, as data, in the study's shape (a round adds fields, never
   renames). Overall: Guidance On, the breathing room, at most a minute and a visit, how fast freshness recovers, the learning rules, the
   visitor's long press, Remember across visits. The kinds: App (a bubble, never spoken) with The start (In order: Opening message, Tap a
   marker) and The wall (Ask about the view); Content (spoken once the visitor has heard the guide this visit, else a bubble) with Tours
   (A tour of {point}). Each kind has its On, its cap a visit and its gap; each group its weight; the kinds' weights along the visit are
   a fixed table (GD_MIX). An item's words are in Languages & Terms (S.tx), owned by the item's foldout (gd:i:<id>); the bubble's own
   words (Fewer like this?) by Overall (gd:overall). The events the items wait for or retire on are named in GD_EVENTS (the same names in
   Unity). Unity: GuidanceCatalog, a ScriptableObject (kinds, groups, items; the words as CardStrings keys), read by GuidanceScheduler. */
S.guidance={on:true,breathing:15,perMinute:2,perVisit:30,freshTau:120,learnIgnore:true,learnDismiss:true,fewer:true,remember:true,
  kinds:[{id:"app",name:"App",spoken:"bubble",on:true,cap:20,gap:0,groups:[
      {id:"start",name:"The start",mode:"order",weight:1,items:["opening","tapmarker"]},
      {id:"wall",name:"The wall",mode:"all",weight:1,items:["askview"]}]},
    {id:"content",name:"Content",spoken:"heard",on:true,cap:12,gap:60,groups:[
      {id:"tours",name:"Tours",mode:"all",weight:1,items:["tourof"]}]}],
  items:{
    /* Opening message: the visit's first words, beside Azu at the ✦; Essential (skips the breathing room and the limits); once a visit */
    opening:{id:"opening",label:"Opening message",on:true,kind:"app",group:"start",
      says:"gd_opening",saysBack:"gd_opening_back",spoken:"kind",
      about:"nothing",mustSee:true,form:"tip",goes:"stays",points:false,focus:"none",tap:"close",
      when:"visit",screen:"wall",stillFor:0,afterPrev:0,notBefore:0,
      importance:5,essential:true,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",onScreen:"auto",duringTour:"never",along:"kind"},
    /* Tap a marker: the mascot goes beside the nearest marker, points and rings it: "Tap here."; until a card is opened, at most 3 a visit */
    tapmarker:{id:"tapmarker",label:"Tap a marker",on:true,kind:"app",group:"start",
      says:"wall_tip",saysBack:"",spoken:"kind",
      about:"marker",mustSee:true,form:"tip",goes:"beside",points:true,focus:"ring",tap:"close",
      when:"pause",screen:"wall",stillFor:1.2,afterPrev:20,notBefore:0,
      importance:4,essential:false,shows:"until",max:3,again:25,doneWhen:"card",teaches:"card",onScreen:"auto",duringTour:"never",along:"kind"},
    /* Ask about the view: about the ✦ (its menu holds Ask about the view; the wall has no pill of its own): Azu at the ✦ points at it, a ring
       round it; a tap opens Ask about the view; up to 2 a visit, again after 90 s, not before 60 s of the visit, done when it is opened */
    askview:{id:"askview",label:"Ask about the view",on:true,kind:"app",group:"wall",
      says:"gd_askview",saysBack:"",spoken:"kind",
      about:"gbtn",mustSee:true,form:"tip",goes:"stays",points:true,focus:"ring",tap:"view",
      when:"pause",screen:"wall",stillFor:0,afterPrev:0,notBefore:60,
      importance:3,essential:false,shows:"upto",max:2,again:90,doneWhen:"view",teaches:"view",onScreen:"auto",duringTour:"never",along:"kind"},
    /* A tour of {point}: an invitation about the point in the middle (one with a tour); Azu beside it, pointing; a tap starts its tour; once a
       visit, late in the visit, done when a tour step has been heard */
    tourof:{id:"tourof",label:"A tour of {point}",on:true,kind:"content",group:"tours",
      says:"gd_tourof",saysBack:"",spoken:"kind",
      about:"middle",mustSee:true,form:"invitation",goes:"beside",points:true,focus:"none",tap:"tour",
      when:"pause",screen:"wall",stillFor:1.2,afterPrev:0,notBefore:0,
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"tour",teaches:"tour",onScreen:"auto",duringTour:"never",along:"late"}}};
/* the events (study 3.2): their names in the Lab and in Unity, and the words the Editor reads them back with */
/* the events as a condition in the read-back ("until the visitor opens a card") */
var GD_UNTIL={card:"the visitor opens a card",view:"the visitor opens Ask about the view",tour:"the visitor hears a step of a tour",ask:"the visitor asks a question"};
var GD_EVENTS={visit:"the visit started",marker:"a marker in view",moving:"the phone moving",still:"the phone still",card:"the visitor opened a card",
  view:"the visitor opened Ask about the view",tour:"a step of the tour was heard",ask:"the visitor asked a question",intro:"the guide's introduction was heard"};
/* the features used (study 3.2's events the Lab has): the milestones of the stage of the visit */
var GD_FEATURES=[["card","card opened"],["tour","a tour step heard"],["ask","a question asked"],["view","Ask about the view opened"]];
/* the values an item's fields take, in the Editor's words */
var GD_SHOWS=[["once","Once ever"],["visit","Once a visit"],["upto","Up to n a visit"],["until","Until done, at most n a visit"],["every","Every time (Again after only)"]];
var GD_DONE=[["","Never (it does not retire)"],["card","When the visitor opens a card"],["view","When the visitor opens Ask about the view"],["tour","When a step of a tour is heard"]];
var GD_TOUR=[["never","Never"],["between","Between steps"]];
var GD_ALONG=[["kind","As the kind"],["steady","Steady"],["early","Early"],["late","Late"]];
/* the stage of the visit, s from 0 to 1: the kinds' weights at s = 0 / 0.3 / 1 (a fixed table: the mix), and the items' curves along the
   visit at the same three points (As the kind: the kind's weight alone; Early: it fades after a third; Late: it grows from a third);
   Steady holds the kind's weight at its middle (s = 0.3) all visit. Straight lines between the points. */
var GD_MIX={app:[70,40,15],content:[25,50,60]};
var GD_CURVE={kind:[1,1,1],early:[1,1,0],late:[0,0,1]};
function gdCurve(p,s){s=Math.max(0,Math.min(1,+s||0));return s<=0.3?p[0]+(p[1]-p[0])*s/0.3:p[1]+(p[2]-p[1])*(s-0.3)/0.7}
/* the items in the Editor's order (kind, group, item), each a copy with its group's and kind's values (the scheduler reads only its
   arguments): the scheduler's input, its tie-break and an In order group's sequence */
function gdItems(){var out=[];S.guidance.kinds.forEach(function(k){k.groups.forEach(function(g){g.items.forEach(function(id){var it=S.guidance.items[id];
  if(it)out.push(Object.assign({},it,{groupMode:g.mode,groupName:g.name,groupWeight:g.weight==null?1:+g.weight,kindName:k.name,kindOn:k.on!==false,kindCap:+k.cap||0,kindGap:+k.gap||0,kindSpoken:k.spoken}))})})});return out}
function gdGroupOf(it){var g=null;S.guidance.kinds.forEach(function(k){k.groups.forEach(function(x){if(x.items.indexOf(it.id)>=0)g=x})});return g}
function gdKindOf(it){return S.guidance.kinds.filter(function(k){return k.id===it.kind})[0]||null}
function gdKindById(id){return S.guidance.kinds.filter(function(k){return k.id===id})[0]||null}
function gdGroupById(id){var g=null;S.guidance.kinds.forEach(function(k){k.groups.forEach(function(x){if(x.id===id)g=x})});return g}
/* the line an item says now: its words in the visitor's language, the returning visitor's line when it has one, the slots filled
   ({point}: the point it is about; {min}: the minutes of that point's tour) */
function gdLine(it,returning,slots){var k=returning&&it.saysBack?it.saysBack:it.says,t=TXT(k);slots=slots||{};
  return t.replace(/\{point\}/g,slots.point||"").replace(/\{min\}/g,slots.min!=null?slots.min:"")}
/* the read-back of an item, in words (study 3.2): "Tap a marker · App › The start · when a marker is in view and the phone is still ·
   at most about every 25 s, up to 3 times a visit, until the visitor opens a card." */
function gdHowOften(it){var n=+it.max,ag=+it.again;
  var s=it.shows==="once"?"once ever":it.shows==="visit"?"once a visit":it.shows==="upto"?"up to "+n+(n===1?" time":" times")+" a visit":it.shows==="until"?"up to "+n+(n===1?" time":" times")+" a visit":"every time";
  if(ag>0&&it.shows!=="once"&&it.shows!=="visit")s="at most about every "+ag+" s, "+s;
  if(it.doneWhen)s+=", until "+GD_UNTIL[it.doneWhen];return s}
var GD_ABOUT={nothing:"",marker:"when a marker is in view",gbtn:"when the ✦ is on the screen",middle:"when a point with a tour is in the middle"};
function gdAlongWords(it){var k=GD_MIX[it.kind]||[0,0,0],kn=(gdKindOf(it)||{}).name||"?";
  if(it.along==="steady")return"steady: "+kn+"'s "+k[1]+" % all visit";if(it.along==="early")return"early: full for the first third, then fading to nothing";
  if(it.along==="late")return"late: nothing for the first third, then growing to full";return"as the kind ("+kn+": "+k[0]+" % → "+k[1]+" % → "+k[2]+" %)"}
function gdReadBack(it){var g=gdGroupOf(it),k=gdKindOf(it),w=[];
  if(it.when==="visit")w.push("once the visit has started");else w.push("at a pause");
  if(GD_ABOUT[it.about])w.push(GD_ABOUT[it.about]);if(+it.stillFor>0)w.push("the phone still "+it.stillFor+" s");
  if(+it.notBefore>0)w.push("not before "+it.notBefore+" s of the visit");
  if(g&&g.mode==="order"&&+it.afterPrev>0){var i=g.items.indexOf(it.id),pv=i>0?S.guidance.items[g.items[i-1]]:null;if(pv)w.push("at least "+it.afterPrev+" s after the "+pv.label)}
  return it.label+" · "+(k?k.name:"?")+" › "+(g?g.name:"?")+(it.essential?" · Essential":"")+" · "+w.join(", ")+" · "+gdHowOften(it)+" · along the visit "+gdAlongWords(it)+
    " · during a tour "+(it.duringTour==="between"?"only between steps":"never")+"."}
