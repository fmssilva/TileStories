/* Each element's demo content (S.demoC) and where a demo field lives; each Editor row's demo (rowDemos, showRowDemo) and the self-test's NO DEMO over every tab. */
S.demoC={title:{text:DEMO.title},eyebrow:{text:DEMO.eyebrow},heading:{text:DEMO.heading},lede:{text:DEMO.lede},paragraph:{text:DEMO.paragraph},
 callout:{title:DEMO.ctitle,text:DEMO.ctext,back:DEMO.back},pull_quote:{text:DEMO.quote,cite:DEMO.cite},list:{items:DEMO.list},
 picture:{img:"g3",cap:DEMO.piccap},picture_pair:{imgs:["before","g2"],cap:DEMO.paircap},carousel:{imgs:["g1","g2","g3"]},video:{img:"hero",cap:DEMO.vidcap},
 audio:{cap:T("Guided tour · 3:11","Visita guiada · 3:11")},animation:{file:"tile_bloom.lottie"},
 before_after:{before:"before",after:"g2",blabel:T("Before 1755","Antes de 1755"),alabel:T("Today","Hoje")},hotspot:{img:"g3"},model_3d:{img:"g1"},panorama:{img:"hero"},
 timeline:{items:{en:["c. 1500 · The court moves to the river","1580s · The Torreão rises","1755 · Earthquake, fire and tsunami"],pt:["c. 1500 · A corte muda-se para o rio","1580s · Ergue-se o Torreão","1755 · Terramoto, incêndio e maremoto"]}},
 scrolly:{items:{en:["1662: the palace along the river.","Before 1755: the Torreão and the opera.","Today: the square that took its place."],pt:["1662: o palácio ao longo do rio.","Antes de 1755: o Torreão e a ópera.","Hoje: a praça que ficou no seu lugar."]}}};
var DEMOC0=JSON.parse(JSON.stringify(S.demoC));
on("renderEl.content",function(c,kind){if(!c||!Object.keys(c).length)c=S.demoC[kind]||{};return c});
on("renderEl.html",function(h,a){return a.kind==="animation"&&S.view!=="demo"?h.replace(/<small>[\s\S]*?<\/small>/,""):h});
on("contentAt.answer",function(path){if(path.indexOf("demo:")===0){var k=path.slice(5);return S.demoC[k]=S.demoC[k]||{}}
  if(path.indexOf("termpage:")===0){var q=path.split(":"),t=byId(S.terms,q[1]),r=t&&t.page[+q[2]];return r?(r.c=r.c||{}):null}});
ed.addEventListener("focusin",function(e){var f=e.target.dataset&&e.target.dataset.field;if(f&&(f.indexOf("demo:")===0||f.indexOf("termpage:")===0))pathShow(f)});

/* ----- each row's demo: a row declares its demo, one runner shows it -----
   A module declares the demos of its own rows, by the row's key: rowDemos({"guideCfg.x": spec}) (it joins the point rowDemo.spec), or
   rowDemos(function(key){…}) for a family of rows (every block's options, every section's, every motion's): it returns the spec of a key
   it owns, else nothing. A row's key is its option's path (@…), or, for a row with none, its first control's key (motion.dur, fx.h…:
   the part it edits is the foldout it sits in, as setCtx reads it). A change to the row, or a press on its label (▸), runs
   showRowDemo(key): the phone goes to the spec's view, sets its state, does its action and rings the part the row changes. A spec:
   view ("card", "story", "democard", "demo"…) and stop (the sheet's stop, "full" when not given); block (a block kind: the card that
   holds it, the marker's or else the demo card, at Full, the block brought into view and ringed when no ring is given); page (a block
   kind: its full page open, the story, the gallery or its own page); past (the card scrolled past the hero: true or a scrollTop);
   wall (true: the wall; "open": Ask about the view open); tour (a step's id, or true: the start) and dock (the dock's height then);
   intro (the tour starts with Azu's introduction); scroll (a selector brought into view); act (a function: a tap, a line, a gesture)
   and wait (ms before it: 0 starts it in the same frame as the change); ring (the part: selectors joined by "|", the first found is
   ringed) and ringAfter (ms after the act). view, stop, block, page, past, scroll and ring may be functions, read when the demo runs.
   Unity: the POI Editor's row gets its preview action, which sets the Play-mode guide the same way (PlayMode Live Config) */
