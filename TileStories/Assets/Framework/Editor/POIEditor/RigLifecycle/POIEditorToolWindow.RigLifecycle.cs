using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private void RefreshRigVisuals()
        {
            if (_config == null || _config.pois == null) return;

            Transform rig = GetExistingRig();
            if (rig == null) return;

            foreach (var poi in _config.pois)
            {
                var child = rig.Find(poi.id);
                if (child == null) continue;

                // Config is the source of truth for the edit-scene yaw; re-apply on
                // every visual refresh so undo/redo/field edits don't drift the
                // Scene-view preview away from the stored angle.
                child.localRotation = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);

                // Reuse shared configuration logic.
                ConfigureRigChild(poi, child);
            }
        }

        // Configures a single rig child with all visual settings (MarkerView, POIAnchor, etc.).
        // Shared between PopulateRig and the per-POI add-flow so both use identical setup.
        internal void ConfigureRigChild(POIData poi, Transform child)
        {
            if (_config == null || child == null) return;

            bool hasCategoryDefinitions = _config.category_styles != null && _config.category_styles.Count > 0;
            if (hasCategoryDefinitions) CategoryPalette.Configure(_config.category_styles);
            else CategoryPalette.ClearOverrides();

            BadgeCategoryPalette.Configure(_config.badge_categories);

            bool hasOutlineLevels = _config.outline_levels != null && _config.outline_levels.Count > 0;
            if (hasOutlineLevels) StatusRamp.Configure(_config.outline_levels);

            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);

            bool hasShapeFromConfig = MarkerVisualsParser.TryParseShape(_config.marker_shape, out var shape);
            MarkerVisualsParser.TryParseShape(_config.badge_shape, out var badgeShape);
            if (badgeShape == default) badgeShape = MarkerShape.Circle;

            MarkerOutlineMode outlineMode;
            bool useBadge;
            if (!string.IsNullOrWhiteSpace(_config.marker_outline_mode))
            {
                if (!MarkerVisualsParser.TryParseOutlineMode(_config.marker_outline_mode, out outlineMode))
                    outlineMode = MarkerOutlineMode.None;
                useBadge = _config.marker_use_badge;
            }
            else if (MarkerVisualsParser.TryParseStyle(_config.marker_style, out var legacyStyle))
            {
                MarkerVisualsParser.DeriveOutlineAndBadgeFromLegacyStyle(
                    legacyStyle == MarkerStyle.Badge ? "badge" :
                    legacyStyle == MarkerStyle.OutlineSameHue ? "outline_same_hue" : "outline_gold",
                    out outlineMode,
                    out useBadge);
            }
            else
            {
                outlineMode = MarkerOutlineMode.None;
                useBadge = false;
            }

            var runtimeLibrary = _wallIconLibrary;

            // Config is the source of truth for the edit-scene yaw; re-apply on
            // every visual refresh so undo/redo/field edits don't drift the
            // Scene-view preview away from the stored angle.
            child.localRotation = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);

            var anchor = child.GetComponentInChildren<POIAnchor>() ?? child.gameObject.AddComponent<POIAnchor>();
            anchor.Initialise(poi);

            var markerView = child.GetComponentInChildren<MarkerView>();
            var effects = MarkerEffectFlags.None;
            markerView?.Initialise(anchor, outlineMode, useBadge, shape, effects,
                hasCategoryDefinitions,
                hasShapeFromConfig,
                hasOutlineLevels,
                runtimeLibrary,
                badgeShape);
        }

        internal bool IsRigInSyncWithConfig(out int outOfSyncCount)
        {
            outOfSyncCount = 0;

            Transform rig = GetExistingRig();
            if (rig == null || _config == null || _config.pois == null)
                return true;

            foreach (Transform child in rig)
            {
                var poi = _config.pois.Find(p => p.id == child.name);

                if (poi == null)
                {
                    outOfSyncCount++;
                    continue;
                }

                if (poi.position == null)
                {
                    outOfSyncCount++;
                    continue;
                }

                Vector3 savedPos = new Vector3(
                    poi.position.x,
                    poi.position.y,
                    poi.position.z);

                float distance = Vector3.Distance(child.localPosition, savedPos);
                if (distance > SyncPositionTolerance)
                    outOfSyncCount++;
            }

            return outOfSyncCount == 0;
        }

        private void ClearRig()
        {
            Transform rig = GetExistingRig();

            if (rig == null || rig.childCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "Nothing to clear",
                    "POIEditorRig has no children.",
                    "OK");
                return;
            }

            bool inSync = IsRigInSyncWithConfig(out int outOfSyncCount);
            bool proceed = inSync || EditorUtility.DisplayDialog(
                "Uncaptured or unsynced positions",
                $"{outOfSyncCount} marker(s) in the rig don't match config.json (never captured, or moved since the last capture). Clearing now will lose that placement work. Clear anyway?",
                "Clear anyway",
                "Cancel");

            if (!proceed)
                return;

            for (int i = rig.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(rig.GetChild(i).gameObject);

            Debug.Log("[POIEditor] Cleared POIEditorRig children.");
        }

        private Transform GetExistingRig()
        {
            // Search for POIEditorRig in the scene
            var rigObject = GameObject.Find("POIEditorRig");
            return rigObject != null ? rigObject.transform : null;
        }

        private Transform GetOrCreateRig()
        {
            Transform existing = GetExistingRig();
            if (existing != null)
                return existing;

            var go = new GameObject("POIEditorRig");
            Undo.RegisterCreatedObjectUndo(go, "Create POIEditorRig");
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        private int GetRigChildCount()
        {
            Transform rig = GetExistingRig();
            return rig != null ? rig.childCount : 0;
        }

internal enum ReloadGuardChoice
        {
            ProceedWithSaveOrCapture = 0,
            DiscardAndReload = 1,
            Cancel = 2,
        }

        // Pure decision helper for Guard 1 (uncaptured rig positions).
        // Returns null when no dialog is needed; otherwise the action the
        // caller must take for the given dialog result.
        internal static ReloadGuardChoice? ResolveUncapturedRigChoice(
            bool hasConfig, bool hasRigChildren, bool rigInSync, int dialogResult)
        {
            if (!hasConfig || !hasRigChildren || rigInSync)
                return null; // No dialog needed.
            if (dialogResult == 0) return ReloadGuardChoice.ProceedWithSaveOrCapture;
            if (dialogResult == 1) return ReloadGuardChoice.DiscardAndReload;
            return ReloadGuardChoice.Cancel;
        }

        // Pure decision helper for Guard 2 (unsaved in-memory config edits).
        internal static ReloadGuardChoice? ResolveUnsavedConfigChoice(
            bool hasConfig, bool hasUnsavedChanges, int dialogResult)
        {
            if (!hasConfig || !hasUnsavedChanges)
                return null; // No dialog needed.
            if (dialogResult == 0) return ReloadGuardChoice.ProceedWithSaveOrCapture;
            if (dialogResult == 1) return ReloadGuardChoice.DiscardAndReload;
            return ReloadGuardChoice.Cancel;
        }

        private void LoadAndPopulateRig()
        {
            // Guard 1: rig markers were moved in the scene but never captured
            // to config. PopulateRig would destroy those Transforms, so offer
            // to capture them into the in-memory config first (CapturePositions
            // flips _hasUnsavedChanges, so Guard 2 below then offers to save).
            Transform existingRig = GetExistingRig();
            int outOfSyncCount = 0;
            bool rigNeedsDialog = _config != null
                && existingRig != null && existingRig.childCount > 0
                && !IsRigInSyncWithConfig(out outOfSyncCount)
                && outOfSyncCount > 0;
            if (rigNeedsDialog)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Uncaptured rig positions",
                    $"{outOfSyncCount} marker(s) in the rig were moved but never captured. Repopulating will destroy those moved Transforms.",
                    "Capture & Reload",
                    "Discard & Reload",
                    "Cancel");

                var rigChoice = ResolveUncapturedRigChoice(true, true, false, choice);
                if (rigChoice == ReloadGuardChoice.ProceedWithSaveOrCapture)
                    CapturePositions();
                else if (rigChoice != ReloadGuardChoice.DiscardAndReload)
                    return; // Cancel (or closed via X) -> stay, lose nothing.
            }

            // Guard 2: unsaved in-memory config edits would be overwritten by
            // the reload inside PopulateRig. Ask first, same blocking pattern
            // as ClearRig's unsynced-positions check.
            if (_config != null && _hasUnsavedChanges)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Unsaved config changes",
                    "You have unsaved config edits. Reloading from config.json will discard them.",
                    "Save & Reload",
                    "Discard & Reload",
                    "Cancel");

                var configChoice = ResolveUnsavedConfigChoice(true, true, choice);
                if (configChoice == ReloadGuardChoice.ProceedWithSaveOrCapture)
                    SaveAllToJson(); // CapturePositions + SaveConfig
                else if (configChoice != ReloadGuardChoice.DiscardAndReload)
                    return; // Cancel (or closed via X) -> stay, lose nothing.
            }

            PopulateRig();
        }

        private void PopulateRig()
        {
            if (!File.Exists(_configPath))
            {
                Debug.LogError($"[POIEditor] Source config not found at {_configPath}");
                return;
            }

            LoadConfig();

            if (_config == null || _config.pois == null)
            {
                Debug.LogWarning("[POIEditor] No config loaded.");
                return;
            }

            var rig = GetOrCreateRig();
            if (rig == null)
            {
                Debug.LogError("[POIEditor] Cannot find or create POIEditorRig. Assign a correction anchor.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[POIEditor] Prefab not found at {_prefabPath}");
                return;
            }

            if (rig.childCount > 0)
            {
                bool clear = EditorUtility.DisplayDialog(
                    "POI Editor Rig",
                    $"POIEditorRig already has {rig.childCount} object(s). Clear existing rig first?",
                    "Clear and repopulate",
                    "Cancel");

                if (!clear)
                {
                    Debug.Log("[POIEditor] Populate cancelled by user.");
                    return;
                }

                var children = new List<GameObject>();
                for (int i = 0; i < rig.childCount; i++)
                    children.Add(rig.GetChild(i).gameObject);

                foreach (var child in children)
                    Undo.DestroyObjectImmediate(child);
            }

                        bool hasCategoryDefinitions = _config.category_styles != null && _config.category_styles.Count > 0;
            if (hasCategoryDefinitions) CategoryPalette.Configure(_config.category_styles);
            else CategoryPalette.ClearOverrides();

            BadgeCategoryPalette.Configure(_config.badge_categories);

            bool hasOutlineLevels = _config.outline_levels != null && _config.outline_levels.Count > 0;
            if (hasOutlineLevels) StatusRamp.Configure(_config.outline_levels);

            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);

            bool hasShapeFromConfig = MarkerVisualsParser.TryParseShape(_config.marker_shape, out var shape);
            MarkerVisualsParser.TryParseShape(_config.badge_shape, out var badgeShape);
            if (badgeShape == default) badgeShape = MarkerShape.Circle;

            MarkerOutlineMode outlineMode;
            bool useBadge;
            if (!string.IsNullOrWhiteSpace(_config.marker_outline_mode))
            {
                if (!MarkerVisualsParser.TryParseOutlineMode(_config.marker_outline_mode, out outlineMode))
                    outlineMode = MarkerOutlineMode.None;
                useBadge = _config.marker_use_badge;
            }
            else if (MarkerVisualsParser.TryParseStyle(_config.marker_style, out var legacyStyle))
            {
                MarkerVisualsParser.DeriveOutlineAndBadgeFromLegacyStyle(
                    legacyStyle == MarkerStyle.Badge ? "badge" :
                    legacyStyle == MarkerStyle.OutlineSameHue ? "outline_same_hue" : "outline_gold",
                    out outlineMode,
                    out useBadge);
            }
            else
            {
                outlineMode = MarkerOutlineMode.None;
                useBadge = false;
            }

            foreach (var poi in _config.pois)
            {
                if (!POIPositionResolver.TryResolvePosition(poi, out Vector3 localPos))
                {
                    Debug.LogWarning($"[POIEditor] Skipping POI '{poi.id}' - position could not be resolved.");
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, rig);
                instance.name = poi.id;
                instance.transform.localPosition = localPos;
                instance.transform.localRotation = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);

                Undo.RegisterCreatedObjectUndo(instance, $"Populate marker for {poi.id}");

                var anchor = instance.GetComponentInChildren<POIAnchor>() ?? instance.AddComponent<POIAnchor>();
                anchor.Initialise(poi);

                var markerView = instance.GetComponentInChildren<MarkerView>();
                if (markerView != null)
                {
                    var effects = MarkerEffectFlags.None;
                    markerView.Initialise(anchor, outlineMode, useBadge, shape, effects,
                        hasCategoryDefinitions,
                        hasShapeFromConfig,
                        hasOutlineLevels,
                        _wallIconLibrary,
                        badgeShape);
                }
            }

            SelectRigObjects();
            Debug.Log($"[POIEditor] Populated {rig.childCount} markers under POIEditorRig.");
        }

        private void CapturePositions(bool silentWhenRigMissing = false)
        {
            if (_config == null || _config.pois == null)
            {
                Debug.LogWarning("[POIEditor] No config loaded.");
                return;
            }

            Transform rig = GetExistingRig();
            if (rig == null || rig.childCount == 0)
            {
                if (!silentWhenRigMissing)
                    Debug.LogWarning("[POIEditor] No POIEditorRig with children found. Populate first.");
                return;
            }

            int captured = 0;
            int skipped = 0;
            var sceneObjects = new Dictionary<string, Transform>();

            for (int i = 0; i < rig.childCount; i++)
            {
                var child = rig.GetChild(i);
                sceneObjects[child.name] = child;
            }

            foreach (var poi in _config.pois)
            {
                if (!sceneObjects.TryGetValue(poi.id, out var markerTransform))
                {
                    skipped++;
                    continue;
                }

                // Rig is at origin, so localPosition == world position
                Vector3 localPos = markerTransform.localPosition;

                poi.position = new PositionData
                {
                    x = localPos.x,
                    y = localPos.y,
                    z = localPos.z
                };

                // Capture the marker's full edit-scene rotation (pitch/yaw/roll) so the
                // developer's scene-rotate tool changes persist into config on Save. The
                // Y axis maps to the legacy editor_rotation_deg (yaw); X/Z are the new
                // pitch/roll fields added alongside it.
                Vector3 euler = markerTransform.localRotation.eulerAngles;
                poi.editor_rotation_x_deg = PoiRotationResolver.NormalizeAngleDeg(euler.x);
                poi.editor_rotation_deg = PoiRotationResolver.NormalizeAngleDeg(euler.y);
                poi.editor_rotation_z_deg = PoiRotationResolver.NormalizeAngleDeg(euler.z);

                captured++;
            }

            _hasUnsavedChanges = true;
            Debug.Log($"[POIEditor] {captured} positions synced (skipped {skipped} missing scene objects).");
            Repaint();
        }

        private void SelectRigObjects()
        {
            Transform rig = GetExistingRig();
            if (rig == null || rig.childCount == 0)
                return;

            var gos = new List<GameObject>();
            for (int i = 0; i < rig.childCount; i++)
                gos.Add(rig.GetChild(i).gameObject);

            Selection.objects = gos.ToArray();
        }

    }
}