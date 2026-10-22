using System;
using System.Collections;
using Unity.Profiling;
using UnityEngine;

namespace TileStories.Dev.StyleSpike
{
    // What one stretch of frames cost, averaged per frame (DEV-ONLY, _3.3 P0)
    public struct CostSample
    {
        public int Frames;
        public double Batches;
        public double SetPassCalls;
        public double DrawCalls;
        public double UpdatePanelsMs;   // UI Toolkit style + layout + animation + render data (CPU)
        public double RepaintPanelsMs;  // UI Toolkit repaint (CPU)
        public double DrawChainMs;      // UIR.DrawChain as reported -- in the Editor it holds the present / vsync wait (18 ms for one box), so it is logged, never compared
        public double GpuFrameMs;       // the whole frame on the GPU (0 where the platform reports nothing)

        // The UI Toolkit CPU work of one frame, the number variants are compared by (update + repaint; DrawChain excluded, see above)
        public double UiCpuMs => UpdatePanelsMs + RepaintPanelsMs;

        public override string ToString() =>
            $"frames {Frames}, batches {Batches:0.0}, setpass {SetPassCalls:0.0}, draw calls {DrawCalls:0.0}, UI cpu {UiCpuMs:0.000} ms " +
            $"(update {UpdatePanelsMs:0.000}, repaint {RepaintPanelsMs:0.000}, draw {DrawChainMs:0.000}), gpu frame {GpuFrameMs:0.00} ms";
    }

    // Reads the engine's own profiler counters over N frames. The main-thread frame time in the Editor is the vsync wait, so the
    // spike compares variants by the UI Toolkit markers instead (real work only). Editor numbers rank variants on one machine; the
    // absolute cost on a phone is a device measurement (S9).
    public static class SpikeCost
    {
        public const int WarmUpFrames = 10;

        // Let the rig settle, then average the render counters and the UI Toolkit markers over `frames` frames
        public static IEnumerator Measure(int frames, Action<CostSample> got)
        {
            for (int i = 0; i < WarmUpFrames; i++) yield return null;

            using var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            using var setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            using var drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            using var update =ProfilerRecorder.StartNew(new ProfilerCategory("PlayerLoop"), "PreLateUpdate.UIElementsUpdatePanels");
            using var repaint = ProfilerRecorder.StartNew(new ProfilerCategory("PlayerLoop"), "PostLateUpdate.UIElementsRepaintPanels");
            using var drawChain = ProfilerRecorder.StartNew(ProfilerCategory.Render, "UIR.DrawChain");
            using var gpu = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");

            var sample = new CostSample();
            for (int i = 0; i < frames; i++)
            {
                yield return null;
                sample.Batches += batches.LastValue;
                sample.SetPassCalls += setPass.LastValue;
                sample.DrawCalls += drawCalls.LastValue;
                sample.UpdatePanelsMs += update.LastValue * 1e-6;
                sample.RepaintPanelsMs += repaint.LastValue * 1e-6;
                sample.DrawChainMs += drawChain.LastValue * 1e-6;
                sample.GpuFrameMs += gpu.LastValue * 1e-6;
            }
            sample.Frames = frames;
            sample.Batches /= frames;
            sample.SetPassCalls /= frames;
            sample.DrawCalls /= frames;
            sample.UpdatePanelsMs /= frames;
            sample.RepaintPanelsMs /= frames;
            sample.DrawChainMs /= frames;
            sample.GpuFrameMs /= frames;
            got(sample);
        }
    }
}
