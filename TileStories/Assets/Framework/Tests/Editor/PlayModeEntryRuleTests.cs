using System.IO;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // An EditMode fixture that enters Play Mode costs 10-45 s; they live in their own assembly (Tests/Editor/Live,
    // TileStories.Editor.Tests.Live) so a block's EditMode half runs without them through assembly_names, and CORE / FULL run all
    // (40-testing.md 4.2). These keep a new one from slowing every block and a fast one from leaving the block's run.
    public class PlayModeEntryRuleTests
    {
        private const string LiveFolder = "Assets/Framework/Tests/Editor/Live";
        // - split so this file does not match its own pattern
        private static readonly string Entry = "new Enter" + "PlayMode";

        // Every test source under Assets/ that yields Play Mode, outside the Live folder
        [Test]
        public void NoTestOutsideTheLiveAssembly_EntersPlayMode()
        {
            var offenders = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories)
                .Select(f => f.Replace('\\', '/'))
                .Where(f => !f.StartsWith(LiveFolder + "/") && File.ReadAllText(f).Contains(Entry))
                .ToList();
            Assert.IsEmpty(offenders, "a test that enters Play Mode belongs in " + LiveFolder + ":\n" + string.Join("\n", offenders));
        }

        [Test]
        public void EveryFixtureInTheLiveAssembly_EntersPlayMode_AndTheAssemblyIsTheLiveOne()
        {
            var sources = Directory.GetFiles(LiveFolder, "*.cs");
            Assert.IsNotEmpty(sources, "precondition: the Live folder holds the Play Mode fixtures");
            var fast = sources.Where(f => !File.ReadAllText(f).Contains(Entry)).ToList();
            Assert.IsEmpty(fast, "a fixture that never enters Play Mode belongs in the main Editor test assembly (a block's run skips Live):\n"
                + string.Join("\n", fast));
            StringAssert.Contains("\"name\": \"TileStories.Editor.Tests.Live\"", File.ReadAllText(Path.Combine(LiveFolder, "TileStories.Editor.Tests.Live.asmdef")),
                "the Live folder is its own assembly, the name a block's assembly_names leaves out");
        }
    }
}
