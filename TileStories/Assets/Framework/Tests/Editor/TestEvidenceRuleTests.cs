using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // Test renders never land inside Assets/ (_3.1 step 11-fix Part 0): every picture a test saves goes through
    // TileStories.Tests.TestEvidence to TileStories/TestEvidence/<domain>/, which Unity does not import and git ignores. These
    // tests keep a private copy of the old path from coming back and keep the ignore rules in place.
    public class TestEvidenceRuleTests
    {
        [Test]
        public void NoSourceFile_BuildsAPathToAScreenshotsFolder_InsideAssets()
        {
            var offenders = new System.Collections.Generic.List<string>();
            foreach (string file in Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories))
            {
                // - this file names the old path in its own pattern
                if (Path.GetFileName(file) == nameof(TestEvidenceRuleTests) + ".cs") continue;
                string text = File.ReadAllText(file);
                // - Path.Combine(Application.dataPath, "Screenshots") was copied into eight test files before the helper existed
                if (Regex.IsMatch(text, @"dataPath\s*,\s*""Screenshots""") || text.Contains("\"MarkerGalleryScreenshots\""))
                    offenders.Add(file);
            }
            Assert.IsEmpty(offenders, "a test writes its render inside Assets/ (use TestEvidence.PathFor):\n" + string.Join("\n", offenders));
        }

        [Test]
        public void ThereIsNoScreenshotsFolder_InsideAssets()
        {
            Assert.IsFalse(Directory.Exists("Assets/Screenshots"), "Assets/Screenshots must not exist: renders live in TestEvidence/");
            Assert.IsFalse(File.Exists("Assets/Screenshots.meta"), "Assets/Screenshots.meta must not exist");
        }

        [Test]
        public void TheIgnoreFile_KeepsEveryRenderFolderOutOfGit()
        {
            var lines = File.ReadAllLines(".gitignore").Select(l => l.Trim()).ToList();
            CollectionAssert.Contains(lines, "TestEvidence/", "TestEvidence/ is ignored");
            CollectionAssert.Contains(lines, "MarkerGalleryScreenshots/", "the old marker gallery folder stays ignored");
        }

        [Test]
        public void NoUnityTestFrameworkLeftoverScene_SitsInAssets()
        {
            var leftovers = Directory.GetFiles("Assets", "InitTestScene*.unity", SearchOption.TopDirectoryOnly);
            Assert.IsEmpty(leftovers, "aborted test runs leave InitTestScene<guid>.unity files behind: delete them");
        }
    }
}
