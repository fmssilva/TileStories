using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The poll block (_3.1 Tier 3, step 8B): one question, two to six options as tappable rows, one vote.
    // A vote is remembered through the card's CardLocalState (the block opens showing it) under the option's authored row and
    // reported ONCE as a poll event through ICardEvents; there is no changing it.
    // There is NO backend: after voting the block shows the visitor's own choice (a tick and the words Your choice) and a
    // thank-you -- never a percentage it invented. Other visitors' votes would come from an IPollResults; while that returns nothing
    // (NoPollResults, today) the results bars stay hidden. Only classes here; Play.uss draws it.
    public sealed class PollBlockView : IBlockView
    {
        // One option: its tap target, the results bar behind its words (hidden without results), the words, the tick and
        // "Your choice" of the option picked, and the share when results exist
        public sealed class OptionRow
        {
            public Button Button;
            public VisualElement Fill;
            public Label Text;
            public CardIcons.VectorGlyph Mark;
            public Label Caption;
            public Label Percent;
            // The authored row this option is (what the vote is stored under)
            public int Row;
        }

        public VisualElement Root { get; }
        public Label Question { get; }
        public VisualElement OptionsColumn { get; }
        public Label Thanks { get; }
        public IReadOnlyList<OptionRow> Options => _shown;

        // The authored row the visitor voted for (-1 = no vote yet)
        public int Voted { get; private set; } = -1;

        // Whether the results bars are drawn (only after a vote, and only when an IPollResults gave numbers)
        public bool ResultsShown { get; private set; }

        private readonly List<OptionRow> _pool = new();
        private readonly List<OptionRow> _shown = new();
        private readonly List<PollRule.Option> _options = new();
        private CardStrings _strings;
        private CardLocalState _state;
        private ICardEvents _events;
        private IPollResults _results;
        private string _wallId = "";
        private string _poiId = "";
        private string _blockKey = "";
        private string _variant = "";
        private string _variantClass;

        public PollBlockView()
        {
            Root = new VisualElement { name = "card-poll" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-poll");
            Question = new Label();
            Question.AddToClassList("card-poll__question");
            OptionsColumn = new VisualElement();
            OptionsColumn.AddToClassList("card-poll__options");
            Thanks = new Label();
            Thanks.AddToClassList("card-poll__thanks");
            Root.Add(Question);
            Root.Add(OptionsColumn);
            Root.Add(Thanks);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variant = context.Variant;
            _variantClass = "card-poll--" + _variant;
            Root.AddToClassList(_variantClass);
            _strings = context.Strings;
            _state = context.State;
            _events = context.Events;
            _results = context.Service<IPollResults>();
            _wallId = context.State?.WallId ?? context.Taxonomy?.wall_id ?? "";
            _poiId = context.Poi?.id ?? "";
            _blockKey = instance.key ?? "";
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            Question.text = read.Text(BuiltInBlocks.PollQuestionField);
            Thanks.text = _strings?.Get(CardStrings.Keys.PollThanks) ?? "";
            _options.AddRange(PollRule.Options(read));
            foreach (var option in _options)
            {
                var row = Take(_shown.Count);
                row.Row = option.Row;
                row.Text.text = option.Text;
                row.Button.tooltip = option.Text;
                row.Caption.text = _strings?.Get(CardStrings.Keys.PollYourChoice) ?? "";
                OptionsColumn.Add(row.Button);
                _shown.Add(row);
            }
            int stored = _state?.PollVote(_poiId, _blockKey) ?? -1;
            // - a vote stored for an option that is no longer shown is no vote
            Voted = stored >= 0 && PollRule.IsShownRow(_options, stored) ? stored : -1;
            ShowVoted();
        }

        public void Unbind()
        {
            foreach (var row in _shown) row.Button.RemoveFromHierarchy();
            _shown.Clear();
            _options.Clear();
            Voted = -1;
            ResultsShown = false;
            _state = null;
            _events = null;
            _results = null;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Vote for the option of authored row `row` (an option that is shown; a block already voted on keeps its vote): remember it,
        // tell the events sink once, show it as given
        public void Vote(int row)
        {
            if (Voted >= 0 || !PollRule.IsShownRow(_options, row)) return;
            Voted = row;
            _state?.SetPollVote(_poiId, _blockKey, row);
            _events?.Raise(new CardEvent(CardEventKinds.Poll, _wallId, _poiId, _blockKey, _variant, PollRule.EventValue(row)));
            ShowVoted();
        }

        // The block as it stands: nothing picked, or the vote given (a tick, "Your choice", the thank-you) and -- only when results
        // exist -- each option's share
        private void ShowVoted()
        {
            bool voted = Voted >= 0;
            Root.EnableInClassList("card-poll--voted", voted);
            Thanks.style.display = voted ? DisplayStyle.Flex : DisplayStyle.None;
            int[] shares = null;
            if (voted && _results != null && _results.TryGet(_wallId, _poiId, _blockKey, out var counts))
                shares = PollRule.Percentages(_options, counts);
            ResultsShown = shares != null;
            string percentFormat = _strings?.Get(CardStrings.Keys.PollPercent) ?? "";
            for (int i = 0; i < _shown.Count; i++)
            {
                var row = _shown[i];
                bool mine = voted && row.Row == Voted;
                row.Button.EnableInClassList("card-poll__option--chosen", mine);
                row.Button.EnableInClassList("card-poll__option--other", voted && !mine);
                row.Mark.style.display = mine ? DisplayStyle.Flex : DisplayStyle.None;
                row.Caption.style.display = mine ? DisplayStyle.Flex : DisplayStyle.None;
                row.Fill.style.display = ResultsShown ? DisplayStyle.Flex : DisplayStyle.None;
                row.Percent.style.display = ResultsShown ? DisplayStyle.Flex : DisplayStyle.None;
                if (!ResultsShown) continue;
                row.Fill.style.width = new Length(shares[i], LengthUnit.Percent);
                row.Percent.text = string.Format(System.Globalization.CultureInfo.InvariantCulture, percentFormat, shares[i]);
            }
        }

        private OptionRow Take(int index)
        {
            while (_pool.Count <= index)
            {
                var row = new OptionRow
                {
                    Button = new Button(),
                    Fill = new VisualElement { pickingMode = PickingMode.Ignore },
                    Text = new Label { pickingMode = PickingMode.Ignore },
                    Mark = CardIcons.CreateVector(CardIcons.Shape.Tick),
                    Caption = new Label { pickingMode = PickingMode.Ignore },
                    Percent = new Label { pickingMode = PickingMode.Ignore },
                };
                row.Button.AddToClassList("card-poll__option");
                row.Button.AddToClassList("card-tap");
                row.Fill.AddToClassList("card-poll__fill");
                row.Text.AddToClassList("card-poll__option-text");
                row.Mark.AddToClassList("card-poll__mark");
                row.Caption.AddToClassList("card-poll__caption");
                row.Percent.AddToClassList("card-poll__percent");
                // - the words and "Your choice" stack in one column; the tick and the share sit at the right
                var words = new VisualElement { pickingMode = PickingMode.Ignore };
                words.AddToClassList("card-poll__words");
                words.Add(row.Text);
                words.Add(row.Caption);
                row.Button.Add(row.Fill);
                row.Button.Add(words);
                row.Button.Add(row.Percent);
                row.Button.Add(row.Mark);
                var captured = row;
                row.Button.clicked += () => Vote(captured.Row);
                _pool.Add(row);
            }
            return _pool[index];
        }
    }
}
