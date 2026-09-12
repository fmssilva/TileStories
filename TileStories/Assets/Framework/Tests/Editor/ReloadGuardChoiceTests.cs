using NUnit.Framework;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tier-0 tests for the Load & Populate Rig safety guards: the pure
    // decision helpers that map each blocking dialog's result to an action.
    // The dialogs themselves (EditorUtility.DisplayDialogComplex) cannot run
    // headless, so the helpers carry the whole decision table and are tested
    // directly. Real-code, real assertions: no mock UI involved.
    public class ReloadGuardChoiceTests
    {
        [Test]
        public void UncapturedRig_NoConfig_NoDialog()
        {
            Assert.IsNull(POIAuthoringToolWindow.ResolveUncapturedRigChoice(
                false, true, false, 0));
        }

        [Test]
        public void UncapturedRig_EmptyRig_NoDialog()
        {
            Assert.IsNull(POIAuthoringToolWindow.ResolveUncapturedRigChoice(
                true, false, false, 0));
        }

        [Test]
        public void UncapturedRig_InSync_NoDialog()
        {
            Assert.IsNull(POIAuthoringToolWindow.ResolveUncapturedRigChoice(
                true, true, true, 0));
        }

        [Test]
        public void UncapturedRig_Button0_CapturesThenReloads()
        {
            Assert.AreEqual(
                POIAuthoringToolWindow.ReloadGuardChoice.ProceedWithSaveOrCapture,
                POIAuthoringToolWindow.ResolveUncapturedRigChoice(true, true, false, 0));
        }

        [Test]
        public void UncapturedRig_Button1_DiscardsAndReloads()
        {
            Assert.AreEqual(
                POIAuthoringToolWindow.ReloadGuardChoice.DiscardAndReload,
                POIAuthoringToolWindow.ResolveUncapturedRigChoice(true, true, false, 1));
        }

        [Test]
        public void UncapturedRig_Cancel_StaysPut()
        {
            Assert.AreEqual(
                POIAuthoringToolWindow.ReloadGuardChoice.Cancel,
                POIAuthoringToolWindow.ResolveUncapturedRigChoice(true, true, false, 2));
        }

        [Test]
        public void UnsavedConfig_CleanState_NoDialog()
        {
            Assert.IsNull(POIAuthoringToolWindow.ResolveUnsavedConfigChoice(
                true, false, 0));
        }

        [Test]
        public void UnsavedConfig_NoConfig_NoDialog()
        {
            Assert.IsNull(POIAuthoringToolWindow.ResolveUnsavedConfigChoice(
                false, true, 0));
        }

        [Test]
        public void UnsavedConfig_Button0_SavesThenReloads()
        {
            Assert.AreEqual(
                POIAuthoringToolWindow.ReloadGuardChoice.ProceedWithSaveOrCapture,
                POIAuthoringToolWindow.ResolveUnsavedConfigChoice(true, true, 0));
        }

        [Test]
        public void UnsavedConfig_Button1_DiscardsAndReloads()
        {
            Assert.AreEqual(
                POIAuthoringToolWindow.ReloadGuardChoice.DiscardAndReload,
                POIAuthoringToolWindow.ResolveUnsavedConfigChoice(true, true, 1));
        }

        [Test]
        public void UnsavedConfig_Cancel_StaysPut()
        {
            Assert.AreEqual(
                POIAuthoringToolWindow.ReloadGuardChoice.Cancel,
                POIAuthoringToolWindow.ResolveUnsavedConfigChoice(true, true, 2));
        }
    }
}
