/* ---------- App › 1 Design system: the app's own UI kit ----------
   Foundations (tokens): audience and mode, colour (one design per mode, D.themes; a closed accent list), type, spacing and shape,
   depth and motion. Components: buttons, panels, controls, lists and messages. Patterns: navigation, sheets and dialogs.
   Every app screen builds from these (main AR screen, on tap, start, onboarding, Explore); a block may override where it says so.
   The values follow the Unity theme (Theme.default.uss, _3.3 §4): Fraunces SemiBold + Atkinson Hyperlegible Next, the 4 / 8 pt
   grid, radius sets 6 / 12 / 20 / 30, durations 120 / 220 / 360 / 500 ms, text at 7:1, 48 pt targets, light by default.
   Audiences are token deltas on one component set (_3.5 §3.5), never a second skin.
   Unity: one theme asset (both modes) → generated USS variables; one UXML template per component. */
D.ds = {
  titles: "serif",           // the display face on page and card titles: serif (Fraunces) · body face
  text_size: 1,              // ×, every text; the visitor's own text size multiplies it (clamped 0.85-1.35)
  corner: 20,                // pt: the card radius; the set follows it (small 0.3×, medium 0.6×, sheet 1.5×); Unity Soft = 20 (6 / 12 / 20 / 30)
  density: 1,                // × inner paddings (0.75-1.25); never the touch targets
  icon_stroke: 1.5,          // px: line weight of every icon (24 pt grid)
  selected: "tint",          // a selected chip: tint (a light accent wash, a ring, bold: quiet, so the main button stays the one focal fill) · fill (solid accent)
  accent: "heritage",        // the closed accent list (ACCENTS)
  button: { size: 1, shape: "rounded" },   // the action (icon) buttons; On tap › Base elements can override
  glass: "blur",             // blur: a blurred copy of the camera behind the glass (a Unity shader) · solid: a dense tint, no blur (Unity 6.3 as is)
  depth: 1,                  // × the theme's shadows (0-1.5) (light: three levels; dark: only what floats over the camera)
  motion: "follow",          // follow the phone's Reduce Motion · full · reduced
  scheme: "standard",        // standard (calm, no overshoot) · expressive (springs that overshoot a little)
  scrim: 0.85,               // the shade under words on a photo (worst pixel 4.5:1)
  nav: { style: "raised", shape: "floating", fill: "solid", labels: "always", tabs: "4", title: "large" },
  sheet: { grabber: true, backdrop: 0.35, detent: "half" },
};
const DS_FONT = '"Atkinson Hyperlegible Next","Segoe UI",system-ui,sans-serif', DS_DISPLAY = '"Fraunces",Georgia,serif', DS_ROUNDED = '"Fredoka","Atkinson Hyperlegible Next",system-ui,sans-serif';
/* the radius set from one value (pt): small · medium · cards and panels · sheets and dialogs. 20 gives Unity's 6 / 12 / 20 / 30 */
const radiiFor = r => [Math.round(r * 0.3), Math.round(r * 0.6), Math.round(r), Math.round(r * 1.5)];
const PT = v => v / 3.9;   // pt → u (1 u = 1 % of a 390 pt phone width)
/* the look classes on a screen root: the live look, and the children's tone (visitors) */
const audCls = () => " look-" + activeLook();
/* the tokens in use: the live look's (resolved through its base), times the visitor's text size */
function dsT() { const S = D.ds; return { ...S, text: S.text_size * textScale() }; }
Object.assign(ICONS, {
  back: '<path d="M14.5 5.5L8 12l6.5 6.5"/>', chev: '<path d="M9.5 5.5L16 12l-6.5 6.5"/>', check: '<path d="M5 12.5l4.5 4.5L19 7.5"/>',
  info: '<circle cx="12" cy="12" r="8.5"/><path d="M12 11v5.5M12 7.8v.2"/>', bell: '<path d="M6.5 16.5V11a5.5 5.5 0 0111 0v5.5l1.5 2h-14z"/><path d="M10 20.5a2 2 0 004 0"/>',
  plus: '<path d="M12 5v14M5 12h14"/>', sound: '<path d="M4 9.5h3.5L12 6v12l-4.5-3.5H4z"/><path d="M15.5 9a4 4 0 010 6M18 6.5a7.5 7.5 0 010 11"/>',
});
/* the button look a component uses: the design system's, unless On tap › Base elements overrides it */
const btnLook = () => D.tap_button.use_ds === false ? D.tap_button : { ...D.tap_button, ...D.ds.button };
const reducedMotion = () => { const m = D.ds.motion; return m === "reduced" || (m === "follow" && matchMedia("(prefers-reduced-motion: reduce)").matches); };
function applyDS(root) {
  const T = dsT(), set = (k, v) => root.style.setProperty(k, v), [rs, rm, rl, rx] = radiiFor(+T.corner || 20);
  set("--font", DS_FONT); set("--tfont", T.titles === "serif" ? DS_DISPLAY : DS_FONT);
  set("--ts", T.text.toFixed(3)); set("--r-s", PT(rs)); set("--r-m", PT(rm)); set("--r-card", PT(rl)); set("--r-sheet", PT(rx));
  set("--r-chip", +T.corner < 14 ? PT(rm) : 99); set("--r-btn", +T.corner < 14 ? PT(rm) : 99);   // pills, unless the corners are sharp
  set("--sp", +T.density || 1); set("--stroke", T.icon_stroke);
  set("--dur-f", "0.12s"); set("--dur", "0.22s"); set("--dur-s", "0.36s"); set("--dur-x", "0.5s");
  set("--ease", "cubic-bezier(.33,1,.68,1)"); set("--spring", T.scheme === "expressive" ? "cubic-bezier(.34,1.56,.64,1)" : "cubic-bezier(.22,1,.36,1)");
  set("--scrim", T.scrim); set("--sdim", T.sheet.backdrop);
  set("--depth", (0.35 * T.depth).toFixed(3));
  root.classList.toggle("m-reduced", reducedMotion()); root.classList.toggle("sel-fill", T.selected === "fill");
  root.classList.toggle("glass-solid", T.glass === "solid"); root.classList.toggle("expressive", T.scheme === "expressive");
  for (const k of Object.keys(D.looks)) root.classList.toggle("look-" + k, activeLook() === k);
  set("--outline", PT(+T.outline_pt || 0)); root.classList.toggle("outlined", +T.outline_pt > 0); root.classList.toggle("sh-hard", T.shadow_style === "hard");
  set("--tfont", T.title_face === "rounded" ? DS_ROUNDED : T.title_face === "body" ? DS_FONT : (T.titles === "serif" ? DS_DISPLAY : DS_FONT));
}

