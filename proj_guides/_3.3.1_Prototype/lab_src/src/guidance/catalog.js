/* Guidance, the catalog (_3.8 section 3.2): Overall → Kind → Group → Item, as data, in the study's shape (a round adds fields, never
   renames). Overall: Guidance On, the breathing room, at most a minute and a visit, how fast freshness recovers, the learning rules, the
   visitor's long press, Remember across visits. The kinds: App (a bubble, never spoken) with The start (In order: Opening message, Tap a
   marker) and The wall (Ask about the view, The ✦); Content (spoken once the visitor has heard the guide this visit, else a bubble) with
   What's in view (one item a wall point: Azu's remarks, the 400 ms timer they had is gone) and Tours (A tour of {point}). Each kind has
   its On, its cap a visit and its gap; each group its On, its weight, its own cap a visit and its own gap (0: none of its own; a group's
   gap stands in for its kind's for its items); the kinds' weights along the visit are data (round 5: S.guidance.mix, Overall › Mix along the visit, read by gdScore through
   each item's kindMix). Round 5 also adds Talkativeness (one knob over the room, the limits and the kinds' caps: GD_TALK) and the presets
   (GD_PRESETS: each writes the Overall and kind values, never the items; Custom lists its changes). A per-point item
   (perPoint) is one item for each point of the wall in view (gdItems: "Ribeira Palace in the middle"), each with its own memory, edited
   once. An item's words are in Languages & Terms (S.tx), owned by the item's foldout (gd:i:<id>); the bubble's own words (Fewer like
   this?) by Overall (gd:overall). The events the items wait for or retire on are named in GD_EVENTS (the same names in Unity). Unity:
   GuidanceCatalog, a ScriptableObject (kinds, groups, items; a per-point item expanded from the wall's POI list at load; the words as
   CardStrings keys), read by GuidanceScheduler. */
