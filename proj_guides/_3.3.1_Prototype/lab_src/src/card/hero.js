/* The hero: its content (S.hero), its height by the stop, the bar after it. */
S.hero={title:T("The Ribeira Palace","O Paço da Ribeira"),eyebrow:T("Lisbon · c. 1500 to 1755","Lisboa · c. 1500 a 1755"),line:null,fx:50,fy:50,pic:"hero",pic2:"g2"};
/* the hero's content (the Header page) */
on("cardScreen.html",function(h){if(typeof inDemoCard!=="undefined"&&inDemoCard)return h;
  /* the eyebrow and the line under the title: the point's own (guidance round 16: the wall's other points have their hero), else the demo's */
  var ey=S.hero.eyebrow?L(S.hero.eyebrow):L(UI.where),ln=S.hero.line?L(S.hero.line):L(UI.line);
  return h.replace("<small>"+esc(L(UI.where))+"</small><h2>","<small>"+esc(ey)+"</small><h2>").replace("</h2><p>"+esc(L(UI.line))+"</p>","</h2><p>"+esc(ln)+"</p>").replace(/<h2>The Ribeira Palace<\/h2>/,"<h2>"+esc(L(S.hero.title))+"</h2>").replace(/(<div class="hero[^"]*" style="background-image:url\()[^)]*\)/,"$1"+IMG[S.hero.pic]+");background-position:"+S.hero.fx+"% "+S.hero.fy+"%")});
function heroH(){var H=SCREENS[S.screen][1];if(S.screen==="land")return 300;return Math.max(220,H-stopTop(S.stop==="peek"||S.stop==="closed"?"half":S.stop))}
function barAfterHero(){var nb=document.getElementById("navbar"),c=S.ctl.bar;if(!nb||S.view!=="card"||!c.on||c.showFrom!=="hero"||S.stop==="peek"||S.stop==="closed")return;
  var hero=scr.querySelector(".hero"),past=hero?scr.scrollTop>hero.offsetHeight-140:true;if(!past)nb.style.display="none";else if(S.card.length>=c.minBlocks&&S.sections.filter(shownOnBar).length>=2)nb.style.display=""}
on("renderPhone.after",function(keep){fitHero();heroPaint();barAfterHero();editorTools()});
/* ----- guidance round 16b: a point's own picture, a crop of the wall painting around that point (the developer: the Torreão, the Terreiro
   do Paço and the Tagus showed the Ribeira's hero). S.hero.crop = {wall, at (the wall's point), h (the crop's height, a fraction of the
   picture's, at most), w (the crop's width, a fraction of the picture's, at most: 0.4, so a short hero at Half still shows its point and
   not the whole wall), fx, fy (where the point sits in the hero, fractions)}: drawn on the same file by background-size and -position (no new file),
   the point at (fx, fy) of the hero, never past the picture's edges; its motion drifts around the crop (hero.css, .hcrop). It holds while
   the Picture is the wall's (Specific Marker › Header › Picture: another picture is drawn whole). Unity: the RawImage's uvRect on the
   same texture (u0 = -x / width, v0 = 1 - (-y + H) / height, its width W / width and height H / height) ----- */
function heroCropOf(){var c=S.hero.crop;if(!c||typeof WALLS==="undefined")return null;var wl=WALLS[c.wall];if(!wl||IMG[S.hero.pic]!==wl.img())return null;
  var p=(wl.pois||[]).filter(function(x){return x.id===c.at})[0];return p?{iw:wl.w,ih:wl.h,u:p.u,v:p.v,h:c.h||1,w:c.w||.4,fx:c.fx!=null?c.fx:.5,fy:c.fy!=null?c.fy:.4}:null}
/* the crop at a hero H high: the picture's drawn size (covering the hero), and its offset as CSS: the point at fx of the hero's width
   whatever that width (a background-position percentage is of the hero less the picture: fx·100% + (fx − u)·width px = fx·W − u·width),
   clamped to the picture's edges (100% is the hero less the picture, 0 its left edge); the drift's two ends 12 px each way */
function heroCropBox(c,W,H){var z=kopt("Header","look")==="spotlight"?Math.max(1,+kopt("Header","zoom")||1):1,s=Math.max(H/(c.ih*Math.min(1,c.h/z)),W/(c.iw*Math.min(1,c.w/z)));var bw=c.iw*s,bh=c.ih*s;
  var y=Math.min(0,Math.max(H-bh,H*c.fy-c.v*bh)),cx=function(d){return"clamp(100%, calc("+(c.fx*100).toFixed(1)+"% + "+((c.fx-c.u)*bw+d).toFixed(1)+"px), 0px)"};
  return{bw:bw,bh:bh,y:y,x:cx(0),x0:cx(12),x1:cx(-12)}}
/* the hero's own picture, drawn again (after a redraw, and after the teaser's pictures); its width and height the screen's target ones
   (heroH: the phone may still be growing to its new size as it is drawn) */
function heroPaint(h){h=h||scr.querySelector(".hero");if(!h||S.view!=="card"&&S.view!=="democard")return;var c=heroCropOf();h.classList.toggle("hcrop",!!c);
  if(h.style.backgroundImage!=="none")h.style.backgroundImage="url("+IMG[S.hero.pic]+")";if(!c)return;
  var W=(SCREENS[S.screen]||[h.offsetWidth])[0],H=S.heroH||h.offsetHeight,b=heroCropBox(c,W,H),px=function(v){return v.toFixed(1)+"px"};
  h.style.backgroundSize=px(b.bw)+" "+px(b.bh);h.style.backgroundPosition=b.x+" "+px(b.y);h.style.setProperty("--hx0",b.x0);h.style.setProperty("--hx1",b.x1);h.style.setProperty("--hy",px(b.y))}
/* the pictures the teaser shows one after another: the palace's four; a point drawn as a crop keeps its own */
function heroPics(){return heroCropOf()||S.m&&S.m.poi&&S.m.poi!=="painting"?[IMG[S.hero.pic]]:[IMG[S.hero.pic],IMG.g1,IMG.g3,IMG.g2]}
scr.addEventListener("scroll",barAfterHero,{passive:true});
/* ----- a change redraws in place: the hero keeps its height, what was on screen stays shown, the scroll stays ----- */
function fitHero(){var hero=scr.querySelector(".hero");if(!hero||(S.view!=="card"&&S.view!=="democard"))return;
  if(S.heroH){hero.style.transition="none";hero.style.height=S.heroH+"px";void hero.offsetHeight;hero.style.transition=""}
  S.heroH=heroH();hero.style.height=S.heroH+"px"}
