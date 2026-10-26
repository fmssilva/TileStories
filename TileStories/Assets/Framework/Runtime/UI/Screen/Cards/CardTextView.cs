using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace TileStories
{
    // A card long text (_3.1 section 6): one Label per paragraph (GlossaryMarkup.Paragraphs), glossary words as
    // underlined links, and ONE definition panel that opens under the paragraph whose word was tapped (tap the word
    // again, or the panel, to close it). The first paragraph can be a lede or start with a raised initial (drop cap).
    // Shared by every block that shows long text; plain C#, labels reused across binds. Styled by CardParts.uss.
    // DropCapFlow (_3.3 S2a, the story's paragraph): the initial raised beside the first DropCapLines lines only, the rest of the
    // paragraph running the full width under it (DropCapRule cuts it once the paragraph has a width). Leading: the lines' height as a
    // percentage of the type size (a rich-text line-height; 0 = the face's own), handed in by a component from its own USS token.
    public sealed class CardTextView
    {
        public enum FirstParagraph { Normal, Lede, DropCap, DropCapFlow }

        // How many lines sit beside a flowing drop cap (the raised letter's height in lines of the paragraph)
        public const int DropCapLines = 2;

        public VisualElement Root { get; }
        // The paragraph labels in order (the drop cap's paragraph label is the one beside the initial)
        public IReadOnlyList<Label> Paragraphs => _paragraphs.Shown;
        public Label DropCap { get; }
        public VisualElement DefinitionPanel { get; }
        public Label DefinitionTitle { get; }
        public Label DefinitionText { get; }
        // The glossary term whose definition is open, or null
        public string OpenTerm { get; private set; }

        // The lines' height in percent of the type size (0 = the face's own): set before Bind
        public float Leading { get; set; }
        // A flowing drop cap's two parts: the words beside the letter and the rest of the paragraph under it (null when none is shown)
        public Label FlowBeside { get; private set; }
        public Label FlowUnder { get; private set; }

        private readonly string _paragraphClass;
        private readonly VisualElement _capRow;
        private readonly ElementPool<Label> _paragraphs;
        private CardGlossary _glossary;
        // The first paragraph after its initial, cut again whenever the room beside the letter changes
        private string _flowText;
        private float _flowWidth = -1f;

        public CardTextView(string paragraphClass)
        {
            _paragraphClass = paragraphClass;
            _paragraphs = new ElementPool<Label>(NewParagraph, label =>
            {
                label.RemoveFromClassList("card-text__beside-cap");
                label.RemoveFromClassList(FlowClass);
                label.style.display = StyleKeyword.Null;
                label.RemoveFromHierarchy();
            });
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
                var label = _paragraphs.Take();
                label.EnableInClassList("card-text__lede", i == 0 && first == FirstParagraph.Lede);
                if (i == 0 && first == FirstParagraph.DropCapFlow && TrySplitInitial(text, out string flowInitial, out string flowRest))
                {
                    // - beside the letter until the first layout gives it a width, then cut (Flow)
                    DropCap.text = flowInitial;
                    label.text = RichText(flowRest);
                    label.AddToClassList("card-text__beside-cap");
                    label.AddToClassList(FlowClass);
                    _capRow.Add(label);
                    Root.Add(_capRow);
                    FlowBeside = label;
                    FlowUnder = _paragraphs.Take();
                    FlowUnder.text = "";
                    FlowUnder.style.display = DisplayStyle.None;
                    Root.Add(FlowUnder);
                    _flowText = flowRest;
                    _flowWidth = -1f;
                    Flow();
                }
                else if (i == 0 && first == FirstParagraph.DropCap && TrySplitInitial(text, out string initial, out string rest))
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
            }
        }

        public void Clear()
        {
            CloseDefinition();
            _paragraphs.ReleaseAll();
            DropCap.text = "";
            _capRow.RemoveFromHierarchy();
            _glossary = null;
            FlowBeside = FlowUnder = null;
            _flowText = null;
        }

        // The class a flowing drop cap's beside part wears (its width is the row's, never its words')
        public const string FlowClass = "card-text__beside-cap--flow";

        // Cut the flowing paragraph for the room beside the letter now: the words of DropCapLines lines beside it, the rest under it. Only a
        // change of that room cuts again -- the words' own height never feeds back into the width, so this cannot loop
        private void Flow()
        {
            if (FlowBeside == null || _flowText == null) return;
            float width = FlowBeside.contentRect.width;
            if (float.IsNaN(width) || !(width > 0f) || UnityEngine.Mathf.Abs(width - _flowWidth) < 0.5f) return;
            _flowWidth = width;
            var beside = FlowBeside;
            var cut = DropCapRule.Split(_flowText, width, DropCapLines,
                run => beside.MeasureTextSize(run, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x);
            beside.text = RichText(cut.Beside);
            FlowUnder.text = cut.Under.Length > 0 ? RichText(cut.Under) : "";
            FlowUnder.style.display = cut.Under.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
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
            var shown = _paragraphs.Shown;
            if (definition == null || index < 0 || index >= shown.Count) return;
            OpenTerm = term;
            DefinitionTitle.text = shownWords;
            DefinitionText.text = definition;
            DefinitionPanel.RemoveFromHierarchy();
            // - under the paragraph (or under the drop-cap row that holds it)
            var anchor = shown[index].parent == _capRow ? _capRow : shown[index];
            Root.Insert(Root.IndexOf(anchor) + 1, DefinitionPanel);
        }

        public void CloseDefinition()
        {
            OpenTerm = null;
            DefinitionPanel.RemoveFromHierarchy();
        }

        private string RichText(string paragraph)
        {
            string text = GlossaryMarkup.ToRichText(GlossaryMarkup.Parse(paragraph), t => _glossary != null && _glossary.Knows(t));
            // - USS has no line height: TextCore's rich-text tag carries it, its percentage a component's token (never typed here)
            return ElementStyle.WithLeading(text, Leading);
        }

        // The first letter of a paragraph that starts with plain text (a paragraph starting with a [[term]] has none)
        private static bool TrySplitInitial(string paragraph, out string initial, out string rest)
        {
            initial = rest = null;
            if (string.IsNullOrEmpty(paragraph) || paragraph.StartsWith("[[") || !char.IsLetterOrDigit(paragraph[0])) return false;
            initial = paragraph.Substring(0, 1);
            rest = paragraph.Substring(1);
            return true;
        }

        // The paragraph of place `at`: a glossary word tapped in it opens its definition under that place
        private Label NewParagraph(int at)
        {
            var label = new Label { enableRichText = true };
            label.AddToClassList("card-text__paragraph");
            label.AddToClassList(_paragraphClass);
            label.RegisterCallback<PointerUpLinkTagEvent>(evt => ToggleTerm(evt.linkID, evt.linkText, at));
            // - a flowing drop cap is cut again when the room beside its letter changes (only the beside part's width counts)
            label.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (label == FlowBeside) Flow();
            });
            return label;
        }
    }
}
