// IdentityDeleteGuard.cs
//
// Editor-only guard for DELETING a row from a taxonomy table whose identity is
// referenced by POIs. This is the delete half of the same problem
// IdentityRenameEditState handles for rename: a rename can propagate to the
// referencing POIs, but a delete cannot propagate anything, so the row simply
// disappears out from under the POIs that still name it. Those POIs then
// silently fall back to the framework default (hash colour, no badge icon,
// 12cm marker) with no error anywhere.
//
// The counting rule itself is pure and lives in
// IdentityRenameResolver.CountReferences; this file owns only the question, so
// the rule stays Tier-0 testable with `new` and no EditorWindow running.
namespace TileStories.Editor
{
    internal static class IdentityDeleteGuard
    {
        // True when removing the row is safe: either nothing still points at the
        // identity, or the developer confirmed after being told exactly how many
        // POIs would be orphaned. Zero references means no prompt at all, so
        // deleting an unused row stays a single click.
        public static bool Confirm(string rowKind, string identity, int referenceCount)
        {
            if (referenceCount <= 0)
                return true;

            return EditorDecision.Ask(
                $"{rowKind} still in use",
                $"{referenceCount} POI(s) still reference '{identity}'.\n\n" +
                "Deleting this row leaves them naming a value that no longer exists, so " +
                "their marker silently falls back to the framework default. Reassign those " +
                "POIs first, or confirm to delete anyway.",
                "Delete anyway") == DecisionAnswer.Confirm;
        }
    }
}
