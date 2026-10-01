

# `FUTURE TASKS OR THINGS TO CONFIRM`


synonyms not implemented

add the button create new POI in the specific mrker sub tab

add a button create empty json config that "loads" an empty default json config to the scene with 1 default specific poi marker also... 

install de voice search thing

check thee FOV splike thing???




about this:
__Section 4.2 Decision:__ Should manually-typed positions be tagged as "manual" in captured_position_source (recommended default)?
I think yes.
but then, what are we going to do with this "flag"? should we for example just add some "?" to the POI marker labels, or add some "circle" around the POI marker or some other visual marker so the developer in the edit mode can clearly see the markers that were already well positioned in the glb vs the ones that are still just interpolated from the x,ynorm or just hand writen positions...?? or what are the best options to make a good "positioning whole system"? 

Question 1 — What do we do with the "manual" tag once it exists?
Agreed it should be tracked. The question is what surfaces it — and I'd draw a hard line here: this stays entirely in Editor-only tooling, never in the runtime/visitor-facing marker. Putting a "?" or a ring on a marker a real museum visitor sees would leak internal QA metadata into the shipped experience — exactly the Framework/Apps, Editor/Runtime boundary this project has protected everywhere else (EditorOnly tags, "never shown to a visitor," etc.). A visitor doesn't care whether a position was field-captured or interpolated; a developer deciding what's left to do very much does. Two different audiences, two different places.

Concretely, three additions to the Authoring Tool window (all Editor-only, none touching MarkerView.cs or anything that ships):

Color-code the OnSceneGUI sphere handles the tool already draws — green for field-captured (Workflow A/B), orange for manual, grey/blue for still-interpolated-not-captured. This is the single highest-value addition, since it's visible at exactly the moment someone is deciding what to place next.
Annotate the POI list in the tool window with the same status, as a short prefix or colored label per row — this is where someone reviews "what's left."
A one-line summary at the top of the window: "3/3 POIs captured (0 manual) - 2/2 calibration anchors captured" — cheap, and gives an at-a-glance completeness check before considering a wall done.
Worth noting for later, not building now: this same signal is exactly what a Stage 4 baker validation check should refuse to bake on — "don't let a wall ship with manual or uncaptured positions" is a natural extension of the NaN/Infinity checks already planned for B3, just not needed yet.











about the whole process to positioning markers: Use TileStories > POI Editor:
Load config → Populate Rig from JSON → Position markers using sphere handles → Capture Positions to JSON → Save config → Copy to StreamingAssets → Build & Run...
One thing that the "normal unity developer will do" is to just populate the rig, and then just go to the hierarchy and click each POI and position it, and then click "Ctrl+S" and he will think everything is well done and saved to the config json?? is this probable to happen? so what should we do? maybe we can save to config json automatic when the user saves normally... so when the unity saves the prject at some moment we always save things to the config json automatic? is this good?


Question 3 — The Ctrl+S trap. Real risk. Auto-capture-on-save is the wrong fix, though.
This is a sharper catch than the rig-cleanup issue, honestly — someone can place markers, hit Ctrl+S out of habit, close Unity, and not discover config.json is stale until much later, with no visual signal anything's wrong in the meantime.

I'd argue against silently auto-running "Capture Positions to JSON" on every scene save, for a specific reason: it trades one silent failure for a different one. If someone saves mid-experiment — dragging a marker around, hasn't found the right spot yet, saves for an unrelated reason — an auto-capture would bake that half-placed position into config.json without them ever deciding to finalize it. "Capture" is currently a deliberate act; making it an invisible side effect of an unrelated action (saving the scene) removes that deliberateness in both directions, not just the one you're trying to fix.

Better version of the same instinct: make the mistake loud instead of invisible, using the exact same EditorSceneManager.sceneSaving hook already planned for the rig-cleanup check (Task 2.3), upgraded to actually compare state instead of just checking presence:

On scene save, for every marker currently under POIEditorRig, compare its current scene position against what's currently saved in config.json for the matching POI id (using a distance threshold like Vector3.Distance(a, b) > 0.001f, not exact equality — float precision noise would otherwise cause constant false alarms).
If anything differs, show an actual EditorUtility.DisplayDialog — a blocking popup, not a Console line easy to scroll past — with a "Capture Now" button right in the dialog that runs the capture immediately, one click, the moment the problem is caught.
This keeps the person in control (they click, nothing happens invisibly) while making the failure mode almost impossible to walk away from unnoticed. A cheap complementary touch: an asterisk in the tool window's title when there are uncaptured changes, matching the "unsaved document" convention from basically every text editor — an ambient reminder, not just a save-time interrupt.













