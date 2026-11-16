/* ---------- editor window (Unity-like) ---------- */
const HELP = {
  styles: "A region style is defined once for the wall. Hierarchy levels pick one; a POI or a single shape may pick another. Change a style and every shape that uses it changes. The name is what you see in dropdowns; the key is generated and never shown in the real Editor.",
  width: "Line width in centimetres on the wall: it behaves like paint, thinner when you stand further away. Min / Max px keep it readable on screen at any distance.",
  minmax: "Screen limits for the line width, in pixels.",
  colormode: "Category: the POI's category colour from the Marker table. Fixed: one colour for every POI using this style.",
  pattern: "Solid, dashed or dotted. Dash and gap are centimetres on the wall.",
  placement: "On edge: the line sits on the outline. Outside: it is pushed out by the offset, so a few cm of tracking error still reads as 'around the building'. Inside: pushed in. Paths ignore this.",
  offset: "Pushes a closed outline outward by this many cm, so tracking error does not cut the line across the facade. 0 = on the edge. Paths and Points ignore it. The drawn contour stays exact for the fill, the spotlight hole and the tap area.",
  glow: "A soft glow drawn by the line's own shader. No camera-wide Bloom: it would make the camera image milky.",
  double: "A second, thinner line further out. An architectural-drawing look.",
  showat: "Which knots get a mark: none, only the corners (sharp and rounded knots), or all knots. A Points shape always shows every knot.",
  mark: "The shape of the mark. Corner bracket draws an L along the two edges; on a Points shape it falls back to a ring.",
  fill: "None; Tint: one faint colour; Gradient: radial (centre to edge) or linear (an angle), alpha from → to, so the region fades into the wall; Pattern: hatching, dots or an azulejo tile grid in the style's colour; Media: an image or a video seen only inside the shape (the shape cuts it). Keep colour fills light: modulate the camera image, do not paste over it.",
  actlib: "The action library is framework code: each action type knows its controls (rotate, zoom, play a moment…). The wall shows them around a tapped POI (Global Scene › Interaction); the Detail Card shows the same action as a block on screen. A POI only supplies the content (Specific Marker › Content for actions).",
  model: "Any format at authoring time: glTF / GLB, FBX, OBJ, Blender, USD… are imported in the Editor into a prefab (POIs/<id>/models/). At runtime the app loads one form: the prefab, or a GLB streamed with glTFast.",
  moments: "Named clips inside the model prefab, offered as buttons by the 3D view: “Rebuild the palace”, “1755: the earthquake”. Each plays an Animator trigger or a Timeline of that name: the framework knows only “play moment X”, the prefab holds the animation.",
  media: "The picture or video shown inside this shape. In Unity: an asset field (Texture2D or VideoClip) from POIs/<poi_id>/media/. The region's own fill mesh gets planar UVs from the shape's box, and its material samples the texture or the VideoPlayer's RenderTexture: no stencil, no extra camera. Here you can load your own file for this session.",
  feather: "Softens the fill edge in centimetres. A soft edge hides alignment error.",
  intro: "How the shape appears when the POI is revealed (after its level's reveal delay). Fade; Draw on: one continuous pen stroke; Build: the knots pop in, then each edge draws between them.",
  idle: "The quiet life while visible. Breathe: the glow swells and fades. Flow: dashes march along the line. Comet: sparks travel the line. Sweep: a band of light crosses the fill. Knot pulse: the marks pulse in turn.",
  focus: "What happens when the visitor taps the POI. Brighten: width and glow go up. Redraw: the intro plays again. Lock-on: corner brackets close in. Spotlight: the rest of the wall dims. Appear: the line goes from its rest opacity to the tap opacity (a quiet window shows its edge only when tapped).",
  precision: "Not a setting: an estimate of how much tracking error this style hides before the misalignment shows, from the outward offset, half the line width, a quarter of the glow, half the fill feather (Spotlight counts as about 4 cm). Compared with the alignment error measured on the real wall (step 0). The formula is a guess to calibrate on the phone.",
  test: "Simulates tracking error by shifting everything (Play Mode and the Precision screen). Reduce motion replaces intros with a fade and stops idle motion and ambient Wall Life.",
  levels: "Each hierarchy level chooses a presence (point, region or both) and a region style. Every POI of that level takes them unless it overrides.",
  presence: "Point: the existing marker only. Region: the shapes are the marker. Both: a smaller point plus the shapes. A POI with no visible shapes always shows as a point.",
  poistyle: "Picks another named style for this POI. To change only the look of one POI, create a new style: values are never overridden one by one.",
  shapes: "The shapes drawn on the wall for this POI. Polygon: closed (a facade, a square). Path: open (a street, a ridgeline). Points: separate marks (windows, lamps). The style column lets one shape use another style, e.g. the door. A quiet style (line opacity 0, no fill) draws nothing at rest but still anchors World Content and taps; change its style later to show it, or use Focus › Appear so its edge shows on tap.",
  corner: "Sharp: a straight corner. Rounded: a fillet of the given radius. Smooth: the curve passes through the knot. Default applies to knots set to Default.",
  unity: "In Unity: each shape is a SplineContainer child of the POI, drawn on the GLB with the Splines Draw tool. Sharp = tangent mode Linear, Smooth = Auto; the rounded radius is stored per knot (SplineData<float>) and our mesh builder rounds the corner. Capture writes the knots to config.json, Apply writes them back.",
  life: "Living content on the wall. For the framework every row is the same thing: a prefab anchored to the wall plus a schedule (trigger). The kind only decides the anchor, the masks and the template the Editor creates. Wall-level rows belong to no POI (birds, the mascot, rain).",
  content: "This POI's Wall Life rows. Each row creates its prefab from the kind's template in POIs/<poi_id>/world/ and opens it in Prefab Mode in Context, with the wall around it. Export wall canvas renders the wall straight-on at a known scale for painting in Krita / Photoshop; Import wall art puts the layers back exactly in place.",
  trigger: "When the row plays. Reveal: once when the POI appears. Loop: always while visible. Focus: when the visitor taps the POI (and back when the card closes). Unfocus: when the card closes. Every: at random every N–M s. Lapse: when the time period changes (the timeline domain, later). Inside the prefab the trigger calls an Animator trigger of the same name, a Timeline, ParticleSystem.Play or VideoPlayer.Play.",
  reduce: "What the row does when the visitor asks for reduced motion: show a still frame, or hide. Every and screen guests always hide.",
};
let uid = 0;
const h = (tag, props = {}, ...kids) => {
  const el = document.createElement(tag);
  for (const [k, v] of Object.entries(props || {})) {
    if (k === "class") el.className = v; else if (k === "style") el.style.cssText = v;
    else if (k.startsWith("on")) el.addEventListener(k.slice(2), v);
    else if (v === true) el.setAttribute(k, ""); else if (v !== false && v != null) el.setAttribute(k, v);
  }
  for (const c of kids.flat()) if (c != null && c !== false) el.append(c.nodeType ? c : document.createTextNode(c));
  return el;
};
const helpBtn = key => key ? h("button", { class: "hb", type: "button", title: "Help", "aria-expanded": String(ui.help === key), onclick: () => { ui.help = ui.help === key ? null : key; renderEditor(); } }, "?") : h("span");
const helpText = key => key && ui.help === key ? h("div", { class: "uhelp" }, HELP[key] ?? key) : null;
function row(label, ctrl, help, cond) { const id = "f" + (++uid), c = ctrl(id), sc = c && c._sc;
  return [h("div", { class: "urow" + (cond ? " cond" : "") + (sc ? " scoped" + (sc.inherited ? " inh" : sc.own && sc.base ? " own" : "") : "") }, h("label", { for: id }, label, sc ? scopeTag(sc) : null), c, helpBtn(help)), helpText(help)]; }
