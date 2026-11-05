/* Each element's demo content (S.demoC) and where a demo field lives; each Editor row's demo (rowDemos, showRowDemo). */
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
   A module declares the demos of its own rows, by the row's key: rowDemos({"guideCfg.x": spec}) (it joins the point rowDemo.spec).
   A change to the row, or a press on its label (▸), runs showRowDemo(key): the phone goes to the spec's view, sets its state, does its
   action and rings the part the row changes. A spec: view ("card", "story"…) and stop (the sheet's stop, "full" when not given);
   past (the card scrolled past the hero); wall (true: the wall; "open": Ask about the view open); tour (a step's id, or true: the
   start) and dock (the dock's height then); intro (the tour starts with Azu's introduction); scroll (a selector brought into view);
   act (a function: a tap, a line, a gesture) and wait (ms before it); ring (the part: selectors joined by "|", the first found is
   ringed, or a function giving them) and ringAfter (ms after the act). Unity: the POI Editor's row gets its
   preview action, which sets the Play-mode guide the same way (PlayMode Live Config) */
function rowDemos(map){on("rowDemo.spec",function(key){return map[key]})}
function rowDemoOf(key){return firstAnswer("rowDemo.spec",String(key||"").replace(/^@/,""))}
function rowKey(r){var c=r.querySelector('[data-set^="@"],[data-sel^="@"],[data-num^="@"]');return c?(c.dataset.set||c.dataset.sel||c.dataset.num).slice(1):null}
function showRowDemo(key){var sp=rowDemoOf(key);if(!sp||noDraw)return false;var my=G.demoSeq=(G.demoSeq||0)+1;clearTimeout(G.demoT);
  if(G.on)stopTour();if(typeof hideStopUi==="function")hideStopUi();S.settingsOpen=false;S.contentsOpen=false;G.fab.menu=false;G.cam.open=false;var o=ph.querySelector(".onb");if(o)o.remove();
  if(sp.wall){S.view="card";S.stop="closed";G.cam.open=sp.wall==="open";renderPhone(false)}else{S.view=sp.view||"card";S.stop=sp.stop||"full";renderPhone(false);if(sp.past)scr.scrollTop=typeof sp.past==="number"?sp.past:900}
  if(sp.tour){S.introSeen=!sp.intro;startTour();var j=typeof sp.tour==="string"?S.guideSteps.findIndex(function(s){return s.id===sp.tour}):-1;if(j>0){stopSpeech();G.i=j;runStep()}if(sp.dock){S.dockH=sp.dock;renderGuide()}}
  G.demoT=setTimeout(function(){if(G.demoSeq!==my)return;if(sp.scroll){var e=scr.querySelector(sp.scroll);if(e)scr.scrollTop=Math.max(0,e.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-scr.clientHeight/3)}
    if(sp.act)sp.act();G.demoT=setTimeout(function(){if(G.demoSeq===my)ringPart(key,typeof sp.ring==="function"?sp.ring():sp.ring)},sp.ringAfter!=null?sp.ringAfter:sp.act?300:60)},sp.wait!=null?sp.wait:sp.tour?900:400);return true}
/* the part, ringed for 2.4 s; body[data-rowdemo] says what was shown (the checks read it) */
function ringPart(key,ring){var sels=String(ring||"").split("|"),el=null,sel="";for(var i=0;i<sels.length&&!el;i++){sel=sels[i];el=sel&&ph.querySelector(sel)}
  document.body.dataset.rowdemo=key+"|"+(el?"found":"missing")+"|"+sel;if(!el)return;el.classList.remove("pfocus");void el.offsetWidth;el.classList.add("pfocus");clearTimeout(el._rdT);el._rdT=setTimeout(function(){el.classList.remove("pfocus")},2400)}
/* the triggers: a changed value (after the change is made) or a press on the row's label */
document.addEventListener("click",function(e){if(noDraw||!ed.contains(e.target))return;var b=e.target.closest('[data-set^="@"]');if(b){var k=b.dataset.set.slice(1);if(S.ui.rowDemo!==false&&rowDemoOf(k))setTimeout(function(){showRowDemo(k)},0);return}
  var lb=e.target.closest(".row > label.rd");if(lb){var r=rowKey(lb.parentNode);if(r)showRowDemo(r)}},true);
document.addEventListener("change",function(e){if(noDraw||S.ui.rowDemo===false||!ed.contains(e.target))return;var t=e.target,k=t.dataset&&(t.dataset.sel||t.dataset.num);if(!k||k[0]!=="@")return;k=k.slice(1);if(rowDemoOf(k))setTimeout(function(){showRowDemo(k)},0)},true);
/* 0 · Preview & Test > Show each row's demo: Off keeps the phone as it is when a value changes (the label's ▸ still shows it) */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;addRows(d,"s0",row("Show each row's demo",seg("ui.rowDemo",["true","false"],["On","Off"],String(S.ui.rowDemo!==false)),"On: a change to a row shows that row's own demo on the phone (its view, its state, an action, the part ringed). Off: the phone stays as it is; a press on a row's label (▸) still shows its demo.",true),"Preview language");return d.innerHTML});
on("setOpt.answer",function(key,v){if(key==="ui.rowDemo"){S.ui.rowDemo=v==="true";renderEditor();return true}});
/* a row with a demo has a pressable label */
on("renderEditor.after",function(){ed.querySelectorAll(".row > label").forEach(function(lb){var k=rowKey(lb.parentNode);if(k&&rowDemoOf(k)){lb.classList.add("rd");lb.title="Show it on the phone"}})});
/* the self-test: every row of 6 · Guide and 7 · Audiences has its demo (a row without one fails) */
function rowsWithoutDemo(ids){var out=[];ids.forEach(function(id){ed.querySelectorAll('[data-fo="'+id+'"] .row').forEach(function(r){var k=rowKey(r);if(k&&!rowDemoOf(k)&&!out.some(function(x){return x.key===k}))out.push({key:k,label:(r.querySelector("label")||{}).textContent||""})})});return out}
on("selfTest.res",function(res){var keep=S.tab;S.tab="card";renderEditor();rowsWithoutDemo(["s7","s8"]).forEach(function(r){res.push({tab:"card",key:"@"+r.key,v:"(its demo)",label:r.label,result:"NO DEMO (a row of 6 · Guide or 7 · Audiences shows nothing on the phone when changed or opened)"})});S.tab=keep;return res});
