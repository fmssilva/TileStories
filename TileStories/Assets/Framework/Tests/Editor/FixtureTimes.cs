using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TileStories.Editor.Tests
{
    // Pure: one test run's result tree -> one row per FIXTURE (its tests, passed / failed / skipped, seconds), slowest first, and
    // the CSV TestRunRecorderHooks writes after every run (TestEvidence/Harness/fixture_times.csv). Where the time goes is what
    // the three test tiers are planned on (40-testing 4.2).
    public static class FixtureTimes
    {
        public const string Header = "fixture,tests,passed,failed,skipped,seconds";

        // One node of a result tree, as the Test Framework's ITestResultAdaptor describes it
        public sealed class Node
        {
            public string FullName;
            public bool IsSuite, HasType, HasMethod;
            public double Seconds;
            public int Passed, Failed, Skipped;
            public List<Node> Children = new List<Node>();
        }

        public struct Row
        {
            public string Fixture;
            public int Tests, Passed, Failed, Skipped;
            public double Seconds;
        }

        // A fixture is a suite with a class whose children are its test methods (a test case, or a parameterized method's
        // suite) -- a SetUpFixture also has a class, but its children are namespaces and fixtures, never methods
        public static bool IsFixture(Node node) =>
            node.IsSuite && node.HasType && !node.HasMethod && node.Children.Any(c => c.HasMethod);

        // Every fixture that ran at least one test, slowest first (ties by name, so the order is stable)
        public static List<Row> Rows(Node root)
        {
            var rows = new List<Row>();
            Collect(root, rows);
            return rows.OrderByDescending(r => r.Seconds).ThenBy(r => r.Fixture, System.StringComparer.Ordinal).ToList();
        }

        private static void Collect(Node node, List<Row> rows)
        {
            if (node == null) return;
            if (IsFixture(node))
            {
                int tests = node.Passed + node.Failed + node.Skipped;
                if (tests > 0)
                    rows.Add(new Row { Fixture = node.FullName, Tests = tests, Passed = node.Passed, Failed = node.Failed, Skipped = node.Skipped, Seconds = node.Seconds });
                return;
            }
            foreach (var child in node.Children) Collect(child, rows);
        }

        // The CSV: the header, then a line per row; seconds with one decimal in the invariant culture, a name with a comma quoted
        public static string ToCsv(IEnumerable<Row> rows)
        {
            var text = new StringBuilder(Header).Append('\n');
            foreach (var r in rows)
                text.Append(Quote(r.Fixture)).Append(',').Append(r.Tests).Append(',').Append(r.Passed).Append(',').Append(r.Failed)
                    .Append(',').Append(r.Skipped).Append(',').Append(r.Seconds.ToString("0.0", CultureInfo.InvariantCulture)).Append('\n');
            return text.ToString();
        }

        private static string Quote(string name) =>
            name.IndexOfAny(new[] { ',', '"' }) < 0 ? name : "\"" + name.Replace("\"", "\"\"") + "\"";
    }
}