/* ---------- the components (used by the kit pages and by the app's own screens) ---------- */
const kitS = () => ui.kitState ||= { tab: 1, push: null, chips: [true, false, true, false], seg: 1, sw: [true, false], slide: 0.6, search: false, sheet: null, dialog: false };
const dsBtn = (label, kind = "main", o = {}) => h("span", { class: `ds-btn ${kind}${kind === "glass" ? " g" : ""}${o.state ? " " + o.state : ""}${o.size ? " " + o.size : ""} hit`, tabindex: 0, role: "button", onclick: o.on },
  o.icon ? h("span", { class: "i" }, svg(o.icon)) : null, label);
const dsIBtn = (k, label, o = {}) => h("div", { class: "kb" }, h("div", { class: "a-rad st" + (o.state ? " " + o.state : "") + (o.more ? " more" : "") }, h("div", { class: "c g" }, svg(k))), label ? h("span", { class: "l" }, label) : null);
const dsChip = (label, on, cb, dot) => h("span", { class: "lchip g hit" + (on ? " on" : ""), tabindex: 0, role: "checkbox", "aria-checked": String(!!on), onclick: cb }, dot ? h("i", { style: `background:${dot}` }) : null, label);
const dsSeg = (opts, cur, cb) => h("div", { class: "ds-seg g", role: "radiogroup" }, ...opts.map((t, i) => h("span", { class: "hit" + (i === cur ? " on" : ""), tabindex: 0, role: "radio", "aria-checked": String(i === cur), onclick: () => cb(i) }, t)));
const dsSwitch = (on, cb) => h("span", { class: "ds-sw hit" + (on ? " on" : ""), tabindex: 0, role: "switch", "aria-checked": String(!!on), onclick: e => { e.stopPropagation(); cb?.(); } }, h("i"));
const dsRow = o => h("div", { class: "ds-row" + (o.on ? " hit" : ""), onclick: o.on },
  o.icon ? h("span", { class: "ic" }, svg(o.icon)) : o.dot ? h("span", { class: "ic", style: `background:${o.dot}` }) : null,
  h("div", { class: "tt" }, h("b", {}, o.title), o.sub ? h("small", {}, o.sub) : null),
  o.trail !== undefined ? o.trail : h("span", { class: "tr" }, svg("chev")));
const dsList = (...rows) => h("div", { class: "ds-list g" }, ...rows);
const dsPanel = (...kids) => h("div", { class: "ds-panel g" }, ...kids);
const dsBadge = t => h("span", { class: "ds-badge" }, t);
const dsSec = (title, ...kids) => h("div", { class: "ks" }, h("div", { class: "kc" }, title), ...kids);
const dsState = (el, label) => h("div", { class: "kst2" }, el, h("small", {}, label));
const dsPhoto = (p, eyebrow, title, sub) => h("div", { class: "ds-photo", style: heroStyle(p) }, h("div", { class: "scr" }), h("div", { class: "tx" }, h("small", { class: "ds-eyebrow" }, eyebrow), h("b", { class: "ds-h2" }, title), sub ? h("span", {}, sub) : null));
const dsEmpty = (icon, title, text, cta) => h("div", { class: "ds-empty" }, h("span", { class: "ei" }, svg(icon), typeof KIDS !== "undefined" ? KIDS.azuImg("point", "ei-azu") : null), h("b", { class: "ds-h2" }, title), h("p", { class: "ds-body" }, text), cta || null);
const isLand = () => { const d = DEVICES[ui.device]; return d.w > d.h; };
const poiWithPhoto = () => D.pois.find(p => p.id === "castle_hill") || D.pois.find(p => p.media?.hero) || D.pois[0];
const catName = p => CATS[p.category]?.label || "";

/* the nav bar: 3–5 top-level destinations; a rail on the leading edge in landscape */
const NAV_ALL = { wall: ["scan", "Wall"], explore: ["grid", "Explore"], tours: ["tour", "Tours"], saved: ["favourite", "Saved"], you: ["user", "You"] };
const navTabs = () => ({ 3: ["wall", "explore", "you"], 4: ["wall", "explore", "saved", "you"], 5: ["wall", "explore", "tours", "saved", "you"] })[D.ds.nav.tabs] || ["wall", "explore", "saved", "you"];
function dsNavBar(tabs, cur, pick) {
  const N = D.ds.nav, rail = isLand(), n = tabs.length;
  const nav = h("nav", { class: `ds-nav g nav-${N.shape} fill-${N.fill} lab-${N.labels} ind-${N.style}` + (rail ? " rail" : "") },
    ...tabs.map((k, i) => h("span", { class: "ds-tab hit" + (i === cur ? " on" : ""), onclick: () => pick(i) }, h("span", { class: "ti" }, svg(NAV_ALL[k][0])), h("span", { class: "tl" }, NAV_ALL[k][1]))));
  if (N.style === "raised" && !rail) navShape(nav, n, cur);
  return nav;
}
/* Raised: the bar and its bump are ONE outline (fill, rim and shadow follow it): a cap of radius 30 around the 48 pt disc,
   lifted 20 pt, joined to the bar's top edge by two concave shoulders. Unity: the same path in Painter2D (generateVisualContent). */
function navShape(nav, n, cur) {
  const N = D.ds.nav, scr = dev.SW / (dev.U / 3.9), H = 64, bar = N.shape === "bar", dock = N.shape === "dock";
  const pad = bar ? 8 : dock ? 10 : 16, tw = dock ? 62 : null;
  const W = bar ? scr : dock ? n * tw + (n - 1) * 4 + 2 * pad : Math.min(scr - 32, 420);
  const cxOf = i => dock ? pad + i * (tw + 4) + tw / 2 : pad + (i + 0.5) * (W - 2 * pad) / n;
  const c0 = cxOf(0), cx = cxOf(cur), R = 30, cy = 10, ph = 40 * Math.PI / 180;   // the cap: centre 10 pt under the bar's top edge
  const r = bar ? 0 : Math.max(8, Math.min(dock ? 30 : 16, c0 - 31)), hw = Math.min(40, c0 - r);
  const px = R * Math.cos(ph), py = cy - R * Math.sin(ph), sh = hw - px;   // where the shoulder meets the cap
  const f = v => v.toFixed(2), tx = 0.643, ty = -0.766;
  const top = `L${f(cx - hw)},0 C${f(cx - hw + sh * 0.55)},0 ${f(cx - px - tx * sh * 0.45)},${f(py - ty * sh * 0.45)} ${f(cx - px)},${f(py)} A${R},${R} 0 0 1 ${f(cx + px)},${f(py)} C${f(cx + px + tx * sh * 0.45)},${f(py - ty * sh * 0.45)} ${f(cx + hw - sh * 0.55)},0 ${f(cx + hw)},0`;
  const d = r ? `M${r},0 ${top} L${f(W - r)},0 A${r},${r} 0 0 1 ${f(W)},${r} L${f(W)},${H - r} A${r},${r} 0 0 1 ${f(W - r)},${H} L${r},${H} A${r},${r} 0 0 1 0,${H - r} L0,${r} A${r},${r} 0 0 1 ${r},0 Z`
    : `M0,0 ${top} L${f(W)},0 L${f(W)},${H + 26} L0,${H + 26} Z`;
  const t = document.createElement("template");
  t.innerHTML = `<svg class="ds-navshape" viewBox="0 -24 ${f(W)} ${bar ? H + 50 : H + 24}" preserveAspectRatio="none" aria-hidden="true"><path d="${d}"/></svg>`;
  nav.classList.add("shaped"); nav.style.width = `calc(${f(W)} * var(--P))`; nav.prepend(t.content.firstChild);
}

