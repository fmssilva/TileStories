using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private void DrawSpecificMarkerOptions()
        {
            if (_config.pois == null || _config.pois.Count == 0)
            {
                EditorGUILayout.HelpBox("No POIs yet. Use the + buttons below to add your first POI.", MessageType.Info);
                // Rendered as a shared editor row: transparent indent spacer + width
                // capped to max(MinRowWidth, min(visible panel, MaxRowWidth)).
                DrawEditorRow(out float firstRowWidth, out _);
                if (GUILayout.Button("+ Add first", GUILayout.Width(firstRowWidth), GUILayout.ExpandWidth(false)))
                {
                    AddNewPoi(); // adds at index 0
                }
                EditorRowEnd();
                return;
            }

            // "+ Add before first" button (down arrow) before the first POI.
            // Rendered as a shared editor row: transparent indent spacer + width
            // capped to max(MinRowWidth, min(visible panel, MaxRowWidth)).
            DrawEditorRow(out float beforeRowWidth, out _);
            if (GUILayout.Button("+ Add POI before next ▼", GUILayout.Width(beforeRowWidth), GUILayout.ExpandWidth(false)))
            {
                AddNewPoiInternal(false); // insert at index 0
            }
            EditorRowEnd();

            for (int i = 0; i < _config.pois.Count; i++)
            {
                var poi = _config.pois[i];
                if (poi == null)
                    continue;

                string foldoutKey = string.IsNullOrWhiteSpace(poi.id) ? $"poi_{i}" : poi.id;
                bool expanded = GetPoiFoldout(foldoutKey);
                string editModeKey = $"editName_{foldoutKey}";
                bool isEditing = SessionState.GetBool(editModeKey, false);

                var headerStyle = CreateFoldoutStyle(PoiHeaderColorFor(foldoutKey, i));
                var headerRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

                // Layout: [foldout arrow] [name click zone] [pencil right after name].
                // The pencil is measured off the name text width (not pinned to
                // headerRect.xMax) so it hugs the label even on wide windows.
                const float arrowWidth = 14f;
                const float pencilWidth = 22f;
                var arrowRect = new Rect(headerRect.x, headerRect.y, arrowWidth, headerRect.height);
                string displayName = $"{i + 1}. {poi.name}";
                float nameWidth = headerStyle.CalcSize(new GUIContent(displayName)).x;
                var nameRect = new Rect(headerRect.x + arrowWidth, headerRect.y, nameWidth + 4f, headerRect.height);
                const float pencilGapX = 20f;  // breathing room between name text and pencil
                var pencilRect = new Rect(nameRect.xMax + pencilGapX, headerRect.y, pencilWidth, headerRect.height);

                // Foldout arrow (click toggles)
                expanded = EditorGUI.Foldout(arrowRect, expanded, "", true, headerStyle);
                if (isEditing)
                {
                    // Edit mode: full-width text field so long names are editable.
                    string fieldRectName = $"POIName_{foldoutKey}";
                    string editValueKey = $"editValue_{foldoutKey}";
                    var e = Event.current;

                    // Resolve Enter/Escape BEFORE the TextField draws. Unity's
                    // TextField consumes the first Return it sees (commit +
                    // keyboard-focus release), turning the event into
                    // EventType.Used before any post-field check could see it --
                    // the root cause of the "press Enter twice" bug. Consuming
                    // the event pre-field is deterministic; the decision table
                    // lives in PoiRenameKeys (pure, Tier-0 tested).
                    var keyAction = PoiRenameKeys.Resolve(e.type, e.keyCode);
                    if (keyAction == PoiRenameKeys.Action.Commit)
                    {
                        // The draft mirror is current as of the last keystroke pass.
                        SaveNameChange(poi, foldoutKey, editModeKey, editValueKey,
                            SessionState.GetString(editValueKey, displayName));
                        e.Use();
                    }
                    else if (keyAction == PoiRenameKeys.Action.Cancel)
                    {
                        // Cancel on Escape -- discard edits, keep the old name.
                        SessionState.SetBool(editModeKey, false);
                        SessionState.EraseString(editValueKey);
                        SessionState.EraseString(editValueKey + "_focused");
                        GUI.FocusControl(null);
                        e.Use();
                    }

                    GUI.SetNextControlName(fieldRectName);
                    var editRect = new Rect(headerRect.x + arrowWidth, headerRect.y, headerRect.width - arrowWidth, headerRect.height);
                    // TextField returns the live value every repaint; the SessionState
                    // draft mirrors it so Enter/click-outside commit the typed text.
                    var newName = EditorGUI.TextField(editRect, SessionState.GetString(editValueKey, displayName));
                    if (GUI.changed && GUI.GetNameOfFocusedControl() == fieldRectName)
                        SessionState.SetString(editValueKey, newName);

                    // Focus once on entering edit mode so the cursor lands inside.
                    if (Event.current.type == EventType.Repaint && string.IsNullOrEmpty(SessionState.GetString(editValueKey + "_focused", "")))
                    {
                        GUI.FocusControl(fieldRectName);
                        SessionState.SetString(editValueKey + "_focused", "1");
                    }

                    // Click outside the field commits the rename.
                    if (e.type == EventType.MouseDown && !editRect.Contains(e.mousePosition))
                    {
                        SaveNameChange(poi, foldoutKey, editModeKey, editValueKey, newName);
                    }
                }
                else
                {
                    // Display mode: transparent button over the name toggles the foldout.
                    if (GUI.Button(nameRect, GUIContent.none, GUIStyle.none))
                        expanded = !expanded;
                    EditorGUI.LabelField(nameRect, displayName, headerStyle);
                }

                // Pencil button right after the name; hidden while this POI is in
                // edit mode (the field replaces the label + pencil for that row).
                if (!isEditing)
                {
                    Texture2D editTex = AssetDatabase.LoadAssetAtPath<Texture2D>(EditIconAssetPath);
                    var editContent = new GUIContent(editTex);
                    Color prevColor = GUI.color;
                    GUI.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                    if (GUI.Button(pencilRect, editContent, GUIStyle.none))
                    {
                        SessionState.SetBool(editModeKey, true);
                        SessionState.SetString($"editValue_{foldoutKey}", displayName);
                        SessionState.EraseString($"editValue_{foldoutKey}_focused");
                    }
                    GUI.color = prevColor;
                }

                // Minimal reorder controls: move this POI earlier/later in the config list.
                // The config list is the canonical order persisted to JSON, and the save flow
                // already serializes the whole object so there is no custom file rewrite logic.
                if (!isEditing)
                {
                    const float reorderButtonWidth = 22f;
                    const float deleteButtonWidth = 34f;
                    const float reorderGapX = 4f;
                    const float deleteGapX = 6f;
                    var upRect = new Rect(pencilRect.xMax + reorderGapX, headerRect.y, reorderButtonWidth, headerRect.height);
                    var downRect = new Rect(upRect.xMax + reorderGapX, headerRect.y, reorderButtonWidth, headerRect.height);
                    var deleteRect = new Rect(downRect.xMax + deleteGapX, headerRect.y, deleteButtonWidth, headerRect.height);

                    using (new EditorGUI.DisabledScope(i == 0))
                    {
                        if (GUI.Button(upRect, "↑", EditorStyles.miniButton))
                        {
                            DrawConfigMutationScope(() => ReorderPoi(_config.pois, i, -1), true);
                        }
                    }

                    using (new EditorGUI.DisabledScope(i >= _config.pois.Count - 1))
                    {
                        if (GUI.Button(downRect, "↓", EditorStyles.miniButton))
                        {
                            DrawConfigMutationScope(() => ReorderPoi(_config.pois, i, 1), true);
                        }
                    }

                    var deleteStyle = new GUIStyle(EditorStyles.miniButton);
                    deleteStyle.fontSize = 14;
                    deleteStyle.fontStyle = FontStyle.Bold;
                    deleteStyle.alignment = TextAnchor.MiddleCenter;
                    deleteStyle.padding = new RectOffset(0, 0, 0, 0);
                    deleteStyle.border = new RectOffset(1, 1, 1, 1);
                    deleteStyle.fixedHeight = 24f;
                    deleteStyle.fixedWidth = 34f;
                    deleteStyle.normal.textColor = new Color(1f, 0.82f, 0.82f, 1f);
                    deleteStyle.hover.textColor = Color.white;
                    deleteStyle.active.textColor = Color.white;
                    deleteStyle.focused.textColor = Color.white;

                    Color prevBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.72f, 0.18f, 0.18f, 1f);
                    if (GUI.Button(deleteRect, TrashIcon, deleteStyle))
                    {
                        TryDeletePoiAt(i, poi);
                    }
                    GUI.backgroundColor = prevBg;
                }

                _poiFoldouts[foldoutKey] = expanded;

                if (!expanded)
                {
                    if (i < _config.pois.Count - 1)
                        DrawSeparatorButtons(i);
                    continue;
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    // Focus helper, tinted with this POI's header color so the
                    // action reads as belonging to the marker above. Disabled
                    // until the rig has a child named poi.id (populate first).
                    // Shared editor row so the Focus helper aligns with the
                    // foldout indent and stays within the capped row width.
                    DrawEditorRow(out float focusRowWidth, out _);
                    {
                        using (new EditorGUI.DisabledScope(!CanFocusPoiInScene(poi)))
                        {
                            Color poiColor = PoiHeaderColorFor(foldoutKey, i);
                            var prevBg = GUI.backgroundColor;
                            GUI.backgroundColor = poiColor;
                            // Leave room in the row for the trailing help button.
                            float focusWidth = Mathf.Max(140f, focusRowWidth - 40f);
                            if (GUILayout.Button("Focus in Scene", GUILayout.Width(focusWidth), GUILayout.ExpandWidth(false)))
                                FocusPoiInScene(poi);
                            GUI.backgroundColor = prevBg;
                        }

                        GUILayout.Space(10f);
                        HelpInfoButton.Draw("Focus in Scene", FocusInSceneHelpBody);
                    }
                    EditorRowEnd();

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
                }

                // Separator buttons after this POI (except after last)
                if (i < _config.pois.Count - 1)
                    DrawSeparatorButtons(i);

                EditorGUILayout.Space(6f);
            }

            // "+ Add after last" button (up arrow) after the last POI.
            // Rendered as a shared editor row: transparent indent spacer + width
            // capped to max(MinRowWidth, min(visible panel, MaxRowWidth)).
            DrawEditorRow(out float afterRowWidth, out _);
            if (GUILayout.Button("+ Add POI after previous ▲", GUILayout.Width(afterRowWidth), GUILayout.ExpandWidth(false)))
            {
                AddNewPoiInternal(true); // appends at end
            }
            EditorRowEnd();
        }

        private void DrawSeparatorButtons(int index)
        {
            // index is the current POI index. We are drawing separator after this POI (i.e., between index and index+1)
            EditorGUILayout.Space(4f);
            // Wrap the separator pair in the shared row so it gets the same
            // indent spacer and stays within the capped row width. Two buttons,
            // each half the row so they never exceed max(Min, min(panel, Max)).
            DrawEditorRow(out float sepRowWidth, out _);
            {
                string prevLabel = "+ Add POI near previous ▲";
                string nextLabel = "+ Add POI near next ▼";
                float sepHalf = Mathf.Max(120f, (sepRowWidth - 4f) / 2f);
                // "+ Add POI after previous ^" (up arrow) - inserts after current index
                if (GUILayout.Button(prevLabel, GUILayout.Width(sepHalf), GUILayout.ExpandWidth(false)))
                {
                    AddNewPoiAfter(index);
                }
                // "+ Add POI before next ▼" (down arrow) - inserts before next
                if (GUILayout.Button(nextLabel, GUILayout.Width(sepHalf), GUILayout.ExpandWidth(false)))
                {
                    AddNewPoiBefore(index + 1);
                }
            }
            EditorRowEnd();
            EditorGUILayout.Space(4f);
        }


        internal static void ReorderPoi(List<POIData> pois, int index, int direction)
        {
            if (pois == null || index < 0 || index >= pois.Count)
                return;

            int targetIndex = index + direction;
            if (targetIndex < 0 || targetIndex >= pois.Count)
                return;

            var temp = pois[index];
            pois[index] = pois[targetIndex];
            pois[targetIndex] = temp;
        }

        internal static bool RemovePoiAt(List<POIData> pois, IDictionary<string, bool> poiFoldouts, int index)
        {
            if (pois == null || poiFoldouts == null || index < 0 || index >= pois.Count)
                return false;

            var poi = pois[index];
            pois.RemoveAt(index);

            if (poi != null && !string.IsNullOrWhiteSpace(poi.id) && poiFoldouts.ContainsKey(poi.id))
                poiFoldouts.Remove(poi.id);

            return true;
        }

        private void TryDeletePoiAt(int index, POIData poi)
        {
            if (_config == null || _config.pois == null || poi == null)
                return;

            if (index < 0 || index >= _config.pois.Count)
                return;

            string poiName = string.IsNullOrWhiteSpace(poi.name) ? "this POI" : poi.name;
            bool confirm = EditorUtility.DisplayDialog(
                "Delete POI?",
                $"Are you sure you want to delete \"{poiName}\"?\n\nThis removes the POI from the config and from the Scene rig.",
                "Delete",
                "Cancel");

            if (!confirm)
                return;

            var rig = GetExistingRig();
            if (rig != null)
            {
                var child = rig.Find(poi.id);
                if (child != null)
                    Undo.DestroyObjectImmediate(child.gameObject);
            }

            DrawConfigMutationScope(() =>
            {
                if (_config.pois.Count > index && _config.pois[index] == poi)
                    RemovePoiAt(_config.pois, _poiFoldouts, index);
            }, true);
        }

        private void DrawPoiMarkerStyleFields(POIData poi)
        {
            using (new EditorGUI.IndentLevelScope(-1))
            {
            // Note: the POI name is renamed from the header row pencil, not here --
            // a second name field would fight the header draft (two writers, one field).
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
            // Shared row: transparent indent spacer + labelled Toggle capped to rowWidth.
            DrawEditorRow(out float customToggleRow, out _);
            poi.has_custom_symbol = EditorGUILayout.Toggle("Use Custom Symbol", poi.has_custom_symbol,
                GUILayout.Width(customToggleRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();

            if (poi.has_custom_symbol)
            {
                // Multi-element shared row: label + preview keep fixed widths; the
                // ObjectField takes the remaining rowWidth, all inside the same capped row.
                DrawEditorRow(out float symbolRow, out _);
                {
                    EditorGUILayout.LabelField("Custom symbol (optional)", GUILayout.Width(150f));
                    Sprite current = ResolveSpriteForKey(poi.custom_symbol_key);
                    DrawSpritePreview(current);
                    float symbolFieldW = Mathf.Max(90f, symbolRow - 150f - 36f - 12f);
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false,
                        GUILayout.Width(symbolFieldW), GUILayout.ExpandWidth(false));
                    if (chosen != current)
                        poi.custom_symbol_key = chosen != null ? AssignSpriteToLibraryAndGetKey(chosen, poi.id + "_symbol") : null;
                }
                EditorRowEnd();
                EditorGUILayout.LabelField("Overrides just this POI's icon (e.g. a small castle glyph). Category color, ring, and badge stay unchanged.", EditorStyles.wordWrappedMiniLabel);
            }
            }
        }

        private void DrawPoiBadgeStyleFields(POIData poi)
        {
            using (new EditorGUI.IndentLevelScope(-1))
            {
            poi.badge_category = DrawBadgeCategoryDropdown("Badge category", poi.badge_category);
            }
        }

        private void DrawPoiOutlineFields(POIData poi)
        {
            using (new EditorGUI.IndentLevelScope(-1))
            {
            bool hasStatus = poi.has_status;
            // Shared row: transparent indent spacer + labelled Toggle capped to rowWidth.
            DrawEditorRow(out float hasStatusRow, out _);
            bool wantsStatus = EditorGUILayout.Toggle("Has status", hasStatus,
                GUILayout.Width(hasStatusRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();

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
                {
                    // Shared row: transparent indent spacer + labelled Slider capped to rowWidth.
                    DrawEditorRow(out float statusPctRow, out _);
                    poi.status_pct = EditorGUILayout.Slider("Status %", poi.status_pct, 0f, 100f,
                        GUILayout.Width(statusPctRow), GUILayout.ExpandWidth(false));
                    EditorRowEnd();
                }

                bool wasUnknown = poi.status_unknown;
                // Shared row: transparent indent spacer + labelled Toggle capped to rowWidth.
                DrawEditorRow(out float statusUnknownRow, out _);
                poi.status_unknown = EditorGUILayout.Toggle("Status unknown", poi.status_unknown,
                    GUILayout.Width(statusUnknownRow), GUILayout.ExpandWidth(false));
                EditorRowEnd();
                if (!wasUnknown && poi.status_unknown)
                    ApplyUnknownStatusDefaults(poi);
            }

            // rotate_contour is now a hierarchy-level property, not a per-POI field.
            // Configured in the Global Scene Hierarchy table (see DrawGlobalHierarchySection).
            }
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

            // Shared row: transparent indent spacer + labelled Popup capped to rowWidth.
            DrawEditorRow(out float hierarchyRow, out _);
            int next = EditorGUILayout.Popup(label, selectedIndex, labels,
                GUILayout.Width(hierarchyRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            return next == 0 ? null : entries[next - 1].key;
        }

        private string DrawCategoryDropdown(string label, string current)
        {
            var options = CollectCategoryOptions();
            int idx = Mathf.Max(0, options.IndexOf(current));
            // Shared row: transparent indent spacer + labelled Popup capped to rowWidth.
            DrawEditorRow(out float categoryRow, out _);
            int next = EditorGUILayout.Popup(label, idx, options.ToArray(),
                GUILayout.Width(categoryRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
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
            // Shared row: transparent indent spacer + labelled Popup capped to rowWidth.
            DrawEditorRow(out float badgeRow, out _);
            int next = EditorGUILayout.Popup(label, idx, options.ToArray(),
                GUILayout.Width(badgeRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            return options[next];
        }

        private void DrawStatusLevelDropdown(POIData poi)
        {
            var levels = _config.outline_levels;
            if (levels == null || levels.Count == 0)
            {
                // Shared row: transparent indent spacer + labelled Slider capped to rowWidth.
                DrawEditorRow(out float statusPctFallbackRow, out _);
                poi.status_pct = EditorGUILayout.Slider("Status %", poi.status_pct, 0f, 100f,
                    GUILayout.Width(statusPctFallbackRow), GUILayout.ExpandWidth(false));
                EditorRowEnd();
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

            // Shared row: transparent indent spacer + labelled Popup capped to rowWidth
            // (no trailing resolved-status label).
            DrawEditorRow(out float statusRow, out _);
            int next = EditorGUILayout.Popup("Status level", selectedIndex, labels,
                GUILayout.Width(statusRow), GUILayout.ExpandWidth(false));
            next = Mathf.Clamp(next, 0, levels.Count - 1);
            poi.status_level_key = levels[next].key;
            poi.status_pct = levels[next].pct;
            EditorRowEnd();
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
            using (new EditorGUI.IndentLevelScope(-1))
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
                // Shared row: transparent indent spacer + read-only label capped to rowWidth,
                // with the trailing help button in the same capped row.
                DrawEditorRow(out float derivedRow, out _);
                {
                    float derivedLabelW = Mathf.Max(140f, derivedRow - 40f);
                    EditorGUILayout.LabelField("Auto-included from taxonomy (read-only)", EditorStyles.miniLabel,
                        GUILayout.Width(derivedLabelW), GUILayout.ExpandWidth(false));
                    DrawHelpButton(SearchKeywordsDerivedHelp);
                }
                EditorRowEnd();
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
                        // Shared row: transparent indent spacer + warning icon + required label.
                        DrawEditorRow(out float requiredRow, out _);
                        {
                            EditorGUILayout.LabelField(
                                EditorGUIUtility.IconContent("console.warnicon.sml"),
                                GUILayout.Width(18f), GUILayout.Height(18f));
                            float requiredLabelW = Mathf.Max(120f, requiredRow - 30f);
                            EditorGUILayout.LabelField($"{displayLabel} (required)", EditorStyles.boldLabel,
                                GUILayout.Width(requiredLabelW), GUILayout.ExpandWidth(false));
                        }
                        EditorRowEnd();
                    }
                    else
                    {
                        EditorGUILayout.LabelField(displayLabel, EditorStyles.miniLabel);
                    }

                    string joined = entry.keywords != null ? string.Join(", ", entry.keywords) : string.Empty;
                    // Shared row: transparent indent spacer + TextField capped to rowWidth.
                    DrawEditorRow(out float keywordRow, out _);
                    string edited = EditorGUILayout.TextField(joined,
                        GUILayout.Width(keywordRow), GUILayout.ExpandWidth(false));
                    EditorRowEnd();
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
            // Shared row: transparent indent spacer + TextField capped to rowWidth.
            DrawEditorRow(out float othersRow, out _);
            string othersEdited = EditorGUILayout.TextField(othersJoined,
                GUILayout.Width(othersRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            if (othersEdited != othersJoined)
            {
                poi.search_keywords = ParseKeywordListStatic(othersEdited);
                _hasUnsavedChanges = true;
            }
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
        // Frame only: move the scene camera, not the POI, and do not write config.
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

        private static POIData CreateDefaultPoi(float initialRotation, POIData sourcePoi = null)
        {
            var poi = new POIData
            {
                id = System.Guid.NewGuid().ToString("N"),
                name = "New POI",
                category = "default",
                editor_rotation_deg = initialRotation,
                position_verified = false,
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

            if (sourcePoi != null)
            {
                poi.category = sourcePoi.category;
                poi.hierarchy_level_key = sourcePoi.hierarchy_level_key;
                poi.has_custom_symbol = sourcePoi.has_custom_symbol;
                poi.custom_symbol_key = sourcePoi.custom_symbol_key;
                poi.badge_category = sourcePoi.badge_category;
                poi.has_status = sourcePoi.has_status;
                poi.status_unknown = sourcePoi.status_unknown;
                poi.status_pct = sourcePoi.status_pct;
                poi.search_keywords = sourcePoi.search_keywords != null ? new List<string>(sourcePoi.search_keywords) : new List<string>();
                poi.search_keyword_fields = sourcePoi.search_keyword_fields != null ? new List<POISearchKeywordField>(sourcePoi.search_keyword_fields) : new List<POISearchKeywordField>();
                poi.editor_rotation_deg = sourcePoi.editor_rotation_deg;
            }

            return poi;
        }

        private void InsertPoiAt(int index, POIData poi)
        {
            DrawConfigMutationScope(() => _config.pois.Insert(index, poi), true);
        }

        // Add a new POI with sensible defaults.
        private void AddNewPoiInternal(bool afterPrevious = true)
        {
            if (_config == null) return;
            if (_config.pois == null) _config.pois = new List<POIData>();

            if (_config.pois.Count == 0)
            {
                var fallbackPoi = CreateDefaultPoi(PoiRotationResolver.DefaultEditorRotationDeg);
                InsertPoiAt(0, fallbackPoi);
                SpawnAndFocusNewPoi(fallbackPoi);
                _poiFoldouts[fallbackPoi.id] = true;
                Repaint();
                return;
            }

            if (afterPrevious)
            {
                AddNewPoiAfter(_config.pois.Count - 1);
            }
            else
            {
                AddNewPoiBefore(0);
            }
        }

        // Parameterless overload used by UI buttons and tests (appends to end).
        private void AddNewPoi() => AddNewPoiInternal(true);

        // Insert a new POI immediately after the given index, using that POI as the template and spatial reference.
        private void AddNewPoiAfter(int index)
        {
            if (_config == null) return;
            if (_config.pois == null) _config.pois = new List<POIData>();
            if (index < 0 || index >= _config.pois.Count) return;

            var sourcePoi = _config.pois[index];
            var newPoi = CreateDefaultPoi(sourcePoi.editor_rotation_deg, sourcePoi);
            InsertPoiAt(index + 1, newPoi);

            SpawnAndFocusNewPoi(newPoi, sourcePoi);
            _poiFoldouts[newPoi.id] = true;
            Repaint();
        }

        // Insert a new POI immediately before the given index, using the next POI as the template and spatial reference.
        private void AddNewPoiBefore(int index)
        {
            if (_config == null) return;
            if (_config.pois == null) _config.pois = new List<POIData>();
            if (index < 0 || index > _config.pois.Count) return;

            var sourcePoi = index < _config.pois.Count ? _config.pois[index] : null;
            var newPoi = CreateDefaultPoi(sourcePoi != null ? sourcePoi.editor_rotation_deg : PoiRotationResolver.DefaultEditorRotationDeg, sourcePoi);
            int insertIndex = index;
            InsertPoiAt(insertIndex, newPoi);

            SpawnAndFocusNewPoi(newPoi, sourcePoi);
            _poiFoldouts[newPoi.id] = true;
            Repaint();
        }

        // Spawns a rig child for the given POI and focuses the Scene view on it.
        private void SpawnAndFocusNewPoi(POIData poi, POIData referencePoi = null)
        {
            var rig = GetOrCreateRig();
            if (rig == null)
            {
                // No rig available (e.g. no CorrectionAnchor in scene). Warn but keep POI in config.
                EditorGUILayout.HelpBox("No POI Editor rig found. Add a PlacementCorrectionAnchor to the scene to see markers in the Scene view.", MessageType.Warning);
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[POIEditor] Prefab not found at {_prefabPath}");
                return;
            }

            // Resolve initial position using the relevant adjacent POI as the source template.
            Vector3 initialPosition;
            if (referencePoi != null)
            {
                var referenceChild = GetExistingRig()?.Find(referencePoi.id);
                if (referenceChild != null)
                {
                    initialPosition = referenceChild.localPosition + new Vector3(0.3f, 0f, 0.3f);
                }
                else
                {
                    initialPosition = Vector3.zero;
                }
            }
            else
            {
                int myIndex = _config.pois.IndexOf(poi);
                if (myIndex == 0 && _config.pois.Count == 1)
                {
                    initialPosition = Vector3.zero;
                }
                else
                {
                    POIData prevPoi = null;
                    if (myIndex > 0)
                    {
                        prevPoi = _config.pois[myIndex - 1];
                    }
                    else if (_config.pois.Count > 1)
                    {
                        prevPoi = _config.pois[1];
                    }

                    if (prevPoi != null)
                    {
                        var prevChild = GetExistingRig()?.Find(prevPoi.id);
                        initialPosition = prevChild != null
                            ? prevChild.localPosition + new Vector3(0.3f, 0f, 0.3f)
                            : Vector3.zero;
                    }
                    else
                    {
                        initialPosition = Vector3.zero;
                    }
                }
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, GetExistingRig());
            instance.name = poi.id;
            instance.transform.localPosition = initialPosition;
            instance.transform.localRotation = PoiRotationResolver.ToYawQuaternion(poi.editor_rotation_deg);
            Undo.RegisterCreatedObjectUndo(instance, "Create POI Marker");

            // Persist the initial position into config immediately (unverified).
            // This makes the position survive Save/Load/Populate even if the user
            // never touches the rig sync path.
            poi.position = new PositionData
            {
                x = initialPosition.x,
                y = initialPosition.y,
                z = initialPosition.z
            };
            poi.position_verified = false;

            // Reuse the same visual configuration logic as PopulateRig.
            ConfigureRigChild(poi, instance.transform);

            // Select and frame the new marker.
            FocusPoiInScene(poi);
        }

        // Strip the "N. " numbering prefix the header prepends for display.
        // "3. Lamp" -> "Lamp"; no prefix -> trimmed as-is. Pure, Tier-0 testable.
        internal static string StripNumberPrefix(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            int dotIndex = text.IndexOf(". ");
            string namePart = dotIndex >= 0 ? text.Substring(dotIndex + 2) : text;
            return namePart.Trim();
        }

        // Helper: save name change from edit mode text field
        private void SaveNameChange(POIData poi, string foldoutKey, string editModeKey, string editValueKey, string textToSave)
        {
            string namePart = StripNumberPrefix(textToSave);

            if (!string.IsNullOrWhiteSpace(namePart) && namePart != poi.name)
            {
                DrawConfigMutationScope(() => { poi.name = namePart; }, true);
                RefreshRigVisuals();
            }

            SessionState.SetBool(editModeKey, false);
            SessionState.EraseString($"editValue_{foldoutKey}");
            SessionState.EraseString(editValueKey + "_focused");
        }
    }
}
