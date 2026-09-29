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


    }
}
