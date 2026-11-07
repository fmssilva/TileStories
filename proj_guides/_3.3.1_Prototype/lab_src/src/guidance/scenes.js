/* Guidance tested in the prototype: ten scenes in Ask Azu's test console (round 1's five, round 2's five: the 15-minute script, Ignore three,
   Fewer like this, Leave and come back, During a tour) (Set up, Set up and ask, Run all scenes), each putting the
   phone where a visitor would be, acting as a finger does (a tap on the marker, a drag on the wall) and checking the phone. The visit's
   clock runs ×10 or ×20 in a scene, is stepped by hand to ignore a tip for 5 minutes, and runs ×1 for the drag; Azu's remarks wait; the clock and the remarks come back after it. */
/* how much of the tip (its bubble, and Azu beside the marker) covers the words on the wall, in px² (0: nothing) */
function gdOverWords(){var b=ph.querySelector(".gdbub:not(.gd-out)"),az=G.gd.az&&ph.querySelector(".azfab"),mine=[b,az].filter(Boolean).map(gdPR),a=0;
  gdObstacles().filter(function(o){return o.text}).forEach(function(o){mine.forEach(function(m){a+=gdOver(m,o.b)})});return Math.round(a)}
function gdScWait(f,ms){var t0=performance.now();return new Promise(function(res){(function w(){var v=false;try{v=f()}catch(e){}if(v||performance.now()-t0>ms){res(!!v);return}setTimeout(w,50)})()})}
/* a scene's start: the Ribeira wall, a first visit (or one whose opening was said 30 s ago), the clock at its rate, the remarks resting */
function gdScSetup(rate,opened){if(G.on)stopTour();setWall("ribeira");gdStop("a scene");G.gd.demo=false;gdChip(null);G.gd.sc={rate:G.gd.rate,comments:S.camCfg.comments,gbtn:S.guideCfg.gbtn};
  S.camCfg.comments=false;G.fab.bub=null;G.gd.manual=false;G.gd.rate=rate;gdResetVisitor();
  if(opened){var now=gdNow(),vm=gdVM("opening"),em=gdEM("opening");vm.shown=1;vm.last=now-30000;vm.end=now-21000;em.shown=1;S.gdVisitor.visit.lastEnd=now-21000}
  G.gd.movedAt=gdNow();renderPhone(false)}
