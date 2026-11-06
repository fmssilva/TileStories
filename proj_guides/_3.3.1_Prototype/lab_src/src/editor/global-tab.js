/* The Global Scene tab: the wall's markers and how a visitor finds them (edGlobal, GSECS). */
function edGlobal(){var h='<div class="note">The top level only: the wall\'s markers and how a visitor finds them.</div>';
  var gc=function(id,sub){return fo(id,esc(COMP[id][1])+' <span class="sub">· '+sub+'</span>',null,ownbar(id)+' '+ownPanels(id),2)};
  GSECS.forEach(function(g,i){var srch=i===GSECS.length-1,body=srch?row("Synonym Groups",'<button class="ubtn sm" type="button" data-goloc="term:t3">Languages &amp; Terms › Terms &#8250;</button>',"One list with the card\'s glossary. Keyword Fields stay here: only the search reads them.")+gc("g:10s","where the visitor types a search")+gc("g:10f","opens the filters, with how many are on"):' ';
   h+=fo("g:"+i,esc(g[0])+' <span class="sub">· '+esc(g[1])+'</span>',srch?["ext","EXTENDED"]:["","EXISTING"],body,1)});
  h+=fo("g:11",'Onboarding hints <span class="sub">· the first-use hints over the wall</span>',["new","NEW"],row("Wall hint",'<button class="ubtn sm" type="button" data-goto="gd:g:start">Said by the guide: Guide › Guidance › The start &#8250;</button>',"The pill over the wall is gone: the guide says it now, beside the nearest marker, at a pause (Opening message, then Tap a marker). Its words, its timing and how often: Guide › 4 · Guidance › App › The start.")+ownbar("g:11")+ownPanels("g:11"),1);return h}
/* ----- Global Scene: the top level only ----- */
var GSECS=[["Labels, Text & Fonts","the marker labels' words, sizes and fonts"],["Marker","shape, icon and the category table"],["Orientation","how markers, labels and badges face the camera"],
 ["Badge","the second sign on a marker, and its table"],["Outline","the ring around a marker, and its levels"],["Effects","glow, pulse and the other marker effects, with their Test grid"],
 ["Hierarchy Levels","what shows at each level of the wall"],["LOD","how much detail shows with the distance"],["Zoom","how far the visitor zooms, and its steps"],
 ["Displacement","how labels make room for each other"],["Select, Filter & Search","selection, search, filters, results, minimap, voice, keyword fields"]];
