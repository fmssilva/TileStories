/* ---------- scene input ---------- */
function toImg(e) { return unproject(e.clientX, e.clientY); }   // through the camera (side views included)
function snap45(p, from) { if (!shiftDown || !from) return p; const dx = p.x - from.x, dy = p.y - from.y, d = Math.hypot(dx, dy), a = Math.round(Math.atan2(dy, dx) / (Math.PI / 4)) * (Math.PI / 4); return { x: from.x + Math.cos(a) * d, y: from.y + Math.sin(a) * d }; }
function nearShape(S, p, tol) {
  const g = build(S);
  if (S.type === "points") return S.knots.some(q => Math.hypot(q.x - p.x, q.y - p.y) < tol * 1.5);
  const path = toPath(g.pts, g.closed); ctx.save(); ctx.setTransform(1, 0, 0, 1, 0, 0); ctx.lineWidth = tol * 1.4;
  const r = (g.closed && ctx.isPointInPath(path, p.x, p.y)) || ctx.isPointInStroke(path, p.x, p.y); ctx.restore(); return r;
}
function hitPoi(p) {
  const tol = 12 / scale;
  for (let pi = D.pois.length - 1; pi >= 0; pi--) {
    const P = D.pois[pi];
    if (playing && typeof shownByTime === "function" && !shownByTime(P)) continue;   // hidden by the visitor's Time: not tappable either
    if (Math.hypot(P.pos.x - p.x, P.pos.y - p.y) < tol && (presenceOf(P) !== "region" || !playing)) return { pi, si: -1 };
    for (let si = P.world.shapes.length - 1; si >= 0; si--) if (nearShape(P.world.shapes[si], p, tol)) return { pi, si };
  }
  return null;
}
function hitEdit(p) {
  const tol = 10 / scale, P = D.pois[ui.poi], S = P.world.shapes[ui.shape];
  if (S && S.type !== "points") for (const m of build(S).mids) if (Math.hypot(m.p[0] - p.x, m.p[1] - p.y) < tol) return { type: "mid", seg: m.seg };
  for (let si = P.world.shapes.length - 1; si >= 0; si--) { const Kn = P.world.shapes[si].knots; for (let i = Kn.length - 1; i >= 0; i--) if (Math.hypot(Kn[i].x - p.x, Kn[i].y - p.y) < tol) return { type: "knot", si, i }; }
  if (Math.hypot(P.pos.x - p.x, P.pos.y - p.y) < tol) return { type: "pos" };
  for (let si = P.world.shapes.length - 1; si >= 0; si--) if (nearShape(P.world.shapes[si], p, tol)) return { type: "shape", si };
  const o = hitPoi(p); if (o) return { type: "other", pi: o.pi, si: Math.max(0, o.si) };
  return null;
}
function newShape(type) {
  const P = D.pois[ui.poi], used = new Set(P.world.shapes.map(s => s.key));
  let n = 1; while (used.has("shape_" + n)) n++;
  const nm = { polygon: "Polygon", path: "Path", points: "Points" }[type];
  P.world.shapes.push({ key: "shape_" + n, name: nm + " " + n, type, style: "", default_corner: type === "path" ? "smooth" : "sharp", knots: [] });
  ui.shape = P.world.shapes.length - 1; ui.knot = -1; ui.open["shape"] = true;
}
function finishDrawing(cancel) {
  if (!drawing) return;
  const P = D.pois[ui.poi], S = P.world.shapes[ui.shape], min = S.type === "polygon" ? 3 : S.type === "path" ? 2 : 1;
  if (cancel || S.knots.length < min) { if (S.knots.length < min) { P.world.shapes.splice(ui.shape, 1); ui.shape = Math.max(0, P.world.shapes.length - 1); ui.knot = -1; } }
  drawing = false; setTool("edit");
}
function selectPoi(pi, si) { ui.poi = pi; ui.shape = si; ui.knot = -1; ui.open["poi_" + pi] = true; ui.open["region_" + pi] = true; }
function tapPlay(p) {
  const h = hitPoi(p), now = performance.now() / 1000;
  if (h) { if (focus !== h.pi) { focus = h.pi; focusT0 = now; const S = D.pois[h.pi].world.shapes.find(s => styleFor(D.pois[h.pi], s)), st = S && styleFor(D.pois[h.pi], S); if (st && st.motion.focus.preset === "redraw" && !D.test.reduce_motion) poiStart[h.pi] = focusT0; openSheet(D.pois[h.pi]); } }
  else if (focus !== null) { focus = null; unfocusT0 = now; closeSheet(); }
}

