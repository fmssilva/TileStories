# Architect hand-off -- from `_3.2` POI Card Navigation to `_3.3` POI Card Styles (2026-10-22)

Read this once at the start of the new Architect chat, after section 1 "Ground" of `__AI_Architect.md`. It is REPLACED at the
next domain change (it is in git). It tells you where to look, not what to conclude: check the claims on disk.

## 1. Where `_3.2` ended

- Closed 2026-10-22. Status table all done (19 steps, split into 21 blocks); audit `_3.2.1_Audit.md` (sections 1-11 = the
  audit, 12 = the fix-list status blocks, 13 = the close-out: the chunk table 13.1-13.2, the triage 13.3, the final numbers
  13.4, and **13.5 "State of the domain" -- read those ten lines first, they are the fastest way in**). `_3.2` has NO open TODO:
  its block is empty by design, every line was triaged to a destination.
- What the domain delivers: a visitor moves inside ONE open card through **16 elements in five slots** (top, bottom, floating,
  menu overlay, edge) plus four in-flow nav blocks, in **five layouts** (scroll, accordion, tabs, pages, bento), with in-card
  facets, system Back, a full view with a breadcrumb and sibling swipe, and **nine presets** that write a sensible set in one
  undo step. All 23 settings have an Editor row, an (i), a default, undo/save, live Play Mode sync and a test.
- Also built here, though they are `_3.1`'s data: the **Card Starter** (ready-made BLOCK sets for an empty point) and the
  **demo point** -- The Painting is now the Paco da Ribeira with 11 blocks in EN + PT, and Lamp - Residential carries all four
  in-flow nav kinds.
- Last full runs (chunks D+G, commit `b184c8f`): **EditMode 2021/2021, PlayMode 1192/1192** (3311 s; Unity came to the front
  for part of it, so not a clean background run). RUN TIMES live in `40-testing.md` 4.2. The domain added 171 files and
  +39,104 lines, 77 files and 23,231 lines of them tests.

## 2. What moved where (do not re-triage -- `_3.2.1_Audit.md` 13.3 has all 38 rows with reasons)

- **`_3.3` (this new domain):** the audit's fix **chunk E** (the demo card's content and pictures), the Half-is-mostly-header
  finding, the `--ts-track-peek` tuning, the outline-contrast items, and the "not finished" looks list that came from `_3.1`.
- **`_5.1_Editor_Tab.md`:** "Preview this preset on a point", the Card Starter offered on every point, and the Editor rows'
  own debts.
- **`__mixed_TODOs.md`:** the device list (thumb checks for the radial, the scrubber, the chips, Back on Android, pages vs
  vertical scroll, the placed chip over a real camera), the harness list, the media ship blockers, and the cross-domain
  taxonomy-names-per-language item.
- **LATER / other stages:** per-point navigation overrides, "remember where the visitor was", a breadcrumb for full screens a
  block opens itself, tablet / landscape layouts, screen readers (Stage 7), packaging (Stage 6).
- Nothing went to `_3.4` (lenses): it has no file yet and no open item belongs to it.

## 3. Open risks to keep in view

- **Media licences.** Every picture on the demo card is a placeholder (one prints "PLACEHOLDER DETAIL") and the audio guide is
  an unlicensed clip. It blocks any build given to anyone else, and it blocks honest design judgement -- see section 5.
- **One harness block is owed.** The PlayMode fixtures hold **14 private copies of `Rested()`**, 4 of `InViewAndStill` and 1 of
  `StackAtRest`. One shared wait in `CardTestInput` would close four open flake / timing entries at once. The rule of "the
  third copy is the cue to share it" passed long ago.
- **Two watched flakes**, each with a known trigger and none reproducible on demand: `PoiCardAudioSceneTests` timing,
  `ConfigUndoGroupingTests.DraggingASlider` (three sightings), plus `KeywordListFieldTypingTests` focus. A fourth, "Layout
  update is struggling", now has a guard (`LayoutStruggleGuard`) that will fail the test naming the scene load -- it has never
  fired, so its value is the next time.
- **Editor tests that need the window at 880 pt** fail on a screen that cannot give it (seen at 1080x1920 @125%). They must
  fail with that sentence, not with an assertion about a row's position. In `__mixed_TODOs.md`.
- The whole domain is proven in the Editor only. **Nothing has been under a thumb.**

## 4. How the process stands (already in the guides; pointers only)

