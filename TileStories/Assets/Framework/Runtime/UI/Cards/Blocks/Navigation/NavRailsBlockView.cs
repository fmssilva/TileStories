using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The nav_rails block (_3.2 section 5.6, D3): one horizontal rail per section of the card, a card for each block in it -- the family's
    // icon in its colour, the block's words, its format and minutes (the card's compact-row words: NavController.RowOf, data only, so a card
    // of rails loads no file). A tap on a card moves the card to that block (nav_jump "nav_rails").
    // A rail is a NavStrip -- a clipped viewport and a row moved by an offset, one SwipeGrab on it -- NOT a horizontal ScrollView: a sideways
    // drag moves the rail and a vertical one moves the card, judged once as the finger leaves the slop, so a rail never takes a drag meant to
    // scroll the card (and, in the pages layout, a finger that lands on a rail is the rail's). The cards are sized by CardTrackFit /
    // CardTrackRule like the related carousel's: a whole number of them fills the strip, and with Peek Next Card (the wall's setting) a slice of
    // the next one shows at rest -- the half-cut card IS the cue to swipe. NavRailRule keeps every rail within Swipes To The End swipes of its
    // last card: a section with more blocks ends its rail on an "All n" card that moves the card to the section.
    // It computes no structure: the rails are NavController.Model's sections, drawn when the controller is Bound (the stack binds before
    // the model exists), like nav_icon_grid.
    public sealed class NavRailsBlockView : IBlockView
    {
        // What nav_jump names as the element that asked
        public const string ElementKey = BuiltInBlocks.NavRailsKind;

        // One card of a rail: a block's, or the "All n" card that closes a long rail
        public sealed class RailCard
        {
            public readonly Button Button;
            public readonly VisualElement Icon;
            public readonly Label Words;
            public readonly Label Meta;
            // The block it leads to ("" on the "All" card)
            public string BlockKey { get; internal set; } = "";

            internal RailCard(Button button, VisualElement icon, Label words, Label meta)
            {
                Button = button;
                Icon = icon;
                Words = words;
                Meta = meta;
            }
        }

        // One section's rail: its title, the strip and the cards
        public sealed class Rail
        {
            public readonly VisualElement Root;
            public readonly VisualElement TitleIcon;
            public readonly Label TitleWords;
            public readonly Label TitleCount;
            public readonly NavStrip Strip;
            public readonly ElementPool<RailCard> Cards;
            public readonly RailCard AllCard;
            public int SectionIndex { get; internal set; }
            public string SectionKey { get; internal set; }
            // How many blocks the section holds (what the "All" card says) and what the rail draws of them now (NavRailRule)
            public int Targets { get; internal set; }
            public NavRailRule.Plan Plan { get; internal set; }

            internal Rail(VisualElement root, VisualElement titleIcon, Label titleWords, Label titleCount, NavStrip strip, ElementPool<RailCard> cards, RailCard allCard)
            {
                Root = root;
                TitleIcon = titleIcon;
                TitleWords = titleWords;
                TitleCount = titleCount;
                Strip = strip;
                Cards = cards;
                AllCard = allCard;
            }

            // The block cards the rail draws now, in order (the ones NavRailRule left out are not among them)
            public IEnumerable<RailCard> ShownCards
            {
                get
                {
                    for (int i = 0; i < Cards.Shown.Count && i < Plan.Cards; i++) yield return Cards.Shown[i];
                }
            }
        }

        public VisualElement Root { get; }
        public IReadOnlyList<Rail> Rails => _rails.Shown;

        private readonly ElementPool<Rail> _rails;
        private readonly NavTint _tint;
        private NavController _nav;
        private BlockBindContext _context;
        private BlockInstanceData _instance;

        public NavRailsBlockView()
        {
            Root = new VisualElement { name = "card-nav-rails" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("nav-rails");
            _rails = new ElementPool<Rail>(NewRail, rail => rail.Root.RemoveFromHierarchy());
            _tint = new NavTint(Root);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _context = context;
            _instance = instance;
            _nav = context.Nav;
            // - bound with the stack: nothing to draw yet, the sheet binds the controller to THIS card's model right after it (Bound). Bound
            //   later -- an expandable row opened -- the model is this card's already: draw at once
            Root.style.display = DisplayStyle.None;
            if (_nav == null) return;
            _nav.Bound += Draw;
            if (_nav.HasCurrentModel) Draw();
        }

        public void Unbind()
        {
            if (_nav != null) _nav.Bound -= Draw;
            _nav = null;
            _context = null;
            _instance = null;
            ReleaseRails();
        }

        private void ReleaseRails()
        {
            foreach (var rail in _rails.Shown)
            {
                rail.Cards.ReleaseAll();
                rail.AllCard.Button.RemoveFromHierarchy();
            }
            _rails.ReleaseAll();
            _tint.Clear();
        }

        // One rail per section of the bound model, in its order; none (and no block) with no target at all
        private void Draw()
        {
            ReleaseRails();
            var model = _nav?.Model;
            bool shown = model != null && model.TargetCount > 0;
            Root.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            if (!shown) return;
            for (int sectionIndex = 0; sectionIndex < model.Sections.Count; sectionIndex++)
            {
                var section = model.Sections[sectionIndex];
                var rail = _rails.Take();
                Root.Add(rail.Root);
                rail.SectionIndex = sectionIndex;
                rail.SectionKey = section.Key;
                rail.Targets = section.Targets.Count;
                rail.Plan = new NavRailRule.Plan(section.Targets.Count, false);
                rail.TitleWords.text = Words(section.LabelKey, section.Key);
                // - a format section has no icon: its words say it alone (NavLabelRule)
                CardIcons.SetKey(rail.TitleIcon, section.IconKey);
                rail.TitleIcon.style.display = section.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
                _tint.Icon(rail.TitleIcon, section.ColorToken);
                NavCount.Set(rail.TitleCount, true, section.Count);
                rail.Strip.Reset();
                foreach (string target in section.Targets)
                {
                    var card = rail.Cards.Take();
                    rail.Strip.Row.Add(card.Button);
                    card.BlockKey = target;
                    var row = _nav.RowOf(target);
                    card.Words.text = row.Words;
                    card.Meta.text = row.Meta;
                    card.Meta.style.display = row.Meta.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                    CardIcons.SetKey(card.Icon, row.IconKey);
                    card.Icon.style.display = row.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
                    _tint.Icon(card.Icon, row.ColorToken);
                    card.Button.tooltip = row.Words;
                    card.Button.style.display = DisplayStyle.Flex;
                }
                rail.AllCard.Words.text = string.Format(CultureInfo.InvariantCulture, Words(CardStrings.Keys.NavRailsAll, "{0}"), section.Targets.Count);
                rail.AllCard.Button.tooltip = rail.AllCard.Words.text;
                rail.Strip.Row.Add(rail.AllCard.Button);
                rail.AllCard.Button.style.display = DisplayStyle.None;
                Fit(rail);
            }
        }

        // Size a rail's cards for its strip as laid out now (CardTrackFit: a whole number of cards, plus the next one's slice with Peek Next
        // Card), then keep the rail within the swipes it may take (NavRailRule): the cards that do not fit are put away and the "All n" card
        // takes the last cell. Runs on every size change of the strip (a rotated screen, a sheet raised to another stop)
        private void Fit(Rail rail)
        {
            if (rail.Cards.Shown.Count == 0) return;
            // - the fit counts the block cards only: the "All" cell exists to end a rail that is already longer than it may be, and counted in
            //   a short one it would ask for a next card to peek at where there is none
            var cells = new List<VisualElement>();
            foreach (var card in rail.Cards.Shown) cells.Add(card.Button);
            float width = CardTrackFit.Apply(rail.Strip.Viewport, cells, _context?.Settings?.container?.peek_next_card ?? true);
            if (width > 0f) rail.AllCard.Button.style.width = width;
            if (width <= 0f)
            {
                rail.AllCard.Button.style.display = DisplayStyle.None;
                return;
            }
            float gap = rail.Cards.Shown[0].Button.resolvedStyle.marginRight;
            float viewport = rail.Strip.VisibleWidth;
            rail.Plan = NavRailRule.Of(rail.Targets, width + gap, viewport, MaxSwipes());
            for (int i = 0; i < rail.Cards.Shown.Count; i++)
                rail.Cards.Shown[i].Button.style.display = i < rail.Plan.Cards ? DisplayStyle.Flex : DisplayStyle.None;
            rail.AllCard.Button.style.display = rail.Plan.HasAll ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // The block's Swipes To The End (the field's default when none is stored)
        private int MaxSwipes()
        {
            var field = BuiltInBlocks.NavRails.Field(BuiltInBlocks.NavRailsMaxSwipesField);
            return Mathf.RoundToInt(new BlockFieldReader(_instance, _context?.Language, _context?.FallbackLanguage).Number(field));
        }

        private Rail NewRail(int index)
        {
            var root = new VisualElement { name = "nav-rails-rail-" + index };
            root.AddToClassList("nav-rails__rail");
            var title = new VisualElement { pickingMode = PickingMode.Ignore };
            title.AddToClassList("nav-rails__title");
            var icon = CardIcons.Create();
            icon.pickingMode = PickingMode.Ignore;
            icon.AddToClassList("nav-rails__title-icon");
            var words = new Label { pickingMode = PickingMode.Ignore };
            words.AddToClassList("nav-rails__title-words");
            var count = NavCount.Create();
            title.Add(icon);
            title.Add(words);
            title.Add(count);
            var strip = new NavStrip("nav-rails-strip-" + index, "nav-rails__viewport", "nav-rails__row");
            root.Add(title);
            root.Add(strip.Viewport);
            Rail rail = null;
            var cards = new ElementPool<RailCard>(i => NewCard("nav-rails-card-" + index + "-" + i, () => Tap(rail, i)), card => card.Button.RemoveFromHierarchy());
            var all = NewCard("nav-rails-all-" + index, () => TapAll(rail));
            all.Button.AddToClassList("nav-rails__card--all");
            rail = new Rail(root, icon, words, count, strip, cards, all);
            // - a new width (another card's words, a rotated screen, the sheet at another stop) re-fits the cards and the rail's length
            strip.Viewport.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (IsShown(rail)) Fit(rail);
            });
            return rail;
        }

        // Whether this rail is one of the rails drawn now (a pooled rail kept off the card is not re-fitted)
        private bool IsShown(Rail rail)
        {
            for (int i = 0; i < _rails.Shown.Count; i++)
                if (ReferenceEquals(_rails.Shown[i], rail)) return true;
            return false;
        }

        private static RailCard NewCard(string name, System.Action tap)
        {
            var button = new Button(tap) { name = name };
            button.AddToClassList("nav-rails__card");
            button.AddToClassList("card-tap");
            var icon = CardIcons.Create();
            icon.pickingMode = PickingMode.Ignore;
            icon.AddToClassList("nav-rails__card-icon");
            var words = new Label { pickingMode = PickingMode.Ignore };
            words.AddToClassList("nav-rails__card-words");
            var meta = new Label { pickingMode = PickingMode.Ignore };
            meta.AddToClassList("nav-rails__card-meta");
            button.Add(icon);
            button.Add(words);
            button.Add(meta);
            return new RailCard(button, icon, words, meta);
        }

        private string Words(string key, string fallback)
        {
            string words = key != null && _context?.Strings != null ? _context.Strings.Get(key) : "";
            return string.IsNullOrEmpty(words) ? fallback : words;
        }

        // A card moves the card to its block, as a contents row would
        private void Tap(Rail rail, int cardIndex)
        {
            if (rail == null || cardIndex >= rail.Cards.Shown.Count) return;
            string block = rail.Cards.Shown[cardIndex].BlockKey;
            if (!string.IsNullOrEmpty(block)) _nav?.JumpToBlock(block, ElementKey);
        }

        // The "All n" card moves the card to its section's first block, as that section's tab would
        private void TapAll(Rail rail)
        {
            if (rail != null) _nav?.JumpTo(rail.SectionIndex, ElementKey);
        }

        // What a test taps: the card at `cardIndex` of rail `railIndex`
        public void TapCard(int railIndex, int cardIndex) => Tap(_rails.Shown[railIndex], cardIndex);
    }
}
