

synonyms not implemented

add the button create new POI in the specific mrker sub tab

add a button create empty json config that "loads" an empty default json config to the scene with 1 default specific poi marker also... 

install de voice search thing

check thee FOV splike thing???


## From the POI Detail Card review (2026-09-27)

- Test renders live INSIDE `Assets/Screenshots/` (143 PNGs, 17 MB and growing ~150 per card tier). Unity imports
  every one as a texture (import time, `.meta` churn) and git tracks them all. Move every suite's evidence renders
  to a folder next to `Assets/` (like `TileStories/MarkerGalleryScreenshots/`), e.g. `TileStories/TestEvidence/<domain>/`,
  gitignore it, and keep only the few captures a doc links to. One shared helper for the output path.
- Flaky EditMode test seen twice across sessions: `TaxonomyRowIdentityTests` (`...ReportsBothRowsByPosition`,
  `...TypingADuplicateAndABlankCategoryLabel_...`), right after leaving Play Mode. Likely leftover window /
  undo state between tests; investigate in a taxonomy session.
- Commit hygiene: commit after every verified agent session (456 files were uncommitted after the card 6A session);
  keep `IPCE/` out of TileStories commits (separate repo or .gitignore).

