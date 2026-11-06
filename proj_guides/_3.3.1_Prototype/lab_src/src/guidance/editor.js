/* Guidance in the Editor: Guide tab > 4 · Guidance (Overall: Guidance On / Off, Breathing room; App › The start: each item a row, On,
   its label, its importance dots, how often, ▸ Show now, that opens its fields: Label, Says per language, Essential, Shows, Again after,
   Done when and the read-back in words) and the guidance's tests in 0 · Preview & Test (Show next now and why; the visitor, New visit,
   Reset the visitor). Each row's demo (rowDemos): an item's row puts the phone on the wall and shows the item now (a demo is pulled: it
   never writes the visitor's memory); Shows and Again after show it twice with a ×10 countdown chip. Global Scene > Onboarding hints
   links here (the wall hint's pill is gone). Unity: the POI Editor's Guide tab, its Guidance section drawn from GuidanceCatalog, each
   row's ▸ a Play-mode preview (PlayMode Live Config). */
COMP["gd:i:opening"]=["Guide","Guidance › The start › Opening message"];COMP["gd:i:tapmarker"]=["Guide","Guidance › The start › Tap a marker"];
FOCUS["gd:overall"]=["card","#wall","closed"];FOCUS["gd:app"]=["card","[data-marker]","closed"];FOCUS["gd:g:start"]=["card","[data-marker]","closed"];
FOCUS["gd:i:opening"]=["card",".gbtn-wrap","closed"];FOCUS["gd:i:tapmarker"]=["card","[data-marker]","closed"];
/* a guidance word's place in the tab (Languages & Terms' links; Global Scene's link) */
on("guidePath.answer",function(o){if(o==="gd:overall"||o==="gd:app")return["gt4",o];if(o.indexOf("gd:g:")===0)return["gt4","gd:app",o];
  var m=o.match(/^gd:i:(.+)$/),it=m&&S.guidance.items[m[1]];if(it)return["gt4","gd:app","gd:g:"+it.group,o]});
