/* Guidance, the catalog (_3.8 section 3.2): Overall → Kind → Group → Item, as data, in the study's shape (a round adds fields, never
   renames). Overall: Guidance On, the breathing room, at most a minute and a visit, how fast freshness recovers, the learning rules, the
   visitor's long press, Remember across visits. The kinds: App (a bubble, never spoken) with The start (In order: Opening message, Tap a
   marker) and The wall (Ask about the view, The ✦); Content (spoken once the visitor has heard the guide this visit, else a bubble) with
   What's in view (one item a wall point: Azu's remarks, the 400 ms timer they had is gone) and Tours (A tour of {point}). Each kind has
   its On, its cap a visit and its gap; each group its On, its weight, its own cap a visit and its own gap (0: none of its own; a group's
   gap stands in for its kind's for its items); the kinds' weights along the visit are data (round 5: S.guidance.mix, Overall › Mix along the visit, read by gdScore through
   each item's kindMix). Round 5 also adds Talkativeness (one knob over the room, the limits and the kinds' caps: GD_TALK) and the presets
   (GD_PRESETS: each writes the Overall and kind values, never the items; Custom lists its changes). A per-point item
   (perPoint) is one item for each point of the wall in view (gdItems: "The Ribeira Palace in the middle"), each with its own memory, edited
   once. An item's words are in Languages & Terms (S.tx), owned by the item's foldout (gd:i:<id>); the bubble's own words (Fewer like
   this?) by Overall (gd:overall). The events the items wait for or retire on are named in GD_EVENTS (the same names in Unity). Unity:
   GuidanceCatalog, a ScriptableObject (kinds, groups, items; a per-point item expanded from the wall's POI list at load; the words as
   CardStrings keys), read by GuidanceScheduler.
   Round 7: the kind Fun (a bubble only, never spoken; never on a Solemn point: a fixed gate of the kind, no option breaks it) with
   Moments (the water break and the cookie, form Moment: Azu's prop and bubble, between two steps of a tour; their old timers are gone)
   and Play (A joke: after pokes on Azu, or rarely at a pause late in the visit, the least seen variant; {point}'s joke: one item a point
   of the wall, its lines the point's own Jokes in Specific Marker › Guide script, a point with none says none; Treasure hunt?: Family
   only, once a visit, after a card, a tap starts Ask about the view's hunt; Seasonal: one of its lines, the one whose window of dates
   holds today, a Solemn day's line (1 November, 1755) is a remembrance and rests every joke that day). */
