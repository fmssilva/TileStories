

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

## From the Architect review of 9-pre + 9A (2026-09-29)

- `Markers/Fonts/Oswald Bold SDF.asset` changes on test runs (-944 lines: its dynamic glyph table and atlas are rewritten). A
  TextMeshPro font in Dynamic mode saves whatever glyphs a run happened to use. Keep it out of commits (`git checkout -- <file>`)
  and, in the marker domain, make it Static with the needed character set (Latin + Portuguese) or clear dynamic data on build.
- Two Worker sessions in a row began from an interrupted, uncommitted predecessor (8B earlier, 9A now). Sessions that end with
  "audit the WIP" cost a whole run. Keep each brief to ONE step, and have the Worker commit-suggest after each Part.
- The MCP-for-Unity bridge can keep an orphaned test job after Unity is idle; every new run is then refused. `editor_state` +
  `get_test_job` diagnose it; restarting the Editor clears it. Worth a line in `50-terminal_and_tools.md` if it happens again.

## From _3.1 9A-fix / 9B (2026-09-29)

- [zoom domain] The AR zoom strip's labels ("Zoom out", "Fit", "Zoom in") are wider than their 48 px round buttons and spill past
  them (every Search_* capture with the strip shows it). An icon per button, or pill-shaped buttons sized to the word, would fix it.
- [testing] It happened again, in another form: the whole EditMode baseline was red with Unity's own `[Assert] Access version should be
  odd when acquiring lock` (from `UnityEditor.TestRunner` internals, on every real-click Editor test), identical over four runs, focused
  or not; only an Editor restart cleared it. Proposed line for `40-testing.md` 4.2.2/4.2.3: identical failures whose message comes
  from Unity's test framework and that include tests unrelated to the change -> one confirming rerun, then ask for an Editor restart;
  do not chase focus (the suites are meant to pass in the background). Also: close stray `*Tests+Host` windows before a baseline run.
- [tooling] When Claude Code's auto-mode permission check stops answering ("no verdict"), every mutating call (Bash, execute_code,
  run_tests...) fails and ten in a row end the turn. After two, stop mutating calls and ask the developer (switch permission mode, or
  wait); reads still work. Proposed for `50-terminal_and_tools.md`.
  ROOT FIX (2026-09-29): the developer added `.claude/settings.local.json` with `{"permissions":{"allow":["mcp__UnityMCP"]}}` --
  allowed tools skip the check, so a check outage no longer stops Unity work (file edits and Bash still go through it).

## From the stability pass (2026-09-29)

- [LOD / demo field] At the phone's own frame (390 x 844, portrait) only 3 of the 15 LOD demo-field markers are on screen from
  the stage start: the default field (5 m wide at 1.5 m) is laid out for the Editor's landscape Game view. The field is also
  allowed in development builds, where a phone in portrait sees a narrow slice of it. Decide in the LOD domain: size the default
  field from the camera's own view (fit the width), or keep it Editor-shaped and say so in its help. `LodWallSceneTests` now
  takes an explicit 1280 x 720 frame for this test (40-testing 4.2.3).
- [MCP config] `~/.claude.json` holds two project entries for this folder, `C:/...` and `c:/...` (drive-letter case). The VS
  Code extension uses the lowercase one, whose project-scoped `UnityMCP` URL is written `http:\127.0.0.1:8080\mcp` (works: URL
  parsers read `\` as `/`). The user-scoped entry (`http://127.0.0.1:8080/mcp`) points at the same server. Cosmetic; tidy it with
  VS Code closed if it ever matters.

## Repo (2026-09-30)

- **Git LFS is not in effect.** The `[attr]lfs` macro is defined in `TileStories/.gitattributes`, but git allows macro
  definitions only in the TOP-level `.gitattributes`, so `*.png lfs`, `*.mp4 lfs` etc. set nothing (git warns "not allowed" on
  every command). Every binary is committed as a normal blob (checked: the 17 MB castle video and the PNGs are raw in HEAD). To
  use LFS: move the macro lines to a root `.gitattributes` (or write `filter=lfs diff=lfs merge=lfs -text` directly), install
  git-lfs, and run `git lfs migrate` if the history should shrink. Developer's decision (needs a remote that supports LFS).
