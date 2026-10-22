Card media of the LivingRoom dev wall (Detail Card fixture, _3.1_POI_Card_Blocks.md section 9; real content since _3.3 P1b).
Loaded at runtime through Resources: card_settings.media_resources_path = LivingRoom/CardMedia.

LOCAL TEST WALL, NOT FOR SHIPPING. The pictures, the video and the narration are real-shaped content gathered for testing the card.
Most are freely licensed (public domain, CC BY, CC BY-SA, ODbL) and SOURCES.md (next to this file) records each file's author, licence,
source URL and changes -- but a build given to anyone else still needs those attribution lines shown in the app and a final licence
check of each file (the ship blocker of _3.1 stays open).

Import settings: pictures as the wall's other card pictures (max 2048, compressed, quality 50) EXCEPT Non Power of 2 = None and no
mipmaps (a power-of-2 stretch changed every picture's proportion, and ETC2 cannot compress a mipmapped non-power-of-2 picture);
narrations Streaming + Vorbis; the video transcoded to H.264 at a Low bitrate.

images/ (_3.3 P1b; source line = SOURCES.md)
paco_hero.jpg               -- The Painting's hero (2048x1152): Terreiro do Paco in 1662, Dirk Stoop, public domain
paco_gallery_1..4.jpg       -- its gallery: royal procession 1662 (anonymous, PD; 716x412, cropped 3 px for ETC2), ruins of the Opera do
                               Tejo 1757 (Le Bas, PD; 1048x784), palace front 1687 (Joao dos Reis, PD; portrait 1536x2048), the square
                               from the river today (Jakub Halun, CC BY 4.0)
paco_before_1755.jpg        -- before_after BEFORE + image_choice option 1: the palace before the earthquake (anonymous, PD)
paco_after_today.jpg        -- before_after AFTER + image_choice option 2: Praca do Comercio today (Dguendel, CC BY 4.0)
paco_hotspots.jpg           -- hotspot_image: the palace before 1755, uncropped (anonymous, PD)
paco_map.jpg                -- today_map: OpenStreetMap tiles, pin on the square (ODbL, attribution drawn on the picture)
azulejo_detail.jpg          -- zoom_image (both points): ships at the Ribeira das Naus, Grande Panorama de Lisboa (RickMorais, CC BY-SA 4.0)
castle_hero.jpg             -- The Lamp's hero (2048x1152): towers and walls (deror_avi, CC BY-SA 3.0)
castle_then.jpg             -- the castle on the Grande Panorama tile panel (RickMorais, CC BY-SA 4.0): hotspot_image + split_then_now THEN
castle_now.jpg              -- the castle hill from the river (Massimo Catarinella, CC BY 3.0): split_then_now NOW
castle_gallery_1..4.jpg     -- the Lamp's four gallery looks and Lamp - Military's gallery: approach (Ricardo Tulio Gandelman, CC BY 2.0),
                               river and bridge (jad99, CC BY-SA 2.0), Dom Fradique Gate (to lobato, CC BY 2.0; portrait), tower at dusk
                               (Robert Nyman, CC BY 2.0)
castle_photo_1.jpg, castle_photo_2.jpg -- posters (Lamp chapters video, Lamp - Military's header loop; Lamp - Residential's video and
                               bento): Aleksandr Zykov / Peter, CC BY-SA 2.0 (2048x1364, cropped 1 px for ETC2)
castle_map.jpg              -- today_map of the castle: OpenStreetMap tiles (ODbL)

audio/ (_3.3 P1b narrations: own text, synthesized locally; EN = Microsoft David, PT-PT = Piper pt_PT-tugao-medium)
guide_castle_en.mp3 + .vtt  -- 164.5 s, 26 exact cues: The Lamp's guide (hero chip + player), English
guide_castle_pt.mp3 + .vtt  -- 129.0 s, 26 cues: its Portuguese file (the clip field's Per Language row)
guide_paco_en.mp3 + .vtt    -- 191.4 s, 30 cues: The Painting's guide, English (and Lamp - Economic's player, without captions)
guide_paco_pt.mp3 + .vtt    -- 144.0 s, 30 cues: its Portuguese file
lamp_tone.wav, lamp_tone.vtt -- generated PLACEHOLDER tone (20 s, four sections, a chime per section so a seek is audible) + its four
                               cues; CC0, made by Assets/Apps/LivingRoom/generate_card_audio.py (Lamp - Military's chip: the seek fixture)

video/ (_3.3 P1b)
lisbon_square_castle_55s.mp4 -- 54.9 s, 854x480, ambient sound: three own-work clips by deror_avi joined (CC BY-SA 3.0): the square, the
                               view from the castle, performers in the castle. Audio resampled 16 -> 44.1 kHz for the Windows importer.
                               3.8 MB in a build
lisbon_square_castle_55s.vtt, lisbon_square_castle_55s_pt.vtt -- three cues that DESCRIBE the picture (EN; PT = the Per Language file)
lisbon_square_castle_poster.jpg -- the inline video's poster: a frame at 10 s (852x480)

placeholder_hero.png        -- 512x288 generated gradient (CC0): the media seam's fixture file (MediaSourceTests, PoiCardSceneTests)
models/146267-LivingRoom2-tex.glb -- the room scan: model_3d's LivingRoom-only fixture (The Lamp's block_59)
