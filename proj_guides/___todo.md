A. A full tour (the Ribeira Palace)

Open the Ribeira Palace from the wall and tap "▶ Start the guide". Listen to the teaser: does it make you want to go on?
Let it play. Check the opening lines of each block (warm, never "Now the story") and that no fact is said twice.
On a step that offers "Tell me more", tap it, including on the gallery. On another step, tap "Skip ahead".
Let it reach the end. Check the scroll to the quiz, Azu's question, and the What next? panel: its questions, the ask field, the nearby points and the small actions.
Shrink the panel to the strip, bring it back from the ✦ menu, then tap a nearby point such as the Torreão: does the fly and its invitation feel right?
B. Stopping and coming back
6. Start a tour, tap Stop mid-way, and try each choice on the Stop bar: Continue, Start again, What next?.
7. Stop again, reload the page and open the same point: check "Continue the tour (step N of M)" and Azu's "Where we were: …" recap.

C. Games and asking
8. During the tour's quiz, ask Azu something unrelated, then ask about the question itself three times to go through all three hints. Try "Tell me" once.
9. Try Guess the year and the card's own quiz (tap Play the quiz first) the same way.
10. Ask Azu something with no game on screen, by typing and by the mic.

D. Shorter points
11. Take the Torreão's or the Tagus's short tour from start to end.

E. The Editor (Guide tab)
12. Open 2 · The tour › Ways in › Continue the tour, then The tour's end, then 3 · Games on the tour › Asking during a game. Press a few ▸ demos and change a setting or two. Does each one do what its name says, and is anything confusing or missing?

Send your notes in any form, in one message or a few as you go. I'll group them into one polish round, then confirm the list with you before building. The three items already on the list (the off-topic quiz reply, the chip on the card's quiz, Azu over the card behind the panel) are in the handoff notes.






# FIRST NOTES TO CORRECT:
USE THIS AS A CHECK LIST. MARK WITH [DONE] EACH LITTLE TASK ONLY AFTER YOU DO IT AND CNFIRM AND TEST IT IS ALL GOOD SO WE USE THIS AS A CHECK LIST. 
sometimes the "click here" is a bit off?? (see attached image) 
when we open ribeira palace de mascote disapears. maybe stays behind that card.. maybe it can move to the lefts side of the "start the guide" button? and we move the button a bit to the right, and we make the button a bit more "active" with some "click me action" and the guide pointing at it to show the user to click?? 

when i click the guide, it it using i think the device voice?? but we have already some good voices that we already set up as default previously?? so lets use those better voices we used from azure or eleven labs?? 

Ah, but the good voice entered at the moment of really start the presentation. but the intro "I am your guide, if anything just tap ask"... that is still in some older version not plished in terms of voice, AND in terms of text... lets make it more human and friendly and warm and exciting and not robot like... and update the guidancee that intro gives, because for example now we don't have a clear "tap button", or at least the user don't what tht is... 
And again when the guide said: just so you know at the end I will ask you some questions..." it was again the device voice...

SO WE HAVE A BIG ARCHITECTURE AND STRUCUTRE AND CLEEAN TO DO. WE NEED to have some global AI Guide voice and mascote and other settings to be done in the Guide > 1. Azu section. currently it defines the mascote , the manners, and its moments.... and so we need to define there the vooices also?? and confirm that whole editor section is well organized and with the right usefull developer config options to choose the right feature and ptions for that feature... 

AND then all the "AI guide sub componentes and features should use those global settings". sounds good? so confirm what should be the glbal settings and then plan the editor tab well... and then update the AI guide feature sand components t always use those global settings and not some local hard coded ones. 

and the "editor sections and the whole app", we are "hard coding the name" Azu for the mascot. but in the editor sections and so on lets use the clear name "mascot" for example. not the specific name azu. And, also lets add to the global section of the mascot the definition of its name and maybe the presentation it should give the first time it talks etc... ?? 

and the editor, we have some sub tabs for the Guide sections. but lets use some sort of acordion style. we start with the sections colapsed so the developer sees clearly all we have and then he can opens it, and we keep a sort of "sticky headers" of the sections and sub sections so they still visibel at the top when we scroll down...?? 


then in the presentation, that "before i tell you, a guess", it was too fast. lets give it more time as default for the user to have time the first time... 


