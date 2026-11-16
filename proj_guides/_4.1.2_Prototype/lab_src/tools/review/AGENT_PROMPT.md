# Instructions for a section reviewer + fixer (v19)

You are one of six parallel reviewers of the TileStories "World Editor Lab", an HTML prototype of a Unity 6.3 AR app that guides visitors over a Lisbon azulejo panel. The user wants **every section and subsection of the editor confirmed to have a component with good design for every profile option**. "Be good, not fast."

Paths are relative to `lab_src/` (run everything from there). `$REVIEW_DIR` is a working folder of your choice for the review documents, shots and verdicts (default `review_work/` beside lab_src; it is not part of the repo).

## Read first
- $REVIEW_DIR/v19/SECTION_REVIEW.md: goal, groups, ownership, rules.
- $REVIEW_DIR/v19/GROUP_BRIEF.md.
- $REVIEW_DIR/playful_spec.md: the Playful (ages 7–11) look spec.
- ../../_4.1.9d_Unity_Feasibility.md: what Unity UI Toolkit can and cannot do. **Hard rule: nothing Unity can't implement.**

## Evidence
- Contact sheets: $REVIEW_DIR/v19/sections/base/sheets/<tab>__<id>.png.
  - Rows: premium light, premium dark, playful light, playful dark, premium text L, playful text L.
  - Columns: phone portrait, phone landscape.
- Full-resolution raw shots, which you should use for detail: $REVIEW_DIR/v19/sections/base/raw/<tab>__<id>__<look>_<mode>_<text>__<device>.png.
- To make new shots (after your fixes, or tablets), run:
  `LAB=out/dev3_<G>.html python tools/review/section_shots.py $REVIEW_DIR/v19/sections/<G>/after app <id1,id2> phone_p,phone_l,tablet_p,tablet_l`
  - Device keys: phone_p, phone_l, tablet_p, tablet_l.
  - Read the PNGs with the Read tool. Crop with PIL if you need detail.
- Build your copy: `python build.py out/dev3_<G>.html`. lab_src is shared with the other reviewers, so a build includes their in-progress work. That is expected.
- Lint: `python tests/unity_lint.py` must pass (exit code 0: 0 strict problems, no ratchet increase).

## What "good" means
- **Premium (12+):** modern, premium, minimalist, immersive, a calm museum quality. Glass over the camera, refined type, restrained colour.
- **Playful (7–11):** hand-drawn azulejo style: cobalt line on tin-glaze white, 9-slice sketchy frames, the Azu mascot used sparingly. Warm and fun, like a great mobile game, but still a cultural app. It must never be childish clip-art, and it must not hide the wall.
- **Both looks, light and dark:**
  - text at least 13 pt and readable (contrast ≥ 7:1 for body);
  - targets at least 48 pt;
  - nothing clipped, cut off, overlapping or overflowing, including at text L;
  - selection is not shown by colour alone;
  - hierarchy is clear;
  - consistent with the rest of that look.
- **Over the camera:** at least 75 % of the wall stays visible, or 55 % with an open card.

## Your process (for EVERY fold in your list)
1. Look at its sheet and the raw shots for all 6 profile rows × phone_p/phone_l. Also render tablet_p/tablet_l for at least the main folds.
2. Write a verdict per profile row: OK, or the concrete problems.
3. Fix the problems in **your owned files only** (listed below). Keep Playful rules scoped to `.tapc.look-playful …` and Premium rules to `.tapc:not(.look-playful) …`. Use only Unity-feasible CSS.
4. Rebuild, re-shoot the fixed folds, and confirm by looking. Iterate until good.
5. Anything that needs a change outside your files goes to `$REVIEW_DIR/v19/sections/<G>/proposals.md`, with exact code (file, old text, new text) and the reason. Do not edit other files.
6. Write `$REVIEW_DIR/v19/sections/<G>/verdicts.md`.
   - A table: rows are folds; columns are Prem-L, Prem-D, Play-L, Play-D, Prem-textL, Play-textL, Tablet. Each cell is OK, fixed, or open plus a reason.
   - Then a short list of what you changed.
7. Final check: `python tests/regress.py out/dev3_<G>.html $REVIEW_DIR/reg_<G>.json` (prints passed/failed) must not regress, and lint must pass (exit code 0: 0 strict problems, no ratchet increase). Report the numbers.

Final answer: a concise summary (under 300 words) covering the verdict counts, the main fixes, the open items and the proposals.
