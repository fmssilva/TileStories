/* Card, Sections and Card Controls as in Unity, merged with the Lab's: the accents, the corners, the strips, the section starts, the bar's shape and fill, the term popups. */
/* ----- state: the merged options and their defaults ----- */
Object.assign(S.cardCfg,{fullHeight:"top",backBtn:"step",accent:"heritage",round:"soft",strips:"edge",openOne:false,earbuds:"pause",lockScreen:"pause",resume:true});
if(S.cardCfg.topBand==="clear")S.cardCfg.topBand="fade";
Object.assign(S.secCfg,{glass:true,rim:true,motion:"reveal",divSec:"none",nextUp:true});
Object.assign(S.ctl.bar,{shape:"floating",fill:"solid",chosenIcon:"filled",chosenColour:"section",minBlocks:8,showAt:"both",showFrom:"hero",more:6});
Object.assign(S.ctl,{stickySlide:true,contents:{on:true,rows:"sections",ticks:true,chips:true},term:{opens:"popup",underline:"first"},pageChips:true,viewer:{counter:"chips"},player:{captions:true}});
S.sections.forEach(function(s){if(s.shown===false)s.shown=false;else s.shown=true});
var ACCENTS={heritage:["#1B4691","#8FB0EC","Heritage"],azulejo:["#13507E","#7DB8E8","Azulejo"],terracotta:["#823114","#F0A07A","Terracotta"],garden:["#24532F","#8CC99B","Garden"]};
/* the section icons: Unity's set (CardIcons.All) + the Lab's star and ar */
Object.assign(SICON,{home:'<path d="M4 11l8-7 8 7v9H4z"/><path d="M10 20v-5h4v5"/>',clock:'<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',map:'<path d="M3.5 6.5l5.5-2 6 2 5.5-2v13l-5.5 2-6-2-5.5 2z"/><path d="M9 4.5v13M15 6.5v13"/>',
 camera:'<rect x="3.5" y="7" width="17" height="12" rx="2"/><circle cx="12" cy="13" r="3.5"/><path d="M9 7l1.5-2.5h3L15 7"/>',music:'<path d="M9 18V5l11-2v13"/><circle cx="6.5" cy="18" r="2.5"/><circle cx="17.5" cy="16" r="2.5"/>',
 question:'<circle cx="12" cy="12" r="9"/><path d="M9.5 9.5a2.5 2.5 0 1 1 3.5 2.3c-.6.3-1 .9-1 1.6v.6M12 17v.4"/>',people:'<circle cx="9" cy="8" r="3"/><circle cx="17" cy="9" r="2.4"/><path d="M3.5 19c0-3 2.5-5 5.5-5s5.5 2 5.5 5M14.5 14.5c3 0 5.5 1.7 5.5 4.5"/>',
 crown:'<path d="M4 17l-1-9 5 4 4-7 4 7 5-4-1 9z"/><path d="M4 20h16"/>',ship:'<path d="M3 16l2 4h14l2-4z"/><path d="M12 3v13M12 4l6 8h-6M12 6l-5 6h5"/>',church:'<path d="M12 2v4M10 4h4M6 21V11l6-5 6 5v10z"/><path d="M10 21v-4a2 2 0 0 1 4 0v4"/>',
 tower:'<path d="M7 21V8h10v13zM7 8V4h2v2h2V4h2v2h2V4h2v4"/><path d="M11 21v-3h2v3"/>',tree:'<path d="M12 21v-6"/><path d="M12 3l6 8h-3l4 5H5l4-5H6z"/>',ticket:'<path d="M3.5 8.5V6h17v2.5a2.5 2.5 0 0 0 0 5V16h-17v-2.5a2.5 2.5 0 0 0 0-5z"/><path d="M14 6v10" stroke-dasharray="2 2"/>',
 access:'<circle cx="12" cy="4.5" r="1.8"/><path d="M5 8.5l7 1.5 7-1.5M12 10v4l-3 7M12 14l3 7"/>',landmark:'<path d="M3 21h18M5 18h14M6 18V10M10 18V10M14 18V10M18 18V10M4 10h16L12 4z"/>',sparkle:'<path d="M12 3l1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8z"/><path d="M19 15l.8 2.2L22 18l-2.2.8L19 21l-.8-2.2L16 18l2.2-.8z"/>',
 heart:'<path d="M12 20s-7-4.4-7-10a4 4 0 0 1 7-2.6A4 4 0 0 1 19 10c0 5.6-7 10-7 10z"/>',flag:'<path d="M5 21V4M5 4h12l-2 4 2 4H5"/>',sun:'<circle cx="12" cy="12" r="4"/><path d="M12 2v2.5M12 19.5V22M2 12h2.5M19.5 12H22M4.9 4.9l1.8 1.8M17.3 17.3l1.8 1.8M4.9 19.1l1.8-1.8M17.3 6.7l1.8-1.8"/>',
 water:'<path d="M12 3s6 6.5 6 11a6 6 0 0 1-12 0c0-4.5 6-11 6-11z"/>',key:'<circle cx="8" cy="15" r="4"/><path d="M11 12l9-9M17 6l3 3M15 8l2 2"/>',route:'<circle cx="6" cy="18" r="2.5"/><circle cx="18" cy="6" r="2.5"/><path d="M8.5 18H15a3 3 0 0 0 0-6H9a3 3 0 0 1 0-6h6.5"/>'});
