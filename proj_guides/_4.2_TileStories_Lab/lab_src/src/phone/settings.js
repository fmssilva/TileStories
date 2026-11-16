/* The visitor's settings panel (P.vis, settingsPanel) and the motion level. */
D.card.motionLevel="full";P.vis={lang:null,mode:"phone",motion:"phone",text:"default",feedback:true};ui.settingsOpen=false;
ed.addEventListener("click",function(e){var t=e.target;
  if(t.closest("[data-forget]")){e.stopPropagation();P.vis={lang:null,mode:"phone",motion:"phone",text:"default",feedback:true};P.kqDone={};P.guide={};t.textContent="Forgotten";setTimeout(function(){renderEditor()},900);return}
  if(t.closest("[data-secreset]")){e.stopPropagation();D.card.sections=JSON.parse(SECTIONS0);renderAll(true);show("card");return}
  if(t.closest("[data-contents-try]")){e.stopPropagation();ui.view="card";ui.stop="full";ui.contentsOpen=true;renderPhone(false);return}},true);
/* the Navigation Bar: shape, fill, the chosen item, labels, More */
/* the visitor's settings panel and the Contents sheet */
function settingsPanel(){var langs=(D.wall.langs&&D.wall.langs.list||[]).filter(function(l){return l.code});var opt=function(k,v,lab,cur){return'<button type="button" data-vis="'+k+'|'+v+'" aria-pressed="'+(cur===v)+'">'+esc(lab)+'</button>'};
  return pipe("settingsPanel.html",'<div class="vset" role="dialog" aria-label="'+TXH("visitor_settings")+'"><div class="vh"><b>'+TXH("visitor_settings")+'</b><button type="button" data-vset-close="1" aria-label="'+TXH("close")+'">&#10005;</button></div>'+
   (langs.length>1?'<small>'+TXH("language_switch")+'</small><div class="vo">'+langs.map(function(l){return opt("lang",l.code,l.name||l.code,P.lang)}).join("")+'</div>':"")+
   '<small>'+TXH("settings_appearance")+'</small><div class="vo">'+opt("mode","light",TXT("mode_light"),P.vis.mode)+opt("mode","dark",TXT("mode_dark"),P.vis.mode)+opt("mode","phone",TXT("mode_system"),P.vis.mode)+'</div>'+
   '<small>'+TXH("settings_motion")+'</small><div class="vo">'+opt("motion","phone",TXT("mode_system"),P.vis.motion)+opt("motion","reduced",TXT("motion_reduced"),P.vis.motion)+'</div>'+
   '<small>'+TXH("settings_text")+'</small><div class="vo">'+opt("text","default",TXT("text_default"),P.vis.text)+opt("text","large",TXT("text_large"),P.vis.text)+opt("text","larger",TXT("text_larger"),P.vis.text)+'</div>'+
   '<small>'+TXH("settings_feedback")+'</small><div class="vo">'+opt("feedback","true",TXT("settings_on"),String(P.vis.feedback))+opt("feedback","false",TXT("settings_off"),String(P.vis.feedback))+'</div></div>')}
