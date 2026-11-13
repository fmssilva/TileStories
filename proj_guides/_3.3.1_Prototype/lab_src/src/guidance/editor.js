/* Guidance in the Editor: Guide tab > 4 · Guidance. 1 · Overall: Guidance On / Off, Breathing room, At most a minute, At most a visit,
   Freshness, the learning (Ignored three in a row, After a dismissal, Fewer like this?), Remember across visits, the long
   press's words. 2 · App and 3 · Content: the kind's rows (On, At most a visit, Gap, its weight along the visit read back in words), its
   groups (their On, weight, own cap a visit and own gap) and each item a row (On, its label, its importance dots, how often, ▸ Show now)
   that opens its fields: Label, Says per language, its own rows (What's in view: Lines per point and the points on the wall now; The ✦:
   After pokes on Azu, After idle), Essential, Shows, At most, Again after, Done when, During a tour, Along the visit, the fixed ones in
   words and the read-back. Ask › Ask about the view › Azu comments links to What's in view (world/wall.js). 6 · Preview & Test: Show next now and why; Why not now? (an item's reason, in the scheduler's own words); the visitor
   (first or returning, the visits, the active minutes, the stage, the features used, each item's memory; New visit, Leave for 31 min,
   Reset the visitor); the Log of the last 30 decisions; round 5: Simulate a visit (a timeline 0-20 min, lanes App / Content / Fun /
   Quiet, a scripted visitor, ×1 ×10 ×30 or Instant, a tap or a scrub shows a dot's item on the phone, a skip's reason in the
   scheduler's words, the summary, and what moved since the last change). Overall, round 5: Preset (Custom lists its changes),
   Talkativeness (the values in force under each row), Mix along the visit (a 3 × 3 table and its curve), and the help's tables in
   words. Each row's demo (rowDemos): an item's row puts the phone on the wall and shows
   the item now (a demo is pulled: it never writes the visitor's memory); a limit or a learning row shows its effect on a ×10 clock with
   the countdown chip. Global Scene > Onboarding hints links here (the wall hint's pill is gone). Unity: the POI Editor's Guide tab, its
   Guidance section drawn from GuidanceCatalog, each row's ▸ a Play-mode preview (PlayMode Live Config); Why not now? and the Log read
   GuidanceScheduler.WhyNot and the service's decision log. */
COMP["gd:overall"]=["Guide","The mascot speaks up › Overall"];
COMP["gd:i:opening"]=["Guide","The mascot speaks up › The start › Opening message"];COMP["gd:i:tapmarker"]=["Guide","The mascot speaks up › The start › Tap a marker"];
COMP["gd:i:askview"]=["Guide","The mascot speaks up › The wall › Ask about the view"];COMP["gd:i:tourof"]=["Guide","The mascot speaks up › Tours › A tour of {point}"];
COMP["gd:i:gbhint"]=["Guide","The mascot speaks up › The wall › The ✦"];COMP["gd:i:inview"]=["Guide","The mascot speaks up › What's in view › {point} in the middle"];
FOCUS["gd:overall"]=["card","#wall","closed"];FOCUS["gd:app"]=["card","[data-marker]","closed"];FOCUS["gd:content"]=["card","[data-marker]","closed"];
FOCUS["gd:g:start"]=["card","[data-marker]","closed"];FOCUS["gd:g:wall"]=["card",".gbtn-wrap","closed"];FOCUS["gd:g:tours"]=["card","[data-marker]","closed"];FOCUS["gd:g:inview"]=["card","#wall","closed"];
FOCUS["gd:i:opening"]=["card",".gbtn-wrap","closed"];FOCUS["gd:i:tapmarker"]=["card","[data-marker]","closed"];FOCUS["gd:i:askview"]=["card",".gbtn-wrap","closed"];FOCUS["gd:i:tourof"]=["card","[data-marker]","closed"];
FOCUS["gd:i:gbhint"]=["card",".gbtn-wrap","closed"];FOCUS["gd:i:inview"]=["card","#wall","closed"];
/* round 4: App › The card (cues), each opened on the card at its element; App › How to use the card (its steps' words, the ✦ menu's) */
var GD_CUE_SEL={nudge:"#grab",label:".locpin",navbar:"#navbar",ba:"[data-ba]",strip:".el-car,.hstrip",term:".gterm"};
COMP["gd:g:card"]=["Guide","The mascot speaks up › The card (cues)"];FOCUS["gd:g:card"]=["card","#grab","half"];COMP["gd:g:howto"]=["Guide","The mascot speaks up › How to use the card"];FOCUS["gd:g:howto"]=["card",".chrome","half"];
Object.keys(GD_CUE_SEL).forEach(function(id){COMP["gd:i:"+id]=["Guide","The mascot speaks up › The card › "+S.guidance.items[id].label];FOCUS["gd:i:"+id]=["card",GD_CUE_SEL[id].split(",")[0],id==="nudge"||id==="label"?"half":"full"]});
gdGroupById("howto").items.forEach(function(id){var it=S.guidance.items[id];COMP["gd:i:"+id]=["Guide","The mascot speaks up › How to use the card › "+it.label];FOCUS["gd:i:"+id]=["card",it.sel||".hero","full"]});
/* round 7: 4 · Fun, its groups and items (a Moment opens on the card in a tour; Play on the wall) */
FOCUS["gd:fun"]=["card","#wall","closed"];COMP["gd:g:play"]=["Guide","The mascot speaks up › Fun › Play"];FOCUS["gd:g:play"]=["card","#wall","closed"];
["joke","ptjoke","hunt","season"].forEach(function(id){var it=S.guidance.items[id];COMP["gd:i:"+id]=["Guide","The mascot speaks up › Fun › Play › "+it.label];FOCUS["gd:i:"+id]=["card","#wall","closed"]});
/* a guidance word's place in the tab (Languages & Terms' links; Global Scene's link) */
function gdKindOfGroup(gid){var k=null;S.guidance.kinds.forEach(function(x){x.groups.forEach(function(g){if(g.id===gid)k=x.id})});return k}
on("guidePath.answer",function(o){if(o==="gd:g:ontour")return[o];if(o==="gd:overall"||/^gd:(app|content|fun|seq)$/.test(o))return["gt4",o];if(o==="gd:visitor"||o==="gd:sim")return["gt0",o];
  if(o.indexOf("gd:g:")===0)return["gt4",gdGroupFo(o.slice(5)),o];var f=/^gd:f:([^:]+):/.exec(o),fi=f&&S.guidance.items[f[1]];if(fi)return gdItemPath(fi).concat([o]);
  var m=o.match(/^gd:i:(.+)$/),it=m&&S.guidance.items[m[1]];if(it)return gdItemPath(it).concat(["gd:f:"+it.id+":says"])});
/* round 14: an item's place in the tab: a game in 3 · Games on the tour (the quiz's pair as one), any other in 4 · Azu speaks up */
function gdItemPath(it){if(it.group==="ontour"){var pa=gdPairOf(it),vb=gdVarBaseOf(it);return["gd:g:ontour"].concat(pa?["gd:pair:"+pa[0].id]:[],vb?["gd:var:"+vb.id]:[],["gd:i:"+it.id])}return["gt4",gdGroupFo(it.group),"gd:g:"+it.group,"gd:i:"+it.id]}
/* round 8: the foldout a group sits in: its kind's, or Sequences for an In order sequence (How to use the card, First pages) */
function gdGroupFo(gid){var g=gdGroupById(gid);return g&&g.seq?"gd:seq":"gd:"+(gdKindOfGroup(gid)||"app")}
/* the self-test: a tip's words show on the wall, at its moment (behaviour); every guidance option is timing (behaviour) */
TX_DOM.wall_tip="shown in the guide's tip beside the nearest marker";TX_DOM.gd_opening="shown in the guide's opening tip on the wall";TX_DOM.gd_opening_back="shown in the opening tip to a returning visitor";
TX_DOM.gd_askview="shown in the guide's tip beside the ✦";TX_DOM.gd_tourof="shown in the tour invitation on the wall";
TX_DOM.fab_comment_1="shown in the mascot's remark on the point in the middle, in turn";TX_DOM.fab_comment_2=TX_DOM.fab_comment_1;TX_DOM.fab_comment_3=TX_DOM.fab_comment_1;
TX_DOM.fab_comment_card="shown in the mascot's remark on a point with a card";TX_DOM.gb_hint="shown in the guide's tip beside the ✦ (after pokes on the mascot, or idle)";
TX_DOM.hint_navbar="shown over the Navigation Bar in its cue";TX_DOM.gd_how="shown in the ✦ menu";TX_DOM.gd_how_wall="shown in How does this work? on the wall";TX_DOM.gd_how_open="shown in How does this work? on the wall";
["how_grab","how_settings","how_bar","how_locate","how_block"].forEach(function(k){TX_DOM[k]="said in How to use the card's walkthrough"});
TX_DOM.mascot_joke_1="shown in A joke's bubble (Fun › Play), the least seen of its variants";TX_DOM.mascot_joke_2=TX_DOM.mascot_joke_1;TX_DOM.mascot_joke_3=TX_DOM.mascot_joke_1;
TX_DOM.mascot_water="shown in the water break's bubble, between two tour steps";TX_DOM.mascot_hungry="shown in the cookie's bubble, before the tour's last step";
TX_DOM.gd_hunt="shown in the treasure hunt invitation (Family, after a card)";TX_DOM.gd_season_antonio="shown as the seasonal line in its window of dates";TX_DOM.gd_season_xmas=TX_DOM.gd_season_antonio;TX_DOM.gd_season_1755=TX_DOM.gd_season_antonio;
["ptj_painting_1","ptj_torreao_1","ptj_river_1"].forEach(function(k){TX_DOM[k]="shown in its point's joke when the point settles in the middle"});
TX_DOM.gd_fewer="shown after a long press on a tip";TX_DOM.gd_fewer_yes="shown after a long press on a tip";TX_DOM.gd_fewer_no="shown after a long press on a tip";
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(guidance\\.|"));
/* the visit's time as the Log and the visitor read it: m:ss */
function gdClock(ms){var s=Math.max(0,Math.round(ms/1000));return Math.floor(s/60)+":"+("0"+s%60).slice(-2)}
/* ----- the rows ----- */
function gdDots(it){var p="@guidance.items."+it.id+".importance";return'<span class="gddots" role="group" aria-label="Importance">'+[1,2,3,4,5].map(function(n){
  return'<button type="button" data-set="'+p+'" data-v="'+n+'" aria-pressed="'+(+it.importance===n)+'"'+(n<=+it.importance?' class="on"':'')+' title="Importance '+n+' of 5" aria-label="Importance '+n+' of 5"></button>'}).join("")+'</span>'}
var GD_ABOUT_WORDS={nothing:"nothing (the wall)",marker:"the nearest marker (it must be on the screen)",gbtn:"the ✦ (it must be on the screen: on the wall, the way into Ask about the view; on the card, once the hero's Start is out of sight)",middle:"the point in the middle, one with a tour",
  point:"its point of the wall, when it is the one in the middle (one item a point of the wall, each with its own memory)"};
var GD_TAP_WORDS={prop:"its prop: the mascot drinks or eats it (acted), and the tour goes on",hunt:"Ask about the view opens and its treasure hunt starts (acted)",close:"it closes (acted)",view:"Ask about the view opens (acted)",tour:"the tour of that point starts (acted)",remark:"a point with a card starts its tour; any other opens Ask about the view about it, the point lit (acted)",menu:"the ✦'s menu opens (acted; then it is done)"};
/* a per-point item's points on the wall now, and the line each says (Lines per point) */
function gdPointsWords(it){var ins=gdInstances(it.id),wn=S.camCfg.wall==="belem"?"the Belém panorama":"the Ribeira painting";
  return"One item a point of the wall in the Lab ("+wn+", "+ins.length+" points): "+ins.map(function(x){return x.pointName+(x.card&&it.cardLine!==false?" (a card: its own line)":"")}).join(", ")+
    ". Each once a visit, with its own memory; the others say the 3 remarks in turn, so never the same twice in a row."}
/* ----- round 14: an item's form, every group folded (the developer's decision, 2026-11-11). Each group of a form (Identity, Says, About,
   Show, When, Only if, How much, Things to try; a beat's Place, The visitor, Needs and pairs, The game, The point, Who and when, Its look;
   a page's Show and When; a cue's How often) is its own closed foldout, gd:f:<item>:<group>, under the item's header line (On, its name,
   its when, ▸ Show now: as they were). Its header says what it holds and how many of its fields differ from the item's defaults (as the
   catalog made it, or + Add guidance and Duplicate), and Reset in it puts them back. Opening an item shows its group headers only; a
   group never opens by level alone (editor/guide-tab.js, guideDepths). A form is {sum, top, groups, foot}: gdItemRow draws it. Unity:
   the GuidanceItem inspector's Foldouts, a changed field marked as a prefab override is ----- */
/* each item's defaults: the catalog's, taken once at load; a new or duplicated item's as it was made (gdRegister); a new word's as it was
   made (gdNewWord), else Languages & Terms' own (TXDEF) */
var GD_ITEM0={},GD_TXDEF={};
function gdDefSnap(id){var it=S.guidance.items[id];if(it)GD_ITEM0[id]=JSON.parse(JSON.stringify(it))}
Object.keys(S.guidance.items).forEach(gdDefSnap);
function gdItemDef(id){if(!GD_ITEM0[id])gdDefSnap(id);return GD_ITEM0[id]||{}}
function gdTxDef(k){return GD_TXDEF[k]||TXDEF[k]||null}
function gdGet(o,k){return String(k).split(".").reduce(function(a,x){return a==null?a:a[x]},o)}
function gdSet(o,k,v){var p=String(k).split("."),x=o;for(var i=0;i<p.length-1;i++){if(x[p[i]]==null)x[p[i]]={};x=x[p[i]]}if(v===undefined)delete x[p[p.length-1]];else x[p[p.length-1]]=v}
/* a value as the count compares it: unset, Off, No and 0 are one; a list or a table by its contents */
function gdNz(v){return v==null||v===""||v===false||v===0||v==="false"||v==="0"?"":typeof v==="object"?JSON.stringify(v):String(v)}
/* a group's fields: the item's own keys in its controls (and the lists it edits: g.keys), and the words in it */
function gdGroupFields(it,g){var id=String(it.base||it.id).replace(/[.*+?^${}()|[\]\\]/g,"\\$&"),re=new RegExp('data-(?:set|sel|num|path)="@?guidance\\.items\\.'+id+'\\.([\\w.]+)"',"g"),f={},w={},m;
  while((m=re.exec(g.html)))f[m[1]]=1;(g.keys||[]).forEach(function(k){f[k]=1});var rw=/data-tx="([^"|]+)\|/g;while((m=rw.exec(g.html)))w[m[1]]=1;delete f.on;return{f:Object.keys(f),w:Object.keys(w)}}
function gdChangedIn(it,g){var id=it.base||it.id,F=gdGroupFields(it,g),d=gdItemDef(id),c=S.guidance.items[id]||it,n=0;
  F.f.forEach(function(k){if(gdNz(gdGet(c,k))!==gdNz(gdGet(d,k)))n++});
  F.w.forEach(function(k){var d0=gdTxDef(k);if(d0&&S.tx[k]&&JSON.stringify(S.tx[k])!==JSON.stringify(d0))n++});return n}
/* Reset: the group's changed fields and words back to the item's defaults (a group moves it back, through gdMoveItem) */
function gdResetGroup(id,k){var it=S.guidance.items[id],g=it&&gdFormOf(it).groups.filter(function(x){return x&&x.k===k})[0];if(!g)return 0;var F=gdGroupFields(it,g),d=gdItemDef(id),n=0;
  F.f.forEach(function(f){var v=gdGet(d,f);if(gdNz(v)===gdNz(gdGet(it,f)))return;n++;if(f==="group"){gdMoveItem(id,v);return}gdSet(it,f,v==null?v:JSON.parse(JSON.stringify(v)))});
  F.w.forEach(function(w){var d0=gdTxDef(w);if(d0&&S.tx[w]&&JSON.stringify(S.tx[w])!==JSON.stringify(d0)){S.tx[w]=JSON.parse(JSON.stringify(d0));n++}});return n}
ed.addEventListener("click",function(e){var t=e.target.closest&&e.target.closest("[data-gdreset]");if(!t)return;e.stopPropagation();var q=t.dataset.gdreset.split("|"),it=S.guidance.items[q[0]];if(!it)return;var n=gdResetGroup(q[0],q[1]);renderAll(true);
  gdEdToast(n?"Reset: "+n+(n>1?" fields":" field")+" of the “"+it.label+"” back to its defaults.":"Nothing to reset: the “"+it.label+"” is as it was made.")},true);
