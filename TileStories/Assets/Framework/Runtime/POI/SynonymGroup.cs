using System;
using System.Collections.Generic;

namespace TileStories
{
    // One synonym group of the wall (WallConfigData.synonym_groups): words that mean the same thing.
    // The key and the synonyms are equal members -- a POI holding ANY member also matches every
    // other member (POISearchIndex.Build), so "church" finds a POI tagged "chapel" and the reverse.
    [Serializable]
    public class SynonymGroup
    {
        // The group's name in the Editor table; also a member word
        public string key;
        // The other member words
        public List<string> synonyms = new();
    }
}
