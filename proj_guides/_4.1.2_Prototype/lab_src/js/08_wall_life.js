/* ---------- Wall Life ---------- */
const allContent = () => { const out = []; D.pois.forEach((p, pi) => (p.world.content || []).forEach((c, ci) => out.push({ c, p, pi, ci }))); D.life.wall.forEach((c, ci) => out.push({ c, p: null, pi: -1, ci })); return out; };
const reduceState = c => c.reduce || D.life.settings.reduce_motion_state;
function runLen(c, p) {
  const k = c.kind;
  if (k === "agent") { const S = shapeByKey(p, c.anchor); if (!S) return 4; return plen(build(S).pts) / (c.p.speed_cm_s * PXCM) + 0.6; }
  if (k === "particles") return c.p.sprite === "leaves" ? 5 : (IW + 60) / (c.p.speed_cm_s * PXCM);
  if (k === "guest") return 0.7 + c.p.stay_s + 0.7;
  if (k === "flat") return 4;
  if (k === "hinged") return c.p.dur_s * 2 + 2;
  return 2;
}
/* each content item's state now: env (0..1 presence), run (0..1 one-shot progress or -1), t (local clock) */
function lifeState(o, now) {
  const { c, p, pi } = o, red = D.test.reduce_motion, key = (pi < 0 ? "w" : pi) + ":" + c.key;
  if (!playing) return { env: 1, run: -1, t: 0, still: true };
  const start = pi >= 0 ? poiTimes(pi, now).start + (c.delay_s || 0) : playT0 + (c.delay_s || 0);
  if (now < start) return { env: 0, run: -1, t: 0 };
  const R = lifeRun[key] || (lifeRun[key] = { next: start + rnd(0.5, 2), runStart: -1e9, openT: -1e9, closeT: -1e9 });
  const isFocus = pi >= 0 && focus === pi;
  const tr = c.trigger;
  if (red && (tr === "every" || c.kind === "guest") ) return { env: 0, run: -1, t: 0 };
  if (tr === "loop") { if (red && reduceState(c) === "hidden") return { env: 0, run: -1, t: 0 }; return { env: clamp((now - start) / 0.5), run: red ? 0.3 : ((now - start) / runLen(c, p)) % 1, t: red ? 0 : now - start }; }
  if (tr === "reveal") { const d = c.kind === "repaint" ? c.p.wipe_s : 0.6; return { env: red ? 1 : clamp((now - start) / d), run: -1, t: now - start }; }
  if (tr === "focus") {
    const d = c.kind === "hinged" ? c.p.dur_s : 0.6, closeOn = c.kind !== "hinged" || c.p.close_on_unfocus;
    if (isFocus && R.openT < focusT0) { R.openT = focusT0; R.runStart = now; }
    if (!isFocus && R.openT > R.closeT && closeOn) R.closeT = now;
    const open = R.openT > R.closeT;
    const env = red ? (open ? 1 : 0) : open ? ease((now - R.openT) / d) : 1 - ease((now - R.closeT) / d);
    const rl = runLen(c, p), run = (now - R.runStart) / rl;
    return { env: R.openT < 0 ? 0 : clamp(env), run: run >= 0 && run <= 1 ? run : -1, t: now - R.openT };
  }
  if (tr === "unfocus") { const run = (now - unfocusT0) / 1.5; return { env: run >= 0 && run <= 1 ? Math.sin(Math.PI * clamp(run)) : 0, run: -1, t: now - unfocusT0 }; }
  if (tr === "every") {
    const rl = runLen(c, p);
    if (now >= R.next && R.runStart + rl < now) {
      if (c.kind === "guest" && c.p.hide_on_card && focus !== null) R.next = now + 1.2;
      else { R.runStart = now; R.next = now + rl + rnd(c.every_min_s, c.every_max_s); lastAmbientEnd = now + rl; if (c.kind === "guest") lastGuest = now; }
    }
    const run = (now - R.runStart) / rl;
    return { env: run >= 0 && run <= 1 ? 1 : 0, run: run >= 0 && run <= 1 ? run : -1, t: now - R.runStart };
  }
  return { env: 0, run: -1, t: 0 };
}
function triggerNow(o) {
  if (!playing) togglePlay();
  const now = performance.now() / 1000, key = (o.pi < 0 ? "w" : o.pi) + ":" + o.c.key;
  if (o.c.trigger === "focus" && o.pi >= 0) { focus = focus === o.pi ? null : o.pi; focusT0 = now; if (focus !== null) openSheet(D.pois[o.pi]); else { closeSheet(); unfocusT0 = now; } return; }
  if (o.c.trigger === "unfocus") { unfocusT0 = now; return; }
  if (o.c.trigger === "reveal" && o.pi >= 0) { poiStart[o.pi] = now; delete lifeRun[key]; return; }
  const R = lifeRun[key] || (lifeRun[key] = { next: 0, runStart: -1e9, openT: -1e9, closeT: -1e9 });
  R.runStart = now; R.next = now + runLen(o.c, o.p) + rnd(o.c.every_min_s, o.c.every_max_s); if (o.c.kind === "guest") lastGuest = now;
}

