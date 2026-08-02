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
                is_hero = true
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
                is_hero = true,
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
                is_hero = true,
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
                is_hero = true,
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

        [UnityTest]
        public IEnumerator Initialise_EffectsFollowFlag_LabelFollowsHero()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();

            // Hero marker WITH pulse+sun effects: label visible, effects active.
            // is_hero controls only the label; MarkerEffectFlags controls effects.
            var heroGo = Object.Instantiate(prefab);
            var heroAnchor = heroGo.AddComponent<POIAnchor>();
            heroAnchor.Initialise(new POIData
            {
                id = "hero",
                name = "Hero Label",
                category = "religious",
                has_status = false,
                status_unknown = false,
                is_hero = true,
                has_captured_position = true,
            });

            var heroView = heroGo.GetComponentInChildren<MarkerView>();
            heroView.Initialise(heroAnchor, MarkerStyle.OutlineGold, MarkerShape.Circle, MarkerEffectFlags.PulseSunContours);
            yield return null;

            var heroLabel = heroGo.transform.Find("Label")?.gameObject;
            Assert.IsNotNull(heroLabel, "Hero label object missing.");
            Assert.IsTrue(heroLabel.activeSelf, "Hero label should be visible.");

            var heroLabelRect = heroLabel.GetComponent<RectTransform>();
            Assert.IsNotNull(heroLabelRect, "Hero label RectTransform missing.");
            Assert.AreEqual(0.5f, heroLabelRect.anchorMin.x, 0.0001f, "Hero label anchorMin.x should be centered.");
            Assert.AreEqual(0.5f, heroLabelRect.anchorMax.x, 0.0001f, "Hero label anchorMax.x should be centered.");
            Assert.AreEqual(0.5f, heroLabelRect.pivot.x, 0.0001f, "Hero label pivot.x should be centered.");
            Assert.AreEqual(0f, heroLabelRect.anchoredPosition.x, 0.0001f,
                "Hero label should stay centered under the marker container.");

            Assert.IsTrue(GetPrivateBool(heroGo.GetComponent<MarkerPulseEffect>(), "_active"),
                "Pulse effect should be active when PulseSunContours flag is set.");
            Assert.IsTrue(GetPrivateBool(heroGo.GetComponent<MarkerSunEffect>(), "_active"),
                "Sun effect should be active when PulseSunContours flag is set.");

            // Non-hero marker WITHOUT effects: label hidden, effects inactive.
            // Proves effects follow the flag, not is_hero.
            var normalGo = Object.Instantiate(prefab);
            var normalAnchor = normalGo.AddComponent<POIAnchor>();
            normalAnchor.Initialise(new POIData
            {
                id = "normal",
                name = "Standard Label",
                category = "religious",
                has_status = false,
                status_unknown = false,
                is_hero = false,
                has_captured_position = true,
            });

            var normalView = normalGo.GetComponentInChildren<MarkerView>();
            normalView.Initialise(normalAnchor, MarkerStyle.OutlineGold, MarkerShape.Circle, MarkerEffectFlags.None);
            yield return null;

            var normalLabel = normalGo.transform.Find("Label")?.gameObject;
            Assert.IsNotNull(normalLabel, "Standard label object missing.");
            Assert.IsFalse(normalLabel.activeSelf, "Standard marker label should be hidden.");
            Assert.IsFalse(GetPrivateBool(normalGo.GetComponent<MarkerPulseEffect>(), "_active"),
                "Pulse effect should be inactive when no effect flags are set.");
            Assert.IsFalse(GetPrivateBool(normalGo.GetComponent<MarkerSunEffect>(), "_active"),
                "Sun effect should be inactive when no effect flags are set.");

            Object.Destroy(heroGo);
            Object.Destroy(normalGo);
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

    }
}
