using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The framework's default wording of every card UI text (CardStrings.Keys), one row per key with its text in
    // each framework language and a note on where the card shows it (the POI Editor's Card Texts (i)). The asset
    // CardStrings.asset sits next to PoiCard.uss and is wired into PoiCardHost like the style sheets; a wall
    // rewords a text in card_settings.strings, never here.
    [CreateAssetMenu(fileName = "CardStrings", menuName = "TileStories/Card String Table")]
    public sealed class CardStringTable : ScriptableObject
    {
        [System.Serializable]
        public sealed class Row
        {
            public string key;
            [Tooltip("Where the card shows this text (the POI Editor's Card Texts help)")]
            public string where;
            public List<LocalizedEntry> text = new();
        }

        public List<Row> rows = new();

        // The rows as the CardStrings lookup reads them
        public List<CardStringEntry> Entries()
        {
            var list = new List<CardStringEntry>(rows.Count);
            foreach (var row in rows)
                if (row != null) list.Add(new CardStringEntry { key = row.key, text = row.text });
            return list;
        }

        public Row RowOf(string key) => rows.Find(r => r != null && r.key == key);
    }
}
