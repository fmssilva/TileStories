/* ---------- On tap: named layouts (focus · tag · actions · screen card · ask), the renderer shared by the phone and the Wall screen, and the action views ---------- */
const TAP_OPTS = {
  focus:   [["ring", "Ring"], ["pulse", "Pulse"], ["spotlight", "Spotlight (dim the rest)"], ["none", "None"]],
  tag_kind: [["label", "Label (name only)"], ["chip", "Chip (name + line)"], ["card", "Compact card"], ["immersive", "Immersive card (image is the card)"], ["panel", "Floating panel (leader line)"]],
  place:   [["auto", "Auto"], ["above", "Above the POI"], ["below", "Below the POI"]],
  act_kind: [["radial", "Radial around the POI"], ["capsule", "Floating capsule"], ["in_tag", "Inside the tag"], ["dock", "Dock row at the bottom"], ["tiles", "Tiles in the screen card"]],
  sheet:   [["none", "None"], ["peek", "Peek"], ["half", "Half, with image"]],
  ask:     [["off", "Off"], ["capsule", "In the capsule"], ["sheet", "In the screen card"]],
};
/* the On-tap system, in three layers (App › On tap):
   1. base elements: the action buttons (one per action type, in priority order) and how a button looks;
   2. components: finished tag components (label, chip, card, immersive, panel) and action components (radial, capsule…);
   3. on-tap styles: which components a tap shows and how they are arranged.
   The app has a default style; a level may pick another; a POI may pick another and override single fields. */
D.tap_actions = [
  { key: "card", label: "Card", show: "always" },
  { key: "model_wall", label: "3D here", show: "auto" },
  { key: "model_screen", label: "3D viewer", show: "auto" },
  { key: "pano", label: "360°", show: "auto" },
  { key: "then_now", label: "Then & now", show: "auto" },
  { key: "play", label: "Play", show: "auto" },
  { key: "map", label: "Map", show: "always" },
  { key: "favourite", label: "Save", show: "always" },
  { key: "ask", label: "Ask", show: "always" },
  { key: "navigate", label: "Go", show: "off" },
  { key: "share", label: "Share", show: "off" },
  { key: "tour", label: "Tour", show: "off" },
  { key: "quiz", label: "Quiz", show: "auto" },
];
/* "auto": the button shows only when the POI has the content for it (framework code, one rule per action type) */
const NEEDS = {
  model_wall: [p => !!p.actions?.model?.file, "a 3D model"], model_screen: [p => !!p.actions?.model?.file, "a 3D model"],
  pano: [p => !!p.media?.pano, "a 360 photo"], then_now: [p => !!p.media?.today, "a today photo"], play: [p => !!p.media?.video, "a video or audio"],
  quiz: [p => !!p.quiz, "a quiz"],
};
D.tap_button = { use_ds: true, size: 1.1, shape: "circle", surface: "glass" };   // use_ds: the Design system's buttons (App › 1); off = this override
/* Two designs, one per mode (App › 1 Design system › Colour). The values are the Unity theme's (Theme.default.uss, --ts-*),
   measured at 7:1 for text in linear light; the prototype adds the app's glass over the camera. Each group is a role family:
   glass (bars, search, chips, glass panels: --ts-app-glass), text (--ts-ink / --ts-ink-2), border (--ts-glass-edge), accent
   (the closed accent list, App › 1 › Colour › Accent), icon buttons, tags, panels (--ts-surface: screen card, sheets, dialogs,
   app pages) and the POI labels on the wall. The main button is the accent (one focal action per view, rule D5).
   Unity: one theme asset, both modes, the same role names. */