/* ---------- the kit pages: one per subsection of App › 1 Design system ---------- */
function kitRoot(extra = "") {
  const B = btnLook();
  const root = h("div", { class: `tapc kit bg-${ui.kitBg || "dim"}${audCls()}${B.shape === "rounded" ? " b-round" : ""}${B.surface === "solid" ? " b-solid" : ""}${isLand() ? " land" : ""} ${DEVICES[ui.device].kind} ${extra}`, style: `--u:${dev.U}px;--bk:${B.size}` });
  applyGlass(root); return root;
}
function kitHead(title, sub) {
  return h("div", { class: "kh" }, h("div", { class: "kht" }, h("small", { class: "keb" }, "Design system"), h("b", {}, title), h("span", {}, sub)),
    h("div", { class: "kbg ds-seg g" }, ...[["dim", "Dimmed"], ["tiles", "Tiles"], ["plain", "Plain"]].map(([k, t]) => h("span", { class: "hit" + ((ui.kitBg || "dim") === k ? " on" : ""), onclick: () => { ui.kitBg = k; renderPhone(); } }, t))));
}
const rp = () => renderPhone();
const KIT_PAGES = {
  look: ["Overview", "the whole look at a glance", () => { const p = D.pois[0], K = kitS(); return [
    dsPanel(h("small", { class: "ds-eyebrow" }, "Religious · lost in 1755"), h("b", { class: "ds-h1" }, p.name), h("p", { class: "ds-body" }, "A baroque church on the river square, lost in the 1755 earthquake."),
      h("div", { class: "krow sm" }, dsChip("Architecture", K.chips[0], () => { K.chips[0] = !K.chips[0]; rp(); }), dsChip("1755", K.chips[1], () => { K.chips[1] = !K.chips[1]; rp(); })),
      h("div", { class: "ds-btns" }, dsBtn("Open the story", "main", { icon: "card" }), dsBtn("Save", "tonal", { icon: "favourite" }))),
    h("div", { class: "krow" }, dsIBtn("model_wall", "3D here"), dsIBtn("pano", "360°"), dsIBtn("then_now", "Then & now"), dsIBtn("more", "+2", { more: true })),
    dsList(dsRow({ icon: "tour", title: "Tours", sub: "3 walks, 20–40 min", on: () => {} }), dsRow({ icon: "sound", title: "Sound", trail: dsSwitch(K.sw[0], () => { K.sw[0] = !K.sw[0]; rp(); }) })),
    dsNavBar(navTabs(), Math.min(K.tab, navTabs().length - 1), i => { K.tab = i; K.push = null; rp(); }),
  ]; }],
  colour: ["Colour", "roles, in the current design", () => { const T = D.themes[appearance()]; return [
    dsSec("Roles", h("div", { class: "ksw" }, ...[["Surface", T.glass.color], ["Text", T.text.main], ["Secondary", T.text.soft], ["Border", rgba(T.border.color, T.border.opacity), `${T.border.color} · ${Math.round(T.border.opacity * 100)} %`], ["Accent", T.accent], ["On accent", T.accent_text], ["Button", T.buttons.color], ["Icon", T.buttons.icon], ["Panel", T.sheet.color], ["Panel text", T.sheet.text], ["Label", T.labels.color], ["Label text", T.labels.text]]
      .map(([n, c, t]) => h("span", { class: "sw" }, h("i", { style: `background:${c}` }), h("b", {}, n), h("small", {}, t || c))))),
    dsSec("Icon buttons over the camera", h("div", { class: "krow lp" }, dsIBtn("card", "Card"), dsIBtn("model_wall", "3D here"), dsIBtn("pano", "360°"), dsIBtn("map", "Map"))),
    dsSec("On glass", dsPanel(h("b", { class: "ds-h2" }, "Text on glass"), h("p", { class: "ds-body soft" }, "Secondary text explains; accent marks what is selected."), h("div", { class: "krow sm" }, dsChip("Selected", true), dsChip("Not selected", false)))),
    dsSec("On a panel", h("div", { class: "ds-solid" }, h("b", { class: "ds-h2" }, "Text on a panel"), h("p", { class: "ds-body soft" }, "Cards, sheets and dialogs use the panel colours. The main button is the accent: one per view."), dsBtn("Main button", "main"))),
    activeLook() === "playful" ? dsSec("Accent", h("p", { class: "knote" }, "Playful pins its accent: cobalt in light, antimony yellow in dark. The closed list applies to Premium.")) : dsSec("Accent · the closed list", h("div", { class: "kacc" }, ...Object.entries(ACCENTS).map(([k, A]) => { const [c, on] = A[appearance()]; return h("span", { class: "kac hit" + (D.ds.accent === k ? " on" : ""), onclick: () => { snap(); D.ds.accent = k; setAccent(k); renderEditor(); rp(); } }, h("i", { style: `background:${c};color:${on}` }, "Aa"), h("small", {}, A.name)); }))),
  ]; }],
  type: ["Type", "two faces, seven sizes", () => { const T = dsT(), pt = v => Math.round(v * T.text), fam = T.title_face === "rounded" ? "Fredoka SemiBold" : T.title_face === "body" || T.titles !== "serif" ? "Atkinson Hyperlegible Next" : "Fraunces SemiBold";
    return [dsPanel(...[["Display", "ds-d", "Lisbon, 1700", 28, fam], ["Title", "ds-h1", "Twin-tower church", 24, fam], ["Heading", "ds-h2", "Before the earthquake", 20, T.title_face === "rounded" ? "Fredoka SemiBold" : "Atkinson Bold"], ["Body", "ds-body", "A baroque church on the river square, lost in the 1755 earthquake.", 16, "Atkinson Regular"], ["Label", "ds-lbl", "Open the story", 16, "Atkinson Bold"], ["Caption", "ds-cap", "3 walks · 20–40 min", 14, "Atkinson Regular"], ["Eyebrow", "ds-eyebrow", "Religious · lost in 1755", 13, T.title_face === "rounded" ? "Atkinson Bold, sentence case" : "Atkinson Bold, capitals"]]
      .map(([n, c, t, v, f]) => h("div", { class: "ktr" }, h("small", { class: "kmeta" }, `${n} · ${pt(v)} pt · ${f}`), h("div", { class: c }, t)))), h("p", { class: "knote" }, `Text ×${T.text.toFixed(2)} · nothing under 13 pt · every text 7:1 on its ground · the visitor's own text size multiplies it`)]; }],
  space: ["Spacing and shape", "the 4 / 8 pt grid, corners, icons, touch", () => { const T = dsT(), R = radiiFor(+T.corner); return [
    dsSec("Spacing · 4 / 8 pt", dsPanel(h("div", { class: "kspace" }, ...[4, 8, 12, 16, 24, 32, 48].map(v => h("div", { class: "ksp" }, h("i", { style: `width:calc(${PT(v)} * var(--u));height:calc(${PT(v)} * var(--u))` }), h("small", {}, String(v))))))),
    dsSec("Density", dsList(dsRow({ icon: "tour", title: "Tours", sub: "3 walks" }), dsRow({ icon: "quiz", title: "Games", sub: "2 games" }))),
    dsSec(`Corners · ${T.corner} pt`, h("div", { class: "kcorn" }, ...[["Small", "var(--r-s)", R[0]], ["Medium", "var(--r-m)", R[1]], ["Card", "var(--r-card)", R[2]], ["Sheet", "var(--r-sheet)", R[3]]].map(([n, r, v]) => h("div", { class: "kc1" }, h("i", { class: "g", style: n === "Sheet" ? `border-radius:calc(${r} * var(--u)) calc(${r} * var(--u)) 0 0` : `border-radius:calc(${r} * var(--u))` }), h("small", {}, `${n} ${v}`))))),
    dsSec("Icons", h("div", { class: "kicons g" }, ...["search", "grid", "tour", "favourite", "share", "user", "sound", "bell"].map(k => h("span", {}, svg(k))))),
    dsSec("Touch target", h("div", { class: "krow" }, h("div", { class: "ktouch" }, dsIBtn("favourite")), h("p", { class: "knote" }, "Every control answers a touch of at least 48 × 48 pt, even when it looks smaller; 8 pt between two."))),
  ]; }],
  depth: ["Depth and motion", "how surfaces lift and move", () => [
    dsSec("Elevation", h("div", { class: "klev" }, ...[["0", "The wall", "camera"], ["1", "Floating", "tags, bars, buttons"], ["2", "Raised", "cards, panels"], ["3", "Overlay", "sheets, dialogs"]].map(([l, n, s]) => h("div", { class: `kl l${l}` + (l === "0" ? "" : " g") }, h("b", {}, n), h("small", {}, s))))),
    dsSec("Motion", dsPanel(...[["Fast", "a tap answers", 120, "--dur-f"], ["Standard", "panels, chips", 220, "--dur"], ["Slow", "sheets, push", 360, "--dur-s"], ["Slower", "the camera turns", 500, "--dur-x"]].map(([n, s, ms, v]) => h("div", { class: "kmo" }, h("div", { class: "kmt" }, h("b", {}, n), h("small", {}, `${s} · ${ms} ms`)), h("div", { class: "ktrack" }, h("i", { style: `animation-duration:calc(var(${v}) * 4);animation-timing-function:var(--spring)` })))),
      h("p", { class: "knote in2" }, reducedMotion() ? "Reduced: a short cross-fade, nothing moves." : dsT().scheme === "expressive" ? "Expressive: springs overshoot a little, then settle." : "Standard: ease-out, nothing overshoots. Reduce Motion turns every move into a fade."))),
  ]],
  buttons: ["Buttons", "main, tonal, glass, icon, text", () => [
    dsSec("Main · one per screen", h("div", { class: "krow" }, dsState(dsBtn("Open", "main"), "Normal"), dsState(dsBtn("Open", "main", { state: "pressed" }), "Pressed"), dsState(dsBtn("Open", "main", { state: "disabled" }), "Disabled"), dsState(dsBtn("Open", "main", { state: "focus" }), "Focused"))),
    dsSec("Tonal and glass · the others", h("div", { class: "krow" }, dsState(dsBtn("Save", "tonal", { icon: "favourite" }), "Tonal"), dsState(dsBtn("Map", "glass", { icon: "map" }), "Glass"), dsState(dsBtn("Map", "glass", { icon: "map", state: "pressed" }), "Pressed"))),
    dsSec("Icon buttons · actions", h("div", { class: "krow" }, dsIBtn("card", "Normal"), dsIBtn("card", "Pressed", { state: "pressed" }), dsIBtn("favourite", "Selected", { state: "on" }), dsIBtn("play", "Disabled", { state: "off" }))),
    dsSec("Text", h("div", { class: "krow" }, dsBtn("See all", "text"), dsBtn("Skip", "text soft"))),
    dsSec("Sizes", h("div", { class: "krow" }, dsBtn("Small", "main", { size: "s" }), dsBtn("Regular", "main"), dsBtn("Large", "main", { size: "l" }))),
  ]],
  panels: ["Panels", "glass, solid, photo, buttons on panels", () => { const p = poiWithPhoto(); return [
    dsSec("Glass panel · floats over the camera", dsPanel(h("b", { class: "ds-h2" }, "Glass"), h("p", { class: "ds-body soft" }, dsT().glass === "solid" ? "A dense tint (no blur) for anything that floats over the camera." : "Blurs the wall behind; for anything that floats over the camera."))),
    dsSec(activeLook() === "playful" ? "Photo card · a polaroid" : "Photo card · words on a shade", dsPhoto(p, catName(p), p.name, "Tap to open its story")),
    dsSec("Buttons on a panel", h("div", { class: "ds-solid" }, h("div", { class: "khd" }, h("b", { class: "ds-h2" }, "Twin-tower church"), h("span", { class: "ds-x hit" }, svg("close"))),
      h("div", { class: "tiles" }, ...["model_wall", "pano", "map"].map(k => h("div", { class: "tile" }, h("span", { class: "i" }, svg(k)), actionLabel(k)))),
      h("div", { class: "ds-btns" }, dsBtn("Open the story", "main"), dsBtn("Share", "tonal", { icon: "share" })),
      h("p", { class: "knote in" }, "On a panel, secondary buttons turn tonal: glass on glass would vanish."))),
  ]; }],
  controls: ["Controls", "search, chips, segments, switches, slider", () => { const K = kitS(), cats = Object.values(CATS).slice(0, 4); return [
    dsSec("Search", h("div", { class: "msearch g pill hit" + (K.search ? " focus" : ""), onclick: () => { K.search = !K.search; rp(); } }, h("span", { class: "mi" }, svg("search")),
      K.search ? h("span", { class: "ph1 typed" }, "Castle", h("i", { class: "caret" })) : h("span", { class: "ph1" }, "Search the wall"), K.search ? h("span", { class: "ds-cancel" }, "Cancel") : h("span", { class: "mi" }, svg("mic")))),
    dsSec("Chips · filter", h("div", { class: "kchips" }, ...cats.map((c, i) => dsChip(c.label, K.chips[i], () => { K.chips[i] = !K.chips[i]; rp(); }, c.color)))),
    dsSec("Segmented · one of a few", dsSeg(["Then", "Both", "Now"], K.seg, i => { K.seg = i; rp(); })),
    dsSec("Switches", dsList(dsRow({ icon: "sound", title: "Sound on the wall", trail: dsSwitch(K.sw[0], () => { K.sw[0] = !K.sw[0]; rp(); }) }), dsRow({ icon: "bell", title: "Tips from Azu", trail: dsSwitch(K.sw[1], () => { K.sw[1] = !K.sw[1]; rp(); }) }))),
    dsSec("Slider", dsPanel(h("div", { class: "khd" }, h("b", { class: "ds-lbl" }, "Then ⇄ now"), h("b", { class: "ds-lbl num" }, Math.round(K.slide * 100) + "%")),
      h("input", { class: "ds-range hit", type: "range", min: 0, max: 1, step: 0.01, value: K.slide, style: `--v:${K.slide * 100}%`, oninput: e => { K.slide = +e.target.value; e.target.style.setProperty("--v", K.slide * 100 + "%"); e.target.closest(".ds-panel").querySelector(".num").textContent = Math.round(K.slide * 100) + "%"; } }))),
  ]; }],
  lists: ["Lists and messages", "rows, badges, toasts, progress", () => { const K = kitS(); return [
    dsSec("List", dsList(dsRow({ icon: "tour", title: "Tours", sub: "3 walks · 20–40 min", on: () => {} }), dsRow({ icon: "quiz", title: "Games", sub: "Find the 7 bells", trail: h("span", { class: "tr" }, dsBadge("New"), svg("chev")), on: () => {} }),
      dsRow({ icon: "play", title: "Moments on the wall", sub: "The 1755 earthquake", on: () => {} }), dsRow({ icon: "sound", title: "Sound", trail: dsSwitch(K.sw[0], () => { K.sw[0] = !K.sw[0]; rp(); }) }))),
    dsSec("Messages", h("div", { class: "ktoast g" }, h("span", { class: "mi" }, svg("check")), "Saved to your favourites"),
      h("div", { class: "ds-banner g" }, h("span", { class: "mi" }, svg("info")), h("span", { class: "bt" }, "Point your phone at the tiles"), dsBtn("Help", "text"))),
    dsSec("Badges", h("div", { class: "krow" }, h("div", { class: "kbdg" }, dsIBtn("bell"), h("span", { class: "ds-dot" }, "3")), dsBadge("New"), dsBadge("12"), h("span", { class: "ds-badge soft" }, "Seen"))),
    dsSec("Loading", dsList(...[0, 1].map(() => h("div", { class: "ds-skel" }, h("i"), h("div", {}, h("b"), h("small")))))),
    dsSec("Progress", dsPanel(h("div", { class: "khd" }, h("b", { class: "ds-lbl" }, "Getting the wall ready"), h("b", { class: "ds-lbl num" }, "64%")), h("div", { class: "ds-prog" }, h("i", { style: "width:64%" })), h("p", { class: "ds-body soft shimmer" }, "Painting the tiles…"))),
  ]; }],
  nav: ["Navigation", "nav bar, top bar, push and back", null, () => navDemo()],
  sheets: ["Sheets and dialogs", "bottom sheet, dialog", null, () => sheetsDemo()],
};
/* landscape: the sections flow into two columns (Unity: two column VisualElements in a row), so the important rows sit above the fold.
   A number: how many sections go in the first column; a pair of lists: the section order of each column (the most telling first).
   A page left out stays one column (Type: its panel goes 2-up). */
