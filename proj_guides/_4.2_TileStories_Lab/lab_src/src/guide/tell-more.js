/* Tell me more (guidance round 17; the developer's plan, section 5, 2026-11-11): short or deep per step, at the visitor's own pace. A step
   can have a deep part: the words its rule left out (the tour made from a card: a text's further sentences beyond the cap, the facts beyond
   three, the pictures' own captions; guide/autotour.js makes them, never a fact twice with every deep part heard), or a written step's own
   "more" words (Specific Marker › 3 · Guide script › Tell me more). While a step with a deep part plays, a chip above the guide panel says
   Tell me more: a tap says the deep part after the step's line (at once when the line has ended), then the tour goes on; the panel's Next is
   Skip ahead. The pace, for the tour: 3 Tell me more in a row and the deep part follows each line by itself (the chip says More coming; a
   tap on it lets that step go short); 3 Skip ahead in a row and the tour is lighter again (no deep part first). Start deep for: no one,
   Tour lovers (the guide's length Deep: Audiences, or the visitor's own Settings) or everyone. Ask Azu's "tell me more" (typed or spoken)
   does what the chip does. The tour's length is the short version's minutes (the deep parts are the visitor's choice); the Editor's step
   list shows both. Not the games' Short / Deep (Guide › 3 · Games on the tour): its own keys (guideCfg.tm*).
   Unity: GuideTourBuilder's deep units per step (plain C#, the same fact keys, EditMode-tested on the same cards); a written step's More
   (a LocalizedString in the GuideScript asset); GuideTourPlayer.Pace (short / deep) kept for the tour; the chip a UI Toolkit Button above
   the guide panel (the panel's own USS); the phrase in the Ask path's intent match before the model */
S.guideCfg.tmOn=true;S.guideCfg.tmStart="none";
var TM_ROW=3;
COMP.s7more=["Guide","Tell me more"];FOCUS.s7more=["card",".hero","full"];
G.tm={pace:"short",more:0,skip:0,want:-1,playing:false,done:{},keys:{},said:{},tok:0,heard:[],log:[]};
function tmLog(t){G.tm.log.push(t);if(G.tm.log.length>40)G.tm.log.shift()}
/* a tour lover: the guide's length is Deep (Audiences › Looks per audience, or the visitor's own Settings) */
function tmLover(){return typeof lenNow==="function"&&lenNow()==="deep"}
function tmStartPace(){var s=S.guideCfg.tmStart;return s==="all"||s==="lover"&&tmLover()?"deep":"short"}
/* a step's deep part in the visitor's language: a written step's own more words, else the tour from the card's; the units already heard in
   an earlier deep part of this tour left out (no fact twice, no sentence twice); null when there is none */
function tmDeepOf(st,lang){if(!st||/^(teaser|end|intro|recap)$/.test(st.act))return null;lang=lang||S.lang;var d=null,mo=st.more&&String(wordIn(st.more,lang)||"").trim();
  if(mo)d={kind:"sents",units:atSentences(mo).map(function(t){return{t:t,k:atFactKeys(t,lang,{})}})};
  else if(st.at&&st.at.deeps&&st.at.deeps[lang])d=st.at.deeps[lang];
  if(!d||!d.units||!d.units.length)return null;
  var u=d.units.filter(function(x){var n=atNorm(x.t);return n&&!G.tm.said[n]&&!(x.k&&x.k.length&&x.k.every(function(k){return atKnown(G.tm.keys,k)}))});if(!u.length)return null;
  return{kind:d.kind,units:u,lead:d.lead||null,text:u.length===d.units.length&&d.text?d.text:atDeepText({kind:d.kind,units:u,lead:d.lead},lang,function(k){return wordIn(S.tx[k],lang)})}}
function tmDeep(st){return S.guideCfg.tmOn?tmDeepOf(st):null}
function tmCur(){return G.on?S.guideSteps[G.i]:null}
/* the step's line still being said (the device voice, a recorded or live audio) */
function tmSpeaking(){return!!(G.utter||G.recAudio||G.timer&&!G.waiting)}
/* Tell me more (the chip, or Ask Azu's phrase): the deep part comes after the step's line, at once when the line has ended; a tap on the chip
   while it says More coming lets this step go short */
