using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // Pure: reads TestRunRecorder's log and finds every time Unity became the active application during the LAST run in it
    // (a "jump"), with the test that was running and the events of the few seconds before it -- the table that tells which
    // part of a run pulls Unity to the front. TestRunReportTests runs it on a recorded log excerpt.
    public sealed class TestRunReport
    {
        // How far back "just before" reaches
        public const double ContextSeconds = 3.0;

        // How many of those events a jump lists (the newest ones)
        public const int MaxContextEvents = 6;

        public sealed class Entry
        {
            public DateTime Time;
            public string Kind;
            public string Detail;
        }

        public sealed class Jump
        {
            public int Number;
            public DateTime Time;
            public TimeSpan SinceRunStart;
            public string Where;          // "in <test> (setup|body|teardown)", "between <a> and <b>", "before the first test", "after the last test"
            public string FocusedWindow;  // the FocusGained line's own detail
            public List<Entry> JustBefore = new();
        }

        public DateTime RunStart;
        public DateTime? RunEnd;
        public int Tests;
        public int FocusLosses;
        public readonly List<Jump> Jumps = new();

        public string Summary => "run of " + Tests + " tests, " + Jumps.Count + " focus gains after its start, " + FocusLosses + " losses";

        // One log line -> an entry, or null for a line that is not one (a blank or a torn last line)
        public static Entry Parse(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            string[] parts = line.Split(new[] { '\t' }, 3);
            if (parts.Length < 2) return null;
            if (!DateTime.TryParseExact(parts[0], "yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                return null;
            return new Entry { Time = time, Kind = parts[1], Detail = parts.Length > 2 ? parts[2] : "" };
        }

        // The kinds that can explain a jump (everything but the tests' own markers and the focus lines)
        private static bool IsContext(string kind) =>
            kind != TestRunRecorder.TestStart && kind != TestRunRecorder.BodyStart && kind != TestRunRecorder.BodyEnd
            && kind != TestRunRecorder.TestEnd && kind != TestRunRecorder.FocusGained && kind != TestRunRecorder.RunStart;

        // The last run of the log: from its last RunStart to the RunEnd after it (or the end of the file while it still runs)
        public static TestRunReport Build(IEnumerable<string> lines)
        {
            var entries = lines.Select(Parse).Where(e => e != null).ToList();
            int start = entries.FindLastIndex(e => e.Kind == TestRunRecorder.RunStart);
            if (start < 0) throw new ArgumentException("the log holds no RunStart line: was the recorder ON when the run started?");
            int end = entries.FindIndex(start + 1, e => e.Kind == TestRunRecorder.RunEnd);
            var run = entries.GetRange(start, (end < 0 ? entries.Count : end + 1) - start);

            var report = new TestRunReport { RunStart = run[0].Time, RunEnd = end < 0 ? (DateTime?)null : run[run.Count - 1].Time };
            string openTest = null, phase = null, lastEnded = null;
            var waitingForNextTest = new List<Jump>();

            for (int i = 0; i < run.Count; i++)
            {
                var e = run[i];
                if (e.Kind == TestRunRecorder.TestStart)
                {
                    foreach (var jump in waitingForNextTest) jump.Where += " and " + e.Detail;
                    waitingForNextTest.Clear();
                    openTest = e.Detail;
                    phase = "setup";
                    report.Tests++;
                }
                else if (e.Kind == TestRunRecorder.BodyStart) phase = "body";
                else if (e.Kind == TestRunRecorder.BodyEnd) phase = "teardown";
                else if (e.Kind == TestRunRecorder.TestEnd)
                {
                    lastEnded = openTest ?? e.Detail;
                    openTest = null;
                }
                else if (e.Kind == TestRunRecorder.FocusLost) report.FocusLosses++;
                else if (e.Kind == TestRunRecorder.FocusGained)
                {
                    var jump = new Jump
                    {
                        Number = report.Jumps.Count + 1,
                        Time = e.Time,
                        SinceRunStart = e.Time - report.RunStart,
                        FocusedWindow = e.Detail,
                        JustBefore = run.Take(i).Where(c => IsContext(c.Kind) && (e.Time - c.Time).TotalSeconds <= ContextSeconds)
                            .Reverse().Take(MaxContextEvents).Reverse().ToList(),
                    };
                    if (openTest != null) jump.Where = "in " + openTest + " (" + phase + ")";
                    else if (lastEnded == null) jump.Where = "before the first test";
                    else
                    {
                        jump.Where = "between " + lastEnded;
                        waitingForNextTest.Add(jump);
                    }
                    report.Jumps.Add(jump);
                }
            }
            foreach (var jump in waitingForNextTest) jump.Where = "after the last test (" + lastEnded + ")";
            return report;
        }

        // The table: one block per jump, then the summary
        public string ToText()
        {
            var text = new StringBuilder();
            text.AppendLine("Test run " + RunStart.ToString("yyyy-MM-dd HH:mm:ss") + " -> "
                + (RunEnd.HasValue ? RunEnd.Value.ToString("HH:mm:ss") + " (" + (RunEnd.Value - RunStart).ToString(@"hh\:mm\:ss") + ")" : "still running"));
            text.AppendLine(Summary);
            foreach (var jump in Jumps)
            {
                text.AppendLine();
                text.AppendLine("#" + jump.Number + "  " + jump.Time.ToString("HH:mm:ss.fff") + "  +" + jump.SinceRunStart.ToString(@"hh\:mm\:ss")
                    + "  " + jump.Where);
                text.AppendLine("    gained: " + jump.FocusedWindow);
                if (jump.JustBefore.Count == 0) text.AppendLine("    just before (" + ContextSeconds + " s): nothing recorded");
                foreach (var c in jump.JustBefore)
                    text.AppendLine("    -" + (jump.Time - c.Time).TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) + " s  " + c.Kind + "  " + c.Detail);
            }
            return text.ToString();
        }
    }
}
