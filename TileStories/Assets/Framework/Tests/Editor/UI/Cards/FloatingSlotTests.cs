using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The card's floating slot (FloatingSlot, _3.2 step 9, sections 4.2 and 5.4) on a REAL sheet with The Lamp's real card and the shipped
    // words, and the placed-model chip over the REAL AR placement owner (ArPlacementService over a ManualArWall, placing the real framework
    // arch through The Lamp's own block_62). Tier 0 of the structure: the layer's place (over the sheet, under the menu layer and the full
    // view), the Floating Button the wall picks, the dial's and the radial's items through the card's one overlay owner (Back and a tap outside
    // close them first), More on the radial, and every state of the chip -- placed or not, this card or another, the control off, released when
    // the card closes. Real layout, real fingers and the visible rect are PoiCardFloatingSceneTests'.
    public class FloatingSlotTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";

        private ManualArWall _wall;
        private ScopedMediaSource _media;
        private ArPlacementService _placement;

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        // The shipped wall with `floating` as its Floating Button, no Top Bar, the blocks Grouped By Family (8 sections on The Lamp)
        private static WallConfigData With(string floating, string menu = CardOptions.NavNone, int barSlots = 5)
        {
            var wall = Shipped();
            var nav = wall.card_settings.navigation;
            nav.floating = floating;
            nav.menu = menu;
            nav.top = CardOptions.NavNone;
            nav.block_order = CardOptions.BlockOrderByFamily;
            nav.max_slots = barSlots;
            return wall;
        }

        private static PoiCardSheetView Sheet() => new(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());

        private void ShowCard(PoiCardSheetView sheet, WallConfigData wall, string poiId = Lamp, string language = "en")
        {
            var poi = wall.pois.Single(p => p.id == poiId);
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared, wall.pois);
            var context = CardContextBuilder.Build(new BlockBindContext { ArPlacement = _placement }, poi, wall, wall.card_settings, language,
                AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries());
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Full, wall.card_settings.container.half_max_ratio);
        }

        // The Lamp's own place_in_ar block (block_62: the framework arch, every other field its default) asks the owner to place it
        private bool PlaceTheLampsArch(WallConfigData wall)
        {
            var lamp = wall.pois.Single(p => p.id == Lamp);
            Assert.IsTrue(lamp.card.blocks.Any(b => b.key == "block_62" && b.kind == "place_in_ar"), "precondition: The Lamp's block_62 is its place_in_ar");
            return _placement.Place(new ArPlacementRequest
            {
                Poi = lamp, BlockKey = "block_62", ModelPath = "default:azulejo_arch", OffsetCm = 10f, ScaleMode = ArPlacementRule.ScaleRealSize,
            });
        }

        private static CardStrings Words(string language) =>
            new(AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries(), null, language, "en");

        [OneTimeSetUp]
        public void TheAppsKindIsRegistered()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet("size_comparison", out _), "precondition: the LivingRoom app's kind is registered in Edit Mode");
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath), "precondition: the framework's words");
        }

        [SetUp]
        public void TheRealOwner()
        {
            _wall = new ManualArWall { Viewer = new Vector3(0f, 0f, -2f), MarkerDiameter = 0.1f };
            _media = new ScopedMediaSource(new ResourcesMediaSource("") { FrameworkDefaults = CardMediaLibraryLookup.Framework });
            _placement = new ArPlacementService(_wall, () => _media);
        }

        [TearDown]
        public void LetItGo()
        {
            _placement.Dispose();
            _wall.Dispose();
        }

        // Section 4.2: the floating layer is the card layer's child over the sheet, BELOW the menu layer (an open drawer covers it) and the full
        // view -- paint order is child order
        [Test]
        public void TheFloatingLayer_SitsOverTheSheet_UnderTheMenuLayerAndTheFullView_AndIsHiddenWithNoCard()
        {
            var sheet = Sheet();
            int sheetAt = sheet.Layer.IndexOf(sheet.Root);
            int floatingAt = sheet.Layer.IndexOf(sheet.Floating.Root);
            int menuAt = sheet.Layer.IndexOf(sheet.Overlay.Root);
            int fullViewAt = sheet.Layer.IndexOf(sheet.Takeover.Root);
            Assert.GreaterOrEqual(sheetAt, 0, "precondition: the sheet is in the card layer");
            Assert.Greater(floatingAt, sheetAt, "over the sheet");
            Assert.AreEqual(menuAt - 1, floatingAt, "right under the menu layer");
            Assert.AreEqual(floatingAt - 1, sheet.Layer.IndexOf(sheet.Floating.Scrim), "its scrim right under it: it dims the card, never the slot");
            Assert.IsFalse(sheet.Floating.IsDimmed, "and starts off");
            Assert.Greater(fullViewAt, menuAt, "and so under the full view");
            Assert.AreSame(sheet.Floating.Elements, sheet.Slots.Floating, "the slots bind the Floating Button into the floating layer");
            Assert.IsTrue(sheet.Floating.Root.ClassListContains("ts-card"), "it carries the card's tokens (its gap is read there)");
            Assert.IsFalse(sheet.Floating.IsShown, "no card: no slot");
            Assert.AreEqual(PickingMode.Ignore, sheet.Floating.Root.pickingMode, "the row never takes a finger itself: a tap beside it reaches the camera");
        }

        // Section 5.4: the wall's Floating Button, in its own slot; None leaves the slot empty (and hidden while no model stands); a card below
        // Show Navigation From gets none, like every element
        [Test]
        public void TheFloatingButton_IsTheWallsPick_InTheFloatingSlot_AndNoneOrAShortCardLeavesItEmpty()
        {
            CollectionAssert.AreEqual(new[] { BuiltInNavElements.FabDial, BuiltInNavElements.FabRadial },
                NavElementRegistry.Shared.OfSlot(NavSlotKeys.Floating).Select(e => e.Key), "the floating slot's elements, from the registry");
            Assert.IsTrue(NavElementRegistry.Shared.OfSlot(NavSlotKeys.Floating).Single(e => e.Key == BuiltInNavElements.FabRadial).ReadsBarSlots,
                "the radial reads Bar Slots (its arc caps like a bar)");

            var sheet = Sheet();
            ShowCard(sheet, With(CardOptions.NavNone));
            Assert.IsNull(sheet.Slots.FloatingView, "None: no element");
            Assert.IsFalse(sheet.Floating.IsShown, "and nothing standing: the slot stays hidden");

            ShowCard(sheet, With(BuiltInNavElements.FabDial));
            Assert.IsInstanceOf<FabDialNavView>(sheet.Slots.FloatingView);
            Assert.AreEqual(BuiltInNavElements.FabDial, sheet.Slots.FloatingKey);
            Assert.AreSame(sheet.Floating.Elements, sheet.Slots.FloatingView.Root.parent, "drawn in the floating layer");
            Assert.IsTrue(sheet.Floating.IsShown, "the slot shows over the open card");

            ShowCard(sheet, With(BuiltInNavElements.FabRadial));
            Assert.IsInstanceOf<FabRadialNavView>(sheet.Slots.FloatingView, "a live switch rebinds the slot");

            ShowCard(sheet, With(BuiltInNavElements.FabDial), LampMilitary);
            Assert.IsFalse(sheet.Nav.Model.ShowsNavigation, "precondition: Lamp - Military is under Show Navigation From");
            Assert.IsNull(sheet.Slots.FloatingView, "a short card: no Floating Button");
            Assert.IsFalse(sheet.Floating.IsShown);

            sheet.Hide();
            Assert.IsNull(sheet.Slots.FloatingView);
            Assert.IsFalse(sheet.Floating.IsShown, "a closed card: no slot");
        }

        // fab_dial: a tap opens the column as the overlay owner's ONE anchored list, a labelled pill per section; Back closes it first and the card
        // stays; a tap outside closes it first and only the next one closes the card; the button again closes it
        [Test]
        public void TheDial_OpensAsTheOverlaysOneList_APillPerSection_BackAndATapOutsideCloseItFirst()
        {
            var sheet = Sheet();
            var wall = With(BuiltInNavElements.FabDial);
            ShowCard(sheet, wall);
            var dial = (FabDialNavView)sheet.Slots.FloatingView;
            var model = sheet.Nav.Model;
            Assert.AreEqual(8, model.Sections.Count, "precondition: The Lamp's 8 sections");
            Assert.IsFalse(dial.IsOpen);
            Assert.AreEqual(DisplayStyle.None, dial.List.style.display.value, "zero chrome until needed");

            dial.Toggle();
            Assert.IsTrue(dial.IsOpen);
            Assert.AreEqual(FabDialNavView.ListKey, sheet.Overlay.OpenKey, "the one thing open over the card");
            Assert.IsFalse(sheet.Overlay.IsModal, "anchored: no scrim");
            Assert.IsTrue(dial.Fab.IsOpen, "the button shows the X");
            Assert.IsTrue(sheet.Floating.IsDimmed, "the card and the camera view behind the column are dimmed");
            var words = Words("en");
            CollectionAssert.AreEqual(model.Sections.Select(s => words.Get(s.LabelKey)).ToArray(), dial.Items.Select(i => i.Label.text).ToArray(),
                "a pill per section, in order, each its words");
            Assert.IsTrue(dial.Items.All(i => i.Icon.style.display.value == DisplayStyle.Flex), "each with its family's icon");
            Assert.IsTrue(dial.Items[0].Button.ClassListContains(NavTab.CurrentClass), "the section being read is marked");
            Assert.AreEqual(1, dial.Items.Count(i => i.Button.ClassListContains(NavTab.CurrentClass)), "only it");

            Assert.IsTrue(sheet.Back(), "Back is the card's");
            Assert.IsFalse(dial.IsOpen, "Back closes the column first");
            Assert.IsFalse(sheet.Floating.IsDimmed, "and the dim goes with it");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "and the card stays where it was");

            dial.Toggle();
            int closes = 0;
            sheet.CloseRequested += () => closes++;
            sheet.TapOutside();
            Assert.IsFalse(dial.IsOpen, "a tap on the camera view closes the column first");
            Assert.AreEqual(0, closes, "not the card");
            sheet.TapOutside();
            Assert.AreEqual(1, closes, "with nothing open, the next tap outside closes the card");

            dial.Toggle();
            dial.Toggle();
            Assert.IsFalse(dial.IsOpen, "the button again closes it");
            Assert.IsFalse(sheet.Overlay.IsOpen, "and the owner knows");
        }

        // fab_radial: the arc holds Bar Slots items, the last More when the sections overflow; More turns the arc to the rest -- or, with a Menu,
        // opens the Menu (which closes the arc: one thing at a time)
        [Test]
        public void TheRadial_HoldsBarSlotsItemsWithMoreLast_MoreTurnsTheArcToTheRest_OrOpensTheMenu()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(BuiltInNavElements.FabRadial));
            var radial = (FabRadialNavView)sheet.Slots.FloatingView;
            var model = sheet.Nav.Model;
            Assert.AreEqual(4, model.BarSections.Count, "precondition: 5 Bar Slots over 8 sections: 4 and More");
            Assert.AreEqual(4, model.MoreSections.Count);

            radial.Tap();
            Assert.IsTrue(radial.IsOpen, "a tap opens the arc");
            Assert.AreEqual(FabRadialNavView.ArcKey, sheet.Overlay.OpenKey, "as the one thing open over the card");
            Assert.AreEqual(5, radial.Items.Count, "Bar Slots items");
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, -1 }, radial.Items.Select(i => i.Section).ToArray(), "the first four sections, More last");
            var words = Words("en");
            Assert.AreEqual(words.Get(CardStrings.Keys.NavMore), radial.Items[4].Label.text, "More says so");
            Assert.AreEqual(words.Get(model.Sections[0].LabelKey), radial.Items[0].Label.text, "every item its words");

            radial.Pick(4);
            Assert.IsTrue(radial.ShowsMore, "More turns the arc to the sections that did not fit");
            Assert.IsTrue(radial.IsOpen, "the arc stays open");
            CollectionAssert.AreEqual(new[] { 4, 5, 6, 7 }, radial.Items.Select(i => i.Section).ToArray(), "exactly the rest, in order");
            radial.Tap();
            Assert.IsFalse(radial.IsOpen, "the button (its X) closes the arc");
            Assert.IsFalse(radial.ShowsMore, "and the next opening starts on the first level");

            ShowCard(sheet, With(BuiltInNavElements.FabRadial, menu: BuiltInNavElements.DrawerTree));
            radial = (FabRadialNavView)sheet.Slots.FloatingView;
            radial.Tap();
            radial.Pick(4);
            Assert.IsTrue(sheet.Overlay.IsModal, "with a Menu, More opens it");
            Assert.AreEqual(BuiltInNavElements.DrawerTree, sheet.Overlay.OpenKey);
            Assert.IsFalse(radial.IsOpen, "and the arc closed: never two at once");
            Assert.IsFalse(sheet.Floating.IsDimmed, "the menu dims with its own scrim, over the slot (which it covers)");

            ShowCard(sheet, With(BuiltInNavElements.FabRadial, barSlots: 3));
            radial = (FabRadialNavView)sheet.Slots.FloatingView;
            radial.Tap();
            Assert.AreEqual(3, radial.Items.Count, "Bar Slots = 3: two sections and More");
        }

        // The chip draws the ONE owner: nothing standing hides it; the card that placed the model says so and offers Remove (which takes it
        // away); another point's card names the point and offers no Remove (a tap on it would open the placing card); Placed Model Control =
        // None hides it; in Portuguese with its accents; and a closed card lets the owner go
        [Test]
        public void TheChip_DrawsTheOneOwner_ThisCardOffersRemove_AnotherNamesThePoint_NoneHidesIt_AndAClosedCardLetsGo()
        {
            var sheet = Sheet();
            var wall = With(CardOptions.NavNone);
            var chip = sheet.Floating.Chip;
            ShowCard(sheet, wall);
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, chip.State, "nothing stands: no chip");
            Assert.IsFalse(sheet.Floating.IsShown, "and no Floating Button: no slot");

            Assert.IsTrue(PlaceTheLampsArch(wall), "precondition: the real arch is placed");
            Assert.AreEqual(PlacedModelChipRule.State.ThisCard, chip.State, "the chip redraws from the owner's Changed");
            Assert.AreEqual("Placed by the wall", chip.Text.text);
            Assert.AreEqual(DisplayStyle.Flex, chip.RemoveButton.style.display.value, "Remove on the card that placed it");
            Assert.AreEqual("Remove", chip.RemoveButton.Q<Label>().text);
            Assert.AreEqual(PickingMode.Ignore, chip.Words.pickingMode, "its words are no link here");
            Assert.IsTrue(sheet.Floating.IsShown, "the chip alone shows the slot");

            chip.Remove();
            Assert.IsNull(_placement.Placed, "Remove takes the model away through the owner");
            Assert.AreEqual(0, _wall.Root.childCount, "nothing left in the world");
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, chip.State, "and the chip goes");
            Assert.IsFalse(sheet.Floating.IsShown);

            Assert.IsTrue(PlaceTheLampsArch(wall));
            ShowCard(sheet, wall, LampMilitary);
            Assert.AreEqual(PlacedModelChipRule.State.OtherCard, chip.State, "another point's card (the model was kept)");
            Assert.AreEqual(wall.pois.Single(poi => poi.id == Lamp).name + " is placed by the wall", chip.Text.text, "names the point by its name");
            Assert.AreEqual(DisplayStyle.None, chip.RemoveButton.style.display.value, "and offers NO Remove: only the first card does");
            Assert.AreEqual(PickingMode.Position, chip.Words.pickingMode, "its words lead to the card that placed it");
            chip.Remove();
            Assert.IsNotNull(_placement.Placed, "Remove does nothing from another card");

            ShowCard(sheet, wall, LampMilitary, "pt");
            StringAssert.Contains("à parede", chip.Text.text, "Portuguese keeps its accents");
            StringAssert.StartsWith(wall.pois.Single(poi => poi.id == Lamp).name, chip.Text.text, "the point's name");

            wall.card_settings.navigation.placed_model_control = CardOptions.PlacedModelNone;
            ShowCard(sheet, wall);
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, chip.State, "Placed Model Control = None: no chip");
            wall.card_settings.navigation.placed_model_control = CardOptions.PlacedModelChip;
            ShowCard(sheet, wall);
            Assert.AreEqual(PlacedModelChipRule.State.ThisCard, chip.State, "Chip again: it shows");

            sheet.Hide();
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, chip.State, "a closed card: no chip");
            _placement.Remove();
            Assert.IsTrue(PlaceTheLampsArch(wall));
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, chip.State, "released: the closed card's chip no longer listens to the owner");
            Assert.IsFalse(sheet.Floating.IsShown);
        }
    }
}
