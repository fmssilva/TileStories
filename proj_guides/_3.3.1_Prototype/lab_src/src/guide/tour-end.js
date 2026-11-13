/* The tour's end (guidance round 17c; the developer, 2026-11-12: "think better what the options should really be at the end of the tour …
   leave the visitor in a nice, entertaining place to go on exploring by themselves"). Every tour, from the card or written, ends the same way
   (tour.js's tourEnd.answer): after its end line the card scrolls, by the tour's own smooth scroll, to the card's showcase (TE_SHOW, the one
   list: a game or the quiz not played yet, a 3D model, a video, a gallery, a before and after; none: the hero), the block whole in the part
   of the screen above the panel; Azu stands on the panel's top edge and asks, from Azu's intro lines › After the tour (picked by the point,
   the next line at each end: never the same twice in a row), spoken, the bubble fading a few seconds later; the What next? panel rises above
   the bottom bar (at most 34 % of the phone's height): a header (a check, "Tour complete · 2 min", the quiz's star and score when it was
   played), then its sections in the Editor's order: suggested questions (2-3 chips: this point's own, else templates from the card's own
   words, never one the tour answered word for word; a tap asks Ask Azu at once), Ask right away (a field and a microphone, Ask Azu's own
   path), Explore nearby (3-5 points of the wall: the Editor's picks, then the same category, then the nearest by the wall's coordinates; a
   tap flies the wall to the point and opens its card with its invitation) and small actions (Start again · Play the quiz · Done).
   The panel's chevron or a swipe down makes it a 56 px pill with Azu's face (a tap opens it again); Done or the pill's × closes it, and the
   ✦ menu offers What next? until the visitor leaves the point; another tour, the quiz and Ask Azu's full view close it. It is not modal:
   the card behind still scrolls and plays. Reduced motion: no movement, a fade only.
   Unity: GuideTourEnd (plain C#: the showcase from the card's blocks and TE_SHOW's order, the questions from the card's words and the
   CardStrings templates, the nearby order from the marker's picks, its category and the wall's coordinates; EditMode-tested); the panel a UI
   Toolkit VisualElement with two snap heights (Open, Minimal: a PanelSnap manipulator on its header), its chips and cards in horizontal
   ScrollViews, its rise and the cards' stagger as USS transitions (translate and opacity, transition-delay per card; DOTween for the
   ScrollView's scrollOffset to the showcase); Reduced Motion: opacity only (AccessibilitySettings) */
S.guideCfg.teOn=true;S.guideCfg.teWhere="show";S.guideCfg.teBlock={};S.guideCfg.teStart="open";
S.guideCfg.teSec={qs:true,ask:true,near:true,act:true};S.guideCfg.teOrder=["qs","ask","near","act"];
S.guideCfg.teQsN="3";S.guideCfg.teQsSrc="own";S.guideCfg.teQs={painting:[T("Who lived in the palace?","Quem vivia no paço?")]};
S.guideCfg.teNearN="3";S.guideCfg.teNearRule="pcd";S.guideCfg.tePicks={};Object.keys(POINTS).forEach(function(k){S.guideCfg.tePicks[k]={p1:"",p2:""}});
COMP.s7end=["Guide","The tour's end"];FOCUS.s7end=["card",".hero","full"];
G.te={open:false,offer:false,poi:null,len:0,quiz:null,played:{},steps:null,ends:{},line:null,inv:null};
/* the showcase: the first block of the card in this order (the one list; Unity: GuideTourEnd.ShowcaseOrder) */
var TE_SHOW=[["play",["Knowledge Check","Poll"],"a game or the quiz the visitor has not played"],["model",["3D Model"],"a 3D model"],["video",["Video"],"a video"],
  ["gallery",["Gallery"],"a gallery"],["ba",["Before and After"],"a before and after"]];
var TE_SECS={qs:"Suggested questions",ask:"Ask right away",near:"Explore nearby",act:"Small actions"};
/* the region the showcase sits in: below the card's close and settings, above the panel (and Azu's bubble when the block fits there too) */
var TE_TOP=64,TE_FLY_MS=750,TE_FLY_Y=.3,TE_BUB_MS=3500;
function teCfg(){return S.guideCfg}
function teBlocks(poi){return pointOf(poi).card||[]}
function teName(poi,lang){var h=pointOf(poi).hero;return h&&h.title?wordIn(h.title,lang||S.lang):""}
/* ----- the quiz played (the card's own Knowledge Check, by a tap on an answer; this visit) ----- */
function tePlayed(poi,id){return!!G.te.played[(poi||svPoi())+"|"+id]}
ph.addEventListener("click",function(e){var b=e.target.closest&&e.target.closest(".kq button");if(!b)return;var blk=b.closest('[data-ref^="b:"]'),id=blk?blk.dataset.ref.slice(2):null;
  if(!id&&S.view==="kpage"){var k=S.card.filter(function(x){return x.kind===S.pageKind})[0];id=k&&k.id}if(id)G.te.played[svPoi()+"|"+id]={right:!!b.dataset.ok}},true);
/* the header's star: the games' quiz played in this tour (its score), else the card's quiz answered this visit */
function teQuizNow(poi){var P=G.gd&&G.gd.plan;if(P&&P.quizScore&&(P.point||"painting")===poi)return{right:P.quizScore.right,n:P.quizScore.n};
  var q=teBlocks(poi).filter(function(b){return b.kind==="Knowledge Check"&&tePlayed(poi,b.id)});if(!q.length)return null;
  return{right:q.filter(function(b){return G.te.played[poi+"|"+b.id].right}).length,n:q.length}}
function teQuizLeft(poi){return teBlocks(poi).filter(function(b){return b.kind==="Knowledge Check"&&!tePlayed(poi,b.id)})[0]||null}
/* ----- 1. where the tour leaves the visitor ----- */
function teBlockOf(poi){var v=(teCfg().teBlock||{})[poi],card=teBlocks(poi);return v&&byId(card,v)?v:(card[0]||{}).id||""}
function teBlockName(b){if(!b)return"";var w=typeof blockSays==="function"?blockSays(b,S.lang)||{}:{},t=w.title||w.q||w.cap||"";return"the "+b.kind+(t?" “"+String(t).replace(/[.!?…]+$/,"")+"”":"")}
function teShowcase(poi){poi=poi||svPoi();var c=teCfg(),card=teBlocks(poi);if(c.teWhere==="hero")return{ref:"hero",why:"the hero (Where the tour ends: The hero)"};
  if(c.teWhere==="pick"){var b=byId(card,teBlockOf(poi));if(b)return{ref:"b:"+b.id,id:b.id,kind:b.kind,why:"the block picked"+((c.teBlock||{})[poi]===b.id?"":" (none picked yet: the card's first block)")}}
  for(var g=0;g<TE_SHOW.length;g++){var x=TE_SHOW[g],hit=card.filter(function(k){return x[1].indexOf(k.kind)>=0&&!(x[0]==="play"&&tePlayed(poi,k.id))})[0];if(hit)return{ref:"b:"+hit.id,id:hit.id,kind:hit.kind,why:x[2]}}
  return{ref:"hero",why:"no block that invites play or looking: the hero"}}
