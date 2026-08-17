using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NUnit.Framework;
using TileStories;

namespace TileStories.Tests
{
    // Verifies the shared UI Toolkit PanelSettings contract (1c deliverable): one
    // shared panel settings for ALL screen-space UI Toolkit screens, scaled to
    // 390x844 reference (iPhone 14 Pro base) per 10-structure Shared/ spec.
    public class PanelSettingsTests
    {
        private const string AssetPath = "Assets/Framework/Runtime/UI/Shared/PanelSettings.asset";

        [Test]
        public void Shared_PanelSettings_uses_scale_with_screen_size_and_390x844_reference()
        {
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(AssetPath);
            Assert.IsNotNull(ps, "Shared PanelSettings.asset missing at Runtime/UI/Shared/PanelSettings.asset");
            Assert.AreEqual("ScaleWithScreenSize", ps.scaleMode.ToString());
            Assert.AreEqual(new Vector2Int(390, 844), ps.referenceResolution);
        }
    }
}
