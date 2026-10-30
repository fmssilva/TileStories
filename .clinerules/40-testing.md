## 4. Testing Strategy

Two separate questions need separate answers here, and conflating them is what makes AR
testing feel slower than it needs to be: **"how do I iterate on a feature quickly"** and
**"how do I know a piece of logic is actually correct."** §4.1 answers the first, §4.2
the second. Do both — they're not alternatives to each other.

### 4.1 The Development Loop — Iterate Fast, Spend Real-Device Time Only Where It's Needed

A full build-deploy-test cycle on a real device takes minutes; an Editor Play Mode
iteration takes seconds. Defaulting to the slow loop for everything is the single
biggest productivity loss in Unity AR development. Use the fastest tier that can
actually answer the question you're asking, and only escalate when it can't.

**Tier A — Mock localization in the Editor (this is where most iteration happens).**
Every wall-tracking implementation gets a `MockLocalizationProvider` (or equivalent)
alongside the real tracker, selected via `#if UNITY_EDITOR` or a settings flag, that
immediately fires a successful "localized" event with a fixed pose against a flat
reference plane in the scene — no camera, no Immersal call, no device. Add basic
keyboard fly-through controls to the scene camera so you can walk the virtual wall in
the Editor Game View. Build this *first*, before any other feature that depends on
localization being "done," because everything above the tracking layer (POI spawning,
LOD, content cards, circuits, the timeline) can then be built and tested entirely with
this mock, in seconds per iteration. Only drop out of this tier when the thing you're
actually testing is tracking-layer behavior itself.

**Before planning any verification, confirm Tier A can actually produce the input
the feature responds to.** A mock/harness is only as good as the states it can
reach, and an agent that plans a whole test matrix against a harness that
structurally cannot generate the relevant input will discover this at the end,
after the plan is written. Concrete instance from this project: the entire
marker-orientation domain hangs on device *roll*, and the mock camera's look
controller returned `Quaternion.Euler(pitch, yaw, 0f)` — roll hard-zeroed — so
no roll-dependent behaviour was reachable in the Editor at all. The fix (one
extra parameter) was trivial; discovering the need late would not have been.
Make "can the harness reach this state?" an explicit first step of the plan, and
if it can't, extending the harness is its own prerequisite task, sequenced
*before* the feature work rather than folded into it.

**Tier B — Unity's XR Simulation (AR Foundation plumbing, still no device).**
Unity ships this as part of AR Foundation (`Project Settings -> XR Plugin Management ->
Simulation`) — no extra package needed. It simulates AR session lifecycle, device
orientation, and general AR Foundation plane/tracking behavior inside the Editor. It
does **not** simulate real VPS localization — that's what Tier A's mock is for. Use
Tier B specifically for AR-session-level plumbing questions Tier A's mock doesn't
touch (session lifecycle, orientation handling), not as a general substitute for Tier A.

**Tier C — Real device build (mocked or real tracking, depending on the question).**
Reserve this for things that genuinely can't be answered in the Editor: real screen
density and UI scaling, device-specific hardware behavior (Bluetooth audio,
permissions, storage), thermal/performance behavior over a real session length. If the
thing under test doesn't actually need a real wall, keep the mock tracker active even
on-device — that isolates "is this a device problem" from "is this a tracking problem."

**Tier D — Real wall, real Immersal, real device (field test).**
The one tier that can't be simulated or automated: actual localization against a
physical wall, real camera feed quality, real tracking stability as a visitor moves.
Requires a human physically present with the device. Use the human-in-the-loop
protocol in §4.6 for this tier specifically — don't invoke it for anything Tier A, B, or
C could have answered instead.

### 4.2 Automated Correctness Tests — What Actually Gets a Unit Test

Not every class needs a unit test, and writing one for everything is itself a form of
over-engineering for a project this size. Write a real, automated test for a class when
**both** of these are true:

- It's deterministic, pure logic — no `MonoBehaviour`, no scene, no device dependency,
  so it can be instantiated directly with `new` and asserted against.
- A silent bug in it would be expensive: it runs identically across every wall (so a
  bug affects all of them at once), or it drives a state machine with edge cases a
  quick manual pass would plausibly miss (entry points, race conditions, re-entrant
  triggers).

This is the actual filter — not "does this class have logic in it." Concretely, this
means config parsing and validation, the JSON-to-runtime-asset baking step, position/
coordinate resolution math, and any state machine governing multi-step user flows
(progression through a sequence, epoch/state switching, achievement or trigger
evaluation) all meet the bar. Something like a UI view's exact visual layout, or a
one-off wall-specific data quirk, usually doesn't — that's better caught by the dev
loop in §4.1 or an informal pass, not a maintained automated test.

**Preferred path — Unity MCP (use this first).** When `unity-mcp` (CoplayDev) is
connected, use it exclusively for compile checks and test runs:

1. `refresh_unity` (force compile) — confirms zero `error CS` before running tests.
2. `run_tests` (EditMode) + `get_test_job` — asserts EditMode suite.
3. `run_tests` (PlayMode) + `get_test_job` — asserts PlayMode suite.

Acceptance gate: **zero failed tests in both suites**. Never use a fixed pass count
(e.g. "59/59") as the acceptance criterion — counts change as tests are added; zero
failures does not.

**Which tests, when (three tiers).** RUN TIMES (replaced after a run when a number moved > ~10 %): BLOCK's EditMode half ~2.0 min (2200 tests, without
the Live assembly); full EditMode ~3.4 min (2211 tests); CardSmoke ~1.4 min (22 tests); CORE ~12.2 min (PlayMode 175 tests 12.0 min + EditMode 120 tests
14 s); FULL PlayMode ~76 min (1155 tests); per-fixture times in `TestEvidence/Harness/fixture_times.csv` (`TestRunRecorder` writes it after every run).

