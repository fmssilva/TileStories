using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // A theme's values in one mode, for C# (_3.3 R12.1): the card's family colours today (CardTokenColours), the markers' and the
    // materials' colours later. Reads the SAME data the theme stylesheet is generated from, so C# and USS can never disagree -- and
    // never reads a stylesheet (a token moved up to the app root is not in the card's customStyle, R8 S1). Plain C#: built by
    // ThemeAsset.ResolverFor; a mode's value wins over a shared one of the same name.
    public sealed class ThemeResolver
    {
        private readonly Dictionary<string, Color> _colours = new();

        public ThemeResolver(ThemeTokenSet shared, ThemeTokenSet mode)
        {
            Add(shared);
            Add(mode);
        }

        private void Add(ThemeTokenSet set)
        {
            if (set == null) return;
            foreach (var token in set.colours)
                if (!string.IsNullOrEmpty(token.name)) _colours[token.name] = token.value;
        }

        // The colour `token` ("--ts-accent") has in this mode; false when the theme holds no such colour
        public bool TryColour(string token, out Color colour)
        {
            colour = default;
            return !string.IsNullOrEmpty(token) && _colours.TryGetValue(token, out colour);
        }
    }
}
