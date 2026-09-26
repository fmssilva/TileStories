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
            ConfirmOrStopBuild(findings, Application.isBatchMode);
        }

        // "Build anyway" lets the build go on; Cancel (or Esc / closing the dialog) stops it with a
        // BuildFailedException. Batch mode cannot ask: it only warns. No findings = nothing asked.
        internal static void ConfirmOrStopBuild(IReadOnlyList<string> findings, bool batchMode)
        {
            if (findings == null || findings.Count == 0) return;

            string text = string.Join("\n\n", findings);
            if (batchMode)
            {
                Debug.LogWarning("[DevFeatureBuildCheck] " + text.Replace("\n\n", " | "));
                return;
            }

            bool buildAnyway = EditorDecision.Ask(
                "Developer-only switch is ON",
                text + "\n\nBuild anyway, or Cancel and switch it off first?",
                "Build anyway") == DecisionAnswer.Confirm;
            if (!buildAnyway)
                throw new BuildFailedException("[DevFeatureBuildCheck] Build cancelled: a developer-only switch is ON in the config. " + text.Replace("\n\n", " | "));
        }
    }
}