const KIT_LAND_SPLIT = { look: 1, colour: [[0], [2, 3, 1, 4]], space: [[0, 2], [3, 1, 4]], depth: 1, buttons: 2, panels: [[1, 0], [2]], controls: 3, lists: [[0], [1, 4, 2, 3]] };
/* the page end: a "More" cue while there is more below (Unity: the ScrollView's scroller value vs its range) */
const kitEnd = (root, pg) => root.classList.toggle("kend", pg.scrollTop + pg.clientHeight >= pg.scrollHeight - parseFloat(getComputedStyle(pg).paddingBottom) + 12);   // past the last row (the page's own end padding)
/* tablet landscape has the height for more: move sections from the end of the taller column to the shorter one while that evens them out
   (Unity: the same move on GeometryChangedEvent) */
function kitBalance(pg) {
  const [a, b] = pg.querySelectorAll(".kcol"); if (!a || !b) return;
  for (let i = 0; i < 6; i++) {
    const [hi, lo] = a.offsetHeight >= b.offsetHeight ? [a, b] : [b, a], last = hi.lastElementChild;
    if (!last || hi.children.length < 2 || lo.offsetHeight + last.offsetHeight >= hi.offsetHeight) return;
    lo.append(last);
  }
}
function uiKit() {
  const page = ui.compPreview?.page || "look", P = KIT_PAGES[page] || KIT_PAGES.look;
  if (P[3]) { const root = kitRoot("full pg-" + page); root.append(P[3]()); return root; }
  const root = kitRoot("pg-" + page), items = P[2](), n = isLand() ? KIT_LAND_SPLIT[page] : 0;
  const nav = page === "look" ? items.pop() : null;   // Overview: the nav sits where the app puts it: pinned at the bottom (portrait), a rail on the leading edge (landscape)
  const cols = !n ? null : Array.isArray(n) ? n.map(ix => ix.map(i => items[i])) : [items.slice(0, n), items.slice(n)];
  const body = cols ? [h("div", { class: "kcols" }, ...cols.map(c => h("div", { class: "kcol" }, ...c)))] : items;
  const pg = h("div", { class: "kitp", onscroll: () => kitEnd(root, pg) }, kitHead(P[0], P[1]), ...body);
  /* the page end: a ramp from clear to the backdrop (Unity: a ramp sprite) so a row never shows sliced, and a cue that the page scrolls */
  root.append(pg, h("div", { class: "kfoot", "aria-hidden": "true" }), h("span", { class: "kmore hit", role: "button", tabindex: 0, onclick: () => pg.scrollBy({ top: pg.clientHeight * 0.7, behavior: "smooth" }) }, h("span", { class: "i" }, svg("chev")), "More"));
  if (nav) { nav.classList.add(isLand() ? "krail" : "kbar"); root.append(nav); root.classList.add(isLand() ? "has-rail" : "has-bar"); }
  requestAnimationFrame(() => { if (cols && DEVICES[ui.device].kind === "tablet") kitBalance(pg); kitEnd(root, pg); });
  return root;
}