/* the hero: its words (eyebrow, title, line and Start), whole above the panel; its picture is their backdrop */
function teShowEl(sc){return sc.ref==="hero"?scr.querySelector(".hero .w")||scr.querySelector(".hero"):scr.querySelector('[data-ref="'+sc.ref+'"]')}
function tePR(el){var pr=ph.getBoundingClientRect(),q=pr.height/ph.offsetHeight||1,r=el.getBoundingClientRect();return{t:(r.top-pr.top)/q,b:(r.bottom-pr.top)/q,l:(r.left-pr.left)/q,r:(r.right-pr.left)/q,h:r.height/q,q:q}}
/* the part of the screen above the panel (its top, its bottom), and Azu's bubble's top while it shows */
/* round 18 (the Architect): the panel and the bubble by their layout (their final place, not where their rise is now), so the showcase is
   placed once, while they still rise */
function teLay(el){var t=0,e=el;while(e&&e!==ph){t+=e.offsetTop;e=e.offsetParent}return t}
function teRegion(){var H=ph.offsetHeight,bot=H,bub=null,p=ph.querySelector(".tepanel,.tepill"),b=ph.querySelector(".tebub:not(.te-out)");if(p)bot=teLay(p);if(b)bub=teLay(b);return{top:TE_TOP,bot:bot-12,bub:bub!=null?bub-10:null}}
/* the scroll that puts the block whole in that part, a little above its middle (the hero: the top) */
function teShowTop(el,hero){if(hero)return 0;var R=teRegion(),r=tePR(el),s=tePR(scr),y0=r.t-s.t+scr.scrollTop,bot=R.bub!=null&&r.h<=R.bub-R.top?R.bub:R.bot,room=bot-R.top,want=r.h<=room?R.top+(room-r.h)*.4:R.top;
  return Math.max(0,Math.min(scr.scrollHeight-scr.clientHeight,Math.round(y0-(want-s.t))))}
function teShowcaseGo(){var sc=teShowcase(G.te.poi),el=teShowEl(sc);G.te.sc=sc;if(!el)return;var hero=el.classList.contains("hero"),top=teShowTop(el,hero);G.te.scTop=top;G.te.userScroll=false;
  /* one smooth scroll to the final place (computed after the panel, its foot padding and Azu's bubble are laid out): no correction */
  scr.scrollTo({top:top,behavior:S.reduce?"auto":"smooth"})}
["wheel","touchmove","pointerdown"].forEach(function(ev){scr.addEventListener(ev,function(){G.te.userScroll=true},{passive:true})});
/* ----- 2. Azu asks: a line of After the tour, picked by the point, the next one at each end there (never the same twice in a row) ----- */
function teAfterLine(poi,lang,k){var cap=+teCfg().atIntroCap||20,ls=atIntroLines("after",lang).filter(function(x){return x.max<=cap});if(!ls.length)return null;
  var f=G.te.lineForce?ls.filter(function(x){return x.key===G.te.lineForce})[0]:null,x=f||ls[(atSeed(poi)+atIntroSalt("after")+(k||0))%ls.length];
  var t=/\{point\}/.test(x.text)?nameIn(x.text,"{point}",teName(poi,lang),lang):x.text;return{key:x.key,text:t.charAt(0).toUpperCase()+t.slice(1)}}
function teAzuAsk(){var l=teAfterLine(G.te.poi,S.lang,G.te.k);G.te.lineForce=null;G.te.line=l;if(!l||G.te.open!=="open")return;G.te.bubAt=performance.now();teDraw();
  var tok=G.te.bubTok=(G.te.bubTok||0)+1,fade=function(ms){clearTimeout(G.te.bubT);G.te.bubT=setTimeout(function(){if(G.te.bubTok===tok)teBubFade()},ms)};
  fade(12000);say(l.text,function(){},function(){if(G.te.bubTok===tok)fade(TE_BUB_MS)})}
function teBubFade(){var b=ph.querySelector(".tebub");G.te.line=null;if(!b)return;b.classList.add("te-out");setTimeout(function(){if(b.parentNode)b.remove()},420)}
/* ----- 3. the suggested questions: this point's own (the Editor), then templates from the card's own words (no AI), never one the tour
   answered word for word (its answer's words all said by the tour) nor a sentence it said ----- */
var TE_QW={en:/^(what|who|why|how|when|where|which)\b/i,pt:/^(o que|quem|porqu[eê]|como|quando|onde|qual|quais)\b/i},TE_PREP={en:/^(on|in|at|by|from|to|under|with|of|for|around)\s/i,pt:/^(na|no|nas|nos|em|junto|sobre|com|de|da|do|das|dos|pelo|pela|por|para|até)\s/i},TE_JOIN=/^(of|the|and|do|da|dos|das|de|e)$/;
function teProperName(t){var ws=String(t).trim().split(/\s+/);return ws.length<=5&&!/\d/.test(t)&&ws.every(function(w,i){return TE_JOIN.test(w)&&i>0||/^\p{Lu}/u.test(w)})}
/* the card's words a template can use, in its order: headings (a story's, a text's title), dates (a timeline's, a fact's), kings and queens
   (named in its words), names (a fact's value written as a name); each with its answer's words */
function teCardBits(poi,lang){var p=pointOf(poi),out=[],add=function(k,t,ans,at){t=String(t||"").trim();if(t)out.push({k:k,t:t,ans:String(ans||"").trim(),at:at})},all=[];
  var heading=function(t,ans,at){t=String(t||"").trim().replace(/[.:]+$/,"");if(!t||atWords(t)>7)return;add(TE_QW[lang]&&TE_QW[lang].test(t)?"ask":"more",t,ans,at)};
  (p.card||[]).forEach(function(b){var w=typeof blockSays==="function"?blockSays(b,lang)||{}:{};
    if(b.kind==="Quick Facts")(w.facts||[]).forEach(function(f){var y=/\b(1\d{3}|20\d\d)\b/.exec(f[1]);if(y)add("year",y[1],f[0]+" "+f[1],"b:"+b.id);else if(teProperName(f[1]))add("more",f[1],f[0]+" "+f[1],"b:"+b.id);all.push(f[1])});
    if(b.kind==="Inline"&&w.title){heading(w.title,w.text,"b:"+b.id)}if(w.text)all.push(w.text);if(w.back)all.push(w.back);
    if(b.kind==="Story"&&w.title){heading(w.title,w.text,"b:"+b.id)}
    if(b.kind==="Timeline")(w.events||[]).forEach(function(e){if(/^\d{4}$/.test(String(e[0]).trim()))add("year",String(e[0]).trim(),e[1],"b:"+b.id);all.push(e[1])})});
  var rows=p.story||[];rows.forEach(function(r,i){var txt=function(o){return o?wordIn(o,lang):""};
    if(r.kind==="heading"&&r.c){var nx=rows[i+1];heading(txt(r.c.text),nx&&nx.c&&nx.c.text?txt(nx.c.text):"","r:"+r.id)}
    if(r.kind==="paragraph"&&r.c)all.push(txt(r.c.text));
    if(r.pattern&&r.slots){var sl=r.slots,h0=sl[0]&&sl[0].text&&!sl[0].img?txt(sl[0].text):"";if(h0&&!/[.!?]$/.test(h0)&&sl.length>1)heading(h0,sl.slice(1).map(function(x){return txt(x.text)}).join(" "),"r:"+r.id);
      sl.forEach(function(x){if(x.text)all.push(txt(x.text))})}});
  var seen={};all.forEach(function(t){var re=lang==="pt"?/\b((?:D\.\s)?[A-ZÁÉÍÓÚ][\p{Ll}]+ [IVX]{1,3})\b/gu:/\b(?:King |Queen )?([A-Z][\p{Ll}]+ [IVX]{1,3})\b/gu,m;while((m=re.exec(t))){var n=m[1];if(!seen[n]){seen[n]=1;add("who",n,"","")}}});
  return out}
