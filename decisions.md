# Design Decisions Log

Append on every real design decision made during implementation. Never
reconstructed from memory.

---

## 2026-08-06: Rig safety prompt -- dialog vs. hard block

**Decision**: Replace the hard `BuildFailedException` on build and the
non-blocking warning on Play Mode with a single interactive
`EditorUtility.DisplayDialogComplex` dialog that offers "Save, Clear &
Continue", "Continue Without Clearing" (Play Mode only), and "Cancel".

**Reason**: A hard build failure gives the developer no path forward without
manually opening the tool, saving, and clearing. An interactive dialog
surfaces the problem at the exact moment the developer is about to enter
Play Mode or build, and offers the save+clear action inline. The "Continue
Without Clearing" option for Play Mode respects that a developer may
intentionally want to test with the rig present (e.g. comparing rig markers
against runtime markers).

**Trade-off**: Builds always require clearing (no "Continue Without
Clearing" option) because a build is visitor-facing and duplicate markers
in a shipped build are unacceptable.

---

## 2026-08-06: "Don't show again" as menu toggle, not dialog checkbox

**Decision**: Implement the opt-out as a menu item toggle
(`TileStories/POI Authoring/Rig Safety Prompt on Play/Build`) backed by
`EditorPrefs`, rather than a checkbox inside the dialog.

**Reason**: `EditorUtility.DisplayDialogComplex` does not support checkboxes
or custom controls -- only button labels. A menu toggle is the standard
Unity pattern for "don't show this again" preferences and provides visual
feedback via `Menu.SetChecked`.

---

## 2026-08-06: Play-mode hook stays in RigSafetyCheck, not the window class

**Decision**: Keep the `EditorApplication.playModeStateChanged` subscription
in `POIAuthoringRigSafetyCheck.cs` (the `[InitializeOnLoad]` static class),
not in `POIAuthoringToolWindow.cs`.

**Reason**: `[InitializeOnLoad]` is required for the static constructor to
run on editor reload. The safety-check class already has this attribute and
is the natural home for the hook. The window class stays focused on
authoring logic; the dialog method is `internal static` so the safety-check
class can call it without a circular dependency.

---

## 2026-08-06: Static rig child count via GameObject.Find

**Decision**: `GetRigChildCountStatic()` uses `GameObject.Find("POIAuthoringRig")`
rather than `GetWindow<POIAuthoringToolWindow>()` to count rig children.

**Reason**: The tool window may not be open when the user clicks Play or
Build. `GetWindow` would create an unwanted window instance.
`GameObject.Find` works regardless of window state. The method also
verifies the rig's parent is `PlacementCorrectionAnchor` to avoid false
positives from unrelated objects named `POIAuthoringRig`.

---

## 2026-08-06: Scene saving stays non-blocking

**Decision**: `OnSceneSaving` keeps its non-blocking `Debug.LogWarning`
behavior rather than routing through the dialog.

**Reason**: A developer mid-placement should still be able to save the scene
without being blocked by a dialog. Scene saving is not a runtime/build
concern -- it's an editor workflow step. Only Play Mode entry and build
initiation are blocked, because those are the points where duplicate
markers would actually appear.
