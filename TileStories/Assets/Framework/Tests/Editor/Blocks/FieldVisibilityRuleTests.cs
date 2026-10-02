using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // Which fields the Editor draws (_3.1 step 15.1): FieldShownWhen judged by FieldVisibilityRule on the REAL built-in kinds -- the look a
    // block is drawn in (its own, else the Block Library's), a Choice's effective option (its own, else the Library's), a row's own words.
    // Hiding never touches a value; BlockRegistry.Validate refuses a ShownWhen that names something the kind does not have.
    public class FieldVisibilityRuleTests
    {
        private static BlockInstanceData Block(string kind, string variant = "") => new() { key = "b", kind = kind, variant = variant };

        private static CardSettings LibraryWith(string kind, string defaultVariant = "", string fieldKey = null, string fieldValue = null)
        {
            var row = new BlockKindSetting { kind = kind, default_variant = defaultVariant };
            if (fieldKey != null) row.field_defaults = new List<BlockFieldDefault> { new() { key = fieldKey, value = fieldValue } };
            return new CardSettings { kinds = new List<BlockKindSetting> { row } };
        }

        // The keys of a block's fields the Editor draws, in order
        private static string[] ShownFields(BlockKindDefinition kind, BlockInstanceData block, CardSettings settings) =>
            kind.Fields.Where(f => FieldVisibilityRule.IsShown(f, block, kind, settings)).Select(f => f.Key).ToArray();

        // The keys of one Items row's sub-fields the Editor draws, in order
        private static string[] ShownSubFields(BlockKindDefinition kind, string itemsKey, BlockItemData row, BlockInstanceData block, CardSettings settings)
        {
            var items = kind.Field(itemsKey);
            return items.ItemFields.Where(s => FieldVisibilityRule.IsShown(s, row, items, block, kind, settings)).Select(s => s.Key).ToArray();
        }

        private static BlockItemData RowWithText(params (string key, string lang, string text)[] values)
        {
            var row = new BlockItemData();
            foreach (var (key, lang, text) in values)
                row.fields.Add(new BlockItemFieldValue { key = key, text = new List<LocalizedEntry> { new() { lang = lang, value = text } } });
            return row;
        }

        // ---------------- the predicate ----------------

        [Test]
        public void ThePredicate_Looks_Choice_Filled_HoldOnlyForWhatTheyName()
        {
            var looks = FieldShownWhen.Looks("a", "b");
            Assert.IsTrue(looks.Holds("b", null, null));
            Assert.IsFalse(looks.Holds("c", null, null));
            Assert.IsFalse(looks.Holds(null, null, null), "no look: not shown");

            var choice = FieldShownWhen.Choice("scale", "height_cm");
            Assert.IsTrue(choice.Holds("x", key => key == "scale" ? "height_cm" : "", null));
            Assert.IsFalse(choice.Holds("x", key => "real_size", null));
            Assert.IsFalse(choice.Holds("x", key => null, null), "no option: not shown");

            var filled = FieldShownWhen.Filled("choice_1");
            Assert.IsTrue(filled.Holds("x", null, key => key == "choice_1"));
            Assert.IsFalse(filled.Holds("x", null, key => false));
        }

        [Test]
        public void AFieldWithoutACondition_IsAlwaysShown_InEveryLook()
        {
            var title = BuiltInBlocks.Header.Field(BlockStackBuilder.HeaderTitleField);
            Assert.IsNull(title.ShownWhen);
            foreach (string look in BuiltInBlocks.Header.Variants)
                Assert.IsTrue(FieldVisibilityRule.IsShown(title, Block(BuiltInBlocks.HeaderKind, look), BuiltInBlocks.Header, null), look);
        }

        // ---------------- place_in_ar: Height / Marker Multiple by Scale Mode ----------------

        [Test]
        public void PlaceInAr_HeightAndMarkerMultiple_FollowTheEffectiveScaleMode_TheBlocksOwnThenTheBlockLibrarys()
        {
            var kind = BuiltInBlocks.PlaceInAr;
            var block = Block(BuiltInBlocks.PlaceInArKind);
            string[] common = { BlockKindDefinition.HeadingField, BuiltInBlocks.PlaceInArModelField, BuiltInBlocks.PlaceInArAnchorField,
                BuiltInBlocks.PlaceInArOffsetField, BuiltInBlocks.PlaceInArScaleField, BuiltInBlocks.PlaceInArLabelField };

            CollectionAssert.AreEqual(common, ShownFields(kind, block, null), "no Scale Mode: Real Size (the field's default) needs neither");

            POIEditorToolWindow.SetChoiceValue(block, BuiltInBlocks.PlaceInArScaleField, ArPlacementRule.ScaleHeightCm);
            CollectionAssert.Contains(ShownFields(kind, block, null), BuiltInBlocks.PlaceInArHeightField);
            CollectionAssert.DoesNotContain(ShownFields(kind, block, null), BuiltInBlocks.PlaceInArMultipleField);

            POIEditorToolWindow.SetChoiceValue(block, BuiltInBlocks.PlaceInArScaleField, ArPlacementRule.ScaleMarkerMultiple);
            CollectionAssert.Contains(ShownFields(kind, block, null), BuiltInBlocks.PlaceInArMultipleField);
            CollectionAssert.DoesNotContain(ShownFields(kind, block, null), BuiltInBlocks.PlaceInArHeightField);

            // - a block that names no Scale Mode reads the wall's Default Scale Mode: its Height row is drawn
            var empty = Block(BuiltInBlocks.PlaceInArKind);
            var wall = LibraryWith(BuiltInBlocks.PlaceInArKind, fieldKey: BuiltInBlocks.PlaceInArScaleField, fieldValue: ArPlacementRule.ScaleHeightCm);
            CollectionAssert.Contains(ShownFields(kind, empty, wall), BuiltInBlocks.PlaceInArHeightField, "the Block Library's default reaches the Editor");
            // - a stale option falls through to the default like the card's own read (BlockLibraryRule.Choice)
            POIEditorToolWindow.SetChoiceValue(empty, BuiltInBlocks.PlaceInArScaleField, "giant");
            CollectionAssert.Contains(ShownFields(kind, empty, wall), BuiltInBlocks.PlaceInArHeightField, "a stale option reads the default");
        }

        // ---------------- knowledge_check: the fields of each look ----------------

        [Test]
        public void KnowledgeCheck_EachLook_DrawsOnlyTheRowFieldsItUses_AndANamelessBlockUsesTheLibrarysLook()
        {
            var kind = BuiltInBlocks.KnowledgeCheck;
            var row = new BlockItemData();
            string[] options = { "option_1", "option_2", "option_3", "option_4" };
            string[] pictures = { "image_1", "image_2", "image_3", "image_4" };
            string q = BuiltInBlocks.KnowledgeCheckQuestionField, right = BuiltInBlocks.KnowledgeCheckCorrectField,
                isTrue = BuiltInBlocks.KnowledgeCheckIsTrueField, why = BuiltInBlocks.KnowledgeCheckExplanationField;

            CollectionAssert.AreEqual(new[] { q }.Concat(options).Concat(new[] { right, why }),
                ShownSubFields(kind, BuiltInBlocks.KnowledgeCheckQuestionsField, row, Block(kind.Key, BuiltInBlocks.KnowledgeCheckMultipleChoice), null),
                "Multiple Choice: the question, four options, the right one, the explanation (7)");
            CollectionAssert.AreEqual(new[] { q, isTrue, why },
                ShownSubFields(kind, BuiltInBlocks.KnowledgeCheckQuestionsField, row, Block(kind.Key, BuiltInBlocks.KnowledgeCheckTrueFalseSwipe), null),
                "True / False Swipe: the statement, whether it is true, the explanation (3)");
            CollectionAssert.AreEqual(new[] { q }.Concat(options).Concat(pictures).Concat(new[] { right, why }),
                ShownSubFields(kind, BuiltInBlocks.KnowledgeCheckQuestionsField, row, Block(kind.Key, BuiltInBlocks.KnowledgeCheckImageChoice), null),
                "Image Choice: the options as captions AND their pictures (11)");

            var wall = LibraryWith(kind.Key, BuiltInBlocks.KnowledgeCheckTrueFalseSwipe);
            CollectionAssert.AreEqual(new[] { q, isTrue, why }, ShownSubFields(kind, BuiltInBlocks.KnowledgeCheckQuestionsField, row, Block(kind.Key), wall),
                "a block with no look of its own is drawn in the Block Library's default look");
            CollectionAssert.AreEqual(new[] { q, isTrue, why }, ShownSubFields(kind, BuiltInBlocks.KnowledgeCheckQuestionsField, row, Block(kind.Key, "flashcards"), wall),
                "a look the kind lacks reads the default too");
            // - the block-level fields (heading, the rows, Show After Reading) never depend on the look
            CollectionAssert.AreEqual(kind.Fields.Select(f => f.Key), ShownFields(kind, Block(kind.Key, BuiltInBlocks.KnowledgeCheckTrueFalseSwipe), null));
        }

        // ---------------- dialogue: the next Choice / Reply once the one before has words ----------------

        [Test]
        public void Dialogue_ChoiceAndReplyNPlusOne_AppearOnlyOnceChoiceNHasWords_InAnyLanguage()
        {
            var kind = BuiltInBlocks.Dialogue;
            var block = Block(kind.Key);
            string[] Shown(BlockItemData row) => ShownSubFields(kind, BuiltInBlocks.DialogueLinesField, row, block, null);
            string speaker = BuiltInBlocks.DialogueSpeakerField, line = BuiltInBlocks.DialogueTextField;

            CollectionAssert.AreEqual(new[] { speaker, line, "choice_1", "reply_1" }, Shown(new BlockItemData()), "a plain line: one Choice, one Reply");
            CollectionAssert.AreEqual(new[] { speaker, line, "choice_1", "choice_2", "reply_1", "reply_2" }, Shown(RowWithText(("choice_1", "pt", "Sim"))),
                "Choice 1 has words (Portuguese only is enough): Choice 2 and Reply 2 appear");
            CollectionAssert.AreEqual(new[] { speaker, line, "choice_1", "choice_2", "choice_3", "reply_1", "reply_2", "reply_3" },
                Shown(RowWithText(("choice_1", "en", "Yes"), ("choice_2", "en", "No"))), "all three once Choice 2 has words");
            CollectionAssert.AreEqual(new[] { speaker, line, "choice_1", "reply_1" }, Shown(RowWithText(("choice_1", "en", "   "))),
                "blank words are no words");
        }

        // ---------------- header: each look's own fields ----------------

        [Test]
        public void Header_EachLook_DrawsItsOwnFields_AndNoOther()
        {
            var kind = BuiltInBlocks.Header;
            string title = BlockStackBuilder.HeaderTitleField, subtitle = BlockStackBuilder.HeaderSubtitleField, level = BuiltInBlocks.HeaderShowLevelField,
                picture = BuiltInBlocks.HeaderImageField, heading = BlockKindDefinition.HeadingField;
            var expected = new Dictionary<string, string[]>
            {
                [BuiltInBlocks.HeaderCompact] = new[] { heading, title, level },
                [BuiltInBlocks.HeaderTextOnly] = new[] { heading, title, subtitle, level },
                [BuiltInBlocks.HeaderImageParallax] = new[] { heading, title, subtitle, level, picture },
                [BuiltInBlocks.HeaderSplitThenNow] = new[] { heading, title, subtitle, level, picture, BuiltInBlocks.HeaderSecondImageField },
                [BuiltInBlocks.HeaderSpotlightCrop] = new[] { heading, title, subtitle, level, picture, BuiltInBlocks.HeaderFocusXField, BuiltInBlocks.HeaderFocusYField, BuiltInBlocks.HeaderZoomField },
                [BuiltInBlocks.HeaderVideoLoop] = new[] { heading, title, subtitle, level, picture, BuiltInBlocks.HeaderLoopClipField },
                // - the picture is the turntable's Fallback (HeaderBlockView hands it to the model view)
                [BuiltInBlocks.HeaderModelTurntable] = new[] { heading, title, subtitle, level, picture, BuiltInBlocks.HeaderModelField, BuiltInBlocks.HeaderModelFitField },
            };
            CollectionAssert.AreEquivalent(kind.Variants, expected.Keys, "every look is checked");
            foreach (var pair in expected)
                CollectionAssert.AreEqual(pair.Value, ShownFields(kind, Block(kind.Key, pair.Key), null), pair.Key);
        }

        // ---------------- video: Chapters for the Chapters look ----------------

        [Test]
        public void Video_ChaptersRows_OnlyForTheChaptersLook_AndTheRestForBoth()
        {
            var kind = BuiltInBlocks.Video;
            CollectionAssert.DoesNotContain(ShownFields(kind, Block(kind.Key, BuiltInBlocks.VideoInline), null), BuiltInBlocks.VideoChaptersField);
            CollectionAssert.AreEqual(kind.Fields.Select(f => f.Key), ShownFields(kind, Block(kind.Key, BuiltInBlocks.VideoChapters), null));
            Assert.AreEqual(kind.Fields.Count - 1, ShownFields(kind, Block(kind.Key, BuiltInBlocks.VideoInline), null).Length, "only Chapters is left out");
        }

        // ---------------- hiding never writes ----------------

        [Test]
        public void JudgingVisibility_NeverChangesTheBlock()
        {
            var block = Block(BuiltInBlocks.PlaceInArKind);
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArHeightField, number = 80f });
            string before = UnityEngine.JsonUtility.ToJson(block);
            ShownFields(BuiltInBlocks.PlaceInAr, block, LibraryWith(BuiltInBlocks.PlaceInArKind));
            Assert.AreEqual(before, UnityEngine.JsonUtility.ToJson(block), "a hidden Height keeps its 80 cm");
        }

        // ---------------- the Block Library's default rows ----------------

        [Test]
        public void TheLibrarysHeaderDefaultFit_IsDrawnWhileAHeaderCanUseIt_TheDefaultLookOrAnyHeaderOnTheWall()
        {
            var kind = BuiltInBlocks.Header;
            var fit = kind.Field(BuiltInBlocks.HeaderModelFitField);
            Assert.IsTrue(fit.LibraryDefault, "precondition: the header's Fit has a Block Library row");

            Assert.IsFalse(FieldVisibilityRule.IsShownInLibrary(fit, kind, new CardSettings(), new List<POIData>()), "the stock default look is Text Only: no model to fit");
            Assert.IsTrue(FieldVisibilityRule.IsShownInLibrary(fit, kind, LibraryWith(kind.Key, BuiltInBlocks.HeaderModelTurntable), null),
                "the wall's default header look is Model Turntable");

            var picturesOnly = new POIData { id = "p1", card = new POICardData { blocks = { Block(kind.Key, BuiltInBlocks.HeaderImageParallax) } } };
            Assert.IsFalse(FieldVisibilityRule.IsShownInLibrary(fit, kind, new CardSettings(), new List<POIData> { picturesOnly }));
            var withModel = new POIData { id = "p2", card = new POICardData { blocks = { Block(kind.Key, BuiltInBlocks.HeaderModelTurntable) } } };
            Assert.IsTrue(FieldVisibilityRule.IsShownInLibrary(fit, kind, new CardSettings(), new List<POIData> { picturesOnly, withModel }),
                "one header on the wall drawn as a model: the default it would read is never hidden");

            // - a default row with no condition (3D Model's Fit, Place In AR's Scale Mode) is always drawn
            Assert.IsTrue(FieldVisibilityRule.IsShownInLibrary(BuiltInBlocks.Model3D.Field(BuiltInBlocks.Model3DFitField), BuiltInBlocks.Model3D, new CardSettings(), null));
            Assert.IsTrue(FieldVisibilityRule.IsShownInLibrary(BuiltInBlocks.PlaceInAr.Field(BuiltInBlocks.PlaceInArScaleField), BuiltInBlocks.PlaceInAr, new CardSettings(), null));
        }

        // ---------------- the Display popup ----------------

        [Test]
        public void TheDisplayPopup_IsOfferedOnlyByKindsWithTwoDisplaysOrMore()
        {
            var offering = BlockRegistry.Shared.All.Where(POIEditorToolWindow.CardBlockOffersDisplayChoice).Select(k => k.Key).ToList();
            CollectionAssert.AreEquivalent(new[] { BuiltInBlocks.VideoKind, BuiltInBlocks.Model3DKind, BuiltInBlocks.Panorama360Kind }, offering,
                "the three teaser kinds (inline / takeover); every other kind has inline only");
            Assert.IsFalse(POIEditorToolWindow.CardBlockOffersDisplayChoice(BuiltInBlocks.Header));
        }

        // ---------------- the declarations are checked ----------------

        [Test]
        public void BlockRegistryValidate_RefusesACondition_ThatNamesALookAFieldOrAnOptionTheKindDoesNotHave()
        {
            BlockKindDefinition KindWith(params BlockFieldDefinition[] fields) => new()
            {
                Key = "k", Family = "about", DisplayName = "K", Help = "h", Variants = new[] { "one", "two" }, DefaultVariant = "one",
                DisplayModes = new[] { CardOptions.DisplayInline }, Fields = fields,
            };
            var mode = new BlockFieldDefinition { Key = "mode", Type = BlockFieldType.Choice, Options = new[] { "a", "b" } };
            var words = new BlockFieldDefinition { Key = "words", Type = BlockFieldType.LocalizedText };
            BlockFieldDefinition Shown(FieldShownWhen when) => new() { Key = "x", Type = BlockFieldType.Number, ShownWhen = when };

            Assert.IsNull(BlockRegistry.Validate(KindWith(mode, words, Shown(FieldShownWhen.Looks("two")))), "a real look");
            Assert.IsNull(BlockRegistry.Validate(KindWith(mode, words, Shown(FieldShownWhen.Choice("mode", "b")))), "a real option");
            Assert.IsNull(BlockRegistry.Validate(KindWith(mode, words, Shown(FieldShownWhen.Filled("words")))), "a real text field");

            StringAssert.Contains("not one of the kind's looks", BlockRegistry.Validate(KindWith(mode, Shown(FieldShownWhen.Looks("three")))));
            StringAssert.Contains("not one of its options", BlockRegistry.Validate(KindWith(mode, Shown(FieldShownWhen.Choice("mode", "c")))));
            StringAssert.Contains("not a Choice field", BlockRegistry.Validate(KindWith(words, Shown(FieldShownWhen.Choice("words", "a")))));
            StringAssert.Contains("not a text field", BlockRegistry.Validate(KindWith(mode, Shown(FieldShownWhen.Filled("mode")))));
            StringAssert.Contains("not another field beside it", BlockRegistry.Validate(KindWith(mode, Shown(FieldShownWhen.Filled("missing")))));
            StringAssert.Contains("not another field beside it", BlockRegistry.Validate(KindWith(new BlockFieldDefinition
                { Key = "self", Type = BlockFieldType.LocalizedText, ShownWhen = FieldShownWhen.Filled("self") })), "never itself");
            // - a row's sub-field names a field of its OWN row, not one of the block
            var rows = new BlockFieldDefinition { Key = "rows", Type = BlockFieldType.Items, ItemFields = new[] { Shown(FieldShownWhen.Filled("words")) } };
            StringAssert.Contains("not another field beside it", BlockRegistry.Validate(KindWith(words, rows)));
            // - and every built-in kind's declarations pass (an error would also stop BuiltInBlocks from registering)
            foreach (var kind in BlockRegistry.Shared.All) Assert.IsNull(BlockRegistry.Validate(kind), kind.Key);
        }
    }
}