then the new marker configured in the config json and well flexible and editable... 		

	size of things: dev defines in json the size of the markers, and then in the editor panel we allow to adjust that size ... 	

		and, the size of markers, even if they are ugui elements in the word, can we make their size in relation to the screen? because if we set the size like in real cm in the world, then, if the user is very near they will be very big, if the user is faraway they will be very small. so lets have the size to be set up in pixeis in screen or something? can we do that? 

		








	hierarchy of levels of categories ... and so, in practice we can have it clear in the guide that in practice we have 3 kinds of categories of markers: we have the first category the dev wants (ex type of buildings - religious, military... ), other second category (example level of destruction, or price or whatever... ), and then the "relevance category" which determines the markers that are bigger and the ones that are smaller... 	

		you said 2 levels, but maybe lets have more levels. lets have level 1 with big markers and labels, level 2 with still big markers but a bit smaller and without labels, level 3 with even smaller markers, and level 4 only little markers really 

		i think to simplify, we can have the same kind of marker logic at all levels. instead of having symbols in big markers and only circles in small markers... no, lets have symbols in all levels... so we have the same logic for all kinds of markers and  we just change the size... 

		and maybe even, we can, instead of hard code some possible number of levels, we can leave it flexible and allow for the dev to add as many levels as he wants, considering the marker is always the same and we just adjust the size, we can allow this flexibility easy yes? 

		and maybe as default for sizes we can take the bigger size and then divide by 2 or other ratio to calculate the smaller levels markers... ?? and we can also allow in the editor panel to adjust this ratio...??? 





now in the editor tab, can we have some undo option in case for example we delete some category or some other "hard" change, so we are able to undo it with some button or with Ctl+Z? 

now about the symbols, we currently have them right inside the runtime folder:
C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Runtime\UI\Markers\Icons
and
C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Runtime\UI\Markers\SymbolCircle.png

but these are png images. this is "assets" or something like that. AND i want it to be possible for the developer to add new symbols, new png images as he wants to use in a new wall. 
so what is the best way to have this well organized and clean? first of all, does the runtime needs these png images here inside this folder? or could it read these images from anywhere? example from the respective wall folder?? and so do a deep analysis and think what is the best way for us to "offeer these default symbols imges", but then allow for new images to be added? maybe the developer can add images to the wall folder, example to a folder like C:\Users\franc\Desktop\TileStories\TileStories\Assets\Apps\LivingRoom\MediaAssets\Images\marker_symbols
and then our editor is able to get them from there? and then if needed copy them to the runtime folder??? or even just leave them there in the wall folder? and if needed copy the defaults from the runtime folder to the wall folder...?? or what to do? how can we have default symbols and also allow to add any kind of new symbol imge in an easy way so a user can add them and see them right away in the editor mode? 
or maybe the developer can just drag the imges he wants from the project asets tab directly to the editor tab and that action directly copies those images to the final destination... example the runtime folder if needed or other place??
and this choosing of image, for the developer should be done by actually draging the image or then eearching for it by name, and not by "key", so it is intuitive and easy to use... and so in the editor tab, instead of having those 3 colums for the category name and symbol key... maybe we can have just the ategory name and then the symbol name of the png file, and then also we show directly the symbol imge on the third colum so we have a preview of the symbol. can we do that? and so when we add some new category we should be able to have some + button that we can click and go and find the image we want to use for the symbol category... and be able to drag and drop the image from the project tab in unity directly to that spot... ??? see the best options and ways to do this. 

and for the badge we can use exacly the same thing. the same "editor component" we can use for category symbols and for badge symbols, because they are exacly the same... only the size really changes... so keep things simple... create a good editor component maybe and then we can use it for both the category symbol "enum definition" and also for the badge "enum definition"?? 



---
and then, we need to reorganize the whole POI Editor tab into a good and well organized set of components. because currently everything is in the first level, and always visible. we need to organize things by groups or by components and so we can have some "expland" and minimize button in each component and so we can hide or expand that component information. 

and so, for starters, we should put all those buttons of "big actions" right on top of the POI Editor.
so we have the config path, the marker prefab, the correction anchor, the wall mesh reference... 
and then we have the buttons:
load config (and here... should we allow to add the path or to click in some "+" button and so we add the specific config we want? currently maybe it is a bit hard coded?? and we should be able to select the config we want from our wall or from any new wall??)
then the buttons populate and clear rig and capture positions to json and save config and copy streamingassets buttons all of them here together on top of the POI Authoting.
and about these button... lets review how many buttons and for what functions we have them?? 
example, as soon as we click in populate the rig button, can we right away in automatic paint the "clear the rig" button in red or something or orange with some warning symbol, so the developer knows that he should clear the rig before running the app to avoid duplciates... and even we can add this message bellow to be clear to the developer why he should clear th rig before runing...?? 

