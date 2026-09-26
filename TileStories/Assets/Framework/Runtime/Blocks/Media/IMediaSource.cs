namespace TileStories
{
    // Where a card block gets its media (images, audio, video...) (_3.1 step 5). A block loads LAZILY when it is
    // bound and releases when it is unbound (work plan performance rule 3): nothing a card might show is held in
    // memory until a visitor opens that card. ResourcesMediaSource is today's implementation; Addressables replace
    // it at packaging time (work plan Stage 6) behind this same interface.
    public interface IMediaSource
    {
        // The asset at `path` (a path under the wall's card media folder, extension optional), or null when there
        // is none -- never an exception: a missing file is the block's "media unavailable" state
        T Load<T>(string path) where T : UnityEngine.Object;

        // Give back one Load of `path`; the asset is freed when nothing holds it any more
        void Release(string path);
    }
}