/* the self-test: a tip's words show on the wall, at its moment (behaviour); every guidance option is timing (behaviour) */
TX_DOM.wall_tip="shown in the guide's tip beside the nearest marker";TX_DOM.gd_opening="shown in the guide's opening tip on the wall";TX_DOM.gd_opening_back="shown in the opening tip to a returning visitor";
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(guidance\\.|"));
/* ----- the rows ----- */
function gdDots(it){var p="@guidance.items."+it.id+".importance";return'<span class="gddots" role="group" aria-label="Importance">'+[1,2,3,4,5].map(function(n){
  return'<button type="button" data-set="'+p+'" data-v="'+n+'" aria-pressed="'+(+it.importance===n)+'"'+(n<=+it.importance?' class="on"':'')+' title="Importance '+n+' of 5" aria-label="Importance '+n+' of 5"></button>'}).join("")+'</span>'}
function gdItemRow(it){var p="guidance.items."+it.id+".",k=gdKindOf(it),g=gdGroupOf(it),i=g?g.items.indexOf(it.id):0,pv=i>0?S.guidance.items[g.items[i-1]]:null;
  var sum='<span class="gdi"><span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b>'+(it.essential?'<span class="badge new">ESSENTIAL</span>':"")+gdDots(it)+
    '<span class="gdhow">'+esc(gdHowOften(it))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Show it on the phone now">&#9656; Show now</button></span>';
  var body=row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the item's name in these lists, in the read-back and in the scenes. The visitor never sees it.")+
    '<div class="sub2">Says · one line per language (Languages &amp; Terms, owned here)</div>'+txPanel("gd:i:"+it.id)+
    row("Essential",pseg(p+"essential",["true","false"],["Yes","No"]),"Yes: it skips the breathing room between tips (never the gates: one at a time, never over a sentence, never while the phone moves or a panel is open). For the visit's first words.")+
    row("Shows",pseg(p+"shows",GD_SHOWS.map(function(x){return x[0]}),GD_SHOWS.map(function(x){return x[1]})),"How often it may show: once ever; once a visit; up to n a visit; until done (its Done when), at most n a visit; or every time (only Again after and the breathing room limit it).")+
    (it.shows==="upto"||it.shows==="until"?row("At most",pslider(p+"max",1,10,1),"Times a visit.",true):"")+
    (it.shows==="once"||it.shows==="visit"?"":row("Again after",pslider(p+"again",0,120,5),"Seconds after it ends before it may show again (its cooldown; 0: only the breathing room). The read-back says it as the visitor lives it: “at most about every 25 s”. Not for Once ever or Once a visit.",true))+
    row("Done when",pseg(p+"doneWhen",GD_DONE.map(function(x){return x[0]}),GD_DONE.map(function(x){return x[1]})),"The visitor's action that retires it for good (this visit and the next, until Reset the visitor): Apple TipKit's rule, a tip gone once its feature is used.")+
    '<div class="note gdfix">'+esc("About: "+(it.about==="marker"?"the nearest marker (it must be on the screen)":"nothing (the wall)")+" · Mascot goes: "+(it.goes==="beside"?"beside it, and points":"stays beside the ✦")+
      " · Focus: "+(it.focus==="ring"?"Ring":"None")+" · When: "+(it.when==="visit"?"the visit started":"at a pause")+(+it.stillFor>0?", the phone still "+it.stillFor+" s":"")+(pv&&+it.afterPrev>0?", at least "+it.afterPrev+" s after the "+pv.label:"")+
      " · Spoken: "+(k&&k.spoken==="bubble"?"no, a bubble only (App)":"as the kind")+". Fixed for this item.")+'</div>'+
    '<div class="note gdrb">'+esc(gdReadBack(it))+'</div>';
  return fo("gd:i:"+it.id,sum,null,body,4)}
function gdVisitorLine(){var v=S.gdVisitor;return"Visit "+v.visits+(v.visits>1?" (a returning visitor)":" (a first visit)")+" · "+gdItems().map(function(it){var vm=gdVisitOf(v,it.id),em=gdEverOf(v,it.id);
  return it.label+": "+vm.shown+" this visit, "+em.shown+" ever"+(em.done?", done ("+GD_EVENTS[em.by]+")":"")}).join(" · ")}
function gdNowLine(){var c=G.gd.cur;return c?"Now: showing the "+c.item.label+(c.pull?" (a demo)":"")+".":G.gd.last?"Now: "+(G.gd.last.item?G.gd.last.why:"nothing, "+G.gd.last.why):"Now: nothing yet."}
on("edGuide.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"gt0",'<div class="sub2">Guidance</div>'+row("Show next now",'<button class="ubtn sm" type="button" data-gdnext="1">&#9656; Show next now</button>',"Asks the scheduler what it would show next, now, with the phone on the wall and still: the item and why, or why nothing (each item's reason). It is shown as the visitor would get it, and counted in the visitor's memory.")+
    '<div class="note gdwhy" id="gdwhy">'+esc(G.gd.next?(G.gd.next.item?"Next: "+G.gd.next.why:"Next: nothing. "+G.gd.next.why):"Next: press Show next now.")+'</div><div class="note gdwhy" id="gdnow">'+esc(gdNowLine())+'</div>'+
    row("The visitor",'<div class="btnrow" style="margin:0"><button class="ubtn sm" type="button" data-gdvisit="new">New visit</button><button class="ubtn sm" type="button" data-gdvisit="reset">Reset the visitor</button></div>',"The visitor's memory, kept on the phone: per tip, this visit and ever, and what is done. New visit: the same visitor comes back (the opening says Welcome back; what is done stays done). Reset the visitor: a first visit again, nothing remembered.")+
    '<div class="note" id="gdvisitor">'+esc(gdVisitorLine())+'</div>');
  var ov=row("Guidance",pseg("guidance.on",["true","false"],["On","Off"]),"On: the guide takes the initiative at a pause, one tip at a time, never over a sentence, never while the phone moves. Off: no tips; what the visitor asks for (the ✦ menu, Ask) still answers.")+
    row("Breathing room",pslider("guidance.breathing",5,60,1),"Seconds between any two tips, from the end of one to the start of the next (an Essential item skips it).");
  var gs="";S.guidance.kinds.forEach(function(k){var inner="";k.groups.forEach(function(g){var items=g.items.map(function(id){return S.guidance.items[id]}).filter(Boolean);
      inner+=fo("gd:g:"+g.id,esc(g.name)+' <span class="sub">· '+(g.mode==="order"?"In order: the visit's first words, then the first gesture":"each on its own")+'</span>',null,
        '<div class="note">'+(g.mode==="order"?"In order: an item waits until the one before it has shown this visit.":"")+' A row: On, its name, importance (●), how often, ▸ Show now; open it for its fields.</div>'+items.map(gdItemRow).join(""),3)});
    gs+=fo("gd:"+k.id,esc(k.name)+' <span class="sub">· how to use the app: a bubble, never spoken</span>',null,inner,2)});
  d.insertAdjacentHTML("beforeend",fo("gt4",sectionTitle("4","Guidance","when the guide speaks up: a tip at a pause, one at a time"),["new","NEW"],
    fo("gd:overall",'Overall <span class="sub">· on, and the breathing room between any two</span>',null,ov,2)+gs,1));
  return d.innerHTML});
