# TileStories — Unity Development Guidelines for AI Coding Agents

You are acting as a principal software architect and lead engineer on this project.
These guidelines are the complete standing rules for how you write code, structure the
project, test your work, and communicate progress. Follow them for every task, no
exceptions, unless the person working with you explicitly says otherwise for that task.

---

## 1. Before You Start Any Task

**Evidence discipline, stated once here because it applies to every single task in
this document**: never report a rendering/visual/behavioral task as passing without
pointing to a specific mechanical result that proves it — an `Assert` that actually
passed (raw output shown, not paraphrased), or a specific, itemized observation
citing exactly what was seen. "No errors were thrown" is not evidence of
correctness. A previous agent's summary is not evidence of correctness. Only
independently re-derived evidence is. See §6.5 for the full protocol — this rule is
why it exists.


- Read the current project plan / phase plan document in full before writing anything.
- Read every existing file relevant to the task before touching it. Never assume you know
  what a file does from its name — open it and read it.
- Search the project for existing types, classes, ScriptableObjects, or helper methods
  that already do part of what you're about to build. Extend or reuse them instead of
  writing a duplicate.
- Write a comprehensive TODO list of every step the task requires, including test steps,
  before starting implementation. Do not skip steps later because the list didn't include
  them — write the list completely up front.
- Take your time. Quality over speed. There is no reward for finishing fast if the result
  is messy, duplicated, or untested.

---

## 0.1. Think Before You Code

For every non-trivial feature or fix, stop and reason at two separate levels before
opening a file to edit:

- **Architecture level.** Where does this thing live? Which folder, which assembly, is it
  shared framework code or specific to one wall/app? Write out three distinct structural
  options with their trade-offs, then pick the cleanest one. State the choice and the
  reason in your response before implementing.
- **Implementation level.** How is this actually written, locally, in code? Write out
  three distinct concrete approaches (e.g. event-driven vs. direct method call,
  ScriptableObject-driven vs. hardcoded, composition vs. inheritance), then pick the most
  robust and simplest one.

**Benchmark standard:** write this project as if it were an academic reference
implementation that other students will read to learn from. If an existing type, class,
or system is messy or wrong, do not patch around it with an adapter, a wrapper, or a
backward-compatibility shim. Stop, propose the structural fix as three options, get
agreement, and rewrite it properly. Shortcuts that leave messy code in place because
"it still works" are not acceptable.

---


## 3. Writing Code

- Create the actual file and write its content into it directly. Do not draft a large
  file in your own working memory and dump it all at once at the end — write it as you
  go, so that if the session is interrupted partway through, the work already done is not
  lost.

---


## 0.3. General Operating Principles

- There is no time pressure that justifies messy or untested code. Be thorough, not fast.
- Every task starts with a complete TODO list, including its own testing steps, written
  before implementation begins — not assembled retroactively once code already exists.
- When something about the existing code is unclear or looks wrong, stop and ask or flag
  it rather than guessing and building on top of an assumption that might be incorrect.







## 1. Project Structure

### 1.1 Two top-level areas: Framework and Apps

- `Assets/Framework/` — everything that behaves identically no matter which heritage
  wall is loaded. This includes AR session bootstrap, tracking abstractions, the POI data
  model and rendering, UI shells, content-card rendering, analytics, and the guide
  character system.
- `Assets/Apps/<WallName>/` — one self-contained folder per wall (e.g. `Panorama`,
  `Chafariz`, `Mural`, and any local development wall used for fast iteration). Each
  contains only that wall's data: its POI list, category taxonomy, map/localization
  files, and media (images, audio, 3D models, video).
- A system moves from an app-specific folder into Framework only once a **second**
  wall needs the exact same thing, unchanged. If only one wall currently needs it, it
  stays local to that wall's folder, even if it looks reusable. Do not generalize ahead
  of actual need.
- Nothing in `Framework/` may ever reference anything inside a specific `Apps/<WallName>/`
  folder. If Framework code needs wall-specific information, that information is passed
  in through a data contract (a ScriptableObject base class or interface) that the wall
  folder implements — never a direct reference the other way.