S.guidance={on:true,breathing:15,cueRoom:5,perMinute:2,perVisit:30,freshTau:120,learnIgnore:true,learnDismiss:true,fewer:true,remember:true,
  /* round 5: Talkativeness (Quiet / Balanced / Chatty), the preset last chosen (First visit: these values), the Mix along the visit (each kind's
     weight at s = 0 / 0.3 / 1; Fun's row is ready for round 7: Fun has no items yet) */
  talk:"balanced",preset:"first",mix:{app:[70,40,15],content:[25,50,60],fun:[5,10,25]},
  kinds:[{id:"app",name:"App",spoken:"bubble",on:true,cap:20,gap:0,groups:[
      {id:"start",name:"The start",mode:"order",on:true,weight:1,cap:0,gap:0,items:["opening","tapmarker"]},
      {id:"wall",name:"The wall",mode:"all",on:true,weight:1,cap:0,gap:0,items:["askview","gbhint"]},
      /* the card's Hints (round 4): one Cue a hint, motion on its element, no mascot; their moments and looks as before (card/hints.js) */
      {id:"card",name:"The card (cues)",mode:"all",on:true,weight:1,cap:0,gap:0,items:["nudge","label","navbar","ba","strip","term"]},
      /* How to use the card: a Walkthrough, In order, pulled only (Settings, the ✦ menu's How does this work?): never scheduled, never capped */
      {id:"howto",name:"How to use the card",mode:"order",pulled:true,on:true,inSettings:true,inMenu:true,items:["wk_grab","wk_settings","wk_bar","wk_locate","wk_block"]}]},
    {id:"content",name:"Content",spoken:"heard",on:true,cap:12,gap:60,groups:[
      {id:"inview",name:"What's in view",mode:"all",on:true,weight:1,cap:12,gap:7,items:["inview"]},
      {id:"tours",name:"Tours",mode:"all",on:true,weight:1,cap:0,gap:0,items:["tourof"]}]}],
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
    /* The ✦ (it was the ✦ hint and its 1 s timer): "Need anything? Tap ✦." wherever the ✦ shows (the wall, or the card past the hero); Azu
       beside it points at it; after 4 pokes on Azu or 25 s idle (no touch, the phone still), Lively only, never on a Solemn point; once a
       visit, done when the ✦ menu is opened; a tap opens the menu. Tap on Azu = Points to the ✦ pulls it at once (gdPull) */
    gbhint:{id:"gbhint",label:"The ✦",on:true,kind:"app",group:"wall",
      says:"gb_hint",saysBack:"",spoken:"kind",
      about:"gbtn",mustSee:true,form:"tip",goes:"stays",points:true,focus:"none",tap:"menu",
      when:"pokes",screen:"any",stillFor:0,afterPrev:0,notBefore:0,pokes:4,idleFor:25,lively:true,notSolemn:true,
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"menu",teaches:"menu",onScreen:"auto",duringTour:"never",along:"kind"},
    /* {point} in the middle (Azu's remarks): one item a point of the wall; when its point settles in the middle 1.2 s (never while
       panning); once a visit for each point; its line: the 3 remarks in turn, or a point with a card its own line; a tap: a point with a
       card starts its tour, any other opens Ask about the view about it */
    inview:{id:"inview",label:"{point} in the middle",on:true,kind:"content",group:"inview",perPoint:true,
      says:["fab_comment_1","fab_comment_2","fab_comment_3"],saysCard:"fab_comment_card",cardLine:true,saysBack:"",spoken:"kind",
      about:"point",mustSee:true,form:"remark",goes:"beside",points:true,focus:"none",tap:"remark",
      when:"settle",screen:"wall",stillFor:1.2,afterPrev:0,notBefore:0,
      importance:2,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",onScreen:"auto",duringTour:"never",along:"kind"},
    /* A tour of {point}: an invitation about the point in the middle (one with a tour); Azu beside it, pointing; a tap starts its tour; once a
       visit, late in the visit, done when a tour step has been heard */
    tourof:{id:"tourof",label:"A tour of {point}",on:true,kind:"content",group:"tours",
      says:"gd_tourof",saysBack:"",spoken:"kind",
      about:"middle",mustSee:true,form:"invitation",goes:"beside",points:true,focus:"none",tap:"tour",
      when:"pause",screen:"wall",stillFor:1.2,afterPrev:0,notBefore:0,
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"tour",teaches:"tour",onScreen:"auto",duringTour:"never",along:"late"},
    /* ----- App › The card (cues): today's six card Hints, form Cue (motion on the element, no mascot, a bubble only where the hint had
       words: the Navigation Bar's tip, the Locate label). about: the element (card/hints.js finds it: cueTargets); when: open = the card
       opening (the nudge at Half; the label), else at a pause with the element in view; shows: once a visit or every card (once on each);
       reduce: skip (a moving cue) or still; Done when: the matching step of How to use the card seen. Their looks: card/hints.js ----- */
    nudge:{id:"nudge",label:"Grabber nudge",on:true,kind:"app",group:"card",form:"cue",says:"",about:"grab",when:"open",screen:"card",reduce:"skip",
      importance:4,essential:false,shows:"visit",max:1,again:0,doneWhen:"wk_grab",teaches:"",duringTour:"never",along:"kind",
      look:"The card opened at Half rises 8 px and settles (0.45 s in, for 0.7 s): the visitor sees it moves."},
    label:{id:"label",label:"Locate label",on:true,kind:"app",group:"card",form:"cue",says:"locate_label",about:"locate",when:"open",screen:"card",reduce:"still",untilSeen:true,
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"wk_locate",teaches:"",duringTour:"never",along:"kind",
      look:"“Find it on the wall” beside the pin until it has been seen for 4 s (the time counts only while it has room beside the pin), then it folds into the pin; a tap on the pin counts as read."},
    navbar:{id:"navbar",label:"Navigation Bar",on:true,kind:"app",group:"card",form:"cue",says:"hint_navbar",about:"navbar",when:"pause",screen:"card",reduce:"still",
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"wk_bar",teaches:"",duringTour:"never",along:"kind",
      look:"The bar rings once and “Jump to any part” sits over it for 3 s (under Reduce Motion it fades in and out, no ring travel)."},
    ba:{id:"ba",label:"Before / After",on:true,kind:"app",group:"card",form:"cue",says:"",about:"ba",when:"pause",screen:"card",reduce:"skip",
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",duringTour:"never",along:"kind",
      look:"The first before / after in view sweeps 12 % each way and returns (1.4 s); a touch stops it."},
    strip:{id:"strip",label:"Swipe strips",on:true,kind:"app",group:"card",form:"cue",says:"",about:"strip",when:"pause",screen:"card",reduce:"skip",
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",duringTour:"never",along:"kind",
      look:"The first strip with more to the side slides 40 px and back (1.1 s)."},
    term:{id:"term",label:"Underlined words",on:true,kind:"app",group:"card",form:"cue",says:"",about:"term",when:"pause",screen:"card",reduce:"still",
      importance:2,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",duringTour:"never",along:"kind",
      look:"The first underlined word in view glows once (2 s): a tap explains it."},
    /* ----- App › How to use the card: the walkthrough's steps, in order, run through the tour's steps (ref: the part the step scrolls
       to; sel: the control it marks); each step's line heard is its memory ("seen"), and a seen step retires the cue whose Done when
       names it ----- */
    wk_grab:{id:"wk_grab",label:"The grabber",on:true,kind:"app",group:"howto",form:"walk",says:"how_grab",ref:"hero",sel:"#grab"},
    wk_settings:{id:"wk_settings",label:"The settings mark",on:true,kind:"app",group:"howto",form:"walk",says:"how_settings",ref:"hero",sel:".chrome i:last-child"},
    wk_bar:{id:"wk_bar",label:"The Navigation Bar",on:true,kind:"app",group:"howto",form:"walk",says:"how_bar",ref:"sec:stories",sel:"#navbar"},
    wk_locate:{id:"wk_locate",label:"The Locate Button",on:true,kind:"app",group:"howto",form:"walk",says:"how_locate",ref:"sec:stories",sel:".locpin"},
    wk_block:{id:"wk_block",label:"A block and its page",on:true,kind:"app",group:"howto",form:"walk",says:"how_block",ref:"b:b_story",sel:null}}};
