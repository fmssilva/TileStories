using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // A card colour token read BY NAME from C# (_3.2 step 6): a family's ColorToken is data ("--ts-family-about"), so a view cannot name
    // it in a stylesheet rule of its own. Every colour is the app THEME's (_3.3 R12.1), so this asks the theme the element sits under
    // (ThemeController.TryResolverFor: the nearest .ts-app root's theme and mode) -- the very data its stylesheet is generated from.
    // Never a stylesheet read: the colours are declared on the app root, not in the card's customStyle (R8 S1).
    public static class CardTokenColours
    {
        // The colour `token` has in the theme `from` sits under; false when it sits under no marked root or the theme holds no such colour
        public static bool TryResolve(VisualElement from, string token, out Color colour)
        {
            colour = default;
            if (string.IsNullOrEmpty(token)) return false;
            return ThemeController.TryResolverFor(from, out var theme) && theme.TryColour(token, out colour);
        }
    }
}
