The app's two faces (_3.3 R12.3, P4.3): display = Fraunces SemiBold, body = Atkinson Hyperlegible Next Regular / Medium / Bold.
Each key of the framework font library (Runtime/UI/Markers/FontLibrary.asset) points at TWO assets built from the same .ttf here:
"<name> TMP.asset" (TextMeshPro, the markers) and "<name> UITK.asset" (TextCore, every UI Toolkit panel) -- one asset cannot serve
both systems (_3.3 R8 S2). Both are STATIC atlases, built by TileStories > Fonts > Rebuild Theme Font Atlases (FontAtlasBuilder):
never switch them to Dynamic (a dynamic asset writes every glyph it meets back into the file).

Source: the official google/fonts GitHub repository, branch main, retrieved 2026-10-23:
  https://github.com/google/fonts/tree/main/ofl/fraunces/                  Fraunces[SOFT,WONK,opsz,wght].ttf
  https://github.com/google/fonts/tree/main/ofl/atkinsonhyperlegiblenext/  AtkinsonHyperlegibleNext[wght].ttf
Licence: SIL Open Font License 1.1, OFL.txt beside each family (neither declares a Reserved Font Name).

Changes (OFL allows modified versions): each .ttf is a STATIC instance cut from the variable font with fontTools 4.67
(varLib.instancer), named by family + style (name IDs 1 / 2 / 4 / 6; the typographic names removed; the Bold cut flagged bold):
  Fraunces-SemiBold                        wght 600, opsz 28, SOFT 0, WONK 0   (the cut P0 S2 judged)
  AtkinsonHyperlegibleNext-Regular         wght 400
  AtkinsonHyperlegibleNext-Medium          wght 500
  AtkinsonHyperlegibleNext-Bold            wght 700
Coverage of Latin-1 + Latin Extended-A (checked in each cmap): Fraunces lacks U+0149 and U+017F (both deprecated); Atkinson
Hyperlegible Next lacks 37, among them the soft hyphen U+00AD, the micro sign U+00B5 and Esperanto / Latvian / Sami letters
(C, G, H, J, S with circumflex, I / O / U with macron or tilde, eng, kra). Every Portuguese letter is in both.
The cutting script lives outside the repo (agent scratchpad, instance_fonts.py); re-cut from the variable fonts if needed.
