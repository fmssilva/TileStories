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
                if (GUILayout.Button("+ Add first POI", GUILayout.Width(firstRowWidth), GUILayout.ExpandWidth(false)))
                {
                    AddFirstPoi();
                }
                EditorRowEnd();
                return;
            }

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
                // Separate LABEL style for the name text: a foldout style carries the arrow
                // texture as its background, so handing it to LabelField painted a SECOND
                // foldout arrow right beside the real one.
                var headerLabelStyle = CreateHeaderLabelStyle(PoiHeaderColorFor(foldoutKey, i));
                var headerRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

                // A reveal (see RevealPoiInSpecificMarkerTab) queued a scroll to this POI. The
                // rect is only real during Repaint, and it is in the scroll view's content
                // space, so it can be assigned straight to _scrollPos.y; the repaint picks it up.
                if (Event.current.type == EventType.Repaint && poi.id == _pendingScrollPoiId)
                {
                    _pendingScrollPoiId = null;
                    _scrollPos.y = Mathf.Max(0f, headerRect.y - 4f);
                    Repaint();
                }

                // Row width follows the same shared convention as every other non-table
                // row in this window (see DrawEditorRow / EditorRowWidthForIndent): capped
                // to max(MinRowWidth, min(panel - margin - indent, MaxRowWidth)), so the
                // header row's right edge lines up with every other row's, not the raw
                // window edge. headerRect.x already IS this row's runtime indent (Unity
                // auto-indents GetControlRect), so it's reused directly as that indent.
                float rowWidth = EditorRowWidthForIndent(EditorGUIUtility.currentViewWidth, AddButtonRowRightMargin, headerRect.x);
                float rowRight = headerRect.x + rowWidth;

                // Layout: [foldout arrow] [flexible name] [icon cluster pinned to the row's
                // right edge: focus, pencil, up, down, +, (i), delete]. The icon cluster is
                // fixed width and computed right-to-left off rowRight FIRST, so it stays put
                // regardless of name length; the name then flexes to fill whatever space is
                // left between the arrow and the cluster (never overlaps it). Delete is the
                // SAME width as its siblings -- the danger affordance is the icon's own red
                // color, not an oversized button.
                const float arrowWidth = 14f;
                const float pencilWidth = 22f;
                const float reorderButtonWidth = 22f;
                const float addButtonWidth = 22f;
                const float infoButtonWidth = 22f;
                const float deleteButtonWidth = 22f;
                const float iconGapX = 4f;
                const float nameToIconsGapX = 12f;
                var arrowRect = new Rect(headerRect.x, headerRect.y, arrowWidth, headerRect.height);

                var deleteRect = new Rect(rowRight - deleteButtonWidth, headerRect.y, deleteButtonWidth, headerRect.height);
                var infoRect = new Rect(deleteRect.x - iconGapX - infoButtonWidth, headerRect.y, infoButtonWidth, headerRect.height);
                var addRect = new Rect(infoRect.x - iconGapX - addButtonWidth, headerRect.y, addButtonWidth, headerRect.height);
                var downRect = new Rect(addRect.x - iconGapX - reorderButtonWidth, headerRect.y, reorderButtonWidth, headerRect.height);
                var upRect = new Rect(downRect.x - iconGapX - reorderButtonWidth, headerRect.y, reorderButtonWidth, headerRect.height);
                var pencilRect = new Rect(upRect.x - iconGapX - pencilWidth, headerRect.y, pencilWidth, headerRect.height);
                var focusRect = new Rect(pencilRect.x - iconGapX - pencilWidth, headerRect.y, pencilWidth, headerRect.height);

                string displayName = $"{i + 1}. {poi.name}";
                // CalcSize slightly under-measures this bold label's real rendered width
                // (confirmed by screenshot: even a 10f pad still clipped the last glyph on
                // longer names) -- pad generously; this only matters when
                // nameAvailableWidth is the smaller of the two, so it never over-reserves.
                float nameNaturalWidth = headerLabelStyle.CalcSize(new GUIContent(displayName)).x + 18f;
                float nameAvailableWidth = Mathf.Max(20f, focusRect.x - nameToIconsGapX - (headerRect.x + arrowWidth));
                var nameRect = new Rect(headerRect.x + arrowWidth, headerRect.y, Mathf.Min(nameNaturalWidth, nameAvailableWidth), headerRect.height);

                // Foldout arrow (click toggles)
                expanded = EditorGUI.Foldout(arrowRect, expanded, "", true, headerStyle);
                if (isEditing)
                {
                    // Edit mode: text field spans the row's own width (not the whole
                    // window) so long names edit inside the same capped row as everything
                    // else, consistent with the shared row-width convention.
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
                    var editRect = new Rect(headerRect.x + arrowWidth, headerRect.y, rowWidth - arrowWidth, headerRect.height);
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
                    EditorGUI.LabelField(nameRect, displayName, headerLabelStyle);
                }

                // Header row controls, right-aligned to the row's own right edge (rowRight,
                // computed above): focus (crosshair), edit (pencil), reorder up/down,
                // add-near (+), help (i), delete. Hidden while this POI is in edit mode (the
                // text field replaces name + pencil for that row). Rects already computed
                // above (right-to-left off rowRight) so the name's available width can be
                // derived from them before the name itself is drawn.
                if (!isEditing)
                {
                    // All six icon buttons share one look now: a real miniButton background
                    // (so every one of them gets the standard hover/press highlight, not just
                    // up/down/delete) with the icon drawn separately via DrawIconButton so its
                    // visual size is controlled by inset, not by the source texture's native
                    // pixel size.

                    // Focus (crosshair) button: selects + frames this marker in the Scene
                    // view without expanding the foldout (the old big "Focus in Scene" row
                    // was folded into this header icon). Disabled when the rig has no child
                    // named after this POI -- same guard the old row button used.
                    using (new EditorGUI.DisabledScope(!CanFocusPoiInScene(poi)))
                    {
                        Texture2D focusTex = AssetDatabase.LoadAssetAtPath<Texture2D>(FocusIconAssetPath);
                        if (DrawIconButton(focusRect, focusTex, "Focus in Scene"))
                            FocusPoiInScene(poi);
                    }

                    Texture2D editTex = AssetDatabase.LoadAssetAtPath<Texture2D>(EditIconAssetPath);
                    if (DrawIconButton(pencilRect, editTex, "Rename"))
                    {
                        SessionState.SetBool(editModeKey, true);
                        SessionState.SetString($"editValue_{foldoutKey}", displayName);
                        SessionState.EraseString($"editValue_{foldoutKey}_focused");
                    }

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

                    // Add a new POI right below this one, using this POI as the template
                    // (style, hierarchy level, rotation, ...) and spatial reference. This is
                    // the ONLY way to insert a POI other than the empty-list "+ Add first POI"
                    // -- replaces the old pair of "near previous/near next" separator buttons.
                    if (DrawIconButton(addRect, (Texture2D)AddIcon.image, "Add a new POI below this one, copied from it"))
                    {
                        AddNewPoiAfter(i);
                    }

                    // Explains the whole icon cluster (focus/rename/reorder/add/delete) in
                    // one place, since none of the icon-only buttons carry a visible label.
                    // A smaller inset than its siblings so the glyph itself reads clearly.
                    if (DrawIconButton(infoRect, (Texture2D)InfoIcon.image, "Row icons help", iconInset: 2f))
                        EditorPopup.ShowAt(new HelpInfoPopup("POI Row Icons", PoiHeaderIconsHelpBody), infoRect);

                    // Same width/height as its sibling icon buttons. Goes through the
                    // shared DeleteButton (Marker Table's trash glyph) like every other
                    // delete affordance in the window now.
                    if (DeleteButton.Draw(deleteRect, "Delete POI"))
                        TryDeletePoiAt(i, poi);
                }

                _poiFoldouts[foldoutKey] = expanded;

                if (!expanded)
                    continue;

                using (new EditorGUI.IndentLevelScope())
                {
                    // Focus lives on the POI title row (crosshair). Verified + help live
                    // together on the Position foldout's own header row, right-aligned, so
                    // "commit this position" and "how do I do this" sit next to each other.
                    _showPoiPosition = DrawFramedFoldout(
                        ref _showPoiPosition,
                        () => DrawPositionTabs(poi),
                        "Position",
                        FoldoutDefaultColor,
                        () => DrawPositionHeaderTrailing(poi),
                        rightAlignTrailing: true);

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
                    _showPoiSearchKeywords = DrawFramedFoldout(ref _showPoiSearchKeywords, () => DrawPoiSearchKeywordsField(poi), PoiSearchSectionTitle, FoldoutDefaultColor);
                }

                EditorGUILayout.Space(6f);
            }
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
            bool confirm = EditorDecision.Ask(
                "Delete POI?",
                $"Are you sure you want to delete \"{poiName}\"?\n\nThis removes the POI from the config and from the Scene rig.",
                "Delete") == DecisionAnswer.Confirm;

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

        // Specific Marker > POI > Marker Style: the category and hierarchy level this POI uses, plus
        // an optional custom icon. The POI name is renamed from the header row pencil, not here (a
        // second name field would fight the header draft: two writers, one field). Every reference
        // popup keeps a stale key visible as "(missing)" instead of silently rewriting it.
        private void DrawPoiMarkerStyleFields(POIData poi)
        {
            ReferenceRows(_config.category_styles, e => e.category, e => e.category, out var categoryKeys, out var categoryLabels);
            poi.category = DrawReferencePopupField("Category", poi.category, categoryKeys, categoryLabels,
                allowNone: false, PoiCategoryHelp);

            if (_config.hierarchy_levels == null || _config.hierarchy_levels.Count == 0)
            {
                EditorGUILayout.HelpBox("No hierarchy levels yet: add one in Global Scene > Hierarchy Levels.", MessageType.Info);
            }
            else
            {
                ReferenceRows(_config.hierarchy_levels, e => e.key, e => e.level_name, out var levelKeys, out var levelLabels);
                poi.hierarchy_level_key = DrawReferencePopupField("Hierarchy Level", poi.hierarchy_level_key, levelKeys, levelLabels,
                    allowNone: true, PoiHierarchyLevelHelp);
            }

            // Read-only: what this POI actually gets from its level (effects + reveal), so it can be
            // seen here without opening Hierarchy Levels. Effects are per level, never per POI.
            DrawEffectNote(EffectUsageSummary.DescribeLevelEffects(
                _config.hierarchy_levels?.Find(l => l != null && l.key == poi.hierarchy_level_key),
                _config.effect_defaults), 0f);

            poi.has_custom_symbol = DrawToggleField("Use Custom Symbol", poi.has_custom_symbol, PoiCustomSymbolHelp);
            if (poi.has_custom_symbol)
                DrawPoiCustomSymbolRow(poi);
        }

        // Custom symbol row (shown under "Use Custom Symbol"): label, the clickable thumbnail (the
        // curated wall + framework picker) and the project-wide Sprite field, in one capped row.
        private void DrawPoiCustomSymbolRow(POIData poi)
        {
            const float labelWidth = 110f;
            DrawEditorRow(out float rowWidth, out _, ConditionalAdvance);
            EditorGUILayout.LabelField("Custom symbol", GUILayout.Width(labelWidth));
            Sprite current = ResolveSpriteForKey(poi.custom_symbol_key);
            var targetPoi = poi;
            DrawSpritePreview(current,
                () => EditorPopup.ShowAt(CreateSymbolPickerPopup(key => targetPoi.custom_symbol_key = key), GUILayoutUtility.GetLastRect()));
            float fieldWidth = Mathf.Max(90f, rowWidth - labelWidth - 36f - 12f);
            Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false,
                GUILayout.Width(fieldWidth), GUILayout.ExpandWidth(false));
            if (chosen != current)
                poi.custom_symbol_key = chosen != null ? AssignSpriteToLibraryAndGetKey(chosen, poi.id + "_symbol") : null;
            EditorRowEnd();
        }

        // Specific Marker > POI > Badge Style (only while Global Scene > Badge is enabled)
        private void DrawPoiBadgeStyleFields(POIData poi)
        {
            ReferenceRows(_config.badge_categories, e => e.key, e => e.key, out var badgeKeys, out var badgeLabels);
            poi.badge_category = DrawReferencePopupField("Badge category", poi.badge_category, badgeKeys, badgeLabels,
                allowNone: true, PoiBadgeCategoryHelp);
        }

        // Specific Marker > POI > Outline (only while Global Scene > Outline is enabled). The ring
        // follows the chosen Outline Types row (status_level_key); status_pct is kept in step with it
        // and is what the ring uses only while the wall has no Outline Types rows yet (Status %).
        private void DrawPoiOutlineFields(POIData poi)
        {
            bool hasLevels = _config.outline_levels != null && _config.outline_levels.Count > 0;

            bool wantsStatus = DrawToggleField("Has status", poi.has_status, PoiHasStatusHelp);
            if (wantsStatus && !poi.has_status)
            {
                // Start on the first Outline Types row, a real pickable value (never a blank key)
                poi.has_status = true;
                var first = hasLevels ? _config.outline_levels.Find(l => l != null && !string.IsNullOrWhiteSpace(l.key)) : null;
                poi.status_level_key = first?.key;
                poi.status_pct = first?.pct ?? 0f;
            }
            else if (!wantsStatus && poi.has_status)
            {
                poi.has_status = false;
                poi.status_pct = 0f;
                poi.status_unknown = false;
                poi.status_level_key = null;
            }

            if (!poi.has_status)
                return;

            if (hasLevels)
            {
                ReferenceRows(_config.outline_levels, e => e.key,
                    e => (string.IsNullOrWhiteSpace(e.label) ? e.key : e.label) + " (" + e.pct.ToString("0") + "%)",
                    out var levelKeys, out var levelLabels);
                string picked = DrawReferencePopupField("Status level", poi.status_level_key, levelKeys, levelLabels,
                    allowNone: false, PoiStatusLevelHelp);
                if (picked != poi.status_level_key)
                {
                    poi.status_level_key = picked;
                    var level = _config.outline_levels.Find(l => l != null && l.key == picked);
                    if (level != null) poi.status_pct = level.pct;
                }
            }
            else
            {
                poi.status_pct = DrawSliderField("Status %", poi.status_pct, 0f, 100f, PoiStatusPctHelp);
            }

            bool wasUnknown = poi.status_unknown;
            poi.status_unknown = DrawToggleField("Status unknown", poi.status_unknown, PoiStatusUnknownHelp);
            if (!wasUnknown && poi.status_unknown)
                ApplyUnknownStatusDefaults(poi);
        }

        // Ticking "Status unknown": point the POI at the wall's "unknown" outline type and
        // "unknown_damage" badge (the framework's seeded default keys) when those rows exist
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

            // The unknown badge follows the POI's own badge category at runtime, so a known one (e.g.
            // "intact") left in place would contradict the grey unknown ring: switch it too.
            if (_config?.badge_categories != null)
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

        // Keys and display labels of one taxonomy table, for a reference popup (null rows skipped)
        private static void ReferenceRows<T>(List<T> rows, Func<T, string> key, Func<T, string> label,
            out List<string> keys, out List<string> labels) where T : class
        {
            keys = new List<string>();
            labels = new List<string>();
            if (rows == null) return;
            foreach (var row in rows)
            {
                if (row == null) continue;
                keys.Add(key(row));
                labels.Add(label(row));
            }
        }

        private bool GetPoiFoldout(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            // Default collapsed: a wall with a dozen-plus POIs is unreadable if every
            // row opens expanded on load. Newly-added POIs are the one exception --
            // SpawnAndFocusNewPoi explicitly opens the one just created.
            if (!_poiFoldouts.TryGetValue(key, out bool expanded))
            {
                expanded = false;
                _poiFoldouts[key] = false;
            }

            return expanded;
        }

        // Per-POI "Summary & Keywords": the POI's summary, its keyword list per Keyword Field, its Others, and
        // read-only "Found by" -- every word the search finds this POI by (SearchKeywordSources, the same list
        // the index is built from). Drawing never writes: a keyword-field list is created only when typed into.
        private void DrawPoiSearchKeywordsField(POIData poi)
        {
            if (poi == null)
                return;

            DrawPoiSummaryRows(poi);

            // --- one keyword list per Keyword Field ---
            if (_config?.search_fields != null)
            {
                foreach (var fieldDef in _config.search_fields)
                {
                    if (fieldDef == null || string.IsNullOrWhiteSpace(fieldDef.key))
                        continue;

                    var entry = poi.search_keyword_fields?.Find(e => e != null && e.field_key == fieldDef.key);
                    string displayLabel = string.IsNullOrWhiteSpace(fieldDef.label) ? fieldDef.key : fieldDef.label;
                    bool isEmpty = entry?.keywords == null || entry.keywords.Count == 0;

                    EditorGUILayout.Space(4f);
                    DrawEditorRow(out float labelRow, out _);
                    if (fieldDef.forced && isEmpty)
                    {
                        // - a Required field left empty: warning icon + "(required)", never blocking
                        GUILayout.Label(EditorGUIUtility.IconContent("console.warnicon.sml"), GUILayout.Width(18f), GUILayout.Height(18f));
                        GUILayout.Label($"{displayLabel} (required)", EditorStyles.boldLabel,
                            GUILayout.Width(Mathf.Max(40f, labelRow - 22f - 36f)), GUILayout.ExpandWidth(false));
                    }
                    else
                    {
                        GUILayout.Label(displayLabel, EditorStyles.miniBoldLabel,
                            GUILayout.Width(Mathf.Max(40f, labelRow - 36f)), GUILayout.ExpandWidth(false));
                    }
                    HelpInfoButton.Draw(displayLabel, PoiKeywordFieldHelp);
                    EditorRowEnd();

                    var current = entry?.keywords ?? new List<string>();
                    DrawEditorRow(out float keywordRow, out _);
                    var edited = DrawKeywordListField(current, GUILayout.Width(keywordRow), GUILayout.ExpandWidth(false));
                    EditorRowEnd();
                    if (!ReferenceEquals(edited, current))
                    {
                        poi.search_keyword_fields ??= new List<POISearchKeywordField>();
                        if (entry == null)
                            poi.search_keyword_fields.Add(entry = new POISearchKeywordField { field_key = fieldDef.key });
                        entry.keywords = edited;
                    }
                }
            }

            // --- Others: freeform keywords outside every Keyword Field ---
            EditorGUILayout.Space(4f);
            DrawEditorRow(out float othersLabelRow, out _);
            GUILayout.Label("Others", EditorStyles.miniBoldLabel,
                GUILayout.Width(Mathf.Max(40f, othersLabelRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Others", PoiOthersKeywordsHelp);
            EditorRowEnd();
            var others = poi.search_keywords ?? new List<string>();
            DrawEditorRow(out float othersRow, out _);
            var othersEdited = DrawKeywordListField(others, GUILayout.Width(othersRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            if (!ReferenceEquals(othersEdited, others))
                poi.search_keywords = othersEdited;

            // --- Found by: the result of everything above plus the tables and synonym groups ---
            EditorGUILayout.Space(4f);
            DrawEditorRow(out float foundRow, out _);
            GUILayout.Label("Found by (read-only)", EditorStyles.miniBoldLabel,
                GUILayout.Width(Mathf.Max(40f, foundRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Found by", PoiFoundByHelp);
            EditorRowEnd();
            DrawEditorRow(out float foundTextRow, out _);
            GUILayout.Label(FoundByText(SearchKeywordSources.Collect(_config, poi)), EditorStyles.helpBox,
                GUILayout.Width(foundTextRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
        }

        // The POI's Summary: a short description shown on its card and searched (below its own keywords)
        private void DrawPoiSummaryRows(POIData poi)
        {
            DrawEditorRow(out float labelRow, out _);
            GUILayout.Label("Summary", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, labelRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Summary", PoiSummaryHelp);
            EditorRowEnd();

            DrawEditorRow(out float textRow, out _);
            var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            string summary = EditorGUILayout.TextArea(poi.summary ?? "", style, GUILayout.Width(textRow),
                GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 2.5f), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            // - write only a real change: drawing a POI without a summary must not turn null into ""
            if (summary != (poi.summary ?? ""))
                poi.summary = summary;
        }

        // "Found by" text: one line per origin, in the order the search ranks them ("Name: The Lamp",
        // "Category: religious, church", "Synonyms: chapel"); a long text is cut, the rest counted
        internal static string FoundByText(List<SearchKeywordSources.Word> words)
        {
            if (words == null || words.Count == 0)
                return "Nothing yet: give this point a name, keywords or a category.";

            var order = new List<string>();
            var byOrigin = new Dictionary<string, List<string>>();
            foreach (var w in words)
            {
                if (!byOrigin.TryGetValue(w.Origin, out var list))
                {
                    byOrigin[w.Origin] = list = new List<string>();
                    order.Add(w.Origin);
                }
                string text = w.Text.Length > FoundByMaxTextLength ? w.Text.Substring(0, FoundByMaxTextLength) + "..." : w.Text;
                if (!list.Contains(text)) list.Add(text);
            }

            var lines = new List<string>();
            foreach (var origin in order)
                lines.Add(origin + ": " + string.Join(", ", byOrigin[origin]));
            return string.Join("\n", lines);
        }

        private const int FoundByMaxTextLength = 60;

        private const string PoiSearchSectionTitle = "Summary & Keywords";

        // DrawIconButton lives in Shared/POIEditorToolWindow.IconButton.cs now (it's the
        // shared look for every icon-only button in this window, not just this row's).

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

        // A first POI (no sourcePoi) has no colour taxonomy to copy from, so its category comes
        // from the wall's own first category_styles row -- a real, pickable value in the dropdown,
        // never a literal "default" that matches nothing and silently falls through to the hash.
        private string FirstAvailableCategory()
        {
            if (_config?.category_styles != null)
                foreach (var entry in _config.category_styles)
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.category))
                        return entry.category;
            return string.Empty;
        }

        // Same rule for the level: the wall's first Hierarchy Levels row, so a first POI gets a real
        // size and label (and passes the save-time check) instead of the 12 cm "no level" fallback.
        private string FirstAvailableHierarchyLevelKey() => HighestPriorityLevelKey(_config?.hierarchy_levels);

        // A new level goes last: one more than the largest priority in use (and than the row count).
        internal static int NextLowestPriority(IList<HierarchyLevelEntry> levels)
        {
            int max = levels?.Count ?? 0;
            if (levels != null)
                foreach (var l in levels)
                    if (l != null && l.priority > max) max = l.priority;
            return max + 1;
        }

        // The level with the highest priority (smallest number), whatever numbers the developer chose
        // (10..100 works the same as 1..5). Uses the runtime's own rule: priority >= 1 is explicit,
        // otherwise the 1-based table position. A tie goes to the earlier row. Null when there is none.
        internal static string HighestPriorityLevelKey(IList<HierarchyLevelEntry> levels)
        {
            string bestKey = null;
            int bestPriority = int.MaxValue;
            if (levels == null) return null;
            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                if (level == null || string.IsNullOrWhiteSpace(level.key)) continue;
                int effective = level.priority >= 1 ? level.priority : i + 1;
                if (bestKey == null || effective < bestPriority)
                {
                    bestKey = level.key;
                    bestPriority = effective;
                }
            }
            return bestKey;
        }

        private POIData CreateDefaultPoi(POIData sourcePoi = null)
        {
            var poi = new POIData
            {
                id = System.Guid.NewGuid().ToString("N"),
                name = "New POI",
                category = FirstAvailableCategory(),
                position_verified = false,
                status_pct = 0f,
                has_status = false,
                status_unknown = false,
                status_level_key = null,
                hierarchy_level_key = FirstAvailableHierarchyLevelKey(),
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
                poi.status_level_key = sourcePoi.status_level_key;
                poi.search_keywords = sourcePoi.search_keywords != null ? new List<string>(sourcePoi.search_keywords) : new List<string>();
                // Deep copy: each field's own keyword list, not the source POI's list instances --
                // otherwise editing the new POI's keywords silently rewrites the source POI's too.
                poi.search_keyword_fields = new List<POISearchKeywordField>();
                if (sourcePoi.search_keyword_fields != null)
                    foreach (var field in sourcePoi.search_keyword_fields)
                        poi.search_keyword_fields.Add(new POISearchKeywordField
                        {
                            field_key = field.field_key,
                            keywords = field.keywords != null ? new List<string>(field.keywords) : new List<string>()
                        });
                poi.editor_rotation_deg = sourcePoi.editor_rotation_deg;
                poi.editor_rotation_x_deg = sourcePoi.editor_rotation_x_deg;
                poi.editor_rotation_z_deg = sourcePoi.editor_rotation_z_deg;
            }

            return poi;
        }

        private void InsertPoiAt(int index, POIData poi)
        {
            DrawConfigMutationScope(() => _config.pois.Insert(index, poi), true);
        }

        // Add the very first POI to an empty config (the "+ Add first POI" button --
        // the only add path with no existing POI to use as a source/template).
        private void AddFirstPoi()
        {
            if (_config == null) return;
            if (_config.pois == null) _config.pois = new List<POIData>();

            var newPoi = CreateDefaultPoi();
            InsertPoiAt(0, newPoi);
            SpawnAndFocusNewPoi(newPoi);
            _poiFoldouts[newPoi.id] = true;
            Repaint();
        }

        // Insert a new POI immediately after the given index, using that POI as the
        // template and spatial reference. This is the ONLY way to add a POI to a
        // non-empty list -- driven by the per-row "+" button on the header row.
        private void AddNewPoiAfter(int index)
        {
            if (_config == null) return;
            if (_config.pois == null) _config.pois = new List<POIData>();
            if (index < 0 || index >= _config.pois.Count) return;

            var sourcePoi = _config.pois[index];
            var newPoi = CreateDefaultPoi(sourcePoi);
            InsertPoiAt(index + 1, newPoi);

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
                // (A HelpBox here was drawn mid-layout from an event handler and vanished
                // next frame, so nobody saw it -- a queued notice dialog actually reaches the developer.)
                EditorNotice.Queue("No POI Editor rig found", "Add a PlacementCorrectionAnchor to the scene to see markers in the Scene view. The POI was still added to the config.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[POIEditor] Prefab not found at {_prefabPath}");
                return;
            }

            // Resolve initial position offset from the source POI's own rig child, when
            // there is one (AddFirstPoi has no source -- the very first POI spawns at
            // the rig origin instead).
            Vector3 initialPosition = Vector3.zero;
            if (referencePoi != null)
            {
                var referenceChild = GetExistingRig()?.Find(referencePoi.id);
                if (referenceChild != null)
                    initialPosition = referenceChild.localPosition + new Vector3(0.3f, 0f, 0.3f);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, GetExistingRig());
            instance.name = poi.id;
            instance.transform.localPosition = initialPosition;
            instance.transform.localRotation = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);
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
            ConfigureRigChild(poi, instance.transform, PrepareRigVisuals());

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
