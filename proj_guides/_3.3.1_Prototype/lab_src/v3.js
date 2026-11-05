(function(){
var IMG={hero:"__HERO__",g1:"__G1__",g2:"__G2__",g3:"__G3__",before:"__BEFORE__"};
Object.keys(IMG).forEach(function(k){try{var b64=IMG[k].split(",")[1],bin=atob(b64),u=new Uint8Array(bin.length);for(var i=0;i<bin.length;i++)u[i]=bin.charCodeAt(i);IMG[k]=URL.createObjectURL(new Blob([u],{type:"image/jpeg"}))}catch(e){}});
var ASSETS=["hero","g1","g2","g3","before"];
var TILE='<svg viewBox="0 0 24 24" aria-hidden="true"><rect width="24" height="24" fill="currentColor" opacity=".16"/><path d="M0 0H7A7 7 0 0 1 0 7ZM24 0V7A7 7 0 0 1 17 0ZM24 24H17A7 7 0 0 1 24 17ZM0 24V17A7 7 0 0 1 7 24Z" fill="currentColor" opacity=".55"/><path d="M12 3.2C14.8 6.6 14.8 9.4 12 12C9.2 9.4 9.2 6.6 12 3.2ZM20.8 12C17.4 14.8 14.6 14.8 12 12C14.6 9.2 17.4 9.2 20.8 12ZM12 20.8C9.2 17.4 9.2 14.6 12 12C14.8 14.6 14.8 17.4 12 20.8ZM3.2 12C6.6 9.2 9.4 9.2 12 12C9.4 14.8 6.6 14.8 3.2 12Z" fill="currentColor"/></svg>';
var PIN='<svg viewBox="0 0 48 64" aria-hidden="true"><defs><linearGradient id="pg" x1="0" y1="0" x2=".35" y2="1"><stop offset="0" stop-color="#7fe3c4"/><stop offset="1" stop-color="#0e9a7c"/></linearGradient></defs><path d="M24 62C24 62 4 38.5 4 23.5C4 12.2 12.9 3 24 3s20 9.2 20 20.5C44 38.5 24 62 24 62Z" fill="url(#pg)" stroke="#0b7a62"/><circle cx="24" cy="23" r="11.5" fill="#fff"/><circle cx="24" cy="23" r="3.6" fill="#0e9a7c"/></svg>';
var ANIMSVG='<svg viewBox="0 0 48 48" aria-hidden="true"><rect x="4" y="4" width="40" height="40" rx="2" class="draw"/><path class="draw" d="M24 8C30 15 30 20 24 24C18 20 18 15 24 8ZM40 24C33 30 28 30 24 24C28 18 33 18 40 24ZM24 40C18 33 18 28 24 24C30 28 30 33 24 40ZM8 24C15 18 20 18 24 24C20 30 15 30 8 24Z"/><path class="fillin" fill="currentColor" d="M24 8C30 15 30 20 24 24C18 20 18 15 24 8ZM40 24C33 30 28 30 24 24C28 18 33 18 40 24ZM24 40C18 33 18 28 24 24C30 28 30 33 24 40ZM8 24C15 18 20 18 24 24C20 30 15 30 8 24Z"/></svg>';
function T(en,pt){return{en:en,pt:pt}}
var UI={about:T("About","Sobre"),stories:T("Stories","Histórias"),media:T("Media","Multimédia"),listen:T("Listen","Ouvir"),play:T("Play","Jogar"),visit:T("Visit","Visitar"),
 read:T("Read the story · 4 min","Ler a história · 4 min"),line:T("Royal palace, lost in 1755","Palácio real, perdido em 1755"),where:T("Lisbon · c. 1500 to 1755","Lisboa · c. 1500 a 1755"),
 home:T("Royal home for","Casa real durante"),homev:T("About 250 years","Cerca de 250 anos"),lost:T("Lost","Perdido"),lostv:T("1 November 1755","1 de novembro de 1755"),
 demo:T("Block Components demo","Demo dos Block Components"),open:T("Open","Abrir"),more:T("Read more","Ler mais")};

/* ---------- the element kinds (the registry) ---------- */
var KINDS=[
 {k:"title",n:"Title",c:"Text",ab:"Ti"},{k:"eyebrow",n:"Eyebrow",c:"Text",ab:"Ey"},{k:"heading",n:"Heading",c:"Text",ab:"He"},
 {k:"lede",n:"Lede",c:"Text",ab:"Le"},{k:"paragraph",n:"Paragraph",c:"Text",ab:"Pa"},{k:"callout",n:"Callout",c:"Text",ab:"Ca"},
 {k:"pull_quote",n:"Pull Quote",c:"Text",ab:"PQ"},{k:"list",n:"List",c:"Text",ab:"Li"},
 {k:"picture",n:"Picture",c:"Media",ab:"Pi"},{k:"picture_pair",n:"Picture Pair",c:"Media",ab:"PP"},{k:"carousel",n:"Carousel",c:"Media",ab:"Cr"},
 {k:"video",n:"Video",c:"Media",ab:"Vi"},{k:"audio",n:"Audio",c:"Media",ab:"Au"},{k:"animation",n:"Animation",c:"Media",ab:"An"},
 {k:"before_after",n:"Before / After",c:"Interactive",ab:"BA"},{k:"hotspot",n:"Hotspot Picture",c:"Interactive",ab:"Hs"},{k:"model_3d",n:"3D Model",c:"Interactive",ab:"3D"},
 {k:"panorama",n:"360 Panorama",c:"Interactive",ab:"36"},{k:"scrolly",n:"Scroll Story",c:"Interactive",ab:"SS"},
 {k:"map",n:"Map",c:"Data",ab:"Ma"},{k:"timeline",n:"Timeline",c:"Data",ab:"TL"},{k:"swatches",n:"Swatch Row",c:"Data",ab:"Sw"},
 {k:"divider",n:"Divider",c:"Structure",ab:"Di"},{k:"spacer",n:"Spacer",c:"Structure",ab:"Sp"}];
var KIND={};KINDS.forEach(function(x){KIND[x.k]=x});
var CATS=["Text","Media","Interactive","Data","Structure"];
var MEDIA={picture:1,picture_pair:1,carousel:1,video:1};
var OPTS={title:["tone","size","align"],eyebrow:["tone","align"],heading:["tone","size","rule"],lede:["size","align"],paragraph:["dropcap"],
 callout:["tone","size","calltap"],pull_quote:["variant"],list:["marker"],picture:["width","frame","pictap"],picture_pair:["width"],carousel:["layout"],video:["width"],
 audio:["alook"],animation:["source","trigger"],before_after:["start"],hotspot:["pulse"],model_3d:["spin"],panorama:["gyro"],scrolly:[],map:["mlook"],timeline:["tdot"],swatches:["swsize"],
 divider:["uses"],spacer:["space"]};
var OPTDEF={
 tone:{l:"Tone",v:["neutral","family","accent"],vl:["Neutral","Family colour","Accent"],h:"Family colour = the colour of the bar group the block sits in. Every tone keeps text at 7:1 (a fixed rule)."},
 size:{l:"Size",v:["s","m","l"],vl:["Small","Medium","Large"],h:"A step on the theme's type scale."},
 align:{l:"Alignment",v:["start","centre"],vl:["Start","Centre"],h:"Start for reading; Centre for short display words."},
 rule:{l:"Rule Above",v:["off","on"],vl:["Off","On"],h:"A short line in the family colour above the heading."},
 dropcap:{l:"Drop Cap",v:["off","on"],vl:["Off","On"],h:"The first letter large, in the family colour."},
 calltap:{l:"Interaction",v:["none","flip"],vl:["None","Tap to Flip"],h:"Tap to Flip shows a second side (the Back text)."},
 variant:{l:"Look",v:["serif","minimal"],vl:["Serif Large","Minimal"],h:"Serif Large: a big quotation mark. Minimal: a bar and italic words."},
 marker:{l:"Markers",v:["bullets","numbers"],vl:["Bullets","Numbers"],h:"Numbers when the order is information."},
 width:{l:"Width",v:["inset","bleed"],vl:["Inset","Full Bleed"],h:"Inset keeps the text margin; Full Bleed runs edge to edge."},
 frame:{l:"Frame",v:["rect","arch"],vl:["Rectangle","Arch"],h:"Arch is the window shape of the palace's arcades."},
 pictap:{l:"Interaction",v:["none","hold","tilt"],vl:["None","Hold to Reveal","Tilt to Look"],h:"Hold to Reveal hides the picture until a press; Tilt to Look moves it with the phone."},
 layout:{l:"Layout",v:["peek","one"],vl:["Peek Strip","One at a time"],h:"Peek Strip: a swipeable strip, the next picture peeking. One at a time: a full frame that turns with the Transition."},
 alook:{l:"Look",v:["bar","chip"],vl:["Player Bar","Chip"],h:"A full player bar, or a small chip."},
 source:{l:"Source",v:["lottie","rive","video","unity"],vl:["Lottie","Rive","Video + alpha","Unity Animation"],h:"Where the animation is made. See In Unity below."},
 trigger:{l:"Plays",v:["enter","loop","tap","scrub"],vl:["On view","Loop","On tap","With scroll"],h:"With scroll scrubs the animation with the visitor's scroll."},
 start:{l:"Start at",v:["25","50","75"],vl:["25%","50%","75%"],h:"Where the slider rests before the first drag."},
 pulse:{l:"Pins",v:["pulse","still"],vl:["Pulse","Still"],h:"Pulsing pins invite a tap."},
 spin:{l:"Auto Spin",v:["on","off"],vl:["On","Off"],h:"Turns slowly until the visitor drags (off under Reduce Motion)."},
 gyro:{l:"Gyro",v:["on","off"],vl:["On","Off"],h:"Turning the phone looks around."},
 mlook:{l:"Look",v:["paper","dark"],vl:["Paper","Night"],h:"The map's colours."},
 tdot:{l:"Markers",v:["dots","years"],vl:["Dots","Years only"],h:"How each date is marked."},
 swsize:{l:"Size",v:["s","m"],vl:["Small","Medium"],h:"The chips' height."},
 uses:{l:"Uses effect",v:[],vl:[],h:"A divider draws one of the Effects (an edge or a line divider)."},
 space:{l:"Room",v:["s","m","l"],vl:["Small","Medium","Large"],h:"16 / 24 / 40 pt."}};

/* ---------- motion ---------- */
var TYPES={entrance:"Entrance",transition:"Transition",scroll:"With the scroll",emphasis:"Emphasis",imported:"Imported animation"};
var PATH={rise:"nat",slide:"nat",pop:"nat",fade:"nat",tilt:"nat",wipe:"nat",words:"nat",letters:"nat",t_slide:"nat",t_cube:"bld",t_deck:"nat",t_flip:"bld",t_fade:"nat",t_zoom:"nat",
 parallax:"nat",zoom:"nat",scalein:"nat",shine:"nat",pulse:"nat",float:"nat",im_tile:"imp",im_ship:"imp",im_dust:"imp",im_door:"nat"};
var uid=200;function nid(p){return p+(++uid)}
var S={tab:"card",step:1,dark:false,reduce:false,lang:"en",screen:"phone",focusOn:true,view:"demo",focusRef:null,family:"rise",replay:"every",
 test:{on:true,poi:"painting"},ui:{single:false,level:1,q:"",cat:"All"},op:{s3:false,s4:false,s5:false,s6:false,s2:true},
 motions:[
  {id:"rise",name:"Rise",type:"entrance",from:"below",dist:18,dur:420,ease:"out",stagger:70,pieces:"whole",rot:0},
  {id:"slide",name:"Slide In",type:"entrance",from:"left",dist:40,dur:450,ease:"out",stagger:70,pieces:"whole",rot:0},
  {id:"pop",name:"Pop",type:"entrance",from:"scale",dist:0,dur:420,ease:"overshoot",stagger:50,pieces:"whole",rot:0},
  {id:"fade",name:"Fade",type:"entrance",from:"none",dist:0,dur:500,ease:"out",stagger:0,pieces:"whole",rot:0},
  {id:"tilt",name:"Tilt In",type:"entrance",from:"below",dist:24,dur:520,ease:"spring",stagger:80,pieces:"whole",rot:-4},
  {id:"wipe",name:"Wipe",type:"entrance",from:"wipe",dist:0,dur:700,ease:"out",stagger:120,pieces:"whole",rot:0},
  {id:"words",name:"Build by Words",type:"entrance",from:"below",dist:10,dur:380,ease:"out",stagger:45,pieces:"words",rot:0},
  {id:"letters",name:"Build by Letters",type:"entrance",from:"below",dist:8,dur:300,ease:"overshoot",stagger:18,pieces:"letters",rot:0},
  {id:"t_slide",name:"Swipe",type:"transition",style:"slide",dur:450,ease:"out"},{id:"t_cube",name:"Cube",type:"transition",style:"cube",dur:650,ease:"inout"},
  {id:"t_deck",name:"Card Deck",type:"transition",style:"deck",dur:600,ease:"out"},{id:"t_flip",name:"Flip",type:"transition",style:"flip",dur:650,ease:"inout"},
  {id:"t_fade",name:"Cross-fade",type:"transition",style:"fade",dur:500,ease:"inout"},{id:"t_zoom",name:"Zoom Through",type:"transition",style:"zoom",dur:550,ease:"out"},
  {id:"parallax",name:"Parallax",type:"scroll",style:"parallax",factor:0.18},{id:"zoom",name:"Slow Zoom",type:"scroll",style:"zoom",factor:0.08},{id:"scalein",name:"Grow In",type:"scroll",style:"scalein",factor:0.12},
  {id:"shine",name:"Shine",type:"emphasis",style:"shine",dur:2400},{id:"pulse",name:"Pulse",type:"emphasis",style:"pulse",dur:1800},{id:"float",name:"Float",type:"emphasis",style:"float",dur:2600},
  {id:"im_tile",name:"Azulejo Draws Itself",type:"imported",source:"lottie",trigger:"enter",file:"azulejo_draw.json"},
  {id:"im_ship",name:"Caravel Sails In",type:"imported",source:"rive",trigger:"scrub",file:"caravel.riv"},
  {id:"im_dust",name:"Gold Dust",type:"imported",source:"video",trigger:"loop",file:"gold_dust.webm"},
  {id:"im_door",name:"Palace Door Opens",type:"imported",source:"unity",trigger:"tap",file:"PalaceDoor.prefab"}],
 inter:[
  {id:"flip",name:"Tap to Flip",o:{dur:600},h:"A card turns to show its second side."},
  {id:"hold",name:"Hold to Reveal",o:{hold:900},h:"Press and hold: a ring fills, then the picture appears (a small game of patience)."},
  {id:"compare",name:"Drag to Compare",o:{start:50},h:"A slider uncovers one picture over another."},
  {id:"deck",name:"Swipe Deck",o:{throwd:120},h:"Throw the top card aside to see the next."},
  {id:"scrub",name:"Scroll Scrub",o:{steps:3},h:"The picture stays pinned while words scroll past it and change it (scrollytelling)."},
  {id:"tiltlook",name:"Tilt to Look",o:{strength:12},h:"The picture moves with the phone's tilt (here: with the pointer)."},
  {id:"choice",name:"Choice Point",o:{},h:"The visitor picks which thread to follow."},
  {id:"hotspots",name:"Tap Hotspots",o:{},h:"Pins on a picture open short notes."}],
 effects:[
  {id:"e_wave",grp:"edge",name:"Wave",o:{h:30,colour:"family"}},{id:"e_multiwave",grp:"edge",name:"Multi Wave",o:{h:40,colour:"family"}},
  {id:"e_torn",grp:"edge",name:"Torn Paper",o:{h:22,colour:"family"}},{id:"e_arches",grp:"edge",name:"Arcade Arches",o:{h:34,colour:"family"}},
  {id:"e_skyline",grp:"edge",name:"Lisbon Skyline",o:{h:46,colour:"family"}},{id:"e_diamonds",grp:"edge",name:"Diamond Frame",o:{h:30,colour:"family"}},
  {id:"e_calcada",grp:"edge",name:"Calçada Waves",o:{h:30,colour:"family"}},{id:"e_tiles",grp:"edge",name:"Tile Row",o:{h:24,colour:"family"}},{id:"e_mosaic",grp:"edge",name:"Mosaic Strip",o:{h:28,colour:"family"}},
  {id:"l_hair",grp:"line",name:"Hairline",o:{colour:"neutral"}},{id:"l_tile",grp:"line",name:"Tile Ornament",o:{colour:"family"}},{id:"l_fleur",grp:"line",name:"Fleur",o:{colour:"family"}},
  {id:"l_diamond",grp:"line",name:"Diamond",o:{colour:"family"}},{id:"l_scroll",grp:"line",name:"Scroll Ends",o:{colour:"family"}},{id:"l_dots",grp:"line",name:"Dots and Star",o:{colour:"family"}},
  {id:"x_fade",grp:"light",name:"Fade",o:{h:60}},{id:"x_tone",grp:"light",name:"Tone",o:{spread:1}},
  {id:"x_reveal",grp:"moving",name:"Reveal",o:{}},{id:"x_drift",grp:"moving",name:"Drift",o:{speed:10}}],
 styles:{
  title:[{id:"st1",name:"Display",o:{tone:"neutral",size:"m",align:"start"},enter:"family",def:true},{id:"st2",name:"Display Centred",o:{tone:"neutral",size:"l",align:"centre"},enter:"letters"}],
  eyebrow:[{id:"st3",name:"Tracked Caps",o:{tone:"family",align:"start"},enter:"family",def:true},{id:"st4",name:"Accent Caps",o:{tone:"accent",align:"start"},enter:"wipe"}],
  heading:[{id:"st5",name:"Section",o:{tone:"neutral",size:"m",rule:"off"},enter:"family",def:true},{id:"st6",name:"Chapter",o:{tone:"neutral",size:"l",rule:"on"},enter:"words"}],
  lede:[{id:"st7",name:"Lede",o:{size:"m",align:"start"},enter:"family",def:true}],
  paragraph:[{id:"st8",name:"Body",o:{dropcap:"off"},enter:"family",def:true},{id:"st9",name:"Opening",o:{dropcap:"on"},enter:"fade"}],
  callout:[{id:"st10",name:"Family Box",o:{tone:"family",size:"m",calltap:"none"},enter:"pop",def:true},{id:"st11",name:"Quiet Note",o:{tone:"neutral",size:"s",calltap:"none"},enter:"family"},{id:"st28",name:"Flip Card",o:{tone:"family",size:"m",calltap:"flip"},enter:"pop"}],
  pull_quote:[{id:"st12",name:"Serif Large",o:{variant:"serif"},enter:"words",def:true},{id:"st13",name:"Minimal",o:{variant:"minimal"},enter:"slide"}],
  list:[{id:"st14",name:"Bullets",o:{marker:"bullets"},enter:"family",def:true},{id:"st15",name:"Numbered",o:{marker:"numbers"},enter:"tilt"}],
  picture:[{id:"st16",name:"Inset",o:{width:"inset",frame:"rect",pictap:"none"},enter:"family",scroll:"none",def:true},{id:"st17",name:"Full Bleed",o:{width:"bleed",frame:"rect",pictap:"none"},enter:"fade",scroll:"parallax"},
   {id:"st18",name:"Arch Window",o:{width:"inset",frame:"arch",pictap:"none"},enter:"pop",scroll:"zoom"},{id:"st29",name:"Hidden Treasure",o:{width:"inset",frame:"rect",pictap:"hold"},enter:"fade",scroll:"none"},{id:"st30",name:"Window Tilt",o:{width:"inset",frame:"rect",pictap:"tilt"},enter:"family",scroll:"none"}],
  picture_pair:[{id:"st19",name:"Side by Side",o:{width:"inset"},enter:"family",scroll:"none",def:true}],
  carousel:[{id:"st20",name:"Peek Strip",o:{layout:"peek"},enter:"slide",def:true},{id:"st27",name:"Story Cube",o:{layout:"one"},enter:"fade",turn:"t_cube"},{id:"st31",name:"Card Deck",o:{layout:"one"},enter:"pop",turn:"t_deck"}],
  video:[{id:"st21",name:"Teaser",o:{width:"inset"},enter:"family",scroll:"none",def:true},{id:"st22",name:"Cinema",o:{width:"bleed"},enter:"fade",scroll:"scalein"}],
  audio:[{id:"st32",name:"Player Bar",o:{alook:"bar"},enter:"family",def:true}],
  animation:[{id:"st33",name:"Drawn Tile",o:{source:"lottie",trigger:"enter"},enter:"fade",def:true},{id:"st34",name:"Scrubbed",o:{source:"rive",trigger:"scrub"},enter:"none"}],
  before_after:[{id:"st35",name:"Slider",o:{start:"50"},enter:"family",def:true}],
  hotspot:[{id:"st36",name:"Pulsing Pins",o:{pulse:"pulse"},enter:"family",def:true}],
  model_3d:[{id:"st37",name:"Turntable",o:{spin:"on"},enter:"pop",def:true}],
  panorama:[{id:"st38",name:"Look Around",o:{gyro:"on"},enter:"fade",def:true}],
  scrolly:[{id:"st39",name:"Pinned Picture",o:{},enter:"none",def:true}],
  map:[{id:"st40",name:"Paper Map",o:{mlook:"paper"},enter:"family",def:true}],
  timeline:[{id:"st41",name:"Dots",o:{tdot:"dots"},enter:"tilt",def:true}],
  swatches:[{id:"st42",name:"Tile Colours",o:{swsize:"m"},enter:"words",def:true}],
  divider:[{id:"st23",name:"Hairline",o:{uses:"l_hair"},enter:"wipe",def:true},{id:"st24",name:"Tile Ornament",o:{uses:"l_tile"},enter:"pop"},{id:"st43",name:"Fleur",o:{uses:"l_fleur"},enter:"wipe"},{id:"st44",name:"Arcade Edge",o:{uses:"e_arches"},enter:"fade"},{id:"st45",name:"Skyline Edge",o:{uses:"e_skyline"},enter:"wipe"}],
  spacer:[{id:"st25",name:"Medium",o:{space:"m"},enter:"none",def:true},{id:"st26",name:"Large",o:{space:"l"},enter:"none"}]},
 patterns:[
  {id:"pt1",name:"Chapter Opener",cat:"Text",rows:[{kind:"picture",style:"st17"},{kind:"eyebrow",style:"st3"},{kind:"title",style:"st1"},{kind:"lede",style:"st7"}]},
  {id:"pt2",name:"Quote Moment",cat:"Text",rows:[{kind:"divider",style:"st24"},{kind:"pull_quote",style:"st12"},{kind:"divider",style:"st23"}]},
  {id:"pt3",name:"Fact Box",cat:"Text",rows:[{kind:"heading",style:"st5"},{kind:"callout",style:"st10"}]},
  {id:"pt6",name:"Fun Fact",cat:"Interactive",rows:[{kind:"callout",style:"st28"}]},
  {id:"pt4",name:"Person",cat:"Text",rows:[{kind:"picture",style:"st18"},{kind:"heading",style:"st5"},{kind:"eyebrow",style:"st4"},{kind:"paragraph",style:"st8"}]},
  {id:"pt5",name:"Steps",cat:"Text",rows:[{kind:"heading",style:"st5"},{kind:"list",style:"st15"}]},
  {id:"pt7",name:"Then and Now",cat:"Interactive",rows:[{kind:"heading",style:"st6"},{kind:"before_after",style:"st35"},{kind:"paragraph",style:"st8"}]},
  {id:"pt8",name:"Scrollytelling",cat:"Interactive",rows:[{kind:"scrolly",style:"st39"}]}],
 sel:{layer:"motion",kind:"callout",style:"st10",motion:"rise",inter:"flip",effect:"e_arches",pattern:"pt1",block:"Story",row:"r1",cblock:"b_story",picker:null}
};
var LIB=[
 ["About","Header","keep","The hero: its own owner (motion, words on a picture).","hero picture, eyebrow, title, line, Tone"],
 ["About","Inline","new","NEW: elements and patterns placed straight on the card, no page. Takes over Rich Text, Pull Quote, Process Steps, Fun Fact, Person.","any elements / patterns"],
 ["About","Story","keep","Preview + its own page (chapters as an option, from Story Chapters).","a page of elements / patterns"],
 ["About","Quick Facts","keep","Structured pairs the hero and search read.","label / value pairs"],
 ["About","Status","keep","Data (open, closed, under works).","badge + line"],
 ["About","Actions","keep","Buttons with behaviour (the sticky action).","buttons"],
 ["Stories","Timeline","keep","Dated rows with their own order rule.","Timeline element"],
 ["Stories","Compare Points","keep","Reads another point's card.","Picture Pair + data"],
 ["Stories","Dialogue","keep","Turn-by-turn lines with taps.","lines + taps"],
 ["Media","Gallery","keep","The picture viewer (zoom included, from Zoom Image).","Carousel + viewer"],
 ["Media","Before and After","keep","The compare slider.","Before / After element"],
 ["Media","Hotspot Image","keep","Spots that open blocks.","Hotspot Picture element"],
 ["Media","Video","keep","The one video owner.","Video element"],
 ["Media","3D Model","keep","A turntable stage.","3D Model element"],
 ["Media","360 Panorama","keep","Drag and gyro.","360 element"],
 ["Guides","Audio Guide","keep","The one audio owner.","Audio element"],
 ["AR","Place In AR","keep","Places a model in the world.","button + AR"],
 ["Play","Knowledge Check","keep","Questions and answers.","questions"],["Play","Poll","keep","Votes.","options + bars"],["Play","Collect","keep","Stamps across points.","stamp"],
 ["Community","Feedback","keep","Thumbs / stars.","buttons"],
 ["Visit","Wall Locator","keep","Where the point sits on the wall.","Map element"],["Visit","Today Map","keep","Directions.","Map element"],["Visit","Related","keep","Other points.","cards"],["Visit","Practical Info","keep","Hours, prices: data.","label / value pairs"],
 ["Meta","Sources","keep","Credits and licences.","list"],["Navigation","Contents","keep","Rebuild planned on the bar's pattern.","list"]];
var GONE="Became patterns: Rich Text, Pull Quote, Process Steps, Fun Fact, Person. Merged: Story Chapters into Story, Zoom Image into Gallery, Show On Wall into the Locate Button, Swatches into an element. (Developer, 2026-10-31.)";
/* the card of the marker: blocks in order with their preview settings */
S.card=[
 {id:"b_facts",kind:"Quick Facts",grp:"about",enter:"family",tap:"nothing",orow:"plain"},
 {id:"b_fun",kind:"Inline",grp:"about",pattern:"pt6",enter:"pop",tap:"nothing",orow:"plain"},
 {id:"b_story",kind:"Story",grp:"stories",enter:"family",tap:"page",orow:"nudge",preview:"auto"},
 {id:"b_ba",kind:"Before and After",grp:"media",enter:"fade",tap:"nothing",orow:"plain"},
 {id:"b_gal",kind:"Gallery",grp:"media",enter:"slide",tap:"page",orow:"plain"},
 {id:"b_audio",kind:"Audio Guide",grp:"listen",enter:"family",tap:"nothing",orow:"plain"},
 {id:"b_quiz",kind:"Knowledge Check",grp:"play",enter:"tilt",tap:"nothing",orow:"plain"},
 {id:"b_info",kind:"Practical Info",grp:"visit",enter:"family",tap:"expand",orow:"plain"}];
var GROUPS=["about","stories","media","listen","play","visit"];
/* the story page (place: content only) */
S.story=[
 {id:"r1",pattern:"pt1",slots:[{img:"g3",cap:T("The Terreiro do Paço in 1662, the palace along the river front.","O Terreiro do Paço em 1662, o palácio ao longo da frente ribeirinha."),credit:"Dirk Stoop"},{text:T("Lisbon · c. 1500 to 1755","Lisboa · c. 1500 a 1755")},{text:T("Two hundred and fifty years by the river","Duzentos e cinquenta anos à beira-rio")},{text:T("For about 250 years the kings of Portugal lived by the river, in a palace that opened straight onto the Tagus.","Durante cerca de 250 anos, os reis de Portugal viveram à beira do rio, num palácio aberto diretamente para o Tejo.")}]},
 {id:"r2",kind:"paragraph",style:"st9",c:{text:T("When King Manuel I found the old castle on the hill too cramped, he moved the court down to the docks around 1500 and filled the new palace with the wealth of the voyages to India.","Quando D. Manuel I achou o velho castelo da colina demasiado apertado, mudou a corte para junto das docas por volta de 1500 e encheu o novo palácio com a riqueza das viagens à Índia.")}},
 {id:"r3",kind:"heading",style:null,c:{text:T("Two centuries of additions","Dois séculos de acrescentos")}},
 {id:"r4",kind:"paragraph",style:null,c:{text:T("Later kings kept adding to it. In the 1580s, under Philip I, who ruled Portugal and Spain together, the river front received a new wing and its corner tower, the Torreão.","Os reis seguintes foram acrescentando obra. Na década de 1580, com Filipe I, que reinava em Portugal e em Espanha, a frente ribeirinha ganhou uma nova ala e o seu torreão de canto, o Torreão.")}},
 {id:"r5",pattern:"pt3",slots:[{text:T("What was inside","O que havia lá dentro")},{title:T("A royal city","Uma cidade real"),text:T("A library of some 70,000 books, paintings and maps, the royal archive, a chapel, offices and workshops and, next door, the new opera house.","Uma biblioteca com cerca de 70 000 livros, pinturas e mapas, o arquivo real, uma capela, gabinetes e oficinas e, ao lado, a nova ópera.")}]},
 {id:"r6",pattern:"pt7",slots:[{text:T("Then and now","Antes e agora")},{before:"before",after:"g2"},{text:T("Drag across: the palace before 1755, and the square that took its place.","Arraste: o palácio antes de 1755 e a praça que ficou no seu lugar.")}]},
 {id:"r7",pattern:"pt2",slots:[{},{text:T("Where the palace had stood, an open square was laid out to the river.","Onde estivera o palácio, abriu-se uma praça para o rio."),cite:"The Pombaline plan, 1758"},{}]},
 {id:"r8",kind:"heading",style:"st6",c:{text:T("One morning in November","Uma manhã de novembro")}},
 {id:"r9",kind:"paragraph",style:null,c:{text:T("On the morning of 1 November 1755, All Saints' Day, an earthquake, a fire and a tsunami destroyed the palace, its library and the opera house in a few hours.","Na manhã de 1 de novembro de 1755, Dia de Todos os Santos, um terramoto, um incêndio e um maremoto destruíram em poucas horas o palácio, a biblioteca e a ópera.")}},
 {id:"r10",kind:"carousel",style:"st27",c:{imgs:["g1","g2","g3"],cap:T("The square through three centuries.","A praça ao longo de três séculos.")}},
 {id:"r11",kind:"video",style:null,c:{img:"hero",cap:T("From the square to the castle · 0:55","Da praça ao castelo · 0:55")}}];
var DEMO={title:T("Two hundred and fifty years by the river","Duzentos e cinquenta anos à beira-rio"),eyebrow:T("Lisbon · c. 1500 to 1755","Lisboa · c. 1500 a 1755"),heading:T("Two centuries of additions","Dois séculos de acrescentos"),
 lede:T("For about 250 years the kings of Portugal lived by the river, in a palace that opened straight onto the Tagus.","Durante cerca de 250 anos, os reis de Portugal viveram à beira do rio, num palácio aberto diretamente para o Tejo."),
 paragraph:T("When King Manuel I found the old castle on the hill too cramped, he moved the court down to the docks and filled the new palace with the wealth of the voyages.","Quando D. Manuel I achou o velho castelo no alto da colina demasiado apertado, mudou a corte para junto das docas e encheu o novo palácio com a riqueza das viagens."),
 ctitle:T("What was inside","O que havia lá dentro"),ctext:T("A library of some 70,000 books, the royal archive, a chapel and, next door, the new opera house.","Uma biblioteca com cerca de 70 000 livros, o arquivo real, uma capela e, ao lado, a nova ópera."),
 back:T("The opera house opened in March 1755 and stood for only seven months.","A ópera abriu em março de 1755 e durou apenas sete meses."),
 quote:T("Where the palace had stood, an open square was laid out to the river.","Onde estivera o palácio, abriu-se uma praça para o rio."),cite:"The Pombaline plan, 1758",
 list:{en:["The royal library","The chapel","The opera house, opened in 1755"],pt:["A biblioteca real","A capela","A ópera, inaugurada em 1755"]},
 piccap:T("The Terreiro do Paço in 1662.","O Terreiro do Paço em 1662."),paircap:T("Before 1755 and today.","Antes de 1755 e hoje."),vidcap:T("From the square to the castle · 0:55","Da praça ao castelo · 0:55")};
var POIS=[["lamp","1. St George's Castle"],["cathedral","2. Lisbon Cathedral"],["painting","7. Ribeira Palace"],["camera","13. The Camera"]];
/* content fields per kind (Specific Marker > a page) */
var FIELDS={title:[["text","ltext","Text"]],eyebrow:[["text","ltext","Text"]],heading:[["text","ltext","Text"]],lede:[["text","ltext","Text"]],paragraph:[["text","ltext","Text"]],
 callout:[["title","ltext","Title"],["text","ltext","Text"],["back","ltext","Back (Flip)"]],pull_quote:[["text","ltext","Quote"],["cite","text","Attribution"]],list:[["items","litems","Items (one per line)"]],
 picture:[["img","img","Picture"],["cap","ltext","Caption"],["credit","text","Credit"]],picture_pair:[["imgs","imgs","Pictures (two)"],["cap","ltext","Caption"]],carousel:[["imgs","imgs","Pictures"],["cap","ltext","Caption"]],
 video:[["img","img","Poster (the clip's frame)"],["cap","ltext","Caption"]],audio:[["cap","ltext","Title"]],animation:[["file","text","File"]],before_after:[["before","img","Before"],["after","img","After"]],
 hotspot:[["img","img","Picture"]],model_3d:[["img","img","Model (stand-in picture)"]],panorama:[["img","img","Panorama"]],scrolly:[],map:[],timeline:[],swatches:[],divider:[],spacer:[]};

/* ---------- helpers ---------- */
function L(o,f){var ans=firstAnswer("L.answer",o,f);return ans!==undefined?ans:Lbase(o,f)}
/* the words in the visitor's language (then the fallback language), before any audience's own words */
function Lbase(o,f){if(o==null)return f==null?"":L(f);if(typeof o==="string")return o;return o[S.lang]||(S.langs&&o[S.langs.fallback])||o.en||o.pt||""}
function esc(s){return String(s==null?"":s).replace(/[&<>"]/g,function(c){return{"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;"}[c]})}
function styleById(id){for(var k in S.styles){var a=S.styles[k];for(var i=0;i<a.length;i++)if(a[i].id===id)return a[i]}return null}
function kindOfStyle(id){for(var k in S.styles){if(S.styles[k].some(function(s){return s.id===id}))return k}return null}
function defStyle(kind){var a=S.styles[kind]||[];for(var i=0;i<a.length;i++)if(a[i].def)return a[i];return a[0]}
function byId(a,id){for(var i=0;i<a.length;i++)if(a[i].id===id)return a[i];return null}
function motionById(id){return byId(S.motions,id)} function effectById(id){return byId(S.effects,id)} function patternById(id){return byId(S.patterns,id)} function interById(id){return byId(S.inter,id)}
function styleUses(id){var n=0;S.patterns.forEach(function(p){p.rows.forEach(function(r){if(r.style===id)n++})});S.story.forEach(function(r){if(!r.pattern&&(r.style===id||(!r.style&&defStyle(r.kind).id===id)))n++});return n}
function motionUses(id){var n=0;for(var k in S.styles)S.styles[k].forEach(function(s){if(s.enter===id||s.scroll===id||s.turn===id)n++});S.card.forEach(function(b){if(b.enter===id)n++});if(S.family===id)n++;return pipe("motionUses.count",n,id)}
function effectUses(id){var n=0;S.styles.divider.forEach(function(s){if(s.o.uses===id)n++});return n}
function patternUses(id){return S.story.filter(function(r){return r.pattern===id}).length+S.card.filter(function(b){return b.pattern===id}).length}
function entranceOf(style){var m=style.enter==="family"?S.family:style.enter;return m==="none"?null:motionById(m)}
function easeCss(e){return e==="overshoot"?"cubic-bezier(.3,1.45,.5,1)":e==="spring"?"cubic-bezier(.2,1.3,.4,1)":e==="inout"?"cubic-bezier(.65,0,.35,1)":"cubic-bezier(.2,.8,.2,1)"}
function motionVars(m,i){if(!m||m.type!=="entrance")return "";var dx=0,dy=0,sc=1;
  if(m.from==="below")dy=m.dist;else if(m.from==="left")dx=-m.dist;else if(m.from==="right")dx=m.dist;else if(m.from==="scale")sc=.84;
  return "--dx:"+dx+"px;--dy:"+dy+"px;--sc:"+sc+";--rt:"+(m.rot||0)+"deg;--dur:"+m.dur+"ms;--ease:"+easeCss(m.ease)+";--delay:"+((i||0)*m.stagger)+"ms";}
function wrapMv(html,m,i){if(!m||m.type!=="entrance")return html;
  if(m.pieces!=="whole")return '<div class="mvp" data-pieces="'+m.id+'">'+html+'</div>';
  return '<div class="mv pre'+(m.from==="wipe"?" wipe":"")+'" style="'+motionVars(m,i)+'">'+html+'</div>';}
function colourVar(c){return c==="accent"?"var(--accent2)":c==="neutral"?"var(--ink2)":"var(--fam)"}
function mtile(c){return TILE}

/* ---------- effects: edges and line dividers ---------- */
function edgeSvg(f){var ans=firstAnswer("edgeSvg.answer",f);if(ans!==undefined)return ans;var o=f.o,h=o.h||30,col=colourVar(o.colour),W=390,p="";
  if(f.id==="e_wave")p='<path d="M0 '+h+'V'+h*.55+'C48 '+h*.2+' 98 '+h*.15+' 150 '+h*.45+'S262 '+h*.9+' 318 '+h*.6+'S372 '+h*.25+' 390 '+h*.35+'V'+h+'Z" fill="currentColor" opacity=".18"/>';
  if(f.id==="e_multiwave"){[[.18,.3],[.12,.5],[.25,.7]].forEach(function(a,i){var y=h*a[1];p+='<path d="M0 '+h+'V'+y+'C80 '+(y-h*.25)+' 140 '+(y+h*.2)+' 210 '+y+'S330 '+(y-h*.3)+' 390 '+(y+h*.05)+'V'+h+'Z" fill="currentColor" opacity="'+a[0]+'"/>'})}
  if(f.id==="e_torn"){var d="M0 "+h,x=0,seed=7;while(x<=W){seed=(seed*9301+49297)%233280;var y=h*.25+(seed/233280)*h*.5;d+=" L"+x+" "+y.toFixed(1);x+=6+(seed%7)}d+=" L390 "+h+"Z";p='<path d="'+d+'" fill="currentColor" opacity=".22"/>'}
  if(f.id==="e_arches"){var w=39;for(var i=0;i<10;i++){var x0=i*w;p+='<path d="M'+x0+' '+h+'V'+(h*.45)+'A'+(w/2)+' '+(w/2)+' 0 0 1 '+(x0+w)+' '+(h*.45)+'V'+h+'Z" fill="currentColor" opacity=".2"/><path d="M'+(x0+4)+' '+h+'V'+(h*.5)+'A'+(w/2-4)+' '+(w/2-4)+' 0 0 1 '+(x0+w-4)+' '+(h*.5)+'V'+h+'" fill="none" stroke="currentColor" stroke-opacity=".5"/>'}}
  if(f.id==="e_skyline"){var b=[[0,.55],[22,.4],[40,.62],[64,.35],[78,.5],[100,.3],[112,.45],[140,.58],[160,.42],[176,.2],[188,.42],[212,.5],[236,.36],[252,.6],[276,.46],[300,.28],[316,.5],[340,.4],[364,.56],[390,.5]],d2="M0 "+h;
    b.forEach(function(q,i){var nx=i<b.length-1?b[i+1][0]:390;d2+=" L"+q[0]+" "+(h*q[1]).toFixed(1)+" L"+nx+" "+(h*q[1]).toFixed(1)});d2+=" L390 "+h+"Z";
    p='<path d="'+d2+'" fill="currentColor" opacity=".25"/><path d="M168 '+(h*.42)+'A14 14 0 0 1 196 '+(h*.42)+'Z" fill="currentColor" opacity=".35"/><rect x="180" y="'+(h*.05)+'" width="4" height="'+(h*.15)+'" fill="currentColor" opacity=".4"/><rect x="100" y="'+(h*.1)+'" width="8" height="'+(h*.2)+'" fill="currentColor" opacity=".3"/><rect x="296" y="'+(h*.12)+'" width="8" height="'+(h*.16)+'" fill="currentColor" opacity=".3"/>'}
  if(f.id==="e_diamonds"){for(var j=0;j<20;j++){var cx=j*20+10;p+='<path d="M'+cx+' '+(h*.2)+'L'+(cx+10)+' '+(h*.55)+'L'+cx+' '+(h*.9)+'L'+(cx-10)+' '+(h*.55)+'Z" fill="'+(j%3===1?"currentColor":"none")+'" fill-opacity=".35" stroke="currentColor" stroke-opacity=".6"/>'}p+='<rect y="'+(h*.55)+'" width="390" height="'+(h*.45)+'" fill="currentColor" opacity=".12"/>'}
  if(f.id==="e_calcada"){for(var r=0;r<3;r++)for(var k=0;k<56;k++){var xx=k*7+3,yy=h*.5+Math.sin(k/4+r*.9)*h*.22+(r-1)*h*.18;p+='<circle cx="'+xx+'" cy="'+yy.toFixed(1)+'" r="2.4" fill="currentColor" opacity="'+(r===1?.75:.3)+'"/>'}}
  if(f.id==="e_tiles"){var n=Math.ceil(W/(h+1));for(var t=0;t<n;t++)p+='<g transform="translate('+(t*(h+1))+' 0) scale('+(h/24)+')">'+TILE.replace(/<svg[^>]*>|<\/svg>/g,"")+'</g>'}
  if(f.id==="e_mosaic"){var rows=4;for(var c=0;c<56;c++){var mid=1.5+1.45*Math.sin(2*Math.PI*c/14);for(var rr=0;rr<rows;rr++){var dd=Math.abs(rr-mid),a=dd<.8?.92:dd<1.7?.42:.14;p+='<rect x="'+c*7+'" y="'+(rr*7)+'" width="6" height="6" rx="1.5" fill="currentColor" opacity="'+a+'"/>'}}h=28}
  return '<svg class="edge" viewBox="0 0 390 '+h+'" preserveAspectRatio="none" style="height:'+h+'px;color:'+col+'">'+p+'</svg>';}
function lineSvg(f){var col=colourVar(f.o.colour),m="";
  if(f.id==="l_hair")return '<div class="el-div" style="background:'+col+';opacity:.35"></div>';
  if(f.id==="l_tile")m='<svg width="22" height="22" viewBox="0 0 24 24">'+TILE.replace(/<svg[^>]*>|<\/svg>/g,"")+'</svg>';
  if(f.id==="l_fleur")m='<svg width="30" height="26" viewBox="0 0 30 26"><path d="M15 1C18 6 18 10 15 14C12 10 12 6 15 1ZM15 14C20 9 27 10 27 15C27 19 22 19 20 16C22 21 17 23 15 18C13 23 8 21 10 16C8 19 3 19 3 15C3 10 10 9 15 14ZM9 21H21" fill="currentColor" stroke="currentColor" stroke-width=".6"/></svg>';
  if(f.id==="l_diamond")m='<svg width="56" height="16" viewBox="0 0 56 16"><path d="M28 1L35 8L28 15L21 8Z" fill="currentColor"/><path d="M15 4L19 8L15 12L11 8Z M41 4L45 8L41 12L37 8Z" fill="none" stroke="currentColor"/></svg>';
  if(f.id==="l_scroll")m='<svg width="70" height="18" viewBox="0 0 70 18"><path d="M35 3L39 9L35 15L31 9Z" fill="currentColor"/><path d="M27 9C23 3 15 3 15 9C15 13 21 13 21 9 M43 9C47 3 55 3 55 9C55 13 49 13 49 9" fill="none" stroke="currentColor" stroke-width="1.4"/></svg>';
  if(f.id==="l_dots")m='<svg width="70" height="16" viewBox="0 0 70 16"><circle cx="20" cy="8" r="2" fill="currentColor"/><circle cx="27" cy="8" r="2.6" fill="currentColor"/><path d="M35 1L37 6L42 6L38 9L40 14L35 11L30 14L32 9L28 6L33 6Z" fill="currentColor"/><circle cx="43" cy="8" r="2.6" fill="currentColor"/><circle cx="50" cy="8" r="2" fill="currentColor"/></svg>';
  return '<div class="lined" style="color:'+col+'">'+m+'</div>';}
function effectArt(f){var ans=firstAnswer("effectArt.answer",f);if(ans!==undefined)return ans;if(f.grp==="edge")return '<div class="edgebox">'+edgeSvg(f)+'</div>';if(f.grp==="line")return '<div style="padding:4px 20px">'+lineSvg(f)+'</div>';
  if(f.id==="x_fade")return '<div class="fadeband" style="background-image:url('+IMG.g1+');--fh:'+f.o.h+'px"></div>';
  if(f.id==="x_tone")return '<div class="toneband" style="background-image:url('+IMG.hero+')"><div class="tn" style="background:radial-gradient('+(130*f.o.spread)+'% '+(75*f.o.spread)+'% at 12% 100%,rgba(0,0,0,.55),rgba(0,0,0,.25) 45%,rgba(0,0,0,0) 80%)"></div><div class="w">Ribeira Palace</div></div>';
  if(f.id==="x_reveal"){var n=15,t="";for(var i=0;i<n;i++)t+='<span class="t" style="transition-delay:'+(i*45)+'ms;width:24px;height:24px;display:block">'+TILE.replace("<svg",'<svg width="24" height="24"')+'</span>';
    return '<div class="tilerow" style="gap:1px;padding:4px 0">'+t+'</div><div class="ctrl" style="padding:0 14px 12px"><button type="button" data-reveal-play>&#9654; Play the reveal</button><span style="font-size:12px;color:var(--ink2);align-self:center">Replay: '+(S.replay==="every"?"Every Pass":"Once")+' (Motion)</span></div>';}
  if(f.id==="x_drift"){var g=Object.assign({},effectById("e_mosaic"));return '<div style="overflow:hidden"><div class="drift" style="width:780px;display:flex;animation-duration:'+Math.max(4,390/Math.max(1,f.o.speed))+'s">'+edgeSvg(g).replace('class="edge"','class="edge" style="width:390px;flex:none;height:28px;color:var(--fam)"')+edgeSvg(g).replace('class="edge"','class="edge" style="width:390px;flex:none;height:28px;color:var(--fam)"')+'</div></div>';}
  return "";}

/* ---------- render one element ---------- */
function C(c,k,f){return L(c&&c[k],f)}
/* an element: an own answer, or the plain one (its content first) through renderEl.html; every result through renderEl.final */
function renderEl(kind,style,c,i,still){var a={kind:kind,style:style,c:c,i:i,still:still},ans=firstAnswer("renderEl.answer",a);if(ans!==undefined)return pipe("renderEl.final",ans,a);
  c=pipe("renderEl.content",c,kind);c=c||{};var o=style.o||{},h="";
  var tone=o.tone?" t-"+o.tone:"",size=o.size&&o.size!=="m"?" s-"+o.size:"",al=o.align==="centre"?" a-centre":"";
  var scroll=!still&&style.scroll&&style.scroll!=="none"?' data-scroll="'+style.scroll+'"':"";
  switch(kind){
   case"title":h='<h3 class="el-title'+tone+size+al+'">'+esc(C(c,"text",DEMO.title))+'</h3>';break;
   case"eyebrow":h='<div class="el-eyebrow'+tone+al+'">'+esc(C(c,"text",DEMO.eyebrow))+'</div>';break;
   case"heading":h='<h4 class="el-heading'+tone+size+(o.rule==="on"?" rule":"")+'">'+esc(C(c,"text",DEMO.heading))+'</h4>';break;
   case"lede":h='<p class="el-lede'+size+al+'">'+esc(C(c,"text",DEMO.lede))+'</p>';break;
   case"paragraph":h='<p class="el-par'+(o.dropcap==="on"?" dropcap":"")+'">'+gloss(C(c,"text",DEMO.paragraph))+'</p>';break;
   case"callout":var ti=C(c,"title",DEMO.ctitle),tx=C(c,"text",DEMO.ctext);
     if(o.calltap==="flip"&&!still)h='<div class="flip" data-flip><div class="in2"><div class="fc"><b>'+esc(TXT("fun_fact_label"))+'</b><p>'+esc(tx)+'</p></div><div class="bc"><b>'+esc(ti)+'</b><p>'+esc(C(c,"back",DEMO.back))+'</p></div></div></div>';
     else h='<div class="el-callout '+(o.tone||"neutral")+size+'"><b class="'+tone+'">'+esc(ti)+'</b><p>'+esc(tx)+'</p></div>';break;
   case"pull_quote":h='<blockquote class="el-quote '+(o.variant||"serif")+'"><q>'+esc(C(c,"text",DEMO.quote))+'</q><cite>'+esc(c.cite||DEMO.cite)+'</cite></blockquote>';break;
   case"list":var tag=o.marker==="numbers"?"ol":"ul",items=(c.items&&c.items[S.lang])||DEMO.list[S.lang];h='<'+tag+' class="el-list">'+items.map(function(x){return"<li>"+esc(x)+"</li>"}).join("")+'</'+tag+'>';break;
   case"picture":var im=c.img||"g3",cap=C(c,"cap",DEMO.piccap);
     if(o.pictap==="hold"&&!still)h='<div class="hold cover" data-hold><img alt="" src="'+IMG[im]+'"><div class="ring"></div></div>';
     else if(o.pictap==="tilt"&&!still)h='<div class="tilt" data-tilt><img alt="" src="'+IMG[im]+'"></div>';
     else h='<figure class="el-pic'+(o.width==="bleed"?" bleed":"")+(o.frame==="arch"?" arch":"")+'"><div class="fr"><img alt="" src="'+IMG[im]+'"'+scroll+'></div><figcaption>'+esc(cap)+(c.credit?' · '+esc(c.credit):'')+'</figcaption></figure>';break;
   case"picture_pair":var pi=c.imgs||["before","g2"];h='<figure class="el-pair'+(o.width==="bleed"?" bleed":"")+'">'+pi.slice(0,2).map(function(x){return'<div class="fr"><img alt="" src="'+IMG[x]+'"></div>'}).join("")+'</figure><div class="el-pic" style="margin-top:8px"><figcaption>'+esc(C(c,"cap",DEMO.paircap))+'</figcaption></div>';break;
   case"carousel":var ci=c.imgs||["g1","g2","g3"];
     if(o.layout!=="one"||still)h='<div class="el-car">'+ci.map(function(x){return'<div class="fr"><img alt="" src="'+IMG[x]+'"></div>'}).join("")+'</div>';
     else{var tm=motionById(style.turn||"t_slide");h='<div class="trv'+(tm&&tm.style==="deck"?" deck":"")+'" data-trv="'+(tm?tm.id:"t_slide")+'" data-imgs="'+ci.join(",")+'"></div><div class="ctrl"><button type="button" data-turn="-1" aria-label="'+TXH("carousel_prev")+'">&#8249;</button><button type="button" data-turn="1" aria-label="'+TXH("carousel_next")+'">&#8250;</button></div>'}
     if(c.cap)h+='<div class="el-pic" style="margin-top:6px"><figcaption>'+esc(C(c,"cap"))+'</figcaption></div>';break;
   case"video":h='<div class="el-vid'+(o.width==="bleed"?" bleed":"")+'"><img alt="" src="'+IMG[c.img||"hero"]+'"'+scroll+'><i>&#9654;</i><span>'+esc(C(c,"cap",DEMO.vidcap))+'</span></div>';break;
   case"audio":h='<div class="audio" data-audio><button type="button" aria-label="'+TXH("audio_play")+'">&#9654;</button><div class="bar"><i></i></div><small>'+esc(C(c,"cap",T("Guided tour · 3:11","Visita guiada · 3:11")))+'</small></div>';break;
   case"animation":h='<div class="anim'+(o.trigger==="loop"&&!still?" loop":"")+(still?" play":"")+'" data-anim="'+(o.trigger||"enter")+'">'+ANIMSVG+'<small>'+(c.file?esc(c.file)+' · ':'')+esc(({lottie:"Lottie",rive:"Rive",video:"Video + alpha",unity:"Unity Animation"})[o.source||"lottie"])+' · '+esc(({enter:"plays on view",loop:"loops",tap:"tap to play",scrub:"scrubs with the scroll"})[o.trigger||"enter"])+'</small></div>';break;
   case"before_after":h='<div class="ba" data-ba style="--x:'+(o.start||50)+'%"><img alt="" src="'+IMG[c.after||"g2"]+'"><img class="top" alt="" src="'+IMG[c.before||"before"]+'"><span class="lb l">'+esc(L(c.blabel)||TXT("before_label"))+'</span><span class="lb r">'+esc(L(c.alabel)||TXT("after_label"))+'</span><div class="knob"></div></div>';break;
   case"hotspot":h='<div class="hs"><img alt="" src="'+IMG[c.img||"g3"]+'"><button class="pin" type="button" data-hspot="0" style="left:38%;top:42%;'+(o.pulse==="still"?"animation:none":"")+'">1</button><button class="pin" type="button" data-hspot="1" style="left:72%;top:58%;'+(o.pulse==="still"?"animation:none":"")+'">2</button><div class="pop"></div></div>';break;
   case"model_3d":h='<div class="turn" data-turnt style="background-image:url('+IMG[c.img||"g1"]+')"><span class="hint2">'+(TXH("model_3d_hint"))+'</span></div>';break;
   case"panorama":h='<div class="pano" data-pano style="background-image:url('+IMG[c.img||"hero"]+')"><span class="hint2">'+(TXH("panorama_360_drag_hint"))+'</span></div>';break;
   case"scrolly":h=still?'<figure class="el-pic"><div class="fr"><img alt="" src="'+IMG.g3+'"></div></figure>':'<div class="scrub" data-scrub><div class="stick"><img alt="" src="'+IMG.g3+'"><img alt="" src="'+IMG.before+'" style="opacity:0"><img alt="" src="'+IMG.g2+'" style="opacity:0"></div>'+
     [T("1662: the palace along the river.","1662: o palácio ao longo do rio."),T("Before 1755: the Torreão and the opera.","Antes de 1755: o Torreão e a ópera."),T("Today: the square that took its place.","Hoje: a praça que ficou no seu lugar.")].map(function(s,j){var it=c.items&&(c.items[S.lang]||c.items.en);return'<div class="step">'+esc(it&&it[j]!=null?it[j]:L(s))+'</div>'}).join("")+'<div style="height:60px"></div></div>';break;
   case"map":h='<div class="map"><svg viewBox="0 0 320 200"><path d="M0 150C60 130 120 165 180 150S290 120 320 135V200H0Z" fill="#9FC1D6" opacity=".7"/><text x="230" y="185" font-size="11" fill="#2E5870" font-family="sans-serif">Tagus</text><rect x="120" y="92" width="70" height="42" fill="none" stroke="var(--fam)" stroke-width="2"/><path d="M60 40L120 92M190 92L260 50M155 92V20" stroke="var(--ink2)" stroke-width="1.5" opacity=".5"/><g transform="translate(145 70)">'+PIN.replace("<svg",'<svg width="22" height="30"')+'</g></svg></div>';break;
   case"timeline":var ev=[["c. 1500",T("The court moves to the river","A corte muda-se para o rio")],["1580s",T("The Torreão rises","Ergue-se o Torreão")],["1755",T("Earthquake, fire and tsunami","Terramoto, incêndio e maremoto")]];if(c.items&&(c.items[S.lang]||c.items.en))ev=(c.items[S.lang]||c.items.en).map(function(s){var p=s.split(" · ");return[p[0],p.slice(1).join(" · ")]});
     h='<div class="tl">'+ev.map(function(e){return'<div><b>'+e[0]+'</b><span>'+esc(L(e[1]))+'</span></div>'}).join("")+'</div>';break;
   case"swatches":h='<div class="sw">'+[["#1F4E9C","Cobalt"],["#C99A3B","Ochre"],["#B4572E","Terracotta"],["#EFE9DC","Lime white"],["#4E7A6B","River green"]].map(function(s){return'<div><i style="background:'+s[0]+';'+(o.swsize==="s"?"height:28px":"")+'"></i><small>'+s[1]+'</small></div>'}).join("")+'</div>';break;
   case"divider":var f=effectById(o.uses||"l_hair");h=f?(f.grp==="edge"?'<div style="margin:16px -20px">'+edgeSvg(f)+'</div>':lineSvg(f)):'<div class="el-div missing"></div>';break;
   case"spacer":h='<div class="el-sp" style="--h:'+({s:16,m:24,l:40}[o.space||"m"])+'px"></div>';break;}
  return pipe("renderEl.final",pipe("renderEl.html",still?h:wrapMv(h,entranceOf(style),i),a),a);}
function noBtn(h){return h.replace(/<button/g,"<span").replace(/<\/button>/g,"</span>")}
function thumb(kind,style,cls){return noBtn('<div class="thumb'+(cls?" "+cls:"")+' cv'+(S.dark?" dark":"")+'"><div class="tin">'+renderEl(kind,style,{},0,true)+'</div></div>')}
function patThumb(p,cls){return noBtn('<div class="thumb tall'+(cls?" "+cls:"")+' cv'+(S.dark?" dark":"")+'"><div class="tin">'+p.rows.map(function(r){return renderEl(r.kind,styleById(r.style)||defStyle(r.kind),{},0,true)}).join("")+'</div></div>')}

/* ---------- motion, interaction demos ---------- */
function pathBadge(id){var p=PATH[id]||"bld";return '<span class="mpath">'+({nat:"NATIVE",bld:"BUILD",imp:"IMPORT"}[p])+'</span>'}
function motionSummary(m){var ans=firstAnswer("motionSummary.answer",m);if(ans!==undefined)return ans;if(m.type==="entrance")return "Entrance · "+(m.from==="scale"?"scales up":m.from==="none"?"fades":m.from==="wipe"?"wipes in":"from "+m.from)+(m.rot?" · tilts "+m.rot+"°":"")+" · "+m.dur+" ms"+(m.pieces!=="whole"?" · "+m.pieces+" one by one":"");
  if(m.type==="transition")return "Transition · "+({slide:"Swipe",cube:"Cube",deck:"Card Deck",flip:"Flip",fade:"Cross-fade",zoom:"Zoom"})[m.style]+" · "+m.dur+" ms";if(m.type==="scroll")return "With the scroll · "+m.factor;if(m.type==="imported")return ({lottie:"Lottie",rive:"Rive",video:"Video + alpha",unity:"Unity Animation"})[m.source]+" · "+m.file+" · "+({enter:"on view",loop:"loops",tap:"on tap",scrub:"with the scroll"})[m.trigger];return "Emphasis loop · "+m.dur+" ms";}
function motionDemo(m){var ans=firstAnswer("motionDemo.answer",m);if(ans!==undefined)return ans;
  var head='<div class="mhead"><div><b>'+esc(m.name)+'</b>'+pathBadge(m.id)+'<small>'+esc(motionSummary(m))+'</small></div><button type="button" data-play="'+m.id+'">&#9654; Play</button></div>',body="";
  if(m.type==="entrance"){
    if(m.pieces==="words")body=wrapMv('<h3 class="el-title">'+esc(L(DEMO.title))+'</h3>',m,0);
    else if(m.pieces==="letters")body=wrapMv('<h3 class="el-title a-centre">Ribeira Palace</h3>',m,0);
    else body=wrapMv('<div class="el-eyebrow t-family">'+esc(L(DEMO.eyebrow))+'</div>',m,0)+wrapMv('<h4 class="el-heading">'+esc(L(DEMO.heading))+'</h4>',m,1)+wrapMv('<p class="el-par" style="font-size:15px">'+esc(L(T("Later kings kept adding to it: a new wing and its corner tower, the Torreão.","Os reis seguintes foram acrescentando: uma nova ala e o seu torreão de canto.")))+'</p>',m,2)+wrapMv('<figure class="el-pic" style="margin:6px 0 0"><div class="fr" style="aspect-ratio:16/8"><img alt="" src="'+IMG.g2+'"></div></figure>',m,3);}
  else if(m.type==="transition")body='<div class="trv'+(m.style==="deck"?" deck":"")+'" data-trv="'+m.id+'" data-imgs="g1,g2,g3"></div><div class="ctrl"><button type="button" data-turn="-1">&#8249; Back</button><button type="button" data-turn="1">Next &#8250;</button></div>';
  else if(m.type==="scroll")body='<div class="mbox"><div class="pad">Scroll inside this box (or press Play)</div><figure class="el-pic"><div class="fr"><img alt="" src="'+IMG.g3+'" data-scroll="'+m.id+'"></div></figure><div class="pad">&#8595;</div><div class="pad"></div></div>';
  else if(m.type==="imported"){
    if(m.source==="video")body='<div class="anim" style="background:#1A1712;height:140px;position:relative;overflow:hidden" data-dust>'+Array(18).join("x").split("x").map(function(_,i){return'<i style="position:absolute;left:'+((i*53)%100)+'%;top:'+((i*37)%100)+'%;width:4px;height:4px;border-radius:50%;background:#F2C14E;box-shadow:0 0 8px #F2C14E;animation:pulse '+(1.2+(i%5)*.3)+'s ease-in-out infinite;opacity:.8"></i>'}).join("")+'<small style="position:absolute;bottom:8px;color:#EADBB0">Video with alpha over the card</small></div>';
    else if(m.source==="unity")body='<div class="anim" data-door style="perspective:600px;height:150px;display:flex;gap:2px;justify-content:center;align-items:center;background:#2A2016"><div class="door l" style="width:60px;height:110px;background:#7A4A22;transform-origin:left;transition:transform 1.2s cubic-bezier(.2,.8,.2,1);border:2px solid #C99A3B"></div><div class="door r" style="width:60px;height:110px;background:#7A4A22;transform-origin:right;transition:transform 1.2s cubic-bezier(.2,.8,.2,1);border:2px solid #C99A3B"></div></div>';
    else body='<div class="anim'+(m.trigger==="loop"?" loop":"")+'" data-anim="'+m.trigger+'">'+ANIMSVG+'<small>'+esc(motionSummary(m))+'</small></div>';}
  else body='<div class="emrow">'+(m.style==="shine"?'<span class="shine" data-em>'+esc(L(UI.read))+' &#8250;</span>':m.style==="pulse"?'<span class="pulse" data-em>'+(TXH("locate_label"))+'</span>':'<span class="floatpin" data-em>'+PIN+'</span>')+'</div>';
  return '<div class="item" data-ref="m:'+m.id+'"><div class="mdemo">'+head+body+'</div></div>';}
function interDemo(x){var b="",o=x.o;
  if(x.id==="flip")b=renderEl("callout",{o:{tone:"family",calltap:"flip"}},{},0,false);
  if(x.id==="hold")b=renderEl("picture",{o:{pictap:"hold"}},{img:"before"},0,false);
  if(x.id==="compare")b=renderEl("before_after",{o:{start:String(o.start)}},{},0,false);
  if(x.id==="deck")b='<div class="deck" data-deck>'+["g3","g2","g1"].map(function(g,i){return'<div class="cd" style="transform:translateY('+(i*-6)+'px) scale('+(1-(2-i)*.04)+');z-index:'+i+'"><img alt="" src="'+IMG[g]+'"></div>'}).join("")+'</div>';
  if(x.id==="scrub")b=renderEl("scrolly",{o:{}},{},0,false);
  if(x.id==="tiltlook")b=renderEl("picture",{o:{pictap:"tilt"}},{img:"hero"},0,false);
  if(x.id==="choice")b='<div class="choice" data-choice><div class="opts"><button type="button" data-c="0" aria-pressed="true">'+(S.lang==="pt"?"Seguir o rei":"Follow the king")+'</button><button type="button" data-c="1" aria-pressed="false">'+(S.lang==="pt"?"Seguir a biblioteca":"Follow the library")+'</button></div><p>'+esc(L(T("Manuel I moved the court to the river around 1500.","D. Manuel I mudou a corte para o rio por volta de 1500.")))+'</p></div>';
  if(x.id==="hotspots")b=renderEl("hotspot",{o:{pulse:"pulse"}},{},0,false);
  return '<div class="item" data-ref="i:'+x.id+'"><div class="mdemo"><div class="mhead"><div><b>'+esc(x.name)+'</b><span class="mpath">'+(x.id==="scrub"?"BUILD":"NATIVE")+'</span><small>'+esc(x.h)+'</small></div></div>'+b+'</div></div>';}
function effectDemo(f){return '<div class="item" data-ref="f:'+f.id+'"><div class="fx"><div class="cap">'+esc(f.name)+' <small>· '+({edge:"edge",line:"line divider",light:"light",moving:"moving"})[f.grp]+'</small></div>'+effectArt(f)+'</div></div>'}

/* ---------- block previews on the card ---------- */
/* a block on the card, from its effective settings: an own answer or the plain one, its frame, then the last touches */
function blockPreviewHtml(b,i){emitLatestFirst("blockPreviewHtml.before",b,i);var out;
  try{out=firstAnswer("blockPreviewHtml.answer",b,i);if(out===undefined){var m=b.enter==="family"?motionById(S.family):motionById(b.enter),h="",tap=b.tap==="page"?" tap":"";
  var or=b.tap==="page"?'<div class="open'+(b.orow==="nudge"?" nudge":"")+(b.orow==="shine"?" shine":"")+'">'+(b.kind==="Story"?esc(L(UI.read)):esc(L(UI.open))+(b.orow==="nudge"?"":" &#8250;"))+'</div>':"";
  if(b.kind==="Quick Facts")h='<div class="qf"><div><small>'+esc(L(UI.home))+'</small><b>'+esc(L(UI.homev))+'</b></div><div><small>'+esc(L(UI.lost))+'</small><b>'+esc(L(UI.lostv))+'</b></div></div>';
  if(b.kind==="Inline"){var p=patternById(b.pattern);h=p?p.rows.map(function(r,j){return renderEl(r.kind,styleById(r.style)||defStyle(r.kind),{},j,true).replace('class="el-callout','class="el-callout')}).join(""):"";
    if(p&&p.rows[0].kind==="callout"&&(styleById(p.rows[0].style)||{o:{}}).o.calltap==="flip")h=renderEl("callout",styleById(p.rows[0].style),{text:T("The palace had its own opera house, opened in March 1755.","O palácio tinha a sua própria ópera, inaugurada em março de 1755."),back:T("It stood for only seven months.","Durou apenas sete meses.")},0,false).replace(/^<div class="mv[^>]*>|<\/div>$/g,"")}
  if(b.kind==="Story")h='<img alt="" src="'+IMG.g3+'" style="width:100%;height:170px;object-fit:cover;border-radius:12px;display:block"><div class="bt" style="color:var(--stories);margin-top:12px">'+esc(L(UI.where))+'</div><h3>'+esc(L(DEMO.title))+'</h3><p>'+esc(L(DEMO.lede))+'</p>';
  if(b.kind==="Before and After")h=renderEl("before_after",defStyle("before_after"),{},0,true);
  if(b.kind==="Gallery")h=renderEl("carousel",defStyle("carousel"),{},0,true);
  if(b.kind==="Audio Guide")h=renderEl("audio",defStyle("audio"),{},0,true);
  if(b.kind==="Knowledge Check")h='<div class="kq" data-kq><b>'+esc(L(T("Which king moved the court to the river?","Que rei mudou a corte para o rio?")))+'</b><button type="button" data-ok="1">Manuel I</button><button type="button">Philip I</button><button type="button">José I</button></div>';
  if(b.kind==="Practical Info")h='<dl class="info"><dt>'+(S.lang==="pt"?"Onde":"Where")+'</dt><dd>Praça do Comércio</dd></dl><div class="more"><dl class="info" style="margin-top:6px"><dt>'+(S.lang==="pt"?"Hoje":"Today")+'</dt><dd>'+(S.lang==="pt"?"Praça aberta, sempre":"An open square, always")+'</dd><dt>Metro</dt><dd>Terreiro do Paço</dd></dl></div>'+'<div class="open">'+(TXH("open_row_expand"))+' &#8964;</div>';
  var inner='<div class="bprev'+tap+(b.tap==="expand"?" expand":"")+'" '+(b.tap==="page"?'data-go="'+(b.kind==="Story"?"story":"gallery")+'"':b.tap==="expand"?"data-expand":"")+'>'+h+or+'</div>';
  out='<div class="item" data-ref="b:'+b.id+'">'+wrapMv(inner,m,0)+'</div>'}out=pipe("blockPreviewHtml.frame",out,b)}finally{emit("blockPreviewHtml.done",b,i)}
  return pipe("blockPreviewHtml.html",out,b)}

/* ---------- phone ---------- */
var scr=document.getElementById("scr"),ph=document.getElementById("ph"),hint=document.getElementById("hint"),fit=document.getElementById("fit");
var SCREENS={phone:[360,740],land:[740,360],tablet:[640,860]};
function topbar(crumb){var ans=firstAnswer("topbar.answer",crumb);if(ans!==undefined)return ans;return '<div class="topbar"><button type="button" data-go="card" aria-label="'+TXH("page_back")+'">&#8249;</button><span>Ribeira Palace &#8250; '+crumb+'</span></div>';}
function layerHead(t,s){return '<div class="layerhead">'+t+'<small>'+s+'</small></div>'}
function demoScreen(){var h=topbar("<b>"+esc(L(UI.demo))+"</b>")+'<div class="pg">';
  h+=layerHead("Motion","Each motion on real parts. Motion Family: <b>"+esc((motionById(S.family)||{}).name)+"</b> · Replay: <b>"+(S.replay==="every"?"Every Pass":"Once")+"</b>.");
  Object.keys(TYPES).forEach(function(t){S.motions.filter(function(m){return m.type===t}).forEach(function(m){h+=motionDemo(m)})});
  h+=layerHead("Interaction","Try each one: tap, hold, drag, swipe, scroll, tilt.");S.inter.forEach(function(x){h+=interDemo(x)});
  h+=layerHead("Effects","Edges, line dividers, light and moving effects (the Look Kit).");S.effects.forEach(function(f){h+=effectDemo(f)});
  h+=layerHead("Elements","Each element in each of its styles. ★ = the default a new row gets.");
  KINDS.forEach(function(K){(S.styles[K.k]||[]).forEach(function(st){h+='<div class="item" data-ref="s:'+st.id+'"><span class="tag">'+esc(K.n)+' · <b>'+esc(st.name)+(st.def?" ★":"")+'</b></span><div class="blk">'+renderEl(K.k,st,{},0)+'</div></div>'})});
  h+=layerHead("Patterns","Groups of elements with their styles; a marker fills the content.");
  S.patterns.forEach(function(p){h+='<div class="item" data-ref="p:'+p.id+'"><span class="tag">Pattern · <b>'+esc(p.name)+'</b></span><div class="blk">'+p.rows.map(function(r,i){return renderEl(r.kind,styleById(r.style)||defStyle(r.kind),{},i)}).join("")+'</div></div>'});
  return pipe("demoScreen.html",h+'</div>');}
function storyScreen(){emitLatestFirst("storyScreen.before");var h=topbar(esc(L(secById("stories").name))+" &#8250; <b>"+esc(L(DEMO.title))+"</b>")+'<div class="pg"><div class="blk">';
  S.story.forEach(function(r){if(r.pattern){var p=patternById(r.pattern);if(!p){h+='<div class="item" data-ref="r:'+r.id+'"><p class="el-par missing">(missing pattern)</p></div>';return}
      var parts=p.rows.map(function(pr,i){return renderEl(pr.kind,styleById(pr.style)||defStyle(pr.kind),r.slots[i]||{},i)});
      h+='<div class="item" data-ref="r:'+r.id+'">'+(r.pattern==="pt1"?'<div class="wide2"><div>'+parts[0]+'</div><div>'+parts.slice(1).join("")+'</div></div>':parts.join(""))+'</div>';}
    else h+='<div class="item" data-ref="r:'+r.id+'">'+renderEl(r.kind,(r.style&&styleById(r.style))||defStyle(r.kind),r.c,0)+'</div>'});
  return pipe("storyScreen.html",h+'</div></div>');}
function galleryScreen(){emitLatestFirst("galleryScreen.before");return pipe("galleryScreen.html",topbar(esc(L(secById("media").name))+" &#8250; <b>"+TXH("gallery_name")+"</b>")+'<div class="pg"><div class="blk">'+(S.galleryImgs||["g1","g2","g3","before"]).map(function(g,i){return renderEl("picture",defStyle("picture"),{img:g,cap:T("Picture "+(i+1),"Imagem "+(i+1))},i)}).join("")+'</div></div>')}
/* the phone: the features before (latest first) and after (in module order), sharing the call's notes cx */
function renderPhone(keep){if(noDraw)return;var cx={};emitLatestFirst("renderPhone.before",keep,cx);var top=keep?scr.scrollTop:0,sz=SCREENS[S.screen];
  ph.style.width=sz[0]+"px";ph.style.height=sz[1]+"px";ph.classList.toggle("land",S.screen==="land");
  var k=Math.min(1,390/sz[0]);fit.style.transform="scale("+k+")";fit.style.height=(sz[1]*k)+"px";fit.style.width=(sz[0]*k)+"px";fit.style.transformOrigin="top left";
  document.getElementById("camera").style.backgroundImage=S.screen==="land"?"url("+IMG.hero+")":"none";
  ph.classList.toggle("dark",S.dark);ph.classList.toggle("reduce",S.reduce);
  scr.innerHTML=S.view==="card"?cardScreen():S.view==="demo"?demoScreen():S.view==="story"?storyScreen():S.view==="term"?termScreen():S.view==="democard"?demoCardScreen():galleryScreen();
  document.getElementById("ptool").innerHTML=ptool();
  var nb=document.getElementById("navbar");nb.innerHTML=navbar();nb.style.display=S.view==="card"?"":"none";
  hint.style.display=S.view==="card"?"":"none";hint.firstChild.textContent="The card · Ribeira Palace";
  splitPieces();scr.scrollTop=top;observe();initTransitions();initBoxes();onScroll(scr);applyFocus();
  emit("renderPhone.after",keep,cx)}
function splitPieces(){scr.querySelectorAll(".mvp").forEach(function(w){var m=motionById(w.dataset.pieces);var n=0;
  w.querySelectorAll("h3,h4,p,q,div.el-eyebrow").forEach(function(el){if(el.children.length)return;
    var parts=m.pieces==="letters"?el.textContent.split(""):el.textContent.split(/(\s+)/);
    el.innerHTML=parts.map(function(x){if(/^\s+$/.test(x))return x;return'<span class="piece mv pre" style="'+motionVars(m,n++)+'">'+esc(x)+'</span>'}).join("")})})}
var io=null;
function observe(){if(io)io.disconnect();io=new IntersectionObserver(function(es){es.forEach(function(e){var t=e.target;
   if(e.isIntersecting){t.classList.add("in");if(t.dataset&&t.dataset.anim==="enter")t.classList.add("play")}
   else if(e.boundingClientRect.top>0&&S.replay==="every"){t.classList.remove("in");if(t.dataset&&t.dataset.anim==="enter")t.classList.remove("play")}})},{root:scr,rootMargin:"0px 0px -10% 0px",threshold:0.05});
  scr.querySelectorAll(".mv,[data-anim=enter]").forEach(function(el){io.observe(el)});}
function onScroll(box){var r=box.getBoundingClientRect();
  box.querySelectorAll("img[data-scroll]").forEach(function(img){if(box===scr&&img.closest(".mbox"))return;
   if(S.reduce){img.style.transform="";return}var b=img.parentNode.getBoundingClientRect();var c=(b.top+b.height/2-(r.top+r.height/2))/r.height;
   var m=motionById(img.dataset.scroll);if(!m)return;
   if(m.style==="zoom")img.style.transform="scale("+(1+m.factor*Math.max(0,Math.min(1,1-Math.abs(c))))+")";
   else if(m.style==="scalein"){var k=Math.max(0,Math.min(1,1-c));img.parentNode.style.transform="scale("+(1-m.factor+m.factor*k)+")";}
   else img.style.transform="translateY("+(c*-m.factor*b.height)+"px) scale(1.2)";});
  if(box===scr)scr.querySelectorAll('[data-anim="scrub"]').forEach(function(a){var b=a.getBoundingClientRect();var k=Math.max(0,Math.min(1,(r.bottom-b.top)/(r.height*.8)));
    a.querySelectorAll(".draw").forEach(function(d){d.style.strokeDashoffset=160*(1-k)});var fi=a.querySelector(".fillin");if(fi)fi.style.opacity=Math.max(0,(k-.6)*2)*.85});}
scr.addEventListener("scroll",function(){onScroll(scr)},{passive:true});
function initBoxes(){scr.querySelectorAll(".mbox").forEach(function(bx){bx.addEventListener("scroll",function(){onScroll(bx)},{passive:true});onScroll(bx)});
  scr.querySelectorAll("[data-scrub]").forEach(function(sb){sb.addEventListener("scroll",function(){var st=sb.querySelectorAll(".step"),imgs=sb.querySelectorAll(".stick img"),top=sb.getBoundingClientRect().top+150,idx=0;
    st.forEach(function(s,i){if(s.getBoundingClientRect().top<top+40)idx=i});imgs.forEach(function(im,i){im.style.opacity=i===idx?1:0})},{passive:true})});
  scr.querySelectorAll("[data-ba]").forEach(dragX(function(el,f){el.style.setProperty("--x",(f*100).toFixed(1)+"%")}));
  scr.querySelectorAll("[data-turnt]").forEach(dragX(function(el,f,dx){el.style.backgroundPositionX=(parseFloat(el.dataset.bx||50)+dx*.3)+"%"},true));
  scr.querySelectorAll("[data-pano]").forEach(dragX(function(el,f,dx){var v=(parseFloat(el.dataset.px||0)+dx);el.dataset.px=v;el.style.backgroundPositionX=v+"px"},true));
  scr.querySelectorAll("[data-tilt]").forEach(function(t){t.addEventListener("pointermove",function(e){if(S.reduce)return;var b=t.getBoundingClientRect(),x=(e.clientX-b.left)/b.width-.5,y=(e.clientY-b.top)/b.height-.5;var s=(interById("tiltlook")||{o:{strength:12}}).o.strength;t.querySelector("img").style.transform="translate("+(-x*s)+"px,"+(-y*s)+"px)"})});
  scr.querySelectorAll("[data-hold]").forEach(function(hd){var timer=null,start=0;function stop(){cancelAnimationFrame(timer);timer=null;if(hd.classList.contains("cover"))hd.querySelector(".ring").style.setProperty("--p",0)}
    hd.addEventListener("pointerdown",function(){if(!hd.classList.contains("cover")){hd.classList.add("cover");return}start=performance.now();var need=(interById("hold")||{o:{hold:900}}).o.hold;(function step(){var p=Math.min(1,(performance.now()-start)/need);hd.querySelector(".ring").style.setProperty("--p",p*100);if(p>=1){hd.classList.remove("cover");timer=null;return}timer=requestAnimationFrame(step)})()});
    hd.addEventListener("pointerup",stop);hd.addEventListener("pointerleave",stop)});
  scr.querySelectorAll("[data-deck]").forEach(function(dk){dk.addEventListener("pointerdown",function(e){var cards=dk.querySelectorAll(".cd"),top=cards[cards.length-1];if(!top)return;var x0=e.clientX;top.setPointerCapture(e.pointerId);
    function mv(ev){top.style.transition="none";top.style.transform="translateX("+(ev.clientX-x0)+"px) rotate("+((ev.clientX-x0)/12)+"deg)"}
    function up(ev){top.removeEventListener("pointermove",mv);top.removeEventListener("pointerup",up);top.style.transition="";var dx=ev.clientX-x0,need=(interById("deck")||{o:{throwd:120}}).o.throwd;
      if(Math.abs(dx)>need/2){top.style.transform="translateX("+(dx>0?400:-400)+"px) rotate("+(dx>0?25:-25)+"deg)";top.style.opacity=0;setTimeout(function(){dk.insertBefore(top,dk.firstChild);top.style.transform="translateY(6px) scale(.92)";top.style.opacity=1;restack(dk)},350)}else top.style.transform=""}
    top.addEventListener("pointermove",mv);top.addEventListener("pointerup",up)})});}
function restack(dk){var cards=dk.querySelectorAll(".cd");cards.forEach(function(c,i){c.style.zIndex=i;c.style.transform="translateY("+((cards.length-1-i)*6)+"px) scale("+(1-(cards.length-1-i)*.04)+")"})}
function dragX(fn,rel){return function(el){el.addEventListener("pointerdown",function(e){el.setPointerCapture(e.pointerId);var x0=e.clientX,last=e.clientX;
  function mv(ev){var b=el.getBoundingClientRect();fn(el,Math.max(0,Math.min(1,(ev.clientX-b.left)/b.width)),ev.clientX-last);last=ev.clientX}
  function up(){el.removeEventListener("pointermove",mv);el.removeEventListener("pointerup",up)}
  if(!rel)mv(e);el.addEventListener("pointermove",mv);el.addEventListener("pointerup",up)})}}
function initTransitions(){scr.querySelectorAll(".trv").forEach(function(t){var imgs=t.dataset.imgs.split(",");t.dataset.i="0";
  t.innerHTML=imgs.map(function(x,i){return'<div class="sl" style="opacity:'+(i?0:1)+'"><img alt="" src="'+IMG[x]+'"><span>'+(i+1)+' / '+imgs.length+'</span></div>'}).join("")})}
function turn(t,dir){var m=motionById(t.dataset.trv),sl=t.querySelectorAll(".sl"),n=sl.length,i=+t.dataset.i,j=(i+dir+n)%n;var A=sl[i],B=sl[j];t.dataset.i=j;
  sl.forEach(function(s,k){if(k!==i&&k!==j)s.style.opacity=0});
  if(S.reduce||!m){A.style.opacity=0;B.style.opacity=1;return}
  var s=m.style,KA,KB,oa="50% 50%",ob="50% 50%";
  if(s==="slide"){KA=[{transform:"translateX(0)"},{transform:"translateX("+(-100*dir)+"%)"}];KB=[{transform:"translateX("+(100*dir)+"%)"},{transform:"translateX(0)"}]}
  else if(s==="cube"){oa=dir>0?"100% 50%":"0% 50%";ob=dir>0?"0% 50%":"100% 50%";KA=[{transform:"translateX(0) rotateY(0)"},{transform:"translateX("+(-100*dir)+"%) rotateY("+(-90*dir)+"deg)"}];KB=[{transform:"translateX("+(100*dir)+"%) rotateY("+(90*dir)+"deg)"},{transform:"translateX(0) rotateY(0)"}]}
  else if(s==="deck"){KA=[{transform:"translate(0,0) rotate(0)"},{transform:"translate("+(-115*dir)+"%,6%) rotate("+(-14*dir)+"deg)"}];KB=[{transform:"translateY(14px) scale(.9)",opacity:.6},{transform:"translateY(0) scale(1)",opacity:1}]}
  else if(s==="flip"){KA=[{transform:"rotateY(0)"},{transform:"rotateY("+(180*dir)+"deg)"}];KB=[{transform:"rotateY("+(-180*dir)+"deg)"},{transform:"rotateY(0)"}]}
  else if(s==="zoom"){KA=[{transform:"scale(1)",opacity:1},{transform:"scale(1.25)",opacity:0}];KB=[{transform:"scale(.85)",opacity:0},{transform:"scale(1)",opacity:1}]}
  else{KA=[{opacity:1},{opacity:0}];KB=[{opacity:0},{opacity:1}]}
  A.style.transformOrigin=oa;B.style.transformOrigin=ob;A.style.opacity=1;B.style.opacity=1;A.style.zIndex=2;B.style.zIndex=1;
  var o={duration:m.dur,easing:easeCss(m.ease),fill:"forwards"};A.animate(KA,o).onfinish=function(){A.style.opacity=0;A.getAnimations().forEach(function(x){x.cancel()});A.style.zIndex=""};B.animate(KB,o).onfinish=function(){B.getAnimations().forEach(function(x){x.cancel()});B.style.opacity=1}}
function applyFocus(){var on=S.focusOn&&S.view==="demo"&&S.focusRef;scr.classList.toggle("focusing",!!on);
  scr.querySelectorAll(".item.focus").forEach(function(x){x.classList.remove("focus")});if(on){var el=scr.querySelector('[data-ref="'+S.focusRef+'"]');if(el)el.classList.add("focus")}}
function flashRef(ref){if(noDraw)return;var el=scr.querySelector('[data-ref="'+ref+'"]');if(!el)return;var y=el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-70;
  scr.scrollTo({top:Math.max(0,y),behavior:S.reduce?"auto":"smooth"});if(S.view!=="demo"){el.classList.remove("flash");void el.offsetWidth;el.classList.add("flash")}}
function show(view,ref){if(noDraw){S.view=view;return}if(ref&&view==="demo")S.focusRef=ref;if(S.view!==view){S.view=view;renderPhone(false)}else applyFocus();if(ref)setTimeout(function(){flashRef(ref)},40)}
/* replay: jump back without a transition, then play forward */
function replayIn(root){var els=root.querySelectorAll(".mv");els.forEach(function(el){el.style.transition="none";el.classList.remove("in")});void root.offsetWidth;
  requestAnimationFrame(function(){els.forEach(function(el){el.style.transition=""});requestAnimationFrame(function(){els.forEach(function(el){el.classList.add("in")})})})}
ph.addEventListener("click",function(e){var t=e.target;
  var pt=t.closest("[data-pt]");if(pt){var k=pt.dataset.pt;if(k==="screen")S.screen=pt.dataset.v;else if(k==="lang")S.lang=S.lang==="en"?"pt":"en";else S[k]=!S[k];renderAll(true);return}
  var g=t.closest("[data-go]");if(g&&!t.closest(".kq")){S.view=g.dataset.go;renderPhone(false);return}
  var ex=t.closest("[data-expand]");if(ex){ex.classList.toggle("opened");return}
  var kq=t.closest(".kq button");if(kq){kq.classList.add(kq.dataset.ok?"ok":"no");return}
  var fl=t.closest("[data-flip]");if(fl){fl.classList.toggle("on");return}
  var au=t.closest("[data-audio] button");if(au){var a=au.parentNode;a.classList.toggle("playing");au.innerHTML=a.classList.contains("playing")?"&#10073;&#10073;":"&#9654;";au.setAttribute("aria-label",a.classList.contains("playing")?TXT("audio_pause"):TXT("audio_play"));return}
  var an=t.closest('[data-anim="tap"]');if(an){an.classList.remove("play");void an.offsetWidth;an.classList.add("play");return}
  var dr=t.closest("[data-door]");if(dr){var op=dr.dataset.open!=="1";dr.dataset.open=op?"1":"0";dr.querySelector(".l").style.transform=op?"rotateY(-75deg)":"";dr.querySelector(".r").style.transform=op?"rotateY(75deg)":"";return}
  var hsp=t.closest("[data-hspot]");if(hsp){var pop=hsp.parentNode.querySelector(".pop");var notes=[T("The Torreão: the corner tower of the 1580s.","O Torreão: a torre de canto da década de 1580."),T("The river front where the court landed.","A frente ribeirinha onde a corte desembarcava.")];pop.textContent=L(notes[+hsp.dataset.hspot]);pop.classList.add("on");return}
  var ch=t.closest("[data-c]");if(ch){var box=ch.closest("[data-choice]");box.querySelectorAll("[data-c]").forEach(function(x){x.setAttribute("aria-pressed",String(x===ch))});box.querySelector("p").textContent=L(ch.dataset.c==="0"?T("Manuel I moved the court to the river around 1500.","D. Manuel I mudou a corte para o rio por volta de 1500."):T("The royal library held some 70,000 books, lost in 1755.","A biblioteca real guardava cerca de 70 000 livros, perdidos em 1755."));return}
  var tb=t.closest("[data-turn]");if(tb){var tv=tb.parentNode.previousElementSibling;if(tv&&tv.classList.contains("trv"))turn(tv,+tb.dataset.turn);return}
  var rp=t.closest("[data-reveal-play]");if(rp){var row=rp.closest(".fx").querySelector(".tilerow"),ts=row.querySelectorAll(".t");ts.forEach(function(x){x.style.transition="none"});row.classList.add("hidden");void row.offsetWidth;
    requestAnimationFrame(function(){ts.forEach(function(x){x.style.transition=""});requestAnimationFrame(function(){row.classList.remove("hidden")})});return}
  var p=t.closest("[data-play]");if(p){var m=motionById(p.dataset.play),bx=p.closest(".mdemo");
    if(m.type==="entrance")replayIn(bx);else if(m.type==="transition")turn(bx.querySelector(".trv"),1);
    else if(m.type==="scroll"){var mb=bx.querySelector(".mbox");mb.scrollTop=0;setTimeout(function(){mb.scrollTo({top:mb.scrollHeight,behavior:"smooth"})},60)}
    else if(m.type==="imported"){var a2=bx.querySelector("[data-anim]");if(a2){a2.classList.remove("play","loop");a2.querySelectorAll(".draw").forEach(function(d){d.style.strokeDashoffset=""});void a2.offsetWidth;a2.classList.add("play")}var d2=bx.querySelector("[data-door]");if(d2)d2.click()}
    else{var em=bx.querySelector("[data-em]");em.style.animation="none";void em.offsetWidth;em.style.animation=""}}});

/* ---------- Editor ---------- */
var ed=document.getElementById("ed");
function help(t){return '<button class="help" type="button" data-help="'+esc(t)+'" aria-label="Help">?</button>'}
function seg(key,vals,labels,cur){return '<div class="seg">'+vals.map(function(v,i){return'<button type="button" data-set="'+key+'" data-v="'+v+'" aria-pressed="'+(String(cur)===String(v))+'">'+esc(labels[i])+'</button>'}).join("")+'</div>'}
function row(label,ctrl,h,ind){return '<div class="row'+(ind?" ind":"")+'"><label>'+esc(label)+'</label><div>'+ctrl+'</div>'+(h?help(h):"<span></span>")+'</div>'}
function sel(key,opts,cur){return '<select class="u" data-sel="'+key+'">'+opts.map(function(o){return'<option value="'+esc(o[0])+'"'+(o[0]===cur?" selected":"")+'>'+esc(o[1])+'</option>'}).join("")+'</select>'}
function slider(key,min,max,step,val,adv){return '<div class="sl2"><input type="range" data-num="'+key+'" min="'+min+'" max="'+max+'" step="'+step+'" value="'+val+'" aria-label="'+key+'"><input class="u" type="number" data-num="'+key+'" min="'+min+'" max="'+max+'" step="'+step+'" value="'+val+'" aria-label="'+key+'"></div>'+(adv?'<div class="note warn">'+adv+'</div>':"")}
/* foldouts: open state by id; depth for "open to level" */
function isOpen(id,depth){if(S.op.hasOwnProperty(id))return S.op[id];return depth<=S.ui.level}
function fo(id,title,badge,body,depth,cur){return '<details class="fo'+(cur?" cur":"")+'" data-fo="'+id+'" data-depth="'+depth+'"'+(isOpen(id,depth)?" open":"")+'><summary>'+title+'<span class="sp"></span>'+(badge?'<span class="badge '+badge[0]+'">'+badge[1]+'</span>':"")+'</summary>'+(body?'<div class="inner">'+body+'</div>':"")+'</details>'}
function locked(t){return '<details class="fo locked"><summary>'+esc(t)+'<span class="sp"></span><span class="badge">EXISTING</span></summary></details>'}
function motionOpts(type,withFamily){var a=[];if(withFamily)a.push(["family","Family's ("+((motionById(S.family)||{}).name||"(missing)")+")"]);S.motions.forEach(function(m){if(m.type===type)a.push([m.id,m.name])});a.push(["none","None"]);return a}
function matchQ(name){var q=S.ui.q.trim().toLowerCase();return !q||name.toLowerCase().indexOf(q)>=0}
function matches(name,cat){var q=S.ui.q.trim().toLowerCase();return (S.ui.cat==="All"||cat===S.ui.cat)&&(!q||name.toLowerCase().indexOf(q)>=0)}
function advice(m){if(m.type==="entrance"&&m.dur>600)return "Long for an entrance: the visitor waits "+m.dur+" ms to read. Fine for one hero moment.";if(m.type==="transition"&&m.dur>900)return "A slow turn: good for a story mode, tiring for a quick swipe.";if(m.type==="entrance"&&m.dist>80)return "A long travel: a dramatic entrance.";return ""}
function motionBody(m){var ans=firstAnswer("motionBody.answer",m);if(ans!==undefined)return ans;var b=row("Name",'<input class="u" data-text="motion" value="'+esc(m.name)+'">',"The name styles and blocks pick it by.");
  if(m.type==="entrance"){b+=row("Comes in",seg("motion.from",["below","left","right","scale","wipe","none"],["Below","Left","Right","Scale","Wipe","In place"],m.from),"Where it enters from.",true)+
   row("Distance (pt)",slider("motion.dist",0,200,1,m.dist),"How far it travels.",true)+row("Tilt (°)",slider("motion.rot",-20,20,1,m.rot||0),"A rotation that settles to straight.",true)+
   row("Duration (ms)",slider("motion.dur",100,2000,10,m.dur,advice(m)),"Advice, not a cap: 250-500 ms reads best.",true)+row("Easing",seg("motion.ease",["out","overshoot","spring","inout"],["Ease Out","Overshoot","Spring","In-Out"],m.ease),"Overshoot and Spring give the little jump.",true)+
   row("Pieces",seg("motion.pieces",["whole","words","letters"],["Whole","Word by word","Letter by letter"],m.pieces),"Words and letters build a line of text.",true)+row("Stagger (ms)",slider("motion.stagger",0,300,1,m.stagger),"The delay between pieces or rows.",true)}
  else if(m.type==="transition")b+=row("Turn",seg("motion.style",["slide","cube","deck","flip","fade","zoom"],["Swipe","Cube","Card Deck","Flip","Cross-fade","Zoom"],m.style),"How one item gives way to the next.",true)+row("Duration (ms)",slider("motion.dur",150,2000,10,m.dur,advice(m)),"Advice: 400-700 ms.",true)+row("Easing",seg("motion.ease",["out","inout","spring"],["Ease Out","In-Out","Spring"],m.ease),"",true);
  else if(m.type==="scroll")b+=row("Strength",slider("motion.factor",0,0.5,0.01,m.factor),"How much the picture moves or grows.",true);
  else if(m.type==="imported")b+=row("Source",seg("motion.source",["lottie","rive","video","unity"],["Lottie","Rive","Video + alpha","Unity Animation"],m.source),"Made in another tool (or a Unity prefab), imported as an asset.",true)+row("File",'<input class="u" data-text="motionfile" value="'+esc(m.file)+'">',"The asset in the wall's folder.",true)+row("Plays",seg("motion.trigger",["enter","loop","tap","scrub"],["On view","Loop","On tap","With scroll"],m.trigger),"With scroll scrubs it with the visitor's scroll.",true);
  else b+=row("Period (ms)",slider("motion.dur",600,6000,50,m.dur),"One loop; loops stop under Reduce Motion.",true);
  return b;}
function edCardV3(){var h='';
  /* Editor view: how foldouts behave + find */
  h+='<div class="fo" style="padding:6px 8px"><div class="row" style="grid-template-columns:150px minmax(0,1fr) 22px"><label>Foldouts</label><div>'+seg("ui.single",["false","true"],["Many open","One at a time"],String(S.ui.single))+'</div>'+help("Editor preference (not saved to the wall): one open at a time closes the others at the same level.")+'</div>'+
   row("Open to level",seg("ui.level",["0","1","2","3"],["Closed","1","2","3"],String(S.ui.level)),"How deep the foldouts open by default.")+
   '<div class="filters"><input class="u" data-q="1" placeholder="Find a part..." value="'+esc(S.ui.q)+'">'+["All"].concat(CATS).map(function(c){return'<button class="chip" type="button" data-cat="'+c+'" aria-pressed="'+(S.ui.cat===c)+'">'+c+'</button>'}).join("")+'</div></div>';
  h+=locked("Card Components")+locked("Sections")+locked("Navigation Bar")+locked("Locate Button");
  var b="";
  b+=fo("test","Test",["test","TEST"],row("Show the components demo",seg("test.on",["true","false"],["On","Off"],String(S.test.on)),"Adds a test block at the top of the chosen marker's About, in Scene and Play only. Never saved to the marker.")+
     row("On marker",sel("test.poi",POIS,S.test.poi),"Which marker's card shows the demo. The Ribeira Palace is the reference card.",true)+
     row("Focus on what I edit",seg("focusOn",["true","false"],["On","Off"],String(S.focusOn)),"The demo jumps to the part you edit, marks it EDITING and dims the rest.",true)+
     '<div class="note">Theme, Reduce Motion, language and screen are on the phone\'s floating Preview bar (Play Mode).</div>'+
     fo("how1","How to Scene Test","",'<div class="tip">LivingRoomScene, the marker above, its card preview: the orange tile is first in About. Edit any part: the demo jumps to it.</div>',9)+
     fo("how2","How to Playmode Test","",'<div class="tip">Play, tap the marker, open the card, tap the orange tile. Use the floating Preview bar: Light / Dark, Reduce, EN / PT, Focus, screen.</div>',9),2);
  /* Motion */
  var mb=row("Motion Family",sel("family",motionOpts("entrance",false),S.family),"The card's default entrance.")+
   row("Replay",seg("replay",["every","once"],["Every Pass","Once"],S.replay),"Every Pass: scrolling down plays an entrance, scrolling back up undoes it, down again replays. Once: it stays. One choice for the whole card (Reveal follows it).")+
   '<div class="note fixed">One hard rule: Reduce Motion shows everything whole at once and stops loops. Everything else is yours; warnings are advice.</div>';
  Object.keys(TYPES).forEach(function(t){var inner="";S.motions.filter(function(m){return m.type===t&&matchQ(m.name)}).forEach(function(m){var u=motionUses(m.id);
     inner+=fo("mo:"+m.id,esc(m.name)+' <span class="sub">· '+esc(motionSummary(m))+' · used '+u+'</span>',[PATH[m.id]==="imp"?"bro":PATH[m.id]==="bld"?"bld":"nat",({nat:"NATIVE",bld:"BUILD",imp:"IMPORT"})[PATH[m.id]||"bld"]],motionBody(m)+'<div class="btnrow"><button class="del" type="button" data-del="motion" data-id="'+m.id+'" '+(u?"disabled title=\"In use\"":"")+'>Delete</button></div>',9,S.sel.layer==="motion"&&S.sel.motion===m.id)});
    inner+='<div class="btnrow"><button class="ubtn sm" type="button" data-add="motion" data-type="'+t+'">+ New '+TYPES[t].toLowerCase()+' preset</button></div>';
    mb+=fo("mt:"+t,TYPES[t]+' <span class="sub">· '+S.motions.filter(function(m){return m.type===t}).length+'</span>',null,inner,3)});
  mb+='<div class="tip"><b>Making a motion.</b> + New makes a named preset of a type: that covers most needs. Imported animation brings motion made elsewhere (Rive, Lottie, a video with alpha, or a Unity prefab\'s Animation) with a trigger. A new kind of movement in code is a motion type registered like an element kind.</div>';
  b+=fo("motion","Motion",["new","NEW"],mb,2);
  /* Interaction */
  var ib='<div class="note">Things the visitor does to a part. Elements and patterns pick them (a Callout\'s Tap to Flip, a Picture\'s Hold to Reveal).</div>';
  S.inter.forEach(function(x){if(!matchQ(x.name))return;var o=x.o,bb='<div class="note">'+esc(x.h)+'</div>';
    if(x.id==="flip")bb+=row("Turn (ms)",slider("inter.dur",200,1500,10,o.dur),"",true);if(x.id==="hold")bb+=row("Hold (ms)",slider("inter.hold",300,3000,50,o.hold),"How long the press must last.",true);
    if(x.id==="compare")bb+=row("Start at (%)",slider("inter.start",0,100,1,o.start),"",true);if(x.id==="deck")bb+=row("Throw at (pt)",slider("inter.throwd",40,240,5,o.throwd),"How far a drag must go to throw a card.",true);
    if(x.id==="tiltlook")bb+=row("Strength (pt)",slider("inter.strength",0,40,1,o.strength),"",true);
    ib+=fo("in:"+x.id,esc(x.name),[x.id==="scrub"?"bld":"nat",x.id==="scrub"?"BUILD":"NATIVE"],bb,9,S.sel.layer==="inter"&&S.sel.inter===x.id)});
  b+=fo("interaction","Interaction",["new","NEW"],ib,2);
  /* Effects */
  var eb='<div class="note">The Look Kit. Edges shape the start of a section or a divider; line dividers sit between parts. A Divider element picks one of these.</div>';
  [["edge","Edges"],["line","Line dividers"],["light","Light"],["moving","Moving"]].forEach(function(gp){var inner="";S.effects.filter(function(f){return f.grp===gp[0]&&matchQ(f.name)}).forEach(function(f){var o=f.o,fb='<div class="thumb cv'+(S.dark?" dark":"")+'" style="height:auto;padding:4px 0">'+(f.grp==="edge"?edgeSvg(f):f.grp==="line"?lineSvg(f):"")+'</div>';
     if(o.h!=null&&f.grp==="edge")fb+=row("Height (pt)",slider("fx.h",12,80,1,o.h),"",true);
     if(o.colour)fb+=row("Colour",seg("fx.colour",["family","accent","neutral"],["Family colour","Accent","Neutral"],o.colour),"",true);
     if(f.id==="x_fade")fb+=row("Height (pt)",slider("fx.h",20,160,1,o.h),"",true);if(f.id==="x_tone")fb+=row("Spread",slider("fx.spread",0.4,1.5,0.05,o.spread),"Its strength stays automatic.",true);if(f.id==="x_drift")fb+=row("Speed (pt/s)",slider("fx.speed",2,40,1,o.speed),"",true);
     if(f.id==="x_reveal")fb+='<div class="note">Replay follows Motion > Replay.</div>';
     inner+=fo("fx:"+f.id,esc(f.name)+' <span class="sub">· used '+effectUses(f.id)+'</span>',["nat","NATIVE"],fb,9,S.sel.layer==="effect"&&S.sel.effect===f.id)});
    eb+=fo("fg:"+gp[0],gp[1],null,inner,3)});
  eb+='<div class="btnrow"><button class="ubtn sm" type="button" disabled title="Lab: an SVG asset of the wall">+ Import an edge (SVG)</button></div>';
  b+=fo("effects","Effects",["ext","EXTENDED"],eb,2);
  /* Elements */
  var elb='<div class="note">Each element opens to its styles, rendered. ★ = the default a new row gets.</div>';
  CATS.forEach(function(cat){var inner="";KINDS.filter(function(K){return K.c===cat&&matches(K.n,cat)}).forEach(function(K){var sts=S.styles[K.k]||[],d=defStyle(K.k);
    var cards='<div class="thumbs">'+sts.map(function(st){return'<button class="sc" type="button" data-pickstyle="'+st.id+'" aria-pressed="'+(S.sel.style===st.id)+'">'+thumb(K.k,st)+'<span class="nm">'+(st.def?"<i>&#9733;</i>":"")+'<span>'+esc(st.name)+'</span><em>'+styleUses(st.id)+'</em></span></button>'}).join("")+'</div>';
    var st=styleById(S.sel.style),sb="";
    if(st&&kindOfStyle(st.id)===K.k){sb+=row("Name",'<input class="u" data-text="style" value="'+esc(st.name)+'">',"The name rows and patterns pick.");
      (OPTS[K.k]||[]).forEach(function(k){var dd=OPTDEF[k];if(k==="uses"){sb+=row(dd.l,sel("style.uses",S.effects.filter(function(f){return f.grp==="edge"||f.grp==="line"}).map(function(f){return[f.id,f.name+" ("+(f.grp==="edge"?"edge":"line")+")"]}),st.o.uses),dd.h,true);return}sb+=row(dd.l,seg("o."+k,dd.v,dd.vl,st.o[k]),dd.h,true)});
      sb+=row("Entrance",sel("style.enter",motionOpts("entrance",true),st.enter),"How it comes into view.",true);
      if(K.k==="carousel"&&st.o.layout==="one")sb+=row("Transition",sel("style.turn",motionOpts("transition",false).filter(function(x){return x[0]!=="none"}),st.turn||"t_slide"),"How one picture gives way to the next.",true);
      if(MEDIA[K.k]&&K.k!=="carousel")sb+=row("With the scroll",sel("style.scroll",motionOpts("scroll",false),st.scroll||"none"),"Parallax, a slow zoom or growing in.",true);
      var u2=styleUses(st.id);sb+='<div class="btnrow"><button class="ubtn sm" type="button" data-def="'+st.id+'"'+(st.def?" disabled":"")+'>&#9733; Make default</button><button class="ubtn sm" type="button" data-add="style">+ Add style (copy)</button><button class="del" type="button" data-del="style" data-id="'+st.id+'" '+(u2||st.def?"disabled":"")+'>Delete</button></div>';}
    inner+=fo("el:"+K.k,'<span class="foth cv'+(S.dark?" dark":"")+'"><span class="tin" style="display:block">'+renderEl(K.k,d,{},0,true)+'</span></span><span class="kicon'+(cat==="Media"?" m":cat==="Structure"?" s":cat==="Interactive"?" p":"")+'">'+K.ab+'</span>'+esc(K.n)+' <span class="sub">· '+sts.length+' style'+(sts.length>1?"s":"")+'</span>',null,cards+sb,9,S.sel.layer==="style"&&S.sel.kind===K.k)});
    if(inner)elb+=fo("eg:"+cat,cat,null,inner,3)});
  b+=fo("elements","Elements",["new","NEW"],elb,2);
  /* Patterns */
  var pb='<div class="note">A pattern owns its rows\' styles; a marker only fills its content.</div>';
  S.patterns.forEach(function(p){if(!matches(p.name,p.cat))return;var u=patternUses(p.id),body=patThumb(p)+row("Name",'<input class="u" data-text="pattern" data-pid="'+p.id+'" value="'+esc(p.name)+'">',"The name a marker picks.");
    body+='<table class="tbl"><thead><tr><th style="width:22px">#</th><th style="width:34%">Element</th><th>Style</th><th style="width:80px"></th></tr></thead><tbody>'+p.rows.map(function(r,i){var opts=(S.styles[r.kind]||[]).map(function(s){return[s.id,s.name+(s.def?" ★":"")]});if(r.style&&!styleById(r.style))opts.unshift([r.style,"(missing)"]);
      return'<tr><td class="uses">'+(i+1)+'</td><td><span class="kicon">'+KIND[r.kind].ab+'</span>'+KIND[r.kind].n+'</td><td>'+sel("prow."+p.id+"."+i,opts,r.style)+'</td><td><button class="del" type="button" data-pmove="'+p.id+'.'+i+'" data-dir="-1">^</button> <button class="del" type="button" data-pmove="'+p.id+'.'+i+'" data-dir="1">v</button> <button class="del" type="button" data-pdel="'+p.id+'.'+i+'">&#128465;</button></td></tr>'}).join("")+'</tbody></table>'+
     '<div class="note">+ Add element</div><div class="btnrow">'+KINDS.map(function(K){return'<button class="ubtn sm" type="button" data-padd="'+p.id+'.'+K.k+'">'+K.n+'</button>'}).join("")+'</div><div class="btnrow"><button class="del" type="button" data-del="pattern" data-id="'+p.id+'" '+(u?"disabled":"")+'>Delete pattern</button></div>';
    pb+=fo("pt:"+p.id,'<span class="kicon p">Pt</span>'+esc(p.name)+' <span class="sub">· '+p.cat+' · '+p.rows.length+' elements · used '+u+'</span>',null,body,9,S.sel.layer==="pattern"&&S.sel.pattern===p.id)});
  pb+='<div class="btnrow"><button class="ubtn sm" type="button" data-add="pattern">+ Add pattern</button></div>';
  b+=fo("patterns","Patterns",["new","NEW"],pb,2);
  h+=fo("bc","Block Components",["new","NEW"],b,1);
  /* Block Library */
  var lb='<div class="note">A block earns its place with behaviour, data or an owner; arrangements of words and pictures are patterns, placed by the Inline block. '+esc(GONE)+'</div><div class="bgrid">';
  LIB.forEach(function(r){if(!matchQ(r[1]))return;lb+='<button class="sc bcard" type="button" data-block="'+esc(r[1])+'" aria-pressed="'+(S.sel.block===r[1])+'">'+libThumb(r[1])+'<span class="nm">'+esc(r[1])+(r[2]==="new"?' <span class="badge new">NEW</span>':'')+'</span><span class="from">'+esc(r[0])+' · built from '+esc(r[4])+'</span></button>'});
  lb+='</div>';var cur=LIB.filter(function(r){return r[1]===S.sel.block})[0];if(cur)lb+='<div class="tip"><b>'+esc(cur[1])+'</b> · '+esc(cur[3])+' Its own options (Block Library defaults) and its preview / page settings live here; a marker places it and fills its content.</div>';
  h+=fo("library","Block Library",["ext","EXTENDED"],lb,1);
  h+=jsonBox("config.json · card_settings.block_components",bcJson());return h;}
function libThumb(name){var ans=firstAnswer("libThumb.answer",name);if(ans!==undefined)return ans;var k={"Header":["picture","st17"],"Inline":["callout","st28"],"Story":["picture","st16"],"Quick Facts":["swatches","st42"],"Timeline":["timeline","st41"],"Gallery":["carousel","st20"],"Before and After":["before_after","st35"],"Hotspot Image":["hotspot","st36"],"Video":["video","st21"],"3D Model":["model_3d","st37"],"360 Panorama":["panorama","st38"],"Audio Guide":["audio","st32"],"Wall Locator":["map","st40"],"Today Map":["map","st40"],"Compare Points":["picture_pair","st19"],"Related":["picture_pair","st19"],"Sources":["list","st14"],"Contents":["list","st15"],"Practical Info":["list","st14"]}[name]||["callout","st11"];
  return thumb(k[0],styleById(k[1]))}

function edMarkerV3(){var h=row("Marker",sel("m.poi",[["painting","7. Ribeira Palace"]],"painting"),"The marker whose card you edit.");
  h+=locked("Position & Visual")+locked("Labels");
  /* 1. the card */
  var cb='<div class="note">The card as the visitor meets it: blocks in order, in their bar groups, each with its preview and what a tap does. The Header is set above (existing).</div><table class="tbl"><thead><tr><th style="width:22px">#</th><th style="width:80px">Preview</th><th>Block</th><th style="width:70px">Group</th><th style="width:74px"></th></tr></thead><tbody>';
  S.card.forEach(function(b,i){cb+='<tr class="pick'+(S.sel.cblock===b.id?" sel":"")+'" data-pick="cblock" data-id="'+b.id+'"><td class="uses">'+(i+1)+'</td><td>'+cardThumb(b)+'</td><td><b>'+esc(b.kind)+'</b>'+(b.pattern?' <span class="uses">· '+esc((patternById(b.pattern)||{}).name||"(missing)")+'</span>':'')+'</td><td class="uses">'+esc(L(UI[b.grp]))+'</td><td>'+mv2("cmove",i)+'</td></tr>';
    if(S.sel.cblock===b.id){var x='<div class="fields">'+row("Group",sel("cb.grp",GROUPS.map(function(g){return[g,UI[g].en]}),b.grp),"The bar group (section) it sits in.")+
      row("Entrance",sel("cb.enter",motionOpts("entrance",true),b.enter),"How the preview comes into view.")+
      row("Tap",seg("cb.tap",["page","expand","nothing"],["Open its page","Expand here","Nothing"],b.tap),"What a tap on the preview does. Open its page goes to the block's full page (Pages below).")+
      (b.tap==="page"?row("Open Row",seg("cb.orow",["plain","nudge","shine"],["Plain","Nudge","Shine"],b.orow),"The words that invite the tap, and their small motion."):"")+
      (b.kind==="Story"?row("Preview",seg("cb.preview",["auto","custom"],["Automatic","Custom"],b.preview||"auto"),"Automatic: the page's first picture, eyebrow, title and lede."):"")+
      (b.kind==="Inline"?row("Pattern",sel("cb.pattern",S.patterns.map(function(p){return[p.id,p.name]}),b.pattern),"What the Inline block places on the card."):"")+'</div>';
      cb+='<tr class="slot"><td></td><td colspan="4">'+x+'</td></tr>'}});
  cb+='</tbody></table><div class="note">+ Add block</div><div class="btnrow">'+LIB.slice(0,10).map(function(r){return'<button class="ubtn sm" type="button" data-cadd="'+esc(r[1])+'">'+esc(r[1])+'</button>'}).join("")+'<span class="uses">…</span></div>';
  h+=fo("mcard","1 · The card · blocks and previews",["new","NEW"],cb,1);
  /* 2. pages */
  var t='<div class="note">Story · '+S.story.length+' rows. Pick a style by its picture; open a row for its content fields (EN / PT).</div><table class="tbl"><thead><tr><th style="width:22px">#</th><th style="width:26%">Element</th><th style="width:38%">Style</th><th>Content</th><th style="width:74px"></th></tr></thead><tbody>';
  S.story.forEach(function(r,i){
   if(r.pattern){var p=patternById(r.pattern);
     t+='<tr class="grp pick'+(S.sel.row===r.id?" sel":"")+'" data-pick="row" data-id="'+r.id+'"><td class="uses">'+(i+1)+'</td><td colspan="2"><span class="kicon p">Pt</span><b>'+(p?esc(p.name):'<span class="missing">(missing)</span>')+'</b></td><td class="uses">pattern · '+(p?p.rows.length:0)+' slots</td><td>'+mv2("smove",i)+'</td></tr>';
     if(p&&S.sel.row===r.id)p.rows.forEach(function(pr,j){var st=styleById(pr.style)||defStyle(pr.kind);r.slots[j]=r.slots[j]||{};
       t+='<tr class="slot"><td></td><td colspan="4"><div class="sub2">'+KIND[pr.kind].n+' · '+esc(st?st.name:"(missing)")+' · the pattern\'s style</div>'+fieldsFor(pr.kind,r.slots[j],r.id+".s"+j)+'</td></tr>'});}
   else{var st2=(r.style&&styleById(r.style))||defStyle(r.kind);
     t+='<tr class="pick'+(S.sel.row===r.id?" sel":"")+'" data-pick="row" data-id="'+r.id+'"><td class="uses">'+(i+1)+'</td><td><span class="kicon">'+KIND[r.kind].ab+'</span>'+KIND[r.kind].n+'</td><td><button class="ubtn sm" type="button" data-openpicker="'+r.id+'" style="height:auto;padding:2px 6px;display:flex;align-items:center;max-width:100%">'+thumb(r.kind,st2,"sm")+'<span style="overflow:hidden;text-overflow:ellipsis">'+esc(st2.name)+(r.style?"":" (default)")+' &#9662;</span></button></td><td class="uses">'+esc(C(r.c,"text")||C(r.c,"cap"))+'</td><td>'+mv2("smove",i)+'</td></tr>';
     if(S.sel.picker===r.id)t+='<tr class="slot"><td></td><td colspan="4"><div class="picker"><div class="thumbs">'+[{id:"",name:defStyle(r.kind).name+" (default)"}].concat(S.styles[r.kind]).map(function(s){var real=s.id?s:defStyle(r.kind);return'<button class="sc" type="button" data-setstyle="'+r.id+'|'+s.id+'" aria-pressed="'+((r.style||"")===s.id)+'">'+thumb(r.kind,real)+'<span class="nm"><span>'+esc(s.name)+'</span></span></button>'}).join("")+'</div></div></td></tr>';
     if(S.sel.row===r.id)t+='<tr class="slot"><td></td><td colspan="4">'+fieldsFor(r.kind,r.c,r.id+".c")+'</td></tr>';}});
  t+='</tbody></table><div class="note">+ Add element</div><div class="btnrow">'+KINDS.map(function(K){return'<button class="ubtn sm" type="button" data-sadd="'+K.k+'">'+K.n+'</button>'}).join("")+'</div>';
  t+='<div class="note">+ Add pattern</div><div class="thumbs">'+S.patterns.map(function(p){return'<button class="sc" type="button" data-spadd="'+p.id+'">'+patThumb(p)+'<span class="nm"><span>'+esc(p.name)+'</span></span></button>'}).join("")+'</div>';
  var pages=row("Page of",sel("m.page",[["story","Story · "+L(DEMO.title)],["gallery","Gallery"]],S.sel.page||"story"),"Each block with a page has its content here.")+fo("pg:story","Story · "+esc(L(DEMO.title)),["ext","EXTENDED"],t,9);
  h+=fo("mpages","2 · Pages · each block's content",["new","NEW"],pages,1);
  h+=jsonBox("config.json · the marker's detail card (blocks + the story page)",markerJson());return h;}
function mv2(kind,i){return'<button class="del" type="button" data-'+kind+'="'+i+'" data-dir="-1">^</button> <button class="del" type="button" data-'+kind+'="'+i+'" data-dir="1">v</button> <button class="del" type="button" data-'+kind+'="'+i+'" data-dir="0">&#128465;</button>'}
function cardThumb(b){var h=noBtn(blockPreview(b,0)).replace(/class="mv pre[^"]*"/g,'class=""').replace(/data-go="[^"]*"/g,"");return '<div class="thumb sm cv'+(S.dark?" dark":"")+'" style="width:72px;height:44px"><div class="tin" style="transform:scale(.21)">'+h+'</div></div>'}
function fieldsFor(kind,c,path){var f=FIELDS[kind]||[],h='<div class="fields">';if(!f.length)return '<span class="uses">No content: the element draws from its style (and the wall\'s data).</span>';
  f.forEach(function(x){var key=x[0],type=x[1],lab=x[2],v=c[key];
   if(type==="ltext"){v=v||{en:"",pt:""};langCodes().forEach(function(lg){h+=row(lab+" · "+lg.toUpperCase(),'<textarea class="u" rows="2" data-field="'+path+'|'+key+'|'+lg+'">'+esc(v[lg]||"")+'</textarea>',lg==="en"?"One per language; empty in PT falls back to EN.":"")})}
   else if(type==="litems"){v=v||{en:[],pt:[]};langCodes().forEach(function(lg){h+=row(lab+" · "+lg.toUpperCase(),'<textarea class="u" rows="3" data-field="'+path+'|'+key+'|'+lg+'|lines">'+esc((v[lg]||[]).join("\n"))+'</textarea>',"")})}
   else if(type==="text")h+=row(lab,'<input class="u" data-field="'+path+'|'+key+'" value="'+esc(v||"")+'">',"");
   else if(type==="img")h+=row(lab,'<div class="assets">'+ASSETS.map(function(a){return'<button type="button" data-asset="'+path+'|'+key+'|'+a+'" aria-pressed="'+(v===a)+'" style="background-image:url('+IMG[a]+')" aria-label="'+a+'"></button>'}).join("")+'</div>',"A picture from the wall's media folder.");
   else if(type==="imgs"){v=v||[];h+=row(lab,'<div class="assets">'+ASSETS.map(function(a){return'<button type="button" data-assets="'+path+'|'+key+'|'+a+'" aria-pressed="'+(v.indexOf(a)>=0)+'" style="background-image:url('+IMG[a]+')" aria-label="'+a+'"></button>'}).join("")+'</div>',"Tap to add or remove; the order is the tap order.")}});
  return h+'</div>';}
function contentAt(path){var ans=firstAnswer("contentAt.answer",path);if(ans!==undefined)return ans;var p=path.split("."),r=byId(S.story,p[0]);if(!r)return null;if(p[1]==="c")return r.c=r.c||{};var j=+p[1].slice(1);return r.slots[j]=r.slots[j]||{}}
function edGlobal(){var h='<div class="note">The top level only: the wall\'s markers and how a visitor finds them.</div>';
  var gc=function(id,sub){return fo(id,esc(COMP[id][1])+' <span class="sub">· '+sub+'</span>',null,ownbar(id)+' '+ownPanels(id),2)};
  GSECS.forEach(function(g,i){var srch=i===GSECS.length-1,body=srch?row("Synonym Groups",'<button class="ubtn sm" type="button" data-goloc="term:t3">Languages &amp; Terms › Terms &#8250;</button>',"One list with the card\'s glossary. Keyword Fields stay here: only the search reads them.")+gc("g:10s","where the visitor types a search")+gc("g:10f","opens the filters, with how many are on"):' ';
   h+=fo("g:"+i,esc(g[0])+' <span class="sub">· '+esc(g[1])+'</span>',srch?["ext","EXTENDED"]:["","EXISTING"],body,1)});
  h+=fo("g:11",'Onboarding hints <span class="sub">· the first-use hints over the wall</span>',["new","NEW"],row("Wall hint",pseg("wallTip",["true","false"],["Shown","Hidden"]),"Over the wall until the visitor opens a card.")+ownbar("g:11")+ownPanels("g:11"),1);return h}
function jsonBox(t,obj){return'<div class="json"><div class="jh"><span>'+esc(t)+'</span><span>live</span></div><pre>'+esc(JSON.stringify(obj,null,2))+'</pre></div>'}
function bcJson(){var es={};for(var k in S.styles)es[k]=S.styles[k].map(function(s){var o={key:s.id,name:s.name};for(var x in s.o)o[x]=s.o[x];o.entrance=s.enter;if(s.scroll)o.scroll=s.scroll;if(s.turn)o.transition=s.turn;if(s.def)o["default"]=true;return o});
  return{motion_family:S.family,replay:S.replay,motions:S.motions.map(function(m){var o={key:m.id};for(var x in m)if(x!=="id")o[x]=m[x];return o}),interactions:S.inter.map(function(x){return{key:x.id,options:x.o}}),
   effects:S.effects.map(function(f){return{key:f.id,group:f.grp,options:f.o}}),element_styles:es,patterns:S.patterns.map(function(p){return{key:p.id,name:p.name,rows:p.rows.map(function(r){return{element:r.kind,style:r.style}})}})}}
function markerJson(){return{blocks:S.card.map(function(b){var o={kind:b.kind,group:b.grp,entrance:b.enter,tap:b.tap};if(b.tap==="page")o.open_row=b.orow;if(b.pattern)o.pattern=b.pattern;return o}),
  story_page:S.story.map(function(r){return r.pattern?{pattern:r.pattern,slots:r.slots}:{element:r.kind,style:r.style||undefined,content:r.c}})}}
var lastRender=0;
/* noDraw: the self-test works the Editor's controls without drawing the Editor or the phone (show only sets the view) */
var noDraw=false;
function renderEditor(){if(noDraw)return;emitLatestFirst("renderEditor.before");lastRender=performance.now();document.querySelectorAll(".utab").forEach(function(t){t.setAttribute("aria-selected",String(t.dataset.tab===S.tab))});
  var h=firstAnswer("renderEditor.tab");ed.innerHTML=h!==undefined?h:S.tab==="card"?edCard():S.tab==="marker"?edMarker():edGlobal();emit("renderEditor.after")}
function renderAll(keep){if(noDraw)return;renderEditor();renderPhone(keep)}
function refreshJson(){var pre=ed.querySelector(".json pre");if(pre)pre.textContent=JSON.stringify(S.tab==="card"?bcJson():markerJson(),null,2)}

/* ---------- Editor events ---------- */
ed.addEventListener("toggle",function(e){var d=e.target;if(!d.dataset||!d.dataset.fo)return;var id=d.dataset.fo,quiet=performance.now()-lastRender<150;S.op[id]=d.open;
  if(!d.open||quiet)return;
  if(S.ui.single){var par=d.parentElement;Array.prototype.forEach.call(par.children,function(s){if(s!==d&&s.tagName==="DETAILS"&&s.open){s.open=false;S.op[s.dataset.fo]=false}})}
  var k=id.split(":");
  if(k[0]==="el"){S.sel.kind=k[1];if(kindOfStyle(S.sel.style)!==k[1])S.sel.style=defStyle(k[1]).id;S.sel.layer="style";markCur();show("demo","s:"+S.sel.style)}
  else if(k[0]==="pt"){S.sel.pattern=k[1];S.sel.layer="pattern";markCur();show("demo","p:"+k[1])}
  else if(k[0]==="mo"){S.sel.motion=k[1];S.sel.layer="motion";markCur();show("demo","m:"+k[1])}
  else if(k[0]==="fx"){S.sel.effect=k[1];S.sel.layer="effect";markCur();show("demo","f:"+k[1])}
  else if(k[0]==="in"){S.sel.inter=k[1];S.sel.layer="inter";markCur();show("demo","i:"+k[1])}
  else if(id==="mcard"){show("card")}else if(id==="mpages"){show("story")}},true);
function markCur(){ed.querySelectorAll(".fo.cur").forEach(function(x){x.classList.remove("cur")});var s=S.sel,id={motion:"mo:"+s.motion,effect:"fx:"+s.effect,pattern:"pt:"+s.pattern,inter:"in:"+s.inter,style:"el:"+s.kind}[s.layer];var el=ed.querySelector('[data-fo="'+id+'"]');if(el)el.classList.add("cur")}
ed.addEventListener("click",function(e){var t=e.target;
  var hb=t.closest("[data-help]");if(hb){var nx=hb.parentNode.nextElementSibling;if(nx&&nx.dataset&&nx.dataset.h){nx.remove();return}var tip=document.createElement("div");tip.className="tip";tip.dataset.h="1";tip.textContent=hb.dataset.help;hb.parentNode.after(tip);return}
  var b=t.closest("[data-set]");if(b){setOpt(b.dataset.set,b.dataset.v);return}
  var ct=t.closest("[data-cat]");if(ct){S.ui.cat=ct.dataset.cat;renderEditor();return}
  var bl=t.closest("[data-block]");if(bl){S.sel.block=bl.dataset.block;renderEditor();return}
  var ps=t.closest("[data-pickstyle]");if(ps){S.sel.style=ps.dataset.pickstyle;S.sel.kind=kindOfStyle(S.sel.style);S.sel.layer="style";renderEditor();show("demo","s:"+S.sel.style);return}
  var op=t.closest("[data-openpicker]");if(op){S.sel.picker=S.sel.picker===op.dataset.openpicker?null:op.dataset.openpicker;S.sel.row=op.dataset.openpicker;renderEditor();show("story","r:"+S.sel.row);return}
  var ss=t.closest("[data-setstyle]");if(ss){var pr=ss.dataset.setstyle.split("|"),rr=byId(S.story,pr[0]);rr.style=pr[1]||null;S.sel.picker=null;renderAll(true);show("story","r:"+rr.id);return}
  var as=t.closest("[data-asset]");if(as){var q=as.dataset.asset.split("|"),c=contentAt(q[0]);c[q[1]]=q[2];renderAll(true);show("story","r:"+q[0].split(".")[0]);return}
  var as2=t.closest("[data-assets]");if(as2){var q2=as2.dataset.assets.split("|"),c2=contentAt(q2[0]),arr=c2[q2[1]]=c2[q2[1]]||[],ix=arr.indexOf(q2[2]);if(ix>=0)arr.splice(ix,1);else arr.push(q2[2]);renderAll(true);show("story","r:"+q2[0].split(".")[0]);return}
  var star=t.closest("[data-def]");if(star&&!star.disabled){var k=kindOfStyle(star.dataset.def);S.styles[k].forEach(function(s){s.def=s.id===star.dataset.def});renderAll(true);show("demo","s:"+star.dataset.def);return}
  var d=t.closest("[data-del]");if(d&&!d.disabled){var id=d.dataset.id;
    if(d.dataset.del==="motion"){S.motions=S.motions.filter(function(m){return m.id!==id});S.sel.motion="rise"}
    if(d.dataset.del==="style"){var kk=kindOfStyle(id);S.styles[kk]=S.styles[kk].filter(function(s){return s.id!==id});S.sel.style=defStyle(kk).id}
    if(d.dataset.del==="pattern"){S.patterns=S.patterns.filter(function(p){return p.id!==id});S.sel.pattern=S.patterns[0]&&S.patterns[0].id}
    renderAll(true);return}
  var a=t.closest("[data-add]");if(a){
    if(a.dataset.add==="motion"){var ty=a.dataset.type,base=S.motions.filter(function(m){return m.type===ty})[0],m=JSON.parse(JSON.stringify(base));m.id=nid("mo");m.name="New "+TYPES[ty];PATH[m.id]=PATH[base.id];S.motions.push(m);S.sel.motion=m.id;S.sel.layer="motion";S.op["mo:"+m.id]=true;renderAll(true);show("demo","m:"+m.id);return}
    if(a.dataset.add==="style"){var bs=styleById(S.sel.style)||defStyle(S.sel.kind),ns=JSON.parse(JSON.stringify(bs));ns.id=nid("st");ns.name=bs.name+" Copy";ns.def=false;S.styles[S.sel.kind].push(ns);S.sel.style=ns.id;renderAll(true);show("demo","s:"+ns.id);return}
    if(a.dataset.add==="pattern"){var np={id:nid("pt"),name:"New Pattern",cat:"Text",rows:[{kind:"heading",style:defStyle("heading").id},{kind:"paragraph",style:defStyle("paragraph").id}]};S.patterns.push(np);S.sel.pattern=np.id;S.sel.layer="pattern";S.op["pt:"+np.id]=true;renderAll(true);show("demo","p:"+np.id);return}}
  var pa=t.closest("[data-padd]");if(pa){var q3=pa.dataset.padd.split("."),p=patternById(q3[0]);p.rows.push({kind:q3[1],style:defStyle(q3[1]).id});S.story.forEach(function(r){if(r.pattern===p.id)r.slots.push({})});renderAll(true);show("demo","p:"+p.id);return}
  var pm=t.closest("[data-pmove]");if(pm){var q4=pm.dataset.pmove.split("."),p2=patternById(q4[0]),i=+q4[1],j=i+(+pm.dataset.dir);if(j<0||j>=p2.rows.length)return;swap(p2.rows,i,j);S.story.forEach(function(r){if(r.pattern===p2.id)swap(r.slots,i,j)});renderAll(true);show("demo","p:"+p2.id);return}
  var pd=t.closest("[data-pdel]");if(pd){var q5=pd.dataset.pdel.split("."),p3=patternById(q5[0]),k3=+q5[1];p3.rows.splice(k3,1);S.story.forEach(function(r){if(r.pattern===p3.id)r.slots.splice(k3,1)});renderAll(true);show("demo","p:"+p3.id);return}
  var sa=t.closest("[data-sadd]");if(sa){var kd=sa.dataset.sadd,nr={id:nid("r"),kind:kd,style:null,c:{}};S.story.push(nr);S.sel.row=nr.id;renderAll(true);show("story","r:"+nr.id);return}
  var sp=t.closest("[data-spadd]");if(sp){var pp=patternById(sp.dataset.spadd),r2={id:nid("r"),pattern:pp.id,slots:pp.rows.map(function(){return{}})};S.story.push(r2);S.sel.row=r2.id;renderAll(true);show("story","r:"+r2.id);return}
  var sm=t.closest("[data-smove]");if(sm){var i2=+sm.dataset.smove,dir=+sm.dataset.dir;if(dir===0)S.story.splice(i2,1);else{var j2=i2+dir;if(j2<0||j2>=S.story.length)return;swap(S.story,i2,j2)}renderAll(true);return}
  var cm=t.closest("[data-cmove]");if(cm){var i3=+cm.dataset.cmove,dir2=+cm.dataset.dir;if(dir2===0)S.card.splice(i3,1);else{var j3=i3+dir2;if(j3<0||j3>=S.card.length)return;swap(S.card,i3,j3)}renderAll(true);show("card");return}
  var ca=t.closest("[data-cadd]");if(ca){var nb={id:nid("b"),kind:ca.dataset.cadd,grp:"about",enter:"family",tap:"nothing",orow:"plain"};if(nb.kind==="Inline")nb.pattern="pt3";S.card.push(nb);S.sel.cblock=nb.id;renderAll(true);show("card","b:"+nb.id);return}
  var pk=t.closest("[data-pick]");if(pk&&!t.closest("select,input,textarea,button")){if(pk.dataset.pick==="row"){S.sel.row=pk.dataset.id;S.sel.picker=null;renderEditor();show("story","r:"+S.sel.row)}else{S.sel.cblock=pk.dataset.id;renderEditor();show("card","b:"+S.sel.cblock)}}});
function swap(a,i,j){var x=a[i];a[i]=a[j];a[j]=x}
function setOpt(key,v){if(firstAnswer("setOpt.answer",key,v)!==undefined)return;
  if(key==="test.on"){S.test.on=v==="true";renderEditor();S.view="card";renderPhone(false);return}
  if(key==="focusOn"){S.focusOn=v==="true";renderAll(true);return}
  if(key==="replay"){S.replay=v;renderAll(true);return}
  if(key==="ui.single"){S.ui.single=v==="true";renderEditor();return}
  if(key==="ui.level"){S.ui.level=+v;S.op={};renderEditor();return}
  if(key.indexOf("cb.")===0){var b=byId(S.card,S.sel.cblock);b[key.slice(3)]=v;renderAll(true);show("card","b:"+b.id);return}
  var m=motionById(S.sel.motion),st=styleById(S.sel.style),f=effectById(S.sel.effect);
  if(key.indexOf("motion.")===0&&m){m[key.slice(7)]=v;renderAll(true);show("demo","m:"+m.id);playRef("m:"+m.id);return}
  if(key.indexOf("o.")===0&&st){st.o[key.slice(2)]=v;renderAll(true);show("demo","s:"+st.id);return}
  if(key.indexOf("fx.")===0&&f){f.o[key.slice(3)]=v;renderAll(true);show("demo","f:"+f.id);return}}
function playRef(ref){if(noDraw)return;setTimeout(function(){var b=scr.querySelector('[data-ref="'+ref+'"] [data-play]');if(b)b.click()},380)}
ed.addEventListener("change",function(e){var t=e.target,k=t.dataset.sel;if(t.dataset.num){numChange(t,true);return}if(!k)return;
  if(k==="test.poi"){S.test.poi=t.value;S.view="card";renderAll(false);return}
  if(k==="family"){S.family=t.value;renderAll(true);return}
  if(k.indexOf("cb.")===0){var b=byId(S.card,S.sel.cblock);b[k.slice(3)]=t.value;renderAll(true);show("card","b:"+b.id);return}
  var st=styleById(S.sel.style);
  if(k==="style.enter"&&st){st.enter=t.value;renderAll(true);show("demo","s:"+st.id);setTimeout(function(){var it=scr.querySelector('[data-ref="s:'+st.id+'"]');if(it)replayIn(it)},420);return}
  if(k==="style.scroll"&&st){st.scroll=t.value;renderAll(true);show("demo","s:"+st.id);return}
  if(k==="style.turn"&&st){st.turn=t.value;renderAll(true);show("demo","s:"+st.id);return}
  if(k==="style.uses"&&st){st.o.uses=t.value;renderAll(true);show("demo","s:"+st.id);return}
  if(k.indexOf("prow.")===0){var q=k.split(".");patternById(q[1]).rows[+q[2]].style=t.value;renderAll(true);show("demo","p:"+q[1]);return}});
function numChange(t,final){var key=t.dataset.num,v=parseFloat(t.value);if(isNaN(v))return;ed.querySelectorAll('[data-num="'+key+'"]').forEach(function(o){if(o!==t)o.value=v});
  if(key.indexOf("motion.")===0){var m=motionById(S.sel.motion);if(!m)return;m[key.slice(7)]=v;if(final){renderAll(true);show("demo","m:"+m.id);playRef("m:"+m.id)}else refreshJson();return}
  if(key.indexOf("fx.")===0){var f=effectById(S.sel.effect);if(!f)return;f.o[key.slice(3)]=v;renderPhone(true);show("demo","f:"+f.id);refreshJson();return}
  if(key.indexOf("inter.")===0){var x=interById(S.sel.inter);if(!x)return;x.o[key.slice(6)]=v;if(final){renderPhone(true);show("demo","i:"+x.id)}refreshJson();return}}
ed.addEventListener("input",function(e){var t=e.target;
  if(t.dataset.num){numChange(t,false);return}
  if(t.dataset.q){S.ui.q=t.value;var pos=t.selectionStart;renderEditor();var n=ed.querySelector("[data-q]");if(n){n.focus();n.setSelectionRange(pos,pos)}return}
  if(t.dataset.text){var w=t.dataset.text,obj=w==="motion"||w==="motionfile"?motionById(S.sel.motion):w==="style"?styleById(S.sel.style):patternById(t.dataset.pid);if(obj){if(w==="motionfile")obj.file=t.value;else obj.name=t.value;renderPhone(true);refreshJson()}return}
  if(t.dataset.field){var q=t.dataset.field.split("|"),c=contentAt(q[0]);if(!c)return;if(q[2]){c[q[1]]=c[q[1]]||{};c[q[1]][q[2]]=q[3]==="lines"?t.value.split("\n"):t.value}else c[q[1]]=t.value;renderPhone(true);refreshJson()}});
document.querySelectorAll(".utab").forEach(function(t){t.addEventListener("click",function(){S.tab=t.dataset.tab;renderEditor();if(S.tab==="marker")show("card");else if(S.tab==="card")show("demo")})});
document.querySelectorAll("[data-step]").forEach(function(b){b.addEventListener("click",function(){var n=+b.dataset.step;S.step=n;document.querySelectorAll("[data-step]").forEach(function(x){x.setAttribute("aria-pressed",String(x===b))});
  S.op={};S.op.bc=n<=3;S.op.motion=n===1;S.op.elements=n===1;S.op.patterns=n===2;S.op.library=n===3;S.op.test=false;S.op.interaction=false;S.op.effects=false;S.op.mcard=n===4;S.op.mpages=n===5;S.op["pg:story"]=n===5;if(n===1){S.op["mt:entrance"]=true;S.op["mo:"+S.sel.motion]=true}
  if(n===1){S.tab="card";S.sel.layer="motion";renderEditor();show("demo","m:"+S.sel.motion)}
  if(n===2){S.tab="card";S.sel.layer="pattern";S.op["pt:"+S.sel.pattern]=true;renderEditor();show("demo","p:"+S.sel.pattern)}
  if(n===3){S.tab="card";renderEditor();show("demo")}
  if(n===4){S.tab="marker";renderEditor();S.view="card";renderPhone(false)}
  if(n===5){S.tab="marker";renderEditor();S.view="story";renderPhone(false)}
  if(window.innerWidth<1000)document.querySelector(".ed").scrollIntoView({behavior:"smooth"})})});
document.getElementById("replay").addEventListener("click",function(){scr.scrollTop=0;renderPhone(false);if(S.focusRef&&S.view==="demo")setTimeout(function(){flashRef(S.focusRef)},60)});