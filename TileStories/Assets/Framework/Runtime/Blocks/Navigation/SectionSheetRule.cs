using System.Collections.Generic;
using System.Globalization;

namespace TileStories
{
    // How the card's scroll reads as CHAPTERS (_3.3 R10.7, R11), pure: the blocks of the scroll, in the stack's order, are cut into RUNS --
    // consecutive blocks of one family -- and each run is one glass sheet (SectionSheets draws it). A family that opens no section
    // (FamilyDefinition.OpensSection false: the navigation family, whose blocks ARE the card's navigation) makes a BARE run: no tint, no
    // opener, nothing laid over it. Under a block order that is not by family (interleaved families) a new run starts wherever the family
    // changes, so a family may have several runs.
    //   - a run is SHOWN while one of its blocks is (a layout sets blocks aside, a block may wait to be revealed)
    //   - a shown run shows its OPENER unless it is bare, or it CONTINUES: the shown run before it is of the same family (a tabs / pages
    //     layout hides the runs between two runs of one family -- the family reads as one chapter, not two)
    //   - a shown run OVERLAPS the one before (is laid over it) when there is a shown run before it and neither of them is bare, unless it
    //     continues it
    //   - the opener's meta line: how many blocks the run holds and, when its blocks give them (Reading Time), their minutes -- never a
    //     clip's length, which would load the clip (CompactRowRule's rule)
    public static class SectionSheetRule
    {
        // One run: its family, where it starts in the scroll's blocks and how many it holds, and whether it is bare
        public readonly struct Run
        {
            public readonly string Family;
            public readonly int Start;
            public readonly int Count;
            public readonly bool Bare;

            public Run(string family, int start, int count, bool bare)
            {
                Family = family;
                Start = start;
                Count = count;
                Bare = bare;
            }
        }

        // How a run shows now
        public readonly struct Look
        {
            public readonly bool Shown;
            public readonly bool Opener;
            public readonly bool Overlaps;
            public readonly bool Continues;

            public Look(bool shown, bool opener, bool overlaps, bool continues)
            {
                Shown = shown;
                Opener = opener;
                Overlaps = overlaps;
                Continues = continues;
            }
        }

        // The runs of the scroll's blocks, by the family of each (in stack order); `opensSection` says whether a family opens a section
        public static List<Run> Runs(IReadOnlyList<string> families, System.Func<string, bool> opensSection)
        {
            var runs = new List<Run>();
            int start = 0;
            for (int i = 1; i <= families.Count; i++)
            {
                if (i < families.Count && families[i] == families[start]) continue;
                string family = families[start];
                runs.Add(new Run(family, start, i - start, !(opensSection?.Invoke(family) ?? true)));
                start = i;
            }
            return runs;
        }

        // How each run shows, given which runs hold a shown block now
        public static Look[] Looks(IReadOnlyList<Run> runs, IReadOnlyList<bool> shown)
        {
            var looks = new Look[runs.Count];
            int previous = -1;
            for (int i = 0; i < runs.Count; i++)
            {
                if (!shown[i])
                {
                    looks[i] = new Look(false, false, false, false);
                    continue;
                }
                bool continues = previous >= 0 && runs[previous].Family == runs[i].Family;
                bool overlaps = previous >= 0 && !continues && !runs[i].Bare && !runs[previous].Bare;
                looks[i] = new Look(true, !runs[i].Bare && !continues, overlaps, continues);
                previous = i;
            }
            return looks;
        }

        // The opener's meta line: "1 block" / "4 blocks", then " · 9 min" in the card's own wording when the blocks give minutes.
        // `words` reads a CardStrings key in the card's language (null: no words)
        public static string Meta(int blocks, int minutes, System.Func<string, string> words)
        {
            string count = string.Format(CultureInfo.InvariantCulture,
                Words(words, blocks == 1 ? CardStrings.Keys.SectionOpenerBlock : CardStrings.Keys.SectionOpenerBlocks), blocks);
            if (minutes <= 0) return count;
            string time = string.Format(CultureInfo.InvariantCulture, Words(words, CardStrings.Keys.NavContentsMinutes), minutes);
            return string.Format(CultureInfo.InvariantCulture, Words(words, CardStrings.Keys.CompactMeta), count, time);
        }

        // The theme colours a section paints for a family, named from the family's own colour token (FamilyDefinition.ColorToken): the ink
        // on its filled pill and its sheet's glass
        public static string InkOf(string colorToken) => colorToken + "-ink";
        public static string GlassOf(string colorToken) => colorToken + "-glass";

        private static string Words(System.Func<string, string> words, string key) => words?.Invoke(key) ?? "";
    }
}
