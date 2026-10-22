DEV-ONLY fonts for the _3.3 P0 capability spike (S2, the font bake-off). Nothing in Framework or any app references them.

Source: the official google/fonts GitHub repository, branch main, retrieved 2026-10-22:
  https://github.com/google/fonts/tree/main/ofl/<family>/  (fraunces, sourceserif4, playfairdisplay, atkinsonhyperlegiblenext,
  inter, plusjakartasans) -- the upright variable TTF of each family. Licence: SIL Open Font License 1.1, OFL.txt beside each.

Changes (OFL allows modified versions): each .ttf here is a STATIC instance cut from the variable font with fontTools 4.67
(varLib.instancer), named by its family and style:
  Fraunces-SemiBold            wght 600, opsz 28, SOFT 0, WONK 0
  SourceSerif4-SemiBold        wght 600, opsz 28
  PFSpike-SemiBold             Playfair Display, wght 600 -- RENAMED "PF Spike": "Playfair Display" is a Reserved Font Name
                               in its OFL.txt, and OFL section 3 forbids that name on a modified version
  AtkinsonHyperlegibleNext-Regular / -SemiBold   wght 400 / 600
  Inter-Regular / -SemiBold    wght 400 / 600, opsz 17
  PlusJakartaSans-Regular / -SemiBold            wght 400 / 600
Every cut holds the Portuguese letters a~ o~ c, a' e' i' o' u' a^ e^ o^ a` and their capitals (checked in each font's cmap).
The cutting script lives outside the repo (agent scratchpad, instance_fonts.py); re-cut from the variable fonts if needed.
