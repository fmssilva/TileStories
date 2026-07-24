using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TileStories.Editor
{
    // Editor window for POI marker placement against a 3D wall mesh (Workflow A).
    // Two operations, one window:
    //   Populate Rig from JSON  - creates one prefab instance per POI under POIAuthoringRig
    //   Capture Positions to JSON - reads scene objects by name, writes captured_position back
    public class POIAuthoringToolWindow : EditorWindow
    {
        [SerializeField] private WallConfigData _config;
        [SerializeField] private Transform _correctionAnchor;
        [SerializeField] private GameObject _wallMesh;
        [SerializeField] private string _configPath = "Assets/Apps/LivingRoom/config.json";
        [SerializeField] private string _streamingConfigPath = "Assets/StreamingAssets/LivingRoom/config.json";
        [SerializeField] private string _prefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        [SerializeField] private Vector2 _scrollPos;

        // Position-match tolerance: differences smaller than this count as "in sync."
        private const float SyncPositionTolerance = 0.001f;

        [MenuItem("TileStories/POI Authoring Tool #P")]
        private static void ShowWindow()
        {
            var w = GetWindow<POIAuthoringToolWindow>();
            w.titleContent = new GUIContent("POI Authoring");
            w.Show();
        }

        private void OnEnable()
        {
            TryResolveSceneReferences();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawConfigSection();
            DrawSyncStatusIndicator();
            DrawPOIList();
            DrawActionButtons();
        }

        private void DrawToolbar()
        {
            GUILayout.Label("POI Authoring Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        private void DrawConfigSection()
        {
            TryResolveSceneReferences();

            _configPath = EditorGUILayout.TextField("Config path", _configPath);
            _prefabPath = EditorGUILayout.TextField("Marker prefab", _prefabPath);
            _correctionAnchor = (Transform)EditorGUILayout.ObjectField("Correction anchor", _correctionAnchor, typeof(Transform), true);
            _wallMesh = (GameObject)EditorGUILayout.ObjectField("Wall mesh (reference)", _wallMesh, typeof(GameObject), true);

            if (_correctionAnchor == null)
            {
                EditorGUILayout.HelpBox(
                    "Populate/Capture need PlacementCorrectionAnchor. The tool tries to auto-find it by name, " +
                    "but you can still assign it manually if this scene uses a different structure.",
                    MessageType.Warning);
            }

            if (GUILayout.Button("Load config"))
            {
                LoadConfig();
            }

            EditorGUILayout.Space();
        }

        // Live sync-status indicator - shows whether rig positions match config.json
        private void DrawSyncStatusIndicator()
        {
            Transform rig = GetOrCreateRig();
            if (rig != null && rig.childCount > 0)
            {
                bool inSync = IsRigInSyncWithConfig(out int outOfSyncCount);

                var originalColor = GUI.color;
                GUI.color = inSync ? Color.green : Color.red;

                string label = inSync
                    ? "Rig matches config.json"
                    : $"Rig OUT OF SYNC - {outOfSyncCount} marker(s) differ from config.json";

                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

                GUI.color = originalColor;
            }
        }

        private void DrawPOIList()
        {
            if (_config == null || _config.pois == null || _config.pois.Count == 0)
            {
                EditorGUILayout.HelpBox("No POI data loaded. Click 'Load config' first.", MessageType.Info);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _config.pois.Count; i++)
            {
                var poi = _config.pois[i];
                EditorGUILayout.LabelField($"{i + 1}. {poi.name}", EditorStyles.boldLabel);

                using (new EditorGUI.IndentLevelScope())
                {
                    poi.name = EditorGUILayout.TextField("Name", poi.name);
                    poi.category = EditorGUILayout.TextField("Category", poi.category);
                    poi.x_norm = EditorGUILayout.Slider("X norm", poi.x_norm, 0f, 1f);
                    poi.y_norm = EditorGUILayout.Slider("Y norm", poi.y_norm, 0f, 1f);

                    bool hasCaptured = poi.has_captured_position;
                    bool wantsCaptured = EditorGUILayout.Toggle("Use captured position", hasCaptured);

                    if (wantsCaptured && !hasCaptured)
                    {
                        poi.captured_position = new CapturedPosition();
                        poi.has_captured_position = true;
                        poi.captured_position_source = "manual";
                        poi.captured_position_timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    }
                    else if (!wantsCaptured && hasCaptured)
                    {
                        poi.captured_position = null;
                        poi.has_captured_position = false;
                        poi.captured_position_source = null;
                    }

                    if (poi.captured_position != null)
                    {
                        var cp = poi.captured_position;
                        cp.x = EditorGUILayout.FloatField("X", cp.x);
                        cp.y = EditorGUILayout.FloatField("Y", cp.y);
                        cp.z = EditorGUILayout.FloatField("Z", cp.z);
                    }

                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Populate Rig from JSON"))
            {
                PopulateRig();
            }

            if (GUILayout.Button("Clear Rig"))
            {
                ClearRig();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Capture Positions to JSON"))
            {
                CapturePositions();
            }

            if (GUILayout.Button("Save config"))
            {
                SaveConfig();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Copy to StreamingAssets"))
            {
                CopyToStreamingAssets();
            }

            if (GUILayout.Button("Select all rig objects"))
            {
                SelectRigObjects();
            }

            GUILayout.EndHorizontal();
        }

        // Returns true if every POI whose corresponding rig child exists has a
        // position matching its config.json captured_position within tolerance.
        // A POI with no matching rig child, or a rig child whose position differs
        // from config.json (or from an uncaptured/never-captured POI), counts as
        // "out of sync."
        internal bool IsRigInSyncWithConfig(out int outOfSyncCount)
        {
            outOfSyncCount = 0;

            Transform rig = GetOrCreateRig();
            if (rig == null || _config == null || _config.pois == null)
                return true; // nothing to compare, treat as "in sync" (nothing to lose)

            foreach (Transform child in rig)
            {
                var poi = _config.pois.Find(p => p.id == child.name);

                if (poi == null)
                {
                    // A rig child that doesn't match any POI id in the loaded config
                    // at all - definitely out of sync, this data has nowhere to go.
                    outOfSyncCount++;
                    continue;
                }

                if (!poi.has_captured_position)
                {
                    // This POI has never been captured - the rig child holds a
                    // position not yet written anywhere. Out of sync by definition.
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

        // Clear Rig: removes all child objects from POIAuthoringRig
        private void ClearRig()
        {
            Transform rig = GetOrCreateRig();

            if (rig == null || rig.childCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "Nothing to clear",
                    "POIAuthoringRig has no children.",
                    "OK");
            }
            else
            {
                bool inSync = IsRigInSyncWithConfig(out int outOfSyncCount);

                bool proceed = inSync || EditorUtility.DisplayDialog(
                    "Uncaptured or unsynced positions",
                    $"{outOfSyncCount} marker(s) in the rig don't match config.json " +
                    "(never captured, or moved since the last capture). Clearing now " +
                    "will lose that placement work. Clear anyway?",
                    "Clear anyway",
                    "Cancel");

                if (proceed)
                {
                    for (int i = rig.childCount - 1; i >= 0; i--)
                    {
                        Undo.DestroyObjectImmediate(rig.GetChild(i).gameObject);
                    }
                    Debug.Log("[POIAuthoring] Cleared POIAuthoringRig children.");
                }
            }
        }

        // Locate or create the POIAuthoringRig container
        private Transform GetOrCreateRig()
        {
            TryResolveSceneReferences();

            Transform container = null;

            if (_correctionAnchor != null)
            {
                // Look for existing POIAuthoringRig under correction anchor
                for (int i = 0; i < _correctionAnchor.childCount; i++)
                {
                    var child = _correctionAnchor.GetChild(i);
                    if (child.name == "POIAuthoringRig")
                    {
                        container = child;
                        break;
                    }
                }

                if (container == null)
                {
                    var go = new GameObject("POIAuthoringRig");
                    Undo.RegisterCreatedObjectUndo(go, "Create POIAuthoringRig");
                    go.transform.SetParent(_correctionAnchor);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                    container = go.transform;
                }
            }

            return container;
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
            {
                _wallMesh = GameObject.Find("146267-LivingRoom2-tex");
            }
        }

        // Populate Rig from JSON: create prefab instances under POIAuthoringRig
        private void PopulateRig()
        {
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

            // Check if rig already has children and ask before clearing
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

                // Destroy all children with undo
                var children = new List<GameObject>();
                for (int i = 0; i < rig.childCount; i++)
                    children.Add(rig.GetChild(i).gameObject);

                foreach (var child in children)
                    Undo.DestroyObjectImmediate(child);
            }

            var anchors = _config.calibration_anchors?.ToArray() ?? System.Array.Empty<CalibrationAnchor>();

            foreach (var poi in _config.pois)
            {
                if (!POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 localPos))
                {
                    Debug.LogWarning($"[POIAuthoring] Skipping POI '{poi.id}' - position could not be resolved.");
                    continue;
                }

                // Use PrefabUtility.InstantiatePrefab to keep prefab link intact
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, rig);
                instance.name = poi.id;
                instance.transform.localPosition = localPos;
                instance.transform.localRotation = Quaternion.identity;

                Undo.RegisterCreatedObjectUndo(instance, $"Populate marker for {poi.id}");

                Debug.Log($"[POIAuthoring] Created marker '{poi.id}' at localPos {localPos}");
            }

            // Select all created objects
            SelectRigObjects();
            Debug.Log($"[POIAuthoring] Populated {rig.childCount} markers under POIAuthoringRig.");
        }

        // Capture Positions to JSON: read scene objects by name, write captured_position back
        private void CapturePositions()
        {
            if (_config == null || _config.pois == null)
            {
                Debug.LogWarning("[POIAuthoring] No config loaded.");
                return;
            }

            TryResolveSceneReferences();

            Transform rig = null;

            if (_correctionAnchor != null)
            {
                for (int i = 0; i < _correctionAnchor.childCount; i++)
                {
                    var child = _correctionAnchor.GetChild(i);
                    if (child.name == "POIAuthoringRig")
                    {
                        rig = child;
                        break;
                    }
                }
            }

            if (rig == null || rig.childCount == 0)
            {
                Debug.LogWarning("[POIAuthoring] No POIAuthoringRig with children found. Populate first.");
                return;
            }

            int captured = 0;
            int skipped = 0;

            // Build a lookup from scene objects by name = POI id
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

                // Match by name = POI id; read position relative to correction anchor
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
                poi.captured_position_timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                captured++;
            }

            Debug.Log($"[POIAuthoring] Captured {captured} positions (skipped {skipped} missing scene objects).");
            Repaint();
        }

        private void SaveConfig()
        {
            if (_config == null)
            {
                Debug.LogWarning("[POIAuthoring] Nothing to save.");
                return;
            }

            string json = JsonUtility.ToJson(_config, prettyPrint: true);
            File.WriteAllText(_configPath, json);
            AssetDatabase.Refresh();
            Debug.Log($"[POIAuthoring] Saved config to {_configPath}");
        }

        private void LoadConfig()
        {
            if (!File.Exists(_configPath))
            {
                Debug.LogError($"[POIAuthoring] Config not found at {_configPath}");
                return;
            }

            string json = File.ReadAllText(_configPath);
            _config = JsonUtility.FromJson<WallConfigData>(json);

            if (_config == null)
            {
                Debug.LogError("[POIAuthoring] Failed to parse config.");
                return;
            }

            Debug.Log($"[POIAuthoring] Loaded {_config.pois?.Count ?? 0} POIs from {_configPath}");
            Repaint();
        }

        private void CopyToStreamingAssets()
        {
            if (!File.Exists(_configPath))
            {
                Debug.LogError("[POIAuthoring] Source config not found.");
                return;
            }

            string dir = Path.GetDirectoryName(_streamingConfigPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.Copy(_configPath, _streamingConfigPath, overwrite: true);
            AssetDatabase.Refresh();
            Debug.Log($"[POIAuthoring] Copied to StreamingAssets: {_streamingConfigPath}");
        }

        private void SelectRigObjects()
        {
            TryResolveSceneReferences();

            Transform rig = null;

            if (_correctionAnchor != null)
            {
                for (int i = 0; i < _correctionAnchor.childCount; i++)
                {
                    var child = _correctionAnchor.GetChild(i);
                    if (child.name == "POIAuthoringRig")
                    {
                        rig = child;
                        break;
                    }
                }
            }

            if (rig == null || rig.childCount == 0)
                return;

            var gos = new List<GameObject>();
            for (int i = 0; i < rig.childCount; i++)
                gos.Add(rig.GetChild(i).gameObject);

            Selection.objects = gos.ToArray();
        }

        // Draw handle hints in Scene view for loaded POIs
        private void OnSceneGUI(SceneView sv)
        {
            if (_config == null || _config.pois == null)
                return;

            var anchors = _config.calibration_anchors?.ToArray() ?? System.Array.Empty<CalibrationAnchor>();

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