# _4.1.2 Prototype -- World Design studies (2026-10-29)

Design studies the `_4.1` plan is judged on. They are the **reference for look, timing and precision**, not code to port: each
effect names the Unity mechanism it maps to (`_4.1.1_World_Design_Research.md` section 4).

## Study 1 -- Wall Effects (`Study1_Wall_Effects.html`)
- **Live version:** https://claude.ai/artifact/NjRLSTLwgv5hWf5Tm9pgAm (private to the developer). The file here is its record;
  it runs on its own in a browser (the picture is embedded).
- **Stand-in wall:** a phone photo of the LivingRoom painting (388 x 447 px). Blue and golden strokes are found by colour inside
  the frame and play buildings ("Cobalt Tower", "Golden Palace", "Navy Gate"); small strokes play windows. The same painting is in
  the current Immersal map's textured GLB (texture 0, about 850 x 1350 px of a 4096 atlas), so the Unity work can use it too.
- **Effects:** Selection moment (spotlight + outline draw-on + name with a leader line), Spotlight, Outline draw-on, Breathing
  glow, Light sweep, Tile reveal (wave from the tap), Windows light up, Lock-on (wall edges + corner marks), Theme glow.
- **Tweaks:** tempo, alignment error (cm, assuming a 1 m wide painting), dim strength, soft edge, line width, light colour
  (gold / warm white / cobalt), presence (marker / region / both), reduce motion.
- **What to judge:** which effects feel premium; the timings; which survive 2-4 cm of error (the precision chip per effect).

---

## P1 Region Geometry (2026-10-29)

Live: https://claude.ai/artifact/LxjMpnDXQAj1sZ753aHGo5 · record copy: `P1_Region_Geometry.html`.
First prototype of the series in `_4.1.3_World_Editor_Spec.md` section 9. It replaces Study 1's colour detection: the author
draws the shape. On the painting photo, you can:
- draw a polygon, a path or a rectangle (the rectangle becomes a 4-knot polygon);
- drag knots, drag a whole shape, and insert a knot with the + handle;
- set each knot's corner to sharp, rounded (with a radius in cm) or smooth;
- keep several shapes per POI, with undo and redo and a visitor view.

The right-hand panel copies the planned Specific Marker > Region section, and the POI's `world.shapes` JSON updates live. An
"In Unity" line names the Splines equivalent of each action.

Judge: is drawing and reshaping quick enough? Are the three corner types enough? Is the panel organisation right?

---

## Region Editor Lab (P1 + P2 + P3 merged, 2026-10-29)

Record copy: `Region_Editor_Lab.html` (v1). It supersedes P1 and is superseded by the World Editor Lab below.

The developer asked for one prototype that looks like the real POI Editor. Geometry (per POI) and style (per wall) are two
sides of one thing, and separate prototypes hid how they connect.

Left, the Scene / Game view:
- Scene tools: Edit, Polygon, Path, Points, Rectangle.
- ▶ Play shows the reveal (after each level's delay), the idle motion and a tap. A tap runs the focus moment and opens a
  card-sheet peek.

Right, a Unity-skinned POI Editor:
- **Global Scene › Region Styles (NEW):**
  - A styles table with name, preview, precision chip and use count, plus duplicate and delete. Delete is blocked while a
    style is in use.
  - For the selected style, foldouts for Line / Vertices / Fill / Motion (Intro, Idle, Focus).
  - A Test sub-foldout with alignment error and reduce motion.
- **Global Scene › Hierarchy Levels (EXTENDED):** the Presence and Region style columns.
- **Specific Marker › POI › Region (NEW):**
  - Presence and style overrides (the style choice only).
  - A Shapes table with name, type, a per-shape style, knot count, edit and delete.
  - Buttons to add a polygon, path, points or rectangle.
  - A selected-shape foldout with its default corner and the selected knot's corner and radius.
- A live config.json preview in each tab.

Sample: Golden Palace has 4 shapes (an outline in Facade, a door in Detail, windows as Points in Windows, a cornice path in
Detail). Cobalt Tower has a smooth outline. Navy Gate is a point plus a dashed "street" path.

---

## World Editor Lab (v2, 2026-11-12) -- the one Lab from now on

Live: https://claude.ai/artifact/MDZ3DZzjDscHEU6dw22Qp6 (same link as the Region Editor Lab, renamed) · record copy:
`World_Editor_Lab.html`. It supersedes the Region Editor Lab and absorbs the planned Wall Life Lab (`_4.1.4` section 7).
It keeps growing: every new World feature lands here first. The specs (`_4.1.3`, `_4.1.4`) are the source of truth; this
README says where each feature is in the Lab.

Layout (developer's rule): the page is full height, with no title or text above or below the view. The view has screen tabs,
▶ / ↻, Undo / Redo and a ⌨ button for the shortcuts. The editor scrolls on its own.

Screens (left):
- **Wall** -- the wall as the visitor sees it (▶) and the Scene view for drawing shapes (Specific Marker).
- **Style** -- only the shapes that use the selected Region Style, looping intro → idle → focus → back.
- **Precision** -- the selected style at 0, 2, 4 and 6 cm of tracking error, the true contour dashed.
- **Wall Life** -- the six kinds on the painting. Scene: each row's anchor, dashed, with its K code (click one to edit it).
  ▶: rows play on their triggers; tap a POI for focus rows.

**Editor → screen:** a click or focus anywhere in the editor opens the screen that shows it (Region Styles → Style; the
Precision row, Offset and Test → Precision; Hierarchy Levels and Region → Wall; Wall Life and World Content → Wall Life).

Editor (right):
- **Global Scene › Region Styles** -- as before, plus the section 11 proposals (precision as "hides ≈X cm" with a warning,
  one "Offset outward", Gradient and Pattern fills; a new "Glow wash" style shows the gradient).
- **Global Scene › Wall Life (NEW)** -- wall-level rows (birds, mascot) and the Budget (loops per POI, ambient gap, mascot
  interval, reduce-motion state).
- **Specific Marker › POI › World Content (NEW)** -- rows created from a kind template: kind, anchor shape, trigger,
  kind settings, reduce motion, prefab path; Export wall canvas / Import wall art (Unity only).
- **Region › Shapes** gains the style **Hidden (anchor only)**: a window that anchors a curtain or shutters without an outline.
- **Decisions (Lab only)** -- the open decisions with their feature, options and the recommendation; decisions 1-3 switch
  the Lab live (Current / Proposed). "Copy answers" gives a text to paste in the chat.

Sample Wall Life: Golden Palace = K1 night repaint (reveal) + K2 curtain in Window 3 (loop); Cobalt Tower = K3 shutters
(focus); Navy Gate = K4 tram on the street (every); wall = K5 birds (every) + K6 mascot peek (every, never during a card).
Lab timings are shorter than real ones.
