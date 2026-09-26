using System;
using UnityEditor;

namespace TileStories.Editor
{
    // What the developer answered. Cancel is also what Esc, the dialog's X and anything unexpected mean.
    internal enum DecisionAnswer
    {
        Confirm,
        Alternative,
        Cancel,
    }

    // One question, as the developer sees it: the buttons are [Confirm] [Cancel] or
    // [Confirm] [Cancel] [Alternative]. Cancel is always there, always labelled "Cancel".
    internal sealed class DecisionRequest
    {
        public string Title;
        public string Message;
        public string ConfirmLabel;
        public string AlternativeLabel;   // null = a two-button question
        public string DontAskAgainKey;    // null = always asked
    }

    // The ONE way the POI Editor (and its Play / Build gates) asks a question the caller must wait
    // for: a destructive confirm, a reload guard, a gate before Play or a build. It is the only
    // BLOCKING popup kind -- a native modal dialog, because the Play Mode hook and a build
    // preprocessor must have the answer before they return. Everything that needs no answer goes
    // to EditorPopup instead ("Popups: the two kinds", _5.1).
    //
    // Why one wrapper: DisplayDialogComplex returns 1 for the Cancel button AND for Esc / closing the
    // dialog, so a destructive choice in slot 1 once let Esc discard unsaved work. Here the slot order
    // is fixed once (Resolve), so no caller can get it wrong again.
    //
    // Tests cannot click a modal dialog (it hangs the run). They set Responder instead: it receives
    // the exact request and "clicks" a button, so a test runs the REAL flow (Clear Rig, Delete POI...)
    // with no dialog on screen. TestDialogGuard clears it around every test.
    internal static class EditorDecision
    {
        public const string CancelLabel = "Cancel";

        internal static Func<DecisionRequest, DecisionAnswer> Responder;

        // Ask and wait. dontAskAgainKey adds Unity's own "do not show again" checkbox; a hidden
        // question answers Confirm without being shown (TileStories > Reset Hidden Messages brings it back).
        public static DecisionAnswer Ask(string title, string message, string confirmLabel,
            string alternativeLabel = null, string dontAskAgainKey = null)
        {
            return Ask(new DecisionRequest
            {
                Title = title,
                Message = message,
                ConfirmLabel = confirmLabel,
                AlternativeLabel = alternativeLabel,
                DontAskAgainKey = dontAskAgainKey,
            });
        }

        // Same, for a question built elsewhere (a pure builder that picks labels from the real state)
        public static DecisionAnswer Ask(DecisionRequest request)
        {
            if (request.DontAskAgainKey != null && EditorNotice.IsHidden(request.DontAskAgainKey))
                return DecisionAnswer.Confirm;

            bool hasAlternative = request.AlternativeLabel != null;
            // a scripted click obeys the same rule as a real one: no button, no answer
            if (Responder != null)
                return OnlyOfferedButtons(Responder(request), hasAlternative);

            if (!hasAlternative)
                return AskTwoButtons(request);

            int result = EditorUtility.DisplayDialogComplex(request.Title, request.Message,
                request.ConfirmLabel, CancelLabel, request.AlternativeLabel);
            return Resolve(result, hasAlternative: true);
        }

        private static DecisionAnswer AskTwoButtons(DecisionRequest request)
        {
            if (request.DontAskAgainKey == null)
                return EditorUtility.DisplayDialog(request.Title, request.Message, request.ConfirmLabel, CancelLabel)
                    ? DecisionAnswer.Confirm : DecisionAnswer.Cancel;

            bool confirmed = EditorUtility.DisplayDialog(request.Title, request.Message, request.ConfirmLabel, CancelLabel,
                EditorNotice.OptOutScope, request.DontAskAgainKey);
            // the box only sticks with a Confirm: "never ask me" must never mean "always cancel"
            if (!confirmed)
                EditorNotice.Unhide(request.DontAskAgainKey);
            return confirmed ? DecisionAnswer.Confirm : DecisionAnswer.Cancel;
        }

        // DisplayDialogComplex result -> answer. 0 = Confirm, 2 = Alternative (only when one was offered),
        // 1 (the Cancel button, Esc, the X) and anything unexpected = Cancel: the safe default.
        internal static DecisionAnswer Resolve(int dialogResult, bool hasAlternative)
        {
            if (dialogResult == 0) return DecisionAnswer.Confirm;
            if (dialogResult == 2) return OnlyOfferedButtons(DecisionAnswer.Alternative, hasAlternative);
            return DecisionAnswer.Cancel;
        }

        // An Alternative to a question that offered none is a Cancel, so no caller has to check it
        private static DecisionAnswer OnlyOfferedButtons(DecisionAnswer answer, bool hasAlternative) =>
            answer == DecisionAnswer.Alternative && !hasAlternative ? DecisionAnswer.Cancel : answer;
    }
}
