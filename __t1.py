# -*- coding: utf-8 -*-
# T1: fix the namespace wart (early close before POIEditorAddPoiTests).
import io

path = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests\Editor\POIEditorAddPoiTests.cs"
with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
    content = f.read()

nl = "\r\n"
old = (
    "        }" + nl +
    "    }" + nl +
    "}" + nl +
    "" + nl +
    "    // Tier-0 EditMode tests for the \"Add POI\" button (Step 19)." + nl +
    "    public class POIEditorAddPoiTests"
)
new = (
    "        }" + nl +
    "    }" + nl +
    "" + nl +
    "    // Tier-0 EditMode tests for the \"Add POI\" button (Step 19)." + nl +
    "    public class POIEditorAddPoiTests"
)
n = content.count(old)
if n != 1:
    raise SystemExit(f"FAIL T1: found {n}")
content = content.replace(old, new)

old2 = (
    "            Assert.AreNotEqual(id1, id2, \"Each POI should receive a unique GUID\");" + nl +
    "        }" + nl +
    "    }"
)
new2 = old2 + nl + "}"
n2 = content.count(old2)
if n2 != 1:
    raise SystemExit(f"FAIL T2: found {n2}")
content = content.replace(old2, new2)

with io.open(path, "w", encoding="utf-8", newline="") as f:
    f.write(content)
print("T1-T2-OK")