function tmMore(how){var st=tmCur(),d=tmDeep(st),i=G.i;if(!d||G.tm.playing||G.tm.done[i])return false;
  if(how==="tap"&&(G.tm.want===i||G.tm.pace==="deep"&&G.tm.want!==-2)){G.tm.want=-2;G.tm.done[i]=true;tmLog("step "+(i+1)+": short this time (the chip)");tmChip();return true}
  G.tm.more++;G.tm.skip=0;G.tm.want=i;tmLog("step "+(i+1)+": Tell me more ("+(how||"tap")+")");
  if(G.tm.more>=TM_ROW&&G.tm.pace!=="deep"){G.tm.pace="deep";tmLog("the pace: deep ("+TM_ROW+" Tell me more in a row)")}
  if(G.paused){G.askOpen=false;resumeTour();tmChip();return true}
  if(G.waiting){G.waiting=false;clearTimeout(G.timer);G.timer=null;next();return true}
  if(!tmSpeaking()&&!G.tryWait){next();return true}
  tmChip();return true}
/* the deep part said: its words in the panel and the captions, then the tour goes on (the older joiners of next: the games' seams) */
function tmPlay(st,d){var i=G.i,tok=++G.tm.tok;G.tm.playing=true;G.tm.at=i;G.tm.done[i]=true;G.tm.want=-1;G.tm.heard.push(st.id);
  d.units.forEach(function(u){G.tm.said[atNorm(u.t)]=1;(u.k||[]).forEach(function(k){G.tm.keys[k]=1})});G.tm.text=d.text;tmLog("step "+(i+1)+": the deep part said");
  renderGuide(d.text);tmChip();
  say(d.text,function(k){markWord(k,null)},function(){if(tok!==G.tm.tok||!G.on||G.paused)return;G.tm.playing=false;tmChip();
    setTimeout(function(){if(tok===G.tm.tok&&G.on&&!G.paused&&G.i===i&&!G.tm.playing)next()},700)})}
/* the tour moving on: the deep part first when asked (or at the deep pace); Skip ahead counts toward a lighter tour */
on("next.answer",function(how){if(!G.on)return;var i=G.i,st=tmCur();
  if(how==="skip"){var off=G.tm.playing||!!tmDeep(st)&&!G.tm.done[i];if(G.tm.playing){G.tm.playing=false;G.tm.tok++}if(G.tm.want===i)G.tm.want=-1;
    if(off){G.tm.skip++;G.tm.more=0;tmLog("step "+(i+1)+": Skip ahead");if(G.tm.skip>=TM_ROW&&G.tm.pace==="deep"){G.tm.pace="short";tmLog("the pace: lighter ("+TM_ROW+" Skip ahead in a row)")}}return}
  if(G.tm.playing)return true;
  var d=tmDeep(st);if(!d||G.tm.done[i])return;
  if(G.tm.want===i||G.tm.pace==="deep"&&G.tm.want!==-2){tmPlay(st,d);return true}
  /* offered and let go: the streaks start again */
  G.tm.more=0;G.tm.skip=0});
on("startTour.before",function(from){var keep=from&&G.tm;G.tm={pace:keep?G.tm.pace:tmStartPace(),more:0,skip:0,want:-1,playing:false,done:{},keys:keep?G.tm.keys:{},said:keep?G.tm.said:{},tok:(G.tm.tok||0)+1,heard:keep?G.tm.heard:[],log:keep?G.tm.log:[]};
  if(!keep&&G.tm.pace==="deep")tmLog("the pace: deep from the start (Start deep for: "+(S.guideCfg.tmStart==="all"?"everyone":"a tour lover")+")")});
on("runStep.after",function(){var i=G.i;if(G.tm.want!==i&&G.tm.want!==-1)G.tm.want=-1;
  /* the step told again (a pause with nothing to resume, Previous): its deep part again after it, when it was playing */
  if(G.tm.playing&&G.tm.at===i){G.tm.playing=false;G.tm.tok++;G.tm.done[i]=false;G.tm.want=i}else if(G.tm.playing){G.tm.playing=false;G.tm.tok++}tmChip()});
on("stopTour.after",function(){G.tm.playing=false;G.tm.tok++;G.tm.want=-1;tmChip()});
/* while the deep part is said, the panel, the bubble and the captions keep its words (a redraw with no words of its own, the panel's height
   changed or Compact, would show the step's line under the voice) */
on("renderGuide.after",function(text){if(G.on&&G.tm.playing&&G.tm.at===G.i&&text==null&&G.tm.text&&G.tpText!==G.tm.text){renderGuide(G.tm.text);return}tmChip()});
/* Ask Azu: "tell me more" during a step with a deep part, typed or spoken, does what the chip does (never the model) */
function tmAsks(q){var t=atNorm(q);if(!t)return false;var ps=[S.lang,"en"].map(function(l){return wordIn(S.tx.tm_ask,l)}).join("|").split("|");return ps.some(function(p){p=atNorm(p);return p&&(t===p||t.indexOf(p+" ")===0)})}
on("ask.answer",function(q){if(!G.on||!q||!tmAsks(q))return;var st=tmCur();if(!tmDeep(st)||G.tm.done[G.i]||G.tm.playing)return;
  if(G.thread)G.thread.push({who:"me",text:String(q).trim()});G.askOpen=false;G.typing=false;if(S.dockH==="peek"||S.dockH==="full")S.dockH="mini";
  var gq=ph.querySelector("#gq");if(gq)gq.value="";tmLog("Ask the mascot heard “"+String(q).trim()+"”");if(G.tm.want===G.i){if(G.paused)resumeTour();return true}tmMore("ask");renderGuide();return true});