- **BLOCK -- every block, after each change:** compile, the EditMode half, then the PlayMode fixtures of the changed code
  (the feature's own + every fixture that greps for the changed type, method or config key), named one by one in `test_names`;
  a few minutes. A block closes on this tier unless its TEST PLAN says CORE or FULL.
  - **The EditMode half** is the suite without the Live assembly: `assembly_names: ["TileStories.Editor.Tests",
    "TileStories.LivingRoom.Editor.Tests"]`. The fixtures that enter Play Mode (10-45 s each: the Editor -> running card push) live in
    `Tests/Editor/Live` (`TileStories.Editor.Tests.Live`; `PlayModeEntryRuleTests` keeps them there and only them). A block that touches
    an Editor row's live push or live-sync code runs the full EditMode suite instead.
  - **In a review phase** (a domain's look reworked block after block) BLOCK's PlayMode half is the changed component's selection
    (`proj_guides/_Test_Suite_2.md` 3.2, kept current there: its fixtures in one `test_names` run, its gallery cases in one
    `group_names` run) + `category_names: ["CardSmoke"]` in a run of its own: one cheapest test per card component, the tripwire for a
    changed shared look that breaks fixtures no grep names. A block that changes a component's fixtures keeps its row and its smoke
    test current.
  Tests stay on scope: a block's own tests run every OTHER component at its default (e.g. the card's Navigation Bar at Bottom with
  its default look); another component's option matrix lives only in that component's own fixture, so it is never re-run for nothing.
- **CORE -- `category_names: ["Core"]`, PlayMode then EditMode, ~12 min, background:** every 3rd-4th block, and at the end of a block on SHARED GROUND
  (the definition below).
- **FULL -- everything, <= 90 min, background:** every ~12 blocks with the maintenance, before a domain close-out, a device
  session or a build.
- A wider tier confirms, never discovers: green on BLOCK before CORE, green on CORE before FULL. The report names the tier, its
  counts, and the blocks since the last CORE and the last FULL.

- **SHARED GROUND:** what every fixture stands on -- the test infrastructure (`CardTestInput`, `SearchSceneFixture`, the harnesses,
  `FixedFrameForTheRun`), `WallSession`, `PanelSettings`, the card tokens, the sheet / stack / host's stops, layout or binding
  lifecycle, a registry or pure rule a whole domain reads (`BlockRegistry`, `BlockStackBuilder`, `CardNavModel`). Not shared ground: a
  view in a slot that exists, Editor-only code, a config field old configs load unchanged, a new kind in its own files, docs.
- **The Core category** (`[Category("Core")]`) sits on the cheapest test that proves each domain's critical path on the SHIPPED
  default, never on a slow class or an option's matrix; the list and the path each test guards: `proj_guides/_Test_Suite_1.md`
  section 3. A block that adds a critical path tags its test.
- **`test_names` is a JSON array** of class names (`["TileStories.Tests.CardVideoGalleryTests", ...]`); a comma-separated string
  runs 0 tests. **`group_names`** is a JSON array of regexes over the FULL test name, parameters included: one kind's gallery cases are
  `["CardGalleryTests\\..*\\(\"quick_facts"]`.
- **The MCP job lists at most 25 failures:** after a red-heavy run, re-run the fixtures past the 25th by name.
- **MEASUREMENTS** skip themselves (reported as skipped) unless their local switch file `TestEvidence/<domain>/MEASURING` exists --
  the gated look / cost instruments. `[Explicit]` does not keep a test out of the unfiltered MCP run, and MCP cannot exclude a category.

**Fallback — batch-mode (only when MCP is unavailable).** In this workspace,
batch-mode Unity frequently exits with return code 1 and produces no test XML — this
is a known, persistent issue. If a batch run exits 1 with no XML output, **stop
immediately and switch to MCP**. Do not retry the batch command; do not treat
absence of XML as a pass.

```
# Only run these when MCP is explicitly not connected:
Unity.exe -batchmode -nographics -projectPath "<path>" -quit -logFile compile_log.txt
Unity.exe -batchmode -nographics -projectPath "<path>" -runTests ^
  -testPlatform EditMode -testResults editmode_results.xml -logFile editmode_log.txt
```
If the compile log shows zero `error CS` lines and the result XML exists and reports
no failures, that counts. If either file is missing or contains failures, stop and
investigate before proceeding.

When a test fails, stop and think before changing anything: is the test wrong (it's
asserting something that isn't actually the correct behavior), or is the logic wrong
(the code isn't doing what it should)? Write out up to three possible fixes, pick the
best one, apply it, and re-run.

### 4.2.1 Composition/integration tests are not optional when a spec describes composition

**Lesson from a direct audit finding**, found independently three
separate times across one project (`_2.7_Corrections_TODO.md` #2.6-i,
#2.6-al, #2.4-l — a filter-tray component, a search-synonym component,
and an AR-zoom-state component were each individually built, each
individually unit-tested in complete isolation, each passing green —
and in all three cases, the actual connection to the rest of the running
app was never built or never verified, so the feature did nothing end
to end despite its own tests being genuinely correct and green).

This is a distinct, recurring failure class from "insufficient test
coverage" — coverage of each *piece* was fine; what was missing was a
test of the *seam between* pieces. It's dangerous specifically because
it's invisible from inside either piece's own test suite: Component A's
tests pass, Component B's tests pass, and nothing anywhere asserts that
A's output is ever actually consumed by B in the running app.

**Rule:** when a domain spec explicitly describes composition between
two or more components (e.g. "component A's output feeds directly into
component B's input," "these two systems must stay in sync," "this is
one result set, not two independent outputs") — write a test asserting
the actual end-to-end composed behavior **before or alongside** writing
each component's isolated unit tests, not after, and not only if time
permits. A checklist question like "does toggling A visibly change B's
output" is a stronger, cheaper, more direct test of the thing that
actually matters than a large suite of tests that only ever exercise A
and B separately. When auditing or reviewing a feature described as
composed, explicitly search for a test that calls both real components
together — the mere existence of a call site (e.g. grep for the
method name) is not sufficient evidence the composition is exercised;
confirm the call site is reached from the actual runtime bootstrap path
(`WallSession` or equivalent), not only from a test file.

**A comment asserting an invariant is not a verified invariant — and it is more
dangerous than no comment at all.** A second, related failure mode found in a
direct audit: component B's code carried a detailed comment explaining that it
was safe *because* component A guarantees some property ("A copies the camera's
rotation exactly, so this object's local axes ARE screen axes by construction"),
and that guarantee was real in the common case and false in an edge case nobody
had tested. The comment was load-bearing, confidently worded, and never asserted
anywhere. Every later reader trusted it instead of checking, which is precisely
what a well-written comment earns.

**Rule:** when you find a comment in one component that states a guarantee made
by a *different* component, treat it as an untested claim, not as documentation.
Either write the test that asserts the guarantee end to end, or rewrite the
comment to say which conditions it actually holds under. Grepping for the
guarantee's phrasing across the repo is a cheap way to find every place that
silently depends on it before you change the component that provides it.

### 4.2.2 Modal dialogs stall the Editor and every test run

A modal dialog (a native `EditorUtility.DisplayDialog*`, a Play / Build gate) freezes Unity's main thread until someone clicks it. While it is open, MCP calls (`refresh_unity`, `run_tests`, `get_test_job`, `execute_code`) stall or time out without a useful error and a test run never advances. In the POI Editor the only modal kind left is `EditorDecision` (a question the code must wait for); every other popup -- notices included -- is a non-blocking `EditorPopup` (`_5.1_Editor_Tab.md`, "Popups: the two kinds and when to use which").

- Never write or run a test that can open a modal dialog. Drive the real flow with `EditorDecision.Responder` instead (it receives the exact question and "clicks" a button; `TestDialogGuard` answers Cancel for the whole EditMode run), and open non-blocking popups for real (`EditorPopupTests`).
- If a run or tool call stalls, assume a dialog is open before retrying: tell the developer "a dialog may be open in Unity, please click it" (a screen capture can confirm), then continue. Do not queue new runs on a blocked Editor.
- If a task truly needs a dialog (a real build with a guard), announce it first and say which button to click.

### 4.2.3 Tests never depend on window focus; tools leave no windows behind

The developer works in other applications while a run is going, so a test that passes only while Unity is the active
application is a broken test, not a flaky one.

- **Real input devices ignore focus.** A PlayMode fixture that feeds a real `Touchscreen` / `Mouse` through the Input System sets,
  on a copy of `InputSystem.settings` restored in TearDown: `editorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView`
  (an unfocused Game view) AND `backgroundBehavior = IgnoreFocus` (Unity not the active application: the default switches the
  device off and every finger reads "no touch"). `PoiCardTapZoomTests.AddFinger` is the reference; the second fixture that needs it
  moves it into one shared test helper instead of copying it.
- **A real drag's distance is not the stack's distance.** A `CardTestInput` touch stroke moves the card's stack by more than
  the finger travels (measured ~1.7x, more the longer the hold). Assert what the drag ACHIEVED --
  the spy moved to that section, the block is in view, the chrome hid -- never "N px of finger scrolled N px". A test written
  the other way is measuring the harness, not the card.
- **A failure that goes away on rerun is investigated, not dismissed.** Before calling a failure "focus" or "flaky", reproduce it
  on purpose (e.g. run the fixture with Unity in the background). If it reproduces, fix the harness; "passed on the second run" is
  not a finish.
- **What raises Unity during a run: the MCP server, never the run.** The UnityMCP server's focus nudge (`utils/focus_nudge.py`)
  runs on EVERY `get_test_job` call, with or without `wait_timeout`: when Unity reports itself unfocused and the job has had no progress
  for 3 s (any test longer than that, or the start of a run) it brings Unity to the front with `SetForegroundWindow` for 3-12 s, then
  backs off. No setting turns it off. Measured with `TestRunRecorder`: the same fixture with Unity behind,
  polled = 2 jumps, not polled = 0; the Game view's "Play Focused", profiler recorders, asset imports and the LodWall Game-view re-size
  raise nothing on their own. So a background run stays behind as long as
  nothing asks the MCP server about it while it runs: wait as 4.2.5 says.
- **What a background run guarantees.** Unity is not raised from the start to the end (the developer can work beside it), every
  test is green unfocused as well as focused, and the two Editor log lines `TestRunBoundary` writes say where Unity was at each end
  (`[TestRun] started (Unity behind)` ... `[TestRun] ended (Unity behind)`): count a run as background only when both say behind.
  With **TileStories > Dev > Record Test Runs** ticked (off by default), `TestEvidence/Harness/TestRunRecorder_report.txt` lists every
  focus gain after the start with the test that was running and what happened in the 3 s before it -- one run measures a regression.
- **An Editor-window gesture runs inside ONE editor tick.** Unity switching to or from another application, a focus change and a real
  mouse event are handled only BETWEEN editor ticks; a deactivation ends a text field's editing session for good (what was typed after it
  is lost), while a drag's `hotControl` and a button click survive it. So a real-click Editor test sends a whole gesture -- click and
  typed word, press / drags / release -- as events with `PoiEditorWindowHost.PaintNow` (Layout + Repaint of the window, now) between
  them, never one event per `yield` (`ConfigUndoGroupingTests`, `KeywordListFieldTypingTests`, `PoiEditorWindowHost.ReplaceText`). IMGUI
  state is read inside the window's own `OnGUI`: Unity swaps `GUIUtility.keyboardControl` per window around each `OnGUI`, so a test's
  coroutine reads whichever window painted last (0 one frame, the field the next).
- **Tools leave no windows.** A capture or helper script that opens an Editor window closes it with `window.Close()` (never only
  `DestroyImmediate`, which can leave an empty native shell -- the blank "POI Editor capture" / "gate" windows) and then checks
  `Resources.FindObjectsOfTypeAll<EditorWindow>()` holds none of its windows. Captures and test runs never overlap: close every
  capture window before starting a run (an open utility window takes focus from real-click Editor tests).
- Blank windows the developer finds are safe to close with their X; they hold nothing.
- **Where the developer puts Unity: exactly ONE of three short actions, in Portuguese, word for word, no explanation**
  (the developer reads only the action; the reasons below are for the agent):
  - **a)** "Mete o Unity destapado e com a janela ativa." -- anything the AGENT types or clicks into an Editor window by hand
    (an `execute_code` that drives a window); test runs no longer need it.
  - **b)** "Mete o Unity destapado; podes trabalhar noutras apps ao lado." -- the GATE capture and every POI Editor window
    capture: they read the DESKTOP, so the window must be visible, not active.
  - **c)** "Mete o Unity em background (se saltar para a frente, clica noutra app; não minimizes)." -- PlayMode runs, Phase A /
    B, Game-view captures in tests (they read Unity's own render; real input ignores focus) and every full PlayMode run (taken
    in the background, 4.2), and full EditMode runs (their real-click tests no longer need the focus). Unity does not jump to the front
    when nothing polls the MCP server during the run (above); a run that ends "(Unity in front)" is still green but proves nothing
    about focus: say so in the report instead of calling it a background run.
  In every case the developer never touches Unity itself during a run (no Play, no edits, no saves).
- **Pixel and layout assertions never depend on the Editor's window sizes.** Every PlayMode run lays out in ONE frame: 390 x 844,
  the PanelSettings reference resolution (a phone in portrait), pinned for the whole run by an assembly-wide NUnit `[SetUpFixture]`
  (`Tests/Runtime/FixedFrameForTheRun.cs`, and `LivingRoomFixedFrameForTheRun.cs` for the app's PlayMode assembly -- a SetUpFixture
  only covers its own assembly, so a new PlayMode test assembly adds its own). Fixtures do not pin it again. The pin selects a
  fixed-resolution Game view size (`CardTestInput.FixedGameViewSize`; a RenderTexture panel would break `PixelAt`, which reads the
  real Game view). A test whose subject is built for the Editor's desktop view (a dev-only demo on its Editor stage) says so and
  takes its own landscape frame for its lifetime (`using var desktopFrame = new CardTestInput.FixedGameViewSize(1280, 720);`,
  `LodWallSceneTests`); everything else is judged at the phone frame. If a test fails only after the Editor layout changed, the
  test is wrong, not the environment: fix the fixture (an unpinned run lets a landscape Free Aspect Game view break tests that only ever passed at
  one lucky window size).
- **A fixture measures the framework, not the demo wall's choices.** The LivingRoom wall is a demo and ships settings a wall author picked
  (its own navigation preset); a test that loads it measures those unless it pins what it measures. Every scene fixture
  starts from `SearchSceneFixture.SetUp`'s pins -- LOD off, the card's navigation at the framework default (the wall's own kept in
  `ShippedNavigation`) -- and an EditMode test of the framework's navigation rows loads `ShippedWall.WithFrameworkNavigation()`. A test whose
  subject IS the wall's choice pushes it back (`PoiCardPaintingSceneTests`). When a new wall setting starts to differ from the framework default
  and a fixture breaks on it, pin it in the same place -- never re-baseline the suite to the demo wall's taste.
- **A run never reads or clears the developer's own saved visitor state.** What a visitor's device remembers (the card's answers, votes, language,
  light / dark; recent searches; the last result view) is read only through `DeviceStateStore.Current`: each test assembly's `[SetUpFixture]` gives
  the run its own memory store (`IsolatedVisitorState`), and `SearchSceneFixture` a fresh one per test BEFORE the scene loads (the card reads it at its
  first bind). Never read, clear or restore PlayerPrefs in a test (`VisitorStateIsolationTests` fails on it). Why: a "dark" picked while playing by
  hand coloured a test's card.
- **Measure relative to the thing itself, and prove the precondition.** An offset is measured from the element's own rest
  position, not from a neighbour whose distance depends on the screen (`DisplacementLabelTests`); a tap on the real card's stack goes
  through `CardTestInput.TapInReach` (the target to the middle of `ReachableStack`, held still) -- the Navigation Bar and the Locate
  Button float over the stack's foot, exactly where a bare `ScrollTo` leaves a target, and the finger lands on the bar;
  `TapInView` (the viewport only) is for a ScrollView nothing floats over (a takeover page); a test that reads the scroll before a tap
  (to compare after Back) reads it AFTER `IntoReach`; a colour on the card is measured as SEEN on the render (`ThemePixelSet.SeenOverBackdrops`,
  `RingContrast`): the sheet's glass and the painted fades are pictures no background-colour walk (`EffectiveBackground`, `GroundOver`)
  sees; an "empty spot" is empty under a whole finger (22 px around it, `SearchSceneFixture.IsEmptyAround`), not one pixel; a test about scrolling asserts the stack really scrolls (content taller
  than the viewport at the stop it uses), never a 0 that proves nothing.
- A capture that keeps catching a neighbouring panel is fixed by restoring the default layout (`Window/Layouts/Default`
  through `execute_menu_item`, or `EditorUtility.LoadWindowLayout`, or ask the developer once), never by skipping the capture.

### 4.2.4 Test renders live outside Assets/

Every picture a test or a capture saves (a Game-view render, a gallery entry, a POI Editor window capture) goes to
`TileStories/TestEvidence/<domain>/`, next to `Assets/`, git-ignored -- never inside `Assets/`, where Unity would import hundreds of
generated PNGs and git would track them.

- A test asks `TileStories.Tests.TestEvidence.PathFor(domain, fileName)` (`Framework/Tests/Runtime/TestEvidence.cs`, public: the app's test
  assemblies use it too). Never `Path.Combine(Application.dataPath, "Screenshots")` -- `TestEvidenceRuleTests` fails on it.
- An agent's own captures (the `execute_code` window capture, `manage_camera screenshot`) write to `TestEvidence/<domain>/` as well; the
  domain is the tested area (`Card`, `Search`, `Editor`, `Displacement`, `Lod`, `Effects`, `MarkerGallery`).
- An aborted run can leave `Assets/InitTestScene<guid>.unity` files: delete them (`TestEvidenceRuleTests` fails while one sits there).
- A capture that shows another application (a chat window, a browser) is private content: delete it and capture again.

### 4.2.4b Precise tests that survive harmless changes

A test should fail when the feature breaks and stay green when something unrelated moves (a window, a token, a frame rate).
Precision is kept by asserting the right thing exactly, not by loosening tolerances. Before calling a test finished, check:

1. **Fixed ground.** It runs in the run's pinned frame (4.2.3); a test that needs another frame takes it explicitly.
2. **Expected values come from their source**, never re-typed: a size from `CardTestInput.TokenPx("--ts-...")`, a count or text
   from the wall's config / the gallery definitions, a colour from the palette. A token change then moves test and card together.
3. **Relative, not absolute.** Positions and offsets are measured from the element itself or its own container (rest position,
   parent bounds, `OnePixel`), never as screen coordinates that depend on the frame.
4. **Preconditions are asserted, with numbers in the message.** "The stack scrolls", "the button is inside the visible part", "this
   spot is empty under a finger", "the clip loaded": when one fails the message says the SETUP is wrong, not the feature -- and
   a test never passes on a 0 that proves nothing.
5. **Time is the test's.** Event times, `ManualVideoOutput` / manual audio devices, injected clocks (`Card.Clock`,
   `slowSheet`); waits end on a condition or a frame count, not on seconds of real time.
