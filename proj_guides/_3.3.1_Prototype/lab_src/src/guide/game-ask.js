/* Ask during a game (guidance round 18; the developer, 2026-11-11: "Ask Azu during a game (the game pauses, the answer, the game goes
   on)"; 2026-11-13: a question about the very question being played gets a hint, not the answer, "a gentle clue from the card's words
   that keeps the fun; the full answer comes after they answer"). One rule for every game on screen: the tour's quiz question, Guess the
   year, Don't say it!, the breather's question, Find it on the wall, and the card's own Knowledge Check and Poll outside a tour.
   1. The game pauses as Ask opens (the panel raised, the microphone, a question typed, the ✦ menu's or Tap on Azu's Ask, What next?'s
      field): its timers and its clock (guidance/present.js's gdBeatFreeze, the tour's own clock for Don't say it!), its answers, the
      slider and the score wait; the voice of the game stops at its word; a quiet "Game paused" chip (44 px) sits on the game; no tour
      restart, no scroll (the card's Ask keeps its place as round 17c's What next? does: gaCardEnter).
   2. Azu answers in the panel as Ask Azu does (the model, with the game and its rule in its context; offline, the card's words). An
      answer's block is offered as Show me instead of scrolling the card away (goTo.answer).
   3. The game goes on: "Back to the game" (44 px, the panel's primary style), or by itself N s after the answer is heard (Back by itself;
      a tap or a new question cancels it). Azu says a line of its intro lines › Back to the game (picked by the point, the next one each
      time: never twice in a row), then the game's clock goes on exactly where it stopped and the chip goes.
   4. A question about the question on screen (no AI: its content words shared with the question or its choices, gaAbout; the
      strictness in one place, GA_RATIO): the hint ladder, one step an ask: Where to look (the question's own written hint, else the
      card's block whose words hold the answer, with Show me), One wrong answer out (a wrong choice fades; Guess the year's band
      narrows), the visitor's choice (Tell me: the answer and its why, no star, the next question; I'll try: back). A question about
      anything else is answered as usual; a question already answered is answered in full.
   5. Off: "Let's talk about it once you've answered" on the game, and it never pauses. A Stop ends the game and Ask as Stop does; leaving
      the point closes Ask and keeps the game paused (Play, or the chip, goes back). Reduced motion: no fade.
   The Editor: Guide › 3 · Games on the tour › Asking during a game (s7gask); each quiz question's own hint in Specific Marker › 3 · Guide
   script beside the question (Draft with AI, Reviewed). Unity: GuideGameAsk (plain C#, EditMode-tested: the detection with its stop words
   and threshold, the ladder, the block finder over the card's CardStrings); the pause a GameClock every game's coroutines wait on (unscaled
   time summed while not paused), the chip and the Back button UI Toolkit elements (USS, 44 px), the voice an AudioSource paused and resumed
   at its word time (the device TTS from its word) ----- */
S.guideCfg.gaOn=true;S.guideCfg.gaBack=true;S.guideCfg.gaBackS=4;S.guideCfg.gaH1=true;S.guideCfg.gaH2=true;S.guideCfg.gaH3=true;S.guideCfg.gaStrict="normal";
COMP.s7gask=["Guide","Asking during a game"];FOCUS.s7gask=["card",".gdock","full"];
G.ga={on:false,lad:{},polled:{},started:{},k:0};
var GA_PAUSE='<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="7" y="5.5" width="3.4" height="13" rx="1.2"/><rect x="13.6" y="5.5" width="3.4" height="13" rx="1.2"/></svg>';
/* the block kinds' names when a block has no heading of its own (hint 1 names the place warmly) */
var GA_KIND={"Quick Facts":T("Quick facts","Factos rápidos"),"Before and After":T("Before and after","Antes e depois"),"Gallery":T("Gallery","Galeria"),"Timeline":T("Timeline","Cronologia"),
  "Practical Info":T("Practical info","Informações práticas"),"Dialogue":T("Dialogue","Diálogo"),"Inline":T("Fun fact","Curiosidade"),"Story":T("The story","A história"),"Status":T("Status","Estado"),"Video":T("Video","Vídeo")};

/* ----- the game on screen now: its kind, its question (words, choices, the right one, why, its own hint), answered or not ----- */
/* o.any: the card's quiz too before the visitor started it (idle: its hint ladder, no pause, no chip) */
function gaGame(o){var B=G.gd&&G.gd.bt,l=S.lang||"en",inTour=G.on&&!(G.te&&G.te.askMode)&&!G.ga.card;
  if(G.on&&B&&B.clk&&!B.away){
    if(B.kind==="finale"&&B.qs&&B.k<B.qs.length){var id=B.qs[B.k],q=S.tourQ[id];if(!q)return null;
      return{kind:"quiz",B:B,label:"the quiz",key:"tq:"+id,step:id,q:L(q.q),opts:(q.a||[]).map(function(a){return L(a)}),ok:+q.ok,why:L(q.why),hint:q.hint&&q.hrev&&q.hrev[l]?String(q.hint[l]||"").trim():"",answered:B.answered!=null,lad:gaLadOf(B,id)}}
    if(B.kind==="guess"&&B.gs){var g=B.gs.g;return{kind:"guess",B:B,label:"Guess the year",key:"tg:"+B.gs.step,step:B.gs.step,q:L(g.q),opts:[],n:+g.n,from:+g.from,to:+g.to,answered:!!B.told,lad:gaLadOf(B,"guess")}}
    if(B.kind==="breather"||B.kind==="find"||B.kind==="word")return{kind:B.kind,B:B,label:B.kind==="breather"?"the breather":B.kind==="find"?"Find it on the wall":"Don't say it!",key:"b:"+B.kind,q:"",opts:[],answered:false,lad:{}}}
  if(G.on&&G.gd&&G.gd.ds&&G.gd.ds.on&&!B)return{kind:"word",label:"Don't say it!",key:"ds",q:"",opts:[],answered:false,lad:{}};
  if(B)return null;
  /* the card's own Knowledge Check (and Poll): in view and not answered; in a tour, only at its own step */
  if(S.view!=="card"&&S.view!=="kpage"||S.stop==="closed")return null;
  var st=inTour?S.guideSteps[G.i]:null,poi=svPoi(),sr=scr.getBoundingClientRect(),best=null;
  scr.querySelectorAll(".kq,.poll").forEach(function(el){if(best)return;var blk=el.closest('[data-ref^="b:"]'),bid=blk?blk.dataset.ref.slice(2):S.view==="kpage"?(S.card.filter(function(x){return x.kind===S.pageKind})[0]||{}).id:null;if(!bid)return;
    if(inTour&&(!st||st.ref!=="b:"+bid))return;var r=el.getBoundingClientRect();if(!r.height||Math.min(r.bottom,sr.bottom)-Math.max(r.top,sr.top)<r.height*.5)return;
    var poll=el.classList.contains("poll"),key=poi+"|"+bid;if(poll?G.ga.polled[key]:G.te&&G.te.played[key])return;var idle=!poll&&!inTour&&!G.ga.started[key];if(idle&&!(o&&o.any))return;
    var bs=[].slice.call(el.querySelectorAll(":scope > button:not(.ga-chip), .tfb > button, .kpic > button")),qt=el.querySelector("b,.tfcard");
    best={kind:poll?"poll":"kc",el:el,bid:bid,poi:poi,label:poll?"the card's poll":"the card's quiz",key:(poll?"poll:":"kc:")+key,q:qt?qt.textContent.trim():"",opts:bs.map(function(b){return b.textContent.trim()}),ok:poll?-1:bs.findIndex(function(b){return!!b.dataset.ok}),why:"",hint:"",answered:false,lad:G.ga.lad[(poll?"poll:":"kc:")+key]||(G.ga.lad[(poll?"poll:":"kc:")+key]={})}});
  if(best&&best.kind==="poll")best.q="";if(best)best.idle=best.kind==="kc"&&!inTour&&!G.ga.started[best.poi+"|"+best.bid];return best}
function gaLadOf(B,id){var o=B.gaLad||(B.gaLad={});return o[id]||(o[id]={})}
/* a Poll voted: no longer a game (the card's own Poll has no answer of its own) */
/* the card's quiz started (the Architect, round 18): Play the quiz (What next?), a tap on one of its answers, its microphone */
function gaStart(poi,bid){G.ga.started[(poi||svPoi())+"|"+bid]=true}
ph.addEventListener("pointerdown",function(e){var t=e.target.closest&&e.target.closest('[data-te="quiz"],.kq button,.kq [data-talk]');if(!t)return;var poi=svPoi();
  if(t.dataset.te==="quiz"){(teBlocks(poi)||[]).forEach(function(b){if(b.kind==="Knowledge Check")gaStart(poi,b.id)});return}var blk=t.closest('[data-ref^="b:"]');if(blk)gaStart(poi,blk.dataset.ref.slice(2))},true);
