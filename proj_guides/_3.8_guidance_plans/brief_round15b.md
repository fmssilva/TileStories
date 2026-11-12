You are building round 15b in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app: every
look must be buildable there (UI Toolkit: USS, VisualElements); say each change's Unity path. Work only in the Lab.

Round 15 was the design pass of the AI guide (captures and notes in
`C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g15\index.md` and `g15\open\`,
the rules as arithmetic in `tests/g15lib.js` with test9s, 9sb, 9sc, 9sd). Round 15b applies the developer's answers and fixes one old defect.

READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules, "Where a change goes"), `README.md` ("Prove it", "Levels"), the round 15 paragraph in
PROTOTYPE_STRUCTURE, and `C:\Users\franc\Desktop\TileStories\proj_guides\_3.3_POI_Card_Styles.md` section 11. Grep before you edit.

## The developer's answers (2026-11-11; do not reopen them)

1. **Mascot Off = the ✦ alone, the bubble signed.** With Mascot Off, Azu's picture no longer stands beside the ✦ (on the card and on the
   wall), and the plain bubble at the target carries a small ✦ mark in its corner, so a voice-less bubble still says who speaks (capture
   `g15\open\q2_mascot-off_C-signed-bubble.png` is the look). The mark is not a tap target and covers no word of the bubble. Unity: the
   bubble's USS pseudo-element (a child VisualElement with the ✦ sprite). With the mascot on, nothing changes.
2. **A point's name mid-sentence takes a lower-case article:** "That's the Torreão right there", "É o Torreão" ("The" becomes "the";
   PT "O/A/Os/As" become "o/a/os/as"), applied when a name fills a slot in a remark, a tip, an invitation or Azu's lines, and only
   when the slot is not the first word of the sentence. The stored names stay as written (the marker's label still says "The Torreão").
   One function, one home (where the slots are filled); every slot filler uses it. Unity: the same string rule where the localized
   string's slots are filled.
3. **The ring for the ✦ with the 3D model: none** (as today). Nothing to change.
4. **Ink glass stays solid at Open and Full** (as round 15 built it). Nothing to change.
5. The rest stay as round 15 recommended (nothing to change): the PT opening line as today; Azu Medium beside a marker; the ✦ on the
   card on the left, hidden during a tour; a group's own gap as today; the remarks' cap later; Paper glass Open's rows 6 px apart; Azu
   hidden under the panel while listening.

## The old defect (found by the Architect; on master before round 15 too)

After a quick step jump in a tour (the test's own way: `stopSpeech();G.i=j;runStep()` at steps g5 and g7, light and dark, Mini), Azu
sometimes settles **over the words** and stays there: 189-271 px² of text under Azu after it has stood still for 1.5 s (2 of 18 probes on
master, 1 of 18 with round 15). The probe is
`C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g15\probe_azu.js`
(run it with node; it prints `still=` and `(+1.5s)=` per case). Find why (likely Azu placed against the block before the block's scroll
lands, then never placed again), fix it in the module that places Azu (one home; no wrappers), and make the probe read 0 in every case
over several runs (raise its repeats to 6). Unity: the same re-placement on the ScrollView's scroll end.

## Tests

- Add the checks to the round 15 family (`tests/g15lib.js` and the file whose surfaces they belong to, or a new short `test9se.js`):
  Mascot Off (no Azu picture beside the ✦ on the card and the wall; the bubble's ✦ mark present, inside the bubble, 0 px over its words);
  the article (EN and PT, a slot mid-sentence lower-cased, a slot at the start kept, the marker label unchanged); Azu 0 px over the words
  after a quick step jump, 6 repeats at g1, g5, g7, light and dark, measured after Azu stands still and again 1.5 s later.
- A mutation (`MUTATE=1`) that turns each new check red.
- Then, one at a time, after checking the CPU (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`;
  above about 40 %: wait): the new checks' file, test9da, test9s, test9sc, the files of every module you change (README's "Prove it"),
  and the self-test `tests\runtest8.ps1 -tabs guide` (bad=0). Never `-tabs guide,words`. test9da showed one timing red per run on this
  busy machine (on master too): if it reds, run it again alone and report both.
- Update `PROTOTYPE_STRUCTURE.md`, `README.md` and `tests_baseline.json`; capture the three changes (390 px, light and dark) in
  `scratchpad\g15b\` with a short `index.md`.

## Limits

- After each change only `python build.py`; its check must read 0/0/0/0.
- Edit only files under lab_src (captures in the scratchpad). Do not commit, do not publish. No keys, no live model, no downloads, no Unity.
- Write patch scripts with the Write tool (bash heredocs mangle apostrophes); keep each file's line endings.
- A check that reads a state right after an action races the app's timers: `until(...)`, never a fixed wait.

## Report (your final message)

What changed where (file, Unity path), the cause of Azu over the words, the probe's numbers before and after, every test file run with
its numbers and time, the mutation, the CPU load, the captures' path, and 2-3 questions for the developer if any remain.
