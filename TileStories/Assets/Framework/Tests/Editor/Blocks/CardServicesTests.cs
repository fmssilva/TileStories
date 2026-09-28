using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The card's service lookup (_3.1 step 11): a typed registry an app adds to at startup and a block view reads through
    // BlockBindContext.Service<T>(). Absent is null, registered is the very instance, and it reaches a view through the REAL
    // BlockStackView (the seam between the two, not each on its own). The poll moved onto it: its behaviour is unchanged.
    public class CardServicesTests
    {
        private interface IProbeService { string Name { get; } }

        private sealed class ProbeService : IProbeService
        {
            public string Name { get; set; }
        }

        private sealed class OtherService { }

        private sealed class Counts : IPollResults
        {
            public IReadOnlyList<int> Votes;
            public bool TryGet(string wallId, string poiId, string blockKey, out IReadOnlyList<int> votesPerRow)
            {
                votesPerRow = Votes;
                return Votes != null;
            }
        }

        // A kind registered the way an app registers one: its view keeps what the context's Service<T>() gave it when bound
        private sealed class ServiceProbeView : IBlockView
        {
            public VisualElement Root { get; } = new();
            public IProbeService Seen;
            public int Binds;

            public void Bind(BlockInstanceData instance, BlockBindContext context)
            {
                Binds++;
                Seen = context.Service<IProbeService>();
            }

            public void Unbind() => Seen = null;
        }

        private static readonly BlockKindDefinition ProbeKind = new()
        {
            Key = "service_probe", Family = "meta", DisplayName = "Service probe", Help = "test kind",
            Variants = new[] { "plain" }, DefaultVariant = "plain", DisplayModes = new[] { CardOptions.DisplayInline },
        };

        private static List<LocalizedEntry> En(string value) => new() { new LocalizedEntry { lang = "en", value = value } };

        private static BlockInstanceData PollBlock()
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.PollKind, variant = "" };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PollQuestionField, text = En("Which?") });
            var rows = new List<BlockItemData>();
            foreach (string option in new[] { "A", "B", "C" })
            {
                var row = new BlockItemData();
                row.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.PollOptionTextField, text = En(option) });
                rows.Add(row);
            }
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PollOptionsField, items = rows });
            return block;
        }

        [Test]
        public void ANoServiceIsNull_OnTheRegistry_AndOnEveryContextThatHasNone()
        {
            var services = new CardServices();
            Assert.IsNull(services.Get<IProbeService>(), "nothing registered: null, not an exception");
            Assert.IsFalse(services.Has<IProbeService>());
            Assert.IsNull(new BlockBindContext().Service<IProbeService>(), "a context built with no registry at all");
            Assert.IsNull(new BlockBindContext { Services = services }.Service<IProbeService>(), "a registry that does not hold it");
        }

        [Test]
        public void ARegisteredService_IsTheVeryInstance_UnderTheTypeItWasAddedAs_AndOnlyThatType()
        {
            var services = new CardServices();
            var probe = new ProbeService { Name = "one" };
            var other = new OtherService();
            services.Add<IProbeService>(probe);
            services.Add(other);
            Assert.AreSame(probe, services.Get<IProbeService>());
            Assert.AreSame(probe, new BlockBindContext { Services = services }.Service<IProbeService>(), "through the context");
            Assert.AreSame(other, services.Get<OtherService>(), "two services live side by side");
            Assert.IsNull(services.Get<ProbeService>(), "asked under the concrete type it was NOT registered as: no guessing");
            Assert.IsTrue(services.Remove<IProbeService>());
            Assert.IsNull(services.Get<IProbeService>(), "removed");
            Assert.IsFalse(services.Remove<IProbeService>(), "removing what is not there says so");
            Assert.AreSame(other, services.Get<OtherService>(), "the other one stays");
        }

        [Test]
        public void ASecondRegistrationOfATypeIsRefused_ANullServiceIsRefused()
        {
            var services = new CardServices();
            services.Add<IProbeService>(new ProbeService { Name = "first" });
            Assert.Throws<ArgumentException>(() => services.Add<IProbeService>(new ProbeService { Name = "second" }), "a silent replace would hide a clash");
            Assert.AreEqual("first", services.Get<IProbeService>().Name, "the first one is kept");
            Assert.Throws<ArgumentNullException>(() => services.Add<OtherService>(null));
        }

        [Test]
        public void TheSharedRegistry_IsOneObject_AnAppFillsAtStartup()
        {
            Assert.AreSame(CardServices.Shared, CardServices.Shared);
            Assert.IsNull(CardServices.Shared.Get<ProbeService>(), "the framework registers no service of its own here");
        }

        [Test]
        public void TheStack_HandsEveryBoundViewTheCardsServices_AndAViewWithoutThemSeesNull()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            registry.Register(ProbeKind, () => new ServiceProbeView());
            var stack = new BlockStackView(new VisualElement(), new VisualElement(), registry);
            var poi = new POIData { id = "p", name = "P" };
            poi.card.blocks.Add(new BlockInstanceData { key = "block_1", kind = ProbeKind.Key });
            var entries = BlockStackBuilder.Build(poi, new CardSettings(), registry).Entries;
            var probe = new ProbeService { Name = "app" };
            var services = new CardServices();
            services.Add<IProbeService>(probe);

            stack.Bind(entries, new BlockBindContext { Poi = poi, Language = "en", FallbackLanguage = "en", Services = services });
            var view = (ServiceProbeView)stack.BoundViews.Single(v => v is ServiceProbeView);
            Assert.AreSame(probe, view.Seen, "the view got the registered instance through the real stack");

            stack.Bind(entries, new BlockBindContext { Poi = poi, Language = "en", FallbackLanguage = "en", Services = new CardServices() });
            Assert.AreSame(view, stack.BoundViews.Single(v => v is ServiceProbeView), "the pooled view is bound again");
            Assert.AreEqual(2, view.Binds);
            Assert.IsNull(view.Seen, "another card with the service absent: the view sees null");

            stack.Bind(entries, new BlockBindContext { Poi = poi, Language = "en", FallbackLanguage = "en" });
            Assert.IsNull(view.Seen, "a caller with no registry at all");
            stack.UnbindAll();
        }

        [Test]
        public void ThePoll_TakesItsResultsFromTheServiceLookup_AbsentMeansNoResults_AndTheVoteStillWorks()
        {
            var poi = new POIData { id = "p", name = "P" };
            BlockBindContext Context(CardServices services) => new()
            {
                Poi = poi, Language = "en", FallbackLanguage = "en", Variant = "bars", Services = services,
                State = new CardLocalState(new MemoryCardStateStore(), "w"),
                Strings = new CardStrings(new List<CardStringEntry> { new() { key = CardStrings.Keys.PollPercent, text = En("{0}%") } }, null, null, "en", "en"),
            };

            // - no IPollResults registered (today's app): the vote is kept and thanked, no bar, no percentage
            var poll = new PollBlockView();
            poll.Bind(PollBlock(), Context(new CardServices()));
            poll.Vote(1);
            Assert.AreEqual(1, poll.Voted);
            Assert.IsFalse(poll.ResultsShown, "absent = no results");
            Assert.IsFalse(poll.Root.Query<Label>().ToList().Any(l => l.style.display != DisplayStyle.None && l.text.Contains("%")), "no percentage without data");

            // - the explicit "no results" implementation behaves the same
            var none = new CardServices();
            none.Add<IPollResults>(new NoPollResults());
            poll.Bind(PollBlock(), Context(none));
            poll.Vote(0);
            Assert.IsFalse(poll.ResultsShown, "NoPollResults = no results");

            // - a registered IPollResults with counts: the bars come from it, after the visitor's own vote only
            var withCounts = new CardServices();
            withCounts.Add<IPollResults>(new Counts { Votes = new[] { 1, 3, 0 } });
            poll.Bind(PollBlock(), Context(withCounts));
            Assert.IsFalse(poll.ResultsShown, "results wait for the vote");
            poll.Vote(2);
            Assert.IsTrue(poll.ResultsShown, "the registered service had numbers");
            CollectionAssert.AreEqual(new[] { "25%", "75%", "0%" }, poll.Options.Select(o => o.Percent.text).ToList());
        }
    }
}
