using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Dev-only "Add effects demo grid" (effect_defaults.preview): spawns a labelled grid of REAL markers
    // so a developer sees every effect (and every hierarchy level's exact look) running, without hunting
    // for a marker in the wall. Three stacked blocks, each its own row (DevPreviewGridLayout forces a
    // break at every block boundary): QUICK ROW (No effect, Pulse, Spin Ring -- the single-effect
    // building blocks), COMBO ROW (the two Ripple variants, a blank spacer cell, the three Halo
    // variants -- grouped so Ripple and Halo read as two separate, independently-combinable effect
    // slots, not one flat list), then LEVEL ROW: one cell per hierarchy level, using that level's real
    // Size, Show Marker Label?, Marker Label Style, effects, Spin Ring and reveal timing, labelled with
    // the base marker's own name (never the level name). Off by default; only
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
        // One grid cell. Name identifies the cell (its GameObject is "Preview_" + Name, so a developer
        // can find it in the Hierarchy window); LabelText is what the marker's own text label shows
        // under the Symbol. The effect cells use the effect name for both (they are a legend of the
        // effects). A level cell is named after its hierarchy level but LABELLED with the base
        // marker's own name, exactly like a real POI at that level -- a hierarchy level name is an
        // authoring id, never text a marker shows. IsSpacer marks a blank placeholder cell (the gap
        // between the Ripple and Halo groups): it occupies one grid slot but spawns nothing visible.
        public readonly struct Cell
        {
            public readonly string Name;
            public readonly string LabelText;
            public readonly HierarchyStyle Style;
            public readonly bool IsSpacer;

            public Cell(string name, HierarchyStyle style, bool isSpacer = false, string labelText = null)
            {
                Name = name;
                LabelText = labelText ?? name;
                Style = style;
                IsSpacer = isSpacer;
            }
        }

        // What a cell built on the plain grey circle base is called -- the same words the "Base
        // marker" dropdown uses for it, so the label on screen matches the option the developer picked.
        public const string PlainCircleName = "Plain grey circle";

        // Height above the scene at which the grid lives. Far beyond any AR camera far plane, small
        // enough that float precision stays well below a millimetre.
        public const float FarOffsetMetres = 5000f;

        // Neutral backdrop the grid camera clears to: effect tints are judged on a fixed colour, not on
        // whatever the wall photo happens to be.
        public static readonly Color BackgroundColor = new Color(0.12f, 0.13f, 0.16f, 1f);

        // Fixed indices within the flattened cell list (quick row + combo row + level row), so a
        // caller that needs one specific cell (e.g. a pixel-reading render test) never hardcodes a
        // magic number that could silently drift from BuildRows' actual order.
        public const int QuickRowCount = 3;
        public const int ComboRowCount = 6;
        public const int IndexNoEffect = 0, IndexPulse = 1, IndexSpinRing = 2;
        public const int IndexRippleRings = 3, IndexRippleDiscs = 4, IndexComboSpacer = 5;
        public const int IndexHaloRing = 6, IndexHaloDisc = 7, IndexBeacon = 8;
        public const int LevelRowStart = QuickRowCount + ComboRowCount;

        private const float DefaultSizeCm = 28f;
        private const float QuickRevealSeconds = 0.35f;
        private static readonly Color PlainCircleColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        private static readonly HierarchyStyle SpacerStyle = new HierarchyStyle(0f, false, MarkerEffectFlags.None, false, 0f, 0f);

        // Release builds ignore the preview switch: it is developer tooling.
        public static bool IsAllowed(bool isEditor, bool isDebugBuild) => isEditor || isDebugBuild;

        private static Cell QuickCell(string name, float baseSizeCm, MarkerEffectFlags effect, bool rotate) =>
            new Cell(name, new HierarchyStyle(baseSizeCm, true, effect, rotate, 0f, QuickRevealSeconds));

        // Build the three blocks of cells for a wall. baseSizeCm is the symbol size of the quick/combo
        // rows (the level row always uses each level's own real, configured size); baseMarkerName is
        // the text a level cell's label shows (the base POI's own name, or PlainCircleName). Effects
        // that are switched off (master or own checkbox) are labelled "(off)" so a still cell is never
        // mistaken for a broken one.
        public static (List<Cell> quickRow, List<Cell> comboRow, List<Cell> levelRow) BuildRows(WallConfigData config,
            float baseSizeCm, string baseMarkerName = PlainCircleName)
        {
            var defaults = config.effect_defaults;
            string Label(MarkerEffectFlags effect) =>
                MarkerEffectNames.DisplayName(effect) + (defaults != null && (!defaults.effects_enabled || !defaults.IsEffectEnabled(effect)) ? " (off)" : "");

            // QUICK ROW: the three single building blocks a marker can carry on their own.
            var quickRow = new List<Cell>
            {
                QuickCell("No effect", baseSizeCm, MarkerEffectFlags.None, rotate: false),
                QuickCell(Label(MarkerEffectFlags.Pulse), baseSizeCm, MarkerEffectFlags.Pulse, rotate: false),
                QuickCell("Spin Ring", baseSizeCm, MarkerEffectFlags.None, rotate: true),
            };

            // COMBO ROW: Ripple (2 variants) -- spacer -- Halo (3 variants). One row, grouped so the
            // two independent, freely-combinable effect "slots" (_2.2.4_Markers_Effects.md) read as
            // two visually separate groups instead of one flat list.
            var comboRow = new List<Cell>
            {
                QuickCell(Label(MarkerEffectFlags.RippleRings), baseSizeCm, MarkerEffectFlags.RippleRings, rotate: false),
                QuickCell(Label(MarkerEffectFlags.RippleDiscs), baseSizeCm, MarkerEffectFlags.RippleDiscs, rotate: false),
                new Cell("", SpacerStyle, isSpacer: true),
                QuickCell(Label(MarkerEffectFlags.HaloRing), baseSizeCm, MarkerEffectFlags.HaloRing, rotate: false),
                QuickCell(Label(MarkerEffectFlags.HaloDisc), baseSizeCm, MarkerEffectFlags.HaloDisc, rotate: false),
                QuickCell(Label(MarkerEffectFlags.Beacon), baseSizeCm, MarkerEffectFlags.Beacon, rotate: false),
            };

            var levelRow = new List<Cell>();
            if (config.hierarchy_levels != null)
            {
                foreach (var level in config.hierarchy_levels)
                {
                    if (level == null || string.IsNullOrWhiteSpace(level.key)) continue;
                    // The label shows the base marker's name (what a real POI at this level shows),
                    // never the level name. The style comes from the same StyleOf real markers use,
                    // so Show Marker Label?, the Marker Label Style override and every other column
                    // reach the grid exactly as they reach the wall.
                    string levelName = string.IsNullOrWhiteSpace(level.level_name) ? level.key : level.level_name;
                    levelRow.Add(new Cell("Level: " + levelName,
                        MarkerHierarchyResolver.StyleOf(level, logWarnings: false), labelText: baseMarkerName));
                }
            }
            return (quickRow, comboRow, levelRow);
        }

        // ---------------- layout (pure) ----------------
        // Thin wrappers over the N-block math shared with OutlinePreviewSpawner in
        // DevPreviewGridLayout, so the two grids' camera-fit logic cannot drift apart.

        public static Vector2 GridExtent(IReadOnlyList<int> blockCounts, int columns) =>
            DevPreviewGridLayout.GridExtent(blockCounts, columns);

        public static float FitDistance(Vector2 extent, float verticalFovDeg, float aspect) =>
            DevPreviewGridLayout.FitDistance(extent, verticalFovDeg, aspect);

        public static int ChooseColumns(IReadOnlyList<int> blockCounts, float verticalFovDeg, float aspect) =>
            DevPreviewGridLayout.ChooseColumns(blockCounts, verticalFovDeg, aspect);

        public static List<Vector2> CellPositions(IReadOnlyList<int> blockCounts, int columns) =>
            DevPreviewGridLayout.CellPositions(blockCounts, columns);

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

            string baseMarkerName = basePoi == null ? PlainCircleName
                : string.IsNullOrWhiteSpace(basePoi.name) ? basePoi.id : basePoi.name;
            var (quickRow, comboRow, levelRow) = BuildRows(config, baseSizeCm, baseMarkerName);

            var root = new GameObject("EffectsPreview");
            root.transform.position = new Vector3(0f, FarOffsetMetres, 0f);

            var all = new List<Cell>(quickRow);
            all.AddRange(comboRow);
            all.AddRange(levelRow);
            var cells = new List<Transform>();
            foreach (var cell in all)
                cells.Add(SpawnCell(root.transform, markerPrefab, basePoi, cell, initialise));

            var blockCounts = new[] { quickRow.Count, comboRow.Count, levelRow.Count };
            root.AddComponent<EffectsPreviewFocus>().Begin(camera, cells, blockCounts);

            Debug.Log($"[Preview] effects grid spawned: {all.Count} cells, base '{(basePoi != null ? basePoi.id : "plain circle")}'.");
            return root;
        }

        private static Transform SpawnCell(Transform root, GameObject prefab, POIData basePoi, Cell cell,
            Action<MarkerView, POIAnchor, HierarchyStyle> initialise)
        {
            // A spacer occupies exactly one grid slot (so the layout math and cell-index-based lookups
            // stay simple) but spawns nothing visible: an empty transform, no marker prefab.
            if (cell.IsSpacer)
            {
                var spacer = new GameObject("Preview_(spacer)").transform;
                spacer.SetParent(root, false);
                return spacer;
            }

            var go = UnityEngine.Object.Instantiate(prefab, root);
            go.name = "Preview_" + cell.Name;

            var poi = CopyForCell(basePoi, go.name, cell.LabelText);
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
        // renamed so the marker's own label shows the cell's LabelText. No hierarchy key: the style override
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