cv.addEventListener("pointerdown", e => {
  const p = toImg(e); cursor = p;
  if (ui.screen === "style" || ui.screen === "precision" || ui.screen === "app") return;
  if (playing) { cv._tap = p; return; }   // a tap, unless the pointer moves (then it was a look-around drag)
  if (ui.screen === "life") {
    const hit = allContent().reverse().find(o => { const b = anchorBox(o); return b && p.x >= b.x0 - 4 && p.x <= b.x1 + 4 && p.y >= b.y0 - 12 && p.y <= b.y1 + 4; });
    if (hit) selectLife(hit);
    else if (guestLabelAt && p.x >= guestLabelAt.x && p.x <= guestLabelAt.x1 && Math.abs(p.y - guestLabelAt.y) < 12 / scale) selectLife(guestLabelAt.o);
    return;
  }
  cv.setPointerCapture(e.pointerId);
  if (ui.tab !== "specific") { const h = hitPoi(p); if (h && h.si >= 0) { const st = styleFor(D.pois[h.pi], D.pois[h.pi].world.shapes[h.si]); const i = D.styles.indexOf(st); if (i >= 0) { ui.tab = "global"; ui.style = i; ui.open["styles"] = true; ui.open["style_body"] = true; renderEditor(); } } return; }
  if (ui.tool === "rect") { rectStart = p; return; }
  if (ui.tool !== "edit") {
    if (!drawing) { snap(); newShape(ui.tool); drawing = true; }
    const S = D.pois[ui.poi].world.shapes[ui.shape];
    if (S.type === "polygon" && S.knots.length >= 3 && Math.hypot(S.knots[0].x - p.x, S.knots[0].y - p.y) < 10 / scale) { finishDrawing(false); return; }
    const q = S.type === "points" ? p : snap45(p, S.knots.at(-1)); S.knots.push(K(q.x, q.y)); ui.knot = S.knots.length - 1; renderEditor(); return;
  }
  const h = hitEdit(p);
  if (!h) { ui.knot = -1; renderEditor(); return; }
  if (h.type === "other") { selectPoi(h.pi, h.si); renderEditor(); scrollToPoi(); return; }
  snap();
  const S = D.pois[ui.poi].world.shapes[ui.shape];
  if (h.type === "mid") { S.knots.splice(h.seg + 1, 0, K(p.x, p.y)); ui.knot = h.seg + 1; drag = { i: h.seg + 1 }; }
  else if (h.type === "knot") { ui.shape = h.si; ui.knot = h.i; drag = { i: h.i }; }
  else if (h.type === "pos") drag = { pos: true };
  else { ui.shape = h.si; ui.knot = -1; drag = { whole: true, last: p }; }
  ui.open["shape"] = true; renderEditor();
});
cv.addEventListener("pointermove", e => {
  const p = toImg(e); cursor = p;
  if (ui.screen !== "wall" || playing) { cv.style.cursor = (playing && hitPoi(p)) || (ui.screen === "life" && !playing) ? "pointer" : "default"; return; }
  if (drag) {
    const P = D.pois[ui.poi], S = P.world.shapes[ui.shape];
    if (drag.pos) { P.pos.x = p.x; P.pos.y = p.y; }
    else if (drag.whole) { const dx = p.x - drag.last.x, dy = p.y - drag.last.y; S.knots.forEach(q => { q.x += dx; q.y += dy; }); drag.last = p; }
    else { const n = S.knots.length, from = S.type === "points" || (S.type === "path" && drag.i === 0) ? null : S.knots[(drag.i - 1 + n) % n]; const q = snap45(p, from); S.knots[drag.i].x = q.x; S.knots[drag.i].y = q.y; }
    return;
  }
  if (ui.tab === "specific" && ui.tool === "edit") { hover = hitEdit(p); cv.style.cursor = hover ? (hover.type === "shape" ? "move" : hover.type === "other" ? "pointer" : "grab") : "default"; }
  else cv.style.cursor = ui.tab === "specific" ? "crosshair" : (hitPoi(p) ? "pointer" : "default");
});
cv.addEventListener("pointerup", e => {
  if (playing && cv._tap) { const p = cv._tap; cv._tap = null; if (!dev.moved) tapPlay(p); return; }
  if (rectStart) {
    const p = toImg(e), x0 = Math.min(p.x, rectStart.x), x1 = Math.max(p.x, rectStart.x), y0 = Math.min(p.y, rectStart.y), y1 = Math.max(p.y, rectStart.y);
    rectStart = null;
    if (x1 - x0 > 6 && y1 - y0 > 6) { snap(); newShape("polygon"); D.pois[ui.poi].world.shapes[ui.shape].knots = [K(x0,y0),K(x1,y0),K(x1,y1),K(x0,y1)]; D.pois[ui.poi].world.shapes[ui.shape].name = "Rectangle " + (ui.shape + 1); setTool("edit"); }
    return;
  }
  if (drag) { drag = null; renderEditor(); }
});
cv.addEventListener("pointerleave", () => { if (!drag) { hover = null; cursor = null; } });
cv.addEventListener("dblclick", () => { if (drawing) finishDrawing(false); });

