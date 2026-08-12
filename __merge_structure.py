# __merge_structure.py
# Merge post-2.3 updates from "10-structure copy.md" into canonical "10-structure.md".
# Adopted (accurate, additive): hunk1 TryResolvePriority, hunk5 Priority column,
# hunk6 ConfigValidation test, hunk7 test count 66->158, hunk8 HierarchyLevelSizeRangeTests,
# hunk9 LivingRoomConfigIntegrationTests.
# REJECTED (factually inaccurate vs on-disk code): hunk2 LODController simplification
#   (SetActive-7m/top-5-15 stub contradicts the real 7-step density pipeline),
#   hunk3 ClusterIndicator "(also not yet built)" (LODController IS built),
#   hunk4 drop of the DetailCard.uxml "confirm which version exists" caveat
#   (DetailCard.uxml does not exist, so the caveat stays).
# For hunk2 we keep the canonical accurate block and only fix a stale doc ref.

path = r"C:\Users/franc/Desktop/TileStories\.clinerules\10-structure.md"
with open(path, "rb") as f:
    raw = f.read()
has_bom = raw[:3] == b"\xef\xbb\xbf"
s = raw.decode("utf-8-sig")
start_len = len(s)

ARROW = "\u2190 "  # Unicode LEFTWARDS ARROW + space, as used in the tree

def rep(old, new, label, expected=1):
    global s
    n = s.count(old)
    assert n == expected, f"{label}: anchor count {n} (want {expected}): {old[:70]!r}"
    s = s.replace(old, new, 1)
    print(f"[ok] {label}")

def insert_before(anchor_substr, new_line, label):
    global s
    n = s.count(anchor_substr)
    assert n == 1, f"{label}: anchor count {n} (want 1): {anchor_substr[:70]!r}"
    i = s.index(anchor_substr)
    line_start = s.rfind("\n", 0, i) + 1
    s = s[:line_start] + new_line + "\n" + s[line_start:]
    print(f"[ok] {label}")

# --- Hunk 1: MarkerHierarchyResolver.cs -- add TryResolvePriority clause ---
rep("TryResolveByKey/Fallback, same pattern as StatusRamp.",
    "TryResolveByKey/Fallback, plus `TryResolvePriority` (1-based ordering key that LODController and Displacement consume), same pattern as StatusRamp.",
    "hunk1_TryResolvePriority")

# --- Hunk 2: LODController.cs -- keep canonical accurate block; fix stale doc refs ---
assert s.count("_2.4_LOD.md") == 2, f"hunk2: expected 2 _2.4_LOD.md refs, got {s.count('_2.4_LOD.md')}"
s = s.replace("_2.4_LOD.md", "_2.4_Marker_LOD.md")
print("[ok] hunk2_docref_fix (_2.4_LOD.md -> _2.4_Marker_LOD.md x2)")

# --- Hunk 5: GlobalScene.cs -- add Priority column ---
rep("RecomputeLevelPercentSpacing.",
    "RecomputeLevelPercentSpacing (+ DrawGlobalHierarchySection: Hierarchy table incl. Priority column).",
    "hunk5_PriorityColumn")

# --- Hunk 7: test count 66 -> 158 ---
rep("66 tests total as of", "158 tests total as of", "hunk7a_testcount")
rep("2026-08-07. No TestFixtures/ folder",
    "2026-08-11 (127 EditMode + 31 PlayMode). No TestFixtures/ folder",
    "hunk7b_testcount_date")

# --- Hunk 6: insert ConfigValidation.cs (sibling before ConfigFileIO, which is last) ---
cfg_io_an = "POIAuthoringToolWindow.ConfigFileIO.cs  " + ARROW + "SaveAllToJson, SaveConfig,"
i = s.index(cfg_io_an)
ls = s.rfind("\n", 0, i) + 1
ci_line = s[ls:s.index("\n", i)]
prefix = ci_line[:ci_line.find(ARROW)]
new_line6 = (prefix + "POIAuthoringToolWindow.ConfigValidation.cs  " + ARROW +
             "Validates hierarchy_level_key -> level-table match (ValidateHierarchyLevelKeys) "
             "+ size_cm soft range 0.5-100cm warning (ValidateHierarchyLevelSizeRange); "
             "collected before the zero-count early-return in ValidateAndAlert.")
insert_before(cfg_io_an, new_line6, "hunk6_ConfigValidation")

# --- Hunk 8: insert HierarchyLevelSizeRangeTests.cs (sibling before MarkerLayoutTests) ---
ml_an = "MarkerLayoutTests.cs            " + ARROW + "Tests pure-logic layout math (label offsets,"
i = s.index(ml_an)
ls = s.rfind("\n", 0, i) + 1
ml_line = s[ls:s.index("\n", i)]
prefix = ml_line[:ml_line.find("MarkerLayoutTests.cs")]
new_line8 = (prefix + "HierarchyLevelSizeRangeTests.cs    " + ARROW +
             "Tests size_cm soft-sanity range (0.5-100 cm) warning logic + the ValidateAndAlert "
             "zero-count early-return guard (InRange/boundaries, OutOfRange, Null, Empty, NullEntry).")
insert_before(ml_an, new_line8, "hunk8_HierarchyLevelSizeRangeTests")

# --- Hunk 9: insert LivingRoomConfigIntegrationTests.cs (sibling before MarkerViewRuntimeTests, last) ---
mv_an = "MarkerViewRuntimeTests.cs        " + ARROW + "Tests that MarkerView correctly wires its"
i = s.index(mv_an)
ls = s.rfind("\n", 0, i) + 1
mv_line = s[ls:s.index("\n", i)]
prefix = mv_line[:mv_line.find(ARROW)]
new_line9 = (prefix + "LivingRoomConfigIntegrationTests.cs  " + ARROW +
             "PlayMode integration: real StreamingAssets/LivingRoom/config.json (18 POIs, 5 "
             "hierarchy levels) loads and every hierarchy_level_key resolves.")
insert_before(mv_an, new_line9, "hunk9_LivingRoomConfigIntegrationTests")

assert len(s) != start_len
enc = "utf-8-sig" if has_bom else "utf-8"
with open(path, "w", encoding=enc, newline="") as f:
    f.write(s)
print("[ok] wrote", path, "delta_chars", len(s) - start_len)
