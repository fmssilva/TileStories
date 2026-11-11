/* Azu in 2D: depth, feet, moods, the mouth by the letters, blinks; play (poke, tickle, a fall, a cookie, a water break). */
function moveMascot(el){var m=ph.querySelector(".mascot");if(!m)return;var p=ph.getBoundingClientRect(),k=p.width/ph.offsetWidth,y=120;
  if(el){var r=el.getBoundingClientRect();y=besideWords(m,Math.max(70,Math.min(ph.offsetHeight-210,(r.top-p.top)/k+10)),70,ph.offsetHeight-210,el)}m.style.top=y+"px";m.classList.remove("hop");void m.offsetWidth;m.classList.add("hop")}

/* ----- the guide's floating controls, captions, mascot and Ask ----- */
var MASCOT='<svg viewBox="0 0 64 64" aria-hidden="true"><rect x="6" y="8" width="52" height="50" rx="10" fill="#F4EEE3" stroke="#1F4E9C" stroke-width="3"/><path d="M6 18H18A10 10 0 0 1 6 28ZM58 18V28A10 10 0 0 1 46 18Z" fill="#1F4E9C" opacity=".55"/><path d="M32 14C36 19 36 22 32 25C28 22 28 19 32 14Z" fill="#1F4E9C"/><circle cx="23" cy="36" r="4.5" fill="#1A1814"/><circle cx="41" cy="36" r="4.5" fill="#1A1814"/><circle cx="24.5" cy="34.5" r="1.4" fill="#fff"/><circle cx="42.5" cy="34.5" r="1.4" fill="#fff"/><path class="mouth" d="M26 46Q32 51 38 46" stroke="#1A1814" stroke-width="3" fill="none" stroke-linecap="round"/><circle cx="16" cy="44" r="3" fill="#E79AB7" opacity=".6"/><circle cx="48" cy="44" r="3" fill="#E79AB7" opacity=".6"/></svg>';
S.guideCfg.life="lively";S.guideCfg.azuTap="play";S.guideCfg.lips=true;S.guideCfg.moods=true;S.m=S.m||{};S.m.solemn=false;
/* Azu in 2.5D: a tile with its thickness behind it, light from the top left, feet and a soft shadow; the face's parts are named so they can move */
MASCOT='<svg viewBox="0 0 64 74" aria-hidden="true"><defs><linearGradient id="azg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#FFFFFF"/><stop offset=".55" stop-color="#F4EEE3"/><stop offset="1" stop-color="#E2D6C1"/></linearGradient></defs>'+
  '<ellipse class="az-shadow" cx="32" cy="70.5" rx="17" ry="2.6" fill="rgba(0,0,0,.2)"/>'+
  '<g class="az-feet"><ellipse cx="23" cy="65" rx="6" ry="3.6" fill="#1F4E9C"/><ellipse cx="41" cy="65" rx="6" ry="3.6" fill="#1F4E9C"/></g>'+
  '<g class="az-body"><rect x="9.5" y="11" width="52" height="50" rx="10" fill="#173E7E"/><rect x="6" y="8" width="52" height="50" rx="10" fill="url(#azg)" stroke="#1F4E9C" stroke-width="3"/>'+
  '<path d="M6 18H18A10 10 0 0 1 6 28ZM58 18V28A10 10 0 0 1 46 18Z" fill="#1F4E9C" opacity=".55"/><path d="M32 14C36 19 36 22 32 25C28 22 28 19 32 14Z" fill="#1F4E9C"/>'+
  '<path d="M12 12Q20 9.5 28 11" stroke="#fff" stroke-width="2" fill="none" stroke-linecap="round" opacity=".9"/>'+
  '<g class="az-brows"><path class="az-brow l" d="M18.5 28.5Q23 26.5 27.5 28.5"/><path class="az-brow r" d="M36.5 28.5Q41 26.5 45.5 28.5"/></g>'+
  '<g class="az-eyes"><g class="az-eye"><circle cx="23" cy="36" r="4.5" fill="#1A1814"/><circle cx="24.5" cy="34.5" r="1.4" fill="#fff"/></g><g class="az-eye"><circle cx="41" cy="36" r="4.5" fill="#1A1814"/><circle cx="42.5" cy="34.5" r="1.4" fill="#fff"/></g></g>'+
  '<path class="mouth" d="M26 46Q32 51 38 46" stroke="#1A1814" stroke-width="3" fill="none" stroke-linecap="round" stroke-linejoin="round"/>'+
  '<circle class="az-cheek" cx="16" cy="44" r="3" fill="#E79AB7" opacity=".6"/><circle class="az-cheek" cx="48" cy="44" r="3" fill="#E79AB7" opacity=".6"/>'+
  '<path class="az-armr" d="M58.5 40Q64 44 62 50" stroke="#1F4E9C" stroke-width="4.5" fill="none" stroke-linecap="round"/></g></svg>';