function gdScDone(r){var k=G.gd.sc;if(k){G.gd.rate=k.rate;G.gd.manual=false;S.camCfg.comments=k.comments;if(S.guideCfg.gbtn!==k.gbtn){S.guideCfg.gbtn=k.gbtn;fabLayer()}G.gd.sc=null}return r}
function gdIs(id){return!!(G.gd.cur&&G.gd.cur.item.id===id&&!G.gd.cur.pull)}
function gdAzuBy(){var az=ph.querySelector(".azfab"),M=gdMarker();if(!az||!M)return null;var A=gdPR(az);return Math.round(Math.hypot((A.l+A.r)/2-M.x,(A.t+A.b)/2-M.y))}
var GDSCENES=[
  {id:"gd_first",label:"Guidance · first visit on the wall: the opening, then Tap a marker",setup:function(){gdScSetup(10,false)},
    q:T("nothing: a first visit on the Ribeira wall, the clock ×10; it waits","nada: uma primeira visita na parede da Ribeira, o relógio ×10; espera"),
    expect:T("The opening message by Azu at the ✦; at least 20 s later (the phone still), Azu goes beside Ribeira Palace, points and rings it: “Tap here.”; nothing over the words.","A mensagem de abertura pelo Azu junto ao ✦; pelo menos 20 s depois (o telemóvel parado), o Azu vai para junto do Paço da Ribeira, aponta e rodeia-o: “Toque aqui.”; nada sobre as palavras."),
    act:function(){var t1=null,line="";return gdScWait(function(){return gdIs("opening")},4000).then(function(a){if(a){t1=G.gd.cur.at;line=G.gd.cur.text}
      return gdScWait(function(){return gdIs("tapmarker")},9000)}).then(function(b){return gdScWait(function(){return false},700).then(function(){
        var gap=b&&t1!=null?Math.round((G.gd.cur?G.gd.cur.at:0)-t1)/1000:null,d=gdAzuBy(),ring=!!ph.querySelector(".gdring"),over=gdOverWords();
        var ans="Opening: "+(line?"“"+line+"”":"not shown")+"; Tap a marker "+(b?gap+" s after it (the visit's clock), Azu "+d+" px from the marker, the ring "+(ring?"on":"missing")+", "+over+" px² over words":"not shown")+".";
        return gdScDone(line&&b&&gap>=20&&d!=null&&d<120&&ring&&over===0?[true,"the opening, then Tap a marker beside the marker, pointing and ringed",ans]:[false,"the opening or Tap a marker did not come as it should",ans])})})}},
  {id:"gd_ignore",label:"Guidance · ignore it: again after 25 s, at most 3",setup:function(){gdScSetup(1,true)},
    q:T("nothing: the opening was said 30 s ago; it ignores Tap a marker for 5 minutes of the visit (the clock stepped ×30)","nada: a abertura foi dita há 30 s; ignora o Toque num marcador durante 5 minutos da visita (o relógio a passos, ×30)"),
    expect:T("Tap a marker three times, each at least 25 s after the last one ended; then not again this visit.","O Toque num marcador três vezes, cada uma pelo menos 25 s depois de a anterior acabar; depois não volta nesta visita."),
    act:function(){var t0=gdNow();G.gd.manual=true;gdChip("×30 · the visit's clock, stepped");return new Promise(function(res){var iv=setInterval(function(){gdAdvance(1000);if(gdNow()-t0>=300000){clearInterval(iv);gdChip(null);res()}},33)}).then(function(){
      var ev=G.gd.log.filter(function(l){return l.t>=t0&&/Tap a marker/.test(l.why)&&(l.what==="shown"||l.what==="ended")}),starts=ev.filter(function(l){return l.what==="shown"}).map(function(l){return l.t}),ends=ev.filter(function(l){return l.what==="ended"}).map(function(l){return l.t});
      var gaps=starts.slice(1).map(function(s,i){return Math.round((s-ends[i])/1000)});
      var ans="Shown "+starts.length+" times, at "+starts.map(function(s){return Math.round((s-t0)/1000)+" s"}).join(", ")+"; the gaps after each end: "+(gaps.join(" s, ")||"none")+(gaps.length?" s":"")+"; in the next "+Math.round((t0+300000-(starts[2]||t0))/1000)+" s, nothing more.";
      return gdScDone(starts.length===3&&gaps.every(function(g){return g>=25})?[true,"again after 25 s, at most 3 a visit",ans]:[false,"the count or the gaps are wrong",ans])})}},
  {id:"gd_card",label:"Guidance · open a card: never again (also next visit)",setup:function(){gdScSetup(20,true)},
    q:T("nothing: taps the marker Azu points at, closes the card, comes back (a new visit), then resets the visitor","nada: toca no marcador para onde o Azu aponta, fecha o cartão, volta (uma nova visita) e depois reinicia o visitante"),
    expect:T("The card opens and Tap a marker is done: not in the new visit (the opening says Welcome back); after Reset the visitor it comes again.","O cartão abre e o Toque num marcador fica feito: não aparece na nova visita (a abertura diz Bem-vindo de volta); depois de reiniciar o visitante volta a aparecer."),
    act:function(){var r={};return gdScWait(function(){return gdIs("tapmarker")},6000).then(function(a){r.shown=a;gsTouch(ph.querySelector("[data-marker]"));r.open=S.stop!=="closed";r.done=gdEverOf(S.gdVisitor,"tapmarker").done;
      S.stop="closed";renderPhone(false);gdNewVisit();var t0=gdNow();return gdScWait(function(){if(gdIs("opening"))r.back=G.gd.cur.text;if(gdIs("tapmarker"))r.again=true;return gdNow()-t0>70000},12000)}).then(function(){
      gdResetVisitor();return gdScWait(function(){return gdIs("tapmarker")},8000)}).then(function(c){r.reset=c;
      var ans="Tap a marker "+(r.shown?"shown":"not shown")+"; the tap "+(r.open?"opened the card":"did not open it")+(r.done?", done":"")+"; the new visit: "+(r.back?"“"+r.back+"”":"no opening")+", Tap a marker "+(r.again?"shown again":"not shown")+"; after Reset the visitor: "+(c?"shown":"not shown")+".";
      return gdScDone(r.shown&&r.open&&r.done&&r.back===TXT("gd_opening_back")&&!r.again&&c?[true,"a card opened retires Tap a marker, also next visit; Reset the visitor brings it back",ans]:[false,"Done when did not hold",ans])})}},
  {id:"gd_pan",label:"Guidance · panning the wall: nothing while it moves",setup:function(){gdScSetup(1,true)},
    q:T("nothing: drags the wall back and forth for 3 s as a finger does, then lets go","nada: arrasta a parede para um lado e para o outro durante 3 s, como um dedo, e depois larga"),
    expect:T("No tip while the wall moves; Tap a marker comes only once the phone has been still 1.2 s.","Nenhuma dica enquanto a parede se move; o Toque num marcador só vem depois de o telemóvel estar parado 1,2 s."),
    act:function(){var during=0,n=0;return new Promise(function(res){var iv=setInterval(function(){fingerDrag(n%2?-10:10,0);n++;if(G.gd.cur&&!G.gd.cur.pull)during++;if(n>=25){clearInterval(iv);res()}},120)}).then(function(){
      var stop=gdNow();return gdScWait(function(){return gdIs("tapmarker")},5000).then(function(b){var wait=b?Math.round(G.gd.cur.at-stop)/1000:null;
        var ans=n+" drags in 3 s: "+(during?during+" ticks with a tip":"no tip")+"; after letting go, Tap a marker "+(b?wait+" s later":"never")+".";
        return gdScDone(!during&&b&&wait>=1.2?[true,"quiet while the phone moves; the tip once it is still 1.2 s",ans]:[false,"a tip came while the phone moved, or too soon after",ans])})})}},
  {id:"gd_off",label:"Guidance · Mascot Off: a plain bubble at the marker",setup:function(){gdScSetup(10,true);S.guideCfg.gbtn="star";fabLayer()},
    q:T("nothing: Guide button = ✦ only (no Azu), the opening said 30 s ago; it waits","nada: Botão do guia = só ✦ (sem Azu), a abertura dita há 30 s; espera"),
    expect:T("Tap a marker as a plain bubble beside the marker, its tail to it, the ring on; no character; nothing over the words.","O Toque num marcador como uma bolha simples junto ao marcador, com a ponta para ele, o anel aceso; sem personagem; nada sobre as palavras."),
    act:function(){return gdScWait(function(){return gdIs("tapmarker")},6000).then(function(a){var b=ph.querySelector(".gdbub"),M=gdMarker(),B=b&&gdPR(b),d=B&&M?Math.round(Math.max(0,B.l-M.x,M.x-B.r)+Math.max(0,B.t-M.y,M.y-B.b)):null;
      var c=[a,!ph.querySelector(".azfab"),!!b&&b.classList.contains("plain"),b&&b.dataset.tail!=="none",!!ph.querySelector(".gdring"),gdOverWords()];
      var ans="Tap a marker "+(a?"shown":"not shown")+"; Azu "+(c[1]?"none":"shown")+"; the bubble "+(c[2]?"plain":"with a character")+", "+d+" px from the marker, its tail "+(c[3]?"to it":"none")+"; the ring "+(c[4]?"on":"off")+"; "+c[5]+" px² over words.";
      return gdScDone(c[0]&&c[1]&&c[2]&&c[3]&&c[4]&&c[5]===0&&d!=null&&d<90?[true,"a plain bubble at the marker, no character",ans]:[false,"Mascot Off did not show a plain bubble at the marker",ans])})}}];
