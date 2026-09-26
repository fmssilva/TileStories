using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UI;

namespace TileStories.Editor.Tests
{
    // Marker design domain (_2.2.1 marker, _2.2.2 badge, _2.2.3 outline), authoring side: what the
    // POI Editor window offers must be exactly what the runtime understands. Real code only: the real
    // window constants and the real parser.
    public class MarkerDesignAuthoringTests
    {
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        // Regression for a real bug found via OrientationWallSessionIntegrationTests: the Ring and
        // Badge Images defaulted to raycastTarget=true, and their bounding RECTANGLE (not their
        // actual hollow/offset sprite) is what GraphicRaycaster tests, so a decorative ring's square
        // bounding box could out-rank the Symbol at the marker's own centre. Selection still worked
        // (MarkerSelectable is on the root and the click event bubbles up regardless of which child
        // graphic was hit), but the Symbol is the intended Tier-0.5 occlusion anchor (40-testing.md),
        // so decorative layers must not compete with it for raycasts.
        [Test]
        public void RingAndBadgeImages_AreNotRaycastTargets()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(MarkerPrefabPath);
            Assert.IsNotNull(prefab, $"Could not load POI_Marker prefab at {MarkerPrefabPath}");

            var ring = prefab.transform.Find("Ring")?.GetComponent<Image>();
            var badge = prefab.transform.Find("Badge")?.GetComponent<Image>();
            Assert.IsNotNull(ring, "Ring Image missing from POI_Marker.prefab");
            Assert.IsNotNull(badge, "Badge Image missing from POI_Marker.prefab");

            Assert.IsFalse(ring.raycastTarget, "Ring must not be a raycast target: its bounding box overlaps the Symbol's centre.");
            Assert.IsFalse(badge.raycastTarget, "Badge must not be a raycast target: its bounding box can overlap the Symbol.");
        }

        private static string[] EditorOptions(string fieldName)
        {
            var value = (string[])typeof(POIEditorToolWindow).GetField(fieldName, Static)?.GetValue(null);
            Assert.IsNotNull(value, fieldName + " must exist on the editor window");
            return value;
        }

        [Test]
        public void EveryOutlineModeTheEditorOffers_IsUnderstoodByTheRuntimeParser()
        {
            string[] options = EditorOptions("OutlineModeOptions");
            Assert.GreaterOrEqual(options.Length, 2, "Precondition: the popup offers real choices.");
            foreach (string option in options)
                Assert.IsTrue(MarkerVisualsParser.TryParseOutlineMode(option, out _),
                    $"Outline Color option '{option}' is saved to config.json but the runtime parser rejects it, so a wall using it renders NO outline.");
        }

        [Test]
        public void EveryShapeTheEditorOffers_IsUnderstoodByTheRuntimeParser()
        {
            string[] options = EditorOptions("ShapeOptions");
            Assert.AreEqual(6, options.Length, "Precondition: circle, rounded square, hexagon, diamond, star, none.");
            foreach (string option in options)
                Assert.IsTrue(MarkerVisualsParser.TryParseShape(option, out _), $"Shape option '{option}' must parse.");
        }

        [Test]
        public void MarkerBadgeOutlineTestGuides_DoNotHardcodeAWallsRealPoiIdsOrCategories()
        {
            // Guide text must describe framework behaviour, not one specific wall's taxonomy
            // (_5.1_Editor_Tab.md, "Domain Manual Tests" > "Guide content must stay app-agnostic").
            // These are real LivingRoom POI ids/categories/badge keys that must never be assumed
            // to exist as framework fixtures. "LivingRoom" itself is NOT forbidden: the real
            // scene-path references (Apps/LivingRoom/LivingRoomScene) are legitimate and kept.
            var t = typeof(POIEditorToolWindow);
            var forbidden = new[]
            {
                "lamp_military", "lamp_economic", "lamp_religious", "lamp_infrastructure", "lamp_residential",
                "painting_religious", "camera_religious", "painting_military", "camera_military",
                "painting_economic", "camera_economic",
                "dev_marker_custom_symbol", "dev_marker_nolevel", "dev_marker_nobadge", "dev_marker_nostatus",
                "'lamp'", "'religious'", "'military'", "partial_damage",
            };

            foreach (string guideName in new[]
            {
                "MarkerSceneTestGuide", "MarkerPlaymodeTestGuide", "MarkerDeviceTestGuide",
                "BadgeSceneTestGuide", "BadgePlaymodeTestGuide", "BadgeDeviceTestGuide",
                "OutlineSceneTestGuide", "OutlinePlaymodeTestGuide", "OutlineDeviceTestGuide",
            })
            {
                var field = t.GetField(guideName, Static);
                Assert.IsNotNull(field, guideName + " must exist");
                var guide = (string)field.GetValue(null);
                Assert.IsFalse(string.IsNullOrWhiteSpace(guide), guideName + " must exist and not be empty");
                foreach (string term in forbidden)
                    StringAssert.DoesNotContain(term, guide, guideName + " must not hardcode the real wall-specific id/category/key '" + term + "'");
            }
        }

        // Labels, Text & Fonts domain (_2.0_Labels_And_Fonts_Design.md, 2026-09-22): same
        // guide-contract shape as the test above, kept separate since it is a distinct domain.
        [Test]
        public void LabelsAndFontsTestGuides_ExistAndDoNotHardcodeAWallsRealPoiIdsOrCategories()
        {
            var t = typeof(POIEditorToolWindow);
            var forbidden = new[]
            {
                "lamp_military", "lamp_economic", "lamp_religious", "lamp_infrastructure", "lamp_residential",
                "painting_religious", "camera_religious", "painting_military", "camera_military",
                "painting_economic", "camera_economic",
                "dev_marker_custom_symbol", "dev_marker_nolevel", "dev_marker_nobadge", "dev_marker_nostatus",
                "'lamp'", "'religious'", "'military'", "partial_damage",
            };

            foreach (string guideName in new[]
            {
                "LabelsAndFontsSceneTestGuide", "LabelsAndFontsPlaymodeTestGuide", "LabelsAndFontsDeviceTestGuide",
            })
            {
                var field = t.GetField(guideName, Static);
                Assert.IsNotNull(field, guideName + " must exist");
                var guide = (string)field.GetValue(null);
                Assert.IsFalse(string.IsNullOrWhiteSpace(guide), guideName + " must exist and not be empty");
                foreach (string term in forbidden)
                    StringAssert.DoesNotContain(term, guide, guideName + " must not hardcode the real wall-specific id/category/key '" + term + "'");
            }
        }
    }
}