/* the events (study 3.2): their names in the Lab and in Unity, and the words the Editor reads them back with */
/* the events as a condition in the read-back ("until the visitor opens a card") */
var GD_UNTIL={card:"the visitor opens a card",view:"the visitor opens Ask about the view",tour:"the visitor hears a step of a tour",ask:"the visitor asks a question",menu:"the visitor opens the ✦ menu"};
var GD_EVENTS={visit:"the visit started",marker:"a marker in view",moving:"the phone moving",still:"the phone still",card:"the visitor opened a card",
  view:"the visitor opened Ask about the view",tour:"a step of the tour was heard",ask:"the visitor asked a question",intro:"the guide's introduction was heard",
  menu:"the visitor opened the ✦ menu",poked:"the visitor poked Azu",idle:"the visitor idle",opened:"the card opened"};
/* How to use the card's steps as events: a step seen (its line heard) retires the cue whose Done when names it */
["wk_grab","wk_settings","wk_bar","wk_locate","wk_block"].forEach(function(id){var n=S.guidance.items[id].label;GD_EVENTS[id]="the visitor saw How to use the card's step “"+n+"”";GD_UNTIL[id]="the visitor has seen it in How to use the card"});
/* the features used (study 3.2's events the Lab has): the milestones of the stage of the visit */
var GD_FEATURES=[["card","card opened"],["tour","a tour step heard"],["ask","a question asked"],["view","Ask about the view opened"]];
/* the values an item's fields take, in the Editor's words */
var GD_SHOWS=[["once","Once ever"],["visit","Once a visit"],["upto","Up to n a visit"],["until","Until done, at most n a visit"],["every","Every time (Again after only)"]];
var GD_DONE=[["","Never (it does not retire)"],["card","When the visitor opens a card"],["view","When the visitor opens Ask about the view"],["tour","When a step of a tour is heard"],["menu","When the visitor opens the ✦ menu"]];
/* a cue's own values: how often (Once a visit / Every card: once on each card opened) and Done when (a step of How to use the card seen) */
var GD_CUE_SHOWS=[["visit","Once a visit"],["card","Every card"]];
var GD_DONE_CUE=[["","Never"],["wk_grab","Its step: The grabber"],["wk_bar","Its step: The Navigation Bar"],["wk_locate","Its step: The Locate Button"]];
/* where a cue's element must be, in words (the read-back and Why not now?) */
var GD_CUE_WHEN={grab:"when the card opens at Half (the opening's own answer to the tap)",locate:"when the card opens, the Locate Button on the screen (else at the next pause)",
  navbar:"at a pause, when the Navigation Bar shows",ba:"at a pause, when a Before / After is in view",strip:"at a pause, when a strip with more to the side is in view",term:"at a pause, when an underlined word is in view"};
