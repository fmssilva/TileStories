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

            var settings = PrepareRigVisuals();
            foreach (var poi in _config.pois)
            {
                var child = rig.Find(poi.id);
                if (child == null) continue;

                // Config is the source of truth for the edit-scene yaw; re-apply on
                // every visual refresh so undo/redo/field edits don't drift the
                // Scene-view preview away from the stored angle.
                child.localRotation = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);

                // Reuse shared configuration logic.
                ConfigureRigChild(poi, child, settings);
            }
        }

        // Point the palettes at the window's config and resolve the wall-level marker look ONCE per
        // refresh -- the same MarkerVisualSettings.Resolve the running wall uses (_2.2.1 section 5).
        // Passes _wallFontLibrary too (2026-09-24 fix): without it, a wall's own font override
        // rendered correctly in Play Mode (WallSession resolves it) but never in the Scene-view
        // rig, since Resolve silently treats a missing font library as "use the framework default".
        private MarkerVisualSettings PrepareRigVisuals()
        {
            MarkerVisualSettings.ApplyPalettes(_config);
            return MarkerVisualSettings.Resolve(_config, _wallIconLibrary, _wallFontLibrary);
        }

        // Configures a single rig child with all visual settings (MarkerView, POIAnchor, etc.).
        // Shared by PopulateRig, RefreshRigVisuals and the per-POI add-flow so all use identical setup.
        internal void ConfigureRigChild(POIData poi, Transform child, MarkerVisualSettings settings)
        {
            if (_config == null || child == null) return;

            var anchor = child.GetComponentInChildren<POIAnchor>() ?? child.gameObject.AddComponent<POIAnchor>();
            anchor.Initialise(poi);

            child.GetComponentInChildren<MarkerView>()?.Initialise(anchor, settings);
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
                EditorNotice.Queue("Nothing to clear", "POIEditorRig has no children.");
                return;
            }

            bool inSync = IsRigInSyncWithConfig(out int outOfSyncCount);
            bool proceed = inSync || EditorDecision.Ask(
                "Uncaptured or unsynced positions",
                $"{outOfSyncCount} marker(s) in the rig don't match config.json (never captured, or moved since the last capture). Clearing now will lose that placement work. Clear anyway?",
                "Clear anyway") == DecisionAnswer.Confirm;

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

        // "Load & Populate Rig": reload config.json and rebuild the rig from it. The reload replaces
        // the in-memory config AND every rig marker, so when that would lose work (markers moved
        // since the last save, unsaved config edits) ONE question lists what, and every button does
        // exactly what it says: Save & Reload keeps the work (saved first), Discard & Reload drops
        // it, Cancel changes nothing. Nothing to lose = no question.
        private void LoadAndPopulateRig()
        {
            int movedCount = 0;
            if (_config != null)
                IsRigInSyncWithConfig(out movedCount);
            bool hasUnsavedEdits = _config != null && _hasUnsavedChanges;

            if (movedCount > 0 || hasUnsavedEdits)
            {
                var answer = EditorDecision.Ask(BuildReloadQuestion(movedCount, hasUnsavedEdits));
                if (answer == DecisionAnswer.Confirm)
                    SaveAllToJson(); // CapturePositions + SaveConfig: the reload then reads the work back
                else if (answer != DecisionAnswer.Alternative)
                    return; // Cancel (or Esc / X) -> stay, lose nothing.
            }

            PopulateRig();
        }

        // Pure: the Load & Populate question, listing exactly what a reload would drop
        internal static DecisionRequest BuildReloadQuestion(int movedCount, bool hasUnsavedEdits)
        {
            var lines = new List<string> { "Reloading replaces the config and every rig marker with what config.json holds. It would drop:" };
            if (movedCount > 0) lines.Add($"- {movedCount} rig marker(s) moved since the last save");
            if (hasUnsavedEdits) lines.Add("- unsaved config edits");
            lines.Add("Save & Reload saves them first, so the reload keeps them. Discard & Reload drops them.");
            return new DecisionRequest
            {
                Title = "Reload config.json?",
                Message = string.Join("\n", lines),
                ConfirmLabel = "Save & Reload",
                AlternativeLabel = "Discard & Reload",
            };
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

            // Rebuilding the rig IS what Load & Populate means, and LoadAndPopulateRig already asked
            // about anything that would be lost, so the old markers go without a further question
            // (undoable with Edit > Undo, like Clear Rig).
            for (int i = rig.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(rig.GetChild(i).gameObject);

            var settings = PrepareRigVisuals();

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

                ConfigureRigChild(poi, instance.transform, settings);
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
                // pitch/roll fields added alongside it. Skipped while Edit-Mode orientation
                // preview is active (_2.1_Marker_Orientation.md section 14) -- the rig's
                // current localRotation is the PREVIEW, not an authored edit, and capturing
                // it would silently corrupt the stored angles.
                // Also skipped for a Verified POI: its facing is locked, so the rig can only
                // differ from the stored angles by an edit the lock is about to revert.
                if (_config.orientation_settings?.edit_mode_preview_enabled != true && !poi.position_verified)
                {
                    Vector3 euler = markerTransform.localRotation.eulerAngles;
                    poi.editor_rotation_x_deg = PoiRotationResolver.NormalizeAngleDeg(euler.x);
                    poi.editor_rotation_deg = PoiRotationResolver.NormalizeAngleDeg(euler.y);
                    poi.editor_rotation_z_deg = PoiRotationResolver.NormalizeAngleDeg(euler.z);
                }

                captured++;
            }

            _hasUnsavedChanges = true;
            Debug.Log($"[POIEditor] {captured} positions synced (skipped {skipped} missing scene objects).");
            Repaint();
        }

        // Block 7 (_2.1_Marker_Orientation.md section 14): Edit-Mode orientation preview.
        // True only while a preview tick has actually run this session; used to know
        // whether the rig's rotations need restoring when the toggle switches off.
        private bool _orientationPreviewWasActive;

        private void HandleOrientationPreviewSceneGui(SceneView sceneView)
        {
            bool active = _config?.orientation_settings?.edit_mode_preview_enabled == true;
            if (!active)
            {
                if (_orientationPreviewWasActive)
                    RestoreRigRotationsFromConfig();
                _orientationPreviewWasActive = false;
                return;
            }

            if (sceneView == null || sceneView.camera == null) return;
            ApplyOrientationPreview(sceneView.camera);
            _orientationPreviewWasActive = true;
            sceneView.Repaint();
        }

        // The single orientation evaluation per repaint, decoupled from SceneView so it
        // is directly testable (mirrors SyncPoiRotationFromScene's own "pure logic,
        // Tier-0 testable" split). Calls the SAME MarkerOrientationResolver the Play-Mode
        // path (MarkerBillboard.LateUpdate) uses -- never a parallel implementation.
        // MonoBehaviour.LateUpdate does not tick in Edit Mode, which is why this exists
        // at all rather than just letting the rig's MarkerBillboard components run.
        internal void ApplyOrientationPreview(Camera previewCamera)
        {
            Transform rig = GetExistingRig();
            if (rig == null || _config?.pois == null || previewCamera == null) return;

            var settings = _config.orientation_settings ?? new OrientationSettings();
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(previewCamera);
            Vector3 upReference = MarkerOrientationResolver.ResolveUpReference(settings, rig);
            Quaternion parentRotation = rig.rotation;

            foreach (var poi in _config.pois)
            {
                var child = rig.Find(poi.id);
                if (child == null) continue;

                string facingModeOverride = MarkerHierarchyResolver.ResolveFacingModeOverride(poi.hierarchy_level_key);
                Quaternion authored = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);

                var result = MarkerOrientationResolver.ResolveRootRotation(
                    settings, facingModeOverride, child.position, previewCamera.transform.position, previewCamera.transform.forward,
                    screenUp, upReference, parentRotation, authored);

                if (result.Resolved)
                    child.rotation = result.Rotation;

                TickPreviewChildren(child, settings, screenUp, upReference);
            }
        }

        // Label and Badge counter-rotate independently of the root (MarkerChildOrientation).
        // MarkerBillboard.Configure is NOT used here: it snapshots the root's localRotation as
        // "authored", and under preview that rotation is the preview itself. Configure only the
        // children, then Tick them against the root's final rotation, like MarkerBillboard does.
        private static void TickPreviewChildren(Transform marker, OrientationSettings settings, Vector3 screenUp, Vector3 upReference)
        {
            foreach (var childOrientation in marker.GetComponentsInChildren<MarkerChildOrientation>(true))
            {
                string mode = childOrientation.gameObject.name == "Label" ? settings.label_vertical_alignment_mode
                    : childOrientation.gameObject.name == "Badge" ? settings.badge_vertical_alignment_mode
                    : "inherit";
                childOrientation.Configure(mode);
                childOrientation.Tick(marker.rotation, screenUp, upReference);
            }
        }

        // Restores every rig child's localRotation from the stored config angles.
        // Called when preview switches off, so a preview session never leaves the rig
        // showing a rotation that was never actually authored.
        internal void RestoreRigRotationsFromConfig()
        {
            Transform rig = GetExistingRig();
            if (rig == null || _config?.pois == null) return;

            foreach (var poi in _config.pois)
            {
                var child = rig.Find(poi.id);
                if (child == null) continue;
                child.localRotation = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);

                // "inherit" resolves to identity, so this puts Label/Badge back exactly as authored.
                TickPreviewChildren(child, new OrientationSettings(), Vector3.up, Vector3.up);
            }
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