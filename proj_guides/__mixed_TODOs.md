

synonyms not implemented

add the button create new POI in the specific mrker sub tab

add a button create empty json config that "loads" an empty default json config to the scene with 1 default specific poi marker also... 

install de voice search thing

check thee FOV splike thing???


## From the POI Detail Card review (2026-09-27)

- Flaky EditMode test seen twice across sessions: `TaxonomyRowIdentityTests` (`...ReportsBothRowsByPosition`,
  `...TypingADuplicateAndABlankCategoryLabel_...`), right after leaving Play Mode. Likely leftover window /
  undo state between tests; investigate in a taxonomy session. Same pattern seen 2026-09-27 on
  `KeywordListFieldTypingTests.TypingTwoKeywordsWithACommaBetween_GivesTwoKeywords` ("the click must focus the keyword
  field") in an EditMode run started right after a full PlayMode run; 1207/1207 on the immediate rerun. Suspect: Editor
  window focus still settling after Play Mode exits (real-click tests need a focused window).
- Commit hygiene: commit after every verified agent session (456 files were uncommitted after the card 6A session);
  keep `IPCE/` out of TileStories commits (separate repo or .gitignore).
- Search results rows show "category - level" (`PoiSubtitle`), so a hierarchy level name ("Hub", "Landmark")
  reaches visitors. Level names are authoring vocabulary. Decide in the Select/Filter/Search domain: category
  only, or a per-level "visitor label" field. (The card header gets its own fix in `_3.1` [6C].)


## From the POI Detail Card 6C + Tier 2 group A session (2026-09-27)

- `Editor.log` is 3.9 GB (`%LOCALAPPDATA%/Unity/Editor/Editor.log`): every compile check greps its tail and a search for an
  import error from an hour earlier needs the last 400 MB. Unity truncates it on restart; restart the Editor now and then, or
  add a small Editor-only "compile check" tool that reads only the lines after the last compile start.
- A reusable Editor capture helper (open the window, scroll to a section, `ReadScreenPixel` cropped to the part of the window
  that is on screen) would save every UI session rebuilding it by hand in `execute_code`; keep it Editor-only, in `Editor/Dev/`.
- Flaky tests keep appearing "right after leaving Play Mode" (TaxonomyRowIdentityTests x2, KeywordListFieldTypingTests).
  Common suspect: STATIC state that survives between tests -- `SelectionEventBus` (static events + CurrentPoiId),
  `BlockRegistry.Shared`, `EditorNotice` queues. A small shared `[SetUp]/[TearDown]` reset (or a test base class)
  for every static bus would make these deterministic. One focused session, all domains.
- Interrupted agent sessions leave the tree broken: the card 7A session found the previous (interrupted) run had left
  code that did not compile and 22 red tests. Commit after every verified session so an interruption costs only
  the current session.
- Two notions of "where a POI sits on the wall": `WallAxisRule` (Runtime/POI, principal axis on the floor plane;
  card wall_locator + related) and `MinimapLayout`'s projection (wall x/y vs floor x/z, widest spread). They can
  disagree about left/right. In the minimap domain, consider building the minimap's wall projection on
  `WallAxisRule` so the minimap and the card's strip always agree.
- Agent session size: two card sessions in a row were interrupted mid-way and left uncommitted, non-compiling
  work for the next agent. Keep one session = one close-out-able scope (about 4-6 block kinds), and make the
  first step of every prompt "verify the tree compiles and is committed".


## From the POI Detail Card step 7C + Tier 3 group A session (2026-09-28)

- A stale zero-byte `.git/index.lock` blocked the first commit of the session (no git process was running). A crashed
  editor git integration leaves it behind; worth knowing before assuming a broken repo. Check with `ls .git/*.lock` and
  `tasklist | grep git`, then remove it.
  Cause found 2026-09-28: the reviewer's read-only `git status` from the Cowork VM (mounted folder, no delete right)
  creates `index.lock` and cannot remove it. Reviews now use `git --no-optional-locks status`; if a lock is there after a
  review, it is safe to delete when no git process runs.
- The `_5.1` capture recipe multiplied a window's rect by `pixelsPerPoint`; on the 1920 x 1080 / 1.25 machine that captured
  another application. The recipe now says to open the first capture and try the other scale. The Editor capture helper
  idea above (`Editor/Dev/`) should read the real scale from a known-good probe (capture a known corner, compare) instead
  of assuming one.
- `LogCardEvents` (one log line) is the placeholder behind `ICardEvents`: the work plan's Stage 3 telemetry (Type A / B
  events, consent) replaces it. The card raises `feedback`, `poll` and `collect` events today (`CardEventKinds`, 8A / 8B);
  answering a knowledge question is a natural next event once the consent story exists.
