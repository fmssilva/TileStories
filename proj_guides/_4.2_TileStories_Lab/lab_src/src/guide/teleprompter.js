/* The teleprompter and where the mascot's words go (round 21; the developer, 2026-11-13: "when not possible, for example during the guide
   where the mascot moves around to explain the blocks and a moving balloon would hide those blocks, have a moving teleprompter panel at
   the bottom of the screen ... a card coming from the base of the screen behind the minimal panel, going some 2 or 3 cm up, showing the
   text being spoken highlighted, and transparent, disappearing as we move forward").
   During a tour (Words during a tour: Teleprompter, the default; The bubble; Off), at Mini and Compact, the words being said
   show in a strip rising from the screen's bottom edge behind the Mini (Teleprompter height, 184 px: about 3 cm on a 6.1-inch phone,
   whose 390 px are about 64 mm; it leaves about 1.7 cm above the Mini, two lines of 17 px words and the strip's own small row). Above the
   card, below the Mini (z 13: the card's 0-12, the Mini 14). The line said now in the one highlighter (guide/highlight.js); a new line
   slides up, the line before it fading up and out; the next line faint below; the glass melts into the card above (a 22 px fade, no hard
   top edge). Its small row: the mascot's name with a live dot, and Full script › (the open panel's whole text, Read all). In the Looks
   whose Mini has its own line of words (Paper glass, Teleprompter) the Mini's line is the teleprompter; a game's card on the tour or Ask
   hides it. The told block is kept above it (freeHeight counts it), and the mascot too. Reduced Motion: the lines change with a fade.
   The bubble (The bubble): the mascot's own words in a bubble anchored to its head, its tail on the head, following it every frame as
   it moves, flipping side at the screen's edge. Outside a tour (A tip's bubble), a tip's bubble sits beside the mascot's
   head (the default) or above it, and follows its hop (guidance/present.js).
   Unity: a UI Toolkit panel (position absolute, bottom 0, its height the option) with a gradient mask image at its top; the lines in a
   vertical layout scrolled by a DOTween (Ease.OutCubic, 420 ms), the old line's opacity tweened out; its surface the panel's look
   (guide/panel-look.js); the bubble's anchor the mascot's head Transform projected to the panel each frame (RuntimePanelUtils
   .CameraTransformWorldToPanel), its side flipped when it would leave the safe area. */
S.guideCfg.tourWords="tele";S.guideCfg.teleH=184;S.guideCfg.bubbleOut="beside";
/* the strip is wanted now: a tour at Mini or Compact, its words on (Aa), no Ask, no game's card; the Looks with a Mini line keep theirs */
function teleWant(){if(!G.on||!dockOn()||S.guideCfg.tourWords!=="tele"||!S.guideCfg.captions||G.askOpen||G.typing||G.tpText==null||!String(G.tpText).trim())return false;
  var h=S.dockH;if(h!=="mini"&&h!=="compact")return false;if(h==="mini"&&(dkLook()==="paper"||dkLook()==="prompter")&&dkWordsOn())return false;
  return!ph.querySelector(".gdbeat:not(.bt-away)")}
/* the line said now (its index), from the reading head when this text is being said */
function teleLineNow(t){var P=tpLines(t),k=hlSaying(t)&&G.hl&&G.hl.k>=0?(G.hl.ended?P.ws.length-1:G.hl.k):0;return{P:P,i:tpLineOf(P.ls,k)}}
function teleLine(P,x,c){var l=P.ls[x];return'<p class="'+c+(l?"":" tl-empty")+'">'+(l?tpWords(P,l.a,l.b,-1):"&nbsp;")+'</p>'}
function teleLinesHtml(P,i,prev){return'<div class="tl-lines'+(prev?" tl-up":"")+'" aria-hidden="true">'+teleLine(P,i-1,"tl-prev")+teleLine(P,i,"tl-now")+teleLine(P,i+1,"tl-next")+'</div>'}
function teleHtml(t){var L=teleLineNow(t);return'<div class="tl-glass" aria-hidden="true"><i class="tl-glow"></i></div><div class="tl-head"><span class="tl-who"><i class="tl-dot" aria-hidden="true"></i>'+esc(mascotName())+'</span>'+
  '<button type="button" class="tl-full" data-tele="full">'+TXH("tele_full")+'<span aria-hidden="true">&#8250;</span></button></div>'+teleLinesHtml(L.P,L.i,false)}
