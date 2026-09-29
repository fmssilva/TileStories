using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // _3.1 step 13: the Framework's SHIPPED default media library (CardMediaLibraryGenerator's real output, not a
    // fabricated instance) -- every promised key resolves, the whole generated set stays inside its size budget,
    // and it is reachable the same way a wall's own default library would be, through Resources.Load.
    public class CardMediaLibraryGeneratorTests
    {
        [Test]
        public void FrameworkDefault_ResolvesFromResources_LikeAWallsOwnLibraryWould()
        {
            var library = CardMediaLibraryLookup.Framework;
            Assert.IsNotNull(library, "TileStories/CardMediaLibrary must exist under a Resources folder (CardMediaLibraryGenerator.LibraryPath)");
        }

        [TestCase("azulejo_blue", MediaKind.Image)]
        [TestCase("azulejo_ochre", MediaKind.Image)]
        [TestCase("azulejo_detail", MediaKind.Image)]
        [TestCase("panel_plain", MediaKind.Image)]
        [TestCase("poster_plain", MediaKind.Image)]
        [TestCase("map_plan", MediaKind.Image)]
        [TestCase("chime", MediaKind.Audio)]
        [TestCase("ambient", MediaKind.Audio)]
        [TestCase("ambient", MediaKind.Captions)]
        [TestCase("tile_pattern", MediaKind.Video)]
        public void EveryPromisedKey_ResolvesToARealAsset(string key, MediaKind kind)
        {
            var asset = CardMediaLibraryLookup.Framework.Get(key, kind);
            Assert.IsNotNull(asset, key + " (" + kind + ") is missing from the shipped default library");
        }

        [Test]
        public void ChimeHasNoCaptions_TooShortToCaption()
        {
            Assert.IsNull(CardMediaLibraryLookup.Framework.Get("chime", MediaKind.Captions));
        }

        [Test]
        public void TheWholeGeneratedSet_StaysUnderTheThreeMegabyteBudget()
        {
            string folder = Path.Combine(TestRoot(), "CardMedia");
            Assert.IsTrue(Directory.Exists(folder), folder);
            long total = 0;
            foreach (var file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta")) continue;
                total += new FileInfo(file).Length;
            }
            Assert.Greater(total, 0, "the sum found nothing: the generator has not run, or the path drifted");
            Assert.LessOrEqual(total, 3L * 1024 * 1024, "generated media " + total + " bytes -- over the 3 MB budget (_3.1 step 13)");
        }

        private static string TestRoot() => Path.Combine(Application.dataPath, CardMediaLibraryGenerator.RootFolder.Substring("Assets/".Length));
    }
}
