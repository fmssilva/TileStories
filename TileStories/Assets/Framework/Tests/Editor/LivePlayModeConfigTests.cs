using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Live Play Mode config: the dispatcher's "who has to hear about this change" rule, and the
    // Effects applier driving a REAL WallSession with REAL POI_Marker prefabs (built the same way
    // OrientationWallSessionIntegrationTests builds one). The preview-grid rebuild needs Destroy and
    // is proven in PlayMode (EffectsPreviewSpawnerTests); these tests keep the grid off.
    public class LivePlayModeConfigTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        private readonly List<Object> _created = new();

        [TearDown]
        public void TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            foreach (var o in _created) if (o != null) Object.DestroyImmediate(o);
            _created.Clear();
        }

        private static WallConfigData LoadShippedConfig()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json");
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(path));
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");
            config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings();
            return config;
        }

        private static WallConfigData Copy(WallConfigData c) =>
            JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(c));

        private WallSession NewSession() => NewSessionOn(null);

        private WallSession NewSessionOn(WallConfigData config)
        {
            var go = new GameObject("LiveTestSession");
            go.SetActive(false);
            _created.Add(go);
            var ws = go.AddComponent<WallSession>();
            if (config == null) return ws;

            var anchor = new GameObject("LiveTestAnchor");
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

        // ---------------- the dispatcher ----------------

        // Real implementation of the applier interface that counts its calls.
        private class CountingApplier : ILivePlayModeApplier
        {
            private readonly System.Func<WallConfigData, string> _fingerprint;
            public int Calls;
            public WallConfigData LastCopy;
            public CountingApplier(string name, System.Func<WallConfigData, string> fingerprint)
            { Name = name; _fingerprint = fingerprint; }
            public string Name { get; }
            public string Fingerprint(WallConfigData c) => _fingerprint(c);
            public void Apply(WallSession s, WallConfigData copy) { Calls++; LastCopy = copy; }
        }

        [Test]
        public void Dispatcher_FirstPushToAWall_AppliesEveryDomain()
        {
            var a = new CountingApplier("a", c => c.wall_name);
            var b = new CountingApplier("b", c => c.immersal_map_id.ToString());
            var d = new LivePlayModeConfigDispatcher(new[] { a, b });

            var applied = d.Push(NewSession(), LoadShippedConfig());

            CollectionAssert.AreEqual(new[] { "a", "b" }, applied);
            Assert.AreEqual(1, a.Calls);
            Assert.AreEqual(1, b.Calls);
        }

        [Test]
        public void Dispatcher_IdenticalSecondPush_AppliesNothing()
        {
            var a = new CountingApplier("a", c => c.wall_name);
            var d = new LivePlayModeConfigDispatcher(new[] { a });
            var session = NewSession();
            var config = LoadShippedConfig();

            d.Push(session, config);
            var applied = d.Push(session, config);

            Assert.IsEmpty(applied);
            Assert.AreEqual(1, a.Calls, "An unchanged domain must not be re-applied.");
        }

        [Test]
        public void Dispatcher_OnlyTheDomainWhoseFingerprintChanged_IsApplied()
        {
            var a = new CountingApplier("a", c => c.wall_name);
            var b = new CountingApplier("b", c => c.immersal_map_id.ToString());
            var d = new LivePlayModeConfigDispatcher(new[] { a, b });
            var session = NewSession();
            var config = LoadShippedConfig();
            d.Push(session, config);

            config.wall_name += "_edited";
            var applied = d.Push(session, config);

            CollectionAssert.AreEqual(new[] { "a" }, applied);
            Assert.AreEqual(2, a.Calls);
            Assert.AreEqual(1, b.Calls);
        }

        [Test]
        public void Dispatcher_ANewWall_GetsEveryDomainAgain()
        {
            var a = new CountingApplier("a", c => c.wall_name);
            var d = new LivePlayModeConfigDispatcher(new[] { a });
            var config = LoadShippedConfig();
            d.Push(NewSession(), config);

            var applied = d.Push(NewSession(), config);

            Assert.AreEqual(1, applied.Count, "A new Play Mode run knows nothing of earlier pushes.");
            Assert.AreEqual(2, a.Calls);
        }

        [Test]
        public void Dispatcher_HandsTheApplierAPrivateCopy_NotTheAuthoringObject()
        {
            var a = new CountingApplier("a", c => c.wall_name);
            var config = LoadShippedConfig();

            new LivePlayModeConfigDispatcher(new[] { a }).Push(NewSession(), config);

            Assert.IsNotNull(a.LastCopy);
            Assert.AreNotSame(config, a.LastCopy);
            Assert.AreNotSame(config.effect_defaults, a.LastCopy.effect_defaults,
                "The running wall must never share objects with the window's config.");
            Assert.AreEqual(JsonUtility.ToJson(config), JsonUtility.ToJson(a.LastCopy), "The copy must be faithful.");
        }

        [Test]
        public void Dispatcher_NullWallOrConfig_DoesNothing()
        {
            var a = new CountingApplier("a", c => c.wall_name);
            var d = new LivePlayModeConfigDispatcher(new[] { a });

            Assert.IsEmpty(d.Push(null, LoadShippedConfig()));
            Assert.IsEmpty(d.Push(NewSession(), null));
            Assert.AreEqual(0, a.Calls);
        }

        [Test]
        public void PushToRunningWall_InEditMode_DoesNothing()
        {
            var config = LoadShippedConfig();
            var session = NewSessionOn(Copy(config));
            var marker = session.SpawnedMarkers[0];
            bool pulseBefore = marker.GetComponent<MarkerPulseEffect>().IsActive;
            config.effect_defaults.effects_enabled = false;

            Assert.IsFalse(Application.isPlaying, "Precondition: EditMode.");
            LivePlayModeConfigPush.PushToRunningWall(config);

            Assert.AreEqual(pulseBefore, marker.GetComponent<MarkerPulseEffect>().IsActive,
                "Live pushes exist for Play Mode only.");
        }

        // ---------------- the Effects applier ----------------

        [Test]
        public void EffectsFingerprint_ChangesForEffectSettingsAndLevelEffectColumns_NotForOtherSections()
        {
            var applier = new LivePlayModeEffectsApplier();
            var config = LoadShippedConfig();
            string baseline = applier.Fingerprint(config);

            config.effect_defaults.pulse.enabled = !config.effect_defaults.pulse.enabled;
            Assert.AreNotEqual(baseline, applier.Fingerprint(config), "effect definition");
            config.effect_defaults.pulse.enabled = !config.effect_defaults.pulse.enabled;
            Assert.AreEqual(baseline, applier.Fingerprint(config), "reverting must restore the fingerprint");

            config.effect_defaults.preview.enabled = true;
            Assert.AreNotEqual(baseline, applier.Fingerprint(config), "focus grid switch");
            config.effect_defaults.preview.enabled = false;

            config.hierarchy_levels[0].halo_effect = config.hierarchy_levels[0].halo_effect == "beacon" ? "none" : "beacon";
            Assert.AreNotEqual(baseline, applier.Fingerprint(config), "level effect column");
            config = LoadShippedConfig();

            config.wall_name += "_x";
            config.immersal_map_id++;
            Assert.AreEqual(baseline, applier.Fingerprint(config), "other sections must not trigger an effects push");
        }

        [Test]
        public void EffectsApplier_DrivesRealMarkers_ThroughEverySwitchAndLevelChoice()
        {
            // The running wall gets its own copy, exactly like a real Play Mode run reading the file.
            var authoring = LoadShippedConfig();
            var level = authoring.hierarchy_levels[0];
            level.pulse = true;
            level.ripple_effect = "ripple_rings";
            level.halo_effect = "beacon";
            authoring.effect_defaults.effects_enabled = true;
            MarkerHierarchyResolver.Configure(authoring.hierarchy_levels);
            var session = NewSessionOn(Copy(authoring));

            var poi = authoring.pois.First(p => p.hierarchy_level_key == level.key);
            var marker = session.SpawnedMarkers.First(m => m.name == poi.id).gameObject;
            var dispatcher = new LivePlayModeConfigDispatcher(new ILivePlayModeApplier[] { new LivePlayModeEffectsApplier() });

            void AssertRuns(bool pulse, MarkerRippleEffect.RippleStyle? ripple, MarkerHaloEffect.HaloVariant? halo, string step)
            {
                Assert.AreEqual(pulse, marker.GetComponent<MarkerPulseEffect>().IsActive, step + ": pulse");
                var r = marker.GetComponent<MarkerRippleEffect>();
                Assert.AreEqual(ripple.HasValue, r.IsActive, step + ": ripple active");
                if (ripple.HasValue) Assert.AreEqual(ripple.Value, r.CurrentStyle, step + ": ripple style");
                var h = marker.GetComponent<MarkerHaloEffect>();
                Assert.AreEqual(halo.HasValue, h.IsActive, step + ": halo active");
                if (halo.HasValue) Assert.AreEqual(halo.Value, h.CurrentVariant, step + ": halo variant");
            }

            AssertRuns(true, MarkerRippleEffect.RippleStyle.Rings, MarkerHaloEffect.HaloVariant.Beacon, "spawned");

            // 1. master switch off
            authoring.effect_defaults.effects_enabled = false;
            Assert.AreEqual(1, dispatcher.Push(session, authoring).Count);
            AssertRuns(false, null, null, "master off");

            // 2. master back on, one effect's own checkbox off
            authoring.effect_defaults.effects_enabled = true;
            authoring.effect_defaults.ripple_rings.enabled = false;
            dispatcher.Push(session, authoring);
            AssertRuns(true, null, MarkerHaloEffect.HaloVariant.Beacon, "ripple_rings unchecked");

            // 3. level columns choose different effects
            authoring.effect_defaults.ripple_rings.enabled = true;
            authoring.hierarchy_levels[0].ripple_effect = "ripple_discs";
            authoring.hierarchy_levels[0].halo_effect = "halo_ring";
            authoring.hierarchy_levels[0].pulse = false;
            dispatcher.Push(session, authoring);
            AssertRuns(false, MarkerRippleEffect.RippleStyle.Discs, MarkerHaloEffect.HaloVariant.Ring, "level columns changed");

            // 4. a change to an unrelated section pushes nothing and leaves the markers alone
            authoring.wall_name += "_x";
            Assert.IsEmpty(dispatcher.Push(session, authoring));
            AssertRuns(false, MarkerRippleEffect.RippleStyle.Discs, MarkerHaloEffect.HaloVariant.Ring, "unrelated edit");

            // 5. the wall never shares objects with the window's config
            var wallDefaults = (EffectDefaults)typeof(WallSession).GetField("_effectDefaults", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(session);
            Assert.AreNotSame(authoring.effect_defaults, wallDefaults);
            authoring.effect_defaults.effects_enabled = false;   // edited but NOT pushed
            AssertRuns(false, MarkerRippleEffect.RippleStyle.Discs, MarkerHaloEffect.HaloVariant.Ring, "edited without a push");
        }

        // ---------------- the Orientation applier ----------------

        [Test]
        public void OrientationFingerprint_ChangesForEveryOrientationField_AndLevelFacingOverride_NotForEffects()
        {
            var applier = new LivePlayModeOrientationApplier();
            string baseline = applier.Fingerprint(LoadShippedConfig());

            // Every public field of OrientationSettings, walked by reflection: a new field can never be forgotten.
            foreach (var field in typeof(OrientationSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var config = LoadShippedConfig();
                object value = field.GetValue(config.orientation_settings);
                if (field.FieldType == typeof(string)) field.SetValue(config.orientation_settings, value + "_x");
                else if (field.FieldType == typeof(float)) field.SetValue(config.orientation_settings, (float)value + 1f);
                else if (field.FieldType == typeof(bool)) field.SetValue(config.orientation_settings, !(bool)value);
                else Assert.Fail("Unhandled orientation field type " + field.FieldType + " for " + field.Name);
                Assert.AreNotEqual(baseline, applier.Fingerprint(config), "orientation field " + field.Name + " must change the fingerprint");
            }

            var levelEdit = LoadShippedConfig();
            levelEdit.hierarchy_levels[0].facing_mode_override = "wall_fixed";
            Assert.AreNotEqual(baseline, applier.Fingerprint(levelEdit), "level facing override");

            foreach (System.Action<POIData> edit in new System.Action<POIData>[]
                     { p => p.editor_rotation_x_deg += 5f, p => p.editor_rotation_deg += 5f, p => p.editor_rotation_z_deg += 5f })
            {
                var poiFacing = LoadShippedConfig();
                edit(poiFacing.pois[0]);
                Assert.AreNotEqual(baseline, applier.Fingerprint(poiFacing), "a POI's own Facing X/Y/Z must trigger an orientation push");
            }

            var unrelated = LoadShippedConfig();
            unrelated.effect_defaults.pulse.enabled = !unrelated.effect_defaults.pulse.enabled;
            unrelated.hierarchy_levels[0].pulse = !unrelated.hierarchy_levels[0].pulse;
            unrelated.hierarchy_levels[0].size_cm += 5f;
            unrelated.wall_name += "_x";
            Assert.AreEqual(baseline, applier.Fingerprint(unrelated), "effects / size / name edits must not trigger an orientation push");
        }

        [Test]
        public void OrientationApplier_RepointsRealMarkers_WithTheirOwnLevelOverride_AndDoesNotDisturbEffects()
        {
            var authoring = LoadShippedConfig();
            var levelA = authoring.hierarchy_levels[0];
            var poiA = authoring.pois.First(p => p.hierarchy_level_key == levelA.key);
            var poiB = authoring.pois.First(p => p.hierarchy_level_key != levelA.key);
            MarkerHierarchyResolver.Configure(authoring.hierarchy_levels);
            var session = NewSessionOn(Copy(authoring));
            var a = session.SpawnedMarkers.First(m => m.name == poiA.id);
            var b = session.SpawnedMarkers.First(m => m.name == poiB.id);
            var billboardA = a.GetComponentInChildren<MarkerBillboard>();
            var billboardB = b.GetComponentInChildren<MarkerBillboard>();
            var dispatcher = new LivePlayModeConfigDispatcher(new ILivePlayModeApplier[]
                { new LivePlayModeEffectsApplier(), new LivePlayModeOrientationApplier() });
            CollectionAssert.AreEquivalent(new[] { "effects", "orientation" }, dispatcher.Push(session, authoring), "first push reaches both domains");
            bool pulseA = a.GetComponent<MarkerPulseEffect>().IsActive;

            authoring.orientation_settings.facing_mode = "yaw_only";
            authoring.orientation_settings.label_vertical_alignment_mode = "screen_up";
            authoring.hierarchy_levels[0].facing_mode_override = "wall_fixed";
            var applied = dispatcher.Push(session, authoring);

            CollectionAssert.AreEqual(new[] { "orientation" }, applied, "only the orientation domain changed");
            Assert.AreEqual("yaw_only", billboardA.ConfiguredSettings.facing_mode);
            Assert.AreEqual("yaw_only", billboardB.ConfiguredSettings.facing_mode);
            Assert.AreEqual("screen_up", billboardA.ConfiguredSettings.label_vertical_alignment_mode);
            Assert.AreEqual("wall_fixed", billboardA.ConfiguredModeOverride, "the changed level's marker gets the override");
            Assert.AreEqual("", billboardB.ConfiguredModeOverride, "other levels keep no override");
            Assert.AreEqual(pulseA, a.GetComponent<MarkerPulseEffect>().IsActive, "effects untouched by an orientation push");
            Assert.AreNotSame(authoring.orientation_settings, billboardA.ConfiguredSettings, "the wall never shares the window's object");
        }

        // ---------------- the Marker applier ----------------

        [Test]
        public void MarkerFingerprint_ChangesForWallAndPoiMarkerFields_NotForEffectsOrFacing()
        {
            var applier = new LivePlayModeMarkerApplier();
            string baseline = applier.Fingerprint(LoadShippedConfig());

            void AssertChanges(System.Action<WallConfigData> mutate, string label)
            {
                var config = LoadShippedConfig();
                mutate(config);
                Assert.AreNotEqual(baseline, applier.Fingerprint(config), label);
            }

            AssertChanges(c => c.marker_shape = "hexagon", "marker_shape");
            AssertChanges(c => c.badge_shape = "hexagon", "badge_shape");
            AssertChanges(c => c.marker_outline_mode = "same_hue", "marker_outline_mode");
            AssertChanges(c => c.outline_uniform_color_hex = "#123456", "outline_uniform_color_hex");
            AssertChanges(c => { c.outline_preview ??= new OutlinePreviewSettings(); c.outline_preview.enabled = !c.outline_preview.enabled; }, "outline_preview.enabled");
            AssertChanges(c => c.marker_use_badge = !c.marker_use_badge, "marker_use_badge");
            AssertChanges(c => c.badge_corner = "bottom_left", "badge_corner");
            AssertChanges(c => c.badge_size_ratio += 0.05f, "badge_size_ratio");
            AssertChanges(c => c.ring_size_ratio += 0.05f, "ring_size_ratio");
            AssertChanges(c => c.contour_spin_deg_per_s += 10f, "contour_spin_deg_per_s");
            AssertChanges(c => c.category_styles[0].color_hex = "#123456", "category_styles row");
            AssertChanges(c => c.badge_categories[0].icon_key = "changed", "badge_categories row");
            AssertChanges(c => c.outline_levels[0].line_style = "dotted", "outline_levels row");
            AssertChanges(c => c.pois[0].category = "changed_category", "POI category");
            AssertChanges(c => c.pois[0].badge_category = "changed_badge", "POI badge_category");
            AssertChanges(c => c.pois[0].has_custom_symbol = !c.pois[0].has_custom_symbol, "POI has_custom_symbol");
            AssertChanges(c => c.pois[0].name += "_renamed", "POI name (the marker's label text)");

            var unrelated = LoadShippedConfig();
            unrelated.pois[0].editor_rotation_deg += 10f;
            unrelated.pois[0].hierarchy_level_key = "level_5";
            unrelated.effect_defaults.pulse.enabled = !unrelated.effect_defaults.pulse.enabled;
            unrelated.wall_name += "_x";
            Assert.AreEqual(baseline, applier.Fingerprint(unrelated), "facing/hierarchy/effects/name edits must not trigger a marker push");
        }

        [Test]
        public void MarkerApplier_DrivesRealMarkers_WallLevelAndPerPoiFields()
        {
            var authoring = LoadShippedConfig();
            MarkerHierarchyResolver.Configure(authoring.hierarchy_levels);
            var session = NewSessionOn(Copy(authoring));
            var poi = authoring.pois[0];
            var marker = session.SpawnedMarkers.First(m => m.name == poi.id);
            var symbol = marker.transform.Find("Symbol").GetComponent<UnityEngine.UI.Image>();
            var dispatcher = new LivePlayModeConfigDispatcher(new ILivePlayModeApplier[] { new LivePlayModeMarkerApplier() });
            bool ringOnBefore = marker.transform.Find("Ring").GetComponent<UnityEngine.UI.Image>().enabled;
            Assert.IsTrue(ringOnBefore, "Precondition: the shipped POI has an outline ring.");

            // 1. wall-level: turning the outline off hides the ring on the running marker
            authoring.marker_outline_mode = "none";
            var applied = dispatcher.Push(session, authoring);
            CollectionAssert.AreEqual(new[] { "marker" }, applied);
            Assert.IsFalse(marker.transform.Find("Ring").GetComponent<UnityEngine.UI.Image>().enabled, "outline off must hide the ring live");

            // 2. wall-level: category colour edit reaches the running symbol
            authoring.marker_outline_mode = "uniform";
            var categoryEntry = authoring.category_styles.First(e => e.category == poi.category);
            categoryEntry.color_hex = "#00FF00";
            dispatcher.Push(session, authoring);
            Color expected; ColorUtility.TryParseHtmlString("#00FF00", out expected);
            Assert.AreEqual(expected, symbol.color, "a live category colour edit must reach the running Symbol.");

            // 3. per-POI: switching this POI to a different real category changes it live
            string otherCategory = authoring.category_styles.First(e => e.category != poi.category).category;
            poi.category = otherCategory;
            dispatcher.Push(session, authoring);
            Assert.AreNotEqual(expected, symbol.color, "the marker must recolour when its own POI.category changes live.");

            // 4. an unrelated edit pushes nothing
            authoring.wall_name += "_x";
            Assert.IsEmpty(dispatcher.Push(session, authoring));

            // 5. the wall never shares config objects with the window
            var sessionConfigField = typeof(WallSession).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            var wallConfig = (WallConfigData)sessionConfigField.GetValue(session);
            Assert.AreNotSame(authoring.category_styles, wallConfig.category_styles);
        }

        // icon_color_hex / icon_size_ratio (_2.2.1): the Marker section's Icon color / Icon size reach
        // a real running marker's Symbol icon (colour + anchors) and its Badge icon colour, live.
        [Test]
        public void MarkerApplier_DrivesRealMarkers_IconColorAndSize()
        {
            var authoring = LoadShippedConfig();
            MarkerHierarchyResolver.Configure(authoring.hierarchy_levels);
            var session = NewSessionOn(Copy(authoring));
            var poi = authoring.pois.First(p => p.has_status && !p.status_unknown && !string.IsNullOrEmpty(p.badge_category));
            var marker = session.SpawnedMarkers.First(m => m.name == poi.id);
            var icon = marker.transform.Find("Symbol/Icon");
            var badgeIcon = marker.transform.Find("Badge/Icon");
            Assert.IsNotNull(icon, "POI_Marker has Symbol/Icon");
            Assert.IsNotNull(badgeIcon, "POI_Marker has Badge/Icon");
            var dispatcher = new LivePlayModeConfigDispatcher(new ILivePlayModeApplier[] { new LivePlayModeMarkerApplier() });
            dispatcher.Push(session, authoring);
            float widthBefore = ((RectTransform)icon).anchorMax.x - ((RectTransform)icon).anchorMin.x;

            authoring.icon_color_hex = "#FF00FF";
            authoring.icon_size_ratio = 0.85f;
            CollectionAssert.AreEqual(new[] { "marker" }, dispatcher.Push(session, authoring));

            ColorUtility.TryParseHtmlString("#FF00FF", out var magenta);
            var iconColor = icon.GetComponent<UnityEngine.UI.Image>().color;
            Assert.AreEqual(magenta.r, iconColor.r, 1e-3f); Assert.AreEqual(magenta.g, iconColor.g, 1e-3f); Assert.AreEqual(magenta.b, iconColor.b, 1e-3f);
            var badgeColor = badgeIcon.GetComponent<UnityEngine.UI.Image>().color;
            Assert.AreEqual(magenta.r, badgeColor.r, 1e-3f, "the badge icon uses the same Icon color");
            float widthAfter = ((RectTransform)icon).anchorMax.x - ((RectTransform)icon).anchorMin.x;
            Assert.AreEqual(0.85f, widthAfter, 1e-3f, "the icon spans Icon size of the symbol");
            Assert.Greater(widthAfter, widthBefore, "a live Icon size edit must grow the running icon");
        }

        // label_gap_ratio / label_font_size_ratio (_2.2.1): wall-level ratios of the symbol
        // diameter, resolved through MarkerVisualSettings the same way ring_size_ratio/
        // badge_size_ratio already are. Proves the config actually reaches a real marker's real
        // Label RectTransform/TextMeshProUGUI component, live, on a wall that shows its label
        // (level_1 in the shipped config has show_label true).
        [Test]
        public void MarkerApplier_DrivesRealMarkers_LabelGapAndFontSizeRatios()
        {
            var authoring = LoadShippedConfig();
            var level = authoring.hierarchy_levels.First(l => l.show_label);
            MarkerHierarchyResolver.Configure(authoring.hierarchy_levels);
            var session = NewSessionOn(Copy(authoring));
            var poi = authoring.pois.First(p => p.hierarchy_level_key == level.key);
            var marker = session.SpawnedMarkers.First(m => m.name == poi.id);
            var labelRect = (RectTransform)marker.transform.Find("Label");
            var labelText = labelRect.GetComponent("TextMeshProUGUI");
            var fontSizeProp = labelText.GetType().GetProperty("fontSize");
            float symbolDiameter = level.size_cm / 100f;
            var dispatcher = new LivePlayModeConfigDispatcher(new ILivePlayModeApplier[] { new LivePlayModeMarkerApplier() });

            authoring.label_gap_ratio = 0.2f;
            authoring.label_font_size_ratio = 0.4f;
            var applied = dispatcher.Push(session, authoring);
            CollectionAssert.AreEqual(new[] { "marker" }, applied);

            Assert.AreEqual(0.4f * symbolDiameter, (float)fontSizeProp.GetValue(labelText), 1e-4f,
                "label_font_size_ratio must reach the real marker's TMP font size live");
            float symbolRadius = symbolDiameter * 0.5f;
            Assert.AreEqual(-symbolRadius - 0.2f * symbolDiameter, labelRect.anchoredPosition.y, 1e-4f,
                "label_gap_ratio must reach the real marker's Label offset live");
        }

        // Font system (_2.0_Labels_And_Fonts_Design.md): the wall's label_font_key reaches the real
        // marker's TMP font asset live, through the same prefab-attached FontKeyLibrary MarkerView
        // resolves at draw time.
        [Test]
        public void MarkerApplier_DrivesRealMarkers_LabelFontKey()
        {
            var authoring = LoadShippedConfig();
            var level = authoring.hierarchy_levels.First(l => l.show_label);
            MarkerHierarchyResolver.Configure(authoring.hierarchy_levels);
            var session = NewSessionOn(Copy(authoring));
            var poi = authoring.pois.First(p => p.hierarchy_level_key == level.key);
            var marker = session.SpawnedMarkers.First(m => m.name == poi.id);
            var labelRect = (RectTransform)marker.transform.Find("Label");
            var labelText = labelRect.GetComponent("TextMeshProUGUI");
            var fontProp = labelText.GetType().GetProperty("font");
            var fontLib = AssetDatabase.LoadAssetAtPath<FontKeyLibrary>("Assets/Framework/Runtime/UI/Markers/FontLibrary.asset");
            var dispatcher = new LivePlayModeConfigDispatcher(new ILivePlayModeApplier[] { new LivePlayModeMarkerApplier() });

            authoring.label_font_key = "roboto_bold";
            dispatcher.Push(session, authoring);

            Assert.AreSame(fontLib.Get("roboto_bold"), fontProp.GetValue(labelText),
                "label_font_key must reach the real marker's TMP font asset live");
        }

        // Marker Label Style (_2.0_Labels_And_Fonts_Design.md section 4), the whole contract on a real
        // marker through the real applier: override ON = the level's own values win; while ON a wall
        // default edit does NOT reach that level; switched OFF live = the level follows the wall
        // default again (its stored values are ignored, not deleted).
        [Test]
        public void MarkerApplier_DrivesRealMarkers_LevelLabelStyleOverride_OnOffAndWallEdits()
        {
            var authoring = LoadShippedConfig();
            authoring.label_gap_ratio = 0.075f;
            authoring.label_font_size_ratio = 0.25f;
            authoring.label_font_key = "liberation_sans";
            var level = authoring.hierarchy_levels.First(l => l.show_label);
            level.override_label_style = true;
            level.label_gap_ratio = 0.2f;
            level.label_font_size_ratio = 0.5f;
            level.label_font_key = "oswald_bold";
            MarkerHierarchyResolver.Configure(authoring.hierarchy_levels);
            var session = NewSessionOn(Copy(authoring));
            var poi = authoring.pois.First(p => p.hierarchy_level_key == level.key);
            var marker = session.SpawnedMarkers.First(m => m.name == poi.id);
            var labelRect = (RectTransform)marker.transform.Find("Label");
            var labelText = labelRect.GetComponent("TextMeshProUGUI");
            var fontSizeProp = labelText.GetType().GetProperty("fontSize");
            var fontProp = labelText.GetType().GetProperty("font");
            var fontLib = AssetDatabase.LoadAssetAtPath<FontKeyLibrary>("Assets/Framework/Runtime/UI/Markers/FontLibrary.asset");
            float symbolDiameter = level.size_cm / 100f;
            float symbolRadius = symbolDiameter * 0.5f;
            float FontSize() => (float)fontSizeProp.GetValue(labelText);
            var dispatcher = new LivePlayModeConfigDispatcher(new ILivePlayModeApplier[] { new LivePlayModeMarkerApplier() });
            dispatcher.Push(session, authoring);

            Assert.AreEqual(0.5f * symbolDiameter, FontSize(), 1e-4f, "override ON: the level's own font size wins");
            Assert.AreEqual(-symbolRadius - 0.2f * symbolDiameter, labelRect.anchoredPosition.y, 1e-4f, "override ON: the level's own gap wins");
            Assert.AreSame(fontLib.Get("oswald_bold"), fontProp.GetValue(labelText), "override ON: the level's own font wins");

            authoring.label_font_size_ratio = 0.9f;
            dispatcher.Push(session, authoring);
            Assert.AreEqual(0.5f * symbolDiameter, FontSize(), 1e-4f, "override ON: a wall default edit must not reach this level");

            level.override_label_style = false;
            CollectionAssert.AreEqual(new[] { "marker" }, dispatcher.Push(session, authoring), "the override switch is a marker-applier field");
            Assert.AreEqual(0.9f * symbolDiameter, FontSize(), 1e-4f, "override OFF: the level follows the current wall default");
            Assert.AreEqual(-symbolRadius - 0.075f * symbolDiameter, labelRect.anchoredPosition.y, 1e-4f, "override OFF: wall gap");
            Assert.AreSame(fontLib.Get("liberation_sans"), fontProp.GetValue(labelText), "override OFF: wall font");
            Assert.AreEqual(0.5f, level.label_font_size_ratio, "switching OFF keeps the level's stored values (just ignored)");
        }

        // Confirms the exact gap a developer reported: editing the Hierarchy Levels table while
        // Play Mode runs must show up on the real markers right away, for every column the marker
        // applier owns (Size, Text Label, Spin Ring, Search Keywords -- Ripple/Halo/Pulse are
        // covered by EffectsApplier_DrivesRealMarkers_ThroughEverySwitchAndLevelChoice above,
        // Facing Override by OrientationApplier_RepointsRealMarkers... above). Reveal Delay/
        // Duration are NOT asserted here: MarkerRevealEffect.Play only runs once, from
        // MarkerView.Initialise, at spawn time (_2.3_Marker_Hierarchy.md section 5's "settled
        // baseline" contract) -- a live edit to those two fields has nothing left to animate on an
        // already-revealed marker, by design, not by a live-sync bug.
        [Test]
        public void MarkerApplier_DrivesRealMarkers_HierarchyLevelFields()
        {
            var authoring = LoadShippedConfig();
            var level = authoring.hierarchy_levels[0];
            MarkerHierarchyResolver.Configure(authoring.hierarchy_levels);
            var session = NewSessionOn(Copy(authoring));
            var poi = authoring.pois.First(p => p.hierarchy_level_key == level.key);
            var marker = session.SpawnedMarkers.First(m => m.name == poi.id);
            var symbol = marker.transform.Find("Symbol").GetComponent<RectTransform>();
            var label = marker.transform.Find("Label").gameObject;
            var ring = marker.transform.Find("Ring").GetComponent<MarkerRingView>();
            var rotatingField = typeof(MarkerRingView).GetField("_rotating", BindingFlags.NonPublic | BindingFlags.Instance);
            var dispatcher = new LivePlayModeConfigDispatcher(new ILivePlayModeApplier[] { new LivePlayModeMarkerApplier() });

            // 1. Size (cm): the real Symbol RectTransform must resize live, no restart.
            // `level` aliases authoring.hierarchy_levels[0], so the original value is captured
            // BEFORE mutating -- reading level.size_cm afterwards would double-count the edit.
            float originalSizeCm = level.size_cm;
            authoring.hierarchy_levels[0].size_cm = originalSizeCm + 25f;
            var applied = dispatcher.Push(session, authoring);
            CollectionAssert.AreEqual(new[] { "marker" }, applied);
            Assert.AreEqual((originalSizeCm + 25f) / 100f, symbol.sizeDelta.x, 0.001f, "Size (cm) must resize the running marker live");

            // 2. Text Label (Show/Don't Show): the real Label GameObject must toggle live.
            bool showBefore = label.activeSelf;
            authoring.hierarchy_levels[0].show_label = !showBefore;
            dispatcher.Push(session, authoring);
            Assert.AreEqual(!showBefore, label.activeSelf, "Text Label must show/hide the running marker's label live");

            // 3. Spin Ring (rotate_contour): the real ring's rotation state must toggle live.
            authoring.hierarchy_levels[0].rotate_contour = true;
            dispatcher.Push(session, authoring);
            Assert.IsTrue((bool)rotatingField.GetValue(ring), "Spin Ring ON must start the running marker's ring rotating live");
            authoring.hierarchy_levels[0].rotate_contour = false;
            dispatcher.Push(session, authoring);
            Assert.IsFalse((bool)rotatingField.GetValue(ring), "Spin Ring OFF must stop the running marker's ring rotating live");

            // 4. Search Keywords: not marker-visual, but must still round-trip live into the running
            // wall's own config copy (POISearchIndex reads WallSession's config, not the window's).
            authoring.hierarchy_levels[0].search_keywords = new List<string> { "edited_live" };
            dispatcher.Push(session, authoring);
            var wallConfigField = typeof(WallSession).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            var wallConfig = (WallConfigData)wallConfigField.GetValue(session);
            CollectionAssert.AreEqual(new[] { "edited_live" }, wallConfig.hierarchy_levels[0].search_keywords,
                "Search Keywords must reach the running wall's own hierarchy_levels copy live");

            // 5. an unrelated edit pushes nothing (same discipline as every other applier test)
            authoring.wall_name += "_x";
            Assert.IsEmpty(dispatcher.Push(session, authoring));
        }

        // A POI's own Hierarchy Level assignment (Specific Marker > Marker Style > Hierarchy Level) is
        // live: moving a real POI to another level resizes it, shows/hides its label, swaps its effects
        // and its facing override at once -- and ONLY the poi-level applier reacts (the levels' own
        // columns are untouched, so no other domain re-applies). All four real appliers, real order.
        [Test]
        public void PoiLevelApplier_MovingARealPoiToAnotherLevel_ChangesItsSizeLabelEffectsAndFacingLive()
        {
            var authoring = LoadShippedConfig();
            var from = authoring.hierarchy_levels.First(l => l.show_label);
            var to = authoring.hierarchy_levels.First(l => !l.show_label && Mathf.Abs(l.size_cm - from.size_cm) > 1f);
            to.facing_mode_override = "wall_fixed";
            from.facing_mode_override = "";
            from.pulse = true;
            to.pulse = false;
            MarkerHierarchyResolver.Configure(authoring.hierarchy_levels);
            var session = NewSessionOn(Copy(authoring));
            var poi = authoring.pois.First(p => p.hierarchy_level_key == from.key);
            var marker = session.SpawnedMarkers.First(m => m.name == poi.id);
            var symbol = marker.transform.Find("Symbol").GetComponent<RectTransform>();
            var label = marker.transform.Find("Label").gameObject;
            var billboard = marker.GetComponentInChildren<MarkerBillboard>();
            var pulse = marker.GetComponent<MarkerPulseEffect>();
            var dispatcher = new LivePlayModeConfigDispatcher(new ILivePlayModeApplier[]
            {
                new LivePlayModeEffectsApplier(), new LivePlayModeOrientationApplier(),
                new LivePlayModeMarkerApplier(), new LivePlayModePoiLevelApplier(),
            });
            dispatcher.Push(session, authoring);   // first push to a new wall applies every domain once

            Assert.AreEqual(from.size_cm / 100f, symbol.sizeDelta.x, 0.001f, "precondition: the POI starts at its own level's size");
            Assert.IsTrue(label.activeSelf, "precondition: its level shows the label");
            Assert.IsTrue(pulse.IsActive, "precondition: its level pulses");

            poi.hierarchy_level_key = to.key;
            CollectionAssert.AreEqual(new[] { "poi-level" }, dispatcher.Push(session, authoring),
                "a POI level change is the poi-level applier's alone");

            Assert.AreEqual(to.size_cm / 100f, symbol.sizeDelta.x, 0.001f, "the POI takes its new level's size live");
            Assert.IsFalse(label.activeSelf, "the POI takes its new level's label visibility live");
            Assert.IsFalse(pulse.IsActive, "the POI takes its new level's effects live");
            Assert.AreEqual("wall_fixed", billboard.ConfiguredModeOverride, "the POI takes its new level's facing override live");

            poi.hierarchy_level_key = "";
            dispatcher.Push(session, authoring);
            Assert.AreEqual(MarkerHierarchyResolver.Fallback.SizeCm / 100f, symbol.sizeDelta.x, 0.001f,
                "'(none)' drops the POI to the framework Fallback live");

            from.size_cm += 5f;
            Assert.IsFalse(dispatcher.Push(session, authoring).Contains("poi-level"), "a level column edit is not a POI assignment change");
        }
    }
}