/* mouth shapes: a flipbook, as an animated character's visemes; the open ones are filled */
var MOUTH={rest:"M26 46Q32 51 38 46",A:"M25.5 45Q32 56 38.5 45Q32 48 25.5 45Z",E:"M24.5 46Q32 51.5 39.5 46Q32 47.5 24.5 46Z",O:"M29 47.5a3 3.6 0 1 0 6 0a3 3.6 0 1 0 -6 0Z",M:"M27 47.5L37 47.5",
  big:"M24 44.5Q32 55 40 44.5Z",serious:"M27.5 48.5Q32 47 36.5 48.5",surprised:"M29 47a3 4.2 0 1 0 6 0a3 4.2 0 1 0 -6 0Z",thinking:"M27 48Q31 46.5 37 47.5",chew:"M27 47Q32 49.5 37 47"};
var MOODS={smile:"Smile",big:"Delighted",surprised:"Surprised",thinking:"Thinking",serious:"Serious"};
/* azMouth: the drawn mouth (azMouth2d), then every other Azu that follows it (azMouth.after) */
function azMouth(m,shape){azMouth2d(m,shape);emit("azMouth.after",m,shape)}
function azMouth2d(m,shape){var p=m&&m.querySelector(".mouth");if(!p)return;var d=MOUTH[shape]||MOUTH.rest,open=/Z$/.test(d);p.setAttribute("d",d);p.setAttribute("fill",open?"#7A2333":"none");p.setAttribute("stroke-width",open?"2":"3")}
function azMood(){var st=S.guideSteps[G.i];if(!S.guideCfg.moods)return"smile";if(S.m.solemn)return st&&st.mood==="big"?"smile":(st&&st.mood==="surprised"?"thinking":"serious");return(st&&st.mood)||"smile"}
function restShape(md){return{big:"big",serious:"serious",surprised:"surprised",thinking:"thinking"}[md]||"rest"}
/* the demo point's moods: the loss in 1755 is serious, the fun fact surprising, the quiz thoughtful, the end delighted */
(function(){var md={g1:"serious",g2:"surprised",g7:"serious",g9:"thinking",g10:"big"};S.guideSteps.forEach(function(s){if(md[s.id])s.mood=md[s.id]})})();
function azApply(){var m=ph.querySelector(".mascot");if(!m)return;var md=azMood();m.dataset.mood=md;m.classList.toggle("lively",S.guideCfg.life==="lively");if(!G.lipT)azMouth(m,restShape(md))}
/* the mouth follows the words: each word's letters give 1-3 shapes over the word's time (the voice's own visemes in the app) */
function shapesOf(w){var out=[],m={a:"A",á:"A",à:"A",ã:"A",â:"A",e:"E",é:"E",ê:"E",i:"E",í:"E",o:"O",ó:"O",ô:"O",õ:"O",u:"O",ú:"O",m:"M",b:"M",p:"M"};
  String(w).toLowerCase().replace(/[^\p{L}]/gu,"").split("").forEach(function(c){var s=m[c];if(s&&out[out.length-1]!==s)out.push(s)});if(!out.length)out.push("E");
  if(out.length>3)out=[out[0],out[Math.floor(out.length/2)],out[out.length-1]];return out}
on("markWord.after",function(k){if(!S.guideCfg.lips||S.reduce||!G.words[k])return;var m=ph.querySelector(".mascot");if(!m)return;
  (G.lipT||[]).forEach(clearTimeout);var sh=shapesOf(G.words[k].w),dur=Math.max(120,wordMs(k)),per=dur/(sh.length+.6);
  G.lipT=sh.map(function(s,i){return setTimeout(function(){azMouth(ph.querySelector(".mascot"),s)},i*per)}).concat([setTimeout(function(){G.lipT=null;azMouth(ph.querySelector(".mascot"),restShape(azMood()))},sh.length*per)])});