function teSaidOf(steps,lang){return" "+atNorm((steps||[]).map(function(s){return s.text?wordIn(s.text,lang):(function(){var r=byId(pointOf().story||[],String(s.ref||"").slice(2));return r&&r.c?wordIn(r.c.text,lang):""})()}).join(" "))+" "}
function teAnswered(said,ans){var a=atNorm(ans);return!!a&&atWords(a)>=2&&said.indexOf(" "+a+" ")>=0}
/* the chips, in the visitor's language: own first (Questions from: This point's own), then one of each template in turn (a heading that asks, a
   date, a person, a heading or a name), then the rest; at most How many */
function teQuestions(poi,lang,steps){poi=poi||svPoi();lang=lang||S.lang;var c=teCfg(),n=+c.teQsN||3,said=teSaidOf(steps||svTour(poi),lang),sents={},out=[],keys={};
  atSentences(said).forEach(function(x){sents[atNorm(x)]=1});(steps||svTour(poi)).forEach(function(s){atSentences(s.text?wordIn(s.text,lang):"").forEach(function(x){sents[atNorm(x)]=1})});
  var put=function(q,src){var k=atNorm(q);if(!k||keys[k]||sents[k]||out.length>=n)return;keys[k]=1;out.push({q:q,src:src})};
  if(c.teQsSrc!=="tpl")((c.teQs||{})[poi]||[]).forEach(function(o,i){var t=String(wordIn(o,lang)||"").trim();if(t)put(t,"own:"+i)});
  var bits=teCardBits(poi,lang).filter(function(b){return!teAnswered(said,b.ans)}),nm=teName(poi,lang),W=function(k){return wordIn(S.tx[k],lang)};
  var make=function(b){if(b.k==="ask")return nameIn(nameIn(W("te_tq_heading"),"{0}",b.t,lang),"{point}",nm,lang);if(b.k==="year")return fmt(W("te_tq_year"),b.t);if(b.k==="who")return fmt(W("te_tq_who"),b.t);
    if(TE_PREP[lang]&&TE_PREP[lang].test(b.t))return"";return nameIn(W("te_tq_more"),"{0}",teProperName(b.t)?b.t:atLowerFirst(b.t,lang),lang)};
  var order=["ask","year","who","more"],first=[],rest=[];order.forEach(function(k){var x=bits.filter(function(b){return b.k===k});if(x.length)first.push(x[0]);rest=rest.concat(x.slice(1))});
  first.concat(rest).forEach(function(b){var q=make(b);if(q)put(q,"tpl:"+b.k+(b.at?":"+b.at:""))});return out}
/* ----- 4. Explore nearby: the Editor's picks, then the same category, then the nearest (the wall's own coordinates); at most How many ----- */
function teWall(){return WALLS.ribeira}
function teDist(a,b){var wl=teWall();return Math.hypot((a.u-b.u)*wl.w,(a.v-b.v)*wl.h)}
function tePicksOf(poi){var pk=(teCfg().tePicks||{})[poi]||{},ps=teWall().pois||[],out=[];[pk.p1,pk.p2].forEach(function(id){if(id&&id!==poi&&out.indexOf(id)<0&&byId(ps,id))out.push(id)});return out}
function teLenWord(id,lang){if(!POINTS[id])return wordIn(S.tx.te_len_view,lang);var m=tourMinutes(id);return+m<2?wordIn(S.tx.te_len_short,lang):fmt(wordIn(S.tx.te_len,lang),m)}
function teNear(poi,lang){poi=poi||svPoi();lang=lang||S.lang;var c=teCfg(),ps=teWall().pois||[],me=byId(ps,poi);if(!me)return[];var picks=tePicksOf(poi);
  var rest=ps.filter(function(p){return p.id!==poi&&picks.indexOf(p.id)<0}).sort(function(a,b){return teDist(a,me)-teDist(b,me)});
  var same=c.teNearRule==="pcd"?rest.filter(function(p){return p.cat&&p.cat===me.cat}):[],other=rest.filter(function(p){return same.indexOf(p)<0});
  var list=(c.teNearRule==="d"?[]:picks.map(function(id){return{p:byId(ps,id),how:"pick"}})).concat(same.map(function(p){return{p:p,how:"cat"}}),other.map(function(p){return{p:p,how:"near"}}));
  if(c.teNearRule==="d")list=picks.map(function(id){return byId(ps,id)}).concat(rest).sort(function(a,b){return teDist(a,me)-teDist(b,me)}).map(function(p){return{p:p,how:"near"}});
  /* the developer (2026-11-12): the points with a card and a tour always first, then the places without one, each in the order above */
  list=list.filter(function(x){return!!POINTS[x.p.id]}).concat(list.filter(function(x){return!POINTS[x.p.id]}));
  return list.slice(0,Math.max(3,Math.min(5,+c.teNearN||3))).map(function(x){var p=x.p,cw=x.how==="cat"&&S.tx["te_why_"+p.cat]?wordIn(S.tx["te_why_"+p.cat],lang):"";
    return{id:p.id,name:wordIn(p.name,lang),how:x.how,why:cw||fmt(wordIn(S.tx[x.how==="pick"?"te_why_pick":"te_why_near"],lang),teLenWord(p.id,lang)),card:!!POINTS[p.id],d:Math.round(teDist(p,me))}})}
/* a point's picture: its hero's crop of the wall (a point with no card: the wall around it), else its hero's picture. Unity: a uvRect */
function teThumb(id,B){B=B||44;var wl=teWall(),p=byId(wl.pois||[],id),h=POINTS[id]?pointOf(id).hero:null;
  if(h&&!h.crop&&h.pic&&IMG[h.pic])return"background-image:url("+IMG[h.pic]+");background-size:cover;background-position:"+(h.fx!=null?h.fx:50)+"% "+(h.fy!=null?h.fy:50)+"%";
  if(!p)return"";var s=B/(wl.h*.34),bw=wl.w*s,bh=wl.h*s,x=Math.min(0,Math.max(B-bw,B/2-p.u*bw)),y=Math.min(0,Math.max(B-bh,B/2-p.v*bh));
  return"background-image:url("+wl.img()+");background-size:"+bw.toFixed(1)+"px "+bh.toFixed(1)+"px;background-position:"+x.toFixed(1)+"px "+y.toFixed(1)+"px"}
/* ----- the panel's markup ----- */
var TE_ICO={ok:'<svg viewBox="0 0 16 16" aria-hidden="true"><path d="M3.6 8.3l2.9 2.9 5.9-6.3"/></svg>',star:'<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 3.5l2.6 5.3 5.8.8-4.2 4.1 1 5.8L12 16.8l-5.2 2.7 1-5.8-4.2-4.1 5.8-.8z"/></svg>',
  down:'<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6.5 9.5l5.5 5.5 5.5-5.5"/></svg>',up:'<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6.5 14.5L12 9l5.5 5.5"/></svg>',x:'<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M7 7l10 10M17 7L7 17"/></svg>'};
