/* The agent's tool manifest with its policies and the tools' runner (AGTOOLS, agTools, agRunTool). */
function agRunTool(tools,name,args){var t=(tools||[]).filter(function(x){return x.name===name})[0],out;try{out=t?t.execute(args||{}):"Error: no tool "+name}catch(e){out="Error: "+e.message}
  return Promise.resolve(out).then(function(o){return typeof o==="string"?o:JSON.stringify(o)})}
function agSearch(q,k){var w=String(q||"").toLowerCase().split(/[^a-zà-ú0-9]+/).filter(function(x){return x.length>2});
  return corpus().map(function(c){var t=(c.title+" "+c.text).toLowerCase(),s=0;w.forEach(function(x){if(t.indexOf(x)>=0)s++});return{c:c,s:s}}).filter(function(x){return x.s>0}).sort(function(a,b){return b.s-a.s}).slice(0,k||3).map(function(x){return x.c})}

/* ----- the manifest: at most 9 tools (round 9 added show_how), every id an enum of real ids, each with its policy (Auto runs it, Suggest and Confirm offer it as a button) ----- */
var AGTOOLS={search_library:{ro:true,label:"Search the library",what:"Searches the point's texts and sources; returns passages with their ids, to cite."},
  find:{ro:true,label:"Find",what:"Finds blocks, story pages and points on the wall by name; returns their ids."},
  show_links:{label:"Link cards",what:"Shows up to 4 tappable cards under the answer, each with a reason. Never moves the visitor by itself."},
  highlight_points:{label:"Light points",what:"Lights the named points on the wall and dims the rest (on the wall)."},
  open_feature:{label:"Open a feature",what:"Opens one of the point's features: the before and after, the gallery, the story, the video."},
  navigate_to:{label:"Go to",what:"Takes the visitor to a block, a story page, or a point (on the wall, the camera turns to it; a point with a card opens it)."},
  make_route:{label:"Route",what:"Draws a numbered route over 2-8 points of the wall, in order."},
  start_tour:{label:"Start the tour",what:"Plays the point's authored tour, in Azu's recorded voice."},
  /* round 9 (_3.8 section 3.4): "how do I…?" about the app, answered from the guidance's own App items (their words) with a Show me card */
  show_how:{ro:true,label:"Show how",what:"Answers a “how do I…” about the app from its own tips (Guide › Azu speaks up: the feature's words) and adds a Show me card under the answer that plays the gesture. Never for the history."}};
