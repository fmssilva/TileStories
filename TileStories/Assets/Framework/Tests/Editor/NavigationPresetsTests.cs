using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The navigation presets (_3.2 section 8) as DATA: the table is the guide's, every element it names is registered for the slot it
    // fills, and applying one writes exactly its seven fields -- a walk over EVERY field of the settings object, lists included, before and
    // after, on a card whose other rows were all changed first.
    public class NavigationPresetsTests
    {
        // Section 8's table, typed once here from the guide: label, layout, top, bottom, floating, menu, edge, auto_nav_block
        private static readonly string[][] GuideTable =
        {
            new[] { "Long read", "scroll", "text_tabs", "none", "none", "none", "none", "none" },
            new[] { "Rich point", "scroll", "icon_tabs_chips", "none", "none", "drawer_tree", "none", "none" },
            new[] { "One hand", "scroll", "none", "bottom_bar", "none", "grid_overlay", "none", "none" },
            new[] { "Over the camera", "scroll", "none", "none", "fab_dial", "none", "none", "none" },
            new[] { "Deep dive", "scroll", "kind_chips", "none", "none", "drawer_tree", "edge_scrubber", "nav_contents" },
            new[] { "Guided", "scroll", "progress_map", "next_up", "none", "none", "none", "nav_contents" },
            new[] { "Story", "pages", "none", "next_up", "none", "none", "none", "none" },
            new[] { "Quick glance", "accordion", "none", "none", "fab_dial", "none", "none", "none" },
            new[] { "Overview", "bento", "none", "none", "none", "none", "none", "none" },
        };

        private static readonly BindingFlags PublicFields = BindingFlags.Public | BindingFlags.Instance;

        [Test]
        public void TheNinePresets_AreSectionEightsTable_StrongestFirst()
        {
            Assert.AreEqual(GuideTable.Length, NavigationPresets.All.Count, "nine named sets");
            for (int i = 0; i < GuideTable.Length; i++)
            {
                var row = GuideTable[i];
                var preset = NavigationPresets.All[i];
                Assert.AreEqual(row[0], preset.Label, "row " + i + " is named");
                Assert.AreEqual(row[1], preset.Layout, row[0] + ": layout");
                Assert.AreEqual(row[2], preset.Top, row[0] + ": top");
                Assert.AreEqual(row[3], preset.Bottom, row[0] + ": bottom");
                Assert.AreEqual(row[4], preset.Floating, row[0] + ": floating");
                Assert.AreEqual(row[5], preset.Menu, row[0] + ": menu");
                Assert.AreEqual(row[6], preset.Edge, row[0] + ": edge");
                Assert.AreEqual(row[7], preset.AutoNavBlock, row[0] + ": auto_nav_block");
            }
            CollectionAssert.AreEqual(new[] { "Long read", "Rich point", "One hand", "Over the camera" },
                NavigationPresets.All.Take(NavigationPresets.Recommended).Select(p => p.Label), "the four strongest come first, above the separator");
            foreach (var preset in NavigationPresets.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(preset.Suits), preset.Label + " says who it suits");
                Assert.Less(preset.Suits.Length, 120, preset.Label + ": a few words, not a paragraph");
                EditorTextChecks.AssertAscii(preset.Label + preset.Suits, preset.Label);
            }
            Assert.AreEqual(NavigationPresets.All.Count, NavigationPresets.All.Select(p => p.Label).Distinct().Count(), "no two presets share a name");
        }

        [Test]
        public void EveryElementAPresetNames_IsRegisteredForTheSlotItFills_AndEveryLayoutIsOffered()
        {
            foreach (var preset in NavigationPresets.All)
            {
                foreach (var (slot, key) in new[]
                         {
                             (NavSlotKeys.Top, preset.Top), (NavSlotKeys.Bottom, preset.Bottom), (NavSlotKeys.Floating, preset.Floating),
                             (NavSlotKeys.Menu, preset.Menu), (NavSlotKeys.Edge, preset.Edge),
                         })
                {
                    if (key == CardOptions.NavNone) continue;
                    Assert.IsTrue(NavElementRegistry.Shared.TryGet(key, out var element), preset.Label + ": '" + key + "' is a registered element");
                    Assert.AreEqual(slot, element.Slot, preset.Label + ": " + element.Label + " is an element of the " + slot + " slot");
                }
                CollectionAssert.Contains(CardOptions.Layouts, preset.Layout, preset.Label + ": its layout is one the Layout row offers");
                if (preset.AutoNavBlock != CardOptions.NavNone)
                {
                    Assert.IsTrue(BlockRegistry.Shared.TryGet(preset.AutoNavBlock, out var kind), preset.Label + ": its Auto Nav Block is a registered kind");
                    Assert.AreEqual(BuiltInFamilies.Navigation, kind.Family, preset.Label + ": and a kind of the navigation family");
                    Assert.IsFalse(kind.NeedsAuthoredContent, preset.Label + ": that the card can put in by itself");
                }
            }
        }

        [Test]
        public void TheFieldsAPresetWrites_AreRealFieldsOfTheSettings_AndLongReadIsTheFrameworkDefault()
        {
            foreach (string name in NavigationPresets.WrittenFields)
                Assert.IsNotNull(typeof(CardNavigationSettings).GetField(name, PublicFields), name + " is a field of card_settings.navigation");
            Assert.AreEqual(7, NavigationPresets.WrittenFields.Count);
            Assert.AreSame(NavigationPresets.All[0], NavigationPresets.Matching(new CardNavigationSettings()),
                "section 8 calls Long read 'default fields': a wall that never set the seven rows IS on Long read");
        }

        [Test]
        public void ApplyingAPreset_WritesExactlyItsSevenFields_AndNothingElse_OnACardWhoseOtherRowsWereAllChanged()
        {
            foreach (var preset in NavigationPresets.All)
            {
                var nav = BusyNavigation();
                var before = Snapshot(nav);
                // - the precondition: every field the preset does NOT write really was changed from its default, lists included
                var defaults = Snapshot(new CardNavigationSettings());
                foreach (var field in typeof(CardNavigationSettings).GetFields(PublicFields))
                    if (!NavigationPresets.WrittenFields.Contains(field.Name))
                        Assert.AreNotEqual(defaults[field.Name], before[field.Name], "precondition: '" + field.Name + "' was changed before " + preset.Label);

                NavigationPresets.Apply(nav, preset);
                var after = Snapshot(nav);

                foreach (var field in typeof(CardNavigationSettings).GetFields(PublicFields))
                {
                    bool written = NavigationPresets.WrittenFields.Contains(field.Name);
                    if (!written)
                        Assert.AreEqual(before[field.Name], after[field.Name], preset.Label + ": '" + field.Name + "' is not a preset field and must not move");
                }
                Assert.AreEqual(preset.Layout, nav.layout, preset.Label);
                Assert.AreEqual(preset.Top, nav.top, preset.Label);
                Assert.AreEqual(preset.Bottom, nav.bottom, preset.Label);
                Assert.AreEqual(preset.Floating, nav.floating, preset.Label);
                Assert.AreEqual(preset.Menu, nav.menu, preset.Label);
                Assert.AreEqual(preset.Edge, nav.edge, preset.Label);
                Assert.AreEqual(preset.AutoNavBlock, nav.auto_nav_block, preset.Label);
                // - twice is the same as once
                NavigationPresets.Apply(nav, preset);
                CollectionAssert.AreEquivalent(after, Snapshot(nav), preset.Label + ": applying it again changes nothing");
            }
        }

        [Test]
        public void Matching_NamesThePresetTheSevenRowsHold_AndNullOnceAnyOfThemIsChanged()
        {
            Assert.IsNull(NavigationPresets.Matching(null));
            foreach (var preset in NavigationPresets.All)
            {
                var nav = BusyNavigation();
                NavigationPresets.Apply(nav, preset);
                Assert.AreSame(preset, NavigationPresets.Matching(nav), preset.Label + " is recognised, whatever the other rows hold");
                foreach (string name in NavigationPresets.WrittenFields)
                {
                    var changed = BusyNavigation();
                    NavigationPresets.Apply(changed, preset);
                    typeof(CardNavigationSettings).GetField(name, PublicFields).SetValue(changed, "something_else");
                    Assert.IsNull(NavigationPresets.Matching(changed), preset.Label + " with '" + name + "' changed is Custom");
                }
            }
        }

        // ---------------- helpers ----------------

        // A settings object whose every field is away from its default, so a preset that touched an extra one shows
        internal static CardNavigationSettings BusyNavigation()
        {
            var nav = new CardNavigationSettings();
            foreach (var field in typeof(CardNavigationSettings).GetFields(PublicFields))
            {
                if (field.FieldType == typeof(string)) field.SetValue(nav, "changed_" + field.Name);
                else if (field.FieldType == typeof(bool)) field.SetValue(nav, !(bool)field.GetValue(nav));
                else if (field.FieldType == typeof(int)) field.SetValue(nav, (int)field.GetValue(nav) + 1);
            }
            nav.families.Add(new CardFamilySetting { family = "media", order = 7, at_end = true, group = "watch" });
            nav.groups.Add(new CardNavGroup { key = "group_1", label = new List<LocalizedEntry> { new() { lang = "en", value = "Mine" } } });
            // - the Navigation Bar (_3.3 S4a review): no preset writes it
            nav.bar.enabled = true;
            nav.bar.indicator = CardOptions.NavBarIndicatorRaised;
            // - the seven rows then hold something no preset writes, so Apply is the only thing that can move them
            return nav;
        }

        // Every public field as a comparable string: a value by its text, a list by the JSON of each of its elements
        internal static Dictionary<string, string> Snapshot(CardNavigationSettings nav)
        {
            var map = new Dictionary<string, string>();
            foreach (var field in typeof(CardNavigationSettings).GetFields(PublicFields))
            {
                object value = field.GetValue(nav);
                // - a list by its rows, a settings object (the Navigation Bar) by its JSON, a value by itself
                map[field.Name] = value is IEnumerable list && !(value is string)
                    ? "[" + string.Join("|", list.Cast<object>().Select(JsonUtility.ToJson)) + "]"
                    : value != null && field.FieldType.IsClass && !(value is string) ? JsonUtility.ToJson(value)
                    : value?.ToString() ?? "null";
            }
            return map;
        }
    }
}
