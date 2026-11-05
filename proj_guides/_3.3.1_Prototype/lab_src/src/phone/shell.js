/* The phone: renderPhone, the screens' top bar, the demo screen, show and the phone's own taps. */
/* ---------- phone ---------- */
var scr=document.getElementById("scr"),ph=document.getElementById("ph"),hint=document.getElementById("hint"),fit=document.getElementById("fit");
var SCREENS={phone:[360,740],land:[740,360],tablet:[640,860]};
function topbar(crumb){var ans=firstAnswer("topbar.answer",crumb);if(ans!==undefined)return ans;return '<div class="topbar"><button type="button" data-go="card" aria-label="'+TXH("page_back")+'">&#8249;</button><span>Ribeira Palace &#8250; '+crumb+'</span></div>';}
function layerHead(t,s){return '<div class="layerhead">'+t+'<small>'+s+'</small></div>'}
function demoScreen(){var h=topbar("<b>"+esc(L(UI.demo))+"</b>")+'<div class="pg">';
  h+=layerHead("Motion","Each motion on real parts. Motion Family: <b>"+esc((motionById(S.family)||{}).name)+"</b> · Replay: <b>"+(S.replay==="every"?"Every Pass":"Once")+"</b>.");
  Object.keys(TYPES).forEach(function(t){S.motions.filter(function(m){return m.type===t}).forEach(function(m){h+=motionDemo(m)})});
  h+=layerHead("Interaction","Try each one: tap, hold, drag, swipe, scroll, tilt.");S.inter.forEach(function(x){h+=interDemo(x)});
  h+=layerHead("Effects","Edges, line dividers, light and moving effects (the Look Kit).");S.effects.forEach(function(f){h+=effectDemo(f)});
  h+=layerHead("Elements","Each element in each of its styles. ★ = the default a new row gets.");
  KINDS.forEach(function(K){(S.styles[K.k]||[]).forEach(function(st){h+='<div class="item" data-ref="s:'+st.id+'"><span class="tag">'+esc(K.n)+' · <b>'+esc(st.name)+(st.def?" ★":"")+'</b></span><div class="blk">'+renderEl(K.k,st,{},0)+'</div></div>'})});
  h+=layerHead("Patterns","Groups of elements with their styles; a marker fills the content.");
  S.patterns.forEach(function(p){h+='<div class="item" data-ref="p:'+p.id+'"><span class="tag">Pattern · <b>'+esc(p.name)+'</b></span><div class="blk">'+p.rows.map(function(r,i){return renderEl(r.kind,styleById(r.style)||defStyle(r.kind),{},i)}).join("")+'</div></div>'});
  return pipe("demoScreen.html",h+'</div>');}
/* the phone: the features before (latest first) and after (in module order), sharing the call's notes cx */
function renderPhone(keep){if(noDraw)return;var cx={};emitLatestFirst("renderPhone.before",keep,cx);var top=keep?scr.scrollTop:0,sz=SCREENS[S.screen];
  ph.style.width=sz[0]+"px";ph.style.height=sz[1]+"px";ph.classList.toggle("land",S.screen==="land");
  var k=Math.min(1,390/sz[0]);fit.style.transform="scale("+k+")";fit.style.height=(sz[1]*k)+"px";fit.style.width=(sz[0]*k)+"px";fit.style.transformOrigin="top left";
  document.getElementById("camera").style.backgroundImage=S.screen==="land"?"url("+IMG.hero+")":"none";
  ph.classList.toggle("dark",S.dark);ph.classList.toggle("reduce",S.reduce);
  scr.innerHTML=S.view==="card"?cardScreen():S.view==="demo"?demoScreen():S.view==="story"?storyScreen():S.view==="term"?termScreen():S.view==="democard"?demoCardScreen():galleryScreen();
  document.getElementById("ptool").innerHTML=ptool();
  var nb=document.getElementById("navbar");nb.innerHTML=navbar();nb.style.display=S.view==="card"?"":"none";
  hint.style.display=S.view==="card"?"":"none";hint.firstChild.textContent="The card · Ribeira Palace";
  splitPieces();scr.scrollTop=top;observe();initTransitions();initBoxes();onScroll(scr);applyFocus();
  emit("renderPhone.after",keep,cx)}
function applyFocus(){var on=S.focusOn&&S.view==="demo"&&S.focusRef;scr.classList.toggle("focusing",!!on);
  scr.querySelectorAll(".item.focus").forEach(function(x){x.classList.remove("focus")});if(on){var el=scr.querySelector('[data-ref="'+S.focusRef+'"]');if(el)el.classList.add("focus")}}
function flashRef(ref){if(noDraw)return;var el=scr.querySelector('[data-ref="'+ref+'"]');if(!el)return;var y=el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-70;
  scr.scrollTo({top:Math.max(0,y),behavior:S.reduce?"auto":"smooth"});if(S.view!=="demo"){el.classList.remove("flash");void el.offsetWidth;el.classList.add("flash")}}