var GD_CUE_NO={grab:"the grabber nudges only as the card opens at Half",locate:"no Locate Button on the screen",navbar:"the Navigation Bar is not showing",
  ba:"no Before / After in view",strip:"no strip with more to the side in view",term:"no underlined word in view"};
var GD_TOUR=[["never","Never"],["between","Between steps"]];
var GD_ALONG=[["kind","As the kind"],["steady","Steady"],["early","Early"],["late","Late"]];
/* the stage of the visit, s from 0 to 1: the kinds' weights at s = 0 / 0.3 / 1 (the Mix along the visit: data, S.guidance.mix, Overall ›
   Mix along the visit), and the items' curves along the visit at the same three points (As the kind: the kind's weight alone; Early: it
   fades after a third; Late: it grows from a third); Steady holds the kind's weight at its middle (s = 0.3) all visit. Straight lines
   between the points. A kind with no row weighs nothing. */
function gdMix(kind,g){var m=(g||S.guidance).mix,r=m&&m[kind];return r?r.map(Number):[0,0,0]}
var GD_MIX_KINDS=[["app","App"],["content","Content"],["fun","Fun"]],GD_MIX_AT=["on arrival","a third in","at the end"];
/* ----- Talkativeness (round 5, _3.8 section 3.3 point 6): one knob over the numbers. Balanced is the rows' own values; Quiet doubles the
   breathing room and halves the limits; Chatty shortens the room to 0.6 and gives the limits half again. The limits: At most a minute, At
   most a visit and each kind's At most a visit. The value in force is the row's × the factor (the room to the second; a limit to a whole
   tip, at least 1; a cap of 0 stays none). Judged with the simulator (Preview & Test › Simulate a visit): with the limits ×0.5 a first
   visit kept its 12 tips in 15 min (its items' own rules bind first: a remark once a visit for each point), only slower; at ×0.4 the
   caps bite (Content 12 → 5: 10 tips, at most 1 a minute). Chatty cannot add what there is not to say: the same tips, sooner (3 a minute).
   Unity: GuidanceCatalog.talkativeness, applied by GuidanceScheduler's state builder ----- */
var GD_TALK={quiet:{name:"Quiet",room:2,limits:0.4},balanced:{name:"Balanced",room:1,limits:1},chatty:{name:"Chatty",room:0.6,limits:1.5}},GD_TALK_IDS=["quiet","balanced","chatty"];
function gdTalk(g){return GD_TALK[(g||S.guidance).talk]||GD_TALK.balanced}
function gdRoomNow(){return Math.round(+S.guidance.breathing*gdTalk().room)}
function gdLimitNow(v){v=+v||0;return v>0?Math.max(1,Math.round(v*gdTalk().limits)):0}
/* the value in force in words, under its row: "In force: 15 s × 2 = 30 s (Quiet)" */
function gdForceWords(v,unit,room){var t=gdTalk(),f=room?t.room:t.limits,n=room?Math.round(+v*f):gdLimitNow(v);return"In force: "+(f===1?v+unit:v+unit+" × "+f+" = "+n+unit)+" ("+t.name+")"}
/* ----- the presets (round 5, _3.8 section 7): each writes the Overall and kind values (Talkativeness, the breathing room, the limits, each
   kind's cap and gap, the Mix, Remember across visits), never the items. One table, read back by the Editor's help. First visit is the
   catalog's own values above (today's); the others, and why:
   - Returning: the app is known (its tips are mostly done), so App weighs less and Content more, a little more room, App capped at 8;
   - Family: a group that moves on fast and likes play: a shorter room (12 s), Content's gap 45 s, more Fun (ready for round 7);
   - Expert: Quiet (the room ×2, the limits ×0.4), App at 40 / 20 / 10 (an expert learns fast), Content at 55 / 70 / 70, its gap 45 s;
   - Quiet and Chatty: First visit with Talkativeness Quiet or Chatty.
   Remember across visits is On in every preset (Off is for a kiosk or a demo: set by hand, which makes it Custom). The audiences choose
   one (Detail Card › 6 · Audiences › Looks per audience › Guidance preset). Unity: GuidancePreset assets (ScriptableObjects) ----- */
