using System;
using System.Collections.Generic;

namespace TileStories
{
    // Token coverage of a multi-word query: All = every word must match the POI (precise), Any = one is
    // enough (broad). Typed and voice queries share it (select_filter_search.search.match_mode).
    public enum SearchMatchMode
    {
        Any = 0,
        All = 1,
    }

    // Which indexed words a partial query word may complete: none, only POI names, or every source.
    public enum SearchPrefixScope
    {
        Off,
        NameOnly,
        AllFields,
    }

    // How one query runs (built from SearchSettings by SelectFilterSearchOptions.ToSearchOptions)
    public readonly struct SearchOptions
    {
        public readonly SearchMatchMode MatchMode;
        public readonly SearchPrefixScope Prefix;
        public readonly int MaxTypoEdits;

        public SearchOptions(SearchMatchMode matchMode, SearchPrefixScope prefix, int maxTypoEdits)
        {
            MatchMode = matchMode;
            Prefix = prefix;
            MaxTypoEdits = Math.Max(0, maxTypoEdits);
        }

        // The framework defaults: every word must match, partial words complete, one typo forgiven
        public static SearchOptions Default => new SearchOptions(SearchMatchMode.All, SearchPrefixScope.AllFields, 1);
    }

    // Inverted index over every searchable word of a wall's POIs (spec _2.6 section 5). Plain C#,
    // built once per config (Build replaces, never accumulates). Authoring keeps keywords as a tree --
    // taxonomy rows, custom search axes, freeform per-POI keywords, wall synonym groups -- and Build
    // flattens it into token -> (POI, rank, source), so a query is one dictionary lookup per word.
    public class POISearchIndex
    {
        // Where an indexed word came from (the readout says it, the rank follows from it)
        public enum MatchSource { Name, Keyword, Synonym, Summary, Taxonomy }

        // How a query word met the indexed word
        public enum MatchKind { Exact, Prefix, Typo }

        // Rank of a word by its source: the POI's own name beats its own keywords beats its summary
        // beats what it inherits from the taxonomy rows it belongs to.
        public const float RankName = 1.0f;
        public const float RankKeyword = 0.7f;
        public const float RankSynonym = 0.6f;
        public const float RankSummary = 0.4f;
        public const float RankTaxonomy = 0.3f;

        // A partial word or a misspelt one is a weaker match than the whole word
        public const float PrefixFactor = 0.9f;
        public const float OneTypoFactor = 0.8f;
        public const float TwoTypoFactor = 0.6f;

        // Shortest query word that may complete a longer one ("c" completing everything is noise)
        public const int MinPrefixLength = 2;
        // Shortest query word forgiven one typo, and two typos (short words have too many neighbours)
        public const int MinLengthForOneTypo = 4;
        public const int MinLengthForTwoTypos = 8;

        private struct TokenEntry
        {
            public int PoiIndex;
            public float Rank;
            public MatchSource Source;
        }

        private readonly Dictionary<string, List<TokenEntry>> _index = new();
        private readonly List<POIData> _pois = new();
        private readonly Dictionary<string, int> _indexById = new();

        // Number of POIs indexed (POIs without an id and repeated ids are skipped)
        public int PoiCount => _pois.Count;

        // Number of distinct indexed words
        public int TokenCount => _index.Count;

        // Index every word each POI of the config is found by (SearchKeywordSources: name, own keywords,
        // summary, taxonomy rows, synonyms). Calling it again replaces the previous index.
        public void Build(WallConfigData config)
        {
            Clear();
            if (config?.pois == null)
                return;

            foreach (var poi in config.pois)
            {
                // - first occurrence of an id wins, same as spawning
                if (poi == null || string.IsNullOrEmpty(poi.id) || _indexById.ContainsKey(poi.id))
                    continue;
                int i = _pois.Count;
                _pois.Add(poi);
                _indexById[poi.id] = i;

                foreach (var word in SearchKeywordSources.Collect(config, poi))
                    AddText(word.Text, i, word.Rank, word.Source);
            }
        }

        private void AddText(string text, int poiIndex, float rank, MatchSource source)
        {
            foreach (var token in SearchTokenizer.Tokenize(text))
                Add(token, poiIndex, rank, source);
        }

        // One (word, POI) pair is stored once, at its best rank
        private void Add(string token, int poiIndex, float rank, MatchSource source)
        {
            if (!_index.TryGetValue(token, out var entries))
            {
                entries = new List<TokenEntry>();
                _index[token] = entries;
            }
            for (int k = 0; k < entries.Count; k++)
            {
                if (entries[k].PoiIndex != poiIndex)
                    continue;
                if (rank > entries[k].Rank)
                    entries[k] = new TokenEntry { PoiIndex = poiIndex, Rank = rank, Source = source };
                return;
            }
            entries.Add(new TokenEntry { PoiIndex = poiIndex, Rank = rank, Source = source });
        }

