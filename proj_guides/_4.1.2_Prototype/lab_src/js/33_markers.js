/* ---------- Global Scene › Looks › Point styles: how a POI shows as a point ----------
   The Unity marker system (_2.2.1-_2.2.4) as named styles, like the region styles: a shape and its fill, how the category
   shows, an icon (a line icon, or the project's own symbol art), a rim, a shadow, the label, the status ring (the wall's
   outline levels: solid → long dash → short dash → dotted), a corner badge and one effect. A level picks a style (Hub /
   Landmark / Detail), a POI may pick another. One drawing routine serves the wall (canvas) and the app (a canvas per marker),
   so the two always match. Sizes are screen pt (anchored to the wall, a constant size on screen, as Unity's LOD keeps them). */
Object.assign(CATS, {   // the LivingRoom wall's six categories (category_styles), with the colours the Unity config holds
  category_3: { label: "Military", color: "#B44848" },
  category_4: { label: "Residential", color: "#FF9FFC" },
  category_5: { label: "Economic", color: "#8F5E48" },
});
const CAT_LINE = { category_1: "crown", category_2: "church", category_3: "tower", category_4: "house", category_5: "anchor", category_6: "bridge" };
const CAT_SYM = { category_1: "@@SYM_ROYAL@@", category_2: "@@SYM_RELIGIOUS@@", category_3: "@@SYM_MILITARY@@", category_4: "@@SYM_RESIDENTIAL@@", category_5: "@@SYM_ECONOMIC@@", category_6: "@@SYM_INFRA@@" };
/* line icons on a 24 grid (Unity: one icon library sprite each; the symbol art stays for the Storybook style) */
const MK_ICONS = {
  crown: "M4 18h16M5 18 4 8l5 4 3-6 3 6 5-4-1 10",
  church: "M12 2.5v4M10 4.5h4M6 21V11l6-4.5 6 4.5v10M10 21v-3.5a2 2 0 0 1 4 0V21M3 21h18",
  tower: "M6 21V10h12v11M6 10V5h2.4v2h2.4V5h2.4v2h2.4V5H18v5M10 21v-4.5h4V21M3 21h18",
  house: "M3.5 11.5 12 4l8.5 7.5M6 10v11h12V10M10 21v-6h4v6",
  anchor: "M12 8a2.2 2.2 0 1 0 0-4.4A2.2 2.2 0 0 0 12 8zM12 8v13M8 11.5h8M4.5 14a7.5 7.5 0 0 0 15 0",
  bridge: "M2.5 9h19M3.5 9v12M20.5 9v12M3.5 16a4.25 4.25 0 0 1 8.5 0v5M12 21v-5a4.25 4.25 0 0 1 8.5 0",
  unknown: "M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zM9.6 9.4a2.5 2.5 0 1 1 3.4 2.4c-.6.3-1 .8-1 1.5v.6M12 16.8v.2",
};
/* the wall's status levels (Unity outline_levels): an ordinal axis drawn as the ring's dash rhythm, never by colour alone */
D.status_levels = [
  { key: "intact", label: "Intact", dash: [] },
  { key: "partial_damage", label: "Partial damage", dash: [7, 4] },
  { key: "destroyed", label: "Destroyed", dash: [2.5, 3.5] },
  { key: "unknown", label: "Unknown", dash: [0.1, 3.2] },
];
const statusOf = p => D.status_levels.find(s => s.key === p.status);
function mkPoint(key, name, o = {}) {
  return Object.assign({
    key, name,
    shape: "disc",        // disc · tile · diamond · dot · none (the icon floats, Unity "none")
    size_pt: 32,          // the shape's diameter on screen
    fill: "glass",        // glass (the app's glass) · surface (the panel colour) · category · accent · ink
    cat: "icon",          // how the category shows: ring (a thin inner ring) · fill (the shape) · dot (a corner dot) · icon (the icon colour)
    icon: "line",         // none · line (one line-icon set) · symbol (the project's art)
    icon_ratio: 0.56,     // × the shape (Unity icon_size_ratio 0.56)
    icon_stroke: 1.6,     // px on the 24 grid
    rim_pt: 1,            // the outer rim's width
    rim: "light",         // light (white) · accent · category
    shadow: 0.5,          // contact shadow strength
    label: "always",      // always · focus (only when tapped) · off
    label_style: "chip",  // chip (a glass pill) · halo (the text alone, with a soft halo)
    label_pt: 14, label_face: "body", label_pos: "right",
    status: "ring",       // ring (the outline level's dash rhythm) · off
    status_gap_pt: 3,     // the ring's minimum distance from the shape (it sits at Unity's ring_size_ratio 1.18 × the radius)
    badge: false, badge_ratio: 0.36,   // a corner badge (Unity badge_size_ratio)
    effect: "none",       // none · pulse · halo · ripple · beacon
    effect_strength: 0.6,
    intro: "pop",         // pop · fade
  }, o);
}
D.point_styles = [
  mkPoint("point_style_1", "Glass chip"),
  mkPoint("point_style_2", "Pin dot", { shape: "dot", size_pt: 16, fill: "category", cat: "fill", icon: "none", rim_pt: 2, shadow: 0.6, label_style: "halo", label_pt: 13, status: "off" }),
  mkPoint("point_style_3", "Medallion", { size_pt: 44, fill: "surface", cat: "icon", rim: "light", rim_pt: 1.5, icon_ratio: 0.52, shadow: 0.7, label_pos: "below", label_face: "display", label_pt: 15, effect: "halo", effect_strength: 0.8 }),
  mkPoint("point_style_4", "Tile", { shape: "tile", size_pt: 36, fill: "surface", cat: "icon", rim_pt: 1, shadow: 0.6 }),
    mkPoint("point_style_6", "Tile disc (ages 7–11)", { shape: "disc", size_pt: 40, fill: "tin", cat: "ring", icon: "line", icon_ratio: 0.58, icon_stroke: 2.4, rim: "cobalt", rim_pt: 3, shadow: 0.6, effect: "pulse", effect_strength: 0.6, status: "off", badge: true, badge_ratio: 0.34, label_style: "chip", label_pt: 14 }),   // v19: a cobalt-ringed tin-glaze disc that reads as part of the tile (the same in light and dark: the wall does not change)
];
D.point_default = "point_style_1";
const pointStyleByKey = k => D.point_styles.find(s => s.key === k);
/* the style a POI's point uses: the POI's, else its level's, else the app default */
/* the Point styles section previews its selected style on every POI of the wall demo (so each style, and each of its rows, shows) */
function pointPreview() { return ui.tab === "global" && ui.screen === "wall" && ui.open["pointstyles"] ? D.point_styles[ui.pstyle ?? 0] || null : null; }
function pointStyleOf(p) { const pv = pointPreview(); if (pv) return pv; const lp = typeof D.looks !== "undefined" ? D.ds.marker_preset : null; if (lp && lp !== "by_level" && pointStyleByKey(lp) && !p.world?.point_style) return pointStyleByKey(lp);   // a look may dress every marker (Playful: Storybook)
  return pointStyleByKey(p.world?.point_style) || pointStyleByKey(levelOf(p)?.point_style) || pointStyleByKey(D.point_default) || D.point_styles[0]; }

