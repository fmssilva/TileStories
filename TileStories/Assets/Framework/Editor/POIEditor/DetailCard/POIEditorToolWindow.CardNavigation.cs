// POIEditorToolWindow.CardNavigation.cs
//
// Partial: Detail Card > Navigation (_3.2_POI_Card_Navigation.md section 7) -- how a visitor moves inside ONE open
// card. First the Preset (step 17a; first since 17b: where a wall author starts), then the Layout (Scroll, Accordion, Tabs: step 13, with a warning for Tabs with no bar to switch sections; its Open One At A
// Time row, accordion only, comes after Placed Model Control). The two rows that order the stack: Block Order and the Families table (Order with reorder arrows, Family,
// At The End, Group, one (i) on its header); Group By; which cards get navigation, at which heights and what the top strip
// shows (Show Navigation From, Top Bar: step 5a; Show Navigation At: 5b; Labels, Show Counts: step 6, each only for an element
// that reads it), what the bar under the stack shows (Bottom Bar and its Bar Slots, step 7a, Bar Slots only for an element that is a
// bar), what the header's menu button opens over the card (Menu, step 8), the round button above the card's top-right edge (Floating
// Button, step 9; Bar Slots for the Radial too), the dots down the right edge of the content (Edge, step 10), the nav block every long card
// gets after its header (Auto Nav Block, step 11a), Follow Scroll (step 6), what hides while the visitor reads (Hide While Scrolling, 5b); then how a jump moves
// the card (Jump Opens Card To, Jump Motion) and what system Back does (Back Button), step 4; what tells the visitor a model placed in the room
// stands (Placed Model Control, step 9); the Groups table last (step 6,
// only for the Segmented bar, POIEditorToolWindow.CardNavigationGroups.cs). The other nav elements, layouts and presets join
// this same section at later steps. Its (i) texts and Test guides live in POIEditorToolWindow.CardNavigationHelp.cs.

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

            // - the busiest card of the wall, built only when a warning below needs it (a card's map costs a stack build per point)
            bool busiestKnown = false;
            NavigationWarningRule.BusiestCard busiest = default;
            int BusiestSections()
            {
                if (!busiestKnown)
                {
                    busiest = NavigationWarningRule.BusiestOf(_config.card_settings, _config.pois, NavigationRegistry);
                    busiestKnown = true;
                }
                return busiest.Sections;
            }

            // - section 7's order: the Preset first -- it is where a wall author STARTS, then adjusts the rows under it (decided at the 17a
            //   review); a gap sets it apart, since it writes seven of the rows below rather than being one of them
            DrawNavigationPresetRow(nav);
            GUILayout.Space(TableGapBetweenGroups);

            // - then the Layout -- it decides what most rows below mean (step 13). A layout of a later step reads as Scroll
            nav.layout = DrawPopupField("Layout", nav.layout, CardOptions.Layouts, CardOptions.LayoutLabels, CardLayoutHelp);
            if (CardLayoutRule.TabsWithoutAnElement(nav))
                DrawNavigationMessage(CardTabsWithoutABarWarning, MessageType.Warning);
            if (NavigationWarningRule.BentoOutgrown(nav, BusiestSections))
                DrawNavigationMessage(CardBentoWarning(NavigationCardName(busiest.PoiIndex), busiest.Sections), MessageType.Warning);

            nav.block_order = DrawPopupField("Block Order", nav.block_order, CardOptions.BlockOrders, CardOptions.BlockOrderLabels, CardBlockOrderHelp);

            // - which of the wall's options the Top Bar's element reads: a row (or column) it ignores is hidden and keeps its value
            //   (_3.2 section 3). Read from the element's own definition, so an app's element needs no edit here
            //   The top element is the one the card really draws (CardLayoutRule.TopElement: Pages always uses the Family Pager)
            NavElementRegistry.Shared.TryGet(CardLayoutRule.TopElement(nav), out var topElement);
            NavElementRegistry.Shared.TryGet(nav.bottom, out var bottomElement);
            NavElementRegistry.Shared.TryGet(nav.floating, out var floatingElement);
            // - an element counts only in its own slot: a hand-edited key of the other slot draws nothing there
            if (topElement != null && topElement.Slot != NavSlotKeys.Top) topElement = null;
            if (bottomElement != null && bottomElement.Slot != NavSlotKeys.Bottom) bottomElement = null;
            if (floatingElement != null && floatingElement.Slot != NavSlotKeys.Floating) floatingElement = null;
            bool usesGroups = topElement != null && topElement.Choice == NavChoice.Groups;

            DrawFamiliesTable(nav, usesGroups);
            // - a group gap: without it the first row under the table reads as one more family (the 620 pt capture showed it)
            GUILayout.Space(TableGapBetweenGroups);

            nav.group_by = DrawPopupField("Group By", nav.group_by, CardOptions.GroupBys, CardOptions.GroupByLabels, CardGroupByHelp);
            // - which cards get navigation, at which heights, and what the strip under the title shows
            nav.min_blocks = DrawIntSliderField("Show Navigation From", nav.min_blocks, CardNavigationSettings.MinBlocksMin, CardNavigationSettings.MinBlocksMax,
                CardShowNavigationFromHelp);
            nav.show_at = DrawPopupField("Show Navigation At", nav.show_at, CardOptions.ShowNavigationAts, CardOptions.ShowNavigationAtLabels, CardShowNavigationAtHelp);
            var (topKeys, topLabels) = SlotOptions(NavElementRegistry.Shared, NavSlotKeys.Top);
            // - a reference popup: a key no registered element has (an app element taken out) stays as "<key> (missing)", never swapped.
            //   Hidden where the layout picks the top bar itself (Pages: the Family Pager; Bento: none, the tiles are the map), keeping its
            //   value for the day the wall picks another layout -- the card's own rule, so the row and the card agree
            if (!CardLayoutRule.LayoutPicksTheTopBar(nav))
            {
                nav.top = DrawReferencePopupField("Top Bar", nav.top, topKeys, topLabels, allowNone: false, CardTopBarHelp(NavElementRegistry.Shared));
                if (NavigationWarningRule.ProgressMapOutgrown(nav, BusiestSections))
                    DrawNavigationMessage(CardProgressMapWarning(NavigationCardName(busiest.PoiIndex), busiest.Sections), MessageType.Warning);
            }
            var (bottomKeys, bottomLabels) = SlotOptions(NavElementRegistry.Shared, NavSlotKeys.Bottom);
            nav.bottom = DrawReferencePopupField("Bottom Bar", nav.bottom, bottomKeys, bottomLabels, allowNone: false, CardBottomBarHelp(NavElementRegistry.Shared));
            // - section 7's order: the Menu right after the two bars (its button sits in the header; a bar's More opens it)
            var (menuKeys, menuLabels) = SlotOptions(NavElementRegistry.Shared, NavSlotKeys.Menu);
            nav.menu = DrawReferencePopupField("Menu", nav.menu, menuKeys, menuLabels, allowNone: false, CardMenuHelp(NavElementRegistry.Shared));
            // - section 7's order: the Floating Button after the Menu (the round button above the card's top-right edge, step 9)
            var (floatingKeys, floatingLabels) = SlotOptions(NavElementRegistry.Shared, NavSlotKeys.Floating);
            nav.floating = DrawReferencePopupField("Floating Button", nav.floating, floatingKeys, floatingLabels, allowNone: false,
                CardFloatingButtonHelp(NavElementRegistry.Shared));
            // - section 7's order: the Edge after the Floating Button (the dots down the right edge of the content, step 10)
            var (edgeKeys, edgeLabels) = SlotOptions(NavElementRegistry.Shared, NavSlotKeys.Edge);
            nav.edge = DrawReferencePopupField("Edge", nav.edge, edgeKeys, edgeLabels, allowNone: false, CardEdgeHelp(NavElementRegistry.Shared));
            // - the third bar: the Edge is the last of the three rows, so the warning sits under it
            if (NavigationWarningRule.MoreThanTwoBars(nav))
                DrawNavigationMessage(CardMoreThanTwoBarsWarning, MessageType.Warning);
            // - section 7's order: Auto Nav Block after the Edge (a nav block put after the header of every long card, step 11a). Its choices
            //   are the registered kinds of the navigation family, so a nav block an app (or a later step) registers is listed with no edit here
            var (autoKeys, autoLabels) = AutoNavBlockOptions(BlockRegistry.Shared);
            nav.auto_nav_block = DrawReferencePopupField("Auto Nav Block", nav.auto_nav_block, autoKeys, autoLabels, allowNone: false,
                CardAutoNavBlockHelp(BlockRegistry.Shared));
            // - a display option shows while EITHER bar reads it: both bars follow the same wall setting
            if (topElement != null && topElement.ReadsLabels || bottomElement != null && bottomElement.ReadsLabels)
                nav.labels = DrawPopupField("Labels", nav.labels, CardOptions.Labels, CardOptions.LabelsLabels, CardLabelsHelp);
            if (topElement != null && topElement.ShowsCounts || bottomElement != null && bottomElement.ShowsCounts)
                nav.show_counts = DrawToggleField("Show Counts", nav.show_counts, CardShowCountsHelp);
            // - a bar's slots, or the radial's arc (its items cap at Bar Slots like a bar's)
            if (bottomElement != null && bottomElement.ReadsBarSlots || floatingElement != null && floatingElement.ReadsBarSlots)
            {
                nav.max_slots = DrawIntSliderField("Bar Slots", nav.max_slots, CardNavigationSettings.MaxSlotsMin, CardNavigationSettings.MaxSlotsMax,
                    CardBarSlotsHelp);
                if (NavigationWarningRule.BarSlotsBelowTheSections(nav, true, BusiestSections))
                    DrawNavigationMessage(CardBarSlotsWarning(NavOntologyRule.MaxSlots(nav), NavigationCardName(busiest.PoiIndex), busiest.Sections), MessageType.Warning);
            }
            nav.follow_scroll = DrawToggleField("Follow Scroll", nav.follow_scroll, CardFollowScrollHelp);
            nav.hide_on_scroll = DrawPopupField("Hide While Scrolling", nav.hide_on_scroll, CardOptions.HideOnScrolls, CardOptions.HideOnScrollLabels,
                CardHideWhileScrollingHelp);

            // - section 7's order: the jump rows, then Back.
            //   The first option of each is the default the card reads for an unknown value too, so the row and the card always agree
            nav.jump_raises_to = DrawPopupField("Jump Opens Card To", nav.jump_raises_to, CardOptions.JumpRaises, CardOptions.JumpRaiseLabels, CardJumpRaisesHelp);
            nav.jump_motion = DrawPopupField("Jump Motion", nav.jump_motion, CardOptions.JumpMotions, CardOptions.JumpMotionLabels, CardJumpMotionHelp);
            nav.back_button = DrawPopupField("Back Button", nav.back_button, CardOptions.BackButtons, CardOptions.BackButtonLabels, CardBackButtonHelp);
            // - section 7's order: what tells the visitor a placed model stands, after Back (step 9)
            nav.placed_model_control = DrawPopupField("Placed Model Control", nav.placed_model_control, CardOptions.PlacedModelControls,
                CardOptions.PlacedModelControlLabels, CardPlacedModelControlHelp);
            // - section 7's order: Open One At A Time after Placed Model Control, an accordion option (hidden elsewhere, keeping its value)
            if (CardLayoutRule.Layout(nav) == CardOptions.LayoutAccordion)
                nav.accordion_one_open = DrawToggleField("Open One At A Time", nav.accordion_one_open, CardOpenOneAtATimeHelp);

            // - section 7 puts the Groups table last: it only matters to the Segmented Top Bar
            if (usesGroups)
            {
                GUILayout.Space(TableGapBetweenGroups);
                DrawGroupsTable(nav);
            }

            DrawDomainTestSubSection(_cardNavigationTest, CardNavigationSceneTestGuide, CardNavigationPlaymodeTestGuide, CardNavigationDeviceTestGuide,
                DrawNavigationPreviewRows);
        }

        // A slot's choices (Top Bar, Bottom Bar): None, then every element registered for that slot, named by its Editor label -- listed
        // FROM the registry, so an app's own element is offered here with no edit to the window
        internal static (List<string> Keys, List<string> Labels) SlotOptions(NavElementRegistry registry, string slot)
        {
            var keys = new List<string> { CardOptions.NavNone };
            var labels = new List<string> { CardOptions.NavNoneLabel };
            foreach (var element in registry.OfSlot(slot))
            {
                keys.Add(element.Key);
                labels.Add(element.Label);
            }
            return (keys, labels);
        }

        // Auto Nav Block's choices: None, then every registered kind of the navigation family the card can put in by itself (not one whose
        // content an author must fill in: Highlights), named by its Editor name (the Block Library's)
        internal static (List<string> Keys, List<string> Labels) AutoNavBlockOptions(BlockRegistry registry)
        {
            var keys = new List<string> { CardOptions.NavNone };
            var labels = new List<string> { CardOptions.NavNoneLabel };
            foreach (var kind in registry.Ordered)
            {
                if (kind.Family != BuiltInFamilies.Navigation || kind.NeedsAuthoredContent) continue;
                keys.Add(kind.Key);
                labels.Add(kind.DisplayName);
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
        // label with its key, whether its blocks close the card, and -- with a Top Bar that shows Groups -- the group it joins.
        private void DrawFamiliesTable(CardNavigationSettings nav, bool showGroup)
        {
            DrawEditorRow(out float headerRow, out _);
            Rect header = FamilyCellsRect(headerRow);
            GUI.Label(FamilyOrderRect(header), "Order", EditorStyles.miniBoldLabel);
            GUI.Label(FamilyNameRect(header), "Family", EditorStyles.miniBoldLabel);
            // - the header word is wider than its checkbox column, so it is drawn from the column's left edge
            GUI.Label(new Rect(FamilyAtEndRect(header).x, header.y, 70f, header.height), "At The End", EditorStyles.miniBoldLabel);
            if (showGroup) GUI.Label(FamilyGroupRect(header, headerRow: true), "Group", EditorStyles.miniBoldLabel);
            HelpInfoButton.Draw("Families", CardFamiliesHelp);
            EditorRowEnd();

            var (groupKeys, groupNames) = showGroup ? GroupChoices(nav) : (null, null);
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
                // - a family left out of navigation (the Sources) is in no segment whatever it picks: no Group to pick
                if (showGroup && family.InNav) DrawFamilyGroupCell(nav, family, FamilyGroupRect(cells, headerRow: false), i, groupKeys, groupNames);
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
            string words = FamilyWords(family);
            return string.IsNullOrWhiteSpace(words) ? family.Key : words + " (" + family.Key + ")";
        }

        // A family's visitor label in the wall's first language (the wall's own wording first), "" when no table names it -- what a
        // message names a family by
        private string FamilyWords(FamilyDefinition family)
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
            return label ?? "";
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

        // The Group cell starts after the "At The End" header WORD (wider than its checkbox: measured, the 620 pt capture showed the two
        // headers run together). A family row has no (i) of its own, so its popup may also use the row's free (i) column; the header row
        // keeps that column for the table's (i)
        private static Rect FamilyGroupRect(Rect cells, bool headerRow)
        {
            float atEndWord = EditorStyles.miniBoldLabel.CalcSize(new GUIContent("At The End")).x;
            float x = FamilyAtEndRect(cells).x + atEndWord + FamilyCellGap;
            float end = headerRow ? cells.xMax : cells.xMax + 36f - FamilyCellGap;
            return new Rect(x, cells.y, Mathf.Max(0f, end - x), cells.height);
        }
    }
}
