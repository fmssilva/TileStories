using System;
using UnityEngine.UIElements;

namespace TileStories
{
    // The Page Chrome's foot (_3.3 R17.5, S2c): two cards at the end of a block page, Previous on the left and Next on the right -- the small
    // label ("Previous" / "Next") over the neighbouring page's name, a chevron on the outer side. A card whose neighbour does not exist is
    // not drawn but keeps its place, so Next stays on the right. Each is a whole touch target; its tooltip (and accessible name) reads label
    // and name together. Host-agnostic (R15.2): the host decides what a tap turns to. Styled by Components.uss.
    public sealed class PageFootView : VisualElement
    {
        public const string RootClass = "ts-page-foot";

        public Button Previous { get; }
        public Button Next { get; }
        public Label PreviousLabel { get; }
        public Label PreviousName { get; }
        public Label NextLabel { get; }
        public Label NextName { get; }

        private Action _onPrevious;
        private Action _onNext;

        public PageFootView()
        {
            name = "ts-page-foot";
            AddToClassList(RootClass);
            Previous = new Button(() => _onPrevious?.Invoke()) { name = "ts-page-previous" };
            Next = new Button(() => _onNext?.Invoke()) { name = "ts-page-next" };
            Previous.AddToClassList(RootClass + "__card");
            Next.AddToClassList(RootClass + "__card");
            Next.AddToClassList(RootClass + "__card--next");
            PreviousLabel = Words(RootClass + "__label");
            PreviousName = Words(RootClass + "__name");
            NextLabel = Words(RootClass + "__label");
            NextName = Words(RootClass + "__name");
            var back = Chevron(RootClass + "__chevron--previous");
            var forward = Chevron(RootClass + "__chevron--next");
            Previous.Add(back);
            Previous.Add(Column(PreviousLabel, PreviousName, false));
            Next.Add(Column(NextLabel, NextName, true));
            Next.Add(forward);
            Add(Previous);
            Add(Next);
        }

        // Fill the two cards: a null name = no neighbour on that side (the card keeps its place, undrawn)
        public void Show(string previousLabel, string previousName, Action onPrevious, string nextLabel, string nextName, Action onNext)
        {
            _onPrevious = onPrevious;
            _onNext = onNext;
            Fill(Previous, PreviousLabel, PreviousName, previousLabel, previousName);
            Fill(Next, NextLabel, NextName, nextLabel, nextName);
        }

        private static void Fill(Button card, Label label, Label name, string words, string neighbour)
        {
            bool there = neighbour != null;
            card.visible = there;
            card.pickingMode = there ? PickingMode.Position : PickingMode.Ignore;
            label.text = words ?? "";
            name.text = neighbour ?? "";
            // - the tooltip is the accessible name: both lines read as one ("Previous: The opera ruins")
            card.tooltip = there ? (string.IsNullOrEmpty(words) ? neighbour : words + ": " + neighbour) : "";
        }

        private static Label Words(string cls)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList(cls);
            return label;
        }

        private static VisualElement Column(Label label, Label name, bool alignEnd)
        {
            var column = new VisualElement { pickingMode = PickingMode.Ignore };
            column.AddToClassList(RootClass + "__column");
            column.EnableInClassList(RootClass + "__column--end", alignEnd);
            column.Add(label);
            column.Add(name);
            return column;
        }

        private static VisualElement Chevron(string cls)
        {
            var chevron = CardIcons.CreateVector(CardIcons.Shape.Chevron);
            chevron.pickingMode = PickingMode.Ignore;
            chevron.AddToClassList(RootClass + "__chevron");
            chevron.AddToClassList(cls);
            return chevron;
        }
    }
}
