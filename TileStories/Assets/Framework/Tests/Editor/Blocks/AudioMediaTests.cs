using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The audio side of the media seam (_3.1 step 9A) on the REAL LivingRoom fixture files: the media rule for audio and captions, the
    // clips and caption files load through the real ResourcesMediaSource (the .vtt importer really makes a TextAsset), a clip and its
    // captions with one base name are two assets, and every audio block the shipped wall authors names files that exist.
    public class AudioMediaTests
    {
        private const string Root = "LivingRoom/CardMedia";
        private const string ConfigPath = "Assets/Apps/LivingRoom/config.json";

        [Test]
        public void MediaPathRule_AudioTakesMp3WavOgg_CaptionsTakeVtt_EachRefusesTheOthers()
        {
            foreach (string ok in new[] { "audio/guide.mp3", "a/b/Guide.WAV", " guide.ogg ", "audio\\guide.mp3" })
                Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check(ok, MediaKind.Audio), ok);
            foreach (string wrong in new[] { "guide.png", "guide.vtt", "guide.mp4", "guide" })
                Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check(wrong, MediaKind.Audio), wrong);
            Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check("audio/guide.vtt", MediaKind.Captions));
            Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check("GUIDE.VTT", MediaKind.Captions));
            foreach (string wrong in new[] { "guide.srt", "guide.mp3", "guide.txt" })
                Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check(wrong, MediaKind.Captions), wrong);
            Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check("guide.mp3", MediaKind.Image), "an mp3 in a picture field");
            Assert.AreEqual(MediaPathProblem.OutsideFolder, MediaPathRule.Check("Assets/Sounds/guide.mp3", MediaKind.Audio), "a project path is outside the media folder");
            Assert.AreEqual(MediaPathProblem.OutsideFolder, MediaPathRule.Check("../guide.mp3", MediaKind.Audio));
            Assert.AreEqual(MediaPathProblem.Empty, MediaPathRule.Check("  ", MediaKind.Audio));
            Assert.AreEqual("audio/guide.mp3", MediaPathRule.StoredPathFor("Assets/Apps/W/Resources/W/CardMedia/audio/guide.mp3", "W/CardMedia"), "the stored path is relative to the folder");
        }

        [Test]
        public void MediaPathRule_KindOfExtension_AndAssetKinds_TellWhatAPathAndALoadedAssetAre()
        {
            Assert.AreEqual(MediaKind.Audio, MediaPathRule.KindOfExtension("a/guide.MP3"));
            Assert.AreEqual(MediaKind.Captions, MediaPathRule.KindOfExtension("guide.vtt"));
            Assert.AreEqual(MediaKind.Image, MediaPathRule.KindOfExtension("guide.jpeg"));
            Assert.AreEqual(MediaKind.None, MediaPathRule.KindOfExtension("guide"));
            Assert.AreEqual(MediaKind.None, MediaPathRule.KindOfExtension("guide.xyz"));
            Assert.AreEqual(MediaKind.None, MediaPathRule.KindOfExtension(""));
            var clip = AudioClip.Create("probe", 100, 1, 8000, false);
            var text = new TextAsset("WEBVTT");
            var texture = new Texture2D(2, 2);
            try
            {
                Assert.IsTrue(MediaPathRule.IsAssetOfKind(clip, MediaKind.Audio));
                Assert.IsFalse(MediaPathRule.IsAssetOfKind(clip, MediaKind.Captions));
                Assert.IsTrue(MediaPathRule.IsAssetOfKind(text, MediaKind.Captions));
                Assert.IsFalse(MediaPathRule.IsAssetOfKind(text, MediaKind.Image));
                Assert.IsTrue(MediaPathRule.IsAssetOfKind(texture, MediaKind.Image));
                Assert.IsTrue(MediaPathRule.IsAssetOfKind(texture, MediaKind.None), "no kind named: any asset fits");
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(text);
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void EveryFixtureClip_LoadsAsARealClip_AndEveryCaptionsFile_AsATextAssetThatParsesInsideItsClip()
        {
            var source = new ResourcesMediaSource(Root);
            var expected = new[]
            {
                ("audio/lamp_tone.wav", "audio/lamp_tone.vtt", 20f, 4),
                ("audio/castelo_s_jorge_guide_pt.mp3", "audio/castelo_s_jorge_guide_pt.vtt", 200.39f, 20),
            };
            foreach (var (clipPath, captionsPath, length, cueCount) in expected)
            {
                var clip = source.Load<AudioClip>(clipPath);
                Assert.IsNotNull(clip, clipPath + " loads through Resources");
                Assert.AreEqual(length, clip.length, 0.1f, clipPath + ": its real length");
                var captions = source.Load<TextAsset>(captionsPath);
                Assert.IsNotNull(captions, captionsPath + " loads as a TextAsset (the .vtt importer)");
                var cues = VttRule.Parse(captions.text);
                Assert.AreEqual(cueCount, cues.Count, captionsPath);
                Assert.LessOrEqual(cues[cues.Count - 1].End, clip.length + 0.01f, captionsPath + ": the last caption ends inside the clip");
                for (int i = 1; i < cues.Count; i++) Assert.GreaterOrEqual(cues[i].Start, cues[i - 1].End - 0.001f, captionsPath + ": cues in order, none overlapping");
                foreach (var cue in cues) VisitorTextChecks.AssertValid(cue.Text, captionsPath + " caption");
            }
            Assert.IsNotNull(source.Load<AudioClip>("audio/portugal_tourism_guide_en.mp3"), "the long English clip loads too");
            Assert.AreEqual(232.91f, source.Load<AudioClip>("audio/portugal_tourism_guide_en.mp3").length, 0.1f);
            Assert.IsTrue(VttRule.Parse(source.Load<TextAsset>("audio/castelo_s_jorge_guide_pt.vtt").text).Any(c => c.Text.Contains("provisório")),
                "the Portuguese captions keep their letters");
        }

        [Test]
        public void AClipAndItsCaptions_WithOneBaseName_AreTwoAssets_CountedAndReleasedApart()
        {
            var source = new ResourcesMediaSource(Root);
            var clip = source.Load<AudioClip>("audio/lamp_tone.wav");
            var captions = source.Load<TextAsset>("audio/lamp_tone.vtt");
            Assert.IsNotNull(clip, "the sound");
            Assert.IsNotNull(captions, "and its captions, though the Resources path is the same without the extension");
            Assert.AreEqual(2, source.LoadedCount);
            Assert.AreEqual(1, source.RefCount("audio/lamp_tone.wav"));
            Assert.AreEqual(1, source.RefCount("audio/lamp_tone.vtt"));
            source.Release("audio/lamp_tone.wav");
            Assert.AreEqual(0, source.RefCount("audio/lamp_tone.wav"), "the sound was given back...");
            Assert.AreEqual(1, source.RefCount("audio/lamp_tone.vtt"), "...and the captions are still held");
            Assert.AreEqual(1, source.LoadedCount);
            Assert.AreEqual("WEBVTT", captions.text.Substring(0, 6), "still readable");
            source.Release("audio/lamp_tone.vtt");
            Assert.AreEqual(0, source.LoadedCount);
        }

        [Test]
        public void TheTwoLongNarrations_StreamFromDisk_SoNothingIsDecodedUntilPlayed()
        {
            foreach (string name in new[] { "castelo_s_jorge_guide_pt.mp3", "portugal_tourism_guide_en.mp3" })
            {
                var importer = (AudioImporter)AssetImporter.GetAtPath("Assets/Apps/LivingRoom/Resources/LivingRoom/CardMedia/audio/" + name);
                Assert.IsNotNull(importer, name);
                Assert.AreEqual(AudioClipLoadType.Streaming, importer.defaultSampleSettings.loadType, name + ": a minutes-long clip streams");
            }
        }

        [Test]
        public void TheShippedAudioBlocks_NameFilesThatExist_AndNoneIsSkipped_TheTwoLampChipsAndPlayerAreOneAudio()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(ConfigPath));
            var settings = config.card_settings;
            var source = new ResourcesMediaSource(settings.media_resources_path);
            int total = 0;
            foreach (string id in new[] { "lamp", "lamp_military", "lamp_economic" })
            {
                var poi = config.pois.Single(p => p.id == id);
                var stack = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared, config.pois);
                var audio = stack.Entries.Where(e => e.Definition.Key == BuiltInBlocks.AudioGuideKind).ToList();
                Assert.IsNotEmpty(audio, id + " has an audio guide");
                CollectionAssert.IsEmpty(stack.Skipped, id + ": every block of the card shows");
                foreach (var entry in audio)
                {
                    total++;
                    var read = new BlockFieldReader(entry.Instance, "en", "en");
                    string clipPath = read.ValidAsset(BuiltInBlocks.AudioGuideClipField, MediaKind.Audio);
                    Assert.IsNotEmpty(clipPath, id + ": the clip path passes the media rule");
                    Assert.IsNotNull(source.Load<AudioClip>(clipPath), id + ": " + clipPath + " exists");
                    string captionsPath = read.ValidAsset(BuiltInBlocks.AudioGuideCaptionsField, MediaKind.Captions);
                    if (captionsPath.Length > 0) Assert.IsNotNull(source.Load<TextAsset>(captionsPath), id + ": " + captionsPath + " exists");
                }
            }
            Assert.AreEqual(4, total, "lamp: chip + player, military: chip, economic: player");
            var lampAudio = config.pois.Single(p => p.id == "lamp").card.blocks.Where(b => b.kind == BuiltInBlocks.AudioGuideKind).ToList();
            CollectionAssert.AreEquivalent(new[] { BuiltInBlocks.AudioGuideHeroChip, BuiltInBlocks.AudioGuidePlayer }, lampAudio.Select(b => b.variant).ToArray(), "both looks on the Lamp");
            Assert.AreEqual(1, lampAudio.Select(b => new BlockFieldReader(b, "en", "en").Asset(BuiltInBlocks.AudioGuideClipField)).Distinct().Count(), "the same clip: one audio");
        }
    }
}