and the pannel of the AI Guid when opened is too big with too big elements. the "speach has too big words... and the hilighting word by word is not smooth... lets make some interpolation so the hilighting is smooth and then leaving the hilighted coolors vanishing in the back like a comet... I think we impleented this already? but it was maybe only in the "AI Guide ballon"? so maybe we should implement it in a way to be used in all "speach text hilighting features, in the ballon, in this AI pannel and in the blcks directly when the AI guide reads them... ?? 

and we need to have a "back 10 secnds and front 10 seconds buttons", both in the minimal AI guide panel and in the big one. in the minimal panel maybe can be only the "back 10 seconds button"... ?? so the whole panel stays minimal.. 

in terms of the "focus of the block that the AI Guide is explaining, it is currently too much mechaninc. we "move to the block instantly, and so the user has no sense we are scrolling down,  or entering the block of the story from the preview to the detail page etc..." so we need to improve much the whole navigation feature of this. I want to the AI Guide to basically replicate all the movemnts and motion tha tthe user whould need to do by hand to execute that tour. so the scroll needs to be a bit more sloly and more natural and with some "scrolling effect" to show we are moving down the page or up?? some gray transparent element pulsing to show when we are scrolling?? and we scroll a bit more natural... ?? and then when the AI guide do sme action that corresponds to enter a card or click somethign, so we also show thaat on the screen, like if it was a "how to use the app demo"?? example we have some sort of little transparent hand moving like a commet going from some 2cm away in the direction of the place to be clicked, and then we have some "click effect" like some circle gray, transparent, the resembles the click?? and then we have the opening of the page with the good normal effets and animations we might have normally? like a sort of page zoom ou and in from the whole screen to the respective block preview position?? or other thigns...?? AND so lets make the "app movement during the AI guide to be exactly as if it was a normal user using the app". so with each guide we are basically doing a demo to the usser of how to use the app. And the same in the interactive elements like the drag the image befre/after... or the 3D mdel etc... ??? 

AND the focus we currently give, by higlighting the current beeing presented block and darkeing the rest of the app... lets remove that, because it damages the navigation sense too much... lets have only some sort of little bar that appears smooth on the left side pulsing a bit to represent that we are talking about that blck or something like thata? and we don't make the rest of the app darker... everything stays the same... ?? 

and the ballon i think we should improve that...  because it is sort of pointing at the mascot head, but then the mascot moves away and the ballon stays there in the same place... so maybe lets have the ballon to be used always near the mascot, so it is really pointing at the mascot... ?? and when not possible, for example during the guide where the mascot needs to move around to explain the blocks, and we can't have a ballon moving also because it will hide those blocks, so intead lets have a sort of "moving teleprompt panel on the bottom of the screen. something with a grandient or with some motion like a sky blured or something, from bottom until the top of the "telepromt" card... and we place this card above the detaail card and bellow the AI guide minimalist panel... parecido com a imagem anexo?? ou melhor...?? mais imersivo e minimalista e moderno e premium...?? a card for the teleprompt coming frm the base of the screen behind the Ai guide minimalist pannel and going sme 2 or 3 cm up to show the text beeing spoken hilighted, and trasparent disapearing as we mve frwards... ??? 

And in terms of the AI Guide panels, the minimal and the halfscreen... they are black and to much flat... maybe we can make them more light? and smooth and immersive and technological? maybe with some light shape and colors and 3D shapes and light and gradients and little motion??? 


# THE CHECKLIST (the Architect's, from the notes and prints above, 2026-11-13)

Each item gets [DONE] only after it is built, looked at in captures and tested. Items marked (known) were already in the handoff.
Builders and the Architect tick here; an item that changes during a round says so in one line under it.

## Round 19 · One mascot, one voice (the global settings first; everything else uses them)

Why the device voice: the good voices are files recorded on 2026-11-03/04 for the lines that existed then (106 lines). Every line
written since (the first introduction, the quiz teaser, the tour's links, Tell me more, the tour's end, Back to the game…) has no
file, so it falls back to the device voice.

- [DONE] 19.1 Guide › 1 · The mascot holds the global settings, in one place: Who (its name, EN and PT; its first introduction),
      Look, Voice (the voice, speed, what a line with no recording does), Speech on screen (bubble, teleprompter, highlight; built in
      round 21), Manner, Moments. Every guide part reads them; no part keeps its own voice, name or highlight.