/* the read-outs follow the visit (a tip shown, a card opened, a new visit) */
on("gdLog.after",function(){var v=ed.querySelector("#gdvisitor");if(v)v.textContent=gdVisitorLine();var n=ed.querySelector("#gdnow");if(n)n.textContent=gdNowLine()});
setInterval(function(){var n=ed.querySelector("#gdnow");if(n)n.textContent=gdNowLine()},1000);
/* ----- the Editor's actions: Show next now, the visitor, ▸ Show now; a changed label redraws the rows ----- */
function gdOnWallNow(){if(G.on)stopTour();if(typeof hideStopUi==="function")hideStopUi();S.settingsOpen=false;S.contentsOpen=false;G.fab.menu=false;G.cam.open=false;G.fab.bub=null;S.view="card";S.stop="closed";renderPhone(false)}
function gdShowNext(){gdOnWallNow();gdStop("Show next now");var ctx=gdContext();ctx.onWall=true;ctx.where="";ctx.stillSince=-Infinity;ctx.movedAt=null;ctx.panel=null;ctx.remark=null;ctx.showing=null;ctx.speaking=false;ctx.demo=false;
  var r=gdPick(gdItems(),S.gdVisitor,ctx,gdNow());G.gd.next=r;if(r.item)gdStart(r.item,r.why);var w=ed.querySelector("#gdwhy");if(w)w.textContent=r.item?"Next: "+r.why:"Next: nothing. "+r.why;return r}
ed.addEventListener("click",function(e){var t=e.target;
  var nx=t.closest("[data-gdnext]");if(nx){e.stopPropagation();gdShowNext();return}
  var vs=t.closest("[data-gdvisit]");if(vs){e.stopPropagation();gdOnWallNow();if(vs.dataset.gdvisit==="new")gdNewVisit();else gdResetVisitor();return}
  var sh=t.closest("[data-gdshow]");if(sh){e.stopPropagation();e.preventDefault();showRowDemo("guidance.items."+sh.dataset.gdshow+".on");return}},true);
ed.addEventListener("change",function(e){var t=e.target;if(t.dataset.path&&t.dataset.path.indexOf("guidance.")===0)renderEditor()},true);
/* ----- each row's demo ----- */
function gdItemNow(id){return gdItems().filter(function(x){return x.id===id})[0]}
/* a demo begins: the guidance waits (gate "an Editor demo is showing") while it runs; a newer demo of any row ends it */
function gdDemoBegin(){G.gd.demo=true;G.gd.demoOwner=G.demoSeq;clearTimeout(G.gd.demoT);clearInterval(G.gd.demoI);gdStop("an Editor demo");gdChip(null);var r=ph.querySelector(".azfabbub");if(r)r.remove();return G.demoSeq}
function gdDemoLive(my){if(G.demoSeq===my)return true;if(G.gd.demoOwner===my){G.gd.demo=false;gdChip(null);clearInterval(G.gd.demoI)}return false}
function gdDemoEnd(my,ms){G.gd.demoT=setTimeout(function(){if(!gdDemoLive(my))return;gdChip(null);gdStop("the demo ended");G.gd.demo=false},ms)}
function gdDemoShow(it,ms){if(it.about==="marker"&&!gdMarker()&&typeof setWall==="function"){setWall("ribeira");S.stop="closed";renderPhone(false)}return gdStart(it,"the Editor's demo",{pull:true,ms:ms})}
var GD_FIELD_CHIP={essential:function(it){return it.essential?"Essential: it skips the breathing room":"Not Essential: it waits for the breathing room ("+S.guidance.breathing+" s)"},
  doneWhen:function(it){return it.doneWhen?"Done when "+GD_EVENTS[it.doneWhen]+": then never again":"Never done: only Shows and Again after limit it"},
  importance:function(it){return"Importance "+it.importance+" of 5: when two are due, the higher shows first"}};
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
var GD_DEMOS={"guidance.on":{wall:true,act:function(){gdDemoOverall("on")},ringAfter:600,ring:".gdbub|.gdchip"},
  "guidance.breathing":{wall:true,act:function(){gdDemoOverall("breathing")},ringAfter:600,ring:".gdbub|.gdchip"}};
Object.keys(S.guidance.items).forEach(function(id){var p="guidance.items."+id+".";
  ["on","importance","essential","doneWhen"].forEach(function(f){GD_DEMOS[p+f]={wall:true,act:function(){gdDemoItem(id,f)},ringAfter:600,ring:".gdbub|.gdchip"}});
  ["shows","max","again"].forEach(function(f){GD_DEMOS[p+f]={wall:true,act:function(){gdDemoTwice(id)},ringAfter:600,ring:".gdbub|.gdchip"}})});
rowDemos(GD_DEMOS);
