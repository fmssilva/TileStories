function globalTab() {
  const out = [];
  const lock = (t, c, sub) => fold("x_" + t, t, { color: c, locked: true, sub: sub || "existing · not in this prototype" }, () => []);
  out.push(grpHead(1, "Wall & content", "what the wall is and what its POIs are"));
  out.push(fold("wall", "Wall", { color: "#9FB8DC", badge: "new", screen: "precision" }, () => [
    ro("Config", "Assets/Apps/LivingRoom/config.json"), ro("Wall mesh (reference)", "146267-LivingRoom2-tex (GLB)"),
    ...row("Alignment error", fRange(D.test, "error_cm", 0, 8, 0.5, " cm"), "test"),
    h("p", { class: "guide" }, "Measured on the device (step 0); every style's “hides ≈X cm” compares with it, and Play Mode simulates it.")]));
  out.push(planned("georef", "Geo-reference", "planned", "Control points (a wall point ↔ a lat / lon) that map the wall to today's world. Needed by Today on the map, the map overlay, GPS and today's landmarks beyond the panel.", ["control points (≥ 4)", "fit quality", "derived lat / lon per POI"], "Task: later (needs internet and a map licence). The place is kept here."));
  out.push(fold("cats", "Categories", { color: "#E8C27A", screen: "wall", sub: "moved from Marker › Category Symbols", extra: tagChip("moved") }, () => {
    const tb = h("tbody"); Object.entries(CATS).forEach(([k, c]) => tb.append(h("tr", {}, h("td", {}, h("input", { class: "uin", value: c.label, onchange: e => { const v = e.target.value.trim(); if (v) { c.label = v; changed(); } } })), h("td", {}, fColor(c, "color")("cat_" + k)), h("td", { class: "num" }, "icon"))));
    return [h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th", {}, "Category label"), h("th", {}, "Colour"), h("th", {}, "Icon"))), tb), h("p", { class: "guide" }, "Read by the point, the region colour, filters, the card chip and the guide.")];
  }));
  out.push(lock("Text & Fonts", "#E46D8A", "existing (Labels, Text & Fonts)"));

  out.push(grpHead(2, "Looks", "named styles, defined once; levels assign them, a POI may pick another"));
  out.push(fold("pointstyles", "Point styles", { color: "#3FC1C9", badge: "new", screen: "wall", onToggle: o => { if (o) pointDemo(); }, sub: `${D.point_styles.length} styles · default ${pointStyleByKey(D.point_default)?.name}` }, pointStylesBody));
  out.push(fold("styles", "Region styles", { color: "#3FC1C9", badge: "new", extra: helpBtn("styles"), screen: "style" }, () => [
    ...row("Default for the whole app", fSelect(D, "region_default", styleOpts(null, D.region_default)), "Every shape uses this style unless its level (Levels › Region style), its POI or the shape itself picks another."),
    ...regionStylesBody()]));
  if (ui.help === "styles") out.push(helpText("styles"));
  out.push(planned("tagstyles", "Tag styles", "merged", "From the simplest label (one text block: the name) to a small card (image, title, one line). Every Tag is made of Detail Card blocks, anchored to its POI on the wall; a card Tag opens the real card when tapped.", ["blocks: title · image · summary", "box: none / pill / card", "under / beside", "show when space allows"], "Task: phase B. Today's label is the “Label” Tag style (title only).", { sub: "label + summary card", screen: "app" }));
  out.push(planned("modelstyles", "Model styles", "new", "How a 3D model stands on the wall: scale, base, shadow, the wall occluder, how it rises out on tap.", ["scale", "base & shadow", "wall occluder", "rise-out motion"], "Task: phase C.", {screen: "app", step: "tapped", demo: { step: "tapped", poi: 0, preview: null, view: "model_wall" }}));

  out.push(grpHead(3, "Levels", "the hub: each level assigns layers, a region style and an on-tap style (or keeps the app defaults)"));
  out.push(fold("levels", "Hierarchy Levels", { color: "#E8D44D", badge: "ext", extra: helpBtn("levels"), screen: "wall", defaultOpen: true }, levelsBody));
  if (ui.help === "levels") out.push(helpText("levels"));

  out.push(grpHead(4, "Life", "wall-level content that moves; per-POI rows live in each POI"));
  out.push(fold("life_g", "Wall Life", { color: "#F08C5A", badge: "new", extra: helpBtn("life"), screen: "life" }, lifeGlobalBody));
  if (ui.help === "life") out.push(helpText("life"));
  out.push(planned("sound", "Ambient sound", "planned", "Wind, birds, water, the city: a sound row kind for wall-level and per-POI rows, with a mute.", ["clip", "volume", "trigger"], "Task: with Wall Life (an audio kind)."));

  out.push(grpHead(5, "Behaviour on the wall", "systems that act on all POIs at once"));
  for (const [t, c] of [["Orientation", "#8E92F0"], ["LOD", "#3FB2A2"], ["Zoom", "#D45FD0"], ["Displacement", "#C97FB0"], ["Select, Filter & Search", "#B88A5A"]]) out.push(lock(t, c, "existing"));
  out.push(planned("wayfind", "Wayfinding helpers", "new", "Arrows at the screen edge to off-screen POIs, numbered POIs, seen / unseen marks, favourites, suggested next.", ["edge arrows", "numbers", "seen / unseen", "favourites"], "Edge arrows are built: App › Main AR screen › Pointing. The rest: phase D, with tours.", {screen: "app", step: "main", demo: { step: "main", preview: null }}));

  out.push(grpHead(6, "Test & validate", ""));
  out.push(fold("rs_test", "Test", { screen: "precision" }, () => [
    ...row("Reduce motion", fToggle(D.test, "reduce_motion")),
    h("ul", { class: "guide" }, h("li", {}, "Style screen: the selected style alone, looping intro → idle → focus."), h("li", {}, "Precision screen: the style at 0, 2, 4 and 6 cm of error, the true contour dashed."), h("li", {}, "Each section keeps its own Test in Unity; the save report lists every warning."))]));
  out.push(fold("json_g", "config.json (live preview)", { color: "#9BB0C8", sub: "styles · levels · wall life" }, () => [h("pre", { class: "json" }, jsonGlobal())]));
  return groupify(out);
}
function precisionChip(s) { const v = verdict(s); return h("span", { class: "prec " + v.cls, "data-chip": s.key, title: `hides about ${v.t.toFixed(1)} cm` }, chipText(v)); }
function refreshChips() { edb.querySelectorAll("[data-chip]").forEach(el => { const s = styleByKey(el.dataset.chip); if (!s) return; const v = verdict(s); el.className = "prec " + v.cls; el.textContent = chipText(v); }); edb.querySelectorAll("[data-pline]").forEach(el => { const s = styleByKey(el.dataset.pline); if (s) el.replaceChildren(precisionLine(s)); }); }
function precisionLine(st) {
  const v = verdict(st);
  return h("span", { class: "prec " + v.cls }, `hides ≈${v.t.toFixed(1)} cm · measured ${D.test.error_cm} cm: ${v.word}`);
}
function regionStylesBody() {
  const tb = h("tbody");
  D.styles.forEach((s, i) => {
    const used = usesOfStyle(s.key);
    const tr = h("tr", { class: i === ui.style ? "sel" : "", onclick: e => { if (e.target.closest("button,input")) return; ui.style = i; ui.open["style_body"] = true; styleT0 = performance.now() / 1000; renderEditor(); if (ui.screen === "style") styleCam(); } },
      h("td", {}, h("input", { class: "uin", value: s.label, "aria-label": "Style name", onfocus: () => { if (ui.style !== i) { ui.style = i; styleT0 = performance.now() / 1000; renderEditor(); if (ui.screen === "style") styleCam(); } }, onchange: e => { const v2 = e.target.value.trim(); if (!v2) { e.target.value = s.label; return; } snap(); s.label = v2; changed(); } })),
      h("td", {}, h("canvas", { class: "sw", width: 120, height: 34, "data-style": s.key, style: "width:60px;height:17px" })),
      h("td", {}, precisionChip(s)),
      h("td", { class: "num" }, String(used)),
      h("td", {}, h("button", { class: "ib", title: "Duplicate", onclick: () => { snap(); const c = clone(s); let n = 1; while (styleByKey("region_style_" + n)) n++; c.key = "region_style_" + n; c.label = s.label + " copy"; D.styles.splice(i + 1, 0, c); ui.style = i + 1; changed(); } }, "⧉")),
      h("td", {}, h("button", { class: "ib", title: used ? `Used by ${used}: pick another style there first` : "Delete", disabled: used > 0, onclick: () => { snap(); D.styles.splice(i, 1); ui.style = Math.max(0, i - 1); changed(); } }, "🗑")));
    tb.append(tr);
  });
  const st = D.styles[ui.style];
  return [
    h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th", {}, "Name"), h("th", {}, "Preview"), h("th", {}, "Precision"), h("th", {}, "Used"), h("th"), h("th"))), tb),
    h("div", { class: "addrow" }, h("button", { class: "ubtn", onclick: () => { snap(); let n = 1; while (styleByKey("region_style_" + n)) n++; D.styles.push(mkStyle("region_style_" + n, "Style " + n)); ui.style = D.styles.length - 1; ui.open["style_body"] = true; changed(); } }, "+ Add style")),
    st ? fold("style_body", "Style · " + st.label, { color: "#8FE0E5", defaultOpen: true }, () => styleBody(st)) : null,
  ];
}
function styleBody(st) {
  const L = st.line, V = st.vertices, F = st.fill, M = st.motion, v = verdict(st);
  const fillOpts = [["none", "None"], ["tint", "Tint"], ["gradient", "Gradient"], ["spotlight", "Spotlight (dim the wall around it)"], ["pattern", "Pattern"], ["media", "Media (image or video)"]];
  const fmode = F.mode;
  const pwrap = h("div", { "data-screen": "precision" }, h("div", { class: "urow" }, h("label", {}, "Precision"), h("span", { "data-pline": st.key }, precisionLine(st)), helpBtn("precision")), helpText("precision"));
  if (D.test.error_cm > v.t) pwrap.append(h("div", { class: "notice" }, `The measured error (${D.test.error_cm} cm) is larger than this style hides (≈${v.t.toFixed(1)} cm): the line will cut across the facade. Raise the offset, the glow or the feather.`));
  return [
    pwrap,
    fold("st_line", "Line", { defaultOpen: true }, () => [
      ...row("Width", fRange(L, "width_cm", 0.2, 5, 0.1, " cm"), "width"),
      ...row("Min / max", id => h("div", { class: "slide", style: "grid-template-columns:1fr 1fr" }, fRange(L, "min_px", 0.5, 4, 0.5, " px")(id), fRange(L, "max_px", 2, 12, 0.5, " px")(id + "b")), "minmax"),
      ...row("Corner smoothing", fRange(L, "smooth", 0, 1, 0.05), "Rounds each real corner of the outline (radius = this × 6 cm, at most 45 % of the shorter edge): a hand-drawn polygon reads as a clean shape. 0 = the knots as drawn."),
      ...row("Colour", fSelect(L, "color_mode", [["accent", "Wall ink (gold: reads on azulejo blue)"], ["category", "Category colour"], ["fixed", "Fixed"]]), "colormode"),
      ...(L.color_mode === "fixed" ? row("Fixed colour", fColor(L, "color"), null, true) : []),
      ...row("Opacity", fRange(L, "opacity", 0, 1, 0.05)),
      ...(L.opacity === 0 && M.focus.preset !== "appear" ? [h("p", { class: "guide" }, "The line is off (opacity 0): this style is fill only. Raise the opacity to style the line.")] : []),
      ...(L.opacity === 0 && M.focus.preset !== "appear" ? [] : [
      ...row("Pattern", fSelect(L, "pattern", [["solid", "Solid"], ["dashed", "Dashed"], ["dotted", "Dotted"], ["brackets", "Focus brackets (the corners of its box)"]]), "pattern"),
      ...(L.pattern === "dashed" ? [...row("Dash", fRange(L, "dash_cm", 0.5, 10, 0.5, " cm"), null, true), ...row("Gap", fRange(L, "gap_cm", 0.5, 10, 0.5, " cm"), null, true)] : []),
      h("div", L.pattern === "brackets" ? {} : { "data-screen": "precision" }, ...row(L.pattern === "brackets" ? "Bracket margin" : "Offset outward", fRange(L, "offset_cm", 0, 8, 0.5, " cm"), "offset")),
      ...row("Glow", fToggle(L, "glow"), "glow"),
      ...(L.glow ? [...row("Glow width", fRange(L, "glow_width_cm", 0.5, 8, 0.5, " cm"), null, true), ...row("Glow intensity", fRange(L, "glow_intensity", 0, 1, 0.05), null, true)] : []),
      ...row("Double line", fToggle(L, "double"), "double"),
      ...(L.double ? row("Double gap", fRange(L, "double_gap_cm", 0.5, 6, 0.5, " cm"), null, true) : []),
      ]),
    ]),
    fold("st_vert", "Vertices", {}, () => [
      ...row("Show at", fSelect(V, "show_at", [["none", "None"], ["corners", "Corners only"], ["all", "All knots"]]), "showat"),
      ...(V.show_at === "none" ? [h("p", { class: "guide" }, "Vertices are off: pick Corners or All knots to style the marks.")] : [
      ...row("Mark", fSelect(V, "mark", [["dot", "Dot"], ["square", "Square"], ["diamond", "Diamond"], ["ring", "Ring"], ["cross", "Cross"], ["bracket", "Corner bracket"]]), "mark"),
      ...row("Size", fRange(V, "size_cm", 1, 12, 0.5, " cm")),
      ...row("Colour", fSelect(V, "color_mode", [["line", "Same as line"], ["fixed", "Fixed"]])),
      ...(V.color_mode === "fixed" ? row("Fixed colour", fColor(V, "color"), null, true) : []),
      ]),
    ]),
    fold("st_fill", "Fill", {}, () => [
      ...row("Fill", fSelect(F, "mode", fillOpts), "fill"),
      ...(fmode === "media" ? [
        ...row("Fit", fSelect(F, "fit", [["cover", "Cover (fill the shape, crop)"], ["contain", "Contain (whole picture)"], ["stretch", "Stretch"]]), null, true),
        ...row("Opacity", fRange(F, "opacity", 0, 1, 0.01), null, true),
        ...row("Video plays", fSelect(F, "media_play", [["loop", "Loop while visible"], ["on_tap", "On tap (still frame at rest)"]]), null, true),
        ...row("Edge feather", fRange(F, "feather_cm", 0, 8, 0.5, " cm"), "feather", true),
        h("p", { class: "guide" }, "The file is picked per shape (Specific Marker › Region › Shape), so one style serves every window with its own picture or video.")]
      : fmode !== "none" ? [
        ...(fmode === "spotlight" ? row("Dim the wall", fRange(F, "dim", 0, 0.7, 0.05), "How dark the wall around the region gets (light design: 70 % of it).", true) : [
        ...row("Colour", fSelect(F, "color_mode", [["accent", "Wall ink (gold)"], ["category", "Category colour"], ["fixed", "Fixed"]]), null, true),
        ...(F.color_mode === "fixed" ? row("Fixed colour", fColor(F, "color"), null, true) : []),
        ...(fmode === "tint" || fmode === "gradient" ? row("Blend", fSelect(F, "blend", [["normal", "Normal (paint over)"], ["screen", "Screen (light: lifts the tiles)"]]), "Screen adds light, so cobalt stays cobalt and white glows; normal lays the colour on top.", true) : [])]),
        ...(fmode === "gradient" ? [
          ...row("Shape", fSelect(F, "grad", [["radial", "Radial (centre → edge)"], ["linear", "Linear (angle)"]]), null, true),
          ...(F.grad === "linear" ? row("Angle", fRange(F, "angle_deg", 0, 360, 5, "°"), null, true) : []),
          ...row("Alpha from", fRange(F, "alpha_from", 0, 0.6, 0.01), null, true),
          ...row("Alpha to", fRange(F, "alpha_to", 0, 0.6, 0.01), null, true)]
        : fmode === "pattern" ? [
          ...row("Pattern", fSelect(F, "pattern", [["hatch", "Hatching"], ["dots", "Dots"], ["azulejo", "Azulejo tile grid"]]), null, true),
          ...row("Scale", fRange(F, "scale_cm", 2, 20, 0.5, " cm"), null, true),
          ...row("Opacity", fRange(F, "opacity", 0, 0.6, 0.01), null, true)]
        : fmode === "spotlight" ? [] : row("Opacity", fRange(F, "opacity", 0, 0.6, 0.01), null, true)),
        ...(fmode !== "pattern" ? row("Edge feather", fRange(F, "feather_cm", 0, 8, 0.5, " cm"), "feather", true) : [])] : []),
    ]),
    fold("st_motion", "Motion", {}, () => [
      fold("st_intro", "Intro (reveal)", { defaultOpen: true }, () => [
        ...row("Preset", fSelect(M.intro, "preset", [["fade", "Fade"], ["draw_on", "Draw on"], ["build", "Build (knots, then edges)"]]), "intro"),
        ...row("Duration", fRange(M.intro, "duration_s", 0.2, 4, 0.1, " s")),
        ...(M.intro.preset === "build" ? row("Segment delay", fRange(M.intro, "segment_delay_s", 0.02, 0.4, 0.01, " s"), null, true) : []),
        ...(M.intro.preset === "draw_on" ? row("Direction", fSelect(M.intro, "direction", [["cw", "Clockwise"], ["ccw", "Counter-clockwise"]]), null, true) : []),
      ]),
      fold("st_idle", "Idle (while visible)", { defaultOpen: true }, () => [
        ...row("Preset", fSelect(M.idle, "preset", [["none", "None"], ["breathe", "Breathe"], ["flow", "Flow"], ["comet", "Comet"], ["sweep", "Sweep (fill)"], ["knot_pulse", "Knot pulse"]]), "idle"),
        ...(M.idle.preset !== "none" ? [...row("Period", fRange(M.idle, "period_s", 0.5, 10, 0.1, " s"), null, true), ...row("Intensity", fRange(M.idle, "intensity", 0, 1, 0.05), null, true)] : []),
        ...(M.idle.preset === "comet" ? row("Count", fRange(M.idle, "count", 1, 3, 1), null, true) : []),
      ]),
      fold("st_focus", "Focus (on tap)", { defaultOpen: true }, () => [
        ...row("Preset", fSelect(M.focus, "preset", [["brighten", "Brighten"], ["redraw", "Redraw"], ["lock_on", "Lock-on"], ["spotlight", "Spotlight"], ["appear", "Appear (from the rest opacity)"]], () => { if (M.focus.preset === "spotlight" || M.focus.preset === "appear") M.focus.amount = Math.min(M.focus.amount, 0.9); else if (M.focus.amount < 1) M.focus.amount = 1.6; }), "focus"),
        ...(M.focus.preset === "spotlight" ? row("Dim the rest", fRange(M.focus, "amount", 0, 0.9, 0.05), null, true) : M.focus.preset === "appear" ? row("Opacity on tap", fRange(M.focus, "amount", 0, 1, 0.05), null, true) : M.focus.preset === "redraw" ? [] : row("Amount", fRange(M.focus, "amount", 1, 3, 0.1, "×"), null, true)),
        ...row("Duration", fRange(M.focus, "duration_s", 0.1, 2, 0.1, " s")),
      ]),
    ]),
  ];
}
function levelsBody() {
  const tb = h("tbody"), LAY = [["point", "P", "Point"], ["region", "R", "Region"], ["label", "T", "Tag (a label or a card, by Tag style)"], ["model", "M", "Model (phase C)"]];
  D.levels.forEach(l => tb.append(h("tr", {},
    h("td", {}, l.name),
    h("td", { style: "white-space:nowrap" }, ...LAY.map(([k, c, t]) => h("label", { title: t, style: "margin-right:4px;font-size:11px" }, h("input", { type: "checkbox", checked: !!l.layers[k], onchange: e => { snap(); l.layers[k] = e.target.checked; changed(); } }), c))),
    h("td", {}, h("select", { "aria-label": l.name + " point style", onchange: e => { snap(); l.point_style = e.target.value; changed(); setScreen("wall"); } }, [["", `(Default: ${pointStyleByKey(D.point_default)?.name ?? "—"})`], ...D.point_styles.map(t => [t.key, t.name])].map(([v, t]) => h("option", { value: v, selected: (l.point_style || "") === v }, t)))),
    h("td", {}, h("select", { "aria-label": l.name + " region style", onchange: e => { snap(); l.region_style = e.target.value; changed(); } }, styleOpts(`(Default: ${styleByKey(D.region_default)?.label ?? "—"})`, l.region_style).map(([v, t]) => h("option", { value: v, selected: (l.region_style || "") === v }, t)))),
    h("td", {}, h("select", { "aria-label": l.name + " on tap", onchange: e => { snap(); l.tap_layout = e.target.value; changed(); const i = D.pois.findIndex(p => p.level === l.key); if (i >= 0) runDemo({ step: "tapped", poi: i, preview: null, view: null, after: frameForDemo }, true); } }, [["", `(Default: ${tapStyleByKey(D.app.tap.default_layout)?.name ?? "—"})`], ...D.tap_layouts.map(t => [t.key, t.name])].map(([v, t]) => h("option", { value: v, selected: (l.tap_layout || "") === v }, t)))),
    h("td", { style: "min-width:110px" }, fRange(l, "reveal_delay_s", 0, 2, 0.1, " s")("rv_" + l.key)))));   // a slider: when the level appears after the wall is found
  return [h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th", {}, "Level"), h("th", {}, "Layers"), h("th", {}, "Point style"), h("th", {}, "Region style"), h("th", {}, "On tap"), h("th", {}, "Reveal"))), tb),
    h("p", { class: "guide" }, "Layers: P point · R region · T label · M model. Each level picks a point style (Looks › Point styles) and a region style, or keeps the app defaults. Unity's per-level size and effects live in the point style now; the reveal delay staggers the levels.")];
}

