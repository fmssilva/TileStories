# Current plan: POI Detail Card -- 9-pre (Portuguese spelling) + step 9A (audio_guide + card audio)

Baseline (2026-09-28, session start): 0 error CS; EditMode 1360/1360; PlayMode 581/581 (Unity in the foreground; re-run in background before the final).
Gate: Game view + POI Editor window captures opened and read; capture window closed (0 left).

Picked up mid-way: a prior session had already built nearly all of PART 1 and PART 2 uncommitted (real code, real tests, real
fixtures -- not stubs), but never checked the boxes below or updated the docs. Read-only audit (2026-09-28) confirmed items 1.1-1.3,
2.1-2.11 done; this pass closed the remaining gaps: recovered a stuck UnityMCP test job (Unity restarted), re-verified the full
baseline clean (EditMode 1407/1407, PlayMode 619/619, 0 error CS -- the one failure the stuck/corrupted run had reported
(a hero_chip subtitle layout assert) did not reproduce in the clean re-run: noise from the unfocused/orphaned job, not a real bug),
and closed 2.12 (docs) and 1.4's missing PT size_comparison capture.

## Media triage (developer's files in Apps/LivingRoom/MediaAssets)
- Audio/castelo_s_jorge_audio.mp4 is a VIDEO (H.264 + AAC), Videos/castelo_s_jorge_video.mp3 is an AUDIO (mp3, PT, ~3:28): names / folders were swapped.
- PT_Tourism_Audio.mp3 = English promo, ~7:18 (the long clip). castelo mp3 = PT narration, ~3:28 (the Lamp's real guide).
- The card loads media through Resources (card_settings.media_resources_path = LivingRoom/CardMedia): files outside a Resources folder are unreachable and do not ship.
- .avif is not importable by Unity (leave / delete). Videos are 9B: stay in MediaAssets/Videos (fix the swapped mp4 into Videos/), no Resources copy yet (30 MB in every build).
- Done (found already applied on audit): the swap is fixed (Videos/castelo_s_jorge_video.mp4, Videos/PT_Tourism_video.mp4), the real
  clips and photos are triaged into Resources/LivingRoom/CardMedia/audio + photos/, and MediaAssets/Images/castelo_s_jorge_1.avif is
  left in place (not importable, not deleted -- the developer's own source file, `10-structure.md` now documents the decision).

## PART 1 -- 9-pre (no behaviour change)
- [x] 1.1 Tests: visitor text = valid (NFC, no control chars, no U+FFFD); Editor labels/help/where-notes stay ASCII; a test proves an accented Editor help text still FAILS (shared `EditorTextChecks.AssertAscii`) -- `Tests/Editor/Blocks/TextChecks.cs` + `TextChecksTests.cs`
- [x] 1.2 pt rows of CardStrings.asset + LivingRoomCardStrings.asset (through Unity's serializer)
- [x] 1.3 pt card content of the LivingRoom fixture, both config copies (round trip identity checked first; identity fields untouched); update test literals
- [x] 1.4 PT render of The Lamp read item by item (feedback, poll, size comparison, rich text); font glyph check -- `PoiCardPortugueseTests.cs` (whole-card glyph check across every accented character + rich_text/feedback/poll captures); size_comparison's own PT test + capture is `LivingRoomCardSceneTests.InPortuguese_TheHeadingAndTheCaptionFollowTheCardsLanguage` (`Search_Lamp_SizeComparison_pt.png`), asserting the exact accented text ("Tao alto como quase quatro telemoveis empilhados." with the real accents) -- opened and read this pass: Castelo de Sao Jorge / Qual e o tamanho? / Telemovel / the caption all render correctly, no missing-glyph boxes

## PART 2 -- 9A audio_guide
- [x] 2.1 Media: MediaKind.Audio + Captions (.vtt) in MediaPathRule, Asset drawer (AudioClip / TextAsset), `.vtt` ScriptedImporter, ResourcesMediaSource keyed by full path (mp3 + vtt share a base name)
- [x] 2.2 Fixture files: real mp3s moved into Resources/LivingRoom/CardMedia/audio (streaming import), generated WAV tone + vtt (stdlib Python), placeholder .vtt for the real clips, README licence lines
- [x] 2.3 Pure rules (Runtime/Blocks/Audio/): VttRule, AudioSpeedRule, AudioTimeRule, AudioSwitchRule, AudioFadeRule, AudioInterruptionRule (+ output route drop rule) -- fade lives inside AudioSwitchRule (no separate file: a design choice, not a gap)
- [x] 2.4 CardAudioService (ICardAudio, IAudioOutput seam, injectable clock) + UnityAudioOutput + CardAudioPlayer (ONE AudioSource, OnApplicationPause / OnAudioConfigurationChanged / Android poll behind a flag)
- [x] 2.5 Config: container.audio_when_another_starts (switch|queue), container.audio_android_output_poll; Editor rows + help; keep_audio_on_close live
- [x] 2.6 Block definition (family guides, `player` / `hero_chip`, PinnedTopVariants), CardStrings keys (en + pt), CardIcons Play / Pause, Guides.uss + tokens
- [x] 2.7 Views: AudioGuideBlockView (player, hero_chip), AudioScrubber, MiniPlayerView + host wiring (keep on close, tap reopens)
- [x] 2.8 Phase A: gallery entries (both variants x short / long / no captions) + real taps
- [x] 2.9 Fixture: Lamp (PT real clip + captions: hero_chip + player), Lamp - Military (hero_chip, tone WAV), Lamp - Economic (player, long English clip, no captions)
- [x] 2.10 Phase B: real Lamp tests (play, captions follow, mini-player, reopen, switch / queue, live edit, interruptions)
- [x] 2.11 Captures read item by item (both variants idle / playing / captions / PT, mini-player, Editor rows)
- [x] 2.12 Docs: _3.1 rows 9-pre / 9A + TODOs + design history, 10-structure.md, _5.1, __mixed_TODOs -- done this pass
- [x] 2.13 Final: refresh (0 error CS), EditMode + PlayMode with Unity in the background, close-out windows -- EditMode 1407/1407, PlayMode 619/619, 0 error CS (this pass, after recovering a stuck UnityMCP test job by restarting Unity)

## Listed for the developer (not done here)
- Device: Bluetooth connect / disconnect mid-narration on real Android (work plan risk register), Android poll flag default OFF.
- The stray `MediaAssets/Images/castelo_s_jorge_1.avif`: Unity cannot import AVIF; left as-is (not deleted, since it is the developer's own dropped-in source file) -- decide whether to convert it to jpg/png or remove it.

## Status: PART 1 and PART 2 complete and verified. Ready for the developer to review; not committed (per "commit only if the developer asks").
