using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Inverted index for POI name / summary / keyword / category / hierarchy search.
    // Plain C# class -- no MonoBehaviour. Instantiated per wall session with
    // `new POISearchIndex()`.
    // Per-query token coverage for search (spec _2.6 section 5 / §12 match-mode).
    // `Any`: a POI qualifies if ANY query token matches (existing default behavior).
    // `All`: a POI qualifies only if EVERY query token matches (conjunction).
    // Honored by POISearchIndex.Search and threaded through ResultsListView so that
    // voice search can use a stricter mode than typed search without changing the
    // index internals -- one seam for the match policy, none duplicated elsewhere.
    public enum SearchMatchMode
    {
        Any = 0,
        All = 1,
    }

    public class POISearchIndex
    {
        // One entry in the inverted index: which POI owns a token and at what rank.
        private struct TokenEntry
        {
            public int PoiIndex;
            public float Rank;

            public TokenEntry(int poiIndex, float rank)
            {
                PoiIndex = poiIndex;
                Rank = rank;
            }
        }

        // Token -> list of (POI index, rank) pairs. A single POI appears once
        // per token, keeping the highest rank if the token appears in multiple fields.
        private readonly Dictionary<string, List<TokenEntry>> _invertedIndex = new();

        // Token -> list of POI indices that have this token in their name.
        // Used for prefix matching at search time (e.g. "lis" -> "Lisbon").
        private readonly Dictionary<string, List<int>> _nameTokenToPoiIndices = new();

        // Stored POI data references, indexed by position in _poiDatas.
        private readonly List<POIData> _poiDatas = new();

        // All unique indexed tokens, for GetMatchingKeywords prefix lookups.
        private readonly HashSet<string> _allIndexedTokens = new();

        // Duplicate-ID detection during Build.
        private readonly HashSet<string> _poiIds = new();

        // Rank constants: exact name is highest, name prefix just below,
        // keyword mid-tier, summary and taxonomy lower.
        private const float RANK_NAME = 1.0f;
        private const float RANK_NAME_PREFIX = 0.9f;
        private const float RANK_KEYWORD = 0.7f;
        private const float RANK_SUMMARY = 0.4f;
        private const float RANK_TAXONOMY = 0.3f;


        // Build the inverted index from a fully-deserialized WallConfigData.
        // Clears existing state first -- calling Build twice replaces, never
        // accumulates.
        public void Build(WallConfigData config)
        {
            Clear();

            if (config == null || config.pois == null)
                return;

            // --- First pass: index each POI's direct fields ---
            foreach (var poi in config.pois)
            {
                if (string.IsNullOrEmpty(poi.id))
                    continue;

                // Skip duplicate IDs -- first occurrence wins, matching POI
                // spawn behavior which also uses the first occurrence.
                if (!_poiIds.Add(poi.id))
                    continue;

                int index = _poiDatas.Count;
                _poiDatas.Add(poi);

                // POI name (rank 1.0)
                foreach (var token in SearchTokenizer.Tokenize(poi.name))
                {
                    AddToIndex(token, index, RANK_NAME);
                    AddToNameIndices(token, index);
                }

                // POI search_keywords -- the "Others" freeform bucket (rank 0.7)
                if (poi.search_keywords != null)
                {
                    foreach (var keyword in poi.search_keywords)
                    {
                        foreach (var token in SearchTokenizer.Tokenize(keyword))
                            AddToIndex(token, index, RANK_KEYWORD);
                    }
                }

                // Per-field custom keywords (rank 0.7) -- field_key is authoring-only;
                // the runtime index treats all keyword matches at the same quality tier.
                if (poi.search_keyword_fields != null)
                {
                    foreach (var fieldEntry in poi.search_keyword_fields)
                    {
                        if (fieldEntry?.keywords == null)
                            continue;
                        foreach (var keyword in fieldEntry.keywords)
                        {
                            foreach (var token in SearchTokenizer.Tokenize(keyword))
                                AddToIndex(token, index, RANK_KEYWORD);
                        }
                    }
                }

                // POI summary (rank 0.4)
                foreach (var token in SearchTokenizer.Tokenize(poi.summary))
                {
                    AddToIndex(token, index, RANK_SUMMARY);
                }

                // POI category (rank 0.3)
                foreach (var token in SearchTokenizer.Tokenize(poi.category))
                {
                    AddToIndex(token, index, RANK_TAXONOMY);
                }

                // Hierarchy level label (rank 0.3) -- resolved from the wall's
                // hierarchy_levels table, not stored on the POI itself.
                if (!string.IsNullOrEmpty(poi.hierarchy_level_key) && config.hierarchy_levels != null)
                {
                    HierarchyLevelEntry level = null;
                    foreach (var entry in config.hierarchy_levels)
                    {
                        if (entry.key == poi.hierarchy_level_key)
                        {
                            level = entry;
                            break;
                        }
                    }
                    if (level != null && !string.IsNullOrEmpty(level.label))
                    {
                        foreach (var token in SearchTokenizer.Tokenize(level.label))
                        {
                            AddToIndex(token, index, RANK_TAXONOMY);
                        }
                    }
                }
            }

            // --- Second pass: index taxonomy-level search_keywords ---
            IndexTaxonomyKeywords(config);
        }

        // Index search_keywords from category_styles, badge_categories, and
        // outline_levels. Each taxonomy entry's keywords are applied to all
        // POIs whose corresponding field matches the entry's key.
        private void IndexTaxonomyKeywords(WallConfigData config)
        {
            // Category styles -> POIs with matching category
            if (config.category_styles != null)
            {
                foreach (var entry in config.category_styles)
                {
                    if (entry.search_keywords == null || string.IsNullOrEmpty(entry.category))
                        continue;

                    var matchingPois = FindPoisByCategory(entry.category);
                    foreach (var keyword in entry.search_keywords)
                    {
                        foreach (var token in SearchTokenizer.Tokenize(keyword))
                        {
                            foreach (int idx in matchingPois)
                                AddToIndex(token, idx, RANK_TAXONOMY);
                        }
                    }
                }
            }

            // Badge categories -> POIs with matching badge_category
            if (config.badge_categories != null)
            {
                foreach (var entry in config.badge_categories)
                {
                    if (entry.search_keywords == null || string.IsNullOrEmpty(entry.key))
                        continue;

                    var matchingPois = FindPoisByBadgeCategory(entry.key);
                    foreach (var keyword in entry.search_keywords)
                    {
                        foreach (var token in SearchTokenizer.Tokenize(keyword))
                        {
                            foreach (int idx in matchingPois)
                                AddToIndex(token, idx, RANK_TAXONOMY);
                        }
                    }
                }
            }

            // Outline levels -> POIs with matching status_level_key
            if (config.outline_levels != null)
            {
                foreach (var entry in config.outline_levels)
                {
                    if (entry.search_keywords == null || string.IsNullOrEmpty(entry.key))
                        continue;

                    var matchingPois = FindPoisByStatusLevel(entry.key);
                    foreach (var keyword in entry.search_keywords)
                    {
                        foreach (var token in SearchTokenizer.Tokenize(keyword))
                        {
                            foreach (int idx in matchingPois)
                                AddToIndex(token, idx, RANK_TAXONOMY);
                        }
                    }
                }
            }
        }

        // Find all POI indices whose category matches the given key.
        private List<int> FindPoisByCategory(string category)
        {
            var result = new List<int>();
            for (int i = 0; i < _poiDatas.Count; i++)
            {
                if (_poiDatas[i].category == category)
                    result.Add(i);
            }
            return result;
        }

        // Find all POI indices whose badge_category matches the given key.
        private List<int> FindPoisByBadgeCategory(string key)
        {
            var result = new List<int>();
            for (int i = 0; i < _poiDatas.Count; i++)
            {
                if (_poiDatas[i].badge_category == key)
                    result.Add(i);
            }
            return result;
        }

        // Find all POI indices whose status_level_key matches the given key.
        private List<int> FindPoisByStatusLevel(string key)
        {
            var result = new List<int>();
            for (int i = 0; i < _poiDatas.Count; i++)
            {
                if (_poiDatas[i].status_level_key == key)
                    result.Add(i);
            }
            return result;
        }

        // Add a (token, POI, rank) entry to the inverted index. If the POI
        // already owns this token, keep the higher rank.
        private void AddToIndex(string token, int poiIndex, float rank)
        {
            if (string.IsNullOrEmpty(token))
                return;

            _allIndexedTokens.Add(token);

            if (!_invertedIndex.TryGetValue(token, out var entries))
            {
                entries = new List<TokenEntry>();
                _invertedIndex[token] = entries;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].PoiIndex == poiIndex)
                {
                    if (rank > entries[i].Rank)
                        entries[i] = new TokenEntry(poiIndex, rank);
                    return;
                }
            }

            entries.Add(new TokenEntry(poiIndex, rank));
        }

        // Record that a POI has a given name token (for prefix matching).
        private void AddToNameIndices(string token, int poiIndex)
        {
            if (!_nameTokenToPoiIndices.TryGetValue(token, out var indices))
            {
                indices = new List<int>();
                _nameTokenToPoiIndices[token] = indices;
            }
            indices.Add(poiIndex);
        }

        // Search the index for matches against a user query string.
        // Each query token is looked up exactly first (1.0 / 0.7 / 0.4 / 0.3).
        // If no exact name match is found, name-prefix matching kicks in (0.9).
        // Per-POI score is the MAX rank across all query tokens, not the sum.
        // Results are sorted by score desc, then POI index asc (stable).
        // Search the index for matches against a user query string.
        // Each query token is looked up exactly first (1.0 / 0.7 / 0.4 / 0.3);
        // name-prefix matching (0.9) is a fallback. Per-POI score is the MAX rank
        // across the query tokens, not the sum. matchMode controls token coverage:
        // `Any` returns any POI hit by >=1 token (existing behavior); `All` requires
        // every query token to match the POI (conjunction). Results are sorted by
        // score desc, then POI index asc (stable).
        public List<SearchResult> Search(string query, SearchMatchMode matchMode = SearchMatchMode.Any)
        {
            var results = new List<SearchResult>();

            if (string.IsNullOrEmpty(query) || _poiDatas.Count == 0)
                return results;

            var queryTokens = SearchTokenizer.Tokenize(query);
            if (queryTokens.Count == 0)
                return results;

            // Track best score per POI across all query tokens, plus which distinct
            // query-token indices each POI matched (so `All` mode can enforce coverage).
            var bestScores = new Dictionary<int, float>();
            var matchedTokenSets = new Dictionary<int, HashSet<int>>();

            int tokenCount = queryTokens.Count;
            int ti = 0;
            foreach (string queryToken in queryTokens)
            {
                // Exact token match in the inverted index
                if (_invertedIndex.TryGetValue(queryToken, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        if (!bestScores.TryGetValue(entry.PoiIndex, out float current) || entry.Rank > current)
                            bestScores[entry.PoiIndex] = entry.Rank;
                        RecordMatch(matchedTokenSets, entry.PoiIndex, ti);
                    }
                }

                // Name prefix match: queryToken is a proper prefix of a name token,
                // or a name token is a proper prefix of queryToken. Only applies to
                // name tokens (rank 0.9), never to keywords/summary/taxonomy.
                foreach (string nameToken in _nameTokenToPoiIndices.Keys)
                {
                    bool isPrefix = false;

                    // queryToken is a proper prefix of nameToken ('lis' -> 'lisbon')
                    if (nameToken.Length > queryToken.Length && nameToken.StartsWith(queryToken))
                        isPrefix = true;

                    // nameToken is a proper prefix of queryToken ('li' -> 'lisbon')
                    else if (queryToken.Length > nameToken.Length && queryToken.StartsWith(nameToken))
                        isPrefix = true;

                    if (isPrefix)
                    {
                        foreach (int poiIndex in _nameTokenToPoiIndices[nameToken])
                        {
                            if (!bestScores.TryGetValue(poiIndex, out float current) || RANK_NAME_PREFIX > current)
                                bestScores[poiIndex] = RANK_NAME_PREFIX;
                            RecordMatch(matchedTokenSets, poiIndex, ti);
                        }
                    }
                }

                ti++;
            }

            // Convert to SearchResult list, applying match-mode coverage filtering.
            foreach (var kvp in bestScores)
            {
                int poiIndex = kvp.Key;
                float score = kvp.Value;

                // `All` mode: the POI must have matched every query token.
                if (matchMode == SearchMatchMode.All)
                {
                    int matched = matchedTokenSets.TryGetValue(poiIndex, out var set) ? set.Count : 0;
                    if (matched != tokenCount)
                        continue;
                }

                string poiId = poiIndex < _poiDatas.Count ? _poiDatas[poiIndex].id : "";
                results.Add(new SearchResult(poiIndex, score, poiId));
            }

            // Stable sort: score descending, then POIIndex ascending
            results.Sort((a, b) =>
            {
                int scoreCmp = b.Score.CompareTo(a.Score);
                if (scoreCmp != 0)
                    return scoreCmp;
                return a.POIIndex.CompareTo(b.POIIndex);
            });

            return results;
        }

        // Record that POI `poiIndex` matched query-token index `tokenIndex`.
        // Helper for Search() so match-mode coverage (All) can be enforced without
        // changing the scoring loop above it.
        private static void RecordMatch(Dictionary<int, HashSet<int>> sets, int poiIndex, int tokenIndex)
        {
            if (!sets.TryGetValue(poiIndex, out var set))
            {
                set = new HashSet<int>();
                sets[poiIndex] = set;
            }
            set.Add(tokenIndex);
        }

        // Return all indexed tokens that start with the given prefix.
        // Sorted alphabetically. Empty prefix returns an empty list
        // (we never return the full vocabulary as auto-complete options).
        public List<string> GetMatchingKeywords(string prefix)
        {
            var results = new List<string>();

            if (string.IsNullOrEmpty(prefix))
                return results;

            foreach (string token in _allIndexedTokens)
            {
                if (token.StartsWith(prefix))
                    results.Add(token);
            }

            results.Sort();
            return results;
        }

        // Drop all internal state. Replaces, does not accumulate.
        public void Clear()
        {
            _invertedIndex.Clear();
            _nameTokenToPoiIndices.Clear();
            _poiDatas.Clear();
            _allIndexedTokens.Clear();
            _poiIds.Clear();
        }

        // Apply synonym groups: for each group, if any POI contains the key POI contains the key
        // token, also index the synonyms at keyword rank (0.7). This is a
        // build-time expansion so Search has zero runtime cost for synonyms.
        // Accepts a list of SynonymGroup (Runtime data type) rather than the
        // SearchSynonymGroups ScriptableObject (Editor-only) to avoid a
        // Runtime->Editor assembly dependency. The Editor wiring layer
        // extracts groups from the asset and passes them here.
        public void ConfigureWithSynonyms(IList<SynonymGroup> groups)
        {
            if (groups == null || groups.Count == 0)
                return;

            // Build a quick lookup: token -> POI indices that contain that token
            var tokenToPois = new Dictionary<string, List<int>>();
            foreach (var kvp in _invertedIndex)
            {
                var pois = new List<int>();
                foreach (var entry in kvp.Value)
                    pois.Add(entry.PoiIndex);
                tokenToPois[kvp.Key] = pois;
            }

            foreach (var group in groups)
            {
                if (string.IsNullOrEmpty(group.key))
                    continue;

                // Tokenize the key and each synonym
                var keyTokens = SearchTokenizer.Tokenize(group.key);
                var synonymTokens = new List<string>();
                if (group.synonyms != null)
                {
                    foreach (var syn in group.synonyms)
                    {
                        synonymTokens.AddRange(SearchTokenizer.Tokenize(syn));
                    }
                }

                // For each key token, find POIs that contain it, then index
                // the synonyms for those same POIs at keyword rank.
                foreach (string keyToken in keyTokens)
                {
                    if (tokenToPois.TryGetValue(keyToken, out var matchingPois))
                    {
                        foreach (int poiIndex in matchingPois)
                        {
                            foreach (string synToken in synonymTokens)
                            {
                                AddToIndex(synToken, poiIndex, RANK_KEYWORD);
                            }
                        }
                    }
                }
            }
        }

        // A single search result: the POI index, its best match score, and its ID.
        public readonly struct SearchResult
        {
            public readonly int POIIndex;
            public readonly float Score;
            public readonly string POIId;

            public SearchResult(int poiIndex, float score, string poiId)
            {
                POIIndex = poiIndex;
                Score = score;
                POIId = poiId;
            }
        }
    }
}
