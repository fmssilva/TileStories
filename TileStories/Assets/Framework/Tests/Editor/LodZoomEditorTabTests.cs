using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // The LOD and Zoom sections of the POI Editor (_2.4_Marker_LOD.md section 6): real clicks on the
    // real window, the Suggest Values contract, the section's show-this-row rules against the
    // runtime's own rules, and the help / guide text contract (_5.1_Editor_Tab.md, "Domain Manual
    // Tests"). No mocks: the window is the production POIEditorToolWindow hosted in a real OnGUI.
    public class LodZoomEditorTabTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;

        // ---------------- real clicks on the Distance Bands table ----------------

        private sealed class Host : EditorWindow
        {
            public static POIEditorToolWindow Editor;
            public static Vector2 RootScreen;
            public static int Repaints;

            private void OnGUI()
            {
                if (Editor == null) return;
                RootScreen = GUIUtility.GUIToScreenPoint(Vector2.zero);
                typeof(POIEditorToolWindow).GetMethod("OnGUI", Instance).Invoke(Editor, null);
                if (Event.current.type == EventType.Repaint) Repaints++;
            }
        }

        private Host _host;
        private POIEditorToolWindow _editor;
        private readonly Dictionary<string, Rect> _cells = new();

        private WallConfigData HostWindowOn(WallConfigData config)
        {
            _editor = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(_editor, config);
            typeof(POIEditorToolWindow).GetField("_showGlobalLod", Instance).SetValue(_editor, true);
            typeof(POIEditorToolWindow).GetMethod("InitializeConfigHistory", Instance).Invoke(_editor, null);
            _cells.Clear();
            POIEditorToolWindow.LodBandCellRectProbe = (column, row, rect) => _cells[column + "#" + row] = GUIUtility.GUIToScreenRect(rect);
            Host.Editor = _editor;
            Host.Repaints = 0;
            _host = ScriptableObject.CreateInstance<Host>();
            _host.ShowUtility();
            _host.position = new Rect(40f, 40f, 900f, 1400f);
            return config;
        }

        [TearDown]
        public void TearDown()
        {
            POIEditorToolWindow.LodBandCellRectProbe = null;
            Host.Editor = null;
            if (_host != null) _host.Close();
            if (_editor != null) Object.DestroyImmediate(_editor);
        }

        private IEnumerator WaitForRepaint()
        {
            int before = Host.Repaints;
            _host.Repaint();
            for (int i = 0; i < 120 && Host.Repaints == before; i++)
                yield return null;
            Assert.Greater(Host.Repaints, before, "the host window must have repainted");
        }

        private void Click(string cell)
        {
            Assert.IsTrue(_cells.TryGetValue(cell, out Rect screen), "precondition: '" + cell + "' was drawn");
            Vector2 local = screen.center - Host.RootScreen;
            _host.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = local });
            _host.SendEvent(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = local });
        }

        private static WallConfigData ConfigWithThreeBands() => new WallConfigData
        {
            pois = new List<POIData>(),
            hierarchy_levels = new List<HierarchyLevelEntry>(),
            lod_settings = new LodSettings(), // the three framework default bands
        };

        [UnityTest]
        public IEnumerator DeletingABand_WithARealClick_RemovesThatRowOnly_WithoutBreakingTheLayout_AndIsUndoable()
        {
            var config = HostWindowOn(ConfigWithThreeBands());
            yield return WaitForRepaint();

            Click("delete#1"); // the 7 m row
            CollectionAssert.AreEqual(new[] { 2f, 9999f }, config.lod_settings.bands.Select(b => b.max_distance_m).ToArray(),
                "the clicked row, and only it, is removed");
            Assert.IsTrue((bool)typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).GetValue(_editor), "marks the config unsaved");
            yield return WaitForRepaint(); // a broken layout group would log an error here (and fail this test)

            _host.SendEvent(new Event { type = EventType.KeyDown, keyCode = KeyCode.Z, control = true });
            var live = (WallConfigData)typeof(POIEditorToolWindow).GetField("_config", Instance).GetValue(_editor);
            CollectionAssert.AreEqual(new[] { 2f, 7f, 9999f }, live.lod_settings.bands.Select(b => b.max_distance_m).ToArray(),
                "Ctrl+Z brings the band back");
            yield return WaitForRepaint();
            Assert.IsTrue(_cells.ContainsKey("delete#2"), "after undo the table draws three rows again");
        }

        [Test]
        public void SuggestValues_ChangesOnlyTheBandsAndTheTwoCrowdingCounts()
        {
            var config = ConfigWithThreeBands();
            for (int i = 0; i < 30; i++) config.pois.Add(new POIData { id = "p" + i });
            config.lod_settings.transition_fade_duration_s = 0.9f;
            config.lod_settings.density_response_mode = "cluster";
            config.lod_settings.cluster_size_ratio = 2.5f;
            config.zoom_settings.max_factor = 6f;
            var zoom = config.zoom_settings;
            string lodBefore = JsonUtility.ToJson(config.lod_settings);

            var editor = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(editor, config);
                editor.ApplySuggestedLodValues();
            }
            finally { Object.DestroyImmediate(editor); }

            var expected = LodAutoSuggest.Suggest(30);
            Assert.AreEqual(JsonUtility.ToJson(expected.bands[2]), JsonUtility.ToJson(config.lod_settings.bands[2]));
            Assert.AreEqual(expected.cluster_min_count, config.lod_settings.cluster_min_count);
            Assert.AreEqual(expected.shrink_start_neighbor_count, config.lod_settings.shrink_start_neighbor_count);
            Assert.AreEqual(0.9f, config.lod_settings.transition_fade_duration_s, "Fade kept");
            Assert.AreEqual("cluster", config.lod_settings.density_response_mode, "Response kept");
            Assert.AreEqual(2.5f, config.lod_settings.cluster_size_ratio, "Cluster Size kept");
            Assert.AreSame(zoom, config.zoom_settings, "Zoom untouched (it used to be reset with the whole LOD object)");
            Assert.AreEqual(6f, config.zoom_settings.max_factor);
            Assert.AreNotEqual(lodBefore, JsonUtility.ToJson(config.lod_settings), "precondition: something was suggested");
        }

        // ---------------- the section's rules agree with the runtime ----------------

        private static readonly string[] Modes = { "none", "select_hide", "cluster", "shrink_and_fade", "hybrid" };

        // Every density state the runtime can reach for this mode, over a range of neighbour counts
        private static HashSet<DensityState> Reachable(string mode, bool safetyNet)
        {
            var s = new LodSettings { density_response_mode = mode, density_safety_escalation_enabled = safetyNet };
            var states = new HashSet<DensityState>();
            for (int n = 0; n <= 40; n++)
                foreach (bool selectedToHide in new[] { false, true }) // Select & Hide's priority selection (Crowded At) decides this
                    states.Add(LODController.ComputeTargetDensityState(n, s, s.shrink_start_neighbor_count, s.cluster_min_count, selectedToHide));
            return states;
        }

        [Test]
        public void RowRules_ShowAFieldExactlyWhenTheRuntimeCanUseIt([ValueSource(nameof(Modes))] string mode)
        {
            var noNet = Reachable(mode, false);
            Assert.AreEqual(noNet.Contains(DensityState.Shrinking), LodEditorRules.UsesShrink(mode), mode + ": Shrink rows");
            Assert.AreEqual(noNet.Contains(DensityState.Clustered) || noNet.Contains(DensityState.Shrinking) && mode == "shrink_and_fade",
                LodEditorRules.UsesCrowdedCount(mode), mode + ": Crowded At row");
            // Clusters sub-foldout: shown exactly when the runtime can build a cluster with these settings
            foreach (bool net in new[] { false, true })
            {
                var s = new LodSettings { density_response_mode = mode, density_safety_escalation_enabled = net };
                Assert.AreEqual(RuntimeCanCluster(s), LodEditorRules.CanBuildClusters(s), mode + " safety=" + net + ": Clusters sub-foldout");
            }

            // Safety Net rows: shown exactly when switching the net on changes whether clusters can happen
            bool netMatters = RuntimeCanCluster(new LodSettings { density_response_mode = mode, density_safety_escalation_enabled = true })
                              != RuntimeCanCluster(new LodSettings { density_response_mode = mode, density_safety_escalation_enabled = false });
            Assert.AreEqual(netMatters, LodEditorRules.UsesSafetyNet(mode), mode + ": Safety Net rows");
        }

        // Can LODController turn some marker into a cluster member with these settings? (its own rules)
        private static bool RuntimeCanCluster(LodSettings s) => Enumerable.Range(0, 41).Any(n => new[] { false, true }.Any(selectedToHide =>
            LODController.ComputeTargetDensityState(n, s, s.shrink_start_neighbor_count, s.cluster_min_count, selectedToHide) == DensityState.Clustered
            && LODController.ShouldAggregate(new VisualUnit { densityState = DensityState.Clustered, neighborCount = n }, s)));

        [Test]
        public void BandProblems_FlagWhatTheRuntimeCannotUse()
        {
            Assert.IsEmpty(LodEditorRules.BandProblems(LodSettings.DefaultBands()), "the framework defaults are fine");
            Assert.IsNotEmpty(LodEditorRules.BandProblems(new List<LodBandEntry>()), "no bands");
            Assert.IsNotEmpty(LodEditorRules.BandProblems(new List<LodBandEntry>
            {
                new LodBandEntry { max_distance_m = 7f, max_visible_count = -1 },
                new LodBandEntry { max_distance_m = 2f, max_visible_count = 5 },
            }), "far before near: the runtime's first-match lookup would never reach the 2 m row");
            Assert.IsNotEmpty(LodEditorRules.BandProblems(new List<LodBandEntry> { new LodBandEntry { max_distance_m = 5f, max_visible_count = 0 } }),
                "Max markers 0 would hide every marker");
        }

        // ---------------- help / guide text contract ----------------

        private static readonly string[] Prefixes = { "Lod", "Zoom", "DemoField" };

        private static readonly string[] ForbiddenTerms =
        {
            ".md", ".cs", "_2.", "_5.1", "LivingRoom", "lamp", "painting_", "dev_marker_",
            "LODController", "MarkerClusterView", "ARZoom", "Evaluate(", "CanvasGroup", "MarkerOverlapResolver",
            "_settings", "density_", "cluster_min", "shrink_start", "zoom_min", "zoom_max", "hysteresis_margin", "max_visible",
        };

        private static Dictionary<string, string> Texts()
        {
            var texts = new Dictionary<string, string>();
            foreach (var f in typeof(POIEditorToolWindow).GetFields(Static))
                if (f.FieldType == typeof(string) && Prefixes.Any(p => f.Name.StartsWith(p)) && (f.Name.EndsWith("Help") || f.Name.EndsWith("Guide")))
                    texts[f.Name] = (string)f.GetValue(null);
            return texts;
        }

        private static string[] Labels(string field) => (string[])typeof(POIEditorToolWindow).GetField(field, Static).GetValue(null);

        [Test]
        public void HelpAndGuides_AreAsciiAppAgnostic_AndNameOnlyEditorTabControls()
        {
            var texts = Texts();
            Assert.Greater(texts.Count, 30, "precondition: the LOD / Zoom texts were really collected");
            foreach (var pair in texts)
            {
                Assert.IsTrue(pair.Value.All(c => c < 128), pair.Key + " must be ASCII only");
                foreach (string term in ForbiddenTerms)
                    StringAssert.DoesNotContain(term, pair.Value, pair.Key + " must not contain '" + term + "'");
                // "the Displacement section" names a window section; "section 6.2" would point at a doc
                Assert.IsFalse(Regex.IsMatch(pair.Value, @"section \d"), pair.Key + " must not point at a document section");
            }
        }

        [Test]
        public void Guides_HaveOneBlockPerSubFoldout_AndSayWhatTheSceneCannotShow()
        {
            var texts = Texts();
            var lodBlocks = new[] { "SETUP", "DISTANCE BANDS", "CROWDING", "CLUSTERS", "TRANSITIONS & PERFORMANCE" };
            var zoomBlocks = new[] { "SETUP", "ZOOM RANGE", "GESTURES & BUTTONS" };
            foreach (var (guide, blocks) in new[]
                     {
                         ("LodSceneTestGuide", lodBlocks), ("LodPlaymodeTestGuide", lodBlocks), ("LodDeviceTestGuide", lodBlocks),
                         ("ZoomSceneTestGuide", zoomBlocks), ("ZoomPlaymodeTestGuide", zoomBlocks), ("ZoomDeviceTestGuide", zoomBlocks),
                     })
            {
                int last = -1;
                foreach (string block in blocks)
                {
                    int at = texts[guide].IndexOf(block + "\n", System.StringComparison.Ordinal);
                    Assert.Greater(at, last, guide + ": block '" + block + "' present and in editor order");
                    last = at;
                }
            }
            foreach (string guide in new[] { "LodSceneTestGuide", "ZoomSceneTestGuide" })
                Assert.AreEqual(Regex.Matches(texts[guide], @"\n\n[A-Z&, ]+\n").Count, Regex.Matches(texts[guide], "Not possible in Scene test").Count,
                    guide + ": every block after SETUP says the Scene view cannot show it");
        }

        [Test]
        public void EveryOptionAndEveryControlOfTheSections_IsExplainedSomewhere()
        {
            string all = string.Join("\n", Texts().Values);
            foreach (string field in new[] { "DensityModeLabels", "ClusterIconLabels", "ClusterBandSourceLabels" })
                foreach (string label in Labels(field))
                    StringAssert.Contains(label, all, "option '" + label + "' (" + field + ") must be explained");

            // every labelled control the LOD / Zoom sections draw is named in their help or guides
            string source = File.ReadAllText(Path.Combine(Application.dataPath, "Framework/Editor/POIEditor/GlobalScene/POIEditorToolWindow.LodZoom.cs"));
            source = source.Substring(source.IndexOf("// ---- LOD section ----", System.StringComparison.Ordinal));
            var labels = Regex.Matches(source, @"Draw(?:Toggle|Scalar|Int|Popup|Slider|IntSlider)Field\(""([^""]+)""")
                .Cast<Match>().Select(m => Regex.Replace(m.Groups[1].Value, @" \(.*\)$", "")).Distinct().ToList();
            Assert.Greater(labels.Count, 20, "precondition: the section's controls were found");
            foreach (string label in labels)
                StringAssert.Contains(label, all, "control '" + label + "' is never explained in the LOD / Zoom help or guides");
        }

        [Test]
        public void TestFoldouts_StartOpen_WithTheirGuidesCollapsed()
        {
            var editor = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                foreach (string field in new[] { "_lodTest", "_zoomTest" })
                {
                    var state = typeof(POIEditorToolWindow).GetField(field, Instance).GetValue(editor);
                    var t = state.GetType();
                    Assert.IsTrue((bool)t.GetField("Open").GetValue(state), field + ": the Test foldout starts open");
                    foreach (string guide in new[] { "Scene", "Playmode", "Device" })
                        Assert.IsFalse((bool)t.GetField(guide).GetValue(state), field + ": the " + guide + " guide starts collapsed");
                }
            }
            finally { Object.DestroyImmediate(editor); }
        }
    }
}
