using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // DevLog: a domain's detail lines reach Unity's real log only while that domain is on, and as one line (no stack trace)
    public class DevLogTests
    {
        private LogDomain _saved;
        private readonly List<(string message, string stack)> _received = new();

        [SetUp]
        public void SetUp()
        {
            _saved = DevLog.Enabled;
            _received.Clear();
            Application.logMessageReceived += Collect;
        }

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= Collect;
            DevLog.Enabled = _saved;
        }

        private void Collect(string message, string stack, LogType type)
        {
            if (message.StartsWith("[DevLogTest]")) _received.Add((message, stack));
        }

        [Test]
        public void Detail_WritesNothing_WhenItsDomainIsOff()
        {
            DevLog.Enabled = LogDomain.None;
            DevLog.Detail(LogDomain.Wall, "[DevLogTest] off");
            DevLog.Enabled = LogDomain.Card | LogDomain.Lod;
            DevLog.Detail(LogDomain.Wall, "[DevLogTest] other domains on");
            Assert.IsEmpty(_received, "a detail line of a domain that is off never reaches the log");
            Assert.IsFalse(DevLog.IsOn(LogDomain.Wall));
        }

        [Test]
        public void Detail_WritesOneLineWithoutStackTrace_WhenItsDomainIsOn()
        {
            DevLog.Enabled = LogDomain.Wall | LogDomain.Card;
            DevLog.Detail(LogDomain.Wall, "[DevLogTest] wall");
            DevLog.Detail(LogDomain.Card, "[DevLogTest] card");
            Assert.AreEqual(2, _received.Count, "each ticked domain logs its line");
            Assert.AreEqual("[DevLogTest] wall", _received[0].message);
            Assert.IsTrue(string.IsNullOrEmpty(_received[0].stack), "a detail line carries no stack trace");
        }

        [Test]
        public void EveryDomainIsOffByDefault_AndHasItsOwnBit()
        {
            var seen = LogDomain.None;
            foreach (LogDomain domain in System.Enum.GetValues(typeof(LogDomain)))
            {
                if (domain == LogDomain.None) continue;
                Assert.AreEqual(0, (int)(seen & domain), domain + " shares a bit with another domain");
                seen |= domain;
            }
            Assert.AreEqual(LogDomain.None, default(LogDomain), "the default mask logs nothing");
        }
    }
}
