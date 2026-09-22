using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Dev-only "Add outline demo grid" (outline_preview): spawns a labelled grid of REAL markers so a
    // developer sees every configured outline level's colour, dash pattern and spin running, without
    // hunting for the right POI in the wall. Same shape as EffectsPreviewSpawner (_2.2.4 section 3.9):
    // Block 1 is a no-outline control cell. Block 2 is one cell per outline level (real colour, line
    // style and spin, exactly as MarkerVisualResolver would draw them for a POI on that level) plus one
    // "Unknown" cell. Off by default; only honoured in the Editor and development builds, so a config
    // that ships with the switch left on does nothing in a release build. The camera-fit math is
    // shared with the effects grid via DevPreviewGridLayout so the two implementations cannot drift.
    //
    // Why a far-away grid with its own camera: see EffectsPreviewSpawner. This grid lives at a
    // different height (OutlineFarOffsetMetres) so the two grids never coincide if both switches are
    // ever left on at once.
    //
    // WallSession delegates here after it spawned the wall's POIs and supplies how a MarkerView is
    // initialised (the wall's own visual settings), so this class never needs to know them -- it shows
    // whichever outline mode (Uniform / Same hue / Per outline type) the wall is actually configured
    // with, the same as every real marker on the wall.
    public static class OutlinePreviewSpawner
    {
        // One grid cell: what to label it and the POI shape (status axis + spin) it is shown with.
        public readonly struct Cell
        {
            public readonly string Name;
            public readonly bool HasStatus;
            public readonly float StatusPct;
            public readonly string StatusLevelKey;
            public readonly bool StatusUnknown;
            public readonly bool RotateContour;

            public Cell(string name, bool hasStatus, float statusPct, string statusLevelKey, bool statusUnknown, bool rotateContour)
            {
                Name = name;
                HasStatus = hasStatus;
                StatusPct = statusPct;
                StatusLevelKey = statusLevelKey;
                StatusUnknown = statusUnknown;
                RotateContour = rotateContour;
            }
        }

        // A different height than EffectsPreviewSpawner.FarOffsetMetres, so the two grids never
        // coincide if both preview switches are ever left on at the same time.
        public const float FarOffsetMetres = 6500f;

        public static readonly Color BackgroundColor = new Color(0.10f, 0.12f, 0.15f, 1f);

        private const float DefaultSizeCm = 20f;
        private static readonly Color PlainCircleColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        // Release builds ignore the preview switch: it is developer tooling.
        public static bool IsAllowed(bool isEditor, bool isDebugBuild) => isEditor || isDebugBuild;

        // Build the two blocks of cells for a wall's currently configured outline levels.
        public static (List<Cell> controlRow, List<Cell> levelRow) BuildRows(WallConfigData config)
        {
            var controlRow = new List<Cell>
            {
                new Cell("No outline", hasStatus: false, statusPct: 0f, statusLevelKey: "", statusUnknown: false, rotateContour: false),
            };

            var levelRow = new List<Cell>();
            bool hasOwnUnknownLevel = false;
            if (config.outline_levels != null)
            {
                foreach (var level in config.outline_levels)
                {
                    if (level == null) continue;
                    string label = string.IsNullOrWhiteSpace(level.label) ? level.key : level.label;
                    bool isUnknownRow = string.Equals(level.key?.Trim(), "unknown", System.StringComparison.OrdinalIgnoreCase);
                    hasOwnUnknownLevel |= isUnknownRow;
                    // A wall's own "unknown" row is a real, developer-authored level like any other
                    // (colour, dash pattern, spin) -- it drives status_unknown POIs on its own, so it
                    // gets a normal known-status cell here, not the synthetic dotted-grey one below.
                    levelRow.Add(new Cell(label, hasStatus: true, statusPct: level.pct,
                        statusLevelKey: level.key, statusUnknown: false, rotateContour: true));
                }
            }
            // Only add the synthetic "Unknown" cell when the wall has NOT authored its own "unknown"
            // row -- otherwise the grid would show it twice (real bug found visually in this pass: the
            // shipped LivingRoom config already has a "key": "unknown" row, and this cell duplicated
            // it under an identical label). Mirrors MarkerVisualResolver.TryResolveUnknownLevel's own
            // fallback chain: a real authored level wins over the synthetic default.
            if (!hasOwnUnknownLevel)
                levelRow.Add(new Cell("Unknown", hasStatus: true, statusPct: 100f,
                    statusLevelKey: "unknown", statusUnknown: true, rotateContour: true));

            return (controlRow, levelRow);
        }

        // Spawn the grid if the wall asks for it and this build allows it. Returns the grid root (its
        // children are the cells, named Preview_*, plus the grid camera owned by its OutlinePreviewFocus),
        // or null when nothing was spawned. `initialise` runs the real MarkerView.Initialise for one cell
        // with the cell's style override.
        public static GameObject TrySpawn(WallConfigData config, GameObject markerPrefab, Camera camera,
            Action<MarkerView, POIAnchor, HierarchyStyle> initialise)
        {
            var preview = config?.outline_preview;
            if (preview == null || !preview.enabled) return null;
            if (!IsAllowed(Application.isEditor, Debug.isDebugBuild)) return null;
            if (markerPrefab == null || camera == null || initialise == null)
            {
                Debug.LogWarning("[Preview] outline grid skipped: needs a marker prefab and a camera.");
                return null;
            }

            var basePoi = FindPoi(config, preview.base_poi_id);
            float baseSizeCm = DefaultSizeCm;
            if (basePoi != null && MarkerHierarchyResolver.TryResolveByKey(basePoi.hierarchy_level_key, out var baseStyle))
                baseSizeCm = baseStyle.SizeCm;

            var (controlRow, levelRow) = BuildRows(config);

            var root = new GameObject("OutlinePreview");
            root.transform.position = new Vector3(0f, FarOffsetMetres, 0f);

            var all = new List<Cell>(controlRow);
            all.AddRange(levelRow);
            var cells = new List<Transform>();
            foreach (var cell in all)
                cells.Add(SpawnCell(root.transform, markerPrefab, basePoi, cell, baseSizeCm, initialise));

            root.AddComponent<OutlinePreviewFocus>().Begin(camera, cells, controlRow.Count, levelRow.Count);

            Debug.Log($"[Preview] outline grid spawned: {all.Count} cells, base '{(basePoi != null ? basePoi.id : "plain circle")}'.");
            return root;
        }

        private static Transform SpawnCell(Transform root, GameObject prefab, POIData basePoi, Cell cell,
            float baseSizeCm, Action<MarkerView, POIAnchor, HierarchyStyle> initialise)
        {
            var go = UnityEngine.Object.Instantiate(prefab, root);
            go.name = "Preview_" + cell.Name;

            var poi = CopyForCell(basePoi, go.name, cell.Name, cell);
            var anchor = go.GetComponent<POIAnchor>() ?? go.AddComponent<POIAnchor>();
            anchor.Initialise(poi);

            var style = new HierarchyStyle(baseSizeCm, true, MarkerEffectFlags.None, cell.RotateContour, 0f, 0.35f);
            var view = go.GetComponentInChildren<MarkerView>();
            initialise(view, anchor, style);
            if (basePoi == null) MakePlainCircle(go.transform);

            // The grid root follows the main camera's rotation (OutlinePreviewFocus), so cells simply
            // face the grid camera, whatever the wall's own facing mode is.
            var billboard = go.GetComponentInChildren<MarkerBillboard>();
            if (billboard != null)
                billboard.Configure(new OrientationSettings { vertical_alignment_mode = "screen_up" }, "", root);
            return go.transform;
        }

        // A cell's POI: a copy of the chosen base POI (category, badge...) or a blank one, with this
        // cell's own status axis, renamed so the marker's own label shows the cell name.
        private static POIData CopyForCell(POIData basePoi, string id, string label, Cell cell)
        {
            var poi = basePoi != null
                ? JsonUtility.FromJson<POIData>(JsonUtility.ToJson(basePoi))
                : new POIData();
            poi.id = id;
            poi.name = label;
            poi.hierarchy_level_key = null;
            poi.has_status = cell.HasStatus;
            poi.status_pct = cell.StatusPct;
            poi.status_level_key = cell.StatusLevelKey;
            poi.status_unknown = cell.StatusUnknown;
            return poi;
        }

        // Base "plain grey circle": the symbol as a flat grey disc without an icon, so ring colours
        // are judged against a neutral background.
        private static void MakePlainCircle(Transform marker)
        {
            var symbol = marker.Find("Symbol")?.GetComponent<Image>();
            if (symbol != null)
            {
                symbol.sprite = MarkerCircleSpriteFactory.GetFilled(0.98f);
                symbol.color = PlainCircleColor;
                symbol.enabled = true;
            }
            var icon = marker.Find("Symbol/Icon")?.GetComponent<Image>();
            if (icon != null) icon.enabled = false;
        }

        private static POIData FindPoi(WallConfigData config, string id)
        {
            if (string.IsNullOrWhiteSpace(id) || config.pois == null) return null;
            return config.pois.Find(p => p != null && p.id == id);
        }
    }
}
