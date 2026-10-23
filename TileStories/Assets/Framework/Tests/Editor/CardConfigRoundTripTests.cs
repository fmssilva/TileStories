using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The POI Detail Card config (_3.1 section 5) survives Save All to JSON -> load through JsonUtility, the one
    // serializer of the POI Editor, its undo history and WallConfigLoader. The whole card tree is walked by
    // reflection -- every leaf set to a non-default, every list given one filled element -- so a field added later
    // is covered without touching this test, and the test fails if a card type is never reached.
    public class CardConfigRoundTripTests
    {
        private static readonly Type[] CardTypes =
        {
            typeof(CardSettings), typeof(CardContainerSettings), typeof(CardNavigationSettings), typeof(CardFamilySetting),
            typeof(BlockKindSetting), typeof(POICardData),
            typeof(BlockInstanceData), typeof(BlockFieldValue), typeof(BlockItemData), typeof(BlockItemFieldValue),
            typeof(LocalizedEntry), typeof(CardStringEntry), typeof(GlossaryEntry), typeof(BlockFieldDefault),
        };

        // Set every public field of `target` (and of everything it holds) to a value that differs from the default
        private static void Fill(object target, HashSet<Type> reached, int seed)
        {
            reached.Add(target.GetType());
            foreach (var f in target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var t = f.FieldType;
                if (t == typeof(bool)) f.SetValue(target, !(bool)f.GetValue(target));
                else if (t == typeof(int)) f.SetValue(target, (int)f.GetValue(target) + 7 + seed);
                else if (t == typeof(float)) f.SetValue(target, (float)f.GetValue(target) + 0.125f + seed);
                else if (t == typeof(string)) f.SetValue(target, f.Name + "_" + seed);
                else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var list = (IList)Activator.CreateInstance(t);
                    var element = t.GetGenericArguments()[0];
                    if (element == typeof(string)) list.Add("s_" + seed);
                    else { var item = Activator.CreateInstance(element); Fill(item, reached, seed + 1); list.Add(item); }
                    f.SetValue(target, list);
                }
                else if (t.IsClass)
                {
                    var child = Activator.CreateInstance(t);
                    Fill(child, reached, seed + 1);
                    f.SetValue(target, child);
                }
                else throw new AssertionException("no round-trip value for " + target.GetType().Name + "." + f.Name);
            }
        }

        [Test]
        public void EveryCardField_WalkedByReflection_SurvivesAJsonRoundTrip()
        {
            var reached = new HashSet<Type>();
            var config = new WallConfigData { wall_id = "rt" };
            Fill(config.card_settings, reached, 0);
            var poi = new POIData { id = "p", name = "P" };
            Fill(poi.card, reached, 0);
            config.pois.Add(poi);

            CollectionAssert.IsSubsetOf(CardTypes, reached, "the walk reached every card type (not vacuous)");
            string written = JsonUtility.ToJson(config, true);
            Assert.AreNotEqual(JsonUtility.ToJson(new CardSettings()), JsonUtility.ToJson(config.card_settings), "precondition: values differ from the defaults");

            var loaded = JsonUtility.FromJson<WallConfigData>(written);
            Assert.AreEqual(JsonUtility.ToJson(config.card_settings), JsonUtility.ToJson(loaded.card_settings), "card_settings survives");
            Assert.AreEqual(JsonUtility.ToJson(poi.card), JsonUtility.ToJson(loaded.pois[0].card), "POIData.card survives");

            // - spot-check the deepest leaf, so a silently empty list cannot pass the string compare above
            var deep = loaded.pois[0].card.blocks[0].fields[0].items[0].fields[0].text[0];
            Assert.AreEqual("lang_5", deep.lang);
            Assert.AreEqual("value_5", deep.value);
        }

        [Test]
        public void AConfigWrittenBeforeTheCard_LoadsWithTheDefaults_AndEveryPoiGetsAnEmptyCard()
        {
            var loaded = JsonUtility.FromJson<WallConfigData>("{\"wall_id\":\"old\",\"pois\":[{\"id\":\"a\",\"name\":\"A\"}]}");
            var s = loaded.card_settings;
            Assert.IsNotNull(s);
            Assert.IsTrue(s.enabled, "the card is on by default");
            CollectionAssert.AreEqual(new[] { "en", "pt" }, s.languages, "English first (the fallback), then Portuguese");
            Assert.AreEqual("", s.media_resources_path);
            Assert.AreEqual(CardOptions.StopPeek, s.container.open_stop, "opens at peek, never auto-expanded");
            Assert.AreEqual(0.40f, s.container.half_max_ratio, 1e-6);
            Assert.IsTrue(s.container.dismiss_on_tap_outside);
            Assert.IsTrue(s.container.keep_audio_on_close);
            CollectionAssert.IsEmpty(s.kinds, "no Block Library rows = every kind enabled with its default variant");
            // - _3.2 step 2: the nav model's three fields load on _3.2 section 3's defaults
            Assert.AreEqual(CardOptions.GroupByFamily, s.navigation.group_by, "sections are families unless the wall says format");
            Assert.AreEqual(8, s.navigation.min_blocks, "a card shows navigation from 8 targets");
            Assert.AreEqual(5, s.navigation.max_slots, "a bar holds 5 slots before More");
            // - _3.2 step 7a: an old config has no Bottom Bar
            Assert.AreEqual(CardOptions.NavNone, s.navigation.bottom, "no bottom bar unless the wall picks one");
            // - _3.2 step 8: an old config has no Menu (never a default)
            Assert.AreEqual(CardOptions.NavNone, s.navigation.menu, "no menu button unless the wall picks one");
            // - _3.2 step 9: an old config has no Floating Button, and a placed model gets its chip
            Assert.AreEqual(CardOptions.NavNone, s.navigation.floating, "no floating button unless the wall picks one");
            // - _3.2 step 10: an old config has no Edge element (it lies over the content, so it is opt-in)
            Assert.AreEqual(CardOptions.NavNone, s.navigation.edge, "no edge element unless the wall picks one");
            Assert.AreEqual(CardOptions.NavNone, s.navigation.auto_nav_block, "no nav block is added to any card unless the wall picks one");
            Assert.AreEqual(CardOptions.PlacedModelChip, s.navigation.placed_model_control, "a placed model says so above the card");
            // - _3.2 step 3: the jump and spy options load on their section 3 defaults
            Assert.IsTrue(s.navigation.follow_scroll, "the marked section follows the scroll");
            Assert.AreEqual(CardOptions.JumpRaisesFull, s.navigation.jump_raises_to, "a jump first raises the card to Full");
            Assert.AreEqual(CardOptions.JumpMotionSmooth, s.navigation.jump_motion, "a jump glides");
            // - _3.2 step 4: an old config steps the card down on Back
            Assert.AreEqual(CardOptions.BackButtonStepDown, s.navigation.back_button, "Back steps the card down");
            // - _3.2 step 5b: the chrome options load on their section 3 defaults
            Assert.AreEqual(CardOptions.ShowNavHalfAndFull, s.navigation.show_at, "the strip may show at Half and Full");
            Assert.AreEqual(CardOptions.HideOnScrollDown, s.navigation.hide_on_scroll, "the bottom area hides while the reader moves down");
            // - _3.2 step 6: the top elements' options load on their section 3 defaults; no Groups rows = the framework's four
            Assert.AreEqual(CardOptions.LabelsIconAndText, s.navigation.labels, "icon tabs show icon and words");
            Assert.IsFalse(s.navigation.show_counts, "no block counts");
            CollectionAssert.IsEmpty(s.navigation.groups, "no Groups rows written");
            CollectionAssert.AreEqual(NavGroupRule.DefaultKeys, NavGroupRule.Groups(s.navigation).Select(g => g.Key), "so the four defaults apply");
            var family = JsonUtility.FromJson<CardFamilySetting>("{\"family\":\"about\",\"order\":10,\"at_end\":false}");
            Assert.AreEqual("", family.group, "a Families row written before step 6 has no group: its family's default");
            Assert.AreEqual(NavGroupRule.Discover, NavGroupRule.GroupOf(BuiltInFamilies.About, new CardNavigationSettings { families = { family } }));
            Assert.IsNotNull(loaded.pois[0].card);
            CollectionAssert.IsEmpty(loaded.pois[0].card.blocks, "no blocks = the header-only card");
        }

        [Test]
        public void AnExplicitNullCard_IsRepairedOnLoad()
        {
            var loaded = JsonUtility.FromJson<POIData>("{\"id\":\"a\",\"card\":{\"blocks\":null}}");
            Assert.IsNotNull(loaded.card.blocks, "OnAfterDeserialize guards the list like the keyword lists");
        }

        [Test]
        public void TheShippedWall_LoadsWithItsCard_AndKeepsEveryPoiIdentityTheSearchTestsRelyOn()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            Assert.IsNotNull(config.card_settings);
            Assert.IsTrue(config.pois.All(p => p.card != null && p.card.blocks != null), "every POI has a card object");
            var lamp = config.pois.Single(p => p.id == "lamp");
            var military = config.pois.Single(p => p.id == "lamp_military");
            Assert.IsNotEmpty(lamp.name, "the wall's own name is the identity the scene tests read back from the config");
            Assert.IsNotEmpty(military.name);
            Assert.AreEqual("military", military.category);
        }
    }
}
