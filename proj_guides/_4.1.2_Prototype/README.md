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

Live: https://claude.ai/artifact/X7CadwbEKM9eSaU41Z1ASg · record copy: `Region_Editor_Lab.html`. It supersedes P1.

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
