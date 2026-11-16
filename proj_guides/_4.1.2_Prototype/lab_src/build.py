"""Builds the World Editor Lab: joins the parts listed in PARTS, inlines every @@NAME@@ from media/NAME.txt.
usage: python build.py <out.html>   (output is UTF-8 with LF line endings on every OS)"""
import os, re, sys
here = os.path.dirname(os.path.abspath(__file__))
def read(path):
    with open(path, encoding="utf-8", newline="") as f: return f.read()
parts = [p.strip("\r") for p in read(os.path.join(here, "PARTS")).split("\n") if p.strip("\r")]
html = "".join(read(os.path.join(here, p)) for p in parts)
html = html[:-1]  # each part ends with a newline; the original file did not
def media(m):
    f = os.path.join(here, "media", m.group(1) + ".txt")
    if not os.path.exists(f): sys.exit(f"missing media: {m.group(1)}")
    return read(f)
html = re.sub(r"@@([A-Z0-9_]+)@@", media, html)
with open(sys.argv[1], "w", encoding="utf-8", newline="\n") as f: f.write(html)
print("built", sys.argv[1], len(html), "bytes")
