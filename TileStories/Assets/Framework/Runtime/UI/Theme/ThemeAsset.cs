using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The app's look as DATA (_3.3 R12.1): the ONE source of every T1 / T2 value (R12.2) a screen draws with -- colours per mode, the type,
    // space and radius scales, motion. Two readers, one source: the Editor generates the theme USS from it (ThemeUssGenerator ->
    // Generated/Theme.<key>.uss, reached by every UI Toolkit panel through TileStoriesTheme.tss), and C# reads it through ThemeResolver
    // (never from a stylesheet: a custom property is only in the customStyle of the element that declares it, R8 S1). Loaded at runtime by
    // key from Resources/TileStories/Themes/Theme.<key> (ThemeController.Load).
    [CreateAssetMenu(fileName = "Theme.default", menuName = "TileStories/Theme")]
    public sealed class ThemeAsset : ScriptableObject
    {
        [Tooltip("The theme's key: its root class (ts-theme-<key>) and its generated stylesheet (Generated/Theme.<key>.uss)")]
        [SerializeField] private string key = "default";

        [Tooltip("The mode a screen opens in (ts-mode-<mode>) until a wall or a visitor picks another")]
        [SerializeField] private string defaultMode = "dark";

        [Tooltip("The values every mode shares: the type, space and radius scales, motion")]
        [SerializeField] private ThemeTokenSet shared = new();

        [Tooltip("One entry per mode (dark, light): that mode's colours")]
        [SerializeField] private List<ThemeMode> modes = new();

        [Tooltip("A wall's closed choices (accent, roundness, motion level, R12.4): each option a set of values laid over the theme's. Not generated yet (_3.3 P4.5)")]
        [SerializeField] private List<ThemeKnobOption> knobs = new();

        public string Key => key;
        public string DefaultMode => defaultMode;
        public ThemeTokenSet Shared => shared;
        public IReadOnlyList<ThemeMode> Modes => modes;
        public IReadOnlyList<ThemeKnobOption> Knobs => knobs;

        // Built once per mode on first use; an edit in the Inspector drops them (OnValidate)
        [NonSerialized] private Dictionary<string, ThemeResolver> _resolvers;

        // The mode named `modeKey`, or null when the theme has none
        public ThemeMode Mode(string modeKey) => modes.Find(m => m.key == modeKey);

        // The resolver of `modeKey` (a mode this theme has), or null
        public ThemeResolver ResolverFor(string modeKey)
        {
            if (string.IsNullOrEmpty(modeKey)) return null;
            _resolvers ??= new Dictionary<string, ThemeResolver>();
            if (_resolvers.TryGetValue(modeKey, out var resolver)) return resolver;
            var mode = Mode(modeKey);
            if (mode == null) return null;
            resolver = new ThemeResolver(shared, mode.tokens);
            _resolvers[modeKey] = resolver;
            return resolver;
        }

        private void OnValidate() => _resolvers = null;
    }

    // One set of values, by kind: each kind is written to USS its own way (ThemeUssGenerator) and read in C# as its own type
    [Serializable]
    public sealed class ThemeTokenSet
    {
        public List<ThemeToken<Color>> colours = new();
        public List<ThemeToken<ThemeLength>> lengths = new();
        public List<ThemeToken<float>> numbers = new();
        [Tooltip("In seconds")]
        public List<ThemeToken<float>> durations = new();
        // - a closed list (UI Toolkit's own curves): a cubic-bezier() written in USS throws on every style update (_3.3 R8 S6)
        public List<ThemeToken<EasingMode>> easings = new();
        public List<ThemeToken<FontStyle>> fontStyles = new();
    }

    // One token: its USS name (--ts-...), its value, and an optional note the generated stylesheet carries above it
    [Serializable]
    public struct ThemeToken<T>
    {
        public string name;
        public T value;
        public string note;
    }

    // A size: px, or a share of the element's own size (a radius of 50% = round)
    [Serializable]
    public struct ThemeLength
    {
        public float value;
        public ThemeLengthUnit unit;
    }

    public enum ThemeLengthUnit { Px, Percent }

    // A mode (ts-mode-<key>): the values that change with it
    [Serializable]
    public sealed class ThemeMode
    {
        public string key;
        public ThemeTokenSet tokens = new();
    }

    // One option of a wall knob (R12.4: knob "roundness", option "round"): the values it lays over the theme's, shared and per mode
    [Serializable]
    public sealed class ThemeKnobOption
    {
        public string knob;
        public string option;
        public ThemeTokenSet shared = new();
        public List<ThemeMode> modes = new();
    }
}
