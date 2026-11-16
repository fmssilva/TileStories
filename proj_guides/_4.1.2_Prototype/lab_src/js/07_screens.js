/* ---------- screens ---------- */
function draw() {
  const now = performance.now() / 1000, dpr = dprNow, s = scale * dpr;
  const env = { ctx, sc: scale, dpr };
  ctx.setTransform(1, 0, 0, 1, 0, 0); ctx.filter = "none"; ctx.globalAlpha = 1; ctx.globalCompositeOperation = "source-over";
  ctx.fillStyle = "#0A0D13"; ctx.fillRect(0, 0, cv.width, cv.height);
  ctx.setTransform(s, 0, 0, s, 0, 0);
  const ready = img.complete && img.naturalWidth;
  if (ui.screen === "app") { if (ready) ctx.drawImage(img, 0, 0, IW, IH); hudSet("APP · " + ui.appStep.toUpperCase() + (ui.appStep === "tapped" && ui.tapInfo ? " · " + ui.tapInfo.toUpperCase() : " · " + (D.looks[activeLook()].name.toUpperCase() + " · " + appearance().toUpperCase() + " · " + timeRow().name.toUpperCase()))); return; }
  if (ui.screen === "precision") { drawPrecision(env, now, s, ready); return; }
  if (ready) { ctx.imageSmoothingQuality = "high"; ctx.drawImage(img, 0, 0, IW, IH); }
  const errX = playing ? D.test.error_cm * PXCM * 0.8 : 0, errY = playing ? D.test.error_cm * PXCM * 0.6 : 0;
  if (ui.screen === "style") { drawStyleScreen(env, now, s); return; }
  if (ui.screen === "life") { drawLife(env, now, s, errX, errY); return; }
  drawWorld(env, now, s, errX, errY);
  if (!playing && ui.tab === "specific") drawHandles();
  if (!playing && rectStart && cursor) { ctx.save(); ctx.setLineDash([5 / scale, 4 / scale]); ctx.strokeStyle = "#fff"; ctx.lineWidth = 1.5 / scale; ctx.strokeRect(rectStart.x, rectStart.y, cursor.x - rectStart.x, cursor.y - rectStart.y); ctx.restore(); }
}

/* Style screen: only the shapes that use the selected style, looping intro → idle → focus → back */
function styleCycle(st, now) {
  const dur = D.test.reduce_motion ? 0.3 : st.motion.intro.duration_s, fd = Math.max(0.1, st.motion.focus.duration_s);
  const cyc = dur + 3.5 + fd + 1.4 + 0.6 + 0.6, ph = ui.styleHold != null ? ui.styleHold : (now - styleT0) % cyc;   // styleHold: tests freeze a phase
  let f = 0, phase = "intro";
  if (ph > dur) phase = "idle";
  if (ph > dur + 3.5) { phase = "focus"; f = D.test.reduce_motion ? 1 : ease((ph - dur - 3.5) / fd); }
  if (ph > dur + 3.5 + fd + 1.4) { phase = "back"; f = 1 - ease((ph - dur - 3.5 - fd - 1.4) / 0.6); }
  if (ph > cyc - 0.6) { phase = "pause"; f = 0; }
  return { T: phase === "pause" ? Infinity : ph, f, phase, idle: ph > dur };
}
function shapesUsing(st) { const out = []; D.pois.forEach(p => p.world.shapes.forEach(S => { if (styleFor(p, S) === st) out.push({ p, S }); })); return out; }
/* a style no shape uses yet is previewed on the first POI's outline, so every preset has a real demo on the wall */
function styleSample(st) { const l = shapesUsing(st); if (l.length) return l; const p = D.pois.find(q => /castle/i.test(q.name)) || D.pois[0], S = p.world.shapes.find(x => x.type === "polygon") || p.world.shapes[0]; return [{ p, S }]; }   // Castle hill: central on the panel, so the camera never clamps it
function drawStyleScreen(env, now, s) {
  const st = D.styles[ui.style]; if (!st) return;
  ctx.fillStyle = "rgba(6,8,12,.18)"; ctx.fillRect(0, 0, IW, IH);
  const list = styleSample(st);
  const c = styleCycle(st, now);
  if (st.motion.focus.preset === "spotlight" && c.f > 0) spotlight(s, list.map(x => x.S), st.motion.focus.amount * c.f, null, 0, 0);
  for (const { p, S } of list) drawRegion(env, p, S, st, { T: c.T, t: now - styleT0, idle: c.idle, f: c.f, alpha: 1 });
  if (st.motion.focus.preset === "lock_on" && c.f > 0) {
    const all = list.flatMap(x => x.S.knots.map(q => [q.x, q.y])), b = bboxPts(all), pad = 6 * PXCM, sc2 = 1 + 0.5 * (1 - c.f), cx = (b.x0 + b.x1) / 2, cy = (b.y0 + b.y1) / 2;
    const hw = ((b.x1 - b.x0) / 2 + pad) * sc2, hh = ((b.y1 - b.y0) / 2 + pad) * sc2, Lb = Math.min(hw, hh) * 0.35;
    ctx.save(); ctx.globalAlpha = c.f; ctx.strokeStyle = "#fff"; ctx.lineWidth = 2.2 / scale; ctx.lineCap = "round";
    for (const [sx, sy] of [[-1,-1],[1,-1],[1,1],[-1,1]]) { const x = cx + sx * hw, y = cy + sy * hh; ctx.beginPath(); ctx.moveTo(x - sx * Lb, y); ctx.lineTo(x, y); ctx.lineTo(x, y - sy * Lb); ctx.stroke(); }
    ctx.restore();
  }
  hudSet(`STYLE · ${st.label.toUpperCase()} · ${c.phase.toUpperCase()} · ${list.length} SHAPE${list.length === 1 ? "" : "S"}`);
}

