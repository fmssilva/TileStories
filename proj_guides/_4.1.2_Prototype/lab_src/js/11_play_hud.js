/* ---------- play mode, screens, hud ---------- */
const $ = id => document.getElementById(id);
function resetRuns() { lifeRun = {}; lastAmbientEnd = -1e9; lastGuest = -1e9; unfocusT0 = -1e9; }
function togglePlay() {
  if (drawing) finishDrawing(false);
  playing = !playing; focus = null; closeSheet(); poiStart = {}; resetRuns();
  if (playing) playT0 = performance.now() / 1000;
  $("play").setAttribute("aria-pressed", String(playing)); renderSide();
  syncTools(); updateHud();
}
$("play").addEventListener("click", togglePlay);
$("replay").addEventListener("click", () => { const now = performance.now() / 1000; if (ui.screen === "style") { styleT0 = now; return; } if (!playing) { togglePlay(); return; } playT0 = now; poiStart = {}; focus = null; closeSheet(); resetRuns(); });
/* the tap UI in Play Mode on the Wall and Wall Life screens: the same renderer as the App, on the device */
let tapPoi = null;
function renderTapWall(still = false) {
  if (!dev.layer || ui.screen === "app") return;
  dev.layer.replaceChildren(); dev.glue.replaceChildren();
  if (tapPoi === null || !playing || ui.screen === "style" || ui.screen === "precision") return;
  const p = D.pois[tapPoi];
  if (D.app.tap.offscreen === "close" && !onScreen(camProject(p.pos.x, p.pos.y)) && tapState.view?.p !== p) { closeSheet(); return; }
  paintTap(dev.layer, p, tapLayoutOf(p), { still, rerender: () => renderTapWall(), id: "tapwall" });
}
function openSheet(p) { tapPoi = D.pois.indexOf(p); tapState.view = null; ui.tapAnchor = null; renderTapWall(); }
function closeSheet() { tapPoi = null; tapState.view = null; renderTapWall(); }
let hudText = "";
function hudSet(t) { if (t !== hudText) { hudText = t; $("hud").textContent = t; } }
function updateHud() {
  if (ui.screen !== "wall") return;
  hudSet(playing ? (focus !== null ? "PLAY · SELECTED " + D.pois[focus].name.toUpperCase() : "PLAY · TAP A POI · ERROR " + D.test.error_cm + " CM")
    : ui.tab !== "specific" ? "SCENE · CLICK A SHAPE TO OPEN ITS STYLE"
    : drawing ? "SCENE · DRAWING " + D.pois[ui.poi].world.shapes[ui.shape].type.toUpperCase() + (D.pois[ui.poi].world.shapes[ui.shape].type === "polygon" ? " · CLICK THE RINGED KNOT OR ENTER TO CLOSE" : " · ENTER TO FINISH")
    : ui.tool === "rect" ? "SCENE · DRAG A BOX" : ui.tool !== "edit" ? "SCENE · CLICK TO PLACE THE FIRST KNOT" : "SCENE · EDITING " + D.pois[ui.poi].name.toUpperCase());
}
/* the style demo frames the shapes that use the selected style (or its sample), filling the screen */
function styleCam() {
  const st = D.styles[ui.style], l = st && styleSample(st); if (!l || !l.length) return;
  const ctr = x => { const q = bboxPts(x.S.knots.map(k => [k.x, k.y])); return Math.abs((q.x0 + q.x1) / 2 / IW - 0.5) + Math.abs((q.y0 + q.y1) / 2 / IH - 0.5); }, p0 = l.slice().sort((A, B) => ctr(A) - ctr(B))[0].p, b = bboxPts(l.filter(x => x.p === p0).flatMap(x => x.S.knots.map(q => [q.x, q.y]))), pad = 10 * PXCM;   // one POI at a time (never two half-framed), with room for brackets and glow
  const base = camSize({ ...ui.cam, z: 1, r: 0, yaw: 0, pitch: 0 });
  const z = clamp(Math.min(0.78 * dev.SW / ((b.x1 - b.x0 + 2 * pad) / IW * base.imgW), 0.72 * dev.SH / ((b.y1 - b.y0 + 2 * pad) / IH * base.imgH)), 1, 3.2);
  camTo(camCover({ ...ui.cam, cx: (b.x0 + b.x1) / 2 / IW, cy: (b.y0 + b.y1) / 2 / IH, z, r: 0, yaw: 0, pitch: 0 }), 300);
}
function setScreen(s) {
  if (ui.screen === s) return;
  if (drawing) finishDrawing(false);
  ui.screen = s; hover = null; rectStart = null;
  if (s === "style") styleT0 = performance.now() / 1000;
  syncScreens(); syncTools(); updateHud();
  if (s === "precision") camTo({ cx: 0.5, cy: 0.5, z: Math.min(1, PHR / IMA * dev.base / dev.SH), r: 0, yaw: 0, pitch: 0 }, 300);   // the four panels, whole
  if (s === "style") styleCam();
  renderSide();
  if (s === "app") renderPhone(); else { dev.layer?.replaceChildren(); dev.glue?.replaceChildren(); renderTapWall(); }
}
function syncScreens() {
  document.querySelectorAll("#stabs button").forEach(b => b.setAttribute("aria-selected", String(b.dataset.screen === ui.screen)));
  $("replay").title = ui.screen === "style" ? "Replay the style loop" : "Replay the reveal";
}
document.querySelectorAll("#stabs button").forEach(b => b.addEventListener("click", () => setScreen(b.dataset.screen)));
function setTool(t) { if (drawing && t !== ui.tool) finishDrawing(false); ui.tool = t; syncTools(); updateHud(); renderEditor(); }
function syncTools() { $("tools").hidden = playing || ui.tab !== "specific" || ui.screen !== "wall"; $("tools").querySelectorAll("button").forEach(b => b.setAttribute("aria-pressed", String(b.dataset.tool === ui.tool))); $("keys").hidden = !ui.keys || ui.screen !== "wall"; }
$("tools").querySelectorAll("button").forEach(b => b.addEventListener("click", () => setTool(b.dataset.tool)));
$("keysbtn").onclick = () => { ui.keys = !ui.keys; $("keysbtn").setAttribute("aria-pressed", String(ui.keys)); if (ui.keys) setScreen("wall"); syncTools(); };
function syncUndo() { for (const id of ["undo", "eundo"]) $(id).disabled = !past.length; for (const id of ["redo", "eredo"]) $(id).disabled = !future.length; }
$("undo").onclick = $("eundo").onclick = undo; $("redo").onclick = $("eredo").onclick = redo;

