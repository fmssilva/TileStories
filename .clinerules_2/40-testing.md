
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

This matters more than it looks because of an asymmetry §4.2's batch-mode test
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
