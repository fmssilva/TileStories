using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The fun_fact block (_3.1 Tier 1): one small card per fact, headed "Did you know?" (CardStrings). Two looks --
    // flip: the fact is hidden behind an invitation to tap; a tap on the card reveals it, a tap on its heading hides it
    // again (a tap on the revealed text is left to the text: a glossary word opens there); postcard: the fact shown at
    // once on a tilted postcard. The fact text is a CardTextView (paragraphs, glossary words). Only classes here.
    public sealed class FunFactBlockView : IBlockView
    {
        public sealed class Fact
        {
            public VisualElement Box;
            public Label Heading;
            public Label Hint;
            public CardTextView Text;
            public bool Revealed;
        }

        public VisualElement Root { get; }
        public IReadOnlyList<Fact> Facts => _shown;

        private readonly List<Fact> _pool = new();
        private readonly List<Fact> _shown = new();
        private string _variantClass;
        private bool _flips;

        public FunFactBlockView()
        {
            Root = new VisualElement { name = "card-fun-fact" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-fun");
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _flips = context.Variant == BuiltInBlocks.FunFactFlip;
            _variantClass = "card-fun--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string heading = context.Strings?.Get(CardStrings.Keys.FunFactLabel) ?? "";
            string hint = context.Strings?.Get(CardStrings.Keys.FunFactHint) ?? "";
            foreach (var item in read.Items(BuiltInBlocks.FunFactItemsField))
            {
                var paragraphs = GlossaryMarkup.Paragraphs(read.ItemText(item, BuiltInBlocks.FunFactTextField));
                if (paragraphs.Count == 0) continue;
                var fact = Take(_shown.Count);
                fact.Heading.text = heading;
                fact.Hint.text = hint;
                fact.Text.Bind(paragraphs, context.Glossary);
                fact.Box.EnableInClassList("card-tap", _flips);
                SetRevealed(fact, !_flips);
                Root.Add(fact.Box);
                _shown.Add(fact);
            }
        }

        public void Unbind()
        {
            foreach (var fact in _shown)
            {
                fact.Text.Clear();
                fact.Box.RemoveFromHierarchy();
            }
            _shown.Clear();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Show or hide a flip card's fact (the postcard look is always revealed)
        public void SetRevealed(Fact fact, bool revealed)
        {
            fact.Revealed = revealed;
            fact.Box.EnableInClassList("card-fun__box--revealed", revealed);
            fact.Text.Root.style.display = revealed ? DisplayStyle.Flex : DisplayStyle.None;
            fact.Hint.style.display = revealed || !_flips ? DisplayStyle.None : DisplayStyle.Flex;
            if (!revealed) fact.Text.CloseDefinition();
        }

        private void OnTap(Fact fact, ClickEvent evt)
        {
            if (!_flips) return;
            if (!fact.Revealed) SetRevealed(fact, true);
            // - once revealed only the heading hides it: a tap on the text belongs to the text (glossary words)
            else if (evt.target == fact.Heading) SetRevealed(fact, false);
        }

        private Fact Take(int index)
        {
            while (_pool.Count <= index)
            {
                var fact = new Fact { Box = new VisualElement(), Heading = new Label(), Hint = new Label(), Text = new CardTextView("card-fun__paragraph") };
                fact.Box.AddToClassList("card-fun__box");
                fact.Heading.AddToClassList("card-fun__heading");
                fact.Hint.AddToClassList("card-fun__hint");
                fact.Box.Add(fact.Heading);
                fact.Box.Add(fact.Hint);
                fact.Box.Add(fact.Text.Root);
                fact.Box.RegisterCallback<ClickEvent>(evt => OnTap(fact, evt));
                _pool.Add(fact);
            }
            return _pool[index];
        }
    }
}