var GD_PRESET_IDS=[["first","First visit"],["returning","Returning"],["family","Family"],["expert","Expert"],["quiet","Quiet"],["chatty","Chatty"]];
/* the values a preset writes, read from a settings object (S.guidance, or a copy) */
function gdPresetOf(g){g=g||S.guidance;var k={},m={};g.kinds.forEach(function(x){k[x.id]={cap:+x.cap,gap:+x.gap}});Object.keys(g.mix||{}).forEach(function(x){m[x]=g.mix[x].map(Number)});
  return{talk:g.talk||"balanced",breathing:+g.breathing,perMinute:+g.perMinute,perVisit:+g.perVisit,remember:g.remember!==false,mix:m,kinds:k}}
function gdPresetLike(p,o){var q=JSON.parse(JSON.stringify(p));Object.keys(o).forEach(function(x){q[x]=o[x]});return q}
var GD_PRESETS=(function(){var first=gdPresetOf();return{first:first,
  returning:{talk:"balanced",breathing:20,perMinute:2,perVisit:24,remember:true,mix:{app:[35,20,10],content:[55,65,65],fun:[10,15,25]},kinds:{app:{cap:8,gap:0},content:{cap:14,gap:45}}},
  family:{talk:"balanced",breathing:12,perMinute:2,perVisit:30,remember:true,mix:{app:[60,35,15],content:[25,40,45],fun:[15,25,40]},kinds:{app:{cap:20,gap:0},content:{cap:12,gap:45}}},
  expert:{talk:"quiet",breathing:15,perMinute:2,perVisit:24,remember:true,mix:{app:[40,20,10],content:[55,70,70],fun:[5,10,20]},kinds:{app:{cap:8,gap:0},content:{cap:16,gap:45}}},
  quiet:gdPresetLike(first,{talk:"quiet"}),chatty:gdPresetLike(first,{talk:"chatty"})}})();
/* each preset's why, in words (the Editor's help reads it back) */
var GD_PRESET_WHY={first:"today's values: App first, then Content as the visit grows; a tip at most every 15 s, 2 a minute.",
  returning:"the app is known (its tips are mostly done): App weighs less and Content more, 20 s between two tips, App capped at 8.",
  family:"a group that moves on fast and likes play: 12 s between two tips, Content's gap 45 s, more Fun (ready for round 7).",
  expert:"Quiet (the room ×2, the limits ×0.4), App at 40 / 20 / 10 (an expert learns fast) and Content at 55 / 70 / 70.",
  quiet:"First visit's values with Talkativeness Quiet: twice the room, the limits ×0.4.",chatty:"First visit's values with Talkativeness Chatty: the room ×0.6, the limits ×1.5.",custom:"values changed by hand."};
