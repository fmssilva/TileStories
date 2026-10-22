"""Download the chosen Commons files, crop to the block's proportion, resize (long side <= 2048), save JPG q85.
Writes content/images/*.jpg and scratchpad/images_meta.json (feeds SOURCES.md)."""
import json, os, sys, time, io, urllib.request
sys.path.insert(0, os.path.dirname(__file__))
import cs
from PIL import Image

OUT = r"C:\Users\franc\Desktop\TileStories\proj_guides\detail_card\content\images"
SRC = os.path.join(os.path.dirname(__file__), "src")
os.makedirs(OUT, exist_ok=True)
os.makedirs(SRC, exist_ok=True)

# out, commons title, ratio (w/h or None = keep), focus (fx, fy), long side
SPECS = [
    ("paco_hero.jpg", "File:Terreiro do Paço em 1662.jpg", 16 / 9, (0.5, 0.40), 2048),
    ("paco_before_1755.jpg", "File:Terreiro do Paço antes do Terramoto de 1755.png", 4 / 3, (0.5, 0.45), 2048),
    ("paco_after_today.jpg", "File:Lisbon, view from Arco Triunfal to the statue of Joseph I of Portugal.JPG", 4 / 3, (0.5, 0.5), 2048),
    ("paco_gallery_1.jpg", "File:Cortejo Real no Terreiro do Paço, 23 de Abril de 1662.png", None, (0.5, 0.5), 2048),
    ("paco_gallery_2.jpg", "File:Ruinas da Ópera do Tejo após o Terramoto de 1755 - Jacques Philippe Le Bas, 1757.png", 4 / 3, (0.5, 0.5), 2048),
    ("paco_gallery_3.jpg", "File:Fogo artifício representado no terreno do Paço.jpg", 3 / 4, (0.5, 0.5), 2048),
    ("paco_gallery_4.jpg", "File:View of Praça do Comércio and Doca da Caldeirinha in Lisbon, 20250604 2034 9644.jpg", 4 / 3, (0.62, 0.5), 2048),
    ("paco_hotspots.jpg", "File:Terreiro do Paço antes do Terramoto de 1755.png", None, (0.5, 0.5), 2048),
    ("azulejo_detail.jpg", "File:Ribeira das Naus (Grande Panorama de Lisboa, MNAz).png", 1.0, (0.72, 0.55), 2048),
    ("castle_hero.jpg", "File:Castelo Sao Jorge Lisboa P1000883.JPG", 16 / 9, (0.5, 0.45), 2048),
    ("castle_then.jpg", "File:Castelo de São Jorge (Grande Panorama de Lisboa, MNAz).png", 4 / 3, (0.5, 0.5), 2048),
    ("castle_now.jpg", "File:CastleSaintGeorge.jpg", 4 / 3, (0.52, 0.5), 2048),
    ("castle_gallery_1.jpg", "File:Castelo de São Jorge (2) - Mar 2010.jpg", 4 / 3, (0.5, 0.5), 2048),
    ("castle_gallery_2.jpg", "File:Castelo de São Jorge (14005713291).jpg", 4 / 3, (0.5, 0.5), 2048),
    ("castle_gallery_3.jpg", "File:Entrance in the Castelo parish in Rua do Chão da Feira - “Dom Fradique Gate”.jpg", 3 / 4, (0.5, 0.5), 2048),
    ("castle_gallery_4.jpg", "File:Castelo de São Jorge (4166046851).jpg", 4 / 3, (0.4, 0.5), 2048),
    ("castle_peacock.jpg", "File:Lisbon, Castelo de São Jorge, peacock.JPG", 4 / 3, (0.45, 0.5), 2048),
    ("castle_photo_1.jpg", "File:Castelo de São Jorge, Lisbon (11977414404).jpg", 3 / 2, (0.5, 0.5), 2048),
    ("castle_photo_2.jpg", "File:Castelo de S. Jorge (10291024785).jpg", 3 / 2, (0.5, 0.5), 2048),
]


