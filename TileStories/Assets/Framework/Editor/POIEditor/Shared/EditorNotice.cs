using UnityEditor;

namespace TileStories.Editor
{
    // The ONE way the POI Editor tells the developer something short and informational
    // (a locked edit, an override that hides a change, "nothing to clear", a rejected
    // rename, config validation issues...). It queues the message and an editor-update
    // pump shows it as the standard native dialog (title, message, centred OK) -- the same
    // look as every Yes/No/Cancel EditorUtility.DisplayDialog in this window. Yes/No
    // decisions call DisplayDialog directly because they must return the choice.
    //
    // Two timings:
    //  - discrete event (a click that failed): quietSeconds = 0, shown on the next editor update;
    //  - gesture (a Scene-view drag or slider drag firing the same warning every event):
    //    GestureQuietSeconds, re-armed by every Queue call, so the dialog only appears once
    //    the developer stops -- a modal opened mid-drag would break the drag.
    //
    // Advisory notices that can repeat pass a dontShowAgainKey: the dialog then carries Unity's
    // own opt-out checkbox next to the single OK button (DisplayDialog's DialogOptOutDecisionType
    // overload), and Unity stores the choice per machine. Notices that explain WHY an edit was
    // refused (the verified lock) or report the result of the developer's own click deliberately
    // pass no key: hiding them would make the editor look broken. TileStories > Reset Hidden
    // Notices un-ticks every opt-out.
    //
    // DisplayDialog has no icon parameter (the triangle is fixed by Unity/the OS), so there
    // is deliberately no severity flag: it could not change anything.
    [InitializeOnLoad]
    internal static class EditorNotice
    {
        public const float GestureQuietSeconds = 0.5f;

        // Modal dialogs cannot be clicked by an automated test and would hang the run, so the
        // test assembly switches this off for the whole session (see TestDialogGuard).
        internal static bool ShowDialogs = true;

        private const DialogOptOutDecisionType OptOutScope = DialogOptOutDecisionType.ForThisMachine;

        private static bool _pending;
        private static string _title;
        private static string _message;
        private static string _dontShowAgainKey;
        private static double _dueAt;
        private static bool _showing;

        static EditorNotice()
        {
            EditorApplication.update -= Pump;
            EditorApplication.update += Pump;
        }

        // Queue (or replace) the pending notice. Empty messages and notices the developer has
        // chosen to hide (their dontShowAgainKey is hidden) are ignored.
        public static void Queue(string title, string message, float quietSeconds = 0f, string dontShowAgainKey = null)
        {
            if (string.IsNullOrEmpty(message))
                return;
            if (dontShowAgainKey != null && IsHidden(dontShowAgainKey))
                return;

            _pending = true;
            _dontShowAgainKey = dontShowAgainKey;
            _title = string.IsNullOrEmpty(title) ? "Notice" : title;
            _message = message;
            _dueAt = EditorApplication.timeSinceStartup + quietSeconds;
        }

        public static bool HasPending => _pending;
        public static string PendingTitle => _pending ? _title : null;
        public static string PendingMessage => _pending ? _message : null;
        public static string PendingDontShowAgainKey => _pending ? _dontShowAgainKey : null;

        // Has the developer ticked "don't show again" for this notice? (Unity owns the storage.)
        public static bool IsHidden(string key) => EditorUtility.GetDialogOptOutDecision(OptOutScope, key);
        public static void Unhide(string key) => EditorUtility.SetDialogOptOutDecision(OptOutScope, key, false);

        // Bring back every notice the developer chose to hide.
        [MenuItem("TileStories/Reset Hidden Notices")]
        private static void ResetHiddenNotices()
        {
            foreach (string key in NoticeKeys.All)
                Unhide(key);
        }

        // Take the pending notice if its quiet period has elapsed.
        internal static bool TryTakeDue(double now, out string title, out string message, out string dontShowAgainKey)
        {
            title = null;
            message = null;
            dontShowAgainKey = null;
            if (!_pending || now < _dueAt)
                return false;

            title = _title;
            message = _message;
            dontShowAgainKey = _dontShowAgainKey;
            Clear();
            return true;
        }

        internal static void Clear()
        {
            _pending = false;
            _title = null;
            _message = null;
            _dontShowAgainKey = null;
        }

        // Runs on every editor update: show the due notice, once. Independent of any window,
        // so it still shows when the POI Editor tab is hidden behind another docked tab.
        private static void Pump()
        {
            if (!ShowDialogs || _showing)
                return;
            if (!TryTakeDue(EditorApplication.timeSinceStartup, out string title, out string message, out string key))
                return;

            _showing = true;
            try
            {
                if (key == null)
                    EditorUtility.DisplayDialog(title, message, "OK");
                else
                    EditorUtility.DisplayDialog(title, message, "OK", OptOutScope, key);
            }
            finally { _showing = false; }
        }
    }

    // The advisory notices that offer the opt-out checkbox. One key per kind of notice, so the
    // reset menu can bring every one of them back.
    internal static class NoticeKeys
    {
        // All Edit-Mode-preview "this facing change is invisible" warnings (slider and gizmo).
        public const string FacingPreviewWarning = "facing-preview-warning";
        public const string ConfigValidation = "config-validation";

        public static readonly string[] All = { FacingPreviewWarning, ConfigValidation };
    }
}