var SLIDERS='<svg viewBox="0 0 24 24" width="17" height="17" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" aria-hidden="true"><path d="M4 7h9M17 7h3M4 17h3M11 17h9"/><circle cx="15" cy="7" r="2"/><circle cx="9" cy="17" r="2"/></svg>';
var DOTS='<svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor" aria-hidden="true"><circle cx="5" cy="12" r="1.8"/><circle cx="12" cy="12" r="1.8"/><circle cx="19" cy="12" r="1.8"/></svg>';
function shownOnCard(s){return s&&s.shown!==false&&S.poiSec[s.id]!=="hide"}
function shownOnBar(s){return s&&s.shown===true&&S.poiSec[s.id]!=="hide"}
var SEC_STARTS=[["tiles","Tiles"],["wave","Wave"],["arches","Arches"],["skyline","Skyline"],["fade","Fade (the tint only)"],["plain","Plain (no edge, no tint)"],["own","Own strip picture"]];
function startOpts(withInherit){var o=SEC_STARTS.slice();S.effects.filter(function(f){return f.grp==="edge"&&["e_wave","e_tiles","e_arches","e_skyline"].indexOf(f.id)<0}).forEach(function(f){o.push([f.id,f.name+" (Effects)"])});return withInherit?[["inherit","As configured ("+S.secCfg.start+")"]].concat(o):o}

