/* Guidance, the catalog (_3.8 section 3.2): Overall → Kind → Group → Item, as data. Round 1 holds the fields it uses, in the study's
   shape (later rounds add fields, never rename): the kind App, its group The start (In order), the items Opening message and Tap a
   marker. An item's words are in Languages & Terms (S.tx), owned by the item's foldout (gd:i:<id>). The events the items wait for or
   retire on are named in GD_EVENTS (the same names in Unity). Unity: GuidanceCatalog, a ScriptableObject (kinds, groups, items; the
   words as CardStrings keys), read by GuidanceScheduler. */
S.guidance={on:true,breathing:15,
  kinds:[{id:"app",name:"App",spoken:"bubble",groups:[{id:"start",name:"The start",mode:"order",items:["opening","tapmarker"]}]}],
  items:{
    /* Opening message: the visit's first words, beside Azu at the ✦; Essential (skips the breathing room); once a visit */
    opening:{id:"opening",label:"Opening message",on:true,kind:"app",group:"start",
      says:"gd_opening",saysBack:"gd_opening_back",spoken:"kind",
      about:"nothing",mustSee:true,form:"tip",goes:"stays",points:false,focus:"none",
      when:"visit",screen:"wall",stillFor:0,afterPrev:0,
      importance:5,essential:true,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",onScreen:"auto"},
    /* Tap a marker: the mascot goes beside the nearest marker, points and rings it: "Tap here."; until a card is opened, at most 3 a visit */
    tapmarker:{id:"tapmarker",label:"Tap a marker",on:true,kind:"app",group:"start",
      says:"wall_tip",saysBack:"",spoken:"kind",
      about:"marker",mustSee:true,form:"tip",goes:"beside",points:true,focus:"ring",
      when:"pause",screen:"wall",stillFor:1.2,afterPrev:20,
      importance:4,essential:false,shows:"until",max:3,again:25,doneWhen:"card",teaches:"card",onScreen:"auto"}}};
/* the events (study 3.2): their names in the Lab and in Unity, and the words the Editor reads them back with */
/* the events as a condition in the read-back ("until the visitor opens a card") */
var GD_UNTIL={card:"the visitor opens a card"};
var GD_EVENTS={visit:"the visit started",marker:"a marker in view",moving:"the phone moving",still:"the phone still",card:"the visitor opened a card"};
/* the values an item's fields take, in the Editor's words */
var GD_SHOWS=[["once","Once ever"],["visit","Once a visit"],["upto","Up to n a visit"],["until","Until done, at most n a visit"],["every","Every time (Again after only)"]];
var GD_DONE=[["","Never (it does not retire)"],["card","When the visitor opens a card"]];
/* the items in the Editor's order (kind, group, item), each a copy with its group's mode: the scheduler's input, its tie-break and an In order group's sequence */
function gdItems(){var out=[];S.guidance.kinds.forEach(function(k){k.groups.forEach(function(g){g.items.forEach(function(id){var it=S.guidance.items[id];if(it)out.push(Object.assign({},it,{groupMode:g.mode}))})})});return out}
function gdGroupOf(it){var g=null;S.guidance.kinds.forEach(function(k){k.groups.forEach(function(x){if(x.items.indexOf(it.id)>=0)g=x})});return g}
function gdKindOf(it){return S.guidance.kinds.filter(function(k){return k.id===it.kind})[0]||null}
/* the line an item says now: its words in the visitor's language, the returning visitor's line when it has one */
function gdLine(it,returning){var k=returning&&it.saysBack?it.saysBack:it.says;return TXT(k)}
/* the read-back of an item, in words (study 3.2): "Tap a marker · App › The start · when a marker is in view and the phone is still ·
   at most about every 25 s, up to 3 times a visit, until the visitor opens a card." */
function gdHowOften(it){var n=+it.max,ag=+it.again;
  var s=it.shows==="once"?"once ever":it.shows==="visit"?"once a visit":it.shows==="upto"?"up to "+n+(n===1?" time":" times")+" a visit":it.shows==="until"?"up to "+n+(n===1?" time":" times")+" a visit":"every time";
  if(ag>0&&it.shows!=="once"&&it.shows!=="visit")s="at most about every "+ag+" s, "+s;
  if(it.doneWhen)s+=", until "+GD_UNTIL[it.doneWhen];return s}
function gdReadBack(it){var g=gdGroupOf(it),k=gdKindOf(it),w=[];
  if(it.when==="visit")w.push("once the visit has started");else w.push("at a pause");
  if(it.about==="marker")w.push("when a marker is in view");if(+it.stillFor>0)w.push("the phone still "+it.stillFor+" s");
  if(g&&g.mode==="order"&&+it.afterPrev>0){var i=g.items.indexOf(it.id),pv=i>0?S.guidance.items[g.items[i-1]]:null;if(pv)w.push("at least "+it.afterPrev+" s after the "+pv.label)}
  return it.label+" · "+(k?k.name:"?")+" › "+(g?g.name:"?")+(it.essential?" · Essential":"")+" · "+w.join(", ")+" · "+gdHowOften(it)+"."}