function teleCls(){return"gtele"+pipe("dock.cls","")+(S.dockH==="compact"?" tl-compact":" tl-mini")}
/* the strip on the phone: made once a tour's words want it, its lines moved on as the voice goes; it fades down when not wanted */
function teleDraw(){var el=ph.querySelector(".gtele:not(.tl-out)");if(!teleWant()){if(el)teleOut(el);return}
  if(!el){el=document.createElement("div");el._t=null;el.setAttribute("role","region");el.setAttribute("aria-label",TXT("tele_full"));ph.appendChild(el);el.className=teleCls()+(S.reduce?"":" tl-in")}
  else el.className=teleCls();el.style.setProperty("--tlh",Math.round(+S.guideCfg.teleH||184)+"px");
  if(el._t!==G.tpText){el._t=G.tpText;el._i=teleLineNow(G.tpText).i;el.innerHTML=teleHtml(G.tpText)}
  el.classList.toggle("tl-live",talkState()==="speaking");telePlace(el)}
function teleOut(el){el.classList.add("tl-out");if(S.reduce){el.remove();return}setTimeout(function(){if(el.parentNode)el.remove()},260)}
/* the lines' foot: just above the Mini (its row; the hide tab's side kept clear), or near the screen's foot at Compact */
function telePlace(el){var H=ph.offsetHeight,W=ph.offsetWidth,d=ph.querySelector(".gdock.h-mini"),r=d&&(d.querySelector(".gd-mrow")||d),foot=20,pr=16;
  if(r&&r.getClientRects().length){foot=Math.round(H-phRect(r).t+12);var hd=d.querySelector(".gd-hide");if(hd&&hd.getClientRects().length){var hr=phRect(hd);if(hr.t+hr.h>H-foot-60)pr=Math.max(16,Math.round(W-hr.l+4))}}
  else{var gb=ph.querySelector(".gbtn-wrap.intour");if(gb&&gb.getClientRects().length){var gr=phRect(gb);pr=S.guideCfg.gbSide==="right"?Math.max(16,Math.round(W-gr.l+6)):16;el.style.setProperty("--tlpl",(S.guideCfg.gbSide==="right"?16:Math.round(gr.l+gr.w+6))+"px")}}
  el.style.setProperty("--tlb",foot+"px");el.style.setProperty("--tlpr",pr+"px")}
/* the voice goes on: a new line slides up (the one before fading up and out); within a line the highlighter lights the word */
function teleWord(){var el=ph.querySelector(".gtele:not(.tl-out)");if(!el||!hlSaying(el._t))return;var L=teleLineNow(el._t);if(L.i===el._i)return;var up=L.i>el._i;el._i=L.i;
  var x=document.createElement("div");x.innerHTML=teleLinesHtml(L.P,L.i,up&&!S.reduce);var n=x.firstChild;if(S.reduce)n.classList.add("tl-fade");var o=el.querySelector(".tl-lines");if(o)o.replaceWith(n);else el.appendChild(n)}
on("markWord.after",function(){teleWord()});
on("renderGuide.after",function(){teleDraw()});
on("stopTour.after",function(){var el=ph.querySelector(".gtele");if(el)teleOut(el)});
/* the one highlighter lights the line said now */
on("hlBoxes.add",function(out){var el=ph.querySelector(".gtele:not(.tl-out)");if(el&&hlSaying(el._t)){var n=el.querySelector(".tl-now");if(n)out.push([n,"prompter"])}});
/* Full script: the open panel's whole text (Read all) */
ph.addEventListener("click",function(e){var t=e.target.closest("[data-tele]");if(!t)return;e.stopPropagation();S.dockH="full";G.askOpen=false;G.typing=false;renderGuide()},true);
/* keep its place and its live dot in step (the Mini's width changes with its controls; the voice starts and stops) */
setInterval(function(){var el=ph.querySelector(".gtele:not(.tl-out)");if(!el)return;if(!G.on){teleOut(el);return}el.classList.toggle("tl-live",talkState()==="speaking");telePlace(el)},250);

/* ----- the bubble in a tour (The bubble): anchored to the mascot's head, following it every frame; kept inside the phone and above the
   panel only (a game's card or a Tell me more chip may sit under it: its tail stays on the head) ----- */
