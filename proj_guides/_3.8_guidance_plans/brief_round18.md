You are building round 18 in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app (say each
change's Unity path). Work only in the Lab. This round lets the visitor **ask Azu during a game**: the game pauses, Azu answers, the
game goes on. It is the last of the four features the developer accepted on 2026-11-11 (`_3.8_guidance_plans/guide_polish_plan.md`
section 5); after it comes a polish pass where the developer tries the whole guide.

READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules; the rows of `guide/ask.js`, `guide/dock.js`, `guidance/catalog.js`,
`guidance/service.js`, `guidance/present.js`, `guide/tour-end.js`, and the rule "A game inside a step"), `README.md` ("Prove it",
"Levels": this is a **feature** round), `src/guide/ask.js`, `src/guidance/present.js` and `src/guidance/service.js` (the tour's games:
the promised quiz, Guess the year, Don't say it!, the breather; their clocks and taps), the card's Knowledge Check block and its page
(`src/card/blocks.js`, `src/card/pages.js`), `src/guide/tour-end.js` (`teAskEnter` / `teAskLeave`: an Ask that keeps the card's place).

## What the developer decided

- 2026-11-11: "Ask Azu during a game (the game pauses, the answer, the game goes on)."
- 2026-11-13: when the visitor asks about the very question they are playing, **Azu gives a hint, not the answer**: a gentle clue from the
  card's words that keeps the fun; the full answer comes after they answer.
- Standing: Azu's words are a warm storyteller, never a robot; every Editor row has its ▸ demo and ? help; messages name things, not
  ids; Lab features must be Unity-buildable; never the live model in tests (stub it as the other Ask tests do).

## The design to build (the Architect's; adjust where the captures show something better, and say what and why)

First find out what happens today when the visitor opens Ask (the dock's mic or keyboard, the ✦ menu's Ask, Tap on Azu, the What next?
panel's field) while each game is on screen: the tour's quiz question, Guess the year, Don't say it!, the breather's question, and the
card's own Knowledge Check (and Poll) played outside a tour. Report it in a small table, then make them all follow one rule:

1. **The game pauses.** Opening Ask during a game freezes it where it is: its clock or timer, the chosen option, the slider, the score,
   the step of the tour. A small "Game paused" chip (44 px tall, quiet) sits on the game; the voice of the game stops at a word boundary.
   No tour restart, no scroll (reuse the 17c pattern of an Ask that keeps the place).
2. **Azu answers** in the dock as Ask Azu does today (the model, stubbed in tests; the fallback answer with the model off).
3. **The game goes on.** After the answer, a "Back to the game" button (44 px, the dock's primary style) and, with the Editor's
   "Back by itself" On, a return after the answer is heard plus N s (default 4 s; any tap or new question cancels it). Azu says a short
   warm line from a new set, Azu's intro lines › Back to the game (4 lines EN and PT, e.g. "Right, back to our game. Where were we?" /
   "Bem, de volta ao jogo. Onde é que íamos?"; picked with no AI by the point, never twice in a row), the game's clock resumes exactly
   where it stopped, and the chip goes.
4. **A question about the question: the hint ladder.** When the visitor's question is about the game question on screen, Azu does not
   give the answer. Detect it with no AI first (the visitor's words share content words with the question or its options: a small
   stop-word list EN and PT, the threshold in one place), and also tell the model the rule in Ask's context ("the visitor is playing; the
   question is …; do not say the answer; give the hint below"), so a model-on answer follows it too. The ladder, one step per ask:
   - **Hint 1, where to look:** the question's own written hint if it has one (Editor); else, with no AI, the card's block whose words
     hold the right answer, named warmly: "Have a look at “Two centuries of additions”: the answer is hiding there." / "Espreite
     “Dois séculos de acrescentos”: a resposta está lá escondida.", with a "Show me" chip that scrolls the card to that block while the
     game stays paused (and back with Back to the game).
   - **Hint 2, one wrong answer out:** for a multiple choice, one wrong option fades ("It's not José I, that much I can tell you." /
     "Não é D. José I, isso posso dizer-lhe."); for Guess the year, the band narrows ("It's before 1600."); a game with no options skips
     to step 3.
   - **Step 3, the visitor's choice:** "Shall I just tell you? Then this one won't earn its star." with two buttons, "Tell me" and "I'll
     try" (each 44 px). Tell me says the answer and its explanation, marks the question as told (no star, the header's score in
     17c's What next? panel counts it as not earned), then the game goes on to its next question.
   - A question about anything else (the place, Azu, the app) is answered normally; a game question already answered is answered in full.
   - The hint and its line words go through `core/words.js` (EN and PT) and Azu's intro lines like the other sets.
5. **Off and edge cases.** With "Ask during a game" Off, Ask during a game says "Let's talk about it once you've answered" / "Falamos
   disso assim que responder" and the game never pauses. A Stop of the tour during the pause ends both as Stop does today (the Stop bar
   with Continue). Leaving the point closes Ask and keeps the game paused for Continue. Reduced motion: no fade, an instant change.
6. **The Editor:** Guide › 3 · Games on the tour › a new foldout **"Asking during a game"**: Ask during a game On / Off; Back by itself
   On / Off and its delay (2-8 s); the hint ladder's three steps each On / Off (Where to look, One wrong answer out, Tell me with the star
   lost); the detection's strictness (Loose / Normal / Strict, a plain help on what each catches); each quiz question's own hint (EN and
   PT, beside the question where questions are written today, with Draft with AI + Reviewed like round 12's questions; never called in
   tests); Azu's Back to the game lines (a link to Azu's intro lines). Each row with its ▸ demo (a game started, paused by an Ask, the
   answer or the hint, and back) and its ? help. Words on their UI element's foldout as the other texts are.

