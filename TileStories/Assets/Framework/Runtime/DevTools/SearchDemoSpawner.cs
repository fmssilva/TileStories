using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Dev-only "Add search & filter demo" (SearchDemoSettings): spawns the SearchDemoLayout POIs as REAL
    // POI_Marker instances at the demo's pose (DemoFieldStage decides it) and hands them to WallSession,
    // which then searches, filters and selects them instead of its own POIs. Off by default; honoured
    // only in the Editor and development builds.
    public static class SearchDemoSpawner
    {
        // Release builds ignore the switch: it is developer tooling (same rule as the other demos)
        public static bool IsAllowed(bool isEditor, bool isDebugBuild) => DemoFieldSpawner.IsAllowed(isEditor, isDebugBuild);

        // The demo exists when its switch is on, this kind of build allows it, there is a prefab and a
        // camera -- and neither the LOD demo field nor the displacement demo is on (they win)
        public static bool ShouldSpawn(WallConfigData config, GameObject markerPrefab, Camera camera) =>
            config?.search_demo != null && config.search_demo.enabled && markerPrefab != null && camera != null
            && IsAllowed(Application.isEditor, Debug.isDebugBuild)
            && !DemoFieldSpawner.ShouldSpawn(config, markerPrefab, camera)
            && !DisplacementDemoSpawner.ShouldSpawn(config, markerPrefab, camera);

        // LOD leaves the demo alone unless "Run LOD on the demo" is ticked (it would hide or cluster the
        // very POIs a search should show)
        public static bool PausesLod(SearchDemoSettings settings) => settings != null && !settings.run_lod;

        // Spawn the demo under `parent` at `pose` (call only when ShouldSpawn); every MarkerView goes into `live`
        public static GameObject Spawn(WallConfigData config, GameObject markerPrefab, Pose pose, Transform parent,
            Action<MarkerView, POIAnchor, HierarchyStyle?> initialise, List<MarkerView> live)
        {
            var settings = config.search_demo;
            var root = new GameObject("Search Demo");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(pose.position, pose.rotation);

            foreach (var entry in SearchDemoLayout.Build(settings, config))
            {
                var go = UnityEngine.Object.Instantiate(markerPrefab, root.transform);
                go.name = entry.Poi.id;
                go.transform.localPosition = entry.LocalPosition;
                go.transform.localRotation = Quaternion.identity;

                var anchor = go.GetComponent<POIAnchor>();
                if (anchor == null) anchor = go.AddComponent<POIAnchor>();
                anchor.Initialise(entry.Poi);
                var view = go.GetComponentInChildren<MarkerView>();
                if (view == null) continue;
                initialise(view, anchor, DemoFieldSpawner.LevelStyleWithLabel(config, entry.Poi.hierarchy_level_key, settings.show_labels));
                live.Add(view);
            }
            return root;
        }
    }
}
