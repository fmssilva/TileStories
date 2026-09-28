namespace TileStories
{
    // The framework's own block kinds (_3.1 section 3), registered once into BlockRegistry.Shared (and into any
    // registry a test builds). Kinds are added tier by tier, one at a time (_3.1 section 7). Every help text is
    // framework-authored and app-agnostic: it names Editor Tab controls, never a wall's own content.
    public static partial class BuiltInBlocks
    {
        // What every long-text help says about paragraphs and glossary words
        private const string LongTextHelp =
            "Leave an empty line between paragraphs. Write [[word]] to link a word to its Glossary entry (Detail Card > " +
            "Glossary): the visitor taps it to read the definition. [[shown words|word]] shows other words for the same entry.";

        // Register every built-in kind into `registry`
        public static void Register(BlockRegistry registry)
        {
            registry.Register(Header, () => new HeaderBlockView());
            registry.Register(Status, () => new StatusBlockView());
            registry.Register(QuickFacts, () => new QuickFactsBlockView());
            registry.Register(RichText, () => new RichTextBlockView());
            registry.Register(FunFact, () => new FunFactBlockView());
            registry.Register(PullQuote, () => new PullQuoteBlockView());
            registry.Register(ProcessSteps, () => new ProcessStepsBlockView());
            registry.Register(Swatches, () => new SwatchesBlockView());
            registry.Register(Timeline, () => new TimelineBlockView());
            registry.Register(Person, () => new PersonBlockView());
            registry.Register(StoryChapters, () => new StoryChaptersBlockView());
            registry.Register(ComparePoints, () => new ComparePointsBlockView());
            registry.Register(PracticalInfo, () => new PracticalInfoBlockView());
            registry.Register(Sources, () => new SourcesBlockView());
            registry.Register(Actions, () => new ActionsBlockView());
            registry.Register(Gallery, () => new GalleryBlockView());
            registry.Register(BeforeAfter, () => new BeforeAfterBlockView());
            registry.Register(ZoomImage, () => new ZoomImageBlockView());
            registry.Register(HotspotImage, () => new HotspotImageBlockView());
            registry.Register(WallLocator, () => new WallLocatorBlockView());
            registry.Register(TodayMap, () => new TodayMapBlockView());
            registry.Register(Related, () => new RelatedBlockView());
            registry.Register(KnowledgeCheck, () => new KnowledgeCheckBlockView());
            registry.Register(Poll, () => new PollBlockView());
            registry.Register(Collect, () => new CollectBlockView());
            registry.Register(Feedback, () => new FeedbackBlockView());
            registry.Register(Dialogue, () => new DialogueBlockView());
            registry.Register(ShowOnWall, () => new ShowOnWallBlockView());
        }
    }
}
