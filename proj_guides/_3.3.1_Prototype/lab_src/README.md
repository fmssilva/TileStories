# Block Components Lab: sources

The Lab is one HTML page built from layers. Each layer adds to or wraps the one before it; nothing is edited in place.

- **Layers:** `v3.js` (the catalogues and the state) -> `v4.js` (Detail Card's organisation) -> `v5.js` (the flow, the self-test) -> `v6.js`
  (the Guide) -> `v7.js` (texts on their parts, the demo card, Languages & Terms) -> `v8a-e.js` (the Unity audit merged: card, sections
  and controls; every block kind; elements and motion; Specific Marker and languages; the hero and the editor tools). CSS in the same
  order; `v3body.html` is the page.
- **Words:** `realstrings.json` (the framework's real words, with their owner part); `build8.py` moves words to new owners and adds
  the Lab's new ones.
- **Build:** `python build8.py` writes `block-components-lab.html` here (not committed). Copy it to `../Block Components Lab.html` (the
  record copy) and publish it to https://claude.ai/artifact/KDs2Ko1dYdBWYwZVhjvuEp.
- **Prove it:** `runtest8.ps1` opens the page headless with `#selftest`: every option, text and demo field must change the phone or be
  classed behaviour (it prints `bad=0` when all is well; about 3 minutes). `shots8b.ps1` captures the demo card at each kind
  (`#dcshot=<kind>`); `#pshot=<stop>|<scroll>` opens the card at a stop and a scroll.
- **The audit** that shaped v8: `AUDIT_DECISIONS.md` and `audit/` (one report per area).
- A change: add it to the newest layer (or a new `v8f.js`), list it in `build8.py`'s `V8`, rebuild, run the self-test, look at a
  capture, then fold the decision into `_3.3`.
