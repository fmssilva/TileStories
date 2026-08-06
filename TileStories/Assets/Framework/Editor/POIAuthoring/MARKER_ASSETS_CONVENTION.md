# Marker Assets Folder Convention

This document records the recommended folder convention for marker symbol assets.
It is forward-looking only -- it does not prescribe any retroactive migration
of existing walls' assets. It exists so that every wall added from here on has
a single, consistent answer for "where do this wall's symbols live."

See `_5.1_Editor_Tab.md` section 14.8 for the full rationale.

## Where things live

### Framework defaults
```
Assets/Framework/Runtime/UI/Markers/{Icons,Rings,Shapes}/
```
These PNGs ship with every wall and rarely change. They are referenced by the
`POI_Marker` prefab as direct serialized references (not via `Resources.Load`),
so they must NOT be moved into any folder literally named `Editor` -- Unity
strips everything inside `Editor/` folders from player builds entirely.

### Per-wall custom symbols (PNG sources)
```
Assets/Apps/<Wall>/MarkerAssets/Symbols/
```
New PNG icons a developer adds for that specific wall. Textures dropped here
are auto-configured as Sprite (2D and UI) with alpha and no mipmaps by
`MarkerSymbolTexturePostprocessor` (section 14.9 of `_5.1_Editor_Tab.md`).

### Per-wall SpriteKeyLibrary asset
```
Assets/Apps/<Wall>/MarkerAssets/Resources/MarkerSymbols/
```
The `SpriteKeyLibrary` `.asset` file itself. This MUST live under a `Resources/`
folder (any depth) so `WallSession` can load it at runtime via
`Resources.Load` using the string from `config.json`'s
`marker_icon_library_resources_path` field. The PNGs it references do not need
to be in a `Resources/` folder -- only the library asset does.

### Freshly-imported third-party packs (unreviewed)
```
Assets/Apps/<Wall>/MarkerAssets/RawImports/
```
A staging subfolder. After review, assets are moved into `Symbols/` or
discarded. This keeps "stuff I haven't sorted yet" visibly separate from
"stuff actually in use" without requiring any code to enforce the distinction.

## Key principles

1. Nothing in this pipeline physically relocates PNGs at save time. Unity's
   GUID-based asset reference system keeps references correct regardless of
   physical file location. The PNG can live anywhere; what makes it "the wall's
   icon for `temple`" is the entry in that wall's `SpriteKeyLibrary` asset,
   not its folder.

2. A wall can add custom symbols without modifying shared framework assets. The
   wall's `SpriteKeyLibrary` is loaded at runtime and passed into `MarkerView`
   as an override; if missing or invalid, the prefab default library is used.

3. All three symbol purposes (icons, shapes, line styles) use the same
   `SpriteKeyLibrary` class. A single per-wall library asset can serve all
   three, under distinctly-named keys where needed.