function gdPresetName(id){var p=GD_PRESET_IDS.filter(function(x){return x[0]===id})[0];return p?p[1]:id==="custom"?"Custom":id}
/* what differs between two sets of a preset's values, in words ("Breathing room 20 s, was 15 s"): b the values now, a the preset's */
function gdPresetWords(a,b){var w=[],kn={app:"App",content:"Content"};
  if(a.talk!==b.talk)w.push("Talkativeness "+(GD_TALK[b.talk]||{}).name+", was "+(GD_TALK[a.talk]||{}).name);
  if(a.breathing!==b.breathing)w.push("Breathing room "+b.breathing+" s, was "+a.breathing+" s");
  if(a.perMinute!==b.perMinute)w.push("At most a minute "+b.perMinute+", was "+a.perMinute);
  if(a.perVisit!==b.perVisit)w.push("At most a visit "+b.perVisit+", was "+a.perVisit);
  Object.keys(b.kinds).forEach(function(k){var x=a.kinds[k]||{},y=b.kinds[k];if(x.cap!==y.cap)w.push((kn[k]||k)+": At most a visit "+y.cap+", was "+x.cap);if(x.gap!==y.gap)w.push((kn[k]||k)+": Gap "+y.gap+" s, was "+x.gap+" s")});
  GD_MIX_KINDS.forEach(function(k){var x=a.mix[k[0]]||[0,0,0],y=b.mix[k[0]]||[0,0,0];[0,1,2].forEach(function(i){if(x[i]!==y[i])w.push("Mix: "+k[1]+" "+GD_MIX_AT[i]+" "+y[i]+" %, was "+x[i]+" %")})});
  if(a.remember!==b.remember)w.push("Remember across visits "+(b.remember?"On":"Off")+", was "+(a.remember?"On":"Off"));return w}
/* the preset in force: the one last chosen while every value is still its own (or another whose values they all are: First visit with
   Talkativeness Quiet is Quiet), else Custom; and Custom's changes in words, from the one last chosen */
function gdPresetDiff(id){var p=GD_PRESETS[id||S.guidance.preset];return p?gdPresetWords(p,gdPresetOf()):["no preset chosen"]}
function gdPresetNow(){var id=S.guidance.preset;if(GD_PRESETS[id]&&!gdPresetDiff(id).length)return id;var m=GD_PRESET_IDS.filter(function(x){return!gdPresetDiff(x[0]).length})[0];return m?m[0]:"custom"}
/* a preset chosen (Overall › Preset, or an audience in Who's exploring): it writes its values, never the items */
function gdPresetWrite(id){var p=GD_PRESETS[id],g=S.guidance;if(!p)return false;g.talk=p.talk;g.breathing=p.breathing;g.perMinute=p.perMinute;g.perVisit=p.perVisit;g.remember=p.remember;
  g.mix=JSON.parse(JSON.stringify(p.mix));g.kinds.forEach(function(k){var v=p.kinds[k.id];if(v){k.cap=v.cap;k.gap=v.gap}});g.preset=id;return true}
var GD_CURVE={kind:[1,1,1],early:[1,1,0],late:[0,0,1]};
function gdCurve(p,s){s=Math.max(0,Math.min(1,+s||0));return s<=0.3?p[0]+(p[1]-p[0])*s/0.3:p[1]+(p[2]-p[1])*(s-0.3)/0.7}
/* the items in the Editor's order (kind, group, item), each a copy with its group's and kind's values (the scheduler reads only its
   arguments): the scheduler's input, its tie-break and an In order group's sequence */
function gdItems(defs){var out=[];S.guidance.kinds.forEach(function(k){k.groups.forEach(function(g){if(g.pulled&&!defs)return;g.items.forEach(function(id){var it=S.guidance.items[id];if(!it)return;
  /* a pulled group (How to use the card) is never the scheduler's: only the Editor's list (defs) holds its steps */
  var o=Object.assign({},it,{groupMode:g.mode,groupName:g.name,groupOn:g.on!==false,groupPulled:!!g.pulled,groupWeight:g.weight==null?1:+g.weight,groupCap:+g.cap||0,groupGap:+g.gap||0,kindName:k.name,kindOn:k.on!==false,kindCap:gdLimitNow(k.cap),kindGap:+k.gap||0,kindSpoken:k.spoken,kindMix:gdMix(k.id)});
  /* a per-point item: one for each point of the wall in view, its id and label naming the point, each with its own memory */
  if(it.perPoint&&!defs)(S.wallPois||[]).forEach(function(p){var n=L(p.name);out.push(Object.assign({},o,{id:it.id+":"+p.id,base:it.id,point:p.id,pointName:n,card:!!p.card,label:it.label.replace(/\{point\}/g,n)}))});
  else out.push(o)})})});return out}
