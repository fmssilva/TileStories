using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Edits made from a popup (the curated symbol picker behind every Symbol/Outline Style/Custom
    // symbol thumbnail, and every Details note) are written from the POPUP's own OnGUI, after the POI
    // Editor's DrawConfigMutationScope has already closed. Without their own scope they skipped undo,
    // the unsaved flag, the Scene rig refresh and the live Play Mode push. These tests build the REAL
    // popup through the window's factory and fire the exact callback the popup itself calls.
    public class POIEditorPopupEditTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        private POIEditorToolWindow _window;
        private WallConfigData _config;

        [SetUp]
        public void SetUp()
        {
            _config = new WallConfigData
            {
                pois = new List<POIData>(),
                category_styles = new List<CategoryStyleEntry>
                {
                    new CategoryStyleEntry { category = "cat_a", icon_key = "old_icon", details = "old note" }
                }
            };
            _window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(_window, _config);
            typeof(POIEditorToolWindow).GetMethod("InitializeConfigHistory", Instance).Invoke(_window, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_window != null) UnityEngine.Object.DestroyImmediate(_window);
        }

        private T Call<T>(string method, params object[] args)
        {
            var m = typeof(POIEditorToolWindow).GetMethod(method, Instance);
            Assert.IsNotNull(m, "POIEditorToolWindow." + method + " must exist");
            return (T)m.Invoke(_window, args);
        }

        private bool Unsaved => (bool)typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).GetValue(_window);

        // The live config (undo swaps _config wholesale, so never keep the SetUp reference after undo)
        private WallConfigData LiveConfig => (WallConfigData)typeof(POIEditorToolWindow).GetField("_config", Instance).GetValue(_window);

        [Test]
        public void SymbolPicked_FromTheCuratedPicker_IsUndoable_AndMarksTheConfigUnsaved()
        {
            var entry = _config.category_styles[0];
            var popup = Call<ExistingSymbolPickerPopup>("CreateSymbolPickerPopup", (Action<string>)(key => entry.icon_key = key));
            var onPicked = (Action<string>)typeof(ExistingSymbolPickerPopup).GetField("_onPicked", Instance).GetValue(popup);

            onPicked("new_icon");   // exactly what the popup calls when a thumbnail is clicked

            Assert.AreEqual("new_icon", entry.icon_key, "the pick must reach the config");
            Assert.IsTrue(Unsaved, "a pick must mark the config unsaved (Save All to JSON turns yellow)");
            Assert.IsTrue(Call<bool>("CanUndoConfigChange"), "a pick must be one undo step");

            Call<object>("UndoConfigChange");
            Assert.AreEqual("old_icon", LiveConfig.category_styles[0].icon_key, "Ctrl+Z must restore the previous symbol");
        }

        [Test]
        public void DetailsTyped_InTheDetailsPopup_IsUndoable_AndMarksTheConfigUnsaved()
        {
            var entry = _config.category_styles[0];
            var popup = Call<EntryDetailsPopup>("CreateDetailsPopup", "cat_a",
                (Func<string>)(() => entry.details), (Action<string>)(v => entry.details = v));
            var set = (Action<string>)typeof(EntryDetailsPopup).GetField("_set", Instance).GetValue(popup);

            set("new note");   // exactly what the popup calls on each keystroke

            Assert.AreEqual("new note", entry.details);
            Assert.IsTrue(Unsaved, "a typed note must mark the config unsaved, or a reload silently loses it");
            Call<object>("UndoConfigChange");
            Assert.AreEqual("old note", LiveConfig.category_styles[0].details, "Ctrl+Z must restore the previous note");
        }

        // Every popup in the window is built by the two factories, so no call site can forget the scope
        [Test]
        public void EveryPopupInTheWindow_IsBuiltThroughTheMutationScopedFactories()
        {
            string root = Path.Combine(Application.dataPath, "Framework/Editor/POIEditor");
            var offenders = new List<string>();
            foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                if (file.EndsWith("POIEditorToolWindow.ConfigHistory.cs")) continue;   // the factories themselves
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                    if (Regex.IsMatch(lines[i], @"new\s+(ExistingSymbolPickerPopup|EntryDetailsPopup)\s*\("))
                        offenders.Add(Path.GetFileName(file) + ":" + (i + 1));
            }
            Assert.IsEmpty(offenders, "Build popups with CreateSymbolPickerPopup / CreateDetailsPopup: " + string.Join(", ", offenders));
        }
    }
}
