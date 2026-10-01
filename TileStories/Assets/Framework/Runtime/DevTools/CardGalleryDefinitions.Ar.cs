using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    public static partial class CardGalleryDefinitions
    {
        private static void AddShowOnWall(List<Entry> list)
        {
            System.Action<WallConfigData> neighbours = wall =>
            {
                wall.pois.Add(WallPoi("near_a", "North Tower", new Vector3(-1f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_b", "South Tower", new Vector3(2f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_c", "The chapel of Saint George with its bell tower and the old cemetery", new Vector3(3f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_d", "Too Far To Be Named", new Vector3(30f, 0f, 0f), null));
            };
            // - three neighbours whose card titles are all long: the chips wrap
            System.Action<WallConfigData> longNeighbours = wall =>
            {
                wall.pois.Add(WallPoi("near_l1", "The Royal Palace of the Kings of Portugal and of the Algarves, before the earthquake", new Vector3(-1f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_l2", "The chapel of Saint George with its bell tower and the old cemetery", new Vector3(2f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_l3", "The long arcade with its many arches and the market that once stood under it", new Vector3(3f, 0f, 0f), null));
            };
            foreach (var variant in BuiltInBlocks.ShowOnWall.Variants)
            {
                // - with_neighbours: only the nearest few are named (the fourth point is far away and is not); button: no neighbours drawn
                list.Add(new Entry(BuiltInBlocks.ShowOnWallKind, variant, "short", ShowOnWallBlock(variant), null, neighbours));
                if (variant == BuiltInBlocks.ShowOnWallWithNeighbours)
                    list.Add(new Entry(BuiltInBlocks.ShowOnWallKind, variant, "long", ShowOnWallBlock(variant), null, longNeighbours));
            }
        }

        private static BlockInstanceData ShowOnWallBlock(string variant) =>
            new() { key = QuizBlockKey, kind = BuiltInBlocks.ShowOnWallKind, variant = variant };

        // place_in_ar (_3.1 step 10B.3): the Framework's own model with every default (real size, 10 cm out); and an authored long Button
        // Label with the Height scale. The localised / not-localised / placed states are the harness's ArWall and a real tap
        // (CardGalleryTests.PlaceInAr_...), not entries: the same block draws all three.
        private static void AddPlaceInAr(List<Entry> list)
        {
            list.Add(new Entry(BuiltInBlocks.PlaceInArKind, BuiltInBlocks.PlaceInArButton, "short", PlaceInArBlock(null, null)));
            list.Add(new Entry(BuiltInBlocks.PlaceInArKind, BuiltInBlocks.PlaceInArButton, "long",
                PlaceInArBlock("Stand the tiled arch here in front of you, life size, and walk around it", ArPlacementRule.ScaleHeightCm)));
        }

        private static BlockInstanceData PlaceInArBlock(string label, string scale)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.PlaceInArKind, variant = BuiltInBlocks.PlaceInArButton };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArModelField, asset = MediaPathRule.PathForDefaultKey("azulejo_arch") });
            if (label != null) block.fields.Add(Text(BuiltInBlocks.PlaceInArLabelField, label));
            if (scale != null)
            {
                block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArScaleField, value = scale });
                block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArHeightField, number = 45f });
            }
            return block;
        }


    }
}
