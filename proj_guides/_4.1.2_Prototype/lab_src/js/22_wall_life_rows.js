/* Wall Life editor rows */
const selLife = () => ui.life.pi < 0 ? D.life.wall[ui.life.ci] : D.pois[ui.life.pi]?.world.content?.[ui.life.ci];
function selectLife(o) {
  ui.life = { pi: o.pi, ci: o.ci };
  if (o.pi < 0) { ui.tab = "global"; ui.open["life_g"] = true; ui.open["life_item_g"] = true; }
  else { ui.tab = "specific"; ui.poi = o.pi; ui.open["poi_" + o.pi] = true; ui.open["content_" + o.pi] = true; ui.open["life_item_" + o.pi] = true; }
  setScreen("life"); renderEditor();
  requestAnimationFrame(() => { const el = edb.querySelector(".ut tr.sel[data-life]"); if (el) el.scrollIntoView({ block: "nearest" }); });
}
function lifeTable(list, pi, p) {
  const tb = h("tbody");
  list.forEach((c, ci) => {
    const sel = ui.life.pi === pi && ui.life.ci === ci, K_ = KINDS[c.kind], S = p ? shapeByKey(p, c.anchor) : null;
    tb.append(h("tr", { class: sel ? "sel" : "", "data-life": "1", onclick: e => { if (e.target.closest("button,input,select")) return; ui.life = { pi, ci }; ui.open[pi < 0 ? "life_item_g" : "life_item_" + pi] = true; setScreen("life"); renderEditor(); } },
      h("td", {}, h("input", { class: "uin", value: c.name, "aria-label": "Row name", onchange: e => { const v = e.target.value.trim(); if (!v) { e.target.value = c.name; return; } snap(); c.name = v; changed(); } })),
      h("td", {}, h("span", { class: "kc", title: K_.label }, K_.code + " " + K_.label)),
      h("td", { style: "color:#AAB" }, K_.anchor === "screen" ? "screen edge" : S ? S.name : (p ? "(none)" : "sky band")),
      h("td", { style: "color:#AAB" }, c.trigger + (c.trigger === "every" ? ` ${c.every_min_s}–${c.every_max_s}s` : "")),
      h("td", {}, h("button", { class: "ib", title: "Trigger now (Play Mode)", onclick: () => { setScreen("life"); triggerNow({ c, p, pi, ci }); } }, "▶")),
      h("td", {}, h("button", { class: "ib", title: "Delete row", onclick: () => { snap(); list.splice(ci, 1); ui.life.ci = Math.max(0, ci - 1); changed(); } }, "🗑"))));
  });
  return h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th", {}, "Name"), h("th", {}, "Kind"), h("th", {}, "Anchor"), h("th", {}, "Trigger"), h("th"), h("th"))), tb);
}
function addLifeRow(list, kinds, p, pi) {
  return h("select", { "aria-label": "Add a Wall Life row", onchange: e => {
    const k = e.target.value; if (!k) return; snap();
    const used = new Set(list.map(c => c.key)), pre = pi < 0 ? "wall_content_" : "content_"; let n = 1; while (used.has(pre + n)) n++;
    const want = KINDS[k].anchor, sh = p ? p.world.shapes.find(s => want === "any" || s.type === want) : null;
    list.push(mkContent(k, pre + n, KINDS[k].label + " " + n, sh ? sh.key : ""));
    ui.life = { pi, ci: list.length - 1 }; ui.open[pi < 0 ? "life_item_g" : "life_item_" + pi] = true; changed();
  } }, h("option", { value: "" }, "+ Add a row from a kind template…"), ...kinds.map(k => h("option", { value: k }, `${KINDS[k].code} · ${KINDS[k].label}`)));
}
function itemBody(c, p, pi) {
  const K_ = KINDS[c.kind], P = c.p;
  const anchors = p ? p.world.shapes.filter(s => K_.anchor === "any" || s.type === K_.anchor) : [];
  const folder = p ? `Apps/LivingRoom/Resources/LivingRoom/POIs/${p.id}/world/` : "Apps/LivingRoom/Resources/LivingRoom/Wall/world/";
  const prefab = c.name.replace(/[^A-Za-z0-9]+/g, "") + ".prefab";
  const kp = [];
  if (c.kind === "repaint") kp.push(...row("Look", fSelect(P, "look", [["night", "Night relight"], ["cartoon", "Cartoon (strong colour)"], ["then", "Then (sepia)"]])), ...row("Reveal", fSelect(P, "reveal", [["brush", "Paint on (brush strokes)"], ["wipe", "Wipe"]])), ...row("Reveal time", fRange(P, "wipe_s", 0.2, 4, 0.1, " s")), ...row("Edge feather", fRange(P, "feather_cm", 0, 8, 0.5, " cm")), ...row("Strength", fRange(P, "strength", 0, 1, 0.05)));
  if (c.kind === "flat") kp.push(...row("Subject", fSelect(P, "subject", [["curtain", "Curtain in the wind"]])), ...row("Cloth colour", fColor(P, "color")), ...row("Wave", fRange(P, "amp_cm", 0, 3, 0.1, " cm")), ...row("Speed", fRange(P, "speed", 0.2, 3, 0.1, "×")), ...row("Cover patch", fToggle(P, "cover")));
  if (c.kind === "hinged") kp.push(...row("Open angle", fRange(P, "angle_deg", 20, 170, 5, "°")), ...row("Duration", fRange(P, "dur_s", 0.2, 3, 0.1, " s")), ...row("Interior box", fToggle(P, "interior")), ...row("Close on unfocus", fToggle(P, "close_on_unfocus")));
  if (c.kind === "agent") kp.push(...row("Sprite", fSelect(P, "sprite", [["tram", "Tram"], ["horse", "Horse"]])), ...row("Size", fRange(P, "size_cm", 2, 14, 0.5, " cm")), ...row("Speed", fRange(P, "speed_cm_s", 5, 60, 1, " cm/s")));
  if (c.kind === "particles") kp.push(...row("Sprite", fSelect(P, "sprite", [["birds", "Birds"], ["leaves", "Falling leaves"]])), ...row("Count", fRange(P, "count", 1, 20, 1)), ...row("Speed", fRange(P, "speed_cm_s", 10, 80, 1, " cm/s")));
  if (c.kind === "guest") kp.push(...row("Edge", fSelect(P, "edge", [["right", "Right"], ["left", "Left"]])), ...row("Stay", fRange(P, "stay_s", 0.5, 6, 0.1, " s")), ...row("Hide while a card is open", fToggle(P, "hide_on_card")));
  const out = [
    ro("Kind", `${K_.code} · ${K_.label} · precision ${K_.prec}`),
    ro("Lives", K_.lives), ro("Made with", K_.made),
    ...row("Name", fText(c, "name")),
  ];
  if (K_.anchor !== "screen") {
    if (p) out.push(...row("Anchor shape", fSelect(c, "anchor", [["", K_.anchor === "any" ? "(none: wall box)" : "(pick a shape)"], ...anchors.map(s => [s.key, `${s.name} · ${s.type}${isQuiet(styleFor(p, s)) ? " · quiet" : ""}`])])));
    else out.push(ro("Anchor", "Sky band (top of the wall)"));
    if (p && !shapeByKey(p, c.anchor) && K_.anchor !== "any") out.push(h("div", { class: "notice" }, `A ${K_.label} needs a ${K_.anchor} shape. Draw one in Region › Shapes (style Hidden if it should not show an outline).`));
  }
  out.push(...row("Trigger", fSelect(c, "trigger", Object.entries(TRIGS).map(([k, l]) => [k, l, k === "lapse" || !K_.trigs.includes(k)])), "trigger"));
  if (c.trigger === "every") out.push(...row("Every from", fRange(c, "every_min_s", 2, 60, 1, " s"), null, true), ...row("Every to", fRange(c, "every_max_s", 2, 90, 1, " s"), null, true));
  if (c.trigger === "reveal" || c.trigger === "loop") out.push(...row("Delay", fRange(c, "delay_s", 0, 5, 0.1, " s"), null, true));
  out.push(...row("Reduce motion", fSelect(c, "reduce", [["", `(wall: ${D.life.settings.reduce_motion_state})`], ["static", "Still frame"], ["hidden", "Hidden"]]), "reduce"));
  out.push(fold("kp_" + (pi < 0 ? "g" : pi), K_.label + " settings", { defaultOpen: true }, () => kp));
  out.push(ro("Prefab", folder + prefab));
  out.push(h("div", { class: "addrow" }, h("button", { class: "ubtn", onclick: () => triggerNow({ c, p, pi, ci: ui.life.ci }) }, "▶ Trigger now"), h("button", { class: "ubtn", disabled: true, title: "Unity only: opens the prefab in Prefab Mode in Context, with the wall around it" }, "Edit prefab")));
  return out;
}
function lifeGlobalBody() {
  const B = D.life.settings, list = D.life.wall, c = ui.life.pi < 0 ? list[ui.life.ci] : null;
  return [
    h("p", { class: "guide" }, "Wall-level rows: they belong to no POI."),
    lifeTable(list, -1, null),
    h("div", { class: "addrow" }, addLifeRow(list, ["particles", "guest"], null, -1)),
    c ? fold("life_item_g", "Row · " + c.name, { defaultOpen: true, color: "#F2C14E" }, () => itemBody(c, null, -1)) : null,
    fold("life_settings", "Settings", { defaultOpen: true }, () => [
      ...row("Reduce motion", fSelect(B, "reduce_motion_state", [["static", "Still frame"], ["hidden", "Hidden"]]), "reduce"),
      ...row("Show regions (Lab)", fToggle(B, "show_regions")),
      h("ul", { class: "guide" }, h("li", {}, "No attention budget: how much moves is the author's call."), h("li", {}, "Wall Life screen ▶: rows play on their triggers; tap a POI for focus rows, tap the wall to close."), h("li", {}, "Timings here are shortened for the Lab; on the wall, ambient events are rarer.")),
    ]),
  ];
}
function contentBody(p, pi) {
  const list = p.world.content || (p.world.content = []), c = ui.life.pi === pi ? list[ui.life.ci] : null;
  return [
    h("div", { class: "addrow" }, h("button", { class: "ubtn", disabled: true, title: "Unity only: renders the POI's GLB region straight-on to a PNG at a known scale, outlines on a separate layer" }, "Export wall canvas"), h("button", { class: "ubtn", disabled: true, title: "Unity only: puts a PSD/PSB painted over the canvas back on a quad in the same rectangle" }, "Import wall art"), helpBtn("content")), helpText("content"),
    list.length ? lifeTable(list, pi, p) : h("p", { class: "guide" }, "No Wall Life rows yet."),
    h("div", { class: "addrow" }, addLifeRow(list, ["repaint", "flat", "hinged", "agent", "particles"], p, pi)),
    c ? fold("life_item_" + pi, "Row · " + c.name, { defaultOpen: true, color: "#F2C14E" }, () => itemBody(c, p, pi)) : null,
  ];
}

