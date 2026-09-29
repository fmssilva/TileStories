using System.IO;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace TileStories.Editor
{
    // Unity has no importer for a ".vtt" captions file (its TextAsset list stops at .txt / .json / .csv...), so a caption file
    // sitting in a wall's Media Folder would be a DefaultAsset the card cannot load. This importer turns it into a plain TextAsset
    // with the file's text: Resources.Load<TextAsset>("<folder>/clip") then reads it in the Editor and in a build, and the POI
    // Editor's Asset field can hold it (_3.1 step 9A).
    [ScriptedImporter(1, "vtt")]
    public sealed class VttTextImporter : ScriptedImporter
    {
        // Read the file and make it the asset's one object
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var text = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("captions", text);
            ctx.SetMainObject(text);
        }
    }
}
