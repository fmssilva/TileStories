using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's ONE icon set (_3.1 Tier 1): small line icons drawn by CardParts.uss from four generic parts -- a frame (a
    // ring or a box), two bars and a dot -- inside an 18 px glyph box; each key's class shows and places the parts. No
    // picture and no glyph font: the icons follow the card's tokens (colour, stroke, size) like every other element.
    // Used by the actions buttons (their action's icon) and practical_info (a row's icon). A look may resize the icon's
    // root (the actions' circles) -- the glyph box keeps the drawing its own size, centred.
    public static class CardIcons
    {
        public const string ShowOnWall = "show_on_wall";
        public const string Time = "time";
        public const string Access = "access";
        public const string Location = "location";
        public const string Info = "info";
        public const string Ticket = "ticket";
        public const string Light = "light";

        // Every key the set draws
        public static readonly IReadOnlyList<string> All = new[] { ShowOnWall, Time, Access, Location, Info, Ticket, Light };

        // A new icon element (no key yet: SetKey picks the drawing)
        public static VisualElement Create()
        {
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("card-icon");
            var glyph = new VisualElement { pickingMode = PickingMode.Ignore };
            glyph.AddToClassList("card-icon__glyph");
            foreach (string part in new[] { "card-icon__frame", "card-icon__bar card-icon__bar--a", "card-icon__bar card-icon__bar--b", "card-icon__dot" })
            {
                var e = new VisualElement { pickingMode = PickingMode.Ignore };
                foreach (string c in part.Split(' ')) e.AddToClassList(c);
                glyph.Add(e);
            }
            icon.Add(glyph);
            return icon;
        }

        // Draw `key` on an icon made by Create (a key the set does not have draws nothing)
        public static void SetKey(VisualElement icon, string key)
        {
            foreach (string known in All)
                icon.EnableInClassList("card-icon--" + known, known == key);
        }

        // The key an icon draws now, or null
        public static string KeyOf(VisualElement icon)
        {
            foreach (string known in All)
                if (icon.ClassListContains("card-icon--" + known)) return known;
            return null;
        }
    }
}
