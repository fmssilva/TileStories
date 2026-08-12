using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Core invariant (spec §2): every distance value in this domain is the real
    // 3D Euclidean distance from the camera to that specific marker's world
    // position (Vector3.Distance(camera.position, marker.transform.position)),
    // never a shared "distance to wall plane" reused across markers. This makes
    // the "standing close, looking sideways at a wide wall" scenario resolve
    // correctly — a marker 15m to the side has a genuinely large distance and
    // therefore gets thinned/shrunk like any other far marker.
    //
    // Runs a periodic 7-step evaluation pipeline (spec §4):
    // 1. Frustum cull (§8) — skip markers outside camera FOV + margin
    // 2. Effective distance (§10: real distance / zoom factor)
    // 3. LOD band lookup (§3 explicit three-row scan) with hysteresis (§7)
    // 4. Density evaluation (§5: screen-space neighbor counts)
    // 5. Density response (§6: select_hide / cluster / shrink_and_fade / hybrid)
    // 6. Count-cap truncation (§4 step 5: cap on visual units, not raw POI count)
    // 7. Apply visibility (§4 step 6: SetVisible with soft transition fade)
    //
    // Static methods (FindBand, FindBandWithHysteresis, DensityFactor, DefaultBands)
    // are pure and Tier-0 testable without a scene. Instance methods require a
    // Camera reference and spawned markers.
    public class LODController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WallSession _wallSession;
        [SerializeField] private Camera _camera;

        [Header("Debug")]
        [SerializeField] private bool _debug = false;

                // Runtime state
        private LodSettings _settings;
        private float _timer;
        private readonly Dictionary<string, LodBand> _bandCache = new();
        private readonly Dictionary<string, float> _prevEffectiveDistance = new();
        // Density-response hysteresis (§7, Decision 7): committed density state
        // per PoiId. Persists across Evaluate() cycles because units are
        // rebuilt each cycle but the threshold-crossing decision must not flicker.
                private readonly Dictionary<string, DensityHysteresisState> _densityHysteresis = new();

        // Cluster lifecycle (spec §6.1): live cluster view instances pooled across
        // Evaluate() cycles so a dense-region aggregate fades rather than popping.
        private readonly List<MarkerClusterView> _activeClusterViews = new();
        [SerializeField] private GameObject _clusterPrefab;

        private void Awake()
        {
            if (_wallSession == null)
                _wallSession = GetComponent<WallSession>();
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Update()
        {
            if (_settings == null)
            {
                EnsureSettings();
                if (_settings == null) return;
            }

            if (!_settings.enabled) return;

            var markers = _wallSession?.SpawnedMarkers;
            if (markers == null || markers.Count == 0) return;

            _timer += Time.deltaTime;
            if (_timer < _settings.evaluation_interval_s) return;
            _timer = 0f;

            Evaluate();
        }

        // Lazily read settings from WallSession (config may load after Start).
        private void EnsureSettings()
        {
            var lodSettings = _wallSession?.LodSettings;
            if (lodSettings != null && !ReferenceEquals(lodSettings, _settings))
            {
                _settings = lodSettings;
                                _bandCache.Clear();
                _prevEffectiveDistance.Clear();
                _densityHysteresis.Clear();
            }
        }

        // ------------------------------------------------------------------
        // 7-step pipeline (spec §4)
        // ------------------------------------------------------------------

        public void Evaluate()
        {
            if (_settings == null || _wallSession == null) return;

            var allMarkers = GetMarkers();
            if (allMarkers.Count == 0) return;

            // Step 1: Frustum cull
            var frustumVisible = FrustumCull(allMarkers);

            // Step 2: Effective distance (§10: real distance / zoom factor)
            var distances = ComputeEffectiveDistances(frustumVisible);

            // Step 3: LOD band lookup with hysteresis (§3, §7)
            var bands = AssignBands(distances);

            // Step 4: Density evaluation (§5)
            var neighborCounts = EvaluateDensity(frustumVisible);

            // Step 5: Create visual units + apply density response (§6)
            var visualUnits = ApplyDensityResponse(frustumVisible, allMarkers, bands, distances, neighborCounts);

            // Step 6: Count-cap truncation (§4 step 5)
            ApplyCountCap(visualUnits, bands);

            // Step 7: Apply visibility with soft transition (§4 step 6, §7)
            ApplyVisibility(visualUnits);
        }

        // Step 1: Frustum cull (§8). Returns markers whose screen position
        // is within the viewport expanded by fov_culling_margin_deg.
        public List<MarkerView> FrustumCull(List<MarkerView> markers)
        {
            if (!_settings.frustum_culling_enabled)
                return new List<MarkerView>(markers);

            var cam = _camera;
            if (cam == null) return new List<MarkerView>();

            // Convert degree margin to viewport fraction.
            // At viewport edge (y=0 or 1), angle from center = fov/2.
            // Margin pushes the effective FOV boundary outward.
            float halfFovRad = Mathf.Deg2Rad * cam.fieldOfView * 0.5f;
            float marginRad = Mathf.Deg2Rad * _settings.fov_culling_margin_deg;
            float viewportMargin = Mathf.Tan(halfFovRad + marginRad) / (2f * Mathf.Tan(halfFovRad));

            var result = new List<MarkerView>(markers.Count);
            foreach (var marker in markers)
            {
                if (marker == null) continue;
                var viewportPos = cam.WorldToViewportPoint(marker.transform.position);
                if (viewportPos.z < 0) continue; // behind camera
                if (viewportPos.x < -viewportMargin || viewportPos.x > 1f + viewportMargin) continue;
                if (viewportPos.y < -viewportMargin || viewportPos.y > 1f + viewportMargin) continue;
                result.Add(marker);
            }

            return result;
        }

        // Step 2: Effective distance (§10: real distance / zoom factor).
        // Per the core invariant (§2), distance is per-marker Euclidean.
        public Dictionary<string, float> ComputeEffectiveDistances(List<MarkerView> markers)
        {
            var result = new Dictionary<string, float>(markers.Count);
            var cam = _camera;
            if (cam == null) return result;

            // ARZoomState.ZoomFactor defaults to 1f (no zoom) when
            // ARZoomController hasn't been set up yet (Phase 6+).
            float zoomFactor = ARZoomState.ZoomFactor;
            if (zoomFactor <= 0f) zoomFactor = 1f;

            foreach (var marker in markers)
            {
                if (marker == null) continue;
                float realDistance = Vector3.Distance(cam.transform.position, marker.transform.position);
                result[marker.PoiId] = realDistance / zoomFactor;
            }

            return result;
        }

        // Step 3: Band lookup with hysteresis (§3, §7).
        // Uses _bandCache to remember each marker's previous band.
        public Dictionary<string, LodBand> AssignBands(Dictionary<string, float> distances)
        {
            var result = new Dictionary<string, LodBand>(distances.Count);
            var bands = _settings.bands != null && _settings.bands.Count > 0
                ? _settings.bands
                : DefaultBands();

            foreach (var kvp in distances)
            {
                string id = kvp.Key;
                float effectiveDistance = kvp.Value;

                bool hasPrev = _bandCache.TryGetValue(id, out var prevBand);
                bool hasPrevDist = _prevEffectiveDistance.TryGetValue(id, out var prevDist);

                // Hysteresis (§7): demotion (farther = higher band index) is
                // immediate at the threshold; promotion (closer = lower band
                // index) only applies after crossing back by hysteresis_margin_m.
                var newBand = hasPrev
                    ? FindBandWithHysteresis(effectiveDistance, bands, prevBand, _settings.hysteresis_margin_m)
                    : FindBand(effectiveDistance, bands);

                _bandCache[id] = newBand;
                _prevEffectiveDistance[id] = effectiveDistance;
                result[id] = newBand;

                if (_debug && hasPrev && newBand.Index != prevBand.Index)
                {
                    Debug.Log($"[LOD] marker {id} band {prevBand.Index} -> {newBand.Index} at {effectiveDistance:F1}m");
                }
            }

            return result;
        }

        // Step 4: Density evaluation (§5). Screen-space radius check.
        // Counts how many OTHER markers project within density_radius_px.
        // O(n^2) over small n (post-LOD, tens not hundreds) — see spec §5.
        public Dictionary<string, int> EvaluateDensity(List<MarkerView> markers)
        {
            var result = new Dictionary<string, int>();
            var cam = _camera;
            if (cam == null || markers.Count == 0) return result;

            float radius = _settings.density_radius_px;
            float radiusSq = radius * radius;

            // Cache screen positions once per evaluation cycle.
            var screenPositions = new Vector2[markers.Count];
            var valid = new bool[markers.Count];
            for (int i = 0; i < markers.Count; i++)
            {
                var screenPos = cam.WorldToScreenPoint(markers[i].transform.position);
                valid[i] = screenPos.z > 0; // in front of camera
                screenPositions[i] = new Vector2(screenPos.x, screenPos.y);
            }

            for (int i = 0; i < markers.Count; i++)
            {
                if (!valid[i]) { result[markers[i].PoiId] = 0; continue; }

                int count = 0;
                for (int j = 0; j < markers.Count; j++)
                {
                    if (i == j || !valid[j]) continue;
                                        float distSq = (screenPositions[i] - screenPositions[j]).sqrMagnitude;
                    if (distSq < radiusSq) count++;
                }
                result[markers[i].PoiId] = count;
            }

            return result;
        }

        // Step 5: Create VisualUnits for ALL markers and apply density response (§6).
        // Frustum-culled markers become VisualUnits with isVisible=false.
        // Frustum-visible markers get band/distance/neighbor data; density
        // response strategies are implemented in Phase 3.
        public List<VisualUnit> ApplyDensityResponse(
            List<MarkerView> frustumVisible,
            List<MarkerView> allMarkers,
            Dictionary<string, LodBand> bands,
            Dictionary<string, float> distances,
            Dictionary<string, int> neighborCounts)
        {
            var visibleSet = new HashSet<MarkerView>(frustumVisible);
            var units = new List<VisualUnit>(allMarkers.Count);

            foreach (var marker in allMarkers)
            {
                if (marker == null) continue;

                                                var unit = new VisualUnit { marker = marker, poiId = marker.PoiId, worldPosition = marker.transform.position };

                if (visibleSet.Contains(marker))
                {
                    // Frustum-visible: populate full data
                    unit.isVisible = true;
                    unit.effectiveDistance = distances.TryGetValue(marker.PoiId, out var d) ? d : 0f;
                    if (bands.TryGetValue(marker.PoiId, out var band))
                        unit.band = band;
                    if (neighborCounts.TryGetValue(marker.PoiId, out var n))
                        unit.neighborCount = n;
                    unit.hierarchyLevelIndex = MarkerHierarchyResolver.GetLevelPriority(marker.HierarchyLevelKey);
                }
                else
                {
                    // Frustum-culled: not visible, no data
                    unit.isVisible = false;
                }

                units.Add(unit);
            }

            // Step 5b: apply density-response strategy (§6) with 2-cycle hysteresis
            // (§7). Operates on VisualUnit data + the caller-owned hysteresis dict —
            // Tier 0 testable with fabricated units (no MarkerView/MonoBehaviour).
            ApplyDensityStrategy(units, _settings, _densityHysteresis);

            return units;
        }

        // Step 6: Count-cap truncation (§4 step 5).
        // max_visible_count caps the number of visual units PER BAND,
        // not total — applied to visible units only, lowest-priority first.
        public void ApplyCountCap(List<VisualUnit> visualUnits, Dictionary<string, LodBand> bands)
        {
            // Group visible units by band index
            var byBand = new Dictionary<int, List<VisualUnit>>();
            foreach (var unit in visualUnits)
            {
                // A cluster aggregate (marker == null, clusterMembers != null) is
                // exactly ONE visual unit for the count cap — do not skip it.
                // It carries its own band and competes for the cap like any
                // individual marker (Phase 2 populates it).
                if (!unit.isVisible) continue;

                int bandIndex = unit.band.Index;
                if (!byBand.TryGetValue(bandIndex, out var list))
                {
                    list = new List<VisualUnit>();
                    byBand[bandIndex] = list;
                }
                list.Add(unit);
            }

            // For each band, sort by priority (closest first = highest priority)
            // and hide excess units beyond max_visible_count.
            foreach (var kvp in byBand)
            {
                var units = kvp.Value;
                var band = units[0].band; // all units in this group share the same band

                if (band.MaxVisibleCount == -1) continue; // unlimited ("show all" band)

                // Sort by hierarchy level priority first (lower index = higher priority),
                // then by effective distance as tiebreak within the same level.
                units.Sort((a, b) => ComparePriority(a, b));
                
                // Static comparison: hierarchy level index, then effective distance.
                // Extracted as a static method so Tier 0 tests can assert on it
                // without instantiating MarkerView (MonoBehaviour).

                for (int i = band.MaxVisibleCount; i < units.Count; i++)
                {
                    units[i].isVisible = false;
                }

                if (_debug)
                {
                    Debug.Log($"[LOD] band {band.Index}: showing {Mathf.Min(band.MaxVisibleCount, units.Count)}/{units.Count} markers (cap={band.MaxVisibleCount})");
                }
            }
        }

        // Step 7: Apply visibility with soft transition (§4 step 6, §7).
        // Calls SetVisible(true/false, fadeDuration) on each marker.
        public void ApplyVisibility(List<VisualUnit> visualUnits)
        {
            float fadeDuration = _settings.transition_fade_duration_s;

                        foreach (var unit in visualUnits)
            {
                if (unit.marker != null)
                {
                    unit.marker.SetVisible(unit.isVisible, fadeDuration);
                }
                else if (unit.clusterView != null)
                {
                    // Cluster aggregate: reuse the shared soft-transition fade (§4 step 6).
                    unit.clusterView.SetVisible(unit.isVisible, fadeDuration);
                }
            }
        }

        // ------------------------------------------------------------------
        // Static methods — Tier 0 testable without a scene
        // ------------------------------------------------------------------

        // Default bands per spec §3 (2m/7m/9999m, counts -1/15/5).
        // All three tiers are explicit rows — no implicit "beyond last row".
                public static List<LodBandEntry> DefaultBands()
        {
            return new List<LodBandEntry>
            {
                new LodBandEntry { max_distance_m = 2f,   max_visible_count = -1 }, // all
                new LodBandEntry { max_distance_m = 7f,   max_visible_count = 15 },
                new LodBandEntry { max_distance_m = 9999f, max_visible_count = 5 },
            };
        }

        // Band lookup: ordered scan, first row whose max_distance_m the
        // distance is under, wins. Last row is catch-all for any distance
        // beyond its threshold (but since last row = 9999m, this rarely triggers).
                public static LodBand FindBand(float effectiveDistance, List<LodBandEntry> bands)
        {
            if (bands == null || bands.Count == 0)
                bands = DefaultBands();

            for (int i = 0; i < bands.Count; i++)
            {
                if (effectiveDistance < bands[i].max_distance_m)
                    return new LodBand
                    {
                        Index = i,
                        MaxDistanceM = bands[i].max_distance_m,
                        MaxVisibleCount = bands[i].max_visible_count
                    };
            }

            // Distance >= all band thresholds: use last band
            var last = bands.Count - 1;
            return new LodBand
            {
                Index = last,
                MaxDistanceM = bands[last].max_distance_m,
                MaxVisibleCount = bands[last].max_visible_count
            };
        }

        // Band lookup with hysteresis (§7 distance-band transitions).
        // Demotion (higher Index = farther) is immediate at threshold.
        // Promotion (lower Index = closer) only after crossing back by
        // hysteresisMargin beyond the boundary.
        public static LodBand FindBandWithHysteresis(
            float effectiveDistance,
                        List<LodBandEntry> bands,
            LodBand prevBand,
            float hysteresisMargin)
        {
            var rawBand = FindBand(effectiveDistance, bands);

            if (rawBand.Index == prevBand.Index)
                return rawBand; // same band, no transition

            if (rawBand.Index > prevBand.Index)
                return rawBand; // demotion: immediate

            // Promotion (rawBand.Index < prevBand.Index): bias distance by
            // +hysteresisMargin to make promotion harder. If the biased
            // lookup still lands in the previous band, stay there.
                        // Promotion (closer band) is committed only when, even with the margin biased
            // against it, the distance still lands in a closer band than the previous one.
            // If the biased distance is still at (>=) the previous band, movement was not
            // enough to cross the hysteresis threshold -> hold the previous band.
            var biasedBand = FindBand(effectiveDistance + hysteresisMargin, bands);
            if (biasedBand.Index >= prevBand.Index)
                return prevBand; // not enough movement for promotion

            return rawBand; // promoted
        }

        // Density factor for shrink_and_fade (§6). Linear ramp from
        // shrink_start_neighbor_count (factor=1.0, unaffected) to
        // cluster_min_count (factor=0.4, floor — never fully invisible).
                public static float DensityFactor(float neighborCount, int shrinkStart, int clusterMin)
        {
            if (clusterMin <= shrinkStart) return 1f; // misconfigured, no shrink
            float t = Mathf.InverseLerp(shrinkStart, clusterMin, neighborCount);
            return Mathf.Lerp(1f, 0.4f, t);
        }

        // --- Density response strategies (§6) + 2-cycle hysteresis (§7) ---

        // Apply the configured density-response strategy to every frustum-visible
        // unit, mutating shrinkScale / isVisible in place. Reads and updates the
        // caller-owned `hysteresis` dict so the 2-cycle commit state survives
        // across Evaluate() cycles. Pure data logic: Tier 0 tests drive this with
        // fabricated VisualUnit instances (marker = null) -- no scene required.
        public static void ApplyDensityStrategy(
            List<VisualUnit> units,
            LodSettings settings,
            Dictionary<string, DensityHysteresisState> hysteresis)
        {
            if (settings == null || units == null || hysteresis == null) return;

            // Process in priority order (§5): highest hierarchy priority (lowest
            // index), then closest first. select_hide and the safety net act on the
            // lowest-priority members, so ordering first makes the step-6 cap
            // interaction deterministic and test-stable.
            var visible = new List<VisualUnit>();
            foreach (var u in units)
                if (u.isVisible) visible.Add(u);
            visible.Sort(ComparePriority);

            int shrinkStart = settings.shrink_start_neighbor_count;
            int clusterMin = settings.cluster_min_count;

            foreach (var u in visible)
            {
                var target = ComputeTargetDensityState(u.neighborCount, settings, shrinkStart, clusterMin);
                                var committed = CommitDensityState(u.poiId, target, hysteresis);
                u.densityState = committed;
                ApplyCommittedDensityState(u, committed, shrinkStart, clusterMin);
            }
        }

        // Resolve which density state a marker TARGETS this cycle (pre-hysteresis).
        // Pure on its inputs; the 2-cycle commit gate lives in CommitDensityState.
        public static DensityState ComputeTargetDensityState(
            int neighborCount,
            LodSettings s,
            int shrinkStart,
            int clusterMin)
        {
            // Safety-net escalation (§6.2): in any NON-hybrid mode, if a region is
            // unexpectedly far denser than cluster_min (x the multiplier), force
            // cluster behaviour for this marker this cycle -- a deterministic guard
            // so select_hide / shrink_and_fade never leave an unreadable pile.
            if (s != null && s.density_safety_escalation_enabled && s.density_response_mode != "hybrid" && s.density_response_mode != "none")
            {
                float safetyThreshold = clusterMin * s.density_safety_escalation_multiplier;
                if (neighborCount > safetyThreshold)
                    return DensityState.Clustered;
            }

            if (s == null) return DensityState.Normal;

            switch (s.density_response_mode)
            {
                case "none":
                    // Density is still evaluated (cheap) but nothing acts on it.
                    return DensityState.Normal;
                case "select_hide":
                    // Over cluster_min -> hidden outright; lowest-priority first
                    // is enforced by the caller's priority ordering.
                    return neighborCount >= clusterMin ? DensityState.Clustered : DensityState.Normal;
                case "cluster":
                    return neighborCount >= clusterMin ? DensityState.Clustered : DensityState.Normal;
                case "shrink_and_fade":
                    // Ramp only -- floor 0.4 at cluster_min via DensityFactor,
                    // never clusters or hides.
                    return neighborCount >= shrinkStart ? DensityState.Shrinking : DensityState.Normal;
                                case "hybrid":
                    // Shrink first; if still at/above cluster_min, escalate to cluster.
                    if (neighborCount >= clusterMin) return DensityState.Clustered;
                    if (neighborCount >= shrinkStart) return DensityState.Shrinking;
                    return DensityState.Normal;

                default:
                    // Unknown mode: conservative fallback (no action). The Stage-4
                    // WallConfigValidator flags unrecognised mode strings at edit time.
                    return DensityState.Normal;
            }
        }

        // 2-cycle commit gate (§7, Decision 7). A threshold crossing is provisional
        // on the first agreeing cycle; only a second consecutive agreement commits.
        // Returns the committed state (unchanged on the provisional first cycle).
        public static DensityState CommitDensityState(
            string poiId,
            DensityState target,
            Dictionary<string, DensityHysteresisState> hysteresis)
        {
            if (!hysteresis.TryGetValue(poiId, out var h))
                h = new DensityHysteresisState { committed = DensityState.Normal, pending = DensityState.Normal, pendingCycles = 0 };

            if (target == h.committed)
            {
                // Stable: clear any in-flight transition (one contrary cycle cancels).
                h.pending = h.committed;
                h.pendingCycles = 0;
            }
            else if (target == h.pending)
            {
                // Second consecutive cycle agrees on the provisional target -> commit.
                h.pendingCycles++;
                if (h.pendingCycles >= 2)
                {
                    h.committed = target;
                    h.pendingCycles = 0;
                    h.pending = target;
                }
            }
            else
            {
                // New target differs from both committed and pending: fresh provisional.
                h.pending = target;
                h.pendingCycles = 1;
            }

            hysteresis[poiId] = h;
            return h.committed;
        }

        // Translate a committed density state into concrete VisualUnit fields.
        // shrinkScale feeds the shrink_and_fade ramp; isVisible drives the final
        // SetVisible soft-transition call in ApplyVisibility (§7).
        private static void ApplyCommittedDensityState(
            VisualUnit unit,
            DensityState committed,
            int shrinkStart,
            int clusterMin)
        {
            switch (committed)
            {
                case DensityState.Normal:
                    unit.shrinkScale = 1f;
                    unit.isVisible = true;
                    break;
                case DensityState.Shrinking:
                    // Reuse the shared DensityFactor ramp: 1.0 at shrink_start,
                    // 0.4 floor at cluster_min. Above cluster_min it clamps to 0.4
                    // (never fully vanishes) -- the correct floor for shrink_and_fade.
                    unit.shrinkScale = DensityFactor(unit.neighborCount, shrinkStart, clusterMin);
                    unit.isVisible = true;
                    break;
                case DensityState.Clustered:
                    // select_hide hides outright; cluster/hybrid absorb into a cluster
                    // aggregate (rendered by MarkerClusterView, Phase 2). Either way
                    // the individual marker is not rendered this cycle.
                    unit.shrinkScale = 1f;
                    unit.isVisible = false;
                    break;
            }
        }

        // Config sanity check (§6: shrink_start < cluster_min). Misconfiguration
        // makes the shrink ramp a no-op -- DensityFactor returns 1.0 when
        // clusterMin <= shrinkStart -- so this is a detection surface, not a guard.
        public static bool IsDensityConfigValid(LodSettings s) =>
            s != null && s.cluster_min_count > 0 && s.shrink_start_neighbor_count < s.cluster_min_count;

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------
        
        // Compare two visual units by priority for count-cap truncation.
        // Lower hierarchyLevelIndex wins; ties broken by effectiveDistance (closer first).
        public static int ComparePriority(VisualUnit a, VisualUnit b)
        {
            int cmp = a.hierarchyLevelIndex.CompareTo(b.hierarchyLevelIndex);
            if (cmp != 0) return cmp;
            return a.effectiveDistance.CompareTo(b.effectiveDistance);
        }

        private List<MarkerView> GetMarkers()
        {
            if (_wallSession == null || _wallSession.SpawnedMarkers == null)
                return new List<MarkerView>();

            var list = new List<MarkerView>();
            foreach (var m in _wallSession.SpawnedMarkers)
            {
                if (m != null) list.Add(m);
            }
            return list;
        }
    }

    // A resolved LOD band. Index for ordering, MaxDistanceM for boundary,
    // MaxVisibleCount (-1 = unlimited) for per-band count cap.
    public struct LodBand
    {
        public int Index;
        public float MaxDistanceM;
        public int MaxVisibleCount;
    }

    // Represents one marker's state through the LOD pipeline.
        //isVisible determines whether SetVisible(true/false) is called in step 7.
    // Phase 4 adds cluster support (clusterMembers non-null).
    public class VisualUnit
    {
        public MarkerView marker;
        public string poiId;
        public Vector3 worldPosition; // L272 sets this; Phase 2 (clustering) reads it for centroid
        public bool isVisible = true;
        public float effectiveDistance;
        public LodBand band;
        public int neighborCount;
        public float shrinkScale = 1f; // 1.0 = full, 0.4 = floor (Phase 3: shrink_and_fade)
        public DensityState densityState; // committed state this cycle (set L492); read by Phase 2 clustering

        // Cluster support (Phase 4): null for individual markers,
        // non-null for cluster aggregates.
        public List<VisualUnit> clusterMembers;
        public MarkerClusterView clusterView; // aggregate that absorbed this unit (null for individuals; guarded at L367)
        public int hierarchyLevelIndex = int.MaxValue; // Phase 1: for count-cap priority sort
    }

    // Committed density-response state for one marker (§6/§7).
    public enum DensityState : byte
    {
        Normal,    // below shrink_start: no density action
        Shrinking, // in the shrink_and_fade ramp (or hybrid's shrink phase)
        Clustered, // at/above cluster_min: hidden or absorbed into a cluster aggregate
    }

    // Hysteresis bookkeeping for one marker, keyed by PoiId in LODController.
    public struct DensityHysteresisState
    {
        public DensityState committed;   // state currently driving visuals
        public DensityState pending;     // provisional target mid-transition
        public int pendingCycles;        // consecutive cycles the pending target has held
    }
}
