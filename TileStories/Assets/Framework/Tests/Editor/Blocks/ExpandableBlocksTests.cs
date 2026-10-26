using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // Display Expandable (_3.2 step 12, section 6.2) on a REAL BlockStackView with The Lamp's real card, the shipped words and the wall's real
    // media folder (ResourcesMediaSource: every picture, clip and caption file a view loads is counted). The point of the step, proved by
    // COUNTING: a card of expandable rows binds no view and holds no file until a row is opened; opening binds the kind's real view through
    // its own scope, closing unbinds it and gives every file back, and the view goes back to the pool. Plus: which kinds offer it, where a row
    // is drawn (never for a pinned or footer look), rows kept open over a rebind of the same point, and a playing video paused on close.
    // Real taps on the row are Phase A / B's (CardGalleryTests.Expandable, PoiCardExpandableSceneTests): Edit Mode has no panel to tap on.
    public class ExpandableBlocksTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Screen/Cards/CardStrings.asset";
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        // The shipped wall with every block of every point set to `display` (an in-memory copy: display is content, never an identity field)
        private static WallConfigData AllAs(string display)
        {
            var wall = Shipped();
            foreach (var poi in wall.pois)
                if (poi.card?.blocks != null)
                    foreach (var block in poi.card.blocks) block.display = display;
            return wall;
        }

        private static List<BlockStackBuilder.Entry> Entries(WallConfigData wall, string poiId) =>
            BlockStackBuilder.Build(wall.pois.Single(p => p.id == poiId), wall.card_settings, BlockRegistry.Shared, wall.pois).Entries;

        private static BlockBindContext Context(WallConfigData wall, string poiId, IMediaSource media, string language = "en", ICardVideo video = null)
        {
            var poi = wall.pois.Single(p => p.id == poiId);
            return CardContextBuilder.Build(new BlockBindContext { Media = media, Video = video }, poi, wall, wall.card_settings, language,
                AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries());
        }

        private static BlockStackView Stack() => new(new VisualElement(), new VisualElement(), BlockRegistry.Shared);

        [OneTimeSetUp]
        public void Preconditions()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet("size_comparison", out _), "precondition: the LivingRoom app's kind is registered in Edit Mode");
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath), "precondition: the framework's words");
        }

        // ---------------- which kinds, where ----------------

        [Test]
        public void EveryRegisteredKindButTheHeader_ListsExpandable_AfterItsDefaultDisplay()
        {
            Assert.Greater(BlockRegistry.Shared.All.Count, 30, "precondition: the whole catalogue, the app's kind included");
            foreach (var kind in BlockRegistry.Shared.All)
            {
                if (kind.Key == BuiltInBlocks.HeaderKind)
                {
                    CollectionAssert.AreEqual(new[] { CardOptions.DisplayInline }, kind.DisplayModes, "the pinned title is never a row");
                    continue;
                }
                CollectionAssert.Contains(kind.DisplayModes, CardOptions.DisplayExpandable, kind.Key);
                Assert.AreEqual(CardOptions.DisplayInline, kind.DisplayModes[0], kind.Key + ": inline stays the default (an unset display)");
            }
        }

        [Test]
        public void ShowsCompact_OnlyForAnExpandableBlockInTheScroll_NeverAPinnedOrFooterLook()
        {
            var entries = Entries(AllAs(CardOptions.DisplayExpandable), Lamp);
            BlockStackBuilder.Entry Of(string key) => entries.Single(e => e.Instance.key == key);
            Assert.IsTrue(BlockDisplayRule.ShowsCompact(Of("block_2")), "a rich text in the scroll");
            Assert.IsTrue(BlockDisplayRule.ShowsCompact(Of("block_55")), "the audio player in the scroll");
            Assert.IsFalse(BlockDisplayRule.ShowsCompact(Of("block_54")), "the audio hero chip is pinned under the title: drawn whole");
            Assert.IsFalse(BlockDisplayRule.ShowsCompact(Of("block_19")), "the sticky call to action is the footer: drawn whole");
            Assert.IsFalse(BlockDisplayRule.ShowsCompact(Of("block_1")), "the header has no Expandable: it resolves to inline");

            var shipped = Entries(Shipped(), Lamp);
            Assert.IsFalse(shipped.Any(BlockDisplayRule.ShowsCompact), "the shipped wall authors no expandable block: the card is unchanged");
            var typo = new BlockStackBuilder.Entry(new BlockInstanceData { key = "x", kind = BuiltInBlocks.RichTextKind, display = "expandible" },
                BuiltInBlocks.RichText, BuiltInBlocks.RichText.DefaultVariant, false);
            Assert.IsFalse(BlockDisplayRule.ShowsCompact(typo), "a display the kind does not offer reads as its first (inline)");
        }

        // ---------------- the acceptance: count what is bound ----------------

        [Test]
        public void TheLamp_AllExpandable_BindsNoViewAndHoldsNoFile_InItsRows_UntilOneIsOpened()
        {
            var inline = Shipped();
            var expandable = AllAs(CardOptions.DisplayExpandable);
            string folder = inline.card_settings.media_resources_path;

            // - the same card drawn whole first: what "the media of a card" is on this wall, so the zero below is not vacuous
            var wholeSource = new ResourcesMediaSource(folder);
            var whole = Stack();
            whole.Bind(Entries(inline, Lamp), Context(inline, Lamp, wholeSource));
            // - _3.3 P4.4a: the hero chip is drawn in the hero (HeroActionViews), with the sticky action's second view: 61 + 2
            Assert.AreEqual(61, whole.BoundViews.Count, "precondition: drawn whole, the 61 stack views are bound");
            Assert.AreEqual(2, whole.HeroActionViews.Count, "precondition: and the hero's two");
            int wholeFiles = wholeSource.LoadedCount;
            Assert.GreaterOrEqual(wholeFiles, 10, "precondition: The Lamp drawn whole holds many files (" + wholeFiles + ")");
            whole.UnbindAll();

            var source = new ResourcesMediaSource(folder);
            var stack = Stack();
            var entries = Entries(expandable, Lamp);
            stack.Bind(entries, Context(expandable, Lamp, source));
            var rows = entries.Where(e => stack.CompactRowOf(e.Instance.key) != null).Select(e => e.Instance.key).ToList();
            Assert.AreEqual(59, rows.Count, "every scrolling block of The Lamp is a row: 62 less the header, the pinned hero chip and the footer CTA");
            CollectionAssert.AreEqual(new[] { BuiltInBlocks.HeaderKind, BuiltInBlocks.ActionsKind },
                stack.BoundViews.Select(v => entries.First(e => stack.ViewOfBlock(e.Instance.key) == v).Definition.Key),
                "the only stack views bound: the header and the sticky footer");
            CollectionAssert.AreEqual(new[] { typeof(ActionsBlockView), typeof(AudioGuideBlockView) }, stack.HeroActionViews.Select(v => v.GetType()),
                "and the hero's: the sticky action and Listen (the hero chip)");
            foreach (string key in rows)
            {
                Assert.IsNull(stack.ViewOfBlock(key), key + ": a closed row binds no view");
                Assert.AreEqual(0, stack.HeldMediaOf(key), key + ": and holds no file");
            }
            Assert.AreEqual(stack.HeldMedia, source.LoadedCount, "every file the card holds is held by the whole blocks and the hero's actions");
            Assert.Less(source.LoadedCount, wholeFiles, "the rows load nothing of what the whole card loads (" + source.LoadedCount + " of " + wholeFiles + ")");
            Assert.AreEqual(61, stack.BoundKinds.Count, "every block of the stack is still on the card, in its order (the hero chip: in the hero)");

            // - open the picture gallery: its real view, its own files; close it: everything back
            int before = source.LoadedCount;
            Assert.AreEqual(4, stack.CreatedViewCount, "only the whole blocks built a view: header, footer, the hero's action and Listen");
            Assert.IsTrue(stack.Expand("block_29"));
            int created = stack.CreatedViewCount;
            Assert.AreEqual(5, created, "opening built the gallery's view, the first time only");
            var gallery = stack.ViewOfBlock("block_29");
            Assert.IsInstanceOf<GalleryBlockView>(gallery, "the kind's own view, bound under its row");
            Assert.IsTrue(stack.IsExpanded("block_29"));
            Assert.IsTrue(stack.CompactRowOf("block_29").IsExpanded, "the row shows it open");
            Assert.AreSame(stack.SlotOfBlock("block_29"), gallery.Root.parent, "in the row's own slot");
            Assert.Greater(stack.HeldMediaOf("block_29"), 0, "its pictures loaded through its own scope");
            Assert.AreEqual(before + stack.HeldMediaOf("block_29"), source.LoadedCount);
            Assert.IsFalse(stack.Expand("block_29"), "already open: nothing more");

            Assert.IsTrue(stack.Collapse("block_29"));
            Assert.IsNull(stack.ViewOfBlock("block_29"));
            Assert.AreEqual(0, stack.HeldMediaOf("block_29"));
            Assert.AreEqual(before, source.LoadedCount, "closing gave every file back");
            Assert.IsNull(gallery.Root.parent, "the view left the card");
            Assert.IsFalse(stack.Collapse("block_29"), "already closed: nothing");

            stack.Expand("block_29");
            Assert.AreSame(gallery, stack.ViewOfBlock("block_29"), "opened again: the pooled view, never a new one");
            Assert.AreEqual(created, stack.CreatedViewCount, "no view built for a second opening");
            stack.UnbindAll();
            Assert.AreEqual(0, source.LoadedCount, "closing the card holds nothing");
        }

        [Test]
        public void OpeningAndClosing_RedrawTheRow_AndRaiseExpandedChanged()
        {
            var wall = AllAs(CardOptions.DisplayExpandable);
            var stack = Stack();
            stack.Bind(Entries(wall, Lamp), Context(wall, Lamp, new ResourcesMediaSource(wall.card_settings.media_resources_path)));
            var changed = new List<string>();
            stack.ExpandedChanged += changed.Add;
            var row = stack.CompactRowOf("block_2");
            Assert.AreEqual("The story", row.Words.text);
            Assert.AreEqual(DisplayStyle.None, row.FullView.style.display.value, "closed: no full-view button");
            StringAssert.StartsWith("Open - ", row.Toggle.tooltip, "its name says what a tap does");

            stack.Expand("block_2");
            Assert.IsTrue(stack.IsExpanded("block_2"));
            Assert.IsInstanceOf<RichTextBlockView>(stack.ViewOfBlock("block_2"));
            Assert.AreEqual(DisplayStyle.Flex, row.FullView.style.display.value, "open: the full-view button joins it");
            StringAssert.StartsWith("Close - ", row.Toggle.tooltip);
            stack.Collapse("block_2");
            Assert.IsFalse(stack.IsExpanded("block_2"));
            Assert.IsNull(stack.ViewOfBlock("block_2"));
            CollectionAssert.AreEqual(new[] { "block_2", "block_2" }, changed);
        }

        [Test]
        public void ARowLeftOpen_StaysOpenOverARebindOfTheSamePoint_AndAnotherPointStartsClosed()
        {
            var wall = AllAs(CardOptions.DisplayExpandable);
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var stack = Stack();
            stack.Bind(Entries(wall, Lamp), Context(wall, Lamp, media));
            stack.Expand("block_2");
            stack.Expand("block_29");

            // - a language switch rebinds the same point: the rows stay open, drawn in the new language
            stack.Bind(Entries(wall, Lamp), Context(wall, Lamp, media, "pt"));
            Assert.IsTrue(stack.IsExpanded("block_2") && stack.IsExpanded("block_29"), "both rows open again after the rebind");
            Assert.AreEqual("A história", stack.CompactRowOf("block_2").Words.text, "in Portuguese");
            Assert.Greater(stack.HeldMediaOf("block_29"), 0, "the gallery's view is bound again with its files");

            stack.Bind(Entries(wall, LampMilitary), Context(wall, LampMilitary, media));
            stack.Bind(Entries(wall, Lamp), Context(wall, Lamp, media));
            Assert.IsFalse(stack.IsExpanded("block_2"), "another point in between: The Lamp opens with every row closed");
            Assert.AreEqual(stack.HeldMedia, media.LoadedCount);
            stack.UnbindAll();
            Assert.AreEqual(0, media.LoadedCount);
        }

        [Test]
        public void ClosingAVideosRow_PausesTheVisitorsVideo_ItWasShowing_AndOnlyThat()
        {
            var wall = AllAs(CardOptions.DisplayExpandable);
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var output = new ManualVideoOutput();
            var video = new CardVideoService(output, () => media);
            try
            {
                var stack = Stack();
                stack.Bind(Entries(wall, Lamp), Context(wall, Lamp, media, video: video));
                stack.Expand("block_56");
                var track = ((VideoBlockView)stack.ViewOfBlock("block_56")).Track;
                Assert.IsNotNull(track, "precondition: the block names a clip");
                video.Toggle(track);
                Assert.AreEqual(CardVideoState.Playing, video.State, "precondition: the visitor started it");

                stack.Collapse("block_56");
                Assert.AreEqual(CardVideoState.Paused, video.State, "closed: nothing on the card shows it, so it pauses");
                Assert.IsTrue(video.IsCurrent(track), "paused, not dropped: opening the row again shows where it was");

                // - another block's row closing never touches it
                video.Resume();
                stack.Expand("block_2");
                stack.Collapse("block_2");
                Assert.AreEqual(CardVideoState.Playing, video.State, "a text row closing leaves the video alone");
                stack.UnbindAll();
            }
            finally
            {
                video.CardClosed();
                output.Release();
            }
        }
    }
}