ph.addEventListener("click",function(e){var b=e.target.closest&&e.target.closest(".poll button");if(!b)return;var blk=b.closest('[data-ref^="b:"]');if(blk)G.ga.polled[svPoi()+"|"+blk.dataset.ref.slice(2)]=true},true);

/* ----- the detection, with no AI: the visitor's content words shared with the question or its choices. The stop words (EN and PT,
   accents taken off) and the rule are here, in one place: Loose, any word shared; Normal, two shared, or a choice's word; Strict, at least GA_RATIO of them and two shared or a choice's word. Words that ask for
   the answer itself (answer, hint, clue; resposta, pista, dica) count at every level ----- */
var GA_RATIO=.5;
var GA_STOP=("the a an of to in on at by for with and or is was were are be been being it its this that these those what which who whom whose when where why how did does do done "+
  "can could would should will shall may might me my you your we our they their he his she her him them there here about tell know think please just so very not no yes from as into "+
  "than then also one much many some any all more most like say said azu hey hi ok okay give show mean really something anything thing "+
  "o os as um uma uns umas de do da dos das em no na nos nas num numa por pelo pela pelos pelas para com sem e ou foi era eram sao ser esta estava isto isso este esse essa aquele "+
  "aquela que qual quais quem quando onde porque como me meu minha eu voce se lhe ele ela eles elas ha aqui ali sobre diga diz sabe pode favor muito mais menos nao sim tem tinha "+
  "ter mas algum alguma todos todas tudo entao ja ainda ola diga-me mostre fale coisa seus suas").split(" ");
var GA_ASKS=["answer","hint","clue","resposta","pista","dica"];
function gaNorm(t){return String(t||"").normalize("NFD").replace(/[̀-ͯ]/g,"").toLowerCase()}
function gaWords(t){return(gaNorm(t).match(/[a-z0-9]+/g)||[]).filter(function(w){return(/^\d+$/.test(w)?w.length>=2:w.length>=3)&&GA_STOP.indexOf(w)<0})}
function gaStem(w){return/^\d+$/.test(w)?w:w.slice(0,5)}
function gaToks(t){var o=[];gaWords(t).forEach(function(w){var s=gaStem(w);if(o.indexOf(s)<0)o.push(s)});return o}
function gaAbout(q,g,level){var r={about:false,shared:[],opt:0,n:0,asks:false};if(!g||!g.q)return r;var qs=gaToks(q),gq=gaToks(g.q),go=[];(g.opts||[]).forEach(function(o){gaToks(o).forEach(function(s){if(go.indexOf(s)<0)go.push(s)})});
  r.n=qs.length;r.asks=gaWords(q).some(function(w){return GA_ASKS.indexOf(w)>=0});qs.forEach(function(s){if(gq.indexOf(s)>=0||go.indexOf(s)>=0)r.shared.push(s);if(go.indexOf(s)>=0)r.opt++});
  var sh=r.shared.length,ra=r.n?sh/r.n:0;level=level||S.guideCfg.gaStrict||"normal";
  r.about=r.asks||(level==="loose"?sh>=1:level==="strict"?ra>=GA_RATIO&&(sh>=2||r.opt>=1):sh>=2||r.opt>=1);return r}

/* ----- hint 1, where to look, with no AI: the card's block (or the story's row) whose words hold the right answer, the question's own
   words as the tie-break (and the quiz question's own step); named by its heading, never by words that give the answer away ----- */
function gaRowText(r,l){var x=function(o){return wordIn(o,l)},t=[];if(r.pattern)(r.slots||[]).forEach(function(s){t.push(x(s.text),x(s.title),x(s.cap))});else if(r.c)t.push(x(r.c.text),x(r.c.cap));return t.filter(Boolean).join(" ").trim()}
function gaHeadOf(r,l){if(r.kind==="heading"&&r.c&&r.c.text)return wordIn(r.c.text,l);if(!r.pattern)return"";
  var s=(r.slots||[]).map(function(x){return wordIn(x.title||x.text,l)}).filter(function(t){return t&&t.split(/\s+/).length<=8&&!/[.·:]$|·/.test(t.trim())})[0];return s||""}
function gaUnits(l){var out=[];
  (S.card||[]).forEach(function(b){if(/^(Knowledge Check|Poll)$/.test(b.kind))return;var w=blockSays(b,l)||{},p=[w.title,w.text,w.back,w.cap,w.q];
    (w.facts||[]).concat(w.events||[],w.rows||[]).forEach(function(f){p.push((f||[]).join(" "))});(w.lines||[]).concat(w.caps||[],w.points||[]).forEach(function(x){p.push(x)});
    var t=p.filter(Boolean).join(" ").trim();if(t)out.push({ref:"b:"+b.id,view:"card",text:t,title:w.title||(GA_KIND[b.kind]?wordIn(GA_KIND[b.kind],l):b.kind)})});
  var head=wordIn(DEMO.title,l);(S.story||[]).forEach(function(r){var h=gaHeadOf(r,l);if(h)head=h;var t=gaRowText(r,l);if(t)out.push({ref:"r:"+r.id,view:"story",text:t,title:head})});return out}
function gaWhere(g,l){l=l||S.lang||"en";if(!g||g.ok==null||g.ok<0||!g.opts[g.ok])return null;var ans=gaToks(g.opts[g.ok]),qt=gaToks(g.q),best=null,bs=0,st=g.step&&byId(S.fullSteps||S.guideSteps,g.step);
  gaUnits(l).forEach(function(u){var ut=gaToks(u.text),a=ans.filter(function(s){return ut.indexOf(s)>=0}).length;if(!a)return;var sc=3*a+qt.filter(function(s){return ut.indexOf(s)>=0}).length+(st&&st.ref===u.ref?2:0);if(sc>bs){bs=sc;best=u}});
  if(!best)return null;var title=gaSpoils(best.title,gaKeys(g))||gaToks(best.title).some(function(s){return ans.indexOf(s)>=0})?(best.view==="story"?wordIn(GA_KIND.Story,l):wordIn(GA_KIND[(byId(S.card,best.ref.slice(2))||{}).kind]||"",l)):best.title;
  return{ref:best.ref,view:best.view,title:String(title||"").replace(/[.!?…]+$/,"")}}

/* ----- the ladder: the steps the Editor keeps, one per ask; past the last, the last again ----- */
function gaSteps(g){var c=S.guideCfg,o=[];if(c.gaH1&&(g.hint||gaWhere(g)))o.push(1);if(c.gaH2&&gaCanH2(g))o.push(2);if(c.gaH3&&(g.kind==="quiz"||g.kind==="kc"||g.kind==="guess"))o.push(3);return o}
function gaCanH2(g){if(g.kind==="guess")return isFinite(g.n)&&g.to>g.from;return(g.kind==="quiz"||g.kind==="kc")&&g.opts.length>=3&&gaWrongOf(g)>=0}
/* the wrong choice that fades: the last wrong one still in play */
function gaWrongOf(g){for(var i=g.opts.length-1;i>=0;i--)if(i!==g.ok&&g.lad.out!==i)return i;return-1}
/* Guess the year's band: the half that holds the answer, its edge a round year (to 100, else 50, else the middle) */
function gaBand(g){var b=g.lad.band||{from:g.from,to:g.to},mid=(b.from+b.to)/2,r=[100,50,10,1].map(function(u){return Math.round(mid/u)*u}).filter(function(m){return m>b.from&&m<b.to&&m!==g.n})[0];if(r==null)return null;
  return g.n<r?{from:b.from,to:r,word:"before",at:r}:{from:r,to:b.to,word:"after",at:r}}
function gaStepWords(st,g,l){l=l||S.lang||"en";var w=function(k){return wordIn(S.tx[k],l)};
  if(st===1){if(g.hint)return fmt(w("ga_h1_own"),g.hint);var wh=gaWhere(g,l);return wh?fmt(w("ga_h1_look"),wh.title):""}
  if(st===2){if(g.kind==="guess"){var bd=gaBand(g);return bd?fmt(w(bd.word==="before"?"ga_h2_before":"ga_h2_after"),bd.at):""}var i=gaWrongOf(g);return i>=0?fmt(w("ga_h2_opt"),gaMid(g.opts[i])):""}
  if(st===3)return w("ga_h3");return w("ga_none")}
