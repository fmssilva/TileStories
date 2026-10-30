using System.Runtime.CompilerServices;

// Allow the Editor test assemblies to access internal members (the Live one holds the fixtures that enter Play Mode)
[assembly: InternalsVisibleTo("TileStories.Editor.Tests")]
[assembly: InternalsVisibleTo("TileStories.Editor.Tests.Live")]