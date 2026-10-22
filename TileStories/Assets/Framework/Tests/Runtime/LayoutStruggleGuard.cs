using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Turns UI Toolkit's "Layout update is struggling" error into a failure of the test whose scene logged it (_3.2 audit F3-1, chunk G).
    // It listens from the scene load in SetUp to the end of the unload in TearDown -- the fixtures' own error collectors let go BEFORE the
    // unload, which is where the audit's 17 lines fell, so the run went green -- and the failure names the test, the scene load and the
    // phase each line was heard in. Public: the app's PlayMode fixtures use it too.
    public sealed class LayoutStruggleGuard
    {
        // The start of the line UI Toolkit logs (Debug.LogError, from its layout updater) when a panel's layout keeps changing frame after frame
        public const string Struggling = "Layout update is struggling";

        // Scene loads guarded in this Editor session: the number the failure gives, to find that load in Editor.log
        private static int _loads;

        private readonly List<string> _heard = new();
        private string _firstLine;
        private string _test;
        private string _scene;
        private string _phase;
        private int _load;
        private bool _listening;

        public bool Listening => _listening;

        // Start listening as `scene` loads for the test running now
        public void Begin(string scene)
        {
            Stop();
            _heard.Clear();
            _firstLine = null;
            _test = TestContext.CurrentContext.Test.FullName;
            _scene = scene;
            _load = ++_loads;
            _phase = "SetUp, while the scene loads";
            Application.logMessageReceived += Heard;
            _listening = true;
        }

        // Name the stretch the next lines belong to (the test itself, the TearDown's unload)
        public void Phase(string phase) => _phase = phase;

        // Stop listening, then fail with every line heard since Begin (the fixtures' TearDown calls it last)
        public void End()
        {
            string failure = Finish();
            if (failure != null) Assert.Fail(failure);
        }

        // Stop listening and say what was heard since Begin: the failure's words, or null when nothing was (or the guard was not listening)
        public string Finish()
        {
            if (!_listening) return null;
            Stop();
            if (_heard.Count == 0) return null;
            return _test + ": Unity logged \"" + Struggling + "\" " + _heard.Count + " time(s) on scene load #" + _load + " (" + _scene + ") -- "
                + string.Join(", ", _heard) + ". First line: " + _firstLine;
        }

        private void Stop()
        {
            if (_listening) Application.logMessageReceived -= Heard;
            _listening = false;
        }

        private void Heard(string message, string stack, LogType type)
        {
            if (message == null || !message.StartsWith(Struggling)) return;
            _firstLine ??= message;
            _heard.Add(type + " in " + _phase);
        }
    }
}
