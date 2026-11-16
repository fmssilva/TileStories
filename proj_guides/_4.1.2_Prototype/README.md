# _4.1.2 Prototype -- the World Editor Lab and its study pages

Prototypes for the World phase (`_4.1`). They are the **reference for look, timing and precision**, not code to port: each
effect names the Unity mechanism it maps to (`_4.1.1_World_Design_Research.md` section 4).

## What is in this folder

| File | What it is |
|---|---|
| `World_Editor_Lab.html` | **The one current Lab.** One self-contained page: the app on a phone or tablet (left) and a Unity-style POI Editor (right). Built from `lab_src/`; never edit it by hand. Published at https://claude.ai/artifact/AmWUgE6nA9VLY7YeoUB4ir (always republished over that link). |
| `lab_src/` | The Lab's source: JS and CSS parts, media, build script, tests and review tools. `lab_src/README.md` explains every file and the safety net. |
| `_archive/` | Snapshots of the source (`lab_src_v19.tgz`, `lab_src_v20.tgz`). |
| `lab_v12.zip` | An older source snapshot, kept as a record. |
| `Study1_Wall_Effects.html` | Superseded study: wall effects on a photo of the painting. |
| `P1_Region_Geometry.html` | Superseded study: drawing and reshaping a region. |
| `Region_Editor_Lab.html`, `Region Editor Lab_exported.html` | Superseded study: region geometry and style in one Editor-like page. |

The study pages are records only. They run on their own in a browser and are not maintained. Their live copies:
- Study 1: https://claude.ai/artifact/NjRLSTLwgv5hWf5Tm9pgAm
- P1 Region Geometry: https://claude.ai/artifact/LxjMpnDXQAj1sZ753aHGo5
- Region Editor Lab: the old account's link, retired.

## The World Editor Lab

The page is full height, with no title or text above or below the view. The view has the screen tabs, the device buttons
(phone, tablet; click again for portrait / landscape), ▶ / ↻, Undo / Redo and a ⌨ button for the shortcuts. The editor
scrolls on its own. Every screen is seen through the device, with a 3D camera (scroll, drag, Alt+drag, Shift+scroll; see ⌨).
The wall is the real Grande Panorama de Lisboa, with sample photos of the castle.

**Screens (left)**
- **Wall** -- the wall as the visitor sees it (▶) and the Scene view for drawing shapes (Specific Marker).
- **Style** -- the shapes that use the selected Region Style, looping intro → idle → focus → back.
- **Precision** -- the selected style at 0, 2, 4 and 6 cm of tracking error, the true contour dashed.
- **Wall Life** -- the six kinds on the painting; each row's anchor with its K code; ▶ plays the rows on their triggers.
- **App** -- the app's screens on the phone: Start, Onboarding, Main AR, a tapped POI, Explore, Settings, You, the album,
  Coming soon.

**Editor → screen.** A click or focus anywhere in the editor opens the screen that shows it. Every section has a demo, except
the plans with nothing built yet.

**Editor tabs (right)**, above them the profile bar (look, mode, audience, time, text) that switches the visitor profile from
any tab:
- **App**: 0 Profiles · 1 Design system (the UI kit) · 2 Start · 3 Onboarding · 4 Main AR (category filter, action bar,
  pointing) · 5 On tap (base elements, tags, actions, styles, coming soon) · 6 Explore · 7 Settings and You.
- **Global Scene**: the wall and its content, the looks (point, region, label, tag styles), levels, Wall Life, behaviour, test.
- **Specific Marker**: per POI: identity, position, presentation (shapes, on tap, alive), content (ready / coming soon), card.
- **Experiences**: tours and routes, games, wall layers, capture and share.
- **Decisions**: what is settled and what is open.

Two looks: **Premium** (calm glass over the camera) and **Playful** (hand-drawn azulejo, the Azu mascot; ages 7-11).

## Where to read more
- `lab_src/README.md` -- the source, the build, the tests, the Unity lint.
- `../_4.1.3_World_Editor_Spec.md` (the editor and the Region spec), `../_4.1.4_Wall_Life_Spec.md`,
  `../_4.1.5_World_Features_Plan.md` (feature map, App screens, On tap).
- `../_4.1.7_App_Design_System.md`, `../_4.1.8_Markers_And_Regions_Design.md`, `../_4.1.9_Visitor_Profiles.md`,
  `../_4.1.9b_Playful_Look_Spec.md`, `../_4.1.9d_Unity_Feasibility.md`, `../_4.1.12_Onboarding_Settings_OnTap.md`.
- `../_4.1.6_Session_Log.md` -- the version history; `../_4.1.13_Handoff.md` -- the current state and what comes next.
