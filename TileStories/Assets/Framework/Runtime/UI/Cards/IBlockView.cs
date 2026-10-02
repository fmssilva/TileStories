using System.Reflection;
using UnityEngine.UIElements;

namespace TileStories
{
    // What one block view is handed when it is bound: the POI, the wall's taxonomy (names of keys), the
    // resolved variant, the languages to read localized fields in and where to load media from (_3.1 section 3).
    // Media is the view's OWN scope: whatever it loads is released for it when it is unbound (BlockStackView).
    public sealed class BlockBindContext
    {
        // Every public instance field, cached once: ForBlock copies through this list by reflection instead of
        // naming each field by hand, so a field added here later reaches every block automatically (_3.1 10A-fix:
        // BlockStackView.Bind's old hand-written copy silently dropped Preview when it was added in 10A.2b.3).
        private static readonly FieldInfo[] Fields = typeof(BlockBindContext).GetFields(BindingFlags.Public | BindingFlags.Instance);

        public POIData Poi;
        public WallConfigData Taxonomy;
        // The wall's card settings as the card runs them NOW (the Block Library's wall-wide defaults, the container). Not Taxonomy.card_settings:
        // on the running wall the taxonomy is the session's trimmed search copy, which carries no card settings. Null where the caller has none
        // (every default then reads as the kind's own)
        public CardSettings Settings;
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
        // and outlives the card
        public ICardAudio Audio;
        // The card's ONE video owner (_3.1 step 9B), beside the audio one and for the same reasons: what a video block and a header loop ask
        // to play, and whose picture they draw. Null where the caller has none (a video then shows its poster and plays nothing)
        public ICardVideo Video;
        // The card's ONE preview owner (_3.1 step 10A.2), beside audio/video: what a model_3d / panorama_360 block (or a
        // header model_turntable) asks for its slot (by its own block key) and renders through. Null where the caller has
        // none (a block then shows its fallback picture). Unlike Audio/Video, several slots can be live at once: this is
        // a slot manager, not a single current owner (see ICardPreview)
        public ICardPreview Preview;
        // The card's ONE AR placement owner (_3.1 step 10B.1), beside audio/video/preview: what a place_in_ar block asks to place its model
        // in the world at its POI (one model at a time) and whether the wall is localised. Null where the caller has none (the block's
        // button then stays disabled)
        public ICardArPlacement ArPlacement;
        // Show still pictures instead of motion (card_settings.container.reduce_motion today; the visitor's own setting joins it with _3.3):
        // a header loop shows its poster only
        public bool ReduceMotion;

        // The registered service of type T, or null when there is none: a kind that needs a service treats null as "nothing to show"
        public T Service<T>() where T : class => Services?.Get<T>();

        // A copy of this (the STACK's own context) for one bound block: `media` is that block's own scoped media source
        // and `variant` is its resolved look -- the two things that actually differ per block in BlockStackView.Bind's
        // loop. Every other field reaches the copy through Fields above, so it can never again fall out of sync with a
        // field added here later (see BlockBindContextTests).
        public BlockBindContext ForBlock(IMediaSource media, string variant)
        {
            var copy = new BlockBindContext();
            foreach (var field in Fields) field.SetValue(copy, field.GetValue(this));
            copy.Media = media;
            copy.Variant = variant;
            return copy;
        }
    }

    // What a block may ask of its card (_3.1 section 3): lower the card so the selected POI's marker shows on the wall;
    // open the full-screen view over the card (Tier 2: the gallery's lightbox); select another POI of the wall and where
    // the visitor stands (Tier 2 group B: wall_locator, related). Tier 4 adds audio with its first block.
    public interface IBlockHost
    {
        // Where the card rests now (peek / half / full): a live preview that animates on its own pauses while
        // SheetStopRule.RevealsBlocks is false, since its stage is hidden under the sheet
        SheetStopRule.Stop Stop { get; }

        void ShowOnWall();

        // Scroll the card to its top and lower it to its peek, so what shows is the header and nothing cut off from further down the card
        // (place_in_ar, once its model stands)
        void ShowHeaderAtPeek();

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
