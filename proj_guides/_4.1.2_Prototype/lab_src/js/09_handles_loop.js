/* ---------- handles (wall screen, Specific Marker) ---------- */
function handle(x, y, kind, sel, hov, ring, faint) {
  const u = 1 / scale, r = (kind === "plus" ? 5 : 6) * u * (hov || ring ? 1.25 : 1);
  ctx.save(); ctx.translate(x, y); ctx.globalAlpha = faint ? 0.55 : 1; ctx.lineWidth = 1.5 * u; ctx.strokeStyle = "#0A0D13";
  ctx.fillStyle = sel ? "#F2C14E" : kind === "plus" ? "rgba(255,255,255,.8)" : "#FFFFFF";
  ctx.beginPath();
  if (kind === "sharp") ctx.rect(-r, -r, 2 * r, 2 * r); else if (kind === "rounded") ctx.roundRect(-r, -r, 2 * r, 2 * r, r * .55);
  else if (kind === "point") { ctx.moveTo(0, -r * 1.2); ctx.lineTo(r * 1.2, 0); ctx.lineTo(0, r * 1.2); ctx.lineTo(-r * 1.2, 0); ctx.closePath(); }
  else ctx.arc(0, 0, r, 0, 7);
  ctx.fill(); ctx.stroke();
  if (kind === "plus") { ctx.beginPath(); ctx.moveTo(-r * .55, 0); ctx.lineTo(r * .55, 0); ctx.moveTo(0, -r * .55); ctx.lineTo(0, r * .55); ctx.stroke(); }
  if (ring) { ctx.beginPath(); ctx.arc(0, 0, r * 1.9, 0, 7); ctx.strokeStyle = "#F2C14E"; ctx.stroke(); }
  ctx.restore();
}
function drawHandles() {
  const P = D.pois[ui.poi]; if (!P) return;
  const sel = P.world.shapes[ui.shape];
  if (sel && sel.type !== "points" && ui.tool === "edit" && !drawing) { const g = build(sel); ctx.save(); ctx.setLineDash([4 / scale, 3 / scale]); ctx.strokeStyle = "rgba(242,193,78,.9)"; ctx.lineWidth = 1.2 / scale; ctx.stroke(toPath(g.pts, g.closed)); ctx.restore(); for (const m of g.mids) handle(m.p[0], m.p[1], "plus", false, hover?.type === "mid" && hover.seg === m.seg); }
  P.world.shapes.forEach((S, si) => S.knots.forEach((q, i) => handle(q.x, q.y, S.type === "points" ? "circle" : cornerOf(S, i), si === ui.shape && i === ui.knot, hover?.type === "knot" && hover.si === si && hover.i === i, drawing && si === ui.shape && i === 0 && S.type === "polygon" && S.knots.length >= 3, si !== ui.shape)));
  handle(P.pos.x, P.pos.y, "point", false, hover?.type === "pos", false, false);
  if (drawing && cursor) { const S = sel, last = S?.knots.at(-1); if (last && S.type !== "points") { const c = snap45(cursor, last); ctx.save(); ctx.setLineDash([5 / scale, 4 / scale]); ctx.strokeStyle = "#fff"; ctx.lineWidth = 1.5 / scale; ctx.beginPath(); ctx.moveTo(last.x, last.y); ctx.lineTo(c.x, c.y); ctx.stroke(); ctx.restore(); } }
}

function loop() { try { draw(); if (ui.screen === "wall") updateHud(); } catch (e) { console.error(e); } requestAnimationFrame(loop); }

