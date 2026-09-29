using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // _3.1 step 13: CardMediaLibrary (the ScriptableObject lookup), CardMediaLibraryLookup (wall-over-Framework
    // resolution order) and ResourcesMediaSource's "default:<key>" handling. Pure, no Resources.Load involved --
    // libraries and assets are built in memory and released with Object.DestroyImmediate.
    public class CardMediaLibraryTests
    {
        private static Texture2D _wallImage, _frameworkImage;
        private static CardMediaLibrary _wall, _framework;

        [SetUp]
        public void SetUp()
        {
            _wallImage = new Texture2D(2, 2) { name = "wall_azulejo" };
            _frameworkImage = new Texture2D(2, 2) { name = "framework_azulejo" };
            _wall = ScriptableObject.CreateInstance<CardMediaLibrary>();
            _framework = ScriptableObject.CreateInstance<CardMediaLibrary>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_wallImage);
            Object.DestroyImmediate(_frameworkImage);
            Object.DestroyImmediate(_wall);
            Object.DestroyImmediate(_framework);
        }

        private static void Add(CardMediaLibrary library, string key, MediaKind kind, Object asset, string note = "") =>
            library.Set(key, kind, asset, note);

        [Test]
        public void Get_FindsTheEntryOfTheRightKindUnderThatKey_AndNothingElse()
        {
            Add(_framework, "azulejo_blue", MediaKind.Image, _frameworkImage);
            Assert.AreEqual(_frameworkImage, _framework.Get("azulejo_blue", MediaKind.Image));
            Assert.IsNull(_framework.Get("azulejo_blue", MediaKind.Audio), "same key, wrong kind: no entry");
            Assert.IsNull(_framework.Get("unknown_key", MediaKind.Image));
            Assert.IsNull(_framework.Get("", MediaKind.Image));
            Assert.IsNull(_framework.Get(null, MediaKind.Image));
        }

        [Test]
        public void AudioAndCaptions_ShareAKey_AsTwoSeparateEntries()
        {
            var clip = AudioClip.Create("chime", 1, 1, 8000, false);
            var vtt = new TextAsset("WEBVTT");
            Add(_framework, "chime", MediaKind.Audio, clip);
            Add(_framework, "chime", MediaKind.Captions, vtt);
            Assert.AreEqual(clip, _framework.Get("chime", MediaKind.Audio));
            Assert.AreEqual(vtt, _framework.Get("chime", MediaKind.Captions));
            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(vtt);
        }

        [Test]
        public void Keys_ListsOnlyTheEntriesOfThatKind()
        {
            Add(_framework, "one", MediaKind.Image, _frameworkImage);
            Add(_framework, "two", MediaKind.Image, _wallImage);
            Add(_framework, "clip", MediaKind.Audio, _frameworkImage);
            CollectionAssert.AreEquivalent(new[] { "one", "two" }, _framework.Keys(MediaKind.Image));
            CollectionAssert.AreEquivalent(new[] { "clip" }, _framework.Keys(MediaKind.Audio));
        }

        [Test]
        public void Set_RefusesABlankKeyOrANullAsset_SoAnEntryIsNeverInAnUnusableState()
        {
            _framework.Set("", MediaKind.Image, _frameworkImage);
            _framework.Set("blank", MediaKind.Image, null);
            Assert.IsEmpty(_framework.Entries);
        }

        [Test]
        public void Set_OnTheSameKeyAndKind_ReplacesTheEntry_RatherThanDuplicatingIt()
        {
            _framework.Set("azulejo_blue", MediaKind.Image, _frameworkImage, "first");
            _framework.Set("azulejo_blue", MediaKind.Image, _wallImage, "second");
            Assert.AreEqual(1, _framework.Entries.Count);
            Assert.AreEqual(_wallImage, _framework.Get("azulejo_blue", MediaKind.Image));
            Assert.AreEqual("second", _framework.Entries[0].note);
        }

        [Test]
        public void CopyFrom_ReplacesEveryEntry_LikeTheIconAndFontLibraries()
        {
            Add(_wall, "old", MediaKind.Image, _wallImage);
            Add(_framework, "azulejo_blue", MediaKind.Image, _frameworkImage);
            _wall.CopyFrom(_framework);
            Assert.IsNull(_wall.Get("old", MediaKind.Image), "the wall's own prior entries are gone");
            Assert.AreEqual(_frameworkImage, _wall.Get("azulejo_blue", MediaKind.Image));
        }

        [Test]
        public void Lookup_Resolve_TheWallsLibraryWinsOverTheFrameworks_AndEitherAloneStillResolves()
        {
            Add(_wall, "azulejo_blue", MediaKind.Image, _wallImage);
            Add(_framework, "azulejo_blue", MediaKind.Image, _frameworkImage);
            Add(_framework, "framework_only", MediaKind.Image, _frameworkImage);

            Assert.AreEqual(_wallImage, CardMediaLibraryLookup.Resolve("azulejo_blue", MediaKind.Image, _wall, _framework), "wall over framework");
            Assert.AreEqual(_frameworkImage, CardMediaLibraryLookup.Resolve("framework_only", MediaKind.Image, _wall, _framework), "not in the wall's: falls back");
            Assert.IsNull(CardMediaLibraryLookup.Resolve("azulejo_blue", MediaKind.Image, null, null), "neither library: unresolved");
            Assert.AreEqual(_frameworkImage, CardMediaLibraryLookup.Resolve("azulejo_blue", MediaKind.Image, null, _framework), "no wall library at all");
        }

        [Test]
        public void ResourcesMediaSource_LoadsADefaultKey_ThroughTheTwoLibraries_NeverThroughResourcesLoad()
        {
            Add(_wall, "azulejo_blue", MediaKind.Image, _wallImage);
            Add(_framework, "azulejo_blue", MediaKind.Image, _frameworkImage);
            Add(_framework, "framework_only", MediaKind.Image, _frameworkImage);
            var media = new ResourcesMediaSource("LivingRoom/CardMedia") { WallDefaults = _wall, FrameworkDefaults = _framework };

            Assert.AreEqual(_wallImage, media.Load<Texture2D>("default:azulejo_blue"), "the wall's own override wins");
            Assert.AreEqual(_frameworkImage, media.Load<Texture2D>("default:framework_only"));
            Assert.IsNull(media.Load<Texture2D>("default:nope"), "an unknown key resolves to nothing, never an exception");
        }

        [Test]
        public void ResourcesMediaSource_NeverReferenceCountsADefaultKey_ReleaseIsANoOp()
        {
            Add(_framework, "azulejo_blue", MediaKind.Image, _frameworkImage);
            var media = new ResourcesMediaSource("LivingRoom/CardMedia") { FrameworkDefaults = _framework };
            media.Load<Texture2D>("default:azulejo_blue");
            media.Load<Texture2D>("default:azulejo_blue");
            Assert.AreEqual(0, media.LoadedCount, "a library asset is a static reference, never counted as 'held'");
            media.Release("default:azulejo_blue");
            Assert.AreEqual(0, media.LoadedCount);
        }
    }
}
