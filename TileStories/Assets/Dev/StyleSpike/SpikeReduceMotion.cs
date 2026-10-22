using UnityEngine;
#if UNITY_IOS && TILESTORIES_SPIKE_IOS_REDUCE_MOTION && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace TileStories.Dev.StyleSpike
{
    // What the OS says about motion (DEV-ONLY, _3.3 P0 S8)
    public enum OsMotionPreference { NotAvailable, Full, Reduced }

    // DEV-ONLY (_3.3 P0 S8): the OS "reduce motion" read that UnityEngine.Accessibility.AccessibilitySettings does not offer (it has
    // fontScale, isBoldTextEnabled, isClosedCaptioningEnabled). Written, NOT shipped and not verified on a device:
    //   Android -- Settings.Global.ANIMATOR_DURATION_SCALE (0 = the user turned animations off) through AndroidJavaClass.
    //   iOS     -- UIAccessibilityIsReduceMotionEnabled() through Plugins/iOS/SpikeReduceMotion.mm, a native plugin whose importer is
    //              switched OFF for every platform; compiled only with the define TILESTORIES_SPIKE_IOS_REDUCE_MOTION and the plugin
    //              enabled for iOS (both off, so no build can link a missing symbol).
    //   Editor / desktop -- NotAvailable.
    public static class SpikeReduceMotion
    {
        // Ask the OS once (call again on app focus: the user can change it while the app is in the background)
        public static OsMotionPreference Read()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return FromAnimatorDurationScale(ReadAndroidAnimatorDurationScale());
#elif UNITY_IOS && TILESTORIES_SPIKE_IOS_REDUCE_MOTION && !UNITY_EDITOR
            return _TileStoriesSpikeIsReduceMotionEnabled() ? OsMotionPreference.Reduced : OsMotionPreference.Full;
#else
            return OsMotionPreference.NotAvailable;
#endif
        }

        // Pure: Android's animator duration scale -> the preference (0 = "Remove animations" / "Animation off"; any other value
        // scales durations, which is not a request to remove motion; a negative value means the read failed)
        public static OsMotionPreference FromAnimatorDurationScale(float scale)
        {
            if (scale < 0f) return OsMotionPreference.NotAvailable;
            return Mathf.Approximately(scale, 0f) ? OsMotionPreference.Reduced : OsMotionPreference.Full;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // Settings.Global.getFloat(contentResolver, "animator_duration_scale", 1f) on the current activity
        private static float ReadAndroidAnimatorDurationScale()
        {
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var resolver = activity.Call<AndroidJavaObject>("getContentResolver");
                using var global = new AndroidJavaClass("android.provider.Settings$Global");
                return global.CallStatic<float>("getFloat", resolver, "animator_duration_scale", 1f);
            }
            catch (AndroidJavaException e)
            {
                Debug.LogWarning("[StyleSpike] reduce motion: the Android read failed: " + e.Message);
                return -1f;
            }
        }
#endif

#if UNITY_IOS && TILESTORIES_SPIKE_IOS_REDUCE_MOTION && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool _TileStoriesSpikeIsReduceMotionEnabled();
#endif
    }
}
