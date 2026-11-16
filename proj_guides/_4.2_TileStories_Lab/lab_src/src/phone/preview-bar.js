/* The preview bar under the phone (ptool): phone, wide, tablet, language, dark, reduce motion. */
LAB.previewBar=true;
function ptool(){var nx={phone:"land",land:"tablet",tablet:"phone"},nm={phone:"pbar_phone",land:"pbar_wide",tablet:"pbar_tablet"};
  return pipe("ptool.html",'<span class="lbl">'+TXH("pbar_label")+'</span><button type="button" data-pt="dark" aria-pressed="'+P.dark+'" title="'+TXH("pbar_tip_theme")+'">'+(P.dark?"&#9790; "+TXH("pbar_dark"):"&#9728; "+TXH("pbar_light"))+'</button>'+
   '<button type="button" data-pt="reduce" aria-pressed="'+P.reduce+'" title="'+TXH("pbar_tip_motion")+'">'+(P.reduce?TXH("pbar_still"):TXH("pbar_motion"))+'</button>'+
   '<button type="button" data-pt="lang" aria-pressed="false" title="'+TXH("pbar_tip_lang")+'">'+P.lang.toUpperCase()+'</button>'+
   '<button type="button" data-pt="focusOn" aria-pressed="'+LAB.focusOn+'" title="'+TXH("pbar_tip_focus")+'">'+TXH("pbar_focus")+'</button>'+
   '<button type="button" data-pt="screen" data-v="'+nx[LAB.screen]+'" aria-pressed="false" title="'+TXH("pbar_tip_screen")+'">'+TXH(nm[LAB.screen])+'</button>')}
on("renderPhone.after",function(keep){var pt=document.getElementById("ptool");if(pt)pt.style.display=LAB.previewBar?"":"none"});
