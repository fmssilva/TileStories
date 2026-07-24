using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace TileStories.Editor
{
    // Blocks the build outright if POIAuthoringRig still has child objects.
    // These are Edit-Mode authoring stand-ins (see the Stage 1.2 plan's
    // Mechanism 1 vs Mechanism 2 distinction) and must never ship inside a
    // built app. Unlike POIAuthoringRigSafetyCheck's save/Play-mode warning
    // (non-blocking, since those happen constantly during normal iteration),
    // this is a hard failure, because a build is visitor-facing.
    public class POIAuthoringRigBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var rig = GameObject.Find("POIAuthoringRig");

            if (rig != null && rig.transform.childCount > 0)
            {
                throw new BuildFailedException(
                    $"[POIAuthoringRigBuildCheck] Build blocked: POIAuthoringRig " +
                    $"still has {rig.transform.childCount} child object(s) in the " +
                    $"active scene. These are Edit-Mode authoring stand-ins and " +
                    $"must never ship in a build.\n\n" +
                    $"Fix: open the POI Authoring Tool, run 'Capture Positions to " +
                    $"JSON' if you haven't already for these markers, then click " +
                    $"'Clear Rig' to remove them. Then build again.");
            }
        }
    }
}