        // Rank every POI against a query. candidatePoiIds narrows the search to a filtered set: null =
        // every POI, an empty set = nothing (a filter that matches nothing leaves nothing to search).
        // An empty query returns the candidates themselves in config order (a filter with no text shows
        // its set), or nothing when there are no candidates. Results: best score first, then config order.
        public List<SearchResult> Search(string query, SearchOptions options, ICollection<string> candidatePoiIds = null)
        {
            var results = new List<SearchResult>();
            if (_pois.Count == 0)
                return results;

            var queryTokens = SearchTokenizer.Tokenize(query);
            if (queryTokens.Count == 0)
            {
                if (candidatePoiIds != null)
                    for (int i = 0; i < _pois.Count; i++)
                        if (candidatePoiIds.Contains(_pois[i].id))
                            results.Add(new SearchResult(_pois[i], 0f, ""));
                return results;
            }

            // per POI: its best score over the query words, why, and how many words it matched
            var best = new Dictionary<int, (float score, string reason)>();
            var matchedWords = new Dictionary<int, int>();

            foreach (string word in queryTokens)
            {
                var wordBest = new Dictionary<int, (float score, string reason)>();
                foreach (var kvp in _index)
                {
                    float factor = MatchFactor(word, kvp.Key, kvp.Value, options, out MatchKind kind);
                    if (factor <= 0f)
                        continue;
                    foreach (var e in kvp.Value)
                    {
                        if (kind == MatchKind.Prefix && options.Prefix == SearchPrefixScope.NameOnly && e.Source != MatchSource.Name)
                            continue;
                        if (candidatePoiIds != null && !candidatePoiIds.Contains(_pois[e.PoiIndex].id))
                            continue;
                        float score = e.Rank * factor;
                        if (!wordBest.TryGetValue(e.PoiIndex, out var cur) || score > cur.score)
                            wordBest[e.PoiIndex] = (score, Describe(e.Source, kind));
                    }
                }

                foreach (var kvp in wordBest)
                {
                    matchedWords[kvp.Key] = matchedWords.TryGetValue(kvp.Key, out int n) ? n + 1 : 1;
                    if (!best.TryGetValue(kvp.Key, out var cur) || kvp.Value.score > cur.score)
                        best[kvp.Key] = kvp.Value;
                }
            }

            foreach (var kvp in best)
            {
                if (options.MatchMode == SearchMatchMode.All && matchedWords[kvp.Key] < queryTokens.Count)
                    continue;
                results.Add(new SearchResult(_pois[kvp.Key], kvp.Value.score, kvp.Value.reason));
            }

            results.Sort((a, b) =>
            {
                int byScore = b.Score.CompareTo(a.Score);
                return byScore != 0 ? byScore : _indexById[a.PoiId].CompareTo(_indexById[b.PoiId]);
            });
            return results;
        }

        // How well a query word meets one indexed word: 1 exact, PrefixFactor when the query word starts
        // it, a typo factor within the allowed edits, 0 otherwise.
        private static float MatchFactor(string word, string token, List<TokenEntry> entries, SearchOptions options, out MatchKind kind)
        {
            kind = MatchKind.Exact;
            if (token == word)
                return 1f;

            if (options.Prefix != SearchPrefixScope.Off && word.Length >= MinPrefixLength
                && token.Length > word.Length && token.StartsWith(word, StringComparison.Ordinal))
            {
                kind = MatchKind.Prefix;
                return PrefixFactor;
            }

            int allowed = AllowedTypos(word, options.MaxTypoEdits);
            if (allowed > 0 && Math.Abs(token.Length - word.Length) <= allowed)
            {
                int edits = SearchTextDistance.Bounded(word, token, allowed);
                if (edits > 0 && edits <= allowed)
                {
                    kind = MatchKind.Typo;
                    return edits == 1 ? OneTypoFactor : TwoTypoFactor;
                }
            }
            return 0f;
        }

        // Typos forgiven for a query word of this length, never more than the setting allows
        public static int AllowedTypos(string word, int maxTypoEdits)
        {
            int byLength = word.Length >= MinLengthForTwoTypos ? 2 : word.Length >= MinLengthForOneTypo ? 1 : 0;
            return Math.Min(byLength, maxTypoEdits);
        }

        private static string Describe(MatchSource source, MatchKind kind)
        {
            string s = source switch
            {
                MatchSource.Name => "name",
                MatchSource.Keyword => "keyword",
                MatchSource.Synonym => "synonym",
                MatchSource.Summary => "summary",
                _ => "taxonomy",
            };
            return kind switch
            {
                MatchKind.Prefix => s + ", partial word",
                MatchKind.Typo => s + ", typo",
                _ => s,
            };
        }

        // Drop the whole index
        public void Clear()
        {
            _index.Clear();
            _pois.Clear();
            _indexById.Clear();
        }

        // One ranked match: the POI, its score (0..1) and a short reason ("keyword, typo")
        public readonly struct SearchResult
        {
            public readonly POIData Poi;
            public readonly float Score;
            public readonly string Reason;

            public string PoiId => Poi?.id;

            public SearchResult(POIData poi, float score, string reason)
            {
                Poi = poi;
                Score = score;
                Reason = reason;
            }
        }
    }
}
