using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The feedback block (_3.1 Tier 3): one question asked of the visitor, answered once.
    //   thumbs -- a thumb up and a thumb down, each with its words under it
    //   stars  -- one to five stars; the vote fills the stars up to it
    // The question is the block's own, else the card's wording for the look (CardStrings). A vote is remembered through the
    // card's CardLocalState (the block opens showing it as given, with the thank-you) and reported ONCE as a feedback event
    // through ICardEvents; there is no changing it and no backend. The thumbs and stars are drawn by USS (no glyph font, no
    // picture). Only classes here; Community.uss draws it.
    public sealed class FeedbackBlockView : IBlockView
    {
        // One thing the visitor can tap: a thumb or a star
        public sealed class Vote
        {
            public Button Button;
            public VisualElement Glyph;
            public Label Text;
            // What it votes (FeedbackRule: thumbs 1 up / 0 down, stars 1..5)
            public int Value;
        }

        public VisualElement Root { get; }
        public Label Question { get; }
        public VisualElement Row { get; }
        public Label Thanks { get; }
        public IReadOnlyList<Vote> Votes => _shown;

        // The vote given (-1 = none yet)
        public int Voted { get; private set; } = -1;

        private readonly List<Vote> _pool = new();
        private readonly List<Vote> _shown = new();
        private CardStrings _strings;
        private CardLocalState _state;
        private ICardEvents _events;
        private string _wallId = "";
        private string _poiId = "";
        private string _blockKey = "";
        private string _variant = "";
        private string _variantClass;

        public FeedbackBlockView()
        {
            Root = new VisualElement { name = "card-feedback" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-feedback");
            Question = new Label();
            Question.AddToClassList("card-feedback__question");
            Row = new VisualElement();
            Row.AddToClassList("card-feedback__row");
            Thanks = new Label();
            Thanks.AddToClassList("card-feedback__thanks");
            Root.Add(Question);
            Root.Add(Row);
            Root.Add(Thanks);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variant = context.Variant;
            _variantClass = "card-feedback--" + _variant;
            Root.AddToClassList(_variantClass);
            _strings = context.Strings;
            _state = context.State;
            _events = context.Events;
            _wallId = context.State?.WallId ?? context.Taxonomy?.wall_id ?? "";
            _poiId = context.Poi?.id ?? "";
            _blockKey = instance.key ?? "";
            bool stars = _variant == BuiltInBlocks.FeedbackStars;
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string own = read.Text(BuiltInBlocks.FeedbackQuestionField);
            Question.text = own.Length > 0 ? own : _strings?.Get(stars ? CardStrings.Keys.FeedbackStarsQuestion : CardStrings.Keys.FeedbackThumbsQuestion) ?? "";
            Question.style.display = Question.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            Thanks.text = _strings?.Get(CardStrings.Keys.FeedbackThanks) ?? "";

            if (stars)
                for (int star = 1; star <= FeedbackRule.StarCount; star++) Add(star, "star");
            else
            {
                Add(FeedbackRule.ThumbUp, "up");
                Add(FeedbackRule.ThumbDown, "down");
            }
            Voted = FeedbackRule.Stored(_variant, _state?.Vote(_poiId, _blockKey) ?? -1);
            ShowVoted();
        }

        public void Unbind()
        {
            foreach (var vote in _shown) vote.Button.RemoveFromHierarchy();
            _shown.Clear();
            Voted = -1;
            _state = null;
            _events = null;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Give the vote `value` (a vote this look can have; a block already voted on keeps its vote): remember it, tell the
        // events sink once, show it as given
        public void Cast(int value)
        {
            if (Voted >= 0 || !FeedbackRule.IsValid(_variant, value)) return;
            Voted = value;
            _state?.SetVote(_poiId, _blockKey, value);
            _events?.Raise(new CardEvent(CardEventKinds.Feedback, _wallId, _poiId, _blockKey, _variant, FeedbackRule.EventValue(_variant, value)));
            ShowVoted();
        }

        // The block as it stands: nothing picked, or the vote given (thumbs: that thumb; stars: every star up to it) and the thank-you
        private void ShowVoted()
        {
            bool voted = Voted >= 0;
            Root.EnableInClassList("card-feedback--voted", voted);
            bool stars = _variant == BuiltInBlocks.FeedbackStars;
            foreach (var vote in _shown)
                vote.Button.EnableInClassList("card-feedback__vote--on", voted && (stars ? vote.Value <= Voted : vote.Value == Voted));
            Thanks.style.display = voted ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Add(int value, string glyphKind)
        {
            var vote = Take(_shown.Count);
            vote.Value = value;
            vote.Glyph.ClearClassList();
            vote.Glyph.AddToClassList("card-feedback__glyph");
            vote.Glyph.AddToClassList("card-feedback__glyph--" + glyphKind);
            bool stars = glyphKind == "star";
            // - thumbs carry words under the glyph (they say what each thumb means); a star needs none
            vote.Text.text = stars ? "" : _strings?.Get(value == FeedbackRule.ThumbUp ? CardStrings.Keys.FeedbackThumbUp : CardStrings.Keys.FeedbackThumbDown) ?? "";
            vote.Text.style.display = stars ? DisplayStyle.None : DisplayStyle.Flex;
            string named = stars
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, _strings?.Get(CardStrings.Keys.FeedbackStar) ?? "", value, FeedbackRule.StarCount)
                : vote.Text.text;
            vote.Button.tooltip = named;
            Row.Add(vote.Button);
            _shown.Add(vote);
        }

        private Vote Take(int index)
        {
            while (_pool.Count <= index)
            {
                var vote = new Vote { Button = new Button(), Glyph = new VisualElement { pickingMode = PickingMode.Ignore }, Text = new Label { pickingMode = PickingMode.Ignore } };
                vote.Button.AddToClassList("card-feedback__vote");
                vote.Button.AddToClassList("card-tap");
                vote.Text.AddToClassList("card-feedback__vote-text");
                // - a star is two squares, one turned an eighth of a turn: an eight-pointed star, no font glyph needed
                for (int point = 0; point < 2; point++)
                {
                    var square = new VisualElement { pickingMode = PickingMode.Ignore };
                    square.AddToClassList("card-feedback__star-square");
                    square.AddToClassList(point == 0 ? "card-feedback__star-square--a" : "card-feedback__star-square--b");
                    vote.Glyph.Add(square);
                }
                vote.Button.Add(vote.Glyph);
                vote.Button.Add(vote.Text);
                var captured = vote;
                vote.Button.clicked += () => Cast(captured.Value);
                _pool.Add(vote);
            }
            return _pool[index];
        }
    }
}
