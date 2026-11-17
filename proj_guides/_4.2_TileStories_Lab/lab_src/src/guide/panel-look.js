/* The panels' look (round 21, 21.6; the developer, 2026-11-13: "the AI Guide panels, the minimal and the half screen, are black and too flat.
   Maybe make them more light, smooth, immersive, technological, with some light shapes and colours, 3D shapes, light and gradients and a
   little motion"; their references: a pastel glass app with soft pink-blue gradient cards on white, and a dark pill button with a soft
   green glow and a gradient edge). One option for the Mini, the open panel and the teleprompter together (Guide › 1 · The mascot ›
   Speech on screen › The panels' look; there because it is the surface the mascot's words are read on): Light glass (frosted white
   glass, soft pastel light shapes drifting very slowly behind, a fine bright rim and a soft inner light at its foot), Aurora (deep blue
   glass with coloured light drifting, a softly glowing mint rim, Play a dark pill with a green glow), Dark (round 11's Ink glass, kept),
   and Follows the theme (Light glass in light, Aurora in dark; the default). The drift is one slow loop of 26-28 s, off with Reduce
   Motion; every word 4.5:1 or more over the worst backdrop and the brightest light shape (test9z's arithmetic). The Look (Ink glass,
   Paper glass, Player sheet, Teleprompter) is still the panel's layout; the panels' look dresses Ink glass (the default) and the
   teleprompter; the other Looks keep their own paper surfaces.
   Unity: the glass a pre-blurred backdrop (a URP blur of the camera into a RenderTexture, sampled under the panel; a frosted texture when
   blur is off), the light shapes gradient sprites moved by a slow DOTween loop, the rim a 9-slice border image, the inner light a
   gradient image; one USS class per look on the dock's and the teleprompter's root (sk-light, sk-aurora, sk-dark). */
/* The mode decides the panels (the one dark mode, merge P1b.1): Light glass in light mode; in dark mode the author picks Aurora (the
   default) or the Ink glass of before. No panel has a light / dark setting of its own. */
D.guide.darkPanels="aurora";
var DARK_PANELS=[["aurora","Aurora"],["dark","Ink glass"]];
function pSkin(){return themeMode()!=="dark"?"light":D.guide.darkPanels==="dark"?"dark":"aurora"}
on("dock.cls",function(c){return c+" sk-"+pSkin()+" dkp-"+D.guide.darkPanels});
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7cap",row("The panels in dark mode",pseg("D.guide.darkPanels",DARK_PANELS.map(function(x){return x[0]}),DARK_PANELS.map(function(x){return x[1]})),"The surface of the Mini, the open panel and the teleprompter when the app is dark; in light mode they are Light glass (frosted white glass, soft pastel light drifting very slowly behind, a fine bright rim). Aurora: deep blue glass with coloured light drifting, a softly glowing rim, Play a dark pill with a green glow. Ink glass: the dark ink glass of before. The mode itself is the app's (App › Look and mode). The drift is one slow loop (26-28 s), still with Reduce Motion; every word stays 4.5:1 or more. It dresses the Ink glass Look (the default) and the teleprompter; Paper glass, Player sheet and the Teleprompter Look keep their own paper. Unity: a pre-blurred backdrop (URP blur into a RenderTexture) or a frosted texture, gradient sprites on a slow tween, a 9-slice rim."));
  return d.innerHTML});
/* the phone's signature (the self-test): the dock's surface as the visitor sees it */
on("modelSig.sig",function(h){return sigWant(/D\.guide\.darkPanels/)?h+'#skin:<div class="gdock lk-'+dkLook()+' sk-'+pSkin()+' dkp-'+D.guide.darkPanels+'"></div>':h});
/* its demo shows the dark mode: the option dresses nothing in light */
rowDemos({"D.guide.darkPanels":{tour:"g2",dock:"mini",ring:".gdock",wait:0,act:function(){if(themeMode()!=="dark"){P.vis.mode="dark";emit("profileChanged","mode");modeInPlace()}}}});
