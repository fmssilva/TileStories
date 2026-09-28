using System.IO;
using UnityEngine;

namespace TileStories.Tests
{
    // Where every test render (a PNG a person or the agent looks at) is written: TileStories/TestEvidence/<domain>/, NEXT TO
    // Assets/ and never inside it, so Unity does not import hundreds of generated pictures and git ignores them (_3.1 step 11-fix
    // Part 0). Every test that saves a picture asks here; TestEvidenceTests fails the build if a private copy of the old path appears.
    // Public: the app's own test assemblies use it too.
    public static class TestEvidence
    {
        // The folder that holds every domain's evidence: <project>/TestEvidence
        public static string Root => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "TestEvidence"));

        // The full path of a render of `domain` (its folder is created): TestEvidence/<domain>/<fileName>
        public static string PathFor(string domain, string fileName)
        {
            string dir = Path.Combine(Root, domain);
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, fileName);
        }
    }
}
