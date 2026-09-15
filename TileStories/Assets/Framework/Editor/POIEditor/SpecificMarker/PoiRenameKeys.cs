using UnityEngine;

namespace TileStories.Editor
{
    // Pure decision table for the POI header rename field's commit/cancel keys.
    // IMGUI event flow itself is not headless-testable, so the key -> action
    // mapping lives here and is Tier-0 tested (PoiRenameKeysTests). The window
    // resolves the action BEFORE drawing the TextField: Unity's TextField
    // consumes the first Return it sees (commit + keyboard-focus release),
    // which silently turned the event into EventType.Used for any check placed
    // after the field -- the root cause of the "press Enter twice" bug.
    public static class PoiRenameKeys
    {
        public enum Action { None, Commit, Cancel }

        // Map a raw key event to its rename action. Only KeyDown acts: KeyUp,
        // Used (already consumed by a control), Layout/Repaint passes, and all
        // non-key events are None. Enter/KeypadEnter commit; Escape cancels.
        public static Action Resolve(EventType type, KeyCode key)
        {
            if (type != EventType.KeyDown)
                return Action.None;
            if (key == KeyCode.Return || key == KeyCode.KeypadEnter)
                return Action.Commit;
            if (key == KeyCode.Escape)
                return Action.Cancel;
            return Action.None;
        }
    }
}