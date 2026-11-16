# v20 brief: what the user asked for (translated from Portuguese), and the rules

TileStories World Editor Lab: an HTML prototype of a Unity 6.3 AR app over a Lisbon azulejo panel. Source is in `lab_src/` (work from inside it); build with `python build.py out/<file>.html`.

There are two looks:
- **Premium (12+):** modern, premium, minimalist, calm museum quality, with glass over the camera.
- **Playful (ages 7–11):** a hand-drawn azulejo style: cobalt line on tin-glaze white, sketchy 9-slice frames, and Azu the tile mascot, used sparingly. It should feel as fun as a great mobile game while still being a cultural app.

Both looks come in light and dark, with text sizes S, M and L.

## The user's requests
1. **Onboarding:**
   - Add the LOOK dimension (Premium / Playful). Picking one makes the page restyle automatically, so the visitor sees the difference at once.
   - Put small, optional icons beside it: light/dark, language and text size. They must be well positioned and never mandatory.
   - Audience: two simple buttons, with **Ages 7–11 on the left and 12+ on the right**. Under them goes ONE card that shows the difference in practice (a mini POI or guide example), instead of putting descriptions inside the buttons.
   - Then duration, then optional interest categories at the end.
2. **Settings:**
   - The settings/profile page must be refined and much more complete, organised by **areas**, with "the level of options that is good to give: fun to use, everything the user wants, but not overwhelming". The app's own options belong here too (e.g. show the mini-map or not).
   - The user suggested a gear icon at the top, left of the search bar.
   - Decision taken: the gear opens Settings, and "You" stays as the visitor's personal panel (saved places, progress, album, plus a Settings row).
3. **Editor, Main AR screen:**
   - "Lenses" is renamed to the clearer **"Category filter"** (done).
   - Add an **Action bar** subsection: choose how many buttons (2–5) and which ones, in order (done: App › 4 › "Action bar · the main buttons").
4. **On tap:**
   - The **immersive card** "does not look good: too much text, the buttons have no life, the focus on the rest of the screen is too strong."
   - The JS now gives it a photo on top (a child `.im`), the name, two clamped lines (`.sm`), and an action row `.irow.acts`: the first button `.ii.pri` is the story, in the accent colour, followed by icon buttons with short labels. The spotlight is lighter.
5. **Floating panel:** show the normal buttons like the immersive card (now the same `.irow.acts`).
6. **"Focus only" style:** removed.
   - New "Coming soon" state for POIs marked on the wall without content yet. The name/title is always shown, with a line saying the content isn't there yet and we're working on it, worded for each profile option.
   - `isSoon(p)`, `soonText()`, `.soonb` badge, and `.tv-soon` page (the "What's coming" page).
   - Two new POIs use it: "Hilltop convent" and "Arcaded house".
   - Editor: App › 5 On tap › "Coming soon", and Specific Marker › Content.

## Code map (base already implemented)
- `js/34_profiles.js`:
  - onboardingScreen, with `lookSample` and `audienceExample` (`.oex`)
  - settingsScreen (areas: `.sarea`, rows: `.srow` / `.srow.stk`)
  - youScreen (`.ds-you`)
  - prefs (`D.profile.prefs`), `D.app.soon`, isSoon
- `js/31_main.js`: BAR catalogue, gear `.mgear` in `.msrow`, `.mbar.nN`, actionBarBody.
- `js/30_tap.js`: immersive / panel / soon, actRow, shortOf, the soon card view.
- `js/24_tab_app.js`: editor sections (app_set, tap_soon, soonDemo).
- `css/ds.css`: the block at the end, "v20", holds the base structure for both looks.
- Look-specific refinements go in:
  - Playful: `css/kids_*.css`, scoped `.tapc.look-playful …`
  - Premium: `css/prem_*.css`, scoped `.tapc:not(.look-playful) …`

## Evidence tool
`LAB=out/<your build>.html python tools/review/state_shots.py <outdir> <states|all> <devices> [profiles]`
- states: onb, onb_end, main, main_filter, settings, settings_end, you, imm, pan, soon, soon_chip, soon_page
- devices: phone_p, phone_l, tablet_p, tablet_l
- profiles: `all` (6 rows: premium light, premium dark, playful light, playful dark, premium text L, playful text L), or e.g. `prem:l:m,play:d:m`
- Output: sheets in `<outdir>/sheets/<state>.png`, raws in `<outdir>/raw`. Look at them with the Read tool, and crop with PIL when you need detail.

## Rules
- **Unity-feasible CSS only.** `python tests/unity_lint.py` must pass (exit code 0: 0 strict problems, no ratchet increase). Never use backdrop-filter outside the camera glass, filter, mask, clip-path, 3D, or arbitrary cubic-bezier. Gradients and soft shadows need a `/* unity: ramp */` or `/* unity: sprite */` comment.
- **Text and targets:**
  - text at least 13 pt (labels at least 11 pt), with body contrast at least 7:1;
  - touch targets at least 48 pt;
  - selection never shown by colour alone;
  - nothing clipped, overlapping or overflowing, including at text L and in landscape.
- **Over the camera:** at least 75 % of the wall stays visible, or 55 % with a card open.
- **Tests:**
  - Regress: `python tests/regress.py out/<build>.html $REVIEW_DIR/r_<G>.json`, 0 failed. 
  - Your build must not break anyone else's area.
- **Be good, not fast.** Check every profile row on phone_p and phone_l, plus tablet_p and tablet_l for your states, and iterate until each one is genuinely good.
