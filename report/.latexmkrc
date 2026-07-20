# Use LuaLaTeX by default so `latexmk template.tex` builds PDFs directly.
# This avoids DVI-mode image errors like "no BoundingBox" for PDF graphics.
$pdf_mode = 4;
$lualatex = 'lualatex --shell-escape --file-line-error %O %S';
# Keep biber as bibliography backend for biblatex.
$bibtex = 'biber %O %B';
