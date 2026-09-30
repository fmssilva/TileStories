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
                    new CategoryStyleEntry { key = "cat_a", icon_key = "old_icon", details = "old note" }
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

        // _3.1 step 13: the default-media picker follows the same real-factory / real-callback rule as the symbol
        // picker above -- a pick must be undoable and mark the config unsaved through the SAME mutation scope
        [Test]
        public void DefaultMediaPicked_FromTheCuratedPicker_IsUndoable_AndMarksTheConfigUnsaved_AndRedoable()
        {
            _config.card_settings.media_resources_path = "";
            var popup = Call<CardMediaDefaultPickerPopup>("CreateCardMediaDefaultPickerPopup", MediaKind.Image,
                (Action<string>)(key => _config.card_settings.media_resources_path = MediaPathRule.PathForDefaultKey(key)));
            var onPicked = (Action<string>)typeof(CardMediaDefaultPickerPopup).GetField("_onPicked", Instance).GetValue(popup);

            onPicked("azulejo_blue");   // exactly what the popup calls when a row is clicked

            Assert.AreEqual("default:azulejo_blue", LiveConfig.card_settings.media_resources_path, "the pick must reach the config");
            Assert.IsTrue(Unsaved, "a pick must mark the config unsaved");
            Assert.IsTrue(Call<bool>("CanUndoConfigChange"));

            Call<object>("UndoConfigChange");
            Assert.AreEqual("", LiveConfig.card_settings.media_resources_path, "Ctrl+Z must restore the previous value");
            Call<object>("RedoConfigChange");
            Assert.AreEqual("default:azulejo_blue", LiveConfig.card_settings.media_resources_path, "Ctrl+Y must bring the pick back");
        }

        [Test]
        public void DefaultMediaPicker_OffersOnlyKeysOfTheFieldsOwnMediaKind()
        {
            var pictures = Call<CardMediaDefaultPickerPopup>("CreateCardMediaDefaultPickerPopup", MediaKind.Image, (Action<string>)(_ => { }));
            var audio = Call<CardMediaDefaultPickerPopup>("CreateCardMediaDefaultPickerPopup", MediaKind.Audio, (Action<string>)(_ => { }));
            Assert.Greater(pictures.RowCount, 0, "the Framework ships default pictures");
            Assert.Greater(audio.RowCount, 0, "the Framework ships default audio");
            Assert.AreNotEqual(pictures.RowCount, 0);
        }

        // 10A.4: a Panorama field's picker lists the shipped 360 picture, and a picture field's does not list it (both are textures:
        // only the library entry's kind tells them apart), and picking it stores the exact `default:` value the view resolves
        [Test]
        public void DefaultMediaPicker_ForAPanoramaField_OffersTheShippedPanorama_AndAPictureFieldDoesNot()
        {
            System.Collections.Generic.List<string> KeysOf(MediaKind kind)
            {
                var popup = Call<CardMediaDefaultPickerPopup>("CreateCardMediaDefaultPickerPopup", kind, (Action<string>)(_ => { }));
                var rows = (System.Collections.IEnumerable)typeof(CardMediaDefaultPickerPopup).GetField("_rows", Instance).GetValue(popup);
                var keys = new System.Collections.Generic.List<string>();
                foreach (CardMediaLibrary.Entry row in rows) keys.Add(row.key);
                return keys;
            }

            CollectionAssert.Contains(KeysOf(MediaKind.Panorama), "tiled_room_360", "the Framework's 360 picture is offered to a Panorama field");
            CollectionAssert.DoesNotContain(KeysOf(MediaKind.Image), "tiled_room_360", "and not to a picture field");
            CollectionAssert.DoesNotContain(KeysOf(MediaKind.Panorama), "azulejo_detail", "a Panorama field offers no ordinary picture");

            string picked = null;
            var popup2 = Call<CardMediaDefaultPickerPopup>("CreateCardMediaDefaultPickerPopup", MediaKind.Panorama, (Action<string>)(key => picked = MediaPathRule.PathForDefaultKey(key)));
            ((Action<string>)typeof(CardMediaDefaultPickerPopup).GetField("_onPicked", Instance).GetValue(popup2))("tiled_room_360");
            Assert.AreEqual("default:tiled_room_360", picked);
            Assert.IsTrue(MediaPathRule.IsValid(picked, MediaKind.Panorama), "a Panorama field accepts the picked value");
        }

        // An unknown default key (renamed or removed from the library, or typed by hand into a saved config):
        // resolving it returns nothing and the Editor's own row warns instead of throwing
        [Test]
        public void AnUnknownDefaultKey_ResolvesToNothing_AndTheRowsWarningNamesIt()
        {
            Assert.IsNull(POIEditorToolWindow.MediaAssetFor("default:not_a_real_key", "", MediaKind.Image, ""));
            StringAssert.Contains("not_a_real_key", POIEditorToolWindow.CardUnknownDefaultKeyText("Picture", "not_a_real_key"));
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
                    if (Regex.IsMatch(lines[i], @"new\s+(ExistingSymbolPickerPopup|EntryDetailsPopup|CardMediaDefaultPickerPopup)\s*\("))
                        offenders.Add(Path.GetFileName(file) + ":" + (i + 1));
            }
            Assert.IsEmpty(offenders, "Build popups with CreateSymbolPickerPopup / CreateDetailsPopup / CreateCardMediaDefaultPickerPopup: " + string.Join(", ", offenders));
        }
    }
}
