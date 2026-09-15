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
            Assert.IsNull(POIEditorToolWindow.ResolveUncapturedRigChoice(
                false, true, false, 0));
        }

        [Test]
        public void UncapturedRig_EmptyRig_NoDialog()
        {
            Assert.IsNull(POIEditorToolWindow.ResolveUncapturedRigChoice(
                true, false, false, 0));
        }

        [Test]
        public void UncapturedRig_InSync_NoDialog()
        {
            Assert.IsNull(POIEditorToolWindow.ResolveUncapturedRigChoice(
                true, true, true, 0));
        }

        [Test]
        public void UncapturedRig_Button0_CapturesThenReloads()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.ProceedWithSaveOrCapture,
                POIEditorToolWindow.ResolveUncapturedRigChoice(true, true, false, 0));
        }

        [Test]
        public void UncapturedRig_Button1_DiscardsAndReloads()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.DiscardAndReload,
                POIEditorToolWindow.ResolveUncapturedRigChoice(true, true, false, 1));
        }

        [Test]
        public void UncapturedRig_Cancel_StaysPut()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.Cancel,
                POIEditorToolWindow.ResolveUncapturedRigChoice(true, true, false, 2));
        }

        [Test]
        public void UnsavedConfig_CleanState_NoDialog()
        {
            Assert.IsNull(POIEditorToolWindow.ResolveUnsavedConfigChoice(
                true, false, 0));
        }

        [Test]
        public void UnsavedConfig_NoConfig_NoDialog()
        {
            Assert.IsNull(POIEditorToolWindow.ResolveUnsavedConfigChoice(
                false, true, 0));
        }

        [Test]
        public void UnsavedConfig_Button0_SavesThenReloads()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.ProceedWithSaveOrCapture,
                POIEditorToolWindow.ResolveUnsavedConfigChoice(true, true, 0));
        }

        [Test]
        public void UnsavedConfig_Button1_DiscardsAndReloads()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.DiscardAndReload,
                POIEditorToolWindow.ResolveUnsavedConfigChoice(true, true, 1));
        }

        [Test]
        public void UnsavedConfig_Cancel_StaysPut()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.Cancel,
                POIEditorToolWindow.ResolveUnsavedConfigChoice(true, true, 2));
        }
    }
}
