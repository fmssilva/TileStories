using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TileStories.Editor;

namespace TileStories.Tests
{
    // A new POI's default level follows the developer's own priority numbers, the Priority
    // rules hold, and every row of the symbol picker is clickable anywhere.
    public class HierarchyPriorityAndPickerTests
    {
        private static HierarchyLevelEntry L(string key, int priority) => new HierarchyLevelEntry { key = key, priority = priority };

        [Test]
        public void HighestPriorityLevel_UsesTheSmallestNumber_NotTheFirstRow()
        {
            var levels = new List<HierarchyLevelEntry> { L("a", 100), L("b", 10), L("c", 50) };
            Assert.AreEqual("b", POIEditorToolWindow.HighestPriorityLevelKey(levels));
        }

        [Test]
        public void HighestPriorityLevel_TieGoesToTheEarlierRow_AndUnsetUsesRowPosition()
        {
            Assert.AreEqual("a", POIEditorToolWindow.HighestPriorityLevelKey(new List<HierarchyLevelEntry> { L("a", 7), L("b", 7) }));
            // b is unset (0) -> row position 2, which beats a's explicit 5, exactly as the runtime ranks them.
            Assert.AreEqual("b", POIEditorToolWindow.HighestPriorityLevelKey(new List<HierarchyLevelEntry> { L("a", 5), L("b", 0) }));
        }

        [Test]
        public void HighestPriorityLevel_MatchesTheRuntimeResolver()
        {
            var levels = new List<HierarchyLevelEntry> { L("x", 40), L("y", 0), L("z", 3), L("w", 3) };
            MarkerHierarchyResolver.Configure(levels);
            try
            {
                string expected = null;
                int best = int.MaxValue;
                foreach (var l in levels)
                    if (MarkerHierarchyResolver.TryResolvePriority(l.key, out int p) && p < best) { best = p; expected = l.key; }
                Assert.AreEqual(expected, POIEditorToolWindow.HighestPriorityLevelKey(levels));
            }
            finally { MarkerHierarchyResolver.ResetToDefaults(); }
        }

        [Test]
        public void HighestPriorityLevel_EmptyOrNull_ReturnsNull_AndSkipsBlankKeys()
        {
            Assert.IsNull(POIEditorToolWindow.HighestPriorityLevelKey(null));
            Assert.IsNull(POIEditorToolWindow.HighestPriorityLevelKey(new List<HierarchyLevelEntry>()));
            Assert.AreEqual("ok", POIEditorToolWindow.HighestPriorityLevelKey(new List<HierarchyLevelEntry> { L("", 1), null, L("ok", 9) }));
        }

        [Test]
        public void NextLowestPriority_IsOneMoreThanTheLargestInUse()
        {
            Assert.AreEqual(101, POIEditorToolWindow.NextLowestPriority(new List<HierarchyLevelEntry> { L("a", 10), L("b", 100) }));
            Assert.AreEqual(4, POIEditorToolWindow.NextLowestPriority(new List<HierarchyLevelEntry> { L("a", 1), L("b", 2), L("c", 3) }));
            Assert.AreEqual(1, POIEditorToolWindow.NextLowestPriority(null));
        }

        // A file without Priority loads 0 = unset. Load stores the order the runtime ALREADY gave each level, so
        // the value is explicit from then on and nothing changes meaning (drawing used to write 1 = top priority).
        [Test]
        public void FillUnsetPriorities_StoresExactlyTheRuntimeOrder_AndLeavesSetOnesAlone()
        {
            var levels = new List<HierarchyLevelEntry> { L("a", 5), L("", 0), null, L("b", 0), L("c", -3), L("d", 100) };
            var runtimeBefore = new Dictionary<string, int>();
            MarkerHierarchyResolver.Configure(levels);
            try
            {
                foreach (var k in new[] { "a", "b", "c", "d" })
                {
                    Assert.IsTrue(MarkerHierarchyResolver.TryResolvePriority(k, out int p));
                    runtimeBefore[k] = p;
                }
            }
            finally { MarkerHierarchyResolver.ResetToDefaults(); }

            Assert.AreEqual(2, POIEditorToolWindow.FillUnsetLevelPriorities(levels), "b and c were unset; the blank-key row is not a level");
            Assert.AreEqual(5, levels[0].priority, "a set priority is never touched");
            Assert.AreEqual(100, levels[5].priority);
            foreach (var k in new[] { "a", "b", "c", "d" })
                Assert.AreEqual(runtimeBefore[k], levels.Find(l => l != null && l.key == k).priority,
                    k + ": the stored value is the one the runtime was already using (rows counted like the runtime: keyed rows only)");
            Assert.AreEqual(0, POIEditorToolWindow.FillUnsetLevelPriorities(levels), "nothing left to fill");
            Assert.AreEqual(0, POIEditorToolWindow.FillUnsetLevelPriorities(null));
        }

