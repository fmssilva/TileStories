using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TileStories.Editor.Tests
{
    // Live Play Mode sync, FIELD BY FIELD, for the Marker / Badge / Outline / Effects / Orientation
    // domains and every per-POI field: one test case per Editor Tab field. Each case spawns a REAL
    // WallSession with REAL POI_Marker prefabs on the shipped wall config, edits the authoring config
    // exactly as the field would, pushes it through the SAME applier list the window uses
    // (LivePlayModeConfigPush.CreateDispatcher) and asserts the running marker really changed -- a
    // snapshot of its images, label, ring, effect parameters and orientation settings, read off the
    // real components. The completeness test makes every serialized field either a row here or an
    // explicit, reasoned exclusion, so a new config field cannot silently skip the live path.
    public class LiveSyncFieldMatrixTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string DefaultPoi = "lamp";      // shipped: level_1 (label, ripple discs, halo ring, pulse, spin), badge + status
        private const BindingFlags AllInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly List<UnityEngine.Object> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _created) if (o != null) UnityEngine.Object.DestroyImmediate(o);
            _created.Clear();
            MarkerHierarchyResolver.ResetToDefaults();
            StatusRamp.ResetToDefaults();
            CategoryPalette.ClearOverrides();
            BadgeCategoryPalette.Clear();
        }

        // ---------------- the matrix ----------------

        public sealed class Row
        {
            public string Field;                       // the config path, e.g. "badge_corner"
            public Action<WallConfigData> Setup;       // optional: state the field needs, before spawn
            public Action<WallConfigData> Edit;        // the edit the Editor Tab field makes
            public string PoiId = DefaultPoi;
            public override string ToString() => Field;
        }

        private static Row R(string field, Action<WallConfigData> edit, Action<WallConfigData> setup = null, string poi = DefaultPoi) =>
            new Row { Field = field, Edit = edit, Setup = setup, PoiId = poi };

        private static POIData P(WallConfigData c, string id = DefaultPoi) => c.pois.First(p => p.id == id);
        private static HierarchyLevelEntry L(WallConfigData c, string id = DefaultPoi) => c.hierarchy_levels.First(l => l.key == P(c, id).hierarchy_level_key);
        private static CategoryStyleEntry Cat(WallConfigData c) => c.category_styles.First(e => e.category == P(c).category);
        private static BadgeCategoryEntry Badge(WallConfigData c) => c.badge_categories.First(e => e.key == P(c).badge_category);
        private static OutlineLevelEntry Status(WallConfigData c) => c.outline_levels.First(e => e.key == P(c).status_level_key);

        // Put the default POI's level on one ripple/halo choice so that effect's parameters are live on it
        private static Action<WallConfigData> Uses(string ripple, string halo) => c => { L(c).ripple_effect = ripple; L(c).halo_effect = halo; L(c).pulse = true; };

        private static IEnumerable<Row> MarkerRows()
        {
            yield return R("marker_shape", c => c.marker_shape = "hexagon");
            yield return R("icon_color_hex", c => c.icon_color_hex = "#FF00FF");
            yield return R("icon_size_ratio", c => c.icon_size_ratio = 0.85f);
            yield return R("label_gap_ratio", c => c.label_gap_ratio = 0.4f);
            yield return R("label_font_size_ratio", c => c.label_font_size_ratio = 0.6f);
            yield return R("label_font_key", c => c.label_font_key = "roboto_bold");
            yield return R("category_styles[].color_hex", c => Cat(c).color_hex = "#00FF00");
            yield return R("category_styles[].icon_key", c => Cat(c).icon_key = "IconMilitary");
        }

        private static IEnumerable<Row> BadgeRows()
        {
            yield return R("marker_use_badge", c => c.marker_use_badge = false);
            yield return R("badge_shape", c => c.badge_shape = "hexagon");
            yield return R("badge_corner", c => c.badge_corner = "bottom_left");
            yield return R("badge_size_ratio", c => c.badge_size_ratio = 0.48f);
            yield return R("badge_categories[].color_hex", c => Badge(c).color_hex = "#00FFFF");
            yield return R("badge_categories[].icon_key", c => Badge(c).icon_key = "IconMilitary");
        }

        private static IEnumerable<Row> OutlineRows()
        {
            yield return R("marker_outline_mode(none)", c => c.marker_outline_mode = "none");
            yield return R("marker_outline_mode(same_hue)", c => c.marker_outline_mode = "same_hue");
            yield return R("marker_outline_mode(per_type)", c => c.marker_outline_mode = "per_type", setup: c => Status(c).color_hex = "#00FF00");
            yield return R("outline_uniform_color_hex", c => c.outline_uniform_color_hex = "#0000FF", setup: c => c.marker_outline_mode = "uniform");
            yield return R("ring_size_ratio", c => c.ring_size_ratio = 1.32f);
            yield return R("contour_spin_deg_per_s", c => c.contour_spin_deg_per_s = 200f);
            yield return R("outline_levels[].line_style", c => Status(c).line_style = "dotted");
            yield return R("outline_levels[].color_hex", c => Status(c).color_hex = "#FF0000", setup: c => c.marker_outline_mode = "per_type");
            yield return R("outline_levels[].pct", c => Status(c).pct = 90f, setup: c => c.marker_outline_mode = "same_hue");
        }

        private static IEnumerable<Row> EffectRows()
        {
            yield return R("effect_defaults.effects_enabled", c => c.effect_defaults.effects_enabled = false);
            // Every parameter of every effect block, walked by reflection so a new one is covered automatically
            var blocks = new (string block, Action<WallConfigData> uses)[]
            {
                ("pulse", Uses("ripple_discs", "halo_ring")),
                ("ripple_rings", Uses("ripple_rings", "halo_ring")),
                ("ripple_discs", Uses("ripple_discs", "halo_ring")),
                ("halo_ring", Uses("ripple_discs", "halo_ring")),
                ("halo_disc", Uses("ripple_discs", "halo_disc")),
                ("beacon", Uses("ripple_discs", "beacon")),
            };
            foreach (var (block, uses) in blocks)
            {
                var blockField = typeof(EffectDefaults).GetField(block);
                foreach (var f in blockField.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    var field = f;
                    yield return R("effect_defaults." + block + "." + field.Name, c => Nudge(field, blockField.GetValue(c.effect_defaults)), setup: uses);
                }
            }
        }

        private static IEnumerable<Row> LevelRows()
        {
            yield return R("hierarchy_levels[].ripple_effect", c => L(c).ripple_effect = "ripple_rings");
            yield return R("hierarchy_levels[].halo_effect", c => L(c).halo_effect = "beacon");
            yield return R("hierarchy_levels[].pulse", c => L(c).pulse = false);
            yield return R("hierarchy_levels[].rotate_contour", c => L(c).rotate_contour = false);
            yield return R("hierarchy_levels[].size_cm", c => L(c).size_cm = 12f);
            yield return R("hierarchy_levels[].show_label", c => L(c).show_label = false);
            yield return R("hierarchy_levels[].facing_mode_override", c => L(c).facing_mode_override = "wall_fixed");
            yield return R("hierarchy_levels[].override_label_style", c => L(c).override_label_style = true,
                setup: c => { L(c).label_gap_ratio = 0.5f; L(c).label_font_size_ratio = 0.9f; L(c).label_font_key = "oswald_bold"; });
            var withOverride = (Action<WallConfigData>)(c => { L(c).override_label_style = true; L(c).label_gap_ratio = 0.2f; L(c).label_font_size_ratio = 0.3f; L(c).label_font_key = "roboto_bold"; });
            yield return R("hierarchy_levels[].label_gap_ratio", c => L(c).label_gap_ratio = 0.6f, setup: withOverride);
            yield return R("hierarchy_levels[].label_font_size_ratio", c => L(c).label_font_size_ratio = 0.9f, setup: withOverride);
            yield return R("hierarchy_levels[].label_font_key", c => L(c).label_font_key = "oswald_bold", setup: withOverride);
        }

        private static IEnumerable<Row> OrientationRows()
        {
            // - two valid values per field: the row writes whichever differs from the shipped config, so a
            //   developer re-authoring the wall can never turn the edit into a silent no-op
            var alternatives = new Dictionary<string, string[]>
            {
                { "vertical_alignment_mode", new[] { "screen_up", "world_up" } },
                { "label_vertical_alignment_mode", new[] { "world_up", "screen_up" } },
                { "badge_vertical_alignment_mode", new[] { "world_up", "screen_up" } },
                { "cluster_vertical_alignment_mode", new[] { "world_up", "screen_up" } },
                { "up_reference", new[] { "custom", "world_gravity" } },
                { "facing_mode", new[] { "yaw_only", "always_facing_camera" } },
                { "facing_basis", new[] { "camera_position", "view_plane" } },
                { "update_mode", new[] { "interval", "every_frame" } },
            };
            foreach (var f in typeof(OrientationSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (f.Name == "edit_mode_preview_enabled") continue;   // excluded: Scene view only
                var field = f;
                if (field.FieldType == typeof(string))
                {
                    Assert.IsTrue(alternatives.ContainsKey(field.Name), "no live-sync row value for orientation field " + field.Name);
                    string[] options = alternatives[field.Name];
                    yield return R("orientation_settings." + field.Name, c =>
                        field.SetValue(c.orientation_settings,
                            (string)field.GetValue(c.orientation_settings) == options[0] ? options[1] : options[0]));
                }
                else yield return R("orientation_settings." + field.Name, c => Nudge(field, c.orientation_settings));
            }
        }

        private static IEnumerable<Row> PoiRows()
        {
            yield return R("pois[].name", c => P(c).name = "Renamed live");
            yield return R("pois[].category", c => P(c).category = "military");
            yield return R("pois[].hierarchy_level_key", c => P(c).hierarchy_level_key = "level_5");
            yield return R("pois[].has_custom_symbol", c => P(c).has_custom_symbol = true, setup: c => P(c).custom_symbol_key = "IconMilitary");
            yield return R("pois[].custom_symbol_key", c => P(c).custom_symbol_key = "IconReligious",
                setup: c => { P(c).has_custom_symbol = true; P(c).custom_symbol_key = "IconMilitary"; });
            yield return R("pois[].badge_category", c => P(c).badge_category = "destroyed");
            yield return R("pois[].has_status", c => P(c).has_status = false);
            yield return R("pois[].status_level_key", c => P(c).status_level_key = "destroyed");
            yield return R("pois[].status_pct", c => P(c).status_pct = 100f, setup: c => P(c).status_level_key = null);
            // What ticking Status unknown does (ApplyUnknownStatusDefaults; pinned by POIEditorAddPoiTests)
            yield return R("pois[].status_unknown", c => { P(c).status_unknown = true; P(c).status_level_key = "unknown"; P(c).badge_category = "unknown_damage"; });
            yield return R("pois[].editor_rotation_x_deg", c => P(c).editor_rotation_x_deg = 25f);
            yield return R("pois[].editor_rotation_deg", c => P(c).editor_rotation_deg = 40f);
            yield return R("pois[].editor_rotation_z_deg", c => P(c).editor_rotation_z_deg = 15f);
        }

        public static IEnumerable<TestCaseData> AllRows()
        {
            foreach (var row in MarkerRows().Concat(BadgeRows()).Concat(OutlineRows()).Concat(EffectRows())
                         .Concat(LevelRows()).Concat(OrientationRows()).Concat(PoiRows()))
                yield return new TestCaseData(row).SetName("LiveSync_" + row.Field);
        }

        // Nudge one numeric/bool/colour field of a settings object to a different, in-range value
        private static void Nudge(FieldInfo field, object owner)
        {
            object value = field.GetValue(owner);
            if (field.FieldType == typeof(bool)) field.SetValue(owner, !(bool)value);
            else if (field.FieldType == typeof(float)) field.SetValue(owner, (float)value + 0.05f);
            else if (field.FieldType == typeof(string)) field.SetValue(owner, "#123456");   // tint_color_hex
            else Assert.Fail("no live-sync nudge for " + field.FieldType + " " + field.Name);
        }

        [TestCaseSource(nameof(AllRows))]
        public void LiveEdit_ReachesTheRunningMarker(Row row)
        {
            var authoring = LoadShippedConfig();
            row.Setup?.Invoke(authoring);
            MarkerVisualSettings.ApplyPalettes(authoring);
            var session = NewSessionOn(Copy(authoring));
            var dispatcher = LivePlayModeConfigPush.CreateDispatcher();
            dispatcher.Push(session, authoring);   // first push of a Play session: every domain once

            var marker = session.SpawnedMarkers.First(m => m.name == row.PoiId);
            string before = Snapshot(marker);

            row.Edit(authoring);
            var applied = dispatcher.Push(session, authoring);

            Assert.IsNotEmpty(applied, row.Field + ": the edit must trigger a live push (no applier fingerprints it)");
            string after = Snapshot(marker);
            Assert.AreNotEqual(before, after, row.Field + ": pushed (" + string.Join(", ", applied) + ") but the running marker did not change");
        }

        // ---------------- completeness ----------------

        // Serialized fields that are deliberately NOT part of this live marker matrix, with the reason
        private static readonly Dictionary<string, string> Excluded = new()
        {
            // identity / notes / search: no marker visual
            { "category_styles[].category", "identity key: a rename rewrites every POI to the new key, the marker looks the same" },
            { "badge_categories[].key", "identity key (same as category)" },
            { "badge_categories[].label", "not shown on markers" },
            { "outline_levels[].key", "identity key, generated by the table" },
            { "outline_levels[].label", "only the Status level dropdown's text" },
            { "hierarchy_levels[].key", "identity key, generated by the table" },
            { "hierarchy_levels[].level_name", "only the Hierarchy Level dropdown's text (grid cells show the base marker's name)" },
            { "hierarchy_levels[].priority", "sort key for LOD/overlap, not a marker visual" },
            { "hierarchy_levels[].reveal_delay_s", "reveal plays once at spawn; live edits reach the demo grid (EffectsPreviewSpawnerTests)" },
            { "hierarchy_levels[].reveal_duration_s", "reveal plays once at spawn; live edits reach the demo grid (EffectsPreviewSpawnerTests)" },
            { "*.details", "developer note (Details popup), not a visual" },
            { "*.search_keywords", "search index: Select, Filter & Search (SearchFilterEditorTabTests fingerprints, SearchSceneTests)" },
            { "pois[].id", "identity" },
            { "pois[].position", "Marker Positioning domain: moved in the Scene rig, read at spawn" },
            { "pois[].position_verified", "editor-only QA flag, never read at runtime" },
            { "pois[].summary", "content card text, not the marker" },
            { "pois[].search_keyword_fields", "search index: Select, Filter & Search (SearchFilterEditorTabTests fingerprints)" },
            // window-managed paths and dev-only grids
            { "marker_icon_library_resources_path", "set by the window when it creates/assigns the wall icon library" },
            { "label_font_library_resources_path", "set by the window when it creates/assigns the wall font library" },
            { "outline_preview", "dev-only demo grid: its live rebuild needs Destroy, proven in PlayMode (OutlinePreviewRenderTests)" },
            { "effect_defaults.preview", "dev-only demo grid: live rebuild proven in PlayMode (EffectsPreviewSpawnerTests)" },
            { "orientation_settings.edit_mode_preview_enabled", "Scene view only: no runtime code reads it" },
            // other domains (not live in Play Mode, own docs)
            { "wall_id", "wall identity" }, { "wall_name", "wall identity" }, { "immersal_map_id", "tracking" },
            // Displacement acts through the LOD cycle, not on a marker's look: field by field on real markers in
            // DisplacementSettingsRealMarkerTests / DisplacementDemoTests (PlayMode), fingerprints in DisplacementEditorTabTests
            { "displacement_settings", "DisplacementSettingsRealMarkerTests" }, { "displacement_demo", "DisplacementDemoTests" },
            // LOD, Zoom and the LOD demo field: field by field in LodZoomLiveSyncTests (not marker visuals)
            { "lod_settings", "LodZoomLiveSyncTests" }, { "zoom_settings", "LodZoomLiveSyncTests" }, { "demo_field", "LodZoomLiveSyncTests" },
            // Select, Filter & Search acts on the search UI and the selection channel, not on a marker's look:
            // every field live in SearchFilterEditorTabTests (fingerprints) and SearchSceneTests / SearchDemoTests (PlayMode)
            { "select_filter_search", "SearchSceneTests" }, { "search_demo", "SearchDemoTests" },
            { "search_fields", "SearchFilterEditorTabTests" }, { "synonym_groups", "SearchFilterEditorTabTests" },
        };

        [Test]
        public void EveryConfigField_IsEitherALiveSyncRow_OrAReasonedExclusion()
        {
            var covered = new HashSet<string>(AllRows().Select(t => ((Row)t.Arguments[0]).Field.Split('(')[0]));
            var missing = new List<string>();

            void Check(string path)
            {
                if (covered.Contains(path) || Excluded.ContainsKey(path)) return;
                string leaf = path.Substring(path.LastIndexOf('.') + 1);
                if (leaf.Contains("]")) leaf = path.Substring(path.IndexOf("].", StringComparison.Ordinal) + 2);
                if (Excluded.ContainsKey("*." + leaf)) return;
                missing.Add(path);
            }

            void Walk(Type type, string prefix)
            {
                foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    string path = prefix + f.Name;
                    if (covered.Contains(path) || Excluded.ContainsKey(path)) continue;
                    var ft = f.FieldType;
                    bool isList = ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(List<>);
                    var element = isList ? ft.GetGenericArguments()[0] : null;
                    if (isList && element.IsClass && element != typeof(string) && element.Namespace == typeof(WallConfigData).Namespace)
                        Walk(element, path + "[].");
                    else if (!isList && ft.IsClass && ft != typeof(string) && ft.Namespace == typeof(WallConfigData).Namespace)
                        Walk(ft, path + ".");
                    else
                        Check(path);
                }
            }

            Walk(typeof(WallConfigData), "");
            Assert.IsEmpty(missing, "Config fields with no live-sync row and no exclusion reason:\n" + string.Join("\n", missing));
        }

        // ---------------- harness ----------------

        private static WallConfigData LoadShippedConfig()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json");
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(path));
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");
            config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings();   // demo grids need Destroy: PlayMode only
            config.outline_preview = new OutlinePreviewSettings();
            return config;
        }

        private static WallConfigData Copy(WallConfigData c) => JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(c));

        private WallSession NewSessionOn(WallConfigData config)
        {
            var go = new GameObject("LiveMatrixSession");
            go.SetActive(false);
            _created.Add(go);
            var ws = go.AddComponent<WallSession>();
            var anchor = new GameObject("LiveMatrixAnchor");
            _created.Add(anchor);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath);
            Assert.IsNotNull(prefab, "POI_Marker prefab must exist.");
            Set(ws, "_config", config);
            Set(ws, "_effectDefaults", config.effect_defaults);
            Set(ws, "poiAnchorPrefab", prefab);
            Set(ws, "correctionAnchor", anchor.transform);
            typeof(WallSession).GetMethod("SpawnPOIs", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ws, null);
            foreach (var m in ws.SpawnedMarkers) _created.Add(m.transform.root.gameObject);
            return ws;
        }

        private static void Set(object obj, string field, object value) =>
            obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);

        // Everything a developer could see or a later frame would use, read off the real components
        private static string Snapshot(MarkerView marker)
        {
            var sb = new StringBuilder();
            var root = marker.transform;
            foreach (string child in new[] { "Symbol", "Symbol/Icon", "Ring", "Badge", "Badge/Icon" })
            {
                var t = root.Find(child);
                if (t == null) { sb.Append(child).Append("=missing|"); continue; }
                var img = t.GetComponent<Image>();
                var rt = (RectTransform)t;
                sb.Append(child).Append('=').Append(t.gameObject.activeInHierarchy).Append(',')
                  .Append(img != null && img.enabled).Append(',').Append(img != null && img.sprite != null ? img.sprite.name : "-").Append(',')
                  .Append(img != null ? img.color.ToString("F3") : "-").Append(',')
                  .Append(rt.sizeDelta.ToString("F4")).Append(rt.anchoredPosition.ToString("F4"))
                  .Append(rt.anchorMin.ToString("F3")).Append(rt.anchorMax.ToString("F3")).Append('|');
            }

            var label = root.Find("Label");
            var tmp = label != null ? label.GetComponent<TextMeshProUGUI>() : null;
            if (tmp != null)
                sb.Append("Label=").Append(label.gameObject.activeSelf).Append(',').Append(tmp.text).Append(',')
                  .Append(tmp.fontSize.ToString("F4")).Append(',').Append(tmp.font != null ? tmp.font.name : "-").Append(',')
                  .Append(((RectTransform)label).anchoredPosition.ToString("F4")).Append('|');

            foreach (var component in new MonoBehaviour[]
                     {
                         root.GetComponentInChildren<MarkerRingView>(true), marker.GetComponent<MarkerPulseEffect>(),
                         marker.GetComponent<MarkerRippleEffect>(), marker.GetComponent<MarkerHaloEffect>(),
                     })
                if (component != null) sb.Append(DumpFields(component));

            var billboard = marker.GetComponentInChildren<MarkerBillboard>();
            if (billboard != null)
                sb.Append("Billboard=").Append(JsonUtility.ToJson(billboard.ConfiguredSettings)).Append(',')
                  .Append(billboard.ConfiguredModeOverride).Append(',').Append(billboard.AuthoredLocalRotation.ToString("F4")).Append('|');
            return sb.ToString();
        }

        // A component's own value fields (parameters, flags, tints), whatever their visibility
        private static string DumpFields(MonoBehaviour component)
        {
            var sb = new StringBuilder(component.GetType().Name).Append('{');
            for (var type = component.GetType(); type != null && type != typeof(MonoBehaviour); type = type.BaseType)
                foreach (var f in type.GetFields(AllInstance | BindingFlags.DeclaredOnly))
                {
                    var ft = f.FieldType;
                    if (ft.IsPrimitive || ft.IsEnum || ft == typeof(string) || ft == typeof(Color) || ft == typeof(Vector2) || ft == typeof(Vector3))
                        sb.Append(f.Name).Append('=').Append(f.GetValue(component)).Append(';');
                }
            return sb.Append("}|").ToString();
        }
    }
}
