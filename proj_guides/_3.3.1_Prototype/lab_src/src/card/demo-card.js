/* The demo card: one block of every kind (S.demoCard), its screen and the demo tile. */
/* ----- the demo card: a card of its own with every block once, seeded once from the Ribeira Palace, then independent of every marker ----- */
function demoCardScreen(){var keep=S.card,kt=S.test;S.card=S.demoCard;S.test={on:true,poi:"painting"};inDemoCard=true;var h;try{h=cardScreen()}finally{S.card=keep;S.test=kt;inDemoCard=false}
  h=h.replace(/<i title="([^"]*)">&#10005;<\/i>/,'<i title="$1" data-go="card" role="button" style="cursor:pointer">&#10005;</i>');
  return h.replace(/<small>[^<]*<\/small><h2>The Ribeira Palace<\/h2>/,"<small>"+TXH("democard_tag")+"</small><h2>"+TXH("democard_title")+"</h2>")}
/* ----- the phone's own words for the test tile, the preview bar and the demo card ----- */
var inDemoCard=false;
/* ----- the demo card: every kept kind once, in its section's order ----- */
S.demoCard=[];(function(){var n=0;S.sections.forEach(function(sec){sec.kinds.forEach(function(k){var src=S.card.filter(function(b){return b.kind===k})[0];
  var b=src?JSON.parse(JSON.stringify(src)):{kind:k,grp:"inherit",enter:"inherit",tap:"inherit",orow:"inherit"};b.id="d"+(++n);b.grp="inherit";S.demoCard.push(b)})})})();
S.card.concat(S.demoCard).forEach(function(b){if(b.kind==="Story")b.preview="inherit"});
var DEMOCARD0=JSON.stringify(S.demoCard);
/* captures: #dcshot=<kind> opens the demo card at that block, still */
if(location.hash.indexOf("#dcshot=")===0)setTimeout(function(){var dk=decodeURIComponent(location.hash.slice(8)),pv=dk.split("|");S.reduce=true;S.previewBar=false;if(pv[1])S.lang=pv[1];S.view="democard";S.stop="full";renderAll(false);setTimeout(function(){var b=S.demoCard.filter(function(x){return x.kind===pv[0]})[0],el=b&&scr.querySelector('[data-ref="b:'+b.id+'"]');if(el)scr.scrollTop=Math.max(0,el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-70)},200)},50);
/* ----- the demo: a tile first on every marker's card opens Demo Components; its top button opens the demo card ----- */
delete FOCUS.s0tile;FOCUS.s0card=["card",".testtile","full"];
function testTile(){return inDemoCard?"":'<button class="testtile" type="button" data-go="demo"><div><b>'+TXH("test_tile_tag")+'</b><strong>'+TXH("test_tile_title")+'</strong><span>'+TXH("test_tile_line")+'</span></div><em>&#8250;</em></button>'}
on("demoScreen.html",function(h){return h.replace('<b>'+esc(L(UI.demo))+'</b>','<b>'+TXH("test_tile_title")+'</b>').replace('<div class="pg">','<div class="pg"><button class="testtile demoopen" type="button" data-go="democard"><div><b>'+TXH("test_tile_tag")+'</b><strong>'+TXH("test_demo_title")+'</strong><span>'+TXH("test_demo_line")+'</span></div><em>&#8250;</em></button>')});
