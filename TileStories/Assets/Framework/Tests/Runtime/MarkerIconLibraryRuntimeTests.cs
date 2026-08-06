// MarkerIconLibraryRuntimeTests.cs
//
// PlayMode test for the runtime half of the Sprite Asset Pipeline that section
// 4 of _5.1_Editor_Tab.md documents: the wall's SpriteKeyLibrary is loaded by the
// config-supplied Resources path (marker_icon_library_resources_path) and a
// sprite is resolved from a key. This is the end of the "keys in config.json,
// sprites resolved at runtime" flow (sections 14.3-14.4) and is not covered by
// the existing MarkerGalleryTests, which build libraries by hand.

using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    public class MarkerIconLibraryRuntimeTests
    {
        // Must match marker_icon_library_resources_path in
        // Assets/Apps/LivingRoom/config.json (== living_room_IconLibrary).
        private const string ResourcesKeyPath = "MarkerSymbols/living_room_IconLibrary";

        [Test]
        public void WallIconLibrary_IsLoadableViaConfigResourcesPath()
        {
            // Section 14.2's whole point: the wall library lives under a Resources
            // folder so Runtime.Load picks it up by the config string.
            SpriteKeyLibrary library = Resources.Load<SpriteKeyLibrary>(ResourcesKeyPath);

            Assert.IsNotNull(library,
                "Wall icon library must be loadable by the Resources path " +
                "the config supplies (marker_icon_library_resources_path).");
            Assert.IsNotNull(library.Entries, "Library entries list must not be null.");
            Assert.Greater(library.Entries.Count, 0,
                "The living_room icon library should register at least one symbol.");

            var first = library.Entries[0];
            Assert.IsFalse(string.IsNullOrWhiteSpace(first.key), "Entry key must not be empty.");
            Assert.IsNotNull(first.sprite, "Entry sprite must not be null.");
        }

        [Test]
        public void ResolveKnownKey_ReturnsRegisteredSprite()
        {
            SpriteKeyLibrary library = Resources.Load<SpriteKeyLibrary>(ResourcesKeyPath);
            Assert.IsNotNull(library, "Setup precondition: wall library must load.");

            var first = library.Entries[0];

            // Round-trip: the key the config uses must resolve back to the same sprite.
            Sprite resolved = library.Get(first.key);
            Assert.AreEqual(first.sprite, resolved,
                $"Key '{first.key}' must resolve to its registered sprite.");
        }

        [Test]
        public void ResolveUnknownKey_ReturnsNull()
        {
            SpriteKeyLibrary library = Resources.Load<SpriteKeyLibrary>(ResourcesKeyPath);
            Assert.IsNotNull(library, "Setup precondition: wall library must load.");

            // A key that no wall symbol owns must not silently return a default sprite.
            Sprite resolved = library.Get("__definitely_not_a_real_key__");
            Assert.IsNull(resolved, "Unknown keys must resolve to null.");
        }

        // The authoring tool seeds six heritage default rows when a wall has no
        // category_styles (DefaultCategoryStyles, _5.3_Default_Icons.md). Each row's
        // icon_key must resolve to a real sprite in the wall's runtime icon library,
        // otherwise the spawned markers fall back to the colour-only circle and the
        // "default rows appear in the markers table" feature is only half-wired.
        [Test]
        public void DefaultHeritageIconKeys_ResolveToSprites()
        {
            SpriteKeyLibrary library = Resources.Load<SpriteKeyLibrary>(ResourcesKeyPath);
            Assert.IsNotNull(library, "Setup precondition: wall library must load.");

            string[] heritageKeys =
            {
                "IconRoyal&Government",
                "IconReligious",
                "IconMilitary",
                "IconNobel&PrivateResidence",
                "IconIndustry&Trade",
                "IconInfrastructures"
            };

            foreach (string key in heritageKeys)
            {
                Assert.IsNotNull(library.Get(key),
                    $"Default heritage icon key '{key}' must resolve to a registered sprite.");
            }
        }

        // The authoring tool seeds four building damage levels when a wall has no
        // badge_categories (DefaultBadgeCategories, _5.3_Default_Icons.md). Each row's
        // icon_key must resolve to a real sprite in the wall's runtime icon library,
        // otherwise the badge preview in the editor shows a gray box and the badge
        // icon never renders on markers at runtime.
        [Test]
        public void DefaultDamageIconKeys_ResolveToSprites()
        {
            SpriteKeyLibrary library = Resources.Load<SpriteKeyLibrary>(ResourcesKeyPath);
            Assert.IsNotNull(library, "Setup precondition: wall library must load.");

            string[] damageKeys =
            {
                "IconIntact",
                "IconPartialDamage",
                "IconDestroyed",
                "IconUnknownDamage"
            };

            foreach (string key in damageKeys)
            {
                Assert.IsNotNull(library.Get(key),
                    $"Default damage icon key '{key}' must resolve to a registered sprite.");
            }
        }

        // The authoring tool seeds six outline levels when a wall has no
        // outline_levels (DefaultOutlineLevels, _5.3_Default_Icons.md). Each level's
        // line_style must resolve to a real ring sprite in the wall's runtime icon
        // library, otherwise the status ring on markers renders as a gray box and
        // the "default rows appear in the outline table" feature is only half-wired.
        [Test]
        public void DefaultRingStyleKeys_ResolveToSprites()
        {
            SpriteKeyLibrary library = Resources.Load<SpriteKeyLibrary>(ResourcesKeyPath);
            Assert.IsNotNull(library, "Setup precondition: wall library must load.");

            string[] ringStyleKeys =
            {
                "solid",
                "dash_long",
                "dash_medium",
                "dash_short",
                "dotted"
            };

            foreach (string key in ringStyleKeys)
            {
                Assert.IsNotNull(library.Get(key),
                    $"Default ring style key '{key}' must resolve to a registered sprite.");
            }
        }
    }
}
