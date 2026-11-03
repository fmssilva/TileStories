# Block Components Lab: sources

The Lab is one HTML page built from layers. Each layer adds to or wraps the one before it; nothing is edited in place.

- **Layers:** `v3.js` (the catalogues and the state) -> `v4.js` (Detail Card's organisation) -> `v5.js` (the flow, the self-test) -> `v6.js`
  (the Guide) -> `v7.js` (texts on their parts, the demo card, Languages & Terms) -> `v8a-e.js` (the Unity audit merged: card, sections
  and controls; every block kind; elements and motion; Specific Marker and languages; the hero and the editor tools) -> `v8f.js`
  (close and settings pinned, the tappable grabber, the demo card's X, a change redraws in place, the Locate Button rides with the bar and its label folds after 4 s seen) -> `v8g.js` (the demo
  in one component: the tile opens Demo Components, its top button the demo card; the Hints; How to use the card) -> `v8h.js` (Loading & states) -> `v8i.js` (the guide's voice
  from the first step; the read-along light: Comet / Sentence + word / Fill / One word, Trail, Glide) -> `v8j.js` (Focus: the spotlight, the zoom to a
  spot named in a picture, Azu pointing; the Guide script's spots) -> `v8k.js` (a livelier Azu:
  2.5D, blinks, a mouth that follows the words, moods; play: poke, tickle, a fall, the water break, the cookie; Solemn) -> `v8l.js` (Audiences: Family /
  Explorer / Expert, the first-card screen, Settings, looks, per-field overrides with coverage and Draft with AI, length) -> `v8m.js`
  (Lenses: "See it as" chips, tags per block, story row and guide step; no backdrop blur, as in Unity) -> `v8n.js`
  (the guide as a person: introduction, the invitation ladder, scroll keeps it going, the block placed above the panel, a voice
  that survives a pause; the page is the phone and the Editor, the phone takes the window's height) -> `v8o.js` (the Guide Dock: Mini / Peek / Full, the bubble, the talk
  button and its states, the conversation, suggested questions) -> `v8r.js` (recorded voices from `../voicebench`: each word at its own
  time, pause / resume / mute on the audio, never a stale recording; the blind Voice test) -> `v8p.js` (Ask about the view: the points in view as text,
  the middle one, the model's tools to light points and draw routes, suggestions, a treasure hunt) -> `v8q.js`
  (Azu, the tile of light: a WebGL shader, glazed tile to holographic dot cube; the peek from behind a block; the walk to the
  wall) -> `v8s.js` (Azu as the guide's button, its remarks on what the camera sees, the Belém test wall to pan and zoom, a
  tour raises the card to Full) -> `v8t.js` (Ask Azu, the live agent: one model adapter, a manifest of 8 tools with policies,
  link cards, the Editor's panel and test console) -> `v8u.js` (the agent tested in the prototype: test scenes with checks,
  a trace under each answer, every point named, the Belém wall's own library) -> `v8v.js` (the card's markers ride on the wall's
  picture, the Ribeira wall shows its points, the preview bar's Wall button, the markers' test scene) -> `v8w.js` (Azu as a 3D
  model: the GLB from `../../mascote/3d/`, embedded by the build, drawn by three.js from jsdelivr; Mascot source, Play a clip,
  Import, Generate from a picture). CSS in the same order;
  `v3body.html` is the page.
- **Words:** `realstrings.json` (the framework's real words, with their owner part); `build8.py` moves words to new owners and adds
  the Lab's new ones.
- **Build:** `python build8.py` writes `block-components-lab.html` here (not committed). Copy it to `../Block Components Lab.html` (the
  record copy) and publish it to https://claude.ai/artifact/KDs2Ko1dYdBWYwZVhjvuEp.
- **Prove it:** `runtest8.ps1` opens the page headless with `#selftest`: every option, text and demo field must change the phone or be
  classed behaviour (it prints `bad=0` when all is well; about 10 minutes, so run it alone: other headless tests beside it slow it past its limit). `shots8b.ps1` captures the demo card at each kind
  (`#dcshot=<kind>`); `#pshot=<stop>|<scroll>` opens the card at a stop and a scroll. `node test8f.js` drives the page with real
  taps, drags and keys (Playwright with the installed Edge; `#probe` opens a handle on the page's state) and prints PASS / FAIL per
  check: the sheet's gestures, the pinned close and settings, the demo card's X, and that a change moves no frame. `node test8g.js`
  does the same for the demo's flow, each hint (its moment, Off, Reduce Motion, one at a time) and How to use the card; `node test8t.js`
  checks the agent with a test double and with fetch faked for Gemini and the Claude API (no network): each policy, wrong ids,
  citations, the fallbacks, the wall, the Editor and the console. `node test8t_live.js [dir]` asks the real model with this
  computer's key (by hand only: it spends the quota); `node test8u.js` checks the trace, the names on the points, the remark that
  follows the camera and the test scenes with their checks; `node test8u_live.js [dir]` presses Run all scenes with the real
  model, then drags the wall and types a question, as the developer does by hand; `node test8v.js` checks the markers on the picture at rest, after a real drag and a zoom, the Ribeira wall's points, the
  Wall button and the markers' scene in the console (with a mutation: markers left where they were turn it red); `node test8w.js`
  (WebGL by SwiftShader, needs the network for three.js) checks the 3D Azu: the source, the model's clips and morphs, its frame,
  each state's clip, the mouth, the blink, the peek, Reduce Motion, Import (the real GLB and a bare one) and the fallback with
  no network; `node test8s.js` checks the test wall, the pan and zoom,
  Azu's remarks and their taps, the button's rules and the rise to Full; `node test8q.js` (WebGL by SwiftShader)
  checks the shader drawing, its forms by state, the mouth, the peek, the walk and its arrival, Tile, Reduce Motion;
  `node test8p.js` checks the pill, the
  pins, the context, each suggestion, the hunt, moving the phone, the model's tools and a card opened from a pin;
  `node test8r.js` (needs two voice sets
  rendered by voicebench) checks the recorded playback, its word times, pause, mute, an unrendered line and the Voice test;
  `node test8o.js` checks the
  dock's heights, the bubble and the block's words, the talk button (a stand-in recogniser), the conversation, Classic and the
  words' contrast; `node test8n.js` checks the
  page, the introduction, pause and resume of the voice, a failing voice, the gesture mid-sentence, the asks, a scroll and the
  placing; `node test8m.js` checks the
  chips, what steps aside, the heading rule, the scroll that stays, the guide by lens and the tag table; `node test8l.js` checks
  the overrides and their fallback, the review rule, the looks, the guide's length, Settings, the first card and the coverage;
  `node test8k.js` checks
  each gesture on Azu, the moods, the mouth, the moments and their limits, Calm and Solemn; `node test8j.js` checks the
  spotlight, the zoom to each spot, Azu's arm, a scroll, Reduce Motion and drawing a spot; `node test8i.js` checks the
  voice that loads late, each light style, the glide, Reduce Motion and the words' contrast; `node test8h.js`
  shows every state in every look, light and dark, and measures its words' contrast.
- **Keys:** the local Lab's live agent reads `keys.local.js`, which `python keys_local.py` writes from the workspace's
  `.secrets/keys.env` (git ignores both); `python keys_local.py --check` asks each service whether its key works. The headless
  checks (`#probe`) never load it; `#probe-live` does. See `../../__Services_And_Keys.md`.
- **The audit** that shaped v8: `AUDIT_DECISIONS.md` and `audit/` (one report per area).
- A change: add it to the newest layer (or a new `v8v.js`), list it in `build8.py`'s `V8`, rebuild, run the self-test, look at a
  capture, then fold the decision into `_3.3`.
