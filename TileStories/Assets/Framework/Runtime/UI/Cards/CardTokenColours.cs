using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // A card colour token read BY NAME from C# (_3.2 step 6): a family's ColorToken is data ("--ts-family-about"), so a view cannot name
    // it in a stylesheet rule of its own. USS custom properties have no C# setter and are only in the customStyle of the element that
    // declares them -- the card root (.ts-card), where CardTokens.uss puts every token -- so this reads them there, the way CardTrackFit
    // reads --ts-track-peek. The colour itself stays in the stylesheet.
    public static class CardTokenColours
    {
        // The colour `token` resolves to on the card `from` sits in; false when there is no card above it, its style is not resolved yet,
        // or no stylesheet defines the token
        public static bool TryResolve(VisualElement from, string token, out Color colour)
        {
            colour = default;
            if (string.IsNullOrEmpty(token)) return false;
            for (var e = from; e != null; e = e.parent)
                if (e.ClassListContains("ts-card")) return e.customStyle.TryGetValue(new CustomStyleProperty<Color>(token), out colour);
            return false;
        }
    }
}