6. **Every tolerance has a named reason** (one physical pixel, a finger's radius, a float epsilon). A tolerance is never
   widened to make a red test green; a test that fails by a hair at a new frame is measuring the wrong thing.
7. **Real input stays real** (panel events, Input System devices); only its timing and its target point are the test's.

### 4.2.5 One Unity job at a time (never compile under a running test)

A compile (`refresh_unity` with a compile request, a saved `.cs`, an asset import that triggers one) reloads the C# domain,
and a domain reload in the middle of a test run does not pause it: it destroys it. The Test Framework throws inside
`TestJobRunner`, the MCP job stays "running" with nothing behind it (an orphan: every later `run_tests` is refused), and
the aborted run leaves `Assets/InitTestScene<guid>.unity` behind; a `refresh_unity` sent while a PlayMode job is still going can cost Editor restarts.

- **Compile clean before every run.** Save every file first, then `refresh_unity` and `read_console` (Error): zero `error CS`,
  and `editor_state` `compilation.is_compiling` false. Only then `run_tests`. A file saved after that compile (a new test file,
  an `execute_code` that writes an asset) recompiles under the run and kills it -- compile again first. A run that never starts,
  reports 0 tests or stays "running" with nothing in the Editor log: read the console for `error CS` before anything else
  (a red compile leaves the old assemblies, so tests run old code or none).
- **Serialize.** Edit -> `refresh_unity` -> `run_tests` -> wait for the run's end (the next bullet: the Editor log's
  `[TestRun] ended`; a short run with Unity in FRONT may use `get_test_job` with `wait_timeout` 60, which nudges nothing while
  Unity is focused) -> `get_test_job` says `succeeded` / `failed` -> only then the next edit, refresh, `execute_code` that saves
  assets, or run. A `get_test_job` that timed out means "still running", never "go on".
