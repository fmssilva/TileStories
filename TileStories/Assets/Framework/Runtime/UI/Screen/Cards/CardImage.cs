using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // One picture on the card (_3.1 step 7): a frame (Root, clipped), the picture inside it (Picture, a background image)
    // and the "picture unavailable" words shown instead when the file cannot be shown. Every picture of every Tier 2 kind
    // is one of these. It loads LAZILY in Show through the block's own media scope (the stack releases that scope when the
    // block is unbound; the full-screen view has a scope of its own), and only a path MediaPathRule accepts. How the
    // picture fills its frame (cover / fit), and every size, is USS (Media.uss): here only classes.
    public sealed class CardImage
    {
        public VisualElement Root { get; }
        public VisualElement Picture { get; }
        public Label Unavailable { get; }

        // The loaded picture (null: none, or it could not be loaded)
        public Texture2D Texture { get; private set; }
        public string Path { get; private set; } = "";
        // width / height of the loaded picture (1 when none)
        public float Aspect => Texture != null && Texture.height > 0 ? (float)Texture.width / Texture.height : 1f;

        public CardImage(string modifierClass = null)
        {
            Root = new VisualElement();
            Root.AddToClassList("card-image");
            if (modifierClass != null) Root.AddToClassList(modifierClass);
            Picture = new VisualElement { pickingMode = PickingMode.Ignore };
            Picture.AddToClassList("card-image__picture");
            Unavailable = new Label { pickingMode = PickingMode.Ignore };
            Unavailable.AddToClassList("card-image__unavailable");
            Root.Add(Picture);
            Root.Add(Unavailable);
            Clear(null);
        }

        // Show the picture at `path` (a path inside the wall's media folder) loaded through `media`; a path the rule
        // refuses, or a file that is not there, shows the unavailable words. Returns whether a picture shows.
        public bool Show(IMediaSource media, string path, CardStrings strings)
        {
            Path = (path ?? "").Trim();
            Texture = MediaPathRule.IsValid(Path, MediaKind.Image) ? media?.Load<Texture2D>(Path) : null;
            if (Texture == null)
            {
                Clear(strings);
                return false;
            }
            Picture.style.backgroundImage = new StyleBackground(Texture);
            Root.RemoveFromClassList("card-image--unavailable");
            Unavailable.text = "";
            return true;
        }

        // Forget the picture (the media scope gives the file back) and show the unavailable state
        public void Clear(CardStrings strings)
        {
            Texture = null;
            Picture.style.backgroundImage = StyleKeyword.None;
            Root.AddToClassList("card-image--unavailable");
            Unavailable.text = strings?.Get(CardStrings.Keys.MediaUnavailable) ?? "";
        }
    }
}
