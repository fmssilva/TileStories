using System.Collections.Generic;

namespace TileStories
{
    // The video part of the Phase A gallery list (_3.1 step 9B). Unlike pictures and audio, a VideoClip cannot be made in memory: the
    // gallery's clips are small generated files (CardGalleryVideoGenerator writes them into CardGalleryMedia.VideoFolder), named here
    // by the path a block stores and the file they come from. Any other name is a missing file.
    public static partial class CardGalleryDefinitions
    {
        public readonly struct VideoFile
        {
            public readonly string File;
            public readonly float Seconds;

            public VideoFile(string file, float seconds)
            {
                File = file;
                Seconds = seconds;
            }
        }

        public static readonly IReadOnlyDictionary<string, VideoFile> Videos = new Dictionary<string, VideoFile>
        {
            ["short.mp4"] = new VideoFile("gallery_short.mp4", 12f),
            ["long.mp4"] = new VideoFile("gallery_long.mp4", 95f),
        };

        public const string VideoBlockKey = "block_2";
        public const string VideoTitle = "The castle film";

        // Three chapters of the 12 s clip, written out of time order on purpose (the buttons run in time order)
        public static readonly (string Time, string Label)[] ShortChapters = { ("0:08", "The view"), ("0:00", "Opening"), ("0:04", "The walls") };
        // The 95 s clip's many chapters with long names (they wrap), plus a row with a time that is not one and a row past the clip's end:
        // neither becomes a button
        public static readonly (string Time, string Label)[] LongChapters =
        {
            ("0:00", "Arriving at the gate under the old coat of arms"),
            ("0:12", "The walls and the ten towers along the hill"),
            ("25", "The keep, where the governor lived"),
            ("0:38", "The gardens and the peacocks"),
            ("0:51", "The view over the river and the lower town"),
            ("1:04", "The archaeological site and the Moorish quarter"),
            ("1:17", "The camera obscura on the tower"),
            ("1:30", "Leaving by the south gate"),
            ("later", "A row whose time is not a time"),
            ("2:00", "A row past the end of the clip"),
        };

        // Both looks x (poster, no poster, no captions, long chapters, a clip that is not there) and the takeover display's teaser
        private static void AddVideos(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Video.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.VideoKind, variant, "poster", VideoBlock(variant, "short.mp4", "wide.png", "short.vtt", ShortChapters)));
                list.Add(new Entry(BuiltInBlocks.VideoKind, variant, "noposter", VideoBlock(variant, "short.mp4", null, "short.vtt", ShortChapters)));
                list.Add(new Entry(BuiltInBlocks.VideoKind, variant, "nocaptions", VideoBlock(variant, "short.mp4", "wide.png", null, ShortChapters)));
                list.Add(new Entry(BuiltInBlocks.VideoKind, variant, "longchapters", VideoBlock(variant, "long.mp4", "wide.png", null, LongChapters)));
                list.Add(new Entry(BuiltInBlocks.VideoKind, variant, "unavailable", VideoBlock(variant, "ghost.mp4", "wide.png", null, null)));
                list.Add(new Entry(BuiltInBlocks.VideoKind, variant, "takeover",
                    VideoBlock(variant, "short.mp4", "wide.png", "short.vtt", ShortChapters, display: CardOptions.DisplayTakeover)));
            }
        }

        // A video block over the gallery's clips: a null `poster` / `captions` / `chapters` leaves the field out
        public static BlockInstanceData VideoBlock(string variant, string clip, string poster, string captions, (string Time, string Label)[] chapters,
            string title = VideoTitle, bool captionsOn = false, string display = CardOptions.DisplayInline)
        {
            var block = new BlockInstanceData { key = VideoBlockKey, kind = BuiltInBlocks.VideoKind, variant = variant, display = display };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.VideoClipField, asset = clip });
            if (poster != null) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.VideoPosterField, asset = poster });
            if (captions != null) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.VideoCaptionsField, asset = captions });
            if (title != null) block.fields.Add(Text(BuiltInBlocks.VideoTitleField, title));
            if (captionsOn) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.VideoCaptionsOnField, flag = true });
            if (chapters != null)
            {
                var rows = new BlockFieldValue { key = BuiltInBlocks.VideoChaptersField };
                foreach (var (time, label) in chapters)
                    rows.items.Add(Item(new BlockItemFieldValue { key = BuiltInBlocks.VideoChapterTimeField, value = time },
                        ItemText(BuiltInBlocks.VideoChapterLabelField, label)));
                block.fields.Add(rows);
            }
            return block;
        }
    }
}