function agTools(where,turn){var tg=agTargets(),ids=tg.map(function(t){return t.id}),pois=S.wallPois.map(function(p){return"poi:"+p.id}),fk=agFeatures().map(function(f){return f.kind}),out=[];
  function add(name,desc,schema,check,run,label){var pol=S.agentCfg.pol[name]||"auto",d=AGTOOLS[name];if(pol==="off")return;if(!d.ro&&name!=="show_links"&&!S.agentCfg.actions)return;if(name==="show_links"&&!S.agentCfg.links)return;
    out.push({name:name,description:desc,inputSchema:schema,execute:function(a){a=a||{};var bad=check(a),call={name:name,args:a,ok:!bad,policy:d.ro?"auto":pol};turn.calls.push(call);
      if(bad){call.err=bad;return"Error: "+bad}if(d.ro||pol==="auto")return run(a);
      turn.chips.push({name:name,label:label(a),confirm:pol==="confirm",run:function(){run(a)}});return"Offered to the visitor as a button; they decide. Mention it in one short phrase."}})}
  function only(list,set){var b=(list||[]).filter(function(x){return set.indexOf(x)<0});return b.length?"unknown id "+b.join(", "):!(list||[]).length?"no ids":""}
  add("search_library","Searches this point's library (texts and sources). Returns passages with their ids; cite them as [src:id].",{type:"object",properties:{query:{type:"string"}},required:["query"]},function(a){return a.query?"":"no query"},
    function(a){var r=agSearch(a.query,3);return r.length?r.map(function(c){return{id:c.ref,title:c.title,text:c.text}}):"Nothing in the library."});
  add("find","Finds blocks, story pages and points on the wall whose name matches. Returns ids for show_links or navigate_to.",{type:"object",properties:{query:{type:"string"}},required:["query"]},function(a){return a.query?"":"no query"},
    function(a){var w=String(a.query).toLowerCase(),r=tg.filter(function(t){return(t.title+" "+t.kind).toLowerCase().indexOf(w)>=0||w.split(/\s+/).some(function(x){return x.length>3&&t.title.toLowerCase().indexOf(x)>=0})}).slice(0,5);
      return r.length?r.map(function(t){return{target_id:t.id,title:t.title,kind:t.kind}}):"Nothing found."});
  add("show_links","Shows up to 4 link cards under your answer, each a target and a short reason. Use it when the visitor asks where or what they can see, or when you cannot answer.",
    {type:"object",properties:{items:{type:"array",maxItems:4,items:{type:"object",properties:{target_id:{type:"string",enum:ids},reason:{type:"string"}},required:["target_id"]}}},required:["items"]},
    function(a){return only((a.items||[]).map(function(i){return i.target_id}),ids)},function(a){a.items.slice(0,4).forEach(function(i){if(!turn.links.some(function(l){return l.id===i.target_id}))turn.links.push({id:i.target_id,title:agTitle(i.target_id),reason:i.reason||""})});return"Shown."},function(){return""});
  if(where==="wall"){add("highlight_points","Lights the named points on the wall and dims the rest.",{type:"object",properties:{poi_ids:{type:"array",items:{type:"string",enum:pois}}},required:["poi_ids"]},function(a){return only(a.poi_ids,pois)},
      function(a){camLight(a.poi_ids.map(function(x){return x.slice(4)}));return"Lit."},function(a){return fmt(TXT("ag_chip_light"),a.poi_ids.map(agTitle).join(", "))});
    add("make_route","Draws a numbered route over the points, in this order (2 to 8, each once).",{type:"object",properties:{poi_ids:{type:"array",minItems:2,maxItems:8,items:{type:"string",enum:pois}}},required:["poi_ids"]},
      function(a){return only(a.poi_ids,pois)||((a.poi_ids||[]).length<2?"a route needs 2 points":"")},function(a){G.cam.route=a.poi_ids.map(function(x){return x.slice(4)});camLight(G.cam.route);return"Drawn."},function(){return TXT("ag_chip_route")})}
  add("open_feature","Opens one of this point's features.",{type:"object",properties:{kind:{type:"string",enum:fk}},required:["kind"]},function(a){return fk.indexOf(a.kind)<0?"unknown feature "+a.kind:""},
    function(a){var f=agFeatures().filter(function(x){return x.kind===a.kind})[0];return agGo(f.ref)},function(a){var f=agFeatures().filter(function(x){return x.kind===a.kind})[0];return fmt(TXT("ag_chip_open"),f?L(f.name):a.kind)});
  add("navigate_to","Takes the visitor to a block, a story page or a point (on the wall the camera turns to it).",{type:"object",properties:{target_id:{type:"string",enum:ids}},required:["target_id"]},function(a){return only([a.target_id],ids)},
    function(a){return agGo(a.target_id)},function(a){return fmt(TXT("ag_chip_go"),agTitle(a.target_id))});
  add("start_tour","Plays this point's tour (its written tour, else the one made from its card; its length as the hero's button says it), in Azu's voice.",{type:"object",properties:{}},function(){return""},function(){G.cam.open=false;S.view="card";startTour();return"Started."},function(){return TXT("ag_chip_tour")});
  /* round 9: show_how, the features the App items teach (Guide › Azu speaks up, each item's Teaches): their answer, a Show me card, the item seen */
  var how=gdTeachers(),hf=how.map(function(x){return x.teaches});
  if(hf.length)add("show_how","For a question about how to use the app (\"how do I…\": open a card, ask about the view, drag the before and after, pause the guide…): give the feature. Returns the app's own words for it and puts a Show me card under your answer, which plays the gesture. Never for a question about the history or the place.",
    {type:"object",properties:{feature:{type:"string",enum:hf,description:how.map(function(x){return x.teaches+": "+gdTryWord(x)}).join("; ")}},required:["feature"]},function(a){return hf.indexOf(a.feature)<0?"unknown feature "+a.feature:""},
    function(a){var it=how[hf.indexOf(a.feature)],sh=turn.shows||(turn.shows=[]);if(sh.indexOf(it.id)<0)sh.push(it.id);gdTold(it.id,"Ask Azu, the live agent");return{feature:a.feature,does:gdTryWord(it),says:gdTryHowLine(it),card:"A Show me card is under your answer: say it in one short phrase."}},function(){return""});
  return out}
/* the show_how policy's demo: a scripted "how do I…" answer in the dock's conversation, with its Show me card (no model asked) */
var GD_HOW_DEMO_Q=T("How do I see the before and after?","Como vejo o antes e depois?");
rowDemos({"agentCfg.pol.show_how":{tour:true,dock:"full",act:function(){if(S.agentCfg.pol.show_how==="off"){gdHowAsk(L(GD_HOW_DEMO_Q));gdChip("show_how Off: the live agent has no Show me; the offline answer still matches");return}gdHowAsk(L(GD_HOW_DEMO_Q));gdChip("show_how: a how do I… answer from the app's own tips, with its Show me card")},ringAfter:900,ring:".gdhowc|.gd-thread"}});