let guestLabelAt = null;
const LOOKS = { night: "brightness(0.5) saturate(0.7) contrast(1.1)", cartoon: "saturate(2.2) contrast(1.5) brightness(1.05)", then: "sepia(0.85) contrast(1.15) brightness(0.95)" };
function drawRepaint(o, S, st, s) {
  const { c, p } = o; if (!S || st.env <= 0) return;
  const g = build(S); if (!g.closed) return;
  off2.width = cv.width; off2.height = cv.height; const x = off2.getContext("2d");
  x.setTransform(ctx.getTransform());
  x.filter = LOOKS[c.p.look] || "none"; x.drawImage(img, 0, 0, IW, IH); x.filter = "none";
  if (c.p.look === "night") {
    x.globalCompositeOperation = "source-atop"; x.fillStyle = "rgba(25,40,100,.38)"; x.fillRect(0, 0, IW, IH); x.globalCompositeOperation = "lighter";
    for (const W of p.world.shapes.filter(q => q.type === "points")) for (const q of W.knots) { const gr = x.createRadialGradient(q.x, q.y, 0, q.x, q.y, 9); gr.addColorStop(0, "rgba(255,200,110,.95)"); gr.addColorStop(1, "rgba(255,160,60,0)"); x.fillStyle = gr; x.fillRect(q.x - 10, q.y - 10, 20, 20); }
    x.globalCompositeOperation = "source-over";
  }
  const b = bboxPts(g.pts), feather = c.p.feather_cm * PXCM * scale * dprNow * 0.5;
  x.globalCompositeOperation = "destination-in"; if (feather > 0) x.filter = `blur(${feather}px)`; x.fillStyle = "#000"; x.fill(toPath(g.pts, true)); x.filter = "none";
  if (st.env < 1 && c.p.reveal === "brush") { // paint-on: a broad brush zig-zags across the shape
    const rows = 5, hh = (b.y1 - b.y0) / rows, P = [];
    for (let r = 0; r <= rows; r++) { const y = b.y0 + hh * (r + 0.5), l = r % 2 ? b.x1 + 8 : b.x0 - 8, rr = r % 2 ? b.x0 - 8 : b.x1 + 8; P.push([l, y], [rr, y + hh * 0.35]); }
    const part = slice(P, 0, P.length - 1, st.env);
    x.globalCompositeOperation = "destination-in"; x.lineWidth = hh * 1.5; x.lineCap = "round"; x.lineJoin = "round"; x.strokeStyle = "#000";
    if (part.length > 1) x.stroke(toPath(part, false)); else x.clearRect(-1e4, -1e4, 2e4, 2e4);
  } else if (st.env < 1) { const wx = b.x0 + (b.x1 - b.x0 + 20) * st.env, gr = x.createLinearGradient(wx - 20, 0, wx, 0); gr.addColorStop(0, "#000"); gr.addColorStop(1, "rgba(0,0,0,0)"); x.fillStyle = gr; x.fillRect(b.x0 - 30, b.y0 - 30, b.x1 - b.x0 + 60, b.y1 - b.y0 + 60); }
  x.globalCompositeOperation = "source-over";
  ctx.save(); ctx.setTransform(1, 0, 0, 1, 0, 0); ctx.globalAlpha = c.p.strength; ctx.drawImage(off2, 0, 0); ctx.restore();
}
function drawFlat(o, S, st) {
  const { c } = o; if (!S || st.env <= 0) return;
  const b = bboxPts(S.knots.map(q => [q.x, q.y])), w = b.x1 - b.x0, hh = b.y1 - b.y0, t = st.t * c.p.speed, amp = c.p.amp_cm * PXCM * 0.6;
  ctx.save(); ctx.globalAlpha = st.env; ctx.beginPath(); ctx.rect(b.x0, b.y0, w, hh); ctx.clip();
  if (c.p.cover) { const gr = ctx.createRadialGradient(b.x0 + w * .6, b.y0 + hh * .4, 1, b.x0 + w * .6, b.y0 + hh * .4, hh); gr.addColorStop(0, "#F7C77A"); gr.addColorStop(1, "#5A2E12"); ctx.fillStyle = gr; ctx.fillRect(b.x0, b.y0, w, hh); }
  const edge = y => b.x0 + w * (0.52 + 0.1 * Math.sin((y - b.y0) / hh * 2.4 + t * 2.1)) + amp * Math.sin((y - b.y0) / hh * 5 + t * 3.3);
  ctx.beginPath(); ctx.moveTo(b.x0, b.y0); ctx.lineTo(b.x0 + w * 0.5, b.y0);
  for (let i = 0; i <= 12; i++) { const y = b.y0 + hh * i / 12; ctx.lineTo(edge(y), y); }
  ctx.lineTo(b.x0, b.y1); ctx.closePath(); ctx.fillStyle = c.p.color; ctx.fill();
  ctx.strokeStyle = "rgba(90,60,30,.35)"; ctx.lineWidth = 0.6;
  for (let k = 1; k <= 3; k++) { ctx.beginPath(); for (let i = 0; i <= 12; i++) { const y = b.y0 + hh * i / 12, x = b.x0 + (edge(y) - b.x0) * k / 4; i ? ctx.lineTo(x, y) : ctx.moveTo(x, y); } ctx.stroke(); }
  ctx.restore();
  ctx.save(); ctx.strokeStyle = "rgba(60,40,25,.8)"; ctx.lineWidth = 0.8; ctx.strokeRect(b.x0, b.y0, w, hh); ctx.restore();
}
function drawHinged(o, S, st) {
  const { c } = o; if (!S) return;
  const open = st.still ? 0.45 : st.env; if (open <= 0.001) return;
  const b = bboxPts(S.knots.map(q => [q.x, q.y])), w = b.x1 - b.x0, hh = b.y1 - b.y0, cy = (b.y0 + b.y1) / 2, a = open * c.p.angle_deg * Math.PI / 180;
  ctx.save();
  if (c.p.interior) { const gr = ctx.createRadialGradient(b.x0 + w / 2, b.y0 + hh * .55, 1, b.x0 + w / 2, b.y0 + hh * .55, hh * .8); gr.addColorStop(0, `rgba(255,205,120,${0.95 * open})`); gr.addColorStop(1, "rgba(40,18,8,1)"); ctx.fillStyle = gr; }
  else ctx.fillStyle = "#120C08";
  ctx.fillRect(b.x0, b.y0, w, hh);
  const hw = w / 2, N = 10, cosA = Math.cos(a), grow = 1 + 0.22 * Math.sin(a);
  for (const side of [-1, 1]) {
    const hinge = side < 0 ? b.x0 : b.x1;
    for (let i = 0; i < N; i++) {
      const u0 = i / N, u1 = (i + 1) / N, k = 1 + (grow - 1) * (u0 + u1) / 2;
      let x0 = hinge - side * hw * cosA * u0, x1 = hinge - side * hw * cosA * u1;
      if (side < 0) { x0 = hinge + hw * cosA * u0; x1 = hinge + hw * cosA * u1; }
      const dx = Math.min(x0, x1), dw = Math.abs(x1 - x0) + 0.35, dh = hh * k;
      const sx = side < 0 ? b.x0 + hw * u0 : b.x1 - hw * u1;
      if (cosA > 0 && img.complete) ctx.drawImage(img, sx, b.y0, hw / N, hh, dx, cy - dh / 2, dw, dh);
      else { ctx.fillStyle = "#6B4A2E"; ctx.fillRect(dx, cy - dh / 2, dw, dh); }
      ctx.fillStyle = `rgba(0,0,0,${0.4 * Math.sin(a) * (0.5 + 0.5 * (u0 + u1) / 2)})`; ctx.fillRect(dx, cy - dh / 2, dw, dh);
    }
  }
  ctx.restore();
}
function drawAgent(o, S, st) {
  const { c } = o; if (!S || (st.run < 0 && !st.still)) return;
  const P = build(S).pts; if (P.length < 2) return;
  const run = st.still ? 0.15 : st.run, L = plen(P), q = pointAt(P, L * clamp(run));
  const sz = c.p.size_cm * PXCM, dir = q.tx >= 0 ? 1 : -1, fade = st.still ? 0.8 : clamp(Math.min(run, 1 - run) * 10);
  ctx.save(); ctx.globalAlpha = fade; ctx.translate(q.x, q.y); ctx.scale(dir, 1);
  ctx.shadowColor = "rgba(0,0,0,.45)"; ctx.shadowBlur = 3;
  if (c.p.sprite === "tram") {
    const w = sz * 1.7, hgt = sz * 0.95, bob = st.still ? 0 : Math.sin(st.t * 14) * 0.25;
    ctx.translate(0, -hgt / 2 - 1 + bob);
    ctx.fillStyle = "#E8C547"; ctx.beginPath(); ctx.roundRect(-w / 2, -hgt / 2, w, hgt, sz * 0.18); ctx.fill();
    ctx.shadowBlur = 0; ctx.fillStyle = "#3B2A12"; ctx.fillRect(-w / 2, hgt * 0.18, w, hgt * 0.12);
    ctx.fillStyle = "#CFE7F5"; for (let i = 0; i < 3; i++) ctx.fillRect(-w / 2 + w * (0.1 + i * 0.29), -hgt * 0.32, w * 0.22, hgt * 0.36);
    ctx.strokeStyle = "#2A2A2A"; ctx.lineWidth = 0.6; ctx.beginPath(); ctx.moveTo(0, -hgt / 2); ctx.lineTo(sz * 0.3, -hgt / 2 - sz * 0.5); ctx.stroke();
    ctx.fillStyle = "#222"; for (const x of [-w * 0.3, w * 0.3]) { ctx.beginPath(); ctx.arc(x, hgt / 2, sz * 0.12, 0, 7); ctx.fill(); }
  } else {
    const leg = st.still ? 0 : Math.sin(st.t * 12), hgt = sz;
    ctx.translate(0, -hgt * 0.62); ctx.fillStyle = "#4A3426"; ctx.strokeStyle = "#4A3426"; ctx.lineCap = "round";
    ctx.beginPath(); ctx.ellipse(0, 0, sz * 0.62, sz * 0.26, 0, 0, 7); ctx.fill();
    ctx.lineWidth = sz * 0.16; ctx.beginPath(); ctx.moveTo(sz * 0.5, -sz * 0.1); ctx.lineTo(sz * 0.78, -sz * 0.48); ctx.stroke();
    ctx.beginPath(); ctx.ellipse(sz * 0.88, -sz * 0.5, sz * 0.18, sz * 0.1, 0.5, 0, 7); ctx.fill();
    ctx.lineWidth = sz * 0.09;
    for (const [x, ph] of [[-0.42, 1], [-0.3, -1], [0.34, -1], [0.46, 1]]) { ctx.beginPath(); ctx.moveTo(sz * x, sz * 0.15); ctx.lineTo(sz * x + leg * ph * sz * 0.12, sz * 0.62); ctx.stroke(); }
    ctx.beginPath(); ctx.moveTo(-sz * 0.6, -sz * 0.05); ctx.quadraticCurveTo(-sz * 0.85, sz * 0.1, -sz * 0.78, sz * 0.4); ctx.stroke();
  }
  ctx.restore();
}
function drawParticles(o, S, st) {
  const { c } = o; if (st.run < 0 && !st.still) return;
  const run = st.still ? 0.35 : st.run, n = Math.round(c.p.count);
  const b = S ? bboxPts(build(S).pts.length ? build(S).pts : S.knots.map(q => [q.x, q.y])) : { x0: 0, x1: IW, y0: 14, y1: 90 };
  ctx.save();
  for (let i = 0; i < n; i++) {
    const sd = Math.sin(i * 12.9898) * 43758.5453, r1 = sd - Math.floor(sd), r2 = (r1 * 7.13) % 1;
    if (c.p.sprite === "leaves") {
      const y = b.y0 + (b.y1 - b.y0 + 60) * clamp(run * 1.2 - r1 * 0.2), x = b.x0 + (b.x1 - b.x0) * r2 + Math.sin(st.t * 2 + i) * 6;
      ctx.save(); ctx.translate(x, y); ctx.rotate(st.t * 2 + i); ctx.globalAlpha = st.still ? 0.7 : clamp(Math.min(run, 1 - run) * 6); ctx.fillStyle = ["#C9772E", "#E0A43A", "#9C4A1E"][i % 3];
      ctx.beginPath(); ctx.ellipse(0, 0, 2.6, 1.3, 0, 0, 7); ctx.fill(); ctx.restore();
    } else {
      const x = -30 + (IW + 60) * run + (r1 - 0.5) * 40 - i * 4, y = b.y0 + (b.y1 - b.y0) * r2 + Math.sin(st.t * 1.6 + i) * 3;
      const sz = 3 + r1 * 2.5, flap = Math.sin(st.t * 11 + i * 1.7);
      ctx.strokeStyle = "rgba(18,18,24,.85)"; ctx.lineWidth = 0.9; ctx.lineCap = "round";
      ctx.beginPath(); ctx.moveTo(x - sz, y - sz * 0.55 * flap); ctx.quadraticCurveTo(x - sz * 0.4, y - sz * 0.2, x, y); ctx.quadraticCurveTo(x + sz * 0.4, y - sz * 0.2, x + sz, y - sz * 0.55 * flap); ctx.stroke();
    }
  }
  ctx.restore();
}
function drawGuest(o, st) {
  const { c } = o; if (st.run < 0) return;
  const W = cv.width, H = cv.height, dpr = dprNow, r = 30 * dpr, tot = runLen(c, null), tt = st.run * tot;
  const inn = ease(tt / 0.7), out = ease((tt - 0.7 - c.p.stay_s) / 0.7), k = inn - out;
  const right = c.p.edge !== "left", x = right ? W + r * 1.2 - k * r * 2.4 : -r * 1.2 + k * r * 2.4, y = H * 0.62;
  ctx.save(); ctx.setTransform(1, 0, 0, 1, 0, 0); ctx.translate(x, y); if (!right) ctx.scale(-1, 1); ctx.rotate(-0.25 * (1 - k));
  ctx.shadowColor = "rgba(0,0,0,.4)"; ctx.shadowBlur = 8 * dpr;
  ctx.fillStyle = "#2F6FB3"; ctx.beginPath(); ctx.roundRect(-r, -r, r * 2, r * 2, r * 0.45); ctx.fill(); ctx.shadowBlur = 0;
  ctx.strokeStyle = "rgba(255,255,255,.55)"; ctx.lineWidth = 2 * dpr; ctx.beginPath(); ctx.moveTo(-r, 0); ctx.lineTo(r, 0); ctx.moveTo(0, -r); ctx.lineTo(0, r); ctx.stroke();
  ctx.fillStyle = "#fff"; for (const ex of [-0.38, 0.12]) { ctx.beginPath(); ctx.arc(r * ex, -r * 0.28, r * 0.17, 0, 7); ctx.fill(); }
  ctx.fillStyle = "#14243A"; for (const ex of [-0.42, 0.08]) { ctx.beginPath(); ctx.arc(r * ex, -r * 0.26, r * 0.08, 0, 7); ctx.fill(); }
  ctx.strokeStyle = "#14243A"; ctx.lineWidth = 2 * dpr; ctx.beginPath(); ctx.arc(-r * 0.14, r * 0.12, r * 0.22, 0.2, Math.PI - 0.2); ctx.stroke();
  const wave = Math.sin(tt * 9) * 0.5;
  ctx.strokeStyle = "#2F6FB3"; ctx.lineWidth = 6 * dpr; ctx.lineCap = "round"; ctx.beginPath(); ctx.moveTo(-r * 0.95, r * 0.1); ctx.lineTo(-r * 1.45, -r * 0.5 + wave * r * 0.3); ctx.stroke();
  ctx.restore();
}
function anchorBox(o) {
  if (o.c.kind === "guest") return null;
  const S = o.p ? shapeByKey(o.p, o.c.anchor) : null;
  if (S) return bboxPts(S.knots.map(q => [q.x, q.y]));
  return o.c.kind === "particles" ? { x0: 4, x1: IW - 4, y0: 14, y1: 90 } : null;
}
function drawLife(env, now, s, errX, errY) {
  const items = allContent(), st = new Map(items.map(o => [o, lifeState(o, now)]));
  const S = o => o.p ? shapeByKey(o.p, o.c.anchor) : null;
  ctx.save(); ctx.translate(errX, errY);
  for (const o of items) if (o.c.kind === "repaint") drawRepaint(o, S(o), st.get(o), s);
  ctx.restore(); ctx.save(); ctx.translate(errX, errY);
  for (const o of items) if (o.c.kind === "flat") drawFlat(o, S(o), st.get(o));
  ctx.restore();
  if (D.life.settings.show_regions) drawWorld(env, now, s, errX, errY);
  ctx.save(); ctx.translate(errX, errY);
  for (const o of items) if (o.c.kind === "hinged") drawHinged(o, S(o), st.get(o));
  for (const o of items) if (o.c.kind === "agent") drawAgent(o, S(o), st.get(o));
  for (const o of items) if (o.c.kind === "particles") drawParticles(o, S(o), st.get(o));
  ctx.restore();
  if (!playing) { // scene view: each item's anchor, dashed, with its kind code
    items.forEach(o => {
      const b = anchorBox(o); const sel = selLife() === o.c;
      if (!b) return;
      ctx.save(); ctx.setLineDash([4 / scale, 3 / scale]); ctx.strokeStyle = sel ? "#F2C14E" : "rgba(160,200,255,.85)"; ctx.lineWidth = (sel ? 2 : 1.2) / scale;
      const Sh = S(o); if (Sh && Sh.type !== "points") ctx.stroke(toPath(build(Sh).pts, build(Sh).closed)); else ctx.strokeRect(b.x0, b.y0, b.x1 - b.x0, b.y1 - b.y0);
      ctx.restore();
      drawLabel(env, `${KINDS[o.c.kind].code} ${o.c.name}`, b.x0, b.y0 - 10 / scale, 1, sel ? "rgba(120,90,20,.92)" : "rgba(30,50,80,.9)");
    });
    const g = items.find(o => o.c.kind === "guest");
    if (g) drawLabel(env, `K6 ${g.c.name} · screen edge ▸`, g.c.p.edge !== "left" ? IW - 4 / scale : 4 / scale, IH * 0.62, 1, selLife() === g.c ? "rgba(120,90,20,.92)" : "rgba(30,50,80,.9)", g.c.p.edge !== "left"),
    guestLabelAt = { x: g.c.p.edge !== "left" ? IW - 170 / scale : 0, x1: g.c.p.edge !== "left" ? IW : 170 / scale, y: IH * 0.62, o: g };
  }
  for (const o of items) if (o.c.kind === "guest") drawGuest(o, st.get(o));
  hudSet(playing ? (focus !== null ? "PLAY · SELECTED " + D.pois[focus].name.toUpperCase() : "PLAY · WALL LIFE RUNS ON ITS TRIGGERS · TAP A POI") : "SCENE · WALL LIFE ANCHORS · CLICK ONE TO EDIT IT");
}

