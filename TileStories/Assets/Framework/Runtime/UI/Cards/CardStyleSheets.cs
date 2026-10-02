using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's stylesheets in the order they apply (_3.1 section 3, "Styles"): the --ts-* tokens, the container, then the blocks' sheets
    // (CardParts.uss, one per family, an app's own last). Both hosts use it -- the wall's PoiCardHost and the Phase A CardGalleryHarness -- so
    // the gallery card and the wall's card are styled by exactly the same list.
    public static class CardStyleSheets
    {
        // Every sheet of the card, tokens first
        public static IEnumerable<StyleSheet> InOrder(StyleSheet tokens, StyleSheet container, IEnumerable<StyleSheet> blockStyles)
        {
            yield return tokens;
            yield return container;
            if (blockStyles == null) yield break;
            foreach (var sheet in blockStyles) yield return sheet;
        }
    }
}
