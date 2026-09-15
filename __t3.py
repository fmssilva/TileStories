# -*- coding: utf-8 -*-
# T3: insert PoiRenameKeysTests between the two test classes. Deleted after use.
import io

path = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests\Editor\POIEditorAddPoiTests.cs"
with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
    content = f.read()

nl = "\r\n"
anchor = "    // Tier-0 EditMode tests for the \"Add POI\" button (Step 19)."
block = (
    "    // Tier-0 tests for the rename key decision table (Enter/Esc double-press bug)." + nl +
    "    public class PoiRenameKeysTests" + nl +
    "    {" + nl +
    "        [Test]" + nl +
    "        public void Resolve_KeyDown_Return_Commits()" + nl +
    "        {" + nl +
    "            Assert.AreEqual(PoiRenameKeys.Action.Commit," + nl +
    "                PoiRenameKeys.Resolve(EventType.KeyDown, KeyCode.Return));" + nl +
    "        }" + nl +
    "" + nl +
    "        [Test]" + nl +
    "        public void Resolve_KeyDown_KeypadEnter_Commits()" + nl +
    "        {" + nl +
    "            Assert.AreEqual(PoiRenameKeys.Action.Commit," + nl +
    "                PoiRenameKeys.Resolve(EventType.KeyDown, KeyCode.KeypadEnter));" + nl +
    "        }" + nl +
    "" + nl +
    "        [Test]" + nl +
    "        public void Resolve_KeyDown_Escape_Cancels()" + nl +
    "        {" + nl +
    "            Assert.AreEqual(PoiRenameKeys.Action.Cancel," + nl +
    "                PoiRenameKeys.Resolve(EventType.KeyDown, KeyCode.Escape));" + nl +
    "        }" + nl +
    "" + nl +
    "        // Regression: the TextField consumes the first Return (event becomes" + nl +
    "        // Used); the resolver must never re-fire on consumed events." + nl +
    "        [Test]" + nl +
    "        public void Resolve_Used_Return_IsNone()" + nl +
    "        {" + nl +
    "            Assert.AreEqual(PoiRenameKeys.Action.None," + nl +
    "                PoiRenameKeys.Resolve(EventType.Used, KeyCode.Return));" + nl +
    "        }" + nl +
    "" + nl +
    "        // KeyUp must not re-fire the action after KeyDown handled it." + nl +
    "        [Test]" + nl +
    "        public void Resolve_KeyUp_Return_IsNone()" + nl +
    "        {" + nl +
    "            Assert.AreEqual(PoiRenameKeys.Action.None," + nl +
    "                PoiRenameKeys.Resolve(EventType.KeyUp, KeyCode.Return));" + nl +
    "        }" + nl +
    "" + nl +
    "        // Layout/Repaint passes must never trigger actions." + nl +
    "        [Test]" + nl +
    "        public void Resolve_LayoutAndRepaint_AreNone()" + nl +
    "        {" + nl +
    "            Assert.AreEqual(PoiRenameKeys.Action.None," + nl +
    "                PoiRenameKeys.Resolve(EventType.Layout, KeyCode.Return));" + nl +
    "            Assert.AreEqual(PoiRenameKeys.Action.None," + nl +
    "                PoiRenameKeys.Resolve(EventType.Repaint, KeyCode.Escape));" + nl +
    "        }" + nl +
    "" + nl +
    "        // Ordinary typing keys must not commit or cancel." + nl +
    "        [Test]" + nl +
    "        public void Resolve_KeyDown_Letter_IsNone()" + nl +
    "        {" + nl +
    "            Assert.AreEqual(PoiRenameKeys.Action.None," + nl +
    "                PoiRenameKeys.Resolve(EventType.KeyDown, KeyCode.A));" + nl +
    "        }" + nl +
    "    }" + nl +
    "" + nl
)
n = content.count(anchor)
if n != 1:
    raise SystemExit(f"FAIL T3: found {n}")
content = content.replace(anchor, block + anchor)
with io.open(path, "w", encoding="utf-8", newline="") as f:
    f.write(content)
print("T3-OK")