## Tests for this round (feature tier: ~5-15 min)

- A new file `tests/test9w.js` (asking during a game), with its mutation, real taps and real typing where the visitor would:
  [today] the table's cases now all pause (each game and the card's Knowledge Check, each way into Ask); [pause] the clock, the option,
  the slider and the score frozen during Ask (the test's own clock readings), the chip shown, no scroll and no tour restart; [back] Back to
  the game and Back by itself resume exactly (the clock within 100 ms of where it stopped), the warm line from its set, not twice in a
  row, a tap cancels the timer; [about] the detection, EN and PT: questions about the question, about something else, a near miss; the
  model's context holds the rule (stubbed); [ladder] hint 1 (written hint; the block found from the card's words; Show me scrolls and
  back), hint 2 (one wrong option out; Guess the year's band), step 3 (Tell me: answer, no star, the next question; I'll try: back);
  [off] Off says its line and never pauses; [stop] a Stop during the pause; [editor] each new row changes what the visitor sees and its
  ▸ demo plays it; [reduced motion].
- The self-test of the tab you touch: `tests\runtest8.ps1 -tabs guide` (bad=0).
- test9p (the round 12 games: their clocks and taps must still work) and test9v (17c's Ask that keeps the place).
- Not core this round (core ran in 17b; this is the 2nd round since). Check the CPU before each run
  (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: wait). Add test9w to
  `tests_baseline.json`. No other files.
- Capture and look (390 px, light and dark): a quiz question paused by an Ask (the chip, the dock's answer, Back to the game); hint 1
  with Show me; hint 2 (an option faded); step 3's choice; Guess the year paused; the Editor's new foldout; in
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g18\` with `index.md`.
  Look at them as a designer: calm, one focal point, the game clearly paused and clearly back.

## Limits

- Edit only files under lab_src (captures in the scratchpad). Do not commit, do not publish, do not switch branches. No keys, no live
  model, no downloads, no Unity. Patch scripts through the Write tool or Python; keep each file's line endings. `until(...)`, never a
  fixed wait.
- Before you finish, make sure none of your test processes is still running (the tavily MCP node processes are not yours).

## Report (your final message)

Today's table (before), what the visitor sees now (each part, EN and PT words), what you changed from this design and why, the Editor
foldout, each part's Unity path, every test run with its numbers, time and CPU load, and 1-3 questions for the developer, each opening
with one plain sentence on what the feature does.
