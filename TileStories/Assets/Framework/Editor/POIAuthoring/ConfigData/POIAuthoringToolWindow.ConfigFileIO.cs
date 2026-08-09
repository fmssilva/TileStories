using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        private void SaveAllToJson()
        {
            CapturePositions(silentWhenRigMissing: true);

            // Non-blocking validation: warn before saving if any POI's
            // hierarchy_level_key does not resolve to a hierarchy_levels entry.
            ValidateAndAlert("before save");

            SaveConfig();
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
            _hasUnsavedChanges = false;
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

            if (_config.category_styles == null) _config.category_styles = new List<CategoryStyleEntry>();
            if (_config.badge_categories == null) _config.badge_categories = new List<BadgeCategoryEntry>();
            if (_config.outline_levels == null) _config.outline_levels = new List<OutlineLevelEntry>();
            if (_config.hierarchy_levels == null) _config.hierarchy_levels = new List<HierarchyLevelEntry>();
            if (_config.pois == null) _config.pois = new List<POIData>();

            // Seed defaults only if genuinely empty (section 13.2) -- a brand-new wall,
            // not one that already has entries the developer chose.
            if (_config.category_styles.Count == 0)
                _config.category_styles.AddRange(DefaultCategoryStyles.Create());

            if (_config.badge_categories.Count == 0)
                _config.badge_categories.AddRange(DefaultBadgeCategories.Create());

            if (_config.outline_levels.Count == 0)
                _config.outline_levels.AddRange(DefaultOutlineLevels.Create());

            EnsureDefaultIconLibraryLoaded();
            TryResolveWallIconLibraryFromConfig();
            InitializeConfigHistory();
            _hasUnsavedChanges = false;

            // Non-blocking validation: warn after load if any POI's
            // hierarchy_level_key does not resolve to a hierarchy_levels entry.
            ValidateAndAlert("after load");

            Debug.Log($"[POIAuthoring] Loaded {_config.pois.Count} POIs from {_configPath}");
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
    }
}
