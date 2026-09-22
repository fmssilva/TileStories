namespace TileStories.Editor
{
    // One domain's way of re-applying its part of the wall config to a running wall.
    // Implemented once per domain (effects now; orientation, LOD... later), never by the window.
    public interface ILivePlayModeApplier
    {
        // Short name used in logs and tests
        string Name { get; }

        // Text that changes exactly when this domain's settings change (compared between pushes)
        string Fingerprint(WallConfigData config);

        // Re-apply this domain's settings from a private copy of the config to the running wall
        void Apply(WallSession session, WallConfigData configCopy);
    }
}