/* ----- the Editor: Card ----- */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  var s1p=d.querySelector('[data-fo="s1peek"]');if(s1p)s1p.remove();
  setRow(d,"s1a","Opens at",row("Opens at",pseg("cardCfg.openAt",["peek","half"],["Peek","Half"]),"How far the card opens when a marker is tapped. It never opens Full by itself, so the wall stays in view (D8)."));
  setRow(d,"s1a","Half height max",row("Half height max",pslider("cardCfg.halfMax",0.30,0.60,0.01),"How much of the screen the Half stop covers; the wall stays visible above it. 0.40 is the measured Half (337.6 of 844 pt)."));
  addRows(d,"s1a",row("Full height",pseg("cardCfg.fullHeight",["top","strip"],["To the top","Leave a camera strip"]),"How high the card rises at Full: to the top of the screen, or 48 pt short so a strip of the live camera stays above it."),"Tap outside closes");
  setRow(d,"s1a","Tap outside closes",row("Tap outside closes",pseg("cardCfg.tapOutside",["true","false"],["On","Off"]),"A tap on the camera closes the card. A tap on a marker still selects it; a drag never closes; the X and a swipe down always close."));
  addRows(d,"s1a",row("Phone's Back button",pseg("cardCfg.backBtn",["step","app"],["Steps the card down","Left to the app"]),"Android. Each press undoes the innermost thing: a full view, a list, the row opened last, then one height down, then close."));
  addRows(d,"s1b",row("Accent",pseg("cardCfg.accent",Object.keys(ACCENTS),Object.keys(ACCENTS).map(function(k){return ACCENTS[k][2]})),"The one accent colour: links, the focal action, the Open Row. A closed list, each measured at 7:1 or more in light and dark.")+
    prow("Roundness",pseg("cardCfg.round",["sharp","soft","round"],["Sharp","Soft","Round"]),"The corners of blocks, pictures and buttons: three radius sets of the theme."),"Layout");
  addRows(d,"s1b",row("Open one at a time",pseg("cardCfg.openOne",["false","true"],["Off","On"]),"With the Accordion layout, and for every Expand Row: opening one folds the open one; the tapped row keeps its place.",true),"Top band");
  setRow(d,"s1b","Top band",row("Top band",pseg("cardCfg.topBand",["fade","glass"],["Fade","Glass"]),"What sits under the title row after the hero. Fade: the sheet's own colour, solid under the row, then nothing. Glass: frosted. (Clear is gone: the title would sit on scrolling text.)"));
  addRows(d,"s1b",row("Swipe strips",pseg("cardCfg.strips",["edge","whole"],["Show the next item's edge","Whole items only"]),"Every horizontal strip (a peek Carousel, the Timeline strip, Related): a slice of the next item shows at the edge, and a swipe ends on a whole item."));
  setRow(d,"s1c","Two sounds at once",row("When another sound starts",pseg("cardCfg.audioOverlap",["fade","duck","queue"],["Fade out and switch","Lower the other","Queue the new one"]),"What the playing sound does when the visitor starts another (an audio or a video). Fade out and switch: it fades in 0.5 s and waits on its play button. Lower the other: it goes on quietly under the new one. Queue: a new audio waits for the playing one (a video still fades the audio out). Never two voices at full volume."));
  addRows(d,"s1c",prow("When earbuds disconnect",pseg("cardCfg.earbuds",["pause","speaker"],["Pause","Keep playing on the speaker"]),"Pause is the phones' own convention: a narration never bursts from the speaker in a quiet room."));
  addRows(d,"s1mini",prow("With the screen locked",pseg("cardCfg.lockScreen",["pause","play"],["Pause","Keep playing"]),"Keep playing lets a walking visitor listen with the phone in a pocket.")+
    prow("Resume where the visitor left",pseg("cardCfg.resume",["true","false"],["On","Off"]),"Within one visit, an audio or a story opens where the visitor stopped."));
  addRows(d,"s0card",row("Saved answers",'<button class="ubtn sm" type="button" data-forget="1">Forget saved answers</button>',"Forgets on this computer everything this wall's cards saved: answers, votes, revealed questions, Contents ticks and the picked language."));
  /* ----- Sections ----- */
  addRows(d,"s2cfg",row("Glass light",pseg("secCfg.glass",["true","false"],["On","Off"]),"Cards style only: slow coloured light moving behind each section's glass sheet. Still under Reduce Motion.",true)+
    row("Rim light",pseg("secCfg.rim",["true","false"],["On","Off"]),"Cards style only: one rim light rides the block nearest the middle; never two. It is the card's one glow (D5).",true),"Block order");
  setRow(d,"s2cfg","Block order",row("Block order",pseg("secCfg.order",["sections","placed","placed_sources"],["By section","As placed","As placed, Sources last"]),"By section: blocks gather in their section's order. As placed: the marker's own order. As placed, Sources last: the marker's order, with the sources closing the card."));
  var cardsNote=S.secCfg.look==="cards"?'<div class="note warn">Style is Cards: section starts, dividers between sections and tints do not draw.</div>':"";
  setRow(d,"s2start","Style",row("Style",psel("secCfg.start",startOpts(false)),"What marks where a section begins: an edge, the tint alone (Fade), or nothing (Plain). Any edge of Effects can be one.")+
    row("Motion",pseg("secCfg.motion",["reveal","still"],["Reveal","Still"]),"Reveal: the start's tiles turn in one by one as the row rises from the Navigation Bar to a third of the way up; Replay follows Motion › Replay. Reduce Motion shows them whole.")+cardsNote);
  setRow(d,"s2div","Style",row("Between sections",psel("secCfg.divSec",[["none","None"],["l_hair","Hairline"],["l_tile","Tile Ornament"],["e_tiles","Tile Row"],["e_mosaic","Mosaic Strip"],["own","Own strip picture"]]),"Only before a Fade or Plain start: an edge start is the one ornament at its boundary.")+
    row("Between blocks",psel("secCfg.divider",[["none","None"]].concat(S.effects.filter(function(f){return f.grp==="line"}).map(function(f){return[f.id,f.name]}))),"The line between two blocks of one section. Every line divider of Effects.")+cardsNote);
  setRow(d,"s2tint","Colour",row("Colour",pseg("secCfg.colour",["section","accent","neutral"],["Section colour","Accent","Neutral"]),"Neutral: a warm grey tint."));
  setRow(d,"s2tint","Intensity",row("Intensity",pslider("secCfg.intensity",0.04,0.20,0.01),"At 0.30 even white fails 7:1 over a dark tinted start, so 0.20 is the top."));
  var f2n=d.querySelector('[data-fo="s2next"]');if(f2n){var b2=f2n.querySelector(".badge");if(b2)b2.remove();if(!innerOf(d,"s2next"))f2n.insertAdjacentHTML("beforeend",'<div class="inner"></div>');
    addRows(d,"s2next",row("Shown",pseg("secCfg.nextUp",["true","false"],["On","Off"]),"In the flow at a section's end: the next block's name, its place on the card (left out under Pages) and Next. At the card's end: \"You have reached the end\", no button."))}
  S.sections.forEach(function(sec,i){var id="sec:"+sec.id;
    setRow(d,id,"Start",row("Start",psel("sections."+i+".start",startOpts(true)),"Overrides All sections › Section start for this section only."));
    setRow(d,id,"On the bar",row("Shown",pseg("sections."+i+".shown",["true","card","false"],["On the card and the bar","On the card only","Hidden"]),"On the card only: its blocks stay on the card, with no bar item. Hidden: the section and its blocks leave every card."))});
  var s2i=innerOf(d,"s2");if(s2i){var br=s2i.querySelector(":scope > .btnrow:last-of-type");var busy=S.sections.filter(shownOnBar).length;
    var html='<div class="btnrow"><button class="ubtn sm" type="button" data-secreset="1">Reset to the framework\'s sections</button></div>'+(busy>S.ctl.bar.more?'<div class="note warn">'+esc(L(S.hero.title))+' shows '+busy+' sections and the bar holds '+S.ctl.bar.more+': the rest go under More.</div>':"");
    if(br)br.insertAdjacentHTML("afterend",html);else s2i.insertAdjacentHTML("beforeend",html)}
  /* ----- Motion: the wall's level ----- */
  addRows(d,"s4m",row("Motion level",pseg("motionLevel",["full","reduced"],["Full","Reduced"]),"Reduced gives every visitor whole, still parts: no reveals, no parallax, no loops. A phone set to Reduce Motion, or a visitor who picks Reduced, always gets Reduced."));
  /* ----- Card Controls ----- */
  dropRow(d,"s5top","Settings button");
  addRows(d,"s5top",'<div class="note">The settings button is always shown: it holds the visitor\'s language, light / dark, motion and text size.</div>');
  addRows(d,"s5settings",'<div class="note">The panel offers, in this order: Language (only with 2+ languages, by their own names), Appearance (Light / Dark / Follow the phone), Motion (Follow the phone / Reduced), Text size (Default / Large / Larger) and Haptics and sounds (On / Off). A visitor\'s reading needs are never the wall\'s to switch off. Tap the settings mark on the phone to try it.</div>');
  setRow(d,"s5bar","Indicator",row("Indicator",pseg("ctl.bar.indicator",["raised","pill","underline","none"],["Raised","Pill","Underline","None"]),"How the chosen section is marked. None: only the bolder icon and word mark it."));
  addRows(d,"s5bar",row("Shape",pseg("ctl.bar.shape",["floating","full"],["Floating","Full width"]),"Floating: rounded and inset from the sides. Full width: edge to edge, square corners.")+
    row("Fill",pseg("ctl.bar.fill",["solid","glass","clear"],["Solid","Glass","Clear"]),"Only Solid has a shadow. Over the hero, Glass and Clear turn white on dark glass; Clear is solid after the hero.")+
    row("Chosen icon",pseg("ctl.bar.chosenIcon",["filled","plain"],["Filled","Plain"]),"Filled: in a disc of the chosen colour. Plain: the icon alone in that colour. The chosen item is always bold (a fixed rule: never colour alone).")+
    row("Chosen colour",pseg("ctl.bar.chosenColour",["section","accent","ink"],["Section colour","Accent","Ink"]),"The colour of the chosen item's disc, pill or underline."),"Hide while scrolling");
  setRow(d,"s5bar","Labels",row("Labels",pseg("ctl.bar.labels",["both","icon","text","chosen"],["Icon and text","Icon","Text","Text on the chosen"]),"An icon alone is still named for screen readers."));
  addRows(d,"s5bar",row("Shown from",pslider("ctl.bar.minBlocks",2,30,1),"A card with fewer blocks shows no bar and simply scrolls (Tabs and Pages too). The bar also needs two sections.")+
    row("Shown at",pseg("ctl.bar.showAt",["both","full"],["Half and Full","Full only"]),"Full only keeps Half's room for the content; with Position = Top.")+
    row("Shown from the start",pseg("ctl.bar.showFrom",["always","hero"],["Always","After the hero"]),"After the hero: the bar comes once the hero's picture scrolls away.")+
    row("Items before More",pseg("ctl.bar.more",["4","5","6"],["4","5","6"]),"With more sections, the last item is More; it opens the Contents sheet."),"Hide while scrolling");
  setRow(d,"s5bar","After a tap, the card is at",row("After a tap, the card is at",pseg("ctl.jump.opens",["full","stay"],["Full","Where it is"]),"The bar never shows at Peek, so a tap either raises the card to Full or leaves it where it is."));
  var cs=d.querySelector('[data-fo="s5contents"]');if(cs){var b3=cs.querySelector(".badge");if(b3)b3.remove();if(!innerOf(d,"s5contents"))cs.insertAdjacentHTML("beforeend",'<div class="inner"></div>');
    addRows(d,"s5contents",row("Shown",pseg("ctl.contents.on",["true","false"],["On","Off"]),"Opened from the bar's More, or from a Contents block. Section rows with their counts are fixed.")+
      row("Rows",pseg("ctl.contents.rows",["sections","every"],["Sections first","Every block"]),"Sections first: the sections, each opening to its blocks. Every block: one flat list.",true)+
      row("Read ticks",pseg("ctl.contents.ticks",["true","false"],["Shown","Hidden"]),"A tick beside what this visitor has read, kept on the device.",true)+
      row("Filter chips",pseg("ctl.contents.chips",["true","false"],["Shown","Hidden"]),"Up to 2 / 5 min, No headphones, No camera, No network, and Clear.",true)+
      '<div class="btnrow"><button class="ubtn sm" type="button" data-contents-try="1">Open it on the phone</button></div>')}
  var pgr=d.querySelector('[data-fo="s5pager"]');if(pgr){var b4=pgr.querySelector(".badge");if(b4)b4.remove();if(!innerOf(d,"s5pager"))pgr.insertAdjacentHTML("beforeend",'<div class="inner"></div>');
    addRows(d,"s5pager",'<div class="note">No options. Under Pages it is always there ("2 of 6" between chevrons, and the next page\'s edge with its icon and name): the way between pages without a swipe (WCAG 2.5.7). Under Tabs it shows while the bar is off. Set Card › Look › Layout to Pages to see it.</div>')}
  addRows(d,"s5expand",row("Open one at a time",pseg("cardCfg.openOne",["false","true"],["Off","On"]),"The same switch as Card › Look (one value): opening a row folds the open one."));
  addRows(d,"s5back",row("Page chips",pseg("ctl.pageChips",["true","false"],["Shown","Hidden"]),"Under the page's top bar: one icon chip per block of the section; a swipe goes to the next one."));
  addRows(d,"s5sticky",row("Slides away while reading",pseg("ctl.stickySlide",["true","false"],["On","Off"]),"It slides away while the visitor reads down, and comes back on a move up or at the top or the end."));
  addRows(d,"s5term",row("Opens as",pseg("ctl.term.opens",["popup","under"],["Popup by the word","Under the paragraph"]),"Where a tap on an underlined word shows its definition; a second tap closes it.")+
    prow("Underline a term",pseg("ctl.term.underline",["first","every"],["The first time on the card","Every time"]),"Repeated underlines clutter the body text."));
  setRow(d,"s5loc","Style",row("Style",pseg("ctl.locate.style",["pin","pin_label","glass"],["Floating Pin","Pin + Label","Glass + Ping"]),"Pin + Label: the pin with \"Find it on the wall\" while there is room. Glass + Ping: the pin in a glass disc that pings."));
  addRows(d,"s5loc",'<div class="note">Hidden without the camera. It does not depend on the Navigation Bar.</div>');
  /* new components: the viewers and the placed model chip */
  var s5=innerOf(d,"s5");if(s5){var anchor=s5.querySelector(':scope > [data-fo="s5term"]');
    var nv=comp("s5viewer","the full-screen picture: swipe, pinch, double tap",row("Counter",pseg("ctl.viewer.counter",["chips","count","none"],["Chips","3 of 12","None"]),"How the viewer shows which picture this is. Double tap zooms 2x; a pinch 1-4x; one finger pans when zoomed."))+
      comp("s5player","the video page and the full-screen player",row("Captions",pseg("ctl.player.captions",["true","false"],["On at start","Off at start"]),"A block may override. Captions help in noisy rooms and with the sound off."))+
      comp("s5chip","above the card while a model placed in the room stands",'<div class="note">No options: it shows at every stop while a model stands, and its Remove is the only way to take the model away.</div>');
    if(anchor)anchor.insertAdjacentHTML("beforebegin",nv);else s5.insertAdjacentHTML("beforeend",nv);
    ["s5viewer","s5player","s5chip"].forEach(function(o){var f=innerOf(d,o);if(f)f.insertAdjacentHTML("beforeend",ownbar(o)+ownPanels(o))})}
  return d.innerHTML});