function teHeadHtml(poi){var m=G.te.poi===poi&&G.te.len?G.te.len:tourMinutes(poi),q=G.te.poi===poi?G.te.quiz:null,pz=tePausedOf(poi),done=pz?fmt(TXT("te_paused"),pz.n,pz.of):+m<2?TXT("te_done_short"):fmt(TXT("te_done"),tourLength(m));
  return'<div class="te-head"><span class="te-ok">'+TE_ICO.ok+'</span><b class="te-hd">'+esc(done)+'</b>'+(q?'<span class="te-star" aria-label="'+esc(fmt(TXT("te_score"),q.right,q.n))+'">'+TE_ICO.star+'<span>'+esc(fmt(TXT("te_score"),q.right,q.n))+'</span></span>':"")+
    '<button type="button" class="te-chev" data-te="min" aria-label="'+TXH("te_min")+'">'+TE_ICO.down+'</button>'+(pz?'<button type="button" class="te-chev te-hx" data-te="done" aria-label="'+TXH("te_done_btn")+'">'+TE_ICO.x+'</button>':"")+'</div>'}
function teSecHtml(id,poi){var c=teCfg();if(c.teSec&&c.teSec[id]===false)return"";
  if(id==="qs"){var qs=teQuestions(poi,S.lang,G.te.poi===poi&&G.te.steps?G.te.steps:null);G.te.qs=qs;return qs.length?'<div class="te-qs te-sc" role="group" aria-label="'+TXH("te_qs")+'">'+qs.map(function(x,i){return'<button type="button" class="te-q" data-teq="'+i+'" style="--i:'+i+'">'+esc(x.q)+'</button>'}).join("")+'</div>':""}
  if(id==="ask"){var phd=nameIn(TXT("te_ask_ph"),"{point}",teName(poi),S.lang);return'<form class="te-ask" autocomplete="off"><input id="teq" class="te-field" type="text" enterkeyhint="send" placeholder="'+esc(phd)+'" aria-label="'+esc(phd)+'"><button type="button" class="te-mic" data-te="mic" aria-label="'+TXH("te_mic")+'">'+MIC+'</button></form>'}
  if(id==="near"){var ns=teNear(poi);return ns.length?'<div class="te-near te-sc" role="list" aria-label="'+TXH("te_near")+'">'+ns.map(function(x,i){return'<button type="button" role="listitem" class="te-card" data-tenear="'+x.id+'" style="--i:'+i+'"><i class="te-th" style="'+teThumb(x.id)+'"></i><span class="te-ct"><b>'+esc(x.name)+'</b><small>'+esc(x.why)+'</small></span></button>'}).join("")+'</div>':""}
  if(id==="act"){var qz=teQuizLeft(poi),pz=tePausedOf(poi);
    /* paused (the Stop bar's What next?): Continue first, then Start again; Done is the header's × (the row stays one line in EN and PT); the quiz comes with the tour */
    if(pz)return'<div class="te-acts te-pz"><button type="button" class="te-cont" data-te="cont">'+esc(svLabel(pz))+'</button><button type="button" data-te="again">'+TXH("te_again")+'</button></div>';
    return'<div class="te-acts"><button type="button" data-te="again">'+TXH("te_again")+'</button>'+(qz?'<i class="te-dot" aria-hidden="true"></i><button type="button" data-te="quiz">'+TXH("te_quiz")+'</button>':"")+'<i class="te-dot" aria-hidden="true"></i><button type="button" class="te-fin" data-te="done">'+TXH("te_done_btn")+'</button></div>'}
  return""}
function tePanelHtml(poi){poi=poi||svPoi();return'<i class="te-grab" aria-hidden="true"></i>'+teHeadHtml(poi)+(teCfg().teOrder||[]).map(function(id){return teSecHtml(id,poi)}).join("")}
function tePillHtml(){var face=S.guideCfg.mascot!==false?'<span class="te-head-crop">'+mascotStill()+'</span>':GBSTAR;return'<button type="button" class="te-po" data-te="open" aria-label="'+TXH("te_title")+'"><span class="te-face">'+face+'</span><span class="te-pt">'+TXH("te_title")+'</span>'+TE_ICO.up+'</button><button type="button" class="te-x" data-te="done" aria-label="'+TXH("te_close")+'">'+TE_ICO.x+'</button>'}
/* ----- drawing and placing ----- */
function teShown(){return!!G.te.open&&!G.on&&(S.view==="card"||S.view==="story"&&G.te.mode==="paused")&&(S.stop==="full"||S.stop==="half")&&G.te.poi===svPoi()&&!S.settingsOpen&&!S.contentsOpen}
function teDraw(){var want=teShown()?G.te.open:null,pn=ph.querySelector(".tepanel"),pl=ph.querySelector(".tepill");
  if(want!=="open"){if(pn)pn.remove();ph.querySelectorAll(".teazu,.tebub").forEach(function(x){x.remove()})}if(want!=="min"&&pl)pl.remove();
  if(!want){teInvDraw();if(G.te.pad){G.te.pad=false;scr.style.paddingBottom="";if(typeof fabLayer==="function")fabLayer()}return}
  var anim=G.te.anim;G.te.anim=false;
  if(want==="open"){var h=tePanelHtml(G.te.poi),k=S.lang+"|"+S.dark+"|"+S.reduce+"|"+h;if(!pn||pn._k!==k){var typed=pn&&pn.querySelector("#teq")?pn.querySelector("#teq").value:"";if(pn)pn.remove();pn=document.createElement("section");pn.className="tepanel"+(anim?" te-in":"");
      pn.setAttribute("role","region");pn.setAttribute("aria-label",TXT("te_title"));pn.innerHTML=h;pn._k=k;ph.appendChild(pn);if(typed){var i=pn.querySelector("#teq");if(i)i.value=typed}}
    var az=ph.querySelector(".teazu");if(S.guideCfg.mascot!==false){if(!az){az=document.createElement("div");az.className="teazu"+(anim?" te-in":"");az.setAttribute("aria-hidden","true");az.innerHTML=mascotStill();ph.appendChild(az)}}else if(az)az.remove();
    var bb=ph.querySelector(".tebub");if(G.te.line){if(!bb||bb._t!==G.te.line.text){if(bb)bb.remove();bb=document.createElement("div");bb.className="tebub"+(S.guideCfg.mascot===false?" plain":"");bb.setAttribute("role","status");bb.setAttribute("aria-live","polite");
      bb.innerHTML=(S.guideCfg.mascot===false?'<i class="te-sign" aria-hidden="true">✦</i>':"")+'<span>'+esc(G.te.line.text)+'</span><i class="te-tail" aria-hidden="true"></i>';bb._t=G.te.line.text;ph.appendChild(bb)}}else if(bb&&!bb.classList.contains("te-out"))bb.remove()}
  else{if(!pl){pl=document.createElement("div");pl.className="tepill te-in";pl.innerHTML=tePillHtml();ph.appendChild(pl)}}
  tePlace();if(typeof fabLayer==="function")fabLayer()}
