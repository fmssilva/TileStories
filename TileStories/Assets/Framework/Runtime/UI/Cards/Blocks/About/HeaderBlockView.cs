using UnityEngine.UIElements;

namespace TileStories
{
    // The header block (_3.1 Tier 1): the title and the category chip ("category - level", PoiSubtitle) in its
    // TOP part -- all the peek stop shows -- and the subtitle below it. Variant "compact" is the top part only.
    // Only classes here; PoiCard.uss holds every size and colour.
    public sealed class HeaderBlockView : IBlockView
    {
        public VisualElement Root { get; }
        // The part the peek stop shows (PoiCardSheetView measures its bottom edge)
        public VisualElement PeekPart { get; }

        private readonly Label _title;
        private readonly Label _chip;
        private readonly Label _subtitle;

        public HeaderBlockView()
        {
            Root = new VisualElement { name = "card-header" };
            Root.AddToClassList("card-header");
            PeekPart = new VisualElement { name = "card-header-top" };
            PeekPart.AddToClassList("card-header__top");
            _title = new Label { name = "card-header-title" };
            _title.AddToClassList("card-header__title");
            _chip = new Label { name = "card-header-chip" };
            _chip.AddToClassList("card-chip");
            _subtitle = new Label { name = "card-header-subtitle" };
            _subtitle.AddToClassList("card-header__subtitle");
            PeekPart.Add(_title);
            PeekPart.Add(_chip);
            Root.Add(PeekPart);
            Root.Add(_subtitle);
        }

        public string TitleText => _title.text;
        public string ChipText => _chip.text;
        public string SubtitleText => _subtitle.text;
        public bool SubtitleShown => _subtitle.style.display != DisplayStyle.None;

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string title = read.Text(BlockStackBuilder.HeaderTitleField);
            _title.text = title.Length > 0 ? title : context.Poi?.name ?? "";

            _chip.text = PoiSubtitle.Of(context.Poi, context.Taxonomy);
            _chip.style.display = _chip.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            bool compact = context.Variant == BuiltInBlocks.HeaderCompact;
            Root.EnableInClassList("card-header--compact", compact);
            _subtitle.text = read.Text(BlockStackBuilder.HeaderSubtitleField);
            _subtitle.style.display = !compact && _subtitle.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Unbind()
        {
            _title.text = "";
            _chip.text = "";
            _subtitle.text = "";
        }
    }
}