/* ----- writes that need more than the state ----- */
on("setOpt.answer",function(key,v){if(key==="@motionLevel"){S.motionLevel=v;S.reduce=v==="reduced";renderAll(true);show("card");return true}
  if(key==="@cardCfg.mode"){S.cardCfg.mode=v;S.dark=v==="dark"||(v==="system"&&!!(window.matchMedia&&matchMedia("(prefers-color-scheme: dark)").matches));renderAll(true);show("card");return true}
  if(key==="@ctl.bar.more"){S.ctl.bar.more=+v;renderAll(true);show("card");return true}});
var SECTIONS0=JSON.stringify(S.sections);

/* ----- the phone: stops, accent, roundness, strips ----- */
on("stopTop.answer",function(st){var H=SCREENS[S.screen][1];if(S.screen==="land")return 0;if(st==="peek")return H-64;if(st==="full")return S.cardCfg.fullHeight==="strip"?42:0});
function v8vars(){var st=document.getElementById("v8vars");if(!st){st=document.createElement("style");st.id="v8vars";document.head.appendChild(st)}var a=ACCENTS[S.cardCfg.accent]||ACCENTS.heritage;
  st.textContent=".cv{--accent2:"+a[0]+"}.cv.dark,#ph.dark,.dark .cv{--accent2:"+a[1]+"}"}