/* above the bottom bar (the Navigation Bar when it shows), the full width with its top rounded; the pill on the side away from the ✦ */
function teFoot(){var nb=document.getElementById("navbar");if(nb&&getComputedStyle(nb).display!=="none"&&nb.offsetHeight)return Math.max(0,ph.offsetHeight-nb.offsetTop);return 0}
function tePlace(){var pn=ph.querySelector(".tepanel"),pl=ph.querySelector(".tepill"),f=teFoot(),H=ph.offsetHeight;
  if(pn){pn.style.bottom=f+"px";var top=H-f-pn.offsetHeight,az=ph.querySelector(".teazu"),bb=ph.querySelector(".tebub");
    if(az){az.style.bottom=(H-top-6)+"px"}if(bb){bb.style.bottom=(H-top+12)+"px";bb.classList.toggle("noazu",!az)}
    var pad=(H-top+16)+"px";if(scr.style.paddingBottom!==pad)scr.style.paddingBottom=pad;G.te.pad=true}
  if(pl){pl.style.bottom=(f+12)+"px";var right=S.guideCfg.gbSide!=="right";pl.style.right=right?"16px":"auto";pl.style.left=right?"auto":"16px"}}
setInterval(function(){if(noDraw)return;if(G.te.open&&(ph.querySelector(".tepanel,.tepill")||teShown())){if(teShown()!==!!ph.querySelector(".tepanel,.tepill"))teDraw();else tePlace()}teInvPlace()},250);
/* while the panel is open the ✦, Azu beside it and their band step aside (the panel has its own Azu; one focal point) */
on("fabLayer.after",function(){/* the pill carries Azu's face: the Azu beside the ✦ steps aside (one Azu) */if(teShown()&&G.te.open==="min"){ph.querySelectorAll(".gbtn-wrap:not(.intour) .azfab").forEach(function(x){x.remove()});return}
  if(!(teShown()&&G.te.open==="open"))return;ph.querySelectorAll(".gbtn-wrap:not(.intour),.gbband:not(.tour),.gbmenu").forEach(function(x){x.remove()});G.fab.menu=false;tePlace()});
on("renderPhone.after",function(){teDraw()});
/* ----- opening, the two heights, closing ----- */
function teSet(state){if(state&&!G.te.poi)return;var was=G.te.open;G.te.open=state||false;if(state&&!was)G.te.anim=true;if(state==="min"&&was==="open")G.te.anim=true;if(state==="open"&&was!=="open")G.te.anim=true;
  if(state&&(S.view!=="card"||S.stop!=="full"&&S.stop!=="half")){S.view="card";S.stop="full";renderPhone(false)}if(state!=="open")teBubFade();teDraw()}
function teOffered(){return!!G.te.offer&&G.te.poi===svPoi()&&G.te.open!=="open"}
function teClose(forget){G.te.open=false;if(forget)G.te.offer=false;teBubFade();teDraw()}
/* round 17c (the developer): the Stop bar's What next? opens the same panel, paused: its header "Tour paused · step N of M" and "Continue the
   tour (step N of M)" first of its small actions (as the Stop bar's: back to the sentence, within the visit); no showcase scroll, no question
   from Azu (the visitor asked for it); the questions leave out only what the tour said so far */
function tePausedOf(poi){return G.te.mode==="paused"&&G.te.poi===poi?(svOffer(poi)||G.te.pof):null}
function teOpenPaused(){var poi=svPoi(),of=svOffer(poi);if(!of)return;G.te.mode="paused";G.te.pof=of;G.te.poi=poi;G.te.steps=svTour(poi).slice(0,of.n);G.te.len=tourMinutes(poi);G.te.quiz=teQuizNow(poi);
  G.te.line=null;G.te.offer=true;G.te.open="open";G.te.anim=true;if(S.view!=="story")S.view="card";if(S.stop!=="full"&&S.stop!=="half")S.stop="full";teDraw()}
/* the end: the line said, the tour stops, the card goes to its showcase, the panel rises and Azu asks (Off: Ask opens, as before) */
on("tourEnd.answer",function(st){if(!teCfg().teOn)return;var poi=svPoi(),dk=G.te.demoKey;G.te.demoKey=null;
  G.te.steps=(S.guideSteps||[]).slice();G.te.poi=poi;G.te.len=tourMinutes(poi);G.te.quiz=teQuizNow(poi);G.te.k=(G.te.ends[poi]=(G.te.ends[poi]||0)+1)-1;
  stopTour();G.te.mode="end";G.te.pof=null;G.te.offer=true;G.te.open=teCfg().teStart==="min"?"min":"open";G.te.anim=true;teDraw();teAzuAsk();teShowcaseGo();
  if(dk)setTimeout(function(){ringPart(dk,teRingSel(dk))},S.reduce?60:320);return true});
/* another tour closes it (and the offer: a new tour has its own end); Ask Azu's full view closes it and keeps the offer; leaving the point ends both */
on("startTour.before",function(){if(G.te.askMode)teAskDrop();G.te.open=false;G.te.offer=false;G.te.inv=null;teBubFade();teDraw()});
on("point.opened",function(){G.te.open=false;G.te.offer=false;teDraw()});
/* ----- the taps ----- */
var teSw=null;
ph.addEventListener("pointerdown",function(e){var h=e.target.closest&&e.target.closest(".tepanel .te-head,.tepanel .te-grab,.tepill");if(h&&!e.target.closest(".te-x"))teSw={y:e.clientY,pill:!!e.target.closest(".tepill")}},true);
document.addEventListener("pointerup",function(e){if(!teSw)return;var s=teSw,dy=e.clientY-s.y,q=ph.getBoundingClientRect().height/ph.offsetHeight||1;teSw=null;if(Math.abs(dy)/q<28)return;
  if(!s.pill&&dy>0){G.te.swiped=performance.now();teSet("min")}else if(s.pill&&dy<0){G.te.swiped=performance.now();teSet("open")}},true);
ph.addEventListener("click",function(e){var t=e.target.closest&&e.target.closest("[data-te],[data-teq],[data-tenear],[data-teinv]");if(!t)return;e.stopPropagation();
  if(G.te.swiped&&performance.now()-G.te.swiped<400){G.te.swiped=0;return}
  if(t.dataset.teinv!=null){teInvGo();return}
  if(t.dataset.teq!=null){var x=(G.te.qs||[])[+t.dataset.teq];if(x)teAsk(x.q);return}
  if(t.dataset.tenear){teNearGo(t.dataset.tenear);return}
  var k=t.dataset.te;if(k==="min"){teSet("min");return}if(k==="open"){teSet("open");return}if(k==="done"){teClose(false);return}
  if(k==="again"){teClose(true);startTour();return}if(k==="cont"){teClose(false);svContinue();return}if(k==="quiz"){tePlayQuiz();return}if(k==="mic"){teMic();return}},true);
ph.addEventListener("submit",function(e){if(!e.target.classList||!e.target.classList.contains("te-ask"))return;e.preventDefault();e.stopPropagation();var i=e.target.querySelector("#teq");if(i&&i.value.trim())teAsk(i.value)},true);
/* Ask right away: Ask Azu's own path (its full view, as the ✦ menu's Ask), the question asked at once; the panel closes, the offer stays */
/* round 17c (the Architect): the panel's own way into Ask Azu. Ask's full view (the guide panel at Full, its question box) opens over the card where
   it is: no tour restarted at the teaser, no scroll, the kept places (a Stop's, the next visit's) untouched. The guide is "on" and paused at the
   step the visitor is at (the end step; paused: the stopped one), no step run; G.keepScroll holds the card. Ask closes (the panel lowered to Mini or
   hidden, Stop, Play, Next or Previous): the card back at its scroll, the panel back as it was (open or minimal). Unity: GuideTourEnd.AskFromPanel
   shows the Ask view over the card (no GuideTourPlayer call) and restores the panel's snap height when it closes */