- `PlayerPrefs` keeps the visitor's answers and votes on the device only (`CardLocalState`); there is no export, no
  cross-device sync and no per-visitor profile. If the evaluation protocol (`_7.1`) wants per-visitor answer data, that is a
  telemetry decision, not a card one.
- `LivingRoomScene.unity` was found saved at 3.9 MB (spawned marker rig objects, e.g. `RippleMiddle`) against 58 KB in git in the middle of
  the 8A-fix / 8B session, with the scene open and clean in the Editor and nothing in the session saving it on purpose. It was restored from
  HEAD (plus the one stylesheet line) and stayed small through every later test run. Suspect: Unity's scene auto-save catching a rig
  populated in the open scene by an EditMode test or the POI Editor's Load & Populate Rig. Worth finding before it lands in a commit: check
  `git diff --stat` on the scene each time, and see whether an EditMode fixture should close or reload the scene it populates.
- `EditorNotice` does not block the Editor with a native dialog (it queues an in-window popup) and its test switch is
  `EditorNotice.ShowPopups`; a UnityMCP capture script that loads the wall config should set it false around `LoadConfig`, as the 8B
  captures did, so nothing floats over the window being captured.
- Real touches drive UI Toolkit's `ScrollView` even while a child holds the pointer capture; a child that wants to keep vertical
  scrolling alive should still not capture at touch-down (the card then swallows the finger for its own gestures elsewhere, e.g. inside a
  horizontal ScrollView). `SwipeGrabRule` is the pattern for the next swipeable block.

## From the Architect review of 8A-fix + 8B (2026-09-28)

- Reviews from the Cowork VM see every CRLF file as modified unless git runs with `-c core.autocrlf=true`; the Windows side is
  clean. Not a repo problem; the Architect command (`__AI_Architect.md`) says so.

## From the Architect review of step 11 (2026-09-28)

- Repo hygiene, developer's call (the evidence clean-up of 2026-09-28 left these alone): tracked files `TileStories/Assets.7z` (48 MB
  backup), `TileStories/__orientation_screenshot.png`, `edit_file.py`, `Fundo Desktop.jpg`.
- Commit scope: `git add TileStories .clinerules proj_guides` instead of `git add .`, so `IPCE/`, `report/` and
  `flutter_prototypes/` never ride along by accident.

## From the POI Detail Card evidence clean-up, step 11-fix and step 12 (2026-09-28)

- The Block Library's family order is derived, not chosen: families sort by the order they first appear in the registry (about, stories, visit,
  meta, media, play, community, ar), so `meta` (Sources) now sits before the picture, play and community kinds. Fine for a first grouping;
  if the catalog should read differently (Sources last, say), give a family an explicit order (`BlockRegistry.Ordered`) instead of hoping
  registration order says it.
- `LivePlayModeConfigPush` applies EVERY domain on the first push of a Play run (a new wall knows nothing of earlier pushes). That is fine as
  long as each applier's runtime seam is idempotent; `WallSession.RebuildDemoField` was not (it cleared the selection even with no demo on, so the
  developer's first edit closed the open card) and was fixed in step 12. Worth one look at the other `Apply...` seams when a domain next needs to
  keep something across a push (the LOD / zoom / displacement seams were not audited for this).
- An EditMode `[UnityTest]` can enter Play Mode (`yield return new EnterPlayMode(expectDomainReload: false)`, this project has domain reload off)
  and then drive the REAL POI Editor window against the running scene: `LivePlayModeCardTests` is the pattern for the live-sync Phase B of any other
  domain. It needs `UnityEditor.TestRunner` in the Editor test asmdef; the runner's own scene is the open one, so load the wall scene with
  `EditorSceneManager.LoadSceneAsyncInPlayMode`.
- `Open Gallery` in Edit Mode replaces the open scene (after refusing to do so over unsaved changes). Loading the gallery additively, or in its
  own window, would keep the wall scene open beside it; not needed yet.
