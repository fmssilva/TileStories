/* The agent tested in the prototype: the trace under each answer, every point named, the test scenes with their checks. */
S.agentCfg.trace=true;S.camCfg.names="all";AG.scene=0;AG.sceneRes={};AG.runAll=false;AG.stopAll=false;
/* ----- the trace: what answered, how fast, the tokens, the cost and each tool call, under the answer (never spoken, never in the text) ----- */
function agTrace(turn){var st=turn.st;if(!st)return"";var c=(turn.calls||[]).map(function(k){var a=k.args||{},id=a.target_id||(a.poi_ids||[]).join(",")||a.kind||a.query||(a.items||[]).map(function(i){return i.target_id}).join(",");
    return(k.ok?"✓ ":"✗ ")+k.name+(id?"("+String(id).slice(0,40)+")":"")+(k.ok&&k.policy!=="auto"?" ["+k.policy+"]":"")}).join(" · ");
  return'<div class="agtrace">'+esc(agProvName(st.provider)+" · "+st.model+(st.err?" · "+st.err:"")+" · "+(st.first?st.first+" ms / ":"")+st.ms+" ms · "+st.tin+" in / "+st.tout+" out"+(st.est?" (est.)":"")+(st.cost?" · $"+st.cost.toFixed(4):""))+(c?'<br>'+esc(c):"")+'</div>'}
on("agTurnHtml.html",function(h,turn){if(!S.agentCfg.trace||!turn)return h;turn.vid=turn.vid||(AG.vids=(AG.vids||0)+1);
  return h+agTrace(turn)+'<div class="agtrace agvoice" data-vid="'+turn.vid+'">'+esc(turn.voice?"Voice: "+turn.voice:"")+'</div>'});
/* the voice that speaks the answer, on the card or on the wall (Narration > Speaking now), under it, as it starts */
on("sayNow.after",function(n){var t=[G.agTurn,G.cam.turn].filter(function(x){return x&&n&&x.text===n.text})[0];if(!n||!n.answer||!t)return;t.voice=sayNowText();
  ph.querySelectorAll('.agvoice[data-vid="'+t.vid+'"]').forEach(function(e){e.textContent="Voice: "+t.voice})});
/* every answer keeps its trace, a failed one too (its offline answer says why) */
on("agentAsk.before",function(q,where,ui){var d=ui.done,f=ui.fail;
  ui.done=function(t,turn){turn.st=AG.last;d(t,turn)};
  ui.fail=function(code,turn){turn.st=AG.last||{provider:null,model:"-",ms:0,tin:0,tout:0,err:code};f(code,turn);
    if(where==="wall"){G.cam.turn=turn;camLayer()}else{var m=G.thread[G.thread.length-1];if(m&&m.who==="azu"&&!m.turn){m.turn=turn;renderGuide()}}}});

/* ----- the wall: every point in view named (testing: does the guide name what is on the screen?), or only the middle one ----- */
on("camLayer.after",function(){ph.classList.toggle("pinnames",S.camCfg.names==="all")});
/* a remark about a point that has left the screen (another wall, a pan) goes */
on("fabLayer.before",function(){if(G.fab.bub&&S.stop==="closed"&&!camView().pois.some(function(p){return p.id===G.fab.bub.id}))G.fab.bub=null});
setInterval(function(){if(G.fab.bub&&S.stop==="closed"&&ph.querySelector(".azfabbub")&&!camView().pois.some(function(p){return p.id===G.fab.bub.id}))fabLayer()},400);