D.themes = {
  light: {
    glass: { color: "#FBFAF7", tint: 0.78, blur: 28 }, text: { main: "#14110D", soft: "#38312A" }, border: { color: "#14110D", opacity: 0.12 },
    accent: "#1E3A8A", accent_text: "#FFFFFF",
    buttons: { color: "#FFFFFF", tint: 0.94, icon: "#14110D", label: "#FFFFFF", label_text: "#14110D" },
    tags: { color: "#FFFFFF", tint: 0.94 },
    sheet: { color: "#F8F6F2", tint: 0.97, text: "#14110D", soft: "#38312A" },
    labels: { color: "#FAF8F4", tint: 0.94, text: "#14110D" },
  },
  dark: {
    glass: { color: "#1A1B1E", tint: 0.8, blur: 24 }, text: { main: "#F6F1E8", soft: "#D9D1C3" }, border: { color: "#FFFFFF", opacity: 0.14 },
    accent: "#F2B544", accent_text: "#1A1206",
    buttons: { color: "#1C1A17", tint: 0.94, icon: "#F6F1E8", label: "#1C1A17", label_text: "#F6F1E8" },
    tags: { color: "#1C1A17", tint: 0.96 },
    sheet: { color: "#1C1A17", tint: 0.98, text: "#F6F1E8", soft: "#D9D1C3" },
    labels: { color: "#0A0806", tint: 0.92, text: "#FFFFFF" },
  },
};
/* the accent: a closed list, each pair measured at 7:1 (text) on the surfaces, light | dark, with the ink on it (_3.3 §6.2) */
const ACCENTS = {
  heritage: { name: "Heritage", light: ["#1E3A8A", "#FFFFFF"], dark: ["#F2B544", "#1A1206"] },
  azulejo: { name: "Azulejo", light: ["#13507E", "#FFFFFF"], dark: ["#7DB8E8", "#0B1A26"] },
  terracotta: { name: "Terracotta", light: ["#823114", "#FFFFFF"], dark: ["#F0A07A", "#2A0F05"] },
  garden: { name: "Garden", light: ["#24532F", "#FFFFFF"], dark: ["#8CC99B", "#0C2213"] },
};
function setAccent(key) { const A = ACCENTS[key]; if (!A) return; for (const m of ["light", "dark"]) { D.themes[m].accent = A[m][0]; D.themes[m].accent_text = A[m][1]; } }
D.app.appearance = "light";   // light is the default (sunlight first, rule D1); or follow the phone's setting, or always dark
ui.appearance = ui.appearance || "light";   // the Lab's preview when the app follows the phone
/* the design shown: the app's setting, unless the Lab is previewing the other design (a Dark / Light design section is open) */
const appearance = () => { const v = ui.forceMode || D.profile?.appearance || D.app.appearance; return v === "follow" ? ui.appearance : v; };   // the Lab preview, then the visitor's Settings, then the app's default
/* sets the theme variables on a container (a tap or main-screen layer, or the phone screen for the wall labels) */
function applyGlass(root) {
  const mode = appearance(), T = D.themes[mode];
  root.classList.toggle("light", mode === "light"); root.classList.toggle("dark", mode === "dark"); applyDS(root);
  const v = {
    "--tg": rgba(T.glass.color, D.ds.glass === "solid" ? Math.max(T.glass.tint, mode === "light" ? 0.84 : 0.98) : T.glass.tint), "--gblur": T.glass.blur + "px", "--tx": T.text.main, "--td": T.text.soft, "--tb": rgba(T.border.color, T.border.opacity), "--acc": T.accent, "--acc-fg": T.accent_text || "#15171C",
    "--btn": rgba(T.buttons.color, T.buttons.tint), "--btn-fg": T.buttons.icon, "--btl": rgba(T.buttons.label, 0.85), "--btl-fg": T.buttons.label_text,
    "--tagbg": rgba(T.tags.color, T.tags.tint), "--sheet": rgba(T.sheet.color, T.sheet.tint), "--sheet-fg": T.sheet.text, "--sheet-fg2": T.sheet.soft,
    "--cta": T.accent, "--cta-fg": T.accent_text, "--lbl": rgba(T.labels.color, T.labels.tint), "--lbl-fg": T.labels.text,
    "--ink": T.border.color, "--hs": mode === "light" ? rgba(T.border.color, 0.9) : "rgba(0,0,0,.6)" };   // the outline ink and the hard shadow (looks with outline / hard shadows)
  Object.entries(v).forEach(([k, x]) => root.style.setProperty(k, x));
}
D.tap_tags = [
  { key: "tag_comp_1", name: "Label", kind: "label" },
  { key: "tag_comp_2", name: "Chip", kind: "chip", show_cat: true, show_summary: false, buttons: false },
  { key: "tag_comp_3", name: "Compact card", kind: "card", media: "image", show_cat: true, show_summary: true, buttons: false },
  { key: "tag_comp_4", name: "Immersive card", kind: "immersive", size: 66, media: "image", show_cat: true, show_summary: true, buttons: true },
  { key: "tag_comp_5", name: "Floating panel", kind: "panel", media: "image", show_cat: false, show_summary: true, buttons: true },
];
D.tap_action_comps = [
  { key: "act_comp_1", name: "Radial ring", kind: "radial", max: 4, labels: true },
  { key: "act_comp_2", name: "Capsule", kind: "capsule", max: 4, labels: true },
  { key: "act_comp_3", name: "Links in the tag", kind: "in_tag", max: 4, labels: false },
  { key: "act_comp_4", name: "Dock", kind: "dock", max: 3, labels: true },
  { key: "act_comp_5", name: "Tiles", kind: "tiles", max: 6, labels: true },
];
const SCREEN = { focus: false, tag: false, actions: false };
D.tap_layouts = [
  { key: "tap_layout_1", name: "Minimal", focus: "ring", tag: "tag_comp_2", place: "auto", actions: "act_comp_2", sheet: "none", ask: "off", on_wall: { ...SCREEN }, depth_cm: 8, gap: 0 },
  { key: "tap_layout_2", name: "Radial", focus: "ring", tag: "tag_comp_2", place: "auto", actions: "act_comp_1", sheet: "peek", ask: "off", on_wall: { ...SCREEN }, depth_cm: 8, gap: 0 },
  { key: "tap_layout_3", name: "Immersive card", focus: "spotlight", tag: "tag_comp_4", place: "auto", actions: "act_comp_3", sheet: "none", ask: "off", on_wall: { ...SCREEN }, depth_cm: 8, gap: 0 },
  { key: "tap_layout_4", name: "Floating panels", focus: "pulse", tag: "tag_comp_5", place: "below", actions: "act_comp_3", sheet: "none", ask: "off", on_wall: { focus: true, tag: true, actions: true }, depth_cm: 10, gap: 0 },
  { key: "tap_layout_5", name: "Sheet with tiles", focus: "ring", tag: "tag_comp_1", place: "auto", actions: "act_comp_5", sheet: "peek", ask: "sheet", on_wall: { ...SCREEN }, depth_cm: 8, gap: 0 },
  { key: "tap_layout_6", name: "Card only", focus: "ring", tag: "none", place: "auto", actions: "none", sheet: "half", ask: "off", on_wall: { ...SCREEN }, depth_cm: 8, gap: 0 },
];
/* the style a POI uses (preview › POI › level › app default) and the flat layout the renderer reads (components and POI overrides resolved) */
const tapStyleByKey = k => D.tap_layouts.find(t => t.key === k);
const tagComp = k => D.tap_tags.find(t => t.key === k), actComp = k => D.tap_action_comps.find(t => t.key === k);
/* a component preview (App › On tap): the phone shows one tag or one action component on the POI, alone */
function compPreviewStyle() {
  const C = ui.compPreview; if (!C || C.type === "buttons") return null;
  if (C.type === "tag") return { key: "_comp", name: "Tag · " + (tagComp(C.key)?.name || ""), focus: "ring", tag: C.key, place: "auto", actions: "none", sheet: "none", ask: "off", on_wall: { ...SCREEN }, depth_cm: 0, gap: 0 };
  const A = actComp(C.key), tagFor = A?.kind === "in_tag" ? "tag_comp_3" : "tag_comp_1";
  return { key: "_comp", name: "Buttons · " + (A?.name || ""), focus: "ring", tag: D.tap_tags.some(t => t.key === tagFor) ? tagFor : "none", place: "auto", actions: C.key, sheet: A?.kind === "tiles" ? "peek" : "none", ask: "off", on_wall: { ...SCREEN }, depth_cm: 0, gap: 0 };
}
function tapStyleOf(p) {
  const cp = compPreviewStyle(); if (cp) return cp; return tapStyleByKey(ui.previewLayout) || tapStyleByKey(p?.on_tap?.tap_layout) || tapStyleByKey(levelOf(p)?.tap_layout) || tapStyleByKey(D.app.tap.default_layout) || D.tap_layouts[0]; }
