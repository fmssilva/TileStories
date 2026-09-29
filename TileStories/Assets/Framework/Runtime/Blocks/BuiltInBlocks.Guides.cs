namespace TileStories
{
    // The guides family: what the visitor listens to -- an audio guide, in a full player or as a chip pinned under the header.
    // Part of BuiltInBlocks (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and
    // field constants first, then the definitions.
    public static partial class BuiltInBlocks
    {
        public const string AudioGuideKind = "audio_guide";
        public const string AudioGuidePlayer = "player";
        public const string AudioGuideHeroChip = "hero_chip";
        public const string AudioGuideClipField = "clip";
        public const string AudioGuideCaptionsField = "captions";
        public const string AudioGuideTitleField = "title";
        public const string AudioGuideSpeedsField = "speeds";
        public const string AudioGuideCaptionsOnField = "captions_on";

        // Tier 4: one clip the visitor listens to. Both looks drive the card's ONE audio owner, so a player and a chip that name the
        // same clip show one playing state.
        public static readonly BlockKindDefinition AudioGuide = new()
        {
            Key = AudioGuideKind,
            Family = "guides",
            DisplayName = "Audio Guide",
            Help = "A recording the visitor listens to. Player: a play / pause button, a bar to drag to any point, the time, a speed chip and " +
                   "a captions switch with the caption line of the moment. Hero Chip: a small play / pause chip with the progress, pinned " +
                   "under the card's title so it plays from the peek stop without scrolling. Two blocks that use the same clip are the " +
                   "same audio (one shows what the other does). It keeps playing in a small player on the wall after the card closes " +
                   "(Detail Card > Card Container > Keep Audio Playing). Only one audio plays at a time: what starting another one does " +
                   "is set in Card Container > Audio Overlap.",
            Variants = new[] { AudioGuidePlayer, AudioGuideHeroChip },
            DefaultVariant = AudioGuidePlayer,
            DisplayModes = new[] { CardOptions.DisplayInline },
            PinnedTopVariants = new[] { AudioGuideHeroChip },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = AudioGuideClipField, Type = BlockFieldType.Asset, Media = MediaKind.Audio, Label = "Clip", Required = true,
                    Help = "An MP3, WAV or OGG file inside the wall's Media Folder (Detail Card > Card Container). A long recording is best " +
                           "imported as Streaming in Unity so it is not decoded into memory until played.",
                },
                new BlockFieldDefinition
                {
                    Key = AudioGuideCaptionsField, Type = BlockFieldType.Asset, Media = MediaKind.Captions, Label = "Captions",
                    Help = "A WebVTT (.vtt) captions file inside the Media Folder, in the language the clip is spoken in. Empty: no captions " +
                           "switch. The Player shows the caption line of the moment under the bar.",
                },
                new BlockFieldDefinition
                {
                    Key = AudioGuideTitleField, Type = BlockFieldType.LocalizedText, Label = "Title",
                    Help = "The name shown next to the play button and in the small player on the wall. Empty: the point's card title.",
                },
                new BlockFieldDefinition
                {
                    Key = AudioGuideSpeedsField, Type = BlockFieldType.Choice, Label = "Speeds",
                    Options = AudioSpeedRule.Presets, OptionLabels = AudioSpeedRule.PresetLabels,
                    Help = "The speeds the speed chip cycles through. Off: no chip, normal speed only. Empty: Narration. A faster speed also " +
                           "raises the voice (Unity plays speed as pitch).",
                },
                new BlockFieldDefinition
                {
                    Key = AudioGuideCaptionsOnField, Type = BlockFieldType.Toggle, Label = "Captions On By Default",
                    Help = "Show the caption line as soon as the card opens (the visitor can still switch it off). Needs a Captions file.",
                },
            },
        };
    }
}
