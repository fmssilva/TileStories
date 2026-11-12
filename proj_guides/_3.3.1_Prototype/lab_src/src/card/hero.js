/* The hero: its content (S.hero), its height by the stop, the bar after it. */
S.hero={title:T("Ribeira Palace","Palácio da Ribeira"),eyebrow:T("Lisbon · c. 1500 to 1755","Lisboa · c. 1500 a 1755"),line:null,fx:50,fy:50,pic:"hero",pic2:"g2"};
/* the hero's content (the Header page) */
on("cardScreen.html",function(h){if(typeof inDemoCard!=="undefined"&&inDemoCard)return h;
  /* the eyebrow and the line under the title: the point's own (guidance round 16: the wall's other points have their hero), else the demo's */
  var ey=S.hero.eyebrow?L(S.hero.eyebrow):L(UI.where),ln=S.hero.line?L(S.hero.line):L(UI.line);
  return h.replace("<small>"+esc(L(UI.where))+"</small><h2>","<small>"+esc(ey)+"</small><h2>").replace("</h2><p>"+esc(L(UI.line))+"</p>","</h2><p>"+esc(ln)+"</p>").replace(/<h2>Ribeira Palace<\/h2>/,"<h2>"+esc(L(S.hero.title))+"</h2>").replace(/(<div class="hero[^"]*" style="background-image:url\()[^)]*\)/,"$1"+IMG[S.hero.pic]+");background-position:"+S.hero.fx+"% "+S.hero.fy+"%")});
function heroH(){var H=SCREENS[S.screen][1];if(S.screen==="land")return 300;return Math.max(220,H-stopTop(S.stop==="peek"||S.stop==="closed"?"half":S.stop))}
function barAfterHero(){var nb=document.getElementById("navbar"),c=S.ctl.bar;if(!nb||S.view!=="card"||!c.on||c.showFrom!=="hero"||S.stop==="peek"||S.stop==="closed")return;
  var hero=scr.querySelector(".hero"),past=hero?scr.scrollTop>hero.offsetHeight-140:true;if(!past)nb.style.display="none";else if(S.card.length>=c.minBlocks&&S.sections.filter(shownOnBar).length>=2)nb.style.display=""}
on("renderPhone.after",function(keep){fitHero();barAfterHero();editorTools()});
scr.addEventListener("scroll",barAfterHero,{passive:true});
/* ----- a change redraws in place: the hero keeps its height, what was on screen stays shown, the scroll stays ----- */
function fitHero(){var hero=scr.querySelector(".hero");if(!hero||(S.view!=="card"&&S.view!=="democard"))return;
  if(S.heroH){hero.style.transition="none";hero.style.height=S.heroH+"px";void hero.offsetHeight;hero.style.transition=""}
  S.heroH=heroH();hero.style.height=S.heroH+"px"}
