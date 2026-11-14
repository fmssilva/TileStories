/* The one highlighter (round 21; the developer, 2026-11-13: "make some interpolation so the highlighting is smooth, leaving the
   highlighted colours vanishing behind like a comet ... in all speech text highlighting: in the balloon, in this panel, and in the blocks
   directly when the guide reads them"). Every place that shows the words being spoken lights them here, with one function (hlGlide):
   the bubbles (the dock's, the tips'), the teleprompter (guide/teleprompter.js), the open panel (its lines, its whole text, the Mini's
   line), the block's own text (the read-along), the games' cards and the classic captions. A module names its places at the point
   hlBoxes.add ([box, place, offset]); the reading head (guide/readalong.js's hlTick) paints them all each frame (hlPaintAll).
   The light: a soft shape (a pill with a fine underline) moves and resizes from word to word on an ease (in-out cubic; its time from the
   word's own length: a recorded line's word times, the device voice's boundaries, so it never lags the voice), and the words just said
   keep a warm tint fading over Trail words behind it (the comet). Highlight style keeps its other ways: Sentence + word (a soft band on
   the sentence, the shape on the word), Fill (the said words stay tinted), One word (the shape alone, no tail). Glide = Word by word,
   or Reduce Motion: no glide, the shape is on the word at once; the tail stays. A new line: the shape grows from the word's start on its
   line (it never crosses the text diagonally). Each place picks only its size and its colours, from its theme: --hlc (the light's
   colour), --hla (the shape's strength), --hlln (the underline), --hlt (the tail's strength), --hlun (the words not yet said).
   Unity: a TextMeshPro / UI Toolkit label with a highlight quad (a VisualElement behind the text) tweened between the words' bounds
   (DOTween, Ease.InOutCubic; the bounds from TMP_TextInfo.characterInfo, or the TextElement's glyph rects), the tail as per-character
   vertex colours fading (TMP colors32), the word's state a rich-text-free colour per character. */
S.guideCfg.hl="comet";S.guideCfg.trail=6;S.guideCfg.glide=true;
var HL={hooks:{}};
/* the places showing the line being said now: [box, place, offset] (offset: the head's word k is the box's word k + offset) */
function hlBoxes(h){var out=[];emit("hlBoxes.add",out,h);return out}
function hlPaintAll(h){if(!h)return;hlBoxes(h).forEach(function(b){hlGlide(b[0],b[1],h.p+(b[2]||0),h.ended)})}
/* is this text the line being said? */
function hlSaying(t){return!!G.sayLine&&t!=null&&G.sayLine.text===t}
function hlEase(x){return x<.5?4*x*x*x:1-Math.pow(-2*x+2,3)/2}
/* the glide's time: about half the word's spoken length (it arrives as the word is heard), 90-260 ms */
function hlMs(k){var ms=typeof wordMs==="function"&&G.words&&G.words[k]?wordMs(k):180;return Math.max(90,Math.min(260,ms*.55))}
/* a word's box in its place's own px (the place's padding box; the phone's scale taken out) */
function hlRectOf(box,w){var b=box.getBoundingClientRect(),q=b.width/(box.offsetWidth||1)||1,rs=w.getClientRects(),r=rs.length?rs[0]:w.getBoundingClientRect();
  return{x:(r.left-b.left)/q-box.clientLeft+box.scrollLeft,y:(r.top-b.top)/q-box.clientTop+box.scrollTop,w:r.width/q,h:r.height/q}}