- No wall folder references another wall folder. If two walls need the same thing, that
  thing belongs in Framework, not copy-pasted between wall folders.

### 1.2 Editor code is physically separated from Runtime code

- `Assets/Framework/Runtime/` — code that ships in the built app.
- `Assets/Framework/Editor/` — code that only runs inside the Unity Editor (custom
  inspectors, menu-item tools, validation scripts, wall-setup wizards). This code must
  never end up in a device build.
- Do this separation using **Assembly Definition files (.asmdef)**, not just folder
  naming. Folder naming is a convention an editor mistake can silently violate; an
  assembly reference rule is enforced by the compiler and fails loudly if violated.
- The Runtime assembly must never reference the Editor assembly. If a runtime script
  needs to call editor-only functionality, that is a sign the code belongs in the Editor
  assembly instead, wired through a build step or menu tool, not a runtime call.
- Wrap any code that must exist in a runtime file but only makes sense in the editor
  in `#if UNITY_EDITOR` — but prefer physically moving it to the Editor assembly whenever
  possible, since that is caught at compile time rather than relying on a preprocessor
  directive someone might forget.

### 1.3 Domain-centered folders, not type-centered folders

- Group files by what they do (`Tracking/`, `POI/`, `Content/`, `Analytics/`), never by
  generic technical category (`Scripts/`, `Prefabs/`, `Managers/`). A reader should be
  able to look at the folder tree alone and understand what the project does, without
  reading a single file.
- Never create a `Utils.cs`, `Helpers.cs`, or `Common.cs` file that accumulates unrelated
  static methods over time. If a file starts doing more than one clearly nameable job,
  split it into separate files named after each job.
- Test folders live alongside the code they test (`Framework/Tests/EditMode/`,
  `Framework/Tests/PlayMode/`), not in a separate top-level `tests/` tree disconnected
  from the code.

---

## 1.4. Complete Project File Structure

  Read in the global project guide in the file: 
  C:\Users\franc\Desktop\TileStories\_0_work_plan.md
  the section "## A. Complete Project File Structure", with the project structure that we should follow as base when implementing things. 


---








## 2. Code Quality Rules

- **MonoBehaviours stay thin.** A MonoBehaviour's job is to exist in the scene and
  respond to Unity's lifecycle (`Awake`, `Update`, `OnTriggerEnter`, UI event callbacks).
  Actual logic — filtering, scoring, parsing, deciding what content to show — belongs in
  plain C# classes with no `MonoBehaviour` inheritance, so that logic can be created and
  tested with a plain `new SomeClass()` and no scene running at all. If a MonoBehaviour
  grows past roughly 150–200 lines, that is a signal logic needs extracting out of it.
- **Configuration is data, not code.** Anything specific to one wall (POI positions,
  category names and colors, localization map identifiers, content text) lives in a
  ScriptableObject asset or a structured config file, never hardcoded inside a script.
  A new wall should be addable by authoring new data assets, not by writing new code.
- **No single class owns everything.** Do not build one `GameManager`-style class that
  owns tracking state, POI state, UI state, and analytics all at once. Split by
  responsibility into separate classes (e.g. a session/state owner, a POI registry, an
  analytics logger) and connect them through serialized references or small C#
  events/`UnityEvent`s, not by having one object know about and control everything else.
- **Session/orchestrator classes are the most common place this rule quietly gets
  broken — watch them specifically.** Any class named `*Session`, `*Manager`, or
  `*Controller` that's responsible for sequencing a startup flow (load config ->
  wait for tracking -> spawn content -> wire UI) has a natural gravity toward
  absorbing "just one more piece of logic" at each step, since it's already touching
  everything. Its job is to call other objects that make decisions, not to make
  decisions itself. If you find yourself writing an `if`/`switch` inside one of these
  classes that decides *what* should happen rather than just *when* to call the thing
  that decides, that logic belongs in a separate class instead. A useful check: could
  you delete every line of this class's own decision logic and replace it with calls
  to smaller classes without changing what the app does? If yes, do that now, before
  the class grows further — this is far cheaper to fix at 50 lines than at 300.
