using System;
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
    // Global Scene > Select, Filter & Search in the real POIEditorToolWindow (_2.6): the help and guide contract
    // (ASCII, app-agnostic, Editor Tab controls only, every drawn row and option explained, labels that fit),
    // every field of select_filter_search and search_demo -- walked by reflection so a new field cannot be
    // forgotten -- through undo / redo, the JSON round trip and the live Play Mode fingerprints, the shipped
    // configs, the validation, the real OnGUI in every branch, the readout wording, and the per-POI keyword
    // drawer never writing the config just by drawing.
    public class SearchFilterEditorTabTests
    {
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        private static string Text(string name)
        {
            var f = typeof(POIEditorToolWindow).GetField(name, Static);
            Assert.IsNotNull(f, name + " must exist");
            return (string)f.GetValue(null);
        }

        private static string[] Labels(string name) => (string[])typeof(POIEditorToolWindow).GetField(name, Static).GetValue(null);

        private static string[] HelpNames() => typeof(POIEditorToolWindow).GetFields(Static)
            .Where(f => f.FieldType == typeof(string) && f.DeclaringType == typeof(POIEditorToolWindow))
            .Select(f => f.Name)
            .Where(n => SearchHelpNames.Contains(n)).ToArray();

        private static readonly string[] SearchHelpNames =
        {
            "SearchEnabledHelp", "HighlightSelectionHelp", "DimOthersHelp", "ZoomOnSelectHelp", "ZoomTriggerHelp", "CrowdRadiusHelp",
            "MinNeighboursHelp", "ZoomFactorHelp", "SearchModeHelp", "MatchModeHelp", "PrefixModeHelp", "TypoToleranceHelp",
            "NoResultsHelp", "FacetGroupsHelp", "MismatchHelp", "DimFilteredHelp", "RelaxHelp", "DefaultViewHelp", "RememberViewHelp",
            "RecentSearchesHelp", "SuggestionsHelp", "MinimapEnabledHelp", "MinimapVisibilityHelp", "MinimapIconHelp",
            "MinimapDotSizeHelp", "MinimapTapTargetHelp", "MinimapProjectionHelp", "MinimapBoundsHelp", "VoiceEnabledHelp",
            "VoiceIndicatorHelp", "KeywordFieldsHelp", "KeywordFieldKeyHelp", "KeywordFieldLabelHelp", "KeywordFieldRequiredHelp",
            "SynonymGroupsHelp", "SearchDemoHelp", "SearchDemoPerCategoryHelp", "SearchDemoTestCasesHelp", "SearchDemoLabelsHelp",
            "SearchDemoDistanceHelp", "SearchDemoSpacingHelp", "SearchDemoRunLodHelp", "TryQueryHelp", "SearchReadoutHelp",
            "NoResultsFiltersHelp", "PoiSummaryHelp", "PoiKeywordFieldHelp", "PoiOthersKeywordsHelp", "PoiFoundByHelp", "KeywordFieldFilterHelp", "DemoQueryHelp",
        };

        private static readonly string[] GuideNames = { "SearchSceneTestGuide", "SearchPlaymodeTestGuide", "SearchDeviceTestGuide" };

        // (label array, the runtime's option values it must line up with)
        private static readonly (string labels, string[] values)[] OptionPairs =
        {
            ("ZoomTriggerLabels", SelectFilterSearchOptions.Triggers), ("SearchModeLabels", SelectFilterSearchOptions.Modes),
            ("MatchModeLabels", SelectFilterSearchOptions.MatchModes), ("PrefixModeLabels", SelectFilterSearchOptions.PrefixModes),
            ("MismatchLabels", SelectFilterSearchOptions.Mismatches), ("ResultViewLabels", SelectFilterSearchOptions.Views),
            ("SuggestionSourceLabels", SelectFilterSearchOptions.SuggestionSources), ("MinimapVisibilityLabels", SelectFilterSearchOptions.Visibilities),
            ("MinimapIconLabels", SelectFilterSearchOptions.IconStyles), ("MinimapProjectionLabels", SelectFilterSearchOptions.Projections),
            ("MinimapBoundsLabels", SelectFilterSearchOptions.BoundsModes), ("VoiceIndicatorLabels", SelectFilterSearchOptions.IndicatorStyles),
        };

        private static string SectionSource() =>
            File.ReadAllText(Directory.GetFiles(Application.dataPath, "POIEditorToolWindow.SearchFilter.cs", SearchOption.AllDirectories).Single());

        private static List<string> DrawnLabels() =>
            Regex.Matches(SectionSource(), "(?:Draw(?:Toggle|Popup|Slider|IntSlider|Scalar|Int)Field|DrawTextRow|DrawVector3Row)\\(\"([^\"]+)\"")
                .Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();

        // ---- help and guides ----

        [Test]
        public void EveryHelpAndGuide_IsAscii_AppAgnostic_AndNamesNoCodeOrDoc()
        {
            Assert.AreEqual(SearchHelpNames.Length, HelpNames().Length, "every help text exists");
            string[] forbidden =
            {
                ".cs", ".md", "_2.", "section 7", "WallSession", "POISearchIndex", "SearchUIHost", "LivingRoom", "lamp", "painting",
                "Royal Government", "level_1", "all_fields", "camera_highlight", "category_colored_dots", "mic_text",
                "name_only", "inert", "planned", "WordNet",
            };
            foreach (string name in SearchHelpNames.Concat(GuideNames))
            {
                string text = Text(name);
                Assert.IsFalse(string.IsNullOrWhiteSpace(text), name);
                Assert.IsTrue(text.All(c => c < 128), name + " must be ASCII");
                foreach (string bad in forbidden)
                    Assert.IsFalse(text.IndexOf(bad, StringComparison.OrdinalIgnoreCase) >= 0, name + " must not mention '" + bad + "'");
            }
        }

        [Test]
        public void PlaymodeGuide_ExplainsEveryDrawnRow_AndEveryOption_OneBlockPerSubSection()
        {
            string guide = Text("SearchPlaymodeTestGuide");
            foreach (string block in new[] { "SETUP", "DEMO CONTROLS", "SELECTION", "SEARCH", "FILTERS", "RESULTS AND VIEWS", "MINIMAP", "VOICE", "KEYWORDS AND SYNONYMS" })
                StringAssert.Contains(block + "\n", guide, "one block per sub-section");

            var drawn = DrawnLabels();
            Assert.GreaterOrEqual(drawn.Count, 35, "precondition: the section's rows were found");
            foreach (string label in drawn)
                StringAssert.Contains(label, guide, "the Playmode guide must say what '" + label + "' does");
            StringAssert.Contains("Live Search Readout", guide);
            StringAssert.Contains("Try a Query", guide);

            foreach (var (labels, _) in OptionPairs)
                foreach (string option in Labels(labels))
                    StringAssert.Contains(option, guide, "option '" + option + "' is explained in the Playmode guide");
        }

        [Test]
        public void EveryDrawnLabel_FitsTheLabelColumn()
        {
            foreach (string label in DrawnLabels())
                Assert.LessOrEqual(label.Length, 23, "'" + label + "' would be clipped");
        }

        [Test]
        public void SceneGuide_SaysPlayModeIsNeeded_DeviceGuide_TellsToUntickTheDemo()
        {
            StringAssert.Contains("Not possible in Scene test", Text("SearchSceneTestGuide"));
            StringAssert.Contains("Untick 'Add search demo'", Text("SearchDeviceTestGuide"));
            StringAssert.Contains("Developer only", Text("SearchDemoHelp"));
        }

        [Test]
        public void EveryLabelArray_LinesUpWithTheRuntimeValues()
        {
            foreach (var (labels, values) in OptionPairs)
                Assert.AreEqual(values.Length, Labels(labels).Length, labels + ": one label per runtime value");
        }

        // ---- every field, walked ----

        // Every leaf field under a settings object: (path, owner getter, field)
        private static List<(string path, Func<WallConfigData, object> owner, FieldInfo field)> Leaves()
        {
            var leaves = new List<(string, Func<WallConfigData, object>, FieldInfo)>();
            void Walk(Type type, string prefix, Func<WallConfigData, object> owner)
            {
                foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (f.FieldType.IsClass && f.FieldType != typeof(string) && f.FieldType.Namespace == typeof(WallConfigData).Namespace)
                    {
                        var inner = f;
                        Walk(f.FieldType, prefix + f.Name + ".", c => inner.GetValue(owner(c)));
                    }
                    else leaves.Add((prefix + f.Name, owner, f));
                }
            }
            Walk(typeof(SelectFilterSearchSettings), "select_filter_search.", c => c.select_filter_search);
            Walk(typeof(SearchDemoSettings), "search_demo.", c => c.search_demo);
            return leaves;
        }

        private static readonly Dictionary<string, string[]> StringOptions = new()
        {
            { "trigger", SelectFilterSearchOptions.Triggers }, { "mode", SelectFilterSearchOptions.Modes },
            { "match_mode", SelectFilterSearchOptions.MatchModes }, { "prefix_matching", SelectFilterSearchOptions.PrefixModes },
            { "mismatch", SelectFilterSearchOptions.Mismatches }, { "default_view", SelectFilterSearchOptions.Views },
            { "suggestion_source", SelectFilterSearchOptions.SuggestionSources }, { "visibility", SelectFilterSearchOptions.Visibilities },
            { "icon_style", SelectFilterSearchOptions.IconStyles }, { "projection", SelectFilterSearchOptions.Projections },
            { "bounds_mode", SelectFilterSearchOptions.BoundsModes }, { "indicator_style", SelectFilterSearchOptions.IndicatorStyles },
            { "no_results_message", new[] { "Nothing here", "Still nothing" } },
            { "no_results_filters_message", new[] { "No filter match", "Still no filter match" } },
        };

        // A different, valid value for any leaf
        private static object Changed(FieldInfo f, object current)
        {
            if (f.FieldType == typeof(bool)) return !(bool)current;
            if (f.FieldType == typeof(int)) return (int)current + 1;
            if (f.FieldType == typeof(float)) return (float)current + 0.25f;
            if (f.FieldType == typeof(Vector3)) return (Vector3)current + new Vector3(1f, 2f, 3f);
            if (f.FieldType == typeof(string))
            {
                Assert.IsTrue(StringOptions.ContainsKey(f.Name), "no test values for the string field " + f.Name);
                return StringOptions[f.Name].First(v => v != (string)current);
            }
            throw new AssertionException("no test value for " + f.Name + " (" + f.FieldType + ")");
        }

        [Test]
        public void EveryField_UndoAndRedo_ThroughTheRealWindowHistory()
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

                var leaves = Leaves();
                Assert.GreaterOrEqual(leaves.Count, 43, "precondition: every field was found (36 settings + 7 demo)");
                foreach (var (path, owner, f) in leaves)
                {
                    var config = new WallConfigData { wall_id = "undo" };
                    configField.SetValue(window, config);
                    initHistory.Invoke(window, null);
                    object original = f.GetValue(owner(config));
                    object changed = Changed(f, original);
                    mutate.Invoke(window, new object[] { (Action)(() => f.SetValue(owner(config), changed)), false });
                    undo.Invoke(window, null);
                    Assert.AreEqual(original, f.GetValue(owner((WallConfigData)configField.GetValue(window))), path + ": undo restores it");
                    redo.Invoke(window, null);
                    Assert.AreEqual(changed, f.GetValue(owner((WallConfigData)configField.GetValue(window))), path + ": redo re-applies it");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        [Test]
        public void EveryField_SurvivesTheJsonRoundTrip_AndAMissingBlockGetsTheDefaults()
        {
            var config = new WallConfigData();
            var expected = new Dictionary<string, object>();
            foreach (var (path, owner, f) in Leaves())
            {
                object changed = Changed(f, f.GetValue(owner(config)));
                f.SetValue(owner(config), changed);
                expected[path] = changed;
            }
            var back = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(config));
            foreach (var (path, owner, f) in Leaves())
                Assert.AreEqual(expected[path], f.GetValue(owner(back)), path + " survives Save + Load");

            var bare = JsonUtility.FromJson<WallConfigData>("{\"wall_id\":\"x\"}");
            Assert.IsNotNull(bare.select_filter_search?.selection?.zoom, "a config without the block gets every sub-block");
            Assert.IsTrue(bare.select_filter_search.enabled);
            Assert.IsFalse(bare.search_demo.enabled, "the dev-only demo is off by default");
        }

        [Test]
        public void ShippedConfigs_UseTheBlock_NoOldFlatField_AndBothCopiesAgreeOnIt()
        {
            string apps = File.ReadAllText(Path.Combine(Application.dataPath, "Apps/LivingRoom/config.json"));
            string streaming = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json"));
            foreach (string flat in new[] { "\"selection_highlight_enabled\"", "\"minimap_enabled\"", "\"search_filter_select_enabled\"", "\"wall_bounds\"", "\"zoom_on_select_trigger\"" })
            {
                Assert.IsFalse(apps.Contains(flat), "authoring config still has " + flat);
                Assert.IsFalse(streaming.Contains(flat), "StreamingAssets config still has " + flat);
            }
            var a = JsonUtility.FromJson<WallConfigData>(apps);
            var s = JsonUtility.FromJson<WallConfigData>(streaming);
            Assert.AreEqual(JsonUtility.ToJson(a.select_filter_search), JsonUtility.ToJson(s.select_filter_search), "both copies agree");
            Assert.IsFalse(s.search_demo.enabled, "the shipped copy has the dev-only demo off");
        }

        // ---- live Play Mode ----

        [Test]
        public void LiveFingerprints_ChangeForEveryFieldOfTheirOwn_AndNothingElse()
        {
            var search = new LivePlayModeSearchApplier();
            var demo = new LivePlayModeSearchDemoApplier();
            WallConfigData Fresh() => new WallConfigData { pois = new List<POIData> { new() { id = "p", name = "P" } } };
            var baseline = Fresh();
            string s0 = search.Fingerprint(baseline), d0 = demo.Fingerprint(baseline);

            foreach (var (path, owner, f) in Leaves())
            {
                var c = Fresh();
                f.SetValue(owner(c), Changed(f, f.GetValue(owner(c))));
                if (path.StartsWith("search_demo."))
                {
                    Assert.AreNotEqual(d0, demo.Fingerprint(c), path + " rebuilds the demo live");
                    Assert.AreEqual(s0, search.Fingerprint(c), path + " never re-applies the search settings");
                }
                else
                {
                    Assert.AreNotEqual(s0, search.Fingerprint(c), path + " is pushed live");
                    Assert.AreEqual(d0, demo.Fingerprint(c), path + " never rebuilds the demo");
                }
            }

            var vocabulary = new (string what, Action<WallConfigData> edit)[]
            {
                ("a keyword field", c => c.search_fields.Add(new SearchFieldDefinition { key = "era" })),
                ("a synonym group", c => c.synonym_groups.Add(new SynonymGroup { key = "church" })),
                ("a POI's Others keywords", c => c.pois[0].search_keywords.Add("stone")),
                ("a POI's keyword field", c => c.pois[0].search_keyword_fields.Add(new POISearchKeywordField { field_key = "era" })),
                ("a POI's summary", c => c.pois[0].summary = "new text"),
            };
            foreach (var (what, edit) in vocabulary)
            {
                var c = Fresh();
                edit(c);
                Assert.AreNotEqual(s0, search.Fingerprint(c), what + " re-indexes live");
            }

            var lodEdit = Fresh();
            lodEdit.lod_settings.enabled = !lodEdit.lod_settings.enabled;
            Assert.AreEqual(s0, search.Fingerprint(lodEdit), "another domain's edit is ignored");

            var dispatcher = typeof(LivePlayModeConfigPush).GetMethod("CreateDispatcher", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, null);
            var appliers = (IEnumerable<ILivePlayModeApplier>)dispatcher.GetType()
                .GetFields(Instance | BindingFlags.Public).First(f => typeof(IEnumerable<ILivePlayModeApplier>).IsAssignableFrom(f.FieldType))
                .GetValue(dispatcher);
            Assert.IsTrue(appliers.Any(a => a is LivePlayModeSearchApplier), "the search applier is registered");
            Assert.IsTrue(appliers.Any(a => a is LivePlayModeSearchDemoApplier), "the demo applier is registered");
        }

        [Test]
        public void PoiSummary_UndoAndRedo_ThroughTheRealWindowHistory()
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var t = typeof(POIEditorToolWindow);
                var configField = t.GetField("_config", Instance);
                var config = new WallConfigData { pois = new List<POIData> { new() { id = "p", name = "P", summary = "before" } } };
                configField.SetValue(window, config);
                t.GetMethod("InitializeConfigHistory", Instance).Invoke(window, null);
                t.GetMethod("DrawConfigMutationScope", Instance).Invoke(window, new object[] { (Action)(() => config.pois[0].summary = "after"), false });

                t.GetMethod("UndoConfigChange", Instance).Invoke(window, null);
                Assert.AreEqual("before", ((WallConfigData)configField.GetValue(window)).pois[0].summary, "undo restores the summary");
                t.GetMethod("RedoConfigChange", Instance).Invoke(window, null);
                Assert.AreEqual("after", ((WallConfigData)configField.GetValue(window)).pois[0].summary, "redo re-applies it");
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        // ---- validation, guard ----

        [Test]
        public void Validation_ReportsEachUnknownOption_AndNothingForKnownOnes()
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var validate = typeof(POIEditorToolWindow).GetMethod("ValidateSearchEnumFields", Instance);
                var config = new WallConfigData();
                typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(window, config);
                Assert.AreEqual(0, ((IList)validate.Invoke(window, null)).Count, "the defaults are all valid");

                int strings = 0;
                foreach (var (path, owner, f) in Leaves().Where(l => l.path.StartsWith("select_filter_search.") && l.field.FieldType == typeof(string) && !l.field.Name.StartsWith("no_results")))
                {
                    var c = new WallConfigData();
                    typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(window, c);
                    f.SetValue(owner(c), "not_a_value");
                    Assert.AreEqual(1, ((IList)validate.Invoke(window, null)).Count, path + ": an unknown value is reported");
                    strings++;
                }
                Assert.AreEqual(12, strings, "every option string is validated");
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        [Test]
        public void BuildGuard_ReportsTheSearchDemo_InADevelopmentBuildOnly()
        {
            var sw = DevFeatureBuildGuard.Registry.Single(r => r.Name == "Add search demo");
            Assert.IsFalse(sw.ActiveInReleaseBuild, "release builds ignore it (SearchDemoSpawner.IsAllowed)");
            Assert.AreEqual(SearchDemoSpawner.IsAllowed(false, false), sw.ActiveInReleaseBuild);
            var on = new WallConfigData { search_demo = new SearchDemoSettings { enabled = true } };
            Assert.IsTrue(sw.IsOn(on));
            Assert.IsFalse(sw.IsOn(new WallConfigData()));
            StringAssert.Contains("Select, Filter & Search > Test", sw.HowToDisable);
        }

        [Test]
        public void NextFreeSearchFieldKey_NeverRepeatsAKey()
        {
            var fields = new List<SearchFieldDefinition> { new() { key = "field_1" }, new() { key = "field_3" } };
            Assert.AreEqual("field_2", POIEditorToolWindow.NextFreeSearchFieldKey(fields));
            fields.Add(new SearchFieldDefinition { key = "field_2" });
            Assert.AreEqual("field_4", POIEditorToolWindow.NextFreeSearchFieldKey(fields));
        }

        [Test]
        public void LiveReadout_SaysWhatTheSearchShows()
        {
            var poi = new POIData { id = "a", name = "Old Tower" };
            var state = new ResultSetState { Active = true };
            state.Results.Add(new POISearchIndex.SearchResult(poi, 0.9f, "name, partial word"));
            string text = POIEditorToolWindow.SearchLiveReadout("towe", 2, 26, 140, state);
            StringAssert.Contains("Searchable points: 26 (140 indexed words)", text);
            StringAssert.Contains("Query: 'towe'", text);
            StringAssert.Contains("Active filters: 2", text);
            StringAssert.Contains("1. Old Tower  0.90 (name, partial word)", text);
            StringAssert.Contains("none shown", POIEditorToolWindow.SearchLiveReadout("", 0, 26, 140, new ResultSetState()));

            var typo = SearchDemoLayout.TestCases().Single(c => c.Query == "lantren");
            var verdicts = new List<SearchDemoCheck.Verdict> { new(typo, true, true), new(typo, true, false) };
            string withDemo = POIEditorToolWindow.SearchLiveReadout("", 0, 40, 200, new ResultSetState(), verdicts);
            StringAssert.Contains("Demo Test Cases: 1/2 as expected", withDemo);
            StringAssert.Contains("[ok] 'lantren' finds Lantern Tower", withDemo);
            StringAssert.Contains("[!!] 'lantren' does not find Lantern Tower -- it should", withDemo);
        }

        [Test]
        public void EveryDemoTestCaseQuery_IsInThePlaymodeGuide()
        {
            string guide = Text("SearchPlaymodeTestGuide");
            foreach (var c in SearchDemoLayout.TestCases())
                StringAssert.Contains("'" + c.Query + "'", guide, "the guide names test case '" + c.Query + "'");
            StringAssert.Contains("Demo Query", guide);
            StringAssert.Contains("Demo Test Cases", guide);
        }

        // ---- the real OnGUI ----

        private sealed class Harness : EditorWindow
        {
            public static bool DidDraw;
            public static Exception Thrown;
            public static Action<WallConfigData> Setup;
            public static string ConfigBefore, ConfigAfter;
            public static string Method = "DrawGlobalSearchFilterSection";
            public static object[] Args;

            private void OnGUI()
            {
                DidDraw = true;
                Thrown = null;
                var window = CreateInstance<POIEditorToolWindow>();
                try
                {
                    var config = new WallConfigData { pois = new List<POIData> { new() { id = "p", name = "P" } } };
                    Setup?.Invoke(config);
                    typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(window, config);
                    ConfigBefore = JsonUtility.ToJson(config);
                    object[] args = Args == null ? null : new object[] { config.pois[0] };
                    try { typeof(POIEditorToolWindow).GetMethod(Method, Instance).Invoke(window, args); }
                    catch (TargetInvocationException ex)
                    {
                        if (!(ex.InnerException is ExitGUIException)) Thrown = ex.InnerException ?? ex;
                    }
                    ConfigAfter = JsonUtility.ToJson(config);
                }
                finally { DestroyImmediate(window); }
            }
        }

        private static IEnumerator DrawOnce(Action<WallConfigData> setup, string method = "DrawGlobalSearchFilterSection", bool poiArg = false)
        {
            Harness.Setup = setup;
            Harness.Method = method;
            Harness.Args = poiArg ? new object[1] : null;
            Harness.DidDraw = false;
            var window = EditorWindow.GetWindow<Harness>(true, "SearchFilterHarness");
            window.position = new Rect(0f, 0f, 700f, 1400f);
            window.Repaint();
            for (int i = 0; i < 60 && !Harness.DidDraw; i++)
                yield return null;
            window.Close();
            Assert.IsTrue(Harness.DidDraw, "the harness OnGUI ran");
            Assert.IsNull(Harness.Thrown, method + " must not throw: " + Harness.Thrown);
        }

        [UnityTest]
        public IEnumerator Section_DrawsWithoutThrowing_InEveryBranch(
            [Values(true, false)] bool enabled, [Values(true, false)] bool conditionalRowsOn, [Values(true, false)] bool demo)
        {
            yield return DrawOnce(c =>
            {
                var s = c.select_filter_search;
                s.enabled = enabled;
                s.selection.highlight_enabled = conditionalRowsOn;
                s.selection.zoom.enabled = conditionalRowsOn;
                s.filter.mismatch = conditionalRowsOn ? "dim" : "hide";
                s.results.suggestions_enabled = conditionalRowsOn;
                s.minimap.enabled = conditionalRowsOn;
                s.minimap.bounds_mode = conditionalRowsOn ? "manual" : "auto";
                s.voice.enabled = conditionalRowsOn;
                c.search_fields.Add(new SearchFieldDefinition { key = "era", label = "Era" });
                c.synonym_groups.Add(new SynonymGroup { key = "church", synonyms = new List<string> { "chapel" } });
                c.search_demo.enabled = demo;
            });
        }

        [UnityTest]
        public IEnumerator PoiSearchKeywords_Drawing_NeverWritesTheConfig()
        {
            yield return DrawOnce(c =>
            {
                c.search_fields.Add(new SearchFieldDefinition { key = "era", label = "Era", forced = true });
                c.pois[0].search_keyword_fields.Clear();      // the POI has no list for 'era' yet
            }, "DrawPoiSearchKeywordsField", poiArg: true);
            Assert.AreEqual(Harness.ConfigBefore, Harness.ConfigAfter,
                "drawing a POI's Search Keywords must not add an empty keyword list (it used to, on every draw)");
        }
    }
}
