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
    // Detail Card > Locate Button (_3.3 P3d review b) on the REAL window (PoiEditorWindowHost): its rows in the brief's order (Style, Colour,
    // Size, Float), each with its (i), right after Navigation, at their defaults; drawing writes nothing (an old config with no locate block,
    // an out-of-range Size); a Style / Colour pick writes its field only, one undo step; a REAL drag of Size and a REAL click on Float, one
    // undo step each; the help texts ASCII
    public class CardLocateEditorTabTests
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

        private void Open(WallConfigData config = null)
        {
            _window = new PoiEditorWindowHost(config ?? ShippedWall.WithFrameworkNavigation(), "_showCardNavigation");
            _window.SetWindowField("_showCardLocate", true);
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
        private float Row(string title) => _rows.Last(r => r.Title == title).Rect.y;

        private void Pick(Action edit) =>
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { edit, false });

        [UnityTest]
        public IEnumerator TheComponent_ItsRowsInOrder_AfterNavigation_AtTheDefaults_AndDrawingWritesNothing()
        {
            Open();
            yield return Draw();
            Assert.IsFalse(_window.Unsaved, "drawing writes nothing");
            var rows = POIEditorToolWindow.CardLocateRows;
            CollectionAssert.AreEqual(new[] { "Style", "Colour", "Size", "Float" }, rows, "the brief's rows");
            for (int i = 0; i < rows.Length; i++)
            {
                CollectionAssert.Contains(Titles, rows[i], "the " + rows[i] + " row, with its (i)");
                if (i > 0) Assert.Less(Row(rows[i - 1]), Row(rows[i]), rows[i - 1] + " before " + rows[i]);
            }
            Assert.Less(Row("Locate Button"), Row("Style"), "after Navigation (its Navigation Bar's Locate Button switch above)");
            Assert.AreEqual(CardOptions.LocatePinLabel, LocateFabRule.Style(Card.locate), "Pin + Label by default");
            Assert.AreEqual(CardOptions.LocateTeal, LocateFabRule.Colour(Card.locate), "AR Teal by default");
            Assert.AreEqual(CardLocateSettings.SizeDefault, LocateFabRule.Size(Card.locate), "Size's default");
            Assert.IsTrue(Card.locate.float_motion, "Float on");

            // - an old config (no locate block) and a Size past the range are drawn as the card reads them, never rewritten by a draw
            _window.Close();
            var old = ShippedWall.WithFrameworkNavigation();
            old.card_settings.locate = JsonUtility.FromJson<CardSettings>("{}").locate;
            old.card_settings.locate.size = 7f;
            Open(old);
            yield return Draw();
            Assert.IsFalse(_window.Unsaved, "drawing an out-of-range Size writes nothing");
            Assert.AreEqual(7f, Card.locate.size, "...the stored value kept");

            foreach (var text in new[] { "CardLocateOffNote", "CardLocateStyleHelp", "CardLocateColourHelp", "CardLocateSizeHelp", "CardLocateFloatHelp",
                "CardLocateSceneTestGuide", "CardLocatePlaymodeTestGuide", "CardLocateDeviceTestGuide" })
            {
                var field = typeof(POIEditorToolWindow).GetField(text, BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(field, text + " exists");
                EditorTextChecks.AssertAscii((string)field.GetValue(null), text);
            }
        }

        [UnityTest]
        public IEnumerator EveryPick_WritesItsField_OneUndoStep_ARealSizeDragAndARealFloatClickToo()
        {
            Open();
            yield return Draw();
            foreach (var (row, edit, took) in new (string, Action, Func<bool>)[]
            {
                ("Style", () => Card.locate.style = CardOptions.LocateGlassPing, () => LocateFabRule.Style(Card.locate) == CardOptions.LocateGlassPing),
                ("Colour", () => Card.locate.colour = CardOptions.LocateCyan, () => LocateFabRule.Colour(Card.locate) == CardOptions.LocateCyan),
            })
            {
                string before = JsonUtility.ToJson(Card.locate);
                int steps = _window.UndoStepsLeft;
                Pick(edit);
                yield return Draw();
                Assert.IsTrue(took(), row + ": the pick is what the card reads");
                Assert.AreEqual(steps + 1, _window.UndoStepsLeft, row + ": one undo step");
                yield return _window.PressUndo();
                Assert.AreEqual(before, JsonUtility.ToJson(Card.locate), row + ": one Ctrl+Z takes it back");
            }

            // - a real drag along Size's track (press, moves, release inside one editor tick): one undo step
            yield return _window.ScrollTo("Size");
            yield return Draw();
            int sizeSteps = _window.UndoStepsLeft;
            Rect track = _window.RectOf("Size");
            float y = track.center.y, startX = track.xMax - 90f;
            _window.Send(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX, y)) });
            _window.PaintNow();
            for (int i = 1; i <= 6; i++)
            {
                _window.Send(new Event { type = EventType.MouseDrag, button = 0, mousePosition = _window.Local(new Vector2(startX + 12f * i, y)), delta = new Vector2(12f, 0f) });
                _window.PaintNow();
            }
            _window.Send(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX + 72f, y)) });
            _window.PaintNow();
            yield return _window.WaitForRepaint();
            float size = LocateFabRule.Size(Card.locate);
            Assert.Greater(size, CardLocateSettings.SizeDefault + 0.05f, "the drag grew the pin (" + size + ")");
            Assert.LessOrEqual(size, CardLocateSettings.SizeMax, "...inside its range");
            Assert.AreEqual(sizeSteps + 1, _window.UndoStepsLeft, "a whole drag = one undo step");
            yield return _window.PressUndo();
            Assert.AreEqual(CardLocateSettings.SizeDefault, LocateFabRule.Size(Card.locate), 1e-4f, "one Ctrl+Z: the default again");

            // - a real click on Float's checkbox: off, one undo step
            yield return _window.ScrollTo("Float");
            int floatSteps = _window.UndoStepsLeft;
            _window.Click("Float");
            yield return _window.WaitForRepaint();
            Assert.IsFalse(Card.locate.float_motion, "a real click turned Float off");
            Assert.AreEqual(floatSteps + 1, _window.UndoStepsLeft, "one undo step");
            yield return _window.PressUndo();
            Assert.IsTrue(Card.locate.float_motion, "one Ctrl+Z: on again");
        }
    }
}
