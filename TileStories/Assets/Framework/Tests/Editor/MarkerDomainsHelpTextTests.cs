using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // Every (i) help, Test guide and validation notice of the Marker / Badge / Outline / Effects /
    // Orientation domains (and their per-POI rows) is text a wall developer reads in the window:
    // ASCII only, app-agnostic, and pointing at Editor Tab controls -- never at a project doc, a code
    // class, a specific wall or this repo's dev scenes, and never at a control name that no longer
    // exists (_5.1_Editor_Tab.md, "Domain Manual Tests"). Collected by reflection, so a new constant
    // with a domain prefix is scanned automatically.
    public class MarkerDomainsHelpTextTests
    {
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;

        private static readonly string[] Prefixes =
        {
            "Marker", "Badge", "Outline", "Ring", "Icon", "Symbol", "Poi",
            "Effect", "Pulse", "Ripple", "Halo", "Beacon", "DisabledEffect",
            "Orientation", "VerticalAlignment", "ChildVerticalAlignment", "UpReference", "CustomUp",
            "FacingMode", "FacingBasis", "EditModePreview",
        };

        private static readonly string[] ForbiddenTerms =
        {
            ".md", ".cs", "_2.", "_5.1", "LivingRoom", "MockLocalizationProvider", "MarkerBillboard",
            "CategoryPalette", "Gallery", "Assets/Dev", "Facing column", "Custom symbol (optional)",
            "> Size (cm)", "lamp", "painting_", "dev_marker_", "dev_disp_",
        };

        // The old Hierarchy Levels column "Rotate" is now "Spin Ring" (Unity's own "Rotate tool" is fine)
        private static readonly Regex StaleRotateColumn = new Regex(@"\bRotate\b(?! tool)");

        private static Dictionary<string, string> DomainTexts()
        {
            var texts = new Dictionary<string, string>();
            foreach (var field in typeof(POIEditorToolWindow).GetFields(Static))
            {
                if (field.FieldType != typeof(string)) continue;
                if (!(field.Name.EndsWith("Help") || field.Name.EndsWith("Guide") || field.Name.EndsWith("Note"))) continue;
                if (!Prefixes.Any(p => field.Name.StartsWith(p))) continue;
                texts[field.Name] = (string)field.GetValue(null);
            }
            return texts;
        }

        [Test]
        public void DomainHelpAndGuideTexts_AreAsciiAppAgnostic_AndNameOnlyCurrentEditorTabControls()
        {
            var texts = DomainTexts();
            Assert.Greater(texts.Count, 40, "precondition: the domain help texts were really collected");
            foreach (string required in new[] { "MarkerSceneTestGuide", "BadgePlaymodeTestGuide", "OutlineDeviceTestGuide",
                         "EffectsPlaymodeTestGuide", "OrientationSceneTestGuide", "PoiCategoryHelp", "IconColorHelp", "OutlineTypesHelp" })
                Assert.IsTrue(texts.ContainsKey(required), required + " must be scanned");

            foreach (var pair in texts)
            {
                Assert.IsTrue(pair.Value.All(c => c < 128), pair.Key + " must be ASCII only");
                foreach (string term in ForbiddenTerms)
                    StringAssert.DoesNotContain(term, pair.Value, pair.Key + " must not contain '" + term + "'");
                Assert.IsFalse(StaleRotateColumn.IsMatch(pair.Value), pair.Key + " still names the old 'Rotate' column (now Spin Ring)");
            }
        }

        // Statements that were once wrong in these texts, each checked against what the runtime does
        [Test]
        public void DomainTexts_DoNotRepeatClaimsTheRuntimeContradicts()
        {
            var texts = DomainTexts();
            // MarkerVisualResolver: a known status with no badge category shows a status-coloured badge
            StringAssert.DoesNotContain("in which case it shows no badge", texts["BadgeSceneTestGuide"]);
            // OutlinePreviewSpawner draws the wall's CURRENT mode only (No outline + one cell per row)
            StringAssert.DoesNotContain("one cell per outline mode", texts["OutlinePreviewHelp"]);
            // LivePlayModeMarkerApplier: hierarchy-level size edits are live
            StringAssert.DoesNotContain("still needs Save + Copy + Play again", texts["MarkerPlaymodeTestGuide"]);
        }

        // The save/load validation notice names the Editor Tab control to fix, never a code class
        [Test]
        public void TaxonomyValidationNotices_NameTheEditorTabControl_NotCode()
        {
            var config = new WallConfigData
            {
                category_styles = new List<CategoryStyleEntry> { new CategoryStyleEntry { category = "real" } },
                badge_categories = new List<BadgeCategoryEntry> { new BadgeCategoryEntry { key = "real" } },
                outline_levels = new List<OutlineLevelEntry> { new OutlineLevelEntry { key = "real" } },
                pois = new List<POIData>
                {
                    new POIData { id = "p", category = "gone", badge_category = "gone", has_status = true, status_level_key = "gone", has_custom_symbol = true }
                }
            };
            var window = UnityEngine.ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                typeof(POIEditorToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, config);
                var issues = (List<EditorAlertItem>)typeof(POIEditorToolWindow)
                    .GetMethod("ValidateMarkerTaxonomyReferences", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(window, null);
                Assert.AreEqual(4, issues.Count, "category, badge, status level and custom symbol are all stale");
                string report = EditorAlertItem.FormatList(issues, "");
                foreach (string term in ForbiddenTerms)
                    StringAssert.DoesNotContain(term, report, "notice must not contain '" + term + "'");
                StringAssert.Contains("Specific Marker > this POI", report, "each notice says where to fix it");
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }
    }
}