/* the head: the drawn figure's top 40 % (guidance/present.js's gdFigure, gdHeadOf), else the tile's */
function gbHead(m){var f=typeof gdFigure==="function"?gdFigure(m):null;if(f&&f.r>f.l)return gdHeadOf(f);var r=phRect(m);return{l:r.l+r.w*.2,r:r.l+r.w*.8,t:r.t,b:r.t+r.h*.4}}
var GB_TIP=10;
function gbFollow(){G.gbRaf=0;if(gbPlace())G.gbRaf=requestAnimationFrame(gbFollow)}
function gbPlace(){var b=ph.querySelector(".gbubble.gb-head"),m=ph.querySelector(".mascot");if(!b||!m||!m.getClientRects().length)return false;
  var W=ph.offsetWidth,H=ph.offsetHeight,hd=gbHead(m),hy=(hd.t+hd.b)/2,room=hd.l-GB_TIP-12,flip=room<170,bw=Math.min(300,flip?W-12-(hd.r+GB_TIP):room);
  b.style.bottom="";b.style.right="";b.style.width=Math.round(bw)+"px";var bh=b.offsetHeight,dk=ph.querySelector(".gdock"),lo=(dk&&dk.getClientRects().length?phRect(dk.querySelector(".gd-mrow")||dk).t-8:H-12)-bh,
    top=Math.max(56,Math.min(lo,hy-bh/2)),left=flip?hd.r+GB_TIP:hd.l-GB_TIP-bw;
  b.style.left=left.toFixed(1)+"px";b.style.top=top.toFixed(1)+"px";b.classList.toggle("gb-flip",flip);b.style.setProperty("--tly",Math.round(Math.max(12,Math.min(bh-12,hy-top)))+"px");return true}
function gbFollowStart(){if(!G.gbRaf)G.gbRaf=requestAnimationFrame(gbFollow)}
on("renderGuide.after",function(){var b=ph.querySelector(".gbubble");if(!b||!G.on||S.guideCfg.tourWords!=="bubble")return;b.classList.add("gb-head");gbPlace();gbFollowStart()});
/* the tail's tip (the checks): the bubble's edge plus the tail's reach, at its height */
function gbTailTip(){var b=ph.querySelector(".gbubble.gb-head");if(!b)return null;var r=phRect(b),y=r.t+(parseFloat(b.style.getPropertyValue("--tly"))||r.h/2),f=b.classList.contains("gb-flip");return{x:f?r.l-GB_TIP:r.l+r.w+GB_TIP,y:y}}

/* ----- the Editor: Guide › 1 · The mascot › Speech on screen ----- */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7cap",row("Words during a tour",pseg("guideCfg.tourWords",["tele","bubble","off"],["Teleprompter","The bubble","Off"]),"While the mascot walks around the card to point at the blocks, at Mini and Compact. Teleprompter: a strip rises from the screen's bottom edge behind the Mini, the line being said lit by the highlighter, the line before sliding up and fading, the next one faint below; it never covers the block being told (the block is kept above it) and its Full script › opens the whole text. The bubble: the mascot's own words in a bubble by its head, following it as it moves (it may cover the words around the mascot). Off: no words on screen at Mini (the block's own words still light up as they are read; Aa and Read all show them). In Paper glass and the Teleprompter Look the Mini's own line of words stays the teleprompter. Unity: a UI Toolkit panel with a gradient mask, its lines scrolled by a DOTween.")+
    row("Its height",pslider("guideCfg.teleH",130,240,2),"Its height from the screen's bottom edge, in px of a 390 px wide phone (184: about 3 cm on a 6.1-inch phone, about 1.7 cm of it above the Mini, room for two lines of words and its small row).",true)+
    row("A tip's bubble",pseg("guideCfg.bubbleOut",["beside","above"],["Beside the mascot's head","Above the mascot"]),"A tip's bubble on the wall or the card: beside the mascot's head, its tail on the head (above only when neither side has room), or above the mascot first. Either way it follows the mascot as it hops to its place."));
  return d.innerHTML});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(bubbleOut|"));
/* the phone's signature (the self-test): the strip as the visitor sees it, for the tour's first line, or none */
on("modelSig.sig",function(h){if(!sigWant(/guideCfg\.(tourWords|teleH|pSkin)/))return h;var t=TXT("tour_promise");return h+"#tele:"+S.guideCfg.tourWords+"|"+(S.guideCfg.tourWords==="tele"?'<div class="'+teleCls()+'" style="--tlh:'+Math.round(+S.guideCfg.teleH||184)+'px">'+teleHtml(t)+'</div>':"")});
/* each row's demo: the tour at Mini, a step whose words the mascot says while it moves; the tip on the wall */
rowDemos({"guideCfg.tourWords":{tour:"g2",dock:"mini",ring:function(){return S.guideCfg.tourWords==="tele"?".gtele":S.guideCfg.tourWords==="bubble"?".gbubble|.mascot":".gdock"}},
  "guideCfg.teleH":{tour:"g2",dock:"mini",ring:".gtele|.gdock"},
  "guideCfg.bubbleOut":{wall:true,wait:300,act:function(){gdDemoItem("tapmarker")},ringAfter:900,ring:".gdbub"}});
