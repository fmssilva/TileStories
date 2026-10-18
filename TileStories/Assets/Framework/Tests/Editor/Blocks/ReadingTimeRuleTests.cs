using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // Reading Time (_3.2 step 11b): the common `minutes` field, which kinds carry it and which read a clip's length instead, the one rule
    // that turns either into the minutes a contents list shows, and what BlockRegistry.Validate refuses about it.
    public class ReadingTimeRuleTests
    {
        private static BlockInstanceData WithMinutes(float? minutes)
        {
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.RichTextKind, fields = new List<BlockFieldValue>() };
            if (minutes.HasValue) block.fields.Add(new BlockFieldValue { key = BlockKindDefinition.MinutesField, number = minutes.Value });
            return block;
        }

        // ---------------- the three sources: field, clip, neither ----------------

        [Test]
        public void AKindWithTheField_ShowsWhatWasTyped_InWholeMinutes_AndNothingWhileUnsetOrZero()
        {
            var kind = BuiltInBlocks.RichText;
            Assert.AreEqual(4, ReadingTimeRule.Minutes(kind, WithMinutes(4f), 0f));
            Assert.AreEqual(3, ReadingTimeRule.Minutes(kind, WithMinutes(2.6f), 0f), "a dragged slider's 2.6 reads as 3 whole minutes");
            Assert.AreEqual(0, ReadingTimeRule.Minutes(kind, WithMinutes(0f), 0f), "0 = not given");
            Assert.AreEqual(0, ReadingTimeRule.Minutes(kind, WithMinutes(null), 0f), "never set: the field's default, 0 = no time");
            Assert.AreEqual(BlockKindDefinition.MinutesDefinition.NumberMax, ReadingTimeRule.Minutes(kind, WithMinutes(500f), 0f),
                "a hand-edited 500 is kept inside the field's range");
            Assert.AreEqual(4, ReadingTimeRule.Minutes(kind, WithMinutes(4f), 95f), "a kind without a clip ignores any clip length: its typed 4");
            Assert.AreEqual(0, ReadingTimeRule.Minutes(kind, WithMinutes(null), 95f), "...and shows no time from a clip it does not read");
        }

        [Test]
        public void AKindThatReadsAClip_ShowsTheClipsLength_RoundedUp_NeverATypedNumber()
        {
            var audio = BuiltInBlocks.AudioGuide;
            var block = new BlockInstanceData { key = "block_55", kind = audio.Key, fields = new List<BlockFieldValue>
            {
                // - a stray minutes value in a hand-edited file is not this kind's field: the clip wins
                new() { key = BlockKindDefinition.MinutesField, number = 9f },
            } };
            Assert.AreEqual(2, ReadingTimeRule.Minutes(audio, block, 95f), "95 s -> 2 min (rounded up)");
            Assert.AreEqual(1, ReadingTimeRule.Minutes(audio, block, 40f), "a 40 s clip is 1 min, never 0");
            Assert.AreEqual(1, ReadingTimeRule.Minutes(audio, block, 60f), "exactly one minute");
            Assert.AreEqual(0, ReadingTimeRule.Minutes(audio, block, 0f), "a missing clip: no time, and not the stray 9");
            Assert.AreEqual(0, ReadingTimeRule.FromClip(float.NaN));
            Assert.AreEqual(12, ReadingTimeRule.Minutes(BuiltInBlocks.Video, block, 12f * 60f - 0.5f), "a video reads its clip the same way");
            Assert.AreSame(audio.Field(BuiltInBlocks.AudioGuideClipField), ReadingTimeRule.ClipField(audio), "what the view loads");
            Assert.IsNull(ReadingTimeRule.ClipField(BuiltInBlocks.RichText));
        }

        [Test]
        public void AKindWithNeither_ShowsNoTime()
        {
            Assert.AreEqual(0, ReadingTimeRule.Minutes(BuiltInBlocks.Header, WithMinutes(5f), 0f), "the title block carries no Reading Time");
            Assert.AreEqual(0, ReadingTimeRule.Minutes(BuiltInBlocks.NavIconGrid, WithMinutes(5f), 0f), "a nav block neither");
            Assert.AreEqual(0, ReadingTimeRule.Minutes(null, WithMinutes(5f), 0f));
            Assert.AreEqual(0, ReadingTimeRule.Minutes(BuiltInBlocks.RichText, null, 0f));
        }

        // ---------------- which kinds carry the field ----------------

        [Test]
        public void EveryRegisteredKind_CarriesTheHeading_AndReadingTimeUnlessItReadsAClipOrIsNeverAContentsRow()
        {
            var clips = new List<string>();
            var none = new List<string>();
            foreach (var kind in BlockRegistry.Shared.All)
            {
                Assert.AreEqual(BlockKindDefinition.HeadingField, kind.Fields[0].Key, kind.Key + ": the heading stays the first field");
                bool carries = kind.Field(BlockKindDefinition.MinutesField) != null;
                if (kind.ClipLengthField != null) clips.Add(kind.Key);
                else if (!carries) none.Add(kind.Key);
                Assert.AreEqual(kind.OffersReadingTime && kind.ClipLengthField == null, carries, kind.Key);
                if (carries) Assert.AreSame(BlockKindDefinition.MinutesDefinition, kind.Fields[1], kind.Key + ": Reading Time right after the heading");
            }
            CollectionAssert.AreEquivalent(new[] { BuiltInBlocks.AudioGuideKind, BuiltInBlocks.VideoKind }, clips, "the kinds that read a clip");
            CollectionAssert.AreEquivalent(new[] { BuiltInBlocks.HeaderKind, BuiltInBlocks.NavIconGridKind, BuiltInBlocks.NavContentsKind }, none,
                "the kinds that are never a row of a contents list");
        }

        [Test]
        public void ReadingTime_IsOptional_AWholeNumberSliderFromZero_NeverRequired()
        {
            var field = BlockKindDefinition.MinutesDefinition;
            Assert.AreEqual(BlockFieldType.Number, field.Type);
            Assert.IsFalse(field.Required, "an empty Reading Time never hides a block");
            Assert.AreEqual(0f, field.NumberMin);
            Assert.AreEqual(0f, field.NumberDefault, "unset = 0 = no time shown");
            Assert.AreEqual("Reading Time", field.Label);
        }

        [Test]
        public void TheCommonList_DoesNotDependOnTheInitializersOrder()
        {
            // - ClipLengthField set AFTER Fields in the initializer: the merge happens on the first read, so it still has no Reading Time
            var kind = new BlockKindDefinition
            {
                Key = "app_clip", Family = BuiltInFamilies.Media, Format = CardOptions.FormatListen, Variants = new[] { "a" }, DefaultVariant = "a",
                DisplayModes = new[] { CardOptions.DisplayInline },
                Fields = new[] { new BlockFieldDefinition { Key = "clip", Type = BlockFieldType.Asset, Media = MediaKind.Audio, Label = "Clip" } },
                ClipLengthField = "clip",
            };
            Assert.IsNull(kind.Field(BlockKindDefinition.MinutesField), "the clip kind carries no Reading Time, whatever the order");
            Assert.AreEqual(new[] { BlockKindDefinition.HeadingField, "clip" }, kind.Fields.Select(f => f.Key).ToArray());
            Assert.IsNull(BlockRegistry.Validate(kind), "an app kind that reads a clip registers");
        }

        // ---------------- what the registry refuses ----------------

        private static BlockKindDefinition AppKind(string clipField, params BlockFieldDefinition[] own) => new()
        {
            Key = "app_kind", Family = BuiltInFamilies.About, Format = CardOptions.FormatRead, Variants = new[] { "a" }, DefaultVariant = "a",
            DisplayModes = new[] { CardOptions.DisplayInline }, Fields = own, ClipLengthField = clipField,
        };

        [Test]
        public void Validate_RefusesAClipLengthFieldThatIsNoAudioOrVideoAssetField()
        {
            var picture = new BlockFieldDefinition { Key = "picture", Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Picture" };
            var text = new BlockFieldDefinition { Key = "words", Type = BlockFieldType.LocalizedText, Label = "Words" };
            var video = new BlockFieldDefinition { Key = "film", Type = BlockFieldType.Asset, Media = MediaKind.Video, Label = "Film" };
            StringAssert.Contains("'picture'", BlockRegistry.Validate(AppKind("picture", picture)), "a picture has no length");
            StringAssert.Contains("'words'", BlockRegistry.Validate(AppKind("words", text)), "a text field has no length");
            StringAssert.Contains("'nothing'", BlockRegistry.Validate(AppKind("nothing", text)), "a field the kind does not have");
            Assert.IsNull(BlockRegistry.Validate(AppKind("film", video)), "a video Asset field is a clip");
        }

        [Test]
        public void Validate_RefusesAKindsOwnFieldUnderACommonKey_EvenOnAKindThatCarriesNoReadingTime()
        {
            var ownMinutes = new BlockFieldDefinition { Key = BlockKindDefinition.MinutesField, Type = BlockFieldType.Number, Label = "Minutes" };
            var clip = new BlockFieldDefinition { Key = "clip", Type = BlockFieldType.Asset, Media = MediaKind.Audio, Label = "Clip" };
            Assert.IsNotNull(BlockRegistry.Validate(AppKind(null, ownMinutes)), "two fields keyed 'minutes' on a kind that carries it");
            StringAssert.Contains("'minutes'", BlockRegistry.Validate(AppKind("clip", clip, ownMinutes)),
                "a clip kind has no common Reading Time, but its own 'minutes' would still read as one to every reader");
            Assert.IsNull(BlockRegistry.Validate(AppKind("clip", clip)));
        }
    }
}
