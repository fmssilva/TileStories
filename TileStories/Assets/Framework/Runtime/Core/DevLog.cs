using UnityEngine;

namespace TileStories
{
    // The domains a detail (per-item / per-frame) log line can belong to. One bit each, so several can be on at once.
    // Add a domain here when a new area needs its own detail logs; this enum is the one list of them.
    [System.Flags]
    public enum LogDomain
    {
        None = 0,
        Wall = 1 << 0,      // WallSession bootstrap: config load, spawn, per-POI placement
        Markers = 1 << 1,   // uGUI world-space markers: symbol, badge, outline, label, effects, orientation
        Lod = 1 << 2,       // LOD, clustering, displacement
        Search = 1 << 3,    // search, filters, selection
        Card = 1 << 4,      // UI Toolkit POI Detail Card and its blocks, media owners
        Tracking = 1 << 5,  // localisation, AR zoom
    }

    // Detail logs a developer switches on per domain (TileStories > Detail Logs menu in the Editor, remembered per machine).
    // All domains are OFF by default -- test runs and device builds log only the one-line summaries, warnings and errors,
    // which stay plain Debug.Log / LogWarning / LogError and are never gated here (_20-code-quality 2.1).
    public static class DevLog
    {
        // The domains whose detail lines are written; set by the Editor menu, None everywhere else
        public static LogDomain Enabled = LogDomain.None;

        // Whether a domain's detail lines are on (lets a caller skip building an expensive message)
        public static bool IsOn(LogDomain domain) => (Enabled & domain) != 0;

        // Write one detail line when its domain is on; no stack trace, so a line stays one line in Editor.log
        public static void Detail(LogDomain domain, string message)
        {
            if (!IsOn(domain)) return;
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", message);
        }
    }
}
