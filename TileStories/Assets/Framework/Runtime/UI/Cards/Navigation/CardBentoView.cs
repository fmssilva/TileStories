using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The bento layout's overview (_3.2 section 6.1, step 15b): the card's sections as TILES where its blocks were -- every section's blocks
    // set aside by CardLayoutView, so the overview holds no view and no file. Each tile is the section's family icon in its colour, its words
    // and its block count (never colour alone) with, between them, the names of its blocks (the compact row's words: no view, no file) as many
    // as its size holds, in a tile wide enough to read them -- a larger tile shows more of what it leads to; its size from BentoLayoutRule (the section with the most blocks leads a row of its own, the
    // others share theirs by count, never under a tile's minimum: the touch target, its words); the tiles fill the card's visible part while
    // their minimums fit, else they keep them and the overview scrolls -- and never grow past what they hold (step 16: a card with few sections
    // leaves the room under its tiles empty). A tile tap is NavController.JumpTo(section, "bento"): the same jump
    // every element makes, which in this layout opens the section in the full view (CardLayoutView.OpenSectionView).
    // Sheet-owned beside CardPagerView; it reads the layout, never decides it. The tiles' host is in the stack's scroll ONLY while the card is
    // drawn as bento -- right before the first block (after a picture header's hero), so a block of no section (the sources) stays under the
    // tiles -- and out of it in every other layout, which keeps the stack's children exactly what they were (found by the targeted run: a hidden
    // host at index 0 moved every block's place in the scroll by one).
    public sealed class CardBentoView
    {
        // What a tile reports as its element (nav_jump's Variant)
        public const string ElementKey = "bento";
        public const string LeadClass = "card-bento__tile--lead";

        private static readonly CustomStyleProperty<float> GapToken = new("--ts-bento-gap");
        private static readonly CustomStyleProperty<float> NamesWidthToken = new("--ts-bento-names-min-width");

        // One tile: the button (its whole area is the tap target), the family icon, the section's words, the names of its blocks (Lines: the
        // ones the tile's height holds are visible, the others hidden in place) and its count
        public sealed class Tile
        {
            public readonly int Index;
            public readonly Button Button;
            public readonly VisualElement Icon;
            public readonly Label Words;
            public readonly VisualElement LinesBox;
            public readonly List<Label> Lines = new();
            public readonly Label Count;

            public Tile(int index, Button button, VisualElement icon, Label words, VisualElement linesBox, Label count)
            {
                Index = index;
                Button = button;
                Icon = icon;
                Words = words;
                LinesBox = linesBox;
                Count = count;
            }

            // The block names shown now (the tile's height holds them whole)
            public IEnumerable<Label> ShownLines
            {
                get
                {
                    foreach (var line in Lines)
                        if (line.style.display.value != DisplayStyle.None && line.style.visibility.value != Visibility.Hidden) yield return line;
                }
            }

            // The section's block names, one line each (lines left from a longer section are not drawn)
            public void SetNames(IReadOnlyList<string> names)
            {
                for (int i = 0; i < names.Count; i++)
                {
                    if (i >= Lines.Count)
                    {
                        var line = new Label { pickingMode = PickingMode.Ignore };
                        line.AddToClassList("card-bento__name");
                        Lines.Add(line);
                        LinesBox.Add(line);
                    }
                    Lines[i].text = names[i];
                    Lines[i].style.display = DisplayStyle.Flex;
                }
                for (int i = names.Count; i < Lines.Count; i++) Lines[i].style.display = DisplayStyle.None;
            }

            // Hide (in place: visibility, so nothing moves and nothing is laid out again) every name the box does not hold whole -- and all of
            // them in a box narrower than `minWidth` (a name cut to a few letters is noise, not a cue)
            public void FitNames(float minWidth)
            {
                float room = LinesBox.layout.height;
                float width = LinesBox.layout.width;
                if (float.IsNaN(room) || float.IsNaN(width)) return;
                bool wideEnough = width >= minWidth;
                foreach (var line in Lines)
                {
                    if (line.style.display.value == DisplayStyle.None) continue;
                    bool fits = wideEnough && !float.IsNaN(line.layout.yMax) && line.layout.yMax <= room + 0.5f;
                    line.style.visibility = fits ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        // The tiles' box in the stack's scroll (out of it while the card is not drawn as bento)
        public VisualElement Host { get; }
        // Whether the card is drawn as bento now
        public bool IsOn { get; private set; }
        // The tiles shown, one per section in the model's order
        public IReadOnlyList<Tile> Tiles => _tiles.Shown;
        // What the rule said the last time the tiles were placed (null before)
        public BentoLayoutRule.Result Placed { get; private set; }

        private readonly BlockStackView _stack;
        private readonly ScrollView _scroll;
        private readonly CardLayoutView _layout;
        private readonly NavController _nav;
        private readonly ElementPool<Tile> _tiles;
        private readonly NavTint _tint;
        private CardNavModel _model;
        // The size the tiles were last placed for: a placement that would change nothing is skipped (a geometry callback must never dirty
        // the layout it was called for twice)
        private Vector2 _placedFor = new(float.NaN, float.NaN);

        public CardBentoView(BlockStackView stack, CardLayoutView layout, NavController nav)
        {
            _stack = stack;
            var scroll = stack.Scroll;
            _scroll = scroll;
            _layout = layout;
            _nav = nav;
            Host = new VisualElement { name = "card-bento" };
            Host.AddToClassList("card-bento");
            _tint = new NavTint(Host);
            _tiles = new ElementPool<Tile>(CreateTile, tile => tile.Button.RemoveFromHierarchy());
            // - the tiles fill what the visitor sees: the viewport's size, and the host's width once its margins resolve
            scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => Place());
            Host.RegisterCallback<GeometryChangedEvent>(_ => Place());
        }

        // The card's map is (re)built: tiles for its sections when the card is drawn as bento (after CardLayoutView.Bind), else nothing
        public void Bind(CardNavModel model, BlockBindContext context)
        {
            _model = model;
            IsOn = model != null && CardLayoutRule.OpensSectionsInFullView(_layout.Effective);
            _tiles.ReleaseAll();
            _tint.Clear();
            _placedFor = new Vector2(float.NaN, float.NaN);
            Placed = null;
            if (!IsOn)
            {
                Host.RemoveFromHierarchy();
                return;
            }
            PutHostBeforeTheBlocks();
            var strings = context?.Strings;
            System.Func<string, string> words = strings != null ? strings.Get : null;
            int lead = BentoLayoutRule.Lead(Counts(model));
            for (int i = 0; i < model.Sections.Count; i++)
            {
                var section = model.Sections[i];
                var tile = _tiles.Take();
                string sectionWords = section.LabelKey != null && strings != null ? strings.Get(section.LabelKey) : "";
                tile.Words.text = string.IsNullOrEmpty(sectionWords) ? section.Key : sectionWords;
                var names = new List<string>();
                foreach (string target in section.Targets)
                    names.Add(_stack.RowOfBlock(target, context?.Language, context?.FallbackLanguage, words).Words);
                tile.SetNames(names);
                tile.Button.tooltip = tile.Words.text;
                CardIcons.SetKey(tile.Icon, section.IconKey);
                tile.Icon.style.display = section.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
                _tint.Icon(tile.Icon, section.ColorToken);
                tile.Count.text = section.Count.ToString();
                tile.Button.EnableInClassList(LeadClass, i == lead);
                Host.Add(tile.Button);
            }
            Place();
        }

        // The host right before the blocks of the scroll -- the first section sheet holding them (_3.3 P4.4c1) -- after a picture header's hero,
        // which the stack inserts first; or last
        private void PutHostBeforeTheBlocks()
        {
            var content = _scroll.contentContainer;
            Host.RemoveFromHierarchy();
            for (int i = 0; i < content.childCount; i++)
                if (content[i].ClassListContains(SectionSheets.SheetClass) || content[i].ClassListContains("card-block-slot"))
                {
                    content.Insert(i, Host);
                    return;
                }
            content.Add(Host);
        }

        // The card closed
        public void Unbind()
        {
            _model = null;
            IsOn = false;
            _tiles.ReleaseAll();
            _tint.Clear();
            Placed = null;
            Host.RemoveFromHierarchy();
        }

        // Size and place every tile for the room the visitor sees now (BentoLayoutRule decides, this applies)
        private void Place()
        {
            if (!IsOn || _model == null || _tiles.Shown.Count == 0) return;
            float width = Host.layout.width;
            float height = _scroll.contentViewport.layout.height;
            if (float.IsNaN(width) || float.IsNaN(height) || width <= 0f || height <= 0f) return;
            if (Mathf.Abs(width - _placedFor.x) < 0.5f && Mathf.Abs(height - _placedFor.y) < 0.5f) return;
            var minWidths = new float[_tiles.Shown.Count];
            float minHeight = 0f;
            for (int i = 0; i < _tiles.Shown.Count; i++)
            {
                minWidths[i] = MinimumWidth(_tiles.Shown[i]);
                minHeight = Mathf.Max(minHeight, _tiles.Shown[i].Button.resolvedStyle.minHeight.value);
            }
            float namesMinWidth = Token(NamesWidthToken);
            var placed = BentoLayoutRule.Layout(Counts(_model), minWidths, minHeight, width, height, Gap(),
                (section, tileWidth) => ContentHeight(_tiles.Shown[section], tileWidth >= namesMinWidth));
            foreach (var tile in _tiles.Shown)
            {
                var rect = placed.Tiles[tile.Index];
                tile.Button.style.left = rect.X;
                tile.Button.style.top = rect.Y;
                tile.Button.style.width = rect.Width;
                tile.Button.style.height = rect.Height;
            }
            Host.style.height = placed.Height;
            Placed = placed;
            _placedFor = new Vector2(width, height);
        }

        // A tile's narrowest: the touch target (its min-width), or its icon and whole words with the tile's padding
        private static float MinimumWidth(Tile tile)
        {
            var button = tile.Button.resolvedStyle;
            var words = tile.Words.MeasureTextSize(tile.Words.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            float icon = tile.Icon.resolvedStyle.display == DisplayStyle.Flex
                ? tile.Icon.resolvedStyle.width + tile.Icon.resolvedStyle.marginLeft + tile.Icon.resolvedStyle.marginRight : 0f;
            float content = button.paddingLeft + button.paddingRight + button.borderLeftWidth + button.borderRightWidth + icon + words.x
                + tile.Words.resolvedStyle.marginLeft + tile.Words.resolvedStyle.marginRight;
            float min = button.minWidth.value;
            return Mathf.Max(float.IsNaN(min) ? 0f : min, Mathf.Ceil(content));
        }

        // How tall a tile must be to hold everything it shows: its padding and border, the title line (icon or words, the taller), every one of
        // its block names when its width shows them (each with its margin), and its count -- measured from the text, never from the tile's own
        // layout (which is what is being decided). What the rule caps a row at, so a plate never grows past its words (_3.2 step 16)
        private static float ContentHeight(Tile tile, bool showsNames)
        {
            var button = tile.Button.resolvedStyle;
            float line = Mathf.Max(tile.Icon.resolvedStyle.display == DisplayStyle.Flex ? tile.Icon.resolvedStyle.height : 0f, TextHeight(tile.Words));
            float names = 0f;
            if (showsNames)
                foreach (var name in tile.Lines)
                    if (name.style.display.value != DisplayStyle.None) names += name.resolvedStyle.marginTop + TextHeight(name);
            float count = TextHeight(tile.Count) + tile.Count.resolvedStyle.marginTop + tile.Count.resolvedStyle.marginBottom;
            return Mathf.Ceil(button.paddingTop + button.paddingBottom + button.borderTopWidth + button.borderBottomWidth + line + names + count);
        }

        private static float TextHeight(Label label) =>
            label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).y;

        // The gap between tiles: a plain-number token on the card root (.ts-card), where the tokens are declared
        private float Gap() => Token(GapToken);

        // A plain-number token read on the card root (.ts-card); 0 when there is none
        private float Token(CustomStyleProperty<float> token)
        {
            for (var e = Host; e != null; e = e.parent)
                if (e.ClassListContains("ts-card")) return e.customStyle.TryGetValue(token, out float value) ? value : 0f;
            return 0f;
        }

        private static int[] Counts(CardNavModel model)
        {
            var counts = new int[model.Sections.Count];
            for (int i = 0; i < counts.Length; i++) counts[i] = model.Sections[i].Count;
            return counts;
        }

        private Tile CreateTile(int index)
        {
            var button = new Button(() => _nav.JumpTo(index, ElementKey)) { name = "card-bento-tile-" + index };
            button.AddToClassList("card-bento__tile");
            var line = new VisualElement { pickingMode = PickingMode.Ignore };
            line.AddToClassList("card-bento__line");
            var icon = CardIcons.Create();
            icon.pickingMode = PickingMode.Ignore;
            icon.AddToClassList("card-bento__icon");
            var words = new Label { pickingMode = PickingMode.Ignore };
            words.AddToClassList("card-bento__words");
            line.Add(icon);
            line.Add(words);
            var lines = new VisualElement { pickingMode = PickingMode.Ignore };
            lines.AddToClassList("card-bento__names");
            var count = new Label { pickingMode = PickingMode.Ignore };
            count.AddToClassList("card-bento__count");
            button.Add(line);
            button.Add(lines);
            button.Add(count);
            var tile = new Tile(index, button, icon, words, lines, count);
            // - the box's height is the tile's (fixed by the rule): fitting the names never changes it, so this never lays out again
            lines.RegisterCallback<GeometryChangedEvent>(_ => tile.FitNames(Token(NamesWidthToken)));
            return tile;
        }
    }
}
