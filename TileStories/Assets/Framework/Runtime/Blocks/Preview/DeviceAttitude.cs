using UnityEngine;
using UnityEngine.InputSystem;

namespace TileStories
{
    // The phone's attitude sensor through the Input System package (_3.1 step 10A.4.2), the one place that touches it: a gyro panorama
    // asks Available, calls Begin() when it starts following and End() when it stops, and reads TryRead() on its own tick. Where no
    // sensor exists (the Editor, a tablet without one) Available is false and the panorama falls back to drag. Sensors draw power only
    // while enabled, so the device is switched on by the first Begin and off by the last End (several panoramas can be bound at once:
    // the card's inline block and its full-screen page).
    public static class DeviceAttitude
    {
        // A real attitude is a unit quaternion (squared length 1); the zero state of a sensor with no event yet is 0
        private const float NoReadingBelowSqrMagnitude = 0.5f;

        private static int _users;

        // A sensor is present right now (the Input System's current one)
        public static bool Available => AttitudeSensor.current != null;

        public static int Users => _users;

        // A view started following the sensor
        public static void Begin()
        {
            _users++;
            var sensor = AttitudeSensor.current;
            if (sensor != null && !sensor.enabled) InputSystem.EnableDevice(sensor);
        }

        // A view stopped following it; the last one switches the sensor off
        public static void End()
        {
            if (_users == 0) return;
            _users--;
            var sensor = AttitudeSensor.current;
            if (_users == 0 && sensor != null && sensor.enabled) InputSystem.DisableDevice(sensor);
        }

        // The sensor's attitude now, false when there is no sensor or it has not delivered a reading yet. A sensor that appeared after Begin (a device plugged in later) is
        // switched on here, while someone is following it
        public static bool TryRead(out Quaternion attitude)
        {
            attitude = Quaternion.identity;
            var sensor = AttitudeSensor.current;
            if (sensor == null) return false;
            if (_users > 0 && !sensor.enabled) InputSystem.EnableDevice(sensor);
            attitude = sensor.attitude.ReadValue();
            // - a sensor that was just switched on holds an all-zero quaternion until its first event: no reading yet, not "facing the
            // sensor's zero" (the gyro anchor is the FIRST real reading)
            return attitude.x * attitude.x + attitude.y * attitude.y + attitude.z * attitude.z + attitude.w * attitude.w >= NoReadingBelowSqrMagnitude;
        }
    }
}
