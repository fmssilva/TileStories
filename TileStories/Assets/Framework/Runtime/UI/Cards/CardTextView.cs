using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace TileStories
{
    // A card long text (_3.1 section 6): one Label per paragraph (GlossaryMarkup.Paragraphs), glossary words as
    // underlined links, and ONE definition panel that opens under the paragraph whose word was tapped (tap the word
    // again, or the panel, to close it). The first paragraph can be a lede or start with a raised initial (drop cap).
    // Shared by every block that shows long text; plain C#, labels reused across binds. Styled by PoiCard.uss.
    public sealed class CardTextView
    {
        public enum FirstParagraph { Normal, Lede, DropCap }

        public VisualElement Root { get; }
        // The paragraph labels in order (the drop cap's paragraph label is the one beside the initial)
        public IReadOnlyList<Label> Paragraphs => _shown;
        public Label DropCap { get; }
        public VisualElement DefinitionPanel { get; }
        public Label DefinitionTitle { get; }
        public Label DefinitionText { get; }
        // The glossary term whose definition is open, or null
        public string OpenTerm { get; private set; }

        private readonly string _paragraphClass;
        private readonly VisualElement _capRow;
        private readonly List<Label> _pool = new();
        private readonly List<Label> _shown = new();
        private CardGlossary _glossary;

        public CardTextView(string paragraphClass)
        {
            _paragraphClass = paragraphClass;
            Root = new VisualElement();
            Root.AddToClassList("card-text");

            _capRow = new VisualElement();
            _capRow.AddToClassList("card-text__cap-row");
            DropCap = new Label { name = "card-text-dropcap" };
            DropCap.AddToClassList("card-text__dropcap");
            _capRow.Add(DropCap);

            DefinitionPanel = new VisualElement { name = "card-text-definition" };
            DefinitionPanel.AddToClassList("card-text__definition");
            DefinitionTitle = new Label();
            DefinitionTitle.AddToClassList("card-text__definition-title");
            DefinitionText = new Label();
            DefinitionText.AddToClassList("card-text__definition-text");
            DefinitionPanel.Add(DefinitionTitle);
            DefinitionPanel.Add(DefinitionText);
            DefinitionPanel.RegisterCallback<ClickEvent>(_ => CloseDefinition());
        }

        // Show these paragraphs (markup already split: GlossaryMarkup.Paragraphs)
        public void Bind(IReadOnlyList<string> paragraphs, CardGlossary glossary, FirstParagraph first = FirstParagraph.Normal)
        {
            Clear();
            _glossary = glossary;
            for (int i = 0; i < paragraphs.Count; i++)
            {
                string text = paragraphs[i];
                var label = Take(i);
                label.EnableInClassList("card-text__lede", i == 0 && first == FirstParagraph.Lede);
                if (i == 0 && first == FirstParagraph.DropCap && TrySplitInitial(text, out string initial, out string rest))
                {
                    DropCap.text = initial;
                    label.text = RichText(rest);
                    label.AddToClassList("card-text__beside-cap");
                    _capRow.Add(label);
                    Root.Add(_capRow);
                }
                else
                {
                    label.text = RichText(text);
                    Root.Add(label);
                }
                _shown.Add(label);
            }
        }

        public void Clear()
        {
            CloseDefinition();
            foreach (var label in _shown)
            {
                label.RemoveFromClassList("card-text__beside-cap");
                label.RemoveFromHierarchy();
            }
            _shown.Clear();
            DropCap.text = "";
            _capRow.RemoveFromHierarchy();
            _glossary = null;
        }

        // Open the definition of `term` under paragraph `index` (the same term again closes it)
        public void ToggleTerm(string term, string shownWords, int index)
        {
            if (OpenTerm != null && CardGlossary.SameTerm(OpenTerm, term))
            {
                CloseDefinition();
                return;
            }
            string definition = _glossary?.Definition(term);
            if (definition == null || index < 0 || index >= _shown.Count) return;
            OpenTerm = term;
            DefinitionTitle.text = shownWords;
            DefinitionText.text = definition;
            DefinitionPanel.RemoveFromHierarchy();
            // - under the paragraph (or under the drop-cap row that holds it)
            var anchor = _shown[index].parent == _capRow ? _capRow : _shown[index];
            Root.Insert(Root.IndexOf(anchor) + 1, DefinitionPanel);
        }

        public void CloseDefinition()
        {
            OpenTerm = null;
            DefinitionPanel.RemoveFromHierarchy();
        }

        private string RichText(string paragraph) =>
            GlossaryMarkup.ToRichText(GlossaryMarkup.Parse(paragraph), t => _glossary != null && _glossary.Knows(t));

        // The first letter of a paragraph that starts with plain text (a paragraph starting with a [[term]] has none)
        private static bool TrySplitInitial(string paragraph, out string initial, out string rest)
        {
            initial = rest = null;
            if (string.IsNullOrEmpty(paragraph) || paragraph.StartsWith("[[") || !char.IsLetterOrDigit(paragraph[0])) return false;
            initial = paragraph.Substring(0, 1);
            rest = paragraph.Substring(1);
            return true;
        }

        private Label Take(int index)
        {
            while (_pool.Count <= index)
            {
                var label = new Label { enableRichText = true };
                label.AddToClassList("card-text__paragraph");
                label.AddToClassList(_paragraphClass);
                int at = _pool.Count;
                label.RegisterCallback<PointerUpLinkTagEvent>(evt => ToggleTerm(evt.linkID, evt.linkText, at));
                _pool.Add(label);
            }
            return _pool[index];
        }
    }
}