const symImgs = {};
/* the project's symbols load once at boot; when they arrive every marker repaints (the first paint never shows a badge with no symbol) */
function preloadSymbols() { let left = Object.keys(CAT_SYM).length; for (const k in CAT_SYM) { if (symImgs[k]) { left--; continue; } const im = new Image(); im.onload = im.onerror = () => { if (--left <= 0) { try { if (typeof paintAR === "function" && ui.tab === "app") paintAR(false); if (typeof drawPointSwatches === "function") drawPointSwatches(); } catch (e) {} } }; im.src = CAT_SYM[k]; symImgs[k] = im; } }
const stickers = {};
/* a symbol with a real sticker outline (the art dilated in white), cached per category and size */
function stickerOf(im, key, w, h, rim, dpr) {
  const id = key + "|" + Math.round(w * dpr) + "|" + rim.toFixed(1); if (stickers[id]) return stickers[id];
  const pad = Math.ceil(rim * dpr) + 1, cw = Math.ceil(w * dpr) + 2 * pad, ch = Math.ceil(h * dpr) + 2 * pad, cv3 = document.createElement("canvas"); cv3.width = cw; cv3.height = ch;
  const o = cv3.getContext("2d"), r = rim * dpr;
  for (let i = 0; i < 16; i++) { const a = i / 16 * Math.PI * 2; o.drawImage(im, pad + Math.cos(a) * r, pad + Math.sin(a) * r, w * dpr, h * dpr); }
  o.globalCompositeOperation = "source-in"; o.fillStyle = "#FFFFFF"; o.fillRect(0, 0, cw, ch);
  o.globalCompositeOperation = "source-over"; o.drawImage(im, pad, pad, w * dpr, h * dpr);
  return stickers[id] = { cv: cv3, pad: pad / dpr, dpr };
}
const symImg = k => { if (!CAT_SYM[k]) return null; if (!symImgs[k]) { const im = new Image(); im.src = CAT_SYM[k]; symImgs[k] = im; } return symImgs[k].complete && symImgs[k].naturalWidth ? symImgs[k] : null; };
const iconPaths = {};
const iconPath = k => iconPaths[k] ||= new Path2D(MK_ICONS[k] || MK_ICONS.unknown);
function mkColours() {
  const m = appearance(), T = D.themes[m];
  return { mode: m, glass: rgba(T.glass.color, Math.max(T.glass.tint, 0.82)), surface: T.sheet.color, ink: m === "light" ? "#14110D" : "#F6F1E8", accent: T.accent, onAccent: T.accent_text,
    lbl: rgba(T.labels.color, T.labels.tint), lblInk: T.labels.text, rimLight: "#FFFFFF", tin: "#FFF8EC", cobalt: "#1F3F9A" };
}
/* draws one marker at (x, y) in the context's units; k = pt → those units; st = { a: reveal 0..1, f: focus 0..1, t: clock, alpha } */
function drawMarker(c, x, y, k, p, P, st) {
  const C = mkColours(), FX = "#F2CA71", cat = catColor(p), a = st.a ?? 1, f = st.f || 0, t = st.t || 0, red = reducedMotionWall();
  const grow = P.intro === "pop" && !red ? 0.6 + 0.4 * ease(a) : 1;
  let R = P.size_pt / 2 * k * grow * (1 + 0.14 * f);
  if (P.effect === "pulse" && !red) R *= 1 + 0.05 * P.effect_strength * Math.sin(t * 2 * Math.PI / 1.6);
  c.save(); c.globalAlpha = clamp(a) * (st.alpha ?? 1); c.translate(x, y);
  // effects behind the marker
  if (!red && P.effect === "halo") { const b = 0.5 + 0.5 * Math.sin(t * 2 * Math.PI / 2); c.beginPath(); c.arc(0, 0, R * (1.45 + 0.08 * b), 0, 7); c.fillStyle = rgba(FX, 0.22 * P.effect_strength * (0.7 + 0.3 * b)); c.fill(); }
  if (!red && P.effect === "ripple") for (let i = 0; i < 3; i++) { const q = ((t / 1.8) + i / 3) % 1; c.beginPath(); c.arc(0, 0, R * (1 + q * 1.4), 0, 7); c.strokeStyle = rgba(FX, (1 - q) * 0.6 * P.effect_strength); c.lineWidth = 1.5 * k; c.stroke(); }
  if (!red && P.effect === "beacon") {
    const H = R * 4.2, g = c.createLinearGradient(0, -R, 0, -R - H); g.addColorStop(0, rgba(FX, 0.6 * P.effect_strength)); g.addColorStop(1, rgba(FX, 0));
    c.fillStyle = g; c.beginPath(); c.moveTo(-R * 0.45, -R * 0.6); c.lineTo(R * 0.45, -R * 0.6); c.lineTo(R * 0.06, -R - H); c.lineTo(-R * 0.06, -R - H); c.closePath(); c.fill();
    const q = (t / 2.4) % 1; c.beginPath(); c.arc(0, 0, R * (1 + q * 0.9), 0, 7); c.strokeStyle = rgba(FX, (1 - q) * 0.6 * P.effect_strength); c.lineWidth = 1.5 * k; c.stroke();
  }
  // focus = lift (the marker grows 14 % and its shadow deepens and drops), never one more ring
  if (P.shape !== "none" && P.shadow > 0) { c.beginPath(); c.ellipse(0, R * (0.95 + 0.25 * f), R * (0.75 - 0.1 * f), R * 0.22, 0, 0, 7); c.fillStyle = `rgba(0,0,0,${0.22 * P.shadow * (1 - 0.4 * f)})`; c.filter = `blur(${3 * k}px)`; c.fill(); c.filter = "none"; }   // the contact shadow: it floats off the wall
  // the shape
  const shape = new Path2D();
  if (P.shape === "tile") shape.roundRect(-R, -R, 2 * R, 2 * R, R * 0.32);
  else if (P.shape === "diamond") { shape.moveTo(0, -R * 1.12); shape.lineTo(R * 1.12, 0); shape.lineTo(0, R * 1.12); shape.lineTo(-R * 1.12, 0); shape.closePath(); }
  else if (P.shape !== "none") shape.arc(0, 0, R, 0, 7);
  const fillCol = P.cat === "fill" || P.fill === "category" ? cat : P.fill === "accent" ? C.accent : P.fill === "ink" ? (C.mode === "light" ? "#14110D" : "#1C1A17") : P.fill === "surface" ? C.surface : P.fill === "tin" ? C.tin : C.glass;
  if (P.shape !== "none") {
    c.save(); if (P.shadow > 0) { c.shadowColor = `rgba(0,0,0,${(0.42 + 0.25 * f) * P.shadow})`; c.shadowBlur = (10 + 6 * f) * k; c.shadowOffsetY = (3 + 3 * f) * k; }
    c.fillStyle = fillCol; c.fill(shape); c.restore();
    if (P.fill === "glass") { const g = c.createLinearGradient(0, -R, 0, R); g.addColorStop(0, "rgba(255,255,255,.35)"); g.addColorStop(0.45, "rgba(255,255,255,0)"); g.addColorStop(1, "rgba(0,0,0,.14)"); c.fillStyle = g; c.fill(shape); }
    if (P.fill === "glass" || P.fill === "surface") { c.save(); c.clip(shape); c.beginPath(); c.arc(0, R * 0.08, R - 1.2 * k, Math.PI * 1.15, Math.PI * 1.85); c.strokeStyle = "rgba(255,255,255,.5)"; c.lineWidth = 1 * k; c.stroke(); c.restore(); }   // the top highlight: glass catching light
    if (P.cat === "ring") {   // v19 R6: the category ring sits inside the rim with a clear tin/glass gap (a double frame line, as on an azulejo border), never smeared into the rim
      const rw = 2 * k, ri = (P.rim_pt > 0 ? P.rim_pt * k / 2 + 1.6 * k : 0.4 * k) + rw / 2;
      c.save(); c.clip(shape); c.lineWidth = rw; c.strokeStyle = cat; c.beginPath(); P.shape === "tile" ? c.roundRect(-R + ri, -R + ri, 2 * (R - ri), 2 * (R - ri), Math.max(0, R * 0.32 - ri)) : c.arc(0, 0, R - ri, 0, 7); c.stroke(); c.restore(); }
    if (P.rim_pt > 0 && P.rim === "cobalt") { c.lineWidth = (P.rim_pt + 3) * k; c.strokeStyle = "#FFFFFF"; c.stroke(shape); }   // v19 R6: a 1.5 pt white sticker edge outside the cobalt rim (Playful spec §4): the disc separates from dense cobalt painting
    if (P.rim_pt > 0) { c.lineWidth = P.rim_pt * k; c.strokeStyle = P.rim === "accent" ? C.accent : P.rim === "cobalt" ? C.cobalt : P.rim === "category" ? cat : C.mode === "light" ? "#FFFFFF" : "rgba(255,255,255,.85)"; c.stroke(shape);
      if (P.shape === "dot") { c.beginPath(); c.arc(0, 0, R + P.rim_pt * k / 2 + 0.5 * k, 0, 7); c.lineWidth = 1 * k; c.strokeStyle = "rgba(20,17,13,.6)"; c.stroke(); } }   // a dark hairline: pale dots never vanish on white tiles
  }
  // the icon
  const dark = c => { const v = parseInt(c.slice(1), 16), L = (0.299 * (v >> 16) + 0.587 * (v >> 8 & 255) + 0.114 * (v & 255)) / 255; return L < 0.55; };
  if (P.icon === "line") {
    const s = 2 * R * P.icon_ratio / 24, base = P.fill === "tin" ? C.cobalt : P.fill === "glass" ? (C.mode === "light" ? "#14110D" : "#F6F1E8") : P.fill === "surface" ? (P.rim === "accent" ? C.accent : C.ink) : (dark(fillCol.startsWith("#") ? fillCol : "#888888") ? "#FFFFFF" : "#14110D");
    c.save(); c.translate(-12 * s, -12 * s); c.scale(s, s); c.lineWidth = P.icon_stroke * (24 / (2 * R * P.icon_ratio)) * k * 1.1; c.lineCap = "round"; c.lineJoin = "round";
    c.strokeStyle = P.cat === "icon" ? (C.mode === "light" ? shade(cat, -0.35) : cat) : base; c.stroke(iconPath(CAT_LINE[p.category])); c.restore();
  } else if (P.icon === "symbol") {
    const im = symImg(p.category);
    if (im) { const s = 2 * R * P.icon_ratio / Math.max(im.naturalWidth, im.naturalHeight), w = im.naturalWidth * s, h = im.naturalHeight * s;
      const sk = P.rim_pt > 0 ? stickerOf(im, p.category, w, h, P.rim_pt * k, Math.min(3, (window.devicePixelRatio || 1) * 2)) : null;
      c.save(); c.shadowColor = `rgba(0,0,0,${(0.45 + 0.2 * f) * P.shadow})`; c.shadowBlur = (6 + 4 * f) * k; c.shadowOffsetY = (2 + 3 * f) * k;
      if (sk) c.drawImage(sk.cv, -w / 2 - sk.pad, -h / 2 - sk.pad, w + 2 * sk.pad, h + 2 * sk.pad); else c.drawImage(im, -w / 2, -h / 2, w, h); c.restore(); }
  }
  if (P.cat === "dot" && P.shape !== "none") { c.beginPath(); c.arc(R * 0.68, -R * 0.68, R * 0.24, 0, 7); c.fillStyle = cat; c.fill(); c.lineWidth = 1.2 * k; c.strokeStyle = "#FFFFFF"; c.stroke(); }
  // the status ring: the outline level's dash rhythm, in the accent
  const S = P.status === "ring" ? statusOf(p) : null;
  if (S) {   // one look in both modes (the wall does not change): Unity's gold over a dark under-stroke, so it reads on white and on cobalt
    const Rr = Math.max(R * 1.18, R + P.status_gap_pt * k), ring = () => { c.beginPath(); if (P.shape === "tile") c.roundRect(-Rr, -Rr, 2 * Rr, 2 * Rr, Rr * 0.34); else c.arc(0, 0, Rr, 0, 7); };
    c.setLineDash(S.dash.map(v => v * k)); c.lineCap = "round";
    ring(); c.lineWidth = 3.5 * k; c.strokeStyle = "rgba(20,17,13,.55)"; c.stroke();
    ring(); c.lineWidth = 2 * k; c.strokeStyle = "#E3BD72"; c.stroke(); c.setLineDash([]); }
  // the badge: the status level's mark in a corner. v19 R6: at least 16 pt across with a 2 pt glyph, so the mark reads at arm's length; "destroyed" is a crack, not a ×
  // (a red × on a marker reads as "close" or "error", to a child most of all)
  if (P.badge && statusOf(p)) { const br = Math.max(R * P.badge_ratio, 8 * k), bx = R * 0.74, by = -R * 0.74, key = p.status;
    c.beginPath(); c.arc(bx, by, br, 0, 7); c.fillStyle = key === "intact" ? "#2E7D32" : key === "destroyed" ? "#A62B1F" : key === "partial_damage" ? "#A35400" : "#5E5E5E"; c.fill(); c.lineWidth = 1.5 * k; c.strokeStyle = "#FFFFFF"; c.stroke();
    c.strokeStyle = "#FFFFFF"; c.fillStyle = "#FFFFFF"; c.lineWidth = Math.max(1.6, br * 0.2); c.lineCap = "round"; c.lineJoin = "round"; c.beginPath();
    // a distinct mark per level, never colour alone: ✓ intact · half disc partial · crack destroyed · ? unknown
    if (key === "intact") { c.moveTo(bx - br * 0.42, by + br * 0.02); c.lineTo(bx - br * 0.1, by + br * 0.34); c.lineTo(bx + br * 0.44, by - br * 0.32); c.stroke(); }
    else if (key === "partial_damage") { c.arc(bx, by, br * 0.5, 0, 7); c.lineWidth = Math.max(1.2, br * 0.14); c.stroke(); c.beginPath(); c.arc(bx, by, br * 0.5, Math.PI / 2, Math.PI * 1.5); c.closePath(); c.fill(); }
    else if (key === "destroyed") { c.save(); c.beginPath(); c.arc(bx, by, br - 0.75 * k, 0, 7); c.clip(); c.beginPath();   // a crack splitting the disc edge to edge
      for (const [u, v] of [[-0.1, -1], [0.16, -0.48], [-0.14, -0.06], [0.2, 0.36], [0.02, 1]]) c.lineTo(bx + u * br, by + v * br); c.stroke(); c.restore(); }
    else { c.font = `700 ${br * 1.3}px "Atkinson Hyperlegible Next", sans-serif`; c.textAlign = "center"; c.textBaseline = "middle"; c.fillText("?", bx, by + br * 0.06); } }
  c.restore();
}
const shade = (hex, f) => { const v = parseInt(hex.slice(1), 16), ch = [v >> 16, v >> 8 & 255, v & 255].map(x => Math.round(f < 0 ? x * (1 + f) : x + (255 - x) * f)); return "#" + ch.map(x => x.toString(16).padStart(2, "0")).join(""); };
const reducedMotionWall = () => D.test.reduce_motion || (typeof reducedMotion === "function" && reducedMotion());
/* v19 R6: a wall label's size follows the look's text size and the visitor's (S 0.9 · M 1 · L 1.2), clamped like every text; 13 pt stays the floor */
const mkLabelPt = P => Math.max(13, P.label_pt * Math.min(1.35, Math.max(0.85, typeof dsT === "function" ? dsT().text : 1)));
/* the label beside a marker, the style's way: a glass chip or the text with a halo.
   v19 R6: it stays on the screen: a right-hand label that would run off the right edge flips to the marker's left, a centred one slides
   inside the edges, and one below that would run off the bottom goes above (Unity: the same test on the label's screen rect, per frame) */
