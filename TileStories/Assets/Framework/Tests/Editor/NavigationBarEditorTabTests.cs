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
    // Detail Card > Navigation > Navigation Bar (_3.3 S4 third round, v5) on the REAL window (PoiEditorWindowHost): the component first in
    // the section, ON by default, its rows v5's Tweaks -- same names, same order -- above the Preset, the Elements rows replaced by a notice
    // (and back when it is turned off); an old config drawn is never rewritten; the Bar Groups table showing the six default groups without
    // writing, its first real click writing them all as ONE undo step; real clicks moving, hiding, adding and deleting a group, real typing
    // of a group's name; the warnings read from what the window draws, naming things, never keys.
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

        // Unfold a Bar Groups row (the window's own session view state, never saved): its Name, Icon, Colour and Families rows draw
        private void Unfold(string key) =>
            ((HashSet<string>)typeof(POIEditorToolWindow).GetField("_openBarGroups", Instance).GetValue(_window.Editor)).Add(key);

        [UnityTest]
        public IEnumerator TheComponent_IsFirst_OnByDefault_ItsRowsAreV5s_InOrder_AndOff_TheElementsComeBack_OneUndoStep()
        {
            OpenNavigation();
            yield return Draw();
            Assert.IsFalse(_window.Unsaved, "drawing writes nothing");
            Assert.IsTrue(Nav.bar.enabled, "on by default");
            Assert.Less(Row("Navigation Bar"), Row("Preset"), "the Navigation Bar is the section's first component");
            // - every row of v5's Tweaks "Navigation Bar", by its name, in its order, inside the component (above the Preset)
            var rows = POIEditorToolWindow.NavigationBarRows;
            for (int i = 0; i < rows.Length; i++)
            {
                CollectionAssert.Contains(RowTitles, rows[i], "the " + rows[i] + " row");
                Assert.Less(Row(rows[i]), Row("Preset"), rows[i] + " sits in the component");
                if (i > 0) Assert.Less(Row(rows[i - 1]), Row(rows[i]), rows[i - 1] + " comes before " + rows[i] + " (v5's order)");
            }
            Assert.Less(Row(rows[rows.Length - 1]), Row("Bar Groups"), "then the Bar Groups table");
            CollectionAssert.Contains(_groups.Select(g => g.Title).ToList(), POIEditorToolWindow.NavigationBarGroupsTitle, "under its sub-heading");
            foreach (string gone in new[] { "Corner Radius", "Side Margin", "Height", "Fill Opacity", "Border", "Shadow", "Slide Indicator",
                "Enlarge Chosen Icon", "Other Groups' Colour" })
                CollectionAssert.DoesNotContain(RowTitles, gone, gone + ": not one of v5's options");
            foreach (string gone in new[] { "Top Bar", "Bottom Bar", "Menu", "Floating Button", "Edge" })
                CollectionAssert.DoesNotContain(RowTitles, gone, "on: " + gone + " is replaced by the bar");
            CollectionAssert.Contains(_messages, Text("CardNavBarOnNotice"), "...and the notice says so");

            Pick(() => Nav.bar.enabled = false);
            yield return Draw();
            CollectionAssert.DoesNotContain(RowTitles, "Position", "off: no bar rows");
            CollectionAssert.Contains(RowTitles, "Top Bar", "off: the Elements rows come back");
            yield return _window.PressUndo();
            Assert.IsTrue(Nav.bar.enabled, "one real Ctrl+Z turns it on again");
        }

        [UnityTest]
        public IEnumerator AnOldConfig_DrawsItsUnderTitleAsTop_AndDrawingNeverRewritesIt_APickWritesOnlyItsRow()
        {
            OpenNavigation();
            // - the old values put in through the window's own edit path (its history knows them), then left to the drawing alone
            Pick(() =>
            {
                Nav.bar.position = CardOptions.NavBarUnderTitle;
                Nav.bar.hide_on_scroll = "anything";
            });
            yield return Draw();
            string before = JsonUtility.ToJson(Nav.bar);
            int stepsBefore = _window.UndoStepsLeft;
            yield return Draw();
            yield return Draw();
            Assert.AreEqual(before, JsonUtility.ToJson(Nav.bar), "two draws of an old bar write nothing (under_title stays as written)");
            Assert.AreEqual(stepsBefore, _window.UndoStepsLeft, "...and add no undo step");
            // - each option written through the window's own edit path, one undo step each, read back as the card reads it
            var picks = new (string Row, System.Action Edit, System.Func<bool> Took)[]
            {
                ("Position", () => Nav.bar.position = CardOptions.NavBarTop, () => !NavBarRule.AtBottom(Nav.bar)),
                ("Shape", () => Nav.bar.shape = CardOptions.NavBarFullWidth, () => !NavBarRule.Floating(Nav.bar)),
                ("Fill", () => Nav.bar.fill = CardOptions.NavBarClear, () => NavBarRule.Fill(Nav.bar) == CardOptions.NavBarClear),
                ("Indicator", () => Nav.bar.indicator = CardOptions.NavBarIndicatorUnderline,
                    () => NavBarRule.Indicator(Nav.bar) == CardOptions.NavBarIndicatorUnderline),
                ("Chosen Icon", () => Nav.bar.chosen_icon = CardOptions.NavBarChosenIconPlain,
                    () => NavBarRule.ChosenIcon(Nav.bar) == CardOptions.NavBarChosenIconPlain),
                ("Labels", () => Nav.bar.labels = CardOptions.NavBarLabelsIcon, () => NavBarRule.Labels(Nav.bar) == CardOptions.NavBarLabelsIcon),
                ("Chosen Colour", () => Nav.bar.selected_color = CardOptions.NavBarColourInk,
                    () => NavBarRule.SelectedColour(Nav.bar) == CardOptions.NavBarColourInk),
                ("Bold Chosen Icon", () => Nav.bar.bold_selected = false, () => !NavBarRule.BoldChosen(Nav.bar)),
                ("Show", () => Nav.bar.show = CardOptions.NavBarShowAfterHero, () => NavBarRule.Show(Nav.bar) == CardOptions.NavBarShowAfterHero),
                ("Hide While Scrolling", () => Nav.bar.hide_on_scroll = CardOptions.NavBarHideDown, () => NavBarRule.HidesWhileScrolling(Nav.bar)),
            };
            foreach (var pick in picks)
            {
                int steps = _window.UndoStepsLeft;
                Pick(pick.Edit);
                yield return Draw();
                Assert.IsTrue(pick.Took(), pick.Row + ": the pick is what the card reads");
                Assert.AreEqual(steps + 1, _window.UndoStepsLeft, pick.Row + ": one undo step");
                CollectionAssert.Contains(RowTitles, pick.Row, pick.Row + " still drawn after the pick");
            }
        }

        [UnityTest]
        public IEnumerator TheGroupsTable_ShowsTheSixGroups_WithoutWriting_AndTheFirstRealClick_WritesThemAll_OneUndoStep()
        {
            OpenNavigation();
            yield return Draw();
            var defaults = NavBarRule.DefaultRows(Nav, BlockRegistry.Shared);
            Assert.AreEqual(0, Nav.bar.items.Count, "drawing the table writes no row");
            Assert.AreEqual(6, defaults.Count(d => d.families.Any(f => BlockRegistry.Shared.TryGetFamily(f, out var fam) && fam.InNav)),
                "the framework's six groups");
            Assert.IsFalse(_window.ScreenRects.ContainsKey("Bar group shown#" + defaults.Count), "one row per group, no more");
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

        [UnityTest]
        public IEnumerator RealTyping_NamesAGroup_ARealClickDeletesOne_EachOneUndoStep_TheMessagesNameNoKey()
        {
            OpenNavigation();
            yield return Draw();
            var defaults = NavBarRule.DefaultRows(Nav, BlockRegistry.Shared);
            string language = _window.Config.card_settings.languages.Count > 0 ? _window.Config.card_settings.languages[0] : "en";
            Unfold(defaults[0].key);
            yield return Draw();
            string nameKey = "Bar group name " + language + "#0";
            Assert.IsTrue(_window.ScreenRects.ContainsKey(nameKey), "an unfolded group shows its Name per language");
            CollectionAssert.Contains(RowTitles, "Icon", "...its Icon");
            CollectionAssert.Contains(RowTitles, "Chosen Icon", "...its Chosen Icon");
            CollectionAssert.Contains(RowTitles, "Colour", "...its Colour");
            CollectionAssert.Contains(RowTitles, "Families", "...its Families");

            int steps = _window.UndoStepsLeft;
            yield return _window.ScrollTo(nameKey);
            yield return _window.ReplaceText(nameKey, "Overview");
            yield return _window.ClickAway();
            yield return Draw();
            Assert.AreEqual("Overview", Nav.bar.items[0].label.Single(l => l.lang == language).value, "real typing names the group");
            Assert.AreEqual(defaults.Count, Nav.bar.items.Count, "the first edit writes every default row, no more");
            Assert.AreEqual(steps + 1, _window.UndoStepsLeft, "typing a name is one undo step");

            steps = _window.UndoStepsLeft;
            yield return _window.ScrollTo("Bar group delete#" + (defaults.Count - 1));
            _window.Click("Bar group delete#" + (defaults.Count - 1));
            yield return Draw();
            Assert.AreEqual(defaults.Count - 1, Nav.bar.items.Count, "a real click on the trash removes the group");
            Assert.AreEqual(steps + 1, _window.UndoStepsLeft, "one undo step");
            BlockRegistry.Shared.TryGetFamily(defaults[defaults.Count - 1].families[0], out var lastFamily);
            string words = lastFamily == null ? null : (string)typeof(POIEditorToolWindow).GetMethod("FamilyWords", Instance).Invoke(_window.Editor, new object[] { lastFamily });
            var warning = _messages.FirstOrDefault(m => m.StartsWith("In no shown group:"));
            Assert.IsNotNull(warning, "the removed group's family can no longer be reached: the warning says so");
            foreach (string family in defaults[defaults.Count - 1].families)
                StringAssert.DoesNotContain(" " + family + ",", warning + ",", "the warning names the family by its words, never its key " + family);
            if (!string.IsNullOrEmpty(words)) StringAssert.Contains(words, warning, "...by the visitor's word");

            yield return _window.PressUndo();
            Assert.AreEqual(defaults.Count, Nav.bar.items.Count, "Ctrl+Z gives the group back");
        }
    }
}
