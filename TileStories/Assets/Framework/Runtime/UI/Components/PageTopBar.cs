using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The Page Chrome's top bar (_3.3 R17.5, S2c): a Back chevron in a 44 pt round target and the breadcrumb on ONE line -- "point > section >
    // page", the page's own name last and bold ("Castle > Media > 360"); a page with a big title of its own (a story) leaves its name out, the
    // trail is then the context alone ("Ribeira Palace > About", ShowName). The section is a tap target when the page belongs to
    // one (the host decides what the tap does). When the line is too long the MIDDLE part is cut with an ellipsis first, then the point's,
    // the name last (PageCrumbFitRule). The Back arrow draws no word: its tooltip is its accessible name. Host-agnostic (R15.2): the
    // callbacks are the host's, nothing here names the sheet or the full view. Styled by Components.uss.
    public sealed class PageTopBar : VisualElement
    {
        public const string RootClass = "ts-page-bar";

        public Button Back { get; }
        // The breadcrumb's row, its three parts and the section's words
        public VisualElement Crumb { get; }
        public Label Point { get; }
        public Button Section { get; }
        public Label SectionWords { get; }
        public Label Name { get; }
        // Whether the breadcrumb is drawn at all (the chrome's wall / block choice); off: the Back arrow stands alone
        public bool BreadcrumbShown { get; private set; } = true;
        public bool SectionShown => Section.style.display.value != DisplayStyle.None;
        // The page's name says nothing the section does not (a block with no heading of its own is named by its kind's words, often the
        // section's: "Listen > Listen"): it is left out
        public bool NameRepeatsSection => SectionShown && string.Equals(Name.text?.Trim(), SectionWords.text?.Trim(), StringComparison.OrdinalIgnoreCase);
        // Whether the trail ends with the page's own name: off for a page that shows a big title of its own (a story), whose trail then
        // carries only the context, "point > section" (the host decides)
        public bool NameShown { get; private set; } = true;

        // The whole breadcrumb as a visitor reads it ("St George's Castle > Media > 360"); empty while it is hidden
        public string CrumbText
        {
            get
            {
                if (!BreadcrumbShown) return "";
                var parts = new List<string>();
                foreach (var part in new[] { Point.text, SectionShown ? SectionWords.text : "", NameRepeatsSection || !NameShown ? "" : Name.text })
                    if (!string.IsNullOrEmpty(part)) parts.Add(part);
                return string.Join(" > ", parts);
            }
        }

        private readonly VisualElement _firstArrow;
        private readonly VisualElement _secondArrow;

        public PageTopBar(Action onBack, Action onSection)
        {
            name = "ts-page-bar";
            AddToClassList(RootClass);
            Back = new Button(onBack) { name = "ts-page-back" };
            Back.AddToClassList(RootClass + "__back");
            var chevron = CardIcons.CreateVector(CardIcons.Shape.Chevron);
            chevron.pickingMode = PickingMode.Ignore;
            chevron.AddToClassList(RootClass + "__back-chevron");
            Back.Add(chevron);

            Crumb = new VisualElement { name = "ts-page-crumb" };
            Crumb.AddToClassList(RootClass + "__crumb");
            Point = Part(RootClass + "__point");
            _firstArrow = Arrow();
            Section = new Button(onSection) { name = "ts-page-crumb-section" };
            Section.AddToClassList(RootClass + "__section");
            SectionWords = Part(RootClass + "__section-words");
            Section.Add(SectionWords);
            _secondArrow = Arrow();
            Name = Part(RootClass + "__name");
            foreach (var part in new VisualElement[] { Point, _firstArrow, Section, _secondArrow, Name }) Crumb.Add(part);
            Add(Back);
            Add(Crumb);

            // - the widths follow the room the row has: measured again whenever the row's size changes (the phone turns, a longer name)
            Crumb.RegisterCallback<GeometryChangedEvent>(_ => Fit());
            SetTrail("", null, "");
        }

        // The tooltip and accessible name of the Back arrow
        public void SetBackName(string words)
        {
            Back.tooltip = words ?? "";
        }

        // Show or hide the breadcrumb beside the Back arrow
        public void ShowBreadcrumb(bool shown)
        {
            BreadcrumbShown = shown;
            Crumb.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // The trail: the point's name, the section the page belongs to (null / empty: none), the page's own name
        public void SetTrail(string point, string sectionWords, string name)
        {
            Point.text = point ?? "";
            bool section = !string.IsNullOrEmpty(sectionWords);
            SectionWords.text = section ? sectionWords : "";
            Section.tooltip = SectionWords.text;
            Section.style.display = section ? DisplayStyle.Flex : DisplayStyle.None;
            Name.text = name ?? "";
            Parts();
        }

        // The page's own name alone changes (a page turned within the same card)
        public void SetName(string name)
        {
            Name.text = name ?? "";
            Parts();
        }

        // Whether the trail ends with the page's name (off: the page carries a big title of its own)
        public void ShowName(bool shown)
        {
            NameShown = shown;
            Parts();
        }

        // Each part and arrow only where there is something on both sides of it
        private void Parts()
        {
            bool point = Point.text.Length > 0, section = SectionShown, name = NameShown && Name.text.Length > 0 && !NameRepeatsSection;
            Point.style.display = point ? DisplayStyle.Flex : DisplayStyle.None;
            Name.style.display = name ? DisplayStyle.Flex : DisplayStyle.None;
            _firstArrow.style.display = point && (section || name) ? DisplayStyle.Flex : DisplayStyle.None;
            _secondArrow.style.display = section && name ? DisplayStyle.Flex : DisplayStyle.None;
            Fit();
        }

        // Give each part the width PageCrumbFitRule says: whole while the line fits, the middle part cut first
        private void Fit()
        {
            float room = Crumb.layout.width;
            if (!BreadcrumbShown || float.IsNaN(room) || room <= 0f) return;
            float arrows = ArrowWidth(_firstArrow) + ArrowWidth(_secondArrow);
            var natural = new float[3];
            var floor = new float[3];
            natural[PageCrumbFitRule.Point] = Point.style.display.value == DisplayStyle.None ? 0f : Width(Point, Point.text);
            natural[PageCrumbFitRule.Section] = SectionShown ? Mathf.Max(Width(SectionWords, SectionWords.text) + Padding(Section), Section.resolvedStyle.minWidth.value) : 0f;
            natural[PageCrumbFitRule.Name] = Name.style.display.value == DisplayStyle.None ? 0f : Width(Name, Name.text);
            floor[PageCrumbFitRule.Point] = Width(Point, "...");
            // - the section is a touch target: cut, it never gets narrower than its own minimum width (the Back arrow's 44)
            floor[PageCrumbFitRule.Section] = Mathf.Max(Width(SectionWords, "...") + Padding(Section), Section.resolvedStyle.minWidth.value);
            floor[PageCrumbFitRule.Name] = Width(Name, "...");
            var widths = PageCrumbFitRule.Widths(natural, room - arrows, floor);
            Apply(Point, widths[PageCrumbFitRule.Point]);
            Apply(Section, widths[PageCrumbFitRule.Section]);
            Apply(Name, widths[PageCrumbFitRule.Name]);
        }

        // A part's width: its whole width when nothing was cut (so a later, wider room gives it back), else the cut one
        private static void Apply(VisualElement part, float width)
        {
            var wanted = new StyleLength(Mathf.Ceil(width));
            if (!part.style.width.Equals(wanted) && width > 0f) part.style.width = wanted;
        }

        private static float Width(Label label, string text) =>
            Mathf.Ceil(label.MeasureTextSize(text, 0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined).x) + 1f;

        private static float Padding(VisualElement element) => element.resolvedStyle.paddingLeft + element.resolvedStyle.paddingRight;

        private static float ArrowWidth(VisualElement arrow) =>
            arrow.style.display.value == DisplayStyle.None ? 0f
                : arrow.resolvedStyle.width + arrow.resolvedStyle.marginLeft + arrow.resolvedStyle.marginRight;

        private static Label Part(string cls)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList(RootClass + "__part");
            label.AddToClassList(cls);
            return label;
        }

        // The step between two parts: the card's chevron turned to point onward (a drawing, not a word: nothing to translate)
        private static VisualElement Arrow()
        {
            var arrow = CardIcons.CreateVector(CardIcons.Shape.Chevron);
            arrow.pickingMode = PickingMode.Ignore;
            arrow.AddToClassList(RootClass + "__arrow");
            return arrow;
        }
    }
}