/* Navigation: a small app made of the patterns — nav bar between destinations, a top bar, push into a detail and back */
function navDemo() {
  const K = kitS(), N = D.ds.nav, tabs = navTabs(), land = isLand(); K.tab = Math.min(K.tab, tabs.length - 1);
  const k = tabs[K.tab], p = poiWithPhoto(), push = t => { K.push = t; rp(); };
  const bodies = {
    wall: () => [h("div", { class: "kwall" }, h("span", { class: "lchip g" }, h("i", { style: "background:var(--acc)" }), "The camera view: the wall, its POIs, Azu"))],
    explore: () => [dsList(dsRow({ icon: "tour", title: "Tours", sub: "3 walks · 20–40 min", on: () => push("Tours") }), dsRow({ icon: "quiz", title: "Games", sub: "Find the 7 bells", trail: h("span", { class: "tr" }, dsBadge("New"), svg("chev")), on: () => push("Games") }),
      dsRow({ icon: "play", title: "Moments on the wall", sub: "The 1755 earthquake", on: () => push("Moments") }), dsRow({ icon: "then_now", title: "Wall layers", sub: "Today's map, then and now", on: () => push("Wall layers") }))],
    tours: () => D.experiences.tours.slice(0, 2).map((t, i) => h("div", { class: "hit", onclick: () => push(t.name) }, dsPhoto(D.pois[(i * 3 + 1) % D.pois.length], `${t.theme} · ${t.minutes} min`, t.name))),
    saved: () => [dsEmpty("favourite", "Nothing saved yet", "Tap the heart on a building to keep it here.", dsBtn("Find a building", "main", { on: () => { K.tab = 0; rp(); } }))],
    you: () => [dsList(dsRow({ icon: "user", title: "Explorer look", sub: "Change in onboarding", on: () => push("Look") }), dsRow({ icon: "sound", title: "Sound", trail: dsSwitch(K.sw[0], () => { K.sw[0] = !K.sw[0]; rp(); }) }), dsRow({ icon: "bell", title: "Tips from Azu", trail: dsSwitch(K.sw[1], () => { K.sw[1] = !K.sw[1]; rp(); }) }))],
  };
  const title = K.push || NAV_ALL[k][1], onWall = k === "wall" && !K.push;
  const top = onWall ? null : h("div", { class: "ds-top" + (K.push || N.title === "inline" ? " inline" : " large") },
    K.push ? h("span", { class: "ds-back hit", onclick: () => { K.push = null; rp(); } }, svg("back"), h("span", {}, NAV_ALL[k][1])) : h("span"),
    h("b", { class: K.push || N.title === "inline" ? "ds-lbl" : "ds-h1" }, title), h("span", { class: "ds-x hit" }, svg(K.push ? "share" : "search")));
  const detail = () => [dsPhoto(p, CATS[p.category]?.label || "", p.name), h("p", { class: "ds-body" }, p.summary + " Tap “Show it on the wall” to find it there."),
    h("div", { class: "ds-btns" }, dsBtn("Show it on the wall", "main", { icon: "scan", on: () => { K.push = null; K.tab = 0; rp(); } }), dsBtn("Save", "tonal", { icon: "favourite" }))];
  const view = h("div", { class: "ds-scroll ds-view" + (K.push ? " pushed" : "") }, ...(K.push ? detail() : bodies[k]()));
  const nav = dsNavBar(tabs, K.tab, i => { K.tab = i; K.push = null; rp(); });
  return h("div", { class: "ds-app" + (onWall ? " onwall" : "") + (land ? " land" : "") }, land ? nav : null, h("div", { class: "ds-main" }, top, view), land ? null : nav);
}
/* Sheets and dialogs: a bottom sheet with three heights (tap the grabber), a dialog over a scrim */
function sheetsDemo() {
  /* a phone in landscape is a compact height: like iOS, the half stop is not offered there and the sheet opens full (a column on the trailing side) */
  const K = kitS(), S = D.ds.sheet, det0 = K.sheet || S.detent, det = isLand() && DEVICES[ui.device].kind === "phone" && det0 === "half" ? "full" : det0, p = D.pois[0], next = { peek: "half", half: "full", full: "peek" };
  const sheet = h("div", { class: `ds-sheet d-${det}` + (textScale() > 1.1 ? " big" : "") },   // big: text L, the tiles go compact
    S.grabber ? h("span", { class: "grab hit", title: "Tap: change the height", onclick: () => { K.sheet = next[det]; rp(); } }, h("i")) : h("span", { class: "grab0" }),
    h("div", { class: "khd" }, h("div", {}, h("small", { class: "ds-eyebrow" }, "Religious · lost in 1755"), h("b", { class: "ds-h1" }, p.name)), h("span", { class: "ds-x hit", onclick: () => { K.sheet = "peek"; rp(); } }, svg("close"))),
    h("div", { class: "ds-scroll sbody" }, h("p", { class: "ds-body soft" }, "A baroque church on the river square, lost in the 1755 earthquake."),
      h("div", { class: "tiles" }, ...["model_wall", "pano", "map"].map(k => h("div", { class: "tile" }, h("span", { class: "i" }, svg(k)), actionLabel(k))))),
    /* the actions are pinned under the scrolling body (Unity: a sibling after the ScrollView), so they never scroll away or cover the tiles */
    h("div", { class: "ds-btns sfoot" }, dsBtn("Leave the tour", "tonal", { on: () => { K.dialog = true; rp(); } }), dsBtn("Open the story", "main")));
  const out = [h("div", { class: "ds-trig" }, dsBtn(det === "peek" ? "Open the sheet" : "Lower the sheet", "glass", { on: () => { K.sheet = det === "peek" ? "half" : "peek"; rp(); } }), dsBtn("Show a dialog", "glass", { on: () => { K.dialog = true; rp(); } })),
    h("div", { class: `ds-dim d-${det}` }), sheet];
  if (K.dialog) out.push(h("div", { class: "ds-scrim", onclick: () => { K.dialog = false; rp(); } }), h("div", { class: "ds-dialog" }, h("b", { class: "ds-h2" }, "Leave the tour?"), h("p", { class: "ds-body soft" }, "You have seen 4 of 9 stops. You can come back to it from Explore."),
    h("div", { class: "ds-btns" }, dsBtn("Stay", "tonal", { on: () => { K.dialog = false; rp(); } }), dsBtn("Leave", "main", { on: () => { K.dialog = false; rp(); } }))));
  return h("div", { class: "ds-sheets" }, ...out);
}

