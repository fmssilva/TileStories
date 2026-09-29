using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace TileStories.Editor.Tests
{
    // The video side of the media seam (_3.1 step 9B) on REAL files: the media rule for video, the LivingRoom fixture clip through the real
    // ResourcesMediaSource (loaded, counted, given back), the import settings that keep it small in a build, and the Phase A gallery's
    // generated clips matching the lengths CardGalleryDefinitions promises.
    public class VideoMediaTests
    {
        private const string Root = "LivingRoom/CardMedia";
        private const string FixtureClip = "video/castelo_s_jorge_video.mp4";
        private const string FixtureAssetPath = "Assets/Apps/LivingRoom/Resources/LivingRoom/CardMedia/" + FixtureClip;

        [Test]
        public void MediaPathRule_VideoTakesMp4AndWebm_AndRefusesEveryOtherKind_TheOtherKindsRefuseIt()
        {
            foreach (string ok in new[] { "video/castle.mp4", "a/b/Castle.MP4", " clip.webm ", "video\\castle.mp4" })
                Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check(ok, MediaKind.Video), ok);
            foreach (string wrong in new[] { "castle.mov", "castle.png", "castle.mp3", "castle.vtt", "castle" })
                Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check(wrong, MediaKind.Video), wrong);
            foreach (var other in new[] { MediaKind.Image, MediaKind.Audio, MediaKind.Captions })
                Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check("castle.mp4", other), "an mp4 in a " + other + " field");
            Assert.AreEqual(MediaPathProblem.OutsideFolder, MediaPathRule.Check("Assets/Videos/castle.mp4", MediaKind.Video));
            Assert.AreEqual(MediaPathProblem.OutsideFolder, MediaPathRule.Check("../castle.mp4", MediaKind.Video));
            Assert.AreEqual(MediaPathProblem.Empty, MediaPathRule.Check("", MediaKind.Video));
            Assert.AreEqual(MediaKind.Video, MediaPathRule.KindOfExtension("v/castle.MP4"));
            Assert.AreEqual(MediaKind.Video, MediaPathRule.KindOfExtension("clip.webm"));
            Assert.AreEqual(MediaKind.None, MediaPathRule.KindOfExtension("clip.mov"));
        }

        [Test]
        public void TheFixtureClip_LoadsAsARealVideoClip_ThroughTheWallsSource_IsItsOwnKind_AndIsGivenBack()
        {
            var source = new ResourcesMediaSource(Root);
            var clip = source.Load<VideoClip>(FixtureClip);
            Assert.IsNotNull(clip, "the castle video loads through Resources");
            Assert.AreEqual(200.3, clip.length, 0.2, "its real length");
            Assert.AreEqual(640u, clip.width);
            Assert.AreEqual(360u, clip.height);
            Assert.AreEqual(1, clip.audioTrackCount, "it has sound: starting it must pause the card's audio");
            Assert.IsTrue(MediaPathRule.IsAssetOfKind(clip, MediaKind.Video));
            Assert.IsFalse(MediaPathRule.IsAssetOfKind(clip, MediaKind.Image));
            Assert.IsFalse(MediaPathRule.IsAssetOfKind(clip, MediaKind.Audio));
            Assert.AreSame(clip, source.Load<VideoClip>(FixtureClip), "a second load is the same asset");
            Assert.AreEqual(2, source.RefCount(FixtureClip));
            source.Release(FixtureClip);
            source.Release(FixtureClip);
            Assert.AreEqual(0, source.RefCount(FixtureClip));
            Assert.AreEqual(0, source.LoadedCount, "the last release unloads it");
        }

        [Test]
        public void TheFixtureClip_IsTranscodedAtALowBitrate_SoABuildCarriesAboutHalfOfIt()
        {
            var importer = (VideoClipImporter)AssetImporter.GetAtPath(FixtureAssetPath);
            Assert.IsNotNull(importer);
            var settings = importer.defaultTargetSettings;
            Assert.IsTrue(settings.enableTranscoding, "transcoded on import");
            Assert.AreEqual(VideoCodec.H264, settings.codec, "H.264 plays on every target");
            Assert.AreEqual(VideoBitrateMode.Low, settings.bitrateMode);
            Assert.Less(importer.outputFileSize, importer.sourceFileSize * 0.75, "the imported clip is well under the 16 MB source");
        }

        [Test]
        public void TheGallerysGeneratedClips_ExistAndLastWhatTheGalleryListSays()
        {
            foreach (var pair in CardGalleryDefinitions.Videos)
            {
                var clip = AssetDatabase.LoadAssetAtPath<VideoClip>(CardGalleryMedia.VideoFolder + "/" + pair.Value.File);
                Assert.IsNotNull(clip, pair.Value.File + " (CardGalleryVideoGenerator.GenerateAll writes it)");
                Assert.AreEqual(pair.Value.Seconds, clip.length, 0.15, pair.Key);
                var media = new CardGalleryMedia();
                Assert.AreSame(clip, media.Load<VideoClip>(pair.Key), "the gallery's source hands the same file for the name a block stores");
                media.Release(pair.Key);
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<VideoClip>(CardGalleryMedia.VideoFolder + "/" + pair.Value.File),
                    "releasing never destroys the project file");
                Assert.AreEqual(pair.Value.File.Replace(".mp4", ""), clip.name, "and never renames it");
            }
            Assert.IsNull(new CardGalleryMedia().Load<VideoClip>("ghost.mp4"), "any other name is a missing file");
        }
    }
}
