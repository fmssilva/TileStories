# Block Components Lab: sources

The Lab is one HTML page built from layers. Each layer adds to or wraps the one before it; nothing is edited in place.

- **Layers:** `v3.js` (the catalogues and the state) -> `v4.js` (Detail Card's organisation) -> `v5.js` (the flow, the self-test) -> `v6.js`
  (the Guide) -> `v7.js` (texts on their parts, the demo card, Languages & Terms) -> `v8a-e.js` (the Unity audit merged: card, sections
  and controls; every block kind; elements and motion; Specific Marker and languages; the hero and the editor tools) -> `v8f.js`
  (close and settings pinned, the tappable grabber, the demo card's X, a change redraws in place, the Locate Button rides with the bar and its label folds after 4 s seen) -> `v8g.js` (the demo
  in one component: the tile opens Demo Components, its top button the demo card; the Hints; How to use the card) -> `v8h.js` (Loading & states) -> `v8i.js` (the guide's voice
  from the first step; the read-along light: Comet / Sentence + word / Fill / One word, Trail, Glide) -> `v8j.js` (Focus: the spotlight, the zoom to a
  spot named in a picture, Azu pointing; the Guide script's spots) -> `v8k.js` (a livelier Azu:
  2.5D, blinks, a mouth that follows the words, moods; play: poke, tickle, a fall, the water break, the cookie; Solemn). CSS in the same order;
  `v3body.html` is the page.
- **Words:** `realstrings.json` (the framework's real words, with their owner part); `build8.py` moves words to new owners and adds
  the Lab's new ones.
- **Build:** `python build8.py` writes `block-components-lab.html` here (not committed). Copy it to `../Block Components Lab.html` (the
  record copy) and publish it to https://claude.ai/artifact/KDs2Ko1dYdBWYwZVhjvuEp.
- **Prove it:** `runtest8.ps1` opens the page headless with `#selftest`: every option, text and demo field must change the phone or be
  classed behaviour (it prints `bad=0` when all is well; about 3 minutes). `shots8b.ps1` captures the demo card at each kind
  (`#dcshot=<kind>`); `#pshot=<stop>|<scroll>` opens the card at a stop and a scroll. `node test8f.js` drives the page with real
  taps, drags and keys (Playwright with the installed Edge; `#probe` opens a handle on the page's state) and prints PASS / FAIL per
  check: the sheet's gestures, the pinned close and settings, the demo card's X, and that a change moves no frame. `node test8g.js`
  does the same for the demo's flow, each hint (its moment, Off, Reduce Motion, one at a time) and How to use the card; `node test8k.js` checks
  each gesture on Azu, the moods, the mouth, the moments and their limits, Calm and Solemn; `node test8j.js` checks the
  spotlight, the zoom to each spot, Azu's arm, a scroll, Reduce Motion and drawing a spot; `node test8i.js` checks the
  voice that loads late, each light style, the glide, Reduce Motion and the words' contrast; `node test8h.js`
  shows every state in every look, light and dark, and measures its words' contrast.
- **The audit** that shaped v8: `AUDIT_DECISIONS.md` and `audit/` (one report per area).
- A change: add it to the newest layer (or a new `v8l.js`), list it in `build8.py`'s `V8`, rebuild, run the self-test, look at a
  capture, then fold the decision into `_3.3`.