GDSCENES.forEach(function(s){s.where="wall";AGSCENES.push(s)});
/* ----- round 2: five more scenes (ids gd2_*): the 15-minute script, Ignore three, Fewer like this, Leave and come back, During a tour ----- */
/* the visit's clock stepped by hand: ms of the visit, a second each 33 ms of real time (×30); each(now) after every second may act as the visitor */
function gdScRun(ms,each){var t0=gdNow();G.gd.manual=true;return new Promise(function(res){var iv=setInterval(function(){gdAdvance(1000);var stop=each&&each(gdNow()-t0);if(stop||gdNow()-t0>=ms){clearInterval(iv);res(gdNow()-t0)}},33)})}
/* the tips shown since a time (not the demos'): their start, item, kind and the stage then */
function gdScShows(t0){var vi=G.gd.log.map(function(l){return l.what}).lastIndexOf("visit");return G.gd.log.slice(vi+1).filter(function(l){return l.t>=t0&&l.what==="shown"&&!/^\(pulled\)/.test(l.why)}).map(function(l){var it=gdItems().filter(function(x){return l.why.indexOf(x.label.replace(/\{point\}.*/,""))===0})[0];return{t:l.t,id:it&&it.id,label:it?it.label:"?",kind:it&&it.kind,why:l.why}})}
/* a long press on the bubble, as a finger does: down, held 0.7 s, up */
function gdScHold(el){var r=el.getBoundingClientRect(),o={bubbles:true,cancelable:true,composed:true,pointerId:61,pointerType:"touch",isPrimary:true,clientX:r.left+r.width/2,clientY:r.top+r.height/2};
  el.dispatchEvent(new PointerEvent("pointerdown",o));return new Promise(function(res){setTimeout(function(){el.dispatchEvent(new PointerEvent("pointerup",o));el.dispatchEvent(new MouseEvent("click",o));res()},700)})}
