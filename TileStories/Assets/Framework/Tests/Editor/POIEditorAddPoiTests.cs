using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TileStories;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for the POI header rename (pencil button).
    public class POIHeaderRenameTests
    {
        // An unshown window instance. EditorWindow.GetWindow parks a real dock
        // tab in the user's editor layout; leaking one leaves a stale HostView
        // whose GetExtraButtonsWidth throws NullReferenceException while the
        // tab strip repaints. CreateInstance never touches the dock, and
        // TearDown destroys it so nothing survives the test run.
        private static POIEditorToolWindow _testWindow;

        private static POIEditorToolWindow CreateTestWindow(WallConfigData config)
        {
            _testWindow = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            var field = typeof(POIEditorToolWindow).GetField("_config",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_testWindow, config);
            return _testWindow;
        }

        [TearDown]
        public void TeardownTestWindow()
        {
            if (_testWindow != null)
            {
                Object.DestroyImmediate(_testWindow);
                _testWindow = null;
            }
        }

        [Test]
        public void StripNumberPrefix_NumberedDisplayName_ReturnsBareName()
        {
            Assert.AreEqual("Lamp", POIEditorToolWindow.StripNumberPrefix("3. Lamp"));
        }

        [Test]
        public void StripNumberPrefix_UnnumberedName_ReturnsTrimmed()
        {
            Assert.AreEqual("Lamp", POIEditorToolWindow.StripNumberPrefix("  Lamp  "));
        }

        [Test]
        public void StripNumberPrefix_NullOrBlank_ReturnsEmpty()
        {
            Assert.AreEqual("", POIEditorToolWindow.StripNumberPrefix(null));
            Assert.AreEqual("", POIEditorToolWindow.StripNumberPrefix("   "));
        }

        [Test]
        public void StripNumberPrefix_DotWithoutSpace_KeepsWholeString()
        {
            // Only ". " (dot+space) is the numbering separator; "v1.2" is a name.
            Assert.AreEqual("v1.2 Wall", POIEditorToolWindow.StripNumberPrefix("v1.2 Wall"));
        }

        [Test]
        public void SaveNameChange_NumberedInput_StripsPrefixAndWritesPoiName()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_1", name = "Old Name", category = "default" }
                }
            };
            var window = CreateTestWindow(config);
            var save = typeof(POIEditorToolWindow).GetMethod("SaveNameChange",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(save, "SaveNameChange must exist");

            save.Invoke(window, new object[] { config.pois[0], "poi_1", "editName_poi_1", "editValue_poi_1", "1. New Name" });

            Assert.AreEqual("New Name", config.pois[0].name);
        }

        [Test]
        public void SaveNameChange_BlankInput_KeepsOldName()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_1", name = "Old Name", category = "default" }
                }
            };
            var window = CreateTestWindow(config);
            var save = typeof(POIEditorToolWindow).GetMethod("SaveNameChange",
                BindingFlags.NonPublic | BindingFlags.Instance);

            save.Invoke(window, new object[] { config.pois[0], "poi_1", "editName_poi_1", "editValue_poi_1", "   " });

            Assert.AreEqual("Old Name", config.pois[0].name, "Blank rename must not wipe the name.");
        }

        [Test]
        public void EditIconAssetPath_PointsInsideFrameworkEditor()
        {
            StringAssert.StartsWith("Assets/Framework/Editor/",
                POIEditorToolWindow.EditIconAssetPath,
                "Editor-only icons must live under Framework/Editor per 10-structure.");
        }
    }

    // Tier-0 tests for the rename key decision table (Enter/Esc double-press bug).
    public class PoiRenameKeysTests
    {
        [Test]
        public void Resolve_KeyDown_Return_Commits()
        {
            Assert.AreEqual(PoiRenameKeys.Action.Commit,
                PoiRenameKeys.Resolve(EventType.KeyDown, KeyCode.Return));
        }

        [Test]
        public void Resolve_KeyDown_KeypadEnter_Commits()
        {
            Assert.AreEqual(PoiRenameKeys.Action.Commit,
                PoiRenameKeys.Resolve(EventType.KeyDown, KeyCode.KeypadEnter));
        }

        [Test]
        public void Resolve_KeyDown_Escape_Cancels()
        {
            Assert.AreEqual(PoiRenameKeys.Action.Cancel,
                PoiRenameKeys.Resolve(EventType.KeyDown, KeyCode.Escape));
        }

        // Regression: the TextField consumes the first Return (event becomes
        // Used); the resolver must never re-fire on consumed events.
        [Test]
        public void Resolve_Used_Return_IsNone()
        {
            Assert.AreEqual(PoiRenameKeys.Action.None,
                PoiRenameKeys.Resolve(EventType.Used, KeyCode.Return));
        }

        // KeyUp must not re-fire the action after KeyDown handled it.
        [Test]
        public void Resolve_KeyUp_Return_IsNone()
        {
            Assert.AreEqual(PoiRenameKeys.Action.None,
                PoiRenameKeys.Resolve(EventType.KeyUp, KeyCode.Return));
        }

        // Layout/Repaint passes must never trigger actions.
        [Test]
        public void Resolve_LayoutAndRepaint_AreNone()
        {
            Assert.AreEqual(PoiRenameKeys.Action.None,
                PoiRenameKeys.Resolve(EventType.Layout, KeyCode.Return));
            Assert.AreEqual(PoiRenameKeys.Action.None,
                PoiRenameKeys.Resolve(EventType.Repaint, KeyCode.Escape));
        }

        // Ordinary typing keys must not commit or cancel.
        [Test]
        public void Resolve_KeyDown_Letter_IsNone()
        {
            Assert.AreEqual(PoiRenameKeys.Action.None,
                PoiRenameKeys.Resolve(EventType.KeyDown, KeyCode.A));
        }
    }

    // Tier-0 EditMode tests for the "Add POI" button (Step 19).
    public class POIEditorAddPoiTests
    {
        // Unshown window instance -- CreateInstance never docks a tab, so no
        // HostView can be left holding a stale reference (see POIHeaderRenameTests).
        private static POIEditorToolWindow _testWindow;

        private static POIEditorToolWindow CreateWindowWithConfig(WallConfigData config)
        {
            _testWindow = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            var field = typeof(POIEditorToolWindow).GetField("_config",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_testWindow, config);
            return _testWindow;
        }

        private static void InvokeAddFirstPoi(POIEditorToolWindow window)
        {
            var method = typeof(POIEditorToolWindow).GetMethod("AddFirstPoi",
                BindingFlags.NonPublic | BindingFlags.Instance, null, System.Type.EmptyTypes, null);
            Assert.IsNotNull(method, "AddFirstPoi method not found");
            method.Invoke(window, null);
        }

        private static void InvokeAddNewPoiAfter(POIEditorToolWindow window, int index)
        {
            var method = typeof(POIEditorToolWindow).GetMethod("AddNewPoiAfter",
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int) }, null);
            Assert.IsNotNull(method, "AddNewPoiAfter method not found");
            method.Invoke(window, new object[] { index });
        }

        // AddNewPoi spawns a real global POIEditorRig GameObject (via
        // SpawnAndFocusNewPoi) that is never auto-destroyed. Without this teardown
        // it leaks across tests in the same editor, and GameObject.Find in the
        // unrelated sync-check tests finds the stale rig -> they fail with phantom
        // out-of-sync children. Destroy it so every test starts clean.
        // The window instance is destroyed here for the same reason: a leaked
        // EditorWindow survives into the user's editor session.
        [TearDown]
        public void Teardown()
        {
            var rig = GameObject.Find("POIEditorRig");
            if (rig != null)
                Object.DestroyImmediate(rig);

            if (_testWindow != null)
            {
                Object.DestroyImmediate(_testWindow);
                _testWindow = null;
            }
        }

        private static WallConfigData CreateMinimalConfig()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                pois = new System.Collections.Generic.List<POIData>()
            };
            return config;
        }

        [Test]
        public void AddFirstPoi_IncrementsListAndSetsDefaults()
        {
            var config = CreateMinimalConfig();
            var window = CreateWindowWithConfig(config);

            int beforeCount = config.pois.Count;
            InvokeAddFirstPoi(window);
            int afterCount = config.pois.Count;

            Assert.AreEqual(beforeCount + 1, afterCount, "POI list should increment by one");

            var newPoi = config.pois[0];
            Assert.IsFalse(string.IsNullOrEmpty(newPoi.id), "New POI should have a GUID id");
            Assert.AreEqual("New POI", newPoi.name);
            // A config with no category_styles has nothing real to copy from -- empty, not a
            // literal "default" that matches no dropdown option and silently falls through to
            // CategoryPalette's hash fallback.
            Assert.AreEqual(string.Empty, newPoi.category);
            Assert.AreEqual(0f, newPoi.editor_rotation_deg, 0.0001f);
            Assert.IsFalse(newPoi.position_verified);
            Assert.AreEqual(0f, newPoi.status_pct);
            Assert.IsFalse(newPoi.has_status);
            Assert.IsFalse(newPoi.status_unknown);
            Assert.IsNull(newPoi.hierarchy_level_key);
            Assert.IsFalse(newPoi.has_custom_symbol);
            Assert.IsNull(newPoi.custom_symbol_key);
            Assert.IsNull(newPoi.badge_category);
            Assert.IsNotNull(newPoi.search_keywords);
            Assert.IsNotNull(newPoi.search_keyword_fields);
        }

        [Test]
        public void AddFirstPoi_WithCategoryTaxonomy_UsesTheWallsFirstRealCategory()
        {
            var config = CreateMinimalConfig();
            config.category_styles = new System.Collections.Generic.List<CategoryStyleEntry>
            {
                new CategoryStyleEntry { category = "religious", icon_key = "unknown" },
                new CategoryStyleEntry { category = "military", icon_key = "unknown" },
            };
            var window = CreateWindowWithConfig(config);

            InvokeAddFirstPoi(window);

            Assert.AreEqual("religious", config.pois[0].category,
                "A new POI's category should be a real, pickable dropdown value, not an invented literal.");
        }

        // Ticking Status unknown on a POI that already shows a known badge (e.g. intact) must switch
        // the badge to the unknown row too: the runtime unknown badge follows the POI's own badge key
        [Test]
        public void TickingStatusUnknown_SelectsTheUnknownOutlineType_AndTheUnknownBadge_EvenOverAKnownBadge()
        {
            var config = CreateMinimalConfig();
            config.outline_levels = new System.Collections.Generic.List<OutlineLevelEntry>
            {
                new OutlineLevelEntry { key = "intact", pct = 0f }, new OutlineLevelEntry { key = "unknown", pct = 100f }
            };
            config.badge_categories = new System.Collections.Generic.List<BadgeCategoryEntry>
            {
                new BadgeCategoryEntry { key = "intact" }, new BadgeCategoryEntry { key = "unknown_damage" }
            };
            var poi = new POIData { id = "p", has_status = true, status_level_key = "intact", badge_category = "intact" };
            config.pois.Add(poi);
            var window = CreateWindowWithConfig(config);

            typeof(POIEditorToolWindow).GetMethod("ApplyUnknownStatusDefaults", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(window, new object[] { poi });

            Assert.AreEqual("unknown", poi.status_level_key);
            Assert.AreEqual(100f, poi.status_pct);
            Assert.AreEqual("unknown_damage", poi.badge_category, "a known badge would contradict the unknown ring");
        }

        [Test]
        public void AddFirstPoi_WithHierarchyLevels_UsesTheHighestPriorityLevel_SoItPassesTheSaveCheck()
        {
            var config = CreateMinimalConfig();
            config.hierarchy_levels = new System.Collections.Generic.List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "big", level_name = "Big", size_cm = 8f, priority = 100 },
                new HierarchyLevelEntry { key = "top", level_name = "Top", size_cm = 20f, priority = 10 },
            };
            var window = CreateWindowWithConfig(config);

            InvokeAddFirstPoi(window);

            Assert.AreEqual("top", config.pois[0].hierarchy_level_key,
                "A first POI should get a real level (size + label), not the 12 cm 'no level' fallback.");
            var issues = (System.Collections.IList)typeof(POIEditorToolWindow)
                .GetMethod("ValidateHierarchyLevelKeys", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(window, null);
            Assert.AreEqual(0, issues.Count, "the new POI must not trigger the empty-level validation notice");
        }

        [Test]
        public void AddNewPoiAfter_CopiesStatusLevelKey_AndDeepCopiesSearchKeywordFields()
        {
            var sourceKeywords = new System.Collections.Generic.List<POISearchKeywordField>
            {
                new POISearchKeywordField { field_key = "architect", keywords = new System.Collections.Generic.List<string> { "wren" } }
            };
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_1", name = "Lamp", category = "civic", has_status = true, status_level_key = "partial_damage", search_keyword_fields = sourceKeywords }
                }
            };
            var window = CreateWindowWithConfig(config);

            InvokeAddNewPoiAfter(window, 0);

            var added = config.pois[1];
            Assert.AreEqual("partial_damage", added.status_level_key, "status_level_key must be copied from the source POI.");

            added.search_keyword_fields[0].keywords.Add("hooke");
            Assert.AreEqual(1, config.pois[0].search_keyword_fields[0].keywords.Count,
                "Editing the new POI's keywords must not mutate the source POI's own list.");
        }

        [Test]
        public void AddNewPoiAfter_UsesPreviousPoiAsTemplate()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_1", name = "Lamp", category = "default", editor_rotation_deg = 30f, hierarchy_level_key = "level_a", has_status = true, status_pct = 0.4f },
                    new POIData { id = "poi_2", name = "Chair", category = "seating", editor_rotation_deg = 90f, hierarchy_level_key = "level_b", has_status = false, status_pct = 0f }
                }
            };
            var window = CreateWindowWithConfig(config);

            InvokeAddNewPoiAfter(window, 0);

            Assert.AreEqual(3, config.pois.Count, "The new POI should be inserted after the source POI.");
            Assert.AreEqual(config.pois[0].category, config.pois[1].category, "The inserted POI should inherit the source POI's style.");
            Assert.AreEqual(config.pois[0].editor_rotation_deg, config.pois[1].editor_rotation_deg, 0.0001f, "The inserted POI should inherit the source POI's rotation.");
        }

        [Test]
        public void AddFirstPoi_StartsWithZeroFacingOnAllThreeAxes()
        {
            var config = CreateMinimalConfig();
            var window = CreateWindowWithConfig(config);

            InvokeAddFirstPoi(window);

            var poi = config.pois[0];
            Assert.AreEqual(0f, poi.editor_rotation_x_deg, 0.0001f);
            Assert.AreEqual(0f, poi.editor_rotation_deg, 0.0001f);
            Assert.AreEqual(0f, poi.editor_rotation_z_deg, 0.0001f);
        }

        [Test]
        public void AddNewPoiAfter_CopiesAllThreeFacingAnglesFromSource_AndPutsThemOnTheRigChild()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_1", name = "Lamp", category = "default", editor_rotation_x_deg = 15f, editor_rotation_deg = 120f, editor_rotation_z_deg = 45f }
                }
            };
            var window = CreateWindowWithConfig(config);

            InvokeAddNewPoiAfter(window, 0);

            var added = config.pois[1];
            Assert.AreEqual(15f, added.editor_rotation_x_deg, 0.0001f);
            Assert.AreEqual(120f, added.editor_rotation_deg, 0.0001f);
            Assert.AreEqual(45f, added.editor_rotation_z_deg, 0.0001f);

            var rigChild = GameObject.Find("POIEditorRig")?.transform.Find(added.id);
            Assert.IsNotNull(rigChild, "The new POI's rig child must exist.");
            Assert.That(Quaternion.Angle(rigChild.localRotation, Quaternion.Euler(15f, 120f, 45f)), Is.LessThan(0.05f),
                "The Scene-view marker must start with the copied facing, not identity.");
        }

        [Test]
        public void AddNewPoiAfter_GeneratesUniqueIds()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_1", name = "Lamp", category = "default" }
                }
            };
            var window = CreateWindowWithConfig(config);

            InvokeAddNewPoiAfter(window, 0);
            InvokeAddNewPoiAfter(window, 0);

            Assert.AreEqual(3, config.pois.Count);
            Assert.AreNotEqual(config.pois[1].id, config.pois[2].id, "Each POI should receive a unique GUID");
        }

        [Test]
        public void AddNewPoiAfter_OnLastPoi_AppendsToEnd()
        {
            // The per-row "+" button on the LAST POI is now the only way to append at
            // the end (the old dedicated "+ Add POI near previous" trailing button was
            // removed as part of simplifying the add-near-POI flow).
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_1", name = "Lamp", category = "default" },
                    new POIData { id = "poi_2", name = "Chair", category = "seating" }
                }
            };
            var window = CreateWindowWithConfig(config);

            InvokeAddNewPoiAfter(window, config.pois.Count - 1);

            Assert.AreEqual(3, config.pois.Count);
            Assert.AreEqual("New POI", config.pois[2].name, "Adding after the last POI must append at the end.");
        }
    }
}
