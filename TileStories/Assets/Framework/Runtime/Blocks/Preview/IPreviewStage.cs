using System;
using UnityEngine;

namespace TileStories
{
    // The seam CardPreviewService drives to actually build something and render it (_3.1 step 10A.2): CardPreviewStage
    // implements this for real (a disabled camera far from the wall, one RenderTexture per slot, glTFast for a model,
    // EquirectRule's own sphere for a panorama); tests use a fake that never touches the scene. Kept separate from
    // ICardPreview/ICardPreviewSlot (the block-facing contract) the same way IAudioOutput sits under CardAudioService.
    public interface IPreviewStage
    {
        // Build whatever `kind` needs from `path` (loaded through `media`) far from the wall. Calls `onReady` once ready
        // (glTFast's own load may finish later frames) or `onFailed` if the file is missing or the wrong type; either way
        // exactly once. Returns a handle the slot keeps for as long as it is requested.
        IPreviewHandle Load(MediaKind kind, IMediaSource media, string path, Action onReady, Action onFailed);
    }

    // What CardPreviewService keeps per requested slot: the RenderTexture a view reads pixels from, one render call for
    // after a real gesture (never continuous), and Release, which must leave no GameObject, mesh or texture behind.
    public interface IPreviewHandle
    {
        RenderTexture Texture { get; }

        // The stage the view draws into just resized (or is asking for the first time): recreate the RenderTexture at
        // this pixel size (a no-op if unchanged) so its aspect matches what the card actually shows, and refit the
        // camera distance to it. Both dimensions must be > 0; the view guards that.
        void Resize(int width, int height);

        // Render one frame now, the model/sphere looking the way `turntable`/`panorama` (whichever this kind uses) says
        void RenderNow(TurntableState turntable, PanoramaViewState panorama);

        void Release();
    }
}
