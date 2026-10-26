# Plan tracker -- _3.3 S2b: the media components and every row's motion

Brief: R16 row 5b. Baseline: none (HEAD 0ed5b7c; S2a targeted 1120 / 0 failed; its full runs' two reds are watched flakes).

## GATE
- [x] push: 0 ahead, nothing to push
- [x] captures: TestEvidence/Gate/gate_game_view-2.png (Game view, LivingRoom scan + The Painting) + gate_poi_editor.png (window only; closed, 0 left)
- [x] compile: last Tundra ExitCode 0 (the 36 old error CS lines are an earlier session's, before it)
- [x] SDF fonts unmodified

## Decisions (autonomous)
- media components + the motion driver in Components/ (any panel); maths in pure ElementMotionRule / ElementMediaRule
- ONE clock-driven C# driver per composed view (ElementMotion): transform / opacity only; scroll found by GetFirstAncestorOfType<ScrollView>
- Reduce Motion = every effect still (reveal included: "identical frames"); ReducedOf drops CrossFade
- element taps only on the Page surface; on the preview the whole block opens the page; carousel swipes on both
- PictureViewer (shared lightbox page) used by the gallery and the picture elements; TakeoverPageDrawer takes ITakeoverPage -> gallery's R15.2 allow-list line removed
- TakeoverView.GoBack: a takeover opened over a block's page returns to that page (its scroll kept) on Back; Close (jumps, card close) never returns
- carousel = NavStrip + opt-in snap (NavStripRule.SnapOffset), first item on the 16 pt text inset, strip full width
- block entry (rise + fade) for composed blocks only (the story's preview)
- content glides with the returning bar: CardChromeView eases the scroll offset over the bar's own transition duration (ScrollView scrolls by translate: no second translate)
- the "not built yet" plate + BlockBindContext.DevGallery deleted
- review fix: a composing kind's slot heading hidden on the card (words kept for navigation)

## Steps
1. [x] Review fix -- BlockStackView heading class for composing kinds (+ section page); test
2. [x] Pure rules -- ElementMotionRule (Entered, Stagger, Reveal pose, Parallax, Zoom, Pan, Mover, reduce), ElementMediaRule (frame aspect), NavStripRule.SnapOffset, CardChromeRule.GlideOffset; EditMode tests + mutants
3. [x] Media components -- PictureElement, PicturePairElement, CarouselElement, VideoElement, PictureViewer; ITakeoverPage; GoBack + return in CardFullViewOpener; guard allow-list; plate removed; Components.uss media looks
4. [x] Motion -- ElementMotion driver in ComposedBlockView (rows + block entry), Finish/Tick seams; CardChromeView glide
5. [x] Phase A -- gallery media entries + CardStoryMediaGalleryTests (every media element x style x mode; frame sequences reveal / parallax / slow zoom; reduce = identical; real taps / swipes); existing story tests re-based
6. [x] PART 4 -- The Painting's story media rows through the Editor's config path (EN + PT)
7. [x] Phase B -- PoiCardStorySceneTests extended (preview picture, page rows, carousel swipe h/v, video tap -> takeover -> Back to the page; EN/PT; light/dark) + gated cost measurement
8. [x] Runs -- full EditMode; targeted Card|Poi|Theme|Section; FULL PlayMode; full EditMode (all background)
9. [x] Docs -- _3.1 5.5 / 3, _3.3 Build log + R16 row, 10-structure.md, RUN TIMES; final report; LivingRoomScene open

## Notes / re-bases
- full EditMode 2212 / 0 failed / 1 skipped (recorder) after the config write; rules 45/45; mutants: Mover + stagger cap killed, the enter line's < vs <= an equivalent mutant (float boundary): test judges +-0.5 px
- Phase A: CardStoryMediaGalleryTests 21/21 + CardStoryGalleryTests re-based (Finish() before judging renders; plate test removed); Phase B: PoiCardStorySceneTests 13 + gated cost (skipped until MEASURING) -- story fixtures 66/66 + 1 gated skip
- found by the captures: preview clip cut the full bleed (clip now bleeds, rows take the margin back); preview captions removed on the compact preview; badge words on their own plate (the contrast helper reads outside the label)
- found by red tests: an unbound pooled VideoElement held the player panel (removed); TokenPx reads no component token (page padding read instead)
- runs: targeted Card|Poi|Theme|Section 1154 (2 red: gallery heading check fixed; sticky action not reproduced, watched); FULL PlayMode 1404 / 0 failed / 49 gated; full EditMode 2212 / 0 failed; cost: motion 261.8 batches / 0.366 ms vs still 261.8 / 0.365 ms
- review question: full bleed reaches the PAGE edge (16..374), the full view's own gutter stays (S2c Page Chrome)
