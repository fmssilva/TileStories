using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // The POI Editor's own undo (Ctrl+Z / Ctrl+Y) records one step per GESTURE: typing a word into one cell,
    // or dragging one slider, is one step; a new gesture (focus left and came back, another control) is a new
    // step. Driven for real on the REAL window: real clicks, real key events, a real mouse drag.
    public class ConfigUndoGroupingTests
    {
        private PoiEditorWindowHost _window;

        private static WallConfigData BadgeConfig() => new WallConfigData
        {
            pois = new List<POIData>(),
            marker_shape = "circle",
            marker_use_badge = true,
            badge_size_ratio = 0.36f,
            marker_outline_mode = "uniform",
            effect_defaults = new EffectDefaults(),
            hierarchy_levels = new List<HierarchyLevelEntry>(),
            category_styles = new List<CategoryStyleEntry>(),
            outline_levels = new List<OutlineLevelEntry>(),
            badge_categories = new List<BadgeCategoryEntry>
            {
                new BadgeCategoryEntry { key = "badge_1", label = "Old", icon_key = "unknown", search_keywords = new List<string>() },
            },
        };

        [TearDown]
        public void TearDown() => _window?.Close();

        [UnityTest]
        public IEnumerator TypingAWord_IsOneUndoStep_AndOneRedoStep()
        {
            _window = new PoiEditorWindowHost(BadgeConfig(), "_showGlobalBadge");
            yield return _window.WaitForRepaint();
            int stepsBefore = _window.UndoStepsLeft;

            yield return _window.ReplaceText("Badge label#0", "Cracked");
            Assert.AreEqual("Cracked", _window.Config.badge_categories[0].label, "precondition: the typing landed");
            Assert.AreEqual(stepsBefore + 1, _window.UndoStepsLeft, "seven keystrokes in one cell = one history step");

            yield return _window.ClickAway();
            yield return _window.PressUndo();
            Assert.AreEqual("Old", _window.Config.badge_categories[0].label, "ONE Ctrl+Z takes the whole word back");

            yield return _window.PressRedo();
            Assert.AreEqual("Cracked", _window.Config.badge_categories[0].label, "ONE Ctrl+Y puts the whole word back");
        }

        [UnityTest]
        public IEnumerator TypingIntoTheSameCellAgainLater_IsASecondStep()
        {
            _window = new PoiEditorWindowHost(BadgeConfig(), "_showGlobalBadge");
            yield return _window.WaitForRepaint();

            yield return _window.ReplaceText("Badge label#0", "First");
            yield return _window.ClickAway();
            yield return _window.ReplaceText("Badge label#0", "Second");
            Assert.AreEqual("Second", _window.Config.badge_categories[0].label, "precondition: both edits landed");

            yield return _window.ClickAway();
            yield return _window.PressUndo();
            Assert.AreEqual("First", _window.Config.badge_categories[0].label,
                "focus left the cell between the two words, so the first Ctrl+Z only takes back the second word");
            yield return _window.PressUndo();
            Assert.AreEqual("Old", _window.Config.badge_categories[0].label, "the second Ctrl+Z takes back the first word");
        }

        [UnityTest]
        public IEnumerator DraggingASlider_IsOneUndoStep()
        {
            _window = new PoiEditorWindowHost(BadgeConfig(), "_showGlobalBadge");
            yield return _window.WaitForRepaint();
            int stepsBefore = _window.UndoStepsLeft;

            // A real drag along the slider track: press near its right end, move left in steps, release
            Rect row = _window.RectOf("Badge size");
            float y = row.center.y;
            float startX = row.xMax - 70f;
            var seen = new HashSet<float>();
            _window.Send(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX, y)) });
            yield return _window.WaitForRepaint();
            seen.Add(_window.Config.badge_size_ratio);
            for (int i = 1; i <= 6; i++)
            {
                _window.Send(new Event { type = EventType.MouseDrag, button = 0, mousePosition = _window.Local(new Vector2(startX - 12f * i, y)), delta = new Vector2(-12f, 0f) });
                yield return _window.WaitForRepaint();
                seen.Add(_window.Config.badge_size_ratio);
            }
            _window.Send(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX - 72f, y)) });
            yield return _window.WaitForRepaint();

            Assert.GreaterOrEqual(seen.Count, 3, "precondition: the drag must have changed the value several times");
            Assert.AreNotEqual(0.36f, _window.Config.badge_size_ratio, "precondition: the drag moved the slider");
            Assert.AreEqual(stepsBefore + 1, _window.UndoStepsLeft, "a whole drag = one history step");

            yield return _window.PressUndo();
            Assert.AreEqual(0.36f, _window.Config.badge_size_ratio, 1e-5f, "ONE Ctrl+Z puts the slider back where the drag started");
        }
    }
}