function teAskEnter(){var poi=svPoi(),st=G.te.mode==="paused"?(svOffer(poi)||G.te.pof||{}).id:(teEndStep()||{}).id,j=Math.max(0,S.guideSteps.findIndex(function(x){return x.id===st}));
  G.te.askBack={top:scr.scrollTop,open:G.te.open||"open",mode:G.te.mode,view:S.view,stop:S.stop};G.te.open=false;teBubFade();teDraw();if(G.on)stopTour();
  G.te.askMode=true;G.keepScroll=true;G.on=true;G.paused=true;G.waiting=false;G.i=j;G.askOpen=true;S.dockH="full";renderGuide();scr.scrollTop=G.te.askBack.top}
/* Ask closed: the guide off (no step, no toast), the card where it was, the panel back */
function teAskDrop(){G.te.askMode=false;G.keepScroll=false}
function teAskLeave(){var b=G.te.askBack;teAskDrop();G.te.askBack=null;stopSpeech();G.on=false;G.paused=false;G.askOpen=false;G.typing=false;clearGuideMarks();renderGuide();if(typeof fabLayer==="function")fabLayer();
  if(!b)return;if(S.view!==b.view||S.stop!==b.stop){S.view=b.view;S.stop=b.stop;renderPhone(false)}G.te.mode=b.mode;G.te.open=b.open;G.te.anim=false;teDraw();G.te.userScroll=false;
  /* the card back at its scroll, again once the guide's panel has gone and the page settled (unless the visitor scrolls meanwhile) */
  var put=function(){if(!G.te.userScroll&&!G.on)scr.scrollTop=b.top};put();requestAnimationFrame(put);setTimeout(put,250);setTimeout(put,600)}
/* what closes it: the panel lowered to Mini or hidden; Stop; Play; Next; Previous (each would play the tour) */
on("renderGuide.after",function(){if(G.te.askMode&&G.on&&(S.dockH==="mini"||S.dockH==="compact"))setTimeout(function(){if(G.te.askMode)teAskLeave()},0)});
["stopTap.answer","resumeTour.answer","prevTap.answer","next.answer"].forEach(function(n){on(n,function(){if(!G.te.askMode)return;teAskLeave();return true})});
on("runStep.before",function(){if(G.te.askMode)teAskDrop()});on("stopTour.after",function(){if(G.te.askMode)teAskDrop()});
function teAsk(q){q=String(q||"").trim();if(!q)return;teAskEnter();ask(q)}
function teMic(){teAskEnter();if(typeof listen==="function")listen(false)}
/* Play the quiz: it closes the panel (a game), the card's quiz whole above the ✦, its first answer focused */
function tePlayQuiz(){var b=teQuizLeft(G.te.poi);teClose(false);if(!b)return;var el=scr.querySelector('[data-ref="b:'+b.id+'"]');if(!el)return;
  scr.scrollTo({top:teShowTop(el,false),behavior:S.reduce?"auto":"smooth"});el.classList.remove("pfocus");void el.offsetWidth;el.classList.add("pfocus");setTimeout(function(){el.classList.remove("pfocus")},2400);
  var a=el.querySelector(".kq button");if(a)a.focus({preventScroll:true})}
/* ----- Explore nearby: the panel minimal, the card down, the wall flies to the point (Reduced motion: at once); a point with a card opens it
   at Half with its invitation (the guidance's words for A tour of {point}); one without, Ask about the view about it. Unity: the card lowers,
   the AR view rings the point's marker (an edge arrow while it is off screen) while the card rises with its invitation: the Lab's pan stands
   in for the visitor turning the phone ----- */
function teFlyTarget(p){var wl=teWall(),z=Math.max(S.camZoom,1.35),W=ph.offsetWidth,H=ph.offsetHeight,k=Math.max(W/wl.w,H/wl.h)*z,dw=wl.w*k,dh=wl.h*k,cl=function(v){return Math.max(0,Math.min(1,v))};
  return{x:dw>W?cl((W/2-p.u*dw)/(W-dw)):S.camX,y:dh>H?cl((H*TE_FLY_Y-p.v*dh)/(H-dh)):S.camY,z:z}}
function teFly(p,done){var a={x:S.camX,y:S.camY,z:S.camZoom},b=teFlyTarget(p),tok=G.te.flyTok=(G.te.flyTok||0)+1;emit("phone.moved");
  var put=function(e){S.camX=a.x+(b.x-a.x)*e;S.camY=a.y+(b.y-a.y)*e;S.camZoom=a.z+(b.z-a.z)*e;camPaint();camLayer()};
  if(S.reduce){put(1);done();return}var t0=performance.now();(function f(){if(G.te.flyTok!==tok)return;var u=Math.min(1,(performance.now()-t0)/TE_FLY_MS),e=u<.5?4*u*u*u:1-Math.pow(-2*u+2,3)/2;put(e);if(u<1)requestAnimationFrame(f);else{emit("phone.moved");done()}})()}
function teNearGo(id){var p=byId(teWall().pois||[],id);if(!p)return;teSet("min");G.te.inv=null;S.view="card";S.stop="closed";G.cam.open=false;G.fab.menu=false;if(S.camCfg.wall!=="ribeira"){S.camCfg.wall="ribeira";G.cam.lit=[];G.cam.route=[]}renderPhone(false);G.te.flying=id;
  teFly(p,function(){G.te.flying=null;if(POINTS[id]){pointOpen(id);S.view="card";S.stop="half";renderPhone(false);emit("card.opened","What next? › Explore nearby");teInvite(id)}
    else{G.cam.open=true;renderPhone(true);emit("view.opened","What next? › Explore nearby");camAsk("","what");camLight([id])}})}
/* its invitation: the words of Guide › Azu speaks up › Tours › A tour of {point} ("A short tour of the Torreão?"; a place kept: Continue the
   tour of …?), Azu beside it above the card, spoken; a tap starts (or continues) that tour */
function teInvite(id){var it=typeof gdItems==="function"?gdItems().filter(function(x){return x.id==="tourof"})[0]:null;it=it||S.guidance&&S.guidance.items&&S.guidance.items.tourof;
  var sv=typeof svOffer==="function"?svOffer(id):null,sl={id:id,point:teName(id),min:tourMinutes(id)};if(sv){sl.cont=sv.n;sl.of=sv.of}
  var t=it?gdLine(it,false,sl):nameIn(TXT("gd_tourof_short"),"{point}",teName(id));G.te.inv={id:id,text:t,cont:!!sv};teInvDraw();say(t,function(){},function(){})}
function teInvShown(){var v=G.te.inv;return!!v&&!G.on&&S.view==="card"&&S.stop==="half"&&svPoi()===v.id}
function teInvDraw(){var o=ph.querySelector(".teinv");if(!teInvShown()){if(o)o.remove();if(G.te.inv&&(G.on||svPoi()!==G.te.inv.id||S.stop==="closed"))G.te.inv=null;return}
  if(!o||o._t!==G.te.inv.text){if(o)o.remove();o=document.createElement("div");o.className="teinv"+(S.guideCfg.mascot===false?" plain":"");o.innerHTML=(S.guideCfg.mascot!==false?'<span class="te-iaz" aria-hidden="true">'+mascotStill()+'</span>':'<i class="te-sign" aria-hidden="true">✦</i>')+
    '<button type="button" data-teinv="1">'+esc(G.te.inv.text)+'<span aria-hidden="true"> &#8250;</span></button>';o._t=G.te.inv.text;ph.appendChild(o)}teInvPlace()}
