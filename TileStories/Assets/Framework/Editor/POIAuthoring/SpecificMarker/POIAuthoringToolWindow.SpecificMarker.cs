using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        private void DrawSpecificMarkerOptions()
        {
            if (_config.pois == null || _config.pois.Count == 0)
            {
                EditorGUILayout.HelpBox("No POIs yet. Use the + buttons below to add your first POI.", MessageType.Info);
                if (GUILayout.Button("+ Add first", GUILayout.Width(120f)))
                {
                    AddNewPoi();
                }
                return;
            }

            // "+ Add first" button before the first POI
            if (GUILayout.Button("+ Add first", GUILayout.Width(120f)))
            {
                AddNewPoi(); // adds at index 0
            }

            for (int i = 0; i < _config.pois.Count; i++)
            {
                var poi = _config.pois[i];
                if (poi == null)
                    continue;

                string foldoutKey = string.IsNullOrWhiteSpace(poi.id) ? $"poi_{i}" : poi.id;
                bool expanded = GetPoiFoldout(foldoutKey);
                expanded = EditorGUILayout.Foldout(expanded, $"{i + 1}. {poi.name} ({poi.id})", true, CreateFoldoutStyle(PoiHeaderColorFor(foldoutKey, i)));
                _poiFoldouts[foldoutKey] = expanded;
                if (!expanded)
                    continue;

                using (new EditorGUI.IndentLevelScope())
                {
                    // Focus helper, tinted with this POI's header color so the
                    // action reads as belonging to the marker above. Disabled
                    // until the rig has a child named poi.id (populate first).
                    using (new EditorGUI.DisabledScope(!CanFocusPoiInScene(poi)))
                    {
                        Color poiColor = PoiHeaderColorFor(foldoutKey, i);
                        var prevBg = GUI.backgroundColor;
                        GUI.backgroundColor = poiColor;
                        if (GUILayout.Button("Focus in Scene", GUILayout.Width(140f)))
                            FocusPoiInScene(poi);
                        GUI.backgroundColor = prevBg;
                    }

                    _showPoiPosition = DrawFramedFoldout(ref _showPoiPosition, () => DrawPositionTabs(poi), "Position", FoldoutDefaultColor);

                    _showPoiMarkerStyle = DrawFramedFoldout(ref _showPoiMarkerStyle, () => DrawPoiMarkerStyleFields(poi), "Marker Style", FoldoutDefaultColor);

                    if (_config.marker_use_badge)
                    {
                        _showPoiBadgeStyle = DrawFramedFoldout(ref _showPoiBadgeStyle, () => DrawPoiBadgeStyleFields(poi), "Badge Style", FoldoutDefaultColor);
                    }

                    // Outline is an independent axis from badge (section 13.0) --
                    // it must be reachable even when badge is off, as long as the
                    // wall has a non-"none" outline mode.
                    bool outlineEnabled = !string.Equals(_config.marker_outline_mode, "none", StringComparison.OrdinalIgnoreCase);
                    if (outlineEnabled)
                    {
                        _showPoiOutline = DrawFramedFoldout(ref _showPoiOutline, () => DrawPoiOutlineFields(poi), "Outline", FoldoutDefaultColor);
                    }

                    // Effects foldout removed: per-POI effect selection is now
                    // driven entirely by the hierarchy level (see DrawPoiMarkerStyleFields).
                    // Global effect *defaults* remain in the Global Scene Effects section.
                    _showPoiSearchKeywords = DrawFramedFoldout(ref _showPoiSearchKeywords, () => DrawPoiSearchKeywordsField(poi), "Search Keywords", FoldoutDefaultColor);

                    // "+ after" button for this POI
                    EditorGUILayout.Space(4f);
                    if (GUILayout.Button("+ Add after this", GUILayout.Width(120f)))
                    {
                        AddNewPoiAfter(i);
                    }
                }

                EditorGUILayout.Space(6f);
            }

            // "+ after last" button at the end of the list
            if (GUILayout.Button("+ Add after last", GUILayout.Width(120f)))
            {
                AddNewPoi(); // appends at end
            }
        }


        private void DrawPoiMarkerStyleFields(POIData poi)
        {
            poi.name = EditorGUILayout.TextField("Name", poi.name);
            poi.category = DrawCategoryDropdown("Category", poi.category);

            // Hierarchy Level: selects this POI's size/label/effects/reveal-delay
            // from the wall's hierarchy_levels table (section 2.3). Populated from
            // _config.hierarchy_levels; writes poi.hierarchy_level_key.
            poi.hierarchy_level_key = DrawHierarchyLevelDropdown("Hierarchy Level", poi.hierarchy_level_key);

            // Custom symbol override (section 13.6/21) -- replaces the old "is_hero"
            // concept. When checked, shows a Sprite field + preview. Uses the same
            // assign-to-wall-library-and-get-key flow as the category table.
            // Setting it changes just this POI's icon; category color, ring, and
            // badge are unaffected.
            poi.has_custom_symbol = EditorGUILayout.Toggle("Use Custom Symbol", poi.has_custom_symbol);

            if (poi.has_custom_symbol)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Custom symbol (optional)", GUILayout.MinWidth(130f));
                    Sprite current = ResolveSpriteForKey(poi.custom_symbol_key);
                    DrawSpritePreview(current);
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.MinWidth(140f));
                    if (chosen != current)
                        poi.custom_symbol_key = chosen != null ? AssignSpriteToLibraryAndGetKey(chosen, poi.id + "_symbol") : null;
                }
                EditorGUILayout.LabelField("Overrides just this POI's icon (e.g. a small castle glyph). Category color, ring, and badge stay unchanged.", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void DrawPoiBadgeStyleFields(POIData poi)
        {
            poi.badge_category = DrawBadgeCategoryDropdown("Badge category", poi.badge_category);
        }

        private void DrawPoiOutlineFields(POIData poi)
        {
            bool hasStatus = poi.has_status;
            bool wantsStatus = EditorGUILayout.Toggle("Has status", hasStatus);

            if (wantsStatus && !hasStatus)
            {
                poi.has_status = true;
                poi.status_pct = 0f;
            }
            else if (!wantsStatus && hasStatus)
            {
                poi.has_status = false;
                poi.status_pct = 0f;
                poi.status_unknown = false;
                poi.status_level_key = null;
            }

            if (poi.has_status)
            {
                if (_config.outline_levels != null && _config.outline_levels.Count > 0)
                    DrawStatusLevelDropdown(poi);
                else
                    poi.status_pct = EditorGUILayout.Slider("Status %", poi.status_pct, 0f, 100f);

                bool wasUnknown = poi.status_unknown;
                poi.status_unknown = EditorGUILayout.Toggle("Status unknown", poi.status_unknown);
                if (!wasUnknown && poi.status_unknown)
                    ApplyUnknownStatusDefaults(poi);
            }

            // rotate_contour is now a hierarchy-level property, not a per-POI field.
            // Configured in the Global Scene Hierarchy table (see DrawGlobalHierarchySection).
        }

        private void ApplyUnknownStatusDefaults(POIData poi)
        {
            if (poi == null)
                return;

            if (_config?.outline_levels != null)
            {
                for (int i = 0; i < _config.outline_levels.Count; i++)
                {
                    var level = _config.outline_levels[i];
                    if (level == null || string.IsNullOrWhiteSpace(level.key))
                        continue;

                    if (level.key == "unknown")
                    {
                        poi.status_level_key = level.key;
                        poi.status_pct = level.pct;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(poi.badge_category) && _config?.badge_categories != null)
            {
                for (int i = 0; i < _config.badge_categories.Count; i++)
                {
                    var badge = _config.badge_categories[i];
                    if (badge == null || string.IsNullOrWhiteSpace(badge.key))
                        continue;

                    if (badge.key == "unknown_damage")
                    {
                        poi.badge_category = badge.key;
                        break;
                    }
                }
            }
        }

        // Hierarchy Level dropdown: maps level labels back to their stable keys.
        // Follows the same pattern as DrawStatusLevelDropdown -- select by current
        // key, display by label, write back the key. "(none)" option clears the
        // key so the marker falls through to MarkerHierarchyResolver.Fallback.
        private string DrawHierarchyLevelDropdown(string label, string currentKey)
        {
            if (_config?.hierarchy_levels == null || _config.hierarchy_levels.Count == 0)
            {
                EditorGUILayout.LabelField(label, "No hierarchy levels defined (see Global Scene).", EditorStyles.miniLabel);
                return currentKey;
            }

            var entries = _config.hierarchy_levels;
            var labels = new string[entries.Count + 1];
            labels[0] = "(none)";

            int selectedIndex = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                    continue;

                string display = string.IsNullOrWhiteSpace(entry.label) ? entry.key : entry.label;
                labels[i + 1] = display;

                if (entry.key == currentKey)
                    selectedIndex = i + 1;
            }

            int next = EditorGUILayout.Popup(label, selectedIndex, labels);
            return next == 0 ? null : entries[next - 1].key;
        }

        private string DrawCategoryDropdown(string label, string current)
        {
            var options = CollectCategoryOptions();
            int idx = Mathf.Max(0, options.IndexOf(current));
            int next = EditorGUILayout.Popup(label, idx, options.ToArray());
            return options[next];
        }

        private string DrawBadgeCategoryDropdown(string label, string current)
        {
            var options = new List<string> { "" };
            if (_config?.badge_categories != null)
            {
                foreach (var entry in _config.badge_categories)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                        continue;
                    if (!options.Contains(entry.key))
                        options.Add(entry.key);
                }
            }

            int idx = Mathf.Max(0, options.IndexOf(current));
            int next = EditorGUILayout.Popup(label, idx, options.ToArray());
            return options[next];
        }

        private void DrawStatusLevelDropdown(POIData poi)
        {
            var levels = _config.outline_levels;
            if (levels == null || levels.Count == 0)
            {
                poi.status_pct = EditorGUILayout.Slider("Status %", poi.status_pct, 0f, 100f);
                return;
            }

            var labels = new string[levels.Count];
            int selectedIndex = 0;
            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                string levelLabel = !string.IsNullOrWhiteSpace(level.label) ? level.label : (level.key ?? $"Level {i + 1}");
                labels[i] = levelLabel + " (" + level.pct.ToString("0") + "%)";

                if (!string.IsNullOrWhiteSpace(poi.status_level_key) && poi.status_level_key == level.key)
                    selectedIndex = i;
            }

            int next = EditorGUILayout.Popup("Status level", selectedIndex, labels);
            next = Mathf.Clamp(next, 0, levels.Count - 1);
            poi.status_level_key = levels[next].key;
            poi.status_pct = levels[next].pct;
            EditorGUILayout.LabelField("Resolved status %", poi.status_pct.ToString("0.0"));
        }

        private List<string> CollectCategoryOptions()
        {
            var options = new List<string>();

            if (_config?.category_styles != null)
            {
                foreach (var entry in _config.category_styles)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.category))
                        continue;
                    if (!options.Contains(entry.category))
                        options.Add(entry.category);
                }
            }

            if (options.Count == 0)
                options.Add("unknown");

            return options;
        }

        private bool GetPoiFoldout(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            if (!_poiFoldouts.TryGetValue(key, out bool expanded))
            {
                expanded = true;
                _poiFoldouts[key] = true;
            }

            return expanded;
        }

        // Per-POI search keywords editor (Block 5, Phase 5.1, task 4).
        // Edits POIData.search_keywords via a multi-line TextField popup.
        private void DrawPoiSearchKeywordsField(POIData poi)
        {
            if (poi == null)
                return;

            if (poi.search_keywords == null)
                poi.search_keywords = new List<string>();
            if (poi.search_keyword_fields == null)
                poi.search_keyword_fields = new List<POISearchKeywordField>();

            // --- Derived keywords (read-only: auto-applied from taxonomy assignments) ---
            var derived = CollectDerivedKeywords(poi);
            if (derived.Count > 0)
            {
                EditorGUILayout.LabelField("Auto-included from taxonomy (read-only)", EditorStyles.miniLabel);
                EditorGUILayout.HelpBox(string.Join(", ", derived), MessageType.None);
            }
            else
            {
                EditorGUILayout.LabelField("No taxonomy keywords yet (assign category/badge/outline/hierarchy in the tables above).", EditorStyles.wordWrappedMiniLabel);
            }

            // --- Custom fields (one editable keyword row per SearchFieldDefinition) ---
            if (_config?.search_fields != null && _config.search_fields.Count > 0)
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Custom keyword fields", EditorStyles.boldLabel);

                foreach (var fieldDef in _config.search_fields)
                {
                    if (fieldDef == null || string.IsNullOrWhiteSpace(fieldDef.key))
                        continue;

                    // Find or create the matching entry on this POI.
                    var entry = poi.search_keyword_fields.Find(e => e.field_key == fieldDef.key);
                    if (entry == null)
                    {
                        entry = new POISearchKeywordField { field_key = fieldDef.key, keywords = new List<string>() };
                        poi.search_keyword_fields.Add(entry);
                    }

                    string displayLabel = string.IsNullOrWhiteSpace(fieldDef.label) ? fieldDef.key : fieldDef.label;
                    bool isEmpty = entry.keywords == null || entry.keywords.Count == 0;

                    // Show a warning icon next to the label when the field is forced and empty.
                    if (fieldDef.forced && isEmpty)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(
                                EditorGUIUtility.IconContent("console.warnicon.sml"),
                                GUILayout.Width(18f), GUILayout.Height(18f));
                            EditorGUILayout.LabelField($"{displayLabel} (required)", EditorStyles.boldLabel);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(displayLabel, EditorStyles.miniLabel);
                    }

                    string joined = entry.keywords != null ? string.Join(", ", entry.keywords) : string.Empty;
                    string edited = EditorGUILayout.TextField(joined);
                    if (edited != joined)
                    {
                        entry.keywords = ParseKeywordListStatic(edited);
                        _hasUnsavedChanges = true;
                    }
                }
            }

            // --- Others row (freeform flat keywords) ---
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Others (freeform)", EditorStyles.miniLabel);
            DrawHelpButton(SearchKeywordsOthersHelp);
            string othersJoined = string.Join(", ", poi.search_keywords);
            string othersEdited = EditorGUILayout.TextField(othersJoined);
            if (othersEdited != othersJoined)
            {
                poi.search_keywords = ParseKeywordListStatic(othersEdited);
                _hasUnsavedChanges = true;
            }
        }

        // Collect the keywords that will be auto-included at index-build time from
        // this POI's taxonomy assignments. Read-only in the UI -- just for developer
        // visibility of what the index will pick up without manual entry.
        private List<string> CollectDerivedKeywords(POIData poi)
        {
            var keywords = new List<string>();
            if (_config == null)
                return keywords;

            // Category keywords.
            if (!string.IsNullOrEmpty(poi.category) && _config.category_styles != null)
            {
                foreach (var entry in _config.category_styles)
                {
                    if (entry?.category == poi.category && entry.search_keywords != null)
                        keywords.AddRange(entry.search_keywords);
                }
            }

            // Badge keywords.
            if (!string.IsNullOrEmpty(poi.badge_category) && _config.badge_categories != null)
            {
                foreach (var entry in _config.badge_categories)
                {
                    if (entry?.key == poi.badge_category && entry.search_keywords != null)
                        keywords.AddRange(entry.search_keywords);
                }
            }

            // Outline / status keywords.
            if (!string.IsNullOrEmpty(poi.status_level_key) && _config.outline_levels != null)
            {
                foreach (var entry in _config.outline_levels)
                {
                    if (entry?.key == poi.status_level_key && entry.search_keywords != null)
                        keywords.AddRange(entry.search_keywords);
                }
            }

            // Hierarchy keywords.
            if (!string.IsNullOrEmpty(poi.hierarchy_level_key) && _config.hierarchy_levels != null)
            {
                foreach (var entry in _config.hierarchy_levels)
                {
                    if (entry?.key == poi.hierarchy_level_key && entry.search_keywords != null)
                        keywords.AddRange(entry.search_keywords);
                }
            }

            return keywords;
        }

        // Render a small inline help button that opens a HelpInfoPopup.
        private static void DrawHelpButton(string message)
        {
            if (GUILayout.Button("?", GUILayout.Width(26f), GUILayout.Height(16f)))
                PopupWindow.Show(GUILayoutUtility.GetLastRect(), new HelpInfoPopup("Help", message));
        }

        // Parse a comma-separated keyword string into a list, trimming empties.
        // Duplicated from SymbolTable.cs to avoid assembly-boundary issues
        // (this partial is in the same assembly, but keeps the method self-contained).
        private static List<string> ParseKeywordListStatic(string text)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return result;

            foreach (string part in text.Split(','))
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    result.Add(trimmed);
            }
            return result;
        }

        // Whether the Focus button is clickable: rig must contain a child
        // named after this POI's id (rig child name == poi.id binding key).
        private bool CanFocusPoiInScene(POIData poi)
        {
            if (poi == null || string.IsNullOrWhiteSpace(poi.id))
                return false;
            Transform rig = GetExistingRig();
            if (rig == null)
                return false;
            return rig.Find(poi.id) != null;
        }

        // Standard Unity workflow, automated: select the rig child (highlights
        // it in Hierarchy, shows its Transform in Inspector so you can drag it
        // with normal move tools), ping it, and frame a tight context box in
        // the Scene view. FrameSelected alone uses the renderer's bounds, which
        // for a uGUI marker is near-zero, so the view stays zoomed far out.
        // Instead we frame a cube sized to ~4 marker-widths across, keeping the
        // current view direction (Frame only dollies to fit). Framing only:
        // moves no object, writes no config.
        private void FocusPoiInScene(POIData poi)
        {
            if (poi == null || string.IsNullOrWhiteSpace(poi.id))
                return;
            Transform rig = GetExistingRig();
            if (rig == null)
                return;
            Transform child = rig.Find(poi.id);
            if (child == null)
                return;
            Selection.activeGameObject = child.gameObject;
            EditorGUIUtility.PingObject(child.gameObject);
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
                return;
            var markerView = child.GetComponentInChildren<MarkerView>();
            float diameter = markerView != null
                ? markerView.GetVisualRadiusWorld() * 2f * child.lossyScale.x
                : 0f;
            float width = PoiFocusResolver.ComputeFocusWidth(diameter);
            sceneView.Frame(new Bounds(child.position, Vector3.one * width), false);
        }

        // Add a new POI with sensible defaults.
        // If atEnd is true, appends to end; otherwise inserts at index 0 (first).
        private void AddNewPoiInternal(bool atEnd = false)
        {
            if (_config == null) return;
            if (_config.pois == null) _config.pois = new List<POIData>();

            // Determine initial rotation: first POI gets 0, subsequent inherit from previous.
            float initialRotation = PoiRotationResolver.DefaultEditorRotationDeg;
            if (_config.pois.Count > 0)
            {
                var prevPoi = atEnd ? _config.pois[_config.pois.Count - 1] : _config.pois[0];
                initialRotation = prevPoi.editor_rotation_deg;
            }

            var newPoi = new POIData
            {
                id = System.Guid.NewGuid().ToString("N"),
                name = "New POI",
                category = "default",
                editor_rotation_deg = initialRotation,
                has_captured_position = false,
                status_pct = 0f,
                has_status = false,
                status_unknown = false,
                hierarchy_level_key = null,
                has_custom_symbol = false,
                custom_symbol_key = null,
                badge_category = null,
                search_keywords = new List<string>(),
                search_keyword_fields = new List<POISearchKeywordField>()
            };

            DrawConfigMutationScope(() =>
            {
                if (atEnd)
                    _config.pois.Add(newPoi);
                else
                    _config.pois.Insert(0, newPoi);
            }, true);

            // Auto-spawn + auto-focus for the new POI
            SpawnAndFocusNewPoi(newPoi);
        }

        // Parameterless overload used by UI buttons and tests (appends to end).
        private void AddNewPoi() => AddNewPoiInternal(true);

        // Insert a new POI immediately after the given index.
        private void AddNewPoiAfter(int index)
        {
            if (_config == null) return;
            if (_config.pois == null) _config.pois = new List<POIData>();
            if (index < 0 || index >= _config.pois.Count) return;

            // Inherit rotation from the POI we're inserting after.
            float initialRotation = _config.pois[index].editor_rotation_deg;

            var newPoi = new POIData
            {
                id = System.Guid.NewGuid().ToString("N"),
                name = "New POI",
                category = "default",
                editor_rotation_deg = initialRotation,
                has_captured_position = false,
                status_pct = 0f,
                has_status = false,
                status_unknown = false,
                hierarchy_level_key = null,
                has_custom_symbol = false,
                custom_symbol_key = null,
                badge_category = null,
                search_keywords = new List<string>(),
                search_keyword_fields = new List<POISearchKeywordField>()
            };

            DrawConfigMutationScope(() => _config.pois.Insert(index + 1, newPoi), true);

            // Auto-spawn + auto-focus for the new POI
            SpawnAndFocusNewPoi(_config.pois[index + 1]);
        }

        // Spawns a rig child for the given POI and focuses the Scene view on it.
        private void SpawnAndFocusNewPoi(POIData poi)
        {
            var rig = GetOrCreateRig();
            if (rig == null)
            {
                // No rig available (e.g. no CorrectionAnchor in scene). Warn but keep POI in config.
                EditorGUILayout.HelpBox("No authoring rig found. Add a PlacementCorrectionAnchor to the scene to see markers in the Scene view.", MessageType.Warning);
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[POIAuthoring] Prefab not found at {_prefabPath}");
                return;
            }

            // Resolve initial position: first POI at origin, subsequent at previous POI + offset.
            Vector3 initialPosition;
            int myIndex = _config.pois.IndexOf(poi);
            if (myIndex == 0 && _config.pois.Count == 1)
            {
                // First POI ever → origin.
                initialPosition = Vector3.zero;
            }
            else
            {
                // Find the "previous" POI in the list (the one before this one).
                POIData prevPoi = null;
                if (myIndex > 0)
                {
                    prevPoi = _config.pois[myIndex - 1];
                }
                else if (_config.pois.Count > 1)
                {
                    // Inserted at front; use the old-first as reference.
                    prevPoi = _config.pois[1];
                }

                if (prevPoi != null)
                {
                    // Use previous POI's rig child position + small offset.
                    var prevChild = GetExistingRig()?.Find(prevPoi.id);
                    if (prevChild != null)
                    {
                        initialPosition = prevChild.localPosition + new Vector3(0.3f, 0f, 0.3f);
                    }
                    else
                    {
                        initialPosition = Vector3.zero;
                    }
                }
                else
                {
                    initialPosition = Vector3.zero;
                }
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, GetExistingRig());
            instance.name = poi.id;
            instance.transform.localPosition = initialPosition;
            instance.transform.localRotation = PoiRotationResolver.ToYawQuaternion(poi.editor_rotation_deg);
            Undo.RegisterCreatedObjectUndo(instance, "Create POI Marker");

            // Reuse the same visual configuration logic as PopulateRig.
            ConfigureRigChild(poi, instance.transform);

            // Select and frame the new marker.
            FocusPoiInScene(poi);
        }
    }
}
