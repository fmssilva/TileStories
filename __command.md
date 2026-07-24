Do the tasks written in this file:
C:\Users\franc\Desktop\TileStories\_2.1_Marker_Orientation.md

Before writing any code: read that file in full, then open and read the
actual current files it references (POI_Marker.prefab, MarkerView.cs,
POIAnchor.cs, MarkerOverlapResolver.cs, WallSession.cs) yourself — do not assume the plan's description of them is still accurate, confirm it.

Follow these guidelines exactly, no exceptions:
C:\Users\franc\Desktop\TileStories\.clinerules

Read the global work plan if you need broader context:
C:\Users\franc\Desktop\TileStories\_0_work_plan.md

Write a full TODO list first, including test steps, before implementing
anything (guidelines - 00-process).

Some numeric values in the plan (symbol size, label offset, font size) are explicitly starting points, not final — tune them visually per the plan's own verification steps, don't just apply them once and stop.

When it's time to verify: follow guidelines 40-testing exactly — check the Unity batch-mode compile log for "error CS" lines first, then run the
EditMode/PlayMode tests and read the actual result XML, don't just report
"it works." Implement the one PlayMode test the plan specifies.

If you reach the device smoke-test step, stop and follow the
Human-in-the-Loop protocol in guidelines — tell me exactly what
physical action you need me to do, and wait. Do not skip this step or
report it as done without me confirming it.

Use PowerShell conventions from guidelines section 9 (semicolons, not &&;
redirect output to a log file, don't pipe through findstr).

If anything about the actual project state doesn't match what the plan
assumes, stop and flag it to me rather than guessing and building on top of
an assumption that might be wrong.

When finished: update the plan file itself to check off what's done, per
guidelines 60-finishing, and give me a short summary in chat — files touched,
what changed, and the resulting behavior. Don't create a separate summary file.

Be methodical, not fast.











i already started implementing the tasks written in this file:
C:\Users\franc\Desktop\TileStories\_1.2_POI_Colision_Solver.md

confirm the current status of evything and then continue to implement and test evrything well. 

be methodical and confirm you d everything in the correct way, and then you confirm thing compile well and execute well - implement also some tests to confirm everything is ok. when needed you have the unity MCP tool that you can call to confirm everything in unity project. 

keep in mind the guidelines:
C:\Users\franc\Desktop\TileStories\.clinerules

and if needed for better context you can read if necessary the 1.2 stage work plan:
C:\Users\franc\Desktop\TileStories\_1.2_POI_markers_plan.md

be good, and methodical, not fast.

and also start by confirming what is wrong with the project currently because we have these errors when cmpiling:
nity CIL Linker
Mono.Linker.LinkerFatalErrorException: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests\Runtime\MarkerOverlapResolverTests.cs(39,13): error IL1005: TileStories.Tests.MarkerOverlapResolverTests.ApplyOverlapOffsets_FiveClusteredMarkers_AllEndUpVisuallySeparated(): Error processing method 'TileStories.Tests.MarkerOverlapResolverTests.ApplyOverlapOffsets_FiveClusteredMarkers_AllEndUpVisuallySeparated()' in assembly 'Assembly-CSharp.dll'
 ---> Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: 'nunit.framework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=null'
   at Unity.IL2CPP.Common.MissingMethodStubber.GetTypeModule(TypeReference type, IEnumerable`1 assemblies)
   at Unity.Linker.Steps.AddUnresolvedStubsStep.MarkAssemblyOfType(UnityLinkContext context, TypeReference type)
   at Unity.Linker.Steps.Marking.UnresolvedStubMarking.HandleUnresolvedType(TypeReference reference)
   at Unity.Linker.Steps.UnityMarkStep.HandleUnresolvedType(TypeReference reference)
   at Mono.Linker.Steps.MarkStep.MarkCustomAttributes(ICustomAttributeProvider provider, DependencyInfo& reason, IMemberDefinition sourceLocationMember)
   at Mono.Linker.Steps.MarkStep.ProcessMethod(MethodDefinition method, DependencyInfo& reason)
   at Unity.Linker.Steps.UnityMarkStep.ProcessMethod(MethodDefinition method, DependencyInfo& reason)
   at Mono.Linker.Steps.MarkStep.ProcessQueue()
   --- End of inner exception stack trace ---
   at Mono.Linker.Steps.MarkStep.ProcessQueue()
   at Mono.Linker.Steps.MarkStep.ProcessPrimaryQueue()
   at Mono.Linker.Steps.MarkStep.Process()
   at Unity.Linker.Steps.UnityMarkStep.Process(LinkContext context)
   at Unity.Linker.UnityPipeline.ProcessStep(LinkContext context, IStep step)
   at Mono.Linker.Pipeline.Process(LinkContext context)
   at Unity.Linker.UnityDriver.UnityRun(UnityLinkContext context, UnityPipeline p, LinkRequest linkerOptions, TinyProfiler2 tinyProfiler, ILogger customLogger)
   at Unity.Linker.UnityDriver.RunDriverWithoutErrorHandling(TinyProfiler2 tinyProfiler, ILogger customLogger)
   at Unity.Linker.UnityDriver.RunDriverWithoutErrorHandling()
   at Unity.Linker.UnityDriver.RunDriver()

UnityEditor.EditorApplication:Internal_CallDelayFunctions ()


solve all errors and use unity mcp to cnfirm evrything is correct and well done