var GDSCENES2=[
  {id:"gd2_15min",label:"Guidance · 15 minutes in: App first, then the tour invitation",setup:function(){gdScSetup(1,false)},
    q:T("nothing: a scripted first visit at ×30: it ignores the tips, opens the card at 2:10 (closes it at 2:40), opens Ask about the view at 3:20 (closes it at 3:40), then lets the visit run to 15:00","nada: uma primeira visita encenada a ×30: ignora as dicas, abre o cartão aos 2:10 (fecha aos 2:40), abre Pergunte sobre a vista aos 3:20 (fecha aos 3:40) e deixa a visita correr até aos 15:00"),
    expect:T("The App items first (the opening, Tap a marker, Ask about the view), then, once the stage has grown, A tour of Ribeira Palace; at most 2 a minute; none over a sentence; the Log says each pick and why.","Primeiro os itens da App (a abertura, o Toque num marcador, Pergunte sobre a vista), depois, quando a visita já vai adiantada, a Visita ao Paço da Ribeira; no máximo 2 por minuto; nunca sobre uma frase; o Registo diz cada escolha e porquê."),
    act:function(){var v0=S.gdVisitor.visit.at,t0=v0,over=0,stages={};
      return gdScRun(900000,function(t){var c=G.gd.cur;if(c&&!c.pull&&!stages[c.at]){stages[c.at]=gdStage(S.gdVisitor,c.at).s;if(G.utter||G.timer||G.recAudio)over++}
        if(t===130000){var m=ph.querySelector("[data-marker]");if(m)gsTouch(m)}
        if(t===160000){S.stop="closed";renderPhone(false)}
        if(t===200000){if(G.gd.cur&&G.gd.cur.item.id==="askview"&&!G.gd.cur.pull)gdTap();else gbGo("view")}
        if(t===220000){G.cam={open:false,answer:"",busy:false,lit:[],route:[],hunt:null};renderPhone(false)}}).then(function(){
        var sh=gdScShows(t0),app=sh.filter(function(s){return s.kind==="app"}),con=sh.filter(function(s){return s.kind==="content"});
        var firstC=con[0],appBefore=firstC?app.filter(function(s){return s.t<firstC.t}).length:app.length,sC=firstC?stages[firstC.t]:null;
        var perMin=sh.reduce(function(m,s){return Math.max(m,sh.filter(function(x){return x.t>s.t-60000&&x.t<=s.t}).length)},0),scored=sh.every(function(s){return/score \d/.test(s.why)});
        var ans=sh.length+" tips in 15 min: "+sh.map(function(s){return gdClock(s.t-v0)+" "+s.label+" ("+(s.kind==="app"?"App":"Content")+(s.kind==="content"?", stage s "+gdN(stages[s.t]||0):"")+")"}).join(", ")+"; at most "+perMin+" in any minute; "+over+" over a sentence; "+(scored?"each pick logged with its score":"a pick logged without its score")+".";
        var ok=sh.length>=4&&sh[0].kind==="app"&&!!firstC&&appBefore>=3&&sC>=0.3&&con.every(function(s){return stages[s.t]>=0.3})&&perMin<=+S.guidance.perMinute&&over===0&&scored;
        return gdScDone(ok?[true,"App first, then the tour invitation once the stage had grown (s "+gdN(sC)+"), at most "+perMin+" a minute",ans]:[false,"the order, the stage or a limit was wrong",ans])})}},
  {id:"gd2_ignore3",label:"Guidance · ignore three: the breathing room doubles",setup:function(){gdScSetup(1,true)},
    q:T("nothing: the opening was said 30 s ago; it lets three tips time out in a row (the clock stepped ×30), then taps the next one","nada: a abertura foi dita há 30 s; deixa três dicas esgotarem o tempo seguidas (o relógio a passos, ×30) e depois toca na seguinte"),
    expect:T("After the third ignored tip the breathing room is 30 s (×2) for the visit, and the next tip waits for it; a tap on that tip brings it back to 15 s.","Depois da terceira dica ignorada o intervalo passa a 30 s (×2) nesta visita, e a dica seguinte espera por ele; um toque nessa dica devolve-o aos 15 s."),
    act:function(){var e3=null,why3="",s4=null;return gdScRun(400000,function(){var v=S.gdVisitor.visit;
        if(e3==null&&v.ignoredRun>=3){e3=v.lastEnd;why3=gdPick(gdItems(),S.gdVisitor,gdContext(),gdNow()).why}
        if(e3!=null&&G.gd.cur&&!G.gd.cur.pull){s4=G.gd.cur.at;return true}}).then(function(){var mul=S.gdVisitor.visit.breathMul,b=ph.querySelector(".gdbub button[data-gdbub]"),it=G.gd.cur&&G.gd.cur.item;
        if(b)gsTouch(b);var after=S.gdVisitor.visit.breathMul,br=+S.guidance.breathing,gap=s4!=null&&e3!=null?Math.round((s4-e3)/1000):null;
        var ans="Three ignored: the breathing room ×"+mul+" (the scheduler: “"+(why3.match(/[^·]*breathing room[^·]*/)||[why3])[0].trim()+"”); the next tip ("+(it?it.label:"none")+") "+gap+" s after the third ended; tapped: ×"+after+".";
        return gdScDone(mul===2&&/breathing room 30 s \(×2: 3 ignored in a row\)/.test(why3)&&gap>=2*br&&after===1?[true,"three ignored double the breathing room; an acted tip resets it",ans]:[false,"the breathing room did not double, or a tap did not reset it",ans])})}},
  {id:"gd2_fewer",label:"Guidance · Fewer like this: a long press rests the group",setup:function(){gdScSetup(1,true)},
    q:T("nothing: holds a finger on Tap a marker's bubble, answers Fewer, then lets 5 minutes of the visit pass (×30)","nada: mantém o dedo na bolha do Toque num marcador, responde Menos e deixa passar 5 minutos da visita (×30)"),
    expect:T("A long press asks Fewer like this?; Fewer ends the tip and The start rests for the visit: no Tap a marker for 5 minutes, Why not now? says so; the next visit, it may come again.","Um toque longo pergunta Menos dicas assim?; Menos fecha a dica e O início descansa nesta visita: nenhum Toque num marcador durante 5 minutos, e Porque não agora? di-lo; na visita seguinte pode voltar."),
    act:function(){var r={};return gdScRun(30000,function(){return gdIs("tapmarker")}).then(function(){var b=ph.querySelector(".gdbub button[data-gdbub]");r.shown=!!b;return b?gdScHold(b):null}).then(function(){
        r.ask=!!ph.querySelector(".gdbub [data-gdfewer]")&&(ph.querySelector(".gdfewer").textContent||"").indexOf(TXT("gd_fewer"))>=0;var y=ph.querySelector('[data-gdfewer="yes"]');if(y)gsTouch(y);r.gone=!G.gd.cur;var t0=gdNow();
        return gdScRun(300000,function(){if(gdIs("tapmarker"))r.again=true}).then(function(){r.why=gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{showing:null}),gdNow(),"tapmarker");gdNewVisit();
          r.next=gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{showing:null}),gdNow(),"tapmarker");
          var ans="Tap a marker "+(r.shown?"shown":"not shown")+"; the long press "+(r.ask?"asked Fewer like this?":"asked nothing")+"; Fewer "+(r.gone?"ended it":"left it")+"; 5 min after: "+(r.again?"it came again":"not again")+" (“"+r.why+"”); the next visit: “"+r.next+"”.";
          return gdScDone(r.shown&&r.ask&&r.gone&&!r.again&&/Fewer like this: The start rests for the visit/.test(r.why)&&!/Fewer like this/.test(r.next)?[true,"a long press, Fewer: the group rests for the visit",ans]:[false,"Fewer like this did not rest the group",ans])})})}},
  {id:"gd2_back",label:"Guidance · leave and come back: the returning line, the stage from 5 min",setup:function(){gdScSetup(1,false)},
    q:T("nothing: a first visit; presses Leave for 31 min, comes back; then the same with Remember across visits Off","nada: uma primeira visita; carrega em Sair 31 min e volta; depois o mesmo com Lembrar entre visitas desligado"),
    expect:T("Back after 31 min: a new visit, the opening says Welcome back and the stage starts at 5 min (s 0.33); with Remember Off: a first visit again.","De volta depois de 31 min: uma nova visita, a abertura diz Bem-vindo de volta e a visita começa nos 5 min (s 0,33); com Lembrar desligado: de novo uma primeira visita."),
    act:function(){var r={},keep=S.guidance.remember;return gdScRun(3000,function(){return gdIs("opening")}).then(function(){r.first=G.gd.cur&&G.gd.cur.text;gdLeave(31*60000);
        return gdScRun(3000,function(){return gdIs("opening")})}).then(function(){r.back=G.gd.cur&&G.gd.cur.text;r.visits=S.gdVisitor.visits;r.s=gdStage(S.gdVisitor,gdNow()).s;S.guidance.remember=false;gdLeave(31*60000);
        return gdScRun(3000,function(){return gdIs("opening")})}).then(function(){r.off=G.gd.cur&&G.gd.cur.text;r.visitsOff=S.gdVisitor.visits;S.guidance.remember=keep;
        var ans="First: “"+r.first+"”; back after 31 min: visit "+r.visits+", “"+r.back+"”, the stage s "+gdN(r.s)+"; Remember Off: visit "+r.visitsOff+", “"+r.off+"”.";
        return gdScDone(r.first===TXT("gd_opening")&&r.back===TXT("gd_opening_back")&&r.visits===2&&r.s>=0.33&&r.off===TXT("gd_opening")&&r.visitsOff===1?[true,"a visit ends after 30 min away; the returning visitor's line and stage; Remember Off: a first visit",ans]:[false,"the visit, its line or its stage was wrong",ans])})}},
  {id:"gd2_tour",label:"Guidance · during a tour: only Between steps items, between steps",setup:function(){gdScSetup(30,false);G.gd.sc.tour=S.guidance.items.askview.duringTour;S.guidance.items.askview.duringTour="between"},
    q:T("nothing: Ask about the view set to Between steps for the scene; starts the tour at its flip card (the clock ×30) and watches until the gap between two steps (up to 20 s)","nada: Pergunte sobre a vista posto em Entre passos para a cena; começa a visita guiada no cartão que vira (o relógio ×30) e observa até ao intervalo entre dois passos (até 20 s)"),
    expect:T("Nothing shows during the tour: at a line, never over a sentence; between two steps the Never items (Tap a marker) stay quiet (quiet in a tour) and only Ask about the view gets past the tour (then waits for its ✦, not on the screen during a tour).","Nada aparece durante a visita: numa frase, nunca sobre uma frase; entre dois passos os itens Nunca ficam calados e só Pergunte sobre a vista passa a visita (e espera pelo seu ✦, que não está no ecrã durante a visita)."),
    act:function(){var r={shown:0,line:"",never:"",between:""},keepIntro=S.introSeen,keepMute=S.guideMute;S.introSeen=true;S.guideMute=true;startTour();var j=S.guideSteps.findIndex(function(s){return s.id==="g2"});if(j>0){stopSpeech();G.i=j;runStep()}
      return new Promise(function(res){var t0=performance.now(),iv=setInterval(function(){var c=G.gd.cur;if(c&&!c.pull)r.shown++;var ctx=gdContext(),items=gdItems(),now=gdNow();
        if(ctx.speaking&&!r.line)r.line=gdWhyNot(items,S.gdVisitor,ctx,now,"tapmarker");
        if(ctx.tour&&ctx.tour.between&&!r.never){r.never=gdWhyNot(items,S.gdVisitor,ctx,now,"tapmarker");r.between=gdWhyNot(items,S.gdVisitor,ctx,now,"askview")}
        /* until it has seen a line and the gap between two steps (a slow phone takes longer to reach it), at most 20 s */
        if(performance.now()-t0>20000||r.never&&r.line&&performance.now()-t0>6000){clearInterval(iv);res()}},100)}).then(function(){stopTour();S.introSeen=keepIntro;S.guideMute=keepMute;S.guidance.items.askview.duringTour=G.gd.sc.tour;
        var ans="During the tour: "+r.shown+" ticks with a tip; at a line: “"+r.line+"”; between steps: “"+r.never+"” and “"+r.between+"”.";
        return gdScDone(r.shown===0&&/never over a sentence/.test(r.line)&&/quiet in a tour \(During a tour: Never\)/.test(r.never)&&r.between&&!/quiet in a tour/.test(r.between)?[true,"quiet in a tour; only a Between steps item gets past it, between steps",ans]:[false,"something showed in the tour, or the tour's gate held the wrong items",ans])})}}];
GDSCENES2.forEach(function(s){s.where=s.id==="gd2_tour"?"card":"wall";AGSCENES.push(s)});
