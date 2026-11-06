/* The preview bar under the phone (ptool): phone, wide, tablet, language, dark, reduce motion. */
S.previewBar=true;
function ptool(){var nx={phone:"land",land:"tablet",tablet:"phone"},nm={phone:"pbar_phone",land:"pbar_wide",tablet:"pbar_tablet"};
  return pipe("ptool.html",'<span class="lbl">'+TXH("pbar_label")+'</span><button type="button" data-pt="dark" aria-pressed="'+S.dark+'" title="'+TXH("pbar_tip_theme")+'">'+(S.dark?"&#9790; "+TXH("pbar_dark"):"&#9728; "+TXH("pbar_light"))+'</button>'+
   '<button type="button" data-pt="reduce" aria-pressed="'+S.reduce+'" title="'+TXH("pbar_tip_motion")+'">'+(S.reduce?TXH("pbar_still"):TXH("pbar_motion"))+'</button>'+
   '<button type="button" data-pt="lang" aria-pressed="false" title="'+TXH("pbar_tip_lang")+'">'+S.lang.toUpperCase()+'</button>'+
   '<button type="button" data-pt="focusOn" aria-pressed="'+S.focusOn+'" title="'+TXH("pbar_tip_focus")+'">'+TXH("pbar_focus")+'</button>'+
   '<button type="button" data-pt="screen" data-v="'+nx[S.screen]+'" aria-pressed="false" title="'+TXH("pbar_tip_screen")+'">'+TXH(nm[S.screen])+'</button>')}
on("renderPhone.after",function(keep){var pt=document.getElementById("ptool");if(pt)pt.style.display=S.previewBar?"":"none"});
