using System;

namespace TileStories
{
    // The card's 3D/360 preview owner (_3.1 step 10A.2): unlike audio/video (one current, at most one plays) a card can
    // show several previews at once -- one per model_3d / panorama_360 block bound in the stack right now -- so this is a
    // SLOT manager, not a single-current owner: each block instance asks for its own slot by a stable key (its block key),
    // gets it loaded once, and gives it back when it unbinds. The card closing releases every slot still held.
    public interface ICardPreview
    {
        // A block bound with this key wants its media loaded (or the existing slot, if already requested for this key
        // and kind/path unchanged). Returns the slot at once; it may still be loading (IsLoading), already ready, or
        // already failed -- Changed fires when a slot's own state moves.
        ICardPreviewSlot Request(string key, MediaKind kind, string path);

        // The block behind `key` unbound (scrolled out of a pooled slot, or the card closed): give back whatever it held
        void Release(string key);

        // The card closed: give back everything, whichever blocks never got to unbind their own key
        void ReleaseAll();

        event Action Changed;
    }

    // One block's own preview: whether it is still loading, failed, or ready with something to render. `Texture` is the
    // RenderTexture the stage renders into on demand (null until ready); a view asks for a render after a real gesture
    // (a drag, a pinch, a gyro reading) through the stage, never every frame.
    public interface ICardPreviewSlot
    {
        string Key { get; }
        MediaKind Kind { get; }
        bool IsLoading { get; }
        bool Failed { get; }
        UnityEngine.RenderTexture Texture { get; }

        // The view's own stage element resized (or is reporting its size for the first time): recreate the texture at
        // this pixel size so its aspect matches the stage, a no-op while loading/failed/released or already this size
        void Resize(int width, int height);

        // Render one frame now (a no-op while loading/failed, or once released): the view calls this after a real
        // gesture moved `turntable`/`panorama` (whichever this slot's kind uses), never every frame on its own
        void RenderNow(TurntableState turntable, PanoramaViewState panorama);
    }
}
