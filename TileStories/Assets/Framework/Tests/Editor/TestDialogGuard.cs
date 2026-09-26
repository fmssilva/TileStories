using NUnit.Framework;
using TileStories.Editor;

// Runs once around EVERY test of this EditMode assembly. It sits outside any namespace on
// purpose: an NUnit [SetUpFixture] only covers fixtures in its own namespace and below, and
// this assembly has two (TileStories.Tests and TileStories.Editor.Tests) -- inside one of them
// it silently left the other unguarded.
//  - A test cannot click a modal dialog (it hangs the run): every EditorDecision question asked
//    by code under test gets a Cancel from here instead of a real dialog. A test that needs a
//    different answer sets EditorDecision.Responder itself and restores the guard's afterwards.
//  - Notices queued by code under test stay in the queue (observable) instead of opening popups.
[SetUpFixture]
public class TestDialogGuard
{
    // Every question asked without a test's own responder is answered Cancel: the safe default
    internal static DecisionAnswer CancelEverything(DecisionRequest request) => DecisionAnswer.Cancel;

    private bool _previousShowPopups;

    [OneTimeSetUp]
    public void DisableBlockingAndStrayPopups()
    {
        _previousShowPopups = EditorNotice.ShowPopups;
        EditorNotice.ShowPopups = false;
        EditorDecision.Responder = CancelEverything;
    }

    [OneTimeTearDown]
    public void Restore()
    {
        EditorNotice.Clear();
        EditorNotice.ShowPopups = _previousShowPopups;
        EditorDecision.Responder = null;
    }
}