/* the app's own screens, built from the components: Start, Onboarding, the Explore menu */
function dsScreenRoot(cls) { const root = kitRoot(cls + " scrn"); root.classList.remove("kit", "bg-dim", "bg-tiles", "bg-plain"); return root; }
function startScreen(go) {
  const A = D.app, kids = forKids(), root = dsScreenRoot("ds-start");
  root.append(h("div", { class: "ds-hero", style: `background-image:url(${SRC})` }), h("div", { class: "ds-heroshade" }),
    h("div", { class: "ds-center st" }, h("small", { class: "ds-eyebrow" }, "Lisbon · before 1755"), h("b", { class: "ds-hero-t" }, "TileStories"),
      h("p", { class: "ds-body" }, "The city before the earthquake, on twenty metres of tiles."),
      h("div", { class: "ds-tiles" }, ...Array.from({ length: 12 }, (_, i) => h("i", { style: `animation-delay:calc(${i} * var(--dur-f))` }))),
      A.start.loading_lines ? h("p", { class: "ds-cap ld" }, kids ? "Ready? Don't bump your head on the castle!" : "Painting the tiles… mind your head on the 3D.") : null,
      h("div", { class: "ds-go" }, dsBtn("Start", "main", { size: "l", on: go }))));
  return root;
}
/* onboardingScreen: see js/34_profiles.js (v18: who is visiting, how long, interests) */
function exploreScreen(close) {
  const A = D.app, X = A.explore, Ex = D.experiences, root = dsScreenRoot("ds-explore");
  const grp = (t, rows) => [h("div", { class: "ds-sh" }, t), dsList(...rows)];
  root.append(h("div", { class: "ds-dim d-full" }), h("div", { class: "ds-sheet d-full sheet-ex" }, h("span", { class: "grab" }, h("i")),
    h("div", { class: "khd" }, h("b", { class: "ds-h1" }, "Explore the wall"), h("span", { class: "ds-x hit", onclick: close }, svg("close"))),
    h("div", { class: "ds-scroll sbody" }, activeLook() === "playful" && typeof KF !== "undefined" ? KF.entry(() => { ui.appStep = "album"; renderPhone(); }) : null,
      ...(() => {   // Visitors › games first puts games on top; Time › tour length hides the longer tours
        const fit = Ex.tours.filter(t => t.minutes <= timeRow().tour_max), tours = X.tours && fit.length ? grp("Tours", fit.map(t => dsRow({ icon: KIDS.rowIcon(t.key, "tour"), title: t.name, sub: `${t.theme} · ${t.minutes} min`, on: () => {} }))) : [];
        const games = X.games ? grp("Games", Ex.games.map(g => dsRow({ icon: KIDS.rowIcon(g.key, "quiz"), title: g.name, sub: g.rule ? g.rule[0].toUpperCase() + g.rule.slice(1) : g.type, on: () => {} }))) : [];
        return visitorRow().games_first ? [...games, ...tours] : [...tours, ...games]; })(),
      ...(X.moments ? grp("Moments on the wall", Ex.moments.map(m => dsRow({ icon: KIDS.rowIcon(m.key, "play"), title: m.name, sub: "Plays over the whole wall", on: () => {} }))) : []),
      ...(X.layers ? grp("Wall layers", [dsRow({ icon: "then_now", title: "Today's map", sub: "later", on: () => {} })]) : []),
      ...(A.main.buttons.profile === "explore" ? grp("You", [dsRow({ icon: "user", title: "Your visit", sub: "Saved places, what you found, Settings", on: () => { ui.appStep = "you"; renderPhone(); } })]) : []))));
  return root;
}

