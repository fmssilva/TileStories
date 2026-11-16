const cv = document.getElementById("cv"), ctx = cv.getContext("2d"), off = document.createElement("canvas"), off2 = document.createElement("canvas"), off3 = document.createElement("canvas");
const img = new Image(); img.src = SRC;
let scale = 1, dprNow = 1;
const stageEl = document.getElementById("stage");
function fit() {   // the canvas is the wall at the camera's distance: its size follows the camera's zoom (capped at ~9 Mpx)
  const { imgW, imgH } = camSize(), dpr = Math.min(window.devicePixelRatio || 1, Math.sqrt(9e6 / (imgW * imgH)));
  if (Math.abs(imgW - (fit.w || 0)) < 0.5 && dpr === fit.d) return;
  fit.w = imgW; fit.d = dpr;
  cv.style.width = imgW + "px"; cv.style.height = imgH + "px";
  cv.width = Math.round(imgW * dpr); cv.height = Math.round(imgH * dpr); scale = imgW / IW; dprNow = dpr;
  if (typeof draw === "function" && img.complete) draw();
}

function poiTimes(pi, now) {
  const p = D.pois[pi], start = poiStart[pi] ?? (playT0 + (levelOf(p)?.reveal_delay_s || 0));
  return { start, k: playing ? clamp((now - start) / 0.4) : 1 };
}
function focusAmount(now, poi) {
  if (!playing || focus === null || D.pois[focus] !== poi) return 0;
  const S = poi.world.shapes.find(s => styleFor(poi, s)), st = S && styleFor(poi, S), d = st ? st.motion.focus.duration_s : 0.4;
  return D.test.reduce_motion ? 1 : ease((now - focusT0) / Math.max(0.05, d));
}
function spotlight(s, shapes, dim, fallback, ex, ey) {
  off.width = cv.width; off.height = cv.height; const o = off.getContext("2d");
  o.setTransform(s, 0, 0, s, 0, 0); o.fillStyle = `rgba(4,6,10,${dim})`; o.fillRect(0, 0, IW, IH);
  o.globalCompositeOperation = "destination-out"; o.filter = `blur(${6 * scale * dprNow}px)`; o.translate(ex, ey);
  let any = false;
  for (const S of shapes) { const g = build(S); if (g.closed) { o.fill(toPath(offsetPts(g.pts, 3 * PXCM, true), true)); any = true; } }
  if (!any && fallback) { o.beginPath(); o.arc(fallback.x, fallback.y, 30, 0, 7); o.fill(); }
  ctx.save(); ctx.setTransform(1, 0, 0, 1, 0, 0); ctx.drawImage(off, 0, 0); ctx.restore();
}

/* the wall layer: regions, points and labels as the visitor sees them (wall and wall-life screens) */
function drawWorld(env, now, s, errX, errY, opts = {}) {
  const fpoi = playing && focus !== null ? D.pois[focus] : null;
  const fS = fpoi && fpoi.world.shapes.find(x => styleFor(fpoi, x)), fst = fS && styleFor(fpoi, fS);
  if (fpoi && fst && fst.motion.focus.preset === "spotlight" && presenceOf(fpoi) !== "point")
    spotlight(s, fpoi.world.shapes.filter(x => !isQuiet(styleFor(fpoi, x))), fst.motion.focus.amount * focusAmount(now, fpoi), fpoi.pos, errX, errY);
  ctx.save(); ctx.translate(errX, errY);
  D.pois.forEach((p, pi) => {
    if (playing && !shownByTime(p)) return;   // the visitor's Time (Quick: top levels only) in Play
    const pres = presenceOf(p), tm = poiTimes(pi, now), f = focusAmount(now, p);
    const dimOthers = fpoi && fpoi !== p ? 0.55 : 1;
    const showRegion = pres !== "point" && p.world.shapes.some(x => !isQuiet(styleFor(p, x)));
    const authorView = !playing && ui.screen === "wall" && ui.tab === "specific" && pi === ui.poi;
    if (opts.regions !== false) p.world.shapes.forEach((S, si) => {
      const st = styleFor(p, S);
      if (!showRegion) { if (authorView) dashed(build(S), "rgba(255,255,255,.45)"); return; }
      if (!st) { if (!playing) dashed(build(S), "#E86B5E"); return; }
      const T = playing ? now - (tm.start + si * 0.25) : Infinity;
      if (T < 0) return;
      if (isQuiet(st) && !playing && ui.screen === "wall") dashed(build(S), "rgba(160,200,255,.6)");
      drawRegion(env, p, S, st, { T, t: now - playT0, idle: playing, f, alpha: dimOthers });
    });
    const st0 = fS && fpoi === p ? fst : null;
    if (showRegion && st0 && st0.motion.focus.preset === "lock_on" && f > 0) {
      const b = bboxOf(p), pad = 6 * PXCM, sc2 = 1 + 0.5 * (1 - f), cx = (b.x0 + b.x1) / 2, cy = (b.y0 + b.y1) / 2;
      const hw = ((b.x1 - b.x0) / 2 + pad) * sc2, hh = ((b.y1 - b.y0) / 2 + pad) * sc2, Lb = Math.min(hw, hh) * 0.35;
      ctx.save(); ctx.globalAlpha = f; ctx.strokeStyle = st0.line.color_mode === "category" ? catColor(p) : st0.line.color; ctx.lineWidth = 2.2 / scale; ctx.lineCap = "round";
      for (const [sx, sy] of [[-1,-1],[1,-1],[1,1],[-1,1]]) { const x = cx + sx * hw, y = cy + sy * hh; ctx.beginPath(); ctx.moveTo(x - sx * Lb, y); ctx.lineTo(x, y); ctx.lineTo(x, y - sy * Lb); ctx.stroke(); }
      ctx.restore();
    }
    const pv = pointPreview();   // previewing a point style: every POI shows it, full size
    if (!showRegion || pres === "both" || pv) drawPoint(env, p, { k: tm.k, f: showRegion && !pv ? 0 : f, alpha: dimOthers }, pres === "both" && !pv);
    // the label: the point style's (chip or halo, size, face, side); a region-only POI labels its region's top-left corner
    const PS = pointStyleOf(p), kk = dev.U / 3.9 / scale, Rm = PS.size_pt / 2 * kk * (pres === "both" && !pv ? 0.75 : 1);
    if (levelOf(p)?.layers.label !== false && PS.label !== "off" && (PS.label !== "focus" || f > 0)) {
      if (showRegion && pres === "region" && !pv) { const b = bboxOf(p); drawMarkerLabel(ctx, p.name, b.x0, b.y0 - (PS.label_pt / 2 + 10) * kk, kk, PS, clamp(tm.k) * dimOthers); }
      else if (PS.label_pos === "below") drawMarkerLabel(ctx, p.name, p.pos.x, p.pos.y + Rm + (mkLabelPt(PS) / 2 + 10) * kk, kk, PS, clamp(tm.k) * dimOthers, "center", p);
      else drawMarkerLabel(ctx, p.name, p.pos.x + Rm + 6 * kk, p.pos.y, kk, PS, clamp(tm.k) * dimOthers, "left", p);
    }
  });
  ctx.restore();
}
function dashed(g, col, w = 1.2) { if (g.pts.length < 2) return; ctx.save(); ctx.setLineDash([4 / scale, 4 / scale]); ctx.strokeStyle = col; ctx.lineWidth = w / scale; ctx.stroke(toPath(g.pts, g.closed)); ctx.restore(); }

