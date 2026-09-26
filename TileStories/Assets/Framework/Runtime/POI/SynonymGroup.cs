using System;
using System.Collections.Generic;

namespace TileStories
{
    // One synonym group of the wall (WallConfigData.synonym_groups): words that mean the same thing.
    // The word and the synonyms are equal members -- a POI holding ANY member also matches every
    // other member (POISearchIndex.Build), so "church" finds a POI tagged "chapel" and the reverse.
    // Not a taxonomy row: nothing points at a group, so it has no generated key; its word is data.
    [Serializable]
    public class SynonymGroup
    {
        // The group's first word, the table's Word column; searched like every other member
        public string word;
        // The other member words
        public List<string> synonyms = new();
    }
}
