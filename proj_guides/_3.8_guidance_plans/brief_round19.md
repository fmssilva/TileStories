You are building round 19 in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app (say each
change's Unity path). Work only in the Lab (and the voice bench, below). This round is **"One mascot, one voice"**: the mascot's global
settings in one place that every part of the guide reads, its name as a setting, every line in the good recorded voice, a warmer first
introduction, and five small fixes the developer found while trying the Lab.

THE CHECKLIST: `C:\Users\franc\Desktop\TileStories\proj_guides\___todo.md`, the section "THE CHECKLIST", items **19.1 to 19.11**. The
developer's own notes and prints are above it in that file; read them. Mark an item `[DONE]` there (replace `[ ]`) only after it is built,
looked at in your captures and tested; an item you change or cannot finish gets one line under it saying what and why. Keep the file's
CRLF line endings; edit only items 19.x.

READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules; the rows of `guide/voice.js`, `guide/voice-test.js`, `guide/person.js`,
`mascot/*`, `editor/guide-tab.js`, `core/words.js`), `README.md` ("Prove it", "Levels": a **feature** round),
`..\voicebench\README.md`, `..\voicebench\export_script.js`, `src/guide/voice.js` (sayOrder, recSay, currentSet), `src/editor/guide-tab.js`
(GUIDE_SECS, GUIDE_PLAN), `src/mascot/mouth.js`.

## What the developer decided (2026-11-13)

- "We need some global AI Guide voice and mascot and other settings in Guide › 1 · Azu… and then all the AI guide sub-components and
  features should use those global settings, not local hard-coded ones."
- "In the editor sections and the whole app we are hard-coding the name Azu. Use the clear name 'mascot'. Add to the mascot's global
  section the definition of its name and the presentation it gives the first time it talks."
- The voice: "check the voice we are using as default during the Ribeira Palace and use that one". That is **Azure Ryan / Duarte**
  (`azure-ryan-duarte`: en-GB-RyanNeural, pt-PT-DuarteNeural): with no set chosen, `currentSet()` picks the set with the most lines, and
  all three have 58, so the first wins. Make it the explicit default (not "the most lines").
- Standing: the mascot's words are a warm storyteller, never a robot; every Editor row has its ▸ demo and ? help; messages name things,
  not ids; Lab features must be Unity-buildable; never the live model in tests.

## Why the visitor hears the device voice today

The recorded sets hold the 106 lines `export_script.js` collected on 2026-11-03 (the steps and eleven fixed words). Every line written
since (the first introduction, the quiz promise "Just so you know…", the mascot's intro lines sets, the tour's links, Tell me more, the
tour's end, Back to the game, the hint lines, the games' lines…) has no file, so `sayFrom` falls through to `sayDevice`.

## The design to build