- **Prefer simple duplication over a fragile generic abstraction.** If two pieces of code
  look similar but serve genuinely different purposes, it is often better to keep them as
  two small, separately readable pieces of code than to merge them into one function
  controlled by several boolean flags and `if` branches. Only extract a shared function
  or base class once the exact same logic is genuinely needed unchanged in more than one
  place — and even then, prefer the simplest possible shared shape. A wrong abstraction
  that has to be unwound later costs more than a little repetition did.
- **Use what Unity and the SDKs already give you.** Use Addressables for wall content
  packaging and loading, Unity Test Framework for tests, the Input System package for
  input, UI Toolkit for interface layout and styling, the Localization package if and
  when text localization is implemented, and the Immersal SDK's own APIs for tracking —
  do not hand-roll a custom content loader, input handler, or localization system that
  duplicates what an installed package already does correctly.
- **No speculative code.** Implement only what the current task actually needs. Do not
  add a parameter, a flag, an interface method, or a config field because it "might be
  useful for a future wall" unless a concrete, currently-known requirement needs it.
- **Self-documenting names.** Name every class, method, field, and folder so a reader
  understands its purpose from the name alone, without needing to open the file.
- **Standard C# naming:** `PascalCase` for classes, methods, and public properties;
  `camelCase` for local variables and parameters; a leading underscore for private
  serialized fields (`_wallConfig`) is acceptable and common in Unity codebases — pick one
  convention and use it consistently throughout the project, do not mix styles between
  files.

---

## 2.1 Comments and Logging

- **One short comment above every method**, written in plain, direct, infinitive-form
  language (e.g. `// Localize the camera pose against the current wall's map`,
  `// Load and validate this wall's POI config`). Where first-person-plural reads more
  naturally for something the code is actively doing, that's fine too (e.g.
  `// we cache the resolved position here so we don't recompute it every frame`).
- **Inline comments explain the why, not the obvious what.** Write them as short, casual
  bullet-style notes, like one developer explaining a tricky bit of code to another
  developer standing next to them — not formal prose, not restating what the line already
  says. Example: `// Immersal returns pose in map space, not world space - convert
  before spawning POIs` is useful. `// loop over the list of POIs` is not.
- **Zero emojis, strict ASCII only — everywhere.** This applies to code, comments, log
  messages, editor window labels, and any text written to a file or the console. Unity's
  console and device logs (Logcat, Player.log) can mangle non-ASCII characters on some
  platforms, so avoid them entirely. Use plain equivalents:
  - Replace `✓` with `[ok]`
  - Replace `→` with `->`
  - Replace `»` with `>>`
- **Logging is minimal and single-line.** A log line exists to help pinpoint a failure,
  not to narrate everything happening. Prefix log lines by system so they're easy to find
  in a long device log: `[Tracking] localized in 2.3s`, `[POI] spawned 12/12`,
  `[Config] loaded 45 POIs for wall Panorama`. Strip temporary trace logs once a task is
  verified working — don't leave debug noise permanently wired into shipped code.
- **Never trust a bare print/log line to mean something is actually correct — assert it.**
  If a log message says something like `[ok] wall localized`, back that claim with an
  actual assertion right above or below it, so that if the condition were ever false the
  assertion would fail loudly instead of a log quietly claiming success. Use
  `Debug.Assert` in runtime code and `Assert.IsTrue`/`Assert.AreEqual` (from
  `NUnit.Framework` or `UnityEngine.Assertions`) in tests, placed at the real invariant
  boundaries: config isn't null before use, a POI list isn't empty before spawning, a
  returned pose isn't the zero/identity default before trusting it, IDs are unique before
  building a lookup table from them.

---

## 2.2 Errors
  After implementing things use the Unity MCP to confirm and make sure we have no errors like compilation errors or others. Example using calls like "refresh_unity", "mcpforunity://editor/state", "read_console", or others...








