/* The preview bar on the phone (ptool): dark, reduce motion, language, focus; the device is chosen by W's device buttons (screen/device.js). */
LAB.previewBar=true;
function ptool(){
  return pipe("ptool.html",'<span class="lbl">'+TXH("pbar_label")+'</span><button type="button" data-pt="dark" aria-pressed="'+(themeMode()==="dark")+'" title="'+TXH("pbar_tip_theme")+'">'+(themeMode()==="dark"?"&#9790; "+TXH("pbar_dark"):"&#9728; "+TXH("pbar_light"))+'</button>'+
   '<button type="button" data-pt="reduce" aria-pressed="'+P.reduce+'" title="'+TXH("pbar_tip_motion")+'">'+(P.reduce?TXH("pbar_still"):TXH("pbar_motion"))+'</button>'+
   '<button type="button" data-pt="lang" aria-pressed="false" title="'+TXH("pbar_tip_lang")+'">'+P.lang.toUpperCase()+'</button>'+
   '<button type="button" data-pt="focusOn" aria-pressed="'+LAB.focusOn+'" title="'+TXH("pbar_tip_focus")+'">'+TXH("pbar_focus")+'</button>')}
on("renderPhone.after",function(keep){var pt=document.getElementById("ptool");if(pt)pt.style.display=LAB.previewBar?"":"none"});
