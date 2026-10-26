using UnityEngine.UIElements;

namespace TileStories
{
    // An element row's closed style options as classes on its component (_3.3 R14.2): ts-el--tone-<tone>, ts-el--size-<size>,
    // ts-el--align-<align>, a media row's ts-el--width-<inset | full-bleed> and ts-el--frame-<rectangle | arch>, and ts-el--preview on a
    // block's preview -- Components.uss gives each its look from theme tokens. The Family
    // tone's colour is the BLOCK's family, so it cannot be a class: it is painted by name (NavTint, repainted on a mode switch).
    public static class ElementStyle
    {
        public const string RootClass = "ts-el";
        public const string PreviewClass = "ts-el--preview";
        // The class a text that speaks in the display face wears (one of the theme's display selectors)
        public const string DisplayClass = "ts-el--display";

        // Every element root: its own kind's class and the shared one
        public static void Init(VisualElement root, string kind)
        {
            root.AddToClassList(RootClass);
            root.AddToClassList(RootClass + "-" + kind.Replace('_', '-'));
        }

        // The size a row is drawn at: its own, or the smallest in a compact (Automatic) preview
        public static string SizeOf(ElementRow row, ElementBindContext context) =>
            context.Compact ? BuiltInElements.SizeSmall : context.StyleOf(BuiltInElements.SizeStyle, row);

        // Put `row`'s style options on `root` as classes (only the options its element has); the classes a previous bind left are cleared first
        public static void Apply(VisualElement root, ElementRow row, ElementBindContext context)
        {
            Clear(root);
            root.EnableInClassList(PreviewClass, context.Surface == ElementSurface.Preview);
            if (context.Element?.Field(BuiltInElements.ToneStyle) != null)
                root.AddToClassList(RootClass + "--tone-" + context.StyleOf(BuiltInElements.ToneStyle, row));
            if (context.Element?.Field(BuiltInElements.SizeStyle) != null)
                root.AddToClassList(RootClass + "--size-" + SizeOf(row, context));
            if (context.Element?.Field(BuiltInElements.AlignStyle) != null)
                root.AddToClassList(RootClass + "--align-" + context.StyleOf(BuiltInElements.AlignStyle, row));
            if (context.Element?.Field(BuiltInElements.WidthStyle) != null)
            {
                bool fullBleed = context.StyleOf(BuiltInElements.WidthStyle, row) == BuiltInElements.WidthFullBleed;
                root.AddToClassList(RootClass + "--width-" + (fullBleed ? "full-bleed" : "inset"));
                // - on the card, edge to edge is the block frame's inner edge (its own bleed); on the page, the page's (Components.uss)
                root.EnableInClassList(BlockFrame.BleedClass, fullBleed);
            }
            if (context.Element?.Field(BuiltInElements.FrameStyle) != null)
                root.AddToClassList(RootClass + "--frame-" + context.StyleOf(BuiltInElements.FrameStyle, row));
        }

        // Whether a media row's frame is the arch
        public static bool IsArch(ElementRow row, ElementBindContext context) =>
            context.Element?.Field(BuiltInElements.FrameStyle) != null && context.StyleOf(BuiltInElements.FrameStyle, row) == BuiltInElements.FrameArch;

        // A component's line height (its own --ts-el-leading token, percent of the type size; 0 = the face's own): USS has no line height,
        // so the component reads its token and the text carries it (WithLeading)
        public static readonly CustomStyleProperty<float> LeadingProperty = new("--ts-el-leading");

        public static float LeadingOf(VisualElement root) => root.customStyle.TryGetValue(LeadingProperty, out float percent) ? percent : 0f;

        // `richText` with its lines `percent` of the type size apart (TextCore's rich-text line-height); unchanged for 0 or no text
        public static string WithLeading(string richText, float percent) =>
            percent > 0f && !string.IsNullOrEmpty(richText)
                ? "<line-height=" + percent.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "%>" + richText
                : richText ?? "";

        // Small capitals' words (an eyebrow, a callout's title): written in sentence case, drawn in capitals by the rules of their language (a
        // Portuguese c-cedilla keeps its cedilla); an unknown code takes the invariant rules
        public static string Capitals(string words, string language)
        {
            if (string.IsNullOrEmpty(words)) return "";
            System.Globalization.CultureInfo culture;
            try { culture = string.IsNullOrEmpty(language) ? System.Globalization.CultureInfo.InvariantCulture : System.Globalization.CultureInfo.GetCultureInfo(language); }
            catch (System.Globalization.CultureNotFoundException) { culture = System.Globalization.CultureInfo.InvariantCulture; }
            return words.ToUpper(culture);
        }

        // Whether the row is drawn in its block family's colour
        public static bool IsFamilyTone(ElementRow row, ElementBindContext context) =>
            context.Element?.Field(BuiltInElements.ToneStyle) != null && context.StyleOf(BuiltInElements.ToneStyle, row) == BuiltInElements.ToneFamily;

        private static void Clear(VisualElement root)
        {
            var stale = new System.Collections.Generic.List<string>();
            foreach (string c in root.GetClasses())
                if (c.StartsWith(RootClass + "--tone-") || c.StartsWith(RootClass + "--size-") || c.StartsWith(RootClass + "--align-")
                    || c.StartsWith(RootClass + "--width-") || c.StartsWith(RootClass + "--frame-")) stale.Add(c);
            foreach (string c in stale) root.RemoveFromClassList(c);
        }
    }
}
