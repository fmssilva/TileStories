function specificTab() {
  const out = [];
  D.pois.forEach((p, pi) => {
    const open = ui.open["poi_" + pi] ?? false;
    const head = h("div", { class: "poih", role: "button", tabindex: 0, "aria-expanded": String(open), "data-screen": ui.screen === "life" ? "life" : "wall",
      onclick: e => { if (e.target.closest("button")) return; ui.open["poi_" + pi] = !open; if (!open) { ui.poi = pi; ui.shape = 0; ui.knot = -1; } renderEditor(); updateHud(); },
      onkeydown: e => { if (e.key === "Enter") { ui.open["poi_" + pi] = !open; renderEditor(); } } },
      h("span", { class: "tri", style: `display:inline-block;width:10px;font-size:9px;color:#bbb;transform:rotate(${open ? 90 : 0}deg)` }, "▶"),
      h("span", { class: "ft", style: `color:${catColor(p)}` }, `${pi + 1}. ${p.name}`), h("span", { class: "grow" }),
      h("button", { class: "ib", title: "Select in Scene", "aria-pressed": String(ui.poi === pi), onclick: () => { selectPoi(pi, 0); setScreen("wall"); renderEditor(); updateHud(); } }, "⌖"));
    out.push(head);
    if (!open) return;
    const lv = levelOf(p), A = p.actions || (p.actions = { model: { file: "", controls: { rotate: true, zoom: true, move: false }, moments: [] } });
    p.on_tap = p.on_tap || { tap_layout: "" }; p.on_tap.over = p.on_tap.over || {};
    out.push(h("div", { class: "fb" },
      fold("id_" + pi, "1 · Identity", { screen: "wall", sub: "category, level, keywords, summary" }, () => [
        ...row("Category", fSelect(p, "category", Object.entries(CATS).map(([k, c]) => [k, c.label]))),
        ...row("Hierarchy Level", fSelect(p, "level", D.levels.map(l => [l.key, l.name])), "levels"),
        ro("Keywords & summary", "existing (Summary & Keywords)")]),
      fold("pos_" + pi, "2 · Position", { screen: "wall" }, () => [ro("Position (m)", toM(p.pos).join(", ")), ro("Lat / lon", "derived from the geo-reference (planned)"), h("p", { class: "guide" }, "Drag the diamond handle in the Scene view.")]),
      fold("pres_" + pi, "3 · Presentation", { color: "#3FC1C9", badge: "new", defaultOpen: true, screen: "wall" }, () => [
        fold("rest_" + pi, "At rest · how it is seen", { defaultOpen: true, screen: "wall" }, () => [
          ...row("Layers", fSelect(p.world, "presence_override", [["", `(level ${lv?.name}: ${[["point", "point"], ["region", "region"], ["label", "tag"], ["model", "model"]].filter(([k]) => lv?.layers[k]).map(x => x[1]).join(" + ")})`], ["point", "Point"], ["region", "Region"], ["both", "Point + Region"]]), "presence"),
          ...row("Region style", fSelect(p.world, "style_override", styleOpts(`(level: ${styleByKey(lv?.region_style)?.label ?? "missing"})`, p.world.style_override)), "poistyle"),
          ...row("Point style", fSelect(p.world, "point_style", [["", `(level: ${pointStyleByKey(lv?.point_style)?.name ?? pointStyleByKey(D.point_default)?.name})`], ...D.point_styles.map(t => [t.key, t.name])])),
          ...row("Content", fSelect(p, "content", [["", "Ready (its story is written)"], ["soon", "Coming soon (name only for now)"]], () => { renderPhone?.(); renderTapWall?.(); }), "Coming soon: the POI is on the wall with its name; a tap shows “Coming soon” and a line for the visitor's audience (App › 5 On tap › Coming soon). A POI with no summary and no story counts as coming soon."),
          ...row("Status", fSelect(p, "status", [["", "(none: no ring, no badge)"], ...D.status_levels.map(s => [s.key, s.label])]), "The wall's outline level (Unity: has_status + outline level): the point's ring and badge show it."),
  fold("shapes_" + pi, "Shapes", { defaultOpen: true, screen: "wall", sub: `${p.world.shapes.length} shape(s)` }, () => regionBody(p, pi))]),
        fold("tap_" + pi, "On tap · what a tap does", { screen: "app", step: "tapped", demo: { step: "tapped", poi: pi, preview: null, view: null, keepLayout: true, after: frameForDemo }, sub: tapStyleOf(p).name + (Object.keys(p.on_tap.over).length ? ` · ${Object.keys(p.on_tap.over).length} override(s)` : "") }, () => onTapPoiBody(p, pi, lv)),
        fold("content_" + pi, "Alive · Wall Life rows", { sub: `${(p.world.content || []).length} row(s)`, screen: "life" }, () => contentBody(p, pi))]),
      fold("acts_" + pi, "4 · Content for actions", { color: "#C9A6F2", badge: "new", screen: "app", step: "tapped", demo: { step: "tapped", poi: pi, preview: null, after: frameForDemo }, sub: "what this POI's buttons show" }, () => actionContentBody(p, A)),
      fold("cc_" + pi, "5 · Card", { locked: true, sub: "existing · Card Content → Detail Card tab" }, () => [])));
  });
  out.push(fold("json_s", "config.json (live preview)", { color: "#9BB0C8", sub: "selected POI" }, () => [h("pre", { class: "json" }, jsonPoi(D.pois[ui.poi]))]));
  return out;
}
function actionContentBody(p, A) {
  const M = A.model, used = new Set(poiActions(p));
  const mark = k => used.has(k) ? "" : " · not in this POI's action set";
  const mt = h("tbody"); M.moments.forEach((m, i) => mt.append(h("tr", {},
    h("td", {}, h("input", { class: "uin", value: m.name, onchange: e => { snap(); m.name = e.target.value; changed(); } })),
    h("td", {}, h("input", { class: "uin", value: m.plays, onchange: e => { snap(); m.plays = e.target.value; changed(); } })),
    h("td", {}, h("button", { class: "ib", onclick: () => { snap(); M.moments.splice(i, 1); changed(); } }, "🗑")))));
  return [
    fold("am_" + p.id, "3D model" + mark("model_wall"), { defaultOpen: true, screen: "app", step: "tapped", demo: { step: "tapped", poi: D.pois.indexOf(p), preview: null, view: "model_wall" }, extra: tagChip("new"), sub: "used by 3D on the wall, 3D on screen and the Model layer" }, () => [
      ...row("Model file", fSelect(M, "file", [["", "(none)"], ["twin_tower_church.glb", "twin_tower_church.glb (sample)"], ["castle_photogrammetry.fbx", "castle_photogrammetry.fbx (sample)"]]), "model"),
      ...row("Visitor may rotate", fToggle(M.controls, "rotate")), ...row("Visitor may zoom", fToggle(M.controls, "zoom")), ...row("Visitor may move", fToggle(M.controls, "move")),
      h("div", { class: "urow" }, h("label", {}, "Moments"), h("span", { class: "guide", style: "margin:0" }, "buttons the 3D view offers; each plays a clip inside the prefab"), helpBtn("moments")), helpText("moments"),
      M.moments.length ? h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th", {}, "Button"), h("th", {}, "Plays (in the prefab)"), h("th"))), mt) : null,
      h("div", { class: "addrow" }, h("button", { class: "ubtn", onclick: () => { snap(); M.moments.push({ name: "New moment", plays: "Animator trigger · " }); changed(); } }, "+ Add moment"))]),
    planned("ap_" + p.id, "360 visit" + mark("pano"), "planned", "An equirectangular photo or video around the visitor.", ["360 file (POIs/<id>/pano/)"], "Task: phase B.", { screen: "app", step: "tapped", demo: { step: "tapped", poi: D.pois.indexOf(p), preview: null, view: "pano" } }),
    planned("amap_" + p.id, "Today on the map" + mark("map"), "planned", "Where it is today; directions; street view.", ["lat / lon (derived)", "street-view link"], "Task: later, with the geo-reference.", { screen: "app", step: "tapped", demo: { step: "tapped", poi: D.pois.indexOf(p), preview: null, view: null } }),
    planned("atn_" + p.id, "Then & now" + mark("then_now"), "planned", "Today's photo over the tiles, with a wipe.", ["photo", "4 corner points on the wall"], "Task: phase B.", { screen: "app", step: "tapped", demo: { step: "tapped", poi: D.pois.indexOf(p), preview: null, view: "then_now" } }),
    planned("aq_" + p.id, "Quiz" + mark("quiz"), "planned", "This POI's questions, used by the Quiz action and the Quiz rally game.", ["questions", "answers", "difficulty"], "Task: phase E.", { screen: "app", step: "tapped", demo: { step: "tapped", poi: D.pois.indexOf(p), preview: null, view: null } }),
  ];
}
function regionBody(p, pi) {
  const W = p.world, lv = levelOf(p), pres = presenceOf(p);
  const out = [];
  const visible = W.shapes.filter(s => !isQuiet(styleFor(p, s)));
  if (pres !== "point" && !visible.length) out.push(h("div", { class: "notice" }, `The Region layer is on but there are no visible shapes yet: this POI shows as a point.`));
  if (pres === "point" && visible.length) out.push(h("div", { class: "notice" }, "The Region layer is off: the shapes are kept but hidden in Play Mode (dashed in the Scene view)."));
  const tb = h("tbody");
  W.shapes.forEach((S, si) => {
    const sel = pi === ui.poi && si === ui.shape;
    tb.append(h("tr", { class: sel ? "sel" : "", onclick: e => { if (e.target.closest("button,input,select")) return; ui.poi = pi; ui.shape = si; ui.knot = -1; renderEditor(); updateHud(); } },
      h("td", {}, h("input", { class: "uin", value: S.name, "aria-label": "Shape name", onchange: e => { const v = e.target.value.trim(); if (!v) { e.target.value = S.name; return; } snap(); S.name = v; changed(); } })),
      h("td", {}, S.type),
      h("td", {}, h("select", { "aria-label": "Shape style", onchange: e => { snap(); S.style = e.target.value; changed(); } }, styleOpts("(POI style)", S.style).map(([v, t]) => h("option", { value: v, selected: S.style === v }, t)))),
      h("td", { class: "num" }, String(S.knots.length)),
      h("td", {}, h("button", { class: "ib", title: "Edit in Scene", "aria-pressed": String(sel && ui.tool === "edit"), onclick: () => { ui.poi = pi; ui.shape = si; ui.knot = -1; setScreen("wall"); setTool("edit"); } }, "✎")),
      h("td", {}, h("button", { class: "ib", title: "Delete shape", onclick: () => { snap(); W.shapes.splice(si, 1); fixSel(); changed(); } }, "🗑"))));
  });
  out.push(h("div", { class: "urow" }, h("label", {}, "Shapes"), h("span"), helpBtn("shapes")), helpText("shapes"));
  if (W.shapes.length) out.push(h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th", {}, "Name"), h("th", {}, "Type"), h("th", {}, "Style"), h("th", {}, "Knots"), h("th"), h("th"))), tb));
  out.push(h("div", { class: "addrow" }, ...[["polygon", "+ Polygon"], ["path", "+ Path"], ["points", "+ Points"], ["rect", "+ Rectangle"]].map(([t, l]) =>
    h("button", { class: "ubtn", "aria-pressed": String(pi === ui.poi && ui.tool === t), onclick: () => { ui.poi = pi; setScreen("wall"); setTool(t); } }, l))));
  out.push(h("div", { class: "addrow" }, h("button", { class: "ubtn", disabled: true, title: "Unity only: reads the scene splines into config.json" }, "Capture from Scene"), h("button", { class: "ubtn", disabled: true, title: "Unity only: rebuilds the scene splines from config.json" }, "Apply to Scene"), helpBtn("unity")), helpText("unity"));
  const S = pi === ui.poi ? W.shapes[ui.shape] : null;
  if (S) {
    const st = styleFor(p, S), g = build(S), Lm = plen(g.pts) / PXCM / 100;
    out.push(fold("shape", "Shape · " + S.name, { defaultOpen: true, color: "#F2C14E" }, () => {
      const q = S.knots[ui.knot], kc = q ? cornerOf(S, ui.knot) : null, end = q && S.type === "path" && (ui.knot === 0 || ui.knot === S.knots.length - 1);
      return [
        ro("Uses style", st ? st.label + (S.style ? " (shape)" : W.style_override ? " (POI)" : " (level)") : "(missing)"),
        ro("Size", S.type === "points" ? `${S.knots.length} points` : `${S.knots.length} knots · ${S.type === "polygon" ? "perimeter" : "length"} ${Lm.toFixed(2)} m` + (g.closed ? ` · area ${(Math.abs(signedArea(g.pts)) / PXCM / PXCM / 10000).toFixed(3)} m²` : "")),
        ...(st && st.fill.mode === "media" && g.closed ? [...row("Media", fSelect(S, "media", [["", "(pick an image or video)"], ...D.media.map(m => [m.key, m.name])]), "media"),
          h("div", { class: "addrow" }, h("label", { class: "ubtn", style: "display:flex;align-items:center;justify-content:center;cursor:pointer" }, "Load an image or video…",
            h("input", { type: "file", accept: "image/*,video/*", style: "display:none", onchange: e => { const f = e.target.files[0]; if (f) loadMediaFile(f, S); } }))),
          ...(S.media ? [] : [h("div", { class: "notice" }, "This shape's style fills it with media, but no file is picked: it shows a faint tint.")])] : []),
        ...(S.type !== "points" ? row("Default corner", fSelect(S, "default_corner", [["sharp", "Sharp"], ["rounded", "Rounded"], ["smooth", "Smooth"]]), "corner") : []),
        q ? fold("knot", `Knot ${ui.knot + 1} of ${S.knots.length}`, { defaultOpen: true }, () => [
          ro("Position (m)", toM(q).join(", ")),
          ...(S.type !== "points" && !end ? row("Corner", fSelect(q, "corner", [["", `Default (${S.default_corner})`], ["sharp", "Sharp"], ["rounded", "Rounded"], ["smooth", "Smooth"]]), "corner") : []),
          ...(kc === "rounded" && !end ? row("Radius", fRange(q, "radius", 1, 30, 0.5, " cm"), null, true) : []),
          h("div", { class: "addrow" }, h("button", { class: "ubtn", onclick: removeKnot }, "Remove knot")),
        ]) : h("p", { class: "guide" }, "Click a knot in the Scene view to edit its corner."),
      ];
    }));
  }
  return out;
}


/* a POI's On tap: the style (inherited unless picked) and single-field overrides, Unity-style: an overridden field is bold
   with a blue bar and a ↺ to revert to the inherited value */
function onTapPoiBody(p, pi, lv) {
  const O = p.on_tap.over, inh = (() => { const keep = p.on_tap.over; p.on_tap.over = {}; const L = tapLayoutOf(p); p.on_tap.over = keep; return L; })();
  const show = () => { ui.appPoi = pi; ui.appStep = "tapped"; ui.previewLayout = null; tapState.view = null; ui.tapAnchor = null; };
  const set = (k, v) => { snap(); if (v === undefined) delete O[k]; else O[k] = v; show(); changed(); };
  const orow = (label, k, ctl, inherited, guide) => {
    const on = O[k] !== undefined;
    const key = guide ? "ovr_" + k : null; if (guide) HELP[key] = guide;
    return [h("div", { class: "urow ovrow" + (on ? " ovr" : "") }, h("label", {}, label), h("div", { class: "ovf" }, ctl, on ? h("button", { class: "ib", title: `Revert to ${inherited}`, onclick: () => set(k, undefined) }, "↺") : h("span", { class: "inh", title: "inherited" }, inherited)), helpBtn(key)), helpText(key)];
  };
  const pick = (k, opts, cur) => h("select", { onchange: e => set(k, e.target.value) }, opts.map(([v, t]) => h("option", { value: v, selected: cur === v }, t)));
  const lvStyle = tapStyleByKey(lv?.tap_layout), from = lvStyle ? `level ${lv.name}` : "app default";
  const btnState = k => O.buttons?.[k];
  const setBtn = (k, v) => { const b = { ...(O.buttons || {}) }; if (v) b[k] = v; else delete b[k]; set("buttons", Object.keys(b).length ? b : undefined); };
  return [
    ...row("On-tap style", fSelect(p.on_tap, "tap_layout", [["", `(${from}: ${(lvStyle || tapStyleByKey(D.app.tap.default_layout))?.name})`], ...D.tap_layouts.map(t => [t.key, t.name])], () => { show(); renderPhone(); })),
    h("div", { class: "grp" }, "Overrides", h("span", {}, "change one field, keep the rest of the style")),
    ...orow("Distance to the marker", "gap", h("input", { type: "range", min: 0, max: 20, step: 1, value: O.gap ?? inh.gap, oninput: e => { O.gap = +e.target.value; show(); renderPhone(); }, onchange: e => set("gap", +e.target.value) }), `${inh.gap}`,
      "How far the tag and the buttons sit from the marker (phone units). Raise it for a big marker or a 3D model so nothing covers it."),
    ...orow("Tag component", "tag", pick("tag", [["none", "None"], ...D.tap_tags.map(t => [t.key, t.name])], O.tag ?? inh.style.tag), tagComp(inh.style.tag)?.name || "None"),
    ...orow("Action component", "actions", pick("actions", [["none", "None"], ...D.tap_action_comps.map(t => [t.key, t.name])], O.actions ?? inh.style.actions), actComp(inh.style.actions)?.name || "None"),
    ...orow("Focus", "focus", pick("focus", TAP_OPTS.focus, O.focus ?? inh.focus), inh.focus),
    h("div", { class: "grp" }, "Buttons", h("span", {}, "from the app's button list; auto ones need this POI's content")),
    h("div", { class: "btnlist" }, ...D.tap_actions.filter(a => a.show !== "off" || btnState(a.key)).map(a => {
      const auto = a.show === "auto", has = auto ? (NEEDS[a.key]?.[0](p) ?? true) : true, st = btnState(a.key), shown = st === "hide" ? false : st === "show" ? true : has;
      return h("label", { class: "bchip" + (st ? " ovr" : "") + (shown ? "" : " off"), title: auto && !has && !st ? `Hidden: this POI has no ${NEEDS[a.key][1]}` : "" },
        h("input", { type: "checkbox", checked: shown, onchange: e => setBtn(a.key, e.target.checked === has ? null : e.target.checked ? "show" : "hide") }), h("span", { class: "i" }, svg(a.key)), a.label, auto && !has && !st ? h("small", {}, "no content") : null);
    })),
    h("p", { class: "guide" }, "Bold with a blue bar = overridden here; ↺ goes back to the style. Buttons: untick to hide one for this POI only; an “auto” button without content shows greyed."),
  ];
}
