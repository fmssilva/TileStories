using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Dev-only "Add LOD demo field" (DemoFieldSettings): spawns the DemoFieldLayout markers as REAL
    // POI_Marker instances at the field's pose (DemoFieldStage decides it), so the real LODController
    // (bands, crowding, clusters) and the displacement step act on them exactly as on the wall's POIs,
    // and the developer walks through them with the mock camera. Unlike the effects / outline demo grids
    // there is no separate camera: LOD depends on the real camera's distance and screen crowding. Off by
    // default; honoured only in the Editor and development builds.
    //
    // WallSession supplies how a MarkerView is initialised (the wall's own visual settings, orientation),
    // so this class never needs to know them.
    public static class DemoFieldSpawner
    {
        // Release builds ignore the switch: it is developer tooling.
        public static bool IsAllowed(bool isEditor, bool isDebugBuild) => isEditor || isDebugBuild;

        // The field exists when its switch is on, this kind of build allows it, and there is a marker
        // prefab and a camera to see it with
        public static bool ShouldSpawn(WallConfigData config, GameObject markerPrefab, Camera camera) =>
            config?.demo_field != null && config.demo_field.enabled && markerPrefab != null && camera != null
            && IsAllowed(Application.isEditor, Debug.isDebugBuild);

        // The pose of the field: at the camera, turned like the camera around the vertical only, so
        // the box stays level even when the camera looks up or down at spawn.
        public static Pose FieldPose(Transform camera)
        {
            Vector3 forward = Vector3.ProjectOnPlane(camera.forward, Vector3.up);
            if (forward.sqrMagnitude < 1e-6f) forward = Vector3.ProjectOnPlane(camera.up, Vector3.up); // looking straight down/up
            if (forward.sqrMagnitude < 1e-6f) forward = Vector3.forward;
            return new Pose(camera.position, Quaternion.LookRotation(forward.normalized, Vector3.up));
        }

        // Spawn the field under `parent` at `fieldPose` (call only when ShouldSpawn); every created
        // MarkerView is appended to `spawned`. The root exists even when every count is 0, so the field
        // stays where it is while the developer slides a count down and up again.
        public static GameObject Spawn(WallConfigData config, GameObject markerPrefab, Pose fieldPose, Transform parent,
            Action<MarkerView, POIAnchor, HierarchyStyle?> initialise, List<MarkerView> spawned)
        {
            var settings = config.demo_field;
            var categories = new List<string>();
            if (config.category_styles != null)
                foreach (var c in config.category_styles)
                    if (c != null && !string.IsNullOrEmpty(c.category)) categories.Add(c.category);

            var layout = DemoFieldLayout.Build(settings, config.hierarchy_levels, categories);

            var root = new GameObject("LOD Demo Field");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(fieldPose.position, fieldPose.rotation);

            foreach (var entry in layout)
            {
                var go = UnityEngine.Object.Instantiate(markerPrefab, root.transform);
                go.name = entry.Id;
                go.transform.localPosition = entry.LocalPosition;
                go.transform.localRotation = Quaternion.identity;

                var anchor = go.GetComponent<POIAnchor>();
                if (anchor == null) anchor = go.AddComponent<POIAnchor>();
                anchor.Initialise(new POIData
                {
                    id = entry.Id,
                    name = entry.Name,
                    category = entry.Category,
                    hierarchy_level_key = entry.LevelKey,
                });
                var view = go.GetComponentInChildren<MarkerView>();
                if (view == null) continue;
                initialise(view, anchor, settings.show_labels ? (HierarchyStyle?)null : LevelStyleWithLabel(config, entry.LevelKey, false));
                spawned.Add(view);
            }
            return root;
        }

        // The level's real look with its text label forced on or off (a demo's "Show labels" switch)
        public static HierarchyStyle LevelStyleWithLabel(WallConfigData config, string levelKey, bool showLabel)
        {
            var level = config.hierarchy_levels?.Find(l => l != null && l.key == levelKey);
            var s = MarkerHierarchyResolver.StyleOf(level, logWarnings: false);
            return new HierarchyStyle(s.SizeCm, showLabel, s.EffectFlags, s.RotateContour, s.RevealDelaySeconds,
                s.RevealDurationSeconds, s.OverridesLabelStyle, s.LabelGapRatio, s.LabelFontSizeRatio, s.LabelFontKey);
        }
    }
}
