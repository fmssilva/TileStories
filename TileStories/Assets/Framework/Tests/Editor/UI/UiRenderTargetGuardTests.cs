using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TileStories.Tests
{
    // The render-target rule of _3.3 R15.2, held by a scan of the source. Runtime/UI has one folder per target: World/ (uGUI), Screen/
    // (UI Toolkit, the screen panel), Blocks/ and Components/ (UI Toolkit, ANY panel -- screen or world). What draws on any panel must not
    // depend on the card's SHEET, so drawing a block on a world panel later is a hosting job, not a rewrite:
    //   - Blocks/ and Components/ never name a sheet / host type, the screen PanelSettings or Screen.width / height
    //   - no UnityEngine.UI / TMPro in a UI Toolkit folder (Screen/, Blocks/, Components/)
    //   - no UnityEngine.UIElements in World/
    // The views that break the rule TODAY are listed in ONE allow-list below, a reason per line: S2 removes them, and an entry the scan
    // no longer needs fails the test, so the list can only shrink. Comments and string literals are not code and are not scanned.
    public class UiRenderTargetGuardTests
    {
        private const string UiRoot = "Assets/Framework/Runtime/UI";

        private const string SheetType = "sheet-type";
        private const string ScreenSize = "screen-size";
        private const string ScreenPanel = "screen-panel-settings";
        private const string UguiInUitk = "ugui-in-uitk";
        private const string UitkInWorld = "uitk-in-world";

        private static readonly Regex Sheet = new(@"\b(PoiCardSheetView|PoiCardHost|NavController|TakeoverView|BlockStackView|CardChromeView)\b");
        private static readonly Regex Size = new(@"\bScreen\s*\.\s*(width|height)\b");
        private static readonly Regex Panel = new(@"\bPanelSettings\b");
        private static readonly Regex Ugui = new(@"\bUnityEngine\.UI(?![A-Za-z0-9_])|\bTMPro\b");
        private static readonly Regex Uitk = new(@"\bUnityEngine\.UIElements\b");

        // Comments, then string / char literals, blanked in ONE left-to-right pass (a "//" inside a string is not a comment)
        private static readonly Regex NotCode = new(
            @"//[^\n]*|/\*.*?\*/|@""(?:[^""]|"""")*""|""(?:[^""\\\n]|\\.)*""|'(?:[^'\\\n]|\\.)'", RegexOptions.Singleline);

        // - "folder/relative/File.cs | rule": the views S2 untangles, each with why it names the sheet today
        private static readonly Dictionary<string, string> AllowList = new()
        {
            ["Blocks/Media/GalleryBlockView.cs | " + SheetType] =
                "the lightbox page draws into the card's TakeoverView (DrawLightboxPage takes it as a parameter): S2 hands it a plain container",
            ["Blocks/Navigation/NavBentoBlockView.cs | " + SheetType] =
                "holds the sheet's NavController (_nav) for the model and the jump: S2 reads both through BlockBindContext",
            ["Blocks/Navigation/NavContentsBlockView.cs | " + SheetType] =
                "holds the sheet's NavController (_nav) for the model and the jump: S2 reads both through BlockBindContext",
            ["Blocks/Navigation/NavIconGridBlockView.cs | " + SheetType] =
                "holds the sheet's NavController (_nav) for the model and the jump: S2 reads both through BlockBindContext",
            ["Blocks/Navigation/NavRailsBlockView.cs | " + SheetType] =
                "holds the sheet's NavController (_nav) for the model and the jump: S2 reads both through BlockBindContext",
        };

        // The violations of every .cs under a ui root (a folder with Blocks / Components / Screen / World inside), as "relative path | rule"
        private static List<string> Scan(string uiRoot)
        {
            var found = new List<string>();
            void Check(string folder, bool anyPanel, bool uitk, bool world)
            {
                string dir = Path.Combine(uiRoot, folder);
                if (!Directory.Exists(dir)) return;
                foreach (string file in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
                {
                    string code = NotCode.Replace(File.ReadAllText(file), " ");
                    string rel = file.Substring(uiRoot.Length).Replace('\\', '/').TrimStart('/');
                    if (anyPanel && Sheet.IsMatch(code)) found.Add(rel + " | " + SheetType);
                    if (anyPanel && Size.IsMatch(code)) found.Add(rel + " | " + ScreenSize);
                    if (anyPanel && Panel.IsMatch(code)) found.Add(rel + " | " + ScreenPanel);
                    if (uitk && Ugui.IsMatch(code)) found.Add(rel + " | " + UguiInUitk);
                    if (world && Uitk.IsMatch(code)) found.Add(rel + " | " + UitkInWorld);
                }
            }
            Check("Blocks", anyPanel: true, uitk: true, world: false);
            Check("Components", anyPanel: true, uitk: true, world: false);
            Check("Screen", anyPanel: false, uitk: true, world: false);
            Check("World", anyPanel: false, uitk: false, world: true);
            return found.OrderBy(s => s, System.StringComparer.Ordinal).ToList();
        }

        // The scan reads the real folders: not vacuous (every folder has the code the rule is about)
        [Test]
        public void TheScan_ReadsTheRealFolders()
        {
            foreach (var (folder, atLeast, example) in new[]
            {
                ("Blocks", 30, "About/HeaderBlockView.cs"), ("Screen", 60, "Cards/PoiCardSheetView.cs"), ("World", 20, "Markers/MarkerView.cs"),
            })
            {
                var files = Directory.GetFiles(UiRoot + "/" + folder, "*.cs", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')).ToList();
                Assert.GreaterOrEqual(files.Count, atLeast, folder + "/ holds the code the rule is about");
                CollectionAssert.Contains(files, UiRoot + "/" + folder + "/" + example, "the scan reaches " + example);
            }
            Assert.IsTrue(Directory.Exists(UiRoot + "/Components"), "Components/ exists, empty of code until S2");
        }

        // Every violation the scan finds is in the allow-list with a reason, and every allow-list line is still a violation
        [Test]
        public void EveryViolation_IsAllowListedWithAReason_AndNoAllowListLineIsStale()
        {
            var found = Scan(UiRoot);
            var unlisted = found.Where(f => !AllowList.ContainsKey(f)).ToList();
            Assert.IsEmpty(unlisted, "these break the render-target rule (_3.3 R15.2) -- fix them, or list them with a reason:\n  " + string.Join("\n  ", unlisted));
            var stale = AllowList.Keys.Where(k => !found.Contains(k)).ToList();
            Assert.IsEmpty(stale, "these allow-list lines no longer break the rule -- delete them:\n  " + string.Join("\n  ", stale));
            Assert.IsTrue(AllowList.Values.All(reason => !string.IsNullOrWhiteSpace(reason)), "every allow-list line says why");
        }

        // The scan catches what it is for, on planted files in a throwaway ui root: each forbidden reference is red and names its file,
        // while the same words in a comment or a string are not code
        [Test]
        public void ThePlantedViolations_AreFound_ByFileAndRule_AndCommentsAreNot()
        {
            string root = Path.Combine(Path.GetTempPath(), "ui_guard_" + System.Guid.NewGuid().ToString("N"));
            try
            {
                void Plant(string rel, string source)
                {
                    string path = Path.Combine(root, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, source);
                }
                Plant("Blocks/X/PlantedSheet.cs", "class A { private NavController _nav; }");
                Plant("Components/PlantedHost.cs", "class B { void M() { var h = new PoiCardHost(); } }");
                Plant("Blocks/X/PlantedSize.cs", "class C { float W => Screen.width; }");
                Plant("Components/PlantedPanel.cs", "class D { PanelSettings p; }");
                Plant("Screen/Y/PlantedUgui.cs", "using UnityEngine.UI;\nclass E { }");
                Plant("Blocks/X/PlantedTmp.cs", "class F { TMPro.TMP_Text t; }");
                Plant("World/Z/PlantedUitk.cs", "using UnityEngine.UIElements;\nclass G { }");
                Plant("Blocks/X/Clean.cs", "using UnityEngine.UIElements;\n// NavController PoiCardHost Screen.width UnityEngine.UI\nclass H { string s = \"TakeoverView // PanelSettings\"; /* BlockStackView */ }");
                Plant("Screen/Y/CleanUitk.cs", "using UnityEngine.UIElements;\nclass I { }");
                Plant("World/Z/CleanUgui.cs", "using UnityEngine.UI;\nclass J { }");

                CollectionAssert.AreEqual(new[]
                {
                    "Blocks/X/PlantedSheet.cs | " + SheetType,
                    "Blocks/X/PlantedSize.cs | " + ScreenSize,
                    "Blocks/X/PlantedTmp.cs | " + UguiInUitk,
                    "Components/PlantedHost.cs | " + SheetType,
                    "Components/PlantedPanel.cs | " + ScreenPanel,
                    "Screen/Y/PlantedUgui.cs | " + UguiInUitk,
                    "World/Z/PlantedUitk.cs | " + UitkInWorld,
                }, Scan(root));
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
