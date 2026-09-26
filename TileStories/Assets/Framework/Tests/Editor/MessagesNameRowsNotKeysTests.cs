using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The Editor Tab never shows a POI id or a taxonomy row's generated key, so no message may name anything by
    // one: a POI is named by its list title ("1. North tower"), a row by its name. Every config validator (found
    // by reflection, so a new one is covered automatically) runs on a real window over a config that breaks every
    // rule at once; the base-marker dropdown and the delete guard are checked the same way.
    public class MessagesNameRowsNotKeysTests
    {
        private const BindingFlags Any = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        // Keys and ids no developer has ever seen: none may appear in a message
        private static readonly string[] HiddenKeys = { "poi_zz9", "poi_yy8", "lvl_hub_7", "cat_rel_3", "bdg_crk_2", "out_des_9", "fld_mat_4",
                                                        "cat_blank_5", "bdg_dup_6" };

        private static WallConfigData BrokenEverywhere() => new WallConfigData
        {
            hierarchy_levels = new List<HierarchyLevelEntry>
            {
                new() { key = "lvl_hub_7", level_name = "Hub", size_cm = 250f, priority = 1 },   // size wrong
            },
            // a category with no name, and two badges with one name
            category_styles = new List<CategoryStyleEntry> { new() { key = "cat_rel_3", label = "Religious" }, new() { key = "cat_blank_5", label = " " } },
            badge_categories = new List<BadgeCategoryEntry> { new() { key = "bdg_crk_2", label = "Cracked" }, new() { key = "bdg_dup_6", label = "cracked" } },
            outline_levels = new List<OutlineLevelEntry> { new() { key = "out_des_9", label = "Destroyed" } },
            search_fields = new List<SearchFieldDefinition> { new() { key = "fld_mat_4", label = "Material", forced = true } },
            select_filter_search = new SelectFilterSearchSettings { search = new SearchSettings { mode = "not_a_mode" } },
            pois = new List<POIData>
            {
                // no level, no keywords for the required field, a custom symbol with no symbol
                new() { id = "poi_zz9", name = "North tower", category = "cat_rel_3", has_custom_symbol = true },
                // every reference stale: rows deleted after the POI was set up
                new() { id = "poi_yy8", name = "South gate", hierarchy_level_key = "gone_level", category = "gone_cat",
                        badge_category = "gone_badge", has_status = true, status_level_key = "gone_status" },
            },
        };

        private static List<EditorAlertItem> RunEveryValidator(WallConfigData config, out List<string> ran)
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            typeof(POIEditorToolWindow).GetField("_config", Any).SetValue(window, config);
            var issues = new List<EditorAlertItem>();
            ran = new List<string>();
            try
            {
                foreach (var m in typeof(POIEditorToolWindow).GetMethods(Any)
                             .Where(m => m.Name.StartsWith("Validate") && m.ReturnType == typeof(List<EditorAlertItem>)))
                {
                    var p = m.GetParameters();
                    object[] args = p.Length == 0 ? new object[0]
                        : p.Length == 1 && p[0].ParameterType.IsAssignableFrom(typeof(List<HierarchyLevelEntry>)) ? new object[] { config.hierarchy_levels }
                        : null;
                    Assert.IsNotNull(args, "the test does not know how to call validator " + m.Name + ": teach it");
                    issues.AddRange((List<EditorAlertItem>)m.Invoke(m.IsStatic ? null : window, args));
                    ran.Add(m.Name);
                }
            }
            finally { Object.DestroyImmediate(window); }
            return issues;
        }

        [Test]
        public void EveryValidationFinding_NamesPoisAndRowsTheWayTheWindowShowsThem_NeverByIdOrKey()
        {
            var issues = RunEveryValidator(BrokenEverywhere(), out var ran);
            Assert.GreaterOrEqual(ran.Count, 8, "precondition: every validator was found and run (" + string.Join(", ", ran) + ")");
            string report = EditorAlertItem.FormatList(issues, "");
            string all = string.Join("\n", issues.Select(i => i.subject + " | " + i.value + " | " + i.problem + " | " + i.fixHint));

            foreach (string key in HiddenKeys)
                StringAssert.DoesNotContain(key, all, "a message named something by a key the Editor Tab never shows");

            // POIs by their list title, rows by their names
            StringAssert.Contains("1. North tower", report);
            StringAssert.Contains("2. South gate", report);
            StringAssert.Contains("Hierarchy level 'Hub'", all, "the size finding names the level");
            StringAssert.Contains("pick one of 'Hub'", all, "the fix lists the levels as the dropdown does");
            StringAssert.Contains("'Material'", all, "the required keyword field is named by its label");
            StringAssert.Contains("Marker > Category Symbols, row 2", all, "a row with no name is named by its table and position");
            StringAssert.Contains("Badge > Badge Categories, rows 1 and 2", all, "two rows sharing a name (case ignored) are named by position");

            // A deleted row has no name left: its finding quotes exactly what the POI's dropdown shows for it
            foreach (string stale in new[] { "gone_level", "gone_cat", "gone_badge", "gone_status" })
                StringAssert.Contains("shown as '" + stale + ReferencePopupOptions.MissingSuffix + "'", all);

            // Every problem type was really produced (so the scan above covered each one)
            foreach (string phrase in new[] { "No Hierarchy Level is assigned", "no longer matches any row", "Category does not match",
                         "Badge category does not match", "Status level does not match", "Use Custom Symbol", "Marker Size (cm)",
                         "missing keywords", "Search Mode", "has no Category label", "Two rows share one Badge label" })
                StringAssert.Contains(phrase, all, "precondition: the broken config produced this finding");

            // No config-file vocabulary either: the developer reads Editor Tab labels
            foreach (string code in new[] { "hierarchy_level_key", "size_cm", "status_level_key", "config field", "keyed" })
                StringAssert.DoesNotContain(code, all);
        }

        [Test]
        public void BaseMarkerDropdown_ListsPoisByTheirListTitle_NeverByTheirId()
        {
            EffectUsageSummary.PreviewBaseOptions(BrokenEverywhere(), out string[] ids, out string[] labels);
            CollectionAssert.AreEqual(new[] { EffectsPreviewSpawner.PlainCircleName, "1. North tower", "2. South gate" }, labels);
            CollectionAssert.AreEqual(new[] { "", "poi_zz9", "poi_yy8" }, ids, "the value stored is still the id");
        }

        [Test]
        public void DeleteGuard_NamesTheRow_AndARowWithNoNameIsThisRow()
        {
            var asked = new List<string>();
            var saved = EditorDecision.Responder;
            EditorDecision.Responder = request => { asked.Add(request.Message); return DecisionAnswer.Cancel; };
            try
            {
                IdentityDeleteGuard.Confirm("Badge", "Cracked", 2);
                IdentityDeleteGuard.Confirm("Badge", "", 1);
            }
            finally { EditorDecision.Responder = saved; }

            StringAssert.StartsWith("2 POI(s) still reference 'Cracked'.", asked[0]);
            StringAssert.StartsWith("1 POI(s) still reference this row.", asked[1]);
        }
    }
}
