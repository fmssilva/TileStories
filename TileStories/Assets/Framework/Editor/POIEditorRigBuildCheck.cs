using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace TileStories.Editor
{
    // Intercepts the build pipeline and, if POIEditorRig still has child
    // objects, shows the interactive safety dialog (Save, Clear & Build / Cancel)
    // instead of hard-failing. Only throws BuildFailedException if the user
    // explicitly cancels or the dialog cannot proceed. Builds always require a
    // clean rig — there is no "continue without clearing" option for builds
    // because a build is visitor-facing.
    public class POIEditorRigBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Delegate to the tool window's dialog. For builds there is no
            // "Continue Without Clearing" option — the dialog only offers
            // Save/Clear/Build or Cancel.
            if (!POIEditorToolWindow.PromptBeforePlayOrBuild(isBuild: true))
            {
                // User cancelled the build, or opted out of prompts but the
                // rig still has children (shouldn't happen — PromptBeforePlayOrBuild
                // returns true when SkipPromptPrefKey is set. If it returns false,
                // the user clicked Cancel).
                var rig = GameObject.Find("POIEditorRig");
                int count = rig != null ? rig.transform.childCount : 0;
                throw new BuildFailedException(
                    $"[POIEditorRigBuildCheck] Build blocked by user: POIEditorRig " +
                    $"still has {count} child object(s). Open the POI Editor, " +
                    $"run 'Save All to JSON', then click 'Clear Rig' before building.");
            }
        }
    }
}
