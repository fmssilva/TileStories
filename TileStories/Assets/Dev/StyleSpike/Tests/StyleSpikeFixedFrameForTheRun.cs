using System.IO;
using NUnit.Framework;
using TileStories.Tests;

// No namespace on purpose: wraps every test of the style spike's PlayMode assembly in the Framework's fixed 390 x 844 frame
// (an NUnit SetUpFixture only covers its own assembly; same as LivingRoomFixedFrameForTheRun).
// The spike is a measurement, not a suite member (_3.3 R8: what it proved lives in UssVariableChainTests and R8's table; its fixtures time
// themselves in real time): the whole assembly runs only while the local switch file TestEvidence/StyleSpike/MEASURING exists, and is
// ignored otherwise -- the full run skips it. By hand: create the file, run_tests PlayMode with
// assembly_names ["TileStories.Dev.StyleSpike.Tests"], delete the file
[SetUpFixture, RecordEachTest]
public class StyleSpikeFixedFrameForTheRun
{
    private CardTestInput.FixedGameViewSize _frame;

    // Ignore the spike unless its switch is on; else pin the Game view before the first test of the run
    [OneTimeSetUp]
    public void PinTheFrame()
    {
        if (!File.Exists(Path.Combine(TestEvidence.Root, "StyleSpike", "MEASURING")))
            Assert.Ignore("The style spike is a measurement: create TestEvidence/StyleSpike/MEASURING to run it");
        _frame = new CardTestInput.FixedGameViewSize();
    }

    // Give the Editor its own Game view size back after the last one
    [OneTimeTearDown]
    public void RestoreTheFrame()
    {
        _frame?.Dispose();
        _frame = null;
    }
}
