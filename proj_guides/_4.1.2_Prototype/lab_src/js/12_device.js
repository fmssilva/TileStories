/* ---------- the device: every screen is seen through a phone or a tablet held in front of the wall ----------
   The camera: cx, cy = the wall point at the screen centre (fractions), z = distance (1 = the wall's height fills the
   screen's long side), r = tilt of the phone (deg), yaw = walking to the side (deg), pitch = looking from above or below (deg). */
const DEVICES = {
  phone_p:  { label: "Phone", short: "Phone", w: 390, h: 844, kind: "phone" },
  phone_l:  { label: "Phone, landscape", short: "Phone ⟲", w: 844, h: 390, kind: "phone" },
  tablet_p: { label: "Tablet", short: "Tablet", w: 820, h: 1180, kind: "tablet" },
  tablet_l: { label: "Tablet, landscape", short: "Tablet ⟲", w: 1180, h: 820, kind: "tablet" },
};
ui.device = "phone_p";
const IMA = IW / IH;
let PHR = 390 / 844;   // the screen's width / height
const CAM0 = { cx: 0.84, cy: 0.5, z: 1.1, r: 0, yaw: 0, pitch: 0 };
/* demos only: keep the wall over the whole screen (no blurred room at the edges); the visitor's own camera is never clamped */
function camCover(cam) { const { imgW, imgH } = camSize(cam), hx = dev.SW / 2 / imgW, hy = dev.SH / 2 / imgH;
  return { ...cam, cx: hx >= 0.5 ? 0.5 : Math.min(1 - hx, Math.max(hx, cam.cx)), cy: hy >= 0.5 ? 0.5 : Math.min(1 - hy, Math.max(hy, cam.cy)) }; }
const camCovers = cam => { const c = camCover(cam); return Math.abs(c.cx - cam.cx) < 1e-3 && Math.abs(c.cy - cam.cy) < 1e-3; };
ui.cam = { ...CAM0 };
const dev = { SW: 390, SH: 844, U: 3.9, P: 720, base: 844, scr: null, world: null, glue: null, layer: null, side: null, moved: false };
let camAnim = 0, justDragged = false;

