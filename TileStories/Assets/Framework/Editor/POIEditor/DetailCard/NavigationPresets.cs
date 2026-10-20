// NavigationPresets.cs
//
// Detail Card > Navigation presets (_3.2 section 8): nine named sets of the seven rows that decide HOW a card is navigated
// (Layout, Top Bar, Bottom Bar, Floating Button, Menu, Edge, Auto Nav Block). A preset is DATA and a convenience: it writes
// ONLY those seven fields of card_settings.navigation and leaves every other field alone; it computes nothing at runtime
// (the card never learns a preset exists -- it reads the seven fields like any wall's). The Editor assembly owns it: a wall's
// config.json carries the seven values, never the preset's name.

using System.Collections.Generic;

namespace TileStories.Editor
{
    internal static class NavigationPresets
    {
        // One preset: the words the picker shows and the seven values it writes
        internal sealed class Preset
        {
            // The name the picker shows
            public readonly string Label;
            // Who it suits, in a few words (a wall author's words, never a screen size)
            public readonly string Suits;
            public readonly string Layout, Top, Bottom, Floating, Menu, Edge, AutoNavBlock;

            public Preset(string label, string suits, string layout, string top, string bottom, string floating, string menu, string edge,
                string autoNavBlock)
            {
                Label = label;
                Suits = suits;
                Layout = layout;
                Top = top;
                Bottom = bottom;
                Floating = floating;
                Menu = menu;
                Edge = edge;
                AutoNavBlock = autoNavBlock;
            }
        }

        // How many presets the picker lists first, above its separator: the strongest ones for most walls
        public const int Recommended = 4;

        // The JSON names of the fields a preset writes -- the ONLY fields a preset may change (a test walks the whole settings object)
        public static readonly IReadOnlyList<string> WrittenFields = new[]
        {
            "layout", "top", "bottom", "floating", "menu", "edge", "auto_nav_block",
        };

        private const string None = CardOptions.NavNone;

        // Section 8's table, strongest first: Long read, Rich point, One hand, Over the camera, then the rest
        public static readonly IReadOnlyList<Preset> All = new[]
        {
            new Preset("Long read", "A long card read from top to bottom, with a map of tabs under the title.",
                CardOptions.LayoutScroll, BuiltInNavElements.TextTabs, None, None, None, None, None),
            new Preset("Rich point", "A card with many kinds of content: icon tabs with chips, and a drawer with the whole map.",
                CardOptions.LayoutScroll, BuiltInNavElements.IconTabsChips, None, None, BuiltInNavElements.DrawerTree, None, None),
            new Preset("One hand", "A phone held in one hand: a bar and a menu where the thumb reaches.",
                CardOptions.LayoutScroll, None, BuiltInNavElements.BottomBar, None, BuiltInNavElements.GridOverlay, None, None),
            new Preset("Over the camera", "A card read over the live camera view: one round button, no bars.",
                CardOptions.LayoutScroll, None, None, BuiltInNavElements.FabDial, None, None, None),
            new Preset("Deep dive", "A very long card explored in depth: kind chips, a drawer, an edge scrubber and a contents list.",
                CardOptions.LayoutScroll, BuiltInNavElements.KindChips, None, None, BuiltInNavElements.DrawerTree, BuiltInNavElements.EdgeScrubber,
                BuiltInBlocks.NavContentsKind),
            new Preset("Guided", "A card read in order: a progress map, an Up Next bar and a contents list.",
                CardOptions.LayoutScroll, BuiltInNavElements.ProgressMap, BuiltInNavElements.NextUp, None, None, None, BuiltInBlocks.NavContentsKind),
            new Preset("Story", "A short card read section by section, turned like pages.",
                CardOptions.LayoutPages, None, BuiltInNavElements.NextUp, None, None, None, None),
            new Preset("Quick glance", "A visitor who skims and picks one or two things: folding lines and a round button.",
                CardOptions.LayoutAccordion, None, None, BuiltInNavElements.FabDial, None, None, None),
            new Preset("Overview", "A point explored by topic: the sections as tiles.",
                CardOptions.LayoutBento, None, None, None, None, None, None),
        };

        // Write the preset's seven fields into `nav`; nothing else is touched. Called inside ONE mutation scope, so it is one undo step
        public static void Apply(CardNavigationSettings nav, Preset preset)
        {
            nav.layout = preset.Layout;
            nav.top = preset.Top;
            nav.bottom = preset.Bottom;
            nav.floating = preset.Floating;
            nav.menu = preset.Menu;
            nav.edge = preset.Edge;
            nav.auto_nav_block = preset.AutoNavBlock;
        }

        // The preset whose seven values `nav` holds exactly, or null (the wall's rows are its own: "Custom")
        public static Preset Matching(CardNavigationSettings nav)
        {
            if (nav == null) return null;
            foreach (var preset in All)
                if (nav.layout == preset.Layout && nav.top == preset.Top && nav.bottom == preset.Bottom && nav.floating == preset.Floating
                    && nav.menu == preset.Menu && nav.edge == preset.Edge && nav.auto_nav_block == preset.AutoNavBlock)
                    return preset;
            return null;
        }
    }
}
