// Guidance round 15: the design pass's rules as arithmetic, shared by test9s.js and test9sb.js (installed in the page as window.G15).
// Each rule is measured on the surfaces the app really drew: 44 px hit areas (probed with elementFromPoint, so a hit area larger than
// the drawing counts and a layer over a target does not), 4.5:1 text against the background really behind each word (a glass panel's
// tint composed over the lightest and the darkest sample of the wall or the card behind it), 0 px over the words being told (the told
// block's word boxes in view and the captions' current line), inside the phone (its edges, the notch's 47 px, the dock), one focal point.
module.exports = String.raw`window.G15=(function(){
var NOTCH=47;
function col(s){s=String(s||"").trim();var m;if((m=s.match(/^rgba?\(([^)]+)\)/))){var a=m[1].split(/[\s,\/]+/).filter(Boolean).map(parseFloat);return{r:a[0],g:a[1],b:a[2],a:a.length>3?a[3]:1}}
  if((m=s.match(/^color\(srgb ([^)]+)\)/))){var c=m[1].split(/[\s\/]+/).filter(Boolean).map(parseFloat);return{r:c[0]*255,g:c[1]*255,b:c[2]*255,a:c.length>3?c[3]:1}}
  if(s==="transparent")return{r:0,g:0,b:0,a:0};return null}
function over(f,b){var a=f.a+b.a*(1-f.a);if(!a)return{r:0,g:0,b:0,a:0};return{r:(f.r*f.a+b.r*b.a*(1-f.a))/a,g:(f.g*f.a+b.g*b.a*(1-f.a))/a,b:(f.b*f.a+b.b*b.a*(1-f.a))/a,a:a}}
function lum(c){return[c.r,c.g,c.b].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)}
function cr(a,b){var x=lum(a),y=lum(b);return(Math.max(x,y)+.05)/(Math.min(x,y)+.05)}
function shown(e){if(!e||!e.isConnected)return false;var r=e.getBoundingClientRect();if(r.width<1||r.height<1)return false;
  for(var x=e;x&&x!==document.body;x=x.parentElement){var cs=getComputedStyle(x);if(cs.display==="none"||cs.visibility==="hidden"||+cs.opacity<.05)return false}return true}
function R(e){var r=e.getBoundingClientRect();return{l:r.left,t:r.top,r:r.right,b:r.bottom,w:r.width,h:r.height}}
function inter(a,b){return Math.max(0,Math.min(a.r,b.r)-Math.max(a.l,b.l))*Math.max(0,Math.min(a.b,b.b)-Math.max(a.t,b.t))}
/* over a word: deeper than 1 px both ways (a line box carries its half-leading below the letters; the app's own azuOver allows 1 px) */
function deep(a,b){var x=Math.min(a.r,b.r)-Math.max(a.l,b.l),y=Math.min(a.b,b.b)-Math.max(a.t,b.t);return x>1&&y>1?x*y:0}
/* a point scrolled out of view inside a clipping parent (the thread scrolled, a chips row) is not seen */
function clipped(e,x,y){for(var a=e.parentElement;a&&a!==ph;a=a.parentElement){var ac=getComputedStyle(a);if(ac.overflowX!=="visible"||ac.overflowY!=="visible"){var q=R(a);if(x<q.l||x>q.r||y<q.t||y>q.b)return true}}return false}
function name(e){return(e.tagName.toLowerCase()+(e.className&&typeof e.className==="string"?"."+e.className.trim().split(/\s+/).slice(0,3).join("."):"")+(e.dataset&&(e.dataset.gdbt||e.dataset.gdtl||e.dataset.g||e.dataset.gdpg)?"["+(e.dataset.gdbt||e.dataset.gdtl||e.dataset.g||e.dataset.gdpg)+"]":"")).slice(0,60)}
/* the guide's surfaces: what the visitor sees from the guide */
var TEXTROOTS=".gdock,.gbubble,.gdbub,.gdbeat,.gdstrip,.gbtn-wrap,.gbmenu,.azbub,.gdpages,.gdtry,.gtoast,.gdtoast,.gdfewer,.camask .ca-ans,.camask .ca-chips";
var FOCAL=[".gdbub:not(.gd-out)",".azbub",".gbubble",".gdbeat",".gbmenu",".gdpages:not(.gd-out)",".gdtry"];
var COVER=".gdbub:not(.gd-out),.azbub,.gbubble,.gdbeat,.gdstrip,.mascot,.azfab,.azmoment,.gbtn-wrap,.gdfinger,.gdtoast,.gtoast";
var TARGET="button,[role=button],a[href],input,select,.gd-seek,[data-gdbt],[data-gdtl],[data-chip],[data-aglink],[data-gdpg]";
function roots(sel){return[].filter.call(ph.querySelectorAll(sel),shown)}
/* the background behind a text: the element's own and its parents' fills, up to an opaque one (a gradient: each of its stops) */
var SAMPLES=null;
function bgs(e){var layers=[],x=e,op=1,er=R(e),ex=er.l+er.w/2,ey=er.t+er.h/2;for(;x&&x!==ph.parentElement;x=x.parentElement){var xr=R(x);if(x!==e&&(ex<xr.l||ex>xr.r||ey<xr.t||ey>xr.b))continue;/* a parent the text sits outside (a pin's label above its dot) is not behind it */
    var cs=getComputedStyle(x),c=col(cs.backgroundColor),im=cs.backgroundImage;
    if(im&&im!=="none"&&/gradient/.test(im)){var st=(im.match(/(rgba?\([^)]*\)|color\(srgb[^)]*\))/g)||[]).map(col).filter(function(c){return c&&c.a>=.5});if(st.length){var al=st.map(function(s){return layers.concat([s])});return{alts:al,glass:!st.every(function(s){return s.a>.99})}}}
    if(c&&c.a>0){layers.push(c);if(c.a>.99)return{alts:[layers],glass:false}}}
  return{alts:[layers],glass:true}}
function compose(layers,base){var c=base;for(var i=layers.length-1;i>=0;i--)c=over(layers[i],c);return c}
function opac(e){var o=1;for(var x=e;x&&x!==ph;x=x.parentElement)o*=+getComputedStyle(x).opacity;return o}
/* the lightest and darkest of what is behind a glass surface: the card's paper and the wall's picture (sampled on a canvas) */
function behind(){if(SAMPLES)return SAMPLES;var out=[],sc=getComputedStyle(scr),pc=col(getComputedStyle(ph).backgroundColor);
  var card=ph.querySelector(".sheet,.card,.cardscr")||scr;var cc=col(getComputedStyle(card).backgroundColor);if(cc&&cc.a>.9)out.push(cc);if(pc&&pc.a>.9)out.push(pc);
  out.push({r:255,g:255,b:255,a:1},{r:0,g:0,b:0,a:1});SAMPLES=out;return out}
function text(rootSel){var res=[];roots(rootSel||TEXTROOTS).forEach(function(rt){var w=document.createTreeWalker(rt,NodeFilter.SHOW_TEXT),n;while((n=w.nextNode())){if(!/\S/.test(n.nodeValue))continue;var e=n.parentElement;if(!shown(e))continue;
    var rg=document.createRange();rg.selectNodeContents(n);var rr=rg.getBoundingClientRect();if(rr.width<2||rr.height<2)continue;if(clipped(e,rr.left+rr.width/2,rr.top+rr.height/2))continue;var pr=ph.getBoundingClientRect();if(rr.right<pr.left||rr.left>pr.right||rr.bottom<pr.top||rr.top>pr.bottom)continue;
    var cs=getComputedStyle(e),fc=col(cs.color);if(!fc)continue;var o=opac(e),worst=99,wb=null,fcs=[fc],own=e;
    /* a word painted by its background (the read-along's background-clip:text): its colours are its gradient's stops, over its parents */
    var fill=col(cs.webkitTextFillColor);if(fill&&fill.a===0&&/text/.test(cs.webkitBackgroundClip||cs.backgroundClip)){var st=((cs.backgroundImage||"").match(/(rgba?\([^)]*\)|color\(srgb[^)]*\))/g)||[]).map(col).filter(Boolean);if(st.length){fcs=st;own=e.parentElement}}
    var B=bgs(own);var bases=B.glass?behind():[{r:0,g:0,b:0,a:1}];
    fcs.forEach(function(f0){var f1=Object.assign({},f0,{a:f0.a*o});B.alts.forEach(function(L){bases.forEach(function(bs){var bg=compose(L,bs),t=over(f1,bg),v=cr(t,bg);if(v<worst){worst=v;wb=bg}})})});
    res.push({t:n.nodeValue.trim().slice(0,28),in:name(rt),el:name(e),cr:+worst.toFixed(2),glass:B.glass,bg:wb?[Math.round(wb.r),Math.round(wb.g),Math.round(wb.b)]:null})}});return res}
/* a target's hit area: probed 21.5 px each side of its centre (a larger hit area than its drawing counts; a layer over it does not) */
var covered=[];function targets(){var out=[],seen=[];covered=[];roots(TEXTROOTS+",.mascot,.azfab").forEach(function(rt){var list=[].slice.call(rt.querySelectorAll(TARGET));if(rt.matches(".gdbub,.mascot,.azfab,.gbtn-wrap .gbtn")||rt.matches(TARGET))list.unshift(rt);
    if(rt.matches(".gbtn-wrap"))list=list.concat([].slice.call(rt.querySelectorAll(".gbtn")));
    list.forEach(function(e){if(seen.indexOf(e)>=0||!shown(e))return;var cs=getComputedStyle(e);if(cs.pointerEvents==="none"||e.disabled)return;if(e.tagName==="INPUT"&&e.type==="hidden")return;seen.push(e);
      var r=R(e),cx=r.l+r.w/2,cy=r.t+r.h/2;for(var a=e.parentElement;a&&a!==ph;a=a.parentElement){var ac=getComputedStyle(a);if(ac.overflowX!=="visible"||ac.overflowY!=="visible"){var q=R(a);if(cx<q.l||cx>q.r||cy<q.t||cy>q.b)return}}
      var c0=document.elementFromPoint(cx,cy);if(c0&&c0!==e&&!e.contains(c0)&&!c0.contains(e)){covered.push(name(e)+" under "+name(c0));return}
      var pts=[[cx-20,cy],[cx+20,cy],[cx,cy-20],[cx,cy+20]],hit=pts.map(function(p){var h=document.elementFromPoint(p[0],p[1]);return!!h&&(h===e||e.contains(h))});
      out.push({el:name(e),in:name(rt),w:+r.w.toFixed(1),h:+r.h.toFixed(1),ok:hit.every(Boolean)&&r.w>=43.5&&r.h>=43.5,hit:hit.map(function(x){return x?1:0}).join("")})})});return out}
/* the words being told: the told block's text in view, and the captions' current line */
/* at a seam (a beat card showing) no step is being told; a game inside a step (the strip, the word card) is judged against it */
function told(){var out=[],sv=R(scr);if(G.on&&!(G.gd&&G.gd.bt)&&typeof stepEl==="function"){var st=stepEl(S.guideSteps[G.i]);if(st){var w=document.createTreeWalker(st,NodeFilter.SHOW_TEXT),n,rg=document.createRange();
    while((n=w.nextNode())){if(!/\S/.test(n.nodeValue))continue;if(!shown(n.parentElement))continue;rg.selectNodeContents(n);[].forEach.call(rg.getClientRects(),function(q){var a={l:Math.max(q.left,sv.l),t:Math.max(q.top,sv.t),r:Math.min(q.right,sv.r),b:Math.min(q.bottom,sv.b)};if(a.r-a.l>1&&a.b-a.t>1)out.push({k:"block",r:a})})}}}
  var cap=ph.querySelector(".gd-tp .tp-now");if(cap&&shown(cap)){[].forEach.call(cap.querySelectorAll(".tw"),function(t){if(shown(t))out.push({k:"captions",r:R(t)})})}return out}
/* Azu's box has a transparent margin round its drawing (the tile, the 3D model's canvas): a contact of 2 px or less is its margin, not
   a word covered (the app's own placing, azuOver, allows 1 px) */
function coverRect(e){var r=R(e);if(e.matches(".mascot,.azfab"))return{l:r.l+2,t:r.t+2,r:r.r-2,b:r.b-2,w:r.w-4,h:r.h-4};return r}
function overWords(){var W=told(),out=[],dk=ph.querySelector(".gdock"),dkr=dk&&shown(dk)?R(dk):null;
  /* a word the panel already hides (at Full, the card under it) is not seen: what else sits over it is not judged */
  var seen=W.filter(function(w){if(w.k!=="block"||!dkr)return true;var cx=(w.r.l+w.r.r)/2,cy=(w.r.t+w.r.b)/2;if(cx<dkr.l||cx>dkr.r||cy<dkr.t||cy>dkr.b)return true;var h=document.elementFromPoint(cx,cy);return!(h&&dk.contains(h))});
  roots(COVER).forEach(function(e){if(e.closest(".gdock"))return;var r=coverRect(e),a=0;if(dkr&&e.matches(".mascot")&&inter(r,dkr)>=.9*r.w*r.h)return;seen.forEach(function(w){a+=deep(r,w.r)});if(a>0.5)out.push({el:name(e),px:Math.round(a)})});
  /* the dock over the told block's words */
  var d=ph.querySelector(".gdock");if(d&&shown(d)&&!d.classList.contains("h-full")){var dr=R(d),a2=0;W.forEach(function(w){if(w.k==="block")a2+=deep(dr,w.r)});if(a2>0.5)out.push({el:"the panel",px:Math.round(a2)})}
  /* a focus ring's stroke (2 px round the hole) over the words */
  roots(".gspot.on,.gdring").forEach(function(g){var r=R(g),o={l:r.l-2,t:r.t-2,r:r.r+2,b:r.b+2},a3=0;W.forEach(function(w){if(w.k!=="block")return;a3+=inter(o,w.r)-inter(r,w.r)});if(a3>0.5)out.push({el:"the ring "+name(g),px:Math.round(a3)})});
  return{n:W.length,over:out}}
/* inside the phone: its edges, the notch's 47 px at the top, never clipped under the dock */
function inside(){var pr=R(ph),out=[],dock=ph.querySelector(".gdock"),dr=dock&&shown(dock)?R(dock):null;
  roots(TEXTROOTS+",.mascot,.azfab,.azmoment,.gdseek,.gdfinger").forEach(function(e){if(e.closest(".gdock")&&!e.matches(".gdock"))return;if(e.matches(".gbtn-wrap")&&!shown(e.querySelector(".gbtn")))return;var r=coverRect(e),why=[],c0=document.elementFromPoint(r.l+r.w/2,r.t+r.h/2);
    if(c0&&!e.contains(c0)&&!c0.contains(e)&&dr&&inter(r,dr)>=.9*r.w*r.h&&dock.contains(c0))return;
    if(c0&&!e.contains(c0)&&!c0.contains(e)&&c0.closest(".gdpages,.onb"))return;
    if(r.l<pr.l-1)why.push("left "+Math.round(pr.l-r.l));if(r.r>pr.r+1)why.push("right "+Math.round(r.r-pr.r));if(r.b>pr.b+1)why.push("bottom "+Math.round(r.b-pr.b));
    if(r.t<pr.t-1)why.push("top "+Math.round(pr.t-r.t));else if(r.t<pr.t+NOTCH-1&&!e.matches(".gdpages,.gdtry,.gdock.h-full"))why.push("notch "+Math.round(pr.t+NOTCH-r.t));
    if(dr&&!e.matches(".gdock,.gdbeat")){var a=inter(r,dr);if(a>4){var cx=(Math.max(r.l,dr.l)+Math.min(r.r,dr.r))/2,cy=(Math.max(r.t,dr.t)+Math.min(r.b,dr.b))/2,h=document.elementFromPoint(cx,cy);if(h&&dock.contains(h))why.push("under the dock "+Math.round(a)+" px²")}}
    if(why.length)out.push({el:name(e),why:why.join(", ")})});return out}
/* round 15b: Mascot Off's ✦ mark in the bubble: present, inside the bubble, its px² over the bubble's words (any overlap), whether a tap at
   its centre reaches it (elementFromPoint), its pointer-events and aria-hidden */
function sign(){var b=ph.querySelector(".gdbub:not(.gd-out)");if(!b)return null;var s=b.querySelector(".gdsign"),pl=b.classList.contains("plain");if(!s||!shown(s))return{has:false,plain:pl};
  var sr=R(s),br=R(b),a=0,w=document.createTreeWalker(b.querySelector("[data-gdbub]")||b,NodeFilter.SHOW_TEXT),n,rg=document.createRange();
  while((n=w.nextNode())){if(!/\S/.test(n.nodeValue))continue;rg.selectNodeContents(n);[].forEach.call(rg.getClientRects(),function(q){a+=inter(sr,{l:q.left,t:q.top,r:q.right,b:q.bottom})})}
  var h=document.elementFromPoint(sr.l+sr.w/2,sr.t+sr.h/2);
  return{has:true,plain:pl,inside:sr.l>=br.l-.5&&sr.t>=br.t-.5&&sr.r<=br.r+.5&&sr.b<=br.b+.5,over:Math.round(a),tap:!!h&&(h===s||s.contains(h)),pe:getComputedStyle(s).pointerEvents,aria:s.getAttribute("aria-hidden"),wh:[Math.round(sr.w),Math.round(sr.h)]}}
/* round 15b: the card's words under Azu: every text of the card in view under Azu's whole box, deeper than 1 px both ways (the Architect's
   probe, probe_azu.js: a stricter measure than overWords, which judges the told words with Azu's 2 px margin) */
function azuInk(){var m=ph.querySelector(".mascot");if(!m)return 0;var r=R(m),a=0,w=document.createTreeWalker(scr,NodeFilter.SHOW_TEXT),n,rg=document.createRange();
  while((n=w.nextNode())){if(!/\S/.test(n.nodeValue))continue;var cs=getComputedStyle(n.parentElement);if(cs.visibility==="hidden"||cs.display==="none"||cs.opacity==="0")continue;rg.selectNodeContents(n);[].forEach.call(rg.getClientRects(),function(q){a+=deep(r,{l:q.left,t:q.top,r:q.right,b:q.bottom})})}return Math.round(a)}
/* round 16b: the hero's words at a step told at the hero (the teaser, the end; the written tour's g0): its eyebrow, title, line and any
   button still shown, each word's box against the panel (not at Full), the bubbles, Azu, the ✦ and the beat cards (px² deeper than 1 px
   both ways) and inside the phone's free height; at Mini the panel covered the line and the Start button (the Architect's captures) */
function hero(){var h=scr.querySelector(".hero");if(!h)return null;var W=[],rg=document.createRange(),w=document.createTreeWalker(h,NodeFilter.SHOW_TEXT),n,sv=R(scr);
  while((n=w.nextNode())){if(!/\S/.test(n.nodeValue)||!shown(n.parentElement))continue;rg.selectNodeContents(n);[].forEach.call(rg.getClientRects(),function(q){if(q.width>1&&q.height>1&&q.bottom>sv.t&&q.top<sv.b)W.push({t:n.nodeValue.trim().slice(0,20),r:{l:q.left,t:q.top,r:q.right,b:q.bottom}})})}
  var out=[],d=ph.querySelector(".gdock");if(d&&shown(d)&&!d.classList.contains("h-full")){var a=0,dr=R(d);W.forEach(function(x){a+=deep(dr,x.r)});if(a>.5)out.push({el:"the panel",px:Math.round(a)})}
  roots(COVER).forEach(function(e){if(e.closest(".gdock"))return;var r=coverRect(e),a=0;W.forEach(function(x){a+=deep(r,x.r)});if(a>.5)out.push({el:name(e),px:Math.round(a)})});
  return{n:W.length,words:W.map(function(x){return x.t}).filter(function(t,i,a){return a.indexOf(t)===i}),over:out,btn:!!(h.querySelector(".gstart")&&shown(h.querySelector(".gstart")))}}
function focal(){var out=[];FOCAL.forEach(function(s){roots(s).forEach(function(e){if(out.indexOf(e)<0)out.push(e)})});return out.map(name)}
function audit(){SAMPLES=null;var tg=targets();return{targets:tg,covered:covered.slice(),text:text(),words:overWords(),inside:inside(),focal:focal(),ph:[ph.offsetWidth,ph.offsetHeight]}}
return{audit:audit,text:text,targets:targets,told:told,overWords:overWords,inside:inside,focal:focal,sign:sign,azuInk:azuInk,hero:hero,col:col,cr:cr}})();true`;
