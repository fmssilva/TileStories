## PLAN FOR BLOCK 1 - POIAuthoring Window Tab Width Optimization

### 1. TARGET DOMAIN AND CONTEXT RE-GROUNDING

__File to Modify__: `Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.cs`

- The OnGUI method (lines 235-266) contains the current tab toolbar implementation
- Currently uses hardcoded 16f spacers and fixed-height toolbar

### 2. DETAILED ANALYSIS OF CURRENT STRUCTURE

Current toolbar (lines 240-248):

```csharp
GUILayout.Label(" ", GUILayout.Width(16f)); // Left spacer
string[] tabNames = { "Global Scene", "Specific Marker" };
int selectedIndex = (int)_selectedTab;
int newSelectedIndex = GUILayout.Toolbar(selectedIndex, tabNames, GUILayout.Height(26f));
GUILayout.Label(" ", GUILayout.Width(16f)); // Right spacer
```

### 3. ARCHITECTURE LEVEL OPTIONS EVALUATION

__Option A (SELECTED)__: Use `ToolbarButtonSize.FitToContents` and remove spacers

- __Where__: Same file, same lines

- __How__:

  - Remove left/right spacers (lines 240, 248)
  - Add `GUILayout.Toolbar(selectedIndex, tabNames, GUILayout.Height(26f), GUILayout.ButtonStyle(GUI.skin.button))` with `FitToContents` via style or direct parameter
  - Actually, looking more carefully - the `buttonSize` parameter goes after `style`, before `params GUILayoutOption[] options`
  - So: `GUILayout.Toolbar(selectedIndex, tabNames, GUI.skin.button, ToolbarButtonSize.FitToContents, GUILayout.Height(26f))`

- __Pros__:

  - Buttons auto-size to text content (minimal width)
  - Toolbar left-aligned (starts at window edge)
  - Always visible even when window is narrow
  - Prepares for future additional tabs
  - No hardcoded widths

- __Cons__: None significant for this scope

### 4. IMPLEMENTATION LEVEL OPTIONS EVALUATION

__Selected__: Direct modification of the existing toolbar call

- __How__: Replace the current GUILayout.Toolbar call with one that includes `ToolbarButtonSize.FitToContents`
- __Pros__: Minimal change, leverages existing Unity functionality
- __Cons__: None

### 5. DETAILED IMPLEMENTATION PLAN

#### WHAT: Acceptance Criteria

- Tab buttons sized exactly to fit their text content (Global Scene, Specific Marker)
- Tab toolbar left-aligned (starts at window left edge)
- Light greenish/gray background preserved (0.9f, 1.0f, 0.9f)
- Vertical spacing preserved (10f)
- Tab switching functionality preserved (no _hasUnsavedChanges false positives)
- All existing functionality preserved (buttons, undo/redo, config load/save, rig handling)

#### HOW: Execution Plan

__PHASE 1: PREPARATION & SETUP__

- Verify current mode is PLAN_MODE (confirmed via __mode.md check)
- Read and understand current OnGUI structure (lines 219-266)
- Confirm exact lines for toolbar modification (240-248)

__PHASE 2: CORE IMPLEMENTATION__

- Remove left spacer (line 240)
- Remove right spacer (line 248)
- Modify GUILayout.Toolbar call to use ToolbarButtonSize.FitToContents
- Preserve light greenish/gray background (lines 237-238, 250)
- Preserve vertical spacing (line 252)

__PHASE 3: VALIDATION & VERIFICATION__

- Verify code compiles with zero error CS (refresh_unity)

- Run EditMode tests (zero failures)

- Run PlayMode tests (zero failures)

- Manual verification: Open POI Authoring Tool and confirm:

  - Tab buttons are minimal width (just text)
  - Toolbar is left-aligned
  - Background color preserved
  - Tab switching works
  - No false unsaved changes warnings

### 6. EXECUTION PLAN: ITEMIZED TODO LIST

__PHASE 1: PREPARATION & SETUP__

- Verify current mode is PLAN_MODE (confirmed via __mode.md check)
- Read and understand current OnGUI structure (lines 219-266)
- Confirm exact lines for toolbar modification (240-248)

__PHASE 2: CORE IMPLEMENTATION__

- Remove left spacer (line 240)
- Remove right spacer (line 248)
- Modify GUILayout.Toolbar call to use ToolbarButtonSize.FitToContents
- Preserve light greenish/gray background (lines 237-238, 250)
- Preserve vertical spacing (line 252)

__PHASE 3: VALIDATION & VERIFICATION__

- Verify code compiles with zero error CS (refresh_unity)
- Run EditMode tests (zero failures)
- Run PlayMode tests (zero failures)
- Manual verification: Open POI Authoring Tool and confirm tab optimization

### 7. TEST STRATEGY

__Tier 0: Compilation Check__

- `refresh_unity` to force compile
- Verify zero `error CS` lines in console

__Tier 1: Existing Test Regression__

- Run EditMode tests via Unity MCP `run_tests`
- Run PlayMode tests via Unity MCP `run_tests`
- Acceptance: zero failed tests in both suites

__Tier 2: Manual Verification (no automated test for EditorWindow UI)__

- Open POI Authoring Tool (TileStories/POI Authoring Tool, shortcut Shift+P)
- Confirm tab buttons are minimal width (just text width)
- Confirm toolbar is left-aligned (starts at window edge)
- Confirm tabs have light greenish/gray background
- Click "Specific Marker" tab, confirm content switches
- Switch back to "Global Scene", confirm content switches back
- Confirm no false "unsaved changes" warning when switching tabs
- Confirm all config load/save/rig buttons still work

### 8. COMPLETE PLAN SUMMARY

This plan implements user decisions:

- Tab buttons sized to fit text content (minimal width)
- Tab toolbar left-aligned (starts at window edge)
- Tab Styling: Light greenish/gray background (0.9f, 1.0f, 0.9f) preserved
- Initial State: Opens on Global Scene tab (_selectedTab = TabSelection.GlobalScene)
- Extension: Designed for easy accommodation of additional tabs via enum + switch (now works better with narrow windows)
- Bug Fix: Removed false _hasUnsavedChanges = true on tab switch (already implemented)
- Width Optimization: Uses ToolbarButtonSize.FitToContents for automatic text-based sizing
- Alignment: Toolbar left-aligned by removing spacers and using FitToContents
