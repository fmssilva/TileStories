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
                status_unknown = entry.StatusUnknown,
                hierarchy_level_key = entry.HierarchyLevelKey,
                has_captured_position = true,
            };

            if (entry.HasStatus)
            {
                poiData.status_level_key = entry.StatusUnknown ? "unknown" : "partial_damage";
                poiData.badge_category = entry.StatusUnknown ? "unknown_damage" : "partial_damage";
            }

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
            ConfigureStatusAndBadgeTaxonomyForGallery();
        }

        [TearDown]
        public void TearDown()
        {
            CategoryPalette.ClearOverrides();
            BadgeCategoryPalette.Clear();
            StatusRamp.ResetToDefaults();
        }

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

        private static void ConfigureStatusAndBadgeTaxonomyForGallery()
        {
            BadgeCategoryPalette.Configure(new List<BadgeCategoryEntry>
            {
                new BadgeCategoryEntry { key = "intact", icon_key = "IconIntact", color_hex = "#22C55E" },
                new BadgeCategoryEntry { key = "partial_damage", icon_key = "IconPartialDamage", color_hex = "#F97316" },
                new BadgeCategoryEntry { key = "destroyed", icon_key = "IconDestroyed", color_hex = "#991B1B" },
                new BadgeCategoryEntry { key = "unknown_damage", icon_key = "IconUnknownDamage", color_hex = "#71717A" },
            });

            StatusRamp.Configure(new List<OutlineLevelEntry>
            {
                new OutlineLevelEntry { key = "intact", pct = 0f, line_style = "solid", color_hex = string.Empty, ring_width = 3.2f },
                new OutlineLevelEntry { key = "partial_damage", pct = 20f, line_style = "dash_long", color_hex = string.Empty, ring_width = 2.8f },
                new OutlineLevelEntry { key = "destroyed", pct = 100f, line_style = "dash_short", color_hex = string.Empty, ring_width = 2.0f },
                new OutlineLevelEntry { key = "unknown", pct = 100f, line_style = "dotted", color_hex = "#71717A", ring_width = 1.8f },
            });
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
        public IEnumerator Ring_EnabledForStatusAndNonBadgeStyle()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            int failedCount = 0;
            string failures = "";
            
            foreach (var entry in MarkerGalleryDefinitions.Entries)
            {
                var (go, _) = Spawn(prefab, entry);
                yield return null;
                var ringImage = go.transform.Find("Ring")?.GetComponent<Image>();
                bool expectRing = entry.HasStatus && entry.Style != MarkerStyle.Badge;
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
        public IEnumerator UnknownStatus_UsesUnknownBadgeAndDottedOutlineWhenApplicable()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var unknownEntries = MarkerGalleryDefinitions.Entries.FindAll(e => e.StatusUnknown);
            if (unknownEntries.Count == 0) yield break; // not added until Phase 2 step 3-5

            SpriteKeyLibrary iconLibrary = Resources.Load<SpriteKeyLibrary>("MarkerSymbols/living_room_IconLibrary");
            Assert.IsNotNull(iconLibrary, "Expected wall icon library at MarkerSymbols/living_room_IconLibrary.");
            Sprite expectedUnknownDamageIcon = iconLibrary.Get("IconUnknownDamage");
            Assert.IsNotNull(expectedUnknownDamageIcon, "Expected IconUnknownDamage key in icon library.");
            Sprite expectedDottedRing = iconLibrary.Get("dotted");
            Assert.IsNotNull(expectedDottedRing, "Expected dotted line-style key in icon library.");

            foreach (var entry in unknownEntries)
            {
                var (go, _) = Spawn(prefab, entry);
                yield return null;
                var badgeImage = go.transform.Find("Badge")?.GetComponent<Image>();
                Assert.IsNotNull(badgeImage, $"{entry.Label}: expected a Badge Image for unknown status");
                Color expectedUnknownTint;
                ColorUtility.TryParseHtmlString("#71717A", out expectedUnknownTint);
                Assert.AreEqual(expectedUnknownTint, badgeImage.color,
                    $"{entry.Label}: unknown-status badge should use the configured unknown_damage color.");

                var badgeIcon = go.transform.Find("Badge/Icon")?.GetComponent<Image>();
                Assert.IsNotNull(badgeIcon, $"{entry.Label}: expected Badge/Icon image.");
                Assert.AreEqual(expectedUnknownDamageIcon, badgeIcon.sprite,
                    $"{entry.Label}: unknown-status badge should resolve IconUnknownDamage by default taxonomy.");

                if (entry.Style != MarkerStyle.Badge)
                {
                    var ringImage = go.transform.Find("Ring")?.GetComponent<Image>();
                    Assert.IsNotNull(ringImage, $"{entry.Label}: expected Ring image.");
                    Assert.IsTrue(ringImage.enabled, $"{entry.Label}: unknown status should render a ring for non-badge styles.");
                    Assert.AreEqual(expectedDottedRing, ringImage.sprite,
                        $"{entry.Label}: unknown status ring should use dotted outline style.");
                }

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
                MarkerStyle.OutlineSameHue, MarkerShape.Circle, 80f, true, false);

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
                MarkerStyle.Badge, MarkerShape.Circle, 60f, true, false);

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
            Assert.AreEqual("IconPartialDamage", badgeIcon.sprite.name,
                "Badge style with badge_category=partial_damage should render IconPartialDamage.");

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
                MarkerStyle.OutlineGold, MarkerShape.Circle, 40f, true, false);

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

        [UnityTest]
        public IEnumerator SunContours_UsesCenterToOuterLightFalloff()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var entry = new MarkerGalleryEntry("Adhoc", "sun_falloff", "religious",
                MarkerStyle.OutlineGold, MarkerShape.Circle, 0f, false, false,
                effectFlags: MarkerEffectFlags.SunContours);

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

