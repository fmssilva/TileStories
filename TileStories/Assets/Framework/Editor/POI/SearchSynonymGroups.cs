using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories
{
    // ScriptableObject asset holding all synonym groups for a wall. Created via
    // the CreateAssetMenu wizard in the editor. Entirely editor-only: the type
    // is absent from player builds. The SynonymGroup data class it contains
    // lives in Runtime so POISearchIndex can reference it without a Runtime->Editor
    // assembly dependency violation.
    #if UNITY_EDITOR
    public class SearchSynonymGroups : ScriptableObject
    {
        public List<SynonymGroup> groups = new();

        [MenuItem("TileStories/Search/Create Synonym Asset")]
        private static void CreateAsset()
        {
            var asset = CreateInstance<SearchSynonymGroups>();
            AssetDatabase.CreateAsset(asset, "Assets/SearchSynonyms.asset");
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }
    }
    #endif
}
