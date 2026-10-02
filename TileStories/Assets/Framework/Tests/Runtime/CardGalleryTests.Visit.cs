using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // Phase A promises of the visit family: practical_info (and the card's one icon set), wall_locator, today_map.
    public partial class CardGalleryTests
    {
        // ---------------- practical_info + the card's icon set ----------------

        // The parts of an icon that are drawn (shown and of real size)
        private static int DrawnParts(VisualElement icon) =>
            icon.Q(className: "card-icon__glyph").Children().Count(p => p.resolvedStyle.display == DisplayStyle.Flex && p.worldBound.width > 0f && p.worldBound.height > 0f);

        // The colour an icon is drawn in: its ring's line, else its first shown bar or dot
        private static Color IconInk(VisualElement icon)
        {
            var shown = icon.Q(className: "card-icon__glyph").Children().Where(p => p.resolvedStyle.display == DisplayStyle.Flex).ToList();
            var frame = shown.FirstOrDefault(p => p.ClassListContains("card-icon__frame"));
            return frame != null ? frame.resolvedStyle.borderTopColor : shown[0].resolvedStyle.backgroundColor;
        }

        [UnityTest]
        public IEnumerator PracticalInfo_EachRowWearsItsIconFromTheOneSet_TheWordsLineUp_AndARowWithNoLabelIsLeftOut()
        {
            PracticalInfoBlockView info = null;
            yield return ShowBlock("practical_info_rows_long", v => info = (PracticalInfoBlockView)v);
            CollectionAssert.AreEqual(BuiltInBlocks.PracticalInfoIcons.OrderBy(k => k), info.Rows.Select(r => CardIcons.KeyOf(r.Icon)).OrderBy(k => k), "every icon of the set once");
            var drawings = new System.Collections.Generic.HashSet<string>();
            foreach (var row in info.Rows)
            {
                var glyph = row.Icon.Q(className: "card-icon__glyph");
                Assert.GreaterOrEqual(DrawnParts(row.Icon), 2, CardIcons.KeyOf(row.Icon) + ": drawn from at least two parts");
                foreach (var part in glyph.Children())
                    if (part.resolvedStyle.display == DisplayStyle.Flex)
                        Assert.IsTrue(row.Icon.worldBound.Overlaps(part.worldBound), CardIcons.KeyOf(row.Icon) + ": its parts inside the icon");
                // - a fingerprint of which parts show where: two keys must never draw the same picture
                drawings.Add(string.Join("|", glyph.Children().Where(p => p.resolvedStyle.display == DisplayStyle.Flex)
                    .Select(p => p.worldBound.x - glyph.worldBound.x + "," + (p.worldBound.y - glyph.worldBound.y) + "," + p.worldBound.width + "," + p.worldBound.height + "," + p.resolvedStyle.rotate.angle.value)));
                Assert.GreaterOrEqual(UIAccessibility.ContrastRatio(IconInk(row.Icon), CardTestInput.EffectiveBackground(row.Icon)),
                    UIAccessibility.MinRatioLargeTextOrUIComponent, CardIcons.KeyOf(row.Icon) + ": the icon reads on its badge (3:1, a UI component)");
            }
            Assert.AreEqual(info.Rows.Count, drawings.Count, "six different pictures");
            for (int i = 1; i < info.Rows.Count; i++)
            {
                Assert.AreEqual(info.Rows[0].Label.worldBound.xMin, info.Rows[i].Label.worldBound.xMin, 0.5f, "every row's words start on one line");
                Assert.Greater(info.Rows[i].Box.resolvedStyle.borderTopWidth, 0f, "a hairline between rows");
            }
            Assert.GreaterOrEqual(info.Rows[0].Value.worldBound.yMin, info.Rows[0].Label.worldBound.yMax - 0.5f, "the value under its label");

            yield return ShowBlock("practical_info_rows_partial", v => info = (PracticalInfoBlockView)v);
            CollectionAssert.AreEqual(new[] { "Open", "Tickets", "Where" }, info.Rows.Select(r => r.Label.text), "a row with no label is left out");
            Assert.AreEqual(Visibility.Hidden, info.Rows[0].Icon.resolvedStyle.visibility, "no icon picked: no empty badge");
            Assert.AreEqual(Visibility.Hidden, info.Rows[1].Icon.resolvedStyle.visibility, "an action's icon is not a row icon: none drawn");
            Assert.AreEqual(info.Rows[0].Label.worldBound.xMin, info.Rows[2].Label.worldBound.xMin, 0.5f, "...and the words still line up");
            Assert.IsFalse(CardTestInput.IsShown(info.Rows[2].Value, info.Root), "no value written: none shown");
        }

        [UnityTest]
        public IEnumerator TheActions_DrawTheirIconFromTheSameSet_AsATarget()
        {
            ActionsBlockView actions = null;
            yield return ShowBlock("actions_pill_row_short", v => actions = (ActionsBlockView)v);
            var icon = actions.Actions[0].Icon;
            Assert.IsTrue(icon.ClassListContains("card-icon"), "the card's one icon set");
            Assert.AreEqual(CardIcons.ShowOnWall, CardIcons.KeyOf(icon));
            var frame = icon.Q(className: "card-icon__frame");
            var dot = icon.Q(className: "card-icon__dot");
            Assert.AreEqual(2, DrawnParts(icon), "a ring and a dot");
            Assert.AreEqual(frame.worldBound.center.x, dot.worldBound.center.x, 0.5f, "the dot in the ring's centre");
            Assert.AreEqual(frame.worldBound.center.y, dot.worldBound.center.y, 0.5f);

            yield return ShowBlock("actions_circles_short", v => actions = (ActionsBlockView)v);
            icon = actions.Actions[0].Icon;
            var glyph = icon.Q(className: "card-icon__glyph");
            Assert.GreaterOrEqual(icon.worldBound.width, 44f, "circles: the icon's round badge is finger-sized");
            Assert.Less(glyph.worldBound.width, icon.worldBound.width, "...and the drawing keeps its own size inside it");
            Assert.AreEqual(icon.worldBound.center.x, glyph.worldBound.center.x, 0.5f, "centred");

            yield return ShowBlock("actions_sticky_cta_short", v => actions = (ActionsBlockView)v);
            var sticky = actions.Actions[0];
            Assert.AreEqual(sticky.Label.resolvedStyle.color, sticky.Icon.Q(className: "card-icon__frame").resolvedStyle.borderTopColor, "on the accent button the icon wears the button's text colour");
        }

        // ---------------- wall_locator (Tier 2 group B) ----------------

        // Where a share of the strip is on screen: the track's left edge + share x its width
        private static float TrackX(WallLocatorBlockView view, float share) => view.Track.worldBound.x + share * view.Track.worldBound.width;

        [UnityTest]
        public IEnumerator WallLocator_Strip_EveryPointIsADotAtItsPlaceAlongTheWall_ThisOneMarked_AndTheVisitorIsHere()
        {
            WallLocatorBlockView view = null;
            yield return ShowBlock("wall_locator_strip_short", v => view = (WallLocatorBlockView)v);
            Assert.AreEqual("On this wall", _harness.Sheet.Stack.HeadingOf(view).text, "no heading written: the kind's own (Card Texts)");
            Assert.AreEqual(2, view.Dots.Count, "a dot per OTHER point of the wall");
            // - West Gate x = -2, this point 0, East Tower 3: the ends of the strip are the wall's end points
            Assert.AreEqual(0.4f, view.SelfShare, 1e-3f, "this point: 2 m of the wall's 5 from its left end");
            var dotsX = view.Dots.Select(d => d.worldBound.center.x).OrderBy(x => x).ToList();
            Assert.AreEqual(TrackX(view, 0f), dotsX[0], 1f, "West Gate at the left end");
            Assert.AreEqual(TrackX(view, 1f), dotsX[1], 1f, "East Tower at the right end");
            Assert.AreEqual(TrackX(view, 0.4f), view.Self.worldBound.center.x, 1f, "this point's dot at its place");
            Assert.Greater(view.Self.worldBound.width, view.Dots[0].worldBound.width, "...and larger than the others");
            Assert.AreEqual(0.6f, view.YouShare, 1e-3f, "the visitor stands 1 m right of this point: 3 m of 5");
            Assert.AreEqual(TrackX(view, 0.6f), view.You.worldBound.center.x, 1f, "the visitor's ring at their place");
            Assert.AreEqual("Gate", view.SelfTitle.text, "the legend names this point by its card title");
            Assert.AreEqual("You are here", view.YouLabel.text);
            Assert.IsTrue(CardTestInput.IsShown(view.YouLegend, view.Root));
            yield return Render("Card_wall_locator_strip_you");

            // - the visitor walks to the West Gate: the ring follows while the block is shown
            _harness.Sheet.Viewer = () => new Vector3(-2f, 1.6f, 1f);
            yield return new WaitForSecondsRealtime(WallLocatorBlockView.ViewerRefreshMs / 1000f * 2.5f);
            Assert.AreEqual(0f, view.YouShare, 1e-3f, "the visitor's ring followed them to the wall's left end");
            Assert.AreEqual(TrackX(view, 0f), view.You.worldBound.center.x, 1f);
            _harness.Sheet.Viewer = () => null;
            yield return new WaitForSecondsRealtime(WallLocatorBlockView.ViewerRefreshMs / 1000f * 2.5f);
            Assert.IsFalse(CardTestInput.IsShown(view.You, view.Root), "the visitor lost (no camera): no ring...");
            Assert.IsFalse(CardTestInput.IsShown(view.YouLegend, view.Root), "...and no 'You are here'");

            yield return ShowBlock("wall_locator_strip_noviewer", v => view = (WallLocatorBlockView)v);
            Assert.AreEqual(-1f, view.YouShare, "no viewer known: no ring at all");
            Assert.IsFalse(CardTestInput.IsShown(view.YouLegend, view.Root));
        }

        [UnityTest]
        public IEnumerator WallLocator_Strip_OnAWallAtAnAngle_FindsItsOwnAxis_HeightNeverCounts_AndAFarVisitorSitsAtTheEnd()
        {
            WallLocatorBlockView view = null;
            yield return ShowBlock("wall_locator_strip_long", v => view = (WallLocatorBlockView)v);
            Assert.AreEqual(12, view.Dots.Count, "twelve other points");
            Assert.AreEqual(0.5f, view.SelfShare, 1e-3f, "the middle of twelve points spaced evenly along the ANGLED wall (at three heights)");
            var shares = view.Dots.Select(d => (d.worldBound.center.x - view.Track.worldBound.x) / view.Track.worldBound.width).OrderBy(x => x).ToList();
            for (int i = 1; i < shares.Count; i++)
                Assert.Greater(shares[i] - shares[i - 1], 0.05f, "evenly spread: no two points folded together (the wall's own axis, not world x)");
            Assert.AreEqual(1f, view.YouShare, 1e-4f, "a visitor far past the right end: the ring waits at that end");
        }

        [UnityTest]
        public IEnumerator WallLocator_Neighbours_NameTheNearestPointOnEachSide_ARealTapSelectsItThroughTheBus()
        {
            WallLocatorBlockView view = null;
            SelectionEventBus.ResetState();
            yield return ShowBlock("wall_locator_neighbours_short", v => view = (WallLocatorBlockView)v);
            Assert.AreEqual("West Gate", view.LeftTitle.text, "the nearest point to the left along the wall");
            Assert.AreEqual("East Tower", view.RightTitle.text, "...and to the right");
            Assert.Less(view.Left.worldBound.center.x, view.Right.worldBound.center.x, "left on the left");
            yield return CardTestInput.Tap(view.Root.panel, view.Right.worldBound.center);
            yield return null;
            Assert.AreEqual("east_tower", SelectionEventBus.CurrentPoiId, "a real tap selected East Tower through the selection bus");
            SelectionEventBus.ResetState();

            yield return ShowBlock("wall_locator_neighbours_end", v => view = (WallLocatorBlockView)v);
            Assert.IsNull(view.LeftPoiId, "the wall's left end: nothing to its left");
            Assert.AreEqual(Visibility.Hidden, view.Left.resolvedStyle.visibility, "...so no left button");
            Assert.AreEqual("West Gate", view.RightTitle.text, "the nearest to its right");
            Assert.Greater(view.Right.worldBound.center.x, view.Root.worldBound.center.x, "the right button keeps its side");
            yield return CardTestInput.Tap(view.Root.panel, view.Left.worldBound.center);
            yield return null;
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "a tap where the hidden button would be selects nothing");

            yield return ShowBlock("wall_locator_neighbours_long", v => view = (WallLocatorBlockView)v);
            StringAssert.StartsWith("The Royal Palace", view.LeftTitle.text, "a neighbour is named by its own card title");
            StringAssert.StartsWith("The chapel of Saint George", view.RightTitle.text);
            Assert.AreEqual(TextAnchor.MiddleLeft, view.LeftTitle.resolvedStyle.unityTextAlign, "the left title reads from its chevron");
            Assert.AreEqual(TextAnchor.MiddleRight, view.RightTitle.resolvedStyle.unityTextAlign, "...the right one towards its chevron");
            yield return Render("Card_wall_locator_neighbours_long");
            SelectionEventBus.ResetState();
        }

        // ---------------- today_map (Tier 2 group B) ----------------

        // What the card would hand to the device (the IUrlOpener seam: nothing leaves Unity)
        private sealed class RecordingOpener : IUrlOpener
        {
            public readonly System.Collections.Generic.List<string> Opened = new();
            public void Open(string url) => Opened.Add(url);
        }

        [UnityTest]
        public IEnumerator TodayMap_Static_ShowsTheMapAndCoordinates_ARealTapOnDirectionsOpensTheLinkOnce()
        {
            TodayMapBlockView view = null;
            var opener = new RecordingOpener();
            yield return ShowBlock("today_map_static_short", v => view = (TodayMapBlockView)v);
            _harness.Sheet.UrlOpener = opener;
            Assert.AreEqual("Where it is today", _harness.Sheet.Stack.HeadingOf(view).text, "no heading written: the default");
            yield return PixelAt(view.Map.Root, view.Map.Root.worldBound.center + new Vector2(10f, 10f), c => AssertColour(PictureColour("wide.png"), c, "the map picture"));
            Assert.AreEqual("38.71390, -9.13340", view.Coordinates.text, "both coordinates, joined by the card's format");
            Assert.AreEqual("Directions", view.Directions.text);
            Assert.IsTrue(CardTestInput.IsShown(view.Directions, view.Root));

            yield return CardTestInput.Tap(view.Root.panel, view.Directions.worldBound.center);
            yield return null;
            CollectionAssert.AreEqual(new[] { CardGalleryDefinitions.TodayMapUrl }, opener.Opened, "a real tap opened the Maps Link, once");
            yield return Render("Card_today_map_static_directions");

            yield return ShowBlock("today_map_static_nourl", v => view = (TodayMapBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(view.Directions, view.Root), "no link: no button");
            Assert.IsTrue(CardTestInput.IsShown(view.Coordinates, view.Root), "...the coordinates still show");
            yield return ShowBlock("today_map_static_badurl", v => view = (TodayMapBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(view.Directions, view.Root), "a link that is not a web address: no button");
            _harness.Sheet.OpenUrl("javascript:alert(1)");
            Assert.AreEqual(1, opener.Opened.Count, "...and the card never hands it to the device, even if asked");
            yield return ShowBlock("today_map_static_nocoords", v => view = (TodayMapBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(view.Coordinates, view.Root), "only a latitude written: no coordinates line (never '38.7, 0')");
            yield return ShowBlock("today_map_static_missing", v => view = (TodayMapBlockView)v);
            Assert.IsTrue(view.Map.Root.ClassListContains("card-image--unavailable"), "a missing map says so");
        }

        [UnityTest]
        public IEnumerator TodayMap_Bridge_ThePointAsTheWallShowsItBesideTodaysMap()
        {
            TodayMapBlockView view = null;
            yield return ShowBlock("today_map_bridge_short", v => view = (TodayMapBlockView)v);
            Assert.IsTrue(CardTestInput.IsShown(view.Bridge, view.Root));
            Assert.IsFalse(CardTestInput.IsShown(view.Map.Root, view.Root), "the bridge replaces the large map");
            Assert.AreEqual("On the wall", view.ThenLabel.text);
            Assert.AreEqual("Today", view.NowLabel.text);
            Assert.AreEqual("Gate", view.ThenTitle.text, "the point's card title");
            Assert.Less(view.ThenPicture.Root.worldBound.center.x, view.NowMap.Root.worldBound.center.x, "the wall on the left, today on the right");
            Assert.AreEqual(view.ThenLabel.worldBound.yMin, view.NowLabel.worldBound.yMin, 0.5f, "the two sides line up: their words...");
            Assert.AreEqual(view.ThenPicture.Root.worldBound.yMin, view.NowMap.Root.worldBound.yMin, 0.5f, "...and their pictures");
            Assert.AreEqual(view.ThenTitle.worldBound.xMin, view.Coordinates.worldBound.xMin, 0.5f, "the coordinates start where the title starts");
            yield return PixelAt(view.ThenPicture.Root, view.ThenPicture.Root.worldBound.center + new Vector2(6f, 6f), c => AssertColour(PictureColour("then.png"), c, "the point's own header picture"));
            yield return PixelAt(view.NowMap.Root, view.NowMap.Root.worldBound.center + new Vector2(6f, 6f), c => AssertColour(PictureColour("now.png"), c, "today's map"));
            Assert.AreEqual("38.71390, -9.13340", view.Coordinates.text);
            yield return Render("Card_today_map_bridge_pictures");

            yield return ShowBlock("today_map_bridge_noheaderpicture", v => view = (TodayMapBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(view.ThenPicture.Root, view.Root),
                "a picture stored on a text-only header: the card's header shows none, so neither does the bridge (the title alone)");
            Assert.AreEqual(0, _harness.Media.RefCount("then.png"), "...and it is not even loaded");
            Assert.AreEqual("Gate", view.ThenTitle.text);
        }
    }
}