/* the items as the Editor lists them: one a definition (a per-point item once) */
function gdDefs(){return gdItems(true)}
/* a definition's instances (a per-point item's, one a point; any other: itself) */
function gdInstances(id){return gdItems().filter(function(x){return x.id===id||x.base===id})}
function gdGroupOf(it){var g=null,id=it.base||it.id;S.guidance.kinds.forEach(function(k){k.groups.forEach(function(x){if(x.items.indexOf(id)>=0)g=x})});return g}
/* a group's index path in the state (the Editor's rows: guidance.kinds.<k>.groups.<g>.) */
function gdGroupPath(gid){var p=null;S.guidance.kinds.forEach(function(k,ki){k.groups.forEach(function(g,gi){if(g.id===gid)p="guidance.kinds."+ki+".groups."+gi+"."})});return p}
function gdKindOf(it){return S.guidance.kinds.filter(function(k){return k.id===it.kind})[0]||null}
function gdKindById(id){return S.guidance.kinds.filter(function(k){return k.id===id})[0]||null}
function gdGroupById(id){var g=null;S.guidance.kinds.forEach(function(k){k.groups.forEach(function(x){if(x.id===id)g=x})});return g}
/* the line an item says now: its words in the visitor's language, the returning visitor's line when it has one; with several lines,
   the next in turn (slots.n: how many of its group have shown this visit, so never the same twice in a row) or, for a point with a card,
   its own (saysCard, when Lines per point says so); the slots filled ({point}: the point it is about; {min}: the minutes of that
   point's tour; {button}: the ✦, or the word when the guide's button shows Azu's face or the word) */
function gdLine(it,returning,slots){slots=slots||{};var k=returning&&it.saysBack?it.saysBack:it.says;
  if(Array.isArray(k))k=slots.card&&it.saysCard&&it.cardLine!==false?it.saysCard:k[(+slots.n||0)%k.length];
  return TXT(k).replace(/\{point\}/g,slots.point||"").replace(/\{min\}/g,slots.min!=null?slots.min:"").replace(/\{button\}/g,S.guideCfg.gbIcon==="star"?"✦":TXT("gb_label"))}
/* the read-back of an item, in words (study 3.2): "Tap a marker · App › The start · when a marker is in view and the phone is still ·
   at most about every 25 s, up to 3 times a visit, until the visitor opens a card." */
function gdHowOften(it){var n=+it.max,ag=+it.again;if(it.form==="walk")return"pulled only: never on its own";
  var s=it.shows==="once"?"once ever":it.shows==="visit"?"once a visit":it.shows==="card"?"once on each card":it.shows==="upto"?"up to "+n+(n===1?" time":" times")+" a visit":it.shows==="until"?"up to "+n+(n===1?" time":" times")+" a visit":"every time";
  if(ag>0&&it.shows!=="once"&&it.shows!=="visit")s="at most about every "+ag+" s, "+s;
  if(it.perPoint)s+=" for each point";if(it.doneWhen)s+=", until "+GD_UNTIL[it.doneWhen];return s}
var GD_ABOUT={nothing:"",marker:"when a marker is in view",gbtn:"when the ✦ is on the screen",middle:"when a point with a tour is in the middle",point:"when its point settles in the middle"};
/* its own When, in words: after n pokes on Azu, or n s idle; Lively only, never on a Solemn point */
function gdWhenWords(it){var w=[],o=[];if(+it.pokes>0)o.push("after "+it.pokes+" pokes on Azu");if(+it.idleFor>0)o.push("after "+it.idleFor+" s idle (no touch, the phone still)");if(o.length)w.push(o.join(" or "));
  if(it.lively)w.push("Lively only");if(it.notSolemn)w.push("never on a Solemn point");return w}
function gdAlongWords(it){var k=gdMix(it.kind),kn=(gdKindOf(it)||{}).name||"?";
  if(it.along==="steady")return"steady: "+kn+"'s "+k[1]+" % all visit";if(it.along==="early")return"early: full for the first third, then fading to nothing";
  if(it.along==="late")return"late: nothing for the first third, then growing to full";return"as the kind ("+kn+": "+k[0]+" % → "+k[1]+" % → "+k[2]+" %)"}
