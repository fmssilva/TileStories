using System;
using System.Collections.Generic;

namespace TileStories
{
    // The visitor words an APP adds to the card (_3.1 step 11-fix): one CardStringTable per app, registered under the app's name the
    // same way an app registers its block kinds (BlockRegistry.Shared) and its services (CardServices.Shared), so a kind an app ships
    // can name its own things ("2 euro coin") in every language with no edit to the Framework's table. CardStrings reads the rows
    // between the wall's own wording and the framework's; the POI Editor's Card Texts lists them under the app's name so a wall can
    // still reword any of them. A key is the app's alone: one app's key never repeats another's.
    public sealed class CardStringSources
    {
        // One app's table under the app's name (what Card Texts groups its rows under)
        public sealed class Source
        {
            public string AppName { get; }
            public CardStringTable Table { get; }

            public Source(string appName, CardStringTable table)
            {
                AppName = appName;
                Table = table;
            }
        }

        private readonly List<Source> _sources = new();

        // The app's tables: what the wall's card host and the gallery harness read by default. Tests build their own.
        public static CardStringSources Shared { get; } = new();

        // Every registered table, in registration order
        public IReadOnlyList<Source> All => _sources;

        public bool Has(string appName) => Find(appName) != null;

        // Register an app's table. Refused: no name, no table, a name already registered (a silent replace would hide a clash), or a
        // key another app's table already holds (two apps would fight over one text).
        public void Add(string appName, CardStringTable table)
        {
            if (string.IsNullOrWhiteSpace(appName)) throw new ArgumentException("[Blocks] an app string table needs the app's name", nameof(appName));
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (Has(appName)) throw new ArgumentException("[Blocks] the app '" + appName + "' already registered its card strings");
            foreach (var row in table.rows)
            {
                if (row == null || string.IsNullOrEmpty(row.key)) continue;
                foreach (var other in _sources)
                    if (other.Table.RowOf(row.key) != null)
                        throw new ArgumentException("[Blocks] the card text '" + row.key + "' of '" + appName + "' is already held by '" + other.AppName + "'");
            }
            _sources.Add(new Source(appName, table));
        }

        // Forget one app's table; false when there was none (a test cleaning up after itself)
        public bool Remove(string appName)
        {
            var source = Find(appName);
            return source != null && _sources.Remove(source);
        }

        // Every app row as the CardStrings lookup reads them (empty when no app registered a table)
        public List<CardStringEntry> Entries()
        {
            var all = new List<CardStringEntry>();
            foreach (var source in _sources) all.AddRange(source.Table.Entries());
            return all;
        }

        private Source Find(string appName) => _sources.Find(s => s.AppName == appName);
    }
}
