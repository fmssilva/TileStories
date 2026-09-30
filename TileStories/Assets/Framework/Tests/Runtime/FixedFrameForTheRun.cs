using NUnit.Framework;
using TileStories.Tests;

// No namespace on purpose: an NUnit SetUpFixture outside any namespace wraps EVERY test of this assembly, so the whole
// run lays out in the same 390 x 844 frame (the PanelSettings reference resolution, a phone in portrait) whatever size and
// shape the Editor's Game view happens to have (_3.1 [13-fix], 40-testing 4.2.3). One place, so no fixture can forget it:
// on 2026-09-29 a Default layout's landscape Free Aspect Game view broke 11 card tests that only one fixture had pinned.
[SetUpFixture]
public class FixedFrameForTheRun
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
