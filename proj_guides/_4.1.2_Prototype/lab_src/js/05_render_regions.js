/* ---------- rendering: regions ---------- */
function drawMark(ctx, x, y, mark, s, col, u, dirs) {
  ctx.save(); ctx.translate(x, y); ctx.fillStyle = col; ctx.strokeStyle = col; ctx.lineCap = "round";
  const lw = Math.max(1.4 * u, s * 0.18); ctx.lineWidth = lw;
  if (mark === "dot") { ctx.beginPath(); ctx.arc(0, 0, s / 2, 0, 7); ctx.fill(); }
  else if (mark === "square") ctx.fillRect(-s / 2, -s / 2, s, s);
  else if (mark === "diamond") { ctx.beginPath(); ctx.moveTo(0, -s * .6); ctx.lineTo(s * .6, 0); ctx.lineTo(0, s * .6); ctx.lineTo(-s * .6, 0); ctx.closePath(); ctx.fill(); }
  else if (mark === "cross") { ctx.beginPath(); ctx.moveTo(-s / 2, 0); ctx.lineTo(s / 2, 0); ctx.moveTo(0, -s / 2); ctx.lineTo(0, s / 2); ctx.stroke(); }
  else if (mark === "bracket" && dirs) { const L = s * 1.3; ctx.lineWidth = Math.max(1.6 * u, s * 0.22); ctx.beginPath(); ctx.moveTo(dirs[0][0] * L, dirs[0][1] * L); ctx.lineTo(0, 0); ctx.lineTo(dirs[1][0] * L, dirs[1][1] * L); ctx.stroke(); }
  else { ctx.beginPath(); ctx.arc(0, 0, s / 2, 0, 7); ctx.stroke(); }
  ctx.restore();
}
function drawFill(env, poi, F, g, A, idle, t, I, per, S, f = 0) {
  const { ctx, sc, dpr } = env;
  const mode = F.mode;
  if (!g.closed || mode === "none" || A <= 0) return;
  const fc = F.color_mode === "category" ? catColor(poi) : F.color_mode === "accent" ? WALL_INK : F.color, path = toPath(g.pts, true), b = bboxPts(g.pts);
  ctx.save();
  if (mode === "spotlight") {   // dim the wall around the region; the soft edge hides an imprecise outline
    const big = new Path2D(); big.rect(-IW, -IH, IW * 3, IH * 3); big.addPath(path);
    if (F.feather_cm > 0) ctx.filter = `blur(${F.feather_cm * PXCM * sc * dpr * 0.35}px)`;
    ctx.fillStyle = `rgba(6,8,12,${clamp(F.dim * (appearance() === "light" ? 0.7 : 1)) * A})`; ctx.fill(big, "evenodd");
  } else if (mode === "tint") {
    if (F.blend === "screen") ctx.globalCompositeOperation = "screen";   // screen lifts the tiles like light instead of painting over them
    if (F.feather_cm > 0) ctx.filter = `blur(${F.feather_cm * PXCM * sc * dpr * 0.35}px)`;
    ctx.fillStyle = rgba(fc, F.opacity * A); ctx.fill(path);
  } else if (mode === "gradient") {
    let gr;
    if (F.grad === "radial") {
      const cx = g.pts.reduce((a, p) => a + p[0], 0) / g.pts.length, cy = g.pts.reduce((a, p) => a + p[1], 0) / g.pts.length;
      const R = Math.max(...g.pts.map(p => Math.hypot(p[0] - cx, p[1] - cy)));
      gr = ctx.createRadialGradient(cx, cy, 0, cx, cy, R);
    } else {
      const a = F.angle_deg * Math.PI / 180, cx = (b.x0 + b.x1) / 2, cy = (b.y0 + b.y1) / 2, hl = (Math.abs(Math.cos(a)) * (b.x1 - b.x0) + Math.abs(Math.sin(a)) * (b.y1 - b.y0)) / 2;
      gr = ctx.createLinearGradient(cx - Math.cos(a) * hl, cy - Math.sin(a) * hl, cx + Math.cos(a) * hl, cy + Math.sin(a) * hl);
    }
    if (F.blend === "screen") ctx.globalCompositeOperation = "screen";
    gr.addColorStop(0, rgba(fc, F.alpha_from * A)); gr.addColorStop(1, rgba(fc, F.alpha_to * A));
    if (F.feather_cm > 0) ctx.filter = `blur(${F.feather_cm * PXCM * sc * dpr * 0.35}px)`;
    ctx.fillStyle = gr; ctx.fill(path);
  } else if (mode === "media") {
    ctx.restore();
    const still = F.media_play === "on_tap" && f < 0.01;
    const el = env.ctx === ctx ? mediaSource(S && S.media, still ? 0 : performance.now() / 1000) : null;
    if (el && el.tagName === "VIDEO") { if (still && !el.paused) el.pause(); else if (!still && el.paused) el.play().catch(() => {}); }
    if (!mediaReady(el)) { ctx.save(); ctx.fillStyle = rgba(fc, 0.18 * A); ctx.fill(path); ctx.restore(); }
    else {
      const sw = el.videoWidth || el.naturalWidth || el.width, sh = el.videoHeight || el.naturalHeight || el.height, bw = b.x1 - b.x0, bh = b.y1 - b.y0;
      const r = F.fit === "contain" ? Math.min(bw / sw, bh / sh) : Math.max(bw / sw, bh / sh);
      const dw = F.fit === "stretch" ? bw : sw * r, dh = F.fit === "stretch" ? bh : sh * r;
      off3.width = cv.width; off3.height = cv.height; const x = off3.getContext("2d");
      x.setTransform(ctx.getTransform()); x.drawImage(el, b.x0 + (bw - dw) / 2, b.y0 + (bh - dh) / 2, dw, dh);
      const fe = F.feather_cm * PXCM * sc * dpr * 0.35;
      x.globalCompositeOperation = "destination-in"; if (fe > 0) x.filter = `blur(${fe}px)`; x.fill(path); x.filter = "none";
      ctx.save(); ctx.setTransform(1, 0, 0, 1, 0, 0); ctx.globalAlpha = clamp(F.opacity * A); ctx.drawImage(off3, 0, 0); ctx.restore();
    }
    ctx.save();
  } else if (mode === "pattern") {
    ctx.clip(path); const st = Math.max(2, F.scale_cm * PXCM);
    ctx.strokeStyle = rgba(fc, F.opacity * A * 2.2); ctx.fillStyle = rgba(fc, F.opacity * A * 2.2); ctx.lineWidth = Math.max(0.6, st * 0.07);
    if (F.pattern === "hatch") { ctx.beginPath(); for (let x = b.x0 - (b.y1 - b.y0); x < b.x1; x += st * 0.6) { ctx.moveTo(x, b.y1); ctx.lineTo(x + (b.y1 - b.y0), b.y0); } ctx.stroke(); }
    else if (F.pattern === "dots") { for (let y = b.y0; y < b.y1 + st; y += st * 0.7) for (let x = b.x0; x < b.x1 + st; x += st * 0.7) { ctx.beginPath(); ctx.arc(x, y, st * 0.1, 0, 7); ctx.fill(); } }
    else { // azulejo: a tile grid with a diamond in each tile
      ctx.beginPath();
      for (let y = b.y0; y < b.y1 + st; y += st) for (let x = b.x0; x < b.x1 + st; x += st) {
        ctx.rect(x, y, st, st); const c = st / 2, r = st * 0.28;
        ctx.moveTo(x + c, y + c - r); ctx.lineTo(x + c + r, y + c); ctx.lineTo(x + c, y + c + r); ctx.lineTo(x + c - r, y + c); ctx.closePath();
      }
      ctx.stroke();
    }
  }
  ctx.restore();
  if (idle === "sweep") {
    const W = (b.x1 - b.x0 + b.y1 - b.y0) * 0.18, prog = (t / per) % 1, cx = b.x0 - W + (b.x1 - b.x0 + 2 * W) * prog;
    ctx.save(); ctx.clip(path);
    const gr = ctx.createLinearGradient(cx - W, b.y0, cx + W, b.y0 + W * 0.6);
    gr.addColorStop(0, "rgba(255,255,255,0)"); gr.addColorStop(0.5, `rgba(255,248,225,${0.45 * I * A})`); gr.addColorStop(1, "rgba(255,255,255,0)");
    ctx.fillStyle = gr; ctx.fillRect(b.x0, b.y0, b.x1 - b.x0, b.y1 - b.y0); ctx.restore();
  }
}