/* ----- the chip: above the guide panel (at Compact, beside the ✦ on its row), left; 44 px, 4.5:1; only while the step has more ----- */
function tmChip(){var c=ph.querySelector(".gtmore"),st=tmCur(),d=G.on&&!G.tm.playing&&!G.askOpen&&S.dockH!=="full"&&!G.tm.done[G.i]?tmDeep(st):null;
  if(!d){if(c)c.remove();return}var on=G.tm.want===G.i||G.tm.pace==="deep";
  if(!c){c=document.createElement("button");c.type="button";c.className="gtmore";c.dataset.tm="more";ph.appendChild(c)}
  c.classList.toggle("on",on);c.setAttribute("aria-pressed",String(on));
  var h='<span class="tmi" aria-hidden="true">'+(on?'<svg viewBox="0 0 16 16"><path d="M3.5 8.4l3 3 6-6.4"/></svg>':'<svg viewBox="0 0 16 16"><path d="M8 3.5v9M3.5 8h9"/></svg>')+'</span><span class="tmt">'+TXH(on?"tm_more_on":"tm_more")+'</span>';if(c.innerHTML!==h)c.innerHTML=h;tmPlace(c)}
/* at Compact on the ✦'s row, beside the ✦ and its Pause (on the other side when the ✦ is on the right); else 10 px above the panel, its hide
   tab and Azu's bubble, whichever is highest */
function tmPlace(c){var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight||1,H=ph.offsetHeight,gb=ph.querySelector(".gbtn-wrap"),seen=function(e){return!!e&&e.getClientRects().length>0&&getComputedStyle(e).display!=="none"&&getComputedStyle(e).visibility!=="hidden"};
  if(S.dockH==="compact"&&seen(gb)){var r=gb.getBoundingClientRect(),rr=r.right;[].forEach.call(gb.querySelectorAll("button"),function(x){if(!seen(x))return;var b=x.getBoundingClientRect();if(b.width&&b.right>rr)rr=b.right});
    c.style.left=((r.left-pr.left)/q<ph.offsetWidth/2?Math.round((rr-pr.left)/q+12):16)+"px";c.style.bottom=Math.round(H-(r.bottom-pr.top)/q+Math.max(0,(r.height/q-44)/2))+"px";return}
  var top=H-16;[".gdock",".gd-hide",".gbubble"].forEach(function(s){var e=ph.querySelector(s);if(!seen(e))return;var b=e.getBoundingClientRect();if(b.height)top=Math.min(top,(b.top-pr.top)/q)});
  c.style.left="16px";c.style.bottom=Math.round(H-top+10)+"px"}
setInterval(function(){var c=ph.querySelector(".gtmore");if(c&&!noDraw)tmPlace(c)},250);
ph.addEventListener("click",function(e){var b=e.target.closest("[data-tm]");if(!b)return;e.stopPropagation();tmMore("tap")},true);

/* ----- the Editor: Guide › 2 · The tour › Tell me more; a written step's Tell me more words in Specific Marker › 3 · Guide script ----- */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var at=d.querySelector('[data-fo="s7auto"]');if(!at)return d.innerHTML;
  var body=row("Tell me more",pseg("guideCfg.tmOn",["true","false"],["On","Off"]),"On: while a step with more to tell plays, the guide panel shows Tell me more (only there); a tap says the step's deep part after its line, then the tour goes on. The panel's Next is Skip ahead. A step's deep part: from the card, the words its rule left out (a text's further sentences beyond the cap, the facts beyond three, the pictures' own captions), never a fact the tour says elsewhere; in a written tour, its Tell me more words (Specific Marker › 3 · Guide script). Ask the mascot's “tell me more” (typed or spoken) does the same. Off: no chip, no deep parts, Next is Skip ahead all the same.")+
    row("Start deep for",psel("guideCfg.tmStart",[["none","No one: each visitor finds their pace"],["lover","Tour lovers (the guide's length Deep)"],["all","Everyone"]]),"The pace a tour starts at. Deep: each step's deep part follows its line by itself (the chip says More coming; a tap on it keeps that step short). Tour lovers: a visitor whose guide's length is Deep (Audiences › Looks per audience, the Expert's by default, or their own Settings › The guide). The visitor's own pace then takes over: 3 Tell me more in a row make it deep, 3 Skip ahead in a row make it lighter again.",true)+
    '<div class="note fixed">Fixed rules: the chip only where a step has more to tell; the visitor\'s pace is kept for the tour (3 in a row either way); no fact twice, with every deep part heard (a deep part never tells a fact a later step says, nor one an earlier deep part said); the tour\'s length everywhere (the hero, the ✦ menu, the invitation, the teaser) is the short version\'s minutes, the step list shows both. Not the games\' Short / Deep (3 · Games on the tour): their rhythm, not the words.</div>'+
    '<div class="note">Unity: GuideTourBuilder adds each step\'s deep units (plain C#, the same fact keys); a written step\'s More is a LocalizedString in its GuideScript asset; GuideTourPlayer keeps the pace for the tour; the chip is a UI Toolkit Button above the guide panel.</div>';
  at.insertAdjacentHTML("afterend",comp("s7more","the visitor's own pace: a step's deeper words, when asked",body));var f=innerOf(d,"s7more");if(f)f.insertAdjacentHTML("beforeend",ownbar("s7more")+ownPanels("s7more"));return d.innerHTML});
