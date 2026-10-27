using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Detail Card > Navigation > Navigation Bar (_3.3 S4a review) on the REAL window (PoiEditorWindowHost): the component first in the
    // section, off by default; on, its four sub-components above the Preset and the Elements rows replaced by a notice; the Bar Groups table
    // showing one group per family without writing, its first real click writing them all as ONE undo step; a real click moving and hiding
    // a group; the warnings read from what the window draws.
    public class NavigationBarEditorTabTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;
        private PoiEditorWindowHost _window;
        private readonly List<string> _messages = new();
        private readonly List<(string Title, Rect Rect)> _rows = new();
        private readonly List<(string Title, Rect Rect)> _groups = new();

        [TearDown]
        public void TearDown()
        {
            POIEditorToolWindow.NavigationMessageProbe = null;
            POIEditorToolWindow.NavigationGroupProbe = null;
            HelpInfoButton.DrawnProbe = null;
            _window?.Close();
        }

        private CardNavigationSettings Nav => _window.Config.card_settings.navigation;

        private void OpenNavigation()
        {
            _window = new PoiEditorWindowHost(ShippedWall.WithFrameworkNavigation(), "_showCardNavigation");
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "DetailCard"));
        }

        private void Pick(Action edit) =>
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { edit, false });

        private IEnumerator Draw()
        {
            _messages.Clear();
            _rows.Clear();
            _groups.Clear();
            POIEditorToolWindow.NavigationMessageProbe = _messages.Add;
            HelpInfoButton.DrawnProbe = (title, rect) => _rows.Add((title, rect));
            POIEditorToolWindow.NavigationGroupProbe = (title, rect) => _groups.Add((title, rect));
            try { yield return _window.WaitForRepaint(); }
            finally
            {
                POIEditorToolWindow.NavigationMessageProbe = null;
                HelpInfoButton.DrawnProbe = null;
                POIEditorToolWindow.NavigationGroupProbe = null;
            }
        }

        private List<string> RowTitles => _rows.Select(r => r.Title).ToList();
        private float Row(string title) => _rows.First(r => r.Title == title).Rect.y;
        private static string Text(string field) => (string)typeof(POIEditorToolWindow).GetField(field, Static).GetValue(null);

        [UnityTest]
        public IEnumerator TheComponent_IsFirst_OffByDefault_AndOn_ItsFourSubComponentsReplaceTheElements_OneUndoStep()
        {
            OpenNavigation();
            yield return Draw();
            Assert.IsFalse(_window.Unsaved, "drawing writes nothing");
            Assert.Less(Row("Navigation Bar"), Row("Preset"), "the Navigation Bar is the section's first component");
            Assert.Less(Row("Enabled"), Row("Preset"));
            CollectionAssert.IsEmpty(_groups.Select(g => g.Title).Intersect(POIEditorToolWindow.NavigationBarSubTitles).ToList(), "off: no sub-components");
            CollectionAssert.Contains(RowTitles, "Top Bar", "off: the Elements rows as before");

            Pick(() => Nav.bar.enabled = true);
            yield return Draw();
            var subs = _groups.Where(g => POIEditorToolWindow.NavigationBarSubTitles.Contains(g.Title)).ToList();
            CollectionAssert.AreEqual(POIEditorToolWindow.NavigationBarSubTitles, subs.Select(g => g.Title).ToList(), "its four sub-components, in order");
            foreach (var sub in subs) Assert.Less(sub.Rect.y, Row("Preset"), sub.Title + " sits in the component, above the Preset");
            foreach (string row in new[] { "Shape", "Fill", "Indicator", "Labels", "Position", "Show", "Bar Groups" })
                CollectionAssert.Contains(RowTitles, row, "the " + row + " row");
            foreach (string gone in new[] { "Top Bar", "Bottom Bar", "Menu", "Floating Button", "Edge" })
                CollectionAssert.DoesNotContain(RowTitles, gone, "on: " + gone + " is replaced by the bar");
            CollectionAssert.Contains(_messages, Text("CardNavBarOnNotice"), "...and the notice says so");

            yield return _window.PressUndo();
            Assert.IsFalse(Nav.bar.enabled, "one real Ctrl+Z turns it off again");
        }

        [UnityTest]
        public IEnumerator TheGroupsTable_ShowsOneGroupPerFamily_WithoutWriting_AndTheFirstRealClick_WritesThemAll_OneUndoStep()
        {
            OpenNavigation();
            Pick(() => Nav.bar.enabled = true);
            yield return Draw();
            var defaults = NavBarRule.DefaultRows(Nav, BlockRegistry.Shared);
            Assert.AreEqual(0, Nav.bar.items.Count, "drawing the table writes no row");
            for (int i = 0; i < defaults.Count; i++)
                Assert.IsTrue(_window.ScreenRects.ContainsKey("Bar group shown#" + i), "the default group " + defaults[i].key + " has its row");

            yield return _window.ScrollTo("Bar group add#0");
            _window.Click("Bar group add#0");
            yield return _window.WaitForRepaint();
            Assert.AreEqual(defaults.Count + 1, Nav.bar.items.Count, "+ Add group writes the defaults and the new group");
            CollectionAssert.AreEqual(defaults.Select(d => d.key).ToList(), Nav.bar.items.Take(defaults.Count).Select(r => r.key).ToList(),
                "the defaults kept as they were shown");
            StringAssert.StartsWith(TaxonomyRowKeys.GroupPrefix, Nav.bar.items.Last().key, "the new group's generated key");
            yield return _window.PressUndo();
            Assert.AreEqual(0, Nav.bar.items.Count, "one real Ctrl+Z: back to no rows (the defaults again)");
        }

        [UnityTest]
        public IEnumerator ARealClick_MovesAGroupDown_AndHidesOne_AndTheWarningNamesTheFamilyLeftOut()
        {
            OpenNavigation();
            Pick(() => Nav.bar.enabled = true);
            yield return Draw();
            var defaults = NavBarRule.DefaultRows(Nav, BlockRegistry.Shared);
            yield return _window.ScrollTo("Bar group down#0");
            _window.Click("Bar group down#0");
            yield return _window.WaitForRepaint();
            Assert.AreEqual(defaults[1].key, Nav.bar.items[0].key, "the first group moved down");
            Assert.AreEqual(defaults[0].key, Nav.bar.items[1].key);

            yield return _window.ScrollTo("Bar group shown#0");
            _window.Click("Bar group shown#0");
            yield return Draw();
            Assert.IsFalse(Nav.bar.items[0].shown, "a real click on Shown hides the group");
            Assert.IsFalse(NavBarRule.Items(Nav, BlockRegistry.Shared).Any(i => i.Key == defaults[1].key), "...and the bar leaves it out");
            BlockRegistry.Shared.TryGetFamily(defaults[1].key, out var family);
            Assert.IsTrue(_messages.Any(m => m.StartsWith("In no shown group:")), "the warning says a family can no longer be reached ("
                + string.Join(" | ", _messages) + ")");
            Assert.IsNotNull(family, "precondition: the hidden group is a family's default");
        }
    }
}
