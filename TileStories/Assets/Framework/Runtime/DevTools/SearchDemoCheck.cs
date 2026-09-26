using System.Collections.Generic;

namespace TileStories
{
    // Runs the search demo's test cases (SearchDemoLayout.TestCases) against a REAL index with the wall's
    // current search settings: for each case, is its POI found, and should it be? The POI Editor's Live
    // Search Readout prints the verdicts ("Demo Test Cases"), so switching Typo Tolerance to 0 visibly
    // flips the typo lines; the tests assert every verdict is right for every combination of settings. Pure.
    public static class SearchDemoCheck
    {
        public readonly struct Verdict
        {
            public readonly SearchDemoLayout.TestCase Case;
            public readonly bool Expected;
            public readonly bool Found;

            public Verdict(SearchDemoLayout.TestCase testCase, bool expected, bool found)
            {
                Case = testCase;
                Expected = expected;
                Found = found;
            }

            public bool Ok => Expected == Found;
        }

        // Every test case, searched the way the visitor's search runs (no filter)
        public static List<Verdict> Evaluate(POISearchIndex index, SearchSettings settings)
        {
            var verdicts = new List<Verdict>();
            if (index == null) return verdicts;
            settings ??= new SearchSettings();
            var options = SelectFilterSearchOptions.ToSearchOptions(settings);
            foreach (var c in SearchDemoLayout.TestCases())
            {
                bool found = false;
                foreach (var r in index.Search(c.Query, options))
                    if (r.PoiId == c.PoiId) { found = true; break; }
                verdicts.Add(new Verdict(c, c.Expected(settings), found));
            }
            return verdicts;
        }

        // One readout line: "[ok] 'lantren' finds Lantern Tower -- one typo (Typo Tolerance)"
        public static string Line(Verdict v)
        {
            string mark = v.Ok ? "[ok]" : "[!!]";
            string what = v.Found ? "finds " + v.Case.PoiName : "does not find " + v.Case.PoiName;
            string why = string.IsNullOrEmpty(v.Case.DependsOn) ? v.Case.Note : v.Case.Note + " (" + v.Case.DependsOn + ")";
            string wrong = v.Ok ? "" : v.Expected ? " -- it should" : " -- it should not";
            return $"{mark} '{v.Case.Query}' {what}{wrong} -- {why}";
        }
    }
}
