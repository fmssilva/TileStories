using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Device-local, recency-ordered history of recent search queries, persisted
    // via PlayerPrefs as a small JSON blob (spec _2.6 section 13). Plain C# class
    // -- no MonoBehaviour, no scene, so it is directly unit-testable.
    public sealed class RecentSearchesManager
    {
        // PlayerPrefs key for the persisted recent-searches blob.
        public const string PREFS_KEY = "TileStories.recent_searches";

        // Cap on stored queries; defaults to WallConfigData.recent_search_count (5).
        private readonly int _maxCount;

        // Recency-ordered: index 0 is the most recent query.
        private readonly List<string> _entries = new List<string>();

        public RecentSearchesManager(int maxCount = 5)
        {
            _maxCount = Math.Max(1, maxCount);
            Load();
        }

        public IReadOnlyList<string> Entries => _entries;

        // Add a query to the top of the list, deduping any prior occurrence and
        // trimming to _maxCount. No-ops on null/empty/whitespace queries.
        public void Add(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return;

            _entries.RemoveAll(q => q == query);
            _entries.Insert(0, query.Trim());

            TrimToMax();
            Save();
        }

        // Drop all recent searches.
        public void Clear()
        {
            _entries.Clear();
            Save();
        }

        // Re-read persisted entries from PlayerPrefs. Called once at construction.
        private void Load()
        {
            _entries.Clear();
            string json = PlayerPrefs.GetString(PREFS_KEY, string.Empty);
            if (string.IsNullOrEmpty(json))
                return;

            try
            {
                var payload = JsonUtility.FromJson<Payload>(json);
                if (payload?.items != null)
                {
                    foreach (string item in payload.items)
                    {
                        if (!string.IsNullOrWhiteSpace(item) && !_entries.Contains(item))
                            _entries.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                // Corrupted blob: don't crash the app, start clean.
                Debug.LogWarning($"[Search] failed to parse recent searches JSON: {e.Message}");
                _entries.Clear();
            }

            TrimToMax();
        }

        // Persist entries to PlayerPrefs as a JSON array wrapper.
        private void Save()
        {
            var payload = new Payload { items = _entries.ToArray() };
            PlayerPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(payload));
            PlayerPrefs.Save();
        }

        // Enforce the recency-count cap from the newest end.
        private void TrimToMax()
        {
            while (_entries.Count > _maxCount)
                _entries.RemoveAt(_entries.Count - 1);
        }

        // JsonUtility can't serialize a bare string list, so wrap it.
        [Serializable]
        private class Payload
        {
            public string[] items;
        }
    }
}
