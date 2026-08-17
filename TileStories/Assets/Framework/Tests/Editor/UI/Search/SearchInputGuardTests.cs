using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 tests for the SearchInputGuard input policy extracted from
    // SearchOverlayView (spec _2.6 section 4 / §9). Stateless; no scene needed.
    public class SearchInputGuardTests
    {
        [Test]
        public void ShouldSubmit_ExplicitMode_NonEmptyQuery_ReturnsTrue()
        {
            Assert.IsTrue(SearchInputGuard.ShouldSubmit("cathedral", "explicit", 0f));
        }

        [Test]
        public void ShouldSubmit_ExplicitMode_AlwaysTrue() =>
            Assert.IsTrue(SearchInputGuard.ShouldSubmit("x", "explicit", 999f));

        [Test]
        public void ShouldSubmit_DynamicMode_BeforeDebounce_ReturnsFalse() =>
            Assert.IsFalse(SearchInputGuard.ShouldSubmit("cat", "dynamic", 0.05f));

        [Test]
        public void ShouldSubmit_DynamicMode_AfterDebounce_ReturnsTrue() =>
            Assert.IsTrue(SearchInputGuard.ShouldSubmit("cat", "dynamic", 0.2f));

        [Test]
        public void ShouldSubmit_EmptyOrNullWhitespace_ReturnsFalse()
        {
            Assert.IsFalse(SearchInputGuard.ShouldSubmit("", "explicit", 0f));
            Assert.IsFalse(SearchInputGuard.ShouldSubmit(null, "dynamic", 1f));
            Assert.IsFalse(SearchInputGuard.ShouldSubmit("   ", "dynamic", 1f));
        }

        [Test]
        public void IsMicVisible_VoiceDisabled_ReturnsFalse() =>
            Assert.IsFalse(SearchInputGuard.IsMicVisible(false, true));

        [Test]
        public void IsMicVisible_TranscriberUnsupported_ReturnsFalse() =>
            Assert.IsFalse(SearchInputGuard.IsMicVisible(true, false));

        [Test]
        public void IsMicVisible_BothTrue_ReturnsTrue() =>
            Assert.IsTrue(SearchInputGuard.IsMicVisible(true, true));
    }
}