on("edMarker.html",function(h){var d=document.createElement("div");d.innerHTML=h;var m3=innerOf(d,"m3");if(!m3)return d.innerHTML;var all=S.fullSteps||S.guideSteps;
  m3.querySelectorAll('tr.pick[data-pick="gstep"]').forEach(function(r){var s=byId(all,r.dataset.id),mo=s&&s.more&&String(L(s.more)||"").trim();if(!mo)return;var c=r.querySelector("td:last-child");if(c)c.insertAdjacentHTML("beforeend",'<div class="atmore"><small>Tell me more</small> '+esc(mo)+'</div>')});
  var st=S.sel.gstep&&byId(all,S.sel.gstep),slot=st&&m3.querySelector('[data-pick="gstep"][data-id="'+cssq(st.id)+'"] + tr.slot td:last-child');
  /* under the step's own words (its Says boxes, or the note that it reads the block's own words), before the games' fields */
  var anchor=slot?[].filter.call(slot.querySelectorAll(":scope > .row, :scope > .note"),function(x){var l=x.querySelector(":scope > label");return x.classList.contains("note")&&/^Reads the block/.test(x.textContent)||!!l&&/^(Says · |Order$)/.test(l.textContent)}).pop():null;
  if(slot&&!/^(teaser|end)$/.test(st.act))(anchor||slot).insertAdjacentHTML(anchor?"afterend":"beforeend",langCodes().map(function(lg,j){return row("Tell me more · "+lg.toUpperCase(),'<textarea class="u" rows="2" data-gmore="'+esc(st.id)+'|'+lg+'" placeholder="No deeper words: this step has no Tell me more">'+esc(st.more&&st.more[lg]||"")+'</textarea>',j?"":"This step's deep part: said after its line when the visitor taps Tell me more (or at their deep pace), then the tour goes on. Empty: no Tell me more at this step.")}).join(""));
  return d.innerHTML});
ed.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.gmore)return;e.stopImmediatePropagation();var q=t.dataset.gmore.split("|"),st=byId(S.fullSteps||S.guideSteps,q[0]);if(!st)return;st.more=st.more||{};st.more[q[1]]=t.value},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(tmOn|tmStart|"));
/* each row's demo: the tour at its first step with more to tell, at Mini: the chip (Tell me more); Start deep for: the deep part playing */
function tmDemoAt(play){var gen=S.guideSteps.some(function(s){return!!tmDeepOf(s)})?null:atSteps(),list=gen||S.guideSteps,st=list.filter(function(s){return!!tmDeepOf(s)})[0];
  if(!st){if(G.on)stopTour();S.view="card";S.stop="full";renderPhone(false);if(typeof gdChip==="function"){gdChip("No step of this tour has more to tell");clearTimeout(G.tmChipT);G.tmChipT=setTimeout(function(){gdChip(null)},3200)}return}
  if(gen)atDemoAt(null,st.id);else{if(G.on)stopTour();S.view=st.view;S.stop="full";S.introSeen=true;G.demoTour=true;startTour({id:st.id});G.demoTour=false}
  S.dockH="mini";renderGuide();if(play&&S.guideCfg.tmOn){stopSpeech();G.tm.want=G.i;next()}}
rowDemos({"guideCfg.tmOn":{act:function(){tmDemoAt(false)},ringAfter:700,ring:".gtmore|.gdock"},"guideCfg.tmStart":{act:function(){tmDemoAt(true)},ringAfter:700,ring:".gdock|.gcap"}});
