using System;
using System.Collections.Generic;

namespace TileStories
{
    // One facet group a visitor can filter by (spec _2.6 section 7): the four taxonomy tables, plus one group
    // per Keyword Field the developer ticked "Filter" on (its values = the keywords the POIs hold in it).
    // A small value type compared by Id; Id is also the chip's element-name part ("facet-Category-religious").
    public readonly struct FacetGroup : IEquatable<FacetGroup>
    {
        public readonly string Id;
        // The Keyword Field's key for a field group, null for a taxonomy group
        public readonly string FieldKey;

        private FacetGroup(string id, string fieldKey)
        {
            Id = id;
            FieldKey = fieldKey;
        }

        public static readonly FacetGroup Category = new("Category", null);
        public static readonly FacetGroup Badge = new("Badge", null);
        public static readonly FacetGroup Status = new("Status", null);
        public static readonly FacetGroup Hierarchy = new("Hierarchy", null);

        // The group of one filterable Keyword Field
        public static FacetGroup Field(string fieldKey) => new("Field_" + fieldKey, fieldKey);

        public bool IsField => FieldKey != null;

        public bool Equals(FacetGroup other) => Id == other.Id;
        public override bool Equals(object obj) => obj is FacetGroup g && Equals(g);
        public override int GetHashCode() => Id?.GetHashCode() ?? 0;
        public static bool operator ==(FacetGroup a, FacetGroup b) => a.Equals(b);
        public static bool operator !=(FacetGroup a, FacetGroup b) => !a.Equals(b);
        public override string ToString() => Id;
    }

    // The visitor's active filter values, one set of keys per facet group
    public sealed class FacetSelection
    {
        private static readonly HashSet<string> None = new();
        private readonly Dictionary<FacetGroup, HashSet<string>> _active = new();

        public IReadOnlyCollection<string> Active(FacetGroup group) => _active.TryGetValue(group, out var set) ? set : None;
        public bool IsActive(FacetGroup group, string key) => _active.TryGetValue(group, out var set) && set.Contains(key);

        // Every group with at least one active value, in the order they were first switched on
        public IEnumerable<FacetGroup> ActiveGroups
        {
            get
            {
                foreach (var kvp in _active)
                    if (kvp.Value.Count > 0) yield return kvp.Key;
            }
        }

        // Turn one filter value on or off; true when that changed anything
        public bool Set(FacetGroup group, string key, bool on)
        {
            if (!_active.TryGetValue(group, out var set))
            {
                if (!on) return false;
                _active[group] = set = new HashSet<string>();
            }
            return on ? set.Add(key) : set.Remove(key);
        }

        public void Clear() => _active.Clear();

        public int Count
        {
            get
            {
                int n = 0;
                foreach (var set in _active.Values) n += set.Count;
                return n;
            }
        }

        public bool Any => Count > 0;

        // A copy with one value removed (for the relax suggestion)
        public FacetSelection Without(FacetGroup group, string key)
        {
            var copy = new FacetSelection();
            foreach (var kvp in _active)
                foreach (var k in kvp.Value)
                    if (kvp.Key != group || k != key) copy.Set(kvp.Key, k, true);
            return copy;
        }
    }

    // One filter the visitor could drop to get results back, and how many POIs that would give
    public readonly struct RelaxSuggestion
    {
        public readonly FacetGroup Group;
        public readonly string Key;
        public readonly int ResultCount;

        public RelaxSuggestion(FacetGroup group, string key, int resultCount)
        {
            Group = group;
            Key = key;
            ResultCount = resultCount;
        }
    }

    // Pure facet filtering (spec _2.6 section 7): OR within a group (Religious OR Military), AND across
    // groups (Religious AND Destroyed), an empty group filters nothing.
    public static class FilterFacetEvaluator
    {
        public static bool PoiPasses(POIData poi, FacetSelection facets)
        {
            if (poi == null) return false;
            foreach (var group in facets.ActiveGroups)
                if (!Passes(poi, facets, group)) return false;
            return true;
        }

        private static bool Passes(POIData poi, FacetSelection facets, FacetGroup group)
        {
            if (group.IsField)
            {
                // - a keyword field: the POI passes with ANY of its keywords in that field ticked
                var field = poi.search_keyword_fields?.Find(f => f != null && f.field_key == group.FieldKey);
                if (field?.keywords != null)
                    foreach (var keyword in field.keywords)
                        if (facets.IsActive(group, FieldValueKey(keyword))) return true;
                return false;
            }
            string value = group == FacetGroup.Category ? poi.category
                : group == FacetGroup.Badge ? poi.badge_category
                : group == FacetGroup.Status ? (poi.has_status ? poi.status_level_key : null)
                : group == FacetGroup.Hierarchy ? poi.hierarchy_level_key
                : null;
            return !string.IsNullOrEmpty(value) && facets.IsActive(group, value);
        }

        // A keyword field's filter value key: 'Granite ' and 'granite' are one chip
        public static string FieldValueKey(string keyword) => (keyword ?? "").Trim().ToLowerInvariant();

        // The ids of every POI that passes the active filters
        public static HashSet<string> CandidateIds(IReadOnlyList<POIData> pois, FacetSelection facets)
        {
            var ids = new HashSet<string>();
            if (pois == null) return ids;
            foreach (var poi in pois)
                if (PoiPasses(poi, facets)) ids.Add(poi.id);
            return ids;
        }

        // With two or more filters active, the single filter whose removal gives the most results
        // (`countResults` counts the results of a filter set: the caller applies the query too).
        // Null when fewer than two are active or no removal gives any result.
        public static RelaxSuggestion? ComputeRelaxSuggestion(FacetSelection facets,
            Func<FacetSelection, int> countResults)
        {
            if (facets.Count < 2) return null;
            RelaxSuggestion? best = null;
            foreach (var group in new List<FacetGroup>(facets.ActiveGroups))
            {
                foreach (var key in new List<string>(facets.Active(group)))
                {
                    int n = countResults(facets.Without(group, key));
                    if (n > 0 && (best == null || n > best.Value.ResultCount))
                        best = new RelaxSuggestion(group, key, n);
                }
            }
            return best;
        }
    }
}
