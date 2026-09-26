using System.Collections.Generic;

namespace TileStories
{
    // The ONE list of everything a POI can be found by (spec _2.6 section 4): the authoring tree -- its name,
    // its own keywords (Others + each Keyword Field), its summary, the taxonomy rows it belongs to (their
    // name / label and their Search Keywords) and the synonyms of all of those -- flattened into words with
    // their source and rank. POISearchIndex.Build indexes exactly this list, and the POI Editor shows it
    // ("Found by"), so the preview can never drift from what the search really does. Pure.
    public static class SearchKeywordSources
    {
        // One searchable text of a POI: what it is, where it came from (for people) and how it ranks
        public readonly struct Word
        {
            public readonly string Text;
            public readonly POISearchIndex.MatchSource Source;
            public readonly float Rank;
            // Where the word is authored: "Name", "Others", a keyword field's label, "Category", ...
            public readonly string Origin;

            public Word(string text, POISearchIndex.MatchSource source, float rank, string origin)
            {
                Text = text;
                Source = source;
                Rank = rank;
                Origin = origin;
            }
        }

        public const string OriginName = "Name";
        public const string OriginOthers = "Others";
        public const string OriginSummary = "Summary";
        public const string OriginCategory = "Category";
        public const string OriginBadge = "Badge";
        public const string OriginStatus = "Status";
        public const string OriginLevel = "Level";
        public const string OriginSynonym = "Synonyms";

        // Every word the search finds this POI by, in rank order of their sources
        public static List<Word> Collect(WallConfigData config, POIData poi)
        {
            var words = new List<Word>();
            if (poi == null) return words;

            Add(words, poi.name, POISearchIndex.MatchSource.Name, POISearchIndex.RankName, OriginName);
            AddOwnKeywords(words, config, poi);
            Add(words, poi.summary, POISearchIndex.MatchSource.Summary, POISearchIndex.RankSummary, OriginSummary);
            if (config != null) AddTaxonomy(words, config, poi);
            if (config?.synonym_groups != null) AddSynonyms(words, config.synonym_groups);
            return words;
        }

        // Others + one list per Keyword Field (labelled by the field's label when the wall defines it)
        private static void AddOwnKeywords(List<Word> words, WallConfigData config, POIData poi)
        {
            if (poi.search_keyword_fields != null)
                foreach (var field in poi.search_keyword_fields)
                {
                    if (field?.keywords == null) continue;
                    var def = config?.search_fields?.Find(f => f != null && f.key == field.field_key);
                    string origin = !string.IsNullOrWhiteSpace(def?.label) ? def.label : field.field_key;
                    foreach (var k in field.keywords)
                        Add(words, k, POISearchIndex.MatchSource.Keyword, POISearchIndex.RankKeyword, origin);
                }
            if (poi.search_keywords != null)
                foreach (var k in poi.search_keywords)
                    Add(words, k, POISearchIndex.MatchSource.Keyword, POISearchIndex.RankKeyword, OriginOthers);
        }

        // What a POI inherits from each taxonomy row it belongs to: the row's name / label and the row's own
        // Search Keywords. The outline row only while the POI has a status (without one it shows no ring).
        private static void AddTaxonomy(List<Word> words, WallConfigData config, POIData poi)
        {
            if (!string.IsNullOrEmpty(poi.category))
            {
                AddRow(words, poi.category, OriginCategory);
                var row = config.category_styles?.Find(e => e != null && e.category == poi.category);
                AddRowKeywords(words, row?.search_keywords, OriginCategory);
            }

            if (!string.IsNullOrEmpty(poi.badge_category))
            {
                var row = config.badge_categories?.Find(e => e != null && e.key == poi.badge_category);
                AddRow(words, string.IsNullOrEmpty(row?.label) ? poi.badge_category : row.label, OriginBadge);
                AddRowKeywords(words, row?.search_keywords, OriginBadge);
            }

            if (poi.has_status && !string.IsNullOrEmpty(poi.status_level_key))
            {
                var row = config.outline_levels?.Find(e => e != null && e.key == poi.status_level_key);
                AddRow(words, string.IsNullOrEmpty(row?.label) ? poi.status_level_key : row.label, OriginStatus);
                AddRowKeywords(words, row?.search_keywords, OriginStatus);
            }

            if (!string.IsNullOrEmpty(poi.hierarchy_level_key))
            {
                // - a level key that matches no row is not a word anyone reads: only a real row's name
                var row = config.hierarchy_levels?.Find(e => e != null && e.key == poi.hierarchy_level_key);
                if (row != null)
                {
                    AddRow(words, row.level_name, OriginLevel);
                    AddRowKeywords(words, row.search_keywords, OriginLevel);
                }
            }
        }

        private static void AddRow(List<Word> words, string text, string origin) =>
            Add(words, text, POISearchIndex.MatchSource.Taxonomy, POISearchIndex.RankTaxonomy, origin);

        private static void AddRowKeywords(List<Word> words, List<string> keywords, string origin)
        {
            if (keywords == null) return;
            foreach (var k in keywords) AddRow(words, k, origin);
        }

        // A synonym group is a set of words meaning the same thing: a POI holding any member also gets every
        // other member, at RankSynonym but never above the word it came from. Computed from the words collected
        // so far only -- a synonym never brings in its own synonyms (groups do not chain).
        private static void AddSynonyms(List<Word> words, List<SynonymGroup> groups)
        {
            var held = new Dictionary<string, float>();
            foreach (var w in words)
                foreach (var token in SearchTokenizer.Tokenize(w.Text))
                    if (!held.TryGetValue(token, out float r) || w.Rank > r) held[token] = w.Rank;

            foreach (var group in groups)
            {
                var members = Members(group);
                if (members.Count < 2) continue;
                float best = -1f;
                foreach (var m in members)
                    if (held.TryGetValue(m, out float r) && r > best) best = r;
                if (best < 0f) continue;
                float rank = System.Math.Min(best, POISearchIndex.RankSynonym);
                // - a member the POI already holds is added only when the synonym ranks it higher
                foreach (var m in members)
                    if (!held.TryGetValue(m, out float own) || own < rank)
                        words.Add(new Word(m, POISearchIndex.MatchSource.Synonym, rank, OriginSynonym));
            }
        }

        // A group's words, tokenised the way the search reads them (key + synonyms, no repeats)
        public static List<string> Members(SynonymGroup group)
        {
            var members = new List<string>();
            if (group == null) return members;
            members.AddRange(SearchTokenizer.Tokenize(group.key));
            if (group.synonyms != null)
                foreach (var synonym in group.synonyms)
                    foreach (var token in SearchTokenizer.Tokenize(synonym))
                        if (!members.Contains(token)) members.Add(token);
            return members;
        }

        private static void Add(List<Word> words, string text, POISearchIndex.MatchSource source, float rank, string origin)
        {
            if (!string.IsNullOrWhiteSpace(text)) words.Add(new Word(text.Trim(), source, rank, origin));
        }
    }
}
