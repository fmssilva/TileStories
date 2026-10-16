// POIEditorToolWindow.CardNavigation.cs
//
// Partial: Detail Card > Navigation (_3.2_POI_Card_Navigation.md section 7) -- how a visitor moves inside ONE open
// card. The two rows that order the stack: Block Order and the Families table (Order with reorder arrows, Family,
// At The End, one (i) on its header); which cards get navigation, at which heights and what the top strip shows (Show
// Navigation From, Top Bar: step 5a; Show Navigation At: 5b), what hides while the visitor reads (Hide While Scrolling, 5b);
// then how a jump moves the card (Jump Opens Card To, Jump Motion) and what system Back does (Back Button), step 4. The
// other nav elements, layouts and presets join this same section at later steps.
// Its (i) texts and Test guides live in POIEditorToolWindow.CardNavigationHelp.cs.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const float FamilyOrderColumnWidth = 30f;
        // The same width the card's own Items rows give their ^ / v buttons, so every reorder arrow of this tab is one size
        private const float FamilyArrowButtonWidth = 22f;
        private const float FamilyNameColumnWidth = 190f;
        // The gap between two cells of a Families row (GUILayout's own gap between two labels)
        private const float FamilyCellGap = 4f;
        // Where the At The End column starts inside the row's cells, capped on a narrow window so it never runs under the (i)
        private const float FamilyAtEndColumnX = 300f;

        private readonly TestGuideState _cardNavigationTest = new TestGuideState();

        // card_settings.navigation: the order the card shows its blocks in, and what each family does there.
        // Drawing never writes: a family with no row shows its registered defaults, and its row is created by the
        // first real edit (_5.1 "Config, rig and undo", rule 9a).
        private void DrawCardNavigationSection()
        {
            var nav = _config.card_settings.navigation ??= new CardNavigationSettings();

            nav.block_order = DrawPopupField("Block Order", nav.block_order, CardOptions.BlockOrders, CardOptions.BlockOrderLabels, CardBlockOrderHelp);

            DrawFamiliesTable(nav);
            // - a group gap: without it the first row under the table reads as one more family (the 620 pt capture showed it)
            GUILayout.Space(TableGapBetweenGroups);

            // - which cards get navigation, at which heights, and what the strip under the title shows
            nav.min_blocks = DrawIntSliderField("Show Navigation From", nav.min_blocks, CardNavigationSettings.MinBlocksMin, CardNavigationSettings.MinBlocksMax,
                CardShowNavigationFromHelp);
            nav.show_at = DrawPopupField("Show Navigation At", nav.show_at, CardOptions.ShowNavigationAts, CardOptions.ShowNavigationAtLabels, CardShowNavigationAtHelp);
            var (topKeys, topLabels) = TopBarOptions(NavElementRegistry.Shared);
            // - a reference popup: a key no registered element has (an app element taken out) stays as "<key> (missing)", never swapped
            nav.top = DrawReferencePopupField("Top Bar", nav.top, topKeys, topLabels, allowNone: false, CardTopBarHelp(NavElementRegistry.Shared));
            // - section 7's order puts Follow Scroll before it (its row is still to come; its data has been read since step 3)
            nav.hide_on_scroll = DrawPopupField("Hide While Scrolling", nav.hide_on_scroll, CardOptions.HideOnScrolls, CardOptions.HideOnScrollLabels,
                CardHideWhileScrollingHelp);

            // - section 7's order: the jump rows, then Back.
            //   The first option of each is the default the card reads for an unknown value too, so the row and the card always agree
            nav.jump_raises_to = DrawPopupField("Jump Opens Card To", nav.jump_raises_to, CardOptions.JumpRaises, CardOptions.JumpRaiseLabels, CardJumpRaisesHelp);
            nav.jump_motion = DrawPopupField("Jump Motion", nav.jump_motion, CardOptions.JumpMotions, CardOptions.JumpMotionLabels, CardJumpMotionHelp);
            nav.back_button = DrawPopupField("Back Button", nav.back_button, CardOptions.BackButtons, CardOptions.BackButtonLabels, CardBackButtonHelp);

            DrawDomainTestSubSection(_cardNavigationTest, CardNavigationSceneTestGuide, CardNavigationPlaymodeTestGuide, CardNavigationDeviceTestGuide);
        }

        // Top Bar's choices: None, then every element registered for the top slot, named by its Editor label -- listed FROM the registry,
        // so an app's own element is offered here with no edit to the window
        internal static (List<string> Keys, List<string> Labels) TopBarOptions(NavElementRegistry registry)
        {
            var keys = new List<string> { CardOptions.NavNone };
            var labels = new List<string> { CardOptions.NavNoneLabel };
            foreach (var element in registry.OfSlot(NavSlotKeys.Top))
            {
                keys.Add(element.Key);
                labels.Add(element.Label);
            }
            return (keys, labels);
        }

        // A Families row's cells: ONE rect the size and style of a field row's label area (the row less the 36 pt (i)
        // column), reserved in the shared row and split in C# below -- so every (i) drawn after it lands in the
        // SECTION's (i) column, in line with Block Order above, instead of hugging the table's own last column.
        // (The same recipe as the Default Media table; a table that builds its own HorizontalScope cannot do this.)
        private static Rect FamilyCellsRect(float rowWidth)
        {
            float width = Mathf.Max(40f, rowWidth - 36f);
            // - reserved with the popup's style: GUILayout spaces the (i) after it by the style's margin, so the header's (i) lands
            //   exactly where Block Order's popup puts its own (a label style put it 2 pt further right)
            return GUILayoutUtility.GetRect(width, EditorGUIUtility.singleLineHeight, EditorStyles.popup,
                GUILayout.Width(width), GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false));
        }

        private static Rect FamilyOrderRect(Rect cells) => new(cells.x, cells.y, FamilyOrderColumnWidth, cells.height);

        private static Rect FamilyArrowRect(Rect cells, int which) =>
            new(cells.x + FamilyOrderColumnWidth + FamilyCellGap + which * FamilyArrowButtonWidth, cells.y, FamilyArrowButtonWidth, cells.height);

        private static Rect FamilyNameRect(Rect cells)
        {
            float x = cells.x + FamilyOrderColumnWidth + 2f * FamilyCellGap + 2f * FamilyArrowButtonWidth;
            return new Rect(x, cells.y, Mathf.Max(0f, Mathf.Min(FamilyNameColumnWidth, FamilyAtEndRect(cells).x - x - FamilyCellGap)), cells.height);
        }

        // - never past the cells' own end: on a narrow window the toggle moves left instead of under the (i)
        private static Rect FamilyAtEndRect(Rect cells)
        {
            float x = cells.x + Mathf.Min(FamilyAtEndColumnX, Mathf.Max(0f, cells.width - EditorGUIUtility.singleLineHeight));
            return new Rect(x, cells.y, EditorGUIUtility.singleLineHeight, cells.height);
        }

        // One row per registered family, in the order Grouped By Family uses: the arrows that move it, its visitor
        // label with its key, and whether its blocks close the card.
        private void DrawFamiliesTable(CardNavigationSettings nav)
        {
            DrawEditorRow(out float headerRow, out _);
            Rect header = FamilyCellsRect(headerRow);
            GUI.Label(FamilyOrderRect(header), "Order", EditorStyles.miniBoldLabel);
            GUI.Label(FamilyNameRect(header), "Family", EditorStyles.miniBoldLabel);
            // - the header word is wider than its checkbox column, so it is drawn from the column's left edge
            GUI.Label(new Rect(FamilyAtEndRect(header).x, header.y, 70f, header.height), "At The End", EditorStyles.miniBoldLabel);
            HelpInfoButton.Draw("Families", CardFamiliesHelp);
            EditorRowEnd();

            var families = FamiliesInEffectiveOrder(nav);
            // - never move a row inside the loop: the move is applied after it, like every other table of this window
            int moveAt = -1, moveBy = 0;
            for (int i = 0; i < families.Count; i++)
            {
                var family = families[i];
                DrawEditorRow(out float rowWidth, out _);
                Rect cells = FamilyCellsRect(rowWidth);

                GUI.Label(FamilyOrderRect(cells), EffectiveFamilyOrder(nav, family).ToString());
                using (new EditorGUI.DisabledScope(i == 0))
                    if (GUI.Button(FamilyArrowRect(cells, 0), "^", EditorStyles.miniButton)) { moveAt = i; moveBy = -1; }
                ReportTableCellRect("Family up", i, FamilyArrowRect(cells, 0));
                using (new EditorGUI.DisabledScope(i >= families.Count - 1))
                    if (GUI.Button(FamilyArrowRect(cells, 1), "v", EditorStyles.miniButton)) { moveAt = i; moveBy = 1; }
                ReportTableCellRect("Family down", i, FamilyArrowRect(cells, 1));

                // - the visitor's own word for the family first, its key after it: name things, never bare ids
                GUI.Label(FamilyNameRect(cells), FamilyRowName(family));

                bool atEnd = EffectiveFamilyAtEnd(nav, family);
                bool ticked = EditorGUI.Toggle(FamilyAtEndRect(cells), atEnd);
                ReportTableCellRect("Family at end", i, FamilyAtEndRect(cells));
                if (ticked != atEnd) SetFamilyAtEnd(nav, family.Key, ticked);
                // - no (i) per row: the header's one says everything a row's would (they all opened CardFamiliesHelp)
                EditorRowEnd();
            }

            if (moveAt >= 0) MoveFamily(nav, families, moveAt, moveBy);
        }

        // Every registered family, sorted by its effective order (the wall's row when it has one, else the registered default)
        internal List<FamilyDefinition> FamiliesInEffectiveOrder(CardNavigationSettings nav)
        {
            var families = new List<FamilyDefinition>(BlockRegistry.Shared.Families);
            // - a stable insertion sort: two families sharing an order keep their registration order, so the table never jumps
            for (int i = 1; i < families.Count; i++)
            {
                var family = families[i];
                int order = EffectiveFamilyOrder(nav, family);
                int at = i;
                while (at > 0 && EffectiveFamilyOrder(nav, families[at - 1]) > order) { families[at] = families[at - 1]; at--; }
                families[at] = family;
            }
            return families;
        }

        // The words the table shows for a family: its visitor label in the wall's first language, then its key
        private string FamilyRowName(FamilyDefinition family)
        {
            var table = FrameworkCardStrings();
            var languages = _config.card_settings?.languages;
            string language = languages != null && languages.Count > 0 ? languages[0] : "en";
            string label = null;
            if (table != null && !string.IsNullOrEmpty(family.LabelKey))
            {
                var entries = table.Entries();
                // - the wall's own wording of the label wins, exactly as the card shows it
                label = CardTextOverride(_config.card_settings, family.LabelKey, language);
                if (string.IsNullOrWhiteSpace(label)) label = CardStrings.Find(entries, family.LabelKey, language);
                if (string.IsNullOrWhiteSpace(label)) label = CardStrings.Find(entries, family.LabelKey, "en");
            }
            return string.IsNullOrWhiteSpace(label) ? family.Key : label + " (" + family.Key + ")";
        }

        internal static int EffectiveFamilyOrder(CardNavigationSettings nav, FamilyDefinition family) =>
            FamilyRowOf(nav, family.Key)?.order ?? family.Order;

        internal static bool EffectiveFamilyAtEnd(CardNavigationSettings nav, FamilyDefinition family) =>
            FamilyRowOf(nav, family.Key)?.at_end ?? family.AtEnd;

        private static CardFamilySetting FamilyRowOf(CardNavigationSettings nav, string key) =>
            nav?.families?.Find(f => f != null && f.family == key);

        // Move a family past its neighbour: the two swap their effective orders, so only the two families the
        // developer actually moved get a row (a family nobody touched keeps its registered default).
        internal void MoveFamily(CardNavigationSettings nav, List<FamilyDefinition> families, int index, int direction)
        {
            int target = index + direction;
            if (families == null || index < 0 || index >= families.Count || target < 0 || target >= families.Count) return;
            int mine = EffectiveFamilyOrder(nav, families[index]);
            int theirs = EffectiveFamilyOrder(nav, families[target]);
            // - equal orders would make the swap a no-op: push them one apart in the direction of travel
            if (mine == theirs) theirs = mine + direction;
            string movedKey = families[index].Key, otherKey = families[target].Key;
            POIEditorToolWindow.ReorderRow(families, index, direction);
            EnsureFamilyRow(nav, movedKey).order = theirs;
            EnsureFamilyRow(nav, otherKey).order = mine;
        }

        internal static void SetFamilyAtEnd(CardNavigationSettings nav, string key, bool atEnd) => EnsureFamilyRow(nav, key).at_end = atEnd;

        // The wall's row for a family, created on the first real edit and seeded with the family's registered values
        private static CardFamilySetting EnsureFamilyRow(CardNavigationSettings nav, string key)
        {
            nav.families ??= new List<CardFamilySetting>();
            var row = FamilyRowOf(nav, key);
            if (row != null) return row;
            BlockRegistry.Shared.TryGetFamily(key, out var family);
            nav.families.Add(row = new CardFamilySetting
            {
                family = key,
                order = family != null ? family.Order : 0,
                at_end = family != null && family.AtEnd,
            });
            return row;
        }
    }
}
