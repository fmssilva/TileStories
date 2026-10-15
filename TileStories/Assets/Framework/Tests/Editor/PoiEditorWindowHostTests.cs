using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // The shared typing harness (PoiEditorWindowHost.ReplaceText) under a focus loss made on purpose, on a known editor tick.
    // While Unity is not the active application, focus can leave the host window BETWEEN two editor ticks, and a focus loss ends
    // the IMGUI text field's editing session: what was typed so far is committed, every later key is dropped ("Live heading"
    // arrived as "Live head", 40-testing 4.2.3). Another window taking focus from an editor-update callback is that same event,
    // so these tests prove the harness without a person clicking away at the right second, and they pass focused or not (proven:
    // a full EditMode run with Unity in the background; Focus() alone did NOT end editing there, hence FocusThief's explicit call).
    public class PoiEditorWindowHostTests
    {
        private PoiEditorWindowHost _window;
        private FocusThief _thief;

        // A window that takes keyboard focus from the host ONCE, on the first editor tick its condition holds. Focus() moves focus only
        // while Unity is the active application, so the field-side effect of a focus loss -- the editing session ends, what was typed
        // is committed -- is also done explicitly, the call Unity makes for it: the theft is the same focused or in the background
        private sealed class FocusThief : EditorWindow
        {
            private static readonly MethodInfo EndEditing = typeof(EditorGUI).GetMethod("EndEditingActiveTextField", BindingFlags.NonPublic | BindingFlags.Static);
            public Func<bool> StealWhen;
            public int Stolen;

            public void OnTick()
            {
                if (StealWhen == null || !StealWhen()) return;
                StealWhen = null;
                Stolen++;
                Focus();
                EndEditing.Invoke(null, null);
            }
        }

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

        private string Label => _window.Config.badge_categories[0].label;

        [SetUp]
        public void SetUp()
        {
            _thief = ScriptableObject.CreateInstance<FocusThief>();
            _thief.ShowUtility();
            _thief.position = new Rect(40f, 40f, 160f, 80f);
            // - registered after the test runner's own update: it runs on the same tick, right after the test's step
            EditorApplication.update += _thief.OnTick;
        }

        [TearDown]
        public void TearDown()
        {
            EditorApplication.update -= _thief.OnTick;
            _window?.Close();
            _thief.Close();
            Assert.IsEmpty(Resources.FindObjectsOfTypeAll<FocusThief>(), "the test leaves no window behind (40-testing 4.2.3)");
        }

        // The window on the badge table, with the precondition both tests stand on: a theft really ends the cell's editing
        // session (else they would prove nothing)
        private IEnumerator OpenAndProveATheftEndsEditing()
        {
            _window = new PoiEditorWindowHost(BadgeConfig(), "_showGlobalBadge");
            yield return _window.WaitForRepaint();
            _window.Click("Badge label#0", leftEdge: true);
            yield return _window.WaitForRepaint();
            Assert.IsTrue(EditorGUIUtility.editingTextField, "precondition: a click starts editing the cell");
            _thief.StealWhen = () => true;
            yield return _window.WaitForRepaint();
            Assert.AreEqual(1, _thief.Stolen, "precondition: the thief took focus");
            Assert.IsFalse(EditorGUIUtility.editingTextField, "precondition: another window taking focus ends the cell's editing session");
            _thief.Stolen = 0;
            yield return _window.ClickAway();
        }

        [UnityTest]
        public IEnumerator AFocusLossRightAfterTheClick_TheHarnessEntersTheCellAgain_AndTheWordIsOneUndoStep()
        {
            yield return OpenAndProveATheftEndsEditing();
            int stepsBefore = _window.UndoStepsLeft;

            // - the tick right after the click, before any key: the only place a focus loss can fall once the word goes out in one tick
            _thief.StealWhen = () => EditorGUIUtility.editingTextField && Label == "Old";
            yield return _window.ReplaceText("Badge label#0", "Cracked");
            Assert.AreEqual(1, _thief.Stolen, "precondition: focus was really taken between the click and the typing");
            yield return _window.ClickAway();

            Assert.AreEqual("Cracked", Label, "every key of the word arrived");
            Assert.AreEqual(stepsBefore + 1, _window.UndoStepsLeft, "entering the cell a second time made no extra history step");
            yield return _window.PressUndo();
            Assert.AreEqual("Old", Label, "ONE Ctrl+Z takes the whole word back, as for a person typing it");
        }

        [UnityTest]
        public IEnumerator AFocusLossAsSoonAsTypingLanded_CommitsTheWholeWord()
        {
            yield return OpenAndProveATheftEndsEditing();
            int stepsBefore = _window.UndoStepsLeft;

            // - the first tick on which ANY typed key reached the config: one key per tick would be cut here ("C")
            _thief.StealWhen = () => Label != "Old";
            yield return _window.ReplaceText("Badge label#0", "Cracked");
            Assert.AreEqual(1, _thief.Stolen, "precondition: focus was really taken as soon as typing landed");
            yield return _window.ClickAway();

            Assert.AreEqual("Cracked", Label, "the focus loss committed the WHOLE word, never a part of it");
            Assert.AreEqual(stepsBefore + 1, _window.UndoStepsLeft, "one word = one history step");
        }
    }
}
