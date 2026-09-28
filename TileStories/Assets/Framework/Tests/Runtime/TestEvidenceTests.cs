using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // The one evidence-path helper (_3.1 step 11-fix Part 0): a render of a domain lands in TestEvidence/<domain>/, next to Assets/
    public class TestEvidenceTests
    {
        [Test]
        public void PathFor_IsOutsideAssets_UnderTheProjectsTestEvidenceFolder_AndItsFolderExists()
        {
            string path = TestEvidence.PathFor("EvidenceProbe", "probe.png");
            try
            {
                string project = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                string assets = Path.GetFullPath(Application.dataPath);
                Assert.AreEqual(Path.Combine(project, "TestEvidence", "EvidenceProbe", "probe.png"), Path.GetFullPath(path));
                Assert.IsFalse(Path.GetFullPath(path).StartsWith(assets), "a render is never inside Assets/");
                Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(path)), "the domain folder is created");
            }
            finally
            {
                string dir = Path.GetDirectoryName(path);
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }
    }
}