/* the groups, each a closed foldout: its header (the group, what it holds, the count), its rows, Reset when a field differs */
function gdGroupsHtml(it,gs){var plain="";gs=gs.filter(function(g){if(!g||!g.html)return false;if(/class="row|data-tx=/.test(g.html))return true;plain+=g.html;return false});
  return gs.map(function(g){var n=gdChangedIn(it,g),id="gd:f:"+it.id+":"+g.k;
  return fo(id,'<span class="gdgt">'+esc(g.t)+'</span><span class="gdgs" title="'+esc(g.s)+'">· '+esc(g.s)+'</span><span class="gdchg'+(n?"":" z")+'" data-changed="'+n+'">'+(n?n+" changed":"")+'</span>',null,
    g.html+(n?'<div class="btnrow gdresetrow"><button class="ubtn sm" type="button" data-gdreset="'+it.id+'|'+g.k+'">Reset '+(n===1?"this one":"these "+n)+' to the item\'s defaults</button></div>':""),9)}).join("")+plain}
function gdItemRow(it){var F=gdFormOf(it);return fo("gd:i:"+it.id,F.sum,null,(F.top||"")+gdGroupsHtml(it,F.groups)+(F.foot||""),4)}
function gdFormOf(it){return it.form==="beat"?gdBeatRow(it):it.form==="try"?gdTryRow(it):it.form==="cue"?gdCueRow(it):it.form==="walk"?gdStepRow(it):it.form==="page"?gdPageRow(it):gdTipForm(it)}
/* what a group holds, in a few words (its header) */
function gdLow(t){t=String(t||"");return t.charAt(0).toLowerCase()+t.slice(1)}
function gdWhenSum(it){var o=[];if(it.when==="pokes"){if(+it.pokes>0)o.push("after "+it.pokes+" pokes");if(+it.idleFor>0)o.push("after "+it.idleFor+" s idle");if(+it.latePause>0)o.push("a late pause");return o.join(" or ")||"after pokes or idle"}
  var w=gdLow(gdNameOf(GD_WHEN,it.when));if(it.when==="event")w+=": "+gdNameOf(GD_WHEN_EVENTS,it.event);if(it.when==="idle"&&+it.idleFor>0)w+=" ("+it.idleFor+" s)";if(+it.pokes>0)w+=" or after "+it.pokes+" pokes";
  if(it.when==="between")w+=+it.talkFor>0?", after "+it.talkFor+" s of narration":"";return w}
function gdSaysSum(it,k){var n=[].concat(it.says||[]).length,a=Object.keys(it.saysAud||{}).length,s=[];
  if(it.seasons)s.push(it.seasons.length+" lines by date");else if(it.ptJokes)s.push("each point's own jokes");else s.push(n===1?"1 line":n+" lines, "+(it.pick==="least"?"the least seen":"in turn"));
  if(a)s.push(a+" for an audience");if(it.saysBack)s.push("a returning visitor's");if(it.perPoint&&it.cardLine!==false)s.push("a card's own line");
  s.push(it.spoken==="bubble"||it.spoken==="kind"&&k&&k.spoken==="bubble"?"a bubble":"spoken once the guide is heard");return s.join(" · ")}
function gdTrySum(it){var i=gdTryAt(it.id);return i>=0?"row "+(i+1)+" of "+gdTryDefs().length+": “"+gdTryWord(it)+"”":it.inTry===false?"not in Things to try":"not a row"}
/* a point's own field the item uses: the demo point's value, and the way to Specific Marker › Guide script at that field */
function gdStepNo(id){var all=S.fullSteps||S.guideSteps,i=all.findIndex(function(s){return s.id===id});return i>=0?i+1:"?"}
function gdPointLine(it){var id=it.base||it.id,ls=langCodes(),go=function(t,w){return' · <button class="ubtn sm gdedited gdpplink" type="button" data-gdscript="'+t+'" title="Opens Specific Marker › Guide script at '+esc(w||"its field")+'">Specific Marker › Guide script &#8250;</button>'},
    line=function(t,v,target,w){return'<div class="note gdpp" data-gdpp="'+id+'">'+esc(t)+': <b>'+esc(v)+'</b>'+go(target,w)+'</div>'},UP=function(l){return l.toUpperCase()};
  if(it.needs==="quiz"){var q0=null,v=ls.map(function(l){var q=gdQSteps(l);if(!q0&&q.length)q0=q[0];return UP(l)+" "+q.length+" reviewed"+(q.length?" (steps "+q.map(gdStepNo).join(", ")+")":"")}).join(" · ");
    return line("This point's questions",v,"q:"+(q0||Object.keys(S.tourQ)[0]||"g1"),"its steps' quiz questions")}
  if(it.needs==="year"){var gs=Object.keys(S.tourGuess||{}).filter(function(s){return S.tourGuess[s]}),s0=gs[0],gg=s0&&S.tourGuess[s0];
    return line("This point's guess",gg?gg.n+" (before step "+gdStepNo(s0)+") · "+ls.map(function(l){return UP(l)+" "+(gdGuessOk(gg,l)?"Reviewed":"not reviewed")}).join(", "):"none yet","guess:"+(s0||"g1"),"its step's guess")}
  if(it.beat==="word"){return line("This point's word",ls.map(function(l){var w=gdWordOf(l);return(ls.length>1?UP(l)+" ":"")+(w?w.w+" ("+(w.rev?"Reviewed":"not reviewed")+")":"none")}).join(" · "),"word","Don't say it!")}
  if(it.beat==="find"){var fd=S.tourFind,fp=fd&&(WALLS.ribeira.pois||[]).filter(function(x){return x.id===fd.point})[0];
    return line("This point's find",fd&&fp?"“"+(L(fd.thing)||L(fp.name))+"” ("+L(fp.name)+"), after step "+gdStepNo(fd.after):"none","find","Find it on the wall")}
  if(it.ptJokes){var P=S.wallPois||[],pid=S.m&&S.m.poi||"painting",me=P.filter(function(x){return x.id===pid})[0]||P[0],ks=me?gdPtJokes(me.id):[];
    return line("This point's joke",me?(ks.length?"“"+TXT(ks[0])+"”"+(ks.length>1?" (+"+(ks.length-1)+")":""):"none for "+L(me.name)):"none","jokes","Jokes")}
  if(it.perPoint&&it.id==="inview"){return line("This point's line",it.cardLine!==false?"“"+TXT(it.saysCard||"fab_comment_card")+"” (a point with a card and a tour: its own line)":"the 3 remarks in turn","steps","its tour")}
  if(it.id==="tourof"){var st=(S.fullSteps||S.guideSteps).length;return line("This point's tour",st+" steps","steps","its steps")}
  return""}
/* a cue's form (App › The card): Identity (Label), Says (its words, where it has any), How often (Shows, the gesture, Done when), Things to
   try; the fixed ones in words and its read-back below */
function gdCueRow(it){var p="guidance.items."+it.id+".",r=+S.guidance.cueRoom||0;
  var sum='<span class="gdi">'+gdDragH(it)+'<span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b><span class="badge">CUE</span>'+gdDots(it)+
    '<span class="gdhow">'+esc(gdHowOften(it))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Show it on the card now">&#9656; Show now</button></span>';
  var words=it.id==="label"?'<div class="note">Says: the Locate Button\'s own label, “'+esc(TXT("locate_label"))+'” (Languages &amp; Terms, owned by Card Controls › Locate Button).</div>':
    it.says?txPanel("gd:i:"+it.id):'<div class="note">No words: motion on the element only.</div>';
  var tr=gdTryRows(it,!(it.says&&it.id!=="label"),true);
  return{sum:sum,groups:[
    {k:"identity",t:"Identity",s:it.label+" · a cue of The card (cues)",html:row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the cue's name in these lists, in Card Controls > Hints, the Log and the scenes.")},
    {k:"says",t:"Says",s:it.id==="label"?"the Locate Button's own label":it.says?"its words over the bar":"no words: motion on the element only",html:words},
    {k:"much",t:"How often",s:gdHowOften(it),html:row("Shows",pseg(p+"shows",GD_CUE_SHOWS.map(function(x){return x[0]}),GD_CUE_SHOWS.map(function(x){return x[1]})),"Once a visit: once it has shown, not again this visit. Every card: once on each card the visitor opens. Off (the row's On): never. A cue is not a tip: the tips' limits (at most a minute, a visit, App's cap) do not count it.")+
      (GD_GESTURE[it.about]?row("Shows the gesture",pseg(p+"gesture",["true","false"],["On","Off"]),"A translucent finger does "+GD_GESTURE_WORDS[GD_GESTURE[it.about]]+" once with the cue, beside its element. (This cue is skipped under Reduce Motion, a movement.)"):"")+
      row("Done when",pseg(p+"doneWhen",GD_DONE_CUE.map(function(x){return x[0]}),GD_DONE_CUE.map(function(x){return x[1]})),"Retired for good once the visitor has seen that step of How to use the card (its line heard): the walkthrough taught it, so the cue is not needed.")},
    tr?{k:"try",t:"Things to try",s:gdTrySum(it),html:tr}:null],
   foot:'<div class="note gdfix">'+esc("Its element: "+GD_CUE_WHEN[it.about]+" · Its look: "+it.look+" · Under Reduce Motion: "+(it.reduce==="skip"?"skipped (it is a movement)":"shown, with no travel")+
      " · One at a time: never two cues at once, never with a tip; the cue room ("+r+" s, Overall) between a tip and a cue, either way; none between two cues · Never in a tour, with a panel open, while the visitor scrolls or touches (0.8 s), or in the card's first 1.5 s"+(it.when==="open"?" (it answers the opening itself, so the touch that opened the card and the first 1.5 s do not hold it)":"")+". Fixed for this cue.")+'</div>'+
    '<div class="note gdrb">'+esc(gdReadBack(it))+'</div>'}}
/* a step of How to use the card: On, its name, ▸ Show now (the walkthrough from this step); opened: Identity (Label), Show (Focus), Says
   (its line), its read-back */
function gdStepRow(it){var p="guidance.items."+it.id+".";
  var sum='<span class="gdi">'+gdDragH(it)+'<span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b><span class="gdhow">'+esc(gdHowOften(it))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Walk the card from this step">&#9656; Show now</button></span>';
  return{sum:sum,groups:[
    {k:"identity",t:"Identity",s:it.label+" · a step of How to use the card",html:row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the step's name in these lists and the read-back.")},
    {k:"says",t:"Says",s:"its line in the walkthrough",html:txPanel("gd:i:"+it.id)},
    {k:"show",t:"Show",s:"focus: "+gdLow(gdNameOf(GD_STEP_FOCUS,it.focus)),html:row("Focus",pseg(p+"focus",GD_STEP_FOCUS.map(function(x){return x[0]}),GD_STEP_FOCUS.map(function(x){return x[1]})),"How the step marks its control: as the tour's own Focus (Guide › The tour › Focus: "+gdNameOf([["spotlight","Spotlight"],["ring","Soft ring"],["none","None"]],S.guideCfg.cue)+"), Spotlight (the rest dims, the control cut out), Ring, or None. A walkthrough step may use the Spotlight.")}],
   foot:'<div class="note gdrb">'+esc(gdReadBack(it))+'</div>'}}
/* Says, one list (round 14): each line once, EN | PT side by side, ▸ and Remove, + Add a variant; under it Per audience, Returning visitor,
   the item's other words (Languages & Terms' own keys and owner: only the drawing changed) and Spoken */
function gdSaysGroup(it,p,k){var lpp=it.perPoint?row("Lines per point",pseg(p+"cardLine",["true","false"],["A point with a card: its own line","The 3 in turn for every point"]),"Which line each point says. The remarks come in turn (never the same twice in a row). A point with a card can say its own line instead (“Tap and I'll show you around”), since a tap there starts its tour.")+'<div class="note gdpts">'+esc(gdPointsWords(it))+'</div>':"",spoken=row("Spoken",pseg(p+"spoken",GD_SPOKEN.map(function(x){return x[0]}),GD_SPOKEN.map(function(x){return x[1]})),"As the kind: "+(k&&k.spoken==="bubble"?k.name+" is a bubble only":(k?k.name:"")+" is spoken once the visitor has heard the guide this visit")+". A bubble only: never spoken. Spoken once the guide is heard: the voice says it, its word lit. Never spoken while a screen reader is on (it is announced).");
  if(it.seasons)return row("Says by date",gdSeasonHtml(it),"One of these: each line has its window of dates, From and To (MM-DD, both included; a window may run over the new year, 12-01 to 01-06). The line whose window holds today shows (Preview & Test › Today is tries each date); none: Seasonal is quiet. A Solemn day's line (1 November, the 1755 earthquake) is a remembrance, never a joke: that day no joke shows. Each line's words are beside it, in each language; ▸ shows it.")+
    gdOtherWords(it,(it.seasons||[]).map(function(x){return x.says}))+spoken;
  if(it.ptJokes)return row("Says",gdPtJokesHtml(it),"Each point's own Jokes, 0 to 3 a point in every language, written in Specific Marker › Guide script › Jokes; in turn when a point has several. A point with no joke says none: never a generic one in its place.")+lpp+spoken;
  var shown=[].concat(it.says||[]);Object.keys(it.saysAud||{}).forEach(function(a){shown.push(it.saysAud[a])});if(it.saysBack)shown.push(it.saysBack);
  return row("Says",gdSaysHtml(it),(it.pick==="least"?"One of these: the variant this visitor has seen least shows (ties: the first). ":"1 to 4 variants that come in turn, never the same twice in a row (a per-point item turns through them across its points). ")+"Each line once, in each language side by side (Languages &amp; Terms has the same words). The slots: {point} the point it is about, {year} that point's year, {button} the ✦ (or its word), {min} a tour's minutes"+(it.tap==="hunt"||it.button==="hunt"?", {thing} the point the treasure hunt asks for":"")+". ▸ shows the item saying it.")+
    lpp+
    row("Per audience",gdAudHtml(it),"Optional: its own line for Family, Explorer or Expert (an empty line falls back to Says). ▸ shows it as that audience.")+
    row("Returning visitor",gdBackHtml(it),"Optional: the line a returning visitor gets (Welcome back…); an empty line falls back to Says.")+
    gdOtherWords(it,shown)+spoken}
/* the item's words not in the list above (a card's own line, a chip's word…): Languages & Terms' table of them */
function gdOtherWords(it,shown){var ks=(TXO["gd:i:"+(it.base||it.id)]||[]).filter(function(k){return shown.indexOf(k)<0});
  return ks.length?'<div class="sub2">Its other words <span class="sub">· Languages &amp; Terms, owned here</span></div>'+txPanel("gd:i:"+(it.base||it.id),ks):""}
/* a tip's form: Identity, Says, About, Show, When, Only if, How much, Things to try; the fixed ones, its warnings and its read-back below */
function gdTipForm(it){var p="guidance.items."+it.id+".",k=gdKindOf(it),g=gdGroupOf(it),i=g?g.items.indexOf(it.id):0,pv=i>0&&g.mode==="order"?S.guidance.items[g.items[i-1]]:null,wn=gdWarnsOf(it.id),sp=gdSpotOk(it),gs=GD_GESTURE[it.about];
  var sum='<span class="gdi">'+gdDragH(it)+'<span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b>'+(it.essential?'<span class="badge new">ESSENTIAL</span>':"")+
    (wn.length?'<span class="badge gdwb" title="'+esc(wn.map(function(w){return w.text}).join(" · "))+'">&#9888; '+wn.length+'</span>':"")+gdDots(it)+
    '<span class="gdhow">'+esc(gdHowOften(it))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Show it on the phone now">&#9656; Show now</button></span>';
  var tr=gdTryRows(it,false,true),only=gdOnlyList(it);
  var G=[
    {k:"identity",t:"Identity",s:it.label+" · in "+(g?(k?k.name+" › ":"")+g.name:"no group"),html:row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the item's name in these lists, in the read-back, the Log and the scenes. The visitor never sees it.")+
      row("Group",psel(p+"group",gdGroupOpts()),"The group it belongs to, and with it its kind (App, Content): its group's On, weight, cap and gap hold for it. In an In order group it waits for the one before it. Moving it puts it at the end of the new group (drag ≡ to reorder there).")},
    {k:"says",t:"Says",s:gdSaysSum(it,k),keys:["says","saysAud","saysBack"],html:gdSaysGroup(it,p,k)},
    {k:"about",t:"About",s:it.perPoint?"its point, in the middle":gdNameOf(GD_ABOUT_OPTS,it.about)+(it.mustSee?" · on the screen first":""),html:(it.perPoint?'<div class="note">About: '+esc(GD_ABOUT_WORDS.point)+'</div>':row("About",psel(p+"about",GD_ABOUT_OPTS),"Its target: Nothing (the bubble at the ✦), the nearest marker, the point in the middle (one with a tour) or the ✦. The mascot goes beside it, points, and its focus is on it; the bubble is never over it."))+
      row("Must be on screen",pseg(p+"mustSee",["true","false"],["Yes","No"]),"Yes: it waits until its target is on the screen, and ends when the target leaves (see Expires). No: it may show without it, at the ✦.")},
    {k:"show",t:"Show",s:[gdNameOf(GD_FORMS,it.form),"The mascot: "+gdLow(gdNameOf(GD_GOES,it.goes)),"focus: "+gdLow(gdNameOf(GD_FOCUS,gdFocusNow(it)))].concat(it.button&&it.button!=="none"?["a chip: "+gdNameOf(GD_BUTTONS,it.button)]:[]).join(" · "),html:
      row("Form",pseg(p+"form",GD_FORMS.map(function(x){return x[0]}),GD_FORMS.map(function(x){return x[1]})),"Tip: a bubble. Invitation: a bubble with its Button chip. Remark: about the point in view, narrower, ending with › (a tap does more). Moment: the mascot walks to the middle with its prop and asks for one tap, between two steps of a tour (it holds the next step until a tap or 8 s). A Cue is the card's own (The card (cues)); a Walkthrough step is How to use the card's; a Page is the first pages' (round 8).")+
      (it.form==="moment"?row("Prop",pseg(p+"prop",GD_PROPS.map(function(x){return x[0]}),GD_PROPS.map(function(x){return x[1]})),"The mascot's prop: a glass of water (The mascot drinks, “Ahh”) or a cookie (The mascot eats it, “Thank you!”). The look is the mascot's own (Rive states in the app).",true):"")+
      row("Mascot goes",pseg(p+"goes",GD_GOES.map(function(x){return x[0]}),GD_GOES.map(function(x){return x[1]})),"Beside it: the mascot hops to it, a short arc (0.45 s). Stays: the mascot stays at the ✦. Walks to it (on the wall): a slower walk along the bottom, then up to its place. Reduce Motion: no hop, no walk, the mascot appears there.")+
      row("Points at it",pseg(p+"points",["true","false"],["Yes","No"]),"The mascot points at its target (its arm; the 3D model's point clip).")+
      row("Focus",gdFocusSeg(it),"None. Glint: the target shines once, a light sweep (Reduce Motion: a still highlight). Ring: one soft pulse, then still. Spotlight: the rest dims, the target cut out; kept for Essential items and walkthrough steps, or it loses its force.")+
      (it.focus==="spotlight"&&sp?row("Dim the rest",pslider(p+"dim",0.2,0.6,0.05),"Spotlight: how dark the rest goes (0.4 = 40 %); the target and its name stay clear in the cut-out.",true):"")+
      (gs?row("Shows the gesture",pseg(p+"gesture",["true","false"],["On","Off"]),"A translucent finger beside the target does "+GD_GESTURE_WORDS[gs]+" once, after the mascot's hop. Reduce Motion: it is shown still, at its end."):'<div class="note">Shows the gesture: none for '+esc(gdNameOf(GD_ABOUT_OPTS,it.about))+' (no target to touch).</div>')+
      row("Button",pseg(p+"button",GD_BUTTONS.map(function(x){return x[0]}),GD_BUTTONS.map(function(x){return x[1]})),"A chip in the bubble that does its action: Show me (the gesture on its target), Start the tour, Open the card, Ask about the view. A tap on the words still does its own action ("+(GD_TAP_WORDS[it.tap]||"it closes")+").")+
      '<div class="note">Mood, clip and sound: off by default; haptic: none (haptics only answer the visitor\'s own touch).</div>'},
    {k:"when",t:"When",s:gdWhenSum(it),html:
      row("When",pseg(p+"when",GD_WHEN.map(function(x){return x[0]}).concat(it.when==="pokes"?["pokes"]:[]),GD_WHEN.map(function(x){return x[1]}).concat(it.when==="pokes"?["After pokes or idle"]:[])),"At a pause (the gates: never while the visitor acts). At the visit's start. When its target settles in the middle (1.2 s still). After idle (no touch, the phone still). On an event (within "+GD_EVENT_MS/1000+" s after it).")+
      (it.when==="event"?row("The event",psel(p+"event",GD_WHEN_EVENTS),"The visitor's action it follows.",true):"")+
      (it.when==="between"?row("After narration (s)",pslider(p+"talkFor",0,300,10),"Seconds of the tour's narration first, since the tour started or the last Moment (0: at any step).",true)+
        row("Before the last step",pseg(p+"beforeEnd",["true","false"],["Yes","No"]),"Yes: only as the tour moves on to its last step (the cookie, before the end).",true):"")+
      (it.when==="pokes"||+it.pokes>0?row("After pokes on the mascot",pslider(p+"pokes",0,10,1),"Pokes on the mascot in this visit that bring it (0: not by pokes). The tip waits for the finger to lift (0.8 s) and for the mascot's own bubble (a joke after a poke) to end.",true):"")+
      (it.when==="pokes"||+it.latePause>0?row("Or a pause late (from)",pslider(p+"latePause",0,1200,60)+'<div class="gdforce">'+esc(+it.latePause>0?"Also, rarely, at a pause from "+gdClock(+it.latePause*1000)+" into the visit (once a visit, and only if it has not shown)":"Not at a pause: only after pokes")+'</div>',"Besides the pokes: at a pause late in the visit, from this moment (0: never), once a visit; then Along the visit weighs it. After pokes it answers the visitor, so Along the visit does not hold it back.",true):"")+
      (it.when==="idle"||it.when==="pokes"||+it.idleFor>0?row("After idle (s)",pslider(p+"idleFor",0,90,5),"Seconds with no touch and the phone still that bring it (0: "+(it.when==="idle"?"20 s":"not by idle")+"), counted from the visit's start at the earliest.",true):"")+
      row("The phone still (s)",pslider(p+"stillFor",0,5,0.1),"Seconds the phone must be still first (never while panning, nor 0.7 s after).")+
      (g&&g.mode==="order"?row("After the previous (s)",pslider(p+"afterPrev",0,120,5),"In order: seconds after the "+(pv?pv.label:"item before it")+" showed.",true):"")},
    {k:"only",t:"Only if",s:only.map(function(x){return x[1]}).join(" · ")||"always",html:'<div class="note">Every chip must hold; × takes one away.</div><div class="gdchips">'+gdOnlyChips(it)+'</div>'+
      row("Screen",pseg(p+"screen",GD_SCREEN.map(function(x){return x[0]}),GD_SCREEN.map(function(x){return x[1]})),"Where it may show: the wall, a card, or either.",true)+
      row("Audience",pseg(p+"aud",GD_AUD.map(function(x){return x[0]}),GD_AUD.map(function(x){return x[1]})),"Only for this audience (Who's exploring).",true)+
      row("Visit",pseg(p+"visitIs",GD_VISIT_IS.map(function(x){return x[0]}),GD_VISIT_IS.map(function(x){return x[1]})),"Only on a first visit, or only on a returning one.",true)+
      row("Has used",psel(p+"hasUsed",GD_USED),"Only once the visitor has done this (a milestone of the visit).",true)+
      row("Has not used",psel(p+"notUsed",GD_USED),"Only until the visitor has done this.",true)+
      (gdKindSolemnOff(it)?'<div class="note gdfixs">Not on Solemn points: always. Fixed for '+esc((gdKindOf(it)||{}).name||"Fun")+' (not an option): Why not now? says “a Solemn point: no fun here”.</div>':row("Not on Solemn points",pseg(p+"notSolemn",["true","false"],["Yes","No"]),"Never on a point marked Solemn (a memorial).",true))+
      row("Lively only",pseg(p+"lively",["true","false"],["Yes","No"]),"Only when the mascot › Liveliness is Lively.",true)+
      row("Earliest (s)",pslider(p+"notBefore",0,1200,30)+'<div class="gdforce">'+esc(+it.notBefore>0?"Not before "+gdClock(+it.notBefore*1000)+" into the visit":"From the visit's start")+'</div>',"The earliest moment of the visit, in seconds (1:00 = a minute in).",true)+
      row("Latest (s)",pslider(p+"notAfter",0,1200,30)+'<div class="gdforce">'+esc(+it.notAfter>0?"Not after "+gdClock(+it.notAfter*1000)+" into the visit":"No latest")+'</div>',"The latest moment of the visit (0: none).",true)},
    {k:"much",t:"How much",s:"importance "+it.importance+(it.essential?" · Essential":"")+" · "+gdLow(gdNameOf(GD_SHOWS,it.shows))+(it.doneWhen?" · done when "+(GD_EVENTS[it.doneWhen]||it.doneWhen):"")+(it.duringTour==="between"?" · between tour steps":""),html:
      row("Importance",gdDots(it),"1 to 5: when two are due, the higher scores more.")+
      row("Essential",pseg(p+"essential",["true","false"],["Yes","No"]),"Yes: it skips the breathing room and the limits (never the gates: one at a time, never over a sentence, never while the phone moves or a panel is open), and goes before any other. For the visit's first words. Spotlight is kept for Essentials.")+
      row("Along the visit",pseg(p+"along",GD_ALONG.map(function(x){return x[0]}),GD_ALONG.map(function(x){return x[1]})),"How its weight follows the stage of the visit (s: 0 on arrival, 1 after 15 active minutes or every milestone done). As the kind; Steady; Early; Late. A weight in the score, never a quota.")+
      row("Shows",pseg(p+"shows",GD_SHOWS.map(function(x){return x[0]}),GD_SHOWS.map(function(x){return x[1]})),"How often it may show: once ever; once a visit; up to n a visit; until done (its Done when), at most n a visit; or every time (only Again after and the breathing room limit it).")+
      (it.shows==="upto"||it.shows==="until"?row("At most",pslider(p+"max",1,10,1),"Times a visit.",true):"")+
      (it.shows==="once"||it.shows==="visit"?"":row("Again after (s)",pslider(p+"again",0,120,5),"Seconds after it ends before it may show again (its cooldown; 0: only the breathing room). The read-back says it as the visitor lives it: “at most about every 25 s”.",true))+
      row("Done when",pseg(p+"doneWhen",GD_DONE.map(function(x){return x[0]}),GD_DONE.map(function(x){return x[1]})),"The visitor's action that retires it for good (this visit and the next, until Reset the visitor): a tip gone once its feature is used. A tip showing when its feature is used counts as acted.")+
      row("Teaches",psel(p+"teaches",GD_TEACH),"The feature it teaches: one tap on it closes both, and it scores ×1.3 while an App tip about that feature is still open.")+
      row("After a dismissal",pseg(p+"afterDismiss",GD_AFTER_DISMISS.map(function(x){return x[0]}),GD_AFTER_DISMISS.map(function(x){return x[1]})),"Swiped away (or closed): Wait ×3 (an App tip waits ×3 its Again after; a Content item rests its group 5 min); Not again this visit; Never again.")+
      row("Expires",pseg(p+"expires",GD_EXPIRES.map(function(x){return x[0]}),GD_EXPIRES.map(function(x){return x[1]})),"When its target leaves the screen it ends (the default); Never: only its time ends it.")+
      row("On screen for",pseg(p+"onScreen",GD_ONSCREEN.map(function(x){return x[0]}),GD_ONSCREEN.map(function(x){return x[1]})),"Auto: by its words, 4 to 9 s. For n s. Until tapped: it stays until the visitor taps or closes it (the visitor's Settings › Tips stay until I tap them does the same for every tip).")+
      (it.onScreen==="secs"?row("Seconds",pslider(p+"onScreenS",2,20,1),"How long it stays.",true):"")+
      row("During a tour",pseg(p+"duringTour",GD_TOUR.map(function(x){return x[0]}),GD_TOUR.map(function(x){return x[1]})),"Never: quiet for the whole tour. Between steps: it may show at a pause between two steps of a tour (never over a step's words, never over the block being told).")},
    tr?{k:"try",t:"Things to try",s:gdTrySum(it),html:tr}:null];
  var foot='<div class="note gdfix">'+esc("A tap: "+(GD_TAP_WORDS[it.tap]||"it closes")+(it.id==="gbhint"?" · The mascot › Tap on the mascot = Points to the ✦ button shows it at once, as asked: a pull, past every rule but one at a time":"")+
      (it.about==="gbtn"?" · Two items about the ✦ never both in the visit's first minute (the first in this list goes first)":"")+". Fixed for this item.")+'</div>'+
    wn.map(function(w){return'<div class="note warn gdwarn" data-gdwarn="'+w.kind+'">&#9888; '+esc(w.text)+'</div>'}).join("")+
    '<div class="note gdrb">'+esc(gdReadBack(it))+'</div>'+
    '<div class="btnrow gdacts"><button class="ubtn sm" type="button" data-gddup="'+it.id+'">Duplicate</button><button class="ubtn sm" type="button" data-gddel="'+it.id+'">Delete</button></div>';
  return{sum:sum,top:gdPointLine(it),groups:G,foot:foot}}
/* a kind's weight along the visit, read back in words (its row of 1 · Overall › Mix along the visit) */
function gdKindWords(k){var w=gdMix(k.id);return"Its weight along the visit: "+w[0]+" % on arrival, "+w[1]+" % a third of the way in, "+w[2]+" % at the end (s = 0 / 0.3 / 1, straight lines between): a weight in each item's score, never a quota; an item's Along the visit shapes it further. "+
  (k.spoken==="bubble"?"Spoken: never, a bubble only.":"Spoken: once the visitor has heard the guide this visit (a tour step, an answer), else a bubble.")}
function gdKindSub(k){return k.id==="app"?"how to use the app: a bubble, never spoken":k.id==="fun"?"play: a joke, each point's joke, the treasure hunt, a seasonal line; never on a Solemn point":"what is here and what next: spoken once the guide has been heard"}
/* ----- 6 · Preview & Test: the read-outs ----- */
/* an item's memory as the Editor reads it: a per-point item's, summed over its points on this wall (the latest line used) */
function gdMemOf(def){var v=S.gdVisitor,r={shown:0,ever:0,last:null,line:"",acted:0,dismissed:0,done:false,by:""};
  gdInstances(def.id).forEach(function(it){var vm=gdVisitOf(v,it.id),em=gdEverOf(v,it.id);r.shown+=vm.shown;r.ever+=em.shown;r.acted+=em.acted||0;r.dismissed+=em.dismissed||0;
    if(em.last!=null&&(r.last==null||em.last>r.last)){r.last=em.last;r.line=em.line||""}if(em.done){r.done=true;r.by=em.by}});return r}
function gdVisitorLine(){var v=S.gdVisitor;return"Visit "+v.visits+(v.visits>1?" (a returning visitor)":" (a first visit)")+" · "+gdDefs().map(function(it){var m=gdMemOf(it);
  return it.label+": "+m.shown+" this visit, "+m.ever+" ever"+(m.done?", done ("+GD_EVENTS[m.by]+")":"")}).join(" · ")}
function gdVisitorMore(){var v=S.gdVisitor,vi=v.visit,now=gdNow(),st=gdStage(v,now),used=GD_FEATURES.filter(function(f){return v.used[f[0]]!=null}).map(function(f){return f[1]+" ("+gdClock(v.used[f[0]]-vi.at)+")"}),
    rest=Object.keys(vi.rest||{}).filter(function(g){return vi.rest[g]>now}).map(function(g){var gg=gdGroupById(g);return(gg?gg.name:g)+(vi.fewer&&vi.fewer[g]?" (Fewer like this: the visit)":" ("+gdClock(vi.rest[g]-now)+")")});
  return(v.visits>1?"A returning visitor":"A first visit")+" · visits "+v.visits+" · active "+gdN(st.active,1)+" min"+(st.returning?" (the stage counts from 5 min)":"")+" · the stage s "+gdN(st.s)+
    " · met the guide: "+(v.met?"yes":"no")+" · heard the guide this visit: "+(vi.heard?"yes":"no")+" · pokes on the mascot: "+(vi.pokes||0)+" · features used: "+(used.join(", ")||"none")+
    " · breathing room ×"+(vi.breathMul||1)+" ("+(vi.ignoredRun||0)+" ignored in a row) · resting: "+(rest.join(", ")||"none")+" · Remember across visits: "+(S.guidance.remember!==false?"On":"Off")+" · "+gdPagesSeenWords()}
function gdMemTable(){var v=S.gdVisitor;return'<table class="tbl gdmem"><thead><tr><th>Item</th><th>Visit</th><th>Ever</th><th>Last shown</th><th>The line used</th><th>Acted</th><th>Dismissed</th><th>Done</th></tr></thead><tbody>'+
  gdDefs().map(function(it){var m=gdMemOf(it);return'<tr><td>'+esc(it.label)+(it.perPoint?" <small>(its points summed)</small>":"")+'</td><td>'+m.shown+'</td><td>'+m.ever+'</td><td>'+(m.last!=null?gdClock(m.last-v.visit.at):"")+'</td><td>'+esc(m.line)+'</td><td>'+m.acted+'</td><td>'+m.dismissed+'</td><td>'+(m.done?esc(GD_EVENTS[m.by]||m.by):"")+'</td></tr>'}).join("")+'</tbody></table>'}
/* the Log: the last 30 decisions (shown, skipped, ended, and what the learning changed), the newest first, each with the visit's time */
function gdLogLines(){return G.gd.log.filter(function(l){return/^(shown|skipped|ended|learned)$/.test(l.what)}).slice(-30).reverse()}
function gdLogHtml(){var l=gdLogLines();return l.length?l.map(function(x){return'<div class="gdl gdl-'+x.what+'"><span title="visit '+x.v+'">'+gdClock(x.vt)+'</span><b>'+x.what+'</b><span>'+(x.what==="skipped"?x.why.split(" · ").map(esc).join("<br>"):esc(x.why))+'</span></div>'}).join(""):'<div class="uses">Nothing yet.</div>'}
function gdNowLine(){var c=G.gd.cur;return c?"Now: showing the "+c.item.label+(c.pull?" (a demo)":"")+".":G.gd.last?"Now: "+(G.gd.last.item?G.gd.last.why:"nothing, "+G.gd.last.why):"Now: nothing yet."}
/* the visitor in one line (Preview & Test): the visit, the stage, heard the guide; the rest is in its Details */
function gdVisitorShort(){var v=S.gdVisitor,st=gdStage(v,gdNow());return"Visit "+v.visits+(v.visits>1?" (a returning visitor)":" (a first visit)")+" · the stage s "+gdN(st.s)+" ("+gdN(st.active,1)+" active min) · heard the guide this visit: "+(v.visit.heard?"yes":"no")}
on("edGuide.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  /* 6 · Preview & Test (round 14): the tools first (Start the app, below; Today is, Show next now, Why not now?, the Log), then the
     visitor (one line, its Details folded), the visitor's settings (stand-ins), Simulate a visit; the Voice test after it (guide-tab.js) */
  var g0=innerOf(d,"gt0");if(g0)g0.insertAdjacentHTML("afterbegin",
    row("Today is",'<input type="date" class="u gdtoday" data-sel="@ui.gdToday" value="'+esc(S.ui.gdToday||"")+'" aria-label="Today is">',"The date the phone takes as today: each seasonal line shows only in its window of dates (The mascot speaks up › Fun › Play › Seasonal), and a Solemn day (1 November) rests every joke. The real date by default; pick one to try each season.")+
    '<div class="note gdwhy" id="gdtoday">'+esc(gdTodayWords())+'</div>'+
    row("Show next now",'<button class="ubtn sm" type="button" data-gdnext="1">&#9656; Show next now</button>',"Asks the scheduler what it would show next, now, with the phone on the wall and still: the item and why, or why nothing (each item's reason). It is shown as the visitor would get it, and counted in the visitor's memory.")+
    '<div class="note gdwhy" id="gdwhy">'+esc(G.gd.next?(G.gd.next.item?"Next: "+G.gd.next.why:"Next: nothing. "+G.gd.next.why):"Next: press Show next now.")+'</div><div class="note gdwhy" id="gdnow">'+esc(gdNowLine())+'</div>'+
    row("Why not now?",'<div class="btnrow" style="margin:0"><select class="u gdwhysel" data-gdwhysel="1" aria-label="The item">'+gdItems().map(function(it){return'<option value="'+it.id+'"'+(G.gd.whyId===it.id?" selected":"")+'>'+esc(it.label)+'</option>'}).join("")+'</select><button class="ubtn sm" type="button" data-gdwhynot="1">Why not now?</button></div>',
      "Pick an item: why it does not show at this moment, in the scheduler's own words (a gate, its own rule, a limit, the learning, its score), or which item goes first. The phone stays as it is.")+
    '<div class="note gdwhy" id="gdwhynot">'+esc(G.gd.whyText||"Pick an item and press Why not now?")+'</div>'+
    row("Log",'<div class="gdlog" id="gdlog" role="log" aria-label="The last 30 decisions">'+gdLogHtml()+'</div>',"The last 30 decisions, the newest first: each with the visit's time, shown (and why: its rules and its score), skipped (why nothing shows: a gate, or each item's reason; written when the reason changes), ended (and how: acted, dismissed, ignored, or the moment), and what the learning changed.")+
    '<div class="sub2 gth">The visitor <span class="sub">· this phone\'s memory of the visitor</span></div>'+
    row("The visitor",'<div class="btnrow" style="margin:0"><button class="ubtn sm" type="button" data-gdvisit="new">New visit</button><button class="ubtn sm" type="button" data-gdvisit="leave">Leave for 31 min</button><button class="ubtn sm" type="button" data-gdvisit="reset">Reset the visitor</button></div>',"The visitor's memory, kept on the phone only. New visit: the same visitor comes back now (the opening says Welcome back; what is done stays done). Leave for 31 min: the visit's clock moves 31 minutes with the app away: a visit ends after 30 min away, so the visitor comes back to a new visit (the stage counts from 5 min). Reset the visitor: a first visit again, nothing remembered. With Remember across visits Off, every new visit is a first visit.")+
    '<div class="note gdvline" id="gdvisitorline">'+esc(gdVisitorShort())+'</div>'+
    fo("gd:visitor",'Details <span class="sub">· the visits, the features used, the learning, each item\'s memory</span>',null,gdWideRow("The visit",'<div class="note" id="gdvisitor2">'+esc(gdVisitorMore())+'</div>',"This visit and this visitor as the phone keeps them: the visits, the active minutes and the stage, met and heard the guide, the pokes, the features used, the learning (the breathing room, the groups resting), Remember across visits, the first pages, Things to try.")+
      gdWideRow("Each item's memory",'<div class="note" id="gdvisitor">'+esc(gdVisitorLine())+'</div><div id="gdmem">'+gdMemTable()+'</div>',"Each item: shown this visit and ever, when it last showed and the line it used, acted, dismissed, and done (and by what). A per-point item sums its points."),3)+
    '<div class="sub2 gth">The visitor\'s settings <span class="sub">· stand-ins for the phone\'s own: Settings › The guide, the screen reader</span></div>'+
    row("Tips",pseg("gdVisitor.prefs.tips",["all","essentials","off"],["All","Essentials only","Off"]),"The visitor's own Settings › The guide › Tips (kept in its memory): All; Essentials only (the opening); Off (the ✦ and Ask still answer: “You can always tap ✦ for help”).")+
    row("Tips stay until tapped",pseg("gdVisitor.prefs.stay",["false","true"],["Off","On"]),"The visitor's Settings › The guide › Tips stay until I tap them (WCAG 2.2.1): a tip's time never runs out; it waits for a tap, a swipe or its close button. Default Off.")+
    row("Screen reader on",seg("ui.gdSR",["false","true"],["Off","On"],String(!!S.ui.gdSR)),"A stand-in for VoiceOver / TalkBack on the phone: no tip is spoken (the bubble is announced politely, aria-live), and each still lands in the dock's conversation under Tips.")+
    gdSimHtml());
  /* 4 · Azu speaks up: Overall, App, Content, Fun (Play), Sequences; 3 · Games on the tour: Fun › On the tour's own rows and items */
  var g=S.guidance,ov=row("Guidance",pseg("guidance.on",["true","false"],["On","Off"]),"On: the guide takes the initiative at a pause, one tip at a time, never over a sentence, never while the phone moves. Off: no tips; what the visitor asks for (the ✦ menu, Ask) still answers.")+gdPresetRow()+
    row("Talkativeness",pseg("guidance.talk",GD_TALK_IDS,GD_TALK_IDS.map(function(x){return GD_TALK[x].name})),"One knob over the numbers: it scales the breathing room, At most a minute, At most a visit and each kind's At most a visit. Balanced: the rows' own values. Quiet: the room ×2, the limits ×0.4. Chatty: the room ×0.6, the limits ×1.5. Each row below shows the value in force. The factors, in a table, are in Help below.")+
    '<div class="note gdtalk">'+esc(gdTalkWords())+'</div>'+gdHelpHtml()+
    row("Breathing room (s)",pslider("guidance.breathing",5,60,1)+gdForceNote(g.breathing," s",true),"Seconds between any two tips, from the end of one to the start of the next (an Essential item skips it). Doubled after three ignored in a row (below).")+
    row("Cue room (s)",pslider("guidance.cueRoom",0,30,1),"Seconds between a tip and a card cue (a hint on the card: the grabber's nudge, the bar's ring…), either way: from the end of a tip to a cue, and from the end of a cue to a tip. Shorter than the breathing room: a cue is a small movement on the element, no bubble, no mascot. Two cues follow each other with no room, one at a time. 0: none (still never at once).")+
    row("At most a minute",pslider("guidance.perMinute",1,6,1)+gdForceNote(g.perMinute,"",false),"Tips that may start in any 60 s (an Essential item skips it). Talkative scripts meet it; most visits never do.")+
    row("At most a visit",pslider("guidance.perVisit",5,60,1)+gdForceNote(g.perVisit,"",false),"Tips in one visit, all kinds together (an Essential item skips it). A kind has its own cap below.")+
    row("Freshness (s)",pslider("guidance.freshTau",30,600,10),"Seconds (τ) to recover. An item just shown scores ×0, then recovers: 1 − e^(−t/τ) (after τ: ×0.63, after 2τ: ×0.86). So a tip that was just seen yields to another when both are due.")+gdMixHtml()+
    row("Ignored three in a row",pseg("guidance.learnIgnore",["true","false"],["Double the breathing room","Off"]),"Three tips in a row whose time ran out (never tapped, never swiped): the breathing room doubles for the visit (six: ×4, no more). A tip acted on brings it back. The opening's time running out is not ignoring.")+
    row("After a dismissal",pseg("guidance.learnDismiss",["true","false"],["Wait longer","Off"]),"A tip swiped away: an App tip waits ×3 its Again after; a Content item rests its whole group 5 min.")+
    row("Fewer like this?",pseg("guidance.fewer",["true","false"],["Offered on a long press","Off"]),"A long press on a tip asks Fewer like this? Fewer: its group rests for the rest of the visit. Keep: the tip stays. Its words are below.")+
    row("Remember across visits",pseg("guidance.remember",["true","false"],["On","Off"]),"On: the visitor's memory stays on the phone from visit to visit (what is done stays done; the opening says Welcome back; the stage counts from 5 min). Off, for kiosks and demos: every visit starts as a first visit. A visit ends after 30 min away.")+
    '<div class="sub2">The long press · its words (Languages &amp; Terms, owned here)</div>'+txPanel("gd:overall");
  var gs="";S.guidance.kinds.forEach(function(k,ki){var kp="guidance.kinds."+ki+".",inner=row("On",pseg(kp+"on",["true","false"],["On","Off"]),"Off: none of the "+k.name+" items show (each keeps its own On)."+(k.groups.some(function(x){return x.tour})?" Off turns the games on the tour off too (3 · Games on the tour).":""))+
      row("At most a visit",pslider(kp+"cap",1,40,1)+gdForceNote(k.cap,"",false),"The "+k.name+" items in one visit, all together (an Essential item skips it). Talkativeness scales it (the value in force below).")+
      row("Gap (s)",pslider(kp+"gap",0,k.id==="fun"?600:180,k.id==="fun"?10:5),"Seconds from the end of one "+k.name+" item to the start of the next (0: only the breathing room). An Essential item skips it.")+
      '<div class="note gdmix">'+esc(gdKindWords(k))+'</div>'+
      (k.solemnOff?'<div class="note gdsolemn"><b>Solemn points: off.</b> '+esc("Fun never shows on a Solemn point (Specific Marker › Solemn point): no game, no joke, no hunt invitation, no seasonal line. Fixed for the kind, not an option; Why not now? says “a Solemn point: no fun here”. Calm (a mascote › Liveliness) allows only its items without Lively only.")+'</div>':"");
    if(k.groups.some(function(g){return g.seq}))inner+='<div class="note gdseqnote">Its sequences (In order: '+esc(k.groups.filter(function(g){return g.seq}).map(function(g){return g.name}).join(", "))+') are in Sequences, below.</div>';
    /* round 14: Fun's games on the tour have their own section */
    if(k.groups.some(function(g){return g.tour}))inner+='<div class="note gdseqnote">Its games on the tour (the quiz, Guess the year, Don\'t say it!, Find it on the wall, the breather) have their own section: <button class="ubtn sm gdedited" type="button" data-goto="gd:g:ontour">3 · Games on the tour &#8250;</button></div>';
    k.groups.forEach(function(g,gi){var items=g.items.map(function(id){return S.guidance.items[id]}).filter(Boolean),gp=kp+"groups."+gi+".",per=items.some(function(x){return x.perPoint});
      /* round 8: a sequence (How to use the card, the first pages) is drawn in Sequences (gdSeqFo) */
      if(g.seq)return;
      if(g.pulled){inner+=gdPullFo(g);return}
      /* round 12, 14: Fun › On the tour is 3 · Games on the tour (gdGamesHtml) */
      if(g.tour)return;
      inner+=fo("gd:g:"+g.id,esc(g.name)+' <span class="sub">· '+(g.mode==="order"?"In order: the visit's first words, then the first gesture":g.id==="inview"?"one item a point of the wall: the mascot's remarks":g.id==="play"?"a joke, each point's joke, the treasure hunt, a seasonal line":g.id==="card"?"the card's hints, one cue each: motion on the element, no mascot":"each on its own")+'</span>',null,
        '<div class="note">'+(g.mode==="order"?"In order: an item waits until the one before it has shown this visit.":g.id==="card"?"Each cue on its own, at the moment its feature is in reach, taking turns with the tips (one at a time, the cue room between them). Card Controls › Hints shows them as a view.":"Each item on its own.")+' A row: On, its name, importance (●), how often, ▸ Show now; open it for its groups.</div>'+
        row("On",pseg(gp+"on",["true","false"],["On","Off"]),"Off: none of "+g.name+"'s items show (each keeps its own On)."+(g.id==="inview"?" Ask the mascot › Ask about the view › The mascot comments is this same switch.":""))+
        row("Weight",pslider(gp+"weight",0,2,0.1),"×: this group's items' scores against the others' (1: as they are; 0: they never win).")+
        row("At most a visit",pslider(gp+"cap",0,40,1),"The "+g.name+" items in one visit, all together (0: no cap of its own; "+(g.id==="card"?"a cue is not a tip, so "+k.name+"'s cap does not count it":k.name+"'s "+k.cap+" still holds")+"). An Essential item skips it.")+
        row("Gap (s)",pslider(gp+"gap",0,k.id==="fun"?600:120,k.id==="fun"?10:1),"Seconds from the end of one "+g.name+" item to the start of the next. 0: "+(g.id==="card"?"none: two cues follow each other with no room":"as the kind ("+k.name+"'s "+k.gap+" s)")+"; more: the group's own, in place of the kind's. The breathing room between any two tips ("+S.guidance.breathing+" s) still holds.")+
        items.map(gdItemRow).join(""),3)});
    gs+=fo("gd:"+k.id,esc(k.name)+' <span class="sub">· '+gdKindSub(k)+'</span>'+(k.solemnOff?' <span class="badge gdsolb">Solemn points: off</span>':""),null,inner,2)});gs+=gdSeqFo();
  var g4=innerOf(d,"gt4");if(g4)g4.innerHTML=fo("gd:overall",'Overall <span class="sub">· on, the breathing room, the limits, the learning and the memory</span>',null,ov,2)+gs;
  var gg=innerOf(d,"gd:g:ontour");if(gg)gg.innerHTML=gdGamesHtml();
  return d.innerHTML});
/* the read-outs follow the visit (a tip shown, a card opened, a new visit) */
function gdReadouts(){var v=ed.querySelector("#gdvisitor");if(v)v.textContent=gdVisitorLine();var vl=ed.querySelector("#gdvisitorline");if(vl)vl.textContent=gdVisitorShort();var n=ed.querySelector("#gdnow");if(n)n.textContent=gdNowLine();
  var v2=ed.querySelector("#gdvisitor2");if(v2)v2.textContent=gdVisitorMore();var m=ed.querySelector("#gdmem");if(m)m.innerHTML=gdMemTable();var lg=ed.querySelector("#gdlog");if(lg)lg.innerHTML=gdLogHtml()}
on("gdLog.after",function(){if(!G.gd.roT)G.gd.roT=setTimeout(function(){G.gd.roT=0;gdReadouts()},60)});
setInterval(function(){var n=ed.querySelector("#gdnow");if(n)n.textContent=gdNowLine();var vl=ed.querySelector("#gdvisitorline");if(vl)vl.textContent=gdVisitorShort();var v2=ed.querySelector("#gdvisitor2");if(v2)v2.textContent=gdVisitorMore()},1000);
/* ----- the Editor's actions: Show next now, Why not now?, the visitor, ▸ Show now; a changed label redraws the rows ----- */
function gdOnWallNow(){if(G.on)stopTour();if(typeof hideStopUi==="function")hideStopUi();S.settingsOpen=false;S.contentsOpen=false;G.fab.menu=false;G.cam.open=false;G.gd.tryOpen=false;S.view="card";S.stop="closed";renderPhone(false)}
function gdShowNext(){gdOnWallNow();gdStop("Show next now");var ctx=gdContext();ctx.onWall=true;ctx.where="";ctx.stillSince=-Infinity;ctx.movedAt=null;ctx.touchAt=null;ctx.cardAt=null;ctx.panel=null;ctx.dockFull=false;ctx.tour=null;ctx.state=null;ctx.remark=null;ctx.showing=null;ctx.speaking=false;ctx.demo=false;
  var r=gdPick(gdItems(),S.gdVisitor,ctx,gdNow());G.gd.next=r;if(r.item)gdStart(r.item,r.why);var w=ed.querySelector("#gdwhy");if(w)w.textContent=r.item?"Next: "+r.why:"Next: nothing. "+r.why;return r}
/* Why not now?: the scheduler's words for one item at this moment (the phone as it is; its bubble measured, so a covered target says so) */
function gdWhyNow(id){var items=gdItems(),ctx=gdContext(),it=items.filter(function(x){return x.id===id})[0];if(it&&it.onTour&&!G.on)ctx.planNow=gdPlanNow();if(it&&!gdGate(ctx,gdNow())){var cov=gdCovers(it);if(cov)ctx.covers[id]=cov}
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
function gdDemoBegin(){G.gd.demo=true;G.gd.demoOwner=G.demoSeq;clearTimeout(G.gd.demoT);clearInterval(G.gd.demoI);gdStop("an Editor demo");gdChip(null);gdWordOff();gdTailEnd();if(G.gd.find)gdFindEnd(G.gd.find);return G.demoSeq}
function gdDemoLive(my){if(G.demoSeq===my)return true;if(G.gd.demoOwner===my){G.gd.demo=false;gdChip(null);clearInterval(G.gd.demoI)}return false}
/* a demo ends its own pulled tip only (never a tip the visitor or a scene got meanwhile) */
function gdDemoEnd(my,ms){G.gd.demoT=setTimeout(function(){if(!gdDemoLive(my))return;gdChip(null);if(G.gd.cur&&G.gd.cur.pull)gdStop("the demo ended");G.gd.demo=false},ms)}
function gdDemoShow(it,ms,o){if(it.form==="cue"){cuePlace(it.id);return gdStart(it,"the Editor's demo",Object.assign({pull:true,ms:ms||4000},o||{}))}
  /* round 7: a Moment in a tour (paused while it shows, on again after); a point's joke on a point that has one, turned to the middle; a
     seasonal line outside its window: its first line (the chip says so) */
  if(it.form==="moment"){if(!G.on){S.introSeen=true;startTour()}if(!G.paused)pauseTour();var cm=gdStart(it,"the Editor's demo",Object.assign({pull:true,ms:ms||8000},o||{}));gdChip("A Moment: "+it.label+", between two tour steps (one tap, or 8 s)");return cm}
  if(it.ptJokes){var c0=gdCentral(),ins=gdInstances(it.base||it.id).filter(function(x){return x.jokes&&x.jokes.length}),pk=ins.filter(function(x){return o&&o.point?x.point===o.point:!!(c0&&x.point===c0.id)})[0]||ins[0];
    if(!pk)return null;if(typeof setWall==="function"&&S.camCfg.wall!=="ribeira")setWall("ribeira");gdSimAim(pk.point);return gdStart(pk,"the Editor's demo",Object.assign({pull:true,ms:ms},o||{}))}
  if(it.seasons&&!(o&&o.text)&&!gdSeasonOf(it,gdTodayMD())){var f0=(it.seasons||[]).filter(function(x){return gdHasWords(x.says)})[0];if(f0)o=Object.assign({},o||{},{text:TXT(f0.says)})}var need=it.about==="marker"&&!gdMarker()||it.about==="middle"&&!(gdCentral()||{}).tour||it.about==="gbtn"&&!gdGbtn()||it.about==="point"&&!gdCentral();
  if(need&&typeof setWall==="function"){setWall("ribeira");S.stop="closed";renderPhone(false)}
  /* a per-point item: the one of the point in the middle now */
  if(it.base){var m=gdItemNow(it.base);if(m)it=m}
  return gdStart(it,"the Editor's demo",Object.assign({pull:true,ms:ms},o||{}))}
var GD_FIELD_CHIP={essential:function(it){return it.essential?"Essential: it skips the breathing room":"Not Essential: it waits for the breathing room ("+gdRoomNow()+" s)"},
  doneWhen:function(it){return it.doneWhen?"Done when "+GD_EVENTS[it.doneWhen]+": then never again":"Never done: only Shows and Again after limit it"},
  importance:function(it){return"Importance "+it.importance+" of 5: when two are due, the higher scores more"}};
function gdDemoItem(id,field){var my=gdDemoBegin(),it=gdItemNow(id);if(!it)return;
  if(!it.on){gdChip("Off: the "+it.label+" never shows");gdDemoEnd(my,3500);return}
  var c=gdDemoShow(it);if(GD_FIELD_CHIP[field])gdChip(GD_FIELD_CHIP[field](it));gdDemoEnd(my,Math.max(3500,c?c.until-c.at+300:0))}
/* Shows and Again after: the item, the ×10 countdown to its next time, the item again, then how often in words */
function gdDemoTwice(id){var my=gdDemoBegin(),it=gdItemNow(id);if(!it)return;
  if(!it.on){gdChip("Off: the "+it.label+" never shows");gdDemoEnd(my,3500);return}
  var once=it.shows==="once"||it.shows==="visit",gap=+it.again>0?+it.again:gdRoomNow(),why=+it.again>0?"Again after "+it.again+" s":"Breathing room "+gdRoomNow()+" s";
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
  var left=gdRoomNow();gdDemoShow(its[0],2600);G.gd.demoT=setTimeout(function(){if(!gdDemoLive(my))return;gdStop("the demo");gdChip("×10 · Breathing room: "+left+" s");
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
  perMinute:function(){var n=gdLimitNow(S.guidance.perMinute),its=gdOnItems(),st=[];for(var k=0;k<=n&&its.length;k++){var it=its[k%its.length];
      if(k===n){st.push({count:Math.max(5,60-n*4),label:"At most "+n+" a minute, the next tip"});st.push({show:it.id,chip:"×10 · then the next tip: a new minute"})}
      else st.push({stop:true,show:it.id,ms:1800,chip:"×10 · tip "+(k+1)+" of at most "+n+" in this minute",wait:2000})}return st},
  perVisit:function(){var its=gdOnItems(),all=gdItems().reduce(function(a,o){return a+gdVisitOf(S.gdVisitor,o.id).shown},0);
    return[{show:its[0]&&its[0].id,chip:"At most "+gdLimitNow(S.guidance.perVisit)+" a visit: "+all+" shown so far this visit",wait:3000},{stop:true,chip:"After "+gdLimitNow(S.guidance.perVisit)+": quiet until the next visit (an Essential item still shows)",wait:2800}]},
  freshTau:function(){var tau=+S.guidance.freshTau,its=gdOnItems(),f=function(t){return gdN(1-Math.exp(-t/tau))},id=its[0]&&its[0].id;
    return[{show:id,chip:"Freshness: just shown, it scores ×0.00",wait:2200},{stop:true,chip:"×10 · after 30 s: ×"+f(30),wait:1000},{chip:"×10 · after τ = "+tau+" s: ×"+f(tau),wait:1000},{chip:"×10 · after 2τ: ×"+f(2*tau),wait:1000},{show:id,chip:"Fresh again: it scores in full",wait:2600}]},
  learnIgnore:function(){var its=gdOnItems().filter(function(x){return!x.essential}),br=gdRoomNow();if(!its.length)return[{chip:"No tip to ignore"}];
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
  if(f==="cap"){var n=its.reduce(function(a,o){return a+gdVisitOf(S.gdVisitor,o.id).shown},0);gdDemo([{show:its[0].id,chip:k.name+": at most "+gdLimitNow(k.cap)+" a visit ("+n+" so far)",wait:3200}]);return}
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
  for(var k=1;k<=n;k++)st.push({fn:function(){azReact("r-giggle",450,"A")},chip:"A poke on the mascot: "+k+" of "+n,wait:550});
  st.push({show:it.id,chip:"After "+n+" pokes on the mascot: the ✦ tip"+(S.guideCfg.life==="lively"?"":" (Lively only: the mascot is Calm now, so not by itself)"),wait:3400});gdDemo(st)}
function gdDemoIdle(id){var it=gdItemNow(id),d=S.guidance.items[id];if(!it)return;
  if(!(+d.idleFor>0)){gdDemo([{chip:"Not by idle (0)"+(+d.pokes>0?": only after "+d.pokes+" pokes on the mascot":""),wait:3000}]);return}
  gdDemo([{count:+d.idleFor,label:"Idle: no touch, the phone still"},{show:it.id,chip:"After "+d.idleFor+" s idle: the ✦ tip",wait:3400}])}
function gdDemoAlong(id){var it=gdItemNow(id);if(!it)return;var kw=it.kindMix||gdMix(it.kind),w=function(s){return it.along==="steady"?kw[1]/100:gdCurve(kw,s)/100*gdCurve(GD_CURVE[it.along]||GD_CURVE.kind,s)},lab=(GD_ALONG.filter(function(a){return a[0]===it.along})[0]||[0,""])[1];
  gdDemo([{chip:"×10 · "+lab+": on arrival (s 0) ×"+gdN(w(0)),wait:1100},{chip:"×10 · "+lab+": a third in (s 0.3) ×"+gdN(w(0.3)),wait:1100},{chip:"×10 · "+lab+": at the end (s 1) ×"+gdN(w(1)),wait:1100},
    {show:it.on?id:null,chip:"Along the visit, "+lab.toLowerCase()+": "+gdAlongWords(it),wait:3200}])}
function gdDemoTour(id){var it=gdItemNow(id);if(!it)return;var my=gdDemoBegin();gdChip(it.duringTour==="between"?"During a tour · Between steps: the "+it.label+" may show at a pause between two steps":"During a tour · Never: the "+it.label+" waits for the tour to end");gdDemoEnd(my,4200)}
var GD_DEMOS={"guidance.on":{wall:true,act:function(){gdDemoOverall("on")},ringAfter:600,ring:".gdbub|.gdchip"},
  "guidance.breathing":{wall:true,act:function(){gdDemoOverall("breathing")},ringAfter:600,ring:".gdbub|.gdchip"}};
Object.keys(GD_DEMO_ROWS).forEach(function(f){GD_DEMOS["guidance."+f]={wall:true,act:function(){gdDemo(GD_DEMO_ROWS[f]())},ringAfter:600,ring:".gdbub|.gdchip"}});
S.guidance.kinds.forEach(function(k,ki){var kp="guidance.kinds."+ki+".";["on","cap","gap"].forEach(function(f){GD_DEMOS[kp+f]={wall:true,wait:0,act:function(){gdDemoKind(ki,f)},ringAfter:60,ring:".gdbub|.gdchip"}});
  k.groups.forEach(function(g,gi){var gp=kp+"groups."+gi+".";if(g.launch){GD_DEMOS[gp+"on"]={wall:true,wait:0,act:function(){gdDemoPagesOn()},ringAfter:400,ring:".gdpages|.gdchip"};return}if(g.pulled&&g.id==="howto"){GD_DEMOS[gp+"on"]={act:function(){gdDemoHow("on")},ringAfter:1100,ring:".gfocus|.gdchip"};
      GD_DEMOS[gp+"inSettings"]={stop:"half",act:function(){gdDemoHow("settings")},ringAfter:500,ring:".vhow|.vset"};GD_DEMOS[gp+"inMenu"]={wall:true,act:function(){gdDemoHow("menu")},ringAfter:500,ring:'[data-gbgo="how"]|.gbmenu'};return}
    GD_DEMOS[kp+"groups."+gi+".weight"]={wall:true,wait:0,act:function(){gdDemoGroup(ki,gi)},ringAfter:60,ring:".gdbub|.gdchip"};
    ["on","cap","gap"].forEach(function(f){GD_DEMOS[kp+"groups."+gi+"."+f]={wall:true,wait:0,act:function(){gdDemoGroupField(ki,gi,f)},ringAfter:60,ring:".gdbub|.gdchip"}})})});
Object.keys(S.guidance.items).forEach(function(id){var p="guidance.items."+id+".",d=S.guidance.items[id];if(d.form==="page")return;
  if(d.form==="try"){GD_DEMOS[p+"on"]={wait:0,act:function(){gdDemoTryShow(id)},ringAfter:1500,ring:".gdfinger|.gdchip"};return}
  /* a cue: on the card at its element (never the wall); a step of How to use the card: the walkthrough from that step */
  if(d.form==="cue"){var rg=".gdchip|"+GD_CUE_SEL[id].split(",").join("|");["on","importance","doneWhen"].forEach(function(f){GD_DEMOS[p+f]={stop:id==="nudge"||id==="label"?"half":"full",act:function(){gdDemoCue(id,f)},ringAfter:700,ring:rg}});
    GD_DEMOS[p+"shows"]={stop:"full",act:function(){gdDemoCueShows(id)},ringAfter:700,ring:rg};return}
  if(d.form==="walk"){GD_DEMOS[p+"on"]={act:function(){gdDemoWalk(id)},ringAfter:1100,ring:".gfocus|.gdchip"};return}
  if(d.perPoint)GD_DEMOS[p+"cardLine"]={wall:true,wait:0,act:function(){gdDemoLines(id)},ringAfter:60,ring:".gdbub|.gdchip"};
  if(d.pokes!=null)GD_DEMOS[p+"pokes"]={wall:true,wait:0,act:function(){gdDemoPokes(id)},ringAfter:60,ring:".gdchip|.gdbub"};
  if(d.idleFor!=null)GD_DEMOS[p+"idleFor"]={wall:true,wait:0,act:function(){gdDemoIdle(id)},ringAfter:60,ring:".gdchip|.gdbub"};
  ["on","importance","essential","doneWhen"].forEach(function(f){GD_DEMOS[p+f]={wall:true,wait:0,act:function(){gdDemoItem(id,f)},ringAfter:60,ring:".gdbub|.gdchip"}});
  ["shows","max","again"].forEach(function(f){GD_DEMOS[p+f]={wall:true,wait:0,act:function(){gdDemoTwice(id)},ringAfter:60,ring:".gdbub|.gdchip"}});
  GD_DEMOS[p+"along"]={wall:true,wait:0,act:function(){gdDemoAlong(id)},ringAfter:60,ring:".gdbub|.gdchip"};
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
/* (round 7) an item's, a group's and a kind's demo show their tip in the same frame: no wait before or after (the Fun rows doubled the
   Guide tab's rows; a second a row was 3 minutes of the tests) */
rowDemos(GD_DEMOS);
/* ----- round 5: Overall's Preset, Talkativeness and the Mix along the visit; 6 · Preview & Test › Simulate a visit ----- */
COMP["gd:mix"]=["Guide","The mascot speaks up › Overall › Mix along the visit"];FOCUS["gd:mix"]=["card","#wall","closed"];COMP["gd:help"]=["Guide","The mascot speaks up › Overall › Help"];FOCUS["gd:help"]=["card","#wall","closed"];
COMP["gd:sim"]=["Guide","Preview & Test › Simulate a visit"];FOCUS["gd:sim"]=["card","#wall","closed"];
/* the Editor's own state of the simulator: the scripted visitor, the speed (Instant: the whole 20 min at once) */
S.ui.gdSim="first";S.ui.gdSpeed="instant";
G.gd.sim={t:0,upto:GD_SIM_MS,playI:0,cache:{},set:null,prev:null,shown:null};
/* the value in force under a row (Talkativeness) */
function gdForceNote(v,unit,room){return'<div class="gdforce">'+esc(gdForceWords(v,unit,room))+'</div>'}
function gdTalkWords(){var t=gdTalk();return S.guidance.talk==="balanced"||!GD_TALK[S.guidance.talk]?"Balanced: the rows' own values are in force.":t.name+": the breathing room ×"+t.room+", the limits ×"+t.limits+" (each row below shows its value in force)."}
/* the Preset row: the preset in force (Custom once a value differs from the one last chosen: listed only then, it is a state, not a
   choice), Custom's changes in words */
function gdPresetRow(){var now=gdPresetNow(),base=S.guidance.preset,diff=gdPresetDiff(base),why=(GD_PRESET_WHY[now]||"");
  return row("Preset",sel("@guidance.preset",GD_PRESET_IDS.concat(now==="custom"?[["custom","Custom"]]:[]),now),"A preset writes the Overall and kind values (Talkativeness, the breathing room, the limits, each kind's cap and gap, the Mix along the visit, Remember across visits), never the items. A change to any of them by hand makes it Custom, which lists its changes. Each audience sets its own (Detail Card › 6 · Audiences › Looks per audience › Guidance preset). Each preset's values are in Help below.")+
    '<div class="note gdpreset'+(now==="custom"?" custom":"")+'">'+esc(now==="custom"?"Custom, changed from "+gdPresetName(base)+": "+diff.join(" · ")+".":gdPresetName(now)+": "+why)+'</div>'}
/* Help: the presets' table and Talkativeness's factors, read from their tables (GD_PRESETS, GD_TALK) */
function gdHelpHtml(){var P=GD_PRESET_IDS,now=gdPresetNow(),g=S.guidance,cell=function(x){return'<td>'+esc(x)+'</td>'},mx=function(m){return(m||[0,0,0]).join(" / ")};
  var rows=[["Talkativeness",function(p){return GD_TALK[p.talk].name}],["Breathing room",function(p){return p.breathing+" s"}],["At most a minute",function(p){return p.perMinute}],["At most a visit",function(p){return p.perVisit}],
    ["App: at most a visit, gap",function(p){return p.kinds.app.cap+", "+p.kinds.app.gap+" s"}],["Content: at most a visit, gap",function(p){return p.kinds.content.cap+", "+p.kinds.content.gap+" s"}],["Fun: at most a visit, gap",function(p){return p.kinds.fun?p.kinds.fun.cap+", "+p.kinds.fun.gap+" s":"–"}],
    ["Mix: App",function(p){return mx(p.mix.app)}],["Mix: Content",function(p){return mx(p.mix.content)}],["Mix: Fun",function(p){return mx(p.mix.fun)}],["Remember across visits",function(p){return p.remember?"On":"Off"}]];
  var pt='<table class="tbl gdhelp"><thead><tr><th></th>'+P.map(function(p){return'<th'+(p[0]===now?' class="on"':'')+'>'+esc(p[1])+'</th>'}).join("")+'</tr></thead><tbody>'+
    rows.map(function(r){return'<tr><td>'+esc(r[0])+'</td>'+P.map(function(p){return cell(r[1](GD_PRESETS[p[0]]))}).join("")+'</tr>'}).join("")+'</tbody></table>'+
    '<div class="note">'+P.map(function(p){return"<b>"+esc(p[1])+"</b>: "+esc(GD_PRESET_WHY[p[0]])}).join("<br>")+'</div>';
  var force=function(id){var t=GD_TALK[id],l=function(v){v=+v||0;return v>0?Math.max(1,Math.round(v*t.limits)):0};return Math.round(+g.breathing*t.room)+" s · "+l(g.perMinute)+" a minute · "+l(g.perVisit)+" a visit · App "+l(g.kinds[0].cap)+", Content "+l(g.kinds[1].cap)+(g.kinds[2]?", Fun "+l(g.kinds[2].cap):"")};
  var tt='<table class="tbl gdhelp"><thead><tr><th></th>'+GD_TALK_IDS.map(function(id){return'<th'+(id===g.talk?' class="on"':'')+'>'+GD_TALK[id].name+'</th>'}).join("")+'</tr></thead><tbody>'+
    '<tr><td>The breathing room</td>'+GD_TALK_IDS.map(function(id){return cell("×"+GD_TALK[id].room)}).join("")+'</tr>'+
    '<tr><td>The limits: At most a minute, At most a visit, each kind\'s At most a visit</td>'+GD_TALK_IDS.map(function(id){return cell("×"+GD_TALK[id].limits)}).join("")+'</tr>'+
    '<tr><td>With the rows as they are now</td>'+GD_TALK_IDS.map(function(id){return cell(force(id))}).join("")+'</tr></tbody></table>'+
    '<div class="note">Read in words: Quiet waits twice as long between two tips and allows fewer than half as many (rounded to a whole tip, at least 1); Chatty waits a little over half as long and allows half again as many. Judged with Simulate a visit: Chatty cannot add what there is not to say (the same tips, sooner); Quiet spaces them and its caps cut the remarks.</div>';
  return fo("gd:help",'Help <span class="sub">· the presets and Talkativeness, in words</span>',null,gdWideRow("The presets: what each writes",pt,"Each preset writes Talkativeness, the room, the limits, the kinds' caps and gaps, the Mix and Remember (never the items). Read from GD_PRESETS.")+gdWideRow("Talkativeness: its factors",tt,"How each level scales the breathing room and the limits, with the rows as they are now. Read from GD_TALK."),3)}
/* a row whose content takes the row's whole width, its label above it (a table: Help, the Mix) */
function gdWideRow(label,html,h){return row(label,html,h).replace('<div class="row','<div class="row gdwide')}
/* Mix along the visit: the 3 × 3 table (the kinds' weights at s = 0 / 0.3 / 1) and its curve (straight lines between the points). Unity:
   an IMGUI table of IntFields and a Painter2D curve in the POI Editor's window */
var GD_KIND_INK={app:"#E2C36B",content:"#8CC4F5",fun:"#EFA3C0"};
function gdMixCurve(){var W=480,H=170,l=46,r=96,t=12,b=26,X=function(s){return l+(W-l-r)*s},Y=function(v){return t+(H-t-b)*(1-v/100)},h='';
  [0,50,100].forEach(function(v){h+='<line x1="'+l+'" x2="'+(W-r)+'" y1="'+Y(v)+'" y2="'+Y(v)+'" class="gdgrid"/><text x="'+(l-6)+'" y="'+(Y(v)+4)+'" text-anchor="end" class="gdax">'+v+' %</text>'});
  [[0,"arrival"],[0.3,"a third in"],[1,"the end"]].forEach(function(a){h+='<line x1="'+X(a[0])+'" x2="'+X(a[0])+'" y1="'+t+'" y2="'+(H-b)+'" class="gdgrid'+(a[0]===0.3?' dash':'')+'"/><text x="'+X(a[0])+'" y="'+(H-b+15)+'" text-anchor="'+(a[0]===1?"end":a[0]===0?"start":"middle")+'" class="gdax">'+a[1]+'</text>'});
  var ends=[];GD_MIX_KINDS.forEach(function(k){var m=gdMix(k[0]),pts=[[0,m[0]],[0.3,m[1]],[1,m[2]]],c=GD_KIND_INK[k[0]];
    h+='<polyline points="'+pts.map(function(p){return X(p[0]).toFixed(1)+","+Y(p[1]).toFixed(1)}).join(" ")+'" fill="none" stroke="'+c+'" stroke-width="2.5" stroke-linejoin="round"'+(k[0]==="fun"?' stroke-dasharray="5 4"':'')+'/>';
    pts.forEach(function(p){h+='<circle cx="'+X(p[0]).toFixed(1)+'" cy="'+Y(p[1]).toFixed(1)+'" r="3.5" fill="'+c+'"/>'});ends.push({y:Y(m[2]),c:c,t:k[1]+" "+m[2]+" %"})});
  /* the end labels kept 12 px apart */
  ends.sort(function(a,b){return a.y-b.y});for(var i=1;i<ends.length;i++)if(ends[i].y-ends[i-1].y<12)ends[i].y=ends[i-1].y+12;
  ends.forEach(function(e){h+='<text x="'+(W-r+8)+'" y="'+(e.y+4).toFixed(1)+'" fill="'+e.c+'" class="gdend">'+esc(e.t)+'</text>'});
  return'<svg class="gdcurve" viewBox="0 0 '+W+' '+H+'" role="img" aria-label="The Mix along the visit: each kind\'s weight from arrival to the end">'+h+'</svg>'}
function gdMixHtml(){var cols=["On arrival (s 0)","A third in (s 0.3)","At the end (s 1)"];
  var tb='<table class="tbl gdmixt"><thead><tr><th>Kind</th>'+cols.map(function(c){return'<th>'+c+'</th>'}).join("")+'</tr></thead><tbody>'+GD_MIX_KINDS.map(function(k){var m=gdMix(k[0]),has=S.guidance.kinds.some(function(x){return x.id===k[0]});
    return'<tr><td><i class="gdsw" style="background:'+GD_KIND_INK[k[0]]+'"></i>'+k[1]+(has?'':'<small>no items yet: round 7 adds them, so it weighs nothing until then</small>')+'</td>'+[0,1,2].map(function(i){var p="@guidance.mix."+k[0]+"."+i;
      return'<td><span class="gdmixc"><input class="u" type="number" data-num="'+p+'" min="0" max="100" step="5" value="'+m[i]+'" aria-label="'+k[1]+' '+cols[i]+'"> %</span></td>'}).join("")+'</tr>'}).join("")+'</tbody></table>';
  return fo("gd:mix",'Mix along the visit <span class="sub">· each kind\'s weight at s = 0 / 0.3 / 1, and its curve</span>',null,
    gdWideRow("The weights",tb+gdMixCurve(),"The kinds' weights along the visit (s: 0 on arrival, 1 after 15 active minutes or every milestone done): a weight in each item's score, never a quota. App tips also retire by themselves once learned. The order across kinds is this table, never a drag.")+
    '<div class="note">Each row\'s weight is read back on its kind (App, Content, Fun). A change re-runs Simulate a visit (6 · Preview & Test) and its demo shows what moved.</div>',3)}
/* ----- the simulator in the Editor ----- */
var GD_SIM_QUIET={tour:"a tour",pan:"panning",card:"a card open",panel:"a panel open"};
/* the settings now, and the ones before the last change (what moved is between them) */
function gdSimSig(){return JSON.stringify([S.guidance,S.guideCfg.life,S.camCfg.wall,S.lang,gdTodayMD(),S.ptJokes,S.vis.aud])}
function gdSimTrack(){var k=G.gd.sim,sig=gdSimSig();if(k.set&&k.set.sig===sig)return;k.prev=k.set;k.set={sig:sig,g:JSON.parse(JSON.stringify(S.guidance)),life:S.guideCfg.life}}
function gdSimRes(sid,set){var k=G.gd.sim,key=sid+"|"+set.sig;if(!k.cache[key]){if(Object.keys(k.cache).length>24)k.cache={};k.cache[key]=gdSimulate(sid,set)}return k.cache[key]}
function gdSimNow(sid){gdSimTrack();return gdSimRes(sid||S.ui.gdSim,G.gd.sim.set)}
function gdSimPrev(sid){gdSimTrack();var p=G.gd.sim.prev;return p?gdSimRes(sid||S.ui.gdSim,p):null}
/* what moved between two runs: each item's n-th showing moved (1 s or more), new, or gone */
function gdSimDiff(a,b){var key=function(d){return d.id+"#"+d.n},A={},B={},o={moved:[],added:[],gone:[],map:{},first:null};var f=function(t){if(o.first==null||t<o.first)o.first=t};
  a.dots.forEach(function(d){A[key(d)]=d});b.dots.forEach(function(d){B[key(d)]=d});
  b.dots.forEach(function(d){var k=key(d),p=A[k];if(!p){o.added.push(d);o.map[k]={add:true};f(d.t)}else if(Math.abs(p.t-d.t)>=1000){o.moved.push({d:d,from:p.t});o.map[k]={from:p.t};f(Math.min(p.t,d.t))}});
  a.dots.forEach(function(d){if(!B[key(d)]){o.gone.push(d);f(d.t)}});o.same=!o.moved.length&&!o.added.length&&!o.gone.length;return o}
function gdSimChange(){var k=G.gd.sim;if(!k.prev||!k.set)return"";var w=gdPresetWords(gdPresetOf(k.prev.g),gdPresetOf(k.set.g));return w.length?w.join(" · "):k.prev.life!==k.set.life?"Liveliness "+k.set.life+", was "+k.prev.life:"a setting changed"}
function gdSimMovedWords(prev,cur){if(!prev||G.gd.sim.prev.sig===G.gd.sim.set.sig)return"";var df=gdSimDiff(prev,cur),a=gdSimSum(prev,15),b=gdSimSum(cur,15),lab=function(d){return d.label+(d.n>1?" ("+d.n+")":"")};
  if(df.same)return"Since the last change ("+gdSimChange()+"): nothing moved.";
  var w=df.moved.slice(0,4).map(function(m){return lab(m.d)+" "+gdClock(m.from)+" → "+gdClock(m.d.t)});if(df.moved.length>4)w.push("+"+(df.moved.length-4)+" more moved");
  if(df.added.length)w.push(df.added.length+" new ("+df.added.slice(0,2).map(function(d){return lab(d)+" "+gdClock(d.t)}).join(", ")+(df.added.length>2?"…":"")+")");
  if(df.gone.length)w.push(df.gone.length+" gone ("+df.gone.slice(0,2).map(function(d){return lab(d)+" "+gdClock(d.t)}).join(", ")+(df.gone.length>2?"…":"")+")");
  return"What moved since the last change ("+gdSimChange()+"): "+w.join(" · ")+". In 15 min: "+b.tips+" tips, were "+a.tips+"."}
/* the timeline: 0-20 min, the lanes App / Content / Fun / Quiet; dots at least 10 px apart, else merged into a count; what moved ringed
   (its old place a hollow ghost); the playhead. Unity: Painter2D in the POI Editor's window (generateVisualContent), the dots as
   hit rects */
var GD_LANES=[["app","App"],["content","Content"],["fun","Fun"],["quiet","Quiet"]];
function gdSimW(){var f=ed.querySelector('[data-fo="gd:sim"] > .inner'),cs=f&&getComputedStyle(f),w=f&&f.clientWidth?f.clientWidth-parseFloat(cs.paddingLeft)-parseFloat(cs.paddingRight):(ed.clientWidth||640)-70;return Math.max(340,Math.floor(w-2))}
function gdSimR(c){return c.dots.length>1?8:c.dots[0].cue?4:5}
function gdSimClusters(dots,X){var out=[];dots.forEach(function(d){var x=X(d.t),c=out[out.length-1];if(c&&x-c.x<10)c.dots.push(d);else out.push({x:x,dots:[d]})});
  /* then neighbours closer than their two radii and 4 px merge too (so no two drawn dots touch), until none are */
  for(var again=true;again;){again=false;for(var i=1;i<out.length;i++){if(out[i].x-out[i-1].x<gdSimR(out[i-1])+gdSimR(out[i])+4){out[i-1].dots=out[i-1].dots.concat(out[i].dots);out.splice(i,1);again=true;break}}}return out}
function gdSimSvg(res,prev,W){var k=G.gd.sim,L=70,R=12,T=22,LH=28,H=T+GD_LANES.length*LH+4,X=function(t){return L+(W-L-R)*Math.min(t,GD_SIM_MS)/GD_SIM_MS},up=k.upto,h='',df=prev&&G.gd.sim.prev&&G.gd.sim.prev.sig!==G.gd.sim.set.sig?gdSimDiff(prev,res):null;
  for(var m=0;m<=20;m++){var x=X(m*60000).toFixed(1);h+='<line x1="'+x+'" x2="'+x+'" y1="'+(T-(m%5?3:6))+'" y2="'+(H-4)+'" class="gdtick'+(m%5?'':' maj')+'"/>';if(m%5===0)h+='<text x="'+x+'" y="'+(T-9)+'" text-anchor="'+(m===0?"start":m===20?"end":"middle")+'" class="gdax">'+(m?m+" min":"0")+'</text>'}
  GD_LANES.forEach(function(ln,i){var y=T+i*LH;h+='<rect x="0" y="'+y+'" width="'+W+'" height="'+LH+'" class="gdlane'+(i%2?' alt':'')+'"/><text x="8" y="'+(y+LH/2+4)+'" class="gdlab">'+ln[1]+'</text>'});
  var yOf=function(lane){return T+GD_LANES.map(function(x){return x[0]}).indexOf(lane)*LH+LH/2};
  if(!S.guidance.kinds.some(function(k){return k.id==="fun"}))h+='<text x="'+(L+6)+'" y="'+(yOf("fun")+4)+'" class="gdnone">no Fun kind</text>';
  res.quiet.forEach(function(q){if(q.from>up)return;var a=X(q.from),b=X(Math.min(q.to,up));h+='<rect x="'+a.toFixed(1)+'" y="'+(yOf("quiet")-6)+'" width="'+Math.max(1.5,b-a).toFixed(1)+'" height="12" rx="3" class="gdq gdq-'+q.cls+'"><title>'+gdClock(q.from)+"–"+gdClock(q.to)+" · quiet: "+GD_SIM_QUIET[q.cls]+'</title></rect>'});
  if(df)df.gone.forEach(function(d){if(d.t>up)return;h+='<circle cx="'+X(d.t).toFixed(1)+'" cy="'+yOf(d.kind)+'" r="5" class="gdghost"><title>gone: '+esc(d.label)+" (was "+gdClock(d.t)+')</title></circle>'});
  var px=X(k.t).toFixed(1);h+='<line x1="'+px+'" x2="'+px+'" y1="'+(T-6)+'" y2="'+(H-2)+'" class="gdhead"/><circle cx="'+px+'" cy="'+(T-6)+'" r="3.5" class="gdheadk"/>';
  ["app","content","fun"].forEach(function(lane){var ds=res.dots.filter(function(d){return d.kind===lane&&d.t<=up}),y=yOf(lane);
    gdSimClusters(ds,X).forEach(function(c){var d0=c.dots[0],i0=res.dots.indexOf(d0),cx=c.x.toFixed(1),n=c.dots.length,tip=c.dots.some(function(d){return!d.cue}),mv=df&&c.dots.map(function(d){return df.map[d.id+"#"+d.n]}).filter(Boolean)[0];
      if(mv&&mv.from!=null&&Math.abs(X(mv.from)-c.x)>=8)h+='<circle cx="'+X(mv.from).toFixed(1)+'" cy="'+y+'" r="4" class="gdghost"/>';
      var tt=c.dots.map(function(d){return gdClock(d.t)+" "+d.label+(d.cue?" (a cue)":"")}).join("\n");
      var on=k.shown&&c.dots.some(function(d){return d.t===k.shown.t&&d.id===k.shown.id});
      h+='<g class="gddot'+(n>1?' many':'')+(tip?'':' cue')+(mv?' moved':'')+(on?' on':'')+'" data-gdsimdot="'+i0+'" data-gdsimn="'+c.dots.map(function(d){return res.dots.indexOf(d)}).join(",")+'" tabindex="0" role="button" aria-label="'+esc(tt.replace(/\n/g,", "))+'"><circle cx="'+cx+'" cy="'+y+'" r="11" class="gdhit"/>'+
        (mv||on?'<circle cx="'+cx+'" cy="'+y+'" r="'+(n>1?11:8.5)+'" class="gdring2'+(on?' sel':'')+'"/>':'')+'<circle cx="'+cx+'" cy="'+y+'" r="'+(n>1?8:tip?5:4)+'" fill="'+GD_KIND_INK[lane]+'" class="gdd"/>'+
        (n>1?'<text x="'+cx+'" y="'+(y+3.5)+'" text-anchor="middle" class="gdn">'+n+'</text>':'')+'<title>'+esc(tt)+'</title></g>'})});
  return'<svg class="gdtl" data-gdsimsvg="1" width="'+W+'" height="'+H+'" viewBox="0 0 '+W+' '+H+'" role="img" aria-label="Simulate a visit: the timeline, 0 to 20 minutes">'+h+'</svg>'}
/* what the playhead is on: a dot showing then, or nothing and why (the scheduler's own words) */
function gdSimAt(res,t){var d=res.dots.filter(function(x){return x.t<=t&&(x.end==null||t<x.end)}).pop();if(d)return{dot:d};var q=res.quiet.filter(function(x){return x.from<=t&&t<x.to})[0],sk=null;res.skips.forEach(function(s){if(s.t<=t)sk=s});return{quiet:q,skip:sk}}
function gdSimNowWords(res,t){var a=gdSimAt(res,t),d=a.dot;if(d)return gdClock(d.t)+"–"+gdClock(d.end!=null?d.end:t)+" · "+(d.kind==="app"?"App":d.kind==="content"?"Content":"Fun")+" · "+d.label+(d.cue?" (a cue on the card)":"")+(d.text?" · “"+d.text+"”":"")+" · why: "+(d.why.indexOf(d.label+": ")===0?d.why.slice(d.label.length+2):d.why)+(d.how?" · it ended "+d.how:d.endWhy?" · it ended: "+d.endWhy:"");
  var why=a.skip?a.skip.why.split(" · "):[];return gdClock(t)+" · nothing shows"+(a.quiet?" (quiet: "+GD_SIM_QUIET[a.quiet.cls]+")":"")+(why.length?": "+why.slice(0,3).join(" · ")+(why.length>3?" · … ("+(why.length-3)+" more)":""):"")}
function gdSimSumWords(res){var s=gdSimSum(res,15),all=res.dots.filter(function(d){return!d.cue}).length;return s.words+" · in 20 min: "+all+" tips"}
function gdSimLearnWords(res){return res.learned.length?"The learning: "+res.learned.map(function(l){return gdClock(l.t)+" "+l.why}).join(" · ")+".":"The learning changed nothing."}
function gdSimHtml(){var sid=S.ui.gdSim,sc=gdSimScript(sid);gdSimTrack();
  var body=row("The visitor",seg("ui.gdSim",GD_SIM_SCRIPTS.map(function(x){return x.id}),GD_SIM_SCRIPTS.map(function(x){return x.name}),sid),"A scripted visitor on a fake visit clock: what it does is below. The real scheduler decides at each moment, 4 times a second, as on the phone: the same script gives the same timeline.")+
    '<div class="note gdsimsays">'+esc("The script: "+sc.says+".")+'</div>'+
    row("Speed",'<div class="btnrow" style="margin:0">'+seg("ui.gdSpeed",["1","10","30","instant"],["×1","×10","×30","Instant"],S.ui.gdSpeed)+'<button class="ubtn sm" type="button" data-gdsimplay="1">'+(G.gd.sim.playI?"&#9632; Stop":"&#9656; Play")+'</button></div>',"Play runs the visit on its clock (×1, ×10 or ×30), each dot shown on the phone as it comes. Instant draws the whole 20 minutes at once. A tap on a dot, or a drag along the timeline, shows that moment on the phone.");
  if(!isOpen("gd:sim",2))return fo("gd:sim",'Simulate a visit <span class="sub">· a scripted visitor, 0-20 min, the real scheduler</span>',["new","NEW"],body+'<div class="note">Open it to run the simulation.</div>',2);
  var res=gdSimNow(sid),prev=gdSimPrev(sid),mv=gdSimMovedWords(prev,res);
  body+='<div class="gdtlw" id="gdsimtl">'+gdSimSvg(res,prev,gdSimW())+'</div>'+
    '<div class="gdleg"><span><i class="gdsw" style="background:'+GD_KIND_INK.app+'"></i>App</span><span><i class="gdsw cue"></i>a cue on the card</span><span><i class="gdsw" style="background:'+GD_KIND_INK.content+'"></i>Content</span><span><i class="gdsw" style="background:'+GD_KIND_INK.fun+'"></i>Fun</span><span><i class="gdsw n">3</i>merged: too close to tell apart (a tap lists them)</span><span class="gdlq">Quiet:<i class="gdsw q gdq-tour"></i>a tour<i class="gdsw q gdq-pan"></i>panning<i class="gdsw q gdq-card"></i>a card open<i class="gdsw q gdq-panel"></i>a panel open</span>'+(mv&&!/nothing moved/.test(mv)?'<span><i class="gdsw mv"></i>moved since the last change</span><span><i class="gdsw gh"></i>where it was</span>':'')+'</div>'+
    '<div class="note gdsimnow" id="gdsimnow">'+esc(gdSimNowWords(res,G.gd.sim.t))+'</div><div class="btnrow gdsimpick" id="gdsimpick">'+gdSimPickHtml(res)+'</div><div class="note gdsimsum" id="gdsimsum">'+esc(gdSimSumWords(res))+'</div><div class="note gdsimlearn">'+esc(gdSimLearnWords(res))+'</div>'+(mv?'<div class="note gdsimmoved" id="gdsimmoved">'+esc(mv)+'</div>':'');
  return fo("gd:sim",'Simulate a visit <span class="sub">· a scripted visitor, 0-20 min, the real scheduler</span>',["new","NEW"],body,2)}
/* the timeline drawn again in place (the playhead, a playback) */
function gdSimDraw(){var w=ed.querySelector("#gdsimtl");if(!w)return;var res=gdSimNow(),prev=gdSimPrev();w.innerHTML=gdSimSvg(res,prev,gdSimW());var n=ed.querySelector("#gdsimnow");if(n)n.textContent=gdSimNowWords(res,G.gd.sim.t);
  var pk=ed.querySelector("#gdsimpick");if(pk)pk.innerHTML=gdSimPickHtml(res)}
/* a merged count tapped: its items, each a button that shows it */
function gdSimPickHtml(res){var p=G.gd.sim.pick||[];return p.length>1?'<span>'+p.length+' here:</span>'+p.map(function(i){var d=res.dots[i];return d?'<button type="button" class="ubtn sm'+(G.gd.sim.shown===d?' on':'')+'" data-gdsimpick="'+i+'">'+gdClock(d.t)+" "+esc(d.label)+(d.cue?" (cue)":"")+'</button>':""}).join(""):""}
/* a dot shown on the phone at its moment: the wall (or, for a cue, the card at its element), its line as it was said; a demo (pulled: no memory) */
function gdSimShow(d,inDemo){var it=gdItems().filter(function(x){return x.id===d.id})[0];if(!it)return null;var my=inDemo?null:gdDemoBegin(),sc=gdSimScript(S.ui.gdSim);
  if(it.form==="cue")cuePlace(it.id);else{if(!(S.view==="card"&&S.stop==="closed"&&!G.on&&!S.settingsOpen&&!G.cam.open))gdOnWallNow();else gdStop("the simulator");gdSimAim(it.about==="marker"?((S.wallPois||[]).filter(function(x){return x.card})[0]||{}).id:d.at)}
  /* a Moment: in the tour, as the visitor had it (round 7) */
  var c=it.form==="moment"?gdDemoShow(it,6500,{text:d.text}):gdStart(it,"the simulator ("+sc.name+", "+gdClock(d.t)+")",{pull:true,ms:it.form==="cue"?4000:6500,text:it.form==="cue"?undefined:d.text,ret:!!sc.returning});
  gdChip("Simulated · "+sc.name+" · "+gdClock(d.t)+" · "+gdSimShort(d));G.gd.sim.shown=d;if(!inDemo)gdDemoEnd(my,7000);return c}
/* the phone turned to the point the visitor was pointing at then (a tip about the marker: to the marker, which the script keeps in view): in the middle of the screen (the wall zoomed in when the point sits
   too low or too high to reach the middle; Ask about the view's agCenter pans) */
function gdSimAim(id){var p=(S.wallPois||[]).filter(function(x){return x.id===id})[0];if(!p||typeof agCenter!=="function")return;S.camZoom=1;agCenter(id);var s=poiScreen(p);
  if(!gdInMiddle(s.x,s.y)){S.camZoom=1.6;agCenter(id)}renderPhone(false)}
/* a dot's name short enough for the phone's chip (a remark by its point) */
function gdSimShort(d){return d.label.replace(/ in the middle$/,"")}
function gdSimStop(){var k=G.gd.sim;if(k.playI){clearInterval(k.playI);k.playI=0}var b=ed.querySelector("[data-gdsimplay]");if(b)b.innerHTML="&#9656; Play"}
function gdSimPlay(){var k=G.gd.sim;if(k.playI){gdSimStop();return}if(S.ui.gdSpeed==="instant"){k.t=0;k.upto=GD_SIM_MS;gdSimDraw();return}
  var rate=+S.ui.gdSpeed||10,res=gdSimNow();if(k.t>=GD_SIM_MS)k.t=0;k.upto=k.t;var b=ed.querySelector("[data-gdsimplay]");if(b)b.innerHTML="&#9632; Stop";
  k.playI=setInterval(function(){var t0=k.t;k.t=Math.min(GD_SIM_MS,k.t+100*rate);k.upto=k.t;var hit=res.dots.filter(function(d){return(d.t>t0||t0===0&&d.t===0)&&d.t<=k.t}).pop();if(hit)gdSimShow(hit);gdSimDraw();
    if(k.t>=GD_SIM_MS){gdSimStop();k.upto=GD_SIM_MS}},100)}
/* the playhead put at a time: a dot showing then goes on the phone */
function gdSimSeek(t){var k=G.gd.sim;k.t=Math.max(0,Math.min(GD_SIM_MS,t));if(k.upto<k.t)k.upto=k.t;var a=gdSimAt(gdSimNow(),k.t);if(a.dot&&a.dot!==k.shown)gdSimShow(a.dot);gdSimDraw()}
on("setOpt.answer",function(key,v){if(key==="ui.gdSim"||key==="ui.gdSpeed"){gdSimStop();S.ui[key.slice(3)]=v;var k=G.gd.sim;if(key==="ui.gdSim"||v==="instant"){k.t=0;k.upto=GD_SIM_MS;k.shown=null}renderEditor();return true}
  /* a preset chosen writes its values (Custom keeps them as they are) */
  if(key==="@guidance.preset"){if(v!=="custom")gdPresetWrite(v);renderAll(true);return true}});
ed.addEventListener("click",function(e){var t=e.target;var pl=t.closest("[data-gdsimplay]");if(pl){e.stopPropagation();gdSimPlay();return}
  var dt=t.closest("[data-gdsimdot]");if(dt){e.stopPropagation();gdSimStop();var res=gdSimNow(),d=res.dots[+dt.dataset.gdsimdot];if(!d)return;G.gd.sim.pick=String(dt.dataset.gdsimn||"").split(",").map(Number);G.gd.sim.t=d.t;gdSimShow(d);gdSimDraw();return}
  var pi=t.closest("[data-gdsimpick]");if(pi){e.stopPropagation();var r2=gdSimNow(),d2=r2.dots[+pi.dataset.gdsimpick];if(!d2)return;G.gd.sim.t=d2.t;gdSimShow(d2);gdSimDraw();return}},true);
/* a drag along the timeline scrubs it (a press on a dot is the dot's own) */
ed.addEventListener("pointerdown",function(e){var sv=e.target.closest&&e.target.closest("[data-gdsimsvg]");if(!sv||e.target.closest("[data-gdsimdot]"))return;gdSimStop();var r=sv.getBoundingClientRect(),W=+sv.getAttribute("width"),L=70,R=12;
  var at=function(ev){return(Math.max(L,Math.min(W-R,(ev.clientX-r.left)*W/r.width))-L)/(W-L-R)*GD_SIM_MS};G.gd.sim.drag=true;gdSimSeek(at(e));
  var mv=function(ev){if(G.gd.sim.drag)gdSimSeek(at(ev))},up=function(){G.gd.sim.drag=false;document.removeEventListener("pointermove",mv);document.removeEventListener("pointerup",up)};
  document.addEventListener("pointermove",mv);document.addEventListener("pointerup",up)},true);
ed.addEventListener("keydown",function(e){var dt=e.target.closest&&e.target.closest("[data-gdsimdot]");if(dt&&(e.key==="Enter"||e.key===" ")){e.preventDefault();dt.dispatchEvent(new MouseEvent("click",{bubbles:true}))}},true);
/* the simulator runs when its foldout opens */
ed.addEventListener("toggle",function(e){var d=e.target;if(d.dataset&&d.dataset.fo==="gd:sim"&&d.open&&!d.querySelector("#gdsimtl")&&!edQuiet(d))setTimeout(renderEditor,0)},true);
on("renderEditor.after",function(){gdSimTrack()});
/* ----- round 5's rows' demos: the change re-runs the simulator; the phone plays the two visit-minutes where something moved, ×10 (each dot's
   item at its moment, the chip saying what moved); the old ×10 chip demos stay for the rows that had them ----- */
function gdDemoSim(o){o=o||{};var sid=S.ui.gdSim||"first",sc=gdSimScript(sid),cur=o.set?gdSimRes(sid,o.set):gdSimNow(sid),prev=o.set?gdSimNow(sid):gdSimPrev(sid),df=prev?gdSimDiff(prev,cur):null;
  var t0=df&&df.first!=null?Math.max(0,Math.floor((df.first-5000)/5000)*5000):0,t1=t0+120000,dots=cur.dots.filter(function(d){return d.t>=t0&&d.t<t1}),tips=dots.filter(function(d){return!d.cue}).length,was=prev?prev.dots.filter(function(d){return!d.cue&&d.t>=t0&&d.t<t1}).length:null;
  /* the chips stay one short line on the phone: what changed, the two minutes, each item's time, name and move */
  var st=[{chip:"×10 · "+(o.label||"Simulated")+" · "+gdClock(t0)+"–"+gdClock(t1)+(df&&df.same?" · nothing moved":""),wait:Math.max(900,dots.length?(dots[0].t-t0)/10:1600)}];
  dots.forEach(function(d,i){var m=df&&df.map[d.id+"#"+d.n],nx=dots[i+1];st.push({fn:function(){gdSimShow(d,true)},chip:"×10 · "+gdClock(d.t)+" · "+gdSimShort(d)+(m&&m.from!=null?" · was "+gdClock(m.from):m&&m.add?" · new":""),wait:Math.max(1300,nx?(nx.t-d.t)/10:1800)})});
  st.push({stop:true,chip:"×10 · 2 min: "+tips+" tip"+(tips===1?"":"s")+(was!=null&&was!==tips?" (were "+was+")":"")+(o.after?" · "+o.after:""),wait:2600});gdDemo(st)}
/* an audience's Guidance preset (Detail Card › 6 · Audiences): that preset simulated against the settings now */
function gdDemoAudPreset(a){var p=S.aud.looks[a].gdPreset,g=JSON.parse(JSON.stringify(S.guidance)),keep=S.guidance;S.guidance=g;gdPresetWrite(p);S.guidance=keep;
  var set={sig:"aud:"+p+"|"+JSON.stringify(g),g:g,life:S.aud.looks[a].life};gdDemoSim({set:set,label:AUDN[a]+" → "+gdPresetName(p),after:gdSimSum(gdSimRes(S.ui.gdSim||"first",set),15).tips+" in 15 min"})}
var GD_DEMOS5={"guidance.preset":{wall:true,act:function(){gdDemoSim({label:"Preset "+gdPresetName(gdPresetNow())})},ringAfter:600,ring:".gdchip|.gdbub"},
  "guidance.talk":{wall:true,act:function(){gdDemoSim({label:gdTalk().name+": room "+gdRoomNow()+" s, "+gdLimitNow(S.guidance.perMinute)+"/min"})},ringAfter:600,ring:".gdchip|.gdbub"}};
GD_MIX_KINDS.forEach(function(k){[0,1,2].forEach(function(i){GD_DEMOS5["guidance.mix."+k[0]+"."+i]={wall:true,act:function(){gdDemoSim({label:"Mix "+k[1]+" s "+["0","0.3","1"][i]+": "+gdMix(k[0])[i]+" %"})},ringAfter:600,ring:".gdchip|.gdbub"}})});
rowDemos(GD_DEMOS5);
/* ----- round 6: the full item form (_3.8 section 3.2's groups: Identity, Words, About, Show, When, How much; gdItemRow above), its named
   warnings, + Add guidance with its templates, Duplicate, Delete with an Undo toast (no popup), drag ≡ to reorder inside a group (the
   tie-break and an In order group's sequence; the order across kinds is the Mix table, never a drag), and every field row's demo (the item
   on the phone with the new value). Preview & Test › The visitor gains Tips, Tips stay until tapped and Screen reader on (the Lab's
   stand-in). Unity: the POI Editor's GuidanceItem inspector (IMGUI foldouts per group, a ReorderableList for the drag, the templates a
   GenericMenu-free inline list, Undo.RecordObject for Delete's Undo shown in the window's own toast) ----- */
S.ui.gdSR=false;S.ui.gdAddIn=null;
/* round 7: Preview & Test › Today is (the real date until one is picked) and the point whose Jokes Specific Marker › Guide script shows */
S.ui.gdToday=gdTodayISO();S.ui.gdJkPoint="painting";
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(ui\\.gdToday|ui\\.gdJkPoint|"));
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(gdVisitor\\.|"));
TX_DOM.gd_btn_show="shown on a tip's Button chip";TX_DOM.gd_btn_tour=TX_DOM.gd_btn_show;TX_DOM.gd_btn_card=TX_DOM.gd_btn_show;TX_DOM.gd_btn_view=TX_DOM.gd_btn_show;
["settings_tips","settings_tips_all","settings_tips_ess","settings_tips_off","settings_tips_off_note","settings_tips_stay","settings_tips_again"].forEach(function(k){TX_DOM[k]="shown in the visitor's Settings › The guide"});
TX_DOM.gd_tips_again_done="shown in the toast after Show the tips again";TX_DOM.gd_undo=TX_DOM.gd_tips_again_done;TX_DOM.gd_dismiss="on a tip that stays until tapped";TX_DOM.dock_tips="shown in the dock's conversation";
function gdDragH(it){return'<button type="button" class="gddrag" data-gddrag="'+it.id+'" aria-label="Drag to reorder the '+esc(it.label)+'" title="Drag to reorder inside its group (the tie-break and the order)">&#8942;&#8942;</button>'}
/* the groups an item may move to (not a pulled group, not the card's cues: those hold their own forms) */
function gdGroupOpts(){var o=[];S.guidance.kinds.forEach(function(k){k.groups.forEach(function(g){if(!g.pulled&&!g.launch&&g.id!=="card")o.push([g.id,k.name+" › "+g.name])})});return o}
/* Says, one list (round 14): each variant once, its words in each language side by side (the same keys as Languages & Terms), ▸ shows
   the item saying it, Remove, + Add a variant */
function gdTxIn(k){return S.tx[k]?txInputs(k).replace(/<\/?td>/g,""):'<span class="gdvt empty">(no words)</span>'}
function gdSaysHtml(it){var ks=[].concat(it.says||[]);
  return'<div class="gdsays"><table class="tbl txt gdsayt"><thead><tr><th style="width:22px">#</th>'+langHead()+'<th style="width:104px"></th></tr></thead><tbody>'+ks.map(function(k,i){
    return'<tr class="gdsrow" data-gdvark="'+esc(k)+'"><td class="gdvn">'+(i+1)+'</td>'+(S.tx[k]?txInputs(k):'<td colspan="'+langCodes().length+'"><span class="gdvt empty">(no words)</span></td>')+
      '<td class="gdva"><button class="ubtn sm" type="button" data-gdsay="'+it.id+'|'+k+'" title="Show the item saying this line" aria-label="Show line '+(i+1)+' on the phone">&#9656;</button>'+
      (ks.length>1?'<button class="del" type="button" data-gdvardel="'+it.id+'|'+i+'">Remove</button>':"")+'</td></tr>'}).join("")+'</tbody></table>'+
    (ks.length<4?'<button class="ubtn sm" type="button" data-gdvaradd="'+it.id+'">+ Add a variant</button>':'<span class="uses">4 variants: the most</span>')+'</div>'}
function gdAudHtml(it){var A=GD_AUD.slice(1),has=function(a){return!!(it.saysAud&&it.saysAud[a[0]])};
  return'<div class="gdvars">'+A.filter(has).map(function(a){var k=it.saysAud[a[0]];return'<div class="gdvar gdvarin"><span class="gdvn">'+esc(a[1])+'</span>'+gdTxIn(k)+'<button class="ubtn sm" type="button" data-gdsay="'+it.id+'|'+k+'|'+a[0]+'" aria-label="Show '+esc(a[1])+'\'s line on the phone">&#9656;</button><button class="del" type="button" data-gdauddel="'+it.id+'|'+a[0]+'">Remove</button></div>'}).join("")+
    '<div class="btnrow" style="margin:0">'+A.filter(function(a){return!has(a)}).map(function(a){return'<button class="ubtn sm" type="button" data-gdaudadd="'+it.id+'|'+a[0]+'">+ '+esc(a[1])+'</button>'}).join("")+'</div></div>'}
function gdBackHtml(it){var k=it.saysBack;return'<div class="gdvars">'+(k&&S.tx[k]?'<div class="gdvar gdvarin">'+gdTxIn(k)+'<button class="ubtn sm" type="button" data-gdsay="'+it.id+'|'+k+'|back" aria-label="Show the returning visitor\'s line on the phone">&#9656;</button>'+(TXOWN[k]==="gd:i:"+it.id&&/^gdw_/.test(k)?'<button class="del" type="button" data-gdbackdel="'+it.id+'">Remove</button>':"")+'</div>':
  '<button class="ubtn sm" type="button" data-gdbackadd="'+it.id+'">+ A returning visitor\'s line</button>')+'</div>'}
/* Focus: Spotlight offered only where it is allowed (else a disabled choice that says why) */
function gdFocusSeg(it){var key="@guidance.items."+it.id+".focus",ok=gdSpotOk(it);
  return'<div class="seg">'+GD_FOCUS.map(function(f){if(f[0]==="spotlight"&&!ok)return'<button type="button" class="gdno" disabled aria-disabled="true" title="'+esc(gdSpotWhy(it))+'">Spotlight</button>';
    return'<button type="button" data-set="'+key+'" data-v="'+f[0]+'" aria-pressed="'+(it.focus===f[0])+'">'+f[1]+'</button>'}).join("")+'</div>'+(!ok?'<div class="gdforce gdspotwhy">'+esc(gdSpotWhy(it)+(it.focus==="spotlight"?": shown as a Ring.":"."))+'</div>':"")}
/* Only if, as chips: every condition in force, each with its × (the screen has none: it is always one) */
function gdOnlyList(it){var c=[],x=function(f,t){c.push([f,t])};
  x(null,it.screen==="any"?"on the wall or a card":it.screen==="card"?"on a card":"on the wall");if(+it.stillFor>0)x("stillFor","the phone still "+it.stillFor+" s");
  if(it.aud)x("aud","the audience is "+gdNameOf(GD_AUD,it.aud));if(it.visitIs)x("visitIs",gdNameOf(GD_VISIT_IS,it.visitIs).toLowerCase());
  if(it.hasUsed)x("hasUsed","has used: "+gdNameOf(GD_USED,it.hasUsed));if(it.notUsed)x("notUsed","has not used: "+gdNameOf(GD_USED,it.notUsed));
  if(gdKindSolemnOff(it))x(null,"not on Solemn points (fixed for "+((gdKindOf(it)||{}).name||"Fun")+")");else if(it.notSolemn)x("notSolemn","not on Solemn points");if(it.lively)x("lively","Lively only");
  if(it.when==="between")x(null,"in a tour, between two steps");if(it.seasons)x(null,"in its window of dates");if(it.ptJokes)x(null,"a point with a joke of its own");if(it.joke)x(null,"not on a Solemn day");
  if(+it.notBefore>0)x("notBefore","from "+gdClock(+it.notBefore*1000));if(+it.notAfter>0)x("notAfter","until "+gdClock(+it.notAfter*1000));return c}
function gdOnlyChips(it){return gdOnlyList(it).map(function(o){var f=o[0],t=o[1];return'<span class="gdchipc">'+esc(t)+(f?'<button type="button" data-gdoff="'+it.id+'|'+f+'" aria-label="Remove: '+esc(t)+'">&#10005;</button>':"")+'</span>'}).join("")}
/* ----- the named warnings: an item that can never show; a line too long for 2 lines in either language; a line said by two items ----- */
G.gd.warns=[];
function gdSample(t){var n=(S.wallPois||[]).map(function(p){return L(p.name)}).filter(function(x){return/Ribeira|Paço/.test(x)})[0]||"The Ribeira Palace";return String(t).replace(/\{point\}/g,n).replace(/\{year\}/g,"1755").replace(/\{min\}/g,"4").replace(/\{button\}/g,S.guideCfg.gbIcon==="star"?"✦":TXT("gb_label"))}
function gdWarnings(){var out=[],seen={},langs=langCodes(),keepL=S.lang;
  gdDefs().filter(gdIsTip).forEach(function(it){gdNever(it).forEach(function(r){out.push({id:it.id,kind:"never",text:"The “"+it.label+"” can never show: "+r+"."})});
    (it.form==="beat"?[]:gdLineKeys(it)).forEach(function(k){langs.forEach(function(l){var t=String(S.tx[k][l]||"").trim();if(!t)return;var n=gdLineCount(gdSample(t),it.form,!!gdBtnOf(it));
      if(n>2)out.push({id:it.id,kind:"long",text:"The “"+it.label+"” has a line too long for 2 lines in "+(GD_LANG_NAME[l]||l)+" ("+n+" lines): “"+t+"”. Shorten it."});
      var key=l+"|"+t.toLowerCase();(seen[key]=seen[key]||[]).push(it)})})});
  var dup={};Object.keys(seen).forEach(function(key){var its=seen[key].filter(function(x,i,a){return a.indexOf(x)===i});if(its.length<2)return;var l=key.split("|")[0],t=String(S.tx[gdLineKeys(its[0]).filter(function(k){return String(S.tx[k][l]||"").trim().toLowerCase()===key.slice(l.length+1)})[0]][l]);
    its.forEach(function(it){var o=its.filter(function(x){return x!==it}).map(function(x){return"the “"+x.label+"”"}).join(", "),dk=it.id+"|"+o;if(!dup[dk]){dup[dk]={id:it.id,kind:"dup",o:o,label:it.label,lines:[]};out.push(dup[dk])}dup[dk].lines.push("“"+t+"” ("+(GD_LANG_NAME[l]||l)+")")})});
  Object.keys(dup).forEach(function(k){var w=dup[k];w.text="The “"+w.label+"” says the same line as "+w.o+": "+w.lines.join(", ")+"."});
  S.lang=keepL;return out}
function gdWarnsOf(id){return G.gd.warns.filter(function(w){return w.id===id})}
function gdWarnsHtml(games){var w=G.gd.warns.filter(function(x){var it=S.guidance.items[x.id];return!!(it&&it.group==="ontour")===!!games});return'<div class="gdwarns'+(w.length?" on":"")+'" id="'+(games?"gdwarnsg":"gdwarns")+'" role="status">'+(w.length?'<b>&#9888; '+w.length+(w.length>1?" warnings":" warning")+'</b>'+w.map(function(x){return'<div class="gdwl" data-gdwarn="'+x.kind+'"><span>'+esc(x.text)+'</span><button class="ubtn sm" type="button" data-gdgoitem="'+x.id+'">Open it</button></div>'}).join(""):
  (games?"No warnings: every game can be planned, and its words are there.":"No warnings: every tip can show, every line fits in 2 lines in each language, and no line is said by two items."))+'</div>'}
/* ----- new words, new items: + Add guidance (a template), Duplicate; Delete (an Undo toast) ----- */
function gdFreeId(pre,map){var n=1;while(map[pre+n])n++;return pre+n}
function gdNewWord(owner,label,en,pt){var k=gdFreeId("gdw_",S.tx);S.tx[k]={en:en||"",pt:pt||""};(TXO[owner]=TXO[owner]||[]).push(k);TXW[k]="A guidance item's line (Guide › The mascot speaks up).";GD_TXDEF[k]={en:en||"",pt:pt||""};TXF[k]="";TXP[k]=label;TXOWN[k]=owner;TX_DOM[k]="shown in the guide's tip";return k}
function gdRegister(it){var g=gdGroupById(it.group);COMP["gd:i:"+it.id]=["Guide",it.group==="ontour"?"Games on the tour › "+it.label:"The mascot speaks up › "+(g?g.name:"?")+" › "+it.label];if(!GD_ITEM0[it.id])gdDefSnap(it.id);FOCUS["gd:i:"+it.id]=["card",it.about==="gbtn"?".gbtn-wrap":it.about==="nothing"?"#wall":"[data-marker]","closed"]}
function gdNewItem(tid,gid){var T=GD_TEMPLATES.filter(function(x){return x.id===tid})[0],g=gdGroupById(gid);if(!T||!g)return null;var id=gdFreeId("gdn",S.guidance.items),it=JSON.parse(JSON.stringify(T.f));
  it.id=id;it.on=true;it.kind=gdKindOfGroup(gid);it.group=gid;gdFill(it);it.says=gdNewWord("gd:i:"+id,"Says",T.en,T.pt);S.guidance.items[id]=it;
  /* round 8: a page gets its title and one button (Next), and goes before Straight to the wall (the run ends there) */
  if(T.page){it.title=gdNewWord("gd:i:"+id,"Title",T.title.en,T.title.pt);it.buttons=[{word:gdNewWord("gd:i:"+id,"Button 1",T.btn.en,T.btn.pt),act:"next"}];
    var wi=g.items.map(function(x){return(S.guidance.items[x]||{}).page}).indexOf("wall");if(wi>=0)g.items.splice(wi,0,id);else g.items.push(id)}else g.items.push(id);gdRegister(it);gdLog("event","the Editor added the “"+it.label+"” ("+T.name+") to "+g.name);return it}
function gdDupItem(id){var src=S.guidance.items[id],g=src&&gdGroupOf(src);if(!g)return null;var nid=gdFreeId("gdn",S.guidance.items),it=JSON.parse(JSON.stringify(src)),own="gd:i:"+nid,cp=function(k){return k&&S.tx[k]?gdNewWord(own,TXP[k]||"Says",S.tx[k].en,S.tx[k].pt):k};
  it.id=nid;it.label=src.label+" (copy)";it.says=Array.isArray(src.says)?src.says.map(cp):cp(src.says);if(src.form==="page"){it.title=cp(src.title);it.buttons=(src.buttons||[]).map(function(b){return{word:cp(b.word),act:b.act}})}if(src.saysCard)it.saysCard=cp(src.saysCard);it.saysBack=src.saysBack?cp(src.saysBack):"";it.saysAud={};Object.keys(src.saysAud||{}).forEach(function(a){it.saysAud[a]=cp(src.saysAud[a])});
  S.guidance.items[nid]=it;g.items.splice(g.items.indexOf(id)+1,0,nid);gdRegister(it);gdLog("event","the Editor duplicated the “"+src.label+"”");return it}
function gdDelItem(id){var it=S.guidance.items[id],g=it&&gdGroupOf(it);if(!g)return false;var i=g.items.indexOf(id);if(G.gd.cur&&(G.gd.cur.item.base||G.gd.cur.item.id)===id)gdStop("its item was deleted");
  g.items.splice(i,1);it.deleted=true;G.gd.undoDel={id:id,gid:g.id,i:i};gdLog("event","the Editor deleted the “"+it.label+"” from "+g.name);return true}
function gdUndoDel(){var u=G.gd.undoDel,g=u&&gdGroupById(u.gid),it=u&&S.guidance.items[u.id];if(!g||!it)return false;delete it.deleted;g.items.splice(Math.min(u.i,g.items.length),0,u.id);G.gd.undoDel=null;gdLog("event","the Editor's delete undone: the “"+it.label+"” is back");return true}
function gdMoveItem(id,gid){var it=S.guidance.items[id],a=it&&gdGroupOf(it),b=gdGroupById(gid);if(!a||!b||a===b)return false;a.items.splice(a.items.indexOf(id),1);b.items.push(id);it.group=gid;it.kind=gdKindOfGroup(gid);gdRegister(it);return true}
/* the Editor's toast (no popup): what was done, and Undo where it can be undone. Unity: a label at the foot of the POI Editor's window */
function gdEdToast(text,undo){var o=document.getElementById("gdedtoast");if(o)o.remove();clearTimeout(G.gd.edToastT);var r=ed.getBoundingClientRect(),d=document.createElement("div");d.id="gdedtoast";d.className="gdedtoast";d.setAttribute("role","status");
  d.style.left=Math.round(r.left+16)+"px";d.style.width=Math.round(Math.max(260,r.width-32))+"px";d.innerHTML='<span>'+esc(text)+'</span>'+(undo?'<button type="button" class="ubtn sm" data-gdedundo="'+undo+'">Undo</button>':"");document.body.appendChild(d);
  G.gd.edToastT=setTimeout(function(){d.remove()},8000);return d}
/* an item opened where it is (round 14: grp, one of its groups opened too: a new item's Says, where its words are written) */
function gdOpenItem(id,grp){var it=S.guidance.items[id];if(!it)return;S.tab="guide";gdItemPath(it).forEach(function(x){S.op[x]=true});if(grp)S.op["gd:f:"+id+":"+grp]=true;renderEditor();var f=ed.querySelector('[data-fo="gd:i:'+id+'"]');if(f)f.scrollIntoView({block:"center"})}
ed.addEventListener("click",function(e){var t=e.target,q;
  var dg=t.closest("[data-gddrag]");if(dg){e.preventDefault();e.stopPropagation();return}
  var ad=t.closest("[data-gdadd]");if(ad){e.stopPropagation();S.ui.gdAddIn=ad.dataset.gdadd&&S.ui.gdAddIn!==ad.dataset.gdadd?ad.dataset.gdadd:null;renderEditor();return}
  var tp=t.closest("[data-gdtpl]");if(tp){e.stopPropagation();q=tp.dataset.gdtpl.split("|");var nw=gdNewItem(q[1],q[0]);S.ui.gdAddIn=null;if(!nw)return;gdOpenItem(nw.id,"says");renderPhone(true);
    gdEdToast("Added the “"+nw.label+"” to "+gdGroupById(q[0]).name+". Its words are in Says.");showRowDemo("guidance.items."+nw.id+".on");return}
  var du=t.closest("[data-gddup]");if(du){e.stopPropagation();var c=gdDupItem(du.dataset.gddup);if(!c)return;gdOpenItem(c.id);gdEdToast("Duplicated: the “"+c.label+"”, just below it.");showRowDemo("guidance.items."+c.id+".on");return}
  var dl=t.closest("[data-gddel]");if(dl){e.stopPropagation();var it=S.guidance.items[dl.dataset.gddel];if(!gdDelItem(dl.dataset.gddel))return;renderAll(true);gdEdToast("Deleted the “"+it.label+"” from "+gdGroupById(G.gd.undoDel.gid).name+".","del");return}
  var va=t.closest("[data-gdvaradd]");if(va){e.stopPropagation();var a=S.guidance.items[va.dataset.gdvaradd],ks=[].concat(a.says);if(ks.length>=4)return;ks.push(gdNewWord("gd:i:"+a.id,"Says (variant "+(ks.length+1)+")","",""));a.says=ks;renderEditor();return}
  var vd=t.closest("[data-gdvardel]");if(vd){e.stopPropagation();q=vd.dataset.gdvardel.split("|");var b=S.guidance.items[q[0]],k2=[].concat(b.says),gone=k2.splice(+q[1],1)[0];b.says=k2.length===1?k2[0]:k2;G.gd.undoVar={id:q[0],i:+q[1],k:gone};renderAll(true);
    gdEdToast("Removed line "+(+q[1]+1)+" of the “"+b.label+"”: “"+String(TXT(gone)||"(no words)").slice(0,48)+"”.","var");return}
  var aa=t.closest("[data-gdaudadd]");if(aa){e.stopPropagation();q=aa.dataset.gdaudadd.split("|");var c2=S.guidance.items[q[0]];c2.saysAud=c2.saysAud||{};c2.saysAud[q[1]]=gdNewWord("gd:i:"+c2.id,"Says ("+gdNameOf(GD_AUD,q[1])+")","","");renderEditor();return}
  var adl=t.closest("[data-gdauddel]");if(adl){e.stopPropagation();q=adl.dataset.gdauddel.split("|");delete S.guidance.items[q[0]].saysAud[q[1]];renderAll(true);return}
  var ba=t.closest("[data-gdbackadd]");if(ba){e.stopPropagation();var c3=S.guidance.items[ba.dataset.gdbackadd];c3.saysBack=gdNewWord("gd:i:"+c3.id,"Says (returning visitor)","","");renderEditor();return}
  var bd=t.closest("[data-gdbackdel]");if(bd){e.stopPropagation();S.guidance.items[bd.dataset.gdbackdel].saysBack="";renderAll(true);return}
  var sy=t.closest("[data-gdsay]");if(sy){e.stopPropagation();q=sy.dataset.gdsay.split("|");gdDemoSay(q[0],q[1],q[2]);return}
  var of=t.closest("[data-gdoff]");if(of){e.stopPropagation();q=of.dataset.gdoff.split("|");S.guidance.items[q[0]][q[1]]=GD_TIP_DEF[q[1]];renderAll(true);showRowDemo("guidance.items."+q[0]+"."+q[1]);return}
  var gi=t.closest("[data-gdgoitem]");if(gi){e.stopPropagation();gdOpenItem(gi.dataset.gdgoitem);return}},true);
/* a guidance word typed: the warnings box follows (a line too long, the same line twice, no words) */
document.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.tx||!ed.contains(t))return;var k=t.dataset.tx.split("|")[0];if(!/^gd:/.test(TXOWN[k]||""))return;
  clearTimeout(G.gd.warnT);G.gd.warnT=setTimeout(function(){G.gd.warns=gdWarnings();var w=ed.querySelector("#gdwarns");if(w)w.outerHTML=gdWarnsHtml();var wg=ed.querySelector("#gdwarnsg");if(wg)wg.outerHTML=gdWarnsHtml(true)},250)},true);
/* the toast's Undo (the toast sits over the page, outside the Editor's panel) */
document.addEventListener("click",function(e){var un=e.target.closest&&e.target.closest("[data-gdedundo]");if(!un)return;e.stopPropagation();
  /* a removed line of Says back in its place (its words were never deleted: the same key) */
  if(un.dataset.gdedundo==="var"){var v=G.gd.undoVar,vi=v&&S.guidance.items[v.id];if(!vi)return;var ks=[].concat(vi.says);ks.splice(Math.min(v.i,ks.length),0,v.k);vi.says=ks;G.gd.undoVar=null;renderAll(true);var y=document.getElementById("gdedtoast");if(y)y.remove();gdEdToast("Undone: line "+(v.i+1)+" of the “"+vi.label+"” is back.");return}
  var u=G.gd.undoDel;
  if(gdUndoDel()){gdOpenItem(u.id);renderPhone(true);var x=document.getElementById("gdedtoast");if(x)x.remove();gdEdToast("Undone: the “"+S.guidance.items[u.id].label+"” is back in its place.")}},true);
/* a group chosen for an item moves it (its kind follows) */
on("setOpt.answer",function(key,v){var m=/^@guidance\.items\.([^.]+)\.group$/.exec(key);if(m){gdMoveItem(m[1],v);renderAll(true);return true}
  if(key==="ui.gdSR"){S.ui.gdSR=v==="true";renderEditor();gdDemoSR();return true}});
/* ----- drag ≡ to reorder inside a group: the row follows the pointer, a line shows where it lands; the group's order is the tie-break
   (and an In order group's sequence) ----- */
ed.addEventListener("pointerdown",function(e){var h=e.target.closest&&e.target.closest("[data-gddrag]");if(!h)return;e.preventDefault();e.stopPropagation();var row=h.closest("details[data-fo]"),box=row.parentElement,
    rows=[].filter.call(box.children,function(x){return x.matches&&x.matches('details[data-fo^="gd:i:"]')}),ln=document.createElement("div");ln.className="gddropline";box.appendChild(ln);
  G.gd.drag={id:h.dataset.gddrag,row:row,rows:rows,box:box,y0:e.clientY,from:rows.indexOf(row),to:rows.indexOf(row),line:ln};row.classList.add("gddragging");try{h.setPointerCapture(e.pointerId)}catch(x){}},true);
ed.addEventListener("pointermove",function(e){var d=G.gd.drag;if(!d)return;var dy=e.clientY-d.y0;d.row.style.translate="0 "+Math.round(dy)+"px";var to=0,br=d.box.getBoundingClientRect();
  d.rows.forEach(function(r,i){if(r===d.row)return;var b=r.getBoundingClientRect();if(e.clientY>b.top+b.height/2)to=i+(i<d.from?1:0)});if(e.clientY<d.rows[0].getBoundingClientRect().top)to=0;d.to=Math.max(0,Math.min(d.rows.length-1,to));
  var others=d.rows.filter(function(r){return r!==d.row}),ref=others[d.to],y=ref?ref.getBoundingClientRect().top:others.length?others[others.length-1].getBoundingClientRect().bottom:br.top;d.line.style.top=Math.round(y-br.top+d.box.scrollTop-2)+"px";
  var why=gdDragWhy(d);d.line.classList.toggle("no",!!why);d.line.dataset.why=why||""},true);
ed.addEventListener("pointerup",function(e){var d=G.gd.drag;if(!d)return;G.gd.drag=null;d.row.classList.remove("gddragging");d.row.style.translate="";d.line.remove();if(d.to===d.from)return;
  var it=S.guidance.items[d.id],g=gdGroupOf(it),ids=d.rows.map(function(r){return r.dataset.fo.slice(5)}),mv=ids.splice(d.from,1)[0];ids.splice(d.to,0,mv);
  var why=g.launch?gdPagesRule(ids):"";if(g.launch){G.gd.pgRule=why;if(why){renderEditor();gdEdToast(why+". The drag was undone.");gdLog("event","the Editor's drag undone: "+why);return}}
  /* the dragged rows take the places they held in the group (a drag inside the quiz's pair moves no other item) */
  var at=0;g.items=g.items.map(function(x){return ids.indexOf(x)>=0?ids[at++]:x});renderAll(true);gdLog("event","the Editor moved the “"+it.label+"” to "+(d.to+1)+" of "+ids.length+" in "+g.name);
  gdEdToast("Moved: the “"+it.label+"” is "+(d.to+1)+" of "+ids.length+" in "+g.name+" ("+(g.mode==="order"?"its place in the sequence":"it wins a tie against the ones below it")+").")},true);
/* ----- the Editor's new parts: the warnings at the top of 4 · Guidance, + Add guidance in each group, the visitor's Tips and the
   screen reader in 6 · Preview & Test ----- */
/* the warnings are known before the rows are drawn (each item's badge and its own warnings) */
on("renderEditor.before",function(){if(S.tab==="guide")G.gd.warns=gdWarnings()});
on("edGuide.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  var g4=innerOf(d,"gt4");if(g4)g4.insertAdjacentHTML("afterbegin",gdWarnsHtml());var gg=innerOf(d,"gd:g:ontour");if(gg)gg.insertAdjacentHTML("afterbegin",gdWarnsHtml(true));
  S.guidance.kinds.forEach(function(k){k.groups.forEach(function(g){if(g.pulled||g.id==="card")return;var f=innerOf(d,"gd:g:"+g.id);if(!f)return;
    f.insertAdjacentHTML("beforeend",'<div class="gdadd"><button class="ubtn sm" type="button" data-gdadd="'+g.id+'" aria-expanded="'+(S.ui.gdAddIn===g.id)+'">+ Add guidance</button>'+
      (S.ui.gdAddIn===g.id?'<div class="gdtpl" role="group" aria-label="Templates"><span>A new '+(g.launch?"page":"item")+' in '+esc(g.tour?"Games on the tour":g.name)+' from:</span>'+GD_TEMPLATES.filter(function(t){return!!t.page===!!g.launch}).map(function(t){return'<button class="ubtn sm" type="button" data-gdtpl="'+g.id+'|'+t.id+'">'+esc(t.name)+'</button>'}).join("")+'<button class="ubtn sm" type="button" data-gdadd="">Cancel</button></div>':"")+'</div>')})});
  return d.innerHTML});
/* ----- each new row's demo: the item on the phone with the new value, and a chip that says it; a new item's own rows use the round-1 demos ----- */
var GD_FORM_DEMO={prop:1,talkFor:1,beforeEnd:1,latePause:1,group:1,spoken:1,about:1,mustSee:1,form:1,goes:1,points:1,focus:1,dim:1,gesture:1,button:1,when:1,event:1,stillFor:1,afterPrev:1,screen:1,aud:1,visitIs:1,hasUsed:1,notUsed:1,notSolemn:1,lively:1,notBefore:1,notAfter:1,teaches:1,afterDismiss:1,expires:1,onScreen:1,onScreenS:1};
function gdFieldChip(it,f){var v=it[f],nm=function(t){return gdNameOf(t,v)},ctx=Object.assign(gdContext(),{demo:false,showing:null,remark:null,speaking:false,panel:null,touchAt:null,movedAt:null,cardAt:null,scrollAt:null,stillSince:-Infinity}),r=gdCheck(it,gdItems(),S.gdVisitor,ctx,gdNow()),now=r.ok?"now: it could show":"now: "+r.no;
  var w={group:"In "+((gdGroupOf(it)||{}).name||"?")+" ("+(gdKindOf(it)||{}).name+")",spoken:"Spoken: "+nm(GD_SPOKEN)+(G.gd.cur&&G.gd.cur.spoken?" (speaking now)":" (a bubble now)"),about:"About: "+nm(GD_ABOUT_OPTS),mustSee:"Must be on screen: "+(v?"Yes":"No"),
    form:"Form: "+nm(GD_FORMS),goes:"Mascot goes: "+nm(GD_GOES),points:"Points at it: "+(v?"Yes":"No"),focus:"Focus: "+nm(GD_FOCUS)+(gdFocusNow(it)!==v?" (shown as a Ring: for Essentials)":""),dim:"Spotlight: the rest at "+Math.round(+v*100)+" %",
    gesture:"The ghost finger: "+(gdGestureOf(it)?GD_GESTURE_WORDS[gdGestureOf(it)]:"Off"),button:"Button: "+nm(GD_BUTTONS),teaches:"Teaches: "+nm(GD_TEACH),afterDismiss:"After a dismissal: "+nm(GD_AFTER_DISMISS),expires:"Expires: "+nm(GD_EXPIRES),onScreen:"On screen for: "+nm(GD_ONSCREEN),onScreenS:"On screen for "+v+" s",prop:"Prop: "+nm(GD_PROPS),talkFor:+v>0?"After "+v+" s of the tour's narration":"At any step of a tour",beforeEnd:v?"Only before the tour's last step":"At any step, not only the last",latePause:+v>0?"Or rarely at a pause from "+gdClock(+v*1000):"Only after pokes"}[f];
  return(w||(gdReadBack(it).split(" · ")[2]||f))+(GD_FORM_DEMO[f]&&/^(when|event|stillFor|afterPrev|screen|aud|visitIs|hasUsed|notUsed|notSolemn|lively|notBefore|notAfter|talkFor|beforeEnd|latePause)$/.test(f)?" · "+now:"")}
function gdDemoField(id,f){var my=gdDemoBegin(),it=gdItemNow(id);if(!it){gdChip("Not in a group: it never shows");gdDemoEnd(my,3000);return}if(!it.on){gdChip("Off: the "+it.label+" never shows");gdDemoEnd(my,3500);return}
  var c=gdDemoShow(it,6500);gdChip(gdFieldChip(it,f));gdDemoEnd(my,Math.max(4200,c?c.until-c.at+400:0))}
/* a line of Says (a variant, an audience's, the returning visitor's) on the phone */
function gdDemoSay(id,k,who){var my=gdDemoBegin(),it=gdItemNow(id);if(!it)return;var slots=gdSlots(it),t=gdLine(Object.assign({},it,{says:k,saysAud:{},saysBack:""}),false,slots);
  if(!String(t).trim()){gdChip("No words yet: write them in the table above");gdDemoEnd(my,3000);return}
  var c=gdDemoShow(it,6500,{text:t});gdChip(who==="back"?"A returning visitor's line":who?gdNameOf(GD_AUD,who)+"'s line":"Says: this variant");gdDemoEnd(my,Math.max(4200,c?c.until-c.at+400:0))}
/* the visitor's Tips and the screen reader: on the phone, the visitor's own Settings, or a tip announced and in the dock */
function gdDemoPrefs(f){var my=gdDemoBegin(),P=gdPrefs();S.settingsOpen=true;renderPhone(false);var v=ph.querySelector(".vset"),g=v&&v.querySelector(".vtips");if(v&&g)v.scrollTop=Math.max(0,g.offsetTop-40);
  gdChip(f==="stay"?"Tips stay until I tap them: "+(P.stay?"On":"Off"):"Tips: "+(P.tips==="all"?"All":P.tips==="essentials"?"Essentials only":"Off (the ✦ still answers)"));gdDemoEnd(my,3600)}
function gdDemoSR(){var my=gdDemoBegin(),it=gdItemNow("inview")||gdItemNow("opening");if(!it)return;var keep=S.gdVisitor.visit.heard;S.gdVisitor.visit.heard=true;var c=gdDemoShow(it,6000);S.gdVisitor.visit.heard=keep;
  gdChip(S.ui.gdSR?"Screen reader on: announced politely, never spoken":"Screen reader off: "+(c&&c.spoken?"spoken, its word lit":"a bubble"));gdDemoEnd(my,4200)}
rowDemos({"gdVisitor.prefs.tips":{stop:"half",act:function(){gdDemoPrefs("tips")},ringAfter:500,ring:".vtips|.gdchip"},"gdVisitor.prefs.stay":{stop:"half",act:function(){gdDemoPrefs("stay")},ringAfter:500,ring:".vtips|.gdchip"},
  "ui.gdSR":{wall:true,act:gdDemoSR,ringAfter:600,ring:".gdbub|.gdchip"}});
on("rowDemo.spec",function(key){var m=/^guidance\.items\.([^.]+)\.(\w+)$/.exec(key);if(!m||GD_DEMOS[key]||GD_DEMOS5[key])return;var id=m[1],f=m[2],d=S.guidance.items[id];if(!d)return;
  if(d.form==="cue"){if(f==="gesture")return{stop:"full",act:function(){gdDemoCue(id,"on");gdChip(d.gesture?"Shows the gesture: the ghost finger, "+GD_GESTURE_WORDS[GD_GESTURE[d.about]]:"Shows the gesture Off: the cue alone")},ringAfter:900,ring:".gdfinger|.gdchip"};return}
  if(d.form==="walk"){if(f==="focus")return{act:function(){gdDemoWalk(id);gdChip("Focus: "+gdNameOf(GD_STEP_FOCUS,d.focus))},ringAfter:1100,ring:".gspot.on|.gfocus|.gdchip"};return}
  /* the item shows at once on the wall (no sheet to settle, the bubble drawn in the same frame): no wait before or after */
  if(GD_FORM_DEMO[f])return{wall:true,wait:0,act:function(){gdDemoField(id,f)},ringAfter:60,ring:".gdbub|.gdchip"};
  if(/^(on|importance|essential|doneWhen)$/.test(f))return{wall:true,wait:0,act:function(){gdDemoItem(id,f)},ringAfter:60,ring:".gdbub|.gdchip"};
  if(/^(shows|max|again)$/.test(f))return{wall:true,wait:0,act:function(){gdDemoTwice(id)},ringAfter:60,ring:".gdbub|.gdchip"};
  if(f==="along")return{wall:true,wait:0,act:function(){gdDemoAlong(id)},ringAfter:60,ring:".gdbub|.gdchip"};if(f==="duringTour")return{tour:"g1",act:function(){gdDemoTour(id)},ringAfter:60,ring:".gdchip"};
  if(f==="cardLine")return{wall:true,wait:0,act:function(){gdDemoLines(id)},ringAfter:60,ring:".gdbub|.gdchip"};if(f==="pokes")return{wall:true,wait:0,act:function(){gdDemoPokes(id)},ringAfter:60,ring:".gdchip|.gdbub"};
  if(f==="idleFor")return{wall:true,wait:0,act:function(){gdDemoIdle(id)},ringAfter:60,ring:".gdchip|.gdbub"}});

/* ----- round 7, Fun in the Editor: Says by date (Seasonal), each point's jokes (read here, written in Specific Marker › Guide script ›
   Jokes), Preview & Test › Today is; their demos (a line on the phone with its date, a point's joke turned to the middle of the wall).
   Unity: the POI Editor's GuidanceItem inspector (a list of {line, from, to, solemn}), the POI's own Guide script (its Jokes as CardStrings
   keys), a date field in the Preview panel ----- */
function gdTodayWords(){var md=gdTodayMD(),it=S.guidance.items.season,sv=it&&!it.deleted?gdSeasonOf(it,md):null,sd=gdSolemnDay(md);
  return"Today is "+gdDayWords(md)+(S.ui.gdToday&&S.ui.gdToday!==gdTodayISO()?" (picked; the real date is "+gdDayWords(gdTodayISO().slice(5))+")":" (the real date)")+": "+
    (sv?"Seasonal says “"+TXT(sv.says)+"”"+(sd?"; a Solemn day: no jokes today":""):"no seasonal line's window holds it, so Seasonal is quiet")+"."}
function gdSeasonTodayWords(it){var md=gdTodayMD(),sv=gdSeasonOf(it,md);return"Today ("+gdDayWords(md)+", Preview & Test › Today is): "+(sv?"“"+TXT(sv.says)+"”"+(sv.solemn?", a Solemn day: no jokes today":""):"no line's window holds it: Seasonal is quiet")+"."}
function gdSeasonHtml(it){var md=gdTodayMD(),p="guidance.items."+it.id+".seasons.";
  return'<div class="gdvars gdseasons">'+(it.seasons||[]).map(function(x,i){var on=gdInWindow(md,x.from,x.to),w=gdHasWords(x.says);
    return'<div class="gdvar gdseason'+(on?" on":"")+'"><div class="gdsline">'+gdTxIn(x.says)+(on?'<span class="badge new">TODAY</span>':"")+
      '<button class="ubtn sm" type="button" data-gdseason="'+it.id+'|'+i+'" title="Show this line on the phone">&#9656;</button></div>'+
      '<div class="gdsdate"><label>From <input class="u gdmd" data-sel="@'+p+i+'.from" value="'+esc(x.from)+'" maxlength="5" placeholder="MM-DD" aria-label="From (month-day)"></label><label>To <input class="u gdmd" data-sel="@'+p+i+'.to" value="'+esc(x.to)+'" maxlength="5" placeholder="MM-DD" aria-label="To (month-day)"></label>'+
      '<span class="gdsw2">'+esc(gdDayWords(x.from)+(x.to!==x.from?" – "+gdDayWords(x.to):""))+'</span>'+pseg(p+i+".solemn",["false","true"],["A light line","Solemn day: no jokes"])+'</div></div>'}).join("")+'</div>'+
    '<div class="gdforce">'+esc(gdSeasonTodayWords(it))+'</div>'}
function gdPtJokesHtml(it){var P=S.wallPois||[],w=P.filter(function(p){return gdPtJokes(p.id).length});
  return'<div class="gdvars">'+(w.length?w.map(function(p){var ks=gdPtJokes(p.id);return'<div class="gdvar"><span class="gdvn">'+esc(L(p.name))+'</span><span class="gdvt">'+ks.map(function(k){return esc(TXT(k))}).join(" · ")+'</span><button class="ubtn sm" type="button" data-gdjkshow="'+p.id+'|'+ks[0]+'" title="Show it on the wall">&#9656;</button></div>'}).join(""):'<div class="note">No point has a joke yet.</div>')+
    '<div class="note">'+esc((P.length-w.length)+" of the wall's "+P.length+" points have no joke: none shows there.")+'</div>'+
    '<button class="ubtn sm" type="button" data-gdjkgo="1">Edit in Specific Marker › Guide script › Jokes &#8250;</button></div>'}
/* Specific Marker › 3 · Guide script › Jokes: the point (the wall's points; in the app each has its own Specific Marker), its 0-3 lines in each language */
function gdJokesBlock(){var P=S.wallPois||[],p=P.filter(function(x){return x.id===S.ui.gdJkPoint})[0]||P[0];if(!p)return"";var pid=p.id,ks=S.ptJokes[pid]||[],n=function(x){return(S.ptJokes[x.id]||[]).length};
  return'<div class="gdjk"><div class="sub2">Jokes <span class="sub">· 0-3 a point, each in every language: said when the point settles in the middle, once a visit (Guide › The mascot speaks up › Fun › Play › {point}\'s joke)</span></div>'+
    row("Point",sel("@ui.gdJkPoint",P.map(function(x){return[x.id,L(x.name)+" ("+n(x)+(n(x)===1?" joke)":" jokes)")]}),pid),"Whose jokes these are: in the app each point has its own Specific Marker and Guide script; the Lab's one marker page lists the wall's points here. A change shows that point's joke on the wall.")+
    (ks.length?'<table class="tbl txt gdjkt"><thead><tr><th style="width:24%">Joke</th>'+langHead()+'<th style="width:96px"></th></tr></thead><tbody>'+ks.map(function(k,i){return'<tr><td>'+(i+1)+' · '+esc(L(p.name))+'</td>'+txInputs(k)+'<td><button class="ubtn sm" type="button" data-gdjkshow="'+pid+'|'+k+'" title="Show it on the wall">&#9656;</button> <button class="del" type="button" data-gdjkdel="'+pid+'|'+i+'">Remove</button></td></tr>'}).join("")+'</tbody></table>':'<div class="note">No joke for '+esc(L(p.name))+': none shows there (never a generic one in its place).</div>')+
    '<div class="btnrow">'+(ks.length<3?'<button class="ubtn sm" type="button" data-gdjkadd="'+pid+'">+ Add a joke</button>':'<span class="uses">3 jokes: the most</span>')+'</div>'+
    '<div class="note">Short and kind, about what is painted there: one line, two at most. A Solemn point never jokes (Fun is off there).</div></div>'}
on("edMarker.html",function(h){var d=document.createElement("div");d.innerHTML=h;var f=innerOf(d,"m3");if(!f)return h;var sr=f.querySelector('[data-set="@m.solemn"]'),at=sr&&sr.closest(".row"),b=gdJokesBlock();
  if(at)at.insertAdjacentHTML("afterend",b);else f.insertAdjacentHTML("afterbegin",b);return d.innerHTML});
/* a point's joke on the wall now (a demo: no memory), the point turned to the middle */
function gdJkShow(pid,k){var my=gdDemoBegin();gdOnWallNow();var ins=gdItems().filter(function(x){return x.id==="ptjoke:"+pid})[0],t=k?TXT(k):ins?gdLine(ins,false,gdSlots(ins)):"",p=(S.wallPois||[]).filter(function(x){return x.id===pid})[0],nm=p?L(p.name):pid;
  if(!ins||!String(t).trim()){gdChip(nm+": no joke, so none shows (never a generic one)");gdDemoEnd(my,3200);return null}
  var c=gdDemoShow(ins,6500,{text:t,point:pid});gdChip(nm+"'s joke: when it settles in the middle, once a visit");gdDemoEnd(my,Math.max(4200,c?c.until-c.at+400:0));return c}
function gdDemoSeason(id,i){var my=gdDemoBegin(),it=gdItemNow(id),x=it&&it.seasons&&it.seasons[i];if(!x)return;
  if(!gdHasWords(x.says)){gdChip("No words yet: write them in the table above");gdDemoEnd(my,3000);return}var on=gdInWindow(gdTodayMD(),x.from,x.to);
  var c=gdDemoShow(it,6500,{text:TXT(x.says)});gdChip(gdDayWords(x.from)+(x.to!==x.from?" – "+gdDayWords(x.to):"")+(x.solemn?" · a Solemn day: no jokes":"")+(on?" · today":" · not today ("+gdDayWords(gdTodayMD())+")"));gdDemoEnd(my,Math.max(4200,c?c.until-c.at+400:0))}
function gdDemoToday(){var my=gdDemoBegin(),md=gdTodayMD(),it=gdItemNow("season"),sv=it?gdSeasonOf(it,md):null,sd=gdSolemnDay(md);
  if(sv&&it.on){var c=gdDemoShow(it,6500);gdChip("Today is "+gdDayWords(md)+(sd?": a Solemn day, no jokes":": its seasonal line"));gdDemoEnd(my,Math.max(4200,c?c.until-c.at+400:0));return}
  gdChip("Today is "+gdDayWords(md)+": no seasonal line");gdDemoEnd(my,3200)}
rowDemos({"ui.gdToday":{wall:true,act:gdDemoToday,ringAfter:300,ring:".gdbub|.gdchip"},"ui.gdJkPoint":{wall:true,act:function(){gdJkShow(S.ui.gdJkPoint)},ringAfter:300,ring:".gdbub|.gdchip"}});
on("rowDemo.spec",function(key){var m=/^guidance\.items\.([^.]+)\.seasons\.(\d+)\.(from|to|solemn)$/.exec(key);if(!m)return;var id=m[1],i=+m[2];return{wall:true,wait:0,act:function(){gdDemoSeason(id,i)},ringAfter:60,ring:".gdbub|.gdchip"}});
ed.addEventListener("click",function(e){var t=e.target,q;
  var sn=t.closest("[data-gdseason]");if(sn){e.stopPropagation();q=sn.dataset.gdseason.split("|");gdDemoSeason(q[0],+q[1]);return}
  var js=t.closest("[data-gdjkshow]");if(js){e.stopPropagation();q=js.dataset.gdjkshow.split("|");gdJkShow(q[0],q[1]);return}
  var ja=t.closest("[data-gdjkadd]");if(ja){e.stopPropagation();var pid=ja.dataset.gdjkadd,l=S.ptJokes[pid]||(S.ptJokes[pid]=[]),p=(S.wallPois||[]).filter(function(x){return x.id===pid})[0];if(l.length>=3)return;
    l.push(gdNewWord("gd:i:ptjoke","Joke · "+(p?L(p.name):pid),"",""));renderEditor();return}
  var jd=t.closest("[data-gdjkdel]");if(jd){e.stopPropagation();q=jd.dataset.gdjkdel.split("|");(S.ptJokes[q[0]]||[]).splice(+q[1],1);renderAll(true);return}
  var jg=t.closest("[data-gdjkgo]");if(jg){e.stopPropagation();S.tab="marker";S.op.m3=true;renderEditor();var b=ed.querySelector(".gdjk");if(b)b.scrollIntoView({block:"center"});return}},true);
/* ----- round 8, the Editor (_3.8 section 7's 5 · Sequences; section 6): Sequences holds the In order sequences, How to use the card (moved
   here from App, its rows as they were) and First pages: its On, the run now and its time (under 30 s), its words (Skip, the progress),
   each page a row (On, the page, how long, ▸ Show now) that opens its form (Identity, Words, Show: the Page form, Azu, the language chip,
   a custom page's buttons; When, fixed; the read-back), + Add guidance with the Page template, Duplicate and Delete for a page of its own,
   drag ≡ to reorder (Hello first, Straight to the wall last: the drop line says why while a drag would break that, and the drop is
   undone with the reason in the field). 6 · Preview & Test › Start the app (a first run / a later launch) and the visitor's first pages;
   the preview bar's Start the app and Later launch. Each row's demo: a page row shows that page on the phone with its value; the group's
   On the run's first page; Start the app plays the run. Unity: the POI Editor's Guide tab, Sequences drawn from GuidanceCatalog (a
   ReorderableList whose ends are pinned), Start the app a Play-mode launch with the GuidanceMemory file cleared (a first run) or kept ----- */
COMP["gd:seq"]=["Guide","The mascot speaks up › Sequences"];FOCUS["gd:seq"]=["card","#wall","closed"];COMP["gd:g:pages"]=["Guide","The mascot speaks up › Sequences › First pages"];FOCUS["gd:g:pages"]=["card","#wall","closed"];
gdGroupById("pages").items.forEach(function(id){COMP["gd:i:"+id]=["Guide","The mascot speaks up › Sequences › First pages › "+S.guidance.items[id].label];FOCUS["gd:i:"+id]=["card","#wall","closed"]});
["gd_pg_hello_t","gd_pg_hello","gd_pg_go","gd_pg_how","gd_pg_lang","gd_pg_cam_t","gd_pg_cam","gd_pg_cam_allow","gd_pg_cam_usage","gd_pg_off_t","gd_pg_off","gd_pg_settings","gd_pg_retry","gd_pg_skip","gd_pg_dots"].forEach(function(k){TX_DOM[k]="shown on the first pages (Start the app)"});
TX_DOM.gd_pg_wall="shown in the Opening message right after the first pages";
G.gd.pgRule="";S.ui.gdLaunch="first";
/* How to use the card's foldout (round 4's rows, unchanged; drawn in Sequences since round 8) */
function gdHowtoFo(g){var gp=gdGroupPath(g.id),items=g.items.map(function(id){return S.guidance.items[id]}).filter(Boolean);
  return fo("gd:g:"+g.id,esc(g.name)+' <span class="sub">· a walkthrough, in order, pulled only</span>',null,
    '<div class="note">Pulled only: never scheduled, never capped, never by itself. It opens from the visitor\'s Settings and from the ✦ menu (How does this work?). On a card it walks the card, step by step, through the guide\'s tour; on the wall one bubble says how the wall works and offers to open a card. A step seen (its line heard) retires its cue in The card (cues).</div>'+
    row("On",pseg(gp+"on",["true","false"],["On","Off"]),"Off: the walkthrough is gone, and with it its entries in Settings and the ✦ menu.")+
    row("In Settings",pseg(gp+"inSettings",["true","false"],["Shown","Hidden"]),"A button at the foot of the visitor's Settings, “"+TXT("settings_how")+"” (its word in Card Controls › Settings panel). It closes the panel and walks the card.")+
    row("In the ✦ menu",pseg(gp+"inMenu",["true","false"],["Shown","Hidden"]),"An item of the ✦ menu, “"+TXT("gd_how")+"”, on the card and on the wall: on the card it walks the card; on the wall one bubble says how the wall works, with a chip that opens a card.")+
    '<div class="sub2">Its words · the ✦ menu\'s item and the wall\'s bubble (Languages &amp; Terms, owned here)</div>'+txPanel("gd:g:"+g.id)+
    '<div class="note">Its steps, in this order (a row: On, the step, ▸ Show now walks the card from it; open it for its words):</div>'+items.map(gdItemRow).join(""),3)}
/* 5 · Sequences: each In order sequence of the catalog, in its order */
function gdSeqFo(){var inner='<div class="note">The In order sequences: How to use the card (a walkthrough, pulled from Settings and the ✦ menu) and the first pages (the app\'s first run). Each step or page is an item; drag ≡ to reorder: the order is the sequence.</div>';
  S.guidance.kinds.forEach(function(k){k.groups.forEach(function(g){if(g.seq)inner+=g.launch?gdPagesFo(g):gdHowtoFo(g)})});
  return fo("gd:seq",'Sequences <span class="sub">· in order: How to use the card, the first pages</span>',null,inner,2)}
/* a first run as it would go now (the camera not yet allowed: a new install) and its time */
function gdPagesRunWords(){var g=gdGroupById("pages"),pl=gdPagesPlan(),sec=gdPagesSecs(pl),w=gdPageOf("wall");
  if(!g||g.on===false)return"First pages Off: a first run opens on the wall (the camera's own request first, while it is not allowed). Who's exploring: "+gdNameOf(AUD_WHERE,S.aud.where)+".";
  return"A first run now: "+pl.map(function(x){return x.label}).concat(["the wall"+(w&&w.on?" (its short line)":"")]).join(" → ")+" · about "+sec+" s"+(sec>GD_PAGE_RUN_MAX?": over "+GD_PAGE_RUN_MAX+" s, shorten a page or turn one Off":", under "+GD_PAGE_RUN_MAX+" s")+
    ". Who's exploring: "+(S.aud.where==="start"?"asked here":S.aud.where==="card"?"asked on the first card, not here":"never asked (Explorer)")+" (Detail Card › 6 · Audiences › Where to ask)."}
function gdPagesFo(g){var gp=gdGroupPath(g.id),its=g.items.map(function(id){return S.guidance.items[id]}).filter(function(x){return x&&!x.deleted});
  var body='<div class="note">'+esc("In order, at the app's start, on a first run only; the scheduler is quiet while it runs. Every page can be skipped: Skip skips the rest of the intro (the camera's own request still comes: the app never goes on to the wall without the camera). Later launches: no pages, the Opening message's Welcome back (Overall › Remember across visits Off: every launch is a first). No carousel of benefits, no account, no tutorial of the card before a card exists.")+'</div>'+
    row("On",pseg(gp+"on",["true","false"],["On","Off"]),"Off: no pages; a first run opens on the wall (the camera's own request still comes first while it is not allowed).")+
    '<div class="note gdpgrun'+(gdPagesSecs(gdPagesPlan())>GD_PAGE_RUN_MAX?" warn":"")+'" id="gdpgrun">'+esc(gdPagesRunWords())+'</div>'+
    (G.gd.pgRule?'<div class="note warn gdpgrule" role="status">&#9888; '+esc(G.gd.pgRule)+'. The drag was undone.</div>':"")+
    '<div class="sub2">Its words · every page\'s Skip and the progress (Languages &amp; Terms, owned here)</div>'+txPanel("gd:g:"+g.id)+
    '<div class="note">Its pages, in this order (a row: On, the page, ▸ Show now; open it for its form). Drag ≡ to reorder: Hello stays first, Straight to the wall last.</div>'+its.map(gdItemRow).join("");
  return fo("gd:g:"+g.id,esc(g.name)+' <span class="sub">· the app\'s first run: Hello, Who\'s exploring, the camera, then the wall</span>',null,body,3)}
/* a custom page's buttons (up to 2): what each does; its words are in the table above */
function gdPageBtnsHtml(it){var p="guidance.items."+it.id+".",bs=it.buttons||[];
  return bs.map(function(b,i){return row("Button "+(i+1),pseg(p+"buttons."+i+".act",GD_PAGE_ACTS.map(function(x){return x[0]}),["Next page","Show me how it works"])+(i>0?'<button class="del" type="button" data-gdpgbtndel="'+it.id+'|'+i+'">Remove</button>':""),
    "What a tap does: the next page, or the next page and How to use the card's bubble on the wall after the pages. Its words are in the table above. The first button is filled, the second outlined; Skip is always there.")}).join("")+
    (bs.length<2?'<button class="ubtn sm" type="button" data-gdpgbtnadd="'+it.id+'">+ A second button</button>':'<span class="uses">2 buttons: the most (and Skip)</span>')}
/* a page's form: Identity (Label, its place), Says (its title, line and buttons), Show (the Page form, Azu, the language chip, a custom
   page's buttons), When (fixed; Who's exploring's Where to ask); its read-back below */
function gdPageRow(it){var p="guidance.items."+it.id+".",scr=gdPagesPlan(),i=scr.indexOf(it),wall=it.page==="wall";
  var sum='<span class="gdi">'+gdDragH(it)+'<span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b><span class="badge gdpgb">'+(wall?"THE WALL":"PAGE")+'</span>'+
    '<span class="gdhow">'+esc(wall?(it.on?"its short line opens the wall":"Off: the Opening message's own line"):!it.on?"Off: skipped":it.page==="aud"&&S.aud.where!=="start"?"not asked here (Where to ask)":"page "+(i+1)+" of "+scr.length+" · about "+gdPageSecs(it)+" s")+'</span>'+
    '<button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Show it on the phone now">&#9656; Show now</button></span>';
  return{sum:sum,groups:[
    {k:"identity",t:"Identity",s:it.label+" · "+(it.page==="hello"?"first":wall?"last":"between Hello and Straight to the wall"),html:row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the page's name in these lists, the read-back, the Log and the scenes. The visitor never sees it.")+
      '<div class="note gdfix">'+esc("Its place: "+(it.page==="hello"?"first (Hello stays first: the visitor's first screen is the guide's hello)":wall?"last (Straight to the wall stays last: the run ends on the wall)":"between Hello and Straight to the wall: drag ≡ to move it")+".")+'</div>'},
    {k:"says",t:"Says",s:it.page==="aud"?"Who's exploring's own screen":"its title, line and buttons, "+langCodes().map(function(l){return l.toUpperCase()}).join(" | "),html:
      (it.page==="aud"?'<div class="note">Its words are Who\'s exploring\'s own (Detail Card › 6 · Audiences › Who\'s exploring): the same screen, one home, no copy.</div>':txPanel("gd:i:"+it.id))},
    {k:"show",t:"Show",s:wall?"the wall itself":"a page"+(it.page==="hello"||it.page==="custom"?" · The mascot: "+gdLow(gdNameOf(GD_PAGE_AZU,it.azu))+(it.langChip?" · the language chip":""):""),html:
      '<div class="note gdfix">'+esc((wall?"Form: the wall itself (no screen). ":"Form: Page (a title, a line, up to 2 buttons and Skip; fixed for the first pages). ")+gdPageLook(it)+".")+'</div>'+
      (it.page==="hello"||it.page==="custom"?row("The mascot",pseg(p+"azu",GD_PAGE_AZU.map(function(x){return x[0]}),GD_PAGE_AZU.map(function(x){return x[1]})),"How the mascot comes on the page: it forms from a tile and waves (the Rive file's states in the app: form, then wave), it simply appears, or it is not shown. Reduce Motion: it appears (no tile, no wave). Mascot Off (Guide › The mascot › Character): no mascot.")+
        row("Language chip",pseg(p+"langChip",["true","false"],["Shown","Hidden"]),"EN / PT at the top of the page: a tap switches the app's language at once (the same as the visitor's Settings › Language)."):"")+
      (it.page==="custom"?gdPageBtnsHtml(it):"")+
      (it.page==="hello"?'<div class="note gdfix">'+esc("Its buttons: “"+TXT("gd_pg_go")+"” (the next page) and “"+TXT("gd_pg_how")+"” (the next page, and after the pages How to use the card's bubble on the wall, which says the visit's first words). Fixed for Hello.")+'</div>':"")+
      (it.page==="camera"?'<div class="note gdfix">'+esc("After “"+TXT("gd_pg_cam_allow")+"” the phone's own request comes (a stand-in in the Lab: Allow / Don't Allow), with this page's request line. Refused: the page says how to allow it, with Open Settings (the phone's Settings for the app) and Try again (it asks again); no Skip there: the app never goes on to the wall without the camera. Skip on the page itself goes straight to the request. Unity: Application.RequestUserAuthorization(UserAuthorization.WebCam) on iOS, Permission.RequestUserPermission(Permission.Camera) on Android.")+'</div>':"")},
    {k:"when",t:"When",s:"at the app's start, a first run only",html:'<div class="note gdfix">'+esc("At the app's start, on a first run only: "+gdPageWhen(it)+". Later launches: no pages, the Opening message's Welcome back. Overall › Remember across visits Off: every launch is a first.")+'</div>'+
      (it.page==="aud"?row("Where to ask",'<button class="ubtn sm" type="button" data-gdwhere="1">'+esc(gdNameOf(AUD_WHERE,S.aud.where))+' · open it in 6 · Audiences &#8250;</button>',"Who's exploring is asked here only when Detail Card › 6 · Audiences › Where to ask is At the app's start; On the first card, the card's own screen asks there; Never: neither asks."):"")}],
   foot:'<div class="note gdrb">'+esc(gdReadBack(it))+'</div>'+
    (it.page==="custom"?'<div class="btnrow gdacts"><button class="ubtn sm" type="button" data-gddup="'+it.id+'">Duplicate</button><button class="ubtn sm" type="button" data-gddel="'+it.id+'">Delete</button></div>':"")}}
/* the visitor's first pages, in words (Preview & Test › The visitor) */
function gdPagesSeenWords(){var P=G.gd.pages,n=gdPageNow(),pg=S.gdVisitor.pages;
  if(P&&n)return"The first pages: "+(P.preview?"a page shown by the Editor":"running now")+", "+(n.it?n.it.label:"?")+" ("+(P.i+1)+" of "+P.run.length+(P.cam?", the camera "+P.cam:"")+")";
  return pg?"The first pages: seen at visit "+pg.visit+(pg.earlier?" (an earlier launch, before the Lab opened)":"")+(pg.skipped?", skipped":"")+(pg.how?", with Show me how it works":"")+"; the camera "+G.gd.camPerm:"The first pages: not seen yet (Start the app shows them); the camera "+G.gd.camPerm}
/* while a drag would break the order's ends, the drop line says why */
function gdDragWhy(d){var it=S.guidance.items[d.id],g=it&&gdGroupOf(it);if(!g||!g.launch)return"";var ids=d.rows.map(function(r){return r.dataset.fo.slice(5)}),mv=ids.splice(d.from,1)[0];ids.splice(d.to,0,mv);return gdPagesRule(ids)}
/* ----- the demos: a page on the phone with its value; the group's On: the run's first page; Start the app: the run ----- */
function gdDemoPagesOn(){var my=gdDemoBegin(),g=gdGroupById("pages");
  if(g.on===false){gdOnWallNow();gdChip("First pages Off: a first run opens on the wall (the camera's request first)");gdDemoEnd(my,3200);return}
  var f=gdPagesPlan()[0];if(f)gdPagesPreview(f.id);gdChip("First pages On: a first run starts here");gdDemoEnd(my,2800)}
function gdDemoPage(id,f){var my=gdDemoBegin(),it=S.guidance.items[id];if(!it)return;
  if(it.page==="wall"){gdOnWallNow();var op=gdItemNow("opening");if(op)gdStart(op,"the Editor's demo",{pull:true,text:it.on&&gdHasWords(it.says)?TXT(it.says):gdLine(op,false,{}),ms:6000});
    gdChip(it.on?"Straight to the wall: the Opening message's short line, then Tap a marker":"Straight to the wall Off: the Opening message says its own line");gdDemoEnd(my,4200);return}
  if(!it.on){gdOnWallNow();gdChip("Off: the first run skips "+it.label);gdDemoEnd(my,3000);return}
  gdPagesPreview(id);var b=/^buttons\.(\d)/.exec(f||""),bt=b&&(it.buttons||[])[+b[1]];
  var ch=f==="azu"?"The mascot: "+gdNameOf(GD_PAGE_AZU,it.azu)+(S.reduce&&it.azu==="form"?" (Reduce Motion: it appears)":!gdAzuOn()?" (Mascot Off: na mascote)":""):f==="langChip"?"The language chip: "+(it.langChip?"shown":"hidden"):bt?"Button "+(+b[1]+1)+": "+gdNameOf(GD_PAGE_ACTS,bt.act).split(" (")[0]:
    it.page==="aud"&&S.aud.where!=="start"?"Who's exploring: not asked here now (Where to ask: "+gdNameOf(AUD_WHERE,S.aud.where)+")":it.label+" · "+(G.gd.pages?"page "+(G.gd.pages.i+1)+" of "+G.gd.pages.run.length:"");
  gdChip(ch);gdDemoEnd(my,3000)}
on("rowDemo.spec",function(key){var m=/^guidance\.items\.([^.]+)\.(on|azu|langChip|buttons\.\d+\.act)$/.exec(key);if(!m)return;var d=S.guidance.items[m[1]];if(!d||d.form!=="page")return;
  var id=m[1],f=m[2];return{wall:true,wait:0,act:function(){gdDemoPage(id,f)},ringAfter:400,ring:d.page==="wall"?".gdbub|.gdchip":".gdpages|.gdchip"}});
rowDemos({"ui.gdLaunch":{wall:true,wait:0,act:function(){var k=S.ui.gdLaunch||"first";gdLaunch(k);if(!G.gd.pages){gdChip(k==="later"?"A later launch: no pages; the Opening message says Welcome back":"No page to show: the wall");setTimeout(function(){if(!G.gd.pages)gdChip(null)},3500)}},ringAfter:450,ring:".gdpages|.gdbub|.gdchip"}});
/* ----- 6 · Preview & Test › Start the app, and the visitor's first pages ----- */
on("edGuide.html",function(h){var d=document.createElement("div");d.innerHTML=h;var v=d.querySelector('[data-fo="gt0"] [data-sel="@ui.gdToday"]'),r=v&&v.closest(".row");if(!r)return h;
  r.insertAdjacentHTML("beforebegin",row("Start the app",pseg("ui.gdLaunch",["first","later"],["▸ A first run","▸ A later launch"]),"Plays the app's start on the phone, as the preview bar's Start the app and Later launch do. A first run: the visitor reset to a first visit (as Reset the visitor), the audience not yet asked, the camera not yet allowed: the first pages from Hello, then the wall. A later launch: the same visitor opens the app again: no pages, the Opening message's Welcome back (with Remember across visits Off every launch is a first: the pages again). A press plays it again.")+
    '<div class="note gdwhy" id="gdpgseen">'+esc(gdPagesSeenWords())+'</div>');return d.innerHTML});
on("gdLog.after",function(){var n=ed.querySelector("#gdpgseen");if(n)n.textContent=gdPagesSeenWords()});
/* ----- the preview bar: Start the app (a first run) and Later launch ----- */
on("ptool.html",function(h){return h+'<button type="button" class="gdlaunch" data-gdlaunch="first" title="'+TXH("pbar_tip_start")+'">&#9656; '+TXH("pbar_start")+'</button><button type="button" class="gdlaunch" data-gdlaunch="later" title="'+TXH("pbar_tip_later")+'">'+TXH("pbar_later")+'</button>'});
document.addEventListener("click",function(e){var b=e.target.closest&&e.target.closest("[data-gdlaunch]");if(!b)return;e.stopPropagation();e.preventDefault();G.demoSeq=(G.demoSeq||0)+1;gdLaunch(b.dataset.gdlaunch)},true);
/* ----- the page form's own buttons: a custom page's second button, Where to ask's link ----- */
ed.addEventListener("click",function(e){var t=e.target,q;
  var ba=t.closest("[data-gdpgbtnadd]");if(ba){e.stopPropagation();var it=S.guidance.items[ba.dataset.gdpgbtnadd];if(!it||(it.buttons||[]).length>=2)return;it.buttons=(it.buttons||[]).concat([{word:gdNewWord("gd:i:"+it.id,"Button "+((it.buttons||[]).length+1),"Show me how it works","Mostre-me como funciona"),act:"how"}]);renderAll(true);showRowDemo("guidance.items."+it.id+".buttons."+(it.buttons.length-1)+".act");return}
  var bd=t.closest("[data-gdpgbtndel]");if(bd){e.stopPropagation();q=bd.dataset.gdpgbtndel.split("|");var c=S.guidance.items[q[0]];if(c&&c.buttons)c.buttons.splice(+q[1],1);renderAll(true);return}
  var wh=t.closest("[data-gdwhere]");if(wh){e.stopPropagation();S.tab="card";S.op.s8=true;S.op.s8onb=true;renderEditor();var f=ed.querySelector('[data-fo="s8onb"]');if(f)f.scrollIntoView({block:"center"});return}},true);
/* ----- round 9 (_3.8 section 3.4), the pull in the Editor. Overall › The visitor's controls › Things to try (In the ✦ menu: Shown /
   Hidden): it lives in Overall, not in App's kind row, because the study puts the visitor's controls there (section 7's tree) and because
   it is pull: App's kind row holds the push (its On, cap, gap and weight), and Things to try lists rows from every App group (The start,
   The wall, the card's cues, The guide). Each App item that teaches a feature: In Things to try (Yes / No; Yes when it Teaches), its row's
   and its answer's words, and what ticks it. App › The guide (pulled only): Pause the guide. 6 · Preview & Test › The visitor: "Things to
   try: 3 of 8 tried". Each row's demo: the sheet on the phone with the new value (the ✦ menu with or without it; the sheet with the row in
   or out; Show me playing). Unity: the POI Editor's Guidance section (a Toggle per item, a PopupField for In the ✦ menu) ----- */
COMP["gd:g:guide"]=["Guide","The mascot speaks up › The guide"];FOCUS["gd:g:guide"]=["card",".gbtn-wrap","full"];COMP["gd:i:pause"]=["Guide","The mascot speaks up › The guide › Pause the guide"];FOCUS["gd:i:pause"]=["card",".gbtn-wrap","full"];
TX_DOM.gd_try="shown in the ✦ menu, as Things to try's title and in Settings › The guide";TX_DOM.gd_try_count="shown at the top of Things to try";TX_DOM.gd_try_all="shown at the top of Things to try once every row is done";
TX_DOM.gd_try_done="read by a screen reader on a done row of Things to try";TX_DOM.gd_try_todo="read by a screen reader on a row of Things to try not yet done";
TX_DOM.gd_fewer_done="shown in the toast after Fewer like this";TX_DOM.gd_fewer_try="shown in the toast after Fewer like this on an App tip";
Object.keys(GD_TRY_KEYS).forEach(function(f){TX_DOM["gd_try_"+f]="shown as a row of Things to try";TX_DOM["gd_tryhow_"+f]="shown in Ask the mascot's answer to how do I…"});
/* an item's Things to try, in its form: In Things to try, its words (where the form has no words panel of its own), what ticks it (bare: no
   heading of its own, the group's header says it) */
function gdTryRows(it,words,bare){if(it.kind!=="app")return"";var p="guidance.items."+it.id+".",H=bare?"":'<div class="sub2 gdfg">Things to try <span class="sub">· the visitor\'s checklist (the ✦ menu, Settings › The guide)</span></div>';
  if(!it.teaches)return it.form==="cue"?"":'<div class="note gdtrynone">Things to try: no row (it teaches nothing; choose what it Teaches in How much).</div>';
  var first=gdTeachers().filter(function(x){return x.teaches===it.teaches})[0],other=first&&first.id!==it.id?first:null;
  return H+row("In Things to try",pseg(p+"inTry",["true","false"],["Yes","No"]),"Yes (the default when it Teaches): a row of Things to try, “"+gdTryWord(it)+"”, ticked once the visitor has done it, with Show me (the gesture on its target, the visitor taken there first). No: no row. The visitor's own pull: the item's On (its push), its group's and kind's, and the visitor's Tips never hide it. One row a feature: the first item In Things to try that teaches it.")+
    (words?'<div class="sub2">Its row and its answer · Languages &amp; Terms, owned here</div>'+txPanel("gd:i:"+it.id):"")+
    '<div class="note gdtryrb">'+esc("Its row: “"+gdTryWord(it)+"” · Ask the mascot's answer to how do I…: “"+gdTryHowLine(it)+"” · ticked when "+(GD_UNTIL[it.teaches]||it.teaches)+(it.doneWhen&&it.doneWhen!==it.teaches?" (or once it is done: "+(GD_UNTIL[it.doneWhen]||it.doneWhen)+")":"")+
      (other?" · another item teaches it first: the row is the "+other.label+"'s":"")+" · now "+(gdTryAt(it.id)>=0?"row "+(gdTryAt(it.id)+1)+" of "+gdTryDefs().length:"not a row")+".")+'</div>'}
/* an item's place among Things to try's rows (-1: not a row) */
function gdTryAt(id){return gdTryDefs().map(function(x){return x.id}).indexOf(id)}
/* a Try item's form (App › The guide, pulled only): its name, ▸ Show me; opened: Identity (Label), Says (its row and Ask Azu's answer),
   Things to try; what it is about and the read-back below */
function gdTryRow(it){var p="guidance.items."+it.id+".";
  var sum='<span class="gdi">'+gdDragH(it)+'<b>'+esc(it.label)+'</b><span class="badge">PULL</span><span class="gdhow">'+esc(gdHowOften(it))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Show me on the phone now">&#9656; Show me</button></span>';
  var tr=gdTryRows(it,false,true);
  return{sum:sum,groups:[
    {k:"identity",t:"Identity",s:it.label+" · pulled only",html:row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: its name in these lists, the read-back and the Log.")},
    {k:"says",t:"Says",s:"its row in Things to try and Ask the mascot's answer",html:txPanel("gd:i:"+it.id)},
    tr?{k:"try",t:"Things to try",s:gdTrySum(it),html:tr}:null],
   foot:'<div class="note gdfix">'+esc("About: "+(GD_ABOUT[it.about]||it.about).replace(/^when /,"")+" · Show me: "+(GD_GESTURE_WORDS[GD_GESTURE[it.about]]||"a tap")+" on it (a tour starts at Mini when none runs) · Teaches: "+gdNameOf(GD_TEACH,it.teaches)+" · Done when "+(GD_UNTIL[it.doneWhen]||"never")+". Fixed for this item; pulled only, so no bubble, no When and no limits.")+'</div>'+
    '<div class="note gdrb">'+esc(gdReadBack(it))+'</div>'}}
/* a pulled group that is not a sequence (App › The guide): its note and its items */
function gdPullFo(g){var items=g.items.map(function(id){return S.guidance.items[id]}).filter(function(x){return x&&!x.deleted});
  return fo("gd:g:"+g.id,esc(g.name)+' <span class="sub">· pulled only: Things to try and Ask the mascot\'s how do I…</span>',null,
    '<div class="note">Pulled only (round 9): each item is a row of Things to try and an answer of Ask the mascot\'s how do I…, never on its own (no bubble, never scheduled, never capped). Its push (a tip between two steps of a tour) is not built: it waits for the developer\'s call.</div>'+items.map(gdItemRow).join(""),3)}
/* Overall › The visitor's controls › Things to try, and what it holds now */
function gdTryNowWords(){var r=gdTryDefs();return"In the ✦ menu: "+(S.guidance.tryMenu!==false?"Shown":"Hidden")+" · Settings › The guide › Things to try ›: always · its "+r.length+" rows, in the Editor's order: "+r.map(function(it){return gdTryWord(it)+" ("+it.label+")"}).join(", ")+"."}
function gdTrySeenWords(){var c=gdTryCount(),r=gdTryDefs().filter(function(it){return gdTryDone(it)});return"Things to try: "+c.done+" of "+c.n+" tried"+(r.length?" ("+r.map(gdTryWord).join(", ")+")":"")+(G.gd.tryOpen?" · the sheet is open":"")}
on("edGuide.html",function(h){var d=document.createElement("div");d.innerHTML=h;var ov=d.querySelector('[data-fo="gd:overall"] > .inner');if(!ov)return h;
  var lp=[].filter.call(ov.querySelectorAll(":scope > .sub2"),function(x){return/The long press/.test(x.textContent)})[0];
  var w='<div class="sub2 gdvc">The visitor\'s controls <span class="sub">· what the visitor asks for: never scheduled, never capped</span></div>'+
    row("Things to try",pseg("guidance.tryMenu",["true","false"],["Shown","Hidden"]),"In the ✦ menu. Shown: the ✦ menu has Things to try, a short sheet of the App items that teach a feature (each item's In Things to try), in this order, each ticked once the visitor has done it (its Done when, or the feature used), each with Show me. Hidden: not in the menu; Settings › The guide › Things to try › still opens it (pull help stays findable). The visitor's own pull: never scheduled, never capped; the visitor's Tips Off silences the pushed tips, never this.")+
    '<div class="note gdtrynow" id="gdtrynow">'+esc(gdTryNowWords())+'</div>';
  if(lp)lp.insertAdjacentHTML("beforebegin",w);else ov.insertAdjacentHTML("beforeend",w);
  var pg=d.querySelector("#gdvisitor2");if(pg)pg.insertAdjacentHTML("afterend",'<div class="note gdwhy" id="gdtryseen">'+esc(gdTrySeenWords())+'</div>');return d.innerHTML});
on("gdLog.after",function(){var n=ed.querySelector("#gdtryseen");if(n)n.textContent=gdTrySeenWords()});
/* the demos: the ✦ menu with or without it, then the sheet; the sheet with a row in or out; Show me on the phone */
function gdDemoTryClose(my,ms){setTimeout(function(){if(G.gd.tryOwner===my)gdTryClose()},ms)}
function gdDemoTryMenu(){var my=gdDemoBegin();G.fab.menu=true;fabLayer();var on=S.guidance.tryMenu!==false&&gdTryDefs().length>0;
  if(!on){gdChip(S.guidance.tryMenu===false?"Hidden from the ✦ menu: Settings › The guide › Things to try › still opens it":"No row to try: no App item teaches a feature");gdDemoEnd(my,3600);return}
  gdChip("In the ✦ menu: Things to try");G.gd.demoT=setTimeout(function(){if(!gdDemoLive(my))return;gdTryOpen("the Editor's demo",true);var c=gdTryCount();gdChip("Things to try: "+c.done+" of "+c.n+" tried");gdDemoEnd(my,3400);gdDemoTryClose(my,3600)},1200)}
function gdDemoTryRow(id){var my=gdDemoBegin(),d=S.guidance.items[id];if(!d)return;gdTryOpen("the Editor's demo",true);var rows=gdTryDefs(),i=gdTryAt(id),first=gdTeachers().filter(function(x){return x.teaches===d.teaches})[0];
  gdChip(i>=0?"In Things to try: row "+(i+1)+" of "+rows.length+", “"+gdTryWord(d)+"”":"Out of Things to try: no row for the "+d.label+(gdTryIn(d)&&first&&first.id!==d.id?" (the "+first.label+" teaches it first)":""));gdDemoEnd(my,3600);gdDemoTryClose(my,3800)}
function gdDemoTryShow(id){var my=gdDemoBegin(),it=gdTryItem(id);if(!it)return;gdTryShow(id,"the Editor's demo");gdChip("Show me: "+gdTryWord(it)+(G.gd.tryShow?" ("+G.gd.tryShow.where+")":""));gdDemoEnd(my,4400)}
rowDemos({"guidance.tryMenu":{wall:true,wait:0,act:gdDemoTryMenu,ringAfter:300,ring:'[data-gbgo="try"]|.gbmenu|.gdchip'}});
on("rowDemo.spec",function(key){var m=/^guidance\.items\.([^.]+)\.inTry$/.exec(key);if(!m||!S.guidance.items[m[1]])return;var id=m[1];
  return{wall:true,wait:0,act:function(){gdDemoTryRow(id)},ringAfter:300,ring:'[data-gdtryrow="'+id+'"]|.gdtry|.gdchip'}});
/* ----- round 10: 6 · Preview & Test's simulator rows show on the phone too: The visitor plays its first two visit-minutes (×10);
   Speed plays the timeline at that speed, each dot's item on the phone (Instant: the two minutes ×10) ----- */
rowDemos({"ui.gdSim":{wall:true,wait:0,act:function(){var s=gdSimScript(S.ui.gdSim||"first");gdDemoSim({label:"The visitor: "+(s?s.name:S.ui.gdSim)})},ringAfter:600,ring:".gdchip|.gdbub"},
  "ui.gdSpeed":{wall:true,wait:0,act:function(){if(S.ui.gdSpeed==="instant"||!ed.querySelector("#gdsimtl"))gdDemoSim({label:S.ui.gdSpeed==="instant"?"Instant: the whole visit at once":"×"+S.ui.gdSpeed});else{G.gd.sim.t=0;gdSimPlay()}},ringAfter:600,ring:".gdchip|.gdbub"}});

/* ----- round 12, Guide › Guidance › Fun › On the tour (research_tour_entertainment.md 3.2, 3.3): the group's rows (On, Liveliness, Audiences;
   its rhythm: One beat per, At most a tour, on Short, on Deep), the tour's plan in words ("This tour's plan: …"), and each beat's form
   (gdBeatRow: Place in the tour, The visitor answers by, Waits, Needs, Pairs with, Scores, Calm too, Audiences, Tour length, and its
   own rows); Specific Marker › 3 · Guide script's per-step quiz question and guess (EN / PT, Draft with AI, a Reviewed tick per
   language); 6 · Preview & Test › The visitor's Games on the tour. Each row's demo plays its beat on the phone, in a tour, now
   (gdDemoBeat). Unity: the POI Editor's GuidanceGroup and GuidanceItem inspectors (the beat fields), the GuideStep's QuizQuestion and
   Guess (with a Draft with AI button calling the Worker, and a Reviewed toggle per language) ----- */
COMP["gd:g:ontour"]=["Guide","Games on the tour"];FOCUS["gd:g:ontour"]=["card",".gdock","full"];
["quizpromise","quizfinale","guessyear","dontsay","findwall","water","cookie"].forEach(function(id){var it=S.guidance.items[id];COMP["gd:i:"+id]=["Guide","Games on the tour › "+it.label];FOCUS["gd:i:"+id]=["card",".gdock","full"]});
["tour_promise","tour_promise_expert","tour_deal","tour_noquiz","tour_yes_please","tour_no_thanks"].forEach(function(k){TX_DOM[k]="said and shown in the quiz promise's card, after the introduction of a tour"});TX_DOM.tour_star="the star at the end of the tour's progress line";
Object.keys(TXOWN).filter(function(k){return/^tour_/.test(k)&&!TX_DOM[k]}).forEach(function(k){TX_DOM[k]="said and shown in a beat of the tour (Games on the tour)"});TX_DOM.settings_games="shown in Settings › The guide";
/* the plan line: the tour playing (or stopped) now, else the plan a tour would get now */
function gdPlanLine(){var P=G.gd.plan&&(G.on||G.gd.plan.stopped)?G.gd.plan:null;return P?"This tour's plan"+(P.stopped?" (stopped: kept for Resume)":"")+": "+gdPlanWords(P):"This tour's plan (a tour started now: "+(AUDN[aud()]||aud())+"): "+gdPlanWords(gdPlanNow())}
on("gdLog.after",function(){var n=ed.querySelector("#gdplan");if(n)n.textContent=gdPlanLine()});
/* 3 · Games on the tour (round 14: promoted from Guidance › Fun › On the tour): the group's On, the plan line, its rhythm, who plays,
   the fixed rules, then the games: the quiz (its promise and its questions, one beat: drawn as one, gd:pair:<the first>), Guess the year,
   Don't say it!, Find it on the wall, the breather and the cookie (its variant) */
function gdGamesHtml(){var g=gdGroupById("ontour"),k=gdKindById("fun"),gp=gdGroupPath("ontour");return g&&k&&gp?gdTourGroupFo(g,gp,k):""}
/* an item and its variants (the breather and the cookie, never both in a tour): drawn as one foldout, gd:var:<the base> (round 14b, the
   developer: the cookie inside the breather). The base of an item: itself when it has variants in its group, else the item it varies */
function gdVariantsOf(it){var g=it&&gdGroupById(it.group);return g?g.items.map(function(id){return S.guidance.items[id]}).filter(function(x){return x&&!x.deleted&&x.variantOf===it.id}):[]}
function gdVarBaseOf(it){if(!it)return null;var b=it.variantOf&&S.guidance.items[it.variantOf],g=gdGroupById(it.group);if(b&&!b.deleted&&g&&g.items.indexOf(b.id)>=0)return b;return gdVariantsOf(it).length?it:null}
function gdPairOf(it){var pr=it&&it.pairs&&S.guidance.items[it.pairs];if(!pr||pr.deleted)return null;var g=gdGroupById(it.group);if(!g||g.items.indexOf(pr.id)<0)return null;return g.items.indexOf(it.id)<g.items.indexOf(pr.id)?[it,pr]:[pr,it]}
function gdTourGroupFo(g,gp,k){var items=g.items.map(function(id){return S.guidance.items[id]}).filter(function(x){return x&&!x.deleted}),done={},rows="";
  items.forEach(function(it){if(done[it.id])return;var pa=gdPairOf(it);done[it.id]=1;
    if(pa){pa.forEach(function(x){done[x.id]=1});var fin=pa.filter(function(x){return x.beat==="finale"})[0]||pa[1];
      rows+=fo("gd:pair:"+pa[0].id,esc(fin.label)+' <span class="sub">· “'+esc(pa[0].label)+'” and “'+esc(pa[1].label)+'”: one beat, planned together or not at all</span>',null,pa.map(gdItemRow).join(""),2);return}
    var vb=gdVarBaseOf(it);if(vb){var vs=[vb].concat(gdVariantsOf(vb));vs.forEach(function(x){done[x.id]=1});
      rows+=fo("gd:var:"+vb.id,esc(vb.label)+' <span class="sub">· with its variant'+(vs.length>2?"s":"")+" "+vs.slice(1).map(function(x){return"“"+esc(x.label)+"”"}).join(", ")+': never both in a tour</span>',null,vs.map(gdItemRow).join(""),2);return}
    rows+=gdItemRow(it)});
  return'<div class="note">Made for the tour: each plays only at a seam of its tour\'s plan (after the introduction, between two steps, before the last step), never over a sentence. Its own rhythm, never Fun\'s visit cap; Fun\'s hold in a tour does not hold it, but Fun\'s On (4 · The mascot speaks up › Fun) turns them off too. A row: On, its name, its place, ▸ Show now; open it for its groups.</div>'+
    row("On",pseg(gp+"on",["true","false"],["On","Off"]),"Off: no beat in any tour (each keeps its own On).")+
    '<div class="note gdplan" id="gdplan" role="status">'+esc(gdPlanLine())+'</div>'+
    '<div class="sub2">Its rhythm <span class="sub">· the tour\'s own</span></div>'+
    row("One beat per (s)",pslider(gp+"every",60,120,5),"Seconds of narration for each beat (75): a tour has room for its narration ÷ this, rounded up, the opening and the finale included (a 1:24 tour: 2).")+
    row("At most a tour",pslider(gp+"perTour",1,6,1),"Beats in one Standard tour (3), whatever its length.")+
    row("At most on Short",pslider(gp+"perShort",1,6,1),"Beats in a Short tour (2).",true)+
    row("At most on Deep",pslider(gp+"perDeep",1,8,1),"Beats in a Deep tour (5).",true)+
    '<div class="sub2">Who plays <span class="sub">· The mascot\'s liveliness and the audiences</span></div>'+
    row("Liveliness",pseg(gp+"life",["azu","calm"],["As the mascot (1 · The mascot › Liveliness)","Calm"]),"As the mascot: Lively plays every beat; Calm plays only the items marked Calm too (the quiz, Guess the year). Calm: only the Calm too ones, whatever the mascot's Liveliness.")+
    GD_AUDS.map(function(a){return row(a[1],pseg(gp+"auds."+a[0],["true","false"],["On","Off"]),"Off: no beat for "+a[1]+" (Who's exploring). Each item has its own Audiences too.",true)}).join("")+
    '<div class="note gdfix">Fixed: never over a sentence (In a step: a game between the words, never stopping them) · the quiz\'s promise and the quiz count as one beat, and with the quiz promised the card\'s own quiz step is skipped · never on a Solemn point · never two seams in a row · never in the tour\'s first 20 s but the opening · never the same item twice in a tour · with a screen reader on, only items with a button path · a game\'s Not now for 4 s, two Not nows end the games for the visit · Fun\'s visit cap does not count them.</div>'+
    '<div class="sub2 gth">The games <span class="sub">· the quiz, Guess the year, Don\'t say it!, Find it on the wall, the breather and its variant</span></div>'+rows}
/* a beat's form: Identity, Says, Place, The visitor, Needs and pairs, The game (Don't say it!), The point (Find it on the wall), Who and
   when, Its look (the breather); its warnings and read-back below */
function gdBeatRow(it){var p="guidance.items."+it.id+".",g=gdGroupOf(it),wn=gdWarnsOf(it.id),A=GD_ANSWERS_OF[it.beat]||["chips"],pr=it.pairs&&S.guidance.items[it.pairs],P=gdPlanNow(),pw=P.why[it.id]||"",vo=it.variantOf&&S.guidance.items[it.variantOf];
  var sum='<span class="gdi">'+gdDragH(it)+'<span class="gdon">'+pseg(p+"on",["true","false"],["On","Off"])+'</span><b>'+esc(it.label)+'</b>'+(vo?'<span class="badge" title="A variant of '+esc(vo.label)+'">VARIANT</span>':"")+
    (wn.length?'<span class="badge gdwb" title="'+esc(wn.map(function(w){return w.text}).join(" · "))+'">&#9888; '+wn.length+'</span>':"")+
    '<span class="gdhow">'+esc(gdNameOf(GD_PLACES,it.place)+" · "+(/^planned/.test(pw)?pw.replace(/^planned for /,""):"not planned now"))+'</span><button type="button" class="ubtn sm gdshow" data-gdshow="'+it.id+'" title="Show it on the phone now, in a tour">&#9656; Show now</button></span>';
  var needs=it.needs==="quiz"?langCodes().map(function(l){var q=gdQSteps(l);return(GD_LANG_NAME[l]||l)+" "+q.length+" reviewed ("+q.join(", ")+")"}).join(" · "):it.needs==="year"?langCodes().map(function(l){var s=(S.fullSteps||S.guideSteps).filter(function(st){return gdGuessOk(S.tourGuess[st.id],l)}).map(function(st){return st.id});return(GD_LANG_NAME[l]||l)+": "+(s.join(", ")||"none")}).join(" · "):"";
  var fixedPlace=!!it.pairs||it.beat==="guess"||it.beat==="word"||it.beat==="find",ans=A.length>1?it.answers:A[0];
  return{sum:sum,top:gdPointLine(it),groups:[
    {k:"identity",t:"Identity",s:it.label+" · in Games on the tour"+(vo?", a variant of "+vo.label:""),html:row("Label",'<input class="u" data-path="'+p+'label" value="'+esc(it.label)+'">',"Editor only: the item's name in these lists, the plan, the Log and the scenes.")},
    {k:"says",t:"Says",s:"what the mascot says and the chips' words, "+langCodes().map(function(l){return l.toUpperCase()}).join(" | "),html:txPanel("gd:i:"+it.id)},
    {k:"place",t:"Place",s:gdNameOf(GD_PLACES,it.place)+" · now: "+(/^planned/.test(pw)?pw.replace(/^planned for /,""):"not planned"),html:
      (fixedPlace?'<div class="note">Place in the tour: '+esc(gdNameOf(GD_PLACES,it.place))+' ('+esc(GD_PLACE_WORDS[it.place])+(it.beat==="guess"?"; the seam before the step that holds the year":it.beat==="word"?"; its span is the word's (Specific Marker › Guide script)":it.beat==="find"?"; the seam after its step (The point)":"; fixed for the pair")+').</div>':
        row("Place in the tour",pseg(p+"place",GD_PLACES.map(function(x){return x[0]}),GD_PLACES.map(function(x){return x[1]})),"Opening: after the introduction, before the first step. Between steps: its own moment (below). Finale: before the tour's last step. In a step comes in round 13."))+
      (it.place==="between"&&it.beat==="breather"?row("After narration (s)",pslider(p+"talkFor",0,180,10),"The first free seam after this much of the tour's narration (never the last seam).",true)+
        row("Before the last step",pseg(p+"beforeEnd",["true","false"],["Yes","No"]),"Yes: at the seam before the tour's last step (the cookie).",true)+
        row("Only in tours of (s)",pslider(p+"minNarr",0,180,10),"A tour shorter than this has no room for it (the breather: 90 s; the demo tour without its introduction is 1:24).",true):"")+
      (+it.minSteps>0||it.beat==="promise"?row("Only in tours of (steps)",pslider(p+"minSteps",2,8,1),"A promise needs a tour long enough to quiz on (4 steps or more).",true):"")+
      '<div class="note gdplan2">'+esc("Now: "+(pw||"—"))+'</div>'},
    {k:"visitor",t:"The visitor",s:"answers by "+gdLow(gdNameOf(GD_ANSWERS,ans))+(it.beat==="finale"?" · no timer":" · waits "+it.waits+" s"+(it.beat==="guess"?" (+"+(+it.waitsFirst||0)+" s the first time)":""))+(it.game?" · a game":""),html:
      (A.length>1?row("The visitor answers by",pseg(p+"answers",A,A.map(function(x){return gdNameOf(GD_ANSWERS,x)})),it.beat==="finale"?"Tap: the choices only. Tap or say: the choices, and the mic where it is allowed (the recogniser's words matched to a choice: “the second”, “1755”). Never voice only.":"Chips: the ask and its chips. Nothing: it goes on by itself."):'<div class="note">The visitor answers by: '+esc(gdNameOf(GD_ANSWERS,A[0]))+'.</div>')+
      (it.beat==="finale"?'<div class="note">Waits: no timer (the quiz waits for each answer; Skip the quiz is always there).</div>':row("Waits (s)",pslider(p+"waits",3,it.beat==="guess"?30:12,1),it.beat==="guess"?"The time to answer from the card's showing, before the tour goes on by itself (14 s, round 19: it was 8, too fast for a first look at the slider; a drag starts it again).":it.beat==="word"?"Let's play / Not now stay up this long after the announce, while the span starts (4): waiting is Let's play.":it.beat==="find"?"Its Not now chip's seconds (4); the find itself waits for the visitor up to Give up after.":it.beat==="promise"?"The chips' seconds after the line (4): waiting is a Deal (Expert's opt-in: waiting is no).":"Seconds after it asks; then it carries on by itself (8)."))+
      (it.beat==="guess"?row("First guess of a visit (+ s)",pslider(p+"waitsFirst",0,10,1),"Seconds more for the visit's first guess, while the slider is new to the visitor (4: 18 s in all by default). Unity: GuidanceItem.waitsFirst."):"")+
      (it.game?'<div class="note">A game: its Not now for its first 4 s'+(it.beat==="promise"?" (No quiz, thanks)":"")+'; two Not nows this visit end the games for the visit (said once, kindly). Settings › The guide › Games on the tour Off: no game.</div>':'<div class="note">Not a game: Games on the tour does not hold it.</div>')},
    {k:"needs",t:"Needs and pairs",s:(it.needs?{quiz:"reviewed quiz questions",year:"a reviewed guess",word:"a reviewed word in each language",wall:"a point of the wall to find"}[it.needs]||it.needs:"nothing of the point's")+(pr?" · pairs with “"+pr.label+"”":""),html:
      '<div class="note">Needs: '+esc(GD_NEEDS[it.needs]||"nothing of the point's")+(needs&&!gdPointLine(it)?" · "+needs:"")+'</div>'+
      (it.beat==="promise"?row("Questions it needs",pslider(p+"qMin",2,5,1),"Reviewed questions in the visitor's language, from at least 2 steps (3); fewer: no promise (a promise kept).",true):"")+
      (pr?'<div class="note">Pairs with: “'+esc(pr.label)+'” (a promise and its payoff are one: planned together or not at all; a stop offers the quiz from the ✦ menu, or says it away).</div>':"")+
      (it.beat==="finale"?row("Questions",pslider(p+"qn",2,5,1),"How many it asks (3): from the steps heard (80 % played), one from each third, never two from a step.",true)+
        row("Scores",pseg(p+"scores",["true","false"],["On","Off"]),"On: the score at the end, never mocking, and the best score here kept on the phone.",true)+
        row("Show me after a wrong answer",pseg(p+"showMe",["true","false"],["On","Off"]),"On: a Show me chip that goes to the block the question came from.",true):"")},
    it.beat==="word"?{k:"game",t:"The game",s:"its word and span in each language · a tap window of "+it.tapWin+" s · "+(it.falseTaps==="nope"?"a playful Nope!":"false taps counted quietly"),html:gdWordRows(it,p)}:null,
    it.beat==="find"?{k:"point",t:"The point",s:(function(){var fd=S.tourFind,fp=fd&&(WALLS.ribeira.pois||[]).filter(function(x){return x.id===fd.point})[0];return(fp?L(fp.name)+", after step "+gdStepNo(fd.after):"no point")+" · give up after "+it.giveUp+" s"})(),html:gdFindRows(it,p)}:null,
    {k:"who",t:"Who and when",s:(it.calm?"Calm too":"Lively only")+" · "+(GD_AUDS.filter(function(a){return it.auds&&it.auds[a[0]]}).map(function(a){return a[1]}).join(", ")||"no audience")+" · "+gdLow(gdNameOf(GD_TOURLEN,it.tourLen)),html:
      row("Calm too",pseg(p+"calm",["true","false"],["Yes","No"]),"Yes: it plays when Liveliness is Calm too. No: Lively only.")+
      GD_AUDS.map(function(a){return row("For "+a[1],pseg(p+"auds."+a[0],["true","false"],["Yes","No"]),"Its Audiences: "+a[1]+" gets it (Who's exploring).",true)}).join("")+
      (it.beat==="promise"?row("For Expert, it is",pseg(p+"expert",GD_EXPERT.map(function(x){return x[0]}),GD_EXPERT.map(function(x){return x[1]})),"An opt-in question: “Want three tough questions at the end?” (waiting is no). As the others: the promise.",true):"")+
      row("Tour length",pseg(p+"tourLen",GD_TOURLEN.map(function(x){return x[0]}),GD_TOURLEN.map(function(x){return x[1]})),"The tours it plays in: every length, not on Short, or Deep only (the steps' own rule).")},
    it.beat==="breather"?{k:"look",t:"Its look",s:"prop: "+(it.props==="date"?"by date (today "+(GD_PROP_NAMES[gdPropOf(it,gdTodayMD(),[])]||"a glass")+")":GD_PROP_NAMES[it.prop]||it.prop)+" · "+(it.shows==="visit"?"once a visit":"every tour"),html:
      row("Prop",pseg(p+"props",["date","fixed"],["By date","Its own ("+(GD_PROP_NAMES[it.prop]||it.prop)+")"]),"By date: a glass, a fan in June-September, a scarf in December-February (Preview & Test › Today is), never the same twice in a visit. Its own: always its prop. Placed beside the mascot, 0 px over any words.")+
      '<div class="note">'+esc("Today ("+gdDayWords(gdTodayMD())+"): "+(GD_PROP_NAMES[gdPropOf(it,gdTodayMD(),[])]||"a glass"))+'</div>'+
      row("Shows",pseg(p+"shows",["visit","every"],["Once a visit","Every tour"]),"Once a visit: cute the first time, not the third (the research's rule).",true)}:null],
   foot:wn.map(function(w){return'<div class="note warn gdwarn" data-gdwarn="'+w.kind+'">&#9888; '+esc(w.text)+'</div>'}).join("")+
    '<div class="note gdrb">'+esc(gdReadBack(it))+'</div>'+
    '<div class="btnrow gdacts"><button class="ubtn sm" type="button" data-gddup="'+it.id+'">Duplicate</button><button class="ubtn sm" type="button" data-gddel="'+it.id+'">Delete</button></div>'}}
/* Specific Marker › Guide script at a point's own field (round 14: an item's per-point line, the Needs note's link): the tab, the Guide
   script open, the step chosen when the field is a step's, the field in view and marked a moment. t: word, find, jokes, steps, q:<step>,
   guess:<step>, or 1 (the Guide script) */
function gdScriptGo(t){var q=String(t||"1").split(":"),k=q[0],st=q[1];S.tab="marker";S.op.m3=true;if(st)S.sel.gstep=st;else if(k==="1")S.sel.gstep=S.sel.gstep||"g1";renderEditor();
  var sel={word:".gdwbox",find:".gdfindln",jokes:".gdjk",steps:'[data-fo="m3"] > .inner > table.tbl',q:'[data-gdland="q"]',guess:'[data-gdland="guess"]'}[k],el=sel&&ed.querySelector(sel),f=el||ed.querySelector('[data-fo="m3"]');
  if(f){f.scrollIntoView({block:el&&el.getBoundingClientRect().height<innerHeight*0.6?"center":"start"});if(el){el.classList.add("gdland");setTimeout(function(){el.classList.remove("gdland")},2400)}}document.body.dataset.gdland=k+"|"+(el?"found":"m3");return!!el}
ed.addEventListener("click",function(e){var t=e.target.closest("[data-gdscript]");if(!t)return;e.stopPropagation();gdScriptGo(t.dataset.gdscript)},true);
/* ----- each row's demo: the beat on the phone, in a tour, now. The tour starts (or stays), the phone goes to the beat's seam, and the beat
   plays as a pull (no memory), the value in a chip; the plan's rows: the plan a tour gets now, and its first beat played ----- */
function gdDemoSeamOf(id){var P=gdPlanNow(),b=P.beats.filter(function(x){return x.id===id})[0];if(b)return b.after;var it=S.guidance.items[id],st=gdStepsNow(),k0=P.k0,kf=P.kf;
  if(!it)return null;if(it.place==="instep"){var w=gdWordOf(S.lang||"en"),j0=w?st.findIndex(function(s){return s.id===w.span[0]}):-1;if(j0>0)return st[j0-1].id}if(it.beat==="find"&&S.tourFind)return S.tourFind.after;if(it.place==="opening")return P.ids[k0];if(it.place==="finale"||it.beforeEnd)return P.ids[kf];if(it.beat==="guess"){for(var j=k0+2;j<=kf;j++)if(S.tourGuess[st[j].id])return P.ids[j-1]}
  for(var s=k0+1;s<kf;s++)if(P.at[s]>=(+it.talkFor||20))return P.ids[s];return P.ids[Math.min(kf-1,k0+2)]}
function gdDemoBeat(id,chip,o){o=o||{};var my=gdDemoBegin(),it=gdItems().filter(function(x){return x.id===id})[0]||S.guidance.items[id];if(!it)return;
  if(!G.on){S.view="card";S.stop="full";startTour()}stopSpeech();G.paused=true;var after=o.after||gdDemoSeamOf(id),j=S.guideSteps.findIndex(function(s){return s.id===after});if(j>=0)G.i=j;renderGuide();
  var b={id:id,after:after,place:it.place,label:it.label,seam:j},l=S.lang||"en",qs=null;
  if(it.place==="instep"){var w=gdWordOf(l);if(w){b.span=w.span.slice();b.endAfter=w.span[w.span.length-1]}}
  if(it.beat==="finale"){qs=o.qs||S.guideSteps.filter(function(st){return gdQOk(S.tourQ[st.id],l)}).map(function(st){return st.id}).slice(0,+it.qn||3);if(!qs.length)qs=[]}
  if(G.gd.plan&&it.beat==="promise")G.gd.plan.quiz=null;
  var c=gdStart(it,"the Editor's demo",{pull:true,beat:b,owed:qs?{qs:qs,n:qs.length}:null});if(chip)gdChip(chip);gdDemoEnd(my,o.ms||9000);return c}
/* a field's demo: the beat with that value, a chip in words */
function gdBeatChipWords(id,f){var it=S.guidance.items[id],v=it[f];if(/^auds\./.test(f)){var a=f.slice(5);return"For "+gdNameOf(GD_AUDS,a)+": "+(it.auds[a]?"Yes":"No")+" · "+(it.auds[aud()]?"this visitor ("+gdNameOf(GD_AUDS,aud())+") gets it":"not for this visitor ("+gdNameOf(GD_AUDS,aud())+")")}
  var nm={tapWin:"Tap window: the word's start to its end + "+v+" s",falseTaps:"False taps: "+(v==="nope"?"a playful Nope!":"counted quietly"),slipBack:"Play my slip back: "+(v?"On":"Off"),giveUp:"Give up after "+v+" s: the mascot shows it",hintAfter:"Hint after "+v+" s",
    place:"Place in the tour: "+gdNameOf(GD_PLACES,v),answers:"Answers by: "+gdNameOf(GD_ANSWERS,v),waits:"Waits "+v+" s",calm:"Calm too: "+(v?"Yes":"No"),tourLen:"Tour length: "+gdNameOf(GD_TOURLEN,v),
    scores:"Scores: "+(v?"On":"Off"),showMe:"Show me after a wrong answer: "+(v?"On":"Off"),qn:"Questions: "+v,qMin:"Needs "+v+" reviewed questions",minSteps:"Only in tours of "+v+" steps or more",talkFor:"After "+v+" s of narration",
    beforeEnd:"Before the last step: "+(v?"Yes":"No"),minNarr:"Only in tours of "+v+" s or more",props:"Prop: "+(v==="date"?"by date ("+(GD_PROP_NAMES[gdPropOf(it,gdTodayMD(),[])]||"")+")":"its own"),shows:"Shows: "+(v==="visit"?"once a visit":"every tour"),
    expert:"For Expert: "+gdNameOf(GD_EXPERT,v),on:it.label+": "+(v?"On":"Off"),label:it.label};var P=gdPlanNow(),w=P.why[id]||"";return(nm[f]||it.label)+" · "+(/^planned/.test(w)?w:"not in a tour's plan now: "+w)}
function gdDemoBeatField(id,f){var it=S.guidance.items[id];if(!it)return;if(!it.on&&f==="on"){var my=gdDemoBegin();if(!G.on){S.view="card";S.stop="full";startTour()}gdChip(it.label+": Off · no tour plans it");gdDemoEnd(my,3200);return}
  if(f==="answers"&&it.beat==="breather"&&it.answers==="nothing"){gdDemoBeat(id,gdBeatChipWords(id,f));return}
  if(it.beat==="promise"&&f==="expert"){var keep=S.vis.aud;S.vis.aud="expert";gdDemoBeat(id,gdBeatChipWords(id,f));S.vis.aud=keep;return}
  /* round 13: Don't say it!'s game rows: the span playing (a slip's window, a false tap's Nope!), the result with the slip played back */
  if(it.beat==="word"&&(f==="tapWin"||f==="falseTaps"||f==="auds.family"||f==="auds.explorer")){gdDemoWord(f==="falseTaps"?"nope":"span",gdBeatChipWords(id,f));return}
  if(it.beat==="word"&&f==="slipBack"){gdDemoWord("result",gdBeatChipWords(id,f));return}
  if(it.beat==="find"&&(f==="giveUp"||f==="hintAfter")){gdDemoFind(f==="giveUp"?"giveup":"hint",gdBeatChipWords(id,f));return}
  gdDemoBeat(id,gdBeatChipWords(id,f))}
on("rowDemo.spec",function(key){var m=/^guidance\.items\.([^.]+)\.([\w.]+)$/.exec(key);if(!m)return;var d=S.guidance.items[m[1]];if(!d||d.form!=="beat")return;var id=m[1],f=m[2];
  return{tour:true,dock:"mini",wait:0,act:function(){gdDemoBeatField(id,f)},ringAfter:500,ring:".gdbeat|.gdchip"}});
/* the group's rows: the plan a tour gets now (a chip), its first beat played */
function gdDemoPlan(f){var my=gdDemoBegin(),P=gdPlanNow(),b=P.beats[0];if(!G.on){S.view="card";S.stop="full";startTour()}var w=gdPlanWords(P);
  if(!b){gdChip("No beats: "+w.replace(/^.*?\)\. /,""));gdDemoEnd(my,4200);return}gdDemoBeat(b.id,w.slice(0,140)+(w.length>140?"…":""))}
on("rowDemo.spec",function(key){var gp=gdGroupPath("ontour");if(!gp||key.indexOf(gp)!==0)return;var f=key.slice(gp.length);if(!/^(on|life|every|perTour|perShort|perDeep|auds\.\w+)$/.test(f))return;
  return{tour:true,dock:"mini",wait:0,act:function(){gdDemoPlan(f)},ringAfter:500,ring:".gdbeat|.gdchip"}});
/* 6 · Preview & Test › The visitor: Games on the tour (the visitor's own switch, Settings › The guide) */
on("edGuide.html",function(h){var d=document.createElement("div");d.innerHTML=h;var r=d.querySelector('[data-fo="gt0"] [data-set="@gdVisitor.prefs.stay"]'),row0=r&&r.closest(".row");if(!row0)return h;
  row0.insertAdjacentHTML("afterend",row("Games on the tour",pseg("gdVisitor.prefs.games",["on","off"],["On","Off"]),"The visitor's own switch (Settings › The guide › Games on the tour): Off, no game in a tour (the quiz, Guess the year); the breather still comes.",true));return d.innerHTML});
rowDemos({"gdVisitor.prefs.games":{stop:"half",act:function(){var my=gdDemoBegin();S.settingsOpen=true;renderPhone(false);var v=ph.querySelector(".vset"),g=v&&v.querySelector(".vgames");if(v&&g)v.scrollTop=Math.max(0,g.offsetTop-80);gdChip("Games on the tour: "+(gdPrefs().games==="off"?"Off: no game in a tour":"On"));gdDemoEnd(my,3200)},ringAfter:500,ring:".vgames|.gdchip"}});
/* ----- Specific Marker › 3 · Guide script: a step's quiz question and its guess (round 12). Each language its own words and its own
   Reviewed tick; Draft with AI writes a draft from the step's own text in that language (marked "AI draft", new names, dates and numbers
   flagged as the audiences' drafts are); an unreviewed question never plays ----- */
function gdTqHtml(st){var id=st.id,q=S.tourQ[id],g=S.tourGuess[id],p="tourQ."+id+".",gp="tourGuess."+id+".",H=function(t,s,land){return'<div class="sub2"'+(land?' data-gdland="'+land+'"':"")+'>'+t+(s?' <span class="sub">· '+s+'</span>':"")+'</div>'};
  var h=H("Quiz question","Games on the tour › The quiz: from this step, once it was heard","q");
  if(!q)h+='<div class="btnrow"><button class="ubtn sm" type="button" data-tqadd="'+id+'">+ Add a question</button>'+langCodes().map(function(l){return'<button class="ubtn sm" type="button" data-tqai="'+id+'|'+l+'">'+(S.tqBusy===id+"|"+l?"Drafting…":"Draft with AI ("+l.toUpperCase()+")")+'</button>'}).join("")+'</div>';
  else{h+=langCodes().map(function(l){var flags=q.src==="ai"?newTokens([L1(q.q,l)].concat((q.a||[]).map(function(a){return L1(a,l)}),[L1(q.why,l)]).join(" "),{en:stepTextIn(st,"en"),pt:stepTextIn(st,"pt")}):[],ww=gdWhyWords(L1(q.why,l));
      return'<div class="tqlang"><b>'+l.toUpperCase()+(q.src==="ai"?' <span class="badge gdwb">AI draft</span>':"")+(q.rev&&q.rev[l]?' <span class="badge new">REVIEWED</span>':' <span class="badge">NOT REVIEWED: it never plays</span>')+'</b>'+
        row("Question",'<input class="u" data-path="'+p+'q.'+l+'" value="'+esc(L1(q.q,l))+'">',"The question, as the mascot asks it.",true)+
        (q.a||[]).map(function(a,k){return row("Answer "+(k+1),'<input class="u" data-path="'+p+'a.'+k+'.'+l+'" value="'+esc(L1(a,l))+'">',"A choice (2 or 3).",true)}).join("")+
        row("Why",'<input class="u" data-path="'+p+'why.'+l+'" value="'+esc(L1(q.why,l))+'">'+(ww>12?'<div class="ovflag">'+ww+' words: 12 or fewer.</div>':""),"The line after the answer: 12 words or fewer.",true)+
        (flags.length?'<div class="ovflag" role="note">Not in the step\'s text: <b>'+flags.map(esc).join("</b>, <b>")+'</b>. Check each before it ships.</div>':"")+
        '<div class="btnrow"><button class="ubtn sm" type="button" data-tqai="'+id+'|'+l+'">'+(S.tqBusy===id+"|"+l?"Drafting…":"Draft with AI")+'</button><label class="ovrev"><input type="checkbox" data-tqrev="'+id+'|'+l+'"'+(q.rev&&q.rev[l]?" checked":"")+'> Reviewed: it can play</label></div></div>'}).join("")+
    row("The right one",pseg(p+"ok",(q.a||[]).map(function(a,k){return String(k)}),(q.a||[]).map(function(a,k){return"Answer "+(k+1)})),"The right answer (the same in each language).",true)+
    '<div class="btnrow">'+((q.a||[]).length<3?'<button class="ubtn sm" type="button" data-tqans="'+id+'|add">+ An answer</button>':'<button class="ubtn sm" type="button" data-tqans="'+id+'|drop">Remove answer 3</button>')+'<button class="ubtn sm" type="button" data-tqdel="'+id+'">Remove the question</button></div>'}
  if(S.tqNote)h+='<div class="ovflag">'+esc(S.tqNote)+'</div>';
  h+=H("Guess","Games on the tour › Guess the year: before this step, the visitor guesses the number this step says","guess");
  if(!g)h+='<div class="btnrow"><button class="ubtn sm" type="button" data-tgadd="'+id+'">+ Add a guess</button></div>';
  else h+=langCodes().map(function(l){return row("Question · "+l.toUpperCase(),'<input class="u" data-path="'+gp+'q.'+l+'" value="'+esc(L1(g.q,l))+'"><label class="ovrev"><input type="checkbox" data-tgrev="'+id+'|'+l+'"'+(g.rev&&g.rev[l]?" checked":"")+'> Reviewed</label>',"The guess's question in "+(GD_LANG_NAME[l]||l)+".",true)}).join("")+
    row("The number",'<input class="u" type="number" data-path="'+gp+'n" value="'+esc(g.n)+'">',"What this step says (the answer).",true)+
    row("Range from",'<input class="u" type="number" data-path="'+gp+'from" value="'+esc(g.from)+'">',"The slider's start: a range where a good guess is possible.",true)+
    row("Range to",'<input class="u" type="number" data-path="'+gp+'to" value="'+esc(g.to)+'">',"The slider's end.",true)+
    row("Unit",pseg(gp+"unit",["year","number"],["A year","A number"]),"A year (“Only 20 years off!”) or a number.",true)+
    '<div class="btnrow"><button class="ubtn sm" type="button" data-tgdel="'+id+'">Remove the guess</button></div>';
  return'<div class="tqbox">'+h+'</div>'}
function L1(o,l){return o&&o[l]!=null?String(o[l]):""}
function stepTextIn(st,l){if(st.text)return st.text[l]||"";var r=byId(S.story,(st.ref||"").slice(2));return r&&r.c&&r.c.text?r.c.text[l]||"":""}
on("edMarker.html",function(h){var d=document.createElement("div");d.innerHTML=h;var st=S.sel.gstep&&byId(S.fullSteps||S.guideSteps,S.sel.gstep),slot=st&&d.querySelector('[data-pick="gstep"][data-id="'+cssq(st.id)+'"] + tr.slot td:last-child');
  if(slot&&!/^(intro|teaser|end)$/.test(st.act))slot.insertAdjacentHTML("beforeend",gdTqHtml(st));return d.innerHTML});
/* Draft with AI: one question from the step's own text, in one language (sampleFn: the published Lab's model; never here offline) */
function gdTqDraft(id,l){var st=byId(S.fullSteps||S.guideSteps,id);if(!st)return;if(typeof sampleFn!=="function"||!sampleFn){S.tqNote="AI is not available here (it runs inside the published Lab).";renderEditor();return}
  S.tqBusy=id+"|"+l;S.tqNote="";renderEditor();var lang=l==="pt"?"European Portuguese (the visitor addressed politely in the third person)":"English";
  var prompt="Write one multiple-choice quiz question about this heritage text, for a visitor who has just heard it. Use ONLY facts that are in the text: add no names, dates, numbers, places or events. 2 or 3 short answers, exactly one right; a why line of 12 words or fewer. Language: "+lang+". Return JSON only: {\"q\":\"...\",\"a\":[\"...\",\"...\",\"...\"],\"ok\":0,\"why\":\"...\"}\n\nTEXT: "+stepTextIn(st,l);
  sampleFn(prompt,{modelTier:"quick"}).then(function(r){var j=null;try{j=JSON.parse(String(r.text).replace(/^[^{]*/,"").replace(/[^}]*$/,""))}catch(x){}S.tqBusy=null;
    if(!j||!j.q||!Array.isArray(j.a)||j.a.length<2){S.tqNote="The draft could not be read; try again.";renderEditor();return}
    var q=S.tourQ[id]||(S.tourQ[id]={q:{},a:[],ok:0,why:{},src:"ai",rev:{}}),n=Math.min(3,j.a.length),other=(q.a||[]).some(function(a){return langCodes().some(function(x){return x!==l&&a&&a[x]})});
    q.q[l]=String(j.q);for(var k=0;k<n;k++){q.a[k]=q.a[k]||{};q.a[k][l]=String(j.a[k])}if(!other){q.a.length=n;q.ok=Math.max(0,Math.min(n-1,+j.ok||0))}q.why[l]=String(j.why||"");q.src="ai";q.rev=q.rev||{};q.rev[l]=false;renderEditor()})
    .catch(function(){S.tqBusy=null;S.tqNote="The AI could not answer; try again.";renderEditor()})}
ed.addEventListener("click",function(e){var t=e.target.closest("[data-tqai],[data-tqadd],[data-tqdel],[data-tqans],[data-tgadd],[data-tgdel]");if(!t)return;e.stopPropagation();var q;
  if(t.dataset.tqai){q=t.dataset.tqai.split("|");gdTqDraft(q[0],q[1]);return}
  if(t.dataset.tqadd){S.tourQ[t.dataset.tqadd]={q:{en:"",pt:""},a:[{en:"",pt:""},{en:"",pt:""}],ok:0,why:{en:"",pt:""},src:"human",rev:{en:false,pt:false}};renderEditor();return}
  if(t.dataset.tqdel){delete S.tourQ[t.dataset.tqdel];renderEditor();return}
  if(t.dataset.tqans){q=t.dataset.tqans.split("|");var o=S.tourQ[q[0]];if(!o)return;if(q[1]==="add")o.a.push({en:"",pt:""});else{o.a.length=2;if(+o.ok>1)o.ok=0}renderEditor();return}
  if(t.dataset.tgadd){S.tourGuess[t.dataset.tgadd]={q:{en:"",pt:""},n:1700,from:1500,to:1900,unit:"year",src:"human",rev:{en:false,pt:false}};renderEditor();return}
  if(t.dataset.tgdel){delete S.tourGuess[t.dataset.tgdel];renderEditor()}},true);
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset)return;var q;
  if(t.dataset.tqrev){e.stopImmediatePropagation();q=t.dataset.tqrev.split("|");var o=S.tourQ[q[0]];if(o){(o.rev=o.rev||{})[q[1]]=t.checked;renderEditor()}return}
  if(t.dataset.tgrev){e.stopImmediatePropagation();q=t.dataset.tgrev.split("|");var g=S.tourGuess[q[0]];if(g){(g.rev=g.rev||{})[q[1]]=t.checked;renderEditor()}}},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(tourQ\\.|tourGuess\\.|"));
/* a step's question or guess row: that question in the quiz on the phone, or the guess before its step */
on("rowDemo.spec",function(key){var m=/^(tourQ|tourGuess)\.([^.]+)\./.exec(key);if(!m)return;var id=m[2];
  if(m[1]==="tourQ")return{tour:true,dock:"mini",wait:0,act:function(){gdDemoBeat("quizfinale","The quiz · the question of this step",{qs:[id]})},ringAfter:500,ring:".gdbeat|.gdchip"};
  return{tour:true,dock:"mini",wait:0,act:function(){var P=gdPlanNow(),j=P.ids.indexOf(id);gdDemoBeat("guessyear","Guess the year · before this step",{after:j>0?P.ids[j-1]:null})},ringAfter:500,ring:".gdbeat|.gdchip"}});

/* ----- round 13: Don't say it!'s and Find it on the wall's rows in the beat's form (research 4.4, 4.7), their demos; Specific Marker › 3 ·
   Guide script's word per language (the Editor's suggestions from the tour's own text, each with its count and span, the author's pick,
   its Reviewed tick, the warnings for a count of 1 or over 4) and the find's line. Unity: the GuideTour asset's ForbiddenWord per language
   (a suggestions list drawn by the inspector, a Reviewed toggle) and its WallFind (a point of the wall, its step) ----- */
function gdWordPickOpts(l){var w=gdWordOf(l),out=[],seen={},cur=w?w.w+"|"+w.span.join(","):"";
  if(w)out.push([cur,w.w+" · "+gdSpanWords(w.span)+" · "+gdWordCount(w,l)+" times"+(w.src==="typed"?" (typed)":"")]);seen[cur]=1;
  gdWordSuggest(l).slice(0,8).forEach(function(c){var v=c.w+"|"+c.span.join(",");if(seen[v])return;seen[v]=1;out.push([v,gdWordLine(c)+(c.flags.length?" · "+c.flags.length+" rule"+(c.flags.length>1?"s":"")+" bent":"")])});
  if(!out.length)out.push(["","no word fits: no game in "+(GD_LANG_NAME[l]||l)]);return out}
/* the spans the word could have: 1-2 consecutive steps that say it */
function gdWordSpanOpts(l){var w=gdWordOf(l);if(!w)return[["","—"]];var all=(S.fullSteps||S.guideSteps).filter(function(st){return!/^(intro|teaser|end)$/.test(st.act)}),out=[];
  for(var i=0;i<all.length;i++)[1,2].forEach(function(n){var sp=all.slice(i,i+n);if(sp.length<n)return;var o={w:w.w,span:sp.map(function(s){return s.id})},c=gdWordCount(o,l);if(!c)return;
    var secs=sp.reduce(function(a,s){return a+gdStepSecsIn(s,l)},0);out.push([o.span.join(","),gdSpanWords(o.span)+" · "+c+" time"+(c>1?"s":"")+" ("+gdMmss(secs)+")"])});
  if(!out.some(function(x){return x[0]===w.span.join(",")}))out.unshift([w.span.join(","),gdSpanWords(w.span)+" · "+gdWordCount(w,l)+" times"]);return out}
function gdWordRows(it,p){
  return langCodes().map(function(l){var w=gdWordOf(l);return row("The word · "+l.toUpperCase(),'<select class="u" data-sel="@tourWord.'+l+'.pick">'+gdWordPickOpts(l).map(function(o,i){return'<option value="'+esc(o[0])+'"'+(i?"":" selected")+'>'+esc(o[1])+'</option>'}).join("")+'</select>'+
      '<div class="note">'+(w?(w.rev?"Reviewed":"NOT reviewed: no game in "+(GD_LANG_NAME[l]||l))+' · <button class="ubtn sm gdedited" type="button" data-gdscript="word">Guide script ›</button>':"No word: no game in "+(GD_LANG_NAME[l]||l))+'</div>',
      "The word (suggested): from the tour's own text in "+(GD_LANG_NAME[l]||l)+", a word of 4 letters or more said 2-4 times in a span of 1-2 steps (45-120 s), not in the span's first sentence, a noun the story is about; each with its count and span, and the rules it bends. Picking one needs the author's Reviewed tick (Specific Marker › Guide script) before it plays.")}).join("")+
    langCodes().map(function(l){return row("The span · "+l.toUpperCase(),'<select class="u" data-sel="@tourWord.'+l+'.spanPick">'+gdWordSpanOpts(l).map(function(o){var w=gdWordOf(l);return'<option value="'+esc(o[0])+'"'+(w&&o[0]===w.span.join(",")?" selected":"")+'>'+esc(o[1])+'</option>'}).join("")+'</select>',
      "The steps the game plays in: announced at the seam before them, its result at the seam after them. Its slips are the word's own times in them (2-4).",true)}).join("")+
    row("Tap window (s)",pslider(p+"tapWin",1,5,.5),"A tap on the mascot counts from the word's start to its end + this (2.5 s: outdoors, a phone in hand; to tune with the five visitors). Pause, a drag and Ask freeze it.")+
    row("False taps",pseg(p+"falseTaps",["nope","quiet"],["A playful Nope!","Counted quietly"]),"A tap when the word was not said: the mascot shakes its head and a tiny “Nope!” for 1 s, no voice (counted, never punished); or counted with nothing shown.")+
    row("Play my slip back",pseg(p+"slipBack",["true","false"],["On","Off"]),"On: at the result, the first slip the visitor missed is played back (the recorded clip from 1.2 s before the word to 0.3 s after; with the live or device voice, the phrase said again), the word lit.")+
    '<div class="note">Fixed: pokes are off in the span (a tap on the mascot means “you said it”; three quick taps still tickle) · at Compact and with Mascot Off a <b>Heard it!</b> button by the ✦ · a screen reader: not offered · Reduce Motion: the pip only · a language change or a hidden step ends it quietly · the captions light the word like every word · Previous never counts a slip twice.</div>'}
function gdFindRows(it,p){var fd=S.tourFind||{},pts=(WALLS.ribeira.pois||[]).filter(function(x){return x.id!==(S.m&&S.m.poi||"painting")}),
    st=(S.fullSteps||S.guideSteps).filter(function(s){return!/^(intro|teaser|end)$/.test(s.act)}),all=S.fullSteps||S.guideSteps;
  return     row("The point",psel("tourFind.point",pts.map(function(x){return[x.id,L(x.name)]})),"A detail of the card's wall that the tour's text names (the demo: the Torreão, step 4 says “its great corner tower on the left”). The wall only: the card's point and this one on the wall in view.")+
    row("After step",psel("tourFind.after",st.map(function(s){return[s.id,(all.indexOf(s)+1)+" · "+(stepTitle(s)||s.id)]})),"The seam after this step: the step that names it.",true)+
    langCodes().map(function(l){return row("Its name · "+l.toUpperCase(),'<input class="u" data-path="tourFind.thing.'+l+'" value="'+esc(L1(fd.thing,l))+'">',"{thing} in the line: “find "+(L1(fd.thing,l)||"…")+" on the real wall”.",true)}).join("")+
    row("Give up after (s)",pslider(p+"giveUp",10,40,1),"After this, the mascot walks to it and shows it (20 s: the arm tires); I give up does it at once.")+
    row("Hint after (s)",pslider(p+"hintAfter",3,20,1),"After this, a hint from the point's direction on the screen: “A little to the left…” (10 s).")+
    '<div class="note">Fixed: the card lowers and the wall comes back, the panel at Mini; a seek ring in the middle; the point settled in the ring 1.2 s (the phone still) is “That\'s it!” with a glint; never while the wall is lost (a system state wins); then back to the card, the tour goes on.</div>'}
/* the selects: a suggestion picked (the word and its span; it needs Reviewed again), or another span for the same word */
on("setOpt.answer",function(key,v){var m=/^@?tourWord\.(\w+)\.(pick|spanPick)$/.exec(key);if(!m)return;var l=m[1],o=S.tourWord[l]||(S.tourWord[l]={w:"",span:[],src:"suggested",rev:false});if(!v)return true;
  /* the choice made is kept as it was picked (what the Editor shows; the word and its span are what plays) */
  o[m[2]]=String(v);
  if(m[2]==="pick"){var a=String(v).split("|");if(a[0]!==o.w||a[1]!==o.span.join(",")){o.w=a[0];o.span=(a[1]||"").split(",").filter(Boolean);o.src="suggested";o.rev=false}}
  else{var sp=String(v).split(",").filter(Boolean);if(sp.join(",")!==o.span.join(",")){o.span=sp;o.rev=false}}renderAll(true);return true});
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!/^tourWord\.\w+\.w$/.test(t.dataset.path||""))return;var l=t.dataset.path.split(".")[1],o=S.tourWord[l];if(!o)return;o.w=gdWordNorm(t.value)||String(t.value).trim();o.src="typed";o.rev=false;renderEditor()},true);
ed.addEventListener("click",function(e){var t=e.target.closest("[data-gdwpick]");if(!t)return;e.stopPropagation();var a=t.dataset.gdwpick.split("|"),l=a[0],o=S.tourWord[l]||(S.tourWord[l]={});o.w=a[1];o.span=a[2].split(",");o.src="suggested";o.rev=false;renderEditor()},true);
/* Specific Marker › 3 · Guide script: the point's word per language, and its find (a read line: edited in the beat's form) */
function gdWordHtml(){var H=function(t,s){return'<div class="sub2">'+t+(s?' <span class="sub">· '+s+'</span>':"")+'</div>'};
  var h=H("Don't say it! · the word","each language its own, Reviewed before it plays");
  h+=langCodes().map(function(l){var w=gdWordOf(l),n=w?gdWordCount(w,l):0,sug=gdWordSuggest(l),warn=!w?"no word: no game in "+(GD_LANG_NAME[l]||l):n<2?"said "+(n?"once":"never")+" in its span: no game":n>4?"said "+n+" times in its span: too easy, too long (2-4)":"",
      mine=w&&sug.filter(function(c){return c.w===w.w&&c.span.join(",")===w.span.join(",")})[0];
    return'<div class="tqlang"><b>'+l.toUpperCase()+(w&&w.src==="suggested"?' <span class="badge gdwb">SUGGESTED</span>':w?' <span class="badge gdwb">TYPED</span>':"")+(w&&w.rev?' <span class="badge new">REVIEWED</span>':' <span class="badge">NOT REVIEWED: no game</span>')+'</b>'+
      row("The word",'<select class="u" data-sel="@tourWord.'+l+'.pick">'+gdWordPickOpts(l).map(function(o,i){return'<option value="'+esc(o[0])+'"'+(i?"":" selected")+'>'+esc(o[1])+'</option>'}).join("")+'</select>',"The word (suggested): picked from the suggestions (each: its count and span). Games on the tour › Don't say it! plays it.",true)+
      row("Or type it",'<input class="u" data-path="tourWord.'+l+'.w" value="'+esc(w?w.w:"")+'">',"A word of your own (it needs Reviewed again; the count and the warnings follow it).",true)+
      row("The span",'<select class="u" data-sel="@tourWord.'+l+'.spanPick">'+gdWordSpanOpts(l).map(function(o){return'<option value="'+esc(o[0])+'"'+(w&&o[0]===w.span.join(",")?" selected":"")+'>'+esc(o[1])+'</option>'}).join("")+'</select>',"The 1-2 steps it plays in.",true)+
      '<div class="note gdwcount">'+esc(w?"“"+w.w+"”: "+n+" time"+(n===1?"":"s")+" in "+gdSpanWords(w.span)+(mine?" ("+gdMmss(mine.secs)+")":""):"—")+'</div>'+
      (warn?'<div class="ovflag" role="note">&#9888; '+esc(warn)+'</div>':"")+(mine&&mine.flags.length?'<div class="note">Rules it bends: '+esc(mine.flags.join(" · "))+'</div>':"")+
      '<div class="note gdwsug">Suggestions: '+(sug.length?sug.slice(0,6).map(function(c){return'<button class="ubtn sm" type="button" data-gdwpick="'+l+'|'+esc(c.w)+'|'+c.span.join(",")+'" title="'+esc(c.flags.join(" · ")||"fits every rule")+'">'+esc(gdWordLine(c))+(c.flags.length?" · "+c.flags.length+" rule"+(c.flags.length>1?"s":"")+" bent":"")+'</button>'}).join(" "):"none: no word of 4 letters or more is said 2-4 times in a span of 1-2 steps")+'</div>'+
      '<div class="btnrow"><label class="ovrev"><input type="checkbox" data-gdwrev="'+l+'"'+(w&&w.rev?" checked":"")+(w?"":" disabled")+'> Reviewed: it can play</label></div></div>'}).join("");
  var fd=S.tourFind,fp=fd&&(WALLS.ribeira.pois||[]).filter(function(x){return x.id===fd.point})[0],all=S.fullSteps||S.guideSteps,ai=fd?all.findIndex(function(s){return s.id===fd.after})+1:0;
  h+=H("Find it on the wall","after a step, a detail on the real wall")+
    '<div class="note gdfindln">'+(fd&&fp?esc("“"+(L(fd.thing)||L(fp.name))+"” ("+L(fp.name)+"), after step "+ai+(gdFindOnWall(fd)?"":" · not on the wall in view now: no find"))+' <button class="ubtn sm gdedited" type="button" data-goto="gd:f:findwall:point">Edited in Guide › Games on the tour › Find it on the wall › The point &#8250;</button>':"No find on this point.")+'</div>';
  return'<div class="tqbox gdwbox">'+h+'</div>'}
on("edMarker.html",function(h){var d=document.createElement("div");d.innerHTML=h;var t=d.querySelector('[data-fo="m3"] > .inner > table.tbl');if(!t)return h;t.insertAdjacentHTML("afterend",gdWordHtml());return d.innerHTML});
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.gdwrev)return;e.stopImmediatePropagation();var o=S.tourWord[t.dataset.gdwrev];if(o){o.rev=t.checked;renderEditor()}},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(tourWord\\.|tourFind\\.|"));
/* the demos: the announce at its seam (the word's rows), the game playing in its span, a false tap, the result with the slip played back;
   the find at its seam (its rows), the hint at once, I give up at once */
function gdDemoWord(stage,chip){var my=gdDemoBegin(),it=gdItems().filter(function(x){return x.id==="dontsay"})[0]||S.guidance.items.dontsay,w=gdWordOf(S.lang||"en");if(!w){gdChip("Don't say it!: no word in this language");gdDemoEnd(my,3200);return}
  if(!G.on){S.view="card";S.stop="full";startTour()}stopSpeech();gdBeatClose();gdTailEnd();var j=S.guideSteps.findIndex(function(s){return s.id===w.span[0]});if(j<0){gdChip("Don't say it!: its span is not in this tour");gdDemoEnd(my,3200);return}
  var b={id:"dontsay",span:w.span.slice(),endAfter:w.span[w.span.length-1]};
  if(stage==="result"){var d=gdWordStart(b);var k=0,st=S.guideSteps[j];G.i=j;G.paused=true;renderGuide();G.words=(stepText(st).match(/\S+/g)||[]).map(function(x){return{w:x}});G.words.some(function(x,i){if(gdWordHit(x.w,w.w,S.lang)){k=i;return true}});
    d.slips[st.id+":"+k]={key:st.id+":"+k,step:st.id,k:k,caught:false,text:stepText(st),phrase:gdWordPhrase(k),said:1};d.order.push(st.id+":"+k);d.slips.x={key:"x",caught:true};d.order.push("x");d.caught=1;
    var P=G.gd.plan;d.on=false;clearInterval(d.iv);if(P)P.wordScore={word:d.word,caught:1,n:2,falses:0};gdStart(it,"the Editor's demo",{pull:true,react:true});if(chip)gdChip(chip);gdDemoEnd(my,14000);return}
  gdWordStart(b);G.i=j;G.paused=false;runStep();if(chip)gdChip(chip);if(stage==="nope")setTimeout(function(){if(G.demoSeq===my||G.gd.demoOwner===my)gdWordTap("the Editor's demo")},900);gdDemoEnd(my,9000)}
function gdDemoFind(stage,chip){var c=gdDemoBeat("findwall",chip,{ms:12000});setTimeout(function(){var F=G.gd.find;if(!F)return;if(stage==="hint"){F.clock=(+F.B.item.hintAfter||10)*1000;F.hinted=true;gdFindHint(F)}else gdFindGiveUp(F,"the Editor's demo")},1800)}
on("rowDemo.spec",function(key){var m=/^(tourWord|tourFind)\./.exec(key);if(!m)return;
  if(m[1]==="tourWord")return{tour:true,dock:"mini",wait:0,act:function(){gdDemoBeat("dontsay","Don't say it! · its word in "+(GD_LANG_NAME[S.lang||"en"]||S.lang))},ringAfter:500,ring:".gdbeat|.gdchip"};
  return{tour:true,dock:"mini",wait:0,act:function(){gdDemoBeat("findwall","Find it on the wall · "+(S.tourFind?S.tourFind.point:""),{ms:12000})},ringAfter:700,ring:".gdseek|.gdbeat|.gdchip"}});
