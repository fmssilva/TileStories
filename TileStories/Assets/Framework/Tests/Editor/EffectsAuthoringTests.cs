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
    // Effects domain (_2.2.4), authoring side: the editor surface, the config file and the
    // runtime resolver must agree. Real code only: the real window's undo/redo history, the real
    // MarkerHierarchyResolver, the shipped LivingRoom config, the constants the window shows.
    // Round trip and undo/redo walk EffectDefaults BY REFLECTION, so a field added later is covered
    // without touching this file.
    public class EffectsAuthoringTests
    {
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        [TearDown]
        public void TearDown() => MarkerHierarchyResolver.ResetToDefaults();

        private static string[] EditorOptions(string fieldName)
        {
            var value = (string[])typeof(POIEditorToolWindow).GetField(fieldName, Static)?.GetValue(null);
            Assert.IsNotNull(value, fieldName + " must exist on the editor window");
            return value;
        }

        // ---------------- editor option strings <-> runtime resolver ----------------

        [Test]
        public void EditorDropdownOptions_AreAllUnderstoodByTheResolver_AndReachEveryEffectFlag()
        {
            string[] ripples = EditorOptions("RippleEffectOptions");
            string[] halos = EditorOptions("HaloEffectOptions");
            Assert.AreEqual(3, ripples.Length, "Precondition: none + rings + discs");
            Assert.AreEqual(4, halos.Length, "Precondition: none + halo ring + halo disc + beacon");

            var warnings = new List<string>();
            Application.LogCallback capture = (msg, _, type) => { if (type == LogType.Warning) warnings.Add(msg); };
            Application.logMessageReceived += capture;

            var reached = MarkerEffectFlags.None;
            var singles = new HashSet<MarkerEffectFlags>();
            try
            {
                foreach (string ripple in ripples)
                    foreach (string halo in halos)
                    {
                        var level = new HierarchyLevelEntry { key = "k", size_cm = 10f, ripple_effect = ripple, halo_effect = halo };
                        MarkerHierarchyResolver.Configure(new[] { level });
                        Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey("k", out var style), $"{ripple}|{halo} must resolve");
                        reached |= style.EffectFlags;
                        if (halo == "none") singles.Add(style.EffectFlags);
                        if (ripple == "none") singles.Add(style.EffectFlags);
                    }
                reached |= MarkerHierarchyResolver.EffectFlagsOf(new HierarchyLevelEntry { pulse = true });
            }
            finally { Application.logMessageReceived -= capture; }

            CollectionAssert.IsEmpty(warnings, "Every editor dropdown option must be accepted by the resolver without an 'Unknown ... effect' warning.");
            foreach (var effect in EffectDefaults.SelectableEffects)
                Assert.IsTrue((reached & effect) != 0, $"{effect} must be selectable in the Hierarchy Levels table");

            // none + 2 ripple + 3 halo distinct single results.
            Assert.AreEqual(6, singles.Count, "Each dropdown option must map to a different flag set.");
        }

        // ---------------- reflection helpers ----------------

        private static IEnumerable<FieldInfo> PublicFields(Type t) =>
            t.GetFields(BindingFlags.Public | BindingFlags.Instance);

        private static bool IsBlock(Type t) => t.IsClass && t != typeof(string);

        // Change every value inside obj (recursively) to something different from its default.
        private static void MutateEverything(object obj)
        {
            foreach (var f in PublicFields(obj.GetType()))
            {
                object v = f.GetValue(obj);
                if (f.FieldType == typeof(float)) f.SetValue(obj, (float)v + 0.0173f);
                else if (f.FieldType == typeof(bool)) f.SetValue(obj, !(bool)v);
                else if (f.FieldType == typeof(string)) f.SetValue(obj, "#ABCDEF");
                else if (IsBlock(f.FieldType)) MutateEverything(v);
            }
        }

        private static void AssertSameValues(object expected, object actual, string path)
        {
            foreach (var f in PublicFields(expected.GetType()))
            {
                object e = f.GetValue(expected), a = f.GetValue(actual);
                string p = path + "." + f.Name;
                if (f.FieldType == typeof(float)) Assert.AreEqual((float)e, (float)a, 1e-4f, p);
                else if (IsBlock(f.FieldType) && f.FieldType != typeof(string)) AssertSameValues(e, a, p);
                else Assert.AreEqual(e, a, p);
            }
        }

        private static int CountFields(object obj) =>
            PublicFields(obj.GetType()).Sum(f => IsBlock(f.FieldType) && f.FieldType != typeof(string) ? CountFields(f.GetValue(obj)) : 1);

        // ---------------- JSON round trip ----------------

        [Test]
        public void EveryEffectField_SurvivesJsonRoundTrip()
        {
            var config = new WallConfigData { wall_id = "fx" };
            config.effect_defaults = new EffectDefaults();
            MutateEverything(config.effect_defaults);
            Assert.GreaterOrEqual(CountFields(config.effect_defaults), 44, "Precondition: the whole schema was really walked.");
            Assert.IsFalse(config.effect_defaults.effects_enabled, "Precondition: the mutation flipped the master switch.");

            var loaded = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(config, true));
            AssertSameValues(config.effect_defaults, loaded.effect_defaults, "effect_defaults");
        }

        [Test]
        public void LevelEffectFields_SurviveJsonRoundTrip()
        {
            var config = new WallConfigData { wall_id = "fx" };
            config.hierarchy_levels.Add(new HierarchyLevelEntry
            {
                key = "lv", ripple_effect = "ripple_discs", halo_effect = "beacon", pulse = true, rotate_contour = true,
                reveal_delay_s = 0.6f, reveal_duration_s = 0.9f, show_label = true, size_cm = 22f,
            });
            var lv = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(config, true)).hierarchy_levels[0];
            Assert.AreEqual("ripple_discs", lv.ripple_effect); Assert.AreEqual("beacon", lv.halo_effect);
            Assert.IsTrue(lv.pulse); Assert.IsTrue(lv.rotate_contour);
            Assert.AreEqual(0.6f, lv.reveal_delay_s, 1e-4f); Assert.AreEqual(0.9f, lv.reveal_duration_s, 1e-4f);
        }

        [Test]
        public void PartialEffectBlock_LoadsWithEveryMissingValueAtItsDefault()
        {
            const string json = "{\"effect_defaults\":{\"pulse\":{\"amplitude\":0.3}}}";
            var d = JsonUtility.FromJson<WallConfigData>(json).effect_defaults;
            Assert.AreEqual(0.3f, d.pulse.amplitude, 1e-4f, "Precondition: the given value was read.");
            Assert.IsTrue(d.effects_enabled, "master switch defaults ON");
            foreach (var effect in EffectDefaults.SelectableEffects)
                Assert.IsTrue(d.IsEffectEnabled(effect), effect + " defaults ON");
            Assert.AreEqual(1.6f, d.pulse.period, 1e-4f, "missing pulse.period keeps its default");
            Assert.AreEqual(0.12f, d.ripple_rings.stagger, 1e-4f, "missing blocks keep their defaults");
            Assert.IsFalse(d.preview.enabled, "preview defaults OFF");
        }

        [Test]
        public void ShippedLivingRoomConfig_HasSaneEffectValues_AndBothCopiesAgree()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Apps/LivingRoom/config.json");
            string streamingPath = Path.Combine(Application.dataPath, "StreamingAssets/LivingRoom/config.json");
            string sourceJson = File.ReadAllText(sourcePath);
            Assert.AreEqual(sourceJson, File.ReadAllText(streamingPath), "Apps and StreamingAssets copies must be identical.");
            StringAssert.DoesNotContain("\"sun_effect\"", sourceJson, "Old effect keys must be gone from the config.");
            StringAssert.DoesNotContain("\"accent_effect\"", sourceJson, "Old effect keys must be gone from the config.");

            var config = JsonUtility.FromJson<WallConfigData>(sourceJson);
            var d = config.effect_defaults;
            Assert.IsNotNull(d, "LivingRoom authors effect_defaults");
            Assert.IsTrue(d.effects_enabled, "LivingRoom ships with effects on");

            foreach (var period in new[] { d.pulse.period, d.ripple_rings.period, d.ripple_discs.period, d.halo_ring.period, d.halo_disc.period, d.beacon.period })
                Assert.GreaterOrEqual(period, 0.1f);
            foreach (var r in new[] { d.ripple_rings, d.ripple_discs })
            {
                foreach (float a in new[] { r.inner_alpha, r.middle_alpha, r.outer_alpha })
                    Assert.IsTrue(a >= 0f && a <= 1f, "alpha in 0..1, was " + a);
                Assert.Greater(r.inner_alpha, r.middle_alpha, "inner wave strongest");
                Assert.Greater(r.middle_alpha, r.outer_alpha, "outer wave faintest");
                Assert.IsTrue(ColorUtility.TryParseHtmlString(r.tint_color_hex, out _), "ripple tint parses");
            }
            foreach (var tint in new[] { d.halo_ring.tint_color_hex, d.halo_disc.tint_color_hex, d.beacon.tint_color_hex })
                Assert.IsTrue(ColorUtility.TryParseHtmlString(tint, out _), "halo tint parses: " + tint);

            Assert.Greater(config.hierarchy_levels.Count, 0, "Precondition: levels authored");
            foreach (var level in config.hierarchy_levels)
            {
                CollectionAssert.Contains(EditorOptions("RippleEffectOptions"), level.ripple_effect, level.key + " ripple_effect must be a dropdown option");
                CollectionAssert.Contains(EditorOptions("HaloEffectOptions"), level.halo_effect, level.key + " halo_effect must be a dropdown option");
                Assert.GreaterOrEqual(level.reveal_delay_s, 0f, level.key);
                Assert.GreaterOrEqual(level.reveal_duration_s, 0f, level.key);
            }
        }

        // ---------------- undo / redo through the real window ----------------

        private sealed class FieldEdit
        {
            public string Name;
            public Action<WallConfigData> Change;
            public Func<WallConfigData, object> Read;
        }

        // One edit per leaf field of EffectDefaults (found by reflection) plus the six per-level fields.
        private static List<FieldEdit> AllEffectFieldEdits()
        {
            var edits = new List<FieldEdit>();
            foreach (var top in PublicFields(typeof(EffectDefaults)))
            {
                var topField = top;
                if (top.FieldType == typeof(bool))
                {
                    edits.Add(new FieldEdit { Name = top.Name, Change = c => topField.SetValue(c.effect_defaults, !(bool)topField.GetValue(c.effect_defaults)), Read = c => topField.GetValue(c.effect_defaults) });
                    continue;
                }
                foreach (var leaf in PublicFields(top.FieldType))
                {
                    var leafField = leaf;
                    edits.Add(new FieldEdit
                    {
                        Name = top.Name + "." + leaf.Name,
                        Change = c =>
                        {
                            object block = topField.GetValue(c.effect_defaults);
                            object v = leafField.GetValue(block);
                            leafField.SetValue(block, leafField.FieldType == typeof(float) ? (object)((float)v + 0.0173f)
                                : leafField.FieldType == typeof(bool) ? !(bool)v : "#ABCDEF");
                        },
                        Read = c => leafField.GetValue(topField.GetValue(c.effect_defaults)),
                    });
                }
            }

            edits.Add(new FieldEdit { Name = "level.ripple_effect", Change = c => c.hierarchy_levels[0].ripple_effect = "ripple_rings", Read = c => c.hierarchy_levels[0].ripple_effect });
            edits.Add(new FieldEdit { Name = "level.halo_effect", Change = c => c.hierarchy_levels[0].halo_effect = "beacon", Read = c => c.hierarchy_levels[0].halo_effect });
            edits.Add(new FieldEdit { Name = "level.pulse", Change = c => c.hierarchy_levels[0].pulse = true, Read = c => c.hierarchy_levels[0].pulse });
            edits.Add(new FieldEdit { Name = "level.rotate_contour", Change = c => c.hierarchy_levels[0].rotate_contour = true, Read = c => c.hierarchy_levels[0].rotate_contour });
            edits.Add(new FieldEdit { Name = "level.reveal_delay_s", Change = c => c.hierarchy_levels[0].reveal_delay_s = 1.5f, Read = c => c.hierarchy_levels[0].reveal_delay_s });
            edits.Add(new FieldEdit { Name = "level.reveal_duration_s", Change = c => c.hierarchy_levels[0].reveal_duration_s = 1.2f, Read = c => c.hierarchy_levels[0].reveal_duration_s });
            return edits;
        }

        [Test]
        public void EveryEffectField_UndoAndRedo_ThroughTheRealWindowHistory()
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var t = typeof(POIEditorToolWindow);
                var configField = t.GetField("_config", Instance);
                var initHistory = t.GetMethod("InitializeConfigHistory", Instance);
                var mutate = t.GetMethod("DrawConfigMutationScope", Instance);
                var undo = t.GetMethod("UndoConfigChange", Instance);
                var redo = t.GetMethod("RedoConfigChange", Instance);
                Assert.IsNotNull(configField); Assert.IsNotNull(initHistory); Assert.IsNotNull(mutate); Assert.IsNotNull(undo); Assert.IsNotNull(redo);

                var edits = AllEffectFieldEdits();
                Assert.GreaterOrEqual(edits.Count, 50, "Precondition: every effect field plus the 6 level fields");

                foreach (var edit in edits)
                {
                    var config = new WallConfigData { wall_id = "undo" };
                    config.effect_defaults = new EffectDefaults();
                    config.hierarchy_levels.Add(new HierarchyLevelEntry { key = "lv", size_cm = 10f, ripple_effect = "none", halo_effect = "none" });
                    configField.SetValue(window, config);
                    initHistory.Invoke(window, null);

                    object original = edit.Read(config);
                    mutate.Invoke(window, new object[] { (Action)(() => edit.Change(config)), false });
                    object changed = edit.Read(config);
                    Assert.AreNotEqual(original, changed, edit.Name + ": precondition, the edit must actually change the value");

                    undo.Invoke(window, null);
                    Assert.AreEqual(original, edit.Read((WallConfigData)configField.GetValue(window)), edit.Name + ": undo must restore the original value");

                    redo.Invoke(window, null);
                    Assert.AreEqual(changed, edit.Read((WallConfigData)configField.GetValue(window)), edit.Name + ": redo must re-apply the edited value");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        [Test]
        public void TurningEffectsOffAndOn_KeepsEverySetting_ThroughSaveAndLoad()
        {
            // The editor only HIDES rows when the master switch or an effect is off; this proves the
            // data is untouched by the round trip a save does while they are off.
            var config = new WallConfigData { wall_id = "keep" };
            config.effect_defaults = new EffectDefaults();
            config.effect_defaults.pulse.amplitude = 0.31f;
            config.effect_defaults.beacon.end_scale = 2.7f;
            config.effect_defaults.ripple_discs.tint_color_hex = "#123456";

            config.effect_defaults.effects_enabled = false;
            config.effect_defaults.beacon.enabled = false;
            var saved = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(config, true));
            saved.effect_defaults.effects_enabled = true;
            saved.effect_defaults.beacon.enabled = true;

            Assert.AreEqual(0.31f, saved.effect_defaults.pulse.amplitude, 1e-4f);
            Assert.AreEqual(2.7f, saved.effect_defaults.beacon.end_scale, 1e-4f);
            Assert.AreEqual("#123456", saved.effect_defaults.ripple_discs.tint_color_hex);
        }

        // ---------------- constants: guides and help texts ----------------

        [Test]
        public void EffectsTestGuides_ExistAsciiCoverEveryPart_NameEveryEffect_AndStartCollapsed()
        {
            var t = typeof(POIEditorToolWindow);
            var effectNames = EffectDefaults.SelectableEffects.Select(MarkerEffectNames.DisplayName).ToList();
            var terms = new List<string> { "Enable effects", "Hierarchy Levels", "Reveal Delay", "Rotate", "Add effects demo grid" };
            terms.AddRange(effectNames);

            foreach (string guideName in new[] { "EffectsSceneTestGuide", "EffectsPlaymodeTestGuide", "EffectsDeviceTestGuide" })
            {
                var guide = (string)t.GetField(guideName, Static)?.GetValue(null);
                Assert.IsFalse(string.IsNullOrWhiteSpace(guide), guideName + " must exist and not be empty");
                AssertAscii(guide, guideName);

                foreach (string block in new[] { "MASTER SWITCH", "PULSE", "RIPPLE", "HALO", "REVEAL AND ROTATE" })
                    StringAssert.Contains(block, guide, guideName + " must have a '" + block + "' block");

                if (guideName == "EffectsSceneTestGuide")
                {
                    StringAssert.Contains("Not possible in Scene test", guide, guideName);
                    StringAssert.Contains("Edit Mode does not run Update", guide, guideName);
                }
                else if (guideName == "EffectsPlaymodeTestGuide")
                {
                    foreach (string term in terms)
                        StringAssert.Contains(term, guide, guideName + " must mention '" + term + "'");
                    StringAssert.Contains("MarkerGalleryScene", guide, guideName);
                }
                else
                {
                    StringAssert.Contains("adb logcat", guide, guideName);
                    StringAssert.Contains("Development Build", guide, guideName);
                }
            }

            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                foreach (string foldoutField in new[] { "_showEffectsSceneTestGuide", "_showEffectsPlaymodeTestGuide", "_showEffectsDeviceTestGuide",
                    "_showEffectPulse", "_showEffectRippleRings", "_showEffectRippleDiscs", "_showEffectHaloRing", "_showEffectHaloDisc", "_showEffectBeacon" })
                {
                    var f = t.GetField(foldoutField, Instance);
                    Assert.IsNotNull(f, foldoutField + " must exist");
                    Assert.IsFalse((bool)f.GetValue(window), foldoutField + " must default to collapsed");
                }
                Assert.IsTrue((bool)t.GetField("_showEffectsTest", Instance).GetValue(window), "The Test foldout itself defaults to open");
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        [Test]
        public void EffectHelpTexts_AreAscii_AndEveryEffectHelpExplainsTheFlow()
        {
            var t = typeof(POIEditorToolWindow);
            var helpNames = new Regex("^(Effect|Pulse|Ripple|Halo|Beacon|Disabled|Hierarchy)\\w*(Help|Note)$");
            var fields = t.GetFields(Static).Where(f => f.FieldType == typeof(string) && helpNames.IsMatch(f.Name)).ToList();
            Assert.GreaterOrEqual(fields.Count, 20, "Precondition: the effect help texts were found by name.");
            foreach (var f in fields)
            {
                var text = (string)f.GetValue(null);
                Assert.IsFalse(string.IsNullOrWhiteSpace(text), f.Name);
                AssertAscii(text, f.Name);
            }

            // Each effect's own (i) says where it is chosen, so the link to the level table is never lost.
            foreach (string name in new[] { "PulseHelp", "RippleRingsHelp", "RippleDiscsHelp", "HaloRingHelp", "HaloDiscHelp", "BeaconHelp" })
                StringAssert.Contains("hierarchy level", ((string)t.GetField(name, Static).GetValue(null)).ToLowerInvariant(), name);
            StringAssert.Contains("Hierarchy Levels", (string)t.GetField("EffectsFlowNote", Static).GetValue(null));
        }

        private static void AssertAscii(string text, string what)
        {
            foreach (char c in text)
                Assert.IsTrue(c == '\n' || (c >= ' ' && c <= '~'), what + " must be ASCII only, found char code " + (int)c);
        }
    }
}
