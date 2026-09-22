using System.Collections.Generic;
using UnityEngine;

namespace TileStories.Editor
{
    // Decides WHICH domains a config push has to reach: remembers each applier's last fingerprint per
    // running wall and calls only the appliers whose settings changed. Plain C#, no scene needed.
    public class LivePlayModeConfigDispatcher
    {
        private readonly List<ILivePlayModeApplier> _appliers;
        private readonly Dictionary<string, string> _lastFingerprints = new();
        private WallSession _lastSession;

        public LivePlayModeConfigDispatcher(IEnumerable<ILivePlayModeApplier> appliers)
        {
            _appliers = new List<ILivePlayModeApplier>(appliers);
        }

        // Push the authoring config to a running wall; returns the names of the domains re-applied.
        public List<string> Push(WallSession session, WallConfigData authoringConfig)
        {
            var applied = new List<string>();
            if (session == null || authoringConfig == null) return applied;

            // A new wall (new Play Mode run) knows nothing of earlier pushes: apply every domain once
            if (!ReferenceEquals(session, _lastSession))
            {
                _lastFingerprints.Clear();
                _lastSession = session;
            }

            WallConfigData copy = null;
            foreach (var applier in _appliers)
            {
                string fingerprint = applier.Fingerprint(authoringConfig);
                if (_lastFingerprints.TryGetValue(applier.Name, out var previous) && previous == fingerprint)
                    continue;

                // one private copy per push, made only when something actually changed
                copy ??= JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(authoringConfig));
                applier.Apply(session, copy);
                _lastFingerprints[applier.Name] = fingerprint;
                applied.Add(applier.Name);
            }
            return applied;
        }
    }
}