S.guidance={on:true,breathing:15,cueRoom:5,perMinute:2,perVisit:30,freshTau:120,learnIgnore:true,learnDismiss:true,fewer:true,remember:true,
  /* round 9: Things to try in the ✦ menu (Overall › The visitor's controls; Settings › The guide always links it) */
  tryMenu:true,
  /* round 5: Talkativeness (Quiet / Balanced / Chatty), the preset last chosen (First visit: these values), the Mix along the visit (each kind's
     weight at s = 0 / 0.3 / 1; Fun's row weighs Fun's items since round 7) */
  talk:"balanced",preset:"first",mix:{app:[70,40,15],content:[25,50,60],fun:[5,10,25]},
  kinds:[{id:"app",name:"App",spoken:"bubble",on:true,cap:20,gap:0,groups:[
      {id:"start",name:"The start",mode:"order",on:true,weight:1,cap:0,gap:0,items:["opening","tapmarker"]},
      {id:"wall",name:"The wall",mode:"all",on:true,weight:1,cap:0,gap:0,items:["askview","gbhint"]},
      /* the card's Hints (round 4): one Cue a hint, motion on its element, no mascot; their moments and looks as before (card/hints.js) */
      {id:"card",name:"The card (cues)",mode:"all",on:true,weight:1,cap:0,gap:0,items:["nudge","label","navbar","ba","strip","term"]},
      /* How to use the card: a Walkthrough, In order, pulled only (Settings, the ✦ menu's How does this work?): never scheduled, never capped */
      {id:"howto",name:"How to use the card",mode:"order",pulled:true,seq:true,on:true,inSettings:true,inMenu:true,items:["wk_grab","wk_settings","wk_bar","wk_locate","wk_block"]},
      /* round 8: the first pages (_3.8 section 6): In order, form Page, at the app's start (launch: never the scheduler's; it is quiet while they
         run); shown in the Editor's Sequences beside How to use the card (seq) */
      {id:"pages",name:"First pages",mode:"order",launch:true,seq:true,on:true,items:["pg_hello","pg_aud","pg_camera","pg_wall"]},
      /* round 9: The guide (_3.8 section 3.1: Pause any time…), pulled only for now: its item is a row of Things to try and an answer of Ask
         Azu's how do I…, never on its own (its push, between two steps of a tour, waits for the developer's call). Last in the list: code that finds a
         group by its place (guidance.kinds.0.groups.N) keeps its N */
      {id:"guide",name:"The guide",mode:"all",pulled:true,on:true,items:["pause"]}]},
    {id:"content",name:"Content",spoken:"heard",on:true,cap:12,gap:60,groups:[
      {id:"inview",name:"What's in view",mode:"all",on:true,weight:1,cap:12,gap:7,items:["inview"]},
      {id:"tours",name:"Tours",mode:"all",on:true,weight:1,cap:0,gap:0,items:["tourof"]}]},
    /* round 7: Fun (play; a bubble only, never spoken; rare: at most 4 a visit, 5 min apart; judged with the simulator: at 6 and 2 min a first
       visit heard 4 Fun lines in 15 min and 16 tips in all, too many). Never on a Solemn point: fixed for the kind (solemnOff), the
       scheduler's own gate, never an option. Moments: today's caps (once a card each, the group 5 min apart); Play: each on its own */
    {id:"fun",name:"Fun",spoken:"bubble",on:true,cap:4,gap:300,solemnOff:true,groups:[
      /* round 12: Fun › On the tour (it took Moments' place: the water break and the cookie moved in). Its own rhythm (the tour's, never
         Fun's visit cap): one beat per 75 s of narration (every, 60-120), at most 3 a tour (perTour; Short 2, Deep 5), the opening and the
         finale included; who gets it (auds: Family, Explorer, Expert). Liveliness is Azu › Liveliness (Calm: only the items Calm too) */
      {id:"ontour",name:"On the tour",mode:"all",on:true,weight:1,cap:0,gap:0,tour:true,life:"azu",every:75,perTour:3,perShort:2,perDeep:5,auds:{family:true,explorer:true,expert:true},items:["quizpromise","quizfinale","guessyear","dontsay","findwall","water","cookie"]},
      {id:"play",name:"Play",mode:"all",on:true,weight:1,cap:0,gap:0,items:["joke","ptjoke","hunt","season"]}]}],
  items:{
    /* Opening message: the visit's first words, beside Azu at the ✦; Essential (skips the breathing room and the limits); once a visit */
    opening:{id:"opening",label:"Opening message",on:true,kind:"app",group:"start",
      says:"gd_opening",saysBack:"gd_opening_back",spoken:"kind",
      about:"nothing",mustSee:true,form:"tip",goes:"stays",points:false,focus:"none",tap:"close",
      when:"visit",screen:"wall",stillFor:0,afterPrev:0,notBefore:0,
      importance:5,essential:true,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",onScreen:"auto",duringTour:"never",along:"kind"},
    /* Tap a marker: the mascot goes beside the nearest marker, points and rings it: "Tap here."; until a card is opened, at most 3 a visit */
    tapmarker:{id:"tapmarker",label:"Tap a marker",on:true,kind:"app",group:"start",
      says:"wall_tip",saysBack:"",spoken:"kind",
      about:"marker",mustSee:true,form:"tip",goes:"beside",points:true,focus:"ring",gesture:true,tap:"close",
      when:"pause",screen:"wall",stillFor:1.2,afterPrev:20,notBefore:0,
      importance:4,essential:false,shows:"until",max:3,again:25,doneWhen:"card",teaches:"card",onScreen:"auto",duringTour:"never",along:"kind"},
    /* Ask about the view: about the ✦ (its menu holds Ask about the view; the wall has no pill of its own): Azu at the ✦ points at it, a ring
       round it; a tap opens Ask about the view; up to 2 a visit, again after 90 s, not before 60 s of the visit, done when it is opened */
    askview:{id:"askview",label:"Ask about the view",on:true,kind:"app",group:"wall",
      says:"gd_askview",saysBack:"",spoken:"kind",
      about:"gbtn",mustSee:true,form:"tip",goes:"stays",points:true,focus:"ring",tap:"view",
      when:"pause",screen:"wall",stillFor:0,afterPrev:0,notBefore:60,
      importance:3,essential:false,shows:"upto",max:2,again:90,doneWhen:"view",teaches:"view",onScreen:"auto",duringTour:"never",along:"kind"},
    /* The ✦ (it was the ✦ hint and its 1 s timer): "Need anything? Tap ✦." wherever the ✦ shows (the wall, or the card past the hero); Azu
       beside it points at it; after 4 pokes on Azu or 25 s idle (no touch, the phone still), Lively only, never on a Solemn point; once a
       visit, done when the ✦ menu is opened; a tap opens the menu. Tap on Azu = Points to the ✦ pulls it at once (gdPull) */
    gbhint:{id:"gbhint",label:"The ✦",on:true,kind:"app",group:"wall",
      says:"gb_hint",saysBack:"",spoken:"kind",
      about:"gbtn",mustSee:true,form:"tip",goes:"stays",points:true,focus:"none",tap:"menu",
      when:"pokes",screen:"any",stillFor:0,afterPrev:0,notBefore:0,pokes:4,idleFor:25,lively:true,notSolemn:true,
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"menu",teaches:"menu",onScreen:"auto",duringTour:"never",along:"kind"},
    /* {point} in the middle (Azu's remarks): one item a point of the wall; when its point settles in the middle 1.2 s (never while
       panning); once a visit for each point; its line: the 3 remarks in turn, or a point with a card its own line; a tap: a point with a
       card starts its tour, any other opens Ask about the view about it */
    inview:{id:"inview",label:"{point} in the middle",on:true,kind:"content",group:"inview",perPoint:true,
      says:["fab_comment_1","fab_comment_2","fab_comment_3"],saysCard:"fab_comment_card",cardLine:true,saysBack:"",spoken:"kind",
      about:"point",mustSee:true,form:"remark",goes:"beside",points:true,focus:"none",tap:"remark",
      when:"settle",screen:"wall",stillFor:1.2,afterPrev:0,notBefore:0,
      importance:2,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",onScreen:"auto",duringTour:"never",along:"kind"},
    /* A tour of {point}: an invitation about the point in the middle (one with a tour); Azu beside it, pointing; a tap starts its tour; once a
       visit, late in the visit, done when a tour step has been heard */
    tourof:{id:"tourof",label:"A tour of {point}",on:true,kind:"content",group:"tours",
      says:"gd_tourof",saysBack:"",spoken:"kind",
      about:"middle",mustSee:true,form:"invitation",goes:"beside",points:true,focus:"none",tap:"tour",
      when:"pause",screen:"wall",stillFor:1.2,afterPrev:0,notBefore:0,
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"tour",teaches:"tour",onScreen:"auto",duringTour:"never",along:"late"},
    /* ----- App › The card (cues): today's six card Hints, form Cue (motion on the element, no mascot, a bubble only where the hint had
       words: the Navigation Bar's tip, the Locate label). about: the element (card/hints.js finds it: cueTargets); when: open = the card
       opening (the nudge at Half; the label), else at a pause with the element in view; shows: once a visit or every card (once on each);
       reduce: skip (a moving cue) or still; Done when: the matching step of How to use the card seen. Their looks: card/hints.js ----- */
    nudge:{id:"nudge",label:"Grabber nudge",on:true,kind:"app",group:"card",form:"cue",says:"",about:"grab",when:"open",screen:"card",reduce:"skip",
      importance:4,essential:false,shows:"visit",max:1,again:0,doneWhen:"wk_grab",teaches:"",duringTour:"never",along:"kind",
      look:"The card opened at Half rises 8 px and settles (0.45 s in, for 0.7 s): the visitor sees it moves."},
    label:{id:"label",label:"Locate label",on:true,kind:"app",group:"card",form:"cue",says:"locate_label",about:"locate",when:"open",screen:"card",reduce:"still",untilSeen:true,
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"wk_locate",teaches:"locate",duringTour:"never",along:"kind",
      look:"“Find it on the wall” beside the pin until it has been seen for 4 s (the time counts only while it has room beside the pin), then it folds into the pin; a tap on the pin counts as read."},
    navbar:{id:"navbar",label:"Navigation Bar",on:true,kind:"app",group:"card",form:"cue",says:"hint_navbar",about:"navbar",when:"pause",screen:"card",reduce:"still",
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"wk_bar",teaches:"navbar",duringTour:"never",along:"kind",
      look:"The bar rings once and “Jump to any part” sits over it for 3 s (under Reduce Motion it fades in and out, no ring travel)."},
    ba:{id:"ba",label:"Before / After",on:true,kind:"app",group:"card",form:"cue",says:"",about:"ba",when:"pause",screen:"card",reduce:"skip",
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"ba",duringTour:"never",along:"kind",
      look:"The first before / after in view sweeps 12 % each way and returns (1.4 s); a touch stops it."},
    strip:{id:"strip",label:"Swipe strips",on:true,kind:"app",group:"card",form:"cue",says:"",about:"strip",when:"pause",screen:"card",reduce:"skip",
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"strip",duringTour:"never",along:"kind",
      look:"The first strip with more to the side slides 40 px and back (1.1 s)."},
    term:{id:"term",label:"Underlined words",on:true,kind:"app",group:"card",form:"cue",says:"",about:"term",when:"pause",screen:"card",reduce:"still",
      importance:2,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",duringTour:"never",along:"kind",
      look:"The first underlined word in view glows once (2 s): a tap explains it."},
    /* ----- App › How to use the card: the walkthrough's steps, in order, run through the tour's steps (ref: the part the step scrolls
       to; sel: the control it marks); each step's line heard is its memory ("seen"), and a seen step retires the cue whose Done when
       names it ----- */
    wk_grab:{id:"wk_grab",label:"The grabber",on:true,kind:"app",group:"howto",form:"walk",says:"how_grab",ref:"hero",sel:"#grab"},
    wk_settings:{id:"wk_settings",label:"The settings mark",on:true,kind:"app",group:"howto",form:"walk",says:"how_settings",ref:"hero",sel:".chrome i:last-child"},
    wk_bar:{id:"wk_bar",label:"The Navigation Bar",on:true,kind:"app",group:"howto",form:"walk",says:"how_bar",ref:"sec:stories",sel:"#navbar"},
    wk_locate:{id:"wk_locate",label:"The Locate Button",on:true,kind:"app",group:"howto",form:"walk",says:"how_locate",ref:"sec:stories",sel:".locpin"},
    wk_block:{id:"wk_block",label:"A block and its page",on:true,kind:"app",group:"howto",form:"walk",says:"how_block",ref:"b:b_story",sel:null},
    /* ----- round 8, App › First pages (_3.8 section 6): the app's first run, in order, form Page (a title, a line, up to 2 buttons, and Skip),
       under 30 s. Hello (Azu forms from a tile and waves; the language as a chip; Let's go, Show me how it works); Who's exploring (the
       audiences' own screen, reused: only when Detail Card › 6 · Audiences › Where to ask = At the app's start); The camera (asked when
       needed: its line, Allow the camera, then the phone's own request; refused: how to allow it, Open Settings, Try again: never the wall
       without the camera); Straight to the wall (no screen: the Opening message says its short line, then Tap a marker as the start's).
       Skip on any page skips the rest of the intro (the camera's own request still comes). Later launches: no pages, Welcome back ----- */
    pg_hello:{id:"pg_hello",label:"Hello",on:true,kind:"app",group:"pages",form:"page",page:"hello",title:"gd_pg_hello_t",says:"gd_pg_hello",
      buttons:[{word:"gd_pg_go",act:"next"},{word:"gd_pg_how",act:"how"}],langChip:true,azu:"form"},
    pg_aud:{id:"pg_aud",label:"Who's exploring",on:true,kind:"app",group:"pages",form:"page",page:"aud",title:"",says:"",buttons:[]},
    pg_camera:{id:"pg_camera",label:"The camera",on:true,kind:"app",group:"pages",form:"page",page:"camera",title:"gd_pg_cam_t",says:"gd_pg_cam",
      buttons:[{word:"gd_pg_cam_allow",act:"camera"}],usage:"gd_pg_cam_usage",offTitle:"gd_pg_off_t",offSays:"gd_pg_off",offSettings:"gd_pg_settings",offRetry:"gd_pg_retry"},
    pg_wall:{id:"pg_wall",label:"Straight to the wall",on:true,kind:"app",group:"pages",form:"page",page:"wall",title:"",says:"gd_pg_wall",buttons:[]},
    /* ----- round 9, App › The guide: Pause the guide, form Try (pulled only: a row of Things to try, an answer of Ask Azu; no bubble). About
       the guide's Pause (the dock's ⏸ in a tour, or the ✦'s at Compact); Show me taps it; done when the visitor pauses the guide ----- */
    pause:{id:"pause",label:"Pause the guide",on:true,kind:"app",group:"guide",form:"try",about:"pause",teaches:"pause",doneWhen:"pause"},
    /* ----- round 12, Fun › On the tour (research_tour_entertainment.md 4.1, 4.3, 4.5, 5.1, 5.3): form Beat, made for the tour; each in
       its tour's plan (gdTourPlan) at a seam: the Opening (after the introduction), Between steps, or the Finale (before the last step).
       The quiz promise and the quiz are one pair (a promise kept: the quiz is promised only when the plan holds both and the point has
       its reviewed questions). The breather was round 7's Water break (the same id): Azu says its line, drinks (the glass beside Azu, by
       date a fan in June-September or a scarf in December-February, never the same prop twice in a visit), then Go on / A question first
       (8 s: Go on). The cookie, a breather variant before the last step (only in a tour without the breather). Guess the year: before
       the step that holds a year (Specific Marker › Guide script: its guess), a slider, Tell me, the answer after that step ----- */
    quizpromise:{id:"quizpromise",label:"The quiz promise",on:true,kind:"fun",group:"ontour",form:"beat",beat:"promise",says:"tour_promise",saysAud:{expert:"tour_promise_expert"},spoken:"kind",
      place:"opening",answers:"chips",waits:4,needs:"quiz",pairs:"quizfinale",scores:false,calm:true,game:true,minSteps:4,qMin:3,expert:"optin",
      importance:4,shows:"every",tourLen:"every"},
    quizfinale:{id:"quizfinale",label:"The quiz",on:true,kind:"fun",group:"ontour",form:"beat",beat:"finale",says:"tour_quiz_start",spoken:"kind",
      place:"finale",answers:"tapsay",waits:0,needs:"quiz",pairs:"quizpromise",scores:true,calm:true,game:true,qn:3,showMe:true,
      importance:4,shows:"every",tourLen:"every"},
    guessyear:{id:"guessyear",label:"Guess the year",on:true,kind:"fun",group:"ontour",form:"beat",beat:"guess",says:"tour_guess",spoken:"kind",
      place:"between",answers:"slider",waits:8,needs:"year",scores:false,calm:true,game:true,
      importance:3,shows:"every",tourLen:"every"},
    water:{id:"water",label:"The breather",on:true,kind:"fun",group:"ontour",form:"beat",beat:"breather",prop:"water",props:"date",says:"tour_breather",spoken:"kind",
      place:"between",answers:"chips",waits:8,talkFor:60,minNarr:90,needs:"",scores:false,calm:false,game:false,
      importance:3,shows:"visit",tourLen:"every"},
    cookie:{id:"cookie",label:"Cookie",on:true,kind:"fun",group:"ontour",form:"beat",beat:"breather",prop:"cookie",props:"fixed",says:"tour_cookie",spoken:"kind",
      place:"between",beforeEnd:true,variantOf:"water",answers:"chips",waits:8,needs:"",scores:false,calm:false,game:false,
      importance:2,shows:"visit",tourLen:"every"},
    /* ----- round 13, Fun › On the tour (research_tour_entertainment.md 4.4, 4.7, 5.4). Don't say it!: In a step (its first use), a game
       inside a span of 1-2 steps: announced at the seam before the span (Let's play / Not now for 4 s; waiting is Let's play), the word
       card pinned beside Azu while the span is told, each slip (the word in the narration) opening a tap window from its start to its end +
       2.5 s; a tap on Azu in a window is a catch (flinch, blush, the pip, never stopping the voice), outside every window a playful
       "Nope!"; the result at the next seam, with the first missed slip played back. Family and Explorer, Lively only, never with a screen
       reader (no button path: the game is not offered). The word per language is the author's (Specific Marker › Guide script: suggested
       from the tour's own text, Reviewed); a language without a reviewed word has no game. Find it on the wall: Between steps, the wall
       only (a tour of a card whose point is on the wall in view), the card lowered, a seek ring in the middle; the point settled in the
       middle 1.2 s is "That's it!"; a hint after 10 s from the point's direction; I give up (and after 20 s) Azu walks to it ----- */
    dontsay:{id:"dontsay",label:"Don't say it!",on:true,kind:"fun",group:"ontour",form:"beat",beat:"word",says:"tour_word",spoken:"kind",
      place:"instep",answers:"tapazu",waits:4,needs:"word",scores:true,calm:false,game:true,srPath:false,auds:{family:true,explorer:true,expert:false},
      tapWin:2.5,falseTaps:"nope",slipBack:true,importance:3,shows:"every",tourLen:"every"},
    findwall:{id:"findwall",label:"Find it on the wall",on:true,kind:"fun",group:"ontour",form:"beat",beat:"find",says:"tour_find",spoken:"kind",
      place:"between",answers:"aim",waits:4,needs:"wall",scores:false,calm:true,game:true,giveUp:20,hintAfter:10,
      importance:3,shows:"every",tourLen:"every"},
    /* ----- Fun › Play. A joke (it was Azu's joke every third poke): its variants Azu's jokes, the least seen shows; after 3 pokes on Azu
       since it last showed (as before), or rarely at a pause late in the visit (from 10:00, once a visit: latePause); Lively only; a
       bubble at the ✦; never in a tour. After pokes it answers the visitor (as Azu's bubble did): the breathing room and Fun's gap neither hold
       it nor start from it; at a pause they do. Its first variant is today's first joke ("Careful! I chip easily.") ----- */
    joke:{id:"joke",label:"A joke",on:true,kind:"fun",group:"play",form:"tip",says:["mascot_joke_2","mascot_joke_3","mascot_joke_1"],pick:"least",joke:true,spoken:"kind",
      about:"nothing",mustSee:false,goes:"stays",points:false,focus:"none",tap:"close",when:"pokes",pokes:3,idleFor:0,latePause:600,screen:"any",lively:true,
      importance:3,essential:false,shows:"every",max:1,again:0,doneWhen:"",teaches:"",onScreen:"auto",duringTour:"never",along:"late"},
    /* {point}'s joke: one item a point of the wall; its lines are the point's own Jokes (Specific Marker › Guide script, 0-3 a language),
       in turn; a point with no joke says none (never a generic one in its place); when its point settles in the middle; once a visit a point;
       Along the visit Late (none in the visit's first third: play comes once the visitor knows the wall) */
    ptjoke:{id:"ptjoke",label:"{point}'s joke",on:true,kind:"fun",group:"play",perPoint:true,ptJokes:true,joke:true,form:"tip",says:[],spoken:"kind",
      about:"point",mustSee:true,goes:"beside",points:true,focus:"none",tap:"close",when:"settle",stillFor:1.2,screen:"wall",lively:true,
      importance:2,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",onScreen:"auto",duringTour:"never",along:"late"},
    /* Treasure hunt? (an invitation): Family only, once a visit, after the visitor has opened a card; "Find {thing} on the wall" ({thing}:
       the point Ask about the view's hunt asks for, camHuntPick); its chip or a tap starts that hunt (G.cam.hunt, no copy); done when a
       treasure is found */
    hunt:{id:"hunt",label:"Treasure hunt?",on:true,kind:"fun",group:"play",form:"invitation",says:"gd_hunt",spoken:"kind",button:"hunt",
      about:"nothing",mustSee:false,goes:"stays",points:false,focus:"none",tap:"hunt",when:"pause",screen:"wall",aud:"family",hasUsed:"card",
      importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"hunt",teaches:"hunt",onScreen:"auto",duringTour:"never",along:"kind"},
    /* Seasonal (one of these): each line has its window of dates (From, To: MM-DD, a window may run over the new year); the one that holds
       today shows (Preview & Test › Today is tries each). A Solemn day's line (1 November: the 1755 earthquake) is a remembrance, never
       a joke: that day no joke shows */
    season:{id:"season",label:"Seasonal",on:true,kind:"fun",group:"play",form:"tip",says:["gd_season_antonio","gd_season_xmas","gd_season_1755"],spoken:"kind",
      seasons:[{says:"gd_season_antonio",from:"06-01",to:"06-30",solemn:false},{says:"gd_season_xmas",from:"12-01",to:"01-06",solemn:false},{says:"gd_season_1755",from:"11-01",to:"11-01",solemn:true}],
      about:"nothing",mustSee:false,goes:"stays",points:false,focus:"none",tap:"close",when:"pause",screen:"wall",notBefore:60,
      importance:2,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",onScreen:"auto",duringTour:"never",along:"kind"}}};
/* each point's own Jokes (Specific Marker › Guide script › Jokes: 0-3 lines a point, each in every language; owner m3): the demo points' */
S.ptJokes={painting:["ptj_painting_1"],torreao:["ptj_torreao_1"],river:["ptj_river_1"]};
/* a point's jokes with words in the visitor's language (a point with none: none) */
function gdPtJokes(id){return(S.ptJokes[id]||[]).filter(function(k){return gdHasWords(k)})}
/* today (Preview & Test › Today is, else the phone's date) as MM-DD; a window of dates holds it (From and To included; one that runs
   over the new year, 12-01 to 01-06, too); the seasonal line whose window holds today */
function gdTodayISO(){var d=new Date();return d.getFullYear()+"-"+("0"+(d.getMonth()+1)).slice(-2)+"-"+("0"+d.getDate()).slice(-2)}
function gdTodayMD(){var t=S.ui&&S.ui.gdToday||gdTodayISO();return String(t).slice(5,10)}
function gdInWindow(md,a,b){if(!a||!b||!md)return false;return a<=b?md>=a&&md<=b:md>=a||md<=b}
function gdSeasonOf(it,md){return(it.seasons||[]).filter(function(x){return gdInWindow(md,x.from,x.to)&&gdHasWords(x.says)})[0]||null}
var GD_MONTHS=["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"];
function gdDayWords(md){var p=String(md||"").split("-");return(+p[1]||"?")+" "+(GD_MONTHS[(+p[0]||1)-1]||"?")}
/* a Solemn day: a seasonal line marked Solemn holds today (no joke that day) */
function gdSolemnDay(md){var sv=null;Object.keys(S.guidance.items).forEach(function(id){var it=S.guidance.items[id];if(it.seasons&&it.on&&!it.deleted)(it.seasons||[]).forEach(function(x){if(x.solemn&&gdInWindow(md,x.from,x.to))sv=x})});return sv}
/* the events (study 3.2): their names in the Lab and in Unity, and the words the Editor reads them back with */
/* the events as a condition in the read-back ("until the visitor opens a card") */
var GD_UNTIL={card:"the visitor opens a card",view:"the visitor opens Ask about the view",tour:"the visitor hears a step of a tour",ask:"the visitor asks a question",menu:"the visitor opens the ✦ menu",hunt:"the visitor finds a treasure",
  /* round 9: the features the card's cues and The guide teach (Things to try's ticks) */
  navbar:"the visitor uses the Navigation Bar",locate:"the visitor taps the Locate Button",ba:"the visitor drags a Before / After",strip:"the visitor swipes a strip",pause:"the visitor pauses the guide"};
var GD_EVENTS={visit:"the visit started",marker:"a marker in view",moving:"the phone moving",still:"the phone still",card:"the visitor opened a card",
  view:"the visitor opened Ask about the view",tour:"a step of the tour was heard",ask:"the visitor asked a question",intro:"the guide's introduction was heard",
  menu:"the visitor opened the ✦ menu",poked:"the visitor poked Azu",idle:"the visitor idle",opened:"the card opened",hunt:"the visitor found the treasure",
  navbar:"the visitor used the Navigation Bar",locate:"the visitor tapped the Locate Button",ba:"the visitor dragged a Before / After",strip:"the visitor swiped a strip",pause:"the visitor paused the guide"};
/* How to use the card's steps as events: a step seen (its line heard) retires the cue whose Done when names it */
["wk_grab","wk_settings","wk_bar","wk_locate","wk_block"].forEach(function(id){var n=S.guidance.items[id].label;GD_EVENTS[id]="the visitor saw How to use the card's step “"+n+"”";GD_UNTIL[id]="the visitor has seen it in How to use the card"});
/* the features used (study 3.2's events the Lab has): the milestones of the stage of the visit */
var GD_FEATURES=[["card","card opened"],["tour","a tour step heard"],["ask","a question asked"],["view","Ask about the view opened"]];
/* the values an item's fields take, in the Editor's words */
var GD_SHOWS=[["once","Once ever"],["visit","Once a visit"],["card","Once a card"],["upto","Up to n a visit"],["until","Until done, at most n a visit"],["every","Every time (Again after only)"]];
var GD_DONE=[["","Never (it does not retire)"],["card","When the visitor opens a card"],["view","When the visitor opens Ask about the view"],["tour","When a step of a tour is heard"],["menu","When the visitor opens the ✦ menu"],["hunt","When the visitor finds a treasure"]];
/* a cue's own values: how often (Once a visit / Every card: once on each card opened) and Done when (a step of How to use the card seen) */
var GD_CUE_SHOWS=[["visit","Once a visit"],["card","Every card"]];
var GD_DONE_CUE=[["","Never"],["wk_grab","Its step: The grabber"],["wk_bar","Its step: The Navigation Bar"],["wk_locate","Its step: The Locate Button"]];
/* where a cue's element must be, in words (the read-back and Why not now?) */
var GD_CUE_WHEN={grab:"when the card opens at Half (the opening's own answer to the tap)",locate:"when the card opens, the Locate Button on the screen (else at the next pause)",
  navbar:"at a pause, when the Navigation Bar shows",ba:"at a pause, when a Before / After is in view",strip:"at a pause, when a strip with more to the side is in view",term:"at a pause, when an underlined word is in view"};
var GD_CUE_NO={grab:"the grabber nudges only as the card opens at Half",locate:"no Locate Button on the screen",navbar:"the Navigation Bar is not showing",
  ba:"no Before / After in view",strip:"no strip with more to the side in view",term:"no underlined word in view"};
var GD_TOUR=[["never","Never"],["between","Between steps"]];
var GD_ALONG=[["kind","As the kind"],["steady","Steady"],["early","Early"],["late","Late"]];
/* the stage of the visit, s from 0 to 1: the kinds' weights at s = 0 / 0.3 / 1 (the Mix along the visit: data, S.guidance.mix, Overall ›
   Mix along the visit), and the items' curves along the visit at the same three points (As the kind: the kind's weight alone; Early: it
   fades after a third; Late: it grows from a third); Steady holds the kind's weight at its middle (s = 0.3) all visit. Straight lines
   between the points. A kind with no row weighs nothing. */
function gdMix(kind,g){var m=(g||S.guidance).mix,r=m&&m[kind];return r?r.map(Number):[0,0,0]}
var GD_MIX_KINDS=[["app","App"],["content","Content"],["fun","Fun"]],GD_MIX_AT=["on arrival","a third in","at the end"];
/* ----- Talkativeness (round 5, _3.8 section 3.3 point 6): one knob over the numbers. Balanced is the rows' own values; Quiet doubles the
   breathing room and halves the limits; Chatty shortens the room to 0.6 and gives the limits half again. The limits: At most a minute, At
   most a visit and each kind's At most a visit. The value in force is the row's × the factor (the room to the second; a limit to a whole
   tip, at least 1; a cap of 0 stays none). Judged with the simulator (Preview & Test › Simulate a visit): with the limits ×0.5 a first
   visit kept its 12 tips in 15 min (its items' own rules bind first: a remark once a visit for each point), only slower; at ×0.4 the
   caps bite (Content 12 → 5: 10 tips, at most 1 a minute). Chatty cannot add what there is not to say: the same tips, sooner (3 a minute).
   Unity: GuidanceCatalog.talkativeness, applied by GuidanceScheduler's state builder ----- */
var GD_TALK={quiet:{name:"Quiet",room:2,limits:0.4},balanced:{name:"Balanced",room:1,limits:1},chatty:{name:"Chatty",room:0.6,limits:1.5}},GD_TALK_IDS=["quiet","balanced","chatty"];
function gdTalk(g){return GD_TALK[(g||S.guidance).talk]||GD_TALK.balanced}
function gdRoomNow(){return Math.round(+S.guidance.breathing*gdTalk().room)}
function gdLimitNow(v){v=+v||0;return v>0?Math.max(1,Math.round(v*gdTalk().limits)):0}
/* the value in force in words, under its row: "In force: 15 s × 2 = 30 s (Quiet)" */
function gdForceWords(v,unit,room){var t=gdTalk(),f=room?t.room:t.limits,n=room?Math.round(+v*f):gdLimitNow(v);return"In force: "+(f===1?v+unit:v+unit+" × "+f+" = "+n+unit)+" ("+t.name+")"}
/* ----- the presets (round 5, _3.8 section 7): each writes the Overall and kind values (Talkativeness, the breathing room, the limits, each
   kind's cap and gap, the Mix, Remember across visits), never the items. One table, read back by the Editor's help. First visit is the
   catalog's own values above (today's); the others, and why:
   - Returning: the app is known (its tips are mostly done), so App weighs less and Content more, a little more room, App capped at 8;
   - Family: a group that moves on fast and likes play: a shorter room (12 s), Content's gap 45 s, more Fun (its row, up to 6 a visit, 3 min apart);
   - Expert: Quiet (the room ×2, the limits ×0.4), App at 40 / 20 / 10 (an expert learns fast), Content at 55 / 70 / 70, its gap 45 s;
   - Quiet and Chatty: First visit with Talkativeness Quiet or Chatty.
   Remember across visits is On in every preset (Off is for a kiosk or a demo: set by hand, which makes it Custom). The audiences choose
   one (Detail Card › 6 · Audiences › Looks per audience › Guidance preset). Unity: GuidancePreset assets (ScriptableObjects) ----- */
var GD_PRESET_IDS=[["first","First visit"],["returning","Returning"],["family","Family"],["expert","Expert"],["quiet","Quiet"],["chatty","Chatty"]];
/* the values a preset writes, read from a settings object (S.guidance, or a copy) */
function gdPresetOf(g){g=g||S.guidance;var k={},m={};g.kinds.forEach(function(x){k[x.id]={cap:+x.cap,gap:+x.gap}});Object.keys(g.mix||{}).forEach(function(x){m[x]=g.mix[x].map(Number)});
  return{talk:g.talk||"balanced",breathing:+g.breathing,perMinute:+g.perMinute,perVisit:+g.perVisit,remember:g.remember!==false,mix:m,kinds:k}}
function gdPresetLike(p,o){var q=JSON.parse(JSON.stringify(p));Object.keys(o).forEach(function(x){q[x]=o[x]});return q}
var GD_PRESETS=(function(){var first=gdPresetOf();return{first:first,
  returning:{talk:"balanced",breathing:20,perMinute:2,perVisit:24,remember:true,mix:{app:[35,20,10],content:[55,65,65],fun:[10,15,25]},kinds:{app:{cap:8,gap:0},content:{cap:14,gap:45},fun:{cap:4,gap:300}}},
  family:{talk:"balanced",breathing:12,perMinute:2,perVisit:30,remember:true,mix:{app:[60,35,15],content:[25,40,45],fun:[15,25,40]},kinds:{app:{cap:20,gap:0},content:{cap:12,gap:45},fun:{cap:6,gap:180}}},
  expert:{talk:"quiet",breathing:15,perMinute:2,perVisit:24,remember:true,mix:{app:[40,20,10],content:[55,70,70],fun:[5,10,20]},kinds:{app:{cap:8,gap:0},content:{cap:16,gap:45},fun:{cap:2,gap:420}}},
  quiet:gdPresetLike(first,{talk:"quiet"}),chatty:gdPresetLike(first,{talk:"chatty"})}})();
/* each preset's why, in words (the Editor's help reads it back) */
var GD_PRESET_WHY={first:"today's values: App first, then Content as the visit grows; a tip at most every 15 s, 2 a minute.",
  returning:"the app is known (its tips are mostly done): App weighs less and Content more, 20 s between two tips, App capped at 8.",
  family:"a group that moves on fast and likes play: 12 s between two tips, Content's gap 45 s, more Fun (15 / 25 / 40, up to 6 a visit, 3 min apart).",
  expert:"Quiet (the room ×2, the limits ×0.4), App at 40 / 20 / 10 (an expert learns fast) and Content at 55 / 70 / 70.",
  quiet:"First visit's values with Talkativeness Quiet: twice the room, the limits ×0.4.",chatty:"First visit's values with Talkativeness Chatty: the room ×0.6, the limits ×1.5.",custom:"values changed by hand."};
function gdPresetName(id){var p=GD_PRESET_IDS.filter(function(x){return x[0]===id})[0];return p?p[1]:id==="custom"?"Custom":id}
/* what differs between two sets of a preset's values, in words ("Breathing room 20 s, was 15 s"): b the values now, a the preset's */
function gdPresetWords(a,b){var w=[],kn={app:"App",content:"Content",fun:"Fun"};
  if(a.talk!==b.talk)w.push("Talkativeness "+(GD_TALK[b.talk]||{}).name+", was "+(GD_TALK[a.talk]||{}).name);
  if(a.breathing!==b.breathing)w.push("Breathing room "+b.breathing+" s, was "+a.breathing+" s");
  if(a.perMinute!==b.perMinute)w.push("At most a minute "+b.perMinute+", was "+a.perMinute);
  if(a.perVisit!==b.perVisit)w.push("At most a visit "+b.perVisit+", was "+a.perVisit);
  Object.keys(b.kinds).forEach(function(k){var x=a.kinds[k]||{},y=b.kinds[k];if(x.cap!==y.cap)w.push((kn[k]||k)+": At most a visit "+y.cap+", was "+x.cap);if(x.gap!==y.gap)w.push((kn[k]||k)+": Gap "+y.gap+" s, was "+x.gap+" s")});
  GD_MIX_KINDS.forEach(function(k){var x=a.mix[k[0]]||[0,0,0],y=b.mix[k[0]]||[0,0,0];[0,1,2].forEach(function(i){if(x[i]!==y[i])w.push("Mix: "+k[1]+" "+GD_MIX_AT[i]+" "+y[i]+" %, was "+x[i]+" %")})});
  if(a.remember!==b.remember)w.push("Remember across visits "+(b.remember?"On":"Off")+", was "+(a.remember?"On":"Off"));return w}
/* the preset in force: the one last chosen while every value is still its own (or another whose values they all are: First visit with
   Talkativeness Quiet is Quiet), else Custom; and Custom's changes in words, from the one last chosen */
function gdPresetDiff(id){var p=GD_PRESETS[id||S.guidance.preset];return p?gdPresetWords(p,gdPresetOf()):["no preset chosen"]}
function gdPresetNow(){var id=S.guidance.preset;if(GD_PRESETS[id]&&!gdPresetDiff(id).length)return id;var m=GD_PRESET_IDS.filter(function(x){return!gdPresetDiff(x[0]).length})[0];return m?m[0]:"custom"}
/* a preset chosen (Overall › Preset, or an audience in Who's exploring): it writes its values, never the items */
function gdPresetWrite(id){var p=GD_PRESETS[id],g=S.guidance;if(!p)return false;g.talk=p.talk;g.breathing=p.breathing;g.perMinute=p.perMinute;g.perVisit=p.perVisit;g.remember=p.remember;
  g.mix=JSON.parse(JSON.stringify(p.mix));g.kinds.forEach(function(k){var v=p.kinds[k.id];if(v){k.cap=v.cap;k.gap=v.gap}});g.preset=id;return true}
var GD_CURVE={kind:[1,1,1],early:[1,1,0],late:[0,0,1]};
function gdCurve(p,s){s=Math.max(0,Math.min(1,+s||0));return s<=0.3?p[0]+(p[1]-p[0])*s/0.3:p[1]+(p[2]-p[1])*(s-0.3)/0.7}
/* the items in the Editor's order (kind, group, item), each a copy with its group's and kind's values (the scheduler reads only its
   arguments): the scheduler's input, its tie-break and an In order group's sequence */
function gdItems(defs){var out=[];S.guidance.kinds.forEach(function(k){k.groups.forEach(function(g){if((g.pulled||g.launch)&&!defs)return;g.items.forEach(function(id){var it=S.guidance.items[id];if(!it)return;
  /* a pulled group (How to use the card) is never the scheduler's: only the Editor's list (defs) holds its steps */
  var o=Object.assign({},it,{groupMode:g.mode,groupName:g.name,groupOn:g.on!==false,groupPulled:!!g.pulled,groupWeight:g.weight==null?1:+g.weight,groupCap:+g.cap||0,groupGap:+g.gap||0,kindName:k.name,kindOn:k.on!==false,kindCap:gdLimitNow(k.cap),kindGap:+k.gap||0,kindSpoken:k.spoken,kindMix:gdMix(k.id),kindSolemnOff:k.solemnOff===true,onTour:!!g.tour,groupLife:g.life||""});
  /* a per-point item: one for each point of the wall in view, its id and label naming the point, each with its own memory */
  if(it.perPoint&&!defs)(S.wallPois||[]).forEach(function(p){var n=L(p.name),x={id:it.id+":"+p.id,base:it.id,point:p.id,pointName:n,card:!!p.card,label:it.label.replace(/\{point\}/g,n)};if(it.ptJokes)x.jokes=gdPtJokes(p.id);out.push(Object.assign({},o,x))});
  else out.push(o)})})});return out}
/* the items as the Editor lists them: one a definition (a per-point item once) */
function gdDefs(){return gdItems(true)}
/* a definition's instances (a per-point item's, one a point; any other: itself) */
function gdInstances(id){return gdItems().filter(function(x){return x.id===id||x.base===id})}
function gdGroupOf(it){var g=null,id=it.base||it.id;S.guidance.kinds.forEach(function(k){k.groups.forEach(function(x){if(x.items.indexOf(id)>=0)g=x})});return g}
/* a group's index path in the state (the Editor's rows: guidance.kinds.<k>.groups.<g>.) */
function gdGroupPath(gid){var p=null;S.guidance.kinds.forEach(function(k,ki){k.groups.forEach(function(g,gi){if(g.id===gid)p="guidance.kinds."+ki+".groups."+gi+"."})});return p}
function gdKindOf(it){return S.guidance.kinds.filter(function(k){return k.id===it.kind})[0]||null}
function gdKindById(id){return S.guidance.kinds.filter(function(k){return k.id===id})[0]||null}
function gdGroupById(id){var g=null;S.guidance.kinds.forEach(function(k){k.groups.forEach(function(x){if(x.id===id)g=x})});return g}
/* the line an item says now: its words in the visitor's language, the returning visitor's line when it has one; with several lines,
   the next in turn (slots.n: how many of its group have shown this visit, so never the same twice in a row) or, for a point with a card,
   its own (saysCard, when Lines per point says so); the slots filled ({point}: the point it is about; {min}: the minutes of that
   point's tour; {button}: the ✦, or the word when the guide's button shows Azu's face or the word) */
/* round 7: the key of the line it says: a seasonal line, the one whose window holds today; a point's joke, its own in turn (none: no line);
   with pick "least" (A joke), the variant this visitor has seen least (ties: the first) */
function gdLineKey(it,returning,slots){slots=slots||{};var a=slots.aud!==undefined?slots.aud:typeof aud==="function"?aud():"",ak=it.saysAud&&it.saysAud[a];
  if(it.seasons){var sv=gdSeasonOf(it,slots.today||gdTodayMD());return sv?sv.says:""}
  if(it.ptJokes){var js=slots.jokes||it.jokes||gdPtJokes(it.point);return js.length?js[(+slots.n||0)%js.length]:""}
  /* round 8: right after the first pages the visit's first words are Straight to the wall's short line (slots.pagesLine: the page's Says) */
  if(slots.pagesLine&&!returning&&gdHasWords(slots.pagesLine))return slots.pagesLine;
  /* round 6: the returning visitor's line, else the audience's own line (when it has words), else Says (its variants in turn) */
  var k=returning&&it.saysBack&&gdHasWords(it.saysBack)?it.saysBack:ak&&gdHasWords(ak)?ak:it.says;
  if(Array.isArray(k)){if(slots.card&&it.saysCard&&it.cardLine!==false)k=it.saysCard;else if(it.pick==="least"&&k.length){var sn=slots.seen||{},best=null;k.forEach(function(x){var c=+(sn[x]||0);if(!best||c<best.c)best={k:x,c:c}});k=best.k}else k=k[(+slots.n||0)%k.length]}
  return k}
/* round 16b (the developer): a tour under 2 minutes is offered as a short one, with no minutes: a line with a {min} slot has its "_short"
   word (gd_tourof_short: "A short tour of {point}?"), used when the tour is under 2 minutes */
function gdLine(it,returning,slots){slots=slots||{};var k=gdLineKey(it,returning,slots);if(!k)return"";if(slots.cont!=null&&gdHasWords(k+"_cont"))k=k+"_cont";else if(slots.min!=null&&+slots.min<2&&gdHasWords(k+"_short"))k=k+"_short";
  return nameIn(nameIn(TXT(k),"{thing}",slots.thing||""),"{point}",slots.point||"").replace(/\{year\}/g,slots.year!=null?slots.year:"").replace(/\{min\}/g,slots.min!=null?slots.min:"").replace(/\{step\}/g,slots.cont!=null?slots.cont:"").replace(/\{of\}/g,slots.of!=null?slots.of:"").replace(/\{button\}/g,S.guideCfg.gbIcon==="star"?"✦":TXT("gb_label")).replace(/\b1 (minute|minuto)s\b/g,"1 $1")}
/* a word key with words in the visitor's language (an empty line falls back to Says) */
function gdHasWords(k){var o=S.tx[k];return!!(o&&String(L(o)||"").trim())}
/* the read-back of an item, in words (study 3.2): "Tap a marker · App › The start · when a marker is in view and the phone is still ·
   at most about every 25 s, up to 3 times a visit, until the visitor opens a card." */
function gdHowOften(it){var n=+it.max,ag=+it.again;if(it.form==="beat")return gdBeatHowOften(it);if(it.form==="walk")return"pulled only: never on its own";if(it.form==="page")return"at the app's start, a first run only";if(it.form==="try")return"pulled only: Things to try and Ask Azu's how do I…";
  var s=it.shows==="once"?"once ever":it.shows==="visit"?"once a visit":it.shows==="card"?"once on each card":it.shows==="upto"?"up to "+n+(n===1?" time":" times")+" a visit":it.shows==="until"?"up to "+n+(n===1?" time":" times")+" a visit":"every time";
  if(ag>0&&it.shows!=="once"&&it.shows!=="visit")s="at most about every "+ag+" s, "+s;
  /* round 7: a joke every n pokes (and rarely once at a pause late) */
  if(it.shows==="every"&&+it.pokes>0)s="every "+it.pokes+" pokes on Azu"+(+it.latePause>0?", or once at a pause late":"");
  if(it.perPoint)s+=" for each point";if(it.doneWhen)s+=", until "+GD_UNTIL[it.doneWhen];return s}
var GD_ABOUT={nothing:"",marker:"when a marker is in view",gbtn:"when the ✦ is on the screen",middle:"when a point with a tour is in the middle",point:"when its point settles in the middle",pause:"when the guide's Pause is on the screen (in a tour)"};
/* round 7: its moment between two tour steps, in words (the water break's narration, the cookie's last step) */
function gdBetweenWords(it){var w=["between two steps of a tour, as it moves on (it holds the next step until a tap or 8 s)"];if(+it.talkFor>0)w.push("after "+it.talkFor+" s of the tour's narration");if(it.beforeEnd)w.push("before the tour's last step");return w.join(", ")}
/* its own When, in words: after n pokes on Azu, or n s idle; Lively only, never on a Solemn point */
function gdWhenWords(it){var w=[],o=[];if(+it.pokes>0)o.push("after "+it.pokes+" pokes on Azu"+(it.kind==="fun"?" since it last showed":""));if(+it.idleFor>0)o.push("after "+it.idleFor+" s idle (no touch, the phone still)");if(+it.latePause>0)o.push("rarely at a pause late in the visit (from "+gdClockW(+it.latePause)+", once a visit)");if(o.length)w.push(o.join(" or "));
  if(it.lively)w.push("Lively only");if(it.notSolemn||gdKindSolemnOff(it))w.push("never on a Solemn point"+(gdKindSolemnOff(it)?" (fixed for Fun)":""));
  if(it.seasons)w.push("only in its window of dates ("+it.seasons.map(function(x){return gdDayWords(x.from)+(x.to!==x.from?"–"+gdDayWords(x.to):"")}).join(", ")+")");
  if(it.ptJokes)w.push("only a point with a joke of its own (Specific Marker › Guide script › Jokes)");if(it.joke)w.push("no joke on a Solemn day");return w}
function gdKindSolemnOff(it){var k=gdKindOf(it);return!!(k&&k.solemnOff)}
function gdClockW(s){s=Math.round(+s||0);return Math.floor(s/60)+":"+("0"+s%60).slice(-2)}
function gdAlongWords(it){var k=gdMix(it.kind),kn=(gdKindOf(it)||{}).name||"?";
  if(it.along==="steady")return"steady: "+kn+"'s "+k[1]+" % all visit";if(it.along==="early")return"early: full for the first third, then fading to nothing";
  if(it.along==="late")return"late: nothing for the first third, then growing to full";return"as the kind ("+kn+": "+k[0]+" % → "+k[1]+" % → "+k[2]+" %)"}
/* How to use the card on the wall (the ✦ menu's How does this work? there): one bubble in the group's words, beside the nearest marker
   (Azu pointing, its ring), and a chip that offers to open a card; a pull, so it writes no memory */
function gdHowWall(){var g=gdGroupById("howto");return{id:"howto:wall",label:"How does this work? (on the wall)",on:true,kind:"app",group:"howto",groupName:g.name,groupMode:"order",groupOn:g.on!==false,groupPulled:true,
  kindName:"App",kindOn:true,kindSpoken:"bubble",says:"gd_how_wall",cta:"gd_how_open",saysBack:"",spoken:"kind",about:"marker",mustSee:false,form:"invitation",goes:"beside",points:true,focus:"ring",tap:"close",
  screen:"wall",duringTour:"never",along:"kind",importance:3,essential:false,shows:"every",max:1,again:0,doneWhen:"",teaches:""}}
/* a cue's read-back: what it is, its moment, how often, Reduce Motion, the room from a tip, Done when */
function gdCueReadBack(it){var g=gdGroupOf(it),r=+S.guidance.cueRoom||0;
  return it.label+" · App › "+(g?g.name:"?")+" · a cue (motion on its element, no mascot"+(it.says?", its own words over it":", no words")+") · "+GD_CUE_WHEN[it.about]+" · "+gdHowOften(it)+
    " · under Reduce Motion "+(it.reduce==="skip"?"skipped (a moving cue)":"shown, with no travel")+" · one at a time with every tip and cue; "+(r>0?"never within "+r+" s of a tip, either way (the cue room)":"no room from a tip (the cue room is 0)")+" · during a tour never."}
/* a step of How to use the card: its place, pulled only, what its line marks and what seeing it retires */
function gdStepReadBack(it){var g=gdGroupOf(it),on=g?g.items.filter(function(id){return S.guidance.items[id].on}):[],i=on.indexOf(it.id),ret=gdDefs().filter(function(x){return x.form==="cue"&&x.doneWhen===it.id}).map(function(x){return x.label});
  return it.label+" · App › How to use the card · "+(it.on?"step "+(i+1)+" of "+on.length:"Off: skipped")+" · pulled only (Settings, the ✦ menu's How does this work?): never scheduled, never capped · "+(it.sel?"it marks "+it.label.replace(/^The /,"the "):"it opens a block's page")+
    " · seen (its line heard): "+(ret.length?"the "+ret.join(", the ")+" is done for good":"nothing to retire")+"."}
function gdReadBack(it){if(it.form==="beat")return gdBeatReadBack(it);if(it.form==="cue")return gdCueReadBack(it)+gdGestureWords(it);if(it.form==="walk")return gdStepReadBack(it);if(it.form==="page")return gdPageReadBack(it);if(it.form==="try")return gdTryReadBack(it);var g=gdGroupOf(it),k=gdKindOf(it),w=[];
  if(it.when==="visit")w.push("once the visit has started");else if(it.when==="between")w.push(gdBetweenWords(it));else if(it.when==="event")w.push("within "+GD_EVENT_MS/1000+" s after "+(GD_EVENTS[it.event]||it.event));
  else if(it.when==="idle"&&!(+it.idleFor>0))w.push("after "+gdIdleOf(it)+" s idle");else if(it.when==="settle"&&it.about!=="point")w.push("when its target settles in the middle");else if(it.when!=="settle")w.push("at a pause");
  if(GD_ABOUT[it.about])w.push(GD_ABOUT[it.about]+(it.mustSee===false?" (or without it)":""));if(+it.stillFor>0)w.push("the phone still "+it.stillFor+" s");
  if(+it.notBefore>0)w.push("not before "+it.notBefore+" s of the visit");w=w.concat(gdWhenWords(it)).concat(gdOnlyWords(it));
  if(g&&g.mode==="order"&&+it.afterPrev>0){var i=g.items.indexOf(it.id),pv=i>0?S.guidance.items[g.items[i-1]]:null;if(pv)w.push("at least "+it.afterPrev+" s after the "+pv.label)}
  var gl=g&&(+g.cap>0||+g.gap>0)?" · "+g.name+" together: "+[+g.cap>0?"at most "+g.cap+" a visit":"",+g.gap>0?"at least "+g.gap+" s apart":""].filter(Boolean).join(", "):"";
  return it.label+" · "+(k?k.name:"?")+" › "+(g?g.name:"?")+(it.essential?" · Essential":"")+" · "+w.join(", ")+" · "+gdHowOften(it)+gl+" · along the visit "+gdAlongWords(it)+
    " · during a tour "+(it.duringTour==="between"?"only between steps":"never")+"."+gdLookWords(it)}
/* ----- round 6 (_3.8 section 3.2, the item in full; section 4, its look). Every tip (not a cue, not a walkthrough step) carries the
   study's field groups: Identity (Label, On, its group and with it its kind), Words (Says: 1-4 variants in turn, never the same twice in
   a row; the slots {point} {year} {button} and {min}; an optional line per audience; a returning visitor's line; Spoken), About (its
   target, Must be on screen), Show (Form; Mascot goes: Beside it / Stays / Walks to it; Points at it; Focus: None / Glint / Ring /
   Spotlight and its dim; Shows the gesture; Button), When (When, its event or idle, the phone still, Only if: screen, audience, first /
   returning, has / has not used, Solemn, Lively; the earliest and latest second of the visit), How much (Importance, Essential, Along the
   visit, Shows, At most, Again after, Done when, Teaches, After a dismissal, Expires, On screen for, During a tour). Their defaults are set
   here once (gdFill), for the catalog's items and for each new one (+ Add guidance's templates, Duplicate). A cue keeps its round-4
   fields (its element fixes its look); Before / After and Swipe strips gain Shows the gesture; a walkthrough step gains its Focus.
   Unity: the same fields on GuidanceItem (GuidanceCatalog); the templates a ScriptableObject list ----- */
var GD_EVENT_MS=20000;
var GD_FOCUS=[["none","None"],["glint","Glint"],["ring","Ring"],["spotlight","Spotlight"]];
var GD_STEP_FOCUS=[["tour","As the tour's Focus"],["spotlight","Spotlight"],["ring","Ring"],["none","None"]];
var GD_GOES=[["beside","Beside it"],["stays","Stays"],["walk","Walks to it"]];
var GD_BUTTONS=[["none","None"],["show","Show me"],["tour","Start the tour"],["card","Open the card"],["view","Ask about the view"],["hunt","Start the hunt"]];
var GD_BTN_WORD={show:"gd_btn_show",tour:"gd_btn_tour",card:"gd_btn_card",view:"gd_btn_view",hunt:"gd_btn_hunt"};
var GD_FORMS=[["tip","Tip"],["invitation","Invitation"],["remark","Remark"],["moment","Moment"]];
/* a Moment's prop (Azu's: the Rive file's states in Unity) */
var GD_PROPS=[["water","A glass of water"],["cookie","A cookie"]];
var GD_WHEN=[["pause","At a pause"],["visit","At the visit's start"],["settle","When its target settles"],["idle","After idle"],["event","On an event"],["between","Between tour steps"]];
var GD_WHEN_EVENTS=[["card","a card opened"],["view","Ask about the view opened"],["tour","a tour step heard"],["ask","a question asked"],["menu","the ✦ menu opened"],["poked","Azu poked"]];
var GD_SPOKEN=[["kind","As the kind"],["bubble","A bubble only"],["heard","Spoken once the guide is heard"]];
var GD_AFTER_DISMISS=[["wait","Wait ×3"],["visit","Not again this visit"],["never","Never again"]];
var GD_EXPIRES=[["target","When its target leaves the screen"],["never","Never (its time only)"]];
var GD_ONSCREEN=[["auto","Auto (by its words)"],["secs","For n s"],["tap","Until tapped"]];
var GD_TEACH=[["","Nothing"],["card","Opening a card"],["view","Ask about the view"],["tour","A tour"],["ask","Asking a question"],["menu","The ✦ menu"],["hunt","The treasure hunt"],
  /* round 9: the card's cues' and The guide's features */
  ["navbar","The Navigation Bar"],["locate","The Locate Button"],["ba","Dragging a Before / After"],["strip","Swiping a strip"],["pause","Pausing the guide"]];
var GD_USED=[["","Any"],["card","a card opened"],["tour","a tour step heard"],["ask","a question asked"],["view","Ask about the view opened"],["menu","the ✦ menu opened"]];
var GD_ABOUT_OPTS=[["nothing","Nothing"],["marker","The nearest marker"],["middle","The point in the middle (with a tour)"],["gbtn","The ✦"]];
var GD_AUD=[["","Any"],["family","Family"],["explorer","Explorer"],["expert","Expert"]];
var GD_VISIT_IS=[["","Any"],["first","A first visit"],["returning","A returning visit"]];
var GD_SCREEN=[["wall","The wall"],["card","A card"],["any","Any"]];
function gdNameOf(t,v){return((t.filter(function(x){return x[0]===v})[0])||[0,v])[1]}
/* the gesture a target asks for (the ghost finger): a tap on a marker, a point or the ✦; a drag across a Before / After; a swipe on a strip */
var GD_GESTURE={marker:"tap",middle:"tap",point:"tap",gbtn:"tap",ba:"drag",strip:"swipe",pause:"tap"};
var GD_GESTURE_WORDS={tap:"a tap",drag:"a drag across",swipe:"a swipe"};
var GD_TIP_DEF={dim:0.4,gesture:false,button:"none",event:"card",pokes:0,idleFor:0,aud:"",visitIs:"",hasUsed:"",notUsed:"",lively:false,notSolemn:false,notAfter:0,prop:"water",talkFor:0,beforeEnd:false,latePause:0,pick:"turn",
  afterDismiss:"wait",expires:"target",onScreen:"auto",onScreenS:6,mustSee:true,spoken:"kind",points:false,focus:"none",goes:"stays",tap:"close",form:"tip",when:"pause",screen:"wall",stillFor:0,afterPrev:0,notBefore:0,
  importance:3,essential:false,shows:"visit",max:1,again:0,doneWhen:"",teaches:"",duringTour:"never",along:"kind",about:"nothing",saysBack:""};
/* round 8: a page's own fields (the Page form): its kind (hello, aud, camera, wall, or a custom page), its title and line, up to 2 buttons
   ({word, act}: next / how), the language chip, Azu (forms from a tile and waves / appears / none) */
var GD_PAGE_DEF={page:"custom",title:"",says:"",buttons:[],langChip:false,azu:"none"};
function gdIsTip(it){return!!it&&it.form!=="cue"&&it.form!=="walk"&&it.form!=="page"&&it.form!=="try"}
/* round 12: a beat's own defaults (Fun › On the tour; the fields below, with the round's data at the end of this file) */
var GD_BEAT_DEF={beat:"breather",place:"between",answers:"chips",waits:8,needs:"",pairs:"",scores:false,calm:false,auds:{family:true,explorer:true,expert:true},tourLen:"every",
  game:false,srPath:true,talkFor:0,beforeEnd:false,minNarr:0,minSteps:0,qMin:3,qn:3,showMe:true,expert:"optin",props:"fixed",variantOf:"",
  tapWin:2.5,falseTaps:"nope",slipBack:true,giveUp:20,hintAfter:10,
  when:"tour",duringTour:"between",screen:"any",about:"nothing",mustSee:false,goes:"stays",tap:"close",lively:false};
function gdFill(it){/* round 9: In Things to try (an App item): Yes when it Teaches a feature; a Try item (pulled only) keeps its own few fields */
  if(it.inTry===undefined&&it.kind==="app"&&it.form!=="walk"&&it.form!=="page")it.inTry=!!it.teaches;if(it.form==="try")return it;
  if(it.form==="cue"){if(GD_GESTURE[it.about]&&it.gesture==null)it.gesture=true;return it}if(it.form==="walk"){if(it.focus==null)it.focus="tour";return it}
  /* round 8: a page keeps its own fields (GD_PAGE_DEF), never a tip's */
  if(it.form==="page"){Object.keys(GD_PAGE_DEF).forEach(function(k){if(it[k]===undefined)it[k]=JSON.parse(JSON.stringify(GD_PAGE_DEF[k]))});return it}
  /* round 12: a beat of the tour gets its own fields first (GD_BEAT_DEF: place, answers, waits, needs, pairs, scores, calm too,
     audiences, tour length), then the tip's */
  if(it.form==="beat")Object.keys(GD_BEAT_DEF).forEach(function(k){if(it[k]===undefined)it[k]=JSON.parse(JSON.stringify(GD_BEAT_DEF[k]))});
  Object.keys(GD_TIP_DEF).forEach(function(k){if(it[k]===undefined)it[k]=GD_TIP_DEF[k]});if(!it.saysAud)it.saysAud={};return it}
Object.keys(S.guidance.items).forEach(function(id){gdFill(S.guidance.items[id])});
/* Spotlight is kept for Essential items and walkthrough steps (or it loses its force), and needs a target to cut out */
function gdSpotOk(it){return!!it&&(it.form==="walk"||!!it.essential&&it.about!=="nothing")}
function gdSpotWhy(it){return!it||it.form==="walk"?"":!it.essential?"Spotlight is kept for Essential items and walkthrough steps: this item is not Essential":it.about==="nothing"?"Spotlight needs a target to cut out: this item is about Nothing":""}
/* the Focus in force: a Spotlight that is not allowed shows as a Ring (the Editor says so, beside the field and in the read-back) */
function gdFocusNow(it){return it.focus==="spotlight"&&!gdSpotOk(it)?"ring":it.focus||"none"}
function gdIdleOf(it){return it.when==="idle"?(+it.idleFor||20):+it.idleFor||0}
function gdGestureOf(it){return it&&it.gesture?GD_GESTURE[it.about]||null:null}
function gdGestureWords(it){var g=gdGestureOf(it);return g?" The ghost finger shows "+GD_GESTURE_WORDS[g]+".":""}
/* Only if, in words (only what differs from Any) */
function gdOnlyWords(it){var w=[];if(it.screen==="card")w.push("only on a card");if(it.aud)w.push("only for "+gdNameOf(GD_AUD,it.aud));if(it.visitIs)w.push("only on "+gdNameOf(GD_VISIT_IS,it.visitIs).toLowerCase());
  if(it.hasUsed)w.push("only once "+gdNameOf(GD_USED,it.hasUsed));if(it.notUsed)w.push("only until "+gdNameOf(GD_USED,it.notUsed));if(+it.notAfter>0)w.push("not after "+it.notAfter+" s of the visit");return w}
/* the look, in words, after the read-back */
function gdLookWords(it){var f=gdFocusNow(it),g=gdGestureOf(it),b=it.button&&it.button!=="none"?gdNameOf(GD_BUTTONS,it.button):"";
  return" Look: "+gdNameOf(GD_FORMS,it.form).toLowerCase()+", "+(it.goes==="walk"?"Azu walks to it":it.goes==="beside"?"Azu hops beside it":"Azu stays")+(it.points?" and points":"")+", focus "+gdNameOf(GD_FOCUS,f)+
    (f!==it.focus?" (a Spotlight shows as a Ring: "+gdSpotWhy(it).replace(/^Spotlight /,"")+")":"")+(g?", the ghost finger shows "+GD_GESTURE_WORDS[g]:"")+(b?", its button “"+b+"”":"")+"."}
/* every line an item can say (its variants, a point with a card's own, the returning visitor's, each audience's) */
function gdLineKeys(it){var k=[].concat(it.says||[]);if(it.saysCard)k.push(it.saysCard);if(it.saysBack)k.push(it.saysBack);Object.keys(it.saysAud||{}).forEach(function(a){if(it.saysAud[a])k.push(it.saysAud[a])});return k.filter(function(x){return x&&S.tx[x]})}
var GD_LANG_NAME={en:"English",pt:"Portuguese"};
/* why an item can never show (the named warnings of the Editor), in words; none: it can */
function gdNever(it){var r=[],k=gdKindOf(it),g=gdGroupOf(it),ab=gdNameOf(GD_ABOUT_OPTS,it.about).toLowerCase();if(!gdIsTip(it))return r;
  if(k&&k.on===false)r.push("its kind, "+k.name+", is Off");if(g&&g.on===false)r.push("its group, "+g.name+", is Off");if(g&&!g.pulled&&+g.weight===0)r.push("its group's weight is 0, so it never wins");
  if(it.hasUsed&&it.hasUsed===it.notUsed)r.push("Only if contradicts itself: has used and has not used "+gdNameOf(GD_USED,it.hasUsed));
  if(it.screen==="card"&&/^(marker|middle|point)$/.test(it.about)&&it.mustSee!==false)r.push("Only if contradicts itself: only on a card, but its target is on the wall");
  if(+it.notAfter>0&&+it.notAfter<=+it.notBefore)r.push("Only if contradicts itself: its latest ("+it.notAfter+" s) is not after its earliest ("+it.notBefore+" s)");
  if(it.when==="settle"&&!/^(marker|middle|point)$/.test(it.about))r.push("When its target settles, but it is about "+ab+": nothing settles");
  if(it.doneWhen&&it.doneWhen===it.hasUsed)r.push("Done when is already true: Only if waits until "+gdNameOf(GD_USED,it.hasUsed)+", which retires it");
  if(it.doneWhen&&it.when==="event"&&it.doneWhen===it.event)r.push("Done when is already true: it waits for "+(GD_EVENTS[it.event]||it.event)+", which retires it");
  /* round 7: a point's joke has its words in Specific Marker › Guide script (a point with none says none: never a warning); a Moment only in a tour */
  if(it.form==="beat")return r.concat(gdBeatNever(it));
  if(it.form==="moment"&&it.duringTour!=="between")r.push("it is a Moment, but During a tour is Never: a Moment shows only between two steps of a tour");
  if(it.when==="between"&&it.duringTour!=="between")r.push("When Between tour steps, but During a tour is Never");
  /* round 11: Fun is held while a tour plays, so a Fun item that comes only between tour steps cannot show (until its When changes) */
  if(it.kind==="fun"&&it.when==="between")r.push("Fun is held while a tour plays, and it comes only between two tour steps: it cannot show until its When changes");
  if(it.ptJokes)return r;
  var says=[].concat(it.says||[]).filter(function(x){return S.tx[x]});langCodes().forEach(function(l){if(!says.some(function(x){return String(S.tx[x][l]||"").trim()}))r.push("it has no words in "+(GD_LANG_NAME[l]||l))});return r}
/* + Add guidance's templates (_3.8 section 7): each a new item in the group it is added to, with its words (a Moment: form Moment, Azu's
   prop, between two tour steps; Fun › Moments is its home) */
var GD_TEMPLATES=[
  {id:"control",name:"Tip about a control",f:{label:"New tip about the ✦",about:"gbtn",points:true,focus:"ring",gesture:true,tap:"menu",importance:3,shows:"upto",max:2,again:60,notBefore:30},en:"Anything else? Tap {button}.",pt:"Mais alguma coisa? Toque em {button}."},
  {id:"remark",name:"Remark about the point in view",f:{label:"{point}: a new remark",perPoint:true,form:"remark",about:"point",goes:"beside",points:true,when:"settle",stillFor:1.2,tap:"remark",importance:2,shows:"visit"},en:"{point}: see how the blue catches the light.",pt:"{point}: veja como o azul apanha a luz."},
  {id:"tour",name:"Offer a tour",f:{label:"New tour offer for {point}",form:"invitation",about:"middle",goes:"beside",points:true,tap:"tour",button:"tour",stillFor:1.2,importance:3,shows:"visit",doneWhen:"tour",teaches:"tour",along:"late"},en:"{point} has a story. Shall I tell it?",pt:"{point} tem uma história. Conto-lha?"},
  /* round 12: a Breather (Fun › On the tour): a beat of the tour, Azu's prop and the chips Go on / A question first */
  {id:"moment",name:"Breather",f:{label:"New breather",form:"beat",beat:"breather",prop:"water",props:"date",place:"between",answers:"chips",waits:8,talkFor:90,calm:false,game:false,importance:2,shows:"visit"},en:"Arms tired? Lower the phone a moment, I'll wait.",pt:"Braços cansados? Baixe o telemóvel um momento, eu espero."},
  {id:"step",name:"Step of a sequence",f:{label:"New step",about:"marker",goes:"beside",points:true,focus:"ring",gesture:true,stillFor:1.2,afterPrev:20,importance:3,shows:"until",max:2,again:30,doneWhen:"card",teaches:"card"},en:"Now tap the glowing marker.",pt:"Agora toque no marcador aceso."},
  {id:"blank",name:"Blank",f:{label:"New item"},en:"",pt:""},
  /* round 8: a page of the first pages (offered only there): a title, a line, one button (Next; a second one can be added) */
  {id:"page",name:"Page",page:true,f:{label:"New page",form:"page",page:"custom",azu:"none"},en:"A short line under the title.",pt:"Uma linha curta sob o título.",
    title:{en:"A new page",pt:"Uma página nova"},btn:{en:"Next",pt:"Seguinte"}}];
/* ----- round 8, the first pages (_3.8 section 6) as data: the kinds of page, a button's actions, Azu on a page, the order's fixed ends, the
   reading time of a page and of the run (under 30 s), a page's read-back. Unity: the same group in GuidanceCatalog (its pages
   GuidanceItems of the Page form), read by an OnboardingController that draws each page as a UI Toolkit screen ----- */
var GD_PAGE_KINDS={hello:"Hello",aud:"Who's exploring",camera:"The camera",wall:"Straight to the wall",custom:"A page"};
var GD_PAGE_ACTS=[["next","Next page"],["how","Next, and How to use the card on the wall after the pages"]];
var GD_PAGE_AZU=[["form","Forms from a tile and waves"],["appear","Appears"],["none","None"]];
var GD_PAGE_RUN_MAX=30;
/* the pages in the group's order (a deleted one out) */
function gdPageDefs(){var g=gdGroupById("pages");return g?g.items.map(function(id){return S.guidance.items[id]}).filter(function(x){return x&&!x.deleted}):[]}
function gdPageOf(kind){return gdPageDefs().filter(function(x){return x.page===kind})[0]||null}
/* the order's two fixed ends: Hello first, Straight to the wall last ("" when the order keeps them) */
function gdPagesRule(ids){var k=ids.map(function(id){return(S.guidance.items[id]||{}).page}),h=k.indexOf("hello"),w=k.indexOf("wall");
  if(h>0)return"Hello stays first: the visitor's first screen is the guide's hello, before any question";
  if(w>=0&&w!==k.length-1)return"Straight to the wall stays last: the run ends on the wall, with the Opening message";return""}
/* a page's words (in the visitor's language) and its time: its words read at 4 a second, and 1.5 s to act; Who's exploring 5 s (three
   choices); the camera's own request 3 s more; the wall none (it is the wall itself) */
function gdPageWordsOf(it){return[it.title,it.says].concat((it.buttons||[]).map(function(b){return b.word})).filter(function(k){return k&&S.tx[k]}).map(function(k){return TXT(k)}).join(" ")}
function gdPageSecs(it){if(it.page==="wall")return 0;if(it.page==="aud")return 5;var n=gdPageWordsOf(it).split(/\s+/).filter(Boolean).length;return Math.round((n/4+1.5+(it.page==="camera"?3:0))*10)/10}
/* the screens of a first run now (its On pages, Who's exploring only when asked here; the camera, as on a new install), the wall aside */
function gdPagesPlan(){var g=gdGroupById("pages");if(!g||g.on===false)return[];return gdPageDefs().filter(function(it){return it.on&&it.page!=="wall"&&(it.page!=="aud"||S.aud.where==="start")})}
function gdPagesSecs(list){return Math.round((list||gdPageDefs().filter(function(x){return x.on})).reduce(function(a,x){return a+gdPageSecs(x)},0))}
/* when a page is in the run, in words */
function gdPageWhen(it){return it.page==="hello"?"first":it.page==="aud"?"only when Detail Card › 6 · Audiences › Where to ask is At the app's start (now: "+gdNameOf(AUD_WHERE,S.aud.where)+")":
  it.page==="camera"?"only while the camera is not yet allowed (the phone's own permission)":it.page==="wall"?"last: the run ends on the wall":"in its place in the order"}
function gdPageLook(it){if(it.page==="aud")return"The first card's own screen, reused (Family, Explorer, Expert; its Skip = Explorer): it sets the audience, and with it the guidance preset";
  if(it.page==="wall")return"No screen: the Opening message says this short line on the wall, then Tap a marker comes as The start's";
  var an={next:"the next page",how:"the next page, and How to use the card after the pages",camera:"the phone's own request"},b=(it.buttons||[]).filter(function(x){return x.word}).map(function(x){return"“"+TXT(x.word)+"” ("+(an[x.act]||x.act)+")"}),w=[];
  if(it.azu&&it.azu!=="none")w.push("Azu "+(it.azu==="form"?"forms from a tile and waves (Reduce Motion: it appears; Mascot Off: no Azu)":"appears (Mascot Off: no Azu)"));if(it.langChip)w.push("the language as a chip (EN / PT: the app's language)");
  w.push("its title and line");w.push(b.length?b.join(" and "):"no button");var t=w.join(", ");
  return t.charAt(0).toUpperCase()+t.slice(1)+(it.page==="camera"?"; then the phone's own request (Allow / Don't Allow); refused: how to allow it, Open Settings and Try again, never the wall without the camera":"")}
function gdPageReadBack(it){var all=gdPagesPlan(),i=all.indexOf(it);
  return it.label+" · App › First pages · "+(!it.on?"Off: skipped":it.page==="wall"?"the run's end":i<0?"not in a first run now":"page "+(i+1)+" of "+all.length+" of a first run now")+" · "+gdPageWhen(it)+" · at the app's start, on a first run only (later launches: no pages, Welcome back; Remember across visits Off: every launch is a first) · "+
    gdPageLook(it)+(it.page==="wall"?"":" · Skip skips the rest of the intro (the camera's own request still comes)")+(it.page==="wall"?"":" · about "+gdPageSecs(it)+" s")+"."}
/* ----- round 9 (_3.8 section 3.4), pull: Things to try and Ask Azu's "how do I…?" as data. Things to try is the App items that teach a
   feature (each item's Teaches), one row a feature, in the Editor's order: each item's In Things to try (Yes when it Teaches; the developer
   may take a row out), its words the feature's (gd_try_<feature>: the row, "Open a card"; gd_tryhow_<feature>: the answer to "how do I…",
   both owned by the item that teaches it). A row is ticked once the visitor has done it: the same memory as Done when (its item done for
   good) or the feature it teaches used (the visitor's features used). It is the visitor's own pull: never scheduled, never capped, and the
   item's On, its group's and its kind's (the push) and the visitor's Tips (Off: no pushed tip) never hide it. Ask Azu's "how do I…" (no
   key: a simple match on the question's words, in both languages, against each feature's keywords and its row's words; the live agent:
   its tool show_how) answers from the same items: the feature's answer, and a Show me card that plays its gesture. Unity: the same
   GuidanceItem fields (Teaches, InThingsToTry) and two CardStrings keys a feature; the match a small keyword table in GuidanceCatalog ----- */
/* the keywords of each feature, in English and Portuguese (accents dropped; a phrase, or a word with a mark in it, is matched as it is; a word by its first 5 letters) */
var GD_TRY_KEYS={card:["card","cards","cartao","cartoes","marker","markers","marcador","marcadores"],view:["the view","a vista","view","vista"],menu:["menu","✦","star","estrela"],
  locate:["locate","localizar","find it","encontrar","encontro","the pin","o pino","alfinete"],navbar:["navigation","navegacao","the bar","a barra","jump","saltar","salto"],
  ba:["before and after","before / after","before/after","antes e depois","antes / depois","antes/depois","slider","compare","comparar"],
  strip:["swipe","strip","strips","deslizar","deslizo","faixa","faixas","tira","tiras"],pause:["pause","pausar","pauso","pausa","stop the guide","parar o guia"]};
var GD_HOW_STOP={about:1,what:1,with:1,this:1,that:1,from:1,into:1,then:1,your:1,have:1,does:1,sobre:1,como:1,para:1,numa:1,este:1,esta:1,isto:1,onde:1,quando:1,there:1,where:1,which:1};
/* an item that can be a row: an App item (a tip, a cue, or a Try item) that teaches a feature */
function gdTryCan(it){return!!it&&it.kind==="app"&&!!it.teaches&&!it.deleted&&(gdIsTip(it)||it.form==="cue"||it.form==="try")}
function gdTryIn(it){return it.inTry!=null?it.inTry!==false:!!it.teaches}
/* the rows of Things to try now: one a feature (the first item In Things to try that teaches it), in the Editor's order */
function gdTryDefs(){var seen={},out=[];gdDefs().forEach(function(it){if(!gdTryCan(it)||!gdTryIn(it)||seen[it.teaches])return;seen[it.teaches]=1;out.push(it)});return out}
/* every feature an App item teaches (Ask Azu answers them, In Things to try or not), one item a feature */
function gdTeachers(){var seen={},out=[];gdDefs().forEach(function(it){if(!gdTryCan(it)||seen[it.teaches])return;seen[it.teaches]=1;out.push(it)});return out}
function gdTryItem(id){return gdDefs().filter(function(x){return x.id===id&&gdTryCan(x)})[0]||null}
/* done: the item done for good (its Done when) or the feature it teaches used; the count "3 of 8" */
function gdTryDone(it,V){V=V||S.gdVisitor;return!!(gdEverOf(V,it.id).done||it.teaches&&V.used&&V.used[it.teaches]!=null)}
function gdTryCount(V,rows){V=V||S.gdVisitor;rows=rows||gdTryDefs();return{done:rows.filter(function(it){return gdTryDone(it,V)}).length,n:rows.length}}
/* its words: the feature's row ("Open a card") and the answer to how do I… ({button}: the ✦, or its word) */
function gdTryWord(it){var k="gd_try_"+it.teaches;return S.tx[k]?TXT(k):gdNameOf(GD_TEACH,it.teaches)}
function gdTryHowLine(it){var k="gd_tryhow_"+it.teaches;return(S.tx[k]?TXT(k):gdTryWord(it)).replace(/\{button\}/g,S.guideCfg.gbIcon==="star"?"✦":TXT("gb_label"))}
/* "how do I…?": a question about the app (how / where do I / can I; como / onde / posso), then the feature whose keywords (2 each) and row's
   words in both languages (1 each, 4 letters or more) the question has most; 2 or more to answer; ties: the Editor's order. A question about
   the history ("How old is the palace?", "Como era o palácio antes do terramoto?") matches none */
function gdHowNorm(t){return" "+String(t||"").toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g,"").replace(/[^a-z0-9✦/ ]+/g," ").replace(/\s+/g," ").trim()+" "}
function gdHowAsks(n){return/ (how|can i|where do i|where can i|como|posso|onde posso|onde e que) /.test(n)}
function gdHowWordHit(w,k){return w===k||k.length>=5&&w.length>=5&&w.slice(0,5)===k.slice(0,5)}
function gdHowScore(n,it){var ws=n.trim().split(" "),s=0,f=it.teaches;(GD_TRY_KEYS[f]||[]).forEach(function(k){if(!/^[a-z]{4,}$/.test(k)?n.indexOf(" "+k+" ")>=0:ws.some(function(w){return gdHowWordHit(w,k)}))s+=2});
  var k2="gd_try_"+f,o=S.tx[k2]||{},row=[];Object.keys(o).forEach(function(l){gdHowNorm(o[l]).trim().split(" ").forEach(function(x){if(x.length>=4&&!GD_HOW_STOP[x]&&row.indexOf(x)<0)row.push(x)})});
  row.forEach(function(x){if(ws.some(function(w){return gdHowWordHit(w,x)}))s+=1});return s}
function gdHowFind(q){var n=gdHowNorm(q);if(!gdHowAsks(n))return null;var best=null,bs=1;gdTeachers().forEach(function(it){var sc=gdHowScore(n,it);if(sc>bs){bs=sc;best=it}});return best}
/* a Try item's read-back (pulled only) */
function gdTryReadBack(it){var g=gdGroupOf(it);return it.label+" · App › "+(g?g.name:"?")+" · pulled only: a row of Things to try ("+(gdTryIn(it)?"In":"Out")+") and Ask Azu's answer to how do I… · never on its own (no bubble, never scheduled, never capped) · "+
  (GD_ABOUT[it.about]?"about "+GD_ABOUT[it.about].replace(/^when /,"")+" · ":"")+"Show me: "+(GD_GESTURE_WORDS[GD_GESTURE[it.about]]||"a tap")+" on it, the visitor taken there first · done when "+(GD_UNTIL[it.doneWhen]||GD_EVENTS[it.doneWhen]||"never")+"."}

/* ----- round 12, Fun › On the tour, as data (research_tour_entertainment.md 3.2-3.5). A beat's fields (GD_BEAT_DEF, set once by gdFill):
   Place in the tour (Opening: after the introduction / Between steps / Finale: before the last step; In a step comes in round 13), The
   visitor answers by (Nothing / Chips / Slider / Tap / Tap or say), Waits (3-12 s: then it carries on by itself; the quiz has no timer),
   Needs (what the point must hold: quiz questions, a year; a named warning when it lacks them), Pairs with (a promise and its payoff are
   one), Scores, Calm too (allowed when Azu › Liveliness is Calm), Audiences (Family / Explorer / Expert), Tour length (Every length
   / Not on Short / Deep only), a game (its 4 s Not now chip; two Not nows end games for the visit), a button path (a screen reader may
   get it). The quiz's questions and the guesses are per step, in Specific Marker › Guide script (S.tourQ, S.tourGuess: words EN / PT,
   Draft with AI, a Reviewed tick per language); unreviewed ones never play. Unity: GuidanceItem's beat fields (GuidanceCatalog); the
   questions a QuizQuestion list on each GuideStep (CardStrings keys), the Reviewed flags beside them ----- */
var GD_PLACES=[["opening","Opening"],["between","Between steps"],["instep","In a step"],["finale","Finale"]];
var GD_PLACE_WORDS={opening:"after the introduction, before the first step",between:"between two steps",instep:"inside its span of 1-2 steps (announced at the seam before it, its result at the seam after it)",finale:"before the tour's last step"};
var GD_ANSWERS=[["nothing","Nothing"],["chips","Chips"],["slider","Slider"],["tap","Tap"],["tapsay","Tap or say"],["tapazu","Tap Azu"],["aim","Aim at the wall"]];
/* the answers a beat may offer (the quiz: tap always, voice an extra; a guess: its slider) */
var GD_ANSWERS_OF={promise:["chips","nothing"],finale:["tap","tapsay"],guess:["slider"],breather:["chips","nothing"],word:["tapazu"],find:["aim"]};
var GD_NEEDS={"":"nothing of the point's",quiz:"its reviewed quiz questions (Specific Marker › Guide script)",year:"a step with a reviewed guess (Specific Marker › Guide script)",word:"a reviewed word in each language (Specific Marker › Guide script › Don't say it!)",wall:"a wall point to find, on the wall of the card's point (The point, below)"};
var GD_TOURLEN=[["every","Every length"],["notshort","Not on Short"],["deep","Deep only"]];
var GD_AUDS=[["family","Family"],["explorer","Explorer"],["expert","Expert"]];
var GD_EXPERT=[["optin","An opt-in question"],["same","As the others"]];
var GD_BEAT_KIND={promise:"the quiz's promise",finale:"the quiz",guess:"a guess",breather:"a breather",word:"a forbidden-word game",find:"a find on the wall"};

/* the breather's prop by date (Preview & Test › Today is): a fan in June-September, a scarf in December-February, else the glass; never
   the same prop twice in a visit (the next in turn). The cookie keeps its own */
var GD_PROP_DATES=[{prop:"fan",from:"06-01",to:"09-30"},{prop:"scarf",from:"12-01",to:"02-28"}];
var GD_PROP_NAMES={water:"a glass of water",fan:"a fan",scarf:"a scarf",cookie:"a cookie"};
function gdPropByDate(md){var p=GD_PROP_DATES.filter(function(x){return gdInWindow(md,x.from,x.to)})[0];return p?p.prop:"water"}
function gdPropOf(it,md,used){if(it.props!=="date")return it.prop||"water";var want=gdPropByDate(md);used=used||[];if(used.indexOf(want)<0)return want;
  var alt=["water","fan","scarf"].filter(function(x){return used.indexOf(x)<0});return alt[0]||want}
/* the quiz's questions and the guesses, per step of the demo point (the Guide script's; written from the tour's own text). Each language
   reviewed on its own; the AI's draft (src "ai") of the step "Drag across the picture" is not reviewed yet, so it never plays */
S.tourQ={
  g1:{q:T("How long was the palace the royal home?","Durante quanto tempo foi o palácio casa real?"),a:[T("About 250 years","Cerca de 250 anos"),T("About 50 years","Cerca de 50 anos"),T("About 500 years","Cerca de 500 anos")],ok:0,
    why:T("Kings lived here about 250 years, until 1755.","Os reis viveram aqui cerca de 250 anos, até 1755."),src:"human",rev:{en:true,pt:true}},
  g4:{q:T("What filled the new palace?","O que encheu o novo palácio?"),a:[T("The wealth of the India voyages","A riqueza das viagens à Índia"),T("Gold from Brazil","Ouro do Brasil"),T("Spanish silver","Prata espanhola")],ok:0,
    why:T("Manuel I filled it with the riches of the India voyages.","D. Manuel I encheu-o com a riqueza das viagens à Índia."),src:"human",rev:{en:true,pt:true},
    /* round 18: its own hint (Asking during a game › hint 1), reviewed per language */
    hint:T("Think of where the king's ships were sailing home from.","Pense de onde voltavam os navios do rei."),hrev:{en:true,pt:true}},
  g5:{q:T("How many books did the palace library hold?","Quantos livros tinha a biblioteca do palácio?"),a:[T("About 7,000","Cerca de 7 000"),T("About 70,000","Cerca de 70 000"),T("About 700,000","Cerca de 700 000")],ok:1,
    why:T("Some seventy thousand books, beside the royal archive.","Cerca de setenta mil livros, ao lado do arquivo real."),src:"human",rev:{en:true,pt:true}},
  g6:{q:T("What took the palace's place?","O que ficou no lugar do palácio?"),a:[T("A square","Uma praça"),T("A cathedral","Uma catedral"),T("A harbour","Um porto")],ok:0,
    why:T("The square, the Terreiro do Paço, took its place.","A praça, o Terreiro do Paço, ficou no seu lugar."),src:"ai",rev:{en:false,pt:false}},
  g7:{q:T("What destroyed the palace in 1755?","O que destruiu o palácio em 1755?"),a:[T("A war","Uma guerra"),T("An earthquake, a fire and a tsunami","Um terramoto, um incêndio e um maremoto"),T("A great flood","Uma grande cheia")],ok:1,
    why:T("All three struck on the morning of 1 November.","Os três chegaram na manhã de 1 de novembro."),src:"human",rev:{en:true,pt:true}},
  g8:{q:T("What does the gallery show?","O que mostra a galeria?"),a:[T("The king's portraits","Retratos do rei"),T("The square through three centuries","A praça ao longo de três séculos")],ok:1,
    why:T("Three centuries of the square, in pictures.","Três séculos da praça, em imagens."),src:"human",rev:{en:true,pt:true}}};
S.tourGuess={g4:{q:T("When do you think the king moved the court down to the river?","Quando acha que o rei mudou a corte para junto do rio?"),n:1500,from:1400,to:1700,unit:"year",src:"human",rev:{en:true,pt:true}}};
/* a step's question / guess that plays in a language: its words there, at least two answers, the right one among them, and reviewed */
function gdQOk(q,l){if(!q||!q.rev||!q.rev[l])return false;var t=function(o){return!!(o&&String(o[l]||"").trim())};return t(q.q)&&(q.a||[]).filter(t).length>=2&&(q.a||[]).length<=3&&t((q.a||[])[+q.ok])}
function gdGuessOk(g,l){return!!(g&&g.rev&&g.rev[l]&&g.q&&String(g.q[l]||"").trim()&&isFinite(+g.n)&&+g.from<+g.n&&+g.n<+g.to)}
/* the why line: 12 words or fewer (the study's rule; the Editor warns) */
function gdWhyWords(t){return String(t||"").split(/\s+/).filter(Boolean).length}
/* the point's reviewed questions in a language: their steps (the quiz needs qMin from at least two steps) */
function gdQSteps(l,steps){return(steps||S.fullSteps||S.guideSteps).filter(function(st){return gdQOk(S.tourQ[st.id],l)}).map(function(st){return st.id})}
/* a beat's warnings (gdNever): what it needs and the point lacks, named; a pair whose other half is Off or gone */
function gdBeatNever(it){var r=[],langs=langCodes();
  if(it.needs==="quiz"){langs.forEach(function(l){var q=gdQSteps(l),n=q.length;if(n<+it.qMin||n<2)r.push("the quiz needs "+it.qMin+" reviewed questions from 2 steps in "+(GD_LANG_NAME[l]||l)+"; this point has "+n)})}
  if(it.needs==="year")langs.forEach(function(l){if(!(S.fullSteps||S.guideSteps).some(function(st){return gdGuessOk(S.tourGuess[st.id],l)}))r.push("a guess needs a step with a reviewed guess in "+(GD_LANG_NAME[l]||l)+"; this point has none")});
  if(it.needs==="word")langs.forEach(function(l){var w=gdWordOf(l);if(!w||!w.w)r.push("Don't say it! needs a word in "+(GD_LANG_NAME[l]||l)+"; this point has none: no game in "+(GD_LANG_NAME[l]||l));
    else if(!w.rev)r.push("the word “"+w.w+"” in "+(GD_LANG_NAME[l]||l)+" is not reviewed: no game in "+(GD_LANG_NAME[l]||l));
    else{var n=gdWordCount(w,l);if(n<2)r.push("“"+w.w+"” is said "+(n?"once":"never")+" in its span ("+gdSpanWords(w.span)+"): no game in "+(GD_LANG_NAME[l]||l));else if(n>4)r.push("“"+w.w+"” is said "+n+" times in its span: too easy, too long (2-4)")}});
  if(it.needs==="wall"){var fd=S.tourFind;if(!fd||!fd.point)r.push("Find it on the wall needs a wall point (Guide script › Find it on the wall); this point has none");
    else{var fp=(WALLS.ribeira.pois||[]).filter(function(p){return p.id===fd.point})[0];if(!fp)r.push("its wall point, "+fd.point+", is not on the card's wall");if(!byId(S.fullSteps||S.guideSteps,fd.after))r.push("its step, "+fd.after+", is not in the tour")}}
  if(it.pairs){var o=S.guidance.items[it.pairs];if(!o||o.deleted)r.push("its pair, "+it.pairs+", is gone: a promise and its payoff are one");else if(!o.on)r.push("its pair, the “"+o.label+"”, is Off: a promise and its payoff are one")}
  var a=it.auds||{};if(!GD_AUDS.some(function(x){return a[x[0]]}))r.push("no audience gets it (Audiences)");
  var g=gdGroupOf(it);if(g&&g.auds&&!GD_AUDS.some(function(x){return g.auds[x[0]]&&a[x[0]]}))r.push("no audience of On the tour gets it");
  return r}
/* how often, in words: from its tour's plan */
function gdBeatHowOften(it){return(it.shows==="visit"?"once a visit, ":"once a tour, ")+"in its tour's plan ("+gdNameOf(GD_PLACES,it.place)+")"}
/* the read-back of a beat: what, where, its rhythm, who, what it needs, its pair */
function gdBeatReadBack(it){var g=gdGroupOf(it),a=it.auds||{},w=[];
  w.push(GD_BEAT_KIND[it.beat]||"a beat");w.push(GD_PLACE_WORDS[it.place]||"at a seam");
  if(+it.talkFor>0)w.push("after "+it.talkFor+" s of narration");if(it.beforeEnd)w.push("before the last step");if(+it.minNarr>0)w.push("only in a tour of "+it.minNarr+" s of narration or more");
  if(it.variantOf){var vo=S.guidance.items[it.variantOf];if(vo)w.push("a variant of “"+vo.label+"” (never both in a tour)")}
  if(+it.minSteps>0)w.push("only in a tour of "+it.minSteps+" steps or more");
  if(it.beat==="word")w.push("a tap window from the word's start to its end + "+it.tapWin+" s, "+(it.falseTaps==="nope"?"a false tap a playful “Nope!”":"a false tap counted quietly")+(it.slipBack?", the first missed slip played back":""));
  if(it.beat==="find")w.push("a hint after "+it.hintAfter+" s, Azu shows it after "+it.giveUp+" s (or I give up), never while the wall is lost");
  w.push("answers by "+gdNameOf(GD_ANSWERS,it.answers).toLowerCase());w.push(it.beat==="word"||it.beat==="find"?"its Not now "+it.waits+" s":+it.waits>0?"waits "+it.waits+" s, then carries on":"no timer");
  if(it.needs)w.push("needs "+GD_NEEDS[it.needs]);if(it.pairs){var p=S.guidance.items[it.pairs];if(p)w.push("one pair with “"+p.label+"”")}
  if(it.scores)w.push("scores");if(it.game)w.push("a game (its 4 s Not now chip)");w.push(it.calm?"Calm too":"Lively only");
  w.push("for "+GD_AUDS.filter(function(x){return a[x[0]]}).map(function(x){return x[1]+(x[0]==="expert"&&it.beat==="promise"&&it.expert==="optin"?" (an opt-in)":"")}).join(", ")||"nobody");
  if(it.tourLen!=="every")w.push(gdNameOf(GD_TOURLEN,it.tourLen).toLowerCase());
  return it.label+" · "+(g&&g.tour?"Games on the tour":"Fun › "+(g?g.name:"?"))+" · "+w.join(", ")+" · "+gdBeatHowOften(it)+" · never over a sentence, never on a Solemn point, never two seams in a row; the tour's rhythm, never Fun's visit cap."}

/* ----- round 13, Don't say it! as data (research_tour_entertainment.md 5.4). Per point and per language: the word the author picked from
   the Editor's suggestions (or typed), its span (1-2 consecutive steps), and its own Reviewed tick; a language without a reviewed word
   offers no game. The suggestions (gdWordSuggest): each word of 4 letters or more said 2-4 times in a span of 1-2 steps, with its count and
   its span ("palace: 2 times in steps 7-8 (0:16)"); the research's rules are checked and any it breaks is named (a span of 45-120 s, not in
   the span's first sentence, a noun the story is about: a word of the point's names and headings, or one the tour comes back to in 3
   steps or more; never a common word). The slips at run time are the same matches (gdWordHit): the word, its plural, its possessive.
   Unity: a ForbiddenWord {word, spanFrom, spanTo, reviewed} per language on the GuideTour asset; the suggester plain C# over the steps'
   text (EditMode-tested on the Lab's cases) ----- */
S.tourWord={en:{w:"palace",span:["g3","g4"],src:"suggested",rev:true},pt:{w:"palácio",span:["g3","g4"],src:"suggested",rev:true}};
/* Find it on the wall (research 4.7): the point's one find, after the step that names it */
S.tourFind={after:"g3",point:"torreao",thing:T("the great corner tower","o grande torreão")};
var GD_WORD_STOP={en:"about after again also around back before being came come could down each even every first from have here into just last later like little long made make many more most much must never next only other over right same some still such than that their them then there these they thing this those through under until very want were what when where which while will with would your years year hundred hundreds thousand fifty twenty three seven seventy half centuries century morning brand some people",
  pt:"agora ainda antes apenas aqui assim cada como desde depois deste desta este esta isso isto mais mesmo muito nada para pela pelo pelos pelas pode porque quando quem sobre seus suas também tinha toda todo todos todas uma umas uns vezes onde lado novo nova anos cerca duzentos cinquenta setenta seguintes poucas horas manhã acabada qual então foram durante entre sempre"};
function gdWordNorm(t){return String(t||"").toLowerCase().replace(/[’']s$/,"").replace(/[^a-zà-ÿ]+/g,"")}
/* a token of the narration is the word (or its plural, or its possessive) */
function gdWordHit(tok,w,l){var t=gdWordNorm(tok),x=gdWordNorm(w);if(!t||!x)return false;return t===x||t===x+"s"||(l!=="pt"&&t===x+"es")||(l==="pt"&&/ão$/.test(x)&&t===x.replace(/ão$/,"ões"))}
function gdWordOf(l){var w=S.tourWord&&S.tourWord[l];return w&&w.w?w:null}
function gdSpanSteps(span){var all=S.fullSteps||S.guideSteps;return(span||[]).map(function(id){return byId(all,id)}).filter(Boolean)}
function gdSpanWords(span){var all=S.fullSteps||S.guideSteps,ix=(span||[]).map(function(id){return all.findIndex(function(s){return s.id===id})+1}).filter(function(x){return x>0});return ix.length?"step"+(ix.length>1?"s ":" ")+ix.join("-"):"no steps"}
/* a step's words in a language (the tour's own text) and its seconds at the voice's pace (the recorded line's times in the visitor's
   language; else its words at 170 a minute) */
function gdStepWords(st,l){return(stepTextIn(st,l).match(/\S+/g)||[])}
function gdStepSecsIn(st,l){return l===(S.lang||"en")&&typeof stepSecs==="function"?stepSecs(st):gdStepWords(st,l).length*60/170+.7}
function gdWordCount(w,l){if(!w)return 0;var n=0;gdSpanSteps(w.span).forEach(function(st){gdStepWords(st,l).forEach(function(t){if(gdWordHit(t,w.w,l))n++})});return n}
/* the suggestions in a language, best first: each candidate {w, n, span, secs, flags: the research's rules it breaks} */
function gdWordSuggest(l){var all=(S.fullSteps||S.guideSteps).filter(function(st){return!/^(intro|teaser|end)$/.test(st.act)}),stop={};(GD_WORD_STOP[l]||"").split(" ").forEach(function(x){stop[x]=1});
  var names=[];(WALLS.ribeira.pois||[]).forEach(function(p){names.push(p.name&&p.name[l]||"")});S.story.forEach(function(r){if(r.kind==="heading"&&r.c&&r.c.text)names.push(r.c.text[l]||"")});
  var nameW={};names.join(" ").split(/\s+/).forEach(function(x){var t=gdWordNorm(x);if(t)nameW[t]=1});
  var inSteps={};all.forEach(function(st){var seen={};gdStepWords(st,l).forEach(function(t){var x=gdWordNorm(t);if(x&&!seen[x]){seen[x]=1;inSteps[x]=(inSteps[x]||0)+1}})});
  var out=[],key={};
  for(var i=0;i<all.length;i++)[1,2].forEach(function(len){var sp=all.slice(i,i+len);if(sp.length<len)return;var toks=[],first=0,secs=0;
    sp.forEach(function(st,j){var ws=gdStepWords(st,l);if(j===0){for(var k=0;k<ws.length;k++){if(/[.!?…]["”»)]*$/.test(ws[k])){first=k+1;break}}if(!first)first=ws.length}toks=toks.concat(ws);secs+=gdStepSecsIn(st,l)});
    var cnt={},inFirst={};toks.forEach(function(t,k){var x=gdWordNorm(t);if(x.length<4||stop[x])return;cnt[x]=(cnt[x]||0)+1;if(k<first)inFirst[x]=1});
    Object.keys(cnt).forEach(function(x){var n=0;toks.forEach(function(t){if(gdWordHit(t,x,l))n++});if(n<2||n>4)return;var ids=sp.map(function(s){return s.id}),k=x+"|"+ids.join(",");if(key[k])return;
      /* a two-step span only when its second step says it too (else the one step is the span) */
      if(len===2&&!gdStepWords(sp[1],l).some(function(t){return gdWordHit(t,x,l)})||len===2&&!gdStepWords(sp[0],l).some(function(t){return gdWordHit(t,x,l)}))return;key[k]=1;
      var flags=[];if(secs<45)flags.push("a short span: "+gdMmss(secs)+" (the rule: 45-120 s)");if(secs>120)flags.push("a long span: "+gdMmss(secs)+" (the rule: 45-120 s)");
      if(inFirst[x])flags.push("said in the span's first sentence (the game would start on a slip)");if(!nameW[x]&&(inSteps[x]||0)<3)flags.push("not a name the story is about");
      out.push({w:x,n:n,span:ids,secs:secs,flags:flags})})});
  return out.sort(function(a,b){return a.flags.length-b.flags.length||b.n-a.n||a.secs-b.secs||(a.w<b.w?-1:1)})}
/* a suggestion in words: "palace: 2 times in steps 7-8 (0:16)" */
function gdWordLine(c){return c.w+": "+c.n+" times in "+gdSpanWords(c.span)+" ("+gdMmss(c.secs)+")"}