window.addEventListener("keydown", e => {
  if (e.key === "Shift") shiftDown = true;
  if (e.target.matches("input,select,textarea")) return;
  const mod = e.ctrlKey || e.metaKey, key = e.key.toLowerCase();
  if (mod && key === "z") { e.preventDefault(); e.shiftKey ? redo() : undo(); return; }
  if (mod && key === "y") { e.preventDefault(); redo(); return; }
  if (mod && key === "p") { e.preventDefault(); togglePlay(); return; }
  if (mod || playing || ui.tab !== "specific" || ui.screen !== "wall") return;
  const S = D.pois[ui.poi].world.shapes[ui.shape], q = S && S.knots[ui.knot];
  if (e.key === "Enter") finishDrawing(false);
  else if (e.key === "Escape") { if (drawing) finishDrawing(true); else { ui.knot = -1; renderEditor(); } }
  else if ((e.key === "Delete" || e.key === "Backspace") && q) { e.preventDefault(); removeKnot(); }
  else if (e.key.startsWith("Arrow") && q) { e.preventDefault(); snap(); const d = e.shiftKey ? 5 : 1; if (e.key === "ArrowLeft") q.x -= d; if (e.key === "ArrowRight") q.x += d; if (e.key === "ArrowUp") q.y -= d; if (e.key === "ArrowDown") q.y += d; renderEditor(); }
  else if (q && S.type !== "points" && ({ s: 1, o: 1, c: 1 })[key]) { snap(); q.corner = { s: "sharp", o: "rounded", c: "smooth" }[key]; renderEditor(); }
});
window.addEventListener("keyup", e => { if (e.key === "Shift") shiftDown = false; });
function removeKnot() {
  const P = D.pois[ui.poi], S = P.world.shapes[ui.shape], min = S.type === "polygon" ? 3 : S.type === "path" ? 2 : 1;
  snap();
  if (S.knots.length <= min) { P.world.shapes.splice(ui.shape, 1); ui.shape = Math.max(0, P.world.shapes.length - 1); ui.knot = -1; }
  else { S.knots.splice(ui.knot, 1); ui.knot = Math.min(ui.knot, S.knots.length - 1); }
  renderEditor();
}