1. **Guide › 1 · The mascot: the global settings (19.1, 19.2).** One section, in this order, each a foldout:
   - **Who:** Name (EN and PT, default "Azu"); First introduction (what it says the very first time it talks, EN and PT, with Draft with
     AI + Reviewed as round 12's questions have; never called in tests); and where the name shows.
   - **Look:** today's rows (Character, Mascot source, Glaze… Model tools).
   - **Voice:** The voice (the recorded sets, named by their voices: "Ryan · Duarte (Azure)", default Ryan · Duarte); Speed; A line with
     no recording (the device voice, or silent with the words on screen); the voice of live answers (today's Ask › Voice of the answers,
     moved here, with a link from Ask); Coverage: "212 of 214 lines recorded" with the list of lines that would use the device voice
     (each naming where it is said, never an id), and Speaking now. Narration › Voice and Recorded voice move here; leave a one-line
     link where they were.
   - **Speech on screen:** a placeholder foldout, "The bubble, the teleprompter and the highlight (round 21)", holding today's highlight
     and captions rows if they are guide-wide.
   - **Manner** and **Moments:** today's rows.
   Then the audit: list (in your report, a table) every place in `src` that chooses a voice, a speed, the name, or a highlight on its
   own, and route each through these settings (one function each, e.g. `mascotName()`, the voice through `say()` only). Unity: a
   ScriptableObject `MascotProfile` (name, intro, look, voice, speech style) every guide component reads.
2. **The name (19.3).** In the Editor: "Azu" becomes "the mascot" in every section, foldout, row, help and message (1 · The mascot,
   4 · The mascot speaks up, 5 · Ask the mascot, The mascot's intro lines, Tap on the mascot…). For the visitor: every word with "Azu"
   in `core/words.js` (EN and PT) uses `{name}`, filled from the setting; a test changes the name and sees it on the dock, Ask, the
   bubbles and the panels. Tests that find Editor rows by their old labels: update them (grep `tests/` for "Azu"); list each file.
3. **Every line recorded (19.4).** Extend `..\voicebench\export_script.js` so it collects **every line the guide can say** for the demo
   points, both languages, all audiences: the steps, every set of the mascot's lines (intro lines, links, Tell me more, the tour's end,
   Back to the game, the hints, the games' lines, the quiz promise, the Stop and resume lines…), each with the point's name and numbers
   filled as the visitor hears them. Then render only what is missing, in the default voice:
   `python render.py azure --en en-GB-RyanNeural --pt pt-PT-DuarteNeural` (it skips lines already rendered, reads the key from
   `.secrets/keys.env` itself; Azure F0, free at this size; keep the mouth curves). **Never print, copy or write a key anywhere.** Do not
   render the other sets. Lines that cannot be recorded (the model's answers, the visitor's own words) are said in the Editor's
   Coverage as "live" lines. Report the counts before and after, and the characters sent.
4. **The first introduction (19.5).** Rewrite it: warm, human, a little exciting, short (two sentences, about 8 s), in the mascot's name,
   and it names a control the visitor can really see on screen now (the dock's mic or the ✦ menu as they look at that moment), never an
   unclear "tap Ask". Write 3 options EN and PT in your report and use the best as the default. It is said in the recorded voice.
5. **The quiz promise (19.6).** In the recorded voice; its dark card ("AZU · QUIZ", now the name) must not cover the hero's title or
   subtitle (print: it covers "Royal palace, lost in 1755"); the test's own arithmetic: the card's top below the subtitle's bottom, or
   the card placed elsewhere so neither is covered.
6. **The wall's "Tap here." bubble (19.7).** Today it sits above the mascot, its tail not clearly meeting the mascot's head. The bubble
   sits beside the mascot's head, its tail touching it (within 4 px, the test's arithmetic), and the mascot points at the marker.
7. **Opening the card (19.8, 19.9, 19.10).** Today, when the Ribeira Palace opens, the mascot vanishes behind the card, and "Start the
   guide · 3 min" is cut by "Find it on the wall". Now: the mascot stays in sight at the hero's bottom left, standing beside "Start the
   guide", pointing at it; the button moves right of the mascot; "Find it on the wall" moves so nothing overlaps (each text fully shown,
   each target ≥ 44 px, the test's arithmetic at 390 px). Once the card settles, "Start the guide" gets a calm "tap me" life: a soft
   glow and a slow pulse (like a premium call to action, print 22), stopping once tapped; reduced motion: the glow only, no pulse.
   Unity: a USS transition or DOTween loop on the button's glow image.
8. **"Before I tell you, a guess" (19.11).** It moves on too fast. Find its timer; make the default longer (say what you chose and why,
   e.g. 8 s), the first guess of a visit longer still (+4 s), and an Editor row for both.

## Tests for this round (feature tier: ~5-15 min)

- A new file `tests/test9x.js` (the mascot's global settings), with its mutation, real taps: [settings] the section's foldouts in order,
  each row's ▸ demo; [name] a new name shows everywhere the visitor reads it, EN and PT, and nowhere in the Editor does "Azu" remain
  (the test walks the Guide tab's text); [voice] the default set is Ryan · Duarte by id, not by count; every line of the script for the
  Ribeira Palace has a recording (stub the audio; the coverage list empty but for live lines); the first introduction and the quiz
  promise are said by the recorded source (`G.sayNow.src`); [audit] no module calls `speechSynthesis` or reads a voice outside
  `voice.js`; [intro] the new words, warm (not "Now", not "tap Ask"), EN and PT; [promise] the card covers neither title nor subtitle;
  [tap here] the tail meets the head; [card] the mascot visible beside Start, no overlap, ≥ 44 px, the glow then none after a tap;
  [guess] the longer time, longer the first time; [reduced motion].
- The self-test of the tab you touch: `tests\runtest8.ps1 -tabs guide` (bad=0).
- The test files whose Editor labels you changed (list them), test9t (the tour's lines) and test9w (asking during a game).
- Not core this round (core comes in round 20). Check the CPU before each run
  (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: wait). Add test9x to
  `tests_baseline.json`.
- Capture and look (390 px, light and dark): the Editor's 1 · The mascot with its foldouts open; Coverage; the wall's Tap here bubble;
  the card just opened (mascot beside the glowing Start, Find it on the wall); the quiz promise over the hero; in
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g19\` with `index.md`.
  Look as a designer: calm, one focal point, nothing cut or covered.

## Limits

- Edit only files under lab_src and `..\voicebench` (export_script.js, out/azure-ryan-duarte, out/voicesets.js, script.json), plus the
  checklist's 19.x items. Captures in the scratchpad. Do not commit, publish or switch branches. No live model, no downloads, no Unity;
  the only network call is the Azure render above. Patch scripts through the Write tool or Python, never a bash heredoc with apostrophes;
  keep each file's line endings. `until(...)`, never a fixed wait.
- Before you finish, make sure none of your test processes is still running (the tavily MCP node processes are not yours).

## Report (your final message)

The audit table (before and after), what the visitor sees now (each part, EN and PT words; the three introduction options), what you
changed from this design and why, the Editor's new section, the voice coverage (counts before and after, characters sent), each part's
Unity path, every test run with its numbers, time and CPU load, the checklist items you marked, and 1-3 questions for the developer, each
opening with one plain sentence on what the feature does.
