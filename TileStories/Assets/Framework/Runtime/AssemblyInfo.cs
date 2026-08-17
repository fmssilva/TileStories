using System.Runtime.CompilerServices;

// Assembly-level seam: exposes `internal` members of the Runtime (TileStories)
// assembly to the two test assemblies so EditMode (TileStories.Editor.Tests) and
// PlayMode (TileStories.Tests.Runtime) suites can assert on ReconcileClusters and
// other internal cluster/LOD seams without widening their public surface.
//
// Single source of truth for InternalsVisibleTo. The duplicate previously living in
// Runtime/Core/AssemblyInfo.cs (wrong folder, same assembly) was removed -- see
// the Act B3F / Block 3.8 entry in proj_guides/__curr_plan_tracker.md. The PlayMode
// assembly name is "TileStories.Tests.Runtime" (verified against
// Framework/Tests/Runtime/TileStories.Tests.asmdef), matching the grant below.
// Location mirrors Editor/AssemblyInfo.cs at the assembly root (10-structure rule:
// self-documenting, cross-cutting config belongs at the assembly root, not a domain folder).
[assembly: InternalsVisibleTo("TileStories.Editor.Tests")]
[assembly: InternalsVisibleTo("TileStories.Tests.Runtime")]