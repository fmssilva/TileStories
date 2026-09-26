using UnityEditor;

namespace TileStories.Editor
{
    // The ONE way the POI Editor tells the developer something short and informational
    // (a locked edit, an override that hides a change, "nothing to clear", a rejected
    // rename, config validation issues...). It queues the message and an editor-update
    // pump shows it in the shared, NON-blocking EditorPopup window (NoticePopup): the
    // developer reads it, drags it aside or closes it with X / Esc, and nothing waits for
    // it -- so a notice can never freeze the Editor, Unity MCP or a test run. A question the
    // caller must wait for is a different kind: EditorDecision.
    //
    // Two timings:
    //  - discrete event (a click that failed): quietSeconds = 0, shown on the next editor update;
    //  - gesture (a Scene-view drag or slider drag firing the same warning every event):
    //    GestureQuietSeconds, re-armed by every Queue call, so the popup only appears once
    //    the developer stops -- one popup at the end, not one per mouse event.
    //
    // Advisory notices that can repeat pass a dontShowAgainKey: the popup then carries a
    // "Don't show this again" toggle, stored in Unity's own dialog opt-out storage (per machine).
    // Notices that explain WHY an edit was refused (the verified lock) or report the result of
    // the developer's own click deliberately pass no key: hiding them would make the editor look
    // broken. TileStories > Reset Hidden Messages un-ticks every opt-out, EditorDecision's included.
    [InitializeOnLoad]
    internal static class EditorNotice
    {
        public const float GestureQuietSeconds = 0.5f;

        // A test run switches the popups off (TestDialogGuard) so notices queued by the code under
        // test do not litter the screen; the queue itself stays observable. Not a hang guard any more.
        internal static bool ShowPopups = true;

        internal const DialogOptOutDecisionType OptOutScope = DialogOptOutDecisionType.ForThisMachine;

        private static bool _pending;
        private static string _title;
        private static string _message;
        private static string _dontShowAgainKey;
        private static double _dueAt;

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
        public static void Hide(string key) => EditorUtility.SetDialogOptOutDecision(OptOutScope, key, true);

        // Bring back every notice and question the developer chose to hide.
        [MenuItem("TileStories/Reset Hidden Messages")]
        private static void ResetHiddenMessages()
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
            if (!ShowPopups)
                return;
            if (TryTakeDue(EditorApplication.timeSinceStartup, out string title, out string message, out string key))
                EditorPopup.Show(new NoticePopup(title, message, key));
        }
    }

    // Every notice and question that offers "don't show again". One key per kind, so the reset
    // menu can bring every one of them back.
    internal static class NoticeKeys
    {
        // All Edit-Mode-preview "this facing change is invisible" warnings (slider and gizmo).
        public const string FacingPreviewWarning = "facing-preview-warning";
        public const string ConfigValidation = "config-validation";
        // EditorDecision: "unlock a verified POI's position and facing?"
        public const string UnverifyPosition = "unverify-position";

        public static readonly string[] All = { FacingPreviewWarning, ConfigValidation, UnverifyPosition };
    }
}
