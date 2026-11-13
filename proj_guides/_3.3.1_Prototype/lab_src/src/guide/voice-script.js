/* Every line the guide can say (guideScript), for the voice script (voicebench/export_script.js renders the missing ones) and the Editor's Coverage (Guide › 1 · The mascot › Voice): each line with where it is said, never an id. */
/* round 19 (the developer, 2026-11-13: "when I click the guide it is using the device voice?"): the recorded sets held the 106 lines of
   2026-11-03 (the steps and eleven fixed words); every line written since (the first introduction, the quiz promise, the intro lines, the
   links, Tell me more, the tour's end, Back to the game, the hints, the games' lines) had no file, so it fell through to the device voice.
   The script is now made by the same functions that say each line, for the demo points (the Ribeira wall's four points with a card), in
   each language and for each audience: the tours (written and from the card) with their after-words, deep parts and recaps; the first
   introduction for each control it may name; the try nudges; the games on the tour; the guidance's spoken remarks and invitations; the
   tour's end; asking during a game. A line built from what the visitor does (a guess's distance, a game's counts) or asks (the answers)
   cannot be recorded: it is listed as live. Unity: the POI Editor's Export voice script (the same lines, GuideScript.Collect), rendered by
   voicebench at authoring time; at run time a line with no clip follows MascotProfile.noRecording */