/* How to use the card on the wall (the ✦ menu's How does this work? there): one bubble in the group's words, beside the nearest marker
   (Azu pointing, its ring), and a chip that offers to open a card; a pull, so it writes no memory */
function gdHowWall(){var g=gdGroupById("howto");return{id:"howto:wall",label:"How does this work? (on the wall)",on:true,kind:"app",group:"howto",groupName:g.name,groupMode:"order",groupOn:g.on!==false,groupPulled:true,
  kindName:"App",kindOn:true,kindSpoken:"bubble",says:"gd_how_wall",cta:"gd_how_open",saysBack:"",spoken:"kind",about:"marker",mustSee:false,form:"invitation",goes:"beside",points:true,focus:"ring",tap:"close",
  screen:"wall",duringTour:"never",along:"kind",importance:3,essential:false,shows:"every",max:1,again:0,doneWhen:"",teaches:""}}
/* a cue's read-back: what it is, its moment, how often, Reduce Motion, the room from a tip, Done when */
function gdCueReadBack(it){var g=gdGroupOf(it),r=+S.guidance.cueRoom||0;
  return it.label+" · App › "+(g?g.name:"?")+" · a cue (motion on its element, no mascot"+(it.says?", its own words over it":", no words")+") · "+GD_CUE_WHEN[it.about]+" · "+gdHowOften(it)+
    " · under Reduce Motion "+(it.reduce==="skip"?"skipped (a moving cue)":"shown, with no travel")+" · one at a time with every tip and cue; "+(r>0?"never within "+r+" s of a tip, either way (the cue room)":"no room from a tip (the cue room is 0)")+" · during a tour never."}
/* a step of How to use the card: its place, pulled only, what its line marks and what seeing it retires */
function gdStepReadBack(it){var g=gdGroupOf(it),on=g?g.items.filter(function(id){return S.guidance.items[id].on}):[],i=on.indexOf(it.id),ret=gdDefs().filter(function(x){return x.form==="cue"&&x.doneWhen===it.id}).map(function(x){return x.label});
  return it.label+" · App › How to use the card · "+(it.on?"step "+(i+1)+" of "+on.length:"Off: skipped")+" · pulled only (Settings, the ✦ menu's How does this work?): never scheduled, never capped · "+(it.sel?"it marks "+it.label.replace(/^The /,"the "):"it opens a block's page")+
    " · seen (its line heard): "+(ret.length?"the "+ret.join(", the ")+" is done for good":"nothing to retire")+"."}
function gdReadBack(it){if(it.form==="cue")return gdCueReadBack(it);if(it.form==="walk")return gdStepReadBack(it);var g=gdGroupOf(it),k=gdKindOf(it),w=[];
  if(it.when==="visit")w.push("once the visit has started");else if(it.when!=="settle")w.push("at a pause");
  if(GD_ABOUT[it.about])w.push(GD_ABOUT[it.about]);if(+it.stillFor>0)w.push("the phone still "+it.stillFor+" s");
  if(+it.notBefore>0)w.push("not before "+it.notBefore+" s of the visit");w=w.concat(gdWhenWords(it));
  if(g&&g.mode==="order"&&+it.afterPrev>0){var i=g.items.indexOf(it.id),pv=i>0?S.guidance.items[g.items[i-1]]:null;if(pv)w.push("at least "+it.afterPrev+" s after the "+pv.label)}
  var gl=g&&(+g.cap>0||+g.gap>0)?" · "+g.name+" together: "+[+g.cap>0?"at most "+g.cap+" a visit":"",+g.gap>0?"at least "+g.gap+" s apart":""].filter(Boolean).join(", "):"";
  return it.label+" · "+(k?k.name:"?")+" › "+(g?g.name:"?")+(it.essential?" · Essential":"")+" · "+w.join(", ")+" · "+gdHowOften(it)+gl+" · along the visit "+gdAlongWords(it)+
    " · during a tour "+(it.duringTour==="between"?"only between steps":"never")+"."}
