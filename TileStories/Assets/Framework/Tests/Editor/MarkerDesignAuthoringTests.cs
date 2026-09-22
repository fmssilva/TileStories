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
    }
}
