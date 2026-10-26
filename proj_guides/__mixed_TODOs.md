

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

about this size of markers, a risk of having them to follow the natural depth distance size is that if i am staying at 8m frm the wall, so maybe ll the markers will be little small dots merely visible... so maybe we can have some sort of check like: we take the higher priority marker and we make sure that those markers are at least, even if very far away from user, they are maybe min of 10px or smething like that?? (don't knw what is a good number...)
and maybe also for the oposite thing, if i go with the phone and move like at 1 cm from the wall, thee POI marker if it has for eexample some 30cm in the scene world, it will basically cover the whole screen. so maybe lets als have a max size of poi when nearby to be for example 40px... ?? mayeb these min when far and max when close can also be tunable by thee develper in the editor tab... maybe in the hierarchy domain?? considering we set the markers size for each level there??? and then we can add these hard limits to the "higher" hierarchy level example priority 1, and then we apply the normal "ratio rules" t the other levels??? or maybe we can have these hard limits to all levels, to make sure for example the levl 5 doesn't have some 1px markers and always have at least some 5px or smething????



## LOD - show low level hierarchy labels if space is empty 
LOD - lets also add a "add labels" funtionality, so if we have only hierarhcy 4 or 5 markers in some area, but there is good space between them, so maybe we can show some labels anyway? so when we have "empty screen" we can add more info and so we can add some labels??? is there a way to check the "empty screen level" per region of screen and decide if we add some label or not??? or is there some feature related with this that we should add to our framework??? 

## where is the zoom domain file? 


## From the POI Detail Card review (2026-09-27)

- Flaky EditMode test seen twice across sessions: `TaxonomyRowIdentityTests` (`...ReportsBothRowsByPosition`,
  `...TypingADuplicateAndABlankCategoryLabel_...`), right after leaving Play Mode. Likely leftover window /
  undo state between tests; investigate in a taxonomy session. Same pattern seen 2026-09-27 on
  `KeywordListFieldTypingTests.TypingTwoKeywordsWithACommaBetween_GivesTwoKeywords` ("the click must focus the keyword
  field") in an EditMode run started right after a full PlayMode run; 1207/1207 on the immediate rerun. Suspect: Editor
  window focus still settling after Play Mode exits (real-click tests need a focused window). Seen again 2026-10-16 (`_3.2` step 6):
  the same precondition failed in a full EditMode run started seconds after the agent CLOSED its POI Editor capture utility window
  (focus moved), 1754/1755; passed alone at once. So the trigger is any recent focus change, not only Play Mode exiting.
- Keep `IPCE/` out of TileStories commits (separate repo or .gitignore).
- Search results rows show "category - level" (`PoiSubtitle`), so a hierarchy level name ("Hub", "Landmark")
  reaches visitors. Level names are authoring vocabulary. Decide in the Select/Filter/Search domain: category
  only, or a per-level "visitor label" field. (The card header gets its own fix in `_3.1` [6C].)


## From the POI Detail Card 6C + Tier 2 group A session (2026-09-27)

- A reusable Editor capture helper (open the window, scroll to a section, `ReadScreenPixel` cropped to the part of the window
  that is on screen) would save every UI session rebuilding it by hand in `execute_code`; keep it Editor-only, in `Editor/Dev/`.
- Flaky tests keep appearing "right after leaving Play Mode" (TaxonomyRowIdentityTests x2, KeywordListFieldTypingTests).
  Common suspect: STATIC state that survives between tests -- `SelectionEventBus` (static events + CurrentPoiId),
  `BlockRegistry.Shared`, `EditorNotice` queues. A small shared `[SetUp]/[TearDown]` reset (or a test base class)
  for every static bus would make these deterministic. One focused session, all domains.
- Two notions of "where a POI sits on the wall": `WallAxisRule` (Runtime/POI, principal axis on the floor plane;
  card wall_locator + related) and `MinimapLayout`'s projection (wall x/y vs floor x/z, widest spread). They can
  disagree about left/right. In the minimap domain, consider building the minimap's wall projection on
  `WallAxisRule` so the minimap and the card's strip always agree.


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

## From the Architect review of step 11 (2026-09-28)

- Repo hygiene, developer's call (the evidence clean-up of 2026-09-28 left these alone): tracked files `TileStories/Assets.7z` (48 MB
  backup), `TileStories/__orientation_screenshot.png`, `edit_file.py`, `Fundo Desktop.jpg`.

## From the POI Detail Card evidence clean-up, step 11-fix and step 12 (2026-09-28)

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
  CLOSED 2026-10-19 (_3.2 step 13): both fonts Static, see "The two SDF fonts dirty themselves on every Unity launch" below.

## From _3.1 9A-fix / 9B (2026-09-29)

- [zoom domain] The AR zoom strip's labels ("Zoom out", "Fit", "Zoom in") are wider than their 48 px round buttons and spill past
  them (every Search_* capture with the strip shows it). An icon per button, or pill-shaped buttons sized to the word, would fix it.
- [testing] It happened again, in another form: the whole EditMode baseline was red with Unity's own `[Assert] Access version should be
  odd when acquiring lock` (from `UnityEditor.TestRunner` internals, on every real-click Editor test), identical over four runs, focused
  or not; only an Editor restart cleared it. Proposed line for `40-testing.md` 4.2.2/4.2.3: identical failures whose message comes
  from Unity's test framework and that include tests unrelated to the change -> one confirming rerun, then ask for an Editor restart;
  do not chase focus (the suites are meant to pass in the background). Also: close stray `*Tests+Host` windows before a baseline run.

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

## The two SDF fonts dirty themselves on every Unity launch (CLOSED 2026-10-19, _3.2 step 13)

- Both `Oswald Bold SDF.asset` and `Roboto-Bold SDF.asset` are STATIC now (`m_AtlasPopulationMode: 0`), each with the same 122 characters in
  one 1024 atlas: printable ASCII, the Portuguese letters (upper and lower case), and every character of `CardStrings.asset`, the app's
  `LivingRoomCardStrings.asset` and the wall's `config.json` (only the middle dot was beyond the base set). Rebuilt once with
  `TMP_FontAsset.ClearFontAssetData` + `TryAddCharacters` (nothing missing from either .ttf) + Static; no glyph dropped.
- Guard: `ShippedTextGlyphTests` (EditMode) asserts both are Static and hold every shipped character, naming the character and its source
  when one is missing; it was red before the rebuild (Dynamic) and green after. A new language or accent: run it, then add the characters
  with the same two calls (the test's `ShippedCharacters()` is the set).
- Proof: the developer closed and reopened Unity; both files kept their SHA-1 through the launch and a Portuguese PlayMode run
  (`PoiCardPortugueseTests` + `MarkerGalleryTests`, 14/14). The GATE step "git checkout -- the fonts" is no longer needed.

## Editor-window tests fail on a screen that cannot give them their width (2026-10-22, from the _3.2 chunk C+F review)

- Four Editor tests that need the POI Editor window at 880 pt failed at that session's baseline because the window was clamped
  to 849.6 pt: the primary screen reported 1080 x 1920 at 125%. They passed later, on a different screen arrangement. The
  Worker read it as a machine matter and moved on, which was right for the block but leaves a broken test by the project's own
  standard -- `40-testing.md` 4.2.3 says a test that passes only in one environment is a broken test, not a flaky one, and the
  same reasoning applies to a monitor as to window focus.
- FIX (small, any block): a test that needs a width it cannot get must FAIL WITH THAT SENTENCE ("this test needs the window at
  880 pt; this screen allows 849.6") or skip with it -- never fail on an assertion about a row's position, which sends the next
  reader hunting a layout bug that is not there. Decide which (a clear failure is better than a silent skip: a skipped test on
  the developer's only machine is a test nobody runs). One helper, used by every width-dependent Editor test.

## Repo (2026-09-30)

- **Git LFS is not in effect.** The `[attr]lfs` macro is defined in `TileStories/.gitattributes`, but git allows macro
  definitions only in the TOP-level `.gitattributes`, so `*.png lfs`, `*.mp4 lfs` etc. set nothing (git warns "not allowed" on
  every command). Every binary is committed as a normal blob (checked: the 17 MB castle video and the PNGs are raw in HEAD). To
  use LFS: move the macro lines to a root `.gitattributes` (or write `filter=lfs diff=lfs merge=lfs -text` directly), install
  git-lfs, and run `git lfs migrate` if the history should shrink. Developer's decision (needs a remote that supports LFS).

## Process (2026-10-01)

- [thesis] Re-check the POI card plan (`_3.1`-`_3.3`) against the literature: for the main decisions (block catalogue, one
  owner per medium, bottom-sheet card, takeover, Editor-configurable options, sunlight contrast) find supporting or contrary
  sources and record the citations in the guides' References, for the thesis report. A `Domain_Planning.md` re-check pass.
- [repo] Archive candidates for the next `Guidelines_Review.md`: `_2.6.3__curr_plan_tracker copy.md`, old `_2.x` Vision / Human
  test files that are fully done.

## POI Card Editor (2026-10-01, from _3.1 15.1)

- [_5.1, Editor] Collapsed Card Content rows of kinds with no words of their own (Compare Points, Show On Wall, a Place In AR with no
  heading) show an empty summary. A kind could name what its summary reads instead (Compare Points: the other point's list title; Place
  In AR: its model's name) -- a small `BlockKindDefinition` hook if the developer finds the blank rows hard to tell apart.
- [_5.1, Editor] At 620 pt the summary column is about 60 pt (the Kind / Variant / Display columns are fixed): enough to recognise a
  block, the tooltip holds the rest. If narrow windows are common, let the Variant column shrink before the summary does.

## POI Detail Card close-out (2026-10-02, from _3.1 step 16)

Everything the card domain (`_3.1`) still owed that is not navigation (`_3.2`) or style (`_3.3`) lives here, by kind of destination. The reason
for each is in `_3.1.1_Audit.md` section 12.5.

**Device checks (Tier C / D: a phone, a real wall)**
- [place_in_ar] Real size (a Real Size arch measures its file's metres with a tape; Height / Marker Multiple against the printed markers),
  lighting (the placed model uses the scene's lights, not AR light estimation: does it sit in the room?), anchoring drift (it is a child of the
  wall's frame: does it stay on The Lamp while walking and after a relocalisation?), frame rate with the arch standing, then with the room scan.
  Proven in Editor Play Mode only (`MockLocalizationProvider`, Tier A).
- [place_in_ar, Keep Model On Switch] Is it confusing that only the placing point's card offers Remove From Room? Built since `_3.2` step 9:
  the placed-model chip names the point on another card and a tap on it opens the placing card (with Remove) -- check a visitor finds that.
- [panorama gyro] Proven in the Editor with a SIMULATED `AttitudeSensor` only. On a phone: the view turns the right way in portrait, Input
  System's "Compensate Orientation" matches the screen, Android's RotationVector vs GameRotationVector drift. The Editor path falls back to drag.
- [show_on_wall] The card drops to peek and relies on the selection keeping the marker lit: is the marker ON SCREEN (does zoom-on-select pan to it)?
- [Peek Next Card] Do visitors still not swipe the timeline / related tracks? If so add dots like the gallery's (`_3.3`); if the 32-unit slice
  reads as a broken card on a small phone, tune `--ts-track-peek` (`_3.3`).
- [nav_rails / nav_bento, `_3.2` step 15a] With a real thumb: does a rail's sideways swipe feel like one viewport (the swipe `NavRailRule` counts:
  the strip follows the finger 1:1, no fling), or does a visitor expect it to coast -- if so the rail needs a fling, and the 3-4 swipes count
  changes; does a thumb that starts a vertical scroll ON a rail ever move it by mistake (Phase B proves the rule with a 90 degree stroke, not a
  tilted one); does a visitor find the "All 18" card; and do the bento's full-view tiles read as "opens over the card" from the glyph alone. Also
  how the pages layout's arriving page reads on a real swipe: its words as compact rows, then the real blocks at the turn.
- [bento layout, `_3.2` step 15b] With a thumb, outdoors: is the largest tile what the eye goes to first; are the tiles' block names and counts
  read at arm's length (caption size, `--ts-ink-2`); is the smallest tile (64 px, Listen / Community) tapped with a thumb; does a visitor expect
  Back to return to the tiles (it does) or to leave the card; and does the section opening over the card read as "inside the card" or as a new
  screen.
- [Portuguese card] Read the PT card on a phone once (accents, wrapping of the longer words) with the visitor language chip.
- [_3.2 navigation, once built] With a thumb: the radial arc (step 9) and the edge scrubber (10); the pages layout vs vertical scrolling (14:
  disorienting?); Android Back steps the card down (4: `Keyboard.escapeKey`); the placed-model chip at peek (9) answers the Keep Model On
  Switch question above.
- [_3.2 step 9, radial with a thumb] One-handed, the card at its title and half heights: press the Radial, slide to each family and let go
  -- every family reachable by the same thumb, none under the thumb's own shadow, the 45-degree sectors wide enough not to miss (at Full the
  button rests in the screen's top corner: is it worth reaching there at all, or should a wall pair the Radial with Show Navigation At?).
- [_3.2 step 9, chip over the camera] Place a model at a real wall and lower the card to its title: read the chip (and the floating button)
  over the live camera against a bright patch (a white tile, a window, sunlight) AND a dark one. Proven in the Editor over a white, a black
  and the room-scan frame (`PoiCardFloatingSceneTests`): the dark surface >= 3:1 on white, the light 2 px edge >= 3:1 on black.

- [full view, `_3.2` step 16 / close-out] A sideways swipe that starts ON a part that drags by itself (a 3D / 360 stage, a before / after
  slider, a gallery picture: its tap is a Clickable, which takes the finger at touch-down) is that part's, as in the pages layout; on a Media
  page much of the page is such a part. Watch whether visitors try to swipe on the picture; the chips are always there (WCAG 2.5.7). The video
  player does swipe (its overlay is a Button). If they do try, `SwipeGrabRule.LeavesToPart` is the one place to change.
- [pages, `_3.2` step 15a / close-out] DECIDED: the arriving page shows its blocks' WORDS (compact rows) while a drag is claimed, never a
  second bound stack (a neighbour's media would stay busy to decorate a half-second slide). Only a device reopens it: if the slide reads as
  empty under a thumb, revisit with that evidence (`Nav_Lamp_pages_midswipe_pt_390.png`).

**Ship blockers (before any build leaves the lab)**
- [media licences] The Lamp's two narrations (`CardMedia/audio/*.mp3`), the two photos and the castle video (`CardMedia/video/`) have NO
  established licence (the media README says so): replace or license them.
- [room scan] The 15 MB room scan lives in `Apps/LivingRoom/Resources/`, so it ships in every LivingRoom build: fine for the test wall; never
  copy the pattern to a real wall; drop it when the scan is replaced.
- [content] The castle clip has dark bars down its sides (baked into the 640x360 source): a cleaner source clip is the fix, not the card.

**Stage 3 (backend, telemetry), Stage 4 (baker), Stage 6 (packaging)**
- [Stage 3] `ICardEvents` has one implementation, `LogCardEvents`; the telemetry work replaces it (`feedback`, `poll`, `collect` raise events
  today; a knowledge answer is a natural next one). See also the 7C note above on `PlayerPrefs` and `_7.1`.
- [Stage 3] Poll results: no backend, so `poll` shows the visitor's own choice. Register the real `IPollResults` in `CardServices`; the view
  already draws bars from any implementation (proven in Phase A with a stand-in).
- [Stage 4] `config.json` size: LivingRoom's went from 39 KB to 196 KB because `JsonUtility` writes every unused slot of every
  `BlockFieldValue`. The baker (or a trimmed writer) is where to fix it; do not hand-roll a serializer mid-domain.
- [Stage 6] Models load as IMPORTED prefabs from Resources (glTFast's Editor importer). A wall that downloads content needs runtime
  `GltfImport` from bytes; `IPreviewStage.Load` is the one place that changes.

**Later card features (no domain owns them yet; build when a wall asks)**
- [panorama] equirect only (2:1, loaded whole): a 16K wall picture needs tiling or a cubemap; and a mip-mapped import or a higher-resolution
  default (the 2048 x 1024 default shows its pixels at a narrow zoom).
- [video] landscape full-screen takeover (rotate when the phone turns).
- [media] a wall media library CREATION flow (a wall authors its own `CardMediaLibrary` today).
- [Editor] hotspot_image: a click-to-place pin on a picture preview (sliders only today); duplicate block / copy a block from another POI /
  "insert after this row"; Card Texts grouped by kind with a filter; a block demo mode (a demo point holding one kind in every look, generated
  from `CardGalleryDefinitions`, with Demo Language and "scroll to block"); a "Half Stop Height" label for Half Height Max; looks with display
  labels (`OptionLabels` for variants, raw keys today); the Card Container's rows under "Sheet" and "Audio" headers and a "Media" group;
  `DetailCard.cs` at 307 lines and `DetailCardHelp.cs` at ~510 (the `BlockFieldDrawer` split by type family was done at `_3.2` step 4).
- [options] Tuning controls for constants that live in code: zoom_image's maximum zoom (`ZoomPanRule.MaxScale`), the turntable auto-spin speed,
  the audio switch fade, Show On Wall's neighbour count, related's automatic count. Decide per field, not wholesale.
- [kinds] place_in_ar anchor `surface` (a detected plane: device stage); timeline `then_now_pairs`; a person's photo; story chapter pictures;
  Landscape-aware layouts; the deferred kinds already named in `_3.1` section 6 (`ar_time_travel` `_3.4`, `guide_bubble` / `share_postcard` /
  `badge` / `wall_hunt` / `visitor_notes` Stage 3, `ai_guide` Stage 7, tiled deep zoom).

**Cross-domain (another domain draws or owns it)**
- [taxonomy / marker domain] **Category and status names and the marker names of cardless points are English on the Portuguese card**
  ("Royal Government", "Intact / Partial Damage / Destroyed", "Lamp - Infrastructure" in the related carousel and Show On Wall's neighbour
  chips): every card string, glossary entry and block field already has en + pt. Fix with per-language taxonomy names in the marker /
  taxonomy config and a Header title on the unnamed points. Until then a PT wall shows English in those places.
- [marker domain] The visited state drawn on the marker after the card closes (the card records it, the marker draws it, `_2.2.x`);
  `with_neighbours` NAMES the nearest points but does not light their markers (the wall highlights the selection only, `_2.6`).
- [`_2.6` search] Card text in search is out of the card's scope; decide in `_2.6`.
- [`_5.1`] `SpecificMarker/POIEditorToolWindow.SpecificMarker.cs:197,205` draw non-ASCII arrow glyphs on the move buttons (`.clinerules/20`
  2.1: ASCII only); use `^` / `v` as the Card Content rows do. (Reported by the step 14 audit as moved here; it was not until step 16.)

**Test infrastructure and fixtures**
- [flakes, seen once each, kept until one recurs twice in the background] `TaxonomyRowIdentityTests.TypingADuplicateAndABlankCategoryLabel_...`;
  `DetailCardEditorTabTests.AColourField_RealTypingOfAHexCode_...`; `ConfigUndoGroupingTests.DraggingASlider_IsOneUndoStep` and
  `DetailCardEditorTabTests.Glossary_RealClicksAndTyping_...` (once, with Unity in front); `LivePlayModeCardTests.TheRealWindow_WhilePlayModeRuns_...`
  ("Live heading" arrived as "Live he", 2026-10-02, Unity in the background, 5/5 alone after); `PoiCardLanguageSceneTests.ThePick_StaysForAnotherPoint_...`
  (the chip's click never fired once; its precondition now names what covered the chip). The typing ones share the cause fixed at `_3.2`
  step 3 (any focus change ends an IMGUI field's editing session; `PoiEditorWindowHost.ReplaceText` now types a word inside one tick,
  `PoiEditorWindowHostTests`); still watch the slider drag and the chip click. (The gallery's 1.5 px header flake
  was fixed at its cause in 6B.) Step 16's own runs: 135 EditMode + 37 PlayMode targeted tests passed first time, Unity in the background.
- [cost] `CardGalleryTests` spends 463 s of its 887 s in the two generic per-entry checks (270 cases build a sheet, wait frames and write a PNG): write
  the render only for entries a vision pass needs, or behind a flag.
- [fixtures, add to The Lamp's sibling POIs, identity fields untouched, when a chunk touches those kinds] the Phase B gaps of `_3.1.1` section 2:
  Header compact and Fit sphere / at_rest / visible, Status Label, Swatches Show Code, Size Comparison two_euro_coin / sheet_a4, Dialogue Choice 3 /
  Reply 3, Related same_category, Sources draft, Video / Audio Captions On By Default, Audio Guide speeds off / wide, 3D Model and 360 Panorama Fallback
  Picture, 3D Model Fit sphere / at_rest, Knowledge Check Option 4 / Pictures 3-4, Place In AR Button Label; plus a Phase B test that a Block Library
  field default reaches the card (a wall with Default Fit Sphere), the header's Fit reaching its slot, a wall media library fixture, and the Android
  earbud poll switch (device-only symptom).
- [watch] The glossary tap uses the Experimental `PointerUpLinkTagEvent` (`CardTextView`), isolated in one file on purpose: check it on every Unity
  upgrade.

## Test harness (2026-10-15, from _3.2 step 2)

- [investigate] `CardGalleryTests.BeforeAfter_ARealDragMovesTheHandle_TheCutFollows_AndTheStackDoesNotScroll` failed ONCE
  (handle at 1.0, expected 0.8 +- 0.02) in a targeted 449-test PlayMode run with `editor_is_focused: false` while the
  developer worked in other apps. It passed alone (focused) and in a full CardGalleryTests rerun (327/327; this test ran early,
  still focused). Not reproduced, so not "flaky" yet. Hypothesis: `CardTestInput.DragFrom` sends synthetic mouse events to the
  panel over 12 frames while the slider holds pointer capture, and a REAL mouse move over the Game view (the developer's
  cursor crossing Unity) reaches the same panel and drags the handle to the edge. Test: run the fixture with Unity in the
  background AND the cursor moved across the Game view during the before_after test; if it reproduces, make the card drags
  immune to real pointer input during a run (the harness is shared ground: a full PlayMode run).

- CLOSED (2026-10-16, _3.2 step 5b): the two `PoiCardNavSceneTests` failures of 5a's full PlayMode run (`ABlockAsksTheHost_ToScrollToABlock_...`,
  `ARealFingerDrag_MovesTheSpy_...`), which ran right after the failing Model 3D test and never reproduced alone, were a cascade of the 0 px
  Half: with `CardChromeRule`'s floor the Model 3D test is green on the DEFAULT config, and both stayed green in the 5b closing run of every
  Card / Poi fixture (553 tests, Unity in the background, the same fixture order as the full run).
- OPEN (2026-10-16, seen once at _3.2 step 5b): `PoiCardAudioSceneTests.ALiveEditOfTheBlockWhilePlaying_...` ("the audio never noticed:
  Idle") and `ALiveEditOfTheClipWhilePlaying_...` ("Expected Pause, was Play") failed in ONE background run of the audio + chrome fixtures;
  green in the closing run before it, alone focused (11/11) and alone in the background (11/11). Both tap the hero chip right after
  `OpenFull`, whose wait is a fixed 0.45 s of real time; in the background a frame can carry a whole stale layout. If it comes back, make
  `OpenFull` wait for the sheet's rest height as `PoiCardChromeSceneTests.Rested` does (one of its tests read the Half viewport as 62 px
  instead of 107 in the 5b closing run, in the background; it has waited for the measured rest height since and stayed green unfocused --
  the likely cause, a sheet not yet at rest, was not proven by a measure of the root at the time).

- CLOSED (2026-10-24, harness H1, see "Editor-window tests depend on focus") -- was: OPEN (2026-10-16, seen once at _3.2 step 7b): `ConfigUndoGroupingTests.DraggingASlider_IsOneUndoStep` read 3 history steps instead of 2
  ("a whole drag = one history step") in ONE full EditMode run (Unity focused, 1798 tests, my first run of the block); the same fixture passed
  alone (3/3) at once and in both later full runs (1795/1795, 1795/1795), and in the baseline (1769/1769, Unity not focused). Not reproduced,
  so not yet a finding. It is a real-window drag, so a stray real pointer or focus change during the run is the first hypothesis (same family as
  the 5a "Test harness" notes); if it comes back, reproduce it with the cursor moved across Unity during the run.
  SEEN AGAIN (2026-10-17, _3.2 step 11a): the same 3-for-2 in one full EditMode run (1856, Unity focused, right after two POI Editor capture
  windows had been opened and closed), green alone (3/3) and in the next full run (1856/1856). Twice now, both times in the first full run after
  something else drove the Editor's windows -- reproduce it by opening and closing a utility window just before the run. In the same block one
  unfocused full run also had `KeywordListFieldTypingTests.TypingTwoKeywordsWithACommaBetween_GivesTwoKeywords` miss its focus precondition
  (green alone, unfocused), the step 6 note's pattern. SEEN AGAIN (2026-10-19, _3.2 step 14): `KeywordListFieldTypingTests.TypingTwoKeywords...`
  missed its focus precondition in a full EditMode run started ~3 minutes after a POI Editor capture window was opened and closed (Unity
  focused); green alone at once. Same trigger: a utility window driven just before the run.

- OPEN (2026-10-19, seen once at _3.2 step 14): `PoiCardNavSceneTests.AJumpToABlockInTheFinalScreen_...` failed its PRECONDITION in the full
  PlayMode run (wholly in the background): the lowest block's top 13205 vs the range 13337 -- the Sources slot read 132 px taller than the
  viewport, i.e. the viewport was measured before the sheet reached its Full height. Green in the targeted run before it and alone after it
  (18/18, background). Fix when it recurs: the fixture's `Open` waits for the sheet's measured rest height (as
  `PoiCardChromeSceneTests.TheLampAtHalf_...` does since step 5b) before the test reads the range.

- OPEN (2026-10-17, seen once at _3.2 step 8): **the Editor crashed while IDLE, out of memory, from a native audio assertion flood.** After a full
  EditMode run had ended (21:16) and the gallery scene reloaded, the line `Access version should be odd when acquiring lock` (native,
  `audio::DualThreadManager::ControlUpdate` from `AudioManager::Update`) was logged about 83 million times in ~4 h; the console's buffer ran out
  of memory at 02:40 (crash log 4.4 GB, deleted with the developer's OK). No managed code in the stack; the flood started next to a licensing
  token refresh, with no test running. If it comes back: note what audio device changed (headphones, sleep / wake), check whether a PlayMode
  run with real audio (`PoiCardRealAudioTests`) ran just before, and whether Edit > Project Settings > Audio > Disable Unity Audio while idle
  stops it. Not a code finding today: Unity 6000.3.19f1 engine side.

- OPEN (2026-10-18, seen at _3.2 step 11b): **a real touch drag moves the card's stack about 1.7 times the finger's travel.** A slow
  `CardTestInput` touch stroke of 345 panel px (30 moves, then held still 10 or 60 frames) moved The Lamp's stack 579 px, and a longer hold
  overshot MORE, so it is not a fling. `PoiCardNavContentsSceneTests` now drags in short strokes until the spy line passes its block (a reader's
  way, robust to any gain). Find where the gain comes from (ScrollView touch scroll vs the panel's scale: the run's 390 x 844 frame in a
  differently sized Game view, `PanelSettings` scale with screen size) and whether a device shows it; any test that expects a drag of N px to
  scroll N px is measuring the harness, not the card.

- OPEN (2026-10-19, seen at _3.2 step 12): **a tap right after a long `ScrollTo` can be lost.** `CardTestInput.TapInView` waits 0.15 s of real
  time after `ScrollTo`; a long scroll collapses the header and the content keeps moving for some frames, so on a slow frame the scroll was not
  applied yet (the target at y 10584, the viewport at 328) or the row moved between finger-down and finger-up and the click was lost. Seen
  twice in one `PoiCardExpandableSceneTests` run (green alone and in three later runs; the developer was resizing the Unity window at the time,
  a second plausible cause). That fixture now taps through its own `InViewAndStill` (scroll until in view AND the same rect for three frames).
  If another fixture loses a tap after a scroll, move that precondition into `TapInView` itself (shared ground: a full PlayMode run).

- GUARDED (2026-10-22, _3.2 chunk G; the Lamp / Next Up entry below stays OPEN): the "count" the entries below ask for exists --
  `Tests/Runtime/LayoutStruggleGuard.cs`, wired into `SearchSceneFixture`, `LodWallSceneTests` and the app's `SizeComparisonGalleryTests`: the
  line now fails the test whose scene logged it, naming the test, the scene load # and the phase. Why the audit's 17 named no test: UI Toolkit
  logs it with Debug.LogError, but the fixtures' TearDown unsubscribed their error collector BEFORE unloading the scene, and the burst sat in
  exactly that window (`Editor-prev.log` 171452-171512: after "[WallSession] Ready", before the unload). 0 lines from Unity over this session's
  ~1480 scene loads, so nothing went red yet. If it fails one day: the message's phase says whether the loop is in the card's views (a test phase)
  or in a panel tearing down (the unload) -- start there, and a new cause gets its own entry.

- ONE CAUSE FOUND AND FIXED, the Lamp / Next Up case STILL OPEN (2026-10-21, _3.2 step 17b): "Layout update is struggling" reproduced
  ON DEMAND -- every run, alone -- on The Painting's new card under icon_tabs_chips (dragging across a boundary between a section with kind
  chips and a one-kind section). A probe over navigation variants isolated it (Text Tabs, the framework default and The Lamp under the same
  preset were clean): the emptied chip row collapsed, the viewport grew, the spy line crossed back and refilled the row -- a loop inside one
  layout pass. Fixed (USS: the row keeps a touch target's height), test-first (`PoiCardPaintingSceneTests.TheTabsWithChipsStrip_...`: 103 vs
  55 px red, then green, and the drag test clean). NOT the same as the entry below: that one was The Lamp under Text Tabs with Next Up at the
  card's end, which has no chip row. It shares the SHAPE, though -- a view that changes its own height from a spy/scroll callback -- so if it
  shows again, look for a slot or element whose height depends on the section or block being read (Next Up's finished words, the end-of-card
  reveal) and give it a fixed place, as here.

- SEEN AGAIN (2026-10-23, `_3.3` P4.1, the theme pipeline): the full PlayMode run (1237 results, Unity focused throughout) failed
  `ARealDragToTheEnd_...` with 2 "Layout update is struggling" lines (PanelRootElement 390 x 844, scene load #503); the same test passed in the
  Card / Poi targeted set an hour earlier on the same tree (1021/1021) and 8/8 alone right after (Unity in the background). P4.1 changed no
  geometry callback and no layout value (pixel set: 0 px), but the next long run should still say whether it shows again.
- SEEN AGAIN (2026-10-24, _3.3 P4.4c1b): the 27-fixture targeted set (391 tests, background) failed `ARealDragToTheEnd_...` with 3
  "Layout update is struggling" lines (scene load #17); re-run at once with its whole fixture + PoiCardNavSceneTests + the new
  PoiCardInterleavedSpySceneTests (33 tests, background): 33/33 and 0 struggling lines. P4.4c1b changes what the SECTION spy names at
  the end of an interleaved card; Next Up follows the BLOCK spy, untouched. Still only in long runs; the full PlayMode run of P4.4c1b
  said: 1314 tests, 0 failed, `ARealDragToTheEnd_...` green, 0 real struggling lines (the 5 in the log are LayoutStruggleGuardTests' own). Still OPEN, not closed on one quiet run.
- STILL OPEN (2026-10-20, _3.2 step 15b): NOT seen in either long run of 15b -- the Card / Poi targeted set (892 tests) and the full PlayMode
  run (1113/1113, wholly in the background): 0 "Layout update is struggling" lines in the Editor log over both, `ARealDragToTheEnd_...` green in
  both. Two quiet runs after three loud ones: left open, not closed (the brief: never on one quiet run). If it shows again, the entry below says
  where to start.
- OPEN AGAIN (2026-10-20, seen at _3.2 step 15a, the entry below): the Card / Poi targeted set (865 tests, 37 fixtures, ~test 612) failed
  `PoiCardBottomElementsSceneTests.ARealDragToTheEnd_...` with six "Layout update is struggling" lines (PanelRootElement 390 x 844, Unity in the
  background); the same fixture passed 16/16 in the next run (595 tests with the galleries) and the step 15a block's own views are not bound
  there (no rails, no bento, no pages: the block added a `CardPagerView` cue list that is empty at rest, a USS file with more rules and
  two `CardTrackFit` / `BlockStackView` / `NavController` seams that run only where a rail or tile asks). Not proved either way that 15a did not
  make it likelier: the next full run decides; if it shows again start from the geometry callbacks named below (`CardChromeView`, the end-of-card
  reveal, Next Up's finished words), and add a `LogAssert` count so a "struggling" line fails the SETUP of the next test, not a bystander's.

- CLOSED (2026-10-19, _3.2 step 13: did not come back -- the Card / Poi targeted set 815 and the FULL PlayMode run 1051/1051 both passed
  `ARealDragToTheEnd_...` (25.6 s in the full run) and neither Editor log nor the results hold one "Layout update is struggling" line;
  no loop was found because none showed: if it is seen again, start from the geometry callbacks named below). Was:
  **"Layout update is struggling to process current layout" during
  `PoiCardBottomElementsSceneTests.ARealDragToTheEnd_FinishesTheBar_AndBackUpItNamesABlockAgain_InPortuguese`.** Unity logs it (2-3 times,
  PanelRootElement 390 x 844) as the next_up bar is dragged to The Lamp's end -- where the end-of-card reveal grows the content, the chrome
  brings the bottom area back at the end and Next Up switches to its finished words, all applied from geometry callbacks: a loop between
  them is the likely cause. Seen in BOTH long runs of step 12 (the Card / Poi targeted set, ~test 600 of 825, and the full run, ~600 of 1032),
  never in isolation: alone twice, its fixture with PoiCardBottomBarSceneTests 16/16, in the reveal-path set 153/153. Step 12's code is not on
  that path (no expandable block, no full view; its one style set from a geometry callback is on the full view's own scroll, never on a
  panel there); that it predates step 12 is NOT proved (11a's full run and 11b's targeted set did not show it; the base-commit run was not
  allowed). The developer's call: logged, and the next full run decides -- if it shows again, find the loop (which callback dirties layout
  more than once per frame at the end of the card) and fix it there.

- OPEN (2026-10-20, seen once at _3.2 step 15b): `CardGalleryTests.KnowledgeCheck_TrueFalse_ARealSwipeRightIsTrue_...` failed in the Card /
  Poi targeted set (892 tests) at "the card came back": `translate.x` read 29.83 right after a short drag of a tenth of the stage (the stage is
  ~298 px: exactly the drag) -- the assertion reads the resolved style in the same step as the release, with no frame for the snap-back to apply.
  Passed alone with Unity in the background (149/149 with the gallery fixtures). Step 15b did not touch the knowledge check, SwipeGrab or the
  gallery harness. If it shows again: wait a frame (or for the stage to rest) before reading `translate`, like `InViewAndStill` waits for a rest.

## A placed model with NO card open (2026-10-17, from the _3.2 step 9 review)

- `_3.2` step 9 closed both AR debts: the placed-model chip now floats above the sheet at every stop, PEEK included, and
  offers Remove on the card that placed the model (`Search_Floating_Lamp_chip_peek_en.png` and the three bright / dark frames).
  What it cannot cover, because the chip is a CARD element: a visitor who CLOSES the card leaves the model standing in the room
  with nothing on screen that says so and no way to take it back. Keep Model On Switch makes that easy to reach.
  Not `_3.2`'s to answer -- that domain is "inside ONE open card" (section 1) -- so it belongs to the app shell: either the
  placement owner keeps a control of its own outside the card (like the audio mini-player, which outlives the card), or
  closing the card removes the model. Decide it with the AR / Stage 3 work, and look at the audio mini-player first: it already
  solved the same shape of problem.

## POI Editor (2026-10-16, from _3.2 step 5a)

- (2026-10-16, _3.2 step 6) The progress map at 390 px with eight families: the 48 px touch minimum holds seven pieces at 48 and gives
  About 54, so "sized by target count" barely shows on The Lamp (it does on shorter cards: 190 / 125 / 62 px for 3 / 2 / 1 targets). A
  device check should say whether visitors read it as a map at all with eight pieces; if not, the wall can pick another Top Bar (no code).
- "Trying to access the DPI setting of a visual element that is not on a panel" is logged (several lines) when a live card edit rebinds the open card in Play Mode (seen in `PoiCardTextTabsSceneTests` runs, green). Find which element reads a style before it is on the panel; harmless today, but it is log noise in every live-edit test.

## From _3.2 step 15b (the bento layout, 2026-10-20)

- DECIDED at the _3.2 close-out (KEEP; the Architect may overrule) -- **the ONE video owner keeps a paused clip after its last view left the screen.** Measured at 15b on the owners:
  a video played in a bento section page and the page closed -> `VideoBlockView.Collapsing()` pauses it (step 12's rule) and the owner keeps the
  clip loaded (`Card.Media` holds `video/castelo_s_jorge_video`, one reference) until another video plays or the card closes. The same holds today
  for a closed expandable row, a tabs section put away and a page turned: in all four no view on screen shows that video any more, so nothing can
  resume it but the same block coming back. Either keep it (resuming where the visitor left is cheap and the clip is one file), or let the owner
  give the clip back once no bound view shows its track (an owner rule, shared ground: a full PlayMode run). `PoiCardBentoLayoutSceneTests.Closing...`
  asserts today's behaviour exactly, so a change turns it red on purpose.
- CLOSED at _3.2 step 16 -- the vacuous `IsCurrent(null)` line of `PoiCardLayoutSceneTests` (tabs): the track is read before the section is put away and the line asserts the owner keeps that ONE paused clip (section 7.1); proved load-bearing (Collapsing -> Stop turns exactly that line red).
- CLOSED at _3.2 step 16 (decided at the 15b review) -- a bento tile never grows past its content; the room left stays empty under the last tile (`BentoLayoutRule` RoomLeft).

## From _3.2 step 16 (the full-view breadcrumb and sibling swipe, 2026-10-20)

- Test harness: `ConfigUndoGroupingTests.DraggingASlider_IsOneUndoStep` failed ONCE in a full EditMode run with Unity in the background ("a whole drag = one history step, expected 2, was 3"), green alone right after and in the next full run (1946/1946). An Editor slider drag with no seam into step 16; logged OPEN, to be watched in the next runs (a focus-dependent drag would be the 4.2.3 case). CLOSED (2026-10-24, harness H1): see "Editor-window tests depend on focus" below.
- Fixture fact worth knowing: The Lamp's two videos (block_56, block_57) share ONE clip, so to the one video owner they are the same video (`VideoTrack.SameVideo`: point + clip) -- a test of a handover between them proves nothing. Lamp - Residential's block_4 + block_7 are the pair with two different clips (step 16).
- OPEN (2026-10-20, seen once at _3.2 step 17a): two more `PoiCardAudioSceneTests` failures (`AnotherPointsAudio_InSwitchMode_...`: the narration did not wait for the tone; `ClosingTheCard_WithKeepAudioPlayingOff_...`: Idle, not Playing) in the 41-fixture Card / Poi run right after a PC restart; both passed alone (11/11). Same family as the 5b entry above: audio timing under a loaded machine.

## From the _3.2 domain review (AUDIT, 2026-10-21; verdicts in `_3.2.1_Audit.md` F3)

- OPEN, a fifth "Layout update is struggling" sighting, NOT the Lamp / Next Up shape: 17 lines in the first ~15 scene loads of the full PlayMode run (the LivingRoom app fixtures, Unity focused, `Editor-prev.log` loads #638-#652), every test green; 0 lines in every later run of the day (240 card tests, a 75-combination element x layout matrix counted per combination, the LivingRoom assembly alone 30/30). The fixtures do not fail on the line, so its test is unnamed: add the `LogAssert` guard (fix list chunk G) before chasing it again. The Lamp / Next Up entry above stays OPEN (quiet again).
- OPEN, new: `NavigationPresetEditorTabTests` x3 failed in the FIRST full EditMode run of the day only (a "Lamp - Military shows no navigation" note and a "Preview Point" row drawn while the shipped config's demo card is OFF); 6/6 alone at once, green in the two later full runs. A shared-state leak from earlier work in the same Editor session, not focus. Fix: assert the demo card is off before drawing in the first two tests too (chunk G).
- Seen green in all three full EditMode runs of the day (one focused, two not): `ConfigUndoGroupingTests.DraggingASlider_IsOneUndoStep`, `KeywordListFieldTypingTests`; `PoiCardAudioSceneTests` green in the full PlayMode run. Left OPEN with their triggers (a utility window driven just before the run; `OpenFull`'s fixed wait).
- Harness lesson (the audit's own): starting EditMode runs back to back with `TestRunnerApi.Execute` from editor code raised Unity's native "Scene(s) Have Been Modified: Untitled" modal twice and froze the MCP bridge until the developer clicked Don't Save. The MCP runner handles that dialog: never chain test runs from editor code.

## From the _3.2 close-out (2026-10-22; the triage table is `_3.2.1_Audit.md` section 13)

Where every `_3.2` line of this file stands: the DEVICE list ("Device checks" above: radial, rails, bento, chip over the camera, Portuguese card,
progress map with eight families, the full-view swipe, the pages' arriving page); SHIP BLOCKERS (media licences: `_3.3` step 0 gathers test
content, the blocker stays open); STAGE 3 (a placed model with no card open: the AR / app shell work); the HARNESS list below; the entries
moved to `_3.3` (demo words, the header at Half) and to `_5.1` (narrow window, Card Starter on every point).

**Later card features from `_3.2` (no owner; build when a wall asks)**
- [framework] A per-point navigation override (layout / top for ONE card): today everything is wall-level and a wall's long card and its short
  ones differ only through Show Navigation From. Cost M (a card-level settings object, a resolve rule, rows). After `_3.3`.
- [visitor] The breadcrumb of the full screens a BLOCK opens (the gallery's lightbox, a video's or 3D model's Takeover display): they read
  "<card title> > <name>" while a full view opened by the card climbs the ontology. Needs the block's key through `IBlockHost.OpenTakeover`,
  an interface every block view calls. Cost S-M.
- [visitor] Remember where the visitor was (section / scroll) per point on reopen: a `CardLocalState` slot. Cost S.
- [kiosks] Tablet / landscape behaviour of bento, radial and pages (with the landscape-aware layouts above).
- [Stage 7] A screen reader for every element (the labels exist).
- [Stage 6] Four Framework DEFAULT PATHS name the demo app (`WallSession.configPath`, `POIEditorToolWindow.DefaultConfigPath` /
  `DefaultStreamingConfigPath`, `AssetPaths`): older than `_3.2`; a packaged framework must not name an app.
- [code health] `NavController` is 414 lines (jumps + spy + seen + layout seams): split when the next block touches it. `CardNavigationHelp.cs`
  (602, text only) and `BlockStackView` (580, `_3.1`'s) need nothing.
- DROPPED: "an app registers its own Card Starter sets" (no wall needs it; `20-code-quality`, no speculative code). Build it with the first app
  that has kinds of its own and asks.

**Harness (shared ground: one block, a full PlayMode run; each entry above keeps its own history)**
- **The rest wait is copied 14 times.** `Rested()` is a private copy in 14 PlayMode fixtures, `InViewAndStill` in 4 more, `StackAtRest` in one
  (`PoiCardTopElementsSceneTests`, added at `_3.2` chunk D + G): the "third copy is the cue to move one into `CardTestInput`" of that block is long
  past. Move ONE condition-based wait into `CardTestInput` (the sheet at its rest height; a target in view AND the same rect for three frames;
  the stack's offset holding for three frames), delete the copies, and let `PoiCardAudioSceneTests.OpenFull` (fixed 0.45 s) and `CardTestInput
  .TapInView` (fixed 0.15 s after `ScrollTo`) use it: that closes the audio-timing entries, the lost-tap entry and the step 14 precondition
  entry in one stroke.
- `NavigationPresetEditorTabTests` x3 (a shared-state leak, the first full EditMode run of a day): assert the shipped config's `demo_card` is
  OFF in the first two tests too, as the third does (one line each).
- Width-dependent Editor tests fail with the sentence "needs the window at N pt; this screen allows M" (entry above, one helper).
- CLOSED for the slider drag and the keyword typing (2026-10-24, harness H1); the rest stays watch-only -- was: Watch-only, no action until they recur twice: the `ConfigUndoGroupingTests` slider drag and `KeywordListFieldTypingTests` focus entries
  (trigger: a utility window opened and closed just before the run); the `CardGalleryTests` before / after and knowledge-check drags (read
  the resolved style a frame after the release); the 1.7x touch-drag gain (check on a phone first: if the device shows it, it is the card's).
- The "Layout update is struggling" entries are GUARDED by `LayoutStruggleGuard`; the Lamp / Next Up entry stays open until the guard fails.
- The idle-Editor audio assertion flood (`Access version should be odd when acquiring lock`, 2026-10-17) is the engine's, not the card's:
  environment note only, reopen if it returns (Unity 6000.3.19f1).

## Themes after the main one (2026-10-22, from the `_3.3` re-plan)

- `_3.3` builds ONE main theme (premium heritage, light default + dark, both high contrast) to the end first. Only then, if time
  allows, 1-2 more general themes for a wall developer to choose from. The token tiers of `_3.3` P4 exist so that a new theme
  rewrites the primitive + semantic tiers and nothing in a block.
- **A children's theme** to think through then: more colour, rounder shapes, illustrations / cartoon characters, playful motion
  (Duolingo-like), bigger touch targets, simpler words. Probably more than tokens: it may want its own block variants and its own
  illustration set, so judge it as a product, not a palette.
  References the developer collected: `proj_guides/detail_card/kids_styles/` (Duolingo lessons: flat bright cards, chunky
  rounded buttons, a character with speech bubbles; a low-poly animal "jungle" game).
- **A visitor profile at RUNTIME** ("premium" / "kids" / later others), chosen by the visitor, not only by the wall developer --
  with the wall deciding which profiles it offers. Same switch-classes mechanism as light / dark; needs the profile store
  (`CardLocalState` or an app-level one) and an onboarding question.

## Pin the Immersal SDK (2026-10-22, from the `_3.3` re-plan R7)

`Packages/manifest.json` pulls `com.immersal.core` from `https://github.com/immersal/imdk-unity.git` with no `#tag` or `#commit`:
only `packages-lock.json` holds today's hash, and any lock refresh (a package resolve, an engine upgrade) can bring a different
SDK version into the AR stack unnoticed. Pin it to the tag / commit in the lock file now, and move it on purpose with a device
check. Same for `com.coplaydev.unity-mcp` (`#main`), less critical (dev tool only).

## A USS lint test for every sheet in the project (2026-10-22, from the `_3.3` P0 spike R8)

The P0 spike found three USS mistakes that import WITHOUT a console message and break silently at runtime: a `cubic-bezier()`
timing function (every style update of a matching element then throws `ArgumentOutOfRangeException`), a
`-unity-font-definition: url(...)` pointing at a TMP_FontAsset (the label draws nothing), and a `var(--x)` whose name is misspelt
(the property paints transparent). One EditMode test that scans every `.uss` under `Assets/` (card, search, zoom, onboarding,
Dev excluded) for the first two patterns and checks each `var(--name)` against the names some sheet defines would catch all three
before a render does. Cross-domain (every UI Toolkit screen), so not part of `_3.3` alone. `StyleSheet.m_ImportedWithWarnings`
(reflection) also flags a sheet the importer partly dropped.

## From `_3.3` P1b (real content on the test wall, 2026-10-22)

- [`_2.6`] **Search in the visitor's language.** Categories now have a name per card language (Category Names Per Language) and the
  card's chip reads it, but search has no visitor language: results rows, suggested searches and filter chips show the one Category
  label, and the search bar's own words (Search, Filters, Map) are English. Give search the card's language (one source of truth for the
  visitor's language) and pass it to `PoiSubtitle.Of` / `TaxonomyNames.Category`.
- [`_3.3` P3 / P5] **Caption and credit fields.** The header (hero picture) and before_after have no caption or credit field, so the real
  pictures' credits only reach the sources block (CC BY / CC BY-SA ask for them next to the picture in a shipped build). Decide where a
  credit shows (the full-screen picture is a natural place) and add the fields to the kinds that show pictures.
- [build size] The wall's 21 real pictures are 25.4 MB in an Android build at max 2048 px (ETC2). A card is ~390 pt wide (~1170 px at
  3x): a 1536 cap (or ASTC) would roughly halve it. Measure on a device first.
- [config size] Every block field now serialises `asset_by_language: []` (JsonUtility writes empty lists), so `config.json` grew from
  476 KB to 679 KB with no content change. Harmless today; if load time or diff noise matters, a custom writer that skips empty lists
  (or a sparse per-language list on the BLOCK) would undo it.
- [taxonomy names per language, from P1b part 2's captures] Only the CATEGORY gained a name per language (`label_by_language`, P1b).
  The status scale still shows its English names on a Portuguese card ("Intact", "Partial Damage", "Destroyed" in the status block and
  in compare_points, both cards): the same per-language name for the Status Types (and Badge / Hierarchy Level names wherever the card
  shows them), the same Editor row per other card language, the same `TaxonomyNames` lookup.
- CLOSED (2026-10-24, harness H1: the precondition read keyboardControl outside the host's OnGUI, see below) -- was: [test harness, from P1b part 2's full runs] `KeywordListFieldTypingTests.TypingTwoKeywordsWithACommaBetween_GivesTwoKeywords` fails at
  its precondition ("the click must focus the keyword field", keyboardControl 0) in EVERY full EditMode run after a full PlayMode run
  (3 of 3, 2026-10-22 ~21:58-22:05), though the same code passed the full suite at 19:35. Reproduces on demand with
  `group_names ^TileStories\.Editor\.Tests\.([H-J]|KeywordListFieldTypingTests)` (22 tests); green ALONE and in every smaller subset
  (with HierarchyDesignAuthoringTests + HierarchyTableClickTests; with HotspotPinRuleTests). NOT a focus loss: a second `_host.Focus()`
  right before the click did not help (tried, reverted). Next: log `EditorWindow.focusedWindow`, `GUIUtility.keyboardControl` and
  `Host.FieldRect` at the click in the red group; check whether a Unity restart clears it (state left by the PlayMode run).

## Editor-window tests depend on focus (2026-10-23, from the `_3.3` P1b review)

**CLOSED (2026-10-24, harness H1).** Measured with `TestRunRecorder`, each flaky test's vulnerable state held across REAL
activation changes: (1) what raised Unity and flipped its activation mid-run was the UnityMCP server's focus nudge on every
`get_test_job` (Unity unfocused + no progress for 3 s -> `SetForegroundWindow`), and with Unity raised the 1800 pt test host sat under
the developer's real cursor; with no MCP poll during a run nothing raises Unity (40-testing 4.2.3 / 4.2.5). (2) A deactivation ENDS a
text field's editing session for good (the keyword word typed one key per repaint was lost); a drag's hotControl survived six changes;
the Preset click opened the picker 9/9 in every state. (3) `KeywordListFieldTypingTests`' precondition read `GUIUtility.keyboardControl`
from the coroutine, which Unity swaps per window around each OnGUI: 0 one frame, the field the next, with no focus change -- its
"after a PlayMode run, in a 22-test group" failures. Fixed: every real-click gesture runs inside ONE editor tick
(`PoiEditorWindowHost.PaintNow` between its events; KeywordList and the slider drag), the keyword precondition is read inside the
host's OnGUI, the Preset failure message names where the focus was. Green: the 63-test targeted group incl. the [H-J] reproducer,
then the full runs of the block. The `NavigationPresetEditorTabTests` demo-card leak (below, a different cause) stays open.

The original entry:

Full EditMode on P1b's final tree, run by the Architect: UNFOCUSED -> 2031/2033 (`ConfigUndoGroupingTests.DraggingASlider_IsOneUndoStep`
"a whole drag = one history step: 2 vs 3", `NavigationPresetEditorTabTests.EveryPreset_ARealClick...` "the Preset button opens the picker:
null"); the same 11 tests alone, focused -> 11/11; the full suite again, FOCUSED -> 2033/2033. The P1b Worker saw a third one
(`KeywordListFieldTypingTests`, red only after a PlayMode run, in a 22-test group). The reds rotate between runs and are all real-click
Editor-window tests: the same family as the watched flakes of the `_3.2` hand-off. `40-testing.md` 4.2.3 says tests never depend on
window focus -- these do. One harness block: find what a real click / drag needs from focus in these fixtures (the window's own focus,
`EditorWindow.focusedWindow`, a repaint) and make them independent, or make them fail with that sentence instead of an assert.

## `TileStories/.gitattributes` macros are ignored (2026-10-23, low priority)

Every `git add` warns `[attr]lfs / unity-json / unity-yaml ... not allowed: TileStories/.gitattributes`: git accepts attribute MACROS only
in the repository's top-level `.gitattributes`, and the repo root is one folder above the Unity project. So the Unity template's LFS
rules, `eol=lf` for YAML / JSON and the `unityyamlmerge` driver have NEVER applied (`git lfs ls-files` = 0 files). Nothing is broken
today, but the file promises what it does not do. Decide: drop the macro lines (and keep plain `text` / `binary` rules), or move
the file to the repo root -- which would turn LFS ON for media and needs `git lfs install` + a history decision. Not before a quiet
moment between domains.

## Per-language POI names (2026-10-23, from `_3.3` P1c)

`POIData.name` and `POIData.summary` are ONE plain string each (`WallConfigData.cs`), unlike the card header's title / subtitle, which are
`LocalizedText`. P1c gave the demo points real English names ("Ribeira Palace", "St George's Castle") and carried the Portuguese names as
`search_keywords` ("Paço da Ribeira", "Castelo de São Jorge"), so the search finds them in both languages but the MARKER label, the search
result rows, the "Nearby" cards of points without a header title and the placed-model chip ("X is placed by the wall") stay English on a
Portuguese card. Same family as the search having no visitor language (`_2.6`) and the status names (`_3.3` TODOs). A `_2.x` domain item:
`name_by_language` / `summary_by_language` on `POIData` (like `label_by_language` on the category), read by the marker label, the search
rows and the chip through the card's language, with one Editor row per other wall language under the name.

## From `_3.3` P4.1 (the theme pipeline, 2026-10-23)

- **An app's own family colour.** A family's `ColorToken` must now be a THEME colour (`ThemeResolver`; `FamilyIconAndWordsTests` and the
  `FamilyDefinition` comment say so). Before P4.1 the contract said "an app family names a token its own stylesheet defines" (read from the
  card root's `customStyle`); no app did, no test covered it, and keeping that path would have been a second colour source beside the theme.
  When an app first needs a family colour of its own: let an app add values to the theme (an app theme overlay asset merged by
  `ThemeController.Load`, generated with the framework's), not a USS read in C#.
- **The real audio chip lays out a sub-pixel apart with Unity in front / in the background.** The Lamp's hero audio chip (title + time
  lines) renders 1256 px a hair apart (max channel delta 213, same colours, stable within each state) depending on `Application.isFocused`
  during the run -- found by `ThemePixelSetTests`, on the tree BEFORE P4.1. Harmless to a visitor; a test that compares text pixels of the
  real card across runs must key by focus (the pixel set does). Worth one look when the header is redesigned (P5): what in the chip depends
  on focus (the real clip's load timing on a throttled Editor?).
- **Text pixels depend on the font atlas's fill history.** The Editor's dynamic font atlas is shared by every scene and filled in the order
  text is drawn since it was last reset: twice, in a job right after a re-import, the Lamp card drawn first (`ThemePipelineTests`) moved 1-10
  text pixels of 7 gallery renders by 1-2 levels (A/B proven at P4.1 with the old and the new token file alike; resolved colours of 120
  elements bit-identical); in the full run, with the whole suite before it, the gallery matched exactly. P4.3's STATIC TextCore atlases
  (R12.3) should remove this: re-run the pixel set after another fixture then, and drop the "run it alone" note from `ThemePixelSetTests`.
- **`[Explicit]` does not keep a fixture out of an unfiltered MCP full run** (P4.1: the pixel set ran inside the full PlayMode run). The pixel
  set is now gated by a local switch file (`TestEvidence/ThemePixels/MEASURING`) and is ignored without it. Any other fixture marked
  `[Explicit]` to stay out of full runs needs the same kind of gate (grep `Explicit(` under `Tests/`).
- **`S4ContourTests.TheRing_DrawsAndTurns_AndAnimateZeroFreezesIt` (dev spike) is phase-and-frame-rate sensitive.** Red once in the P4.1
  full PlayMode run (8th test after Play Mode started: ring top pixel (39,89,206) -> (46,90,206), 0.031 < 0.05), green alone. A passing run
  started from the SAME first colour and reached (53,91,205) (0.055): the 1.5 s wait is REAL time while the shader turns on game time, which
  lags in the first, slow frames of a run (Unity caps a frame's delta). Fix when the spike is next touched: wait on `Time.time` (or read
  `_Time` back) for a quarter turn, or assert the change over a whole turn. Nothing of the theme reaches it (no `.ts-app` on its stage).

## "On the wall" cards: UI Toolkit blocks drawn in world space (2026-10-23, from `_3.3` R15.4)

The developer wants, later: tap a marker and a compact card (a short description + a picture slider) appears FIXED ON THE WALL
under it, reusing the card's UI Toolkit components and block previews. World-space UI Toolkit exists since Unity 6.2 (a
UIDocument with a World Space PanelSettings; input by camera rays against colliders). `_3.3` R15.2 keeps components and block
views host-agnostic (guard test) so this becomes a hosting job. Its own domain later, starting with a spike: the AR camera as
event camera, legibility at 1-3 m, sorting against the UGUI markers, a swipe on a world carousel, the cost with the AR session.

## From `_3.3` P4.3 (the app's fonts, 2026-10-23)

- **Italic is synthesized.** The quote's `-unity-font-style: italic` (`About.uss`) draws a slanted Atkinson Regular: the build ships no
  italic cut (the brief named 400 / 500 / 700 upright). google/fonts has `AtkinsonHyperlegibleNext-Italic[wght].ttf`: cut its 400 (and 700)
  like the uprights, build its assets, and let `FontAtlasBuilder.WireWeights` fill the weight table's `italicTypeface` slots -- when the quote
  look is designed (P5), not before.
- **Characters the faces lack.** Atkinson Hyperlegible Next has no glyph for 36 characters of Latin-1 + Latin Extended-A (the soft hyphen
  U+00AD, the micro sign U+00B5, Esperanto / Latvian / Sami letters such as C / G / H / J / S with circumflex and O / U with macron); Fraunces
  lacks only U+0149 and U+017F (both deprecated). Today no shipped text uses them (`ShippedTextGlyphTests` would name the character and its
  source). When a wall's content needs one: give the body UI asset a FALLBACK face (TextCore `fallbackFontAssetTable`, a static asset of a
  face that has it) rather than a dynamic atlas.
- **`FontKeyLibrary` is Shared now, but lives in `Runtime/UI/World/Markers/`.** Since P4.3 it serves both text systems (R15.1: "Shared"). It
  moves with the R16 "3b UI folder layout" step (`AssetDatabase.MoveAsset`, GUID kept), with `FontLibrary.asset`; nothing references it by
  path except the tests' `FrameworkFontLibraryPath` constants and the POI Editor's `DefaultFontLibraryPath`.
- **The P4.1 pixel set's baselines are pre-font now.** Its `TestEvidence/ThemePixels/baseline/` renders were drawn in Unity's default face; P4.3
  changed every text on purpose. Before P4.4 uses the set (a "no pixel moves" step), delete `baseline/` and collect anew. The P4.1 note "text
  pixels depend on the font atlas's fill history" should be gone with the static atlases: check it then (run the set after another fixture).

## From _3.3 P4.4a (the header and the hero, 2026-10-23)

- [_3.2, nav] **A jump right after a fling inherits the fling.** A real drag that ends clamped at the top of the stack keeps the ScrollView's
  velocity; a jump started next (a dock tap) lands, then the leftover velocity carries the stack on (probe: landed 8823, drifted to 8764 over
  ~70 frames, the block stayed at 8862). NavController should stop the stack's inertia when a jump starts. Test side worked around in
  PoiCardBottomElementsSceneTests.ScrollToTop (a resting finger stops the fling).
- [_3.3 P5 / P8, hero] **A long hero title fills the hero.** At 390 x 844 the hero is Half's view (281 px): a two-line title + the line + two
  action rows leave the picture only a thin band (The Lamp EN), and the gallery's six-line title fills it (the hero grows past Half: words
  are never squashed). Options for the review: a taller hero than Half's view, one action row (Listen as an icon pill), an Editor warning
  on long hero titles.
- [x] [_3.3, scrim] (closed P4.4b part 2) The Lamp's eyebrow fell to 6.69:1 once the hero's actions took one row; --ts-scrim-hero alpha
  0.92 -> 0.94 is chosen against a pure-white pixel, so every picture keeps the eyebrow >= 7.75:1 and the title >= 9.34:1 (ThemeContrastTests).
- [tests] The MCP get_test_job lists at most 25 failures: after a red-heavy targeted run, re-run the fixtures after the 25th by name.
- [tests, flake?] PoiCardTopElementsSceneTests.ARealDragOfTheStack_MovesTheMark_ToWhatTheScreenShows(progress_map) red once (block_30 vs 31)
  in a targeted run, green alone and in the full run.

## The glyph test asks card strings to be in the MARKER fonts (2026-10-24, from `_3.3` P4.4b)

P4.4b dropped Spanish's native name ("Español") from the card's settings panel because "ñ" is not in the marker fonts' fixed
character set -- but the panel draws in the CARD fonts (Atkinson / Fraunces, Latin-1 + Latin Extended-A: "ñ" is there).
`ShippedTextGlyphTests` checks every shipped string against both marker fonts. Scope it: strings a MARKER can show (POI names, level
names, marker labels) -> the marker fonts; card strings -> the card fonts. Then the framework can ship native names for more
languages (es, fr, de, it...) without rebuilding the marker atlases.

## Watched flakes (2026-10-26, seen in `_3.3` S2a's full runs; each green alone)

- `ConfigUndoGroupingTests.DraggingASlider_IsOneUndoStep` -- red once in a background full EditMode run AFTER H1 made the drag one
  editor tick: the one-tick gesture did not remove every cause. Next sighting: run the H1 recorder on that run to see what moved
  between the drag's events.
- `PoiCardInterleavedSpySceneTests.TheLamp_RealDragsDownTheWholeCard_...` ("drag 20 at 6279") -- red once in a background full
  PlayMode run, green in the targeted run and 7/7 alone. Likely a drag landing during the sheet's settle (the "wait on the condition,
  not on seconds" rule of `40-testing.md`): check the walk waits for the stack to rest between drags.
