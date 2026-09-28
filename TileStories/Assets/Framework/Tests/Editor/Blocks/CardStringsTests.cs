using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The card's UI texts (_3.1 step 5b, step 11-fix): the lookup order of CardStrings over the wall's, the app's and the framework's
    // wording, the app-table registry, and the SHIPPED framework table (CardStrings.asset) against every key the card's code reads -- so a
    // view can never show a raw key.
    public class CardStringsTests
    {
        private const string AssetPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";

        private static CardStringEntry Entry(string key, params (string Lang, string Value)[] texts) =>
            new() { key = key, text = texts.Select(t => new LocalizedEntry { lang = t.Lang, value = t.Value }).ToList() };

        private static readonly List<CardStringEntry> Framework = new()
        {
            Entry("close", ("en", "Close"), ("pt", "Fechar")),
            Entry("hint", ("en", "Tap to reveal")),
        };

        private readonly List<CardStringTable> _tables = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var t in _tables) if (t != null) UnityEngine.Object.DestroyImmediate(t);
            _tables.Clear();
        }

        private CardStringTable Table(params CardStringEntry[] rows)
        {
            var table = ScriptableObject.CreateInstance<CardStringTable>();
            _tables.Add(table);
            foreach (var r in rows) table.rows.Add(new CardStringTable.Row { key = r.key, where = "test", text = r.text });
            return table;
        }

        [Test]
        public void TheWallsWording_WinsInTheVisitorsLanguage()
        {
            var wall = new List<CardStringEntry> { Entry("close", ("pt", "Sair")) };
            Assert.AreEqual("Sair", new CardStrings(Framework, null, wall, "pt", "en").Get("close"));
        }

        [Test]
        public void AWallsFallbackWording_NeverHidesTheFrameworksTextInTheVisitorsLanguage()
        {
            var wall = new List<CardStringEntry> { Entry("close", ("en", "Dismiss")) };
            Assert.AreEqual("Fechar", new CardStrings(Framework, null, wall, "pt", "en").Get("close"), "framework [pt] before wall [en]");
            Assert.AreEqual("Dismiss", new CardStrings(Framework, null, wall, "en", "en").Get("close"), "wall [en] for an English visitor");
        }

        [Test]
        public void AMissingLanguage_FallsBack_WallFirst_ThenFramework_ThenTheKey()
        {
            Assert.AreEqual("Tap to reveal", new CardStrings(Framework, null, null, "pt", "en").Get("hint"), "framework fallback");
            var wall = new List<CardStringEntry> { Entry("hint", ("en", "Tap it")) };
            Assert.AreEqual("Tap it", new CardStrings(Framework, null, wall, "pt", "en").Get("hint"), "wall fallback before framework fallback");
            Assert.AreEqual("nowhere", new CardStrings(Framework, null, wall, "pt", "en").Get("nowhere"), "an unknown key shows as itself");
        }

        [Test]
        public void ABlankWallField_KeepsTheFrameworksWording()
        {
            var wall = new List<CardStringEntry> { Entry("close", ("en", "  ")) };
            Assert.AreEqual("Close", new CardStrings(Framework, null, wall, "en", "en").Get("close"), "an emptied Card Texts field is not a wording");
        }

        // ---- the app layer (step 11-fix): wall > app > framework in a language, then the same in the fallback language ----

        [Test]
        public void AnAppsWording_SitsBetweenTheWallsAndTheFrameworks_InOneLanguage()
        {
            var app = new List<CardStringEntry> { Entry("close", ("en", "App close")) };
            var wall = new List<CardStringEntry> { Entry("close", ("en", "Wall close")) };
            Assert.AreEqual("Wall close", new CardStrings(Framework, app, wall, "en", "en").Get("close"), "wall [en] beats app [en] and framework [en]");
            Assert.AreEqual("App close", new CardStrings(Framework, app, null, "en", "en").Get("close"), "app [en] beats framework [en]");
            Assert.AreEqual("Close", new CardStrings(Framework, null, null, "en", "en").Get("close"), "framework [en] alone");
            var blankWall = new List<CardStringEntry> { Entry("close", ("en", " ")) };
            Assert.AreEqual("App close", new CardStrings(Framework, app, blankWall, "en", "en").Get("close"), "a blank wall field is no wording: the app's shows");
        }

        [Test]
        public void TheVisitorsLanguage_IsTriedAtEveryLayer_BeforeAnyFallbackWording()
        {
            var app = new List<CardStringEntry> { Entry("coin", ("en", "Coin"), ("pt", "Moeda")) };
            var wall = new List<CardStringEntry> { Entry("coin", ("en", "Wall coin")) };
            Assert.AreEqual("Moeda", new CardStrings(Framework, app, wall, "pt", "en").Get("coin"), "app [pt] before wall [en]");
            var appEnglishOnly = new List<CardStringEntry> { Entry("coin", ("en", "Coin")) };
            Assert.AreEqual("Wall coin", new CardStrings(Framework, appEnglishOnly, wall, "pt", "en").Get("coin"), "no pt anywhere: wall [en] before app [en]");
            Assert.AreEqual("Coin", new CardStrings(Framework, appEnglishOnly, null, "pt", "en").Get("coin"), "no pt anywhere, no wall: app [en]");
            Assert.AreEqual("coin", new CardStrings(Framework, null, null, "pt", "en").Get("coin"), "no layer knows it: the key");
        }

        [Test]
        public void AnAppRowKeepsItsFrameworkNeighbours_TheFrameworkStillAnswersItsOwnKeys()
        {
            var app = new List<CardStringEntry> { Entry("coin", ("en", "Coin")) };
            Assert.AreEqual("Fechar", new CardStrings(Framework, app, null, "pt", "en").Get("close"), "an app table never hides a framework text");
        }

        // ---- the registry an app registers its table in ----

        [Test]
        public void TheRegistry_KeepsTablesUnderTheAppsName_InOrder_AndHandsTheirRowsToTheLookup()
        {
            var sources = new CardStringSources();
            var first = Table(Entry("a_one", ("en", "One")));
            var second = Table(Entry("b_two", ("en", "Two")));
            sources.Add("First App", first);
            sources.Add("Second App", second);
            CollectionAssert.AreEqual(new[] { "First App", "Second App" }, sources.All.Select(s => s.AppName));
            Assert.AreSame(first, sources.All[0].Table);
            Assert.IsTrue(sources.Has("First App"));
            Assert.AreEqual("Two", new CardStrings(Framework, sources.Entries(), null, "en", "en").Get("b_two"));
            Assert.IsTrue(sources.Remove("First App"));
            Assert.IsFalse(sources.Remove("First App"), "removing twice reports nothing removed");
            CollectionAssert.AreEqual(new[] { "b_two" }, sources.Entries().Select(e => e.key));
        }

        [Test]
        public void TheRegistry_Refuses_ABlankName_ANullTable_ASecondTableOfTheSameApp_AndAKeyAnotherAppHolds()
        {
            var sources = new CardStringSources();
            var table = Table(Entry("shared_key", ("en", "x")));
            Assert.Throws<ArgumentException>(() => sources.Add("  ", table), "no name");
            Assert.Throws<ArgumentNullException>(() => sources.Add("App", null), "no table");
            sources.Add("App", table);
            Assert.Throws<ArgumentException>(() => sources.Add("App", Table(Entry("other", ("en", "y")))), "the same app twice");
            Assert.Throws<ArgumentException>(() => sources.Add("Other App", Table(Entry("shared_key", ("en", "z")))), "one key, two apps");
            Assert.AreEqual(1, sources.All.Count, "the refused ones left nothing behind");
        }

        // ---- the framework's own table ----

        [Test]
        public void TheShippedTable_HasEveryKeyTheCardReads_InEnglishAndPortuguese_WithAWhereNote()
        {
            var table = AssetDatabase.LoadAssetAtPath<CardStringTable>(AssetPath);
            Assert.IsNotNull(table, AssetPath + " exists");
            CardStringTableChecks.AssertTableHasEveryKey(table, CardStrings.Keys.All, "framework");
        }

        [Test]
        public void EveryKeyTheCardCodeReads_IsInKeysAll()
        {
            // - a view that reads CardStrings.Keys.X for an X missing from All would never be guarded above
            CardStringTableChecks.AssertEveryKeyReadInSourceIsListed("Assets/Framework/Runtime", "CardStrings.Keys.", typeof(CardStrings.Keys), CardStrings.Keys.All, "framework");
        }
    }
}