/* a choice mid-sentence: a leading little word lower-cased ("It's not about 500 years"), a name kept ("It's not José I") */
var GA_LOW=["about","around","some","a","an","the","only","cerca","uma","um","o","os","as","apenas","so"];
function gaMid(t){t=String(t||"");var w=gaNorm((t.match(/^\S+/)||[""])[0]);return GA_LOW.indexOf(w)>=0?t.charAt(0).toLowerCase()+t.slice(1):t}
/* the next step's words without taking it (the model's rule and the Editor's read-backs) */
function gaPeek(g){var s=gaSteps(g),n=Math.min(g.lad.n||0,Math.max(0,s.length-1));return s.length?gaStepWords(s[n],g):TXT("ga_none")}

/* ----- the pause ----- */
function gaHost(g){if(!g)return null;if(g.el&&g.el.isConnected)return g.el;if(g.kind==="kc"||g.kind==="poll"){var b=scr.querySelector('[data-ref="b:'+g.bid+'"]');return b?b.querySelector(g.kind==="poll"?".poll":".kq"):null}
  if(g.kind==="word")return ph.querySelector(".gdstrip")||ph.querySelector(".gdbeat");return ph.querySelector(".gdbeat")}
function gaPause(g,how){var A=G.ga;if(A.on||!g)return;A.on=true;A.g=g;A.how=how;A.asked=false;A.answered=false;A.where=null;A.choice=false;A.away=null;A.left=false;A.step=0;A.wasPaused=!!G.paused;A.voice=null;A.poi=svPoi();A.at=performance.now();
  A.clock0=gaClock();A.th0=(G.thread||[]).length;A.keep=true;G.keepScroll=true;clearTimeout(A.backT);A.backT=0;A.backAt=0;
  /* the tour's own line (Don't say it!'s narration): kept with its word, said on from it when the game goes on */
  if(G.on&&!G.paused){var st=S.guideSteps[G.i];if(G.sayArgs&&G.sayLine&&st&&G.sayLine.text===stepText(st)&&(G.utter||G.timer))A.voice={a:G.sayArgs,words:(G.words||[]).slice(),k:G.hl&&G.hl.k>=0?G.hl.k:0,line:G.sayLine};pauseNow()}
  if(g.B)gdBeatFreeze(g.B,true);
  if(typeof gdLog==="function")gdLog("event","Ask during a game: "+g.label+" paused ("+how+")");gaDraw()}
/* the game's clock: the beat's (its timers run on it), Don't say it!'s, Find it on the wall's; the card's own quiz has none */
function gaClock(){var B=G.gd&&G.gd.bt;if(G.gd&&G.gd.ds&&G.gd.ds.on&&!B)return gdWordClock();if(B&&B.clk)return gdBeatClock(B);return null}
/* the game goes on: the line from Azu's intro lines › Back to the game on the chip, said, then the clock where it stopped */
function gaBackLine(poi,lang,k){var ls=atIntroLines("gameback",lang||S.lang).filter(function(x){return x.max<=(+S.guideCfg.atIntroCap||20)});if(!ls.length)return null;
  var x=ls[(atSeed(poi||svPoi())+atIntroSalt("gameback")+(k||0))%ls.length],t=/\{point\}/.test(x.text)?nameIn(x.text,"{point}",teName(poi||svPoi(),lang||S.lang),lang||S.lang):x.text;return{key:x.key,text:t}}
function gaBack(how){var A=G.ga;if(!A.on||A.leaving)return;var g=A.g;A.leaving=true;gaCancelBack();if(A.away)gaShowBack();
  gaCloseAsk();var l=gaBackLine(A.poi,S.lang,A.k++);A.line=l;var host=gaHost(g),c=host&&host.querySelector(":scope > .ga-chip");
  if(typeof gdLog==="function")gdLog("event","Ask during a game: back to "+g.label+" ("+(how||"Back to the game")+")");
  if(!l){gaGo();return}if(c){c.classList.add("ga-line");c.innerHTML='<span>'+esc(l.text)+'</span>';c.setAttribute("aria-label",l.text)}
  var done=false,fin=function(){if(done)return;done=true;clearTimeout(A.lineT);gaGo()},n=l.text.split(/\s+/).length;A.lineT=setTimeout(fin,n*60000/(170*(typeof guideSpeed==="function"?guideSpeed():1))+1600);
  try{say(l.text,function(){},fin)}catch(e){fin()}}
/* the clock goes on where it stopped, the voice from its word, the chip goes */
function gaGo(){var A=G.ga,g=A.g;A.on=false;A.leaving=false;A.g=null;gaKeepOff();clearTimeout(A.lineT);if(A.line&&G.sayLine&&G.sayLine.text===A.line.text){G.utter=null;stopSpeech()}
  if(g&&g.B&&G.gd.bt===g.B)gdBeatFreeze(g.B,false);
  if(G.on&&!A.wasPaused){G.paused=false;G.resumeVoice=false;var v=A.voice;A.voice=null;if(v&&S.guideSteps[G.i]&&v.line.text===stepText(S.guideSteps[G.i])){G.sayArgs=v.a;G.words=v.words;G.sayLine=v.line;speakFrom(v.k,false)}}
  gaDraw();if(G.on)renderGuide()}
/* the card held where it is during the pause (the tour never places its step meanwhile); after it, as the Ask that is still open wants */
function gaKeepOff(){var A=G.ga;if(!A.keep)return;A.keep=false;G.keepScroll=!!A.card||!!(G.te&&G.te.askMode)}
/* Ask closes: the panel lowered (or the card's own Ask left: the card where it was) */
function gaCloseAsk(){var A=G.ga;A.lowering=true;try{if(G.te&&G.te.askMode)teAskLeave();else if(A.card)gaCardLeave();else{G.askOpen=false;G.typing=false;if(G.rec)try{G.rec.abort()}catch(e){}G.talk="idle";if(G.on&&(S.dockH==="peek"||S.dockH==="full"))S.dockH="mini";if(G.on)renderGuide()}}finally{A.lowering=false}}
/* the pause ends with no line (a Stop, Next, the game gone) */
function gaEnd(why){var A=G.ga;if(!A.on)return;var g=A.g;gaCancelBack();if(A.away)gaShowBack(true);A.on=false;A.leaving=false;A.g=null;gaKeepOff();clearTimeout(A.lineT);
  if(g&&g.B&&G.gd.bt===g.B)gdBeatFreeze(g.B,false);if(typeof gdLog==="function")gdLog("event","Ask during a game: the pause ends ("+why+")");gaDraw()}

/* ----- the card's own Ask (the ✦ menu's, Tap on Azu's) while its quiz is on screen: over the card where it is, as round 17c's What next? ----- */
function gaCardEnter(){ph.scrollLeft=0;var A=G.ga,g=gaGame(),el=g&&g.bid?scr.querySelector('[data-ref="b:'+g.bid+'"]'):null;A.card={top:scr.scrollTop,view:S.view,stop:S.stop,ref:el?el.dataset.ref:null,off:el?el.getBoundingClientRect().top-scr.getBoundingClientRect().top:0};G.fab.menu=false;ph.querySelectorAll(".gbtn-wrap,.gbmenu,.gbband").forEach(function(x){x.remove()});G.keepScroll=true;G.on=true;G.paused=true;G.waiting=false;G.askOpen=true;S.dockH="full";renderGuide();scr.scrollTop=A.card.top;
  var i=ph.querySelector("#gq");if(i)i.focus({preventScroll:true})}
function gaCardLeave(){var A=G.ga,b=A.card;A.card=null;G.keepScroll=false;stopSpeech();G.on=false;G.paused=false;G.askOpen=false;G.typing=false;clearGuideMarks();renderGuide();if(typeof fabLayer==="function")fabLayer();if(!b)return;
  if(S.view!==b.view||S.stop!==b.stop){S.view=b.view;S.stop=b.stop;renderPhone(false)}/* the card back where it was: its quiz at the same place on the screen (a scroll offset alone drifts while the page's layout settles) */
  var put=function(){if(G.on)return;scr.scrollTop=b.top;var e=b.ref&&scr.querySelector('[data-ref="'+b.ref+'"]');if(e)scr.scrollTop+=e.getBoundingClientRect().top-scr.getBoundingClientRect().top-b.off};put();requestAnimationFrame(put);setTimeout(put,250);setTimeout(put,600)}
on("gbAsk.answer",function(){var g=G.on?null:gaGame({any:true});if(!g)return;if(!S.guideCfg.gaOn&&!g.idle){G.fab.menu=false;fabLayer();gaNote(g,TXT("ga_off"));return true}gaCardEnter();return true});

