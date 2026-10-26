using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The full-screen picture viewer (_3.1 step 7's lightbox, shared since _3.3 S2b): one page per picture, the picture fitted to the screen
    // -- a swipe sideways turns the page, two fingers look closer (ZoomPanSurface, as zoom_image) -- its caption and credit under it. The
    // gallery block and the story's picture elements open it through the host they are bound with (IBlockHost.OpenTakeover); it draws into
    // the plain container the full view hands it and never names the card's sheet (R15.2). Looks: Media.uss (card-gallery__full*).
    public static class PictureViewer
    {
        // One picture of the viewer: its file (a path the media folder holds), its caption and its credit (either may be empty)
        public readonly struct Picture
        {
            public readonly string Path, Caption, Credit;

            public Picture(string path, string caption, string credit)
            {
                Path = path ?? "";
                Caption = caption ?? "";
                Credit = credit ?? "";
            }
        }

        // Open `pictures` full screen at `start`, the breadcrumb ending with `name`; nothing without a host or a picture
        public static void Open(IBlockHost host, string name, IReadOnlyList<Picture> pictures, int start, CardStrings strings)
        {
            if (host == null || pictures == null || pictures.Count == 0) return;
            var shown = new List<Picture>(pictures);
            host.OpenTakeover(name ?? "", shown.Count, start, (view, container, media, index) => DrawPage(view, container, media, strings, shown[index]));
        }

        // One page: the picture on its zoom / pan surface, a sideways swipe turning the view's page, then its caption and credit
        public static void DrawPage(ITakeoverPage view, VisualElement container, IMediaSource media, CardStrings strings, Picture picture)
        {
            var surface = new ZoomPanSurface("card-gallery__full", "card-gallery__full-picture") { Swipes = true };
            surface.Frame.name = "card-gallery-full";
            surface.Swiped += view.Turn;
            container.Add(surface.Frame);
            surface.Show(media, picture.Path, strings);
            foreach (var (text, cls) in new[] { (picture.Caption, "card-gallery__full-caption"), (picture.Credit, "card-gallery__full-credit") })
            {
                if (text.Length == 0) continue;
                var label = new Label(text);
                label.AddToClassList(cls);
                container.Add(label);
            }
        }
    }
}