function teInvPlace(){var o=ph.querySelector(".teinv");if(!o)return;if(!teInvShown()){teInvDraw();return}var s=tePR(scr);o.style.bottom=Math.round(ph.offsetHeight-s.t+10)+"px"}
on("renderPhone.after",function(){teInvDraw()});
function teInvGo(){var v=G.te.inv;if(!v)return;G.te.inv=null;teInvDraw();S.view="card";if(v.cont&&typeof svContinue==="function")svContinue(v.id);else startTour()}

/* ----- the Editor: Guide › 2 · The tour › The tour's end (after Ways in) ----- */
function teRingSel(key){key=String(key||"");if(/teWhere|teBlock/.test(key)){var sc=G.te.sc||teShowcase();return sc.ref==="hero"?".hero":'[data-ref="'+sc.ref+'"]'}
  if(/teSec\.qs|teQs|tq/.test(key))return".te-qs|.tepanel|.tepill";if(/teSec\.ask/.test(key))return".te-ask|.tepanel|.tepill";if(/teSec\.near|teNear|tePicks/.test(key))return".te-near|.tepanel|.tepill";
  if(/teSec\.act/.test(key))return".te-acts|.tepanel|.tepill";if(key==="asks")return".tebub|.teazu|.tepanel";if(/teStart/.test(key))return".tepill|.tepanel";return".tepanel|.tepill|.gdock|.gask"}
/* each row's demo: the tour's last step played (its line said), then the end as configured, the part the row changes ringed */
function teEndStep(){return(S.guideSteps||[]).filter(function(s){return s.act==="end"&&!s.skip}).pop()||null}
function teDemo(key,o){o=o||{};if(G.on)stopTour();var st=teEndStep();S.view="card";S.stop="full";S.settingsOpen=false;S.introSeen=true;G.te.demoKey=key;G.te.lineForce=o.line||null;
  if(!st){renderPhone(false);G.te.demoKey=null;if(typeof gdChip==="function"){gdChip("This tour has no end step (Specific Marker › 3 · Guide script: Does = The end)");clearTimeout(G.teChipT);G.teChipT=setTimeout(function(){gdChip(null)},3200)}return}
  renderPhone(false);G.demoTour=true;try{startTour({id:st.id})}finally{G.demoTour=false}}
rowDemos(function(key){if(!/^guideCfg\.te[A-Z]/.test(key))return;return{act:function(){teDemo(key)},ringAfter:400,ring:function(){return G.te.open?teRingSel(key):".gdock|.gcap|.hero"}}});
/* the Editor's words for a row: what the open point gets now (names, never ids) */
function teNowShow(poi){var sc=teShowcase(poi),b=sc.id?byId(teBlocks(poi),sc.id):null;return(b?teBlockName(b):"the hero")+" ("+sc.why+")"}
function teOwnHtml(poi){var qs=(teCfg().teQs||{})[poi]||[],nl=langCodes().length;
  return'<div class="atin teown"><table class="tbl txt atint"><thead><tr><th style="width:22px">#</th>'+langHead()+'<th style="width:96px"></th></tr></thead><tbody>'+(qs.length?qs.map(function(o,i){
    return'<tr><td class="gdvn">'+(i+1)+'</td>'+langCodes().map(function(l){return'<td><input class="u" data-teqin="'+esc(poi)+'|'+i+'|'+l+'" value="'+esc(o[l]||"")+'" aria-label="'+esc("This point's question "+(i+1)+" · "+l.toUpperCase())+'"></td>'}).join("")+
      '<td class="gdva"><button class="atplay" type="button" data-teqplay="'+i+'" title="Play the tour\'s end with this question" aria-label="Show question '+(i+1)+' on the phone">&#9656;</button><button class="del" type="button" data-teqdel="'+i+'">Remove</button></td></tr>'}).join(""):'<tr><td></td><td colspan="'+(nl+1)+'" class="uses">None written: the templates make them from the card\'s words.</td></tr>')+'</tbody></table>'+
    '<div class="btnrow">'+(qs.length<3?'<button class="ubtn sm" type="button" data-teqadd="1">+ Add a question</button>':'<span class="uses">3 questions: the most</span>')+'</div></div>'}
function teTplHtml(){var ks=["te_tq_heading","te_tq_year","te_tq_who","te_tq_more"];
  return'<div class="atin tetpl"><table class="tbl txt atint"><thead><tr><th style="width:22px">#</th>'+langHead()+'<th style="width:40px"></th></tr></thead><tbody>'+ks.map(function(k,i){
    return'<tr><td class="gdvn" title="'+esc(TXP[k]||k)+'">'+(i+1)+'</td>'+txInputs(k)+'<td class="gdva"><button class="atplay" type="button" data-tetpl="'+k+'" title="Play the tour\'s end with the questions" aria-label="Show '+esc(TXP[k]||k)+' on the phone">&#9656;</button></td></tr>'}).join("")+'</tbody></table></div>'}
