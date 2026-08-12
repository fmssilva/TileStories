using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TileStories.Tests
{
    public class MarkerViewRuntimeTests
    {
        [UnityTest]
        public IEnumerator Initialise_MissingIconAndEffectWiring_RebuildsRequiredChildren()
        {
            var root = new GameObject("MarkerRoot");
            var anchor = root.AddComponent<POIAnchor>();
            anchor.Initialise(new POIData
            {
                id = "poi_1",
                name = "POI One",
                category = "religious",
                has_status = true,
                status_pct = 85f,
            });

            var symbol = CreateGlyphChild(root.transform, "Symbol");
            var badge = CreateGlyphChild(root.transform, "Badge");
            var ring = CreateRingChild(root.transform, "Ring");

            var markerView = root.AddComponent<MarkerView>();
            SetPrivateField(markerView, "symbol", symbol);
            SetPrivateField(markerView, "badge", badge);
            SetPrivateField(markerView, "ring", ring);

            markerView.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle);
            yield return null;

            Assert.IsNotNull(symbol.transform.Find("Icon"));
            Assert.IsNotNull(badge.transform.Find("Icon"));
            Assert.IsNotNull(root.transform.Find("Halo"));
            Assert.IsNotNull(root.transform.Find("SunInner"));
            Assert.IsNotNull(root.transform.Find("SunMiddle"));
            Assert.IsNotNull(root.transform.Find("SunOuter"));
            Assert.IsNotNull(root.GetComponent<MarkerPulseEffect>());
            Assert.IsNotNull(root.GetComponent<MarkerGlowEffect>());
            Assert.IsNotNull(root.GetComponentInChildren<MarkerSunEffect>(true));

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator SunEffect_ActivatesThreeRingChildren()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();

            var heroGo = Object.Instantiate(prefab);
            var heroAnchor = heroGo.AddComponent<POIAnchor>();
            heroAnchor.Initialise(new POIData
            {
                id = "sun",
                name = "Sun Label",
                category = "religious",
                has_status = false,
                status_unknown = false,
                has_captured_position = true,
            });

            var heroView = heroGo.GetComponentInChildren<MarkerView>();
            heroView.Initialise(heroAnchor, MarkerStyle.OutlineGold, MarkerShape.Circle, MarkerEffectFlags.SunContours);
            yield return null;

            Assert.IsTrue(GetPrivateBool(heroGo.GetComponent<MarkerSunEffect>(), "_active"),
                "Sun effect should be active for hero markers using sun mode.");
            Assert.IsFalse(GetPrivateBool(heroGo.GetComponent<MarkerPulseEffect>(), "_active"),
                "Pulse should be inactive when sun mode is selected.");
            Assert.AreEqual(MarkerSunEffect.SunVisualStyle.Contours, heroGo.GetComponent<MarkerSunEffect>().CurrentStyle,
                "MarkerEffectFlags.SunContours should map to the contour sun style.");

            Assert.IsNotNull(heroGo.transform.Find("SunInner")?.GetComponent<Image>());
            Assert.IsNotNull(heroGo.transform.Find("SunMiddle")?.GetComponent<Image>());
            Assert.IsNotNull(heroGo.transform.Find("SunOuter")?.GetComponent<Image>());

            Object.Destroy(heroGo);
        }

        [UnityTest]
        public IEnumerator SunEffect_FilledAndContourVariants_HaveDistinctCenterAlphaProfiles()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();

            var contourGo = Object.Instantiate(prefab);
            var contourAnchor = contourGo.AddComponent<POIAnchor>();
            contourAnchor.Initialise(new POIData
            {
                id = "sun_contours",
                name = "Sun Contours",
                category = "religious",
                has_status = false,
                status_unknown = false,
                has_captured_position = true,
            });

            var contourView = contourGo.GetComponentInChildren<MarkerView>();
            contourView.Initialise(contourAnchor, MarkerStyle.OutlineGold, MarkerShape.Circle, MarkerEffectFlags.SunContours);
            yield return null;

            var contourInner = contourGo.transform.Find("SunInner")?.GetComponent<Image>();
            Assert.IsNotNull(contourInner, "SunInner missing for contour variant.");
            Assert.IsNotNull(contourInner.sprite, "SunInner sprite missing for contour variant.");
            float contourCenterAlpha = SampleCenterAlpha(contourInner.sprite);
            Assert.Less(contourCenterAlpha, 0.15f,
                "Contour variant should keep center mostly transparent (ring behavior).");

            var circlesGo = Object.Instantiate(prefab);
            var circlesAnchor = circlesGo.AddComponent<POIAnchor>();
            circlesAnchor.Initialise(new POIData
            {
                id = "sun_circles",
                name = "Sun Circles",
                category = "religious",
                has_status = false,
                status_unknown = false,
                has_captured_position = true,
            });

            var circlesView = circlesGo.GetComponentInChildren<MarkerView>();
            circlesView.Initialise(circlesAnchor, MarkerStyle.OutlineGold, MarkerShape.Circle, MarkerEffectFlags.SunCircles);
            yield return null;

            var circlesInner = circlesGo.transform.Find("SunInner")?.GetComponent<Image>();
            Assert.IsNotNull(circlesInner, "SunInner missing for filled-circles variant.");
            Assert.IsNotNull(circlesInner.sprite, "SunInner sprite missing for filled-circles variant.");
            float circlesCenterAlpha = SampleCenterAlpha(circlesInner.sprite);
            Assert.Greater(circlesCenterAlpha, 0.85f,
                "Filled-circles variant should keep center mostly opaque.");

            Object.Destroy(contourGo);
            Object.Destroy(circlesGo);
        }

        private static MarkerCircleGlyphView CreateGlyphChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MarkerCircleGlyphView));
            go.transform.SetParent(parent, false);
            return go.GetComponent<MarkerCircleGlyphView>();
        }

        private static MarkerRingView CreateRingChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MarkerRingView));
            go.transform.SetParent(parent, false);
            return go.GetComponent<MarkerRingView>();
        }

        private static Sprite CreateSprite(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static bool GetPrivateBool(object target, string fieldName)
        {
            Assert.IsNotNull(target, "Target instance is null.");
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private bool field '{fieldName}'.");
            return (bool)field.GetValue(target);
        }

        private static float SampleCenterAlpha(Sprite sprite)
        {
            var tex = sprite.texture;
            int x = Mathf.Clamp(Mathf.RoundToInt(tex.width * 0.5f), 0, tex.width - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(tex.height * 0.5f), 0, tex.height - 1);
            return tex.GetPixel(x, y).a;
        }

        // SECTION 16 / Group2 (rect-ratio at all 5 sizes): assert the symbol's
        // RectTransform.sizeDelta follows the SizeCm/100 cm->m conversion (the single
        // call site in MarkerView.ApplyVisuals) and is strictly monotonic largest
        // Level 1 -> smallest Level 5 across the 5 framework-default sizes.
        [UnityTest]
        public IEnumerator HierarchySize_RectRatioScalesBySizeCmAndIsMonotonic()
        {
            var levels = new HierarchyLevelEntry[]
            {
                new HierarchyLevelEntry { key = "level_1", size_cm = 20f, show_label = true },
                new HierarchyLevelEntry { key = "level_2", size_cm = 15f, show_label = true },
                new HierarchyLevelEntry { key = "level_3", size_cm = 10f, show_label = true },
                new HierarchyLevelEntry { key = "level_4", size_cm = 5f,  show_label = false },
                new HierarchyLevelEntry { key = "level_5", size_cm = 2f,  show_label = false },
            };
            MarkerHierarchyResolver.Configure(levels);

            try
            {
                string[] keys = { "level_1", "level_2", "level_3", "level_4", "level_5" };
                float[] expected = { 20f / 100f, 15f / 100f, 10f / 100f, 5f / 100f, 2f / 100f };
                var observed = new float[keys.Length];

                for (int i = 0; i < keys.Length; i++)
                {
                    var root = new GameObject("MarkerRoot_" + keys[i]);
                    var anchor = root.AddComponent<POIAnchor>();
                    anchor.Initialise(new POIData
                    {
                        id = "poi_" + keys[i],
                        name = keys[i] + " Label",
                        category = "religious",
                        hierarchy_level_key = keys[i],
                        has_status = false,
                    });

                    // Same wiring shape as Initialise_MissingIconAndEffectWiring above
                    // (bare root + injected glyph children) so ApplyVisuals runs through
                    // the real Initialise path WallSession uses (Initialise->ApplyVisuals).
                    var symbol = CreateGlyphChild(root.transform, "Symbol");
                    var badge = CreateGlyphChild(root.transform, "Badge");
                    var ring = CreateRingChild(root.transform, "Ring");
                    var view = root.AddComponent<MarkerView>();
                    SetPrivateField(view, "symbol", symbol);
                    SetPrivateField(view, "badge", badge);
                    SetPrivateField(view, "ring", ring);

                    view.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle);
                    yield return null; // ApplyVisuals + EnsureMarkerWiring settle

                    Assert.IsNotNull(symbol, "symbol glyph must be wired.");
                    observed[i] = symbol.RectTransform.sizeDelta.x;
                    Object.Destroy(root);
                }

                // Absolute: sizeDelta.x == SizeCm / 100 (MarkerView cm->m call site).
                for (int i = 0; i < keys.Length; i++)
                {
                    Assert.AreEqual(expected[i], observed[i], 0.001f,
                        $"level {keys[i]}: sizeDelta.x expected {expected[i]:F3} (SizeCm/100), got {observed[i]:F3}");
                }
                // Relative: strictly decreasing as size_cm decreases (Group2 ratio).
                for (int i = 1; i < observed.Length; i++)
                {
                    Assert.Greater(observed[i - 1], observed[i],
                        $"size must decrease level_{i}->level_{i + 1}: {observed[i - 1]:F3} !> {observed[i]:F3}");
                }
            }
            finally
            {
                MarkerHierarchyResolver.ResetToDefaults();
            }
        }

    }
}
