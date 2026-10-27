using System.Linq;
using NUnit.Framework;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // FixtureTimes on a result tree shaped like this project's runs: the assembly, its no-namespace SetUpFixture (a class whose
    // children are namespaces), namespaces, fixtures holding plain cases and a parameterized method's suite, a fixture that ran
    // nothing; and the file's place next to the recorder's
    public class FixtureTimesTests
    {
        private static FixtureTimes.Node Case(string name, int passed, int failed = 0, int skipped = 0) =>
            new FixtureTimes.Node { FullName = name, HasType = true, HasMethod = true, Passed = passed, Failed = failed, Skipped = skipped };

        private static FixtureTimes.Node Suite(string name, bool hasType, bool hasMethod, double seconds, params FixtureTimes.Node[] children)
        {
            var node = new FixtureTimes.Node { FullName = name, IsSuite = true, HasType = hasType, HasMethod = hasMethod, Seconds = seconds };
            node.Children.AddRange(children);
            node.Passed = children.Sum(c => c.Passed);
            node.Failed = children.Sum(c => c.Failed);
            node.Skipped = children.Sum(c => c.Skipped);
            return node;
        }

        private static FixtureTimes.Node Run() =>
            Suite("TileStories.Tests.Runtime.dll", false, false, 300,
                Suite("TestDialogGuard", true, false, 299,                       // the SetUpFixture: a class, but no methods under it
                    Suite("TileStories", false, false, 298,
                        Suite("Tests", false, false, 297,
                            Suite("TileStories.Tests.Slow", true, false, 200.04,
                                Case("TileStories.Tests.Slow.A", 1), Case("TileStories.Tests.Slow.B", 0, 1),
                                Suite("TileStories.Tests.Slow.Each", false, true, 150,   // a [TestCase] method's suite
                                    Case("TileStories.Tests.Slow.Each(1)", 1), Case("TileStories.Tests.Slow.Each(2)", 0, 0, 1))),
                            Suite("TileStories.Tests.Fast", true, false, 2.26, Case("TileStories.Tests.Fast.A", 1)),
                            Suite("TileStories.Tests.NotRun", true, false, 0, Case("TileStories.Tests.NotRun.A", 0))))));

        [Test]
        public void EveryFixtureThatRan_IsOneRow_SlowestFirst_WithItsOwnCounts()
        {
            var rows = FixtureTimes.Rows(Run());
            CollectionAssert.AreEqual(new[] { "TileStories.Tests.Slow", "TileStories.Tests.Fast" }, rows.Select(r => r.Fixture).ToArray(),
                "the fixtures, slowest first -- never the SetUpFixture, a namespace, a parameterized suite or a fixture that ran nothing");
            var slow = rows[0];
            Assert.AreEqual((4, 2, 1, 1), (slow.Tests, slow.Passed, slow.Failed, slow.Skipped), "the parameterized cases count inside their fixture");
            Assert.AreEqual(200.04, slow.Seconds, 1e-9, "the fixture's own duration (its set-up included), not the sum of its cases");
        }

        [Test]
        public void TheCsv_IsTheHeaderAndALinePerRow_InTheInvariantCulture_AndQuotesACommaName()
        {
            var rows = FixtureTimes.Rows(Run()).ToList();
            rows.Add(new FixtureTimes.Row { Fixture = "Gallery(\"a\",b)", Tests = 1, Passed = 1, Seconds = 0.04 });
            string[] lines = FixtureTimes.ToCsv(rows).TrimEnd('\n').Split('\n');
            CollectionAssert.AreEqual(new[]
            {
                "fixture,tests,passed,failed,skipped,seconds",
                "TileStories.Tests.Slow,4,2,1,1,200.0",
                "TileStories.Tests.Fast,1,1,0,0,2.3",
                "\"Gallery(\"\"a\"\",b)\",1,1,0,0,0.0",
            }, lines);
        }

        [Test]
        public void TheFile_SitsBesideTheRecordersOwn_InTestEvidenceHarness()
        {
            StringAssert.EndsWith("Harness/fixture_times.csv", TestRunRecorder.FixtureTimesPath.Replace('\\', '/'));
            Assert.AreEqual(System.IO.Path.GetDirectoryName(TestRunRecorder.LogPath), System.IO.Path.GetDirectoryName(TestRunRecorder.FixtureTimesPath));
        }
    }
}
