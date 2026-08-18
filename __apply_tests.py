import sys
path='Assets/Framework/Tests/Editor/PoiAuthoringVisualHierarchyTests.cs'
raw=open(path,'rb').read()
bom = raw[:3]==b'\xef\xbb\xbf'
body = raw[3:] if bom else raw
text = body.decode('utf-8')
le = '\r\n' if '\r\n' in text else '\n'
norm = text.replace('\r\n','\n').replace('\r','\n')
pairs=[
(r'''            ReflectColor("SearchFilterSectionColor");
            ReflectColor("PoiHeaderColor");
''',
 r'''            ReflectColor("SearchFilterSectionColor");
'''),
(r'''            Assert.IsNull(ReflectField("SearchKeywordsSectionColor"),
                "SearchKeywordsSectionColor drove a colored POI header that is now uncolored");
        }
''',
 r'''            Assert.IsNull(ReflectField("SearchKeywordsSectionColor"),
                "SearchKeywordsSectionColor drove a colored POI header that is now uncolored");
            Assert.IsNull(ReflectField("PoiHeaderColor"),
                "PoiHeaderColor single-color field replaced by PoiHeaderPalette and should be gone");
        }
'''),
(r'''        // Structural guard: POI header foldouts are bold + colored (via the
        // shared CreateFoldoutStyle helper + PoiHeaderColor), while the five
        // inner sub-section foldouts pass the uncolored FoldoutDefaultColor token.
        [Test]
        public void Source_SpecificMarker_PoiHeaderColored_InnerSectionsUncolored()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\SpecificMarker\POIAuthoringToolWindow.SpecificMarker.cs");
            Assert.IsTrue(src.Contains("PoiHeaderColor"),
                "POI header foldouts must reference PoiHeaderColor");
            Assert.IsTrue(src.Contains("CreateFoldoutStyle(PoiHeaderColor)"),
                "POI header foldout must be rendered bold+colored via CreateFoldoutStyle(PoiHeaderColor)");
''',
 r'''        // Structural guard: POI header foldouts are bold + colored via the shared
        // CreateFoldoutStyle helper + PoiHeaderColorFor (stable per-POI palette
        // color), while the five inner sub-section foldouts pass the uncolored
        // FoldoutDefaultColor token.
        [Test]
        public void Source_SpecificMarker_PoiHeaderViaPalette_InnerSectionsUncolored()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\SpecificMarker\POIAuthoringToolWindow.SpecificMarker.cs");
            Assert.IsTrue(src.Contains("PoiHeaderPalette"),
                "POI header foldouts must reference the PoiHeaderPalette array");
            Assert.IsTrue(src.Contains("PoiHeaderColorFor"),
                "POI header foldouts must be colored via PoiHeaderColorFor");
            Assert.IsTrue(src.Contains("CreateFoldoutStyle(PoiHeaderColorFor"),
                "POI header foldout must be rendered bold+colored via CreateFoldoutStyle(PoiHeaderColorFor(...))");
'''),
(r'''            Assert.IsFalse(src.Contains("SearchKeywordsSectionColor"),
                "SearchKeywordsSectionColor must be gone from SpecificMarker source");
        }

    }
}
''',
 r'''            Assert.IsFalse(src.Contains("SearchKeywordsSectionColor"),
                "SearchKeywordsSectionColor must be gone from SpecificMarker source");
        }

        // Tier-0: the per-POI header palette is a static Color[] of >=10 entries.
        [Test]
        public void Color_Constants_PoiHeaderPalette_ArrayPresent_WithAtLeastTenColors()
        {
            var fi = WindowType.GetField("PoiHeaderPalette",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(fi, "PoiHeaderPalette field should exist on POIAuthoringToolWindow");
            Color[] arr = fi.GetValue(null) as Color[];
            Assert.IsNotNull(arr, "PoiHeaderPalette should deserialize as a Color[]");
            Assert.That(arr.Length, Is.GreaterThanOrEqualTo(10),
                "PoiHeaderPalette should hold at least 10 colors");
        }

        // Tier-0: palette entries are visually distinct (no two near-collisions).
        [Test]
        public void Color_Constants_PoiHeaderPalette_SelfDistinct()
        {
            var fi = WindowType.GetField("PoiHeaderPalette",
                BindingFlags.NonPublic | BindingFlags.Static);
            Color[] arr = (Color[])fi.GetValue(null);
            Assert.That(arr.Length, Is.GreaterThanOrEqualTo(2));
            for (int i = 0; i < arr.Length; i++)
            for (int j = i + 1; j < arr.Length; j++)
            {
                Assert.That(ColorDistance(arr[i], arr[j]), Is.GreaterThan(ColorEpsilon),
                    "Palette colors must be visually distinct (no near-duplicates)");
            }
        }

        // Tier-0: PoiHeaderColorFor is deterministic per key, falls back to the
        // index seed for empty keys, and always resolves to a palette member.
        [Test]
        public void Method_PoiHeaderColorFor_ReturnsPaletteMember_Stable_AndFallsBackToIndex()
        {
            var fi = WindowType.GetField("PoiHeaderPalette",
                BindingFlags.NonPublic | BindingFlags.Static);
            var arr = (Color[])fi.GetValue(null);
            Assert.IsNotNull(arr);
            var m = WindowType.GetMethod("PoiHeaderColorFor",
                BindingFlags.NonPublic | BindingFlags.Static,
                null, new[] { typeof(string), typeof(int) }, null);
            Assert.IsNotNull(m, "PoiHeaderColorFor(string,int) should exist");

            Color a1 = (Color)m.Invoke(null, new object[] { "lamp_1", 0 });
            Color a2 = (Color)m.Invoke(null, new object[] { "lamp_1", 0 });
            Assert.AreEqual(a1, a2, "Same POI key must resolve to the same color across calls");
            Assert.That(Array.IndexOf(arr, a1), Is.GreaterThanOrEqualTo(0),
                "Keyed header color must be a palette member");

            Color b = (Color)m.Invoke(null, new object[] { "", 3 });
            Color c = (Color)m.Invoke(null, new object[] { "", 5 });
            Assert.That(Array.IndexOf(arr, b), Is.GreaterThanOrEqualTo(0),
                "Fallback header color (b) must be a palette member");
            Assert.That(Array.IndexOf(arr, c), Is.GreaterThanOrEqualTo(0),
                "Fallback header color (c) must be a palette member");
            Assert.AreNotEqual(b, c, "Distinct fallback seeds must resolve to distinct colors");

            bool[] seen = new bool[arr.Length];
            foreach (string id in new[] {
                "lamp_1", "lamp_2", "lamp_3", "lamp_4", "lamp_5",
                "lamp_6", "lamp_7", "lamp_8", "lamp_9", "lamp_10" })
            {
                Color col = (Color)m.Invoke(null, new object[] { id, 0 });
                int idx = Array.IndexOf(arr, col);
                Assert.That(idx, Is.GreaterThanOrEqualTo(0),
                    "ColorFor result must be a palette member");
                seen[idx] = true;
            }
            int distinct = 0;
            foreach (bool f in seen)
                if (f)
                    distinct++;
            Assert.That(distinct, Is.GreaterThanOrEqualTo(2),
                "Distinct POI keys should distribute across at least 2 palette colors");
        }

        // Tier-0: the tab button row renders above BeginScrollView (pinned header).
        [Test]
        public void Source_MainFile_TabButtonsAboveScrollView()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\POIAuthoringToolWindow.cs");
            int labelIdx = src.IndexOf("Specific Marker", StringComparison.Ordinal);
            Assert.IsTrue(labelIdx >= 0, "Expected the 'Specific Marker' tab button label in source");
            int scrollIdx = src.IndexOf("BeginScrollView", labelIdx, StringComparison.Ordinal);
            Assert.IsTrue(scrollIdx > labelIdx,
                "BeginScrollView must open AFTER the Specific Marker tab button (tabs pinned above scroll)");
            string between = src.Substring(labelIdx, scrollIdx - labelIdx);
            Assert.IsFalse(between.Contains("BeginScrollView"),
                "BeginScrollView must not appear between the tab label and itself (sanity check)");
            Assert.IsFalse(between.Contains("Space("),
                "No Space() spacer should appear between the tab button row and BeginScrollView");
        }

    }
}
''')
]
for k,(o,nw) in enumerate(pairs):
    c=norm.count(o)
    if c!=1:
        print('PAIR',k,'count',c,'-- ABORT. old head:',repr(o[:160]))
        sys.exit(1)
    norm=norm.replace(o,nw,1)
    print('applied pair',k,'OK')
out = (b'\xef\xbb\xbf' if bom else b'') + norm.replace('\n', le).encode('utf-8')
open(path,'wb').write(out)
print('written bytes',len(out))