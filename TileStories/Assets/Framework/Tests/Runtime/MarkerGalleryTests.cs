using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TileStories;

namespace TileStories.Tests
{
    // Loading a known project asset by path via AssetDatabase is the standard,
    // idiomatic way to get a prefab reference into an Editor-run PlayMode test
    // (this project's tests run via the Editor/batch-mode workflow in guidelines
    // section 6.2, not standalone on-device, so UNITY_EDITOR is defined whenever
    // this actually runs).
    internal static class MarkerGalleryTestFixture
    {
        private const string PrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        public static GameObject LoadPrefab()
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, $"Could not load POI_Marker prefab at {PrefabPath} -- has it moved?");
            return prefab;
#else
            Assert.Fail("MarkerGalleryTestFixture.LoadPrefab requires the Unity Editor (AssetDatabase) -- not available in a standalone/on-device build.");
            return null;
#endif
        }
    }

    // The automated half of section 18 -- same MarkerGalleryDefinitions.Entries as the
    // visual harness, asserted instead of eyeballed. Runnable via Unity MCP's
    // run_tests tool with zero screenshots. Grows automatically as Phase 2 adds
    // entries -- no changes needed to this file between phases.
    public class MarkerGalleryTests
    {
        private static (GameObject go, MarkerView view) Spawn(GameObject prefab, MarkerGalleryEntry entry)
        {
            var go = Object.Instantiate(prefab);
            var poiData = new POIData
            {
                id = entry.Label, name = entry.Label, category = entry.Category,
                status_pct = entry.StatusPct, has_status = entry.HasStatus,
                status_unknown = entry.StatusUnknown, is_hero = entry.IsHero,
                rotate_contour = entry.RotateContour,
                has_captured_position = true,
            };
            var anchor = go.AddComponent<POIAnchor>();
            anchor.Initialise(poiData);
            var view = go.GetComponentInChildren<MarkerView>();
            view.Initialise(anchor, entry.Style, entry.Shape, entry.EffectFlags);
            return (go, view);
        }

        [SetUp]
        public void SetUp()
        {
            CategoryPalette.ClearOverrides();
            ConfigureDeclaredCategoriesForGallery();
        }

        [TearDown]
        public void TearDown() => CategoryPalette.ClearOverrides();

        private static void ConfigureDeclaredCategoriesForGallery()
        {
            var declared = MarkerGalleryDefinitions.Entries
                .Select(e => e.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .Select(c => new CategoryStyleEntry
                {
                    category = c,
                    color_hex = string.Empty,
                    icon_key = CategoryPalette.ResolveIconKey(c)
                })
                .ToList();

            CategoryPalette.Configure(declared);
        }

        [UnityTest]
        public IEnumerator Symbol_AlwaysHasNonNullSprite()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            int failedCount = 0;
            string failures = "";
            
            foreach (var entry in MarkerGalleryDefinitions.Entries)
            {
                var (go, _) = Spawn(prefab, entry);
                yield return null;
                var symbolImage = go.transform.Find("Symbol")?.GetComponent<Image>();
                
                if (symbolImage == null)
                {
                    failures += $"\n{entry.Label}: Symbol Image missing entirely";
                    failedCount++;
                }
                else if (symbolImage.sprite == null)
                {
                    failures += $"\n{entry.Label}: Symbol sprite is null (will render as brown square)";
                    failedCount++;
                }
                
                Object.Destroy(go);
            }
            
            if (failedCount > 0)
            {
                Assert.Fail($"Symbol sprite validation failed for {failedCount}/{MarkerGalleryDefinitions.Entries.Count} entries:{failures}");
            }
        }

        [UnityTest]
        public IEnumerator Ring_OnlyEnabledForKnownStatusAndNonBadgeStyle()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            int failedCount = 0;
            string failures = "";
            
            foreach (var entry in MarkerGalleryDefinitions.Entries)
            {
                var (go, _) = Spawn(prefab, entry);
                yield return null;
                var ringImage = go.transform.Find("Ring")?.GetComponent<Image>();
                bool expectRing = entry.HasStatus && !entry.StatusUnknown && entry.Style != MarkerStyle.Badge;
                bool actualEnabled = ringImage != null && ringImage.enabled;
                
                if (actualEnabled != expectRing)
                {
                    failures += $"\n{entry.Label}: ring enabled={actualEnabled} but expected={expectRing} (style={entry.Style}, hasStatus={entry.HasStatus}, unknown={entry.StatusUnknown})";
                    failedCount++;
                }
                
                if (expectRing && ringImage != null && ringImage.sprite == null)
                {
                    failures += $"\n{entry.Label}: ring enabled but sprite is null";
                    failedCount++;
                }
                
                Object.Destroy(go);
            }
            
            if (failedCount > 0)
            {
                Assert.Fail($"Ring validation failed for {failedCount}/{MarkerGalleryDefinitions.Entries.Count} entries:{failures}");
            }
        }

        [UnityTest]
        public IEnumerator Badge_OnlyActiveForBadgeStyleOrUnknownStatus()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            int failedCount = 0;
            string failures = "";
            
            foreach (var entry in MarkerGalleryDefinitions.Entries)
            {
                var (go, _) = Spawn(prefab, entry);
                yield return null;
                var badgeGo = go.transform.Find("Badge")?.gameObject;
                bool expectBadge = entry.HasStatus && (entry.StatusUnknown || entry.Style == MarkerStyle.Badge);
                bool actualActive = badgeGo != null && badgeGo.activeSelf;
                
                if (actualActive != expectBadge)
                {
                    failures += $"\n{entry.Label}: badge active={actualActive} but expected={expectBadge}";
                    failedCount++;
                }
                
                // When badge is active, verify it has a valid sprite
                if (expectBadge && badgeGo != null && badgeGo.activeSelf)
                {
                    var badgeImage = badgeGo.GetComponent<Image>();
                    if (badgeImage == null || badgeImage.sprite == null)
                    {
                        failures += $"\n{entry.Label}: badge is active but has no sprite";
                        failedCount++;
                    }
                }
                
                Object.Destroy(go);
            }
            
            if (failedCount > 0)
            {
                Assert.Fail($"Badge validation failed for {failedCount}/{MarkerGalleryDefinitions.Entries.Count} entries:{failures}");
            }
        }

        [UnityTest]
        public IEnumerator UnknownStatus_AlwaysRendersUnknownColorRegardlessOfStyle()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var unknownEntries = MarkerGalleryDefinitions.Entries.FindAll(e => e.StatusUnknown);
            if (unknownEntries.Count == 0) yield break; // not added until Phase 2 step 3-5

            foreach (var entry in unknownEntries)
            {
                var (go, _) = Spawn(prefab, entry);
                yield return null;
                var badgeImage = go.transform.Find("Badge")?.GetComponent<Image>();
                Assert.IsNotNull(badgeImage, $"{entry.Label}: expected a Badge Image for unknown status");
                Assert.AreEqual(StatusRamp.UnknownColor, badgeImage.color,
                    $"{entry.Label}: unknown-status badge must use StatusRamp.UnknownColor, not the ordinal ramp");
                Object.Destroy(go);
            }
        }

        [UnityTest]
        public IEnumerator CategoryOverride_TakesPriorityOverHashFallback()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            CategoryPalette.Configure(MarkerGalleryDefinitions.Overrides);
            var entry = MarkerGalleryDefinitions.Entries.Find(e => e.Group == "Override" && e.Category == "furniture");
            if (entry.Label == null) yield break; // not added until Phase 2 step 7

            var (go, _) = Spawn(prefab, entry);
            yield return null;
            var symbolImage = go.transform.Find("Symbol")?.GetComponent<Image>();
            ColorUtility.TryParseHtmlString("#8033CC", out var expected);
            Assert.AreEqual(expected, symbolImage.color, "furniture category should render the configured override colour");
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator OutlineSameHue_RingUsesCategoryHueInsteadOfGoldRampColor()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var entry = new MarkerGalleryEntry("Adhoc", "same_hue_ring", "civic",
                MarkerStyle.OutlineSameHue, MarkerShape.Circle, 80f, true, false, false);

            var (go, _) = Spawn(prefab, entry);
            yield return null;

            var ringImage = go.transform.Find("Ring")?.GetComponent<Image>();
            Assert.IsNotNull(ringImage, "Ring image missing.");
            Assert.IsTrue(ringImage.enabled, "Ring should be enabled for known status non-badge markers.");

            Color goldRampColor = StatusRamp.Resolve(80f).RingColor;
            Assert.AreNotEqual(goldRampColor, ringImage.color,
                "OutlineSameHue ring should not use the gold/rust ramp color.");

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator BadgeStyle_BadgeUsesSameIconAsSymbol_AtSmallerSize()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var entry = new MarkerGalleryEntry("Adhoc", "badge_icon", "civic",
                MarkerStyle.Badge, MarkerShape.Circle, 60f, true, false, false);

            var (go, _) = Spawn(prefab, entry);
            yield return null;

            var symbolIcon = go.transform.Find("Symbol/Icon")?.GetComponent<Image>();
            var badgeIcon = go.transform.Find("Badge/Icon")?.GetComponent<Image>();
            var symbolRect = go.transform.Find("Symbol")?.GetComponent<RectTransform>();
            var badgeRect = go.transform.Find("Badge")?.GetComponent<RectTransform>();

            Assert.IsNotNull(symbolIcon, "Symbol icon image missing.");
            Assert.IsNotNull(badgeIcon, "Badge icon image missing.");
            Assert.IsTrue(symbolIcon.enabled, "Symbol icon should be enabled.");
            Assert.IsTrue(badgeIcon.enabled, "Badge icon should be enabled for Badge style known status.");
            Assert.IsNotNull(symbolIcon.sprite, "Symbol icon sprite should not be null.");
            Assert.IsNotNull(badgeIcon.sprite, "Badge icon sprite should not be null.");
            Assert.AreEqual(symbolIcon.sprite.name, badgeIcon.sprite.name,
                "Badge should reuse the same semantic icon as Symbol.");

            Assert.IsNotNull(symbolRect, "Symbol rect missing.");
            Assert.IsNotNull(badgeRect, "Badge rect missing.");
            Assert.Less(badgeRect.sizeDelta.x, symbolRect.sizeDelta.x,
                "Badge should render as a smaller symbol than the main marker.");

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator Contour_RingIsVisiblySeparatedFromSymbol()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var entry = new MarkerGalleryEntry("Adhoc", "contour_gap", "civic",
                MarkerStyle.OutlineGold, MarkerShape.Circle, 40f, true, false, false);

            var (go, _) = Spawn(prefab, entry);
            yield return null;

            var symbolRect = go.transform.Find("Symbol")?.GetComponent<RectTransform>();
            var ringRect = go.transform.Find("Ring")?.GetComponent<RectTransform>();

            Assert.IsNotNull(symbolRect, "Symbol rect missing.");
            Assert.IsNotNull(ringRect, "Ring rect missing.");
            Assert.Greater(ringRect.sizeDelta.x, symbolRect.sizeDelta.x * 1.14f,
                "Ring should be sized with clear separation from symbol base shape.");

            Object.Destroy(go);
        }

        [Test]
        public void StatusShapeCoverage_IncludesNonCircleShapesForRingAndBadgeStyles()
        {
            var requiredShapes = new[] { MarkerShape.RoundedSquare, MarkerShape.Hexagon, MarkerShape.Diamond, MarkerShape.Star };

            foreach (var shape in requiredShapes)
            {
                Assert.IsTrue(MarkerGalleryDefinitions.Entries.Exists(e =>
                        e.Group == "Status+Shape - OutlineGold" && e.Shape == shape && e.HasStatus),
                    $"Missing OutlineGold status+shape entry for {shape}.");

                Assert.IsTrue(MarkerGalleryDefinitions.Entries.Exists(e =>
                        e.Group == "Status+Shape - OutlineSameHue" && e.Shape == shape && e.HasStatus),
                    $"Missing OutlineSameHue status+shape entry for {shape}.");

                Assert.IsTrue(MarkerGalleryDefinitions.Entries.Exists(e =>
                        e.Group == "Status+Shape - Badge" && e.Shape == shape && e.HasStatus),
                    $"Missing Badge status+shape entry for {shape}.");
            }
        }

        [Test]
        public void HeroCoverage_IncludesAllRequestedModesAcrossAllHeroRows()
        {
            string[] heroGroups =
            {
                "Hero Effects - Simple",
                "Hero Effects - OutlineGold",
                "Hero Effects - OutlineSameHue",
                "Hero Effects - Badge",
            };

            MarkerEffectFlags[] requiredModes =
            {
                MarkerEffectFlags.Pulse,
                MarkerEffectFlags.SunCircles,
                MarkerEffectFlags.SunContours,
                MarkerEffectFlags.PulseSunCircles,
                MarkerEffectFlags.PulseSunContours,
            };

            foreach (string group in heroGroups)
            {
                foreach (var mode in requiredModes)
                {
                    Assert.IsTrue(
                        MarkerGalleryDefinitions.Entries.Exists(e => e.Group == group && e.EffectFlags == mode && e.IsHero),
                        $"Missing hero coverage row for group '{group}' with mode '{mode}'.");
                }
            }
        }

        [UnityTest]
        public IEnumerator HeroSunVariants_RenderCircularSprites_NotSquareQuads()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var heroSunEntries = MarkerGalleryDefinitions.Entries.FindAll(e =>
                e.IsHero && HasEffect(e.EffectFlags, MarkerEffectFlags.SunCircles | MarkerEffectFlags.SunContours));

            foreach (var entry in heroSunEntries)
            {
                var (go, _) = Spawn(prefab, entry);
                yield return null;

                var sunInner = go.transform.Find("SunInner")?.GetComponent<Image>();
                var sunMiddle = go.transform.Find("SunMiddle")?.GetComponent<Image>();
                var sunOuter = go.transform.Find("SunOuter")?.GetComponent<Image>();
                var sunFx = go.GetComponent<MarkerSunEffect>();
                Assert.IsNotNull(sunInner, $"{entry.Group}/{entry.Label}: SunInner missing.");
                Assert.IsNotNull(sunMiddle, $"{entry.Group}/{entry.Label}: SunMiddle missing.");
                Assert.IsNotNull(sunOuter, $"{entry.Group}/{entry.Label}: SunOuter missing.");
                Assert.IsNotNull(sunFx, $"{entry.Group}/{entry.Label}: MarkerSunEffect missing.");

                if (HasEffect(entry.EffectFlags, MarkerEffectFlags.SunContours))
                    Assert.AreEqual(MarkerSunEffect.SunVisualStyle.Contours, sunFx.CurrentStyle,
                        $"{entry.Group}/{entry.Label}: expected contour sun style.");
                else if (HasEffect(entry.EffectFlags, MarkerEffectFlags.SunCircles))
                    Assert.AreEqual(MarkerSunEffect.SunVisualStyle.FilledCircles, sunFx.CurrentStyle,
                        $"{entry.Group}/{entry.Label}: expected filled-circles sun style.");

                AssertSpriteLooksCircular(sunInner.sprite, $"{entry.Group}/{entry.Label} SunInner");
                AssertSpriteLooksCircular(sunMiddle.sprite, $"{entry.Group}/{entry.Label} SunMiddle");
                AssertSpriteLooksCircular(sunOuter.sprite, $"{entry.Group}/{entry.Label} SunOuter");

                float centerAlpha = SampleCenterAlpha(sunInner.sprite);
                if (HasEffect(entry.EffectFlags, MarkerEffectFlags.SunContours))
                    Assert.Less(centerAlpha, 0.15f, $"{entry.Group}/{entry.Label}: contour sun should keep center transparent.");
                if (HasEffect(entry.EffectFlags, MarkerEffectFlags.SunCircles))
                    Assert.Greater(centerAlpha, 0.85f, $"{entry.Group}/{entry.Label}: filled sun should keep center opaque.");

                Object.Destroy(go);
            }
        }

        [UnityTest]
        public IEnumerator Ring_RotatesOnlyWhenRotateContourIsSet()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            string failures = "";
            int failedCount = 0;

            foreach (var entry in MarkerGalleryDefinitions.Entries)
            {
                if (entry.Group != "Contour Rotation") continue;

                var (go, _) = Spawn(prefab, entry);
                var ringTransform = go.transform.Find("Ring");
                yield return null;
                float angleBefore = ringTransform.localEulerAngles.z;
                yield return null;
                yield return null;
                float angleAfter = ringTransform.localEulerAngles.z;

                bool didRotate = Mathf.Abs(Mathf.DeltaAngle(angleBefore, angleAfter)) > 0.01f;
                if (didRotate != entry.RotateContour)
                {
                    failures += $"\n{entry.Label}: rotated={didRotate} but expected={entry.RotateContour}";
                    failedCount++;
                }

                Object.Destroy(go);
            }

            if (failedCount > 0)
                Assert.Fail($"Contour rotation check failed for {failedCount} entries:{failures}");
        }

        [UnityTest]
        public IEnumerator AccentEffects_ActiveRegardlessOfIsHero()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            string failures = "";
            int failedCount = 0;

            foreach (var entry in MarkerGalleryDefinitions.Entries)
            {
                bool expectsAccent = entry.EffectFlags.HasFlag(MarkerEffectFlags.RingPulse)
                    || entry.EffectFlags.HasFlag(MarkerEffectFlags.SimpleSun)
                    || entry.EffectFlags.HasFlag(MarkerEffectFlags.Beacon);
                if (!expectsAccent) continue;

                var (go, _) = Spawn(prefab, entry);
                yield return null;
                var accentImage = go.transform.Find("Accent")?.GetComponent<Image>();

                if (accentImage == null || !accentImage.enabled)
                {
                    failures += $"\n{entry.Label} (isHero={entry.IsHero}): Accent missing or not enabled";
                    failedCount++;
                }
                else if (accentImage.sprite == null)
                {
                    failures += $"\n{entry.Label}: Accent sprite is null";
                    failedCount++;
                }

                Object.Destroy(go);
            }

            if (failedCount > 0)
                Assert.Fail($"Accent effect check failed for {failedCount} entries:{failures}");
        }

        // Section 20.1: background shape None hides the symbol's backdrop while
        // keeping the icon visible. The Symbol's background Image should be
        // disabled, but the Icon child should still be enabled.
        [UnityTest]
        public IEnumerator NoneBackground_HidesSymbolBackdrop_KeepsIconVisible()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var entry = MarkerGalleryDefinitions.Entries.Find(e => e.Group == "None Background");
            if (entry.Label == null) yield break;

            var (go, _) = Spawn(prefab, entry);
            yield return null;

            var symbolImage = go.transform.Find("Symbol")?.GetComponent<Image>();
            Assert.IsNotNull(symbolImage, "Symbol Image missing for None background entry.");

            // The background Image must be disabled -- that's what "None" means.
            Assert.IsFalse(symbolImage.enabled,
                "None background: Symbol background Image should be disabled (no backdrop).");

            // The Icon child must still be enabled -- "None" hides the backdrop,
            // not the icon.
            var iconImage = go.transform.Find("Symbol/Icon")?.GetComponent<Image>();
            Assert.IsNotNull(iconImage, "Symbol/Icon Image missing for None background entry.");
            Assert.IsTrue(iconImage.enabled,
                "None background: Icon should remain visible when background shape is None.");

            Object.Destroy(go);
        }

        // Section 21: hero_icon_key overrides the category-derived icon for hero
        // POIs. We set hero_icon_key to "temple" (a known icon key) on a civic
        // category hero POI and verify the Symbol's icon sprite is the temple
        // sprite, not the civic "columns" sprite.
        [UnityTest]
        public IEnumerator HeroIconKey_OverridesCategoryIcon()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var entry = MarkerGalleryDefinitions.Entries.Find(e => e.Group == "Hero Icon Override");
            if (entry.Label == null) yield break;

            // Spawn with the gallery entry, then manually set hero_icon_key on
            // the POIData before Initialise -- the gallery Spawn helper doesn't
            // set hero_icon_key, so we do it here to test the override path.
            var go = Object.Instantiate(prefab);
            var poiData = new POIData
            {
                id = entry.Label, name = entry.Label, category = entry.Category,
                is_hero = entry.IsHero, has_captured_position = true,
                hero_icon_key = "temple",
            };
            var anchor = go.AddComponent<POIAnchor>();
            anchor.Initialise(poiData);
            var view = go.GetComponentInChildren<MarkerView>();
            view.Initialise(anchor, entry.Style, entry.Shape, entry.EffectFlags);
            yield return null;

            var iconImage = go.transform.Find("Symbol/Icon")?.GetComponent<Image>();
            Assert.IsNotNull(iconImage, "Symbol/Icon Image missing for hero icon override test.");
            Assert.IsNotNull(iconImage.sprite, "Hero icon sprite should not be null.");

            // "temple" is the icon key for the "religious" category. "civic"
            // resolves to "columns". If the override works, the sprite name
            // should contain "temple", not "columns".
            Assert.IsTrue(iconImage.sprite.name.ToLowerInvariant().Contains("temple"),
                $"Hero icon override: expected temple icon, got '{iconImage.sprite.name}'.");

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator HeroSunContours_UsesCenterToOuterLightFalloff()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var entry = new MarkerGalleryEntry("Adhoc", "sun_falloff", "religious",
                MarkerStyle.OutlineGold, MarkerShape.Circle, 0f, false, false, true, MarkerEffectFlags.SunContours);

            var (go, _) = Spawn(prefab, entry);
            yield return null;

            var sunFx = go.GetComponent<MarkerSunEffect>();
            Assert.IsNotNull(sunFx, "MarkerSunEffect missing.");

            float innerAlpha = GetPrivateFloat(sunFx, "innerAlpha");
            float middleAlpha = GetPrivateFloat(sunFx, "middleAlpha");
            float outerAlpha = GetPrivateFloat(sunFx, "outerAlpha");
            Assert.Greater(innerAlpha, middleAlpha, "Inner sun layer should be stronger than middle layer.");
            Assert.Greater(middleAlpha, outerAlpha, "Middle sun layer should be stronger than outer layer.");

            Object.Destroy(go);
        }

        // Visual verification: captures a screenshot of each marker to confirm it renders
        // with the correct shape, color, and features (not just a brown square).
        // This test requires a human to inspect the saved screenshots in the project's
        // Screenshots folder to verify visual correctness.
        [UnityTest]
        public IEnumerator Visual_EachMarkerRendersCorrectShape()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            int tested = 0;
            string outputDir = System.IO.Path.Combine(Application.dataPath, "..", "MarkerGalleryScreenshots");
            System.IO.Directory.CreateDirectory(outputDir);

            foreach (var entry in MarkerGalleryDefinitions.Entries)
            {
                var (go, view) = Spawn(prefab, entry);
                go.transform.position = new Vector3(0, 0, 5); // Position in front of camera
                yield return null;
                yield return null; // Wait for render

                // Capture screenshot for this marker
                string filename = SanitizeFileName($"{entry.Group}_{entry.Label}_{entry.Style}_{entry.Shape}.png");
                string filepath = System.IO.Path.Combine(outputDir, filename);
                ScreenCapture.CaptureScreenshot(filepath);
                yield return new WaitForEndOfFrame();

                // Verify the marker has non-default visual properties
                var symbolImage = go.transform.Find("Symbol")?.GetComponent<Image>();
                Assert.IsNotNull(symbolImage, $"{entry.Label}: Symbol Image missing");
                Assert.IsNotNull(symbolImage.sprite, $"{entry.Label}: Symbol sprite is null - will render as brown square");
                Assert.IsTrue(symbolImage.color.a > 0.9f, $"{entry.Label}: Symbol color should be mostly opaque");

                Object.Destroy(go);
                tested++;
            }

            Debug.Log($"[VisualTest] Captured {tested} marker screenshots in {outputDir}");
            yield break;
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');
            return value;
        }

        private static float GetPrivateFloat(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private float field '{fieldName}'.");
            return (float)field.GetValue(target);
        }

        private static void AssertSpriteLooksCircular(Sprite sprite, string context)
        {
            Assert.IsNotNull(sprite, $"{context}: sprite is null.");
            var tex = sprite.texture;
            Assert.IsNotNull(tex, $"{context}: texture is null.");

            // Transparent corners are a strong signal this is not rendering as a square fill.
            Assert.Less(GetPixelAlpha(tex, 0, 0), 0.05f, $"{context}: top-left corner should be transparent.");
            Assert.Less(GetPixelAlpha(tex, tex.width - 1, 0), 0.05f, $"{context}: top-right corner should be transparent.");
            Assert.Less(GetPixelAlpha(tex, 0, tex.height - 1), 0.05f, $"{context}: bottom-left corner should be transparent.");
            Assert.Less(GetPixelAlpha(tex, tex.width - 1, tex.height - 1), 0.05f, $"{context}: bottom-right corner should be transparent.");
        }

        private static float SampleCenterAlpha(Sprite sprite)
        {
            var tex = sprite.texture;
            int x = Mathf.Clamp(Mathf.RoundToInt(tex.width * 0.5f), 0, tex.width - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(tex.height * 0.5f), 0, tex.height - 1);
            return GetPixelAlpha(tex, x, y);
        }

        private static float GetPixelAlpha(Texture2D texture, int x, int y)
        {
            return texture.GetPixel(x, y).a;
        }

        private static bool HasEffect(MarkerEffectFlags mask, MarkerEffectFlags effects)
        {
            return (mask & effects) != 0;
        }
    }
}