function drawMarkerLabel(c, text, x, y, k, P, alpha, anchor = "left", mk) {
  if (alpha <= 0) return; const C = mkColours(), pt = mkLabelPt(P);
  c.save(); c.globalAlpha = alpha; c.font = `${P.label_face === "display" ? 600 : 700} ${pt * k}px ${P.label_face === "display" ? '"Fraunces", Georgia, serif' : '"Atkinson Hyperlegible Next", system-ui, sans-serif'}`;
  const tw = c.measureText(text).width, padX = 10 * k, hgt = (pt + 12) * k, w = P.label_style === "chip" ? tw + 2 * padX : tw + 0.6 * padX;
  let lx = anchor === "center" ? x - w / 2 : anchor === "right" ? x - w : x;
  // the marker this label belongs to (the caller may pass it; else the POI drawn at this height)
  mk ||= D.pois.find(q => Math.abs(q.pos.y - y) < 0.01 && x > q.pos.x && x - q.pos.x < 80 * k) || (anchor === "center" ? D.pois.find(q => Math.abs(q.pos.x - x) < 0.01 && y > q.pos.y) : null);
  if (typeof camProject === "function" && ui.screen !== "app") {
    const a = camProject(lx, y), b = camProject(lx + 100, y + 100), sx = (b.x - a.x) / 100, sy = (b.y - a.y) / 100, M = 2.5;   // screen % per wall px (the demos keep the camera level)
    if (sx > 0 && sy > 0) {
      const right = a.x + w * sx, bottom = a.y + hgt / 2 * sy, left = a.x;
      if (anchor === "left" && mk?.pos && right > 100 - M) { const flipped = mk.pos.x - (lx - mk.pos.x) - w; if (camProject(flipped, y).x >= M) lx = flipped; else lx -= (right - (100 - M)) / sx; }
      else if (anchor === "left" && right > 100 - M) lx -= Math.min(right - (100 - M), Math.max(0, left - M)) / sx;   // a region's corner label slides in
      else if (anchor === "center") { if (right > 100 - M) lx -= (right - (100 - M)) / sx; else if (left < M) lx += (M - left) / sx;
        if (mk?.pos && bottom > 100 - M) y = mk.pos.y - (y - mk.pos.y); }
    }
  }
  c.textBaseline = "middle";
  if (P.label_style === "chip") {
    c.save(); c.shadowColor = "rgba(0,0,0,.25)"; c.shadowBlur = 6 * k; c.shadowOffsetY = 1.5 * k; c.fillStyle = C.lbl; c.beginPath(); c.roundRect(lx, y - hgt / 2, w, hgt, hgt / 2); c.fill(); c.restore();
    c.lineWidth = 1 * k; c.strokeStyle = C.mode === "light" ? "rgba(20,17,13,.08)" : "rgba(255,255,255,.12)"; c.stroke();
    c.fillStyle = C.lblInk; c.fillText(text, lx + padX, y + 0.5 * k);
  } else {
    const halo = C.mode === "light" ? "rgba(255,255,255,.92)" : "rgba(10,8,6,.85)";
    c.lineJoin = "round"; c.lineWidth = 4 * k; c.strokeStyle = halo; c.strokeText(text, lx + padX * 0.3, y);
    c.fillStyle = C.mode === "light" ? "#14110D" : "#FFFFFF"; c.fillText(text, lx + padX * 0.3, y);
  }
  c.restore();
}
/* the wall screen: a POI's point (replaces the old category dot) */
function drawPoint(env, poi, a, small) {
  const P = pointStyleOf(poi), k = (dev.U / 3.9) / scale * (small ? 0.75 : 1);
  drawMarker(env.ctx, poi.pos.x, poi.pos.y, k, poi, P, { a: a.k, f: a.f, t: performance.now() / 1000, alpha: a.alpha });
}
/* the app: one canvas per marker (the same routine), placed on the phone */
/* markers with an effect keep moving on the phone too (one shared loop; Reduce Motion freezes them) */
const liveMarkers = new Set(); let liveRaf = 0;
function liveTick() { liveRaf = 0; if (!liveMarkers.size) return; const t = performance.now() / 1000;
  for (const m of [...liveMarkers]) { if (!m.cv.isConnected) { liveMarkers.delete(m); continue; } const c = m.cv.getContext("2d"); c.setTransform(m.dpr, 0, 0, m.dpr, 0, 0); c.clearRect(0, 0, m.side, m.side); drawMarker(c, m.side / 2, m.cy, m.k, m.p, m.P, { a: 1, f: m.f, t }); }
  if (liveMarkers.size && !reducedMotionWall()) liveRaf = requestAnimationFrame(liveTick); }
