using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 tests for VoiceSearchStateMachine: transition rules and the
    // match-mode resolution (spec _2.6 section 12). No scene required.
    public class VoiceStateMachineTests
    {
        private VoiceSearchStateMachine _sm;

        [SetUp]
        public void SetUp() => _sm = new VoiceSearchStateMachine();

        [Test]
        public void InitialState_IsIdle() => Assert.AreEqual(VoiceSearchState.Idle, _sm.State);

        [Test]
        public void BeginListening_FromIdle_GoesToListening()
        {
            _sm.BeginListening();
            Assert.AreEqual(VoiceSearchState.Listening, _sm.State);
        }

        [Test]
        public void BeginListening_WhileListening_IsNoop()
        {
            _sm.BeginListening();
            _sm.BeginListening();
            Assert.AreEqual(VoiceSearchState.Listening, _sm.State);
        }

        [Test]
        public void BeginListening_WhileProcessing_IsNoop()
        {
            _sm.BeginListening();
            _sm.OnTranscribed("cathedral");
            _sm.BeginListening(); // ignored
            Assert.AreEqual(VoiceSearchState.Processing, _sm.State);
        }

        [Test]
        public void OnTranscribed_FromListening_GoesToProcessing()
        {
            _sm.BeginListening();
            _sm.OnTranscribed("cathedral");
            Assert.AreEqual(VoiceSearchState.Processing, _sm.State);
            Assert.AreEqual("cathedral", _sm.LastTranscript);
        }

        [Test]
        public void OnTranscribed_WhenNotListening_IsIgnored()
        {
            _sm.OnTranscribed("cathedral");
            Assert.AreEqual(VoiceSearchState.Idle, _sm.State);
        }

        [Test]
        public void OnSearchSucceeded_FromProcessing_GoesToResult()
        {
            _sm.BeginListening();
            _sm.OnTranscribed("cathedral");
            _sm.OnSearchSucceeded();
            Assert.AreEqual(VoiceSearchState.Result, _sm.State);
        }

        [Test]
        public void OnSearchFailed_FromProcessing_GoesToError()
        {
            _sm.BeginListening();
            _sm.OnTranscribed("cathedral");
            _sm.OnSearchFailed("pipeline error");
            Assert.AreEqual(VoiceSearchState.Error, _sm.State);
            Assert.AreEqual("pipeline error", _sm.LastError);
        }

        [Test]
        public void OnTranscriberError_FromListening_GoesToError()
        {
            _sm.BeginListening();
            _sm.OnTranscriberError("permission_denied");
            Assert.AreEqual(VoiceSearchState.Error, _sm.State);
        }

        [Test]
        public void Reset_FromResult_GoesToIdle()
        {
            _sm.BeginListening();
            _sm.OnTranscribed("cathedral");
            _sm.OnSearchSucceeded();
            Assert.AreEqual(VoiceSearchState.Result, _sm.State);
            _sm.Reset();
            Assert.AreEqual(VoiceSearchState.Idle, _sm.State);
        }

        [Test]
        public void Reset_FromIdle_IsNoop()
        {
            _sm.Reset();
            Assert.AreEqual(VoiceSearchState.Idle, _sm.State);
        }

        [Test]
        public void StateChanged_IsFiredOnEveryTransition()
        {
            var seen = new List<VoiceSearchState>();
            _sm.StateChanged += s => seen.Add(s);

            _sm.BeginListening();
            _sm.OnTranscribed("cathedral");
            _sm.OnSearchSucceeded();

            CollectionAssert.AreEqual(
                new[] { VoiceSearchState.Listening, VoiceSearchState.Processing, VoiceSearchState.Result },
                seen);
        }

        [Test]
        public void ResolveSearchMode_NonEmpty_ReturnsParsedMode()
        {
            Assert.AreEqual(SearchMatchMode.Any, VoiceSearchStateMachine.ResolveSearchMode("cathedral", "any"));
            Assert.AreEqual(SearchMatchMode.All, VoiceSearchStateMachine.ResolveSearchMode("cathedral", "all"));
        }

        [Test]
        public void ResolveSearchMode_EmptyOrWhitespace_ReturnsNull()
        {
            Assert.IsNull(VoiceSearchStateMachine.ResolveSearchMode("", "all"));
            Assert.IsNull(VoiceSearchStateMachine.ResolveSearchMode("   ", "any"));
            Assert.IsNull(VoiceSearchStateMachine.ResolveSearchMode(null, "all"));
        }

        [Test]
        public void ParseMatchMode_UnknownDefaultsToAny()
        {
            Assert.AreEqual(SearchMatchMode.Any, VoiceSearchStateMachine.ParseMatchMode("weird"));
            Assert.AreEqual(SearchMatchMode.Any, VoiceSearchStateMachine.ParseMatchMode(""));
            Assert.AreEqual(SearchMatchMode.All, VoiceSearchStateMachine.ParseMatchMode("all"));
            Assert.AreEqual(SearchMatchMode.Any, VoiceSearchStateMachine.ParseMatchMode(null));
        }
    }
}
