"""Generate the LivingRoom Detail Card's placeholder pictures (_3.1 step 7, Tier 2 groups A and B).

Tile-panel-like pictures (white glaze, cobalt drawing, grout lines), each with a dark banner reading PLACEHOLDER and
what the picture stands for, so nobody mistakes them for real content. Python standard library only (no PIL): a small
PNG writer (zlib + struct) and a 5x7 pixel font. Output: Resources/LivingRoom/CardMedia/ next to this script.

Run: python generate_card_media.py   (then let Unity import the files)
"""
import math
import os
import struct
import zlib

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Resources", "LivingRoom", "CardMedia")

GLAZE = (242, 238, 227)      # tin white
COBALT = (31, 63, 143)
YELLOW = (217, 169, 58)      # antimony yellow
MANGANESE = (74, 47, 69)
GREEN = (60, 122, 78)
GROUT = (200, 194, 180)
HOLE = (120, 112, 104)       # bare mortar where a tile is missing
BANNER = (28, 24, 20)
INK = (250, 246, 236)
PAPER = (232, 226, 210)      # a street map's city blocks
STREET = (252, 250, 244)
RIVER = (126, 168, 204)

# 5x7 glyphs, one string of 5 bits per row
FONT = {
    "A": ["01110", "10001", "10001", "11111", "10001", "10001", "10001"],
    "B": ["11110", "10001", "10001", "11110", "10001", "10001", "11110"],
    "C": ["01111", "10000", "10000", "10000", "10000", "10000", "01111"],
    "D": ["11110", "10001", "10001", "10001", "10001", "10001", "11110"],
    "E": ["11111", "10000", "10000", "11110", "10000", "10000", "11111"],
    "F": ["11111", "10000", "10000", "11110", "10000", "10000", "10000"],
    "G": ["01111", "10000", "10000", "10011", "10001", "10001", "01111"],
    "H": ["10001", "10001", "10001", "11111", "10001", "10001", "10001"],
    "I": ["11111", "00100", "00100", "00100", "00100", "00100", "11111"],
    "L": ["10000", "10000", "10000", "10000", "10000", "10000", "11111"],
    "M": ["10001", "11011", "10101", "10101", "10001", "10001", "10001"],
    "N": ["10001", "11001", "10101", "10011", "10001", "10001", "10001"],
    "O": ["01110", "10001", "10001", "10001", "10001", "10001", "01110"],
    "P": ["11110", "10001", "10001", "11110", "10000", "10000", "10000"],
    "R": ["11110", "10001", "10001", "11110", "10100", "10010", "10001"],
    "T": ["11111", "00100", "00100", "00100", "00100", "00100", "00100"],
    "W": ["10001", "10001", "10001", "10101", "10101", "11011", "10001"],
    "Y": ["10001", "10001", "01010", "00100", "00100", "00100", "00100"],
    "1": ["00100", "01100", "00100", "00100", "00100", "00100", "01110"],
    "2": ["01110", "10001", "00001", "00110", "01000", "10000", "11111"],
    "3": ["11110", "00001", "00001", "01110", "00001", "00001", "11110"],
    "4": ["00010", "00110", "01010", "10010", "11111", "00010", "00010"],
    " ": ["00000"] * 7,
}


def write_png(path, width, height, pixel):
    rows = bytearray()
    for y in range(height):
        rows.append(0)  # filter: none
        for x in range(width):
            rows.extend(pixel(x, y))

    def chunk(kind, data):
        body = kind + data
        return struct.pack(">I", len(data)) + body + struct.pack(">I", zlib.crc32(body) & 0xFFFFFFFF)

    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(bytes(rows), 9)) + chunk(b"IEND", b"")
    with open(path, "wb") as f:
        f.write(png)


def tile_motif(u, v, ink, accent):
    """One tile's drawing at (u, v) in 0..1: a diamond, a centre dot and quarter circles in the corners (they join into
    circles across four tiles, like a real pattern panel)."""
    du, dv = abs(u - 0.5), abs(v - 0.5)
    if du + dv < 0.08:
        return accent
    if abs(du + dv - 0.30) < 0.035:
        return ink
    for cx, cy in ((0, 0), (1, 0), (0, 1), (1, 1)):
        if abs(math.hypot(u - cx, v - cy) - 0.32) < 0.035:
            return ink
    return None


