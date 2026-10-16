using System.Globalization;
using UnityEngine.UIElements;

namespace TileStories
{
    // The next_up navigation element (_3.2 section 5.2, canvas B4, linear): a gentle guided path along the card. It names the block AFTER
    // the one being read -- "Up next", that block's heading, and "5 of 13" (its place among ALL the card's blocks a jump can land on) --
    // beside a Next button that moves the card there. NextUpRule decides which block and which number; this view reads the controller's
    // CurrentBlock (the spy over the targets, frozen on a jump's block while it glides, so the words move the instant Next is tapped) and
    // draws the answer. At the last block there is nothing to go to, so the bar says the card is finished (CardStrings) and shows no Next
    // button, never one that goes nowhere. The words of the heading are what the stack draws over that block (NavController.HeadingOf);
    // a block with none is named by its kind (the model's words for it, the ones the chips use).
    // It lists BLOCKS, not sections, so a one-family card gets it too (NavChoice.Blocks, rule 2.5.7).
    public sealed class NextUpNavView : INavElementView
    {
        // On the root at the card's last block: the stylesheet gives the finished words the weight the missing block and button leave
        public const string DoneClass = "nav-next-up--done";

        public VisualElement Root { get; }
        // The small line over the heading ("Up next", or the finished words) and the place beside it ("5 of 13")
        public Label Caption { get; }
        public Label Position { get; }
        // The next block's heading (hidden at the end)
        public Label Heading { get; }
        // The Next button and its words (hidden at the end)
        public Button Next { get; }
        public Label NextLabel { get; }
        // The block Next moves to now (null at the end)
        public string NextBlock { get; private set; }
        public bool Finished { get; private set; }

        private NavController _nav;
        private CardNavModel _model;
        private CardStrings _strings;

        public NextUpNavView()
        {
            Root = new VisualElement { name = "nav-next-up" };
            Root.AddToClassList("nav-next-up");
            var text = new VisualElement { name = "nav-next-up-text", pickingMode = PickingMode.Ignore };
            text.AddToClassList("nav-next-up__text");
            var line = new VisualElement { pickingMode = PickingMode.Ignore };
            line.AddToClassList("nav-next-up__line");
            Caption = new Label { name = "nav-next-up-caption", pickingMode = PickingMode.Ignore };
            Caption.AddToClassList("nav-next-up__caption");
            Position = new Label { name = "nav-next-up-position", pickingMode = PickingMode.Ignore };
            Position.AddToClassList("nav-next-up__position");
            line.Add(Caption);
            line.Add(Position);
            Heading = new Label { name = "nav-next-up-heading", pickingMode = PickingMode.Ignore };
            Heading.AddToClassList("nav-next-up__heading");
            text.Add(line);
            text.Add(Heading);
            Next = new Button(GoNext) { name = "nav-next-up-button" };
            // - the actions' pill shape (CardParts.uss), so every pill of the card is one shape
            Next.AddToClassList("card-pill");
            Next.AddToClassList("nav-next-up__button");
            NextLabel = new Label { pickingMode = PickingMode.Ignore };
            NextLabel.AddToClassList("nav-next-up__button-label");
            Next.Add(NextLabel);
            Root.Add(text);
            Root.Add(Next);
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            _model = model;
            _strings = context?.Strings;
            nav.CurrentBlockChanged += Refresh;
            Refresh(nav.CurrentBlock);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentBlockChanged -= Refresh;
            _nav = null;
            _model = null;
            _strings = null;
            NextBlock = null;
            Finished = false;
        }

        // The block being read changed: say which block comes next
        private void Refresh(string currentBlock)
        {
            if (_model == null) return;
            var answer = NextUpRule.Of(_model.Targets, currentBlock);
            NextBlock = answer.NextBlock;
            Finished = answer.Finished;
            Root.EnableInClassList(DoneClass, answer.Finished);
            Caption.text = Words(answer.Finished ? CardStrings.Keys.NavNextUpDone : CardStrings.Keys.NavNextUp);
            Position.text = string.Format(CultureInfo.InvariantCulture, Words(CardStrings.Keys.NavNextUpPosition), answer.Position, answer.Total);
            Heading.text = answer.NextBlock != null ? HeadingOf(answer.NextBlock) : "";
            NextLabel.text = Words(CardStrings.Keys.NavNextButton);
            Next.tooltip = NextLabel.text;
            // - at the end there is no block to name and no button that goes nowhere (the stylesheet keeps the bar one touch target tall,
            //   so it does not shrink under the reader's thumb when the card ends)
            Heading.style.display = answer.Finished ? DisplayStyle.None : DisplayStyle.Flex;
            Next.style.display = answer.Finished ? DisplayStyle.None : DisplayStyle.Flex;
        }

        // The Next button's tap: move the card to the block named
        public void GoNext()
        {
            if (NextBlock != null) _nav?.JumpToBlock(NextBlock, BuiltInNavElements.NextUp);
        }

        // What the stack draws over the block; a block with no heading is named by its kind (what a chip of it says)
        private string HeadingOf(string blockKey)
        {
            string heading = _nav?.HeadingOf(blockKey) ?? "";
            if (heading.Length > 0) return heading;
            string kindWords = _model?.KindOf(blockKey)?.LabelKey;
            return kindWords != null ? Words(kindWords) : "";
        }

        private string Words(string key) => _strings?.Get(key) ?? "";
    }
}