const camSize = (cam = ui.cam) => { const imgH = cam.z * dev.base; return { imgW: imgH * IMA, imgH }; };
function camMatrix(cam = ui.cam) {
  const { imgW, imgH } = camSize(cam);
  return new DOMMatrix().translate(dev.SW / 2, dev.SH / 2).rotate(0, 0, -cam.r).rotate(cam.pitch || 0, 0, 0).rotate(0, cam.yaw || 0, 0).translate(-cam.cx * imgW, -cam.cy * imgH);
}
/* wall px (and a height off the wall in cm) → screen % (x of the width, y of the height) */
function camProject(px, py, cam = ui.cam, zcm = 0) {
  const k = camSize(cam).imgW / IW, v = camMatrix(cam).transformPoint(new DOMPoint(px * k, py * k, zcm * PXCM * k));
  const d = dev.P - v.z; if (d < 1) return { x: -1e4, y: -1e4, behind: true };
  const f = dev.P / d;
  return { x: (dev.SW / 2 + (v.x - dev.SW / 2) * f) / dev.SW * 100, y: (dev.SH / 2 + (v.y - dev.SH / 2) * f) / dev.SH * 100 };
}
/* a point on the screen → the wall px under it (the inverse of camProject on the wall plane) */
function unproject(clientX, clientY, cam = ui.cam) {
  const r = dev.scr.getBoundingClientRect(), sx = (clientX - r.left) * dev.SW / r.width - dev.SW / 2, sy = (clientY - r.top) * dev.SH / r.height - dev.SH / 2;
  const M = camMatrix(cam), c = M.transformPoint(new DOMPoint(0, 0, 0)), A = M.transformPoint(new DOMPoint(1, 0, 0)), B = M.transformPoint(new DOMPoint(0, 1, 0));
  const a = { x: A.x - c.x, y: A.y - c.y, z: A.z - c.z }, b = { x: B.x - c.x, y: B.y - c.y, z: B.z - c.z }, P = dev.P, ox = dev.SW / 2, oy = dev.SH / 2;
  const a11 = -sx * a.z - P * a.x, a12 = -sx * b.z - P * b.x, b1 = P * (c.x - ox) - sx * (P - c.z);
  const a21 = -sy * a.z - P * a.y, a22 = -sy * b.z - P * b.y, b2 = P * (c.y - oy) - sy * (P - c.z);
  const det = a11 * a22 - a12 * a21 || 1e-9, X = (b1 * a22 - a12 * b2) / det, Y = (a11 * b2 - b1 * a21) / det, k = camSize(cam).imgW / IW;
  return { x: X / k, y: Y / k };
}
function wallToClient(px, py) { const q = camProject(px, py), r = dev.scr.getBoundingClientRect(); return { x: r.left + q.x / 100 * r.width, y: r.top + q.y / 100 * r.height }; }
const onScreen = q => q.x >= 0 && q.x <= 100 && q.y >= 0 && q.y <= 100;
function edgePoint(q, m = 7) {   // where a ray from the centre to an off-screen point leaves the safe box
  const dx = q.x - 50, dy = (q.y - 50) / PHR, k = Math.min(Math.abs((50 - m) / (dx || 1e-6)), Math.abs((50 - m * 1.6) / PHR / (dy || 1e-6)));
  return { x: 50 + dx * k, y: 50 + dy * k * PHR };
}
/* the camera that puts wall point p at screen (sx %, sy %), with the given distance, tilt, side and height angles */
function camPoseFor(p, sx, sy, z = 1.1, r = 0, yaw = 0, pitch = 0) {
  const cam = { cx: p.x / IW, cy: p.y / IH, z, r, yaw, pitch };
  for (let it = 0; it < 8; it++) {
    const q = camProject(p.x, p.y, cam), e = 0.001, qx = camProject(p.x, p.y, { ...cam, cx: cam.cx + e }), qy = camProject(p.x, p.y, { ...cam, cy: cam.cy + e });
    const j11 = (qx.x - q.x) / e, j12 = (qy.x - q.x) / e, j21 = (qx.y - q.y) / e, j22 = (qy.y - q.y) / e, det = j11 * j22 - j12 * j21 || 1e-9;
    const fx = sx - q.x, fy = sy - q.y; cam.cx += (fx * j22 - j12 * fy) / det; cam.cy += (j11 * fy - fx * j21) / det;
    if (Math.abs(fx) + Math.abs(fy) < 0.01) break;
  }
  return cam;
}
const centreOn = p => ({ cx: p.pos.x / IW, cy: p.pos.y / IH + 0.08 / ui.cam.z, yaw: 0, pitch: 0 });

/* apply the camera to the world (the wall canvas and the glued tap parts) */
function applyCam() {
  if (!dev.world) return;
  const { imgW, imgH } = camSize();
  dev.world.style.width = imgW + "px"; dev.world.style.height = imgH + "px"; dev.world.style.transform = camMatrix().toString();
  fit();
}
function camChanged(still) {
  applyCam();
  if (ui.screen === "app") paintAR(still); else renderTapWall(still);
  syncCamUI();
}
function camTo(target, ms = 450, after) {
  cancelAnimationFrame(camAnim); const from = { ...ui.cam }, t0 = performance.now();
  const step = () => { const k = ease((performance.now() - t0) / ms); for (const key of ["cx", "cy", "z", "r", "yaw", "pitch"]) ui.cam[key] = (from[key] || 0) + ((target[key] ?? from[key] ?? 0) - (from[key] || 0)) * k;
    if (k < 1) { camChanged(true); camAnim = requestAnimationFrame(step); } else { camChanged(false); after && after(); } };
  step();
}
function selectedPoiForCam() { return ui.screen === "app" ? (ui.appStep === "tapped" ? D.pois[ui.appPoi] : null) : playing ? (tapPoi !== null ? D.pois[tapPoi] : focus !== null ? D.pois[focus] : null) : D.pois[ui.poi]; }
function frameSelected() { const p = selectedPoiForCam() || D.pois[ui.appPoi]; camTo({ ...centreOn(p), z: Math.max(ui.cam.z, 1.1), r: 0 }); }
function camReset() { const p = selectedPoiForCam(); camTo(p ? { ...centreOn(p), z: CAM0.z, r: 0 } : CAM0); }
function cameraTour() {   // a scripted walk: look left, right, closer, further, tilt, walk to each side, look from below, look away and back
  const p = selectedPoiForCam(), b = { ...ui.cam }, c = p ? centreOn(p) : { cx: b.cx, cy: b.cy };
  const frames = [{ cx: c.cx - 0.16 }, { cx: c.cx + 0.16 }, { cx: c.cx, z: b.z * 2 }, { z: b.z * 0.75 }, { z: b.z, r: 28 }, { r: -28 }, { r: 0, yaw: 40 }, { yaw: -40 }, { yaw: 0, pitch: -30 }, { pitch: 0, cy: c.cy - 0.75 }, { cy: c.cy, cx: c.cx + 0.6 }, { cx: c.cx, cy: c.cy, z: b.z, r: 0, yaw: 0, pitch: 0 }];
  let k = 0; const next = () => { if (k >= frames.length) { camChanged(false); return; } camTo({ ...ui.cam, ...frames[k++] }, 900, next); };
  next();
}