function rowDemos(m){on("rowDemo.spec",typeof m==="function"?m:function(key){return m[key]})}
function rowDemoOf(key){return firstAnswer("rowDemo.spec",String(key||"").replace(/^@/,""))}
function rowKey(r){var c=r.querySelector('[data-set^="@"],[data-sel^="@"],[data-num^="@"]')||r.querySelector("[data-set],[data-sel],[data-num]");return c?(c.dataset.set||c.dataset.sel||c.dataset.num).replace(/^@/,""):null}
function rdv(x){return typeof x==="function"?x():x}
function showRowDemo(key){var sp=rowDemoOf(key);if(!sp||noDraw)return false;var my=G.demoSeq=(G.demoSeq||0)+1;clearTimeout(G.demoT);G.demoStart=performance.now();
  if(G.on)stopTour();if(typeof hideStopUi==="function")hideStopUi();S.settingsOpen=false;S.contentsOpen=false;G.fab.menu=false;G.cam.open=false;var o=ph.querySelector(".onb");if(o)o.remove();
  var bk=rdv(sp.block),pk=rdv(sp.page),bv=bk?blockAt(bk):null,ring=sp.ring!=null?sp.ring:bv?bv.sel:null,scroll=sp.scroll!=null?sp.scroll:bv?bv.sel:null,past=rdv(sp.past);
  if(sp.wall){S.view="card";S.stop="closed";G.cam.open=sp.wall==="open";renderPhone(false)}
  else{if(pk)pageOpen(pk);else S.view=bv?bv.view:rdv(sp.view)||"card";S.stop=rdv(sp.stop)||"full";renderPhone(false);if(past)scr.scrollTop=typeof past==="number"?past:900}
  if(sp.tour){S.introSeen=!sp.intro;G.demoTour=true;startTour();G.demoTour=false;var j=typeof sp.tour==="string"?S.guideSteps.findIndex(function(s){return s.id===sp.tour}):-1;if(j>0){stopSpeech();G.i=j;runStep()}if(sp.dock){S.dockH=sp.dock;renderGuide()}}
  var go=function(){if(G.demoSeq!==my)return;var sc=rdv(scroll);if(sc){var e=phFind(sc),put=function(){if(G.demoSeq===my&&e&&scr.contains(e))scr.scrollTop=Math.max(0,e.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-(ph.clientHeight-(parseFloat(scr.style.top)||0))/3)};
      put();setTimeout(put,380)}
    if(sp.act)sp.act();G.demoT=setTimeout(function(){if(G.demoSeq===my)ringPart(key,rdv(ring))},sp.ringAfter!=null?sp.ringAfter:sp.act?300:60)};
  var w=sp.wait!=null?sp.wait:sp.tour?900:400;if(w)G.demoT=setTimeout(go,w);else go();return true}
/* the part, ringed for 2.4 s, the one ring on the phone (an older demo's goes); body[data-rowdemo] says what was shown (the checks read it).
   The scroll puts the part a third of the way down the sheet's height at its stop (not the height it has mid-slide), and once more
   when the hero has settled at the new stop (0.35 s) */
/* the first of the selectors joined by "|" found on the phone (an id also in the page: the preview bar) */
function phFind(sels,out){sels=String(sels||"").split("|");for(var i=0;i<sels.length;i++){var s=sels[i],el=s&&(ph.querySelector(s)||(s.charAt(0)==="#"?document.querySelector(s):null));if(el){if(out)out.sel=s;return el}}return null}
function ringPart(key,ring){var o={},el=phFind(ring,o),sel=el?o.sel:String(ring||"").split("|").pop();G.demoRingAt=performance.now();
  document.body.dataset.rowdemo=key+"|"+(el?"found":"missing")+"|"+sel;document.querySelectorAll(".pfocus").forEach(function(x){if(x!==el)x.classList.remove("pfocus")});if(!el)return;el.classList.remove("pfocus");void el.offsetWidth;el.classList.add("pfocus");clearTimeout(el._rdT);el._rdT=setTimeout(function(){el.classList.remove("pfocus")},2400)}
/* the triggers: a changed value (after the change is made) or a press on the row's label; the change's demo starts as soon as the
   change is made, in its own task (G.demoAsk: when it was asked, G.demoStart: when it started; the checks read them) */
function rowDemoSoon(k){G.demoAsk=performance.now();G.demoPend=k;clearTimeout(G.demoPendT);G.demoPendT=setTimeout(rowDemoNow,0)}
function rowDemoNow(){var k=G.demoPend;G.demoPend=null;clearTimeout(G.demoPendT);if(k)showRowDemo(k)}
/* the change made (the Editor's own handlers ran): the demo starts in the same task; a handler that stops the event leaves it to the
   next task (the timeout above) */
["click","change"].forEach(function(t){document.addEventListener(t,function(){if(G.demoPend)rowDemoNow()},false)});
document.addEventListener("click",function(e){if(noDraw||!ed.contains(e.target))return;var b=e.target.closest("[data-set]");if(b){var k=b.dataset.set.replace(/^@/,"");if(S.ui.rowDemo!==false&&rowDemoOf(k))rowDemoSoon(k);return}
  var lb=e.target.closest(".row > label.rd");if(lb){var r=rowKey(lb.parentNode);if(r){setCtx(lb.parentNode);showRowDemo(r)}}},true);