function vsPoints(){var W=typeof WALLS!=="undefined"&&WALLS.ribeira?WALLS.ribeira.pois:S.wallPois;return(W||[]).filter(function(p){return p.card&&POINTS[p.id]})}
function vsName(id,lang){var p=pointOf(id);return String(wordIn(p&&p.hero&&p.hero.title,lang||"en")||id)}
/* the script: [{where, lang, text, live, n}] (n: how many places say the same words); o.lang: one language only */
function guideScript(o){o=o||{};var out=[],seen={},keep={lang:S.lang,aud:S.vis.aud,tm:G.tm,ae:G.atIntroForce},langs=o.lang?[o.lang]:langCodes();
  var add=function(where,lang,text,live){text=String(text==null?"":text).replace(/\s+/g," ").trim();if(!text)return;var k=lang+"|"+text;if(seen[k]){seen[k].n++;return}var x={where:where,lang:lang,text:text,live:!!live,n:1};seen[k]=x;out.push(x)};
  var tx=function(k,l){return nameFill(wordIn(S.tx[k],l),l)};
  try{
    var pts=vsPoints();
    langs.forEach(function(l){S.lang=l;
      /* the first introduction, for each control it may name (the panel's arrow, the mascot's face, the microphone, the ✦, the Ask button), with each point when it says {0} */
      var said={};INTRO_ASKS.forEach(function(a){pts.forEach(function(p){var t=introLine(l,vsName(p.id,l),a);if(!said[t]){said[t]=1;add("The first introduction ("+String(TXP[a]||a).replace(/^Introduction · /,"")+")",l,t)}})});
      /* the tours: the Ribeira Palace's written tour (as each audience hears it) and every point's tour made from its card */
      pts.forEach(function(p){var nm=vsName(p.id),tours=[];
        var w=p.id==="painting"?(atTour("painting").written||(!atIsGen(S.guideSteps)?S.guideSteps:null)):atTour(p.id).written;if(w)tours.push({steps:w,label:"The written tour of "+nm,aud:true});
        tours.push({steps:atSteps(p.id),label:"The tour of "+nm+" (from its card)"});
        tours.forEach(function(T0){(T0.aud?AUDS:[S.vis.aud]).forEach(function(a){S.vis.aud=a;ovMap();var an=T0.aud&&a!=="explorer"?" · "+AUDN[a]:"";
          T0.steps.forEach(function(st,j){var wh=T0.label+" · step "+(j+1)+an,txt=st.text?wordIn(st.text,l):(function(){var r=st.ref&&st.ref.indexOf("r:")===0?byId(S.story,st.ref.slice(2)):null;return r&&r.c&&p.id===(S.m&&S.m.poi||"painting")?wordIn(r.c.text,l):""})();
            if(st.text&&T0.aud)txt=L(st.text);add(wh,l,txt);if(st.after)add(wh+" · after the gesture",l,T0.aud?L(st.after):wordIn(st.after,l));
            /* Tell me more: the deep part, whole (as the first Tell me more of a tour says it) */
            G.tm={said:{},keys:{},done:{}};var d=typeof tmDeepOf==="function"?tmDeepOf(st,l):null;if(d)add(wh+" · Tell me more",l,d.text)});
          /* Continue the tour on a later visit: "Where we were: …" at each step */
          if(typeof svWordsOf==="function")T0.steps.forEach(function(st,j){if(!j||/^(intro|recap)$/.test(st.act))return;var t="";for(var i=j-1;i>=0&&!t;i--)t=atSentences(svWordsOf(T0.steps[i],l)).filter(function(x){return atWords(x)>=5})[0]||"";
            if(t)add(T0.label+" · Continue at step "+(j+1)+" (the recap)",l,fmt(wordIn(S.tx.sv_recap,l),atLowerFirst(t.replace(/[.!?…]+$/,""),l)))})});S.vis.aud=keep.aud;ovMap()});
        /* the tour's end: the mascot's question (After the tour, in turn), the invitations (A tour of {point}?, Continue the tour of {point}?) */
        atIntroLines("after",l).forEach(function(x,i){var t=/\{point\}/.test(x.text)?nameIn(x.text,"{point}",teName(p.id,l),l):x.text;add("The tour's end at "+nm+" (the mascot's question "+(i+1)+")",l,t.charAt(0).toUpperCase()+t.slice(1))});
        var it=S.guidance.items.tourof,mn=tourMinutes(p.id),pn=vsName(p.id,l);if(it){add("A tour of "+nm+"? (the invitation)",l,gdLine(it,false,{point:pn,min:mn}));
          var n=svTour(p.id).length;for(var c=1;c<=n;c++)add("Continue the tour of "+nm+"? (step "+c+" of "+n+")",l,gdLine(it,false,{point:pn,min:mn,cont:c,of:n}))}
        /* asking during a game: Back to the game (in turn) */
        atIntroLines("gameback",l).forEach(function(x,i){add("Back to the game at "+nm+" ("+(i+1)+")",l,/\{point\}/.test(x.text)?nameIn(x.text,"{point}",teName(p.id,l),l):x.text)})});
      /* the try nudges and the scroll's Moving on */
      ["guide_moving_on","guide_nudge_flip_1","guide_nudge_flip_2","guide_nudge_drag_1","guide_nudge_drag_2","guide_nudge_answer_1","guide_nudge_answer_2","guide_try_moveon","guide_after_flip","guide_after_answer","sv_changed"].forEach(function(k){add("Invite to try · "+(TXP[k]||k),l,tx(k,l))});
      /* the guidance's spoken items: the remarks on the wall's points, the seasonal lines, the jokes, the treasure hunt */
      WALLS.ribeira.pois.forEach(function(p){var pn=String(wordIn(p.name,l));["fab_comment_1","fab_comment_2","fab_comment_3"].concat(p.card?["fab_comment_card"]:[]).forEach(function(k){add("The mascot's remark on "+wordIn(p.name,"en")+" ("+(TXP[k]||k)+")",l,nameIn(tx(k,l),"{point}",pn,l))})});
      ["gd_season_antonio","gd_season_xmas","gd_season_1755","mascot_joke_1","mascot_joke_2","mascot_joke_3","mascot_water","mascot_ahh","mascot_hungry","mascot_thanks"].forEach(function(k){if(S.tx[k])add("The mascot speaks up · "+(TXP[k]||k),l,tx(k,l))});
      /* the games on the tour: the quiz (its promise, questions, verdicts, scores), Guess the year, Don't say it!, Find it on the wall, the breather, the cookie */
      ["tour_promise","tour_promise_expert","tour_quiz_start","tour_quiz_few","tour_quiz_saved","tour_right_1","tour_right_2","tour_right_3","tour_right_4","tour_guess","tour_guess_wait","tour_keepgoing","tour_breather","tour_breather_ask","tour_continue","tour_back","tour_cookie",
        "tour_find_found","tour_find_left","tour_find_right","tour_find_up","tour_find_down","tour_find_lost","tour_word_zero","tour_word_all1","tour_word_missed","tour_word_gotcha","tour_word_dream","tour_word_later","ga_off","ga_h3","ga_none"].forEach(function(k){if(S.tx[k])add("Games on the tour · "+(TXP[k]||k)+" ("+ownerName(TXOWN[k]).replace(/^.*› /,"")+")",l,tx(k,l))});
      Object.keys(S.tourQ||{}).forEach(function(id){var q=S.tourQ[id];if(!q)return;add("The quiz · its question at step "+id.replace(/^g/,""),l,wordIn(q.q,l));var why=wordIn(q.why,l);
        (q.a||[]).forEach(function(a,i){if(i!==+q.ok)return;var an=wordIn(a,l);add("The quiz · a wrong answer at step "+id.replace(/^g/,""),l,tx("tour_wrong",l).replace("{answer}",an).replace("{why}",why));add("The quiz · Not sure at step "+id.replace(/^g/,""),l,tx("tour_unsure",l).replace("{answer}",an).replace("{why}",why));
          add("Asking during a game · Tell me at step "+id.replace(/^g/,""),l,fmt(tx("ga_told",l),an,"").trim())})});
      [0,1,2,3].forEach(function(r){var n=3;add("The quiz · the score "+r+" of "+n,l,fmt(tx(r===n?"tour_score_all":r===0?"tour_score_none":r*2>n?"tour_score_most":"tour_score_some",l),r,n))});
      Object.keys(S.tourGuess||{}).forEach(function(id){var g=S.tourGuess[id],a=String(g.n);add("Guess the year · spot on",l,tx("tour_guess_b1",l).replace("{answer}",a));add("Guess the year · older than they thought",l,tx("tour_guess_b3_older",l).replace("{answer}",a));add("Guess the year · later than they thought",l,tx("tour_guess_b3_newer",l).replace("{answer}",a))});
      var ww=typeof gdWordOf==="function"?gdWordOf(l):null;if(ww)add("Don't say it! · its announce",l,nameIn(tx("tour_word",l),"{thing}",ww.w,l));
      var fd=S.tourFind;if(fd&&fd.thing){var th=wordIn(fd.thing,l);add("Find it on the wall · its ask",l,nameIn(tx("tour_find",l),"{thing}",th,l));["tour_find_show","tour_find_edge_left","tour_find_edge_right","tour_find_edge_up","tour_find_edge_down"].forEach(function(k){add("Find it on the wall · "+(TXP[k]||k),l,nameIn(tx(k,l),"{thing}",th,l))})}
      /* live: never recordable */
      add("Answers to the visitor's questions (made as they ask: Ask, What next?, asking during a game)",l,l==="pt"?"(as respostas)":"(the answers)",true);
      add("Guess the year · how far the guess was, Don't say it!'s counts (made from what the visitor did)",l,l==="pt"?"(as contagens do visitante)":"(the visitor's counts)",true)})}
  finally{S.lang=keep.lang;S.vis.aud=keep.aud;G.tm=keep.tm;ovMap()}
  return out}
