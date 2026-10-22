using NUnit.Framework;
using UnityEngine.Accessibility;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0 S8): what 6.3 reads of the OS accessibility settings in the Editor, and the reduce-motion code path the
    // engine does not offer (written in SpikeReduceMotion, not shipped; the device check is owed, see _3.3 R8 S8).
    public class S8AccessibilityTests
    {
        [Test]
        public void AccessibilitySettings_ReadInTheEditor()
        {
            float scale = AccessibilitySettings.fontScale;
            bool bold = AccessibilitySettings.isBoldTextEnabled;
            bool captions = AccessibilitySettings.isClosedCaptioningEnabled;
            SpikeEvidence.Note("S8", $"Editor: AccessibilitySettings.fontScale {scale}, isBoldTextEnabled {bold}, isClosedCaptioningEnabled {captions}; " +
                $"OS reduce motion read: {SpikeReduceMotion.Read()}");
            Assert.Greater(scale, 0f, "a font scale is always reported (1 where the platform has none)");
        }

        [Test]
        public void ReduceMotion_InTheEditor_IsNotAvailable()
        {
            Assert.AreEqual(OsMotionPreference.NotAvailable, SpikeReduceMotion.Read(), "the Editor has no OS reduce-motion flag");
        }

        [TestCase(0f, OsMotionPreference.Reduced)]
        [TestCase(0.5f, OsMotionPreference.Full)]
        [TestCase(1f, OsMotionPreference.Full)]
        [TestCase(10f, OsMotionPreference.Full)]
        [TestCase(-1f, OsMotionPreference.NotAvailable)]
        public void AndroidAnimatorDurationScale_MapsToThePreference(float scale, OsMotionPreference expected)
        {
            Assert.AreEqual(expected, SpikeReduceMotion.FromAnimatorDurationScale(scale));
        }
    }
}
