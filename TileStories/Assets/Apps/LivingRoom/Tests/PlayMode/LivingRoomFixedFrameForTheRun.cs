using NUnit.Framework;
using TileStories.Tests;

// No namespace on purpose: wraps every test of the LivingRoom PlayMode assembly in the Framework's fixed 390 x 844 frame and its own
// visitor-state store, exactly like the Framework's own FixedFrameForTheRun (an NUnit SetUpFixture only covers its own assembly)
[SetUpFixture, RecordEachTest]
public class LivingRoomFixedFrameForTheRun
{
    private CardTestInput.FixedGameViewSize _frame;
    private IsolatedVisitorState _visitorState;

    // Pin the Game view and isolate the visitor's saved state before the first test of the run
    [OneTimeSetUp]
    public void PinTheFrame()
    {
        _frame = new CardTestInput.FixedGameViewSize();
        _visitorState = new IsolatedVisitorState();
    }

    // Give the Editor its own Game view size and the app its own store back after the last one
    [OneTimeTearDown]
    public void RestoreTheFrame()
    {
        _frame?.Dispose();
        _frame = null;
        _visitorState?.Dispose();
        _visitorState = null;
    }
}