function hlWords(box){return box.querySelectorAll(".gw,.tw,.gdw,.btw")}
function hlIdx(w,j){return w.dataset.k!=null?+w.dataset.k:j}
/* the one painter: p the reading head (k + the share of word k said), ended when the line is over */
function hlGlide(box,place,p,ended){if(!box||!box.isConnected)return;HL.hooks[place]=(HL.hooks[place]||0)+1;var ws=hlWords(box),n=ws.length;if(!n)return;
  box.classList.add("hl-box","gread");box.dataset.hlp=place;
  var mode=S.guideCfg.hl,tr=Math.max(1,+S.guideCfg.trail||6),glide=S.guideCfg.glide!==false&&!S.reduce,k=ended?1e9:Math.floor(p),sid=[],s=0,hw=null,hs=-1,st=box._hl||(box._hl={k:null,cur:null}),now=performance.now();
  /* the line said: its tail fades away over 0.7 s (Fill keeps the said words tinted) */
  if(ended&&st.endAt==null)st.endAt=now;if(!ended)st.endAt=null;var fade=ended&&mode!=="fill"?Math.max(0,1-(now-st.endAt)/700):1;
  for(var i=0;i<n;i++){sid.push(s);if(/[.!?…]["”»)\]]*$/.test(ws[i].textContent))s++;if(hlIdx(ws[i],i)===k){hw=ws[i];hs=sid[i]}}
  for(var j=0;j<n;j++){var w=ws[j],x=hlIdx(w,j),t=0;
    if(x<k)t=(mode==="comet"?Math.max(0,1-(p-x-1)/tr):mode==="fill"?1:0)*fade;
    if(mode==="sentence")t=hs>=0&&sid[j]===hs&&x!==k?.6:0;
    w.classList.toggle("hl-un",x>k);w.classList.toggle("hl-on",x===k);w.classList.toggle("hl-said",x<k);w.style.setProperty("--t",t.toFixed(3))}
  var q=box.querySelector(":scope > .hl-quad");if(!q){q=document.createElement("i");q.className="hl-quad";q.setAttribute("aria-hidden","true");box.insertBefore(q,box.firstChild)}
  if(!hw||!hw.getClientRects().length){q.style.opacity="0";st.k=null;return}
  var to=hlRectOf(box,hw);
  if(st.k!==k){var from=glide&&st.k!=null&&st.cur?st.cur:null;if(from&&Math.abs(from.y-to.y)>to.h*.5)from={x:to.x,y:to.y,w:Math.min(6,to.w),h:to.h};
    st.from=from||to;st.t0=now;st.dur=from?hlMs(k):0;st.k=k}
  var fr=st.dur?Math.min(1,(now-st.t0)/st.dur):1,e=st.dur?hlEase(fr):1,f=st.from,c={x:f.x+(to.x-f.x)*e,y:f.y+(to.y-f.y)*e,w:f.w+(to.w-f.w)*e,h:f.h+(to.h-f.h)*e};st.cur=c;st.e=e;st.fr=fr;st.to=to;
  q.style.transform="translate("+c.x.toFixed(2)+"px,"+c.y.toFixed(2)+"px)";q.style.width=c.w.toFixed(2)+"px";q.style.height=c.h.toFixed(2)+"px";q.style.opacity="1"}
function hlClear(box){if(!box)return;box.classList.remove("gread","hl-box");delete box.dataset.hlp;box._hl=null;var q=box.querySelector(":scope > .hl-quad");if(q)q.remove();
  hlWords(box).forEach(function(w){w.style.removeProperty("--t");w.classList.remove("hl-un","hl-on","hl-said")})}
/* the light's state for the checks: where the shape is now (the place's px) and its word */
function hlState(box){var st=box&&box._hl,q=box&&box.querySelector(":scope > .hl-quad");return st&&st.cur?{k:st.k,x:st.cur.x,y:st.cur.y,w:st.cur.w,h:st.cur.h,e:st.e,shown:!!q&&q.style.opacity!=="0"}:null}
/* the places this module knows: the classic captions or the dock's bubble (.gcap), and the block being read (the reading head's target) */
on("hlBoxes.add",function(out,h){ph.querySelectorAll(".gcap").forEach(function(c){out.push([c,c.closest(".gbubble")?"bubble":"captions"])});if(h.target&&h.target.isConnected)out.push([h.target,"block"])});

/* Editor: Guide › 1 · The mascot › Speech on screen: the highlight's style, its trail, the glide and a Try (round 21: from guide/readalong.js,
   so the highlighter has one home; Captions keeps its link row) */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7cap",row("Highlight style",pseg("guideCfg.hl",["comet","sentence","fill","word"],["Comet","Sentence + word","Fill","One word"]),"How the words being said light up, in every place speech is shown: the bubble, the teleprompter, the open panel, the block being read, the games' cards. Comet: a soft light glides from word to word and the words behind it keep a warm tint that fades away. Sentence + word: a soft band over the sentence being said, the light on the word in it (a small study found a sentence band easiest to follow for readers with dyslexia). Fill: the words already said stay tinted. One word: only the light on the word being said. Unity: a highlight quad behind the TextMeshPro text, tweened between the words' bounds; the tail as vertex colours.")+
    row("Trail",pslider("guideCfg.trail",2,14,1),"Comet only: how many words the fading tail covers behind the word being said.",true)+
    row("Glide",pseg("guideCfg.glide",["true","false"],["Smooth","Word by word"]),"Smooth: the light moves and resizes from one word to the next on an ease, timed by the voice (a recorded line's word times; the device voice's word boundaries). Word by word: it is on each word at once. Reduce Motion: word by word, the tail stays.")+
    row("Try",'<button class="del" type="button" data-hltry="1">Try</button>',"Starts the tour with these settings."));
  return d.innerHTML});
ed.addEventListener("click",function(e){var t=e.target.closest("[data-hltry]");if(t){e.stopPropagation();if(G.on)stopTour();startTour()}},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(hl|trail|glide|"));
/* each row's demo: a block read in place, its words lit */
rowDemos({"guideCfg.hl":{tour:"g4",ring:".gfocus"},"guideCfg.trail":{tour:"g4",ring:".gfocus"},"guideCfg.glide":{tour:"g4",ring:".gfocus"}});