/* Precision screen: the selected style on one shape, at 0, 2, 4 and 6 cm of tracking error; the true contour is dashed */
function drawPrecision(env, now, s, ready) {
  const st = D.styles[ui.style]; if (!st) return;
  const use = shapesUsing(st).find(x => x.S.type === "polygon") || shapesUsing(st)[0] || { p: D.pois[0], S: D.pois[0].world.shapes[0] };
  const g0 = build(use.S), b = bboxPts(g0.pts.length ? g0.pts : [[100, 100], [200, 200]]);
  const pw = (b.x1 - b.x0) * 1.5 + 30, ph = (b.y1 - b.y0) * 1.5 + 30, cx = (b.x0 + b.x1) / 2, cy = (b.y0 + b.y1) / 2;
  const errs = [0, 2, 4, 6], gap = 4, cw = (IW - gap) / 2, chh = (IH - gap) / 2;
  errs.forEach((e, i) => {
    const px = (i % 2) * (cw + gap), py = Math.floor(i / 2) * (chh + gap), z = Math.min(cw / pw, chh / ph);
    ctx.save(); ctx.beginPath(); ctx.rect(px, py, cw, chh); ctx.clip();
    ctx.translate(px + cw / 2, py + chh / 2); ctx.scale(z, z); ctx.translate(-cx, -cy);
    if (ready) ctx.drawImage(img, 0, 0, IW, IH);
    const e2 = { ctx, sc: scale * z, dpr: dprNow };
    const sv = scale; scale = scale * z;
    dashed(g0, "rgba(255,255,255,.75)", 1);
    ctx.save(); ctx.translate(e * PXCM * 0.8, e * PXCM * 0.6);
    drawRegion(e2, use.p, use.S, st, { T: Infinity, t: now, idle: true, f: 0, alpha: 1 });
    ctx.restore(); scale = sv;
    ctx.restore();
    const v = verdict(st, e);
    const bg = v.cls === "p-ok" ? "rgba(36,69,47,.92)" : v.cls === "p-edge" ? "rgba(90,70,30,.92)" : "rgba(90,35,35,.92)";
    drawLabel(env, `${e} cm off · ${v.word}`, px + 6 / scale, py + chh - 14 / scale, 1, bg);
  });
  const v = verdict(st);
  hudSet(`PRECISION · ${st.label.toUpperCase()} · HIDES ≈${v.t.toFixed(1)} CM · MEASURED ${D.test.error_cm} CM: ${v.word.toUpperCase()}`);
}