/* ----- the ways in: the panel raised, the microphone, the keyboard (renderGuide.after); a question (ask.answer, ask.before) ----- */
on("renderGuide.after",function(){var A=G.ga;
  /* Ask opening: the panel raised to Open or Full, the microphone, the keyboard (a panel already open as a step's game begins is not an Ask: a question then pauses it) */
  var open=G.on&&(S.dockH==="peek"||S.dockH==="full"||G.talk==="listening"||!!G.typing),was=A.open;A.open=open;if(A.idle&&!G.askOpen){A.idle=null}if(!A.on&&A.idle){gaDraw();return}
  if(!A.on){if(!S.guideCfg.gaOn||!open||was&&G.talk!=="listening")return;var g=gaGame();if(g)gaPause(g,G.talk==="listening"?"the microphone":G.typing?"the keyboard":"the panel opened");return}
  if(A.g&&A.g.B&&G.gd.bt!==A.g.B){gaEnd("the game ended");return}
  if(G.on&&!A.lowering&&!A.leaving&&!A.away&&!A.left&&(S.dockH==="mini"||S.dockH==="compact")){setTimeout(function(){if(G.ga.on&&!G.ga.leaving&&!G.ga.away&&(S.dockH==="mini"||S.dockH==="compact"))gaBack("the panel lowered")},0);return}
  gaDraw()});
on("renderPhone.after",function(){var A=G.ga;if(!A.on)return;
  /* leaving the point: Ask closes, the game stays paused (Play, or a tap on its chip, goes back) */
  if(!A.left&&!A.away&&(S.stop==="closed"||svPoi()!==A.poi)){A.left=true;gaCancelBack();A.lowering=true;try{G.askOpen=false;G.typing=false;if(A.card){var b=A.card;A.card=null;G.keepScroll=false;G.on=false;G.paused=false;renderGuide()}else if(G.on&&(S.dockH==="peek"||S.dockH==="full")){S.dockH="mini";renderGuide()}}finally{A.lowering=false}
    if(typeof gdLog==="function")gdLog("event","Ask during a game: the point left, Ask closed, "+A.g.label+" stays paused")}
  gaDraw()});
on("ask.answer",function(q){if(!q||!String(q).trim())return;var A=G.ga,g=A.on?A.g:gaGame();A.idle=null;
  if(!g){var gi=gaGame({any:true});if(gi&&gi.idle&&gi.q&&gaAbout(q,gi).about){gaHint(q,gi);return true}return}
  if(!S.guideCfg.gaOn){gaNote(g,TXT("ga_off"));G.askOpen=false;G.typing=false;if(G.on&&(S.dockH==="peek"||S.dockH==="full")){S.dockH="mini";renderGuide()}var gq=ph.querySelector("#gq");if(gq)gq.value="";return true}
  if(!A.on){if(!G.on&&!(g.kind==="kc"||g.kind==="poll"))return;if(!G.on){gaCardEnter()}gaPause(g,"a question")}
  A.asked=true;gaCancelBack();A.choice=false;var ab=!g.answered&&g.q?gaAbout(q,g):{about:false};A.lastAbout=ab;if(!ab.about)return;gaHint(q,g);return true});
on("ask.before",function(){var A=G.ga;if(!A.on){if(S.guideCfg.gaOn){var g=gaGame();if(g&&G.on)gaPause(g,"a question")}if(!G.ga.on)return}G.ga.asked=true;gaCancelBack()});
/* the answer heard (its words' time at the most), then Back by itself's seconds */
on("ask.after",function(t){var A=G.ga;if(!A.on)return;A.answered=true;var t0=performance.now(),est=Math.min(20000,String(t||"").split(/\s+/).length*60000/170+1500),seq=A.seq=(A.seq||0)+1;
  (function w(){if(!G.ga.on||G.ga.seq!==seq)return;if((G.utter||G.recAudio||G.timer||G.busy)&&performance.now()-t0<est){setTimeout(w,200);return}gaArmBack()})()});
function gaArmBack(){var A=G.ga;if(!A.on||!S.guideCfg.gaBack||A.choice||A.away||A.left)return;var s=Math.max(2,Math.min(8,+S.guideCfg.gaBackS||4));clearTimeout(A.backT);A.backAt=performance.now();A.backS=s;
  A.backT=setTimeout(function(){A.backT=0;gaBack("Back by itself, "+s+" s after the answer")},s*1000);gaDraw()}
function gaCancelBack(){var A=G.ga;if(!A.backT&&!A.backAt)return;clearTimeout(A.backT);A.backT=0;A.backAt=0;ph.querySelectorAll(".ga-bar").forEach(function(x){x.remove()})}
/* any tap or a key typed cancels Back by itself (the Back button itself goes back) */
ph.addEventListener("pointerdown",function(e){if(G.ga.backT&&!(e.target.closest&&e.target.closest('[data-ga="back"]')))gaCancelBack()},true);
ph.addEventListener("keydown",function(){if(G.ga.backT)gaCancelBack()},true);
/* the microphone during the quiz's question: what matches a choice is its answer (the game goes on at once); anything else is a question */
on("heard.answer",function(q){var A=G.ga;if(!A.on||!q||!String(q).trim()||!S.guideCfg.gaOn)return;var B=G.gd.bt,g=A.g;
  if(g.kind==="quiz"&&!A.asked&&B&&B.kind==="finale"&&B.answered==null){var qq=gdQuizQ(B,B.k),i=gdQuizMatch(q,qq.a.map(function(a){return L(a)}),S.lang||"en");if(i>=0){gaCloseAsk();gaGo();return}}
  G.talk="idle";ask(q);return true});

/* ----- the hint ladder: one step an ask, in the panel as Azu's answer ----- */
function gaHint(q,g){var A=G.ga;if(!A.on)A.idle=g;var s=gaSteps(g),n=Math.min(g.lad.n||0,Math.max(0,s.length-1)),st=s.length?s[n]:0,t=st?gaStepWords(st,g):TXT("ga_none");if(s.length)g.lad.n=n+1;A.step=st;A.where=null;
  if(st===1&&!g.hint)A.where=gaWhere(g);if(st===2)gaH2(g);if(st===3)A.choice=true;
  if(typeof gdLog==="function")gdLog("event","Ask during a game: “"+q+"” is about "+g.label+"'s question: "+(st?"step "+st:"no step On"));
  emitLatestFirst("ask.before",q);if(G.on&&!G.paused)pauseNow();G.askOpen=true;G.busy=false;G.answer=t;G.agTurn=null;emit("ask.after",t);renderGuide();say(t,function(){},function(){})}
/* hint 2: a wrong choice fades (the quiz's, the card's), Guess the year's band narrows */
function gaH2(g){if(g.kind==="guess"){var b=gaBand(g);if(b)g.lad.band={from:b.from,to:b.to}}else{var i=gaWrongOf(g);if(i>=0)g.lad.out=i}gaApply(g)}
function gaApply(g){if(!g)return;var host=gaHost(g);if(!host)return;
  if(g.lad.out!=null){var bs=g.kind==="quiz"?host.querySelectorAll(".bt-choice"):host.querySelectorAll(":scope > button:not(.ga-chip), .tfb > button, .kpic > button"),b=bs[g.lad.out];if(b&&!b.classList.contains("ga-out")){b.classList.add("ga-out");b.disabled=true;b.setAttribute("aria-disabled","true")}}
  if(g.lad.band&&g.B){var r=host.querySelector(".bt-range"),bd=g.lad.band;if(r&&(+r.min!==bd.from||+r.max!==bd.to)){r.min=bd.from;r.max=bd.to;var v=Math.max(bd.from,Math.min(bd.to,+g.B.v));if(v!==+g.B.v||+r.value!==v){g.B.v=v;r.value=v}var o=host.querySelector(".bt-val");if(o)o.textContent=r.value;
    var e=host.querySelectorAll(".bt-slider .bt-ends span");if(e.length===2){e[0].textContent=bd.from;e[1].textContent=bd.to}host.classList.add("ga-band")}}}