/* the coverage of the chosen voice: each line recorded or not (a line the set has, by what it says in its language) */
function vsCovered(x,set){return!!(set&&set.files&&set.files[fnvKey(x.lang+"|"+x.text)])}
/* the script is made again only when what it is made from changed (the words, the tours, the cards, the games' data, the guide's settings,
   the audiences' words): the Editor draws the Guide tab often (the self-test's walk at each control) */
function vsSig(){try{return JSON.stringify([S.tx,S.tours,S.guideSteps,S.fullSteps,S.card,S.story,S.tourQ,S.tourGuess,S.tourWord,S.tourFind,S.guideCfg,S.guidance&&S.guidance.items&&S.guidance.items.tourof,S.aud])}catch(e){return String(Math.random())}}
function vsScript(){var k=vsSig();if(!G.vsMemo||G.vsMemo.k!==k)G.vsMemo={k:k,sc:guideScript()};return G.vsMemo.sc}
function vsCoverage(){var set=currentSet(),sc=vsScript(),rec=sc.filter(function(x){return!x.live}),miss=rec.filter(function(x){return!vsCovered(x,set)});return{set:set,all:rec.length,have:rec.length-miss.length,miss:miss,live:sc.filter(function(x){return x.live})}}
/* Coverage, under The voice: "212 of 214 lines recorded", then the lines that would use the device voice (where each is said) */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;if(!innerOf(d,"s7voice"))return d.innerHTML;var c=vsCoverage(),nr=S.guideCfg.noRec==="silent"?"would be silent (words only)":"would use the device voice";
  var list=c.miss.length?'<details class="vsmiss"><summary>'+c.miss.length+' line'+(c.miss.length===1?"":"s")+' '+nr+'</summary><ul>'+c.miss.map(function(x){return'<li><b>'+esc(x.where)+'</b> · '+x.lang.toUpperCase()+' · “'+esc(x.text)+'”</li>'}).join("")+'</ul></details>':'<div class="uses">Every line has its recording.</div>';
  var live='<div class="uses vslive">Live, never recorded: '+esc(c.live.filter(function(x){return x.lang===(langCodes()[0]||"en")}).map(function(x){return x.where}).join("; "))+'.</div>';
  addRows(d,"s7voice",row("Coverage",'<span class="uses vscov">'+(c.set?c.have+" of "+c.all+" lines recorded in "+esc(voiceSetName(c.set)):"No recorded voice on this page: every line "+nr)+(currentSetWhy()?" ("+esc(currentSetWhy())+")":"")+'</span>'+list+live+
    '<span class="atin"><button class="atplay" type="button" data-vsdemo="1" title="Say a line with no recording" aria-label="Say a line with no recording on the phone">&#9656;</button></span>',
    "Every line the guide can say for this wall's points, both languages, every audience (the tours, their after-words, Tell me more, the recaps, the first introduction, the nudges, the games, the remarks and invitations, the tour's end, asking during a game), and how many the chosen voice has recorded. A line not recorded (a new or edited one) follows A line with no recording; render it with voicebench (node export_script.js, then render.py: only what is missing). Live lines (the answers, what is built from the visitor's play) can never be recorded."),"Speaking now");
  return d.innerHTML});
ed.addEventListener("click",function(e){var t=e.target.closest("[data-vsdemo]");if(!t)return;e.stopPropagation();showRowDemo("guideCfg.noRec")},true);
