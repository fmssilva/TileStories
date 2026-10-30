using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TileStories.Tests;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // FloatingSlotTests's shared ground: its set-up and helpers, so its own tests and its old-navigation ones stand on one fixture
    public abstract class FloatingSlotFixture
    {
        protected const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Screen/Cards/CardStrings.asset";

        protected const string Lamp = "lamp";

        protected const string LampMilitary = "lamp_military";

        protected ManualArWall _wall;

        protected ScopedMediaSource _media;

        protected ArPlacementService _placement;

        // - the Navigation Bar at the framework default (off): these tests measure the elements, never the wall's own bar (40-testing)
        protected static WallConfigData Shipped()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            config.card_settings.navigation.bar = new CardNavBarSettings();
            // - ...and the placed model's chip at its framework default (the wall may turn it off)
            config.card_settings.navigation.placed_model_control = CardOptions.PlacedModelChip;
            return config;
        }

        // The shipped wall with `floating` as its Floating Button, no Top Bar, the blocks Grouped By Family (8 sections on The Lamp)
        protected static WallConfigData With(string floating, string menu = CardOptions.NavNone, int barSlots = 5)
        {
            var wall = Shipped();
            var nav = wall.card_settings.navigation;
            nav.menu = menu;
            nav.top = CardOptions.NavNone;
            nav.block_order = CardOptions.BlockOrderByFamily;
            nav.max_slots = barSlots;
            return wall;
        }

        protected static PoiCardSheetView Sheet() => new(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());

        protected void ShowCard(PoiCardSheetView sheet, WallConfigData wall, string poiId = Lamp, string language = "en")
        {
            var poi = wall.pois.Single(p => p.id == poiId);
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared, wall.pois);
            var context = CardContextBuilder.Build(new BlockBindContext { ArPlacement = _placement }, poi, wall, wall.card_settings, language,
                AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries());
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Full, wall.card_settings.container.half_max_ratio);
        }

        // The Lamp's own place_in_ar block (block_62: the framework arch, every other field its default) asks the owner to place it
        protected bool PlaceTheLampsArch(WallConfigData wall)
        {
            var lamp = wall.pois.Single(p => p.id == Lamp);
            Assert.IsTrue(lamp.card.blocks.Any(b => b.key == "block_62" && b.kind == "place_in_ar"), "precondition: The Lamp's block_62 is its place_in_ar");
            return _placement.Place(new ArPlacementRequest
            {
                Poi = lamp, BlockKey = "block_62", ModelPath = "default:azulejo_arch", OffsetCm = 10f, ScaleMode = ArPlacementRule.ScaleRealSize,
            });
        }

        protected static CardStrings Words(string language) =>
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
    }

    // The card's floating slot (FloatingSlot, _3.2 step 9, sections 4.2 and 5.4) on a REAL sheet with The Lamp's real card and the shipped
    // words, and the placed-model chip over the REAL AR placement owner (ArPlacementService over a ManualArWall, placing the real framework
    // arch through The Lamp's own block_62). Tier 0 of the structure: the layer's place (over the sheet, under the menu layer and the full
    // view), the Floating Button the wall picks, the dial's and the radial's items through the card's one overlay owner (Back and a tap outside
    // close them first), More on the radial, and every state of the chip -- placed or not, this card or another, the control off, released when
    // the card closes. Real layout, real fingers and the visible rect are PoiCardFloatingSceneTests'.
    public class FloatingSlotTests : FloatingSlotFixture
    {
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
            Assert.Greater(fullViewAt, menuAt, "and so under the full view");
            Assert.IsTrue(sheet.Floating.Root.ClassListContains("ts-card"), "it carries the card's tokens (its gap is read there)");
            Assert.IsFalse(sheet.Floating.IsShown, "no card: no slot");
            Assert.AreEqual(PickingMode.Ignore, sheet.Floating.Root.pickingMode, "the row never takes a finger itself: a tap beside it reaches the camera");
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
