using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using FontAsset = UnityEngine.TextCore.Text.FontAsset;

namespace TileStories.Editor
{
    // The hero's title measured as a phone shows it (_3.3 P4.4b part 2): the default theme's DISPLAY face (its TextCore asset, the one
    // UI Toolkit draws with), at --ts-size-hero-narrow (a phone is narrower than HeroRule.NarrowBelow), on a line as wide as the panel's
    // reference width (PanelSettings, the 390 pt portrait phone) less the hero foot's side padding (--ts-space-5 each side, Media.uss).
    // Feeds HeroTitleFitRule for Card Content's 3-line notice. Glyph advances only (no kerning): PoiCardHeroTitleFitSceneTests checks the
    // count against the real hero label.
    public static class HeroTitleMeasure
    {
        public const string ThemePath = "Assets/Framework/Runtime/Resources/TileStories/Themes/Theme.default.asset";
        public const string PanelSettingsPath = "Assets/Framework/Runtime/UI/Shared/PanelSettings.asset";
        public const string SizeToken = "--ts-size-hero-narrow";
        public const string FootPaddingToken = "--ts-space-5";

        // - Card Content asks on every OnGUI pass: a title is measured once per domain (an edit of the theme's display face or hero size
        //   reaches the notice after the next script reload)
        private static readonly System.Collections.Generic.Dictionary<string, int> Measured = new();

        // The lines `title` takes in a phone's hero; 0 when the theme, its display face or the panel cannot be found (no notice then)
        public static int Lines(string title)
        {
            title ??= "";
            if (Measured.TryGetValue(title, out int known)) return known;
            int lines = TryLine(out var font, out float size, out float width)
                ? HeroTitleFitRule.Lines(title, width, run => HeroTitleFitRule.AdvanceWidth(font, size, run)) : 0;
            if (Measured.Count > 512) Measured.Clear();
            Measured[title] = lines;
            return lines;
        }

        // The face, its size and the line's width, from their sources
        public static bool TryLine(out FontAsset font, out float size, out float width)
        {
            font = null;
            size = width = 0f;
            var theme = AssetDatabase.LoadAssetAtPath<ThemeAsset>(ThemePath);
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (theme == null || panel == null || theme.Type?.fontLibrary == null) return false;
            font = theme.Type.fontLibrary.GetUiFont(theme.Type.displayFont);
            float padding = Length(theme, FootPaddingToken);
            size = Length(theme, SizeToken);
            width = panel.referenceResolution.x - 2f * padding;
            return font != null && size > 0f && width > 0f;
        }

        // A length token of the theme in px (shared first, then the light mode's)
        private static float Length(ThemeAsset theme, string name)
        {
            foreach (var t in theme.Shared.lengths) if (t.name == name) return t.value.value;
            foreach (var mode in theme.Modes)
                foreach (var t in mode.tokens.lengths) if (t.name == name) return t.value.value;
            return 0f;
        }
    }
}