def banner_text(width, height, text, scale):
    """The PLACEHOLDER banner: a pixel test of (x, y) -> colour or None."""
    text_w = len(text) * 6 * scale - scale
    band_h = 11 * scale
    top = height - band_h
    left = (width - text_w) // 2

    def at(x, y):
        if y < top:
            return None
        gx, gy = (x - left), (y - top - 2 * scale)
        if 0 <= gx < text_w and 0 <= gy < 7 * scale:
            ci, cx = divmod(gx // scale, 6)
            glyph = FONT.get(text[ci], FONT[" "])
            if cx < 5 and glyph[gy // scale][cx] == "1":
                return INK
        return BANNER
    return at


def panel(width, height, tile, ink, accent, label, missing=None, scene=None):
    """A tile panel: tiles of `tile` px with grout, a motif per tile, an optional scene drawn over the tiles, optional
    missing tiles (bare mortar) and the PLACEHOLDER banner."""
    banner = banner_text(width, height, "PLACEHOLDER " + label, max(2, width // 160))

    def pixel(x, y):
        b = banner(x, y)
        if b is not None:
            return b
        tx, ty = x // tile, y // tile
        if missing and missing(tx, ty):
            return HOLE
        u, v = (x % tile) / tile, (y % tile) / tile
        if x % tile == 0 or y % tile == 0:
            return GROUT
        if scene:
            s = scene(x / width, y / height)
            if s is not None:
                return s
        m = tile_motif(u, v, ink, accent)
        return m if m is not None else GLAZE
    return pixel


def castle(ink):
    """A castle outline over the tiles (walls, three towers with merlons), in the panel's ink."""
    def at(fx, fy):
        ground = 0.78
        if fy > ground:
            return None
        towers = ((0.18, 0.28, 0.32), (0.45, 0.57, 0.22), (0.74, 0.84, 0.34))
        for x0, x1, top in towers:
            if x0 <= fx <= x1 and fy >= top:
                if fy < top + 0.04 and int((fx - x0) / 0.025) % 2 == 1:
                    return None
                return ink
        if 0.12 <= fx <= 0.90 and fy >= 0.50:
            return ink
        return None
    return at


def street_map(width, height, label):
    """A plain street map (today_map, group B): city blocks between streets, a river along the bottom, a pin on the
    place (slightly left of the middle), and the PLACEHOLDER banner."""
    banner = banner_text(width, height, "PLACEHOLDER " + label, max(2, width // 160))
    pin_x, pin_y, pin_r = width * 0.42, height * 0.40, height * 0.07

    def pixel(x, y):
        b = banner(x, y)
        if b is not None:
            return b
        d = math.hypot(x - pin_x, y - pin_y)
        if d < pin_r * 0.45:
            return COBALT
        if d < pin_r:
            return YELLOW
        if y > height * 0.72 + 12 * math.sin(x / width * 6.0):
            return RIVER
        # - streets: a grid, one avenue at a slant
        if x % 96 < 10 or y % 72 < 8 or abs((x - y * 1.3) % 240) < 12:
            return STREET
        return PAPER
    return pixel


def main():
    os.makedirs(OUT, exist_ok=True)
    pictures = {
        # the header's hero (image_parallax, spotlight_crop): wide, a castle over a cobalt pattern
        "castle_hero.png": (768, 432, panel(768, 432, 48, COBALT, YELLOW, "HERO", scene=castle(COBALT))),
        # split_then_now: the same castle, an old ochre panel and a restored blue one
        "castle_then.png": (512, 384, panel(512, 384, 48, MANGANESE, YELLOW, "THEN", scene=castle(MANGANESE))),
        "castle_now.png": (512, 384, panel(512, 384, 48, COBALT, YELLOW, "NOW", scene=castle(COBALT))),
        # the gallery: four panels, each its own drawing colour so a render tells them apart
        "gallery_1.png": (512, 384, panel(512, 384, 64, COBALT, YELLOW, "GALLERY 1")),
        "gallery_2.png": (512, 384, panel(512, 384, 64, GREEN, YELLOW, "GALLERY 2")),
        "gallery_3.png": (384, 512, panel(384, 512, 64, MANGANESE, COBALT, "GALLERY 3")),
        "gallery_4.png": (512, 384, panel(512, 384, 64, YELLOW, COBALT, "GALLERY 4")),
        # before_after: the same panel whole, then with a patch of tiles gone
        "damage_before.png": (512, 384, panel(512, 384, 48, COBALT, YELLOW, "BEFORE")),
        "damage_after.png": (512, 384, panel(512, 384, 48, COBALT, YELLOW, "AFTER",
                                             missing=lambda tx, ty: 3 <= tx <= 6 and 1 <= ty <= 4 and (tx + ty) % 3 != 0)),
        # zoom_image: a large, finely drawn panel worth pinching into
        "tile_detail.png": (1024, 1024, panel(1024, 1024, 32, COBALT, YELLOW, "DETAIL")),
        # today_map (group B): where the place is today, a pin on a street map
        "castle_map.png": (768, 432, street_map(768, 432, "MAP")),
    }
    for name, (w, h, pixel) in pictures.items():
        write_png(os.path.join(OUT, name), w, h, pixel)
        print("wrote", name, w, "x", h)


if __name__ == "__main__":
    main()
