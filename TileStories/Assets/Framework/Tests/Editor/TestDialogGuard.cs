using NUnit.Framework;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Runs once around every test in this assembly's TileStories.Tests namespace. A test
    // cannot click a modal dialog, so a notice the code under test queues must never open a
    // real one (it would hang the whole run). The queue itself stays fully observable.
    [SetUpFixture]
    public class TestDialogGuard
    {
        private bool _previous;

        [OneTimeSetUp]
        public void DisableNoticeDialogs()
        {
            _previous = EditorNotice.ShowDialogs;
            EditorNotice.ShowDialogs = false;
        }

        [OneTimeTearDown]
        public void RestoreNoticeDialogs()
        {
            EditorNotice.Clear();
            EditorNotice.ShowDialogs = _previous;
        }
    }
}
