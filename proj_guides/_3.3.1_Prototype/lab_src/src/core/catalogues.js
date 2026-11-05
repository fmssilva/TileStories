/* The catalogues: element kinds and their options (KINDS, OPTS, OPTDEF), motion types and paths, the block library (LIB), the fields per kind; lookups by id and use counts. */
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
var GROUPS=["about","stories","media","listen","play","visit"];
/* content fields per kind (Specific Marker > a page) */
var FIELDS={title:[["text","ltext","Text"]],eyebrow:[["text","ltext","Text"]],heading:[["text","ltext","Text"]],lede:[["text","ltext","Text"]],paragraph:[["text","ltext","Text"]],
 callout:[["title","ltext","Title"],["text","ltext","Text"],["back","ltext","Back (Flip)"]],pull_quote:[["text","ltext","Quote"],["cite","text","Attribution"]],list:[["items","litems","Items (one per line)"]],
 picture:[["img","img","Picture"],["cap","ltext","Caption"],["credit","text","Credit"]],picture_pair:[["imgs","imgs","Pictures (two)"],["cap","ltext","Caption"]],carousel:[["imgs","imgs","Pictures"],["cap","ltext","Caption"]],
 video:[["img","img","Poster (the clip's frame)"],["cap","ltext","Caption"]],audio:[["cap","ltext","Title"]],animation:[["file","text","File"]],before_after:[["before","img","Before"],["after","img","After"]],
 hotspot:[["img","img","Picture"]],model_3d:[["img","img","Model (stand-in picture)"]],panorama:[["img","img","Panorama"]],scrolly:[],map:[],timeline:[],swatches:[],divider:[],spacer:[]};
function styleById(id){for(var k in S.styles){var a=S.styles[k];for(var i=0;i<a.length;i++)if(a[i].id===id)return a[i]}return null}
function kindOfStyle(id){for(var k in S.styles){if(S.styles[k].some(function(s){return s.id===id}))return k}return null}
function defStyle(kind){var a=S.styles[kind]||[];for(var i=0;i<a.length;i++)if(a[i].def)return a[i];return a[0]}
function motionById(id){return byId(S.motions,id)} function effectById(id){return byId(S.effects,id)} function patternById(id){return byId(S.patterns,id)} function interById(id){return byId(S.inter,id)}
function styleUses(id){var n=0;S.patterns.forEach(function(p){p.rows.forEach(function(r){if(r.style===id)n++})});S.story.forEach(function(r){if(!r.pattern&&(r.style===id||(!r.style&&defStyle(r.kind).id===id)))n++});return n}
function motionUses(id){var n=0;for(var k in S.styles)S.styles[k].forEach(function(s){if(s.enter===id||s.scroll===id||s.turn===id)n++});S.card.forEach(function(b){if(b.enter===id)n++});if(S.family===id)n++;return pipe("motionUses.count",n,id)}
function effectUses(id){var n=0;S.styles.divider.forEach(function(s){if(s.o.uses===id)n++});return n}
function patternUses(id){return S.story.filter(function(r){return r.pattern===id}).length+S.card.filter(function(b){return b.pattern===id}).length}
/* ----- demo content of each element: what it shows in the Editor's previews and the demo card (never in the app) ----- */
FIELDS.timeline=[["items","litems","Events (date · words, one per line)"]];FIELDS.scrolly=[["items","litems","Steps (one per line)"]];
FIELDS.before_after=FIELDS.before_after.concat([["blabel","ltext","Before label"],["alabel","ltext","After label"]]);
