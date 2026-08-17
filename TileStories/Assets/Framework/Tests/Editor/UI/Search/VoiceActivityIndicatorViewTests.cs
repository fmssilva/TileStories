using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for VoiceActivityIndicatorView: style parsing,
    // mic-label selection, and listen-bar visibility policy. This is a plain C#
    // class (no MonoBehaviour) so every path is exercised directly with new().
    // (spec _2.6 section 12)
    public class VoiceActivityIndicatorViewTests
    {
        [Test]
        public void ParseStyle_MicText_ReturnsMicText()
        {
            Assert.AreEqual(VoiceActivityIndicatorView.IndicatorStyle.MicText,
                VoiceActivityIndicatorView.ParseStyle("mic_text"));
        }

        [Test]
        public void ParseStyle_EmptyString_ReturnsMicText()
        {
            Assert.AreEqual(VoiceActivityIndicatorView.IndicatorStyle.MicText,
                VoiceActivityIndicatorView.ParseStyle(""));
        }

        [Test]
        public void ParseStyle_NullString_ReturnsMicText()
        {
            Assert.AreEqual(VoiceActivityIndicatorView.IndicatorStyle.MicText,
                VoiceActivityIndicatorView.ParseStyle(null));
        }

        [Test]
        public void ParseStyle_ListenBar_ReturnsListenBar()
        {
            Assert.AreEqual(VoiceActivityIndicatorView.IndicatorStyle.ListenBar,
                VoiceActivityIndicatorView.ParseStyle("listen_bar"));
        }

        [Test]
        public void ParseStyle_UnknownValue_ReturnsMicText()
        {
            Assert.AreEqual(VoiceActivityIndicatorView.IndicatorStyle.MicText,
                VoiceActivityIndicatorView.ParseStyle("bogus"));
        }

        [Test]
        public void IsVoiceActive_Listening_ReturnsTrue()
        {
            Assert.IsTrue(VoiceActivityIndicatorView.IsVoiceActive(VoiceSearchState.Listening));
        }

        [Test]
        public void IsVoiceActive_Processing_ReturnsTrue()
        {
            Assert.IsTrue(VoiceActivityIndicatorView.IsVoiceActive(VoiceSearchState.Processing));
        }

        [Test]
        public void IsVoiceActive_Idle_ReturnsFalse()
        {
            Assert.IsFalse(VoiceActivityIndicatorView.IsVoiceActive(VoiceSearchState.Idle));
        }

        [Test]
        public void IsVoiceActive_Result_ReturnsFalse()
        {
            Assert.IsFalse(VoiceActivityIndicatorView.IsVoiceActive(VoiceSearchState.Result));
        }

        [Test]
        public void IsVoiceActive_Error_ReturnsFalse()
        {
            Assert.IsFalse(VoiceActivityIndicatorView.IsVoiceActive(VoiceSearchState.Error));
        }

        [Test]
        public void MicLabelForState_Idle_ReturnsMic()
        {
            var indicator = new VoiceActivityIndicatorView("mic_text");
            Assert.AreEqual("Mic", indicator.MicLabelForState(VoiceSearchState.Idle));
        }

        [Test]
        public void MicLabelForState_Result_ReturnsMic()
        {
            var indicator = new VoiceActivityIndicatorView("mic_text");
            Assert.AreEqual("Mic", indicator.MicLabelForState(VoiceSearchState.Result));
        }

        [Test]
        public void MicLabelForState_Listening_ReturnsEllipsis()
        {
            var indicator = new VoiceActivityIndicatorView("mic_text");
            Assert.AreEqual("...", indicator.MicLabelForState(VoiceSearchState.Listening));
        }

        [Test]
        public void MicLabelForState_Processing_ReturnsEllipsis()
        {
            var indicator = new VoiceActivityIndicatorView("mic_text");
            Assert.AreEqual("...", indicator.MicLabelForState(VoiceSearchState.Processing));
        }

        [Test]
        public void MicLabelForState_Error_ReturnsMic()
        {
            var indicator = new VoiceActivityIndicatorView("mic_text");
            Assert.AreEqual("Mic", indicator.MicLabelForState(VoiceSearchState.Error));
        }

        [Test]
        public void IsBarVisible_Listening_ReturnsTrue()
        {
            var indicator = new VoiceActivityIndicatorView("listen_bar");
            Assert.IsTrue(indicator.IsBarVisible(VoiceSearchState.Listening));
        }

        [Test]
        public void IsBarVisible_Processing_ReturnsTrue()
        {
            var indicator = new VoiceActivityIndicatorView("listen_bar");
            Assert.IsTrue(indicator.IsBarVisible(VoiceSearchState.Processing));
        }

        [Test]
        public void IsBarVisible_Idle_ReturnsFalse()
        {
            var indicator = new VoiceActivityIndicatorView("listen_bar");
            Assert.IsFalse(indicator.IsBarVisible(VoiceSearchState.Idle));
        }

        [Test]
        public void IsBarVisible_Error_ReturnsFalse()
        {
            var indicator = new VoiceActivityIndicatorView("listen_bar");
            Assert.IsFalse(indicator.IsBarVisible(VoiceSearchState.Error));
        }

        [Test]
        public void Style_NullConfigString_ConstructionDefaultsToMicText()
        {
            var indicator = new VoiceActivityIndicatorView(null);
            Assert.AreEqual(VoiceActivityIndicatorView.IndicatorStyle.MicText, indicator.Style);
        }

        [Test]
        public void StyleNames_ContainsBothStyleStrings()
        {
            Assert.AreEqual(2, VoiceActivityIndicatorView.StyleNames.Length);
            Assert.Contains(VoiceActivityIndicatorView.MicTextStyleName, VoiceActivityIndicatorView.StyleNames);
            Assert.Contains(VoiceActivityIndicatorView.ListenBarStyleName, VoiceActivityIndicatorView.StyleNames);
        }

        [Test]
        public void ListenBarLabel_IsNonEmptyAndAccessible()
        {
            Assert.IsFalse(string.IsNullOrEmpty(VoiceActivityIndicatorView.ListenBarLabel));
        }
    }
}