- **Wait without polling the MCP server.** A `get_test_job` call while a run goes raises Unity to the front (the MCP server's focus
  nudge, 4.2.3), and every poll is a full model turn that re-reads the session. For any run Unity is behind for, and any run over
  ~1 min: (1) BEFORE `run_tests`, save the Editor log's size (`stat -c %s "$LOCALAPPDATA/Unity/Editor/Editor.log"`); (2) start the
  run and say in ONE chat line what runs and its expected time (RUN TIMES above); (3) start ONE background Bash watcher
  (`run_in_background`) that waits until the bytes AFTER that offset hold `[TestRun] ended` (a watcher reading from before the
  offset finds the previous run's line); (4) when it wakes you, call `get_test_job` ONCE for the counts. Never `get_test_job`, with or
  without `wait_timeout`, before the end line. Reading the `mcpforunity://editor/state` resource (`tests.is_running`) does not nudge.
  After the run, update RUN TIMES if it moved by more than ~10 %.
- **Ask the developer ONCE before a run or capture that needs Unity in a given place** (the GATE capture, POI Editor captures, a
  background run), with the AskUserQuestion tool so the session waits for the answer. The question is ONLY the action of 4.2.3
  (a, b or c, word for word) plus, at most, how long ("~2 min", "~60 min"); no explanation of why. Options: "Pronto" / "Espera".
  Ordinary runs (Unity may be covered) only get the one chat line above: a blocking question there would stall the block while
  the developer is away.
- **Check before every compile.** Read `editor_state`: `tests.is_running` false and `compilation.is_compiling` false. If a job
  is running, wait for it; do not cancel it to go faster (stopping Play Mode aborts the run and loses its results).
- **Unity preference, set once per machine:** Edit > Preferences > Asset Pipeline > Auto Refresh = "Enabled Outside
  Playmode" (EditorPrefs `kAutoRefreshMode` = 2), so a file saved while a PlayMode run is going waits for the run to end.
  It does not guard EditMode runs or an explicit `refresh_unity` -- the serialize rule above does.
- **An orphaned job heals without a restart.** The MCP `TestJobManager` marks a "running" job failed when a domain reload
  finds it untouched for more than 5 minutes (log line `[TestJobManager] Clearing stale job`). So: confirm Play Mode is
  off and no run is progressing, wait until 5 minutes have passed since the job's last update, then request one compile
  (`refresh_unity` compile=request). Ask the developer to restart Unity only if the job is still there after that.
- After any aborted run, delete the `InitTestScene` leftovers (4.2.4); if one is the active scene, open another scene first.

### 4.2.6 The Editor log stays small

`%LOCALAPPDATA%\Unity\Editor\Editor.log` (and `Editor-prev.log`, the previous session) is where a stuck run, a compile in
the middle of a job or a crash shows up -- read it before guessing (rank repeated lines: strip digits, `sort | uniq -c | sort -rn`).
Writing it is not free: `Debug.Log` lines repeated per POI on every wall build of every test, with their stack traces, have
grown one session's log to 823 MB.

- Player Settings keep **Stack Trace for Log = None** (`m_StackTraceTypes`, the fourth pair = 00): an info line is one line.
  Warnings, errors and exceptions keep their stack traces.
- Per-item / per-frame detail goes through `DevLog.Detail(LogDomain.X, ...)` (20-code-quality 2.1), off by default, so a test
  run writes only one-line summaries, warnings and errors. Deleting the log file fixes nothing: the cost is writing it.

### 4.3 Asset Database Refresh Discipline

Any edit to a `.meta`, `.prefab`, `.asset`, or raw asset file (texture, audio, model)
made through file editing rather than Unity's own Editor UI needs an explicit
AssetDatabase refresh before it can be trusted — Unity does not always detect and
fully re-resolve such changes automatically, and this is documented Unity behaviour,
not a project-specific quirk (see `AssetDatabase.Refresh`'s own scripting reference:
*"You might need to call this method if... you have made changes to assets on disk
from an external application while the Editor is running"*).

This matters more than it looks because of an asymmetry the §4.2 test workflow
doesn't always cover: launching `Unity.exe -batchmode` starts a **fresh** process,
which naturally re-scans the project on that launch — so a batch-mode compile/test
run can genuinely pass right after an asset edit. But if the person is
also running a **separate, already-open** Unity Editor window for manual Play Mode
testing (the normal case), that long-running session was never told anything
changed, and will keep showing stale — sometimes actively broken — asset references
until it's explicitly refreshed. A passing batch-mode test result is not evidence
the person's own open Editor session sees the same state.

- If Unity MCP tooling is connected (e.g. `CoplayDev/unity-mcp`'s `refresh_unity` or
  `manage_asset` reimport action), call it immediately after any asset-file edit,
  before reporting the task done or asking the person to test — do not wait to be
  asked.
- If no such tooling is connected, say so explicitly in the handoff message: name
  the specific files changed and state plainly that the person needs to trigger
  `Assets > Reimport` (or `Reimport All` for a broader change) in their own open
  Editor before testing — do not silently assume a file edit is equivalent to a
  completed import.
- If a reported bug looks identical before and after a fix that checks out correctly
  in the file's own text, a stale Editor/Library cache is a real, common, and cheap
  hypothesis to check before re-diagnosing the reference itself — worth explicitly
  ruling in or out early, not treated as a last resort.

---

### 4.4 Phase A/B — Isolated Verification Before Integration

Any domain with a visual/rendered component (UI, AR markers, effects, animations)
gets built and verified in two phases. Do not build the full feature and the real
AR/data-pipeline integration together and debug both at once — this is what turned
a marker rendering bug into several confusing debugging rounds. Isolating the
rendering system from tracking/config-driven data removes one whole axis of
variables before the harder integration work even starts.

**Phase A — isolated test scene, zero AR/tracking/config.json.** A dedicated,
non-shipping scene (excluded from Build Settings) that exercises every rendering
variant of the domain directly, fed fabricated data instead of the real pipeline.
Built incrementally: the single simplest case first, confirmed correct, only then
the next variant — never every variant built first and debugged together. Do not
proceed to Phase B until every Phase A variant is confirmed via §4.5's tiers.

**Phase B — real pipeline integration.** Wire the now-proven-correct system into
the real domain flow. If Phase B shows a problem Phase A didn't, that's a strong
signal the bug is in the integration/config layer, not the rendering system — Phase
A already proved that part correct in isolation; debug the data feeding it, not the
rendering code itself.

**Which recipe, by rendering system:**

| System | Recipe |
|---|---|
| uGUI world-space (Markers and similar) | Grid gallery scene, positioned in 3D, no AR. One data-struct entry list (group/label/variant fields) drives both the visual harness that spawns every row and the automated test suite that asserts on them — never author the gallery layout and the test list separately, or they will drift apart |
| UI Toolkit screen-space (DetailCard, quiz, toasts, GuideCharacter, Navigation, Sharing) | Two-step: (1) static layout check in Unity's **UI Builder** — zero Play Mode, zero code, catches styling/layout bugs for free; (2) a small runtime harness scene with one `UIDocument`, fed fabricated content variants (long/short text, with/without media, each state) |
| Non-visual integrations (AI API calls, future 3D/animation work) | A minimal sandbox scene or script — call the thing in isolation, log actual request/response/error states; same "isolate before integrating" principle, different mechanism |

**Not every domain needs a gallery.** Build one only when there are genuine
combinatorial variants worth grid-testing. A single-fixed-state element gets one
manual Play Mode check — building gallery machinery for it is the over-engineering
`20-code-quality.md` already warns against.

**Shared conventions across all Phase A scenes**: live under `Assets/Dev/<Domain>Gallery/`;
reuse the same backdrop photo asset across every uGUI gallery scene rather than
re-supplying one per domain; use the same data-struct-driven entry-list pattern
(one list of plain data, consumed by both the visual harness and the automated
test suite, so they can never silently drift apart).

### 4.4.1 Edit-Mode tooling parity (when applicable)

*Applies only to domains that ship or touch an Editor-time authoring/preview
tool (a custom `EditorWindow`, custom inspector, etc.) that instantiates or
configures the domain's real runtime objects in the Scene view outside Play
Mode.* Not every domain has one — skip this entirely for those that don't.

This is not a third phase after A and B. It's a different *axis*: Phase A/B
is about which pipeline drives a component (fabricated test data vs. the real
production flow); this is about which *runtime context* renders it (Edit Mode
vs. Play Mode). A domain can need this check regardless of where it is in
Phase A/B, and it has its own failure class that neither phase's Play-Mode
testing will ever catch, no matter how thorough that testing is:

- `MonoBehaviour.Update()`/coroutines do not tick in Edit Mode. Anything
  per-frame (animation, effects, timers) will not visibly run there — that's
  expected, not a bug. Say so explicitly in the domain plan so a later agent
  doesn't mistake it for one.
- Editor-instantiated objects are often real, persistent scene GameObjects
  ("hard copies"), not ephemeral runtime spawns — confirm this directly by
  reading the tool's code rather than assuming either way. It changes what
  "refresh" should mean: reconfigure the objects already sitting in the
  hierarchy in place, not destroy-and-respawn them.
- The single most common gap: a component's real configuration entry point
  (e.g. `SomeView.Initialise(...)`) being correctly wired for the Play-Mode
  path doesn't mean anyone remembered to call it from the Editor tool too.
  Check this explicitly, by reading the tool's code — it's an easy, silent
  gap. (This exact thing has happened before: an authoring tool positioned
  objects correctly for a long time while never once calling the real
  `Initialise`-equivalent method, so every Editor-mode preview silently
  showed a prefab's raw default look, not the actual configured result.)

Verify with the same evidence discipline as §4.5, adapted to this context:
Tier 0 = does the tool's populate/refresh code path actually call the same
real entry point the Play-Mode path uses (read the code, don't assume it
does just because Play Mode works); Tier 1 = does the Scene view show the
correct *static* composition immediately after a field changes, with zero
Play Mode entries — not whether it animates, which isn't expected here.

### 4.5 Verification Tiers and Evidence Standards

Four tiers, cheapest and most-automatic first. Higher tiers exist to catch what
lower tiers structurally cannot — never skip a cheap tier to save time by going
straight to an expensive one, and never treat a higher tier's pass as covering what
a lower tier should have already caught.

**Tier 0 — structural assertions (language agent, always runs).** Real NUnit
`Assert` calls, not `Debug.Log` inspection. A log line is not a pass/fail signal —
an agent skimming log text for something that "looks plausible" is exactly the
failure mode this tier exists to eliminate. `Assert.IsNotNull(sprite)` either
throws or it doesn't; there is nothing to interpret. If a harness only logs and
never asserts, add the assertions before trusting it.

**Tier 0.5 — programmatic UI-quality checks (language agent, no vision needed).**
Every one of these is exact, not approximate — the mechanism a real interaction
uses, simulated, not inferred:

*Occlusion / actually-clickable check* — simulate a tap at the element's own
centre and confirm it, not something on top of it, receives the hit:
```csharp
// Confirms a UI element is genuinely the topmost raycast target at its own
// centre point -- i.e. actually tappable, not visually present but covered by
// something else. Requires an EventSystem in the scene.
public static bool IsTopmostRaycastTarget(RectTransform target, Camera uiCamera = null)
{
    var raycaster = target.GetComponentInParent<GraphicRaycaster>();
    if (raycaster == null || EventSystem.current == null) return false;

    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
    var pointerData = new PointerEventData(EventSystem.current) { position = screenPoint };
    var results = new List<RaycastResult>();
    raycaster.Raycast(pointerData, results);

    return results.Count > 0 && results[0].gameObject.transform == target;
}
```

*Contrast check* — WCAG 2.1's own formula (SC 1.4.3 text, SC 1.4.11 UI
components), pure arithmetic on two RGB colours:
```csharp
// Relative luminance + contrast ratio, ported directly from the W3C formula.
// AA thresholds: 4.5:1 normal text, 3:1 large text / UI component boundaries.
public static class ContrastCheck
{
    private static float ToLinear(float c) =>
        c <= 0.03928f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);

    private static float RelativeLuminance(Color c) =>
        0.2126f * ToLinear(c.r) + 0.7152f * ToLinear(c.g) + 0.0722f * ToLinear(c.b);

    public static float ContrastRatio(Color a, Color b)
    {
        float la = RelativeLuminance(a), lb = RelativeLuminance(b);
        float lighter = Mathf.Max(la, lb), darker = Mathf.Min(la, lb);
        return (lighter + 0.05f) / (darker + 0.05f);
    }

    public const float MinRatioNormalText = 4.5f;
    public const float MinRatioLargeTextOrUIComponent = 3.0f;
}
```

*Minimum tap target* — WCAG 2.5.5: `target.rect.width >= 44f && target.rect.height >= 44f`.

*Text truncation* — TextMeshPro exposes this directly: `textInfo.isTextTruncated`.

These four cover most of what "does this look okay" prompts actually try to check.
Put them in the same PlayMode suite as Tier 0's structural assertions — same cost,
same evidence standard, still zero vision required.

**Tier 1 — consolidated vision pass (vision agent, rate-limited, use deliberately).**
Batch into as few calls as possible — one screenshot of a whole labeled gallery
grid, not one call per marker. **Never ask "does this look correct?"** — that
invites an agreeable answer and is exactly how the square-marker screenshot got
rubber-stamped. Instead, hand the vision agent a numbered checklist of specific,
falsifiable claims tied to what Tier 0/0.5 already confirmed structurally, and
require a per-item answer citing what is actually observed:

> Entry "religious": is the Symbol a filled circle, not a square or rectangle?
> [yes/no — describe the actual shape you see]
> Entry "60% Heavy" (OutlineGold): is a dashed ring visible around the symbol?
> [yes/no — describe what you see instead if no]

A holistic summary answer to a checklist prompt is itself a sign the check wasn't
done properly — reject it and re-ask item by item if that's what comes back.

**Look before you test (the order of work, `00-process.md` ACT MODE 2):** a new or changed visible element goes build -> compile ->
captures and design fixes -> PlayMode tests. The design pass uses quick captures of the real scene (no fixture needed: the Unity MCP
screenshot of the Game view at the stop, or a throwaway capture call), light and dark, judged item by item; only when it looks right
are the PlayMode tests written and run -- they pin the accepted look (real input, edge cases, motion, contrast as seen). A brief may
add a developer checkpoint here: show the renders, ask once, then test.

**Visual verification is the agent's job, not the developer's.** The agent is vision-capable and the
developer keeps the Unity Editor open and unobstructed during agent sessions. So every change with a visible
result -- a POI Editor row, table or popup, a Game-view render, a demo grid, a card, a marker -- is captured
and checked BY THE AGENT before it is reported done: take the capture (Unity MCP screenshot tool, or the
`ReadScreenPixel` procedure in `proj_guides/_5.1_Editor_Tab.md`, "Verifying and debugging layout"), write a
numbered checklist of falsifiable claims (Tier 1 above), answer each item from what the capture actually
shows, fix what it shows wrong and capture again. "Tested with real clicks but not looked at, please check"
is not an acceptable finish while a capture is possible. The developer's own look is the fallback below.
The checklist always includes the DESIGN questions, not only "is it there": does the subject use its space (a model, picture
or chart fills most of its frame, not a speck in the middle), is the hierarchy readable at 390 px, does it look like a finished
product a visitor would trust. "Renders and rotates" is a test result; "fills the frame and reads well" is the visual check
(10A.2b: both previews were drawn at about a fifth of their stage and passed a "correct" visual check).

**Visual evidence needs the Editor on screen.** A screen capture reads whatever is on the desktop; if another application covers Unity it captures that application instead (private content). Always open a capture and confirm it shows the Editor window, and delete any that does not. When Unity is covered: do not stop the whole task. Finish everything that needs no pixels, then, before the final report, ask the developer ONCE to leave Unity in front and retry (ask early when the whole task is visual). If it still cannot be captured, do not claim visual verification: record the unverified items in the domain's pending-verification list (for the POI Editor window: `proj_guides/_5.1_Editor_Tab.md`, "Verifying and debugging layout") and say so in the report.

**Tier 2 — human (rarest, most expensive).** Real device, final subjective/
aesthetic judgment, real-world legibility. Reserved for what Tiers 0/0.5/1
structurally cannot answer — not a catch-all for skipping the cheaper tiers.

**Handoff between tiers/agents.** A handoff summary's job is to tell the next
agent *where to look*, not *what to conclude* — efficiency (don't re-explore the
whole project), never a substitute for independently checking. Every handoff
includes: which files/scenes to open, which Tier 0/0.5 tests passed with their raw
output attached (not paraphrased), and — for a Tier 1 handoff — the itemized
checklist to run, framed explicitly as "verify these independently," never
"confirm everything is fine." If you are the receiving agent: re-derive the
evidence yourself for anything you report on. Trusting the prior summary's
conclusion without checking is the exact failure this section exists to prevent.

## 4.6. Human-in-the-Loop Protocol (Tier C and Tier D only)

Tier A, Tier B, and §4.2's automated tests need no human interaction at all — run them
yourself and read the results. Only use this protocol for Tier C (real device build)
and Tier D (real wall/AR field test), since those are the only tiers that genuinely
require a human or physical access.

1. Add or update any single-line debug log statements needed to trace the specific thing
   being verified this round, and remove any stale trace logs left from the previous
   round.
2. Send a clear, specific message describing exactly what physical action is needed
   (e.g. "Ready — install and open the app, walk up to the wall, hold the phone steady
   for 5 seconds, then approve this command"), and wait.
3. Once the person confirms the action is done, read the device log file (from `adb
   logcat` redirected to a file, not piped directly to the terminal) and check for the
   expected trace lines and any errors.
4. If something is wrong, think through the big picture first — is this a tracking
   configuration issue, a data issue, or a code bug — write out up to three possible
   fixes, apply the best one, and repeat the loop. Do not move on until this round's
   specific check is clean.

---