/* blinks: every 2.5-5 s, never in sync with anything */
function azBlink(){clearTimeout(G.blinkT);G.blinkT=setTimeout(function(){var m=ph.querySelector(".mascot");if(m&&!S.reduce){m.classList.add("blink");setTimeout(function(){m.classList.remove("blink")},140)}if(G.on)azBlink()},2500+Math.random()*2500)}
on("renderGuide.after",function(){var m=ph.querySelector(".mascot");if(!m)return;m.setAttribute("role","button");m.setAttribute("aria-label",TXT("mascot_name"));m.tabIndex=-1;azApply()});
on("startTour.before",function(){G.moment=null;G.talkFrom=performance.now()});on("startTour.after",function(){azBlink()});

/* ----- play: what the visitor starts is never limited; what Azu starts by itself is the guidance's (Guide › Guidance › Fun, round 7): the
   joke after pokes (Fun › Play › A joke) and the Moments (Fun › Moments: the water break, the cookie) are its items, their timers gone;
   their look stays here (azBubble, azMoment). Unity: the props and Azu's reactions are the Rive file's states ----- */
/* the Azu on the screen: the guide's (during a tour) or the one beside the guide's button (world/guide-button.js); both play the same */
function azuEl(){return ph.querySelector(".mascot")||ph.querySelector(".azfab")}
function azBubble(txt,ms){var m=azuEl();if(!m)return;var o=ph.querySelector(".azbub");if(o)o.remove();var b=document.createElement("div");b.className="azbub";b.textContent=txt;b.setAttribute("role","status");
  var mid=m.classList.contains("center");b.classList.toggle("mid",mid);ph.appendChild(b);
  if(m.classList.contains("azfab")){b.classList.add("byfab");placeNear(b)}else b.style.top=(mid?ph.offsetHeight*.42-48:m.offsetTop-40)+"px";clearTimeout(G.bubT);G.bubT=setTimeout(function(){b.remove()},ms||1800)}
function azReact(cls,ms,mood){var m=azuEl();if(!m)return;m.classList.remove("r-giggle","r-spin","r-jump","r-wink","r-blush","r-fall","r-laugh","r-eat","r-drink");void m.offsetWidth;
  if(!S.reduce||cls==="r-wink"||cls==="r-blush")m.classList.add(cls);if(mood)azMouth(m,mood);clearTimeout(G.reactT);G.reactT=setTimeout(function(){m.classList.remove(cls);azApply()},ms||800)}
function haptic(){try{if(navigator.vibrate)navigator.vibrate(12)}catch(e){}}
var POKES=["r-giggle","r-spin","r-jump","r-wink","r-blush","hey"];
function azPoke(){haptic();var calm=S.guideCfg.life!=="lively",pool=calm?["r-wink","r-blush"]:POKES.filter(function(x){return(G.lastPokes||[]).indexOf(x)<0}),r=pool[Math.floor(Math.random()*pool.length)];
  G.lastPokes=((G.lastPokes||[]).concat([r])).slice(calm?-1:-3);G.pokes=(G.pokes||0)+1;
  if(r==="hey"){azReact("r-jump",700,"surprised");azBubble(TXT("mascot_hey"))}else azReact(r,800,r==="r-wink"||r==="r-blush"?"big":"A");
  emit("mascot.poked",r);return r}
function azTickle(){haptic();azReact("r-laugh",1300,"big");azBubble(TXT("mascot_tickle"))}
function azFall(){haptic();azReact("r-fall",1700,"surprised");azBubble(TXT("mascot_oops"),1400)}
/* a moment: Azu shows a prop and asks for one tap; it never blocks (8 s, then the tour carries on). Its words: the guidance item's
   Says (words), else the prop's own. When it starts is the guidance's (Fun › Moments); its end is moment.ended.
   Round 11: Azu stays where it is (beside the words, never over them) and the prop is placed beside Azu with 0 px over any words (the
   card's, the panel's, a bubble's) and inside the phone; with no room it waits (tried again every 300 ms; 8 s with none: the moment
   ends unseen). Unity: the prop's rect tested against the glyph boxes (TextElement) and the panel's rect, the same candidates */
