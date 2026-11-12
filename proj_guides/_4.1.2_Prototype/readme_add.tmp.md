
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
