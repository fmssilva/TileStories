using UnityEngine;

namespace TileStories
{
    // How many minutes a contents list (nav_contents, _3.2 step 11b) shows beside a block, pure: the view loads the clip and draws.
    //   - a kind that reads a clip (BlockKindDefinition.ClipLengthField: audio_guide, video) shows the clip's own length, rounded UP to whole
    //     minutes (a 40 s clip is "1 min", never "0 min") -- nobody types a time for it
    //   - any other kind shows its Reading Time field (whole minutes, 0 = not given)
    //   - neither: no time (0)
    public static class ReadingTimeRule
    {
        // The minutes to show (0 = none). `clipSeconds` is the loaded clip's length (0 when the kind reads no clip or the clip is missing)
        public static int Minutes(BlockKindDefinition kind, BlockInstanceData instance, float clipSeconds)
        {
            if (kind == null) return 0;
            if (kind.ClipLengthField != null) return FromClip(clipSeconds);
            var field = kind.Field(BlockKindDefinition.MinutesField);
            if (field == null || instance == null) return 0;
            float typed = new BlockFieldReader(instance, null, null).Number(field);
            return Mathf.Max(0, Mathf.RoundToInt(typed));
        }

        // A clip's length in whole minutes, rounded up; 0 for no clip (a missing file, no length)
        public static int FromClip(float clipSeconds) =>
            float.IsNaN(clipSeconds) || clipSeconds <= 0f ? 0 : Mathf.CeilToInt(clipSeconds / 60f);

        // The Asset field whose clip gives the kind's time (null when the kind reads none): what the view loads
        public static BlockFieldDefinition ClipField(BlockKindDefinition kind) =>
            kind?.ClipLengthField != null ? kind.Field(kind.ClipLengthField) : null;
    }
}