- [DONE] 19.2 Audit: list every place that picks a voice, a name or a highlight by itself (hard-coded), and move it to 19.1's settings.
      Changed: every voice and name now goes through 1 · The mascot. The highlight's rows moved to Speech on screen, but the bubbles, the games' cards and the panel still light their own word until round 21 (21.2).
- [DONE] 19.3 The name: "Azu" is a setting (default Azu). The Editor says "the mascot" everywhere (Ask the mascot, The mascot speaks up,
      The mascot's intro lines…); the visitor's words use the name from the setting.
- [DONE] 19.4 Every line the guide can say is in the voice script (the export collects all sets, not only the 2026-11-03 lines), and the
      missing lines are recorded in the chosen voice. The Editor shows the voice's coverage ("212 of 214 lines recorded") and lists the
      lines that would use the device voice.
      Built: the script holds 511 lines (106 before); 459 were missing and are now recorded in Ryan · Duarte (30,747 characters, Azure F0). Coverage is in 1 · The mascot › Voice.
- [DONE] 19.5 The first introduction ("I'm your guide, if anything just tap Ask…") rewritten: warm, human, exciting; it names the real
      controls the visitor can see (no unclear "tap" button); in the good voice.
      Changed: the default words do not name the point (the teaser says it right after). They name the control the visitor sees at that moment: the arrow at Mini, the mascot's face in Player sheet, the ✦ at Compact.
- [DONE] 19.6 The quiz teaser ("Just so you know: at the end I'll ask you three questions") in the good voice; its dark card must not
      cover the hero's title (print 17).
- [DONE] 19.7 "Tap here." bubble on the wall: sits off its target (print 14). It points cleanly from the mascot at the marker.
      Changed: the bubble now sits beside the mascot's head (not above it), with its tail on the head. It goes above only when neither side has room.
- [DONE] 19.8 Opening the Ribeira Palace: the mascot disappears behind the card (print 15). It stays in sight, to the left of
      "Start the guide", pointing at it.
- [DONE] 19.9 "Start the guide · 3 min" is cut by "Find it on the wall" (print 15). Both fit; Start moves right of the mascot.
      Changed: Find it on the wall stays beside its pin. While it shows, the hero's words rise one row, so nothing overlaps at 390 px (EN and PT).
- [DONE] 19.10 "Start the guide" gets a gentle "tap me" life: a soft glow and pulse like print 22, calm, once the card settles.
- [DONE] 19.11 "Before I tell you, a guess": too fast. A longer default time to answer, longest the first time.
      Built: 14 s to answer (it was 8), 18 s for the first guess of a visit. Two Editor rows: Waits (s) and First guess of a visit.

## Round 20 · The Editor as an accordion

- [DONE] 20.1 The Guide tab's sections become an accordion that starts collapsed, so all sections show at a glance (the chip row goes).
      Built: six one-line sections with a sub-line and a count ("6 parts"), Collapse all, One section open at a time (Off).
- [DONE] 20.2 Sticky headers: the open section's header and its open foldout's header stay at the top while scrolling.
- [DONE] 20.3 Check each section holds the right options, in a sensible order, after 19's global settings (a short review table).
      Done: Narration renamed Music (the voice lives in 1 · The mascot); Hero button renamed Start the guide; When the visitor
      scrolls moved from Continue the tour to Focus; Knowledge's unnamed row named What is sent; Wall (the Lab) moved to
      6 · Preview & Test (a Lab stand-in, never in the app); every section's sub-line rewritten.
- [DONE] 20.4 Core tests (4 rounds since 17b).
      All 11 core files green after 4 stale checks were updated (renames of rounds 17c and 19); walks card, guide, marker,
      global bad=0. The words walk ran past 40 min: the same on master (round 19), so not this round's; it is item 21.8.
- [DONE] 20.5 The glow of "Start the guide": an Editor option (Once per visit, the default / On every new card / Off), the developer's
      answer on 2026-11-13. (Also decided: the first introduction stays A; Find it on the wall keeps the hero's rise.)
- [DONE] 20.6 Run the ten long test files whose Editor labels round 19 changed and that core does not run: test8g, 8s, 8t, 8x, 9db,
      9e, 9ea, 9g, 9h, 9k.
      All green after their checks were updated to round 19's names and moved rows. Two Lab scenes had the same stale words
      ("pokes on Azu", "Azu's quiz"): a real red the developer would have seen, fixed in guidance/scenes.js (test9g, 9q).

## Round 21 · Speech on screen and the panels

- [DONE] 21.1 The open guide panel is too big, its words too large (print 18): smaller, calmer, fits half the screen with room.
      Built: Ink glass's Open is 374 px (about 460 before; 44 % of 844), its words 18 px (22), a head row (the mascot, its name and the stop, Aa, Read all, small), the time either side of the progress line; every target 44 px.
- [DONE] 21.2 One smooth word highlighter for every place speech is shown (bubble, teleprompter, panel, the block being read): eased
      between words, a comet tail fading behind.
      Built: one painter (guide/highlight.js) for the bubbles, the teleprompter, the open panel, the block and the games' cards: a soft light gliding between the words on an in-out ease, a tail fading behind; Highlight style, Trail and Glide moved there.
- [DONE] 21.3 Back 10 s and forward 10 s in the open panel; back 10 s in the minimal one.
      Built: on the tour's timeline, across steps, beside Play in every Look; back 10 s in the Mini. Changed: in the Teleprompter Look's capsule they take the place of Previous and Next (no room for both beside its microphone).
- [DONE] 21.4 The bubble always sits beside the mascot and follows it (print 19: today it stays put when the mascot moves).
      Built: Words during a tour = The bubble anchors it to the mascot's head every frame (flipping at the edge); a tip's bubble goes with the mascot's hop; A tip's bubble: beside the head (default) or above.
- [DONE] 21.5 During a tour, the words go to a teleprompter: a strip rising from the screen's bottom behind the minimal panel, about
      2-3 cm, the spoken line highlighted, the lines before and after fading (print 20, but lighter, more immersive).
      Built: the default at Mini and Compact, 184 px (about 3 cm), behind the Mini, its small row (the name, a live dot, Full script ›) at its top. Changed: Paper glass and the Teleprompter Look keep their own Mini line; a game's card or Ask hides the strip.
- [DONE] 21.6 The minimal and open panels: lighter, softer, immersive (glass, light gradients, gentle motion, a hint of 3D; prints
      21-22). Two or three looks shown in captures, the developer picks one.
      Built: Light glass, Aurora, Dark and Follows the theme (the default: Light glass in light, Aurora in dark), in Speech on screen › The panels' look; the developer picks the default.
- [DONE] 21.7 (known) Azu on the What next? panel's edge covers about 10 px of the Up next card.
      Changed: the mascot now sits inside the panel's top corner beside its header (0 px over the card); its bubble points down to it.
- [DONE] 21.8 (from round 20's core run) The self-test's words walk (Languages & Terms: every text field of every tab) takes more
      than 40 min (about 14 at round 17b) and slows as it goes: find what builds up with each field and make it fast again.
      Built: what built up was the HTML each field parses (two signatures a field, about 1.7 MB each, into detached elements: the cardScreen.html handlers redrawing the card for every view), piling up within the one long task. The signature before a field is now drawn once per tab and language, and the page's self-test walks the texts in slices of 20 fields (its JSON pending until done). -tabs words: bad=0 in 3.7 min (past 40 before); in the page: one task, old way past 10 min, signature once 1.9 min, slices 3.5 min. The guide walk bad=0.
- [DONE] 21.9 (the developer, 2026-11-14) The Editor's accordion and sticky headers on the Detail Card and Global tabs too, as on the
      Guide tab: sections collapsed on one line at start, Collapse all, the open section's and foldout's headers pinned.
      Built: Detail Card (7 sections) and Global (12) are accordions like the Guide tab: collapsed on one line at start with a plain sub-line and a count, Collapse all and One section open at a time, two pinned header levels, Languages & Terms' Go to lands under them. test9y [card & global] by real clicks (32 pass), runtest8 -tabs card bad=0; captures ed21_card_*, ed21_global_*.
- [DONE] 21.10 (from the full tests on master after round 20) 17 files red, 3 mutations not caught: fix each (a stale test after the
      renames of round 19 or the accordion of round 20: fix the test; a real regression: fix the code; say which).
      Done: 21 runs on this branch. Stale, the test fixed (round 19 renames and moved rows, round 20/21 accordions opened by real clicks, round 21's own changes): 8j 8k 8l 8o 8u 8v 8w 8y 8z 9a 9b 9da 9db 9e 9o, and the scene of 9h. Load-flaky (green here): 8s 9i 9l 9h's [room]; test9v intermittent (3 crashes, then 28/0 twice). Mutations: 9db, 9ea and 9sc now go red. Real fixes: the Aurora/Light glass gradient stops, the bubbles row's demo ring.

## Round 22 · The tour as a demo of the app

- [DONE] 22.1 The scroll: slower and natural (eased), with a quiet cue that the page is moving down or up.
      Built: every tour scroll is one eased flick (cubic in-out, 0.5-1.2 s by the distance; guide/tour-scroll.js), a soft grey pill with a chevron pulsing near the bottom going down, near the top going up, gone when it lands; a long scroll starts with the hand's swipe. Row The scroll (Natural / Quick). test10a [scroll]; captures l_/d_scroll_down, _scroll_up.
- [DONE] 22.2 A ghost hand: a small see-through hand glides in from about 2 cm away to what the guide taps, with a soft tap ripple.
      Changed: it glides from 88 px, not a true 2 cm (about 128 pt, a third of the screen, crossing other blocks and words); the guidance's ghost finger drawn at 30 px (one hand for the app), a comet trail, a 44 px grey ripple, then the visitor's own click. Never over the words told; a touch on the card removes it and pauses. Row The visitor's hand (Shown / Off). test10a [hand]; captures _hand_glide, _hand_tap.
- [DONE] 22.3 Opening a story or page during the tour plays the app's own opening (zoom from the block's preview to the page), as if the
      visitor had tapped it.
      Built: the card had no opening (the screen swapped at once): card/page-open.js zooms the page out of the preview (460 ms) for the visitor's tap and the tour's alike, and back shrinks it into the preview with the card where it was (before: the card's top). The tour scrolls to the story's preview, the hand taps it; back, it taps the page's ‹. test10a [opening]; captures l_11_open_12/35/65, l_13_close_*.
- [DONE] 22.4 Interactive blocks are shown in use: the hand drags the before/after, turns the 3D model, flips a card…
      Built: once per block per tour, after the scroll lands, never under a finger: the before and after across and back, the 3D model (and a panorama) a little and back, the flip card and back, the gallery's strip one picture and back, each by its own handlers (not counted as the visitor's try). Not the quiz. No point's tour has a 3D model: tested on one added to the card. Row Show blocks in use. test10a [blocks]; captures _use_*.
- [DONE] 22.5 No dimming: the rest of the app stays as it is. The block being read gets a slim bar on its left that fades in and
      pulses softly.
      Built: Focus = The left bar (the default): 4 px in the card's accent in the left margin, 6 px from the block (on its edge it sat on the rounded corners and pictures), the block's height, breathing softly; nothing else darker (measured). Dim the rest, Soft ring and Off kept. test10a [focus], test8j follows; captures _bar, _dim.
- [DONE] 22.6 (the developer, 2026-11-14) The Detail Card tab has two near-identical switches: the old "Foldouts: Many open / One
      at a time" and the accordion's "One section open at a time". Merge them: keep the accordion's at every tab's top.
      Done: the Foldouts row is gone; Open to level stays (it opens every section and foldout to a depth, which the accordion does not). test10a [switches]; capture ed22_card_top.

## Games and Ask (smaller, in whichever round touches them)

- [DONE] G.1 (known) An off-topic question during an open quiz gets hint 1 ("the answer is hiding there"); it should warmly say the answer
      would give the quiz away, then answer the other question.
      Built: the question is answered with what does not give the quiz away; only when something had to go, "I'd love to, but that would give our quiz away. Ask me again once you've answered!" (PT too); a question about the quiz keeps its hint.
- [DONE] G.2 (known) The card's quiz chip touches its eyebrow "… OF 3".
      Changed: on the quiz's upper edge the chip keeps 8 px or more from the eyebrow's words: its pause sign alone (44 px) just after them (with its words it touched the eyebrow and the mascot).

## Still to try from the route (no notes yet)

Steps 5-12 of the route above: Tell me more, Skip ahead, the tour's end and What next?, the Stop bar, Continue after a reload,
asking during the games, the short tours, the Editor's guide rows. Notes there join this list.