const OVERRIDABLE = ["tag", "actions", "focus", "gap", "depth_cm"];
function tapLayoutOf(p) {
  const S = tapStyleOf(p), o = (!ui.previewLayout && !ui.compPreview) ? p?.on_tap?.over || {} : {};
  const M = { ...S }; OVERRIDABLE.forEach(k => { if (o[k] !== undefined) M[k] = o[k]; });
  const T = tagComp(M.tag), A = actComp(M.actions);
  return { ...M, style: S, tagComp: T, actComp: A, tag: T ? T.kind : "none", actions: A ? A.kind : "none", max: A?.max ?? 4, labels: A?.labels ?? false, gap: +M.gap || 0 };
}
/* the buttons a POI offers: the catalogue order; "auto" ones only when the POI has the content; the POI may hide or force one */
function poiActions(p) {
  const o = p?.on_tap?.over?.buttons || {};
  return D.tap_actions.filter(a => o[a.key] === "hide" ? false : o[a.key] === "show" ? true : a.show === "always" || (a.show === "auto" && (NEEDS[a.key]?.[0](p) ?? true))).map(a => a.key);
}
const actionLabel = k => D.tap_actions.find(a => a.key === k)?.label || ACTIONS[k]?.label || k;
const tapSummary = S => [S.focus === "none" ? "—" : S.focus, tagComp(S.tag)?.name || "—", actComp(S.actions)?.name || "—", S.sheet === "none" ? "—" : S.sheet + " card", S.ask === "off" ? "—" : "ask"].join(" · ");
/* v20: the action row's labels under the buttons: one short word each, so four always fit a phone card (both looks, text L) */
const ROWL = { card: "Story", model_wall: "3D", model_screen: "Viewer", pano: "360°", then_now: "Then", play: "Play", map: "Map", favourite: "Save", ask: "Ask", navigate: "Go", share: "Share", tour: "Tour", quiz: "Quiz" };
const SHORT = { card: "Card", model_wall: "3D", model_screen: "3D view", pano: "360°", map: "Map", then_now: "Then & now", play: "Play", ask: "Ask", navigate: "Go", favourite: "Save", share: "Share", tour: "Tour", quiz: "Quiz" };
const ICONS = {
  card: '<rect x="4" y="3" width="16" height="18" rx="2"/><path d="M8 8h8M8 12h8M8 16h5"/>',
  model_wall: '<path d="M12 3l8 4.5v9L12 21l-8-4.5v-9z"/><path d="M12 12l8-4.5M12 12v9M12 12L4 7.5"/>',
  model_screen: '<rect x="3" y="4" width="18" height="13" rx="2"/><path d="M12 7.5l3 1.7v3.6l-3 1.7-3-1.7V9.2z"/><path d="M8 21h8"/>',
  pano: '<path d="M3 12c0-2.5 4-4.5 9-4.5s9 2 9 4.5-4 4.5-9 4.5"/><path d="M8 14.5L5.5 17 8 19.5"/>',
  map: '<path d="M12 21s-6-5.5-6-10a6 6 0 0112 0c0 4.5-6 10-6 10z"/><circle cx="12" cy="11" r="2.2"/>',
  then_now: '<rect x="3" y="5" width="18" height="14" rx="2"/><path d="M12 3v18"/>',
  play: '<circle cx="12" cy="12" r="9"/><path d="M10 8.5l5 3.5-5 3.5z"/>',
  ask: '<path d="M12 3l1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8z"/>',
  navigate: '<path d="M4 11l16-7-7 16-2-7z"/>',
  favourite: '<path d="M12 20s-7-4.4-7-10a4 4 0 017-2.6A4 4 0 0119 10c0 5.6-7 10-7 10z"/>',
  share: '<path d="M12 3v12M7 8l5-5 5 5"/><path d="M5 13v6h14v-6"/>',
  tour: '<circle cx="6" cy="18" r="2"/><circle cx="18" cy="6" r="2"/><path d="M8 18h6a3 3 0 000-6h-4a3 3 0 010-6h6"/>',
  quiz: '<circle cx="12" cy="12" r="9"/><path d="M9.5 9.5a2.5 2.5 0 114 2c-1 .7-1.5 1.2-1.5 2.5M12 17h.01"/>',
  more: '<circle cx="6" cy="12" r="1.3"/><circle cx="12" cy="12" r="1.3"/><circle cx="18" cy="12" r="1.3"/>',
  close: '<path d="M6 6l12 12M18 6L6 18"/>', mic: '<rect x="9" y="3" width="6" height="11" rx="3"/><path d="M5 11a7 7 0 0014 0M12 18v3"/>',
  send: '<path d="M4 12l16-8-6 16-2-6z"/>', dig: '<path d="M9 3.5h6M12 3.5v9.5"/><path d="M7.5 13h9v2.5a4.5 4.5 0 01-9 0z"/>', chev: '<path d="M9 6l6 6-6 6"/>', up: '<path d="M6 15l6-6 6 6"/>',
};
const svg = k => { const t = document.createElement("template"); t.innerHTML = `<svg viewBox="0 0 24 24">${ICONS[k] || ICONS.more}</svg>`; return t.content.firstChild; };
/* the POI's own picture: the wall photo framed on its shapes (stands in for the card's hero image) */
function poiImage(p, pad = 0.18) {
  const b = bboxOf(p); let bw = b.x1 - b.x0, bh = b.y1 - b.y0;
  if (bw < 30) { b.x0 = p.pos.x - 40; bw = 80; } if (bh < 30) { b.y0 = p.pos.y - 40; bh = 80; }
  const x0 = Math.max(0, b.x0 - bw * pad), y0 = Math.max(0, b.y0 - bh * pad), w = Math.min(IW - x0, bw * (1 + 2 * pad)), hh = Math.min(IH - y0, bh * (1 + 2 * pad));
  return `background-image:url(${SRC});background-size:${IW / w * 100}% ${IH / hh * 100}%;background-position:${x0 / Math.max(1, IW - w) * 100}% ${y0 / Math.max(1, IH - hh) * 100}%`;
}
/* the POI's hero image: its photo when it has one, else the tiles framed on its shapes */
const heroStyle = (p, pad) => p.media?.hero ? `background-image:url(${PHOTOS[p.media.hero]});background-size:cover;background-position:center` : poiImage(p, pad);
const tapState = { view: null, toast: "", favs: {}, toastT: 0 };
/* two lines at most for the cards on the wall: Quick's line, else the first sentence of the story at this profile */
function shortOf(p) { const S = p.story || {}; const t = forKids() && S.kids ? S.kids : S.quick || p.summary || storyOf(p) || ""; const m = t.match(/^.*?[.!?](\s|$)/); return (m ? m[0] : t).trim(); }
function tapButtons(p, L) {
  if (isSoon(p)) return [{ a: "card", icon: forKids() ? "dig" : "clock", label: "What's coming" }, { a: "favourite", icon: "favourite", label: "Save" }].slice(0, Math.max(1, L.max));   // a place without its story yet: what's coming, and save it
  const acts = poiActions(p).filter(a => a !== "ask" || L.ask === "off");
  const shown = acts.slice(0, L.max), more = acts.length - shown.length;
  const out = shown.map(a => ({ a, icon: a, label: actionLabel(a) }));
  if (more > 0) out.push({ a: "more", icon: "more", label: "+" + more, more: true });
  return out;
}
/* draws the tap UI for POI p at (x %, y %) of root into root (a .tapc layer). opts: { interactive, rerender } */
function tapUI(root, p, x, y, L, opts = {}) {
  const act = opts.interactive ? a => e => { e.stopPropagation(); tapAction(p, a, opts.rerender); } : () => null;
  const add = el => { root.append(el); return el; };
  const btns = tapButtons(p, L.tag === "card" && (L.actions === "in_tag" || (L.tagComp?.buttons && L.actions === "none")) ? { ...L, max: Math.min(L.max, 3) } : L), ar = root.dataset.ar ? +root.dataset.ar : PHR;
  let toC = Math.hypot(50 - x, (50 - y) / ar) < 12 ? -Math.PI / 2 : Math.atan2((50 - y) / ar, 50 - x);   // from the POI towards the screen centre
  // v19 F-tap: landscape has little height: the ring opens up or down (its arc lies along the width), leaning a little towards the centre; a sideways ring stood as a tall column and its labels collided
  if (root.classList.contains("land") && L.actions === "radial") toC = Math.atan2(y > 50 ? -1 : 1, clamp((50 - x) / 50, -1, 1) * 0.5);
  const above = L.place === "above" || (L.place === "auto" && (L.actions === "radial" ? Math.sin(toC) > 0.2 || (Math.sin(toC) > -0.2 && y > 50) : y > 46));
  const soon = isSoon(p), col = catColor(p), T = L.tagComp || {}, cat = T.show_cat === false ? "" : CATS[p.category]?.label || "", sum = T.show_summary === false ? "" : soon ? soonText().line : storyOf(p), G = L.gap || 0;
  const short = soon ? soonText().line : shortOf(p);   // the immersive card and the panel: two lines at most; the full story is one tap away
  const soonB = () => soon ? h("span", { class: "soonb" }, svg(forKids() ? "dig" : "clock"), soonText().badge) : null;   // Playful: Azu's spade (still digging); Premium: a calm clock
  const media = (pad) => T.media === "none" ? "background:linear-gradient(160deg,#2A3550,#121826)" : heroStyle(p, pad);
  const vid = () => T.media === "video" ? h("span", { class: "vidb" }, svg("play")) : null;
  root.style.setProperty("--g", G); root.style.setProperty("--bk", btnLook().size); applyGlass(root);
  const inside = L.actions === "in_tag" || (T.buttons && L.actions === "none" && L.tag !== "label");   // buttons inside the tag: the style's "in the tag" component, or the tag's own option
  root.classList.toggle("b-round", btnLook().shape === "rounded"); root.classList.toggle("b-solid", btnLook().surface === "solid");
  // focus
  if (L.focus === "spotlight") add(h("div", { class: "f-spot fade", style: `background:radial-gradient(circle at ${x}% ${y}%, rgba(5,7,12,0) calc(18 * var(--u)), rgba(5,7,12,.2) calc(70 * var(--u)))` }));   // v20: a soft vignette, the wall stays readable around the card (Unity: a ramp sprite)
  if (L.focus === "ring") add(h("div", { class: "f-ring pop", style: `left:${x}%;top:${y}%` }));
  if (L.focus === "pulse") { add(h("div", { class: "f-pulse", style: `left:${x}%;top:${y}%` })); add(h("div", { class: "f-pulse", style: `left:${x}%;top:${y}%;animation-delay:.9s` })); }
  add(h("div", { class: "f-dot pop", style: `left:${x}%;top:${y}%` }));
  const uw = +root.dataset.uw || 1, cx = Math.min(100 - 36 * uw, Math.max(36 * uw, x)), off = above ? `bottom:calc(${100 - y}% + ${13 + G} * var(--u))` : `top:calc(${y}% + ${13 + G} * var(--u))`;
  const iconRow = (cls, style) => h("div", { class: cls + (L.labels ? " lab" : ""), style: style || "" }, ...btns.map((b, k) => L.labels ? h("span", { class: "ii hit pop", style: `animation-delay:${.15 + k * .05}s`, onclick: act(b.a) }, h("span", { class: "i" }, svg(b.icon)), h("small", {}, b.more ? "More" : b.label)) : h("span", { class: "i hit pop", title: b.label, style: `animation-delay:${.15 + k * .05}s`, onclick: act(b.a) }, svg(b.icon))));
  const open = opts.interactive ? e => { e.stopPropagation(); tapAction(p, "card", opts.rerender); } : null;
  // tag
  if (L.tag === "label") add(h("div", { class: "t-label g rise hit", style: `left:${cx}%;${off};transform:translateX(-50%)`, onclick: open }, p.name));
  if (L.tag === "chip") add(h("div", { class: "t-chip g rise hit", style: `left:${cx}%;${off};transform:translateX(-50%)`, onclick: open },
    h("span", { class: "cd", style: `background:${col}` }), h("div", {}, h("b", {}, p.name), soon ? h("small", { class: "soonl" }, svg(forKids() ? "dig" : "clock"), soonText().badge) : (cat || sum) ? h("small", {}, [cat, sum.split(",")[0].slice(0, 26)].filter(Boolean).join(" · ")) : null), h("span", { class: "chev" }, svg("chev"))));
  if (L.tag === "card") add(h("div", { class: "t-card g rise hit" + (inside ? " top" : ""), style: `left:${cx}%;${off};transform:translateX(-50%)`, onclick: open },
    T.media === "none" ? null : h("div", { class: "th", style: media() }, vid()), h("div", { style: "flex:1;min-width:0" }, soonB(), cat ? h("small", { class: "cat2", style: `--cc:${col}` }, cat) : null, h("b", {}, p.name), sum ? h("span", {}, sum) : null,
      inside ? iconRow("irow", "margin-top:calc(2 * var(--u))") : null),
    inside ? null : h("span", { class: "chev" }, svg("chev"))));
  // v20: the immersive card and the floating panel share one action row: the story first (labelled, in the accent), then up to three icon buttons with short labels
  // each button pops in after the card has risen, one after another (Unity: a scale+fade tween per button, started with a delay; tagged "tween")
  const actRow = (n = 4) => h("div", { class: "irow lab acts" + (soon ? " soon" : "") }, ...btns.slice(0, n).map((b, k) => h("span", { class: "ii hit apop" + (k === 0 && b.a === "card" ? " pri" : "") + (b.more ? " more" : ""), style: `animation-delay:${.2 + k * .07}s`, title: b.label, onclick: act(b.a) },
    h("span", { class: "i" }, svg(b.icon)), h("small", {}, k === 0 && b.a === "card" ? (soon ? "What's coming" : "Story") : b.more ? "More" : ROWL[b.a] || SHORT[b.a] || b.label))));
  if (L.tag === "immersive") {   // a picture-led card: the photo on top (a child image: Unity Image), the name, two lines, the action row. The full story is the card
    const fav = tapState.favs[p.id];
    const W = typeof T.size === "number" ? T.size : ({ S: 54, M: 66, L: 78 }[T.size] || 58);   // u: the card width
    add(h("div", { class: "t-conn", style: `left:${x}%;${above ? `bottom:calc(${100 - y}% + 3 * var(--u));height:calc(${7.8 + G} * var(--u))` : `top:calc(${y}% + 3 * var(--u));height:calc(${7.8 + G} * var(--u))`}` }));
    add(h("div", { class: "t-imm rise hit" + (soon ? " soon" : ""), style: `width:calc(${W} * var(--u));left:${Math.min(100 - (W / 2 + 5) * uw, Math.max((W / 2 + 5) * uw, x))}%;${above ? `bottom:calc(${100 - y}% + ${10.8 + G} * var(--u))` : `top:calc(${y}% + ${10.8 + G} * var(--u))`};transform:translateX(-50%)`, onclick: open },
      h("div", { class: "im", style: media(0.08) }, vid(), soonB()), soon && inside ? null : h("span", { class: "heart hit" + (fav ? " on" : ""), title: "Save", onclick: act("favourite") }, svg("favourite")),   // a soon place: Save is in the row already
      h("div", { class: "bd" }, cat ? h("span", { class: "cat", style: `--cc:${col}` }, cat) : null, h("b", {}, p.name), short ? h("span", { class: "sm" }, short) : null, inside ? actRow() : null)));
  }
  if (L.tag === "panel") {   // a post rises from the POI to one glass block (picture, name, two lines, the action row), turned a little towards the POI
    const lh = 14 + G, up = y > (root.classList.contains("land") ? 60 : 48),   /* v20: portrait: the panel with its row is ~45 % of the screen tall, so below the middle it opens upwards */ ty = up ? y - 2 : y + 2, left = x > 50, at = up ? `top:calc(-${lh + 2} * var(--u))` : `top:calc(${lh + 2} * var(--u))`;
    const tilt = `${up ? "translateY(-100%) " : ""}`;   // flat: a small tilt read as a rendering error (critics v16)
    add(h("div", { class: "t-pan" + (soon ? " soon" : ""), style: `left:${x}%;top:${ty}%;height:0` },
      h("div", { class: "ln" + (up ? " up" : ""), style: `left:0;top:${up ? `calc(-${lh} * var(--u))` : "calc(6 * var(--u))"};height:calc(${lh - 6} * var(--u))` }),
      h("div", { class: "tx rise", style: `${at};${left ? "right:calc(-6 * var(--u))" : "left:calc(-6 * var(--u))"};animation-delay:.15s;width:calc(66 * var(--u));transform:${tilt};transform-origin:${left ? "right" : "left"} ${up ? "bottom" : "top"}` },
        T.media !== "none" ? h("div", { class: "im in hit", style: media(), onclick: open }, vid(), soonB()) : soonB(),
        cat ? h("small", { class: "cat2", style: `--cc:${col}` }, cat) : null, h("b", {}, p.name), short ? h("span", { class: "sm" }, short) : null,
        inside ? actRow(4) : null)));
  }

  // actions
  if (L.actions === "radial") {
    const n = btns.length, span = Math.min(Math.PI * 0.9, Math.max(Math.PI * 0.21 * (n - 1), n === 2 && L.labels ? Math.PI * 0.34 : 0)) /* v20: two labelled buttons (a soon place) need room for "What's coming" */, R = (24 + 1.6 * n + G) * btnLook().size ** 0.5 * (+root.dataset.uw || 1);
    btns.forEach((b, k) => {
      const ang = toC - span / 2 + span * k / Math.max(1, n - 1), bx = x + Math.cos(ang) * R, by = y + Math.sin(ang) * R * ar;
      add(h("div", { class: "a-rad pop hit" + (b.more ? " more" : ""), title: b.label, style: `left:${bx}%;top:${by}%;animation-delay:${.08 + k * .06}s`, onclick: act(b.a) },
        h("div", { class: "c g" }, svg(b.icon)), L.labels ? h("div", { class: "l" }, b.label) : null));
    });
  }
  if (L.actions === "capsule") {
    const cy = L.tag === "none" ? (above ? `bottom:calc(${100 - y}% + ${13 + G} * var(--u))` : `top:calc(${y}% + ${13 + G} * var(--u))`) : (above ? `bottom:calc(${100 - y}% + ${41.1 + G} * var(--u))` : `top:calc(${y}% + ${41.1 + G} * var(--u))`);
    add(h("div", { class: "a-cap g rise", style: `left:50%;${cy};transform:translateX(-50%);width:calc(84 * var(--u));animation-delay:.12s` },
      h("div", { class: "row" }, ...btns.map((b, k) => h("div", { class: "tib hit pop" + (b.more ? " more" : ""), style: `animation-delay:${.2 + k * .05}s`, onclick: act(b.a) }, h("span", { class: "i" }, svg(b.icon)), L.labels ? h("span", { class: "l" }, b.label) : null))),
      L.ask === "capsule" ? h("div", { class: "ask hit", onclick: act("ask") }, h("span", { class: "dt" }), h("span", { class: "sp" }, `Ask Azu about ${p.name}…`), h("span", { class: "mi" }, svg("mic")), h("span", { class: "se" }, svg("send"))) : null));
  }
  if (L.tag === "chip" && inside) add(iconRow("irow", `position:absolute;left:${cx}%;${above ? `bottom:calc(${100 - y}% + ${32.5 + G} * var(--u))` : `top:calc(${y}% + ${32.5 + G} * var(--u))`};transform:translateX(-50%);margin:0`));
  // screen card
  const sheetH = root.classList.contains("land") ? 0 : L.sheet === "half" ? 46 : L.sheet === "peek" ? (L.actions === "tiles" ? 34 : 22) + (L.ask === "sheet" ? 6 : 0) : 0;
  if (L.actions === "dock") add(h("div", { class: "a-dock rise", style: `bottom:${sheetH ? `${sheetH + 2}%` : "calc(7 * var(--u))"}` }, ...btns.map((b, k) => h("span", { class: "pill g hit", style: `animation-delay:${k * .04}s`, onclick: act(b.a) }, h("span", { class: "i" }, svg(b.icon)), L.labels ? b.label : null))));
  if (L.sheet !== "none") {
    add(h("div", { class: "s-card rise sh-" + L.sheet },
      h("div", { class: "gr" }), L.sheet === "half" ? h("div", { class: "ban", style: heroStyle(p) }) : null,
      h("span", { class: "cat", style: `--cc:${col}` }, cat), soonB(), h("b", {}, p.name), h("p", {}, soon ? soonText().line : storyOf(p)), timeRow().deep_blocks ? h("div", { class: "ddive" }, svg("sparkle"), h("span", {}, forKids() ? "Want more? 3 more stories" : "Deep dive · sources, dates and 3 more stories")) : null,
      L.actions === "tiles" ? h("div", { class: "tiles" }, ...btns.filter(b => b.a !== "card").map(b => h("div", { class: "tile hit" + (L.labels ? "" : " nol"), title: b.label, onclick: act(b.a) }, h("span", { class: "i" }, svg(b.icon)), L.labels ? (b.more ? "More" : b.label) : null))) : null,
      L.ask === "sheet" ? h("div", { class: "ask hit", onclick: act("ask") }, h("span", { class: "dt" }), h("span", { class: "sp" }, "Ask Azu about this place…"), h("span", { class: "mi" }, svg("mic"))) : null,
      h("div", { class: "tcta hit", onclick: open }, soon ? "What's coming" : "Open the full card", h("span", { class: "i" }, svg("up")))));
  }
  // an action view or a toast on top
  if (tapState.view && tapState.view.p === p) add(actionView(p, tapState.view.a, opts));
  if (tapState.toast && performance.now() - tapState.toastT < 1800) add(h("div", { class: "toast g" }, tapState.toast));
}
function tapAction(p, a, rerender) {
  const now = performance.now();
  if (a === "favourite") { tapState.favs[p.id] = !tapState.favs[p.id]; tapState.toast = tapState.favs[p.id] ? "♥ Saved to your favourites" : "Removed from favourites"; tapState.toastT = now; }
  else if (["card", "pano", "then_now", "model_wall", "model_screen"].includes(a)) tapState.view = { p, a };
  else { const phase = { map: "later (geo-reference)", navigate: "phase D", tour: "phase D", quiz: "phase E", share: "later", play: "uses the media fill", ask: "the Guide tab", more: "a “more” sheet" }[a] || "planned"; tapState.toast = `${a === "more" ? "More" : SHORT[a] || a} · ${phase}`; tapState.toastT = now; }
  rerender && rerender(); if (tapState.toast) setTimeout(() => rerender && rerender(), 1900);
}
function actionView(p, a, opts) {
  const close = h("span", { class: "x hit", onclick: e => { e.stopPropagation(); tapState.view = null; opts.rerender && opts.rerender(); } }, svg("close"));
  const v = h("div", { class: "tv fade hit", onclick: e => e.stopPropagation() });
  if (a === "card" && isSoon(p)) {   // the "coming soon" page: the name always, the tiles it will be about, an honest line, save it
    const ST = soonText(), kids = forKids();
    v.classList.add("tv-soon");   // its own page (the detail card's look rules do not apply)
    v.append(h("div", { class: "sn-hero", style: poiImage(p, 0.25) }, h("span", { class: "soonb big" }, svg(kids ? "dig" : "clock"), ST.badge)),
      h("div", { class: "sn-bd" }, h("span", { class: "cat", style: `--cc:${catColor(p)}` }, CATS[p.category]?.label || ""), h("b", { class: "sn-t" }, p.name),
        kids && visitorRow().mascot && typeof KIDS !== "undefined" ? h("div", { class: "sn-say" }, KIDS.azuImg("point", "sn-azu"), h("p", { class: "bub" }, ST.line)) : h("p", { class: "sn-l" }, ST.line),
        h("p", { class: "sn-b" }, ST.body),
        h("div", { class: "sn-way" }, h("small", {}, kids ? "Azu is looking for" : "On its way"), h("div", { class: "sn-slots" }, ...[["card", "The story"], ["then_now", kids ? "Old pictures" : "Pictures"], ["model_wall", kids ? "A 3D model" : "3D"]].map(([i, t]) => h("span", { class: "sn-slot" }, svg(i), t)))),
        h("div", { class: "sn-acts" }, dsBtn(tapState.favs[p.id] ? "Saved ♥" : kids ? "Keep it in my list" : "Save for later", tapState.favs[p.id] ? "tonal" : "main", { on: e => { e.stopPropagation(); tapAction(p, "favourite", opts.rerender); } }),
          dsBtn("Back to the wall", "text", { on: e => { e.stopPropagation(); tapState.view = null; opts.rerender && opts.rerender(); } }))), close);
  } else if (a === "card") {
    v.classList.add("tv-card"); v.style.background = "#F6F3EC"; v.style.color = "#1B1F27";
    v.append(h("div", { style: `height:52%;${heroStyle(p, 0.06)}` }), h("div", { style: "padding:calc(5 * var(--u))" },
      h("span", { class: "cat", style: `font:500 calc(2.4 * var(--u))/1 var(--mono);letter-spacing:.08em;text-transform:uppercase;padding:calc(1.1 * var(--u)) calc(2.2 * var(--u));border-radius:99px;background:${catColor(p)}` }, CATS[p.category]?.label),
      h("b", { style: "display:block;font:600 calc(7.4 * var(--u))/1.05 var(--display);margin:calc(2.4 * var(--u)) 0 calc(1.4 * var(--u))" }, p.name), h("p", { style: "margin:0;font-size:calc(3.2 * var(--u));line-height:1.5;color:#4B5160" }, storyOf(p)),
      ...[70, 92, 84, 60].map(w => h("div", { style: `height:calc(2.2 * var(--u));width:${w}%;border-radius:calc(1 * var(--u));background:#E3DDD1;margin-top:calc(2.6 * var(--u))` })),
      h("p", { style: "font:500 calc(2.4 * var(--u)) var(--mono);color:#8A8F99;margin-top:calc(4 * var(--u))" }, "THE DETAIL CARD (its own Lab) OPENS HERE")), close);
  } else if (a === "pano") {
    const src = p.media?.pano ? null : mediaSource("media_1", 3); const url = p.media?.pano ? PHOTOS[p.media.pano] : src && src.toDataURL ? src.toDataURL() : "";
    let px = 0; const pano = h("div", { class: "pano", style: `background-image:url(${url})` });
    pano.addEventListener("pointerdown", e => { const x0 = e.clientX, p0 = px; pano.setPointerCapture(e.pointerId); const mv = ev => { px = p0 + (ev.clientX - x0); pano.style.backgroundPositionX = px + "px"; }; pano.addEventListener("pointermove", mv); pano.addEventListener("pointerup", () => pano.removeEventListener("pointermove", mv), { once: true }); });
    v.append(pano, h("div", { class: "vt" }, "360° · drag to look around", h("b", {}, p.name + (p.media?.pano ? "" : " · inside (sample)"))), close);
  } else if (a === "then_now") {
    const today = p.media?.today ? `background-image:url(${PHOTOS[p.media.today]});background-size:cover;background-position:center` : null;
    let f = 0.5; const then = h("div", { class: "wipe", style: poiImage(p, 0.12) + ";clip-path:inset(0 50% 0 0)" }), hd = h("div", { class: "handle", style: "left:50%" });
    v.append(today ? h("div", { class: "wipe", style: today }) : h("div", { class: "wipe nophoto" }, h("span", {}, "No “today” photo yet: add one in Specific Marker › Content for actions")), then, hd,
      h("span", { class: "yr", style: "left:4cqw" }, "1700s tiles"), h("span", { class: "yr", style: "right:4cqw" }, "Today"), h("div", { class: "vt" }, "Then & now · drag", h("b", {}, p.name)), close);
    v.addEventListener("pointerdown", e => { v.setPointerCapture(e.pointerId); const mv = ev => { const r = v.getBoundingClientRect(); f = clamp((ev.clientX - r.left) / r.width, 0.02, 0.98); then.style.clipPath = `inset(0 ${(1 - f) * 100}% 0 0)`; hd.style.left = f * 100 + "%"; }; mv(e); v.addEventListener("pointermove", mv); v.addEventListener("pointerup", () => v.removeEventListener("pointermove", mv), { once: true }); });
  } else { // 3D: a preview of phase C
    const img = poiImage(p, 0.04), faces = ["translateZ(calc(20 * var(--u)))", "rotateY(90deg) translateZ(calc(20 * var(--u)))", "rotateY(180deg) translateZ(calc(20 * var(--u)))", "rotateY(-90deg) translateZ(calc(20 * var(--u)))", "rotateX(90deg) translateZ(calc(20 * var(--u)))", "rotateX(-90deg) translateZ(calc(20 * var(--u)))"];
    const moments = p.actions?.model?.moments || [];
    v.style.background = "radial-gradient(circle at 50% 40%, #2A3550, #0B0E14 70%)";
    v.append(h("div", { class: "stage3d" }, h("div", { class: "cube" }, ...faces.map(t => h("div", { style: `transform:${t};${img}` })))),
      h("div", { class: "vt" }, `${p.name} · 3D` + (a === "model_wall" ? " on the wall" : ""), h("b", {}, "Turn it with a finger")),
      h("div", { class: "chipsrow" }, ...[...(p.actions?.model?.controls?.rotate !== false ? ["Rotate"] : []), ...(p.actions?.model?.controls?.zoom !== false ? ["Zoom"] : []), ...(p.actions?.model?.controls?.move ? ["Move"] : []), ...moments.map(m => "▶ " + m.name)].map(t => h("span", { class: "pill g", style: "min-height:calc(12.4 * var(--u));padding:0 calc(4 * var(--u))" }, t))), close);
  }
  return v;
}
