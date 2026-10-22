"""Stitch OSM standard tiles into a 768x432 (16:9) map centred on a point, draw a pin and the ODbL attribution."""
import math, os, sys, time, urllib.request, io
from PIL import Image, ImageDraw, ImageFont
UA = "TileStoriesContentBot/1.0 (porteiroconsigo@gmail.com; one-off test-wall map export)"
OUT = r"C:\Users\franc\Desktop\TileStories\proj_guides\detail_card\content\images"
CACHE = os.path.join(os.path.dirname(__file__), "tiles"); os.makedirs(CACHE, exist_ok=True)

def px(lat, lng, z):
    n = 2 ** z * 256
    x = (lng + 180) / 360 * n
    y = (1 - math.asinh(math.tan(math.radians(lat))) / math.pi) / 2 * n
    return x, y

def tile(z, x, y):
    f = os.path.join(CACHE, "%d_%d_%d.png" % (z, x, y))
    if not os.path.exists(f):
        req = urllib.request.Request("https://tile.openstreetmap.org/%d/%d/%d.png" % (z, x, y), headers={"User-Agent": UA})
        open(f, "wb").write(urllib.request.urlopen(req, timeout=40).read()); time.sleep(1.2)
    return Image.open(f).convert("RGB")

def make(name, lat, lng, z, W=768, H=432):
    cx, cy = px(lat, lng, z)
    x0, y0 = cx - W / 2, cy - H / 2
    tx0, ty0, tx1, ty1 = int(x0 // 256), int(y0 // 256), int((x0 + W) // 256), int((y0 + H) // 256)
    big = Image.new("RGB", ((tx1 - tx0 + 1) * 256, (ty1 - ty0 + 1) * 256))
    for tx in range(tx0, tx1 + 1):
        for ty in range(ty0, ty1 + 1):
            big.paste(tile(z, tx, ty), ((tx - tx0) * 256, (ty - ty0) * 256))
    ox, oy = int(x0 - tx0 * 256), int(y0 - ty0 * 256)
    im = big.crop((ox, oy, ox + W, oy + H))
    d = ImageDraw.Draw(im)
    px_, py_ = W // 2, H // 2
    d.ellipse([px_ - 11, py_ - 11, px_ + 11, py_ + 11], fill=(200, 30, 30), outline=(255, 255, 255), width=3)
    d.ellipse([px_ - 3, py_ - 3, px_ + 3, py_ + 3], fill=(255, 255, 255))
    try:
        font = ImageFont.truetype("arial.ttf", 13)
    except Exception:
        font = ImageFont.load_default()
    t = "\u00a9 OpenStreetMap contributors"
    tw = d.textlength(t, font=font)
    d.rectangle([W - tw - 12, H - 22, W, H], fill=(255, 255, 255))
    d.text((W - tw - 6, H - 19), t, fill=(40, 40, 40), font=font)
    im.save(os.path.join(OUT, name), quality=92, optimize=True)
    print(name, im.size)

make("castle_map.jpg", 38.71390, -9.13340, 17)
make("paco_map.jpg", 38.70790, -9.13620, 17)
