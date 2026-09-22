using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Dev-only "Focus on Effects Grid" (effect_defaults.preview): spawns a labelled grid of REAL markers
    // so a developer sees every effect (and every hierarchy level's exact look) running, without hunting
    // for a marker in the wall. Block 1: a no-effect control plus one cell per effect. Block 2: one cell
    // per hierarchy level, using that level's real size, effects and reveal timing. Off by default; only
    // honoured in the Editor and development builds, so a config that ships with the switch left on does
    // nothing in a release build.
    //
    // Why a far-away grid with its own camera: world-space UI is depth-tested, so a grid placed near the
    // wall is hidden by the room mesh, and any wall can do that. The grid therefore lives FarOffsetMetres
    // above the scene, where nothing else exists, and is drawn by a dedicated camera (EffectsPreviewFocus)
    // that copies the main camera's field of view, render target and rotation and clears to a neutral
    // colour. The real camera, its tracking and every real marker are never touched; the main camera's
    // far plane never reaches the grid, so it draws nothing of it. The view distance and column count are
    // computed to fit the grid on screen (pure functions below), for any aspect ratio.
    //
    // WallSession delegates here after it spawned the wall's POIs and supplies how a MarkerView is
    // initialised (the wall's own visual settings), so this class never needs to know them.
    public static class EffectsPreviewSpawner
    {
        // One grid cell: what to label it and the style (size + effects + reveal) it is shown with.
        public readonly struct Cell
        {
            public readonly string Name;
            public readonly HierarchyStyle Style;

            public Cell(string name, HierarchyStyle style)
            {
                Name = name;
                Style = style;
            }
        }

        // Height above the scene at which the grid lives. Far beyond any AR camera far plane, small
        // enough that float precision stays well below a millimetre.
        public const float FarOffsetMetres = 5000f;

        // Neutral backdrop the grid camera clears to: effect tints are judged on a fixed colour, not on
        // whatever the wall photo happens to be.
        public static readonly Color BackgroundColor = new Color(0.12f, 0.13f, 0.16f, 1f);

        private const float DefaultSizeCm = 20f;
        private const float CellSpacing = 0.5f;
        private const float RowSpacing = 0.65f;
        private const float GridMargin = 0.6f;
        private const float ViewFill = 0.92f;
        private const float MinDistance = 0.5f;
        private const float QuickRevealSeconds = 0.35f;
        private static readonly Color PlainCircleColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        // Release builds ignore the preview switch: it is developer tooling.
        public static bool IsAllowed(bool isEditor, bool isDebugBuild) => isEditor || isDebugBuild;

        // Build the two blocks of cells for a wall. baseSizeCm is the symbol size of the effect block.
        // Effects that are switched off (master or own checkbox) are labelled "(off)" so a still cell
        // is never mistaken for a broken one.
        public static (List<Cell> effectRow, List<Cell> levelRow) BuildRows(WallConfigData config, float baseSizeCm)
        {
            var defaults = config.effect_defaults;
            var effectRow = new List<Cell>
            {
                new Cell("No effect", new HierarchyStyle(baseSizeCm, true, MarkerEffectFlags.None, false, 0f, QuickRevealSeconds)),
            };
            foreach (var effect in EffectDefaults.SelectableEffects)
            {
                bool off = defaults != null && (!defaults.effects_enabled || !defaults.IsEffectEnabled(effect));
                string name = MarkerEffectNames.DisplayName(effect) + (off ? " (off)" : "");
                effectRow.Add(new Cell(name, new HierarchyStyle(baseSizeCm, true, effect, false, 0f, QuickRevealSeconds)));
            }

            var levelRow = new List<Cell>();
            if (config.hierarchy_levels != null)
            {
                foreach (var level in config.hierarchy_levels)
                {
                    if (level == null || string.IsNullOrWhiteSpace(level.key)) continue;
                    levelRow.Add(new Cell(level.key, new HierarchyStyle(
                        level.size_cm, true, MarkerHierarchyResolver.EffectFlagsOf(level, logWarnings: false),
                        level.rotate_contour, level.reveal_delay_s, level.reveal_duration_s)));
                }
            }
            return (effectRow, levelRow);
        }

        // ---------------- layout (pure) ----------------

        private static int RowsFor(int count, int columns) => count <= 0 ? 0 : (count + columns - 1) / columns;

        // Size of the grid, in metres, when the two blocks are wrapped at `columns` per row.
        public static Vector2 GridExtent(int effectCount, int levelCount, int columns)
        {
            int rows = RowsFor(effectCount, columns) + RowsFor(levelCount, columns);
            int cols = Mathf.Min(columns, Mathf.Max(effectCount, levelCount));
            return new Vector2((cols - 1) * CellSpacing + GridMargin, (rows - 1) * RowSpacing + GridMargin * 1.2f);
        }

        // Camera distance at which a grid of this extent fills (ViewFill of) the view.
        public static float FitDistance(Vector2 extent, float verticalFovDeg, float aspect)
        {
            float tan = Mathf.Tan(verticalFovDeg * Mathf.Deg2Rad * 0.5f);
            float byHeight = extent.y / (2f * tan);
            float byWidth = extent.x / (2f * tan * Mathf.Max(0.1f, aspect));
            return Mathf.Max(MinDistance, Mathf.Max(byWidth, byHeight) / ViewFill);
        }

        // The column count that needs the least distance, i.e. gives the biggest cells on screen
        // (a portrait view wants few columns, a wide one many).
        public static int ChooseColumns(int effectCount, int levelCount, float verticalFovDeg, float aspect)
        {
            int best = 1;
            float bestDistance = float.MaxValue;
            int max = Mathf.Max(1, Mathf.Max(effectCount, levelCount));
            for (int columns = 1; columns <= max; columns++)
            {
                float distance = FitDistance(GridExtent(effectCount, levelCount, columns), verticalFovDeg, aspect);
                if (distance < bestDistance - 1e-4f)
                {
                    best = columns;
                    bestDistance = distance;
                }
            }
            return best;
        }

        // Local cell positions: the effect block first, then the level block, each wrapped at `columns`,
        // every row centred, the whole grid centred on the origin.
        public static List<Vector2> CellPositions(int effectCount, int levelCount, int columns)
        {
            var positions = new List<Vector2>();
            int row = 0;
            foreach (int count in new[] { effectCount, levelCount })
            {
                for (int start = 0; start < count; start += columns)
                {
                    int inRow = Mathf.Min(columns, count - start);
                    for (int i = 0; i < inRow; i++)
                        positions.Add(new Vector2((i - (inRow - 1) * 0.5f) * CellSpacing, -row * RowSpacing));
                    row++;
                }
            }
            float shift = (row - 1) * RowSpacing * 0.5f;
            for (int i = 0; i < positions.Count; i++)
                positions[i] += new Vector2(0f, shift);
            return positions;
        }

        // ---------------- spawning ----------------

        // Spawn the grid if the wall asks for it and this build allows it. Returns the grid root (its
        // children are the cells, named Preview_*, plus the grid camera owned by its EffectsPreviewFocus),
        // or null when nothing was spawned. `initialise` runs the real MarkerView.Initialise for one cell with the
        // cell's style override.
        public static GameObject TrySpawn(WallConfigData config, GameObject markerPrefab, Camera camera,
            Action<MarkerView, POIAnchor, HierarchyStyle> initialise)
        {
            var preview = config?.effect_defaults?.preview;
            if (preview == null || !preview.enabled) return null;
            if (!IsAllowed(Application.isEditor, Debug.isDebugBuild)) return null;
            if (markerPrefab == null || camera == null || initialise == null)
            {
                Debug.LogWarning("[Preview] effects grid skipped: needs a marker prefab and a camera.");
                return null;
            }

            var basePoi = FindPoi(config, preview.base_poi_id);
            float baseSizeCm = DefaultSizeCm;
            if (basePoi != null && MarkerHierarchyResolver.TryResolveByKey(basePoi.hierarchy_level_key, out var baseStyle))
                baseSizeCm = baseStyle.SizeCm;

            var (effectRow, levelRow) = BuildRows(config, baseSizeCm);

            var root = new GameObject("EffectsPreview");
            root.transform.position = new Vector3(0f, FarOffsetMetres, 0f);

            var all = new List<Cell>(effectRow);
            all.AddRange(levelRow);
            var cells = new List<Transform>();
            foreach (var cell in all)
                cells.Add(SpawnCell(root.transform, markerPrefab, basePoi, cell, initialise));

            root.AddComponent<EffectsPreviewFocus>().Begin(camera, cells, effectRow.Count, levelRow.Count);

            Debug.Log($"[Preview] effects grid spawned: {all.Count} cells, base '{(basePoi != null ? basePoi.id : "plain circle")}'.");
            return root;
        }

        private static Transform SpawnCell(Transform root, GameObject prefab, POIData basePoi, Cell cell,
            Action<MarkerView, POIAnchor, HierarchyStyle> initialise)
        {
            var go = UnityEngine.Object.Instantiate(prefab, root);
            go.name = "Preview_" + cell.Name;

            var poi = CopyForCell(basePoi, go.name, cell.Name);
            var anchor = go.GetComponent<POIAnchor>() ?? go.AddComponent<POIAnchor>();
            anchor.Initialise(poi);

            var view = go.GetComponentInChildren<MarkerView>();
            initialise(view, anchor, cell.Style);
            if (basePoi == null) MakePlainCircle(go.transform);

            // The grid root follows the main camera's rotation (EffectsPreviewFocus), so cells simply
            // face the grid camera. Screen-up alignment keeps them upright whatever the phone's roll;
            // the wall's own facing mode is ignored (a wall-fixed cell would show its label back to front).
            var billboard = go.GetComponentInChildren<MarkerBillboard>();
            if (billboard != null)
                billboard.Configure(new OrientationSettings { vertical_alignment_mode = "screen_up" }, "", root);
            return go.transform;
        }

        // A cell's POI: a copy of the chosen base POI (category, status, badge...) or a blank one,
        // renamed so the marker's own label shows the cell name. No hierarchy key: the style override
        // supplies size, effects and reveal.
        private static POIData CopyForCell(POIData basePoi, string id, string label)
        {
            var poi = basePoi != null
                ? JsonUtility.FromJson<POIData>(JsonUtility.ToJson(basePoi))
                : new POIData();
            poi.id = id;
            poi.name = label;
            poi.hierarchy_level_key = null;
            return poi;
        }

        // Base "plain grey circle": the symbol as a flat grey disc without an icon, so effect tints are
        // judged against a neutral background.
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