## 3. UI, Visual Design, and Content Rules

- **No hardcoded visual values in C# or UXML.** Colors, fonts, spacing, and corner radii
  are defined once as USS variables or a shared design-tokens ScriptableObject, and
  referenced everywhere else. If the visual style needs to change, it should be a change
  in one place, not a find-and-replace across every screen.
- **No hardcoded user-facing strings in code.** Route all visitor-facing text through a
  strings table asset (even before full multi-language localization is wired up), so
  adding a language later is a data change, not a code change.
- **Every screen has one clear focal point.** A visitor's eye should immediately know
  what to look at or do next. Avoid layouts that present many equally-weighted elements
  competing for attention.
- **Every visual element earns its place.** Every line, margin, icon, and decorative
  touch should serve an actual information or usability purpose. Remove anything that is
  decoration for its own sake.
- **Avoid generic template patterns.** Don't default to predictable card grids or
  boilerplate mobile-app layouts. The interface should look considered and specific to
  this project, not assembled from generic UI-kit defaults.
- **Progressive disclosure over information overload.** Show a small amount of
  information first (a marker, a short label); let the visitor open something to go
  deeper (a full content card). Never dump everything about a point of interest onto the
  screen at once.
- **Accessibility is not optional.** Maintain real contrast between text and background
  in both light and dark conditions (this app is used outdoors in variable light). Any
  screen-reader support (TalkBack/VoiceOver-equivalent APIs) that the project uses must
  be tested against actual accessibility settings, not assumed to work because the
  standard UI components were used.
- **Every core feature must work without the camera.** Since some visitors won't or
  can't use the AR camera view, every feature (browsing POIs, reading content, viewing
  the timeline) must remain reachable through a non-AR fallback mode. Do not build a
  feature that is only reachable through the AR camera path.

---






## 4. Testing Strategy

Two separate questions need separate answers here, and conflating them is what makes AR
testing feel slower than it needs to be: **"how do I iterate on a feature quickly"** and
**"how do I know a piece of logic is actually correct."** §6.1 answers the first, §6.2
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
protocol in §7 for this tier specifically — don't invoke it for anything Tier A, B, or
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
loop in §6.1 or an informal pass, not a maintained automated test.

Run these with Unity Test Framework in batch mode — this needs no human interaction at
all, the agent runs it and reads the results directly:
```
Unity.exe -batchmode -nographics -projectPath "<path>" -quit -logFile compile_log.txt
Unity.exe -batchmode -nographics -projectPath "<path>" -runTests ^
  -testPlatform EditMode -testResults editmode_results.xml -logFile editmode_log.txt
```
Check the compile log for `error CS` lines first — a compile error makes every test
result meaningless — then check the result XML for failures before considering a task
done. Use PlayMode batch tests the same way for the rare case where scene/prefab
wiring itself (not the logic behind it) needs verifying without a human.