document.addEventListener("change",function(e){if(noDraw||S.ui.rowDemo===false||!ed.contains(e.target))return;var t=e.target,k=t.dataset&&(t.dataset.sel||t.dataset.num);if(!k)return;k=k.replace(/^@/,"");if(rowDemoOf(k))rowDemoSoon(k)},true);
/* 0 · Preview & Test > Show each row's demo: Off keeps the phone as it is when a value changes (the label's ▸ still shows it) */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;addRows(d,"s0",row("Show each row's demo",seg("ui.rowDemo",["true","false"],["On","Off"],String(S.ui.rowDemo!==false)),"On: a change to a row shows that row's own demo on the phone (its view, its state, an action, the part ringed). Off: the phone stays as it is; a press on a row's label (▸) still shows its demo.",true),"Preview language");return d.innerHTML});
on("setOpt.answer",function(key,v){if(key==="ui.rowDemo"){S.ui.rowDemo=v==="true";renderEditor();return true}});
/* a row with a demo has a pressable label */
on("renderEditor.after",function(){ed.querySelectorAll(".row > label").forEach(function(lb){var k=rowKey(lb.parentNode);if(k&&rowDemoOf(k)){lb.classList.add("rd");lb.title="Show it on the phone"}})});
/* ----- the self-test: every row of every tab has its demo (a row without one fails: NO DEMO), but the exemptions below ----- */
/* the rows that change nothing the Lab's phone can show, each with its reason (keep them rare: a row with an effect gets a demo) */
var ROW_DEMO_EXEMPT={
  "ui.single":"An Editor preference (many foldouts open or one at a time); not saved to the wall, nothing on the phone.",
  "ui.level":"An Editor preference (how deep the foldouts open); not saved to the wall, nothing on the phone.",
  "cardCfg.enabled":"Enable Detail Card Off (a marker's tap shows only its label): the Lab's markers always open the card, so Off shows nothing yet.",
  "cardCfg.backBtn":"The phone's own Back button (Android): the Lab's phone has none to press.",
  "cardCfg.earbuds":"Earbuds disconnecting is a system event the Lab cannot make; nothing to show on the phone.",
  "cardCfg.lockScreen":"The screen locking is a system event the Lab cannot make; nothing to show on the phone.",
  "cardCfg.audioOverlap":"Two sounds at once: the Lab's audio and video blocks make no sound, so what the other sound does cannot be shown.",
  "cardCfg.resume":"Reopening an audio or a story where the visitor left: the Lab keeps no playback position to show.",
  "cardCfg.keepAudio":"The mini player on the wall after the card closes: the Lab's audio block plays nothing, so there is no player to keep.",
  "ctl.bar.hide":"Hide while scrolling: the Lab's Navigation Bar does not slide away yet (a behaviour still to build).",
  "langs.opensIn":"The language at a first open on a visitor's phone: the Lab's phone always shows the Editor's preview language.",
  "langs.fallback":"Every word of the Lab's card has both languages, so no word ever falls back: nothing changes on the phone."};
function rowExempt(k){return ROW_DEMO_EXEMPT.hasOwnProperty(String(k||"").replace(/^@/,""))}
/* the rows without a demo in the tab drawn now: in the foldouts named (ids), or in the whole tab */
function rowsWithoutDemo(ids){var out=[],seen={},rows=ids?[].concat.apply([],ids.map(function(id){return[].slice.call(ed.querySelectorAll('[data-fo="'+id+'"] .row'))})):[].slice.call(ed.querySelectorAll(".row"));
  rows.forEach(function(r){var k=rowKey(r);if(k&&!seen[k]&&!rowDemoOf(k)&&!rowExempt(k)){seen[k]=1;var d=r.closest("details[data-fo]"),p=[];while(d){var s=d.querySelector(":scope > summary");p.unshift(s?s.textContent.replace(/\s+/g," ").trim():d.dataset.fo);d=d.parentElement&&d.parentElement.closest("details[data-fo]")}
    out.push({key:k,label:(r.querySelector("label")||{}).textContent||"",path:p.join(" › ")})}});return out}
/* each tab the self-test walks, every foldout open */
function rowsWithoutDemoAll(){var keep=[S.tab,S.ui.level,JSON.stringify(S.op)],out=[];S.ui.level=9;S.op={};ST_TABS.filter(stWalks).forEach(function(tab){S.tab=tab;renderEditor();rowsWithoutDemo().forEach(function(r){r.tab=tab;out.push(r)})});
  S.tab=keep[0];S.ui.level=keep[1];S.op=JSON.parse(keep[2]);return out}
on("selfTest.res",function(res){rowsWithoutDemoAll().forEach(function(r){res.push({tab:r.tab,key:"@"+r.key,v:"(its demo)",label:r.label,result:"NO DEMO (this row shows nothing on the phone when changed or opened: "+r.path+")"})});return res});
