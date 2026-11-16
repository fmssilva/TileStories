# Audit A: card and sections (saved from the auditor's hand-back)

60 rows: SAME 20 · ADD 11 · MERGE 11 · DROP 5 · RENAMED 13.

## ADD
- s1a Full height: To the top * / Leave a camera strip (48 pt short, --ts-sheet-top-gap). draws.
- bd:Place In AR Placed model chip: Shown * / Hidden (a chip above the card at every stop, Remove on the card that placed it). draws. [navigation.placed_model_control]
- s0card Forget saved answers (button; answers, votes, revealed questions, Contents ticks, picked language). Editor only.
- s4f new effect Own strip picture (Edges and Line dividers): File, Height 24 pt; "your wall's own motif, repeated across; its left and right edges must meet". [sections.divider_image]
- s2start Motion: Reveal * / Still (drop Drift: endless motion beside text, D6). draws.
- Notice on s2start/s2div/s2tint while Style = Cards: "section starts, dividers and tints do not draw".
- s1b Open one at a time (indented under Accordion): Off * / On. behaviour. (also report B -> s5expand)
- s2 Reset to the framework's sections (button). Editor only.
- s2 notice "<point> shows N sections: each gets under 60 pt" (also B).
- w3 filter chip "App texts" (CardStringSources app blocks).
- Aa and w3 "Back to the framework's wording" per word; the placeholder shows that wording.

## MERGE
- s1a Opens at: Peek / Half * (drop Full: never opens Full by itself, wall stays in view, D8). Unity Peek is the 57 pt compact header; Lab draws Peek 200 px.
- s1a Half height max: Unity 0.25..0.40 = 0.40 (clamps); Lab 0.4..0.8 = 0.62. Auditor: 0.30..0.60, default 0.40 (measured Half 337.6/844 = 0.40).
- s1c "When another sound starts" (audio or video): Fade and switch * (0.5 s; an audio guide stopped by a video resumes from its play button) / Lower the other / Queue (a new AUDIO waits; a video still pauses the audio). Drop Mix. behaviour. Unity today: audio vs audio switch/queue; audio vs video fixed: last started pauses the other; a muted header loop never counts.
- Reduce Motion: s4m Wall motion: Full * / Reduced (phone's Reduce Motion always wins). (B proposes a visitor Motion setting instead: combine.)
- Unity peek_next_card = the horizontal swipe-strip slice (CardTrackRule, Timeline, Related), NOT the Lab's s1peek. -> el:carousel Layout: Peek strip * / Whole cards / One at a time; same choice on Timeline horizontal and Related strip.
- s2start Style: Tiles * / Wave / Arches / Skyline / Fade (tint only) / Plain (no edge no tint) + any s4f Edge + Own strip picture. Drop Bands (tints the whole section). Lab's current Plain = Unity's Fade.
- s2div two rows: Between sections: None * / Hairline / Tile Ornament / Tile Row / Mosaic Strip / Own strip picture (only before a Fade or Plain start); Between blocks: the Lab's row (Unity always draws a hairline between blocks of an Open section; Lab default None).
- s2cfg Block order (with B).
- sec:* Shown: On the card and the bar * / On the card only / Hidden.
- sec:* Icon: one set = Unity's 25 (CardIcons.All, minus more, menu, open_block) + Lab's star and ar.
- s5term Opens as: Popup by the word * / Under the paragraph.

## SAME notes
- s2tint Colour: Unity Neutral = warm grey tint; Lab's Neutral draws no tint. Intensity min 0.04 vs 0.
- Tap outside closes: take Unity's (?) text: a tap on a marker still selects it; a drag never closes; the X and a swipe down always close.
- w1: Unity fallback = first in list; Lab picker has only 4 languages: offer the full code list; Lab has no reorder.
- Preview language change clears the language saved by earlier taps.
- sec:* Start lacks Skyline (Lab bug).

## DROP
- Android Earbud Check (platform workaround; the framework decides). Test guides texts. Vary (breaks "one motif used once"). Chosen Icon per group. Framework defaults table (duplicates Pick default…).

## Lab-only risks
- Top band Clear: title on scrolling text, risks D1/D8: drop or give the title an opaque chip.
- s1c help mentions only a video over the audio guide.

## Guide corrections
- 6.2: s1peek (next POINT's card) is NOT built; Unity's Peek Next Card is the strip slice.
- 6.6: Own stock media IS built (default_media_library_resources_path, CardMediaLibraryLookup).

## Gaps (proposals)
1. w1 Opens in: The phone's language when the wall has it * / The fallback.
2. s1mini With the screen locked: Pause * / Keep playing (walking audio guide in the pocket).
3. s1c When earbuds disconnect: Pause * / Keep playing on the speaker.
4. s5term Underline a term: The first time on the card * / Every time.
5. s1mini Resume where you left off: On * / Off.
