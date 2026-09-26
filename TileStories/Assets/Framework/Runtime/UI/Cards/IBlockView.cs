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