function show(view,ref){if(noDraw){S.view=view;return}if(ref&&view==="demo")S.focusRef=ref;if(S.view!==view){S.view=view;renderPhone(false)}else applyFocus();if(ref)setTimeout(function(){flashRef(ref)},40)}
/* replay: jump back without a transition, then play forward */
function replayIn(root){var els=root.querySelectorAll(".mv");els.forEach(function(el){el.style.transition="none";el.classList.remove("in")});void root.offsetWidth;
  requestAnimationFrame(function(){els.forEach(function(el){el.style.transition=""});requestAnimationFrame(function(){els.forEach(function(el){el.classList.add("in")})})})}
ph.addEventListener("click",function(e){var t=e.target;
  var pt=t.closest("[data-pt]");if(pt){var k=pt.dataset.pt;if(k==="screen")S.screen=pt.dataset.v;else if(k==="lang")S.lang=S.lang==="en"?"pt":"en";else S[k]=!S[k];renderAll(true);return}
  var g=t.closest("[data-go]");if(g&&!t.closest(".kq")){S.view=g.dataset.go;renderPhone(false);return}
  var ex=t.closest("[data-expand]");if(ex){ex.classList.toggle("opened");return}
  var kq=t.closest(".kq button");if(kq){kq.classList.add(kq.dataset.ok?"ok":"no");return}
  var fl=t.closest("[data-flip]");if(fl){fl.classList.toggle("on");return}
  var au=t.closest("[data-audio] button");if(au){var a=au.parentNode;a.classList.toggle("playing");au.innerHTML=a.classList.contains("playing")?"&#10073;&#10073;":"&#9654;";au.setAttribute("aria-label",a.classList.contains("playing")?TXT("audio_pause"):TXT("audio_play"));return}
  var an=t.closest('[data-anim="tap"]');if(an){an.classList.remove("play");void an.offsetWidth;an.classList.add("play");return}
  var dr=t.closest("[data-door]");if(dr){var op=dr.dataset.open!=="1";dr.dataset.open=op?"1":"0";dr.querySelector(".l").style.transform=op?"rotateY(-75deg)":"";dr.querySelector(".r").style.transform=op?"rotateY(75deg)":"";return}
  var hsp=t.closest("[data-hspot]");if(hsp){var pop=hsp.parentNode.querySelector(".pop");var notes=[T("The Torreão: the corner tower of the 1580s.","O Torreão: a torre de canto da década de 1580."),T("The river front where the court landed.","A frente ribeirinha onde a corte desembarcava.")];pop.textContent=L(notes[+hsp.dataset.hspot]);pop.classList.add("on");return}
  var ch=t.closest("[data-c]");if(ch){var box=ch.closest("[data-choice]");box.querySelectorAll("[data-c]").forEach(function(x){x.setAttribute("aria-pressed",String(x===ch))});box.querySelector("p").textContent=L(ch.dataset.c==="0"?T("Manuel I moved the court to the river around 1500.","D. Manuel I mudou a corte para o rio por volta de 1500."):T("The royal library held some 70,000 books, lost in 1755.","A biblioteca real guardava cerca de 70 000 livros, perdidos em 1755."));return}
  var tb=t.closest("[data-turn]");if(tb){var tv=tb.parentNode.previousElementSibling;if(tv&&tv.classList.contains("trv"))turn(tv,+tb.dataset.turn);return}
  var rp=t.closest("[data-reveal-play]");if(rp){var row=rp.closest(".fx").querySelector(".tilerow"),ts=row.querySelectorAll(".t");ts.forEach(function(x){x.style.transition="none"});row.classList.add("hidden");void row.offsetWidth;
    requestAnimationFrame(function(){ts.forEach(function(x){x.style.transition=""});requestAnimationFrame(function(){row.classList.remove("hidden")})});return}
  var p=t.closest("[data-play]");if(p){var m=motionById(p.dataset.play),bx=p.closest(".mdemo");
    if(m.type==="entrance")replayIn(bx);else if(m.type==="transition")turn(bx.querySelector(".trv"),1);
    else if(m.type==="scroll"){var mb=bx.querySelector(".mbox");mb.scrollTop=0;setTimeout(function(){mb.scrollTo({top:mb.scrollHeight,behavior:"smooth"})},60)}
    else if(m.type==="imported"){var a2=bx.querySelector("[data-anim]");if(a2){a2.classList.remove("play","loop");a2.querySelectorAll(".draw").forEach(function(d){d.style.strokeDashoffset=""});void a2.offsetWidth;a2.classList.add("play")}var d2=bx.querySelector("[data-door]");if(d2)d2.click()}
    else{var em=bx.querySelector("[data-em]");em.style.animation="none";void em.offsetWidth;em.style.animation=""}}});
document.getElementById("replay").addEventListener("click",function(){scr.scrollTop=0;renderPhone(false);if(S.focusRef&&S.view==="demo")setTimeout(function(){flashRef(S.focusRef)},60)});