/* Tell me: the game goes on with the answer said (its why), told: no star, then its next question; I'll try: back */
function gaTell(){var A=G.ga,g=A.on?A.g:A.idle;if(!g)return;A.choice=false;A.idle=null;gaCancelBack();if(A.away)gaShowBack(true);gaCloseAsk();A.on=false;A.g=null;
  if(typeof gdLog==="function")gdLog("event","Ask during a game: Tell me ("+g.label+": told, no star)");
  if(g.B&&G.gd.bt===g.B)gdBeatFreeze(g.B,false);if(G.on&&!A.wasPaused){G.paused=false;G.resumeVoice=false}gaDraw();if(G.on)renderGuide();
  if(g.kind==="quiz"){gdQuizAns(g.ok,"told");return}
  if(g.kind==="guess"){var B=g.B;B.told=true;gdBTClr(B,"waitT");gdBTClr(B,"notNowT");G.gd.guess=null;var t=fmt(TXT("ga_told"),g.n,"").trim();gdBeatBody(gdGuessTrack(B.gs.g,g.n,g.n),"");gdBeatSay(t,function(){gdBeatFinish("Guess the year: told (asked during the game)","acted")});return}
  if(g.kind==="kc"){var host=gaHost(g),bs=host?host.querySelectorAll(":scope > button:not(.ga-chip), .tfb > button, .kpic > button"):[],ok=bs[g.ok];if(ok)ok.classList.add("ok","ga-told");bs.forEach(function(b){b.disabled=true});
    if(G.te)G.te.played[g.poi+"|"+g.bid]={right:false,told:true};var tk=fmt(TXT("ga_told"),g.opts[g.ok],"").trim();gaNote(g,tk);try{say(tk,function(){},function(){})}catch(e){}}}

/* ----- Show me (hint 1, or an answer's block): the card goes to the block while the game stays paused; Back to the game comes back ----- */
on("goTo.answer",function(ref){var A=G.ga;if(!A.on)return;var c=corpus().filter(function(x){return x.ref===ref})[0];if(!c)return;if(!A.where)A.where={ref:ref,view:c.view,title:c.title};return"Offered as Show me: "+c.title+" (a game is paused)"});
function gaShow(){var A=G.ga,w=A.where;if(!A.on||!w)return;gaCancelBack();A.away={view:S.view,top:scr.scrollTop,stop:S.stop};var bt=ph.querySelector(".gdbeat");if(bt)bt.classList.add("bt-away");
  A.lowering=true;try{G.askOpen=false;G.typing=false;if(G.on){S.dockH="mini";renderGuide()}}finally{A.lowering=false}
  if(S.view!==w.view){S.view=w.view;renderPhone(false)}var tries=0;(function ring(){var e=scr.querySelector('[data-ref="'+w.ref+'"]');if(!e&&tries++<8){setTimeout(ring,60);return}if(!e)return;
    var y=e.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-90;scr.scrollTo({top:Math.max(0,y),behavior:S.reduce?"auto":"smooth"});e.classList.add("gdshowme");setTimeout(function(){e.classList.remove("gdshowme")},4200)})();
  if(typeof gdLog==="function")gdLog("event","Ask during a game: Show me “"+w.title+"”, the game still paused");gaDraw()}
function gaShowBack(quiet){var A=G.ga,a=A.away;if(!a)return;A.away=null;ph.querySelectorAll(".ga-showback").forEach(function(x){x.remove()});var bt=ph.querySelector(".gdbeat");if(bt)bt.classList.remove("bt-away");
  if(S.view!==a.view||S.stop!==a.stop){S.view=a.view;S.stop=a.stop;renderPhone(false)}scr.scrollTop=a.top;requestAnimationFrame(function(){scr.scrollTop=a.top});if(typeof gdBeatPlace==="function")gdBeatPlace()}

/* ----- Off: the line on the game, never spoken over it (the game goes on); a voice the microphone paused goes on ----- */
on("pauseTour.answer",function(){if(!S.guideCfg.gaOn&&!G.explicitPause&&!G.ga.on&&G.on&&gaGame())return true});
function gaNote(g,t){var host=gaHost(g);ph.querySelectorAll(".ga-note").forEach(function(x){x.remove()});if(!host)return;host.insertAdjacentHTML("beforeend",'<div class="ga-note" role="status">'+esc(t)+'</div>');var n=host.querySelector(":scope > .ga-note");
  clearTimeout(G.ga.noteT);G.ga.noteT=setTimeout(function(){if(n&&n.isConnected)n.remove()},3200);if(typeof gdLog==="function")gdLog("event","Ask during a game: “"+t+"”")}

/* ----- the tour's controls during the pause: Play goes back; Stop ends the game and Ask as Stop does; Next and Previous end the pause ----- */
on("resumeTour.answer",function(){if(!G.ga.on||G.ga.leaving)return;gaBack("Play");return true});
on("stopTap.answer",function(){var A=G.ga;if(!A.on)return;var g=A.g;if(g&&g.B&&G.gd.bt===g.B){gdBeatClose();if(typeof gdStop==="function")gdStop("Stop during Ask during a game","dismissed")}gaEnd("Stop");if(G.gd)G.gd.hold=false;G.askOpen=false;G.typing=false;if(A.card)gaCardLeave()});
on("stopTour.before",function(){if(G.ga.on)gaEnd("the tour stopped")});
["next.answer","prevTap.answer"].forEach(function(n){on(n,function(){var A=G.ga;if(!A.on||A.card||G.te&&G.te.askMode)return;gaEnd(n==="next.answer"?"Next":"Previous");G.paused=false;G.askOpen=false})});

/* ----- the phone: the chip on the game, the panel's Back row, the faded choice, the band ----- */
function gaDraw(){var A=G.ga,d=ph.querySelector(".gdock");
  ph.querySelectorAll(".ga-paused").forEach(function(x){if(!A.on||x!==gaHost(A.g))x.classList.remove("ga-paused")});ph.querySelectorAll(".ga-chip").forEach(function(x){if(!A.on||x.parentNode!==gaHost(A.g))x.remove()});
  if(d){d.classList.toggle("ga-on",A.on&&!A.left);var row=d.querySelector(".ga-row");if(row)row.remove()}
  ph.querySelectorAll(".ga-showback").forEach(function(x){if(!A.away)x.remove()});ph.classList.toggle("ga-pausing",A.on);
  if(!A.on){if(A.idle&&d&&G.askOpen&&(S.dockH==="full"||S.dockH==="peek")&&(A.where||A.choice)){var a0=d.querySelector(".gd-where,.gd-ctl,.gd-askrow,.gd-askbar,.gd-botrow");if(a0)a0.insertAdjacentHTML("beforebegin",gaRowHtml())}return}var g=A.g,host=gaHost(g);gaApply(g);
  if(host){host.classList.add("ga-paused");if(!host.querySelector(":scope > .ga-chip"))host.insertAdjacentHTML("beforeend",'<button type="button" class="ga-chip" data-ga="back" aria-label="'+TXH("ga_paused")+' · '+TXH("ga_back")+'">'+GA_PAUSE+'<span>'+TXH("ga_paused")+'</span></button>')}
  if(A.away){if(!ph.querySelector(".ga-showback")){ph.insertAdjacentHTML("beforeend",'<button type="button" class="ga-showback" data-ga="back">'+TXH("ga_back")+'</button>')}var sb=ph.querySelector(".ga-showback"),dk=ph.querySelector(".gdock");if(sb)sb.style.bottom=(dk&&dk.getClientRects().length?Math.round(ph.offsetHeight-phRect(dk).t+12):24)+"px";return}
  if(d&&!A.left&&(S.dockH==="full"||S.dockH==="peek")){var at=d.querySelector(".gd-where,.gd-ctl,.gd-askrow,.gd-askbar,.gd-botrow");if(at)at.insertAdjacentHTML("beforebegin",gaRowHtml());else d.insertAdjacentHTML("beforeend",gaRowHtml());gaDockPlace(d)}}
function gaRowHtml(){var A=G.ga,b=function(k,w,pri,bar){return'<button type="button" class="ga-btn'+(pri?" pri":"")+'" data-ga="'+k+'">'+esc(w)+bar+'</button>'},bar="";
  if(A.backT&&A.backAt){var el=(performance.now()-A.backAt)/1000;bar='<i class="ga-bar" style="animation-duration:'+A.backS+'s;animation-delay:-'+el.toFixed(2)+'s"></i>'}
  if(A.choice)return'<div class="ga-row" role="group">'+b("tell",TXT("ga_tell"),false,"")+b("try",TXT("ga_try"),true,"")+'</div>';
  if(!A.on)return'<div class="ga-row" role="group">'+b("show",TXT("ga_showme"),true,"")+'</div>';
  return'<div class="ga-row" role="group">'+(A.where?b("show",TXT("ga_showme"),false,""):"")+b("back",TXT("ga_back"),true,bar)+'</div>'}
/* the panel during the pause: shorter than Full, so the game stays in sight above it (its card's top at most 46 % down the phone) */
/* round 21 (G.2: the chip touched its eyebrow "TEST YOURSELF · QUESTION 1 OF 3"): on the quiz's upper edge the chip keeps 8 px or more from
   the words of the block's eyebrow above it: with its words when they fit beside the eyebrow, else its pause sign alone (44 px, its name
   for a screen reader), else inside the quiz's top corner. Unity: the chip's position from the eyebrow label's text bounds */
