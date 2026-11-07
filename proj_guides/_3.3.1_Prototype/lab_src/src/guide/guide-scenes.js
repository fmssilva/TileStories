/* The guide's controls tested in the prototype: their set-ups as scenes in Ask Azu's test console (Set up, Set up and ask, Run all scenes), each pressing the controls as a finger does and checking what the phone shows. */
/* a finger on the phone: the pointer events and the click a touch sends, at the middle of an element (or dy below where it went down) */
function gsTouch(el,dy){if(!el)return false;var r=el.getBoundingClientRect(),x=r.left+r.width/2,y=r.top+r.height/2,o=function(yy){return{bubbles:true,cancelable:true,composed:true,pointerId:52,pointerType:"touch",isPrimary:true,clientX:x,clientY:yy}};
  el.dispatchEvent(new PointerEvent("pointerdown",o(y)));if(dy){for(var i=1;i<=6;i++)el.dispatchEvent(new PointerEvent("pointermove",o(y+dy*i/6)));el.dispatchEvent(new PointerEvent("pointerup",o(y+dy)));return true}
  el.dispatchEvent(new PointerEvent("pointerup",o(y)));el.dispatchEvent(new MouseEvent("click",o(y)));return true}
function gsTap(sel){return gsTouch(ph.querySelector(sel))}
function gsWait(ms){return new Promise(function(r){setTimeout(r,ms)})}
/* a target's size on the phone, in the phone's own pixels */
function gsSize(sel){var e=ph.querySelector(sel);if(!e)return[0,0];var q=ph.getBoundingClientRect().height/ph.offsetHeight,r=e.getBoundingClientRect();return[Math.round(r.width/q),Math.round(r.height/q)]}
function gsSpeaking(){return!!(G.utter||G.timer||G.recAudio)}
/* the set-ups: the card at Full, the tour at a step (the dock at a height), or the card scrolled past the hero */
function gsTour(id,h){setWall("ribeira");S.view="card";S.stop="full";S.introSeen=true;renderPhone(false);startTour();var j=S.guideSteps.findIndex(function(s){return s.id===id});if(j>0){stopSpeech();G.i=j;runStep()}S.dockH=h||"mini";renderGuide()}
function gsPast(){setWall("ribeira");S.view="card";S.stop="full";renderPhone(false);scr.scrollTop=900;fabLayer()}
/* the ✦ past the hero, once the card has settled (it may still be rising): set up again until it shows, at most 2 s */
function gsGbtn(){var t0=performance.now();return new Promise(function(res){(function w(){if(ph.querySelector(".gbtn")||performance.now()-t0>2000){res(!!ph.querySelector(".gbtn"));return}if(!G.on){scr.scrollTop=900;fabLayer()}setTimeout(w,150)})()})}
/* waits until the voice reaches word k of the line (or ms pass) */
function gsAtWord(k,ms){var t0=performance.now();return new Promise(function(res){(function w(){if(G.hl&&G.hl.k>=k||performance.now()-t0>ms){res();return}setTimeout(w,60)})()})}
var GSCENES=[
  {id:"g_mini",label:"Guide · the tour at Mini: Pause and Stop",setup:function(){gsTour("g1","mini")},
    q:T("nothing: starts the tour at Mini, presses Pause, then Play","nada: começa a visita em Mini, carrega em Pausa e depois em Continuar"),
    expect:T("Pause and Stop beside the talk button, each at least 44 px; Pause holds the voice, Play goes on.","Pausa e Parar ao lado do botão de falar, cada um com 44 px ou mais; Pausa suspende a voz, Continuar retoma."),
    act:function(){var n=gsSize('.gd-mini [data-g="pause"]'),s=gsSize('.gd-mini [data-g="stop"]');if(!ph.querySelector(".gdock.h-mini"))return[false,"the dock is not at Mini",""];
      if(n[0]<44||s[0]<44)return[false,"Pause or Stop is under 44 px ("+n+" / "+s+")",""];gsTap('.gd-mini [data-g="pause"]');var p1=G.paused;return gsWait(250).then(function(){gsTap('.gd-mini [data-g="play"]');return gsWait(250)}).then(function(){
        var ans="Pause "+n.join("×")+" px, Stop "+s.join("×")+" px; after Pause: "+(p1?"paused":"still playing")+"; after Play: "+(G.paused?"paused":"playing")+".";
        return p1&&!G.paused?[true,"Pause and Stop in sight at Mini; Pause pauses and Play goes on",ans]:[false,"Pause or Play did not act",ans]})}},
  {id:"g_compact",label:"Guide · hidden (Compact), and back",setup:function(){gsTour("g1","mini")},
    q:T("nothing: swipes down on the grabber at Mini, then taps the ✦","nada: arrasta para baixo a pega em Mini e depois toca no ✦"),
    expect:T("The dock hides, the voice goes on, the ✦ shows the ring and a pause; a tap on the ✦ brings the dock back at Mini.","O painel esconde-se, a voz continua, o ✦ mostra o anel e uma pausa; um toque no ✦ traz o painel em Mini."),
    act:function(){return gsWait(500).then(function(){gsTouch(ph.querySelector(".gd-grab"),90);return gsWait(350)}).then(function(){
      var c=[S.dockH,!!ph.querySelector(".gdock"),!!ph.querySelector(".gbtn.tour .gb-ring"),!!ph.querySelector(".gb-pause"),gsSpeaking()&&!G.paused];gsTap(".gbtn.tour");
      return gsWait(250).then(function(){var back=S.dockH==="mini"&&!!ph.querySelector(".gdock.h-mini"),ans="After the swipe: "+c[0]+(c[1]?", the dock still there":", no dock")+(c[2]?", the ✦ ring":"")+(c[3]?", a pause":"")+(c[4]?", the voice goes on":", silent")+"; after the ✦: "+S.dockH+".";
        return c[0]==="compact"&&!c[1]&&c[2]&&c[3]&&c[4]&&back?[true,"Compact hides the dock and the voice goes on; the ✦ brings it back",ans]:[false,"Compact or the way back did not act",ans]})})}},
  {id:"g_stop",label:"Guide · Stop, its toast and Resume",setup:function(){S.guideCfg.stopUndo="toast";gsTour("g3","mini")},
    q:T("nothing: presses Stop mid-sentence, then the toast's Resume","nada: carrega em Parar a meio de uma frase e depois em Retomar"),
    expect:T("No question; “Tour stopped · Resume” for 5 s; Resume goes back to the same step, from the start of the sentence it stopped in.","Sem pergunta; “Visita interrompida · Retomar” durante 5 s; Retomar volta ao mesmo passo, no início da frase onde parou."),
    act:function(){return gsAtWord(12,8000).then(function(){var st=S.guideSteps[G.i].id,nm=stepTitle(S.guideSteps[G.i]),k=G.hl.k,s=sentenceStart(k);gsTap('.gd-mini [data-g="stop"]');var t=ph.querySelector(".gtoast");
      var c=[!G.on,!!t&&t.textContent.indexOf(TXT("guide_stopped"))>=0,!ph.querySelector(".gstopask")];return gsWait(250).then(function(){gsTap('[data-gstop="resume"]');return gsWait(450)}).then(function(){
        var now=S.guideSteps[G.i],k2=G.hl?G.hl.k:-1,ans="Stopped at “"+nm+"”, word "+(k+1)+" (its sentence starts at word "+(s+1)+"); toast "+(c[1]?"shown":"missing")+"; resumed at “"+(now?stepTitle(now):"nothing")+"”, word "+(k2+1)+".";
        return c[0]&&c[1]&&c[2]&&G.on&&now&&now.id===st&&k2>=s&&k2<k?[true,"Stop shows the toast; Resume goes back to the step and the sentence",ans]:[false,"Stop or Resume did not keep the point",ans]})})}},
  {id:"g_prev",label:"Guide · Previous says the sentence again",setup:function(){gsTour("g3","peek")},
    q:T("nothing: presses Previous in the second sentence, then again within 2 s","nada: carrega em Anterior na segunda frase e outra vez antes de 2 s"),
    expect:T("The first tap says the sentence again from its first word (same step); the second goes to the step before.","O primeiro toque repete a frase desde a primeira palavra (o mesmo passo); o segundo vai para o passo anterior."),
    act:function(){return gsAtWord(12,8000).then(function(){var i=G.i,k=G.hl.k,s=sentenceStart(k);gsTap('.gd-ctl [data-g="prev"]');return gsWait(200).then(function(){var i1=G.i,k1=G.hl.k;gsTap('.gd-ctl [data-g="prev"]');
      return gsWait(300).then(function(){var ans="At word "+(k+1)+" (its sentence from word "+(s+1)+"): after Previous, "+(i1===i?"the same step":"another step")+", word "+(k1+1)+"; again: "+(G.i===i-1?"the step before":"“"+stepTitle(S.guideSteps[G.i])+"”")+".";
        return s>0&&i1===i&&k1>=s-1&&k1<k&&G.i===i-1?[true,"Previous says the sentence again; a second tap goes back a step",ans]:[false,"Previous did not replay the sentence, or the second tap did not go back",ans]})})})}},
  {id:"g_menu",label:"Guide · the ✦ and its menu",setup:function(){gsPast()},
    q:T("nothing: scrolls past the hero, taps the ✦, then each item","nada: passa o topo, toca no ✦ e depois em cada opção"),
    expect:T("The ✦ (and Azu beside it) above the bar; its menu: Start the tour, Ask, Guide settings; each item does what it says.","O ✦ (e o Azu ao lado) acima da barra; o seu menu: Começar a visita, Perguntar, Definições do guia; cada opção faz o que diz."),
    act:function(){return gsGbtn().then(function(there){if(!there)return[false,"no ✦ past the hero","the hero's Start in sight: "+heroStartSeen()+"; the card at "+S.stop+"; scrolled "+Math.round(scr.scrollTop)+" px"];gsTap(".gbtn");var items=[].map.call(ph.querySelectorAll(".gbmenu [data-gbgo]"),function(b){return b.dataset.gbgo}),res=[];
      gsTap('[data-gbgo="settings"]');res.push(!!ph.querySelector(".vset .vguide"));S.settingsOpen=false;renderPhone(true);scr.scrollTop=900;fabLayer();
      gsTap(".gbtn");gsTap('[data-gbgo="ask"]');res.push(G.on&&S.dockH==="full"&&!!ph.querySelector(".gd-form"));stopTour();scr.scrollTop=900;fabLayer();
      gsTap(".gbtn");gsTap('[data-gbgo="tour"]');res.push(G.on&&!G.paused);var ans="Menu: "+items.join(", ")+"; settings "+(res[0]?"open at the guide's group":"not open")+"; ask "+(res[1]?"opens the conversation":"no")+"; tour "+(res[2]?"starts":"no")+".";
      return items.join()==="tour,ask,settings"&&res.every(Boolean)?[true,"the ✦'s menu has its three items and each acts",ans]:[false,"an item is missing or does not act",ans]})}},
  {id:"g_azu",label:"Guide · a tap on Azu plays (never the menu)",setup:function(){S.guideCfg.azuTap="play";gsPast()},
    q:T("nothing: taps Azu beside the ✦ once","nada: toca uma vez no Azu ao lado do ✦"),
    expect:T("Azu reacts (a poke); the ✦'s menu stays closed.","O Azu reage (um toque); o menu do ✦ fica fechado."),
    act:function(){return gsGbtn().then(function(){var a=ph.querySelector(".azfab");if(!a)return[false,"no Azu beside the ✦",""];var n=G.pokes||0;gsTouch(a);
      return gsWait(120).then(function(){var r=(ph.querySelector(".azfab").className.match(/r-\w+/)||[""])[0],ans="Pokes "+n+" → "+(G.pokes||0)+(r?", "+r:"")+"; the menu "+(G.fab.menu?"opened":"closed")+".";return(G.pokes||0)===n+1&&!G.fab.menu&&!ph.querySelector(".gbmenu")?[true,"Azu plays and the menu stays closed",ans]:[false,"the tap did not play, or opened the menu",ans]})})}},
  {id:"g_hint",label:"Guide · Azu points at the ✦, once a visit",setup:function(){S.guideCfg.azuTap="play";S.guideCfg.life="lively";G.pokes=0;gsPast();gdResetVisitor()},
    q:T("nothing: pokes Azu four times, then four more","nada: toca no Azu quatro vezes e depois mais quatro"),
    expect:T("After a few pokes Azu points at the ✦ and says “Need anything? Tap ✦.”; it does not say it again in this visit.","Após alguns toques o Azu aponta para o ✦ e diz “Precisa de alguma coisa? Toque em ✦.”; não volta a dizê-lo nesta visita."),
    act:function(){var hint=gdLine(S.guidance.items.gbhint),seen=0,poke=function(n){var p=Promise.resolve();for(var i=0;i<n;i++)p=p.then(function(){gsTouch(ph.querySelector(".azfab"));return gsWait(650)});return p};
      /* the guidance says it (Guide › Guidance › The wall › The ✦): its bubble, once Azu's own (a joke after a poke) has gone; at most 8 s */
      var look=function(){var b=ph.querySelector(".gdbub:not(.gd-out) button[data-gdbub]");return!!b&&b.textContent===hint},wait=function(ms){var t0=performance.now();return new Promise(function(res){(function w(){if(look()||performance.now()-t0>ms){res();return}setTimeout(w,60)})()})},shown=function(){return gdVisitOf(S.gdVisitor,"gbhint").shown};
      return gsGbtn().then(function(){return poke(4)}).then(function(){return wait(8000)}).then(function(){seen=look()?1:0;var pt=!!ph.querySelector(".azfab.point-gb"),n1=shown();return poke(4).then(function(){return gsWait(1300)}).then(function(){var again=shown()>n1;
        var ans="After 4 pokes: "+(seen?"the hint":"no hint")+(pt?", Azu's arm towards the ✦":"")+"; after 4 more: "+(again?"said again":"not again")+".";return seen&&pt&&!again?[true,"the hint comes once, pointing at the ✦",ans]:[false,"the hint is missing, does not point, or came twice",ans]})})}}];
GSCENES.forEach(function(s){s.where="card";AGSCENES.push(s)});
