# Plan tracker -- test harness H1: a background run stays in the background; Editor-window tests stop depending on focus

Brief: proj_guides/__AI_worker.md. Baseline: none (HEAD fd856cb; P4.4c1 closed with a full PlayMode run, 0 failed).
GATE (done): 0 commits ahead (no push); captures opened: TestEvidence/Harness/Gate_PoiEditor.png (the window only),
Gate_GameView.png (LivingRoom camera); compile: last build "Tundra build success", ExitCode 0, last error CS older than it;
fonts untouched (git status: only __AI_worker.md).

Facts found in the audit (UTF bd7f943e9647, Unity 6000.3.19f1):
- TaskList: PrebuildSetupTask runs BEFORE EnterPlayModeTask; PostbuildCleanupTask AFTER ExitPlayModeTask.
- [assembly: PrebuildSetup/PostBuildCleanup] is collected from EVERY loaded assembly referencing UnityEditor.TestRunner (any filter).
- ITestAction / IOuterUnityTestAction are collected from the test's parent suites' TypeInfo: an [assembly:] action is NOT seen,
  an attribute on a no-namespace [SetUpFixture] IS (it is the assembly's top suite).
- PlayModeView.enterPlayModeBehavior: PlayFocused / PlayMaximized / PlayUnfocused; our Game view = PlayFocused.

MEASURE + diagnosis (done): full PlayMode 1307 / 0 failed / 43 skipped, 4641 s, unfocused first + end; 3 gains, all in the first
22 s; 0 imports, LodWall re-size no gain. A/B runs: every gain comes from the MCP SERVER's focus nudge (utils/focus_nudge.py:
get_test_job + Unity unfocused + no progress 3 s -> PowerShell SetForegroundWindow). Same S3 fixture behind: poll = 2 gains, no
poll = 0; Play Focused no poll = 0; recorders no poll = 0. -> Play Unfocused switching REMOVED (no effect); fix = never call
get_test_job during a background run, wait on the Editor log "[TestRun] ended" (TestRunBoundary). Memory saved.

## Steps
- [x] 1. TestRunRecorder (Tests/Runtime, #if UNITY_EDITOR): switch OFF by default (EditorPrefs + menu TileStories/Dev/Record Test Runs),
      log TestEvidence/Harness/TestRunRecorder.log: focus, window focus, play mode, imports, scene loads, run start/end, test start/body/end
      (RecordEachTest on the SetUpFixtures + one for LivingRoom EditMode). TestRunReport (pure) -> summary table at run end.
- [x] 2. Tests: TestRunReportTests (EditMode); proof the log survives a PlayMode domain reload (one small PlayMode fixture, switch ON).
- [x] 3. MEASURE: full PlayMode run, action c, switch ON. Report the jump table BEFORE fixing.
- [x] 4. FIX: TestRunBoundary (one assembly hook: recorder marks + "[TestRun] started/ended" log lines); waiting procedure in 4.2.5.
- [x] 5. PART 3 (probes across REAL nudges: deactivation ends a text field's editing; drag hotControl survives 6 changes; Preset
      click 9/9 in every state; keyboardControl read outside OnGUI flickers per window): gestures in ONE tick
      (PoiEditorWindowHost.PaintNow = RepaintImmediately), Keyword precondition read inside OnGUI, Preset failure names the focus.
      Targeted 63/63 (incl. the [H-J] reproducer group).
- [x] 6. PROVE (full PlayMode 1307 / 0 failed / 43 gated skips, 4326 s, 0 focus gains, behind at both ends; full EditMode
      2098/2098, 156 s, 0 gains, behind at both ends; + DiscardDynamicFontChanges at the run's end, proven: spike run then EditMode run, fonts clean): full PlayMode run, action c: zero focus gains after the start, green. Full EditMode, action c: green, zero gains.
- [x] 7. Docs: 40-testing 4.2.3 + RUN TIMES, __mixed_TODOs entries closed, 10-structure tree. LivingRoomScene active. Final report 6.4.