        // A blank-key row is not a level: the new-POI default counts rows exactly like the runtime does
        [Test]
        public void HighestPriorityLevel_ABlankKeyRowAbove_DoesNotShiftTheRowOrder()
        {
            var levels = new List<HierarchyLevelEntry> { L("", 0), L("b", 0), L("a", 1) };
            // runtime: b = the FIRST keyed row -> 1, a = 1; the tie goes to the earlier row, b.
            // Counting every row (the old copy of the rule) made b = 2 and picked a.
            Assert.AreEqual("b", POIEditorToolWindow.HighestPriorityLevelKey(levels));
            MarkerHierarchyResolver.Configure(levels);
            try
            {
                Assert.IsTrue(MarkerHierarchyResolver.TryResolvePriority("b", out int b));
                Assert.AreEqual(1, b, "the runtime agrees");
            }
            finally { MarkerHierarchyResolver.ResetToDefaults(); }
        }

        [Test]
        public void PriorityField_IsClampedToOne_InTheEditorSource()
        {
            string src = System.IO.File.ReadAllText("Assets/Framework/Editor/POIEditor/GlobalScene/POIEditorToolWindow.GlobalScene.cs");
            StringAssert.Contains("Mathf.Max(1, EditorGUI.IntField(priorityFieldRect, entry.priority))", src);
        }

        // Real popup drawn in a real editor window and clicked with real events.
        private sealed class PopupHost : EditorWindow
        {
            public ExistingSymbolPickerPopup Popup;
            private void OnGUI() => Popup.Draw();
        }

        [TestCase(20f, Description = "thumbnail")]
        [TestCase(120f, Description = "name")]
        [TestCase(235f, Description = "far right, beside the name")]
        public void RealPopup_ClickingAnywhereOnTheSecondRow_PicksThatSymbol(float x)
        {
            var tex = new Texture2D(4, 4);
            var library = ScriptableObject.CreateInstance<SpriteKeyLibrary>();
            var entries = new SerializedObject(library).FindProperty("entries");
            var so = entries.serializedObject;
            string[] keys = { "first", "second", "third" };
            entries.arraySize = keys.Length;
            for (int i = 0; i < keys.Length; i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("key").stringValue = keys[i];
                e.FindPropertyRelative("sprite").objectReferenceValue = Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.zero, 100f);
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            string picked = null;
            var host = ScriptableObject.CreateInstance<PopupHost>();
            try
            {
                host.Popup = new ExistingSymbolPickerPopup(library, null, k => picked = k);
                host.position = new Rect(100f, 100f, 260f, 300f);
                host.ShowUtility();
                host.Repaint();
                // Rows start at the top (the title is in the window's title bar): row 2 spans y 40-80.
                float y = 60f;
                host.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = new Vector2(x, y) });
                Assert.AreEqual("second", picked);
            }
            finally
            {
                host.Close();
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(library);
                Object.DestroyImmediate(tex);
            }
        }

        // ---- symbol picker: the whole row is the button ----

        private static readonly Rect Row = new Rect(0f, 40f, 240f, 40f);

        private static Event Mouse(EventType type, Vector2 pos, int button = 0) =>
            new Event { type = type, mousePosition = pos, button = button };

        [TestCase(20f, 60f)]   // thumbnail
        [TestCase(90f, 60f)]   // name text
        [TestCase(230f, 45f)]  // empty space at the right end
        public void PickerRow_LeftClickAnywhereInsideTheRow_Picks(float x, float y)
        {
            Assert.IsTrue(ExistingSymbolPickerPopup.IsPickClick(Mouse(EventType.MouseDown, new Vector2(x, y)), Row));
        }

        [Test]
        public void PickerRow_IgnoresOtherRows_RightClick_AndOtherEvents()
        {
            Assert.IsFalse(ExistingSymbolPickerPopup.IsPickClick(Mouse(EventType.MouseDown, new Vector2(90f, 100f)), Row), "another row");
            Assert.IsFalse(ExistingSymbolPickerPopup.IsPickClick(Mouse(EventType.MouseDown, new Vector2(90f, 60f), 1), Row), "right click");
            Assert.IsFalse(ExistingSymbolPickerPopup.IsPickClick(Mouse(EventType.MouseUp, new Vector2(90f, 60f)), Row), "mouse up");
            Assert.IsFalse(ExistingSymbolPickerPopup.IsPickClick(Mouse(EventType.Repaint, new Vector2(90f, 60f)), Row), "repaint");
        }
    }
}
