/* Guidance tested in the prototype: five scenes in Ask Azu's test console (Set up, Set up and ask, Run all scenes), each putting the
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