/* ----- the test scenes: each puts the prototype where a visitor would be (as by hand: close the card, pick the wall, drag, tap Azu), asks, and checks the answer ----- */
function agCardAsk(){S.view="card";S.stop="full";S.camCfg.wall="ribeira";G.cam.open=false;renderPhone(false);startTour();pauseTour();G.askOpen=true;S.dockH="full";renderGuide()}
function agWall(x,y,z){if(G.on)stopTour();S.camCfg.wall="belem";S.view="card";S.stop="closed";S.camX=x;S.camY=y;S.camZoom=z;G.cam={open:true,answer:"",busy:false,lit:[],route:[],hunt:null};renderPhone(false)}
function agNamed(t){return S.wallPois.filter(function(p){var n=L(p.name).toLowerCase();return n.length>4&&String(t).toLowerCase().indexOf(n)>=0}).map(function(p){return p.id})}
/* on the wall: the points it names must be in view (a point named off the screen is one it cannot see) */
function agSeen(turn,ctx){var inv=(ctx.in_view||[]).map(function(v){return v.id.slice(4)}),named=agNamed(turn.text),out=named.filter(function(id){return inv.indexOf(id)<0}),lit=turn.calls.filter(function(k){return k.ok&&k.name==="highlight_points"}).reduce(function(a,k){return a.concat(k.args.poi_ids.map(function(x){return x.slice(4)}))},[]).filter(function(id){return inv.indexOf(id)<0});
  if(out.length||lit.length)return[false,"named or lit a point not in view: "+out.concat(lit).map(function(id){return agTitle("poi:"+id)}).join(", ")];
  return named.length||turn.cites.some(function(c){return c.indexOf("poi:")===0})?[true,"names "+named.map(function(id){return agTitle("poi:"+id)}).join(", ")]:[false,"names no point in view"]}
function agCalled(turn,name){return turn.calls.some(function(k){return k.ok&&k.name===name})}
var AGSCENES=[
  {id:"opera",label:"Card · the opera",where:"card",q:T("Did the palace have an opera house?","O palácio tinha uma ópera?"),expect:T("From the library (Fun fact cited); Go to the Fun fact.","Da biblioteca (Curiosidade citada); Ir para a Curiosidade."),setup:agCardAsk,
    check:function(t){return t.cites.indexOf("b:b_fun")>=0?[true,"cites "+agTitle("b:b_fun")]:[false,"does not cite "+agTitle("b:b_fun")]}},
  {id:"before",label:"Card · where to see it before 1755",where:"card",q:T("Where can I see the palace as it was before the earthquake?","Onde posso ver como era o palácio antes do terramoto?"),expect:T("A link card to the before and after (or Go to it).","Um cartão para o antes e depois (ou Ir para lá)."),setup:agCardAsk,
    check:function(t){var ok=t.links.some(function(l){return l.id==="b:b_ba"})||t.calls.some(function(k){return k.ok&&(k.args.target_id==="b:b_ba"||k.args.kind==="before_after")});return[ok,ok?"offers the before and after":"no link to the before and after"]}},
  {id:"left",label:"Belém wall · left: what do I see?",where:"wall",q:T("What am I looking at?","O que estou a ver?"),expect:T("Names points in view only (Belém Tower, the houses, the bridge…).","Só nomeia pontos à vista (Torre de Belém, as casas, a ponte…)."),setup:function(){agWall(.18,.5,1.3)},check:agSeen},
  {id:"right",label:"Belém wall · right: the oldest here",where:"wall",q:T("What is the oldest thing here?","O que é mais antigo aqui?"),expect:T("The oldest point in view, by year (the monastery, 1501), and lights it.","O ponto mais antigo à vista, pelo ano (o mosteiro, 1501), e acende-o."),setup:function(){agWall(.97,.4,1.3)},
    check:function(t,c){var o=(c.in_view||[]).filter(function(v){return v.year}).sort(function(a,b){return a.year-b.year})[0];if(!o)return[false,"no dated point in view"];var s=agSeen(t,c);if(!s[0])return s;var id=o.id.slice(4);return agNamed(t.text).indexOf(id)>=0?[true,"names "+o.name+" ("+o.year+")"]:[false,"the oldest in view is "+o.name]}},
  {id:"take",label:"Belém wall · take me to a point off the screen",where:"wall",q:T("Show me the Jerónimos Monastery.","Mostre-me o Mosteiro dos Jerónimos."),expect:T("Go to: the camera turns until the monastery is the middle point.","Ir para: a câmara roda até o mosteiro ser o ponto do meio."),setup:function(){agWall(.1,.5,1.4)},
    check:function(){var c=camView().central;return c&&c.id==="jeronimos"?[true,"the monastery is now in the middle"]:[false,"the middle point is "+(c?c.name:"none")]}},
  {id:"route",label:"Belém wall · a route",where:"wall",q:T("Make me a route through what I see.","Faça-me um percurso pelo que estou a ver."),expect:T("A route over points in view (offered as a button: Route is Suggest).","Um percurso por pontos à vista (oferecido num botão: Percurso está em Sugerir)."),setup:function(){agWall(.5,.5,1)},
    check:function(t,c){var k=t.calls.filter(function(x){return x.ok&&x.name==="make_route"})[0];if(!k)return[false,"no route"];var inv=(c.in_view||[]).map(function(v){return v.id}),off=k.args.poi_ids.filter(function(id){return inv.indexOf(id)<0});return off.length?[false,"the route has points not in view"]:[true,"a route of "+k.args.poi_ids.length+" points in view"]}},
  {id:"photo",label:"Belém wall · with the photo",where:"wall",q:T("Describe this part of the panel in one sentence.","Descreva esta parte do painel numa frase."),expect:T("Describes the picture (needs Photo of the view; Gemini only in the Lab).","Descreve a imagem (precisa de Foto da vista; só Gemini no Lab)."),setup:function(){agWall(.3,.5,1.2);S.agentCfg.photo=true},
    check:function(t,c){S.agentCfg.photo=false;return agSeen(t,c)}}];