/* sections on the card: the merged starts, the dividers between sections, Next Up, the order with Sources last */
function startOf(sec){return sec.start==="inherit"?S.secCfg.start:sec.start}
function ownStrip(){return '<div class="ownstrip" style="height:24px;background:repeating-linear-gradient(90deg,var(--fam) 0 12px,color-mix(in srgb,var(--fam) 35%,transparent) 12px 24px)"></div>'}
on("cardScreen.before",function(){S._gseen={}});
on("cardScreen.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  h=null;d.querySelectorAll(".chrome i").forEach(function(i){if(i.innerHTML.charCodeAt(0)===9776||i.textContent==="☰")i.innerHTML=SLIDERS});
  var secs=d.querySelectorAll(".gsec[data-ref]"),runs=cardRuns(),total=S.card.length,pages=S.cardCfg.layout==="pages";
  secs.forEach(function(x,i){var sid=x.dataset.ref.slice(4),sec=secById(sid);if(!sec)return;var st=startOf(sec);
    if(S.secCfg.look==="open"&&st==="plain")x.style.background="none";
    if(S.secCfg.look==="open"&&S.secCfg.colour==="neutral"&&st!=="plain")x.style.background="linear-gradient(to bottom, color-mix(in srgb,#8A8072 "+Math.round(S.secCfg.intensity*100)+"%, transparent) 0, transparent "+S.secCfg.tint+"px)";
    if(S.secCfg.look==="cards"){if(S.secCfg.glass)x.classList.add("glasslit");if(S.secCfg.rim&&i===0){var b0=x.querySelector(".bprev");if(b0)b0.classList.add("rimlit")}}
    if(i>0&&S.secCfg.look==="open"&&S.secCfg.divSec!=="none"&&(st==="fade"||st==="plain")){var f=effectById(S.secCfg.divSec),dv=document.createElement("div");dv.className="secdiv";dv.innerHTML=S.secCfg.divSec==="own"?ownStrip():f?(f.grp==="edge"?edgeSvg(f):lineSvg(f)):"";x.parentNode.insertBefore(dv,x)}
    if(S.secCfg.nextUp&&S.cardCfg.layout!=="accordion"){var ri=-1;runs.forEach(function(r,k){if(r.sec.id===sid&&ri<0)ri=k});var nx=runs[ri+1],nu=document.createElement("div");nu.className="nextup";
      if(nx&&nx.blocks.length){var nb=nx.blocks[0],pos=S.card.indexOf(nb)+1;nu.innerHTML='<div><small>'+TXH("nav_next_up")+'</small><b>'+esc(L(nx.sec.name))+' · '+esc(nb.kind)+'</b>'+(pages?"":'<span>'+esc(fmt(TXT("nav_next_up_position"),pos,total))+'</span>')+'</div><button type="button" data-nextsec="'+nx.sec.id+'">'+TXH("nav_next_button")+' &#8250;</button>'}
      else nu.innerHTML='<div><b>'+TXH("nav_next_up_done")+'</b></div>';x.appendChild(nu)}});
  var lp=d.querySelector(".lt-pages");if(lp){var ids=runs.map(function(r){return r.sec.id}),k=ids.indexOf(S.activeSec);if(k<0)k=0;var nxs=secById(ids[(k+1)%ids.length]);
    lp.innerHTML='<button type="button" data-pagesec="'+ids[(k-1+ids.length)%ids.length]+'" aria-label="'+TXH("nav_pager_previous")+'">&#8249;</button><span><b>'+esc(L(secById(ids[k]).name))+'</b> '+esc(fmt(TXT("nav_pager_position"),k+1,ids.length))+'</span><button type="button" data-pagesec="'+ids[(k+1)%ids.length]+'" aria-label="'+TXH("nav_pager_next")+'">&#8250;</button>';
    lp.insertAdjacentHTML("afterend",'<button type="button" class="pgedge" data-pagesec="'+nxs.id+'"><i style="color:'+colVar(nxs.colour)+'">'+sicon(nxs.icon,16)+'</i>'+esc(L(nxs.name))+' &#8250;</button>')}
  if(S.cardCfg.layout==="tabs"&&!S.ctl.bar.on){var tb=d.querySelector(".lt-tabs");if(tb){var ids2=runs.map(function(r){return r.sec.id}),k2=Math.max(0,ids2.indexOf(S.activeSec)),cs=secById(ids2[k2]);
    tb.insertAdjacentHTML("afterend",'<div class="lt-pages"><button type="button" data-pagesec="'+ids2[(k2-1+ids2.length)%ids2.length]+'" aria-label="'+TXH("nav_pager_previous")+'">&#8249;</button><span><i style="color:'+colVar(cs.colour)+'">'+sicon(cs.icon,14)+'</i> <b>'+esc(L(cs.name))+'</b> '+esc(fmt(TXT("nav_pager_position"),k2+1,ids2.length))+'</span><button type="button" data-pagesec="'+ids2[(k2+1)%ids2.length]+'" aria-label="'+TXH("nav_pager_next")+'">&#8250;</button></div>')}}
  if(S.cardCfg.openOne&&S.cardCfg.layout==="accordion")d.querySelectorAll(".lt-acc").forEach(function(a){a.dataset.one="1"});
  return d.innerHTML});