And then we have capture positions to json and save config... lets maybe just have a button "Save configs to json" or something like that? and so when we click it we save evrything to json (positions, symbols, styles etc.. basically evrything...) (and this button should also become orange or yellow with some warning symbol and some message, automatic, as soon as we do some change in the POI Editor... if we change ome position or style or something... so this button become yellow so the developer knows that he should save things...??)
and then the button copy to streamingassets is ok. 
and then we can remove the button select all rig objects (we dont need it)

and so these global button are always visible on top of the POI editor tab...

then we have a big componnt bellow, with a scrooll bar on the side, and we put everything else inside it (and with a scrool because we'll have many things and we won't b able to see them all without a scrooll bar)


and so inside this big scrollable container we should have 2 big collapsible components:
one for 
"Global Scene Options" or something like that, 
and then another for "specific Markers options"...


and inside the global walls component we should have the respective configurable options... and all organized by groups or components like:
a component "Marker". and then we expand it and we see the line to select the "marker shape", and then the "Category Symbols" component with the whole symbols enum definition...

then a component "Badge Symbols" which is by itself a line with a toggle button, and so in a easy and clear way we can select or deselect to ue bdges. and then we can click in that line to expand that componetn and IF we select the badge option ON, so then yes we can see the whole badge enum options there also...
then the "Outline" which is by itself a line with a toggle button, and so in a easy and clear way we can select or deselect to use the outlines. and then we can click in that line to expand that component and IF we select to use a outline, so then right bellow we have a line saying "outline color" and it is a drop down and we can choose gold or hue. (so currently we have a simple drop down with gold and hue and none... lets break that up in a toggle that says use a outline or not, and then a drop down with the outline options we have - gold and hue)... and then bellow that drop down of the line color, we have the section of the "outline styles" and there we can define the number of levels we want, the label for each level (some input text free), and the kind of line (the drop down with the available options) (we only need these 3 simple fields - we don't need more). AND we should cap the "add more levels" options to the available number of line types + 1, so we can use no line and each line type... and when we have no more so we can't add new line types... and we should also allow to remove line types levels... 


and then in the "specific marker options" we have a inner component for "position" and another for "marker style" or something like that. 


so do a deep analysis and see the best options and best way to implement all this in a clean and simple and good looking and good editor UI well organized tab way, easy to work and understand and navigate. 





## Label Board / Help Button 
» fazer label board in ui toolit pra mostrar automatico primeira vez que corremo app e dp usar pode escolher fechar e escolher nao voltar a motrar... 


## SIZE ADJUST WITH DISTANCE - CONFIRM EVERYTHING

lets create a size and resize domain?? where we set the size of markers and lables of each hierarhcy level and we adjuts the distance scaling? ?? should we have this domain on its own or better to just keep things more closed to the current domains like marker, label, hierarchy, LOD? 

## LOD - show low level hierarchy labels if space is empty 
LOD - lets also add a "add labels" funtionality, so if we have only hierarhcy 4 or 5 markers in some area, but there is good space between them, so maybe we can show some labels anyway? so when we have "empty screen" we can add more info and so we can add some labels??? is there a way to check the "empty screen level" per region of screen and decide if we add some label or not??? or is there some feature related with this that we should add to our framework??? 

## where is the zoom domain file? 


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
- **Real-typing EditMode tests depend on Unity's focus.** `LivePlayModeCardTests.TheRealWindow_WhilePlayModeRuns_ATypedText...` loses
  characters ("Dis" for "Dismiss", "L" for "Live heading") when a full run happens with Unity in the background (seen 9B-fix and again
  2026-10-01; 5/5 alone with focus). Either give these tests their own category that a background run skips, or have the typing helper
  wait until each character has landed before the next. Test-infrastructure work, not a card domain change.

## Process (2026-10-01)

- [thesis] Re-check the POI card plan (`_3.1`-`_3.3`) against the literature: for the main decisions (block catalogue, one
  owner per medium, bottom-sheet card, takeover, Editor-configurable options, sunlight contrast) find supporting or contrary
  sources and record the citations in the guides' References, for the thesis report. A `Domain_Planning.md` re-check pass.
- [repo] Archive candidates for the next `Guidelines_Review.md`: `_2.6.3__curr_plan_tracker copy.md`, old `_2.x` Vision / Human
  test files that are fully done.