function agScene(i){var sc=AGSCENES[i];AG.scene=i;sc.setup();AG.con={q:L(sc.q),text:"",calls:[],st:null,busy:false};renderEditor();emit("agScene.after",sc);return sc}
/* runs a scene as a visitor would ask it (the phone's own Ask), then checks the answer against what the screen showed */
function agRunScene(i){var ans=firstAnswer("agRunScene.answer",i);if(ans!==undefined)return ans;var sc=agScene(i);applyProvider();if(!AG.fn)return Promise.resolve(null);var ctx=agContext(sc.where),t0=AG.log.length;
  (sc.where==="wall"?camAsk:ask)(L(sc.q));
  return new Promise(function(res){var n=0,iv=setInterval(function(){n++;var turn=sc.where==="wall"?G.cam.turn:G.agTurn;if((!AG.con.busy&&turn&&turn.q===L(sc.q))||n>(S.agentCfg.timeout+6)*5){clearInterval(iv);
    setTimeout(function(){var r=turn?sc.check(turn,ctx):[false,"no answer"];if(sc.id==="photo")S.agentCfg.photo=false;AG.sceneRes[sc.id]={ok:r[0],note:r[1],answer:turn?turn.text:"",st:turn&&turn.st,view:(ctx.in_view||[]).map(function(v){return v.name})};agConPaint();res(AG.sceneRes[sc.id])},300)}},200)})}
function agScenesHtml(){var sc=AGSCENES[AG.scene],ctx=agContext(agWhere());
  return pipe("agScenesHtml.html",'<div class="agscenes"><div class="agconb"><select class="u agscsel" data-agscene="1">'+AGSCENES.map(function(s,i){return'<option value="'+i+'"'+(i===AG.scene?" selected":"")+'>'+esc(s.label)+'</option>'}).join("")+'</select>'+
    '<button class="ubtn sm" type="button" data-agsetup="1">Set up</button><button class="ubtn sm" type="button" data-agscrun="1">Set up and ask</button><button class="ubtn sm" type="button" data-agscall="1">'+(AG.runAll?(AG.stopAll?"Stopping…":"Stop"):"Run all scenes")+'</button></div>'+
    '<div class="uses">Asks: “'+esc(L(sc.q))+'” · Expected: '+esc(L(sc.expect))+'</div>'+
    (agWhere()==="wall"?'<div class="uses">In view now: '+esc((ctx.in_view||[]).map(function(v){return v.name+(v.id===ctx.middle?" (middle)":"")}).join(", ")||"none")+'</div>':"")+
    (Object.keys(AG.sceneRes).length?'<table class="tbl agres"><thead><tr><th>Scene</th><th>Answer</th><th>Check</th></tr></thead><tbody>'+AGSCENES.filter(function(s){return AG.sceneRes[s.id]}).map(function(s){var r=AG.sceneRes[s.id];
      return'<tr><td>'+esc(s.label)+'</td><td>'+esc(r.answer||"-")+'</td><td class="'+(r.ok?"ok":"bad")+'">'+(r.ok?"✓ ":"✗ ")+esc(r.note)+(r.st?'<br><span class="uses">'+esc(r.st.model+" · "+r.st.ms+" ms")+'</span>':"")+'</td></tr>'}).join("")+'</tbody></table>':"")+'</div>')}
