/* The Guide tab (edGuide): one place for everything the guide does, on the card, on the wall and at the app's start (_3.8, decision 1).
   Its sections: 0 · Preview & Test, 1 · The tour, 2 · Mascot, 3 · Ask, 4 · Guidance (guidance/editor.js). The guide's modules still make
   and fill their foldouts (s7voice, s7dock, s7mascot…) in the Detail Card's document (edCardDoc, through edCard.html); this tab takes them
   from it by their ids, in GUIDE_SECS's order, and the Detail Card's tab leaves the Guide out (edCard.tab). A new section of the tab
   joins edGuide.html. Unity: the POI Editor's Guide tab (an IMGUI tab of its own beside Detail Card), its foldouts the same classes. */
var GUIDE_SECS=[
  {id:"gt0",n:"0",title:"Preview &amp; Test",sub:"Editor tools, never in the app",badge:["test","TEST"],take:[]},
  {id:"gt1",n:"1",title:"The tour",sub:"its voice, panel, button, captions, focus and the ways in",badge:["new","NEW"],rules:true,
   take:["s7voice","s7vtest","s7dock","s7gbtn","s7cap","s7focus","s7coach","s7tour","s7resume","s7hero","s7block"]},
  {id:"gt2",n:"2",title:"Mascot",sub:"Azu: its look, its size, how it moves and plays",take:["s7mascot"]},
  {id:"gt3",n:"3",title:"Ask",sub:"questions by text or voice, on the card and on the wall; Azu's remarks",badge:["new","NEW"],take:["s7ask","s7cam","s7agent"]}];
/* the Guide tab's button, after Detail Card */
(function(){var tabs=document.querySelector(".utabs"),c=tabs&&tabs.querySelector('[data-tab="card"]');if(!c||tabs.querySelector('[data-tab="guide"]'))return;
  var b=document.createElement("button");b.className="utab gd";b.setAttribute("role","tab");b.type="button";b.dataset.tab="guide";b.setAttribute("aria-selected","false");b.textContent="Guide";c.after(b);
  b.addEventListener("click",function(){S.tab="guide";renderEditor()})})();
function edGuide(){var d=document.createElement("div");d.innerHTML=edCardDoc();var g=d.querySelector('[data-fo="s7"] > .inner'),h="";
  var rules=g&&g.querySelector(":scope > .row");rules=rules&&/Fixed rules/.test(rules.textContent)?rules.outerHTML:"";if(g&&rules)g.querySelector(":scope > .row").remove();
  /* each section takes its foldouts by id; anything left (a foldout added later without a place here) goes to the end of 1 · The tour */
  var parts=GUIDE_SECS.map(function(s){
    var body=s.rules?rules:"";s.take.forEach(function(id){var f=g&&g.querySelector(':scope > [data-fo="'+id+'"]');if(f){f.remove();body+=f.outerHTML}});return body});
  var rest=g?[].map.call(g.children,function(x){return x.outerHTML}).join(""):"";
  GUIDE_SECS.forEach(function(s,i){h+=fo(s.id,sectionTitle(s.n,s.title,s.sub),s.badge,(parts[i]+(s.id==="gt1"?rest:""))||" ",1)});
  h=pipe("edGuide.html",h);
  return h+jsonBox("config.json · guide",guideJson())}
function guideJson(){var a=JSON.parse(JSON.stringify(S.agentCfg||{}));Object.keys(a).forEach(function(k){if(/key/i.test(k))delete a[k]});return{guide:S.guideCfg,view:S.camCfg,agent:a,guidance:S.guidance}}
on("renderEditor.tab",function(){if(S.tab==="guide")return edGuide()});
on("refreshJsonV4.answer",function(){if(S.tab==="guide"){var pre=ed.querySelector(".json pre");if(pre)pre.textContent=JSON.stringify(guideJson(),null,2);return true}});
/* the Detail Card's tab: the Guide is in its own tab now */
on("edCard.tab",function(h){var d=document.createElement("div");d.innerHTML=h;var g=d.querySelector('[data-fo="s7"]');if(g)g.remove();return d.innerHTML});
/* the tab's sections, top level (the self-test's NO DEMO check walks them) */
function guideSecIds(){var d=document.createElement("div");d.innerHTML=edGuide();return[].map.call(d.querySelectorAll(":scope > details[data-fo]"),function(x){return x.dataset.fo})}
/* a word's owner in this tab: its path of foldouts (Languages & Terms' links, goOwner) */
function guideOwns(o){return /^s7/.test(o)||/^gd:/.test(o)}
function guidePath(o){for(var i=0;i<GUIDE_SECS.length;i++)if((GUIDE_SECS[i].take||[]).indexOf(o)>=0)return[GUIDE_SECS[i].id,o];
  var p=firstAnswer("guidePath.answer",o);return p||[o]}
/* the tab's sections have no FOCUS of their own (as the Detail Card's): they render open, and their toggle must never move the phone;
   each foldout inside keeps its own */