function markerCanvas(p, P, f = 0) {
  const k = dev.U / 3.9, dpr = Math.min(2, window.devicePixelRatio || 1), ext = P.effect === "beacon" ? 5.4 : P.effect === "none" ? 1.6 : 2.6;
  const side = Math.ceil((P.size_pt * ext + 24) * k), cv2 = document.createElement("canvas");
  cv2.width = side * dpr; cv2.height = side * dpr; cv2.className = "mkc"; cv2.style.width = cv2.style.height = side + "px";
  const c = cv2.getContext("2d"); c.scale(dpr, dpr); drawMarker(c, side / 2, side / 2 + (P.effect === "beacon" ? P.size_pt * 1.4 * k : 0), k, p, P, { a: 1, f, t: 0.4 });
  if (P.effect === "beacon") cv2.style.marginTop = -P.size_pt * 1.4 * k + "px";
  if (P.effect !== "none" && !reducedMotionWall()) { liveMarkers.add({ cv: cv2, side, cy: side / 2 + (P.effect === "beacon" ? P.size_pt * 1.4 * k : 0), k, p, P, f, dpr }); if (!liveRaf) liveRaf = requestAnimationFrame(liveTick); }
  return cv2;
}

/* ---------- the editor: Global Scene › Looks › Point styles ---------- */
function pointStylesBody() {
  const out = [...row("Default for the whole app", fSelect(D, "point_default", D.point_styles.map(s => [s.key, s.name]), () => { ui.pstyle = D.point_styles.findIndex(s => s.key === D.point_default); renderEditor(); pointDemo(); }), "Every POI shown as a point uses this style unless its level (Levels › Point style) or the POI itself picks another.")];
  const tb = h("tbody");
  D.point_styles.forEach((s, i) => tb.append(h("tr", { class: ui.pstyle === i ? "sel" : "", onclick: e => { if (e.target.closest("button,input,select")) return; ui.pstyle = i; renderEditor(); pointDemo(); } },
    h("td", {}, h("canvas", { class: "psw", "data-ps": s.key, width: 72, height: 40 })), h("td", {}, s.name), h("td", { class: "num" }, String(usedPoint(s.key))),
    h("td", {}, h("button", { class: "ib", title: "Duplicate", onclick: () => { snap(); D.point_styles.push({ ...JSON.parse(JSON.stringify(s)), key: newKey(D.point_styles, "point_style_"), name: s.name + " copy" }); changed(); } }, "⧉")))));
  out.push(h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th"), h("th", {}, "Style"), h("th", {}, "Used"), h("th"))), tb));
  const P = D.point_styles[ui.pstyle ?? 0];
  if (P) out.push(fold("pstyle_body", "Point style · " + P.name, { color: "#8FE0E5", screen: "wall", demo: { screen: "wall" } }, () => pointStyleBody(P)));
  return out;
}
const usedPoint = k => D.pois.filter(p => (pointStyleByKey(p.world?.point_style) || pointStyleByKey(levelOf(p)?.point_style) || pointStyleByKey(D.point_default)).key === k && presenceOf(p) !== "region").length;
function pointDemo() {   // the whole wall, so every POI shows the previewed style
  setScreen("wall"); const z = Math.max(1, Math.min(dev.SW / IMA, dev.SH) / dev.base);
  camTo(camCover({ cx: 0.5, cy: 0.5, z, r: 0, yaw: 0, pitch: 0 }), 300); renderTapWall?.(); }
function pointStyleBody(P) {
  const re = () => { renderTapWall(); drawPointSwatches(); }, sel = (key, opts, help) => row(key === "cat" ? "Category shows as" : key[0].toUpperCase() + key.slice(1).replace("_", " "), fSelect(P, key, opts, re), help);
  const rng = (label, key, mn, mx, st, unit, help) => row(label, id => { const f = fRange(P, key, mn, mx, st, unit)(id); f.addEventListener("input", re); return f; }, help);
  return [
    ...row("Name", fText(P, "name")),
    h("div", { class: "grp" }, "Shape"),
    ...sel("shape", [["disc", "Disc"], ["tile", "Rounded tile"], ["diamond", "Diamond"], ["dot", "Dot"], ["none", "None (the icon floats)"]]),
    ...rng("Size", "size_pt", 10, 60, 1, " pt", "On screen, at every distance (Unity's LOD keeps markers a constant size); the tap area is 48 pt whatever the size."),
    ...sel("fill", [["glass", "Glass (the app's)"], ["surface", "Panel colour"], ["category", "Category colour"], ["accent", "Accent"], ["ink", "Ink"], ["tin", "Tin-glaze white (both modes)"]]),
    ...sel("cat", [["ring", "A thin inner ring"], ["fill", "The shape's fill"], ["dot", "A corner dot"], ["icon", "The icon's colour"]], "Never colour alone: the icon names the category too."),
    ...rng("Rim", "rim_pt", 0, 4, 0.5, " pt"), ...(P.rim_pt > 0 ? sel("rim", [["light", "Light"], ["accent", "Accent"], ["category", "Category"], ["cobalt", "Cobalt (both modes)"]]) : []),
    ...rng("Shadow", "shadow", 0, 1, 0.05),
    h("div", { class: "grp" }, "Icon"),
    ...sel("icon", [["line", "Line icon (one set, 24 grid)"], ["symbol", "The project's symbol art"], ["none", "None"]]),
    ...(P.icon !== "none" ? rng("Icon size", "icon_ratio", 0.3, 1, 0.02, "×") : []), ...(P.icon === "line" ? rng("Icon line", "icon_stroke", 1, 2.5, 0.1, " px") : []),
    h("div", { class: "grp" }, "Label"),
    ...sel("label", [["always", "Always"], ["focus", "Only when tapped"], ["off", "Off"]]),
    ...(P.label !== "off" ? [...sel("label_style", [["chip", "Glass chip"], ["halo", "Text with a halo"]]), ...rng("Label size", "label_pt", 13, 20, 0.5, " pt", "13 pt is the floor (the app's rule)."),
      ...sel("label_face", [["body", "Atkinson (body)"], ["display", "Fraunces (display)"]]), ...sel("label_pos", [["right", "Right"], ["below", "Below"]])] : []),
    h("div", { class: "grp" }, "Status and badge", h("span", {}, "the wall's outline levels")),
    ...sel("status", [["ring", "Ring (dash rhythm per level)"], ["off", "Off"]], "Intact solid · partial damage long dashes · destroyed short dashes · unknown dotted (Unity outline_levels)."),
    ...(P.status === "ring" ? rng("Ring gap", "status_gap_pt", 1, 8, 0.5, " pt") : []),
    ...row("Badge", fToggle(P, "badge", re)), ...(P.badge ? rng("Badge size", "badge_ratio", 0.25, 0.5, 0.01, "×") : []),
    h("div", { class: "grp" }, "Effect"),
    ...sel("effect", [["none", "None"], ["pulse", "Pulse (breathes)"], ["halo", "Halo"], ["ripple", "Ripple rings"], ["beacon", "Beacon (a light shaft)"]], "One animated effect at a time; it stops under Reduce Motion."),
    ...(P.effect !== "none" ? rng("Strength", "effect_strength", 0.1, 1, 0.05) : []),
    ...sel("intro", [["pop", "Pop in"], ["fade", "Fade in"]]),
  ];
}
function drawPointSwatches() {
  document.querySelectorAll("canvas.psw").forEach(cv2 => { const P = pointStyleByKey(cv2.dataset.ps); if (!P) return; const c = cv2.getContext("2d");
    c.setTransform(1, 0, 0, 1, 0, 0); c.clearRect(0, 0, cv2.width, cv2.height); c.fillStyle = "#3A4250"; c.fillRect(0, 0, cv2.width, cv2.height);
    const k = Math.min(1, 30 / (P.size_pt * (P.status === "ring" ? 1.25 : 1))); drawMarker(c, 36, 20, k, D.pois[0], P, { a: 1, t: 0.4 }); });
}