on("renderPhone.before",function(keep){v8vars();ph.classList.toggle("rd-sharp",S.cardCfg.round==="sharp");ph.classList.toggle("rd-round",S.cardCfg.round==="round");ph.classList.toggle("strips-whole",S.cardCfg.strips==="whole");
  ph.classList.toggle("tx-large",S.vis.text==="large");ph.classList.toggle("tx-larger",S.vis.text==="larger");});
on("renderPhone.after",function(keep){var inCard=S.view==="card",nb=document.getElementById("navbar"),c=S.ctl.bar;
  if(nb&&inCard){nb.innerHTML=navbar();var vis=S.sections.filter(shownOnBar);
    if(S.card.length<c.minBlocks||vis.length<2||(c.showAt==="full"&&c.pos==="top"&&S.stop!=="full")||(c.showFrom==="hero"&&scr.scrollTop<200))nb.style.display="none";
    nb.classList.toggle("nb-full",c.shape==="full");nb.classList.toggle("nb-glass",c.fill==="glass");nb.classList.toggle("nb-clear",c.fill==="clear")}
  ["vset","csheet","peekrow"].forEach(function(k){var o=ph.querySelector("."+k);if(o)o.remove()});
  if(inCard&&S.stop==="peek"){var pr=document.createElement("div");pr.className="peekrow";pr.style.top=(stopTop("peek")+14)+"px";pr.innerHTML='<b>'+esc(L(S.hero.title))+'</b><small>'+esc(L(UI.where))+'</small>';ph.appendChild(pr)}
  if(inCard&&S.settingsOpen){ph.insertAdjacentHTML("beforeend",settingsPanel())}
  if(inCard&&S.contentsOpen&&S.ctl.contents.on){ph.insertAdjacentHTML("beforeend",contentsSheet())}
  var tb=ph.querySelector(".topband");if(tb)tb.querySelectorAll("i").forEach(function(i){if(i.textContent==="☰")i.innerHTML=SLIDERS})});
