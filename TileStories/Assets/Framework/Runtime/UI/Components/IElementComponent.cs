using UnityEngine.UIElements;

namespace TileStories
{
    // Which surface of a composed block a row is drawn on (_3.3 R17.1): the block's own page, or its preview on the card
    public enum ElementSurface { Page, Preview }

    // What one element component is handed with its row (_3.3 R14.2): the block's bind context (languages, glossary, media scope, reduce
    // motion), the element's definition, the row whose STYLE applies -- a Custom preview's linked row keeps its own style over its Page row's
    // content (R17.1) -- the surface, and the family colour the block belongs to (a theme colour token, painted by name). Nothing here names
    // the sheet: a component draws on any panel (R15.2).
    public sealed class ElementBindContext
    {
        public BlockBindContext Block;
        public ElementKindDefinition Element;
        // The row holding the style values (a preview link row); null = the content row's own
        public BlockItemData StyleRow;
        public ElementSurface Surface;
        // An Automatic preview: every text at its smallest size, so the preview reads as a summary of the page (the prototype's preview)
        public bool Compact;
        // The block family's colour token ("--ts-family-about"): the Family tone's colour
        public string FamilyToken;

        public string Language => Block?.Language;
        public string Fallback => Block?.FallbackLanguage;

        // The style option `key` of `content`: the style row's value when it holds one the element offers, else the content row's, else
        // the element's default (a closed list: an unknown stored word reads as the default)
        public string StyleOf(string key, ElementRow content)
        {
            var field = Element?.Field(key);
            if (field?.Options == null) return "";
            foreach (var row in StyleRow != null ? new[] { new ElementRow(StyleRow), content } : new[] { content })
            {
                string value = row.Value(key);
                foreach (string option in field.Options)
                    if (option == value) return value;
            }
            return field.ChoiceDefault ?? field.Options[0];
        }
    }

    // A component that shows pictures in clipped frames (the media elements, _3.3 S2b): the pictures a row's parallax, slow zoom or pan moves
    // inside their frames (each one's parent is its frame); empty when nothing is shown
    public interface IFramedPictures
    {
        System.Collections.Generic.IReadOnlyList<VisualElement> Pictures { get; }
    }

    // One element kind's component (_3.3 R14.2): a VisualElement built once, then bound to a row and unbound as the block's rows change
    // (pooled per kind by ElementComponentPool -- never made and thrown away per bind). Styled only by theme tokens and its own --ts-el-*
    // tokens (Components.uss), every closed style option a class.
    public interface IElementComponent
    {
        VisualElement Root { get; }

        // Show this row (its content in the context's language, its style options as classes)
        void Bind(ElementRow row, ElementBindContext context);

        // Forget the row
        void Unbind();
    }
}