/* the device frame, on the stage for every screen; the side panel holds the device choice, the camera and (App) the steps */
function buildDevice() {
  ui.tapAnchor = null;   // the wall plane re-anchors to the new screen (tap4 after a device switch)
  const W = $("devwrap"), r = stageEl.getBoundingClientRect(), Dv = DEVICES[ui.device], bez = Dv.kind === "tablet" ? 14 : 10, land = Dv.w > Dv.h;
  const availW = Math.max(200, r.width - 24), availH = Math.max(200, r.height - 20);
  const k = Math.max(0.15, Math.min((availW - 2 * bez) / Dv.w, (availH - 2 * bez) / Dv.h));
  Object.assign(dev, { SW: Dv.w * k, SH: Dv.h * k, U: 3.9 * k * (Dv.kind === "tablet" ? 1.2 : 1), base: Math.max(Dv.w, Dv.h) * k });
  dev.P = dev.base * 0.85; PHR = dev.SW / dev.SH;
  const scr = h("div", { class: `ph premium${audCls()} ${Dv.kind}${land ? " land" : ""}`, style: `width:${dev.SW}px;height:${dev.SH}px;perspective:${dev.P}px;--U:${dev.U}px` },
    Dv.kind === "phone" ? h("div", { class: "notch" }) : h("div", { class: "cam" }), h("div", { class: "sb" }, h("span", {}, "9:41"), h("span", {}, "●●● 82%")));
  const glue = h("div", { class: "glue" }), world = h("div", { class: "world" }, cv, glue), layer = h("div", { class: "ui" });
  scr.append(h("div", { class: "room", style: `background-image:url(${SRC})` }), world, layer);
  Object.assign(dev, { scr, world, glue, layer });
  dev.side = null;
  W.replaceChildren(h("div", { class: `dev ${Dv.kind}${land ? " land" : ""}`, style: `padding:${bez}px` }, scr));
  wireNav(scr);
  scr.addEventListener("click", e => {   // a tap on the empty picture closes the tapped POI (App)
    if (ui.screen !== "app" || ui.appStep !== "tapped" || justDragged || e.target.closest(".hit,.mk,.fab,.tv,.pin,.edge,.s-card,.g,.sheet2,.kit,.scrn")) return;
    ui.appStep = "main"; tapState.view = null; renderPhone();
  });
  fit.w = 0; applyCam();
  renderSide();
}
/* the device buttons in the top bar: click a device to pick it, click it again to turn it (portrait ⇄ landscape) */
function renderSide() {
  document.querySelectorAll("#devbtns button").forEach(b => { const kind = b.dataset.dev, on = DEVICES[ui.device].kind === kind, land = ui.device.endsWith("_l");
    b.setAttribute("aria-pressed", String(on)); b.classList.toggle("land", on && land); b.title = (kind === "phone" ? "Phone" : "Tablet") + (on ? (land ? " · landscape (click: portrait)" : " · portrait (click: landscape)") : ""); });
}
document.querySelectorAll("#devbtns button").forEach(b => b.onclick = () => {
  const kind = b.dataset.dev, cur = DEVICES[ui.device];
  ui.device = cur.kind === kind ? (ui.device.endsWith("_p") ? kind + "_l" : kind + "_p") : kind + "_p";
  buildDevice(); if (ui.screen === "app") renderPhone(); else renderTapWall();
});
function syncCamUI() {
  dev.side?.querySelectorAll(".cr").forEach(lab => { const key = lab.dataset.k, inp = lab.querySelector("input"), o = lab.querySelector("output"), v = +ui.cam[key] || 0;
    if (document.activeElement !== inp) inp.value = v; o.textContent = v.toFixed(key === "z" ? 1 : 0) + (key === "z" ? "×" : "°"); });
}
/* scroll = walk closer / further · drag (Ctrl+drag on the Wall screen while editing) = look around · Alt+drag = walk around · Shift+scroll = tilt */
function wireNav(scr) {
  scr.addEventListener("contextmenu", e => e.preventDefault());
  scr.addEventListener("pointerdown", e => {
    dev.moved = false;
    const editing = ui.screen === "wall" && !playing;
    const forced = e.altKey ? "orbit" : (e.ctrlKey || e.metaKey || e.button === 1 || e.button === 2) ? "look" : null;
    if (!forced && (editing || e.target.closest(".hit,.mk,.fab,.search,.tv,.edge,.sheet2,.pin,.center,.cta,.opt,.kitp,.ds-app,.ds-sheet,.ds-dialog,.scrn"))) return;
    const mode = forced || "look", x0 = e.clientX, y0 = e.clientY, c0 = { ...ui.cam }, { imgW, imgH } = camSize(c0);
    if (forced) { e.preventDefault(); e.stopPropagation(); }
    const mv = ev => {
      const dx = ev.clientX - x0, dy = ev.clientY - y0;
      if (!dev.moved) { if (!forced && Math.hypot(dx, dy) < 5) return; dev.moved = true; try { scr.setPointerCapture(e.pointerId); } catch (_) {} }
      if (mode === "orbit") { ui.cam.yaw = clamp(c0.yaw + dx * 0.3, -70, 70); ui.cam.pitch = clamp(c0.pitch - dy * 0.3, -50, 50); }
      else { const a = c0.r * Math.PI / 180, ix = dx * Math.cos(a) - dy * Math.sin(a), iy = dx * Math.sin(a) + dy * Math.cos(a);
        ui.cam.cx = clamp(c0.cx - ix / imgW, -0.3, 1.3); ui.cam.cy = clamp(c0.cy - iy / imgH, -0.5, 1.5); }
      camChanged(true);
    };
    const up = () => { scr.removeEventListener("pointermove", mv); scr.removeEventListener("pointerup", up); scr.removeEventListener("pointercancel", up);
      if (dev.moved) { justDragged = true; setTimeout(() => justDragged = false, 0); camChanged(false); } };
    scr.addEventListener("pointermove", mv); scr.addEventListener("pointerup", up); scr.addEventListener("pointercancel", up);
  }, true);
  scr.addEventListener("click", e => { if (justDragged) { e.stopPropagation(); e.preventDefault(); } }, true);
  scr.addEventListener("wheel", e => { if (e.target.closest(".kitp,.ds-scroll")) return; e.preventDefault(); const d = e.deltaY || e.deltaX;
    if (e.shiftKey) ui.cam.r = clamp(ui.cam.r + d * 0.04, -45, 45); else ui.cam.z = clamp(ui.cam.z * Math.exp(-d * 0.0015), 0.5, 4);
    camChanged(true); clearTimeout(wireNav.t); wireNav.t = setTimeout(() => camChanged(false), 160); }, { passive: false });
}
window.addEventListener("keydown", e => {
  if (e.target.matches("input,select,textarea") || e.ctrlKey || e.metaKey || e.altKey) return;
  if (e.key === "f" || e.key === "F") { if (ui.screen === "wall" && !playing && drawing) return; frameSelected(); }
  else if (e.key === "0" || e.key === "Home") camReset();
});