/* where a value is saved: the look (and mode) or the time / visitors option of the profile bar; inherited values say where from */
function scopeTag(sc) {
  if (sc.inherited) return h("span", { class: "sct inh", title: `Inherited from ${sc.base}. Change it to give ${sc.label} its own value.` }, "from " + sc.base);
  if (sc.own && sc.base) return h("span", { class: "sct own", title: `${sc.label} has its own value (the base is ${sc.base}). Click to reset to the inherited value.`, onclick: e => { e.preventDefault(); e.stopPropagation(); resetScoped(sc); } }, h("i"), sc.label, " ↺");
  return h("span", { class: "sct", title: `Saved in ${sc.label} (switch it in the profile bar)` }, sc.label);
}
const changed = (rebuild = true) => { if (rebuild) renderEditor(); else { drawSwatches(); refreshChips(); if (ui.tab === "app" && ui.screen === "app") renderPhone(); } };
/* every editor section shows its demo on the phone: opening it, or a click inside it, puts the phone on that demo (only when it is not already there) */
function runDemo(dm, force) {
  const want = dm.preview === undefined ? ui.compPreview : dm.preview, cur = ui.compPreview, scr = dm.screen || "app";
  const samePrev = (!cur && !want) || (!!cur && !!want && cur.type === want.type && (want.page == null || cur.page === want.page));
  const sameView = dm.view === undefined || (tapState.view?.a ?? null) === dm.view;
  if (!force && ui.screen === scr && (!dm.step || ui.appStep === dm.step) && samePrev && (dm.poi == null || ui.appPoi === dm.poi) && sameView) return;
  if (ui.screen !== scr) setScreen(scr);
  if (scr === "wall" && !camCovers(ui.cam)) camTo(camCover(ui.cam), 300);   // v19 R6: a wall demo never shows the room past the wall
  if (scr !== "app") return;
  if (dm.step) ui.appStep = dm.step;
  if (dm.poi != null) ui.appPoi = dm.poi;
  if (!samePrev || force) { ui.compPreview = want; if (!dm.keepLayout || want) ui.previewLayout = null; }
  if (dm.view !== undefined) tapState.view = dm.view ? { p: D.pois[ui.appPoi], a: dm.view } : null; else if (!samePrev) tapState.view = null;
  clearTimeout(demoTimer);
  renderPhone(); dm.after?.();
}
const fSelect0 = (obj, key, opts, after) => id => h("select", { id, onchange: e => { snap(); obj[key] = e.target.value; after?.(); changed(); } },
  opts.map(([v, l, dis]) => h("option", { value: v, selected: obj[key] === v, disabled: !!dis }, l)));