var PROP=54;
function propObstacles(){var out=typeof wordBoxes==="function"?wordBoxes(null).slice():[];ph.querySelectorAll(".gdock,.gbubble,.azbub,.gbtn-wrap,.gtoast,.gcoach,.gdbeat").forEach(function(e){if(!e.getClientRects().length)return;var r=phRect(e);out.push({l:r.l,t:r.t,r:r.l+r.w,b:r.t+r.h})});return out}
function propFree(x,y,obs){if(x<8||y<64||x+PROP>ph.offsetWidth-8||y+PROP>ph.offsetHeight-8)return false;return!obs.some(function(b){return b.r>x&&b.l<x+PROP&&b.b>y&&b.t<y+PROP})}
/* the nearest free place beside Azu: left or right of it, above or below, then a step further up or down */
function propSpot(){var m=azuEl();if(!m||!m.getClientRects().length)return null;var a=phRect(m),obs=propObstacles(),g=8,c=[];
  [0,-(PROP+g),PROP+g,-2*(PROP+g),2*(PROP+g)].forEach(function(dy){var y=a.t+(a.h-PROP)/2+dy;c.push([a.l-PROP-g,y],[a.l+a.w+g,y])});
  c.push([a.l+(a.w-PROP)/2,a.t-PROP-g],[a.l+(a.w-PROP)/2,a.t+a.h+g]);
  for(var i=0;i<c.length;i++)if(propFree(c[i][0],c[i][1],obs))return{x:Math.round(c[i][0]),y:Math.round(c[i][1])};return null}
/* round 12 (Fun › On the tour's breather): the prop by date (a glass, a fan, a scarf; the cookie), and o: {quiet: no bubble (the words are
   in the beat card), auto: Azu uses it by itself once it is placed (no tap asked)} */
var AZPROPS={water:'<svg viewBox="0 0 24 32" aria-hidden="true"><path d="M4 3H20L18 29H6Z" fill="rgba(170,215,255,.35)" stroke="#5B8FD6" stroke-width="2" stroke-linejoin="round"/><path class="az-water" d="M5.6 12H18.4L17.3 28H6.7Z" fill="#7FB8F0"/></svg>',
  cookie:'<svg viewBox="0 0 32 32" aria-hidden="true"><circle cx="16" cy="16" r="13" fill="#D9A55B" stroke="#9A6A2A" stroke-width="2"/><circle cx="11" cy="12" r="2" fill="#5B3716"/><circle cx="20" cy="10" r="1.8" fill="#5B3716"/><circle cx="19" cy="20" r="2.2" fill="#5B3716"/><circle cx="11" cy="21" r="1.6" fill="#5B3716"/></svg>',
  fan:'<svg viewBox="0 0 32 32" aria-hidden="true"><path d="M16 28L3 9A16 16 0 0 1 29 9Z" fill="#F3D9A4" stroke="#A85A2A" stroke-width="2" stroke-linejoin="round"/><path d="M16 28L9.5 5.5M16 28V4M16 28L22.5 5.5" stroke="#A85A2A" stroke-width="1.6"/><circle cx="16" cy="27" r="2.2" fill="#A85A2A"/></svg>',
  scarf:'<svg viewBox="0 0 32 32" aria-hidden="true"><path d="M5 8C12 4 20 4 27 8L25 13C19 10 13 10 7 13Z" fill="#C8463A" stroke="#7E2A22" stroke-width="1.8" stroke-linejoin="round"/><path d="M19 11L23 29H17L15 12Z" fill="#C8463A" stroke="#7E2A22" stroke-width="1.8" stroke-linejoin="round"/><path d="M17.6 25H22.4M16.8 21H21.4" stroke="#F4E3C7" stroke-width="1.6"/></svg>'};
