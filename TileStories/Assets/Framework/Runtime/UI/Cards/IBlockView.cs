using UnityEngine.UIElements;

namespace TileStories
{
    // What one block view is handed when it is bound: the POI, the wall's taxonomy (names of keys), the
    // resolved variant, the languages to read localized fields in and where to load media from (_3.1 section 3).
    // Media is the view's OWN scope: whatever it loads is released for it when it is unbound (BlockStackView).
    public sealed class BlockBindContext
    {
        public POIData Poi;
        public WallConfigData Taxonomy;
        public string Variant;
        public string Language;
        public string FallbackLanguage;
        public IMediaSource Media;
        // The card's UI texts in this card's language (framework defaults + the wall's own wording)
        public CardStrings Strings;
        // The wall's marker look: blocks that show marker facts (status) resolve them through the markers' own rules
        public MarkerVisualSettings MarkerLook;
        // The wall's glossary in this card's language: the definitions of [[terms]] in long texts
        public CardGlossary Glossary;
        // What a block may ask of the card it sits on (the sheet)
        public IBlockHost Host;
    }

    // What a block may ask of its card (_3.1 section 3). Today: lower the card so the selected POI's marker shows on the
    // wall. Tier 2-4 add their requests here (open a takeover, play audio) with their first block.
    public interface IBlockHost
    {
        void ShowOnWall();
    }

    // One block kind's view (_3.1 section 3): built once, then bound and unbound as the visitor switches POIs
    // (views are pooled per kind -- never instantiate-and-destroy per tap). Styled only through USS classes.
    public interface IBlockView
    {
        VisualElement Root { get; }

        // Show this instance (the view reads it through a BlockFieldReader)
        void Bind(BlockInstanceData instance, BlockBindContext context);

        // Forget the bound instance (its media scope is released right after, by the stack)
        void Unbind();
    }
}