/* ---------- the editor: App › 1 Design system ---------- */
const kitDemo = page => ({ step: "tapped", preview: { type: "kit", page } });
const live = f => id => { const el = f(id); el.addEventListener("input", rp); return el; };
function designBody() {
  const A = D.app, S = D.ds, N = S.nav, SH = S.sheet, T = dsT();
  const sub = (page, title, subt, body) => fold("ds_" + page, title, { color: "#C9B7FF", screen: "app", step: "tapped", sub: subt, demo: kitDemo(page) }, body);
  const head = (t, s) => h("div", { class: "grp" }, t, s ? h("span", {}, s) : null);
  return [
    h("p", { class: "guide" }, "The app's own UI kit: foundations (tokens), components, and the patterns built from them. Every screen uses it; a block overrides only where it says so. The values follow the Unity theme (Fraunces + Atkinson, the 4 / 8 pt grid, text at 7:1, 48 pt targets). Each subsection opens its page on the phone; every change shows there at once."),
    head("Foundations", "tokens"),
    sub("look", "Look and mode", `${D.looks[activeLook()].name} · ${appearance()} · text ${D.profile.text.toUpperCase()}`, () => [
      h("p", { class: "guide" }, "Every row in the App tab is saved into the look and the mode chosen in the profile bar above (Look · Light / Dark). A field marked “from Premium” is inherited: change it to give this look its own value, or reset it. Colours are per mode; everything else is shared by light and dark."),
      head("This look's character", "tokens a look may change"),
      ...row("Titles face", fSelect(S, "title_face", [["display", "Display (Fraunces / the Titles row)"], ["rounded", "Rounded (Fredoka)"], ["body", "Body face (Atkinson)"]], rp), "The face of page and card titles. Body text stays Atkinson Hyperlegible Next in every look, for legibility."),
      ...row("Outline", live(fRange(S, "outline_pt", 0, 3, 0.25, " pt")), "An ink outline on panels, buttons and chips (the sticker look). 0 = none (Premium)."),
      ...row("Shadow style", fSelect(S, "shadow_style", [["soft", "Soft (blurred, Premium)"], ["hard", "Hard offset (cartoon: 0 3 pt, no blur)"]], rp), "Hard: a solid offset shadow under raised parts; a press moves the part down onto it."),
      ...row("Markers", fSelect(S, "marker_preset", [["by_level", "By hierarchy level (Global Scene › Levels)"], ...D.point_styles.map(p => [p.key, p.name])], () => { ui.compPreview = null; ui.appStep = "main"; renderPhone(); }), "A look can dress every POI marker in one point style (Playful: Storybook)."),
      head("Light and dark", "the app's default; the visitor can change it in Settings"),
      ...row("In the app", fSelect(A, "appearance", [["light", "Light (default: sunlight first)"], ["follow", "Follow the phone"], ["dark", "Always dark"]], () => { ui.forceMode = null; renderEditor(); rp(); }), "Light by default: legible in the sun first (rule D1). The camera stays the same; only the app surfaces change. Preview and edit the other mode with the profile bar.")]),
    sub("colour", "Colour", `${ACCENTS[S.accent]?.name || "custom"} accent · ${D.looks[activeLook()].name} ${appearance()}`, () => [
      ...(activeLook() === "playful" ? [] : row("Accent", fSelect(S, "accent", Object.entries(ACCENTS).map(([k, a]) => [k, a.name]), () => { setAccent(S.accent); rp(); }), "A closed list: each accent is a measured light / dark pair, so choosing one sets this look's accent in BOTH modes at once (7:1 on the surfaces; the Unity rule: no free accent colour). It fills the main button, selected chips, segments, tabs and switches.")),
      ...row("Glass", fSelect(S, "glass", [["blur", "Blurred camera behind (Unity shader)"], ["solid", "Dense tint, no blur (Unity 6.3 as is)"]], rp), "Unity 6.3 has no backdrop blur. Blur: a blurred, luminance-shifted copy of the camera behind each glass part (one small shader). Solid: the tint alone, made dense enough for 7:1 (light 0.84, dark 0.98)."),
      h("p", { class: "guide" }, "The colour roles of the look and mode in the profile bar (the Unity theme's values). Tags and wall labels have their own rows, in On tap › Tag components and Main AR screen › Wall labels."),
      colourFold("Colour roles", { only: DS_COLOUR_GROUPS, demo: kitDemo("colour") })]),
    sub("type", "Type", `${T.titles === "serif" ? "Fraunces titles" : "Atkinson titles"} · ×${S.text_size}`, () => [
      ...row("Titles", fSelect(S, "titles", [["serif", "Fraunces SemiBold (display face)"], ["sans", "Atkinson Bold (body face)"]], rp), "Body text is always Atkinson Hyperlegible Next (400 / 500 / 700), the faces the Unity project ships."),
      ...row("Text size", live(fRange(S, "text_size", 1, 1.2, 0.05, "×")), "Every text in this look; the visitor's text size (Small 0.9 · Default 1 · Large 1.2, or the phone's) multiplies it. Never below 1: the smallest roles (eyebrow, nav label) are already at the 13 pt floor.")]),
    sub("space", "Spacing and shape", `corners ${S.corner} pt · density ×${S.density}`, () => [
      ...row("Corners", live(fRange(S, "corner", 4, 32, 1, " pt")), "The card radius; the rest of the set follows it: small 0.3×, medium 0.6×, sheets and dialogs 1.5× (20 = Unity's 6 / 12 / 20 / 30). Under 14 pt, buttons and chips stop being pills."),
      ...row("Density", live(fRange(S, "density", 0.75, 1.25, 0.05, "×")), "Inner paddings and gaps (rows, panels, lists); touch targets stay 48 pt."),
      ...row("Icon line", live(fRange(S, "icon_stroke", 1, 2.5, 0.25, " px")), "The weight of every icon line on the 24 pt grid; 1.5 matches Atkinson Regular.")]),
    sub("depth", "Depth and motion", `${S.motion} · ${S.scheme}`, () => [
      ...row("Shadows", live(fRange(S, "depth", 0, 1.5, 0.05, "×")), "Light: three levels (floating 0.20, raised 0.32, overlay 0.40) times this. Dark: no shadows on panels (depth from the surface tone and a rim); only what floats over the camera keeps a soft one."),
      ...row("Motion", fSelect(S, "motion", [["follow", "Follow the phone (Reduce Motion)"], ["full", "Always full"], ["reduced", "Always reduced (fades only)"]], rp)),
      ...row("Motion style", fSelect(S, "scheme", [["standard", "Standard (ease-out, calm)"], ["expressive", "Expressive (springs)"]], rp), "Durations are fixed tokens: 120 / 220 / 360 / 500 ms.")]),
    head("Components"),
    sub("buttons", "Buttons", `48 pt · action buttons ${S.button.shape}`, () => [
      ...row("Action button size", live(fRange(S.button, "size", 0.85, 1.25, 0.05, "×"))),
      ...row("Action button shape", fSelect(S.button, "shape", [["rounded", "Rounded square"], ["circle", "Circle"]], rp)),
      h("p", { class: "guide" }, "Main (the accent, one per view) · tonal (secondary on panels) · glass (secondary over the camera) · outline · text · icon buttons (the actions; On tap › Base elements can override their look). 48 pt high; small 40, large 56.")]),
    sub("panels", "Panels", "glass · solid · photo", () => [
      h("p", { class: "guide" }, "Glass (over the camera) · solid panel (cards, sheets, dialogs, app pages) · photo card. On a panel, secondary buttons are tonal. The shade under words on a photo is a fixed rule (Unity --ts-scrim-hero): words keep 4.5:1 at the worst pixel, so it is not a setting.")]),
    sub("controls", "Controls", `selected: ${S.selected}`, () => [
      ...row("Selected chips", fSelect(S, "selected", [["tint", "Accent wash + ring (quiet)"], ["fill", "Filled with the accent"]], rp), "A selected chip is also bold, never colour alone. The wash keeps the main button the one accent-filled thing in view (rule D5). Segments use a raised thumb; the nav bar its own current-tab style."),
      h("p", { class: "guide" }, "Try them on the phone: search, chips, segments, switches and the slider respond.")]),
    sub("lists", "Lists and messages", "rows · toast · banner · badges · loading · progress", () => [h("p", { class: "guide" }, "Rows 56 / 72 pt (one or two lines) in a grouped list. Toasts confirm (one at a time), banners guide, badges count, skeletons and progress wait. Spacing follows Density.")]),
    head("Patterns"),
    sub("nav", "Navigation", `${N.style} · ${N.tabs} tabs`, () => [
      ...row("Current tab", fSelect(N, "style", [["raised", "Raised (a disc in a bump)"], ["pill", "Pill behind the icon"], ["underline", "Underline"], ["none", "Bold only"]], rp)),
      ...row("Bar", fSelect(N, "shape", [["floating", "Floating"], ["dock", "Dock (compact capsule)"], ["bar", "Full width"]], rp), "Landscape turns it into a rail on the leading edge."),
      ...row("Fill", fSelect(N, "fill", [["solid", "Solid"], ["glass", "Glass"]], rp)),
      ...row("Destinations", fSelect(N, "tabs", [["3", "3 · Wall, Explore, You"], ["4", "4 · + Saved"], ["5", "5 · + Tours"]], rp)),
      ...row("Labels", fSelect(N, "labels", [["always", "Always"], ["selected", "Only the current tab"], ["off", "Icons only (still named for screen readers)"]], rp)),
      ...row("Page titles", fSelect(N, "title", [["large", "Large, then compact"], ["inline", "Compact, centred"]], rp)),
      h("p", { class: "guide" }, "On the phone: tap the tabs, tap a row to push a page, tap back. Wall is the camera view (the nav bar hides there; the AR screen keeps its own buttons).")]),
    sub("sheets", "Sheets and dialogs", `opens ${SH.detent}`, () => [
      ...row("Opens at", fSelect(SH, "detent", [["peek", "Peek (the title row)"], ["half", "Half (40 %)"], ["full", "Full"]], () => { kitS().sheet = null; rp(); }), "Peek 57 pt, Half 40 % of the screen, Full to 48 pt from the top: the Unity card's stops. The wall dims only at Full and behind a dialog (fixed: --ts-sheet-dim)."),
      ...row("Grabber", fToggle(SH, "grabber"), "36 × 5 pt in a 48 pt target; a tap moves one stop."),
      h("p", { class: "guide" }, "On the phone: tap the grabber to change the height; “Show a dialog” for the dialog.")]),
  ];
}
const DS_COLOUR_GROUPS = ["glass", "text", "buttons", "sheet"];