function gaEyebrow(host){var e=host.previousElementSibling;while(e&&!/\S/.test(e.textContent||""))e=e.previousElementSibling;return e&&e.getClientRects().length?e:null}
function gaTextBox(el){var rg=document.createRange();rg.selectNodeContents(el);var r=rg.getBoundingClientRect(),pr=ph.getBoundingClientRect(),q=pr.height/(ph.offsetHeight||1);return{l:(r.left-pr.left)/q,t:(r.top-pr.top)/q,r:(r.right-pr.left)/q,b:(r.bottom-pr.top)/q}}
function gaGap(a,b){return Math.max(b.l-(a.l+a.w),a.l-b.r,b.t-(a.t+a.h),a.t-b.b)}
function gaChipGap(cp,host){var ey=gaEyebrow(host);return ey?gaGap(phRect(cp),gaTextBox(ey)):1e9}
function gaChipClear(cp,host){cp.style.left="";cp.style.right="";if(gaChipGap(cp,host)>=8)return;cp.classList.add("ga-icon");
  /* the pause sign just after the eyebrow's words (clear of the mascot standing at the screen's edge) */
  var ey=gaEyebrow(host),hr=phRect(host);if(ey){var tb=gaTextBox(ey),x=Math.round(tb.r-hr.l+12);if(x+44<=hr.w-8){cp.style.left=x+"px";cp.style.right="auto"}}
  if(gaChipGap(cp,host)>=8)return;cp.style.left="";cp.style.right="";cp.classList.remove("ga-top");cp.classList.add("ga-inside")}
function gaDockPlace(d){var H=ph.offsetHeight,g=G.ga.g,top=Math.round(H*.46);if(g&&(g.kind==="kc"||g.kind==="poll")){var h=gaHost(g);if(h){var r=phRect(h);top=Math.round(Math.max(H*.4,Math.min(H*.56,r.t+r.h+14)))}}d.style.setProperty("--ga-top",top+"px");if(typeof gdBeatPlace==="function")gdBeatPlace();
  /* the card's quiz: its chip on its lower edge, or on its upper edge when the panel would cover it */
  var ch=g&&(g.kind==="kc"||g.kind==="poll")&&gaHost(g),cp=ch&&ch.querySelector(":scope > .ga-chip");if(cp){cp.classList.remove("ga-top","ga-icon","ga-inside");cp.style.left="";cp.style.right="";var cr=phRect(cp);if(cr.t+cr.h>top-4){cp.classList.add("ga-top");gaChipClear(cp,ch)}}
  /* the conversation shows this pause's questions and answers only, the latest in sight */
  var th=d.querySelector(".gd-thread");if(th){var n=Math.max(0,G.thread.length-(G.ga.th0||0)),ms=[].slice.call(th.children).filter(function(x){return/\bgm-(me|azu)\b/.test(x.className)&&!x.classList.contains("typing")}),keep=n?ms.slice(-n):[];
    [].slice.call(th.children).forEach(function(x){if(!x.classList.contains("typing")&&keep.indexOf(x)<0)x.classList.add("ga-old")});th.scrollTop=th.scrollHeight}}
ph.addEventListener("click",function(e){var t=e.target.closest&&e.target.closest("[data-ga]");if(!t)return;e.stopPropagation();e.preventDefault();var k=t.dataset.ga;
  if(!G.ga.on&&G.ga.idle){var g=G.ga.idle;if(k==="tell"){gaTell();return}var w=G.ga.where;G.ga.idle=null;G.ga.choice=false;gaCloseAsk();if(k==="show"&&w)gaShowIdle(w);gaDraw();return}
  if(k==="back"||k==="try"){gaBack(k==="try"?"I'll try":"Back to the game");return}if(k==="tell"){gaTell();return}if(k==="show"){gaShow();return}},true);
/* Show me with nothing paused (the card's quiz not started): Ask closes and the card goes to the block, ringed */
function gaShowIdle(w){if(S.view!==w.view){S.view=w.view;renderPhone(false)}var e=scr.querySelector('[data-ref="'+w.ref+'"]');if(!e)return;var y=e.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-90;
  scr.scrollTo({top:Math.max(0,y),behavior:S.reduce?"auto":"smooth"});e.classList.add("gdshowme");setTimeout(function(){e.classList.remove("gdshowme")},4200)}

/* ----- the model's context: the game on screen, its question and its rule ----- */
function gaRule(){var g=gaGuarded();if(!g||!g.q)return"";var ans=g.kind==="guess"?String(g.n):g.opts[g.ok]||"";
  return"THE VISITOR IS PLAYING A GAME"+(G.ga.on?", paused while they ask":", on screen")+": the question is “"+g.q+"”"+(g.opts.length?" (choices: "+g.opts.join(" / ")+")":"")+". Do not say the answer, and do not confirm or rule out a choice. "+(ans?"THE ANSWER, never to be said or hinted in any form (its words, its number in figures or in words, in any language): “"+ans+"”. ":"")+"If the visitor asks about this question, give the hint below instead, in your own warm words, and nothing more. HINT: "+gaPeek(g)}
on("askPrompt.text",function(p){var r=gaRule();return r?p.replace("\n\nVISITOR'S QUESTION:","\n\n"+r+"\n\nVISITOR'S QUESTION:"):p});
on("agPrompt.text",function(p,where){var r=where==="wall"?"":gaRule();return r?p.replace("\n\nVISITOR: ","\n- "+r+"\n\nVISITOR: "):p});
on("agContext.ctx",function(c,where){var g=gaGuarded();if(where!=="wall"&&g&&g.q)c.game={playing:g.label,paused:!!G.ga.on,question:g.q,choices:g.opts,rule:"do not say the answer, in any form; give the hint"};return c});

/* ----- no spoiler (the Architect, round 18): while a game's question is open and not answered (the game paused, or the card's quiz on
   screen), no answer, offline or the model's, carries its right answer: the right choice's key words (those no wrong choice has) and its
   numbers, in figures and in words, EN and PT ("250", "two hundred and fifty", "duzentos e cinquenta"). An answer that does is said
   without those sentences (the next one that does not, of the answer, then of the block it came from), else with hint 1's where to
   look. Unity: GuideGameAsk.Unspoil(answer, question) on every answer before it is shown or spoken ----- */
var GA_E1="zero one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen".split(" "),GA_E10="  twenty thirty forty fifty sixty seventy eighty ninety".split(" ");
var GA_P1="zero um dois tres quatro cinco seis sete oito nove dez onze doze treze catorze quinze dezasseis dezassete dezoito dezanove".split(" "),GA_P10="  vinte trinta quarenta cinquenta sessenta setenta oitenta noventa".split(" "),
  GA_P100=" cento duzentos trezentos quatrocentos quinhentos seiscentos setecentos oitocentos novecentos".split(" ");
function gaEn(n,and){var a=and===false?" ":" and ";if(n<20)return GA_E1[n];if(n<100)return GA_E10[Math.floor(n/10)]+(n%10?" "+GA_E1[n%10]:"");if(n<1000)return GA_E1[Math.floor(n/100)]+" hundred"+(n%100?a+gaEn(n%100,and):"");
  if(n<1e6)return gaEn(Math.floor(n/1000),and)+" thousand"+(n%1000?(n%1000<100?a:" ")+gaEn(n%1000,and):"");return String(n)}
function gaPt(n){if(n<20)return GA_P1[n];if(n<100)return GA_P10[Math.floor(n/10)]+(n%10?" e "+GA_P1[n%10]:"");if(n===100)return"cem";if(n<1000)return GA_P100[Math.floor(n/100)]+(n%100?" e "+gaPt(n%100):"");
  if(n<1e6){var k=Math.floor(n/1000),r=n%1000;return(k===1?"mil":gaPt(k)+" mil")+(r?(r<100||r%100===0?" e ":" ")+gaPt(r):"")}return String(n)}
/* a number in words, EN and PT (a year also as "fifteen hundred", "seventeen fifty five"); none under 10 (one, um: everyday words) */
function gaNumWords(n){if(!(n>=10&&n<1e6))return[];var o=[gaEn(n),gaEn(n,false),gaPt(n)];if(n>=1100&&n<2000){var h=Math.floor(n/100),r=n%100;o.push(gaEn(h)+" hundred"+(r?" and "+gaEn(r):""));if(r>=10)o.push(gaEn(h)+" "+gaEn(r))}
  return o.filter(function(x,i){return o.indexOf(x)===i})}