def fetch(title, need_w):
    d = cs.call({"action": "query", "titles": title, "prop": "imageinfo", "iiprop": "url|size|mime|extmetadata",
                 "iiurlwidth": 100})
    page = list(d["query"]["pages"].values())[0]
    ii = page["imageinfo"][0]
    ow = ii["width"]
    w = min(max(need_w, 400), ow)
    if w > 3800:
        w = ow
    if w < ow:
        d = cs.call({"action": "query", "titles": title, "prop": "imageinfo", "iiprop": "url", "iiurlwidth": w})
        url = list(d["query"]["pages"].values())[0]["imageinfo"][0]["thumburl"]
    else:
        url = ii["url"]
    cache = os.path.join(SRC, "%d_%s.bin" % (w, abs(hash(title)) % 10**8))
    if not os.path.exists(cache):
        for attempt in range(5):
            try:
                req = urllib.request.Request(url, headers={"User-Agent": cs.UA})
                open(cache, "wb").write(urllib.request.urlopen(req, timeout=120).read())
                break
            except Exception as e:
                print("   retry", e)
                time.sleep(6 * (attempt + 1))
        time.sleep(1.5)
    m = ii.get("extmetadata", {})
    g = lambda k: cs.strip(m.get(k, {}).get("value", ""))
    meta = dict(title=title, orig=[ow, ii["height"]], lic=g("LicenseShortName"), lic_url=g("LicenseUrl"),
                artist=g("Artist"), date=g("DateTimeOriginal") or g("DateTime"), page=ii["descriptionurl"], got_w=w)
    return cache, meta


def crop(im, ratio, focus):
    W, H = im.size
    if ratio is None:
        return im, (0, 0, 1, 1)
    if W / H > ratio:
        cw, ch = int(round(H * ratio)), H
    else:
        cw, ch = W, int(round(W / ratio))
    x0 = int(min(max(focus[0] * W - cw / 2, 0), W - cw))
    y0 = int(min(max(focus[1] * H - ch / 2, 0), H - ch))
    return im.crop((x0, y0, x0 + cw, y0 + ch)), (x0 / W, y0 / H, cw / W, ch / H)


meta_all = {}
only = sys.argv[1:]
for out, title, ratio, focus, long_side in SPECS:
    if only and out not in only:
        continue
    # thumb width so that the crop still has >= long_side px on its long side
    orig = cs.call({"action": "query", "titles": title, "prop": "imageinfo", "iiprop": "size"})
    ii = list(orig["query"]["pages"].values())[0]["imageinfo"][0]
    ow, oh = ii["width"], ii["height"]
    if ratio is None:
        cwf, out_w = 1.0, long_side if ow >= oh else long_side * ow / oh
    else:
        cwf = min(1.0, oh * ratio / ow)
        out_w = long_side if ratio >= 1 else long_side * ratio
    need_w = int(out_w / cwf) + 1
    path, meta = fetch(title, need_w)
    im = Image.open(path).convert("RGB")
    im, box = crop(im, ratio, focus)
    s = long_side / max(im.size)
    if s < 1:
        im = im.resize((round(im.size[0] * s), round(im.size[1] * s)), Image.LANCZOS)
    im.save(os.path.join(OUT, out), quality=85, optimize=True)
    meta.update(out_size=list(im.size), crop_box=[round(v, 3) for v in box], ratio=None if ratio is None else round(ratio, 3))
    meta_all[out] = meta
    print("%-22s %s <- %s (%s, %s)" % (out, im.size, title[5:60], meta["lic"], meta["artist"][:30]))

mp = os.path.join(os.path.dirname(__file__), "images_meta.json")
old = json.load(open(mp, encoding="utf-8")) if os.path.exists(mp) else {}
old.update(meta_all)
json.dump(old, open(mp, "w", encoding="utf-8"), ensure_ascii=False, indent=1)