function teEdHtml(){var c=teCfg(),poi=svPoi(),nm=nameMid(teName(poi,"en"),"en"),card=teBlocks(poi),ord=c.teOrder||[],qsNow=teQuestions(poi,S.lang),nearNow=teNear(poi,S.lang);
  var secRow=function(id,j){var h={qs:"2-3 chips, one line each (a row that scrolls sideways if needed): this point's own questions (below), else made from the card's own words by the templates (a heading that asks, a date, a king or queen, a heading or a name), with no AI; never one the tour answered word for word. A tap asks Ask the mascot at once, the answer in the guide panel.",
      ask:"One row: a field (“"+nameIn(TXT("te_ask_ph"),"{point}",teName(poi),S.lang)+"”) and a 44 px microphone, Ask the mascot's own path: its full view opens with the question asked (or listening).",
      near:"3-5 cards: the point's picture (its hero's crop of the wall), its name and why (“Part of this palace”, “On the same square”, “Nearby · short tour”), most related first. A tap makes the panel minimal, flies the wall to the point and opens its card with its invitation (“A short tour of the Torreão?”); a point with no card opens Ask about the view about it.",
      act:"One quiet row: Start again · Play the quiz (only while the card's quiz is not played) · Done. Done closes the panel; the ✦ menu then offers What next? until the visitor leaves the point."}[id];
    return row(TE_SECS[id],pseg("guideCfg.teSec."+id,["true","false"],["On","Off"])+'<span class="teord"><button class="del" type="button" data-teord="'+id+'|-1"'+(j===0?" disabled":"")+' aria-label="Move '+esc(TE_SECS[id])+' up">&#9650;</button><button class="del" type="button" data-teord="'+id+'|1"'+(j===ord.length-1?" disabled":"")+' aria-label="Move '+esc(TE_SECS[id])+' down">&#9660;</button></span>',h+" ▲▼: its place in the panel.")};
  var subs={qs:row("How many",pseg("guideCfg.teQsN",["2","3"],["2","3"]),"The most chips (2 or 3); fewer when the card gives fewer.",true)+
      row("Questions from",pseg("guideCfg.teQsSrc",["own","tpl"],["This point's own, then the templates","The templates only"]),"This point's own questions first (written below, EN and PT), the templates after them; or the templates only. Now for "+nm+": "+(qsNow.length?qsNow.map(function(x){return"“"+x.q+"” ("+(x.src.indexOf("own")===0?"its own":"template")+")"}).join(", "):"none")+".",true)+
      '<div class="note tesub"><b>'+esc(nm.charAt(0).toUpperCase()+nm.slice(1))+'\'s own questions</b></div>'+teOwnHtml(poi)+'<div class="note tesub"><b>The templates\' words</b> (the same words as Languages & Terms; {0} the card\'s heading, date or name, {point} the point\'s name)</div>'+teTplHtml(),
    near:row("How many",pseg("guideCfg.teNearN",["3","4","5"],["3","4","5"]),"The most cards (3-5): people rarely look past the first 3 or 4.",true)+
      row("Order",psel("guideCfg.teNearRule",[["pcd","This point's picks, then the same category, then the nearest"],["pd","This point's picks, then the nearest"],["d","The nearest only"]]),"Most related first, and always the points with a card and a tour first; the places without one (the city on the hill, the arcades: a tap opens Ask about the view about them) after them, in the same order. The same category: the wall's points share one (the palace: the Ribeira Palace and the Torreão; the square: the Terreiro do Paço and the arcades). The nearest: by the wall's own coordinates. Now for "+nm+": "+nearNow.map(function(x){return x.name+" ("+x.why+")"}).join(", ")+".",true)+
      row("This point's picks",psel("guideCfg.tePicks."+poi+".p1",[["","No pick"]].concat(teNearOpts(poi)))+" "+psel("guideCfg.tePicks."+poi+".p2",[["","No second pick"]].concat(teNearOpts(poi))),"The points the Editor wants first after "+nm+"'s tour, before any rule (“Recommended”).",true)};
  return'<div class="note teone"><b>Where the tour leaves the visitor.</b> Every tour, written or from the card, ends the same way: after its end line the card scrolls to its showcase, the mascot asks “'+esc((teAfterLine(poi,S.lang,0)||{text:""}).text)+'” (The mascot\'s intro lines › After the tour), and a small What next? panel rises above the bottom bar: at most a third of the screen, the card behind still scrolls and plays. Its chevron or a swipe down makes it a small pill; Done closes it and the ✦ menu offers What next? until the visitor leaves the point.</div>'+
    row("What next? panel",pseg("guideCfg.teOn",["true","false"],["On","Off"]),"On: the end as above. Off: the tour ends on the hero and Ask the mascot opens (the end before round 17c).")+
    row("Where the tour ends",psel("guideCfg.teWhere",[["show","The showcase"],["hero","The hero"],["pick","A block I pick"]]),"The showcase: the card's first block in this order: "+TE_SHOW.map(function(x){return x[2]}).join(", ")+"; none: the hero. It sits whole in the part of the screen above the panel. Now for "+nm+": "+teNowShow(poi)+".")+
    (c.teWhere==="pick"?row("The block",psel("guideCfg.teBlock."+poi,card.map(function(b){return[b.id,teBlockName(b).replace(/^the /,"")]})),"The block "+nm+"'s tour ends on"+((c.teBlock||{})[poi]?".":" (none picked yet: its first block until one is)."),true):"")+
    row("Start as",pseg("guideCfg.teStart",["open","min"],["Open","Minimal"]),"Open: the panel rises at once. Minimal: only its pill (“What next? ▴” with the mascot's face) above the bottom bar, a tap opens it.")+
    '<div class="sub2 gth">Its parts <span class="sub">· each On or Off, ▲▼ their order in the panel</span></div>'+ord.map(function(id,j){return secRow(id,j)+(subs[id]||"")}).join("")+
    '<div class="row"><label>The mascot asks</label><div><span class="uses">“'+esc((teAfterLine(poi,S.lang,0)||{text:"(no line)"}).text)+'” and '+Math.max(0,atIntroLines("after",S.lang).length-1)+' more, in turn</span> <span class="atin"><button class="atplay" type="button" data-tedemo="asks" title="Play the tour\'s end" aria-label="Show the mascot asking on the phone">&#9656;</button></span> <button class="ubtn sm" type="button" data-goto="s7auto">Edit: Tour from the card › The mascot\'s intro lines › After the tour &#8250;</button></div>'+help("The mascot's question beside the panel, spoken, then the bubble stays a few seconds and fades. Its lines are a set of the mascot's intro lines (After the tour), picked by the point like the others, the next one at each end there.")+'</div>'+
    '<div class="note">Unity: GuideTourEnd (plain C#, EditMode-tested: the showcase, the questions from the card\'s words and the CardStrings templates, the nearby order from the marker\'s picks, its category and the wall\'s coordinates); the panel a UI Toolkit VisualElement with two snap heights (a PanelSnap manipulator on its header), its chips and cards in horizontal ScrollViews, the rise and the cards\' stagger as USS transitions (DOTween for the ScrollView\'s scroll to the showcase); Reduced Motion: opacity only.</div>'}
function teNearOpts(poi){return(teWall().pois||[]).filter(function(p){return p.id!==poi}).map(function(p){return[p.id,L(p.name)+(POINTS[p.id]?"":" (no card)")]})}
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var at=d.querySelector('[data-fo="s7more"]')||d.querySelector('[data-fo="s7auto"]');if(!at)return d.innerHTML;
  at.insertAdjacentHTML("afterend",comp("s7end","where the tour leaves the visitor, and What next?",teEdHtml()));var f=innerOf(d,"s7end");if(f)f.insertAdjacentHTML("beforeend",ownbar("s7end")+ownPanels("s7end"));return d.innerHTML});
ed.addEventListener("click",function(e){var b=e.target.closest&&e.target.closest("[data-teord],[data-teqadd],[data-teqdel],[data-teqplay],[data-tetpl],[data-tedemo]");if(!b)return;e.stopPropagation();var c=teCfg(),poi=svPoi();
  if(b.dataset.teord){var q=b.dataset.teord.split("|"),o=(c.teOrder||[]).slice(),i=o.indexOf(q[0]),j=i+(+q[1]);if(i<0||j<0||j>=o.length)return;o.splice(i,1);o.splice(j,0,q[0]);c.teOrder=o;renderAll(true);teDemo("guideCfg.teSec."+q[0]);return}
  if(b.dataset.teqadd){var l=(c.teQs[poi]=(c.teQs[poi]||[]).slice());if(l.length<3)l.push({en:"",pt:""});renderEditor();return}
  if(b.dataset.teqdel!=null){var l2=(c.teQs[poi]||[]).slice();l2.splice(+b.dataset.teqdel,1);c.teQs[poi]=l2;renderAll(true);return}
  if(b.dataset.teqplay!=null||b.dataset.tetpl){teDemo("guideCfg.teQsSrc");return}if(b.dataset.tedemo){teDemo("asks");return}},true);
ed.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.teqin)return;e.stopImmediatePropagation();var q=t.dataset.teqin.split("|"),c=teCfg(),l=(c.teQs[q[0]]=(c.teQs[q[0]]||[]).slice()),o=l[+q[1]]=Object.assign({},l[+q[1]]||{});o[q[2]]=t.value},true);
/* the phone's signature (the self-test): the end as configured for the open point, open and minimal */
on("modelSig.sig",function(h){if(!sigWant(/guideCfg\.te[A-Z]/))return h;var c=teCfg(),poi=svPoi();if(!c.teOn)return h+"#te:off";
  return h+"#te:"+c.teStart+"|"+teShowcase(poi).ref+"|"+tePanelHtml(poi)+"|"+tePillHtml()});