const fRange0 = (obj, key, min, max, step, unit = "") => id => {
  let armed = false; const out = h("output", {}, fmt(obj[key], step) + unit);
  const inp = h("input", { id, type: "range", min, max, step, value: obj[key],
    onpointerdown: () => { if (!armed) { snap(); armed = true; } }, onkeydown: () => { if (!armed) { snap(); armed = true; } }, onchange: () => { armed = false; },
    oninput: e => { obj[key] = +e.target.value; out.textContent = fmt(obj[key], step) + unit; changed(false); } });
  return h("div", { class: "slide" }, inp, out);
};
const fmt = (v, step) => (+v).toFixed(step < 0.1 ? 2 : step < 1 ? 1 : 0);
const fColor0 = (obj, key) => id => {
  const hex = h("input", { class: "uin", value: obj[key].toUpperCase(), "aria-label": "Hex", onchange: e => { const v = e.target.value.trim(); if (/^#?[0-9a-f]{6}$/i.test(v)) { snap(); obj[key] = (v[0] === "#" ? v : "#" + v).toUpperCase(); changed(); } else e.target.value = obj[key]; } });
  const pick = h("input", { id, type: "color", value: obj[key], onclick: () => snap(), oninput: e => { obj[key] = e.target.value.toUpperCase(); hex.value = obj[key]; changed(false); } });
  return h("div", { class: "colr" }, pick, hex);
};
const fToggle0 = (obj, key) => id => h("input", { id, type: "checkbox", checked: obj[key], style: "justify-self:start", onchange: e => { snap(); obj[key] = e.target.checked; changed(); } });
/* every field remembers the profile scope of what it edits (look, look · mode, time, visitors), for the row's tag and dot */
const scoped = fn => (obj, key, ...a) => { const g = fn(obj, key, ...a); return id => { const el = g(id); try { if (obj && typeof SCOPE !== "undefined" && obj[SCOPE]) el._sc = scopeOf(obj, key); } catch (e) {} return el; }; };
const fSelect = scoped(fSelect0), fRange = scoped(fRange0), fColor = scoped(fColor0), fToggle = scoped(fToggle0);
const fText = (obj, key) => id => h("input", { id, class: "uin", value: obj[key], onchange: e => { const v = e.target.value.trim(); if (!v) { e.target.value = obj[key]; return; } snap(); obj[key] = v; changed(); } });
const ro = (label, text) => h("div", { class: "urow ro" }, h("label", {}, label), h("span", {}, text), h("span"));

function fold(id, title, opts, body) {
  const open = opts.locked ? false : (ui.open[id] ?? false);   // every section starts collapsed
  const el = h("div", { class: "fold" + (open ? " open" : ""), "data-id": id });   // data-id: for the review tools
  if (opts.screen) el.dataset.screen = opts.screen;
  if (opts.step) el.dataset.step = opts.step;
  if (opts.demo) { el.classList.add("hasdemo"); el._demo = opts.demo; }
  const hd = h("div", { class: "fh" + (opts.locked ? " locked" : ""), role: "button", tabindex: opts.locked ? -1 : 0, "aria-expanded": String(open),
    onclick: e => { if (opts.locked || e.target.closest("button,select,input")) return; ui.open[id] = !open; renderEditor(); if (!open && opts.demo) runDemo(opts.demo, true); opts.onToggle?.(!open); },
    onkeydown: e => { if ((e.key === "Enter" || e.key === " ") && !opts.locked) { e.preventDefault(); ui.open[id] = !open; renderEditor(); } } },
    h("span", { class: "tri" }, "▶"), h("span", { class: "ft", style: opts.color ? `color:${opts.color}` : "" }, title),
    opts.badge === "new" ? h("span", { class: "tag t-new" }, "NEW") : opts.badge === "ext" ? h("span", { class: "tag t-ext" }, "EXTENDED") : opts.badge === "lab" ? h("span", { class: "tag t-lab" }, "PROPOSED") : null,
    opts.sub ? h("span", { class: "sub" }, opts.sub) : null, h("span", { class: "grow" }), opts.extra || null);
  el.append(hd); if (open) el.append(h("div", { class: "fb" }, body()));
  return el;
}
const styleOpts = (blankLabel, current) => {
  const o = [];
  if (blankLabel !== null) o.push(["", blankLabel]);
  for (const s of D.styles) o.push([s.key, s.label]);
  if (current && !styleByKey(current)) o.push([current, current + " (missing)"]);
  return o;
};

