# Plan tracker -- _3.2 step 16 (full-view breadcrumb + sibling swipe) + the three decided 15b findings

GATE (done): UnityMCP answered (telemetry_status); 1 commit pushed (7abf8e2..1193ec4); captures `TestEvidence/Gate/gate_3_2_step16_{gameview,poieditor}.png`
opened (window closed, 0 left); tree compiled (6 old `error CS` lines precede the last two builds, ExitCode 0); SDF fonts unmodified (git clean);
EditMode baseline 1938/1938 (95 s, unfocused) on clean 1193ec4 (trusted).

Decisions: siblings = the block's model SECTION's targets whose kind lists Expandable (= family under the default Group By), stack order, none for
a non-target or a section of one; the full view of a block opens ALL its siblings as the takeover's pages (ShowPage already hands media over in the
right order); the crumb = title > section pill (tappable, 48 px, JumpTo(section, "breadcrumb") -- the one jump path) > name; chips one clipped line
(NavStrip), current = bold + ring; a TURN pauses the leaving view (Collapsing), closing keeps step 12's rule; Back closes (the card never moved).
Teaser takeovers a block opens itself (lightbox, video / 3D takeover display) keep "title > name" (IBlockHost.OpenTakeover has no block key: logged).

- [x] P1.1 CardLayoutRule.LayoutPicksTheTopBar (pages + bento) read by TopElement AND the Editor's Top Bar row; tests: CardLayoutRuleTests,
      DetailCardEditorTabTests (Bento hides the row, value kept, Ctrl+Z), BentoLayoutSheetTests + PoiCardBentoLayoutSceneTests rewritten; (i) + guides
- [x] P1.2 BentoLayoutRule caps a row at its tallest tile's content (contentHeight delegate), RoomLeft; CardBentoView measures; tests:
      BentoLayoutRuleTests (+cap), CardNavGalleryTests bento promise (small case: room under the last tile, every row as tall as its content)
- [x] P1.3 PoiCardLayoutSceneTests tabs: track read before; owner holds the ONE paused clip; mutation (Collapsing -> Stop) red, restored
- [x] P2.1 FullViewSiblingRule (pure) + FullViewSiblingRuleTests (Lamp, app kind w/o Expandable, group_by format, family of one)
- [x] P2.2 TakeoverView crumb row + CrumbText + ShowSection + SectionTapped + chips strip; BlockFullView PagesFor + SwipeGrab + turn pauses;
      sheet OpenFullView siblings + crumb jump; Media.uss; Crumb.text readers -> CrumbText; EditMode FullViewSiblingsSheetTests
- [x] P2.3 Phase A CardNavGalleryTests full-view crumb + chips over the 5 cases (renders)
- [x] P2.4 Phase B PoiCardFullViewSiblingsSceneTests (real swipe, real chip tap, Back at the block, video owner, preview owner, family pill, Listen)
- [x] P3 captures judged: Card/FullView_Lamp_media_{en,pt}_390.png, Card/FullView_Lamp_listen_en_390.png, Nav_full_view_<case>, Nav_Lamp_bento_{en,pt}_390.png
- [x] P4 docs: _3.2 (status 16, TODOs removed, 5.7 built, 6.1 bento, 7), _3.1 TakeoverView breadcrumb, _5.1 Top Bar row, 10-structure, __mixed_TODOs
- [x] P5 full EditMode + PlayMode fixtures named Card / Poi / SizeComparison + grep TakeoverView / BlockFullView; zero failures
