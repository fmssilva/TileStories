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
        // What the visitor did on the cards on this device (answers, votes, revealed blocks) and where a block reports what
        // the visitor did (_3.1 step 8A). Null where the caller has none: a block then keeps its state in the view only
        public CardLocalState State;
        public ICardEvents Events;
        // The card's other services (_3.1 step 11): what a kind needs beyond the fields above (a poll's IPollResults, an app kind's own).
        // Null where the caller has none: every Service<T>() is then null
        public CardServices Services;
        // The card's ONE audio owner (_3.1 step 9A): what an audio block asks to play, pause or seek, and the mini-player draws. Null where
        // the caller has none (a block then shows its idle state and plays nothing). Not a CardServices entry: it belongs to the card host
        // and outlives the card, and every playing kind (audio now, video in 9B) uses it
        public ICardAudio Audio;

        // The registered service of type T, or null when there is none: a kind that needs a service treats null as "nothing to show"
        public T Service<T>() where T : class => Services?.Get<T>();
    }

    // What a block may ask of its card (_3.1 section 3): lower the card so the selected POI's marker shows on the wall;
    // open the full-screen view over the card (Tier 2: the gallery's lightbox); select another POI of the wall and where
    // the visitor stands (Tier 2 group B: wall_locator, related). Tier 4 adds audio with its first block.
    public interface IBlockHost
    {
        void ShowOnWall();

        // Select another POI of this wall, as a tap on its marker does (SelectionEventBus: the card rebinds to it, the
        // markers, list and zoom-on-select follow)
        void SelectPoi(string poiId);

        // Where the visitor is, in the wall's own frame (the POI positions' frame); false where no viewer is known
        bool TryGetViewer(out UnityEngine.Vector3 wallPosition);

        // Open a web link on the device (its browser or maps app); a link WebLinkRule refuses is never opened
        void OpenUrl(string url);

        // Open the full-screen view: `name` ends the breadcrumb after the card's title, `pageCount` sibling pages starting
        // at `startPage`, each drawn by `drawPage` through the view's own media scope
        void OpenTakeover(string name, int pageCount, int startPage, TakeoverPageDrawer drawPage);
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
