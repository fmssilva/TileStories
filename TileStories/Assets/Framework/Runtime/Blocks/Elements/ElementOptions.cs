namespace TileStories
{
    // Every option string the composed blocks' data shares (_3.3 R14, R17), for the runtime and the Editor dropdowns: the same shape as
    // CardOptions, kept beside the element code. In each list the FIRST value is the default, also read for an unknown stored value.
    // An element's STYLE options are its own (ElementKindDefinition.StyleFields), not listed here.
    public static class ElementOptions
    {
        // A block's Preview (BlockSurfaces.PreviewModeField, R17.1): derived from the Page, or its own row list
        public const string PreviewAutomatic = "automatic";
        public const string PreviewCustom = "custom";
        public static readonly string[] PreviewModes = { PreviewAutomatic, PreviewCustom };
        public static readonly string[] PreviewModeLabels = { "Automatic", "Custom" };

        // A row's motion effect (ElementRow.Motion, R17.3). Auto = the element's own default (ElementKindDefinition.AutoMotion), so a
        // developer only touches the exceptions. What Reduce Motion turns each into is a rule, never a choice (ElementMotionRule)
        public const string MotionAuto = "auto";
        public const string MotionNone = "none";
        public const string MotionReveal = "reveal";
        public const string MotionSlideIn = "slide_in";
        public const string MotionParallax = "parallax";
        public const string MotionSlowZoom = "slow_zoom";
        public const string MotionPan = "pan";
        public static readonly string[] Motions = { MotionAuto, MotionNone, MotionReveal, MotionSlideIn, MotionParallax, MotionSlowZoom, MotionPan };
        public static readonly string[] MotionLabels = { "Auto", "None", "Reveal", "Slide In", "Parallax", "Slow Zoom", "Pan" };

        // How strongly a row's motion plays (ElementRow.Strength)
        public const string StrengthSubtle = "subtle";
        public const string StrengthStrong = "strong";
        public static readonly string[] Strengths = { StrengthSubtle, StrengthStrong };
        public static readonly string[] StrengthLabels = { "Subtle", "Strong" };

        // What a tap on an element does in the page (ElementRow.Tap, R17.4), offered only where an element has a choice
        // (ElementKindDefinition.TapChoices: its first is its default)
        public const string TapNone = "none";
        public const string TapOpenViewer = "open_viewer";
        public const string TapPlayInPlace = "play_in_place";
        public const string TapOpenPlayer = "open_player";
        public static readonly string[] Taps = { TapNone, TapOpenViewer, TapPlayInPlace, TapOpenPlayer };
        public static readonly string[] TapLabels = { "Nothing", "Open The Viewer", "Play In Place", "Open The Player" };

        // Where a reference row points (ElementRef.Scope, R17.8): this block's Page, another block of the same POI, the POI's shared pool
        public const string ScopePage = "page";
        public const string ScopeBlock = "block";
        public const string ScopeShared = "shared";
        public static readonly string[] Scopes = { ScopePage, ScopeBlock, ScopeShared };
        public static readonly string[] ScopeLabels = { "This Page", "Another Block", "Shared" };

        // The shared chrome components a block may override (BlockSurfaces.ComponentOverridesField, R17.5); the Open Row first
        public const string ComponentOpenRow = "open_row";
        public static readonly string[] Components = { ComponentOpenRow };
        public static readonly string[] ComponentLabels = { "Open Row" };

        // The Open Row's emphasis (R14.8): plain, the chevron nudges, a light sweeps across the words. Reduce Motion: plain
        public const string EmphasisPlain = "plain";
        public const string EmphasisNudge = "nudge";
        public const string EmphasisShine = "shine";
        public static readonly string[] OpenRowEmphases = { EmphasisPlain, EmphasisNudge, EmphasisShine };
        public static readonly string[] OpenRowEmphasisLabels = { "Plain", "Nudge", "Shine" };

        // The first value of `options` when `value` is not one of them (the default an unknown stored value reads as)
        public static string OneOf(string value, string[] options)
        {
            if (value != null)
                foreach (string option in options)
                    if (option == value) return value;
            return options[0];
        }
    }
}
