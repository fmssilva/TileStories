using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // ElementEdits (_3.3 S3a) -- every edit the story's Editor makes -- on The Painting's SHIPPED story (block_19, 17 rows) with the card's
    // own registries: what each edit writes, what the card's rules then read (the Automatic preview, links, overrides, reading time), and
    // that row ids are never handed out twice.
    public class ElementEditsTests
    {
        private WallConfigData _config;
        private POIData _painting;
        private BlockInstanceData _story;
        private BlockKindDefinition _kind;
        private ElementScope _scope;
        private static ElementRegistry Elements => ElementRegistry.Shared;

        [SetUp]
        public void LoadTheShippedStory()
        {
            _config = PaintingStory.ShippedConfig();
            _painting = _config.pois.Single(p => p.id == PaintingStory.PaintingId);
            _story = _painting.card.blocks.Single(b => b.kind == BuiltInBlocks.StoryKind);
            Assert.IsTrue(BlockRegistry.Shared.TryGet(BuiltInBlocks.StoryKind, out _kind));
            _scope = ElementScope.Of(_painting, _story, BlockRegistry.Shared);
            Assert.AreEqual(17, Page.Count, "precondition: the shipped story's 17 rows");
            Assert.AreEqual(ElementOptions.PreviewAutomatic, BlockSurfaces.PreviewMode(_story), "precondition: an Automatic preview");
        }

        private List<BlockItemData> Page => ElementEdits.PageList(_story, _kind);
        private List<string> PageIds => Page.Select(r => new ElementRow(r).Id).ToList();
        private ElementPreviewRule.Result Preview => ElementPreviewRule.Of(_scope, Elements);
        private ElementRow Row(string id) => new(ElementEdits.Find(Page, id));
        private static ElementKindDefinition Element(string key) => Elements.TryGet(key, out var e) ? e : null;

        [Test]
        public void Add_AppendsARowBornWithItsDefaults_AndANewIdPastEveryOneEverHandedOut()
        {
            var callout = Element(BuiltInElements.Callout);
            var row = ElementEdits.Add(_story, Page, callout);
            Assert.AreSame(row.Data, Page.Last(), "appended at the end");
            Assert.AreEqual("el_18", row.Id, "one past the block's 17");
            Assert.AreEqual(BuiltInElements.Callout, row.Kind);
            foreach (var style in callout.StyleFields) Assert.AreEqual(style.ChoiceDefault, row.Value(style.Key), style.Key + " at its default");
            Assert.AreEqual(ElementOptions.MotionAuto, row.Motion);
            Assert.IsFalse(row.Validated, "a new row waits to be validated");
            Assert.AreEqual(18, BlockSurfaces.RowsToValidate(_story, _kind));

            ElementEdits.Remove(Page, Page.Count - 1);
            Assert.AreEqual("el_19", ElementEdits.Add(_story, Page, callout).Id, "a deleted row's id is never handed out again");
        }

        [Test]
        public void Duplicate_PutsACopyRightUnder_SameContent_NewId_NotValidated()
        {
            int at = PageIds.IndexOf("el_2");
            Row("el_2").SetValidated(true);
            var copy = ElementEdits.Duplicate(_story, Page, at);
            Assert.AreSame(copy.Data, Page[at + 1], "right under its source");
            Assert.AreEqual("el_18", copy.Id);
            Assert.IsFalse(copy.Validated, "a copy is checked again");
            Assert.AreEqual(Row("el_2").Field(BuiltInElements.TextField).text.Select(e => e.value), copy.Field(BuiltInElements.TextField).text.Select(e => e.value));
            Assert.AreNotSame(Row("el_2").Field(BuiltInElements.TextField).text, copy.Field(BuiltInElements.TextField).text, "its own words, not shared");
        }

        [Test]
        public void MoveAndRemove_ReorderThePage_AndTheAutomaticPreviewFollows()
        {
            CollectionAssert.AreEqual(new[] { "el_12", "el_1", "el_2", "el_3" }, Preview.UsedPageRows, "precondition: picture, eyebrow, title, lede");
            // - the title to the end: the first heading becomes the preview's title; the eyebrow no longer touches a title it could lead
            ElementEdits.Move(Page, PageIds.IndexOf("el_2"), Page.Count - 1);
            Assert.AreEqual("el_2", PageIds.Last());
            CollectionAssert.AreEqual(new[] { "el_12", "el_3", "el_5" }, Preview.UsedPageRows, "picture, lede, then the heading in Page order");
            // - delete the first picture: the next one takes its place
            ElementEdits.Remove(Page, PageIds.IndexOf("el_12"));
            CollectionAssert.AreEqual(new[] { "el_3", "el_13", "el_5" }, Preview.UsedPageRows, "the next picture, in Page order");
            ElementEdits.Move(Page, 0, 99);
            ElementEdits.Move(Page, -1, 0);
            Assert.AreEqual("el_1", PageIds[0], "a move out of range does nothing");
        }

        [Test]
        public void Custom_StartsAsTheAutomaticPreviewShowedIt_AndSwitchingBackKeepsItsRows()
        {
            var automatic = Preview.Entries.Select(e => e.Content).ToList();
            ElementEdits.SetPreviewMode(_scope, Elements, ElementOptions.PreviewCustom);
            var custom = Preview;
            Assert.AreEqual(ElementOptions.PreviewCustom, custom.Mode);
            Assert.IsTrue(custom.Entries.All(e => e.Source == ElementPreviewRule.Source.Linked), "one link per automatic pick");
            CollectionAssert.AreEqual(automatic, custom.Entries.Select(e => e.Content), "the card shows the same rows at the switch");
            Assert.IsTrue(BlockSurfaces.PreviewRows(_story).All(r => new ElementRow(r).Kind.Length > 0), "each link records the element it shows");

            ElementEdits.SetPreviewMode(_scope, Elements, ElementOptions.PreviewAutomatic);
            Assert.AreEqual(4, BlockSurfaces.PreviewRows(_story).Count, "Automatic again: the Custom rows are kept");
            ElementEdits.SetPreviewMode(_scope, Elements, ElementOptions.PreviewCustom);
            Assert.AreEqual(4, BlockSurfaces.PreviewRows(_story).Count, "and not seeded twice");
        }

        [Test]
        public void UseInThePreview_LinkedOrCopied_TurnsItCustom_LinksFollowThePage_CopiesAreTheirOwn()
        {
            var link = ElementEdits.UseInPreview(_scope, Elements, "el_8", linked: true);
            Assert.AreEqual(ElementOptions.PreviewCustom, BlockSurfaces.PreviewMode(_story), "the preview turned Custom");
            Assert.AreEqual(5, BlockSurfaces.PreviewRows(_story).Count, "the four picks, then the new link");
            Assert.IsTrue(link.IsRef);
            Assert.AreEqual(BuiltInElements.PullQuote, link.Kind, "the link knows the element it shows");
            CollectionAssert.Contains(Preview.UsedPageRows, "el_8", "the Page row is marked as used by the preview");

            var copy = ElementEdits.UseInPreview(_scope, Elements, "el_3", linked: false);
            Assert.IsFalse(copy.IsRef, "a copy is a row of its own");
            Assert.AreEqual(BuiltInElements.Lede, copy.Kind);
            Row("el_3").Field(BuiltInElements.TextField).text[0].value = "Edited on the Page";
            Assert.AreNotEqual("Edited on the Page", copy.Field(BuiltInElements.TextField).text[0].value, "a copy does not follow the Page");
            var linkEntry = Preview.Entries.Single(e => ReferenceEquals(e.Row, link.Data));
            Assert.AreSame(Row("el_8").Data, linkEntry.Content, "a link shows the Page row itself");
        }

        [Test]
        public void Unlink_CopiesTheContent_KeepsTheLinksOwnLookAndIdentity_ABrokenLinkBecomesAnEmptyRowOfItsElement()
        {
            var link = ElementEdits.UseInPreview(_scope, Elements, "el_7", linked: true);
            link.SetValue(BuiltInElements.ToneStyle, BuiltInElements.ToneInverse);
            link.SetMotion(ElementOptions.MotionSlideIn, ElementOptions.StrengthStrong);
            link.SetValidated(true);
            string id = link.Id;
            int at = BlockSurfaces.PreviewRows(_story).ToList().IndexOf(link.Data);

            ElementEdits.Unlink(_scope, Elements, at);
            var own = new ElementRow(BlockSurfaces.PreviewRows(_story)[at]);
            Assert.IsFalse(own.IsRef, "a row of its own now");
            Assert.AreEqual(BuiltInElements.Callout, own.Kind);
            Assert.AreEqual(id, own.Id, "the same preview row");
            Assert.AreEqual(BuiltInElements.ToneInverse, own.Value(BuiltInElements.ToneStyle), "the link's own look");
            Assert.AreEqual(ElementOptions.MotionSlideIn, own.Motion);
            Assert.AreEqual(ElementOptions.StrengthStrong, own.Strength);
            Assert.IsTrue(own.Validated, "its mark kept");
            Assert.AreEqual(Row("el_7").Field(BuiltInElements.TextField).text.Select(e => e.value), own.Field(BuiltInElements.TextField).text.Select(e => e.value),
                "the callout's words copied");

            // - a link whose Page row is deleted: a warning row; Unlink gives an empty row of the element it showed, Remove takes it away
            var broken = ElementEdits.UseInPreview(_scope, Elements, "el_9", linked: true);
            ElementEdits.Remove(Page, PageIds.IndexOf("el_9"));
            var entry = Preview.Entries.Single(e => ReferenceEquals(e.Row, broken.Data));
            Assert.AreEqual(ElementPreviewRule.Source.Broken, entry.Source, "precondition: the link is broken");
            int brokenAt = BlockSurfaces.PreviewRows(_story).ToList().IndexOf(broken.Data);
            ElementEdits.Unlink(_scope, Elements, brokenAt);
            var empty = new ElementRow(BlockSurfaces.PreviewRows(_story)[brokenAt]);
            Assert.IsFalse(empty.IsRef);
            Assert.AreEqual(BuiltInElements.Divider, empty.Kind, "an own row of the element the link showed");
            int count = BlockSurfaces.PreviewRows(_story).Count;
            ElementEdits.Remove(ElementEdits.PreviewList(_story), brokenAt);
            Assert.AreEqual(count - 1, BlockSurfaces.PreviewRows(_story).Count, "Remove takes the row away");
        }

        [Test]
        public void Overrides_TurnedOn_SeedFromTheWall_TurnedOff_KeepTheirValues_AndTheCardsReaderFollows()
        {
            var wall = new CardComponentSettings { open_row_emphasis = ElementOptions.EmphasisShine, page_chrome_breadcrumb = false, page_chrome_prev_next = true };
            ElementEdits.SetOverride(_story, ElementOptions.ComponentOpenRow, true, wall);
            var openRow = BlockSurfaces.OverrideOf(_story, ElementOptions.ComponentOpenRow);
            Assert.IsTrue(openRow.IsOn);
            Assert.AreEqual(ElementOptions.EmphasisShine, openRow.Emphasis, "seeded from the wall: nothing jumps at the switch");
            openRow.Row.fields.Find(f => f.key == BlockSurfaces.EmphasisKey).value = ElementOptions.EmphasisPlain;
            ElementEdits.SetOverride(_story, ElementOptions.ComponentOpenRow, false, wall);
            Assert.IsFalse(BlockSurfaces.OverrideOf(_story, ElementOptions.ComponentOpenRow).IsOn, "off: the wall's");
            Assert.AreEqual(ElementOptions.EmphasisPlain, BlockSurfaces.OverrideOf(_story, ElementOptions.ComponentOpenRow).Emphasis, "its own value kept");

            ElementEdits.SetOverride(_story, ElementOptions.ComponentPageChrome, true, wall);
            var chrome = PageChromeRule.Of(wall, _story);
            Assert.IsFalse(chrome.Breadcrumb, "seeded: the wall hides the breadcrumb");
            Assert.IsTrue(chrome.PrevNext);
            Assert.AreEqual(2, _story.fields.Single(f => f.key == BlockSurfaces.ComponentOverridesField).items.Count, "one row per component");
            ElementEdits.SetOverride(_story, ElementOptions.ComponentOpenRow, true, wall);
            Assert.AreEqual(ElementOptions.EmphasisShine, BlockSurfaces.OverrideOf(_story, ElementOptions.ComponentOpenRow).Emphasis, "on again: seeded again");
            Assert.AreEqual(2, _story.fields.Single(f => f.key == BlockSurfaces.ComponentOverridesField).items.Count, "the same row, not a second one");
            Assert.AreSame(ElementEdits.OverrideRow(_story, ElementOptions.ComponentPageChrome), BlockSurfaces.OverrideOf(_story, ElementOptions.ComponentPageChrome).Row,
                "the editor edits the very row the card reads");
        }

        [Test]
        public void ReadingTime_ZeroIsAuto_MinutesSetByHandWin()
        {
            int auto = ElementRules.Minutes(_scope, Elements, "en", "en");
            Assert.Greater(auto, 0, "precondition: the story has something to read");
            ElementEdits.SetMinutes(_story, 9);
            Assert.AreEqual(9, ElementRules.Minutes(_scope, Elements, "en", "en"));
            ElementEdits.SetMinutes(_story, 0);
            Assert.AreEqual(auto, ElementRules.Minutes(_scope, Elements, "en", "en"), "0 = Auto again");
        }
    }
}
