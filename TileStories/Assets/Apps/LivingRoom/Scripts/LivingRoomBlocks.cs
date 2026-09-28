using UnityEngine;

namespace TileStories.LivingRoom
{
    // The living room app's plug-in to the POI Detail Card (_3.1 step 11): its own block kind and the service that kind asks for,
    // added through the Framework's PUBLIC registries. Nothing in Framework/ names this assembly (this one references the Framework,
    // never the reverse), yet the card shows the kind, the Editor lists it in the Block Library and "+ Add block", and a POI can use it.
    public static class LivingRoomBlocks
    {
        // At runtime start, before the first scene loads: the wall's card host reads both registries when it opens a card
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterAtRuntimeStart() => Register(BlockRegistry.Shared, CardServices.Shared);

#if UNITY_EDITOR
        // In the Editor after every script reload: the POI Editor window (Block Library, + Add block, Card Content) needs the kind
        // in Edit Mode too, with no Play Mode
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterInTheEditor() => Register(BlockRegistry.Shared, CardServices.Shared);
#endif

        // Add the app's kind and service to these registries; asking twice is harmless (Play Mode after a script reload runs both
        // entry points above, and Unity may keep statics between Play Mode runs)
        public static void Register(BlockRegistry blocks, CardServices services)
        {
            if (!blocks.TryGet(SizeComparisonBlock.Kind, out _))
                blocks.Register(SizeComparisonBlock.Definition, () => new SizeComparisonBlockView());
            if (!services.Has<IFamiliarObjects>())
                services.Add<IFamiliarObjects>(new FamiliarObjects());
        }
    }
}
