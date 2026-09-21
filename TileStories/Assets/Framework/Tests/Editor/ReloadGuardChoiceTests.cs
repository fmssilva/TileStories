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
        public void UncapturedRig_Button2_DiscardsAndReloads()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.DiscardAndReload,
                POIEditorToolWindow.ResolveUncapturedRigChoice(true, true, false, 2));
        }

        // Unity returns 1 for the Cancel button AND for Esc / closing the window, so slot 1
        // must be the safe cancel: pressing Esc must never discard the developer's work.
        [Test]
        public void UncapturedRig_Button1_OrEsc_StaysPutAndDiscardsNothing()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.Cancel,
                POIEditorToolWindow.ResolveUncapturedRigChoice(true, true, false, 1));
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.Cancel,
                POIEditorToolWindow.ResolveUncapturedRigChoice(true, true, false, -1), "Anything unexpected cancels.");
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
        public void UnsavedConfig_Button2_DiscardsAndReloads()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.DiscardAndReload,
                POIEditorToolWindow.ResolveUnsavedConfigChoice(true, true, 2));
        }

        [Test]
        public void UnsavedConfig_Button1_OrEsc_StaysPutAndDiscardsNothing()
        {
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.Cancel,
                POIEditorToolWindow.ResolveUnsavedConfigChoice(true, true, 1));
            Assert.AreEqual(
                POIEditorToolWindow.ReloadGuardChoice.Cancel,
                POIEditorToolWindow.ResolveUnsavedConfigChoice(true, true, -1), "Anything unexpected cancels.");
        }

        // ---- rig safety prompt on Play / Build ----

        [Test]
        public void RigSafety_HasNoOptOut_NoSettingAndNoMenuItem()
        {
            // The old "Rig Safety Prompt on Play/Build" toggle let a build skip the prompt and
            // ship the editor rig's markers. It (and its EditorPrefs key) must stay gone.
            const System.Reflection.BindingFlags all = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            Assert.IsNull(typeof(POIEditorToolWindow).GetField("SkipPromptPrefKey", all), "The skip setting must not exist.");

            foreach (var method in typeof(POIEditorToolWindow).GetMethods(all))
                foreach (var attribute in method.GetCustomAttributes(typeof(UnityEditor.MenuItem), false))
                    StringAssert.DoesNotContain("Rig Safety", ((UnityEditor.MenuItem)attribute).menuItem, method.Name);
        }

        [Test]
        public void RigSafety_WithNoRigInTheScene_NeverPrompts_ForPlayOrBuild()
        {
            Assert.IsNull(UnityEngine.GameObject.Find("POIEditorRig"), "Premise: no rig in the scene.");
            Assert.IsTrue(POIEditorToolWindow.PromptBeforePlayOrBuild(false));
            Assert.IsTrue(POIEditorToolWindow.PromptBeforePlayOrBuild(true));
        }

        [Test]
        public void RigSafety_Play_MapsEverySlot_AndEscCancels()
        {
            Assert.AreEqual(POIEditorToolWindow.RigSafetyAction.SaveClearAndContinue, POIEditorToolWindow.ResolveRigSafetyChoice(false, 0));
            Assert.AreEqual(POIEditorToolWindow.RigSafetyAction.Cancel, POIEditorToolWindow.ResolveRigSafetyChoice(false, 1),
                "Slot 1 is what Esc / X returns: it must cancel, never start Play with duplicate markers.");
            Assert.AreEqual(POIEditorToolWindow.RigSafetyAction.ContinueWithoutClearing, POIEditorToolWindow.ResolveRigSafetyChoice(false, 2));
        }

        [Test]
        public void RigSafety_Build_HasNoContinueOption_AndEscCancels()
        {
            Assert.AreEqual(POIEditorToolWindow.RigSafetyAction.SaveClearAndContinue, POIEditorToolWindow.ResolveRigSafetyChoice(true, 0));
            Assert.AreEqual(POIEditorToolWindow.RigSafetyAction.Cancel, POIEditorToolWindow.ResolveRigSafetyChoice(true, 1));
            Assert.AreEqual(POIEditorToolWindow.RigSafetyAction.Cancel, POIEditorToolWindow.ResolveRigSafetyChoice(true, 2),
                "A build is visitor-facing: a stray slot 2 must never let it continue uncleared.");
        }
    }
}
