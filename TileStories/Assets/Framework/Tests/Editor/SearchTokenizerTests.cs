using NUnit.Framework;
using System.Collections.Generic;

namespace TileStories.Tests
{
    // Tier 0 EditMode tests for SearchTokenizer.
    // Pure logic, no MonoBehaviour, no scene -- instantiable with new().
    public class SearchTokenizerTests
    {
        [Test]
        public void NullInput_ReturnsEmpty()
        {
            var result = SearchTokenizer.Tokenize(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void EmptyInput_ReturnsEmpty()
        {
            var result = SearchTokenizer.Tokenize("");
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void WhitespaceOnly_ReturnsEmpty()
        {
            var result = SearchTokenizer.Tokenize("   \t  ");
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void PunctuationOnly_ReturnsEmpty()
        {
            var result = SearchTokenizer.Tokenize("!@#$%^&*()");
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Accented_StripsDiacritics()
        {
            // "Sao" + combining tilde + " Jo" + combining tilde + "o"
            // After NFKD + diacritic stripping -> "Sao Joao" -> lowercase -> "sao", "joao"
            var result = SearchTokenizer.Tokenize("S\u00e3o Jo\u00e3o");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("sao", result[0]);
            Assert.AreEqual("joao", result[1]);
        }

        [Test]
        public void MultipleWords_SplitsOnWhitespace()
        {
            var result = SearchTokenizer.Tokenize("hello world");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("hello", result[0]);
            Assert.AreEqual("world", result[1]);
        }

        [Test]
        public void Punctuation_Stripped()
        {
            var result = SearchTokenizer.Tokenize("hello,world");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("hello", result[0]);
            Assert.AreEqual("world", result[1]);
        }

        [Test]
        public void DeduplicatesTokens()
        {
            var result = SearchTokenizer.Tokenize("hello hello");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("hello", result[0]);
        }

        [Test]
        public void Case_InsensitiveNormalized()
        {
            var result = SearchTokenizer.Tokenize("HeLLo");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("hello", result[0]);
        }

        [Test]
        public void NumericTokens_Preserved()
        {
            var result = SearchTokenizer.Tokenize("1755");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("1755", result[0]);
        }

        [Test]
        public void ConsecutiveSpaces_NoEmpty()
        {
            var result = SearchTokenizer.Tokenize("  hello   world  ");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("hello", result[0]);
            Assert.AreEqual("world", result[1]);
        }

        [Test]
        public void SingleCharacterToken_Preserved()
        {
            var result = SearchTokenizer.Tokenize("x");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("x", result[0]);
        }

        [Test]
        public void IsTokenValid_Null_ReturnsFalse()
        {
            Assert.IsFalse(SearchTokenizer.IsTokenValid(null));
        }

        [Test]
        public void IsTokenValid_Empty_ReturnsFalse()
        {
            Assert.IsFalse(SearchTokenizer.IsTokenValid(""));
        }

        [Test]
        public void IsTokenValid_WhitespaceOnly_ReturnsFalse()
        {
            Assert.IsFalse(SearchTokenizer.IsTokenValid("   "));
        }

        [Test]
        public void IsTokenValid_ValidToken_ReturnsTrue()
        {
            Assert.IsTrue(SearchTokenizer.IsTokenValid("hello"));
        }
    }
}
