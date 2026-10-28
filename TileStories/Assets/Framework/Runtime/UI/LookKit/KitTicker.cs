using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.LookKit
{
    // A clock that ticks an action every frame while running, on the element's own panel (made fresh on each Start, never kept across a
    // re-host); the time is the injectable Clock's (tests hand their own). What the Reveal and the Drift run on
    public sealed class KitTicker
    {
        private readonly VisualElement _host;
        private readonly System.Action<float> _tick;
        private IVisualElementScheduledItem _item;
        private double _start;

        public System.Func<double> Clock = () => Time.realtimeSinceStartupAsDouble;
        public bool Running => _item != null;
        public float Elapsed => Running ? (float)(Clock() - _start) : 0f;

        public KitTicker(VisualElement host, System.Action<float> tick)
        {
            _host = host;
            _tick = tick;
            _host.RegisterCallback<AttachToPanelEvent>(_ => { if (_item != null) Schedule(); });
        }

        // Start from 0 now; the action is called at once and every frame after
        public void Start()
        {
            _start = Clock();
            Schedule();
            _tick(0f);
        }

        public void Stop()
        {
            _item?.Pause();
            _item = null;
        }

        private void Schedule()
        {
            _item?.Pause();
            _item = _host.schedule.Execute(() => _tick(Elapsed)).Every(16);
        }
    }
}