scr.addEventListener("scroll",function(){if(S.view!=="card")return;var nb=document.getElementById("navbar"),c=S.ctl.bar;if(nb&&c.on&&c.showFrom==="hero"&&S.stop!=="peek"&&S.stop!=="closed")nb.style.display=scr.scrollTop<200?"none":"";
  var st=ph.querySelector(".sticky");if(st&&S.ctl.stickySlide){var y=scr.scrollTop,dir=y>(S._lastY||0);st.classList.toggle("away",dir&&y>40&&y<scr.scrollHeight-scr.clientHeight-20);S._lastY=y}},{passive:true});
ph.addEventListener("click",function(e){var t=e.target;
  var ch=t.closest(".chrome i, .topband i");if(ch&&S.view==="card"&&ch.querySelector("svg")&&ch.innerHTML.indexOf("M4 7h9")>=0){e.stopPropagation();S.settingsOpen=!S.settingsOpen;renderPhone(true);return}
  if(t.closest("[data-vset-close]")){e.stopPropagation();S.settingsOpen=false;renderPhone(true);return}
  var vo=t.closest("[data-vis]");if(vo){e.stopPropagation();var p=vo.dataset.vis.split("|");if(p[0]==="lang"){S.lang=p[1]}else if(p[0]==="mode"){S.vis.mode=p[1];S.dark=p[1]==="dark"||(p[1]==="phone"&&S.cardCfg.mode==="dark")}else if(p[0]==="motion"){S.vis.motion=p[1];S.reduce=p[1]==="reduced"||S.motionLevel==="reduced"}else if(p[0]==="feedback"){S.vis.feedback=p[1]==="true";S.fb.haptics=S.vis.feedback}else S.vis[p[0]]=p[1];renderAll(true);return}
  if(t.closest("[data-navmore]")){e.stopPropagation();S.contentsOpen=true;renderPhone(true);return}
  if(t.closest("[data-csheet-close]")){e.stopPropagation();S.contentsOpen=false;renderPhone(true);return}
  var cr=t.closest(".csheet .crow.sec");if(cr){e.stopPropagation();S.contentsOpen=false;renderPhone(true);return}
  var nx=t.closest("[data-nextsec]");if(nx){e.stopPropagation();var el=scr.querySelector('[data-ref="sec:'+nx.dataset.nextsec+'"]');S.activeSec=nx.dataset.nextsec;if(el)scr.scrollTo({top:el.offsetTop-60,behavior:S.reduce?"auto":"smooth"});feedback("jump");return}
  var acc=t.closest('.lt-acc[data-one="1"]');if(acc){var id=acc.dataset.accsec;Object.keys(S.openSecs).forEach(function(k){if(k!==id)S.openSecs[k]=false})}},true);
/* the glossary: the first time only, and under the paragraph */
on("gloss.html",function(h){if(S.ctl.term.underline!=="first")return h;S._gseen=S._gseen||{};
  return h.replace(/<span class="gterm" tabindex="0" data-gterm="([^"]*)" data-gdef="[^"]*">([\s\S]*?)<\/span>/g,function(m,id,w){if(S._gseen[id])return w;S._gseen[id]=1;return m})});