on("agConHtml.html",function(h){return h.replace('<div class="agcon">','<div class="agcon">'+agScenesHtml())});
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset||!t.dataset.agscene)return;e.stopPropagation();AG.scene=+t.value;agConPaint()},true);
ed.addEventListener("click",function(e){var t=e.target.closest("[data-agsetup],[data-agscrun],[data-agscall]");if(!t)return;e.stopPropagation();
  if(t.dataset.agsetup){agScene(AG.scene);return}
  if(t.dataset.agscrun){agRunScene(AG.scene);return}
  /* Run all scenes, in order; while it runs the button is Stop (the scene running ends, no next one starts) */
  if(AG.runAll){AG.stopAll=true;agConPaint();return}AG.runAll=true;AG.stopAll=false;AG.sceneRes={};agConPaint();
  AGSCENES.reduce(function(pr,s,i){return pr.then(function(){return AG.stopAll?null:agRunScene(i)})},Promise.resolve()).then(function(){AG.runAll=false;AG.stopAll=false;agConPaint()})},true);

/* ----- Editor rows ----- */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"s7agent",row("Trace under each answer",pseg("agentCfg.trace",["true","false"],["On (development)","Off"]),"Under each answer, in small grey type: the model, the time to the first words and in all, the tokens, the cost, and each tool call (✓ valid, ✗ refused, with its policy when not Auto). It is never spoken and never part of the answer's text, so the link cards and buttons stay as they are. Off in the app."),"Spoken answer");
  addRows(d,"s7cam",row("Names on the points",pseg("camCfg.names",["all","middle"],["All in view (testing)","The middle one"]),"All: every point in view shows its name, to check that the guide names what is on the screen. The middle one: as in the app, the others are dots until lit."),"Wall (the Lab)");
  return d.innerHTML});
on("modelSig.sig",function(h){return h+"#pin:"+S.camCfg.names});
/* ----- on a wall that is not the card's own (the Belém test wall), the library is that wall's: its points, not the Ribeira card's texts ----- */
var WALLINFO={belem:T("The Grande Panorama de Lisboa, the Belém stretch: a tiled view of Lisbon's riverside from about 1700, before the 1755 earthquake (Museu Nacional do Azulejo).","O Grande Panorama de Lisboa, o troço de Belém: uma vista em azulejo da margem de Lisboa, de cerca de 1700, antes do terramoto de 1755 (Museu Nacional do Azulejo).")};
function agTestWall(where){return where==="wall"&&S.camCfg.wall!=="ribeira"}
on("agContext.ctx",function(c,where){if(agTestWall(where))c.point=L(WALLINFO[S.camCfg.wall]||T("A test wall","Uma parede de teste")).split(":")[0];return c});
on("agPrompt.text",function(s,where){if(!agTestWall(where))return s;var a=s.indexOf("LIBRARY of this point"),b=s.indexOf("RULES:");if(a<0||b<0)return s;
  var lib="LIBRARY of this wall (it is not the Ribeira Palace; say nothing about the Ribeira Palace unless asked):\n"+L(WALLINFO[S.camCfg.wall]||"")+"\n"+S.wallPois.map(function(p){return'<doc id="poi:'+p.id+'" title="'+L(p.name)+'">'+L(p.name)+(p.year?", "+p.year:"")+'</doc>'}).join("\n")+"\n";
  return s.slice(0,a)+lib+s.slice(b)});
/* a point's name says what is drawn there, not the place it is in: the river runs through the whole panel, so its point is the open water with the ship */
WALLS.belem.pois.forEach(function(p){if(p.id==="tagus")p.name=T("Open water on the Tagus","As águas abertas do Tejo")});
/* spoken text has no markdown: the rule asks for plain text, and emphasis a model still writes is taken out */
on("agPrompt.text",function(s){return s.replace("RULES:\n","RULES:\n- Plain spoken text: no markdown, no lists, no asterisks.\n")});
on("agClean.text",function(t){return String(t||"").replace(/\*{1,2}([^*\n]+)\*{1,2}/g,"$1").replace(/(^|\s)_([^_\n]+)_(?=\s|[.,;!?]|$)/g,"$1$2")});
/* each row's demo: the points named on the wall; a question answered on the card, its trace under it */
rowDemos({"camCfg.names":{wall:"open",ring:".campins"},"agentCfg.trace":{tour:true,dock:"full",act:function(){ask(L(S.guideAsks[0]))},ringAfter:1400,ring:".agtrace|.gd-thread"}});
