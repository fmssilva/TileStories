/* ---------- geometry ---------- */
function build(S) {
  const Kn = S.knots, n = Kn.length;
  if (S.type === "points" || n < 2) return { pts: Kn.map(p => [p.x, p.y]), closed: false, seg: [], mids: [] };
  const closed = S.type === "polygon" && n >= 3;
  const info = Kn.map((p, i) => {
    const c = cornerOf(S, i), hasP = closed || i > 0, hasN = closed || i < n - 1;
    const pr = hasP ? Kn[(i - 1 + n) % n] : p, nx = hasN ? Kn[(i + 1) % n] : p;
    if (c === "smooth") { const t = [(nx.x - pr.x) / 6, (nx.y - pr.y) / 6]; return { inP: [p.x, p.y], outP: [p.x, p.y], t }; }
    if (c === "rounded" && hasP && hasN) {
      const d1 = Math.hypot(pr.x - p.x, pr.y - p.y) || 1, d2 = Math.hypot(nx.x - p.x, nx.y - p.y) || 1, r = Math.min(p.radius * PXCM, d1 / 2, d2 / 2);
      return { inP: [p.x + (pr.x - p.x) / d1 * r, p.y + (pr.y - p.y) / d1 * r], outP: [p.x + (nx.x - p.x) / d2 * r, p.y + (nx.y - p.y) / d2 * r], t: [0, 0], ctrl: [p.x, p.y] };
    }
    return { inP: [p.x, p.y], outP: [p.x, p.y], t: [0, 0] };
  });
  const pts = [info[0].outP], seg = [], mids = [], segs = closed ? n : n - 1;
  for (let i = 0; i < segs; i++) {
    seg.push(pts.length - 1);
    const a = info[i], b = info[(i + 1) % n];
    const p0 = a.outP, p1 = [p0[0] + a.t[0], p0[1] + a.t[1]], p2 = [b.inP[0] - b.t[0], b.inP[1] - b.t[1]], p3 = b.inP;
    for (let j = 1; j <= 16; j++) {
      const u = j / 16, v = 1 - u;
      const q = [v*v*v*p0[0] + 3*v*v*u*p1[0] + 3*v*u*u*p2[0] + u*u*u*p3[0], v*v*v*p0[1] + 3*v*v*u*p1[1] + 3*v*u*u*p2[1] + u*u*u*p3[1]];
      pts.push(q); if (j === 8) mids.push({ seg: i, p: q });
    }
    if (b.ctrl && (closed || i + 1 < n - 1))
      for (let j = 1; j <= 10; j++) { const u = j / 10, v = 1 - u; pts.push([v*v*b.inP[0] + 2*v*u*b.ctrl[0] + u*u*b.outP[0], v*v*b.inP[1] + 2*v*u*b.ctrl[1] + u*u*b.outP[1]]); }
  }
  seg.push(pts.length - 1);
  return { pts, closed, seg, mids };
}
const plen = P => { let L = 0; for (let i = 1; i < P.length; i++) L += Math.hypot(P[i][0] - P[i-1][0], P[i][1] - P[i-1][1]); return L; };
function signedArea(P) { let a = 0; for (let i = 0; i < P.length; i++) { const p = P[i], q = P[(i + 1) % P.length]; a += p[0] * q[1] - q[0] * p[1]; } return a / 2; }
function offsetPts(P, d, closed) {
  const n = P.length; if (n < 2 || !d) return P;
  const s = closed ? (signedArea(P) > 0 ? -1 : 1) : 1, out = [];
  const far = (i, step) => { for (let j = 1; j < n; j++) { const k = closed ? (i + step * j + n * 4) % n : i + step * j; if (k < 0 || k >= n) return null; if (Math.hypot(P[k][0] - P[i][0], P[k][1] - P[i][1]) > 1e-6) return P[k]; } return null; };
  for (let i = 0; i < n; i++) {
    const a = far(i, -1), b = far(i, 1), p = P[i];
    const nr = (u, v) => { const dx = v[0] - u[0], dy = v[1] - u[1], L = Math.hypot(dx, dy) || 1; return [-dy / L, dx / L]; };
    const n1 = a ? nr(a, p) : null, n2 = b ? nr(p, b) : null;
    let m = n1 && n2 ? [n1[0] + n2[0], n1[1] + n2[1]] : (n1 || n2 || [0, 0]);
    const L = Math.hypot(m[0], m[1]) || 1; m = [m[0] / L, m[1] / L];
    const k = n1 && n2 ? 1 / Math.max(0.35, m[0] * n1[0] + m[1] * n1[1]) : 1;
    out.push([p[0] + m[0] * d * s * k, p[1] + m[1] * d * s * k]);
  }
  return out;
}
function slice(P, a, b, f) {
  const sub = P.slice(a, b + 1); if (f >= 1) return sub; if (f <= 0 || sub.length < 2) return [];
  const goal = plen(sub) * f, out = [sub[0]]; let acc = 0;
  for (let i = 1; i < sub.length; i++) {
    const L = Math.hypot(sub[i][0] - sub[i-1][0], sub[i][1] - sub[i-1][1]);
    if (acc + L >= goal) { const u = (goal - acc) / (L || 1); out.push([sub[i-1][0] + (sub[i][0] - sub[i-1][0]) * u, sub[i-1][1] + (sub[i][1] - sub[i-1][1]) * u]); return out; }
    acc += L; out.push(sub[i]);
  }
  return out;
}
function window_(P, from, to) {
  const out = []; let acc = 0;
  for (let i = 1; i < P.length; i++) {
    const a = P[i-1], b = P[i], L = Math.hypot(b[0] - a[0], b[1] - a[1]), s = acc, e = acc + L; acc = e;
    if (e < from || s > to || L === 0) continue;
    const u0 = clamp((from - s) / L), u1 = clamp((to - s) / L);
    if (!out.length) out.push([a[0] + (b[0] - a[0]) * u0, a[1] + (b[1] - a[1]) * u0]);
    out.push([a[0] + (b[0] - a[0]) * u1, a[1] + (b[1] - a[1]) * u1]);
  }
  return out;
}
function pointAt(P, s) { // point and tangent at length s
  let acc = 0;
  for (let i = 1; i < P.length; i++) {
    const a = P[i-1], b = P[i], L = Math.hypot(b[0] - a[0], b[1] - a[1]);
    if (acc + L >= s && L > 0) { const u = (s - acc) / L; return { x: a[0] + (b[0] - a[0]) * u, y: a[1] + (b[1] - a[1]) * u, tx: (b[0] - a[0]) / L, ty: (b[1] - a[1]) / L }; }
    acc += L;
  }
  const a = P[P.length - 2] || P[0], b = P[P.length - 1]; const L = Math.hypot(b[0] - a[0], b[1] - a[1]) || 1;
  return { x: b[0], y: b[1], tx: (b[0] - a[0]) / L, ty: (b[1] - a[1]) / L };
}
const toPath = (P, close) => { const p = new Path2D(); if (!P.length) return p; p.moveTo(P[0][0], P[0][1]); for (let i = 1; i < P.length; i++) p.lineTo(P[i][0], P[i][1]); if (close) p.closePath(); return p; };
function rgba(hex, a) { const h = hex.replace("#", ""); const n = parseInt(h.length === 3 ? h.split("").map(c => c + c).join("") : h, 16); return `rgba(${n >> 16 & 255},${n >> 8 & 255},${n & 255},${clamp(a)})`; }
const toM = p => [+((p.x - ORIGIN[0]) / PXCM / 100).toFixed(3), +((ORIGIN[1] - p.y) / PXCM / 100).toFixed(3), 0];
const bboxPts = P => { const xs = P.map(p => p[0]), ys = P.map(p => p[1]); return { x0: Math.min(...xs), x1: Math.max(...xs), y0: Math.min(...ys), y1: Math.max(...ys) }; };

/* precision: how many cm of tracking error a style hides (section 11 of _4.1.3) */
function tolerance(st) {
  const L = st.line, half = L.width_cm / 2, pl = effPlacement(L);
  let t = pl === "outside" ? L.offset_cm + half : pl === "inside" ? L.offset_cm * 0.5 + half : half;
  if (L.glow) t += L.glow_width_cm * 0.25;
  if (st.fill.mode !== "none") t = Math.max(t, st.fill.feather_cm * 0.5);
  if (st.motion.focus.preset === "spotlight") t = Math.max(t, 4);
  return t;
}
function verdict(st, e = D.test.error_cm) {
  const t = tolerance(st), grade = t >= 3 ? "low" : t >= 1.5 ? "medium" : "high";
  const v = e <= t ? ["p-ok", "holds"] : e <= t + 1.5 ? ["p-edge", "edge shows"] : ["p-bad", "misses"];
  return { t, grade, cls: v[0], word: v[1] };
}
const chipText = v => `hides ≈${v.t.toFixed(1)} cm`;