on("storyScreen.before",function(){S._gseen={}});
ph.addEventListener("click",function(e){var g=e.target.closest(".gterm");if(!g||S.ctl.term.opens!=="under")return;setTimeout(function(){var p=ph.querySelector(".gpop");if(!p)return;var par=g.closest("p,.el-par,.el-lede")||g.parentNode;p.classList.add("under");par.insertAdjacentElement("afterend",p)},0)});
on("modelSig.before",function(cx){var keep=S.view;S.view="card";var x=(sigWant(/langs|vis/)?settingsPanel():"")+(sigWant(/ctl\.contents|mins/)?contentsSheet():"");S.view=keep;cx.panels=x});
on("modelSig.sig",function(s,cx){return s+cx.panels+JSON.stringify({full:stopTop("full"),peek:stopTop("peek"),acc:S.cardCfg.accent,round:S.cardCfg.round,strips:S.cardCfg.strips,ml:S.motionLevel,
    bar:[S.ctl.bar.shape,S.ctl.bar.fill,S.ctl.bar.minBlocks<=S.card.length,S.ctl.bar.showAt,S.ctl.bar.showFrom,S.ctl.bar.pos]})});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(cardCfg\\.(backBtn|openOne|earbuds|lockScreen|resume|audioOverlap)|ctl\\.(stickySlide|term\\.opens)|"));
delete S.cardCfg.peekNext;
/* ----- the demos of the rows made (or remade) here: the card at the stop and the part each row changes, ringed ----- */
function secSel(id){return'[data-ref="sec:'+id+'"]'}
var CTL_DEMOS={
  "cardCfg.openAt":{stop:function(){return S.cardCfg.openAt},wait:0,ring:"#grab|.hero"},
  "cardCfg.halfMax":{stop:"half",wait:0,ring:"#grab|.hero"},
  "cardCfg.fullHeight":{stop:"full",wait:0,ring:".chrome|.hero"},
  "cardCfg.tapOutside":{stop:"half",wait:0,act:function(){var c=ph.querySelector("#camera");if(c)c.dispatchEvent(new MouseEvent("click",{bubbles:true}))},ringAfter:300,ring:function(){return S.stop==="closed"?"#wall":".hero"}},
  "cardCfg.mode":{stop:"full",wait:0,scroll:".gsec",ring:".gsec"},
  "cardCfg.accent":{block:"Story",wait:0,scroll:function(){return blockAt("Story").sel.split("|")[0]+" .open"},ring:function(){return blockAt("Story").sel.split("|")[0]+" .open"}},
  "cardCfg.round":{block:"Story",wait:0},
  "cardCfg.openOne":{block:"Practical Info",wait:0,act:function(){var e=phFind(blockAt("Practical Info").sel),x=e&&e.querySelector("[data-expand]");if(x)x.click()}},
  "cardCfg.topBand":{stop:"full",past:true,wait:0,ring:".topband"},
  "cardCfg.strips":{block:"Gallery",wait:0},
  "secCfg.glass":{stop:"full",wait:0,scroll:".gsec",ring:".gsec"},"secCfg.rim":{stop:"full",wait:0,scroll:".gsec",ring:".rimlit|.gsec"},
  "secCfg.order":{stop:"full",wait:0,scroll:".gsec",ring:".gsec"},
  "secCfg.start":{stop:"full",wait:0,scroll:secSel("stories"),ring:secSel("stories")+" .sstart|"+secSel("stories")},
  "secCfg.motion":{stop:"full",wait:0,scroll:secSel("stories"),ring:secSel("stories")+" .sstart|"+secSel("stories")},
  "secCfg.divSec":{stop:"full",wait:0,scroll:".secdiv|"+secSel("stories"),ring:".secdiv|"+secSel("stories")},
  "secCfg.divider":{stop:"full",wait:0,scroll:secSel("about"),ring:secSel("about")+" .el-div|"+secSel("about")},
  "secCfg.colour":{stop:"full",wait:0,scroll:secSel("stories"),ring:secSel("stories")},"secCfg.intensity":{stop:"full",wait:0,scroll:secSel("stories"),ring:secSel("stories")},
  "secCfg.nextUp":{stop:"full",wait:0,scroll:".nextup|"+secSel("about"),ring:".nextup|"+secSel("about")},
  "motionLevel":{stop:"full",wait:0,scroll:secSel("stories"),ring:secSel("stories")+" .sstart|"+secSel("stories")},
  "ctl.bar.minBlocks":{stop:"full",past:true,wait:0,ring:barRing},"ctl.bar.showAt":{stop:"half",past:true,wait:0,ring:barRing},
  "ctl.bar.showFrom":{stop:"full",wait:0,ring:function(){return barRing()==="#navbar"?"#navbar":".hero"}},
  "ctl.bar.more":{stop:"full",past:true,wait:0,ring:"#navbar [data-navmore]|#navbar"},
  "ctl.jump.opens":{stop:"half",past:true,wait:0,act:function(){var b=ph.querySelector('#navbar [data-navsec="media"]');if(b)b.click()},ringAfter:700,ring:secSel("media")},
  "ctl.pageChips":{view:"story",wait:0,ring:".pchips|.topbar"},
  "ctl.stickySlide":{stop:"half",wait:0,act:function(){scr.scrollTop=420},ringAfter:300,ring:".sticky|#navbar"},
  "ctl.term.opens":{view:"story",wait:0,scroll:".gterm",act:function(){var g=scr.querySelector(".gterm");if(g)g.click()},ringAfter:200,ring:".gpop|.gterm"},
  "ctl.term.underline":{view:"story",wait:0,scroll:".gterm",ring:".gterm"},
  "ctl.locate.style":{stop:"half",wait:0,ring:".locpin|.hero"},
  "ctl.viewer.counter":{view:"viewer",wait:0,ring:".vchips|.vtop span|.vtop"},
  "ctl.player.captions":{page:"Video",wait:0,ring:".vcap|.vplayer"}};
["indicator","labels","shape","fill","chosenIcon","chosenColour"].forEach(function(k){CTL_DEMOS["ctl.bar."+k]={stop:"full",past:true,wait:0,ring:barRing}});
["on","rows","ticks","chips"].forEach(function(k){CTL_DEMOS["ctl.contents."+k]={stop:"full",past:true,wait:0,act:function(){S.contentsOpen=true;renderPhone(true)},ringAfter:60,ring:{on:".csheet|#navbar",rows:".csheet",ticks:".csheet .ctick|.csheet",chips:".csheet .cchips|.csheet"}[k]}});
rowDemos(CTL_DEMOS);
/* each section's own Start and Shown: that section on the card (Hidden: the bar it left) */
rowDemos(function(key){var m=/^sections\.(\d+)\.(start|shown)$/.exec(key),sec=m&&S.sections[+m[1]];if(!sec)return;var s=secSel(sec.id);
  return{stop:"full",wait:0,scroll:s+"|.gsec",ring:m[2]==="start"?s+" .sstart|"+s:function(){return ph.querySelector(s)?s:barRing()}}});
