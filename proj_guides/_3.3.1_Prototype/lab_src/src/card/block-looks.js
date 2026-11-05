/* A block's own look, frame and reading time on the card. */
S.card.concat(S.demoCard).forEach(function(b){Object.assign(b,{k_look:"inherit",frame:"inherit",minsMode:"auto",mins:5,orw:{en:"",pt:""}},b)});
/* a marker's own look and frame win over the block's */
on("kopt.answer",function(kind,key){var b=S._cb;if(b&&b.kind===kind&&key==="look"&&b.k_look&&b.k_look!=="inherit")return b.k_look});
on("blockPreviewHtml.before",function(b){S._cb=b});on("blockPreviewHtml.done",function(){S._cb=null});
on("blockPreviewHtml.html",function(h,b){if(b.frame&&b.frame!=="inherit"&&S.secCfg.look==="cards")h=h.replace(/class="bprev( fr-[a-z]+)?/,'class="bprev fr-'+b.frame);
  if(b.minsMode==="hand")h=h.replace(/(<div class="open[^"]*"[^>]*>[^<]*?)\d+ min/,"$1"+b.mins+" min");
  var ow=b.orw&&L(b.orw);if(ow)h=h.replace(/(<div class="open[^"]*"[^>]*>)[^<]*/,"$1"+esc(ow)+" ");
  return h});
on("modelSig.before",function(cx){var x="";if(sigWant(/frame/)){var look=S.secCfg.look,keep=S.view;S.secCfg.look="cards";S.view="card";x="#cardsR#"+cardScreen();S.secCfg.look=look;S.view=keep}cx.frame=x});on("modelSig.sig",function(s,cx){return s+cx.frame});
