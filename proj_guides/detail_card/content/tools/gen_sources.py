"""Regenerate SOURCES.md rows for the pictures from images_meta.json (written by build_images.py)."""
import json, re, os
here = os.path.dirname(__file__)
meta = json.load(open(os.path.join(here, "images_meta.json"), encoding="utf-8"))
WHAT = {
 "paco_hero.jpg": "Hero of the Ribeira Palace card: Terreiro do Paço painting, c. 1662",
 "paco_before_1755.jpg": "before_after BEFORE: the Terreiro do Paço and palace before the earthquake",
 "paco_after_today.jpg": "before_after AFTER: Praça do Comércio today from above",
 "paco_gallery_1.jpg": "Gallery 1: royal procession in the Terreiro do Paço, 23 Apr 1662",
 "paco_gallery_2.jpg": "Gallery 2: ruins of the Ópera do Tejo, engraving 1757",
 "paco_gallery_3.jpg": "Gallery 3 (portrait): palace front and garden, print 1687",
 "paco_gallery_4.jpg": "Gallery 4: Praça do Comércio from the river today",
 "paco_hotspots.jpg": "hotspot_image: the palace before 1755, uncropped (5 spots)",
 "azulejo_detail.jpg": "zoom_image: azulejo detail, ships at the Ribeira das Naus (Grande Panorama, MNAz)",
 "castle_hero.jpg": "Hero of the castle card: towers and walls",
 "castle_then.jpg": "split_then_now THEN + hotspot_image: castle on the Grande Panorama tile panel",
 "castle_now.jpg": "split_then_now NOW: the castle hill seen from the river",
 "castle_gallery_1.jpg": "Castle gallery 1: cobbled approach to the walls",
 "castle_gallery_2.jpg": "Castle gallery 2: river and bridge from the walls",
 "castle_gallery_3.jpg": "Castle gallery 3 (portrait): Dom Fradique Gate",
 "castle_gallery_4.jpg": "Castle gallery 4: tower at dusk",
 "castle_peacock.jpg": "Fun-fact picture: peacock in the castle gardens",
 "castle_photo_1.jpg": "Castle photo 1 (video poster / photos): towers and wall walks",
 "castle_photo_2.jpg": "Castle photo 2 (video poster / photos): steps and walls",
}
def clean(s): return re.sub(r"date QS.*", "", s).strip()
for f, m in meta.items():
    a = m["artist"]
    a = "Unknown (anonymous)" if "Unknown author" in a else a
    ch = "cropped to %s (box %s), " % ("%.3f" % m["ratio"] if m["ratio"] else "no crop", m["crop_box"]) if m["ratio"] else "no crop, "
    print("| `images/%s` | %s | %s | %s | %s | 2026-10-22 | %sresized %dx%d to %dx%d, JPG q85 |" % (
        f, WHAT[f], a, m["lic"], m["page"], ch, m["orig"][0], m["orig"][1], m["out_size"][0], m["out_size"][1]))
