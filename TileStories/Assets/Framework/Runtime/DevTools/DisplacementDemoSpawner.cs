using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Dev-only "Add displacement demo" (DisplacementDemoSettings): spawns the DisplacementDemoLayout markers
    // as REAL POI_Marker instances at the demo's pose (DemoFieldStage decides it). The live markers are
    // handed back to WallSession, so the real LODController runs displacement on them exactly as on the
    // wall's POIs; the faded reference copies are not, so they never move and show the true positions.
    // Off by default; honoured only in the Editor and development builds.
    public static class DisplacementDemoSpawner
    {
        // Release builds ignore the switch: it is developer tooling (same rule as the LOD demo field)
        public static bool IsAllowed(bool isEditor, bool isDebugBuild) => DemoFieldSpawner.IsAllowed(isEditor, isDebugBuild);

        // The demo exists when its switch is on, this kind of build allows it, there is a prefab and a
        // camera -- and the LOD demo field is not on (both take over the wall; the LOD field wins)
        public static bool ShouldSpawn(WallConfigData config, GameObject markerPrefab, Camera camera) =>
            config?.displacement_demo != null && config.displacement_demo.enabled && markerPrefab != null && camera != null
            && IsAllowed(Application.isEditor, Debug.isDebugBuild)
            && !DemoFieldSpawner.ShouldSpawn(config, markerPrefab, camera);

        // While the demo runs, LOD leaves it alone unless "Run LOD on the demo" is ticked (crowding would
        // otherwise shrink or cluster the very groups the demo is showing)
        public static bool PausesLod(DisplacementDemoSettings settings) => settings != null && !settings.run_lod;

        // Spawn the demo under `parent` at `pose` (call only when ShouldSpawn); every LIVE MarkerView is
        // appended to `live`, reference copies are not
        public static GameObject Spawn(WallConfigData config, GameObject markerPrefab, Pose pose, Transform parent,
            Action<MarkerView, POIAnchor, HierarchyStyle?> initialise, List<MarkerView> live)
        {
            var settings = config.displacement_demo;
            var categories = new List<string>();
            if (config.category_styles != null)
                foreach (var c in config.category_styles)
                    if (c != null && !string.IsNullOrEmpty(c.category)) categories.Add(c.category);

            var root = new GameObject("Displacement Demo");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(pose.position, pose.rotation);

            foreach (var entry in DisplacementDemoLayout.Build(settings, config.hierarchy_levels, categories))
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
                initialise(view, anchor, DemoFieldSpawner.LevelStyleWithLabel(config, entry.LevelKey, settings.show_labels));
                if (entry.IsReference) view.SetSelectionAlpha(DisplacementDemoLayout.ReferenceAlpha, 0f);
                else live.Add(view);
            }
            return root;
        }
    }
}
