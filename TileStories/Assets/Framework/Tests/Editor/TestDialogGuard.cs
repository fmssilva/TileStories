using NUnit.Framework;
using TileStories.Editor;
using TileStories.Tests;

// Runs once around EVERY test of this EditMode assembly. It sits outside any namespace on
// purpose: an NUnit [SetUpFixture] only covers fixtures in its own namespace and below, and
// this assembly has two (TileStories.Tests and TileStories.Editor.Tests) -- inside one of them
// it silently left the other unguarded.
//  - A test cannot click a modal dialog (it hangs the run): every EditorDecision question asked
//    by code under test gets a Cancel from here instead of a real dialog. A test that needs a
//    different answer sets EditorDecision.Responder itself and restores the guard's afterwards.
//  - Notices queued by code under test stay in the queue (observable) instead of opening popups.
//  - The visitor's saved state (DeviceStateStore) is a store of the run's own: no test reads or clears the developer's PlayerPrefs
//    (a test that Enters Play Mode loads the wall with it too).
//  - RecordEachTest: the top suite is where a test action reaches every test of the assembly (TestRunRecorder).
[SetUpFixture, RecordEachTest]
public class TestDialogGuard
{
    // Every question asked without a test's own responder is answered Cancel: the safe default
    internal static DecisionAnswer CancelEverything(DecisionRequest request) => DecisionAnswer.Cancel;

    private bool _previousShowPopups;
    private IsolatedVisitorState _visitorState;

    [OneTimeSetUp]
    public void DisableBlockingAndStrayPopups()
    {
        _previousShowPopups = EditorNotice.ShowPopups;
        EditorNotice.ShowPopups = false;
        EditorDecision.Responder = CancelEverything;
        _visitorState = new IsolatedVisitorState();
    }

    [OneTimeTearDown]
    public void Restore()
    {
        EditorNotice.Clear();
        EditorNotice.ShowPopups = _previousShowPopups;
        EditorDecision.Responder = null;
        _visitorState?.Dispose();
        _visitorState = null;
    }
}