- **The test cadence changed during this domain and is the single biggest process change: `40-testing.md` 4.2.** A full
  PlayMode run is owed only when a block touches shared ground (the STOPS, LAYOUT or binding lifecycle of the sheet / stack /
  host, test infrastructure, a registry or a rule a whole domain reads) or at the fourth block since the last one -- adding a
  view inside an existing slot is not that. A block that closes without one names its fixtures (for a card-only change, every
  PlayMode fixture whose name holds Card or Poi: ~950 tests, ~50 min against ~55 for the full suite). **A full run confirms, it
  never discovers:** the targeted set goes green first. It saved roughly half the test time of this domain with no regression
  reaching a commit.
- Block size, Worker model tiers, the brief template, "Your tasks now": `__AI_Architect.md` sections 4-6. Sonnet-class was
  used for 4 of the 21 blocks (elements on a built pattern, Editor rows, the close-out) and held every time; Opus for a new
  owner, a container, persisted state, input at the root, gesture maths or a refactor.
- Domain close-out flow: `__AI_Architect.md` 5b (audit -> fix chunks -> a short second AUDIT pass -> this hand-off -> new chat).

**Four habits that earned their place in this domain -- keep asking for them in every brief:**
1. **Mutation proof.** "Break it, watch the named test go red, restore it." It found **three tests that proved nothing** (a
   closed-row guard the stack had already decided with a NaN; a 0 px viewport test whose block was off screen anyway; a tabs
   check reading a cleared track), and the audit's sweep of 144 breaks found 10 real gaps including three options a wall
   author could choose that nothing tested.
2. **Look at the render.** Every single block from 5a to 18 had a defect the green tests missed and the capture caught: an (i)
   column 70 pt off, a candle-shaped icon, a scrim too weak (the project renders in LINEAR colour -- a "50%" scrim measured
   181/247 and needed 0.7; it is written in `_3.3`), a 300 px grid, five radial geometry faults, a chip centred on a stale
   pooled layout, a word broken mid-way.
3. **Measure before you change, and say the number.** The sheet's three stops (195.00 / 337.60 / 796.00) were measured before
   and after every block that could move them, which is how a refactor of the sheet could be called behaviour-free.
4. **A differently shaped fixture finds what the big one cannot.** The Painting has one-kind sections; The Lamp never did, so
   two real bugs hid behind 62 blocks until a small card existed.

## 5. First job in the new chat

**Do NOT start building `_3.3`. Re-plan it first.** `_3.2` and `_3.3` were written on the SAME day (2026-09-26), before a single
block existed; re-planning `_3.2` against the built card was the best decision of that domain, and half of what ended up
mattering was missing from the one-pass plan. `_3.3` has the same age and the same problem, and the developer has already said
what it is thin on. The instructions are written at the top of `_3.3_POI_Card_Styles.md`, in "Before this domain is planned":

Run `proj_guides/skills/Domain_Planning.md` for `_3.3`, grounding on the BUILT card, and make it cover:
- **Space, rhythm and the definition of a block** as its own step. Today it is one sub-line ("structural recipes per preset")
  and it is probably the biggest single lever on the "modern, minimal" look the developer wants: the 8 px grid the tokens
  already imply, when a block is a surface with elevation and when it is only space, the hierarchy between header, section and
  block.
- **Motion, which has no step at all.** There is a `--ts-duration` token and `_3.1`'s `image_parallax` header look, and nothing
  else: no answer to what animates, for how long, with what easing, how the card enters, what a gesture answers with, what
  parallax means as a system, and exactly what `reduce_motion` switches off in each case. The developer asked for parallax,
  immersion and motion by name.
- **Research with citations**, as `_3.2`'s re-plan did. This is a thesis and "modern and minimal" has literature.
- **Fold in both lists of things that already look unfinished:** this guide's own TODOs (from `_3.1.1` section 9) and
  `_3.2.1_Audit.md` section 9's eight items on The Painting.
- **Keep ONE domain.** A preset is its colour AND its spacing AND its motion; splitting them would put one decision in three
  files. Widen the title to cover look, feel and motion.

**And its step 0 is CONTENT, not a token.** Design judged on placeholder art is wasted work. The developer's call (2026-10-21):
the LivingRoom is a TEST wall, so a Worker may gather real material about the Castelo de Sao Jorge and the Paco da Ribeira --
text, pictures, a clip -- and put it in the wall's media folder. What matters is **content of real shape and size** (real
lengths of prose, real picture proportions, a real audio length) so every block and every preset can be judged honestly;
historical accuracy is not the point for a test fixture. Prefer freely licensed sources where they cost nothing (the Paco da
Ribeira was destroyed in 1755, so its engravings are long out of copyright) and record each file's source. This does NOT clear
the ship blocker for a real build.

Then the first `_3.3` Worker brief.
