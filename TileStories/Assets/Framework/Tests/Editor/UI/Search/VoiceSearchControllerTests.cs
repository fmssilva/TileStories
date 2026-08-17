using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 tests for VoiceSearchController: enabled-gating, the
    // transcription -> shared-search-pipeline hand-off, and match-mode passthrough
    // (spec _2.6 section 12). Uses DebugTranscriber so no microphone or plugin is
    // required.
    public class VoiceSearchControllerTests
    {
        private WallConfigData _enabledConfig;
        private DebugTranscriber _transcriber;
        private string _lastSubmittedQuery;
        private SearchMatchMode _lastMode;
        private int _submitCount;

        [SetUp]
        public void SetUp()
        {
            _enabledConfig = new WallConfigData
            {
                voice_search_enabled = true,
                voice_search_match_mode = "all",
            };
            _transcriber = new DebugTranscriber { PresetTranscript = "cathedral" };
            _lastSubmittedQuery = null;
            _lastMode = SearchMatchMode.Any;
            _submitCount = 0;
        }

        private VoiceSearchController MakeController()
        {
            return new VoiceSearchController(_enabledConfig, _transcriber, Submit);
        }

        private void Submit(string query, SearchMatchMode mode)
        {
            _lastSubmittedQuery = query;
            _lastMode = mode;
            _submitCount++;
        }

        [Test]
        public void IsAvailable_FalseWhenDisabled()
        {
            _enabledConfig.voice_search_enabled = false;
            var controller = MakeController();
            // DebugTranscriber.IsSupported is true, so this depends purely on config.
            Assert.IsFalse(controller.IsAvailable);
        }

        [Test]
        public void StartVoiceSearch_Disabled_DoesNotTransactOrSearch()
        {
            _enabledConfig.voice_search_enabled = false;
            var controller = MakeController();
            controller.StartVoiceSearch();

            Assert.AreEqual(VoiceSearchState.Idle, controller.State);
            Assert.AreEqual(0, _submitCount);
        }

        [Test]
        public void StartVoiceSearch_Enabled_SubmitsTranscript_WithAllMode()
        {
            var controller = MakeController();
            controller.StartVoiceSearch();

            Assert.AreEqual(VoiceSearchState.Result, controller.State);
            Assert.AreEqual(1, _submitCount);
            Assert.AreEqual("cathedral", _lastSubmittedQuery);
            Assert.AreEqual(SearchMatchMode.All, _lastMode);
        }

        [Test]
        public void StartVoiceSearch_AnyMode_SubmitsWithAny()
        {
            _enabledConfig.voice_search_match_mode = "any";
            var controller = MakeController();
            controller.StartVoiceSearch();

            Assert.AreEqual(SearchMatchMode.Any, _lastMode);
            Assert.AreEqual(1, _submitCount);
        }

        [Test]
        public void StartVoiceSearch_UnknownMode_DefaultsToAny()
        {
            _enabledConfig.voice_search_match_mode = "bogus";
            var controller = MakeController();
            controller.StartVoiceSearch();

            Assert.AreEqual(SearchMatchMode.Any, _lastMode);
        }

        [Test]
        public void EmptyTranscript_DoesNotSearch_ResetsToIdle()
        {
            _transcriber.PresetTranscript = "   ";
            var controller = MakeController();
            controller.StartVoiceSearch();

            Assert.AreEqual(VoiceSearchState.Idle, controller.State);
            Assert.AreEqual(0, _submitCount);
        }

        [Test]
        public void TranscriberError_TransitionsToError_AndDoesNotSearch()
        {
            _transcriber.PresetTranscript = null; // avoid auto-result
            var controller = MakeController();
            _transcriber.SimulateError("permission_denied");

            Assert.AreEqual(VoiceSearchState.Error, controller.State);
            Assert.AreEqual(0, _submitCount);
        }

        [Test]
        public void StateChanges_AreForwardedThroughController()
        {
            var controller = MakeController();
            var seen = new List<VoiceSearchState>();
            controller.StateChanged += s => seen.Add(s);

            controller.StartVoiceSearch();

            // DebugTranscriber.StartListening synchronously fires OnResult, so the
            // whole idle->listening->processing->result sequence runs in one call.
            Assert.IsTrue(seen.Contains(VoiceSearchState.Listening));
            Assert.IsTrue(seen.Contains(VoiceSearchState.Processing));
            Assert.IsTrue(seen.Contains(VoiceSearchState.Result));
        }
    }
}
