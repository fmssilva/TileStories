# Context Summary - Default Heritage Category Styles Implementation

## Primary Request and Intent
The user wants to update the POI Authoring Tool so that when a new project is started 
and the wall's config.json has no `category_styles` defined, the editor tab's Markers 
component table shows 6 specific default heritage category rows with:
1. Category names (English, matching icon keys)
2. Specific icons (PNGs placed in Assets/Framework/Runtime/UI/Markers/Icons/)
3. Specific hex colors

The 6 categories with their icons and colors:
- "royal_government" - IconRoyal&Government.png - #D97706 (Amber/Imperial Gold)
- "religious" - IconReligious.png - #7C3AED (Deep Violet)
- "military" - IconMilitary.png - #DC2626 (Crimson Red)
- "residential" - IconNobel&PrivateResidence.png - #DB2777 (Rose Pink)
- "economic" - IconIndustry&Trade.png - #059669 (Teal Green)
- "infrastructure" - IconInfrastructures.png - #0284C7 (Sky Blue)

The user said: "about the category name, lets keep it in english" and "lets use same names as the keys basically almost" and "we can just delete the current categories"

## Key Technical Concepts
- Unity 6000.3.19f1 at C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe
- Unity MCP server NOT connected (no "unity" MCP server available)
- AssetPostprocessor for auto-configuring texture imports
- SpriteKeyLibrary (ScriptableObject) for key->Sprite lookup
- CategoryPalette static class with KnownIcons dictionary and hash-based color fallback
- WallConfigData.CategoryStyleEntry with category, color_hex, icon_key, details fields
- Partial class POIAuthoringToolWindow across multiple files

## Files and Code Sections

### Already Modified
- TileStories/Assets/Framework/Runtime/UI/Markers/Icons/*.png.meta (6 files)
  - Changed spriteMode:0->1, textureType:0->8, nPOTScale:1->0, alphaIsTransparency:0->1
  - Makes the 6 new PNGs import as Sprites
- TileStories/Assets/Framework/Editor/POIAuthoring/MarkerSymbolTexturePostprocessor.cs
  - Updated comment to mention /Icons/ folder handling
  - OnPreprocessTexture NOT YET updated to handle /Icons/ paths (only handles /MarkerAssets/)

### Key Files to Read/Modify
- ConfigFileIO.cs: LoadConfig() seeds defaults when category_styles.Count == 0 (lines 55-60)
- GlobalScene.cs: DrawMarkerGlobalSection() also seeds defaults (lines 63-69)
- CategoryPalette.cs: KnownIcons dict (lines 18-28), ResolveIconKey returns "unknown" fallback
- SpriteKeyLibrary.cs: EnsureKeyForSprite for registering sprites, CopyFrom for copying libraries
- Test file: CategoryPaletteTests.cs (EditMode tests)
- LivingRoom config.json (does NOT have category_styles defined - relies on runtime seeding)

### Current Seed Logic (to be replaced in BOTH files)
```csharp
if (_config.category_styles.Count == 0)
{
    _config.category_styles.Add(new CategoryStyleEntry { category = "religious", icon_key = "temple", color_hex = "" });
    _config.category_styles.Add(new CategoryStyleEntry { category = "military", icon_key = "shield", color_hex = "" });
    _config.category_styles.Add(new CategoryStyleEntry { category = "civic", icon_key = "columns", color_hex = "" });
}
```

### CategoryPalette.KnownIcons (current)
```csharp
{ "religious", "temple" },
{ "royal", "crown" },
{ "military", "shield" },
{ "civic", "columns" },
{ "maritime", "anchor" },
{ "infra", "bridge" },
{ "landscape", "leaf" },
{ "commerce", "scale" },
```

## Remaining TODO
- [x] 1. Fix .meta files for 6 new PNG icons - DONE
- [ ] 2. Extend MarkerSymbolTexturePostprocessor OnPreprocessTexture to handle /Icons/ paths
- [ ] 3. Register the 6 new icons in IconLibrary.asset (via editor script at startup)
- [ ] 4. Update seeding logic in GlobalScene.cs and ConfigFileIO.cs
- [ ] 5. Update CategoryPalette.KnownIcons with new heritage categories
- [ ] 6. Update LivingRoom config.json (clear old categories, use new defaults)
- [ ] 7. Write EditMode tests for default categories
- [ ] 8. Compile and run tests (Unity batch mode)
- [ ] 9. Verify no error CS lines and test counts

## Unity Command for Compile/Test
Unity path: C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe
Project path: C:\Users\franc\Desktop\TileStories\TileStories
```
