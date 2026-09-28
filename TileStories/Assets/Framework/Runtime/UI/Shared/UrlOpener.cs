using UnityEngine;

namespace TileStories
{
    // Hands a web link to the device (its browser or maps app). Behind an interface so a test can see exactly what the app
    // would open without leaving Unity (_3.1 step 7B, today_map's Directions).
    public interface IUrlOpener
    {
        void Open(string url);
    }

    // The app's opener: the one place that calls Application.OpenURL
    public sealed class ApplicationUrlOpener : IUrlOpener
    {
        public void Open(string url) => Application.OpenURL(url);
    }
}
