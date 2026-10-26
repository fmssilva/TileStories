using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The view of every COMPOSED kind (_3.3 R14.3, R17.1; the story first): one view, two surfaces, chosen by where it is bound.
    //   - on the card: the PREVIEW -- ElementPreviewRule's rows (Automatic: the Page's picture, eyebrow + title, first text, every text at its
    //     smallest size; Custom: the listed rows, a linked row in its own style), capped at --ts-preview-max of the card's full height
    //     (IBlockHost.FullHeight, the measured full stop: a USS percentage does not resolve inside the ScrollView, R10.2) and cut with a
    //     fade into the surface when it is taller, then the Open Row. A tap anywhere on it opens the page (R17.4)
    //   - in the full view (BlockBindContext.FullView): the PAGE -- every row in order
    // Each row is drawn by its element's component (ElementRegistry, pooled per kind); a row of an element with no component yet (the media
    // elements before S2b) draws nothing for a visitor and a "not built yet" plate in the developer's gallery (BlockBindContext.DevGallery);
    // a broken reference draws nothing. The rhythm between rows is the stylesheet's, by pair: each row wears the class of the kind before it
    // (ts-el--after-heading), USS having no sibling selector. Host-agnostic (R15.2): it asks the host for the full height and the full view.
    public sealed class ComposedBlockView : IBlockView
    {
        public const string RootClass = "ts-composed";
        public const string AfterClassPrefix = "ts-el--after-";
        public const string FirstClass = "ts-el--first";
        public const string UnbuiltClass = "ts-el-unbuilt";
        // The preview's cap: a share of the card's full height (the view's own token, Components.uss)
        private static readonly CustomStyleProperty<float> PreviewMaxProperty = new("--ts-preview-max");

        public VisualElement Root { get; }
        // The preview's parts: the tappable whole, the clip that caps the rows, the rows, the fade over the cut, the Open Row
        public VisualElement Preview { get; }
        public VisualElement Clip { get; }
        public VisualElement Rows { get; }
        public VisualElement Fade { get; }
        public OpenRowView OpenRow { get; }
        // The page's rows (shown only in the full view)
        public VisualElement Page { get; }

        // Whether this binding draws the page (the full view) or the preview (the card)
        public bool IsPage { get; private set; }
        // The bound rows in order: the row on its surface, the content it shows, its component (null for an unbuilt plate)
        public IReadOnlyList<(ElementRow Row, ElementRow Content, IElementComponent Component)> Shown => _shown;
        // The preview's cap in panel units (NaN while there is none: no host height yet, or the page) and whether the rows were cut at it
        public float Cap { get; private set; } = float.NaN;
        public bool IsCut { get; private set; }
        // The pool of this view's components (what a test reads to see reuse)
        public ElementComponentPool Components { get; }

        private readonly BlockRegistry _blocks;
        private readonly ElementRegistry _elements;
        private readonly List<(ElementRow Row, ElementRow Content, IElementComponent Component)> _shown = new();
        private readonly List<(string Kind, IElementComponent Component)> _bound = new();
        private readonly ElementPool<Label> _plates;
        private BlockInstanceData _instance;
        private BlockBindContext _context;
        private float _previewMax;

        public ComposedBlockView(BlockRegistry blocks, ElementRegistry elements)
        {
            _blocks = blocks;
            _elements = elements;
            Components = new ElementComponentPool(elements);
            _plates = new ElementPool<Label>(_ => NewPlate(), plate => plate.RemoveFromHierarchy());
            Root = new VisualElement { name = "ts-composed" };
            Root.AddToClassList("card-block");
            Root.AddToClassList(RootClass);

            Preview = new VisualElement { name = "ts-composed-preview" };
            Preview.AddToClassList(RootClass + "__preview");
            Clip = new VisualElement { name = "ts-composed-clip", pickingMode = PickingMode.Ignore };
            Clip.AddToClassList(RootClass + "__clip");
            Rows = new VisualElement { name = "ts-composed-rows", pickingMode = PickingMode.Ignore };
            Rows.AddToClassList(RootClass + "__rows");
            Fade = new VisualElement { name = "ts-composed-fade", pickingMode = PickingMode.Ignore };
            Fade.AddToClassList(RootClass + "__fade");
            Clip.Add(Rows);
            Clip.Add(Fade);
            OpenRow = new OpenRowView();
            Preview.Add(Clip);
            Preview.Add(OpenRow);
            // - the whole preview is the target: the picture, the words, the Open Row (R13.1's cover button, R17.4)
            Preview.AddManipulator(new Clickable(OpenPage));

            Page = new VisualElement { name = "ts-composed-page", pickingMode = PickingMode.Ignore };
            Page.AddToClassList(RootClass + "__page");
            Root.Add(Preview);
            Root.Add(Page);

            Root.RegisterCallback<CustomStyleResolvedEvent>(_ =>
            {
                _previewMax = Root.customStyle.TryGetValue(PreviewMaxProperty, out float share) ? share : 0f;
                Fit();
            });
            // - the cap follows the rows' own height and the card's measured height: never the clip's (it is what the cap sets)
            Rows.RegisterCallback<GeometryChangedEvent>(_ => Fit());
            OpenRow.RegisterCallback<GeometryChangedEvent>(_ => Fit());
            Root.RegisterCallback<GeometryChangedEvent>(_ => Fit());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _instance = instance;
            _context = context;
            IsPage = context.FullView;
            Preview.style.display = IsPage ? DisplayStyle.None : DisplayStyle.Flex;
            Page.style.display = IsPage ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList(RootClass + "--page", IsPage);

            var scope = ElementScope.Of(context.Poi, instance, _blocks);
            string familyToken = FamilyTokenOf(scope.HolderKind);
            if (IsPage) DrawPage(scope, familyToken);
            else DrawPreview(scope, familyToken);
            Fit();
        }

        public void Unbind()
        {
            foreach (var (kind, component) in _bound)
            {
                component.Unbind();
                foreach (string c in new List<string>(component.Root.GetClasses()))
                    if (c.StartsWith(AfterClassPrefix) || c == FirstClass) component.Root.RemoveFromClassList(c);
                component.Root.RemoveFromHierarchy();
                Components.Return(kind, component);
            }
            _bound.Clear();
            _shown.Clear();
            _plates.ReleaseAll();
            _instance = null;
            _context = null;
            IsCut = false;
            Cap = float.NaN;
            Clip.style.maxHeight = StyleKeyword.Null;
        }

        // Every Page row, in order, each in its own style
        private void DrawPage(ElementScope scope, string familyToken)
        {
            foreach (var row in BlockSurfaces.PageRows(scope.Holder, scope.HolderKind))
            {
                var content = ElementRefRule.ContentOf(row, scope);
                if (content.IsBroken) continue;
                var styleRow = ReferenceEquals(content.Target, row) ? null : row;
                AddRow(Page, new ElementRow(row), new ElementRow(content.Target), styleRow, ElementSurface.Page, compact: false, familyToken);
            }
        }

        // The preview's rows, then the Open Row
        private void DrawPreview(ElementScope scope, string familyToken)
        {
            var preview = ElementPreviewRule.Of(scope, _elements);
            bool compact = preview.Mode == ElementOptions.PreviewAutomatic;
            foreach (var entry in preview.Entries)
            {
                if (entry.Source == ElementPreviewRule.Source.Broken || entry.Content == null) continue;
                var styleRow = entry.Source == ElementPreviewRule.Source.Linked ? entry.Row : null;
                AddRow(Rows, new ElementRow(entry.Row), new ElementRow(entry.Content), styleRow, ElementSurface.Preview, compact, familyToken);
            }
            ShowOpenRow(scope);
        }

        private void AddRow(VisualElement into, ElementRow row, ElementRow content, BlockItemData styleRow, ElementSurface surface, bool compact,
            string familyToken)
        {
            string kind = content.Kind;
            if (!_elements.TryGet(kind, out var element)) return;
            var component = Components.Take(kind);
            if (component == null)
            {
                // - a visitor never sees a hole in the story; the developer's gallery sees what is missing
                if (!_context.DevGallery) return;
                var plate = _plates.Take();
                plate.text = element.DisplayName + " -- not built yet";
                into.Add(plate);
                _shown.Add((row, content, null));
                return;
            }
            string before = _shown.Count == 0 ? null : _shown[_shown.Count - 1].Content.Kind;
            component.Bind(content, new ElementBindContext
            {
                Block = _context, Element = element, StyleRow = styleRow, Surface = surface, Compact = compact, FamilyToken = familyToken,
            });
            if (before == null) component.Root.AddToClassList(FirstClass);
            else component.Root.AddToClassList(AfterClassPrefix + before.Replace('_', '-'));
            into.Add(component.Root);
            _bound.Add((kind, component));
            _shown.Add((row, content, component));
        }

        // The Open Row: the block's own words (its Component Overrides row) else its kind's, joined to the reading time; its emphasis the
        // block's override else the wall's; still under Reduce Motion
        private void ShowOpenRow(ElementScope scope)
        {
            var overriding = BlockSurfaces.OverrideOf(_instance, ElementOptions.ComponentOpenRow);
            var strings = _context.Strings;
            string words = overriding.IsOn ? overriding.Words(_context.Language, _context.FallbackLanguage) : "";
            if (words.Length == 0 && scope.HolderKind?.OpenWordsKey != null && strings != null) words = strings.Get(scope.HolderKind.OpenWordsKey);
            int minutes = ElementRules.Minutes(scope, _elements, _context.Language, _context.FallbackLanguage);
            string line = words;
            if (minutes > 0 && strings != null)
            {
                string time = string.Format(strings.Get(CardStrings.Keys.NavContentsMinutes), minutes);
                line = words.Length > 0 ? string.Format(strings.Get(CardStrings.Keys.OpenRowMeta), words, time) : time;
            }
            string emphasis = overriding.IsOn ? overriding.Emphasis : _context.Settings?.components?.open_row_emphasis;
            OpenRow.Show(line, emphasis, _context.ReduceMotion);
            OpenRow.style.display = line.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Cap the preview's rows at --ts-preview-max of the card's full height, less the Open Row: cut with the fade when they are taller,
        // whole and with no fade when they fit
        private void Fit()
        {
            if (_context == null || IsPage) return;
            float full = _context.Host?.FullHeight ?? 0f;
            float rows = Rows.layout.height;
            if (!(full > 0f) || !(_previewMax > 0f) || float.IsNaN(rows))
            {
                Cap = float.NaN;
                SetCut(false, 0f);
                return;
            }
            Cap = full * _previewMax;
            float openRow = OpenRow.resolvedStyle.display == DisplayStyle.None ? 0f
                : OpenRow.layout.height + OpenRow.resolvedStyle.marginTop + OpenRow.resolvedStyle.marginBottom;
            if (float.IsNaN(openRow)) return;
            float room = Mathf.Max(0f, Cap - openRow);
            // - half a pixel: a layout that lands exactly on the cap is not cut
            SetCut(rows > room + 0.5f, room);
        }

        private void SetCut(bool cut, float room)
        {
            IsCut = cut;
            Clip.style.maxHeight = cut ? room : new StyleLength(StyleKeyword.Null);
            Fade.style.display = cut ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // A tap on the preview: the block's page in the card's full view
        private void OpenPage()
        {
            if (_context == null || IsPage || _instance == null) return;
            _context.Host?.OpenFullView(_instance.key);
        }

        // The colour token of the kind's family ("" when the kind or family is unknown: the Family tone then keeps the stylesheet's colour)
        private string FamilyTokenOf(BlockKindDefinition kind) =>
            kind != null && _blocks.TryGetFamily(kind.Family, out var family) ? family.ColorToken : "";

        private static Label NewPlate()
        {
            var plate = new Label { pickingMode = PickingMode.Ignore };
            plate.AddToClassList(UnbuiltClass);
            return plate;
        }
    }
}
