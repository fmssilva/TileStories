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
    // Detail Card > Sections (_3.3 fourth round, v6's Tweaks "Sections") on the REAL window (PoiEditorWindowHost): its rows are v6's in v6's
    // order, beside Card Container (right after it); drawing writes nothing (an old config, an out-of-range Intensity); each pick writes its
    // row only, one undo step; Divider Image's row only under Divider = Image; Card Container's Blocks row; and an opened Bar Group's Section
    // Style (the card's Start named in its blank) writes only that group's row
    public class CardSectionsEditorTabTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private PoiEditorWindowHost _window;
        private readonly List<(string Title, Rect Rect)> _rows = new();

        [TearDown]
        public void TearDown()
        {
            HelpInfoButton.DrawnProbe = null;
            _window?.Close();
        }

        private CardSettings Card => _window.Config.card_settings;

        private void Open(params string[] foldouts)
        {
            _window = new PoiEditorWindowHost(ShippedWall.WithFrameworkNavigation(), foldouts[0]);
            foreach (string more in foldouts.Skip(1)) _window.SetWindowField(more, true);
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "DetailCard"));
        }

        private IEnumerator Draw()
        {
            _rows.Clear();
            HelpInfoButton.DrawnProbe = (title, rect) => _rows.Add((title, rect));
            try { yield return _window.WaitForRepaint(); }
            finally { HelpInfoButton.DrawnProbe = null; }
        }

        private List<string> Titles => _rows.Select(r => r.Title).ToList();
        private float Row(string title) => _rows.First(r => r.Title == title).Rect.y;

        private void Pick(Action edit) =>
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { edit, false });

        [UnityTest]
        public IEnumerator TheComponent_ItsRowsAreV6s_InOrder_AtV6sDefaults_AndDrawingWritesNothing()
        {
            Open("_showCardContainer", "_showCardSections");
            yield return Draw();
            Assert.IsFalse(_window.Unsaved, "drawing writes nothing");
            var rows = POIEditorToolWindow.CardSectionsRows;
            CollectionAssert.AreEqual(new[] { "Start", "Divider", "Colour", "Intensity", "Tint Depth", "Motion", "Replay", "Vary" }, rows,
                "v6's Tweaks \"Sections\" + the P3c review's Tint Depth and Replay");
            for (int i = 0; i < rows.Length; i++)
            {
                CollectionAssert.Contains(Titles, rows[i], "the " + rows[i] + " row");
                if (i > 0) Assert.Less(Row(rows[i - 1]), Row(rows[i]), rows[i - 1] + " before " + rows[i]);
            }
            Assert.Less(Row("Blocks"), Row("Start"), "beside Card Container: right after it (Card Container's Blocks row above)");
            CollectionAssert.DoesNotContain(Titles, "Divider Image", "only under Divider = Image");
            Assert.AreEqual(CardOptions.SectionStartTiles, SectionStyleRule.Start(Card.sections), "v6's defaults: Tiles");
            Assert.AreEqual(CardOptions.BlocksOpen, Card.container.blocks, "Blocks Open");

            // - an Intensity past the cap (v6's 0.30) is shown at the cap and never rewritten by a draw
            Pick(() => Card.sections.intensity = 0.3f);
            _window.SetWindowField("_hasUnsavedChanges", false);
            yield return Draw();
            Assert.IsFalse(_window.Unsaved, "drawing an out-of-range value writes nothing");
            Assert.AreEqual(0.3f, Card.sections.intensity, 1e-6f);
        }

        [UnityTest]
        public IEnumerator EveryPick_WritesItsRowOnly_OneUndoStep_AndDividerImageShowsUnderImage()
        {
            Open("_showCardSections");
            yield return Draw();
            string before = JsonUtility.ToJson(Card.sections);
            Pick(() => Card.sections.divider = CardOptions.SectionDividerImage);
            yield return Draw();
            CollectionAssert.Contains(Titles, "Divider Image", "Divider = Image shows the strip picture's row");
            var picked = JsonUtility.FromJson<CardSectionSettings>(before);
            picked.divider = CardOptions.SectionDividerImage;
            Assert.AreEqual(JsonUtility.ToJson(picked), JsonUtility.ToJson(Card.sections), "only Divider changed");
            yield return _window.PressUndo();
            Assert.AreEqual(before, JsonUtility.ToJson(Card.sections), "one real Ctrl+Z takes it back");
        }

        // P3d: Tint Depth a slider right after Intensity -- a REAL drag along its track (press, moves, release in one editor tick) writes the
        // depth the card reads, as ONE undo step; Replay right after Motion, only while the tiles turn (Reveal, Drift), a pick one undo step
        [UnityTest]
        public IEnumerator TintDepth_ARealDragWritesIt_OneUndoStep_AndReplayShowsOnlyWhereTheTilesTurn()
        {
            Open("_showCardSections");
            yield return Draw();
            Assert.IsFalse(_window.Unsaved, "drawing writes nothing");
            Assert.Less(Row("Intensity"), Row("Tint Depth"), "Tint Depth right after Intensity");
            Assert.Less(Row("Tint Depth"), Row("Motion"));
            Assert.Less(Row("Motion"), Row("Replay"), "Replay right after Motion");
            Assert.Less(Row("Replay"), Row("Vary"));
            Assert.AreEqual(120f, SectionStyleRule.TintDepth(Card.sections), "precondition: the default depth");

            int steps = _window.UndoStepsLeft;
            Rect row = _window.RectOf("Tint Depth");
            float y = row.center.y, startX = row.xMax - 70f;
            _window.Send(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX, y)) });
            _window.PaintNow();
            for (int i = 1; i <= 6; i++)
            {
                _window.Send(new Event { type = EventType.MouseDrag, button = 0, mousePosition = _window.Local(new Vector2(startX + 20f * i, y)), delta = new Vector2(20f, 0f) });
                _window.PaintNow();
            }
            _window.Send(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX + 120f, y)) });
            _window.PaintNow();
            yield return _window.WaitForRepaint();
            float dragged = SectionStyleRule.TintDepth(Card.sections);
            Assert.Greater(dragged, 120f, "the drag deepened the tint");
            Assert.LessOrEqual(dragged, CardSectionSettings.TintDepthMax, "...inside its range");
            Assert.AreEqual(dragged, Card.sections.tint_depth, 1e-4f, "the card reads what the slider wrote");
            Assert.AreEqual(steps + 1, _window.UndoStepsLeft, "a whole drag = one undo step");
            yield return _window.PressUndo();
            Assert.AreEqual(120f, SectionStyleRule.TintDepth(Card.sections), 1e-4f, "one Ctrl+Z puts 120 back");

            steps = _window.UndoStepsLeft;
            Pick(() => Card.sections.replay = CardOptions.SectionReplayOnce);
            yield return Draw();
            Assert.AreEqual(CardOptions.SectionReplayOnce, SectionStyleRule.Replay(Card.sections), "Replay Once written");
            Assert.AreEqual(steps + 1, _window.UndoStepsLeft, "one undo step");
            Pick(() => Card.sections.motion = CardOptions.SectionMotionStill);
            yield return Draw();
            CollectionAssert.DoesNotContain(Titles, "Replay", "under Still nothing turns: no Replay row");
            Pick(() => Card.sections.motion = CardOptions.SectionMotionDrift);
            yield return Draw();
            CollectionAssert.Contains(Titles, "Replay", "Drift turns the tiles in too: the row is back");
        }

        [UnityTest]
        public IEnumerator AnOpenedBarGroup_HasASectionStyleRow_TheCardsStartInItsBlank_AndAPickWritesOnlyThatGroup()
        {
            Open("_showCardNavigation");
            var key = NavBarRule.DefaultRows(Card.navigation, BlockRegistry.Shared)[1].key;
            ((HashSet<string>)typeof(POIEditorToolWindow).GetField("_openBarGroups", Instance).GetValue(_window.Editor)).Add(key);
            yield return Draw();
            CollectionAssert.Contains(Titles, "Section Style", "the opened group's Section Style");
            Assert.Greater(Row("Section Style"), Row("Families"), "after Families");
            Assert.IsFalse(_window.Unsaved, "drawing the six default groups writes nothing");
            Pick(() => POIEditorToolWindow.MaterializeBarItems(Card.navigation)[1].section_style = CardOptions.SectionStartWave);
            var items = Card.navigation.bar.items;
            Assert.AreEqual(CardOptions.SectionStartWave, items[1].section_style, "the group's own Start");
            Assert.IsTrue(items.Where((_, i) => i != 1).All(i => i.section_style == ""), "no other group changed");
        }
    }
}
