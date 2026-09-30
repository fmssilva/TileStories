using UnityEngine;
using UnityEngine.InputSystem;

namespace TileStories.Tests
{
    // Real input devices that keep working whichever application has the focus (40-testing 4.2.3): a test that feeds a Touchscreen, a Mouse
    // or an AttitudeSensor through the Input System uses this for the device's whole life, so a run passes while the developer is in another
    // window. It swaps in a copy of InputSystem.settings and puts the original back on Dispose. Add the device AFTER creating it: a device added
    // while the default settings are active and the Editor is in the background is switched off on the spot (InputManager.AddDevice), and
    // swapping the settings later never switches it back on.
    public sealed class BackgroundSafeInput : System.IDisposable
    {
        private readonly InputSettings _saved;
        private readonly InputSettings _copy;

        // `compensateForScreenOrientation` false reads a sensor's quaternion exactly as queued (the attitude control otherwise rotates it by the
        // screen's orientation, which an Editor Game view decides arbitrarily)
        public BackgroundSafeInput(bool compensateForScreenOrientation = true)
        {
            _saved = InputSystem.settings;
            _copy = Object.Instantiate(_saved);
            // - an unfocused Game view drops pointer input in the Editor. Play Mode only: outside it this routing drops EVERY device event
            // (an EditMode test feeding a sensor read back zeros until this was skipped)
            if (Application.isPlaying) _copy.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            // - and when the Editor is not the active application the Input System would switch a device off (backgroundBehavior's default):
            // every real-finger test then read "no touch" and failed. Keep it on.
            _copy.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            _copy.compensateForScreenOrientation = compensateForScreenOrientation;
            InputSystem.settings = _copy;
        }

        public void Dispose()
        {
            InputSystem.settings = _saved;
            if (Application.isPlaying) Object.Destroy(_copy);
            else Object.DestroyImmediate(_copy);
        }

        // A device of type T, added under these settings and switched on; the caller removes it (InputSystem.RemoveDevice) when it is done
        public static T AddEnabled<T>() where T : InputDevice
        {
            var device = InputSystem.AddDevice<T>();
            if (!device.enabled) InputSystem.EnableDevice(device);
            return device;
        }
    }
}