var AZUSE={water:"r-drink",cookie:"r-eat",fan:"r-giggle",scarf:"r-blush"};
function azMoment(kind,then,words,o){o=o||{};G.moment=kind;G.momentQuiet=!!o.quiet;G.talkFrom=performance.now();words=words||TXT(kind==="cookie"?"mascot_hungry":"mascot_water");G.momentThen=then;G.propWaited=0;
  clearTimeout(G.momentT);clearInterval(G.propT);var t0=performance.now();
  var show=function(sp){var m=ph.querySelector(".mascot"),el=document.createElement("div");el.className="azmoment "+kind;el.style.left=sp.x+"px";el.style.top=sp.y+"px";
    el.innerHTML='<button type="button" class="azprop" data-azprop="'+kind+'" aria-label="'+esc(words||GD_PROP_NAMES[kind]||kind)+'">'+(AZPROPS[kind]||AZPROPS.water)+'</button>';
    ph.appendChild(el);G.propAt=sp;if(m)azMouth(m,"surprised");G.momentT=setTimeout(function(){azMomentEnd(false)},8000);if(o.auto)setTimeout(function(){if(G.moment===kind)azMomentDo()},700)};
  /* the words first (they ask for the tap), then the prop placed clear of every word, the bubble's included */
  var tryPlace=function(){if(G.moment!==kind){clearInterval(G.propT);return true}var sp=propSpot();if(sp){clearInterval(G.propT);show(sp);return true}
    G.propWaited=performance.now()-t0;if(G.propWaited>8000){clearInterval(G.propT);azMomentEnd(false);return true}return false};
  if(!o.quiet)azBubble(words,8000);if(!tryPlace())G.propT=setInterval(tryPlace,300)}
function azMomentDo(){var k=G.moment;if(!k)return;clearTimeout(G.momentT);haptic();var p=ph.querySelector(".azmoment");if(p)p.classList.add("using");azReact(AZUSE[k]||"r-drink",1500,k==="cookie"?"chew":"O");
  setTimeout(function(){if(!G.momentQuiet)azBubble(TXT(k==="cookie"?"mascot_thanks":"mascot_ahh"),1500);azMomentEnd(true)},1500)}
function azMomentEnd(done){var k=G.moment;if(!k)return;G.moment=null;clearTimeout(G.momentT);clearInterval(G.propT);var p=ph.querySelector(".azmoment");if(p)p.remove();var m=ph.querySelector(".mascot");if(m)m.classList.remove("center");
  var t=G.momentThen;G.momentThen=null;emit("moment.ended",k,!!done);setTimeout(function(){if(t&&G.on)t()},done?900:0)}
on("stopTour.before",function(){if(G.moment){G.momentThen=null;azMomentEnd(false)}clearTimeout(G.blinkT);(G.lipT||[]).forEach(clearTimeout);G.lipT=null;var b=ph.querySelector(".azbub");if(b)b.remove()});
/* the gestures on Azu (the guide's, or the one beside the guide's button): a tap does what Tap on Azu says (it plays: a poke), three
   quick taps tickle, a long press (0.5 s) knocks Azu over; they never pause the tour. A tap on Azu never opens the ✦'s menu unless asked */
var azDown=null;
function azTarget(e){var m=e.target.closest(".mascot,.azfab");return m&&(G.on||m.classList.contains("azfab"))?m:null}
ph.addEventListener("pointerdown",function(e){if(!azTarget(e))return;e.stopPropagation();azDown={t:performance.now()}},true);
ph.addEventListener("pointerup",function(e){if(!azTarget(e)||!azDown){azDown=null;return}e.stopPropagation();var held=performance.now()-azDown.t,now=performance.now();azDown=null;
  if(held>=500){azFall();return}G.taps=(G.taps||[]).filter(function(t){return now-t<1200}).concat([now]);if(G.taps.length>=3){G.taps=[];emit("azu.tickled");azTickle();return}if(firstAnswer("azu.tap",azTarget(e))!==undefined)return;azuTapDo(azuTapAct())},true);
ph.addEventListener("click",function(e){var p=e.target.closest("[data-azprop]");if(p){e.stopPropagation();azMomentDo();return}if(e.target.closest(".mascot,.azfab"))e.stopPropagation()},true);

/* Moments and Water break after (link rows to Fun › On the tour) are gone (guidance round 14): 3 · Games on the tour holds the breather,
   the cookie and their On; Azu's section keeps only its own rows */
