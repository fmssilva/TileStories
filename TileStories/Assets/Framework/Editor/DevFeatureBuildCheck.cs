using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TileStories.Editor
{
    // Before a build: reads every wall's StreamingAssets config (the copy the app will ship with) and,
    // if a developer-only switch is ON and would take effect in this build, asks the developer to
    // confirm. Cancel (or Esc / closing the dialog) fails the build; nothing is changed in any file.
    public class DevFeatureBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        // Check each wall config and confirm with the developer when a dev-only switch is ON
        public void OnPreprocessBuild(BuildReport report)
        {
            bool development = (report.summary.options & BuildOptions.Development) != 0;
            var findings = new List<string>();

            foreach (var configPath in Directory.GetFiles(Application.dataPath + "/StreamingAssets", "config.json", SearchOption.AllDirectories))
            {
                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(configPath));
                foreach (var message in DevFeatureBuildGuard.ActiveMessages(config, development))
                    findings.Add(Path.GetFileName(Path.GetDirectoryName(configPath)) + ": " + message);
            }
            if (findings.Count == 0) return;

            string text = string.Join("\n\n", findings);
            if (Application.isBatchMode)
            {
                Debug.LogWarning("[DevFeatureBuildCheck] " + text.Replace("\n\n", " | "));
                return;
            }

            // DisplayDialog: ok = first button; Esc / closing the dialog = false = cancel
            bool buildAnyway = EditorUtility.DisplayDialog(
                "Developer-only switch is ON",
                text + "\n\nBuild anyway?",
                "Build anyway", "Cancel build");
            if (!buildAnyway)
                throw new BuildFailedException("[DevFeatureBuildCheck] Build cancelled: a developer-only switch is ON in the config. " + text.Replace("\n\n", " | "));
        }
    }
}
