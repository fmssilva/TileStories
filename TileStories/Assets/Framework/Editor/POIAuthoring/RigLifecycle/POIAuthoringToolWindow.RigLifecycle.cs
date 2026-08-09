using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        private void RefreshRigVisuals()
        {
            if (_config == null || _config.pois == null) return;

            Transform rig = GetExistingRig();
                        if (rig == null) return;

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

            foreach (var poi in _config.pois)
            {
                var child = rig.Find(poi.id);
                if (child == null) continue;

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

                if (!poi.has_captured_position)
                {
                    outOfSyncCount++;
                    continue;
                }

                Vector3 savedPos = new Vector3(
                    poi.captured_position.x,
                    poi.captured_position.y,
                    poi.captured_position.z);

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
                    "POIAuthoringRig has no children.",
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

            Debug.Log("[POIAuthoring] Cleared POIAuthoringRig children.");
        }

        private Transform GetExistingRig()
        {
            TryResolveSceneReferences();
            if (_correctionAnchor == null)
                return null;

            for (int i = 0; i < _correctionAnchor.childCount; i++)
            {
                var child = _correctionAnchor.GetChild(i);
                if (child.name == "POIAuthoringRig")
                    return child;
            }

            return null;
        }

        private Transform GetOrCreateRig()
        {
            Transform existing = GetExistingRig();
            if (existing != null)
                return existing;

            if (_correctionAnchor == null)
                return null;

            var go = new GameObject("POIAuthoringRig");
            Undo.RegisterCreatedObjectUndo(go, "Create POIAuthoringRig");
            go.transform.SetParent(_correctionAnchor);
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

        private void TryResolveSceneReferences()
        {
            if (_correctionAnchor == null)
            {
                var correctionAnchorObject = GameObject.Find("PlacementCorrectionAnchor");
                if (correctionAnchorObject != null)
                    _correctionAnchor = correctionAnchorObject.transform;
            }

            if (_wallMesh == null)
                _wallMesh = GameObject.Find("146267-LivingRoom2-tex");
        }

        private void PopulateRig()
        {
            if (!File.Exists(_configPath))
            {
                Debug.LogError($"[POIAuthoring] Source config not found at {_configPath}");
                return;
            }

            LoadConfig();

            if (_config == null || _config.pois == null)
            {
                Debug.LogWarning("[POIAuthoring] No config loaded.");
                return;
            }

            var rig = GetOrCreateRig();
            if (rig == null)
            {
                Debug.LogError("[POIAuthoring] Cannot find or create POIAuthoringRig. Assign a correction anchor.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[POIAuthoring] Prefab not found at {_prefabPath}");
                return;
            }

            if (rig.childCount > 0)
            {
                bool clear = EditorUtility.DisplayDialog(
                    "POI Authoring Rig",
                    $"POIAuthoringRig already has {rig.childCount} object(s). Clear existing rig first?",
                    "Clear and repopulate",
                    "Cancel");

                if (!clear)
                {
                    Debug.Log("[POIAuthoring] Populate cancelled by user.");
                    return;
                }

                var children = new List<GameObject>();
                for (int i = 0; i < rig.childCount; i++)
                    children.Add(rig.GetChild(i).gameObject);

                foreach (var child in children)
                    Undo.DestroyObjectImmediate(child);
            }

                        var anchors = _config.calibration_anchors?.ToArray() ?? Array.Empty<CalibrationAnchor>();

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
                if (!POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 localPos))
                {
                    Debug.LogWarning($"[POIAuthoring] Skipping POI '{poi.id}' - position could not be resolved.");
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, rig);
                instance.name = poi.id;
                instance.transform.localPosition = localPos;
                instance.transform.localRotation = Quaternion.identity;

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
            Debug.Log($"[POIAuthoring] Populated {rig.childCount} markers under POIAuthoringRig.");
        }

        private void CapturePositions(bool silentWhenRigMissing = false)
        {
            if (_config == null || _config.pois == null)
            {
                Debug.LogWarning("[POIAuthoring] No config loaded.");
                return;
            }

            Transform rig = GetExistingRig();
            if (rig == null || rig.childCount == 0)
            {
                if (!silentWhenRigMissing)
                    Debug.LogWarning("[POIAuthoring] No POIAuthoringRig with children found. Populate first.");
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

                Vector3 localPos;
                if (_correctionAnchor != null)
                    localPos = _correctionAnchor.InverseTransformPoint(markerTransform.position);
                else
                    localPos = markerTransform.localPosition;

                poi.captured_position = new CapturedPosition
                {
                    x = localPos.x,
                    y = localPos.y,
                    z = localPos.z
                };
                poi.has_captured_position = true;
                poi.captured_position_source = "workflow_a_editor";
                poi.captured_position_timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                captured++;
            }

            _hasUnsavedChanges = true;
            Debug.Log($"[POIAuthoring] Captured {captured} positions (skipped {skipped} missing scene objects).");
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

        private void OnSceneGUI(SceneView sv)
        {
            if (_config == null || _config.pois == null)
                return;

            var anchors = _config.calibration_anchors?.ToArray() ?? Array.Empty<CalibrationAnchor>();

            Handles.color = Color.cyan;
            foreach (var poi in _config.pois)
            {
                if (!POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 localPos))
                    continue;

                Vector3 worldPos;
                if (_correctionAnchor != null)
                    worldPos = _correctionAnchor.TransformPoint(localPos);
                else
                    worldPos = localPos;

                float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.1f;
                Handles.SphereHandleCap(0, worldPos, Quaternion.identity, handleSize, EventType.Repaint);
            }
        }
    }
}