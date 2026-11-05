/* The peek row's place and the Locate Button at the peek stop. */
/* Peek: the title row starts at the sheet's own top */
function peekFix(){var pr=ph.querySelector(".peekrow");if(pr)pr.style.top=stopTop("peek")+"px"}
/* the locate label shows only where the hero's words leave it room (Unity: LabelFits) */
on("renderPhone.after",function(keep){peekFix();locFit()});scr.addEventListener("scroll",locFit,{passive:true});
on("renderPhone.after",function(keep){var lp=ph.querySelector(".locpin");if(lp)lp.style.display=S.view==="card"&&S.stop==="peek"?"none":"";setTimeout(locFit,420)});
