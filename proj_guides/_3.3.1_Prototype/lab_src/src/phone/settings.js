/* The visitor's settings panel (S.vis, settingsPanel) and the motion level. */
S.motionLevel="full";S.vis={lang:null,mode:"phone",motion:"phone",text:"default",feedback:true};S.settingsOpen=false;
ed.addEventListener("click",function(e){var t=e.target;
  if(t.closest("[data-forget]")){e.stopPropagation();S.vis={lang:null,mode:"phone",motion:"phone",text:"default",feedback:true};S.kqDone={};t.textContent="Forgotten";setTimeout(function(){renderEditor()},900);return}
  if(t.closest("[data-secreset]")){e.stopPropagation();S.sections=JSON.parse(SECTIONS0);renderAll(true);show("card");return}
  if(t.closest("[data-contents-try]")){e.stopPropagation();S.view="card";S.stop="full";S.contentsOpen=true;renderPhone(false);return}},true);
/* the Navigation Bar: shape, fill, the chosen item, labels, More */
/* the visitor's settings panel and the Contents sheet */
function settingsPanel(){var langs=(S.langs&&S.langs.list||[]).filter(function(l){return l.code});var opt=function(k,v,lab,cur){return'<button type="button" data-vis="'+k+'|'+v+'" aria-pressed="'+(cur===v)+'">'+esc(lab)+'</button>'};
  return pipe("settingsPanel.html",'<div class="vset" role="dialog" aria-label="'+TXH("visitor_settings")+'"><div class="vh"><b>'+TXH("visitor_settings")+'</b><button type="button" data-vset-close="1" aria-label="'+TXH("close")+'">&#10005;</button></div>'+
   (langs.length>1?'<small>'+TXH("language_switch")+'</small><div class="vo">'+langs.map(function(l){return opt("lang",l.code,l.name||l.code,S.lang)}).join("")+'</div>':"")+
   '<small>'+TXH("settings_appearance")+'</small><div class="vo">'+opt("mode","light",TXT("mode_light"),S.vis.mode)+opt("mode","dark",TXT("mode_dark"),S.vis.mode)+opt("mode","phone",TXT("mode_system"),S.vis.mode)+'</div>'+
   '<small>'+TXH("settings_motion")+'</small><div class="vo">'+opt("motion","phone",TXT("mode_system"),S.vis.motion)+opt("motion","reduced",TXT("motion_reduced"),S.vis.motion)+'</div>'+
   '<small>'+TXH("settings_text")+'</small><div class="vo">'+opt("text","default",TXT("text_default"),S.vis.text)+opt("text","large",TXT("text_large"),S.vis.text)+opt("text","larger",TXT("text_larger"),S.vis.text)+'</div>'+
   '<small>'+TXH("settings_feedback")+'</small><div class="vo">'+opt("feedback","true",TXT("settings_on"),String(S.vis.feedback))+opt("feedback","false",TXT("settings_off"),String(S.vis.feedback))+'</div></div>')}
