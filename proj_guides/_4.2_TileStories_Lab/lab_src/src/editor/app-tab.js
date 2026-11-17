/* The App tab (merge P1b.1; W's App tab, whose other sections come with W's screens in P4): the wall's picks of the theme
   (D.wall.theme over D.theme, core/theme.js). Look and mode: the wall's Default Mode, its accent (swatches: a closed list, each
   measured), its look. Depth and motion: the knobs a look also picks (roundness, density, glass, motion level). Free colours:
   developer-only rows over the table's own values (LAB ONLY: in the app a wall picks options, never a colour), each with its live
   contrast composed in linear light. Every row is one Undo step; its demo shows the part it changes.
   Unity: the POI Editor's App tab; a knob a list of the ThemeAsset's ThemeKnobOptions, the wall's pick a config.json field. */
var APP_OPT_LABEL={premium:"Premium",playful:"Playful",heritage:"Heritage",azulejo:"Azulejo",terracotta:"Terracotta",garden:"Garden",cobalt:"Cobalt",
  sharp:"Sharp",soft:"Soft",round:"Round",comfortable:"Comfortable",compact:"Compact",blur:"Blurred camera",solid:"Solid tint",follow:"Follow the phone",reduced:"Reduced"};
var APP_KNOB_HELP={
  look:"The app's look, every screen together. Premium: the theme as Unity has it. Playful: rounder type (Fredoka titles), text one step larger, a fine outline and hard shadows, its own warm colours, and it picks the Cobalt accent, Round corners and Solid glass. More looks come as options of the same list.",
  roundness:"The corners of blocks, pictures, buttons and sheets: three radius sets (Sharp 4 / 8 / 12 / 16, Soft 6 / 12 / 20 / 30, Round 8 / 16 / 24 / 32).",
  density:"Inner paddings and gaps. Compact moves each space one step down; the touch targets stay 48 pt.",
  glass:"The surface of the buttons and panels over the camera. Blurred camera: a light tint over the camera blurred behind it. Solid tint: a stronger tint with no blur (cheaper on older phones).",
  motion:"Follow the phone: the visitor's Reduce Motion decides. Reduced: every visitor gets whole, still parts (no reveals, no parallax, no loops)."};
/* the developer-only free colours: the table's own values a developer may try (the mode the phone shows), with the ground each is read on */
/* [token, label, the other colour of its pair, the bar, true when the token is the ground] */
var APP_FREE=[["--ts-ink","Ink (body text)","--ts-sheet-open",7],["--ts-ink-2","Secondary ink","--ts-sheet-open",7],["--ts-sheet-open","The card's ground","--ts-ink",7,true],
  ["--ts-surface","A block's surface","--ts-ink",7,true],["--ts-surface-2","Chips and wells","--ts-ink",7,true]];

/* the tab's button, after Guide (or Detail Card) */
(function(){var tabs=document.querySelector(".utabs"),c=tabs&&(tabs.querySelector('[data-tab="guide"]')||tabs.querySelector('[data-tab="card"]'));if(!c||tabs.querySelector('[data-tab="app"]'))return;
  var b=document.createElement("button");b.className="utab ap";b.setAttribute("role","tab");b.type="button";b.dataset.tab="app";b.setAttribute("aria-selected","false");b.textContent="App";c.after(b);
  b.addEventListener("click",function(){ui.tab="app";renderEditor()})})();
on("renderEditor.tab",function(){if(ui.tab==="app")return edApp()});

// One knob's row: its options as a segmented control; the look's own pick named when it overrides the wall's
function appKnobRow(k,label){var opts=themeOptions(k),look=themeOption("look",D.wall.theme.look),pick=k!=="look"&&look&&look.picks&&look.picks[k];
  var ctrl=pseg("D.wall.theme."+k,opts.map(function(o){return o.option}),opts.map(function(o){return APP_OPT_LABEL[o.option]||o.option}));
  if(pick)ctrl+='<div class="note">The '+esc(APP_OPT_LABEL[look.option])+' look picks '+esc(APP_OPT_LABEL[pick]||pick)+': this row applies with Premium.</div>';
  return(opts.some(function(o){return o.proposed})?prow:row)(label,ctrl,APP_KNOB_HELP[k])}
// The accent's swatches: each option's light and dark accent, drawn from the table
function appAccentRow(){var look=themeOption("look",D.wall.theme.look),pick=look&&look.picks&&look.picks.accent,cur=D.wall.theme.accent;
  var sw=function(o,mode){var t=(themeModeSet(o,mode).colours||[]).filter(function(x){return x.name==="--ts-accent"})[0];return '<i style="background:'+themeCss(t.value,"colours")+'"></i>'};
  var h='<div class="seg swatches">'+themeOptions("accent").map(function(o){return '<button type="button" data-set="@D.wall.theme.accent" data-v="'+o.option+'" aria-pressed="'+(cur===o.option)+'" title="Light and dark">'+
    sw(o,"light")+sw(o,"dark")+esc(APP_OPT_LABEL[o.option]||o.option)+(o.proposed?' <span class="prop">PROPOSED</span>':"")+'</button>'}).join("")+'</div>';
  if(pick)h+='<div class="note">The '+esc(APP_OPT_LABEL[look.option])+' look picks '+esc(APP_OPT_LABEL[pick])+': this row applies with Premium.</div>';
  return row("Accent",h,"The one accent colour of the app: links, the focal action, the Open Row, the active chip. A closed list (the theme's accent knob), each measured in light and dark: text 7:1 on every ground, its own words 7:1 on it.")}
