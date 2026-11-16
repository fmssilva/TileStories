/* The agent's test console and its status (agConHtml, agStatus). */
/* ----- Editor: Guide tab > Ask Azu (the live agent) ----- */
function agTok(){return Math.round(corpus().reduce(function(n,c){return n+c.title.length+c.text.length+30},0)/4)}
function agCostQ(){var p=agProvider(),pr=AGPRICE[agModel(p)];if(p==="claude")return"on the viewer's Claude plan";if(!pr)return"-";var tin=(agTok()+700)*2,tout=S.agentCfg.maxOut;return"≈ $"+((tin*pr[0]+tout*pr[1])/1e6).toFixed(4)+" a question"}
function agStatus(){var p=agProvider(),w=window.LABKEYS||{};
  var line=function(n,ok,how){return'<div class="agst"><b class="'+(ok?"ok":"no")+'">'+(ok?"&#10003;":"&#8211;")+'</b> '+esc(n)+' <span class="uses">'+esc(how)+'</span></div>'};
  return line("Claude (the published Lab)",!!AG.claude,AG.claude?"available here, through the artifact (no key)":"only inside the published Lab")+
    line("Gemini key",!!agKey("gemini"),AG.keys.gemini?"pasted in this browser":w.gemini?"from keys.local.js (.secrets/keys.env)":"none on this computer")+
    line("Claude API key",!!agKey("anthropic"),AG.keys.anthropic?"pasted in this browser":w.anthropic?"from keys.local.js (.secrets/keys.env)":"none on this computer")+
    '<div class="agnow">Now answering: <b>'+esc(p?agProvName(p)+" · "+agModel(p):"nothing live here: offline answers")+'</b></div>'}
function agConHtml(){var c=AG.con,st=c.st;
  return pipe("agConHtml.html",'<div class="agcon"><textarea id="agq" rows="2" placeholder="A visitor’s question, e.g. Onde posso ver o palácio antes do terramoto?">'+esc(c.q||"")+'</textarea>'+
    '<div class="agconb"><button class="ubtn sm" type="button" data-agrun="1">'+(c.busy?"Running…":"Run")+'</button><span class="uses">Runs on the phone as it is now ('+(agWhere()==="wall"?"the wall":"the card")+'): the actions happen there.</span></div>'+
    '<div class="agout" aria-live="polite">'+(c.text||c.err?esc(c.text||"")+(c.err?' <span class="bad">('+esc(c.err)+')</span>':""):'<span class="uses">The answer streams here.</span>')+'</div>'+
    (c.calls.length?'<div class="agcalls">'+c.calls.map(function(k){return'<span class="agcall '+(k.ok?"ok":"bad")+'">'+(k.ok?"&#10003; ":"&#10007; ")+esc(k.name)+" "+esc(JSON.stringify(k.args)).slice(0,90)+' · '+esc(k.ok?k.policy:k.err)+'</span>'}).join("")+'</div>':"")+
    (st?'<div class="agmet">'+esc(agProvName(st.provider)+" · "+st.model)+' · first words '+(st.first||"-")+' ms · '+st.ms+' ms · '+st.tin+' in / '+st.tout+' out'+(st.est?" (estimated)":"")+(st.cost?" · $"+st.cost.toFixed(5):"")+(st.err?' · <span class="bad">'+esc(st.err)+'</span>':"")+'</div>':"")+
    '<div class="agconb"><button class="ubtn sm" type="button" data-agsave="1"'+(c.q&&!c.busy?"":" disabled")+'>Save as eval case</button><button class="ubtn sm" type="button" data-agcopy="1"'+(AG.evals.length?"":" disabled")+'>Copy eval cases (JSON)</button><span class="uses">'+AG.evals.length+' saved</span></div></div>')}
function agConPaint(){var e=ed.querySelector(".agcon");if(!e)return;var t=document.createElement("div");t.innerHTML=agConHtml();e.replaceWith(t.firstChild)}
