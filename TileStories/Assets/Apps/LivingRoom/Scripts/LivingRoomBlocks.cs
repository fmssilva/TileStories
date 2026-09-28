using UnityEngine;

namespace TileStories.LivingRoom
{
    // The living room app's plug-in to the POI Detail Card (_3.1 step 11): its own block kind, the service that kind asks for and (step
    // 11-fix) the visitor words that kind needs, added through the Framework's PUBLIC registries. Nothing in Framework/ names this assembly
    // (this one references the Framework, never the reverse), yet the card shows the kind, the Editor lists it in the Block Library and
    // "+ Add block", a POI can use it, and Detail Card > Card Texts lists the app's words so a wall can reword them.
    public static class LivingRoomBlocks
    {
        // At runtime start, before the first scene loads: the wall's card host reads all three registries when it opens a card
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterAtRuntimeStart() => Register(BlockRegistry.Shared, CardServices.Shared, CardStringSources.Shared);

#if UNITY_EDITOR
        // In the Editor after every script reload: the POI Editor window (Block Library, + Add block, Card Content, Card Texts) needs the kind
        // and the words in Edit Mode too, with no Play Mode
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterInTheEditor()
        {
            Register(BlockRegistry.Shared, CardServices.Shared, CardStringSources.Shared);
            // - right after a reload the asset database may not have the table yet: ask once more when the editor is idle
            if (!CardStringSources.Shared.Has(LivingRoomCardTexts.AppName))
                UnityEditor.EditorApplication.delayCall += () => Register(BlockRegistry.Shared, CardServices.Shared, CardStringSources.Shared);
        }
#endif

        // Add the app's kind, service and words to these registries; asking twice is harmless (Play Mode after a script reload runs both
        // entry points above, and Unity may keep statics between Play Mode runs)
        public static void Register(BlockRegistry blocks, CardServices services, CardStringSources strings)
        {
            if (!blocks.TryGet(SizeComparisonBlock.Kind, out _))
                blocks.Register(SizeComparisonBlock.Definition, () => new SizeComparisonBlockView());
            if (!services.Has<IFamiliarObjects>())
                services.Add<IFamiliarObjects>(new FamiliarObjects());
            if (strings.Has(LivingRoomCardTexts.AppName)) return;
            var table = Resources.Load<CardStringTable>(LivingRoomCardTexts.TableResourcePath);
            if (table != null) strings.Add(LivingRoomCardTexts.AppName, table);
            else Debug.LogWarning("[LivingRoom] card texts not found at Resources/" + LivingRoomCardTexts.TableResourcePath + ": the app's block names show as keys");
        }
    }
}