// A free colour's contrast badge: its ratio on its ground, both read in the mode the phone shows, composed in linear light
function appBadge(f,v){var t=themeResolve(themeMode(),themePicks()),me=themeRgba(v||t[f[0]]),other=themeRgba(t[f[2]]),r=f[4]?themeContrast(other,me):themeContrast(me,other),ok=r>=f[3];
  return '<span class="badge '+(ok?"new":"warn")+'" data-tsbadge="'+f[0]+'">'+r.toFixed(1)+':1 '+(ok?"ok":"under "+f[3]+":1")+'</span>'}
// The free colours' rows (developer-only)
function appFreeRows(){var mode=themeMode(),set=themeModeSet(D.theme,mode).colours;
  return '<div class="note">Developer-only: the table\'s own values in '+mode+' mode, to try a colour. The app never offers a free colour (a wall picks the accent from its list), and the token test fails on any value that is not Unity\'s.</div>'+
    APP_FREE.map(function(f){var t=set.filter(function(x){return x.name===f[0]})[0],c=themeRgba(t.value),hex="#"+c.slice(0,3).map(function(n){return("0"+n.toString(16)).slice(-2)}).join("").toUpperCase();
      return row(f[1],'<div class="tscol"><input type="color" data-tscol="'+f[0]+'" value="'+hex+'" aria-label="'+esc(f[1])+'"><input class="u" type="text" data-tscol="'+f[0]+'" value="'+hex+'" aria-label="'+esc(f[1])+' hex"> '+appBadge(f)+'</div>',
        (f[4]?"Under "+f[2].replace("--ts-",""):"On "+f[2].replace("--ts-",""))+", in "+mode+" mode: "+f[3]+":1 or more, measured in linear light as Unity renders.")}).join("")}
// The tab
function edApp(){var h='<div class="note">The app\'s look: the theme Unity holds (Theme.default) and this wall\'s picks of it. Every screen, the card and the guide read these.</div>';
  h+=fo("ap:look",'<span class="acct">Look and mode</span> <span class="sub">· the Default Mode, the accent, the look</span>',["new","NEW"],
    row("Default mode",pseg("D.wall.theme.mode",["light","follow","dark"],["Light","Follow the phone","Dark"]),"The mode a visitor sees until they pick one in Settings (their pick wins on their phone). Light by default: the app is used outdoors, in sunlight first. Follow the phone: the phone's own light or dark appearance.")+
    appAccentRow()+appKnobRow("look","Look"),1);
  h+=fo("ap:depth",'<span class="acct">Depth and motion</span> <span class="sub">· corners, spacing, glass, motion</span>',["new","NEW"],
    appKnobRow("roundness","Roundness")+appKnobRow("density","Density")+appKnobRow("glass","Glass")+appKnobRow("motion","Motion level"),1);
  h+=fo("ap:free",'<span class="acct">Free colours</span> <span class="sub">· the table\'s values, with their contrast</span>',LAB_ONLY,appFreeRows(),1);
  return h}

/* the developer-only colours: typing or picking writes the table (one Undo step per edit), the badge follows at once */
ed.addEventListener("input",function(e){var t=e.target;if(!t.dataset||!t.dataset.tscol)return;var f=APP_FREE.filter(function(x){return x[0]===t.dataset.tscol})[0],c=themeRgba(t.value.trim());if(!f||!c)return;
  var b=ed.querySelector('[data-tsbadge="'+f[0]+'"]');if(b)b.outerHTML=appBadge(f,t.value.trim())});
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.tscol)return;var v=t.value.trim().toUpperCase();if(!themeRgba(v))return;
  var tk=themeModeSet(D.theme,themeMode()).colours.filter(function(x){return x.name===t.dataset.tscol})[0];if(!tk||tk.value===v)return;tk.value=v;applyTokens();renderAll(true)});

/* a pick: the table's classes swap at once (configChanged, core/theme.js); the motion level also makes the visitor's motion still */
on("setOpt.answer",function(key,v){if(key==="@D.wall.theme.motion"){D.wall.theme.motion=v;P.reduce=v==="reduced"||P.vis.motion==="reduced";renderAll(true);show("card");return true}});
on("undo.applied",function(kind,was){var w=(JSON.parse(was).wall||{}).theme||{};if(w.motion!==D.wall.theme.motion)P.reduce=D.wall.theme.motion==="reduced"||P.vis.motion==="reduced"});

/* each row's demo: the part it changes */
rowDemos({
  "D.wall.theme.mode":{stop:"full",wait:0,scroll:".gsec",ring:".gsec"},
  "D.wall.theme.accent":{block:"Story",wait:0,scroll:function(){return blockAt("Story").sel.split("|")[0]+" .open"},ring:function(){return blockAt("Story").sel.split("|")[0]+" .open"}},
  "D.wall.theme.look":{stop:"half",wait:0,ring:".hero|.gsec"},
  "D.wall.theme.roundness":{block:"Story",wait:0},
  "D.wall.theme.density":{view:"explore",stop:"closed",wait:0,ring:".xp-list"},
  "D.wall.theme.glass":{wall:true,wait:0,ring:".xp-open"},
  "D.wall.theme.motion":{stop:"full",wait:0,scroll:secSel("stories"),ring:secSel("stories")+" .sstart|"+secSel("stories")}});