/* Editor: Guide tab > Mascot gets its liveliness, mouth, moods, moments and a Try per play; Specific Marker > Guide script gets Solemn and each step's mood */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7mascot",row("Tap on Azu",pseg("guideCfg.azuTap",["play","menu","point"],["Play","Opens the menu","Points to the ✦ button"]),"Play: a tap on Azu (the guide's, or the one beside the guide's button) pokes it; it never opens the menu, so one gesture means one thing. Opens the menu: as before, a tap on Azu opens the ✦'s menu (during a tour it shows or hides the dock). Points to the ✦ button: Azu points at the ✦ and says “Need anything? Tap ✦”. Three quick taps and a long press always play. With Guide button = Azu only, Azu is the button and a tap opens the menu.")+
    row("Liveliness",pseg("guideCfg.life",["lively","calm"],["Lively","Calm"]),"Lively: Azu hops, reacts to every poke (6 reactions, never one of the last 3), sometimes tells a joke, and may stop once for a sip of water or a cookie. Calm: blinks, talks and points; a poke gets a wink; nothing unprompted.")+
    row("Mouth",pseg("guideCfg.lips",["true","false"],["Follows the words","Still"]),"The mouth changes shape with each word being said (from its letters here; from the voice's visemes in the app).")+
    row("Moods",pseg("guideCfg.moods",["true","false"],["On","Off"]),"Each step's mood (in the Guide script): smile, delighted, surprised, thinking, serious. On a Solemn point the face stays serious or thoughtful.")+
    row("Try",'<div class="btnrow"><button class="del" type="button" data-aztry="poke">Poke</button><button class="del" type="button" data-aztry="tickle">Tickle</button><button class="del" type="button" data-aztry="fall">Knock over</button><button class="del" type="button" data-aztry="water">Water break</button><button class="del" type="button" data-aztry="cookie">Cookie</button></div>',
      "On the phone: a tap pokes, three quick taps tickle, a long press knocks Azu over. They never pause the tour."));
  return d.innerHTML});
on("edMarker.html",function(h){var d=document.createElement("div");d.innerHTML=h;var f=innerOf(d,"m3");if(!f)return d.innerHTML;
  f.insertAdjacentHTML("afterbegin",row("Solemn point",pseg("m.solemn",["false","true"],["No","Yes"]),"A memorial, a church, a tragedy: Azu keeps a serious face, and no Fun shows on it (Guide › 3 · Games on the tour and 4 · Azu speaks up › Fun: no game, no joke, no hunt invitation, no seasonal line; fixed)."));
  var st=S.sel.gstep&&byId(S.guideSteps,S.sel.gstep),slot=st&&d.querySelector('[data-pick="gstep"][data-id="'+cssq(st.id)+'"] + tr.slot td:last-child');
  if(slot)slot.insertAdjacentHTML("afterbegin",row("Azu's mood",'<select class="u" data-azmood="'+st.id+'">'+Object.keys(MOODS).map(function(k){return'<option value="'+k+'"'+((st.mood||"smile")===k?" selected":"")+'>'+MOODS[k]+'</option>'}).join("")+'</select>',"Azu's face during this step."));
  return d.innerHTML});
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.azmood)return;e.stopImmediatePropagation();byId(S.guideSteps,t.dataset.azmood).mood=t.value;azApply()},true);
ed.addEventListener("click",function(e){var t=e.target.closest("[data-aztry]");if(!t)return;e.stopPropagation();var k=t.dataset.aztry;if(!G.on){startTour()}
  setTimeout(function(){if(k==="poke")azPoke();else if(k==="tickle")azTickle();else if(k==="fall")azFall();else{pauseTour();azMoment(k,function(){resumeTour()})}},G.on?50:900)},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(life|lips|moods|moments|waterAfter|").replace("^@?(","^@?(m\\.solemn|"));
/* each row's demo: Azu tapped once (beside the ✦); a poke in the tour; the mouth and the mood on step 2; a water break */
rowDemos({"guideCfg.azuTap":{past:true,act:function(){var a=ph.querySelector(".azfab");if(a)gsTouch(a)},ring:".gbmenu|.azfab"},"guideCfg.life":{tour:"g1",act:function(){azPoke()},ring:".mascot"},
  "guideCfg.lips":{tour:"g1",ring:".mascot"},"guideCfg.moods":{tour:"g1",ring:".mascot"}});
/* Solemn point's demo (Specific Marker › Guide script): the tour on this point, Azu's face (serious when Yes) ringed */
rowDemos({"m.solemn":{tour:"g1",ring:".mascot"}});