/* corner smoothing (Chaikin, 3 rounds): hand-drawn polygons read as clean, rounded outlines; s = 0..1 */
/* corner smoothing that shows: drop the collinear samples (build() samples edges densely), then round each real corner with an
   arc whose radius is the slider × 6 cm, never more than 45 % of the shorter neighbouring edge */
function simplifyPts(P, closed, tol = 0.35) {
  if (P.length < 3) return P; const out = [], n = P.length;
  for (let i = 0; i < n; i++) {
    if (!closed && (i === 0 || i === n - 1)) { out.push(P[i]); continue; }
    const a = P[(i - 1 + n) % n], b = P[i], c = P[(i + 1) % n], cr = Math.abs((b[0] - a[0]) * (c[1] - a[1]) - (b[1] - a[1]) * (c[0] - a[0])) / (Math.hypot(c[0] - a[0], c[1] - a[1]) || 1);
    if (cr > tol) out.push(b);
  }
  return out.length >= (closed ? 3 : 2) ? out : P;
}
function roundCorners(P, closed, s) {
  if (!s || P.length < 3) return P; const Q = simplifyPts(P, closed), n = Q.length, Rmax = s * 6 * PXCM, out = [];
  for (let i = 0; i < n; i++) {
    if (!closed && (i === 0 || i === n - 1)) { out.push(Q[i]); continue; }
    const a = Q[(i - 1 + n) % n], b = Q[i], c = Q[(i + 1) % n], la = Math.hypot(a[0] - b[0], a[1] - b[1]), lc = Math.hypot(c[0] - b[0], c[1] - b[1]);
    const d = Math.min(Rmax, 0.45 * Math.min(la, lc)); if (d < 0.2) { out.push(b); continue; }
    const p0 = [b[0] + (a[0] - b[0]) / la * d, b[1] + (a[1] - b[1]) / la * d], p2 = [b[0] + (c[0] - b[0]) / lc * d, b[1] + (c[1] - b[1]) / lc * d];
    for (let k = 0; k <= 8; k++) { const t = k / 8, u = 1 - t; out.push([u * u * p0[0] + 2 * u * t * b[0] + t * t * p2[0], u * u * p0[1] + 2 * u * t * b[1] + t * t * p2[1]]); }
  }
  return out;
}
function chaikin(P, closed, s) {
  if (!s || P.length < 3) return P; const r = 0.25 * Math.min(1, s);
  let A = P;
  for (let it = 0; it < 3; it++) {
    const out = [], n = A.length, m = closed ? n : n - 1;
    if (!closed) out.push(A[0]);
    for (let i = 0; i < m; i++) { const p = A[i], q = A[(i + 1) % n]; out.push([p[0] + (q[0] - p[0]) * r, p[1] + (q[1] - p[1]) * r], [p[0] + (q[0] - p[0]) * (1 - r), p[1] + (q[1] - p[1]) * (1 - r)]); }
    if (!closed) out.push(A[n - 1]);
    A = out;
  }
  return A;
}
/* a region's colours: category, a fixed colour, or the app's accent (light design: a cobalt line with a white glow, so it reads on blue tiles; dark: gold with a warm glow) */
const WALL_INK = "#E3BD72";   // Unity's status-ring gold: the one ink that reads on cobalt-and-white tiles
function regionCols(L, poi) {
  /* "accent" = the wall ink: the camera sees the same cobalt-and-white panel in light and dark design, so the region keeps the warm gold that
     contrasts with azulejo blue in both (a cobalt line disappears into the tiles). Only the app chrome follows light / dark. */
  if (L.color_mode === "accent") return { col: WALL_INK, glow: WALL_INK };
  const c = L.color_mode === "category" ? catColor(poi) : L.color; return { col: c, glow: c };
}
/* a: { T: seconds since this shape's reveal (Infinity = settled), t: clock, idle, f: focus 0..1, alpha } */
function drawRegion(env, poi, S, st, a) {
  const { ctx, sc, dpr } = env, u = 1 / sc, L = st.line, red = D.test.reduce_motion;
  const { col, glow: gcol } = regionCols(L, poi);
  const g0 = build(S), pts = S.type === "points";
  const g = !pts && L.smooth > 0 && g0.pts.length > 2 ? (() => { const P = roundCorners(g0.pts, g0.closed, L.smooth), k = (P.length - 1) / Math.max(1, g0.pts.length - 1); return { ...g0, pts: P, seg: g0.seg.map(i => Math.round(i * k)) }; })() : g0;
  const half = L.width_cm * PXCM / 2, pl = effPlacement(L);
  let d = 0;
  if (g.closed) d = pl === "outside" ? L.offset_cm * PXCM + half : pl === "inside" ? -(L.offset_cm * PXCM + half) : 0;
  const lp = d ? offsetPts(g.pts, d, true) : g.pts;
  const intro = red ? "fade" : st.motion.intro.preset, dur = red ? 0.3 : st.motion.intro.duration_s, T = a.T;
  const M = st.motion, sd = M.intro.segment_delay_s, n = S.knots.length;
  let alpha = a.alpha ?? 1, parts = [], fillA = 1, vAlpha = () => 1;

  if (T === Infinity) parts = [lp];
  else if (intro === "fade") { alpha *= clamp(T / dur); parts = [lp]; }
  else if (intro === "draw_on") {
    const P = M.intro.direction === "ccw" ? lp.slice().reverse() : lp;
    parts = [slice(P, 0, P.length - 1, ease(T / dur))]; fillA = smooth((T - dur * 0.7) / (dur * 0.3));
    vAlpha = i => clamp((T - (i / Math.max(1, n)) * dur) / 0.2);
  } else {
    const e0 = pts ? 0 : n * sd * 0.6, segs = g.seg.length - 1, edge = Math.max(0.15, dur - e0 - (segs - 1) * sd);
    for (let i = 0; i < segs; i++) { const f = (T - e0 - i * sd) / edge; if (f > 0) parts.push(slice(lp, g.seg[i], g.seg[i + 1], ease(f))); }
    fillA = smooth((T - dur * 0.75) / (dur * 0.25));
    vAlpha = i => clamp((T - i * sd * (pts ? 1 : 0.6)) / 0.18);
  }

  const idle = a.idle && !red ? M.idle.preset : "none", t = a.t, per = Math.max(0.2, M.idle.period_s), I = M.idle.intensity;
  const fp = M.focus.preset, f = a.f || 0;
  let wMul = 1, gMul = 1, aMul = 1;
  let fMul = 1;
  if (idle === "breathe") { const k = 0.5 + 0.5 * Math.sin(t / per * Math.PI * 2); gMul *= 0.4 + 1.2 * k * I + (1 - I) * 0.6; aMul *= 1 - 0.35 * I * (1 - k); fMul = 1 - 0.5 * I * (1 - k); }   // the wash breathes too
  if (fp === "brighten" || fp === "lock_on") { wMul *= 1 + (M.focus.amount - 1) * f; gMul *= 1 + (M.focus.amount - 1) * f; }
  if (fp === "spotlight") { wMul *= 1 + 0.25 * f; gMul *= 1 + 0.5 * f; }
  const lineOp = fp === "appear" ? L.opacity + (M.focus.amount - L.opacity) * f : L.opacity, vOp = fp === "appear" ? lineOp : 1;

  ctx.save(); ctx.lineJoin = "round"; ctx.lineCap = "round";
  drawFill(env, poi, st.fill, g, fillA * alpha * fMul * (fp === "appear" ? f : 1 + 1.5 * f), idle, t, I, per, S, f);
  if (!pts) {
    const lw = clamp(L.width_cm * PXCM * sc, L.min_px, L.max_px) * wMul * u;
    const dash = L.pattern === "dashed" ? [L.dash_cm * PXCM, L.gap_cm * PXCM] : L.pattern === "dotted" ? [0.01, Math.max(lw * 2.4, 1.2 * PXCM)] : [];
    const stroke = (P, w, al, glow) => {
      if (P.length < 2) return; const path = toPath(P, false);
      ctx.setLineDash(dash); ctx.lineDashOffset = idle === "flow" && dash.length ? -(t / per) * (dash[0] + dash[1]) : 0;
      ctx.lineWidth = w;
      if (glow && L.glow) {   // depth on azulejo: a dark contact shadow first (a gold glow alone has no contrast on cream tiles), then the glow added with light
        ctx.save(); ctx.strokeStyle = rgba("#08101C", 0.3 * al); ctx.shadowColor = rgba("#08101C", 0.35 * al); ctx.shadowBlur = 1.2 * PXCM * sc * dpr; ctx.shadowOffsetY = 0.25 * PXCM * sc * dpr; ctx.stroke(path); ctx.restore();
        ctx.save(); ctx.globalCompositeOperation = "lighter"; ctx.strokeStyle = rgba(col, al * 0.5); ctx.shadowColor = rgba(gcol, clamp(L.glow_intensity * gMul) * al); ctx.shadowBlur = L.glow_width_cm * PXCM * sc * dpr * gMul; ctx.stroke(path); if (L.glow_intensity * gMul > 0.6) ctx.stroke(path); ctx.restore(); }
      ctx.strokeStyle = rgba(col, al); ctx.stroke(path);
    };
    if (L.pattern === "brackets") {   // focus brackets: four corners of the region's box; they never depend on the polygon being exact
      const all = parts.flat(), xs = lp.map(p => p[0]), ys = lp.map(p => p[1]), pad = (L.offset_cm || 0) * PXCM * (1 - 0.6 * f);   // on focus the brackets close in on the region
      const x0 = Math.min(...xs) - pad, x1 = Math.max(...xs) + pad, y0 = Math.min(...ys) - pad, y1 = Math.max(...ys) + pad, arm = Math.min(0.18 * Math.min(x1 - x0, y1 - y0), 60 * dev.U / 3.9 * u);
      const k = T === Infinity ? 1 : clamp(T / dur), al = lineOp * alpha * aMul * (all.length || T === Infinity ? 1 : 0);
      ctx.setLineDash([]); ctx.lineWidth = lw; ctx.strokeStyle = rgba(col, al * k);
      const A = arm * (0.4 + 0.6 * ease(k)), corners = () => { const pb = new Path2D(); for (const [x, y, sx, sy] of [[x0, y0, 1, 1], [x1, y0, -1, 1], [x1, y1, -1, -1], [x0, y1, 1, -1]]) { pb.moveTo(x, y + sy * A); pb.lineTo(x, y); pb.lineTo(x + sx * A, y); } return pb; }, pb = corners();
      ctx.save(); ctx.strokeStyle = rgba("#08101C", 0.3 * al * k); ctx.shadowColor = rgba("#08101C", 0.35 * al * k); ctx.shadowBlur = 1.2 * PXCM * sc * dpr; ctx.shadowOffsetY = 0.25 * PXCM * sc * dpr; ctx.stroke(pb); ctx.restore();
      if (L.glow) { ctx.save(); ctx.globalCompositeOperation = "lighter"; ctx.shadowColor = rgba(gcol, clamp(L.glow_intensity * gMul) * al); ctx.shadowBlur = L.glow_width_cm * PXCM * sc * dpr * gMul; ctx.strokeStyle = rgba(col, al * k * 0.5); ctx.stroke(pb); ctx.restore(); }
      ctx.stroke(pb);
    } else for (const P of parts) stroke(P, lw, lineOp * alpha * aMul, true);
    if (L.double && T >= dur) {
      const d2 = d + (d >= 0 ? 1 : -1) * L.double_gap_cm * PXCM, al = clamp(T === Infinity ? 1 : (T - dur) / 0.4);
      stroke(offsetPts(g.pts, d2, g.closed), lw * 0.6, lineOp * 0.55 * alpha * al, false);
    }
    ctx.setLineDash([]); ctx.lineDashOffset = 0;
    if (idle === "flow" && !dash.length && T >= dur) {
      ctx.setLineDash([2 * PXCM, 5 * PXCM]); ctx.lineDashOffset = -(t / per) * 7 * PXCM; ctx.lineWidth = lw * 1.05; ctx.strokeStyle = `rgba(255,255,255,${0.7 * I * alpha})`; ctx.stroke(toPath(lp, false)); ctx.setLineDash([]);
    }
    if (idle === "comet" && T >= dur) {
      const Lt = plen(lp), tail = Math.min(Lt * 0.3, 14 * PXCM), cnt = Math.max(1, Math.round(M.idle.count));
      for (let c = 0; c < cnt; c++) {
        const head = (((t / per) + c / cnt) % 1) * (Lt + tail);
        for (let k = 0; k < 4; k++) {
          const P = window_(lp, head - tail * (k + 1) / 4, head - tail * k / 4); if (P.length < 2) continue;
          ctx.lineWidth = lw * (1.6 - k * 0.25); ctx.strokeStyle = `rgba(255,250,235,${(0.95 - k * 0.22) * I * alpha * Math.min(1, lineOp * 1.2)})`;
          ctx.shadowColor = rgba(col, 0.9 * I); ctx.shadowBlur = 6 * dpr; ctx.stroke(toPath(P, false)); ctx.shadowBlur = 0;
        }
      }
    }
  }
  const V = st.vertices;
  if (pts || V.show_at !== "none") {
    const kp = d ? offsetPts(S.knots.map(q => [q.x, q.y]), d, true) : S.knots.map(q => [q.x, q.y]);
    const vc = V.color_mode === "line" ? col : V.color, mark = pts && V.mark === "bracket" ? "ring" : V.mark;
    ctx.save();
    if (L.glow) { ctx.shadowColor = rgba(vc, 0.6 * gMul); ctx.shadowBlur = 4 * dpr * gMul; }
    S.knots.forEach((q, i) => {
      if (!pts && V.show_at === "corners" && cornerOf(S, i) === "smooth") return;
      const va = vAlpha(i); if (va <= 0) return;
      let s = V.size_cm * PXCM * (0.6 + 0.4 * ease(va));
      if (idle === "knot_pulse") { const ph = Math.sin((t / per - i / Math.max(1, n)) * Math.PI * 2); s *= 1 + 0.6 * I * Math.pow(Math.max(0, ph), 6); }
      if (pts && (fp === "brighten" || fp === "lock_on")) s *= 1 + (M.focus.amount - 1) * f * 0.6;
      let dirs = null;
      if (mark === "bracket" && n >= 2) {
        const pv = S.knots[(i - 1 + n) % n], nx = S.knots[(i + 1) % n], nd = (o) => { const dx = o.x - q.x, dy = o.y - q.y, l = Math.hypot(dx, dy) || 1; return [dx / l, dy / l]; };
        dirs = S.type === "path" && i === 0 ? [nd(nx), nd(nx)] : S.type === "path" && i === n - 1 ? [nd(pv), nd(pv)] : [nd(pv), nd(nx)];
      }
      ctx.globalAlpha = alpha * va * aMul * vOp; drawMark(ctx, kp[i][0], kp[i][1], mark, s, vc, u, dirs);
    });
    ctx.restore();
  }
  ctx.restore();
}
function drawLabel(env, text, x, y, alpha, bg, right) {
  const { ctx, sc } = env, u = 1 / sc;
  ctx.save(); ctx.globalAlpha = alpha; ctx.font = `600 ${11 * u}px "Atkinson Hyperlegible Next", system-ui, sans-serif`;
  const w = ctx.measureText(text).width + 12 * u, h = 18 * u; if (right) x -= w;
  ctx.fillStyle = bg || "rgba(14,17,24,.78)"; ctx.beginPath(); ctx.roundRect(x, y - h / 2, w, h, h / 2); ctx.fill();
  ctx.fillStyle = "#FFFFFF"; ctx.textBaseline = "middle"; ctx.fillText(text, x + 6 * u, y + 0.5 * u); ctx.restore();
}
function bboxOf(poi) {
  const xs = [], ys = [];
  for (const S of poi.world.shapes) if (!isQuiet(styleFor(poi, S))) for (const q of S.knots) { xs.push(q.x); ys.push(q.y); }
  if (!xs.length) { xs.push(poi.pos.x); ys.push(poi.pos.y); }
  return { x0: Math.min(...xs), x1: Math.max(...xs), y0: Math.min(...ys), y1: Math.max(...ys) };
}

