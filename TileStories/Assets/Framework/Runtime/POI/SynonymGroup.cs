using System;
using System.Collections.Generic;

namespace TileStories
{
    // Serializable data container for one synonym group. A wall author defines
    // a key (the canonical search term) and a list of synonyms; at Build time
    // the search index expands any POI that contains the key to also match the
    // synonyms at keyword rank (0.7).
    //
    // This is a plain C# data class with zero UnityEditor dependency so it can
    // live in the Runtime assembly. The SearchSynonymGroups ScriptableObject
    // that *contains* these groups lives in the Editor assembly and passes
    // them to POISearchIndex.ConfigureWithSynonyms at build time.
    [Serializable]
    public class SynonymGroup
    {
        // The canonical term that POIs already index.
        public string key;
        // Alternative terms that should expand to match POIs indexed with key.
        public List<string> synonyms = new();
    }
}
