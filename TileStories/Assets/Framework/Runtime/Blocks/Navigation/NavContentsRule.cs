namespace TileStories
{
    // How the contents block (nav_contents, _3.2 section 5.6) lists its rows (_3.2 step 13), pure: progressive disclosure (30-ui-content) on
    // exactly the long card it serves -- The Lamp's contents is 56 rows under 8 titles, about 2,700 px.
    //   - Sections First (the default): the section titles only, each opening its own rows (AccordionRule, the card's one accordion
    //     mechanism; sections open independently)
    //   - Every Block: every row under its title, as at step 11b
    // A contents block the CARD inserted (Auto Nav Block, a synthesized entry) always opens with its sections folded -- the visitor did not
    // ask for it, so it takes the least room; one a POI authored follows its own Rows, else the wall's Block Library default
    public static class NavContentsRule
    {
        public static bool SectionsFirst(BlockStackBuilder.Entry entry, CardSettings settings)
        {
            if (entry.Synthesized) return true;
            var field = entry.Definition?.Field(BuiltInBlocks.NavContentsRowsField);
            if (field == null) return true;
            string rows = BlockLibraryRule.Choice(settings, entry.Definition, field, new BlockFieldReader(entry.Instance, null, null));
            return rows != BuiltInBlocks.NavContentsEveryBlock;
        }

        // How many of a section's targets were read (what its folded title says)
        public static int ReadCount(System.Collections.Generic.IReadOnlyList<string> targets, System.Func<string, bool> wasRead)
        {
            int read = 0;
            if (targets == null || wasRead == null) return read;
            foreach (string target in targets)
                if (wasRead(target)) read++;
            return read;
        }
    }
}