function gaNum(t){return String(t||"").replace(/(\d)[\s,.\u00a0\u202f](?=\d{3}\b)/g,"$1")}
function gaKeys(g){if(!g)return null;var right=g.kind==="guess"?String(g.n):g.opts&&g.ok>=0?g.opts[g.ok]:null;if(right==null)return null;var wrong=[],k={words:[],nums:[],phr:[]};
  if(g.kind!=="guess")g.opts.forEach(function(o,i){if(i!==g.ok)gaToks(gaNum(o)).forEach(function(s){wrong.push(s)})});
  gaToks(gaNum(right)).forEach(function(s){if(/^\d+$/.test(s)){k.nums.push(s);gaNumWords(+s).forEach(function(p){k.phr.push(p)})}else if(wrong.indexOf(s)<0)k.words.push(s)});return k}
function gaSpoils(t,k){if(!k||!t)return false;var c=gaNum(t),tk=gaToks(c),n=" "+gaNorm(c).replace(/[^a-z0-9]+/g," ")+" ";
  return k.words.some(function(s){return tk.indexOf(s)>=0})||k.nums.some(function(s){return tk.indexOf(s)>=0})||k.phr.some(function(p){return n.indexOf(" "+p+" ")>=0})}
function gaSentences(t){return(String(t||"").match(/[^.!?…]+[.!?…]+["”»)]*|[^.!?…]+$/g)||[]).map(function(s){return s.trim()}).filter(Boolean)}
/* the game whose answer is kept: the paused one, else the card's own quiz on screen (started or not); not when already answered */
function gaGuarded(){var A=G.ga,g=A.on?A.g:gaGame({any:true});return g&&!g.answered&&(g.kind==="quiz"||g.kind==="guess"||g.kind==="kc")?g:null}
function gaWhereLine(g){var w=gaWhere(g);return w?fmt(TXT("ga_h1_look"),w.title):g.hint?fmt(TXT("ga_h1_own"),g.hint):TXT("ga_none")}
/* round 21 (G.1, the Architect: "Who lived here?" during the quiz got hint 1's "Have a look at …: the answer is hiding there"): a question
   about the quiz's own question keeps the hint (where to look); any other question is answered with every sentence that does not give the
   quiz away, and only when something had to go the mascot says warmly that it will tell after the quiz (ga_spoil; ga_spoil_more after the
   part it could say) */
on("answer.text",function(t,q){var g=gaGuarded(),k=g&&gaKeys(g);if(!t||!gaSpoils(t,k))return t;var pre=(/^\([^)]*\)\s*/.exec(t)||[""])[0],ss=gaSentences(t.slice(pre.length)).filter(function(s){return!gaSpoils(s,k)});
  var about=!!q&&gaAbout(String(q),g,S.guideCfg.gaStrict).about;
  if(about&&!ss.length&&G.askFound)ss=gaSentences(G.askFound).filter(function(s){return!gaSpoils(s,k)}).slice(0,1);
  var out=about?(ss.length?ss.join(" "):gaWhereLine(g)):ss.length?ss.join(" ")+" "+TXT("ga_spoil_more"):TXT("ga_spoil");
  if(typeof gdLog==="function")gdLog("event","Ask during a game: an answer would have given "+g.label+"'s answer away; said instead: “"+out+"”");return pre+out});

/* ----- the Editor: Guide › 3 · Games on the tour › Asking during a game ----- */
/* the card's own quiz as a game (no screen needed): the Editor's read-backs, the self-test's signature */
function gaSample(l){var b=(S.card||[]).filter(function(x){return x.kind==="Knowledge Check"})[0];if(!b)return null;var w=kindWords(b),x=function(o){return wordIn(o,l||S.lang)};
  return{kind:"kc",bid:b.id,label:"the card's quiz",q:x(w.q),opts:(w.choices||[]).map(x),ok:+w.ok||0,why:"",hint:"",answered:false,lad:{}}}
var GA_TRY={en:["Which king moved the court?","Which king moved his great ships, soldiers and horses to Belém?","Which king rebuilt the city after the earthquake?","Who built the library?"],pt:["Que rei mudou a corte?","Que rei mudou os seus grandes navios, soldados e cavalos para Belém?","Que rei reconstruiu a cidade depois do terramoto?","Quem construiu a biblioteca?"]};
function gaLadderNow(l){var g=gaSample(l);if(!g)return[];var o=[];for(var i=0;i<3;i++){var s=gaSteps(g),n=Math.min(g.lad.n||0,Math.max(0,s.length-1)),st=s.length?s[n]:0;o.push(st?gaStepWords(st,g,l):wordIn(S.tx.ga_none,l));if(s.length)g.lad.n=n+1;if(st===2){if(g.kind==="guess"){var bd=gaBand(g);if(bd)g.lad.band=bd}else g.lad.out=gaWrongOf(g)}}return o}
function gaDetectNow(l,level){var g=gaSample(l);return(GA_TRY[l]||GA_TRY.en).map(function(q){return gaAbout(q,g,level).about})}
function gaEdHtml(){var c=S.guideCfg,l=S.lang||"en",lad=gaLadderNow(l),yn=function(b){return b?"yes":"no"},det=function(lv){return gaDetectNow(l,lv).map(yn).join(" · ")},bl=gaBackLine(svPoi(),l,0),q0=Object.keys(S.tourQ).filter(function(id){return gdQOk(S.tourQ[id],l)})[0]||"g1";
  return'<div class="note teone"><b>One rule for every game on screen</b> (the quiz, Guess the year, Don\'t say it!, the breather, Find it on the wall, and the card\'s own quiz and poll outside a tour): Ask opens, the game pauses where it is with a small “'+TXH("ga_paused")+'” chip on it; the mascot answers in the panel; “'+TXH("ga_back")+'” (or by itself) and the game goes on exactly where it stopped. A question about the question itself gets a hint, never the answer.</div>'+
    row("Ask during a game",pseg("guideCfg.gaOn",["true","false"],["On","Off"]),"On: Ask opens during a game and the game pauses. Off: the game never pauses; a question during it gets “"+TXT("ga_off")+"” on the game, and the answer comes once it is played.")+
    row("Back by itself",pseg("guideCfg.gaBack",["true","false"],["On","Off"]),"On: after the mascot's answer has been said, the game goes on by itself after the delay below (a bar empties on “"+TXT("ga_back")+"”); any tap or a new question cancels it. Off: only the button (or Play) goes back.")+
    (c.gaBack?row("Its delay",pslider("guideCfg.gaBackS",2,8,1),"Seconds after the answer has been said (2-8; 4 by default).",true):"")+
    '<div class="sub2 gth">The hint ladder <span class="sub">· a question about the question on screen: one step an ask, the Off steps skipped</span></div>'+
    row("Where to look",pseg("guideCfg.gaH1",["true","false"],["On","Off"]),"Hint 1: the question's own written hint (below, beside each question), else the card's block whose words hold the answer, found with no AI and named by its heading, with a Show me chip that takes the card there while the game stays paused. Now for the card's quiz: “"+(lad[0]||"")+"”")+
    row("One answer out",pseg("guideCfg.gaH2",["true","false"],["On","Off"]),"Hint 2: a wrong choice fades (a question with 3 choices; with 2 it would give the answer away, so it is skipped); Guess the year's slider narrows to the half that holds the year. A game with no choices skips it. Now: “"+(lad[1]||"")+"”")+
    row("Tell me, no star",pseg("guideCfg.gaH3",["true","false"],["On","Off"]),"Step 3: “"+TXT("ga_h3")+"” with "+TXT("ga_tell")+" (the answer and its why; the question counts as not earned, then the next question) and "+TXT("ga_try")+" (back to the game). With every step Off: “"+TXT("ga_none")+"”")+
    row("Detection",psel("guideCfg.gaStrict",[["loose","Loose"],["normal","Normal"],["strict","Strict"]]),"How a question is known to be about the question on screen, with no AI: its content words shared with the question or its choices (the little words of EN and PT left out). Loose: any word shared (catches the most, and some questions about something else, “Did the king live here?”). Normal: two words, or a choice's word. Strict: as Normal, and half the visitor's words at least (misses a long question that only touches it). Words that ask for the answer itself (answer, hint, clue; resposta, pista, dica) always count. For the card's quiz now: “"+GA_TRY[l==="pt"?"pt":"en"].join("”, “")+"”: "+det(c.gaStrict)+". Loose "+det("loose")+"; Strict "+det("strict")+".")+
    '<div class="row"><label>Each question\'s own hint</label><div><span class="uses">Written beside each quiz question, EN and PT, with Draft with AI and Reviewed (an unreviewed hint never plays; the card\'s words stand in, and the row says so)</span> <button class="ubtn sm gdedited gdpplink" type="button" data-gdscript="q:'+esc(q0)+'" title="Opens Specific Marker › Guide script at a step\'s quiz question">Specific Marker › Guide script &#8250;</button></div>'+help("Hint 1 says a question's own hint when it has one, reviewed in the visitor's language (“"+TXT("ga_h1_own").replace("{0}","…")+"”); else the card's block is found from the card's words.")+'</div>'+
    '<div class="row"><label>Back to the game lines</label><div><span class="uses">“'+esc(bl?bl.text:"(no line)")+'” and '+Math.max(0,atIntroLines("gameback",l).length-1)+' more, in turn (never twice in a row)</span> <span class="atin"><button class="atplay" type="button" data-gademo="back" title="Play a game paused and back" aria-label="Show the game going on again on the phone">&#9656;</button></span> <button class="ubtn sm" type="button" data-goto="s7auto">Edit: Tour from the card › The mascot\'s intro lines › Back to the game &#8250;</button></div>'+help("The mascot's line on the game's chip as it goes on again, spoken: a set of the mascot's intro lines (Back to the game), picked by the point like the others, the next one each time.")+'</div>'+
    '<div class="note">Unity: GuideGameAsk (plain C#, EditMode-tested: the detection, its stop words and threshold, the ladder, the block finder over the card\'s CardStrings); a GameClock every game\'s coroutines wait on (unscaled time summed while not paused); the chip and the Back row UI Toolkit elements (USS, 44 px), the faded choice a USS class (no transition with Reduced Motion); the voice paused at its word time and resumed from it.</div>'}
on("edGuide.html",function(h){var d=document.createElement("div");d.innerHTML=h;var gg=innerOf(d,"gd:g:ontour");if(!gg)return h;
  gg.insertAdjacentHTML("beforeend",comp("s7gask","the game pauses, the mascot answers or hints, the game goes on",gaEdHtml()));var f=innerOf(d,"s7gask");if(f)f.insertAdjacentHTML("beforeend",ownbar("s7gask")+ownPanels("s7gask"));return d.innerHTML});
on("guidePath.answer",function(o){if(o==="s7gask")return["gd:g:ontour","s7gask"]});
/* each quiz question's own hint, beside its Why in Specific Marker › 3 · Guide script, per language: Draft with AI (never in the tests), Reviewed */
on("edMarker.html",function(h){if(h.indexOf("tqlang")<0)return h;var d=document.createElement("div");d.innerHTML=h;
  d.querySelectorAll(".tqlang").forEach(function(box){var why=box.querySelector('input[data-path$=".why.en"],input[data-path$=".why.pt"]');if(!why)return;var m=/^tourQ\.([^.]+)\.why\.(\w+)$/.exec(why.dataset.path);if(!m)return;var id=m[1],l=m[2],q=S.tourQ[id];if(!q)return;
    q.hint=q.hint||{};if(q.hint[l]==null)q.hint[l]="";q.hrev=q.hrev||{};var r=why.closest(".row");if(!r)return;
    r.insertAdjacentHTML("afterend",row("Hint",'<input class="u" data-path="tourQ.'+id+'.hint.'+l+'" value="'+esc(L1(q.hint,l))+'" placeholder="'+esc(l==="pt"?"Uma pista, sem dar a resposta":"A clue, never the answer")+'">'+
      '<div class="btnrow">'+(q.hsrc==="ai"?'<span class="badge gdwb">AI draft</span>':"")+'<button class="ubtn sm" type="button" data-gahai="'+id+'|'+l+'">'+(S.gaBusy===id+"|"+l?"Drafting…":"Draft with AI")+'</button><label class="ovrev"><input type="checkbox" data-gahrev="'+id+'|'+l+'"'+(q.hrev[l]?" checked":"")+'> Reviewed: the mascot can say it</label></div>'+
      (String(q.hint[l]||"").trim()&&!q.hrev[l]?'<div class="ovflag" role="note">Not reviewed: hint 1 uses the card\'s words instead.</div>':!String(q.hint[l]||"").trim()?'<div class="uses">None: hint 1 names the card\'s block that holds the answer.</div>':""),
      "Asking during a game › hint 1: what the mascot says when the visitor asks about this question during the quiz (a clue from the step's words, never the answer). Reviewed per language; an unreviewed hint never plays.",true))});return d.innerHTML});
function gaHintDraft(id,l){var q=S.tourQ[id],st=byId(S.fullSteps||S.guideSteps,id);if(!q||!st)return;if(typeof sampleFn!=="function"||!sampleFn){S.tqNote="AI is not available here (it runs inside the published Lab).";renderEditor();return}S.gaBusy=id+"|"+l;renderEditor();
  var prompt="Write one short hint (at most 14 words) for this quiz question, for a visitor who heard the text below. Point them to where the answer is or to a clue in the text; never say the answer or rule out a choice. Use ONLY the text. Language: "+(l==="pt"?"European Portuguese (the visitor addressed politely in the third person)":"English")+". Return the hint only.\n\nQUESTION: "+L1(q.q,l)+"\nRIGHT ANSWER (never say it): "+L1((q.a||[])[+q.ok],l)+"\nTEXT: "+stepTextIn(st,l);
  sampleFn(prompt,{modelTier:"quick"}).then(function(r){S.gaBusy=null;q.hint=q.hint||{};q.hint[l]=String(r&&r.text||"").trim().replace(/^["“]|["”]$/g,"");q.hsrc="ai";q.hrev=q.hrev||{};q.hrev[l]=false;renderEditor()}).catch(function(){S.gaBusy=null;S.tqNote="The AI could not answer; try again.";renderEditor()})}
ed.addEventListener("click",function(e){var t=e.target.closest&&e.target.closest("[data-gahai],[data-gademo]");if(!t)return;e.stopPropagation();if(t.dataset.gahai){var q=t.dataset.gahai.split("|");gaHintDraft(q[0],q[1]);return}if(t.dataset.gademo)gaDemo("back")},true);
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.gahrev)return;e.stopImmediatePropagation();var q=t.dataset.gahrev.split("|"),o=S.tourQ[q[0]];if(o){(o.hrev=o.hrev||{})[q[1]]=t.checked;renderEditor()}},true);
/* each row's demo: a game started (the quiz on the open point), paused by a question typed in the panel, the answer or the hint, and back */
var GA_DEMO_Q={en:{about:"Which king moved the court?",other:"Who lived here?",near:"Which king rebuilt the city after the earthquake?"},pt:{about:"Que rei mudou a corte?",other:"Quem viveu aqui?",near:"Que rei reconstruiu a cidade depois do terramoto?"}};
function gaDemo(key){var l=S.lang==="pt"?"pt":"en",D=GA_DEMO_Q[l],c=S.guideCfg,id=null,m=/^tourQ\.([^.]+)\./.exec(key||"");if(m)id=m[1];
  var first=id||"g4",qs=[first].concat(Object.keys(S.tourQ).filter(function(k){return k!==first&&gdQOk(S.tourQ[k],l)}).slice(0,2)),qq=S.tourQ[first],aq=id||/gaH|gaStrict/.test(key)?null:D.other,step=/gaH2/.test(key)?2:/gaH3/.test(key)?3:1;
  if(G.ga.on)gaEnd("an Editor demo");gdDemoBeat("quizfinale","Asking during a game · "+(key==="back"?"back to the game":key.replace(/^guideCfg\./,"")),{qs:qs,ms:30000});var my=G.demoSeq,tries=0;
  (function go(){if(G.demoSeq!==my&&key!=="back")return;var B=G.gd.bt;if(!B||B.kind!=="finale"||!ph.querySelector(".gdbeat .bt-choice")){if(tries++<60)setTimeout(go,100);return}
    var g=gaGame();if(!g)return;var q=aq||(qq?L(qq.q):D.about);if(/gaStrict/.test(key))q=D.near;g.lad.n=Math.max(0,step-1);
    S.dockH="peek";renderGuide();setTimeout(function(){if(G.gd.bt===B)ask(q)},350)})()}
rowDemos(function(key){if(!/^guideCfg\.ga[A-Z]/.test(key)&&!/^tourQ\.[^.]+\.(hint|hrev)\./.test(key))return;return{tour:true,dock:"mini",wait:0,act:function(){gaDemo(key)},ringAfter:2600,ring:function(){return G.ga.on?".ga-row|.ga-chip|.gdock":".ga-note|.gdbeat|.gdock"}}});
/* the phone's signature (the self-test): what the visitor gets from these rows, for the open point's quiz */
on("modelSig.sig",function(h){if(!sigWant(/guideCfg\.ga[A-Z]/))return h;var c=S.guideCfg,l=S.lang||"en";if(!c.gaOn)return h+"#ga:off|"+TXT("ga_off");
  return h+"#ga:"+(c.gaBack?c.gaBackS:0)+"|"+gaLadderNow(l).join("|")+"|"+gaDetectNow(l,c.gaStrict).join(",")});