When a test fails, stop and think before changing anything: is the test wrong (it's
asserting something that isn't actually the correct behavior), or is the logic wrong
(the code isn't doing what it should)? Write out up to three possible fixes, pick the
best one, apply it, and re-run.

### 4.3 Asset Database Refresh Discipline

Any edit to a `.meta`, `.prefab`, `.asset`, or raw asset file (texture, audio, model)
made through file editing rather than Unity's own Editor UI needs an explicit
AssetDatabase refresh before it can be trusted — Unity does not always detect and
fully re-resolve such changes automatically, and this is documented Unity behaviour,
not a project-specific quirk (see `AssetDatabase.Refresh`'s own scripting reference:
*"You might need to call this method if... you have made changes to assets on disk
from an external application while the Editor is running"*).

This matters more than it looks because of an asymmetry §6.2's batch-mode test
workflow doesn't cover: launching `Unity.exe -batchmode` starts a **fresh** process,
which naturally re-scans the project on that launch — so an agent's own batch-mode
compile/test run can genuinely pass right after an asset edit. But if the person is
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
proceed to Phase B until every Phase A variant is confirmed via §6.5's tiers.

**Phase B — real pipeline integration.** Wire the now-proven-correct system into
the real domain flow. If Phase B shows a problem Phase A didn't, that's a strong
signal the bug is in the integration/config layer, not the rendering system — Phase
A already proved that part correct in isolation; debug the data feeding it, not the
rendering code itself.

**Which recipe, by rendering system:**

| System | Recipe |
|---|---|
| uGUI world-space (Markers and similar) | Grid gallery scene, positioned in 3D, no AR — see `_2_2_Marker_Design.md` §18 for a complete worked example (data-struct-driven entry list, shared harness, PlayMode assertion suite) |
| UI Toolkit screen-space (DetailCard, quiz, toasts, GuideCharacter, Navigation, Sharing) | Two-step: (1) static layout check in Unity's **UI Builder** — zero Play Mode, zero code, catches styling/layout bugs for free; (2) a small runtime harness scene with one `UIDocument`, fed fabricated content variants (long/short text, with/without media, each state) |
| Non-visual integrations (AI API calls, future 3D/animation work) | A minimal sandbox scene or script — call the thing in isolation, log actual request/response/error states; same "isolate before integrating" principle, different mechanism |

**Not every domain needs a gallery.** Build one only when there are genuine
combinatorial variants worth grid-testing. A single-fixed-state element gets one
manual Play Mode check — building gallery machinery for it is the over-engineering
§3 already warns against.

**Shared conventions across all Phase A scenes**: live under `Assets/Dev/<Domain>Gallery/`;
reuse the same backdrop photo asset across every uGUI gallery scene rather than
re-supplying one per domain; use the same data-struct-driven entry-list pattern
(one list of plain data, consumed by both the visual harness and the automated
test suite, so they can never silently drift apart).

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



Tier A, Tier B, and §6.2's automated tests need no human interaction at all — run them
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






## 5.1. Terminal Usage (PowerShell)

- Chain commands with `;`, not `&&`.
- Do not pipe command output through filters (`| findstr`, `| Select-String`, etc.)
  directly in the terminal. Redirect output to a log file instead
  (`> __out.txt 2>&1`), then read the file. This keeps the full, unfiltered output
  available if something unexpected needs investigating.

---

## 5.2. Tool & Context Usage Guidelines

### Tool Execution & Argument Validation
- **Strict Parameter Compliance:** Always verify tool schemas before executing. Never emit missing required parameters (e.g., `path` for `read_file` or `write_to_file`).
- **File Reading Limits:** Do NOT attempt to read entire large files directly with `read_file`.
  - For target edits/searches, use `search_files` or `codebase_search` first.
  - When using `read_file` on large files, always specify `start_line` and `end_line` ranges.
- **Error Handling & Pivot Strategy:**
  - If a tool execution fails or returns an error response, analyze the error output immediately.
  - Do NOT repeat the exact same failing tool call with identical arguments.
  - If a tool continuously fails, pivot to an alternative tool or execute a shell command via `execute_command` (e.g., fallback file inspection).







## 6. Testing Strategy

Two separate questions need separate answers here, and conflating them is what makes AR
testing feel slower than it needs to be: **"how do I iterate on a feature quickly"** and
**"how do I know a piece of logic is actually correct."** §6.1 answers the first, §6.2
the second. Do both — they're not alternatives to each other.

### 6.1 The Development Loop — Iterate Fast, Spend Real-Device Time Only Where It's Needed

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
protocol in §7 for this tier specifically — don't invoke it for anything Tier A, B, or
C could have answered instead.

### 6.2 Automated Correctness Tests — What Actually Gets a Unit Test

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
loop in §6.1 or an informal pass, not a maintained automated test.

Run these with Unity Test Framework in batch mode — this needs no human interaction at
all, the agent runs it and reads the results directly:
```
Unity.exe -batchmode -nographics -projectPath "<path>" -quit -logFile compile_log.txt
Unity.exe -batchmode -nographics -projectPath "<path>" -runTests ^
  -testPlatform EditMode -testResults editmode_results.xml -logFile editmode_log.txt
```
Check the compile log for `error CS` lines first — a compile error makes every test
result meaningless — then check the result XML for failures before considering a task
done. Use PlayMode batch tests the same way for the rare case where scene/prefab
wiring itself (not the logic behind it) needs verifying without a human.

When a test fails, stop and think before changing anything: is the test wrong (it's
asserting something that isn't actually the correct behavior), or is the logic wrong
(the code isn't doing what it should)? Write out up to three possible fixes, pick the
best one, apply it, and re-run.

### 6.3 Asset Database Refresh Discipline

Any edit to a `.meta`, `.prefab`, `.asset`, or raw asset file (texture, audio, model)
made through file editing rather than Unity's own Editor UI needs an explicit
AssetDatabase refresh before it can be trusted — Unity does not always detect and
fully re-resolve such changes automatically, and this is documented Unity behaviour,
not a project-specific quirk (see `AssetDatabase.Refresh`'s own scripting reference:
*"You might need to call this method if... you have made changes to assets on disk
from an external application while the Editor is running"*).

This matters more than it looks because of an asymmetry §6.2's batch-mode test
workflow doesn't cover: launching `Unity.exe -batchmode` starts a **fresh** process,
which naturally re-scans the project on that launch — so an agent's own batch-mode
compile/test run can genuinely pass right after an asset edit. But if the person is
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

### 6.4 Phase A/B — Isolated Verification Before Integration

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
proceed to Phase B until every Phase A variant is confirmed via §6.5's tiers.

**Phase B — real pipeline integration.** Wire the now-proven-correct system into
the real domain flow. If Phase B shows a problem Phase A didn't, that's a strong
signal the bug is in the integration/config layer, not the rendering system — Phase
A already proved that part correct in isolation; debug the data feeding it, not the
rendering code itself.

**Which recipe, by rendering system:**

| System | Recipe |
|---|---|
| uGUI world-space (Markers and similar) | Grid gallery scene, positioned in 3D, no AR — see `_2_2_Marker_Design.md` §18 for a complete worked example (data-struct-driven entry list, shared harness, PlayMode assertion suite) |
| UI Toolkit screen-space (DetailCard, quiz, toasts, GuideCharacter, Navigation, Sharing) | Two-step: (1) static layout check in Unity's **UI Builder** — zero Play Mode, zero code, catches styling/layout bugs for free; (2) a small runtime harness scene with one `UIDocument`, fed fabricated content variants (long/short text, with/without media, each state) |
| Non-visual integrations (AI API calls, future 3D/animation work) | A minimal sandbox scene or script — call the thing in isolation, log actual request/response/error states; same "isolate before integrating" principle, different mechanism |

**Not every domain needs a gallery.** Build one only when there are genuine
combinatorial variants worth grid-testing. A single-fixed-state element gets one
manual Play Mode check — building gallery machinery for it is the over-engineering
§3 already warns against.

**Shared conventions across all Phase A scenes**: live under `Assets/Dev/<Domain>Gallery/`;
reuse the same backdrop photo asset across every uGUI gallery scene rather than
re-supplying one per domain; use the same data-struct-driven entry-list pattern
(one list of plain data, consumed by both the visual harness and the automated
test suite, so they can never silently drift apart).

### 6.4.1 Edit-Mode tooling parity (when applicable)

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
  gap. (Worked example: `_2_2_Marker_Design.md` §19.13 — the Living Room
  POI authoring tool positioned markers correctly for a long time while
  never once calling `MarkerView.Initialise`, so every Editor-mode preview
  silently showed the prefab's raw default look, not the actual configured
  marker.)

Verify with the same evidence discipline as §6.5, adapted to this context:
Tier 0 = does the tool's populate/refresh code path actually call the same
real entry point the Play-Mode path uses (read the code, don't assume it
does just because Play Mode works); Tier 1 = does the Scene view show the
correct *static* composition immediately after a field changes, with zero
Play Mode entries — not whether it animates, which isn't expected here.

### 6.5 Verification Tiers and Evidence Standards

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
