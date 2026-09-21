// POIEditorToolWindow.MarkerSceneEdit.cs
//
// Partial: what happens when the developer moves or rotates the selected rig marker
// (Scene-view gizmo or Inspector): keep the Facing sliders in sync, enforce the Verified
// lock on position AND facing, and reveal the marker's section in this window.
// Editor-only.

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        internal const string VerifiedRotationLockedMessage = "This facing is already verified. If you want to change it, click the Verified button to enable editing.";

        internal const string VerifiedNoticeTitle = "POI is verified";
        internal const string FacingNoticeTitle = "Facing has no visible effect";

        private readonly MarkerEditDetector _markerEditDetector = new MarkerEditDetector();

        // Set by a reveal; consumed by the POI list once it has measured that POI's header.
        private string _pendingScrollPoiId;
        // The marker we already revealed, so dragging the same one does not keep yanking the scroll.
        private string _lastRevealedPoiId;

        // Verified locks facing exactly like position: sliders are read-only until unlocked.
        internal static bool AreFacingSlidersEditable(POIData poi)
        {
            return poi != null && !poi.position_verified;
        }

        // Pure lock rule for facing. The locked orientation is the POI's stored angles: sync
        // from the scene is skipped while verified, so those are always the last verified ones.
        internal static bool ShouldBlockVerifiedRotationChange(POIData poi, Quaternion currentLocalRotation, out Quaternion correctedLocalRotation, out string message)
        {
            correctedLocalRotation = currentLocalRotation;
            message = string.Empty;

            if (poi == null || !poi.position_verified)
                return false;

            Quaternion locked = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);
            if (PoiRotationResolver.IsSameOrientation(currentLocalRotation, locked))
                return false;

            correctedLocalRotation = locked;
            message = VerifiedRotationLockedMessage;
            return true;
        }

        // With Edit-Mode preview on, some facing axes are overridden by the wall's Facing mode
        // (see FacingEditAdvice). Tell the developer (hideable notice) so a slider that
        // "does nothing" is not mistaken for a bug.
        private void WarnIfFacingEditInvisible(POIData poi, bool xChanged, bool yChanged, bool zChanged)
        {
            string warning = FacingEditAdvice.GetSliderWarning(
                _config?.orientation_settings,
                MarkerHierarchyResolver.ResolveFacingModeOverride(poi.hierarchy_level_key),
                xChanged, yChanged, zChanged);

            if (!string.IsNullOrEmpty(warning))
                EditorNotice.Queue(FacingNoticeTitle, warning, EditorNotice.GestureQuietSeconds, NoticeKeys.FacingPreviewWarning);
        }

        // Find the POI whose rig child (or a descendant of it) is the current selection.
        private bool TryGetSelectedRigMarker(out POIData poi, out Transform marker)
        {
            poi = null;
            marker = null;

            if (Selection.activeGameObject == null)
                return false;

            Transform target = Selection.activeGameObject.transform;
            while (target != null && target.parent != null && target.parent.name != "POIEditorRig")
                target = target.parent;

            if (target == null || target.parent == null || target.parent.name != "POIEditorRig")
                return false;

            poi = _config.pois.FirstOrDefault(p => p.id == target.name);
            marker = target;
            return poi != null;
        }

        // Shared entry for Scene-view gestures and Inspector polling. Runs outside the
        // window's DrawConfigMutationScope so it never feeds the scope's JSON diff (which
        // would spam undo history and refresh the rig on every repaint).
        internal void HandleSelectedMarkerEdit(bool revealOnEdit, bool gizmoGesture)
        {
            if (!TryGetSelectedRigMarker(out POIData poi, out Transform marker))
            {
                _markerEditDetector.Reset();
                _lastRevealedPoiId = null;
                return;
            }

            // Edit-Mode orientation preview rewrites localRotation every repaint, and a
            // camera-orbit drag is also a MouseDrag event -- neither is an authored edit.
            var settings = _config.orientation_settings;
            bool previewActive = settings?.edit_mode_preview_enabled == true;
            string levelOverride = MarkerHierarchyResolver.ResolveFacingModeOverride(poi.hierarchy_level_key);
            string facingMode = FacingEditAdvice.EffectiveFacingMode(settings, levelOverride);

            // Observe first (raw pose), so a blocked drag on a verified POI still counts
            // as "the developer started editing this marker".
            bool poseChanged = _markerEditDetector.Observe(poi.id, marker.localPosition, marker.localRotation);

            if (ProcessMarkerTransformEdit(poi, marker, previewActive, facingMode, Tools.current == Tool.Rotate, out string blockedMessage))
            {
                _hasUnsavedChanges = true;
                Repaint();
            }

            // A locked-edit notice wins; otherwise a gizmo drag the preview will override
            // gets the "no visible change" advice (nothing when the mode honours it).
            string notice = blockedMessage;
            if (string.IsNullOrEmpty(notice) && gizmoGesture && Tools.current == Tool.Rotate)
                notice = poi.position_verified
                    ? VerifiedRotationLockedMessage
                    : FacingEditAdvice.GetGizmoWarning(settings, levelOverride);
            if (!string.IsNullOrEmpty(notice))
                // The lock message is never hideable (it explains a refused edit); the preview advice is.
                EditorNotice.Queue(poi.position_verified ? VerifiedNoticeTitle : FacingNoticeTitle, notice, EditorNotice.GestureQuietSeconds,
                    poi.position_verified ? null : NoticeKeys.FacingPreviewWarning);

            // Reveal even when the edit was blocked (verified): the developer should see WHY.
            // Under preview a pose diff is not trustworthy, so only the gizmo gesture counts there.
            bool editStarted = gizmoGesture || (poseChanged && !previewActive);
            if (revealOnEdit && editStarted && poi.id != _lastRevealedPoiId)
            {
                _lastRevealedPoiId = poi.id;
                RevealPoiInSpecificMarkerTab(poi);
            }
        }

        // Applies one observed marker transform to the config: unverified -> the scene's
        // rotation becomes the POI's authored facing; verified -> position and facing snap
        // back to their locked values. Returns true when the config was changed in memory.
        // blockedMessage is non-empty when a locked edit was reverted (caller shows it).
        internal bool ProcessMarkerTransformEdit(POIData poi, Transform marker, bool previewActive, string effectiveFacingMode, bool rotateToolActive, out string blockedMessage)
        {
            blockedMessage = string.Empty;
            if (poi == null || marker == null)
                return false;

            // The marker's rotation IS the authored facing unless Edit-Mode preview is
            // rewriting it. Under preview only wall_fixed still shows the authored angles
            // untouched, so there the gizmo is a real edit; in the other modes it is preview output.
            bool rotationIsAuthored = !previewActive || effectiveFacingMode == "wall_fixed";

            if (!poi.position_verified)
                return rotationIsAuthored && SyncPoiRotationFromScene(poi, marker.localRotation.eulerAngles);

            if (rotationIsAuthored && ShouldBlockVerifiedRotationChange(poi, marker.localRotation, out Quaternion lockedRotation, out string rotationMessage))
            {
                Undo.RecordObject(marker, "Revert verified POI facing");
                marker.localRotation = lockedRotation;
                blockedMessage = rotationMessage;
            }

            Vector3 lastVerifiedPosition = GetLastVerifiedPositionForPoi(poi);
            if (ShouldBlockVerifiedPositionMove(poi, marker.localPosition, lastVerifiedPosition, out Vector3 lockedPosition, out string positionMessage))
            {
                Undo.RecordObject(marker, "Revert verified POI position");
                marker.localPosition = lockedPosition;

                // With Tool Handle Position set to Center, Unity's Rotate gizmo can nudge
                // localPosition as a side effect of spinning around the visual bounds center
                // -- incidental drift, not an intentional move, so correct it silently and
                // keep any facing message that is already set.
                if (!rotateToolActive && string.IsNullOrEmpty(blockedMessage))
                    blockedMessage = positionMessage;
            }

            return false;
        }

        // Open this POI's section so the developer sees the edit they are making: switch
        // to the Specific Marker tab, expand the POI and its Position foldout, and queue
        // a scroll (the list applies it once it has measured the POI's header rect).
        internal void RevealPoiInSpecificMarkerTab(POIData poi)
        {
            if (poi == null || string.IsNullOrWhiteSpace(poi.id))
                return;

            _selectedTab = TabSelection.SpecificMarker;
            _poiFoldouts[poi.id] = true;
            _showPoiPosition = true;
            _pendingScrollPoiId = poi.id;
            Repaint();
        }
    }
}
