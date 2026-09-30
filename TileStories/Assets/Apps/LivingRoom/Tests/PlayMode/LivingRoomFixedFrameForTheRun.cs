using NUnit.Framework;
using TileStories.Tests;

// No namespace on purpose: wraps every test of the LivingRoom PlayMode assembly in the Framework's fixed 390 x 844 frame,
// exactly like the Framework's own FixedFrameForTheRun (an NUnit SetUpFixture only covers its own assembly)
[SetUpFixture]
public class LivingRoomFixedFrameForTheRun
{
    private CardTestInput.FixedGameViewSize _frame;

    // Pin the Game view before the first test of the run
    [OneTimeSetUp]
    public void PinTheFrame() => _frame = new CardTestInput.FixedGameViewSize();

    // Give the Editor its own Game view size back after the last one
    [OneTimeTearDown]
    public void RestoreTheFrame()
    {
        _frame?.Dispose();
        _frame = null;
    }
}
